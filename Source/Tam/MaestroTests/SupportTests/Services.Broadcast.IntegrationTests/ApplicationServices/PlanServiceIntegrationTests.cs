using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Plan;
using System;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanServiceIntegrationTests
    {
        private readonly IPlanService _PlanService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllProducts()
        {
            var products = _PlanService.GetProducts();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(products));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanStatuses()
        {
            var statuses = _PlanService.GetPlanStatuses();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(statuses));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateNewPlan()
        {
            using (new TransactionScopeWrapper())
            {
                CreatePlanDto newPlan = _GetNewPlan();

                var newPlanId = _PlanService.CreatePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
            }
        }                

        [Test]
        public void CreatePlan_InvalidSpotLengthId()
        {
            using (new TransactionScopeWrapper())
            {
                CreatePlanDto newPlan = _GetNewPlan();
                newPlan.SpotLengthId = 100;

                var exception = Assert.Throws<Exception>(() => _PlanService.CreatePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01)));

                Assert.That(exception.Message, Is.EqualTo("Invalid spot length"));
            }
        }

        [Test]
        public void CreatePlan_InvalidProductId()
        {
            using (new TransactionScopeWrapper())
            {
                CreatePlanDto newPlan = _GetNewPlan();
                newPlan.ProductId = 0;

                var exception = Assert.Throws<Exception>(() => _PlanService.CreatePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01)));

                Assert.That(exception.Message, Is.EqualTo("Invalid product"));
            }
        }

        [Test]
        public void CreatePlan_InvalidName()
        {
            using (new TransactionScopeWrapper())
            {
                CreatePlanDto newPlan = _GetNewPlan();
                newPlan.Name = null;

                var exception = Assert.Throws<Exception>(() => _PlanService.CreatePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01)));

                Assert.That(exception.Message, Is.EqualTo("Invalid plan name"));
            }
        }

        private static CreatePlanDto _GetNewPlan()
        {
            return new CreatePlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                SpotLengthId = 1,
                Status = Entities.Enums.PlanStatusEnum.Working
            };
        }
    }
}
