using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class StationsTestData
    {
        public static List<DisplayBroadcastStation> GetStations(int marketCount, int stationsPerMarket)
        {
            var availableStations = new List<DisplayBroadcastStation>();
            var stationId = 0;

            for (var i = 1; i <= marketCount; i++)
            {
                for (var j = 1; j <= stationsPerMarket; j++)
                {
                    var stationName = $"ST{(++stationId).ToString().PadLeft(2, '0')}";
                    availableStations.Add(new DisplayBroadcastStation { Id = stationId, LegacyCallLetters = stationName, MarketCode = j });
                }
            }

            return availableStations;
        }

        public static DisplayBroadcastStation GetStation(int stationId)
        {
            var result = _Stations.SingleOrDefault(s => s.Id == stationId);
            if (result == null)
            {
                result = _Stations.Last();
            }
            return result;
        }

        public static List<DisplayBroadcastStation> GetBroadcastStationsByIds(List<int> stationIds)
        {
            return _Stations.Where(s => stationIds.Contains(s.Id)).ToList();
        }

        private readonly static List<DisplayBroadcastStation> _Stations = new List<DisplayBroadcastStation>
        {
            new DisplayBroadcastStation
            {
                Id = 1,
                CallLetters = "KOB+",
                Affiliation = "NBC",
                MarketCode = 1,
                LegacyCallLetters = "KOB",
                Code = 101
            },
            new DisplayBroadcastStation
            {
                Id = 2,
                CallLetters = "KSTP+",
                Affiliation = "ABC",
                MarketCode = 2,
                LegacyCallLetters = "KSTP",
                Code = 102
            },
            new DisplayBroadcastStation
            {
                Id = 3,
                CallLetters = "WRGB+",
                Affiliation = "CBS",
                MarketCode = 3,
                LegacyCallLetters = "WRGB",
                Code = 103
            },
            new DisplayBroadcastStation
            {
                Id = 999,
                CallLetters = "WSAZ+",
                Affiliation = "NBC",
                MarketCode = 999,
                LegacyCallLetters = "WSAZ",
                Code = 999
            }
        };
    }
}
