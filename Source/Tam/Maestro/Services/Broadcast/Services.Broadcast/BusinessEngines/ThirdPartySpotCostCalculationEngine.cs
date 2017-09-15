using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines
{
    public interface IThirdPartySpotCostCalculationEngine
    {
        void CalculateSpotCost(RatesSaveRequest request, RatesFile ratesFile);
    }

    public class ThirdPartySpotCostCalculationEngine : IThirdPartySpotCostCalculationEngine
    {
        private readonly IRatingForecastRepository _RatingsRepository;

        public ThirdPartySpotCostCalculationEngine(IDataRepositoryFactory dataRepositoryFactory)
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

            var stationsImpressions = _RatingsRepository.GetImpressionsDaypart(request.RatingBook.Value, audiences, stations, request.PlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

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
                stationProgramFlightWeek.Rate15s = ProposalMath.CalculateCost(audience.Cpm15.Value, stationImpressionsWithAudience.impressions);
            }
            else if (audience.Cpm30 != null)
            {
                stationProgramFlightWeek.Rate30s = ProposalMath.CalculateCost(audience.Cpm30.Value, stationImpressionsWithAudience.impressions);
            }
            else if (audience.Cpm60 != null)
            {
                stationProgramFlightWeek.Rate60s = ProposalMath.CalculateCost(audience.Cpm60.Value, stationImpressionsWithAudience.impressions);
            }
            else if (audience.Cpm90 != null)
            {
                stationProgramFlightWeek.Rate90s = ProposalMath.CalculateCost(audience.Cpm90.Value, stationImpressionsWithAudience.impressions);
            }
            else if (audience.Cpm120 != null)
            {
                stationProgramFlightWeek.Rate120s = ProposalMath.CalculateCost(audience.Cpm120.Value, stationImpressionsWithAudience.impressions);
            }
        }
    }
}
