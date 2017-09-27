using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
namespace Services.Broadcast.Converters.RateImport
{
    public class CNNFileImporter : RateFileImporterBase
    {
        private static readonly List<string> CsvFileHeaders = new List<string>()
        {
            "Station",
            "Spots",
            "Daypart",
            "Length",
            "Daypart Code"
        };

        private static readonly List<string> CsvOptionalFileHeaders = new List<string>()
        {
            "Fixed"
        };

        private static readonly List<int> SpothLengths = new List<int>() { 15, 30, 60, 90, 120 };

        private Dictionary<string, int> _requiredFields;
        private Dictionary<string, int> _optionalFields;
        private Dictionary<string, int> _audienceFields;
        private Dictionary<string, DisplayAudience> _audienceMapping;

        public override Entities.RatesFile.RateSourceType RateSource
        {
            get { return RatesFile.RateSourceType.CNN; }
        }

        public override void ExtractFileData(System.IO.Stream stream, Entities.RatesFile ratesFile, List<Entities.RatesFileProblem> fileProblems)
        {
            //TODO: Fixme or remove.
            //try
            //{
            //    _ValidateInputFileParams();

            //    using (var parser = _SetupCSVParser(stream))
            //    {
            //        if (parser == null)
            //        {
            //            throw new Exception("Unable to read file data.");
            //        }

            //        var headerFields = parser.ReadFields();
            //        _requiredFields = _ValidateAndSetupRequiredFields(headerFields);
            //        _optionalFields = _ValidateAndSetupOptionalFields(headerFields);
            //        _audienceFields = _ValidateAndSetupAudienceFields(headerFields, fileProblems);

            //        ratesFile.StationPrograms.AddRange(_BuildStationProgramsList(parser, fileProblems));
            //    }

            //}
            //catch (Exception e)
            //{
            //    throw new Exception(string.Format("Unable to parse rate file: {0} The file may be invalid: {1}", e.Message, ratesFile.FileName), e);
            //}
        }

        //private void _ValidateInputFileParams()
        //{
        //    if (string.IsNullOrEmpty(Request.FileName))
        //        throw new Exception(string.Format("Unable to parse rate file: {0}. The name of the file is invalid.", Request.FileName));

        //    if (Request.RatesStream.Length == 0)
        //        throw new Exception(string.Format("Unable to parse rate file: {0}. Invalid file size.", Request.FileName));

        //    if (string.IsNullOrEmpty(Request.BlockName))
        //        throw new Exception(string.Format("Unable to parse rate file: {0}. The block name is invalid.", Request.FileName));

        //    if (!Request.FlightWeeks.Any())
        //        throw new Exception(string.Format("Unable to parse rate file: {0}. Invalid flight weeks.", Request.FileName));
        //}

        //private Dictionary<string, int> _ValidateAndSetupOptionalFields(IEnumerable<string> fieldsArray)
        //{
        //    var fields = fieldsArray.ToList();
        //    var headerDict = new Dictionary<string, int>();

        //    foreach (var header in CsvOptionalFileHeaders)
        //    {
        //        var headerItemIndex = fields.IndexOf(header);
        //        if (headerItemIndex < 0) 
        //            continue;
        //        headerDict.Add(header, headerItemIndex);
        //    }

        //    return headerDict;
        //}

        //private Dictionary<string, int> _ValidateAndSetupRequiredFields(string[] fieldsArray)
        //{
        //    var fields = fieldsArray.ToList();
        //    var validationErrors = new List<string>();
        //    Dictionary<string, int> headerDict = new Dictionary<string, int>();

        //    foreach (var header in CsvFileHeaders)
        //    {
        //        int headerItemIndex = fields.IndexOf(header);
        //        if (headerItemIndex >= 0)
        //        {
        //            headerDict.Add(header, headerItemIndex);
        //            continue;
        //        }
        //        validationErrors.Add(string.Format("Could not find required column {0}.", header));
        //    }

        //    if (validationErrors.Any())
        //    {
        //        string message = "";
        //        validationErrors.ForEach(err => message += err + Environment.NewLine);
        //        throw new ApplicationException(message);
        //    }
        //    return headerDict;
        //}

        //private TextFieldParser _SetupCSVParser(Stream rawStream)
        //{
        //    var parser = new TextFieldParser(rawStream);
        //    if (parser.EndOfData)
        //    {
        //        parser.Close();
        //        return null;
        //    }

