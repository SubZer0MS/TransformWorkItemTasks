using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.ConsoleFramework;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess;
using Microsoft.EnterpriseManagement.Configuration;
using Conn = Microsoft.EnterpriseManagement.UI.Core.Connection;

namespace CustomTransformWorkItemTasks
{
    public class TransformTaskHandler : ConsoleCommand
    {
        public override void ExecuteCommand(IList<NavigationModelNodeBase> nodes, NavigationModelNodeTask task, ICollection<string> parameters)
        {
            Conn.IManagementGroupSession session = FrameworkServices.GetService<Conn.IManagementGroupSession>();
            EnterpriseManagementGroup emg = session.ManagementGroup;

            ManagementPack incidentMp = emg.GetManagementPack(ManagementPacks.incidentMPName, Constants.mpKeyTocken, Constants.mpSMR2Version);
            ManagementPack wiLibraryMp = emg.GetManagementPack(ManagementPacks.workItemLibraryMPName, Constants.mpKeyTocken, Constants.mpSMR2Version);
            ManagementPack incidentSettingsMp = emg.GetManagementPack(ManagementPacks.incidentSettingsMPName, Constants.mpKeyTocken, Constants.mpSMR2Version);

            ManagementPackClass incidentClass = emg.EntityTypes.GetClass(ClassTypes.incidentWiClassName, incidentMp);
            ManagementPackClass analystCommentClass = emg.EntityTypes.GetClass(ClassTypes.analystCommentLogClassName, wiLibraryMp);
            ManagementPackEnumerationCriteria incidentClosedEnumCriteria = new ManagementPackEnumerationCriteria(string.Format("Name = '{0}'", EnumTypes.incidentStatusClosed));
            ManagementPackEnumeration incidentClosedStatus = emg.EntityTypes.GetEnumerations(incidentClosedEnumCriteria).FirstOrDefault();
            ManagementPackTypeProjection incidentProjection = emg.EntityTypes.GetTypeProjection(TypeProjections.incidentProjectionAdvancedName, incidentSettingsMp);
            
            ManagementPackRelationship createdByRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.createdByUserRelationshipClassName, wiLibraryMp);
            ManagementPackRelationship affectedUserRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.affectedUserRelationshipClassName, wiLibraryMp);
            ManagementPackRelationship assignedToRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.assignedToUserRelationshipClassName, wiLibraryMp);
            ManagementPackRelationship incidentHasAnalystCommentRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.analystCommentToIncidentRelationshipClassName, wiLibraryMp);
            ManagementPackRelationship workItemToWorkItemRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemRelatesToWorkItemRelationshipClassName, wiLibraryMp);

            string workItemClassName = string.Empty;
            string workItemMpName = string.Empty;
            string workItemSettingClassName = string.Empty;
            string workItemSettingPrefixName = string.Empty;
            string workItemSettingMpName = string.Empty;
            string workItemTemplateName = string.Empty;

            if (parameters.Contains(TaskActions.taskActionService))
            {
                workItemClassName = ClassTypes.serviceWiClassName;
                workItemMpName = ManagementPacks.serviceMPName;
                workItemSettingClassName = WorkItemSettings.serviceWiClassSettingName;
                workItemSettingPrefixName = WorkItemPrefixes.serviceWiClassSettingPrefixName;
                workItemSettingMpName = ManagementPacks.serviceSettingsMPName;
                workItemTemplateName = WorkItemTemplates.serviceClassTemplate;

            }
            else if (parameters.Contains(TaskActions.taskActionChange))
            {
                workItemClassName = ClassTypes.changeWiClassName;
                workItemMpName = ManagementPacks.changeMPName;
                workItemSettingClassName = WorkItemSettings.changeWiClassSettingName;
                workItemSettingPrefixName = WorkItemPrefixes.changeWiClassSettingPrefixName;
                workItemSettingMpName = ManagementPacks.changeSettingsMPName;
                workItemTemplateName = WorkItemTemplates.changeClassTemplate;
            }
            else if (parameters.Contains(TaskActions.taskActionProblem))
            {
                workItemClassName = ClassTypes.problemWiClassName;
                workItemMpName = ManagementPacks.problemMPName;
                workItemSettingClassName = WorkItemSettings.problemWiClassSettingName;
                workItemSettingPrefixName = WorkItemPrefixes.problemWiClassSettingPrefixName;
                workItemSettingMpName = ManagementPacks.problemSettingsMPName;
                workItemTemplateName = WorkItemTemplates.problemClassTemplate;
            }
            else if (parameters.Contains(TaskActions.taskActionRelease))
            {
                workItemClassName = ClassTypes.releaseWiClassName;
                workItemMpName = ManagementPacks.releaseMPName;
                workItemSettingClassName = WorkItemSettings.releaseWiClassSettingName;
                workItemSettingPrefixName = WorkItemPrefixes.releaseWiClassSettingPrefixName;
                workItemSettingMpName = ManagementPacks.releaseSettingsMPName;
                workItemTemplateName = WorkItemTemplates.releaseClassTemplate;
            }

            try
            {
                ManagementPack workItemMp = emg.GetManagementPack(workItemMpName, Constants.mpKeyTocken, Constants.mpSMR2Version);
                ManagementPack mpSettings = emg.GetManagementPack(workItemSettingMpName, Constants.mpKeyTocken, Constants.mpSMR2Version);

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
                    
                    workItem.Object[workItemClass, WorkItemProperties.workItemIdPropertyName].Value = generalSetting[workItemClassSetting, workItemSettingPrefixName] + Constants.workItemPrefixPattern;
                    workItem.Object[workItemClass, WorkItemProperties.workItemTitlePropertyName].Value = string.Format("{0} ({1})", incident.Object[incidentClass, WorkItemProperties.workItemTitlePropertyName].Value, incident.Object[incidentClass, WorkItemProperties.workItemIdPropertyName].Value);
                    workItem.Object[workItemClass, WorkItemProperties.workItemDescriptionPropertyName].Value = incident.Object[incidentClass, WorkItemProperties.workItemDescriptionPropertyName].Value;
                    
                    if(incident[createdByRelationshipClass.Target].Any())
                    {
                        workItem.Add(incident[createdByRelationshipClass.Target].FirstOrDefault().Object, createdByRelationshipClass.Target);
                    }

                    if (incident[affectedUserRelationshipClass.Target].Any())
                    {
                        workItem.Add(incident[affectedUserRelationshipClass.Target].FirstOrDefault().Object, affectedUserRelationshipClass.Target);
                    }

                    if (incident[assignedToRelationshipClass.Target].Any())
                    {
                        workItem.Add(incident[assignedToRelationshipClass.Target].FirstOrDefault().Object, assignedToRelationshipClass.Target);
                    }

                    workItem.Add(incident.Object, workItemToWorkItemRelationshipClass.Target);

                    CreatableEnterpriseManagementObject analystComment = new CreatableEnterpriseManagementObject(emg, analystCommentClass);
                    analystComment[analystCommentClass, AnalystCommentProperties.analystCommentPropertyId].Value = Guid.NewGuid().ToString();
                    analystComment[analystCommentClass, AnalystCommentProperties.analystCommentPropertyComment].Value = string.Format(Constants.incidentClosedComment, workItemClass.Name, workItem.Object.Id.ToString());
                    analystComment[analystCommentClass, AnalystCommentProperties.analystCommentPropertyEnteredBy].Value = EnterpriseManagementGroup.CurrentUserName;
                    analystComment[analystCommentClass, AnalystCommentProperties.analystCommentPropertyEnteredDate].Value = DateTime.Now.ToUniversalTime();

                    incident.Add(analystComment, incidentHasAnalystCommentRelationshipClass.Target);

                    incident.Object[incidentClass, IncidentProperties.incidentPropertyStatus].Value = incidentClosedStatus.Id;
                    incident.Object[incidentClass, IncidentProperties.incidentPropertyClosedDate].Value = DateTime.Now.ToUniversalTime();

                    try
                    {
                        workItem.Overwrite();
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }

                    incident.Overwrite();
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
