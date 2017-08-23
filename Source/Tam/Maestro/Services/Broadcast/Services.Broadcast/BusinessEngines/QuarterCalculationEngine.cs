using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;

namespace Services.Broadcast.BusinessEngines
{
    public interface IQuarterCalculationEngine
    {
        Tuple<DateTime, DateTime> GetDatesForTimeframe(RatesTimeframe timeFrameValue, DateTime currentDate);
        QuarterDetailDto GetQuarterRangeByDate(DateTime currentDate, int quarterShift);
    }

    public class QuarterCalculationEngine : IQuarterCalculationEngine
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public QuarterCalculationEngine(IDataRepositoryFactory broadcastDataRepositoryFactory, IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public Tuple<DateTime, DateTime> GetDatesForTimeframe(RatesTimeframe timeFrameValue, DateTime currentDate)
        {
            QuarterDetailDto quarterDetail;
            switch (timeFrameValue)
            {
                case RatesTimeframe.TODAY:
                    var startDate = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day);
                    var endDate = startDate.AddDays(1).AddSeconds(-1);
                    return new Tuple<DateTime, DateTime>(startDate, endDate);
                case RatesTimeframe.THISQUARTER:
                    quarterDetail = GetQuarterRangeByDate(currentDate, 0);
                    return new Tuple<DateTime, DateTime>(quarterDetail.StartDate, quarterDetail.EndDate);
                case RatesTimeframe.LASTQUARTER:
                    quarterDetail = GetQuarterRangeByDate(currentDate, -1);
                    return new Tuple<DateTime, DateTime>(quarterDetail.StartDate, quarterDetail.EndDate);
                case RatesTimeframe.NEXTQUARTER:
                    quarterDetail = GetQuarterRangeByDate(currentDate, 1);
                    return new Tuple<DateTime, DateTime>(quarterDetail.StartDate, quarterDetail.EndDate);
                default:
                    throw new ApplicationException(string.Format("Don't know how to handle timeframe: {0}", timeFrameValue));
            }
        }

        public QuarterDetailDto GetQuarterRangeByDate(DateTime currentDate, int quarterShift)
        {
            var currentMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthContainingDate(currentDate);

            //convert to zero-base
            var currentQuarter = currentMonth.Quarter - 1;
            var desiredQuarter = (currentQuarter + quarterShift) % 4;
            while (desiredQuarter < 0)
            {
                desiredQuarter += 4;
            }

            var currentYear = currentDate.Year;
            var yearShift = (int)Math.Floor((currentQuarter + quarterShift) / 4f);

            var desiredYear = currentYear + yearShift;

            //convert back to one-base
            desiredQuarter++;

            int startMonth, endMonth;
            switch (desiredQuarter)
            {
                case 1:
                    startMonth = 1;
                    endMonth = 3;
                    break;
                case 2:
                    startMonth = 4;
                    endMonth = 6;
                    break;
                case 3:
                    startMonth = 7;
                    endMonth = 9;
                    break;
                case 4:
                    startMonth = 10;
                    endMonth = 12;
                    break;
                default:
                    throw new Exception(String.Format("Invalid year quarter: {0}", desiredQuarter));
            }

            var startMediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(desiredYear, startMonth);
            var endMediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(desiredYear, endMonth);

            var startDate = startMediaMonth.StartDate;
            var endDate = endMediaMonth.EndDate.AddDays(1).AddSeconds(-1); //adjusting time to 11:59:59PM since media month does not include time information
            return new QuarterDetailDto
            {
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}