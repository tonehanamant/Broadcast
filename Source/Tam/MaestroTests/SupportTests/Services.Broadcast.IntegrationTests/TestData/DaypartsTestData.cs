using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class DaypartsTestData
    {
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
    }
}
