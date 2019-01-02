using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Extensions
{
    public static class MediaMonthExtensions
    {
        public static string GetShortMonthNameAndYear(this MediaMonth mediaMonth)
        {
            return $"{mediaMonth.Abbreviation} {mediaMonth.Year}";
        }

        public static string GetCompactMonthNameAndYear(this MediaMonth mediaMonth)
        {
            return $"{(mediaMonth.Month <=9 ? $"0{mediaMonth.Month}" : mediaMonth.Month.ToString())}{mediaMonth.Year.ToString().Substring(2)}";
        }
    }
}
