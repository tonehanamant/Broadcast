using Services.Broadcast.Entities.Buying;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.PlanPricing;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingApiRequestParametersDto
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
        public List<PlanBuyingMarketDto> Markets { get; set; } = new List<PlanBuyingMarketDto>();
        public double CoverageGoalPercent { get; set; }
        public double? Margin { get; set; }
        public int? JobId { get; set; }
        public List<PlanBuyingInventorySourceDto> InventorySourcePercentages { get; set; } = new List<PlanBuyingInventorySourceDto>();
        public List<PlanBuyingInventorySourceTypeDto> InventorySourceTypePercentages { get; set; } = new List<PlanBuyingInventorySourceTypeDto>();
    }
}
