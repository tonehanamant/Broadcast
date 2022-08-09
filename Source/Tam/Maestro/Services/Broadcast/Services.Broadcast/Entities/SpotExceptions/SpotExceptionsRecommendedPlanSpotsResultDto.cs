using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlanSpotsResultDto
    {
        public SpotExceptionsRecommendedPlanSpotsResultDto()
        {
            Active = new List<SpotExceptionsRecommendedActivePlanSpotsDto>();
            Queued = new List<SpotExceptionsRecommendedQueuedPlanSpotsDto>();
            Synced = new List<SpotExceptionsRecommendedSyncedPlanSpotsDto>();
        }
        public List<SpotExceptionsRecommendedActivePlanSpotsDto> Active { get; set; }
        public List<SpotExceptionsRecommendedQueuedPlanSpotsDto> Queued { get; set; }
        public List<SpotExceptionsRecommendedSyncedPlanSpotsDto> Synced { get; set; }
    }

    public class SpotExceptionsRecommendedActivePlanSpotsDto
    {
        public int Id { get; set; }
        public int EstimateId { get; set; }
        public string IsciName { get; set; }
        public string ProgramAirDate { get; set; }
        public string ProgramAirTime { get; set; }
        public string RecommendedPlan { get; set; }
        public double? Impressions { get; set; }
        public string ProgramName { get; set; }
        public string Affiliate { get; set; }
        public int? PlanId { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string InventorySource { get; set; }
    }
    public class SpotExceptionsRecommendedQueuedPlanSpotsDto
    {
        public int Id { get; set; }
        public int EstimateId { get; set; }
        public string IsciName { get; set; }
        public string ProgramAirDate { get; set; }
        public string ProgramAirTime { get; set; }
        public string RecommendedPlan { get; set; }
        public double? Impressions { get; set; }
        public string ProgramName { get; set; }
        public string Affiliate { get; set; }
        public int? PlanId { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string InventorySource { get; set; }
        public string DecisionString { get; set; }
    }
    public class SpotExceptionsRecommendedSyncedPlanSpotsDto
    {
        public int Id { get; set; }
        public int EstimateId { get; set; }
        public string IsciName { get; set; }
        public string ProgramAirDate { get; set; }
        public string ProgramAirTime { get; set; }
        public string RecommendedPlan { get; set; }
        public double? Impressions { get; set; }
        public string ProgramName { get; set; }
        public string Affiliate { get; set; }
        public int? PlanId { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string InventorySource { get; set; }
        public string SyncedTimestamp { get; set; }
        public string DecisionString { get; set; }
    }
}
