using Services.Broadcast.Entities;
using System;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalCalculationEngine
    {
        void UpdateProposal(ProposalDto proposalDto);
    }

    public class ProposalCalculationEngine : IProposalCalculationEngine
    {
        public void UpdateProposal(ProposalDto proposalDto)
        {
            foreach (var proposalDetailDto in proposalDto.Details)
            {
                foreach (var proposalQuarterDto in proposalDetailDto.Quarters)
                {
                    _UpdateProposalWeekValues(proposalQuarterDto, proposalDetailDto.Adu);

                    _UpdateQuarterValues(proposalQuarterDto, proposalDetailDto.Adu);
                }

                _SetQuarterTotals(proposalDetailDto);
            }

            _SetProposalTargets(proposalDto);
        }

        internal static void _UpdateQuarterValues(ProposalQuarterDto proposalQuarterDto, bool adu)
        {
            if (adu)
            {
                proposalQuarterDto.Cpm = 0;
            }

            if (!proposalQuarterDto.DistributeGoals)
            {
                var weeklyImpressions = proposalQuarterDto.Weeks.Select(week => week.Impressions);

                proposalQuarterDto.ImpressionGoal = weeklyImpressions.Sum();
            }
        }

        private static void _SetProposalTargets(ProposalDto proposalDto)
        {
            proposalDto.TargetBudget = proposalDto.Details.Sum(detail => detail.TotalCost);
            proposalDto.TargetImpressions = proposalDto.Details.Sum(detail => detail.TotalImpressions);
            proposalDto.TargetUnits = proposalDto.Details.Sum(detail => detail.TotalUnits);
            proposalDto.TargetCPM = proposalDto.TargetImpressions == 0 ? 0 : proposalDto.TargetBudget / (decimal)proposalDto.TargetImpressions;
        }

        internal static void _SetQuarterTotals(ProposalDetailDto proposalDetailDto)
        {
            var proposalQuarterTotalsDto = new ProposalQuarterTotals();

            foreach (var proposalQuarterDto in proposalDetailDto.Quarters)
            {
                proposalQuarterTotalsDto.TotalCost += proposalQuarterDto.Weeks.Sum(week => week.Cost);
                proposalQuarterTotalsDto.TotalUnits += proposalQuarterDto.Weeks.Sum(week => week.Units);
                proposalQuarterTotalsDto.TotalImpressions += proposalQuarterDto.Weeks.Sum(week => week.Impressions);
            }

            proposalDetailDto.TotalCost = proposalQuarterTotalsDto.TotalCost;
            proposalDetailDto.TotalImpressions = proposalQuarterTotalsDto.TotalImpressions;
            proposalDetailDto.TotalUnits = proposalQuarterTotalsDto.TotalUnits;
        }

        internal static void _UpdateProposalWeekValues(ProposalQuarterDto proposalQuarterDto, bool adu)
        {
            var countOfNonHiatusWeeks = proposalQuarterDto.Weeks.Count(week => !week.IsHiatus);
            var truncatedDistributedImpressions = countOfNonHiatusWeeks == 0 ? 0 : proposalQuarterDto.ImpressionGoal / countOfNonHiatusWeeks;
            foreach (var proposalMediaWeekDto in proposalQuarterDto.Weeks)
            {
                _CalculateProposalWeekValues(proposalQuarterDto, adu, proposalMediaWeekDto, truncatedDistributedImpressions);
            }

            var lastWeekImpressions = truncatedDistributedImpressions + (countOfNonHiatusWeeks == 0 ? 0 : proposalQuarterDto.ImpressionGoal % countOfNonHiatusWeeks);
            _CalculateProposalWeekValues(proposalQuarterDto, adu, proposalQuarterDto.Weeks.LastOrDefault(week => !week.IsHiatus), lastWeekImpressions);
        }

        private static void _CalculateProposalWeekValues(ProposalQuarterDto proposalQuarterDto, bool adu, ProposalWeekDto proposalMediaWeekDto, double impressions)
        {
            if (proposalMediaWeekDto == null)
                return;

            if (proposalQuarterDto.DistributeGoals && !proposalMediaWeekDto.IsHiatus)
            {
                proposalMediaWeekDto.Impressions = impressions;
            }

            if (proposalMediaWeekDto.IsHiatus)
            {
                proposalMediaWeekDto.Cost = 0;
                proposalMediaWeekDto.Impressions = 0;
                proposalMediaWeekDto.Units = 0;
            }
            else if (adu)
            {
                proposalMediaWeekDto.Cost = 0;
            }
            else
            {
                proposalMediaWeekDto.Cost = ProposalMath.CalculateCost(proposalQuarterDto.Cpm, proposalMediaWeekDto.Impressions);
            }
        }
    }
}
