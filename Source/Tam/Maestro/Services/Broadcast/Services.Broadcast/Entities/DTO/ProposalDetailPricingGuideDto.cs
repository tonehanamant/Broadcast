using System.Collections.Generic;
using Services.Broadcast.Repositories;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalDetailPricingGuideGridDto
    {
        public ProposalDetailPricingGuideGridDto()
        {
            OpenMarkets = new List<OpenMarket>();
        }
        public int ProposalDetailId { get; set; }
        public int ProposalId { get; set; }
        public int Spots { get; set; }
        public decimal Cost { get; set; }
        public double Impressions { get; set; }
        public double StationImpressions { get; set; }
        public double OvernightImpressions { get; set; }

        public List<OpenMarket> OpenMarkets { get; set; }

        public class OpenMarket
        {
            public OpenMarket()
            {
                Stations = new List<OpenMarketStation>();
            }
            public string MarketName { get; set; }
            public int MarketRank { get; set; }
            public int MarketId { get; set; }
            public List<OpenMarketStation> Stations { get; set; }
            public int Spots { get; set; }
            public decimal Cost { get; set; }
            public double Impressions { get; set; }
            public double StationImpressions { get; set; }
            public double OvernightImpressions { get; set; }
        }

        public class OpenMarketStation
        {
            public int StationCode { get; set; }
            public string CallLetters { get; set; }
            public string LegacyCallLetters { get; set; }

            public List<OpenMarketProgram> Programs { get; set; }
        }

        public class OpenMarketProgram
        {
            public int ProgramId { get; set; }
            public string ProgramName { get; set; }
            public LookupDto Daypart { get; set; }
            public int Spots { get; set; }
            public decimal CPM { get; set; }
            public decimal Cost { get; set; }
            public double Impressions { get; set; }
            public double StationImpressions { get; set; }
            public double OvernightImpressions { get; set; }

        }
    }

}
