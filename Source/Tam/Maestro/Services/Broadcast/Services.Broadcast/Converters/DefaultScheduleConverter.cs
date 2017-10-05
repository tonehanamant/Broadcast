using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters
{
    public interface IDefaultScheduleConverter : IApplicationService, IScheduleConverter
    {
    }

    public class DefaultScheduleConverter : ScheduleConverterBase, IDefaultScheduleConverter
    {
        public DefaultScheduleConverter(IDataRepositoryFactory broadcastDataRepositoryFactory, 
                                    IDaypartCache daypartCache, 
                                    IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                    IBroadcastAudiencesCache audienceCache)
                : base(broadcastDataRepositoryFactory,daypartCache,mediaMonthAndWeekAggregateCache,audienceCache)
        {
        }

        public override schedule Convert(ScheduleDTO scheduleDto)
        {
            var convertedSchedule = base._MapSchedule(scheduleDto);
            convertedSchedule.schedule_audiences = _MapAudiences(convertedSchedule.id, scheduleDto);
            return convertedSchedule;
        }

        // merges existing audiences with new ones -- population is kept and rank is determined by the order user selected on FE
        private List<schedule_audiences> _MapAudiences(int scheduleId, ScheduleDTO scheduleDto)
        {
            var audiences = new List<schedule_audiences>();
            var existingAudiences = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleAudiences(scheduleDto.Id);

            foreach (var demoAudience in scheduleDto.Audiences)
            {
                var audience = new schedule_audiences
                {
                    audience_id = demoAudience.AudienceId,
                    rank = demoAudience.Rank,
                    schedule_id = scheduleId
                };
                
                var existingAudience = existingAudiences.FirstOrDefault(e => e.audience_id == demoAudience.AudienceId);
                if (existingAudience != null)
                {
                    audience.population = existingAudience.population;
                }

                audiences.Add(audience);
            }

            return audiences;
        }
    }
}
