using Common.Services.Extensions;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using static Services.Broadcast.Entities.Campaign.ProposalQuarterTableData;

namespace Services.Broadcast.Entities.Campaign
{
    public class CampaignReportData
    {
        public string CampaignExportFileName { get; set; }
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
        public List<ProposalQuarterTableData> ProposalQuarterTables { get; set; } = new List<ProposalQuarterTableData>();
        public ProposalQuarterTableData ProposalCampaignTotalsTable { get; set; } = new ProposalQuarterTableData();
        public MarketCoverageData MarketCoverageData { get; set; }
        public List<DaypartData> DaypartsData { get; set; } = new List<DaypartData>();
        public List<string> FlightHiatuses { get; set; } = new List<string>();
        public string Notes { get; set; }
        public List<FlowChartQuarterTableData> FlowChartQuarterTables { get; set; } = new List<FlowChartQuarterTableData>();
        public List<ContractQuarterTableData> ContractQuarterTables { get; set; } = new List<ContractQuarterTableData>();
        public List<object> ContractTotals { get; set; } = new List<object>();
        public bool HasSecondaryAudiences { get; set; }

        private const string DATE_FORMAT_SHORT_YEAR = "MM/dd/yy";
        private const string DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT = "M/d/yy";
        private const string DATE_FORMAT_NO_YEAR_SINGLE_DIGIT = "M/d";
        private const string DATE_FORMAT_FILENAME = "MM-dd";
        private const string FILENAME_FORMAT = "{0} {1} {2} Plan Rev - {3}.xlsx";
        private const string NO_VALUE_CELL = "-";
        private const object EMPTY_CELL = null;
        private const string ADU = "ADU";

        private readonly IWeeklyBreakdownEngine _WeeklyBreakdownEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly List<LookupDto> _AllSpotLengths;
        private static List<DaypartDefaultDto> _DaypartDefaultList;

        public CampaignReportData(CampaignExportTypeEnum exportType, CampaignDto campaign
            , List<PlanDto> plans, AgencyDto agency, AdvertiserDto advertiser
            , List<PlanAudienceDisplay> guaranteedDemos
            , List<LookupDto> spotLengths
            , List<DaypartDefaultDto> daypartDefaults
            , List<PlanAudienceDisplay> orderedAudiences
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IQuarterCalculationEngine quarterCalculationEngine
            , IDateTimeEngine dateTimeEngine
            , IWeeklyBreakdownEngine weeklyBreakdownEngine)
        {
            _WeeklyBreakdownEngine = weeklyBreakdownEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _QuarterCalculationEngine = quarterCalculationEngine;

            _AllSpotLengths = spotLengths;
            _DaypartDefaultList = daypartDefaults;
            HasSecondaryAudiences = plans.Any(x => x.SecondaryAudiences.Any());

            List<PlanProjectionForCampaignExport> projectedPlans = _ProjectPlansForProposalExport(plans);
            _PopulateHeaderData(exportType, campaign, plans, agency, advertiser
                , guaranteedDemos, orderedAudiences, dateTimeEngine);

            List<DateTime> hiatusDays = plans.SelectMany(x => x.FlightHiatusDays).ToList();
            hiatusDays = hiatusDays.Distinct().ToList();

            //proposal tab
            _PopulateProposalQuarterTableData(projectedPlans, orderedAudiences);
            _PopulateProposalTotalsTable();

            _PopulateMarketCoverate(plans);
            _PopulateDaypartsData(projectedPlans);
            _PopulateContentRestrictions(plans);
            _PopulateFlightHiatuses(hiatusDays);
            _PopulateNotes();

            //flow chart tab
            projectedPlans = _ProjectPlansForQuarterExport(plans);
            _PopulateFlowChartQuarterTableData(projectedPlans, plans);

            if (exportType.Equals(CampaignExportTypeEnum.Contract))
            {
                _PopulateContractQuarterTableData(
                    projectedPlans,
                    plans);
            }

            _SetExportFileName(projectedPlans, campaign.ModifiedDate);
        }

        private void _PopulateNotes()
        {
            Notes = "All CPMs are derived from 100% broadcast deliveries, no cable unless otherwise noted.";
        }

        private void _PopulateFlightHiatuses(List<DateTime> plansHiatuses)
        {
            plansHiatuses.Sort();
            foreach (var group in plansHiatuses.GroupConnectedItems((a, b) => (b - a).Days > 1))
            {
                if (group.Count() == 1)
                {
                    FlightHiatuses.Add(group.First().ToString(DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT));
                }
                else
                {
                    FlightHiatuses.Add($"{group.First().ToString(DATE_FORMAT_NO_YEAR_SINGLE_DIGIT)}-{group.Last().ToString(DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT)}");
                }
            }
        }

        private void _PopulateDaypartsData(List<PlanProjectionForCampaignExport> planProjections)
        {
            DaypartsData = planProjections.Select(x => new DaypartData
            {
                DaypartCodeId = x.DaypartCodeId,
                DaypartCode = x.DaypartCode,
                EndTime = x.DaypartEndTime,
                StartTime = x.DaypartStartTime,
                FlightDays = GroupHelper.GroupWeekDays(x.FlightDays)
            }).Distinct(new DaypartDataEqualityComparer()).ToList().OrderDayparts();
        }

        private void _PopulateContentRestrictions(List<PlanDto> plans)
        {
            foreach (var plan in plans)
            {
                string flightDaysString = GroupHelper.GroupWeekDays(plan.FlightDays);
                foreach (var daypart in plan.Dayparts
                    .Where(x => x.Restrictions.GenreRestrictions != null && x.Restrictions.GenreRestrictions.Genres.Any()).ToList())
                {
                    var mappedDaypart = DaypartsData.Single(x => x.DaypartCodeId == daypart.DaypartCodeId
                                            && x.FlightDays.Equals(flightDaysString)
                                            && DaypartTimeHelper.ConvertFormattedTimeToSeconds(x.StartTime) == daypart.StartTimeSeconds
                                            && DaypartTimeHelper.ConvertFormattedTimeToSeconds(x.EndTime) == daypart.EndTimeSeconds);
                    var planDaypart = plan.Dayparts.Single(x => x.DaypartCodeId == daypart.DaypartCodeId);
                    mappedDaypart.GenreRestrictions.Add(new DaypartRestrictionsData
                    {
                        PlanId = plan.Id,
                        ContainType = planDaypart.Restrictions.GenreRestrictions.ContainType,
                        Restrictions = planDaypart.Restrictions.GenreRestrictions.Genres.Select(x => x.Display).ToList()
                    });
                }
                foreach (var daypart in plan.Dayparts
                    .Where(x => x.Restrictions.ProgramRestrictions != null && x.Restrictions.ProgramRestrictions.Programs.Any()).ToList())
                {
                    var mappedDaypart = DaypartsData.Single(x => x.DaypartCodeId == daypart.DaypartCodeId
                                && x.FlightDays.Equals(flightDaysString)
                                && DaypartTimeHelper.ConvertFormattedTimeToSeconds(x.StartTime) == daypart.StartTimeSeconds
                                && DaypartTimeHelper.ConvertFormattedTimeToSeconds(x.EndTime) == daypart.EndTimeSeconds);
                    var planDaypart = plan.Dayparts.Single(x => x.DaypartCodeId == daypart.DaypartCodeId);
                    mappedDaypart.ProgramRestrictions.Add(new DaypartRestrictionsData
                    {
                        PlanId = plan.Id,
                        ContainType = planDaypart.Restrictions.ProgramRestrictions.ContainType,
                        Restrictions = planDaypart.Restrictions.ProgramRestrictions.Programs.Select(x => x.Name).Distinct().ToList()
                    });
                }
            }
        }