        //    parser.SetDelimiters(new string[] { "," });

        //    return parser;
        //}

        //private IEnumerable<StationProgram> _BuildStationProgramsList(TextFieldParser parser, List<RatesFileProblem> fileProblems)
        //{
        //    var stationPrograms = new List<StationProgram>();
        //    while (!parser.EndOfData)
        //    {
        //        var currentLine = parser.LineNumber;
        //        var currentData = parser.ReadFields();
        //        if (_IsEmptyLine(currentData))
        //        {
        //            continue; //skip line if empty
        //        }

        //        if (_IsIncompleteLine(currentData))
        //        {
        //            fileProblems.Add(new RatesFileProblem()
        //            {
        //                ProblemDescription = string.Format("Incomplete record at line {0}", currentLine),
        //                ProgramName = Request.BlockName,
        //                StationLetters = currentData[_requiredFields["Station"]]
        //            });
        //            continue;
        //        }

        //        try
        //        {
        //            var stationProgram = _BuildStationProgram(currentData, fileProblems);

        //            // validation rule for adding same station with same spot lenght
        //            if (_CheckStationAndDaypartAndSpotLengthExists(stationProgram, stationPrograms, fileProblems))
        //                stationPrograms.Add(stationProgram);
        //        }
        //        catch (Exception e)
        //        {
        //            fileProblems.Add(new RatesFileProblem()
        //            {
        //                ProblemDescription = string.Format("Error while parsing record at line {0}: {1}", currentLine, e.Message),
        //                ProgramName = Request.BlockName,
        //                StationLetters = currentData[_requiredFields["Station"]]
        //            });
        //        }
        //    }

        //    if (!stationPrograms.Any() && fileProblems.Any() && fileProblems.All(a => a.ProblemDescription.Contains("Invalid station")))
        //        throw new Exception(string.Format("There are no known stations in the file {0}", this.Request.FileName));

        //    if (!stationPrograms.Any() && fileProblems.Any() && fileProblems.All(a => a.ProblemDescription.Contains("Invalid daypart")))
        //        throw new Exception(string.Format("There are no valid dayparts in the file {0}", this.Request.FileName));

        //    if (!stationPrograms.Any() && fileProblems.Any() && fileProblems.All(a => a.ProblemDescription.Contains("Invalid spot length for program")))
        //        throw new Exception(string.Format("There are no valid spot length in the file {0}", this.Request.FileName));

        //    _CheckIfFixedPriceIsTheSameForDaypart(stationPrograms);

        //    return stationPrograms;
        //}

        //private void _CheckIfFixedPriceIsTheSameForDaypart(IEnumerable<StationProgram> stationPrograms)
        //{
        //    var stationProgramsGroupedByDaypartCode = stationPrograms.GroupBy(stationProgram => stationProgram.DaypartCode);

        //    foreach (var stationProgram in stationProgramsGroupedByDaypartCode)
        //    {
        //        var firstStationProgram = stationProgram.First();
        //        var stationProgramWithDifferentFixedPrice = stationProgram.FirstOrDefault(s => s.FixedPrice != firstStationProgram.FixedPrice);

        //        if (stationProgramWithDifferentFixedPrice != null)
        //        {
        //            throw new Exception(string.Format("Daypart code {0} has multiple costs, please correct the error", firstStationProgram.DaypartCode));
        //        }
        //    }
        //}

        //private bool _IsValidSpotLenght(int spotLength)
        //{
        //    return SpothLengths.Contains(spotLength);
        //}

        //private bool _CheckStationAndDaypartAndSpotLengthExists(StationProgram program, List<StationProgram> stationPrograms,
        //    List<RatesFileProblem> fileProblems)
        //{
        //    if (!stationPrograms.Any(
        //        p =>
        //            p.StationLegacyCallLetters == program.StationLegacyCallLetters &&
        //            p.SpotLength == program.SpotLength &&
        //            p.Daypart.Id == program.Daypart.Id)) return true;

        //    fileProblems.Add(new RatesFileProblem()
        //    {
        //        ProblemDescription = string.Format("Invalid data for Station {0}, duplicate entry for same spot length", program.StationLegacyCallLetters),
        //        ProgramName = Request.BlockName,
        //        StationLetters = program.StationLegacyCallLetters
        //    });

