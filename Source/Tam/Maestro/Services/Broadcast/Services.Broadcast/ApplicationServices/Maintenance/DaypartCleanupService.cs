using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices.Maintenance
{
    public interface IDaypartCleanupService: IApplicationService
    {
        /// <summary>
        /// Calculates the daypart text.
        /// </summary>
        /// <param name="daypartDays">The daypart days.</param>
        /// <returns>The daypart text</returns>
        string CalculateDaypartText(List<int> daypartDays);

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
                var expectedDaypartDaysText = CalculateDaypartText(dp.Days);

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
        public string CalculateDaypartText(List<int> daypartDays)
        {
            var daysOfWeek = new List<string> { "M", "TU", "W", "TH", "F", "SA", "SU" };
            var daypartString = string.Empty;

            //construct the daypart days list
            for (int i = 0; i < daysOfWeek.Count; i++)
            {
                if (!daypartDays.Contains(i + 1))
                {
                    daysOfWeek[i] = null;
                }
            }

            //group the daypart days that are not empty
            var groupOfDaypartDays = daysOfWeek.GroupConnected((a) => string.IsNullOrWhiteSpace(a));
            var daypartDaysList = new List<string>();
            foreach (var group in groupOfDaypartDays.Where(x => x.Count() > 0))
            {
                //if the group contains 1 element, join by comma
                if (group.Count() == 1)
                {
                    daypartDaysList.Add(string.Join(",", group));
                }
                else  //if the group contains more then 2 elements, join the first and the last one with "-"
                {
                    daypartDaysList.Add($"{group.First()}-{group.Last()}");
                }
            }

            daypartString = string.Join(",", daypartDaysList);
            //number of active days this week is 7 minus number of hiatus days
            return daypartString;
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
