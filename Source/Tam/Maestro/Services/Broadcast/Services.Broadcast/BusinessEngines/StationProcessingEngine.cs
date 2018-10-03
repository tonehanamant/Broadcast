using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IStationProcessingEngine: IApplicationService
    {
        string StripStationSuffix(string stationLetters);
    }
    public class StationProcessingEngine : IStationProcessingEngine
    {
        public List<DisplayBroadcastStation> Stations;
        public StationProcessingEngine() { }

        public string StripStationSuffix(string stationLetters)
        {
            const string plusS2Extension = "+s2";

            if (stationLetters.Contains("-"))
            {
                stationLetters = stationLetters.Substring(0, stationLetters.Length - stationLetters.LastIndexOf("-") + 1).Trim();
            }

            if (stationLetters.EndsWith(plusS2Extension, StringComparison.InvariantCultureIgnoreCase))
            {
                stationLetters = stationLetters.Substring(0, stationLetters.Length - plusS2Extension.Length).Trim();
            }

            return stationLetters;
        }

        public DisplayBroadcastStation FindStation(string legacyCallLetters)
        {
            var station = Stations.SingleOrDefault(s => s.LegacyCallLetters.Equals(StripStationSuffix(legacyCallLetters), StringComparison.InvariantCultureIgnoreCase));
            return station;
        }

    }
}
