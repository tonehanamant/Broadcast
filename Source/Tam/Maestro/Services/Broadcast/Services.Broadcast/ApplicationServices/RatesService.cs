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
        StationProgramAudienceRateDto CreateStationProgramRate(NewStationProgramDto programRate, string userName);
        bool DeleteProgramRates(int programId, DateTime startDate, DateTime endDate, string userName);
        bool UpdateProgramRate(int programId, StationProgramAudienceRateDto programRate, string userName);
        List<StationProgramAudienceRateDto> GetStationProgramConflicts(StationProgramConflictRequest conflict);
        bool GetStationProgramConflicted(StationProgramConflictRequest conflict, int programId);
        List<StationProgramAudienceRateDto> GetAllStationRates(string rateSource, int stationCode);
        List<StationProgramAudienceRateDto> GetStationRates(string rateSource, int stationCode, DateTime startDate, DateTime endDate);
        List<StationProgramAudienceRateDto> GetStationRates(string rateSource, int stationCode, string timeFrame, DateTime currentDate);
        List<StationContact> GetStationContacts(string rateSource, int stationCode);
        bool SaveStationContact(StationContact stationContacts, string userName);
        bool DeleteStationContact(int stationContactId, string userName);
        bool TrimProgramFlight(int programId, DateTime endDateValue, DateTime currenDate, string userName);
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
        private readonly IStationProgramRepository _stationProgramRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IRateFileImporterFactory _rateFileImporterFactory;
        private readonly ISMSClient _SmsClient;
        private readonly IRatesRepository _RatesRepository;
        private readonly ILockingManagerApplicationService _LockingManager;
        private readonly IRatingForecastService _RatingForecastService;
        private readonly IInventoryCrunchService _InventoryCrunchService;
        private readonly Dictionary<int, int> _SpotLengthMap;
        private readonly Dictionary<int, double> _SpotLengthCostMultipliers; 
        private readonly IThirdPartySpotCostCalculationEngine _ThirdPartySpotCostCalculationEngine;

        public RatesService(IDataRepositoryFactory broadcastDataRepositoryFactory, IRatesFileValidator ratesFileValidator,
                            IDaypartCache daypartCache, IBroadcastAudiencesCache audiencesCache, IQuarterCalculationEngine quarterCalculationEngine,
                            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache, IRateFileImporterFactory rateFileImporterFactory,
            ISMSClient smsClient, ILockingManagerApplicationService lockingManager,
                            IRatingForecastService ratingForecastService,
                            IInventoryCrunchService inventoryCrunchService,
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
            _stationProgramRepository = _broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _rateFileImporterFactory = rateFileImporterFactory;
            _SmsClient = smsClient;
            _RatesRepository = _broadcastDataRepositoryFactory.GetDataRepository<IRatesRepository>();
            _LockingManager = lockingManager;
            _RatingForecastService = ratingForecastService;
            _InventoryCrunchService = inventoryCrunchService;
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
            var stations = _stationRepository.GetBroadcastStations(_ParseRateSource(rateSource));
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
            var rateSourceType = _ParseRateSourceOrDefault(request.RateSource);
            var fileImporter = _rateFileImporterFactory.GetFileImporterInstance(rateSourceType);
            fileImporter.LoadFromSaveRequest(request);
            fileImporter.CheckFileHash();

            RatesFile ratesFile = fileImporter.GetPendingRatesFile();
            ratesFile.Id = _RatesRepository.CreateRatesFile(ratesFile, request.UserName);

            var stationLocks = new List<IDisposable>();
            var lockedStationCodes = new List<int>();
            var fileProblems = new List<RatesFileProblem>();
            try
            {

                var startTime = DateTime.Now;
                fileImporter.ExtractFileData(request.RatesStream, ratesFile, fileProblems);
                if (ratesFile.StationPrograms == null || ratesFile.StationPrograms.Count == 0)
                {
                    throw new ApplicationException("Unable to parse any file records.");
                }
                var endTime = DateTime.Now;
                System.Diagnostics.Debug.WriteLine(string.Format("Completed file parsing in {0}", endTime - startTime));
                startTime = DateTime.Now;
                var stationLegacyCallLettersList =
                    ratesFile.StationPrograms.Select(p => p.StationLegacyCallLetters).Distinct().ToList();
                var existingfileStations =
                    _stationRepository.GetBroadcastStationListByLegacyCallLetters(stationLegacyCallLettersList);

                foreach (var program in ratesFile.StationPrograms.ToList())
                {
                    try
                    {
                        var programStationCode = (short)existingfileStations
                            .Where(
                                s =>
                                    s.LegacyCallLetters.Equals(
                                        program.StationLegacyCallLetters,
                                        StringComparison.InvariantCultureIgnoreCase))
                            .Single(
                                string.Format(
                                    "Invalid station: {0}",
                                    program.StationLegacyCallLetters)).Code;
                        program.StationCode = programStationCode;
                    }
                    catch (Exception e)
                    {
                        ratesFile.StationPrograms.Remove(program);
                        fileProblems.Add(new RatesFileProblem()
                        {
                            ProblemDescription = e.Message,
                            ProgramName = program.ProgramName,
                            StationLetters = program.StationLegacyCallLetters
                        });

                    }
                }

                var validationProblems = _ratesFileValidator.ValidateRatesFile(ratesFile);
                fileProblems.AddRange(validationProblems.RatesFileProblems);
                // remove the invalid rates
                ratesFile.StationPrograms.RemoveAll(x => validationProblems.InvalidRates.Contains(x));

                endTime = DateTime.Now;
                System.Diagnostics.Debug.WriteLine(string.Format("Completed file validation in {0}", endTime - startTime));

                startTime = DateTime.Now;

                var fileStationCodes = ratesFile.StationPrograms.Select(p => (int)p.StationCode).Distinct().ToList();
                //using (var transaction = new TransactionScopeWrapper(new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(30))))
                using (var transaction = new TransactionScopeWrapper(_CreateTransactionScope(TimeSpan.FromMinutes(20))))
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
                                ratesFile.StationPrograms.Where(p => p.StationCode == stationCode)
                                    .First()
                                    .StationLegacyCallLetters;
                            throw new ApplicationException(string.Format("Unable to update station. Station locked for editing {0}.", stationLetters));
                        }
                    }

                    var isThirdParty = rateSourceType == RatesFile.RateSourceType.CNN ||
                                       rateSourceType == RatesFile.RateSourceType.TTNW ||
                                       rateSourceType == RatesFile.RateSourceType.TVB;

                    if (isThirdParty)
                    {
                        if (!request.RatingBook.HasValue)
                        {
                            throw new InvalidEnumArgumentException("Ratings book id required for third party rate files.");
                        }
                        _ThirdPartySpotCostCalculationEngine.CalculateSpotCost(request, ratesFile);
                    }

                    ratesFile.FileStatus = RatesFile.FileStatusEnum.Loaded;
                    _SaveRateFilePrograms(request, ratesFile);
                    _SaveRateFileContacts(request, ratesFile);

                    // crunch should happen only for ttnw, cnn and tvb and same transaction
                    if (isThirdParty)
                    {
                        var affectedProposals =
                            _InventoryCrunchService.CrunchThirdPartyInventory(ratesFile.StationPrograms, rateSourceType,
                                request.FlightWeeks);

                        if (affectedProposals != null && affectedProposals.Any())
                        {
                            fileProblems.Add(new RatesFileProblem()
                            {
                                ProblemDescription = "Import Updated Inventory reserved in the following Proposals",
                                AffectedProposals = affectedProposals.Select(a => a.ProblemDescription).ToList()
                            });
                        }
                    }


                    transaction.Complete();


                    //unlock stations
                    foreach (var stationCode in lockedStationCodes)
                    {
                        UnlockStation(stationCode);
                    }
                    foreach (var stationLock in stationLocks)
                    {
                        stationLock.Dispose();
                    }

                    endTime = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine(
                        string.Format("Completed file saving in {0}", endTime - startTime));
                }
            }
            catch (Exception e)
            {
                //Try to update the status of the file if possible
                try
                {
                    foreach (var stationCode in lockedStationCodes)
                    {
                        UnlockStation(stationCode);
                    }
                    foreach (var stationLock in stationLocks)
                    {
                        stationLock.Dispose();
                    }
                    _RatesRepository.UpdateRatesFileStatus(ratesFile.Id, RatesFile.FileStatusEnum.Failed);
                }
                catch
                {

                }
                throw new BroadcastRateDataException(string.Format("Error loading new rates file: {0}", e.Message),
                    ratesFile.Id, e);
            }

            return new RatesFileSaveResult()
            {
                FileId = ratesFile.Id,
                Problems = fileProblems
            };
        }

        private void _SaveRateFileContacts(RatesSaveRequest request, RatesFile ratesFile)
        {
            var fileStationCodes = ratesFile.StationPrograms.Select(p => (int)p.StationCode).Distinct().ToList();
            var existingStationContacts = _stationContactsRepository.GetStationContactsByStationCode(fileStationCodes);

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
            fileStationCodes.ForEach(code => _stationRepository.UpdateStation(code, request.UserName, timeStamp));
        }

        private void _SaveRateFilePrograms(RatesSaveRequest request, RatesFile ratesFile)
        {
            System.Diagnostics.Debug.WriteLine("Going to add programs");
            if (ratesFile.RateSource == RatesFile.RateSourceType.TVB ||
                ratesFile.RateSource == RatesFile.RateSourceType.CNN ||
                ratesFile.RateSource == RatesFile.RateSourceType.TTNW)
            {
                _SaveThirdPartyFilePrograms(request, ratesFile);
            }
            else
            {
                _stationProgramRepository.AddRateFilePrograms(ratesFile, request.UserName, _SpotLengthMap);
            }
            System.Diagnostics.Debug.WriteLine("Going to update stations");
            var stationCodes = ratesFile.StationPrograms.Select(y => (int)y.StationCode).Distinct().ToList();
            _stationRepository.UpdateStationList(stationCodes, request.UserName, DateTime.Now);
            System.Diagnostics.Debug.WriteLine("Going to update file record");
            _RatesRepository.UpdateRatesFile(ratesFile, request.UserName);
        }

        private void _SaveThirdPartyFilePrograms(RatesSaveRequest request, RatesFile ratesFile)
        {
            var currentTime = DateTime.Now;
            foreach (var stationProgram in ratesFile.StationPrograms)
            {
                var spotLengthId = _SpotLengthMap[stationProgram.SpotLength];
                // check if program exists
                var programId = _stationProgramRepository.GetStationProgramIdByStationProgram(stationProgram, spotLengthId);
                if (programId > 0)
                {
                    stationProgram.Id = programId;
                    foreach (var flight in stationProgram.FlightWeeks)
                    {
                        // update the spot for flightweek
                        _stationProgramRepository.UpdateFlightWeekSpot(programId, flight, currentTime, request.UserName);

                        foreach (var programAudience in flight.Audiences)
                        {
                            var stationProgramFlight =
                                _stationProgramRepository.GetStationProgramFlightByProgramAndWeek(programId, flight.FlightWeek.Id);
                            // check audience exists. it will add a audience to an existing fligth week audience or update the value 
                            // by its spot
                            if (
                                _stationProgramRepository.ProgramFlightAudienceExists(stationProgramFlight.id,
                                    programAudience.Audience.Id) && stationProgram.SpotLength > 0)
                            {
                                _stationProgramRepository.UpdateProgramFligthAudienceInventory(programId,
                                    flight.FlightWeek.Id, stationProgram.SpotLength, programAudience);
                            }
                            else
                            {
                                _stationProgramRepository.AddProgramFligthAudienceInventory(programId,
                                    flight.FlightWeek.Id, programAudience, request.UserName, currentTime);
                            }
                        }
                    }

                    _stationProgramRepository.UpdateStationProgram(programId, currentTime, request.UserName, stationProgram.FixedPrice);
                }
                else
                {
                    var station = _stationRepository.GetBroadcastStationByCode(stationProgram.StationCode);
                    stationProgram.Id = _stationProgramRepository.CreateStationProgramRate(station, stationProgram, ratesFile.RateSource, spotLengthId,
                        request.UserName, ratesFile.Id);
                }
            }
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
            var rateSource = _ParseRateSource(rateSourceString);

            var station = _stationRepository.GetBroadcastStationByCode(stationCode);

            if (station == null)
            {
                throw new BroadcastRateDataException("No station found with code: " + stationCode);
            }

            var programs = _stationProgramRepository.GetStationProgramsWithPrimaryAudienceRatesByStationCode(rateSource, stationCode);

            var contacts =
                _broadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>()
                    .GetStationContactsByStationCode(stationCode);

            var stationDto = new StationDetailDto()
            {
                Affiliate = station.Affiliation,
                Market = station.OriginMarket,
                StationCode = stationCode,
                StationName = station.LegacyCallLetters,
                Rates = GetStationProgramAudienceRates(programs),
                Contacts = contacts
            };

            return stationDto;
        }

        private List<StationProgramAudienceRateDto> GetStationProgramAudienceRates(List<StationProgram> programs)
        {
            var stationProgramRates = new List<StationProgramAudienceRateDto>();

            try
            {
                foreach (var program in programs)
                {
                    //We're generating a key and grouping by contiguous records that have the same rate, ratings and impressions
                    int contiguousKey = 0;
                    var groupedFlightWeeks = program.FlightWeeks.Select(
                        (f, i) =>
                            i == 0
                                ? new
                                {
                                    Flight = f,
                                    ContiguousKey = contiguousKey
                                }
                                : new
                                {
                                    Flight = f,
                                    ContiguousKey = (f.Rate15s == program.FlightWeeks[i - 1].Rate15s) &&
                                                    (f.Rate30s == program.FlightWeeks[i - 1].Rate30s) &&
                                                    (f.Rate60s == program.FlightWeeks[i - 1].Rate60s) &&
                                                    (f.Rate90s == program.FlightWeeks[i - 1].Rate90s) &&
                                                    (f.Rate120s == program.FlightWeeks[i - 1].Rate120s) &&
                                                    ((f.Audiences.Any() && program.FlightWeeks[i - 1].Audiences.Any() &&
                                                     f.Audiences.First().Impressions ==
                                                     program.FlightWeeks[i - 1].Audiences.First().Impressions) ||
                                                     !f.Audiences.Any() && !program.FlightWeeks[i - 1].Audiences.Any()) && // cnn might not have audiences
                                                    ((f.Audiences.Any() && program.FlightWeeks[i - 1].Audiences.Any() &&
                                                     f.Audiences.First().Rating ==
                                                     program.FlightWeeks[i - 1].Audiences.First().Rating) ||
                                                     !f.Audiences.Any() && !program.FlightWeeks[i - 1].Audiences.Any())

                                        ? contiguousKey
                                        : ++contiguousKey
                                }).GroupBy(a => a.ContiguousKey);

                    var programAudienceRates = groupedFlightWeeks.Select(
                        g =>
                        {
                            var stationProgramAudience = new StationProgramAudienceRateDto();
                            stationProgramAudience.Id = program.Id;
                            stationProgramAudience.Airtime = GetProgramDaypartName(program);
                            stationProgramAudience.Flights =
                                _MediaMonthAndWeekAggregateCache.GetMediaWeeksByIdList(
                                    g.Select(a => a.Flight.FlightWeek.Id).ToList())
                                    .Select(mw => new FlightWeekDto()
                                    {
                                        Id = mw.Id,
                                        StartDate = mw.StartDate,
                                        EndDate = mw.EndDate,
                                        IsHiatus = !g.Single(a => a.Flight.FlightWeek.Id == mw.Id).Flight.Active
                                    }
                                    ).ToList();
                            stationProgramAudience.Flight = GetProgramFlightString(g.Min(a => a.Flight.FlightWeek.Id),
                                g.Max(a => a.Flight.FlightWeek.Id));
                            stationProgramAudience.FlightStartDate =
                                _MediaMonthAndWeekAggregateCache.GetMediaWeekById(g.Min(a => a.Flight.FlightWeek.Id))
                                    .StartDate;
                            stationProgramAudience.FlightEndDate =
                                _MediaMonthAndWeekAggregateCache.GetMediaWeekById(g.Max(a => a.Flight.FlightWeek.Id))
                                    .EndDate;
                            stationProgramAudience.Rate15 = g.First().Flight.Rate15s;
                            stationProgramAudience.Rate30 = g.First().Flight.Rate30s;
                            stationProgramAudience.Genres = program.Genres;
                            stationProgramAudience.Program = program.ProgramName;
                            stationProgramAudience.Spots = g.First().Flight.Spots;

                            // cnn might not have audiences
                            if (g.First().Flight.Audiences.Any())
                            {
                                stationProgramAudience.AudienceId = g.First().Flight.Audiences.First().Audience.Id;
                                stationProgramAudience.Impressions = g.First().Flight.Audiences.First().Impressions;
                                stationProgramAudience.Rating = g.First().Flight.Audiences.First().Rating;

                                stationProgramAudience.Audiences =
                                    g.First().Flight.Audiences.Select(a => new StationProgramFlightWeekAudience()
                                    {
                                        Audience = _audiencesCache.GetDisplayAudienceById(a.Audience.Id),
                                        Rating = a.Rating,
                                        Impressions = a.Impressions,
                                        Cpm120 = a.Cpm120,
                                        Cpm15 = a.Cpm15,
                                        Cpm30 = a.Cpm30,
                                        Cpm60 = a.Cpm60,
                                        Cpm90 = a.Cpm90
                                    }).ToList();
                            }

                            return stationProgramAudience;
                        }).ToList();

                    stationProgramRates.AddRange(programAudienceRates);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + e.StackTrace);
                throw;
            }

            return stationProgramRates;
        }

        public StationProgramAudienceRateDto CreateStationProgramRate(NewStationProgramDto programRate, string userName)
        {
            if (programRate == null)
            {
                throw new Exception("No Station Rate data received.");
            }

            if (programRate.Flights == null || programRate.Flights.Count == 0)
            {
                throw new Exception("No Station Program Flight data received.");
            }

            StationProgramConflictRequest programConflictRequest = new StationProgramConflictRequest()
            {
                Airtime = programRate.Airtime,
                StartDate = programRate.FlightStartDate,
                EndDate = programRate.FlightEndDate
            };

            using (new BomsLockManager(_SmsClient, new StationToken(programRate.StationCode)))
            using (var transaction = new TransactionScopeWrapper())
            {
                var station = _stationRepository.GetBroadcastStationByCode(programRate.StationCode);
                DisplayDaypart daypart = DaypartDto.ConvertDaypartDto(programRate.Airtime);
                daypart.Id = _daypartCache.GetIdByDaypart(daypart);

                // update conflict programs
                if (programRate.Conflicts != null && programRate.Conflicts.Any())
                {
                    foreach (var conflict in programRate.Conflicts)
                    {
                        // get flights, start/end week id, remove flights outside of media week ids and then update existing flights isHiatus flag
                        var startWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(conflict.FlightStartDate).Id;
                        var endWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(conflict.FlightEndDate).Id;

                        var programFlightsToDelete = _stationProgramRepository.GetStationProgramFlightsById(conflict.Id)
                            .Where(f => f.media_week_id < startWeek || f.media_week_id > endWeek)
                            .Select(f => new MediaWeek() { Id = f.media_week_id }).ToList();

                        _stationProgramRepository.TrimProgramFlight(conflict.Id, programFlightsToDelete, conflict.FlightStartDate,
                            conflict.FlightEndDate, userName);
                        _stationProgramRepository.UpdateProgramFlights(conflict.Id, conflict.Flights, userName);
                    }
                }

                StationProgram newProgram = new StationProgram()
                {
                    StationLegacyCallLetters = station.LegacyCallLetters,
                    StationCode = (short)station.Code,
                    ProgramName = programRate.Program,
                    StartDate = programRate.FlightStartDate,
                    EndDate = programRate.FlightEndDate,
                    Daypart = daypart,
                    DayPartName = daypart.Preview,
                    FlightWeeks = programRate.Flights.Select(f => new StationProgramFlightWeek()
                    {
                        FlightWeek = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(f.StartDate, f.EndDate).First(),
                        Active = !f.IsHiatus,
                        Rate15s = programRate.Rate15,
                        Rate30s = programRate.Rate30,
                        Audiences = new List<StationProgramFlightWeekAudience>() {
                        new StationProgramFlightWeekAudience()
                        {
                            Impressions = programRate.Impressions,
                            Rating = programRate.Rating,
                            Audience = new DisplayAudience()
                            {
                                Id = _audiencesCache.GetDefaultAudience().Id
                            }
                        }
                    }
                    }).ToList()
                };

                // add program and get id
                newProgram.Id = _stationProgramRepository.CreateStationProgramRate(station, newProgram, programRate.RateSource, _SpotLengthMap[newProgram.SpotLength], userName, null);

                // update genres
                newProgram.Genres = UpdateGenres(programRate.Genres, userName);

                // update program genres
                _stationProgramRepository.AddStationProgramGenres(newProgram.Id, newProgram.Genres);

                var timeStamp = DateTime.Now;
                // update staion modified date
                _stationRepository.UpdateStation(programRate.StationCode, userName, timeStamp);
                // update program modified date
                _stationProgramRepository.UpdateStationProgram(newProgram.Id, timeStamp, userName);

                transaction.Complete();

                return GetStationProgramAudienceRates(new List<StationProgram>() { newProgram }).SingleOrDefault();
            }
        }

        public bool DeleteProgramRates(int programId, DateTime startDate, DateTime endDate, string userName)
        {
            if (startDate == null || endDate == null)
            {
                throw new BroadcastRateDataException(
                    "Cannot delete program flights/rates without specifying start and end dates.");
            }
            var stationCode = _stationRepository.GetBroadcastStationCodeByProgramId(programId);

            using (new BomsLockManager(_SmsClient, new StationToken(stationCode)))
            using (var transaction = new TransactionScopeWrapper())
            {
                var result = true;

                var startWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(startDate);
                var endWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(endDate);

                // update station modified date
                _stationRepository.UpdateStation(stationCode, userName, DateTime.Now);

                _stationProgramRepository.DeleteProgramRates(programId, startWeek.Id, endWeek.Id, userName,
                    _MediaMonthAndWeekAggregateCache);

                transaction.Complete();

                return result;
            }
        }

        public bool UpdateProgramRate(int programId, StationProgramAudienceRateDto programRate, string userName)
        {
            if (programRate == null)
            {
                throw new Exception("No Station Rate data received.");
            }

            var stationCode = _stationRepository.GetBroadcastStationCodeByProgramId(programId);

            using (new BomsLockManager(_SmsClient, new StationToken(stationCode)))
            using (var transaction = new TransactionScopeWrapper())
            {
                var startWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(programRate.FlightStartDate);

                var endWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(programRate.FlightEndDate);
                programRate.AudienceId = _audiencesCache.GetDefaultAudience().Id;

                _stationProgramRepository.UpdateProgramRate(programId, programRate, userName, startWeek.Id, endWeek.Id);

                _stationProgramRepository.RemoveStationProgramGenres(programId);   // remove program genre associations

                programRate.Genres = UpdateGenres(programRate.Genres, userName);

                // update program genres
                _stationProgramRepository.AddStationProgramGenres(programId, programRate.Genres);

                var timeStamp = DateTime.Now;
                _stationRepository.UpdateStation(stationCode, userName, timeStamp);
                //update program last modified date
                _stationProgramRepository.UpdateStationProgram(programId, timeStamp, userName);

                transaction.Complete();

                return true;
            }
        }

        public List<StationProgramAudienceRateDto> GetStationProgramConflicts(StationProgramConflictRequest conflict)
        {

            if (conflict.StartDate == DateTime.MinValue || conflict.EndDate == DateTime.MinValue)
            {
                throw new Exception(String.Format("Unable to parse start and end date values: {0}, {1}", conflict.StartDate, conflict.EndDate));
            }

            DisplayBroadcastStation station = _stationRepository.GetBroadcastStationByCode(conflict.StationCode);

            DisplayDaypart airtime = DaypartDto.ConvertDaypartDto(conflict.Airtime);
            airtime.Id = _daypartCache.GetIdByDaypart(airtime);

            // get programs within conflict date range
            var programs = _stationProgramRepository
                .GetStationProgramsWithPrimaryAudienceRatesByStationCodeAndDates(conflict.RateSource, station.Code, conflict.StartDate, conflict.EndDate);

            // filter based on date range intersecting and day part intersecting with program    
            var filteredPrograms = programs
                .Where(sp => DateRangesIntersect(sp.StartDate, sp.EndDate, conflict.StartDate, conflict.EndDate) &&
                    DisplayDaypart.Intersects(_daypartCache.GetDisplayDaypart(sp.Daypart.Id), airtime)).ToList();

            return GetStationProgramAudienceRates(filteredPrograms);
        }

        public bool GetStationProgramConflicted(StationProgramConflictRequest conflict, int programId)
        {

            if (conflict.StartDate == DateTime.MinValue || conflict.EndDate == DateTime.MinValue || conflict.ConflictedProgramNewStartDate == DateTime.MinValue || conflict.ConflictedProgramNewEndDate == DateTime.MinValue)
            {
                throw new Exception(String.Format("Unable to parse start and end date values: {0}, {1}, {2}, {3}", conflict.StartDate, conflict.EndDate, conflict.ConflictedProgramNewStartDate, conflict.ConflictedProgramNewEndDate));
            }

            // check date range intersecting
            bool isConflicted = DateRangesIntersect(conflict.ConflictedProgramNewStartDate, conflict.ConflictedProgramNewEndDate, conflict.StartDate, conflict.EndDate);

            // if supplied airtime, check day part intersecting
            if (conflict.Airtime != null)
            {
                // get program and convert day part, then check intersection
                var program = _stationProgramRepository.GetStationProgramById(programId);

                DisplayDaypart daypart = DaypartDto.ConvertDaypartDto(conflict.Airtime);
                daypart.Id = _daypartCache.GetIdByDaypart(daypart);


                isConflicted = isConflicted && DisplayDaypart.Intersects(_daypartCache.GetDisplayDaypart(program.daypart_id), daypart);
            }

            return isConflicted;
        }

        private bool DateRangesIntersect(DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2)
        {
            return (startDate1 >= startDate2 && startDate1 <= endDate2)
                   || (endDate1 >= startDate2 && endDate1 <= endDate2)
                   || (startDate1 < startDate2 && endDate1 > endDate2);
        }

        private string GetProgramFlightString(int week1, int week2)
        {
            var mediaWeek1 = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(week1);
            var mediaWeek2 = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(week2);
            return mediaWeek1.StartDate.ToString("yyyy/MM/dd") + " - " + mediaWeek2.EndDate.ToString("yyyy/MM/dd");
        }

        private string GetProgramDaypartName(StationProgram program)
        {
            return _daypartCache.GetDisplayDaypart(program.Daypart.Id).Preview;
        }

        private List<LookupDto> UpdateGenres(List<LookupDto> genres, string userName)
        {
            List<LookupDto> returnGenres = new List<LookupDto>();

            foreach (var genre in genres)
            {
                var foundGenre = _genreRepository.GetGenre(genre.Display.Trim());
                if (foundGenre == null)
                    genre.Id = _genreRepository.AddGenre(genre, userName);
                else
                    genre.Id = foundGenre.Id;

                returnGenres.Add(genre);
            }


            return returnGenres;
        }

        public List<StationProgramAudienceRateDto> GetAllStationRates(string rateSourceString, int stationCode)
        {
            var rateSource = _ParseRateSource(rateSourceString);
            var programs = _stationProgramRepository.GetStationProgramsWithPrimaryAudienceRatesByStationCode(rateSource, stationCode);
            return GetStationProgramAudienceRates(programs);
        }

        public List<StationProgramAudienceRateDto> GetStationRates(string rateSourceString, int stationCode, DateTime startDate, DateTime endDate)
        {
            var rateSource = _ParseRateSource(rateSourceString);
            var programs = _stationProgramRepository.GetStationProgramsWithPrimaryAudienceRatesByStationCodeAndDates(rateSource, stationCode, startDate, endDate);

            //now we group flight weeks and only return the matching groups
            return GetStationProgramAudienceRates(programs).Where(ar => (ar.FlightStartDate >= startDate && ar.FlightStartDate <= endDate)
                                                                     || (ar.FlightEndDate >= startDate && ar.FlightEndDate <= endDate)
                                                                     || (ar.FlightStartDate < startDate && ar.FlightEndDate > endDate)).ToList();
        }

        public List<StationProgramAudienceRateDto> GetStationRates(string rateSourceString, int stationCode, string timeFrame, DateTime currentDate)
        {

            RatesTimeframe timeFrameValue;
            try
            {
                var succeeded = RatesTimeframe.TryParse(timeFrame.ToUpper(), true, out timeFrameValue);
                if (!succeeded) throw new ArgumentException(string.Format("Invalid timeframe specified: {0}.", timeFrame));
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Unable to parse rate timeframe for {0}: {1}", timeFrame, e.Message));
            }

            var dateRange = _QuarterCalculationEngine.GetDatesForTimeframe(timeFrameValue, currentDate);

            return GetStationRates(rateSourceString, stationCode, dateRange.Item1, dateRange.Item2);
        }

        public bool TrimProgramFlight(int stationProgramId, DateTime endDateValue, DateTime currentDate, string userName)
        {
            var adjustedEndDate = DateFormatter.AdjustEndDate(endDateValue);

            // invalid program
            var stationProgram = _stationProgramRepository.GetStationProgramById(stationProgramId);

            // picked a end date > program´s end date
            if (adjustedEndDate > stationProgram.end_date)
                throw new Exception("Cannot select a date beyond the original flight of the program.");

            // can only select flight in the future
            if (adjustedEndDate < DateFormatter.AdjustStartDate(currentDate))
                throw new Exception("Please select a flight week in the future.");

            var stationProgramFlights = _stationProgramRepository.GetStationProgramFlightsById(stationProgramId);

            // doesn´t have program flights
            if (stationProgramFlights == null)
                throw new Exception(String.Format("Cannot find program flights for id {0}.", stationProgramId));

            // picked invalid end date
            var endWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(adjustedEndDate);

            // the end date is not in the flight
            var flight = stationProgramFlights.Single(q => q.media_week_id == endWeek.Id, String.Format("Cannot find flight for date {0}.", adjustedEndDate.ToShortDateString()));
            if (flight == null)
                throw new Exception(String.Format("Cannot find flight for date {0}.", adjustedEndDate.ToShortDateString()));

            // the selected week is hiatus
            if (!flight.active)
                throw new Exception("Cannot select a date that is inactive or hiatus period.");

            // get the selected media week ids
            var selectedMediaWeeks =
                _MediaMonthAndWeekAggregateCache.GetMediaWeeksByIdList(stationProgramFlights.Select(x => x.media_week_id).ToList());

            var stationCode = _stationRepository.GetBroadcastStationCodeByProgramId(stationProgramId);

            using (new BomsLockManager(_SmsClient, new StationToken(stationCode)))
            using (var transaction = new TransactionScopeWrapper())
            {
                if (selectedMediaWeeks.Any())
                {
                    // get the weeeks that needs to be deleted
                    var mediaWeeksToRemove = selectedMediaWeeks.Where(q => q.EndDate > adjustedEndDate).ToList();
                    if (mediaWeeksToRemove.Any())
                    {
                        // set the new end date for the program
                        _stationProgramRepository.TrimProgramFlight(stationProgramId, mediaWeeksToRemove, null, adjustedEndDate, userName);
                    }
                }

                // update staion modified date
                _stationRepository.UpdateStation(stationCode, userName, DateTime.Now);

                transaction.Complete();
                return true;
            }
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