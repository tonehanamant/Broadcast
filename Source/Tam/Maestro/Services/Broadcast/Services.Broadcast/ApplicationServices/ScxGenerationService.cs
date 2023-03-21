using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
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
        /// <summary>
        /// Gets the Scx OpenMarket Generation Job in Queue.
        /// </summary>
        /// <param name="inventoryScxOpenMarketsDownloadRequest">Inventory Scx OpenMarket Download Request.</param>
        /// <param name="userName">User Name.</param>
        /// <param name="currentDate">Current date.</param>
        /// <returns></returns>
        int QueueScxOpenMarketsGenerationJob(InventoryScxOpenMarketsDownloadRequest inventoryScxOpenMarketsDownloadRequest, string userName, DateTime currentDate);
        /// <summary>
        /// Process Scx Open Market Job.
        /// </summary>
        /// <param name="jobId">The source identifier.</param>
        /// <returns></returns>
        void ProcessScxOpenMarketGenerationJob(int jobId);
        /// <summary>
        /// Process Scx Open Market Job.
        /// </summary>
        /// <param name="job">Job.</param>
        /// <param name="currentDate">Job.</param>
        /// <returns></returns>
        void ProcessScxOpenMarketGenerationJob(ScxOpenMarketsGenerationJob job, DateTime currentDate);


        /// <summary>
        /// Gets the open market History Details
        /// </summary>
        /// <returns>Open market History</returns>
        List<ScxOpenMarketFileGenerationDetail> GetOpenMarketScxFileGenerationHistory();

        /// <summary>
        /// Download the open market scx file
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <returns></returns>
        Tuple<string, Stream, string> DownloadGeneratedScxFileForOpenMarket(int fileId);
    }

    public class ScxGenerationService :BroadcastBaseClass, IScxGenerationService
    {
        private readonly IScxGenerationJobRepository _ScxGenerationJobRepository;
        private readonly IProprietaryInventoryService _ProprietaryInventoryService;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBackgroundJobClient _BackgroundJobClient;

        private readonly IFileService _FileService;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IInventoryManagementApiClient _InventoryApiClient;
        private readonly Lazy<bool> _IsInventoryServiceMigrationEnabled;

        public ScxGenerationService(IDataRepositoryFactory broadcastDataRepositoryFactory, 
            IProprietaryInventoryService proprietaryInventoryService, 
            IFileService fileService,
            ISharedFolderService sharedFolderService,
            IQuarterCalculationEngine quarterCalculationEngine,
            IBackgroundJobClient backgroundJobClient,
            IFeatureToggleHelper featureToggleHelper, 
            IConfigurationSettingsHelper configurationSettingsHelper,
            IInventoryManagementApiClient inventoryApiClient) 
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _ScxGenerationJobRepository = broadcastDataRepositoryFactory.GetDataRepository<IScxGenerationJobRepository>();
            _ProprietaryInventoryService = proprietaryInventoryService;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _BackgroundJobClient = backgroundJobClient;

            _FileService = fileService;
            _SharedFolderService = sharedFolderService;
            _InventoryApiClient = inventoryApiClient;
            _IsInventoryServiceMigrationEnabled = new Lazy<bool>(() =>
          _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION));
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
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    _InventoryApiClient.ProcessScxGenerationJob(jobId);
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                ProcessScxGenerationJob(job, DateTime.Now);
            }           
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
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryApiClient.GetScxFileGenerationHistory(sourceId);
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                var detailDtos = _ScxGenerationJobRepository.GetScxFileGenerationDetails(sourceId);
                var details = TransformFromDtoToEntities(detailDtos);
                return details;
            }
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
            var sharedFileId = _ScxGenerationJobRepository.GetSharedFolderFileIdForFile(fileId);

            if (sharedFileId.HasValue)
            {
                _LogInfo($"Translated fileId '{fileId}' as sharedFolderFileId '{sharedFileId.Value}'");
                var file = _SharedFolderService.GetFile(sharedFileId.Value);
                result = _BuildPackageReturn(file.FileContent, file.FileNameWithExtension);
                return result;
            }

            _LogWarning($"Given fileId '{fileId}' did not map to a sharedFolderFileId.  Checking with FileService.");

            result = _GetFileFromFileService(fileId);
            return result;

        }

        public int QueueScxOpenMarketsGenerationJob(InventoryScxOpenMarketsDownloadRequest inventoryScxOpenMarketsDownloadRequest, string userName, DateTime currentDate)
        {
            var job = new ScxOpenMarketsGenerationJob
            {
                InventoryScxOpenMarketsDownloadRequest = inventoryScxOpenMarketsDownloadRequest,
                Status = BackgroundJobProcessingStatus.Queued,
                QueuedAt = currentDate,
                RequestedBy = userName
            };

            var jobId = _ScxGenerationJobRepository.AddOpenMarketJob(job);

            _BackgroundJobClient.Enqueue<IScxGenerationService>(x => x.ProcessScxOpenMarketGenerationJob(jobId));
            return jobId;
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

        public List<ScxOpenMarketFileGenerationDetail> GetOpenMarketScxFileGenerationHistory()
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryApiClient.GetOpenMarketScxFileGenerationHistory();
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                const int sourceId = 1;
                var detailDtos = _ScxGenerationJobRepository.GetOpenMarketScxFileGenerationDetails(sourceId);
                var result = TransformFromDtoToOpenMarketEntities(detailDtos);
                return result;
            }
        }

        private List<ScxOpenMarketFileGenerationDetail> TransformFromDtoToOpenMarketEntities(List<ScxOpenMarketFileGenerationDetailDto> dtos)
        {
            var entities = dtos.Select<ScxOpenMarketFileGenerationDetailDto, ScxOpenMarketFileGenerationDetail>(d =>
            {
                var entity = TransformFromDtoToOpenMarketEntity(d);
                return entity;
            }).GroupBy(x=>x.FileId).Select(x=>x.First())
                .OrderByDescending(s => s.GenerationRequestDateTime)
                .ToList();

            return entities;
        }

        private ScxOpenMarketFileGenerationDetail TransformFromDtoToOpenMarketEntity(ScxOpenMarketFileGenerationDetailDto scxOpenMarketFileGenerationDetail)
        {
            var processingStatus = EnumHelper.GetEnum<BackgroundJobProcessingStatus>(scxOpenMarketFileGenerationDetail.ProcessingStatusId);
            var quarters = new List<QuarterDetailDto>();
            if (scxOpenMarketFileGenerationDetail.StartDateTime.HasValue && scxOpenMarketFileGenerationDetail.EndDateTime.HasValue)
                quarters = _QuarterCalculationEngine.GetAllQuartersBetweenDates(scxOpenMarketFileGenerationDetail.StartDateTime.Value, scxOpenMarketFileGenerationDetail.EndDateTime.Value);

            var item = new ScxOpenMarketFileGenerationDetail
            {
                GenerationRequestDateTime = scxOpenMarketFileGenerationDetail.GenerationRequestDateTime,
                GenerationRequestedByUsername = scxOpenMarketFileGenerationDetail.GenerationRequestedByUsername,
                Affiliates = scxOpenMarketFileGenerationDetail.Affilates,
                DaypartCodes = scxOpenMarketFileGenerationDetail.DaypartCodes,
                QuarterDetails = quarters,
                ProcessingStatus = processingStatus,
                FileId = scxOpenMarketFileGenerationDetail.FileId,
                FileName = scxOpenMarketFileGenerationDetail.Filename
            };
            return item;
        }
        #endregion // #region Helpers
        public void ProcessScxOpenMarketGenerationJob(int jobId)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)  
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    _InventoryApiClient.ProcessScxOpenMarketGenerationJob(jobId);
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                var job = _ScxGenerationJobRepository.GetOpenMarketsJobById(jobId);

                ProcessScxOpenMarketGenerationJob(job, DateTime.Now);
            }
           
        }
        public void ProcessScxOpenMarketGenerationJob(ScxOpenMarketsGenerationJob job, DateTime currentDate)
        {
            if (job.Status != BackgroundJobProcessingStatus.Queued)
            {
                throw new ApplicationException($"Job with id {job.Id} already has status {job.Status}");
            }

            job.Status = BackgroundJobProcessingStatus.Processing;
            _ScxGenerationJobRepository.UpdateOpenMarketJob(job);

            Exception caught = null;

            using (var transaction = new TransactionScopeWrapper())
            {
                try
                {
                    var files = _ProprietaryInventoryService.GenerateScxOpenMarketFiles(job.InventoryScxOpenMarketsDownloadRequest);

                    _SaveFilesForOpenMarket(files, job.RequestedBy, currentDate);
                    _ScxGenerationJobRepository.SaveScxOpenMarketJobFiles(files, job);

                    job.Complete(currentDate);
                }
                catch (Exception ex)
                {
                    job.Status = BackgroundJobProcessingStatus.Failed;
                    caught = ex;
                }

                _ScxGenerationJobRepository.UpdateOpenMarketJob(job);

                transaction.Complete();

                if (caught != null)
                {
                    throw caught;
                }
            }
        }

        private void _SaveFilesForOpenMarket(List<OpenMarketInventoryScxFile> files, string createdBy, DateTime createdAt)
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

        /// <inheritdoc />
        public Tuple<string, Stream, string> DownloadGeneratedScxFileForOpenMarket(int fileId)
        {
            if (fileId == 0)
            {
                throw new Exception("No file id was supplied!");
            }
            Tuple<string, Stream, string> result;
            var sharedFileId = _ScxGenerationJobRepository.GetSharedFolderForOpenMarketFile(fileId);

            if (sharedFileId.HasValue)
            {
                _LogInfo($"Translated fileId '{fileId}' as sharedFolderFileId '{sharedFileId.Value}'");
                var file = _SharedFolderService.GetFile(sharedFileId.Value);
                result = _BuildPackageReturn(file.FileContent, file.FileNameWithExtension);
                return result;
            }

            _LogWarning($"Given fileId '{fileId}' did not map to a sharedFolderFileId.  Checking with FileService.");

            result = _GetOpenMarketFileFromFileService(fileId);
            return result;
        }

        private Tuple<string, Stream, string> _GetOpenMarketFileFromFileService(int fileId)
        {
            var fileName = _ScxGenerationJobRepository.GetOpenMarketScxFileName(fileId);
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
    }
}
