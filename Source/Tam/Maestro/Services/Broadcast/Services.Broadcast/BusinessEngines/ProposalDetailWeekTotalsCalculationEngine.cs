using System;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalDetailWeekTotalsCalculationEngine
    {
        void CalculateWeekTotalsForOpenMarketInventory(ProposalOpenMarketInventoryWeekDto openMarketWeekTotals,
            ProposalDetailSingleWeekTotalsDto otherInventoryTotals, double margin);

        void CalculateWeekTotalsForProprietary(ProposalInventoryTotalsDto.InventoryWeek proprietaryWeekTotals,
            ProposalInventoryTotalsRequestDto.InventoryWeek proprietaryTotalsRequest,
            ProposalDetailSingleWeekTotalsDto otherInventoryTotals, double margin);
    }

    public class ProposalDetailWeekTotalsCalculationEngine : IProposalDetailWeekTotalsCalculationEngine
    {
        public void CalculateWeekTotalsForProprietary(ProposalInventoryTotalsDto.InventoryWeek proprietaryWeekTotals,
            ProposalInventoryTotalsRequestDto.InventoryWeek proprietaryTotalsRequest,
            ProposalDetailSingleWeekTotalsDto otherInventoryTotals, double margin)
        {
            var totals = CalculateTotals(proprietaryWeekTotals.Impressions, proprietaryWeekTotals.Budget,
                proprietaryTotalsRequest.ImpressionGoal, proprietaryTotalsRequest.Budget, margin,
                otherInventoryTotals);

            proprietaryWeekTotals.Impressions = totals.TotalImpressions;
            proprietaryWeekTotals.Budget = totals.TotalCost;
            proprietaryWeekTotals.ImpressionsPercent = totals.ImpressionsPercent;
            proprietaryWeekTotals.BudgetPercent = totals.BudgetPercent;

            proprietaryWeekTotals.BudgetMarginAchieved = proprietaryWeekTotals.BudgetPercent > 100;
            proprietaryWeekTotals.ImpressionsMarginAchieved = proprietaryWeekTotals.ImpressionsPercent > 100;
        }

        public void CalculateWeekTotalsForOpenMarketInventory(ProposalOpenMarketInventoryWeekDto openMarketWeekTotals,
            ProposalDetailSingleWeekTotalsDto otherInventoryTotals, double margin)
        {
            var totals = CalculateTotals(openMarketWeekTotals.ImpressionsTotal, openMarketWeekTotals.BudgetTotal, openMarketWeekTotals.ImpressionsGoal,
                openMarketWeekTotals.Budget, margin, otherInventoryTotals);

            openMarketWeekTotals.ImpressionsTotal = totals.TotalImpressions;
            openMarketWeekTotals.BudgetTotal = totals.TotalCost;
            openMarketWeekTotals.ImpressionsPercent = totals.ImpressionsPercent;
            openMarketWeekTotals.BudgetPercent = totals.BudgetPercent;

            openMarketWeekTotals.BudgetMarginAchieved = openMarketWeekTotals.BudgetPercent > 100;
            openMarketWeekTotals.ImpressionsMarginAchieved = openMarketWeekTotals.ImpressionsPercent > 100;
        }

        private ProposalDetailWeekTotalsDto CalculateTotals(double impressions, decimal cost, double targetImpressions, decimal targetCost, double margin, ProposalDetailSingleWeekTotalsDto otherInventoryTotals)
        {
            var totals = new ProposalDetailWeekTotalsDto();

            // totals
            totals.TotalImpressions = impressions + otherInventoryTotals.TotalImpressions;
            totals.TotalCost = Math.Round(cost + otherInventoryTotals.TotalCost, 2);

            // percent
            totals.BudgetPercent = ProposalMathEngine.CalculateBudgetPercent(totals.TotalCost, margin, targetCost);
            totals.ImpressionsPercent = ProposalMathEngine.CalculateImpressionsPercent(totals.TotalImpressions, targetImpressions);

            return totals;
        }
    }

    public class ProposalDetailWeekTotalsDto
    {
        public int MediaWeekId { get; set; }
        public decimal TotalCost { get; set; }
        public double BudgetPercent { get; set; }
        public double TotalImpressions { get; set; }
        public double ImpressionsPercent { get; set; }
    }
}
