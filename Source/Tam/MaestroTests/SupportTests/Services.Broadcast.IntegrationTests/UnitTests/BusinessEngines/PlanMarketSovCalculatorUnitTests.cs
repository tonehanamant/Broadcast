using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.IntegrationTests.TestData;
using System;
using System.Collections.Generic;
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
        public void CalculateMarketWeightChangeDecimalPlaceCheck()
        {
            // Arrange
            var availableMarkets = _GetPreparedAvailableMarkets();
            var modifiedMarketCode = availableMarkets[4].MarketCode;
            const double userEnteredValue = 12.333333333;

            var expectedMarketCount = availableMarkets.Count;
            const int expectedUserEnteredValueCount = 3;
            const double expectedUserEnteredSum = 25.133;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketWeightChange(availableMarkets, modifiedMarketCode, userEnteredValue);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));

            var userMarkets = result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).ToList();
            var totalUserEntered = userMarkets
                .Sum(m => m.ShareOfVoicePercent);
            Assert.AreEqual(expectedUserEnteredSum, totalUserEntered);
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
        public void CalculateMarketAdded_KeepUserEnteredValues()
        {
            // Arrange
            var beforeMarkets = _GetPreparedAvailableMarkets();
            var addedMarkets = new List<PlanAvailableMarketDto> { beforeMarkets[3], beforeMarkets[4], beforeMarkets[5] };
            beforeMarkets.RemoveRange(3, 6);

            // test the "if they cleared it out" use case
            addedMarkets[0].IsUserShareOfVoicePercent = true;
            addedMarkets[0].ShareOfVoicePercent = null;

            // test the "if they set a value" use case
            addedMarkets[1].IsUserShareOfVoicePercent = true;
            addedMarkets[1].ShareOfVoicePercent = 5;

            // test the "if they did not set a value" use case
            addedMarkets[2].IsUserShareOfVoicePercent = false;
            addedMarkets[2].ShareOfVoicePercent = 25;

            var addedMarketCodes = addedMarkets.Select(m => m.MarketCode);

            var expectedMarketCount = beforeMarkets.Count + addedMarkets.Count;
            const int expectedUserEnteredValueCount = 3;
            const double expectedUserEnteredSum = 17.8;
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
            Assert.AreEqual(addedMarkets.Count, result.AvailableMarkets.Count(s => addedMarketCodes.Contains(s.MarketCode)));
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
        public void CalculateMarketWeights()
        {
            // Arrange
            var markets = MarketsTestData.GetPlanAvailableMarkets();

            var expectedMarketCount = markets.Count;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketWeights(markets);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.IsFalse(result.AvailableMarkets.Any(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
        }

        [Test]
        public void CalculateMarketWeights_Bug2115()
        {
            // Arrange
            var marketCodes =new List<short> { 101,403,182,158,360,311} ;
            var markets = MarketsTestData.GetPlanAvailableMarkets()
                .Where(m => marketCodes.Contains(m.MarketCode))
                .ToList();
            markets.ForEach(m =>
            {
                switch (m.MarketCode)
                {
                    case 101:
                        m.ShareOfVoicePercent = 84.769;
                        m.IsUserShareOfVoicePercent = true;
                        break;
                    case 403:
                        m.ShareOfVoicePercent = 14.538;
                        break;
                    case 182:
                        m.ShareOfVoicePercent = 0.181;
                        break;
                    case 158:
                        m.ShareOfVoicePercent = 0.172;
                        break;
                    case 360:
                        m.ShareOfVoicePercent = 0.172;
                        break;
                    case 311:
                        m.ShareOfVoicePercent = 0.169;
                        break;
                }
            });

            var expectedMarketCount = markets.Count;
            const double expectedTotalWeight = 100.0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketWeights(markets);

            // Assert
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
        }

        [Test]
        public void CalculateMarketWeightsWhenExceedOneHundred()
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
            var results = testClass.CalculateMarketWeights(markets);

            // Assert
            Assert.AreEqual(expectedMarketCount, results.AvailableMarkets.Count);
            Assert.IsFalse(results.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, results.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, results.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, results.AvailableMarkets.Sum(m => m.ShareOfVoicePercent ?? 0));
            Assert.AreEqual(expectedNonUserEnteredSum, results.AvailableMarkets.Where(m => !m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent ?? 0));
            
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        /// <summary>
        /// Test mimics the file "Auto Distribution Weighted  - Final.xlsx"
        ///     attached to Jira ticket BP-1892
        /// </summary>
        [Test]
        public void CalculateMarketWeightsPerStoryAttachmentExample()
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
            var result = testClass.CalculateMarketWeights(markets);

            // Assert
            Assert.AreEqual(expectedMarketCount, result.AvailableMarkets.Count);
            Assert.IsFalse(result.AvailableMarkets.Any(m => !m.ShareOfVoicePercent.HasValue));
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(m => m.IsUserShareOfVoicePercent));
            Assert.AreEqual(expectedUserEnteredSum, result.AvailableMarkets.Where(m => m.IsUserShareOfVoicePercent).Sum(m => m.ShareOfVoicePercent));
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void CalculateMarketWeightsClearAll()
        {
            // Arrange
            var markets = _GetPreparedAvailableMarkets().Take(8).ToList();
            var expectedCount = markets.Count;
            const double expectedTotalWeight = 100.0;
            const int expectedUserEnteredValueCount = 0;

            var testClass = _GetTestClass();

            // Act
            var result = testClass.CalculateMarketWeightsClearAll(markets);

            // Assert
            Assert.AreEqual(expectedCount, result.AvailableMarkets.Count);
            Assert.AreEqual(expectedTotalWeight, result.TotalWeight);
            Assert.AreEqual(expectedUserEnteredValueCount, result.AvailableMarkets.Count(s => s.IsUserShareOfVoicePercent));
        }

        [Test]
        [TestCase(100.01, false)]
        [TestCase(100.00, true)]
        public void DoesMarketSovTotalExceedThreshold(double threshold, bool expectedOver)
        {
            // Arrange
            var availableMarketes = MarketsTestData.GetPlanAvailableMarkets().Take(10).ToList();
            availableMarketes.ForEach(m => m.ShareOfVoicePercent = 10);
            availableMarketes[0].ShareOfVoicePercent = 10.006;

            var testClass = _GetTestClass();

            // Act
            var exeedsThreshold = testClass.DoesMarketSovTotalExceedThreshold(availableMarketes, threshold);

            // Assert
            Assert.AreEqual(expectedOver, exeedsThreshold);
        }

        /// <summary>
        /// BP-4341 
        /// When adding the markets we ran into the "margin of error" for rounding.
        /// This test will exercise that use case and validate we are within the margin of error.
        /// </summary>
        [Test]
        public void DoesMarketSovTotalExceedThreshold_bp4341()
        {
            // Arrange
            var testClass = _GetTestClass();
            var planAvailableMarkets = new List<PlanAvailableMarketDto>();

            // 100.00 will exceed the threshold
            const double threshold = 100.01;

            foreach (var market in MarketsTestData.GetPlanAvailableMarkets())
            {
                var addedMarket = new List<PlanAvailableMarketDto> { market };
                var addMarketResult = testClass.CalculateMarketsAdded(planAvailableMarkets, addedMarket);
                planAvailableMarkets = addMarketResult.AvailableMarkets;

                // Act
                var exeedsThreshold = testClass.DoesMarketSovTotalExceedThreshold(planAvailableMarkets, threshold);

                // Assert
                Assert.IsFalse(exeedsThreshold, $"Market {market.MarketCode} caused the list to exceed the threshold");
            }
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