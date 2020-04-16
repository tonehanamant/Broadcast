using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Helpers
{
    public static class FrequencyCapHelper
    {
        public static (double capTime, string capType) GetFrequencyCapTimeAndCapTypeString(UnitCapEnum unitCap)
        {
            if (unitCap == UnitCapEnum.Per30Min)
                return (capTime: 0.5d, "hour");

            if (unitCap == UnitCapEnum.PerHour)
                return (capTime: 1d, "hour");

            if (unitCap == UnitCapEnum.PerDay)
                return (capTime: 1d, "day");

            if (unitCap == UnitCapEnum.PerWeek)
                return (capTime: 1d, "week");

            throw new ApplicationException("Unsupported unit cap type was discovered");
        }
    }
}
