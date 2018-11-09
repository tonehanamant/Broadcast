using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;

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
            var targetCost = proposal.TargetBudget ?? 0;
            var margin = proposal.Margin ?? ProposalConstants.ProposalDefaultMargin;

            proposal.TotalImpressionsPercent = ProposalMath.CalculateImpressionsPercent(proposal.TotalImpressions, proposal.TargetImpressions);
            proposal.TotalCostPercent = ProposalMath.CalculateBudgetPercent(proposal.TotalCost, margin, targetCost);
            proposal.TotalCPMPercent = ProposalMath.CalculateCpmPercent(proposal.TotalCost, proposal.TotalImpressions, targetCost, proposal.TargetImpressions, margin);

            proposal.TotalImpressionsMarginAchieved = proposal.TotalImpressionsPercent > 100;
            proposal.TotalCostMarginAchieved = proposal.TotalCostPercent > 100;
            proposal.TotalCPMMarginAchieved = proposal.TotalCPMPercent > 100;
        }
    }
}
