using System.ComponentModel;
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
using Tam.Maestro.Common.Formatters;
using Tam.Maestro.Data.Entities;
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

    public interface IRatesService : IApplicationService
    {
        List<DisplayBroadcastStation> GetStations(string rateSource, DateTime currentDate);
        List<DisplayBroadcastStation> GetStationsWithFilter(string rateSource, string filterValue, DateTime today);
        RatesFileSaveResult SaveRatesFile(RatesSaveRequest request);
        StationDetailDto GetStationDetailByCode(string rateSource, int stationCode);
        List<StationContact> GetStationContacts(string rateSource, int stationCode);
        bool SaveStationContact(StationContact stationContacts, string userName);
        bool DeleteStationContact(int stationContactId, string userName);
        List<LookupDto> GetAllGenres();
        LockResponse LockStation(int stationCode);
        ReleaseLockResponse UnlockStation(int stationCode);
        RatesInitialDataDto GetInitialRatesData();
        Decimal ConvertRateForSpotLength(decimal rateFor30s, int outputRateSpotLength);
        List<StationContact> FindStationContactsByName(string query);
    }

    public class RatesService : IRatesService
    {
        private readonly IStationRepository _stationRepository;
        private readonly IDataRepositoryFactory _broadcastDataRepositoryFactory;
        private readonly IDaypartCache _daypartCache;
        private readonly IBroadcastAudiencesCache _audiencesCache;
        private readonly IRatesFileValidator _ratesFileValidator;
        private readonly IStationContactsRepository _stationContactsRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IRateFileImporterFactory _rateFileImporterFactory;
        private readonly ISMSClient _SmsClient;
        private readonly IRatesRepository _RatesRepository;
        private readonly ILockingManagerApplicationService _LockingManager;
        private readonly IRatingForecastService _RatingForecastService;
        private readonly Dictionary<int, int> _SpotLengthMap;
        private readonly Dictionary<int, double> _SpotLengthCostMultipliers; 
        private readonly IThirdPartySpotCostCalculationEngine _ThirdPartySpotCostCalculationEngine;

        public RatesService(IDataRepositoryFactory broadcastDataRepositoryFactory, 
                            IRatesFileValidator ratesFileValidator,
                            IDaypartCache daypartCache, IBroadcastAudiencesCache audiencesCache, 
                            IQuarterCalculationEngine quarterCalculationEngine,
                            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache, 
                            IRateFileImporterFactory rateFileImporterFactory,
                            ISMSClient smsClient, 
                            ILockingManagerApplicationService lockingManager,
                            IRatingForecastService ratingForecastService,
                            IThirdPartySpotCostCalculationEngine thirdPartySpotCostCalculationEngine)
        {
            _broadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _stationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _daypartCache = daypartCache;
            _audiencesCache = audiencesCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _ratesFileValidator = ratesFileValidator;
            _stationContactsRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>();
            _genreRepository = _broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _rateFileImporterFactory = rateFileImporterFactory;
            _SmsClient = smsClient;
            _RatesRepository = _broadcastDataRepositoryFactory.GetDataRepository<IRatesRepository>();
            _LockingManager = lockingManager;
            _RatingForecastService = ratingForecastService;
            _SpotLengthMap =
                broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            _SpotLengthMap.Add(0, 0);
            _SpotLengthCostMultipliers =
                broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthMultiplierRepository>()
                    .GetSpotLengthIdsAndCostMultipliers();
            _ThirdPartySpotCostCalculationEngine = thirdPartySpotCostCalculationEngine;
        }

        public List<DisplayBroadcastStation> GetStations(string rateSource, DateTime currentDate)
        {
            var stations = _stationRepository.GetBroadcastStationsWithFlightWeeksForRateSource(_ParseRateSource(rateSource));
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
            var stations = _stationRepository.GetBroadcastStationsByFlightWeek(_ParseRateSource(rateSource), mediaWeekId, isIncluded);

            _SetFlightData(stations, today);

            return stations;
        }

        public RatesFileSaveResult SaveRatesFile(RatesSaveRequest request)
        {
            //TODO: fix or remove
            return new RatesFileSaveResult();
            //var rateSourceType = _ParseRateSourceOrDefault(request.RateSource);
            //var fileImporter = _rateFileImporterFactory.GetFileImporterInstance(rateSourceType);
            //fileImporter.LoadFromSaveRequest(request);
            //fileImporter.CheckFileHash();

            //RatesFile ratesFile = fileImporter.GetPendingRatesFile();
            //ratesFile.Id = _RatesRepository.CreateRatesFile(ratesFile, request.UserName);

            //var stationLocks = new List<IDisposable>();
            //var lockedStationCodes = new List<int>();
            //var fileProblems = new List<RatesFileProblem>();
            //try
            //{

            //    var startTime = DateTime.Now;
            //    fileImporter.ExtractFileData(request.RatesStream, ratesFile, fileProblems);
            //    var endTime = DateTime.Now;
            //    System.Diagnostics.Debug.WriteLine(string.Format("Completed file parsing in {0}", endTime - startTime));
            //    startTime = DateTime.Now;

            //    foreach (var program in ratesFile.StationPrograms.ToList())
            //    {
            //        try
            //        {
            //            var programStationCode = (short)existingfileStations
            //                .Where(
            //                    s =>
            //                        s.LegacyCallLetters.Equals(
            //                            program.StationLegacyCallLetters,
            //                            StringComparison.InvariantCultureIgnoreCase))
            //                .Single(
            //                    string.Format(
            //                        "Invalid station: {0}",
            //                        program.StationLegacyCallLetters)).Code;
            //            program.StationCode = programStationCode;
            //        }
            //        catch (Exception e)
            //        {
            //            ratesFile.StationPrograms.Remove(program);
            //            fileProblems.Add(new RatesFileProblem()
            //            {
            //                ProblemDescription = e.Message,
            //                ProgramName = program.ProgramName,
            //                StationLetters = program.StationLegacyCallLetters
            //            });

            //        }
            //    }

            //    var validationProblems = _ratesFileValidator.ValidateRatesFile(ratesFile);
            //    fileProblems.AddRange(validationProblems.RatesFileProblems);
            //    // remove the invalid rates
            //    ratesFile.StationPrograms.RemoveAll(x => validationProblems.InvalidRates.Contains(x));

            //    endTime = DateTime.Now;
            //    System.Diagnostics.Debug.WriteLine(string.Format("Completed file validation in {0}", endTime - startTime));

            //    startTime = DateTime.Now;

            //    var fileStationCodes = ratesFile.StationPrograms.Select(p => (int)p.StationCode).Distinct().ToList();
            //    //using (var transaction = new TransactionScopeWrapper(new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(30))))
            //    using (var transaction = new TransactionScopeWrapper(_CreateTransactionScope(TimeSpan.FromMinutes(20))))
            //    {
            //        //Lock stations before database operations
            //        foreach (var stationCode in fileStationCodes)
            //        {
            //            var lockResult = LockStation(stationCode);
            //            if (lockResult.Success)
            //            {
            //                lockedStationCodes.Add(stationCode);
            //                stationLocks.Add(new BomsLockManager(_SmsClient, new StationToken(stationCode)));
            //            }
            //            else
            //            {
            //                var stationLetters =
            //                    ratesFile.StationPrograms.Where(p => p.StationCode == stationCode)
            //                        .First()
            //                        .StationLegacyCallLetters;
            //                throw new ApplicationException(string.Format("Unable to update station. Station locked for editing {0}.", stationLetters));
            //            }
            //        }

            //        var isThirdParty = rateSourceType == RatesFile.RateSourceType.CNN ||
            //                           rateSourceType == RatesFile.RateSourceType.TTNW ||
            //                           rateSourceType == RatesFile.RateSourceType.TVB;

            //        if (isThirdParty)
            //        {
            //            if (!request.RatingBook.HasValue)
            //            {
            //                throw new InvalidEnumArgumentException("Ratings book id required for third party rate files.");
            //            }
            //            _ThirdPartySpotCostCalculationEngine.CalculateSpotCost(request, ratesFile);
            //        }

            //        ratesFile.FileStatus = RatesFile.FileStatusEnum.Loaded;
            //        _SaveRateFilePrograms(request, ratesFile);
            //        _SaveRateFileContacts(request, ratesFile);

            //        // crunch should happen only for ttnw, cnn and tvb and same transaction
            //        if (isThirdParty)
            //        {
            //            var affectedProposals =
            //                _InventoryCrunchService.CrunchThirdPartyInventory(ratesFile.StationPrograms, rateSourceType,
            //                    request.FlightWeeks);

            //            if (affectedProposals != null && affectedProposals.Any())
            //            {
            //                fileProblems.Add(new RatesFileProblem()
            //                {
            //                    ProblemDescription = "Import Updated Inventory reserved in the following Proposals",
            //                    AffectedProposals = affectedProposals.Select(a => a.ProblemDescription).ToList()
            //                });
            //            }
            //        }


            //        transaction.Complete();


            //        //unlock stations
            //        foreach (var stationCode in lockedStationCodes)
            //        {
            //            UnlockStation(stationCode);
            //        }
            //        foreach (var stationLock in stationLocks)
            //        {
            //            stationLock.Dispose();
            //        }

            //        endTime = DateTime.Now;
            //        System.Diagnostics.Debug.WriteLine(
            //            string.Format("Completed file saving in {0}", endTime - startTime));
            //    }
            //}
            //catch (Exception e)
            //{
            //    //Try to update the status of the file if possible
            //    try
            //    {
            //        foreach (var stationCode in lockedStationCodes)
            //        {
            //            UnlockStation(stationCode);
            //        }
            //        foreach (var stationLock in stationLocks)
            //        {
            //            stationLock.Dispose();
            //        }
            //        _RatesRepository.UpdateRatesFileStatus(ratesFile.Id, RatesFile.FileStatusEnum.Failed);
            //    }
            //    catch
            //    {

            //    }
            //    throw new BroadcastRateDataException(string.Format("Error loading new rates file: {0}", e.Message),
            //        ratesFile.Id, e);
            //}

            //return new RatesFileSaveResult()
            //{
            //    FileId = ratesFile.Id,
            //    Problems = fileProblems
            //};
        }

        private void _SaveRateFileContacts(RatesSaveRequest request, RatesFile ratesFile)
        {
            //TODO: fix or remove
            //var fileStationCodes = ratesFile.StationPrograms.Select(p => (int)p.StationCode).Distinct().ToList();
            List<StationContact> existingStationContacts = null;//_stationContactsRepository.GetStationContactsByStationCode(fileStationCodes);

            var contactsUpdateList =
                ratesFile.StationContacts.Intersect(existingStationContacts, StationContact.StationContactComparer).ToList();

            //Set the ID for those that exist already
            foreach (var updateContact in contactsUpdateList)
            {
                updateContact.Id =
                    existingStationContacts.Single(
                        c => StationContact.StationContactComparer.Equals(c, updateContact)).Id;
            }
            _stationContactsRepository.UpdateExistingStationContacts(contactsUpdateList, request.UserName, ratesFile.Id);

            var contactsCreateList =
                ratesFile.StationContacts.Except(existingStationContacts, StationContact.StationContactComparer).ToList();
            _stationContactsRepository.CreateNewStationContacts(contactsCreateList, request.UserName, ratesFile.Id);

            // update modified date for each station
            var timeStamp = DateTime.Now;
            //fileStationCodes.ForEach(code => _stationRepository.UpdateStation(code, request.UserName, timeStamp));
        }

        private void _SaveRateFilePrograms(RatesSaveRequest request, RatesFile ratesFile)
        {
            //Todo: fix or remove
            //System.Diagnostics.Debug.WriteLine("Going to add programs");
            //if (ratesFile.RateSource == RatesFile.RateSourceType.TVB ||
            //    ratesFile.RateSource == RatesFile.RateSourceType.CNN ||
            //    ratesFile.RateSource == RatesFile.RateSourceType.TTNW)
            //{
            //    _SaveThirdPartyFilePrograms(request, ratesFile);
            //}
            //else
            //{
            //    _stationProgramRepository.AddRateFilePrograms(ratesFile, request.UserName, _SpotLengthMap);
            //}
            //System.Diagnostics.Debug.WriteLine("Going to update stations");
            //var stationCodes = ratesFile.StationPrograms.Select(y => (int)y.StationCode).Distinct().ToList();
            //_stationRepository.UpdateStationList(stationCodes, request.UserName, DateTime.Now);
            //System.Diagnostics.Debug.WriteLine("Going to update file record");
            //_RatesRepository.UpdateRatesFile(ratesFile, request.UserName);
        }

        private void _SaveThirdPartyFilePrograms(RatesSaveRequest request, RatesFile ratesFile)
        {
            //Todo: fix or remove
            //var currentTime = DateTime.Now;
            //foreach (var stationProgram in ratesFile.StationPrograms)
            //{
            //    var spotLengthId = _SpotLengthMap[stationProgram.SpotLength];
            //    // check if program exists
            //    var programId = _stationProgramRepository.GetStationProgramIdByStationProgram(stationProgram, spotLengthId);
            //    if (programId > 0)
            //    {
            //        stationProgram.Id = programId;
            //        foreach (var flight in stationProgram.FlightWeeks)
            //        {
            //            // update the spot for flightweek
            //            _stationProgramRepository.UpdateFlightWeekSpot(programId, flight, currentTime, request.UserName);

            //            foreach (var programAudience in flight.Audiences)
            //            {
            //                var stationProgramFlight =
            //                    _stationProgramRepository.GetStationProgramFlightByProgramAndWeek(programId, flight.FlightWeek.Id);
            //                // check audience exists. it will add a audience to an existing fligth week audience or update the value 
            //                // by its spot
            //                if (
            //                    _stationProgramRepository.ProgramFlightAudienceExists(stationProgramFlight.id,
            //                        programAudience.Audience.Id) && stationProgram.SpotLength > 0)
            //                {
            //                    _stationProgramRepository.UpdateProgramFligthAudienceInventory(programId,
            //                        flight.FlightWeek.Id, stationProgram.SpotLength, programAudience);
            //                }
            //                else
            //                {
            //                    _stationProgramRepository.AddProgramFligthAudienceInventory(programId,
            //                        flight.FlightWeek.Id, programAudience, request.UserName, currentTime);
            //                }
            //            }
            //        }

            //        _stationProgramRepository.UpdateStationProgram(programId, currentTime, request.UserName, stationProgram.FixedPrice);
            //    }
            //    else
            //    {
            //        var station = _stationRepository.GetBroadcastStationByCode(stationProgram.StationCode);
            //        stationProgram.Id = _stationProgramRepository.CreateStationProgramRate(station, stationProgram, ratesFile.RateSource, spotLengthId,
            //            request.UserName, ratesFile.Id);
            //    }
            //}
        }




        public List<StationContact> GetStationContacts(string rateSource, int stationCode)
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

        public StationDetailDto GetStationDetailByCode(string rateSourceString, int stationCode)
        {
            return new StationDetailDto();
            //var rateSource = _ParseRateSource(rateSourceString);

            //var station = _stationRepository.GetBroadcastStationByCode(stationCode);

            //if (station == null)
            //{
            //    throw new BroadcastRateDataException("No station found with code: " + stationCode);
            //}

            //var programs = _stationProgramRepository.GetStationProgramsWithPrimaryAudienceRatesByStationCode(rateSource, stationCode);

            //var contacts =
            //    _broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>()
            //        .GetStationContactsByStationCode(stationCode);

            //var stationDto = new StationDetailDto()
            //{
            //    Affiliate = station.Affiliation,
            //    Market = station.OriginMarket,
            //    StationCode = stationCode,
            //    StationName = station.LegacyCallLetters,
            //    Rates = GetStationProgramAudienceRates(programs),
            //    Contacts = contacts
            //};

            //return stationDto;
        }


        public List<LookupDto> GetAllGenres()
        {
            return _genreRepository.GetAllGenres();
        }

        private void _SetFlightData(List<DisplayBroadcastStation> stations, DateTime currentDate)
        {

            var dateRangeThisQuarter = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, currentDate);
            var dateRangeNextQuarter = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.NEXTQUARTER, currentDate);

            stations.ForEach(s =>
            {
                s.FlightWeeks.ForEach(
                    fw =>
                    {
                        var mediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(fw.Id);
                        fw.StartDate = mediaWeek.StartDate;
                        fw.EndDate = mediaWeek.EndDate;
                    });

                if (s.FlightWeeks.Any())
                {
                    if (s.FlightWeeks.Last().EndDate.Date == dateRangeThisQuarter.Item2.Date)
                        s.RateDataThrough = "This Quarter";
                    else
                    {
                        s.RateDataThrough = s.FlightWeeks.Last().EndDate.Date == dateRangeNextQuarter.Item2.Date ? "Next Quarter" : s.FlightWeeks.Last().EndDate.ToShortDateString();
                    }
                }
                else
                {
                    // no data available
                    s.RateDataThrough = "-";
                }
            });
        }

        private RatesFile.RateSourceType _ParseRateSource(string sourceString)
        {
            RatesFile.RateSourceType rateSource;
            var parseSuccess = Enum.TryParse(sourceString, true, out rateSource);
            if (!parseSuccess)
            {
                throw new ArgumentException(string.Format("Invalid rate source parameter: {0}", sourceString));
            }
            return rateSource;
        }

        private RatesFile.RateSourceType _ParseRateSourceOrDefault(string sourceString)
        {
            if (String.IsNullOrEmpty(sourceString))
            {
                return RatesFile.RateSourceType.OpenMarket;
            }
            else
            {
                return _ParseRateSource(sourceString);
            }
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