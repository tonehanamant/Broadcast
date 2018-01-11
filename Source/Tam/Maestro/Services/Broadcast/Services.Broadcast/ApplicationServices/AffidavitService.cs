using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public enum AffidaviteFileSource
    {
        Strata = 1
    };

    public interface IAffidavitService : IApplicationService
    {
        int SaveAffidavit(AffidavitSaveRequest saveRequest);
    }

    public class AffidavitService : IAffidavitService
    {
        private const ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3;

        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly IPostingBooksService _PostingBooksService;
        private readonly IAffidavitRepository _AffidavitRepository;

        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory, IBroadcastAudiencesCache broadcastAudiencesCache, IPostingBooksService postingBooksService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
            _PostingBooksService = postingBooksService;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
        }

        public int SaveAffidavit(AffidavitSaveRequest saveRequest)
        {
            Dictionary<int, int> spotLengthDict = null;

            if (saveRequest == null)
            {
                throw new Exception("No affidavit data received.");
            }

            var affidavit_file = new affidavit_files();
            affidavit_file.created_date = DateTime.Now;
            affidavit_file.file_hash = saveRequest.FileHash;
            affidavit_file.file_name = saveRequest.FileName;
            affidavit_file.source_id = saveRequest.Source;

            foreach (var detail in saveRequest.Details)
            {
                var det = new affidavit_file_details();
                det.air_time = Convert.ToInt32(detail.AirTime.TimeOfDay.TotalSeconds);
                det.original_air_date = detail.AirTime;
                det.isci = detail.Isci;
                det.program_name = detail.ProgramName;
                det.spot_length_id = GetSpotlength(detail.SpotLength, ref spotLengthDict);
                det.station = detail.Station;

                affidavit_file.affidavit_file_details.Add(det);
            }

            var postingBookId = GetPostingBookId();

            CalculateAffidavitImpressions(affidavit_file, postingBookId);

            var id = _AffidavitRepository.SaveAffidavitFile(affidavit_file);

            return id;
        }

        private int GetPostingBookId()
        {
            var defaultPostingBooks = _PostingBooksService.GetDefaultPostingBooks();

            if (!defaultPostingBooks.DefaultShareBook.PostingBookId.HasValue)
                throw new Exception("No default posting book available");

            return defaultPostingBooks.DefaultShareBook.PostingBookId.Value;
        }

        private int GetSpotlength(int spotLength, ref Dictionary<int, int> spotLengthDict)
        {
            if (spotLengthDict == null)
                spotLengthDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();

            if (!spotLengthDict.ContainsKey(spotLength))
                throw new Exception(string.Format("Invalid spot length '{0}' found.", spotLength));

            return spotLengthDict[spotLength];
        }

        private void CalculateAffidavitImpressions(affidavit_files affidavitFile, int postingBookId)
        {
            var audiencesRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            var allAudiences = _BroadcastAudiencesCache.GetAllEntities().Select(x => x.Id).ToList();
            var ratingAudiences =
                audiencesRepository.GetRatingsAudiencesByMaestroAudience(allAudiences)
                    .Select(r => r.rating_audience_id)
                    .Distinct()
                    .ToList();

            foreach (var affidavitFileDetail in affidavitFile.affidavit_file_details)
            {
                affidavitFileDetail.affidavit_file_detail_audiences = _CalculdateImpressionsForNielsenAudiences(affidavitFileDetail, ratingAudiences, postingBookId);
            }
        }

        private List<affidavit_file_detail_audiences> _CalculdateImpressionsForNielsenAudiences(affidavit_file_details affidavitFileDetail, List<int> ratingAudiences, int postingBookId)
        {
            var affidavitAudiences = new List<affidavit_file_detail_audiences>();
            var stationDetails = new List<StationDetailPointInTime>
            {
                new StationDetailPointInTime
                {
                    LegacyCallLetters = affidavitFileDetail.station,
                    DayOfWeek = affidavitFileDetail.original_air_date.DayOfWeek,
                    TimeAired = affidavitFileDetail.air_time
                }
            };

            var ratingForecastRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            var impressionsPointInTime = ratingForecastRepository.GetImpressionsPointInTime(postingBookId, ratingAudiences, stationDetails,
                DefaultPlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

            foreach (var impressionsWithAudience in impressionsPointInTime)
            {
                affidavitAudiences.Add(new affidavit_file_detail_audiences
                {
                    affidavit_file_detail_id = affidavitFileDetail.id,
                    audience_id = impressionsWithAudience.audience_id,
                    impressions = impressionsWithAudience.impressions
                });
            }

            return affidavitAudiences;
        }
    }
}