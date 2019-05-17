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
using Services.Broadcast.Entities.ProprietaryInventory;
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
    public class ProprietaryInventoryServiceIntegrationTests
    {
        private IProprietaryInventoryService _ProprietaryService;
        private IInventoryFileRepository _InventoryFileRepository;
        private IInventoryRepository _IInventoryRepository;
        private IProprietaryRepository _ProprietaryRepository;
        private IInventoryRatingsProcessingService _InventoryRatingsProcessingService;

        [TestFixtureSetUp]
        public void SetUp()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(new FileServiceDataLakeStubb());
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            _ProprietaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>();
            _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _IInventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ProprietaryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProprietaryRepository>();
            _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesBarterInventoryFile()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_ValidFormat.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);

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
        [Ignore("This test was used to load data into dbs")]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_LoadBarterFilesIntoDbs()
        {
            foreach (string fileName in new List<string>{
                "TTWN EMN Q1 Barter_Inventory Template_2.6.2019 (17).xlsx",
                //"TTWN EVENING NEWS Q2 Barter_Inventory Template_2.6.2019 (6).xlsx"  //this file has an outdated template
            })
            {
                using (var transaction = new TransactionScopeWrapper())
                {
                    var request = new FileRequest
                    {
                        StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                        FileName = fileName
                    };

                    var now = new DateTime(2019, 02, 02);
                    var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "sroibu", now);

                    if (result.FileId > 0 && result.Status == Entities.Enums.FileStatusEnum.Loaded)
                    {
                        transaction.Complete();
                    }
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_ValidFormat_SpotLengthWithColon()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_ValidFormat_SpotLengthWithColon.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_SavesManifests_SingleBook()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_ValidFormat_SingleBook.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_SavesManifests_TwoBooks()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_ValidFormat_TwoBooks.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_SavesManifests_WhenStationIsUnknown()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_ValidFormat_UnknownStation.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_ValidationErrors()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_DataLines file with missed values.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_InvalidFileFormat()
        {
            const string fileName = @"1Chicago WLS Syn 4Q16 UNKNOWN.xml";

            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryFileSaveResult(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_ExtractData_PRI5390()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats_PRI5390.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_SaveBarterInventoryFile_PRI5667()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_BadFormats_PRI5667.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_SaveBarterInventoryFile_InvalidDaypart()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_DataLines file with invalid daypart.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_SaveBarterInventoryFile_SingleDataColumn()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_SingleDataColumn.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_SaveBarterInventoryFile_DateRangeIntersecting()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_DateRangeIntersecting.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile1()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_InvalidFile1.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile2()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_InvalidFile2.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile3()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_InvalidFile3.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile4()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_InvalidFile4.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_WeekIsOutOfValidDateInterval()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_WeekIsOutOfValidDateInterval.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_WeekIsSpecifiedSeveralTimes()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_WeekIsSpecifiedSeveralTimes.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOInventoryFile()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_ValidFile1.xlsx";
            _VerifyInventoryFileMetadataAndHeaderData(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOInventoryFileManifests()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_ValidFile1.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOInventoryFileManifests_EmptyAndSummaryRows()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_ValidFile2.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOInventoryFileManifests_NoHut()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_ValidFile3_NoHut.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOInventoryFileManifests_OAndO_PRI7393_DaypartParsingBug()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_PRI7393.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesOAndOInventoryFileManifests_OAndO_PRI7410_DaypartParsingBug()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_PRI7410.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NoStationHeaderCell()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_NoStationHeaderCell.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_No_HH_HeaderCell()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_No_HH_HeaderCell.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_UnknownAudience()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_UnknownAudience.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }      

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_WeekIsMissing()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_WeekIsMissing.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidWeek()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_NotValidWeek.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NoMediaWeekFound()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_NoMediaWeekFound.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_MissingData1()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_MissingData1.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NoWeeksStartHeaderCell()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_NoWeeksStartHeaderCell.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Barter_SendFileToDataLake()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_ValidFormat.xlsx";

            using (new TransactionScopeWrapper())
            {
                var fileService = IntegrationTestApplicationServiceFactory.Instance.Resolve<IFileService>();
                var dataLakeFolder = BroadcastServiceSystemParameter.DataLake_SharedFolder;
                var filePath = Path.Combine(dataLakeFolder, fileName);
                var proprietaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>();

                if (fileService.Exists(filePath))
                {
                    fileService.Delete(filePath);
                }

                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = proprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);

                Assert.True(fileService.Exists(filePath));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveBarterInventoryFile_DuplicateDataError_ForecastDb_TwoBooks()
        {
            const string fileName = @"ProprietaryDataFiles\DuplicateData_ForecastDB_TwoBooks.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveBarterInventoryFile_DuplicateDataError_ForecastDb_SingleBook()
        {
            const string fileName = @"ProprietaryDataFiles\DuplicateData_ForecastDB_SingleBook.xlsx";
            _VerifyFileInventoryGroups(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_ValidFile()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_ValidFile1.xlsx";
            _VerifyInventoryFileMetadataAndHeaderData(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_ValidFile2()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_ValidFile2.xlsx";
            _VerifyInventoryFileMetadataAndHeaderData(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_ValidFile3()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_ValidFile3.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_InvalidFile1()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_InvalidFile1.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_InvalidFile2()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_InvalidFile2.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_InvalidFile3()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_InvalidFile3.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_InvalidFile4()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_InvalidFile4.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_InvalidFile5()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_InvalidFile5.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_InvalidFile6()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_InvalidFile6.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_InvalidFile7()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_InvalidFile7.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Syndication_InvalidFile8_DuplicateLines()
        {
            const string fileName = @"ProprietaryDataFiles\Syndication_InvalidFile8.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesDiginetInventoryFile()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile1.xlsx";
            _VerifyInventoryFileMetadataAndHeaderData(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesDiginetInventoryFileManifests()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile2.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesDiginetInventoryFileManifests_WithSpacesInAudience()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile3.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesDiginetInventoryFileManifests_WithSpacesInDaypart()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_PRI8845.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesManifests_WhenAudienceThatCanBeMappedIsSpecified()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile4.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesManifests_ButSkipsDefaultDemoAudiences()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile5.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesManifests_ButSkipsEmptyDemoAudiences()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_PRI8905_ValidFile.xlsx";
            _VerifyFileInventoryManifests(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_PRI8905_InvalidFile()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_PRI8905_InvalidFile.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile1()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile1.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile2()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile2.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile3()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile3.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile4()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile4.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile5()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile5.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile6()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile6.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile7()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile7.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile8()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile8.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile9()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile9.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile10()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile10.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Diginet_InvalidFile11()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_InvalidFile11.xlsx";
            _VerifyInventoryFileProblems(fileName);
        }

        private void _VerifyInventoryFileProblems(string fileName)
        {
            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var problems = new List<InventoryFileProblem>();
                try
                {
                    var now = new DateTime(2019, 02, 02);
                    var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);
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
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);

                _VerifyFileInventoryGroups(result.FileId);
            }
        }

        private void _VerifyFileInventoryGroups(int fileId)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "InventoryFileId");
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
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);

                _VerifyFileInventoryManifests(result.FileId);
            }
        }

        private void _VerifyFileInventoryManifests(int fileId)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryManifest), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "InventoryFileId");
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

        private void _VerifyInventoryFileMetadataAndHeaderData(string fileName)
        {
            using (new TransactionScopeWrapper())
            {
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileBase), "Id");
                jsonResolver.Ignore(typeof(ProprietaryInventoryFile), "CreatedDate");
                jsonResolver.Ignore(typeof(InventorySource), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var file = _ProprietaryRepository.GetInventoryFileWithHeaderById(result.FileId);
                var fileJson = IntegrationTestHelper.ConvertToJson(file, jsonSettings);

                Approvals.Verify(fileJson);
            }
        }
    }
}
