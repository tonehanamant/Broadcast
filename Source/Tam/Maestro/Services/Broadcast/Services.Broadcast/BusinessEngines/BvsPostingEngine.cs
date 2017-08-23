using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Aggregates;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IBvsPostingEngine : IApplicationService
    {
        void PostBvsDataByEstimate(int estimateId);
        void PostBvsDataByFile(int bvsFileId);
        void PostBvsData(List<string> iscis, Dictionary<int, int> scheduleAudiences, int postBookingId);
    }

    public class BvsBvsPostingEngine : IBvsPostingEngine
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;

        public BvsBvsPostingEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
        }

        public void PostBvsData(List<string> iscis, Dictionary<int, int> scheduleAudiences,int postBookingId)
        {
            var bvsRepo = _DataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var estimateIds = bvsRepo.GetEstimateIdsByIscis(iscis);

            estimateIds.ForEach(estimateId => PostBvsDataByEstimate(estimateId,scheduleAudiences, postBookingId));
        }

        public void PostBvsDataByEstimate(int estimateId)
        {
            var scheduleAudiences = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetDictionaryOfScheduleAudiencesByRank(estimateId);

            var postingBookId = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleDtoByEstimateId(estimateId).PostingBookId;

            PostBvsDataByEstimate(estimateId, scheduleAudiences,postingBookId);
        }
        public void PostBvsDataByEstimate(int estimateId, Dictionary<int, int> scheduleAudiences,int postingBookId)
        {
            // verify the expectedPostingBookId corresponds with the estimateId
            EnsurePostingBook(estimateId, postingBookId);

            //get the audiences from the schedule for the estimate
            //get the bvs data
            var bvsDetails = _DataRepositoryFactory.GetDataRepository<IBvsRepository>().GetBvsPostDetailsByEstimateId(estimateId);

            //clear any existing post results
            _DataRepositoryFactory.GetDataRepository<IBvsPostDetailsRepository>().DeletePostDetails(estimateId,scheduleAudiences.Select(s => s.Value).ToList());

            //post data
            _PostData(postingBookId, bvsDetails, scheduleAudiences);
        }

        private void EnsurePostingBook(int estimateId, int expectedPostingBookId)
        {
            var schedule= _DataRepositoryFactory.GetDataRepository<IScheduleRepository>()
                                                                .FindByEstimateId(estimateId);

            if (schedule == null)
                return;

            var foundPostingBookId = schedule.posting_book_id;

            if (foundPostingBookId!= expectedPostingBookId)
                throw new Exception(string.Format("Attempting to post using estimate_id that does not belong to given expectedPostingBookId. estimate_id={0}; expected PostingBookId={1}; provided PostingBookId{2}",estimateId, expectedPostingBookId, foundPostingBookId));
        }

        public void PostBvsDataByFile(int bvsFileId)
        {
            //get the unique estimates with matching schedules from the BVS details
            var estimateIds = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleEstimateIdsByBvsFile(bvsFileId);
            estimateIds.ForEach(PostBvsDataByEstimate);
        }

        private void _PostData(int postingBook, List<BvsPostDetail> bvsData, Dictionary<int, int> scheduleAudiences)
        {
            var ratingsAudienceMappings = _DataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>().GetRatingsAudiencesByMaestroAudience(scheduleAudiences.Select(x => x.Value).ToList());
            var uniqueRatingsAudiences = ratingsAudienceMappings.Select(x => x.rating_audience_id).Distinct().ToList();
            var postResults = new List<BvsPostDetailAudience>();

            //ratings are retrieved by media month for weekday or weekend
            //split up the details further
            var ratingsRepository = _DataRepositoryFactory.GetDataRepository<IRatingsRepository>();
            //the last dimension of the ratings call is the station
            var stations = bvsData.Select(x => x.Station).Distinct().ToList();

            var ratingsData = ratingsRepository.GetDeliveryDetails(postingBook, uniqueRatingsAudiences, stations);

            var aggregate = new RatingsAggregate(ratingsData, ratingsAudienceMappings);
            postResults.AddRange(_GetPostResults(bvsData, aggregate, scheduleAudiences));

            //persist post results
            _DataRepositoryFactory.GetDataRepository<IBvsPostDetailsRepository>().SavePostDetails(postResults);
        }
        private static IEnumerable<BvsPostDetailAudience> _GetPostResults(IEnumerable<BvsPostDetail> bvsDetails, RatingsAggregate aggregate, Dictionary<int, int> scheduleAudiences)
        {
            foreach (var bvsDetail in bvsDetails)
            {
                foreach (var audience in scheduleAudiences)
                {
                    var delivery = aggregate.GetDelivery(audience.Value, bvsDetail.TimeAired, bvsDetail.Station, IsWeekEnd(bvsDetail.NsiDate) ? 1 : 0);
                    yield return new BvsPostDetailAudience(bvsDetail.BvsDetailId, audience.Key, audience.Value, delivery);
                }
            }
        }

        private static bool IsWeekEnd(DateTime dateTime)
        {
            return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
