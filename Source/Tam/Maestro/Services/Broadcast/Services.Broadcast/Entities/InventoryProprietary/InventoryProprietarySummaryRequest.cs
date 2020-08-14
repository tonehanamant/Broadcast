using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using static Services.Broadcast.Entities.Plan.PlanBuying.PlanBuyingDetails;

namespace Services.Broadcast.Entities.InventoryProprietary
{
	public class InventoryProprietarySummaryRequest
	{
		public DateTime FlightStartDate { get; set; }
		public DateTime FlightEndDate { get; set; }
		public List<PlanDaypartRequest> PlanDaypartRequests { get; set; }
		public int AudienceId { get; set; }
	}
	public class PlanDaypartRequest
	{
		public int DefaultDayPartId { get; set; }
		public int StartTimeSeconds { get; set; }
		public int EndTimeSeconds { get; set; }
	}
}