using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Common.Systems.LockTokens;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Transactions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

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
        bool DeleteStationContact(int stationContactId, string userName);
        List<LookupDto> GetAllGenres();
        LockResponse LockStation(int stationCode);
        ReleaseLockResponse UnlockStation(int stationCode);
        RatesInitialDataDto GetInitialRatesData();
        Decimal ConvertRateForSpotLength(decimal rateFor30s, int outputRateSpotLength);
        List<StationContact> FindStationContactsByName(string query);
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
        private readonly IThirdPartySpotCostCalculationEngine _ThirdPartySpotCostCalculationEngine;
        private readonly IStationInventoryGroupService _stationInventoryGroupService;
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory, 
                            IInventoryFileValidator inventoryFileValidator,
                            IDaypartCache daypartCache,
                            IQuarterCalculationEngine quarterCalculationEngine,
                            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache, 
                            IInventoryFileImporterFactory inventoryFileImporterFactory,
                            ISMSClient smsClient, 
                            ILockingManagerApplicationService lockingManager,
                            IThirdPartySpotCostCalculationEngine thirdPartySpotCostCalculationEngine,
                            IStationInventoryGroupService stationInventoryGroupService,
                            IBroadcastAudiencesCache audiencesCache)
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
            _SpotLengthMap.Add(0, 0);
            _SpotLengthCostMultipliers =
                broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthMultiplierRepository>()
                    .GetSpotLengthIdsAndCostMultipliers();
            _ThirdPartySpotCostCalculationEngine = thirdPartySpotCostCalculationEngine;
            _stationInventoryGroupService = stationInventoryGroupService;
            _inventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        public List<DisplayBroadcastStation> GetStations(string rateSource, DateTime currentDate)
        {
            var stations =
                _stationRepository.GetBroadcastStationsWithFlightWeeksForRateSource(_ParseInventorySource(rateSource));
            _SetFlightData(stations, currentDate);
            return stations;
        }

        public List<DisplayBroadcastStation> GetStationsWithFilter(string rateSource, string filterValue, DateTime today)
        {

            DisplayBroadcastStation.StationFilter filter;
            var parseSuccess = Enum.TryParse(filterValue, true, out filter);
            if (!parseSuccess)
            {
                throw new ArgumentException(string.Format("Invalid station filter parameter: {0}", filterValue));
            }

            var isIncluded = (filter == DisplayBroadcastStation.StationFilter.WithTodaysData);
            var mediaWeekId = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(today).Id;
            var stations = _stationRepository.GetBroadcastStationsByFlightWeek(_ParseInventorySource(rateSource), mediaWeekId, isIncluded);

            _SetFlightData(stations, today);

            return stations;
        }

        public InventoryFileSaveResult SaveInventoryFile(InventoryFileSaveRequest request)
        {
            var inventorySource = _ParseInventorySourceOrDefault(request.InventorySource);
            var fileImporter = _inventoryFileImporterFactory.GetFileImporterInstance(inventorySource);
            
            fileImporter.LoadFromSaveRequest(request);
            fileImporter.CheckFileHash();

            var inventoryFile = fileImporter.GetPendingInventoryFile();

            inventoryFile.Id = _inventoryFileRepository.CreateInventoryFile(inventoryFile, request.UserName);

            var stationLocks = new List<IDisposable>();
            var lockedStationCodes = new List<int>();
            var fileProblems = new List<InventoryFileProblem>();

            try
            {
                var startTime = DateTime.Now;

                fileImporter.ExtractFileData(request.RatesStream, inventoryFile, request.EffectiveDate, fileProblems);

                if (inventoryFile.InventoryGroups == null || inventoryFile.InventoryGroups.Count == 0 ||
                    !inventoryFile.InventoryGroups.SelectMany(g => g.Manifests).Any())
                {
                    throw new ApplicationException("Unable to parse any file records.");
                }

                var endTime = DateTime.Now;

                System.Diagnostics.Debug.WriteLine("Completed file parsing in {0}", endTime - startTime);

                if (fileProblems.Any())
                {
                    return new InventoryFileSaveResult()
                    {
                        Problems = fileProblems,
                        FileId = inventoryFile.Id
                    };
                }

                startTime = DateTime.Now;

                var validationProblems = _inventoryFileValidator.ValidateInventoryFile(inventoryFile);

                fileProblems.AddRange(validationProblems.InventoryFileProblems);

                endTime = DateTime.Now;

                System.Diagnostics.Debug.WriteLine("Completed file validation in {0}", endTime - startTime);

                if (fileProblems.Any())
                {
                    return new InventoryFileSaveResult
                    {
                        Problems = fileProblems,
                        FileId = inventoryFile.Id
                    };
                }

                startTime = DateTime.Now;

                var fileStationCodes =
                    inventoryFile.InventoryGroups.SelectMany(g => g.Manifests)
                        .Select(i => i.Station.Code)
                        .Distinct()
                        .ToList();

                using (var transaction = new TransactionScopeWrapper(_CreateTransactionScope(TimeSpan.FromMinutes(20))))
                {
                    LockStations(fileStationCodes, lockedStationCodes, stationLocks, inventoryFile);

                    var isThirdParty = inventorySource.Name == "CNN" ||
                                       inventorySource.Name == "TTNW";

                    if (isThirdParty)
                    {
                        _ThirdPartySpotCostCalculationEngine.CalculateSpotCost(request, inventoryFile);
                    }

                    inventoryFile.FileStatus = InventoryFile.FileStatusEnum.Loaded;

                    _SaveStationInventoryGroups(request, inventoryFile);
                    _SaveInventoryFileContacts(request, inventoryFile);

                    transaction.Complete();

                    UnlockStations(lockedStationCodes, stationLocks);

                    endTime = DateTime.Now;

                    System.Diagnostics.Debug.WriteLine("Completed file saving in {0}", endTime - startTime);
                }
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
                    _inventoryFileRepository.UpdateInventoryFileStatus(inventoryFile.Id,
                        InventoryFile.FileStatusEnum.Failed);
                }
                catch
                {
                }

                throw new BroadcastInventoryDataException(string.Format("Error loading new inventory file: {0}", e.Message),
                    inventoryFile.Id, e);
            }
            return new InventoryFileSaveResult
            {
                FileId = inventoryFile.Id,
                Problems = fileProblems
            };
        }

        private void LockStations(List<int> fileStationCodes, List<int> lockedStationCodes, List<IDisposable> stationLocks, InventoryFile inventoryFile)
        {
            //Lock stations before database operations
            foreach (var stationCode in fileStationCodes)
            {
                var lockResult = LockStation(stationCode);
                if (lockResult.Success)
                {
                    lockedStationCodes.Add(stationCode);
                    stationLocks.Add(new BomsLockManager(_SmsClient, new StationToken(stationCode)));
                }
                else
                {
                    var stationLetters =
                        inventoryFile.InventoryGroups.SelectMany(g => g.Manifests).Where(i => i.Station.Code == stationCode)
                            .First()
                            .Station.LegacyCallLetters;
                    throw new ApplicationException(string.Format("Unable to update station. Station locked for editing {0}.",
                        stationLetters));
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

        private void _SaveStationInventoryGroups(InventoryFileSaveRequest request, InventoryFile inventoryFile)
        {
            // set daypart id
            inventoryFile.InventoryGroups.SelectMany(ig => ig.Manifests.SelectMany(m => m.ManifestDayparts.Select(md => md.Daypart))).ForEach(dd =>
            {
                dd.Id = _daypartCache.GetIdByDaypart(dd);
            });

            _stationInventoryGroupService.SaveStationInventoryGroups(request, inventoryFile);
        }


        private void _SaveInventoryFileContacts(InventoryFileSaveRequest request, InventoryFile inventoryFile)
        {
            //TODO: Fixme or remove.
            return;
            //var fileStationCodes = ratesFile.StationPrograms.Select(p => (int)p.StationCode).Distinct().ToList();
            List<StationContact> existingStationContacts = null;//_stationContactsRepository.GetStationContactsByStationCode(fileStationCodes);

            var contactsUpdateList =
                inventoryFile.StationContacts.Intersect(existingStationContacts, StationContact.StationContactComparer).ToList();

            //Set the ID for those that exist already
            foreach (var updateContact in contactsUpdateList)
            {
                updateContact.Id =
                    existingStationContacts.Single(
                        c => StationContact.StationContactComparer.Equals(c, updateContact)).Id;
            }
            _stationContactsRepository.UpdateExistingStationContacts(contactsUpdateList, request.UserName, inventoryFile.Id);

            var contactsCreateList =
                inventoryFile.StationContacts.Except(existingStationContacts, StationContact.StationContactComparer).ToList();
            _stationContactsRepository.CreateNewStationContacts(contactsCreateList, request.UserName, inventoryFile.Id);

            // update modified date for each station
            var timeStamp = DateTime.Now;
            //fileStationCodes.ForEach(code => _stationRepository.UpdateStation(code, request.UserName, timeStamp));
        }



        public List<StationContact> GetStationContacts(string inventorySource, int stationCode)
        {
            return _stationContactsRepository.GetStationContactsByStationCode(stationCode);
        }

        public List<StationContact> FindStationContactsByName(string query)
        {
            return _stationContactsRepository.GetLatestContactsByName(query);
        }

        public bool SaveStationContact(StationContact stationContact, string userName)
        {
            using (new BomsLockManager(_SmsClient, new StationToken(stationContact.StationCode)))
            {

                if (stationContact == null)
                    throw new Exception("Cannot save station contact with invalid data.");

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

                    _stationRepository.UpdateStation(stationContact.StationCode, userName, DateTime.Now);

                    transaction.Complete();
                }
                return true;
            }
        }

        public bool DeleteStationContact(int stationContactId, string userName)
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
                _stationRepository.UpdateStation(stationCode, userName, DateTime.Now);

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

            return new StationDetailDto()
            {
                Affiliate = station.Affiliation,
                Market = station.OriginMarket,
                StationCode = stationCode,
                StationName = station.LegacyCallLetters,
                Programs = GetStationProgramsFromStationInventoryManifest(stationManifests),
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
            manifests.SelectMany(m=>m.ManifestAudiences).ForEach(a =>
            {
                a.Audience = _AudiencesCache.GetDisplayAudienceById(a.Audience.Id);
            });
        }

        private List<StationProgram> GetStationProgramsFromStationInventoryManifest(List<StationInventoryManifest> stationManifests)
        {
            // todo: still some properties missing
            return (from manifest in stationManifests
                from daypart in manifest.ManifestDayparts
                select new StationProgram()
                {
                    ProgramName = daypart.ProgramName,
                    Airtime = daypart.Daypart.Preview,
                    StartDate = manifest.EffectiveDate,
                    EndDate = manifest.EndDate
                }).ToList();
        }


        public List<LookupDto> GetAllGenres()
        {
            return _genreRepository.GetAllGenres();
        }

        private void _SetFlightData(List<DisplayBroadcastStation> stations, DateTime currentDate)
        {

            var dateRangeThisQuarter = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER,
                currentDate);
            var dateRangeNextQuarter = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.NEXTQUARTER,
                currentDate);

            stations.ForEach(s =>
            {
                if (s.FlightWeeks != null && s.FlightWeeks.Any())
                {
                    s.FlightWeeks.ForEach(
                        fw =>
                        {
                            var mediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(fw.Id);
                            fw.StartDate = mediaWeek.StartDate;
                            fw.EndDate = mediaWeek.EndDate;
                        });

                    if (s.FlightWeeks.Last().EndDate.Date == dateRangeThisQuarter.Item2.Date)
                        s.RateDataThrough = "This Quarter";
                    else
                    {
                        s.RateDataThrough = s.FlightWeeks.Last().EndDate.Date == dateRangeNextQuarter.Item2.Date
                            ? "Next Quarter"
                            : s.FlightWeeks.Last().EndDate.ToShortDateString();
                    }
                }
                else
                {
                    // no data available
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
            const string defaultInventorySource = "Open Market";

            if (string.IsNullOrEmpty(sourceString))
            {
                return _inventoryRepository.GetInventorySourceByName(defaultInventorySource);
            }

            return _inventoryRepository.GetInventorySourceByName(sourceString);
        }

        private void _SetTransactionManagerField(string fieldName, object value)
        {
            typeof(TransactionManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
        }

        private TransactionScope _CreateTransactionScope(TimeSpan timeout)
        {
            _SetTransactionManagerField("_cachedMaxTimeout", true);
            _SetTransactionManagerField("_maximumTimeout", timeout);
            return new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted,
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
                RatingBooks = GetRatingBooks(),
                PlaybackTypes = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalPlaybackType>(),
                Audiences = _AudiencesCache.GetAllLookups(),
                DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3
            };

            return ratesInitialDataDto;
        }

        private List<LookupDto> GetRatingBooks()
        {
            var postingBooks = _broadcastDataRepositoryFactory.GetDataRepository<IPostingBookRepository>()
                                    .GetPostableMediaMonths(BroadcastConstants.PostableMonthMarketThreshold);

            var mediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(postingBooks);

            return (from mediaMonth in mediaMonths
                    orderby mediaMonth.Id descending
                    select new LookupDto
                    {
                        Id = mediaMonth.Id,
                        Display = mediaMonth.LongMonthNameAndYear
                    }).ToList();
        }

        public Decimal ConvertRateForSpotLength(decimal rateFor30s, int outputRateSpotLength)
        {
            if (!_SpotLengthMap.ContainsKey(outputRateSpotLength))
            {
                throw new ArgumentException(String.Format("Unable to convert rate to unknown spot length {0}", outputRateSpotLength));
            }

            var spotLengthId = _SpotLengthMap[outputRateSpotLength];

            if (!_SpotLengthCostMultipliers.ContainsKey(spotLengthId))
            {
                throw new InvalidOperationException(string.Format("No conversion factor available for spot length {0}", outputRateSpotLength));
            }

            var costMultiplier = _SpotLengthCostMultipliers[spotLengthId];
            var result = rateFor30s * (Decimal) costMultiplier;
            return result;
        }
    }
}