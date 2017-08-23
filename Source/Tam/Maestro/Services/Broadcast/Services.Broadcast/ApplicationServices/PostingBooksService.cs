using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostingBooksService : IApplicationService
    {
        /// <summary>
        /// Returns the default posting books for the current date.
        /// The returned data might have warning messages about the selection of the default books.
        /// </summary>
        /// <returns>DefaultPostingBookDto</returns>
        DefaultPostingBooksDto GetDefaultPostingBooks();
        /// <summary>
        /// Returns the default posting books for the specified date.
        /// The returned data might have warning messages about the selection of the default books.
        /// </summary>
        /// <returns>DefaultPostingBookDto</returns>
        DefaultPostingBooksDto GetDefaultPostingBooks(DateTime dateTime);
    }

    public class PostingBooksService : IPostingBooksService
    {
        private readonly IRatingForecastService _RatingForecastService;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private const int _QuarterToUseShareBookOnly = 3;

        public static string ShareBookForCurrentQuarterNotAvailable =
            "The share book for the current quarter is not available, the default share book is the latest available";

        public static string ShareBookNotFound =
            "No suitable share book has been found, please run the posting books data crunch";

        public static string HutBookForLastYearNotAvailable =
            "The hut book for the last year's quarter is not available, the default has been set as the latest possible book";

        public static string HutBookNotFound =
            "No suitable hut book has been found, the default has been set to Use Share Only";

        public PostingBooksService(IRatingForecastService ratingForecastService,
            IQuarterCalculationEngine quarterCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _RatingForecastService = ratingForecastService;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
        }

        public DefaultPostingBooksDto GetDefaultPostingBooks()
        {
            return GetDefaultPostingBooks(DateTime.Now);
        }

        public DefaultPostingBooksDto GetDefaultPostingBooks(DateTime dateTime)
        {
            var shareBook = _GetSharePostingBook(dateTime);

            var defaultPostingBookDto = new DefaultPostingBooksDto
            {
                DefaultShareBook = shareBook,
                DefaultHutBook = _GetHutPostingBook(dateTime, shareBook.PostingBookId),
                DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3,
                UseShareBookOnlyId = ProposalConstants.UseShareBookOnlyId
            };

            return defaultPostingBookDto;
        }

        private PostingBookResultDto _GetHutPostingBook(DateTime dateTime, int shareBookMonthId)
        {
            var mediaMonthForQuarter = _MediaMonthAndWeekAggregateCache.GetMediaMonthContainingDate(dateTime);

            if (mediaMonthForQuarter.Quarter == _QuarterToUseShareBookOnly)
            {
                return new PostingBookResultDto { PostingBookId = ProposalConstants.UseShareBookOnlyId };
            }

            var crunchedMonths =
                _RatingForecastService.GetMediaMonthCrunchStatuses()
                    .Where(month => month.Crunched == CrunchStatus.Crunched).ToList();

            var mediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthContainingDate(dateTime);

            var previousYearQuarterBeginEndDate = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, dateTime.AddYears(-1));

            var previousYearQuarterMonths =
                _MediaMonthAndWeekAggregateCache.GetMediaMonthsBetweenDatesInclusive(
                    previousYearQuarterBeginEndDate.Item1.Date, previousYearQuarterBeginEndDate.Item2.Date);

            var previousYearQuarterBook =
                crunchedMonths.FirstOrDefault(
                    crunchedMonth =>
                        previousYearQuarterMonths.Any(
                            currentQuarterMonth => currentQuarterMonth.Id == crunchedMonth.MediaMonth.Id));

            var lastAvailableBook = crunchedMonths.FirstOrDefault(crunchMonth => crunchMonth.MediaMonth.Id <= mediaMonth.Id && 
                                                                                 crunchMonth.MediaMonth.Id < shareBookMonthId);

            if (previousYearQuarterBook.MediaMonth == null)
            {
                if (lastAvailableBook.MediaMonth == null)
                    return new PostingBookResultDto
                    {
                        PostingBookId = ProposalConstants.UseShareBookOnlyId,
                        HasWarning = true,
                        WarningMessage = HutBookNotFound
                    };

                return new PostingBookResultDto
                {
                    PostingBookId = lastAvailableBook.MediaMonth.Id,
                    HasWarning = true,
                    WarningMessage = HutBookForLastYearNotAvailable
                };
            }

            return new PostingBookResultDto { PostingBookId = previousYearQuarterBook.MediaMonth.Id };
        }

        private PostingBookResultDto _GetSharePostingBook(DateTime dateTime)
        {
            var crunchedMonths =
                _RatingForecastService.GetMediaMonthCrunchStatuses()
                    .Where(month => month.Crunched == CrunchStatus.Crunched).ToList();

            var mediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthContainingDate(dateTime);
            
            var currentQuarterBeginEndDate = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, dateTime);

            var currentQuarterMonths =
                _MediaMonthAndWeekAggregateCache.GetMediaMonthsBetweenDatesInclusive(
                    currentQuarterBeginEndDate.Item1.Date, currentQuarterBeginEndDate.Item2.Date);

            var currentQuarterBook =
                crunchedMonths.FirstOrDefault(
                    crunchedMonth =>
                        currentQuarterMonths.Any(
                            currentQuarterMonth => currentQuarterMonth.Id == crunchedMonth.MediaMonth.Id));

            var lastAvailableBook = crunchedMonths.FirstOrDefault(x => x.MediaMonth.Id <= mediaMonth.Id);

            if (currentQuarterBook.MediaMonth == null)
            {
                if (lastAvailableBook.MediaMonth == null)
                    return new PostingBookResultDto
                    {
                        PostingBookId = ProposalConstants.ShareBookNotFoundId,
                        HasWarning = true,
                        WarningMessage = ShareBookNotFound
                    }; 

                return new PostingBookResultDto
                {
                    PostingBookId = lastAvailableBook.MediaMonth.Id,
                    HasWarning = true,
                    WarningMessage = ShareBookForCurrentQuarterNotAvailable
                };
            }

            return new PostingBookResultDto { PostingBookId = currentQuarterBook.MediaMonth.Id };
        }
    }
}
