using System.Collections.Generic;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalOpenMarketsTotalsCalculationEngine : IApplicationService
    {
        void CalculatePartialOpenMarketTotals(ProposalDetailOpenMarketInventoryDto dto);
        void CalculateOpenMarketDetailTotals(ProposalDetailOpenMarketInventoryDto dto,
            ProposalDetailSingleInventoryTotalsDto proposalDetailSingleInventoryTotalsDto,
            List<ProposalDetailSingleWeekTotalsDto> proposalDetailInventoryWeekTotalsDtos);
        void CalculateOpenMarketProgramTotals(ProposalDetailOpenMarketInventoryDto dto);
    }

    public class ProposalOpenMarketsTotalsCalculationEngine : IProposalOpenMarketsTotalsCalculationEngine
    {
        private readonly IProposalDetailHeaderTotalsCalculationEngine _ProposalDetailTotalsCalculationEngine;
        private readonly IProposalDetailWeekTotalsCalculationEngine _ProposalDetailWeekTotalsCalculationEngine;

        public ProposalOpenMarketsTotalsCalculationEngine(
            IProposalDetailHeaderTotalsCalculationEngine proposalDetailTotalsCalculationEngine,
            IProposalDetailWeekTotalsCalculationEngine proposalDetailWeekTotalsCalculationEngine)
        {
            _ProposalDetailTotalsCalculationEngine = proposalDetailTotalsCalculationEngine;
            _ProposalDetailWeekTotalsCalculationEngine = proposalDetailWeekTotalsCalculationEngine;
        }

        public void CalculatePartialOpenMarketTotals(ProposalDetailOpenMarketInventoryDto dto)
        {
            _CalculateMarketsTotals(dto);

            foreach (var inventoryWeek in dto.Weeks)
            {
                _CalculateWeekTotals(dto, inventoryWeek);
            }

            _CalculatePartialDetailTotals(dto);
        }

        public void CalculateOpenMarketDetailTotals(ProposalDetailOpenMarketInventoryDto dto,
            ProposalDetailSingleInventoryTotalsDto proposalDetailSingleInventoryTotalsDto,
            List<ProposalDetailSingleWeekTotalsDto> proposalDetailInventoryWeekTotalsDto)
        {
            _ClearTotals(dto);

            _CalculateMarketsTotals(dto);

            foreach (var inventoryWeek in dto.Weeks)
            {
                _CalculateWeekTotals(dto, inventoryWeek);

                _UpdateTotalsFromOpenMarketWeek(dto, inventoryWeek);

                var currentWeekTotals =
                    proposalDetailInventoryWeekTotalsDto.FirstOrDefault(w => w.MediaWeekId == inventoryWeek.MediaWeekId) ??
                    new ProposalDetailSingleWeekTotalsDto();

                _ProposalDetailWeekTotalsCalculationEngine.CalculateWeekTotalsForOpenMarketInventory(
                    inventoryWeek, currentWeekTotals);
            }

            _ProposalDetailTotalsCalculationEngine.CalculateTotalsForOpenMarketInventory(dto,
                proposalDetailSingleInventoryTotalsDto);

            _SetAchievedMarginForTotals(dto);
        }

        private void _ClearTotals(ProposalDetailOpenMarketInventoryDto dto)
        {
            dto.DetailTotalImpressions = 0;
            dto.DetailTotalBudget = 0;

            foreach (var marketWeek in dto.Weeks)
            {
                foreach (var market in marketWeek.Markets)
                {
                    market.Cost = 0;
                    market.Impressions = 0;
                }
            }
        }

        private void _UpdateTotalsFromOpenMarketWeek(ProposalDetailOpenMarketInventoryDto dto, ProposalOpenMarketInventoryWeekDto inventoryWeek)
        {
            dto.DetailTotalImpressions += inventoryWeek.ImpressionsTotal;
            dto.DetailTotalBudget += inventoryWeek.BudgetTotal;
        }

        public void CalculateOpenMarketProgramTotals(ProposalDetailOpenMarketInventoryDto dto)
        {
            var weekPrograms =
                dto.Weeks.SelectMany(
                    week =>
                        week.Markets.SelectMany(
                            market =>
                                market.Stations.SelectMany(
                                    station => station.Programs.Where(a => a != null)))).ToList();

            foreach (var program in weekPrograms)
            {
                program.TotalImpressions = program.Spots == 0 ? program.UnitImpression : program.Spots * program.UnitImpression;
                program.Cost = program.Spots == 0 ? program.UnitCost : program.Spots * program.UnitCost;
            }
        }

        private static void _SetAchievedMarginForTotals(ProposalDetailOpenMarketInventoryDto dto)
        {
            dto.DetailBudgetMarginAchieved = _HasMarginForBudgetBeenAchieved((double)dto.DetailTotalBudget, dto.Margin, (decimal)dto.DetailTargetBudget);
            dto.DetailImpressionsMarginAchieved = _HasMarginForImpressionsBeenAchieved((double)dto.DetailTotalImpressions, dto.DetailTargetImpressions);
            dto.DetailCpmMarginAchieved = _HasMarginForCPMBeenAchieved(dto.DetailTotalImpressions, (double)dto.DetailTotalBudget, dto.Margin);
        }

        private static void _CalculatePartialDetailTotals(ProposalDetailOpenMarketInventoryDto dto)
        {
            dto.DetailTotalImpressions = dto.Weeks.Sum(w => w.ImpressionsTotal);
            dto.DetailTotalBudget = dto.Weeks.Sum(w => w.BudgetTotal);
        }

        private static void _CalculateWeekTotals(ProposalDetailOpenMarketInventoryDto dto,
            ProposalOpenMarketInventoryWeekDto inventoryWeek)
        {
            inventoryWeek.BudgetTotal = inventoryWeek.Markets.Sum(s => s.Cost);
            inventoryWeek.BudgetPercent = inventoryWeek.Budget == 0
                ? 0
                : (float)(inventoryWeek.BudgetTotal * 100 / inventoryWeek.Budget);
            inventoryWeek.BudgetMarginAchieved = _HasMarginForBudgetBeenAchieved((double)inventoryWeek.BudgetTotal,
                dto.Margin, inventoryWeek.Budget);

            inventoryWeek.ImpressionsTotal = inventoryWeek.Markets.Sum(a => a.Impressions);
            inventoryWeek.ImpressionsPercent = inventoryWeek.ImpressionsGoal == 0
                ? 0
                : (float)(inventoryWeek.ImpressionsTotal * 100 / inventoryWeek.ImpressionsGoal);
            inventoryWeek.ImpressionsMarginAchieved = _HasMarginForImpressionsBeenAchieved(inventoryWeek.ImpressionsTotal, inventoryWeek.ImpressionsGoal);
        }

        private static void _CalculateMarketsTotals(ProposalDetailOpenMarketInventoryDto dto)
        {
            foreach (var marketWeek in dto.Weeks)
            {
                foreach (var market in marketWeek.Markets)
                {
                    market.Cost =
                        market.Stations.Select(a => a.Programs.Where(p => p != null && p.Spots > 0).Sum(b => b.Cost)).Sum();
                    market.Spots = market.Stations.Select(a => a.Programs.Where(p => p != null).Sum(b => b.Spots)).Sum();
                    market.Impressions =
                        market.Stations.Select(a => a.Programs.Where(p => p != null && p.Spots > 0).Sum(p => p.TotalImpressions))
                            .Sum();
                }
            }
        }

        private static bool _HasMarginForCPMBeenAchieved(double totalImpressions, double totalCost, double? margin)
        {
//color indicator: 
//> 100% RED
//< 100% Green
//based on working cpm with margin
//= Total Impression /  (Total Cost +(Total Cost*0.2)) * 1000

            var divValue = (totalCost + (totalCost*(margin/100)))*1000;
            if (divValue == 0) return false;

            return (totalImpressions / divValue) > 100;
        }

        private static bool _HasMarginForBudgetBeenAchieved(double total, double? margin, decimal goal)
        {
// Budget Delivery % = (Total Cost + (total cost* margin)) * 100 / Target Budget 
//color indicator: 
//> 100% RED
//< 100% Green

            if (goal == 0) return false;

            return (total + (total * (margin / 100))) * 100 / (double)goal > 100;
        }

        private static bool _HasMarginForImpressionsBeenAchieved(double total, double? goal)
        {
//Impression Delivery: Total Impressions Delivery  * 100 / Proposal Detail Impression Goal 
//color indicator: 
//< 100% RED
//> 100% Green
            double goalDiv = goal.HasValue ? goal.Value : 0;

            if (goalDiv == 0) return false;

            return ((total * 100) / goalDiv) > 100;
        }
    }
}
