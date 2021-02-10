using System;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.IntegrationTests.TestData;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PlanMarketSovCalculatorUnitTests
    {
        [Test]
        public void CalculateMarketWeightChange()
        {
            // Arrange
            var availableMarkets = _GetPreparedAvailableMarkets();
            const int modifiedMarketId = 5;
            const double userEnteredValue = 12.3;

            var expectedMarketCount = availableMarkets.Count;
            const int expectedUserEnteredValueCount = 3;
            const double expectedUserEnteredSum = 25.1;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketWeightChange(availableMarkets, modifiedMarketId, userEnteredValue);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);

            var toValidate = result.AvailableMarkets.Select(a =>
                new
                {
                    a.Id,
                    a.Rank,
                    a.PercentageOfUS,
                    Before = new 
                    {
                        availableMarkets.Single(s => s.Id == a.Id).IsUserShareOfVoicePercent,
                        availableMarkets.Single(s => s.Id == a.Id).ShareOfVoicePercent
                    },
                    After = new
                    {
                        a.IsUserShareOfVoicePercent,
                        a.ShareOfVoicePercent
                    }
                }).ToList();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate));
        }

        [Test]
        public void CalculateMarketWeightChangeWhenClearValue()
        {
            // Arrange
            var availableMarkets = _GetPreparedAvailableMarkets();

            var modifiedMarket = availableMarkets[2];
            modifiedMarket.ShareOfVoicePercent = 7.8;
            modifiedMarket.IsUserShareOfVoicePercent = true;
            var modifiedMarketId = modifiedMarket.Id;
            double? userEnteredValue = null;

            var expectedMarketCount = availableMarkets.Count;
            const int expectedUserEnteredValueCount = 2;
            const double expectedUserEnteredSum = 12.8;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketWeightChange(availableMarkets, modifiedMarketId, userEnteredValue);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            Assert.IsFalse(result.AvailableMarkets.Single(s => s.Id == modifiedMarketId).IsUserShareOfVoicePercent);

            var toValidate = result.AvailableMarkets.Select(a =>
                new
                {
                    a.Id,
                    a.Rank,
                    a.PercentageOfUS,
                    Before = new
                    {
                        availableMarkets.Single(s => s.Id == a.Id).IsUserShareOfVoicePercent,
                        availableMarkets.Single(s => s.Id == a.Id).ShareOfVoicePercent
                    },
                    After = new
                    {
                        a.IsUserShareOfVoicePercent,
                        a.ShareOfVoicePercent
                    }
                }).ToList();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate));
        }

        [Test]
        public void CalculateMarketAdded()
        {
            // Arrange
            var beforeMarkets = _GetPreparedAvailableMarkets();
            var addedMarket = beforeMarkets[3];
            beforeMarkets.RemoveRange(3, 5);

            addedMarket.ShareOfVoicePercent = null;
            addedMarket.IsUserShareOfVoicePercent = false;
            var addedMarketId = addedMarket.Id;

            var expectedMarketCount = beforeMarkets.Count + 1;
            const int expectedUserEnteredValueCount = 2;
            const double expectedUserEnteredSum = 12.8;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketAdded(beforeMarkets, addedMarket);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            Assert.IsTrue(result.AvailableMarkets.Any(s => s.Id == addedMarketId));
            Assert.IsFalse(result.AvailableMarkets.Single(s => s.Id == addedMarketId).IsUserShareOfVoicePercent);

            var toValidate = result.AvailableMarkets.Select(a =>
                new
                {
                    a.Id,
                    a.Rank,
                    a.PercentageOfUS,
                    Before = new
                    {
                        beforeMarkets.SingleOrDefault(s => s.Id == a.Id)?.IsUserShareOfVoicePercent,
                        beforeMarkets.SingleOrDefault(s => s.Id == a.Id)?.ShareOfVoicePercent
                    },
                    After = new
                    {
                        a.IsUserShareOfVoicePercent,
                        a.ShareOfVoicePercent
                    }
                }).ToList();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate));
        }

        [Test]
        public void CalculateMarketRemoved()
        {
            // Arrange
            var beforeMarkets = _GetPreparedAvailableMarkets();
            var removedMarketId = beforeMarkets[0].Id;

            var expectedMarketCount = beforeMarkets.Count - 1;
            const int expectedUserEnteredValueCount = 1;
            const double expectedUserEnteredSum = 4.5;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketRemoved(beforeMarkets, removedMarketId);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            Assert.IsFalse(result.AvailableMarkets.Any(a => a.Id == removedMarketId));

            var toValidate = result.AvailableMarkets.Select(a =>
                new
                {
                    a.Id,
                    a.Rank,
                    a.PercentageOfUS,
                    Before = new
                    {
                        beforeMarkets.SingleOrDefault(s => s.Id == a.Id)?.IsUserShareOfVoicePercent,
                        beforeMarkets.SingleOrDefault(s => s.Id == a.Id)?.ShareOfVoicePercent
                    },
                    After = new
                    {
                        a.IsUserShareOfVoicePercent,
                        a.ShareOfVoicePercent
                    }
                }).ToList();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate));
        }

        [Test]
        public void BalanceMarketWeights()
        {
            // Arrange
            var markets = _GetPreparedAvailableMarkets();

            var expectedMarketCount = markets.Count;
            const int expectedUserEnteredValueCount = 2;
            const double expectedUserEnteredSum = 12.8;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            testClass._BalanceMarketWeights(markets);

            // Assert
            Assert.AreEqual(expectedMarketCount, markets.Count);
            Assert.IsFalse(markets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, markets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, markets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            var totalWeight = _GetTotalWeight(markets);
            Assert.AreEqual(expectedTotalWeight, totalWeight);
            
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets));
        }

        [Test]
        public void BalanceMarketWeightsWhenExceedOneHundred()
        {
            // Arrange
            var markets = _GetPreparedAvailableMarkets();
            markets[5].ShareOfVoicePercent = 101;
            markets[5].IsUserShareOfVoicePercent = true;

            var expectedMarketCount = markets.Count;
            const int expectedUserEnteredValueCount = 3;
            const double expectedUserEnteredSum = 113.8;
            const double expectedTotalWeight = 113.8;
            const double expectedNonUserEnteredSum = 0.0;

            var testClass = _GetTestClass();

            // Act
            testClass._BalanceMarketWeights(markets);

            // Assert
            Assert.AreEqual(expectedMarketCount, markets.Count);
            Assert.IsFalse(markets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, markets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, markets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, markets.Sum(m => m.ShareOfVoicePercent ?? 0));
            Assert.AreEqual(expectedNonUserEnteredSum, markets.Where(m => !m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent ?? 0));
            
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets));
        }

        /// <summary>
        /// Test mimics the file "Auto Distribution Weighted  - Final.xlsx"
        ///     attached to Jira ticket BP-1892
        /// </summary>
        [Test]
        public void BalanceMarketWeightsPerStoryAttachmentExample()
        {
            // Arrange
            var markets = _GetPreparedAvailableMarkets().Take(8).ToList();
            // set these up per the example spreadsheet attached to the story.
            // markets
            markets[0].PercentageOfUS = 6.309;
            markets[1].PercentageOfUS = 4.743;
            markets[2].PercentageOfUS = 2.942;
            markets[3].PercentageOfUS = 2.559;
            markets[4].PercentageOfUS = 2.222;
            markets[5].PercentageOfUS = 1.496;
            markets[6].PercentageOfUS = 0.063;
            markets[7].PercentageOfUS = 0.004;
            // user entered values
            markets.ForEach(m => m.IsUserShareOfVoicePercent = false);
            markets[0].ShareOfVoicePercent = 40.0;
            markets[0].IsUserShareOfVoicePercent = true;

            var expectedMarketCount = markets.Count;
            const int expectedUserEnteredValueCount = 1;
            const double expectedUserEnteredSum = 40.0;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            testClass._BalanceMarketWeights(markets);

            // Assert
            Assert.AreEqual(expectedMarketCount, markets.Count);
            Assert.IsFalse(markets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, markets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, markets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            var totalWeight = _GetTotalWeight(markets);
            Assert.AreEqual(expectedTotalWeight, totalWeight);
            
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets));
        }

        private double _GetTotalWeight(List<PlanAvailableMarketDto> markets)
        {
            return Math.Round(markets.Sum(m => m.ShareOfVoicePercent ?? 0), 3);
        }

        private PlanMarketSovCalculator _GetTestClass()
        {
            var testClass = new PlanMarketSovCalculator();
            return testClass;
        }

        private List<PlanAvailableMarketDto> _GetPreparedAvailableMarkets()
        {
            var availableMarkets = MarketsTestData.GetPlanAvailableMarkets();
            // set us up in the middle
            availableMarkets[0].ShareOfVoicePercent = 8.3;
            availableMarkets[0].IsUserShareOfVoicePercent = true;
            availableMarkets[1].ShareOfVoicePercent = 4.5;
            availableMarkets[1].IsUserShareOfVoicePercent = true;
            return availableMarkets;
        }
    }
}