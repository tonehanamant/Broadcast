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
        private readonly IProposalMathEngine _ProposalMathEngine;

        public ProposalTotalsCalculationEngine(IProposalMathEngine proposalMathEngine)
        {
            _ProposalMathEngine = proposalMathEngine;
        }

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
            var targetImpressions = (long)((proposal.TargetImpressions ?? 0) * 1000);
            var targetCost = proposal.TargetBudget ?? 0;
            var targetCpm = proposal.TargetCPM ?? 0;
            var margin = proposal.Margin ?? ProposalConstants.ProposalDefaultMargin;

            proposal.TotalImpressionsPercent = _ProposalMathEngine.CalculateImpressionsPercent(
                proposal.TotalImpressions, targetImpressions);
            proposal.TotalCostPercent = _ProposalMathEngine.CalculateBudgetPercent((double) proposal.TotalCost, margin,
                (double)targetCost);
            proposal.TotalCPMPercent = _ProposalMathEngine.CalculateCpmPercent((double)proposal.TotalCPM, margin, (double)targetCpm);

            proposal.TotalImpressionsMarginAchieved = proposal.TotalImpressionsPercent > 100;
            proposal.TotalCostMarginAchieved = proposal.TotalCostPercent > 100;
            proposal.TotalCPMMarginAchieved = proposal.TotalCPMPercent > 100;
        }
    }
}
