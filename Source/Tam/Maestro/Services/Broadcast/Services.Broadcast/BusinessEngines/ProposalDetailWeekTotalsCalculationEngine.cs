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
        private readonly IProposalMathEngine _proposalMathEngine;

        public ProposalDetailWeekTotalsCalculationEngine(IProposalMathEngine proposalMathEngine)
        {
            _proposalMathEngine = proposalMathEngine;
        }

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
        }

        private ProposalDetailWeekTotalsDto CalculateTotals(double impressionsInThousands, decimal cost,
            double targetImpressionsInThousands, decimal targetCost, double margin,
            ProposalDetailSingleWeekTotalsDto otherInventoryTotals)
        {
            var totals = new ProposalDetailWeekTotalsDto();
            var impressions = (long)(impressionsInThousands * 1000);
            var roundedImpressions = impressions / 1000.0;
            var targetImpressions = (long)(targetImpressionsInThousands * 1000);

            // totals
            totals.TotalImpressions = Math.Round(roundedImpressions + otherInventoryTotals.TotalImpressions, 3);
            totals.TotalCost = Math.Round(cost + otherInventoryTotals.TotalCost, 2);

            // percent
            totals.BudgetPercent = _proposalMathEngine.CalculateBudgetPercent((double)totals.TotalCost, margin, (double)targetCost);
            totals.ImpressionsPercent = _proposalMathEngine.CalculateImpressionsPercent(totals.TotalImpressions, targetImpressions);

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
