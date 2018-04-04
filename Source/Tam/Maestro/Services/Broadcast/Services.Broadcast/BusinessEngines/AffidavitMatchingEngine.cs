using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Services;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines
{
    public interface IAffidavitMatchingEngine : IApplicationService
    {
        List<AffidavitMatchingProposalWeek> Match(
            AffidavitSaveRequestDetail requestDetail,
            List<AffidavitMatchingProposalWeek> proposalWeeks);
        List<AffidavitFileDetailProblem> MatchingProblems();
    }

    public class AffidavitMatchingEngine : IAffidavitMatchingEngine
    {
        public List<AffidavitMatchingProposalWeek> _MatchingProposalWeeks;
        public List<AffidavitFileDetailProblem> _MatchingProblems;
        private readonly int _BroadcastMatchingBuffer;
        private readonly IDaypartCache _DaypartCache;

        public List<AffidavitFileDetailProblem> MatchingProblems()
        {
            return _MatchingProblems;
        }

        public AffidavitMatchingEngine(IDaypartCache daypartCache)
        {
            _DaypartCache = daypartCache;
            _BroadcastMatchingBuffer = BroadcastServiceSystemParameter.BroadcastMatchingBuffer;
        }

        public List<AffidavitMatchingProposalWeek> Match(
            AffidavitSaveRequestDetail requestDetail,
            List<AffidavitMatchingProposalWeek> proposalWeeks)
        {
            _MatchingProposalWeeks = new List<AffidavitMatchingProposalWeek>();
            _MatchingProblems = new List<AffidavitFileDetailProblem>();

            if (proposalWeeks == null || proposalWeeks.Count == 0)
            {
                _MatchingProblems.Add(new AffidavitFileDetailProblem()
                {
                    Type = AffidavitFileDetailProblemTypeEnum.UnlinkedIsci,
                    Description = String.Format("Isci not found: {0}", requestDetail.Isci)
                });

                return _MatchingProposalWeeks;
            }

            var unmarriedProposalIscisByProposal = proposalWeeks.Where(i => !i.MarriedHouseIsci).GroupBy(g => g.ProposalVersionId);
            if (unmarriedProposalIscisByProposal.Count() > 1)
            {
                _MatchingProblems.Add(new AffidavitFileDetailProblem()
                {
                    Type = AffidavitFileDetailProblemTypeEnum.UnmarriedOnMultipleContracts,
                    Description = String.Format("Unmarried Isci {0} exists on multiple proposals", requestDetail.Isci)
                });
                return _MatchingProposalWeeks;
            }

            var marriedProposalIscisByProposal = proposalWeeks.Where(i => i.MarriedHouseIsci).GroupBy(g => g.ProposalVersionId);
            if (unmarriedProposalIscisByProposal.Any() && marriedProposalIscisByProposal.Any())
            {
                _MatchingProblems.Add(new AffidavitFileDetailProblem()
                {
                    Type = AffidavitFileDetailProblemTypeEnum.UnmarriedOnMultipleContracts,
                    Description = String.Format("Isci {0} exists as married and unmarried", requestDetail.Isci)
                });
                return _MatchingProposalWeeks;
            }

            var proposalWeeksByProposal = proposalWeeks.GroupBy(g => g.ProposalVersionId);
            foreach (var singleProposalWeeksByProposal in proposalWeeksByProposal)
            {
                var singleProposalWeeks = singleProposalWeeksByProposal.Select(w => w);
                var proposalWeeksByProposalDetail = singleProposalWeeks.GroupBy(g => g.ProposalVersionDetailId);
                AffidavitMatchingProposalWeek matchedProposalDetailWeek;
                var airtimeMatchingProposalDetailWeeks =
                    _GetAirtimeMatchingProposalDetailWeeks(proposalWeeksByProposalDetail, requestDetail);
                var dateAndTimeMatchs = airtimeMatchingProposalDetailWeeks.Where(a => a.TimeMatch && a.DateMatch);
                var dateMatch = airtimeMatchingProposalDetailWeeks.Where(a => a.DateMatch);
                var timeMatch = airtimeMatchingProposalDetailWeeks.Where(a => a.TimeMatch);

                if (dateAndTimeMatchs.Any()) //if matched airtime on one or more
                {
                    matchedProposalDetailWeek = dateAndTimeMatchs.First(); //take first matched on airtime
                }
                else if (dateMatch.Any()) 
                {
                    matchedProposalDetailWeek = dateMatch.First();
                }
                else if (timeMatch.Any()) 
                {
                    matchedProposalDetailWeek = timeMatch.First();
                }
                else //if none matched on airtime
                { 
                    matchedProposalDetailWeek = proposalWeeksByProposalDetail.First().First(); //take first week from first proposal detail
                }

                _MatchingProposalWeeks.Add(matchedProposalDetailWeek);
            }

            return _MatchingProposalWeeks;
        }

        private List<AffidavitMatchingProposalWeek> _GetAirtimeMatchingProposalDetailWeeks
    (System.Collections.Generic.IEnumerable<IGrouping<int, AffidavitMatchingProposalWeek>> proposalWeeksByProposalDetail
    , AffidavitSaveRequestDetail affidavitDetail)
        {
            var result = new List<AffidavitMatchingProposalWeek>();
            var dayparts = _DaypartCache.GetDisplayDayparts(proposalWeeksByProposalDetail.Select(d => d.First().ProposalVersionDetailDaypartId));

            foreach (var proposalDetail in proposalWeeksByProposalDetail)
            {
                var bufferInSeconds = _BroadcastMatchingBuffer;
                var displayDaypart = dayparts[proposalDetail.First().ProposalVersionDetailDaypartId];
                var actualStartTime = displayDaypart.StartTime < 0 ? 86400 - Math.Abs(displayDaypart.StartTime) : displayDaypart.StartTime;
                //add 1 second to include the daypart endTime as valid time
                var actualEndTime = displayDaypart.EndTime < 0 ? Math.Abs(86400 - displayDaypart.EndTime + 1) : displayDaypart.EndTime + 1;
                var adjustedStartTime = displayDaypart.StartTime - bufferInSeconds < 0 ? 86400 - Math.Abs(displayDaypart.StartTime - bufferInSeconds) : displayDaypart.StartTime - bufferInSeconds;
                var isOvernight = (actualEndTime < actualStartTime && actualEndTime < adjustedStartTime);

                if (isOvernight)
                {
                    // some of these "if" can be combined, but will be harder to maintain and negligably performant
                    if (affidavitDetail.AirTime.TimeOfDay.TotalSeconds >= actualStartTime && affidavitDetail.AirTime.TimeOfDay.TotalSeconds >= actualEndTime)
                    {   // covers airtime before midnight
                        foreach (var proposalDetailWeek in proposalDetail)
                        {
                            proposalDetailWeek.IsLeadInMatch = false;

                            if (proposalDetailWeek.Spots != 0)
                                proposalDetailWeek.TimeMatch = true;
                        }
                    }
                    else if (affidavitDetail.AirTime.TimeOfDay.TotalSeconds >= adjustedStartTime && affidavitDetail.AirTime.TimeOfDay.TotalSeconds >= actualEndTime)
                    {   // covers lead in time
                        foreach (var proposalDetailWeek in proposalDetail)
                        {
                            proposalDetailWeek.IsLeadInMatch = true;

                            if (proposalDetailWeek.Spots != 0)
                            {
                                proposalDetailWeek.TimeMatch = true;
                            }
                        }
                    }
                    else if (affidavitDetail.AirTime.TimeOfDay.TotalSeconds <= actualEndTime && affidavitDetail.AirTime.TimeOfDay.TotalSeconds <= actualStartTime)
                    {   // covers airtime after midnight
                        foreach (var proposalDetailWeek in proposalDetail)
                        {
                            proposalDetailWeek.IsLeadInMatch = false;

                            if (proposalDetailWeek.Spots != 0)
                                proposalDetailWeek.TimeMatch = true;
                        }
                    }
                }
                else
                {
                    if (affidavitDetail.AirTime.TimeOfDay.TotalSeconds >= actualStartTime && affidavitDetail.AirTime.TimeOfDay.TotalSeconds <= actualEndTime)
                    {
                        foreach (var proposalDetailWeek in proposalDetail)
                        {
                            proposalDetailWeek.IsLeadInMatch = false;

                            if (proposalDetailWeek.Spots != 0)
                                proposalDetailWeek.TimeMatch = true;
                        }
                    }
                    else if (affidavitDetail.AirTime.TimeOfDay.TotalSeconds >= adjustedStartTime && affidavitDetail.AirTime.TimeOfDay.TotalSeconds <= actualEndTime)
                    {
                        foreach (var proposalDetailWeek in proposalDetail)
                        {
                            proposalDetailWeek.IsLeadInMatch = true;

                            if (proposalDetailWeek.Spots != 0)
                                proposalDetailWeek.TimeMatch = true;
                        }
                    }
                }

                foreach (var proposalDetailWeek in proposalDetail)
                {
                    proposalDetailWeek.DateMatch = _IsDateMatch(affidavitDetail, result, proposalDetail);

                    if (proposalDetailWeek.TimeMatch || proposalDetailWeek.DateMatch)
                        result.Add(proposalDetailWeek);
                }
            }

            return result;
        }

        private bool _IsDateMatch(AffidavitSaveRequestDetail affidavitDetail, List<AffidavitMatchingProposalWeek> result, IGrouping<int, AffidavitMatchingProposalWeek> proposalDetail)
        {
            foreach (var proposalWeek in proposalDetail)
            {
                if (affidavitDetail.AirTime.Date >= proposalWeek.ProposalVersionDetailWeekStart &&
                    affidavitDetail.AirTime.Date <= proposalWeek.ProposalVersionDetailWeekEnd &&
                     proposalWeek.Spots != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
