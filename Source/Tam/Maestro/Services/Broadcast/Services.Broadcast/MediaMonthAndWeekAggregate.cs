using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast
{
    public interface IMediaMonthAndWeekAggregate
    {
        MediaWeek GetMediaWeekById(int mediaWeekId);
        MediaMonth GetMediaMonthContainingMediaWeekId(int mediaWeekId);
        List<MediaMonth> GetMediaMonthsByIds(IEnumerable<int> mediaMonthIds);
        MediaMonth GetMediaMonthContainingDate(DateTime date);
        MediaMonth GetMediaMonthByYearAndMonth(int year, int month);
        MediaMonth GetMediaMonthById(int id);
        List<DisplayMediaWeek> GetDisplayMediaWeekByFlight(DateTime startDate, DateTime endDate);

        List<DisplayMediaWeek> GetDisplayMediaWeekByFlight(DateTime startDate, DateTime endDate,List<MediaWeek> mediaWeeks);
        List<MediaWeek> GetMediaWeeksByFlight(DateTime startDate, DateTime endDate);
        List<MediaWeek> GetMediaWeeksByMediaMonth(int mediaMonthId);
        LookupDto FindMediaWeekLookup(int mediaWeekId);
        MediaWeek GetMediaWeekContainingDate(DateTime date);
        MediaWeek GetMediaWeekContainingDateOrNull(DateTime date);
        List<MediaWeek> GetMediaWeeksByIdList(List<int> idList);
        List<MediaWeek> GetMediaWeeksInRange(DateTime startDate, DateTime endDate);
        List<MediaWeek> GetAllMediaWeeksStartAfterDate(DateTime effectiveDate);
        List<MediaWeek> GetAllMediaWeeksEndAfterDate(DateTime effectiveDate);
        List<DisplayMediaWeek> GetDisplayMediaWeekById(List<int> ids);
        List<CustomRatingMediaWeek> GetMediaWeeksForCustomRatings(List<DateTime?> hiatusWeekStartDates, Flight requestFlight);
        List<MediaMonth> GetMediaMonthsBetweenDates(DateTime startDate, DateTime endDate);
        List<MediaMonth> GetMediaMonthsBetweenDatesInclusive(DateTime startDate, DateTime endDate);
        List<MediaWeek> GetMediaWeeksIntersecting(DateTime startDate, DateTime endDate);
        Dictionary<DateTime, MediaWeek> GetMediaWeeksByContainingDate(List<DateTime> dates);

    }

    public class MediaMonthAndWeekAggregate : IMediaMonthAndWeekAggregate
    {
        private readonly List<MediaMonth> _MediaMonths;
        private readonly List<MediaWeek> _MediaWeeks;

        public MediaMonthAndWeekAggregate(List<MediaMonth> mediaMonths, List<MediaWeek> mediaWeeks)
        {
            _MediaMonths = mediaMonths;
            _MediaWeeks = mediaWeeks;
        }

        public MediaWeek GetMediaWeekById(int mediaWeekId)
        {
            var mediaWeek = _MediaWeeks.SingleOrDefault(x => x.Id == mediaWeekId);
            if (mediaWeek == null)
            {
                throw new Exception("Could not find Media Week with id: " + mediaWeekId);
            }
            return mediaWeek;
        }

        public MediaMonth GetMediaMonthContainingMediaWeekId(int mediaWeekId)
        {
            var mediaWeek = GetMediaWeekById(mediaWeekId);
            return GetMediaMonthById(mediaWeek.MediaMonthId);
        }

        public List<MediaMonth> GetMediaMonthsByIds(IEnumerable<int> mediaMonthIds)
        {
            return _MediaMonths.Where(x => mediaMonthIds.Contains(x.Id)).ToList();
        }

        public MediaMonth GetMediaMonthContainingDate(DateTime date)
        {
            var dateOnly = new DateTime(date.Year, date.Month, date.Day); //ignoring time provided in the method parameter

            var mediaMonth = _MediaMonths.SingleOrDefault(x => dateOnly >= x.StartDate && dateOnly <= x.EndDate);
            if (mediaMonth == null)
            {
                throw new Exception("No Media Month Found that intersect the date: " + dateOnly.ToShortDateString());
            }
            return mediaMonth;
        }

        public MediaMonth GetMediaMonthByYearAndMonth(int year, int month)
        {
            var mediaMonth = _MediaMonths.SingleOrDefault(x => x.Year == year && x.Month == month);
            if (mediaMonth == null)
            {
                throw new Exception(String.Format("No Media Month Found For Year: {0} and Month: {1}", year, month));
            }
            return mediaMonth;
        }

        public MediaMonth GetMediaMonthById(int id)
        {
            var mediaMonth = _MediaMonths.SingleOrDefault(x => x.Id == id);
            if (mediaMonth == null)
            {
                throw new Exception("No Media Month Found with id: " + id);
            }
            return mediaMonth;
        }

        public List<DisplayMediaWeek> GetDisplayMediaWeekByFlight(DateTime startDate, DateTime endDate)
        {
            return GetDisplayMediaWeekByFlight(startDate, endDate, _MediaWeeks);
        }

        public List<MediaWeek> GetMediaWeeksByFlight(DateTime startDate, DateTime endDate)
        {
            return (from mw in _MediaWeeks
                where mw.StartDate <= endDate
                      && mw.EndDate >= startDate
                select mw).ToList();
        }

        public List<DisplayMediaWeek> GetDisplayMediaWeekByFlight(DateTime startDate, DateTime endDate, List<MediaWeek> mediaWeeks = null)
        {
            if (mediaWeeks == null)
                mediaWeeks = _MediaWeeks;

            return (from mw in mediaWeeks
                    join mm in _MediaMonths on mw.MediaMonthId equals mm.Id
                    where mw.StartDate <= endDate
                          && mw.EndDate >= startDate
                    select new DisplayMediaWeek()
                    {
                        Id = mw.Id,
                        Week = mw.WeekNumber,
                        MediaMonthId = mm.Id,
                        Year = mm.Year,
                        Month = mm.Month,
                        WeekStartDate = mw.StartDate,
                        WeekEndDate = mw.EndDate,
                        MonthStartDate = mm.StartDate,
                        MonthEndDate = mm.EndDate,
                        IsHiatus = false
                    }).ToList();
        }

        public List<DisplayMediaWeek> GetDisplayMediaWeekById(List<int> ids)
        {
            return (from mw in _MediaWeeks
                    join mm in _MediaMonths on mw.MediaMonthId equals mm.Id
                    where ids.Contains(mw.Id)
                    select new DisplayMediaWeek()
                    {
                        Id = mw.Id,
                        Week = mw.WeekNumber,
                        MediaMonthId = mm.Id,
                        Year = mm.Year,
                        Month = mm.Month,
                        WeekStartDate = mw.StartDate,
                        WeekEndDate = mw.EndDate,
                        MonthStartDate = mm.StartDate,
                        MonthEndDate = mm.EndDate,
                        IsHiatus = false
                    }).ToList();
        }

        public List<MediaWeek> GetMediaWeeksByMediaMonth(int mediaMonthId)
        {
            return _MediaWeeks.Where(x => x.MediaMonthId == mediaMonthId).ToList();
        }

        public LookupDto FindMediaWeekLookup(int mediaWeekId)
        {
            var mediaWeek = GetMediaWeekById(mediaWeekId);
            return new LookupDto(mediaWeek.Id, mediaWeek.StartDate.ToShortDateString());
        }

        private readonly Dictionary<DateTime, MediaWeek> _ContainingCache = new Dictionary<DateTime, MediaWeek>();

        public MediaWeek GetMediaWeekContainingDate(DateTime date)
        {
            return GetMediaWeekContainingDateOrNull(date) ?? 
                   throw new Exception($"Unable to find media week containing date {date.ToShortDateString()}");
        }

        /// <summary>
        /// Returns a media week containing the date or null when a week is not found
        /// </summary>
        public MediaWeek GetMediaWeekContainingDateOrNull(DateTime date)
        {
            if (_ContainingCache.TryGetValue(date, out MediaWeek value))
                return value;

            var mediaWeek = (from x in _MediaWeeks
                             where date >= x.StartDate
                             && date <= x.EndDate
                             select x).SingleOrDefault();

            if (mediaWeek != null)
            {
                _ContainingCache[date] = mediaWeek;
            }
            
            return mediaWeek;
        }

        public Dictionary<DateTime, MediaWeek> GetMediaWeeksByContainingDate(List<DateTime> dates)
        {
            var mediaWeeks = new Dictionary<DateTime, MediaWeek>(); 
            foreach(var date in dates)
            {
                mediaWeeks[date] = GetMediaWeekContainingDate(date);
            }
            return mediaWeeks;
        }

        public List<MediaWeek> GetMediaWeeksByIdList(List<int> idList)
        {
            return _MediaWeeks.Where(mw => idList.Contains(mw.Id)).ToList();
        }

        //This misses scenarios where the start date is in the middle of a media week. Was this intended?
        public List<MediaWeek> GetMediaWeeksInRange(DateTime startDate, DateTime endDate)
        {
            return _MediaWeeks.Where(x => x.StartDate >= startDate && x.EndDate <= endDate).ToList();
        }

        public List<MediaWeek> GetMediaWeeksIntersecting(DateTime startDate, DateTime endDate)
        {
            return _MediaWeeks.Where(x => x.StartDate <= endDate && x.EndDate >= startDate).ToList();
        }

        public List<CustomRatingMediaWeek> GetMediaWeeksForCustomRatings(List<DateTime?> hiatusWeekStartDates, Flight requestFlight)
        {
            var result = new List<CustomRatingMediaWeek>();
            foreach (var mw in _MediaWeeks.Where(mw => mw.StartDate <= requestFlight.EndDate.Value && mw.EndDate >= requestFlight.StartDate.Value).ToList())
            {
                var crmw = new CustomRatingMediaWeek(mw.MediaMonthId, mw.WeekNumber);
                foreach (var hw in hiatusWeekStartDates)
                {
                    if (mw.StartDate <= hw.Value && hw.Value <= mw.EndDate)
                        crmw.Selected = false;
                }
                result.Add(crmw);
            }
            return result;
        }

        public List<MediaWeek> GetAllMediaWeeksStartAfterDate(DateTime effectiveDate)
        {
            return _MediaWeeks.Where(x => x.StartDate >= effectiveDate).ToList();
        }

        public List<MediaWeek> GetAllMediaWeeksEndAfterDate(DateTime effectiveDate)
        {
            return _MediaWeeks.Where(x => x.EndDate >= effectiveDate).ToList();
        }

        public List<MediaMonth> GetMediaMonthsBetweenDates(DateTime startDate, DateTime endDate)
        {
            return _MediaMonths.Where(x => (x.StartDate >= startDate && x.EndDate <= endDate)).ToList();
        }
        public List<MediaMonth> GetMediaMonthsBetweenDatesInclusive(DateTime startDate, DateTime endDate)
        {
            return _MediaMonths.Where(x => (x.Id >= GetMediaMonthContainingDate(startDate).Id  && 
                                                x.Id <= GetMediaMonthContainingDate(endDate).Id)).ToList();
        }
       }
}
