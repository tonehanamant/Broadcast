using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.Campaign
{
    public class CampaignReportData
    {
        public string CampaignName { get; set; }
        public string CreatedDate { get; set; }
        public string CampaignStartQuarter { get; set; }
        public string CampaignEndQuarter { get; set; }
        public string AgencyName { get; set; }
        public string ClientName { get; set; }
        public string CampaignFlightStartDate { get; set; }
        public string CampaignFlightEndDate { get; set; }
        public List<string> GuaranteedDemo { get; set; }
        public List<string> SpotLengths { get; set; }
        public string PostingType { get; set; }
        public string Status { get; set; }
        public List<ProposalQuarterTableData> QuarterTables { get; set; } = new List<ProposalQuarterTableData>();
        public ProposalQuarterTableData CampaignTotalsTable { get; set; }
        public MarketCoverageData MarketCoverageData { get; set; }
        public List<DaypartData> Dayparts { get; set; } = new List<DaypartData>();
        public List<DateTime> FlightHiatuses { get; set; } = new List<DateTime>();
        public string Notes { get; set; }

        private const string DATE_FORMAT_SHORT_YEAR = "MM/dd/yy";

        public CampaignReportData(CampaignExportTypeEnum exportType, CampaignDto campaign
            , List<PlanDto> plans, AgencyDto agency, AdvertiserDto advertiser
            , List<PlanAudienceDisplay> guaranteedDemos
            , List<LookupDto> spotLenghts
            , List<DaypartCodeDto> daypartCodes
            , List<PlanAudienceDisplay> orderedAudiences
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IQuarterCalculationEngine quarterCalculationEngine)
        {
            List<ProjectedPlan> projectedPlans = _ProjectPlansForQuarterExport(plans, spotLenghts, daypartCodes, mediaMonthAndWeekAggregateCache, quarterCalculationEngine);
            _PopulateHeaderData(exportType, campaign, plans, agency, advertiser, guaranteedDemos, spotLenghts, orderedAudiences, quarterCalculationEngine);
            _PopulateQuarterTableData(projectedPlans, quarterCalculationEngine);
        }

        private List<ProjectedPlan> _ProjectPlansForQuarterExport(List<PlanDto> plans, List<LookupDto> spotLenghts
            , List<DaypartCodeDto> daypartCodes
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IQuarterCalculationEngine quarterCalculationEngine)
        {
            List<ProjectedPlan> result = new List<ProjectedPlan>();
            plans.ForEach(plan =>
            {
                List<QuarterDetailDto> planQuarters = quarterCalculationEngine.GetAllQuartersBetweenDates(plan.FlightStartDate.Value, plan.FlightEndDate.Value);

                planQuarters.ForEach(quarter =>
                {
                    //calculate the quarter factor: how many weeks from plan flight are in this quarter as a percentage to the count of total weeks in the plan
                    //store the week ids from the current quarter in a list
                    List<int> quarterTotalWeeksIdList = mediaMonthAndWeekAggregateCache.GetMediaWeeksByFlight(quarter.StartDate, quarter.EndDate).Select(x => x.Id).ToList();
                    //count all the plan weeks in current quarter
                    double quarterPlanWeeksImpressionsPrecentage = plan.WeeklyBreakdownWeeks
                                                                .Where(x => quarterTotalWeeksIdList.Contains(x.MediaWeekId))
                                                                .Sum(x => x.WeeklyImpressionsPercentage);
                    //calculate quarter factor
                    double quarterFactor = quarterPlanWeeksImpressionsPrecentage / 100;

                    //dayparts that don't have set a value for weighting goal will get an even part from the remaining goal
                    //calculate remaining goal by substracting the user set goal from 100
                    double remainingWeightingGoal = 100 - plan.Dayparts.Select(wg => wg.WeightingGoalPercent ?? 0).Sum();
                    //count total dayparts without weighting goal
                    int daypartsWithoutWeightingGoal = plan.Dayparts.Where(wg => !wg.WeightingGoalPercent.HasValue).Count();
                    //calculate weighting goal factor
                    double weightingGoalFactor = daypartsWithoutWeightingGoal == 0
                                ? 1
                                : remainingWeightingGoal / daypartsWithoutWeightingGoal;

                    plan.Dayparts.ForEach(daypart =>
                    {
                        //if the daypart has weighting goal set, we use that and only apply the quarter factor
                        //if the daypart does not have weighting goal, we use the weighting goal and quarter factors together
                        double multiplicationFactor = daypart.WeightingGoalPercent.HasValue
                                    ? (daypart.WeightingGoalPercent.Value / 100) * quarterFactor
                                    : (weightingGoalFactor / 100) * quarterFactor;

                        var daypartCode = daypartCodes.Single(dc => dc.Id == daypart.DaypartCodeId);
                        var newProjectedPlan = new ProjectedPlan
                        {
                            QuarterYear = quarter.Year,
                            QuarterNumber = quarter.Quarter,
                            DaypartCode = daypartCode.Code,
                            SpotLengthId = plan.SpotLengthId,
                            SpotLength = spotLenghts.Single(y => y.Id == plan.SpotLengthId).Display,
                            Equivalized = plan.Equivalized,
                            TotalCost = plan.Budget.Value * (decimal)multiplicationFactor,
                            Units = 0, //we have no value calculated and stored for this property at the moment                            

                            //HH data
                            TotalHHImpressions = plan.HHImpressions * multiplicationFactor,
                            TotalHHRatingPoints = plan.HHRatingPoints * multiplicationFactor,

                            //guaranteed audience data
                            AudienceId = plan.AudienceId,
                            VPVH = plan.Vpvh,
                            TotalImpressions = plan.TargetImpressions.Value * multiplicationFactor,
                            TotalRatingPoints = plan.TargetRatingPoints.Value * multiplicationFactor
                        };
                        newProjectedPlan.CPM = newProjectedPlan.TotalImpressions == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalImpressions * 1000;
                        newProjectedPlan.CPP = newProjectedPlan.TotalRatingPoints == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalRatingPoints;
                        newProjectedPlan.HHCPM = newProjectedPlan.TotalHHImpressions == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalHHImpressions * 1000;
                        newProjectedPlan.HHCPP = newProjectedPlan.TotalHHRatingPoints == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalHHRatingPoints;
                        result.Add(newProjectedPlan);
                    });

                });
            });

            return result;
        }

        private void _PopulateQuarterTableData(List<ProjectedPlan> allDayparts, IQuarterCalculationEngine quarterCalculationEngine)
        {
            allDayparts.GroupBy(x => new { x.QuarterNumber, x.QuarterYear })
                .ToList()
                .ForEach(planGroup =>
                {
                    ProposalQuarterTableData newTable = new ProposalQuarterTableData
                    {
                        QuarterLabel = $"Q{planGroup.Key.QuarterNumber} {planGroup.Key.QuarterYear}",
                        Rows = new List<ProposalQuarterTableRowData>()
                    };

                    planGroup.GroupBy(x => new { x.DaypartCode, x.SpotLength, x.Equivalized })
                    .ToList()
                    .ForEach(daypartGroup =>
                    {
                        var items = daypartGroup.ToList();
                        int unitsSum = items.Sum(x => x.Units);

                        var row = new ProposalQuarterTableRowData
                        {
                            DaypartCode = daypartGroup.Key.DaypartCode,
                            SpotLength = $"{daypartGroup.Key.SpotLength}{(daypartGroup.Key.Equivalized ? $" eq." : string.Empty)}",
                            TotalCost = items.Sum(x => x.TotalCost),
                            UnitCost = (unitsSum == 0 ? 0 : items.Sum(x => x.TotalCost) / unitsSum),
                            Units = unitsSum,
                            GuaranteedAudienceData = new ProposalQuarterGuaranteedAudienceData
                            {
                                Impressions = (items.Sum(x => x.Units) == 0 ? 0 : (items.Sum(x => x.TotalImpressions) / unitsSum) / 1000),
                                RatingPoints = (unitsSum == 0 ? 0 : items.Sum(x => x.TotalRatingPoints) / unitsSum),
                                TotalImpressions = items.Sum(x => x.TotalImpressions) / 1000,
                                TotalRatingPoints = items.Sum(x => x.TotalRatingPoints)
                            },
                            HHAudienceData = new ProposalQuarterAudienceData
                            {
                                Impressions = (unitsSum == 0 ? 0 : (items.Sum(x => x.TotalHHImpressions) / unitsSum) / 1000),
                                RatingPoints = (unitsSum == 0 ? 0 : items.Sum(x => x.TotalHHRatingPoints) / unitsSum),
                                TotalImpressions = items.Sum(x => x.TotalHHImpressions) / 1000,
                                TotalRatingPoints = items.Sum(x => x.TotalHHRatingPoints)
                            }
                        };
                        row.GuaranteedAudienceData.VPVH = row.GuaranteedAudienceData.TotalImpressions / row.HHAudienceData.TotalImpressions;
                        row.GuaranteedAudienceData.CPM = _CalculateCost(row.GuaranteedAudienceData.TotalImpressions, row.TotalCost);
                        row.GuaranteedAudienceData.CPP = _CalculateCost(row.GuaranteedAudienceData.TotalImpressions, row.TotalCost);
                        row.HHAudienceData.CPM = _CalculateCost(row.HHAudienceData.TotalImpressions, row.TotalCost);
                        row.HHAudienceData.CPP = _CalculateCost(row.HHAudienceData.TotalImpressions, row.TotalCost);

                        _SetRowTotals(newTable);

                        newTable.Rows.Add(row);
                        
                    });
                    QuarterTables.Add(newTable);
                });
        }

        private static void _SetRowTotals(ProposalQuarterTableData table)
        {
            table.TotalCost = table.Rows.Sum(x => x.TotalCost);
            table.TotalUnits = table.Rows.Sum(x => x.Units);
            var totalImpressions = table.Rows.Select(x => x.HHAudienceData).Sum(x => x.TotalImpressions);
            var totalRatingPoints = table.Rows.Select(x => x.HHAudienceData).Sum(x => x.TotalRatingPoints);
            table.TotalHHData = new ProposalQuarterTotalTableData
            {
                TotalImpressions = totalImpressions,
                TotalRatingPoints = totalRatingPoints,
                TotalCPM = _CalculateCost(totalImpressions, table.TotalCost),
                TotalCPP = _CalculateCost(totalRatingPoints, table.TotalCost)
            };
            totalImpressions = table.Rows.Select(x => x.GuaranteedAudienceData).Sum(x => x.TotalImpressions);
            totalRatingPoints = table.Rows.Select(x => x.GuaranteedAudienceData).Sum(x => x.TotalRatingPoints);
            table.TotalGuaranteeedData = new ProposalQuarterTotalTableData
            {
                TotalImpressions = totalImpressions,
                TotalRatingPoints = totalRatingPoints,
                TotalCPM = _CalculateCost(totalImpressions, table.TotalCost),
                TotalCPP = _CalculateCost(totalRatingPoints, table.TotalCost)
            };
        }

        private static decimal _CalculateCost(double impressions, decimal cost)
        {
            return impressions == 0 ? 0 : cost / (decimal)impressions;
        }

        private void _PopulateHeaderData(CampaignExportTypeEnum exportType
            , CampaignDto campaign
            , List<PlanDto> plans
            , AgencyDto agency
            , AdvertiserDto advertiser
            , List<PlanAudienceDisplay> guaranteedDemos
            , List<LookupDto> spotLenghts
            , List<PlanAudienceDisplay> orderedAudiences
            , IQuarterCalculationEngine quarterCalculationEngine)
        {
            CampaignStartQuarter = quarterCalculationEngine.GetQuarterRangeByDate(campaign.FlightStartDate).ShortFormat();
            CampaignEndQuarter = quarterCalculationEngine.GetQuarterRangeByDate(campaign.FlightEndDate).ShortFormat();
            CampaignName = campaign.Name;
            CreatedDate = DateTime.Now.ToString(DATE_FORMAT_SHORT_YEAR);
            AgencyName = agency.Name;
            ClientName = advertiser.Name;
            _SetCampaignFlightDate(plans);
            PostingType = plans.Select(x => x.PostingType).Distinct().Single().ToString();
            Status = exportType.Equals(CampaignExportTypeEnum.Contract) ? "Order" : "Proposal";
            
            _SetGuaranteeedDemo(guaranteedDemos, orderedAudiences);
            _SetSpotLengths(plans, spotLenghts);
        }

        private void _SetSpotLengths(List<PlanDto> plans, List<LookupDto> spotLenghts)
        {
            SpotLengths = plans
                            .Select(x => new { spotLenghts.Single(y => y.Id == x.SpotLengthId).Display, x.Equivalized })
                            .Distinct()
                            .OrderBy(x => int.Parse(x.Display))
                            .Select(x => $":{x.Display}{(x.Equivalized ? " eq." : string.Empty)}")
                            .ToList();
        }

        private void _SetGuaranteeedDemo(List<PlanAudienceDisplay> guaranteedDemos, List<PlanAudienceDisplay> orderedAudiences)
        {
            var orderedAudiencesId = orderedAudiences.Select(x => x.Id).ToList();
            GuaranteedDemo = guaranteedDemos
                            .OrderBy(x => orderedAudiencesId.IndexOf(x.Id))
                            .Select(x => x.Code)
                            .ToList();
        }

        private void _SetCampaignFlightDate(List<PlanDto> plans)
        {
            var minStartDate = plans.Select(x => x.FlightStartDate).Min();
            var maxEndDate = plans.Select(x => x.FlightEndDate).Max();
            CampaignFlightStartDate = minStartDate != null ? minStartDate.Value.ToString(DATE_FORMAT_SHORT_YEAR) : string.Empty;
            CampaignFlightEndDate = maxEndDate != null ? maxEndDate.Value.ToString(DATE_FORMAT_SHORT_YEAR) : string.Empty;
        }

        private class ProjectedPlan
        {
            public int QuarterNumber { get; set; }
            public int QuarterYear { get; set; }
            public string DaypartCode { get; set; }
            public List<string> GuaranteedDemo { get; set; }
            public string SpotLength { get; set; }
            public int SpotLengthId { get; set; }
            public bool Equivalized { get; set; }

            public decimal TotalCost { get; set; }
            public int Units { get; set; }


            public double TotalHHRatingPoints { get; set; }
            public double TotalHHImpressions { get; set; }
            public decimal HHCPM { get; set; }
            public decimal HHCPP { get; set; }


            public int AudienceId { get; set; }
            public double VPVH { get; set; }
            public double TotalRatingPoints { get; set; }
            public double TotalImpressions { get; set; }
            public decimal CPM { get; set; }
            public decimal CPP { get; set; }
        }
    }

    public class ProposalQuarterTableData
    {
        public string QuarterLabel { get; set; }
        public List<ProposalQuarterTableRowData> Rows { get; set; }
        public decimal TotalCost { get; set; }
        public int TotalUnits { get; set; }
        public ProposalQuarterTotalTableData TotalHHData { get; set; }
        public ProposalQuarterTotalTableData TotalGuaranteeedData { get; set; }
    }

    public class ProposalQuarterTableRowData
    {
        public string DaypartCode { get; set; }
        public string SpotLength { get; set; }
        public int Units { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }

        public ProposalQuarterAudienceData HHAudienceData { get; set; }
        public ProposalQuarterGuaranteedAudienceData GuaranteedAudienceData { get; set; }
    }

    public class ProposalQuarterTotalTableData
    {
        public double TotalRatingPoints { get; set; }
        public double TotalImpressions { get; set; }
        public decimal TotalCPM { get; set; }
        public decimal TotalCPP { get; set; }
    }

    public class ProposalQuarterAudienceData
    {
        public double RatingPoints { get; set; }
        public double TotalRatingPoints { get; set; }
        public double Impressions { get; set; }
        public double TotalImpressions { get; set; }
        public decimal CPM { get; set; }
        public decimal CPP { get; set; }

    }
    public class ProposalQuarterGuaranteedAudienceData : ProposalQuarterAudienceData
    {
        public double VPVH { get; set; }
    }

    public class MarketCoverageData
    {
        public List<string> BlackoutMarketsName { get; set; }
        public List<string> PreferentialMarketsName { get; set; }
        public int CoveragePercentage { get; set; }
    }

    public class DaypartData
    {
        public string DaypartCode { get; set; }
        public string DaysLabel { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
    }
}
