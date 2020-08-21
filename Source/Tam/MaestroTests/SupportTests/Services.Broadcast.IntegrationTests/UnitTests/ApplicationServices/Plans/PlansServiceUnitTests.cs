using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
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
        private Mock<IDaypartDefaultService> _DaypartDefaultServiceMock;
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
            _DaypartDefaultServiceMock = new Mock<IDaypartDefaultService>();
            _WeeklyBreakdownEngineMock = new Mock<IWeeklyBreakdownEngine>();
            _CreativeLengthEngineMock = new Mock<ICreativeLengthEngine>();
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

            var daypartCodeRepository = new Mock<IDaypartDefaultRepository>();
            daypartCodeRepository
                .Setup(s => s.GetAllDaypartDefaultsWithAllData())
                .Returns(_GetDaypartCodeDefaultsFull());
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IDaypartDefaultRepository>())
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

            _DaypartDefaultServiceMock
                .Setup(s => s.GetAllDaypartDefaults())
                .Returns(new List<DaypartDefaultDto>());

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
                    _DaypartDefaultServiceMock.Object,
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
        public void DispatchAggregation_WasTriggeredOnSave()
        {
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

            _PlanService.SavePlan(plan, modifiedWho, modifiedWhen);
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

            _PlanService.SavePlan(plan, modifiedWho, modifiedWhen);
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

            PlanDto plan = _GetNewPlan();
            plan.Id = 1;
            plan.VersionId = 1;

            var exception = Assert.Throws<Exception>(() => _PlanService.SavePlan(plan, "IntegrationUser", new DateTime(2019, 10, 23)));

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

            // Act
            _PlanService.SavePlan(plan, "IntegrationUser", new DateTime(2019, 10, 23));

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekDeliveryType_OnPlanSave()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeek;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.IsAduEnabled = true;
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2 });
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 3 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 100,
                WeeklyImpressionsPercentage = 100,
                WeeklyRatings = 50,
                WeeklyBudget = 100,
                WeeklyAdu = 6,
                WeeklyUnits = 5
            });

            _CreativeLengthEngineMock
                .Setup(x => x.DistributeWeight(It.IsAny<IEnumerable<CreativeLength>>()))
                .Returns(new List<CreativeLength>
                {
                    new CreativeLength { SpotLengthId = 1, Weight = 50 },
                    new CreativeLength { SpotLengthId = 2, Weight = 25 },
                    new CreativeLength { SpotLengthId = 3, Weight = 25 }
                });
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetWeeklyBreakdownCombinations(It.IsAny<List<CreativeLength>>(), It.IsAny<List<PlanDaypartDto>>()))
                .Returns(new List<WeeklyBreakdownCombination> {
                    new WeeklyBreakdownCombination{ SpotLengthId = 1, DaypartCodeId = 2, Weighting = 0.3},
                    new WeeklyBreakdownCombination{ SpotLengthId = 1, DaypartCodeId = 11, Weighting = 0.2},
                    new WeeklyBreakdownCombination{ SpotLengthId = 2, DaypartCodeId = 2, Weighting = 0.15},
                    new WeeklyBreakdownCombination{ SpotLengthId = 2, DaypartCodeId = 11, Weighting = 0.1},
                    new WeeklyBreakdownCombination{ SpotLengthId = 3, DaypartCodeId = 2, Weighting = 0.15},
                    new WeeklyBreakdownCombination{ SpotLengthId = 3, DaypartCodeId = 11, Weighting = 0.1}
                });

            var savedWeekyBreakdowns = new List<object>();
            _PlanRepositoryMock
                .Setup(x => x.SavePlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<PlanDto, string, DateTime>((p1, p2, p3) => savedWeekyBreakdowns.Add(p1.WeeklyBreakdownWeeks));

            // Act
            _PlanService.SavePlan(plan, "CreatedBy", new DateTime(2020, 1, 1));

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedWeekyBreakdowns));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekByAdLengthDeliveryType_OnPlanSave()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.IsAduEnabled = true;
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2 });
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 3 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 50,
                WeeklyImpressionsPercentage = 50,
                WeeklyRatings = 25,
                WeeklyBudget = 50,
                WeeklyAdu = 2,
                SpotLengthId = 1,
                WeeklyUnits = 2.5
            });
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 25,
                WeeklyImpressionsPercentage = 25,
                WeeklyRatings = 12.5,
                WeeklyBudget = 25,
                WeeklyAdu = 2,
                SpotLengthId = 2,
                WeeklyUnits = 1.25
            });
            plan.WeeklyBreakdownWeeks.Add(new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 401,
                StartDate = new DateTime(2020, 5, 11),
                EndDate = new DateTime(2020, 5, 17),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 25,
                WeeklyImpressionsPercentage = 25,
                WeeklyRatings = 12.5,
                WeeklyBudget = 25,
                WeeklyAdu = 2,
                SpotLengthId = 3,
                WeeklyUnits = 0
            });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<List<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns<List<WeeklyBreakdownWeek>, double, List<CreativeLength>, bool>((p, q, r, s) => _GetWeeklyBreakdownWeeks(p, q, r, s));
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetWeeklyBreakdownCombinations(It.IsAny<List<CreativeLength>>(), It.IsAny<List<PlanDaypartDto>>()))
                .Returns(new List<WeeklyBreakdownCombination> {
                    new WeeklyBreakdownCombination{ SpotLengthId = 1, DaypartCodeId = 2, Weighting = 0.3},
                    new WeeklyBreakdownCombination{ SpotLengthId = 1, DaypartCodeId = 11, Weighting = 0.2},
                    new WeeklyBreakdownCombination{ SpotLengthId = 2, DaypartCodeId = 2, Weighting = 0.15},
                    new WeeklyBreakdownCombination{ SpotLengthId = 2, DaypartCodeId = 11, Weighting = 0.1},
                    new WeeklyBreakdownCombination{ SpotLengthId = 3, DaypartCodeId = 2, Weighting = 0.15},
                    new WeeklyBreakdownCombination{ SpotLengthId = 3, DaypartCodeId = 11, Weighting = 0.1}
                });
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetStandardDaypardWeightingGoals(It.IsAny<List<PlanDaypartDto>>()))
                .Returns(new List<DaypartDefaultWeightingGoal> {
                    new DaypartDefaultWeightingGoal(2, 60), new DaypartDefaultWeightingGoal(11, 40)
                });

            _CreativeLengthEngineMock
                .Setup(x => x.DistributeWeight(It.IsAny<IEnumerable<CreativeLength>>()))
                .Returns(new List<CreativeLength>
                {
                    new CreativeLength { SpotLengthId = 1, Weight = 50 },
                    new CreativeLength { SpotLengthId = 2, Weight = 25 },
                    new CreativeLength { SpotLengthId = 3, Weight = 25 }
                });
            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Returns(new PlanSummaryDto());

            var savedWeekyBreakdowns = new List<object>();
            _PlanRepositoryMock
                .Setup(x => x.SavePlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<PlanDto, string, DateTime>((p1, p2, p3) => savedWeekyBreakdowns.Add(p1.WeeklyBreakdownWeeks));

            // Act
            _PlanService.SavePlan(plan, "CreatedBy", new DateTime(2020, 1, 1));

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedWeekyBreakdowns));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DistributesGoals_ByWeekByDaypartDeliveryType_OnPlanSave()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart;
            plan.Id = 1;
            plan.VersionId = 1;
            plan.FlightStartDate = new DateTime(2020, 2, 24);
            plan.FlightEndDate = new DateTime(2020, 3, 29);
            plan.FlightHiatusDays = new List<DateTime> { };
            plan.FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            plan.CreativeLengths.Add(new CreativeLength { SpotLengthId = 2 });
            plan.Dayparts[0].WeightingGoalPercent = 60;
            plan.Dayparts[1].WeightingGoalPercent = null;
            plan.TargetImpressions = 5000;
            plan.ImpressionsPerUnit = 100;
            plan.TargetRatingPoints = 4.1;
            plan.TargetCPM = 0.1m;
            plan.Budget = 500;
            plan.WeeklyBreakdownTotals = new WeeklyBreakdownTotals
            {
                TotalActiveDays = 35,
                TotalBudget = 500,
                TotalImpressions = 5000,
                TotalImpressionsPercentage = 100,
                TotalRatingPoints = 4.1,
                TotalUnits = 50
            };
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,2,24),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,2,24),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,2),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,2),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,9),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 3
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,9),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 3
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,16),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,16),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 2,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,23),
                       PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 5
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 11,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,23),
                       PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 5
                    },
                };

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<List<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns<List<WeeklyBreakdownWeek>, double, List<CreativeLength>, bool>((p, q, r, s) => _GetWeeklyBreakdownWeeks(p, q, r, s));

            _CreativeLengthEngineMock
                .Setup(x => x.DistributeWeight(It.IsAny<IEnumerable<CreativeLength>>()))
                .Returns(new List<CreativeLength>
                {
                    new CreativeLength { SpotLengthId = 1, Weight = 50 },
                    new CreativeLength { SpotLengthId = 2, Weight = 50 },
                });
            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Returns(new PlanSummaryDto());

            var savedWeekyBreakdowns = new List<object>();
            _PlanRepositoryMock
                .Setup(x => x.SavePlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<PlanDto, string, DateTime>((p1, p2, p3) => savedWeekyBreakdowns.Add(p1.WeeklyBreakdownWeeks));

            // Act
            _PlanService.SavePlan(plan, "CreatedBy", new DateTime(2020, 1, 1));

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedWeekyBreakdowns));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownWeeks_ByWeekByAdLengthDeliveryType()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength;
            plan.TargetImpressions *= 1000;
            plan.WeeklyBreakdownWeeks.AddRange(_GetWeeklyBreakdownWeeks());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<List<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns<List<WeeklyBreakdownWeek>, double, List<CreativeLength>, bool>((p, q, r, s) => _GetWeeklyBreakdownWeeks(p, q, r, s));

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(It.IsAny<PlanDto>()))
                .Returns(_GetWeeklyBreakdownWeeks().Take(2).ToList());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetWeekNumberByMediaWeekDictionary(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new Dictionary<int, int>
                {
                    { 401, 1 }
                });

            _SpotLengthEngineMock
                .Setup(a => a.GetSpotLengths())
                .Returns(new Dictionary<int, int> { { 30, 1 } });

            // Act
            var result = _PlanService.GetPlan(planId: 1);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.WeeklyBreakdownWeeks));
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
                ImpressionsPerUnit = 20
            };
        }

        private static List<DaypartDefaultFullDto> _GetDaypartCodeDefaultsFull()
        {
            return new List<DaypartDefaultFullDto>
            {
                new DaypartDefaultFullDto { Id = 2, DaypartType = DaypartTypeEnum.News, DefaultEndTimeSeconds = 0, DefaultStartTimeSeconds = 2000 },
                new DaypartDefaultFullDto { Id = 11, DaypartType = DaypartTypeEnum.News, DefaultEndTimeSeconds = 5000, DefaultStartTimeSeconds = 6000 },
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
                UnitCapType = UnitCapEnum.Per30Min,
                InventorySourcePercentages = new List<PlanInventorySourceDto>(),
                InventorySourceTypePercentages = new List<PlanInventorySourceTypeDto>()
            };
        }

        private static PlanBuyingDefaults GetPlanBuyingDefaults()
        {
            return new PlanBuyingDefaults
            {
                UnitCaps = 1,
                UnitCapType = UnitCapEnum.Per30Min,
                InventorySourcePercentages = new List<PlanInventorySourceDto>(),
                InventorySourceTypePercentages = new List<PlanInventorySourceTypeDto>()
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

        private static List<WeeklyBreakdownByWeek> _GetWeeklyBreakdownWeeks(List<WeeklyBreakdownWeek> weeks
               , double impressionsPerUnit = 0, List<CreativeLength> creativeLengths = null, bool equivalized = false)
        {
            return weeks.GroupBy(x => x.MediaWeekId)
                    .Select(x =>
                    {
                        var week = new WeeklyBreakdownByWeek
                        {
                            WeekNumber = x.First().WeekNumber,
                            MediaWeekId = x.First().MediaWeekId,
                            StartDate = x.First().StartDate,
                            EndDate = x.First().EndDate,
                            NumberOfActiveDays = x.First().NumberOfActiveDays,
                            ActiveDays = x.First().ActiveDays,
                            Impressions = x.Sum(i => i.WeeklyImpressions),
                            Budget = x.Sum(i => i.WeeklyBudget),
                            Units = x.Sum(i => i.UnitImpressions) / x.Sum(i => i.WeeklyImpressions)
                        };
                        if (!creativeLengths.IsNullOrEmpty())
                        {
                            week.Adu = _CalculateADU(impressionsPerUnit, x.Sum(y => y.AduImpressions), equivalized, null, creativeLengths);
                        }
                        return week;
                    })
                    .ToList();
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
    }
}