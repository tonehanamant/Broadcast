using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;

namespace Services.Broadcast.Entities.Campaign
{
    public class ProgramLineupReportData
    {
        public string ExportFileName { get; set; }
        public string PlanHeaderName { get; set; }
        public string ReportGeneratedDate { get; set; }
        public string AccuracyEstimateDate { get; set; }
        public string Agency { get; set; }
        public string Client { get; set; }
        public string FlightStartDate { get; set; }
        public string FlightEndDate { get; set; }
        public string GuaranteedDemo { get; set; }
        public string SpotLengths { get; set; }
        public string PostingType { get; set; }
        public string SpotAllocationModelMode { get; set; }
        public string AccountExecutive { get; set; }
        public string ClientContact { get; set; }
        public List<DetailedViewRowDisplay> DetailedViewRows { get; set; }
        public List<DefaultViewRowDisplay> DefaultViewRows { get; set; }
        public List<AllocationViewRowDisplay> AllocationByDaypartViewRows { get; set; }
        public List<AllocationViewRowDisplay> AllocationByGenreViewRows { get; set; }
        public List<AllocationViewRowDisplay> AllocationByDMAViewRows { get; set; }
        public List<AllocationViewRowDisplay> AllocationBySpotLengthViewRows { get; set; }
        public List<AllocationViewRowDisplay> AllocationByAffiliateViewRows { get; set; }

        private const string FILENAME_FORMAT = "Program_Lineup_Report_{0}_{1}_{2}_{3}.xlsx";
        private const string FILENAME_FORMAT_BUYING = "Program_Lineup_Report_B_{0}_{1}_{2}_{3}.xlsx";
        private const string PLAN_HEADER_NAME_FORMAT = "{0} | Program Lineup*";
        private const string DATE_FORMAT_FILENAME = "MMddyyyy";
        private const string DATE_FORMAT_SHORT_YEAR_SLASHES = "MM/dd/yy";
        private const string DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT = "M/d/yy";


        internal ProgramLineupReportData()
        {
        }

        public ProgramLineupReportData(
            PlanDto plan,
            PlanPricingJob planPricingJob,
            AgencyDto agency,
            AdvertiserDto advertiser,
            PlanAudienceDisplay guaranteedDemo,
            List<LookupDto> spotLengths,
            DateTime currentDate,
            List<PlanPricingAllocatedSpot> allocatedSpots,
            List<StationInventoryManifest> manifests,
            MarketCoverageByStation marketCoverageByStation,
            Dictionary<int, BasePlanInventoryProgram.ManifestDaypart.Program> primaryProgramsByManifestDaypartIds,
            List<ProgramLineupProprietaryInventory> proprietaryInventory,
            PostingTypeEnum postingType,
            SpotAllocationModelMode spotAllocationModelMode, bool isProgramLineupAllocationByAffiliateEnabled)

        {
            ExportFileName = _GetFileName(plan.Name, postingType, spotAllocationModelMode, currentDate);
            var marketCoverageByMarketCode = marketCoverageByStation.Markets.ToDictionary(x => x.MarketCode, x => x);

            _PopulateHeaderData(plan, planPricingJob, agency, advertiser, guaranteedDemo, spotLengths, currentDate, postingType, spotAllocationModelMode);

            List<DetailedViewRowData> detailedRowsData = _GetDetailedViewRowDataForOpenMarket(
                                                        plan,
                                                        allocatedSpots,
                                                        manifests,
                                                        marketCoverageByMarketCode,
                                                        primaryProgramsByManifestDaypartIds);
            detailedRowsData.AddRange(_GetDetailedViewRowDataForProprietary(proprietaryInventory, marketCoverageByMarketCode));

            double totalAllocatedImpressions = detailedRowsData.Sum(x => x.TotalSpotsImpressions);
            DetailedViewRows = _MapToDetailedViewRows(detailedRowsData);

            DefaultViewRows = _MapToDefaultViewRows(detailedRowsData, totalAllocatedImpressions);
            AllocationByDaypartViewRows = _MapToAllocationViewRows(detailedRowsData, totalAllocatedImpressions
                , x => x.DaypartCode
                , false);
            AllocationByGenreViewRows = _MapToAllocationViewRows(detailedRowsData, totalAllocatedImpressions
                , x => x.Genre
                , true);
            AllocationByDMAViewRows = _MapDMAToAllocationViewRows(detailedRowsData, totalAllocatedImpressions);

            AllocationBySpotLengthViewRows = _MapSpotLengthToAllocationViewRows(allocatedSpots, proprietaryInventory, totalAllocatedImpressions, spotLengths, plan.Equivalized);
            if (isProgramLineupAllocationByAffiliateEnabled)
            {
                AllocationByAffiliateViewRows = _MapAffiliateToAllocationViewRows(detailedRowsData, totalAllocatedImpressions);
            }
        }

