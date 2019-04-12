using System.Linq;
using Services.Broadcast.Entities.StationInventory;
using System.Collections.Generic;
using Common.Services.ApplicationServices;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProprietarySpotCostCalculationEngine : IApplicationService
    {
        void CalculateSpotCost(IEnumerable<StationInventoryManifest> manifests, bool useProvidedImpressions = false);
    }

    public class ProprietarySpotCostCalculationEngine : IProprietarySpotCostCalculationEngine
    {
        public void CalculateSpotCost(IEnumerable<StationInventoryManifest> manifests, bool useProvidedImpressions)
        {
            foreach (var manifest in manifests)
            {
                var firstAudience = manifest.ManifestAudiences.FirstOrDefault();

                if (firstAudience == null)
                    continue;

                var impressions = useProvidedImpressions ?
                   firstAudience.Impressions.Value :
                   manifest.ProjectedStationImpressions.Average(x => x.Impressions);

                if (impressions == 0)
                    continue;

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
