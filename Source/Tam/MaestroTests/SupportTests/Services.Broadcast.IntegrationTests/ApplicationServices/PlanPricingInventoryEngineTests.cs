using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using static Services.Broadcast.Entities.Plan.PlanDaypartDto;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram;
using static Services.Broadcast.Entities.ProposalProgramDto;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanPricingInventoryEngineTests
    {
        private readonly IPlanPricingInventoryEngine _PlanPricingInventoryEngine;
        private readonly IPlanRepository _PlanRepository;
        private readonly IStationRepository _StationRepository;
        private readonly InventoryFileTestHelper _InventoryFileTestHelper;

        public PlanPricingInventoryEngineTests()
        {
            _PlanPricingInventoryEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingInventoryEngine>();
            _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _StationRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _InventoryFileTestHelper = new InventoryFileTestHelper();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void RunNoDataTest()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1196);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void RunTest()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();

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
                    _GetAvailableInventorySources(),
                    diagnostic);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingInventoryProgram), "ManifestId");
                jsonResolver.Ignore(typeof(ManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(ManifestWeek), "Id");
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
        [Category("long_running")]
        public void RunWithHiatusDayTest()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1199);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public void GetInnventoryforPlanTestOnlyEnhancedPrograms()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                var fileId = _InventoryFileTestHelper.UploadOpenMarketInventoryFile("Open Market Pricing Programs.xml", null, true);
                var enhancements = new List<string>() { "SER", "Entertainment", "84900", "2100", "2020-01-04", "2020-01-05" };
                _InventoryFileTestHelper.EnhanceProgramsForFileId(fileId, enhancements);
                var plan = _PlanRepository.GetPlan(1197);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingInventoryProgram), "ManifestId");
                jsonResolver.Ignore(typeof(ManifestWeek), "Id");
                jsonResolver.Ignore(typeof(ManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(ManifestDaypart), "PrimaryProgramId");
                jsonResolver.Ignore(typeof(ManifestDaypart.Program), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetInnventoryforPlanTestNewProgramsWithoutEnhancementAreNotSelected()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                _InventoryFileTestHelper.UploadOpenMarketInventoryFile("Open Market Pricing Programs.xml", null, false);
                var plan = _PlanRepository.GetPlan(1197);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }
                
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void RunSpotLengthTest()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1197);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void RunAffilitateContentRestrictionTest()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1197);
                plan.Dayparts[0].Restrictions.AffiliateRestrictions = new RestrictionsDto.AffiliateRestrictionsDto
                {
                    Affiliates = new List<LookupDto>()
                };
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.Affiliates.Add(new LookupDto(8, "CW"));
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.ContainType = ContainTypeEnum.Exclude;
                _PlanRepository.SavePlan(plan, "IntegrationTestUser", new System.DateTime(2020, 11, 3));
                // Result has affiliates CW and FOX.
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void RunAffilitateContentRestrictionUpdateMonthDetailsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1197);
                _StationRepository.SaveStationMonthDetails(new Entities.StationMonthDetailDto
                {
                    Affiliation = "CW2",
                    MediaMonthId = 500,
                    DistributorCode = 39,
                    MarketCode = 101,
                    StationId = 575
                });
                // Since affiliate has been updated, it shouldn't be filtered by inventory gathering.
                plan.Dayparts[0].Restrictions.AffiliateRestrictions = new RestrictionsDto.AffiliateRestrictionsDto
                {
                    Affiliates = new List<LookupDto>()
                };
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.Affiliates.Add(new LookupDto(8, "CW"));
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.ContainType = ContainTypeEnum.Exclude;
                _PlanRepository.SavePlan(plan, "IntegrationTestUser", new System.DateTime(2020, 11, 3));
                // Result has affiliates CW and FOX.
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetInventoryForPlanFlightDaysTest()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1197);
                plan.FlightDays.Clear();
                plan.FlightDays.Add(1);
                plan.FlightDays.Add(5);
                _PlanRepository.SavePlan(plan, "IntegrationTestUser", new System.DateTime(2020, 2, 27));
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void RunWithProgramWithMultipleAudiencesTest()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1200);
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic);

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
