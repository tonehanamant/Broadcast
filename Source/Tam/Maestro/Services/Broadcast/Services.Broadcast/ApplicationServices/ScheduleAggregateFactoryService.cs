using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Aggregates;
using Services.Broadcast.Cache;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.ApplicationServices
{
    public interface IScheduleAggregateFactoryService : IApplicationService
    {
        SchedulesAggregate GetScheduleAggregate(int scheduleId);
    }

    public class ScheduleAggregateFactoryService : IScheduleAggregateFactoryService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IBroadcastAudiencesCache _AudienceCache;

        public ScheduleAggregateFactoryService(IDataRepositoryFactory broadcastDataRepositoryFactory, 
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBroadcastAudiencesCache audienceCache)
        {
            _AudienceCache = audienceCache;
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public SchedulesAggregate GetScheduleAggregate(int scheduleId)
        {
            var schedule = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetById(scheduleId);
            var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(schedule.start_date, schedule.end_date);
            var houseHoldAudienceId = _AudienceCache.GetDefaultAudience().Id;

            return _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleAggregateRepository>().Find(scheduleId, mediaWeeks, houseHoldAudienceId);
        }
        
    }
}
