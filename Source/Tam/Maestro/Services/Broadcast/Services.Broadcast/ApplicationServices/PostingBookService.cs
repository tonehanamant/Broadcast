using Common.Services.ApplicationServices;
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
        /// <param name="shareBookId">The share book identifier.</param>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetHUTBooks(int shareBookId);
    }

    public class PostingBookService : IPostingBookService
    {
        private readonly List<MediaMonth> _PostingBooks;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostingBookService"/> class.
        /// </summary>
        /// <param name="ratingForecastService">The rating forecast service.</param>
        /// <param name="mediaMonthAndWeekAggregateCache">The media month and week aggregate cache.</param>
        public PostingBookService(IRatingForecastService ratingForecastService
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
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
        public List<LookupDto> GetHUTBooks(int shareBookId)
        {
            var selectedShareBook = _PostingBooks.Single(x => x.Id == shareBookId);
            return _ToLookupDto(_PostingBooks.Where(x => x.EndDate <= selectedShareBook.StartDate));
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
