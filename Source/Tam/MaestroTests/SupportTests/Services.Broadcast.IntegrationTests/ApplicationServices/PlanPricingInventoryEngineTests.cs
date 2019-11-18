using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanPricingInventoryEngineTests
    {
        private readonly IPlanPricingInventoryEngine _PlanPricingInventoryEngine;

        public PlanPricingInventoryEngineTests()
        {
            _PlanPricingInventoryEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingInventoryEngine>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunNoDataTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(1196);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(1198);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunWithHiatusDayTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(1199);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunSpotLengthTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(1197);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunWithProgramWithMultipleAudiencesTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(1200);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }
    }
}