        public ProgramLineupReportData(
            PlanDto plan,
            PlanBuyingJob planbuyingJob,
            AgencyDto agency,
            AdvertiserDto advertiser,
            PlanAudienceDisplay guaranteedDemo,
            List<LookupDto> spotLengths,
            DateTime currentDate,
            List<PlanBuyingAllocatedSpot> allocatedSpots,
            List<StationInventoryManifest> manifests,
            MarketCoverageByStation marketCoverageByStation,
            Dictionary<int, BasePlanInventoryProgram.ManifestDaypart.Program> primaryProgramsByManifestDaypartIds,
            List<ProgramLineupProprietaryInventory> proprietaryInventory,
            PostingTypeEnum postingType,
            SpotAllocationModelMode spotAllocationModelMode, bool isProgramLineupAllocationByAffiliateEnabled)

        {
            ExportFileName = _GetBuyingFileName(plan.Name, postingType, spotAllocationModelMode, currentDate);
            var marketCoverageByMarketCode = marketCoverageByStation.Markets.ToDictionary(x => x.MarketCode, x => x);

            _PopulateBuyingHeaderData(plan, planbuyingJob, agency, advertiser, guaranteedDemo, spotLengths, currentDate, postingType, spotAllocationModelMode);

            List<DetailedViewRowData> detailedRowsData = _GetDetailedViewRowDataForBuyingOpenMarket(
                                                        plan,
                                                        allocatedSpots,
                                                        manifests,
                                                        marketCoverageByMarketCode,
                                                        primaryProgramsByManifestDaypartIds);
            detailedRowsData.AddRange(_GetDetailedViewRowDataForProprietary(proprietaryInventory, marketCoverageByMarketCode));

            double totalAllocatedImpressions = detailedRowsData.Sum(x => x.TotalSpotsImpressions);
            DetailedViewRows = _MapToDetailedViewRows(detailedRowsData);

            DefaultViewRows = _MapToDefaultViewRows(detailedRowsData, totalAllocatedImpressions);
            AllocationByDaypartViewRows = _MapToAllocationViewRows(detailedRowsData, totalAllocatedImpressions
                , x => x.DaypartCode
                , false);
            AllocationByGenreViewRows = _MapToAllocationViewRows(detailedRowsData, totalAllocatedImpressions
                , x => x.Genre
                , true);
            AllocationByDMAViewRows = _MapDMAToAllocationViewRows(detailedRowsData, totalAllocatedImpressions);

            AllocationBySpotLengthViewRows = _MapSpotLengthToBuyingAllocationViewRows(allocatedSpots, proprietaryInventory, totalAllocatedImpressions, spotLengths, plan.Equivalized);
            if (isProgramLineupAllocationByAffiliateEnabled)
            {
                AllocationByAffiliateViewRows = _MapAffiliateToAllocationViewRows(detailedRowsData, totalAllocatedImpressions);
            }
        }
        internal string _GetFileName(string planName, PostingTypeEnum postingType, SpotAllocationModelMode spotAllocationModelMode, DateTime currentDate)
        {

            var rawFileName = string.Format(FILENAME_FORMAT, planName, postingType, spotAllocationModelMode.ToString().Substring(0, 1), currentDate.ToString(DATE_FORMAT_FILENAME));
            var fileName = rawFileName.PrepareForUsingInFileName();
            return fileName;
        }

        internal string _GetBuyingFileName(string planName, PostingTypeEnum postingType, SpotAllocationModelMode spotAllocationModelMode, DateTime currentDate)
        {

            var rawFileName = string.Format(FILENAME_FORMAT_BUYING, planName, postingType, spotAllocationModelMode.ToString().Substring(0, 1), currentDate.ToString(DATE_FORMAT_FILENAME));
            var fileName = rawFileName.PrepareForUsingInFileName();
            return fileName;
        }

