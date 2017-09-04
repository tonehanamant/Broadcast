using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalTotalsCalculationEngine
    {
        ProposalHeaderTotalsDto SumAllDetailsTotals(List<ProposalDetailTotalsDto> allTotals);
        void CalculateProposalTotalsMargins(ProposalDto proposal);
    }

    public class ProposalTotalsCalculationEngine : IProposalTotalsCalculationEngine
    {
        public ProposalHeaderTotalsDto SumAllDetailsTotals(List<ProposalDetailTotalsDto> allTotals)
        {
            return new ProposalHeaderTotalsDto
            {
                ImpressionsTotal = allTotals.Sum(x => x.OpenMarketImpressionsTotal + x.ProprietaryImpressionsTotal),
                CostTotal = allTotals.Sum(x => x.OpenMarketCostTotal + x.ProprietaryCostTotal)
            };
        }

        public void CalculateProposalTotalsMargins(ProposalDto proposal)
        {
            var targetImpressions = proposal.TargetImpressions ?? 0;
            var targetCost = proposal.TargetBudget ?? 0;
            var targetCpm = proposal.TargetCPM ?? 0;
            var margin = proposal.Margin ?? ProposalConstants.ProposalDefaultMargin;
            proposal.TotalImpressionsPercent = Math.Abs(targetImpressions) < 0.0001d ? 0 : (proposal.TotalImpressions * 100 / targetImpressions);
            proposal.TotalCostPercent = targetCost == 0 ? 0 : (double)(proposal.TotalCost * 100 / targetCost);
            proposal.TotalCPMPercent = targetCpm == 0 ? 0 : (double)(proposal.TotalCPM * 100 / targetCpm);
            proposal.TotalImpressionsMarginAchieved = _HasMarginForImpressionsBeenAchieved(proposal.TotalImpressions, targetImpressions);
            proposal.TotalCostMarginAchieved = _HasMarginForBudgetBeenAchieved(proposal.TotalCost, (decimal)margin, targetCost);
            proposal.TotalCPMMarginAchieved = _HasMarginForCPMBeenAchieved(proposal.TotalImpressions, proposal.TotalCost, (decimal)margin);
        }

        private static bool _HasMarginForCPMBeenAchieved(double totalImpressionsParam, decimal totalCost, decimal? margin)
        {
            var totalImpressions = (long) (totalImpressionsParam / 1000);
            //color indicator: 
            //> 100% RED
            //< 100% Green
            //based on working cpm with margin
            //= Total Impression /  (Total Cost +(Total Cost*0.2)) * 1000

            var divValue = (totalCost + (totalCost * (margin / 100))) * 1000;
            if (divValue == 0) return false;

            return (totalImpressions / divValue) > 100;
        }

        private static bool _HasMarginForBudgetBeenAchieved(decimal total, decimal? margin, decimal goal)
        {
            // Budget Delivery % = (Total Cost + (total cost* margin)) * 100 / Target Budget 
            //color indicator: 
            //> 100% RED
            //< 100% Green

            if (goal == 0) return false;

            return (total + (total * (margin / 100))) * 100 / goal > 100;
        }

        private static bool _HasMarginForImpressionsBeenAchieved(double total, double? goal)
        {
            //Impression Delivery: Total Impressions Delivery  * 100 / Proposal Detail Impression Goal 
            //color indicator: 
            //< 100% RED
            //> 100% Green
            var goalDiv = goal.HasValue ? goal.Value : 0;

            if (goalDiv == 0) return false;

            return ((total * 100) / goalDiv) > 100;
        }
    }
}
