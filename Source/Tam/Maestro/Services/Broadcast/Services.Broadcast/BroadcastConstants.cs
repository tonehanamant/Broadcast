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
    }
}
