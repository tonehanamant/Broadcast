﻿using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.DTO.PricingGuide;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalDetailPricingGuideSave
    {
        public int ProposalDetailId { get; set; }
        public int MarketCoverageFileId { get; set; }
        public double? ImpressionLoss { get; set; }
        public double? Margin { get; set; }
        public double? GoalImpression { get; set; }
        public decimal? GoalBudget { get; set; }
        public double? Inflation { get; set; }
        public decimal? CpmMin { get; set; }
        public decimal? CpmMax { get; set; }
        public int? UnitCapPerStation { get; set; }
        public OpenMarketCpmTarget? OpenMarketCpmTarget { get; set; }
        public List<ProprietaryPricingDto> ProprietaryPricing { get; set; } = new List<ProprietaryPricingDto>();
        public List<PricingGuideSaveMarket> Markets { get; set; } = new List<PricingGuideSaveMarket>();
        public OpenMarketTotalsDto OpenMarketTotals { get; set; }
        public ProprietaryTotalsDto ProprietaryTotals { get; set; }
    }

    public class PricingGuideSaveMarket
    {
        public int ProgramId { get; set; }
        public int MarketId { get; set; }
        public int StationId { get; set; }
        public int StationCode { get; set; }
        public int ManifestDaypartId { get; set; }
        public int DaypartId { get; set; }
        public string ProgramName { get; set; }
        public decimal BlendedCpm { get; set; }
        public int Spots { get; set; }
        public double ImpressionsPerSpot { get; set; }
        public double StationImpressionsPerSpot { get; set; }
        public decimal CostPerSpot { get; set; }
    }
}
