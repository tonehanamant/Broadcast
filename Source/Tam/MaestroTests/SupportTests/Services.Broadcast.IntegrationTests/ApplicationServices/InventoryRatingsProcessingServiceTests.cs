using ApprovalTests.Reporters;
using Common.Services;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.IO;
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
        private IProprietaryInventoryService _ProprietaryService;
        private IInventoryRepository _IInventoryRepository;
        private IInventoryRatingsProcessingService _InventoryRatingsProcessingService;
        private IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;

        [TestFixtureSetUp]
        public void init()
        {
            try
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
                _ProprietaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>();
                _IInventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
                _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
                _InventoryFileRatingsJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRatingsJobsRepository>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryRatingsAfterProprietaryFileLoad()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_ValidFormat_SingleBook.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);
                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.id.Value);

                _VerifyFileInventoryManifests(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryRatingsAfterProprietaryFileLoad_HH()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_ValidFormat_SingleBook_HH.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);
                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.id.Value);

                _VerifyFileInventoryManifests(result.FileId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryRatingsAfterProprietaryFileLoad_OAndO()
        {
            const string fileName = @"ProprietaryDataFiles\OAndO_ValidFile1.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);
                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.id.Value);

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
    }
}
