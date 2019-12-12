using Common.Services;
using Common.Services.Repositories;
using Microsoft.Practices.ObjectBuilder2;
using OfficeOpenXml;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static Services.Broadcast.Entities.ProprietaryInventory.ProprietaryInventoryFile;

namespace Services.Broadcast.Converters.RateImport
{
    public class BarterFileImporter : ProprietaryFileImporterBase
    {
        private readonly FileCell INVENTORY_SOURCE_CELL = new FileCell { ColumnLetter = "B", RowIndex = 3 };
        private readonly FileCell DAYPART_CODE_CELL = new FileCell { ColumnLetter = "B", RowIndex = 4 };
        private readonly FileCell EFFECTIVE_DATE_CELL = new FileCell { ColumnLetter = "B", RowIndex = 5 };
        private readonly FileCell END_DATE_CELL = new FileCell { ColumnLetter = "B", RowIndex = 6 };
        private readonly FileCell CPM_CELL = new FileCell { ColumnLetter = "B", RowIndex = 7 };
        private readonly FileCell DEMO_CELL = new FileCell { ColumnLetter = "B", RowIndex = 8 };
        private readonly FileCell CONTRACTED_DAYPART_CELL = new FileCell { ColumnLetter = "B", RowIndex = 9 };
        private readonly FileCell SHARE_BOOK_CELL = new FileCell { ColumnLetter = "B", RowIndex = 10 };
        private readonly FileCell HUT_BOOK_CELL = new FileCell { ColumnLetter = "B", RowIndex = 11 };
        private readonly FileCell PLAYBACK_TYPE_CELL = new FileCell { ColumnLetter = "B", RowIndex = 12 };

        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        private readonly IImpressionsService _ImpressionsService;

        const string commentsHeader = "COMMENTS";
        const int unitNameRowIndex = 16;
        const int spotLengthRowIndex = 17;
        const int firstUnitColumnIndex = 3;
        private int _ErrorColumnIndex = 0;

        public BarterFileImporter(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            IImpressionsService impressionsService,
            IFileService fileService) : base(
                broadcastDataRepositoryFactory,
                broadcastAudiencesCache,
                inventoryDaypartParsingEngine,
                mediaMonthAndWeekAggregateCache,
                stationProcessingEngine,
                spotLengthEngine,
                fileService)
        {
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _ImpressionsService = impressionsService;
        }

        public override void LoadAndValidateHeaderData(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            var header = new ProprietaryInventoryHeader();
            var validationProblems = new List<string>();
            Dictionary<string, FileCell> requiredProperties = new Dictionary<string, FileCell>
                { {"Inv Source", INVENTORY_SOURCE_CELL }, {"Daypart Code",DAYPART_CODE_CELL }, {"Effective Date", EFFECTIVE_DATE_CELL }
                , {"End Date" , END_DATE_CELL}, {"CPM", CPM_CELL} , {"Demo", DEMO_CELL}, {"Contracted Daypart", CONTRACTED_DAYPART_CELL }
                , {"Share Book", SHARE_BOOK_CELL }, {"Playback type", PLAYBACK_TYPE_CELL } };

            requiredProperties
                .AsEnumerable()
                .Where(x => string.IsNullOrWhiteSpace(worksheet.Cells[x.Value.ToString()].GetStringValue()))
                .ForEach(x =>
                {
                    var errorMessage = $"Required value for {x.Key} is missing";
                    validationProblems.Add(errorMessage);
                    worksheet.Cells[$"{HEADER_ERROR_COLUMN}{x.Value.RowIndex}"].Value = errorMessage;
                });

            if (validationProblems.Any())
            {
                proprietaryFile.ValidationProblems.AddRange(validationProblems);
                return;
            }

            var daypartCode = worksheet.Cells[DAYPART_CODE_CELL.ToString()].GetStringValue();

            if (!DaypartCodeRepository.ActiveDaypartCodeExists(daypartCode))
            {
                var errorMessage = "Not acceptable daypart code is specified";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{DAYPART_CODE_CELL.RowIndex}"].Value = errorMessage;
            }
            else
            {
                header.DaypartCode = daypartCode;
            }

            //Format mm/dd/yyyy and end date must be after start date
            string effectiveDateText = worksheet.Cells[EFFECTIVE_DATE_CELL.ToString()].GetTextValue().Split(' ')[0]; //split is removing time section
            string endDateText = worksheet.Cells[END_DATE_CELL.ToString()].GetTextValue().Split(' ')[0];
            bool validDate = true;
            if (!DateTime.TryParseExact(effectiveDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime effectiveDate))
            {
                var errorMessage = $"Effective date is not in the correct format ({(string.Join(", ", DATE_FORMATS))})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{EFFECTIVE_DATE_CELL.RowIndex}"].Value = errorMessage;
                validDate = false;
            }
            if (!DateTime.TryParseExact(endDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                var errorMessage = $"End date is not in the correct format ({(string.Join(", ", DATE_FORMATS))})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{END_DATE_CELL.RowIndex}"].Value = errorMessage;
                validDate = false;
            }
            if (validDate && endDate <= effectiveDate)
            {
                var errorMessage = $"End date ({endDateText}) should be greater then effective date ({effectiveDateText})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{END_DATE_CELL.RowIndex}"].Value = errorMessage;
                validDate = false;
            }

