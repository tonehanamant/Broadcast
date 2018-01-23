using Common.Services.ApplicationServices;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public enum AffidaviteFileSource
    {
        Strata = 1
    };

    public enum ScrubbingStatus
    {
        OutOfSpec = 0,
        InSpec = 1
    }

    public interface IAffidavitService : IApplicationService
    {
        int SaveAffidavit(AffidavitSaveRequest saveRequest, string username, DateTime currentDateTime);

        ///
        /// Scrubs, but does not save results
        ///
        void ScrubAffidavitFile(affidavit_files affidavit_file);

    }
        
    public class AffidavitService : IAffidavitService
    {
        private const ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3;
        private readonly IAffidavitMatchingEngine _AffidavitMatchingEngine;
        protected readonly IProposalService _ProposalService;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly IPostingBooksService _PostingBooksService;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine; 
        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IAffidavitMatchingEngine affidavitMatchingEngine,
            IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalService proposalService,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IPostingBooksService postingBooksService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitMatchingEngine = affidavitMatchingEngine;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalService = proposalService;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
            _PostingBooksService = postingBooksService;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
        }

        public int SaveAffidavit(AffidavitSaveRequest saveRequest, string username, DateTime currentDateTime)
        {
            affidavit_files affidavit_file = new affidavit_files();
            Dictionary<int, int> spotLengthDict = null;
            if (saveRequest == null)
            {
                throw new Exception("No affidavit data received.");
            }

            affidavit_file = new affidavit_files();

            affidavit_file.created_date = DateTime.Now;
            affidavit_file.file_hash = saveRequest.FileHash;
            affidavit_file.file_name = saveRequest.FileName;
            affidavit_file.source_id = saveRequest.Source;

            var matchedAffidavitDetails = _LinkAndValidateContractIscis(saveRequest);

            foreach (var matchedAffidavitDetail in matchedAffidavitDetails)
            {
                var det = new affidavit_file_details();
                det.air_time = Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.AirTime.TimeOfDay.TotalSeconds);
                det.original_air_date = matchedAffidavitDetail.AffidavitDetail.AirTime;
                det.isci = matchedAffidavitDetail.AffidavitDetail.Isci;
                det.program_name = matchedAffidavitDetail.AffidavitDetail.ProgramName;
                det.spot_length_id = _GetSpotlength(matchedAffidavitDetail.AffidavitDetail.SpotLength, ref spotLengthDict);
                det.station = matchedAffidavitDetail.AffidavitDetail.Station;
                det.affidavit_client_scrubs =
                    matchedAffidavitDetail.ProposalDetailWeeks.Select(
                        w => new affidavit_client_scrubs
                        {
                            proposal_version_detail_quarter_week_id = w.ProposalVersionDetailQuarterWeekId,
                            match_time = w.AirtimeMatch,                      
                            modified_by = username,
                            modified_date = currentDateTime,
                            lead_in = w.IsLeadInMatch,
                            status = _GetScrubStatus(w)
                        }).ToList();

                affidavit_file.affidavit_file_details.Add(det);
            }

            ScrubAffidavitFile(affidavit_file);

            var postingBookId = _GetPostingBookId();
            _CalculateAffidavitImpressions(affidavit_file, postingBookId);

            var id = _AffidavitRepository.SaveAffidavitFile(affidavit_file);

            return id;

        }

        public void ScrubAffidavitFile(affidavit_files affidavit_file)
        {
            var callLetters = affidavit_file.affidavit_file_details.Select(a => a.station).ToList();
            var stations = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStationListByLegacyCallLetters(callLetters).ToDictionary(k => k.LegacyCallLetters, v => v);

            foreach (var affidavitFileDetail in affidavit_file.affidavit_file_details)
            {
                if (!stations.ContainsKey(affidavitFileDetail.station))
                {
                    affidavitFileDetail.affidavit_client_scrubs.ForEach(s =>
                    {
                        s.status = (int) ScrubbingStatus.OutOfSpec;
                        s.match_station = false;
                    });
                    continue;
                }

                var quarterWeekIds =
                    affidavitFileDetail.affidavit_client_scrubs.Select(s => s.proposal_version_detail_quarter_week_id).ToList();
                var stationManifests = _BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalOpenMarketInventoryRepository>()
                    .GetStationManifestFromQuarterWeeks(quarterWeekIds);
                var proposals = _ProposalService.GetProposalsByQuarterWeeks(quarterWeekIds);

                var affidavitStation = stations[affidavitFileDetail.station];
                foreach (var scrub in affidavitFileDetail.affidavit_client_scrubs)
                {
                    if (!stationManifests.Any())
                    {
                        scrub.match_station = false;
                        scrub.match_market = false;
                    }
                    else
                    {
                        var quarterWeekId = scrub.proposal_version_detail_quarter_week_id;
                        var scrubManifests = stationManifests[quarterWeekId];
                        scrub.match_station = true;
                        scrub.match_market = true;


                        if (scrubManifests.All(m =>
                            m.station.legacy_call_letters != affidavitStation.LegacyCallLetters))
                        {
                            scrub.match_station = false;
                            scrub.match_market = false;
                        }
                        else
                        {
                            var proposal = proposals[quarterWeekId];
                            var detail = proposal.Details.Single(d =>
                                d.Quarters.Any(q => q.Weeks.Any(w => w.Id == quarterWeekId)));

                            var markets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposal, detail);

                            var marketGeoName = affidavitStation.OriginMarket;
                            scrub.match_market = false;
                            if (markets.Any(m => m.Display == marketGeoName))
                            {
                                scrub.match_market = true;
                            }
                        }
                    }

                    EnsureScrubadubdubed(scrub);
                }

            }

        }

        private void EnsureScrubadubdubed(affidavit_client_scrubs scrub)
        {
            scrub.status = (int)ScrubbingStatus.OutOfSpec;
            if (scrub.match_station
                && scrub.match_market
                && scrub.match_genre
                && scrub.match_program
                && scrub.match_time)
            {
                scrub.status = (int) ScrubbingStatus.InSpec;
            }
        }
        private int _GetScrubStatus(AffidavitMatchingProposalWeek affidavitMatchingProposalWeek)
        {
                if (!affidavitMatchingProposalWeek.AirtimeMatch)
                    return (int) AffidavitClientScrubStatus.OutOfSpec;
            
                return (int) AffidavitClientScrubStatus.InSpec;
        }

        private List<AffidavitMatchingDetail> _LinkAndValidateContractIscis(AffidavitSaveRequest saveRequest)
        {
            var matchedAffidavitDetails = new List<AffidavitMatchingDetail>();
            var matchingProblems = new List<String>();

            foreach (var requestDetail in saveRequest.Details)
            {
                var proposalWeeks =
                    _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                        .GetAffidavitMatchingProposalWeeksByHouseIsci(requestDetail.Isci);

                var matchedProposalWeeks = _AffidavitMatchingEngine.Match(requestDetail, proposalWeeks); //TODO: combine errors and matches in one results in order to avoid concurrency issues.
                matchingProblems.AddRange(_AffidavitMatchingEngine.MatchingProblems());
                matchedAffidavitDetails.Add(new AffidavitMatchingDetail()
                {
                    AffidavitDetail = requestDetail,
                    ProposalDetailWeeks = matchedProposalWeeks
                });

            }
            if (matchingProblems.Any())
            {
                throw new BroadcastAffidavitException("Found isci problems in the system", matchingProblems);
            }

            return matchedAffidavitDetails;
        }

        private int _GetSpotlength(int spotLength, ref Dictionary<int, int> spotLengthDict)
        {
            if (spotLengthDict == null)
                spotLengthDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();

            if (!spotLengthDict.ContainsKey(spotLength))
                throw new Exception(string.Format("Invalid spot length '{0}' found.", spotLength));

            return spotLengthDict[spotLength];
        }

        private int _GetPostingBookId()
        {
            var defaultPostingBooks = _PostingBooksService.GetDefaultPostingBooks();

            if (!defaultPostingBooks.DefaultShareBook.PostingBookId.HasValue)
                throw new Exception("No default posting book available");

            return defaultPostingBooks.DefaultShareBook.PostingBookId.Value;
        }

        private void _CalculateAffidavitImpressions(affidavit_files affidavitFile, int postingBookId)
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