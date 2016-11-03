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
            // getting the Management Group Connection that is used by the Console (as it should already be open)

            ConsoleSdkConnection.IManagementGroupSession session = FrameworkServices.GetService<ConsoleSdkConnection.IManagementGroupSession>();
            EnterpriseManagementGroup emg = session.ManagementGroup;

            // verifying if the MG Connection is closed and reconnecting if needed

            if (!emg.IsConnected)
            {
                emg.Reconnect();
            }

            // getting some types we need and which we are going to use further in our code in the actual processing

            ManagementPack incidentMp = emg.GetManagementPack(ManagementPacks.incidentLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);
            ManagementPack wiLibraryMp = emg.GetManagementPack(ManagementPacks.workItemLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);
            ManagementPack incidentSettingsMp = emg.GetManagementPack(ManagementPacks.incidentManagementLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);
            ManagementPack activityLibMp = emg.GetManagementPack(ManagementPacks.activityLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);

            ManagementPackClass incidentClass = emg.EntityTypes.GetClass(ClassTypes.incident, incidentMp);
            ManagementPackClass analystCommentClass = emg.EntityTypes.GetClass(ClassTypes.analystCommentLog, wiLibraryMp);
            ManagementPackEnumerationCriteria incidentClosedEnumCriteria = new ManagementPackEnumerationCriteria(string.Format("Name = '{0}'", EnumTypes.incidentStatusClosed));
            ManagementPackEnumeration incidentClosedStatus = emg.EntityTypes.GetEnumerations(incidentClosedEnumCriteria).FirstOrDefault();
            ManagementPackTypeProjection incidentProjection = emg.EntityTypes.GetTypeProjection(TypeProjections.incidentAdvanced, incidentSettingsMp);

            // this is needed in order to know which Activities map to which Activity Profix types
            // this is the case because each Activity can have a different type, but the Prefix used is saved in the same class-object (System.GlobalSetting.ActivitySettings)

            IDictionary<string, string> activityPrefixMapping = new Dictionary<string, string>
            {
                { ActivityTypes.dependent, ActivityPrefixes.dependent },
                { ActivityTypes.manual, ActivityPrefixes.manual },
                { ActivityTypes.parallel, ActivityPrefixes.parallel },
                { ActivityTypes.review, ActivityPrefixes.review },
                { ActivityTypes.sequential, ActivityPrefixes.sequential },
                { ActivityTypes.runbook, ActivityPrefixes.runbook }
            };

            // setting up the (string) variables which we are going to use to decide what type of WorkItem we are going to create and different options/aspects of it

            string workItemClassName = string.Empty;
            string workItemMpName = string.Empty;
            string workItemSettingClassName = string.Empty;
            string workItemSettingPrefixName = string.Empty;
            string workItemSettingMpName = string.Empty;
            string workItemTemplateName = string.Empty;
            string workItemStatusName = string.Empty;

            // if/elseif code to set the variables that define the differences between what WorkItem type we need to create based on the parameter passed by the Task

            if (parameters.Contains(TaskActions.Service))
            {
                workItemClassName = ClassTypes.service;
                workItemMpName = ManagementPacks.serviceLibrary;
                workItemSettingClassName = WorkItemSettings.service;
                workItemSettingPrefixName = WorkItemPrefixes.service;
                workItemSettingMpName = ManagementPacks.serviceManagementLibrary;
                workItemTemplateName = WorkItemTemplates.service;
                workItemStatusName = EnumTypes.serviceStatusNew;

            }
            else if (parameters.Contains(TaskActions.Change))
            {
                workItemClassName = ClassTypes.change;
                workItemMpName = ManagementPacks.changeLibrary;
                workItemSettingClassName = WorkItemSettings.change;
                workItemSettingPrefixName = WorkItemPrefixes.change;
                workItemSettingMpName = ManagementPacks.changeManagementLibrary;
                workItemTemplateName = WorkItemTemplates.change;
                workItemStatusName = EnumTypes.changeStatusNew;
            }
            else if (parameters.Contains(TaskActions.Problem))
            {
                workItemClassName = ClassTypes.problem;
                workItemMpName = ManagementPacks.problemLibrary;
                workItemSettingClassName = WorkItemSettings.problem;
                workItemSettingPrefixName = WorkItemPrefixes.problem;
                workItemSettingMpName = ManagementPacks.problemLibrary;
                workItemTemplateName = WorkItemTemplates.problem;
                workItemStatusName = EnumTypes.problemStatusActive;
            }
            else if (parameters.Contains(TaskActions.Release))
            {
                workItemClassName = ClassTypes.release;
                workItemMpName = ManagementPacks.releaseLibrary;
                workItemSettingClassName = WorkItemSettings.release;
                workItemSettingPrefixName = WorkItemPrefixes.release;
                workItemSettingMpName = ManagementPacks.releaseManagementLibrary;
                workItemTemplateName = WorkItemTemplates.release;
                workItemStatusName = EnumTypes.releaseStatusNew;
            }

            // here is the code that does the actual work
            // we wrap this around a try/catch block to handle any exception that may happen and display it in case of failure

            try
            {
                // getting the types we need based on the string variables we have filled earlier based on the WorkItem type we need to create

                ManagementPack workItemMp = emg.GetManagementPack(workItemMpName, Constants.mpKeyTocken, Constants.mpSMR2Version);
                ManagementPack mpSettings = emg.GetManagementPack(workItemSettingMpName, Constants.mpKeyTocken, Constants.mpSMR2Version);
                ManagementPack knowledgeLibraryMp = emg.GetManagementPack(ManagementPacks.knowledgeLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);

                ManagementPackClass workItemClass = emg.EntityTypes.GetClass(workItemClassName, workItemMp);
                ManagementPackClass workItemClassSetting = emg.EntityTypes.GetClass(workItemSettingClassName, mpSettings);

                EnterpriseManagementObject generalSetting = emg.EntityObjects.GetObject<EnterpriseManagementObject>(workItemClassSetting.Id, ObjectQueryOptions.Default);

                // here is the foreach loop that processes each class-object (in this case Incident) that was multi-selected in the view before executing the Task

                foreach (NavigationModelNodeBase node in nodes)
                {
                    // we need to setup an IList which contains only 1 GUID that correspons to the Incident we are currently working on (node["Id"])
                    // this is needed because we are using the IObjectProjectionReader.GetData(...) which gets an IList<Guid> as parameter in order to retreive the class-objects we want from the db

                    IList<Guid> bmeIdsList = new List<Guid>();
                    bmeIdsList.Add(new Guid(node[Constants.nodePropertyId].ToString()));

                    // we are setting up the ObjectProjectionCriteria using the "System.WorkItem.Incident.ProjectionType" Type Projection as we need to get all the Relationships of the Incident
                    // we will use ObjectRetrievalOptions.Buffered so that we don't get any data from the db which we don't need - we will only get the data one we call the IObjectProjectionReader.GetData(...) method
                    // we are getting the data reader object using GetObjectProjectionReader(...) and setting its PageSize to 1 because we only need 1 object retrieved here

                    ObjectProjectionCriteria incidentObjectProjection = new ObjectProjectionCriteria(incidentProjection);
                    ObjectQueryOptions queryOptions = new ObjectQueryOptions(ObjectPropertyRetrievalBehavior.All);
                    queryOptions.ObjectRetrievalMode = ObjectRetrievalOptions.Buffered;
                    IObjectProjectionReader<EnterpriseManagementObject> incidentReader = emg.EntityObjects.GetObjectProjectionReader<EnterpriseManagementObject>(incidentObjectProjection, queryOptions);
                    incidentReader.PageSize = 1;

                    // we are using EnterpriseManagementObjectProjection for the Incident we are getting from the db instead of EnterpriseManagementObject
                    // this is because we are getting a (Type) Projection object (class-object together with its Relationships & relationship class-objects

                    EnterpriseManagementObjectProjection incident = incidentReader.GetData(bmeIdsList).FirstOrDefault();

                    // we are doing the same for the new WorkItem class-object we are creating because we want to add Relationships (with their relationship class-objects from the Incident) here as well
                    // if we would only have created the new WorkItem class and nothing else with it (no Relationships), we could have used the CreatableEnterpriseManagementObject class (which needs to be used when a new class-object is getting created)

                    EnterpriseManagementObjectProjection workItem = new EnterpriseManagementObjectProjection(emg, workItemClass);

                    // now we need to assign some Template to the new WorkItem (if a default/standard Template exists) in order to already have some Activities created
                    // the Activities and all other Properties of the new WorkItem can be adjusted by modifying the default/standard Template for each WorkItem type

                    if (!string.IsNullOrEmpty(workItemTemplateName))
                    {
                        ManagementPackObjectTemplateCriteria templateCriteria = new ManagementPackObjectTemplateCriteria(string.Format("Name = '{0}'", workItemTemplateName));
                        ManagementPackObjectTemplate template = emg.Templates.GetObjectTemplates(templateCriteria).FirstOrDefault();

                        if(template != null)
                        {
                            // if a Template with this name exists, we apply it to the new WorkItem by calling ApplyTemplate(...) on it

                            workItem.ApplyTemplate(template);

                            // if we have a Template, we also need to process each Activity that it contains in order to set the Prefix for each Activity (based on its type)
                            // we are using the activityPrefixMapping variable we defined above in oder to map each Prefix based on each Activity class-type

                            ManagementPack activityManagementMp = emg.GetManagementPack(ManagementPacks.activityManagementLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);
                            ManagementPackRelationship workItemContainsActivityRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemContainsActivity, activityLibMp);
                            ManagementPackClass activitySettingsClass = emg.EntityTypes.GetClass(ClassTypes.activitySettings, activityManagementMp);

                            EnterpriseManagementObject activitySettings = emg.EntityObjects.GetObject<EnterpriseManagementObject>(activitySettingsClass.Id, ObjectQueryOptions.Default);

                            // for each Activity that exists in the Template we applied, we are going to get its Prefix setting and apply it to its ID in the format: "PREFIX{0}"
                            // "{0}" is the string pattern we need to set for any new WorkItem (including Activity) class-object we are creating as this will be replaced by the next ID available for the new WorkItem

                            foreach (IComposableProjection activity in workItem[workItemContainsActivityRelationshipClass.Target])
                            {
                                ManagementPackClass activityClass = activity.Object.GetClasses(BaseClassTraversalDepth.None).FirstOrDefault();
                                string prefix = activitySettings[null, activityPrefixMapping[activityClass.Name]].Value as string;
                                activity.Object[null, ActivityProperties.Id].Value = string.Format("{0}{1}", prefix, Constants.workItemPrefixPattern);
                            }
                        }
                    }

                    // we are setting the Properties for the new WorkItem class-object here from some Properties of the inital Incident (add more as needed)
                    // it is of highest importance that we also set its status to New/Active (depending on WorkItem class-type) in order for it to be properly processed by the internal workflows on creation
                    // if we don't set the the correct "creation" Status here, it will never be able to progress into a working state and will remain stuck in a "pending" state

                    ManagementPackEnumerationCriteria workItemStatusNewEnumCriteria = new ManagementPackEnumerationCriteria(string.Format("Name = '{0}'", workItemStatusName));
                    ManagementPackEnumeration workItemStatusNew = emg.EntityTypes.GetEnumerations(workItemStatusNewEnumCriteria).FirstOrDefault();

                    workItem.Object[workItemClass, WorkItemProperties.Id].Value = string.Format("{0}{1}", generalSetting[workItemClassSetting, workItemSettingPrefixName], Constants.workItemPrefixPattern);
                    workItem.Object[workItemClass, WorkItemProperties.Title].Value = string.Format("{0} ({1})", incident.Object[incidentClass, WorkItemProperties.Title].Value, incident.Object[incidentClass, WorkItemProperties.Id].Value);
                    workItem.Object[workItemClass, WorkItemProperties.Description].Value = incident.Object[incidentClass, WorkItemProperties.Description].Value;
                    workItem.Object[workItemClass, WorkItemProperties.Status].Value = workItemStatusNew.Id;

                    // we are adding the initial Incident to this new WorkItem as related WorkItem (System.WorkItemRelatesToWorkItem)

                    ManagementPackRelationship workItemToWorkItemRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemRelatesToWorkItem, wiLibraryMp);
                    workItem.Add(incident.Object, workItemToWorkItemRelationshipClass.Target);

                    // we are closing the current Incident by setting its Status to Closed and setting a closed date

                    incident.Object[incidentClass, IncidentProperties.Status].Value = incidentClosedStatus.Id;
                    incident.Object[incidentClass, IncidentProperties.ClosedDate].Value = DateTime.Now.ToUniversalTime();

                    // we create a new (analyst) comment (System.WorkItem.TroubleTicket.AnalystCommentLog) and we add it to the Incident in oder to comment the fact that it was closed tue to this WorkItem Transfomr Task

                    CreatableEnterpriseManagementObject analystComment = new CreatableEnterpriseManagementObject(emg, analystCommentClass);
                    analystComment[analystCommentClass, AnalystCommentProperties.Id].Value = Guid.NewGuid().ToString();
                    analystComment[analystCommentClass, AnalystCommentProperties.Comment].Value = string.Format(Constants.incidentClosedComment, workItemClass.Name, workItem.Object.Id.ToString());
                    analystComment[analystCommentClass, AnalystCommentProperties.EnteredBy].Value = EnterpriseManagementGroup.CurrentUserName;
                    analystComment[analystCommentClass, AnalystCommentProperties.EnteredDate].Value = DateTime.Now.ToUniversalTime();

                    ManagementPackRelationship incidentHasAnalystCommentRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemHasAnalystComment, wiLibraryMp);
                    incident.Add(analystComment, incidentHasAnalystCommentRelationshipClass.Target);

                    // we create an IList of RelationshipTypes we want to transfer from the Incident to the new WorkItem
                    // this is the place we can add any new/custom RelationshipTypes which we want to transfer
                    // just make sure that the RelationshipType can be transfered from an Incident to any other WorkItem class-type

                    IList<ManagementPackRelationship> relationshipsToAddList = new List<ManagementPackRelationship>()
                    {
                        workItemToWorkItemRelationshipClass,
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.createdByUser, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.affectedUser, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.assignedToUser, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemHasAttachment, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemAboutConfigItem, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemRelatesToConfigItem, wiLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.entityToArticle, knowledgeLibraryMp),
                        emg.EntityTypes.GetRelationshipClass(RelationshipTypes.workItemHasCommentLog, wiLibraryMp),
                    };

                    // we are getting an instance of the "System.Membership" RelationshipType as we need to handle RelationshipTypes derived from it as a special case
                    // the reason for this, is that Target class-objects of the "System.Membership" RelationshipType are bound to their Source class-objects
                    // being bound, means that Target class-objects of membership relationships cannot belong to 2 different (Source) class-objects
                    // because of this, we need to make a copy (using "CreatableEnterpriseManagementObject" to create a new object and copying the Properties) of the existing Target class-object
                    // and add that to the new WorkItem instead of adding the already existing Target class-object

                    ManagementPack systemLibraryMp = emg.GetManagementPack(ManagementPacks.systemLibrary, Constants.mpKeyTocken, Constants.mpSMR2Version);
                    ManagementPackRelationship membershipRelationshipClass = emg.EntityTypes.GetRelationshipClass(RelationshipTypes.membership, systemLibraryMp);

                    // we are going through each Target & Source Relationships of the Incident as defined in the relationshipsToAddList variable and adding them to the new WorkItem
                    // we are handling the Target RelationshipTypes which are derived from "System.Membership" as a special case as explained above
                    // notice that we are also removing these Relationships from the Incident by calling Remove()
                    // we are removing the Relationships from the Incident for performance purposes - in order to have less Relationships (less data) in the db
                    // comment the "itemProjection.Remove();" in order to keep the Relationships to the Incident as well if needed for some reason

                    foreach (ManagementPackRelationship relationship in relationshipsToAddList)
                    {
                        if (incident[relationship.Target].Any())
                        {
                            foreach (IComposableProjection itemProjection in incident[relationship.Target])
                            {
                                // create a new Target class-object (CreatableEnterpriseManagementObject) and add it to the projection as it is a member of a Membership RelationshipType (as explained above)
                                // notice that we DON'T remove such a Target class-object Relationship because it will also remove the class-object itself (because it is a Membership RelationshipType object and it cannot exist without this Relationship)
                                // we need it to exist because we are copying data from it and it needs to still exist in the db (ex. Attachments - we still need the binary data to exist in the db when we create the new Attachment object)
                                // we could of course delete it after we create the new WorkItem with its Relationships when calling "workItem.Overwrite()", but I chose not to do it

                                if (relationship.IsSubtypeOf(membershipRelationshipClass))
                                {
                                    CreatableEnterpriseManagementObject instance = new CreatableEnterpriseManagementObject(emg, itemProjection.Object.GetClasses(BaseClassTraversalDepth.None).FirstOrDefault());
                                    foreach (ManagementPackProperty property in itemProjection.Object.GetProperties())
                                    {
                                        instance[property.Id].Value = itemProjection.Object[property.Id].Value;
                                    }

                                    instance[null, Constants.entityId].Value = Guid.NewGuid().ToString();

                                    workItem.Add(instance, relationship.Target);
                                }

                                // just add the existing Target object-class as it is not a member of a Membership RelationshipType (as explained above)

                                else
                                {
                                    workItem.Add(itemProjection.Object, relationship.Target);
                                    itemProjection.Remove();
                                }
                            }
                        }

                        if(incident[relationship.Source].Any())
                        {
                            // we just create the new Relationship of the Source class-object to the new WorkItem because this is not affected by the Membership RelationshipType

                            foreach (IComposableProjection itemProjection in incident[relationship.Source])
                            {
                                workItem.Add(itemProjection.Object, relationship.Source);
                                itemProjection.Remove();
                            }
                        }
                    }

                    // this is where we actually save (write) the new data to the db, when calling "Overwrite()" - here saving the Incident we modified (set Status to Closed & deleted Relationships)
                    // before we have just created the new objects and relationships in-memory
                    // this is also the point when almost all of the code validation is being done
                    // if there are any issues really creating/editing/adding these objects/realtionships, this is where we would get the errors

                    incident.Overwrite();

                    // we are want to handle the error here of saving the new WorkItem and its Relationships because we want to re-open the Incident in case there is an issue when creating the new WorkItem

                    try
                    {
                        // this is where we actually save (write) the new data to the db, when calling "Overwrite()" - here saving the new WorkItem we created with its Relationships we added (from the Incident)

                        workItem.Overwrite();
                    }
                    catch (Exception ex)
                    {
                        // if we faild to create the new WorkItem with its Relationships, we want to revert to setting the Incident to an Active Status (we re-open the Incident)

                        ManagementPackEnumerationCriteria incidentActiveEnumCriteria = new ManagementPackEnumerationCriteria(string.Format("Name = '{0}'", EnumTypes.incidentStatusActive));
                        ManagementPackEnumeration incidentActiveStatus = emg.EntityTypes.GetEnumerations(incidentActiveEnumCriteria).FirstOrDefault();

                        incident.Object[incidentClass, IncidentProperties.Status].Value = incidentActiveStatus.Id;
                        incident.Object[incidentClass, IncidentProperties.ClosedDate].Value = null;

                        // again, after applying the new modifications in memory, we need to actually write them to the db using "Overwrite()"

                        incident.Overwrite();

                        // no need to show this because we are just passing it (throwing) to the wrapped try/catch block so it will be displayed and handled there

                        throw ex;
                    }
                }

                // if everything succeeds, we want to refresh the View (in this case, some View that shows Incidents as this is where we are calling our Task from)
                // we want to refresh the view to show the new Status of the Incient (as Closed in this case) - if the View only shows non-Closed Incidents, it will dissapear from the View

                RequestViewRefresh();
            }
            catch(Exception ex)
            {
                // we want to handle all Exceptions here so that the Console does not crash
                // we also want to show a MessageBox window with the Exception details for troubleshooting purposes

                MessageBox.Show(string.Format("Error: {0}: {1}\n\n{2}", ex.GetType().ToString(), ex.Message, ex.StackTrace));
            }
        }
    }
}