        private IEnumerable<DetailedViewRowData> _GetDetailedViewRowDataForProprietary(
            List<ProgramLineupProprietaryInventory> proprietaryInventory
            , Dictionary<int, MarketCoverageByStation.Market> markets)
        {
            return proprietaryInventory.Select(x =>
            {
                return new DetailedViewRowData
                {
                    DaypartCode = x.Daypart.Code,
                    Dayparts = new List<DisplayDaypart> { x.Daypart },
                    Genre = x.Genre,
                    MarketGeographyName = markets[x.MarketCode].MarketName,
                    Rank = markets[x.MarketCode].Rank,
                    ProgramName = x.ProgramName,
                    StationAffiliation = x.Station.Affiliation,
                    StationLegacyCallLetters = x.Station.LegacyCallLetters,
                    TotalSpotsImpressions = x.TotalImpressions
                };
            }).ToList();
        }

        private List<AllocationViewRowDisplay> _MapSpotLengthToAllocationViewRows(List<PlanPricingAllocatedSpot> allocatedSpots
            , List<ProgramLineupProprietaryInventory> proprietaryInventory
            , double totalAllocatedImpressions, List<LookupDto> spotLengths, bool? equivalized)
        {
            Dictionary<int, double> data = new Dictionary<int, double>();
            _LoadImpressionsBySpotLengthForOpenMarket(allocatedSpots, data);
            _LoadImpressionsBySpotLengthForProprietary(proprietaryInventory, data);
            return data
                .OrderBy(x => int.Parse(spotLengths.Single(w => w.Id == x.Key).Display))
                .Select(x =>
                {
                    var spotLengthDisplay = spotLengths.Single(w => w.Id == x.Key).Display;
                    return new AllocationViewRowDisplay
                    {
                        FilterLabel = $":{spotLengthDisplay}{_GetEquivalizedStatus(equivalized, spotLengthDisplay)}",
                        Weight = _CalculateWeight(x.Value, totalAllocatedImpressions)
                    };
                })
                .ToList();
        }

        private List<AllocationViewRowDisplay> _MapSpotLengthToBuyingAllocationViewRows(List<PlanBuyingAllocatedSpot> allocatedSpots
            , List<ProgramLineupProprietaryInventory> proprietaryInventory
            , double totalAllocatedImpressions, List<LookupDto> spotLengths, bool? equivalized)
        {
            Dictionary<int, double> data = new Dictionary<int, double>();
            _LoadImpressionsBySpotLengthForBuyingOpenMarket(allocatedSpots, data);
            _LoadImpressionsBySpotLengthForProprietary(proprietaryInventory, data);
            return data
                .OrderBy(x => int.Parse(spotLengths.Single(w => w.Id == x.Key).Display))
                .Select(x =>
                {
                    var spotLengthDisplay = spotLengths.Single(w => w.Id == x.Key).Display;
                    return new AllocationViewRowDisplay
                    {
                        FilterLabel = $":{spotLengthDisplay}{_GetEquivalizedStatus(equivalized, spotLengthDisplay)}",
                        Weight = _CalculateWeight(x.Value, totalAllocatedImpressions)
                    };
                })
                .ToList();
        }

        private void _PopulateHeaderData(
            PlanDto plan,
            PlanPricingJob planPricingJob,
            AgencyDto agency,
            AdvertiserDto advertiser,
            PlanAudienceDisplay guaranteedDemo,
            List<LookupDto> spotLengths,
            DateTime currentDate,
            PostingTypeEnum postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            PlanHeaderName = string.Format(PLAN_HEADER_NAME_FORMAT, plan.Name);
            ReportGeneratedDate = currentDate.ToString(DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT);
            AccuracyEstimateDate = planPricingJob.Completed.Value.ToString(DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT);
            Agency = agency.Name;
            Client = advertiser.Name;
            FlightStartDate = plan.FlightStartDate.Value.ToString(DATE_FORMAT_SHORT_YEAR_SLASHES);
            FlightEndDate = plan.FlightEndDate.Value.ToString(DATE_FORMAT_SHORT_YEAR_SLASHES);
            GuaranteedDemo = guaranteedDemo.Code;
            SpotLengths = _GetSpotLengths(plan, spotLengths);
            PostingType = postingType.ToString();
            SpotAllocationModelMode = spotAllocationModelMode.ToString();
            AccountExecutive = string.Empty;
            ClientContact = string.Empty;
        }

