using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.QuoteReport
{
    public class QuoteProgram : BasePlanInventoryProgram
    {
        public List<ImpressionsPerAudience> DeliveryPerAudience { get; set; } = new List<ImpressionsPerAudience>();

        public class ImpressionsPerAudience
        {
            public int AudienceId { get; set; }

            public double ProjectedImpressions { get; set; }

            public double? ProvidedImpressions { get; set; }

            public decimal CPM { get; set; }

            public double Impressions => ProvidedImpressions ?? ProjectedImpressions;
        }
    }
}
