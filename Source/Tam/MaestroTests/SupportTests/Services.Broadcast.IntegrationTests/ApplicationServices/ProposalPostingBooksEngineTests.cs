using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    public class ProposalPostingBooksEngineTests
    {
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        private readonly IProposalPostingBooksEngine _ProposalProgramRatingsEngine = 
            IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalPostingBooksEngine>();

        [Test]
        public void CanGetShareBookFromProposal()
        {
            var proposal = _ProposalService.GetProposalById(248);
            var proposalDetail = proposal.Details.First();

            var ratingBook = _ProposalProgramRatingsEngine.GetPostingBookId(proposalDetail);

            Assert.AreEqual(413, ratingBook);
        }

        [Test]
        public void CanGetSinglePostingBookFromProposal()
        {
            var proposal = _ProposalService.GetProposalById(253);
            var proposalDetail = proposal.Details.First();

            var ratingBook = _ProposalProgramRatingsEngine.GetPostingBookId(proposalDetail);

            Assert.AreEqual(410, ratingBook);
        }
    }
}
