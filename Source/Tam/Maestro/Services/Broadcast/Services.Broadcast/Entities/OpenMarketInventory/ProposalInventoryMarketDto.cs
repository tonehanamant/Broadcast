using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalInventoryMarketDto
    {
        public string MarketName { get; set; }
        public int MarketId { get; set; }
        public double MarketSubscribers { get; set; }
        public int MarketRank { get; set; }
        public List<InventoryMarketStation> Stations { get; set; } = new List<InventoryMarketStation>();

        public class InventoryMarketStation
        {
            public int StationCode { get; set; }
            public string CallLetters { get; set; }
            public string LegacyCallLetters { get; set; }
            public string Affiliation { get; set; }
            public List<InventoryMarketStationProgram> Programs { get; set; } = new List<InventoryMarketStationProgram>();
        }

        public class InventoryMarketStationProgram
        {            
            public int ProgramId { get; set; }
            public List<string> ProgramNames { get; set; } = new List<string>();
            public List<LookupDto> Dayparts { get; set; } = new List<LookupDto>();
            public decimal TargetCpm { get; set; }
            public double TargetImpressions { get; set; }
            public double UnitImpressions { get; set; }
            public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
            public int Spots { get; set; }
            public List<ProposalProgramFlightWeek> FlightWeeks { get; set; } = new List<ProposalProgramFlightWeek>();
            public short StationCode { get; set; }
            public double? ProvidedUnitImpressions { get; set; }
        }
    }
}
