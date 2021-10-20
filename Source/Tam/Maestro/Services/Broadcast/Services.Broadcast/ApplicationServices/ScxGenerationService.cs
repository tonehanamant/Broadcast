using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.ApplicationServices
{
    public interface IScxGenerationService : IApplicationService
    {
        int QueueScxGenerationJob(InventoryScxDownloadRequest inventoryScxDownloadRequest, string userName, DateTime currentDate);
        [Queue("scxfilegeneration")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void ProcessScxGenerationJob(int jobId);
        void ProcessScxGenerationJob(ScxGenerationJob job, DateTime currentDate);
        List<ScxGenerationJob> GetQueuedJobs(int limit);
        void ResetJobStatusToQueued(int jobId);

        void RequeueScxGenerationJob(int jobId);

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

    public class ScxGenerationService :BroadcastBaseClass, IScxGenerationService
    {
        private readonly IScxGenerationJobRepository _ScxGenerationJobRepository;
        private readonly IProprietaryInventoryService _ProprietaryInventoryService;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBackgroundJobClient _BackgroundJobClient;

        private readonly IFileService _FileService;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly Lazy<bool> _EnableSharedFileServiceConsolidation;

        public ScxGenerationService(IDataRepositoryFactory broadcastDataRepositoryFactory, 
            IProprietaryInventoryService proprietaryInventoryService, 
            IFileService fileService,
            ISharedFolderService sharedFolderService,
            IQuarterCalculationEngine quarterCalculationEngine,
            IBackgroundJobClient backgroundJobClient,
            IFeatureToggleHelper featureToggleHelper, 
            IConfigurationSettingsHelper configurationSettingsHelper) 
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _ScxGenerationJobRepository = broadcastDataRepositoryFactory.GetDataRepository<IScxGenerationJobRepository>();
            _ProprietaryInventoryService = proprietaryInventoryService;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _BackgroundJobClient = backgroundJobClient;

            _FileService = fileService;
            _SharedFolderService = sharedFolderService;
            _EnableSharedFileServiceConsolidation = new Lazy<bool>(_GetEnableSharedFileServiceConsolidation);
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

        public void RequeueScxGenerationJob(int jobId)
        {
            _BackgroundJobClient.Enqueue<IScxGenerationService>(x => x.ProcessScxGenerationJob(jobId));
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

                    _SaveFiles(files, job.RequestedBy, currentDate);
                    _ScxGenerationJobRepository.SaveScxJobFiles(files, job);

                    // Save to the original folder until the feature is released.
                    // Delete this once the feature has been released.
                    if (!_EnableSharedFileServiceConsolidation.Value)
                    {
                        _SaveToFolder(files);
                    }

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

        private void _SaveFiles(List<InventoryScxFile> files, string createdBy, DateTime createdAt)
        {
            var dropFolderPath = GetDropFolderPath();
            const string fileMediaType = "application/xml";
            foreach (var file in files)
            {
                // have to copy the stream until we migrate to the SharedFileServiceand stop saving to the FileService.
                // Delete this once the feature has been released.
                var scxStreamCopy = _GetStreamCopy(file.ScxStream);

                var sharedFile = new SharedFolderFile
                {
                    FolderPath = dropFolderPath,
                    FileNameWithExtension = file.FileName,
                    FileMediaType = fileMediaType,
                    FileUsage = SharedFolderFileUsage.InventoryScx,
                    CreatedDate = createdAt,
                    CreatedBy = createdBy,
                    FileContent = scxStreamCopy
                };

                var savedId = _SharedFolderService.SaveFile(sharedFile);
                file.SharedFolderFileId = savedId;
            }
        }

        private Stream _GetStreamCopy(Stream originalStream)
        {
            var copiedStream = new MemoryStream();
            originalStream.CopyTo(copiedStream);
            return copiedStream;
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

            Tuple<string, Stream, string> result;

            if (_EnableSharedFileServiceConsolidation.Value)
            {
                var sharedFileId = _ScxGenerationJobRepository.GetSharedFolderFileIdForFile(fileId);

                if (sharedFileId.HasValue)
                {
                    _LogInfo($"Translated fileId '{fileId}' as sharedFolderFileId '{sharedFileId.Value}'");
                    var file = _SharedFolderService.GetFile(sharedFileId.Value);
                    result = _BuildPackageReturn(file.FileContent, file.FileNameWithExtension);
                    return result;
                }

                _LogWarning($"Given fileId '{fileId}' did not map to a sharedFolderFileId.  Checking with FileService.");
            }

            result = _GetFileFromFileService(fileId);
            return result;
        }

        private Tuple<string, Stream, string> _GetFileFromFileService(int fileId)
        {
            var fileName = _ScxGenerationJobRepository.GetScxFileName(fileId);
            var dropFolderPath = GetDropFolderPath();
            var filePaths = _FileService.GetFiles(dropFolderPath);
            var filePath = filePaths.FirstOrDefault(x => Path.GetFileName(x) == fileName);
            if (String.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("File not found.  Please regenerate.");
            }

            var fileStream = _FileService.GetFileStream(filePath);
            var result = _BuildPackageReturn(fileStream, fileName);
            return result;
        }

        private Tuple<string, Stream, string> _BuildPackageReturn(Stream fileStream, string fileName)
        {
            var fileMimeType = MimeMapping.GetMimeMapping(fileName);
            var result = new Tuple<string, Stream, string>(fileName, fileStream, fileMimeType);
            return result;
        }

        private void _SaveToFolder(List<InventoryScxFile> scxFiles)
        {
            var dropFolderPath = GetDropFolderPath();
            if (!_EnableSharedFileServiceConsolidation.Value)
            {
                _FileService.CreateDirectory(dropFolderPath);
                foreach (var scxFile in scxFiles)
                {
                    var path = Path.Combine(
                        dropFolderPath,
                        scxFile.FileName);

                    _FileService.Create(path, scxFile.ScxStream);
                }
            }
        }

        #region Helpers

        private string GetDropFolderPath()
		{
            var path = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.SCX_EXPORT_DIRECTORY);
            return path;
		}

        /// <summary>
        /// Transforms from dto to entity.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns><see cref="ScxFileGenerationDetail"/></returns>
        internal ScxFileGenerationDetail TransformFromDtoToEntity(ScxFileGenerationDetailDto dto)
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

        private bool _GetEnableSharedFileServiceConsolidation()
        {
            var result = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION);
            return result;
        }

        #endregion // #region Helpers
    }
}
