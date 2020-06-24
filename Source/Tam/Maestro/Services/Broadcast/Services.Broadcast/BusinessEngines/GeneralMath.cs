using System;

namespace Services.Broadcast.BusinessEngines
{
    public static class GeneralMath
    {
        /// <summary>
        /// Converts the percentage to fraction.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        /// <returns>Fraction</returns>
        public static double ConvertPercentageToFraction(double percentage)
        {
            return percentage / 100;
        }

        /// <summary>
        /// Converts the fraction to percentage.
        /// </summary>
        /// <param name="fraction">The fraction.</param>
        /// <returns>Percentage</returns>
        public static double ConvertFractionToPercentage(double fraction)
        {
            return fraction * 100;
        }

        public static decimal ConvertFractionToPercentage(decimal fraction)
        {
	        return fraction * 100;
        }

        public static decimal CalculateCostWithMargin(decimal cost, double? margin)
        {
            if (!margin.HasValue)
                return cost;
            return cost / (decimal)(1.0 - (margin / 100.0));
        }
    }
}
