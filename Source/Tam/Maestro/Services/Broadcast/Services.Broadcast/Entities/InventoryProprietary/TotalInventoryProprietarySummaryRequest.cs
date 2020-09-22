using System.Collections.Generic;
using Services.Broadcast.Entities.Plan;

namespace Services.Broadcast.Entities.InventoryProprietary
{
	public class TotalInventoryProprietarySummaryRequest
	{
		public int PlanGoalImpressions { get; set; }
		public int PlanPrimaryAudienceId { get; set; }
		public List<InventoryProprietarySummaryId> InventoryProprietarySummaryIds { get; set; }
		public List<WeeklyBreakdownWeek> WeeklyBreakdownWeeks { get; set; }
		public List<int> SpotLengthIds { get; set; }
	}

	public class InventoryProprietarySummaryId
	{
		public int Id { get; set; }
		public double? NumberOfUnit { get; set; }
	}

}