namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingJobDiagnostic : ProcessWorkflowTimers
    {
        public static string SW_KEY_TOTAL_DURATION = "Total duration";
        public static string SW_KEY_SETTING_JOB_STATUS_TO_PROCESSING = "Setting job status to Processing";
        public static string SW_KEY_FETCHING_PLAN_AND_PARAMETERS = "Fetching plan and parameters";

        public static string SW_KEY_GATHERING_INVENTORY = "Gathering inventory";
        public static string SW_KEY_CALCULATING_FLIGHT_DATE_RANGES_AND_FLIGHT_DAYS = "Gathering inventory -> Calculating flight date ranges and flight days";

        public static string SW_KEY_FETCHING_INVENTORY_FROM_DB = "Gathering inventory -> Fetching inventory from DB";
        public static string SW_KEY_FETCHING_NOT_POPULATED_INVENTORY = "Gathering inventory -> Fetching inventory from DB -> Fetching not populated inventory";
        public static string SW_KEY_MATCHING_INVENTORY_WEEKS_WITH_PLAN_WEEKS = "Gathering inventory -> Fetching inventory from DB -> Matching inventory weeks with plan weeks";
        public static string SW_KEY_SETTING_PRIMARY_PROGRAM = "Gathering inventory -> Fetching inventory from DB -> Setting primary program";
        public static string SW_KEY_SETTING_INVENTORY_DAYPARTS = "Gathering inventory -> Fetching inventory from DB -> Setting inventory dayparts";
        public static string SW_KEY_SETTING_LATEST_STATION_DETAILS = "Gathering inventory -> Fetching inventory from DB -> Setting latest station details";

        public static string SW_KEY_FILTERING_OUT_INVENTORY_BY_DAYPARTS_AND_ASSOCIATING_WITH_STANDARD_DAYPART = "Gathering inventory -> Filtering out inventory by dayparts and associating with standard daypart";
        public static string SW_KEY_APPLYING_INFLATION_FACTOR = "Gathering inventory -> Applying inflation factor";
        public static string SW_KEY_SETTING_INVENTORY_DAYS_BASED_ON_PLAN_DAYS = "Gathering inventory -> Setting inventory days based on plan days";
        public static string SW_KEY_APPLYING_PROJECTED_IMPRESSIONS = "Gathering inventory -> Applying projected impressions";
        public static string SW_KEY_APPLYING_HOUSEHOLD_PROJECTED_IMPRESSIONS = "Gathering inventory -> Applying HouseHold projected impressions";
        public static string SW_KEY_APPLYING_PROVIDED_IMPRESSIONS = "Gathering inventory -> Applying provided impressions";
        public static string SW_KEY_APPLYING_NTI_CONVERSION_TO_NSI = "Gathering inventory -> Applying NTI conversion to NSI";
        public static string SW_KEY_FILTERING_OUT_INVENTORY_BY_MIN_AND_MAX_CPM = "Gathering inventory -> Filtering out inventory by min and max CPM";

        public static string SW_KEY_CALCULATING_PROPRIETARY_DATA = "Calculating proprietary data";
        public static string SW_KEY_APPLYING_PROPRIETARY_DATA = "Applying proprietary data";
        public static string SW_KEY_PREPARING_API_REQUEST = "Preparing API request";
        public static string SW_KEY_CALLING_API = "Calling API";
        public static string SW_KEY_MAPPING_ALLOCATED_SPOTS = "Mapping allocated spots";
        public static string SW_KEY_CALCULATING_PRICING_CPM = "Calculating pricing CPM";
        public static string SW_KEY_VALIDATING_ALLOCATION_RESULT = "Validating and mapping API response";
        public static string SW_KEY_AGGREGATING_ALLOCATION_RESULTS = "Aggregating allocation results";
        public static string SW_KEY_CALCULATING_PRICING_BANDS = "Calculating pricing bands";
        public static string SW_KEY_CALCULATING_PRICING_STATIONS = "Calculating pricing stations";
        public static string SW_KEY_AGGREGATING_MARKET_RESULTS = "Aggregating market results";

        public static string SW_KEY_SAVING_ALLOCATION_RESULTS = "Saving allocation results";
        public static string SW_KEY_SAVING_AGGREGATION_RESULTS = "Saving aggregation results";
        public static string SW_KEY_SAVING_PRICING_BANDS = "Saving pricing bands";
        public static string SW_KEY_SAVING_PRICING_STATIONS = "Saving pricing stations";
        public static string SW_KEY_SAVING_MARKET_RESULTS = "Saving market results";
        public static string SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED = "Setting job status to Succeeded";
    }
}
