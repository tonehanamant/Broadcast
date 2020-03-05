using Services.Broadcast.BusinessEngines;
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
            _PopulateDetailedViewRows(
                allocatedSpots,
                manifests,
                marketCoverageByStation,
                primaryProgramsByManifestDaypartIds);
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

        public void _PopulateDetailedViewRows(
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
                    var hasInventorySpotsAllocated = allocatedSpots.Any(x => 
                        x.StationInventoryManifestId == manifest.Id && 
                        x.Daypart.Id == daypart.Id &&
                        x.Spots > 0);

                    if (!hasInventorySpotsAllocated)
                        continue;

                    var row = new DetailedViewRowData
                    {
                        Rank = coverage.Rank,
                        MarketGeographyName = coverage.MarketName,
                        StationLegacyCallLetters = manifest.Station.LegacyCallLetters,
                        StationAffiliation = manifest.Station.Affiliation,
                        Daypart = daypart
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

            DetailedViewRows = _MapToDetailedViewRows(dataRows);
        }

        private List<DetailedViewRowDisplay> _MapToDetailedViewRows(List<DetailedViewRowData> dataRows)
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

        private string _GetSpotLength(PlanDto plan, List<LookupDto> spotLenghts)
        {
            var spotLenghtDisplay = spotLenghts.Single(x => x.Id == plan.SpotLengthId).Display;
            spotLenghtDisplay = plan.Equivalized && int.Parse(spotLenghtDisplay) != 30 ? $"{spotLenghtDisplay} eq." : spotLenghtDisplay;
            spotLenghtDisplay = ":" + spotLenghtDisplay;

            return spotLenghtDisplay;
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
        }
    }
}