            if (validDate)
            {
                header.EffectiveDate = effectiveDate;
                header.EndDate = endDate;
            }

            //Format ##.##, no dollar sign in data
            var r = new Regex(@"^\d+(\.\d)?\d*$");
            var cpm = worksheet.Cells[CPM_CELL.ToString()].GetStringValue();
            if (!r.IsMatch(cpm))
            {
                var errorMessage = $"CPM is not in the correct format ({CPM_FORMAT})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{CPM_CELL.RowIndex}"].Value = errorMessage;
            }
            else
            if (!Decimal.TryParse(cpm, out decimal cpmValue))
            {
                var errorMessage = $"Invalid value for CPM ({cpm})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{CPM_CELL.RowIndex}"].Value = errorMessage;
            }
            else
            {
                header.Cpm = cpmValue;
            }

            //Must be valid nelson demo.
            var demo = worksheet.Cells[DEMO_CELL.ToString()].GetStringValue();

            var audience = AudienceCache.GetBroadcastAudienceByCode(demo);
            if (audience == null)
            {
                var errorMessage = $"Invalid demo ({demo})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{DEMO_CELL.RowIndex}"].Value = errorMessage;
            }
            else
            {
                header.Audience = audience;
            }

            //Format: M-F 6:30PM-11PM and Standard Cadent Daypart rules
            string daypartString = worksheet.Cells[CONTRACTED_DAYPART_CELL.ToString()].GetStringValue();
            if (DaypartParsingEngine.TryParse(daypartString, out var displayDayparts))
            {
                if (displayDayparts.Count > 1)
                {
                    var errorMessage = $"Only one contracted daypart should be specified ({daypartString})";
                    validationProblems.Add(errorMessage);
                    worksheet.Cells[$"{HEADER_ERROR_COLUMN}{CONTRACTED_DAYPART_CELL.RowIndex}"].Value = errorMessage;
                }
                else
                {
                    header.ContractedDaypartId = displayDayparts.Single().Id;
                }
            }
            else
            {
                var errorMessage = $"Invalid contracted daypart ({daypartString})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{CONTRACTED_DAYPART_CELL.RowIndex}"].Value = errorMessage;
            }

            string shareBookText = worksheet.Cells[SHARE_BOOK_CELL.ToString()].GetTextValue();
            var shareBookParsedCorrectly = false;
            if (!DateTime.TryParseExact(shareBookText, BOOK_DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime shareBook))
            {
                var errorMessage = $"Share book ({shareBookText}) is not in the correct format ({(string.Join(", ", BOOK_DATE_FORMATS))})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{SHARE_BOOK_CELL.RowIndex}"].Value = errorMessage;
            }
            else
            {
                header.ShareBookId = MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(shareBook.Year, shareBook.Month).Id;
                shareBookParsedCorrectly = true;
            }

            //formats MMM yy, MMM-yy, MMM/yy, yy-MMM, yy/MMM 
            //Hut book must be a media month prior to the Share book media month if value entered
            string hutBookText = worksheet.Cells[HUT_BOOK_CELL.ToString()].GetTextValue();
            if (!string.IsNullOrWhiteSpace(hutBookText))
            {
                if (!DateTime.TryParseExact(hutBookText, BOOK_DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime hutBook))
                {
                    var errorMessage = $"Hut book ({hutBookText}) is not in the correct format ({(string.Join(", ", BOOK_DATE_FORMATS))})";
                    validationProblems.Add(errorMessage);
                    worksheet.Cells[$"{HEADER_ERROR_COLUMN}{HUT_BOOK_CELL.RowIndex}"].Value = errorMessage;
                }
                else if (shareBookParsedCorrectly && hutBook >= shareBook)
                {
                    var errorMessage = "HUT Book must be prior to the Share book";
                    validationProblems.Add(errorMessage);
                    worksheet.Cells[$"{HEADER_ERROR_COLUMN}{HUT_BOOK_CELL.RowIndex}"].Value = errorMessage;
                }
                else
                {
                    header.HutBookId = MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(hutBook.Year, hutBook.Month).Id;
                }
            }

            var playbackString = worksheet.Cells[PLAYBACK_TYPE_CELL.ToString()].GetStringValue().RemoveWhiteSpaces();
            ProposalEnums.ProposalPlaybackType playback = EnumHelper.GetEnumValueFromDescription<ProposalEnums.ProposalPlaybackType>(playbackString);
            if (playback == 0)
            {
                var errorMessage = $"Invalid playback type ({playbackString})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{PLAYBACK_TYPE_CELL.RowIndex}"].Value = errorMessage;
            }
            else
            {
                header.PlaybackType = playback;
            }

            proprietaryFile.Header = header;
            proprietaryFile.ValidationProblems.AddRange(validationProblems);
        }

        public override void LoadAndValidateDataLines(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            const int firstColumnIndex = 1;
            const int firstDataLineRowIndex = 18;
            var rowIndex = firstDataLineRowIndex;
            var columnIndex = firstColumnIndex;
            var lineProblems = new List<string>();

            var commentsColumnIndex = _GetCommentsColumnIndex(worksheet, commentsHeader, firstUnitColumnIndex, spotLengthRowIndex);

            _ErrorColumnIndex = commentsColumnIndex + 2;    //errors column is the second empty column after comments column

            var units = _ReadBarterInventoryUnits(worksheet, commentsColumnIndex, lineProblems);
            if (units == null)
            {
                proprietaryFile.ValidationProblems.AddRange(lineProblems);
                worksheet.Cells[unitNameRowIndex, _ErrorColumnIndex].Value = string.Join(FILE_MULTIPLE_ERRORS_SEPARATOR, lineProblems);
                return;
            }
            while (true)
            {
                List<string> validationProblems = new List<string>();
                // don`t simplify object initialization because line columns should be read with the current order 
                var line = new ProprietaryInventoryDataLine();
                line.Station = worksheet.Cells[rowIndex, columnIndex++].GetStringValue();
                var daypartText = worksheet.Cells[rowIndex, columnIndex++].GetStringValue();

                if (DaypartParsingEngine.TryParse(daypartText, out var dayparts))
                {
                    line.Dayparts = dayparts;
                }

                foreach (var unit in units)
                {
                    string spots = worksheet.Cells[rowIndex, columnIndex].GetStringValue();
                    if (!string.IsNullOrWhiteSpace(spots))
                    {
                        var lineUnit = new ProprietaryInventoryDataLine.Unit
                        {
                            ProprietaryInventoryUnit = unit,
                            Spots = worksheet.Cells[rowIndex, columnIndex].GetIntValue()
                        };
                        line.Units.Add(lineUnit);
                        if (lineUnit.Spots == null)
                        {
                            validationProblems.Add($"Line {rowIndex} contains an invalid number of spots in column {columnIndex.GetColumnAdress()}");
                        }
                    }
                    
                    columnIndex++;
                }

                line.Comment = worksheet.Cells[rowIndex, columnIndex].GetStringValue();

                if (_IsLineEmpty(line))
                {
                    break;
                }
                                
                if (string.IsNullOrWhiteSpace(line.Station))
                {
                    validationProblems.Add($"Line {rowIndex} contains an empty station cell");
                }

                if (line.Dayparts == null)
                {
                    var message = string.IsNullOrWhiteSpace(daypartText) ?
                       $"Line {rowIndex} contains an empty daypart cell" :
                       $"Line {rowIndex} contains an invalid daypart(s): {daypartText}";

                    validationProblems.Add(message);
                }
                
                if (validationProblems.Any())
                {
                    proprietaryFile.ValidationProblems.AddRange(validationProblems);
                    worksheet.Cells[rowIndex, _ErrorColumnIndex].Value = string.Join(FILE_MULTIPLE_ERRORS_SEPARATOR, validationProblems);
                }
                else
                {
                    proprietaryFile.DataLines.Add(line);
                }

                columnIndex = firstColumnIndex;
                rowIndex++;
            }
        }

        private List<ProprietaryInventoryUnit> _ReadBarterInventoryUnits(ExcelWorksheet worksheet, int commentsColumnIndex, List<string> lineProblems)
        {
            var result = new List<ProprietaryInventoryUnit>();

            for (var i = firstUnitColumnIndex; i < commentsColumnIndex; i++)
            {
                var name = worksheet.Cells[unitNameRowIndex, i].GetStringValue();
                if (string.IsNullOrWhiteSpace(name))
                {
                    lineProblems.Add($"Unit name missing in column {i.GetColumnAdress()}");
                    return null;
                }
                Regex r = new Regex("^[a-zA-Z 0-9]*$");
                name = name.Trim();

                if (!r.IsMatch(name))
                {
                    lineProblems.Add($"Invalid unit was found in column {i.GetColumnAdress()}");
                }

                var spotLengthString = worksheet.Cells[spotLengthRowIndex, i].GetStringValue();
                if (string.IsNullOrWhiteSpace(spotLengthString))
                {
                    lineProblems.Add($"Spot length is missing in column {i.GetColumnAdress()}");
                    return null;
                }

                spotLengthString = spotLengthString.Replace(":", string.Empty);
                if (!int.TryParse(spotLengthString, out var spotLength) || !SpotLengthEngine.SpotLengthExists(spotLength))
                {
                    lineProblems.Add($"Invalid spot length was found in column {i.GetColumnAdress()}");
                }
                if (lineProblems.Any()) return null;

                result.Add(new ProprietaryInventoryUnit
                {
                    Name = name,
                    SpotLength = spotLength
                });
            }

            return result;
        }

        private int _GetCommentsColumnIndex(ExcelWorksheet worksheet, string commentsHeader, int firstUnitColumnIndex, int spotLengthRowIndex)
        {
            int lastColumnIndex = firstUnitColumnIndex;
            // let's find lastColumnIndex by looking for "COMMENTS" cell
            while (true)
            {
                try
                {
                    // comments header cell should be on the same row with spot lengths and 1 cell after last unit column
                    var commentsHeaderCell = worksheet.Cells[spotLengthRowIndex, lastColumnIndex].GetStringValue();
                    var isCommentsHeaderCell = !string.IsNullOrWhiteSpace(commentsHeaderCell) && commentsHeaderCell.Equals(commentsHeader, StringComparison.OrdinalIgnoreCase);

                    if (isCommentsHeaderCell)
                    {
                        break;
                    }

                    lastColumnIndex++;
                }
                catch (Exception)
                {
                    throw new Exception("Couldn't find last unit column");
                }
            }
            return lastColumnIndex;
        }

        private bool _IsLineEmpty(ProprietaryInventoryDataLine line)
        {
            return string.IsNullOrWhiteSpace(line.Station) &&
                string.IsNullOrWhiteSpace(line.Comment) &&
                line.Dayparts == null &&
                line.Units.All(x => !x.Spots.HasValue);
        }

        public override void PopulateManifests(ProprietaryInventoryFile proprietaryFile, List<DisplayBroadcastStation> stations)
        {
            proprietaryFile.InventoryGroups = _GetStationInventoryGroups(proprietaryFile, stations);
        }

        private List<StationInventoryGroup> _GetStationInventoryGroups(ProprietaryInventoryFile proprietaryFile, List<DisplayBroadcastStation> stations)
        {
            var fileHeader = proprietaryFile.Header;
            var stationsDict = stations.ToDictionary(x => x.LegacyCallLetters, x => x, StringComparer.OrdinalIgnoreCase);
            return proprietaryFile.DataLines
                .SelectMany(x => x.Units, (dataLine, unit) => new
                {
                    dataLine,
                    unit.ProprietaryInventoryUnit,
                    unit.Spots
                })
                .GroupBy(x => new { x.ProprietaryInventoryUnit.Name, x.ProprietaryInventoryUnit.SpotLength })
                .Select(groupingByUnit => new
                {
                    UnitName = groupingByUnit.Key.Name,
                    groupingByUnit.Key.SpotLength,
                    Manifests = groupingByUnit.Select(x => new
                    {
                        x.Spots,
                        x.dataLine.Station,
                        x.dataLine.Dayparts,
                        x.dataLine.Comment
                    })
                })
                .Where(x => x.Manifests.Any(y => y.Spots != null))  // exclude empty manifest groups
                .Select(manifestGroup => new StationInventoryGroup
                {
                    Name = manifestGroup.UnitName,
                    InventorySource = proprietaryFile.InventorySource,
                    SlotNumber = _ParseSlotNumber(manifestGroup.UnitName),
                    Manifests = manifestGroup.Manifests
                        .Where(x => x.Spots != null) // exclude empty manifests
                        .Select(manifest => new StationInventoryManifest
                        {
                            InventorySourceId = proprietaryFile.InventorySource.Id,
                            InventoryFileId = proprietaryFile.Id,
                            Station = stationsDict[StationProcessingEngine.StripStationSuffix(manifest.Station)],
                            SpotLengthId = SpotLengthEngine.GetSpotLengthIdByValue(manifestGroup.SpotLength),
                            Comment = manifest.Comment,
                            ManifestWeeks = GetManifestWeeksInRange(fileHeader.EffectiveDate, fileHeader.EndDate, manifest.Spots.Value),
                            ManifestDayparts = manifest.Dayparts.Select(x => new StationInventoryManifestDaypart { Daypart = x }).ToList(),
                            ManifestAudiences = new List<StationInventoryManifestAudience>
                            {
                                new StationInventoryManifestAudience
                                {
                                    Audience = fileHeader.Audience.ToDisplayAudience(),
                                    CPM = fileHeader.Cpm.Value,
                                    IsReference = true
                                }
                            }
                        }).ToList()
                }).ToList();
        }

        private int _ParseSlotNumber(string unitName)
        {
            var slotNumberString = Regex.Match(unitName, @"[0-9]+", RegexOptions.IgnoreCase).Value;
            return int.TryParse(slotNumberString, out var slotNumber) ? slotNumber : 0;
        }
    }
}
