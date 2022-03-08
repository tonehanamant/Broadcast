using System;

namespace Services.Broadcast.BusinessEngines
{
    public static class ProposalMath
    {
        public static double CalculateBudgetPercent(decimal total, double margin, decimal goal)
        {
            return (double)(goal == 0 ? 0 : (total + total * (decimal)(margin / 100)) * 100 / goal);
        }

        public static double CalculateImpressionsPercent(double totalImpressions, double targetImpressions)
        {
            return targetImpressions == 0 ? 0 : totalImpressions * 100 / targetImpressions;
        }

        public static double CalculateCpmPercent(decimal totalCost, double totalImpression, decimal targetBudget, double targetImpression, double margin)
        {
            if (totalImpression == 0 || targetImpression == 0 || targetBudget == 0) return 0;

            // working cpm with margin
            var workingCpm = (totalCost + (totalCost * ((decimal)margin / 100))) / (decimal)(totalImpression);
            var proposalCpm = targetBudget / (decimal)targetImpression;

            return (double)(workingCpm / proposalCpm) * 100;
        }

        public static decimal CalculateCpm(decimal totalCost, double totalImpressions)
        {
            return totalImpressions == 0 ? 0 : (totalCost / (decimal)totalImpressions) * 1000;
        }
        public static double CalculateVPVH(double projectedImpressions, double householdProjectedImpressions)
        {
            return projectedImpressions / (householdProjectedImpressions == 0.0 ? 1 : householdProjectedImpressions);
        }
        public static decimal CalculateCost(decimal cpm, double impressions)
        {
            return cpm * (decimal)impressions / 1000;
        }

        public static float CalculateTRP_GRP(double targetImpressions, double universe)
        {
            return (float)(universe != 0 ? targetImpressions / universe * 100 : 0);
        }

        public static decimal CalculateManifestSpotAudienceRate(double impressionsPerSpot, decimal spotCost)
        {
            if (Math.Abs(impressionsPerSpot) < 0.001)
                return 0;

            return (spotCost / (decimal) impressionsPerSpot) * 1000;
        }

        public static double CalculateAvgImpressions(double totalImpressions, int spots)
        {
            return spots == 0 ? 0 : totalImpressions / spots;
        }

        public static double CalculateImpressionsPercentage(double impressions, double totalImpressions)
        {
            return totalImpressions == 0 ? 0 : impressions / totalImpressions * 100;
        }

        public static double RoundDownWithDecimals(double i, double decimalPlaces)
        {
            var power = Math.Pow(10, decimalPlaces);
            return Math.Floor(i * power) / power;
        }
        public static double RoundUpWithDecimals(double i, double decimalPlaces)
        {
            var power = Math.Pow(10, decimalPlaces);
            return Math.Round(i * power) / power;
        }
        public static double CalculateHhImpressionsUsingVpvh(double audienceImpressions, double audienceVpvh)
        {
            return audienceVpvh == 0 ? 0 : audienceImpressions / audienceVpvh;
        }

        public static double CalculateAudienceImpressionsUsingVpvh(double hhImpressions, double audienceVpvh)
        {
            return hhImpressions * audienceVpvh;
        }

        public static double CalculateVpvh(double audienceImpressions, double hhImpressions)
        {
            return hhImpressions == 0 ? 0 : audienceImpressions / hhImpressions;
        }
    }
}
