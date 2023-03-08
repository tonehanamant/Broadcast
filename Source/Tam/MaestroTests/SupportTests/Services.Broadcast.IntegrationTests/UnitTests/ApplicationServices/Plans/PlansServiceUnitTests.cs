using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private Mock<IPlanBuyingRepository> _PlanBuyingRepositoryMock;
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
        private Mock<IPlanMarketSovCalculator> _PlanMarketSovCalculator;
        private Mock<IMarketCoverageRepository> _MarketCoverageRepository;
        private Mock<INtiToNsiConversionRepository> _NtiToNsiConversionRepository;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private Mock<ILockingEngine> _LockingEngineMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<ICampaignRepository> _CampaignRepositoryMock;
        private Mock<IPlanIsciRepository> _PlanIsciRepositoryMock;
        private Mock<ICampaignServiceApiClient> _CampaignServiceApiClient;

        private Mock<INtiUniverseService> _NtiUniverseService;

        [SetUp]
        public void CreatePlanService()
        {
            // Create Mocks
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();
            _PlanBuyingRepositoryMock = new Mock<IPlanBuyingRepository>();
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
            _MarketCoverageRepository = new Mock<IMarketCoverageRepository>();
            _NtiToNsiConversionRepository = new Mock<INtiToNsiConversionRepository>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _LockingEngineMock = new Mock<ILockingEngine>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _CampaignRepositoryMock = new Mock<ICampaignRepository>();
            _PlanIsciRepositoryMock = new Mock<IPlanIsciRepository>();
            _CampaignServiceApiClient = new Mock<ICampaignServiceApiClient>();
            _NtiUniverseService = new Mock<INtiUniverseService>();

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

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ICampaignRepository>())
                .Returns(_CampaignRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanIsciRepository>())
                .Returns(_PlanIsciRepositoryMock.Object);

            _DayRepositoryMock = new Mock<IDayRepository>();
            _DayRepositoryMock.Setup(s => s.GetDays()).Returns(new List<Day>());
            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IDayRepository>())
                .Returns(_DayRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IPlanRepository>())
                .Returns(_PlanRepositoryMock.Object);
            _DataRepositoryFactoryMock
               .Setup(s => s.GetDataRepository<IPlanBuyingRepository>())
               .Returns(_PlanBuyingRepositoryMock.Object);

            _DataRepositoryFactoryMock
               .Setup(s => s.GetDataRepository<IInventoryProprietarySummaryRepository>())
               .Returns(_InventoryProprietarySummaryRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<INtiToNsiConversionRepository>())
                .Returns(_NtiToNsiConversionRepository.Object);

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

            _DataRepositoryFactoryMock
                .Setup(s => s.GetDataRepository<IMarketCoverageRepository>())
                .Returns(_MarketCoverageRepository.Object);

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
            _LaunchDarklyClientStub.FeatureToggles.Add(FeatureToggles.ENABLE_PARTIAL_PLAN_SAVE, false);

            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);

            _PlanMarketSovCalculator = new Mock<IPlanMarketSovCalculator>();
            _PlanMarketSovCalculator.Setup(s => s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns<List<PlanAvailableMarketDto>>((m) => new PlanAvailableMarketCalculationResult
                {
                    AvailableMarkets = m,
                    TotalWeight = 12.123
                });

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
                    featureToggleHelper,
                    _PlanMarketSovCalculator.Object,
                    _ConfigurationSettingsHelperMock.Object,
                    _LockingEngineMock.Object,
                    _DateTimeEngineMock.Object,
                    _CampaignServiceApiClient.Object,
                    _NtiUniverseService.Object
                );

            _SpotLengthEngineMock
                .Setup(a => a.GetSpotLengths())
                .Returns(SpotLengthTestData.GetSpotLengthIdsByDuration);

            _SpotLengthEngineMock
                .Setup(a => a.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns<int>(SpotLengthTestData.GetSpotLengthValueById);

            _SpotLengthEngineMock.Setup(a => a.GetSpotLengthIdByValue(It.IsAny<int>()))
                .Returns<int>(SpotLengthTestData.GetSpotLengthIdByDuration);
            //.Returns(1);

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2022, 01, 01));
        }

        [Test]
        public void Construction()
        {
            Assert.IsNotNull(_PlanService);
        }

        [Test]
        public async Task SavePlanNew()
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

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

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

            _PlanRepositoryMock.Setup(s => s.GetPlanIdFromPricingJob(It.IsAny<int>()))
                .Returns(plan.Id);

            // Act
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

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

        /// <summary>
        /// This tests covers Saving an ADU Plan only.
        /// It verifies that the Goals are initialized correctly and saved correctly.
        /// </summary>
        [Test]
        public async Task SavePlanNew_AduOnlyPlan()
        {
            // Arrange
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = true;

            var sentToWeeklyBreakdownDistributeGoals = new List<PlanDto>();
            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Callback<PlanDto, double?, decimal?>((a, b, c) => sentToWeeklyBreakdownDistributeGoals.Add(a))
                .Returns(new List<WeeklyBreakdownWeek>());

            var sentToGroupWeklyBreakdownBySd = new List<IEnumerable<WeeklyBreakdownWeek>>();
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByStandardDaypart(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Callback<IEnumerable<WeeklyBreakdownWeek>>((a) => sentToGroupWeklyBreakdownBySd.Add(a))
                .Returns(new List<WeeklyBreakdownByStandardDaypart>
                {
                    new WeeklyBreakdownByStandardDaypart
                    {
                        Impressions = 0,
                        Budget = 0,
                        StandardDaypartId = 2
                    },
                    new WeeklyBreakdownByStandardDaypart
                    {
                        Impressions = 0,
                        Budget = 0,
                        StandardDaypartId = 11
                    }
                });

            var sentToCalculateBudget = new List<PlanDeliveryBudget>();
            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Callback<PlanDeliveryBudget>((a) => sentToCalculateBudget.Add(a))
                .Returns<PlanDeliveryBudget>((a) => new PlanDeliveryBudget
                {
                    AudienceId = a.AudienceId,
                    Universe = a.AudienceId * 1000,
                    Impressions = 0,
                    CPM = 0,
                    RatingPoints = 0,
                    CPP = 0
                });

            var saveNewPlanCalls = new List<DateTime>();
            var savedNewPlans = new List<PlanDto>();
            _PlanRepositoryMock.Setup(s => s.SaveNewPlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))                
                .Callback<PlanDto, string, DateTime>((a,b,c) => 
                    {
                        saveNewPlanCalls.Add(DateTime.Now);
                        savedNewPlans.Add(a);
                    });

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

            var plan = _GetNewAduOnlyPlan();
            var campaignId = plan.CampaignId;
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

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

            _PlanRepositoryMock.Setup(s => s.GetPlanIdFromPricingJob(It.IsAny<int>()))
                .Returns(plan.Id);

            // Act
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

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

            // ADU Only Plan Specific
            _PlanValidatorMock.Verify(s => s.ValidateAduPlan(It.IsAny<PlanDto>()), Times.Once);

            // goals are empty
            var savedPlan = savedNewPlans.First();

            Assert.AreEqual(0, savedPlan.Budget);
            Assert.AreEqual(0, savedPlan.TargetImpressions);
            Assert.AreEqual(0, savedPlan.TargetRatingPoints);
            Assert.AreEqual(0, savedPlan.TargetCPM);            
            Assert.AreEqual(0, savedPlan.TargetCPP);

            Assert.AreEqual(0, savedPlan.HHImpressions);
            Assert.AreEqual(0, savedPlan.HHRatingPoints);
            Assert.AreEqual(0, savedPlan.HHCPM);
            Assert.AreEqual(0, savedPlan.HHCPP);

            Assert.AreEqual(1, savedPlan.ImpressionsPerUnit);

            // weekly breakdown weeks were cleared in when sentToGroupWeklyBreakdownBySd
            _WeeklyBreakdownEngineMock
                .Verify(x => x.GroupWeeklyBreakdownByStandardDaypart(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()), Times.Once);
            var groupWeeklyBreakdownInput = sentToGroupWeklyBreakdownBySd.First();
            foreach (var weekitem in groupWeeklyBreakdownInput)
            {
                Assert.AreEqual(0, weekitem.PercentageOfWeek);
                Assert.AreEqual(0, weekitem.WeeklyBudget);
                Assert.AreEqual(0, weekitem.WeeklyImpressions);
                Assert.AreEqual(0, weekitem.WeeklyImpressionsPercentage);
                Assert.AreEqual(0, weekitem.WeeklyRatings);
                Assert.AreEqual(0, weekitem.WeeklyUnits);
            }

            // audience calculation input data was cleared
            _PlanBudgetDeliveryCalculatorMock.Verify(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()), Times.Once);
            var hhCalcInput = sentToCalculateBudget.Single(s => s.AudienceId == BroadcastConstants.HouseholdAudienceId);
            Assert.AreEqual(0, hhCalcInput.Budget);
            Assert.AreEqual(0, hhCalcInput.Impressions);

            // pricing goals are empty
            Assert.AreEqual(0, savedPlan.PricingParameters.AdjustedBudget);
            Assert.AreEqual(0, savedPlan.PricingParameters.AdjustedCPM);
            Assert.AreEqual(0, savedPlan.BuyingParameters.AdjustedBudget);
            Assert.AreEqual(0, savedPlan.BuyingParameters.AdjustedCPM);

            Assert.AreEqual(1, sentToWeeklyBreakdownDistributeGoals.Count);
            var sentToWeeklyBreakdownPlan = sentToWeeklyBreakdownDistributeGoals.First();
            Assert.AreEqual(0, sentToWeeklyBreakdownPlan.Budget);
            Assert.AreEqual(0, sentToWeeklyBreakdownPlan.TargetImpressions);
            Assert.AreEqual(0, sentToWeeklyBreakdownPlan.TargetRatingPoints);
            Assert.AreEqual(0, sentToWeeklyBreakdownPlan.TargetCPM);
            Assert.AreEqual(0, sentToWeeklyBreakdownPlan.TargetCPP);
            Assert.AreEqual(1, sentToWeeklyBreakdownPlan.ImpressionsPerUnit);
            foreach(var week in sentToWeeklyBreakdownPlan.WeeklyBreakdownWeeks)
            {
                Assert.AreEqual(4000000, week.AduImpressions);
            }
        }

        [Test]
        public async Task SavePlanDraft()
        {
            // Arrange
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_PARTIAL_PLAN_SAVE] = true;

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            var saveNewPlanCalls = new List<DateTime>();
            _PlanRepositoryMock.Setup(s => s.SaveDraft(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
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
            plan.IsDraft = true;
            var campaignId = plan.CampaignId;
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

            // Act
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

            // Assert
            Assert.AreEqual(1, saveNewPlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(1, setStatusCalls.Count, "Invalid call count.");
        }

        [Test]
        public void FilterValidDaypart()
        {
            //Arrange
            var sourceDayparts = new List<PlanDaypartDto>()
            {
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.News,
                    DaypartCodeId = 2,
                    StartTimeSeconds = 0,
                    EndTimeSeconds = 2000,
                    WeightingGoalPercent = 28.0
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.Sports,
                    DaypartOrganizationId = 1,
                    DaypartOrganizationName = "NBA",
                    StartTimeSeconds = 0,
                    EndTimeSeconds = 2000,
                    WeightingGoalPercent = 28.0
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.News
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.Sports,
                    DaypartOrganizationId = 1,
                    CustomName="Custom"
                },
            };

            //Act
            var result = _PlanService._FilterValidDaypart(sourceDayparts);

            //Assert
            Assert.AreEqual(2, result.Count);
        }

        private PlanPricingParametersDto _GetPricingParameters(int planId, int planVersionId)
        {
            return new PlanPricingParametersDto
            {
                Budget = 100,
                CPM = 12,
                CPP = 50,
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
        public async Task SavePlanUpdatedVersionNoChangeToPlan()
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

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

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

            // handle the test parameter
            var expectedQueuePricingJobCallCount = 0;
            var expectedUpdatePlanPricingVersionIdCalls = 1;
            var expectedSetPricingPlanVersionIdCallCount = 0;

            plan.IsOutOfSync = false;

            // Act
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

            // Assert
            Assert.AreEqual(plan.SpotAllocationModelMode, SpotAllocationModelMode.Quality);
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
        public async Task SavePlanSetSpotAllocationModelModeToDefaultOnGoalChanged()
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
            plan.SpotAllocationModelMode = SpotAllocationModelMode.Efficiency;
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
            afterPlan.Budget = 200m;
            var getPlanReturns = new Queue<PlanDto>();
            getPlanReturns.Enqueue(beforePlan);
            getPlanReturns.Enqueue(afterPlan);

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

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



            plan.IsOutOfSync = false;

            // Act
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

            // Assert
            Assert.AreEqual(plan.SpotAllocationModelMode, SpotAllocationModelMode.Quality);
        }

        [Test]
        public async Task SavePlanSetWithFlightDatesChanged()
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
            plan.SpotAllocationModelMode = SpotAllocationModelMode.Efficiency;
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
            afterPlan.Budget = 200m;
            DateTime flightStartDate = (DateTime)afterPlan.FlightStartDate;
            TimeSpan starttime = new TimeSpan(0, 0, 0);
            afterPlan.FlightStartDate = flightStartDate.Date + starttime;
            DateTime flightEndDate = (DateTime)plan.FlightEndDate;
            TimeSpan endTime = new TimeSpan(23, 59, 59);
            afterPlan.FlightEndDate = flightEndDate.Date + endTime;

            var getPlanReturns = new Queue<PlanDto>();
            getPlanReturns.Enqueue(beforePlan);
            getPlanReturns.Enqueue(afterPlan);

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

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

            plan.IsOutOfSync = false;

            var expectedStartTime = "12:00:00";
            var expectedEndTime = "11:59:59";
            var resultStartTime = Convert.ToDateTime(afterPlan.FlightStartDate).ToString("hh:mm:ss");
            var resultEndTime = Convert.ToDateTime(afterPlan.FlightEndDate).ToString("hh:mm:ss");

            // Act
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

            // Assert            
            Assert.AreEqual(resultStartTime, expectedStartTime);
            Assert.AreEqual(resultEndTime, expectedEndTime);
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
        [TestCase(PlanService.SaveState.CreatingNewPlan, true, false)]
        [TestCase(PlanService.SaveState.CreatingNewPlan, false, false)]
        [TestCase(PlanService.SaveState.UpdatingExisting, true, false)]
        [TestCase(PlanService.SaveState.UpdatingExisting, true, false)]
        [TestCase(PlanService.SaveState.UpdatingExisting, false, true)]
        public void ShouldPromotePricingResultsOnPlanSave(PlanService.SaveState saveState,
             bool goalsChanged, bool expectedResult)
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
            afterPlan.Dayparts.ForEach(d => d.PlanDaypartId++);

            if (goalsChanged)
            {
                afterPlan.Budget++;
            }

            // Act
            var result = _PlanService._ShouldPromotePricingResultsOnPlanSave(saveState, beforePlan, afterPlan);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(PlanService.SaveState.CreatingNewPlan, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, false)]
        [TestCase(PlanService.SaveState.UpdatingExisting, false)]
        public void _FinalizePricingOnPlanSave(PlanService.SaveState saveState, bool shouldPromotePricingResults)
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
            afterPlan.Dayparts.ForEach(d => d.PlanDaypartId++);

            var expectedUpdatePlanPricingVersionId = 0;
            var expectedSavePlanPricingParametersCalled = 0;

            if (shouldPromotePricingResults)
            {
                plan.JobId = 26;
                expectedUpdatePlanPricingVersionId++;
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

            var queuePricingJobCalled = 0;
            _PlanPricingServiceMock.Setup(s => s.QueuePricingJob(It.IsAny<PlanPricingParametersDto>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback(() => queuePricingJobCalled++);

            var savePlanPricingParametersCallsed = 0;
            _PlanRepositoryMock.Setup(s => s.SavePlanPricingParameters(It.IsAny<PlanPricingParametersDto>()))
                .Callback(() => savePlanPricingParametersCallsed++);

            // Act
            _PlanService._FinalizePricingOnPlanSave(saveState, plan, beforePlan, afterPlan, modifiedDate, modifiedBy, shouldPromotePricingResults);

            // Assert

            Assert.AreEqual(expectedUpdatePlanPricingVersionId, updatePlanPricingVersionIdCalled);
            Assert.AreEqual(expectedSavePlanPricingParametersCalled, savePlanPricingParametersCallsed);
        }
        [Test]
        [TestCase(PlanService.SaveState.CreatingNewPlan, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, true)]
        [TestCase(PlanService.SaveState.UpdatingExisting, false)]
        [TestCase(PlanService.SaveState.UpdatingExisting, false)]
        public void _FinalizeBuyingOnPlanSave(PlanService.SaveState saveState, bool shouldPromotePricingResults)
        {
            // Arrange
            const int planId = 12;
            const int planVersionId = 14;

            var plan = _GetNewPlan();
            plan.Id = planId;
            plan.VersionId = planVersionId;

            var beforePlan = _GetNewPlan();
            beforePlan.Id = planId;
            beforePlan.VersionId = planVersionId;

            var afterPlan = _GetNewPlan();
            afterPlan.Id = planId;
            afterPlan.VersionId = planVersionId + 1;
            afterPlan.Dayparts.ForEach(d => d.PlanDaypartId++);

            var expectedUpdatePlanBuyingVersionId = 0;
            var expectedSavePlanBuyingParametersCalled = 0;

            if (shouldPromotePricingResults)
            {
                plan.JobId = 26;
                expectedUpdatePlanBuyingVersionId++;
            }

            else
            {
                expectedSavePlanBuyingParametersCalled++;
            }

            var setBuyingPlanVersionIdCalled = 0;
            _PlanRepositoryMock.Setup(s => s.SetBuyingPlanVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => setBuyingPlanVersionIdCalled++);

            var updatePlanBuyingVersionIdCalled = 0;
            _PlanRepositoryMock.Setup(s => s.UpdatePlanBuyingVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => updatePlanBuyingVersionIdCalled++);

            var queuePricingJobCalled = 0;
            _PlanBuyingServiceMock.Setup(s => s.QueueBuyingJobAsync(It.IsAny<PlanBuyingParametersDto>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback(() => queuePricingJobCalled++);

            var savePlanBuyingParametersCallsed = 0;
            _PlanBuyingRepositoryMock.Setup(s => s.SavePlanBuyingParameters(It.IsAny<PlanBuyingParametersDto>()))
                .Callback(() => savePlanBuyingParametersCallsed++);

            // Act
            _PlanService._FinalizeBuyingOnPlanSave(saveState, plan, beforePlan, afterPlan, shouldPromotePricingResults);

            // Assert

            Assert.AreEqual(expectedUpdatePlanBuyingVersionId, updatePlanBuyingVersionIdCalled);
            Assert.AreEqual(expectedSavePlanBuyingParametersCalled, savePlanBuyingParametersCallsed);
        }
        private class UpdatePlanPricingVersionIdParams
        {
            public int AfterPlanVersionID { get; set; }
            public int BeforePlanVersionID { get; set; }
        }

        [Test]
        public async Task DispatchAggregation_WasTriggeredOnSave()
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

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto { VersionId = 66 });

            // aggregatePlanSynchronously = false because we are testing that aggregation is on a separate thread.
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: false);
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
        [TestCase(0)]
        public void PlanStatusTransitionTriggersPlanSaveAndCampaignAggregation(int expectedCallCount)
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
            plansToReturn[1].Dayparts.ForEach(d => d.PlanDaypartId++);
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
            Assert.AreEqual(expectedCallCount, savePlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(expectedCallCount, aggregateCallCount, "Invalid call count.");
            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(1, "automated status update"), Times.Exactly(expectedCallCount));
        }

        [Test]
        [TestCase(2)]
        public void PlanStatusTransitionV2TriggersPlanSaveAndCampaignAggregation(int expectedCallCount)
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
            plansToReturn[1].Dayparts.ForEach(d => d.PlanDaypartId++);
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
            _PlanService.AutomaticStatusTransitionsJobEntryPointV2();
            Thread.Sleep(200);

            // Assert
            Assert.AreEqual(expectedCallCount, savePlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(expectedCallCount, aggregateCallCount, "Invalid call count.");
            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(1, "automated status update v2"), Times.Exactly(expectedCallCount));
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
            _PlanRepositoryMock
                 .Setup(s => s.GetAllCustomDaypartOrganizations())
                 .Returns(new List<CustomDaypartOrganizationDto>
                 {
                     new CustomDaypartOrganizationDto
                     {
                        Id = 1,
                        OrganizationName = "NFL"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 2,
                        OrganizationName = "MLB"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 3,
                        OrganizationName = "MLS"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 4,
                        OrganizationName = "NBA"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 5,
                        OrganizationName = "PGA"
                     },
                      new CustomDaypartOrganizationDto
                     {
                        Id = 6,
                        OrganizationName = "PGA"
                     },
                 });
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
                .Setup(s => s.GetAllCustomDaypartOrganizations())
                .Returns(new List<CustomDaypartOrganizationDto>
                {
                     new CustomDaypartOrganizationDto
                     {
                        Id = 1,
                        OrganizationName = "NFL"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 2,
                        OrganizationName = "MLB"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 3,
                        OrganizationName = "MLS"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 4,
                        OrganizationName = "NBA"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 5,
                        OrganizationName = "PGA"
                     },
                      new CustomDaypartOrganizationDto
                     {
                        Id = 6,
                        OrganizationName = "PGA"
                     },
                });

            _PlanRepositoryMock
                .Setup(s => s.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()))
                .Returns(.85d);

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
        [UseReporter(typeof(DiffReporter))]
        public void CanGetPlan_v3()
        {
            // Arrange
            var planToReturn = _GetNewPlanWithSecondaryAudiences();

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
            _WeeklyBreakdownEngineMock
               .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
               .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());
            _PlanRepositoryMock
                .Setup(s => s.GetAllCustomDaypartOrganizations())
                .Returns(new List<CustomDaypartOrganizationDto>
                {
                     new CustomDaypartOrganizationDto
                     {
                        Id = 1,
                        OrganizationName = "NFL"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 2,
                        OrganizationName = "MLB"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 3,
                        OrganizationName = "MLS"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 4,
                        OrganizationName = "NBA"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 5,
                        OrganizationName = "PGA"
                     },
                      new CustomDaypartOrganizationDto
                     {
                        Id = 6,
                        OrganizationName = "PGA"
                     },
                });

            _PlanRepositoryMock
                .Setup(s => s.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()))
                .Returns(.85d);

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
            var result = _PlanService.GetPlan_v3(1, 1);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void PlanStatusTransitionV2FailsOnLockedPlans()
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
            var exception = Assert.Throws<Exception>(() => _PlanService.AutomaticStatusTransitionsJobEntryPointV2());

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public async Task DispatchAggregation_WithAggregationError()
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

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto { VersionId = 66 });

            // aggregatePlanSynchronously = false because we are testing that aggregation is on a separate thread.
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: false);
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
        public async Task CanNotUpdateLockedPlan()
        {
            const string expectedMessage = "The chosen plan has been locked by IntegrationUser Try to save the plan as draft";
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
            plan.Dayparts.ForEach(s => s.PlanDaypartId++);

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(beforePlan);

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

            PlanSaveException exception = null;

            try
            {
                await _PlanService.SavePlanAsync(plan, "IntegrationUser", new DateTime(2019, 10, 23), aggregatePlanSynchronously: true);
            }
            catch (PlanSaveException ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public async Task CanSavePlanWithFlightDaysDefaultingToSevedDays()
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
                .Returns(new PlanDto { VersionId = 66 });

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

            // Act
            await _PlanService.SavePlanAsync(plan, "IntegrationUser", new DateTime(2019, 10, 23), aggregatePlanSynchronously: true);

            // Assert
            Assert.NotNull(plan.FlightDays, "FlightDays list should be initialized");
            Assert.AreEqual(plan.FlightDays, new List<int> { 1, 2, 3, 4, 5, 6, 7 });
        }

        [Test]
        public async Task CanNotUpdatePlanWithPricingModelRunning()
        {
            const string expectedMessage = "The pricing model is running for the plan Try to save the plan as draft";
            _PlanPricingServiceMock.Setup(x => x.IsPricingModelRunningForPlan(It.IsAny<int>())).Returns(true);

            PlanDto plan = _GetNewPlan();
            plan.Id = 1;
            plan.VersionId = 1;

            PlanSaveException exception = null;

            try
            {
                await _PlanService.SavePlanAsync(plan, "IntegrationUser", new DateTime(2019, 10, 23));
            }
            catch (PlanSaveException ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception);
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
            var results = _PlanService.CalculateBudget(planBudget);

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
            var results = _PlanService.CalculateCreativeLengthWeight(request, removeNonCalculatedItems: true);

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
            var results = _PlanService.CalculateCreativeLengthWeight(request, removeNonCalculatedItems: true);

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
            var result = _PlanService.CalculateCreativeLengthWeight(request, removeNonCalculatedItems: true);

            // Assert
            Assert.AreEqual(null, result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateCreativeLengthWeight_DoNotRemoveNonCalculated()
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
            var results = _PlanService.CalculateCreativeLengthWeight(request, removeNonCalculatedItems: false);

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
                ProductMasterId = new Guid("C8C76C3B-8C39-42CF-9657-B7AD2B8BA320"),
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightNotesInternal = "Internal sample notes",
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 1, 20),
                    new DateTime(2019, 4, 15)
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
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 20, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn", IsUserShareOfVoicePercent = true },
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York", IsUserShareOfVoicePercent = true }
                },
                AvailableMarketsSovTotal = 56.7,
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto { MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUS = 5.5, Rank = 5, Market = "Burlington-Plattsburgh" },
                    new PlanBlackoutMarketDto { MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUS = 2.5, Rank = 8, Market = "Amarillo" },
                },
                
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        PlanDaypartId = 56,
                        DaypartTypeId = DaypartTypeEnum.News,
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
                        PlanDaypartId = 58,
                        DaypartTypeId = DaypartTypeEnum.News,
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

        private static PlanDto _GetNewPlanWithSecondaryAudiences()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                ProductMasterId = new Guid("C8C76C3B-8C39-42CF-9657-B7AD2B8BA320"),
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightNotesInternal = "Internal sample notes",
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 1, 20),
                    new DateTime(2019, 4, 15)
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
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 20, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn", IsUserShareOfVoicePercent = true },
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York", IsUserShareOfVoicePercent = true }
                },
                AvailableMarketsSovTotal = 56.7,
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto { MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUS = 5.5, Rank = 5, Market = "Burlington-Plattsburgh" },
                    new PlanBlackoutMarketDto { MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUS = 2.5, Rank = 8, Market = "Amarillo" },
                },
                SecondaryAudiences = new List<PlanAudienceDto>
                {
                    new PlanAudienceDto {Vpvh= 0.027875,AudienceId=4},
                    new PlanAudienceDto {Vpvh= 0.30575,AudienceId=5}
                },
                HHImpressions = 1000,
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        PlanDaypartId = 56,
                        DaypartTypeId = DaypartTypeEnum.News,
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
                        PlanDaypartId = 58,
                        DaypartTypeId = DaypartTypeEnum.News,
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

        private static PlanDto _GetPlanToSaveLockedFlag()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                ProductMasterId = new Guid("C8C76C3B-8C39-42CF-9657-B7AD2B8BA320"),
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightNotesInternal = "Internal sample notes",
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
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 20, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn", IsUserShareOfVoicePercent = true},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York", IsUserShareOfVoicePercent = true}
                },
                AvailableMarketsSovTotal = 56.7,
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
                },

                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
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
                    WeeklyRatings = 15,
                    IsLocked = true
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
                    IsLocked = false
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
            var requestWeeks = _GetWeeklyBreakdownWeeks();
            // make the budgets more believable for a better CPM
            requestWeeks.ForEach(w => w.WeeklyBudget *= 1000);
            var request = new LengthMakeUpRequest
            {
                CreativeLengths = new List<CreativeLength>{
                    new CreativeLength{ SpotLengthId = 1, Weight = 50},
                    new CreativeLength{ SpotLengthId = 2, Weight = 20},
                    new CreativeLength{ SpotLengthId = 3, Weight = 30}
                },
                TotalImpressions = 100000,
                Weeks = requestWeeks
            };

            var distributeWeightCallCount = 0;
            _CreativeLengthEngineMock.Setup(s => s.DistributeWeight(It.IsAny<List<CreativeLength>>()))
                .Callback(() => distributeWeightCallCount++)
                .Returns(request.CreativeLengths);

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
            var requestWeeks = _GetWeeklyBreakdownWeeks();
            // make the budgets more believable for a better CPM
            requestWeeks.ForEach(w => w.WeeklyBudget *= 1000);

            var request = new LengthMakeUpRequest
            {
                CreativeLengths = new List<CreativeLength>{
                    new CreativeLength{ SpotLengthId = 1, Weight = 50},
                    new CreativeLength{ SpotLengthId = 2, Weight = 20},
                    new CreativeLength{ SpotLengthId = 3}
                },
                TotalImpressions = 100000,
                Weeks = requestWeeks
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

        [Test]
        public void CalculateMarketWeightChange()
        {
            // Arrange
            var plan = _GetNewPlan();

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeightChange(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<short>(), It.IsAny<double?>()))
                .Returns(standardResult);
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketsAdded(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketsRemoved(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<short>>()))
                .Returns(standardResult);

            // Act
            var results = _PlanService.CalculateMarketWeightChange(plan.AvailableMarkets, plan.AvailableMarkets[0].MarketCode, 12);

            // Assert
            Assert.IsNotNull(results);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketWeightChange(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<short>(), It.IsAny<double?>()), Times.Once);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketsAdded(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<PlanAvailableMarketDto>>()), Times.Never);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketsRemoved(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<short>>()), Times.Never);
        }

        [Test]
        public void CalculateMarketAdded()
        {
            // Arrange
            var plan = _GetNewPlan();
            var addedMarkets = new List<PlanAvailableMarketDto>
            {
                new PlanAvailableMarketDto { MarketCoverageFileId = 1, Rank = 71, Market = "Flint-Saginaw-Bay City", PercentageOfUS = 0.00367, MarketCode = 113},
                new PlanAvailableMarketDto { MarketCoverageFileId = 1, Rank = 92, Market = "Charleston, SC", PercentageOfUS = 0.00286, MarketCode = 119 }
            };

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeightChange(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<short>(), It.IsAny<double?>()))
                .Returns(standardResult);
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketsAdded(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketsRemoved(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<short>>()))
                .Returns(standardResult);

            // Act
            var results = _PlanService.CalculateMarketsAdded(plan.AvailableMarkets, addedMarkets);

            // Assert
            Assert.IsNotNull(results);
            var addedMarketCodes = addedMarkets.Select(m => m.MarketCode).ToList();
            var didNotResetIsUserShareOfVoicePercent = results.AvailableMarkets.Where(m => addedMarketCodes.Contains(m.MarketCode))
                .Any(m => m.IsUserShareOfVoicePercent);
            Assert.IsFalse(didNotResetIsUserShareOfVoicePercent);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketWeightChange(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<short>(), It.IsAny<double?>()), Times.Never);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketsAdded(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<PlanAvailableMarketDto>>()), Times.Once);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketsRemoved(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<short>>()), Times.Never);
        }

        [Test]
        public void CalculateMarketRemoved()
        {
            // Arrange
            var plan = _GetNewPlan();
            var removedMarketCodes = new List<short> { 101 };

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeightChange(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<short>(), It.IsAny<double?>()))
                .Returns(standardResult);
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketsAdded(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketsRemoved(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<short>>()))
                .Returns(standardResult);

            // Act
            var results = _PlanService.CalculateMarketsRemoved(plan.AvailableMarkets, removedMarketCodes);

            // Assert
            Assert.IsNotNull(results);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketWeightChange(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<short>(), It.IsAny<double?>()), Times.Never);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketsAdded(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<PlanAvailableMarketDto>>()), Times.Never);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketsRemoved(It.IsAny<List<PlanAvailableMarketDto>>(), It.IsAny<List<short>>()), Times.Once);
        }

        [Test]
        public void CalculateDefaultPlanAvailableMarkets()
        {
            // Arrange
            var markets = MarketsTestData.GetMarketsWithLatestCoverage();
            _MarketCoverageRepository.Setup(s => s.GetMarketsWithLatestCoverage())
                .Returns(markets);
            var availablePlanMarkets = MarketsTestData.GetPlanAvailableMarkets();
            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = availablePlanMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

            // Act
            var results = _PlanService.CalculateDefaultPlanAvailableMarkets();

            // Assert
            Assert.IsNotNull(results);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()), Times.Once());
        }

        [Test]
        public void CalculateMarketWeightsClearAll()
        {
            // Arrange
            var markets = MarketsTestData.GetPlanAvailableMarkets().Take(8).ToList();
            var expectedCount = markets.Count;
            const double expectedTotalWeight = 100.0;

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = markets, TotalWeight = expectedTotalWeight };
            _PlanMarketSovCalculator.Setup(s => s.CalculateMarketWeightsClearAll(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

            // Act
            var result = _PlanService.CalculateMarketWeightsClearAll(markets);

            // Assert
            Assert.AreEqual(expectedCount, result.AvailableMarkets.Count);
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            _PlanMarketSovCalculator.Verify(s => s.CalculateMarketWeightsClearAll(It.IsAny<List<PlanAvailableMarketDto>>()), Times.Once());
        }

        [Test]
        [TestCase(PostingTypeEnum.NSI)]
        [TestCase(PostingTypeEnum.NTI)]
        public void CalculatePostingTypeBudgets_ImpressionsCalculateRate(PostingTypeEnum givenPostingType)
        {
            // Budget is static for this use case.

            // Arrange
            const int testStandardDaypartId = 2;
            const double testConversionRate = 0.5;
            const double testOriginalImpressions = 10000.0;
            const decimal testBudget = 5000;

            var expectedNsiImpressions = givenPostingType == PostingTypeEnum.NSI
                ? testOriginalImpressions
                : testOriginalImpressions / testConversionRate;

            var expectedNtiImpressions = givenPostingType == PostingTypeEnum.NSI
                ? testOriginalImpressions * testConversionRate
                : testOriginalImpressions;

            var budgetRequest = new PlanDeliveryPostingTypeBudget
            {
                PostingType = givenPostingType,
                StandardDaypartId = testStandardDaypartId,
                AudienceId = 31,
                Budget = testBudget,
                DeliveryImpressions = testOriginalImpressions
            };

            _NtiToNsiConversionRepository.Setup(s => s.GetLatestNtiToNsiConversionRates())
                .Returns(new List<NtiToNsiConversionRate> { new NtiToNsiConversionRate { StandardDaypartId = testStandardDaypartId, ConversionRate = testConversionRate } });

            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Returns<PlanDeliveryBudget>((i) => i);

            // Act
            var results = _PlanService.CalculatePostingTypeBudgets(budgetRequest);

            // Assert
            Assert.AreEqual(2, results.Count);
            var nsiResult = results.Single(s => s.PostingType == PostingTypeEnum.NSI);
            var ntiResult = results.Single(s => s.PostingType == PostingTypeEnum.NTI);
            Assert.AreEqual(expectedNsiImpressions, nsiResult.DeliveryImpressions);
            Assert.AreEqual(expectedNtiImpressions, ntiResult.DeliveryImpressions);
            Assert.AreEqual(testBudget, nsiResult.Budget);
            Assert.AreEqual(testBudget, ntiResult.Budget);
            _NtiToNsiConversionRepository.Verify(s => s.GetLatestNtiToNsiConversionRates(), Times.Once);
            _PlanBudgetDeliveryCalculatorMock.Verify(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()), Times.Exactly(2));
        }

        [Test]
        [TestCase(PostingTypeEnum.NSI)]
        [TestCase(PostingTypeEnum.NTI)]
        public void CalculatePostingTypeBudgets_ImpressionsCalculateBudget(PostingTypeEnum givenPostingType)
        {
            // Rate is static for this use case.

            // Arrange
            const int testStandardDaypartId = 2;
            const double testConversionRate = 0.5;
            const double testOriginalImpressions = 10000.0;
            const decimal testRate = 50;

            var expectedNsiImpressions = givenPostingType == PostingTypeEnum.NSI
                ? testOriginalImpressions
                : testOriginalImpressions / testConversionRate;

            var expectedNtiImpressions = givenPostingType == PostingTypeEnum.NSI
                ? testOriginalImpressions * testConversionRate
                : testOriginalImpressions;

            var budgetRequest = new PlanDeliveryPostingTypeBudget
            {
                PostingType = givenPostingType,
                StandardDaypartId = testStandardDaypartId,
                AudienceId = 31,
                CPM = testRate,
                DeliveryImpressions = testOriginalImpressions
            };

            _NtiToNsiConversionRepository.Setup(s => s.GetLatestNtiToNsiConversionRates())
                .Returns(new List<NtiToNsiConversionRate> { new NtiToNsiConversionRate { StandardDaypartId = testStandardDaypartId, ConversionRate = testConversionRate } });

            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Returns<PlanDeliveryBudget>((i) => i);

            // Act
            var results = _PlanService.CalculatePostingTypeBudgets(budgetRequest);

            // Assert
            Assert.AreEqual(2, results.Count);
            var nsiResult = results.Single(s => s.PostingType == PostingTypeEnum.NSI);
            var ntiResult = results.Single(s => s.PostingType == PostingTypeEnum.NTI);
            Assert.AreEqual(expectedNsiImpressions, nsiResult.DeliveryImpressions);
            Assert.AreEqual(expectedNtiImpressions, ntiResult.DeliveryImpressions);
            Assert.AreEqual(testRate, nsiResult.CPM);
            Assert.AreEqual(testRate, ntiResult.CPM);
            _NtiToNsiConversionRepository.Verify(s => s.GetLatestNtiToNsiConversionRates(), Times.Once);
            _PlanBudgetDeliveryCalculatorMock.Verify(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()), Times.Exactly(2));
        }

        [Test]
        [TestCase(PostingTypeEnum.NSI)]
        [TestCase(PostingTypeEnum.NTI)]
        public void CalculatePostingTypeBudgets_ImpressionsCalculateDelivery(PostingTypeEnum givenPostingType)
        {
            // Budget is static for this use case.

            // Arrange
            const int testStandardDaypartId = 2;
            const double testConversionRate = 0.5;
            const decimal testRate = 50;
            const decimal testBudget = 5000;

            const double testCalculatedResultImpressions = 10000.0;

            var expectedNsiImpressions = givenPostingType == PostingTypeEnum.NSI
                ? testCalculatedResultImpressions
                : testCalculatedResultImpressions / testConversionRate;

            var expectedNtiImpressions = givenPostingType == PostingTypeEnum.NSI
                ? testCalculatedResultImpressions * testConversionRate
                : testCalculatedResultImpressions;

            var budgetRequest = new PlanDeliveryPostingTypeBudget
            {
                PostingType = givenPostingType,
                StandardDaypartId = testStandardDaypartId,
                AudienceId = 31,
                CPM = testRate,
                Budget = testBudget
            };

            _NtiToNsiConversionRepository.Setup(s => s.GetLatestNtiToNsiConversionRates())
                .Returns(new List<NtiToNsiConversionRate> { new NtiToNsiConversionRate { StandardDaypartId = testStandardDaypartId, ConversionRate = testConversionRate } });

            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Returns<PlanDeliveryBudget>((i) =>
                {
                    i.Impressions = i.Impressions ?? testCalculatedResultImpressions * 1000;
                    return i;
                });

            // Act
            var results = _PlanService.CalculatePostingTypeBudgets(budgetRequest);

            // Assert
            Assert.AreEqual(2, results.Count);
            var nsiResult = results.Single(s => s.PostingType == PostingTypeEnum.NSI);
            var ntiResult = results.Single(s => s.PostingType == PostingTypeEnum.NTI);
            Assert.AreEqual(expectedNsiImpressions, nsiResult.DeliveryImpressions);
            Assert.AreEqual(expectedNtiImpressions, ntiResult.DeliveryImpressions);
            Assert.AreEqual(testBudget, nsiResult.Budget);
            Assert.AreEqual(testBudget, ntiResult.Budget);
            _NtiToNsiConversionRepository.Verify(s => s.GetLatestNtiToNsiConversionRates(), Times.Once);
            _PlanBudgetDeliveryCalculatorMock.Verify(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()), Times.Exactly(2));
        }

        [Test]
        [TestCase(PostingTypeEnum.NSI)]
        [TestCase(PostingTypeEnum.NTI)]
        public void CalculatePostingTypeBudgets_RatingPointsCalculateRate(PostingTypeEnum givenPostingType)
        {
            // Budget is static for this use case.

            // Arrange
            const int testStandardDaypartId = 2;
            const double testConversionRate = 0.5;
            const double testOriginalRatingPoints = 50.0;
            const decimal testBudget = 5000;

            var expectedNsiRatingPoints = givenPostingType == PostingTypeEnum.NSI
                ? testOriginalRatingPoints
                : testOriginalRatingPoints / testConversionRate;

            var expectedNtiRatingPoints = givenPostingType == PostingTypeEnum.NSI
                ? testOriginalRatingPoints * testConversionRate
                : testOriginalRatingPoints;

            var budgetRequest = new PlanDeliveryPostingTypeBudget
            {
                PostingType = givenPostingType,
                StandardDaypartId = testStandardDaypartId,
                AudienceId = 31,
                Budget = testBudget,
                DeliveryRatingPoints = testOriginalRatingPoints
            };

            _NtiToNsiConversionRepository.Setup(s => s.GetLatestNtiToNsiConversionRates())
                .Returns(new List<NtiToNsiConversionRate> { new NtiToNsiConversionRate { StandardDaypartId = testStandardDaypartId, ConversionRate = testConversionRate } });

            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Returns<PlanDeliveryBudget>((i) =>
                    {
                        i.Impressions = i.Impressions ?? 666666;
                        return i;
                    }
                );

            // Act
            var results = _PlanService.CalculatePostingTypeBudgets(budgetRequest);

            // Assert
            Assert.AreEqual(2, results.Count);
            var nsiResult = results.Single(s => s.PostingType == PostingTypeEnum.NSI);
            var ntiResult = results.Single(s => s.PostingType == PostingTypeEnum.NTI);
            Assert.AreEqual(expectedNsiRatingPoints, nsiResult.DeliveryRatingPoints);
            Assert.AreEqual(expectedNtiRatingPoints, ntiResult.DeliveryRatingPoints);
            Assert.AreEqual(testBudget, nsiResult.Budget);
            Assert.AreEqual(testBudget, ntiResult.Budget);
            _NtiToNsiConversionRepository.Verify(s => s.GetLatestNtiToNsiConversionRates(), Times.Once);
            _PlanBudgetDeliveryCalculatorMock.Verify(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()), Times.Exactly(2));
        }

        [Test]
        [TestCase(PostingTypeEnum.NSI)]
        [TestCase(PostingTypeEnum.NTI)]
        public void CalculatePostingTypeBudgets_RatingPointsCalculateBudget(PostingTypeEnum givenPostingType)
        {
            // Rate is static for this use case.

            // Arrange
            const int testStandardDaypartId = 2;
            const double testConversionRate = 0.5;
            const double testOriginalRatingPoints = 50.0;
            const decimal testRate = 50;

            var expectedNsiRatingPoints = givenPostingType == PostingTypeEnum.NSI
                ? testOriginalRatingPoints
                : testOriginalRatingPoints / testConversionRate;

            var expectedNtiRatingPoints = givenPostingType == PostingTypeEnum.NSI
                ? testOriginalRatingPoints * testConversionRate
                : testOriginalRatingPoints;

            var budgetRequest = new PlanDeliveryPostingTypeBudget
            {
                PostingType = givenPostingType,
                StandardDaypartId = testStandardDaypartId,
                AudienceId = 31,
                CPP = testRate,
                DeliveryRatingPoints = testOriginalRatingPoints
            };

            _NtiToNsiConversionRepository.Setup(s => s.GetLatestNtiToNsiConversionRates())
                .Returns(new List<NtiToNsiConversionRate> { new NtiToNsiConversionRate { StandardDaypartId = testStandardDaypartId, ConversionRate = testConversionRate } });

            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Returns<PlanDeliveryBudget>((i) =>
                {
                    i.Impressions = i.Impressions ?? 666666;
                    return i;
                }
                );

            // Act
            var results = _PlanService.CalculatePostingTypeBudgets(budgetRequest);

            // Assert
            Assert.AreEqual(2, results.Count);
            var nsiResult = results.Single(s => s.PostingType == PostingTypeEnum.NSI);
            var ntiResult = results.Single(s => s.PostingType == PostingTypeEnum.NTI);
            Assert.AreEqual(expectedNsiRatingPoints, nsiResult.DeliveryRatingPoints);
            Assert.AreEqual(expectedNtiRatingPoints, ntiResult.DeliveryRatingPoints);
            Assert.AreEqual(testRate, nsiResult.CPP);
            Assert.AreEqual(testRate, ntiResult.CPP);
            _NtiToNsiConversionRepository.Verify(s => s.GetLatestNtiToNsiConversionRates(), Times.Once);
            _PlanBudgetDeliveryCalculatorMock.Verify(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()), Times.Exactly(2));
        }

        [Test]
        [TestCase(PostingTypeEnum.NSI)]
        [TestCase(PostingTypeEnum.NTI)]
        public void CalculatePostingTypeBudgets_RatingPointsCalculateDelivery(PostingTypeEnum givenPostingType)
        {
            // Budget is static for this use case.

            // Arrange
            const int testStandardDaypartId = 2;
            const double testConversionRate = 0.5;
            const decimal testRate = 50;
            const decimal testBudget = 5000;

            const double testCalculatedResultRatingPoints = 50.0;

            var expectedNsiRatingPoints = givenPostingType == PostingTypeEnum.NSI
                ? testCalculatedResultRatingPoints
                : testCalculatedResultRatingPoints / testConversionRate;

            var expectedNtiRatingPoints = givenPostingType == PostingTypeEnum.NSI
                ? testCalculatedResultRatingPoints * testConversionRate
                : testCalculatedResultRatingPoints;

            var budgetRequest = new PlanDeliveryPostingTypeBudget
            {
                PostingType = givenPostingType,
                StandardDaypartId = testStandardDaypartId,
                AudienceId = 31,
                Budget = testBudget,
                CPP = testRate
            };

            _NtiToNsiConversionRepository.Setup(s => s.GetLatestNtiToNsiConversionRates())
                .Returns(new List<NtiToNsiConversionRate> { new NtiToNsiConversionRate { StandardDaypartId = testStandardDaypartId, ConversionRate = testConversionRate } });

            _PlanBudgetDeliveryCalculatorMock
                .Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Returns<PlanDeliveryBudget>((i) =>
                {
                    i.Impressions = 666000;
                    i.RatingPoints = i.RatingPoints ?? testCalculatedResultRatingPoints;
                    return i;
                });

            // Act
            var results = _PlanService.CalculatePostingTypeBudgets(budgetRequest);

            // Assert
            Assert.AreEqual(2, results.Count);
            var nsiResult = results.Single(s => s.PostingType == PostingTypeEnum.NSI);
            var ntiResult = results.Single(s => s.PostingType == PostingTypeEnum.NTI);
            Assert.AreEqual(expectedNsiRatingPoints, nsiResult.DeliveryRatingPoints);
            Assert.AreEqual(expectedNtiRatingPoints, ntiResult.DeliveryRatingPoints);
            Assert.AreEqual(testBudget, nsiResult.Budget);
            Assert.AreEqual(testBudget, ntiResult.Budget);
            _NtiToNsiConversionRepository.Verify(s => s.GetLatestNtiToNsiConversionRates(), Times.Once);
            _PlanBudgetDeliveryCalculatorMock.Verify(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()), Times.Exactly(2));
        }

        [Test]
        public async Task CommitPricingAllocationModel_NoJob()
        {
            const string username = "testUser";
            const PostingTypeEnum testPostingType = PostingTypeEnum.NSI;
            const SpotAllocationModelMode testSpotAllocationModelMode = SpotAllocationModelMode.Efficiency;

            var beforePlan = _GetNewPlan();
            beforePlan.PostingType = PostingTypeEnum.NTI;
            beforePlan.SpotAllocationModelMode = SpotAllocationModelMode.Quality;
            beforePlan.Budget = 50000;
            beforePlan.TargetImpressions = 1200000;
            beforePlan.TargetCPM = 555.6m;
            beforePlan.Vpvh = 12; // this is the user entered VPVH
            var planId = beforePlan.Id;
            beforePlan.PricingParameters.ProprietaryInventory = new List<InventoryProprietarySummary>();

            _PlanRepositoryMock.SetupSequence(s => s.GetPlan(It.IsAny<int>(), null))
                .Returns(beforePlan);
            _PlanRepositoryMock
                .Setup(s => s.GetAllCustomDaypartOrganizations())
                .Returns(new List<CustomDaypartOrganizationDto>
                {
                     new CustomDaypartOrganizationDto
                     {
                        Id = 1,
                        OrganizationName = "NFL"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 2,
                        OrganizationName = "MLB"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 3,
                        OrganizationName = "MLS"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 4,
                        OrganizationName = "NBA"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 5,
                        OrganizationName = "PGA"
                     },
                      new CustomDaypartOrganizationDto
                     {
                        Id = 6,
                        OrganizationName = "PGA"
                     },
                });

            _SpotLengthEngineMock.Setup(s => s.GetSpotLengths())
                .Returns(SpotLengthTestData.GetSpotLengthIdsByDuration());

            _WeeklyBreakdownEngineMock.Setup(s => s.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(It.IsAny<PlanDto>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            _WeeklyBreakdownEngineMock.Setup(s => s.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>(), It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(new List<WeeklyBreakdownByWeek>());

            _PlanPricingServiceMock.Setup(s => s.GetAllCurrentPricingExecutions(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new CurrentPricingExecutions());

            InvalidOperationException caught = null;

            try
            {
                await _PlanService.CommitPricingAllocationModelAsync(beforePlan.Id, testSpotAllocationModelMode, testPostingType, username);
            }
            catch (InvalidOperationException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught);
            Assert.IsTrue(caught.Message.Contains($"Did not find a pricing job for PlanId='{planId}'"));
        }

        [Test]
        public async Task CommitPricingAllocationModel_NoJobResults()
        {
            const string username = "testUser";
            const PostingTypeEnum testPostingType = PostingTypeEnum.NSI;
            const SpotAllocationModelMode testSpotAllocationModelMode = SpotAllocationModelMode.Efficiency;

            var beforePlan = _GetNewPlan();
            beforePlan.PostingType = PostingTypeEnum.NTI;
            beforePlan.SpotAllocationModelMode = SpotAllocationModelMode.Quality;
            beforePlan.Budget = 50000;
            beforePlan.TargetImpressions = 1200000;
            beforePlan.TargetCPM = 555.6m;
            beforePlan.Vpvh = 12; // this is the user entered VPVH
            var planId = beforePlan.Id;
            beforePlan.PricingParameters.ProprietaryInventory = new List<InventoryProprietarySummary>();

            _PlanRepositoryMock.SetupSequence(s => s.GetPlan(It.IsAny<int>(), null))
                .Returns(beforePlan);
            _PlanRepositoryMock
                .Setup(s => s.GetAllCustomDaypartOrganizations())
                .Returns(new List<CustomDaypartOrganizationDto>
                {
                     new CustomDaypartOrganizationDto
                     {
                        Id = 1,
                        OrganizationName = "NFL"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 2,
                        OrganizationName = "MLB"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 3,
                        OrganizationName = "MLS"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 4,
                        OrganizationName = "NBA"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 5,
                        OrganizationName = "PGA"
                     },
                      new CustomDaypartOrganizationDto
                     {
                        Id = 6,
                        OrganizationName = "PGA"
                     },
                });


            _SpotLengthEngineMock.Setup(s => s.GetSpotLengths())
                .Returns(SpotLengthTestData.GetSpotLengthIdsByDuration());

            _WeeklyBreakdownEngineMock.Setup(s => s.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(It.IsAny<PlanDto>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            _WeeklyBreakdownEngineMock.Setup(s => s.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>(), It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(new List<WeeklyBreakdownByWeek>());

            _PlanPricingServiceMock.Setup(s => s.GetAllCurrentPricingExecutions(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new CurrentPricingExecutions
                {
                    Job = new PlanPricingJob()
                });


            InvalidOperationException caught = null;

            try
            {
                await _PlanService.CommitPricingAllocationModelAsync(beforePlan.Id, testSpotAllocationModelMode, testPostingType, username);
            }
            catch (InvalidOperationException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught);
            Assert.IsTrue(caught.Message.Contains($"Did not find pricing results for PlanId='{planId}'"));
        }

        [Test]
        public async Task CommitPricingAllocationModel()
        {
            // Arrange
            const string username = "testUser";
            const PostingTypeEnum testPostingType = PostingTypeEnum.NSI;
            const SpotAllocationModelMode testSpotAllocationModelMode = SpotAllocationModelMode.Efficiency;
            const decimal testBudget = 70000m;
            const double testImpressions = 22000000;
            const decimal testOptimalCpm = 123.4m;
            const int testAudienceId = 31;
            const int testExpectedPlanVersionNumber = 11;
            const int testExpectedPlanVersionId = 666;
            var beforePlan = _GetNewPlan();
            beforePlan.PostingType = PostingTypeEnum.NTI;
            beforePlan.SpotAllocationModelMode = SpotAllocationModelMode.Quality;
            beforePlan.Budget = 50000;
            beforePlan.TargetImpressions = 1200000;
            beforePlan.TargetCPM = 555.6m;
            beforePlan.Vpvh = 12; // this is the user entered VPVH
            beforePlan.AudienceId = testAudienceId;
            beforePlan.Id = 23;
            beforePlan.VersionNumber = 2;
            beforePlan.VersionId = 24;
            beforePlan.PricingParameters.ProprietaryInventory = new List<InventoryProprietarySummary>();
            beforePlan.BuyingParameters = new PlanBuyingParametersDto();

            var afterPlanDto = new PlanDto { VersionId = 666 };

            _SpotLengthEngineMock.Setup(s => s.GetSpotLengths())
                .Returns(SpotLengthTestData.GetSpotLengthIdsByDuration());

            _PlanRepositoryMock.SetupSequence(s => s.GetPlan(It.IsAny<int>(), null))
                .Returns(beforePlan)
                .Returns(afterPlanDto);

            _PlanRepositoryMock.SetupSequence(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(beforePlan)
                .Returns(afterPlanDto);
            _PlanRepositoryMock
                .Setup(s => s.GetAllCustomDaypartOrganizations())
                .Returns(new List<CustomDaypartOrganizationDto>
                {
                     new CustomDaypartOrganizationDto
                     {
                        Id = 1,
                        OrganizationName = "NFL"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 2,
                        OrganizationName = "MLB"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 3,
                        OrganizationName = "MLS"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 4,
                        OrganizationName = "NBA"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 5,
                        OrganizationName = "PGA"
                     },
                      new CustomDaypartOrganizationDto
                     {
                        Id = 6,
                        OrganizationName = "PGA"
                     },
                });


            _WeeklyBreakdownEngineMock.Setup(s => s.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(It.IsAny<PlanDto>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            _WeeklyBreakdownEngineMock.Setup(s => s.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>(), It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(new List<WeeklyBreakdownByWeek>());

            _PlanPricingServiceMock.Setup(s => s.GetAllCurrentPricingExecutions(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new CurrentPricingExecutions
                {
                    Job = new PlanPricingJob(),
                    Results = new List<CurrentPricingExecutionResultDto>
                    {
                        new CurrentPricingExecutionResultDto
                        {
                            SpotAllocationModelMode = testSpotAllocationModelMode,
                            PostingType = testPostingType,
                            OptimalCpm = testOptimalCpm,
                            TotalBudget = testBudget,
                            TotalImpressions = testImpressions,
                        }
                    }
                });

            _PlanRepositoryMock.Setup(s => s.GetLatestVersionNumberForPlan(It.IsAny<int>()))
                .Returns(10);

            var savedPlans = new List<PlanDto>();
            _PlanRepositoryMock.Setup(s => s.SavePlan(It.IsAny<PlanDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<PlanDto, string, DateTime>((p, v, c) => savedPlans.Add(p));

            var calculateBudgetParams = new List<PlanDeliveryBudget>();
            _PlanBudgetDeliveryCalculatorMock.Setup(s => s.CalculateBudget(It.IsAny<PlanDeliveryBudget>()))
                .Callback<PlanDeliveryBudget>((s) => calculateBudgetParams.Add(s))
                .Returns<PlanDeliveryBudget>((b) => new PlanDeliveryBudget
                {
                    Impressions = testImpressions,
                    RatingPoints = 12,
                    CPP = 1522,
                    Universe = 8000000,
                    CPM = b.CPM ?? 8.3m,
                    Budget = b.Budget ?? 125000
                });

            var weeklyBreakdownRequests = new List<WeeklyBreakdownRequest>();
            _WeeklyBreakdownEngineMock
                .Setup(s => s.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Callback<WeeklyBreakdownRequest>(r => weeklyBreakdownRequests.Add(r))
                .Returns<WeeklyBreakdownRequest>(r => new WeeklyBreakdownResponseDto
                {
                    TotalActiveDays = 1,
                    TotalShareOfVoice = 2,
                    TotalImpressions = 3,
                    TotalRatingPoints = 4,
                    TotalImpressionsPercentage = 5,
                    TotalBudget = 6,
                    TotalUnits = 7,
                    Weeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { MediaWeekId = 27, WeeklyImpressions = testImpressions } }
                });

            _WeeklyBreakdownEngineMock.Setup(s =>
                    s.GroupWeeklyBreakdownByStandardDaypart(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByStandardDaypart>());

            _WeeklyBreakdownEngineMock.Setup(s =>
                    s.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(),
                        It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            _PlanAggregatorMock.Setup(s => s.Aggregate(It.IsAny<PlanDto>()))
                .Returns(new PlanSummaryDto());

            int pricingNewVersion = -1;
            _PlanRepositoryMock.Setup(s => s.UpdatePlanPricingVersionId(It.IsAny<int>(), It.IsAny<int>()))
                .Callback<int, int>((n, o) => pricingNewVersion = n);


            // Act
            var results = await _PlanService.CommitPricingAllocationModelAsync(beforePlan.Id, testSpotAllocationModelMode, testPostingType, username,
                aggregatePlanSynchronously: true);

            // Assert
            Assert.IsTrue(results);
            // Validate saved correct info
            Assert.AreEqual(1, savedPlans.Count);
            var savedPlan = savedPlans.First();
            Assert.AreEqual(testImpressions, savedPlan.TargetImpressions);
            Assert.AreEqual(testBudget, savedPlan.Budget);
            Assert.AreEqual(testOptimalCpm, savedPlan.TargetCPM);
            Assert.AreEqual(testSpotAllocationModelMode, savedPlan.SpotAllocationModelMode);
            Assert.AreEqual(testPostingType, savedPlan.PostingType);
            Assert.AreEqual(testExpectedPlanVersionNumber, savedPlan.VersionNumber);
            // Validate sent correct to the calculator
            Assert.AreEqual(2, calculateBudgetParams.Count);
            var sentCalculateBudgetParams = calculateBudgetParams.First();
            Assert.AreEqual(testAudienceId, sentCalculateBudgetParams.AudienceId);
            Assert.AreEqual(testBudget, sentCalculateBudgetParams.Budget);
            Assert.AreEqual(testOptimalCpm, sentCalculateBudgetParams.CPM);
            Assert.IsFalse(sentCalculateBudgetParams.Impressions.HasValue);
            // Validate sent correct info to WeeklyBreakdown distributor
            Assert.AreEqual(1, weeklyBreakdownRequests.Count);
            var sentWeeklyBreakdownRequest = weeklyBreakdownRequests.First();
            const string dateFormat = "yyyyMMdd";
            Assert.AreEqual(beforePlan.FlightStartDate.Value.ToString(dateFormat), sentWeeklyBreakdownRequest.FlightStartDate.ToString(dateFormat));
            Assert.AreEqual(beforePlan.FlightDays.Count, sentWeeklyBreakdownRequest.FlightDays.Count);
            Assert.AreEqual(beforePlan.FlightHiatusDays.Count, sentWeeklyBreakdownRequest.FlightHiatusDays.Count);
            Assert.AreEqual(beforePlan.GoalBreakdownType, sentWeeklyBreakdownRequest.DeliveryType);
            Assert.AreEqual(beforePlan.Budget.Value, sentWeeklyBreakdownRequest.TotalBudget);
            Assert.AreEqual(WeeklyBreakdownCalculationFrom.Percentage, sentWeeklyBreakdownRequest.WeeklyBreakdownCalculationFrom);
            Assert.AreEqual(beforePlan.WeeklyBreakdownWeeks.Count, sentWeeklyBreakdownRequest.Weeks.Count);
            Assert.AreEqual(beforePlan.CreativeLengths.Count, sentWeeklyBreakdownRequest.CreativeLengths.Count);
            Assert.AreEqual(beforePlan.Dayparts.Count, sentWeeklyBreakdownRequest.Dayparts.Count);
            Assert.AreEqual(beforePlan.ImpressionsPerUnit, sentWeeklyBreakdownRequest.ImpressionsPerUnit);
            Assert.AreEqual(beforePlan.Equivalized, sentWeeklyBreakdownRequest.Equivalized);

            var expectedRequestedTotalImpressions = beforePlan.TargetImpressions / 1000;
            Assert.AreEqual(expectedRequestedTotalImpressions, sentWeeklyBreakdownRequest.TotalImpressions);

            // Validate it tied in the results.
            Assert.AreEqual(testExpectedPlanVersionId, pricingNewVersion);
            _PlanRepositoryMock.Verify(s => s.UpdatePlanBuyingVersionId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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

        [Test]
        public void CalculatePlanWeeklyGoalBreakdown_WithClearAllFlag_CallsClearMethod()
        {
            //arrange
            var request = new WeeklyBreakdownRequest
            {
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 1,
                        Weight = 50
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3,
                        Weight = 50
                    },
                }
            };

            //act
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, true);

            //assert
            _WeeklyBreakdownEngineMock.Verify(x => x.ClearPlanWeeklyGoalBreakdown(request), Times.Once);
        }

        [Test]
        public void CalculatePlanWeeklyGoalBreakdown_CallsCalculateMethod()
        {
            //arrange
            var request = new WeeklyBreakdownRequest
            {
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 1,
                        Weight = 50
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3,
                        Weight = 50
                    },
                }
            };

            //act
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            //assert
            _WeeklyBreakdownEngineMock.Verify(x => x.CalculatePlanWeeklyGoalBreakdown(request), Times.Once);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CalculatePlanWeeklyGoalBreakdown_PopulateDefaultCreativeWeights(bool hasEmptyCreativeWeights)
        {
            //arrange
            var request = new WeeklyBreakdownRequest
            {
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 1,
                        Weight = hasEmptyCreativeWeights ? 100 : 50
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3,
                        Weight = hasEmptyCreativeWeights ? (int?)null : 50
                    },
                }
            };
            WeeklyBreakdownRequest calculatedRequest = null;
            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Callback<WeeklyBreakdownRequest>((r) => calculatedRequest = r)
                .Returns(new WeeklyBreakdownResponseDto());

            _CreativeLengthEngineMock.Setup(s => s.DistributeWeight(It.IsAny<List<CreativeLength>>()))
                .Returns(new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 1,
                        Weight = 50
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3,
                        Weight = 50
                    },
                });

            //act
            _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            //assert
            var hasCreativeLengthsWithoutWeights = calculatedRequest.CreativeLengths.Any(s => !s.Weight.HasValue);
            Assert.IsFalse(hasCreativeLengthsWithoutWeights);

            var timesShouldHaveDistributed = hasEmptyCreativeWeights ? Times.Once() : Times.Never();
            _CreativeLengthEngineMock.Verify(s => s.DistributeWeight(It.IsAny<List<CreativeLength>>()), timesShouldHaveDistributed);
        }

        [Test]
        public async Task SavePlan_WithLockedFlag()
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

            var plan = _GetPlanToSaveLockedFlag();
            var campaignId = plan.CampaignId;
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);

            var standardResult = new PlanAvailableMarketCalculationResult { AvailableMarkets = plan.AvailableMarkets, TotalWeight = 50 };
            _PlanMarketSovCalculator.Setup(s =>
                    s.CalculateMarketWeights(It.IsAny<List<PlanAvailableMarketDto>>()))
                .Returns(standardResult);

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

            _PlanRepositoryMock.Setup(s => s.GetPlanIdFromPricingJob(It.IsAny<int>()))
                .Returns(plan.Id);

            // Act
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: true);

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlan_v2_WithLockedFlag()
        {
            // Arrange
            var planToReturn = _GetPlanWithLockedFlag();

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
            _PlanRepositoryMock
                .Setup(s => s.GetAllCustomDaypartOrganizations())
                .Returns(new List<CustomDaypartOrganizationDto>
                {
                     new CustomDaypartOrganizationDto
                     {
                        Id = 1,
                        OrganizationName = "NFL"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 2,
                        OrganizationName = "MLB"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 3,
                        OrganizationName = "MLS"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 4,
                        OrganizationName = "NBA"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 5,
                        OrganizationName = "PGA"
                     },
                      new CustomDaypartOrganizationDto
                     {
                        Id = 6,
                        OrganizationName = "PGA"
                     },
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(
                    It.IsAny<IEnumerable<WeeklyBreakdownWeek>>(),
                    It.IsAny<double>(),
                    It.IsAny<List<CreativeLength>>(),
                    It.IsAny<bool>()))
                .Returns(new List<WeeklyBreakdownByWeek>()
                {
                new WeeklyBreakdownByWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    IsLocked = true
                },
                new WeeklyBreakdownByWeek
                {
                    ActiveDays = "M,Tu,W,Th,F",
                    NumberOfActiveDays = 5,
                    StartDate = new DateTime(2020, 5, 11),
                    EndDate = new DateTime(2020, 5, 17),
                    MediaWeekId = 401,
                    WeekNumber = 1,
                    IsLocked = false
                }
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(It.IsAny<PlanDto>()))
                .Returns(new List<WeeklyBreakdownWeek>()
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
                    WeeklyRatings = 15,
                    IsLocked = true
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
                    IsLocked = false
                }
                });

            // Act
            var result = _PlanService.GetPlan_v2(1, 1);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private static PlanDto _GetPlanWithLockedFlag()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Id = 295,
                VersionId = 1178,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                ProductMasterId = new Guid("C8C76C3B-8C39-42CF-9657-B7AD2B8BA320"),
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightNotesInternal = "Internal sample notes",
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
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 20, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn", IsUserShareOfVoicePercent = true},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York", IsUserShareOfVoicePercent = true}
                },
                AvailableMarketsSovTotal = 56.7,
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
                },

                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
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
                    WeeklyRatings = 15,
                    IsLocked = true
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
                    IsLocked = false
                }
                }
            };
        }

        [Test]
        public void LockPlan_ToggleOn()
        {
            // Arrange
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_LOCKING_CONSOLIDATION] = true;
            int planId = 291;
            bool expectedResult = true;
            _LockingEngineMock
             .Setup(s => s.LockPlan(It.IsAny<int>()))
           .Returns(new PlanLockResponse
           {
               PlanName = "plan - 2345_AfterChange",
               Key = "broadcast_plan : 298",
               Success = true,
               LockTimeoutInSeconds = 109,
               LockedUserId = null,
               LockedUserName = null,
               Error = null
           });
            var result = _PlanService.LockPlan(planId);
            // Assert
            Assert.AreEqual(expectedResult, result.Success);
        }

        [Test]
        public void UnlockPlan_ToggleOn()
        {
            // Arrange
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_LOCKING_CONSOLIDATION] = true;
            int planId = 291;
            bool expectedResult = true;
            _LockingEngineMock
             .Setup(s => s.UnlockPlan(It.IsAny<int>()))
           .Returns(new BroadcastReleaseLockResponse
           {
               Key = "broadcast_plan : 298",
               Success = true,
               Error = null
           });
            var result = _PlanService.UnlockPlan(planId);
            // Assert
            Assert.AreEqual(expectedResult, result.Success);
        }

        [Test]
        public void GetCustomDaypartOrganizations_Organizations_DoesNotExist()
        {
            // Arrange
            _PlanRepositoryMock
                .Setup(s => s.GetAllCustomDaypartOrganizations())
                .Returns(new List<CustomDaypartOrganizationDto>());

            // Act
            var result = _PlanService.GetCustomDaypartOrganizations();

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetCustomDaypartOrganizations_Organizations_Exist()
        {
            // Arrange
            _PlanRepositoryMock
                .Setup(s => s.GetAllCustomDaypartOrganizations())
                .Returns(new List<CustomDaypartOrganizationDto>
                {
                     new CustomDaypartOrganizationDto
                     {
                        Id = 1,
                        OrganizationName = "NFL"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 2,
                        OrganizationName = "MLB"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 3,
                        OrganizationName = "MLS"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 4,
                        OrganizationName = "NBA"
                     },
                     new CustomDaypartOrganizationDto
                     {
                        Id = 5,
                        OrganizationName = "PGA"
                     },
                      new CustomDaypartOrganizationDto
                     {
                        Id = 6,
                        OrganizationName = "PGA"
                     },
                });

            // Act
            var result = _PlanService.GetCustomDaypartOrganizations();

            // Assert          
            Assert.AreEqual(5, result.Count);

        }

        [Test]
        public void GetCustomDaypartOrganizations_ThrowsException()
        {
            // Arrange
            _PlanRepositoryMock
                .Setup(s => s.GetAllCustomDaypartOrganizations())
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _PlanService.GetCustomDaypartOrganizations());

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        [TestCase(0, 1, false, false, 0, 1)]
        [TestCase(1, 0, false, false, 0, 1)]
        [TestCase(1, 1, false, true, 2, 2)]
        [TestCase(1, 1, true, false, 1, 1)]
        [TestCase(1, 1, true, true, 2, 2)]
        [TestCase(1, 1, false, false, 1, 1)]
        public async Task SavePlan_FilterCustomDaypart_BeforeVerifyingPlanPricingInputsChange(int planId, int planVersionId, bool isDraftBefore, bool isDraftNow, int expectedBeforePlanDaypartCount, int expectedAfterPlanDaypartCount)
        {
            // Arrange
            var modifiedWho = "ModificationUser";
            var modifiedWhen = new DateTime(2019, 08, 12, 12, 31, 27);

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(new List<WeeklyBreakdownWeek>());

            var plan = _GetNewPlan();
            plan.Id = planId;
            plan.VersionId = planVersionId;
            plan.IsDraft = isDraftNow;

            // mock a before plan
            var beforePlan = _GetNewPlan();
            beforePlan.Id = plan.Id;
            beforePlan.VersionId = plan.VersionId;
            beforePlan.IsDraft = isDraftBefore;
            beforePlan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    PlanDaypartId = 668,
                    DaypartTypeId = DaypartTypeEnum.News,
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
                    PlanDaypartId = 669,
                    DaypartTypeId = DaypartTypeEnum.Sports,
                    DaypartCodeId = 24,
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
            };

            // mock an after plan
            var afterPlan = _GetNewPlan();
            afterPlan.Id = plan.Id;
            afterPlan.VersionId = plan.VersionId + 1;
            afterPlan.IsDraft = plan.IsDraft;
            afterPlan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    PlanDaypartId = 769,
                    DaypartTypeId = DaypartTypeEnum.News,
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
                    PlanDaypartId = 770,
                    DaypartTypeId = DaypartTypeEnum.Sports,
                    DaypartCodeId = 24,
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
            };

            var getPlanReturns = new Queue<PlanDto>();
            if (plan.Id > 0 && planVersionId > 0)
            {
                getPlanReturns.Enqueue(beforePlan);

                if ((beforePlan.IsDraft ?? false) && !(plan.IsDraft ?? false))
                {
                    getPlanReturns.Enqueue(beforePlan);
                }
            }
            else
            {
                beforePlan.Dayparts = new List<PlanDaypartDto>();
            }
            getPlanReturns.Enqueue(afterPlan);

            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(() => getPlanReturns.Dequeue());

            // Act
            await _PlanService.SavePlanAsync(plan, modifiedWho, modifiedWhen, aggregatePlanSynchronously: false);

            // Assert
            Assert.AreEqual(expectedBeforePlanDaypartCount, beforePlan.Dayparts.Count);
            Assert.AreEqual(expectedAfterPlanDaypartCount, afterPlan.Dayparts.Count);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DeletePlan(bool expectedResult)
        {
            // Arrange
            var planId = 1;
            var deletedBy = "Test User";

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    Status = PlanStatusEnum.Canceled
                });

            _PlanRepositoryMock
                .Setup(x => x.DeletePlan(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(expectedResult);

            _PlanIsciRepositoryMock
                .Setup(x => x.DeleteIsciPlanMappings(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(1);

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    HasPlans = true
                });

            // Act
            var result = _PlanService.DeletePlan(planId, deletedBy);

            // Assert
            if (!result)
            {
                _PlanIsciRepositoryMock.Verify(s => s.DeleteIsciPlanMappings(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
            }
            Assert.AreEqual(result, expectedResult);
        }

        [Test]
        public void DeletePlan_NonCanceledPlan_ThrowsException()
        {
            // Arrange
            var planId = 1;
            var deletedBy = "Test User";

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    Status = PlanStatusEnum.Working
                });

            // Act
            var result = Assert.Throws<ApplicationException>(() => _PlanService.DeletePlan(planId, deletedBy));

            // Assert
            Assert.AreEqual("Plan cannot be deleted. To delete plan, plan status must be canceled.", result.Message);
        }

        [Test]
        public void DeletePlan_ThrowsException()
        {
            // Arrange
            var planId = 1;
            var deletedBy = "Test User";

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    Status = PlanStatusEnum.Canceled
                });

            _PlanRepositoryMock
                .Setup(x => x.DeletePlan(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _PlanService.DeletePlan(planId, deletedBy));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetFluidityParentCategory()
        {
            _PlanRepositoryMock
                .Setup(x => x.GetFluidityParentCategory())
                .Returns(new List<FluidityCategoriesDto>()
                {
                    new FluidityCategoriesDto
                    {
                        Id = 2,
                        Category = "Arts & Entertainment"
                    },
                    new FluidityCategoriesDto
                    {
                        Id = 3,
                        Category = "Automotive"
                    }
                });
            // Act
            var result = _PlanService.GetFluidityParentCategory();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetFluidityChildCategory()
        {
            // Arrange
            int parentCategoryId = 2;
            _PlanRepositoryMock
                .Setup(x => x.GetFluidityChildCategory(It.IsAny<int>()))
                .Returns(new List<FluidityCategoriesDto>()
                {
                    new FluidityCategoriesDto
                    {
                        Id = 36,
                        Category = "Humor"
                    },
                    new FluidityCategoriesDto
                    {
                        Id = 37,
                        Category = "Movie"
                    }
                });
            // Act
            var result = _PlanService.GetFluidityChildCategory(parentCategoryId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(0)]
        [TestCase(0)]
        public void AutomaticStatusTransitions_WithToggleToKeepBuyingResults(int expectedCallCount)
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
            plansToReturn[1].Dayparts.ForEach(d => d.PlanDaypartId++);
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
            Assert.AreEqual(expectedCallCount, savePlanCalls.Count, "Invalid call count.");
            Assert.AreEqual(expectedCallCount, aggregateCallCount, "Invalid call count.");
            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(1, "automated status update"), Times.Exactly(expectedCallCount));
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void SearchPlanIf_Exist(bool enableUnifiedCampaign)
        {
            // Arrange
            int planId = 123;
            int campaignId = 234;

            _PlanRepositoryMock
                .Setup(s => s.SearchPlanByIdExceptUnifiedPlan(It.IsAny<int>()))
                .Returns(campaignId);

            _PlanRepositoryMock
                .Setup(s => s.SearchPlanByIdWithUnifiedPlan(It.IsAny<int>()))
                .Returns(campaignId);

            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_UNIFIED_CAMPAIGN] = enableUnifiedCampaign;

            // Act
            var result = _PlanService.SearchPlan(planId);

            // Assert
            Assert.AreEqual(campaignId, result.CampaignId);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void SearchPlanIf_NotExist(bool enableUnifiedCampaign)
        {
            // Arrange
            int planId = 123;
            int campaignId = 0;

            _PlanRepositoryMock
                .Setup(s => s.SearchPlanByIdExceptUnifiedPlan(It.IsAny<int>()))
                .Returns(campaignId);

            _PlanRepositoryMock
                .Setup(s => s.SearchPlanByIdWithUnifiedPlan(It.IsAny<int>()))
                .Returns(campaignId);

            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_UNIFIED_CAMPAIGN] = enableUnifiedCampaign;
            // Act           
            var result = _PlanService.SearchPlan(planId);

            // Assert
            Assert.AreEqual(null, result.CampaignId);
        }

        [Test]
        public void IsCurrentVersion_True()
        {
            // Arrange
            int planId = 347;
            int versionId = 3027;

            _PlanRepositoryMock
                .Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(
                new PlanDto()
                {
                    VersionId = 3027,
                    Id = 347
                }
                );
            // Act
            var result = _PlanService.IsCurrentVersion(planId, versionId);
            // Assert
            Assert.AreEqual(true, result);
        }
        [Test]
        public void IsCurrentVersion_False()
        {
            // Arrange
            int planId = 347;
            int versionId = 3026;
            string expectedMessage = "The current plan that you are viewing has been updated. Please close the plan and reopen to view the most current information.";
            _PlanRepositoryMock
                .Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(
                new PlanDto()
                {
                    VersionId = 3027,
                    Id = 347
                }
                );
            // Act
            var exception = Assert.Throws<CadentException>(() => _PlanService.IsCurrentVersion(planId, versionId));
            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);            
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ConvertImpressionsToRawFormat(bool isAduForPlanningv2Enabled)
        {
            // Arrange
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = isAduForPlanningv2Enabled;
            var expectedWeeklyAduResult = isAduForPlanningv2Enabled ? 6000 : 6;

            var plan = new PlanDto
            {
                TargetImpressions = 4,
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 5,
                        WeeklyAdu = 6
                    }
                }
            };

            // Act
            _PlanService._ConvertImpressionsToRawFormat(plan);

            // Assert
            Assert.AreEqual(4000, plan.TargetImpressions);

            plan.WeeklyBreakdownWeeks.ForEach(w =>
            {
                Assert.AreEqual(5000, w.WeeklyImpressions);
                Assert.AreEqual(expectedWeeklyAduResult, w.WeeklyAdu);
            });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ConvertImpressionsToUserFormat(bool isAduForPlanningv2Enabled)
        {
            // Arrange
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2] = isAduForPlanningv2Enabled;
            var expectedWeeklyAduResult = isAduForPlanningv2Enabled ? 6 : 6000;

            var plan = new PlanDto
            {
                TargetImpressions = 4686,
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek { WeeklyImpressions = 5000, WeeklyAdu = 6000 }
                },
                RawWeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek { WeeklyImpressions = 5000, WeeklyAdu = 6000 }
                },
                SecondaryAudiences = new List<PlanAudienceDto>
                {
                    new PlanAudienceDto { Impressions = 8000 }
                }
            };

            // Act
            _PlanService._ConvertImpressionsToUserFormat(plan);

            // Assert
            Assert.AreEqual(4, plan.TargetImpressions);
            plan.WeeklyBreakdownWeeks.ForEach(w =>
            {
                Assert.AreEqual(5, w.WeeklyImpressions);
                Assert.AreEqual(expectedWeeklyAduResult, w.WeeklyAdu);
            });
            plan.RawWeeklyBreakdownWeeks.ForEach(w =>
            {
                Assert.AreEqual(5, w.WeeklyImpressions);
                Assert.AreEqual(expectedWeeklyAduResult, w.WeeklyAdu);
            });
            plan.SecondaryAudiences.ForEach(a =>
            {
                Assert.AreEqual(8, a.Impressions);
            });
        }

        private List<WeeklyBreakdownWeek> _GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart()
        {
            return new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 150,
                    WeeklyBudget = 15,
                    MediaWeekId = 100,
                    SpotLengthId = 1,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 250,
                    WeeklyBudget = 15m,
                    MediaWeekId = 101,
                    SpotLengthId = 1,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 100,
                    WeeklyBudget = 15m,
                    MediaWeekId = 102,
                    SpotLengthId = 1,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 0,
                    WeeklyBudget = 15m,
                    MediaWeekId = 103,
                    SpotLengthId = 1,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 150,
                    WeeklyBudget = 15,
                    MediaWeekId = 100,
                    SpotLengthId = 1,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 250,
                    WeeklyBudget = 15m,
                    MediaWeekId = 101,
                    SpotLengthId = 1,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 100,
                    WeeklyBudget = 15m,
                    MediaWeekId = 102,
                    SpotLengthId = 1,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 0,
                    WeeklyBudget = 15m,
                    MediaWeekId = 103,
                    SpotLengthId = 1,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 150,
                    WeeklyBudget = 15,
                    MediaWeekId = 100,
                    SpotLengthId = 2,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 250,
                    WeeklyBudget = 15m,
                    MediaWeekId = 101,
                    SpotLengthId = 2,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 100,
                    WeeklyBudget = 15m,
                    MediaWeekId = 102,
                    SpotLengthId = 2,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 0,
                    WeeklyBudget = 15m,
                    MediaWeekId = 103,
                    SpotLengthId = 2,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 150,
                    WeeklyBudget = 15,
                    MediaWeekId = 100,
                    SpotLengthId = 2,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 250,
                    WeeklyBudget = 15m,
                    MediaWeekId = 101,
                    SpotLengthId = 2,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 100,
                    WeeklyBudget = 15m,
                    MediaWeekId = 102,
                    SpotLengthId = 2,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 0,
                    WeeklyBudget = 15m,
                    MediaWeekId = 103,
                    SpotLengthId = 2,
                    DaypartCodeId = 2
                }
            };
        }

        private static PlanDto _GetNewAduOnlyPlan()
        {
            return new PlanDto
            {
                IsAduEnabled = true,
                IsAduPlan = true,
                IsDraft = false,
                Budget = null,
                TargetCPM = null,
                TargetImpressions = null,
                TargetRatingPoints = 0,
                TargetCPP = 0,                
                ImpressionsPerUnit = 0,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                ProductMasterId = new Guid("C8C76C3B-8C39-42CF-9657-B7AD2B8BA320"),                
                CreativeLengths = new List<CreativeLength> { 
                    new CreativeLength { SpotLengthId = 1, Weight = 80 } ,
                    new CreativeLength { SpotLengthId = 2, Weight = 20 }
                },
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightNotesInternal = "Internal sample notes",
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 1, 20),
                    new DateTime(2019, 4, 15)
                },
                AudienceId = 31,        //HH
                AudienceType = AudienceTypeEnum.Nielsen,
                HUTBookId = 436,
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,                
                CoverageGoalPercent = 80.5,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 20, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn", IsUserShareOfVoicePercent = true },
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York", IsUserShareOfVoicePercent = true }
                },
                AvailableMarketsSovTotal = 56.7,
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto { MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUS = 5.5, Rank = 5, Market = "Burlington-Plattsburgh" },
                    new PlanBlackoutMarketDto { MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUS = 2.5, Rank = 8, Market = "Amarillo" },
                },

                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        PlanDaypartId = 56,
                        DaypartTypeId = DaypartTypeEnum.News,
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
                        PlanDaypartId = 58,
                        DaypartTypeId = DaypartTypeEnum.News,
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
                PricingParameters = new PlanPricingParametersDto
                {
                    AdjustedBudget = 0,
                    AdjustedCPM = 0,
                    CPM = 0,
                    Budget = 0,
                    Currency = PlanCurrenciesEnum.Impressions,
                    DeliveryImpressions = 0,
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
                },
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeeklyAdu = 4000
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyAdu = 4000
                    }
                }
            };
        }
    }
}