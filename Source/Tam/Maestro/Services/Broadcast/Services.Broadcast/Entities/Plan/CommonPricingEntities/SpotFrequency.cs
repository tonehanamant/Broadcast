using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class SpotFrequency
    {
        public int SpotLengthId { get; set; }

        public decimal SpotCost { get; set; }

        public decimal SpotCostWithMargin { get; set; }

        public int Spots { get; set; }
    }
}
