using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingRequestDto
    {
        public int PlanId { get; set; }
        public decimal? MinCpm { get; set; }
        public decimal? MaxCpm { get; set; }
        public double ImpressionsGoal { get; set; }
        public decimal BudgetGoal { get; set; }
        public double ProprietaryBlend { get; set; }
        public decimal CpmGoal { get; set; }
        public double? CompetitionFactor { get; set; }
        public double? InflationFactor { get; set; }
        public int UnitCaps { get; set; }
        public UnitCapEnum UnitCapType { get; set; }
    }
}