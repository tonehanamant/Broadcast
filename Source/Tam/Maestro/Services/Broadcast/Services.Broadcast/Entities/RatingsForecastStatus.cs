using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities
{
    public class RatingsForecastStatus
    {
        public MediaMonth MediaMonth { get; set; }
        public int UniverseMarkets { get; set; }
        public int UsageMarkets { get; set; }
        public int ViewerMarkets { get; set; }
    }
}