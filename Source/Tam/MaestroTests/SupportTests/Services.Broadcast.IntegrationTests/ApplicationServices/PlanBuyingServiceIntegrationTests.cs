using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class PlanBuyingServiceIntegrationTests
    {
        private readonly IPlanBuyingService _PlanBuyingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
        
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
