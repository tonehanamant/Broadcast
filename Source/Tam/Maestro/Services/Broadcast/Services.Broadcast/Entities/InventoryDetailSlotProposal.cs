using System;
using EntityFrameworkMapping.Broadcast;
using Newtonsoft.Json;

namespace Services.Broadcast.Entities
{
    public class InventoryDetailSlotProposal
    {
        public int Order { get; set; }
        public string ProposalName { get; set; }
        public int ProposalDetailSpotLengthId { get; set; }
        public int ProposalDetailId { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }
        public string UserName { get; set; }
        public DateTime WeekStartDate { get; set; }

        [JsonIgnore]
        public inventory_detail_slot_proposal idsp { get; set; }
    }
}