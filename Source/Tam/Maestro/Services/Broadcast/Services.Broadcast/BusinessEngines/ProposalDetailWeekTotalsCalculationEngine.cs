using System;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalDetailWeekTotalsCalculationEngine
    {
        void CalculateWeekTotalsForOpenMarketInventory(ProposalOpenMarketInventoryWeekDto openMarketWeekTotals, ProposalDetailSingleWeekTotalsDto otherInventoryTotals);
        void CalculateWeekTotalsForProprietary(ProposalInventoryTotalsDto.InventoryWeek proprietaryWeekTotals, ProposalInventoryTotalsRequestDto.InventoryWeek proprietaryTotalsRequest, ProposalDetailSingleWeekTotalsDto otherInventoryTotals);
    }

    public class ProposalDetailWeekTotalsCalculationEngine : IProposalDetailWeekTotalsCalculationEngine
    {
        public void CalculateWeekTotalsForProprietary(ProposalInventoryTotalsDto.InventoryWeek proprietaryWeekTotals,
            ProposalInventoryTotalsRequestDto.InventoryWeek proprietaryTotalsRequest,
            ProposalDetailSingleWeekTotalsDto otherInventoryTotals)
        {
            var totals = CalculateTotals(proprietaryWeekTotals.Impressions, proprietaryWeekTotals.Budget,
                proprietaryTotalsRequest.ImpressionGoal, proprietaryTotalsRequest.Budget,
                otherInventoryTotals);

            proprietaryWeekTotals.Impressions = totals.TotalImpressions;
            proprietaryWeekTotals.Budget = totals.TotalCost;
            proprietaryWeekTotals.ImpressionsPercent = totals.ImpressionsPercent;
            proprietaryWeekTotals.BudgetPercent = totals.BudgetPercent;
        }

        public void CalculateWeekTotalsForOpenMarketInventory(ProposalOpenMarketInventoryWeekDto openMarketWeekTotals,
            ProposalDetailSingleWeekTotalsDto otherInventoryTotals)
        {
            var totals = CalculateTotals(openMarketWeekTotals.ImpressionsTotal, openMarketWeekTotals.BudgetTotal, openMarketWeekTotals.ImpressionsGoal,
                openMarketWeekTotals.Budget, otherInventoryTotals);

            openMarketWeekTotals.ImpressionsTotal = totals.TotalImpressions;
            openMarketWeekTotals.BudgetTotal = totals.TotalCost;
            openMarketWeekTotals.ImpressionsPercent = totals.ImpressionsPercent;
            openMarketWeekTotals.BudgetPercent = totals.BudgetPercent;
        }

        private ProposalDetailWeekTotalsDto CalculateTotals(double impressionsInThousands, decimal cost,
            double targetImpressionsInThousands, decimal targetCost,
            ProposalDetailSingleWeekTotalsDto otherInventoryTotals)
        {
            var totals = new ProposalDetailWeekTotalsDto();
            var impressions = (long)(impressionsInThousands * 1000);
            var roundedImpressions = impressions / 1000.0;
            var targetImpressions = (long)(targetImpressionsInThousands * 1000);

            totals.TotalImpressions = Math.Round(roundedImpressions + otherInventoryTotals.TotalImpressions, 3);
            totals.TotalCost = Math.Round(cost + otherInventoryTotals.TotalCost, 2);
            totals.BudgetPercent = targetCost == 0
                ? 0
                : (double)Math.Round(totals.TotalCost * 100 / targetCost, 2);
            totals.ImpressionsPercent = targetImpressions == 0
                ? 0
                : Math.Round(totals.TotalImpressions * 100 / (targetImpressions / 1000.0), 2);

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
