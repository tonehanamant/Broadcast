﻿using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResultMarketsDto
    {
        public int PlanVersionId { get; set; }
        public int? PricingJobId { get; set; }
        public PlanPricingResultMarketsTotalsDto Totals { get; set; }
        public List<PlanPricingResultMarketDetailsDto> MarketDetails { get; set; } = new List<PlanPricingResultMarketDetailsDto>();
        public PostingTypeEnum PostingType { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
    }

    public class PlanPricingResultMarketsDto_v2
    {
        public int PlanVersionId { get; set; }
        public int? PricingJobId { get; set; }
        public PostingTypePlanPricingResultMarkets NsiResults { get; set; }
        public PostingTypePlanPricingResultMarkets NtiResults { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
    }

    public class PostingTypePlanPricingResultMarkets
    {
        public PlanPricingResultMarketsTotalsDto Totals { get; set; }
        public List<PlanPricingResultMarketDetailsDto> MarketDetails { get; set; } = new List<PlanPricingResultMarketDetailsDto>();
    }

    public class PlanPricingResultMarketsTotalsDto
    {
        public int Markets { get; set; }
        public double CoveragePercent { get; set; }
        public int Stations { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
    }

    public class PlanPricingResultMarketDetailsDto
    {
        public short MarketCode { get; set; }
        public int Rank { get; set; }
        public double MarketCoveragePercent { get; set; }
        public int Stations { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public double? ShareOfVoiceGoalPercentage { get; set; }
        public double ImpressionsPercentage { get; set; }
        public string MarketName { get; set; }
    }
}