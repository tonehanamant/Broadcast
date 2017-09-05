﻿using System;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalDetailHeaderTotalsCalculationEngine
    {
        void CalculateTotalsForProprietaryInventory(ProposalInventoryTotalsDto proprietaryTotals,
            ProposalInventoryTotalsRequestDto proprietaryTotalsRequest,
            ProposalDetailSingleInventoryTotalsDto otherInventoryTotals, double margin);

        void CalculateTotalsForOpenMarketInventory(ProposalDetailOpenMarketInventoryDto openMarketTotals,
            ProposalDetailSingleInventoryTotalsDto otherInventoryTotals, double margin);
    }

    public class ProposalDetailHeaderTotalsCalculationEngine : IProposalDetailHeaderTotalsCalculationEngine
    {
        private readonly IProposalMathEngine _proposalMathEngine;

        public ProposalDetailHeaderTotalsCalculationEngine(IProposalMathEngine proposalMathEngine)
        {
            _proposalMathEngine = proposalMathEngine;
        }

        public void CalculateTotalsForProprietaryInventory(ProposalInventoryTotalsDto proprietaryTotals,
            ProposalInventoryTotalsRequestDto proprietaryTotalsRequest,
            ProposalDetailSingleInventoryTotalsDto otherInventoryTotals, double margin)
        {
            var totals = CalculateTotals(proprietaryTotals.TotalImpressions, proprietaryTotals.TotalCost,
                proprietaryTotalsRequest.DetailTargetImpressions,
                proprietaryTotalsRequest.DetailTargetBudget, proprietaryTotalsRequest.DetailCpm, margin,
                otherInventoryTotals);

            proprietaryTotals.TotalImpressions = totals.TotalImpressions;
            proprietaryTotals.TotalCost = totals.TotalCost;
            proprietaryTotals.TotalCpm = totals.TotalCpm;
            proprietaryTotals.ImpressionsPercent = totals.ImpressionsPercent;
            proprietaryTotals.BudgetPercent = totals.BudgetPercent;
            proprietaryTotals.CpmPercent = totals.CpmPercent;

            proprietaryTotals.ImpressionsMarginAchieved = proprietaryTotals.ImpressionsPercent > 100;
            proprietaryTotals.BudgetMarginAchieved = proprietaryTotals.BudgetPercent > 100;
            proprietaryTotals.CpmMarginAchieved = proprietaryTotals.CpmPercent > 100;
        }

        public void CalculateTotalsForOpenMarketInventory(ProposalDetailOpenMarketInventoryDto openMarketTotals,
            ProposalDetailSingleInventoryTotalsDto otherInventoryTotals, double margin)
        {
            var targetImpressions = openMarketTotals.DetailTargetImpressions.HasValue ? openMarketTotals.DetailTargetImpressions.Value : 0d;
            var targetBudget = openMarketTotals.DetailTargetBudget.HasValue ? openMarketTotals.DetailTargetBudget.Value : 0m;
            var targetCpm = openMarketTotals.DetailCpm.HasValue ? openMarketTotals.DetailCpm.Value : 0m;

            var totals = CalculateTotals(openMarketTotals.DetailTotalImpressions, openMarketTotals.DetailTotalBudget, targetImpressions,
                targetBudget, targetCpm, margin, otherInventoryTotals);

            openMarketTotals.DetailTotalImpressions = totals.TotalImpressions;
            openMarketTotals.DetailTotalBudget = totals.TotalCost;
            openMarketTotals.DetailTotalCpm = totals.TotalCpm;
            openMarketTotals.DetailImpressionsPercent = totals.ImpressionsPercent;
            openMarketTotals.DetailBudgetPercent = totals.BudgetPercent;
            openMarketTotals.DetailCpmPercent = totals.CpmPercent;

            openMarketTotals.DetailBudgetMarginAchieved = openMarketTotals.DetailBudgetPercent > 100;
            openMarketTotals.DetailCpmMarginAchieved = openMarketTotals.DetailCpmPercent > 100;
            openMarketTotals.DetailImpressionsMarginAchieved = openMarketTotals.DetailImpressionsPercent > 100;
        }

        private ProposalDetailHeaderTotalsDto CalculateTotals(double impressionsInThousands, decimal cost, double targetImpressionsInThousands,
            decimal targetCost, decimal targetCpm, double margin, ProposalDetailSingleInventoryTotalsDto otherInventoryTotals)
        {
            var totals = new ProposalDetailHeaderTotalsDto();
            var impressions = (long)(impressionsInThousands * 1000);
            var roundedImpressions = impressions / 1000.0;
            var targetImpressions = (long)(targetImpressionsInThousands * 1000);

            // totals
            totals.TotalImpressions = Math.Round(roundedImpressions + otherInventoryTotals.TotalImpressions, 3);
            totals.TotalCost = Math.Round(cost + otherInventoryTotals.TotalCost, 2);
            totals.TotalCpm = _proposalMathEngine.CalculateTotalCpm((double)totals.TotalCost, totals.TotalImpressions);

            // percent
            totals.BudgetPercent = _proposalMathEngine.CalculateBudgetPercent((double)totals.TotalCost, margin, (double)targetCost);
            totals.ImpressionsPercent = _proposalMathEngine.CalculateImpressionsPercent(totals.TotalImpressions, targetImpressions);
            totals.CpmPercent = _proposalMathEngine.CalculateCpmPercent((double)totals.TotalCpm, margin, (double)targetCpm);

            return totals;
        }
    }

    public class ProposalDetailHeaderTotalsDto
    {
        public decimal TotalCost { get; set; }
        public double BudgetPercent { get; set; }
        public double TotalImpressions { get; set; }
        public double ImpressionsPercent { get; set; }
        public decimal TotalCpm { get; set; }
        public double CpmPercent { get; set; }
    }
}
