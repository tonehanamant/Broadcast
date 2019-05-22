using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Microsoft.Practices.ObjectBuilder2;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IImpressionsService : IApplicationService
    {
        void CalculateImpressionsForFileDetails(List<ScrubbingFileDetail> details);

        void RecalculateImpressionsForProposalDetail(int proposalDetailId);

        void AddProjectedImpressionsToManifests(IEnumerable<StationInventoryManifest> manifests, ProposalEnums.ProposalPlaybackType? playbackType, int shareBook, int? hutBook = null);

        /// <summary>
        /// Calculates the projected station impressions for all nsi components
        /// </summary>
        /// <param name="manifests">List of manifests to calculate the component impressions</param>
        /// <param name="playbackType">Playback type</param>
        /// <param name="shareBook">Share book</param>
        /// <param name="hutBook">Hut book</param>
        void AddProjectedImpressionsForComponentsToManifests(IEnumerable<StationInventoryManifest> manifests, ProposalEnums.ProposalPlaybackType? playbackType, int shareBook, int? hutBook);
    }

    public class ImpressionsService : IImpressionsService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly INsiComponentAudienceRepository _NsiComponentAudienceRepository;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IRatingForecastRepository _RatingsRepository;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;

        public ImpressionsService(IDataRepositoryFactory broadcastDataRepositoryFactory, IStationProcessingEngine stationProcessingEngine)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _NsiComponentAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<INsiComponentAudienceRepository>();
            _StationProcessingEngine = stationProcessingEngine;
            _RatingsRepository = broadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            _BroadcastAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
        }

        public void RecalculateImpressionsForProposalDetail(int proposalDetailId)
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

        public void CalculateImpressionsForFileDetails(List<ScrubbingFileDetail> details)
        {
            var audiencesIds =
                _NsiComponentAudienceRepository.GetAllNsiComponentAudiences().
                Select(a => a.Id).
                ToList();

            _CalculateImpressionsForAffidavitDetails(details, audiencesIds);
        }

        private void _CalculateImpressionsForAffidavitDetails(ICollection<ScrubbingFileDetail> details, List<int> audiencesIds)
        {
            var postingDataList = details.SelectMany(d => d.ClientScrubs).Select(s => new { s.PostingBookId, s.PostingPlaybackType }).Distinct().ToList();

            foreach (var postingData in postingDataList)
            {
                var stationDetails = new List<StationDetailPointInTime>();
                var ctr = 1;
                var affidavitDetailsForPostingBook = details.Where(d => d.ClientScrubs.Any(s => s.PostingBookId == postingData.PostingBookId &&
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
                    postingData.PostingPlaybackType.Value);

                ctr = 1;
                foreach (var affidavitFileDetail in affidavitDetailsForPostingBook)
                {
                    var imps = impressionsPointInTime.Where(i => i.Id == ctr).ToList();

                    affidavitFileDetail.ClientScrubs.Where(s => s.PostingBookId == postingData.PostingBookId &&
                                                                         s.PostingPlaybackType == postingData.PostingPlaybackType)
                        .ForEach(s => s.ClientScrubAudiences = imps.Select(imp => new ScrubbingFileAudiences
                        {
                            ClientScrubId = s.Id,
                            AudienceId = imp.audience_id,
                            Impressions = imp.Impressions
                        }).ToList()
                        );
                    ctr++;
                }
            }
        }

        public void AddProjectedImpressionsToManifests(IEnumerable<StationInventoryManifest> manifests, ProposalEnums.ProposalPlaybackType? playbackType, int shareBook, int? hutBook)
        {
            var manifestsByContractedAudienceList = manifests.Where(m => m.ManifestAudiences.Any())
                .GroupBy(m => m.ManifestAudiences.Where(ma => ma.IsReference == false).Single().Audience.Id).ToList();

            foreach (var manifestsByContractedAudience in manifestsByContractedAudienceList)
            {
                //Debug.WriteLine($"Processing impressions for manifest by audience {manifestsByContractedAudience.Key}");
                var contractedAudience = manifestsByContractedAudience.Key;

                var audiencesMappings = _BroadcastAudienceRepository
                    .GetRatingsAudiencesByMaestroAudience(new List<int> { contractedAudience }).ToList();

                var ratingAudiences = audiencesMappings
                    .Where(a => a.custom_audience_id == contractedAudience)
                    .Select(a => a.rating_audience_id)
                    .Distinct()
                    .ToList();

                // flatten out manifest dayparts with unique Ids for each record.
                var manifestsByStationDaypartList =
                    (
                        from manifest in manifestsByContractedAudience.ToList()
                        from manifestDaypart in manifest.ManifestDayparts
                        select new { id = manifestDaypart.Daypart.Id, manifest, daypart = manifestDaypart.Daypart }
                    ).GroupBy(g => new { g.manifest.Station.LegacyCallLetters, g.daypart }).ToList();

                int counter = 1;
                var stationDetails = new List<ManifestDetailDaypart>();
                foreach (var msd in manifestsByStationDaypartList)
                {
                    stationDetails.Add(
                            new ManifestDetailDaypart()
                            {
                                Id = counter++,
                                DisplayDaypart = msd.Key.daypart,
                                LegacyCallLetters = msd.Key.LegacyCallLetters
                            }
                        );
                    Debug.WriteLine($"station detail: {msd.Key.daypart} {msd.Key.LegacyCallLetters}");
                }

                List<StationImpressions> stationImpressions = null;

                //Stopwatch sw;

                if (hutBook.HasValue)
                {
                    //sw = Stopwatch.StartNew();
                    stationImpressions = _RatingsRepository
                        .GetImpressionsDaypart((short)hutBook.Value, (short)shareBook, ratingAudiences, stationDetails, playbackType);
                    //sw.Stop();
                    //Debug.WriteLine($"GetImpressionsDaypart for 2 books: {sw.ElapsedMilliseconds} ");
                }
                else
                {
                    //sw = Stopwatch.StartNew();
                    stationImpressions = _RatingsRepository
                        .GetImpressionsDaypart(shareBook, ratingAudiences, stationDetails, playbackType)
                        .Select(x => (StationImpressions)x)
                        .ToList();
                    //sw.Stop();
                    //Debug.WriteLine($"GetImpressionsDaypart for 1 book: {sw.ElapsedMilliseconds}");                    
                }

                counter = 1;
                foreach (var manifestsByStationDaypart in manifestsByStationDaypartList)
                {
                    var impressions = stationImpressions.Where(i => i.Id == counter).ToList();
                    foreach (var msd in manifestsByStationDaypart)
                    {
                        msd.manifest.ProjectedStationImpressions.Add(
                            new StationImpressions
                            {
                                Impressions = impressions.Sum(i => i.Impressions),
                                Legacy_call_letters = impressions.Select(i => i.Legacy_call_letters).FirstOrDefault()
                            });
                    }
                    counter++;
                }
            }
        }

        /// <summary>
        /// Calculates the projected station impressions for all nsi components
        /// </summary>
        /// <param name="manifests">List of manifests to calculate the component impressions</param>
        /// <param name="playbackType">Playback type</param>
        /// <param name="shareBook">Share book</param>
        /// <param name="hutBook">Hut book</param>
        public void AddProjectedImpressionsForComponentsToManifests(IEnumerable<StationInventoryManifest> manifests, ProposalEnums.ProposalPlaybackType? playbackType, int shareBook, int? hutBook)
        {
            var componentAudiences = _NsiComponentAudienceRepository.GetAllNsiComponentAudiences();
            var manifestsByStation = manifests.GroupBy(x => x.Station.LegacyCallLetters).ToList();

            var manifestsByStationDaypartList =
                   (
                       from manifest in manifests
                       from manifestDaypart in manifest.ManifestDayparts
                       select new { id = manifestDaypart.Daypart.Id, manifest, daypart = manifestDaypart.Daypart }
                   ).GroupBy(g => new { g.manifest.Station.LegacyCallLetters, g.daypart }).ToList();

            foreach (var msd in manifestsByStationDaypartList)
            {
                var stationDetail = new ManifestDetailDaypart()
                {
                    DisplayDaypart = msd.Key.daypart,
                    LegacyCallLetters = msd.Key.LegacyCallLetters
                };

                //get the impressions for this station detail and all the nsi components
                var stationInventoryManifestAudiences = _LoadImpressionsForComponents(componentAudiences, new List<ManifestDetailDaypart>() { stationDetail }, hutBook, shareBook, playbackType);

                //each manifest in this group has the same impressions for the nsi componentns
                foreach (var manifest in msd.Select(x => x.manifest).ToList())
                {
                    manifest.ManifestAudiences = stationInventoryManifestAudiences;
                }
            }
        }

        private List<StationInventoryManifestAudience> _LoadImpressionsForComponents(List<BroadcastAudience> componentAudiences, List<ManifestDetailDaypart> stationDetail
            , int? hutBook, int shareBook, ProposalEnums.ProposalPlaybackType? playbackType)
        {
            List<StationInventoryManifestAudience> result = new List<StationInventoryManifestAudience>();
            List<StationImpressions> stationImpressions = new List<StationImpressions>();
            foreach (var component in componentAudiences)
            {
                if (hutBook.HasValue)
                {
                    stationImpressions.AddRange(_RatingsRepository.GetImpressionsDaypart((short)hutBook.Value, (short)shareBook,
                        new List<int> { component.Id }, stationDetail, playbackType));
                }
                else
                {
                    stationImpressions.AddRange(_RatingsRepository
                        .GetImpressionsDaypart(shareBook, new List<int> { component.Id }, stationDetail, playbackType)
                        .Select(x => (StationImpressions)x)
                        .ToList());
                }
                result.Add(new StationInventoryManifestAudience
                {
                    Audience = new DisplayAudience { Id = component.Id },
                    Impressions = stationImpressions.Sum(x => x.Impressions),
                    IsReference = false
                });
            }
            return result;
        }
    }
}
