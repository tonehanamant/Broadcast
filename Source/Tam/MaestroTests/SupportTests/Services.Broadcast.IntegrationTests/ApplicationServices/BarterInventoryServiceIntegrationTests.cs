using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class BarterInventoryServiceIntegrationTests
    {
        private IBarterInventoryService _BarterService;
        private IInventoryFileRepository _InventoryFileRepository;
        private IInventoryRepository _IInventoryRepository;
        private IBarterRepository _BarterRepository;
        private IInventoryRatingsProcessingService _InventoryRatingsProcessingService;

        [TestFixtureSetUp]
        public void SetUp()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            _BarterService = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterInventoryService>();
            _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _IInventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _BarterRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBarterRepository>();
            _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SaveBarterInventoryFile()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileBase), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var file = _InventoryFileRepository.GetInventoryFileById(result.FileId);
                var fileJson = IntegrationTestHelper.ConvertToJson(file, jsonSettings);

                Approvals.Verify(fileJson);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterFileImporter_ValidFormat_SpotLengthWithColon()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat_SpotLengthWithColon.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SavesManifests_SingleBook()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat_SingleBook.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SavesManifests_TwoBooks()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat_TwoBooks.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SavesManifests_WhenStationIsUnknown()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat_UnknownStation.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_ValidationErrors()
        {
            const string fileName = @"BarterDataFiles\Barter DataLines file with missed values.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_InvalidFileFormat()
        {
            const string fileName = @"1Chicago WLS Syn 4Q16 UNKNOWN.xml";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryFileSaveResult(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterFileImporter_ExtractData_PRI5390()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_BadFormats_PRI5390.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SaveBarterInventoryFile_PRI5667()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_BadFormats_PRI5667.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SaveBarterInventoryFile_InvalidDaypart()
        {
            const string fileName = @"BarterDataFiles\Barter DataLines file with invalid daypart.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SaveBarterInventoryFile_SingleDataColumn()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_SingleDataColumn.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SaveBarterInventoryFile_DateRangeIntersecting()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_DateRangeIntersecting.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile1()
        {
            const string fileName = @"BarterDataFiles\OAndO_InvalidFile1.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile2()
        {
            const string fileName = @"BarterDataFiles\OAndO_InvalidFile2.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile3()
        {
            const string fileName = @"BarterDataFiles\OAndO_InvalidFile3.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile4()
        {
            const string fileName = @"BarterDataFiles\OAndO_InvalidFile4.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOBarterInventoryFile()
        {
            const string fileName = @"BarterDataFiles\OAndO_ValidFile1.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileBase), "Id");
                jsonResolver.Ignore(typeof(BarterInventoryFile), "CreatedDate");
                jsonResolver.Ignore(typeof(InventorySource), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var file = _BarterRepository.GetBarterInventoryFileById(result.FileId);
                var fileJson = IntegrationTestHelper.ConvertToJson(file, jsonSettings);

                Approvals.Verify(fileJson);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOBarterInventoryFileManifests()
        {
            const string fileName = @"BarterDataFiles\OAndO_ValidFile1.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOBarterInventoryFileManifests_EmptyAndSummaryRows()
        {
            const string fileName = @"BarterDataFiles\OAndO_ValidFile2.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOBarterInventoryFileManifests_NoHut()
        {
            const string fileName = @"BarterDataFiles\OAndO_ValidFile3_NoHut.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NoStationHeaderCell()
        {
            const string fileName = @"BarterDataFiles\OAndO_NoStationHeaderCell.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_No_HH_HeaderCell()
        {
            const string fileName = @"BarterDataFiles\OAndO_No_HH_HeaderCell.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_UnknownAudience()
        {
            const string fileName = @"BarterDataFiles\OAndO_UnknownAudience.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NoEmptyColumnBetweenLastWeekAndHH()
        {
            const string fileName = @"BarterDataFiles\OAndO_NoEmptyColumnBetweenLastWeekAndHH.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_WeekIsMissing()
        {
            const string fileName = @"BarterDataFiles\OAndO_WeekIsMissing.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidWeek()
        {
            const string fileName = @"BarterDataFiles\OAndO_NotValidWeek.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NoMediaWeekFound()
        {
            const string fileName = @"BarterDataFiles\OAndO_NoMediaWeekFound.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_MissingData1()
        {
            const string fileName = @"BarterDataFiles\OAndO_MissingData1.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NoWeeksStartHeaderCell()
        {
            const string fileName = @"BarterDataFiles\OAndO_NoWeeksStartHeaderCell.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SendFileToDataLake()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat.xlsx";

            using (new TransactionScopeWrapper())
            {
                var fileService = IntegrationTestApplicationServiceFactory.Instance.Resolve<IFileService>();
                var dataLakeFolder = BroadcastServiceSystemParameter.DataLake_SharedFolder;
                var filePath = Path.Combine(dataLakeFolder, fileName);
                var barterService = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterInventoryService>();

                if (fileService.Exists(filePath))
                {
                    fileService.Delete(filePath);
                }

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = barterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                Assert.True(fileService.Exists(filePath));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveBarterInventoryFile_DuplicateDataError_ForecastDb_TwoBooks()
        {
            const string fileName = @"BarterDataFiles\DuplicateData_ForecastDB_TwoBooks.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveBarterInventoryFile_DuplicateDataError_ForecastDb_SingleBook()
        {
            const string fileName = @"BarterDataFiles\DuplicateData_ForecastDB_SingleBook.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        private void _VerifyInventoryFileProblems(string fileName)
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var problems = new List<InventoryFileProblem>();
                try
                {
                    var now = new DateTime(2019, 02, 02);
                    var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                _VerifyInventoryFileProblems(problems);
            }
        }

        private static void _VerifyInventoryFileProblems(List<InventoryFileProblem> problems)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var problemsJson = IntegrationTestHelper.ConvertToJson(problems, jsonSettings);

            Approvals.Verify(problemsJson);
        }

        private static void _VerifyInventoryFileSaveResult(InventoryFileSaveResult result)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        private void _VerifyFileInventoryGroups(string fileName)
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyFileInventoryGroups(result.FileId);
            }
        }

        private void _VerifyFileInventoryGroups(int fileId)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "FileId");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "ProjectedStationImpressions");
            jsonResolver.Ignore(typeof(StationInventoryManifestAudience), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
            jsonResolver.Ignore(typeof(MediaWeek), "_Id");
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var groups = _IInventoryRepository.GetStationInventoryGroupsByFileId(fileId);
            var groupsJson = IntegrationTestHelper.ConvertToJson(groups, jsonSettings);

            Approvals.Verify(groupsJson);
        }

        private void _VerifyFileInventoryManifests(string fileName)
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyFileInventoryManifests(result.FileId);
            }
        }

        private void _VerifyFileInventoryManifests(int fileId)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryManifest), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "FileId");
            jsonResolver.Ignore(typeof(StationInventoryManifestAudience), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
            jsonResolver.Ignore(typeof(MediaWeek), "_Id");
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var manifests = _IInventoryRepository.GetStationInventoryManifestsByFileId(fileId);
            var manifestsJson = IntegrationTestHelper.ConvertToJson(manifests, jsonSettings);

            Approvals.Verify(manifestsJson);
        }
    }
}
