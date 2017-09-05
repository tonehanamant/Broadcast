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
        }

        public void CalculateTotalsForOpenMarketInventory(ProposalDetailOpenMarketInventoryDto openMarketTotals, ProposalDetailSingleInventoryTotalsDto otherInventoryTotals, double margin)
        {
            var targetBudget = openMarketTotals.DetailTargetBudget.HasValue ? openMarketTotals.DetailTargetBudget.Value : 0m;

            var totals = CalculateTotals(openMarketTotals.DetailTotalImpressions, openMarketTotals.DetailTotalBudget, openMarketTotals.DetailTargetImpressions,
                targetBudget, openMarketTotals.DetailCpm, margin, otherInventoryTotals);

            openMarketTotals.DetailTotalImpressions = totals.TotalImpressions;
            openMarketTotals.DetailTotalBudget = totals.TotalCost;
            openMarketTotals.DetailTotalCpm = totals.TotalCpm;
            openMarketTotals.DetailImpressionsPercent = totals.ImpressionsPercent;
            openMarketTotals.DetailBudgetPercent = totals.BudgetPercent;
            openMarketTotals.DetailCpmPercent = totals.CpmPercent;
        }

        private ProposalDetailHeaderTotalsDto CalculateTotals(double impressions, decimal cost, double targetImpressions, decimal targetCost, decimal targetCpm, double margin, ProposalDetailSingleInventoryTotalsDto otherInventoryTotals)
        {
            var totals = new ProposalDetailHeaderTotalsDto();

            // totals
            totals.TotalImpressions = Math.Round(impressions + otherInventoryTotals.TotalImpressions, 3);
            totals.TotalCost = Math.Round(cost + otherInventoryTotals.TotalCost, 2);
            totals.TotalCpm = ProposalMathEngine.CalculateTotalCpm(totals.TotalCost, totals.TotalImpressions);

            // percent
            totals.BudgetPercent = ProposalMathEngine.CalculateBudgetPercent(totals.TotalCost, margin, targetCost);
            totals.ImpressionsPercent = ProposalMathEngine.CalculateImpressionsPercent(totals.TotalImpressions, targetImpressions);
            totals.CpmPercent = ProposalMathEngine.CalculateCpmPercent(totals.TotalCpm, margin, targetCpm);

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
