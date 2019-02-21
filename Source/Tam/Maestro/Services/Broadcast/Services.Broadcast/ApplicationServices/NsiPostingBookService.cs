using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface INsiPostingBookService : IApplicationService
    {
        List<LookupDto> GetNsiPostingBookMonths();
        List<MediaMonth> GetNsiPostingMediaMonths();
        int GetLatestNsiPostingBookForMonthContainingDate(DateTime now);
        List<LookupDto> GetPostingBookLongMonthNameAndYear();
    }
    public class NsiPostingBookService : INsiPostingBookService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IRatingForecastService _RatingForecastService;

        public NsiPostingBookService(IDataRepositoryFactory dataRepositoryFactory,
                                    IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                    IRatingForecastService ratingForecastService)
        {
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _BroadcastDataRepositoryFactory = dataRepositoryFactory;
            _RatingForecastService = ratingForecastService;
        }

        public List<LookupDto> GetNsiPostingBookMonths()
        {
            var postingBooks = _RatingForecastService.GetPostingBooks().Select(x => x.Id).ToList();

            var mediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(postingBooks);

            return (from x in mediaMonths
                    select new LookupDto
                    {
                        Id = x.Id,
                        Display = x.MediaMonthX
                    }).ToList();
        }

        public List<LookupDto> GetPostingBookLongMonthNameAndYear()
        {
            var postingBooks = _RatingForecastService.GetPostingBooks().Select(x => x.Id).ToList();
            var mediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(postingBooks);

            return (from mediaMonth in mediaMonths
                    orderby mediaMonth.Id descending
                    select new LookupDto
                    {
                        Id = mediaMonth.Id,
                        Display = mediaMonth.LongMonthNameAndYear
                    }).ToList();
        }

        public List<MediaMonth> GetNsiPostingMediaMonths()
        {
            var postingBooks = _RatingForecastService.GetPostingBooks().Select(x => x.Id).ToList();

            return _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(postingBooks);
        }

        public int GetLatestNsiPostingBookForMonthContainingDate(DateTime selectedDateTime)
        {
            var selectedMediaMonth = _MediaMonthAndWeekAggregateCache
                                        .GetMediaMonthContainingDate(selectedDateTime);

            var postingBookIds = _RatingForecastService.GetPostingBooks().Select(x => x.Id).ToList();
            var postingBookMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(postingBookIds);

            var postingBookMonth = postingBookMonths
                                    .Where(m => m.Quarter == selectedMediaMonth.Quarter
                                         && m.Year == selectedMediaMonth.Year)
                                    .OrderByDescending(m => m.Month).FirstOrDefault();
            if (postingBookMonth == null)
            {
                //if not found, try last year same quarter
                postingBookMonth = postingBookMonths
                                    .Where(m => m.Quarter == selectedMediaMonth.Quarter
                                        && m.Year == selectedMediaMonth.Year - 1)
                                    .OrderByDescending(m => m.Month).FirstOrDefault();
            }

            if (postingBookMonth == null)
            {
                throw new ApplicationException("No posting book available for date: " + selectedDateTime.ToLongDateString());
            }

            return postingBookMonth.Id;

        }
    }
}
