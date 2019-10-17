﻿using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class PlanSummaryRepositoryTests
    {
        private readonly IPlanRepository _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
        private readonly IPlanSummaryRepository _PlanSummaryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanSummaryRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanSummaryAndChangeQuarters()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = _CreateAndSavePlan();
                var dtoToSave = _GetPlanSummaryDto(planId);
                _PlanSummaryRepository.SaveSummary(dtoToSave);
                var savedSummary = _PlanSummaryRepository.GetSummaryForPlan(planId);

                Assert.AreEqual(3, savedSummary.PlanSummaryQuarters.Count);
                Assert.AreEqual(1, savedSummary.PlanSummaryQuarters[0].Quarter);
                Assert.AreEqual(2017, savedSummary.PlanSummaryQuarters[0].Year);
                Assert.AreEqual(2, savedSummary.PlanSummaryQuarters[1].Quarter);
                Assert.AreEqual(2017, savedSummary.PlanSummaryQuarters[1].Year);
                Assert.AreEqual(3, savedSummary.PlanSummaryQuarters[2].Quarter);
                Assert.AreEqual(2017, savedSummary.PlanSummaryQuarters[2].Year);

                dtoToSave.PlanSummaryQuarters = new List<PlanSummaryQuarterDto>
                {
                    new PlanSummaryQuarterDto {Quarter = 1, Year = 2019},
                    new PlanSummaryQuarterDto {Quarter = 2, Year = 2019},
                    new PlanSummaryQuarterDto {Quarter = 3, Year = 2019}
                };

                _PlanSummaryRepository.SaveSummary(dtoToSave);
                PlanSummaryDto modifiedSummary = _PlanSummaryRepository.GetSummaryForPlan(planId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(modifiedSummary, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ChangeSummaryAggregationStatus()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = _CreateAndSavePlan();
                var dtoToSave = _GetPlanSummaryDto(planId);
                _PlanSummaryRepository.SaveSummary(dtoToSave);
                var startSummaryDto = _PlanSummaryRepository.GetSummaryForPlan(planId);
                var beforeStatus = startSummaryDto.ProcessingStatus;

                _PlanSummaryRepository.SetProcessingStatusForPlanSummary(planId, PlanAggregationProcessingStatusEnum.Error);

                PlanSummaryDto afterSummaryDto = _PlanSummaryRepository.GetSummaryForPlan(planId);
                var afterStatus = afterSummaryDto.ProcessingStatus;
                Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, beforeStatus);
                Assert.AreEqual(PlanAggregationProcessingStatusEnum.Error, afterStatus);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(afterSummaryDto, _GetJsonSettings()));
            }
        }

        #region Helpers

        private int _CreateAndSavePlan()
        {
            var modifiedUser = "RepoIntegrationTestUser";
            var modifiedDateTime = new DateTime(2018, 10, 17, 12, 0, 0);
            var planDto = new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                SpotLengthId = 1,
                AudienceType = AudienceTypeEnum.Nielsen,
                AudienceId = 31,
                ShareBookId = 437,
                HUTBookId = 437,
                PostingType = PostingTypeEnum.NSI,
                Status = PlanStatusEnum.Working,
                ModifiedBy = modifiedUser,
                ModifiedDate = modifiedDateTime,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 8, 1),
                FlightEndDate = new DateTime(2019,9,1),
                Budget = 500000m,
                DeliveryImpressions = 50000000,
                CPM = 10m,
                CoverageGoalPercent = 60,
                DeliveryRatingPoints = 0.00248816152650979,
                CPP = 200951583.9999m
            };
            var planId = _PlanRepository.SaveNewPlan(planDto, modifiedUser, modifiedDateTime);
            return planId;
        }

        private PlanSummaryDto _GetPlanSummaryDto(int planId)
        {
            var dto = new PlanSummaryDto
            {
                ProcessingStatus = PlanAggregationProcessingStatusEnum.Idle,
                PlanId = planId,
                TotalHiatusDays = 2,
                TotalActiveDays = 20,
                AvailableMarketCount = 12,
                AvailableMarketTotalUsCoveragePercent = 68.7,
                BlackoutMarketCount = 3,
                BlackoutMarketTotalUsCoveragePercent = 13.8,
                AvailableMarketsWithSovCount = 2,                
                ProductName = "ProductTwo",
                AudienceName = "AudienceThree",
                PlanSummaryQuarters = new List<PlanSummaryQuarterDto>
                {
                    new PlanSummaryQuarterDto { Quarter = 1, Year = 2017},
                    new PlanSummaryQuarterDto { Quarter = 2, Year = 2017},
                    new PlanSummaryQuarterDto { Quarter = 3, Year = 2017}
                }
            };
            return dto;
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(PlanSummaryDto), "PlanId");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        #endregion // #region Helpers
    }
}