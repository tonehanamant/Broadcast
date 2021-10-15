
namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlanDetailsDto
    {
        public int Id { get; set; }
        public int SpotExceptionsRecommendedPlanId { get; set; }
        public int RecommendedPlanId { get; set; }
        public double MetricPercent { get; set; }
        public bool IsRecommendedPlan { get; set; }
    }
}