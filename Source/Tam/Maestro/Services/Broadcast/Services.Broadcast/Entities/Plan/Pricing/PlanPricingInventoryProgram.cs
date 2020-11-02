using Amazon.MissingTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingInventoryProgram : BasePlanInventoryProgram, ICloneable
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
        public InventoryPricingQuarterType InventoryPricingQuarterType { get; set; }

        public int SpotLengthId { get; set; }

        public List<ManifestWeek> ManifestWeeks { get; set; }

        public PostingTypeEnum PostingType { get; set; }

        public object Clone()
        {
            return new PlanPricingInventoryProgram
            {
                ManifestId = ManifestId,
                StandardDaypartId = StandardDaypartId,
                Station = Station,
                ManifestRates = ManifestRates,
                ManifestAudiences = ManifestAudiences,
                ManifestDayparts = ManifestDayparts,
                ProjectedImpressions = ProjectedImpressions,
                ProvidedImpressions = ProvidedImpressions,
                Cpm = Cpm,
                Unit = Unit,
                InventorySource = InventorySource,
                InventoryPricingQuarterType = InventoryPricingQuarterType,
                SpotLengthId = SpotLengthId,
                ManifestWeeks = ManifestWeeks,
                PostingType = PostingType
            };
        }

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
