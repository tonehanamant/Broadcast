﻿using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Unity.Interception.Utilities;
using static Services.Broadcast.Entities.Enums.ProposalEnums;
using StationInventoryManifest = Services.Broadcast.Entities.StationInventory.StationInventoryManifest;

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
        void AddProjectedImpressionsForComponentsToManifests(IEnumerable<StationInventoryManifest> manifests, ProposalEnums.ProposalPlaybackType? playbackType, int shareBook, int? hutBook = null);
    }

    public class ImpressionsService : IImpressionsService
    {
        private readonly INsiComponentAudienceRepository _NsiComponentAudienceRepository;
        private readonly IRatingForecastRepository _RatingsRepository;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IAffidavitRepository _AffidavitRepository;

        private readonly IStationProcessingEngine _StationProcessingEngine;

        public ImpressionsService(IDataRepositoryFactory broadcastDataRepositoryFactory, IStationProcessingEngine stationProcessingEngine)
        {
            _NsiComponentAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<INsiComponentAudienceRepository>();
            _RatingsRepository = broadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            _BroadcastAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _AffidavitRepository = broadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();

            _StationProcessingEngine = stationProcessingEngine;
        }

        public void RecalculateImpressionsForProposalDetail(int proposalDetailId)
        {
            var details = _AffidavitRepository.GetAffidavitDetails(proposalDetailId);
            var audiencesIds =
                _NsiComponentAudienceRepository.GetAllNsiComponentAudiences().
                Select(a => a.Id).
                ToList();

            using (var transaction = new TransactionScopeWrapper())
            {
                _AffidavitRepository.RemoveAffidavitAudiences(details);

                _CalculateImpressionsForAffidavitDetails(details, audiencesIds);

                _AffidavitRepository.SaveAffidavitAudiences(details);

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

                var impressionsPointInTime = _RatingsRepository.GetImpressionsPointInTime(postingData.PostingBookId.Value, audiencesIds,
                    stationDetails,
                    postingData.PostingPlaybackType.Value);

                ctr = 1;
                foreach (var affidavitFileDetail in affidavitDetailsForPostingBook)
                {
                    var imps = impressionsPointInTime.Impressions.Where(i => i.Id == ctr).ToList();

                    affidavitFileDetail.ClientScrubs.Where(s => s.PostingBookId == postingData.PostingBookId &&
                                                                         s.PostingPlaybackType == postingData.PostingPlaybackType)
                        .ForEach(s => s.ClientScrubAudiences = imps.Select(imp => new ScrubbingFileAudiences
                        {
                            ClientScrubId = s.Id,
                            AudienceId = imp.AudienceId,
                            Impressions = imp.Impressions
                        }).ToList()
                        );
                    ctr++;
                }
            }
        }

        public void AddProjectedImpressionsToManifests(IEnumerable<StationInventoryManifest> manifests, ProposalEnums.ProposalPlaybackType? playbackType, int shareBook, int? hutBook)
        {
            var manifestsByContractedAudienceList = manifests
                .Where(m => m.ManifestAudiencesReferences.Any())
                .GroupBy(m => m.ManifestAudiencesReferences.Single().Audience.Id)
                .ToList();

            foreach (var manifestsByContractedAudience in manifestsByContractedAudienceList)
            {
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
                }

                var stationImpressions = new List<StationImpressionsWithAudience>();
                foreach (var stationDetail in stationDetails)
                {
                    List<StationImpressionsWithAudience> daypartStationImpressionResult;
                    if (hutBook.HasValue)
                    {
                        var result = _LoadComponentImpressionsForTwoBooks((short)hutBook.Value, (short)shareBook, ratingAudiences, stationDetail, playbackType);
                        daypartStationImpressionResult = result.Impressions;
                    }
                    else
                    {
                        var result = _LoadComponentImpressionsForSingleBook((short)shareBook, ratingAudiences, stationDetail, playbackType);
                        daypartStationImpressionResult = result.Impressions;
                    }
                    stationImpressions.AddRange(daypartStationImpressionResult);
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
                                Impressions = impressions?.Sum(i => i.Impressions) ?? 0,
                                LegacyCallLetters = impressions.Select(i => i.LegacyCallLetters).FirstOrDefault()
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
            var stationMarketCodeDict = _StationRepository.GetBroadcastStations()
                .Where(x => x.MarketCode.HasValue)
                .ToDictionary(x => x.LegacyCallLetters, x => x.MarketCode.Value);

            var manifestsByStationDaypartList =
                   (
                       from manifest in manifests
                       from manifestDaypart in manifest.ManifestDayparts
                       select new { id = manifestDaypart.Daypart.Id, manifest, daypart = manifestDaypart.Daypart }
                   ).GroupBy(g => new { g.manifest.Station.LegacyCallLetters, g.daypart }).ToList();

            foreach (var msd in manifestsByStationDaypartList)
            {
                // skip unknown stations
                if (stationMarketCodeDict.TryGetValue(msd.Key.LegacyCallLetters, out var marketCode))
                {
                    var stationDetail = new ManifestDetailDaypart()
                    {
                        DisplayDaypart = msd.Key.daypart,
                        LegacyCallLetters = msd.Key.LegacyCallLetters
                    };

                    //get the impressions for this station detail and all the nsi components
                    var stationInventoryManifestAudiences = _LoadImpressionsForComponents(stationDetail, hutBook, shareBook, playbackType, marketCode, componentAudiences);

                    //each manifest in this group has the same impressions for the nsi componentns
                    foreach (var manifest in msd.Select(x => x.manifest).ToList())
                    {
                        manifest.ManifestAudiences.AddRange(stationInventoryManifestAudiences);
                    }
                }
            }

            // let`s take an avarage when manifest has several dayparts
            foreach (var manifest in manifests)
            {
                manifest.ManifestAudiences = manifest.ManifestAudiences
                    .GroupBy(x => x.Audience.Id)
                    .Select(x => new StationInventoryManifestAudience
                    {
                        Audience = new DisplayAudience { Id = x.Key },
                        Impressions = x.Average(a => a.Impressions),
                        IsReference = false,
                        SharePlaybackType = x.FirstOrDefault(a => a.SharePlaybackType.HasValue)?.SharePlaybackType,
                        HutPlaybackType = x.FirstOrDefault(a => a.HutPlaybackType.HasValue)?.HutPlaybackType
                    })
                    .ToList();
            }
        }

        private List<StationInventoryManifestAudience> _LoadImpressionsForComponents(ManifestDetailDaypart stationDetail,
            int? hutBook, int shareBook, ProposalPlaybackType? playbackType, int marketCode, List<BroadcastAudience> components)
        {
            List<StationImpressionsWithAudience> stationImpressions;
            ProposalPlaybackType? usedHutPlaybackType = null;
            ProposalPlaybackType? usedSharePlaybackType = null;

            var componentIds = components.Select(c => c.Id).ToList();

            if (hutBook.HasValue)
            {
                var impressionsTwoBooksResult = _LoadComponentImpressionsForTwoBooks(
                        (short)hutBook.Value,
                        (short)shareBook,
                        componentIds,
                        stationDetail,
                        playbackType);

                stationImpressions = impressionsTwoBooksResult.Impressions;

                if (stationImpressions.Any())
                {
                    usedHutPlaybackType = impressionsTwoBooksResult.UsedHutMarketPlaybackTypes.Single(x => x.MarketCode == marketCode).PlaybackType;
                    usedSharePlaybackType = impressionsTwoBooksResult.UsedShareMarketPlaybackTypes.Single(x => x.MarketCode == marketCode).PlaybackType;
                }
            }
            else
            {
                var impressionsSingleBookResult = _LoadComponentImpressionsForSingleBook(
                        shareBook,
                        componentIds,
                        stationDetail,
                        playbackType);

                stationImpressions = impressionsSingleBookResult.Impressions;

                if (stationImpressions.Any())
                {
                    usedSharePlaybackType = impressionsSingleBookResult.UsedMarketPlaybackTypes.Single(x => x.MarketCode == marketCode).PlaybackType;
                }
            }

            var result = new List<StationInventoryManifestAudience>();
            foreach (var component in components)
            {
                var resultImpressions = stationImpressions.FirstOrDefault(s => s.AudienceId == component.Id);
                result.Add(new StationInventoryManifestAudience
                {
                    Audience = new DisplayAudience { Id = component.Id },
                    Impressions = resultImpressions?.Impressions ?? 0,
                    IsReference = false,
                    SharePlaybackType = usedSharePlaybackType,
                    HutPlaybackType = usedHutPlaybackType
                });
            }
            return result;
        }

        private ImpressionsDaypartResultForTwoBooks _LoadComponentImpressionsForTwoBooks(short hutBook, short shareBook, 
            List<int> componentIds, ManifestDetailDaypart stationDaypart, ProposalPlaybackType? playbackType)
        {
            if (stationDaypart.DisplayDaypart.StartTime > stationDaypart.DisplayDaypart.EndTime) // crossing midnight
            {
                var splitDaypart = _SplitStationDaypartByMidnight(stationDaypart);

                var impressionsBeforeMidnight = _RatingsRepository.GetImpressionsDaypart(
                            (short)hutBook,
                            (short)shareBook,
                            componentIds,
                            new List<ManifestDetailDaypart> { splitDaypart.Item1 },
                            playbackType);

                var impressionsAfterMidnight = _RatingsRepository.GetImpressionsDaypart(
                            (short)hutBook,
                            (short)shareBook,
                            componentIds,
                            new List<ManifestDetailDaypart> { splitDaypart.Item2 },
                            playbackType);

                var result = impressionsBeforeMidnight.MergeImpressions(impressionsAfterMidnight, splitDaypart.Item1, splitDaypart.Item2);
                result.Impressions.ForEach(i => i.Id = stationDaypart.Id);
                return result;
            }
            else
            {
                return _RatingsRepository.GetImpressionsDaypart(
                            (short)hutBook,
                            (short)shareBook,
                            componentIds,
                            new List<ManifestDetailDaypart> { stationDaypart },
                            playbackType);
            }
        }

        private ImpressionsDaypartResultForSingleBook _LoadComponentImpressionsForSingleBook(int shareBook, 
            List<int> componentIds, ManifestDetailDaypart stationDaypart, ProposalPlaybackType? playbackType)
        {
            if (stationDaypart.DisplayDaypart.StartTime > stationDaypart.DisplayDaypart.EndTime) //crossing midnight
            {
                var splitDaypart = _SplitStationDaypartByMidnight(stationDaypart);

                var impressionsBeforeMidnight = _RatingsRepository.GetImpressionsDaypart(
                        shareBook,
                        componentIds,
                        new List<ManifestDetailDaypart> { splitDaypart.Item1 },
                        playbackType);

                var impressionsAfterMidnight = _RatingsRepository.GetImpressionsDaypart(
                        shareBook,
                        componentIds,
                        new List<ManifestDetailDaypart> { splitDaypart.Item2 },
                        playbackType);

                var result = impressionsBeforeMidnight.MergeImpressions(impressionsAfterMidnight, splitDaypart.Item1,
                    splitDaypart.Item2);
                result.Impressions.ForEach(i => i.Id = stationDaypart.Id);
                return result;
            }
            else
            {
                return _RatingsRepository.GetImpressionsDaypart(
                        shareBook,
                        componentIds,
                        new List<ManifestDetailDaypart> { stationDaypart },
                        playbackType);
            }
        }

        private Tuple<ManifestDetailDaypart, ManifestDetailDaypart> _SplitStationDaypartByMidnight(ManifestDetailDaypart stationDaypart)
        {
            if (stationDaypart.DisplayDaypart.StartTime < stationDaypart.DisplayDaypart.EndTime)
            {
                throw new ApplicationException("Can't split by midnight if not crossing midnight");
            }

            var daypartBeforeMidnight = (DisplayDaypart)stationDaypart.DisplayDaypart.Clone();
            daypartBeforeMidnight.EndTime = BroadcastConstants.OneDayInSeconds - 1;

            var daypartAfterMidnight = (DisplayDaypart)stationDaypart.DisplayDaypart.Clone();
            daypartAfterMidnight.StartTime = 0;

            var stationDayaprtBeforeMidnight = new ManifestDetailDaypart
            {
                Id = 1,
                LegacyCallLetters = stationDaypart.LegacyCallLetters,
                DisplayDaypart = daypartBeforeMidnight
            };

            var stationDayaprtAfterMidnight = new ManifestDetailDaypart
            {
                Id = 2,
                LegacyCallLetters = stationDaypart.LegacyCallLetters,
                DisplayDaypart = daypartAfterMidnight
            };

            return new Tuple<ManifestDetailDaypart, ManifestDetailDaypart>(stationDayaprtBeforeMidnight, stationDayaprtAfterMidnight);
        }
    }
}
