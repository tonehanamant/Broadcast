using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Microsoft.Practices.Unity;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using Tam.Maestro.Data.EntityFrameworkMapping.ExternalRating;
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
            
            instance.RegisterType<IExternalRatingContextFactory, ExternalRatingContextFactory>();
            instance.RegisterType<IContextFactory<QueryHintExternalRatingContext>, ExternalRatingContextFactory>();

            instance.RegisterType<IContextFactory<QueryHintBroadcastForecastContext>, BroadcastForecastContextFactory>();

            instance.RegisterInstance<ISMSClient>(SMSClient.Handler);
            instance.RegisterType<IDisplayDaypartRepository, DisplayDaypartBroadcastRepository>();
            instance.RegisterType<IAudienceRepository, AudienceRepository>();
            instance.RegisterType<IScheduleRepository, ScheduleRepository>();
            instance.RegisterType<IBvsRepository, BvsRepository>();
            instance.RegisterType<IPostingBookRepository, PostingBookRepository>();
            instance.RegisterType<IRatingsRepository, RatingsRepository>();
            instance.RegisterType<IBvsPostDetailsRepository, BvsPostDetailsRepository>();
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
            instance.RegisterType<IBvsTestDataGeneratorRepository, BvsTestDataGeneratorRepository>();
            instance.RegisterType<INsiUniverseRepository, NsiUniverseRepository>();
            instance.RegisterType<INsiMarketRepository, NsiMarketRepository>();
            instance.RegisterType<IRatingForecastRepository, RatingForecastRepository>();
            instance.RegisterType<IMarketRepository, MarketRepository>();
            instance.RegisterType<IMarketDmaMapRepository, MarketDmaMapRepository>();
            instance.RegisterType<IBroadcastAudienceRepository, BroadcastAudienceRepository>();
            instance.RegisterType<IRatingAdjustmentsRepository, RatingAdjustmentsRepository>();
            instance.RegisterType<IPostPrePostingRepository, PostPrePostingRepository>();
            instance.RegisterType<ISpotLengthRepository, SpotLengthBroadcastRepository>();
            instance.RegisterType<IProposalProgramsCriteriaRepository, ProposalProgramsCriteriaRepository>();
            instance.RegisterType<ITrafficRepository, TrafficRepository>();
            instance.RegisterType<IStationProgramRepository, StationProgramRepository>();
            instance.RegisterType<IShowTypeRepository, ShowTypeRepository>();

            instance.RegisterType<IMediaMonthAndWeekAggregateRepository, MediaMonthAndWeekAggregateAndWeekAggregateRepository>();
            instance.RegisterType<IMediaMonthAndWeekAggregateCache, MediaMonthAndWeekAggregateCache>();
            instance.RegisterType<IDisplayDaypartRepository, DisplayDaypartBroadcastRepository>();
            instance.RegisterType<IInventoryRepository, InventoryRepository>();
            instance.RegisterType<IAffidavitRepository, AffidavitRepository>();
            instance.RegisterType<INsiComponentAudienceRepository, NsiComponentAudienceRepository>();
            instance.RegisterType<IPostRepository, PostRepository>();
            instance.RegisterType<IPostLogRepository, PostLogRepository>();
            instance.RegisterType<ISpotTrackerRepository, SpotTrackerRepository>();

            WasRegistered = true;
        }
    }
}
