using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Common.Systems.LockTokens;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
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
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;
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
        List<LookupDto> GetAllGenres();
        LockResponse LockStation(int stationCode);
        ReleaseLockResponse UnlockStation(int stationCode);
        RatesInitialDataDto GetInitialRatesData();
        Decimal ConvertRateForSpotLength(decimal rateFor30s, int outputRateSpotLength);
        List<StationContact> FindStationContactsByName(string query);
        bool GetStationProgramConflicted(StationProgramConflictRequest conflict, int manifestId);
        bool DeleteProgram(int programId, string inventorySource, int stationCode, string user);

        bool HasSpotsAllocated(int programId);
        List<InventoryUploadHistoryDto> GetInventoryUploadHistory(int inventorySourceId);
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

    public class InventoryService : IInventoryService
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
        private readonly Dictionary<int, double> _SpotLengthCostMultipliers;
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        private readonly IStationInventoryGroupService _StationInventoryGroupService;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly INsiPostingBookService _NsiPostingBookService;

        private readonly ILockingEngine _LockingEngine;
        private readonly IDataLakeFileService _DataLakeFileService;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IImpressionsService _ImpressionsService;
        private readonly IOpenMarketFileImporter _OpenMarketFileImporter;
        private readonly IAudienceRepository _AudienceRepository;
        private readonly IFileService _FileService;

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
            IDataLakeFileService dataLakeFileService,
            ILockingEngine lockingEngine,
            IStationProcessingEngine stationProcessingEngine,
            IImpressionsService impressionsService,
            IOpenMarketFileImporter openMarketFileImporter,
            IFileService fileService)
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
            _SpotLengthMap = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            _SpotLengthCostMultipliers = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsAndCostMultipliers();
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _StationInventoryGroupService = stationInventoryGroupService;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            _NsiPostingBookService = nsiPostingBookService;
            _LockingEngine = lockingEngine;
            _DataLakeFileService = dataLakeFileService;
            _StationProcessingEngine = stationProcessingEngine;
            _ImpressionsService = impressionsService;
            _OpenMarketFileImporter = openMarketFileImporter;
            _AudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>();
            _FileService = fileService;
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
            if (!Path.GetExtension(request.FileName).Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
            {
                return new InventoryFileSaveResult
                {
                    Status = FileStatusEnum.Failed,
                    ValidationProblems = new List<string> { "Invalid file format. Please, provide a .xml File" }
                };
            }

            var inventorySource = _ParseInventorySourceOrDefault(request.InventorySource);

            _OpenMarketFileImporter.LoadFromSaveRequest(request);
            _OpenMarketFileImporter.CheckFileHash();

            InventoryFile inventoryFile = _OpenMarketFileImporter.GetPendingInventoryFile(_ParseInventorySourceOrDefault(request.InventorySource), userName, nowDate);

            inventoryFile.Id = _InventoryFileRepository.CreateInventoryFile(inventoryFile, userName, nowDate);

            var stationLocks = new List<IDisposable>();
            var lockedStationIds = new List<int>();
            try
            {
                _OpenMarketFileImporter.ExtractFileData(request.StreamData, inventoryFile);
                inventoryFile.FileStatus = _OpenMarketFileImporter.FileProblems.Any() ? FileStatusEnum.Failed : FileStatusEnum.Loaded;

                if (_OpenMarketFileImporter.FileProblems.Any())
                {
                    inventoryFile.FileStatus = FileStatusEnum.Failed;
                    inventoryFile.ValidationProblems.AddRange(_LoadValidationProblemsFromFileProblems(_OpenMarketFileImporter.FileProblems));

                    _InventoryRepository.AddValidationProblems(inventoryFile);
                    _InventoryFileRepository.UpdateInventoryFile(inventoryFile);

                    WriteErrorFileToDisk(inventoryFile.Id, request.FileName, inventoryFile.ValidationProblems);
                }
                else
                {
                    if (!inventoryFile.HasManifests())
                    {
                        inventoryFile.FileStatus = FileStatusEnum.Failed;
                        inventoryFile.ValidationProblems.Add("Unable to parse any file records.");

                        _InventoryRepository.AddValidationProblems(inventoryFile);
                        _InventoryFileRepository.UpdateInventoryFile(inventoryFile);

                        WriteErrorFileToDisk(inventoryFile.Id, request.FileName, inventoryFile.ValidationProblems);

                        return _SetInventoryFileSaveResult(inventoryFile);
                    }

                    _CreateUnknownStationsAndPopulate(inventoryFile, userName);

                    _OpenMarketFileImporter.FileProblems.AddRange(_InventoryFileValidator.ValidateInventoryFile(inventoryFile));

                    if (_OpenMarketFileImporter.FileProblems.Any())
                    {
                        inventoryFile.FileStatus = FileStatusEnum.Failed;
                        inventoryFile.ValidationProblems.AddRange(_LoadValidationProblemsFromFileProblems(_OpenMarketFileImporter.FileProblems));

                        _InventoryRepository.AddValidationProblems(inventoryFile);
                        _InventoryFileRepository.UpdateInventoryFile(inventoryFile);

                        WriteErrorFileToDisk(inventoryFile.Id, request.FileName, inventoryFile.ValidationProblems);

                        return _SetInventoryFileSaveResult(inventoryFile);
                    }

                    var fileStationsDict = inventoryFile
                       .GetAllManifests()
                       .Select(x => x.Station)
                       .GroupBy(s => s.Id)
                       .ToDictionary(g => g.First().Id, g => g.First().LegacyCallLetters);

                    using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                    {
                        _LockingEngine.LockStations(fileStationsDict, lockedStationIds, stationLocks);

                        _EnsureInventoryDaypartIds(inventoryFile);
                        _StationInventoryGroupService.AddNewStationInventoryGroups(inventoryFile);

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

            //send valid files to data lake
            if (!inventoryFile.ValidationProblems.Any())
            {
                try
                {
                    _DataLakeFileService.Save(request);
                }
                catch
                {
                    throw new ApplicationException("Unable to send file to Data Lake shared folder and e-mail reporting the error.");
                }
            }

            return _SetInventoryFileSaveResult(inventoryFile);
        }

        private void WriteErrorFileToDisk(int fileId, string fileName, List<string> validationErrors)
        {
            string path = $@"{BroadcastServiceSystemParameter.InventoryUploadErrorsFolder}\{fileId}_{fileName}.txt";

            _FileService.CreateTextFile(path, validationErrors);
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

        private List<string> _LoadValidationProblemsFromFileProblems(List<InventoryFileProblem> fileProblems)
        {
            string problemFormat = "Station: {0}; Program: {1}; Problem: {2}";
            return fileProblems
                .Select(x => string.Format(problemFormat
                    , string.IsNullOrWhiteSpace(x.StationLetters) ? "unknown" : x.StationLetters
                    , string.IsNullOrWhiteSpace(x.ProgramName) ? "unknown" : x.ProgramName
                    , x.ProblemDescription))
                .ToList();
        }

        private void _CreateUnknownStationsAndPopulate(InventoryFile inventoryFile, string userName)
        {
            var now = DateTime.Now;
            var manifestsWithUnknownStations = inventoryFile.GetAllManifests().Where(x => x.Station.Id == 0);
            var contactsWithUnknownStations = inventoryFile.StationContacts.Where(x => x.StationId == 0);
            var unknownStations = manifestsWithUnknownStations
                .Select(x => x.Station.CallLetters)
                .Union(contactsWithUnknownStations.Select(x => x.StationCallLetters))
                .Distinct(StringComparer.CurrentCultureIgnoreCase);
            var stationsToCreate = unknownStations.Select(stationName => new DisplayBroadcastStation
            {
                CallLetters = stationName,
                LegacyCallLetters = _StationProcessingEngine.StripStationSuffix(stationName),
                ModifiedDate = now
            });
            var newStations = _StationRepository.CreateStations(stationsToCreate, userName);
            var stationsDict = newStations.ToDictionary(x => x.CallLetters, x => x);

            foreach (var manifest in manifestsWithUnknownStations)
            {
                manifest.Station = stationsDict[manifest.Station.CallLetters];
            }

            foreach (var contact in contactsWithUnknownStations)
            {
                contact.StationId = stationsDict[contact.StationCallLetters].Id;
            }
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
            if (stationContact == null)
                throw new Exception("Cannot save station contact with invalid data.");

            using (new BomsLockManager(_SmsClient, new StationToken(stationContact.StationCode.Value)))
            {
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

                return true;
            }
        }

        public bool DeleteStationContact(string inventorySourceString, int stationContactId, string userName)
        {
            if (stationContactId <= 0)
                throw new Exception("Cannot delete station contact with invalid data.");

            var stationCode = _StationRepository.GetBroadcastStationCodeByContactId(stationContactId);

            using (new BomsLockManager(_SmsClient, new StationToken(stationCode)))
            using (var transaction = new TransactionScopeWrapper())
            {

                _broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>()
                    .DeleteStationContact(stationContactId);

                // update staion modified date
                _StationRepository.UpdateStation(stationCode, userName, DateTime.Now, _ParseInventorySourceOrDefault(inventorySourceString).Id);

                transaction.Complete();
            }

            return true;
        }

        public List<LookupDto> GetAllGenres()
        {
            return _GenreRepository.GetAllGenres();
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

        public LockResponse LockStation(int stationCode)
        {
            var station = _StationRepository.GetBroadcastStationByCode(stationCode);
            return _LockingEngine.LockStation(station.Id);
        }

        public ReleaseLockResponse UnlockStation(int stationCode)
        {
            var station = _StationRepository.GetBroadcastStationByCode(stationCode);
            return _LockingEngine.UnlockStation(station.Id);
        }

        public List<InventoryUploadHistoryDto> GetInventoryUploadHistory(int inventorySourceId)
        {
            var result = _InventoryRepository.GetInventoryUploadHistoryForInventorySource(inventorySourceId);
            return result;
        }

        public Tuple<string, Stream, string> DownloadErrorFile(int fileId)
        {
            var errorFiles = _FileService.GetFiles(BroadcastServiceSystemParameter.InventoryUploadErrorsFolder);

            //get the file by looking in the errors folder for a file with the name starting with the current id
            var filePath = errorFiles.Where(x => Path.GetFileName(x).StartsWith($"{fileId}_")).SingleOrDefault();
            if (filePath != null)
            {
                var fileName = Path.GetFileName(filePath).Replace($"{fileId}_", string.Empty);   //remove the added id from the filename
                Stream fileStream = _FileService.GetFileStream(filePath);
                var fileMimeType = MimeMapping.GetMimeMapping(fileName);
                return new Tuple<string, Stream, string>(fileName, fileStream, fileMimeType);
            }

            throw new ApplicationException($"Error file {fileId} not found!");

        }

        /// <summary>
        /// Generates an archive with inventory files that contained errors filtered by the list of ids passed
        /// </summary>
        /// <param name="fileIds">List of file ids to filter the files by</param>
        /// <returns>Returns a zip archive as stream and the zip name</returns>
        public Tuple<string, Stream> DownloadErrorFiles(List<int> fileIds)
        {
            string archiveFileName = $"InventoryErrorFiles_{DateTime.Now.ToString("MMddyyyyhhmmss")}.zip";
            var errorFiles = _FileService.GetFiles(BroadcastServiceSystemParameter.InventoryUploadErrorsFolder);
            Dictionary<string, string> errorsFilesToProcess = new Dictionary<string, string>();

            foreach (var id in fileIds)
            {
                //get the file by looking in the errors folder for a file with the name starting with the current id
                string filePath = errorFiles.Where(x => Path.GetFileName(x).StartsWith($"{id}_")).SingleOrDefault();
                if (filePath != null)
                {
                    string fileName = Path.GetFileName(filePath).Replace($"{id}_", string.Empty);   //remove the added id from the filename
                    errorsFilesToProcess.Add(filePath, fileName);
                }
            }

            Stream archiveFile = _FileService.CreateZipArchive(errorsFilesToProcess);
            return new Tuple<string, Stream>(archiveFileName, archiveFile);
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


    }
}