        //    return false;
        //}

        //private StationProgram _BuildStationProgram(string[] currentData, List<RatesFileProblem> fileProblems)
        //{
        //    var station = currentData[_requiredFields["Station"]];
        //    var daypart = ParseStringToDaypart(currentData[_requiredFields["Daypart"]], station);
        //    var stationCallLetters = _ParseStationCallLetters(station);
        //    // check for valid spot length
        //    var spotLength = _ParseSpotLength(currentData[_requiredFields["Length"]], stationCallLetters);
        //    // parse daypart code
        //    var daypartCode = _ParseDaypartCode(currentData[_requiredFields["Daypart Code"]], stationCallLetters);

        //    var program = new StationProgram();
        //    program.Daypart = daypart;
        //    program.Daypart.Id = _DaypartCache.GetIdByDaypart(program.Daypart);
        //    program.DayPartName = daypart.Preview;
        //    program.StartDate = Request.FlightStartDate;
        //    program.EndDate = Request.FlightEndDate;
        //    program.FlightWeeks = _BuildProgramFlightWeeks(Request.FlightWeeks, currentData, fileProblems);
        //    program.ProgramName = Request.BlockName; // selected by user
        //    program.StationLegacyCallLetters = stationCallLetters;
        //    program.SpotLength = spotLength;
        //    program.DaypartCode = daypartCode;
        //    program.RateSource = RateSource;
        //    program.FixedPrice = ParseFixedPrice(currentData, "Fixed");

        //    return program;
        //}

        //private string _ParseDaypartCode(string daypartCode, string stationCallLetters)
        //{
        //    var dayCode = daypartCode.Trim();
        //    if (string.IsNullOrEmpty(dayCode) || daypartCode.Length > 10 || !dayCode.All(c => char.IsLetterOrDigit(c) || c == ' '))
        //    {
        //        throw new Exception(string.Format("Invalid 'daypart code' format for Station {0}", stationCallLetters));
        //    }

        //    return dayCode;
        //}

        //private int _ParseSpotLength(string length, string stationCallLetters)
        //{
        //    int spotLength;

        //    if (!int.TryParse(length, out spotLength))
        //        throw new Exception(string.Format("Invalid spot length for program '{0}' on Station '{1}'", this.Request.BlockName, stationCallLetters));
        //    if (!_IsValidSpotLenght(spotLength))
        //        throw new Exception(string.Format("Invalid spot length for program '{0}' on Station '{1}'", this.Request.BlockName, stationCallLetters));

        //    return spotLength;
        //}


        //private List<StationProgramFlightWeek> _BuildProgramFlightWeeks(List<FlightWeekDto> flightWeeks, string[] currentData, List<RatesFileProblem> fileProblems)
        //{
        //    var flightweeks = new List<StationProgramFlightWeek>();

        //    foreach (var mediaweek in flightWeeks)
        //    {
        //        var flightweek = new StationProgramFlightWeek
        //        {
        //            Active = !mediaweek.IsHiatus,
        //            FlightWeek = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(
        //                mediaweek.StartDate, mediaweek.EndDate).FirstOrDefault()
        //        };
        //        flightweek.Spots = int.Parse(currentData[_requiredFields["Spots"]]);
        //        flightweek.Audiences = _BuildFlightweekAudiences(currentData, fileProblems);
        //        flightweeks.Add(flightweek);
        //    }

        //    return flightweeks;
        //}

        //private decimal? ParseFixedPrice(string[] currentData, string optionalHeader)
        //{
        //    int headerIndex;
        //    if (!_optionalFields.TryGetValue(optionalHeader, out headerIndex))
        //        return null;
        //    return Convert.ToDecimal(currentData[headerIndex]);
        //}

        //private List<StationProgramFlightWeekAudience> _BuildFlightweekAudiences(string[] currentData, List<RatesFileProblem> fileProblems)
        //{
        //    List<StationProgramFlightWeekAudience> flightWeekAudiences = new List<StationProgramFlightWeekAudience>();
        //    foreach (var audienceField in _audienceFields)
        //    {
        //        if (audienceField.Value >= currentData.Length || string.IsNullOrEmpty(currentData[audienceField.Value]))
        //        {
        //            fileProblems.Add(new RatesFileProblem()
        //            {
        //                ProblemDescription =string.Format("Invalid demo for station {0}", currentData[_requiredFields["Station"]]), 
        //                ProgramName = Request.BlockName,
        //                StationLetters = currentData[_requiredFields["Station"]]
        //            });

