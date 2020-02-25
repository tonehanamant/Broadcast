using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using static Services.Broadcast.Entities.ProposalProgramDto;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanPricingInventoryEngineTests
    {
        private readonly IPlanPricingInventoryEngine _PlanPricingInventoryEngine;
        private readonly IPlanRepository _PlanRepository;
        private readonly InventoryFileTestHelper _InventoryFileTestHelper;

        public PlanPricingInventoryEngineTests()
        {
            _PlanPricingInventoryEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingInventoryEngine>();
            _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _InventoryFileTestHelper = new InventoryFileTestHelper();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunNoDataTest()
        {
            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1196);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunTest()
        {
            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile(
                    "PricingModel_Barter.xlsx", 
                    processInventoryRatings: true, 
                    processInventoryProgramsJob: false);

                _InventoryFileTestHelper.UploadProprietaryInventoryFile(
                    "PricingModel_OAndO.xlsx", 
                    processInventoryRatings: true, 
                    processInventoryProgramsJob: false);

                var plan = _PlanRepository.GetPlan(1198);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources());

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingInventoryProgram), "ManifestId");
                jsonResolver.Ignore(typeof(ManifestDaypartDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(result, jsonSettings);

                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunTest_DoesNotShowOpenMarketInventory_WhenItIsNotPassed()
        {
            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile(
                        "PricingModel_OAndO.xlsx",
                        processInventoryRatings: true,
                        processInventoryProgramsJob: false);

                var inventorySources = _GetAvailableInventorySources().Except(new List<int> { 1 });
                var plan = _PlanRepository.GetPlan(1198);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    inventorySources);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingInventoryProgram), "ManifestId");
                jsonResolver.Ignore(typeof(ManifestDaypartDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(result, jsonSettings);

                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunWithHiatusDayTest()
        {
            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1199);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunSpotLengthTest()
        {
            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1197);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunWithProgramWithMultipleAudiencesTest()
        {
            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1200);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        private List<int> _GetAvailableInventorySources()
        {
            return new List<int>
            {
                1,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10,
                11,
                12
            };
        }
    }
}
