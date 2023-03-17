using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.States;
using Services.Broadcast.ApplicationServices.Buying;
using Services.Broadcast.ApplicationServices.Inventory;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping;
using Services.Broadcast.ApplicationServices.Maintenance;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.BusinessEngines.PlanBuying;
using Services.Broadcast.BusinessEngines.PlanPricing;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters;
using Services.Broadcast.Converters.Post;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Unity;
using Unity.Injection;
using Unity.Interception;
using Unity.Interception.InterceptionBehaviors;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using Unity.Lifetime;
using System.Net.Http;
using System.Threading;

namespace Services.Broadcast.ApplicationServices
{
    public class BroadcastApplicationServiceFactory
    {
        private static volatile UnityContainer _instance = null;
        private static readonly object SyncLock = new object();

        public static UnityContainer Instance
        {
            get
            {
                lock (SyncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new UnityContainer();

                        _instance.RegisterInstance<ISMSClient>(SMSClient.Handler);                        
                        _instance.RegisterType<ILaunchDarklyClient, LaunchDarklyClient>();

                        _instance.RegisterType<IDataRepositoryFactory, BroadcastDataDataRepositoryFactory>();

                        // Upgrade to 4.7.2 and now this isn't needed.  Throws an error if included.
                        //_instance.RegisterType<JobStorage>(new InjectionFactory(c => JobStorage.Current));

                        _instance.RegisterType<IJobFilterProvider, JobFilterAttributeFilterProvider>(new InjectionConstructor(true));
                        _instance.RegisterType<IBackgroundJobFactory, BackgroundJobFactory>();
                        _instance.RegisterType<IRecurringJobManager, RecurringJobManager>(new InjectionConstructor());
                        _instance.RegisterType<IBackgroundJobClient, BackgroundJobClient>();
                        _instance.RegisterType<IBackgroundJobStateChanger, BackgroundJobStateChanger>();

                        _instance.RegisterType<IAsyncTaskHelper, AsyncTaskHelper>();

                        RegisterApplicationServices(_instance);
                    }
                }
                return _instance;
            }
        }

