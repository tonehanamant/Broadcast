using Common.Services;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity;

namespace Services.Broadcast.IntegrationTests.Helpers
{
    public class InventoryFileTestHelper
    {
        private readonly IInventoryRatingsProcessingService _InventoryRatingsProcessingService;
        private readonly IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;
        private readonly IProprietaryInventoryService _ProprietaryInventoryService;
        private readonly IInventoryService _InventoryService;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;
        private readonly IInventoryProgramsProcessingService _InventoryProgramsProcessingService;
        private IInventoryRepository _InventoryRepository;

        public InventoryFileTestHelper()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceStub>();
            _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
            _InventoryFileRatingsJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRatingsJobsRepository>();
            _InventoryProgramsByFileJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsByFileJobsRepository>();
            _ProprietaryInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>();
            _InventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();
            _InventoryProgramsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
            _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        public InventoryFileSaveResult UploadProprietaryInventoryFile(
            string fileName, 
            DateTime? date = null, 
            bool processInventoryRatings = true,
            bool processInventoryProgramsJob = false)
        {
            var request = new FileRequest
            {
                StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                FileName = fileName
            };

            var now = date ?? new DateTime(2019, 02, 02);
            var result = _ProprietaryInventoryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);

            if (processInventoryRatings)
            {
                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.Id);
            }

            if (processInventoryProgramsJob)
            {
                var job = _InventoryProgramsByFileJobsRepository.GetLatestJob();
                _InventoryProgramsProcessingService.ProcessInventoryProgramsByFileJob(job.Id);
            }

            return result;
        }

        public int UploadOpenMarketInventoryFile(string fileName, DateTime? date = null, bool enhanceFilePrograms = false)
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

            return result.FileId;
        }
    }
}
