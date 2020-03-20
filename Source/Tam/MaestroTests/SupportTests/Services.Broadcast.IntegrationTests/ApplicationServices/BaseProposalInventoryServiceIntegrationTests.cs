using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class BaseProposalInventoryServiceTests
    {
        [Test]
        public void GetRatingAdjustmentMonth_HasShareAndHut_UsesHut()
        {
            var dto = new ProposalDetailInventoryBase();
            dto.HutProjectionBookId = 420;
            dto.ShareProjectionBookId = 322;

            Assert.That(BaseProposalInventoryService.GetRatingAdjustmentMonth(dto), Is.EqualTo(dto.HutProjectionBookId));
        }

        [Test]
        public void GetRatingAdjustmentMonth_HasSweeps_UsesSweeps()
        {
            var dto = new ProposalDetailInventoryBase();
            dto.SingleProjectionBookId = 420;

            Assert.That(BaseProposalInventoryService.GetRatingAdjustmentMonth(dto), Is.EqualTo(dto.SingleProjectionBookId));
        }
    }
}
