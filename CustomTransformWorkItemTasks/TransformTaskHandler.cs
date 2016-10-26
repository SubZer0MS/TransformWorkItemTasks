using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.ConsoleFramework;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess;
using Microsoft.EnterpriseManagement.Configuration;
using ConsoleSdkConnection = Microsoft.EnterpriseManagement.UI.Core.Connection;

namespace CustomTransformWorkItemTasks
{
    public class TransformTaskHandler : ConsoleCommand
    {
        public override void ExecuteCommand(IList<NavigationModelNodeBase> nodes, NavigationModelNodeTask task, ICollection<string> parameters)
        {
            ConsoleSdkConnection.IManagementGroupSession session = FrameworkServices.GetService<ConsoleSdkConnection.IManagementGroupSession>();
            EnterpriseManagementGroup emg = session.ManagementGroup;

            ManagementPack incidentMp = emg.GetManagementPack(ManagementPacks.incidentLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);
            ManagementPack wiLibraryMp = emg.GetManagementPack(ManagementPacks.workItemLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);
            ManagementPack incidentSettingsMp = emg.GetManagementPack(ManagementPacks.incidentManagementLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);

            ManagementPackClass incidentClass = emg.EntityTypes.GetClass(ClassTypes.incident, incidentMp);
            ManagementPackClass analystCommentClass = emg.EntityTypes.GetClass(ClassTypes.analystCommentLog, wiLibraryMp);
            ManagementPackEnumerationCriteria incidentClosedEnumCriteria = new ManagementPackEnumerationCriteria(string.Format("Name = '{0}'", EnumTypes.incidentStatusClosed));
            ManagementPackEnumeration incidentClosedStatus = emg.EntityTypes.GetEnumerations(incidentClosedEnumCriteria).FirstOrDefault();
            ManagementPackTypeProjection incidentProjection = emg.EntityTypes.GetTypeProjection(TypeProjections.incidentAdvanced, incidentSettingsMp);

            string workItemClassName = string.Empty;
            string workItemMpName = string.Empty;
            string workItemSettingClassName = string.Empty;
            string workItemSettingPrefixName = string.Empty;
            string workItemSettingMpName = string.Empty;
            string workItemTemplateName = string.Empty;

            if (parameters.Contains(TaskActions.Service))
            {
                workItemClassName = ClassTypes.service;
                workItemMpName = ManagementPacks.serviceLibrary;
                workItemSettingClassName = WorkItemSettings.service;
                workItemSettingPrefixName = WorkItemPrefixes.service;
                workItemSettingMpName = ManagementPacks.serviceManagementLibrary;
                workItemTemplateName = WorkItemTemplates.service;

            }
            else if (parameters.Contains(TaskActions.Change))
            {
                workItemClassName = ClassTypes.change;
                workItemMpName = ManagementPacks.changeLibrary;
                workItemSettingClassName = WorkItemSettings.change;
                workItemSettingPrefixName = WorkItemPrefixes.change;
                workItemSettingMpName = ManagementPacks.changeManagementLibrary;
                workItemTemplateName = WorkItemTemplates.change;
            }
            else if (parameters.Contains(TaskActions.Problem))
            {
                workItemClassName = ClassTypes.problem;
                workItemMpName = ManagementPacks.problemLibrary;
                workItemSettingClassName = WorkItemSettings.problem;
                workItemSettingPrefixName = WorkItemPrefixes.problem;
                workItemSettingMpName = ManagementPacks.problemLibrary;
                workItemTemplateName = WorkItemTemplates.problem;
            }
            else if (parameters.Contains(TaskActions.Release))
            {
                workItemClassName = ClassTypes.release;
                workItemMpName = ManagementPacks.releaseLibrary;
                workItemSettingClassName = WorkItemSettings.release;
                workItemSettingPrefixName = WorkItemPrefixes.release;
                workItemSettingMpName = ManagementPacks.releaseManagementLibrary;
                workItemTemplateName = WorkItemTemplates.release;
            }

            try
            {
                ManagementPack workItemMp = emg.GetManagementPack(workItemMpName, Constants.mpKeyTocken, Constants.mpSMR2Version);
                ManagementPack mpSettings = emg.GetManagementPack(workItemSettingMpName, Constants.mpKeyTocken, Constants.mpSMR2Version);
                ManagementPack knowledgeLibraryMp = emg.GetManagementPack(ManagementPacks.knowledgeLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);

                ManagementPackClass workItemClass = emg.EntityTypes.GetClass(workItemClassName, workItemMp);
                ManagementPackClass workItemClassSetting = emg.EntityTypes.GetClass(workItemSettingClassName, mpSettings);
                EnterpriseManagementObject generalSetting = emg.EntityObjects.GetObject<EnterpriseManagementObject>(workItemClassSetting.Id, ObjectQueryOptions.Default);

                foreach (NavigationModelNodeBase node in nodes)
                {
                    IList<Guid> bmeIdsList = new List<Guid>();
                    bmeIdsList.Add(new Guid(node[Constants.nodePropertyId].ToString()));

                    ObjectProjectionCriteria incidentObjectProjection = new ObjectProjectionCriteria(incidentProjection);
                    ObjectQueryOptions queryOptions = new ObjectQueryOptions(ObjectPropertyRetrievalBehavior.All);
                    queryOptions.ObjectRetrievalMode = ObjectRetrievalOptions.Buffered;
                    IObjectProjectionReader<EnterpriseManagementObject> incidentReader = emg.EntityObjects.GetObjectProjectionReader<EnterpriseManagementObject>(incidentObjectProjection, queryOptions);
                    incidentReader.PageSize = 1;

                    EnterpriseManagementObjectProjection incident = incidentReader.GetData(bmeIdsList).FirstOrDefault();

                    EnterpriseManagementObjectProjection workItem = new EnterpriseManagementObjectProjection(emg, workItemClass);

                    if (!string.IsNullOrEmpty(workItemTemplateName))
                    {
                        ManagementPackObjectTemplateCriteria templateCriteria = new ManagementPackObjectTemplateCriteria(string.Format("Name = '{0}'", workItemTemplateName));
                        ManagementPackObjectTemplate template = emg.Templates.GetObjectTemplates(templateCriteria).FirstOrDefault();

                        if(template != null)
                        {
                            workItem.ApplyTemplate(template);
                        }
                    }
                    
                    workItem.Object[workItemClass, WorkItemProperties.Id].Value = generalSetting[workItemClassSetting, workItemSettingPrefixName] + Constants.workItemPrefixPattern;
                    workItem.Object[workItemClass, WorkItemProperties.Title].Value = string.Format("{0} ({1})", incident.Object[incidentClass, WorkItemProperties.Title].Value, incident.Object[incidentClass, WorkItemProperties.Id].Value);
                    workItem.Object[workItemClass, WorkItemProperties.Description].Value = incident.Object[incidentClass, WorkItemProperties.Description].Value;

                    ManagementPackRelationship workItemToWorkItemRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemRelatesToWorkItem, wiLibraryMp);
                    workItem.Add(incident.Object, workItemToWorkItemRelationshipClass.Target);

                    CreatableEnterpriseManagementObject analystComment = new CreatableEnterpriseManagementObject(emg, analystCommentClass);
                    analystComment[analystCommentClass, AnalystCommentProperties.Id].Value = Guid.NewGuid().ToString();
                    analystComment[analystCommentClass, AnalystCommentProperties.Comment].Value = string.Format(Constants.incidentClosedComment, workItemClass.Name, workItem.Object.Id.ToString());
                    analystComment[analystCommentClass, AnalystCommentProperties.EnteredBy].Value = EnterpriseManagementGroup.CurrentUserName;
                    analystComment[analystCommentClass, AnalystCommentProperties.EnteredDate].Value = DateTime.Now.ToUniversalTime();

                    incident.Object[incidentClass, IncidentProperties.Status].Value = incidentClosedStatus.Id;
                    incident.Object[incidentClass, IncidentProperties.ClosedDate].Value = DateTime.Now.ToUniversalTime();

                    ManagementPackRelationship incidentHasAnalystCommentRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemHasAnalystComment, wiLibraryMp);
                    incident.Add(analystComment, incidentHasAnalystCommentRelationshipClass.Target);

                    IList<ManagementPackRelationship> relationshipsToAddList = new List<ManagementPackRelationship>()
                    {
                        workItemToWorkItemRelationshipClass,
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemHasCommentLog, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.createdByUser, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.affectedUser, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.assignedToUser, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemHasAttachment, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemAboutConfigItem, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemRelatesToConfigItem, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.entityToArticle, knowledgeLibraryMp)
                    };

