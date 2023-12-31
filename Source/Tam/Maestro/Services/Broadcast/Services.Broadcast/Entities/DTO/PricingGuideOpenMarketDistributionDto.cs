﻿using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.OpenMarketInventory;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.DTO
{
    public class PricingGuideOpenMarketDistributionDto 
    {
        public PricingGuideOpenMarketInventoryRequestDto DistributionRequest { get; set; }

        public OpenMarketPricingGuideGridDisplayFilterDto DisplayFilter { get; set; } = new OpenMarketPricingGuideGridDisplayFilterDto();

        public OpenMarketPricingGuideGridFilterDto Filter { get; set; } = new OpenMarketPricingGuideGridFilterDto();

        public List<PricingGuideOpenMarketInventory.PricingGuideMarket> Markets { get; set; } = new List<PricingGuideOpenMarketInventory.PricingGuideMarket>();

        public List<PricingGuideMarketTotalsDto> AllMarkets { get; set; } = new List<PricingGuideMarketTotalsDto>();

        public PricingGuideOpenMarketInventory.OpenMarketTotals OpenMarketTotals { get; set; }

        public PricingGuideOpenMarketInventory.ProprietaryTotals ProprietaryTotals { get; set; }

        public PricingGuideOpenMarketInventory.PricingTotals PricingTotals { get; set; }
    }

    public class PricingGuideOpenMarketInventory : ProposalDetailInventoryBase
    {
        public OpenMarketCriterion Criteria { get; set; }
        public List<PricingGuideMarket> Markets { get; set; } = new List<PricingGuideMarket>();
        public List<PricingGuideMarketTotalsDto> AllMarkets { get; set; } = new List<PricingGuideMarketTotalsDto>();
        public double MarketCoverage { get; set; }
        public List<ProposalMarketDto> ProposalMarkets { get; set; }
        public ProposalEnums.ProposalMarketGroups? MarketGroupId { get; set; }
        public ProposalEnums.ProposalMarketGroups? BlackoutMarketGroupId { get; set; }

        public class PricingGuideMarket : IInventoryMarket
        {
            public string MarketName { get; set; }
            public int MarketId { get; set; }
            public double MarketCoverage { get; set; }
            public int MarketRank { get; set; }
            public int TotalSpots { get; set; }
            public double TotalImpressions { get; set; }
            public decimal TotalCost { get; set; }
            public decimal MinCpm { get; set; } = 0;
            public decimal AvgCpm { get; set; } = 0;
            public decimal MaxCpm { get; set; } = 0;
            public List<PricingGuideStation> Stations { get; set; } = new List<PricingGuideStation>();
            public double DisplayImpressions
            {
                get
                {
                    return Stations.SelectMany(s => s.Programs).Sum(p => p.Spots * p.ImpressionsPerSpot);
                }
            }

            public double DisplayStationImpressions
            {
                get
                {
                    return Stations.SelectMany(s => s.Programs).Sum(p => p.Spots * p.StationImpressionsPerSpot);
                }
            }

            public class PricingGuideStation
            {
                public int StationCode { get; set; }
                public string CallLetters { get; set; }
                public string LegacyCallLetters { get; set; }
                public string Affiliation { get; set; }
                public List<PricingGuideProgram> Programs { get; set; } = new List<PricingGuideProgram>();

                public decimal MinProgramsBlendedCpm => Programs.Any() ? Programs.Min(p => p.BlendedCpm) : 0;

                public class PricingGuideProgram
                {
                    public int ProgramId { get; set; }
                    public int ManifestDaypartId { get; set; }

                    public LookupDto Daypart { get; set; } = new LookupDto();
                    public string ProgramName { get; set; } 
                    public decimal BlendedCpm { get; set; }
                    public int Spots { get; set; }
                    public double ImpressionsPerSpot { get; set; }
                    public double Impressions { get; set; }
                    public double StationImpressionsPerSpot { get; set; }
                    public decimal CostPerSpot { get; set; }
                    public decimal Cost { get; set; }
                    public bool HasImpressions { get; set; }
                    public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
                    public double EffectiveImpressionsPerSpot
                    {
                        get
                        {
                            if (StationImpressionsPerSpot != 0)
                            {
                                return StationImpressionsPerSpot;
                            }
                            else
                            {
                                return ImpressionsPerSpot;
                            }
                        }
                    }

                    public double DisplayImpressions
                    {
                        get
                        {
                            if(Spots == 0)
                            {
                                return ImpressionsPerSpot;
                            }
                            else
                            {
                                return ImpressionsPerSpot * Spots;
                            }
                        }
                    }

                    public double DisplayStationImpressions
                    {
                        get
                        {
                            if (Spots == 0)
                            {
                                return StationImpressionsPerSpot;
                            }
                            else
                            {
                                return StationImpressionsPerSpot * Spots;
                            }
                        }
                    }

                    public decimal DisplayCost
                    {
                        get
                        {
                            if (Spots == 0)
                            {
                                return CostPerSpot;
                            }
                            else
                            {
                                return Cost;
                            }
                           
                        }
                    }
                }
            }
        }

        public class OpenMarketTotals
        {
            public decimal Cpm { get; set; }
            public decimal Cost { get; set; }
            public double Impressions { get; set; }
            public double Coverage { get; set; }
        }

        public class ProprietaryTotals
        {
            public decimal Cpm { get; set; }
            public decimal Cost { get; set; }
            public double Impressions { get; set; }
        }

        public class PricingTotals
        {
            public decimal Cpm { get; set; }
            public decimal Cost { get; set; }
            public double Impressions { get; set; }
            public double Coverage { get; set; }
        }
    }
}
