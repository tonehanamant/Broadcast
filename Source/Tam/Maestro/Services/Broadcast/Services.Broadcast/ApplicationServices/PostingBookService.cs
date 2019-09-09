using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
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
        /// Gets the share books available based on the flight start date.
        /// </summary>
        /// <param name="startDate">The start date of the flight.</param>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetShareBooks(DateTime startDate);

        /// <summary>
        /// Gets the hut books available based on the selected share book.
        /// </summary>
        /// <param name="startDate">The start date of the flight.</param>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetHUTBooks(DateTime? startDate, int? shareBookId);
    }

    public class PostingBookService : IPostingBookService
    {
        private readonly List<MediaMonth> _PostingBooks;
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
            _PostingBooks = ratingForecastService.GetMediaMonthCrunchStatuses()
                        .Where(a => a.Crunched == CrunchStatusEnum.Crunched)
                        .Select(m => m.MediaMonth)
                        .ToList();
        }

        ///<inheritdoc/>
        public List<LookupDto> GetAllPostingBooks()
        {
            return _ToLookupDto(_PostingBooks);                
        }

        ///<inheritdoc/>
        public List<LookupDto> GetHUTBooks(DateTime? flightStartDate, int? shareBookId)
        {

            if(!flightStartDate.HasValue && !shareBookId.HasValue)
            {
                return _ToLookupDto(_PostingBooks);
            }

            DateTime shareBookDate;

            if (shareBookId.HasValue)
            {
                shareBookDate = _PostingBooks.Where(b => b.Id == shareBookId).Select(b => b.StartDate).Single();
            }
            else //shareBookStartDate.HasValue
            {
                shareBookDate = flightStartDate.Value;
            }

            var currentQuarter = _QuartersEngine.GetQuarterRangeByDate(shareBookDate);
            var lastYearQuarter = _QuartersEngine.GetQuarterDetail(currentQuarter.Quarter, shareBookDate.Year - 1);

            //HUT book must be last in the flight quarter but from last year
            var hutBooks = _PostingBooks
                .Where(x => x.EndDate <= lastYearQuarter.EndDate);
            return _ToLookupDto(hutBooks);

        }

        ///<inheritdoc/>
        public List<LookupDto> GetShareBooks(DateTime startDate)
        {
            return _ToLookupDto(_PostingBooks.Where(x => x.StartDate <= startDate));
        }

        #region Private methods
        private List<LookupDto> _ToLookupDto(IEnumerable<MediaMonth> postingBooks)
        {
            return postingBooks.Select(x => new LookupDto { Id = x.Id, Display = x.GetShortMonthNameAndYear() }).ToList();
        }
        #endregion
    }
}
