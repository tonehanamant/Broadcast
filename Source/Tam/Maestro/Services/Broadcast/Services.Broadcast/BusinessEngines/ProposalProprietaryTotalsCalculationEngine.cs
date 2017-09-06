using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services.ApplicationServices;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalWeeklyTotalCalculationEngine : IApplicationService
    {
        ProposalInventoryTotalsDto CalculateProprietaryDetailTotals(ProposalInventoryTotalsRequestDto request,
            ProposalDetailSingleInventoryTotalsDto proposalDetailSingleInventoryTotalsDto,
            List<ProposalDetailSingleWeekTotalsDto> proposalDetailInventoryWeekTotalsDtos);
        ProposalInventoryTotalsDto CalculatePartialProprietaryTotals(ProposalDetailProprietaryInventoryDto request);
    }

    public class ProposalProprietaryTotalsCalculationEngine : IProposalWeeklyTotalCalculationEngine
    {
        private readonly IProposalDetailHeaderTotalsCalculationEngine _ProposalDetailTotalsCalculationEngine;
        private readonly IProposalDetailWeekTotalsCalculationEngine _ProposalDetailWeekTotalsCalculationEngine;
        private readonly IProposalMathEngine _proposalMathEngine;

        public ProposalProprietaryTotalsCalculationEngine(
            IProposalDetailHeaderTotalsCalculationEngine proposalDetailTotalsCalculationEngine,
            IProposalDetailWeekTotalsCalculationEngine proposalDetailWeekTotalsCalculationEngine,
            IProposalMathEngine proposalMathEngine)
        {
            _ProposalDetailTotalsCalculationEngine = proposalDetailTotalsCalculationEngine;
            _ProposalDetailWeekTotalsCalculationEngine = proposalDetailWeekTotalsCalculationEngine;
            _proposalMathEngine = proposalMathEngine;
        }

        public ProposalInventoryTotalsDto CalculateProprietaryDetailTotals(ProposalInventoryTotalsRequestDto request,
            ProposalDetailSingleInventoryTotalsDto proposalDetailSingleInventoryTotalsDto,
            List<ProposalDetailSingleWeekTotalsDto> proposalDetailInventoryWeekTotalsDtos)
        {
            var totals = new ProposalInventoryTotalsDto();

            foreach (var inventoryWeek in request.Weeks)
            {
                var weekTotals = new ProposalInventoryTotalsDto.InventoryWeek
                {
                    MediaWeekId = inventoryWeek.MediaWeekId,
                    Budget = inventoryWeek.Slots.Sum(s => s.Cost),
                    Impressions = inventoryWeek.Slots.Sum(s => s.Impressions)
                };

                totals.TotalCost += weekTotals.Budget;
                totals.TotalImpressions += weekTotals.Impressions;

                var currentWeekTotals =
                    proposalDetailInventoryWeekTotalsDtos.FirstOrDefault(w => w.MediaWeekId == inventoryWeek.MediaWeekId) ??
                    new ProposalDetailSingleWeekTotalsDto();

                _ProposalDetailWeekTotalsCalculationEngine.CalculateWeekTotalsForProprietary(weekTotals, inventoryWeek, currentWeekTotals, proposalDetailSingleInventoryTotalsDto.Margin.Value);

                totals.Weeks.Add(weekTotals);
            }

            _ProposalDetailTotalsCalculationEngine.CalculateTotalsForProprietaryInventory(totals, request, proposalDetailSingleInventoryTotalsDto, proposalDetailSingleInventoryTotalsDto.Margin.Value);

            return totals;
        }

        public ProposalInventoryTotalsDto CalculatePartialProprietaryTotals(ProposalDetailProprietaryInventoryDto request)
        {
            var totals = new ProposalInventoryTotalsDto();

            foreach (var inventoryWeek in request.Weeks)
            {
                var weekTotals = new ProposalInventoryTotalsDto.InventoryWeek
                {
                    MediaWeekId = inventoryWeek.MediaWeekId
                };

                foreach (var daypartGroup in inventoryWeek.DaypartGroups)
                {
                    foreach (var daypartSlot in daypartGroup.Value.DaypartSlots)
                    {
                        if (daypartSlot != null && daypartSlot.ProposalsAllocations != null &&
                            daypartSlot.ProposalsAllocations.Any(p => p.IsCurrentProposal))
                        {
                            weekTotals.Budget += daypartSlot.Cost;
                            weekTotals.Impressions += daypartSlot.Impressions;
                        }
                    }
                }

                weekTotals.BudgetPercent = _proposalMathEngine.CalculateBudgetPercent((double)weekTotals.Budget, request.Margin.Value, (double)inventoryWeek.Budget);
                weekTotals.ImpressionsPercent = _proposalMathEngine.CalculateImpressionsPercent(weekTotals.Impressions, inventoryWeek.ImpressionsGoal);

                weekTotals.BudgetMarginAchieved = weekTotals.BudgetPercent > 100;
                weekTotals.ImpressionsMarginAchieved = weekTotals.ImpressionsPercent > 100;

                totals.Weeks.Add(weekTotals);
            }

            // totals
            totals.TotalImpressions = totals.Weeks.Sum(w => w.Impressions);
            totals.TotalCost = totals.Weeks.Sum(w => w.Budget);
            totals.TotalCpm = _proposalMathEngine.CalculateTotalCpm(totals.TotalCost, totals.TotalImpressions); 

            // percent
            totals.ImpressionsPercent = _proposalMathEngine.CalculateImpressionsPercent(totals.TotalImpressions, request.DetailTargetImpressions.Value);
            totals.BudgetPercent = _proposalMathEngine.CalculateBudgetPercent((double)totals.TotalCost, request.Margin.Value, (double)request.DetailTargetBudget.Value);
            totals.CpmPercent = _proposalMathEngine.CalculateCpmPercent(totals.TotalCost, totals.TotalImpressions, request.DetailTargetBudget.Value, request.DetailTargetImpressions.Value, request.Margin.Value);
            
            
            // margin
            totals.BudgetMarginAchieved = totals.BudgetPercent > 100;
            totals.ImpressionsMarginAchieved = totals.ImpressionsPercent > 100;
            totals.CpmMarginAchieved = totals.CpmPercent > 100;

            return totals;
        }
    }
}
