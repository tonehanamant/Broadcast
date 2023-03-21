using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryService : IApplicationService
    {
        /// <summary>
        /// Saves an open market inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing an open market inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <param name="nowDate">Now date</param>
        /// <returns>InventoryFileSaveResult object</returns>
        InventoryFileSaveResult SaveInventoryFile(InventoryFileSaveRequest request, string userName, DateTime nowDate);
        List<StationContact> GetStationContacts(string inventorySource, int stationCode);
        bool SaveStationContact(StationContact stationContacts, string userName);
        bool DeleteStationContact(string inventorySourceString, int stationContactId, string userName);
        List<LookupDto> GetAllMaestroGenres();
        BroadcastLockResponse LockStation(int stationCode);
        BroadcastReleaseLockResponse UnlockStation(int stationCode);
        RatesInitialDataDto GetInitialRatesData();
        Decimal ConvertRateForSpotLength(decimal rateFor30s, int outputRateSpotLength);
        List<StationContact> FindStationContactsByName(string query);
        bool GetStationProgramConflicted(StationProgramConflictRequest conflict, int manifestId);
        bool DeleteProgram(int programId, string inventorySource, int stationCode, string user);
        bool HasSpotsAllocated(int programId);
        List<QuarterDetailDto> GetInventoryUploadHistoryQuarters(int inventorySourceId);
        List<InventoryUploadHistoryDto> GetInventoryUploadHistory(int inventorySourceId, int? quarter, int? year);
        Tuple<string, Stream, string> DownloadErrorFile(int fileId);

        /// <summary>
        /// Checks if the filepath is an excel file or not
        /// </summary>
        /// <param name="filepath">File path to check</param>
        /// <returns>True or false</returns>
        bool IsProprietaryFile(string filepath);

        /// <summary>
        /// Generates an archive with inventory files that contained errors filtered by the list of ids passed
        /// </summary>
        /// <param name="fileIds">List of file ids to filter the files by</param>
        /// <returns>Returns a zip archive as stream and the zip name</returns>
        Tuple<string, Stream> DownloadErrorFiles(List<int> fileIds);
    }

    public class InventoryService : BroadcastBaseClass, IInventoryService
    {
        private readonly IStationRepository _StationRepository;
        private readonly IDataRepositoryFactory _broadcastDataRepositoryFactory;
        private readonly IDaypartCache _DaypartCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IInventoryFileValidator _InventoryFileValidator;
        private readonly IStationContactsRepository _StationContactsRepository;
        private readonly IGenreRepository _GenreRepository;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly ISMSClient _SmsClient;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly Dictionary<int, int> _SpotLengthMap;
        private readonly Dictionary<int, decimal> _SpotLengthCostMultipliers;
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        private readonly IStationInventoryGroupService _StationInventoryGroupService;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly INsiPostingBookService _NsiPostingBookService;
        private readonly ILockingEngine _LockingEngine;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IImpressionsService _ImpressionsService;
        private readonly IOpenMarketFileImporter _OpenMarketFileImporter;
        private readonly IAudienceRepository _AudienceRepository;
        private readonly IFileService _FileService;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IInventoryRatingsProcessingService _InventoryRatingsService;
        private readonly IInventoryProgramsProcessingService _InventoryProgramsProcessingService;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly Lazy<bool> _EnableSaveIngestedInventoryFile;
        private readonly Lazy<bool> _IsInventoryServiceMigrationEnabled;
        private readonly IInventoryManagementApiClient _InventoryApiClient;
        public InventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IInventoryFileValidator inventoryFileValidator,
            IDaypartCache daypartCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ISMSClient smsClient,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            IStationInventoryGroupService stationInventoryGroupService,
            IBroadcastAudiencesCache audiencesCache,
            IRatingForecastService ratingForecastService,
            INsiPostingBookService nsiPostingBookService,
            ILockingEngine lockingEngine,
            IStationProcessingEngine stationProcessingEngine,
            IImpressionsService impressionsService,
            IOpenMarketFileImporter openMarketFileImporter,
            IFileService fileService,
            ISharedFolderService sharedFolderService,
            IInventoryRatingsProcessingService inventoryRatingsService,
            IInventoryProgramsProcessingService inventoryProgramsProcessingService,
            IDateTimeEngine dateTimeEngine,
            IInventoryManagementApiClient inventoryApiClient,
            IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
                : base(featureToggleHelper, configurationSettingsHelper)
        {
            _broadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _DaypartCache = daypartCache;
            _AudiencesCache = audiencesCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _InventoryFileValidator = inventoryFileValidator;
            _StationContactsRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>();
            _GenreRepository = _broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _SmsClient = smsClient;
            _InventoryFileRepository = _broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _SpotLengthMap = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsByDuration();
            _SpotLengthCostMultipliers = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsAndCostMultipliers(true);
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _StationInventoryGroupService = stationInventoryGroupService;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _NsiPostingBookService = nsiPostingBookService;
            _LockingEngine = lockingEngine;
            _StationProcessingEngine = stationProcessingEngine;
            _ImpressionsService = impressionsService;
            _OpenMarketFileImporter = openMarketFileImporter;
            _AudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>();
            _FileService = fileService;
            _SharedFolderService = sharedFolderService;
            _InventoryRatingsService = inventoryRatingsService;
            _InventoryProgramsProcessingService = inventoryProgramsProcessingService;
            _DateTimeEngine = dateTimeEngine;
            _InventoryApiClient = inventoryApiClient;
            _IsInventoryServiceMigrationEnabled = new Lazy<bool>(() =>
              _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION));

            _EnableSaveIngestedInventoryFile = new Lazy<bool>(_GetEnableSaveIngestedInventoryFile);
        }

        private bool _GetEnableSaveIngestedInventoryFile()
        {
            var toggle =
                _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SAVE_INGESTED_INVENTORY_FILE);
            return toggle;
        }

        public bool GetStationProgramConflicted(StationProgramConflictRequest conflict, int manifestId)
        {
            if (conflict.StartDate == DateTime.MinValue || conflict.EndDate == DateTime.MinValue || conflict.ConflictedProgramNewStartDate == DateTime.MinValue || conflict.ConflictedProgramNewEndDate == DateTime.MinValue)
            {
                throw new Exception(String.Format("Unable to parse start and end date values: {0}, {1}, {2}, {3}", conflict.StartDate, conflict.EndDate, conflict.ConflictedProgramNewStartDate, conflict.ConflictedProgramNewEndDate));
            }

            var hasDateRangeConflict = _DateRangesIntersect(conflict.ConflictedProgramNewStartDate, conflict.ConflictedProgramNewEndDate, conflict.StartDate, conflict.EndDate);

            if (conflict.Airtime == null)
                return hasDateRangeConflict;

            var program = _InventoryRepository.GetStationManifest(manifestId);
            var daypart = DaypartDto.ConvertDaypartDto(conflict.Airtime);

            daypart.Id = _DaypartCache.GetIdByDaypart(daypart);

            var hasDaypartConflict = false;

            foreach (var manifestDaypart in program.ManifestDayparts)
            {
                hasDaypartConflict = hasDaypartConflict || DisplayDaypart.Intersects(_DaypartCache.GetDisplayDaypart(manifestDaypart.Daypart.Id), daypart);
            }

            var hasConflict = hasDateRangeConflict && hasDaypartConflict;

            return hasConflict;
        }
        /// <summary>
        /// Saves an open market inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing an open market inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <param name="nowDate">Now date</param>
        /// <returns>InventoryFileSaveResult object</returns>
        public InventoryFileSaveResult SaveInventoryFile(InventoryFileSaveRequest request, string userName, DateTime nowDate)
        {
            InventoryFileSaveResult result;
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    InventoryFileSaveRequestDto fileRequest = new InventoryFileSaveRequestDto
                    {
                        FileName = request.FileName,
                        RawData = FileStreamExtensions.ConvertToBase64String(request.StreamData),
                        UserName = userName
                    };
                    result = _InventoryApiClient.SaveInventoryFile(fileRequest);
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    
                    if (result.Status == FileStatusEnum.Loaded)
                    {
                        _LogInfo("Queueing the secondary processing jobs.");
                        _InventoryRatingsService.QueueInventoryFileRatingsJob(result.FileId);
                        _InventoryProgramsProcessingService.QueueProcessInventoryProgramsByFileJob(result.FileId, userName);
                        _LogInfo("Completed queueing the secondary processing jobs.");
                    }
                    else
                    {
                        _LogInfo($"Not queueing the secondary processing jobs because the result status is not '{FileStatusEnum.Loaded}' it is '{result.Status}'.");
                    }
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }                
            }
            else
            {
                if (!Path.GetExtension(request.FileName).Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new InventoryFileSaveResult
                    {
                        Status = FileStatusEnum.Failed,
                        ValidationProblems = new List<string> { "Invalid file format. Please, provide a .xml File" }
                    };
                }

                var inventorySource = _InventoryRepository.GetInventorySource(BroadcastConstants.OpenMarketSourceId);

                _OpenMarketFileImporter.LoadFromSaveRequest(request);
                _OpenMarketFileImporter.CheckFileHash();

                InventoryFile inventoryFile = _OpenMarketFileImporter.GetPendingInventoryFile(inventorySource, userName, nowDate);

                inventoryFile.Id = _InventoryFileRepository.CreateInventoryFile(inventoryFile, userName, nowDate);

                var stationLocks = new List<IDisposable>();
                var lockedStationIds = new List<int>();
                try
                {
                    _OpenMarketFileImporter.ExtractFileData(request.StreamData, inventoryFile);
                    inventoryFile.FileStatus = _OpenMarketFileImporter.FileProblems.Any() ? FileStatusEnum.Failed : FileStatusEnum.Loaded;

                    if (_OpenMarketFileImporter.FileProblems.Any())
                    {
                        _ProcessFileWithProblems(inventoryFile, _LoadValidationProblemsFromFileProblems(_OpenMarketFileImporter.FileProblems), userName);
                    }
                    else
                    {
                        if (!inventoryFile.HasManifests())
                        {
                            _ProcessFileWithProblems(inventoryFile, new[] { "Unable to parse any file records." }, userName);
                            return _SetInventoryFileSaveResult(inventoryFile);
                        }

                        _OpenMarketFileImporter.FileProblems.AddRange(_InventoryFileValidator.ValidateInventoryFile(inventoryFile));

                        if (_OpenMarketFileImporter.FileProblems.Any())
                        {
                            _ProcessFileWithProblems(inventoryFile, _LoadValidationProblemsFromFileProblems(_OpenMarketFileImporter.FileProblems), userName);
                            return _SetInventoryFileSaveResult(inventoryFile);
                        }

                        var fileStationsDict = inventoryFile
                           .GetAllManifests()
                           .Select(x => x.Station)
                           .GroupBy(s => s.Id)
                           .ToDictionary(g => g.First().Id, g => g.First().LegacyCallLetters);

                        WebUtilityHelper.HtmlDecodeProgramNames(inventoryFile);

                        using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                        {
                            _LockingEngine.LockStations(fileStationsDict, lockedStationIds, stationLocks);

                            _EnsureInventoryDaypartIds(inventoryFile);
                            _StationInventoryGroupService.AddNewStationInventoryOpenMarket(inventoryFile);

                            _SaveInventoryFileContacts(userName, inventoryFile);
                            _StationRepository.UpdateStationList(fileStationsDict.Keys.ToList(), userName, nowDate, inventorySource.Id);
                            inventoryFile.FileStatus = FileStatusEnum.Loaded;
                            _InventoryFileRepository.UpdateInventoryFile(inventoryFile);

                            transaction.Complete();

                            _LockingEngine.UnlockStations(lockedStationIds, stationLocks);
                        }
                    }
                }
                catch (FileUploadException<InventoryFileProblem>)
                {
                    throw;
                }
                catch (BroadcastDuplicateInventoryFileException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // Try to update the status of the file if possible.
                    try
                    {
                        _LockingEngine.UnlockStations(lockedStationIds, stationLocks);
                        _InventoryFileRepository.UpdateInventoryFileStatus(inventoryFile.Id, FileStatusEnum.Failed);
                    }
                    catch { }

                    throw new BroadcastInventoryDataException($"Error loading new inventory file: {e.Message}", inventoryFile.Id, e);
                }

                if (!inventoryFile.ValidationProblems.Any())
                {
                    _InventoryRatingsService.QueueInventoryFileRatingsJob(inventoryFile.Id);
                    _InventoryProgramsProcessingService.QueueProcessInventoryProgramsByFileJob(inventoryFile.Id, userName);

                    if (_EnableSaveIngestedInventoryFile.Value)
                    {
                        _SaveUploadedInventoryFileToFileStore(request, inventoryFile.Id, userName);
                    }
                }

                result = _SetInventoryFileSaveResult(inventoryFile);
            }
            return result;
        }

        internal void _SaveUploadedInventoryFileToFileStore(InventoryFileSaveRequest request, int inventoryFileId, string userName)
        {

            const string fileMediaType = "application/xml";
            var folderPath = Path.Combine(_GetInventoryUploadFolder(), request.FileName);
            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = folderPath,
                FileNameWithExtension = request.FileName,
                FileMediaType = fileMediaType,
                FileUsage = SharedFolderFileUsage.UploadedInventory,
                CreatedDate = _DateTimeEngine.GetCurrentMoment(),
                CreatedBy = userName,
                FileContent = request.StreamData
            };

            _LogInfo($"Saving uploaded inventory file '{request.FileName}'...");
            var sharedFolderFileId = _SharedFolderService.SaveFile(sharedFolderFile);
            _LogInfo($"Saved uploaded inventory file '{request.FileName}' as ID '{sharedFolderFileId}'");

            _InventoryFileRepository.SaveUploadedFileId(inventoryFileId, sharedFolderFileId);
        }

        private void _ProcessFileWithProblems(InventoryFile inventoryFile, string[] problems, string userName)
        {
            inventoryFile.FileStatus = FileStatusEnum.Failed;
            inventoryFile.ValidationProblems.AddRange(problems);

            _InventoryRepository.AddValidationProblems(inventoryFile);
            _InventoryFileRepository.UpdateInventoryFile(inventoryFile);

            _WriteErrorFileToDisk(inventoryFile.Id, inventoryFile.FileName, inventoryFile.ValidationProblems, userName);
        }

        internal void _WriteErrorFileToDisk(int inventoryFileId, string fileName, List<string> validationErrors, string userName)
        {

            // create the text file
            var errorFileName = $"{fileName}.txt";
            var errorFileContent = new MemoryStream();
            var writer = new StreamWriter(errorFileContent);
            validationErrors.ForEach(l => writer.WriteLine(l));
            writer.Flush();
            errorFileContent.Seek(0, SeekOrigin.Begin);

            const string fileMediaType = "text/plain";
            var folderPath = Path.Combine(_GetInventoryUploadErrorsFolder(), errorFileName);
            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = folderPath,
                FileNameWithExtension = errorFileName,
                FileMediaType = fileMediaType,
                FileUsage = SharedFolderFileUsage.InventoryErrorFile,
                CreatedDate = _DateTimeEngine.GetCurrentMoment(),
                CreatedBy = userName,
                FileContent = errorFileContent
            };

            _LogInfo($"Saving error file '{errorFileName}' for inventory file '{fileName}'...");
            var sharedFolderFileId = _SharedFolderService.SaveFile(sharedFolderFile);
            _LogInfo($"Saved error file '{errorFileName}' for inventory file '{fileName}' as ID '{sharedFolderFileId}'");

            _InventoryFileRepository.SaveErrorFileId(inventoryFileId, sharedFolderFileId);
        }

        private InventoryFileSaveResult _SetInventoryFileSaveResult(InventoryFile file)
        {
            return new InventoryFileSaveResult()
            {
                FileId = file.Id,
                ValidationProblems = file.ValidationProblems,
                Status = file.FileStatus
            };
        }

        private string[] _LoadValidationProblemsFromFileProblems(List<InventoryFileProblem> fileProblems)
        {
            string problemFormat = "Station: {0}; Program: {1}; Problem: {2}";
            return fileProblems
                .Select(x => string.Format(problemFormat
                    , string.IsNullOrWhiteSpace(x.StationLetters) ? "unknown" : x.StationLetters
                    , string.IsNullOrWhiteSpace(x.ProgramName) ? "unknown" : x.ProgramName
                    , x.ProblemDescription))
                .OrderBy(x => x)
                .ToArray();
        }

        private void _AddRequestAudienceInfo(InventoryFileSaveRequest request, InventoryFile inventoryFile)
        {
            if (request.AudiencePricing == null || !request.AudiencePricing.Any())
                return;

            var audiencePricing = request.AudiencePricing;
            inventoryFile
                .InventoryGroups
                .SelectMany(g => g.Manifests)
                .ForEach(manifest =>
                    manifest.ManifestAudiences.AddRange(
                        audiencePricing.Select(ap => new StationInventoryManifestAudience()
                        {
                            IsReference = false,
                            Audience = new DisplayAudience() { Id = ap.AudienceId },
                            CPM = ap.Price
                        })));
        }

        private void _EnsureInventoryDaypartIds(InventoryFile inventoryFile)
        {
            // set daypart id
            inventoryFile.InventoryGroups.SelectMany(
                ig =>
                    ig.Manifests.SelectMany(
                        m => m.ManifestDayparts.Where(md => md.Daypart.Id == -1).Select(md => md.Daypart)))
                .ForEach(dd => { dd.Id = _DaypartCache.GetIdByDaypart(dd); });
        }

        private void _SaveInventoryFileContacts(string userName, InventoryFile inventoryFile)
        {
            var fileStationIds = inventoryFile.StationContacts.Select(m => m.StationId).Distinct().ToList();
            List<StationContact> existingStationContacts = _StationContactsRepository.GetStationContactsByStationIds(fileStationIds);

            var contactsUpdateList = inventoryFile.StationContacts.Intersect(existingStationContacts, StationContact.StationContactComparer).ToList();

            //Set the ID for those that exist already
            foreach (var updateContact in contactsUpdateList)
            {
                updateContact.Id = existingStationContacts.Single(c => StationContact.StationContactComparer.Equals(c, updateContact)).Id;
            }

            _StationContactsRepository.UpdateExistingStationContacts(contactsUpdateList, userName, inventoryFile.Id);

            var contactsCreateList =
                inventoryFile.StationContacts.Except(existingStationContacts, StationContact.StationContactComparer)
                    .ToList();
            _StationContactsRepository.CreateNewStationContacts(contactsCreateList, userName, inventoryFile.Id);

            // update modified date for each station
            var timeStamp = DateTime.Now;
            _StationRepository.UpdateStationList(fileStationIds, userName, timeStamp, inventoryFile.InventorySource.Id);
        }

        public List<StationContact> GetStationContacts(string inventorySource, int stationCode)
        {
            return _StationContactsRepository.GetStationContactsByStationCode(stationCode);
        }

        public List<StationContact> FindStationContactsByName(string query)
        {
            return _StationContactsRepository.GetLatestContactsByName(query);
        }

        private StationInventoryManifest _MapToStationInventoryManifest(StationProgram stationProgram)
        {
            const string householdAudienceCode = "HH";
            var householdeAudience = _AudienceRepository.GetDisplayAudienceByCode(householdAudienceCode);
            var inventorySource = _ParseInventorySource(stationProgram.RateSource);
            var manifestRates = _MapManifestRates(stationProgram);
            var spotLengthId = _GetSpotLengthIdForManifest(stationProgram);
            var displayDayparts = new List<DisplayDaypart>();

            if (stationProgram.Id == 0)
                displayDayparts = stationProgram.Airtimes.Select(DaypartDto.ConvertDaypartDto).ToList();

            var manifest = new StationInventoryManifest
            {
                Id = stationProgram.Id,
                Station = new DisplayBroadcastStation
                {
                    Code = stationProgram.StationCode,
                    Id = _StationRepository.GetBroadcastStationByCode(stationProgram.StationCode).Id
                },
                InventorySourceId = inventorySource.Id,
                SpotLengthId = spotLengthId,
                ManifestRates = manifestRates,
                ManifestDayparts = displayDayparts.Select(md =>
                    new StationInventoryManifestDaypart
                    {
                        Daypart = md,
                        Genres = stationProgram.Genres,
                        ProgramName = stationProgram.ProgramNames.FirstOrDefault() //TODO: This needs to be updated once UI can handle multipe program names
                    }
                ).ToList(),
                ManifestAudiencesReferences = new List<StationInventoryManifestAudience>
                {
                    new StationInventoryManifestAudience
                    {
                        Audience = householdeAudience,
                        Impressions = stationProgram.HouseHoldImpressions,
                        Rating = stationProgram.Rating,
                        IsReference = true
                    }
                }
            };

            _DaypartCache.SyncDaypartsToIds(displayDayparts);

            return manifest;
        }

        private int _GetSpotLengthIdForManifest(StationProgram stationProgram)
        {
            var spotLength15Id = _SpotLengthMap[15];
            var spotLength30Id = _SpotLengthMap[30];
            var has15SecondsRate = stationProgram.Rate15 != null;
            var has30SecondsRate = stationProgram.Rate30 != null;

            if (has15SecondsRate && !has30SecondsRate)
                return spotLength15Id;

            return spotLength30Id;
        }

        private List<StationInventoryManifestRate> _MapManifestRates(StationProgram stationProgram)
        {
            var spotLength15Id = _SpotLengthMap[15];
            var spotLength30Id = _SpotLengthMap[30];
            var manifestRates = new List<StationInventoryManifestRate>();
            var has15SecondsRate = stationProgram.Rate15 != null;

            if (has15SecondsRate)
            {
                manifestRates.Add(new StationInventoryManifestRate
                {
                    SpotLengthId = spotLength15Id,
                    SpotCost = stationProgram.Rate15.Value
                });
            }

            if (stationProgram.Rate30 == null)
                stationProgram.Rate30 = (decimal)(_SpotLengthCostMultipliers[spotLength30Id] / _SpotLengthCostMultipliers[spotLength15Id]) * stationProgram.Rate15.Value;

            manifestRates.AddRange(_GetManifestRatesFromMultipliers(stationProgram.Rate30.Value, has15SecondsRate));

            return manifestRates;
        }

        public bool SaveStationContact(StationContact stationContact, string userName)
        {
            bool isStationContactUpdated = false;
            if (stationContact == null)
                throw new Exception("Cannot save station contact with invalid data.");
            _LockingEngine.LockStationContact(stationContact.StationCode.Value);
            try
            {
                if (stationContact.StationId <= 0)
                    throw new Exception("Cannot save station contact with invalid station Id.");

                if (string.IsNullOrWhiteSpace(stationContact.Name))
                    throw new Exception("Cannot save station contact without specifying name value.");

                if (string.IsNullOrWhiteSpace(stationContact.Phone))
                    throw new Exception("Cannot save station contact without specifying phone value.");

                if (string.IsNullOrWhiteSpace(stationContact.Email))
                    throw new Exception("Cannot save station contact without specifying email value.");

                if (stationContact.Type <= 0)
                    throw new Exception("Cannot save station contact without specifying valid type value.");

                using (var transaction = new TransactionScopeWrapper())
                {
                    if (stationContact.Id <= 0)
                    {
                        _broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>()
                            .CreateNewStationContacts(
                                new List<StationContact>()
                                {
                                    stationContact
                                },
                                userName,
                                null);
                    }
                    else
                    {
                        _broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>()
                            .UpdateExistingStationContacts(
                                new List<StationContact>()
                                {
                                    stationContact
                                },
                                userName,
                                null);
                    }
                    _StationRepository.UpdateStation(stationContact.StationCode.Value, userName, DateTime.Now, _ParseInventorySourceOrDefault(stationContact.InventorySourceString).Id);

                    transaction.Complete();
                }
                isStationContactUpdated = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _LockingEngine.UnlockStationContact(stationContact.StationCode.Value);
            }
            return isStationContactUpdated;
        }

        public bool DeleteStationContact(string inventorySourceString, int stationContactId, string userName)
        {
            bool isStationContactDeleted = false;
            if (stationContactId <= 0)
                throw new Exception("Cannot delete station contact with invalid data.");

            var stationCode = _StationRepository.GetBroadcastStationCodeByContactId(stationContactId);
            _LockingEngine.LockStationContact(stationCode);
            try
            {
                if (stationCode <= 0)
                    throw new Exception("Cannot delete station contact with invalid station code.");

                using (var transaction = new TransactionScopeWrapper())
                {

                    _broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>()
                        .DeleteStationContact(stationContactId);

                    // update staion modified date
                    _StationRepository.UpdateStation(stationCode, userName, DateTime.Now, _ParseInventorySourceOrDefault(inventorySourceString).Id);

                    transaction.Complete();
                }
                isStationContactDeleted = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _LockingEngine.UnlockStationContact(stationCode);
            }
            return isStationContactDeleted;
        }

        public List<LookupDto> GetAllMaestroGenres()
        {
            return _GenreRepository.GetAllMaestroGenres();
        }

        private void _SetRateDataThrough(List<DisplayBroadcastStation> stations, DateTime currentDate)
        {
            var dateRangeThisQuarter = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER,
                currentDate);
            var dateRangeNextQuarter = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.NEXTQUARTER,
                currentDate);

            stations.ForEach(s =>
            {
                if (s.ManifestMaxEndDate != null)
                {
                    if (s.ManifestMaxEndDate.Value.Date == dateRangeThisQuarter.Item2.Date)
                        s.RateDataThrough = "This Quarter";
                    else
                    {
                        s.RateDataThrough = s.ManifestMaxEndDate.Value.Date == dateRangeNextQuarter.Item2.Date
                            ? "Next Quarter"
                            : s.ManifestMaxEndDate.Value.Date.ToShortDateString();
                    }
                }
                else
                {
                    s.RateDataThrough = "-";
                }
            });
        }

        private InventorySource _ParseInventorySource(string sourceString)
        {
            var inventorySource = _InventoryRepository.GetInventorySourceByName(sourceString);

            if (inventorySource == null)
            {
                throw new ArgumentException("Invalid inventory source string");
            }

            return inventorySource;
        }

        private InventorySource _ParseInventorySourceOrDefault(string sourceString)
        {
            const string defaultInventorySource = "Open Market";

            if (string.IsNullOrWhiteSpace(sourceString))
            {
                return _InventoryRepository.GetInventorySourceByName(defaultInventorySource);
            }

            return _InventoryRepository.GetInventorySourceByName(sourceString);
        }

        public RatesInitialDataDto GetInitialRatesData()
        {
            var ratesInitialDataDto = new RatesInitialDataDto
            {
                RatingBooks = _NsiPostingBookService.GetPostingBookLongMonthNameAndYear(),
                PlaybackTypes = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalPlaybackType>(),
                Audiences = _AudiencesCache.GetAllLookups(),
                DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3
            };

            return ratesInitialDataDto;
        }

        public Decimal ConvertRateForSpotLength(decimal rateFor30s, int outputRateSpotLength)
        {
            if (!_SpotLengthMap.ContainsKey(outputRateSpotLength))
            {
                throw new ArgumentException(String.Format("Unable to convert rate to unknown spot length {0}",
                    outputRateSpotLength));
            }

            var spotLengthId = _SpotLengthMap[outputRateSpotLength];

            if (!_SpotLengthCostMultipliers.ContainsKey(spotLengthId))
            {
                throw new InvalidOperationException(string.Format("No conversion factor available for spot length {0}",
                    outputRateSpotLength));
            }

            var costMultiplier = _SpotLengthCostMultipliers[spotLengthId];
            var result = rateFor30s * (Decimal)costMultiplier;
            return result;
        }

        private IEnumerable<StationInventoryManifestRate> _GetManifestRatesFromMultipliers(decimal rate, bool has15SecondsRate)
        {

            var manifestRates = new List<StationInventoryManifestRate>();

            foreach (var spotLength in _SpotLengthMap)
            {
                if (spotLength.Key == 15 && has15SecondsRate)
                {
                    //skip 15s, already have it
                    continue;
                }

                var manifestRate = new StationInventoryManifestRate();
                manifestRate.SpotLengthId = _SpotLengthMap[spotLength.Key];
                manifestRate.SpotCost = rate * (decimal)_SpotLengthCostMultipliers[spotLength.Value];
                manifestRates.Add(manifestRate);
            }

            return manifestRates;
        }

        public bool DeleteProgram(int programId, string inventorySource, int stationCode, string user)
        {
            _InventoryRepository.RemoveManifest(programId);
            _StationRepository.UpdateStation(stationCode, user, DateTime.Now, _ParseInventorySourceOrDefault(inventorySource).Id);
            return true;
        }

        public bool HasSpotsAllocated(int programId)
        {
            return _InventoryRepository.HasSpotsAllocated(programId);
        }

        private static bool _DateRangesIntersect(DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2)
        {
            return (startDate1 >= startDate2 && startDate1 <= endDate2)
                   || (endDate1 >= startDate2 && endDate1 <= endDate2)
                   || (startDate1 < startDate2 && endDate1 > endDate2);
        }

        public BroadcastLockResponse LockStation(int stationCode)
        {
            var station = _StationRepository.GetBroadcastStationByCode(stationCode);
            return _LockingEngine.LockStation(station.Id);
        }

        public BroadcastReleaseLockResponse UnlockStation(int stationCode)
        {
            var station = _StationRepository.GetBroadcastStationByCode(stationCode);
            return _LockingEngine.UnlockStation(station.Id);
        }

        public List<InventoryUploadHistoryDto> GetInventoryUploadHistory(int inventorySourceId, int? quarter, int? year)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryApiClient.GetInventoryUploadHistory(inventorySourceId, quarter, year);
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
                var result = new List<InventoryUploadHistoryDto>();
                var quarterDateRange = _QuarterCalculationEngine.GetQuarterDateRange(quarter, year);
                var uploadHistory = _InventoryRepository.GetInventoryUploadHistoryForInventorySource(inventorySourceId, quarterDateRange.Start, quarterDateRange.End);

                foreach (var uploadHistoryItem in uploadHistory)
                {
                    var uploadHistoryItemDto = new InventoryUploadHistoryDto
                    {
                        FileId = uploadHistoryItem.FileId,
                        UploadDateTime = uploadHistoryItem.UploadDateTime,
                        Username = uploadHistoryItem.Username,
                        Filename = uploadHistoryItem.Filename,
                        DaypartCodes = uploadHistoryItem.DaypartCodes,
                        EffectiveDate = uploadHistoryItem.EffectiveDate,
                        EndDate = uploadHistoryItem.EndDate,
                        HutBook = uploadHistoryItem.HutBook,
                        ShareBook = uploadHistoryItem.ShareBook,
                        Rows = uploadHistoryItem.Rows,
                        Status = _GetUploadHistoryStatus(uploadHistoryItem)
                    };

                    if (uploadHistoryItem.EffectiveDate.HasValue &&
                        uploadHistoryItem.EndDate.HasValue)
                    {
                        uploadHistoryItemDto.Quarters = _QuarterCalculationEngine.GetAllQuartersBetweenDates(uploadHistoryItem.EffectiveDate.Value, uploadHistoryItem.EndDate.Value);
                    }

                    result.Add(uploadHistoryItemDto);
                }

                return result;
            }
        }

        private string _GetUploadHistoryStatus(InventoryUploadHistory inventoryUploadHistory)
        {
            if (inventoryUploadHistory.FileLoadStatus == FileStatusEnum.Failed)
            {
                return "Validation Error";
            }

            if (inventoryUploadHistory.RatingProcessingJobStatus == BackgroundJobProcessingStatus.Failed)
            {
                return "Processing Error";
            }

            if (inventoryUploadHistory.RatingProcessingJobStatus == BackgroundJobProcessingStatus.Succeeded)
            {
                return "Succeeded";
            }

            return "Processing";
        }

        public Tuple<string, Stream, string> DownloadErrorFile(int fileId)
        {
            Tuple<string, Stream, string> result;
            // get the file 
            var inventoryFileDetails = _GetInventoryFileById(fileId);
            if (inventoryFileDetails.ErrorFileSharedFolderFileId.HasValue)
            {
                _LogInfo($"Translated fileId '{fileId}' as errorFileSharedFolderFileId '{inventoryFileDetails.ErrorFileSharedFolderFileId.Value}'");
                var file = _SharedFolderService.GetFile(inventoryFileDetails.ErrorFileSharedFolderFileId.Value);
                result = _BuildPackageReturnForSingleFile(file.FileContent, file.FileNameWithExtension);
                return result;
            }

            result = _RetrieveErrorFileWithFileService(fileId);
            return result;
        }

        private Tuple<string, Stream, string> _RetrieveErrorFileWithFileService(int fileId)
        {
            var fileInfo = _GetExistingInventoryErrorFileInfo(fileId);
            Stream fileStream = _FileService.GetFileStream(fileInfo.FilePath);

            var result = _BuildPackageReturnForSingleFile(fileStream, fileInfo.FriendlyFileName);
            return result;
        }

        private Tuple<string, Stream, string> _BuildPackageReturnForSingleFile(Stream fileStream, string fileName)
        {
            var fileMimeType = MimeMapping.GetMimeMapping(fileName);
            var result = new Tuple<string, Stream, string>(fileName, fileStream, fileMimeType);
            return result;
        }

        private Tuple<string, Stream> _BuildPackageReturnForArchiveFile(string archiveFileName, Stream archiveFile)
        {
            var result = new Tuple<string, Stream>(archiveFileName, archiveFile);
            return result;
        }

        /// <summary>
        /// Generates an archive with inventory files that contained errors filtered by the list of ids passed
        /// </summary>
        /// <param name="fileIds">List of file ids to filter the files by</param>
        /// <returns>Returns a zip archive as stream and the zip name</returns>
        public Tuple<string, Stream> DownloadErrorFiles(List<int> fileIds)
        {
            var archiveFileName = $"InventoryErrorFiles_{_DateTimeEngine.GetCurrentMoment().ToString("MMddyyyyhhmmss")}.zip";
            Tuple<string, Stream> result;
            var sharedFolderFileIds = _InventoryFileRepository.GetErrorFileSharedFolderFileIds(fileIds);
            var archiveStream = _SharedFolderService.CreateZipArchive(sharedFolderFileIds);
            result = _BuildPackageReturnForArchiveFile(archiveFileName, archiveStream);
            return result;
        }

        private InventoryFile _GetInventoryFileById(int fileId)
        {
            try
            {
                var fileDetails = _InventoryFileRepository.GetInventoryFileById(fileId);
                return fileDetails;
            }
            catch (Exception ex)
            {
                var message = $"File record for id '{fileId}' not found.";
                _LogError(message, ex);
                throw new InvalidOperationException(message, ex);
            }
        }

        private ExistingInventoryErrorFileInfo _GetExistingInventoryErrorFileInfo(int fileId)
        {
            var fileDetails = _GetInventoryFileById(fileId);

            var fileNameSuffix = string.Empty;
            if (Path.GetExtension(fileDetails.FileName)?.Equals(".XML", StringComparison.OrdinalIgnoreCase) == true)
            {
                fileNameSuffix = ".txt";
            }
            var transformedFileName = $"{fileDetails.Id}_{fileDetails.FileName}{fileNameSuffix}";
            var filePath = Path.Combine(_GetInventoryUploadErrorsFolder(), transformedFileName);

            if (_FileService.Exists(filePath) == false)
            {
                var message = $"File '{fileDetails.FileName}' with id '{fileId}' not found.";
                _LogError(message);
                throw new FileNotFoundException(message, fileDetails.FileName);
            }

            var friendlyFileName = $"{fileDetails.FileName}{fileNameSuffix}";
            var result = new ExistingInventoryErrorFileInfo
            {
                FriendlyFileName = friendlyFileName,
                FilePath = filePath
            };
            return result;
        }

        /// <summary>
        /// Checks if the filepath is an excel file or not
        /// </summary>
        /// <param name="filepath">File path to check</param>
        /// <returns>True or false</returns>
        public bool IsProprietaryFile(string filepath)
        {
            return Path.GetExtension(filepath).Equals(".xlsx", StringComparison.InvariantCultureIgnoreCase);
        }

        public List<QuarterDetailDto> GetInventoryUploadHistoryQuarters(int inventorySourceId)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryApiClient.GetInventoryUploadHistoryQuarters(inventorySourceId);
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
                var uploadHistoryDates = _InventoryRepository.GetInventoryUploadHistoryDatesForInventorySource(inventorySourceId);
                var quarters = _QuarterCalculationEngine.GetQuartersForDateRanges(uploadHistoryDates);
                return quarters.OrderByDescending(q => q.Year).ThenByDescending(q => q.Quarter).ToList();
            }
        }

        /// <summary>
        /// For unit testing.  Abstracts from the static Configuration Service.
        /// </summary>
        protected virtual string _GetInventoryUploadErrorsFolder()
        {
            var path = Path.Combine(_GetInventoryUploadFolder()
                , BroadcastConstants.FolderNames.INVENTORY_UPLOAD_ERRORS);
            return path;
        }

        protected virtual string _GetInventoryUploadFolder()
        {
            var path = Path.Combine(_GetBroadcastAppFolder()
                , BroadcastConstants.FolderNames.INVENTORY_UPLOAD);
            return path;
        }

        private class ExistingInventoryErrorFileInfo
        {
            /// <summary>
            /// The friendly name of the file without the system prefix and suffix. 
            /// </summary>
            public string FriendlyFileName { get; set; }

            /// <summary>
            /// The path to the file.
            /// </summary>
            public string FilePath { get; set; }
        }
    }
}
