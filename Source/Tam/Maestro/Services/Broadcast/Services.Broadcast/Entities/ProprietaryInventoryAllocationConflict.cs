using System.Collections.Generic;
using Newtonsoft.Json;

namespace Services.Broadcast.Entities
{
    public class ProprietaryInventoryAllocationConflict
    {
        public ProprietaryInventoryAllocationConflict()
        {
            SlotProposals = new List<InventoryDetailSlotProposal>();
            ProposalAllocations = new List<ProposalProprietaryInventoryWeekDto.ProposalAllocationDto>();
            Messages = new List<string>();
        }

        public List<string> Messages { get; set; }
        public int InventoryDetailSlotId { get; set; }
        public int MediaWeekId { get; set; }
        public string SlotDaypartCode { get; set; }

        public List<ProposalProprietaryInventoryWeekDto.ProposalAllocationDto> ProposalAllocations { get; set; }

        [JsonIgnore]
        public List<InventoryDetailSlotProposal> SlotProposals { get; set; }
    }
}