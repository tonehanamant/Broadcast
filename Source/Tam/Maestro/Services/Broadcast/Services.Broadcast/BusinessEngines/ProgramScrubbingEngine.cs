using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProgramScrubbingEngine : IApplicationService
    {
        void Scrub(ProposalDetailDto proposalDetail, ScrubbingFileDetail affidavitDetail, ClientScrub scrub);
    }
    public class ProgramScrubbingEngine : IProgramScrubbingEngine
    {
        private readonly int _BroadcastMatchingBuffer;

        public ProgramScrubbingEngine()
        {
            _BroadcastMatchingBuffer =  BroadcastServiceSystemParameter.BroadcastMatchingBuffer;
        }

        public void Scrub(ProposalDetailDto proposalDetail, ScrubbingFileDetail affidavitDetail, ClientScrub scrub)
        {
            var actualStartTime = affidavitDetail.LeadInEndTime;
            var actualEndTime = affidavitDetail.LeadOutStartTime;
            var adjustedStartTime = actualStartTime + _BroadcastMatchingBuffer >= 86400 ? actualStartTime + _BroadcastMatchingBuffer - 86400 : actualStartTime + _BroadcastMatchingBuffer;
            var adjustedEndTime = actualEndTime - _BroadcastMatchingBuffer < 0  ? actualEndTime - _BroadcastMatchingBuffer + 86400 : actualEndTime - _BroadcastMatchingBuffer;

            var isLeadIn = adjustedStartTime > actualStartTime &&
                affidavitDetail.AirTime >= actualStartTime &&
                affidavitDetail.AirTime <= adjustedStartTime;

            var isOvernightLeadIn = adjustedStartTime < actualStartTime &&
                (affidavitDetail.AirTime >= actualStartTime ||
                affidavitDetail.AirTime <= adjustedStartTime);

            var isLeadOut = actualEndTime > adjustedEndTime &&
                affidavitDetail.AirTime <= actualEndTime &&
                affidavitDetail.AirTime >= adjustedEndTime;

            var isOvernightLeadOut = actualEndTime < adjustedEndTime &&
                (affidavitDetail.AirTime <= actualEndTime ||
                affidavitDetail.AirTime >= adjustedEndTime);

            if (_ProposalDetailMatchesProgram(proposalDetail, affidavitDetail.ProgramName) &&
                _ProposalDetailMatchesGenre(proposalDetail, affidavitDetail.Genre) &&
                _ProposalDetailMatchesShowType(proposalDetail, affidavitDetail.ShowType))
            {
                scrub.EffectiveProgramName = affidavitDetail.ProgramName;
                scrub.MatchProgram = true;
                scrub.EffectiveGenre = affidavitDetail.Genre;
                scrub.MatchGenre = true;
                scrub.EffectiveShowType = affidavitDetail.ShowType;
                scrub.MatchShowType = true;
            }
            else if ((isLeadIn || isOvernightLeadIn) &&
                _ProposalDetailMatchesProgram(proposalDetail, affidavitDetail.LeadinProgramName) &&
                _ProposalDetailMatchesGenre(proposalDetail, affidavitDetail.LeadinGenre) &&
                _ProposalDetailMatchesShowType(proposalDetail, affidavitDetail.LeadInShowType))
            {
                scrub.EffectiveProgramName = affidavitDetail.LeadinProgramName;
                scrub.MatchProgram = true;
                scrub.EffectiveGenre = affidavitDetail.LeadinGenre;
                scrub.MatchGenre = true;
                scrub.EffectiveShowType = affidavitDetail.LeadInShowType;
                scrub.MatchShowType = true;
            }
            else if ((isLeadOut || isOvernightLeadOut) &&
                _ProposalDetailMatchesProgram(proposalDetail, affidavitDetail.LeadoutProgramName) &&
                _ProposalDetailMatchesGenre(proposalDetail, affidavitDetail.LeadoutGenre) &&
                _ProposalDetailMatchesShowType(proposalDetail, affidavitDetail.LeadOutShowType))
            {
                scrub.EffectiveProgramName = affidavitDetail.LeadoutProgramName;
                scrub.MatchProgram = true;
                scrub.EffectiveGenre = affidavitDetail.LeadoutGenre;
                scrub.MatchGenre = true;
                scrub.EffectiveShowType = affidavitDetail.LeadOutShowType;
                scrub.MatchShowType = true;
            }
            else
            {
                scrub.EffectiveProgramName = affidavitDetail.ProgramName;
                scrub.MatchProgram = _ProposalDetailMatchesProgram(proposalDetail, affidavitDetail.ProgramName);
                scrub.EffectiveGenre = affidavitDetail.Genre;
                scrub.MatchGenre = _ProposalDetailMatchesGenre(proposalDetail, affidavitDetail.Genre);
                scrub.EffectiveShowType = affidavitDetail.ShowType;
                scrub.MatchShowType = _ProposalDetailMatchesShowType(proposalDetail, affidavitDetail.ShowType);
            }
        }

        private static bool _ProposalDetailMatchesProgram(ProposalDetailDto proposalDetail, string programName)
        {
            var matchesProgram = true;
            if (proposalDetail.ProgramCriteria.Any())
            {
                var matchedProgramCriteria = proposalDetail.ProgramCriteria.SingleOrDefault(pc =>
                                        pc.Program.Display.Equals(programName, StringComparison.InvariantCultureIgnoreCase));

                if(ContainTypeEnum.Include == proposalDetail.ProgramCriteria.First().Contain)
                {
                    matchesProgram = (matchedProgramCriteria != null);
                }
                else
                {
                    matchesProgram = (matchedProgramCriteria == null);
                }

            }
            return matchesProgram;
        }

        private static bool _ProposalDetailMatchesGenre(ProposalDetailDto proposalDetail, string programGenre)
        {
            var matchesGenre = true;
            if (proposalDetail.GenreCriteria.Any())
            {
                var matchedGenreCriteria = proposalDetail.GenreCriteria.SingleOrDefault(pc =>
                    pc.Genre.Display.Equals(programGenre, StringComparison.InvariantCultureIgnoreCase));

                if (ContainTypeEnum.Include == proposalDetail.GenreCriteria.First().Contain)
                {
                    matchesGenre = (matchedGenreCriteria != null);
                }
                else
                {
                    matchesGenre = (matchedGenreCriteria == null);
                }
            }
            return matchesGenre;
        }

        private static bool _ProposalDetailMatchesShowType(ProposalDetailDto proposalDetail, string programShowType)
        {
            var matchesShowType = true;
            if (proposalDetail.ShowTypeCriteria.Any())
            {
                var matchedShowTypeCriteria = proposalDetail.ShowTypeCriteria.SingleOrDefault(pc =>
                    pc.ShowType.Display.Equals(programShowType, StringComparison.InvariantCultureIgnoreCase));

                if (ContainTypeEnum.Include == proposalDetail.ShowTypeCriteria.First().Contain)
                {
                    matchesShowType = (matchedShowTypeCriteria != null);
                }
                else
                {
                    matchesShowType = (matchedShowTypeCriteria == null);
                }
            }
            return matchesShowType;
        }
    }
}
