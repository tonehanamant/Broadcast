﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanPricingInventoryEngineTests
    {
        private IPlanPricingInventoryEngine _PlanPricingInventoryEngine;
        private IPlanRepository _PlanRepository;
        private IStationRepository _StationRepository;
        private InventoryFileTestHelper _InventoryFileTestHelper;

        [SetUp]
        public void SetUp()
        {
            _PlanPricingInventoryEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingInventoryEngine>();
            _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _StationRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _InventoryFileTestHelper = new InventoryFileTestHelper();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetsFallbackInventoryForPricing()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
                var plan = _PlanRepository.GetPlan(1196);
                plan.CreativeLengths.First().SpotLengthId = 2;
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan, 
                    new ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic, Guid.NewGuid());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetsInventoryForPricing()
        {
            using (new TransactionScopeWrapper())
            {
                var diagnostic = new PlanPricingJobDiagnostic();
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

                var engine = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingInventoryEngine>();
                var result = engine.GetInventoryForPlan(
                    plan,
                    new ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic, Guid.NewGuid());

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingInventoryProgram), "ManifestId");
                jsonResolver.Ignore(typeof(BasePlanInventoryProgram.ManifestDaypart), "Id");
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
                    new ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic, Guid.NewGuid());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetInventoryForQuoteTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new QuoteRequestDto
                {
                    FlightStartDate = new DateTime(2018, 10, 1),
                    FlightEndDate = new DateTime(2018, 10, 7),
                    FlightHiatusDays = new List<DateTime>(),
                    FlightDays = new List<int> { 1, 5 },
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength
                        {
                            SpotLengthId = 2
                        }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 1,
                            StartTimeSeconds = 10,
                            EndTimeSeconds = 1699,
                            Restrictions = new PlanDaypartDto.RestrictionsDto
                            {
                                ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                                {
                                    Programs = new List<ProgramDto>
                                    {
                                        new ProgramDto
                                        {
                                            Name = "Early news"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    Margin = 20,
                    HUTBookId = 437,
                    ShareBookId = 437,
                    AudienceId = 33,
                    SecondaryAudiences = new List<PlanAudienceDto>
                    {
                        new PlanAudienceDto
                        {
                            AudienceId = 37
                        }
                    }
                };

                var planPricingInventoryEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingInventoryEngine>();
                ((PlanPricingInventoryEngine) planPricingInventoryEngine)._ThresholdInSecondsForProgramIntersect = new Lazy<int>(() => 100);

                var result = planPricingInventoryEngine.GetInventoryForQuote(request, Guid.NewGuid());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
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
                    new ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic, Guid.NewGuid());

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
                    new ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic, Guid.NewGuid());

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
                plan.Dayparts[0].Restrictions.AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                {
                    Affiliates = new List<LookupDto>()
                };
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.Affiliates.Add(new LookupDto(8, "CW"));
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.ContainType = ContainTypeEnum.Exclude;
                _PlanRepository.SavePlan(plan, "IntegrationTestUser", new System.DateTime(2020, 11, 3));
                // Result has affiliates CW and FOX.
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    new ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic, Guid.NewGuid());

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
                plan.Dayparts[0].Restrictions.AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                {
                    Affiliates = new List<LookupDto>()
                };
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.Affiliates.Add(new LookupDto(8, "CW"));
                plan.Dayparts[0].Restrictions.AffiliateRestrictions.ContainType = ContainTypeEnum.Exclude;
                _PlanRepository.SavePlan(plan, "IntegrationTestUser", new System.DateTime(2020, 11, 3));
                // Result has affiliates CW and FOX.
                var result = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    new ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic, Guid.NewGuid());

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
                    new ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic, Guid.NewGuid());

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
                    new ProgramInventoryOptionalParametersDto(),
                    _GetAvailableInventorySources(),
                    diagnostic, Guid.NewGuid());

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
