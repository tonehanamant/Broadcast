﻿using Common.Services.ApplicationServices;
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

            stationLetters = stationLetters.Split('-')[0].Trim();
            stationLetters = stationLetters.Split('+')[0].Trim();

            return stationLetters;
        }

        public DisplayBroadcastStation FindStation(string legacyCallLetters)
        {
            var station = Stations.SingleOrDefault(s => s.LegacyCallLetters.Equals(StripStationSuffix(legacyCallLetters), StringComparison.InvariantCultureIgnoreCase));
            return station;
        }

    }
}
