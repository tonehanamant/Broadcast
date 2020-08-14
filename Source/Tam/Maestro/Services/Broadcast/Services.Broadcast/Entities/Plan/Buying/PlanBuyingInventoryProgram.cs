using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingInventoryProgram : BasePlanInventoryProgram
    {
        public double ProjectedImpressions { get; set; }

        public double? ProvidedImpressions { get; set; }

        public double Impressions
        {
            get
            {
                return ProvidedImpressions ?? ProjectedImpressions;
            }
        }

        public decimal Cpm { get; set; }

        public string Unit { get; set; }

        public InventorySource InventorySource { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public InventoryPricingQuarterType InventoryBuyingQuarterType { get; set; }

        public int SpotLengthId { get; set; }

        public List<ManifestWeek> ManifestWeeks { get; set; }

        public class ManifestWeek
        {
            public int Id { get; set; }

            /// <summary>
            /// Indicates inventory week
            /// </summary>
            public int InventoryMediaWeekId { get; set; }

            /// <summary>
            /// Indicates what plan flight week inventory week should be associated with
            /// </summary>
            public int ContractMediaWeekId { get; set; }

            public int Spots { get; set; }
        }
    }
}
