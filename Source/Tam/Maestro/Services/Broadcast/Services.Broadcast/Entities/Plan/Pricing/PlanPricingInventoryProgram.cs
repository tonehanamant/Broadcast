using System.Collections.Generic;
using static Services.Broadcast.Entities.ProposalProgramDto;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingInventoryProgram
    {
        public int ManifestId { get; set; }

        public decimal SpotCost { get; set; }

        public double ProjectedImpressions { get; set; }

        public double? ProvidedImpressions { get; set; }

        public string StationLegacyCallLetters { get; set; }

        public string Unit { get; set; }

        public string InventorySource { get; set; }

        public string InventorySourceType { get; set; }

        public List<ManifestDaypartDto> ManifestDayparts { get; set; }

        public List<ManifestAudienceDto> ManifestAudiences { get; set; }

        public List<int> MediaWeekIds { get; set; }
    }
}
