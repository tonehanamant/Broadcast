using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using System;
using System.IO;
using Tam.Maestro.Common.DataLayer;
namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class BarterFileImporterIntegrationTests
    {
        private IBarterFileImporter _BarterFileImporter = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterFileImporter>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterFileImporter_GetPendingBarterInventoryFile()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_GetPendingBarterInventoryFile.xlsx";
            var _barterfileImporter = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                _barterfileImporter.LoadFromSaveRequest(request);

                _barterfileImporter.CheckFileHash();
                var file = _barterfileImporter.GetPendingBarterInventoryFile("integration test");

                _VerifyBarterInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterFileImporter_ExtractData_BadFormats()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_BadFormats.xlsx";
            var _barterfileImporter = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                _barterfileImporter.LoadFromSaveRequest(request);
                BarterInventoryFile file = _barterfileImporter.GetPendingBarterInventoryFile("integration test"); // _LoadBarterFile(fileName);
                _barterfileImporter.ExtractData(file);
                _VerifyBarterInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterFileImporter_ExtractData_PRI5379()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_BadFormats_PRI5379.xlsx";
            var _barterfileImporter = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                _barterfileImporter.LoadFromSaveRequest(request);
                BarterInventoryFile file = _barterfileImporter.GetPendingBarterInventoryFile("integration test"); // _LoadBarterFile(fileName);
                _barterfileImporter.ExtractData(file);
                _VerifyBarterInventoryFile(file);
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterFileImporter_ExtractData_MoreBadFormats()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_BadFormats2.xlsx";
            var _barterfileImporter = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                _barterfileImporter.LoadFromSaveRequest(request);
                BarterInventoryFile file = _barterfileImporter.GetPendingBarterInventoryFile("integration test");
                _barterfileImporter.ExtractData(file);
                _VerifyBarterInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterFileImporter_ExtractData_BadFormatsAgain()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_BadFormats3.xlsx";
            var _barterfileImporter = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                _barterfileImporter.LoadFromSaveRequest(request);
                BarterInventoryFile file = _barterfileImporter.GetPendingBarterInventoryFile("integration test");
                _barterfileImporter.ExtractData(file);
                _VerifyBarterInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Parses_DataLines()
        {
            const string fileName = @".\Files\BarterDataFiles\Barter DataLines file with valid data.xlsx";
            var barterFile = new BarterInventoryFile();

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                _BarterFileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], barterFile);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(barterFile));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Unit name missing")]
        public void ThrowsException_WhenFileHasMissingUnit()
        {
            const string fileName = @".\Files\BarterDataFiles\Barter DataLines file with invalid unit.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                _BarterFileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new BarterInventoryFile());
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Spot length is missing")]
        public void ThrowsException_WhenFileHasMissingSpotLength()
        {
            const string fileName = @".\Files\BarterDataFiles\Barter DataLines file with invalid spot length.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                _BarterFileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new BarterInventoryFile());
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Invalid unit was found")]
        public void ThrowsException_WhenFileHasInvalidUnit()
        {
            const string fileName = @".\Files\BarterDataFiles\Barter DataLines file with invalid unit PRI-5676.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                _BarterFileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new BarterInventoryFile());
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Invalid spot length was found")]
        public void ThrowsException_WhenFileHasInvalidSpotLength()
        {
            const string fileName = @".\Files\BarterDataFiles\Barter DataLines file with invalid spot length PRI-5676.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                _BarterFileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new BarterInventoryFile());
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsFileProblems_WhenFileHasMissedValues()
        {
            const string fileName = @".\Files\BarterDataFiles\Barter DataLines file with missed values.xlsx";
            var barterFile = new BarterInventoryFile();

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                _BarterFileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], barterFile);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(barterFile));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't find last unit column")]
        public void ThrowsException_WhenFileDoesNotHaveValidUnitsEndColumn()
        {
            const string fileName = @".\Files\BarterDataFiles\BarterFileImporter_WrongCommentsColumn.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                _BarterFileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new BarterInventoryFile());
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterFileImporter_ExtractData_PRI5667()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_BadFormats_PRI5667.xlsx";
            var _barterfileImporter = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                _barterfileImporter.LoadFromSaveRequest(request);
                BarterInventoryFile file = _barterfileImporter.GetPendingBarterInventoryFile("integration test");
                _barterfileImporter.ExtractData(file);
                _VerifyBarterInventoryFile(file);
            }
        }

        private static void _VerifyBarterInventoryFile(BarterInventoryFile file)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(InventoryFileBase), "Id");
            jsonResolver.Ignore(typeof(BarterInventoryFile), "CreatedDate");
            jsonResolver.Ignore(typeof(BarterInventoryHeader), "ContractedDaypartId");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var fileJson = IntegrationTestHelper.ConvertToJson(file, jsonSettings);

            Approvals.Verify(fileJson);
        }
    }
}
