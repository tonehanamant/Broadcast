using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines
{
    public interface ISpotCostCalculationEngine
    {
        void CalculateSpotCost(RatesSaveRequest request, RatesFile ratesFile);
    }

    public class SpotCostCalculationEngine : ISpotCostCalculationEngine
    {
        private readonly IRatingForecastRepository _RatingsRepository;

        public SpotCostCalculationEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _RatingsRepository = dataRepositoryFactory.GetDataRepository<IRatingForecastRepository>(); ;
        }

        public void CalculateSpotCost(RatesSaveRequest request, RatesFile ratesFile)
        {
            var audiences = (from stationProgram in ratesFile.StationPrograms
                             from stationProgramFlightWeek in stationProgram.FlightWeeks
                             from stationProgramFlightWeekAudience in stationProgramFlightWeek.Audiences
                             select stationProgramFlightWeekAudience.Audience.Id).Distinct().ToList();

            var stations = ratesFile.StationPrograms.GroupBy(g => g.StationCode).Select(stationProgram => new StationDetailDaypart
            {
                Code = stationProgram.First().StationCode,
                DisplayDaypart = stationProgram.First().Daypart,
                Id = stationProgram.First().Id
            }).ToList();

            var stationsImpressions = _RatingsRepository.GetImpressionsDaypart(request.RatingBook, audiences, stations, request.PlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

            foreach (var stationProgram in ratesFile.StationPrograms)
            {
                foreach (var stationProgramFlightWeek in stationProgram.FlightWeeks)
                {
                    var firstAudience = stationProgramFlightWeek.Audiences.FirstOrDefault();

                    if (firstAudience == null)
                        continue;

                    var stationImpressions =
                        stationsImpressions.FirstOrDefault(
                            stationImpression => stationImpression.station_code == stationProgram.StationCode &&
                                                 stationImpression.audience_id == firstAudience.Audience.Id);

                    if (stationImpressions == null)
                        continue;

                    SetSpotCost(stationImpressions, firstAudience, stationProgramFlightWeek);
                }
            }
        }

        private void SetSpotCost(StationImpressionsWithAudience stationImpressionsWithAudience, StationProgramFlightWeekAudience audience, StationProgramFlightWeek stationProgramFlightWeek)
        {
            if (audience.Cpm15 != null)
            {
                stationProgramFlightWeek.Rate15s = CalculateSpotCostValue(stationImpressionsWithAudience.impressions, audience.Cpm15.Value);
            }
            else if (audience.Cpm30 != null)
            {
                stationProgramFlightWeek.Rate30s = CalculateSpotCostValue(stationImpressionsWithAudience.impressions, audience.Cpm30.Value);
            }
            else if (audience.Cpm60 != null)
            {
                stationProgramFlightWeek.Rate60s = CalculateSpotCostValue(stationImpressionsWithAudience.impressions, audience.Cpm60.Value);
            }
            else if (audience.Cpm90 != null)
            {
                stationProgramFlightWeek.Rate90s = CalculateSpotCostValue(stationImpressionsWithAudience.impressions, audience.Cpm90.Value);
            }
            else if (audience.Cpm120 != null)
            {
                stationProgramFlightWeek.Rate120s = CalculateSpotCostValue(stationImpressionsWithAudience.impressions, audience.Cpm120.Value);
            }
        }

        private decimal CalculateSpotCostValue(double viewers, decimal cpm)
        {
            return (decimal)viewers * cpm / 1000;
        }
    }
}