        private void _PopulateBuyingHeaderData(
            PlanDto plan,
            PlanBuyingJob planBuyingJob,
            AgencyDto agency,
            AdvertiserDto advertiser,
            PlanAudienceDisplay guaranteedDemo,
            List<LookupDto> spotLengths,
            DateTime currentDate,
            PostingTypeEnum postingType,
            SpotAllocationModelMode spotAllocationModelMode)
        {
            PlanHeaderName = string.Format(PLAN_HEADER_NAME_FORMAT, plan.Name);
            ReportGeneratedDate = currentDate.ToString(DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT);
            AccuracyEstimateDate = planBuyingJob.Completed.Value.ToString(DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT);
            Agency = agency.Name;
            Client = advertiser.Name;
            FlightStartDate = plan.FlightStartDate.Value.ToString(DATE_FORMAT_SHORT_YEAR_SLASHES);
            FlightEndDate = plan.FlightEndDate.Value.ToString(DATE_FORMAT_SHORT_YEAR_SLASHES);
            GuaranteedDemo = guaranteedDemo.Code;
            SpotLengths = _GetSpotLengths(plan, spotLengths);
            PostingType = postingType.ToString();
            SpotAllocationModelMode = spotAllocationModelMode.ToString();
            AccountExecutive = string.Empty;
            ClientContact = string.Empty;
        }
        private List<DetailedViewRowData> _GetDetailedViewRowDataForOpenMarket(
            PlanDto plan,
            List<PlanPricingAllocatedSpot> allocatedSpots,
            List<StationInventoryManifest> manifests,
            Dictionary<int, MarketCoverageByStation.Market> markets,
            Dictionary<int, BasePlanInventoryProgram.ManifestDaypart.Program> primaryProgramsByManifestDaypartIds)
        {
            var dataRows = new List<DetailedViewRowData>();
            var planDaypartById = plan.Dayparts.ToDictionary(x => x.DaypartCodeId, x => x);

            // we expect all records belong to the same daypart because it`s OpenMarket
            // So we can just group by StationInventoryManifestId for now
            var spotsByManifest = allocatedSpots
                .GroupBy(x => x.StationInventoryManifestId)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var manifest in manifests)
            {
                if (!spotsByManifest.TryGetValue(manifest.Id.Value, out var allocatedSpotsForManifest))
                    continue;

                var coverage = markets[manifest.Station.MarketCode.Value];

                // OpenMarket manifest has only one daypart
                // This needs to be updated when we start passing other sources to pricing
                var standardDaypart = allocatedSpotsForManifest.First().StandardDaypart;
                var manifestDaypart = manifest.ManifestDayparts.Single();
                var planDaypart = planDaypartById[standardDaypart.Id];
                var ranges = DaypartTimeHelper.GetIntersectingTimeRangesWithAdjustment(
                    firstDaypart: new TimeRange { StartTime = manifestDaypart.Daypart.StartTime, EndTime = manifestDaypart.Daypart.EndTime },
                    secondDaypart: new TimeRange { StartTime = planDaypart.StartTimeSeconds, EndTime = planDaypart.EndTimeSeconds });
                var intersections = ranges
                    .Select(x =>
                    {
                        var intersection = (DisplayDaypart)manifestDaypart.Daypart.Clone();
                        intersection.StartTime = x.StartTime;
                        intersection.EndTime = x.EndTime;
                        return intersection;
                    })
                    .ToList();

                var row = new DetailedViewRowData
                {
                    Rank = coverage.Rank,
                    MarketGeographyName = coverage.MarketName,
                    StationLegacyCallLetters = manifest.Station.LegacyCallLetters,
                    StationAffiliation = manifest.Station.Affiliation,
                    Dayparts = intersections,
                    TotalSpotsImpressions = allocatedSpotsForManifest.Sum(x => x.TotalImpressions),

                    // they all should have the same code because only weeks differ. Appies only for OpenMarket
                    DaypartCode = standardDaypart.Code
                };

                if (primaryProgramsByManifestDaypartIds.TryGetValue(manifestDaypart.Id.Value, out var primaryProgram))
                {
                    row.ProgramName = primaryProgram.Name;
                    row.Genre = primaryProgram.Genre;
                }
                else
                {
                    // If no information from Dativa is available, use the program name from the inventory file
                    row.ProgramName = manifestDaypart.ProgramName ?? string.Empty;
                    row.Genre = string.Empty;
                }

                dataRows.Add(row);
            }