        private void _PopulateMarketCoverate(List<PlanDto> plans)
        {
            MarketCoverageData = new MarketCoverageData
            {
                PreferentialMarketsName = plans.SelectMany(x => x.AvailableMarkets.Where(y => y.ShareOfVoicePercent != null))
                            .OrderByDescending(x => x.ShareOfVoicePercent.Value)
                            .Select(x => x.Market)
                            .ToList(),
                BlackoutMarketsName = plans.SelectMany(x => x.BlackoutMarkets)
                            .OrderBy(x => x.Rank)
                            .Select(x => x.Market)
                            .ToList(),
                CoveragePercentage = plans.Where(x => x.CoverageGoalPercent.HasValue)
                            .Select(x => x.CoverageGoalPercent.Value)
                            .Max()
            };
        }

        private void _SetExportFileName(List<PlanProjectionForCampaignExport> planProjections, DateTime campaignModifiedDate)
        {
            CampaignExportFileName = string.Format(FILENAME_FORMAT
                , ClientName
                , string.Join(" ", planProjections.Select(x => x.DaypartCode).Distinct().OrderBy(x => x).ToList())
                , (CampaignEndQuarter != CampaignStartQuarter ? $"{CampaignStartQuarter}-{CampaignEndQuarter}" : CampaignStartQuarter)
                , campaignModifiedDate.ToString(DATE_FORMAT_FILENAME));
        }

        private List<PlanProjectionForCampaignExport> _ProjectPlansForProposalExport(List<PlanDto> plans)
        {
            List<PlanProjectionForCampaignExport> result = new List<PlanProjectionForCampaignExport>();
            plans.ForEach(plan =>
            {
                List<QuarterDetailDto> planQuarters = _QuarterCalculationEngine.GetAllQuartersBetweenDates(plan.FlightStartDate.Value, plan.FlightEndDate.Value);
                var allSpotLengthIdAndStandardDaypartIdCombinations =
                     _WeeklyBreakdownEngine.GetWeeklyBreakdownCombinations(plan.CreativeLengths, plan.Dayparts);
                foreach (var quarter in planQuarters)
                {
                    //store the week ids from the current quarter in a list
                    List<int> quarterTotalWeeksIdList = _MediaMonthAndWeekAggregateCache.GetMediaWeeksByFlight(quarter.StartDate, quarter.EndDate).Select(x => x.Id).ToList();
                    //all the plan weeks in current quarter
                    var quarterPlanWeeks = plan.WeeklyBreakdownWeeks.Where(x => quarterTotalWeeksIdList.Contains(x.MediaWeekId));
                    foreach (var week in quarterPlanWeeks)
                    {
                        var newProjectedPlan = _GetEmptyWeek(null, plan, quarter, null,
                            _AllSpotLengths.Single(x => x.Id == week.SpotLengthId),
                            plan.Dayparts.Where(x => x.DaypartCodeId == week.DaypartCodeId).Single());

                        newProjectedPlan.TotalCost = week.WeeklyBudget;
                        newProjectedPlan.Units = _CalculateUnitsForWeek(week, allSpotLengthIdAndStandardDaypartIdCombinations);

                        _ProjectGuaranteedAudienceDataByWeek(plan, week, newProjectedPlan);
                        _ProjectHHAudienceData(plan, week, newProjectedPlan);
                        _ProjectSecondaryAudiencesData(plan, week, newProjectedPlan, newProjectedPlan.TotalHHImpressions);

                        result.Add(newProjectedPlan);
                    }
                }
            });

            return result;
        }

        private static void _ProjectHHAudienceData(PlanDto plan, WeeklyBreakdownWeek planWeek, PlanProjectionForCampaignExport planProjection)
        {
            var audienceImpressions = planWeek.WeeklyImpressions;
            var audienceVpvh = _GetVpvhByStandardDaypartAndAudience(plan, planWeek.DaypartCodeId.Value, plan.AudienceId);
            planProjection.TotalHHImpressions = ProposalMath.CalculateHhImpressionsUsingVpvh(audienceImpressions, audienceVpvh);

            var factor = planProjection.TotalHHImpressions / plan.HHImpressions;
            planProjection.TotalHHRatingPoints = plan.HHRatingPoints * factor;
        }

        private static double _GetVpvhByStandardDaypartAndAudience(PlanDto plan, int standardDaypartId, int audienceId)
        {
            return plan.Dayparts.Single(x => x.DaypartCodeId == standardDaypartId).VpvhForAudiences.Single(x => x.AudienceId == audienceId).Vpvh;
        }

        private static void _ProjectGuaranteedAudienceDataByWeek(PlanDto plan, WeeklyBreakdownWeek planWeek, PlanProjectionForCampaignExport projection)
        {
            projection.GuaranteedAudience.IsGuaranteedAudience = true;
            projection.GuaranteedAudience.AudienceId = plan.AudienceId;
            projection.GuaranteedAudience.WeightedPercentage = planWeek.WeeklyImpressions / plan.TargetImpressions.Value;
            projection.GuaranteedAudience.TotalImpressions = planWeek.WeeklyImpressions;
            projection.GuaranteedAudience.TotalRatingPoints = planWeek.WeeklyRatings;
        }

        private static void _ProjectSecondaryAudiencesData(
            PlanDto plan,
            WeeklyBreakdownWeek planWeek,
            PlanProjectionForCampaignExport newProjectedPlan,
            double hhImpressions)
        {
            foreach (var audience in plan.SecondaryAudiences)
            {
                var audienceVpvh = _GetVpvhByStandardDaypartAndAudience(plan, planWeek.DaypartCodeId.Value, audience.AudienceId);
                var audienceImpressions = ProposalMath.CalculateAudienceImpressionsUsingVpvh(hhImpressions, audienceVpvh);
                var factor = audienceImpressions / audience.Impressions.Value;

                newProjectedPlan.SecondaryAudiences.Add(
                    new PlanProjectionForCampaignExport.Audience
                    {
                        AudienceId = audience.AudienceId,
                        TotalImpressions = audienceImpressions,
                        TotalRatingPoints = audience.RatingPoints.Value * factor
                    });
            }
        }

        //flow chart and contract tabs
        private List<PlanProjectionForCampaignExport> _ProjectPlansForQuarterExport(List<PlanDto> plans)
        {
            List<PlanProjectionForCampaignExport> result = new List<PlanProjectionForCampaignExport>();

            foreach (var plan in plans)
            {
                List<QuarterDetailDto> planQuarters = _QuarterCalculationEngine.GetAllQuartersBetweenDates(plan.FlightStartDate.Value
                    , plan.FlightEndDate.Value);
                var allSpotLengthIdAndStandardDaypartIdCombinations =
                     _WeeklyBreakdownEngine.GetWeeklyBreakdownCombinations(plan.CreativeLengths, plan.Dayparts);

                foreach (var quarter in planQuarters)
                {
                    var quarterMediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthsBetweenDatesInclusive(quarter.StartDate, quarter.EndDate);

                    foreach (var week in _MediaMonthAndWeekAggregateCache.GetMediaWeeksInRange(quarter.StartDate, quarter.EndDate))
                    {
                        //not all weeks are in the plan, but we need all of them in the report
                        if (!plan.WeeklyBreakdownWeeks.Where(x => x.MediaWeekId == week.Id).Any())
                        {
                            result.AddRange(_AddEmptyWeeksForPlanSpotLengthDaypartCombinations(week, plan, quarter, quarterMediaMonth));
                            continue;
                        }
                        foreach (var planWeek in plan.WeeklyBreakdownWeeks.Where(x => x.MediaWeekId == week.Id))
                        {
                            var newProjectedPlan = _GetEmptyWeek(week, plan, quarter, quarterMediaMonth
                                , _AllSpotLengths.Single(x => x.Id == planWeek.SpotLengthId)
                                , plan.Dayparts.Single(x => x.DaypartCodeId == planWeek.DaypartCodeId));
                            newProjectedPlan.TotalCost = planWeek.WeeklyBudget;
                            newProjectedPlan.Units = _CalculateUnitsForWeek(planWeek, allSpotLengthIdAndStandardDaypartIdCombinations);

                            //guaranteed audience data
                            _ProjectGuaranteedAudienceDataByWeek(plan, planWeek, newProjectedPlan);
                            newProjectedPlan.HiatusDays = plan.FlightHiatusDays.Where(x => x.Date >= week.StartDate && x.Date <= week.EndDate).ToList();

                            result.Add(newProjectedPlan);
                        }
                    }
                }
            }

            return result;
        }

