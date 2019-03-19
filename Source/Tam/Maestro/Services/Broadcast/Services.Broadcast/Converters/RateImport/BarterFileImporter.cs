using Common.Services.Repositories;
using Microsoft.Practices.ObjectBuilder2;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using static Services.Broadcast.Entities.BarterInventory.BarterInventoryFile;

namespace Services.Broadcast.Converters.RateImport
{
    public class BarterFileImporter : BarterFileImporterBase
    {
        private const string INVENTORY_SOURCE_CELL = "B2";
        private const string DAYPART_CODE_CELL = "B3";
        private const string EFFECTIVE_DATE_CELL = "B4";
        private const string END_DATE_CELL = "B5";
        private const string CPM_CELL = "B6";
        private const string DEMO_CELL = "B7";
        private const string CONTRACTED_DAYPART_CELL = "B8";
        private const string SHARE_BOOK_CELL = "B9";
        private const string HUT_BOOK_CELL = "B10";
        private const string PLAYBACK_TYPE_CELL = "B11";     
        readonly string[] DATE_FORMATS = new string[3] { "MM/dd/yyyy", "M/dd/yyyy", "M/d/yyyy" };

        public BarterFileImporter(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine) : base(
                broadcastDataRepositoryFactory, 
                broadcastAudiencesCache, 
                inventoryDaypartParsingEngine, 
                mediaMonthAndWeekAggregateCache,
                stationProcessingEngine,
                spotLengthEngine)
        {
        }

        protected override void LoadAndValidateHeaderData(ExcelWorksheet worksheet, BarterInventoryFile barterFile)
        {
            var header = new BarterInventoryHeader();
            var validationProblems = new List<string>();
            Dictionary<string, string> requiredProperties = new Dictionary<string, string>
                { {"Inv Source", INVENTORY_SOURCE_CELL }, {"Daypart Code",DAYPART_CODE_CELL }, {"Effective Date", EFFECTIVE_DATE_CELL }
                , {"End Date" , END_DATE_CELL}, {"CPM", CPM_CELL} , {"Demo", DEMO_CELL }, {"Contracted Daypart", CONTRACTED_DAYPART_CELL }
                , {"Share Book", SHARE_BOOK_CELL }, {"Playback type", PLAYBACK_TYPE_CELL } };

            requiredProperties
                .AsEnumerable()
                .Where(x => string.IsNullOrWhiteSpace(worksheet.Cells[x.Value].GetStringValue()))
                .ForEach(x => validationProblems.Add($"Required value for {x.Key} is missing"));

            if (validationProblems.Any()) return;

            header.DaypartCode = worksheet.Cells[DAYPART_CODE_CELL].GetStringValue();

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
                header.AudienceId = AudienceCache.GetDisplayAudienceByCode(demo).Id;
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

            barterFile.Header = header;
            barterFile.ValidationProblems.AddRange(validationProblems);
        }

        public override void LoadAndValidateDataLines(ExcelWorksheet worksheet, BarterInventoryFile barterFile)
        {
            const int firstColumnIndex = 2;
            var rawIndex = 16;
            var columnIndex = firstColumnIndex;
            var units = _ReadBarterInventoryUnits(worksheet);

            while (true)
            {
                // don`t simplify object initialization because line columns should be read with the current order 
                var line = new BarterInventoryDataLine();
                line.Station = worksheet.Cells[rawIndex, columnIndex++].GetStringValue();
                var daypartText = worksheet.Cells[rawIndex, columnIndex++].GetStringValue();

                if (DaypartParsingEngine.TryParse(daypartText, out var dayparts))
                {
                    line.Dayparts = dayparts;
                }

                foreach (var unit in units)
                {
                    line.Units.Add(new BarterInventoryDataLine.Unit
                    {
                        BarterInventoryUnit = unit,
                        Spots = worksheet.Cells[rawIndex, columnIndex++].GetIntValue()
                    });
                }

                line.Comment = worksheet.Cells[rawIndex, columnIndex].GetStringValue();

                if (_IsLineEmpty(line))
                {
                    break;
                }

                var hasValidationProblems = false;

                if (string.IsNullOrWhiteSpace(line.Station))
                {
                    barterFile.ValidationProblems.Add($"Line {rawIndex} contains an empty station cell");
                    hasValidationProblems = true;
                }

                if (line.Dayparts == null)
                {
                    var message = string.IsNullOrWhiteSpace(daypartText) ?
                       $"Line {rawIndex} contains an empty daypart cell" :
                       $"Line {rawIndex} contains an invalid daypart(s): {daypartText}";

                    barterFile.ValidationProblems.Add(message);
                    hasValidationProblems = true;
                }

                if (!hasValidationProblems)
                {
                    barterFile.DataLines.Add(line);
                }

                columnIndex = firstColumnIndex;
                rawIndex++;
            }
        }

