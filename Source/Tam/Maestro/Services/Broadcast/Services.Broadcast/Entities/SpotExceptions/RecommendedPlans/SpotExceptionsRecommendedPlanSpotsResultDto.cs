using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    public class SpotExceptionsRecommendedPlanSpotsResultDto
    {
        public SpotExceptionsRecommendedPlanSpotsResultDto()
        {
            Active = new List<SpotExceptionsRecommendedToDoPlanSpotsDto>();
            Queued = new List<SpotExceptionsRecommendedDonePlanSpotsDto>();
            Synced = new List<SpotExceptionsRecommendedDonePlanSpotsDto>();
        }
        public List<SpotExceptionsRecommendedToDoPlanSpotsDto> Active { get; set; }
        public List<SpotExceptionsRecommendedDonePlanSpotsDto> Queued { get; set; }
        public List<SpotExceptionsRecommendedDonePlanSpotsDto> Synced { get; set; }
    }

    public class SpotExceptionsRecommendedToDoPlanSpotsDto
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
    public class SpotExceptionsRecommendedDonePlanSpotsDto
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
}
