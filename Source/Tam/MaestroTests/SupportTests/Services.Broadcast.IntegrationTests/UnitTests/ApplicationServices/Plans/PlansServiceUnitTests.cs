using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Plans
{
    [TestFixture]
    [Category("short_running")]
    public class PlansServiceUnitTests
    {
        private PlanService _PlanService;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IPlanRepository> _PlanRepositoryMock;
        private Mock<IPlanSummaryRepository> _PlanSummaryRepositoryMock;
        private Mock<IInventoryProprietarySummaryRepository> _InventoryProprietarySummaryRepositoryMock;
        private Mock<IDayRepository> _DayRepositoryMock;
        private Mock<IPlanValidator> _PlanValidatorMock;
        private Mock<IPlanBudgetDeliveryCalculator> _PlanBudgetDeliveryCalculatorMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private Mock<IPlanAggregator> _PlanAggregatorMock;
        private Mock<ICampaignAggregationJobTrigger> _CampaignAggregationJobTriggerMock;
        private Mock<ISpotLengthEngine> _SpotLengthEngineMock;
        private Mock<IBroadcastLockingManagerApplicationService> _BroadcastLockingManagerApplicationServiceMock;
        private Mock<IPlanPricingService> _PlanPricingServiceMock;
        private Mock<IPlanBuyingService> _PlanBuyingServiceMock;
        private Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock;
        private Mock<IStandardDaypartService> _StandardDaypartServiceMock;
        private Mock<IWeeklyBreakdownEngine> _WeeklyBreakdownEngineMock;
        private Mock<ICreativeLengthEngine> _CreativeLengthEngineMock;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;

        [SetUp]
        public void CreatePlanService()
        {
            // Create Mocks
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();
            _PlanSummaryRepositoryMock = new Mock<IPlanSummaryRepository>();
            _DayRepositoryMock = new Mock<IDayRepository>();
            _PlanValidatorMock = new Mock<IPlanValidator>();
            _PlanBudgetDeliveryCalculatorMock = new Mock<IPlanBudgetDeliveryCalculator>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _PlanAggregatorMock = new Mock<IPlanAggregator>();
            _CampaignAggregationJobTriggerMock = new Mock<ICampaignAggregationJobTrigger>();
            _BroadcastLockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
            _PlanPricingServiceMock = new Mock<IPlanPricingService>();
            _PlanBuyingServiceMock = new Mock<IPlanBuyingService>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            _StandardDaypartServiceMock = new Mock<IStandardDaypartService>();
            _WeeklyBreakdownEngineMock = new Mock<IWeeklyBreakdownEngine>();
            _CreativeLengthEngineMock = new Mock<ICreativeLengthEngine>();
            _InventoryProprietarySummaryRepositoryMock = new Mock<IInventoryProprietarySummaryRepository>();
            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.LockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true,
                    LockedUserName = "IntegrationUser"
                });
            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true,
                    LockedUserName = "IntegrationUser"
                });

            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            _DataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignRepository>())
                .Returns(campaignRepositoryMock.Object);

            _DayRepositoryMock = new Mock<IDayRepository>();
            _DayRepositoryMock.Setup(s => s.GetDays()).Returns(new List<Day>());
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IDayRepository>())
                .Returns(_DayRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IPlanRepository>())
                .Returns(_PlanRepositoryMock.Object);

            _DataRepositoryFactoryMock
               .Setup(s => s.GetDataRepository<IInventoryProprietarySummaryRepository>())
               .Returns(_InventoryProprietarySummaryRepositoryMock.Object);

            var daypartCodeRepository = new Mock<IStandardDaypartRepository>();
            daypartCodeRepository
                .Setup(s => s.GetAllStandardDaypartsWithAllData())
                .Returns(_GetDaypartCodeDefaultsFull());
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(daypartCodeRepository.Object);

            _PlanSummaryRepositoryMock = new Mock<IPlanSummaryRepository>();
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IPlanSummaryRepository>())
                .Returns(_PlanSummaryRepositoryMock.Object);

            _PlanPricingServiceMock
                .Setup(s => s.GetPlanPricingDefaults())
                .Returns(_GetPlanPricingDefaults());

            _PlanBuyingServiceMock
                .Setup(s => s.GetPlanBuyingDefaults())
                .Returns(GetPlanBuyingDefaults());

            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Returns(_GetPlanDeliveryBudget());

            _StandardDaypartServiceMock
                .Setup(s => s.GetAllStandardDayparts())
                .Returns(new List<StandardDaypartDto>());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByStandardDaypart(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByStandardDaypart>
                {
                    new WeeklyBreakdownByStandardDaypart
                    {
                        Impressions = 1000,
                        Budget = 100,
                        StandardDaypartId = 2
                    },
                    new WeeklyBreakdownByStandardDaypart
                    {
                        Impressions = 1000,
                        Budget = 100,
                        StandardDaypartId = 11
                    }
                });

            _LaunchDarklyClientStub = new LaunchDarklyClientStub();
            _LaunchDarklyClientStub.FeatureToggles.Add("FEATURE_TOGGLE_ENABLE_PRICING_IN_EDIT", true);
            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);

            _PlanService = new PlanService(
                    _DataRepositoryFactoryMock.Object,
                    _PlanValidatorMock.Object,
                    _PlanBudgetDeliveryCalculatorMock.Object,
                    _MediaMonthAndWeekAggregateCacheMock.Object,
                    _PlanAggregatorMock.Object,
                    _CampaignAggregationJobTriggerMock.Object,
                    _SpotLengthEngineMock.Object,
                    _BroadcastLockingManagerApplicationServiceMock.Object,
                    _PlanPricingServiceMock.Object,
                    _PlanBuyingServiceMock.Object,
                    _QuarterCalculationEngineMock.Object,
                    _StandardDaypartServiceMock.Object,
                    _WeeklyBreakdownEngineMock.Object,
                    _CreativeLengthEngineMock.Object,
                    featureToggleHelper
                );
        }

        [Test]
        public void Construction()
        {
            Assert.IsNotNull(_PlanService);
        }

        [Test]
        public void SavePlanNew()
        {
            // Arrange
            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            var saveNewPlanCalls = new List<DateTime>();
            _PlanRepositoryMock.Setup(s => s.SaveNewPlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => saveNewPlanCalls.Add(DateTime.Now));

            var setStatusCalls = new List<Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s =>
                    s.SetProcessingStatusForPlanSummary(It.IsAny<int>(), It.IsAny<PlanAggregationProcessingStatusEnum>()))
                .Callback<int, PlanAggregationProcessingStatusEnum>((i, s) => setStatusCalls.Add(new Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>(i, s, DateTime.Now)));

            var saveSummaryCalls = new List<Tuple<int, PlanSummaryDto, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<PlanSummaryDto>()))
                .Callback<PlanSummaryDto>((s) => saveSummaryCalls.Add(new Tuple<int, PlanSummaryDto, DateTime>(Thread.CurrentThread.ManagedThreadId, s, DateTime.Now)));

            var planAggregator = new Mock<IPlanAggregator>();
            var aggregateCallCount = 0;
            var aggregateReturn = new PlanSummaryDto();
            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Callback(() => aggregateCallCount++)
                .Returns(aggregateReturn);

            var plan = _GetNewPlan();
            var campaignId = plan.CampaignId;
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);

            var queuePricingJobCallCount = 0;
            _PlanPricingServiceMock.Setup(s =>
                    s.QueuePricingJob(It.IsAny<PlanPricingParametersDto>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback(() => queuePricingJobCallCount++);

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto { VersionId = 66 });

            var setPricingPlanVersionIdCallCount = 0;
            _PlanRepositoryMock.Setup(s => s.SetPricingPlanVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => setPricingPlanVersionIdCallCount++);

            var updatePlanPricingVersionIdCalls = new List<UpdatePlanPricingVersionIdParams>();
            _PlanRepositoryMock.Setup(s => s.UpdatePlanPricingVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback<int, int>((a, b) => updatePlanPricingVersionIdCalls.Add(new UpdatePlanPricingVersionIdParams { AfterPlanVersionID = a, BeforePlanVersionID = b }));

            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.RUN_PRICING_AUTOMATICALLY] = true;
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_PRICING_IN_EDIT] = true;

            _PlanRepositoryMock.Setup(s => s.GetPlanIdFromPricingJob(It.IsAny<int>()))
                .Returns(plan.Id);

            // Act
            _PlanService.SavePlan(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

            // Assert
            Assert.AreEqual(1, saveNewPlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, setStatusCalls.Count, "Invalid call count.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2, "Invalid 'in process' processing status.");
            Assert.AreEqual(1, aggregateCallCount, "Invalid call count.");
            Assert.AreEqual(1, saveSummaryCalls.Count, "Invalid call count.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, saveSummaryCalls[0].Item2.ProcessingStatus, "Invalid final processing status.");
            var planSavedTime = saveNewPlanCalls[0];
            var setInProgressTime = setStatusCalls[0].Item3;
            var summarySavedTime = saveSummaryCalls[0].Item3;
            Assert.IsTrue(planSavedTime <= setInProgressTime, "Plan should have been saved before aggregation started.");
            Assert.IsTrue(setInProgressTime <= summarySavedTime, "Aggregation started should be set before summary saved");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2);
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, saveSummaryCalls[0].Item2.ProcessingStatus);
            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(campaignId, modifiedWho), Times.Once);
            // Pricing Triggering etc is covered by other tests.
        }

        private PlanPricingParametersDto _GetPricingParameters(int planId, int planVersionId)
        {
            return new PlanPricingParametersDto
            {
                Budget = 100,
                CPM = 12,
                CPP=50,
                DeliveryImpressions = 100,
                DeliveryRatingPoints = 50,
                PlanId = planId,
                PlanVersionId = planVersionId,
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.Per30Min,
                Margin = 0.0
            };
        }

        [Test]
        public void SavePlanUpdatedVersionNoChangeToPlan()
        {
            // Arrange
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            var saveNewPlanCalls = new List<DateTime>();
            _PlanRepositoryMock.Setup(s => s.SaveNewPlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => saveNewPlanCalls.Add(DateTime.Now));

            var savePlanCalls = new List<DateTime>();
            _PlanRepositoryMock.Setup(s => s.SavePlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => savePlanCalls.Add(DateTime.Now));

            var setStatusCalls = new List<Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s =>
                    s.SetProcessingStatusForPlanSummary(It.IsAny<int>(), It.IsAny<PlanAggregationProcessingStatusEnum>()))
                .Callback<int, PlanAggregationProcessingStatusEnum>((i, s) => setStatusCalls.Add(new Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>(i, s, DateTime.Now)));

            var saveSummaryCalls = new List<Tuple<int, PlanSummaryDto, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<PlanSummaryDto>()))
                .Callback<PlanSummaryDto>((s) => saveSummaryCalls.Add(new Tuple<int, PlanSummaryDto, DateTime>(Thread.CurrentThread.ManagedThreadId, s, DateTime.Now)));

            _PlanRepositoryMock.Setup(s => s.GetSuccessfulPricingJobs(It.IsAny<int>()))
                .Returns(new List<PlanPricingJob> {
                new PlanPricingJob { Completed = modifiedWhen } });

            var planAggregator = new Mock<IPlanAggregator>();
            var aggregateCallCount = 0;
            var aggregateReturn = new PlanSummaryDto();
            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Callback(() => aggregateCallCount++)
                .Returns(aggregateReturn);

            var plan = _GetNewPlan();
            var campaignId = plan.CampaignId;
            plan.JobId = 42;
            plan.PricingParameters = _GetPricingParameters(plan.Id, plan.VersionId);
            plan.PricingParameters.JobId = plan.JobId;
            plan.ModifiedDate = modifiedWhen;

            plan.Id = 1;
            plan.VersionId = 526;
            plan.VersionNumber = 1;
            plan.IsDraft = false;

            // mock a before plan
            var beforePlan = _GetNewPlan();
            beforePlan.Id = plan.Id;
            beforePlan.VersionId = plan.VersionId;
            beforePlan.VersionNumber = plan.VersionNumber;
            beforePlan.IsDraft = plan.IsDraft;
            beforePlan.JobId = plan.JobId;
            beforePlan.PricingParameters = _GetPricingParameters(beforePlan.Id, beforePlan.VersionId);
            beforePlan.PricingParameters.JobId = plan.JobId;

            // mock an after plan
            var afterPlan = _GetNewPlan();
            afterPlan.Id = plan.Id;
            afterPlan.VersionId = plan.VersionId + 1;
            afterPlan.VersionNumber = plan.VersionNumber + 1;
            afterPlan.IsDraft = plan.IsDraft;

            var getPlanReturns = new Queue<PlanDto>();
            getPlanReturns.Enqueue(beforePlan);
            getPlanReturns.Enqueue(afterPlan);

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(() => getPlanReturns.Dequeue());

            var queuePricingJobCallCount = 0;
            _PlanPricingServiceMock.Setup(s =>
                    s.QueuePricingJob(It.IsAny<PlanPricingParametersDto>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback(() => queuePricingJobCallCount++);

            var setPricingPlanVersionIdCallCount = 0;
            _PlanRepositoryMock.Setup(s => s.SetPricingPlanVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => setPricingPlanVersionIdCallCount++);

            var updatePlanPricingVersionIdCalls = new List<UpdatePlanPricingVersionIdParams>();
            _PlanRepositoryMock.Setup(s => s.UpdatePlanPricingVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback<int, int>((a, b) => updatePlanPricingVersionIdCalls.Add(new UpdatePlanPricingVersionIdParams { AfterPlanVersionID = a, BeforePlanVersionID = b }));

            _PlanRepositoryMock.Setup(s => s.GetPlanIdFromPricingJob(It.IsAny<int>()))
                .Returns(beforePlan.Id);

            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.RUN_PRICING_AUTOMATICALLY] = true;
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_PRICING_IN_EDIT] = true;

            // handle the test parameter
            var expectedQueuePricingJobCallCount = 0;
            var expectedUpdatePlanPricingVersionIdCalls = 1;
            var expectedSetPricingPlanVersionIdCallCount = 0;
            
            plan.IsOutOfSync = false;

            // Act
            _PlanService.SavePlan(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

            // Assert
            Assert.AreEqual(0, saveNewPlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, savePlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, setStatusCalls.Count, "Invalid call count.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2, "Invalid 'in process' processing status.");
            Assert.AreEqual(1, aggregateCallCount, "Invalid call count.");
            Assert.AreEqual(1, saveSummaryCalls.Count, "Invalid call count.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, saveSummaryCalls[0].Item2.ProcessingStatus, "Invalid final processing status.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2);
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, saveSummaryCalls[0].Item2.ProcessingStatus);
            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(campaignId, modifiedWho), Times.Once);
            Assert.AreEqual(expectedQueuePricingJobCallCount, queuePricingJobCallCount, "Invalid queuePricingJobCallCount Count.");
            Assert.AreEqual(expectedUpdatePlanPricingVersionIdCalls, updatePlanPricingVersionIdCalls.Count, "Invalid updatePlanPricingVersionIdCalls Count.");
            Assert.AreEqual(expectedSetPricingPlanVersionIdCallCount, setPricingPlanVersionIdCallCount, "Invalid setPricingPlanVersionIdCallCount Count.");
        }

        [Test]
        public void SavePlanAndTriggerPricing()
        {
            // Arrange
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            var saveNewPlanCalls = new List<DateTime>();
            _PlanRepositoryMock.Setup(s => s.SaveNewPlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => saveNewPlanCalls.Add(DateTime.Now));

            var savePlanCalls = new List<DateTime>();
            _PlanRepositoryMock.Setup(s => s.SavePlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => savePlanCalls.Add(DateTime.Now));

            var setStatusCalls = new List<Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s =>
                    s.SetProcessingStatusForPlanSummary(It.IsAny<int>(), It.IsAny<PlanAggregationProcessingStatusEnum>()))
                .Callback<int, PlanAggregationProcessingStatusEnum>((i, s) => setStatusCalls.Add(new Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>(i, s, DateTime.Now)));

            var saveSummaryCalls = new List<Tuple<int, PlanSummaryDto, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<PlanSummaryDto>()))
                .Callback<PlanSummaryDto>((s) => saveSummaryCalls.Add(new Tuple<int, PlanSummaryDto, DateTime>(Thread.CurrentThread.ManagedThreadId, s, DateTime.Now)));

            _PlanRepositoryMock.Setup(s => s.GetSuccessfulPricingJobs(It.IsAny<int>()))
                .Returns(new List<PlanPricingJob> {
                new PlanPricingJob { Completed = modifiedWhen } });

            var planAggregator = new Mock<IPlanAggregator>();
            var aggregateCallCount = 0;
            var aggregateReturn = new PlanSummaryDto();
            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Callback(() => aggregateCallCount++)
                .Returns(aggregateReturn);

            var plan = _GetNewPlan();
            var campaignId = plan.CampaignId;            
            plan.Id = 1;
            plan.VersionId = 526;
            plan.VersionNumber = 1;
            plan.IsDraft = false;
            plan.JobId = 42;
            plan.IsOutOfSync = false;
            plan.PricingParameters = _GetPricingParameters(plan.Id, plan.VersionId);
            plan.PricingParameters.JobId = plan.JobId;            

            // mock a before plan
            var beforePlan = _GetNewPlan();
            beforePlan.Id = plan.Id;
            beforePlan.VersionId = plan.VersionId;
            beforePlan.VersionNumber = plan.VersionNumber;
            beforePlan.IsDraft = plan.IsDraft;
            beforePlan.JobId = plan.JobId;
            beforePlan.PricingParameters = _GetPricingParameters(beforePlan.Id, beforePlan.VersionId);
            beforePlan.PricingParameters.JobId = plan.JobId;
            beforePlan.ModifiedDate = modifiedWhen;

            // make a major change to the plan properties
            plan.Budget += 1000;
            plan.IsOutOfSync = true;

            // mock an after plan
            var afterPlan = _GetNewPlan();
            afterPlan.Id = plan.Id;
            afterPlan.VersionId = plan.VersionId + 1;
            afterPlan.VersionNumber = plan.VersionNumber + 1;
            afterPlan.IsDraft = plan.IsDraft;

            // progress our changes
            afterPlan.PricingParameters.Budget = plan.PricingParameters.Budget;
            afterPlan.Budget = plan.Budget;

            var getPlanReturns = new Queue<PlanDto>();
            getPlanReturns.Enqueue(beforePlan);
            getPlanReturns.Enqueue(afterPlan);

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(() => getPlanReturns.Dequeue());

            var queuePricingJobCallCount = 0;
            _PlanPricingServiceMock.Setup(s =>
                    s.QueuePricingJob(It.IsAny<PlanPricingParametersDto>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback(() => queuePricingJobCallCount++);

            var setPricingPlanVersionIdCallCount = 0;
            _PlanRepositoryMock.Setup(s => s.SetPricingPlanVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => setPricingPlanVersionIdCallCount++);

            var updatePlanPricingVersionIdCalls = new List<UpdatePlanPricingVersionIdParams>();
            _PlanRepositoryMock.Setup(s => s.UpdatePlanPricingVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback<int, int>((a, b) => updatePlanPricingVersionIdCalls.Add(new UpdatePlanPricingVersionIdParams { AfterPlanVersionID = a, BeforePlanVersionID = b }));

            _PlanRepositoryMock.Setup(s => s.GetPlanIdFromPricingJob(It.IsAny<int>()))
                .Returns(beforePlan.Id);

            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.RUN_PRICING_AUTOMATICALLY] = true;
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_PRICING_IN_EDIT] = true;

            // handle the test parameter
            var expectedQueuePricingJobCallCount = 1;
            var expectedUpdatePlanPricingVersionIdCalls = 0;
            var expectedSetPricingPlanVersionIdCallCount = 0;

            // Act
            _PlanService.SavePlan(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

            // Assert
            Assert.AreEqual(0, saveNewPlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, savePlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, setStatusCalls.Count, "Invalid call count.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2, "Invalid 'in process' processing status.");
            Assert.AreEqual(1, aggregateCallCount, "Invalid call count.");
            Assert.AreEqual(1, saveSummaryCalls.Count, "Invalid call count.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, saveSummaryCalls[0].Item2.ProcessingStatus, "Invalid final processing status.");
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.InProgress, setStatusCalls[0].Item2);
            Assert.AreEqual(PlanAggregationProcessingStatusEnum.Idle, saveSummaryCalls[0].Item2.ProcessingStatus);
            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(campaignId, modifiedWho), Times.Once);
            Assert.AreEqual(expectedQueuePricingJobCallCount, queuePricingJobCallCount, "Invalid queuePricingJobCallCount Count.");
            Assert.AreEqual(expectedUpdatePlanPricingVersionIdCalls, updatePlanPricingVersionIdCalls.Count, "Invalid updatePlanPricingVersionIdCalls Count.");
            Assert.AreEqual(expectedSetPricingPlanVersionIdCallCount, setPricingPlanVersionIdCallCount, "Invalid setPricingPlanVersionIdCallCount Count.");
        }

        [Test]
        public void HandleBuyingOnPlanSaveCreateNew()
        {
            PlanDto beforePlan = null;
            var savingPlan = _GetNewPlan();
            savingPlan.BuyingParameters = new PlanBuyingParametersDto { JobId = 12 };
            var afterPlan = _GetNewPlan();

            _PlanService._HandleBuyingOnPlanSave(PlanService.SaveState.CreatingNewPlan, savingPlan, beforePlan, afterPlan);

            _PlanRepositoryMock.Verify(s => s.SetBuyingPlanVersionId(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            _PlanRepositoryMock.Verify(s => s.UpdatePlanBuyingVersionId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void HandleBuyingOnPlanSaveUpdateExisting()
        {
            var beforePlan = _GetNewPlan();
            var savingPlan = _GetNewPlan();
            savingPlan.BuyingParameters = new PlanBuyingParametersDto { JobId = 12};
            var afterPlan = _GetNewPlan();

            _PlanService._HandleBuyingOnPlanSave(PlanService.SaveState.UpdatingExisting, savingPlan, beforePlan, afterPlan);

            _PlanRepositoryMock.Verify(s => s.SetBuyingPlanVersionId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _PlanRepositoryMock.Verify(s => s.UpdatePlanBuyingVersionId(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void HandleBuyingOnPlanSaveWithoutBuyingResults()
        {
            var beforePlan = _GetNewPlan();
            var savingPlan = _GetNewPlan();
            var afterPlan = _GetNewPlan();

            _PlanService._HandleBuyingOnPlanSave(PlanService.SaveState.UpdatingExisting, savingPlan, beforePlan, afterPlan);

            _PlanRepositoryMock.Verify(s => s.SetBuyingPlanVersionId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _PlanRepositoryMock.Verify(s => s.UpdatePlanBuyingVersionId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        [TestCase(0, 1, false, false, PlanService.SaveState.CreatingNewPlan)]
        [TestCase(1, 0, false, false, PlanService.SaveState.CreatingNewPlan)]
        [TestCase(1, 1, false, true, PlanService.SaveState.CreatingNewDraft)]
        [TestCase(1, 1, true, false, PlanService.SaveState.PromotingDraft)]
        [TestCase(1, 1, true, true, PlanService.SaveState.UpdatingExisting)]
        [TestCase(1, 1, false, false, PlanService.SaveState.UpdatingExisting)]
        public void DeriveSaveState(int planId, int planVersionId, bool isDraftBefore, bool isDraftNow, PlanService.SaveState expectedResult)
        {
            var plan = _GetNewPlan();
            plan.Id = planId;
            plan.VersionId = planVersionId;
            plan.IsDraft = isDraftNow;

            var beforePlan = _GetNewPlan();
            beforePlan.Id = planId;
            beforePlan.VersionId = planVersionId;
            beforePlan.IsDraft = isDraftBefore;

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(beforePlan);

            var expectBeforePlanResult = expectedResult != PlanService.SaveState.CreatingNewPlan;

            var result = _PlanService._DeriveSaveState(plan, out PlanDto resultBeforePlan);

            Assert.AreEqual(expectedResult, result);
            Assert.IsTrue((resultBeforePlan != null) == expectBeforePlanResult);
        }

        [Test]
        [TestCase(PlanService.SaveState.CreatingNewPlan, false, false, true, false)]
        [TestCase(PlanService.SaveState.PromotingDraft, false, false, true, false)]
        [TestCase(PlanService.SaveState.UpdatingExisting, false, false, true, false)]
        [TestCase(PlanService.SaveState.CreatingNewDraft, false, false, true, false)]
        [TestCase(PlanService.SaveState.CreatingNewPlan, true, true, true, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, true, true, true, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, true, false, true, true)]
        [TestCase(PlanService.SaveState.PromotingDraft, true, true, true, true)]
        [TestCase(PlanService.SaveState.PromotingDraft, true, false, true, true)]
        [TestCase(PlanService.SaveState.CreatingNewDraft, true, false, true, false)]
        [TestCase(PlanService.SaveState.CreatingNewDraft, true, true, true, true)]
        [TestCase(PlanService.SaveState.CreatingNewDraft, true, false, false, false)]
        public void CanPromotePricingResultsSafely(PlanService.SaveState saveState,  bool hasPricingResults, bool pricingWasRunAfterLastSave,
            bool canRunPricingDuringEdit, bool expectedResult)
        {
            // Arrange
            const int planId = 12;
            const int planVersionId = 14;
            var modifiedDate = new DateTime(2020, 10, 17, 12, 30, 40);
            
            var plan = _GetNewPlan();
            plan.Id = planId;
            plan.VersionId = planVersionId;
            plan.IsDraft = saveState == PlanService.SaveState.CreatingNewDraft;
            
            var beforePlan = _GetNewPlan();
            beforePlan.Id = planId;
            beforePlan.VersionId = planVersionId;
            beforePlan.IsDraft = saveState == PlanService.SaveState.PromotingDraft;
            beforePlan.ModifiedDate = modifiedDate;

            if (hasPricingResults)
            {
                plan.JobId = 12;
                beforePlan.PricingParameters.JobId = 12;
                var pricingJobs = new List<PlanPricingJob> {new PlanPricingJob { Queued = pricingWasRunAfterLastSave ? modifiedDate.AddHours(2) : modifiedDate}};
                _PlanRepositoryMock.Setup(s => s.GetSuccessfulPricingJobs(It.IsAny<int>()))
                    .Returns(pricingJobs);
            }
            else
            {
                plan.JobId = null;
                plan.PricingParameters = null;
                beforePlan.JobId = null;
                beforePlan.PricingParameters = null;
            }

            var expectedResultPricingWasRunAfterLastSave = (saveState == PlanService.SaveState.CreatingNewPlan && hasPricingResults)
                || pricingWasRunAfterLastSave;

            // Act
            var result = _PlanService._CanPromotePricingResultsSafely(saveState, plan, beforePlan, canRunPricingDuringEdit, out bool resultPricingWasRunAfterLastSave);

            // Assert
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedResultPricingWasRunAfterLastSave, resultPricingWasRunAfterLastSave);
        }

        [Test]
        [TestCase(PlanService.SaveState.CreatingNewPlan, true, true, true, true)]
        [TestCase(PlanService.SaveState.CreatingNewPlan, false, true, true, false)]
        [TestCase(PlanService.SaveState.UpdatingExisting, false, true, true, false)]
        [TestCase(PlanService.SaveState.UpdatingExisting, true, true, true, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, true, false, true, false)]
        public void ShouldPromotePricingResultsOnPlanSave(PlanService.SaveState saveState, 
            bool couldPromotePricingResultsSafely, 
            bool pricingWasRunAfterLastSave, bool goalsChanged, bool expectedResult)
        {
            // Arrange
            const int planId = 12;
            const int planVersionId = 14;
            var modifiedDate = new DateTime(2020, 10, 17, 12, 30, 40);

            var beforePlan = _GetNewPlan();
            beforePlan.Id = planId;
            beforePlan.VersionId = planVersionId;
            beforePlan.IsDraft = saveState == PlanService.SaveState.PromotingDraft;
            beforePlan.ModifiedDate = modifiedDate.AddHours(-2);

            var afterPlan = _GetNewPlan();
            afterPlan.Id = planId;
            afterPlan.VersionId = planVersionId + 1;
            afterPlan.IsDraft = saveState == PlanService.SaveState.PromotingDraft;
            afterPlan.ModifiedDate = modifiedDate;

            if (goalsChanged)
            {
                afterPlan.Budget++;
            }

            // Act
            var result = _PlanService._ShouldPromotePricingResultsOnPlanSave(saveState, beforePlan, afterPlan, couldPromotePricingResultsSafely, pricingWasRunAfterLastSave);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(PlanService.SaveState.CreatingNewPlan, true, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, true, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, false, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, false, false)]
        public void _FinalizePricingOnPlanSave(PlanService.SaveState saveState, bool shouldPromotePricingResults, bool canAutoTriggerPricing)
        {
            // Arrange
            const int planId = 12;
            const int planVersionId = 14;
            var modifiedDate = new DateTime(2020, 10, 17, 12, 30, 40);
            const string modifiedBy = "TestUser";

            var plan = _GetNewPlan();
            plan.Id = planId;
            plan.VersionId = planVersionId;

            var beforePlan = _GetNewPlan();
            beforePlan.Id = planId;
            beforePlan.VersionId = planVersionId;

            var afterPlan = _GetNewPlan();
            afterPlan.Id = planId;
            afterPlan.VersionId = planVersionId + 1;

            var expectedSetPricingPlanVersionIdCalled = 0;
            var expectedUpdatePlanPricingVersionId = 0;
            var expectedQueuePricingJobCalled = 0;
            var expectedSavePlanPricingParametersCalled = 0;

            if (shouldPromotePricingResults)
            {
                plan.JobId = 26;

                if (saveState == PlanService.SaveState.CreatingNewPlan)
                {
                    expectedSetPricingPlanVersionIdCalled++;
                }
                else
                {
                    expectedUpdatePlanPricingVersionId++;
                }
            }
            else if(canAutoTriggerPricing)
            {
                expectedQueuePricingJobCalled++;
            }
            else
            {
                expectedSavePlanPricingParametersCalled++;
            }

            var setPricingPlanVersionIdCalled = 0;
            _PlanRepositoryMock.Setup(s => s.SetPricingPlanVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => setPricingPlanVersionIdCalled++);

            var updatePlanPricingVersionIdCalled = 0;
            _PlanRepositoryMock.Setup(s => s.UpdatePlanPricingVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => updatePlanPricingVersionIdCalled++);

            var queuePricingJobCalled= 0;
            _PlanPricingServiceMock.Setup(s => s.QueuePricingJob(It.IsAny<PlanPricingParametersDto>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback(() => queuePricingJobCalled++);

            var savePlanPricingParametersCallsed = 0;
            _PlanRepositoryMock.Setup(s => s.SavePlanPricingParameters(It.IsAny<PlanPricingParametersDto>()))
                .Callback(() => savePlanPricingParametersCallsed++);

            // Act
            _PlanService._FinalizePricingOnPlanSave(saveState, plan, beforePlan, afterPlan, modifiedDate, modifiedBy, shouldPromotePricingResults, canAutoTriggerPricing);

            // Assert
            Assert.AreEqual(expectedSetPricingPlanVersionIdCalled, setPricingPlanVersionIdCalled);
            Assert.AreEqual(expectedUpdatePlanPricingVersionId, updatePlanPricingVersionIdCalled);
            Assert.AreEqual(expectedQueuePricingJobCalled, queuePricingJobCalled);
            Assert.AreEqual(expectedSavePlanPricingParametersCalled, savePlanPricingParametersCallsed);
        }

        private class UpdatePlanPricingVersionIdParams
        {
            public int AfterPlanVersionID { get; set; }
            public int BeforePlanVersionID { get; set; }
        }

        [Test]
        public void DispatchAggregation_WasTriggeredOnSave()
        {
            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            var saveNewPlanCalls = new List<DateTime>();
            _PlanRepositoryMock.Setup(s => s.SaveNewPlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => saveNewPlanCalls.Add(DateTime.Now));

            var setStatusCalls = new List<Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s =>
                    s.SetProcessingStatusForPlanSummary(It.IsAny<int>(), It.IsAny<PlanAggregationProcessingStatusEnum>()))
                .Callback<int, PlanAggregationProcessingStatusEnum>((i, s) => setStatusCalls.Add(new Tuple<int, PlanAggregationProcessingStatusEnum, DateTime>(i, s, DateTime.Now)));

            var saveSummaryCalls = new List<Tuple<int, PlanSummaryDto, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<PlanSummaryDto>()))
                .Callback<PlanSummaryDto>((s) => saveSummaryCalls.Add(new Tuple<int, PlanSummaryDto, DateTime>(Thread.CurrentThread.ManagedThreadId, s, DateTime.Now)));

            var planAggregator = new Mock<IPlanAggregator>();
            var aggregateCallCount = 0;
            var aggregateReturn = new PlanSummaryDto();
            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Callback(() => aggregateCallCount++)
                .Returns(aggregateReturn);

            var plan = _GetNewPlan();
            var campaignId = plan.CampaignId;
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            
            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto { VersionId = 66 });

            // aggregatePlanSynchronously = false because we are testing that aggregation is on a separate thread.
            _PlanService.SavePlan(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously:false);
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
            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(campaignId, modifiedWho), Times.Once);
        }

        [Test]
        public void PlanStatusTransitionTriggersPlanSaveAndCampaignAggregation()
        {
            // Arrange
            var savePlanCalls = new List<DateTime>();
            _PlanRepositoryMock.Setup(s => s.SavePlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => savePlanCalls.Add(DateTime.Now));

            var plansToReturn = new List<PlanDto> { _GetNewPlan(), _GetNewPlan() };
            plansToReturn[0].Id = 1;
            plansToReturn[1].Id = 2;
            plansToReturn[0].CampaignId = plansToReturn[1].CampaignId = 1;
            plansToReturn[0].Status = PlanStatusEnum.Contracted;
            plansToReturn[1].Status = PlanStatusEnum.Live;
            _PlanRepositoryMock
                .Setup(s => s.GetPlansForAutomaticTransition(It.IsAny<DateTime>()))
                .Returns(plansToReturn);

            var saveSummaryCalls = new List<Tuple<int, PlanSummaryDto, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<PlanSummaryDto>()))
                .Callback<PlanSummaryDto>((s) => saveSummaryCalls.Add(new Tuple<int, PlanSummaryDto, DateTime>(Thread.CurrentThread.ManagedThreadId, s, DateTime.Now)));

            var planAggregator = new Mock<IPlanAggregator>();
            var aggregateCallCount = 0;
            var aggregateReturn = new PlanSummaryDto();
            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Callback(() =>
                {
                    aggregateCallCount++;
                    throw new Exception("Test exception thrown during aggregation.");
                })
                .Returns(aggregateReturn);

            // Act
            _PlanService.AutomaticStatusTransitionsJobEntryPoint();
            Thread.Sleep(200);

            // Assert
            Assert.AreEqual(2, savePlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(2, aggregateCallCount, "Invalid call count.");
            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(1, "automated status update"), Times.Exactly(2));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanGetPlan()
        {
            // Arrange
            var planToReturn = _GetNewPlan();

            _InventoryProprietarySummaryRepositoryMock
                .Setup(x => x.GetInventorySummaryDataById(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetInventorySummaryProprietaryData());

            _PlanRepositoryMock
                .Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int planId, int versionId) =>
                {
                    planToReturn.Id = planId;
                    planToReturn.VersionId = versionId;
                    return planToReturn;
                });
            _SpotLengthEngineMock
                .Setup(a => a.GetSpotLengths())
                .Returns(new Dictionary<int, int> { { 30, 1 } });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(
                    It.IsAny<IEnumerable<WeeklyBreakdownWeek>>(),
                    It.IsAny<double>(),
                    It.IsAny<List<CreativeLength>>(),
                    It.IsAny<bool>()))
                .Returns(new List<WeeklyBreakdownByWeek>());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(It.IsAny<PlanDto>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            // Act
            var result = _PlanService.GetPlan(1, 1);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanGetPlan_v2()
        {
            // Arrange
            var planToReturn = _GetNewPlan();

            _InventoryProprietarySummaryRepositoryMock
                .Setup(x => x.GetInventorySummaryDataById(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetInventorySummaryProprietaryData());

            _PlanRepositoryMock
                .Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int planId, int versionId) =>
                {
                    planToReturn.Id = planId;
                    planToReturn.VersionId = versionId;
                    planToReturn.PricingParameters.PostingType = PostingTypeEnum.NSI;
                    return planToReturn;
                });

            _PlanRepositoryMock
                .Setup(s => s.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()))
                .Returns(.85d);

            _SpotLengthEngineMock
                .Setup(a => a.GetSpotLengths())
                .Returns(new Dictionary<int, int> { { 30, 1 } });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(
                    It.IsAny<IEnumerable<WeeklyBreakdownWeek>>(),
                    It.IsAny<double>(),
                    It.IsAny<List<CreativeLength>>(),
                    It.IsAny<bool>()))
                .Returns(new List<WeeklyBreakdownByWeek>());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(It.IsAny<PlanDto>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            // Act
            var result = _PlanService.GetPlan_v2(1, 1);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void PlanStatusTransitionFailsOnLockedPlans()
        {
            // Arrange
            var planToReturn = _GetNewPlan();
            planToReturn.Id = 1;
            planToReturn.CampaignId = 1;
            planToReturn.Status = PlanStatusEnum.Live;
            _PlanRepositoryMock
                .Setup(s => s.GetPlansForAutomaticTransition(It.IsAny<DateTime>()))
                .Returns(new List<PlanDto> { planToReturn });

            const string expectedMessage = "The chosen plan has been locked by IntegrationUser";
            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.LockObject(It.IsAny<string>())).Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = "IntegrationUser"
                });

            // Act
            var exception = Assert.Throws<Exception>(() => _PlanService.AutomaticStatusTransitionsJobEntryPoint());

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void DispatchAggregation_WithAggregationError()
        {
            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            var saveNewPlanCalls = new List<DateTime>();
            _PlanRepositoryMock.Setup(s => s.SaveNewPlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => saveNewPlanCalls.Add(DateTime.Now));

            var setStatusCalls = new List<Tuple<int, PlanAggregationProcessingStatusEnum, int, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s =>
                    s.SetProcessingStatusForPlanSummary(It.IsAny<int>(), It.IsAny<PlanAggregationProcessingStatusEnum>()))
                .Callback<int, PlanAggregationProcessingStatusEnum>((i, s) => setStatusCalls.Add(new Tuple<int, PlanAggregationProcessingStatusEnum, int, DateTime>(i, s, Thread.CurrentThread.ManagedThreadId, DateTime.Now)));

            var saveSummaryCalls = new List<Tuple<int, PlanSummaryDto, DateTime>>();
            _PlanSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<PlanSummaryDto>()))
                .Callback<PlanSummaryDto>((s) => saveSummaryCalls.Add(new Tuple<int, PlanSummaryDto, DateTime>(Thread.CurrentThread.ManagedThreadId, s, DateTime.Now)));

            var planAggregator = new Mock<IPlanAggregator>();
            var aggregateCallCount = 0;
            var aggregateReturn = new PlanSummaryDto();
            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Callback(() =>
                {
                    aggregateCallCount++;
                    throw new Exception("Test exception thrown during aggregation.");
                })
                .Returns(aggregateReturn);

            var plan = _GetNewPlan();
            var campaignId = plan.CampaignId;
            var modifiedWho = "IntegrationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto { VersionId = 66 });

            // aggregatePlanSynchronously = false because we are testing that aggregation is on a separate thread.
            _PlanService.SavePlan(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: false);
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
            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(campaignId, modifiedWho), Times.Once);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLockPlan()
        {
            // Arrange
            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.LockObject(It.IsAny<string>()))
                .Returns((string key) => new LockResponse
                {
                    Success = true,
                    Key = key,
                    LockedUserName = "IntegrationUser",
                    LockedUserId = "IntegrationUserId",
                    LockTimeoutInSeconds = 900
                });

            _PlanRepositoryMock
                .Setup(s => s.GetPlanNameById(It.IsAny<int>()))
                .Returns("Test Plan");

            // Act
            var lockingResponse = _PlanService.LockPlan(1);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(lockingResponse));
        }

        [Test]
        public void CanNotUpdateLockedPlan()
        {
            const string expectedMessage = "The chosen plan has been locked by IntegrationUser";
            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = "IntegrationUser"
                });

            var beforePlan = _GetNewPlan();
            beforePlan.Id = 1;
            beforePlan.VersionId = 1;

            PlanDto plan = _GetNewPlan();
            plan.Id = 1;
            plan.VersionId = 2;

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(beforePlan);

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            var exception = Assert.Throws<Exception>(() => _PlanService.SavePlan(plan, "IntegrationUser", new DateTime(2019, 10, 23), aggregatePlanSynchronously:true));

            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void CanSavePlanWithFlightDaysDefaultingToSevedDays()
        {
            // Arrange
            var plan = _GetNewPlan();
            plan.FlightDays = null;
            _DayRepositoryMock
                .Setup(s => s.GetDays())
                .Returns(new List<Day>
                {
                    new Day { Id = 1 },
                    new Day { Id = 2 },
                    new Day { Id = 3 },
                    new Day { Id = 4 },
                    new Day { Id = 5 },
                    new Day { Id = 6 },
                    new Day { Id = 7 },
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Returns(new PlanSummaryDto());
            _PlanSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<PlanSummaryDto>()));

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto{VersionId = 66});

            // Act
            _PlanService.SavePlan(plan, "IntegrationUser", new DateTime(2019, 10, 23), aggregatePlanSynchronously:true);

            // Assert
            Assert.NotNull(plan.FlightDays, "FlightDays list should be initialized");
            Assert.AreEqual(plan.FlightDays, new List<int> { 1, 2, 3, 4, 5, 6, 7 });
        }

        [Test]
        public void CanNotUpdatePlanWithPricingModelRunning()
        {
            const string expectedMessage = "The pricing model is running for the plan";
            _PlanPricingServiceMock.Setup(x => x.IsPricingModelRunningForPlan(It.IsAny<int>())).Returns(true);

            PlanDto plan = _GetNewPlan();
            plan.Id = 1;
            plan.VersionId = 1;

            var exception = Assert.Throws<Exception>(() => _PlanService.SavePlan(plan, "IntegrationUser", new DateTime(2019, 10, 23)));

            Assert.AreEqual(expectedMessage, exception.Message);
        }

        private static List<WeeklyBreakdownWeek> _GetWeeklyBreakdownWeeks()
        {
            List<WeeklyBreakdownWeek> result = new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 600000,
                    DaypartCodeId = 2,
                    PercentageOfWeek = 30,
                    SpotLengthId = 1,
                    WeeklyBudget = 30,
                    WeeklyImpressions = 30000,
                    WeeklyImpressionsPercentage = 30,
                    WeeklyRatings = 15
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 400000,
                    DaypartCodeId = 11,
                    PercentageOfWeek = 20,
                    SpotLengthId = 1,
                    WeeklyBudget = 20,
                    WeeklyImpressions = 20000,
                    WeeklyImpressionsPercentage = 20,
                    WeeklyRatings = 10,
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 600000,
                    DaypartCodeId = 2,
                    PercentageOfWeek = 15,
                    SpotLengthId = 2,
                    WeeklyBudget = 15,
                    WeeklyImpressions = 15000,
                    WeeklyImpressionsPercentage = 15,
                    WeeklyRatings = 7.5,
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 400000,
                    DaypartCodeId = 11,
                    PercentageOfWeek = 10,
                    SpotLengthId = 2,
                    WeeklyBudget = 10,
                    WeeklyImpressions = 10000,
                    WeeklyImpressionsPercentage = 10,
                    WeeklyRatings = 5,
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 600000,
                    DaypartCodeId = 2,
                    PercentageOfWeek = 15,
                    SpotLengthId = 3,
                    WeeklyBudget = 15,
                    WeeklyImpressions = 15000,
                    WeeklyImpressionsPercentage = 15,
                    WeeklyRatings = 7.5,
                },
                new WeeklyBreakdownWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    AduImpressions = 400000,
                    DaypartCodeId = 11,
                    PercentageOfWeek = 10,
                    SpotLengthId = 3,
                    WeeklyBudget = 10,
                    WeeklyImpressions = 10000,
                    WeeklyImpressionsPercentage = 10,
                    WeeklyRatings = 5,
                }
            };
            return result;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanGetPlanStatuses()
        {
            var planStatuses = _PlanService.GetPlanStatuses();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(planStatuses));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanGetPlanDefaults()
        {
            _SpotLengthEngineMock.Setup(a => a.GetSpotLengthIdByValue(It.IsAny<int>())).Returns(1);

            var planDefaults = _PlanService.GetPlanDefaults();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(planDefaults));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanGetPlanCurrencies()
        {
            var planCurrencies = _PlanService.GetPlanCurrencies();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(planCurrencies));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanGetPlanVersions()
        {
            // Arrange
            _PlanRepositoryMock
                .Setup(s => s.GetPlanHistory(It.IsAny<int>()))
                .Returns((int planId) => new List<PlanVersion>
                {
                    new PlanVersion
                    {
                        Budget = (decimal?)105.0000,
                        FlightStartDate = new DateTime(2020, 1, 1),
                        FlightEndDate =  new DateTime(2020, 1, 31),
                        IsDraft = false,
                        ModifiedBy = "unit_test",
                        ModifiedDate = new DateTime(2020, 01, 02),
                        Status = 1,
                        TargetAudienceId = 31,
                        TargetCPM = (decimal?)12.0000,
                        TargetImpressions = 100000.0,
                        HiatusDays = new List<DateTime>(),
                        Dayparts = new List<PlanDaypartDto>
                        {
                            new PlanDaypartDto{ DaypartCodeId = 2, StartTimeSeconds = 0, EndTimeSeconds = 2000, WeightingGoalPercent = 28.0 },
                            new PlanDaypartDto{ DaypartCodeId = 11, StartTimeSeconds = 1500, EndTimeSeconds = 2788, WeightingGoalPercent = 33.2 }
                        },
                        VersionId = 2,
                        VersionNumber = 2,
                    },
                    new PlanVersion
                    {
                        Budget = (decimal?)105.0000,
                        FlightStartDate = new DateTime(2020, 1, 1),
                        FlightEndDate =  new DateTime(2020, 1, 29),
                        IsDraft = true,
                        ModifiedBy = "plan_service_unit_test",
                        ModifiedDate = new DateTime(2020, 01, 03),
                        Status = 2,
                        TargetAudienceId = 31,
                        TargetCPM = (decimal?)15.0000,
                        TargetImpressions = 100500.0,
                        HiatusDays = new List<DateTime> { new DateTime(2020, 1, 15) },
                        Dayparts = new List<PlanDaypartDto>
                        {
                            new PlanDaypartDto{ DaypartCodeId = 2, StartTimeSeconds = 0, EndTimeSeconds = 2000, WeightingGoalPercent = 28.0 },
                        },
                        VersionId = 3,
                        VersionNumber = 3,
                    },
                    new PlanVersion
                    {
                        Budget = (decimal?)100.0000,
                        FlightStartDate = new DateTime(2020, 1, 1),
                        FlightEndDate =  new DateTime(2020, 1, 31),
                        IsDraft = false,
                        ModifiedBy = "integration_test",
                        ModifiedDate = new DateTime(2020, 01, 01),
                        Status = 1,
                        TargetAudienceId = 31,
                        TargetCPM = (decimal?)12.0000,
                        TargetImpressions = 100000.0,
                        HiatusDays = new List<DateTime>(),
                        Dayparts = new List<PlanDaypartDto>
                        {
                            new PlanDaypartDto{ DaypartCodeId = 2, StartTimeSeconds = 0, EndTimeSeconds = 2000, WeightingGoalPercent = 28.0 },
                            new PlanDaypartDto{ DaypartCodeId = 11, StartTimeSeconds = 1500, EndTimeSeconds = 2788, WeightingGoalPercent = 33.2 }
                        },
                        VersionId = 1,
                        VersionNumber = 1,
                    }
                });

            // Act
            var results = _PlanService.GetPlanHistory(1);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanCalculatePlanDeliveryBudget()
        {
            // Arrange
            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Returns((PlanDeliveryBudget budget) => budget);

            var planBudget = new PlanDeliveryBudget
            {
                AudienceId = 31,
                CPM = 100,
                Budget = 10000,
                Impressions = 1000000
            };

            // Act
            var results = _PlanService.Calculate(planBudget);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateCreativeLengthWeight_DefaultValueSet()
        {
            var request = new List<CreativeLength>
            {
                new CreativeLength{ SpotLengthId = 1, Weight = 100},
                new CreativeLength{ SpotLengthId = 2},
                new CreativeLength{ SpotLengthId = 3}
            };

            _CreativeLengthEngineMock.Setup(x => x.DistributeWeight(request))
                .Returns(new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 34 } });

            // Act
            var results = _PlanService.CalculateCreativeLengthWeight(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateCreativeLengthWeight_UserInput1()
        {
            var request = new List<CreativeLength>
            {
                new CreativeLength{ SpotLengthId = 1, Weight = 25},
                new CreativeLength{ SpotLengthId = 2},
                new CreativeLength{ SpotLengthId = 3}
            };

            _CreativeLengthEngineMock.Setup(x => x.DistributeWeight(request))
                .Returns(new List<CreativeLength>
                        {
                            new CreativeLength{ SpotLengthId = 1, Weight = 25},
                            new CreativeLength{ SpotLengthId = 2, Weight = 38},
                            new CreativeLength{ SpotLengthId = 3, Weight = 37}
                        });

            // Act
            var results = _PlanService.CalculateCreativeLengthWeight(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        public void CalculateCreativeLengthWeight_AllValuesSet()
        {
            var request = new List<CreativeLength>
            {
                new CreativeLength{ SpotLengthId = 1, Weight = 60},
                new CreativeLength{ SpotLengthId = 2, Weight = 30},
                new CreativeLength{ SpotLengthId = 3, Weight = 10}
            };

            // Act
            var result = _PlanService.CalculateCreativeLengthWeight(request);

            // Assert
            Assert.AreEqual(null, result);
        }

        private static PlanDto _GetNewPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019,1,20),
                    new DateTime(2019,4,15)
                },
                AudienceId = 31,        //HH
                AudienceType = AudienceTypeEnum.Nielsen,
                HUTBookId = 436,
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                TargetCPM = 12m,
                TargetImpressions = 100d,
                CoverageGoalPercent = 80.5,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 20, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn"},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York"}
                },
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto {MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUS = 5.5, Rank = 5, Market = "Burlington-Plattsburgh" },
                    new PlanBlackoutMarketDto {MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUS = 2.5, Rank = 8, Market = "Amarillo" },
                },
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 2,
                        StartTimeSeconds = 0,
                        EndTimeSeconds = 2000,
                        WeightingGoalPercent = 28.0,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.5,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 11,
                        StartTimeSeconds = 1500,
                        EndTimeSeconds = 2788,
                        WeightingGoalPercent = 33.2,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.5,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        }
                    }
                },
                Vpvh = 0.234543,
                TargetRatingPoints = 50,
                TargetCPP = 50,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                ImpressionsPerUnit = 20,
                PricingParameters = new PlanPricingParametersDto
                {
                    AdjustedBudget = 80m,
                    AdjustedCPM = 10m,
                    CPM = 12m,
                    Budget = 100m,
                    Currency = PlanCurrenciesEnum.Impressions,
                    DeliveryImpressions = 100d,
                    InflationFactor = 10,
                    JobId = 1,
                    PlanId = 1,
                    PlanVersionId = 1,
                    ProprietaryInventory = new List<InventoryProprietarySummary>
                    {
                        new InventoryProprietarySummary
                        {
                            Id = 1
                        }
                    }
                }
            };
        }

        private static List<StandardDaypartFullDto> _GetDaypartCodeDefaultsFull()
        {
            return new List<StandardDaypartFullDto>
            {
                new StandardDaypartFullDto { Id = 2, DaypartType = DaypartTypeEnum.News, DefaultEndTimeSeconds = 0, DefaultStartTimeSeconds = 2000 },
                new StandardDaypartFullDto { Id = 11, DaypartType = DaypartTypeEnum.News, DefaultEndTimeSeconds = 5000, DefaultStartTimeSeconds = 6000 },
            };
        }

        private static PlanDeliveryBudget _GetPlanDeliveryBudget()
        {
            return new PlanDeliveryBudget
            {
                AudienceId = 31,
                Budget = 100.0M,
                CPM = 0.0333333333333333333333333333M,
                CPP = 37381.32000000000388347057216M,
                Impressions = 3000.0,
                RatingPoints = 0.0026751329273551603,
                Universe = 112143960.0
            };
        }

        private static PlanPricingDefaults _GetPlanPricingDefaults()
        {
            return new PlanPricingDefaults
            {
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.Per30Min
            };
        }

        private static PlanBuyingDefaults GetPlanBuyingDefaults()
        {
            return new PlanBuyingDefaults
            {
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.Per30Min
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateLengthMakeUpTable()
        {
            var request = new LengthMakeUpRequest
            {
                CreativeLengths = new List<CreativeLength>{
                    new CreativeLength{ SpotLengthId = 1, Weight = 50},
                    new CreativeLength{ SpotLengthId = 2, Weight = 20},
                    new CreativeLength{ SpotLengthId = 3, Weight = 30}
                },
                TotalImpressions = 100000,
                Weeks = _GetWeeklyBreakdownWeeks()
            };

            var distributeWeightCallCount = 0;
            _CreativeLengthEngineMock.Setup(s => s.DistributeWeight(It.IsAny<List<CreativeLength>>()))
                .Callback(() => distributeWeightCallCount++)
                .Returns(request.CreativeLengths);
            _SpotLengthEngineMock
                .Setup(a => a.GetSpotLengths())
                .Returns(new Dictionary<int, int> { { 30, 1 }, { 15, 2 }, { 45, 3 } });

            // Act
            var results = _PlanService.CalculateLengthMakeUpTable(request);

            // Assert
            Assert.AreEqual(1, distributeWeightCallCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateLengthMakeUpTable_WeightNotSet()
        {
            var request = new LengthMakeUpRequest
            {
                CreativeLengths = new List<CreativeLength>{
                    new CreativeLength{ SpotLengthId = 1, Weight = 50},
                    new CreativeLength{ SpotLengthId = 2, Weight = 20},
                    new CreativeLength{ SpotLengthId = 3}
                },
                TotalImpressions = 100000,
                Weeks = _GetWeeklyBreakdownWeeks()
            };

            _CreativeLengthEngineMock
                .Setup(x => x.DistributeWeight(It.IsAny<IEnumerable<CreativeLength>>()))
                .Returns(new List<CreativeLength>
                {
                    new CreativeLength { SpotLengthId = 1, Weight = 50 },
                    new CreativeLength { SpotLengthId = 2, Weight = 20 },
                    new CreativeLength { SpotLengthId = 3, Weight = 30 }
                });
            _SpotLengthEngineMock
                .Setup(a => a.GetSpotLengths())
                .Returns(new Dictionary<int, int> { { 30, 1 }, { 15, 2 }, { 45, 3 } });

            // Act
            var results = _PlanService.CalculateLengthMakeUpTable(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        private static int _CalculateADU(double impressionsPerUnit, double aduImpressions
            , bool equivalized, int? spotLengthId, List<CreativeLength> creativeLengths = null)
        {
            if (equivalized)
            {
                if (spotLengthId.HasValue)
                {
                    return (int)_CalculateUnitsForSingleSpotLength(impressionsPerUnit, aduImpressions, spotLengthId.Value);
                }
                else
                {
                    return (int)_CalculateUnitsForMultipleSpotLengths(creativeLengths, impressionsPerUnit, aduImpressions);
                }
            }
            else
            {
                return (int)(aduImpressions / impressionsPerUnit);
            }
        }

        private static double _CalculateUnitsForMultipleSpotLengths(List<CreativeLength> creativeLengths, double impressionsPerUnit, double impressions)
           => creativeLengths
                  .Sum(p => impressions * GeneralMath.ConvertPercentageToFraction(p.Weight.Value)
                  / _CalculateEquivalizedImpressionsPerUnit(impressionsPerUnit, p.SpotLengthId));

        private static double _CalculateUnitsForSingleSpotLength(double impressionsPerUnit, double impressions, int spotLengthId)
            => impressions / _CalculateEquivalizedImpressionsPerUnit(impressionsPerUnit, spotLengthId);

        private static double _CalculateEquivalizedImpressionsPerUnit(double impressionPerUnit, int spotLengthId)
            => impressionPerUnit * _SpotLengthMultiplier[spotLengthId];

        private static Dictionary<int, double> _SpotLengthMultiplier = new Dictionary<int, double>
        {
            { 1,1}, { 2, 2}, { 3, 0.5}
        };

        private static Dictionary<int, InventoryProprietarySummary> _GetInventorySummaryProprietaryData()
        {
            return new Dictionary<int, InventoryProprietarySummary> {
                    { 1, 
                        new InventoryProprietarySummary
                        {
                            Id = 1,
                            DaypartName = "Daypart name",
                            InventorySourceName = "CNN",
                            UnitType = "PM"
                        }
                    }
                };
        }

    }
}