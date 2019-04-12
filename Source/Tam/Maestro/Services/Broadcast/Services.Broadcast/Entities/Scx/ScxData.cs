using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Scx
{
    public class ScxData
    {
        public string DaypartCode { get; set; }
        public string AdvertisersName { get; set; }
        public string SpotLength { get; set; }
        public DateTime StartDate { get; set; }
        public ProposalDto ProposalDto { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<int, string> SurveyData { get; set; }
        public List<MarketPlaybackTypes> MarketPlaybackTypes { get; set; }
        /// <summary>
        /// Contains all possible markets
        /// </summary>
        public Dictionary<int, ScxMarketDto> InventoryMarkets { get; set; }
        public Dictionary<int, string> DmaMarketName { get; set; }

        /// <summary>
        /// Actual markets with spots
        /// </summary>
        public List<int> MarketIds { get; set; }

        public List<ScxMarketStationProgramSpotWeek> WeekData { get; set; }
        public List<DemoData> Demos { get; set; }

        public InventorySource InventorySource { get; set; }

        public string UnitName { get; set; }
    }

    public class DemoData
    {
        public bool IsPrimaryDemo { get; set; }
        public int DemoRank { get; set; }
        public BroadcastAudience Demo { get; set; }
        public int AudienceId { get; set; }
        public List<int> RatingAudienceIds { get; set; }
        public Dictionary<short, double> MarketPopulations { get; set; }

        public List<StationImpressions> Impressions { get; set; }
        public List<Ratingdata> Ratings { get; set; }
    }

    public class Ratingdata
    {
        public string LegacyCallLetters { get; set; }
        public int DaypartId { get; set; }
        public double Rating { get; set; }
    }

}
