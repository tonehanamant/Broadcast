using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Tam.Maestro.Data.Entities
{
    [DataContract]
    [Serializable]
    public enum EMsaStatusCode : byte
    {
        [EnumMember]
        [Description("")]
        Not_Sent = 0,
        [EnumMember]
        [Description("Pending")]
        Pending = 1,
        [EnumMember]
        [Description("Completed")]
        Completed = 2
    }
    [DataContract]
    [Serializable]
    public enum EPostSourceCode : byte
    {
        [EnumMember]
        [Description("Fast Track")]
        FastTrack = 1,
        [EnumMember]
        [Description("Post")]
        Post = 0,
        [EnumMember]
        [Description("MSA Post")]
        MsaPost = 2
    }
    public enum EAudienceCategoryCode : byte
    {
        [EnumMember]
        Sex = 0,
        [EnumMember]
        Income = 1,
        [EnumMember]
        Ethnicity = 2,
        [EnumMember]
        Other = 3
    }
    [Serializable]
    [DataContract]
    public enum EProposalLinkageType
    {
        [EnumMember]
        [Description("Traffic Plan")]
        TrafficPlan = 0
    }
    [DataContract]
    [Serializable]
    public enum EOperator : byte
    {
        [EnumMember]
        Multiply = 0,
        [EnumMember]
        Divide = 1,
    }

    [DataContract]
    [Serializable]
    public enum TrafficAlertTypeEnums
    {
        [EnumMember]
        CopyChange = 1,
        [EnumMember]
        CancellationPriority = 2,
        [EnumMember]
        Cancellation = 3,
        [EnumMember]
        Revision = 4,
        [EnumMember]
        AdCopyCorrection = 5
    }

    [DataContract]
    [Serializable]
    public enum ReelDispositionEnums
    {
        [EnumMember]
        CurrentlyRunning = 1,
        [EnumMember]
        ExistingCopyOnPriorReel = 2,
        [EnumMember]
        NewCopyOnThisReel = 3
    }

    [DataContract]
    [Serializable]
    public enum EmailMessageTypes
    {
        [EnumMember]
        System_Statement = 1,
        [EnumMember]
        Accounting_Invoice = 2,
        [EnumMember]
        Salesforce_Integration_Report = 3,
        [EnumMember]
        Salesforce_Integration_Error = 4,
        [EnumMember]
        Folder_Monitor = 5,
        [EnumMember]
        Ratings_Load_Failure = 6,
        [EnumMember]
        Forecast_Failure = 7
    }
    [DataContract]
    [Serializable]
    public enum EmailOutboxStatusCode
    {
        [EnumMember]
        Queued = 0,
        [EnumMember]
        Completed_Successfully = 1,
        [EnumMember]
        Partially_Completed = 2,
        [EnumMember]
        Partially_Completed_With_Errors = 3,
        [EnumMember]
        Failed = 4
    }
    [DataContract]
    [Serializable]
    public enum EmailOutboxDetailStatusCode
    {
        [EnumMember]
        Queued = 0,
        [EnumMember]
        Sent = 1,
        [EnumMember]
        Failed = 2,
        [EnumMember]
        Pending = 3
    }
    [DataContract]
    [Serializable]
    public enum EmailProfileTypes
    {
        [EnumMember]
        Auto = 3,
        [EnumMember]
        Traffic = 4,
        [EnumMember]
        Maestro = 5
    }
    [DataContract]
    [Serializable]
    public enum CMWOrderStatus
    {
        [EnumMember]
        InProgress = 28,
        [EnumMember]
        Ordered = 29,
        [EnumMember]
        Released = 27,
        [EnumMember]
        Rejected = 30,
        [EnumMember]
        Revised = 33,
        [EnumMember]
        Modified = 34,
        [EnumMember]
        PreviouslyReleased = 35,
        [EnumMember]
        Confirmed = 36
    }
    [DataContract]
    [Serializable]
    public enum CMWTStatusSets
    {
        [EnumMember]
        cmw_traffic
    }
    [DataContract]
    [Serializable]
    public enum DaypartMapSets
    {
        [EnumMember]
        Proposals,
        [EnumMember]
        NielSugRates,
        [EnumMember]
        Carve
    }
    [DataContract]
    [Serializable]
    public enum RoleType
    {
        [EnumMember]
        Employees = 1,
        [EnumMember]
        Intranet_Administrators = 2,
        [EnumMember]
        IT_Request_Approval = 3,
        [EnumMember]
        Executive = 4,
        [EnumMember]
        IT_Request_Review = 5,
        [EnumMember]
        Invoice_Manager = 6,
        [EnumMember]
        Sales = 7,
        [EnumMember]
        IT_Inventory_Management = 8,
        [EnumMember]
        Rate_Card_Composer = 9,
        [EnumMember]
        Affidavit_Composer = 10,
        [EnumMember]
        Traffic_Composer = 11,
        [EnumMember]
        Research_Composer = 12,
        [EnumMember]
        Materials_Composer = 13,
        [EnumMember]
        Proposal_Composer = 14,
        [EnumMember]
        UnOrder_Proposal_Override = 15,
        [EnumMember]
        Systems_Composer = 16,
        [EnumMember]
        Release_Composer = 17,
        [EnumMember]
        Maestro_Admin = 18,
        [EnumMember]
        Posting_Plans = 19,
        [EnumMember]
        Index_Creation = 20,
        [EnumMember]
        Carve_Plans = 21,
        [EnumMember]
        Traffic_Plans = 22,
        [EnumMember]
        Release_Plans = 23,
        [EnumMember]
        CMW_Traffic_Sales_Assistants = 24,
        [EnumMember]
        Email_Profile_Management = 25,
        [EnumMember]
        System_Statement_Management = 26,
        [EnumMember]
        Include_on_Married_Proposals_Report = 27,
        [EnumMember]
        CMW_Traffic_Management = 28,
        [EnumMember]
        BOMS_Manager = 29,
        [EnumMember]
        Traffic_Rate_Card_Viewer = 30,
        [EnumMember]
        Traffic_Rate_Card_Editor = 31,
        [EnumMember]
        Traffic_Rate_Card_Management = 32,
        [EnumMember]
        Include_on_Avail_Planner = 33,
        [EnumMember]
        Release_Composer_Regenerate_All_Systems = 34,
        [EnumMember]
        Release_Composer_Edit_All_Systems = 35,
        [EnumMember]
        Manage_Posts = 36,
        [EnumMember]
        Delete_Posts = 37,
        [EnumMember]
        Post_Log_Composer = 38,
        [EnumMember]
        Lock_Posts = 39,
        [EnumMember]
        Release_Composer_CanOverrideSpotRate = 40,
        [EnumMember]
        Materials_Composer_CanMarkReelFinal = 41,
        [EnumMember]
        Materials_Composer_CanMarkReelSent = 42,
        [EnumMember]
        Systems_Composer_Can_Modify_Weighting_Factors = 43,
        [EnumMember]
        Accounting_Composer = 44,
        [EnumMember]
        CMW_Traffic_Invoice_Management = 45,
        [EnumMember]
        Order_Proposal = 46,
        [EnumMember]
        Execute_Posts = 47,
        [EnumMember]
        Manage_Fast_Track_Gap_Projections = 49,
        [EnumMember]
        Execute_Fast_Tracks = 50,
        //[EnumMember]
        //Proposal_Composer_Salesforce_Integration = 51,
        [EnumMember]
        Broadcast_Proposer = 52,
        [EnumMember]
        Broadcast_Proposer_Create_Plans = 53,
        [EnumMember]
        Broadcast_Proposer_Approve_Plans = 54,
        [EnumMember]
        Proposal_Upfront_Report = 55,
        [EnumMember]
        Comment_Editor = 56,
        [EnumMember]
        Cable_Sales_Model_Access = 57,
        [EnumMember]
        Hispanic_Sales_Model_Access = 58,
        [EnumMember]
        NationalDr_Sales_Model_Access = 59,
        [EnumMember]
        Impressions_MediaWorks_Sales_Model_Access = 60,
        [EnumMember]
        Unapprove_Rate_Cards_and_Coverage_Universes = 61,
        [EnumMember]
        Override_Proposal_Rate_Restrictions = 62,
        [EnumMember]
        Override_Traffic_and_Release_Caps = 63,
        [EnumMember]
        Modify_Traffic_and_Release_System_Percentages = 64,
        [EnumMember]
        Create_Traffic_Plan_Linkages = 65,
        [EnumMember]
        MSA_Post_Exporter = 67,
        [EnumMember]
        Modify_Zone_Network_History = 68,
        [EnumMember]
        Modify_Network_Maps = 69,
        [EnumMember]
        View_Affidavit_Details = 70,
        [EnumMember]
        Modify_Affidavit_Details = 71,
        [EnumMember]
        Add_Network_To_All_Zones = 72,
        [EnumMember]
        View_Zone_Maps = 73,
        [EnumMember]
        Modify_Zone_Maps = 74,
        [EnumMember]
        Manage_MSA_Locks_by_Media_Month = 76,
        [EnumMember]
        Coverage_Universe_Review = 77,
        [EnumMember]
        Coverage_Universe_Review_And_Approval = 78,
        [EnumMember]
        Cable_Track_Review = 79,
        [EnumMember]
        Cable_Track_Approval = 80,
        [EnumMember]
        System_Component_Parameter_View_Only = 81,
        [EnumMember]
        System_Component_Parameter_Edit = 82,
        [EnumMember]
        Delete_Affidavits = 83,
        [EnumMember]
        Broadcast_Buyer = 84
    }
    [DataContract]
    [Serializable]
    public enum CategorySet
    {
        [EnumMember]
        Releases = 0
    }
    [DataContract]
    [Serializable]
    public enum EmployeeStatus
    {
        [EnumMember]
        Enabled = 0,
        [EnumMember]
        Disabled = 1
    }
    [DataContract]
    [Serializable]
    public enum EmailTypeEnum
    {
        [EnumMember]
        Home = 1,
        [EnumMember]
        Work = 2,
        [EnumMember]
        Assistants = 3
    }


    [DataContract]
    [Serializable]
    public enum ReleaseStatusEnum
    {
        [EnumMember]
        Unreleased = 8,
        [EnumMember]
        Released = 9//,
        //[EnumMember]
        //Processing = 23
    }




    [DataContract]
    [Serializable]
    public enum ResultStatus
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        Error = 1,
        [EnumMember]
        Validation = 2
    }



    [DataContract]
    [Serializable]
    public enum NoteType
    {
        [EnumMember]
        companies,
        [EnumMember]
        contacts,
        [EnumMember]
        dmas,
        [EnumMember]
        materials,
        [EnumMember]
        medias,
        [EnumMember]
        media_locations,
        [EnumMember]
        networks,
        [EnumMember]
        proposals,
        [EnumMember]
        products,
        [EnumMember]
        reels,
        [EnumMember]
        system_groups,
        [EnumMember]
        systems,
        [EnumMember]
        zones,
        [EnumMember]
        system_statements,
        [EnumMember]
        tam_posts
    }

    [DataContract]
    public enum CmwDivisionEnum
    {
        [EnumMember]
        CadentNetwork = 1,
        [EnumMember]
        ApexMedia
    }

    [DataContract]
    public enum InvoiceTypeEnum
    {
        [EnumMember]
        CableInvoice = 1,
        [EnumMember]
        ApexInvoice
    }

    [DataContract]
    public enum CommentTypeEnum
    {
        [EnumMember]
        Normal = 1
    }


    [DataContract]
    [Serializable]
    public enum TrafficMasterAlertStatusEnum
    {
        [EnumMember]
        Working = 40,
        [EnumMember]
        Ready = 41,
        [EnumMember]
        Sent = 42
    }

    [DataContract]
    [Serializable]
    public enum UseCaseEnums
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        CopyManagementWeb = 1,
        [EnumMember]
        CopyUpdateWeb = 2,
        [EnumMember]
        MasterAlertManagementWeb = 3,
        [EnumMember]
        TrafficCancellationWeb = 4,
    }

    [DataContract]
    [Serializable]
    public enum EventTypeEnums
    {
        [EnumMember]
        TrafficAlertCreated = 1,
        [EnumMember]
        TrafficAlertUpdated = 2,
        [EnumMember]
        TrafficAlertDeleted = 3,
        [EnumMember]
        MasterAlertCreated = 4,
        [EnumMember]
        MasterAlertUpdated = 5,
        [EnumMember]
        MasterAlertDeleted = 6,
        [EnumMember]
        MasterAlertPdfGenerated = 7,
        [EnumMember]
        TrafficCancellation = 8,
        [EnumMember]
        AffidavitDeleted = 9
    }

}
