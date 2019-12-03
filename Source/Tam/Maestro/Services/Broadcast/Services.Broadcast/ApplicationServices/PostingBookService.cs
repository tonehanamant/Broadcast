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

        /// <summary>
        /// Gets the monthly and sweep books available based on the selected share book.
        /// </summary>
        /// <param name="shareBookId">The share book id.</param>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetMonthlyBooks(int shareBookId);
    }

    public class PostingBookService : IPostingBookService
    {
        private readonly List<MediaMonthCrunchStatus> _MediaMonthsCrunchStatus;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IQuarterCalculationEngine _QuartersEngine;

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
            _MediaMonthsCrunchStatus = ratingForecastService.GetMediaMonthCrunchStatuses();
        }

        private List<MediaMonth> _GetSweepBooks() =>
            _MediaMonthsCrunchStatus.Where(a => a.Crunched == CrunchStatusEnum.Crunched)
                        .Select(m => m.MediaMonth)
                        .ToList();

        private List<MediaMonth> _GetAllMonthlyBooks() =>
            _MediaMonthsCrunchStatus.Select(m => m.MediaMonth).ToList();

        ///<inheritdoc/>
        public List<LookupDto> GetAllPostingBooks() =>
            _ToLookupDto(_GetAllMonthlyBooks());

        ///<inheritdoc/>
        public List<LookupDto> GetHUTBooks(int shareBookId) =>
            _GetBooks(_GetSweepBooks(), shareBookId);

        /// <inheritdoc/>
        public List<LookupDto> GetMonthlyBooks(int shareBookId) =>
            _GetBooks(_GetAllMonthlyBooks(), shareBookId);

        private List<LookupDto> _GetBooks(List<MediaMonth> mediaMonths, int shareBookId)
        {
            var shareBookDate = mediaMonths.Single(b => b.Id == shareBookId).StartDate;

            var currentQuarter = _QuartersEngine.GetQuarterRangeByDate(shareBookDate);
            var lastYearQuarter = _QuartersEngine.GetQuarterDetail(currentQuarter.Quarter, shareBookDate.Year - 1);

            var books = mediaMonths
                .Where(x => x.EndDate <= lastYearQuarter.EndDate);
            return _ToLookupDto(books);
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
