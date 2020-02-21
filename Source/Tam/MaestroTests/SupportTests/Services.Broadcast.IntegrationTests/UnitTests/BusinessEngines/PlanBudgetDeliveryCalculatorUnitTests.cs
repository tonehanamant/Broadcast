﻿using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    public class PlanBudgetDeliveryCalculatorUnitTests
    {
        private PlanBudgetDeliveryCalculator _PlanBudgetDeliveryCalculator;
        private Mock<INtiUniverseService> _NtiUniverseServiceMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private JsonSerializerSettings _JsonSettings;

        private const decimal BUDGET = 10000;
        private const double IMPRESSIONS = 100;
        private const decimal CPM = 100;
        private const decimal CPP = 2000;
        private const double RATING_POINTS = 5;

        [SetUp]
        public void Setup()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            _JsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            _NtiUniverseServiceMock = new Mock<INtiUniverseService>();
            _NtiUniverseServiceMock.Setup(n => n.GetLatestNtiUniverseByYear(It.IsAny<int>(), It.IsAny<int>())).Returns(100000);

            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _MediaMonthAndWeekAggregateCacheMock.Setup(m => m.GetMediaMonthById(It.IsAny<int>())).Returns(new Tam.Maestro.Data.Entities.MediaMonth { Year = 2019 });

            _PlanBudgetDeliveryCalculator = new PlanBudgetDeliveryCalculator(_NtiUniverseServiceMock.Object, _MediaMonthAndWeekAggregateCacheMock.Object);
        }

        [Test]
        public void CalculateBudget_InvalidImpressions()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.Impressions = -10;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid budget values passed"));
        }

        [Test]
        public void CalculateBudget_InvalidRatingPoints()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.RatingPoints = -10;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid budget values passed"));
        }

        [Test]
        public void CalculateBudget_InvalidCpp()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.CPP = -10;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid budget values passed"));
        }

        [Test]
        public void CalculateBudget_InvalidCpm()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.CPM = -10;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid budget values passed"));
        }

        [Test]
        public void CalculateBudget_InvalidBudget()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.CPM = -10;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Invalid budget values passed"));
        }

        [Test]
        public void CalculateBudget_InvalidMediaMonthId()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.MediaMonthId = -10;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Cannot calculate goal without media month and audience"));
        }

        [Test]
        public void CalculateBudget_InvalidAudienceId()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.AudienceId = -10;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("Cannot calculate goal without media month and audience"));
        }

        [Test]
        public void CalculateBudget_InvalidParametersCombination_CppAndCpm()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.CPP = CPP;
            planDeliveryBudget.CPP = CPM;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("At least 2 values needed to calculate goal amount"));
        }

        [Test]
        public void CalculateBudget_InvalidParametersCombination_RattingPointsAndCpm()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.RatingPoints = RATING_POINTS;
            planDeliveryBudget.CPM = CPM;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("At least 2 values needed to calculate goal amount"));
        }

        [Test]
        public void CalculateBudget_InvalidParametersCombination_RattingPointsAndImpressions()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.RatingPoints = RATING_POINTS;
            planDeliveryBudget.Impressions = IMPRESSIONS;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("At least 2 values needed to calculate goal amount"));
        }

        [Test]
        public void CalculateBudget_InvalidParametersCombination_CppAndImpressions()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.CPP = CPP;
            planDeliveryBudget.Impressions = IMPRESSIONS;

            Assert.That(() => _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget),
                Throws.TypeOf<Exception>().With.Message
                    .EqualTo("At least 2 values needed to calculate goal amount"));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateBudget_ValidParametersCombination_BudgetAndImpressions()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithImpressionsAndBudget();

            var response = _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget);
            
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, _JsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateBudget_ValidParametersCombination_BudgetAndCPM()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithBudgetAndCPM();

            var response = _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, _JsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateBudget_ValidParametersCombination_BudgetAndRatingPoints()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithBudgetAndRatingPoints();

            var response = _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, _JsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateBudget_ValidParametersCombination_BudgetAndCPP()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithCPPAndBudget();

            var response = _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, _JsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateBudget_ValidParametersCombination_RatingPointsAndCPP()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithCPPAndRatingPoints();

            var response = _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, _JsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateBudget_ValidParametersCombination_ImpressionsAndCPM()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithImpressionsAndCPM();

            var response = _PlanBudgetDeliveryCalculator.CalculateBudget(planDeliveryBudget);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, _JsonSettings));
        }

        private PlanDeliveryBudget _GetPlanDeliveryBudgetWithAudienceAndMediaMonth()
        {
            return new PlanDeliveryBudget
            {
                AudienceId = 31,
                MediaMonthId = 30,
            };
        }

        private PlanDeliveryBudget _GetPlanDeliveryBudgetWithImpressionsAndBudget()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.Budget = BUDGET;
            planDeliveryBudget.Impressions = IMPRESSIONS;
            return planDeliveryBudget;
        }

        private PlanDeliveryBudget _GetPlanDeliveryBudgetWithImpressionsAndCPM()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.CPM = CPM;
            planDeliveryBudget.Impressions = IMPRESSIONS;
            return planDeliveryBudget;
        }

        private PlanDeliveryBudget _GetPlanDeliveryBudgetWithBudgetAndCPM()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.CPM = CPM;
            planDeliveryBudget.Budget = BUDGET;
            return planDeliveryBudget;
        }

        private PlanDeliveryBudget _GetPlanDeliveryBudgetWithBudgetAndRatingPoints()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.Budget = BUDGET;
            planDeliveryBudget.RatingPoints = RATING_POINTS;
            return planDeliveryBudget;
        }

        private PlanDeliveryBudget _GetPlanDeliveryBudgetWithCPPAndRatingPoints()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.CPP = CPP;
            planDeliveryBudget.RatingPoints = RATING_POINTS;
            return planDeliveryBudget;
        }

        private PlanDeliveryBudget _GetPlanDeliveryBudgetWithCPPAndBudget()
        {
            var planDeliveryBudget = _GetPlanDeliveryBudgetWithAudienceAndMediaMonth();
            planDeliveryBudget.CPP = CPP;
            planDeliveryBudget.Budget = BUDGET;
            return planDeliveryBudget;
        }

    }
}
