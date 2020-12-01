using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingInventoryProgram : BasePlanInventoryProgram
    {
        public double ProjectedImpressions { get; set; }

        public double? ProvidedImpressions { get; set; }

        public double PostingTypeImpressions
        {
            get
            {
                //Impresions are by default NSI
                var impressions = ProvidedImpressions ?? ProjectedImpressions;

                if (PostingType == PostingTypeEnum.NTI)
                    impressions *= NsiToNtiImpressionConversionRate;

                return impressions;
            }
        }

        public decimal Cpm { get; set; }

        public string Unit { get; set; }

        public InventorySource InventorySource { get; set; }        

        [JsonConverter(typeof(StringEnumConverter))]
        public InventoryPricingQuarterType InventoryPricingQuarterType { get; set; }

        public int SpotLengthId { get; set; }

        public List<ManifestWeek> ManifestWeeks { get; set; }

        public PostingTypeEnum PostingType { get; set; } = PostingTypeEnum.NSI; //Default NSI

        public double NsiToNtiImpressionConversionRate { get; set; } = 1; //Default to 1

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
