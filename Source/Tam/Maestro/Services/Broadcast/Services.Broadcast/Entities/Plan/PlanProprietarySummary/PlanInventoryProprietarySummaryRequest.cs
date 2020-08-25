using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
	public class PlanInventoryProprietarySummaryRequest
	{
		public int PlanGoalImpressions { get; set; }
		public int PlanPrimaryAudienceId { get; set; }
		public List<int> InventoryProprietarySummaryIds { get; set; }
	}
}