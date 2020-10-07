using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast
{
    public interface IMediaMonthAndWeekAggregateCache : IMediaMonthAndWeekAggregate
    {
    }

    public class MediaMonthAndWeekAggregateCache : IMediaMonthAndWeekAggregateCache
    {
        private readonly Lazy<MediaMonthAndWeekAggregate> _MediaMonthAndWeekAggregate;

        public MediaMonthAndWeekAggregateCache(IDataRepositoryFactory dataRepositoryFactory)
        {
            _MediaMonthAndWeekAggregate = new Lazy<MediaMonthAndWeekAggregate>(() => dataRepositoryFactory.GetDataRepository<IMediaMonthAndWeekAggregateRepository>().GetMediaMonthAggregate());
        }

        public MediaWeek GetMediaWeekById(int mediaWeekId)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeekById(mediaWeekId);
        }

        public MediaMonth GetMediaMonthContainingMediaWeekId(int mediaWeekId)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaMonthContainingMediaWeekId(mediaWeekId);
        }

        public List<MediaMonth> GetMediaMonthsByIds(IEnumerable<int> mediaMonthIds)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaMonthsByIds(mediaMonthIds);
        }

        public MediaMonth GetMediaMonthContainingDate(DateTime date)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaMonthContainingDate(date);
        }

        public MediaMonth GetMediaMonthByYearAndMonth(int year, int month)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaMonthByYearAndMonth(year, month);
        }

        public MediaMonth GetMediaMonthById(int id)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaMonthById(id);
        }
        public List<DisplayMediaWeek> GetDisplayMediaWeekByFlight(DateTime startDate, DateTime endDate,List<MediaWeek> mediaWeeksToUse)
        {
            return _MediaMonthAndWeekAggregate.Value.GetDisplayMediaWeekByFlight(startDate, endDate,mediaWeeksToUse);
        }

        public List<DisplayMediaWeek> GetDisplayMediaWeekByFlight(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.Value.GetDisplayMediaWeekByFlight(startDate, endDate);
        }

        public List<DisplayMediaWeek> GetDisplayMediaWeekById(List<int> ids)
        {
            return _MediaMonthAndWeekAggregate.Value.GetDisplayMediaWeekById(ids);
        }

        public List<MediaWeek> GetMediaWeeksByFlight(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeeksByFlight(startDate, endDate);
        }
        public List<MediaWeek> GetMediaWeeksByMediaMonth(int mediaMonthId)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeeksByMediaMonth(mediaMonthId);
        }

        public LookupDto FindMediaWeekLookup(int mediaWeekId)
        {
            return _MediaMonthAndWeekAggregate.Value.FindMediaWeekLookup(mediaWeekId);
        }

        public MediaWeek GetMediaWeekContainingDate(DateTime date)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeekContainingDate(date);
        }

        public MediaWeek GetMediaWeekContainingDateOrNull(DateTime date)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeekContainingDateOrNull(date);
        }

        public List<MediaWeek> GetMediaWeeksByIdList(List<int> idList)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeeksByIdList(idList);
        }

        public List<MediaWeek> GetMediaWeeksInRange(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeeksInRange(startDate, endDate);
        }

        public List<CustomRatingMediaWeek> GetMediaWeeksForCustomRatings(List<DateTime?> hiatusWeekStartDates, Flight requestFlight)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeeksForCustomRatings(hiatusWeekStartDates, requestFlight);
        }

        public List<MediaWeek> GetMediaWeeksIntersecting(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeeksIntersecting(startDate, endDate);
        }

        public List<MediaWeek> GetAllMediaWeeksStartAfterDate(DateTime effectiveDate)
        {
            return _MediaMonthAndWeekAggregate.Value.GetAllMediaWeeksStartAfterDate(effectiveDate);
        }

        public List<MediaWeek> GetAllMediaWeeksEndAfterDate(DateTime effectiveDate)
        {
            return _MediaMonthAndWeekAggregate.Value.GetAllMediaWeeksEndAfterDate(effectiveDate);
        }

        public List<MediaMonth> GetMediaMonthsBetweenDates(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaMonthsBetweenDates(startDate, endDate);
        }
        public List<MediaMonth> GetMediaMonthsBetweenDatesInclusive(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaMonthsBetweenDatesInclusive(startDate, endDate);
        }

        public Dictionary<DateTime, MediaWeek> GetMediaWeeksByContainingDate(List<DateTime> dates)
        {
            return _MediaMonthAndWeekAggregate.Value.GetMediaWeeksByContainingDate(dates);
        }
    }
}