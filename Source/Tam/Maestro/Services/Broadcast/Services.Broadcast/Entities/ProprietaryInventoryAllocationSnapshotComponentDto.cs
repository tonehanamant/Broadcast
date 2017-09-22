using System;

namespace Services.Broadcast.Entities
{
    public class ProprietaryInventoryAllocationSnapshotComponentDto
    {
        public int InventoryDetailSlotComponentId { get; set; }
        public int ProprosalVersionDetailQuarterWeekId { get; set; }
        public int InventoryDetailSlotId { get; set; }
        public int DaypartId { get; set; }
        public short StationCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DaypartCode { get; set; }
        public RatesFile.RateSourceType RateSource { get; set; }
        public int Order { get; set; }
    }
}
