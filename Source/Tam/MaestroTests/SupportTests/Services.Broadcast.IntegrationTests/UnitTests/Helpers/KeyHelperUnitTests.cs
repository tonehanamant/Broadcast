using NUnit.Framework;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    public class KeyHelperUnitTests
    {
        [Test]
        public void GetCampaignLockingKey()
        {
            var result = KeyHelper.GetCampaignLockingKey(1);

            Assert.AreEqual("broadcast_campaign : 1", result);
        }

        [Test]
        public void GetProposalLockingKey()
        {
            var result = KeyHelper.GetProposalLockingKey(1);

            Assert.AreEqual("broadcast_proposal : 1", result);
        }

        [Test]
        public void GetStationLockingKey()
        {
            var result = KeyHelper.GetStationLockingKey(1);

            Assert.AreEqual("broadcast_station : 1", result);
        }

        [Test]
        public void GetPlanLockingKey()
        {
            var result = KeyHelper.GetPlanLockingKey(1);

            Assert.AreEqual("broadcast_plan : 1", result);
        }
    }
}
