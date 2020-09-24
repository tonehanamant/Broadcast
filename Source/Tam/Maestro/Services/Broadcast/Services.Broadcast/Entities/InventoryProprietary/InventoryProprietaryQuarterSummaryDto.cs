using System.Collections.Generic;

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
		public List<InventoryProprietarySummaryByStationByAudience> SummaryByStationByAudience { get; set; }
        public string ProgramName { get; set; }
        public string Genre { get; set; }
    }
}