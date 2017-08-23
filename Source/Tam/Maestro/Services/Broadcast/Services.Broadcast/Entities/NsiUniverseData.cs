using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public class NsiUniverseData
    {
        public NsiUniverseData()
        {
            UniverseDataForProposalAudience = new Dictionary<short, double>();
            UniverseDataForHouseHoldAudience = new Dictionary<short, double>();
            UniverseDataForAdditionalAudience = new Dictionary<short, double>();
        }

        // the ids will be used to compare if any value has been changed against to what is in the proposal
        public int? GuaranteedDemoId { get; set; }
        public int SweepMonthId { get; set; }
        public int? AdditionalAudienceId { get; set; }

        public Dictionary<short, double> UniverseDataForProposalAudience { get; set; }
        public Dictionary<short, double> UniverseDataForHouseHoldAudience { get; set; }
        public Dictionary<short, double> UniverseDataForAdditionalAudience { get; set; }

        public double TotalDemoUniverse
        {
            get
            {
                return UniverseDataForProposalAudience != null && UniverseDataForProposalAudience.Any()
                    ? UniverseDataForProposalAudience.Sum(x => x.Value)
                    : 0;
            }
        }

        public double TotalHHUniverse
        {
            get
            {
                return UniverseDataForHouseHoldAudience != null && UniverseDataForHouseHoldAudience.Any()
                    ? UniverseDataForHouseHoldAudience.Sum(x => x.Value)
                    : 0;
            }
        }
    }
}
