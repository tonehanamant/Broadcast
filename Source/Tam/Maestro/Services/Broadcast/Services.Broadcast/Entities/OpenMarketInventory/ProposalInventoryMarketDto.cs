using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalInventoryMarketDto
    {
        public ProposalInventoryMarketDto()
        {
            Stations = new List<InventoryMarketStation>();
        }

        public string MarketName { get; set; }
        public int MarketId { get; set; }
        public double MarketSubscribers { get; set; }
        public int MarketRank { get; set; }
        public List<InventoryMarketStation> Stations { get; set; }

        public class InventoryMarketStation
        {
            public InventoryMarketStation()
            {
                Programs = new List<InventoryMarketStationProgram>();
            }

            public int StationCode { get; set; }
            public string CallLetters { get; set; }
            public string LegacyCallLetters { get; set; }
            public string Affiliation { get; set; }
            public List<InventoryMarketStationProgram> Programs { get; set; } 
            
        }

        public class InventoryMarketStationProgram
        {
            public InventoryMarketStationProgram()
            {
                Genres = new List<LookupDto>();
                FlightWeeks = new List<ProposalProgramFlightWeek>();
                ProgramNames = new List<string>();
                Dayparts = new List<LookupDto>();
            } 
            public int ProgramId { get; set; }
            public List<string> ProgramNames { get; set; }
            public List<LookupDto> Dayparts { get; set; }
            public decimal TargetCpm { get; set; }
            public double TargetImpressions { get; set; }
            public double UnitImpressions { get; set; }
            public List<LookupDto> Genres { get; set; }
            public int Spots { get; set; }
            public List<ProposalProgramFlightWeek> FlightWeeks { get; set; }
            public short StationCode { get; set; }
        }
    }
}
