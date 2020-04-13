using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingParametersDto
    {
        public int PlanId { get; set; }
        public int PlanVersionId { get; set; }
        public decimal? MinCpm { get; set; }
        public decimal? MaxCpm { get; set; }
        public double DeliveryImpressions { get; set; }
        public decimal Budget { get; set; }
        public double ProprietaryBlend { get; set; }
        public decimal CPM { get; set; }
        public double? CompetitionFactor { get; set; }
        public double? InflationFactor { get; set; }
        public int UnitCaps { get; set; }
        public UnitCapEnum UnitCapsType { get; set; }
        public PlanCurrenciesEnum Currency { get; set; }
        public decimal CPP { get; set; }
        public double DeliveryRatingPoints { get; set; }
        public double Margin { get; set; }
        public int? JobId { get; set; }
        public List<PlanPricingInventorySourceDto> InventorySourcePercentages { get; set; } = new List<PlanPricingInventorySourceDto>();
        public List<PlanPricingInventorySourceTypeDto> InventorySourceTypePercentages { get; set; } = new List<PlanPricingInventorySourceTypeDto>();
    }
}
