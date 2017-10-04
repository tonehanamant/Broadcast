using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Converters.RateImport
{
    public class AssemblyInventoryImporter : InventoryFileImporterBase
    {
        public override InventoryFile.InventorySourceType InventorySource
        {
            get { return InventoryFile.InventorySourceType.Assembly; }
        }

        private static readonly List<string> CsvFileHeaders = new List<string>()
        {
            "MARKET",
            "STATION",
            "Start Date",
            "End Date",
            "LEN", //spot length
            "AFFIL", //network affiliation
            "DAYPART",
            "ROTATION",
            "TIMES",
            "PROGRAM",
            "30 RATE", //30s spot rate
        };

        private Dictionary<string, int> _requiredFields;
        private Dictionary<string, int> _audienceFields;
        private Dictionary<string, DisplayAudience> _audienceMapping; 

        public override void ExtractFileData(Stream rawStream, InventoryFile inventoryFile, List<InventoryFileProblem> fileProblems)
        {
            //TODO: Fixme or remove.

            //try
            //{

            //    using (var parser = _SetupCSVParser(rawStream))
            //    {
            //        if (parser == null)
            //        {
            //            throw new Exception("Unable to read file data.");
            //        }
            //        var filePeriodLine = parser.ReadFields();
            //        var filePeriod = filePeriodLine[0];
            //        _SetRateFileDatesFromPeriod(ratesFile, filePeriod);

            //        var headerFields = parser.ReadFields();
            //        _requiredFields = _ValidateAndSetupRequiredFields(headerFields);
            //        _audienceFields = _ValidateAndSetupAudienceFields(headerFields, fileProblems);

            //        ratesFile.StationPrograms.AddRange(_BuildStationProgramsList(parser, fileProblems));

            //    }

            //}
            //catch (Exception e)
            //{
            //    throw new Exception(string.Format("Unable to parse Assembly rate file: {0} The file may be invalid: {1}", e.Message, ratesFile.FileName), e);
            //}
        }

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

        //        var missingFields = _GetMissingFields(currentData);
        //        if (missingFields.Count > 0)
        //        {
        //            fileProblems.Add(new RatesFileProblem()
        //            {
        //                ProblemDescription = string.Format("Missing values for fields on line {0}: {1}", currentLine, string.Join(", ", missingFields)),
        //                ProgramName = currentData[_requiredFields["PROGRAM"]],
        //                StationLetters = currentData[_requiredFields["STATION"]]

        //            });
        //            continue;
        //        }

        //        if (!currentData[_requiredFields["LEN"]].Equals("30"))
        //        {
        //            fileProblems.Add(new RatesFileProblem()
        //            {
        //                ProblemDescription = string.Format("Invalid spot length on line {0}: {1}", currentLine, string.Join(", ", currentData[_requiredFields["LEN"]])),
        //                ProgramName = currentData[_requiredFields["PROGRAM"]],
        //                StationLetters = currentData[_requiredFields["STATION"]]

        //            });
        //            continue;
        //        }

        //        try
        //        {
        //            var stationProgram = _BuildStationProgram(currentData);
        //            stationPrograms.Add(stationProgram);

        //        }
        //        catch (Exception e)
        //        {
        //            fileProblems.Add(new RatesFileProblem()
        //            {
        //                ProblemDescription = string.Format("Error while parsing record at line {0}: {1}", currentLine, e.Message),
        //                ProgramName = currentData[_requiredFields["PROGRAM"]],
        //                StationLetters = currentData[_requiredFields["STATION"]]
        //            });
        //            continue;
        //        }

        //    }
        //    return stationPrograms;

        //}

        //private StationProgram _BuildStationProgram(string[] currentData)
        //{
        //    var program = new StationProgram();

        //    program.Daypart = AssemblyImportHelper.LookupDaypartByTimeAndRotation(currentData[_requiredFields["TIMES"]], currentData[_requiredFields["ROTATION"]],_DaypartCache);
        //    program.DayPartName = currentData[_requiredFields["DAYPART"]];
        //    var startDate = _ParseDate(currentData[_requiredFields["Start Date"]]);
        //    program.StartDate = startDate;
        //    var endDate = _ParseDate(currentData[_requiredFields["End Date"]]);
        //    program.EndDate = endDate;
        //    program.ProgramName = currentData[_requiredFields["PROGRAM"]];
        //    program.StationLegacyCallLetters = _ParseStationCallLetters(currentData[_requiredFields["STATION"]]);

        //    if (!_IsValidRate(currentData[_requiredFields["30 RATE"]]))
        //        throw new ApplicationException(string.Format("Invalid rate for program '{0}' on Station '{1}'", program.ProgramName, program.StationLegacyCallLetters));

        //    program.FlightWeeks = _BuildProgramFlightweeks(startDate, endDate, currentData);

        //    return program;

        //}

        //private DateTime _ParseDate(string dateString)
        //{
        //    DateTime result;

        //    if (DateTime.TryParseExact(dateString, "M/d/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        //    {
        //        return result;
        //    }

        //    if (DateTime.TryParseExact(dateString, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        //    {
        //        return result;
        //    }

        //    throw new ApplicationException("Unable to parse start date.");
        //}

        //private bool _IsValidRate(string rateToParse)
        //{
        //    Decimal rate;
        //    var parsed = decimal.TryParse(rateToParse, out rate);
        //    return parsed && rate > 0;
        //}

        //private List<StationProgramFlightWeek> _BuildProgramFlightweeks(DateTime startDate, DateTime endDate, string[] currentData)
        //{
        //    var flightweeks = new List<StationProgramFlightWeek>();
        //    var mediaweeks = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(startDate, endDate);
        //    var rate = decimal.Parse(currentData[_requiredFields["30 RATE"]]);

        //    foreach (var mediaweek in mediaweeks)
        //    {
        //        var flightweek = new StationProgramFlightWeek();
        //        flightweek.Active = true;
        //        flightweek.FlightWeek = mediaweek;
        //        _ApplySpotLengthRateMultipliers(flightweek, rate);
        //        flightweek.Audiences = _BuildFlightweekAudiences(currentData);
        //        flightweeks.Add(flightweek);
        //    }

        //    return flightweeks;
        //}

        //private List<StationProgramFlightWeekAudience> _BuildFlightweekAudiences(string[] currentData)
        //{
        //    List<StationProgramFlightWeekAudience> flightWeekAudiences = new List<StationProgramFlightWeekAudience>();
        //    foreach (var audienceField in _audienceFields)
        //    {
        //        var flightWeekAudience = new StationProgramFlightWeekAudience()
        //        {
        //            Audience = _audienceMapping[audienceField.Key],
        //            Impressions = double.Parse(currentData[audienceField.Value]) * 1000
        //        };
        //        flightWeekAudiences.Add(flightWeekAudience);
        //    }
        //    return flightWeekAudiences;
        //}

        //private string _ParseStationCallLetters(string stationName)
        //{
        //    return stationName.Replace("-TV", "").Trim();
        //}


        //private List<string> _GetMissingFields(string[] fieldArray)
        //{
        //    var missingFields = new List<string>();

        //    foreach (var requiredField in _requiredFields)
        //    {
        //        // will be validated later when checking the actual value of the rate for better error handling
        //        if (requiredField.Key == "30 RATE")
        //            continue;

        //        if (string.IsNullOrEmpty(fieldArray[requiredField.Value]))
        //        {
        //            missingFields.Add(requiredField.Key);
        //        }
        //    }

        //    foreach (var audienceField in _audienceFields)
        //    {
        //        if (string.IsNullOrEmpty(fieldArray[audienceField.Value]))
        //        {
        //            missingFields.Add(audienceField.Key);
        //        }
        //    }

        //    return missingFields;
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

        //private void _SetRateFileDatesFromPeriod(RatesFile inventoryFile, string filePeriod)
        //{
        //    var quarter = int.Parse(filePeriod.Substring(0, 1));
        //    var year = int.Parse(filePeriod.Split(' ')[1]);

        //    int startMonth = 0;
        //    int endMonth = 0;
        //    switch (quarter)
        //    {
        //        case 1:
        //            startMonth = 1;
        //            endMonth = 3;
        //            break;
        //        case 2:
        //            startMonth = 4;
        //            endMonth = 6;
        //            break;
        //        case 3:
        //            startMonth = 7;
        //            endMonth = 9;
        //            break;
        //        case 4:
        //            startMonth = 10;
        //            endMonth = 12;
        //            break;
        //        default:
        //            throw new Exception(string.Format("Unable to determine quarter {0} for assembly rate file period {1}", quarter, filePeriod));
        //    }

        //    var startMediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(year, startMonth);
        //    var endMediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(year, endMonth);

        //    ratesFile.StartDate = startMediaMonth.StartDate;
        //    ratesFile.EndDate = endMediaMonth.EndDate;
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

        //private Dictionary<string, int> _ValidateAndSetupAudienceFields(string[] fieldsArray, List<RatesFileProblem> fileProblems)
        //{
        //    Dictionary<string, int> audienceFieldsMap = new Dictionary<string, int>();
        //    _audienceMapping = new Dictionary<string, DisplayAudience>();
        //    var allFields = fieldsArray.ToList();
        //    var audienceFields = fieldsArray.Except(_requiredFields.Keys).ToList();
        //    var validationErrors = new List<string>();

        //    foreach (var field in audienceFields)
        //    {
        //        int fieldIndex = allFields.IndexOf(field);
        //        try
        //        {
        //            var maestroAudience = _GetMaestroAudienceIdByAssemblyAudienceCode(field);
        //            audienceFieldsMap.Add(field, fieldIndex);
        //            _audienceMapping.Add(field, maestroAudience);
        //            continue;

        //        }
        //        catch (Exception e)
        //        {
        //            fileProblems.Add(new RatesFileProblem()
        //            {
        //                ProblemDescription = string.Format("Could not find Maestro audience mapping to {0}: {1}", field, e.Message)
        //            });    
        //        }                

        //    }

        //    return audienceFieldsMap;

        //}

        //private DisplayAudience _GetMaestroAudienceIdByAssemblyAudienceCode(string audienceCode)
        //{
        //    int startAge;
        //    int endAge;
        //    var subcategoryCode = AssemblyImportHelper.ExtractAudienceInfo(audienceCode, out startAge, out endAge);

        //    var matchingAudience =
        //        _BroadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>()
        //            .GetDisplayAudienceByAgeAndSubcategory(subcategoryCode, startAge, endAge);

        //    if (matchingAudience == null)
        //    {
        //        throw new Exception(
        //            string.Format("Unable to find matching maestro audience for subcategory {0} and age {1}-{2}.",
        //            subcategoryCode,
        //            startAge,
        //            endAge));
        //    }
        //    return matchingAudience;

        //}
    }
}
