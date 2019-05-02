using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.Enums;
using System;
using System.IO;
using Tam.Maestro.Common.DataLayer;
namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProprietaryFileImporterIntegrationTests
    {
        private IProprietaryFileImporterFactory _ProprietaryFileImporterFactory = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryFileImporterFactory>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_GetPendingBarterInventoryFile()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_GetPendingBarterInventoryFile.xlsx";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);

                fileImporter.CheckFileHash();
                var file = fileImporter.GetPendingProprietaryInventoryFile("integration test", null);

                _VerifyProprietaryInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_ExtractData_BadFormats()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats.xlsx";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", null);
                fileImporter.ExtractData(file);
                _VerifyProprietaryInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_ExtractData_PRI5379()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats_PRI5379.xlsx";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", null);
                fileImporter.ExtractData(file);
                _VerifyProprietaryInventoryFile(file);
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_ExtractData_MoreBadFormats()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats2.xlsx";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", null);
                fileImporter.ExtractData(file);
                _VerifyProprietaryInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_ExtractData_BadFormatsAgain()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats3.xlsx";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", null);
                fileImporter.ExtractData(file);
                _VerifyProprietaryInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Parses_DataLines()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with valid data.xlsx";
            var barterFile = new ProprietaryInventoryFile();

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], barterFile);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(barterFile));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Unit name missing")]
        public void ThrowsException_WhenFileHasMissingUnit()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with invalid unit.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new ProprietaryInventoryFile());
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Spot length is missing")]
        public void ThrowsException_WhenFileHasMissingSpotLength()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with invalid spot length.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new ProprietaryInventoryFile());
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Invalid unit was found")]
        public void ThrowsException_WhenFileHasInvalidUnit()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with invalid unit PRI-5676.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new ProprietaryInventoryFile());
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Invalid spot length was found")]
        public void ThrowsException_WhenFileHasInvalidSpotLength()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with invalid spot length PRI-5676.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new ProprietaryInventoryFile());
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsFileProblems_WhenFileHasMissedValues()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with missed values.xlsx";
            var proprietaryFile = new ProprietaryInventoryFile();

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], proprietaryFile);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(proprietaryFile));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't find last unit column")]
        public void ThrowsException_WhenFileDoesNotHaveValidUnitsEndColumn()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_WrongCommentsColumn.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new ProprietaryInventoryFile());
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_ExtractData_PRI5667()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats_PRI5667.xlsx";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", null);
                fileImporter.ExtractData(file);
                _VerifyProprietaryInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_ExtractData_PRI5980()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats_PRI5980.xlsx";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.Barter };
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", inventorySource);
                fileImporter.ExtractData(file);
                _VerifyProprietaryInventoryFile(file);
            }
        }

        private static void _VerifyProprietaryInventoryFile(ProprietaryInventoryFile file)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(InventoryFileBase), "Id");
            jsonResolver.Ignore(typeof(ProprietaryInventoryFile), "CreatedDate");
            jsonResolver.Ignore(typeof(ProprietaryInventoryHeader), "ContractedDaypartId");
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
