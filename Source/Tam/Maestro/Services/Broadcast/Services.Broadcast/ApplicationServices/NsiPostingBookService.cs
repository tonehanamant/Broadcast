using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface INsiPostingBookService : IApplicationService
    {
        List<LookupDto> GetNsiPostingBookMonths();
        List<MediaMonth> GetNsiPostingMediaMonths();
        int GetLatestNsiPostingBookForMonthContainingDate(DateTime now);
    }
    public class NsiPostingBookService : INsiPostingBookService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        public NsiPostingBookService(IDataRepositoryFactory dataRepositoryFactory, 
                                    IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _BroadcastDataRepositoryFactory = dataRepositoryFactory;
        }
        public List<LookupDto> GetNsiPostingBookMonths()
        {
            var postingBooks =
                _BroadcastDataRepositoryFactory.GetDataRepository<IPostingBookRepository>()
                    .GetPostableMediaMonths(BroadcastConstants.PostableMonthMarketThreshold);

            var mediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(postingBooks);

            return (from x in mediaMonths
                    select new LookupDto
                    {
                        Id = x.Id
                    ,
                        Display = x.MediaMonthX
                    }).ToList();
        }

        public List<MediaMonth> GetNsiPostingMediaMonths()
        {
            var postingBooks =
                _BroadcastDataRepositoryFactory.GetDataRepository<IPostingBookRepository>()
                    .GetPostableMediaMonths(BroadcastConstants.PostableMonthMarketThreshold);

            return _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(postingBooks);
        }

        public int GetLatestNsiPostingBookForMonthContainingDate(DateTime selectedDateTime)
        {
            var selectedMediaMonth = _MediaMonthAndWeekAggregateCache
                                        .GetMediaMonthContainingDate(selectedDateTime);

            var postingBookIds = _BroadcastDataRepositoryFactory
                                    .GetDataRepository<IPostingBookRepository>()
                                    .GetPostableMediaMonths(BroadcastConstants.PostableMonthMarketThreshold);
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
                                        && m.Year == selectedMediaMonth.Year-1)
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
