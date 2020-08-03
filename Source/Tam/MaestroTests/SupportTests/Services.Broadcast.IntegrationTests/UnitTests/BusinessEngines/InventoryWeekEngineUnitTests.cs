using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using System;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class InventoryWeekEngineUnitTests
    {
        private InventoryWeekEngine _InventoryWeekEngine;

        [SetUp]
        public void Setup()
        {
            _InventoryWeekEngine = new InventoryWeekEngine();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MediaWeekSame()
        {//GetDateRangeInventoryIsAvailableForForWeek_MediaWeekSameAsEffectiveAndEndDate
            var effectiveDate = new DateTime(2020, 03, 01);
            var endDate = new DateTime(2020,03,31);
            var mediaWeek = new MediaWeek { StartDate = new DateTime(2020, 03, 01), EndDate = new DateTime(2020, 03, 31) };

            var result = _InventoryWeekEngine.GetDateRangeInventoryIsAvailableForForWeek(mediaWeek, effectiveDate, endDate);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MediaWeekSmaller()
        {//GetDateRangeInventoryIsAvailableForForWeek_MediaWeekSmallerThanEffectiveAndEndDate
            var effectiveDate = new DateTime(2020, 03, 01);
            var endDate = new DateTime(2020, 03, 31);
            var mediaWeek = new MediaWeek { StartDate = new DateTime(2020, 03, 05), EndDate = new DateTime(2020, 03, 25) };

            var result = _InventoryWeekEngine.GetDateRangeInventoryIsAvailableForForWeek(mediaWeek, effectiveDate, endDate);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void EffectiveDateBiggerThanMediaWeek()
        {//GetDateRangeInventoryIsAvailableForForWeek_EffectiveDateBiggerThanMediaWeek
            var effectiveDate = new DateTime(2020, 03, 5);
            var endDate = new DateTime(2020, 03, 31);
            var mediaWeek = new MediaWeek { StartDate = new DateTime(2020, 03, 01), EndDate = new DateTime(2020, 03, 31) };

            var result = _InventoryWeekEngine.GetDateRangeInventoryIsAvailableForForWeek(mediaWeek, effectiveDate, endDate);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void EndDateSmallerThanMediaWeek()
        {//GetDateRangeInventoryIsAvailableForForWeek_EndDateSmallerThanMediaWeek
            var effectiveDate = new DateTime(2020, 03, 1);
            var endDate = new DateTime(2020, 03, 25);
            var mediaWeek = new MediaWeek { StartDate = new DateTime(2020, 03, 01), EndDate = new DateTime(2020, 03, 31) };

            var result = _InventoryWeekEngine.GetDateRangeInventoryIsAvailableForForWeek(mediaWeek, effectiveDate, endDate);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void EndDateBiggerThanMediaWeek()
        {//GetDateRangeInventoryIsAvailableForForWeek_EndDateBiggerThanMediaWeek
            var effectiveDate = new DateTime(2020, 03, 05);
            var endDate = new DateTime(2020, 04, 25);
            var mediaWeek = new MediaWeek { StartDate = new DateTime(2020, 03, 01), EndDate = new DateTime(2020, 03, 31) };

            var result = _InventoryWeekEngine.GetDateRangeInventoryIsAvailableForForWeek(mediaWeek, effectiveDate, endDate);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}
