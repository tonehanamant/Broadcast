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

        public static string LOGO_FILENAME = "cadentLogo_full_scalable.svg";

        public static string LOGO_PNG_FILENAME = "logo.png";

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
            public const string POST_PROCESSING_FILE_STORE = "PostProcessingFileStore";
            public const string PRICING_RESULTS_REPORT = "PricingResultsReport";
        }
    }
}
