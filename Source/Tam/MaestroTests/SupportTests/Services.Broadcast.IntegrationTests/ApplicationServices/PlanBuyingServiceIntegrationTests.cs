using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Buying;
using Services.Broadcast.Entities.Buying;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class PlanBuyingServiceIntegrationTests
    {
        private readonly IBuyingService _PlanBuyingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IBuyingService>();
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlansBuying()
        {
            var plans = _PlanBuyingService.GetPlansBuying(new PlanBuyingListRequest
            {
                StatusFilter = PlanBuyingStatusEnum.Working,
                FlightFilter = PlanBuyingTimeFramesEnum.All
            });

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
