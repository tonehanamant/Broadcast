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

        private const string DATE_FORMAT_SHORT_YEAR = "MM/dd/yy";
        private const string DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT = "M/d/yy";
        private const string DATE_FORMAT_NO_YEAR_SINGLE_DIGIT = "M/d";
        private const string DATE_FORMAT_FILENAME = "MM-dd";
        private const string FILENAME_FORMAT = "{0} {1} {2} Plan Rev - {3}.xlsx";

        public CampaignReportData(CampaignExportTypeEnum exportType, CampaignDto campaign
            , List<PlanDto> plans, AgencyDto agency, AdvertiserDto advertiser
            , List<PlanAudienceDisplay> guaranteedDemos
            , List<LookupDto> spotLenghts
            , List<DaypartDefaultDto> daypartDefaults
            , List<PlanAudienceDisplay> orderedAudiences
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IQuarterCalculationEngine quarterCalculationEngine)
        {
            List<ProjectedPlan> projectedPlans = _ProjectPlansForProposalExport(plans, spotLenghts, daypartDefaults, mediaMonthAndWeekAggregateCache, quarterCalculationEngine);
            _PopulateHeaderData(exportType, campaign, plans, agency, advertiser, guaranteedDemos, spotLenghts, orderedAudiences, quarterCalculationEngine);

            List<DateTime> hiatusDays = plans.SelectMany(x => x.FlightHiatusDays).Distinct().ToList();

            //proposal tab
            _PopulateProposalQuarterTableData(projectedPlans);
            _PopulateCampaignTotalsTable();
            _PopulateMarketCoverate(plans);
            _PopulateDaypartsData(projectedPlans);
            _PopulateContentRestrictions(plans);
            _PopulateFlightHiatuses(hiatusDays);
            _PopulateNotes();

            //flow chart tab
            projectedPlans = _ProjectPlansForQuarterExport(plans, spotLenghts, daypartDefaults, mediaMonthAndWeekAggregateCache, quarterCalculationEngine);
            _PopulateFlowChartQuarterTableData(projectedPlans, hiatusDays);

            if (exportType.Equals(CampaignExportTypeEnum.Contract))
            {
                _PopulateContractQuarterTableData(projectedPlans);
            }

            _SetExportFileName(projectedPlans, campaign.ModifiedDate);
        }

        private void _PopulateNotes()
        {
            Notes = "All CPMs are derived from 100% broadcast deliveries, no cable unless otherwise noted.";
        }

        private void _PopulateFlightHiatuses(List<DateTime> plansHiatuses)
        {            
            foreach (var group in plansHiatuses.GroupConsecutiveDays())
            {
                if (group.Count == 1)
                {
                    FlightHiatuses.Add(group.First().ToString(DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT));
                }
                if (group.Count > 1)
                {
                    FlightHiatuses.Add($"{group.First().ToString(DATE_FORMAT_NO_YEAR_SINGLE_DIGIT)}-{group.Last().ToString(DATE_FORMAT_SHORT_YEAR_SINGLE_DIGIT)}");
                }
            }
        }

        private void _PopulateDaypartsData(List<ProjectedPlan> projectedPlans)
        {
            DaypartsData = projectedPlans.Select(x => new DaypartData
            {
                DaypartCodeId = x.DaypartCodeId,
                DaypartCode = x.DaypartCode,
                EndTime = x.DaypartEndTime,
                StartTime = x.DaypartStartTime
            }).Distinct(new DaypartDataEqualityComparer()).ToList().OrderDayparts();
        }

        private void _PopulateContentRestrictions(List<PlanDto> plans)
        {
            foreach (var plan in plans)
            {
                foreach (var daypart in plan.Dayparts
                    .Where(x => x.Restrictions.GenreRestrictions != null && x.Restrictions.GenreRestrictions.Genres.Any()).ToList())
                {
                    var mappedDaypart = DaypartsData.Single(x => x.DaypartCodeId == daypart.DaypartCodeId);
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
                    var mappedDaypart = DaypartsData.Single(x => x.DaypartCodeId == daypart.DaypartCodeId);
                    var planDaypart = plan.Dayparts.Single(x => x.DaypartCodeId == daypart.DaypartCodeId);
                    mappedDaypart.ProgramRestrictions.Add(new DaypartRestrictionsData
                    {
                        PlanId = plan.Id,
                        ContainType = planDaypart.Restrictions.ProgramRestrictions.ContainType,
                        Restrictions = planDaypart.Restrictions.ProgramRestrictions.Programs.Select(x => x.Name).ToList()
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

        private void _SetExportFileName(List<ProjectedPlan> projectedPlans, DateTime campaignModifiedDate)
        {
            CampaignExportFileName = string.Format(FILENAME_FORMAT
                , ClientName
                , string.Join(" ", projectedPlans.Select(x => x.DaypartCode).Distinct().OrderBy(x => x).ToList())
                , (CampaignEndQuarter != CampaignStartQuarter ? $"{CampaignStartQuarter}-{CampaignEndQuarter}" : CampaignStartQuarter)
                , campaignModifiedDate.ToString(DATE_FORMAT_FILENAME));
        }

        private List<ProjectedPlan> _ProjectPlansForProposalExport(List<PlanDto> plans
            , List<LookupDto> spotLenghts
            , List<DaypartDefaultDto> daypartDefaults
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

                        var daypartCode = daypartDefaults.Single(dc => dc.Id == daypart.DaypartCodeId);
                        var newProjectedPlan = _GetEmptyWeek(null, plan, quarter, null, daypartDefaults, spotLenghts, daypart);

                        newProjectedPlan.TotalCost = plan.Budget.Value * (decimal)multiplicationFactor;
                        newProjectedPlan.Units = 0; //we have no value calculated and stored for this property at the moment                            

                        //HH data
                        newProjectedPlan.TotalHHImpressions = plan.HHImpressions * multiplicationFactor;
                        newProjectedPlan.TotalHHRatingPoints = plan.HHRatingPoints * multiplicationFactor;
                        newProjectedPlan.HHCPM = newProjectedPlan.TotalHHImpressions == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalHHImpressions * 1000;
                        newProjectedPlan.HHCPP = newProjectedPlan.TotalHHRatingPoints == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalHHRatingPoints;

                        //guaranteed audience data
                        newProjectedPlan.AudienceId = plan.AudienceId;
                        newProjectedPlan.VPVH = plan.Vpvh;
                        newProjectedPlan.TotalImpressions = plan.TargetImpressions.Value * multiplicationFactor;
                        newProjectedPlan.TotalRatingPoints = plan.TargetRatingPoints.Value * multiplicationFactor;

                        newProjectedPlan.CPM = newProjectedPlan.TotalImpressions == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalImpressions * 1000;
                        newProjectedPlan.CPP = newProjectedPlan.TotalRatingPoints == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalRatingPoints;

                        result.Add(newProjectedPlan);
                    });

                });
            });

            return result;
        }

        private List<ProjectedPlan> _ProjectPlansForQuarterExport(List<PlanDto> plans
            , List<LookupDto> spotLenghts
            , List<DaypartDefaultDto> daypartDefaults
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IQuarterCalculationEngine quarterCalculationEngine)
        {
            List<ProjectedPlan> result = new List<ProjectedPlan>();
            plans.ForEach(plan =>
            {
                List<QuarterDetailDto> planQuarters = quarterCalculationEngine.GetAllQuartersBetweenDates(plan.FlightStartDate.Value, plan.FlightEndDate.Value);
                var planMediaWeeks = mediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(plan.FlightStartDate.Value, plan.FlightEndDate.Value);
                planQuarters.ForEach(quarter =>
                {
                    var quarterMediaMonth = mediaMonthAndWeekAggregateCache.GetMediaMonthsBetweenDatesInclusive(quarter.StartDate, quarter.EndDate);

                    //foreach week in this quarter calculate imps, cost, etc and add empty weeks for the missing ones
                    foreach (var week in mediaMonthAndWeekAggregateCache.GetMediaWeeksInRange(quarter.StartDate, quarter.EndDate))
                    {
                        //not all quarter weeks are in the plan, that's why we do OrDefault here
                        var planWeek = plan.WeeklyBreakdownWeeks.Where(x => x.MediaWeekId == week.Id).SingleOrDefault();
                        if (planWeek == null)
                        {
                            result.AddRange(_AddEmptyWeekForPlanDayparts(week, plan, quarter, quarterMediaMonth, daypartDefaults, spotLenghts));
                            continue;
                        }

                        //weekly factor
                        double weeklyFactor = planWeek.WeeklyImpressionsPercentage / 100;

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
                                        ? (daypart.WeightingGoalPercent.Value / 100) * weeklyFactor
                                        : (weightingGoalFactor / 100) * weeklyFactor;

                            var newProjectedPlan = _GetEmptyWeek(week, plan, quarter, quarterMediaMonth, daypartDefaults, spotLenghts, daypart);
                            newProjectedPlan.TotalCost = plan.Budget.Value * (decimal)multiplicationFactor;
                            newProjectedPlan.Units = 0; //we have no value calculated and stored for this property at the moment                            

                            //HH data
                            newProjectedPlan.TotalHHImpressions = plan.HHImpressions * multiplicationFactor;
                            newProjectedPlan.TotalHHRatingPoints = plan.HHRatingPoints * multiplicationFactor;
                            newProjectedPlan.HHCPM = newProjectedPlan.TotalHHImpressions == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalHHImpressions * 1000;
                            newProjectedPlan.HHCPP = newProjectedPlan.TotalHHRatingPoints == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalHHRatingPoints;

                            //guaranteed audience data
                            newProjectedPlan.AudienceId = plan.AudienceId;
                            newProjectedPlan.WeightedPercentage = multiplicationFactor;
                            newProjectedPlan.VPVH = plan.Vpvh;
                            newProjectedPlan.TotalImpressions = plan.TargetImpressions.Value * multiplicationFactor;
                            newProjectedPlan.TotalRatingPoints = plan.TargetRatingPoints.Value * multiplicationFactor;
                            newProjectedPlan.CPM = newProjectedPlan.TotalImpressions == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalImpressions * 1000;
                            newProjectedPlan.CPP = newProjectedPlan.TotalRatingPoints == 0 ? 0 : newProjectedPlan.TotalCost / (decimal)newProjectedPlan.TotalRatingPoints;

                            result.Add(newProjectedPlan);
                        });
                    }
                });
            });

            return result;
        }

        private List<ProjectedPlan> _AddEmptyWeekForPlanDayparts(MediaWeek mediaWeek, PlanDto plan,
            QuarterDetailDto quarter, List<MediaMonth> mediaMonths, List<DaypartDefaultDto> dayparts,
            List<LookupDto> spotLenghts)
        {
            return plan.Dayparts.Select(x => _GetEmptyWeek(mediaWeek, plan, quarter, mediaMonths, dayparts, spotLenghts, x)).ToList();
        }

        //Returns a ProjectedPlan object that will contain media and plan data without imps and cost data
        private static ProjectedPlan _GetEmptyWeek(MediaWeek mediaWeek, PlanDto plan, QuarterDetailDto quarter
            , List<MediaMonth> mediaMonths, List<DaypartDefaultDto> daypartsDefault, List<LookupDto> spotLenghts, PlanDaypartDto daypart)
        {
            return new ProjectedPlan
            {
                QuarterYear = quarter.Year,
                QuarterNumber = quarter.Quarter,
                MediaWeekId = mediaWeek?.Id,
                MediaMonthId = mediaWeek?.MediaMonthId,
                WeekStartDate = mediaWeek?.StartDate,
                MediaMonthName = mediaWeek != null ? mediaMonths.Single(y => y.Id == mediaWeek.MediaMonthId).LongMonthName : null,
                DaypartCodeId = daypart.DaypartCodeId,
                DaypartCode = daypartsDefault.Single(y => y.Id == daypart.DaypartCodeId).Code,
                DaypartStartTime = DaypartTimeHelper.ConvertSecondsToFormattedTime(daypart.StartTimeSeconds),
                DaypartEndTime = DaypartTimeHelper.ConvertSecondsToFormattedTime(daypart.EndTimeSeconds),
                SpotLengthId = plan.SpotLengthId,
                SpotLength = spotLenghts.Single(y => y.Id == plan.SpotLengthId).Display,
                Equivalized = plan.Equivalized,
            };
        }

        private void _PopulateProposalQuarterTableData(List<ProjectedPlan> projectedPlans)
        {
            projectedPlans.GroupBy(x => new { x.QuarterNumber, x.QuarterYear })
                .OrderBy(x => x.Key.QuarterYear).ThenBy(x => x.Key.QuarterNumber)
                .ToList()
                .ForEach(planGroup =>
                {
                    ProposalQuarterTableData newTable = new ProposalQuarterTableData
                    {
                        QuarterLabel = $"Q{planGroup.Key.QuarterNumber} {planGroup.Key.QuarterYear}"
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
                            SpotLength = $"{daypartGroup.Key.SpotLength}{(daypartGroup.Key.Equivalized && ! daypartGroup.Key.SpotLength.Equals("30") ? $" eq." : string.Empty)}",
                            TotalCost = items.Sum(x => x.TotalCost),
                            UnitCost = (unitsSum == 0 ? 0 : items.Sum(x => x.TotalCost) / unitsSum),
                            Units = unitsSum,
                            GuaranteedAudienceData = new ProposalQuarterGuaranteedAudienceData
                            {
                                Impressions = (unitsSum == 0 ? 0 : (items.Sum(x => x.TotalImpressions) / unitsSum) / 1000),
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
                        row.GuaranteedAudienceData.CPP = _CalculateCost(row.GuaranteedAudienceData.TotalRatingPoints, row.TotalCost);
                        row.HHAudienceData.CPM = _CalculateCost(row.HHAudienceData.TotalImpressions, row.TotalCost);
                        row.HHAudienceData.CPP = _CalculateCost(row.HHAudienceData.TotalRatingPoints, row.TotalCost);

                        newTable.Rows.Add(row);

                    });
                    _SetTableTotals(newTable);
                    ProposalQuarterTables.Add(newTable);
                });
        }

        //this is total on each table
        private void _CalculateTotalsForTable(FlowChartQuarterTableData tableData)
        {
            tableData.DistributionPercentages.Add("-");
            tableData.UnitsValues.Add(tableData.UnitsValues.Sum(x => int.Parse(x.ToString())));
            double impressions = tableData.ImpressionsValues.Sum(x => double.Parse(x.ToString()));
            tableData.ImpressionsValues.Add(impressions);
            decimal cost = tableData.CostValues.Sum(x => decimal.Parse(x.ToString()));
            tableData.CostValues.Add(cost);
            tableData.CPMValues.Add(_CalculateCost(impressions, cost));
            tableData.HiatusDaysFormattedValues.Add(tableData.HiatusDays.SelectMany(x=>x).Count());
        }

        //this is the total table
        private FlowChartQuarterTableData _CalculateTotalTableData(List<FlowChartQuarterTableData> tablesInQuarterDaypart)
        {
            const int totalWeeksInQuarter = 13;
            var firstTable = tablesInQuarterDaypart.First();
            var tableData = new FlowChartQuarterTableData
            {
                TableTitle = $"{firstTable.QuarterLabel} {firstTable.DaypartCode} Total",
                WeeksStartDate = firstTable.WeeksStartDate
            };
            tableData.MonthsLabel = firstTable.MonthsLabel;
            for(int i=0; i< totalWeeksInQuarter; i++)
            {
                tableData.DistributionPercentages.Add(tablesInQuarterDaypart.Average(x => double.Parse(x.DistributionPercentages[i].ToString())));
                tableData.UnitsValues.Add(tablesInQuarterDaypart.Sum(x => int.Parse(x.UnitsValues[i].ToString())));
                double impressions = tablesInQuarterDaypart.Sum(x => double.Parse(x.ImpressionsValues[i].ToString()));
                tableData.ImpressionsValues.Add(impressions);
                decimal cost = tablesInQuarterDaypart.Sum(x => decimal.Parse(x.CostValues[i].ToString()));
                tableData.CostValues.Add(cost);
                tableData.CPMValues.Add(_CalculateCost(impressions, cost));

                var hiatusDaysThisWeek = tablesInQuarterDaypart.SelectMany(x => x.HiatusDays[i]).Distinct().ToList();
                tableData.HiatusDays.Add(hiatusDaysThisWeek);
                tableData.HiatusDaysFormattedValues.Add(_GetHiatusDaysFormattedForWeek(hiatusDaysThisWeek));
            }

            _CalculateTotalsForTable(tableData);
            return tableData;
        }

        private void _PopulateFlowChartQuarterTableData(List<ProjectedPlan> projectedPlans, List<DateTime> hiatusDays)
        {
            projectedPlans.GroupBy(x => new { x.QuarterNumber, x.QuarterYear, x.DaypartCode })
                .OrderBy(x => x.Key.QuarterYear).ThenBy(x => x.Key.QuarterNumber)
                .ToList()
                .ForEach(qDGrp =>   //quarter daypart group
                {
                    QuarterDetailDto quarter = new QuarterDetailDto
                    {
                        Quarter = qDGrp.Key.QuarterNumber,
                        Year = qDGrp.Key.QuarterYear
                    };

                    List<FlowChartQuarterTableData> tablesInQuarterDaypart = new List<FlowChartQuarterTableData>();
                    qDGrp.ToList().GroupBy(y => new { y.SpotLength, y.Equivalized })
                    .ToList()
                    .ForEach(qDSGrp =>  //quarter daypart spot length equivalized group
                    {
                        FlowChartQuarterTableData newTable = new FlowChartQuarterTableData
                        {
                            TableTitle = $"{quarter.ShortFormatQuarterNumberFirst()} {qDGrp.Key.DaypartCode} :{qDSGrp.Key.SpotLength}s {(qDSGrp.Key.Equivalized && !qDSGrp.Key.SpotLength.Equals("30") ? "eq." : string.Empty)}",
                            DaypartCode = qDGrp.Key.DaypartCode,
                            QuarterLabel = quarter.ShortFormatQuarterNumberFirst()
                        };
                        qDSGrp.GroupBy(x => x.MediaMonthName)
                            .ToList()
                            .ForEach(monthGroup =>
                            {
                                var monthItems = monthGroup.ToList();
                                newTable.MonthsLabel.Add(monthGroup.Key);

                                monthItems
                                        .GroupBy(x => x.WeekStartDate)
                                        .ToList()
                                        .ForEach(x =>
                                        {
                                            List<DateTime> hiatusDaysThisWeek = hiatusDays.Where(y => y >= x.Key.Value && y<= x.Key.Value.AddDays(6)).ToList();
                                            var weeks = x.ToList();

                                            newTable.WeeksStartDate.Add(x.Key.Value);
                                            newTable.DistributionPercentages.Add(weeks.Average(y => y.WeightedPercentage));
                                            newTable.UnitsValues.Add(0);
                                            var impressions = weeks.Sum(y => y.TotalImpressions) / 1000;
                                            newTable.ImpressionsValues.Add(impressions);
                                            var cost = weeks.Sum(y => y.TotalCost);
                                            newTable.CostValues.Add(cost);
                                            newTable.CPMValues.Add(_CalculateCost(impressions, cost));
                                            newTable.HiatusDaysFormattedValues.Add(_GetHiatusDaysFormattedForWeek(hiatusDaysThisWeek));
                                            newTable.HiatusDays.Add(hiatusDaysThisWeek);
                                        });
                            });
                        _CalculateTotalsForTable(newTable);
                        tablesInQuarterDaypart.Add(newTable);
                        FlowChartQuarterTables.Add(newTable);
                    });
                    FlowChartQuarterTableData totalTable = _CalculateTotalTableData(tablesInQuarterDaypart);
                    FlowChartQuarterTables.Add(totalTable);
                });
        }

        private string _GetHiatusDaysFormattedForWeek(List<DateTime> hiatuseDaysThisWeek)
        {
            List<string> formattedGroups = new List<string>();
            foreach(var group in hiatuseDaysThisWeek.GroupConsecutiveDays())
            {
                if(group.Count == 1)
                {
                    formattedGroups.Add(group.First().ToString(DATE_FORMAT_NO_YEAR_SINGLE_DIGIT));
                }
                if (group.Count > 1)
                {
                    formattedGroups.Add($"{group.First().ToString(DATE_FORMAT_NO_YEAR_SINGLE_DIGIT)}-{group.Last().Day}");
                }
            }
            return string.Join(", ", formattedGroups);
        }

        private void _PopulateContractQuarterTableData(List<ProjectedPlan> projectedPlans)
        {
            projectedPlans.GroupBy(x => new { x.QuarterNumber, x.QuarterYear })
                .OrderBy(x => x.Key.QuarterYear).ThenBy(x => x.Key.QuarterNumber)
                .ToList()
                .ForEach(qDGrp =>   //quarter daypart group
                {
                    QuarterDetailDto quarter = new QuarterDetailDto
                    {
                        Quarter = qDGrp.Key.QuarterNumber,
                        Year = qDGrp.Key.QuarterYear
                    };
                    var quarterTable = new ContractQuarterTableData
                    {
                        Title = quarter.LongFormat(),
                    };
                    qDGrp.ToList().GroupBy(y => new { y.DaypartCode, y.SpotLength, y.Equivalized, y.WeekStartDate })
                    .ToList()
                    .ForEach(row =>  //quarter daypart spot length equivalized group in the same week
                    {
                        var items = row.ToList();
                        decimal totalCost = items.Sum(y => y.TotalCost);
                        double totalImpressions = items.Sum(y => y.TotalImpressions);
                        if (totalImpressions == 0 && totalCost == 0)
                        {
                            return; //don't add empty weeks
                        }
                        int units = items.Sum(y => y.Units);


                        List<object> tableRow = new List<object>
                        {
                            row.Key.DaypartCode,
                            row.Key.WeekStartDate.Value.ToShortDateString(),
                            units,
                            $":{row.Key.SpotLength}s{(row.Key.Equivalized && !row.Key.SpotLength.Equals("30") ? " eq." : string.Empty)}",
                            units == 0 ? 0 : totalCost / units,   //cost per unit
                            totalCost,
                            units == 0 ? 0 : (totalImpressions / units) / 1000,    //impressions per unit
                            totalImpressions / 1000,
                            _CalculateCost(totalImpressions/1000, totalCost)
                        };

                        quarterTable.Rows.Add(tableRow);
                    });
                    quarterTable.TotalRow = _CalculateTotalsForContractTable(quarterTable.Rows, quarter.ShortFormatQuarterNumberFirst());
                    ContractQuarterTables.Add(quarterTable);
                });
            _CalculateContractTabTotals();
        }

        private void _CalculateContractTabTotals()
        {
            ContractTotals = new List<object>
            {
                ContractQuarterTables.SelectMany(x=>x.Rows).Sum(x=>int.Parse(x[2].ToString())), //units
                ContractQuarterTables.SelectMany(x=>x.Rows).Sum(x=>decimal.Parse(x[5].ToString())), //total cost
                null,   //empty cell in the excel
                ContractQuarterTables.SelectMany(x=>x.Rows).Sum(x=>double.Parse(x[7].ToString())), //total demo
                null   //empty cell in the excel
            };
            //CPM
            ContractTotals.Add(_CalculateCost(double.Parse(ContractTotals[3].ToString()), decimal.Parse(ContractTotals[1].ToString())));
        }

        private List<object> _CalculateTotalsForContractTable(List<List<object>> rows, string quarterLabel)
        {
            //add null for all the cells that don't have data

            decimal totalCost = rows.Sum(x => decimal.Parse(x[5].ToString()));
            double totalImpressions = rows.Sum(x => double.Parse(x[7].ToString()));

            return new List<object>
            {
                null,
                $"{quarterLabel} Totals",
                rows.Sum(x => int.Parse(x[2].ToString())),    //units
                "-",
                "-",
                totalCost,     //cost
                "-",
                totalImpressions,
                _CalculateCost(totalImpressions, totalCost)
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

        private void _PopulateCampaignTotalsTable()
        {
            ProposalQuarterTables.SelectMany(x => x.Rows)
                .GroupBy(x => new { x.SpotLength, x.DaypartCode })
                .ToList()
                .ForEach(group =>
                {
                    var items = group.ToList();
                    int unitsSum = items.Sum(x => x.Units);
                    ProposalQuarterTableRowData row = new ProposalQuarterTableRowData
                    {
                        DaypartCode = group.Key.DaypartCode,
                        SpotLength = group.Key.SpotLength,
                        TotalCost = items.Sum(x => x.TotalCost),
                        Units = unitsSum,
                        GuaranteedAudienceData = new ProposalQuarterGuaranteedAudienceData
                        {
                            Impressions = items.Sum(x => x.GuaranteedAudienceData.Impressions),
                            RatingPoints = items.Sum(x => x.GuaranteedAudienceData.RatingPoints),
                            TotalImpressions = items.Sum(x => x.GuaranteedAudienceData.TotalImpressions),
                            TotalRatingPoints = items.Sum(x => x.GuaranteedAudienceData.TotalRatingPoints)
                        },
                        HHAudienceData = new ProposalQuarterAudienceData
                        {
                            Impressions = items.Sum(x => x.HHAudienceData.Impressions),
                            RatingPoints = items.Sum(x => x.HHAudienceData.RatingPoints),
                            TotalImpressions = items.Sum(x => x.HHAudienceData.TotalImpressions),
                            TotalRatingPoints = items.Sum(x => x.HHAudienceData.TotalRatingPoints)
                        }
                    };
                    row.GuaranteedAudienceData.VPVH = row.GuaranteedAudienceData.TotalImpressions / row.HHAudienceData.TotalImpressions;
                    row.GuaranteedAudienceData.CPM = _CalculateCost(row.GuaranteedAudienceData.TotalImpressions, row.TotalCost);
                    row.GuaranteedAudienceData.CPP = _CalculateCost(row.GuaranteedAudienceData.TotalRatingPoints, row.TotalCost);
                    row.HHAudienceData.CPM = _CalculateCost(row.HHAudienceData.TotalImpressions, row.TotalCost);
                    row.HHAudienceData.CPP = _CalculateCost(row.HHAudienceData.TotalRatingPoints, row.TotalCost);

                    ProposalCampaignTotalsTable.Rows.Add(row);
                });
            _SetTableTotals(ProposalCampaignTotalsTable);
        }

        private static void _SetTableTotals(ProposalQuarterTableData table)
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
            CampaignName = campaign.Name;
            CreatedDate = DateTime.Now.ToString(DATE_FORMAT_SHORT_YEAR);
            AgencyName = agency.Name;
            ClientName = advertiser.Name;
            _SetCampaignFlightDate(plans, quarterCalculationEngine);
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
                            .Select(x => $":{x.Display}{(x.Equivalized && !x.Display.Equals("30") ? " eq." : string.Empty)}")
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

        private void _SetCampaignFlightDate(List<PlanDto> plans, IQuarterCalculationEngine quarterCalculationEngine)
        {
            var minStartDate = plans.Select(x => x.FlightStartDate).Min();
            var maxEndDate = plans.Select(x => x.FlightEndDate).Max();
            CampaignFlightStartDate = minStartDate != null ? minStartDate.Value.ToString(DATE_FORMAT_SHORT_YEAR) : string.Empty;
            CampaignFlightEndDate = maxEndDate != null ? maxEndDate.Value.ToString(DATE_FORMAT_SHORT_YEAR) : string.Empty;
            CampaignStartQuarter = minStartDate != null ? quarterCalculationEngine.GetQuarterRangeByDate(minStartDate).ShortFormat() : string.Empty;
            CampaignEndQuarter = maxEndDate != null ? quarterCalculationEngine.GetQuarterRangeByDate(maxEndDate).ShortFormat() : string.Empty;
        }

        private class ProjectedPlan
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
            public double WeightedPercentage { get; set; }
        }
    }

    public class ProposalQuarterTableData
    {
        public string QuarterLabel { get; set; }
        public List<ProposalQuarterTableRowData> Rows { get; set; } = new List<ProposalQuarterTableRowData>();
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
        public List<string> BlackoutMarketsName { get; set; } = new List<string>();
        public List<string> PreferentialMarketsName { get; set; } = new List<string>();
        public double CoveragePercentage { get; set; }
    }

    public class DaypartData
    {
        public int DaypartCodeId { get; set; }
        public string DaypartCode { get; set; }
        public string DaysLabel { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public List<DaypartRestrictionsData> GenreRestrictions { get; set; } = new List<DaypartRestrictionsData>();
        public List<DaypartRestrictionsData> ProgramRestrictions { get; set; } = new List<DaypartRestrictionsData>();
    }

    public class DaypartDataEqualityComparer : IEqualityComparer<DaypartData>
    {
        public bool Equals(DaypartData x, DaypartData y)
        {
            return x.DaypartCode.Equals(y.DaypartCode)
                && x.StartTime.Equals(y.StartTime)
                && x.EndTime.Equals(y.EndTime);
        }

        public int GetHashCode(DaypartData obj)
        {
            return $"{obj.DaypartCode} {obj.StartTime} {obj.EndTime}".GetHashCode();
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
        public List<object> MonthsLabel { get; set; } = new List<object>();
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
    }
}
