using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
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
        private readonly MediaMonthAndWeekAggregate _MediaMonthAndWeekAggregate;

        public MediaMonthAndWeekAggregateCache(IDataRepositoryFactory dataRepositoryFactory)
        {
            using (var scope = new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                _MediaMonthAndWeekAggregate = dataRepositoryFactory.GetDataRepository<IMediaMonthAndWeekAggregateRepository>().GetMediaMonthAggregate();
            }
        }

        public MediaWeek GetMediaWeekById(int mediaWeekId)
        {
            return _MediaMonthAndWeekAggregate.GetMediaWeekById(mediaWeekId);
        }

        public MediaMonth GetMediaMonthContainingMediaWeekId(int mediaWeekId)
        {
            return _MediaMonthAndWeekAggregate.GetMediaMonthContainingMediaWeekId(mediaWeekId);
        }

        public List<MediaMonth> GetMediaMonthsByIds(List<int> mediaMonthIds)
        {
            return _MediaMonthAndWeekAggregate.GetMediaMonthsByIds(mediaMonthIds);
        }

        public MediaMonth GetMediaMonthContainingDate(DateTime date)
        {
            return _MediaMonthAndWeekAggregate.GetMediaMonthContainingDate(date);
        }

        public MediaMonth GetMediaMonthByYearAndMonth(int year, int month)
        {
            return _MediaMonthAndWeekAggregate.GetMediaMonthByYearAndMonth(year, month);
        }

        public MediaMonth GetMediaMonthById(int id)
        {
            return _MediaMonthAndWeekAggregate.GetMediaMonthById(id);
        }

        public List<DisplayMediaWeek> GetDisplayMediaWeekByFlight(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.GetDisplayMediaWeekByFlight(startDate, endDate);
        }

        public List<DisplayMediaWeek> GetDisplayMediaWeekById(List<int> ids)
        {
            return _MediaMonthAndWeekAggregate.GetDisplayMediaWeekById(ids);
        }

        public List<MediaMonth> GetAllSweepsMonthsBeforeCurrentMonth()
        {
            return _MediaMonthAndWeekAggregate.GetAllSweepsMonthsBeforeCurrentMonth();
        }

        public List<MediaWeek> GetMediaWeeksByMediaMonth(int mediaMonthId)
        {
            return _MediaMonthAndWeekAggregate.GetMediaWeeksByMediaMonth(mediaMonthId);
        }

        public LookupDto FindMediaWeekLookup(int mediaWeekId)
        {
            return _MediaMonthAndWeekAggregate.FindMediaWeekLookup(mediaWeekId);
        }

        public MediaWeek GetMediaWeekContainingDate(DateTime date)
        {
            return _MediaMonthAndWeekAggregate.GetMediaWeekContainingDate(date);
        }

        public List<MediaWeek> GetMediaWeeksByIdList(List<int> idList)
        {
            return _MediaMonthAndWeekAggregate.GetMediaWeeksByIdList(idList);
        }

        public List<MediaWeek> GetMediaWeeksInRange(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.GetMediaWeeksInRange(startDate, endDate);
        }

        public List<CustomRatingMediaWeek> GetMediaWeeksForCustomRatings(List<DateTime?> hiatusWeekStartDates, Flight requestFlight)
        {
            return _MediaMonthAndWeekAggregate.GetMediaWeeksForCustomRatings(hiatusWeekStartDates, requestFlight);
        }

        public List<MediaWeek> GetMediaWeeksIntersecting(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.GetMediaWeeksIntersecting(startDate, endDate);
        }

        public List<MediaWeek> GetAllMediaWeeksStartAfterDate(DateTime effectiveDate)
        {
            return _MediaMonthAndWeekAggregate.GetAllMediaWeeksStartAfterDate(effectiveDate);
        }

        public List<MediaWeek> GetAllMediaWeeksEndAfterDate(DateTime effectiveDate)
        {
            return _MediaMonthAndWeekAggregate.GetAllMediaWeeksEndAfterDate(effectiveDate);
        }

        public List<MediaMonth> GetMediaMonthsBetweenDates(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.GetMediaMonthsBetweenDates(startDate, endDate);
        }
        public List<MediaMonth> GetMediaMonthsBetweenDatesInclusive(DateTime startDate, DateTime endDate)
        {
            return _MediaMonthAndWeekAggregate.GetMediaMonthsBetweenDatesInclusive(startDate, endDate);
        }

        public Dictionary<DateTime, MediaWeek> GetMediaWeeksByContainingDate(List<DateTime> dates)
        {
            return _MediaMonthAndWeekAggregate.GetMediaWeeksByContainingDate(dates);
        }
    }
}