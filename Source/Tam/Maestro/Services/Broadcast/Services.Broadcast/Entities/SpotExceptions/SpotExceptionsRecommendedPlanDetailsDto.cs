namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlanDetailsDto
    {
        public SpotExceptionsRecommendedPlanDetailsDto()
        {
            RecommendedPlanDetail = new RecommendedPlanDetailDto();
            SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto();
        }
        public int Id { get; set; }
        public int SpotExceptionsRecommendedPlanId { get; set; }
        public int RecommendedPlanId { get; set; }
        public RecommendedPlanDetailDto RecommendedPlanDetail { get; set; }
        public double MetricPercent { get; set; }
        public bool IsRecommendedPlan { get; set; }
        public SpotExceptionsRecommendedPlanDecisionDto SpotExceptionsRecommendedPlanDecision { get; set; }
    }
}