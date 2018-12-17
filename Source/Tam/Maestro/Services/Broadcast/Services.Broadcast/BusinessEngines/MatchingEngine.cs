﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines
{
    public interface IMatchingEngine : IApplicationService
    {
        /// <summary>
        /// Match file detail with proposal week
        /// </summary>
        /// <param name="fileDetail">File detail to match</param>
        /// <param name="proposalWeeks">Proposal weeks to find the match to.</param>
        /// <returns>List of MatchingProposalWeek objects containing the matched weeks</returns>
        List<MatchingProposalWeek> Match(ScrubbingFileDetail affidavitFileDetail, List<MatchingProposalWeek> proposalWeeks, int? spotLenghtId = null);

        /// <summary>
        /// Access to matching problems object
        /// </summary>
        /// <returns>List of FileDetailProblem objects </returns>
        List<FileDetailProblem> GetMatchingProblems();
    }

    public class MatchingEngine : IMatchingEngine
    {
        public List<MatchingProposalWeek> _MatchingProposalWeeks;
        public List<FileDetailProblem> _MatchingProblems;
        private readonly int _BroadcastMatchingBuffer;
        private readonly IDaypartCache _DaypartCache;

        /// <summary>
        /// Access to matching problems object
        /// </summary>
        /// <returns>List of FileDetailProblem objects </returns>
        public List<FileDetailProblem> GetMatchingProblems()
        {
            return _MatchingProblems;
        }

        public MatchingEngine(IDaypartCache daypartCache)
        {
            _DaypartCache = daypartCache;
            _BroadcastMatchingBuffer = BroadcastServiceSystemParameter.BroadcastMatchingBuffer;
        }

        /// <summary>
        /// Match file detail with proposal week
        /// </summary>
        /// <param name="fileDetail">File detail to match</param>
        /// <param name="proposalWeeks">Proposal weeks to find the match to.</param>
        /// <returns>List of MatchingProposalWeek objects containing the matched weeks</returns>
        public List<MatchingProposalWeek> Match(ScrubbingFileDetail fileDetail, List<MatchingProposalWeek> proposalWeeks, int? spotLenghtId = null)
        {
            _MatchingProposalWeeks = new List<MatchingProposalWeek>();
            _MatchingProblems = new List<FileDetailProblem>();

            if (proposalWeeks == null || proposalWeeks.Count == 0)
            {
                _MatchingProblems.Add(new FileDetailProblem()
                {
                    Type = FileDetailProblemTypeEnum.UnlinkedIsci,
                    Description = $"Isci not found: {fileDetail.Isci}"
                });

                return _MatchingProposalWeeks;
            }

            var unmarriedProposalIscisByProposal = proposalWeeks.Where(i => !i.MarriedHouseIsci).GroupBy(g => g.ProposalVersionId);
            if (unmarriedProposalIscisByProposal.Count() > 1)
            {
                _MatchingProblems.Add(new FileDetailProblem()
                {
                    Type = FileDetailProblemTypeEnum.UnmarriedOnMultipleContracts,
                    Description = $"Unmarried Isci {fileDetail.Isci} exists on multiple proposals"
                });
                return _MatchingProposalWeeks;
            }

            var marriedProposalIscisByProposal = proposalWeeks.Where(i => i.MarriedHouseIsci).GroupBy(g => g.ProposalVersionId);
            if (unmarriedProposalIscisByProposal.Any() && marriedProposalIscisByProposal.Any())
            {
                _MatchingProblems.Add(new FileDetailProblem()
                {
                    Type = FileDetailProblemTypeEnum.MarriedAndUnmarried,
                    Description = $"Isci {fileDetail.Isci} exists as married and unmarried"
                });
                return _MatchingProposalWeeks;
            }

            spotLenghtId = spotLenghtId ?? fileDetail.SpotLengthId;
            proposalWeeks = proposalWeeks.Where(x => x.SpotLengthId == spotLenghtId).ToList();
            if (!proposalWeeks.Any())
            {
                _MatchingProblems.Add(new FileDetailProblem()
                {
                    Type = FileDetailProblemTypeEnum.UnmatchedSpotLength,
                    Description = $"Unmatched Spot length for isci {fileDetail.Isci}"
                });
                return _MatchingProposalWeeks;
            }

            var proposalWeeksByProposal = proposalWeeks.GroupBy(g => g.ProposalVersionId);
            foreach (var singleProposalWeeksByProposal in proposalWeeksByProposal)
            {
                var singleProposalWeeks = singleProposalWeeksByProposal.Select(w => w);
                var proposalWeeksByProposalDetail = singleProposalWeeks.GroupBy(g => g.ProposalVersionDetailId);
                MatchingProposalWeek matchedProposalDetailWeek;
                var airtimeMatchingProposalDetailWeeks = _GetAirtimeMatchingProposalDetailWeeks(proposalWeeksByProposalDetail, fileDetail);
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

        private List<MatchingProposalWeek> _GetAirtimeMatchingProposalDetailWeeks(IEnumerable<IGrouping<int, MatchingProposalWeek>> proposalWeeksByProposalDetail, ScrubbingFileDetail affidavitDetail)
        {
            var result = new List<MatchingProposalWeek>();
            var dayparts = _DaypartCache.GetDisplayDayparts(proposalWeeksByProposalDetail.Select(d => d.First().ProposalVersionDetailDaypartId));

            foreach (var proposalDetail in proposalWeeksByProposalDetail)
            {
                var bufferInSeconds = _BroadcastMatchingBuffer;
                var displayDaypart = dayparts[proposalDetail.First().ProposalVersionDetailDaypartId];
                var actualStartTime = displayDaypart.StartTime < 0 ? 86400 - Math.Abs(displayDaypart.StartTime) : displayDaypart.StartTime;
                //add 1 second to include the daypart endTime as valid time
                var actualEndTime = displayDaypart.EndTime + 1 < 0 ? Math.Abs(86400 - displayDaypart.EndTime + 1) : displayDaypart.EndTime + 1;
                var adjustedStartTime = displayDaypart.StartTime - bufferInSeconds < 0 ? 86400 - Math.Abs(displayDaypart.StartTime - bufferInSeconds) : displayDaypart.StartTime - bufferInSeconds;
                var isOvernight = (actualEndTime < actualStartTime && actualEndTime < adjustedStartTime);

                if (isOvernight)
                {
                    // some of these "if" can be combined, but will be harder to maintain and negligably performant
                    if (affidavitDetail.AirTime >= actualStartTime && affidavitDetail.AirTime >= actualEndTime)
                    {   // covers airtime before midnight
                        foreach (var proposalDetailWeek in proposalDetail)
                        {
                            proposalDetailWeek.IsLeadInMatch = false;

                            if (proposalDetailWeek.Spots != 0)
                                proposalDetailWeek.TimeMatch = true;
                        }
                    }
                    else if (affidavitDetail.AirTime >= adjustedStartTime && affidavitDetail.AirTime >= actualEndTime)
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
                    else if (affidavitDetail.AirTime <= actualEndTime && affidavitDetail.AirTime <= actualStartTime)
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
                    if (affidavitDetail.AirTime >= actualStartTime && affidavitDetail.AirTime <= actualEndTime)
                    {
                        foreach (var proposalDetailWeek in proposalDetail)
                        {
                            proposalDetailWeek.IsLeadInMatch = false;

                            if (proposalDetailWeek.Spots != 0)
                                proposalDetailWeek.TimeMatch = true;
                        }
                    }
                    else if (affidavitDetail.AirTime >= adjustedStartTime && affidavitDetail.AirTime <= actualEndTime)
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
                    proposalDetailWeek.DateMatch = _IsDateMatch(affidavitDetail.OriginalAirDate.Date, result, proposalDetailWeek);

                    if (proposalDetailWeek.TimeMatch || proposalDetailWeek.DateMatch)
                        result.Add(proposalDetailWeek);
                }
            }

            return result;
        }

        private bool _IsDateMatch(DateTime affidavitDetailAirDate, List<MatchingProposalWeek> result, MatchingProposalWeek proposalDetailWeek)
        {
            return affidavitDetailAirDate >= proposalDetailWeek.ProposalVersionDetailWeekStart &&
                   affidavitDetailAirDate <= proposalDetailWeek.ProposalVersionDetailWeekEnd &&
                   proposalDetailWeek.Spots != 0;
        }
    }
}
