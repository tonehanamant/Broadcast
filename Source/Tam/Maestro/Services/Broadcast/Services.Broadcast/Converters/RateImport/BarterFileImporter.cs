using Common.Services.Repositories;
using Microsoft.Practices.ObjectBuilder2;
using OfficeOpenXml;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
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
        private const string INVENTORY_SOURCE_CELL = "B3";
        private const string DAYPART_CODE_CELL = "B4";
        private const string EFFECTIVE_DATE_CELL = "B5";
        private const string END_DATE_CELL = "B6";
        private const string CPM_CELL = "B7";
        private const string DEMO_CELL = "B8";
        private const string CONTRACTED_DAYPART_CELL = "B9";
        private const string SHARE_BOOK_CELL = "B10";
        private const string HUT_BOOK_CELL = "B11";
        private const string PLAYBACK_TYPE_CELL = "B12";

        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        private readonly IImpressionsService _ImpressionsService;

        public BarterFileImporter(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            IImpressionsService impressionsService) : base(
                broadcastDataRepositoryFactory, 
                broadcastAudiencesCache, 
                inventoryDaypartParsingEngine, 
                mediaMonthAndWeekAggregateCache,
                stationProcessingEngine,
                spotLengthEngine)
        {
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _ImpressionsService = impressionsService;
        }

        protected override void LoadAndValidateHeaderData(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            var header = new ProprietaryInventoryHeader();
            var validationProblems = new List<string>();
            Dictionary<string, string> requiredProperties = new Dictionary<string, string>
                { {"Inv Source", INVENTORY_SOURCE_CELL }, {"Daypart Code",DAYPART_CODE_CELL }, {"Effective Date", EFFECTIVE_DATE_CELL }
                , {"End Date" , END_DATE_CELL}, {"CPM", CPM_CELL} , {"Demo", DEMO_CELL }, {"Contracted Daypart", CONTRACTED_DAYPART_CELL }
                , {"Share Book", SHARE_BOOK_CELL }, {"Playback type", PLAYBACK_TYPE_CELL } };

            requiredProperties
                .AsEnumerable()
                .Where(x => string.IsNullOrWhiteSpace(worksheet.Cells[x.Value].GetStringValue()))
                .ForEach(x => validationProblems.Add($"Required value for {x.Key} is missing"));

            if (validationProblems.Any())
            {
                proprietaryFile.ValidationProblems.AddRange(validationProblems);
                return;
            }

            var daypartCode = worksheet.Cells[DAYPART_CODE_CELL].GetStringValue();

            if (!DaypartCodeRepository.ActiveDaypartCodeExists(daypartCode))
            {
                validationProblems.Add("Not acceptable daypart code is specified");
            }
            else
            {
                header.DaypartCode = daypartCode;
            }

            //Format mm/dd/yyyy and end date must be after start date
            string effectiveDateText = worksheet.Cells[EFFECTIVE_DATE_CELL].GetTextValue().Split(' ')[0]; //split is removing time section
            string endDateText = worksheet.Cells[END_DATE_CELL].GetTextValue().Split(' ')[0];
            bool validDate = true;
            if (!DateTime.TryParseExact(effectiveDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime effectiveDate))
            {
                validationProblems.Add($"Effective date is not in the correct format ({(string.Join(", ", DATE_FORMATS))})");
                validDate = false;
            }
            if (!DateTime.TryParseExact(endDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                validationProblems.Add($"End date is not in the correct format ({(string.Join(", ", DATE_FORMATS))})");
                validDate = false;
            }
            if (validDate && endDate <= effectiveDate)
            {
                validationProblems.Add($"End date ({endDateText}) should be greater then effective date ({effectiveDateText})");
                validDate = false;
            }

            if (validDate)
            {
                header.EffectiveDate = effectiveDate;
                header.EndDate = endDate;
            }

            //Format ##.##, no dollar sign in data
            var r = new Regex(@"^\d+(\.\d)?\d*$");
            var cpm = worksheet.Cells[CPM_CELL].GetStringValue();
            if (!r.IsMatch(cpm))
            {
                validationProblems.Add($"CPM is not in the correct format ({CPM_FORMAT})");
            }
            else
            if (!Decimal.TryParse(cpm, out decimal cpmValue))
            {
                validationProblems.Add($"Invalid value for CPM ({cpm})");
            }
            else
            {
                header.Cpm = cpmValue;
            }

            //Must be valid nelson demo.
            var demo = worksheet.Cells[DEMO_CELL].GetStringValue();
            if (!AudienceCache.IsValidAudienceCode(demo))
            {
                validationProblems.Add($"Invalid demo ({demo})");
            }
            else
            {
                header.Audience = AudienceCache.GetBroadcastAudienceByCode(demo);
            }

            //Format: M-F 6:30PM-11PM and Standard Cadent Daypart rules
            string daypartString = worksheet.Cells[CONTRACTED_DAYPART_CELL].GetStringValue();
            if (DaypartParsingEngine.TryParse(daypartString, out var displayDayparts))
            {
                if (displayDayparts.Count > 1)
                {
                    validationProblems.Add($"Only one contracted daypart should be specified ({daypartString})");
                }
                else
                {
                    header.ContractedDaypartId = displayDayparts.Single().Id;
                }
            }
            else
            {
                validationProblems.Add($"Invalid contracted daypart ({daypartString})");
            }

            string shareBookText = worksheet.Cells[SHARE_BOOK_CELL].GetTextValue();
            var shareBookParsedCorrectly = false;
            if (!DateTime.TryParseExact(shareBookText, BOOK_DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime shareBook))
            {
                validationProblems.Add($"Share book ({shareBookText}) is not in the correct format ({(string.Join(", ", BOOK_DATE_FORMATS))})");
            }
            else
            {
                header.ShareBookId = MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(shareBook.Year, shareBook.Month).Id;
                shareBookParsedCorrectly = true;
            }

            //formats MMM yy, MMM-yy, MMM/yy, yy-MMM, yy/MMM 
            //Hut book must be a media month prior to the Share book media month if value entered
            string hutBookText = worksheet.Cells[HUT_BOOK_CELL].GetTextValue();
            if (!string.IsNullOrWhiteSpace(hutBookText))
            {
                if (!DateTime.TryParseExact(hutBookText, BOOK_DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime hutBook))
                {
                    validationProblems.Add($"Hut book ({hutBookText}) is not in the correct format ({(string.Join(", ", BOOK_DATE_FORMATS))})");
                }
                else if (shareBookParsedCorrectly && hutBook >= shareBook)
                {
                    validationProblems.Add("HUT Book must be prior to the Share book");
                }
                else
                {
                    header.HutBookId = MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(hutBook.Year, hutBook.Month).Id;
                }
            }

            var playbackString = worksheet.Cells[PLAYBACK_TYPE_CELL].GetStringValue().RemoveWhiteSpaces();
            ProposalEnums.ProposalPlaybackType playback = EnumHelper.GetEnumValueFromDescription<ProposalEnums.ProposalPlaybackType>(playbackString);
            if (playback == 0)
            {
                validationProblems.Add($"Invalid playback type ({playbackString})");
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
            var units = _ReadBarterInventoryUnits(worksheet);

            while (true)
            {
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
                    line.Units.Add(new ProprietaryInventoryDataLine.Unit
                    {
                        ProprietaryInventoryUnit = unit,
                        Spots = worksheet.Cells[rowIndex, columnIndex++].GetIntValue()
                    });
                }

                line.Comment = worksheet.Cells[rowIndex, columnIndex].GetStringValue();

                if (_IsLineEmpty(line))
                {
                    break;
                }

                var hasValidationProblems = false;

                if (string.IsNullOrWhiteSpace(line.Station))
                {
                    proprietaryFile.ValidationProblems.Add($"Line {rowIndex} contains an empty station cell");
                    hasValidationProblems = true;
                }

                if (line.Dayparts == null)
                {
                    var message = string.IsNullOrWhiteSpace(daypartText) ?
                       $"Line {rowIndex} contains an empty daypart cell" :
                       $"Line {rowIndex} contains an invalid daypart(s): {daypartText}";

                    proprietaryFile.ValidationProblems.Add(message);
                    hasValidationProblems = true;
                }

                if (!hasValidationProblems)
                {
                    proprietaryFile.DataLines.Add(line);
                }

                columnIndex = firstColumnIndex;
                rowIndex++;
            }
        }

        private List<ProprietaryInventoryUnit> _ReadBarterInventoryUnits(ExcelWorksheet worksheet)
        {
            const string commentsHeader = "COMMENTS";
            const int unitNameRowIndex = 16;
            const int spotLengthRowIndex = 17;
            const int firstUnitColumnIndex = 3;
            var result = new List<ProprietaryInventoryUnit>();
            var lastColumnIndex = firstUnitColumnIndex;

            // let's find lastColumnIndex by looking for "COMMENTS" cell
            while (true)
            {
                try
                {
                    // comments header cell should be on the same row with spot lengths and 1 cell after last unit column
                    var commentsHeaderCell = worksheet.Cells[spotLengthRowIndex, lastColumnIndex + 1].GetStringValue();
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

            for (var i = firstUnitColumnIndex; i <= lastColumnIndex; i++)
            {
                var name = worksheet.Cells[unitNameRowIndex, i].GetStringValue();
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new Exception("Unit name missing");
                }
                Regex r = new Regex("^[a-zA-Z 0-9]*$");
                name = name.Trim();

                if (!r.IsMatch(name))
                {
                    throw new Exception("Invalid unit was found");
                }

                var spotLengthString = worksheet.Cells[spotLengthRowIndex, i].GetStringValue(); 
                
                if (string.IsNullOrWhiteSpace(spotLengthString))
                {
                    throw new Exception("Spot length is missing");
                }

                spotLengthString = spotLengthString.Replace(":", string.Empty);

                if (!int.TryParse(spotLengthString, out var spotLength) || !SpotLengthEngine.SpotLengthExists(spotLength))
                {
                    throw new Exception("Invalid spot length was found");
                }

                result.Add(new ProprietaryInventoryUnit
                {
                    Name = name,
                    SpotLength = spotLength
                });
            }

            return result;
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
                    DaypartCode = Regex.Match(manifestGroup.UnitName, @"[a-z]+", RegexOptions.IgnoreCase).Value,
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
                                    CPM = fileHeader.Cpm.Value
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
