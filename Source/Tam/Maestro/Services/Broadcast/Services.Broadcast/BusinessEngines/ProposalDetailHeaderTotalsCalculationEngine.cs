using System;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalDetailHeaderTotalsCalculationEngine
    {
        void CalculateTotalsForProprietaryInventory(ProposalInventoryTotalsDto proprietaryTotals, ProposalInventoryTotalsRequestDto proprietaryTotalsRequest, ProposalDetailSingleInventoryTotalsDto otherInventoryTotals);
        void CalculateTotalsForOpenMarketInventory(ProposalDetailOpenMarketInventoryDto openMarketTotals,
            ProposalDetailSingleInventoryTotalsDto otherInventoryTotals);
    }

    public class ProposalDetailHeaderTotalsCalculationEngine : IProposalDetailHeaderTotalsCalculationEngine
    {
        public void CalculateTotalsForProprietaryInventory(ProposalInventoryTotalsDto proprietaryTotals,
            ProposalInventoryTotalsRequestDto proprietaryTotalsRequest,
            ProposalDetailSingleInventoryTotalsDto otherInventoryTotals)
        {
            var totals = CalculateTotals(proprietaryTotals.TotalImpressions, proprietaryTotals.TotalCost,
                proprietaryTotalsRequest.DetailTargetImpressions,
                proprietaryTotalsRequest.DetailTargetBudget, proprietaryTotalsRequest.DetailCpm,
                otherInventoryTotals);

            proprietaryTotals.TotalImpressions = totals.TotalImpressions;
            proprietaryTotals.TotalCost = totals.TotalCost;
            proprietaryTotals.TotalCpm = totals.TotalCpm;
            proprietaryTotals.ImpressionsPercent = totals.ImpressionsPercent;
            proprietaryTotals.BudgetPercent = totals.BudgetPercent;
            proprietaryTotals.CpmPercent = totals.CpmPercent;
        }

        public void CalculateTotalsForOpenMarketInventory(ProposalDetailOpenMarketInventoryDto openMarketTotals,
            ProposalDetailSingleInventoryTotalsDto otherInventoryTotals)
        {
            var targetImpressions = openMarketTotals.DetailTargetImpressions.HasValue ? openMarketTotals.DetailTargetImpressions.Value : 0d;
            var targetBudget = openMarketTotals.DetailTargetBudget.HasValue ? openMarketTotals.DetailTargetBudget.Value : 0m;
            var targetCpm = openMarketTotals.DetailCpm.HasValue ? openMarketTotals.DetailCpm.Value : 0m;

            var totals = CalculateTotals(openMarketTotals.DetailTotalImpressions, openMarketTotals.DetailTotalBudget, targetImpressions,
                targetBudget, targetCpm, otherInventoryTotals);

            openMarketTotals.DetailTotalImpressions = totals.TotalImpressions;
            openMarketTotals.DetailTotalBudget = totals.TotalCost;
            openMarketTotals.DetailTotalCpm = totals.TotalCpm;
            openMarketTotals.DetailImpressionsPercent = totals.ImpressionsPercent;
            openMarketTotals.DetailBudgetPercent = totals.BudgetPercent;
            openMarketTotals.DetailCpmPercent = totals.CpmPercent;
        }

        private ProposalDetailHeaderTotalsDto CalculateTotals(double impressionsInThousands, decimal cost, double targetImpressionsInThousands,
            decimal targetCost, decimal targetCpm, ProposalDetailSingleInventoryTotalsDto otherInventoryTotals)
        {
            var totals = new ProposalDetailHeaderTotalsDto();
            var impressions = (long)(impressionsInThousands * 1000);
            var roundedImpressions = impressions / 1000.0;
            var targetImpressions = (long)(targetImpressionsInThousands * 1000);

            totals.TotalImpressions = Math.Round(roundedImpressions + otherInventoryTotals.TotalImpressions, 3);
            totals.TotalCost = Math.Round(cost + otherInventoryTotals.TotalCost, 2);
            totals.TotalCpm = (decimal) totals.TotalImpressions == 0
                ? 0
                : Math.Round(totals.TotalCost / (decimal) totals.TotalImpressions, 2);
            totals.BudgetPercent = targetCost == 0
                ? 0
                : (double) Math.Round(totals.TotalCost * 100 / targetCost, 2);
            totals.ImpressionsPercent = targetImpressions == 0
                ? 0
                : Math.Round(totals.TotalImpressions * 100 / (targetImpressions / 1000.0), 2);
            totals.CpmPercent = targetCpm == 0 ? 0 : (double) Math.Round(totals.TotalCpm * 100 / targetCpm, 2);

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