            // Handles the case when two manifests have the same station, market, etc but different weeks 
            // and we have spots allocated to both of them
            var result = dataRows
                .GroupBy(x => new
                {
                    x.Rank,
                    x.MarketGeographyName,
                    x.StationLegacyCallLetters,
                    x.StationAffiliation,
                    Day = x.Dayparts.First().ToDayString(),
                    Time = x.Dayparts.First().ToTimeString(),
                    Day2 = x.Dayparts.LastOrDefault()?.ToDayString(),
                    Time2 = x.Dayparts.LastOrDefault()?.ToTimeString(),
                    x.ProgramName,
                    x.Genre,
                    x.DaypartCode
                })
                .Select(grouping =>
                {
                    var first = grouping.First();
                    first.TotalSpotsImpressions = grouping.Sum(x => x.TotalSpotsImpressions);
                    return first;
                })
                .ToList();

            return result;
        }

        private List<DetailedViewRowData> _GetDetailedViewRowDataForBuyingOpenMarket(
           PlanDto plan,
           List<PlanBuyingAllocatedSpot> allocatedSpots,
           List<StationInventoryManifest> manifests,
           Dictionary<int, MarketCoverageByStation.Market> markets,
           Dictionary<int, BasePlanInventoryProgram.ManifestDaypart.Program> primaryProgramsByManifestDaypartIds)
        {
            var dataRows = new List<DetailedViewRowData>();            
            Dictionary<int, PlanDaypartDto> planDaypartById = new Dictionary<int, PlanDaypartDto>();           
             plan.Dayparts.GroupBy(x => x.DaypartCodeId).ToList().ForEach(x =>
            {
                planDaypartById.Add(x.First().DaypartCodeId, x.First());
            });
            // we expect all records belong to the same daypart because it`s OpenMarket
            // So we can just group by StationInventoryManifestId for now
            var spotsByManifest = allocatedSpots
                .GroupBy(x => x.StationInventoryManifestId)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var manifest in manifests)
            {
                if (!spotsByManifest.TryGetValue(manifest.Id.Value, out var allocatedSpotsForManifest))
                    continue;

                var coverage = markets[manifest.Station.MarketCode.Value];

                // OpenMarket manifest has only one daypart
                // This needs to be updated when we start passing other sources to pricing
                var standardDaypart = allocatedSpotsForManifest.First().StandardDaypart;
                var manifestDaypart = manifest.ManifestDayparts.Single();
                var planDaypart = planDaypartById[standardDaypart.Id];
                var ranges = DaypartTimeHelper.GetIntersectingTimeRangesWithAdjustment(
                    firstDaypart: new TimeRange { StartTime = manifestDaypart.Daypart.StartTime, EndTime = manifestDaypart.Daypart.EndTime },
                    secondDaypart: new TimeRange { StartTime = planDaypart.StartTimeSeconds, EndTime = planDaypart.EndTimeSeconds });
                var intersections = ranges
                    .Select(x =>
                    {
                        var intersection = (DisplayDaypart)manifestDaypart.Daypart.Clone();
                        intersection.StartTime = x.StartTime;
                        intersection.EndTime = x.EndTime;
                        return intersection;
                    })
                    .ToList();

                var row = new DetailedViewRowData
                {
                    Rank = coverage.Rank,
                    MarketGeographyName = coverage.MarketName,
                    StationLegacyCallLetters = manifest.Station.LegacyCallLetters,
                    StationAffiliation = manifest.Station.Affiliation,
                    Dayparts = intersections,
                    TotalSpotsImpressions = allocatedSpotsForManifest.Sum(x => x.TotalImpressions),

                    // they all should have the same code because only weeks differ. Appies only for OpenMarket
                    DaypartCode = standardDaypart.Code
                };

                if (primaryProgramsByManifestDaypartIds.TryGetValue(manifestDaypart.Id.Value, out var primaryProgram))
                {
                    row.ProgramName = primaryProgram.Name;
                    row.Genre = primaryProgram.Genre;
                }
                else
                {
                    // If no information from Dativa is available, use the program name from the inventory file
                    row.ProgramName = manifestDaypart.ProgramName ?? string.Empty;
                    row.Genre = string.Empty;
                }

                dataRows.Add(row);
            }

