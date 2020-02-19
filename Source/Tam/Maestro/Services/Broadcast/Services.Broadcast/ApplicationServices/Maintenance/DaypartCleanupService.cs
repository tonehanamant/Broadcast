using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices.Maintenance
{
    public interface IDaypartCleanupService: IApplicationService
    {
        /// <summary>
        /// Calculates the daypart days by the daypart text.
        /// </summary>
        /// <param name="daypartText">The daypart text.</param>
        /// <returns></returns>
        List<int> CalculateDaypartDaysByText(string daypartText);

        /// <summary>
        /// Filters the erroneous dayparts.
        /// </summary>
        /// <returns></returns>
        List<DaypartCleanupDto> FilterErroneousDayparts();

        /// <summary>
        /// Repairs the erroneous dayparts.
        /// </summary>
        /// <returns></returns>
        List<DaypartCleanupDto> RepairErroneousDayparts();
    }

    public class DaypartCleanupService: IDaypartCleanupService
    {
        private readonly IDisplayDaypartRepository _DisplayDaypartRepository;        

        public DaypartCleanupService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _DisplayDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IDisplayDaypartRepository>();
        }

        /// <inheritdoc />
        public List<DaypartCleanupDto> FilterErroneousDayparts()
        {
            var allDayparts = _DisplayDaypartRepository.GetAllDaypartsIncludeDays();
            var erroneousDayparts = new List<DaypartCleanupDto>();            

            allDayparts.ForEach(dp => {
                var daypartDayText = dp.Text.Substring(0, dp.Text.IndexOf(' '));
                var expectedDaypartDaysText = GroupHelper.GroupWeekDays(dp.Days);

                if (daypartDayText != expectedDaypartDaysText)
                {
                    erroneousDayparts.Add(dp);
                }
            });

            return erroneousDayparts;
        }

        /// <inheritdoc />
        public List<DaypartCleanupDto> RepairErroneousDayparts()
        {
            var erroneousDayparts = FilterErroneousDayparts();
            var repairedDayparts = new List<DaypartCleanupDto>();

            foreach (var daypart in erroneousDayparts)
            {
                daypart.Days = CalculateDaypartDaysByText(daypart.Text.Substring(0, daypart.Text.IndexOf(' ')));
                var repairedDaypart = _DisplayDaypartRepository.UpdateDaysForDayparts(daypart);
                repairedDayparts.Add(repairedDaypart);
            }

            return repairedDayparts;
        }

        /// <inheritdoc />
        public string CalculateDaypartTimespanText(int startTime, int endTime)
        {
            if (startTime < 0 || endTime < 0)
            {
                return string.Empty;
            }

            if (startTime == 0 && endTime == 86400)
            {
                return "24HR";
            }

            var startDateTime = new DateTime().Add(TimeSpan.FromSeconds(startTime));
            var formattedStartTime = startDateTime.Minute > 0 ? startDateTime.ToString("h:mmtt") : startDateTime.ToString("htt");

            var endDateTime = new DateTime().Add(TimeSpan.FromSeconds(endTime));
            var formattedEndTime = endDateTime.Minute > 0 ? endDateTime.ToString("h:mmtt") : endDateTime.ToString("htt");

            return $"{formattedStartTime}-{formattedEndTime}";
        }
        
        /// <inheritdoc />
        public List<int> CalculateDaypartDaysByText(string daypartText)
        {
            var daysOfWeek = new List<string> { "M", "TU", "W", "TH", "F", "SA", "SU" };
            var daypartDays = new List<int>();

            var daypartGroups = daypartText.Split(',');
            foreach (var daypartGroup in daypartGroups)
            {
                if (!daypartGroup.Contains('-'))
                {
                    daypartDays.Add(daysOfWeek.IndexOf(daypartGroup) + 1);
                }
                else
                {
                    var daypartGroupExtremities = daypartGroup.Split(new char[] { '-' }, 2);
                    for (int i = daysOfWeek.IndexOf(daypartGroupExtremities[0]); i <= daysOfWeek.IndexOf(daypartGroupExtremities[1]); i++)
                    {
                        daypartDays.Add(i + 1);
                    }
                }
            }

            return daypartDays;
        }
    }
}
