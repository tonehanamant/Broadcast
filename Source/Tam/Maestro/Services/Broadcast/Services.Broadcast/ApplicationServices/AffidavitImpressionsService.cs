using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Microsoft.Practices.ObjectBuilder2;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitImpressionsService : IApplicationService
    {
        void CalculateAffidavitImpressionsForAffidavitFileDetails(List<AffidavitFileDetail> details);

        void RecalculateAffidavitImpressionsForProposalDetail(int proposalDetailId);
    }

    public class AffidavitImpressionsService : IAffidavitImpressionsService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly INsiComponentAudienceRepository _NsiComponentAudienceRepository;
        private readonly IStationProcessingEngine _StationProcessingEngine;

        public AffidavitImpressionsService(IDataRepositoryFactory broadcastDataRepositoryFactory, IStationProcessingEngine stationProcessingEngine)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _NsiComponentAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<INsiComponentAudienceRepository>();
            _StationProcessingEngine = stationProcessingEngine;
        }

        public void RecalculateAffidavitImpressionsForProposalDetail(int proposalDetailId)
        {
            var affidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            var details = affidavitRepository.GetAffidavitDetails(proposalDetailId);
            var audiencesIds =
                _NsiComponentAudienceRepository.GetAllNsiComponentAudiences().
                Select(a => a.Id).
                ToList();

            using (var transaction = new TransactionScopeWrapper())
            {
                affidavitRepository.RemoveAffidavitAudiences(details);

                _CalculateImpressionsForAffidavitDetails(details, audiencesIds);

                affidavitRepository.SaveAffidavitAudiences(details);

                transaction.Complete();
            }
        }

        public void CalculateAffidavitImpressionsForAffidavitFileDetails(List<AffidavitFileDetail> details)
        {
            var audiencesIds =
                _NsiComponentAudienceRepository.GetAllNsiComponentAudiences().
                Select(a => a.Id).
                ToList();

            _CalculateImpressionsForAffidavitDetails(details, audiencesIds);
        }

        private void _CalculateImpressionsForAffidavitDetails(ICollection<AffidavitFileDetail> details, List<int> audiencesIds)
        {
            var postingDataList = details.SelectMany(d => d.AffidavitClientScrubs).Select(s => new { s.PostingBookId, s.PostingPlaybackType }).Distinct().ToList();

            foreach (var postingData in postingDataList)
            {
                var stationDetails = new List<StationDetailPointInTime>();
                var ctr = 1;
                var affidavitDetailsForPostingBook = details.Where(d => d.AffidavitClientScrubs.Any(s => s.PostingBookId == postingData.PostingBookId &&
                                                                                                         s.PostingPlaybackType == postingData.PostingPlaybackType)).ToList();
                foreach (var affidavitFileDetail in affidavitDetailsForPostingBook)
                {
                    stationDetails.Add(
                        new StationDetailPointInTime
                        {
                            Id = ctr++,
                            LegacyCallLetters = _StationProcessingEngine.StripStationSuffix(affidavitFileDetail.Station),
                            DayOfWeek = affidavitFileDetail.OriginalAirDate.DayOfWeek,
                            TimeAired = affidavitFileDetail.AirTime
                        }
                    );
                }

                var ratingForecastRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
                var impressionsPointInTime = ratingForecastRepository.GetImpressionsPointInTime(postingData.PostingBookId.Value, audiencesIds,
                    stationDetails,
                    postingData.PostingPlaybackType.Value, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

                ctr = 1;
                foreach (var affidavitFileDetail in affidavitDetailsForPostingBook)
                {
                    var imps = impressionsPointInTime.Where(i => i.id == ctr).ToList();

                    affidavitFileDetail.AffidavitClientScrubs.Where(s => s.PostingBookId == postingData.PostingBookId &&
                                                                         s.PostingPlaybackType == postingData.PostingPlaybackType)
                        .ForEach(s => s.AffidavitClientScrubAudiences = imps.Select(imp => new AffidavitClientScrubAudience
                        {
                            AffidavitClientScrubId = s.Id,
                            AudienceId = imp.audience_id,
                            Impressions = imp.impressions
                        }).ToList()
                        );
                    ctr++;
                }
            }
        }
    }
}
