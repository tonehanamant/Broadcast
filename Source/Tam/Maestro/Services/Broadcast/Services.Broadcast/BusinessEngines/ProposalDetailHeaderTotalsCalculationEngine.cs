using System;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
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

            proprietaryTotals.ImpressionsMarginAchieved = proprietaryTotals.ImpressionsPercent > 100;
            proprietaryTotals.BudgetMarginAchieved = proprietaryTotals.BudgetPercent > 100;
            proprietaryTotals.CpmMarginAchieved = proprietaryTotals.CpmPercent > 100;
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

            openMarketTotals.DetailBudgetMarginAchieved = openMarketTotals.DetailBudgetPercent > 100;
            openMarketTotals.DetailCpmMarginAchieved = openMarketTotals.DetailCpmPercent > 100;
            openMarketTotals.DetailImpressionsMarginAchieved = openMarketTotals.DetailImpressionsPercent > 100;
        }

        private static ProposalDetailHeaderTotalsDto CalculateTotals(double impressions, decimal cost, double targetImpressions, decimal targetCost, decimal targetCpm, double margin, ProposalDetailSingleInventoryTotalsDto otherInventoryTotals)
        {
            var totals = new ProposalDetailHeaderTotalsDto();

            // totals
            totals.TotalImpressions = impressions + otherInventoryTotals.TotalImpressions;
            totals.TotalCost = cost + otherInventoryTotals.TotalCost;
            totals.TotalCpm = ProposalMath.CalculateCpm(totals.TotalCost, totals.TotalImpressions);

            // percent
            totals.BudgetPercent = ProposalMath.CalculateBudgetPercent(totals.TotalCost, margin, targetCost);
            totals.ImpressionsPercent = ProposalMath.CalculateImpressionsPercent(totals.TotalImpressions, targetImpressions);
            totals.CpmPercent = ProposalMath.CalculateCpmPercent(totals.TotalCost, totals.TotalImpressions, targetCost, targetImpressions, margin);

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
