namespace Services.Broadcast
{
    public static class FeatureToggles
    {
        public static string USE_TRUE_INDEPENDENT_STATIONS = "broadcast-use-true-independent-stations";
        public static string ENABLE_PROPRIETARY_INVENTORY_SUMMARY = "broadcast-enable-proprietary-inventory-summary";           
        public static string DISPLAY_BUYING_LINK = "broadcast-display-buying-link";
        public static string ENABLE_POSTING_TYPE_TOGGLE = "broadcast-enable-posting-type-toggle";        
        public static string EMAIL_NOTIFICATIONS = "broadcast-email-notifications";
        public static string PRICING_MODEL_BARTER_INVENTORY = "broadcast-pricing-model-barter-inventory";
        public static string PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY = "broadcast-pricing-model-proprietary-o-and-o-inventory";
        public static string CAMPAIGNS_LOCKING = "broadcast-campaigns-locking";              
        public static string ENABLE_SAVE_INGESTED_INVENTORY_FILE = "broadcast-enable-save-ingested-inventory-file";
        public static string ENABLE_SPOT_EXCEPTIONS = "broadcast-enable-spot-exceptions";
        public static string ENABLE_PARALLEL_PRICINGAPICLIENT_REQUESTS = "broadcast-enable-parallel-pricing-requests";
        public static string ENABLE_PARTIAL_PLAN_SAVE = "broadcast-enable-partial-plan-save";
        public static string ENABLE_WEEKLY_BREAKDOWN_ENGINE_V2 = "broadcast-weekly-breakdown-engine-v-2";
        public static string ENABLE_PRICING_REMOVE_UNMATCHED = "broadcast-enable-pricing-remove-unmatched";
        public static string ENABLE_VARIOUS_GENRE_RESTRICTION = "broadcast-enable-various-genre-restriction";
        public static string ENABLE_BUYING_NAVIGATION_PANEL_TOOLS = "broadcast-enable-buying-navigation-panel-tools";
        public static string ENABLE_UM_REEL_ROSTER = "broadcast-enable-um-reel-roster";
        /// <summary>
        /// This feature toggle is used to check the new locking microservice
        /// </summary>
        public static string ENABLE_UNIFIED_CAMPAIGN = "broadcast-enable-unified-campaign";
        public static string ENABLE_PROGRAM_LINEUP_ALLOCATION_BY_AFFILIATE = "broadcast-program-lineup-allocation-by-affiliate";
        public static string ENABLE_CENTRALIZED_PROGRAM_LIST = "broadcast-enable-centralized-program-list";
        public static string ENABLE_STATION_SECONDARY_AFFILIATIONS = "broadcast-enable-station-secondary-affiliations";
        public static string PROGRAM_GENRE_RELATION_V_2 = "program-genre-relation-v-2";

        public static string ENABLE_SPOT_EXCEPTION_NOTIFY_SYNC = "enable-spot-exceptions-notify-results-sync";
        public static string ENABLE_PROGRAM_NAME_MATCH_BY_SIMILARITY_V2 = "enable-program-name-match-by-similarity-v2";

        public static string ENABLE_ZIPPED_PRICING = "broadcast-enable-zipped-pricing";
        public static string ENABLE_INVENTORY_SERVICE_MIGRATION = "enable-inventory-service-migration";
        public static string ENABLE_OPEN_MARKET_INVENTORY_INGEST_CREATES_UNKNOWN_STATIONS = "open-market-inventory-ingest-creates-unknown-stations";

        public static string ENABLE_ADU_FOR_PLANNING_V2 = "enable-adu-for-planning-v2";
        public static string PLANNING_VPVH_SOURCE_V2 = "planning-vpvh-source-v-2";

        /// <summary>
        /// When enabled Pricing and Buying will gather Programs using the v2 methodology.
        /// </summary>
        public const string ENABLE_PRICING_BUYING_PROGRAMS_QUERY_V2 = "pricing-buying-programs-query-v-2";

    }
}
