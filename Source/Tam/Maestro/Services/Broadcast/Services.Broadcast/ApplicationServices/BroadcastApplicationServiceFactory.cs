using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using ConfigurationService.Client;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Services.Broadcast.ApplicationServices.Inventory;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters;
using Services.Broadcast.Converters.Post;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using Services.Broadcast.ApplicationServices.Security;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.Helpers;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Cache;

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
                        _instance.RegisterType<ILockingManagerApplicationService, LockingManagerApplicationService>();
                        _instance.RegisterInstance<IConfigurationWebApiClient>(ConfigurationClientSwitch.Handler);
                        _instance.RegisterType<IDataRepositoryFactory, BroadcastDataDataRepositoryFactory>();

                        SystemComponentParameterHelper.SetConfigurationClient(ConfigurationClientSwitch.Handler);
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
            unityContainer.RegisterType<IBvsPostingEngine, BvsBvsPostingEngine>();
            unityContainer.RegisterType<IDateAdjustmentEngine, DateAdjustmentEngine>();
            unityContainer.RegisterType<ITrackingEngine, TrackingEngine>();
            unityContainer.RegisterType<IScxScheduleConverter, ScxScheduleConverter>();
            unityContainer.RegisterType<ICsvHelper, CsvHelper>();
            unityContainer.RegisterType<IPostLogBaseFileConverter, PostLogBaseFileConverter>();
            unityContainer.RegisterType<IBvsConverter, BvsConverter>();
            unityContainer.RegisterType<ISigmaConverter, SigmaConverter>();
            unityContainer.RegisterType<IDefaultScheduleConverter, DefaultScheduleConverter>();
            unityContainer.RegisterType<IAssemblyScheduleConverter, AssemblyScheduleConverter>();
            unityContainer.RegisterType<IInventorySummaryCache, InventorySummaryCache>(new ContainerControlledLifetimeManager()); // singleton
            unityContainer.RegisterType<IBroadcastAudiencesCache, BroadcastAudiencesCache>();
            unityContainer.RegisterType<IMarketCoverageCache, MarketCoverageCache>();
            unityContainer.RegisterType<IInventoryService, InventoryService>();
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
            unityContainer.RegisterType<INtiTransmittalsService, NtiTransmittalsService>();
            unityContainer.RegisterType<IPricingGuideService, PricingGuideService>();
            unityContainer.RegisterType<IProposalScxConverter, ProposalScxConverter>();
            unityContainer.RegisterType<IProposalScxDataPrep, ProposalScxDataPrep>();
            unityContainer.RegisterType<IInventoryScxDataConverter, InventoryScxDataConverter>();
            unityContainer.RegisterType<IOpenMarketFileImporter, OpenMarketFileImporter>();

            unityContainer.RegisterType<IProposalCalculationEngine, ProposalCalculationEngine>();
            unityContainer.RegisterType<IQuarterCalculationEngine, QuarterCalculationEngine>();
            unityContainer.RegisterType<INsiUniverseService, NsiUniverseService>();

            unityContainer.RegisterType<IMediaMonthAndWeekAggregateCache, MediaMonthAndWeekAggregateCache>();
            unityContainer.RegisterType<IMediaMonthAndWeekAggregateRepository, MediaMonthAndWeekAggregateAndWeekAggregateRepository>();

            //unityContainer.RegisterType<fill, fill>();
            unityContainer.RegisterType<IRatingForecastService, RatingForecastService>();
            unityContainer.RegisterType<IStationContactMasterFileImporter, StationContactMasterFileImporter>();

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
            unityContainer.RegisterType<IAffidavitPreprocessingService, AffidavitPreprocessingService>();
            unityContainer.RegisterType<IAffidavitPostProcessingService, AffidavitPostProcessingService>();
            unityContainer.RegisterType<IMatchingEngine, MatchingEngine>();
            unityContainer.RegisterType<IProgramScrubbingEngine, ProgramScrubbingEngine>();
            unityContainer.RegisterType<IWhosWatchingTvService, WhosWatchingTvService>();
            unityContainer.RegisterType<IWWTVEmailProcessorService, WWTVEmailProcessorService>();
            unityContainer.RegisterType<IAffidavitValidationEngine, AffidavitValidationEngine>();
            unityContainer.RegisterType<IPostReportService, PostReportService>();
            unityContainer.RegisterType<IImpressionsService, ImpressionsService>();
            unityContainer.RegisterType<ISpotTrackerService, SpotTrackerService>();
            unityContainer.RegisterType<IIsciService, IsciService>();

            unityContainer.RegisterType<IInventoryScxDataPrepFactory, InventoryScxDataPrepFactory>();
            
            unityContainer.RegisterType<IPostLogPreprocessingService, PostLogPreprocessingService>();
            unityContainer.RegisterType<IPostLogPostProcessingService, PostLogPostProcessingService>();

            unityContainer.RegisterType<INsiPostingBookService, NsiPostingBookService>();

            unityContainer.RegisterType<IEmailerService, EmailerService>();

            unityContainer.RegisterType<IWWTVFtpHelper, WWTVFtpHelper>();
            unityContainer.RegisterType<IFtpService, FtpService>();
            unityContainer.RegisterType<IFileService, FileService>();
            unityContainer.RegisterType<IFileTransferEmailHelper, FileTransferEmailHelper>();

            unityContainer.RegisterType<IImpersonateUser, ImpersonateUser>();
            unityContainer.RegisterType<IWWTVSharedNetworkHelper, WWTVSharedNetworkHelper>();
            unityContainer.RegisterType<IExcelHelper, ExcelHelper>();

            unityContainer.RegisterType<IStationProcessingEngine, StationProcessingEngine>();
            unityContainer.RegisterType<ISpotLengthEngine, SpotLengthEngine>();
            unityContainer.RegisterType<IInventoryDaypartParsingEngine, InventoryDaypartParsingEngine>();
            unityContainer.RegisterType<ILockingEngine, LockingEngine>();
            unityContainer.RegisterType<IInventoryRatingsProcessingService, InventoryRatingsProcessingService>();
            unityContainer.RegisterType<IInventoryWeekEngine, InventoryWeekEngine>();

            unityContainer.RegisterType<IDataLakeFileService, DataLakeFileService>();
            unityContainer.RegisterType<IDataLakeSystemParameters, DataLakeSystemParameters>();

            unityContainer.RegisterType<IInventorySummaryService, InventorySummaryService>();
            unityContainer.RegisterType<IInventoryGapCalculationEngine, InventoryGapCalculationEngine>();

            unityContainer.RegisterType<ICampaignService, CampaignService>();

            unityContainer.RegisterType<IDaypartCodeService, DaypartCodeService>();

            unityContainer.RegisterType<IScxGenerationService, ScxGenerationService>();


            //@todo This is temporary to control the daypart source for Broadcast
            var repoFactory = unityContainer.Resolve<IDataRepositoryFactory>();
            var daypartRepo = repoFactory.GetDataRepository<IDisplayDaypartRepository>();
            DaypartCache.DaypartCacheInstance = new DaypartCache(daypartRepo);
            unityContainer.RegisterInstance<IDaypartCache>(DaypartCache.Instance);

            MediaMonthCrunchCache.MediaMonthCrunchCacheInstance = new MediaMonthCrunchCache(repoFactory, unityContainer.Resolve<IMediaMonthAndWeekAggregateCache>());
            unityContainer.RegisterInstance<IMediaMonthCrunchCache>(MediaMonthCrunchCache.MediaMonthCrunchCacheInstance);
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
