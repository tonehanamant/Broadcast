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
using Services.Broadcast.IntegrationTests.Stubbs;
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
        private IBarterInventoryService _BarterService = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterInventoryService>();
        private IInventoryFileRepository _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
        private IInventoryRepository _IInventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        private IBarterRepository _BarterRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBarterRepository>();     

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
        public void BarterInventoryService_SavesManifests_SingleBook()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat_SingleBook.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryGroups(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SavesManifests_TwoBooks()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat_TwoBooks.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryGroups(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SavesManifests_WhenStationIsUnknown()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat_UnknownStation.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryGroups(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_ValidationErrors()
        {
            const string fileName = @"BarterDataFiles\Barter DataLines file with missed values.xlsx";

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SaveBarterInventoryFile_PRI5667()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_BadFormats_PRI5667.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryGroups(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SaveBarterInventoryFile_InvalidDaypart()
        {
            const string fileName = @"BarterDataFiles\Barter DataLines file with invalid daypart.xlsx";

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SaveBarterInventoryFile_SingleDataColumn()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_SingleDataColumn.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryGroups(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BarterInventoryService_SaveBarterInventoryFile_DateRangeIntersecting()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_DateRangeIntersecting.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryGroups(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile1()
        {
            const string fileName = @"BarterDataFiles\OAndO_InvalidFile1.xlsx";

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile2()
        {
            const string fileName = @"BarterDataFiles\OAndO_InvalidFile2.xlsx";

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile3()
        {
            const string fileName = @"BarterDataFiles\OAndO_InvalidFile3.xlsx";

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OAndO_NotValidFile4()
        {
            const string fileName = @"BarterDataFiles\OAndO_InvalidFile4.xlsx";

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
        public void BarterInventoryService_SendFileToDataLake()
        {
            const string fileName = @"BarterDataFiles\BarterFileImporter_ValidFormat.xlsx";

            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
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
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();

            const string fileName = @"BarterDataFiles\DuplicateData_ForecastDB_TwoBooks.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryGroups(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveBarterInventoryFile_DuplicateDataError_ForecastDb_SingleBook()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();

            const string fileName = @"BarterDataFiles\DuplicateData_ForecastDB_SingleBook.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _BarterService.SaveBarterInventoryFile(request, "IntegrationTestUser", now);

                _VerifyInventoryGroups(result.FileId);
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

        private void _VerifyInventoryGroups(int fileId)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestBase), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestBase), "FileId");
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
    }
}
