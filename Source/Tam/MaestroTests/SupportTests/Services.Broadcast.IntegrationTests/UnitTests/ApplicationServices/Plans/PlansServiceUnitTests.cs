using Common.Services.Repositories;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Plans
{
    [TestFixture]
    public class PlansServiceUnitTests
    {
        #region Construction

        [Test]
        public void Construction()
        {
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var planValidator = new Mock<IPlanValidator>();
            var planBudgetDeliveryCalculator = new Mock<IPlanBudgetDeliveryCalculator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var planAggregator = new Mock<IPlanAggregator>();
            var nsiUniverseService = new Mock<INsiUniverseService>();
            var broadcastAudienceCacheMock = new Mock<IBroadcastAudiencesCache>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();

            var tc = new PlanService(broadcastDataRepositoryFactory.Object, planValidator.Object,
                planBudgetDeliveryCalculator.Object, mediaMonthAndWeekAggregateCache.Object, planAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                nsiUniverseService.Object, broadcastAudienceCacheMock.Object, spotLengthEngine.Object);

            Assert.IsNotNull(tc);
        }

        #endregion // #region Construction

        #region Dispatch Aggregation

        [Test]
        public void DispatchAggregation_WasTriggeredOnSave()
        {
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var planValidator = new Mock<IPlanValidator>();
            var planBudgetDeliveryCalculator = new Mock<IPlanBudgetDeliveryCalculator>();
            var planRepository = new Mock<IPlanRepository>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var audiencesCache = new Mock<IBroadcastAudiencesCache>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();

            var saveNewPlanCalls = new List<DateTime>();
            planRepository.Setup(s => s.SaveNewPlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => saveNewPlanCalls.Add(DateTime.Now))
                .Returns(2);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IPlanRepository>())
                .Returns(planRepository.Object);
            var planSummaryRepo = new Mock<IPlanSummaryRepository>();
            var setStatusCalls = new List<Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>>();
            planSummaryRepo.Setup(s =>
                    s.SetProcessingStatusForPlanSummary(It.IsAny<int>(), It.IsAny<PlanAggregationProcessingStatusEnum>()))
                .Callback<int, PlanAggregationProcessingStatusEnum>((i, s) => setStatusCalls.Add(new Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>(i, s, DateTime.Now)));
            var saveSummaryCalls = new List<Tuple<int, PlanSummaryDto, DateTime>>();
            planSummaryRepo.Setup(s => s.SaveSummary(It.IsAny<PlanSummaryDto>()))
                .Callback<PlanSummaryDto>((s) => saveSummaryCalls.Add(new Tuple<int, PlanSummaryDto, DateTime>(Thread.CurrentThread.ManagedThreadId, s, DateTime.Now)))
                .Returns(3);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IPlanSummaryRepository>())
                .Returns(planSummaryRepo.Object);
            var planAggregator = new Mock<IPlanAggregator>();
            var aggregateCallCount = 0;
            var aggregateReturn = new PlanSummaryDto();
            planAggregator.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Callback(() => aggregateCallCount++)
                .Returns(aggregateReturn);
            var campaignAggJobTrigger = new Mock<ICampaignAggregationJobTrigger>();
            var nsiUniverseService = new Mock<INsiUniverseService>();
            nsiUniverseService.Setup(n => n.GetAudienceUniverseForMediaMonth(It.IsAny<int>(), It.IsAny<int>())).Returns(1000000);
            var broadcastAudienceCacheMock = new Mock<IBroadcastAudiencesCache>();
            broadcastAudienceCacheMock.Setup(a => a.GetDefaultAudience()).Returns(new Entities.BroadcastAudience());

            var tc = new PlanService(broadcastDataRepositoryFactory.Object, planValidator.Object,
                planBudgetDeliveryCalculator.Object, mediaMonthAndWeekAggregateCache.Object, planAggregator.Object,
                campaignAggJobTrigger.Object, nsiUniverseService.Object, broadcastAudienceCacheMock.Object, spotLengthEngine.Object);

            var plan = _GetNewPlan();
            var campaignId = plan.CampaignId;
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;

            tc.SavePlan(plan, modifiedWho, modifiedWhen);
            Thread.Sleep(200);

            Assert.AreEqual(1, saveNewPlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, setStatusCalls.Count, "Invalid call count.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2, "Invalid 'in process' processing status.");
            Assert.AreEqual(1, aggregateCallCount, "Invalid call count.");
            Assert.AreEqual(1, saveSummaryCalls.Count, "Invalid call count.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, saveSummaryCalls[0].Item2.ProcessingStatus, "Invalid final processing status.");
            Assert.AreNotEqual(currentThreadId, saveSummaryCalls[0].Item1, "PlanSave and PlanAggregate should be on separate threads.");
            var planSavedTime = saveNewPlanCalls[0];
            var setInProgressTime = setStatusCalls[0].Item3;
            var summarySavedTime = saveSummaryCalls[0].Item3;
            Assert.IsTrue(planSavedTime <= setInProgressTime, "Plan should have been saved before aggregation started.");
            Assert.IsTrue(setInProgressTime <= summarySavedTime, "Aggregation started should be set before summary saved");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2);
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, saveSummaryCalls[0].Item2.ProcessingStatus);
            campaignAggJobTrigger.Verify(s => s.TriggerJob(campaignId, modifiedWho), Times.Once);
        }

        [Test]
        public void DispatchAggregation_WithAggregationError()
        {
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var planValidator = new Mock<IPlanValidator>();
            var planBudgetDeliveryCalculator = new Mock<IPlanBudgetDeliveryCalculator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var planRepository = new Mock<IPlanRepository>();
            var audiencesCache = new Mock<IBroadcastAudiencesCache>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            var saveNewPlanCalls = new List<DateTime>();
            planRepository.Setup(s => s.SaveNewPlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => saveNewPlanCalls.Add(DateTime.Now))
                .Returns(2);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IPlanRepository>())
                .Returns(planRepository.Object);
            var planSummaryRepo = new Mock<IPlanSummaryRepository>();

            var setStatusCalls = new List<Tuple<int, PlanAggregationProcessingStatusEnum, int, DateTime>>();
            planSummaryRepo.Setup(s =>
                    s.SetProcessingStatusForPlanSummary(It.IsAny<int>(), It.IsAny<PlanAggregationProcessingStatusEnum>()))
                .Callback<int, PlanAggregationProcessingStatusEnum>((i, s) => setStatusCalls.Add(new Tuple<int, PlanAggregationProcessingStatusEnum, int, DateTime>(i, s, Thread.CurrentThread.ManagedThreadId, DateTime.Now)));
            var saveSummaryCalls = new List<Tuple<int, PlanSummaryDto, DateTime>>();
            planSummaryRepo.Setup(s => s.SaveSummary(It.IsAny<PlanSummaryDto>()))
                .Callback<PlanSummaryDto>((s) => saveSummaryCalls.Add(new Tuple<int, PlanSummaryDto, DateTime>(Thread.CurrentThread.ManagedThreadId, s, DateTime.Now)))
                .Returns(3);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IPlanSummaryRepository>())
                .Returns(planSummaryRepo.Object);
            var planAggregator = new Mock<IPlanAggregator>();
            var aggregateCallCount = 0;
            var aggregateReturn = new PlanSummaryDto();
            planAggregator.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Callback(() =>
                {
                    aggregateCallCount++;
                    throw new Exception("Test exception thrown during aggregation.");
                })
                .Returns(aggregateReturn);
            var campaignAggJobTrigger = new Mock<ICampaignAggregationJobTrigger>();
            var nsiUniverseService = new Mock<INsiUniverseService>();
            nsiUniverseService.Setup(n => n.GetAudienceUniverseForMediaMonth(It.IsAny<int>(), It.IsAny<int>())).Returns(1000000);

            var broadcastAudienceCacheMock = new Mock<IBroadcastAudiencesCache>();
            broadcastAudienceCacheMock.Setup(a => a.GetDefaultAudience()).Returns(new Entities.BroadcastAudience());

            var tc = new PlanService(broadcastDataRepositoryFactory.Object, planValidator.Object,
                planBudgetDeliveryCalculator.Object, mediaMonthAndWeekAggregateCache.Object, planAggregator.Object,
                campaignAggJobTrigger.Object, nsiUniverseService.Object, broadcastAudienceCacheMock.Object, spotLengthEngine.Object);
            var plan = _GetNewPlan();
            var campaignId = plan.CampaignId;
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;

            tc.SavePlan(plan, modifiedWho, modifiedWhen);
            Thread.Sleep(200);

            Assert.AreEqual(1, saveNewPlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(2, setStatusCalls.Count, "Invalid call count.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2, "Invalid 'in process' processing status.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Error, setStatusCalls[1].Item2, "Invalid final processing status.");
            Assert.AreEqual(1, aggregateCallCount, "Invalid call count.");
            Assert.AreEqual(0, saveSummaryCalls.Count, "Invalid call count.");
            var planSavedTime = saveNewPlanCalls[0];
            var setInProgressTime = setStatusCalls[0].Item4;
            var finalStatusSavedTime = setStatusCalls[1].Item4;
            Assert.IsTrue(planSavedTime <= setInProgressTime, "Plan should have been saved before aggregation started.");
            Assert.IsTrue(setInProgressTime <= finalStatusSavedTime, "Aggregation started should be set before summary saved");
            Assert.AreNotEqual(currentThreadId, setStatusCalls[1].Item3, "PlanSave and PlanAggregate should be on separate threads.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2);
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Error, setStatusCalls[1].Item2);
            campaignAggJobTrigger.Verify(s => s.TriggerJob(campaignId, modifiedWho), Times.Once);
        }

        #endregion // #region Dispatch Aggregation

        #region Helpers

        private static PlanDto _GetNewPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                SpotLengthId = 1,
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019,1,20),
                    new DateTime(2019,4,15)
                },
                AudienceId = 31,        //HH
                AudienceType = Entities.Enums.AudienceTypeEnum.Nielsen,
                HUTBookId = 436,
                PostingType = Entities.Enums.PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                CPM = 12m,
                DeliveryImpressions = 100d,
                CoverageGoalPercent = 80.5,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUs = 20, Rank = 1, ShareOfVoicePercent = 22.2},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUs = 32.5, Rank = 2, ShareOfVoicePercent = 34.5}
                },
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto {MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUs = 5.5, Rank = 5 },
                    new PlanBlackoutMarketDto {MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUs = 2.5, Rank = 8 },
                },
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto{ DaypartCodeId = 2, StartTimeSeconds = 0, EndTimeSeconds = 2000, WeightingGoalPercent = 28.0 },
                    new PlanDaypartDto{ DaypartCodeId = 11, StartTimeSeconds = 1500, EndTimeSeconds = 2788, WeightingGoalPercent = 33.2 }
                },
                Vpvh = 0.234543
            };
        }

        #endregion // #region Helpers
    }
}