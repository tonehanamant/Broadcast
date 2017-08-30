namespace Services.Broadcast.Entities
{
    public class ProprietaryInventoryAllocationSnapshotComponentDto
    {
        public int InventoryDetailSlotComponentId { get; set; }
        public int ProprosalVersionDetailQuarterWeekId { get; set; }
        public int InventoryDetailSlotId { get; set; }
        public int DaypartId { get; set; }
        public short StationCode { get; set; }
        public int StationProgramId { get; set; }
        public int StationProgramFlightId { get; set; }
    }
}
