using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalMarketsCalculationEngine : IApplicationService
    {
        List<LookupDto> GetProposalMarketsList(int proposalId, int versionNumber);
        List<LookupDto> GetProposalMarketsList(ProposalDto proposal);
    }

    public class ProposalMarketsCalculationEngine : IProposalMarketsCalculationEngine
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;

        public ProposalMarketsCalculationEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
        }

        public List<LookupDto> GetProposalMarketsList(int proposalId, int versionNumber)
        {
            var proposal = _DataRepositoryFactory.GetDataRepository<IProposalRepository>().GetProposalByIdAndVersion(proposalId, versionNumber);

            return GetProposalMarketsList(proposal);
        }

        public List<LookupDto> GetProposalMarketsList(ProposalDto proposal)
        {
            var allMarkets = _DataRepositoryFactory.GetDataRepository<IMarketRepository>().GetMarketDtos();

            var finalProposalMarkets = allMarkets;

            _ExcludeStandardMarketGroupFromMarkets(proposal, allMarkets, finalProposalMarkets);
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

        private void _ExcludeStandardMarketGroupFromMarkets(ProposalDto proposal, List<LookupDto> allMarkets, List<LookupDto> finalProposalMarkets)
        {
            if (proposal.BlackoutMarketGroupId == ProposalEnums.ProposalMarketGroups.All)
            {
                finalProposalMarkets.RemoveAll(m => allMarkets.Select(e => e.Id).ToList().Contains(m.Id));
            }
        }
    }
}
