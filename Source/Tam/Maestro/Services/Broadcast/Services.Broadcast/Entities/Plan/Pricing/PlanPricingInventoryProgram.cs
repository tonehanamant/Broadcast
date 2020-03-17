using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Services.Broadcast.Entities.Enums;
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

        public DisplayBroadcastStation Station { get; set; }

        public string Unit { get; set; }

        public InventorySource InventorySource { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public InventoryPricingQuarterType InventoryPricingQuarterType { get; set; }

        public List<ManifestDaypart> ManifestDayparts { get; set; }

        public List<ManifestAudience> ManifestAudiences { get; set; }

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

        public class ManifestDaypart
        {
            public int Id { get; set; }

            public DisplayDaypart Daypart { get; set; }
            
            public Program PrimaryProgram { get; set; }

            public int? PrimaryProgramId { get; set; }

            public List<Program> Programs { get; set; }

            public string ProgramName { get; set; }

            public class Program
            {
                public int Id { get; set; }

                public string Name { get; set; }

                public string ShowType { get; set; }

                public string Genre { get; set; }

                public int StartTime { get; set; }

                public int EndTime { get; set; }
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
