using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Campaign
{
    public class ProgramLineupReportRequest
    {
        public List<int> SelectedPlans { get; set; }

        public PostingTypeEnum? PostingType { get; set; }

        public SpotAllocationModelMode? SpotAllocationModelMode { get; set; }
    }
}
