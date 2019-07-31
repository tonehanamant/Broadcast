using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices.Scx;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// Gets the SCX file generation history.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        /// <returns></returns>
        List<ScxFileGenerationDetail> GetScxFileGenerationHistory(int sourceId);

        /// <summary>
        /// Downloads the generated SCX file.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        Tuple<string, Stream> DownloadGeneratedScxFile(int fileId);
    }

    public class ScxGenerationService : IScxGenerationService
    {
        private readonly IScxGenerationJobRepository _ScxGenerationJobRepository;
        private readonly IProprietaryInventoryService _ProprietaryInventoryService;
        private readonly IFileService _FileService;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;

        public ScxGenerationService(IDataRepositoryFactory broadcastDataRepositoryFactory, 
            IProprietaryInventoryService proprietaryInventoryService, 
            IFileService fileService)
        {
            _ScxGenerationJobRepository = broadcastDataRepositoryFactory.GetDataRepository<IScxGenerationJobRepository>();
            _ProprietaryInventoryService = proprietaryInventoryService;
            _FileService = fileService;
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
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

        #region ScxFileGenerationHistory

        /// <inheritdoc />
        public List<ScxFileGenerationDetail> GetScxFileGenerationHistory(int sourceId)
        {
            var historian = GetScxFileGenerationHistorian();
            var results = historian.GetScxFileGenerationHistory(sourceId);
            return results;
        }

        /// <summary>
        /// Gets the SCX file generation historian.
        /// </summary>
        /// <returns></returns>
        protected virtual IScxFileGenerationHistorian GetScxFileGenerationHistorian()
        {
            var repo = GetScxGenerationJobRepository();
            var quarterCalculationEngine = GetQuarterCalculationEngine();
            var dropFolderPath = GetDropFolderPath();
            var historian = new ScxFileGenerationHistorian(repo, quarterCalculationEngine, dropFolderPath);
            return historian;
        }

        /// <summary>
        /// Gets the quarter calculation engine.
        /// </summary>
        /// <returns></returns>
        protected virtual IQuarterCalculationEngine GetQuarterCalculationEngine()
        {
            var repoFactory = GetBroadcastDataRepositoryFactory();
            var aggCache = new MediaMonthAndWeekAggregateCache(repoFactory);
            var engine = new QuarterCalculationEngine(repoFactory, aggCache);
            return engine;
        }

        /// <summary>
        /// Gets the broadcast data repository factory.
        /// </summary>
        /// <returns></returns>
        protected IDataRepositoryFactory GetBroadcastDataRepositoryFactory()
        {
            return _BroadcastDataRepositoryFactory;
        }

        /// <summary>
        /// Gets the drop folder path.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDropFolderPath()
        {
            var result = BroadcastServiceSystemParameter.ScxGenerationFolder;
            return result;
        }

        /// <summary>
        /// Gets the SCX generation job repository.
        /// </summary>
        /// <returns></returns>
        protected virtual IScxGenerationJobRepository GetScxGenerationJobRepository()
        {
            return _ScxGenerationJobRepository;
        }

        #endregion // #region ScxFileGenerationHistory

        /// <inheritdoc />
        public Tuple<string, Stream> DownloadGeneratedScxFile(int fileId)
        {
            var repo = GetScxGenerationJobRepository();
            var fileName = repo.GetScxFileName(fileId);

            var dropFolderPath = GetDropFolderPath();

            var filePaths = _FileService.GetFiles(dropFolderPath);
            var filePath = filePaths.FirstOrDefault(x => Path.GetFileName(x) == fileName);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception($"File not found!");
            }

            Stream fileStream = _FileService.GetFileStream(filePath);
            var result = new Tuple<string, Stream>(fileName, fileStream);
            return result;
        }

        private void _SaveToFolder(List<InventoryScxFile> scxFiles)
        {
            var dropFolderPath = GetDropFolderPath();
            foreach (var scxFile in scxFiles)
            {
                var path = Path.Combine(
                    dropFolderPath, 
                    scxFile.FileName);

                _FileService.Create(path, scxFile.ScxStream);
            }
        }
    }
}
