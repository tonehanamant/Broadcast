using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Extensions
{
    public static class MediaMonthExtensions
    {
        public static string GetShortMonthNameAndYear(this MediaMonth mediaMonth)
        {
            return $"{mediaMonth.Abbreviation} {mediaMonth.Year}";
        }
    }
}
