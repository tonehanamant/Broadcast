using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanPricingInventoryQuarterCalculatorEngine
    {
        QuarterDetailDto GetPlanQuarter(PlanDto plan);

        QuarterDetailDto GetInventoryFallbackQuarter();

        List<DateRange> GetFallbackDateRanges(DateRange activeDateRange, QuarterDetailDto planQuarter, QuarterDetailDto fallbackQuarter);
    }

    public class PlanPricingInventoryQuarterCalculatorEngine : IPlanPricingInventoryQuarterCalculatorEngine
    {
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public PlanPricingInventoryQuarterCalculatorEngine(IQuarterCalculationEngine quarterCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _QuarterCalculationEngine = quarterCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public QuarterDetailDto GetPlanQuarter(PlanDto plan)
        {
            return _QuarterCalculationEngine.GetQuarterRangeByDate(plan.FlightStartDate.Value);
        }

        public QuarterDetailDto GetInventoryFallbackQuarter()
        {
            // the Fallback Quarter is the Previous Quarter from the Current Quarter.
            var currentQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(_GetCurrentDateTime());
            var previousQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentQuarter.StartDate.AddDays(-7));
            return previousQuarter;
        }

        public List<DateRange> GetFallbackDateRanges(DateRange activeDateRange, QuarterDetailDto planQuarter, QuarterDetailDto fallbackQuarter)
        {
            var results = new List<DateRange>();

            // If the flight start date is 10 days into the quarter, then set the Previous start date to 10 days into the previous quarter
            var startDateOffset = activeDateRange.Start.Value.Subtract(planQuarter.StartDate).TotalDays;
            var fallbackStartDate = fallbackQuarter.StartDate.AddDays(startDateOffset);

            // The Previous end date is then X days after the Previous start date where X is the number of days into the flight.
            var dayCount = activeDateRange.End.Value.Subtract(activeDateRange.Start.Value).TotalDays;
            var fallbackEndDate = fallbackStartDate.AddDays(dayCount);

            if (fallbackEndDate.Date <= fallbackQuarter.EndDate.Date)
            {
                results.Add(new DateRange(fallbackStartDate, fallbackEndDate));
                return results;
            }

            // If the date range goes beyond the fallback quarter then
            // use the last week of the previous quarter for each of those extra weeks
            // example :
            //  - Flight has 14 weeks and Previous Quarter has 12 weeks
            //  - Use week 12 for weeks 13 and 14

            // create a dateRange for the fallbackQuarter
            results.Add(new DateRange(fallbackStartDate, fallbackQuarter.EndDate));

            // -1 * 6 + 1(endDate) puts the start date as Monday
            var lastWeekDateRange = new DateRange(fallbackQuarter.EndDate.AddDays(-1 * 6), fallbackQuarter.EndDate);
            var overageDateRange = new DateRange(fallbackQuarter.EndDate.AddDays(1), fallbackEndDate);

            var weeksOver = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(overageDateRange.Start.Value, overageDateRange.End.Value).Count;
            for (var i = 0; i < weeksOver; i++)
            {
                results.Add(new DateRange(lastWeekDateRange.Start, lastWeekDateRange.End));
            }

            if (activeDateRange.End.Value.DayOfWeek != DayOfWeek.Sunday)
            {
                // align the final day of the range 
                var lastWeek = results.Last();
                var daysFromSunday = 7 - (int)activeDateRange.End.Value.DayOfWeek;
                var adjustedEndDate = lastWeek.End.Value.AddDays(-1 * daysFromSunday);
                lastWeek.End = adjustedEndDate;
            }

            return results;
        }

        protected virtual DateTime _GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}