using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Common.Systems.LockTokens;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Transactions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.ApplicationServices
{
    public enum RatesTimeframe
    {
        TODAY,
        LASTQUARTER,
        THISQUARTER,
        NEXTQUARTER
    }

    public interface IInventoryService : IApplicationService
    {
        List<DisplayBroadcastStation> GetStations(string rateSource, DateTime currentDate);
        List<DisplayBroadcastStation> GetStationsWithFilter(string rateSource, string filterValue, DateTime today);
        InventoryFileSaveResult SaveInventoryFile(InventoryFileSaveRequest request);
        StationDetailDto GetStationDetailByCode(string inventorySource, int stationCode);
        List<StationContact> GetStationContacts(string inventorySource, int stationCode);
        bool SaveStationContact(StationContact stationContacts, string userName);
        bool DeleteStationContact(string inventorySourceString, int stationContactId, string userName);
        List<LookupDto> GetAllGenres();
        LockResponse LockStation(int stationCode);
        ReleaseLockResponse UnlockStation(int stationCode);
        RatesInitialDataDto GetInitialRatesData();
        Decimal ConvertRateForSpotLength(decimal rateFor30s, int outputRateSpotLength);
        List<StationContact> FindStationContactsByName(string query);
        bool SaveProgram(StationProgram stationProgram, string userName);
        List<StationProgram> GetStationPrograms(
            string inventorySourceString,
            int stationCode,
            DateTime startDate,
            DateTime endDate);
        List<StationProgram> GetStationPrograms(
            string inventorySourceString,
            int stationCode,
            string timeFrame,
            DateTime currentDate);
        List<StationProgram> GetAllStationPrograms(string inventorySource, int stationCode);
        bool GetStationProgramConflicted(StationProgramConflictRequest conflict, int manifestId);
        List<StationProgram> GetStationProgramConflicts(StationProgramConflictRequest conflict);
        bool DeleteProgram(int programId, string inventorySource, int stationCode, string user);
        bool ExpireManifest(int programId, DateTime endDate, string inventorySource, int stationCode, string user);
        bool HasSpotsAllocated(int programId);        
    }

    public class InventoryService : IInventoryService
    {
        private readonly IStationRepository _stationRepository;
        private readonly IDataRepositoryFactory _broadcastDataRepositoryFactory;
        private readonly IDaypartCache _daypartCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IInventoryFileValidator _inventoryFileValidator;
        private readonly IStationContactsRepository _stationContactsRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IInventoryFileImporterFactory _inventoryFileImporterFactory;
        private readonly ISMSClient _SmsClient;
        private readonly IInventoryFileRepository _inventoryFileRepository;
        private readonly ILockingManagerApplicationService _LockingManager;
        private readonly Dictionary<int, int> _SpotLengthMap;
        private readonly Dictionary<int, double> _SpotLengthCostMultipliers;
        private readonly IProprietarySpotCostCalculationEngine _proprietarySpotCostCalculationEngine;
        private readonly IStationInventoryGroupService _stationInventoryGroupService;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IRatingForecastService _RatingForecastService;
        private readonly INsiPostingBookService _NsiPostingBookService;
        private readonly IDataLakeFileService _DataLakeFileService;

        public InventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IInventoryFileValidator inventoryFileValidator,
            IDaypartCache daypartCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IInventoryFileImporterFactory inventoryFileImporterFactory,
            ISMSClient smsClient,
            ILockingManagerApplicationService lockingManager,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            IStationInventoryGroupService stationInventoryGroupService,
            IBroadcastAudiencesCache audiencesCache,
            IRatingForecastService ratingForecastService,
            INsiPostingBookService nsiPostingBookService,
            IDataLakeFileService dataLakeFileService)
        {
            _broadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _stationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _daypartCache = daypartCache;
            _AudiencesCache = audiencesCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _inventoryFileValidator = inventoryFileValidator;
            _stationContactsRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>();
            _genreRepository = _broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _inventoryFileImporterFactory = inventoryFileImporterFactory;
            _SmsClient = smsClient;
            _inventoryFileRepository = _broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _LockingManager = lockingManager;
            _SpotLengthMap =
                broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            _SpotLengthCostMultipliers =
                broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsAndCostMultipliers();
            _proprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _stationInventoryGroupService = stationInventoryGroupService;
            _inventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _RatingForecastService = ratingForecastService;
            _NsiPostingBookService = nsiPostingBookService;
            _DataLakeFileService = dataLakeFileService;
        }

        public List<DisplayBroadcastStation> GetStations(string rateSource, DateTime currentDate)
        {
            var stations = _stationRepository.GetBroadcastStationsWithFlightWeeksForRateSource(_ParseInventorySourceOrDefault(rateSource));

            //set null every modified date that looks like 0001-01-01T00:00:00 so the UI knows where to put dashes
            stations.Where(x => x.ModifiedDate == DateTime.MinValue).ForEach(x => x.ModifiedDate = null);

            _SetRateDataThrough(stations, currentDate);
            return stations;
        }

        public List<DisplayBroadcastStation> GetStationsWithFilter(string rateSourceString, string filterValue,
            DateTime today)
        {
            var parseSuccess = Enum.TryParse(filterValue, true, out DisplayBroadcastStation.StationFilter filter);

            if (!parseSuccess)
            {
                throw new ArgumentException(string.Format("Invalid station filter parameter: {0}", filterValue));
            }

            var inventorySource = _ParseInventorySourceOrDefault(rateSourceString);

            var isIncluded = (filter == DisplayBroadcastStation.StationFilter.WithTodaysData);
            var stations = _stationRepository.GetBroadcastStationsByDate(inventorySource.Id, today, isIncluded);

            //set null every modified date that looks like 0001-01-01T00:00:00 so the UI knows where to put dashes
            stations.Where(x => x.ModifiedDate == DateTime.MinValue).ForEach(x => x.ModifiedDate = null);

            _SetRateDataThrough(stations, today);

            return stations;
        }

        public List<StationProgram> GetStationPrograms(string inventorySourceString, int stationCode, DateTime startDate,
            DateTime endDate)
        {
            var inventorySource = _ParseInventorySource(inventorySourceString);
            var stationManifests = _inventoryRepository.GetStationManifestsBySourceStationCodeAndDates(inventorySource,
                stationCode, startDate, endDate);
            _SetDisplayDaypartForInventoryManifest(stationManifests);
            _SetAudienceForInventoryManifest(stationManifests);
            return _GetStationProgramsFromStationInventoryManifest(stationManifests);
        }

        public List<StationProgram> GetStationPrograms(string inventorySourceString, int stationCode, string timeFrame,
            DateTime currentDate)
        {
            RatesTimeframe timeFrameValue;

            try
            {
                var succeeded = Enum.TryParse(timeFrame.ToUpper(), true, out timeFrameValue);
                if (!succeeded)
                    throw new ArgumentException(string.Format("Invalid timeframe specified: {0}.", timeFrame));
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Unable to parse rate timeframe for {0}: {1}", timeFrame, e.Message));
            }

            var dateRange = _QuarterCalculationEngine.GetDatesForTimeframe(timeFrameValue, currentDate);

            return GetStationPrograms(inventorySourceString, stationCode, dateRange.Item1, dateRange.Item2);
        }

        public List<StationProgram> GetAllStationPrograms(string inventorySourceString, int stationCode)
        {
            var inventorySource = _ParseInventorySource(inventorySourceString);
            var stationManifests = _inventoryRepository.GetStationManifestsBySourceAndStationCode(inventorySource,
                stationCode);
            _SetDisplayDaypartForInventoryManifest(stationManifests);
            _SetAudienceForInventoryManifest(stationManifests);
            return _GetStationProgramsFromStationInventoryManifest(stationManifests);
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

            var program = _inventoryRepository.GetStationManifest(manifestId);
            var daypart = DaypartDto.ConvertDaypartDto(conflict.Airtime);

            daypart.Id = _daypartCache.GetIdByDaypart(daypart);

            var hasDaypartConflict = false;

            foreach (var manifestDaypart in program.ManifestDayparts)
            {
                hasDaypartConflict = hasDaypartConflict || DisplayDaypart.Intersects(_daypartCache.GetDisplayDaypart(manifestDaypart.Daypart.Id), daypart);
            }

            var hasConflict = hasDateRangeConflict && hasDaypartConflict;

            return hasConflict;
        }

        private InventoryFileSaveResult _SetFileProblemWarnings(int fileId, List<InventoryFileProblem> fileProblems)
        {
            if (fileProblems.Any())
            {
                throw new FileUploadException<InventoryFileProblem>(fileProblems);
            }

            var ret = new InventoryFileSaveResult()
            {
                FileId = fileId
            };
            return ret;
        }

        public InventoryFileSaveResult SaveInventoryFile(InventoryFileSaveRequest request)
        {
            if (request.EffectiveDate == DateTime.MinValue)
            {
                request.EffectiveDate = DateTime.Now;
            }

            var inventorySource = _ParseInventorySourceOrDefault(request.InventorySource);
            var fileImporter = _inventoryFileImporterFactory.GetFileImporterInstance(inventorySource);

            fileImporter.LoadFromSaveRequest(request);
            fileImporter.CheckFileHash();

            try
            {
                _DataLakeFileService.Save(request);
            }
            catch
            {
                throw new ApplicationException("Unable to send file to Data Lake shared folder and e-mail reporting the error.");
            }

            var inventoryFile = fileImporter.GetPendingInventoryFile();

            inventoryFile.Id = _inventoryFileRepository.CreateInventoryFile(inventoryFile, request.UserName);

            var stationLocks = new List<IDisposable>();
            var lockedStationCodes = new List<int>();

            try
            {
                var startTime = DateTime.Now;

                fileImporter.ExtractFileData(request.StreamData, inventoryFile, request.EffectiveDate);

                var endTime = DateTime.Now;

                Debug.WriteLine("Completed file parsing in {0}", endTime - startTime);

                if (fileImporter.FileProblems.Any())
                {
                    return _SetFileProblemWarnings(inventoryFile.Id, fileImporter.FileProblems);
                }

                if (!inventoryFile.HasManifests())
                {
                    throw new ApplicationException("Unable to parse any file records.");
                }

                var validationProblems = _inventoryFileValidator.ValidateInventoryFile(inventoryFile);

                fileImporter.FileProblems.AddRange(validationProblems.InventoryFileProblems);

                endTime = DateTime.Now;

                Debug.WriteLine("Completed file validation in {0}", endTime - startTime);

                if (fileImporter.FileProblems.Any())
                {
                    return _SetFileProblemWarnings(inventoryFile.Id, fileImporter.FileProblems);
                }

                startTime = DateTime.Now;

                var fileStationsDict = inventoryFile
                   .GetAllManifests()
                   .Select(x => x.Station)
                   .GroupBy(s => s.Code)
                   .ToDictionary(g => g.First().Code, g => g.First().LegacyCallLetters);

                using (var transaction = new TransactionScopeWrapper(_CreateTransactionScope(TimeSpan.FromMinutes(20))))
                {
                    LockStations(fileStationsDict, lockedStationCodes, stationLocks, inventoryFile);

                    var isProprietary = inventorySource.Name == "CNN" ||
                                        inventorySource.Name == "TTNW";

                    _AddRequestAudienceInfo(request, inventoryFile);

                    if (isProprietary)
                    {
                        _EnsureInventoryDaypartIds(inventoryFile);
                        _proprietarySpotCostCalculationEngine.CalculateSpotCost(request, inventoryFile);
                    }

                    _AddNewStationInventoryGroups(request, inventoryFile);
                    _SaveInventoryFileContacts(request, inventoryFile);
                    _stationRepository.UpdateStationList(fileStationsDict.Keys.ToList(), request.UserName, DateTime.Now, inventorySource.Id);
                    inventoryFile.FileStatus = FileStatusEnum.Loaded;
                    _inventoryFileRepository.UpdateInventoryFile(inventoryFile, request.UserName);

                    transaction.Complete();

                    UnlockStations(lockedStationCodes, stationLocks);

                    endTime = DateTime.Now;

                    Debug.WriteLine("Completed file saving in {0}", endTime - startTime);
                }
            }
            catch (FileUploadException<InventoryFileProblem> e)
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
                    UnlockStations(lockedStationCodes, stationLocks);
                    _inventoryFileRepository.UpdateInventoryFileStatus(inventoryFile.Id, FileStatusEnum.Failed);
                }
                catch
                {
                }

                throw new BroadcastInventoryDataException($"Error loading new inventory file: {e.Message}", inventoryFile.Id, e);
            }

            return _SetFileProblemWarnings(inventoryFile.Id, new List<InventoryFileProblem>());
        }

        private void LockStations(Dictionary<int, string> fileStationsDict, List<int> lockedStationCodes,
                List<IDisposable> stationLocks, InventoryFile inventoryFile)
        {
            //Lock stations before database operations
            foreach (var fileStation in fileStationsDict)
            {
                var lockResult = LockStation(fileStation.Key);
                if (lockResult.Success)
                {
                    lockedStationCodes.Add(fileStation.Key);
                    stationLocks.Add(new BomsLockManager(_SmsClient, new StationToken(fileStation.Key)));
                }
                else
                {
                    throw new ApplicationException(
                        string.Format("Unable to update station. Station locked for editing {0}.",
                            fileStation.Value));
                }
            }
        }

        private void UnlockStations(List<int> lockedStationCodes, List<IDisposable> stationLocks)
        {
            foreach (var stationCode in lockedStationCodes)
            {
                UnlockStation(stationCode);
            }
            foreach (var stationLock in stationLocks)
            {
                stationLock.Dispose();
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
                            Rate = ap.Price
                        })));
        }

        private void _AddNewStationInventoryGroups(InventoryFileSaveRequest request, InventoryFile inventoryFile)
        {
            _EnsureInventoryDaypartIds(inventoryFile);
            _stationInventoryGroupService.AddNewStationInventoryGroups(request, inventoryFile);
        }

        private void _EnsureInventoryDaypartIds(InventoryFile inventoryFile)
        {
            // set daypart id
            inventoryFile.InventoryGroups.SelectMany(
                ig =>
                    ig.Manifests.SelectMany(
                        m => m.ManifestDayparts.Where(md => md.Daypart.Id == -1).Select(md => md.Daypart)))
                .ForEach(dd => { dd.Id = _daypartCache.GetIdByDaypart(dd); });
        }

        private void _SaveInventoryFileContacts(InventoryFileSaveRequest request, InventoryFile inventoryFile)
        {
            var fileStationCodes = inventoryFile.StationContacts.Select(m => m.StationCode).Distinct().ToList();
            List<StationContact> existingStationContacts = _stationContactsRepository.GetStationContactsByStationCode(fileStationCodes);

            var contactsUpdateList = inventoryFile.StationContacts.Intersect(existingStationContacts, StationContact.StationContactComparer).ToList();

            //Set the ID for those that exist already
            foreach (var updateContact in contactsUpdateList)
            {
                updateContact.Id = existingStationContacts.Single(c => StationContact.StationContactComparer.Equals(c, updateContact)).Id;
            }

            _stationContactsRepository.UpdateExistingStationContacts(contactsUpdateList, request.UserName, inventoryFile.Id);

            var contactsCreateList =
                inventoryFile.StationContacts.Except(existingStationContacts, StationContact.StationContactComparer)
                    .ToList();
            _stationContactsRepository.CreateNewStationContacts(contactsCreateList, request.UserName, inventoryFile.Id);

            // update modified date for each station
            var timeStamp = DateTime.Now;
            _stationRepository.UpdateStationList(fileStationCodes, request.UserName, timeStamp, inventoryFile.InventorySource.Id);
        }

        public List<StationContact> GetStationContacts(string inventorySource, int stationCode)
        {
            return _stationContactsRepository.GetStationContactsByStationCode(stationCode);
        }

        public List<StationContact> FindStationContactsByName(string query)
        {
            return _stationContactsRepository.GetLatestContactsByName(query);
        }

        public bool SaveProgram(StationProgram stationProgram, string userName)
        {
            using (var transaction = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var manifest = _MapToStationInventoryManifest(stationProgram);

                if (stationProgram.Id == 0)
                {
                    _UpdateConflicts(stationProgram.Conflicts);
                    _AddNewProgram(stationProgram, manifest, userName);
                }
                else
                {
                    _UpdateProgram(stationProgram, manifest, userName);
                }

                transaction.Complete();

                return true;
            }
        }

        private void _UpdateConflicts(IEnumerable<StationProgram.StationProgramConflictChangeDto> conflicts)
        {
            if (conflicts == null)
                return;

            foreach (var program in conflicts)
            {
                _ValidateFlightWeeks(program.Flights);

                var flightWeekGroups = _GetFlightWeekGroups(program.Flights);

                if (!flightWeekGroups.Any())
                    continue;

                var firstFlightGroup = flightWeekGroups.First();
                var previousManifest = _inventoryRepository.GetStationManifest(program.Id);

                previousManifest.EffectiveDate = firstFlightGroup.StartDate;
                previousManifest.EndDate = firstFlightGroup.EndDate;

                _inventoryRepository.UpdateStationInventoryManifest(previousManifest);

                foreach (var flightWeekGroup in flightWeekGroups.Skip(1))
                {
                    previousManifest.EffectiveDate = flightWeekGroup.StartDate;
                    previousManifest.EndDate = flightWeekGroup.EndDate;

                    _inventoryRepository.SaveStationInventoryManifest(previousManifest);
                }
            }
        }

        private void _ValidateFlightWeeks(IEnumerable<FlightWeekDto> flights)
        {
            var hasOnlyHiatusFlights = flights.All(w => w.IsHiatus);

            if (hasOnlyHiatusFlights)
                throw new Exception("The program must have at least one valid flight week");
        }

        private void _UpdateProgram(StationProgram stationProgram, StationInventoryManifest manifest, string userName)
        {
            var timeStamp = DateTime.Now;
            var previousManifest = _inventoryRepository.GetStationManifest(stationProgram.Id);

            _SetManifestValuesFromPreviousManifest(manifest, previousManifest);
            _SetManifestDaypartGenres(manifest, stationProgram);

            if (manifest.EffectiveDate > previousManifest.EffectiveDate)
            {
                manifest.EndDate = previousManifest.EndDate;
                previousManifest.EndDate = manifest.EffectiveDate.AddDays(-1);

                _inventoryRepository.SaveStationInventoryManifest(manifest);
                _inventoryRepository.UpdateStationInventoryManifest(previousManifest);
            }
            else if (manifest.EffectiveDate < previousManifest.EffectiveDate)
            {
                manifest.EndDate = previousManifest.EffectiveDate.AddDays(-1);

                _inventoryRepository.SaveStationInventoryManifest(manifest);
                _inventoryRepository.UpdateStationInventoryManifest(previousManifest);
            }
            else
            {
                _inventoryRepository.UpdateStationInventoryManifest(manifest);
            }

            _stationRepository.UpdateStation(stationProgram.StationCode, userName, timeStamp, manifest.InventorySourceId);
        }

        private void _SetManifestValuesFromPreviousManifest(StationInventoryManifest manifest,
            StationInventoryManifest previousManifest)
        {
            manifest.ManifestDayparts = previousManifest.ManifestDayparts;
            manifest.Station = new DisplayBroadcastStation
            {
                Code = previousManifest.Station.Code
            };
        }

        private void _SetManifestDaypartGenres(StationInventoryManifest manifest, StationProgram stationProgram)
        {
            if (manifest.ManifestDayparts != null)
            {
                foreach (var daypart in manifest.ManifestDayparts)
                {
                    daypart.Genres = stationProgram.Genres;
                }
            }
        }

        private void _AddNewProgram(StationProgram stationProgram, StationInventoryManifest manifest, string userName)
        {
            _ValidateFlightWeeks(stationProgram.FlightWeeks);

            var flightWeekGroups = _GetFlightWeekGroups(stationProgram.FlightWeeks);

            foreach (var flightWeekGroup in flightWeekGroups)
            {
                manifest.EffectiveDate = flightWeekGroup.StartDate;
                manifest.EndDate = flightWeekGroup.EndDate;

                _inventoryRepository.SaveStationInventoryManifest(manifest);
            }
            var timeStamp = DateTime.Now;
            _stationRepository.UpdateStation(stationProgram.StationCode, userName, timeStamp, manifest.InventorySourceId);
        }

        private StationInventoryManifest _MapToStationInventoryManifest(StationProgram stationProgram)
        {
            const string householdAudienceCode = "HH";
            var audienceRepository = _broadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>();
            var householdeAudience = audienceRepository.GetDisplayAudienceByCode(householdAudienceCode);
            var inventorySource = _ParseInventorySource(stationProgram.RateSource);
            var manifestRates = _MapManifestRates(stationProgram);
            var spotLengthId = _GetSpotLengthIdForManifest(stationProgram);
            var displayDayparts = new List<DisplayDaypart>();

            if (stationProgram.Id == 0)
                displayDayparts = stationProgram.Airtimes.Select(DaypartDto.ConvertDaypartDto).ToList();

            var manifest = new StationInventoryManifest
            {
                Id = stationProgram.Id,
                EffectiveDate = stationProgram.EffectiveDate,
                EndDate = stationProgram.EndDate,
                Station = new DisplayBroadcastStation
                {
                    Code = stationProgram.StationCode
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

            _daypartCache.SyncDaypartsToIds(displayDayparts);

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
                    Rate = stationProgram.Rate15.Value
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

            using (new BomsLockManager(_SmsClient, new StationToken(stationContact.StationCode)))
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

                    _stationRepository.UpdateStation(stationContact.StationCode, userName, DateTime.Now, _ParseInventorySourceOrDefault(stationContact.InventorySourceString).Id);

                    transaction.Complete();
                }

                return true;
            }
        }

        public bool DeleteStationContact(string inventorySourceString, int stationContactId, string userName)
        {
            if (stationContactId <= 0)
                throw new Exception("Cannot delete station contact with invalid data.");

            var stationCode = _stationRepository.GetBroadcastStationCodeByContactId(stationContactId);

            using (new BomsLockManager(_SmsClient, new StationToken(stationCode)))
            using (var transaction = new TransactionScopeWrapper())
            {

                _broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>()
                    .DeleteStationContact(stationContactId);

                // update staion modified date
                _stationRepository.UpdateStation(stationCode, userName, DateTime.Now, _ParseInventorySourceOrDefault(inventorySourceString).Id);

                transaction.Complete();
            }

            return true;
        }

        public StationDetailDto GetStationDetailByCode(string inventorySource, int stationCode)
        {
            var rateSource = _ParseInventorySource(inventorySource);
            var station = _stationRepository.GetBroadcastStationByCode(stationCode);
            var stationManifests = _inventoryRepository.GetStationManifestsBySourceAndStationCode(rateSource,
                stationCode);

            _SetDisplayDaypartForInventoryManifest(stationManifests);
            _SetAudienceForInventoryManifest(stationManifests);

            return new StationDetailDto
            {
                Affiliate = station.Affiliation,
                Market = station.OriginMarket,
                StationCode = stationCode,
                StationName = station.LegacyCallLetters,
                Programs = _GetStationProgramsFromStationInventoryManifest(stationManifests),
                Contacts = _broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>()
                    .GetStationContactsByStationCode(stationCode)
            };
        }

        private void _SetDisplayDaypartForInventoryManifest(List<StationInventoryManifest> manifests)
        {
            manifests.SelectMany(m => m.ManifestDayparts).ForEach(d =>
            {
                d.Daypart = _daypartCache.GetDisplayDaypart(d.Daypart.Id);
            });
        }

        private void _SetAudienceForInventoryManifest(List<StationInventoryManifest> manifests)
        {
            manifests.SelectMany(m => m.ManifestAudiences).ForEach(a =>
            {
                a.Audience = _AudiencesCache.GetDisplayAudienceById(a.Audience.Id);
            });
        }

        /// <summary>
        /// Transform manifests into Station Program to match FE grid structure 
        /// </summary>
        /// <param name="stationManifests"></param>
        /// <returns></returns>
        private List<StationProgram> _GetStationProgramsFromStationInventoryManifest(
            List<StationInventoryManifest> stationManifests)
        {
            return (from manifest in stationManifests
                    select new StationProgram()
                    {
                        Id = manifest.Id ?? 0,
                        ProgramNames = manifest.ManifestDayparts.Select(md => md.ProgramName).ToList(),
                        Airtimes = manifest.ManifestDayparts.Select(md => DaypartDto.ConvertDisplayDaypart(md.Daypart)).ToList(),
                        AirtimePreviews = manifest.ManifestDayparts.Select(md => md.Daypart.Preview).ToList(),
                        EffectiveDate = manifest.EffectiveDate,
                        EndDate = manifest.EndDate,
                        StationCode = manifest.Station.Code,
                        SpotLength = _SpotLengthMap.Single(a => a.Value == manifest.SpotLengthId).Key,
                        SpotsPerWeek = manifest.SpotsPerWeek,
                        Rate15 = _GetSpotRateFromManifestRates(15, manifest.ManifestRates),
                        Rate30 = _GetSpotRateFromManifestRates(30, manifest.ManifestRates),
                        HouseHoldImpressions = _GetHouseHoldImpressionFromManifestAudiences(manifest.ManifestAudiencesReferences),
                        Rating = _GetHouseHoldRatingFromManifestAudiences(manifest.ManifestAudiencesReferences),
                        FlightWeeks = _GetFlightWeeks(manifest.EffectiveDate, manifest.EndDate),
                        Genres = manifest.ManifestDayparts.SelectMany(d => d.Genres).Distinct().ToList()
                    }).ToList();
        }

        private List<FlightWeekDto> _GetFlightWeeks(DateTime effectiveDate, DateTime? endDate)
        {
            var nonNullableEndDate = endDate ?? effectiveDate.AddYears(1);

            var displayFlighWeeks = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(effectiveDate, nonNullableEndDate);

            var flighWeeks = new List<FlightWeekDto>();

            foreach (var displayMediaWeek in displayFlighWeeks)
            {
                flighWeeks.Add(new FlightWeekDto
                {
                    StartDate = displayMediaWeek.WeekStartDate,
                    EndDate = displayMediaWeek.WeekEndDate
                });
            }

            return flighWeeks;
        }

        private double? _GetHouseHoldRatingFromManifestAudiences(List<StationInventoryManifestAudience> list)
        {
            var houseHold = _GetHouseHoldAudienceFromManifestAudiences(list);
            return houseHold != null ? houseHold.Rating : 0;
        }

        private double? _GetHouseHoldImpressionFromManifestAudiences(List<StationInventoryManifestAudience> list)
        {
            var houseHold = _GetHouseHoldAudienceFromManifestAudiences(list);
            return houseHold != null ? houseHold.Impressions : 0;
        }

        private StationInventoryManifestAudience _GetHouseHoldAudienceFromManifestAudiences(
            List<StationInventoryManifestAudience> list)
        {
            var houseHoldAudienceId = _AudiencesCache.GetDisplayAudienceByCode(BroadcastConstants.HOUSEHOLD_CODE).Id;
            return list.Any()
                ? list.SingleOrDefault(c => c.Audience.Id == houseHoldAudienceId)
                : null;
        }

        private decimal _GetSpotRateFromManifestRates(int spotLength, List<StationInventoryManifestRate> list)
        {
            StationInventoryManifestRate manifestRate = null;
            if (list != null && list.Any())
            {
                manifestRate = list.FirstOrDefault(c => c.SpotLengthId == _SpotLengthMap[spotLength]);
            }

            return manifestRate != null ? manifestRate.Rate : 0;
        }

        public List<LookupDto> GetAllGenres()
        {
            return _genreRepository.GetAllGenres();
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
            var inventorySource = _inventoryRepository.GetInventorySourceByName(sourceString);

            if (inventorySource == null)
            {
                throw new ArgumentException("Invalid inventory source string");
            }

            return inventorySource;
        }

        private InventorySource _ParseInventorySourceOrDefault(string sourceString)
        {
            const string defaultInventorySource = "OpenMarket";

            if (string.IsNullOrWhiteSpace(sourceString))
            {
                return _inventoryRepository.GetInventorySourceByName(defaultInventorySource);
            }

            return _inventoryRepository.GetInventorySourceByName(sourceString);
        }

        private void _SetTransactionManagerField(string fieldName, object value)
        {
            typeof(TransactionManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, value);
        }

        private TransactionScope _CreateTransactionScope(TimeSpan timeout)
        {
            _SetTransactionManagerField("_cachedMaxTimeout", true);
            _SetTransactionManagerField("_maximumTimeout", timeout);
            return new TransactionScope(TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted,
                       Timeout = timeout
                   });
        }

        public LockResponse LockStation(int stationCode)
        {
            var key = string.Format("broadcast_station : {0}", stationCode);
            var result = _LockingManager.LockObject(key);
            return result;
        }

        public ReleaseLockResponse UnlockStation(int stationCode)
        {
            var key = string.Format("broadcast_station : {0}", stationCode);
            var result = _LockingManager.ReleaseObject(key);
            return result;
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
                manifestRate.Rate = rate * (decimal)_SpotLengthCostMultipliers[spotLength.Value];
                manifestRates.Add(manifestRate);
            }

            return manifestRates;
        }

        public List<StationProgram> GetStationProgramConflicts(StationProgramConflictRequest conflict)
        {
            if (conflict.StartDate == DateTime.MinValue || conflict.EndDate == DateTime.MinValue)
            {
                throw new Exception(String.Format("Unable to parse start and end date values: {0}, {1}",
                    conflict.StartDate, conflict.EndDate));
            }

            var station = _stationRepository.GetBroadcastStationByCode(conflict.StationCode);

            var airtime = DaypartDto.ConvertDaypartDto(conflict.Airtime);
            airtime.Id = _daypartCache.GetIdByDaypart(airtime);

            var programs = _inventoryRepository.GetManifestProgramsByStationCodeAndDates(conflict.RateSource,
                station.Code, conflict.StartDate, conflict.EndDate);

            _SetDisplayDaypartForInventoryManifest(programs);
            _SetAudienceForInventoryManifest(programs);

            var filteredPrograms = new List<StationInventoryManifest>();

            foreach (var program in programs)
            {
                foreach (var manifestDaypart in program.ManifestDayparts)
                {
                    if (_DateRangesIntersect(program.EffectiveDate, program.EndDate ?? DateTime.MaxValue,
                            conflict.StartDate, conflict.EndDate) &&
                        DisplayDaypart.Intersects(_daypartCache.GetDisplayDaypart(manifestDaypart.Daypart.Id), airtime))
                        filteredPrograms.Add(program);
                }
            }

            return _GetStationProgramsFromStationInventoryManifest(filteredPrograms);
        }

        public bool DeleteProgram(int programId, string inventorySource, int stationCode, string user)
        {
            _inventoryRepository.RemoveManifest(programId);
            _stationRepository.UpdateStation(stationCode, user, DateTime.Now, _ParseInventorySourceOrDefault(inventorySource).Id);
            return true;
        }

        public bool ExpireManifest(int programId, DateTime endDate, string inventorySource, int stationCode, string user)
        {
            _inventoryRepository.ExpireManifest(programId, endDate);
            _stationRepository.UpdateStation(stationCode, user, DateTime.Now, _ParseInventorySourceOrDefault(inventorySource).Id);
            return true;
        }

        public bool HasSpotsAllocated(int programId)
        {
            return _inventoryRepository.HasSpotsAllocated(programId);
        }

        private static bool _DateRangesIntersect(DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2)
        {
            return (startDate1 >= startDate2 && startDate1 <= endDate2)
                   || (endDate1 >= startDate2 && endDate1 <= endDate2)
                   || (startDate1 < startDate2 && endDate1 > endDate2);
        }

        private static IList<FlightWeekGroup> _GetFlightWeekGroups(IEnumerable<FlightWeekDto> flightWeeks)
        {
            var flightWeekGroups = new List<FlightWeekGroup>();
            FlightWeekGroup currentFlightWeekGroup = null;

            if (flightWeeks == null)
                return flightWeekGroups;

            foreach (var flightWeek in flightWeeks)
            {
                if (flightWeek.IsHiatus)
                {
                    currentFlightWeekGroup = null;
                    continue;
                }

                if (currentFlightWeekGroup == null)
                {
                    currentFlightWeekGroup = new FlightWeekGroup
                    {
                        StartDate = flightWeek.StartDate
                    };

                    flightWeekGroups.Add(currentFlightWeekGroup);
                }

                currentFlightWeekGroup.EndDate = flightWeek.EndDate;
            }

            return flightWeekGroups;
        }
    }
}