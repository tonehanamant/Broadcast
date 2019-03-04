using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class BarterInventoryServiceIntegrationTests
    {
        private IBarterInventoryService _BarterService = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterInventoryService>();
        private IInventoryFileRepository _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
        private IInventoryRepository _IInventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

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

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryManifestAudience), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
                jsonResolver.Ignore(typeof(MediaWeek), "_Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var groups = _IInventoryRepository.GetStationInventoryGroupsByFileId(result.FileId);
                var groupsJson = IntegrationTestHelper.ConvertToJson(groups, jsonSettings);

                Approvals.Verify(groupsJson);
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

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryManifestAudience), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
                jsonResolver.Ignore(typeof(MediaWeek), "_Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var groups = _IInventoryRepository.GetStationInventoryGroupsByFileId(result.FileId);
                var groupsJson = IntegrationTestHelper.ConvertToJson(groups, jsonSettings);

                Approvals.Verify(groupsJson);
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

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryManifestAudience), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
                jsonResolver.Ignore(typeof(MediaWeek), "_Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var groups = _IInventoryRepository.GetStationInventoryGroupsByFileId(result.FileId);
                var groupsJson = IntegrationTestHelper.ConvertToJson(groups, jsonSettings);

                Approvals.Verify(groupsJson);
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

                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var problemsJson = IntegrationTestHelper.ConvertToJson(problems, jsonSettings);

                Approvals.Verify(problemsJson);
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

                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }
    }
}
