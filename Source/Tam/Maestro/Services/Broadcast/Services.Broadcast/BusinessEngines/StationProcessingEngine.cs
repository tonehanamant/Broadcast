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
            if (stationLetters.Contains("-"))
            {
                return stationLetters.Substring(0, stationLetters.Length - stationLetters.LastIndexOf("-") + 1);
            }
            return stationLetters;
        }

    }
}
