using Services.Broadcast.Entities.OpenMarketInventory;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.DTO
{
    public class PricingGuideOpenMarketInventory : ProposalDetailInventoryBase
    {
        public int ProposalDetailId { get; set; }
        public DateTime ProposalDetailFlightStartDate { get; set; }
        public DateTime ProposalDetailFlightEndDate { get; set; }
        public int ProposalDetailSpotLengthId { get; set; }
        public int ProposalDetailDaypartId { get; set; }
        public List<PricingGuideMarket> Markets { get; set; } = new List<PricingGuideMarket>();
    }

    public class PricingGuideMarket : IInventoryMarket
    {
        public string MarketName { get; set; }
        public int MarketId { get; set; }
        public double MarketCoverage { get; set; }
        public int MarketRank { get; set; }
        public List<PricingGuideStation> Stations { get; set; } = new List<PricingGuideStation>();
    }

    public class PricingGuideStation
    {
        public int StationCode { get; set; }
        public string CallLetters { get; set; }
        public string LegacyCallLetters { get; set; }
        public string Affiliation { get; set; }
        public List<PricingGuideProgram> Programs { get; set; } = new List<PricingGuideProgram>();
    }

    public class PricingGuideProgram
    {
        public int ProgramId { get; set; }
        public List<string> ProgramNames { get; set; } = new List<string>();
        public List<LookupDto> Dayparts { get; set; } = new List<LookupDto>();
        public decimal Cpm { get; set; }
        public double TargetImpressions { get; set; }
        public double UnitImpressions { get; set; }
        public double OvernightImpressions { get; set; }
        public double StationImpressions { get; set; }
        public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
        public int Spots { get; set; }
        public List<ProposalProgramFlightWeek> FlightWeeks { get; set; } = new List<ProposalProgramFlightWeek>();
        public short StationCode { get; set; }
    }
}
