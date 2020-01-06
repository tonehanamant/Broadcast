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
    public interface IDetectionPostingEngine : IApplicationService
    {
        void PostDetectionDataByEstimate(int estimateId);
        void PostDetectionDataByFile(int bvsFileId);
        void PostDetectionData(List<string> iscis, Dictionary<int, int> scheduleAudiences, int postBookingId);
    }

    public class DetectionPostingEngine : IDetectionPostingEngine
    {
        public ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus7;
        private readonly IDataRepositoryFactory _DataRepositoryFactory;

        public DetectionPostingEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
        }

        public void PostDetectionData(List<string> iscis, Dictionary<int, int> scheduleAudiences, int postBookingId)
        {
            var bvsRepo = _DataRepositoryFactory.GetDataRepository<IDetectionRepository>();
            var estimateIds = bvsRepo.GetEstimateIdsByIscis(iscis);

            estimateIds.ForEach(estimateId => PostDetectionDataByEstimate(estimateId, scheduleAudiences, postBookingId));
        }

        public void PostDetectionDataByEstimate(int estimateId)
        {
            var postingBookId = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleDtoByEstimateId(estimateId).PostingBookId;

            PostDetectionDataByEstimate(estimateId, null, postingBookId);
        }
        public void PostDetectionDataByEstimate(int estimateId, Dictionary<int, int> scheduleAudiences, int postingBookId)
        {
            var audiences = scheduleAudiences;

            // verify the expectedPostingBookId corresponds with the estimateId
            EnsurePostingBook(estimateId, postingBookId);

            //get the audiences from the schedule for the estimate
            //get the detection data
            var detectionDetails = _DataRepositoryFactory.GetDataRepository<IDetectionRepository>().GetDetectionPostDetailsByEstimateId(estimateId);

            //clear any existing post results
            if (audiences == null || !audiences.Any())
            {
                _DataRepositoryFactory.GetDataRepository<IDetectionPostDetailsRepository>().DeletePostDetails(estimateId);
                // we still need to get the audiences from the schedule for the posting.
                audiences = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetDictionaryOfScheduleAudiencesByRank(estimateId);
            }
            else
            {   // this is for blank schedules that need to supply their own schedule audiences
                _DataRepositoryFactory.GetDataRepository<IDetectionPostDetailsRepository>()
                    .DeletePostDetails(estimateId, audiences.Select(s => s.Value).ToList());
            }
            //post data
            _PostData(postingBookId, detectionDetails, audiences);
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

        public void PostDetectionDataByFile(int bvsFileId)
        {
            //get the unique estimates with matching schedules from the BVS details
            var estimateIds = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleEstimateIdsByDetectionFile(bvsFileId);
            estimateIds.ForEach(PostDetectionDataByEstimate);
        }

        private void _PostData(int postingBook, List<DetectionPostDetail> detectionData, Dictionary<int, int> scheduleAudiences)
        {
            var ratingsAudienceMappings = _DataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>().GetRatingsAudiencesByMaestroAudience(scheduleAudiences.Select(x => x.Value).ToList());
            var uniqueRatingsAudiences = ratingsAudienceMappings.Select(x => x.rating_audience_id).Distinct().ToList();

            //var stationRepo = _DataRepositoryFactory.GetDataRepository<IStationRepository>();
            var stationDetails = new List<StationDetailPointInTime>();
            foreach (var detectionPostDetail in detectionData)
            {
                //var stationCode = stationRepo.GetStationCode(bvsPostDetail.Station);
                //if (stationCode == null)
                //{
                //    continue;
                //}
                var stationDetail =  new StationDetailPointInTime
                {
                    LegacyCallLetters= detectionPostDetail.Station,
                    Id = detectionPostDetail.DetectionDetailId,
                    DayOfWeek = detectionPostDetail.NsiDate.DayOfWeek,
                    TimeAired = detectionPostDetail.TimeAired
                };
                stationDetails.Add(stationDetail);
            }

            var forecastRepo = _DataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            var ratings = forecastRepo.GetImpressionsPointInTime(postingBook,
                                                                    uniqueRatingsAudiences,
                                                                    stationDetails,
                                                                    DefaultPlaybackType).Impressions;

            var postResults = _GetPostResults(ratings, detectionData, scheduleAudiences, ratingsAudienceMappings.GroupBy(g => g.custom_audience_id).ToDictionary(k => k.Key, v => v.Select(a => a.rating_audience_id).ToList())).ToList();

            //persist post results
            _DataRepositoryFactory.GetDataRepository<IDetectionPostDetailsRepository>().SavePostDetails(postResults);
        }
        private static IEnumerable<DetectionPostDetailAudience> _GetPostResults(IEnumerable<StationImpressionsWithAudience> ratings,
                                                                            IEnumerable<DetectionPostDetail> bvsDetails,
                                                                            Dictionary<int, int> scheduleAudiences,
                                                                            IReadOnlyDictionary<int, List<int>> customAudienceMappings)
        {
            var ratingsDict = ratings.GroupBy(g => g.Id)
                .ToDictionary(k => k.Key,
                    v => v.ToList());

            foreach (var bvsDetail in bvsDetails)
            {
                foreach (var audience in scheduleAudiences)
                {
                    //var delivery = aggregate.GetDelivery(audience.Value, bvsDetail.TimeAired, bvsDetail.Station, IsWeekEnd(bvsDetail.NsiDate) ? 1 : 0);
                    List<StationImpressionsWithAudience> detailRatingList;
                    double delivery = 0;
                    if (ratingsDict.TryGetValue(bvsDetail.DetectionDetailId, out detailRatingList))
                    {
                        delivery = detailRatingList
                                            .Where(r => customAudienceMappings[audience.Value]
                                                            .Contains(r.AudienceId))
                                            .Sum(r => r.Impressions);
                    }
                    yield return new DetectionPostDetailAudience(bvsDetail.DetectionDetailId, audience.Key, audience.Value, delivery);
                }
            }
        }
    }
}
