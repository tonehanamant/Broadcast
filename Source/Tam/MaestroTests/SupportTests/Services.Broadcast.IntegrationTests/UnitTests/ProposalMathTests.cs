using System;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    [Category("short_running")]
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
        [Ignore("BP-2513 : VPVH_Calculated")]
        [TestCase(945222.82107598265, 3675017.06392121, 0.25720229447518222)]
        [TestCase(2917764.81390233, 2917764.81390233, 1)]       
        public void CalculateVPVH(double projectedImpressions, double householdProjectedImpressions, double VPVH)
        {
            Assert.That(ProposalMath.CalculateVPVH(projectedImpressions, householdProjectedImpressions), Is.EqualTo(VPVH));
        }

        [TestCase(312, 0, 0, 0, 0, 0)]
        [TestCase(312, 12, 0, 0, 0, 0)]
        [TestCase(312, 0, 4, 0, 0, 0)]
        [TestCase(312, 0, 0, 6, 0, 0)]
        [TestCase(4000, 35000, 35000, 40, 0, .01)]
        [TestCase(4000, 7500, 6000, 54, 0, .48)]
        public void CalculateCpmPercent(decimal totalCost, double totalImpression, decimal targetBudget, double targetImpression, double margin,decimal percent)
        {
            var result =
                ProposalMath.CalculateCpmPercent(totalCost, totalImpression, targetBudget, targetImpression, margin);
            Assert.That(Math.Round(result,2), Is.EqualTo(percent));
        }
    }
}
