using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram.ManifestDaypart;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PricingResultsReportData
    {
        public string ExportFileName { get; set; }

        public string PlanId { get; set; }

        public string PlanVersion { get; set; }

        public SpotAllocationTotals SpotAllocationTotals { get; set; }

        public List<SpotsAllocation> SpotAllocations { get; set; }

        private const string FILENAME_FORMAT = "Pricing_Results_Report_Plan_{0}_Version_{1}.xlsx";
        private const string TIME_FORMAT = "HH:mm:ss";

        public PricingResultsReportData(
            PlanDto plan,
            List<PlanPricingAllocatedSpot> allocatedSpots,
            List<StationInventoryManifest> manifests,
            Dictionary<int, Program> primaryProgramsByManifestDaypartIds,
            List<LookupDto> markets,
            IWeeklyBreakdownEngine weeklyBreakdownEngine)
        {
            ExportFileName = string.Format(FILENAME_FORMAT, plan.Id, plan.VersionNumber.Value);
            PlanId = plan.Id.ToString();
            PlanVersion = $"Version {plan.VersionNumber.Value}";

            _PopulateSpotAllocationTotals(allocatedSpots);

            _PopulateSpotAllocations(
                plan,
                allocatedSpots,
                manifests,
                primaryProgramsByManifestDaypartIds,
                markets,
                weeklyBreakdownEngine);
        }

        private void _PopulateSpotAllocations(
            PlanDto plan,
            List<PlanPricingAllocatedSpot> allocatedSpots,
            List<StationInventoryManifest> manifests,
            Dictionary<int, Program> primaryProgramsByManifestDaypartIds,
            List<LookupDto> markets,
            IWeeklyBreakdownEngine weeklyBreakdownEngine)
        {
            SpotAllocations = new List<SpotsAllocation>();
            var weekNumberByMediaWeek = weeklyBreakdownEngine.GetWeekNumberByMediaWeekDictionary(plan.WeeklyBreakdownWeeks);
            var manifestsById = manifests.ToDictionary(x => x.Id, x => x);
            var marketsByCode = markets.ToDictionary(x => x.Id, x => x.Display);

            foreach (var allocation in allocatedSpots)
            {
                var manifest = manifestsById[allocation.StationInventoryManifestId];
                var manifestDaypart = manifest.ManifestDayparts.Single(); // Works only for OpenMarket
                var program = primaryProgramsByManifestDaypartIds[manifestDaypart.Id.Value];

                var spotAllocation = new SpotsAllocation
                {
                    ProgramName = program.Name,
                    Genre = program.Genre,
                    ShowType = program.ShowType,
                    Station = manifest.Station.LegacyCallLetters,
                    Market = marketsByCode[manifest.Station.MarketCode.Value],
                    DaypartCode = allocation.StandardDaypart.Code,
                    Spots = allocation.Spots,
                    TotalImpressions = allocation.TotalImpressions,
                    TotalCost = allocation.TotalCost,
                    CPM = ProposalMath.CalculateCpm(allocation.SpotCost, allocation.Impressions),
                    PlanWeekNumber = weekNumberByMediaWeek[allocation.ContractMediaWeek.Id],
                    StartDate = allocation.InventoryMediaWeek.StartDate,
                    EndDate = allocation.InventoryMediaWeek.EndDate,
                    StartTime = DaypartTimeHelper.ConvertSecondsToFormattedTime(manifestDaypart.Daypart.StartTime, TIME_FORMAT),
                    EndTime = DaypartTimeHelper.ConvertSecondsToFormattedTime(manifestDaypart.Daypart.EndTime + 1, TIME_FORMAT)
                };

                SpotAllocations.Add(spotAllocation);
            }
        }

        private void _PopulateSpotAllocationTotals(List<PlanPricingAllocatedSpot> allocatedSpots)
        {
            var impressions = allocatedSpots.Sum(x => x.TotalImpressions);
            var cost = allocatedSpots.Sum(x => x.TotalCost);

            SpotAllocationTotals = new SpotAllocationTotals
            {
                Spots = allocatedSpots.Sum(x => x.Spots),
                Impressions = impressions,
                Cost = cost,
                CPM = ProposalMath.CalculateCpm(cost, impressions)
            };
        }
    }

    public class SpotAllocationTotals
    {
        public int Spots { get; set; }

        public double Impressions { get; set; }

        public decimal Cost { get; set; }

        public decimal CPM { get; set; }
    }

    public class SpotsAllocation
    {
        public string ProgramName { get; set; }

        public string Genre { get; set; }

        public string ShowType { get; set; }

        public string Station { get; set; }

        public string Market { get; set; }

        public string DaypartCode { get; set; }

        public int Spots { get; set; }

        public double TotalImpressions { get; set; }

        public decimal TotalCost { get; set; }

        public decimal CPM { get; set; }

        public int PlanWeekNumber { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }
    }
}
