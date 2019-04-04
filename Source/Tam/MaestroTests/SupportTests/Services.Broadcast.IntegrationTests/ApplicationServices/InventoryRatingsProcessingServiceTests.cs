using ApprovalTests.Reporters;
using Common.Services;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Microsoft.Practices.Unity;
using IntegrationTests.Common;
using Services.Broadcast.Entities.StationInventory;
using Tam.Maestro.Data.Entities;
using Newtonsoft.Json;
using ApprovalTests;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryRatingsProcessingServiceTests
    {
        private IBarterInventoryService _BarterService;
        //private IInventoryFileRepository _InventoryFileRepository;
        private IInventoryRepository _IInventoryRepository;
        //private IBarterRepository _BarterRepository;
        private IInventoryRatingsProcessingService _InventoryRatingsProcessingService;

        [TestFixtureSetUp]
        public void init()
        {
            try
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
                _BarterService = IntegrationTestApplicationServiceFactory.GetApplicationService<IBarterInventoryService>();
                //_InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
                _IInventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
                //_BarterRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBarterRepository>();
                _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryRatingsAfterBarterFileLoad()
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
                var jobs = _InventoryRatingsProcessingService.GetQueuedJobs(1);
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(jobs[0].id.Value);

                _VerifyInventoryGroups(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryRatingsAfterBarterFileLoad_OAndO()
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
                var jobs = _InventoryRatingsProcessingService.GetQueuedJobs(1);
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(jobs[0].id.Value);

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

        private void _VerifyInventoryGroups(int fileId)
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
    }
}
