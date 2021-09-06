namespace Services.Broadcast
{
    public static class BroadcastConstants
    {
        public static readonly string HOUSEHOLD_CODE = "HH";

        public static readonly int HouseholdAudienceId = 31;   

        public static readonly int PostableMonthMarketThreshold = 200; //retrieved from nsi.usp_NSI_GetPostableMediaMonths

        public static readonly int RatingsGroupId = 2;

        public static readonly int OpenMarketSourceId = 1;

        public static string EMAIL_PROFILE_SEED = "34oqdfn201@#4-u34nssk10Q@94ihaefje34092";

        public static string CONFIG_FILE_NAME = "appsettings.json";

        public const int LogoCachingDurationInSeconds = 3600;

        public const int NeilsonTimeSlotInSeconds = 900;    // 15 minute timeslot should be moved to System_Settings

        public const int OneDayInSeconds = 86400;

        public const int OneHourInSeconds = 3600;

        public const string DATE_FORMAT_STANDARD = "yyyy-MM-dd";

        public const int DefaultDatabaseQueryChunkSize = 1000;

        public const int SpotLengthId30 = 1;  
        
      
        public class FolderNames
        {
            public const string SCX_EXPORT_DIRECTORY = "ScxFiles";
            public const string INVENTORY_UPLOAD = "InventoryUpload";
            public const string INVENTORY_UPLOAD_ERRORS = "Errors";
            public const string PROGRAM_LINEUP_REPORTS = "ProgramLineupReports";
            public const string CAMPAIGN_EXPORT_REPORTS = "CampaignExportReports";
            public const string INVENTORY_EXPORTS = "InventoryExports";
            public const string QUOTE_REPORTS = "QuoteReports";
            public const string PLAN_BUYING_SCX = "PlanBuyingScx";
        }
    }

    public static class FeatureToggles
    {
        public static string USE_TRUE_INDEPENDENT_STATIONS = "broadcast-use-true-independent-stations";
     
        public static string ENABLE_PROPRIETARY_INVENTORY_SUMMARY = "broadcast-enable-proprietary-inventory-summary";     
        public static string DISPLAY_CAMPAIGN_LINK = "broadcast-display-campaign-link";
        public static string DISPLAY_BUYING_LINK = "broadcast-display-buying-link";
        public static string ALLOW_MULTIPLE_CREATIVE_LENGTHS = "broadcast-allow-multiple-creative-lengths";
        public static string ENABLE_PRICING_EFFICIENCY_MODEL = "broadcast-enable-pricing-efficiency-model";
        public static string ENABLE_POSTING_TYPE_TOGGLE = "broadcast-enable-posting-type-toggle";
        public static string ENABLE_PLAN_MARKET_SOV_CALCULATIONS = "broadcast-enable-plan-market-sov-calculations";
        public static string VPVH_DEMO = "broadcast-vpvh-demo";
        public static string ENABLE_LOCKING_CONSOLIDATION = "broadcast-enable-locking-consolidation";
        public static string ENABLE_PIPELINE_VARIABLES = "Broadcast-Enable-Pipeline-Variables";
        public static string PRICING_MODEL_OPEN_MARKET_INVENTORY = "broadcast-pricing-model-open-market-inventory";
        public static string EMAIL_NOTIFICATIONS = "broadcast-email-notifications";
        public static string PRICING_MODEL_BARTER_INVENTORY = "broadcast-pricing-model-barter-inventory";
        public static string PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY = "broadcast-pricing-model-proprietary-o-and-o-inventory";
        public static string INTERNAL_PROGRAM_SEARCH = "broadcast-internal-program-search";
        public static string CAMPAIGNS_LOCKING = "broadcast-campaigns-locking";
        public static string ADU_FLAG = "broadcast-enabled-adu-flag";
        public static string EXTERNAL_NOTE_EXPORT = "Broadcast-EnableExternalNoteExport";
        public static string ENABLE_ISCI_MAPPING = "broadcast-enable-isci-mapping";
        public static string BUY_EXP_REP_ORG = "broadcast-buy-exp-rep-org";
        public static string CAMPAIGN_EXPORT_TOTAL_MONTHLY_COST = "broadcast-campaign-export-total-monthly-cost";
    }
}