                    foreach (ManagementPackRelationship relationship in relationshipsToAddList)
                    {
                        if (incident[relationship.Target].Any())
                        {
                            foreach (IComposableProjection itemProjection in incident[relationship.Target])
                            {
                                workItem.Add(itemProjection.Object, relationship.Target);
                                itemProjection.Remove();
                            }
                        }

                        if(incident[relationship.Source].Any())
                        {
                            foreach (IComposableProjection itemProjection in incident[relationship.Source])
                            {
                                workItem.Add(itemProjection.Object, relationship.Source);
                                itemProjection.Remove();
                            }
                        }
                    }

                    incident.Overwrite();

                    try
                    {
                        workItem.Overwrite();
                    }
                    catch (Exception ex)
                    {
                        ManagementPackEnumerationCriteria incidentActiveEnumCriteria = new ManagementPackEnumerationCriteria(string.Format("Name = '{0}'", EnumTypes.incidentStatusActive));
                        ManagementPackEnumeration incidentActiveStatus = emg.EntityTypes.GetEnumerations(incidentActiveEnumCriteria).FirstOrDefault();

                        incident.Object[incidentClass, IncidentProperties.Status].Value = incidentActiveStatus.Id;
                        incident.Object[incidentClass, IncidentProperties.ClosedDate].Value = null;

                        incident.Overwrite();

                        throw ex;
                    }
                }

                RequestViewRefresh();
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}: {1}\n\n{2}", ex.GetType().ToString(), ex.Message, ex.StackTrace));
            }
        }
    }
}
