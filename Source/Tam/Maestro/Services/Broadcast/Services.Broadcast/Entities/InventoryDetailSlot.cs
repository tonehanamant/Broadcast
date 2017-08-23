using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class InventoryDetailSlot
    {
        public int Id { get; set; }
        public int InventoryDetailId { get; set; }
        public int SpotLengthId { get; set; }
        public int DetailLevel { get; set; }
        public int MediaWeekId { get; set; }
        public int TotalStations { get; set; }
        public int RolleupDaypartId { get; set; }
        public decimal? SlotCost { get; set; }
        public List<InventoryDetailSlotComponents> InventoryDetailSlotComponents { get; set; }
        public List<InventoryDetailSlotProposal> InventoryDetailSlotProposals { get; set; }
    }
}
