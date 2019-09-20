using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.IO;

namespace Services.Broadcast.IntegrationTests.Helpers
{
    public class InventoryFileTestHelper
    {
        private IInventoryRatingsProcessingService _InventoryRatingsProcessingService;
        private IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;
        private IProprietaryInventoryService _ProprietaryInventoryService;
        private IInventoryService _InventoryService;

        public InventoryFileTestHelper()
        {
            _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
            _InventoryFileRatingsJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRatingsJobsRepository>();
            _ProprietaryInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>();
            _InventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();
        }

        public void UploadProprietaryInventoryFile(string fileName, DateTime? date = null, bool processInventoryRatings = true)
        {
            var request = new FileRequest
            {
                StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                FileName = fileName
            };

            var now = date ?? new DateTime(2019, 02, 02);
            _ProprietaryInventoryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);

            if (processInventoryRatings)
            {
                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.Id);
            }
        }

        public void UploadOpenMarketInventoryFile(string fileName, DateTime? date = null)
        {
            var request = new InventoryFileSaveRequest
            {
                StreamData = new FileStream($@".\Files\ImportingRateData\{fileName}", FileMode.Open, FileAccess.Read),
                FileName = fileName
            };

            var now = date ?? new DateTime(2019, 02, 02);

            var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", now);

            var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
            var source = _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.Id);
        }
    }
}
