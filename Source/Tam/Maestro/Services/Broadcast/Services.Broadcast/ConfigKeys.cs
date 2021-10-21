namespace Services.Broadcast
{
    public static class ConfigKeys
    {
        public static readonly string AABCacheExpirationSeconds = "AABCacheExpirationSeconds";
		public static readonly string BroadcastDayStart = "BroadcastDayStart";
		public static readonly string BroadcastMatchingBuffer = "BroadcastMatchingBuffer";
        public static readonly string DefaultMarketCoverage = "DefaultMarketCoverage";
		public static readonly string DefaultNtiConversionFactor = "DefaultNtiConversionFactor";
		public static readonly string EfficiencyModelCpmGoal = "EfficiencyModelCpmGoal";
		public static readonly string EmailHost = "EmailHost";
		public static readonly string EmailNotificationsEnabled = "EmailNotificationsEnabled";
		public static readonly string InventoryProgramsEngineSaveBatchSize = "InventoryProgramsEngineSaveBatchSize";
		public static readonly string InventorySummaryCacheAbsoluteExpirationSeconds = "InventorySummaryCacheAbsoluteExpirationSeconds";
		public static readonly string MediaMonthCrunchCacheSlidingExpirationSeconds = "MediaMonthCrunchCacheSlidingExpirationSeconds";
		public static readonly string NumberOfFallbackQuartersForPricing = "NumberOfFallbackQuartersForPricing";
		public static readonly string EmailPassword = "EmailPassword";
		public static readonly string EmailUsername = "EmailUsername";
		public static readonly string EnableBarterInventoryForPricingModel = "EnableBarterInventoryForPricingModel";
		public static readonly string EnableCampaignsLocking = "EnableCampaignsLocking";
		public static readonly string EnableOpenMarketInventoryForPricingModel = "EnableOpenMarketInventoryForPricingModel";
		public static readonly string EnableProprietaryOAndOInventoryForPricingModel = "EnableProprietaryOAndOInventoryForPricingModel";
		public static readonly string FloorModelCpmGoal = "FloorModelCpmGoal";
		public static readonly string InventoryProcessingGroupMonitorPauseSeconds = "InventoryProcessingGroupMonitorPauseSeconds";
		public static readonly string InventoryProcessingGroupTimeoutHours = "InventoryProcessingGroupTimeoutHours";
		public static readonly string PlanPricingFloorPricingUrl = "PlanPricingFloorPricingUrl";
		public static readonly string InventoryRatingsParallelJobs = "InventoryRatingsParallelJobs";
		public static readonly string PricingRequestLogAccessKeyId = "PricingRequestLogAccessKeyId";
		public static readonly string PricingRequestLogBucket = "PricingRequestLogBucket";
		public static readonly string PricingRequestLogBucketRegion = "PricingRequestLogBucketRegion";
		public static readonly string PricingRequestLogEncryptedAccessKey = "PricingRequestLogEncryptedAccessKey";
		public static readonly string RelativePathToMarketCoveragesFile = "RelativePathToMarketCoveragesFile";
		public static readonly string SCXGenerationJobIntervalSeconds = "SCXGenerationJobIntervalSeconds";
		public static readonly string SCXGenerationParallelJobs = "SCXGenerationParallelJobs";
		public static readonly string ThresholdInSecondsForProgramIntersectInPricing = "ThresholdInSecondsForProgramIntersectInPricing";
		public static readonly string UseMaestroDayPartRepo = "UseMaestroDayPartRepo";
		
		public static readonly string AgencyAdvertiserBrandApiUrl = "AgencyAdvertiserBrandApiUrl";
		
		public static readonly string BroadcastAppFolder = "BroadcastAppFolder";
		public static readonly string BroadcastNTIUploadApiUrl= "BroadcastNTIUploadApiUrl";
		public static readonly string BroadcastSharedFolder = "BroadcastSharedFolder";
        public static readonly string EmailFrom = "EmailFrom";
		public static readonly string EmailWhiteList = "EmailWhiteList";
		public static readonly string InventoryRatingsJobIntervalSeconds = "InventoryRatingsJobIntervalSeconds";
		public static readonly string PlanPricingAllocationsEfficiencyModelUrl = "PlanPricingAllocationsEfficiencyModelUrl";
		public static readonly string PlanPricingAllocationsUrl = "PlanPricingAllocationsUrl";
		public static readonly string ProgramSearchApiUrl = "ProgramSearchApiUrl";
		public static readonly string ScxGenerationFolder = "ScxGenerationFolder";
		
		public static readonly string LaunchDarklySdkKey = "LaunchDarklySdkKey";
        public static readonly string ReelIsciIngestNumberOfDays = "ReelIsciIngestNumberOfDays";
        public static readonly string BroadcastNotificationEmail = "BroadcastNotificationEmail";
        public static readonly string DaypartCacheSlidingExpirationSeconds = "DaypartCacheSlidingExpirationSeconds";
		public static readonly string TrackerServiceFtpDirectory = "TrackerService:FtpDirector";
        public static readonly string TrackerServiceFtpPassword = "TrackerService:FtpPassword";
        public static readonly string TrackerServiceFtpSaveFolder = "TrackerService:SaveFolder";
        public static readonly string TrackerServiceFtpUrl = "TrackerService:FtpUrl";
        public static readonly string TrackerServiceFtpUserName = "TrackerService:UserName";
	}
}