        private static double _CalculateUnitsForWeek(WeeklyBreakdownWeek planWeek, List<WeeklyBreakdownCombination> combinations)
        {
            var combination = combinations.Where(x => x.DaypartCodeId == planWeek.DaypartCodeId && x.SpotLengthId == planWeek.SpotLengthId)
                                .Single();
            return planWeek.WeeklyImpressions / planWeek.UnitImpressions * combination.Weighting;
        }

        private List<PlanProjectionForCampaignExport> _AddEmptyWeeksForPlanSpotLengthDaypartCombinations(MediaWeek mediaWeek, PlanDto plan,
            QuarterDetailDto quarter, List<MediaMonth> mediaMonths)
        {
            List<PlanProjectionForCampaignExport> result = new List<PlanProjectionForCampaignExport>();
            foreach (var creativeLength in plan.CreativeLengths)
            {
                result.AddRange(plan.Dayparts.Select(x => _GetEmptyWeek(mediaWeek, plan, quarter, mediaMonths,
                _AllSpotLengths.Single(y => y.Id == creativeLength.SpotLengthId), x)).ToList());
            }
            return result;
        }

        //Returns a ProjectedPlan object that will contain media and plan data without imps and cost data
        private static PlanProjectionForCampaignExport _GetEmptyWeek(MediaWeek mediaWeek, PlanDto plan, QuarterDetailDto quarter
            , List<MediaMonth> mediaMonths, LookupDto spotLength, PlanDaypartDto daypart)
        {
            const string SpotLength30Seconds = "30";
            return new PlanProjectionForCampaignExport
            {
                QuarterYear = quarter.Year,
                QuarterNumber = quarter.Quarter,
                MediaWeekId = mediaWeek?.Id,
                MediaMonthId = mediaWeek?.MediaMonthId,
                WeekStartDate = mediaWeek?.StartDate,
                MediaMonthName = mediaWeek != null ? mediaMonths.Single(y => y.Id == mediaWeek.MediaMonthId).LongMonthName : null,
                DaypartCodeId = daypart.DaypartCodeId,
                DaypartCode = _DaypartDefaultList.Single(y => y.Id == daypart.DaypartCodeId).Code,
                DaypartStartTime = DaypartTimeHelper.ConvertSecondsToFormattedTime(daypart.StartTimeSeconds, "hh:mmtt"),
                DaypartEndTime = DaypartTimeHelper.ConvertSecondsToFormattedTime(daypart.EndTimeSeconds, "hh:mmtt"),
                SpotLengthId = spotLength.Id,
                SpotLength = spotLength.Display,
                //we set false for 30s on plans equivalized to group all the data together
                Equivalized = spotLength.Display.Equals(SpotLength30Seconds) ? false : plan.Equivalized,
                FlightDays = plan.FlightDays
            };
        }

        private void _PopulateProposalQuarterTableData(List<PlanProjectionForCampaignExport> planProjections, List<PlanAudienceDisplay> audiences)
        {
            planProjections.GroupBy(x => new { x.QuarterNumber, x.QuarterYear })
                .OrderBy(x => x.Key.QuarterYear).ThenBy(x => x.Key.QuarterNumber)
                .ToList()
                .ForEach(quarterGroup =>
                {
                    List<PlanProjectionForCampaignExport.Audience> secondaryAudiences = quarterGroup
                        .SelectMany(x => x.SecondaryAudiences)
                        .Distinct(new PlanProjectionForCampaignExport.AudienceEqualityComparer()).ToList();
                    ProposalQuarterTableData newTable = new ProposalQuarterTableData
                    {
                        QuarterLabel = $"Q{quarterGroup.Key.QuarterNumber} {quarterGroup.Key.QuarterYear}"
                    };
                    foreach (var demo in secondaryAudiences)
                    {
                        var secondaryTable = new SecondayAudienceTable
                        {
                            AudienceId = demo.AudienceId,
                            AudienceCode = audiences.Single(x => x.Id == demo.AudienceId).Code
                        };
                        newTable.SecondaryAudiencesTables.Add(secondaryTable);
                    }

                    quarterGroup
                    .GroupBy(x => new { x.DaypartCode, x.SpotLength, x.Equivalized })
                    .OrderBy(x => x.Key.DaypartCode).ThenBy(x => Convert.ToInt32(x.Key.SpotLength)).ThenByDescending(x => x.Key.Equivalized)
                    .ToList()
                    .ForEach(daypartGroup =>
                    {
                        var items = daypartGroup.ToList();
                        double unitsSum = items.Sum(x => x.Units);
                        decimal totalCost = items.Sum(x => x.TotalCost);
                        double totalRatingPoints = items.Sum(x => x.GuaranteedAudience.TotalRatingPoints);
                        double totalImpressions = items.Sum(x => x.GuaranteedAudience.TotalImpressions) / 1000;
                        double totalHHRatingPoints = items.Sum(x => x.TotalHHRatingPoints);
                        double totalHHImpressions = items.Sum(x => x.TotalHHImpressions) / 1000;
                        string spotLengthLabel = $"{daypartGroup.Key.SpotLength}{_GetEquivalizedStatus(daypartGroup.Key.Equivalized, daypartGroup.Key.SpotLength)}";
                        var row = new ProposalQuarterTableRowData
                        {
                            DaypartCode = daypartGroup.Key.DaypartCode,
                            SpotLengthLabel = spotLengthLabel,
                            Units = unitsSum,
                            UnitsCost = _CalculateCost(unitsSum, totalCost),
                            TotalCost = totalCost,

                            //HH data
                            HHData = new AudienceData
                            {
                                RatingPoints = (unitsSum == 0 ? 0 : totalHHRatingPoints / unitsSum),
                                TotalRatingPoints = totalHHRatingPoints,
                                Impressions = (unitsSum == 0 ? 0 : totalHHImpressions / unitsSum),
                                TotalImpressions = totalHHImpressions,
                                CPM = _CalculateCost(totalHHImpressions, totalCost),
                                CPP = _CalculateCost(totalHHRatingPoints, totalCost),

                            },
                            GuaranteedData = new AudienceData
                            {
                                VPVH = ProposalMath.CalculateVpvh(totalImpressions, totalHHImpressions),
                                RatingPoints = (unitsSum == 0 ? 0 : totalRatingPoints / unitsSum),
                                TotalRatingPoints = totalRatingPoints,
                                Impressions = (unitsSum == 0 ? 0 : totalImpressions / unitsSum),
                                TotalImpressions = totalImpressions,
                                CPM = _CalculateCost(totalImpressions, totalCost),
                                CPP = _CalculateCost(totalRatingPoints, totalCost)
                            }
                        };
                        if (secondaryAudiences.Any())   //this check is for the current group 
                        {
                            foreach (var demo in secondaryAudiences)
                            {
                                var demoTable = newTable.SecondaryAudiencesTables.Single(x => x.AudienceId == demo.AudienceId);
                                var demoData = items.SelectMany(x => x.SecondaryAudiences).Where(x => x.AudienceId == demo.AudienceId).ToList();
                                double totalRatingPointsForDemo = demoData.Sum(x => x.TotalRatingPoints);
                                double totalImpressionsForDemo = demoData.Sum(x => x.TotalImpressions) / 1000;
                                demoTable.Rows.Add(
                                    new AudienceData
                                    {
                                        DaypartCode = daypartGroup.Key.DaypartCode,
                                        SpotLengthLabel = spotLengthLabel,
                                        VPVH = ProposalMath.CalculateVpvh(totalImpressionsForDemo, totalHHImpressions),
                                        RatingPoints = (unitsSum == 0 ? 0 : totalRatingPointsForDemo / unitsSum),
                                        TotalRatingPoints = totalRatingPointsForDemo,
                                        Impressions = (unitsSum == 0 ? 0 : totalImpressionsForDemo / unitsSum),
                                        TotalImpressions = totalImpressionsForDemo,
                                        CPM = _CalculateCost(totalImpressionsForDemo, totalCost),
                                        CPP = _CalculateCost(totalRatingPointsForDemo, totalCost)
                                    }
                                );
                            }
                        }
                        newTable.Rows.Add(row);

                    });
                    _SetTableTotals(newTable);
                    ProposalQuarterTables.Add(newTable);
                });
        }

