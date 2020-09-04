namespace Services.Broadcast.Entities.InventoryProprietary
{
	public class InventoryProprietarySummary
	{
        public int Id { get; set; }
        public decimal UnitCost { get; set; }
		public string DaypartName { get; set; }
		public string InventorySourceName { get; set; }
		public double ImpressionsTotal { get; set; }
		public double MarketCoverageTotal { get; set; }
		public string UnitType { get; set; }
		public string UnitName => $"{InventorySourceName} {UnitType}";
        public decimal Cpm { get; set; }
    }
}