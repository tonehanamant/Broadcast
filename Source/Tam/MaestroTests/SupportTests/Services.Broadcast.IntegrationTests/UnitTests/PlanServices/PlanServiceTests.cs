using System;
using System.Collections.Generic;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Validators;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.PlanServices
{
    [TestFixture]
    [Category("short_running")]
    public class PlanServiceTests
    {
        private readonly IPlanService planService;
        private readonly Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private readonly Mock<IPlanValidator> _PlanValidatorMock;
        private readonly Mock<IPlanBudgetDeliveryCalculator> _PlanBudgetDeliveryCalculatorMock;
        private readonly Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private readonly Mock<IPlanAggregator> _PlanAggregatorMock;
        private readonly Mock<ICampaignAggregationJobTrigger> _CampaignAggregationJobTriggerMock;
        private readonly Mock<INsiUniverseService> _NsiUniverseServiceMock;
        private readonly Mock<IBroadcastAudiencesCache> _BroadcastAudiencesCacheMock;
        private readonly Mock<ISpotLengthEngine> _SpotLengthEngineMock;
        private readonly Mock<IBroadcastLockingManagerApplicationService> _BroadcastLockingManagerApplicationServiceMock;
        private readonly Mock<IPlanPricingService> _PlanPricingServiceMock;
        private readonly Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock;
        private readonly Mock<IDaypartDefaultService> _DaypartDefaultServiceMock;

        public PlanServiceTests()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
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

            planService = new PlanService(
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
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_Even_Success_Test()
        {
            //Arrange
            var request = GetWeeklyBreakDownEvenRequest();
            var mockedListMediaWeeksByFlight = GetDisplayMediaWeeks_Even();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_Custom_Updated_Impressions_Success_Test()
        {
            //Arrange
            var updatedData =
                @", {""WeekNumber"":3,""MediaWeekId"":846,""StartDate"":""2020-03-09T00:00:00"",""EndDate"":""2020-03-15T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":3000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0}], ""WeeklyBreakdownCalculationFrom"": 1, ""UpdatedWeek"": 3, ""DeliveryType"":2";

            var request = GetWeeklyBreakDownRequest(updatedData);
            var mockedListMediaWeeksByFlight = GetDisplayMediaWeeks();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_Custom_Updated_Impressions_Percentage_Success_Test()
        {
            //Arrange
            var updatedData =
                @", {""WeekNumber"":3,""MediaWeekId"":846,""StartDate"":""2020-03-09T00:00:00"",""EndDate"":""2020-03-15T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":10,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0}], ""WeeklyBreakdownCalculationFrom"": 3, ""UpdatedWeek"": 3, ""DeliveryType"":2";

            var request = GetWeeklyBreakDownRequest(updatedData);
            
            var mockedListMediaWeeksByFlight = GetDisplayMediaWeeks();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePlanWeeklyGoalBreakdown_Custom_Updated_Ratings_Success_Test()
        {
            //Arrange
            var updatedData =
                @", {""WeekNumber"":3,""MediaWeekId"":846,""StartDate"":""2020-03-09T00:00:00"",""EndDate"":""2020-03-15T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":3.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0}], ""WeeklyBreakdownCalculationFrom"": 2, ""UpdatedWeek"": 3, ""DeliveryType"":2";
            var request = GetWeeklyBreakDownRequest(updatedData);
            var mockedListMediaWeeksByFlight = GetDisplayMediaWeeks();

            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(mockedListMediaWeeksByFlight);

            //Act
            var result = planService.CalculatePlanWeeklyGoalBreakdown(request);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private WeeklyBreakdownRequest GetWeeklyBreakDownRequest(
            WeeklyBreakdownCalculationFrom calculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
            int updatedWeek = 3)
        {
            var sampleRequest =
                @"{""FlightStartDate"":""2020-02-24T00:00:00"",""FlightEndDate"":""2020-03-29T00:00:00"",""FlightHiatusDays"":[],""TotalImpressions"":30000,""TotalRatings"":28.041170045861335,""TotalBudget"":300000,""DeliveryType"":2,""Weeks"":[{""WeekNumber"":1,""MediaWeekId"":844,""StartDate"":""2020-02-24T00:00:00"",""EndDate"":""2020-03-01T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0},{""WeekNumber"":2,""MediaWeekId"":845,""StartDate"":""2020-03-02T00:00:00"",""EndDate"":""2020-03-08T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0},{""WeekNumber"":3,""MediaWeekId"":846,""StartDate"":""2020-03-09T00:00:00"",""EndDate"":""2020-03-15T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":3000,""WeeklyImpressionsPercentage"":10,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0},{""WeekNumber"":4,""MediaWeekId"":847,""StartDate"":""2020-03-16T00:00:00"",""EndDate"":""2020-03-22T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0},{""WeekNumber"":5,""MediaWeekId"":848,""StartDate"":""2020-03-23T00:00:00"",""EndDate"":""2020-03-29T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0}]";

            var data = new StringBuilder(sampleRequest);
            data.Append(@", ""WeeklyBreakdownCalculationFrom"":");
            data.Append($"{(int)calculationFrom}");
            data.Append(@", ""UpdatedWeek"": ");
            data.Append($"{updatedWeek}");
            data.Append("}");

            return FromJson<WeeklyBreakdownRequest>(data.ToString());
        }

        private WeeklyBreakdownRequest GetWeeklyBreakDownRequest(
            string updatedData)
        {
            var sampleRequest =
                @"{""FlightStartDate"":""2020-02-24T00:00:00"",""FlightEndDate"":""2020-03-29T00:00:00"",""FlightHiatusDays"":[],""TotalImpressions"":30000,""TotalRatings"":28.041170045861335,""TotalBudget"":300000,""Weeks"":[{""WeekNumber"":1,""MediaWeekId"":844,""StartDate"":""2020-02-24T00:00:00"",""EndDate"":""2020-03-01T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0},{""WeekNumber"":2,""MediaWeekId"":845,""StartDate"":""2020-03-02T00:00:00"",""EndDate"":""2020-03-08T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0},{""WeekNumber"":4,""MediaWeekId"":847,""StartDate"":""2020-03-16T00:00:00"",""EndDate"":""2020-03-22T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0},{""WeekNumber"":5,""MediaWeekId"":848,""StartDate"":""2020-03-23T00:00:00"",""EndDate"":""2020-03-29T00:00:00"",""NumberOfActiveDays"":7,""ActiveDays"":""M-Su"",""WeeklyImpressions"":6000,""WeeklyImpressionsPercentage"":20,""WeeklyRatings"":5.608234009172268,""WeeklyBudget"":60000,""WeeklyAdu"":0}";

            var data = new StringBuilder(sampleRequest);
            data.Append($"{updatedData}");
            data.Append("}");

            var request = FromJson<WeeklyBreakdownRequest>(data.ToString());
            request.FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            return request;
        }

        private WeeklyBreakdownRequest GetWeeklyBreakDownEvenRequest()
        {
            var sampleRequest =
                @"{""FlightStartDate"":""2020-02-25T00:00:00"",""FlightEndDate"":""2020-03-15T00:00:00"",""FlightHiatusDays"":[],""TotalImpressions"":20,""TotalRatings"":0.00928868473742818,""TotalBudget"":400,""DeliveryType"":1,""Weeks"":[],""WeeklyBreakdownCalculationFrom"":1}";

            var data = new StringBuilder(sampleRequest);
            //data.Append($"{updatedData}");
            //data.Append("}");

            var request = FromJson<WeeklyBreakdownRequest>(data.ToString());
            request.FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            return request;
        }

        private List<DisplayMediaWeek> GetDisplayMediaWeeks()
        {
            var sampleData =
                @"[{""Id"":844,""Week"":1,""MediaMonthId"":462,""Year"":2020,""Month"":3,""WeekStartDate"":""2020-02-24T00:00:00"",""WeekEndDate"":""2020-03-01T00:00:00"",""MonthStartDate"":""2020-02-24T00:00:00"",""MonthEndDate"":""2020-03-29T00:00:00""},{""Id"":845,""Week"":2,""MediaMonthId"":462,""Year"":2020,""Month"":3,""WeekStartDate"":""2020-03-02T00:00:00"",""WeekEndDate"":""2020-03-08T00:00:00"",""MonthStartDate"":""2020-02-24T00:00:00"",""MonthEndDate"":""2020-03-29T00:00:00""},{""Id"":846,""Week"":3,""MediaMonthId"":462,""Year"":2020,""Month"":3,""WeekStartDate"":""2020-03-09T00:00:00"",""WeekEndDate"":""2020-03-15T00:00:00"",""MonthStartDate"":""2020-02-24T00:00:00"",""MonthEndDate"":""2020-03-29T00:00:00""},{""Id"":847,""Week"":4,""MediaMonthId"":462,""Year"":2020,""Month"":3,""WeekStartDate"":""2020-03-16T00:00:00"",""WeekEndDate"":""2020-03-22T00:00:00"",""MonthStartDate"":""2020-02-24T00:00:00"",""MonthEndDate"":""2020-03-29T00:00:00""},{""Id"":848,""Week"":5,""MediaMonthId"":462,""Year"":2020,""Month"":3,""WeekStartDate"":""2020-03-23T00:00:00"",""WeekEndDate"":""2020-03-29T00:00:00"",""MonthStartDate"":""2020-02-24T00:00:00"",""MonthEndDate"":""2020-03-29T00:00:00""}]";

            return FromJson<List<DisplayMediaWeek>>(sampleData);
        }

        private List<DisplayMediaWeek> GetDisplayMediaWeeks_Even()
        {
            var sampleData =
                @"[{""Id"":844,""Week"":1,""MediaMonthId"":462,""Year"":2020,""Month"":3,""WeekStartDate"":""2020-02-24T00:00:00"",""WeekEndDate"":""2020-03-01T00:00:00"",""MonthStartDate"":""2020-02-24T00:00:00"",""MonthEndDate"":""2020-03-29T00:00:00""},{""Id"":845,""Week"":2,""MediaMonthId"":462,""Year"":2020,""Month"":3,""WeekStartDate"":""2020-03-02T00:00:00"",""WeekEndDate"":""2020-03-08T00:00:00"",""MonthStartDate"":""2020-02-24T00:00:00"",""MonthEndDate"":""2020-03-29T00:00:00""},{""Id"":846,""Week"":3,""MediaMonthId"":462,""Year"":2020,""Month"":3,""WeekStartDate"":""2020-03-09T00:00:00"",""WeekEndDate"":""2020-03-15T00:00:00"",""MonthStartDate"":""2020-02-24T00:00:00"",""MonthEndDate"":""2020-03-29T00:00:00""}]";

            return FromJson<List<DisplayMediaWeek>>(sampleData);
        }

        //TODO: DELETE this, Move it to common library and Consume it.
        private T FromJson<T>(string data)
        {            
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