        //this is total on each table
        private void _CalculateTotalsForFlowChartTable(FlowChartQuarterTableData tableData)
        {
            tableData.DistributionPercentages.Add(tableData.DistributionPercentages.DoubleSumOrDefault());
            tableData.UnitsValues.Add(tableData.UnitsValues.Sum(x => Convert.ToDouble(x)));
            double impressions = tableData.ImpressionsValues.Sum(x => Convert.ToDouble(x));
            tableData.ImpressionsValues.Add(impressions);
            decimal cost = tableData.CostValues.Sum(x => Convert.ToDecimal(x));
            tableData.CostValues.Add(cost);
            tableData.CPMValues.Add(_CalculateCost(impressions, cost));
            var hiatusDaysCount = tableData.HiatusDays.SelectMany(x => x).Count();
            tableData.HiatusDaysFormattedValues.Add(hiatusDaysCount == 0 ? (int?)null : hiatusDaysCount);
        }

        //this is total on adu table
        private void _CalculateTotalsForAduTable(FlowChartQuarterTableData tableData)
        {
            int aduUnitsNumber = tableData.UnitsValues.Sum(x => Convert.ToInt32(x));
            if (aduUnitsNumber > 0)
            {
                tableData.DistributionPercentages.Add("-");
                tableData.UnitsValues.Add(aduUnitsNumber);
                tableData.ImpressionsValues.Add(ADU);
                tableData.CostValues.Add(ADU);
                tableData.CPMValues.Add(ADU);
            }
            var hiatusDaysCount = tableData.HiatusDays.SelectMany(x => x).Count();
            tableData.HiatusDaysFormattedValues.Add(hiatusDaysCount == 0 ? (int?)null : hiatusDaysCount);
        }

        //this is the total quarter daypart table
        private void _CalculateTotalTableData(List<FlowChartQuarterTableData> tablesInQuarterDaypart
            , double totalImpressions)
        {
            var firstTable = tablesInQuarterDaypart.First();
            var tableData = new FlowChartQuarterTableData
            {
                TableTitle = $"{firstTable.QuarterLabel} {firstTable.DaypartCode} Total",
                WeeksStartDate = firstTable.WeeksStartDate,
                Months = firstTable.Months
            };
            for (int i = 0; i < tableData.TotalWeeksInQuarter; i++)
            {
                tableData.UnitsValues.Add(tablesInQuarterDaypart.Sum(x => Convert.ToDouble(x.UnitsValues[i])));
                double impressions = tablesInQuarterDaypart.Sum(x => Convert.ToDouble(x.ImpressionsValues[i]));
                tableData.ImpressionsValues.Add(impressions);
                decimal cost = tablesInQuarterDaypart.Sum(x => Convert.ToDecimal(x.CostValues[i]));
                tableData.CostValues.Add(cost);
                tableData.CPMValues.Add(_CalculateCost(impressions, cost));

                var hiatusDaysThisWeek = tablesInQuarterDaypart.SelectMany(x => x.HiatusDays[i]).ToList();
                hiatusDaysThisWeek = hiatusDaysThisWeek.Distinct().ToList();
                tableData.HiatusDays.Add(hiatusDaysThisWeek);
                tableData.HiatusDaysFormattedValues.Add(_GetHiatusDaysFormattedForWeek(hiatusDaysThisWeek));

                //multiply impressions by 1000 to get the raw number because the total impressions are in raw format
                var distributionPercentage = totalImpressions == 0
                    ? 0
                    : (impressions * 1000) / totalImpressions; //don't multiply by 100. excel is doing that as part of the cell format
                tableData.DistributionPercentages.Add(distributionPercentage);
            }

            _CalculateTotalsForFlowChartTable(tableData);
            FlowChartQuarterTables.Add(tableData);
        }

        //this is the adu table for a quarter in flow chart table
        private void _CalculateAduTableData(
            List<FlowChartQuarterTableData> tables,
            List<PlanDto> plans)
        {
            var firstTable = tables.First();
            //check if we have an ADU table for this quarter
            var weeksStartDates = firstTable.WeeksStartDate.Select(w => Convert.ToDateTime(w));
            var aduImpressionsInQuarter = plans.Sum(x => x.WeeklyBreakdownWeeks.Where(y => weeksStartDates.Contains(y.StartDate)).Sum(y => y.AduImpressions));

            if (aduImpressionsInQuarter <= 0)
            {//return if no week in the quarter has ADU impressions
                return;
            }

            var tableData = new FlowChartQuarterTableData
            {
                TableTitle = $"{firstTable.QuarterLabel} {ADU}",
                WeeksStartDate = firstTable.WeeksStartDate
            };
            tableData.Months = firstTable.Months;
            for (int i = 0; i < firstTable.WeeksStartDate.Count; i++)
            {
                var thisWeekADUUnits = plans
                    .Sum(plan =>
                    {
                        double sum = 0;
                        plan.CreativeLengths.ForEach(createiveLength =>
                        {
                            sum += _WeeklyBreakdownEngine.CalculateADUWithDecimals(
                            plan.ImpressionsPerUnit
                            , plan.WeeklyBreakdownWeeks.Where(w => w.StartDate.Equals(Convert.ToDateTime(firstTable.WeeksStartDate[i]))
                                                             && w.SpotLengthId == createiveLength.SpotLengthId)
                                                    .Sum(y => y.AduImpressions)
                            , plan.Equivalized
                            , createiveLength.SpotLengthId);
                        });
                        return sum;
                    });
                if (thisWeekADUUnits > 0)
                {
                    tableData.DistributionPercentages.Add(EMPTY_CELL);
                    tableData.UnitsValues.Add(thisWeekADUUnits);
                    tableData.ImpressionsValues.Add(ADU);
                    tableData.CostValues.Add(ADU);
                    tableData.CPMValues.Add(ADU);
                }
                else
                {
                    tableData.DistributionPercentages.Add(EMPTY_CELL);
                    tableData.UnitsValues.Add(EMPTY_CELL);
                    tableData.ImpressionsValues.Add(EMPTY_CELL);
                    tableData.CostValues.Add(EMPTY_CELL);
                    tableData.CPMValues.Add(EMPTY_CELL);
                }
                var hiatusDaysThisWeek = tables.SelectMany(x => x.HiatusDays[i]).ToList();
                hiatusDaysThisWeek = hiatusDaysThisWeek.Distinct().ToList();

                tableData.HiatusDays.Add(hiatusDaysThisWeek);
                tableData.HiatusDaysFormattedValues.Add(_GetHiatusDaysFormattedForWeek(hiatusDaysThisWeek));
            }

            _CalculateTotalsForAduTable(tableData);
            FlowChartQuarterTables.Add(tableData);
        }

