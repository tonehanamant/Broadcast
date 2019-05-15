using Services.Broadcast.Entities.OpenMarketInventory;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.Scx
{
    public class ScxMarketDto
    {
        public string MarketName { get; set; }
        public string DmaMarketName { get; set; }
        public int? MarketId { get; set; }
        public int? MarketRank { get; set; }
        public double? MarketCoverage { get; set; }
        public double? MarketSubscribers { get; set; }
        public List<ScxStation> Stations { get; set; } = new List<ScxStation>();

        public class ScxStation
        {
            public int? StationCode { get; set; }
            public string CallLetters { get; set; }
            public string LegacyCallLetters { get; set; }
            public string Affiliation { get; set; }
            public List<ScxProgram> Programs { get; set; } = new List<ScxProgram>();
        }

        public class ScxProgram
        {
            public int ProgramId { get; set; }
            public List<string> ProgramNames { get; set; } = new List<string>();
            public List<LookupDto> Dayparts { get; set; } = new List<LookupDto>();
            public decimal TargetCpm { get; set; }
            public double TargetImpressions { get; set; }
            public double UnitImpressions { get; set; }
            public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
            public List<ProposalProgramFlightWeek> FlightWeeks { get; set; } = new List<ProposalProgramFlightWeek>();
            public int? StationCode { get; set; }
            public double? ProvidedUnitImpressions { get; set; }
        }
    }

    

    
}
