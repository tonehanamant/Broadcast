using Services.Broadcast.Entities.Enums;
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

        public CrunchStatusEnum Crunched
        {
            get
            {
                if (NielsonMarkets == 0)
                    return CrunchStatusEnum.NoMarkets;

                if (UniverseMarkets == NielsonMarkets &&
                    UsageMarkets == NielsonMarkets &&
                    ViewerMarkets == NielsonMarkets)
                    return CrunchStatusEnum.Crunched;

                if (UniverseMarkets > 0 ||
                    UsageMarkets > 0 ||
                    ViewerMarkets > 0)
                    return CrunchStatusEnum.Incomplete;

                return CrunchStatusEnum.NotCrunched;
            }
        }

        public override string ToString()
        {
            return MediaMonth.MediaMonthX + "("+ MediaMonth.Id + ") - " + Crunched;
        }
    }
}