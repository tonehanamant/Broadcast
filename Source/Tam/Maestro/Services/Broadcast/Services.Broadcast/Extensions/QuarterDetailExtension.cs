
using Services.Broadcast.Entities;

namespace Services.Broadcast.Extensions
{
    public static class QuarterDetailExtension
    {
        /// <summary>
        /// Formats the quarter object to "Q4 '19"
        /// </summary>
        /// <param name="quarter">The quarter to be formated.</param>
        /// <returns>string</returns>
        public static string ShortFormat(this QuarterDetailDto quarter)
        {
            return $"Q{quarter.Quarter} '{quarter.Year.ToString().Substring(2)}";
        }

        /// <summary>
        /// Formats the quarter object to "Q4 2019"
        /// </summary>
        /// <param name="quarter">The quarter to be formated.</param>
        /// <returns>string</returns>
        public static string LongFormat(this QuarterDetailDto quarter)
        {
            return $"Q{quarter.Quarter} {quarter.Year}";
        }
    }
}
