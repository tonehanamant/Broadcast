using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalMarketsCalculationEngine : IApplicationService
    {
        List<LookupDto> GetProposalMarketsList(int proposalId, int versionNumber, int proposalDetailDto);
    }

    public class ProposalMarketsCalculationEngine : IProposalMarketsCalculationEngine
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly IProposalPostingBooksEngine _ProposalPostingBooksEngine;

        public ProposalMarketsCalculationEngine(IDataRepositoryFactory dataRepositoryFactory, IProposalPostingBooksEngine proposalPostingBooksEngine)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
            _ProposalPostingBooksEngine = proposalPostingBooksEngine;
        }

        public List<LookupDto> GetProposalMarketsList(int proposalId, int versionNumber, int proposalDetailDto)
        {
            var proposal =
                _DataRepositoryFactory.GetDataRepository<IProposalRepository>()
                    .GetProposalByIdAndVersion(proposalId, versionNumber);

            var proposalDetail =
                _DataRepositoryFactory.GetDataRepository<IProposalRepository>()
                    .GetProposalDetail(proposalDetailDto);

            var postingBookId = _ProposalPostingBooksEngine.GetPostingBookId(proposalDetail);

            var marketRankings =
                _DataRepositoryFactory.GetDataRepository<INsiMarketRepository>()
                .GetMarketRankingsByMediaMonth(postingBookId);

            var allMarkets = _DataRepositoryFactory.GetDataRepository<IMarketRepository>().GetMarketDtos();

            var finalProposalMarkets = new List<LookupDto>();

            _AddMarketsFromGroup(proposal, finalProposalMarkets, allMarkets, marketRankings);
            _AddCustomMarkets(proposal, finalProposalMarkets);
            _ExcludeStandardMarketGroupFromMarkets(proposal, allMarkets, marketRankings, finalProposalMarkets);
            _ExcludeCustomMarkets(proposal, finalProposalMarkets);

            return finalProposalMarkets.DistinctBy(m => m.Id).ToList();
        }

        private static void _ExcludeCustomMarkets(ProposalDto proposal, List<LookupDto> finalProposalMarkets)
        {
            if (proposal.Markets != null)
            {
                var customBlackoutMarkets =
                    proposal.Markets.Where(m => m.IsBlackout == true).Select(
                        m => new LookupDto()
                        {
                            Display = m.Display,
                            Id = m.Id
                        }).ToList();
                finalProposalMarkets.RemoveAll(m => customBlackoutMarkets.Select(e => e.Id).ToList().Contains(m.Id));
            }
        }

        private void _ExcludeStandardMarketGroupFromMarkets(
            ProposalDto proposal,
            List<LookupDto> allMarkets,
            Dictionary<int, int> marketRankings,
            List<LookupDto> finalProposalMarkets)
        {
            if (proposal.BlackoutMarketGroupId != null && proposal.BlackoutMarketGroupId != ProposalEnums.ProposalMarketGroups.Custom)
            {
                var excludedMarketGroup = _GetMarketListByGroup(
                    allMarkets,
                    marketRankings,
                    proposal.BlackoutMarketGroupId.Value);
                finalProposalMarkets.RemoveAll(m => excludedMarketGroup.Select(e => e.Id).ToList().Contains(m.Id));
            }
        }

        private static void _AddCustomMarkets(ProposalDto proposal, List<LookupDto> finalProposalMarkets)
        {
            if (proposal.Markets != null)
            {
                var customNonBlackoutMarkets =
                    proposal.Markets.Where(m => m.IsBlackout == false).Select(
                        m => new LookupDto()
                        {
                            Display = m.Display,
                            Id = m.Id
                        }).ToList();
                finalProposalMarkets.AddRange(customNonBlackoutMarkets);
            }
        }

        private void _AddMarketsFromGroup(
            ProposalDto proposal,
            List<LookupDto> finalProposalMarkets,
            List<LookupDto> allMarkets,
            Dictionary<int, int> marketRankings)
        {
            if (proposal.MarketGroupId != ProposalEnums.ProposalMarketGroups.Custom)
            {
                finalProposalMarkets.AddRange(_GetMarketListByGroup(allMarkets, marketRankings, proposal.MarketGroupId));
            }
        }

        private IEnumerable<LookupDto> _GetMarketListByGroup(List<LookupDto> allMarkets, Dictionary<int, int> marketRankings, ProposalEnums.ProposalMarketGroups proposalMarketsGroup)
        {
            if (proposalMarketsGroup == ProposalEnums.ProposalMarketGroups.All)
            {
                return allMarkets;
            }

            var sortedMarkets = allMarkets.Where(m => marketRankings.Select(r => r.Key).Contains(m.Id)).OrderBy(m => marketRankings[m.Id]).ToList();

            var applicableMarkets = sortedMarkets.Take((int) proposalMarketsGroup).ToList();

            return applicableMarkets;
        }
    }
}
