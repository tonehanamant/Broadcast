using Common.Services.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalMathEngine : IApplicationService
    {
        double CalculateBudgetPercent(double total, double margin, double goal);
        double CalculateImpressionsPercent(double totalImpressions, double targetImpressions);
        double CalculateCpmPercent(double totalCpm, double margin, double targetCpm);
        decimal CalculateTotalCpm(double totalCost, double totalImpressions);
    }

    public class ProposalMathEngine : IProposalMathEngine
    {
        public double CalculateBudgetPercent(double total, double margin, double goal)
        {
            return goal == 0 ? 0 : Math.Round((total + (total * (margin / 100))) * 100 / goal, 2);
        }

        public double CalculateImpressionsPercent(double totalImpressions, double targetImpressions)
        {
            return targetImpressions == 0 ? 0 : Math.Round(totalImpressions * 100 / (targetImpressions / 1000.0), 2);
        }

        public double CalculateCpmPercent(double totalCpm, double margin, double targetCpm)
        {
            return targetCpm == 0 ? 0 : Math.Round((totalCpm + (totalCpm * (margin / 100))) * 100 / targetCpm, 2);
        }

        public decimal CalculateTotalCpm(double totalCost, double totalImpressions)
        {
            if (totalImpressions == 0) return 0;
            return  (decimal) Math.Round(totalCost / totalImpressions, 2);
        }
    }
}