        private void _PopulateFlowChartQuarterTableData(
            List<PlanProjectionForCampaignExport> planProjections,
            List<PlanDto> plans)
        {
            var totalNumberOfImpressionsForExportedPlans = planProjections.Sum(x => x.GuaranteedAudience.TotalImpressions);
            planProjections.GroupBy(x => new { x.QuarterNumber, x.QuarterYear })
                .OrderBy(x => x.Key.QuarterYear).ThenBy(x => x.Key.QuarterNumber)
                .ToList()
                .ForEach(quarterGroup =>
                {
                    QuarterDetailDto quarter = new QuarterDetailDto
                    {
                        Quarter = quarterGroup.Key.QuarterNumber,
                        Year = quarterGroup.Key.QuarterYear
                    };

                    List<FlowChartQuarterTableData> tablesInQuarter = new List<FlowChartQuarterTableData>();

                    var quarterItems = quarterGroup.ToList();
                    quarterItems.GroupBy(y => y.DaypartCode).OrderBy(x => x.Key)
                    .ToList()
                    .ForEach(daypartGroup =>  //quarter daypart spot length equivalized group
                    {
                        List<FlowChartQuarterTableData> tablesInQuarterDaypart = new List<FlowChartQuarterTableData>();
                        daypartGroup.ToList().GroupBy(y => new { y.SpotLength, y.Equivalized })
                            .ToList()
                            .ForEach(daypartSpotLengthGroup =>
                            {
                                FlowChartQuarterTableData newTable = new FlowChartQuarterTableData
                                {
                                    TableTitle = $"{quarter.ShortFormatQuarterNumberFirst()} {daypartGroup.Key} :{daypartSpotLengthGroup.Key.SpotLength}s{_GetEquivalizedStatus(daypartSpotLengthGroup.Key.Equivalized, daypartSpotLengthGroup.Key.SpotLength)}",
                                    DaypartCode = daypartGroup.Key,
                                    QuarterLabel = quarter.ShortFormatQuarterNumberFirst()
                                };
                                daypartSpotLengthGroup.GroupBy(x => new { x.MediaMonthName, x.MediaMonthId })
                                    .ToList()
                                    .ForEach(monthGroup =>
                                    {
                                        //calculate how many weeks does this month have
                                        var weeksInMonth = _MediaMonthAndWeekAggregateCache.GetMediaWeeksByMediaMonth(monthGroup.Key.MediaMonthId.Value).Count();
                                        //add the tuple to the Month array
                                        newTable.Months.Add((monthGroup.Key.MediaMonthName, weeksInMonth));
                                        var monthItems = monthGroup.ToList();

                                        monthItems
                                                .GroupBy(x => x.WeekStartDate)
                                                .ToList()
                                                .ForEach(x =>
                                                {
                                                    var itemsThisWeek = x.ToList();
                                                    List<DateTime> hiatusDaysThisWeek = itemsThisWeek.SelectMany(y => y.HiatusDays).ToList();
                                                    hiatusDaysThisWeek = hiatusDaysThisWeek.Distinct().ToList();
                                                    newTable.WeeksStartDate.Add(x.Key.Value);

                                                    //the distribution percentage is the percent of all the impressions from this week 
                                                    //from all the plans in this table in the total number of impressions 
                                                    //for all the exported plans
                                                    var distributionPercentage = totalNumberOfImpressionsForExportedPlans == 0
                                                        ? 0
                                                        : itemsThisWeek.Sum(y => y.GuaranteedAudience.TotalImpressions)
                                                                    / totalNumberOfImpressionsForExportedPlans; //do not multiply by 100. excel is doing that.
                                                    newTable.DistributionPercentages.Add(distributionPercentage);
                                                    newTable.UnitsValues.Add(itemsThisWeek.Sum(y => y.Units));
                                                    var impressions = itemsThisWeek.Sum(y => y.GuaranteedAudience.TotalImpressions) / 1000;
                                                    newTable.ImpressionsValues.Add(impressions);
                                                    var cost = itemsThisWeek.Sum(y => y.TotalCost);
                                                    newTable.CostValues.Add(cost);
                                                    newTable.CPMValues.Add(_CalculateCost(impressions, cost));
                                                    newTable.HiatusDaysFormattedValues.Add(_GetHiatusDaysFormattedForWeek(hiatusDaysThisWeek));
                                                    newTable.HiatusDays.Add(hiatusDaysThisWeek);
                                                });
                                    });
                                _CalculateTotalsForFlowChartTable(newTable);
                                tablesInQuarterDaypart.Add(newTable);
                            });

                        tablesInQuarter.AddRange(tablesInQuarterDaypart);
                        FlowChartQuarterTables.AddRange(tablesInQuarterDaypart);
                        _CalculateTotalTableData(tablesInQuarterDaypart, totalNumberOfImpressionsForExportedPlans);
                    });

                    _CalculateAduTableData(tablesInQuarter, plans);
                });
        }

        private string _GetHiatusDaysFormattedForWeek(List<DateTime> hiatuseDaysThisWeek)
        {
            List<string> formattedGroups = new List<string>();
            hiatuseDaysThisWeek.Sort();
            foreach (var group in hiatuseDaysThisWeek.GroupConnectedItems((a, b) => (b - a).Days > 1))
            {
                if (group.Count() == 1)
                {
                    formattedGroups.Add(group.First().ToString(DATE_FORMAT_NO_YEAR_SINGLE_DIGIT));
                }
                else
                {
                    formattedGroups.Add($"{group.First().ToString(DATE_FORMAT_NO_YEAR_SINGLE_DIGIT)}-{group.Last().Day}");
                }
            }
            return string.Join(", ", formattedGroups);
        }

