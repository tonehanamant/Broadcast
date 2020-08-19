namespace Services.Broadcast.Entities.InventoryProprietary
{
	public class InventoryProprietarySummary
	{
		public decimal Cpm { get; set; }
		public string DaypartName { get; set; }
		public int Id { get; set; }
		public string InventorySourceName { get; set; }
		public double ImpressionsTotal { get; set; }
		public double MarketCoverageTotal { get; set; }			
		public string DaypartUnitCode { get; set; }
		public string UnitName=>$"{InventorySourceName} {DaypartUnitCode}";
	}
}