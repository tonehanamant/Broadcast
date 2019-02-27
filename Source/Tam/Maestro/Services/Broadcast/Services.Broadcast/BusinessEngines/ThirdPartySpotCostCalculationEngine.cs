using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Services.Broadcast.Entities.StationInventory;
using static Services.Broadcast.Entities.Enums.ProposalEnums;
using System.Collections.Generic;
using Common.Services.ApplicationServices;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProprietarySpotCostCalculationEngine : IApplicationService
    {
        void CalculateSpotCost(IEnumerable<StationInventoryManifest> manifests, ProposalPlaybackType? playbackType, int shareBook, int? hutBook = null);
    }

    public class ProprietarySpotCostCalculationEngine : IProprietarySpotCostCalculationEngine
    {
        private readonly IRatingForecastRepository _RatingsRepository;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;

        public ProprietarySpotCostCalculationEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _RatingsRepository = dataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            _BroadcastAudienceRepository = dataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
        }

        public void CalculateSpotCost(IEnumerable<StationInventoryManifest> manifests, ProposalPlaybackType? playbackType, int shareBook, int? hutBook)
        {
            var audiences = manifests
                .SelectMany(x => x.ManifestAudiences)
                .Select(x => x.Audience.Id)
                .Distinct()
                .ToList();

            var audiencesMappings = _BroadcastAudienceRepository.GetRatingsAudiencesByMaestroAudience(audiences).ToList();

            // flatten out manifest dayparts with unique Ids for each record.
            var manifestDaypartMap = 
                (
                    from manifest in manifests
                    from manifestDaypart in manifest.ManifestDayparts
                    select new { id = manifestDaypart.Daypart.Id, manifest, daypart = manifestDaypart.Daypart }
                ).ToList();

            // group stations and dayparts (to make them distint)
            var stationDaypartGroups =
            (
                from mdpMap in manifestDaypartMap
                select new ManifestDetailDaypart()
                {
                    LegacyCallLetters = mdpMap.manifest.Station.LegacyCallLetters,
                    DisplayDaypart = mdpMap.daypart,
                    Id = mdpMap.id
                }
            ).GroupBy(g => new { g.LegacyCallLetters, g.Id }).ToList();

            // Fix in future stories
            var stationDetails = stationDaypartGroups.Select(g => g.First()).ToList();
            
            foreach (var manifest in manifests)
            {
                var firstAudience = manifest.ManifestAudiences.FirstOrDefault();

                if (firstAudience == null)
                    continue;

                var ratingAudiences = audiencesMappings
                    .Where(a => a.custom_audience_id == firstAudience.Audience.Id)
                    .Select(a => a.rating_audience_id)
                    .Distinct()
                    .ToList();

                List<StationImpressions> stationImpressions = null;

                if (hutBook.HasValue)
                {
                    stationImpressions = _RatingsRepository
                        .GetImpressionsDaypart((short)hutBook, (short)shareBook, ratingAudiences, stationDetails, playbackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);
                }
                else
                {
                    stationImpressions = _RatingsRepository
                        .GetImpressionsDaypart(shareBook, ratingAudiences, stationDetails, playbackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions)
                        .Select(x => (StationImpressions)x)
                        .ToList();
                }

                stationImpressions = stationImpressions
                                            .Where(si => si.legacy_call_letters == manifest.Station.LegacyCallLetters &&
                                                         manifest.ManifestDayparts.Any(mdp => mdp.Daypart.Id == si.id))
                                            .ToList();

                if (!stationImpressions.Any())
                    continue;

                var impressions = stationImpressions
                    .GroupBy(x => x.id)
                    .Select(g => g.Sum(x => x.impressions))
                    .Average();

                var rate = manifest.ManifestRates.FirstOrDefault(r => r.SpotLengthId == manifest.SpotLengthId);

                if (rate == null)
                {
                    rate = new StationInventoryManifestRate { SpotLengthId = manifest.SpotLengthId };
                    manifest.ManifestRates.Add(rate);
                }

                rate.SpotCost = ProposalMath.CalculateCost(firstAudience.CPM, impressions);
            }
        }
    }
}
