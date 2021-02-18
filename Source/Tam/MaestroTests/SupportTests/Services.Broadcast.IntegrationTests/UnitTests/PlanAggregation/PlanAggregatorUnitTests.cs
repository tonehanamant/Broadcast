using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.IntegrationTests.Stubs;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.IntegrationTests.UnitTests.PlanAggregation
{
    [TestFixture]
    [Category("short_running")]
    public class PlanAggregatorUnitTests
    {
        [SetUp]
        public void SetUp()
        {
            var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
            SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);
        }

        [Test]
        public void ConstructorTest()
        {
            var tc = GetEmptyTestClass();

            Assert.IsNotNull(tc);
        }

        #region Full Aggregations

        [Test]
        public void PerformAggregations()
        {
            var tc = GetTestClassFullySetup(out var quarterCalculationEngine);
            var plan = GetFullTestPlanDto();
            var summary = new PlanSummaryDto();

            tc.PerformAggregations(plan, summary);

            AssertFullSummaryResult(summary);
        }

        [Test]
        public void PerformAggregations_Timed()
        {
            var tc = GetTestClassFullySetup(out var quarterCalculationEngine);
            var plan = GetFullTestPlanDto();
            var summary1 = new PlanSummaryDto();

            var sw = new Stopwatch();
            sw.Start();
            tc.PerformAggregations(plan, summary1, false);
            sw.Stop();
            Debug.WriteLine($"Sequential Time is : {sw.ElapsedMilliseconds}");

            var summary2 = new PlanSummaryDto();
            var sw2 = new Stopwatch();
            sw2.Start();
            tc.PerformAggregations(plan, summary2, true);
            sw2.Stop();
            Debug.WriteLine($"Parallel Time is : {sw2.ElapsedMilliseconds}");

            AssertFullSummaryResult(summary2);
        }

        #endregion // #region Full Aggregations

        #region Individual Aggregations

        [Test]
        public void AggregateFlightAndHiatusDayCounts()
        {
            var tc = GetEmptyTestClass();
            var plan = new PlanDto
            {
                FlightStartDate = new DateTime(2019,01, 01),
                FlightEndDate = new DateTime(2019, 01, 20)
            };
            plan.FlightHiatusDays.Add(new DateTime(2019, 01, 10));
            plan.FlightHiatusDays.Add(new DateTime(2019, 01, 11));
            plan.FlightHiatusDays.Add(new DateTime(2019, 01, 13));
            var summary = new PlanSummaryDto();

            tc.AggregateFlightDays(plan, summary);

            Assert.AreEqual(3, summary.TotalHiatusDays, "Invalid summary.TotalHiatusDays");
            Assert.AreEqual(17, summary.TotalActiveDays, "Invalid summary.TotalActiveDays");
        }

        [Test]
        public void AggregateFlightAndHiatusDayCounts_WithoutValues()
        {
            var tc = GetEmptyTestClass();
            var plan = new PlanDto();
            var summary = new PlanSummaryDto();

            tc.AggregateFlightDays(plan, summary);

            Assert.IsFalse(summary.TotalHiatusDays.HasValue, "Invalid summary.TotalHiatusDays");
            Assert.IsFalse(summary.TotalActiveDays.HasValue, "Invalid summary.TotalActiveDays");
        }

        [Test]
        public void AggregateAvailableMarkets()
        {
            var tc = GetEmptyTestClass();
            var plan = new PlanDto();
            var selectedAvailableMarkets = GetTestSelectedAvailablePlanMarkets();
            plan.AvailableMarkets.AddRange(selectedAvailableMarkets);

            var summary = new PlanSummaryDto();

            tc.AggregateAvailableMarkets(plan, summary);

            Assert.AreEqual(TEST_SELECTED_AVAILABLE_MARKET_COUNT, summary.AvailableMarketCount, "Invalid summary.AvailableMarketCount");
            Assert.AreEqual(TEST_SELECTED_AVAILABLE_MARKET_WITH_SOV_COUNT, summary.AvailableMarketsWithSovCount, "Invalid summary.AvailableMarketsWithSovCount");
            Assert.AreEqual(TEST_SELECTED_AVAILABLE_MARKET_TOTAL_US_COVERAGE_PERCENT, summary.AvailableMarketTotalUsCoveragePercent, "Invalid summary.AvailableMarketTotalUsCoveragePercent");
        }

        [Test]
        public void AggregateAvailableMarkets_WithoutValues()
        {
            var tc = GetEmptyTestClass();
            var plan = new PlanDto();
            var summary = new PlanSummaryDto();

            tc.AggregateAvailableMarkets(plan, summary);

            Assert.IsFalse(summary.AvailableMarketCount.HasValue);
            Assert.IsFalse(summary.AvailableMarketTotalUsCoveragePercent.HasValue);
        }

        [Test]
        public void AggregateBlackoutMarkets()
        {
            var tc = GetEmptyTestClass();
            var plan = new PlanDto();
            var selectedBlackoutMarkets = GetTestSelectedBlackoutPlanMarkets();
            plan.BlackoutMarkets.AddRange(selectedBlackoutMarkets);
            var summary = new PlanSummaryDto();

            tc.AggregateBlackoutMarkets(plan, summary);

            Assert.AreEqual(TEST_SELECTED_BLACKOUT_MARKET_COUNT, summary.BlackoutMarketCount, "Invalid summary.BlackoutMarketCount");
            Assert.AreEqual(TEST_SELECTED_BLACKOUT_MARKET_TOTAL_US_COVERAGE_PERCENT, summary.BlackoutMarketTotalUsCoveragePercent, "Invalid summary.BlackoutMarketTotalUsCoveragePercent");
        }

        [Test]
        public void AggregateBlackoutMarkets_WithoutValues()
        {
            var tc = GetEmptyTestClass();
            var plan = new PlanDto();
            var summary = new PlanSummaryDto();

            tc.AggregateBlackoutMarkets(plan, summary);

            Assert.IsFalse(summary.BlackoutMarketCount.HasValue, "Invalid summary.BlackoutMarketCount");
            Assert.IsFalse(summary.BlackoutMarketTotalUsCoveragePercent.HasValue, "Invalid summary.BlackoutMarketTotalUsCoveragePercent");
        }

        [Test]
        public void AggregateQuarters_Single()
        {
            var testQuarters = new List<QuarterDetailDto>
            {
                new QuarterDetailDto { Quarter = 1, Year = 2019 }
            };

            var tc = GetTestClassSetupForQuarters(testQuarters, out var quarterCalculationEngine);
            var plan = new PlanDto
            {
                FlightStartDate = new DateTime(),
                FlightEndDate = new DateTime()
            };
            var summary = new PlanSummaryDto();

            tc.AggregateQuarters(plan, summary);

            quarterCalculationEngine.Verify(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()),Times.Once, "Invalid GetAllQuartersBetweenDatesCalledCount");
            Assert.AreEqual(1, summary.PlanSummaryQuarters.Count, "Invalid summary quarters count.");
            Assert.AreEqual(1, summary.PlanSummaryQuarters[0].Quarter, "Invalid quarter.");
            Assert.AreEqual(2019, summary.PlanSummaryQuarters[0].Year, "Invalid quarter year.");
        }

        [Test]
        public void AggregateQuarters_SameYear()
        {
            var testQuarters = new List<QuarterDetailDto>
            {
                new QuarterDetailDto { Quarter = 1, Year = 2019 },
                new QuarterDetailDto { Quarter = 2, Year = 2019 }
            };
            var tc = GetTestClassSetupForQuarters(testQuarters, out var quarterCalculationEngine);
            var plan = new PlanDto
            {
                FlightStartDate = new DateTime(),
                FlightEndDate = new DateTime()
            };
            var summary = new PlanSummaryDto();

            tc.AggregateQuarters(plan, summary);

            quarterCalculationEngine.Verify(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once, "Invalid GetAllQuartersBetweenDatesCalledCount");
            Assert.AreEqual(2, summary.PlanSummaryQuarters.Count, "Invalid summary quarters count.");
            Assert.AreEqual(1, summary.PlanSummaryQuarters[0].Quarter, "Invalid quarter.");
            Assert.AreEqual(2019, summary.PlanSummaryQuarters[0].Year, "Invalid quarter year.");
            Assert.AreEqual(2, summary.PlanSummaryQuarters[1].Quarter, "Invalid quarter.");
            Assert.AreEqual(2019, summary.PlanSummaryQuarters[1].Year, "Invalid quarter year.");
        }

        [Test]
        public void AggregateQuarters_MultiYear()
        {
            var testQuarters = new List<QuarterDetailDto>
            {
                new QuarterDetailDto { Quarter = 1, Year = 2019 },
                new QuarterDetailDto { Quarter = 2, Year = 2019 },
                new QuarterDetailDto { Quarter = 4, Year = 2018 },
            };
            var tc = GetTestClassSetupForQuarters(testQuarters, out var quarterCalculationEngine);
            var plan = new PlanDto
            {
                FlightStartDate = new DateTime(),
                FlightEndDate = new DateTime()
            };
            var summary = new PlanSummaryDto();

            tc.AggregateQuarters(plan, summary);

            quarterCalculationEngine.Verify(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once, "Invalid GetAllQuartersBetweenDatesCalledCount");
            Assert.AreEqual(3, summary.PlanSummaryQuarters.Count, "Invalid summary quarters count.");
            Assert.AreEqual(4, summary.PlanSummaryQuarters[0].Quarter, "Invalid quarter.");
            Assert.AreEqual(2018, summary.PlanSummaryQuarters[0].Year, "Invalid quarter year.");
            Assert.AreEqual(1, summary.PlanSummaryQuarters[1].Quarter, "Invalid quarter.");
            Assert.AreEqual(2019, summary.PlanSummaryQuarters[1].Year, "Invalid quarter year.");
            Assert.AreEqual(2, summary.PlanSummaryQuarters[2].Quarter, "Invalid quarter.");
            Assert.AreEqual(2019, summary.PlanSummaryQuarters[2].Year, "Invalid quarter year.");
        }

        [Test]
        public void AggregateQuarters_WithoutFlightStartDate()
        {
            var testQuarters = new List<QuarterDetailDto>();
            var tc = GetTestClassSetupForQuarters(testQuarters, out var quarterCalculationEngine);
            var plan = new PlanDto();
            var summary = new PlanSummaryDto();

            tc.AggregateQuarters(plan, summary);

            quarterCalculationEngine.Verify(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never, "Invalid GetAllQuartersBetweenDatesCalledCount");
            Assert.AreEqual(0, summary.PlanSummaryQuarters.Count, "Invalid summary quarters count.");
        }

        [Test]
        public void AggregateQuartersText_WithoutFlightEndDate()
        {
            var testQuarters = new List<QuarterDetailDto>();
            var tc = GetTestClassSetupForQuarters(testQuarters, out var quarterCalculationEngine);
            var plan = new PlanDto {FlightStartDate = new DateTime()};
            var summary = new PlanSummaryDto();

            tc.AggregateQuarters(plan, summary);

            quarterCalculationEngine.Verify(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never, "Invalid GetAllQuartersBetweenDatesCalledCount");
            Assert.AreEqual(0, summary.PlanSummaryQuarters.Count, "Invalid summary quarters count.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void AggregateProductName(bool isAabEnabled)
        {
            // Arrange
            const string productName = "Product2";
            var tc = GetTestClassSetupForProducts(isAabEnabled, out var aabEngine);
            var plan = new PlanDto { ProductId = 2, ProductMasterId = Guid.NewGuid()};
            var summary = new PlanSummaryDto();

            aabEngine.Setup(s => s.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new ProductDto { Name = productName });
            aabEngine.Setup(s => s.GetProduct(It.IsAny<int>()))
                .Returns(new ProductDto { Name = productName });

            // Act
            tc.AggregateProduct(plan, summary);

            // Assert
            Assert.AreEqual(productName, summary.ProductName, "Invalid ProductName");
            if (isAabEnabled)
            {
                aabEngine.Verify(s => s.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
                aabEngine.Verify(s => s.GetProduct(It.IsAny<int>()), Times.Never);
            }
            else
            {
                aabEngine.Verify(s => s.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
                aabEngine.Verify(s => s.GetProduct(It.IsAny<int>()), Times.Once);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void AggregateProductName_WithoutValue(bool isAabEnabled)
        {
            var tc = GetTestClassSetupForProducts(isAabEnabled, out var aabEngine);
            var plan = new PlanDto();
            var summary = new PlanSummaryDto();

            tc.AggregateProduct(plan, summary);

            Assert.IsNull(summary.ProductName, "Invalid ProductName");
        }

        #endregion // #region Individual Aggregations

        #region Helpers

        private PlanAggregator GetEmptyTestClass()
        {
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();

            var campaignRepository = new Mock<ICampaignRepository>();
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<ICampaignRepository>())
                .Returns(campaignRepository.Object);

            var aabEngine = new Mock<IAabEngine>();
            var featureToggleHelper = new Mock<IFeatureToggleHelper>();

            var tc = new PlanAggregator(
                broadcastDataRepositoryFactory.Object
                , quarterCalculationEngine.Object
                , aabEngine.Object
                , featureToggleHelper.Object
            );

            return tc;
        }

        private PlanAggregator GetTestClassFullySetup(out Mock<IQuarterCalculationEngine> quarterCalculationEngine)
        {
            quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();

            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();

            var campaignRepository = new Mock<ICampaignRepository>();
            campaignRepository.Setup(s => s.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto { AdvertiserMasterId = Guid.NewGuid() });
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<ICampaignRepository>())
                .Returns(campaignRepository.Object);

            var isAabEnabled = true;
            const string productName = "Product2";
            var aabEngine = new Mock<IAabEngine>();
            aabEngine.Setup(s => s.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new ProductDto { Name = productName });
            aabEngine.Setup(s => s.GetProduct(It.IsAny<int>()))
                .Returns(new ProductDto { Name = productName });

            var featureToggleHelper = new Mock<IFeatureToggleHelper>();
            featureToggleHelper.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION))
                .Returns(isAabEnabled);

            var tc = new PlanAggregator(
                broadcastDataRepositoryFactory.Object
                , quarterCalculationEngine.Object
                , aabEngine.Object
                , featureToggleHelper.Object
            );
            var getQuartersReturn = new List<QuarterDetailDto>
            {
                new QuarterDetailDto { Quarter = 2, Year = 2019 },
                new QuarterDetailDto { Quarter = 1, Year = 2019 },
                new QuarterDetailDto { Quarter = 4, Year = 2018 }
            };
            quarterCalculationEngine
                .Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(getQuartersReturn);

            return tc;
        }

        private PlanAggregator GetTestClassSetupForQuarters(List<QuarterDetailDto> getQuartersReturn, out Mock<IQuarterCalculationEngine> quarterCalculationEngine)
        {
            quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();

            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();

            var campaignRepository = new Mock<ICampaignRepository>();
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<ICampaignRepository>())
                .Returns(campaignRepository.Object);

            var aabEngine = new Mock<IAabEngine>();
            var featureToggleHelper = new Mock<IFeatureToggleHelper>();

            var tc = new PlanAggregator(
                broadcastDataRepositoryFactory.Object
                , quarterCalculationEngine.Object
                , aabEngine.Object
                , featureToggleHelper.Object
            );

            quarterCalculationEngine
                .Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(getQuartersReturn);

            return tc;
        }

        private PlanAggregator GetTestClassSetupForProducts(bool isAabEnabled, out Mock<IAabEngine> aabEngine)
        {
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();

            var campaignRepository = new Mock<ICampaignRepository>();
            campaignRepository.Setup(s => s.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto{ AdvertiserMasterId = Guid.NewGuid()});
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<ICampaignRepository>())
                .Returns(campaignRepository.Object);

            aabEngine = new Mock<IAabEngine>();

            var featureToggleHelper = new Mock<IFeatureToggleHelper>();
            featureToggleHelper.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION))
                .Returns(isAabEnabled);

            var tc = new PlanAggregator(
                broadcastDataRepositoryFactory.Object
                , quarterCalculationEngine.Object
                , aabEngine.Object
                , featureToggleHelper.Object
            );

            return tc;
        }

        private PlanDto GetFullTestPlanDto()
        {
            var plan = new PlanDto
            {
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto{DaypartCodeId = 1, StartTimeSeconds = 27, EndTimeSeconds = 3},
                    new PlanDaypartDto{DaypartCodeId = 3, StartTimeSeconds = 27, EndTimeSeconds = 3},
                    new PlanDaypartDto{DaypartCodeId = 2, StartTimeSeconds = 27, EndTimeSeconds = 3}
                },
                FlightStartDate = new DateTime(2019, 01, 01),
                FlightEndDate = new DateTime(2019, 01, 20)
            };
            plan.FlightHiatusDays.Add(new DateTime(2019, 01, 10));
            plan.FlightHiatusDays.Add(new DateTime(2019, 01, 11));
            plan.FlightHiatusDays.Add(new DateTime(2019, 01, 13));
            var selectedAvailableMarkets = GetTestSelectedAvailablePlanMarkets();
            plan.AvailableMarkets.AddRange(selectedAvailableMarkets);
            var selectedBlackoutMarkets = GetTestSelectedBlackoutPlanMarkets();
            plan.BlackoutMarkets.AddRange(selectedBlackoutMarkets);
            plan.AudienceId = 1;
            plan.ProductId = 2;
            plan.ProductMasterId = Guid.NewGuid();

            return plan;
        }

        private void AssertFullSummaryResult(PlanSummaryDto summary)
        {
            Assert.AreEqual(3, summary.TotalHiatusDays, "Invalid summary.TotalHiatusDays");
            Assert.AreEqual(17, summary.TotalActiveDays, "Invalid summary.TotalActiveDays");
            Assert.AreEqual(TEST_SELECTED_AVAILABLE_MARKET_COUNT, summary.AvailableMarketCount, "Invalid summary.AvailableMarketCount");
            Assert.AreEqual(TEST_SELECTED_AVAILABLE_MARKET_TOTAL_US_COVERAGE_PERCENT, summary.AvailableMarketTotalUsCoveragePercent, "Invalid summary.AvailableMarketTotalUsCoveragePercent");
            Assert.AreEqual(TEST_SELECTED_BLACKOUT_MARKET_COUNT, summary.BlackoutMarketCount, "Invalid summary.BlackoutMarketCount");
            Assert.AreEqual(TEST_SELECTED_BLACKOUT_MARKET_TOTAL_US_COVERAGE_PERCENT, summary.BlackoutMarketTotalUsCoveragePercent, "Invalid summary.BlackoutMarketTotalUsCoveragePercent");
            Assert.AreEqual("Product2", summary.ProductName, "Invalid ProductName");
            Assert.AreEqual(3, summary.PlanSummaryQuarters.Count, "Invalid summary quarters count.");
            Assert.AreEqual(4, summary.PlanSummaryQuarters[0].Quarter, "Invalid quarter.");
            Assert.AreEqual(2018, summary.PlanSummaryQuarters[0].Year, "Invalid quarter year.");
            Assert.AreEqual(1, summary.PlanSummaryQuarters[1].Quarter, "Invalid quarter.");
            Assert.AreEqual(2019, summary.PlanSummaryQuarters[1].Year, "Invalid quarter year.");
            Assert.AreEqual(2, summary.PlanSummaryQuarters[2].Quarter, "Invalid quarter.");
            Assert.AreEqual(2019, summary.PlanSummaryQuarters[2].Year, "Invalid quarter year.");
        }

        private const double TEST_SELECTED_AVAILABLE_MARKET_COUNT = 3;
        private const double TEST_SELECTED_AVAILABLE_MARKET_WITH_SOV_COUNT = 3;
        private const double TEST_SELECTED_AVAILABLE_MARKET_TOTAL_US_COVERAGE_PERCENT = 98.5;
        private List<PlanAvailableMarketDto> GetTestSelectedAvailablePlanMarkets()
        {
            var selectedMarkets = new List<PlanAvailableMarketDto>
            {
                new PlanAvailableMarketDto
                {
                    Id = 1,
                    MarketCode = 1,
                    MarketCoverageFileId = 1,
                    Rank = 1,
                    PercentageOfUS = 29.5,
                    ShareOfVoicePercent = 20.0
                },
                new PlanAvailableMarketDto
                {
                    Id = 2,
                    MarketCode = 2,
                    MarketCoverageFileId = 1,
                    Rank = 2,
                    PercentageOfUS = 30.5,
                    ShareOfVoicePercent = 29.5
                },
                new PlanAvailableMarketDto
                {
                    Id = 3,
                    MarketCode = 3,
                    MarketCoverageFileId = 1,
                    Rank = 3,
                    PercentageOfUS = 38.5,
                    ShareOfVoicePercent = 60.5
                }
            };

            return selectedMarkets;
        }

        private const double TEST_SELECTED_BLACKOUT_MARKET_COUNT = 3;
        private const double TEST_SELECTED_BLACKOUT_MARKET_TOTAL_US_COVERAGE_PERCENT = 22.2;
        private List<PlanBlackoutMarketDto> GetTestSelectedBlackoutPlanMarkets()
        {
            var selectedMarkets = new List<PlanBlackoutMarketDto>
            {
                new PlanBlackoutMarketDto
                {
                    Id = 1,
                    MarketCode = 1,
                    MarketCoverageFileId = 1,
                    Rank = 1,
                    PercentageOfUS = 11.0,
                    Market = "MarketOne"
                },
                new PlanBlackoutMarketDto
                {
                    Id = 2,
                    MarketCode = 2,
                    MarketCoverageFileId = 1,
                    Rank = 2,
                    PercentageOfUS = 11.0,
                    Market = "MarketTwo"
                },
                new PlanBlackoutMarketDto
                {
                    Id = 3,
                    MarketCode = 3,
                    MarketCoverageFileId = 1,
                    Rank = 3,
                    PercentageOfUS = 0.2,
                    Market = "MarketThree"
                }
            };

            return selectedMarkets;
        }

        private List<StandardDaypartFullDto> GetTestStandardDaypartFullDtos()
        {
            var result = new List<StandardDaypartFullDto>
            {
                new StandardDaypartFullDto
                {
                    Id = 1,
                    Code = "DP1",
                    FullName = "TestDaypart1",
                    DaypartType = DaypartTypeEnum.EntertainmentNonNews,
                    DefaultStartTimeSeconds = 12,
                    DefaultEndTimeSeconds = 12
                },
                new StandardDaypartFullDto
                {
                    Id = 2,
                    Code = "DP2",
                    FullName = "TestDaypart2",
                    DaypartType = DaypartTypeEnum.EntertainmentNonNews,
                    DefaultStartTimeSeconds = 12,
                    DefaultEndTimeSeconds = 12
                },
                new StandardDaypartFullDto
                {
                    Id = 3,
                    Code = "DP3",
                    FullName = "TestDaypart3",
                    DaypartType = DaypartTypeEnum.News,
                    DefaultStartTimeSeconds = 12,
                    DefaultEndTimeSeconds = 12
                }
            };

            return result;
        }

        #endregion // #region Helpers
    }
}