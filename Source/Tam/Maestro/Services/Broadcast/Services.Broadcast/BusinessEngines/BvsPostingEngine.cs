using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

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
        public ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus7;
        private readonly IDataRepositoryFactory _DataRepositoryFactory;

        public BvsBvsPostingEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
        }

        public void PostBvsData(List<string> iscis, Dictionary<int, int> scheduleAudiences, int postBookingId)
        {
            var bvsRepo = _DataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var estimateIds = bvsRepo.GetEstimateIdsByIscis(iscis);

            estimateIds.ForEach(estimateId => PostBvsDataByEstimate(estimateId, scheduleAudiences, postBookingId));
        }

        public void PostBvsDataByEstimate(int estimateId)
        {
            var postingBookId = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleDtoByEstimateId(estimateId).PostingBookId;

            PostBvsDataByEstimate(estimateId, null, postingBookId);
        }
        public void PostBvsDataByEstimate(int estimateId, Dictionary<int, int> scheduleAudiences, int postingBookId)
        {
            var audiences = scheduleAudiences;

            // verify the expectedPostingBookId corresponds with the estimateId
            EnsurePostingBook(estimateId, postingBookId);

            //get the audiences from the schedule for the estimate
            //get the bvs data
            var bvsDetails = _DataRepositoryFactory.GetDataRepository<IBvsRepository>().GetBvsPostDetailsByEstimateId(estimateId);

            //clear any existing post results
            if (audiences == null || !audiences.Any())
            {
                _DataRepositoryFactory.GetDataRepository<IBvsPostDetailsRepository>().DeletePostDetails(estimateId);
                // we still need to get the audiences from the schedule for the posting.
                audiences = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetDictionaryOfScheduleAudiencesByRank(estimateId);
            }
            else
            {   // this is for blank schedules that need to supply their own schedule audiences
                _DataRepositoryFactory.GetDataRepository<IBvsPostDetailsRepository>()
                    .DeletePostDetails(estimateId, audiences.Select(s => s.Value).ToList());
            }
            //post data
            _PostData(postingBookId, bvsDetails, audiences);
        }

        private void EnsurePostingBook(int estimateId, int expectedPostingBookId)
        {
            var schedule = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>()
                                                                .FindByEstimateId(estimateId);

            if (schedule == null)
                return;

            var foundPostingBookId = schedule.posting_book_id;

            if (foundPostingBookId != expectedPostingBookId)
                throw new Exception(string.Format("Attempting to post using estimate_id that does not belong to given expectedPostingBookId. estimate_id={0}; expected PostingBookId={1}; provided PostingBookId{2}", estimateId, expectedPostingBookId, foundPostingBookId));
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

            //var stationRepo = _DataRepositoryFactory.GetDataRepository<IStationRepository>();
            var stationDetails = new List<StationDetailPointInTime>();
            foreach (var bvsPostDetail in bvsData)
            {
                //var stationCode = stationRepo.GetStationCode(bvsPostDetail.Station);
                //if (stationCode == null)
                //{
                //    continue;
                //}
                var stationDetail =  new StationDetailPointInTime
                {
                    LegacyCallLetters= bvsPostDetail.Station,
                    Id = bvsPostDetail.BvsFileDetailId,
                    DayOfWeek = bvsPostDetail.NsiDate.DayOfWeek,
                    TimeAired = bvsPostDetail.TimeAired
                };
                stationDetails.Add(stationDetail);
            }

            var forecastRepo = _DataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            var ratings = forecastRepo.GetImpressionsPointInTime(postingBook,
                                                                    uniqueRatingsAudiences,
                                                                    stationDetails,
                                                                    DefaultPlaybackType,
                                                                    BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

            var postResults = _GetPostResults(ratings, bvsData, scheduleAudiences, ratingsAudienceMappings.GroupBy(g => g.custom_audience_id).ToDictionary(k => k.Key, v => v.Select(a => a.rating_audience_id).ToList())).ToList();

            //persist post results
            _DataRepositoryFactory.GetDataRepository<IBvsPostDetailsRepository>().SavePostDetails(postResults);
        }
        private static IEnumerable<BvsPostDetailAudience> _GetPostResults(IEnumerable<StationImpressionsWithAudience> ratings,
                                                                            IEnumerable<BvsPostDetail> bvsDetails,
                                                                            Dictionary<int, int> scheduleAudiences,
                                                                            IReadOnlyDictionary<int, List<int>> customAudienceMappings)
        {
            var ratingsDict = ratings.GroupBy(g => g.id)
                .ToDictionary(k => k.Key,
                    v => v.ToList());

            foreach (var bvsDetail in bvsDetails)
            {
                foreach (var audience in scheduleAudiences)
                {
                    //var delivery = aggregate.GetDelivery(audience.Value, bvsDetail.TimeAired, bvsDetail.Station, IsWeekEnd(bvsDetail.NsiDate) ? 1 : 0);
                    List<StationImpressionsWithAudience> detailRatingList;
                    double delivery = 0;
                    if (ratingsDict.TryGetValue(bvsDetail.BvsFileDetailId, out detailRatingList))
                    {
                        delivery = detailRatingList
                                            .Where(r => customAudienceMappings[audience.Value]
                                                            .Contains(r.audience_id))
                                            .Sum(r => r.impressions);
                    }
                    yield return new BvsPostDetailAudience(bvsDetail.BvsFileDetailId, audience.Key, audience.Value, delivery);
                }
            }
        }
    }
}
