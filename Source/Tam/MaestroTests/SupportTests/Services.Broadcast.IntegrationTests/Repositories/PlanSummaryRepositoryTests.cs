using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
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
                _CreateAndSavePlan(out int planId, out int planVersionId);
                var dtoToSave = _GetPlanSummaryDto(planId, planVersionId);
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
                _CreateAndSavePlan(out int planId, out int planVersionId);
                var dtoToSave = _GetPlanSummaryDto(planId, planVersionId);
                _PlanSummaryRepository.SaveSummary(dtoToSave);
                var startSummaryDto = _PlanSummaryRepository.GetSummaryForPlan(planId);
                var beforeStatus = startSummaryDto.ProcessingStatus;

                _PlanSummaryRepository.SetProcessingStatusForPlanSummary(planVersionId, PlanAggregationProcessingStatusEnum.Error);

                PlanSummaryDto afterSummaryDto = _PlanSummaryRepository.GetSummaryForPlan(planId);
                var afterStatus = afterSummaryDto.ProcessingStatus;
                Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, beforeStatus);
                Assert.AreEqual(PlanAggregationProcessingStatusEnum.Error, afterStatus);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(afterSummaryDto, _GetJsonSettings()));
            }
        }

        #region Helpers

        private void _CreateAndSavePlan(out int planId, out int planVersionId)
        {
            var modifiedUser = "RepoIntegrationTestUser";
            var modifiedDateTime = new DateTime(2018, 10, 17, 12, 0, 0);
            var planDto = new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                AudienceType = AudienceTypeEnum.Nielsen,
                AudienceId = 31,
                ShareBookId = 437,
                HUTBookId = 437,
                PostingType = PostingTypeEnum.NSI,
                Status = PlanStatusEnum.Working,
                ModifiedBy = modifiedUser,
                ModifiedDate = modifiedDateTime,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2019, 8, 1),
                FlightEndDate = new DateTime(2019, 9, 1),
                Budget = 500000m,
                TargetImpressions = 50000000,
                TargetCPM = 10m,
                CoverageGoalPercent = 60,
                TargetRatingPoints = 0.00248816152650979,
                TargetCPP = 200951583.9999m,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 }
            };
            _PlanRepository.SaveNewPlan(planDto, modifiedUser, modifiedDateTime);
            planId = planDto.Id;
            planVersionId = planDto.VersionId;
        }

        private PlanSummaryDto _GetPlanSummaryDto(int planId, int planVersionId)
        {
            var dto = new PlanSummaryDto
            {
                ProcessingStatus = PlanAggregationProcessingStatusEnum.Idle,
                PlanId = planId,
                VersionId = planVersionId,
                TotalHiatusDays = 2,
                TotalActiveDays = 20,
                AvailableMarketCount = 12,
                AvailableMarketTotalUsCoveragePercent = 68.7,
                BlackoutMarketCount = 3,
                BlackoutMarketTotalUsCoveragePercent = 13.8,
                AvailableMarketsWithSovCount = 2,                
                ProductName = "ProductTwo",
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
            jsonResolver.Ignore(typeof(PlanSummaryDto), "VersionId");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        #endregion // #region Helpers
    }
}