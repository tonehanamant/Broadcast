using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.BusinessEngines
{
    public interface IInventoryGapCalculationEngine : IApplicationService
    {
        List<InventoryGapDetail> GetInventoryGaps(List<int> manifestMediaWeeks);
    }

    public class InventoryGapCalculationEngine : IInventoryGapCalculationEngine
    {
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public InventoryGapCalculationEngine(IQuarterCalculationEngine quarterCalculationEngine,
                                         IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache                                                   )
        {
            _QuarterCalculationEngine = quarterCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public List<InventoryGapDetail> GetInventoryGaps(List<int> manifestMediaWeeks)
        {
            var inventoryGapDetails = new List<InventoryGapDetail>();
            var startDate = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(manifestMediaWeeks.Min()).StartDate;
            var endDate = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(manifestMediaWeeks.Max()).EndDate;
            var dateRange = new DateRange(startDate, endDate);

            if (dateRange.IsEmpty())
            {
                return inventoryGapDetails;
            }
            var allQuarters = _QuarterCalculationEngine.GetAllQuartersBetweenDates(dateRange.Start.Value, dateRange.End.Value);

            foreach (var quarter in allQuarters)
            {
                var quarterMediaWeeks = _MediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(quarter.StartDate, quarter.EndDate).Select(x => x.Id);
                var commonMediaWeeks = manifestMediaWeeks.Intersect(quarterMediaWeeks).ToList();
                var missingMediaWeeks = quarterMediaWeeks.Except(commonMediaWeeks).ToList();

                if (commonMediaWeeks.Count() == quarterMediaWeeks.Count())
                {
                    // Nothing missing, continue.
                    continue;
                }
                else if (quarterMediaWeeks.Count() == missingMediaWeeks.Count())
                {
                    inventoryGapDetails.Add(new InventoryGapDetail
                    {
                        Quarter = quarter,
                        AllQuarterMissing = true
                    });
                }
                else
                {
                    inventoryGapDetails.Add(new InventoryGapDetail
                    {
                        Quarter = quarter,
                        DateGaps = _GetInventoryDateRangeGaps(_MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekById(missingMediaWeeks))
                    });
                }
            }

            return inventoryGapDetails;
        }

        private List<DateRange> _GetInventoryDateRangeGaps(List<DisplayMediaWeek> displayMediaWeeks)
        {
            var gaps = new List<DateRange>();

            if (!displayMediaWeeks.Any())
                return gaps;

            var orderedWeeks = displayMediaWeeks.OrderBy(x => x.Id);
            var start = displayMediaWeeks.First();
            var previous = displayMediaWeeks.First();

            if (displayMediaWeeks.Count() == 1)
            {
                gaps.Add(new DateRange(previous.WeekStartDate, previous.WeekEndDate));
                return gaps;
            }

            foreach(var week in orderedWeeks)
            {
                if (week.Id == previous.Id)
                    continue;

                if (previous.WeekEndDate.AddDays(1).Date == week.WeekStartDate.Date)
                {
                    previous = week;
                    continue;
                }

                gaps.Add(new DateRange(start.WeekStartDate, previous.WeekEndDate));
                previous = week;
                start = week;
            }

            var last = orderedWeeks.Last();
            gaps.Add(new DateRange(start.WeekStartDate, last.WeekEndDate));

            return gaps;
        }
    }
}
