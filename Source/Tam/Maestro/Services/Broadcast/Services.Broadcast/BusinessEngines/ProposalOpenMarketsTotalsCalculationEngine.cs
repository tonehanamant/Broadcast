using System.Collections.Generic;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;
using System.Linq;
using System;

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
        private readonly IProposalMathEngine _proposalMathEngine;

        public ProposalOpenMarketsTotalsCalculationEngine(
            IProposalDetailHeaderTotalsCalculationEngine proposalDetailTotalsCalculationEngine,
            IProposalDetailWeekTotalsCalculationEngine proposalDetailWeekTotalsCalculationEngine,
            IProposalMathEngine proposalMathEngine)
        {
            _ProposalDetailTotalsCalculationEngine = proposalDetailTotalsCalculationEngine;
            _ProposalDetailWeekTotalsCalculationEngine = proposalDetailWeekTotalsCalculationEngine;
            _proposalMathEngine = proposalMathEngine;
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

                _ProposalDetailWeekTotalsCalculationEngine.CalculateWeekTotalsForOpenMarketInventory(inventoryWeek, currentWeekTotals, dto.Margin.Value);
            }

            _ProposalDetailTotalsCalculationEngine.CalculateTotalsForOpenMarketInventory(dto, proposalDetailSingleInventoryTotalsDto, dto.Margin.Value);
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

        private void _CalculatePartialDetailTotals(ProposalDetailOpenMarketInventoryDto dto)
        {
            dto.DetailTotalImpressions = dto.Weeks.Sum(w => w.ImpressionsTotal);
            dto.DetailTotalBudget = dto.Weeks.Sum(w => w.BudgetTotal);
        }

        private void _CalculateWeekTotals(ProposalDetailOpenMarketInventoryDto dto,
            ProposalOpenMarketInventoryWeekDto inventoryWeek)
        {
            // totals
            inventoryWeek.BudgetTotal = inventoryWeek.Markets.Sum(s => s.Cost);
            inventoryWeek.ImpressionsTotal = inventoryWeek.Markets.Sum(a => a.Impressions);

            // using same * as per week
            var targetImpressions = (long)(inventoryWeek.ImpressionsGoal * 1000);

            // percent
            inventoryWeek.BudgetPercent = _proposalMathEngine.CalculateBudgetPercent((double)inventoryWeek.BudgetTotal, dto.Margin.Value, (double)inventoryWeek.Budget);
            inventoryWeek.ImpressionsPercent = _proposalMathEngine.CalculateImpressionsPercent(inventoryWeek.ImpressionsTotal, targetImpressions);

            // margin achieved
            inventoryWeek.ImpressionsMarginAchieved = inventoryWeek.ImpressionsPercent > 100;
            inventoryWeek.BudgetMarginAchieved = inventoryWeek.BudgetPercent > 100;

        }

        private void _CalculateMarketsTotals(ProposalDetailOpenMarketInventoryDto dto)
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
    }
}