        private void _PopulateContractQuarterTableData(
            List<PlanProjectionForCampaignExport> projectedPlans,
            List<PlanDto> plans)
        {
            projectedPlans.GroupBy(x => new { x.QuarterNumber, x.QuarterYear })
                .OrderBy(x => x.Key.QuarterYear).ThenBy(x => x.Key.QuarterNumber)
                .ToList()
                .ForEach(qDGrp =>   //quarter daypart group
                {
                    QuarterDetailDto quarter = _QuarterCalculationEngine
                        .GetQuarterDetail(qDGrp.Key.QuarterNumber, qDGrp.Key.QuarterYear);
                    var quarterTable = new ContractQuarterTableData
                    {
                        Title = quarter.LongFormat(),
                    };
                    qDGrp.ToList().GroupBy(y => new { y.DaypartCode, y.SpotLength, y.Equivalized, y.WeekStartDate })
                    .OrderBy(x => x.Key.WeekStartDate)    //ASC by week
                    .ThenBy(x => x.Key.DaypartCode)      //Alphabetically ASC by daypart code
                    .ThenBy(x => Convert.ToInt32(x.Key.SpotLength))  // ASC by order of time length
                    .ThenByDescending(x => x.Key.Equivalized)  // then by equivalized
                    .ToList()
                    .ForEach(row =>  //quarter daypart spot length equivalized group in the same week
                    {
                        var items = row.ToList();
                        decimal totalCost = items.Sum(y => y.TotalCost);
                        double totalImpressions = items.Sum(y => y.GuaranteedAudience.TotalImpressions) / 1000;
                        if (totalImpressions == 0 && totalCost == 0)
                        {
                            return; //don't add empty weeks
                        }
                        double unitsSum = items.Sum(y => y.Units);

                        List<object> tableRow = new List<object>
                        {
                            row.Key.DaypartCode,
                            row.Key.WeekStartDate.Value.ToShortDateString(),
                            unitsSum,
                            $":{row.Key.SpotLength}s{_GetEquivalizedStatus(row.Key.Equivalized, row.Key.SpotLength)}",
                            _CalculateCost(unitsSum, totalCost),   //cost per unit
                            totalCost,
                            unitsSum == 0 ? 0 : (totalImpressions / unitsSum),    //impressions per unit
                            totalImpressions,
                            _CalculateCost(totalImpressions, totalCost)
                        };

                        quarterTable.Rows.Add(tableRow);
                    });
                    if (quarterTable.Rows.Any())
                    {//don't add tables that don't have any rows because there are no impressions for this quarter
                        _CalculateTotalsForContractTable(quarterTable, quarter.ShortFormatQuarterNumberFirst());
                        ContractQuarterTables.Add(quarterTable);
                    }

                    _CalculateAduTableData(quarter, plans);
                });
            _CalculateContractTabTotals();
        }

        //ADU table data for contract tab
        private void _CalculateAduTableData(
            QuarterDetailDto quarter,
            List<PlanDto> plans)
        {
            List<DateTime> weeksStartDates = _MediaMonthAndWeekAggregateCache
                .GetMediaWeeksByFlight(quarter.StartDate, quarter.EndDate)
                .Select(x => x.StartDate).ToList();

            var aduImpressionsInQuarter = plans.Sum(x => x.WeeklyBreakdownWeeks.Where(y => weeksStartDates.Contains(y.StartDate)).Sum(y => y.AduImpressions));
            if (aduImpressionsInQuarter <= 0)
            {//return if no week in the quarter has ADU
                return;
            }

            ContractQuarterTableData table = new ContractQuarterTableData
            {
                Title = quarter.LongFormat(),
                IsAduTable = true
            };

            var plansAndweeklyBreakdowns = plans.Select(x => new
            {
                Plan = x,
                WeeklyBreakdown = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(x.WeeklyBreakdownWeeks, x.ImpressionsPerUnit, x.CreativeLengths)
            }).ToList();
            List<int> distinctSpotLengthIdsList = plans
                .SelectMany(x => x.CreativeLengths.Select(y => y.SpotLengthId))
                .Distinct()
                .ToList();
            foreach (var startDate in weeksStartDates)
            {
                //continue if this week does not have ADU Impressions
                if (plans.SelectMany(x => x.WeeklyBreakdownWeeks.Where(y => y.StartDate.Equals(startDate))).Sum(x => x.AduImpressions) <= 0)
                {
                    continue;
                }
                foreach (int spotlengthId in distinctSpotLengthIdsList)
                {
                    plans.Select(x => new
                    {
                        Adu = _WeeklyBreakdownEngine.CalculateADUWithDecimals(x.ImpressionsPerUnit
                            , x.WeeklyBreakdownWeeks.Where(y => y.SpotLengthId == spotlengthId && y.StartDate == startDate).Sum(y => y.AduImpressions)
                            , x.Equivalized, spotlengthId),
                        x.Equivalized
                    })
                    .GroupBy(x => new { SpotLength = _AllSpotLengths.Single(w => w.Id == spotlengthId).Display, x.Equivalized })
                    .OrderBy(x => Convert.ToInt32(x.Key.SpotLength))
                    .ThenByDescending(x => x.Key.Equivalized)
                    .ToList()
                    .ForEach(group =>
                    {
                        double weeklyAdu = group.ToList().Sum(y => y.Adu);

                        if (weeklyAdu > 0)
                        {
                            table.Rows.Add(
                                new List<object>
                                {
                                    EMPTY_CELL,
                                    startDate,
                                    weeklyAdu,
                                    $"{group.Key.SpotLength}{_GetEquivalizedStatus(group.Key.Equivalized, group.Key.SpotLength)}",
                                    ADU,
                                    ADU,
                                    ADU,
                                    ADU,
                                    ADU
                                });
                        }
                    });
                }
            }
            _CalculateTotalsForContractADUTable(table, quarter.ShortFormatQuarterNumberFirst());
            ContractQuarterTables.Add(table);
        }

        /// <summary>
        /// Calculates the contract tab totals for all the tables not ADU.
        /// </summary>
        private void _CalculateContractTabTotals()
        {
            ContractTotals = new List<object>
            {
                ContractQuarterTables.Where(x=>!x.IsAduTable).SelectMany(x=>x.Rows).Sum(x=>double.Parse(x[2].ToString())), //units
                ContractQuarterTables.Where(x=>!x.IsAduTable).SelectMany(x=>x.Rows).Sum(x=>decimal.Parse(x[5].ToString())), //total cost
                EMPTY_CELL,
                ContractQuarterTables.Where(x=>!x.IsAduTable).SelectMany(x=>x.Rows).Sum(x=>double.Parse(x[7].ToString())), //total demo
                EMPTY_CELL
            };
            //CPM
            ContractTotals.Add(_CalculateCost(double.Parse(ContractTotals[3].ToString())
                , decimal.Parse(ContractTotals[1].ToString())));
        }

        private void _CalculateTotalsForContractTable(ContractQuarterTableData quarterTable, string quarterLabel)
        {
            decimal totalCost = quarterTable.Rows.Sum(x => decimal.Parse(x[5].ToString()));
            double totalImpressions = quarterTable.Rows.Sum(x => double.Parse(x[7].ToString()));

            quarterTable.TotalRow = new List<object>
            {
                $"{quarterLabel} Totals",
                quarterTable.Rows.Sum(x => Convert.ToDouble(x[2])),    //units
                NO_VALUE_CELL,
                NO_VALUE_CELL,
                totalCost,     //cost
                NO_VALUE_CELL,
                totalImpressions,
                _CalculateCost(totalImpressions, totalCost)
            };
        }

        private void _CalculateTotalsForContractADUTable(ContractQuarterTableData quarterTable, string quarterLabel)
        {
            //units are the third value in the row
            double totalUnits = quarterTable.Rows.Sum(x => Convert.ToDouble(x[2]));

            quarterTable.TotalRow = new List<object>
            {
                $"{quarterLabel} Totals",
                totalUnits,
                NO_VALUE_CELL,
                NO_VALUE_CELL,
                NO_VALUE_CELL,
                NO_VALUE_CELL,
                NO_VALUE_CELL,
                NO_VALUE_CELL
            };
        }

        /// <summary>
        /// Calculate CPM or CPP depending on the value sent in the points property   
        /// </summary>
        /// <param name="points">Impressions or rating points.</param>
        /// <param name="cost">Available budget</param>
        /// <returns>CPM or CPP</returns>
        private static decimal _CalculateCost(double points, decimal cost)
        {
            return points == 0 ? 0 : cost / (decimal)points;
        }

