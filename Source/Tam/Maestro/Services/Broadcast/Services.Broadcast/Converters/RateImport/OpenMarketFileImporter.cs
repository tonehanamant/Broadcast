using Services.Broadcast.Entities;
using Services.Broadcast.Entities.RatesFileXml;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public class OpenMarketFileImporter : RateFileImporterBase
    {
        private const int SecondsPerMinute = 60;

        public override RatesFile.RateSourceType RateSource
        {
            get { return RatesFile.RateSourceType.OpenMarket; }
        }


        public override void ExtractFileData(Stream rawStream, RatesFile ratesFile, List<RatesFileProblem> fileProblems)
        {

            try
            {
                var message = DeserializeAaaaMessage(rawStream);

                System.Diagnostics.Debug.WriteLine(message.Proposal.uniqueIdentifier + " parsed successfully !");

                var resultFile = BuildRatesFile(message, ratesFile, fileProblems);
                resultFile.StationContacts = ExtractContactData(message, ratesFile.RateSource);

            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to parse rates file: {0} The file may be invalid: {1}", e.Message, ratesFile.FileName), e);
            }
        }

        private RatesFile BuildRatesFile(AAAAMessage message, RatesFile ratesFile, List<RatesFileProblem> fileProblems)
        {

            ratesFile.UniqueIdentifier = message.Proposal.uniqueIdentifier;
            ratesFile.StartDate = message.Proposal.startDate;
            ratesFile.EndDate = message.Proposal.endDate;

            ratesFile.StationPrograms.AddRange(BuildStationProgramList(message.Proposal, fileProblems));

            return ratesFile;
        }

        private List<StationProgram> BuildStationProgramList(AAAAMessageProposal proposal, List<RatesFileProblem> fileProblems)
        {
            List<StationProgram> programs = new List<StationProgram>();

            var audienceMap = GetAudienceMap(proposal.AvailList.DemoCategories);

            if (proposal.AvailList == null || (proposal.AvailList.AvailLineWithDetailedPeriods == null && proposal.AvailList.AvailLineWithPeriods == null))
            {
                throw new Exception("Can't find XML elements in order to build station programs from.");
            }

            if (proposal.AvailList.AvailLineWithDetailedPeriods != null)
            {
                BuildProgramsFromAvailLineWithDetailedPeriods(proposal, fileProblems, audienceMap, programs);
            }

            if (proposal.AvailList.AvailLineWithPeriods != null)
            {
                BuildProgramsFromAvailLineWithPeriods(proposal, fileProblems, audienceMap, programs);
            }

            return programs;
        }

        private void BuildProgramsFromAvailLineWithDetailedPeriods(AAAAMessageProposal proposal, List<RatesFileProblem> fileProblems,
            Dictionary<string, DisplayAudience> audienceMap, List<StationProgram> programs)
        {

            foreach (var availLine in proposal.AvailList.AvailLineWithDetailedPeriods)
            {
                StationProgram program = new StationProgram();
                program.ProgramName = availLine.AvailName;

                try
                {
                    var outletRef = proposal.AvailList.OutletReferences
                        .Where(a => a.outletForListId == availLine.OutletReference.outletFromListRef)
                        .Select(a => a.outletFromProposalRef).First();
                    var callLetters = proposal.Outlets.Where(a => a.outletId == outletRef)
                        .Select(a => a.callLetters).First();

                    program.StationLegacyCallLetters = callLetters;
                    program.Daypart = GetDisplayDaypartForProgram(availLine);
                    program.DayPartName = availLine.DaypartName;
                    program.SpotLength = availLine.SpotLength.Minute * SecondsPerMinute + availLine.SpotLength.Second;
                    if (availLine.Periods != null && availLine.Periods.Count() > 0)
                    {
                        program.StartDate = availLine.Periods.OrderBy(p => p.startDate).First().startDate;
                        program.EndDate = availLine.Periods.OrderBy(p => p.endDate).Last().endDate;
                        program.FlightWeeks = BuildFlightWeeksFromDetailedPeriods(
                            availLine.Periods,
                            program.SpotLength,
                            proposal,
                            audienceMap,
                            program,
                            fileProblems);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error while processing program " + program.ProgramName + ": " + e.Message);
                    throw;
                }
                programs.Add(program);
            }
        }

        private void BuildProgramsFromAvailLineWithPeriods(AAAAMessageProposal proposal, List<RatesFileProblem> fileProblems,
            Dictionary<string, DisplayAudience> audienceMap, List<StationProgram> programs)
        {

            foreach (var availLine in proposal.AvailList.AvailLineWithPeriods)
            {
                StationProgram program = new StationProgram();
                program.ProgramName = availLine.AvailName;

                try
                {
                    var outletRef = proposal.AvailList.OutletReferences
                        .Where(a => a.outletForListId == availLine.OutletReference.outletFromListRef)
                        .Select(a => a.outletFromProposalRef).First();
                    var callLetters = proposal.Outlets.Where(a => a.outletId == outletRef)
                        .Select(a => a.callLetters).First();

                    program.StationLegacyCallLetters = callLetters;
                    program.Daypart = GetDisplayDaypartForProgram(availLine);
                    program.DayPartName = availLine.DaypartName;
                    if (availLine.Periods != null && availLine.Periods.Count() > 0)
                    {
                        program.StartDate = availLine.Periods.OrderBy(p => p.startDate).First().startDate;
                        program.EndDate = availLine.Periods.OrderBy(p => p.endDate).Last().endDate;
                        program.SpotLength = availLine.SpotLength.Minute * SecondsPerMinute +
                                             availLine.SpotLength.Second;
                        var rate = string.IsNullOrEmpty(availLine.Rate) ? (decimal)0 : decimal.Parse(availLine.Rate);
                        program.FlightWeeks = BuildFlightWeeksFromPeriods(
                            availLine.Periods,
                            rate,
                            ToDemoValueDict(availLine.DemoValues),
                            program.SpotLength,
                            proposal,
                            audienceMap,
                            program,
                            fileProblems);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error while processing program " + program.ProgramName + ": " + e.Message);
                    throw;
                }
                programs.Add(program);
            }
        }

        private Dictionary<string, decimal> ToDemoValueDict(AAAAMessageProposalAvailListAvailLineWithPeriodsDemoValue[] periodsDemoValues)
        {
            var demoValues = new Dictionary<string, decimal>();
            foreach (var demo in periodsDemoValues)
            {
                demoValues.Add(demo.demoRef, demo.Value);
            }
            return demoValues;
        }

        private Dictionary<string, decimal> ToDemoValueDict(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDetailedPeriodDemoValue[] detailedPeriodDemoValues)
        {
            var demoValues = new Dictionary<string, decimal>();
            foreach (var demo in detailedPeriodDemoValues)
            {
                demoValues.Add(demo.demoRef, demo.Value);
            }
            return demoValues;
        }

        private Dictionary<string, DisplayAudience> GetAudienceMap(AAAAMessageProposalAvailListDemoCategory[] demos)
        {
            var result = new Dictionary<string, DisplayAudience>();

            if (demos == null)
            {
                throw new Exception("No audiences defined in file.");
            }

            foreach (var demo in demos)
            {
                result.Add(demo.DemoId, GetAudienceForDemo(demo));
            }

            return result;
        }

        private List<StationProgramFlightWeek> BuildFlightWeeksFromDetailedPeriods(AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDetailedPeriod[] detailedPeriods,
            int spotLength, AAAAMessageProposal proposal, Dictionary<string, DisplayAudience> audienceMap, StationProgram program, List<RatesFileProblem> fileProblems)
        {
            List<StationProgramFlightWeek> flightWeeks = new List<StationProgramFlightWeek>();
            foreach (var detailPeriod in detailedPeriods)
            {
                var rate = string.IsNullOrEmpty(detailPeriod.Rate) ? (decimal)0 : decimal.Parse(detailPeriod.Rate);
                var periodFlightWeeks = BuildFlightWeeks(detailPeriod.startDate, detailPeriod.endDate, rate,
                                                        ToDemoValueDict(detailPeriod.DemoValues), spotLength, proposal, audienceMap,
                                                        program, fileProblems);
                if (periodFlightWeeks != null && periodFlightWeeks.Count() > 0)
                {
                    flightWeeks.AddRange(periodFlightWeeks);
                }
            }
            return flightWeeks;

        }

        private List<StationProgramFlightWeek> BuildFlightWeeksFromPeriods(AAAAMessageProposalAvailListAvailLineWithPeriodsPeriod[] periods, decimal programRate, Dictionary<string, decimal> demoValues,
    int spotLength, AAAAMessageProposal proposal, Dictionary<string, DisplayAudience> audienceMap, StationProgram program, List<RatesFileProblem> fileProblems)
        {
            List<StationProgramFlightWeek> flightWeeks = new List<StationProgramFlightWeek>();
            foreach (var period in periods)
            {
                var periodFlightWeeks = BuildFlightWeeks(period.startDate, period.endDate, programRate, demoValues, spotLength, proposal,
                                                        audienceMap, program, fileProblems);
                if (periodFlightWeeks != null && periodFlightWeeks.Count() > 0)
                {
                    flightWeeks.AddRange(periodFlightWeeks);
                }
            }
            return flightWeeks;

        }

        private List<StationProgramFlightWeek> BuildFlightWeeks(DateTime periodStart, DateTime PeriodEnd, decimal periodRate, Dictionary<string, decimal> demoValues,
            int spotLength, AAAAMessageProposal proposal, Dictionary<string, DisplayAudience> audienceMap, StationProgram program, List<RatesFileProblem> fileProblems)
        {
            List<StationProgramFlightWeek> flightWeeks = new List<StationProgramFlightWeek>();
            var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(periodStart, PeriodEnd);

            foreach (var mediaWeek in mediaWeeks)
            {
                var flightWeek = new StationProgramFlightWeek();
                flightWeek.Active = true;
                flightWeek.FlightWeek = mediaWeek;

                // fill in all spots when spotlength is 30
                if (spotLength == 30)
                {
                    _ApplySpotLengthRateMultipliers(flightWeek, periodRate);
                }
                else
                {
                    switch (spotLength)
                    {
                        case 15:
                            flightWeek.Rate15s = periodRate;
                            break;
                        case 60:
                            flightWeek.Rate60s = periodRate;
                            break;
                        case 90:
                            flightWeek.Rate90s = periodRate;
                            break;
                        case 120:
                            flightWeek.Rate120s = periodRate;
                            break;
                        default:
                            fileProblems.Add(
                                new RatesFileProblem()
                                {
                                    ProblemDescription =
                                        string.Format(
                                            "Unknown spot length found: {0}",
                                            spotLength),
                                    ProgramName = program.ProgramName,
                                    StationLetters = program.StationLegacyCallLetters
                                });
                            return new List<StationProgramFlightWeek>(); //returning empty list of flight weeks if any of the weeks has unknown spot length;
                    }
                }


                flightWeek.Audiences = BuildFlightWeekAudiences(demoValues, proposal, audienceMap);
                flightWeeks.Add(flightWeek);
            }

            return flightWeeks;
        }

        private List<StationProgramFlightWeekAudience> BuildFlightWeekAudiences(Dictionary<string, decimal> demoValues,
            AAAAMessageProposal proposal, Dictionary<string, DisplayAudience> audienceMap)
        {
            List<StationProgramFlightWeekAudience> audiences = new List<StationProgramFlightWeekAudience>();

            foreach (var demoValue in demoValues)
            {

                var demo = proposal.AvailList.DemoCategories.Where(a => a.DemoId == demoValue.Key).First();
                var audience = audienceMap[demo.DemoId];

                var flightWeekAudience = audiences.Where(a => a.Audience.Id == audience.Id).FirstOrDefault();

                var newFlightWeekAudience = false;
                if (flightWeekAudience == null)
                {
                    newFlightWeekAudience = true;
                    flightWeekAudience = new StationProgramFlightWeekAudience();
                    flightWeekAudience.Audience = audience;
                }

                switch (demo.DemoType)
                {
                    case "Impression":
                        flightWeekAudience.Impressions = (float)demoValue.Value;
                        break;
                    case "Rating":
                        flightWeekAudience.Rating = (float)demoValue.Value;
                        break;
                    default:
                        throw new Exception("Unknown DemoType [" + demo.DemoType + "] in rate file.");
                }

                if (newFlightWeekAudience)
                {
                    audiences.Add(flightWeekAudience);
                }
            }

            return audiences;
        }

        private DisplayAudience GetAudienceForDemo(AAAAMessageProposalAvailListDemoCategory demo)
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

        private DisplayDaypart GetDisplayDaypartForProgram(AAAAMessageProposalAvailListAvailLineWithDetailedPeriods availLine)
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

            daypart.StartTime = GetTimeInSecondsFrom24HourTimeString(availLine.DayTimes.DayTime.StartTime);

            //We substract 1 second because the end is not included in the daypart/timespan
            daypart.EndTime = GetTimeInSecondsFrom24HourTimeString(availLine.DayTimes.DayTime.EndTime) - 1;
            if (daypart.EndTime < 0)
            {
                daypart.EndTime += AssemblyImportHelper.SecondsPerDay;
            }

            daypart.Id = _DaypartCache.GetIdByDaypart(daypart);

            return daypart;
        }

        private DisplayDaypart GetDisplayDaypartForProgram(AAAAMessageProposalAvailListAvailLineWithPeriods availLine)
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

            daypart.StartTime = GetTimeInSecondsFrom24HourTimeString(availLine.DayTimes.DayTime.StartTime);

            //We substract 1 second because the end is not included in the daypart/timespan
            daypart.EndTime = GetTimeInSecondsFrom24HourTimeString(availLine.DayTimes.DayTime.EndTime) - 1;
            if (daypart.EndTime < 0)
            {
                daypart.EndTime += AssemblyImportHelper.SecondsPerDay;
            }

            daypart.Id = _DaypartCache.GetIdByDaypart(daypart);

            return daypart;
        }

        private int GetTimeInSecondsFrom24HourTimeString(string timeString) // "24:30" => 1800
        {
            var hour = System.Convert.ToInt32(timeString.Substring(0, 2)) % 24;
            var minutes = System.Convert.ToInt32(timeString.Substring(3, 2));
            return hour *AssemblyImportHelper.SecondsPerHour + minutes *AssemblyImportHelper.SecondsPerMinute; //in seconds
        }

        private AAAAMessage DeserializeAaaaMessage(Stream stream)
        {
            AAAAMessage message;

            XmlSerializer serializer = new XmlSerializer(typeof(AAAAMessage));
            XmlReader reader = XmlReader.Create(stream);

            message = (AAAAMessage)serializer.Deserialize(reader);

            return message;
        }

        private List<StationContact> ExtractContactData(AAAAMessage message, RatesFile.RateSourceType rateSourceType)
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
                else if (IsContactARep(repTeamNames, stationContact))
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

        private static bool IsContactARep(List<string> repTeamNames, StationContact stationContact)
        {
            return repTeamNames.Where(rt => stationContact.Company.ToUpper().Contains(rt.ToUpper())).Count() > 0;
        }
    }
}
