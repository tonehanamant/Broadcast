using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostingBookService : IApplicationService
    {
        /// <summary>
        /// Gets all posting books.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetAllPostingBooks();

        /// <summary>
        /// Gets the default share book id based on the flight start date.
        /// </summary>
        /// <param name="startDate">The start date of the flight.</param>
        /// <returns>The default share book id</returns>
        int GetDefaultShareBookId(DateTime startDate);

        /// <summary>
        /// Gets the hut books available based on the selected share book.
        /// </summary>
        /// <param name="shareBookId">The share book id.</param>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetHUTBooks(int shareBookId);
    }

    public class PostingBookService : IPostingBookService
    {
        private readonly List<MediaMonth> _MediaMonths;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IQuarterCalculationEngine _QuartersEngine;

        private const int _February = 2;
        private const int _May = 5;
        private const int _July = 7;
        private const int _November = 11;
        private readonly List<int> _SweepBooksMonths = new List<int> { _February, _May, _July, _November };

        /// <summary>
        /// Initializes a new instance of the <see cref="PostingBookService"/> class.
        /// </summary>
        /// <param name="ratingForecastService">The rating forecast service.</param>
        /// <param name="mediaMonthAndWeekAggregateCache">The media month and week aggregate cache.</param>
        /// <param name="quarterCalculationEngine">The quarter calculation engine</param>
        public PostingBookService(IRatingForecastService ratingForecastService
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IQuarterCalculationEngine quarterCalculationEngine)
        {
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _QuartersEngine = quarterCalculationEngine;
            _MediaMonths = ratingForecastService.GetMediaMonthCrunchStatuses().Where(a => a.Crunched == CrunchStatusEnum.Crunched)
                        .Select(m => m.MediaMonth)
                        .ToList();
        }

        private List<MediaMonth> _GetSweepBooks() =>
            _MediaMonths.Where(m => _SweepBooksMonths.Contains(m.Month)).ToList();

        ///<inheritdoc/>
        public List<LookupDto> GetAllPostingBooks() =>
            _ToLookupDto(_MediaMonths);

        ///<inheritdoc/>
        public List<LookupDto> GetHUTBooks(int shareBookId)
        {
            var MonthsInAYear = 12;
            var shareBookDate = _MediaMonths.Single(b => b.Id == shareBookId).StartDate;

            //var currentQuarter = _QuartersEngine.GetQuarterRangeByDate(shareBookDate);
            //var lastYearQuarter = _QuartersEngine.GetQuarterDetail(currentQuarter.Quarter, shareBookDate.Year - 1);

            //var defaultHUTBook = _MediaMonths
            //    .Where(x => x.EndDate <= lastYearQuarter.EndDate).First();

            var defaultHUTBook = _MediaMonths.Where(m => m.Id <= shareBookId - MonthsInAYear).OrderByDescending(m => m.Id).First();

            return _ToLookupDto(_MediaMonths.Where(x => x.StartDate < shareBookDate)
                .OrderByDescending(x => x.Id == defaultHUTBook.Id));

        }

        ///<inheritdoc/>
        public int GetDefaultShareBookId(DateTime startDate) =>
           _GetSweepBooks()
                .Where(x => x.StartDate <= startDate)
                .OrderByDescending(x => x.StartDate)
                .First().Id;
             

        #region Private methods
        private List<LookupDto> _ToLookupDto(IEnumerable<MediaMonth> postingBooks)
        {
            return postingBooks.Select(x => new LookupDto { Id = x.Id, Display = x.GetShortMonthNameAndYear() }).ToList();
        }
        #endregion
    }
}
