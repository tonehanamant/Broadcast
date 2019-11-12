using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.IntegrationTests.Stubs;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanPricingServiceTests
    {
        private readonly IPlanPricingService _PlanPricingService;

        public PlanPricingServiceTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IPricingApiClient, PricingApiClientStub>();
            _PlanPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunNoDataTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingService.GetInventoryForPlan(1196);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingService.GetInventoryForPlan(1198);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunWithHiatusDayTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingService.GetInventoryForPlan(1199);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunSpotLengthTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingService.GetInventoryForPlan(1197);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunWithProgramWithMultipleAudiencesTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingService.GetInventoryForPlan(1200);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunWithParametersTest()
        {
            using (new TransactionScopeWrapper())
            {
                _PlanPricingService.Run(new PlanPricingRequestDto
                {
                    PlanId = 1197,
                    MaxCpm = 10m,
                    MinCpm = 1m,
                    BudgetGoal = 1000,
                    CompetitionFactor = 0.1,
                    CpmGoal = 5m,
                    ImpressionsGoal = 50000,
                    InflationFactor = 0.5,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapType = UnitCapEnum.PerDay,
                    InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                    {
                        new PlanPricingInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanPricingInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanPricingInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanPricingInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanPricingInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanPricingInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanPricingInventorySourceDto{Id = 12, Percentage = 8},
                    }
                });

                var executions = _PlanPricingService.GetPlanPricingRuns(1197);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(executions));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetUnitCaps()
        {
            var unitCaps = _PlanPricingService.GetUnitCaps();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(unitCaps));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanPricingDefaults()
        {
            var ppDefaults = _PlanPricingService.GetPlanPricingDefaults();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(ppDefaults));
        }
    }
}
