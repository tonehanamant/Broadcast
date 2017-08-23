using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Aggregates;
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

        public ScheduleAggregateFactoryService(IDataRepositoryFactory broadcastDataRepositoryFactory, IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public SchedulesAggregate GetScheduleAggregate(int scheduleId)
        {
            var schedule = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetById(scheduleId);
            var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(schedule.start_date, schedule.end_date);

            return _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleAggregateRepository>().Find(scheduleId, mediaWeeks);
        }
        
    }
}
