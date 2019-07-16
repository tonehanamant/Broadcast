using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IScxGenerationService : IApplicationService
    {
        int QueueScxGenerationJob(InventoryScxDownloadRequest inventoryScxDownloadRequest, string userName, DateTime currentDate);
        void ProcessScxGenerationJob(int jobId, DateTime currentDate);
        void ProcessScxGenerationJob(ScxGenerationJob job, DateTime currentDate);
        List<ScxGenerationJob> GetQueuedJobs(int limit);
        void ResetJobStatusToQueued(int jobId);
    }

    public class ScxGenerationService : IScxGenerationService
    {
        private readonly IScxGenerationJobRepository _ScxGenerationJobRepository;
        private readonly IProprietaryInventoryService _ProprietaryInventoryService;
        private readonly IFileService _FileService;

        public ScxGenerationService(IDataRepositoryFactory broadcastDataRepositoryFactory, 
            IProprietaryInventoryService proprietaryInventoryService, 
            IFileService fileService)
        {
            _ScxGenerationJobRepository = broadcastDataRepositoryFactory.GetDataRepository<IScxGenerationJobRepository>();
            _ProprietaryInventoryService = proprietaryInventoryService;
            _FileService = fileService;
        }

        public int QueueScxGenerationJob(InventoryScxDownloadRequest inventoryScxDownloadRequest, string userName, DateTime currentDate)
        {
            var job = new ScxGenerationJob
            {
                InventoryScxDownloadRequest = inventoryScxDownloadRequest,
                Status = BackgroundJobProcessingStatus.Queued,
                QueuedAt = currentDate,
                RequestedBy = userName
            };

            if (job.InventoryScxDownloadRequest.UnitNames == null)
                job.InventoryScxDownloadRequest.UnitNames = new List<string>();

            return _ScxGenerationJobRepository.AddJob(job);
        }

        public void ProcessScxGenerationJob(int jobId, DateTime currentDate)
        {
            var job = _ScxGenerationJobRepository.GetJobById(jobId);

            ProcessScxGenerationJob(job, currentDate);
        }

        public void ProcessScxGenerationJob(ScxGenerationJob job, DateTime currentDate)
        {
            if (job.Status != BackgroundJobProcessingStatus.Queued)
            {
                throw new ApplicationException($"Job with id {job.Id} already has status {job.Status}");
            }

            try
            {
                using (var transaction = new TransactionScopeWrapper())
                { 
                    var files = _ProprietaryInventoryService.GenerateScxFiles(job.InventoryScxDownloadRequest);

                    _SaveToFolder(files);

                    job.Complete(currentDate);

                    _ScxGenerationJobRepository.UpdateJob(job);

                    _ScxGenerationJobRepository.SaveScxJobFiles(files, job);

                    transaction.Complete();
                }
            }
            catch
            {
                job.Status = BackgroundJobProcessingStatus.Failed;
                _ScxGenerationJobRepository.UpdateJob(job);
                throw;
            }
        }      

        public List<ScxGenerationJob> GetQueuedJobs(int limit)
        {
            return _ScxGenerationJobRepository.GetJobsBatch(limit);
        }

        public void ResetJobStatusToQueued(int jobId)
        {
            var job = _ScxGenerationJobRepository.GetJobById(jobId);
            job.Status = BackgroundJobProcessingStatus.Queued;
            job.CompletedAt = null;
            _ScxGenerationJobRepository.UpdateJob(job);
        }

        private void _SaveToFolder(List<InventoryScxFile> scxFiles)
        {         
            foreach (var scxFile in scxFiles)
            {
                var path = Path.Combine(
                    BroadcastServiceSystemParameter.ScxGenerationFolder, 
                    scxFile.FileName);

                _FileService.Create(path, scxFile.ScxStream);
            }
        }
    }
}
