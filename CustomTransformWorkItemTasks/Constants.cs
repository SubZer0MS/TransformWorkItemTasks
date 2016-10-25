using System;

namespace CustomTransformWorkItemTasks
{
    class Constants
    {
        public static readonly string nodePropertyId = "$Id$";
        public static readonly string workItemPrefixPattern = "{0}";
        public static readonly string mpKeyTocken = "31bf3856ad364e35";
        public static readonly Version mpSMR2Version = new Version("7.5.3079.0");
        public static readonly string incidentClosedComment = "Incident closed via Transform Incident Task with the related {0} with ID: {1}";
    }

    class TaskActions
    {
        public static readonly string taskActionService = "Service";
        public static readonly string taskActionChange = "Change";
        public static readonly string taskActionProblem = "Problem";
        public static readonly string taskActionRelease = "Release";
    }

    class WorkItemProperties
    {
        public static readonly string workItemIdPropertyName = "Id";
        public static readonly string workItemTitlePropertyName = "Title";
        public static readonly string workItemDescriptionPropertyName = "Description";
    }

    class IncidentProperties
    {
        public static readonly string incidentPropertyStatus = "Status";
        public static readonly string incidentPropertyClosedDate = "ClosedDate";
    }

    class AnalystCommentProperties
    {
        public static readonly string analystCommentPropertyId = "Id";
        public static readonly string analystCommentPropertyComment = "Comment";
        public static readonly string analystCommentPropertyEnteredBy = "EnteredBy";
        public static readonly string analystCommentPropertyEnteredDate = "EnteredDate";
    }

    class ManagementPacks
    {
        public static readonly string workItemLibraryMPName = "System.WorkItem.Library";
        public static readonly string incidentMPName = "System.WorkItem.Incident.Library";
        public static readonly string serviceMPName = "System.WorkItem.ServiceRequest.Library";
        public static readonly string problemMPName = "System.WorkItem.Problem.Library";
        public static readonly string releaseMPName = "System.WorkItem.ReleaseRecord.Library";
        public static readonly string changeMPName = "System.WorkItem.ChangeRequest.Library";
        public static readonly string incidentSettingsMPName = "ServiceManager.IncidentManagement.Library";
        public static readonly string serviceSettingsMPName = "ServiceManager.ServiceRequest.Library";
        public static readonly string problemSettingsMPName = "System.WorkItem.Problem.Library";
        public static readonly string releaseSettingsMPName = "ServiceManager.ReleaseManagement.Library";
        public static readonly string changeSettingsMPName = "ServiceManager.ChangeManagement.Library";
    }

    class EnumTypes
    {
        public static readonly string incidentStatusClosed = "IncidentStatusEnum.Closed";
        public static readonly string incidentStatusActive = "IncidentStatusEnum.Active";
    }

    class ClassTypes
    {
        public static readonly string workItemClassName = "System.WorkItem";
        public static readonly string incidentWiClassName = "System.WorkItem.Incident";
        public static readonly string serviceWiClassName = "System.WorkItem.ServiceRequest";
        public static readonly string problemWiClassName = "System.WorkItem.Problem";
        public static readonly string releaseWiClassName = "System.WorkItem.ReleaseRecord";
        public static readonly string changeWiClassName = "System.WorkItem.ChangeRequest";
        public static readonly string analystCommentLogClassName = "System.WorkItem.TroubleTicket.AnalystCommentLog";
    }

    class RelationshipTypes
    {
        public static readonly string affectedUserRelationshipClassName = "System.WorkItemAffectedUser";
        public static readonly string createdByUserRelationshipClassName = "System.WorkItemCreatedByUser";
        public static readonly string assignedToUserRelationshipClassName = "System.WorkItemAssignedToUser";
        public static readonly string workItemHasAnalystCommentRelationshipClassName = "System.WorkItem.TroubleTicketHasAnalystComment";
        public static readonly string workItemHasUserCommentRelationshipClassName = "System.WorkItem.TroubleTicketHasUserComment";
        public static readonly string workItemRelatesToWorkItemRelationshipClassName = "System.WorkItemRelatesToWorkItem";
        public static readonly string workItemHasAttachment = "System.WorkItemHasFileAttachment";
        public static readonly string workItemAboutConfigItem = "System.WorkItemAboutConfigItem";
        public static readonly string workItemRelatesToConfigItem = "System.WorkItemRelatesToConfigItem";
        public static readonly string workItemHasCommentLog = "System.WorkItemHasCommentLog";
    }

    class WorkItemTemplates
    {
        public static readonly string incidentClassTemplate = "DefaultIncidentTemplate";
        public static readonly string serviceClassTemplate = "ServiceManager.ServiceRequest.Library.Template.DefaultServiceRequest";
        public static readonly string problemClassTemplate = "";
        public static readonly string releaseClassTemplate = "DefaultReleaseRecord";
        public static readonly string changeClassTemplate = "StandardChangeRequest";
    }

    class WorkItemSettings
    {
        public static readonly string incidentWiClassSettingName = "System.WorkItem.Incident.GeneralSetting";
        public static readonly string serviceWiClassSettingName = "System.GlobalSetting.ServiceRequestSettings";
        public static readonly string problemWiClassSettingName = "System.GlobalSetting.ProblemSettings";
        public static readonly string releaseWiClassSettingName = "System.GlobalSetting.ReleaseSettings";
        public static readonly string changeWiClassSettingName = "System.GlobalSetting.ChangeSettings";
    }

    class WorkItemPrefixes
    {
        public static readonly string incidentWiClassSettingPrefixName = "PrefixForId";
        public static readonly string serviceWiClassSettingPrefixName = "ServiceRequestPrefix";
        public static readonly string problemWiClassSettingPrefixName = "ProblemIdPrefix";
        public static readonly string releaseWiClassSettingPrefixName = "SystemWorkItemReleaseRecordIdPrefix";
        public static readonly string changeWiClassSettingPrefixName = "SystemWorkItemChangeRequestIdPrefix";
    }

    class TypeProjections
    {
        public static readonly string incidentProjectionAdvancedName = "System.WorkItem.Incident.ProjectionType";
    }
}
