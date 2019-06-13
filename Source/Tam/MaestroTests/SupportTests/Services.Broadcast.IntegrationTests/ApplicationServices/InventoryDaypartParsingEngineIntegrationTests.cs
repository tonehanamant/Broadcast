using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryDaypartParsingEngineIntegrationTests
    {
        private readonly IInventoryDaypartParsingEngine _InventoryDaypartParsingEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryDaypartParsingEngine>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanParseCNNDayparts()
        {
            const string fileName = @".\Files\InventoryDaypartParsing\CNN_dayparts.xlsx";

            var daypartStrings = _LoadDaypartsFromFile(fileName);
            var result = _ParseDayparts(daypartStrings);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanParseCNNDayparts_2()
        {
            const string fileName = @".\Files\InventoryDaypartParsing\CNN_dayparts_2.xlsx";

            var daypartStrings = _LoadDaypartsFromFile(fileName);
            var result = _ParseDayparts(daypartStrings);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DoesNotReturnDaypartsWhenDaypartIsInvalid()
        {
            const string fileName = @".\Files\InventoryDaypartParsing\CNN_dayparts_invalid.xlsx";

            var daypartStrings = _LoadDaypartsFromFile(fileName);
            var result = _ParseDayparts(daypartStrings);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DaypartIsValid()
        {
            const string fileName = @".\Files\InventoryDaypartParsing\CNN_dayparts_valid.xlsx";

            var daypartStrings = _LoadDaypartsFromFile(fileName);
            var result = _ParseDayparts(daypartStrings);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private dynamic _ParseDayparts(List<string> daypartStrings)
        {
            var result = new
            {
                success = new Dictionary<string, IEnumerable<string>>(),
                failures = new List<string>()
            };

            foreach (var daypartString in daypartStrings)
            {
                if (_InventoryDaypartParsingEngine.TryParse(daypartString, out var dayparts))
                {
                    result.success[daypartString] = dayparts.Select(x => x.ToLongString());
                }
                else
                {
                    result.failures.Add(daypartString);
                }
            }

            return result;
        }

        private List<string> _LoadDaypartsFromFile(string fileName)
        {
            var result = new List<string>();

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var worksheet = package.Workbook.Worksheets[1];
                var uniqueDaypartStrings = worksheet.Cells
                    .Where(x => x.Value != null)
                    .Select(x => x.Value.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct();
                result.AddRange(uniqueDaypartStrings);
            }

            return result;
        }
    }
}
