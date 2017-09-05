using System;

namespace Services.Broadcast.BusinessEngines
{
    public static class ProposalMathEngine 
    {
        public static double CalculateBudgetPercent(decimal total, double margin, decimal goal)
        {
            return (double)(goal == 0 ? 0 : Math.Round((total + total * (decimal)(margin / 100)) * 100 / goal, 2));
        }

        public static double CalculateImpressionsPercent(double totalImpressions, double targetImpressions)
        {
            return targetImpressions == 0 ? 0 : Math.Round(totalImpressions * 100 / targetImpressions, 2);
        }

        public static double CalculateCpmPercent(decimal totalCpm, double margin, decimal targetCpm)
        {
            return (double)(targetCpm == 0 ? 0 : Math.Round((totalCpm + totalCpm * (decimal)(margin / 100)) * 100 / targetCpm, 2));
        }

        public static decimal CalculateTotalCpm(decimal totalCost, double totalImpressions)
        {
            if (totalImpressions == 0) return 0;
            return Math.Round(totalCost / (decimal)(totalImpressions / 1000), 2);
        }
    }
}
