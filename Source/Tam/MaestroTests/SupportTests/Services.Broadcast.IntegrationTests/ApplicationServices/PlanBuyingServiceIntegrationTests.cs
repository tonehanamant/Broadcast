using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Plan;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanBuyingServiceIntegrationTests
    {
        private readonly IPlanBuyingService _PlanBuyingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlansBuying()
        {
            var plans = _PlanBuyingService.GetPlansBuying(1,4000);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(plans));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanBuyingById()
        {
            var plan = _PlanBuyingService.GetPlanBuyingById(1);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(plan));
        }
    }
}
