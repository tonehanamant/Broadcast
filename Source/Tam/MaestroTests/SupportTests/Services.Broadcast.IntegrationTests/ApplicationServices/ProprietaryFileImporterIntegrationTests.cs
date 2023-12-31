﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProprietaryInventory;
using System;
using System.IO;
using System.Linq;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProprietaryFileImporterIntegrationTests
    {
        private const string inventorySourceTvb = "TVB";
        private const string inventorySourceMLB = "MLB";
        private const string inventorySourceBounce = "Bounce";
        private const string inventorySourceKatz = "KATZ";
        private const string inventorySourceNbcSyn = "NBCU Syn";
        private const string inventorySourceCbsSyn = "CBS Synd";
        private const string inventorySourceCourTV = "Escape"; // CourTV became TruTV became Escape

        private IProprietaryFileImporterFactory _ProprietaryFileImporterFactory;
        private IInventoryRepository _InventoryRepository;

        [SetUp]
        public void SetUp()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(new FileService());
            _ProprietaryFileImporterFactory = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryFileImporterFactory>();

            _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Barter_GetPendingBarterInventoryFile()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_GetPendingBarterInventoryFile.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
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
                var file = fileImporter.GetPendingProprietaryInventoryFile("integration test", inventorySource);

                _VerifyProprietaryInventoryFile(file);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_ExtractData_BadFormats()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Barter_ExtractData_PRI5379()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats_PRI5379.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
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
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_ExtractData_MoreBadFormats()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats2.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_ExtractData_BadFormatsAgain()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats3.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Parses_DataLines()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with valid data.xlsx";
            var barterFile = new ProprietaryInventoryFile();
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceMLB);

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], barterFile);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(barterFile));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_MissingUnit()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with invalid unit.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceMLB);
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                var proprietaryFile = new ProprietaryInventoryFile();
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], proprietaryFile);
                _VerifyProprietaryInventoryFile(proprietaryFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_MissingSpotLength()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with invalid spot length.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceMLB);
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                var proprietaryFile = new ProprietaryInventoryFile();
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], proprietaryFile);
                _VerifyProprietaryInventoryFile(proprietaryFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_InvalidUnit()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with invalid unit PRI-5676.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceMLB);
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                var proprietaryFile = new ProprietaryInventoryFile();
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], proprietaryFile);
                _VerifyProprietaryInventoryFile(proprietaryFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Barter_Invalid6_PRI10292()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_invalid6.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceMLB);
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                var proprietaryFile = new ProprietaryInventoryFile();
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], proprietaryFile);
                
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proprietaryFile.ValidationProblems));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_InvalidSpotLength()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with invalid spot length PRI-5676.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceMLB);
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                var proprietaryFile = new ProprietaryInventoryFile();
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], proprietaryFile);
                _VerifyProprietaryInventoryFile(proprietaryFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_ReturnsFileProblems_WhenFileHasMissedValues()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_DataLines file with missed values.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                var proprietaryFile = new ProprietaryInventoryFile();
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], proprietaryFile);
                _VerifyProprietaryInventoryFile(proprietaryFile);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't find last unit column")]
        [Category("long_running")]
        public void Barter_DoesNotHaveValidUnitsEndColumn()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_WrongCommentsColumn.xlsx";

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceMLB);
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], new ProprietaryInventoryFile());
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_ExtractData_PRI5667()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats_PRI5667.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void Barter_ExtractData_PRI5980()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats_PRI5980.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
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
            jsonResolver.Ignore(typeof(InventoryFileBase), "CreatedDate");
            jsonResolver.Ignore(typeof(ProprietaryInventoryHeader), "ContractedDaypartId");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var fileJson = IntegrationTestHelper.ConvertToJson(file, jsonSettings);

            Approvals.Verify(fileJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Barter_SaveErrorsToFile()
        {
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
            const string fileName = @"ProprietaryDataFiles\Barter_invalid1.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", inventorySource);
                var result = fileImporter.ExtractData(file);
                Assert.AreNotEqual(request.StreamData, result);

                using (var package = new ExcelPackage(result))
                {
                    var worksheet = package.Workbook.Worksheets.First();
                    var headerErrors = worksheet.Cells["E3:E12"].ToDictionary(x=>x.Address, x=>x.Text);
                    var lineErrors = worksheet.Cells["I16:I22"].ToDictionary(x => x.Address, x => x.Text);
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(new { headerErrors, lineErrors}));
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Diginet_SaveErrorsToFile()
        {
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceBounce);
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
            const string fileName = @"ProprietaryDataFiles\Diginet_invalidFile12.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", inventorySource);
                var result = fileImporter.ExtractData(file);
                Assert.AreNotEqual(request.StreamData, result);

                using (var package = new ExcelPackage(result))
                {
                    var worksheet = package.Workbook.Worksheets.First();
                    var headerErrors = worksheet.Cells["E3:E6"].ToDictionary(x => x.Address, x => x.Text);
                    var lineErrors = worksheet.Cells["J11:J15"].ToDictionary(x => x.Address, x => x.Text);
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(new { headerErrors, lineErrors }));
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Diginet_ExtractData_Bounce_Audience()
        {
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceBounce);
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
            const string fileName = @"ProprietaryDataFiles\Diginet_Bounce_M18+_7.1.19 .xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };
                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", inventorySource);

                var result = fileImporter.ExtractData(file);

                Assert.AreNotEqual(request.StreamData, result);
                using (var package = new ExcelPackage(result))
                {
                    var worksheet = package.Workbook.Worksheets.First();
                    var headerErrors = worksheet.Cells["E3:E6"].ToDictionary(x => x.Address, x => x.Text);
                    var lineErrors = worksheet.Cells["J11:J15"].ToDictionary(x => x.Address, x => x.Text);
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(new { headerErrors, lineErrors }));
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void OAndO_SaveErrorsToFile()
        {
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceKatz);
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
            const string fileName = @"ProprietaryDataFiles\OAndO_InvalidFile5.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", inventorySource);
                var result = fileImporter.ExtractData(file);
                Assert.AreNotEqual(request.StreamData, result);

                using (var package = new ExcelPackage(result))
                {
                    var worksheet = package.Workbook.Worksheets.First();
                    var headerErrors = worksheet.Cells["E3:E12"].ToDictionary(x => x.Address, x => x.Text);
                    var lineErrors = worksheet.Cells["L16:L18"].ToDictionary(x => x.Address, x => x.Text);
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(new { headerErrors, lineErrors }));
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Syndicator_SaveErrorsToFile()
        {
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceNbcSyn);
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
            const string fileName = @"ProprietaryDataFiles\Syndication_InvalidFile9.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                fileImporter.LoadFromSaveRequest(request);
                ProprietaryInventoryFile file = fileImporter.GetPendingProprietaryInventoryFile("integration test", inventorySource);
                var result = fileImporter.ExtractData(file);
                Assert.AreNotEqual(request.StreamData, result);

                using (var package = new ExcelPackage(result))
                {
                    var worksheet = package.Workbook.Worksheets.First();
                    var headerErrors = worksheet.Cells["E3:E10"].ToDictionary(x => x.Address, x => x.Text);
                    var lineErrors = worksheet.Cells["K15:K20"].ToDictionary(x => x.Address, x => x.Text);
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(new { headerErrors, lineErrors }));
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Barter_AudienceMap()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Barter_Valid_AudienceMap1.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceTvb);
            _VerifyFile(fileName, inventorySource);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Syndication_AudienceMap()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Syndicator_Valid_AudienceMap1.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceCbsSyn);
            _VerifyFile(fileName, inventorySource);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Diginet_AudienceMap()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Diginet_Valid_AudienceMap1.xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceCourTV);
            _VerifyFile(fileName, inventorySource);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Diginet_AudienceMap_PRI11102()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Diginet_Bounce_M18+_7.1.19 .xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceBounce);
            _VerifyFile(fileName, inventorySource);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Syndicator_AudienceMap_PRI11102()
        {
            const string fileName = @".\Files\ProprietaryDataFiles\Syndicator NBCUSyn M35+ 07-01-2019 .xlsx";
            var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceNbcSyn);
            _VerifyFile(fileName, inventorySource);
        }

        private void _VerifyFile(string fileName, InventorySource inventorySource)
        {
            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);
                var proprietaryFile = new ProprietaryInventoryFile();
                if (inventorySource.InventoryType == InventorySourceTypeEnum.Diginet)
                {
                    // tie in the 'Diginet Stations' tab.
                    fileImporter.LoadAndValidateHeaderData(package.Workbook.Worksheets[1], proprietaryFile);
                    fileImporter.LoadAndValidateSecondWorksheet(package.Workbook.Worksheets[2], proprietaryFile);
                }
                fileImporter.LoadAndValidateDataLines(package.Workbook.Worksheets[1], proprietaryFile);
                _VerifyProprietaryInventoryFile(proprietaryFile);
            }
        }
    }
}
