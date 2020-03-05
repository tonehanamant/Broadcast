using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class MarketCoverageByStation
    {
        public int MarketCoverageFileId { get; set; }
        public List<Market> Markets { get; set; }

        public class Market
        {
            public int MarketCode { get; set; }
            public string MarketName { get; set; }
            public double Coverage { get; set; }
            public int Rank { get; set; }
            public List<Station> Stations { get; set; }

            public class Station
            {
                public string LegacyCallLetters { get; set; }
            }
        } 
    }
}
