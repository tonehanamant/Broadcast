using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Services.Broadcast.Entities.StationInventory;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProprietarySpotCostCalculationEngine
    {
        void CalculateSpotCost(InventoryFileSaveRequest request, InventoryFile inventoryFile);
    }

    public class ProprietarySpotCostCalculationEngine : IProprietarySpotCostCalculationEngine
    {
        private readonly IRatingForecastRepository _RatingsRepository;

        public ProprietarySpotCostCalculationEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _RatingsRepository = dataRepositoryFactory.GetDataRepository<IRatingForecastRepository>(); ;
        }

        public void CalculateSpotCost(InventoryFileSaveRequest request, InventoryFile inventoryFile)
        {
            var audiences = (from groups in inventoryFile.InventoryGroups
                             from manifest in groups.Manifests
                             from audience in manifest.ManifestAudiences
                             select audience.Audience.Id).Distinct().ToList();

            var manifestList = inventoryFile.InventoryGroups
                .SelectMany(g => g.Manifests)
                .ToList();

            // flatten out manifest dayparts with unique Ids for each record.
            var manifestDaypartMap = 
                (
                    from manifest in manifestList
                    from manifestDaypart in manifest.ManifestDayparts
                    select new {id = manifestDaypart.Daypart.Id, manifest = manifest,daypart = manifestDaypart.Daypart,}
                ).ToList();

            // group stations and dayparts (to make them distint)
            var stationDaypartGroups =
            (
                from mdpMap in manifestDaypartMap
                select new ManifestDetailDaypart()
                {
                    LegacyCallLetters= mdpMap.manifest.Station.LegacyCallLetters,
                    DisplayDaypart = mdpMap.daypart,
                    Id = mdpMap.id
                }
            ).GroupBy(g => new {g.LegacyCallLetters, g.Id }).ToList();
            
            var stationsImpressions = _RatingsRepository.GetImpressionsDaypart(request.RatingBook.Value, audiences, stationDaypartGroups.Select(g => g.First()).ToList(), request.PlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

            foreach (var manifest in manifestList)
            {
                var firstAudience = manifest.ManifestAudiences.FirstOrDefault();

                if (firstAudience == null)
                    continue;

                var stationImpressions = stationsImpressions
                                            .Where(si => si.legacy_call_letters == manifest.Station.LegacyCallLetters 
                                                            && firstAudience.Audience.Id == si.audience_id 
                                                            && manifestDaypartMap
                                                                    .Any(md => md.id == si.id 
                                                                                &&  manifest.ManifestDayparts.Any(mdp => mdp.Daypart.Id == md.daypart.Id ))
                                                            ).ToList();
                if (!stationImpressions.Any()) continue;

                var impression = stationImpressions.Average(si => si.impressions);

                var rate = manifest.ManifestRates.FirstOrDefault(r => r.SpotLengthId == manifest.SpotLengthId);
                if (rate == null)
                {
                    rate = new StationInventoryManifestRate() {SpotLengthId = manifest.SpotLengthId};
                    manifest.ManifestRates.Add(rate);
                }
                rate.Rate = ProposalMath.CalculateCost(firstAudience.Rate, impression);
            }
        }
    }
}
