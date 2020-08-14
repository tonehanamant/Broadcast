using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventoryProprietary
{
	public class InventoryProprietarySummaryResponse
	{
		public List<InventoryProprietarySummary> summaries { get; set; }
		public string ValidationMessage { get; set; }
	}
}