using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanPricingInventoryQuarterCalculatorEngineUnitTests
    {
        Mock<IQuarterCalculationEngine> _QuarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
        Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();

        /// <summary>
        /// Test the fallback date range derivation logic.
        /// </summary>
        /// <remarks>
        ///     Scenario : The active date range fits into the fallback quarter.
        ///     Expected :
        ///         - one date range is returned
        ///             - the dates match the active date range.
        /// </remarks>
        [Test]
        public void GetFallbackDateRangesWithinFallbackRange()
        {
            /*** Arrange ***/
            var activeDateRange = new DateRange(new DateTime(2020, 10, 12),
                new DateTime(2020, 10, 18));

            var expectedFallbackStartDate = new DateTime(2019, 10, 14);
            var expectedFallbackEndDate = new DateTime(2019, 10, 20);

            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 9, 28),
                EndDate = new DateTime(2020, 12, 27)
            };
            var fallBackQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2019,
                StartDate = new DateTime(2019, 9, 30),
                EndDate = new DateTime(2019, 12, 29)
            };

            var engine = _GetEngine();

            /*** Act ***/
            var result = engine.GetFallbackDateRanges(activeDateRange, planQuarter, fallBackQuarter);

            /*** Assert ***/
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expectedFallbackStartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), result[0].Start.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(expectedFallbackEndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), result[0].End.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
        }

        /// <summary>
        /// Test the fallback date range derivation logic.
        /// </summary>
        /// <remarks>
        ///     Scenario : The active date range ends two weeks beyond the fallback quarter end date.
        ///     Expected :
        ///         - three date ranges are returned
        ///             1) From the date range start date to the end of the fallback quarter.
        ///             2) The last week of the fallback quarter
        ///             3) The last week of the fallback quarter again
        /// </remarks>
        [Test]
        public void GetFallbackDateRangesBeyondFallbackQuarter()
        {
            /*** Arrange ***/
            var activeDateRange = new DateRange(new DateTime(2020, 10, 12),
                                                new DateTime(2021, 1, 10));

            var expectedResultDateRanges = new[]
            {
                    new DateRange(new DateTime(2019, 10, 14),
                                  new DateTime(2019, 12, 29)),
                    new DateRange(new DateTime(2019, 12, 23),
                                  new DateTime(2019, 12, 29)),
                    new DateRange(new DateTime(2019, 12, 23),
                                  new DateTime(2019, 12, 29))
                };

            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 9, 28),
                EndDate = new DateTime(2020, 12, 27)
            };
            var fallBackQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2019,
                StartDate = new DateTime(2019, 9, 30),
                EndDate = new DateTime(2019, 12, 29)
            };

            _MediaMonthAndWeekAggregateCache.Setup(s => s.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<DisplayMediaWeek> { new DisplayMediaWeek(), new DisplayMediaWeek() });

            var engine = _GetEngine();

            /*** Act ***/
            var result = engine.GetFallbackDateRanges(activeDateRange, planQuarter, fallBackQuarter);

            /*** Assert ***/
            Assert.AreEqual(3, result.Count);

            for (var i = 0; i < expectedResultDateRanges.Length; i++)
            {
                Assert.AreEqual(
                    expectedResultDateRanges[i].Start.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD),
                    result[i].Start.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
                Assert.AreEqual(expectedResultDateRanges[i].End.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD),
                    result[i].End.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            }
        }

        /// <summary>
        /// Test the fallback date range derivation logic.
        /// </summary>
        /// <remarks>
        ///     Scenario : The active date range ends beyond the fallback quarter end date and the end date is not a Sunday.
        ///     Expected :
        ///         - three date ranges are returned
        ///             1) From the date range start date to the end of the fallback quarter.
        ///             2) The last week of the fallback quarter
        ///             3) The last week of the fallback quarter with end date the same day of the week as the date range end date.
        /// </remarks>
        [Test]
        public void GetFallbackDateRangesOutsideFallbackRangeWithPartialEndWeek()
        {
            /*** Arrange ***/
            var activeDateRange = new DateRange(new DateTime(2020, 10, 12),
                                                new DateTime(2021, 1, 8));

            var expectedResultDateRanges = new[]
            {
                    new DateRange(new DateTime(2019, 10, 14),
                                  new DateTime(2019, 12, 29)),
                    new DateRange(new DateTime(2019, 12, 23),
                                  new DateTime(2019, 12, 29)),
                    new DateRange(new DateTime(2019, 12, 23),
                                  new DateTime(2019, 12, 27))
                };

            var planQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 9, 28),
                EndDate = new DateTime(2020, 12, 27)
            };
            var fallBackQuarter = new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2019,
                StartDate = new DateTime(2019, 9, 30),
                EndDate = new DateTime(2019, 12, 29)
            };

            _MediaMonthAndWeekAggregateCache.Setup(s => s.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<DisplayMediaWeek> { new DisplayMediaWeek(), new DisplayMediaWeek() });

            var engine = _GetEngine();

            /*** Act ***/
            var result = engine.GetFallbackDateRanges(activeDateRange, planQuarter, fallBackQuarter);

            /*** Assert ***/
            Assert.AreEqual(3, result.Count);

            for (var i = 0; i < expectedResultDateRanges.Length; i++)
            {
                Assert.AreEqual(
                    expectedResultDateRanges[i].Start.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD),
                    result[i].Start.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
                Assert.AreEqual(expectedResultDateRanges[i].End.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD),
                    result[i].End.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            }
        }

        /// <summary>
        /// Test we get the expected quarter from the plan.
        /// The plan Quarter is the determined by the start date of the plan's flight.
        /// </summary>
        /// <remarks>
        ///     Scenario : Plan is in two quarters Q3 and Q4 2020.
        ///     Expected : Q3 2020 is returned.
        /// </remarks>
        [Test]
        public void GetPlanQuarter()
        {
            /*** Arrange ***/
            var plan = new PlanDto
            {
                FlightStartDate = new DateTime(2020, 08, 12),
                FlightEndDate = new DateTime(2020, 10, 8)
            };

            var quartersData = _GetQuartersData();
            _QuarterCalculationEngine.Setup(s => s.GetQuarterRangeByDate(It.IsAny<DateTime>()))
                .Returns<DateTime>((d) => quartersData.First(s => d >= s.StartDate && d <= s.EndDate));

            var engine = _GetEngine();

            /*** Act ***/
            var result = engine.GetPlanQuarter(plan);

            /*** Assert ***/
            Assert.AreEqual(3, result.Quarter);
            Assert.AreEqual(2020, result.Year);
        }

        /// <summary>
        /// Test we get the expected fallback quarter for the 'current date';
        /// </summary>
        /// <remarks>
        ///     Scenario : Current date is Q1 2020.
        ///     Expected : Q4 2019 is returned.
        /// </remarks>
        [Test]
        public void GetInventoryFallbackQuarter()
        {
            /*** Arrange ***/
            var quartersData = _GetQuartersData();
            _QuarterCalculationEngine.Setup(s => s.GetQuarterRangeByDate(It.IsAny<DateTime>()))
                .Returns<DateTime>((d) => quartersData.First(s => d >= s.StartDate && d <= s.EndDate));

            var engine = _GetEngine();
            engine.UT_CurrentDateTime = new DateTime(2020, 1, 1);

            /*** Act ***/
            var result = engine.GetInventoryFallbackQuarter();

            /*** Assert ***/
            Assert.AreEqual(4, result.Quarter);
            Assert.AreEqual(2019, result.Year);
        }

        private PlanPricingInventoryQuarterCalculatorEngineTestClass _GetEngine()
        {
            var engine = new PlanPricingInventoryQuarterCalculatorEngineTestClass(_QuarterCalculationEngine.Object,
                _MediaMonthAndWeekAggregateCache.Object);

            return engine;
        }

        private List<QuarterDetailDto> _GetQuartersData()
        {
            var quarters = new List<QuarterDetailDto>();
            quarters.Add(new QuarterDetailDto
            {
                Quarter = 1,
                Year = 2020,
                StartDate = new DateTime(2019, 12, 30),
                EndDate = new DateTime(2020, 03, 29)
            });
            quarters.Add(new QuarterDetailDto
            {
                Quarter = 2,
                Year = 2020,
                StartDate = new DateTime(2020, 3, 30),
                EndDate = new DateTime(2020, 06, 28)
            });
            quarters.Add(new QuarterDetailDto
            {
                Quarter = 3,
                Year = 2020,
                StartDate = new DateTime(2020, 6, 29),
                EndDate = new DateTime(2020, 9, 27)
            });
            quarters.Add(new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2020,
                StartDate = new DateTime(2020, 9, 28),
                EndDate = new DateTime(2020, 12, 27)
            });

            quarters.Add(new QuarterDetailDto
            {
                Quarter = 1,
                Year = 2019,
                StartDate = new DateTime(2018, 12, 31),
                EndDate = new DateTime(2019, 03, 31)
            });
            quarters.Add(new QuarterDetailDto
            {
                Quarter = 2,
                Year = 2019,
                StartDate = new DateTime(2019, 4, 1),
                EndDate = new DateTime(2019, 06, 30)
            });
            quarters.Add(new QuarterDetailDto
            {
                Quarter = 3,
                Year = 2019,
                StartDate = new DateTime(2019, 7, 1),
                EndDate = new DateTime(2019, 9, 29)
            });
            quarters.Add(new QuarterDetailDto
            {
                Quarter = 4,
                Year = 2019,
                StartDate = new DateTime(2019, 9, 30),
                EndDate = new DateTime(2019, 12, 29)
            });

            return quarters;
        }
    }
}