        private void _PopulateProposalTotalsTable()
        {
            ProposalCampaignTotalsTable.QuarterLabel = "Campaign Totals";

            ProposalQuarterTables.SelectMany(x => x.Rows)
                .GroupBy(x => new { SpotLength = x.SpotLengthLabel, x.DaypartCode })
                .ToList()
                .ForEach(group =>
                {
                    var items = group.ToList();
                    double unitsSum = items.Sum(x => x.Units);
                    decimal totalCost = items.Sum(x => x.TotalCost);
                    double totalHHRatingPoints = items.Sum(x => x.HHData.TotalRatingPoints);
                    double totalHHImpressions = items.Sum(x => x.HHData.TotalImpressions);
                    double totalRatingPoints = items.Sum(x => x.GuaranteedData.TotalRatingPoints);
                    double totalImpressions = items.Sum(x => x.GuaranteedData.TotalImpressions);

                    var row = new ProposalQuarterTableRowData
                    {
                        DaypartCode = group.Key.DaypartCode,
                        SpotLengthLabel = group.Key.SpotLength,
                        Units = unitsSum,
                        UnitsCost = _CalculateCost(unitsSum, totalCost),
                        TotalCost = totalCost,
                        HHData = new AudienceData
                        {
                            RatingPoints = items.Sum(x => x.HHData.RatingPoints),
                            TotalRatingPoints = totalHHRatingPoints,
                            Impressions = items.Sum(x => x.HHData.Impressions),
                            TotalImpressions = totalHHImpressions,
                            CPM = _CalculateCost(totalHHImpressions, totalCost),
                            CPP = _CalculateCost(totalHHRatingPoints, totalCost)
                        },
                        GuaranteedData = new AudienceData
                        {
                            VPVH = ProposalMath.CalculateVpvh(totalImpressions, totalHHImpressions),
                            RatingPoints = items.Sum(x => x.GuaranteedData.RatingPoints),
                            TotalRatingPoints = totalRatingPoints,
                            Impressions = items.Sum(x => x.GuaranteedData.Impressions),
                            TotalImpressions = totalImpressions,
                            CPM = _CalculateCost(totalImpressions, totalCost),
                            CPP = _CalculateCost(totalRatingPoints, totalCost)
                        }
                    };
                    ProposalCampaignTotalsTable.Rows.Add(row);
                });
            _PopulateCampainTotalTableSecondaryAudiences();
            _SetTableTotals(ProposalCampaignTotalsTable);
        }

        private void _PopulateCampainTotalTableSecondaryAudiences()
        {
            ProposalQuarterTables
                .SelectMany(x => x.SecondaryAudiencesTables)
                .GroupBy(x => x.AudienceCode)
                .ToList()
                .ForEach(table =>
                {
                    var newTable = new SecondayAudienceTable
                    {
                        AudienceCode = table.Key
                    };
                    table.SelectMany(x => x.Rows).GroupBy(x => new { x.DaypartCode, x.SpotLengthLabel })
                    .ToList()
                    .ForEach(row =>
                    {
                        var quarterRows = ProposalQuarterTables
                            .SelectMany(x => x.Rows.Where(y => y.DaypartCode.Equals(row.Key.DaypartCode) && y.SpotLengthLabel.Equals(row.Key.SpotLengthLabel)));
                        double unitsSum = quarterRows.Sum(x => x.Units);
                        decimal totalCost = quarterRows.Sum(x => x.TotalCost);
                        double totalHHImpressions = quarterRows.Sum(x => x.HHData.TotalImpressions);

                        var demoData = row.ToList();
                        double totalRatingPointsForDemo = demoData.Sum(x => x.TotalRatingPoints);
                        double totalImpressionsForDemo = demoData.Sum(x => x.TotalImpressions);

                        newTable.Rows.Add(
                            new AudienceData
                            {
                                VPVH = ProposalMath.CalculateVpvh(totalImpressionsForDemo, totalHHImpressions),
                                RatingPoints = (unitsSum == 0 ? 0 : totalRatingPointsForDemo / unitsSum),
                                TotalRatingPoints = totalRatingPointsForDemo,
                                Impressions = (unitsSum == 0 ? 0 : totalImpressionsForDemo / unitsSum),
                                TotalImpressions = totalImpressionsForDemo,
                                CPM = _CalculateCost(totalImpressionsForDemo, totalCost),
                                CPP = _CalculateCost(totalRatingPointsForDemo, totalCost)
                            });
                    });
                    ProposalCampaignTotalsTable.SecondaryAudiencesTables.Add(newTable);
                });
        }

        private void _SetTableTotals(ProposalQuarterTableData table)
        {
            decimal totalCost = table.Rows.Sum(x => x.TotalCost);
            _SetQuarterTableTotals(table, totalCost);
            foreach (var demoTable in table.SecondaryAudiencesTables)
            {
                _SetTableSecondaryAudienceTableTotals(demoTable, totalCost);
            }
        }

        private void _SetTableSecondaryAudienceTableTotals(ProposalQuarterTableData.SecondayAudienceTable demoTable, decimal totalCost)
        {
            var totalImpressions = demoTable.Rows.Sum(x => x.TotalImpressions);
            var totalRatingPoints = demoTable.Rows.Sum(x => x.TotalRatingPoints);
            demoTable.TotalRow = new AudienceData
            {
                TotalRatingPoints = totalRatingPoints,
                TotalImpressions = totalImpressions,
                CPM = _CalculateCost(totalImpressions, totalCost),
                CPP = _CalculateCost(totalRatingPoints, totalCost)
            };
        }

        private void _SetQuarterTableTotals(ProposalQuarterTableData table, decimal totalCost)
        {
            double totalUnits = table.Rows.Sum(x => x.Units);
            var totalHHImpressions = table.Rows.Sum(x => x.HHData.TotalImpressions);
            var totalHHRatingPoints = table.Rows.Sum(x => x.HHData.TotalRatingPoints);
            var totalImpressions = table.Rows.Sum(x => x.GuaranteedData.TotalImpressions);
            var totalRatingPoints = table.Rows.Sum(x => x.GuaranteedData.TotalRatingPoints);

            table.TotalRow = new ProposalQuarterTableRowData
            {
                Units = totalUnits,
                TotalCost = totalCost,
                HHData = new AudienceData
                {
                    TotalRatingPoints = totalHHRatingPoints,

                    TotalImpressions = totalHHImpressions,
                    CPM = _CalculateCost(totalHHImpressions, totalCost),
                    CPP = _CalculateCost(totalHHRatingPoints, totalCost),
                },
                GuaranteedData = new AudienceData
                {
                    TotalRatingPoints = totalRatingPoints,
                    TotalImpressions = totalImpressions,
                    CPM = _CalculateCost(totalImpressions, totalCost),
                    CPP = _CalculateCost(totalRatingPoints, totalCost)
                }
            };
        }

        private void _PopulateHeaderData(CampaignExportTypeEnum exportType
            , CampaignDto campaign
            , List<PlanDto> plans
            , AgencyDto agency
            , AdvertiserDto advertiser
            , List<PlanAudienceDisplay> guaranteedDemos
            , List<PlanAudienceDisplay> orderedAudiences
            , IDateTimeEngine dateTimeEngine)
        {
            CampaignName = campaign.Name;
            CreatedDate = dateTimeEngine.GetCurrentMoment().ToString(DATE_FORMAT_SHORT_YEAR);
            AgencyName = agency.Name;
            ClientName = advertiser.Name;
            _SetCampaignFlightDate(plans);
            PostingType = plans.Select(x => x.PostingType).Distinct().Single().ToString();
            Status = exportType.Equals(CampaignExportTypeEnum.Contract) ? "Order" : "Proposal";

            _SetGuaranteeedDemo(guaranteedDemos, orderedAudiences);
            _SetSpotLengths(plans);
        }

