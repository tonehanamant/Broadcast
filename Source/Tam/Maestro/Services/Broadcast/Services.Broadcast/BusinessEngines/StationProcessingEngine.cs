using Common.Services.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.BusinessEngines
{
    public interface IStationProcessingEngine: IApplicationService
    {
        string StripStationSuffix(string stationLetters);
    }
    public class StationProcessingEngine : IStationProcessingEngine
    {
        public string StripStationSuffix(string stationLetters)
        {
            var result = stationLetters.Split('-')[0];
            return result;
        }

    }
}
