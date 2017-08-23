using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
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

        private void _UpdateQuarterValues(ProposalQuarterDto proposalQuarterDto, bool adu)
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

        private void _SetProposalTargets(ProposalDto proposalDto)
        {
            var targets = _CalculateTargetsForProposal(proposalDto.Details);

            proposalDto.TargetBudget = targets.TargetBudget;
            proposalDto.TargetImpressions = targets.TargetImpressions;
            proposalDto.TargetUnits = targets.TargetUnits;
            proposalDto.TargetCPM = targets.TargetCPM;
        }

        private void _SetQuarterTotals(ProposalDetailDto proposalDetailDto)
        {
            var totals = _CalculateTotalsForQuarter(proposalDetailDto.Quarters);

            proposalDetailDto.TotalCost = totals.TotalCost;
            proposalDetailDto.TotalImpressions = totals.TotalImpressions;
            proposalDetailDto.TotalUnits = totals.TotalUnits;
        }

        private void _UpdateProposalWeekValues(ProposalQuarterDto proposalQuarterDto, bool adu)
        {
            var countOfNonHiatusWeeks = proposalQuarterDto.Weeks.Count(week => !week.IsHiatus);
            var impressionGoalInThousands = Math.Truncate(proposalQuarterDto.ImpressionGoal * 1000);
            var truncatedDistributedImpressions = Math.Truncate(impressionGoalInThousands / countOfNonHiatusWeeks);
            var impressionsForLastWeek = truncatedDistributedImpressions +
                                         (impressionGoalInThousands % countOfNonHiatusWeeks);
            var lastNonHiatusMediaWeekDto = proposalQuarterDto.Weeks.LastOrDefault(week => !week.IsHiatus);

            foreach (var proposalMediaWeekDto in proposalQuarterDto.Weeks)
            {
                _CalculateProposalWeekValues(proposalQuarterDto, adu, proposalMediaWeekDto, truncatedDistributedImpressions);
            }

            _CalculateProposalWeekValues(proposalQuarterDto, adu, lastNonHiatusMediaWeekDto, impressionsForLastWeek);
        }

        private void _CalculateProposalWeekValues(ProposalQuarterDto proposalQuarterDto, bool adu,
            ProposalWeekDto proposalMediaWeekDto, double impressions)
        {
            if (proposalMediaWeekDto == null)
                return;

            if (proposalQuarterDto.DistributeGoals && !proposalMediaWeekDto.IsHiatus)
            {
                proposalMediaWeekDto.Impressions = impressions / 1000;
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
                proposalMediaWeekDto.Cost =
                    Math.Round(proposalQuarterDto.Cpm * (decimal) proposalMediaWeekDto.Impressions, 2);
            }
        }

        private ProposalTargets _CalculateTargetsForProposal(List<ProposalDetailDto> proposalDetailDtos)
        {
            var proposalTargets = new ProposalTargets
            {
                TargetBudget = proposalDetailDtos.Sum(detail => detail.TotalCost),
                TargetImpressions = proposalDetailDtos.Sum(detail => detail.TotalImpressions),
                TargetUnits = proposalDetailDtos.Sum(detail => detail.TotalUnits)
            };

            proposalTargets.TargetCPM = proposalTargets.TargetImpressions == 0 ? 0
                : proposalTargets.TargetBudget/(decimal)proposalTargets.TargetImpressions;

            return proposalTargets;
        }

        private ProposalQuarterTotals _CalculateTotalsForQuarter(IEnumerable<ProposalQuarterDto> quarters)
        {
            var proposalQuarterTotalsDto = new ProposalQuarterTotals();

            foreach (var proposalQuarterDto in quarters)
            {
                proposalQuarterTotalsDto.TotalCost += proposalQuarterDto.Weeks.Sum(week => week.Cost);
                proposalQuarterTotalsDto.TotalUnits += proposalQuarterDto.Weeks.Sum(week => week.Units);
                proposalQuarterTotalsDto.TotalImpressions += proposalQuarterDto.Weeks.Sum(week => week.Impressions);
            }

            return proposalQuarterTotalsDto;
        }
    }
}