        private void _SetSpotLengths(List<PlanDto> plans)
        {
            SpotLengths = plans
                            .SelectMany(x => x.CreativeLengths.Select(y => new { _AllSpotLengths.Single(w => w.Id == y.SpotLengthId).Display, x.Equivalized }))
                            .OrderBy(x => int.Parse(x.Display))
                            .ThenBy(x => x.Equivalized)
                            .Select(x => $":{x.Display}{_GetEquivalizedStatus(x.Equivalized, x.Display)}")
                            .Distinct()
                            .ToList();
        }

        private string _GetEquivalizedStatus(bool equivalized, string display)
        {
            return equivalized && !display.Equals("30") ? " eq." : string.Empty;
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
            CampaignStartQuarter = minStartDate != null ? _QuarterCalculationEngine.GetQuarterRangeByDate(minStartDate).ShortFormat() : string.Empty;
            CampaignEndQuarter = maxEndDate != null ? _QuarterCalculationEngine.GetQuarterRangeByDate(maxEndDate).ShortFormat() : string.Empty;
        }

        private class PlanProjectionForCampaignExport
        {
            public int QuarterNumber { get; set; }
            public int QuarterYear { get; set; }
            public int? MediaWeekId { get; set; }
            public DateTime? WeekStartDate { get; set; }
            public int? MediaMonthId { get; set; }
            public string MediaMonthName { get; set; }

            public int DaypartCodeId { get; set; }
            public string DaypartCode { get; set; }
            public string DaypartStartTime { get; set; }
            public string DaypartEndTime { get; set; }
            public List<int> FlightDays { get; set; }

            public List<string> GuaranteedDemo { get; set; }
            public string SpotLength { get; set; }
            public int SpotLengthId { get; set; }
            public bool Equivalized { get; set; }

            public decimal TotalCost { get; set; }
            public double Units { get; set; }

            public double TotalHHRatingPoints { get; set; }
            public double TotalHHImpressions { get; set; }
            public decimal HHCPM { get; set; }
            public decimal HHCPP { get; set; }

            public Audience GuaranteedAudience { get; set; } = new Audience();

            public List<Audience> SecondaryAudiences { get; set; } = new List<Audience>();

            public List<DateTime> HiatusDays { get; set; } = new List<DateTime>();

            internal class Audience
            {
                public int AudienceId { get; set; }
                public double VPVH { get; set; }
                public double TotalRatingPoints { get; set; }
                public double TotalImpressions { get; set; }
                public decimal CPM { get; set; }
                public decimal CPP { get; set; }
                public double WeightedPercentage { get; set; }
                public bool IsGuaranteedAudience { get; set; }
            }

            internal class AudienceEqualityComparer : IEqualityComparer<Audience>
            {
                public bool Equals(Audience x, Audience y)
                {
                    return x.AudienceId == y.AudienceId;
                }

                public int GetHashCode(Audience obj)
                {
                    return $"{obj.AudienceId}".GetHashCode();
                }
            }
        }
    }

    public class ProposalQuarterTableData
    {
        public string QuarterLabel { get; set; }
        public List<ProposalQuarterTableRowData> Rows { get; set; } = new List<ProposalQuarterTableRowData>();
        public ProposalQuarterTableRowData TotalRow { get; set; }
        public List<SecondayAudienceTable> SecondaryAudiencesTables { get; set; } = new List<SecondayAudienceTable>();
        public bool HasSecondaryAudiences
        {
            get
            {
                return SecondaryAudiencesTables.Any();
            }
        }

        public class ProposalQuarterTableRowData
        {
            public string DaypartCode { get; set; }
            public string SpotLengthLabel { get; set; }
            public double Units { get; set; }
            public decimal UnitsCost { get; set; }
            public decimal TotalCost { get; set; }
            public AudienceData HHData { get; set; }
            public AudienceData GuaranteedData { get; set; }
        }

        public class SecondayAudienceTable
        {
            public int AudienceId { get; set; }
            public string AudienceCode { get; set; }
            public List<AudienceData> Rows { get; set; } = new List<AudienceData>();
            public AudienceData TotalRow { get; set; }
        }

        public class AudienceData
        {
            public string DaypartCode { get; set; }
            public string SpotLengthLabel { get; set; }
            public double VPVH { get; set; }
            public double RatingPoints { get; set; }
            public double TotalRatingPoints { get; set; }
            public double Impressions { get; set; }
            public double TotalImpressions { get; set; }
            public decimal CPM { get; set; }
            public decimal CPP { get; set; }
        }
    }

    public class MarketCoverageData
    {
        public List<string> BlackoutMarketsName { get; set; } = new List<string>();
        public List<string> PreferentialMarketsName { get; set; } = new List<string>();
        public double CoveragePercentage { get; set; }
    }

    public class DaypartData
    {
        public int DaypartCodeId { get; set; }
        public string DaypartCode { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string FlightDays { get; set; }
        public List<DaypartRestrictionsData> GenreRestrictions { get; set; } = new List<DaypartRestrictionsData>();
        public List<DaypartRestrictionsData> ProgramRestrictions { get; set; } = new List<DaypartRestrictionsData>();
    }

    public class DaypartDataEqualityComparer : IEqualityComparer<DaypartData>
    {
        public bool Equals(DaypartData x, DaypartData y)
        {
            return x.DaypartCode.Equals(y.DaypartCode)
                && x.StartTime.Equals(y.StartTime)
                && x.EndTime.Equals(y.EndTime)
                && x.FlightDays.Equals(y.FlightDays);
        }

        public int GetHashCode(DaypartData obj)
        {
            return $"{obj.DaypartCode} {obj.StartTime} {obj.EndTime} {obj.FlightDays}".GetHashCode();
        }
    }

    public class DaypartRestrictionsData
    {
        public int PlanId { get; set; }
        public ContainTypeEnum ContainType { get; set; }
        public List<string> Restrictions { get; set; } = new List<string>();
    }

    public class FlowChartQuarterTableData
    {
        public string TableTitle { get; set; }
        public string QuarterLabel { get; set; }
        public string DaypartCode { get; set; }
        public int TotalWeeksInQuarter
        {
            get
            {
                return Months.Sum(x => x.WeeksInMonth);
            }
        }
        public List<(string Name, int WeeksInMonth)> Months { get; set; } = new List<(string Name, int WeeksInMonth)>();
        public List<object> WeeksStartDate { get; set; } = new List<object>();
        public List<object> DistributionPercentages { get; set; } = new List<object>();
        public List<object> UnitsValues { get; set; } = new List<object>();
        public List<object> ImpressionsValues { get; set; } = new List<object>();
        public List<object> CPMValues { get; set; } = new List<object>();
        public List<object> CostValues { get; set; } = new List<object>();
        public List<object> HiatusDaysFormattedValues { get; set; } = new List<object>();
        public List<List<DateTime>> HiatusDays { get; internal set; } = new List<List<DateTime>>();
    }

    public class ContractQuarterTableData
    {
        public string Title { get; set; }
        public List<List<object>> Rows { get; set; } = new List<List<object>>();
        public List<object> TotalRow { get; set; }
        public bool IsAduTable { get; set; }
    }
}
