﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Plan;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices.Plan.PlanBuying
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PlanBuyingInventoryEngineTests
    {
        private readonly IPlanBuyingInventoryEngine _PlanBuyingInventoryEngine;
        private readonly IPlanRepository _PlanRepository;
        private readonly IStationRepository _StationRepository;
        private readonly InventoryFileTestHelper _InventoryFileTestHelper;

        public PlanBuyingInventoryEngineTests()
        {
            _PlanBuyingInventoryEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanBuyingInventoryEngine>();
            _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _StationRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _InventoryFileTestHelper = new InventoryFileTestHelper();
        }

        [Test]
        [Category("short_running")]
        public void GetsFallbackInventoryForBuying()
        {
            ProgramInventoryOptionalParametersDto parameters = new ProgramInventoryOptionalParametersDto();
            parameters.HUTBookId = 437;
            parameters.ShareBookId = 437;
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanBuyingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1196);
                plan.CreativeLengths.First().SpotLengthId = 2;
                var result = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan, 
                   parameters,
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetsInventoryForBuying()
        {
            ProgramInventoryOptionalParametersDto parameters = new ProgramInventoryOptionalParametersDto();
            parameters.HUTBookId = 437;
            parameters.ShareBookId = 437;
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanBuyingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1198);

                plan.CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 1,
                        Weight = 50
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 2,
                        Weight = 50
                    }
                };

                var result = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    _GetAvailableInventorySources(),
                    diagnostic);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingInventoryProgram), "ManifestId");
                jsonResolver.Ignore(typeof(BasePlanInventoryProgram.ManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingInventoryProgram.ManifestWeek), "Id");
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
        [Category("long_running")]
        public void RunWithHiatusDayTest()
        {
            ProgramInventoryOptionalParametersDto parameters = new ProgramInventoryOptionalParametersDto();
            parameters.HUTBookId = 437;
            parameters.ShareBookId = 437;

            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanBuyingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1199);
                var result = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("long_running")]
        public void RunSpotLengthTest()
        {
            ProgramInventoryOptionalParametersDto parameters = new ProgramInventoryOptionalParametersDto();
            parameters.HUTBookId = 437;
            parameters.ShareBookId = 437;
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanBuyingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1197);
                var result = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    parameters,
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("long_running")]
        public void RunAffilitateContentRestrictionTest()
        {
            ProgramInventoryOptionalParametersDto parameters = new ProgramInventoryOptionalParametersDto();
            parameters.HUTBookId = 437;
            parameters.ShareBookId = 437;
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanBuyingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1197);
                plan.Dayparts[0].Restrictions.AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                {
                    Affiliates = new List<LookupDto>()
                };
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.Affiliates.Add(new LookupDto(8, "CW"));
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.ContainType = ContainTypeEnum.Exclude;
                _PlanRepository.SavePlan(plan, "IntegrationTestUser", new System.DateTime(2020, 11, 3));
                // Result has affiliates CW and FOX.
                var result = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("long_running")]
        public void RunAffilitateContentRestrictionUpdateMonthDetailsTest()
        {
            ProgramInventoryOptionalParametersDto parameters = new ProgramInventoryOptionalParametersDto();
            parameters.HUTBookId = 437;
            parameters.ShareBookId = 437;
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanBuyingJobDiagnostic();
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
                plan.Dayparts[0].Restrictions.AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                {
                    Affiliates = new List<LookupDto>()
                };
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.Affiliates.Add(new LookupDto(8, "CW"));
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.ContainType = ContainTypeEnum.Exclude;
                _PlanRepository.SavePlan(plan, "IntegrationTestUser", new System.DateTime(2020, 11, 3));
                // Result has affiliates CW and FOX.
                var result = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetInventoryForPlanFlightDaysTest()
        {
            ProgramInventoryOptionalParametersDto parameters = new ProgramInventoryOptionalParametersDto();
            parameters.HUTBookId = 437;
            parameters.ShareBookId = 437;
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanBuyingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1197);
                plan.FlightDays.Clear();
                plan.FlightDays.Add(1);
                plan.FlightDays.Add(5);
                _PlanRepository.SavePlan(plan, "IntegrationTestUser", new System.DateTime(2020, 2, 27));
                var result = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    _GetAvailableInventorySources(),
                    diagnostic);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("long_running")]
        public void RunWithProgramWithMultipleAudiencesTest()
        {
            ProgramInventoryOptionalParametersDto parameters = new ProgramInventoryOptionalParametersDto();
            parameters.HUTBookId = 437;
            parameters.ShareBookId = 437;
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanBuyingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1200);
                var result = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    parameters,
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