        //            continue;
        //        }

        //        var flightWeekAudience = new StationProgramFlightWeekAudience();
        //        flightWeekAudience.Audience = _audienceMapping[audienceField.Key];

        //        var audienceCpm = decimal.Parse(currentData[audienceField.Value]);

        //        var spotLength = int.Parse(currentData[_requiredFields["Length"]]);
        //        switch (spotLength)
        //        {
        //            case 15:
        //                flightWeekAudience.Cpm15 = audienceCpm;
        //                break;
        //            case 30:
        //                flightWeekAudience.Cpm30 = audienceCpm;
        //                break;
        //            case 60:
        //                flightWeekAudience.Cpm60 = audienceCpm;
        //                break;
        //            case 90:
        //                flightWeekAudience.Cpm90 = audienceCpm;
        //                break;
        //            case 120:
        //                flightWeekAudience.Cpm120 = audienceCpm;
        //                break;
        //        }
        //        flightWeekAudiences.Add(flightWeekAudience);
        //    }
        //    return flightWeekAudiences;
        //}

        //private string _ParseStationCallLetters(string stationName)
        //{
        //    // check if it is legacy or the call letters
        //    var foundStation = _GetDisplayBroadcastStation(stationName);

        //    if (foundStation == null)
        //    {
        //        var station = stationName.Replace("-TV", "").Trim();
        //        foundStation = _GetDisplayBroadcastStation(station);
        //    }

        //    if (foundStation == null)
        //        throw new Exception(string.Format("Invalid station: {0}.", stationName));

        //    return foundStation.LegacyCallLetters;
        //}

        //private DisplayBroadcastStation _GetDisplayBroadcastStation(string stationName)
        //{
        //    var _stationRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
        //    return _stationRepository.GetBroadcastStationByLegacyCallLetters(stationName) ??
        //                        _stationRepository.GetBroadcastStationByCallLetters(stationName);
        //}

        //private Dictionary<string, int> _ValidateAndSetupAudienceFields(string[] fieldsArray, List<RatesFileProblem> fileProblems)
        //{
        //    Dictionary<string, int> audienceFieldsMap = new Dictionary<string, int>();
        //    _audienceMapping = new Dictionary<string, DisplayAudience>();
        //    var allFields = fieldsArray.ToList();
        //    var audienceFields = fieldsArray.Except(_requiredFields.Keys).Except(_optionalFields.Keys).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
        //    var validationErrors = new List<string>();

        //    foreach (var field in audienceFields)
        //    {
        //        int fieldIndex = allFields.IndexOf(field);
        //        try
        //        {
        //            var maestroAudience = _GetMaestroAudienceIdByAudienceCode(field);
        //            audienceFieldsMap.Add(field, fieldIndex);
        //            _audienceMapping.Add(field, maestroAudience);
        //        }
        //        catch (Exception e)
        //        {
        //            fileProblems.Add(new RatesFileProblem()
        //            {
        //                ProblemDescription = string.Format("Could not find Maestro audience mapping to {0}: {1}", field, e.Message),
        //                ProgramName = this.Request.BlockName
        //            });
        //        }
        //    }

        //    return audienceFieldsMap;
        //}

        //private DisplayAudience _GetMaestroAudienceIdByAudienceCode(string audienceCode)
        //{
        //    var matchingAudience = _AudiencesCache.GetDisplayAudienceByCode(audienceCode);

        //    if (matchingAudience == null)
        //    {
        //        throw new Exception(
        //            string.Format("Unable to find matching maestro audience {0}.", audienceCode));
        //    }
        //    return matchingAudience;
        //}

        //private bool _IsEmptyLine(string[] fieldArray)
        //{
        //    foreach (var field in fieldArray)
        //    {
        //        if (!String.IsNullOrEmpty(field))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        //private bool _IsIncompleteLine(string[] fieldArray)
        //{
        //    foreach (var requiredField in _requiredFields)
        //    {
        //        if (string.IsNullOrEmpty(fieldArray[requiredField.Value]))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }
}
