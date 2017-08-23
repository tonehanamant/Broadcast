using System;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalPostingBooksEngine : IApplicationService
    {
        int GetPostingBookId(int proposalDetailId);
        int GetPostingBookId(IHavePostingBooks proposalDetailDto);
    }

    public class ProposalPostingBooksEngine : IProposalPostingBooksEngine
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;

        public ProposalPostingBooksEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
        }

        public int GetPostingBookId(int proposalDetailId)
        {
            var proposalDetailDto = _DataRepositoryFactory.GetDataRepository<IProposalRepository>()
                .GetProposalDetail(proposalDetailId);

            return GetPostingBookId(proposalDetailDto);
        }

        public int GetPostingBookId(IHavePostingBooks proposalDetail)
        {
            if (proposalDetail.SharePostingBookId.HasValue)
                return proposalDetail.SharePostingBookId.Value;

            if (proposalDetail.SinglePostingBookId.HasValue)
                return proposalDetail.SinglePostingBookId.Value;

            throw new Exception("Proposal detail is missing a valid ratings book");
        }
    }
}
