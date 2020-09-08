using System;
using System.Collections.Generic;
using EntityFrameworkMapping.Broadcast;

namespace Services.Broadcast.Entities.InventoryProprietary
{
	public class InventoryProprietaryQuarterSummaryDto
	{
		public int Id { get; set; }
		public int InventorySourceId { get; set; }
		public QuarterDto Quarter { get; internal set; }
		public int? DefaultDaypartId { get; set; }
        public decimal UnitCost { get; set; }
        public int SlotNumber { get; set; }
        public int ProprietaryDaypartProgramMappingId { get; set; }
        public List<InventoryProprietarySummaryAudiencesDto> Audiences { get; set; }
		public List<InventoryProprietarySummaryByMarketByAudience> SummaryByMarketByAudience { get; set; }
	}


	public class InventoryProprietarySummaryAudiencesDto
	{
		public int AudienceId { get; set; }
		public double? Impressions { get; set; }
	}
}