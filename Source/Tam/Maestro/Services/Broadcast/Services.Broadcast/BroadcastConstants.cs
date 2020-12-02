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
        public static string ENABLE_DAYPART_WKD = "broadcast-enable-daypart-wkd";
        public static string ENABLE_PROPRIETARY_INVENTORY_SUMMARY = "broadcast-enable-proprietary-inventory-summary";
        public static string ENABLE_PRICING_IN_EDIT = "broadcast-enable-pricing-in-edit";
        public static string RUN_PRICING_AUTOMATICALLY = "broadcast-enable-run-pricing-automatically";
        public static string DISPLAY_CAMPAIGN_LINK = "broadcast-display-campaign-link";
        public static string DISPLAY_BUYING_LINK = "broadcast-display-buying-link";
        public static string ALLOW_MULTIPLE_CREATIVE_LENGTHS = "broadcast-allow-multiple-creative-lengths";
        public static string ENABLE_EXPORT_PRE_BUY = "broadcast-enable-export-pre-buy";
        public static string ENABLE_AAB_NAVIGATION = "broadcast-enable-aab-navigation";
    }
}
