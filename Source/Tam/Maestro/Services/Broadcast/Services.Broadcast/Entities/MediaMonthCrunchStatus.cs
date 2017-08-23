using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities
{
    public struct MediaMonthCrunchStatus
    {
        public readonly MediaMonth MediaMonth;
        public readonly int NielsonMarkets;
        public readonly int UniverseMarkets;
        public readonly int UsageMarkets;
        public readonly int ViewerMarkets;

        public MediaMonthCrunchStatus(RatingsForecastStatus status, int nielsonMarkets)
        {
            MediaMonth = status.MediaMonth;
            UniverseMarkets = status.UniverseMarkets;
            UsageMarkets = status.UsageMarkets;
            ViewerMarkets = status.ViewerMarkets;
            NielsonMarkets = nielsonMarkets;
        }

        public CrunchStatus Crunched
        {
            get
            {
                if (NielsonMarkets == 0)
                    return CrunchStatus.NoMarkets;

                if (UniverseMarkets == NielsonMarkets &&
                    UsageMarkets == NielsonMarkets &&
                    ViewerMarkets == NielsonMarkets)
                    return CrunchStatus.Crunched;

                if (UniverseMarkets > 0 ||
                    UsageMarkets > 0 ||
                    ViewerMarkets > 0)
                    return CrunchStatus.Incomplete;

                return CrunchStatus.NotCrunched;
            }
        }

        public override string ToString()
        {
            return MediaMonth.MediaMonthX + " - " + Crunched;
        }
    }
}