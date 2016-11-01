using System;

namespace CustomTransformWorkItemTasks
{
    class Constants
    {
        public static readonly string entityId = "Id";
        public static readonly string nodePropertyId = "$Id$";
        public static readonly string workItemPrefixPattern = "{0}";
        public static readonly string mpKeyTocken = "31bf3856ad364e35";
        public static readonly Version mpSMR2Version = new Version("7.5.3079.0");
        public static readonly string incidentClosedComment = "Incident closed via Transform Incident Task with the related {0} with ID: {1}";
    }

    class TaskActions
    {
        public static readonly string Service = "Service";
        public static readonly string Change = "Change";
        public static readonly string Problem = "Problem";
        public static readonly string Release = "Release";
    }

    class WorkItemProperties
    {
        public static readonly string Id = "Id";
        public static readonly string Title = "Title";
        public static readonly string Description = "Description";
        public static readonly string Status = "Status";
    }

    class ActivityProperties
    {
        public static readonly string Id = "Id";
    }

    class IncidentProperties
    {
        public static readonly string Status = "Status";
        public static readonly string ClosedDate = "ClosedDate";
    }

    class AnalystCommentProperties
    {
        public static readonly string Id = "Id";
        public static readonly string Comment = "Comment";
        public static readonly string EnteredBy = "EnteredBy";
        public static readonly string EnteredDate = "EnteredDate";
    }

    class ManagementPacks
    {
        public static readonly string systemLibrary = "System.Library";
        public static readonly string workItemLibrary = "System.WorkItem.Library";
        public static readonly string incidentLibrary = "System.WorkItem.Incident.Library";
        public static readonly string serviceLibrary = "System.WorkItem.ServiceRequest.Library";
        public static readonly string problemLibrary = "System.WorkItem.Problem.Library";
        public static readonly string releaseLibrary = "System.WorkItem.ReleaseRecord.Library";
        public static readonly string changeLibrary = "System.WorkItem.ChangeRequest.Library";
        public static readonly string incidentManagementLibrary = "ServiceManager.IncidentManagement.Library";
        public static readonly string serviceManagementLibrary = "ServiceManager.ServiceRequest.Library";
        public static readonly string releaseManagementLibrary = "ServiceManager.ReleaseManagement.Library";
        public static readonly string changeManagementLibrary = "ServiceManager.ChangeManagement.Library";
        public static readonly string knowledgeLibrary = "System.Knowledge.Library";
        public static readonly string activityLibrary = "System.WorkItem.Activity.Library";
        public static readonly string activityManagementLibrary = "ServiceManager.ActivityManagement.Library";
    }

    class EnumTypes
    {
        public static readonly string incidentStatusClosed = "IncidentStatusEnum.Closed";
        public static readonly string incidentStatusActive = "IncidentStatusEnum.Active";
        public static readonly string serviceStatusNew = "ServiceRequestStatusEnum.New";
        public static readonly string changeStatusNew = "ChangeStatusEnum.New";
        public static readonly string releaseStatusNew = "ReleaseStatusEnum.New";
        public static readonly string problemStatusActive = "ProblemStatusEnum.Active";
    }

    class ClassTypes
    {
        public static readonly string workItem = "System.WorkItem";
        public static readonly string incident = "System.WorkItem.Incident";
        public static readonly string service = "System.WorkItem.ServiceRequest";
        public static readonly string problem = "System.WorkItem.Problem";
        public static readonly string release = "System.WorkItem.ReleaseRecord";
        public static readonly string change = "System.WorkItem.ChangeRequest";
        public static readonly string analystCommentLog = "System.WorkItem.TroubleTicket.AnalystCommentLog";
        public static readonly string activitySettings = "System.GlobalSetting.ActivitySettings";
    }

    class RelationshipTypes
    {
        public static readonly string affectedUser = "System.WorkItemAffectedUser";
        public static readonly string createdByUser = "System.WorkItemCreatedByUser";
        public static readonly string assignedToUser = "System.WorkItemAssignedToUser";
        public static readonly string workItemHasAnalystComment = "System.WorkItem.TroubleTicketHasAnalystComment";
        public static readonly string workItemHasUserComment = "System.WorkItem.TroubleTicketHasUserComment";
        public static readonly string workItemRelatesToWorkItem = "System.WorkItemRelatesToWorkItem";
        public static readonly string workItemHasAttachment = "System.WorkItemHasFileAttachment";
        public static readonly string workItemAboutConfigItem = "System.WorkItemAboutConfigItem";
        public static readonly string workItemRelatesToConfigItem = "System.WorkItemRelatesToConfigItem";
        public static readonly string workItemHasCommentLog = "System.WorkItemHasCommentLog";
        public static readonly string entityToArticle = "System.EntityLinksToKnowledgeDocument";
        public static readonly string workItemContainsActivity = "System.WorkItemContainsActivity";
        public static readonly string membership = "System.Membership";
    }

    class WorkItemTemplates
    {
        public static readonly string incident = "DefaultIncidentTemplate";
        public static readonly string service = "ServiceManager.ServiceRequest.Library.Template.DefaultServiceRequest";
        public static readonly string problem = "";
        public static readonly string release = "DefaultReleaseRecord";
        public static readonly string change = "StandardChangeRequest";
    }

    class WorkItemSettings
    {
        public static readonly string incident = "System.WorkItem.Incident.GeneralSetting";
        public static readonly string service = "System.GlobalSetting.ServiceRequestSettings";
        public static readonly string problem = "System.GlobalSetting.ProblemSettings";
        public static readonly string release = "System.GlobalSetting.ReleaseSettings";
        public static readonly string change = "System.GlobalSetting.ChangeSettings";
    }

    class WorkItemPrefixes
    {
        public static readonly string incident = "PrefixForId";
        public static readonly string service = "ServiceRequestPrefix";
        public static readonly string problem = "ProblemIdPrefix";
        public static readonly string release = "SystemWorkItemReleaseRecordIdPrefix";
        public static readonly string change = "SystemWorkItemChangeRequestIdPrefix";
    }

    class ActivityTypes
    {
        public static readonly string runbook = "System.WorkItem.Activity.AutomatedActivity";
        public static readonly string dependent = "System.WorkItem.Activity.DependentActivity";
        public static readonly string activity = "System.WorkItem.Activity";
        public static readonly string manual = "System.WorkItem.Activity.ManualActivity";
        public static readonly string parallel = "System.WorkItem.Activity.ParallelActivity";
        public static readonly string review = "System.WorkItem.Activity.ReviewActivity";
        public static readonly string sequential = "System.WorkItem.Activity.SequentialActivity";
    }

    class ActivityPrefixes
    {
        public static readonly string runbook = "MicrosoftSystemCenterOrchestratorRunbookAutomationActivityBaseIdPrefix";
        public static readonly string dependent = "SystemWorkItemActivityDependentActivityIdPrefix";
        public static readonly string activity = "SystemWorkItemActivityIdPrefix";
        public static readonly string manual = "SystemWorkItemActivityManualActivityIdPrefix";
        public static readonly string parallel = "SystemWorkItemActivityParallelActivityIdPrefix";
        public static readonly string review = "SystemWorkItemActivityReviewActivityIdPrefix";
        public static readonly string sequential = "SystemWorkItemActivitySequentialActivityIdPrefix";
    }

    class TypeProjections
    {
        public static readonly string incidentAdvanced = "System.WorkItem.Incident.ProjectionType";
    }
}
