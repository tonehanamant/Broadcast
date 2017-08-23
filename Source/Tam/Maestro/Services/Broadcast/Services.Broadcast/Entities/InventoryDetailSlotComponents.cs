namespace Services.Broadcast.Entities
{
    public class InventoryDetailSlotComponents
    {
        public int Id { get; set; }
        public int InventoryDetailSlotId { get; set; }
        public int StationProgramFlightId { get; set; }
        public short StationCode { get; set; }
        public int DaypartId { get; set; }
        public decimal? SpotCost30 { get; set; }
        public decimal? SpotCost15 { get; set; }
        public short MarketCode { get; set; }
    }
}