        public static void RegisterApplicationServices(UnityContainer unityContainer)
        {
            unityContainer.RegisterType<IMarketService, MarketService>();
            unityContainer.RegisterType<ILogoService, LogoService>();
            unityContainer.RegisterType<ITransactionHelper, TransactionHelper>();
            unityContainer.RegisterType<ITrackerService, TrackerService>();
            unityContainer.RegisterType<IDetectionPostingEngine, DetectionPostingEngine>();
            unityContainer.RegisterType<IDateTimeEngine, DateTimeEngine>();
            unityContainer.RegisterType<IDateAdjustmentEngine, DateAdjustmentEngine>();
            unityContainer.RegisterType<ITrackingEngine, TrackingEngine>();
            unityContainer.RegisterType<IWeeklyBreakdownEngine, WeeklyBreakdownEngine>();
            unityContainer.RegisterType<ICreativeLengthEngine, CreativeLengthEngine>();
            unityContainer.RegisterType<IScxScheduleConverter, ScxScheduleConverter>();
            unityContainer.RegisterType<ICsvHelper, CsvHelper>();
            unityContainer.RegisterType<IPostLogBaseFileConverter, PostLogBaseFileConverter>();
            unityContainer.RegisterType<IDetectionConverter, DetectionConverter>();
            unityContainer.RegisterType<ISigmaConverter, SigmaConverter>();
            unityContainer.RegisterType<IDefaultScheduleConverter, DefaultScheduleConverter>();
            unityContainer.RegisterType<IAssemblyScheduleConverter, AssemblyScheduleConverter>();
            unityContainer.RegisterType<IInventoryService, InventoryService>();
            unityContainer.RegisterType<IInventoryExportService, InventoryExportService>();
            unityContainer.RegisterType<IInventoryMarketAffiliatesExportService, InventoryMarketAffiliatesExportService>();
            unityContainer.RegisterType<IProprietaryInventoryService, ProprietaryInventoryService>();
            unityContainer.RegisterType<IInventoryFileValidator, InventoryFileValidator>();
            unityContainer.RegisterType<ISchedulesReportService, SchedulesReportService>();
            unityContainer.RegisterType<IScheduleAggregateFactoryService, ScheduleAggregateFactoryService>();
            unityContainer.RegisterType<IScheduleReportDtoFactoryService, ScheduleReportDtoFactoryService>();
            unityContainer.RegisterType<IProposalService, ProposalService>();
            unityContainer.RegisterType<IProposalProprietaryInventoryService, ProposalProprietaryInventoryService>();
            unityContainer.RegisterType<IProposalOpenMarketInventoryService, ProposalOpenMarketInventoryService>();
            unityContainer.RegisterType<IProposalWeeklyTotalCalculationEngine, ProposalProprietaryTotalsCalculationEngine>();
            unityContainer.RegisterType<ITrafficService, TrafficService>();
            unityContainer.RegisterType<IStationInventoryGroupService, StationInventoryGroupService>();
            unityContainer.RegisterType<IStationService, StationService>();
            unityContainer.RegisterType<INtiTransmittalsService, NtiTransmittalsService>();
            unityContainer.RegisterType<IPricingGuideService, PricingGuideService>();
            unityContainer.RegisterType<IProposalScxConverter, ProposalScxConverter>();
            unityContainer.RegisterType<IProposalScxDataPrep, ProposalScxDataPrep>();
            unityContainer.RegisterType<IInventoryScxDataConverter, InventoryScxDataConverter>();
            unityContainer.RegisterType<IOpenMarketFileImporter, OpenMarketFileImporter>();
            unityContainer.RegisterType<IUserService, UserService>();

            unityContainer.RegisterType<IProposalCalculationEngine, ProposalCalculationEngine>();
            unityContainer.RegisterType<IQuarterCalculationEngine, QuarterCalculationEngine>();
            unityContainer.RegisterType<INsiUniverseService, NsiUniverseService>(new ContainerControlledLifetimeManager()); // singleton
            unityContainer.RegisterType<INtiUniverseService, NtiUniverseService>();
            unityContainer.RegisterType<IMediaMonthAndWeekAggregateRepository, MediaMonthAndWeekAggregateAndWeekAggregateRepository>();

            //unityContainer.RegisterType<fill, fill>();
            unityContainer.RegisterType<IRatingForecastService, RatingForecastService>();
            unityContainer.RegisterType<IStationContactMasterFileImporter, StationContactMasterFileImporter>();
            unityContainer.RegisterType<IUniversesFileImporter, UniversesFileImporter>();
            unityContainer.RegisterType<IVpvhFileImporter, VpvhFileImporter>();

            unityContainer.RegisterType<IProposalProgramsCalculationEngine, ProposalProgramsCalculationEngine>();
            unityContainer.RegisterType<IProposalMarketsCalculationEngine, ProposalMarketsCalculationEngine>();
            unityContainer.RegisterType<IProposalOpenMarketsTotalsCalculationEngine, ProposalOpenMarketsTotalsCalculationEngine>();

            unityContainer.RegisterType<IPostEngine, PostEngine>();
            unityContainer.RegisterType<IPostFileParser, PostFileParser>();
            unityContainer.RegisterType<IPostFileParserFactory, PostFileParserFactory>();
            unityContainer.RegisterType<IReportGenerator<PostPrePostingFile>, PostExcelReportGenerator>();
            unityContainer.RegisterType<IReportGenerator<PostReport>, PostReportGenerator>();
            unityContainer.RegisterType<IPostPrePostingService, PostPrePostingService>();
            unityContainer.RegisterType<IProprietarySpotCostCalculationEngine, ProprietarySpotCostCalculationEngine>();
            unityContainer.RegisterType<IImpressionAdjustmentEngine, ImpressionAdjustmentEngine>();
            unityContainer.RegisterType<IProjectionBooksService, ProjectionBooksService>();
            unityContainer.RegisterType<IProposalDetailHeaderTotalsCalculationEngine, ProposalDetailHeaderTotalsCalculationEngine>();
            unityContainer.RegisterType<IProposalDetailWeekTotalsCalculationEngine, ProposalDetailWeekTotalsCalculationEngine>();
            unityContainer.RegisterType<IProposalTotalsCalculationEngine, ProposalTotalsCalculationEngine>();
            unityContainer.RegisterType<IMyEventsReportNamingEngine, MyEventsReportNamingEngine>();
            unityContainer.RegisterType<IPostLogValidationEngine, PostLogValidationEngine>();
            unityContainer.RegisterType<IPricingGuideDistributionEngine, PricingGuideDistributionEngine>();
            unityContainer.RegisterType<IProprietaryFileImporterFactory, ProprietaryFileImporterFactory>();
            unityContainer.RegisterType<ICNNStationInventoryGroupService, CNNStationInventoryGroupService>();
            unityContainer.RegisterType<IStationInventoryManifestService, StationInventoryManifestService>();

            unityContainer.RegisterType<IPostLogService, PostLogService>();
            unityContainer.RegisterType<IAffidavitService, AffidavitService>();
            unityContainer.RegisterType<IMatchingEngine, MatchingEngine>();
            unityContainer.RegisterType<IProgramScrubbingEngine, ProgramScrubbingEngine>();
            unityContainer.RegisterType<IAffidavitValidationEngine, AffidavitValidationEngine>();
            unityContainer.RegisterType<IPostReportService, PostReportService>();
            unityContainer.RegisterType<IImpressionsService, ImpressionsService>();
            unityContainer.RegisterType<ISpotTrackerService, SpotTrackerService>();
            unityContainer.RegisterType<IIsciService, IsciService>();

            unityContainer.RegisterType<IInventoryScxDataPrepFactory, InventoryScxDataPrepFactory>();

            unityContainer.RegisterType<INsiPostingBookService, NsiPostingBookService>();

            unityContainer.RegisterType<IEmailerService, EmailerService>();

            unityContainer.RegisterType<IFtpService, FtpService>();
            unityContainer.RegisterType<IFileService, FileService>();
            unityContainer.RegisterType<IFileTransferEmailHelper, FileTransferEmailHelper>();

            unityContainer.RegisterType<IImpersonateUser, ImpersonateUser>();
            unityContainer.RegisterType<IExcelHelper, ExcelHelper>();

            unityContainer.RegisterType<IStationProcessingEngine, StationProcessingEngine>();
            unityContainer.RegisterType<IInventoryDaypartParsingEngine, InventoryDaypartParsingEngine>();
            unityContainer.RegisterType<ILockingEngine, LockingEngine>();
            unityContainer.RegisterType<IInventoryRatingsProcessingService, InventoryRatingsProcessingService>();
            unityContainer.RegisterType<IInventoryProgramsProcessingService, InventoryProgramsProcessingService>();
            unityContainer.RegisterType<IInventoryProgramsProcessorFactory, InventoryProgramsProcessorFactory>();
            unityContainer.RegisterType<IInventoryProgramsRepairEngine, InventoryProgramsRepairEngine>();
            unityContainer.RegisterType<IInventoryWeekEngine, InventoryWeekEngine>();

            unityContainer.RegisterType<IPlanIsciService, PlanIsciService>();           
            unityContainer.RegisterType<IInventorySummaryService, InventorySummaryService>();
            unityContainer.RegisterType<IInventoryProprietarySummaryService, InventoryProprietarySummaryService>();
            unityContainer.RegisterType<IInventoryGapCalculationEngine, InventoryGapCalculationEngine>();
            unityContainer.RegisterType<IInventoryExportEngine, InventoryExportEngine>();

            unityContainer.RegisterType<ICampaignService, CampaignService>();
            unityContainer.RegisterType<ICampaignValidator, CampaignValidator>();

            unityContainer.RegisterType<IAgencyService, AgencyService>();
            unityContainer.RegisterType<IAdvertiserService, AdvertiserService>();
            unityContainer.RegisterType<IProductService, ProductService>();

            unityContainer.RegisterType<IStandardDaypartService, StandardDaypartService>();
            unityContainer.RegisterType<IShowTypeService, ShowTypeService>();
            unityContainer.RegisterType<IContainTypeService, ContainTypeService>();
            unityContainer.RegisterType<IAffiliateService, AffiliateService>();

            unityContainer.RegisterType<IVpvhService, VpvhService>();
            unityContainer.RegisterType<IVpvhExportEngine, VpvhExportEngine>();

            unityContainer.RegisterType<IScxGenerationService, ScxGenerationService>();

            unityContainer.RegisterType<IPlanService, PlanService>();
            unityContainer.RegisterType<IPlanValidator, PlanValidator>();
            unityContainer.RegisterType<ISpotLengthService, SpotLengthService>();
            unityContainer.RegisterType<IPostingTypeService, PostingTypeService>();
            unityContainer.RegisterType<IAudienceService, AudienceService>();
            unityContainer.RegisterType<IPostingBookService, PostingBookService>();
            unityContainer.RegisterType<IPlanBudgetDeliveryCalculator, PlanBudgetDeliveryCalculator>();
            unityContainer.RegisterType<IPlanAggregator, PlanAggregator>();
            unityContainer.RegisterType<IBuyingService, BuyingService>();

            unityContainer.RegisterType<IEnvironmentService, EnvironmentService>();
            unityContainer.RegisterType<IDaypartTypeService, DaypartTypeService>();
            unityContainer.RegisterType<ISharedFolderService, SharedFolderService>();

            unityContainer.RegisterType<IDaypartCleanupService, DaypartCleanupService>();
            unityContainer.RegisterType<IStationMappingService, StationMappingService>();
            unityContainer.RegisterType<IProgramMappingCleanupEngine, ProgramMappingCleanupEngine>();
            unityContainer.RegisterType<IProgramMappingService, ProgramMappingService>();
            unityContainer.RegisterType<IProgramNameMappingsExportEngine, ProgramNameMappingsExportEngine>();
            unityContainer.RegisterType<IMasterProgramListImporter, MasterProgramListImporter>();

            unityContainer.RegisterType<ICampaignAggregator, CampaignAggregator>();
            unityContainer.RegisterType<ICampaignAggregationJobTrigger, CampaignAggregationJobTrigger>();

            unityContainer.RegisterType<IGenreService, GenreService>();

            unityContainer.RegisterType<IProgramService, ProgramService>();

            unityContainer.RegisterType<IPlanPricingService, PlanPricingService>();

            unityContainer.RegisterType<IPricingApiClient, PricingJobQueueApiClient>();

            unityContainer.RegisterType<IPricingRequestLogClient, PricingRequestLogClientAmazonS3>();
            unityContainer.RegisterType<IImpressionsCalculationEngine, ImpressionsCalculationEngine>();
            unityContainer.RegisterType<ILogToAmazonS3, LogToAmazonS3>();

            unityContainer.RegisterType<IPlanPricingInventoryEngine, PlanPricingInventoryEngine>();
            unityContainer.RegisterType<IPlanPricingInventoryQuarterCalculatorEngine, PlanPricingInventoryQuarterCalculatorEngine>();
            unityContainer.RegisterType<IPlanPricingBandCalculationEngine, PlanPricingBandCalculationEngine>();
            unityContainer.RegisterType<IPlanPricingUnitCapImpressionsCalculationEngine, PlanPricingUnitCapImpressionsCalculationEngine>();
            unityContainer.RegisterType<IPlanPricingStationCalculationEngine, PlanPricingStationCalculationEngine>();
            unityContainer.RegisterType<IPlanPricingMarketResultsEngine, PlanPricingMarketResultsEngine>();
            unityContainer.RegisterType<IPlanPricingProgramCalculationEngine, PlanPricingProgramCalculationEngine>();
            unityContainer.RegisterType<IHealthService, HealthService>();

            unityContainer.RegisterType<IPlanPricingScxDataPrep, PlanPricingScxDataPrep>();
            unityContainer.RegisterType<IPlanPricingScxDataConverter, PlanPricingScxDataConverter>();

            //plan buying tab
            unityContainer.RegisterType<IPlanBuyingApiClient, PlanBuyingJobQueueApiClient>();
            unityContainer.RegisterType<IPlanBuyingRequestLogClient, PlanBuyingRequestLogClientAmazonS3>();
            unityContainer.RegisterType<IPlanBuyingService, PlanBuyingService>();
            unityContainer.RegisterType<IPlanBuyingInventoryEngine, PlanBuyingInventoryEngine>();
            unityContainer.RegisterType<IPlanBuyingInventoryQuarterCalculatorEngine, PlanBuyingInventoryQuarterCalculatorEngine>();
            unityContainer.RegisterType<IPlanBuyingBandCalculationEngine, PlanBuyingBandCalculationEngine>();
            unityContainer.RegisterType<IPlanBuyingUnitCapImpressionsCalculationEngine, PlanBuyingUnitCapImpressionsCalculationEngine>();
            unityContainer.RegisterType<IPlanBuyingStationEngine, PlanBuyingStationEngine>();
            unityContainer.RegisterType<IPlanBuyingProgramEngine, PlanBuyingProgramEngine>();
            unityContainer.RegisterType<IPlanBuyingOwnershipGroupEngine, PlanBuyingOwnershipGroupEngine>();
            unityContainer.RegisterType<IPlanBuyingMarketResultsEngine, PlanBuyingMarketEngine>();
            unityContainer.RegisterType<IPlanBuyingRepFirmEngine, PlanBuyingRepFirmEngine>();

            unityContainer.RegisterType<IPlanBuyingScxDataPrep, PlanBuyingScxDataPrep>();
            unityContainer.RegisterType<IPlanBuyingScxDataConverter, PlanBuyingScxDataConverter>();

            unityContainer.RegisterType<IPlanMarketSovCalculator, PlanMarketSovCalculator>();

            unityContainer.RegisterType<IReelIsciIngestService, ReelIsciIngestService>();
            unityContainer.RegisterType<IReelIsciApiClient, ReelIsciApiClient>();

            unityContainer.RegisterType<ICampaignServiceApiClient, CampaignServiceApiClient>();
            unityContainer.RegisterType<IApiTokenManager, ApiTokenManager>(TypeLifetime.Singleton);
            
            // Aab Related
            unityContainer.RegisterType<IAgencyAdvertiserBrandApiClient, AgencyAdvertiserBrandApiClient>();
            unityContainer.RegisterType<IAabEngine, AabEngine>();

            // Spot Exceptions Api
            unityContainer.RegisterType<ISpotExceptionsService, SpotExceptionsService>();
            unityContainer.RegisterType<ISpotExceptionsServiceV2, SpotExceptionsServiceV2>();
            unityContainer.RegisterType<ISpotExceptionsRecommendedPlanService, SpotExceptionsRecommendedPlanService>();
            unityContainer.RegisterType<ISpotExceptionsOutOfSpecService, SpotExceptionsOutOfSpecService>();
            unityContainer.RegisterType<ISpotExceptionsOutOfSpecServiceV2, SpotExceptionsOutOfSpecServiceV2>();
            unityContainer.RegisterType<ISpotExceptionsUnpostedService, SpotExceptionsUnpostedService>();
            unityContainer.RegisterType<ISpotExceptionsUnpostedServiceV2, SpotExceptionsUnpostedServiceV2>();
            unityContainer.RegisterType<ISpotExceptionsSyncService, SpotExceptionsSyncService>();
            unityContainer.RegisterType<ISpotExceptionsApiClient, SpotExceptionsApiClient>();
            unityContainer.RegisterType<ISpotExceptionsValidator, SpotExceptionsValidator>();

            //locking service
            unityContainer.RegisterType<IGeneralLockingApiClient, GeneralLockingApiClient>();
            unityContainer.RegisterType<IBroadcastLockingService, BroadcastLockingService>();
            unityContainer.RegisterType<IBroadcastLockingManagerApplicationService, BroadcastLockingManagerApplicationService>();
            //Inventory Microservice service
            unityContainer.RegisterType<IInventoryManagementApiClient, InventoryManagementApiClient>();
            unityContainer.RegisterType<IInventoryManagementApiClient, InventoryManagementApiClient>();
            //launch darkly
            unityContainer.RegisterType<IFeatureToggleHelper, FeatureToggleHelper>();

            unityContainer.RegisterType<IAttachmentMicroServiceApiClient, AttachmentMicroServiceApiClient>();

            // singletons
            unityContainer.RegisterType<IInventorySummaryCache, InventorySummaryCache>(new ContainerControlledLifetimeManager()); // singleton
            unityContainer.RegisterType<IMarketCoverageCache, MarketCoverageCache>(TypeLifetime.Singleton);
            unityContainer.RegisterType<IMediaMonthAndWeekAggregateCache, MediaMonthAndWeekAggregateCache>(TypeLifetime.Singleton);
            unityContainer.RegisterType<IBroadcastAudiencesCache, BroadcastAudiencesCache>(TypeLifetime.Singleton);
            unityContainer.RegisterType<IAabCache, AabCache>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<IGenreCache, GenreCache>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<IShowTypeCache, ShowTypeCache>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<IConfigurationSettingsHelper, ConfigurationSettingsHelper>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<ISpotLengthEngine, SpotLengthEngine>(TypeLifetime.Singleton);
            unityContainer.RegisterType<IApiTokenManager, ApiTokenManager>(TypeLifetime.Singleton);

            _SetupMEdiaMonthCrunchCache(unityContainer);

            //@todo This is temporary to control the daypart source for Broadcast
            _SetupDaypartCache(unityContainer);

            //RestClient
            unityContainer.RegisterFactory<HttpClient>(x => new HttpClient() { Timeout = Timeout.InfiniteTimeSpan }, FactoryLifetime.Singleton);
        }

