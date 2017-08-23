﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tam.Maestro.Common.SystemComponentParameter
{		 
	public static class AccountingServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "AccountingService" ; }
        }
	  
		public static string ApexDefaultBillingTerms  
		{
            get { return "ApexDefaultBillingTerms"; }
        }
	}
	 
	public static class AffidavitsServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "AffidavitsService" ; }
        }
	  
		public static string EchoAirTimeBufferInSeconds  
		{
            get { return "EchoAirTimeBufferInSeconds"; }
        }
	 
		public static string EchoZoneId  
		{
            get { return "EchoZoneId"; }
        }
	 
		public static string EnableTrafficReconciliation  
		{
            get { return "EnableTrafficReconciliation"; }
        }
	 
		public static string GeneralAirTimeBufferInSeconds  
		{
            get { return "GeneralAirTimeBufferInSeconds"; }
        }
	 
		public static string MaxCheckInThreads  
		{
            get { return "MaxCheckInThreads"; }
        }
	 
		public static string MaxLoaderThreads  
		{
            get { return "MaxLoaderThreads"; }
        }
	 
		public static string MaxProcessorQueueSize  
		{
            get { return "MaxProcessorQueueSize"; }
        }
	 
		public static string MaxProcessorThreads  
		{
            get { return "MaxProcessorThreads"; }
        }
	 
		public static string SendPercentage  
		{
            get { return "SendPercentage"; }
        }
	 
		public static string TrafficReconciliationStartHour  
		{
            get { return "TrafficReconciliationStartHour"; }
        }
	}
	 
	public static class AudienceAndRatingsServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "AudienceAndRatingsService" ; }
        }
	  
		public static string EnableAutomaticForecasting  
		{
            get { return "EnableAutomaticForecasting"; }
        }
	 
		public static string EnableGraceNoteDropBox  
		{
            get { return "EnableGraceNoteDropBox"; }
        }
	 
		public static string EnableNewRatingsService  
		{
            get { return "EnableNewRatingsService"; }
        }

	    public static string NumMonthsToCacheForNewRatingsService
	    {
	        get { return "NumMonthsToCacheForNewRatingsService"; }
	    }

        public static string EnableNsiFolderMonitor  
		{
            get { return "EnableNsiFolderMonitor"; }
        }
	 
		public static string EnableProcessIncompleteRatingsLoadJobsCheck  
		{
            get { return "EnableProcessIncompleteRatingsLoadJobsCheck"; }
        }
	 
		public static string EnableRotationalBiasProcessor  
		{
            get { return "EnableRotationalBiasProcessor"; }
        }
	 
		public static string ExpertBiasReportPath  
		{
            get { return "ExpertBiasReportPath"; }
        }
	 
		public static string MaxLoaderThreads  
		{
            get { return "MaxLoaderThreads"; }
        }
	 
		public static string NoOfThreadsForRatingRequest  
		{
            get { return "NoOfThreadsForRatingRequest"; }
        }
	 
		public static string NsiDropBoxDirectory  
		{
            get { return "NsiDropBoxDirectory"; }
        }
	 
		public static string NsiNotificationEmailAddresses  
		{
            get { return "NsiNotificationEmailAddresses"; }
        }
	 
		public static string NsiOtherDirectory  
		{
            get { return "NsiOtherDirectory"; }
        }
	 
		public static string NsiRatingsDirectory  
		{
            get { return "NsiRatingsDirectory"; }
        }
	 
		public static string RatingsLoadFailureReportSubscribers  
		{
            get { return "RatingsLoadFailureReportSubscribers"; }
        }
	 
		public static string RunForecastingTime  
		{
            get { return "RunForecastingTime"; }
        }
	 
		public static string SkipViewershipLoading  
		{
            get { return "SkipViewershipLoading"; }
        }
	}
	 
	public static class BroadcastServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "BroadcastService" ; }
        }
	  
		public static string BroadcastDayStart  
		{
            get { return "BroadcastDayStart"; }
        }
	 
		public static string BroadcastMatchingBuffer  
		{
            get { return "BroadcastMatchingBuffer"; }
        }
	 
		public static string UseMaestroDaypartRepo  
		{
            get { return "UseMaestroDaypartRepo"; }
        }
	}
	 
	public static class CommonSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "Common" ; }
        }
	  
		public static string MaestroEmployeeId  
		{
            get { return "MaestroEmployeeId"; }
        }
	 
		public static string PythonFilePath  
		{
            get { return "PythonFilePath"; }
        }
	}
	 
	public static class DeliveryServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "DeliveryService" ; }
        }
	  
		public static string AutoDeliveryEstimationJobScheduleEndTime  
		{
            get { return "AutoDeliveryEstimationJobScheduleEndTime"; }
        }
	 
		public static string AutoDeliveryEstimationJobScheduleStartTime  
		{
            get { return "AutoDeliveryEstimationJobScheduleStartTime"; }
        }
	 
		public static string EnableAutoDeliveryEstimationJobScheduler  
		{
            get { return "EnableAutoDeliveryEstimationJobScheduler"; }
        }
	 
		public static string RatingSourcesToCalculateDeliveryFor  
		{
            get { return "RatingSourcesToCalculateDeliveryFor"; }
        }
	}
	 
	public static class EmailerServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "EmailerService" ; }
        }
	  
		public static string DefaultEmailMaxNumAttempts  
		{
            get { return "DefaultEmailMaxNumAttempts"; }
        }
	 
		public static string DefaultEmailMinutesBetweenAttempts  
		{
            get { return "DefaultEmailMinutesBetweenAttempts"; }
        }
	 
		public static string DefaultEmailPort  
		{
            get { return "DefaultEmailPort"; }
        }
	 
		public static string EnableSsl  
		{
            get { return "EnableSsl"; }
        }
	}
	 
	public static class InventoryServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "InventoryService" ; }
        }
	  
		public static string DailyImsKickoffDays  
		{
            get { return "DailyImsKickoffDays"; }
        }
	 
		public static string DailyImsProcessEnabled  
		{
            get { return "DailyImsProcessEnabled"; }
        }
	 
		public static string DailyImsStartHour  
		{
            get { return "DailyImsStartHour"; }
        }
	 
		public static string EnableHistoricalAvailabilityStagingProcess  
		{
            get { return "EnableHistoricalAvailabilityStagingProcess"; }
        }
	 
		public static string EnableImsNotificationEmail  
		{
            get { return "EnableImsNotificationEmail"; }
        }
	 
		public static string EnableInventoryForecastProcess  
		{
            get { return "EnableInventoryForecastProcess"; }
        }
	 
		public static string EnableInventoryUpdateJobMonitor  
		{
            get { return "EnableInventoryUpdateJobMonitor"; }
        }
	 
		public static string EnableSalesIMSForecast  
		{
            get { return "EnableSalesIMSForecast"; }
        }
	 
		public static string EnableSalesLoadForecastInCompetitionAlgorithm  
		{
            get { return "EnableSalesLoadForecastInCompetitionAlgorithm"; }
        }
	 
		public static string EnableTrafficIMSForecast  
		{
            get { return "EnableTrafficIMSForecast"; }
        }
	 
		public static string ErrorNotificationRecipients  
		{
            get { return "ErrorNotificationRecipients"; }
        }
	 
		public static string ImsNotificationCcEmailAddresses  
		{
            get { return "ImsNotificationCcEmailAddresses"; }
        }
	 
		public static string RExecutableFilePath  
		{
            get { return "RExecutableFilePath"; }
        }
	 
		public static string RImsLogFilePath  
		{
            get { return "RImsLogFilePath"; }
        }
	}
	 
	public static class MaestroAdministrationServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "MaestroAdministrationService" ; }
        }
	  
		public static string DbConnectivityDisplayName  
		{
            get { return "DbConnectivityDisplayName"; }
        }
	 
		public static string DbConnectivityEmailAddress  
		{
            get { return "DbConnectivityEmailAddress"; }
        }
	 
		public static string DbConnectivityHostAddress  
		{
            get { return "DbConnectivityHostAddress"; }
        }
	 
		public static string DbConnectivityMonitorEmailAddresses  
		{
            get { return "DbConnectivityMonitorEmailAddresses"; }
        }
	 
		public static string DbConnectivityMonitorEnabled  
		{
            get { return "DbConnectivityMonitorEnabled"; }
        }
	 
		public static string DbConnectivityMonitorFrequency  
		{
            get { return "DbConnectivityMonitorFrequency"; }
        }
	 
		public static string DbConnectivityPassword  
		{
            get { return "DbConnectivityPassword"; }
        }
	 
		public static string DbConnectivityUserName  
		{
            get { return "DbConnectivityUserName"; }
        }
	 
		public static string MaestroMonitorEmailAddresses  
		{
            get { return "MaestroMonitorEmailAddresses"; }
        }
	 
		public static string MaestroMonitorEnabled  
		{
            get { return "MaestroMonitorEnabled"; }
        }
	 
		public static string ServiceDownMinutesThreshold  
		{
            get { return "ServiceDownMinutesThreshold"; }
        }
	}
	 
	public static class MaestroEnvironmentSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "MaestroEnvironment" ; }
        }
	  
		public static string Environment  
		{
            get { return "Environment"; }
        }
	}
	 
	public static class MaestroSchemaSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "MaestroSchema" ; }
        }
	  
		public static string SchemaVersion  
		{
            get { return "SchemaVersion"; }
        }
	}
	 
	public static class MaterialsServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "MaterialsService" ; }
        }
	  
		public static string IsciScreenerDirectory  
		{
            get { return "IsciScreenerDirectory"; }
        }
	 
		public static string MonitorScreenersDirectoriesEnabled  
		{
            get { return "MonitorScreenersDirectoriesEnabled"; }
        }
	 
		public static string MonitorScreenersDirectoriesInterval  
		{
            get { return "MonitorScreenersDirectoriesInterval"; }
        }
	 
		public static string ReelFinalizedEmailAddresses  
		{
            get { return "ReelFinalizedEmailAddresses"; }
        }
	 
		public static string ReelFinalizedEmailEnabled  
		{
            get { return "ReelFinalizedEmailEnabled"; }
        }
	 
		public static string ReelScreenerDirectory  
		{
            get { return "ReelScreenerDirectory"; }
        }
	 
		public static string ScreenerExtension  
		{
            get { return "ScreenerExtension"; }
        }
	}
	 
	public static class PostingServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "PostingService" ; }
        }
	  
		public static string AirTimeBuffer  
		{
            get { return "AirTimeBuffer"; }
        }
	 
		public static string DefaultExcelExtension  
		{
            get { return "DefaultExcelExtension"; }
        }
	 
		public static string MaxAggregationThreads  
		{
            get { return "MaxAggregationThreads"; }
        }
	 
		public static string MaxPostingThreads  
		{
            get { return "MaxPostingThreads"; }
        }
	 
		public static string OperationsRootDirectory  
		{
            get { return "OperationsRootDirectory"; }
        }
	 
		public static string YearToDateRootDirectory  
		{
            get { return "YearToDateRootDirectory"; }
        }
	}
	 
	public static class PostLogServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "PostLogService" ; }
        }
	  
		public static string AdgoBusinessId  
		{
            get { return "AdgoBusinessId"; }
        }
	 
		public static string AdgoDirectory  
		{
            get { return "AdgoDirectory"; }
        }
	 
		public static string AirTimeBuffer  
		{
            get { return "AirTimeBuffer"; }
        }
	 
		public static string AttBusinessId  
		{
            get { return "AttBusinessId"; }
        }
	 
		public static string AttDirectory  
		{
            get { return "AttDirectory"; }
        }
	 
		public static string AutoPostKickoffDays  
		{
            get { return "AutoPostKickoffDays"; }
        }
	 
		public static string AutoPostKickoffEnabled  
		{
            get { return "AutoPostKickoffEnabled"; }
        }
	 
		public static string AutoPostKickoffStartingHour  
		{
            get { return "AutoPostKickoffStartingHour"; }
        }
	 
		public static string BrightHouseBusinessId  
		{
            get { return "BrightHouseBusinessId"; }
        }
	 
		public static string BrightHouseDirectory  
		{
            get { return "BrightHouseDirectory"; }
        }
	 
		public static string CableOneBusinessId  
		{
            get { return "CableOneBusinessId"; }
        }
	 
		public static string CableOneDirectory  
		{
            get { return "CableOneDirectory"; }
        }
	 
		public static string CableVisionBusinessId  
		{
            get { return "CableVisionBusinessId"; }
        }
	 
		public static string CableVisionDirectory  
		{
            get { return "CableVisionDirectory"; }
        }
	 
		public static string ComcastBusinessId  
		{
            get { return "ComcastBusinessId"; }
        }
	 
		public static string ComcastDirectory  
		{
            get { return "ComcastDirectory"; }
        }
	 
		public static string CoxApexDirectory  
		{
            get { return "CoxApexDirectory"; }
        }
	 
		public static string CoxBusinessId  
		{
            get { return "CoxBusinessId"; }
        }
	 
		public static string CoxDirectory  
		{
            get { return "CoxDirectory"; }
        }
	 
		public static string CoxImwDirectory  
		{
            get { return "CoxImwDirectory"; }
        }
	 
		public static string DailyErrorDirectory  
		{
            get { return "DailyErrorDirectory"; }
        }
	 
		public static string DailyRatingDirectory  
		{
            get { return "DailyRatingDirectory"; }
        }
	 
		public static string DailyReportAlertEnabled  
		{
            get { return "DailyReportAlertEnabled"; }
        }
	 
		public static string DailyReportFolderName  
		{
            get { return "DailyReportFolderName"; }
        }
	 
		public static string DailyReportRecipients  
		{
            get { return "DailyReportRecipients"; }
        }
	 
		public static string DirectTvBusinessId  
		{
            get { return "DirectTvBusinessId"; }
        }
	 
		public static string DirectTvClusterMappings  
		{
            get { return "DirectTvClusterMappings"; }
        }
	 
		public static string DirectTvDirectory  
		{
            get { return "DirectTvDirectory"; }
        }
	 
		public static string DishBusinessId  
		{
            get { return "DishBusinessId"; }
        }
	 
		public static string DishDailyDirectory  
		{
            get { return "DishDailyDirectory"; }
        }
	 
		public static string DishDirectory  
		{
            get { return "DishDirectory"; }
        }
	 
		public static string DishUABusinessId  
		{
            get { return "DishUABusinessId"; }
        }
	 
		public static string DishUADirectory  
		{
            get { return "DishUADirectory"; }
        }
	 
		public static string DishUAZoneId  
		{
            get { return "DishUAZoneId"; }
        }
	 
		public static string DishUZoneId  
		{
            get { return "DishUZoneId"; }
        }
	 
		public static string EnableDatabaseOptimization  
		{
            get { return "EnableDatabaseOptimization"; }
        }
	 
		public static string EnablePostLogFileLoader  
		{
            get { return "EnablePostLogFileLoader"; }
        }
	 
		public static string EnableUniverseAndRatingsFileLoaders  
		{
            get { return "EnableUniverseAndRatingsFileLoaders"; }
        }
	 
		public static string ErrorNotificationRecipients  
		{
            get { return "ErrorNotificationRecipients"; }
        }
	 
		public static string ExcelFileExtension  
		{
            get { return "ExcelFileExtension"; }
        }
	 
		public static string HitsBusinessId  
		{
            get { return "HitsBusinessId"; }
        }
	 
		public static string HitsDirectory  
		{
            get { return "HitsDirectory"; }
        }
	 
		public static string MidcontinentBusinessId  
		{
            get { return "MidcontinentBusinessId"; }
        }
	 
		public static string MidcontinentDirectory  
		{
            get { return "MidcontinentDirectory"; }
        }
	 
		public static string MsadsBusinessId  
		{
            get { return "MsadsBusinessId"; }
        }
	 
		public static string MsadsDirectory  
		{
            get { return "MsadsDirectory"; }
        }
	 
		public static string NationalDrReportDisclaimer  
		{
            get { return "NationalDrReportDisclaimer"; }
        }
	 
		public static string NetworksBusinessId  
		{
            get { return "NetworksBusinessId"; }
        }
	 
		public static string NetworksDirectory  
		{
            get { return "NetworksDirectory"; }
        }
	 
		public static string NyiBusinessId  
		{
            get { return "NyiBusinessId"; }
        }
	 
		public static string NyiDirectory  
		{
            get { return "NyiDirectory"; }
        }
	 
		public static string NyiZoneId  
		{
            get { return "NyiZoneId"; }
        }
	 
		public static string OvernightRatingsAlertEnabled  
		{
            get { return "OvernightRatingsAlertEnabled"; }
        }
	 
		public static string OvernightRatingsAlertRecipients  
		{
            get { return "OvernightRatingsAlertRecipients"; }
        }
	 
		public static string RootErrorDirectory  
		{
            get { return "RootErrorDirectory"; }
        }
	 
		public static string RootGenericDirectory  
		{
            get { return "RootGenericDirectory"; }
        }
	 
		public static string RootMonitorDirectory  
		{
            get { return "RootMonitorDirectory"; }
        }
	 
		public static string RootOperationsDirectory  
		{
            get { return "RootOperationsDirectory"; }
        }
	 
		public static string SpectrumBusinessId  
		{
            get { return "SpectrumBusinessId"; }
        }
	 
		public static string SpectrumDirectory  
		{
            get { return "SpectrumDirectory"; }
        }
	 
		public static string SpinBusinessId  
		{
            get { return "SpinBusinessId"; }
        }
	 
		public static string SpinDirectory  
		{
            get { return "SpinDirectory"; }
        }
	 
		public static string SuddenLinkBusinessId  
		{
            get { return "SuddenLinkBusinessId"; }
        }
	 
		public static string SuddenlinkDirectory  
		{
            get { return "SuddenlinkDirectory"; }
        }
	 
		public static string SuddenlinkWeeklyDirectory  
		{
            get { return "SuddenlinkWeeklyDirectory"; }
        }
	 
		public static string ViaMediaBusinessId  
		{
            get { return "ViaMediaBusinessId"; }
        }
	 
		public static string ViaMediaDirectory  
		{
            get { return "ViaMediaDirectory"; }
        }
	 
		public static string WeeklyReportDisclaimer  
		{
            get { return "WeeklyReportDisclaimer"; }
        }
	 
		public static string WeeklyReportFolderName  
		{
            get { return "WeeklyReportFolderName"; }
        }
	}
	 
	public static class ProposalComposerSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "ProposalComposer" ; }
        }
	  
		public static string ImsSalesExplorerUrl  
		{
            get { return "ImsSalesExplorerUrl"; }
        }
	}
	 
	public static class ProposalsServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "ProposalsService" ; }
        }
	  
		public static string AutoDailyMODDays  
		{
            get { return "AutoDailyMODDays"; }
        }
	 
		public static string AutoDailyMODEnabled  
		{
            get { return "AutoDailyMODEnabled"; }
        }
	 
		public static string AutoDailyMODStartingHour  
		{
            get { return "AutoDailyMODStartingHour"; }
        }
	 
		public static string ErrorNotificationRecipients  
		{
            get { return "ErrorNotificationRecipients"; }
        }
	 
		public static string MediaOceanFourADSDirectory  
		{
            get { return "MediaOceanFourADSDirectory"; }
        }
	 
		public static string MediaOceanFourAOXFTPPassword  
		{
            get { return "MediaOceanFourAOXFTPPassword"; }
        }
	 
		public static string MediaOceanFourAOXFTPRoot  
		{
            get { return "MediaOceanFourAOXFTPRoot"; }
        }
	 
		public static string MediaOceanFourAOXFTPUserName  
		{
            get { return "MediaOceanFourAOXFTPUserName"; }
        }
	 
		public static string MediaOceanPHeaderDirectory  
		{
            get { return "MediaOceanPHeaderDirectory"; }
        }
	 
		public static string MediaOceanTECCServicePassword  
		{
            get { return "MediaOceanTECCServicePassword"; }
        }
	 
		public static string MediaOceanTECCServiceUserName  
		{
            get { return "MediaOceanTECCServiceUserName"; }
        }
	 
		public static string rootOperationsDirectory  
		{
            get { return "rootOperationsDirectory"; }
        }
	}
	 
	public static class ReleaseComposerSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "ReleaseComposer" ; }
        }
	  
		public static string MsoFeatureManagerEnabled  
		{
            get { return "MsoFeatureManagerEnabled"; }
        }
	 
		public static string TrafficFeatureAlertWizardEnabled  
		{
            get { return "TrafficFeatureAlertWizardEnabled"; }
        }
	}
	 
	public static class Releases2ServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "Releases2Service" ; }
        }
	  
		public static string BillingSystemId  
		{
            get { return "BillingSystemId"; }
        }
	 
		public static string DaypartCacheSlidingExpirationSeconds  
		{
            get { return "DaypartCacheSlidingExpirationSeconds"; }
        }
	 
		public static string DefaultEmailGroupTo  
		{
            get { return "DefaultEmailGroupTo"; }
        }
	 
		public static string MaxTrafficBreakdownThreads  
		{
            get { return "MaxTrafficBreakdownThreads"; }
        }
	 
		public static string PDFPath  
		{
            get { return "PDFPath"; }
        }
	 
		public static string TrafficAlertsPath  
		{
            get { return "TrafficAlertsPath"; }
        }
	 
		public static string URLTrafficCopyManagement  
		{
            get { return "URLTrafficCopyManagement"; }
        }
	 
		public static string XMLOutputPath  
		{
            get { return "XMLOutputPath"; }
        }
	}
	 
	public static class ReleaseServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "ReleaseService" ; }
        }
	  
		public static string OutboxPath  
		{
            get { return "OutboxPath"; }
        }
	}
	 
	public static class ReportingServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "ReportingService" ; }
        }
	  
		public static string BroadcastPdfDirectory  
		{
            get { return "BroadcastPdfDirectory"; }
        }
	 
		public static string CMWPDFPath  
		{
            get { return "CMWPDFPath"; }
        }
	 
		public static string ConfirmationRootDirectory  
		{
            get { return "ConfirmationRootDirectory"; }
        }
	 
		public static string DefaultExcelExtension  
		{
            get { return "DefaultExcelExtension"; }
        }
	 
		public static string FourAsDisclaimer  
		{
            get { return "FourAsDisclaimer"; }
        }
	 
		public static string MaxLoaderThreads  
		{
            get { return "MaxLoaderThreads"; }
        }
	 
		public static string MinuteByMinuteReportPythonScriptFilePath  
		{
            get { return "MinuteByMinuteReportPythonScriptFilePath"; }
        }
	 
		public static string PDFPath  
		{
            get { return "PDFPath"; }
        }
	 
		public static string PostRootDirectory  
		{
            get { return "PostRootDirectory"; }
        }
	 
		public static string TAM4AsContact  
		{
            get { return "TAM4AsContact"; }
        }
	 
		public static string VantageImportFTPAddress  
		{
            get { return "VantageImportFTPAddress"; }
        }
	 
		public static string VantageImportFTPPassword  
		{
            get { return "VantageImportFTPPassword"; }
        }
	 
		public static string VantageImportFTPUser  
		{
            get { return "VantageImportFTPUser"; }
        }
	 
		public static string XMLPath  
		{
            get { return "XMLPath"; }
        }
	}
	 
	public static class ScxFixComposerSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "ScxFixComposer" ; }
        }
	  
		public static string ComcastMappingsFile  
		{
            get { return "ComcastMappingsFile"; }
        }
	 
		public static string DishMappingsFile  
		{
            get { return "DishMappingsFile"; }
        }
	}
	 
	public static class SystemComposerSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "SystemComposer" ; }
        }
	  
		public static string DivisionsEditorUrl  
		{
            get { return "DivisionsEditorUrl"; }
        }
	 
		public static string MvpdMinSpotCostEditorUrl  
		{
            get { return "MvpdMinSpotCostEditorUrl"; }
        }
	 
		public static string SpotLimitEditorUrl  
		{
            get { return "SpotLimitEditorUrl"; }
        }
	 
		public static string URLProcessCableTrack  
		{
            get { return "URLProcessCableTrack"; }
        }
	 
		public static string ZoneToDmaMultiplicity  
		{
            get { return "ZoneToDmaMultiplicity"; }
        }
	}
	 
	public static class TrackerServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "TrackerService" ; }
        }
	  
		public static string FtpDirectory  
		{
            get { return "FtpDirectory"; }
        }
	 
		public static string FtpPassword  
		{
            get { return "FtpPassword"; }
        }
	 
		public static string FtpSaveFolder  
		{
            get { return "FtpSaveFolder"; }
        }
	 
		public static string FtpUrl  
		{
            get { return "FtpUrl"; }
        }
	 
		public static string FtpUserName  
		{
            get { return "FtpUserName"; }
        }
	}
	 
	public static class TrafficServiceSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "TrafficService" ; }
        }
	  
		public static string EnableTrafficCapEmailAddresses  
		{
            get { return "EnableTrafficCapEmailAddresses"; }
        }
	 
		public static string LockTimeoutInSeconds  
		{
            get { return "LockTimeoutInSeconds"; }
        }
	 
		public static string TrafficCancellationNotificationEmailAddresses  
		{
            get { return "TrafficCancellationNotificationEmailAddresses"; }
        }
	 
		public static string TrafficCapEmailAddresses  
		{
            get { return "TrafficCapEmailAddresses"; }
        }
	}
	 
	public static class WizeBuysComposerSystemParameterNames
	{
		public static string ComponentID
        {
            get { return "WizeBuysComposer" ; }
        }
	  
		public static string XMLOutputPath  
		{
            get { return "XMLOutputPath"; }
        }
	}
	 
}