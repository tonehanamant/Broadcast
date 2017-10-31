using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Microsoft.Practices.ObjectBuilder2;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Validators;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.RateImport
{

    public class CNNFileImporter : InventoryFileImporterBase
    {
        private class CNNFileDto
        {
            public int RowNumber { get; set; }
            public DisplayBroadcastStation Station { get; set; }
            public DateTime EffectiveDate { get; set; }
            public string DaypartCode { get; set; }
            public int SpotLengthId { get; set; }
            public int SpotsPerWeek { get; set; }
            public int SpotsPerDay { get; set; }
            public List<DisplayDaypart> Dayparts { get; set; }

            public override string ToString()
            {
                return "R:" + RowNumber + ";S:" + Station.LegacyCallLetters + ";DPC:" + DaypartCode + ";SPW:" + SpotsPerWeek;
            }
        }

        public const string NoGoodDaypartsFound = "There are no valid dayparts in the file";
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

        private DateTime _EffectiveDate;
        private ICNNStationInventoryGroupService _CNNStationInventoryGroupService;
        private readonly IInventoryFileValidator _InventoryFileValidator;

        public CNNFileImporter(ICNNStationInventoryGroupService CNNStationInventoryGroupService,
                                        IInventoryFileValidator inventoryFileValidator)
        {
            _CNNStationInventoryGroupService = CNNStationInventoryGroupService;
            _InventoryFileValidator = inventoryFileValidator;
        }

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

        public override Entities.InventoryFile.InventorySource InventoryFileSource
        {
            get { return InventoryFile.InventorySource.CNN; }
        }

        public override InventorySource InventorySource { get; set; }

        public override InventoryFile GetPendingInventoryFile()
        {
            var result = new InventoryFile();
            return HydrateInventoryFile(result);
        }

        public override void ExtractFileData(Stream stream, InventoryFile inventoryFile, DateTime effectiveDate,List<InventoryFileProblem> fileProblems)
        {
            _EffectiveDate = effectiveDate;

            _FileProblems = fileProblems;
            try
            {
                _ValidateInputFileParams();
                List<CNNFileDto> dtos = new List<CNNFileDto>();

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

                        var spotPerDay = _GetCellValue(row, "Spots Per Day").ToUpper();
                        var spotsPerday = _ParseNumericPositiveInt(spotPerDay, "Invalid Spots Per Day \"{0}\"");

                        var dps = _GetCellValue(row, "Time Periods").ToUpper();
                        var dayparts = _ParseDayparts(dps, stationName);

                        var effectiveDateValue = _GetCellValue(row, "Effective Date of New Barter Added").ToUpper();

                        if (!string.IsNullOrEmpty(effectiveDateValue))
                        {
                            if (!DateTime.TryParse(effectiveDateValue, out effectiveDate))
                                throw new Exception(string.Format("Invalid effective date {0} found.",effectiveDateValue));
                        }
                        else
                        {
                            effectiveDate = _EffectiveDate;
                        }
                        dtos.Add(new CNNFileDto()
                        {
                            RowNumber = row,
                            Station = station,
                            EffectiveDate = effectiveDate,
                            DaypartCode = daypartCode,
                            SpotLengthId = spotLengthId,
                            SpotsPerWeek = spots,
                            SpotsPerDay = spotsPerday,
                            Dayparts = dayparts
                        });
                    }
                    if (!_FileProblems.Any())
                    {
                        var dupProblems = CheckDups(dtos);
                        _FileProblems.AddRange(dupProblems);
                    }

                    if (!_FileProblems.Any())
                    {
                        dtos.ForEach(_AddNewInventory);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    string.Format("Unable to parse inventory file: {0} The file may be invalid: {1}", e.Message,inventoryFile.FileName), e);
            }
            if (!_FoundGoodDaypart)
                throw new Exception(NoGoodDaypartsFound);

            inventoryFile.InventoryGroups = _InventoryGroups.Values.SelectMany(v => v).ToList();
        }

        private List<InventoryFileProblem> CheckDups(List<CNNFileDto> dtos)
        {
            var dups =
                dtos.GroupBy(
                    d =>
                        new
                        {
                            d.Station.LegacyCallLetters,
                            d.SpotLengthId,
                            dayparts =
                            d.Dayparts.Select(dp => dp.ToUniqueString())
                                .OrderBy(dp => dp)
                                .Aggregate((current, next) => current + "," + next)
                        });

            var dupProblems = dups.Where(d => d.Count() > 1)
                .Select(d => _InventoryFileValidator.DuplicateRecordValidation(d.Key.LegacyCallLetters)).ToList();
            return dupProblems;
        }

        private Dictionary<string, List<StationInventoryGroup>> _InventoryGroups = new Dictionary<string, List<StationInventoryGroup>>();
        private void _AddNewInventory(CNNFileDto dto)
        {
            var groups = EnsureGroup(dto.DaypartCode);

            int maxSlots = _CNNStationInventoryGroupService.GetSlotCount(dto.DaypartCode);
            int slotToUse = 1;
            for (int c = 1; c <= dto.SpotsPerWeek; c++)
            {
                var group = groups.Single(g => g.SlotNumber == slotToUse);
                var manifest = group.Manifests.FirstOrDefault(m => m.EndDate == null
                                    && m.ManifestDayparts.All(dp => dto.Dayparts.Any(mdp => mdp == dp.Daypart))
                                    && m.Station.Code == dto.Station.Code
                                    && m.SpotLengthId == dto.SpotLengthId);

                if (manifest == null)
                {
                    var manifestDayparts = dto.Dayparts.Select(
                        d => new StationInventoryManifestDaypart()
                        {
                            Daypart = d
                        }).ToList();
                    manifest = new StationInventoryManifest()
                    {
                        DaypartCode = dto.DaypartCode,
                        SpotLengthId = dto.SpotLengthId,
                        ManifestDayparts = manifestDayparts,
                        SpotsPerDay = dto.SpotsPerDay,
                        EffectiveDate = dto.EffectiveDate,
                        Station = dto.Station,
                        SpotsPerWeek = 0
                    };
                    group.Manifests.Add(manifest);
                }
                manifest.SpotsPerWeek++;

                slotToUse++;
                if (slotToUse > maxSlots)
                    slotToUse = 1;
            }
        }

        public void AddGroup(StationInventoryGroup group)
        {
            List<StationInventoryGroup> groups;
            if (!_InventoryGroups.TryGetValue(group.DaypartCode, out groups))
            {
                throw new Exception("Daypart Code not found in group; disaster");
            }
            groups.Add(group);
        }

        /// <summary>
        /// Gets StationInventoryGroups for the given daypart.  Creates new ones if needed.
        /// </summary>
        private List<StationInventoryGroup> EnsureGroup(string dpCode)
        {
            List<StationInventoryGroup> groups;
            if (!_InventoryGroups.TryGetValue(dpCode, out groups))
            {
                groups = new List<StationInventoryGroup>();

                int slotCount = _CNNStationInventoryGroupService.GetSlotCount(dpCode);
                for (int slotNumber = 1; slotNumber <= slotCount; slotNumber++)
                {
                    groups.Add(new StationInventoryGroup()
                    {
                        DaypartCode = dpCode,
                        SlotNumber = slotNumber,
                        Name = _CNNStationInventoryGroupService.GenerateGroupName(dpCode, slotNumber),
                        InventorySource = this.InventorySource,
                        StartDate = _EffectiveDate
                    });
                }
                _InventoryGroups.Add(dpCode, groups);
            }
            return groups;
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
                throw new Exception(string.Format("Unable to parse inventory file: {0}. The name of the file is invalid.",
                    Request.FileName));

            if (Request.RatesStream.Length == 0)
                throw new Exception(string.Format("Unable to parse inventory file: {0}. Invalid file size.", Request.FileName));
        }


        private List<DisplayDaypart> _ParseDayparts(string daypartInput, string station)
        {
            List<DisplayDaypart> dayparts = new List<DisplayDaypart>();
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

                        var timeOfDaypart = daypart.Substring(0, lastColon);
                        var splitTimeOfDaypart = timeOfDaypart.Split('-');
                        if (splitTimeOfDaypart.Length != 2 ||
                            (!splitTimeOfDaypart[0].ToUpper().EndsWith("A") && !splitTimeOfDaypart[0].ToUpper().EndsWith("P"))
                            ||
                            (!splitTimeOfDaypart[1].ToUpper().EndsWith("A") && !splitTimeOfDaypart[1].ToUpper().EndsWith("P")))
                            throw new Exception();

                        daypart = string.Format("{1} {0}", timeOfDaypart, dayOfWeekPart);
                        dayparts.Add(ParseStringToDaypart(daypart, station));
                        _FoundGoodDaypart = true;
                    }
                    catch (Exception e)
                    {
                        _AddProblem(string.Format("Invalid daypart '{0}' on Station {1}.", dp, station));
                    }
                });

            }
            catch (Exception)
            {
                _AddProblem(string.Format("Invalid daypart for station: {0}",station));
            }

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
                _AddProblem(string.Format("Invalid spot length on Station '{0}'", stationName));
            if (!_IsValidSpotLenght(spotLength))
                _AddProblem(string.Format("Invalid spot length on Station '{0}'", stationName));


            if (_SpothLengths == null)
            {
                var spotLengthRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
                _SpothLengths = spotLengthRepository.GetSpotLengthAndIds();
            }

            int spotLengthId;
            if (!_SpothLengths.TryGetValue(spotLength,out spotLengthId))
                _AddProblem(string.Format("Invalid spot length on Station '{0}'", stationName));

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
