using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.BusinessEngines
{
    public interface IInventoryWeekEngine : IApplicationService
    {
        DateRange GetDateRangeInventoryIsAvailableForForWeek(MediaWeek mediaWeek, DateTime effectiveDate, DateTime endDate);
    }

    public class InventoryWeekEngine : IInventoryWeekEngine
    {
        public DateRange GetDateRangeInventoryIsAvailableForForWeek(MediaWeek mediaWeek, DateTime effectiveDate, DateTime endDate)
        {
            DateTime? start = null;
            DateTime? end = null;

            if (effectiveDate <= mediaWeek.StartDate &&
                endDate >= mediaWeek.EndDate)
            {
                start = mediaWeek.StartDate;
                end = mediaWeek.EndDate;
            }
            else if (effectiveDate <= mediaWeek.StartDate &&
                     endDate >= mediaWeek.StartDate &&
                     endDate <= mediaWeek.EndDate)
            {
                start = mediaWeek.StartDate;
                end = endDate;
            }
            else if (effectiveDate >= mediaWeek.StartDate &&
                     endDate <= mediaWeek.EndDate)
            {
                start = effectiveDate;
                end = endDate;
            }
            else if (effectiveDate >= mediaWeek.StartDate &&
                     effectiveDate <= mediaWeek.EndDate &&
                     endDate >= mediaWeek.EndDate)
            {
                start = effectiveDate;
                end = mediaWeek.EndDate;
            }

            return new DateRange(start, end);
        }
    }
}