            // Handles the case when two manifests have the same station, market, etc but different weeks 
            // and we have spots allocated to both of them
            var result = dataRows
                .GroupBy(x => new
                {
                    x.Rank,
                    x.MarketGeographyName,
                    x.StationLegacyCallLetters,
                    x.StationAffiliation,
                    Day = x.Dayparts.First().ToDayString(),
                    Time = x.Dayparts.First().ToTimeString(),
                    Day2 = x.Dayparts.LastOrDefault()?.ToDayString(),
                    Time2 = x.Dayparts.LastOrDefault()?.ToTimeString(),
                    x.ProgramName,
                    x.Genre,
                    x.DaypartCode
                })
                .Select(grouping =>
                {
                    var first = grouping.First();
                    first.TotalSpotsImpressions = grouping.Sum(x => x.TotalSpotsImpressions);
                    return first;
                })
                .ToList();

            return result;
        }
        private List<DetailedViewRowDisplay> _MapToDetailedViewRows(IEnumerable<DetailedViewRowData> dataRows)
        {
            return dataRows
                .OrderBy(x => x.Rank)
                .ThenBy(x => x.StationAffiliation)
                .ThenBy(x => x.ProgramName)
                .GroupBy(x => new { x.DaypartCode, x.Genre, x.MarketGeographyName, x.ProgramName, x.Rank, x.StationLegacyCallLetters })
                .Select(x =>
                {
                    var first = x.First();
                    return new DetailedViewRowDisplay
                    {
                        Rank = x.Key.Rank,
                        DMA = x.Key.MarketGeographyName.ToUpper(),
                        Station = x.Key.StationLegacyCallLetters.ToUpper(),
                        NetworkAffiliation = first.StationAffiliation.ToUpper(),
                        Days = _CalculateActiveDays(first.Dayparts.First().Days),
                        TimePeriods = string.Join(", ", first.Dayparts.Select(y => y.ToTimeString().ToUpper())),
                        Program = x.Key.ProgramName.ToUpper(),
                        Genre = x.Key.Genre.ToUpper(),
                        Daypart = first.DaypartCode.ToUpper()
                    };
                })
                .ToList();
        }

        private string _CalculateActiveDays(List<DayOfWeek> days)
        {
            if (!days.Contains(DayOfWeek.Saturday) && !days.Contains(DayOfWeek.Sunday))
            {
                return "M-F";
            }
            else
            if (!days.Contains(DayOfWeek.Monday) && !days.Contains(DayOfWeek.Tuesday)
                && !days.Contains(DayOfWeek.Wednesday) && !days.Contains(DayOfWeek.Thursday)
                && !days.Contains(DayOfWeek.Friday))
            {
                return "Sa-Su";
            }
            else
                return "M-Su";

        }

        private List<DefaultViewRowDisplay> _MapToDefaultViewRows(IEnumerable<DetailedViewRowData> dataRows, double totalAllocatedImpressions)
        {
            _RollupStationSpecificNewsPrograms(dataRows);

            return dataRows
                .GroupBy(x => x.ProgramName)
               .Select(x =>
               {
                   var items = x.ToList();
                   return new DefaultViewRowDisplay
                   {
                       Program = x.Key.ToUpper(),
                       Weight = _CalculateWeight(items.Sum(y => y.TotalSpotsImpressions), totalAllocatedImpressions),
                       Genre = items.First().Genre.ToUpper(),
                       NoOfMarkets = items.Select(y => y.MarketGeographyName).Distinct().Count(),
                       NoOfStations = items.Select(y => y.StationLegacyCallLetters).Distinct().Count()
                   };
               })
               .OrderByDescending(x => x.Weight)
               .ToList();
        }

        private static void _RollupStationSpecificNewsPrograms(IEnumerable<DetailedViewRowData> dataRows)
        {
            //we extract 1 second from the endtime of the daypart
            const int AM12_05 = 299;
            const int AM4 = 14400;
            const int AM10 = 35999;
            const int AM11 = 39600;
            const int PM1 = 46799;
            const int PM4 = 57600;
            const int PM7 = 68399;
            const int PM8 = 72000;
            const string MORNING_NEWS = "Morning News";
            const string MIDDAY_NEWS = "Midday News";
            const string EVENING_NEWS = "Evening News";
            const string LATE_NEWS = "Late News";

            foreach (var row in dataRows.Where(x => x.Genre.Equals("News")))
            {
                var daypart = row.Dayparts.First();

                //we do the checking in reverse order because we don't want earlier news be labeled wrong
                if (CheckDaypartForRollup(daypart, PM8, AM12_05))
                {
                    row.ProgramName = LATE_NEWS;
                    continue;
                }

                if (CheckDaypartForRollup(daypart, PM4, PM7))
                {
                    row.ProgramName = EVENING_NEWS;
                    continue;
                }
                if (CheckDaypartForRollup(daypart, AM11, PM1))
                {
                    row.ProgramName = MIDDAY_NEWS;
                    continue;
                }
                if (CheckDaypartForRollup(daypart, AM4, AM10))
                {
                    row.ProgramName = MORNING_NEWS;
                    continue;
                }
            }
        }

        internal static bool CheckDaypartForRollup(DisplayDaypart daypart, int startTimeSeconds, int endTimeSeconds)
        {
            if (startTimeSeconds <= endTimeSeconds)
            {
                if (daypart.StartTime >= startTimeSeconds
                && daypart.StartTime <= endTimeSeconds
                && daypart.EndTime >= startTimeSeconds
                && daypart.EndTime <= endTimeSeconds
                && daypart.EndTime > daypart.StartTime)
                {
                    return true;
                }
            }
            else
            {
                if ((daypart.StartTime >= startTimeSeconds && daypart.EndTime < BroadcastConstants.OneDayInSeconds && daypart.StartTime <= daypart.EndTime)
                    || (daypart.StartTime >= startTimeSeconds && daypart.EndTime > 0 && daypart.EndTime <= endTimeSeconds)
                    || (daypart.StartTime >= 0 && daypart.StartTime <= endTimeSeconds && daypart.StartTime <= daypart.EndTime && daypart.EndTime <= endTimeSeconds))
                {
                    return true;
                }

            }
            return false;
        }

        private List<AllocationViewRowDisplay> _MapToAllocationViewRows(
            IEnumerable<DetailedViewRowData> dataRows
            , double totalAllocatedImpressions
            , Func<DetailedViewRowData, string> groupFunction
            , bool toUpper)
        {
            return dataRows
                .GroupBy(groupFunction)
               .Select(x =>
               {
                   var items = x.ToList();
                   return new AllocationViewRowDisplay
                   {
                       FilterLabel = toUpper ? x.Key.ToUpper() : x.Key,
                       Weight = _CalculateWeight(items.Sum(y => y.TotalSpotsImpressions), totalAllocatedImpressions)
                   };
               })
               .OrderByDescending(x => x.Weight)
               .ToList();
        }

        private List<AllocationViewRowDisplay> _MapDMAToAllocationViewRows(
            IEnumerable<DetailedViewRowData> dataRows
            , double totalAllocatedImpressions)
        {
            return dataRows
                .GroupBy(x => new { x.MarketGeographyName, x.Rank })
                .OrderBy(x => x.Key.Rank)
               .Select(x =>
               {
                   var items = x.ToList();
                   return new AllocationViewRowDisplay
                   {
                       FilterLabel = x.Key.MarketGeographyName.ToUpper(),
                       Weight = _CalculateWeight(items.Sum(y => y.TotalSpotsImpressions), totalAllocatedImpressions)
                   };
               })
               .ToList();
        }

        private string _GetSpotLengths(PlanDto plan, List<LookupDto> spotLengths)
        {
            return string.Join(",", plan.CreativeLengths
                .Select(y => new { spotLengths.Single(w => w.Id == y.SpotLengthId).Display, plan.Equivalized })
                .OrderBy(x => int.Parse(x.Display))
                .Select(x => $":{x.Display}{_GetEquivalizedStatus(x.Equivalized, x.Display)}")
                .ToList());
        }

        private string _GetEquivalizedStatus(bool? equivalized, string display)
        {
            return (equivalized ?? false) && !display.Equals("30") ? " eq." : string.Empty;
        }

        private double _CalculateWeight(double impressions, double totalAllocatedImpressions)
        {
            return totalAllocatedImpressions == 0
                ? 0
                : impressions / totalAllocatedImpressions;
        }

        private static void _LoadImpressionsBySpotLengthForProprietary(List<ProgramLineupProprietaryInventory> proprietaryInventory
            , Dictionary<int, double> data)
        {
            proprietaryInventory.ForEach(spot =>
            {
                if (data.ContainsKey(spot.SpotLengthId))
                {
                    data[spot.SpotLengthId] += spot.TotalImpressions;
                }
                else
                {
                    data.Add(spot.SpotLengthId, spot.TotalImpressions);
                }
            });
        }

        private static void _LoadImpressionsBySpotLengthForOpenMarket(List<PlanPricingAllocatedSpot> allocatedSpots
            , Dictionary<int, double> data)
        {
            allocatedSpots.ForEach(spot =>
            {
                spot.SpotFrequencies.ForEach(frequency =>
                {
                    if (data.ContainsKey(frequency.SpotLengthId))
                    {
                        data[frequency.SpotLengthId] += frequency.Spots * frequency.Impressions;
                    }
                    else
                    {
                        data.Add(frequency.SpotLengthId, frequency.Spots * frequency.Impressions);
                    }
                });
            });
        }

        private static void _LoadImpressionsBySpotLengthForBuyingOpenMarket(List<PlanBuyingAllocatedSpot> allocatedSpots
           , Dictionary<int, double> data)
        {
            allocatedSpots.ForEach(spot =>
            {
                spot.SpotFrequencies.ForEach(frequency =>
                {
                    if (data.ContainsKey(frequency.SpotLengthId))
                    {
                        data[frequency.SpotLengthId] += frequency.Spots * frequency.Impressions;
                    }
                    else
                    {
                        data.Add(frequency.SpotLengthId, frequency.Spots * frequency.Impressions);
                    }
                });
            });
        }

        private List<AllocationViewRowDisplay> _MapAffiliateToAllocationViewRows(IEnumerable<DetailedViewRowData> dataRows
            , double totalAllocatedImpressions)
        {
            return dataRows
                .GroupBy(x => new { x.StationAffiliation })
                .OrderBy(x => x.Key.StationAffiliation)
               .Select(x =>
               {
                   var items = x.ToList();
                   return new AllocationViewRowDisplay
                   {
                       FilterLabel = x.Key.StationAffiliation.ToUpper(),
                       Weight = _CalculateWeight(items.Sum(y => y.TotalSpotsImpressions), totalAllocatedImpressions)
                   };
               })
               .ToList();
        }

        public class DetailedViewRowDisplay
        {
            public int Rank { get; set; }
            public string DMA { get; set; }
            public string Station { get; set; }
            public string NetworkAffiliation { get; set; }
            public string Days { get; set; }
            public string TimePeriods { get; set; }
            public string Program { get; set; }
            public string Genre { get; set; }
            public string Daypart { get; set; }
        }

        public class DefaultViewRowDisplay
        {
            public string Program { get; set; }
            public double Weight { get; set; }
            public string Genre { get; set; }
            public int NoOfStations { get; set; }
            public int NoOfMarkets { get; set; }
        }

        public class AllocationViewRowDisplay
        {
            public string FilterLabel { get; set; }
            public double Weight { get; set; }
        }

        public class DetailedViewRowData
        {
            public int Rank { get; set; }
            public string MarketGeographyName { get; set; }
            public string StationLegacyCallLetters { get; set; }
            public string StationAffiliation { get; set; }
            public List<DisplayDaypart> Dayparts { get; set; }
            public string ProgramName { get; set; }
            public string Genre { get; set; }
            public string DaypartCode { get; set; }
            public double TotalSpotsImpressions { get; set; }
        }
    }
}