        private List<BarterInventoryUnit> _ReadBarterInventoryUnits(ExcelWorksheet worksheet)
        {
            const string commentsHeader = "COMMENTS";
            const int nameRowIndex = 14;
            const int spotLengthRowIndex = 15;
            const int firstColumnIndex = 4;
            var result = new List<BarterInventoryUnit>();
            var lastColumnIndex = firstColumnIndex;

            // let's find lastColumnIndex by looking for "COMMENTS" cell
            while (true)
            {
                try
                {
                    // comments header cell should be on the same row with spot lengths and 1 cell after last unit column
                    var commentsHeaderCell = worksheet.Cells[spotLengthRowIndex, lastColumnIndex + 1].GetStringValue();
                    var isCommentsHeaderCell = !string.IsNullOrWhiteSpace(commentsHeaderCell) && commentsHeaderCell.Equals(commentsHeader);
                    
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

            for (var i = firstColumnIndex; i <= lastColumnIndex; i++)
            {
                var name = worksheet.Cells[nameRowIndex, i].GetStringValue();
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

                var spotLength = worksheet.Cells[spotLengthRowIndex, i].GetIntValue(); 
                
                if (!spotLength.HasValue)
                {
                    throw new Exception("Spot length is missing");
                }

                if (!SpotLengthEngine.SpotLengthExists(spotLength.Value))
                {
                    throw new Exception("Invalid spot length was found");
                }

                result.Add(new BarterInventoryUnit
                {
                    Name = name,
                    SpotLength = spotLength.Value
                });
            }

            return result;
        }

        private bool _IsLineEmpty(BarterInventoryDataLine line)
        {
            return string.IsNullOrWhiteSpace(line.Station) &&
                string.IsNullOrWhiteSpace(line.Comment) &&
                line.Dayparts == null &&
                line.Units.All(x => !x.Spots.HasValue);
        }

        public override void PopulateManifests(BarterInventoryFile barterFile, List<DisplayBroadcastStation> stations)
        {
            barterFile.InventoryGroups = _GetStationInventoryGroups(barterFile, stations);
        }

        private List<StationInventoryGroup> _GetStationInventoryGroups(BarterInventoryFile barterFile, List<DisplayBroadcastStation> stations)
        {
            var fileHeader = barterFile.Header;
            var stationsDict = stations.ToDictionary(x => x.LegacyCallLetters, x => x, StringComparer.OrdinalIgnoreCase);
            return barterFile.DataLines
                .SelectMany(x => x.Units, (dataLine, unit) => new
                {
                    dataLine,
                    unit.BarterInventoryUnit,
                    unit.Spots
                })
                .GroupBy(x => new { x.BarterInventoryUnit.Name, x.BarterInventoryUnit.SpotLength })
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
                .Where(x => x.Manifests.Any(y => y.Spots != null))  //exclude empty manifest groups
                .Select(manifestGroup => new StationInventoryGroup
                {
                    Name = manifestGroup.UnitName,
                    DaypartCode = Regex.Match(manifestGroup.UnitName, @"[a-z]+", RegexOptions.IgnoreCase).Value,
                    InventorySource = barterFile.InventorySource,
                    StartDate = fileHeader.EffectiveDate,
                    EndDate = fileHeader.EndDate,
                    SlotNumber = _ParseSlotNumber(manifestGroup.UnitName),
                    Manifests = manifestGroup.Manifests
                        .Where(x => x.Spots != null) //exclude empty manifests
                        .Select(manifest => new StationInventoryManifest
                        {
                            EffectiveDate = fileHeader.EffectiveDate,
                            EndDate = fileHeader.EndDate,
                            InventorySourceId = barterFile.InventorySource.Id,
                            FileId = barterFile.Id,
                            Station = stationsDict[StationProcessingEngine.StripStationSuffix(manifest.Station)],
                            SpotLengthId = SpotLengthEngine.GetSpotLengthIdByValue(manifestGroup.SpotLength),
                            Comment = manifest.Comment,
                            ManifestWeeks = _GetManifestWeeksInRange(fileHeader.EffectiveDate, fileHeader.EndDate, manifest.Spots.Value),
                            ManifestDayparts = manifest.Dayparts.Select(x => new StationInventoryManifestDaypart { Daypart = x }).ToList(),
                            ManifestAudiences = new List<StationInventoryManifestAudience>
                        {
                            new StationInventoryManifestAudience
                            {
                                Audience = new DisplayAudience { Id = fileHeader.AudienceId.Value },
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

        private List<StationInventoryManifestWeek> _GetManifestWeeksInRange(DateTime startDate, DateTime endDate, int spots)
        {
            var mediaWeeks = MediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(startDate, endDate);
            return mediaWeeks.Select(x => new StationInventoryManifestWeek { MediaWeek = x, Spots = spots }).ToList();
        }
    }
}
