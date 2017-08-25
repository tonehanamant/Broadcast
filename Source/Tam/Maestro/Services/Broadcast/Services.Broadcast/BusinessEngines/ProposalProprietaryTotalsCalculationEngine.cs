using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services.ApplicationServices;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
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

        public ProposalProprietaryTotalsCalculationEngine(
            IProposalDetailHeaderTotalsCalculationEngine proposalDetailTotalsCalculationEngine,
            IProposalDetailWeekTotalsCalculationEngine proposalDetailWeekTotalsCalculationEngine)
        {
            _ProposalDetailTotalsCalculationEngine = proposalDetailTotalsCalculationEngine;
            _ProposalDetailWeekTotalsCalculationEngine = proposalDetailWeekTotalsCalculationEngine;
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

                _ProposalDetailWeekTotalsCalculationEngine.CalculateWeekTotalsForProprietary(weekTotals, inventoryWeek,
                    currentWeekTotals, proposalDetailSingleInventoryTotalsDto.Margin.Value);

                weekTotals.BudgetMarginAchieved = _HasMarginForBudgetBeenAchieved((double) weekTotals.Budget,
                    proposalDetailSingleInventoryTotalsDto.Margin, inventoryWeek.Budget);
                weekTotals.ImpressionsMarginAchieved = _HasMarginForImpressionsBeenAchieved(weekTotals.Impressions, inventoryWeek.ImpressionGoal);

                totals.Weeks.Add(weekTotals);
            }

            _ProposalDetailTotalsCalculationEngine.CalculateTotalsForProprietaryInventory(totals, request, proposalDetailSingleInventoryTotalsDto, proposalDetailSingleInventoryTotalsDto.Margin.Value);

            totals.BudgetMarginAchieved = _HasMarginForBudgetBeenAchieved((double)totals.TotalCost, proposalDetailSingleInventoryTotalsDto.Margin, request.DetailTargetBudget);
            totals.ImpressionsMarginAchieved = _HasMarginForImpressionsBeenAchieved(totals.TotalImpressions, request.DetailTargetImpressions);
            totals.CpmMarginAchieved = _HasMarginForCPMBeenAchieved(totals.TotalImpressions, (double)totals.TotalCost, proposalDetailSingleInventoryTotalsDto.Margin);

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
                
                weekTotals.BudgetPercent = inventoryWeek.Budget == 0 ? 0 : (float)(weekTotals.Budget * 100 / inventoryWeek.Budget);
                weekTotals.ImpressionsPercent = inventoryWeek.ImpressionsGoal == 0 ? 0 : (float)(weekTotals.Impressions * 100 / inventoryWeek.ImpressionsGoal);
                weekTotals.BudgetMarginAchieved = _HasMarginForBudgetBeenAchieved((double)weekTotals.Budget, request.Margin, inventoryWeek.Budget);
                weekTotals.ImpressionsMarginAchieved = _HasMarginForImpressionsBeenAchieved(weekTotals.Impressions, inventoryWeek.ImpressionsGoal);

                totals.Weeks.Add(weekTotals);
            }

            totals.TotalImpressions = totals.Weeks.Sum(w => w.Impressions);
            totals.ImpressionsPercent = request.DetailTargetImpressions == 0 ? 0 : (float)(totals.TotalImpressions * 100 / request.DetailTargetImpressions);
            totals.TotalCost = totals.Weeks.Sum(w => w.Budget);
            totals.BudgetPercent = request.DetailTargetBudget == 0 ? 0 : (float)(totals.TotalCost * 100 / request.DetailTargetBudget);
            totals.TotalCpm = totals.TotalImpressions == 0 ? 0 : totals.TotalCost / (decimal)totals.TotalImpressions;
            totals.CpmPercent = request.DetailCpm == 0 ? 0 : (float)(totals.TotalCpm * 100 / request.DetailCpm);
            totals.BudgetMarginAchieved = _HasMarginForBudgetBeenAchieved((double)totals.TotalCost, request.Margin, (decimal)request.DetailTargetBudget);
            totals.ImpressionsMarginAchieved = _HasMarginForImpressionsBeenAchieved(totals.TotalImpressions, request.DetailTargetImpressions);
            totals.CpmMarginAchieved = _HasMarginForCPMBeenAchieved(totals.TotalImpressions, (double)totals.TotalCost, request.Margin);

            return totals;
        }


        private static bool _HasMarginForCPMBeenAchieved(double totalImpressions, double totalCost, double? margin)
        {
            //color indicator: 
            //> 100% RED
            //< 100% Green
            //based on working cpm with margin
            //= Total Impression /  (Total Cost +(Total Cost*0.2)) * 1000

            var divValue = (totalCost + (totalCost * (margin / 100))) * 1000;
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
