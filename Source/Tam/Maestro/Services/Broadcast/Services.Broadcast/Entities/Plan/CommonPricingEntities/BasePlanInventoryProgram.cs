using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class BasePlanInventoryProgram
    {
        public int ManifestId { get; set; }

        public int StandardDaypartId { get; set; }

        public DisplayBroadcastStation Station { get; set; }

        public List<ManifestRate> ManifestRates { get; set; }

        public List<ManifestAudience> ManifestAudiences { get; set; }

        public List<ManifestDaypart> ManifestDayparts { get; set; }

        public class ManifestRate
        {
            public int SpotLengthId { get; set; }

            public decimal Cost { get; set; }
        }

        public class ManifestAudience
        {
            public int AudienceId { get; set; }

            public double? Impressions { get; set; }
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
    }
}
