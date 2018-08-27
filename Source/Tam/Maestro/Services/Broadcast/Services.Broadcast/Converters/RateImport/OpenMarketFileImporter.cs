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

namespace Services.Broadcast.Converters.RateImport
{
    public class OpenMarketFileImporter : InventoryFileImporterBase
    {
        private const int SecondsPerMinute = 60;

        public override InventorySource InventorySource { get; set; }

        public override void ExtractFileData(Stream rawStream, InventoryFile inventoryFile, DateTime effecitveDate)
        {
            try
            {
                var message = _DeserializeAaaaMessage(rawStream);

                System.Diagnostics.Debug.WriteLine(message.Proposal.uniqueIdentifier + " parsed successfully !");

                var resultFile = BuildRatesFile(message, inventoryFile);
                resultFile.StationContacts = _ExtractContactData(message);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to parse rates file: {0} The file may be invalid: {1}", e.Message, inventoryFile.FileName), e);
            }
        }

        private InventoryFile BuildRatesFile(AAAAMessage message, InventoryFile inventoryFile)
        {

            inventoryFile.UniqueIdentifier = message.Proposal.uniqueIdentifier;
            inventoryFile.StartDate = message.Proposal.startDate;
            inventoryFile.EndDate = message.Proposal.endDate;

            inventoryFile.InventoryManifests.AddRange(_BuildStationProgramList(message.Proposal));

            return inventoryFile;
        }

        private List<StationInventoryManifest> _BuildStationProgramList(AAAAMessageProposal proposal)
        {
            List<StationInventoryManifest> manifests = new List<StationInventoryManifest>();

            var audienceMap = _GetAudienceMap(proposal.AvailList.DemoCategories);

            var invalidStations = new List<string>();
            var allStationNames = proposal.Outlets.Select(o => o.callLetters).Distinct().ToList();
            var validStations = _GetValidStations(allStationNames, invalidStations);

            if (validStations == null || validStations.Count == 0)
            {
                FileProblems.Add(new InventoryFileProblem("There are no known stations in the file"));
                return manifests;
            }

            if (invalidStations.Any())
            {
                FileProblems.AddRange(invalidStations.Select(s => new InventoryFileProblem("Invalid station: " + s)));
                return manifests;
            }

            if (proposal.AvailList == null || (proposal.AvailList.AvailLineWithDetailedPeriods == null && proposal.AvailList.AvailLineWithPeriods == null))
            {
                throw new Exception("Can't find XML elements in order to build station programs from.");
            }

            if (proposal.AvailList.AvailLineWithDetailedPeriods != null)
            {
                BuildProgramsFromAvailLineWithDetailedPeriods(proposal, FileProblems, audienceMap, validStations, manifests);
            }

            if (proposal.AvailList.AvailLineWithPeriods != null)
            {
                BuildProgramsFromAvailLineWithPeriods(proposal, FileProblems, audienceMap, validStations, manifests);
            }

            return manifests;
        }

        //TODO: Move to base class
        private Dictionary<string, DisplayBroadcastStation> _GetValidStations(List<string> stationNameList,List<string> invalidStations)
        {
            var stationsDictionary = new Dictionary<string, DisplayBroadcastStation>();
            foreach (var stationName in stationNameList)
            {
                var station = _ParseStationCallLetters(stationName);
                if (station != null)
                {
                    stationsDictionary.Add(stationName, station);
                }
                else
                {
                    invalidStations.Add(stationName);
                }
            }
            return stationsDictionary;
        }

        //TODO: move to base class
        private DisplayBroadcastStation _ParseStationCallLetters(string stationName)
        {
            // check if it is legacy or the call letters
            var foundStation = _GetDisplayBroadcastStation(stationName);

            if (foundStation == null)
            {
                var station = stationName.Replace("-TV", "").Trim();
                foundStation = _GetDisplayBroadcastStation(station);
            }

            return foundStation;
        }

        //TODO: move to base class
        private DisplayBroadcastStation _GetDisplayBroadcastStation(string stationName)
        {
            var _stationRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            return _stationRepository.GetBroadcastStationByLegacyCallLetters(stationName) ??
                                _stationRepository.GetBroadcastStationByCallLetters(stationName);
        }