        private static void _SetupMEdiaMonthCrunchCache(UnityContainer unityContainer)
        {
            var repoFactory = unityContainer.Resolve<IDataRepositoryFactory>();
            MediaMonthCrunchCache.MediaMonthCrunchCacheInstance = new MediaMonthCrunchCache(repoFactory, unityContainer.Resolve<IMediaMonthAndWeekAggregateCache>(), unityContainer.Resolve<IConfigurationSettingsHelper>());
            unityContainer.RegisterInstance<IMediaMonthCrunchCache>(MediaMonthCrunchCache.MediaMonthCrunchCacheInstance);
        }

        private static void _SetupDaypartCache(UnityContainer unityContainer)
        {
            var repoFactory = unityContainer.Resolve<IDataRepositoryFactory>();
            var daypartRepo = repoFactory.GetDataRepository<IDisplayDaypartRepository>();
            var featureToggleHelper = unityContainer.Resolve<IFeatureToggleHelper>();
            var configHelper = unityContainer.Resolve<IConfigurationSettingsHelper>();

            DaypartCache.DaypartCacheInstance = new DaypartCache(daypartRepo, featureToggleHelper, configHelper);
            unityContainer.RegisterInstance<IDaypartCache>(DaypartCache.Instance);
        }

        public T GetApplicationService<T>() where T : class, IApplicationService
        {
            return Intercept.ThroughProxy(
                Instance.Resolve<T>(),
                new InterfaceInterceptor(),
                new IInterceptionBehavior[] { });
        }

    }
}
