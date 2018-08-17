using Common.Services.ApplicationServices;

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
