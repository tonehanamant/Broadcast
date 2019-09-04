using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Services.Broadcast.Extensions;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.ApplicationServices.Plan
{
    public interface IPlanService : IApplicationService
    {
        /// <summary>
        /// Saves the plan.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="modifiedBy">The modified by.</param>
        /// <param name="modifiedDate">The modified date.</param>
        /// <param name="aggregatePlanSynchronously">
        /// Synchronous execution is required for tests 
        /// because the transaction scope locks DB and summary data can not be saved from another thread
        /// </param>
        /// <returns></returns>
        int SavePlan(PlanDto plan, string modifiedBy, DateTime modifiedDate, bool aggregatePlanSynchronously = false);

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanDto GetPlan(int planId);

        /// <summary>
        /// Gets the plan statuses.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetPlanStatuses();

        /// <summary>
        /// Gets the delivery types.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetPlanCurrencies();

        /// <summary>
        /// Calculates the specified plan budget.
        /// </summary>
        /// <param name="planBudget">The plan budget.</param>
        /// <returns>PlanBudgetDeliveryCalculator object</returns>
        PlanDeliveryBudget Calculate(PlanDeliveryBudget planBudget);

        /// <summary>
        /// Gets the delivery types.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> PlanGloalBreakdownTypes();

        /// <summary>
        /// Calculates the weekly breakdown.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>WeeklyBreakdownResponse object</returns>
        WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request);
    }

    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanValidator _PlanValidator;
        private readonly IPlanBudgetDeliveryCalculator _BudgetCalculator;
        private readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IPlanAggregator _PlanAggregator;
        private readonly IPlanSummaryRepository _PlanSummaryRepository;

        public PlanService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IPlanValidator planValidator
            , IPlanBudgetDeliveryCalculator planBudgetDeliveryCalculator
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IPlanAggregator planAggregator)
        {
            _MediaWeekCache = mediaMonthAndWeekAggregateCache;
            _PlanValidator = planValidator;
            _BudgetCalculator = planBudgetDeliveryCalculator;

            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _PlanSummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanSummaryRepository>();
            _PlanAggregator = planAggregator;
        }

        ///<inheritdoc/>
        public int SavePlan(PlanDto plan, string modifiedBy, DateTime modifiedDate, bool aggregatePlanSynchronously = false)
        {
            plan.ModifiedBy = modifiedBy;
            plan.ModifiedDate = modifiedDate;
            DaypartTimeHelper.SubtractOneSecondToEndTime(plan.Dayparts);
            
            _PlanValidator.ValidatePlan(plan);

            if (plan.Id == 0)
            {
                plan.Id = _PlanRepository.SaveNewPlan(plan, modifiedBy, modifiedDate);
            }
            else
            {
                _PlanRepository.SavePlan(plan);
            }

            _DispatchPlanAggregation(plan, aggregatePlanSynchronously);

            return plan.Id;
        }

        ///<inheritdoc/>
        public PlanDto GetPlan(int planId)
        {
            PlanDto plan = _PlanRepository.GetPlan(planId);
            _SetWeekNumber(plan.WeeklyBreakdownWeeks);
            DaypartTimeHelper.AddOneSecondToEndTime(plan.Dayparts);

            plan.TotalActiveDays = plan.WeeklyBreakdownWeeks.Select(x => x.NumberOfActiveDays).Sum();
            plan.TotalShareOfVoice = plan.WeeklyBreakdownWeeks.Select(x => x.ShareOfVoice).Sum();
            return plan;
        }

        ///<inheritdoc/>
        public List<LookupDto> GetPlanStatuses()
        {
            return EnumExtensions.ToLookupDtoList<PlanStatusEnum>(); ;
        }

        ///<inheritdoc/>
        public List<LookupDto> GetPlanCurrencies()
        {
            return Enum.GetValues(typeof(PlanCurrenciesEnum))
                .Cast<PlanCurrenciesEnum>()
                .Select(x => new LookupDto
                {
                    Id = (int)x,
                    Display = x.GetDescriptionAttribute()
                })
                .OrderBy(x => x.Id).ToList();
        }

        ///<inheritdoc/>
        public PlanDeliveryBudget Calculate(PlanDeliveryBudget planBudget)
        {
            return _BudgetCalculator.CalculateBudget(planBudget);
        }

        ///<inheritdoc/>
        public List<LookupDto> PlanGloalBreakdownTypes()
        {
            return EnumExtensions.ToLookupDtoList<PlanGloalBreakdownTypeEnum>(); ;
        }

        /// <inheritdoc/>
        public WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request)
        {
            WeeklyBreakdownResponseDto response = new WeeklyBreakdownResponseDto();
            _PlanValidator.ValidateWeeklyBreakdown(request);

            //calculate flight weeks based on start/end date of the flight
            List<DisplayMediaWeek> weeks = _MediaWeekCache.GetDisplayMediaWeekByFlight(request.FlightStartDate, request.FlightEndDate);

            //add all the days outside of the flight for the first and last week as hiatus days
            request.FlightHiatusDays.AddRange(_GetDaysOutsideOfTheFlight(request.FlightStartDate, request.FlightEndDate, weeks));

            if (request.DeliveryType.Equals(PlanGloalBreakdownTypeEnum.Even))
            {
                response = _CalculateEvenPlanWeeklyGoalBreakdown(request, weeks);
            }
            else
            {
                response = _CalculateCustomPlanWeeklyGoalBreakdown(request, weeks);
            }
            return response;
        }

        private WeeklyBreakdownResponseDto _CalculateEvenPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks)
        {
            var result = new WeeklyBreakdownResponseDto();
            int weekNumber = 1;
            foreach (DisplayMediaWeek week in weeks)
            {
                int activeDays = _CalculateActiveDays(week, request.FlightHiatusDays, out string activeDaysString);
                result.Weeks.Add(new WeeklyBreakdownWeek
                {
                    ActiveDays = activeDaysString,
                    NumberOfActiveDays = activeDays,
                    StartDate = week.WeekStartDate,
                    EndDate = week.WeekEndDate,
                    WeekNumber = weekNumber++,
                    MediaWeekId = week.Id
                });
            }
            _CalculateImpressionsAndShareOfVoice(result.Weeks, request.TotalImpressions);
            _CalculateWeeklyGoalBreakdownTotals(result);
            return result;
        }
                
        private WeeklyBreakdownResponseDto _CalculateCustomPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks)
        {
            var result = new WeeklyBreakdownResponseDto();
            int weekNumber = request.Weeks.Count();

            //remove deleted weeks
            _RemoveDeletedWeeks(request.Weeks, weeks);
            
            //add the remain weeks
            result.Weeks.AddRange(request.Weeks);

            //add the missing weeks
            foreach (DisplayMediaWeek week in weeks.Where(x=> !request.Weeks.Select(y => y.StartDate).Contains(x.WeekStartDate)))
            {
                int activeDays = _CalculateActiveDays(week, request.FlightHiatusDays, out string activeDaysString);
                result.Weeks.Add(new WeeklyBreakdownWeek
                {
                    ActiveDays = activeDaysString,
                    NumberOfActiveDays = activeDays,
                    StartDate = week.WeekStartDate,
                    EndDate = week.WeekEndDate,
                    MediaWeekId = week.Id
                });
            }
            _CalculateWeeklyGoalBreakdownTotals(result);

            //the order of the weeks might be incorect, so do the order
            result.Weeks = result.Weeks.OrderBy(x => x.StartDate).ToList();
            _SetWeekNumber(result.Weeks);
            
            return result;
        }

        private List<DateTime> _GetDaysOutsideOfTheFlight(DateTime flightStartDate, DateTime flightEndDate, List<DisplayMediaWeek> weeks)
        {
            List<DateTime> result = new List<DateTime>();
            DateTime current = weeks.First().WeekStartDate;
            while (current < flightStartDate)
            {
                result.Add(current);
                current = current.AddDays(1);
            }
            current = weeks.Last().WeekEndDate;
            while (flightEndDate < current)
            {
                result.Add(current);
                current = current.AddDays(-1);
            }

            return result;
        }

        private static void _CalculateWeeklyGoalBreakdownTotals(WeeklyBreakdownResponseDto result)
        {
            result.TotalActiveDays = result.Weeks.Select(x => x.NumberOfActiveDays).Sum();
            result.TotalShareOfVoice = result.Weeks.Select(x => x.ShareOfVoice).Sum();
        }

        private void _RemoveDeletedWeeks(List<WeeklyBreakdownWeek> requestWeeks, List<DisplayMediaWeek> flightWeeks)
        {
            var flightStartDates = flightWeeks.Select(x => x.WeekStartDate).ToList();
            var weeksToRemove = requestWeeks.Where(x => !flightStartDates.Contains(x.StartDate)).ToList();
            weeksToRemove.ForEach(x => requestWeeks.Remove(x));
        }

        private void _SetWeekNumber(IEnumerable<WeeklyBreakdownWeek> weeks)
        {
            int i = 1;
            foreach(var week in weeks)
            {
                week.WeekNumber = i++;
            }
        }

        private void _CalculateImpressionsAndShareOfVoice(List<WeeklyBreakdownWeek> weeks, double totalImpressions)
        {
            var activeWeeks = weeks.Where(x => x.NumberOfActiveDays > 0);
            foreach(var week in activeWeeks)
            {
                week.Impressions = totalImpressions / activeWeeks.Count();
                week.ShareOfVoice = 100 / activeWeeks.Count();
            }
        }

        private int _CalculateActiveDays(DisplayMediaWeek week, List<DateTime> hiatusDays, out string activeDaysString)
        {
            List<string> daysOfWeek = new List<string> { "M", "Tu", "W", "Th", "F", "Sa", "Su" };
            activeDaysString = string.Empty;
            var hiatusDaysInWeek = hiatusDays.Where(x => week.WeekStartDate <= x && week.WeekEndDate >= x).ToList();

            //if there are no hiatus days in this week just return 7 active days
            if (!hiatusDaysInWeek.Any())
            {
                return 7;
            }
            //if all the week is hiatus, return 0 active days
            if (hiatusDaysInWeek.Count() == 7)
            {
                return 0;
            }

            //construct the active days string
            //null the hiatus days in the week
            for (int i = 0; i < daysOfWeek.Count(); i++)
            {
                if (hiatusDaysInWeek.Contains(week.WeekStartDate.AddDays(i)))
                {
                    daysOfWeek[i] = null;
                }
            }

            //group the active days that are not null
            var groupOfActiveDays = daysOfWeek.GroupConnected((a) => string.IsNullOrWhiteSpace(a));

            foreach (var group in groupOfActiveDays.Where(x=>x.Count() > 0))
            {
                //if the group contains 1 or 2 elements, join them by comma
                if (group.Count() == 1 || group.Count() == 2)
                {
                    activeDaysString += string.Join(",", group);
                }
                else  //if the group contains more then 3 elements, join the first and the last one with "-"
                {
                    activeDaysString += string.IsNullOrWhiteSpace(activeDaysString) ? "" : ",";
                    activeDaysString += $"{group.First()}-{group.Last()}";
                }
            }

            //number of active days this week is 7 minus number of hiatus days
            return 7 - hiatusDaysInWeek.Count();
        }

        private void _DispatchPlanAggregation(PlanDto plan, bool aggregatePlanSynchronously)
        {
            _PlanSummaryRepository.SetProcessingStatusForPlanSummary(plan.Id, PlanAggregationProcessingStatusEnum.InProgress);

            if (aggregatePlanSynchronously)
            {
                _AggregatePlan(plan);
            }
            else
            {
                Task.Factory.StartNew(() => _AggregatePlan(plan));
            }
        }

        private void _AggregatePlan(PlanDto plan)
        {
            try
            {
                var summary = _PlanAggregator.Aggregate(plan);
                summary.ProcessingStatus = PlanAggregationProcessingStatusEnum.Idle;
                _PlanSummaryRepository.SaveSummary(summary);
            }
            catch (Exception)
            {
                _PlanSummaryRepository.SetProcessingStatusForPlanSummary(plan.Id, PlanAggregationProcessingStatusEnum.Error);
            }
        }
    }
}
