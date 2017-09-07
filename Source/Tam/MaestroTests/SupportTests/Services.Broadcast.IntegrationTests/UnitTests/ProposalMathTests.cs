using NUnit.Framework;
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class ProposalMathTests
    {
        [TestCase(.5, .5, 1000)]
        [TestCase(1, 1, 1000)]
        [TestCase(1, 13, 13000)]
        [TestCase(1.5, 1.5, 1000)]
        [TestCase(0, 1241, 0)]
        [TestCase(0, 0, 11324512)]
        public void CalculateCpm(decimal cpm, decimal cost, int impressions)
        {
            Assert.That(ProposalMath.CalculateCpm(cost, impressions), Is.EqualTo(cpm));
        }

        [TestCase(.5, .5, 1000)]
        [TestCase(1, 1, 1000)]
        [TestCase(1, 13, 13000)]
        [TestCase(1.5, 1.5, 1000)]
        [TestCase(0, 0, 0)]
        [TestCase(0, 0, 11324512)]
        public void CalculateCost(decimal cpm, decimal cost, int impressions)
        {
            Assert.That(ProposalMath.CalculateCost(cpm, impressions), Is.EqualTo(cost));
        }
    }
}
