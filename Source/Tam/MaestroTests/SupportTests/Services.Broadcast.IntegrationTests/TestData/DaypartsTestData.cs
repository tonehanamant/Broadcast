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

        public static List<StandardDaypartFullDto> GetAllStandardDaypartsWithFullData()
        {
            return _AllStandardDaypartsWithFullData;
        }

        public static List<StandardDaypartDto> GetAllStandardDaypartsWithBaseData()
        {
            return _AllStandardDaypartsWithFullData.ToList<StandardDaypartDto>();
        }

        public static Dictionary<int, List<int>> GetDayIdsFromStandardDayparts()
        {
            var weekendCodes = new List<string> { "WKD" };
            var dayIdDict = new Dictionary<int, List<int>>();
            var full = GetAllStandardDaypartsWithFullData();
            full.ForEach(d =>
            {
                dayIdDict[d.Id] = weekendCodes.Contains(d.Code)
                    ? new List<int> { 6, 7 }
                    : new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            });
            return dayIdDict;
        }

        public static Dictionary<int, int> GetStandardDaypartIdDaypartIds()
        {
            return _StandardDaypartIdDaypartIds;
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
            },
            new DisplayDaypart
            {
                Id = 23, // WKD
                Monday = false,
                Tuesday = false,
                Wednesday = false,
                Thursday = false,
                Friday = false,
                Saturday = true,
                Sunday = true,
                StartTime = 32400, // 9am
                EndTime = 71999 // 8pm
            },
            new DisplayDaypart
            {
                Id = 24,
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = true,
                Sunday = true,
                StartTime = 14400, // 4am
                EndTime = 7199 // 2am
            },
        };

        private static List<StandardDaypartFullDto> _AllStandardDaypartsWithFullData = new List<StandardDaypartFullDto>
        {
            new StandardDaypartFullDto  { Id = 1, Code = "EMN", FullName = "Early Morning News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)1, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 35999,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 2, Code = "MDN", FullName = "Midday News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)1, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 39600, DefaultEndTimeSeconds = 46799,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 3, Code = "EN", FullName = "Evening News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)2, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 57600, DefaultEndTimeSeconds = 71999,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 4, Code = "LN", FullName = "Late News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)2, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 72000, DefaultEndTimeSeconds = 299,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 5, Code = "ENLN", FullName = "Evening News/Late News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)2, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 57600, DefaultEndTimeSeconds = 299,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 6, Code = "EF", FullName = "Early Fringe", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 54000, DefaultEndTimeSeconds = 64799,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 7, Code = "PA", FullName = "Prime Access", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 64800, DefaultEndTimeSeconds = 71999,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 8, Code = "PT", FullName = "Prime", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 72000, DefaultEndTimeSeconds = 82799,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 9, Code = "LF", FullName = "Late Fringe", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 82800, DefaultEndTimeSeconds = 7199,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 10, Code = "SYN", FullName = "Total Day Syndication", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 21600, DefaultEndTimeSeconds = 7499,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 11, Code = "OVN", FullName = "Overnights", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 7200, DefaultEndTimeSeconds = 21599,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 12, Code = "DAY", FullName = "Daytime", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 32400, DefaultEndTimeSeconds = 57599,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 14, Code = "EM", FullName = "Early morning", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 35999,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 15, Code = "AMN", FullName = "AM News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)1, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 46799,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 16, Code = "PMN", FullName = "PM News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)2, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 57600, DefaultEndTimeSeconds = 299,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 17, Code = "TDN", FullName = "Total Day News", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)4, DaypartType = (DaypartTypeEnum)1, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 299,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 19, Code = "ROSS", FullName = "ROS Syndication", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 21600, DefaultEndTimeSeconds = 7499,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 20, Code = "SPORTS", FullName = "ROS Sports", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 21600, DefaultEndTimeSeconds = 7499,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 21, Code = "ROSP", FullName = "ROS Programming", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 21600, DefaultEndTimeSeconds = 7499,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 22, Code = "TDNS", FullName = "Total Day News and Syndication", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)5, DaypartType = (DaypartTypeEnum)3, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 7499,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 23, Code = "WKD", FullName = "Weekend", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)3, DaypartType = (DaypartTypeEnum)2, DefaultStartTimeSeconds = 32400, DefaultEndTimeSeconds = 71999,
                Monday = false, Tuesday = false, Wednesday = false, Thursday = false, Friday = false, Saturday = true, Sunday = true},
            new StandardDaypartFullDto  { Id = 24, Code = "CSP", FullName = "Custom Sports", VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)1, DaypartType = (DaypartTypeEnum)4, DefaultStartTimeSeconds = 14400, DefaultEndTimeSeconds = 7199,
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true}
        };

        private static Dictionary<int, int> _StandardDaypartIdDaypartIds = new Dictionary<int, int>
        {
            {1,59803},
            {2,42219},
            {3,107},
            {4,75644},
            {5,70923},
            {6,101},
            {7,19},
            {8,33},
            {9,6029},
            {10,73039},
            {11,1583},
            {12,9},
            {14,1441},
            {15,70922},
            {16,75643},
            {17,73038},
            {19,73039},
            {20,73039},
            {21,73039},
            {22,73207},
            {23,1546},
            {24,76379}
        };

        #endregion // #region Lists Data
    }
}