        private void BuildProgramsFromAvailLineWithDetailedPeriods(AAAAMessageProposal proposal, List<InventoryFileProblem> fileProblems,
            Dictionary<string, DisplayAudience> audienceMap, Dictionary<string, DisplayBroadcastStation> validStations,
            List<StationInventoryManifest> manifests)
        {
            foreach (var availLine in proposal.AvailList.AvailLineWithDetailedPeriods)
            {
                string programName = availLine.AvailName;

                try
                {
                    var outletRef = proposal.AvailList.OutletReferences
                        .Where(a => a.outletForListId == availLine.OutletReference.outletFromListRef)
                        .Select(a => a.outletFromProposalRef).First();
                    var callLetters = proposal.Outlets.Where(a => a.outletId == outletRef)
                        .Select(a => a.callLetters).First();

                    if (!validStations.ContainsKey(callLetters))
                        throw new Exception(string.Format("Could not find station with call letters \"{0}\"", callLetters));

                    var station = validStations[callLetters];

                    var spotLength = availLine.SpotLength.Minute * SecondsPerMinute +
                                         availLine.SpotLength.Second;

                    var spotLengthProblem = _CheckSpotLength(spotLength, callLetters, programName);
                    if (spotLengthProblem != null)
                    {
                        fileProblems.Add(spotLengthProblem);
                        continue;
                    }

                    var spotLengthId = SpotLengths[spotLength];
                    var periods = availLine.Periods.GetAllPeriods();
                
                    //create a manifest for each period
                    foreach (var availLinePeriod in periods)
                    {
                        var manifestRates = _GetManifestRatesforAvailLineWithDetailedPeriods(spotLengthId, spotLength, availLinePeriod.Rate, programName, callLetters, fileProblems);

                        var manifestAudiences = _GetManifestAudienceListForAvailLine(
                            proposal,
                            audienceMap,
                            _ToDemoValueDict(availLinePeriod.DemoValues));

                        var periodManifest = new StationInventoryManifest()
                        {
                            Station = station,
                            DaypartCode = availLine.DaypartName,
                            SpotsPerWeek = null,
                            SpotLengthId = spotLengthId,
                            ManifestDayparts = _GetDaypartsListForAvailLineWithDetailedPeriods(availLine),
                            ManifestAudiencesReferences = manifestAudiences,
                            ManifestRates = manifestRates,
                            EffectiveDate = availLinePeriod.startDate,
                            EndDate = availLinePeriod.endDate
                        };

                        manifests.Add(periodManifest);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error while processing program " + availLine.AvailName + ": " + e.Message);
                    throw;
                }
            }

        }

        private List<StationInventoryManifestRate> _GetManifestRatesforAvailLineWithPeriods(int spotLengthId,
            AAAAMessageProposalAvailListAvailLineWithPeriods availLine, string stationCallLetters, List<InventoryFileProblem> fileProblems)
        {
            var manifestRates = new List<StationInventoryManifestRate>();
            var spotLength = SpotLengths.Single(x=>x.Value == spotLengthId).Key;

            var availLineRate = string.IsNullOrEmpty(availLine.Rate) ? 0 : decimal.Parse(availLine.Rate);

            if (spotLength == 30) //only create other spot length rates if 30s rate is provided
            {
                manifestRates.AddRange(_GetManifestRatesFromMultipliers(availLineRate));
            }
            else
            {
                var spotLengthProblem = _CheckSpotLength(spotLength, stationCallLetters, availLine.AvailName);
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

        private InventoryFileProblem _CheckSpotLength(int spotLength, string stationLetters, string programName)
        {
            return !SpotLengths.ContainsKey(spotLength) ? new InventoryFileProblem()
            {
                ProblemDescription = $"Unknown spot length found: {spotLength}",
                ProgramName = programName,
                StationLetters = stationLetters
            } : null;
        }

            private List<StationInventoryManifestRate> _GetManifestRatesforAvailLineWithDetailedPeriods(int spotLengthId, int spotLength,
                string linePeriodRate, string programName, string stationCallLetters, List<InventoryFileProblem> fileProblems)
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

            private void BuildProgramsFromAvailLineWithPeriods(AAAAMessageProposal proposal, List<InventoryFileProblem> fileProblems,
                Dictionary<string, DisplayAudience> audienceMap, Dictionary<string, DisplayBroadcastStation> validStations,
                List<StationInventoryManifest> manifests)
            {
                foreach (var availLine in proposal.AvailList.AvailLineWithPeriods)
                {
                    var availLineManifests = new List<StationInventoryManifest>();
                    var programName = availLine.AvailName;

                    try
                    {
                        var outletRef = proposal.AvailList.OutletReferences
                            .Where(a => a.outletForListId == availLine.OutletReference.outletFromListRef)
                            .Select(a => a.outletFromProposalRef).First();
                        var callLetters = proposal.Outlets.Where(a => a.outletId == outletRef)
                            .Select(a => a.callLetters).First();

                        var station = validStations[callLetters];

                        var spotLength = availLine.SpotLength.Minute * SecondsPerMinute +
                                             availLine.SpotLength.Second;

                        var spotLengthProblem = _CheckSpotLength(spotLength, callLetters, programName);
                        if (spotLengthProblem != null)
                        {
                            fileProblems.Add(spotLengthProblem);
                            continue;
                        }

                        var spotLengthId = SpotLengths[spotLength];

                        var manifestAudiences = _GetManifestAudienceListForAvailLine(
                            proposal,
                            audienceMap,
                            _ToDemoValueDict(availLine.DemoValues));

                        if (availLine.Periods != null && availLine.Periods.Count() > 0)
                        {
                            //create a manifest for each period
                            foreach (var availLinePeriod in availLine.Periods)
                            {
                                var manifestRates = _GetManifestRatesforAvailLineWithDetailedPeriods(spotLengthId, spotLength, availLine.Rate, programName, callLetters, fileProblems);

                                var periodManifest = new StationInventoryManifest()
                                {
                                    Station = station,
                                    DaypartCode = availLine.DaypartName,
                                    SpotsPerWeek = null,
                                    SpotLengthId = spotLengthId,
                                    ManifestDayparts = _GetDaypartsListForAvailLineWithPeriods(availLine),
                                    ManifestAudiencesReferences = manifestAudiences,
                                    ManifestRates = manifestRates,
                                    EffectiveDate = availLinePeriod.startDate,
                                    EndDate = availLinePeriod.endDate
                                };
                                manifests.Add(periodManifest);
                            }

                            //var rate = string.IsNullOrEmpty(availLine.Rate) ? 0 : decimal.Parse(availLine.Rate);

                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Error while processing program " + availLine.AvailName + ": " + e.Message);
                        throw;
                    }
                    manifests.AddRange(availLineManifests);
                }
            }

        private List<StationInventoryManifestAudience> _GetManifestAudienceListForAvailLine(AAAAMessageProposal proposal, 
            Dictionary<string, DisplayAudience> audienceMap, Dictionary<string, decimal> demoValues)
        {
                List<StationInventoryManifestAudience> manifestAudiences = new List<StationInventoryManifestAudience>();

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
                        manifestAudience.Impressions = (double) demoValue.Value * 1000;
                        break;
                    case "Rating":
                        manifestAudience.Rating = (double) demoValue.Value;
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

        private List<StationInventoryManifestDaypart> _GetDaypartsListForAvailLineWithPeriods(AAAAMessageProposalAvailListAvailLineWithPeriods availLine)
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

        private List<StationInventoryManifestDaypart> _GetDaypartsListForAvailLineWithDetailedPeriods(AAAAMessageProposalAvailListAvailLineWithDetailedPeriods availLine)
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

        private Dictionary<string, decimal> _ToDemoValueDict(AAAAMessageProposalAvailListAvailLineWithPeriodsDemoValue[] periodsDemoValues)
        {
            var demoValues = new Dictionary<string, decimal>();

            if (periodsDemoValues != null)
            {
                foreach (var demo in periodsDemoValues)
                {
                    demoValues.Add(demo.demoRef, demo.Value);
                }
            }

            return demoValues;
        }

        private Dictionary<string, decimal> _ToDemoValueDict(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue[] detailedPeriodDemoValues)
        {
            var demoValues = new Dictionary<string, decimal>();

            if (detailedPeriodDemoValues != null)
            {
                foreach (var demo in detailedPeriodDemoValues)
                {
                    demoValues.Add(demo.demoRef, demo.Value);
                }
            }

            return demoValues;
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
                throw new Exception(
                    String.Format("Unable to find audience for subcategory {0} and age {1}-{2}", audienceSubcategory, demo.AgeFrom, demo.AgeTo));
            }

            return result;
        }

        private DisplayDaypart _GetDisplayDaypartForProgram(AAAAMessageProposalAvailListAvailLineWithDetailedPeriods availLine)
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

            //We substract 1 second because the end is not included in the daypart/timespan
            daypart.EndTime = _GetTimeInSecondsFrom24HourTimeString(availLine.DayTimes.DayTime.EndTime) - 1;
            if (daypart.EndTime < 0)
            {
                daypart.EndTime += AssemblyImportHelper.SecondsPerDay;
            }

            daypart.Id = _DaypartCache.GetIdByDaypart(daypart);

            return daypart;
        }

        private DisplayDaypart _GetDisplayDaypartForProgram(AAAAMessageProposalAvailListAvailLineWithPeriods availLine)
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

            //We substract 1 second because the end is not included in the daypart/timespan
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
            var hour = System.Convert.ToInt32(timeString.Substring(0, 2)) % 24;
            var minutes = System.Convert.ToInt32(timeString.Substring(3, 2));
            return hour *AssemblyImportHelper.SecondsPerHour + minutes *AssemblyImportHelper.SecondsPerMinute; //in seconds
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

                if (
                    proposalStations.Select(s => s.Value.LegacyCallLetters.Trim().ToUpper())
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
    }
}
