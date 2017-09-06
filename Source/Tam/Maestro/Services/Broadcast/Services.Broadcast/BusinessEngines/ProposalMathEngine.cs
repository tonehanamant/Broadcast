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
        decimal CalculateTotalCpm(decimal totalCost, double totalImpressions);
        double CalculateCpmPercent(decimal totalCost, double totalImpression, decimal targetBudget, double targetImpression, double margin);
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

        public double CalculateCpmPercent(decimal totalCost, double totalImpression, decimal targetBudget, double targetImpression, double margin)
        {
            if (totalImpression == 0 || targetImpression == 0) return 0;

            // working cpm with margin
            var workingCpm = (totalCost + (totalCost * ((decimal)margin / 100))) / (decimal)(totalImpression);
            var proposalCpm = targetBudget / (decimal)targetImpression;

            return (double)Math.Round((workingCpm / proposalCpm) * 100, 2);
        }

        public decimal CalculateTotalCpm(decimal totalCost, double totalImpressions)
        {
            if (totalImpressions == 0) return 0;
            return  Math.Round(totalCost / (decimal)totalImpressions, 2);
        }
    }
}
