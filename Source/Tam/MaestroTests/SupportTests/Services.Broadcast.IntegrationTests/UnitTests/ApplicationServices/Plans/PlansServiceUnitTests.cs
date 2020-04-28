using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Extensions;
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
        private Mock<INsiUniverseService> _NsiUniverseServiceMock;
        private Mock<IBroadcastAudiencesCache> _BroadcastAudiencesCacheMock;
        private Mock<ISpotLengthEngine> _SpotLengthEngineMock;
        private Mock<IBroadcastLockingManagerApplicationService> _BroadcastLockingManagerApplicationServiceMock;
        private Mock<IPlanPricingService> _PlanPricingServiceMock;
        private Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock;
        private Mock<IDaypartDefaultService> _DaypartDefaultServiceMock;

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
            _NsiUniverseServiceMock = new Mock<INsiUniverseService>();
            _BroadcastAudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
            _BroadcastLockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
            _PlanPricingServiceMock = new Mock<IPlanPricingService>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            _DaypartDefaultServiceMock = new Mock<IDaypartDefaultService>();

            // Setup common mocks
            _NsiUniverseServiceMock.Setup(n => n.GetAudienceUniverseForMediaMonth(It.IsAny<int>(), It.IsAny<int>())).Returns(1000000);
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

            var broadcastAudienceCacheMock = new Mock<IBroadcastAudiencesCache>();
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDefaultAudience()).Returns(new BroadcastAudience());

            _PlanPricingServiceMock
                .Setup(s => s.GetPlanPricingDefaults())
                .Returns(_GetPlanPricingDefaults());

            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Returns(_GetPlanDeliveryBudget());

            _DaypartDefaultServiceMock
                .Setup(s => s.GetAllDaypartDefaults())
                .Returns(new List<DaypartDefaultDto>());

            _PlanService = new PlanService(
                _DataRepositoryFactoryMock.Object,
                _PlanValidatorMock.Object,
                _PlanBudgetDeliveryCalculatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _PlanAggregatorMock.Object,
                _CampaignAggregationJobTriggerMock.Object,
                _NsiUniverseServiceMock.Object,
                _BroadcastAudiencesCacheMock.Object,
                _SpotLengthEngineMock.Object,
                _BroadcastLockingManagerApplicationServiceMock.Object,
                _PlanPricingServiceMock.Object,
                _QuarterCalculationEngineMock.Object,
                _DaypartDefaultServiceMock.Object
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
                .Setup(x => x.GetLockObject(It.IsAny<string>())).Returns(new LockResponse
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
        public void getVPVHForMaestroAudience()
        {
            var vpvhRequest = new VPVHRequest
            {
                AudienceIds = new List<int> { 38 },
                ShareBookId = 422
            };
            _NsiUniverseServiceMock
                .Setup(n => n.GetAudienceUniverseForMediaMonth(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int month, int audience) =>
                {
                    if (audience == 38) return 20000;
                    else return 10000;
                });
            var result = _PlanService.GetVPVHForAudiencesWithBooks(vpvhRequest);
            Assert.IsTrue(result.Exists(i => i.AudienceId == 38 && i.VPVH == 2));
        }

        [Test]
        public void getVPVHForHouseholds()
        {
            var vpvhRequest = new VPVHRequest
            {
                AudienceIds = new List<int> { 31 },
                ShareBookId = 422
            };
            var result = _PlanService.GetVPVHForAudiencesWithBooks(vpvhRequest);
            Assert.IsTrue(result.Exists(i => i.AudienceId == 31 && i.VPVH == 1));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OrderPlanDaypartsCorrectly()
        {
            var planDayparts = new List<PlanDaypart>
            {
                new PlanDaypart { DaypartCodeId = 1, StartTimeSeconds = 0, EndTimeSeconds = 2000, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 2, StartTimeSeconds = 1500, EndTimeSeconds = 2788, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 3, StartTimeSeconds = 2788, EndTimeSeconds = 3500, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 3500, EndTimeSeconds = 3600, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 1500, EndTimeSeconds = 3500, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 1499, EndTimeSeconds = 2788, FlightDays = new List<int> { 3 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 1500, EndTimeSeconds = 2788, FlightDays = new List<int> { 2 } }
            };

            var daypartDefaults = new List<DaypartDefaultDto>
            {
                new DaypartDefaultDto { Id = 1, Code = "OVN" },
                new DaypartDefaultDto { Id = 2, Code = "EF" },
                new DaypartDefaultDto { Id = 3, Code = "LF" },
                new DaypartDefaultDto { Id = 4, Code = "PA" },
            };

            var orderedPlanDayparts = planDayparts.OrderDayparts(daypartDefaults);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(orderedPlanDayparts));
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
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDefaultAudience()).Returns(new BroadcastAudience { Id = 1 });
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

        private static PlanDto _GetNewPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                SpotLengthId = 1,
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLenghtId = 1, Weight = 50 } },
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
                AudienceType = Entities.Enums.AudienceTypeEnum.Nielsen,
                HUTBookId = 436,
                PostingType = Entities.Enums.PostingTypeEnum.NTI,
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
                    new PlanDaypartDto{ DaypartCodeId = 2, StartTimeSeconds = 0, EndTimeSeconds = 2000, WeightingGoalPercent = 28.0 },
                    new PlanDaypartDto{ DaypartCodeId = 11, StartTimeSeconds = 1500, EndTimeSeconds = 2788, WeightingGoalPercent = 33.2 }
                },
                Vpvh = 0.234543,
                TargetRatingPoints = 50,
                TargetCPP = 50
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
                InventorySourcePercentages = new List<PlanPricingInventorySourceDto>(),
                InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>()
            };
        }
    }
}