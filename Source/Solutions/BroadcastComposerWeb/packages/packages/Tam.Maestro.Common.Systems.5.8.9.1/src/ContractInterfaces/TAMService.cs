using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [DataContract]
    [Serializable]
    public enum TAMService
    {
        [EnumMember]
        [Description("Audience and Ratings Service")]
        AudienceAndRatingsService = 0,
        [EnumMember]
        [Description("Maestro Administration Service")]
        MaestroAdministrationService = 1,
        [EnumMember]
        [Description("Audience and Ratings Loader Service")]
        AudienceAndRatingsLoaderService = 2,
        [EnumMember]
        [Description("Affidavit Service")]
        AffidavitService = 3,
        [EnumMember]
        [Description("Proposal Service")]
        ProposalService = 4,
        [EnumMember]
        [Description("Business Objections Manager Service")]
        BusinessObjectsManagerService = 5,
        [EnumMember]
        [Description("Material Service")]
        MaterialService = 7,
        [EnumMember]
        [Description("Rate Card Service")]
        RateCardService = 8,
        [EnumMember]
        [Description("Release Service")]
        ReleaseService = 9,
        [EnumMember]
        [Description("Reporting Service")]
        ReportingService = 10,
        [EnumMember]
        [Description("Service Manager Service")]
        ServiceManagerService = 12,
        [EnumMember]
        [Description("System Topography Service")]
        SystemTopographyService = 13,
        [EnumMember]
        [Description("Inventory Service")]
        InventoryService = 14,
        [EnumMember]
        [Description("Delivery Service")]
        DeliveryEstimationService = 16,
        [EnumMember]
        [Description("Posting Service")]
        PostingService = 17,
        [EnumMember]
        [Description("CMW Traffic Service")]
        CmwTrafficService = 18,
        [EnumMember]
        [Description("Traffic Service")]
        TrafficService = 19,
        [EnumMember]
        [Description("Post Log Service")]
        PostLogService = 20,
        [EnumMember]
        [Description("FTP Service")]
        FTPService = 21,
        [EnumMember]
        [Description("Accounting Service")]
        AccountingService = 22,
        [EnumMember]
        [Description("Broadcast Service")]
        BroadcastService = 23,
        [EnumMember]
        [Description("Release Service 2")]
        ReleaseService2 = 24,
        [EnumMember]
        [Description("Singleton Test Service")]
        SingletonTestService = 25,
        [EnumMember]
        [Description("Coverage Universe Service")]
        CoverageUniverseService = 27
    };
}