using System.ComponentModel;
using System.Runtime.Serialization;

namespace Services.Broadcast
{
    public enum TAMResource
    {
        [EnumMember]
        [Description("Maestro")]
        MaestroConnectionString = 0,
        [EnumMember]
        [Description("External Ratings")]
        ExternalRatingsConnectionString = 1,
        [EnumMember]
        [Description("Post Logs")]
        PostLogConnectionString = 2,
        [EnumMember]
        [Description("Rentrak")]
        RentrakConnectionString = 3,
        [EnumMember]
        [Description("Programs")]
        ProgramsConnectionString = 4,
        [EnumMember]
        [Description("Maestro Analysis")]
        MaestroAnalysisConnectionString = 5,
        [EnumMember]
        [Description("CableTrack")]
        CableTrackConnectionString = 6,
        [EnumMember]
        [Description("Post Log Analysis")]
        PostLogAnalysisConnectionString = 7,
        [EnumMember]
        [Description("Inventory")]
        InventoryConnectionString = 8,
        [EnumMember]
        [Description("Nielsen Cable")]
        NielsenCableConnectionString = 9,
        [EnumMember]
        [Description("Broadcast")]
        BroadcastConnectionString = 10,
        [EnumMember]
        [Description("Broadcast Forecast")]
        BroadcastForecastConnectionString = 11,
        [EnumMember]
        [Description("System Settings")]
        SystemSettingsConnectionString = 12,
        [EnumMember]
        [Description("Sales Api")]
        SalesApiConnectionString = 13,
        [EnumMember]
        [Description("NSI Staging")]
        NsiStagingConnectionString = 14
    };
}