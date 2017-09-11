using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class BaseProposalInventoryServiceTests
    {
        [Test]
        public void GetRatingAdjustmentMonth_HasShareAndHut_UsesHut()
        {
            var dto = new ProposalDetailInventoryBase();
            dto.HutPostingBookId = 420;
            dto.SharePostingBookId = 322;

            Assert.That(BaseProposalInventoryService.GetRatingAdjustmentMonth(dto), Is.EqualTo(dto.HutPostingBookId));
        }

        [Test]
        public void GetRatingAdjustmentMonth_HasSweeps_UsesSweeps()
        {
            var dto = new ProposalDetailInventoryBase();
            dto.SinglePostingBookId = 420;

            Assert.That(BaseProposalInventoryService.GetRatingAdjustmentMonth(dto), Is.EqualTo(dto.SinglePostingBookId));
        }
    }
}
