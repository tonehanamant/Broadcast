﻿using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
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
                        _instance.RegisterType<IDataRepositoryFactory, BroadcastDataDataRepositoryFactory>();

                        SystemComponentHelper.SetSmsClient(SMSClient.Handler);
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
            unityContainer.RegisterType<IPostLogBaseFileConverter, PostLogBaseFileConverter>();
            unityContainer.RegisterType<IBvsConverter, BvsConverter>();
            unityContainer.RegisterType<ISigmaConverter, SigmaConverter>();
            unityContainer.RegisterType<IKeepingTracConverter, KeepingTracConverter>();
            unityContainer.RegisterType<IDefaultScheduleConverter, DefaultScheduleConverter>();
            unityContainer.RegisterType<IAssemblyScheduleConverter, AssemblyScheduleConverter>();
            unityContainer.RegisterType<IBroadcastAudiencesCache, BroadcastAudiencesCache>();
            unityContainer.RegisterType<IInventoryService, InventoryService>();
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

            unityContainer.RegisterType<IProposalScxConverter, ProposalScxConverter>();
            unityContainer.RegisterType<IProposalScxDataPrep, ProposalScxDataPrep>();

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
            unityContainer.RegisterType<IReportGenerator<NsiPostReport>, NSIPostReportGenerator>();
            unityContainer.RegisterType<IPostPrePostingService, PostPrePostingService>();
            unityContainer.RegisterType<IProprietarySpotCostCalculationEngine, ProprietarySpotCostCalculationEngine>();
            unityContainer.RegisterType<IImpressionAdjustmentEngine, ImpressionAdjustmentEngine>();
			unityContainer.RegisterType<IProjectionBooksService, ProjectionBooksService>();
            unityContainer.RegisterType<IProposalDetailHeaderTotalsCalculationEngine, ProposalDetailHeaderTotalsCalculationEngine>();
            unityContainer.RegisterType<IProposalDetailWeekTotalsCalculationEngine, ProposalDetailWeekTotalsCalculationEngine>();
            unityContainer.RegisterType<IProposalTotalsCalculationEngine, ProposalTotalsCalculationEngine>();
            unityContainer.RegisterType<IMyEventsReportNamingEngine, MyEventsReportNamingEngine>();
            unityContainer.RegisterType<IPostLogEngine, PostLogEngine>();
            unityContainer.RegisterType<IPricingGuideDistributionEngine, PricingGuideDistributionEngine>();

            unityContainer.RegisterType<IInventoryFileImporterFactory, InventoryFileImporterFactory>();
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

            unityContainer.RegisterType<IStationProcessingEngine, StationProcessingEngine>();

            //@todo This is temporary to control the daypart source for Broadcast
            var repoFactory = unityContainer.Resolve<IDataRepositoryFactory>();
            var daypartRepo = repoFactory.GetDataRepository<IDisplayDaypartRepository>();
            DaypartCache.DaypartCacheInstance = new DaypartCache(daypartRepo);
            unityContainer.RegisterInstance<IDaypartCache>(DaypartCache.Instance);

            MediaMonthCrunchCache.MediaMonthCrunchCacheInstance = new MediaMonthCrunchCache(repoFactory,unityContainer.Resolve<IMediaMonthAndWeekAggregateCache>());
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
