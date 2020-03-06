﻿using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.StationInventory;
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
        private readonly IStandartDaypartEngine _StandartDaypartEngine;

        public string ExportFileName { get; set; }
        public string PlanHeaderName { get; set; }
        public string ReportGeneratedDate { get; set; }
        public string AccuracyEstimateDate { get; set; }
        public string Agency { get; set; }
        public string Client { get; set; }
        public string FlightStartDate { get; set; }
        public string FlightEndDate { get; set; }
        public string GuaranteedDemo { get; set; }
        public string SpotLength { get; set; }
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
            List<LookupDto> spotLenghts,
            DateTime currentDate,
            List<PlanPricingAllocatedSpot> allocatedSpots,
            List<StationInventoryManifest> manifests,
            MarketCoverageByStation marketCoverageByStation,
            Dictionary<int, Program> primaryProgramsByManifestDaypartIds,
            IStandartDaypartEngine standartDaypartEngine)
        {
            _StandartDaypartEngine = standartDaypartEngine;

            ExportFileName = string.Format(FILENAME_FORMAT, plan.Name, currentDate.ToString(DATE_FORMAT_FILENAME));

            _PopulateHeaderData(plan, planPricingJob, agency, advertiser, guaranteedDemo, spotLenghts, currentDate);
            IEnumerable<DetailedViewRowData> detailedRowsData = _GetDetailedViewRowData(
                allocatedSpots,
                manifests,
                marketCoverageByStation,
                primaryProgramsByManifestDaypartIds);
            DetailedViewRows = _MapToDetailedViewRows(detailedRowsData);

            //This code remains commented out until pushed to 20.05 BCOP
            //DefaultViewRows = _MapToDefaultViewRows(detailedRowsData, plan.TargetImpressions.Value);
            //AllocationByDaypartViewRows = _MapToAllocationViewRows(detailedRowsData, plan.TargetImpressions.Value
            //    , x => x.Daypart.Code
            //    , false);
            //AllocationByGenreViewRows = _MapToAllocationViewRows(detailedRowsData, plan.TargetImpressions.Value
            //    , x => x.Genre
            //    , true);
            //AllocationByDMAViewRows = _MapToAllocationViewRows(detailedRowsData, plan.TargetImpressions.Value
            //    , x => x.MarketGeographyName
            //    , true);
        }

        private void _PopulateHeaderData(
            PlanDto plan,
            PlanPricingJob planPricingJob,
            AgencyDto agency,
            AdvertiserDto advertiser,
            PlanAudienceDisplay guaranteedDemo,
            List<LookupDto> spotLenghts,
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
            SpotLength = _GetSpotLength(plan, spotLenghts);
            PostingType = plan.PostingType.ToString();
            AccountExecutive = string.Empty;
            ClientContact = string.Empty;
        }

        private IEnumerable<DetailedViewRowData> _GetDetailedViewRowData(
            List<PlanPricingAllocatedSpot> allocatedSpots,
            List<StationInventoryManifest> manifests,
            MarketCoverageByStation marketCoverageByStation,
            Dictionary<int, Program> primaryProgramsByManifestDaypartIds)
        {
            var dataRows = new List<DetailedViewRowData>();

            foreach (var manifest in manifests)
            {
                var coverage = marketCoverageByStation.Markets.Single(x => x.MarketCode == manifest.Station.MarketCode);

                foreach (var manifestDaypart in manifest.ManifestDayparts)
                {
                    var daypart = manifestDaypart.Daypart;
                    var queryAllocatedSpots = allocatedSpots.Where(x =>
                        x.StationInventoryManifestId == manifest.Id &&
                        x.Daypart.Id == daypart.Id &&
                        x.Spots > 0);

                    if (!queryAllocatedSpots.Any())
                        continue;

                    var row = new DetailedViewRowData
                    {
                        Rank = coverage.Rank,
                        MarketGeographyName = coverage.MarketName,
                        StationLegacyCallLetters = manifest.Station.LegacyCallLetters,
                        StationAffiliation = manifest.Station.Affiliation,
                        Daypart = daypart,
                        TotalSpotsImpressions = queryAllocatedSpots.Sum(x => x.Impressions * x.Spots)
                    };

                    if (primaryProgramsByManifestDaypartIds.TryGetValue(manifestDaypart.Id.Value, out var primaryProgram))
                    {
                        var genre = primaryProgram.Genre;
                        var timeRange = new TimeRange { StartTime = daypart.StartTime, EndTime = daypart.EndTime };
                        var daypartCode = _StandartDaypartEngine.GetDaypartCodeByGenreAndTimeRange(genre, timeRange).Code;

                        row.ProgramName = primaryProgram.Name;
                        row.Genre = genre;
                        row.DaypartCode = daypartCode;
                    }
                    else
                    {
                        // If no information from Dativa is available, use the program name from the inventory file
                        row.ProgramName = manifestDaypart.ProgramName ?? string.Empty;
                        row.Genre = string.Empty;
                        row.DaypartCode = string.Empty;
                    }

                    dataRows.Add(row);
                }
            }

            return dataRows.Distinct(new DetailedViewRowDataComparer());
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
                    Days = x.Daypart.ToDayString().ToUpper(),
                    TimePeriods = x.Daypart.ToTimeString().ToUpper(),
                    Program = x.ProgramName.ToUpper(),
                    Genre = x.Genre.ToUpper(),
                    Daypart = x.DaypartCode.ToUpper()
                })
                .ToList();
        }

        private List<DefaultViewRowDisplay> _MapToDefaultViewRows(IEnumerable<DetailedViewRowData> dataRows, double planImpressions)
        {
            return dataRows
                .GroupBy(x => x.ProgramName)
               .Select(x =>
               {
                   var items = x.ToList();
                   return new DefaultViewRowDisplay
                   {
                       Program = x.Key.ToUpper(),
                       Weight = _CalculateWeight(items.Sum(y => y.TotalSpotsImpressions), planImpressions),
                       Genre = items.First().Genre.ToUpper(),
                       NoOfMarkets = items.Select(y => y.MarketGeographyName).Distinct().Count(),
                       NoOfStations = items.Select(y => y.StationAffiliation).Distinct().Count()
                   };
               })
               .OrderByDescending(x => x.Weight)
               .ToList();
        }

        private List<AllocationViewRowDisplay> _MapToAllocationViewRows(
            IEnumerable<DetailedViewRowData> dataRows
            , double planImpressions
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
                       Weight = _CalculateWeight(items.Sum(y => y.TotalSpotsImpressions), planImpressions)
                   };
               })
               .OrderByDescending(x => x.Weight)
               .ToList();
        }

        private string _GetSpotLength(PlanDto plan, List<LookupDto> spotLenghts)
        {
            var spotLenghtDisplay = spotLenghts.Single(x => x.Id == plan.SpotLengthId).Display;
            spotLenghtDisplay = plan.Equivalized && int.Parse(spotLenghtDisplay) != 30 ? $"{spotLenghtDisplay} eq." : spotLenghtDisplay;
            spotLenghtDisplay = ":" + spotLenghtDisplay;

            return spotLenghtDisplay;
        }

        private double _CalculateWeight(double impressions, double planImpressions)
        {
            return planImpressions == 0
                ? 0
                : impressions / planImpressions;
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
            public DisplayDaypart Daypart { get; set; }
            public string ProgramName { get; set; }
            public string Genre { get; set; }
            public string DaypartCode { get; set; }
            public double TotalSpotsImpressions { get; set; }
        }

        private class DetailedViewRowDataComparer : IEqualityComparer<DetailedViewRowData>
        {
            public bool Equals(DetailedViewRowData x, DetailedViewRowData y)
            {
                return x.Rank == y.Rank
                    && x.MarketGeographyName.Equals(y.MarketGeographyName)
                    && x.StationLegacyCallLetters.Equals(y.StationLegacyCallLetters)
                    && x.StationAffiliation.Equals(y.StationAffiliation)
                    && x.Daypart.ToDayString().Equals(y.Daypart.ToDayString())
                    && x.Daypart.ToTimeString().Equals(y.Daypart.ToTimeString())
                    && x.ProgramName.Equals(y.ProgramName)
                    && x.Genre.Equals(y.Genre)
                    && x.DaypartCode.Equals(y.DaypartCode);
            }

            public int GetHashCode(DetailedViewRowData obj)
            {
                return $@"{obj.Rank}{obj.MarketGeographyName}{obj.StationLegacyCallLetters}{obj.StationAffiliation}
                            {obj.Daypart.ToDayString()}{obj.Daypart.ToTimeString()}{obj.ProgramName}
                            {obj.Genre}{obj.DaypartCode}".GetHashCode();
            }
        }
    }
}
