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

namespace Services.Broadcast.Converters.RateImport
{
    public class OpenMarketFileImporter : InventoryFileImporterBase
    {
        private const int SecondsPerMinute = 60;

        public override void ExtractFileData(Stream rawStream, InventoryFile inventoryFile, DateTime effecitveDate)
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

            inventoryFile.StationContacts = _ExtractContactData(message);
        }

        private void _PopulateStationProgramList(AAAAMessageProposal proposal, InventoryFile inventoryFile)
        {
            if (!IsValid(proposal.AvailList))
            {
                throw new Exception("Can't find XML elements in order to build station programs from.");
            }

            var availLines = new List<AvailLineWithPeriods>();

            if (proposal.AvailList.AvailLineWithDetailedPeriods != null)
            {
                availLines.AddRange(proposal.AvailList.AvailLineWithDetailedPeriods.Select(_Map));
            }

            if (proposal.AvailList.AvailLineWithPeriods != null)
            {
                availLines.AddRange(proposal.AvailList.AvailLineWithPeriods.Select(_Map));
            }

            _PopulateProgramsFromAvailLineWithPeriods(proposal, availLines, inventoryFile, FileProblems);
        }

        private bool IsValid(AAAAMessageProposalAvailList availList)
        {
            return availList != null && (availList.AvailLineWithDetailedPeriods != null || availList.AvailLineWithPeriods != null);
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
                        Rate = availLineRate
                    };
                    manifestRates.Add(manifestRate);
                }
            }

            return manifestRates;
        }

        private void _PopulateProgramsFromAvailLineWithPeriods(
            AAAAMessageProposal proposal,
            List<AvailLineWithPeriods> availLines,
            InventoryFile inventoryFile,
            List<InventoryFileProblem> fileProblems)
        {
            var audienceMap = _GetAudienceMap(proposal.AvailList.DemoCategories);
            var allStationNames = proposal.Outlets.Select(o => o.callLetters).Distinct().ToList();
            var foundStations = FindStations(allStationNames);

            foreach (var availLine in availLines)
            {
                var programName = availLine.AvailName;

                try
                {
                    var outletRef = proposal.AvailList.OutletReferences
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
                        var manifestAudiences = _GetManifestAudienceListForAvailLine(proposal, audienceMap, _ToDemoValueDict(availLinePeriod.DemoValues));
                        var manifestRates = _GetManifestRatesforAvailLineWithDetailedPeriods(spotLengthId, spotLength, availLinePeriod.Rate, programName, callLetters, fileProblems);

                        if (station == null)
                        {
                            inventoryFile.InventoryManifestsStaging.Add(new StationInventoryManifestStaging
                            {
                                Station = callLetters,
                                DaypartCode = availLine.DaypartName,
                                SpotsPerWeek = null,
                                SpotLengthId = spotLengthId,
                                ManifestDayparts = _GetDaypartsListForAvailLineWithPeriods(availLine),
                                ManifestAudiencesReferences = manifestAudiences,
                                ManifestRates = manifestRates,
                                EffectiveDate = availLinePeriod.StartDate,
                                EndDate = availLinePeriod.EndDate
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
                                EffectiveDate = availLinePeriod.StartDate,
                                EndDate = availLinePeriod.EndDate
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error while processing program " + availLine.AvailName + ": " + e.Message);
                    throw;
                }
            }
        }

        private List<StationInventoryManifestAudience> _GetManifestAudienceListForAvailLine(
            AAAAMessageProposal proposal,
            Dictionary<string, DisplayAudience> audienceMap,
            Dictionary<string, decimal> demoValues)
        {
            var manifestAudiences = new List<StationInventoryManifestAudience>();

            foreach (var demoValue in demoValues)
            {
                var demo = proposal.AvailList.DemoCategories.Where(a => a.DemoId == demoValue.Key).First();
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
            var proposalStations = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>().GetBroadcastStationListByLegacyCallLetters(stationLetters).ToDictionary(s => s.LegacyCallLetters);
            var repTeamNames = _BroadcastDataRepositoryFactory.GetDataRepository<IStationContactsRepository>().GetRepTeamNames();

            foreach (var station in message.Proposal.Outlets)
            {
                var proposalStation = proposalStations.Where(ps => ps.Key == station.callLetters).SingleOrDefault();
                if (proposalStation.Value == null)
                {
                    continue; //Skip contacts for unknown stations
                }

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
                stationContact.StationCode = proposalStation.Value.Code;

                if (proposalStations.Select(s => s.Value.LegacyCallLetters.Trim().ToUpper())
                        .ToList()
                        .Contains(stationContact.Company.Trim().ToUpper()))
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

            var result = new AvailLineWithPeriods
            {
                AvailName = model.AvailName,
                DaypartName = model.DaypartName,
                SpotLength = model.SpotLength,
                OutletReference = _Map(model.OutletReference),
                Periods = _Map(model.Periods),
                DayTimes = _Map(model.DayTimes)
            };

            return result;
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

            var result = new AvailLineWithPeriods
            {
                AvailName = model.AvailName,
                DaypartName = model.DaypartName,
                SpotLength = model.SpotLength,
                OutletReference = _Map(model.OutletReference),
                Periods = model.Periods == null ? new List<Period>() : model.Periods.Select(x => _Map(x, model.Rate, model.DemoValues)).ToList(),
                DayTimes = _Map(model.DayTimes)
            };

            return result;
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
