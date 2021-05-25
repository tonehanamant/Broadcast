using System;

namespace Services.Broadcast.Extensions
{
    public static class NumericExtensions
    {
        /// <summary>
        /// Rounds the same as the casting from C# Decimal type to Sql Money type.
        /// </summary>
        public static decimal? AsSqlTypeMoney(this decimal? candidate)
        {
            if (!candidate.HasValue)
            {
                return null;
            }

            var result = Math.Round(candidate.Value, decimals: 4);
            return result;
        }
    }
}
