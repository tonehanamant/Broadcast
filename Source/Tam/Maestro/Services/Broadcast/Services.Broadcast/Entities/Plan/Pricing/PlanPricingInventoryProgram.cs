using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

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

        public List<ManifestDaypart> ManifestDayparts { get; set; }

        public List<ManifestAudience> ManifestAudiences { get; set; }

        public List<int> MediaWeekIds { get; set; }

        public class ManifestDaypart
        {
            public int Id { get; set; }

            public DisplayDaypart Daypart { get; set; }

            public List<Program> Programs { get; set; }

            public class Program
            {
                public string Name { get; set; }

                public string ShowType { get; set; }

                public string Genre { get; set; }
            }
        }

        public class ManifestAudience
        {
            public int AudienceId { get; set; }

            public double? Impressions { get; set; }

            public bool IsReference { get; set; }
        }
    }
}
