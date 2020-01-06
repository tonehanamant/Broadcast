using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Microsoft.Practices.Unity;
using Services.Broadcast.BusinessEngines;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public class BroadcastDataDataRepositoryFactory : CommonRepositoryFactory, IDataRepositoryFactory
    {
        public static volatile bool WasRegistered = false;

        public override void RegisterTypes(UnityContainer instance)
        {
            if (WasRegistered)
            {
                return;
            }

            instance.RegisterType<ITransactionHelper, TransactionHelper>();

            instance.RegisterType<IBroadcastContextFactory, BroadcastContextFactory>();
            instance.RegisterType<IContextFactory<QueryHintBroadcastContext>, BroadcastContextFactory>();

            instance.RegisterType<IContextFactory<QueryHintBroadcastForecastContext>, BroadcastForecastContextFactory>();

            instance.RegisterInstance<ISMSClient>(SMSClient.Handler);
            instance.RegisterInstance<IConfigurationWebApiClient>(ConfigurationClientSwitch.Handler);
            instance.RegisterType<IDisplayDaypartRepository, DisplayDaypartBroadcastRepository>();
            instance.RegisterType<IAudienceRepository, AudienceRepository>();
            instance.RegisterType<IScheduleRepository, ScheduleRepository>();
            instance.RegisterType<IDetectionRepository, DetectionRepository>();
            instance.RegisterType<IPostingBookRepository, PostingBookRepository>();
            instance.RegisterType<IRatingsRepository, RatingsRepository>();
            instance.RegisterType<IDetectionPostDetailsRepository, DetectionPostDetailsRepository>();
            instance.RegisterType<ITrackerMappingRepository, TrackerMappingRepository>();
            instance.RegisterType<IStationRepository, StationRepository>();
            instance.RegisterType<ISpotLengthRepository, SpotLengthBroadcastRepository>();
            instance.RegisterType<IInventoryFileRepository, InventoryFileRepository>();
            instance.RegisterType<IGenreRepository, GenreRepository>();
            instance.RegisterType<IProgramNameRepository, ProgramNameRepository>();
            instance.RegisterType<IScheduleAggregateRepository, ScheduleAggregateRepository>();
            instance.RegisterType<IStationContactsRepository, StationContactsRepository>();
            instance.RegisterType<IProposalRepository, ProposalRepository>();
            instance.RegisterType<IProposalInventoryRepository, ProposalInventoryRepository>();
            instance.RegisterType<IProposalOpenMarketInventoryRepository, ProposalOpenMarketInventoryRepository>();
            instance.RegisterType<IDetectionTestDataGeneratorRepository, DetectionTestDataGeneratorRepository>();
            instance.RegisterType<INsiUniverseRepository, NsiUniverseRepository>();
            instance.RegisterType<INsiMarketRepository, NsiMarketRepository>();
            instance.RegisterType<IRatingForecastRepository, RatingForecastRepository>();
            instance.RegisterType<IMarketRepository, MarketRepository>();
            instance.RegisterType<IMarketCoverageRepository, MarketCoverageRepository>();
            instance.RegisterType<IMarketDmaMapRepository, MarketDmaMapRepository>();
            instance.RegisterType<IBroadcastAudienceRepository, BroadcastAudienceRepository>();
            instance.RegisterType<IRatingAdjustmentsRepository, RatingAdjustmentsRepository>();
            instance.RegisterType<IPostPrePostingRepository, PostPrePostingRepository>();
            instance.RegisterType<ISpotLengthRepository, SpotLengthBroadcastRepository>();
            instance.RegisterType<IProposalProgramsCriteriaRepository, ProposalProgramsCriteriaRepository>();
            instance.RegisterType<ITrafficRepository, TrafficRepository>();
            instance.RegisterType<IStationProgramRepository, StationProgramRepository>();
            instance.RegisterType<IShowTypeRepository, ShowTypeRepository>();
            instance.RegisterType<INtiTransmittalsRepository, NtiTransmittalsRepository>();
            instance.RegisterType<IPricingGuideRepository, PricingGuideRepository>();
            instance.RegisterType<IMediaMonthAndWeekAggregateRepository, MediaMonthAndWeekAggregateAndWeekAggregateRepository>();
            instance.RegisterType<IMediaMonthAndWeekAggregateCache, MediaMonthAndWeekAggregateCache>();
            instance.RegisterType<IDisplayDaypartRepository, DisplayDaypartBroadcastRepository>();
            instance.RegisterType<IInventoryRepository, InventoryRepository>();
            instance.RegisterType<IInventoryFileRatingsJobsRepository, InventoryFileRatingsJobsRepository>();
            instance.RegisterType<IAffidavitRepository, AffidavitRepository>();
            instance.RegisterType<INsiComponentAudienceRepository, NsiComponentAudienceRepository>();
            instance.RegisterType<IPostRepository, PostRepository>();
            instance.RegisterType<IIsciRepository, IsciRepository>();
            instance.RegisterType<IPostLogRepository, PostLogRepository>();
            instance.RegisterType<IProposalBuyRepository, ProposalBuyRepository>();
            instance.RegisterType<ISpotTrackerRepository, SpotTrackerRepository>();
            instance.RegisterType<IStationProcessingEngine, StationProcessingEngine>();
            instance.RegisterType<IProprietaryRepository, ProprietaryInventoryRepository>();
            instance.RegisterType<IDaypartDefaultRepository, DaypartDefaultRepository>();
            instance.RegisterType<IInventorySummaryRepository, InventorySummaryRepository>();
            instance.RegisterType<IProgramRepository, ProgramRepository>();
            instance.RegisterType<ICampaignRepository, CampaignRepository>();
            instance.RegisterType<IInventoryLogoRepository, InventoryLogoRepository>();
            instance.RegisterType<IScxGenerationJobRepository, ScxGenerationJobRepository>();
            instance.RegisterType<IPlanRepository, PlanRepository>();
            instance.RegisterType<IPlanSummaryRepository, PlanSummaryRepository>();
            instance.RegisterType<ICampaignSummaryRepository, CampaignSummaryRepository>();
            instance.RegisterType<IInventoryFileProgramEnrichmentJobsRepository, InventoryFileProgramEnrichmentJobsRepository>();
            instance.RegisterType<IAffiliateRepository, AffiliateRepository>();

            WasRegistered = true;
        }
    }
}
