using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Converters.RateImport.OpenMarketFileImporter.AvailLineWithPeriods;
using static Services.Broadcast.Converters.RateImport.OpenMarketFileImporter.AvailLineWithPeriods.AvailLineWithPeriodsDayTimes;
using static Services.Broadcast.Converters.RateImport.OpenMarketFileImporter.AvailLineWithPeriods.AvailLineWithPeriodsDayTimes.AvailLineWithPeriodsDayTimesDayTime;
using static Services.Broadcast.Converters.RateImport.OpenMarketFileImporter.AvailLineWithPeriods.Period;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IOpenMarketFileImporter : IApplicationService, IBaseInventoryFileImporter
    {
        void ExtractFileData(Stream rawStream, InventoryFile inventoryFile);
        void CheckFileHash();
        void LoadFromSaveRequest(InventoryFileSaveRequest request);
        InventoryFile GetPendingInventoryFile(InventorySource InventorySource, string userName, DateTime nowDate);

        //Access to FileProblems list
        List<InventoryFileProblem> FileProblems { get; set; }
    }

    public class OpenMarketFileImporter : BaseInventoryFileImporter, IOpenMarketFileImporter
    {
        private const int SecondsPerMinute = 60;

        public List<InventoryFileProblem> FileProblems { get; set; } = new List<InventoryFileProblem>();
        private InventoryFileSaveRequest _Request { get; set; }
        private string _FileHash { get; set; }

        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IDaypartCache _DaypartCache;
        private readonly IInventoryDaypartParsingEngine _InventoryDaypartParsingEngine;
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly IStationProcessingEngine _StationEngine;
        private readonly IStationMappingService _StationMappingService;
        private readonly List<DisplayBroadcastStation> _AvailableStations;

        /// <summary>
        /// Spot lengths dictionary where key is the duration and value is the id
        /// </summary>
        private readonly Lazy<Dictionary<int, int>> _SpotLengthIdsByDuration;

        /// <summary>
        /// Spot lengths dictionary where key is the id and value is the duration
        /// </summary>
        private readonly Lazy<Dictionary<int, int>> _SpotLengthDurationsById;

        public OpenMarketFileImporter(IDataRepositoryFactory dataRepositoryFactory
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IDaypartCache daypartCache
            , IInventoryDaypartParsingEngine inventoryDaypartParsingEngine
            , IBroadcastAudiencesCache broadcastAudiencesCache
            , IStationProcessingEngine stationProcessingEngine
            , IStationMappingService stationMappingService)
        {
            _BroadcastDataRepositoryFactory = dataRepositoryFactory;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _DaypartCache = daypartCache;
            _InventoryDaypartParsingEngine = inventoryDaypartParsingEngine;
            _AudienceCache = broadcastAudiencesCache;
            _StationEngine = stationProcessingEngine;
            _StationMappingService = stationMappingService;
            _AvailableStations = dataRepositoryFactory.GetDataRepository<IStationRepository>().GetBroadcastStations();

            _SpotLengthIdsByDuration = new Lazy<Dictionary<int, int>>(() => _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsByDuration());
            _SpotLengthDurationsById = new Lazy<Dictionary<int, int>>(() => _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthDurationsById());
        }

        protected Dictionary<int, double> _SpotLengthMultipliers;

        public void ExtractFileData(Stream rawStream, InventoryFile inventoryFile)
        {
            try
            {
                var message = _DeserializeAaaaMessage(rawStream);

                Debug.WriteLine(message.Proposal.uniqueIdentifier + " parsed successfully !");

                var rowsProcessed = 0;

                BuildRatesFile(message, inventoryFile, ref rowsProcessed);

                if (!FileProblems.Any())
                {
                    inventoryFile.RowsProcessed = rowsProcessed;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to parse rates file: {e.Message} The file may be invalid: {inventoryFile.FileName}", e);
            }
        }

        private void BuildRatesFile(AAAAMessage message, InventoryFile inventoryFile, ref int rowsProcessed)
        {
            var proposal = message.Proposal;

            inventoryFile.UniqueIdentifier = proposal.uniqueIdentifier;

            rowsProcessed = _PopulateStationProgramList(message.Proposal, inventoryFile);
            if (inventoryFile.ValidationProblems.Any() || FileProblems.Any()) return;

            inventoryFile.StationContacts = _ExtractContactData(message);
            PopulateInventoryFileDateRange(inventoryFile);
        }

        private int _PopulateStationProgramList(AAAAMessageProposal proposal, InventoryFile inventoryFile)
        {
            if (!IsValid(proposal.AvailList))
            {
                inventoryFile.ValidationProblems.Add("Can't find XML elements in order to build station programs from.");
                return 0;
            }

            var taskResults = new ConcurrentBag<StationInventoryManifest>();
            var taskProblems = new ConcurrentBag<InventoryFileProblem>();
            var taskList = new List<Task<int>>();

            foreach (var availList in proposal.AvailList)
            {
                taskList.Add(Task.Run(() =>
                {
                    var availLines = new List<AvailLineWithPeriods>();

                    if (availList.AvailLineWithDetailedPeriods != null)
                    {
                        availLines.AddRange(availList.AvailLineWithDetailedPeriods.Select(_Map));
                    }

                    if (availList.AvailLineWithPeriods != null)
                    {
                        availLines.AddRange(availList.AvailLineWithPeriods.Select(_Map));
                    }

                    var processed = _PopulateProgramsFromAvailLineWithPeriods(proposal, availList, availLines, taskProblems, taskResults);
                    return processed;
                }));
            }

            var totalRowsProcessed = Task.WhenAll(taskList).GetAwaiter().GetResult().Sum();

            var taskResultInventoryManifests = taskResults.ToList();
            var taskResultProblems = taskProblems.ToList();

            var cleanInventoryManifests = InventoryImportManifestDuplicateHandler.ScrubInventoryManifests(taskResultInventoryManifests, _SpotLengthDurationsById.Value);

            inventoryFile.InventoryManifests = cleanInventoryManifests;
            FileProblems = taskResultProblems;

            return totalRowsProcessed;
        }

        private bool IsValid(AAAAMessageProposalAvailList[] availLists)
        {
            foreach (var availList in availLists)
            {
                if (availList == null || (availList.AvailLineWithDetailedPeriods == null && availList.AvailLineWithPeriods == null))
                    return false;
            }

            return true;
        }

        private InventoryFileProblem _CheckSpotLength(int spotLength, string stationLetters, string programName)
        {
            return !_SpotLengthIdsByDuration.Value.ContainsKey(spotLength) ? new InventoryFileProblem()
            {
                ProblemDescription = $"Unknown spot length found: {spotLength}",
                ProgramName = programName,
                StationLetters = stationLetters
            } : null;
        }

        private List<StationInventoryManifestRate> _GetManifestRatesforAvailLineWithDetailedPeriods(
            int spotLengthId,
            int spotLength,
            string linePeriodRate,
            string programName,
            string stationCallLetters,
            ConcurrentBag<InventoryFileProblem> fileProblems)
        {
            var manifestRates = new List<StationInventoryManifestRate>();
            var availLineRate = string.IsNullOrEmpty(linePeriodRate) ? 0 : decimal.Parse(linePeriodRate);

            if (spotLength == 30)
            {
                manifestRates.AddRange(_GetManifestRatesFromMultipliers(availLineRate));
            }
            else
            {
                var spotLengthProblem = _CheckSpotLength(spotLength, stationCallLetters, programName);

                if (spotLengthProblem != null)
                {
                    fileProblems.Add(spotLengthProblem);
                }
                else
                {
                    var manifestRate = new StationInventoryManifestRate()
                    {
                        SpotLengthId = spotLengthId,
                        SpotCost = availLineRate
                    };
                    manifestRates.Add(manifestRate);
                }
            }

            return manifestRates;
        }

        private int _PopulateProgramsFromAvailLineWithPeriods(
            AAAAMessageProposal proposal,
            AAAAMessageProposalAvailList availList,
            List<AvailLineWithPeriods> availLines,
            ConcurrentBag<InventoryFileProblem> fileProblems,
            ConcurrentBag<StationInventoryManifest> results)
        {
            var audienceMap = _GetAudienceMap(availList.DemoCategories);
            var allStationNames = proposal.Outlets.Select(o => o.callLetters).Distinct().ToList();
            int rowsProcessed = 0;
            foreach (var availLine in availLines)
            {
                var programName = availLine.AvailName;

                try
                {
                    var outletRef = availList.OutletReferences
                        .Where(a => a.outletForListId == availLine.OutletReference.OutletFromListRef)
                        .Select(a => a.outletFromProposalRef)
                        .First();
                    var callLetters = proposal.Outlets
                        .Where(a => a.outletId == outletRef)
                        .Select(a => a.callLetters)
                        .First();

                    var station = _StationMappingService.GetStationByCallLetters(callLetters);

                    var spotLength = availLine.SpotLength.Minute * SecondsPerMinute + availLine.SpotLength.Second;
                    var spotLengthProblem = _CheckSpotLength(spotLength, callLetters, programName);

                    if (spotLengthProblem != null)
                    {
                        fileProblems.Add(spotLengthProblem);
                        continue;
                    }

                    var spotLengthId = _SpotLengthIdsByDuration.Value[spotLength];

                    foreach (var availLinePeriod in availLine.Periods)
                    {
                        var manifestAudiences = _GetManifestAudienceListForAvailLine(availList, audienceMap, _ToDemoValueDict(availLinePeriod.DemoValues));
                        var manifestRates = _GetManifestRatesforAvailLineWithDetailedPeriods(spotLengthId, spotLength, availLinePeriod.Rate, programName, callLetters, fileProblems);
                        var manifestWeeks = _GetManifestWeeksForAvailLine(availLinePeriod.StartDate, availLinePeriod.EndDate);

                        // let`s group all weeks by quarter and create a manifest per each quarter
                        var allMediaMonthIds = manifestWeeks.Select(x => x.MediaWeek.MediaMonthId).Distinct();
                        var allMediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(allMediaMonthIds);
                        var weeksGroupedByQuarter = manifestWeeks
                            .Select(x => new
                            {
                                week = x,
                                quarter = allMediaMonths.Single(m => m.Id == x.MediaWeek.MediaMonthId).QuarterAndYearText
                            })
                            .GroupBy(x => x.quarter);

                        foreach (var weeksGroup in weeksGroupedByQuarter)
                        {
                            results.Add(new StationInventoryManifest
                            {
                                Station = station,
                                DaypartCode = availLine.DaypartName,
                                SpotLengthId = spotLengthId,
                                ManifestDayparts = _GetDaypartsListForAvailLineWithPeriods(availLine),
                                ManifestAudiencesReferences = manifestAudiences,
                                ManifestRates = manifestRates,
                                ManifestWeeks = weeksGroup.Select(x => x.week).ToList()
                            });
                        }

                        if (weeksGroupedByQuarter.Any())
                            rowsProcessed++;
                    }
                }
                catch (Exception e)
                {
                    fileProblems.Add(new InventoryFileProblem($"Error while processing {availLine.AvailName} on {availList.Name}: {e.Message}"));
                }
            }
            return rowsProcessed;
        }

        private List<StationInventoryManifestWeek> _GetManifestWeeksForAvailLine(DateTime startDate, DateTime endDate)
        {
            var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(startDate, endDate);
            return mediaWeeks.Select(week => new StationInventoryManifestWeek
            {
                MediaWeek = week,
                StartDate = startDate >= week.StartDate ? startDate : week.StartDate,
                EndDate = endDate <= week.EndDate ? endDate : week.EndDate
            }).ToList();
        }

        private List<StationInventoryManifestAudience> _GetManifestAudienceListForAvailLine(
            AAAAMessageProposalAvailList availList,
            Dictionary<string, DisplayAudience> audienceMap,
            Dictionary<string, decimal> demoValues)
        {
            var manifestAudiences = new List<StationInventoryManifestAudience>();

            foreach (var demoValue in demoValues)
            {
                var demo = availList.DemoCategories.Where(a => a.DemoId == demoValue.Key).First();
                var audience = audienceMap[demo.DemoId];
                var manifestAudience = manifestAudiences.Where(a => a.Audience.Id == audience.Id).FirstOrDefault();
                var newManifestAudience = false;

                if (manifestAudience == null)
                {
                    newManifestAudience = true;
                    manifestAudience = new StationInventoryManifestAudience();
                    manifestAudience.Audience = audience;
                    manifestAudience.IsReference = true;
                }
                if (demoValue.Value < 0)
                {
                    throw new Exception($"Demo value cannot be negative. Audience: '{audience.AudienceString}', demo type: '{demo.DemoType}'");
                }
                switch (demo.DemoType)
                {
                    case "Impression":
                        manifestAudience.Impressions = (double)demoValue.Value * 1000;
                        break;
                    case "Rating":
                        manifestAudience.Rating = (double)demoValue.Value;
                        break;
                    default:
                        throw new Exception("Unknown DemoType [" + demo.DemoType + "] in rate file.");
                }

                if (newManifestAudience)
                {
                    manifestAudiences.Add(manifestAudience);
                }
            }

            return manifestAudiences;
        }

        public void CheckFileHash()
        {
            //check if file has already been loaded
            if (_BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>()
                    .GetInventoryFileIdByHash(_FileHash) > 0)
            {
                throw new BroadcastDuplicateInventoryFileException(
                    "Unable to load rate file. The selected rate file has already been loaded or is already loading.");
            }
        }

        public void LoadFromSaveRequest(InventoryFileSaveRequest request)
        {
            _Request = request;
            _FileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(request.StreamData));
            _SpotLengthMultipliers = _GetSpotLengthAndMultipliers();
        }

        public InventoryFile GetPendingInventoryFile(InventorySource InventorySource, string userName, DateTime nowDate)
        {
            return new InventoryFile
            {
                FileName = _Request.FileName ?? "unknown",
                FileStatus = FileStatusEnum.Pending,
                Hash = _FileHash,
                InventorySource = InventorySource,
                CreatedBy = userName,
                CreatedDate = nowDate
            };
        }

        protected virtual DisplayBroadcastStation ParseStationCallLetters(string stationName)
        {
            stationName = _StationEngine.StripStationSuffix(stationName).Trim();
            if (_AvailableStations.Any(x => x.LegacyCallLetters.Equals(stationName)))
            {
                return _AvailableStations.Single(x => x.LegacyCallLetters.Equals(stationName));
            }
            if (_AvailableStations.Any(x => x.CallLetters.Equals(stationName)))
            {
                return _AvailableStations.Single(x => x.CallLetters.Equals(stationName));
            }
            return null;
        }

        protected Dictionary<string, DisplayBroadcastStation> FindStations(List<string> stationNameList)
        {
            var foundStations = new ConcurrentDictionary<string, DisplayBroadcastStation>(StringComparer.OrdinalIgnoreCase);

            var taskList = new List<Task>();

            foreach (var stationName in stationNameList)
            {
                taskList.Add(Task.Run(() =>
                {
                    var station = ParseStationCallLetters(stationName);

                    if (station != null)
                    {
                        foundStations.TryAdd(stationName, station);
                    }
                }));
            }

            //wait for all the tasks to complete
            Task.WaitAll(taskList.ToArray());

            return foundStations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
        }

        private Dictionary<int, double> _GetSpotLengthAndMultipliers()
        {
            // load spot Length ids and multipliers
            var spotMultipliers = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsAndCostMultipliers();

            return (from c in _SpotLengthIdsByDuration.Value
                    join d in spotMultipliers on c.Value equals d.Key
                    select new { c.Key, d.Value }).ToDictionary(x => x.Key, y => y.Value);

        }

        protected List<StationInventoryManifestRate> _GetManifestRatesFromMultipliers(decimal periodRate)
        {
            var manifestRates = new List<StationInventoryManifestRate>();

            foreach (var spotLength in _SpotLengthIdsByDuration.Value)
            {
                var manifestRate = new StationInventoryManifestRate
                {
                    SpotLengthId = spotLength.Value,
                    SpotCost = periodRate * (decimal)_SpotLengthMultipliers[spotLength.Key]
                };
                manifestRates.Add(manifestRate);
            }

            return manifestRates;
        }

        protected List<DisplayDaypart> ParseDayparts(string daypartText, string station)
        {
            if (_InventoryDaypartParsingEngine.TryParse(daypartText, out var displayDayparts) && displayDayparts.Any() && displayDayparts.All(x => x.IsValid))
            {
                return displayDayparts;
            }

            AddProblem($"Invalid daypart '{daypartText}' for station: {station}");
            return new List<DisplayDaypart>();
        }

        protected void AddProblem(string description, string stationLetters = null)
        {
            FileProblems.Add(new InventoryFileProblem()
            {
                ProblemDescription = description,
                StationLetters = stationLetters
            });
        }

        private List<StationInventoryManifestDaypart> _GetDaypartsListForAvailLineWithPeriods(AvailLineWithPeriods availLine)
        {
            var programName = availLine.AvailName;
            var daypart = _GetDisplayDaypartForProgram(availLine);
            var manifestDaypart = new StationInventoryManifestDaypart()
            {
                Daypart = daypart,
                ProgramName = programName
            };

            return new List<StationInventoryManifestDaypart>()
            {
                manifestDaypart
            };
        }

        private Dictionary<string, decimal> _ToDemoValueDict(IEnumerable<DemoValue> periodsDemoValues)
        {
            return periodsDemoValues.ToDictionary(x => x.DemoRef, x => x.Value);
        }

        private Dictionary<string, DisplayAudience> _GetAudienceMap(AAAAMessageProposalAvailListDemoCategory[] demos)
        {
            var result = new Dictionary<string, DisplayAudience>();

            if (demos != null)
            {
                foreach (var demo in demos)
                {
                    result.Add(demo.DemoId, _GetAudienceForDemo(demo));
                }
            }

            return result;
        }

        private DisplayAudience _GetAudienceForDemo(AAAAMessageProposalAvailListDemoCategory demo)
        {
            string audienceCode = $"{demo.Group} {demo.AgeFrom}-{demo.AgeTo}";

            var result = _AudienceCache.GetDisplayAudienceByCode(audienceCode);

            if (result == null)
            {
                throw new Exception($"Unable to find audience for code {audienceCode}");
            }

            return result;
        }

        private DisplayDaypart _GetDisplayDaypartForProgram(AvailLineWithPeriods availLine)
        {
            DisplayDaypart daypart = new DisplayDaypart
            {

                //TODO: Need updated file to be able to handle multiple dayparts?
                Monday = availLine.DayTimes.DayTime.Days.Monday.ToUpper().Equals("Y"),
                Tuesday = availLine.DayTimes.DayTime.Days.Tuesday.ToUpper().Equals("Y"),
                Wednesday = availLine.DayTimes.DayTime.Days.Wednesday.ToUpper().Equals("Y"),
                Thursday = availLine.DayTimes.DayTime.Days.Thursday.ToUpper().Equals("Y"),
                Friday = availLine.DayTimes.DayTime.Days.Friday.ToUpper().Equals("Y"),
                Saturday = availLine.DayTimes.DayTime.Days.Saturday.ToUpper().Equals("Y"),
                Sunday = availLine.DayTimes.DayTime.Days.Sunday.ToUpper().Equals("Y"),

                StartTime = _GetTimeInSecondsFrom24HourTimeString(availLine.DayTimes.DayTime.StartTime),

                // We substract 1 second because the end is not included in the daypart/timespan
                EndTime = _GetTimeInSecondsFrom24HourTimeString(availLine.DayTimes.DayTime.EndTime) - 1
            };

            if (daypart.EndTime < 0)
            {
                daypart.EndTime += AssemblyImportHelper.SecondsPerDay;
            }

            daypart.Id = _DaypartCache.GetIdByDaypart(daypart);

            return daypart;
        }

        private int _GetTimeInSecondsFrom24HourTimeString(string timeString) // "24:30" => 1800
        {
            var hour = Convert.ToInt32(timeString.Substring(0, 2)) % 24;
            var minutes = Convert.ToInt32(timeString.Substring(3, 2));
            return hour * AssemblyImportHelper.SecondsPerHour + minutes * AssemblyImportHelper.SecondsPerMinute; //in seconds
        }

        private AAAAMessage _DeserializeAaaaMessage(Stream stream)
        {
            AAAAMessage message;

            XmlSerializer serializer = new XmlSerializer(typeof(AAAAMessage));
            XmlReader reader = XmlReader.Create(stream);

            message = (AAAAMessage)serializer.Deserialize(reader);

            return message;
        }

        private List<StationContact> _ExtractContactData(AAAAMessage message)
        {
            var stationLetters = message.Proposal.Outlets.Select(s => s.callLetters).ToList();
            var proposalStations = _AvailableStations.Where(x => stationLetters.Contains(x.LegacyCallLetters)).ToList();
            var repTeamNames = _BroadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>().GetRepTeamNames();

            var taskList = new List<Task<StationContact>>();

            var problems = new ConcurrentBag<InventoryFileProblem>();

            foreach (var station in message.Proposal.Outlets)
            {
                taskList.Add(Task.Run(() =>
                {
                    DisplayBroadcastStation proposalStation = null;

                    try
                    {
                        proposalStation = _StationMappingService.GetStationByCallLetters(station.callLetters);
                    }
                    catch(Exception e)
                    {
                        problems.Add(new InventoryFileProblem(e.Message));
                        return null;
                    }

                    var stationContact = new StationContact
                    {
                        Company = message.Proposal.Seller.companyName,
                        Name = message.Proposal.Seller.Salesperson.name,
                        Email = (message.Proposal.Seller.Salesperson != null && message.Proposal.Seller.Salesperson.Email != null) ?
                        message.Proposal.Seller.Salesperson.Email.Where(e => e.type.Equals("primary"))
                            .Select(e => e.Value)
                            .FirstOrDefault() : null,
                        Phone = (message.Proposal.Seller.Salesperson != null && message.Proposal.Seller.Salesperson.Phone != null) ?
                        message.Proposal.Seller.Salesperson.Phone.Where(p => p.type == "voice")
                            .Select(p => p.Value)
                            .FirstOrDefault() : null,
                        Fax = (message.Proposal.Seller.Salesperson != null && message.Proposal.Seller.Salesperson.Phone != null) ?
                        message.Proposal.Seller.Salesperson.Phone.Where(p => p.type == "fax")
                            .Select(p => p.Value)
                            .FirstOrDefault() : null
                    };

                    stationContact.StationId = proposalStation.Id;

                    if (proposalStations.Any(x => x.LegacyCallLetters.Equals(stationContact.Company.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        stationContact.Type = StationContact.StationContactType.Station;
                    }
                    else if (_IsContactARep(repTeamNames, stationContact))
                    {
                        stationContact.Type = StationContact.StationContactType.Rep;
                    }
                    else
                    {
                        stationContact.Type = StationContact.StationContactType.Traffic;
                    }
                    return stationContact;
                }));
            }

            List<StationContact> contacts = Task.WhenAll(taskList).Result.ToList();

            FileProblems.AddRange(problems.ToList());

            return contacts;
        }

        private static bool _IsContactARep(List<string> repTeamNames, StationContact stationContact)
        {
            return repTeamNames.Where(rt => stationContact.Company.ToUpper().Contains(rt.ToUpper())).Count() > 0;
        }

        #region Mappings AvailLineWithDetailedPeriods

        private AvailLineWithPeriods _Map(AAAAMessageProposalAvailListAvailLineWithDetailedPeriods model)
        {
            if (model == null)
            {
                return null;
            }

            return new AvailLineWithPeriods
            {
                AvailName = model.AvailName,
                DaypartName = model.DaypartName,
                SpotLength = model.SpotLength,
                OutletReference = _Map(model.OutletReference),
                Periods = _Map(model.Periods),
                DayTimes = _Map(model.DayTimes)
            };
        }

        private AvailLineWithPeriodsDayTimes _Map(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDayTimes model)
        {
            if (model == null)
            {
                return null;
            }

            return new AvailLineWithPeriodsDayTimes
            {
                DayTime = _Map(model.DayTime)
            };
        }

        private AvailLineWithPeriodsDayTimesDayTime _Map(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDayTimesDayTime model)
        {
            if (model == null)
            {
                return null;
            }

            return new AvailLineWithPeriodsDayTimesDayTime
            {
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Days = _Map(model.Days)
            };
        }

        private AvailLineWithPeriodsDayTimesDayTimeDays _Map(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDayTimesDayTimeDays model)
        {
            if (model == null)
            {
                return null;
            }

            return new AvailLineWithPeriodsDayTimesDayTimeDays
            {
                Monday = model.Monday,
                Tuesday = model.Tuesday,
                Wednesday = model.Wednesday,
                Thursday = model.Thursday,
                Friday = model.Friday,
                Saturday = model.Saturday,
                Sunday = model.Sunday
            };
        }

        private OutletRef _Map(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsOutletReference model)
        {
            if (model == null)
            {
                return null;
            }

            return new OutletRef
            {
                OutletFromListRef = model.outletFromListRef
            };
        }

        private List<Period> _Map(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriods model)
        {
            if (model == null)
            {
                return new List<Period>();
            }

            return model
                .GetAllPeriods()
                .Select(_Map)
                .ToList();
        }

        private Period _Map(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod model)
        {
            if (model == null)
            {
                return null;
            }

            return new Period
            {
                Rate = model.Rate,
                StartDate = model.startDate,
                EndDate = model.endDate,
                DemoValues = model.DemoValues == null ? new List<DemoValue>() : model.DemoValues.Select(_Map).ToList()
            };
        }

        private DemoValue _Map(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue model)
        {
            if (model == null)
            {
                return null;
            }

            return new DemoValue
            {
                DemoRef = model.demoRef,
                Value = model.Value
            };
        }

        #endregion

        #region Mappings AvailLineWithPeriods

        private AvailLineWithPeriods _Map(AAAAMessageProposalAvailListAvailLineWithPeriods model)
        {
            if (model == null)
            {
                return null;
            }

            return new AvailLineWithPeriods
            {
                AvailName = model.AvailName,
                DaypartName = model.DaypartName,
                SpotLength = model.SpotLength,
                OutletReference = _Map(model.OutletReference),
                Periods = model.Periods == null ? new List<Period>() : model.Periods.Select(x => _Map(x, model.Rate, model.DemoValues)).ToList(),
                DayTimes = _Map(model.DayTimes)
            };
        }

        private Period _Map(AAAAMessageProposalAvailListAvailLineWithPeriodsPeriod model, string rate, AAAAMessageProposalAvailListAvailLineWithPeriodsDemoValue[] demoValues)
        {
            if (model == null)
            {
                return null;
            }

            return new Period
            {
                Rate = rate,
                StartDate = model.startDate,
                EndDate = model.endDate,
                DemoValues = demoValues == null ? new List<DemoValue>() : demoValues.Select(_Map).ToList()
            };
        }

        private DemoValue _Map(AAAAMessageProposalAvailListAvailLineWithPeriodsDemoValue model)
        {
            if (model == null)
            {
                return null;
            }

            return new DemoValue
            {
                DemoRef = model.demoRef,
                Value = model.Value
            };
        }

        private OutletRef _Map(AAAAMessageProposalAvailListAvailLineWithPeriodsOutletReference model)
        {
            if (model == null)
            {
                return null;
            }

            return new OutletRef
            {
                OutletFromListRef = model.outletFromListRef
            };
        }

        private AvailLineWithPeriodsDayTimes _Map(AAAAMessageProposalAvailListAvailLineWithPeriodsDayTimes model)
        {
            if (model == null)
            {
                return null;
            }

            return new AvailLineWithPeriodsDayTimes
            {
                DayTime = _Map(model.DayTime)
            };
        }

        private AvailLineWithPeriodsDayTimesDayTime _Map(AAAAMessageProposalAvailListAvailLineWithPeriodsDayTimesDayTime model)
        {
            if (model == null)
            {
                return null;
            }

            return new AvailLineWithPeriodsDayTimesDayTime
            {
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Days = _Map(model.Days)
            };
        }

        private AvailLineWithPeriodsDayTimesDayTimeDays _Map(AAAAMessageProposalAvailListAvailLineWithPeriodsDayTimesDayTimeDays model)
        {
            if (model == null)
            {
                return null;
            }

            return new AvailLineWithPeriodsDayTimesDayTimeDays
            {
                Monday = model.Monday,
                Tuesday = model.Tuesday,
                Wednesday = model.Wednesday,
                Thursday = model.Thursday,
                Friday = model.Friday,
                Saturday = model.Saturday,
                Sunday = model.Sunday
            };
        }

        #endregion

        // This class is used to apply the same logic for AvailLineWithDetailedPeriods and AvailLineWithPeriods
        internal class AvailLineWithPeriods
        {
            public string AvailName { get; set; }

            public string DaypartName { get; set; }

            public DateTime SpotLength { get; set; }

            public OutletRef OutletReference { get; set; }

            public List<Period> Periods { get; set; }

            public AvailLineWithPeriodsDayTimes DayTimes { get; set; }

            internal class OutletRef
            {
                public string OutletFromListRef { get; set; }
            }

            internal class Period
            {
                public string Rate { get; set; }

                public DateTime StartDate { get; set; }

                public DateTime EndDate { get; set; }

                public List<DemoValue> DemoValues { get; set; }

                internal class DemoValue
                {
                    public string DemoRef { get; set; }

                    public decimal Value { get; set; }
                }
            }

            internal class AvailLineWithPeriodsDayTimes
            {
                public AvailLineWithPeriodsDayTimesDayTime DayTime { get; set; }

                internal class AvailLineWithPeriodsDayTimesDayTime
                {
                    public string StartTime { get; set; }

                    public string EndTime { get; set; }

                    public AvailLineWithPeriodsDayTimesDayTimeDays Days { get; set; }

                    internal class AvailLineWithPeriodsDayTimesDayTimeDays
                    {
                        public string Monday { get; set; }

                        public string Tuesday { get; set; }

                        public string Wednesday { get; set; }

                        public string Thursday { get; set; }

                        public string Friday { get; set; }

                        public string Saturday { get; set; }

                        public string Sunday { get; set; }
                    }
                }
            }
        }
    }
}
