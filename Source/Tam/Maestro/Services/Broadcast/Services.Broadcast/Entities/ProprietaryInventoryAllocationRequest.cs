using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProprietaryInventoryAllocationRequest
    {
        public ProprietaryInventoryAllocationRequest()
        {
            SlotAllocations = new List<ProprietaryInventorySlotAllocations>();
        }

        public List<ProprietaryInventorySlotAllocations> SlotAllocations { get; set; }

        public int ProposalDetailId { get; set; }
        public bool ForceSave { get; set; }
        public string UserName { get; set; }
    }

    public class ProprietaryInventorySlotAllocations
    {
        public ProprietaryInventorySlotAllocations()
        {
            Deletes = new List<ProprietaryInventorySlotProposal>();
            Adds = new List<ProprietaryInventorySlotProposal>();
        }

        public int InventoryDetailSlotId { get; set; }
        public List<ProprietaryInventorySlotProposal> Deletes { get; set; }
        public List<ProprietaryInventorySlotProposal> Adds { get; set; }
    }

    public class ProprietaryInventorySlotProposal
    {
        public int QuarterWeekId { get; set; }
        public int Order { get; set; }
        public int SpotLength { get; set; }
        public double Impressions { get; set; }
    }
}