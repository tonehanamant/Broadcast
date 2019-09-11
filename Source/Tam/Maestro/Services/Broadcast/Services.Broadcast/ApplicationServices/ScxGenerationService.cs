using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Hangfire;
using Services.Broadcast.Helpers;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IScxGenerationService : IApplicationService
    {
        int QueueScxGenerationJob(InventoryScxDownloadRequest inventoryScxDownloadRequest, string userName, DateTime currentDate);
        [Queue("scxfilegeneration")]
        [DisableConcurrentExecution(300)]
        void ProcessScxGenerationJob(int jobId);
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
        /// <returns>Tuple : File Name, Content Stream, MIME Type Name</returns>
        Tuple<string, Stream, string> DownloadGeneratedScxFile(int fileId);
    }

    public class ScxGenerationService : IScxGenerationService
    {
        private readonly IScxGenerationJobRepository _ScxGenerationJobRepository;
        private readonly IProprietaryInventoryService _ProprietaryInventoryService;
        private readonly IFileService _FileService;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBackgroundJobClient _BackgroundJobClient;

        public ScxGenerationService(IDataRepositoryFactory broadcastDataRepositoryFactory, 
            IProprietaryInventoryService proprietaryInventoryService, 
            IFileService fileService,
            IQuarterCalculationEngine quarterCalculationEngine,
            IBackgroundJobClient backgroundJobClient)
        {
            _ScxGenerationJobRepository = broadcastDataRepositoryFactory.GetDataRepository<IScxGenerationJobRepository>();
            _ProprietaryInventoryService = proprietaryInventoryService;
            _FileService = fileService;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _BackgroundJobClient = backgroundJobClient;
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

            var jobId = _ScxGenerationJobRepository.AddJob(job);

            _BackgroundJobClient.Enqueue<IScxGenerationService>(x => x.ProcessScxGenerationJob(jobId));

            return jobId;
        }

        public void ProcessScxGenerationJob(int jobId)
        {
            var job = _ScxGenerationJobRepository.GetJobById(jobId);

            ProcessScxGenerationJob(job, DateTime.Now);
        }

        public void ProcessScxGenerationJob(ScxGenerationJob job, DateTime currentDate)
        {
            if (job.Status != BackgroundJobProcessingStatus.Queued)
            {
                throw new ApplicationException($"Job with id {job.Id} already has status {job.Status}");
            }

            job.Status = BackgroundJobProcessingStatus.Processing;
            _ScxGenerationJobRepository.UpdateJob(job);

            Exception caught = null;

            using (var transaction = new TransactionScopeWrapper())
            {
                try
                {
                    var files = _ProprietaryInventoryService.GenerateScxFiles(job.InventoryScxDownloadRequest);
                    _ScxGenerationJobRepository.SaveScxJobFiles(files, job);
                    _SaveToFolder(files);
                    job.Complete(currentDate);
                }
                catch (Exception ex)
                {
                    job.Status = BackgroundJobProcessingStatus.Failed;
                    caught = ex;
                }

                _ScxGenerationJobRepository.UpdateJob(job);

                transaction.Complete();

                if (caught != null)
                {
                    throw caught;
                }
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
            var detailDtos = _ScxGenerationJobRepository.GetScxFileGenerationDetails(sourceId);
            var details = TransformFromDtoToEntities(detailDtos);
            return details;
        }

        private List<ScxFileGenerationDetail> TransformFromDtoToEntities(List<ScxFileGenerationDetailDto> dtos)
        {
            var entities = dtos.Select<ScxFileGenerationDetailDto, ScxFileGenerationDetail>(d =>
                {
                    var entity = TransformFromDtoToEntity(d);
                    return entity;
                })
                .OrderByDescending(s => s.GenerationRequestDateTime)
                .ToList();

            return entities;
        }

        #endregion // #region ScxFileGenerationHistory

        /// <inheritdoc />
        public Tuple<string, Stream, string> DownloadGeneratedScxFile(int fileId)
        {
            if (fileId == 0)
            {
                throw new Exception("No file id was supplied!");
            }

            var fileName = _ScxGenerationJobRepository.GetScxFileName(fileId);

            var dropFolderPath = GetDropFolderPath();

            var filePaths = _FileService.GetFiles(dropFolderPath);
            var filePath = filePaths.FirstOrDefault(x => Path.GetFileName(x) == fileName);
            if (String.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception($"File not found!");
            }

            Stream fileStream = _FileService.GetFileStream(filePath);
            var fileMimeType = MimeMapping.GetMimeMapping(fileName);
            var result = new Tuple<string, Stream, string>(fileName, fileStream, fileMimeType);
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

        #region Helpers

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
        /// Transforms from dto to entity.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns><see cref="ScxFileGenerationDetail"/></returns>
        protected ScxFileGenerationDetail TransformFromDtoToEntity(ScxFileGenerationDetailDto dto)
        {
            var processingStatus = EnumHelper.GetEnum<BackgroundJobProcessingStatus>(dto.ProcessingStatusId);
            var quarters = new List<QuarterDetailDto>();
            if (dto.StartDateTime.HasValue && dto.EndDateTime.HasValue)
                quarters = _QuarterCalculationEngine.GetAllQuartersBetweenDates(dto.StartDateTime.Value, dto.EndDateTime.Value);

            var item = new ScxFileGenerationDetail
            {
                GenerationRequestDateTime = dto.GenerationRequestDateTime,
                GenerationRequestedByUsername = dto.GenerationRequestedByUsername,
                UnitName = dto.UnitName,
                DaypartCode = dto.DaypartCode,
                QuarterDetails = quarters,
                ProcessingStatus = processingStatus,
                FileId = dto.FileId,
                FileName = dto.Filename
            };
            return item;
        }

        #endregion // #region Helpers
    }
}
