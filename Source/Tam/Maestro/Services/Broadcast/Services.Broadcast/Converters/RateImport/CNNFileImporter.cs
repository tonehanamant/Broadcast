using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Practices.ObjectBuilder2;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public class CNNFileImporter : InventoryFileImporterBase
    {
        private static readonly List<string> XlsFileHeaders = new List<string>()
        {
            "Customer Name",
            "Barter Network",
            "Spot Length",
            "# Spots",
            "Spots Per Day",
            "Time Periods",
            "Effective Date of New Barter Added",
            "Traffic Contact Info",
            "Traffic Contact Email",
            "Comments"
        };

        private static readonly List<string> _ValidDaypartCodes = new List<string>()
        {
            "AM News",
            "PM News",
            "Entertainment"
        };
        private Dictionary<int, int> _SpothLengths = null;

        List<InventoryFileProblem> _FileProblems = new List<InventoryFileProblem>();
        private OfficeOpenXml.ExcelWorksheet _Worksheet;
        private Dictionary<string, int> _Headers;

        private int DataStartRow;

        private static readonly List<int> SpothLengths = new List<int>() {15, 30, 60, 90, 120};
        
        private bool _FoundGoodDaypart;

        public override Entities.InventoryFile.InventorySourceType InventorySource
        {
            get { return InventoryFile.InventorySourceType.CNN; }
        }

        public override void ExtractFileData(Stream stream, InventoryFile inventoryFile, List<InventoryFileProblem> fileProblems)
        {
            _FileProblems = fileProblems;
            try
            {
                _ValidateInputFileParams();

                using (var excelPackage = new OfficeOpenXml.ExcelPackage(stream))
                {
                    _Worksheet = excelPackage.Workbook.Worksheets.First();
                    _Headers = _SetupHeadersValidateSheet();
                    for (int row = DataStartRow; row <= _Worksheet.Dimension.End.Row; row++)
                    {
                        if (_IsEmptyRow(row))
                            break; // empty row, done!

                        var stationName = _GetCellValue(row, "Customer Name").ToUpper();
                        var station = _ParseStationCallLetters(stationName);
                        if (station == null)
                        {
                            _AddProblem(string.Format("Invalid station: {0}", stationName));
                        }
                        var daypartCode = _GetCellValue(row, "Barter Network");
                        if (!_ValidDaypartCodes.Contains(daypartCode))
                        {
                            _AddProblem(string.Format("Invalid daypart code format for Station {0}",
                                station.LegacyCallLetters));
                        }
                        var length = _GetCellValue(row, "Spot Length").ToUpper();
                        int spotLengthId = _ParseSpotLength(length, station);

                        var spotPerWeeks = _GetCellValue(row, "# Spots").ToUpper();
                        var spots = _ParseNumericPositiveInt(spotPerWeeks, "Invalid Spots Per Week \"{0}\"");

                        var spotPerDay = _GetCellValue(row, "# Spots").ToUpper();
                        var spotsPerday = _ParseNumericPositiveInt(spotPerDay, "Invalid Spots Per Day \"{0}\"");

                        var dps = _GetCellValue(row, "Time Periods").ToUpper();
                        var dayparts = _ParseDayparts(dps, stationName);

//                        inventoryFile.StationInventoryManifests.Add(new StationInventoryManifest()
//                        {
//                            Station = station,
//                            DaypartCode = daypartCode,
//                            SpotLengthId = spotLengthId,
//                            SpotsPerWeek = spots,
//                            SpotsPerDay = spotsPerday,
//                            Dayparts = dayparts                        
//                        });
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    string.Format("Unable to parse rate file: {0} The file may be invalid: {1}", e.Message,inventoryFile.FileName), e);
            }
            if (!_FoundGoodDaypart)
                throw new Exception("There are no valid dayparts in the file");
        }

        private void _AddProblem(string description, string stationLetters = null, string programName = null,
            List<string> affectedProposals = null)
        {
            _FileProblems.Add(new InventoryFileProblem()
            {
                AffectedProposals = affectedProposals,
                ProblemDescription = description,
                ProgramName = programName,
                StationLetters = stationLetters
            });
        }

        private StationContact _ParseStationContact(string contactInfo, string contactEmail)
        {
            StationContact stationContact = new StationContact();


            return stationContact;
        }

        private Dictionary<string, int> _SetupHeadersValidateSheet()
        {
            var validationErrors = new List<string>();
            Dictionary<string, int> headerDict = new Dictionary<string, int>();

            int headerRow = 1;
            DataStartRow = headerRow + 1;

            foreach (var header in XlsFileHeaders)
            {
                for (int column = 1; column <= _Worksheet.Dimension.End.Column; column++)
                {
                    var cellValue = _GetCellValue(headerRow, column);
                    if (cellValue.ToUpper() == header.ToUpper())
                    {
                        headerDict.Add(header, column);
                        break;
                    }
                }
                if (!headerDict.ContainsKey(header))
                    validationErrors.Add(string.Format("Could not find required column {0}.<br />", header));
            }

            if (validationErrors.Any())
            {
                string message = "";
                validationErrors.ForEach(err => message += err + Environment.NewLine);
                throw new Exception(message);
            }
            return headerDict;
        }

        private void _ValidateInputFileParams()
        {
            if (string.IsNullOrEmpty(Request.FileName))
                throw new Exception(string.Format("Unable to parse rate file: {0}. The name of the file is invalid.",
                    Request.FileName));

            if (Request.RatesStream.Length == 0)
                throw new Exception(string.Format("Unable to parse rate file: {0}. Invalid file size.", Request.FileName));

            if (string.IsNullOrEmpty(Request.BlockName))
                throw new Exception(string.Format("Unable to parse rate file: {0}. The block name is invalid.",
                    Request.FileName));

            if (!Request.FlightWeeks.Any())
                throw new Exception(string.Format("Unable to parse rate file: {0}. Invalid flight weeks.",
                    Request.FileName));
        }


        private List<DisplayDaypart> _ParseDayparts(string daypartInput, string station)
        {
            List<DisplayDaypart> dayparts = new List<DisplayDaypart>();
            bool badDaypart = false;
            try
            {
                var dps = Regex.Replace(daypartInput, "\\s+", "\r\n");
                string[] daypartStrings = Regex.Split(dps, "\r\n|\r|\n");
                daypartStrings.ForEach(dp =>
                {
                    try
                    {
                        // switch order of days vs times and add space between them instead of ":"
                        string daypart = dp;
                        int spaceIndex = dp.IndexOf(" ");
                        if (spaceIndex > 0) // remove everything past first space
                            daypart = daypart.Substring(0, spaceIndex);

                        int lastColon = daypart.LastIndexOf(":");
                        var dayOfWeekPart = daypart.Substring(lastColon + 1);
                        dayOfWeekPart = dayOfWeekPart.Replace(";", ",");

                        var timeOfDayPart = daypart.Substring(0, lastColon);

                        daypart = string.Format("{1} {0}", timeOfDayPart, dayOfWeekPart);
                        dayparts.Add(ParseStringToDaypart(daypart, station));
                    }
                    catch (Exception e)
                    {
                        _AddProblem(e.Message);
                    }
                });

            }
            catch (Exception)
            {
                badDaypart = true;
                _AddProblem(string.Format("Invalid daypart for station: {0}",station));
            }
            if (!badDaypart)
                _FoundGoodDaypart = true;

            return dayparts;
        }


        private int _ParseNumericPositiveInt(string value,string errorMessage)
        {
            int num;
            if (!int.TryParse(value, out num) || num < 0)
            {
                _AddProblem(string.Format(errorMessage,value));
                return -1;
            }
            return num;
        }

        private int _ParseSpotLength(string length, DisplayBroadcastStation station)
        {
            int spotLength;

            int spaceIndex = length.IndexOf(" ");
            if (spaceIndex>=0)
                length = length.Substring(0, spaceIndex);

            string stationName = string.Empty;
            if (station!=null) stationName = station.LegacyCallLetters;
            
            if (!int.TryParse(length, out spotLength))
                _AddProblem(string.Format("Invalid spot length for program '{0}' on Station '{1}'",this.Request.BlockName, stationName));
            if (!_IsValidSpotLenght(spotLength))
                _AddProblem(string.Format("Invalid spot length for program '{0}' on Station '{1}'", this.Request.BlockName, stationName));


            if (_SpothLengths == null)
            {
                var spotLengthRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
                _SpothLengths = spotLengthRepository.GetSpotLengthAndIds();
            }

            int spotLengthId;
            if (!_SpothLengths.TryGetValue(spotLength,out spotLengthId))
                _AddProblem(string.Format("Invalid spot length for program '{0}' on Station '{1}'", this.Request.BlockName, stationName));

            return spotLengthId;
        }

        private bool _IsValidSpotLenght(int spotLength)
        {
            return SpothLengths.Contains(spotLength);
        }

        private DisplayBroadcastStation _ParseStationCallLetters(string stationName)
        {
            // check if it is legacy or the call letters
            var foundStation = _GetDisplayBroadcastStation(stationName);

            if (foundStation == null)
            {
                var station = stationName.Replace("-TV", "").Trim();
                foundStation = _GetDisplayBroadcastStation(station);
            }

            if (foundStation == null)
                _AddProblem(string.Format("Invalid station: {0}", stationName));

            return foundStation;
        }

        private DisplayBroadcastStation _GetDisplayBroadcastStation(string stationName)
        {
            var _stationRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            return _stationRepository.GetBroadcastStationByLegacyCallLetters(stationName) ??
                                _stationRepository.GetBroadcastStationByCallLetters(stationName);
        }



        private string _GetCellValue(int row, string columnName)
        {
            return _Worksheet.Cells[row, _Headers[columnName]].Text.Trim();
        }

        private string _GetCellValue(int row, int column)
        {
            return _Worksheet.Cells[row, column].Text.Trim();
        }
        private bool _IsEmptyRow(int row)
        {
            for (int c = 1; c < _Worksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrEmpty(_Worksheet.Cells[row, c].Text))
                    return false;
            return true;
        }

    }
}
