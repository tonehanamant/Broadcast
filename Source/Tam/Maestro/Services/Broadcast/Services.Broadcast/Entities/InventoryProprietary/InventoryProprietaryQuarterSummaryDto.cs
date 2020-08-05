using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventoryProprietary
{
	public class InventoryProprietaryQuarterSummaryDto
	{
		public int Id { get; set; }
		public int InventorySourceId { get; set; }
		public QuarterDto Quarter { get; internal set; }
		public int? DefaultDaypartId { get; set; }
		public decimal? Cpm { get; set; }
		public int SlotNumber { get; set; }

		public List<InventoryProprietarySummaryAudiencesDto> Audiences { get; set; }

		public List<InventoryProprietarySummaryMarketDto> Markets { get; set; }
	}


	public class InventoryProprietarySummaryMarketDto
	{
		public Int16 MarketCode { get; set; }
		public double MarketCoverage { get; set; }
	}

	public class InventoryProprietarySummaryAudiencesDto
	{
		public int AudienceId { get; set; }
		public double? Impressions { get; set; }
	}
}