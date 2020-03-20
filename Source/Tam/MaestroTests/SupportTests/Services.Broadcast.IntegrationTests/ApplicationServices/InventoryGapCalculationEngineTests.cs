using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [Category("short_running")]
    public class InventoryGapCalculationEngineTests
    {
        private readonly IInventoryGapCalculationEngine _InventoryGapCalculationEngine;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;

        public InventoryGapCalculationEngineTests()
        {
            _InventoryGapCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryGapCalculationEngine>();
            _QuarterCalculationEngine = new QuarterCalculationEngine(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory, new MediaMonthAndWeekAggregateCache(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryGapsTest()
        {
            // All media weeks for Q1 2018.
            var manifestWeeks = _GenerateManifestMediaWeeks(732, 743);
            // First week of Q2 2018.
            manifestWeeks.Add(744);
            var quarterStart = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);
            var quarterEnd = _QuarterCalculationEngine.GetQuarterDetail(2, 2018);
            var inventoryDateRangeTuple = new Tuple<QuarterDetailDto, QuarterDetailDto>(quarterStart, quarterEnd);
            var quarter = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);

            var result = _InventoryGapCalculationEngine.GetInventoryGaps(manifestWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryGapsFullQuarterGapTest()
        {
            // All media weeks for Q1 2018.
            var manifestWeeks = _GenerateManifestMediaWeeks(732, 743);
            // All media weeks for Q3 2018;
            manifestWeeks.AddRange(_GenerateManifestMediaWeeks(757, 770));
            var quarterStart = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);
            var quarterEnd = _QuarterCalculationEngine.GetQuarterDetail(3, 2018);
            var inventoryDateRangeTuple = new Tuple<QuarterDetailDto, QuarterDetailDto>(quarterStart, quarterEnd);
            var quarter = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);

            var result = _InventoryGapCalculationEngine.GetInventoryGaps(manifestWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryGapsWithGapEveryQuarterTest()
        {
            // First two weeks of Q1 2018.
            var manifestWeeks = _GenerateManifestMediaWeeks(732, 733);
            // Last two weeks of Q2 2018.
            manifestWeeks.AddRange(_GenerateManifestMediaWeeks(755, 756));
            var quarterStart = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);
            var quarterEnd = _QuarterCalculationEngine.GetQuarterDetail(2, 2018);
            var inventoryDateRangeTuple = new Tuple<QuarterDetailDto, QuarterDetailDto>(quarterStart, quarterEnd);
            var quarter = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);

            var result = _InventoryGapCalculationEngine.GetInventoryGaps(manifestWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryGapsHasGapInPreviousQuarterTest()
        {
            // First two weeks of Q1 2018.
            var manifestWeeks = _GenerateManifestMediaWeeks(732, 733);
            // Last two weeks of Q2 2018.
            manifestWeeks.AddRange(_GenerateManifestMediaWeeks(755, 756));
            var quarterStart = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);
            var quarterEnd = _QuarterCalculationEngine.GetQuarterDetail(2, 2018);
            var inventoryDateRangeTuple = new Tuple<QuarterDetailDto, QuarterDetailDto>(quarterStart, quarterEnd);
            var quarter = _QuarterCalculationEngine.GetQuarterDetail(2, 2018);

            var result = _InventoryGapCalculationEngine.GetInventoryGaps(manifestWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryGapsNoGapsTest()
        {
            // First two weeks of Q1 2018.
            var manifestWeeks = _GenerateManifestMediaWeeks(732, 733);
            // All media weeks of Q2 2018
            manifestWeeks.AddRange(_GenerateManifestMediaWeeks(744, 756));
            var quarterStart = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);
            var quarterEnd = _QuarterCalculationEngine.GetQuarterDetail(2, 2018);
            var inventoryDateRangeTuple = new Tuple<QuarterDetailDto, QuarterDetailDto>(quarterStart, quarterEnd);
            var quarter = _QuarterCalculationEngine.GetQuarterDetail(2, 2018);

            var result = _InventoryGapCalculationEngine.GetInventoryGaps(manifestWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryGapsMixedGapTest()
        {
            var manifestWeeks = _GenerateManifestMediaWeeks(732, 733);
            manifestWeeks.AddRange(_GenerateManifestMediaWeeks(735, 740));
            manifestWeeks.AddRange(_GenerateManifestMediaWeeks(742, 743));
            var quarterStart = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);
            var quarterEnd = _QuarterCalculationEngine.GetQuarterDetail(2, 2018);
            var quarter = _QuarterCalculationEngine.GetQuarterDetail(1, 2018);

            var result = _InventoryGapCalculationEngine.GetInventoryGaps(manifestWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<int> _GenerateManifestMediaWeeks(int from, int to)
        {
            var result = new List<int>();

            for (var start = from; start <= to; start++)
                result.Add(start);

            return result;
        }
    }
}
