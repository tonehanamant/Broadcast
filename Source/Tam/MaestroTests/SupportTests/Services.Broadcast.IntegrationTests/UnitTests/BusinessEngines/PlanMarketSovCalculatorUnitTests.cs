using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.IntegrationTests.TestData;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

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
            var modifiedMarketCode = availableMarkets[4].MarketCode;
            const double userEnteredValue = 12.3;

            var expectedMarketCount = availableMarkets.Count;
            const int expectedUserEnteredValueCount = 3;
            const double expectedUserEnteredSum = 25.1;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketWeightChange(availableMarkets, modifiedMarketCode, userEnteredValue);

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
                    a.MarketCode,
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
            var modifiedMarketCode = modifiedMarket.MarketCode;
            double? userEnteredValue = null;

            var expectedMarketCount = availableMarkets.Count;
            const int expectedUserEnteredValueCount = 2;
            const double expectedUserEnteredSum = 12.8;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketWeightChange(availableMarkets, modifiedMarketCode, userEnteredValue);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            Assert.IsFalse(result.AvailableMarkets.Single(s => s.MarketCode == modifiedMarketCode).IsUserShareOfVoicePercent);

            var toValidate = result.AvailableMarkets.Select(a =>
                new
                {
                    a.Id,
                    a.MarketCode,
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
        public void CalculateMarketAdded_Single()
        {
            // Arrange
            var beforeMarkets = _GetPreparedAvailableMarkets();
            var addedMarkets = new List<PlanAvailableMarketDto> { beforeMarkets[3] };
            beforeMarkets.RemoveRange(3, 6);

            addedMarkets.ForEach(m =>
            {
                m.ShareOfVoicePercent = null;
                m.IsUserShareOfVoicePercent = false;
            });
            var addedMarketCodes = addedMarkets.Select(m => m.MarketCode);

            var expectedMarketCount = beforeMarkets.Count + addedMarkets.Count;
            const int expectedUserEnteredValueCount = 2;
            const double expectedUserEnteredSum = 12.8;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketsAdded(beforeMarkets, addedMarkets);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            Assert.IsTrue(result.AvailableMarkets.Any(s => addedMarketCodes.Contains(s.MarketCode)));
            Assert.IsFalse(result.AvailableMarkets.Any(s => addedMarketCodes.Contains(s.MarketCode) && s.IsUserShareOfVoicePercent == true));

            var toValidate = result.AvailableMarkets.Select(a =>
                new
                {
                    a.Id,
                    a.MarketCode,
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
        public void CalculateMarketAdded_Many()
        {
            // Arrange
            var beforeMarkets = _GetPreparedAvailableMarkets();
            var addedMarkets = new List<PlanAvailableMarketDto> { beforeMarkets[3], beforeMarkets[4] };
            beforeMarkets.RemoveRange(3, 6);

            addedMarkets.ForEach(m =>
            {
                m.ShareOfVoicePercent = null;
                m.IsUserShareOfVoicePercent = false;
            });
            var addedMarketCodes = addedMarkets.Select(m => m.MarketCode);

            var expectedMarketCount = beforeMarkets.Count + addedMarkets.Count;
            const int expectedUserEnteredValueCount = 2;
            const double expectedUserEnteredSum = 12.8;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketsAdded(beforeMarkets, addedMarkets);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            Assert.IsTrue(result.AvailableMarkets.Any(s => addedMarketCodes.Contains(s.MarketCode)));
            Assert.IsFalse(result.AvailableMarkets.Any(s => addedMarketCodes.Contains(s.MarketCode) && s.IsUserShareOfVoicePercent == true));

            var toValidate = result.AvailableMarkets.Select(a =>
                new
                {
                    a.Id,
                    a.MarketCode,
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
        public void CalculateMarketRemoved_Single()
        {
            // Arrange
            var beforeMarkets = _GetPreparedAvailableMarkets();
            var removedMarketCodes = new List<short>
            {
                beforeMarkets[0].MarketCode
            };

            var expectedMarketCount = beforeMarkets.Count - removedMarketCodes.Count;
            const int expectedUserEnteredValueCount = 1;
            const double expectedUserEnteredSum = 4.5;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketsRemoved(beforeMarkets, removedMarketCodes);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            Assert.IsFalse(result.AvailableMarkets.Any(a => removedMarketCodes.Contains(a.MarketCode)));

            var toValidate = result.AvailableMarkets.Select(a =>
                new
                {
                    a.Id,
                    a.MarketCode,
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
        public void CalculateMarketRemoved_Many()
        {
            // Arrange
            var beforeMarkets = _GetPreparedAvailableMarkets();
            var removedMarketCodes = new List<short>
            {
                beforeMarkets[0].MarketCode,
                beforeMarkets[3].MarketCode,
                beforeMarkets[4].MarketCode
            };

            var expectedMarketCount = beforeMarkets.Count - removedMarketCodes.Count;
            const int expectedUserEnteredValueCount = 1;
            const double expectedUserEnteredSum = 4.5;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketsRemoved(beforeMarkets, removedMarketCodes);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            Assert.IsFalse(result.AvailableMarkets.Any(a => removedMarketCodes.Contains(a.MarketCode)));

            var toValidate = result.AvailableMarkets.Select(a =>
                new
                {
                    a.Id,
                    a.MarketCode,
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