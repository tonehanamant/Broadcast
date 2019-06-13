using System.Linq;
using System;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Services.Broadcast.Repositories;
using Tam.Maestro.Services.ContractInterfaces.Common;
using System.Xml.Serialization;
using System.Xml;
using Services.Broadcast.Extensions;
using static Services.Broadcast.Converters.RateImport.OpenMarketFileImporter.AvailLineWithPeriods;
using static Services.Broadcast.Converters.RateImport.OpenMarketFileImporter.AvailLineWithPeriods.AvailLineWithPeriodsDayTimes;
using static Services.Broadcast.Converters.RateImport.OpenMarketFileImporter.AvailLineWithPeriods.Period;
using static Services.Broadcast.Converters.RateImport.OpenMarketFileImporter.AvailLineWithPeriods.AvailLineWithPeriodsDayTimes.AvailLineWithPeriodsDayTimesDayTime;
using Services.Broadcast.Entities.StationInventory;
using System.Diagnostics;
using Tam.Maestro.Common;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using Common.Services.Repositories;
using Common.Services;
using Services.Broadcast.Exceptions;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IOpenMarketFileImporter : IApplicationService
    {
        void ExtractFileData(Stream rawStream, InventoryFile inventoryFile);
        void CheckFileHash();
        void LoadFromSaveRequest(InventoryFileSaveRequest request);
        InventoryFile GetPendingInventoryFile(InventorySource InventorySource, string userName, DateTime nowDate);

        //Access to FileProblems list
        List<InventoryFileProblem> FileProblems { get; set; }
    }

    public class OpenMarketFileImporter : IOpenMarketFileImporter
    {
        private const int SecondsPerMinute = 60;

        public List<InventoryFileProblem> FileProblems { get; set; } = new List<InventoryFileProblem>();
        private InventoryFileSaveRequest _Request { get; set; }
        private string _FileHash { get; set; }
        private readonly string _validTimeExpression =
            @"(^([0-9]|[0-1][0-9]|[2][0-3])(:)?([0-5][0-9])(\s{0,1})(A|AM|P|PM|a|am|p|pm|aM|Am|pM|Pm{2,2})$)|(^([0-9]|[1][0-9]|[2][0-3])(\s{0,1})(A|AM|P|PM|a|am|p|pm|aM|Am|pM|Pm{2,2})$)";

        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IDaypartCache _DaypartCache;
        private readonly IInventoryDaypartParsingEngine _InventoryDaypartParsingEngine;

        public OpenMarketFileImporter(IDataRepositoryFactory dataRepositoryFactory
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IDaypartCache daypartCache
            , IInventoryDaypartParsingEngine inventoryDaypartParsingEngine)
        {
            _BroadcastDataRepositoryFactory = dataRepositoryFactory;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _DaypartCache = daypartCache;
            _InventoryDaypartParsingEngine = inventoryDaypartParsingEngine;
        }

        /// <summary>
        /// Spot lengths dictionary where key is the length and value is the id
        /// </summary>
        private Dictionary<int, int> SpotLengths
        {
            get
            {
                if (_SpotLengths == null)
                {
                    _SpotLengths = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
                }
                return _SpotLengths;
            }
        }
        private Dictionary<int, int> _SpotLengths { get; set; }

        protected Dictionary<int, double> _SpotLengthMultipliers;

        public void ExtractFileData(Stream rawStream, InventoryFile inventoryFile)
        {
            try
            {
                var message = _DeserializeAaaaMessage(rawStream);

                Debug.WriteLine(message.Proposal.uniqueIdentifier + " parsed successfully !");

                BuildRatesFile(message, inventoryFile);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to parse rates file: {e.Message} The file may be invalid: {inventoryFile.FileName}", e);
            }
        }

        private void BuildRatesFile(AAAAMessage message, InventoryFile inventoryFile)
        {
            var proposal = message.Proposal;

            inventoryFile.UniqueIdentifier = proposal.uniqueIdentifier;
            inventoryFile.StartDate = proposal.startDate;
            inventoryFile.EndDate = proposal.endDate;

            _PopulateStationProgramList(message.Proposal, inventoryFile);
            if (inventoryFile.ValidationProblems.Any()) return;

            inventoryFile.StationContacts = _ExtractContactData(message);
        }

        private void _PopulateStationProgramList(AAAAMessageProposal proposal, InventoryFile inventoryFile)
        {
            if (!IsValid(proposal.AvailList))
            {
                inventoryFile.ValidationProblems.Add("Can't find XML elements in order to build station programs from.");
                return;
            }

            foreach (var availList in proposal.AvailList)
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

                _PopulateProgramsFromAvailLineWithPeriods(proposal, availList, availLines, inventoryFile, FileProblems);
            }
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
            return !SpotLengths.ContainsKey(spotLength) ? new InventoryFileProblem()
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
            List<InventoryFileProblem> fileProblems)
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

        private void _PopulateProgramsFromAvailLineWithPeriods(
            AAAAMessageProposal proposal,
            AAAAMessageProposalAvailList availList,
            List<AvailLineWithPeriods> availLines,
            InventoryFile inventoryFile,
            List<InventoryFileProblem> fileProblems)
        {
            var audienceMap = _GetAudienceMap(availList.DemoCategories);
            var allStationNames = proposal.Outlets.Select(o => o.callLetters).Distinct().ToList();
            var foundStations = FindStations(allStationNames);

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

                    var station = foundStations.ContainsKey(callLetters) ? foundStations[callLetters] : null;
                    var spotLength = availLine.SpotLength.Minute * SecondsPerMinute + availLine.SpotLength.Second;
                    var spotLengthProblem = _CheckSpotLength(spotLength, callLetters, programName);

                    if (spotLengthProblem != null)
                    {
                        fileProblems.Add(spotLengthProblem);
                        continue;
                    }

                    var spotLengthId = SpotLengths[spotLength];

                    foreach (var availLinePeriod in availLine.Periods)
                    {
                        var manifestAudiences = _GetManifestAudienceListForAvailLine(availList, audienceMap, _ToDemoValueDict(availLinePeriod.DemoValues));
                        var manifestRates = _GetManifestRatesforAvailLineWithDetailedPeriods(spotLengthId, spotLength, availLinePeriod.Rate, programName, callLetters, fileProblems);
                        var manifestWeeks = _GetManifestWeeksForAvailLine(availLinePeriod.StartDate, availLinePeriod.EndDate);

                        if (station == null)
                        {
                            inventoryFile.InventoryManifests.Add(new StationInventoryManifest
                            {
                                Station = new DisplayBroadcastStation { CallLetters = callLetters },
                                DaypartCode = availLine.DaypartName,
                                SpotsPerWeek = null,
                                SpotLengthId = spotLengthId,
                                ManifestDayparts = _GetDaypartsListForAvailLineWithPeriods(availLine),
                                ManifestAudiencesReferences = manifestAudiences,
                                ManifestRates = manifestRates,
                                ManifestWeeks = manifestWeeks
                            });
                        }
                        else
                        {
                            inventoryFile.InventoryManifests.Add(new StationInventoryManifest
                            {
                                Station = station,
                                DaypartCode = availLine.DaypartName,
                                SpotsPerWeek = null,
                                SpotLengthId = spotLengthId,
                                ManifestDayparts = _GetDaypartsListForAvailLineWithPeriods(availLine),
                                ManifestAudiencesReferences = manifestAudiences,
                                ManifestRates = manifestRates,
                                ManifestWeeks = manifestWeeks
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    fileProblems.Add(new InventoryFileProblem($"Error while processing {availLine.AvailName} on {availList.Name}: {e.Message}"));
                }
            }
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
                InventorySource =  InventorySource,
                CreatedBy = userName,
                CreatedDate = nowDate
            };
        }

        private DisplayBroadcastStation _GetDisplayBroadcastStation(string stationName)
        {
            var _stationRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            return _stationRepository.GetBroadcastStationByLegacyCallLetters(stationName) ??
                   _stationRepository.GetBroadcastStationByCallLetters(stationName);
        }

        protected virtual DisplayBroadcastStation ParseStationCallLetters(string stationName)
        {
            stationName = stationName.Replace("-TV", "").Trim();
            return _GetDisplayBroadcastStation(stationName);
        }

        protected Dictionary<string, DisplayBroadcastStation> FindStations(List<string> stationNameList)
        {
            var foundStations = new Dictionary<string, DisplayBroadcastStation>(StringComparer.OrdinalIgnoreCase);

            foreach (var stationName in stationNameList)
            {
                var station = ParseStationCallLetters(stationName);

                if (station != null)
                {
                    foundStations.Add(stationName, station);
                }
            }

            return foundStations;
        }
        
        private Dictionary<int, double> _GetSpotLengthAndMultipliers()
        {
            // load spot lenght ids and multipliers
            var spotMultipliers = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsAndCostMultipliers();

            return (from c in SpotLengths
                    join d in spotMultipliers on c.Value equals d.Key
                    select new { c.Key, d.Value }).ToDictionary(x => x.Key, y => y.Value);

        }

        protected List<StationInventoryManifestRate> _GetManifestRatesFromMultipliers(decimal periodRate)
        {
            var manifestRates = new List<StationInventoryManifestRate>();

            foreach (var spotLength in SpotLengths)
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
            string audienceSubcategory = null;

            switch (demo.Group.ToString())
            {
                case "Homes":
                    audienceSubcategory = "H";
                    break;
                case "Adults":
                    audienceSubcategory = "A";
                    break;
                case "Women":
                    audienceSubcategory = "W";
                    break;
                case "Females":
                    audienceSubcategory = "F";
                    break;
                case "Men":
                    audienceSubcategory = "M";
                    break;
                case "Males":
                    audienceSubcategory = "M";
                    break;
                default:
                    throw new Exception("Unknown demo group [" + demo.Group + "] in rate file.");
            }

            var result = _BroadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>().GetDisplayAudienceByAgeAndSubcategory(
                audienceSubcategory,
                demo.AgeFrom,
                demo.AgeTo);

            if (result == null)
            {
                throw new Exception($"Unable to find audience for subcategory {audienceSubcategory} and age {demo.AgeFrom}-{demo.AgeTo}");
            }

            return result;
        }

        private DisplayDaypart _GetDisplayDaypartForProgram(AvailLineWithPeriods availLine)
        {
            DisplayDaypart daypart = new DisplayDaypart();

            //TODO: Need updated file to be able to handle multiple dayparts?
            daypart.Monday = availLine.DayTimes.DayTime.Days.Monday.ToUpper().Equals("Y");
            daypart.Tuesday = availLine.DayTimes.DayTime.Days.Tuesday.ToUpper().Equals("Y");
            daypart.Wednesday = availLine.DayTimes.DayTime.Days.Wednesday.ToUpper().Equals("Y");
            daypart.Thursday = availLine.DayTimes.DayTime.Days.Thursday.ToUpper().Equals("Y");
            daypart.Friday = availLine.DayTimes.DayTime.Days.Friday.ToUpper().Equals("Y");
            daypart.Saturday = availLine.DayTimes.DayTime.Days.Saturday.ToUpper().Equals("Y");
            daypart.Sunday = availLine.DayTimes.DayTime.Days.Sunday.ToUpper().Equals("Y");

            daypart.StartTime = _GetTimeInSecondsFrom24HourTimeString(availLine.DayTimes.DayTime.StartTime);

            // We substract 1 second because the end is not included in the daypart/timespan
            daypart.EndTime = _GetTimeInSecondsFrom24HourTimeString(availLine.DayTimes.DayTime.EndTime) - 1;

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
            var contacts = new List<StationContact>();
            var stationLetters = message.Proposal.Outlets.Select(s => s.callLetters).ToList();
            var proposalStations = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>().GetBroadcastStationListByLegacyCallLetters(stationLetters);
            var repTeamNames = _BroadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>().GetRepTeamNames();

            foreach (var station in message.Proposal.Outlets)
            {
                var proposalStation = proposalStations.SingleOrDefault(ps => ps.LegacyCallLetters == station.callLetters);
                var stationContact = new StationContact();
                stationContact.Company = message.Proposal.Seller.companyName;
                stationContact.Name = message.Proposal.Seller.Salesperson.name;
                stationContact.Email = (message.Proposal.Seller.Salesperson != null && message.Proposal.Seller.Salesperson.Email != null) ?
                    message.Proposal.Seller.Salesperson.Email.Where(e => e.type.Equals("primary"))
                        .Select(e => e.Value)
                        .FirstOrDefault() : null;
                stationContact.Phone = (message.Proposal.Seller.Salesperson != null && message.Proposal.Seller.Salesperson.Phone != null) ?
                    message.Proposal.Seller.Salesperson.Phone.Where(p => p.type == "voice")
                        .Select(p => p.Value)
                        .FirstOrDefault() : null;
                stationContact.Fax = (message.Proposal.Seller.Salesperson != null && message.Proposal.Seller.Salesperson.Phone != null) ?
                    message.Proposal.Seller.Salesperson.Phone.Where(p => p.type == "fax")
                        .Select(p => p.Value)
                        .FirstOrDefault() : null;

                if (proposalStation == null)
                {
                    // station should be created next
                    stationContact.StationCallLetters = station.callLetters;
                    stationContact.Type = StationContact.StationContactType.Station;
                }
                else
                {
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
                }

                contacts.Add(stationContact);
            }

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
