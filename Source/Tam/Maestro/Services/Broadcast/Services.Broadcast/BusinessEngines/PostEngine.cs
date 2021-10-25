using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPostEngine : IApplicationService
    {
        void Post(post_files postFile);
    }

    public class PostEngine : IPostEngine
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;

        public PostEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
        }

        public void Post(post_files postFile)
        {
            var postUploadRepository = _DataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>();
            var stationRepo = _DataRepositoryFactory.GetDataRepository<IStationRepository>();

            var postDetails = new List<StationDetailPointInTime>();

            foreach (var postFileDetail in postFile.post_file_details)
            {
                var postDetail = new StationDetailPointInTime()
                {
                    Id = postFileDetail.id,
                    TimeAired = postFileDetail.time_aired,
                    DayOfWeek = postFileDetail.date.DayOfWeek,
                    LegacyCallLetters= postFileDetail.station
                };
                postDetails.Add(postDetail);
            }

            var maestroAudiences = postFile.post_file_demos.Select(d => d.demo).ToList();
            var audiencemappings = _DataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>().GetMaestroAudiencesGroupedByRatingAudiences(maestroAudiences);

            var uniqueRatingsAudiences = audiencemappings.Keys.Distinct().ToList();

            var ratingsData = _DataRepositoryFactory.GetDataRepository<IRatingForecastRepository>().GetImpressionsPointInTime(postFile.posting_book_id, uniqueRatingsAudiences, postDetails, (ProposalEnums.ProposalPlaybackType)postFile.playback_type);

            var ratingsByStation = ratingsData.Impressions.ToLookup(rd => rd.Id);
            var allImpressions = new List<post_file_detail_impressions>();
            foreach (var postDetail in postDetails)
            {
                var impPerMaestroDemo = new Dictionary<int, post_file_detail_impressions>();
                foreach (var rating in ratingsByStation[postDetail.Id])
                {
                    //Merge the rating audiences back into maestro audiences
                    foreach (var demo in audiencemappings[rating.AudienceId])
                    {
                        var imp = impPerMaestroDemo.GetOrInitialize(demo);
                        imp.demo = demo;
                        imp.post_file_detail_id = postDetail.Id;
                        imp.impression += rating.Impressions;
                    }
                }
                allImpressions.AddRange(impPerMaestroDemo.Values);
            }

            postUploadRepository.SavePostImpressions(allImpressions);
        }
    }
}
