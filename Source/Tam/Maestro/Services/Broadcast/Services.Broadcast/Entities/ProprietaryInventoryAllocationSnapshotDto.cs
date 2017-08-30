using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProprietaryInventoryAllocationSnapshotDto
    {
        public int InventoryDetailSlotId { get; set; }
        public int ProprosalVersionDetailQuarterWeekId { get; set; }
        public int Order { get; set; }
        public string CreatedBy { get; set; }
        public string Isci { get; set; }
        public int RolledUpDaypartId { get; set; }
        public decimal SlotCost { get; set; }
        public int SpotLengthId { get; set; }
        public int Impressions { get; set; }
        public List<ProprietaryInventoryAllocationSnapshotComponentDto> Components { get; set; }
    }
}
