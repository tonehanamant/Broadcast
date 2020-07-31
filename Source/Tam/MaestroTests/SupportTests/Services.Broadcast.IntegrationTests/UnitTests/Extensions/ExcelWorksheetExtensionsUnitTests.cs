using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.Extensions;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class ExcelWorksheetExtensionsUnitTests
    {
        private const string WORKSHEET_NAME = "Test";

        [Test]
        [TestCase(1, "A")]
        [TestCase(13, "M")]
        [TestCase(27, "AA")]
        [TestCase(52, "AZ")]
        [TestCase(82, "CD")]
        [TestCase(8273, "LFE")]
        public void GetColumnAdress(int column, string expected)
        {
            var result = column.GetColumnAdress();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetStringValue()
        {
            var package = _GetExcelPackage(" Test with blank spaces ");

            var result = package.Workbook.Worksheets[WORKSHEET_NAME].Cells[1, 1].GetStringValue();

            Assert.AreEqual("Test with blank spaces", result);
        }

        [Test]
        public void GetTextValue()
        {
            var package = _GetExcelPackage(" Test with blank spaces ");

            var result = package.Workbook.Worksheets[WORKSHEET_NAME].Cells[1, 1].GetTextValue();

            Assert.AreEqual("Test with blank spaces", result);
        }

        [Test]
        public void GetIntValue()
        {
            var package = _GetExcelPackage(15);

            var result = package.Workbook.Worksheets[WORKSHEET_NAME].Cells[1, 1].GetIntValue();

            Assert.AreEqual(15, result);
        }

        [Test]
        public void GetDoubleValue()
        {
            var package = _GetExcelPackage(15.8);

            var result = package.Workbook.Worksheets[WORKSHEET_NAME].Cells[1, 1].GetDoubleValue();

            Assert.AreEqual(15.8, result);
        }

        [Test]
        public void GetDecimalValue()
        {
            var package = _GetExcelPackage(19.21m);

            var result = package.Workbook.Worksheets[WORKSHEET_NAME].Cells[1, 1].GetDecimalValue();

            Assert.AreEqual(19.21m, result);
        }


        private ExcelPackage _GetExcelPackage(object value)
        {
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add(WORKSHEET_NAME);
            package.Workbook.Worksheets[WORKSHEET_NAME].Cells[1, 1].Value = value;
            return package;
        }
    }
}
