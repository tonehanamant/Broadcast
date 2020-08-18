using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using System.Linq;
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class DaypartsTestData
    {
        public static Dictionary<int, DisplayDaypart> GetAllDisplayDayparts()
        {
            var dict = _DisplayDayparts.ToDictionary(s => s.Id);
            return dict;
        }

        public static DisplayDaypart GetDisplayDaypart(int daypartId)
        {
            // returning a default per previous behaviour
            const int defaultKey = 999;
            var dict = GetAllDisplayDayparts();
            if (dict.ContainsKey(daypartId))
            {
                return dict[daypartId];
            }
            return dict[defaultKey];
        }

        public static List<DaypartDefaultFullDto> GetAllDaypartDefaultsWithFullData()
        {
            return _AllDaypartDefaultsWithFullData;
        }

        public static List<DaypartDefaultDto> GetAllDaypartDefaultsWithBaseData()
        {
            return _AllDaypartDefaultsWithFullData.ToList<DaypartDefaultDto>();
        }

        public static Dictionary<int, List<int>> GetDayIdsFromDaypartDefaults()
        {
            var weekendCodes = new List<string> { "WKD" };
            var dayIdDict = new Dictionary<int, List<int>>();
            var full = GetAllDaypartDefaultsWithFullData();
            full.ForEach(d =>
            {
                dayIdDict[d.Id] = weekendCodes.Contains(d.Code)
                    ? new List<int> { 6, 7 }
                    : new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            });
            return dayIdDict;
        }

        #region Lists Data

        private static List<DisplayDaypart> _DisplayDayparts = new List<DisplayDaypart>
        {
            new DisplayDaypart
            {
                Id = 1,
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = false,
                Sunday = false,
                StartTime = 3600, // 1am
                EndTime = 7199 // 2am
            },
            new DisplayDaypart
            {
                Id = 2,
                Monday = false,
                Tuesday = false,
                Wednesday = false,
                Thursday = false,
                Friday = false,
                Saturday = true,
                Sunday = true,
                StartTime = 46800, // 1pm
                EndTime = 53999 // 3pm
            },
            new DisplayDaypart
            {
                Id = 3,
                Monday = true,
                Tuesday = false,
                Wednesday = true,
                Thursday = false,
                Friday = true,
                Saturday = false,
                Sunday = true,
                StartTime = 72000, // 8pm
                EndTime = 79199 // 10pm
            },
            new DisplayDaypart
            {
                Id = 4,
                Monday = true,
                Tuesday = false,
                Wednesday = true,
                Thursday = false,
                Friday = true,
                Saturday = false,
                Sunday = true,
                StartTime = 79200, // 10pm
                EndTime = 82799 // 11pm
            },
            new DisplayDaypart
            {
                Id = 5,
                Monday = true,
                Tuesday = false,
                Wednesday = false,
                Thursday = false,
                Friday = false,
                Saturday = false,
                Sunday = false,
                StartTime = 36000, // 10am
                EndTime = 39599 // 11am
            },
            new DisplayDaypart
            {
                Id = 999,
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = true,
                Sunday = true,
                StartTime = 32400, // 9am
                EndTime = 39599 // 11am
            }
        };

        private static List<DaypartDefaultFullDto> _AllDaypartDefaultsWithFullData = new List<DaypartDefaultFullDto>
        {
            new DaypartDefaultFullDto  { Id = 1, Code = "EMN", FullName = "Early Morning News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)1, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 35999},
            new DaypartDefaultFullDto  { Id = 2, Code = "MDN", FullName = "Midday News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)1, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 39600, DefaultEndTimeSeconds = 46799},
            new DaypartDefaultFullDto  { Id = 3, Code = "EN", FullName = "Evening News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)2, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 57600, DefaultEndTimeSeconds = 68399},
            new DaypartDefaultFullDto  { Id = 4, Code = "LN", FullName = "Late News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)2, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 72000, DefaultEndTimeSeconds = 299},
            new DaypartDefaultFullDto  { Id = 5, Code = "ENLN", FullName = "Evening News/Late News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)2, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 57600, DefaultEndTimeSeconds = 299},
            new DaypartDefaultFullDto  { Id = 6, Code = "EF", FullName = "Early Fringe", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 54000, DefaultEndTimeSeconds = 64799},
            new DaypartDefaultFullDto  { Id = 7, Code = "PA", FullName = "Prime Access", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 64800, DefaultEndTimeSeconds = 71999},
            new DaypartDefaultFullDto  { Id = 8, Code = "PT", FullName = "Prime", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 72000, DefaultEndTimeSeconds = 82799},
            new DaypartDefaultFullDto  { Id = 9, Code = "LF", FullName = "Late Fringe", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 82800, DefaultEndTimeSeconds = 7199},
            new DaypartDefaultFullDto  { Id = 10, Code = "SYN", FullName = "Total Day Syndication", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 21600, DefaultEndTimeSeconds = 7499},
            new DaypartDefaultFullDto  { Id = 11, Code = "OVN", FullName = "Overnights", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 7200, DefaultEndTimeSeconds = 21599},
            new DaypartDefaultFullDto  { Id = 12, Code = "DAY", FullName = "Daytime", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 32400, DefaultEndTimeSeconds = 57599},
            new DaypartDefaultFullDto  { Id = 14, Code = "EM", FullName = "Early morning", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 21600, DefaultEndTimeSeconds = 32399},
            new DaypartDefaultFullDto  { Id = 15, Code = "AMN", FullName = "AM News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)1, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 46799},
            new DaypartDefaultFullDto  { Id = 16, Code = "PMN", FullName = "PM News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)2, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 46800, DefaultEndTimeSeconds = 299},
            new DaypartDefaultFullDto  { Id = 17, Code = "TDN", FullName = "Total Day News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)4, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 299},
            new DaypartDefaultFullDto  { Id = 19, Code = "ROSS", FullName = "ROS Syndication", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 21600, DefaultEndTimeSeconds = 7499},
            new DaypartDefaultFullDto  { Id = 20, Code = "SPORTS", FullName = "ROS Sports", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 21600, DefaultEndTimeSeconds = 7499},
            new DaypartDefaultFullDto  { Id = 21, Code = "ROSP", FullName = "ROS Programming", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 21600, DefaultEndTimeSeconds = 7499},
            new DaypartDefaultFullDto  { Id = 22, Code = "TDNS", FullName = "Total Day News and Syndication", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)5, DaypartType = (DaypartTypeEnum)3, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 7499},
            new DaypartDefaultFullDto  { Id = 23, Code = "WKD", FullName = "Weekend", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 32400, DefaultEndTimeSeconds = 71999},
        };

        #endregion // #region Lists Data
    }
}
