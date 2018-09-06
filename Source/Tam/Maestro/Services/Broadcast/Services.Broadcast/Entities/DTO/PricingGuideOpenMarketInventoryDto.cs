using Services.Broadcast.Entities.OpenMarketInventory;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.DTO
{
    public class PricingGuideOpenMarketInventoryDto : ProposalDetailInventoryBase
    {
        public OpenMarketCriterion Criteria { get; set; }
        public List<PricingGuideMarket> Markets { get; set; } = new List<PricingGuideMarket>();

        public class PricingGuideMarket : IInventoryMarket
        {
            public string MarketName { get; set; }
            public int MarketId { get; set; }
            public double MarketCoverage { get; set; }
            public int MarketRank { get; set; }
            public int TotalSpots { get; set; }
            public double TotalImpressions { get; set; }
            public double TotalOvernightImpressions { get; set; }
            public decimal TotalCost { get; set; }
            public List<PricingGuideStation> Stations { get; set; } = new List<PricingGuideStation>();

            public class PricingGuideStation
            {
                public int StationCode { get; set; }
                public string CallLetters { get; set; }
                public string LegacyCallLetters { get; set; }
                public string Affiliation { get; set; }
                public double TotalStationImpressions { get; set; }
                public List<PricingGuideProgram> Programs { get; set; } = new List<PricingGuideProgram>();

                public class PricingGuideProgram
                {
                    public int ProgramId { get; set; }
                    public List<LookupDto> Dayparts { get; set; } = new List<LookupDto>();
                    public List<string> ProgramNames { get; set; } = new List<string>();
                    public decimal BlendedCpm { get; set; }
                    public int Spots { get; set; }
                    public double ImpressionsPerSpot { get; set; }
                    public double Impressions { get; set; }
                    public double OvernightImpressions { get; set; }
                    public decimal CostPerSpot { get; set; }
                    public decimal Cost { get; set; }
                    public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
                    public List<ProposalProgramFlightWeek> FlightWeeks { get; set; } = new List<ProposalProgramFlightWeek>();
                }
            }
        }
    }
}
