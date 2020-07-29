using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram.ManifestDaypart;

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
        public string AccountExecutive { get; set; }
        public string ClientContact { get; set; }
        public List<DetailedViewRowDisplay> DetailedViewRows { get; set; }
        public List<DefaultViewRowDisplay> DefaultViewRows { get; set; }
        public List<AllocationViewRowDisplay> AllocationByDaypartViewRows { get; set; }
        public List<AllocationViewRowDisplay> AllocationByGenreViewRows { get; set; }
        public List<AllocationViewRowDisplay> AllocationByDMAViewRows { get; set; }

        private const string FILENAME_FORMAT = "Program_Lineup_Report_{0}_{1}.xlsx";
        private const string PLAN_HEADER_NAME_FORMAT = "{0} | Program Lineup*";
        private const string DATE_FORMAT_FILENAME = "MMddyyyy";
        private const string DATE_FORMAT_SHORT_YEAR_SLASHES = "MM/dd/yy";
        private const string DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT = "M/d/yy";

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
            Dictionary<int, Program> primaryProgramsByManifestDaypartIds)
        {
            ExportFileName = string.Format(FILENAME_FORMAT, plan.Name, currentDate.ToString(DATE_FORMAT_FILENAME));

            _PopulateHeaderData(plan, planPricingJob, agency, advertiser, guaranteedDemo, spotLengths, currentDate);
            IEnumerable<DetailedViewRowData> detailedRowsData = _GetDetailedViewRowData(
                plan,
                allocatedSpots,
                manifests,
                marketCoverageByStation,
                primaryProgramsByManifestDaypartIds);
            double totalAllocatedImpressions = detailedRowsData.Sum(x => x.TotalSpotsImpressions);
            DetailedViewRows = _MapToDetailedViewRows(detailedRowsData);

            DefaultViewRows = _MapToDefaultViewRows(detailedRowsData, totalAllocatedImpressions);
            AllocationByDaypartViewRows = _MapToAllocationViewRows(detailedRowsData, totalAllocatedImpressions
                , x => x.DaypartCode
                , false);
            AllocationByGenreViewRows = _MapToAllocationViewRows(detailedRowsData, totalAllocatedImpressions
                , x => x.Genre
                , true);
            AllocationByDMAViewRows = _MapToAllocationViewRows(detailedRowsData, totalAllocatedImpressions
                , x => x.MarketGeographyName
                , true);
        }

        private void _PopulateHeaderData(
            PlanDto plan,
            PlanPricingJob planPricingJob,
            AgencyDto agency,
            AdvertiserDto advertiser,
            PlanAudienceDisplay guaranteedDemo,
            List<LookupDto> spotLengths,
            DateTime currentDate)
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
            PostingType = plan.PostingType.ToString();
            AccountExecutive = string.Empty;
            ClientContact = string.Empty;
        }

        private IEnumerable<DetailedViewRowData> _GetDetailedViewRowData(
            PlanDto plan,
            List<PlanPricingAllocatedSpot> allocatedSpots,
            List<StationInventoryManifest> manifests,
            MarketCoverageByStation marketCoverageByStation,
            Dictionary<int, Program> primaryProgramsByManifestDaypartIds)
        {
            var dataRows = new List<DetailedViewRowData>();
            var planDaypartById = plan.Dayparts.ToDictionary(x => x.DaypartCodeId, x => x);

            // we expect all records belong to the same daypart because it`s OpenMarket
            // So we can just group by StationInventoryManifestId for now
            var spotsByManifest = allocatedSpots
                .GroupBy(x => x.StationInventoryManifestId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var marketCoverageByMarketCode = marketCoverageByStation.Markets.ToDictionary(x => x.MarketCode, x => x);

            foreach (var manifest in manifests)
            {
                if (!spotsByManifest.TryGetValue(manifest.Id.Value, out var allocatedSpotsForManifest))
                    continue;

                var coverage = marketCoverageByMarketCode[manifest.Station.MarketCode.Value];

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
                    TotalSpotsImpressions = allocatedSpotsForManifest.Sum(x => x.Impressions * x.Spots),

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
                .Select(x => new DetailedViewRowDisplay
                {
                    Rank = x.Rank,
                    DMA = x.MarketGeographyName.ToUpper(),
                    Station = x.StationLegacyCallLetters.ToUpper(),
                    NetworkAffiliation = x.StationAffiliation.ToUpper(),
                    Days = x.Dayparts.First().ToDayString().ToUpper(),
                    TimePeriods = string.Join(", ", x.Dayparts.Select(y => y.ToTimeString().ToUpper())),
                    Program = x.ProgramName.ToUpper(),
                    Genre = x.Genre.ToUpper(),
                    Daypart = x.DaypartCode.ToUpper()
                })
                .ToList();
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

        private string _GetSpotLengths(PlanDto plan, List<LookupDto> spotLengths)
        {
            return string.Join(",", plan.CreativeLengths
                .Select(y => new { spotLengths.Single(w => w.Id == y.SpotLengthId).Display, plan.Equivalized })
                .OrderBy(x => int.Parse(x.Display))
                .Select(x => $":{x.Display}{_GetEquivalizedStatus(x.Equivalized, x.Display)}")
                .ToList());
        }

        private string _GetEquivalizedStatus(bool equivalized, string display)
        {
            return equivalized && !display.Equals("30") ? " eq." : string.Empty;
        }

        private double _CalculateWeight(double impressions, double totalAllocatedImpressions)
        {
            return totalAllocatedImpressions == 0
                ? 0
                : impressions / totalAllocatedImpressions;
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
