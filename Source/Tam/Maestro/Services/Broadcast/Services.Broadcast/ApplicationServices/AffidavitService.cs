using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitService : IApplicationService
    {
        WWTVSaveResult SaveAffidavit(InboundFileSaveRequest saveRequest, string username, DateTime currentDateTime);

        WWTVSaveResult SaveAffidavitValidationErrors(InboundFileSaveRequest saveRequest, string userName, List<WWTVInboundFileValidationResult> affidavitValidationResults);

        /// <summary>
        /// Scrubs an affidavit detail by an isci
        /// </summary>
        /// <param name="isci">Isci to scrub</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">User requesting the scrubbing</param>
        /// <returns>True or false</returns>
        bool ScrubUnlinkedAffidavitDetailsByIsci(string isci, DateTime currentDateTime, string username);

        bool RescrubProposalDetail(RescrubProposalDetailRequest request, string userName, DateTime changeDate);
        bool CanRescrubProposalDetail(ProposalDto proposal, ProposalDetailDto proposalDetail);

        string JSONifyFile(Stream rawStream, string fileName, out InboundFileSaveRequest request);

        /// <summary>
        /// Swaps one or more client scrubs to another proposal detail
        /// </summary>
        /// <param name="requestData">SwapProposalDetailRequest object containing a list of affidavit client scrubs to be swapped and the proposal detail id to swap to</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">Username requesting the change</param>
        /// <returns>True or false</returns>
        bool SwapProposalDetails(SwapProposalDetailRequest requestData, DateTime currentDateTime, string username);

        /// <summary>
        /// Maps an original isci to an effective isci
        /// </summary>
        /// <param name="mapIsciDto">MapIsciDto object containing the iscis to map</param>
        /// <param name="name">User requesting the mapping</param>
        /// <returns>The result of the mapping in true or false</returns>
        bool MapIsci(MapIsciDto mapIsciDto, DateTime currentDateTime, string name);

        /// <summary>
        /// Returns a list of the posts and the number of unlinked iscis
        /// </summary>
        /// <returns>List of PostDto objects and the number of unlinked iscis</returns>
        PostedContractedProposalsDto GetPosts();

        /// <summary>
        /// Gets a client post scrubbing proposal with details
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns>ClientPostScrubbingProposalDto object containing the post scrubbing information</returns>
        ClientPostScrubbingProposalDto GetClientScrubbingForProposal(int proposalId, ProposalScrubbingRequest proposalScrubbingRequest, List<ProposalDetailPostScrubbingDto> scrubs = null);

        /// <summary>
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<UnlinkedIscisDto> GetUnlinkedIscis();

        /// <summary>
        /// Returns a list of archived iscis
        /// </summary>
        /// <returns>List of ArchivedIscisDto objects</returns>
        List<ArchivedIscisDto> GetArchivedIscis();

        ClientPostScrubbingProposalDto OverrideScrubbingStatus(ScrubStatusOverrideRequest scrubStatusOverrides);

        /// <summary>
        /// Archives an isci from the unlinked isci list
        /// </summary>
        /// <param name="iscis">Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        /// <returns>True or false based on the errors</returns>
        bool ArchiveUnlinkedIsci(List<string> iscis, string username);

        /// <summary>
        /// Undo the archive process of a list of iscis
        /// </summary>
        /// <param name="fileDetailsIds">List of affidavit file detail ids</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="name">User requesting the undo operation</param>
        /// <returns>True or false</returns>
        bool UndoArchiveUnlinkedIsci(List<long> fileDetailsIds, DateTime currentDateTime, string name);

        /// <summary>
        /// Undo the overriding of an affidavit client scrub status
        /// </summary>
        /// <param name="request">ScrubStatusOverrideRequest object containing the ids of the records to undo</param>
        /// <returns>ClientPostScrubbingProposalDto object</returns>
        ClientPostScrubbingProposalDto UndoOverrideScrubbingStatus(ScrubStatusOverrideRequest request);        
    }


    public class AffidavitService : IAffidavitService
    {
        public const string ProposalNotContractedMessage = "Proposal must be contracted";

        const string ARCHIVED_ISCI = "Not a Cadent Isci";
        private const ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3;

        private readonly IMatchingEngine _AffidavitMatchingEngine;
        private readonly IProgramScrubbingEngine _AffidavitProgramScrubbingEngine;
        private readonly IProposalService _ProposalService;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IProposalRepository _ProposalRepository;
        private readonly IPostRepository _PostRepository;
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IAffidavitValidationEngine _AffidavitValidationEngine;
        private readonly INsiPostingBookService _NsiPostingBookService;
        private readonly IImpressionsService _ImpressionsService;
        private readonly Dictionary<int, int> _SpotLengthsDict;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly ISMSClient _SmsClient;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IIsciService _IsciService;

        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IMatchingEngine affidavitMatchingEngine
            , IProgramScrubbingEngine affidavitProgramScrubbingEngine
            , IProposalMarketsCalculationEngine proposalMarketsCalculationEngine
            , IProposalService proposalService
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IAffidavitValidationEngine affidavitValidationEngine
            , INsiPostingBookService nsiPostingBookService
            , IImpressionsService affidavitImpressionsService
            , IStationProcessingEngine stationProcessingEngine
            , IImpressionAdjustmentEngine impressionAdjustmentEngine
            , IBroadcastAudiencesCache audiencesCache
            , ISMSClient smsClient
            , IIsciService isciService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitMatchingEngine = affidavitMatchingEngine;
            _AffidavitProgramScrubbingEngine = affidavitProgramScrubbingEngine;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalService = proposalService;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _ProposalRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            _PostRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _AffidavitValidationEngine = affidavitValidationEngine;
            _NsiPostingBookService = nsiPostingBookService;
            _ImpressionsService = affidavitImpressionsService;
            _SpotLengthsDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            _StationProcessingEngine = stationProcessingEngine;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
            _AudiencesCache = audiencesCache;
            _SmsClient = smsClient;
            _BroadcastAudienceRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _IsciService = isciService;
        }

        public WWTVSaveResult SaveAffidavitValidationErrors(InboundFileSaveRequest saveRequest, string userName, List<WWTVInboundFileValidationResult> affidavitValidationResults)
        {
            var affidavitFile = _EnsureAffidavitFile(saveRequest, DateTime.Now);

            var problems = _MapValidationErrorToAffidavitFileProblem((affidavitValidationResults));
            affidavitFile.FileProblems.AddRange(problems);
            var result = new WWTVSaveResult
            {
                ValidationResults = affidavitValidationResults
            };
            affidavitFile.Status = affidavitValidationResults.Any() ? FileProcessingStatusEnum.Invalid : FileProcessingStatusEnum.Valid;

            if (affidavitValidationResults.Any())
            {   // save and get out
                result.Id = _AffidavitRepository.SaveAffidavitFile(affidavitFile);
            }
            return result;
        }

        private static ScrubbingFile _EnsureAffidavitFile(InboundFileSaveRequest saveRequest, DateTime currentDateTime)
        {
            if (saveRequest == null)
            {
                throw new Exception("No affidavit data received.");
            }

            var affidavitFile = new ScrubbingFile
            {
                CreatedDate = currentDateTime,
                FileHash = saveRequest.FileHash,
                FileName = saveRequest.FileName,
                SourceId = saveRequest.Source
            };
            return affidavitFile;
        }

        public WWTVSaveResult SaveAffidavit(InboundFileSaveRequest saveRequest, string username, DateTime currentDateTime)
        {
            var affidavitFile = _EnsureAffidavitFile(saveRequest, currentDateTime);
            var result = _AffidavitSaveResult(saveRequest, username, currentDateTime, affidavitFile);

            return result;
        }

        private WWTVSaveResult _AffidavitSaveResult(InboundFileSaveRequest saveRequest, string username,
            DateTime currentDateTime, ScrubbingFile affidavitFile)
        {
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);

            var result = new WWTVSaveResult();
            var affidavitValidationResults = new List<WWTVInboundFileValidationResult>();

            var lineNumber = 0;
            foreach (var saveRequestDetail in saveRequest.Details)
            {
                lineNumber++;
                var validationErrors = _AffidavitValidationEngine.ValidateAffidavitRecord(saveRequestDetail);

                if (validationErrors.Any())
                {
                    validationErrors.ForEach(r => r.InvalidLine = lineNumber);
                    var problems = _MapValidationErrorToAffidavitFileProblem(validationErrors);
                    affidavitFile.FileProblems.AddRange(problems);
                    affidavitValidationResults.AddRange(validationErrors);
                }
            }

            result.ValidationResults = affidavitValidationResults;
            affidavitFile.Status = affidavitValidationResults.Any() ? FileProcessingStatusEnum.Invalid : FileProcessingStatusEnum.Valid;

            if (affidavitValidationResults.Any())
            {   // save and get out
                result.Id = _AffidavitRepository.SaveAffidavitFile(affidavitFile);
                return result;
            }

            var affidavitFileDetailsToBeLinked = _MapToAffidavitFileDetails(saveRequest.Details);
            var affidavitDetailsToNotLink = new List<ScrubbingFileDetail>();

            foreach (var detail in affidavitFileDetailsToBeLinked)
            {
                if (_PostRepository.IsIsciBlacklisted(new List<string> { detail.Isci }))
                {
                    detail.FileDetailProblems.Add(new FileDetailProblem
                    {
                        Description = ARCHIVED_ISCI,
                        Type = FileDetailProblemTypeEnum.ArchivedIsci
                    });
                    detail.Archived = true;
                    affidavitFile.FileDetails.Add(detail);
                    affidavitDetailsToNotLink.Add(detail);
                }
            }

            affidavitFileDetailsToBeLinked.RemoveAll(d => affidavitDetailsToNotLink.Contains(d));

            _IsciService.LoadIsciMappings(affidavitFileDetailsToBeLinked);
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(affidavitFileDetailsToBeLinked);
            _SetPostingBookData(matchedAffidavitDetails, postingBookId);

            //load AffidavitClientScrubs and AffidavitDetailProblems
            _MapClientScrubsAndFileDetailProblems(matchedAffidavitDetails, currentDateTime, username);

            affidavitFile.FileDetails.AddRange(matchedAffidavitDetails.Select(x => x.FileDetail).ToList());

            _ScrubMatchedAffidavitRecords(affidavitFile.FileDetails);
            _ImpressionsService.CalculateImpressionsForFileDetails(affidavitFile.FileDetails);

            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                result.Id = _AffidavitRepository.SaveAffidavitFile(affidavitFile);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(affidavitFile.FileDetails));
                transaction.Complete();
            }

            return result;
        }

        private List<ScrubbingFileDetail> _MapToAffidavitFileDetails(List<InboundFileSaveRequestDetail> details)
        {
            var result = details.Select(d => new ScrubbingFileDetail()
            {
                AirTime = Convert.ToInt32(d.AirTime.TimeOfDay.TotalSeconds),
                OriginalAirDate = d.AirTime,
                Isci = d.Isci,
                ProgramName = d.ProgramName,
                Genre = d.Genre,
                SpotLengthId = _GetSpotlength(d.SpotLength),
                Station = d.Station,
                Market = d.Market,
                Affiliate = d.Affiliate,
                EstimateId = d.EstimateId,
                InventorySource = (int)d.InventorySource,
                SpotCost = d.SpotCost,
                LeadinGenre = d.LeadInGenre,
                LeadoutGenre = d.LeadOutGenre,
                LeadinProgramName = d.LeadInProgramName,
                LeadoutProgramName = d.LeadOutProgramName,
                LeadInEndTime = d.LeadInEndTime == null ? (int?)null : Convert.ToInt32(d.LeadInEndTime.Value.TimeOfDay.TotalSeconds),
                LeadOutStartTime = d.LeadOutStartTime == null ? (int?)null : Convert.ToInt32(d.LeadOutStartTime.Value.TimeOfDay.TotalSeconds),
                ShowType = d.ShowType,
                LeadInShowType = d.LeadInShowType,
                LeadOutShowType = d.LeadOutShowType,
                Demographics = d.Demographics,
                SuppliedProgramName = d.SuppliedProgramName
            }).ToList();
            return result;
        }

        public bool RescrubProposalDetail(RescrubProposalDetailRequest request, string userName, DateTime changeDate)
        {
            var proposal = _ProposalService.GetProposalById(request.ProposalId);
            var proposalDetail = proposal.Details.Single(d => d.Id == request.ProposalDetailId);

            EnsureProposalContracted(proposal, proposalDetail);

            var affidavitDetails = _AffidavitRepository.GetAffidavitDetails(proposalDetail.Id.Value);

            // use swap parameter to keep detail the same.
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(affidavitDetails, proposalDetail.Id.Value);
            _SetPostingBookData(matchedAffidavitDetails, proposalDetail.PostingBookId.Value, proposalDetail.PostingPlaybackType);

            _MapClientScrubsAndFileDetailProblems(matchedAffidavitDetails, changeDate, userName);
            affidavitDetails = matchedAffidavitDetails.Select(ad => ad.FileDetail).ToList();

            _ScrubMatchedAffidavitRecords(affidavitDetails);
            _ImpressionsService.CalculateImpressionsForFileDetails(affidavitDetails);

            using (var transaction = new TransactionScopeWrapper())
            {
                _AffidavitRepository.SaveScrubbedFileDetails(affidavitDetails);
                transaction.Complete();
            }

            return true;
        }

        public bool CanRescrubProposalDetail(ProposalDto proposal, ProposalDetailDto proposalDetail)
        {
            if (proposal.Status != ProposalEnums.ProposalStatusType.Contracted)
                return false;

            var affidavitDetails = _AffidavitRepository.GetAffidavitDetails(proposalDetail.Id.Value);
            if (!affidavitDetails.Any())
                return false;

            return true;
        }

        private void EnsureProposalContracted(ProposalDto proposal, ProposalDetailDto proposalDetail)
        {
            if (proposal.Status != ProposalEnums.ProposalStatusType.Contracted)
                throw new InvalidOperationException(ProposalNotContractedMessage);
        }

        /// <summary>
        /// Scrubs an affidavit detail by an isci
        /// </summary>
        /// <param name="isci">Isci to scrub</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">User requesting the scrubbing</param>
        /// <returns>True or false</returns>
        public bool ScrubUnlinkedAffidavitDetailsByIsci(string isci, DateTime currentDateTime, string username)
        {
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);
            var unlinkedAffidavitDetails = _AffidavitRepository.GetUnlinkedAffidavitDetailsByIsci(isci);
            _IsciService.LoadIsciMappings(unlinkedAffidavitDetails);
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(unlinkedAffidavitDetails);
            _SetPostingBookData(matchedAffidavitDetails, postingBookId);

            //load AffidavitClientScrubs and AffidavitDetailProblems
            _MapClientScrubsAndFileDetailProblems(matchedAffidavitDetails, currentDateTime, username);

            var affidavitFileDetails = matchedAffidavitDetails.Select(d => d.FileDetail).ToList();
            _ScrubMatchedAffidavitRecords(affidavitFileDetails);
            _ImpressionsService.CalculateImpressionsForFileDetails(affidavitFileDetails);
            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                _AffidavitRepository.SaveScrubbedFileDetails(affidavitFileDetails);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(affidavitFileDetails));
                transaction.Complete();
            }
            return true;
        }

        private void _MapClientScrubsAndFileDetailProblems(List<MatchingDetail> matchedAffidavitDetails, DateTime currentDateTime, string username)
        {
            foreach (var matchedAffidavitDetail in matchedAffidavitDetails)
            {
                matchedAffidavitDetail.FileDetail.ClientScrubs = matchedAffidavitDetail.ProposalDetailWeeks.Select(
                            w => new ClientScrub
                            {
                                ProposalVersionDetailQuarterWeekId = w.ProposalVersionDetailQuarterWeekId,
                                ProposalVersionDetailId = w.ProposalVersionDetailId,
                                MatchTime = w.TimeMatch,
                                MatchDate = w.DateMatch,
                                ModifiedBy = username,
                                ModifiedDate = currentDateTime,
                                LeadIn = w.IsLeadInMatch,
                                PostingBookId = w.ProposalVersionDetailPostingBookId.Value,
                                PostingPlaybackType = w.ProposalVersionDetailPostingPlaybackType,
                                EffectiveIsci = matchedAffidavitDetail.EffectiveIsci,
                                EffectiveClientIsci = w.ClientIsci
                            }).ToList();
                matchedAffidavitDetail.FileDetail.FileDetailProblems = matchedAffidavitDetail.FileDetailProblems;
            }
        }

        /// <summary>
        /// Maps an original isci to an effective isci
        /// </summary>
        /// <param name="mapIsciDto">MapIsciDto object containing the iscis to map</param>
        /// <param name="username">User requesting the mapping</param>
        /// <returns>The result of the mapping in true or false</returns>
        public bool MapIsci(MapIsciDto mapIsciDto, DateTime currentDateTime, string username)
        {
            _IsciService.AddIsciMapping(mapIsciDto, username);
            return ScrubUnlinkedAffidavitDetailsByIsci(mapIsciDto.OriginalIsci, currentDateTime, username);
        }

        /// <summary>
        /// Swaps one or more client scrubs to another proposal detail
        /// </summary>
        /// <param name="requestData">SwapProposalDetailRequest object containing a list of affidavit client scrubs to be swapped and the proposal detail id to swap to</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">Username requesting the change</param>
        /// <returns>True or false</returns>
        public bool SwapProposalDetails(SwapProposalDetailRequest requestData, DateTime currentDateTime, string username)
        {
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);
            List<ScrubbingFileDetail> affidavitDetails = _AffidavitRepository.GetAffidavitDetailsByClientScrubIds(requestData.ScrubbingIds);
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(affidavitDetails, requestData.ProposalDetailId);
            _SetPostingBookData(matchedAffidavitDetails, postingBookId);

            //load AffidavitClientScrubs and AffidavitDetailProblems
            _MapClientScrubsAndFileDetailProblems(matchedAffidavitDetails, currentDateTime, username);

            var affidavitFileDetails = matchedAffidavitDetails.Select(d => d.FileDetail).ToList();
            _ScrubMatchedAffidavitRecords(affidavitFileDetails);
            _ImpressionsService.CalculateImpressionsForFileDetails(affidavitFileDetails);
            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                _AffidavitRepository.SaveScrubbedFileDetails(affidavitFileDetails);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(affidavitFileDetails));
                transaction.Complete();
            }
            return true;
        }

        private List<ProposalDetailPostingData> _GetProposalDetailIdsWithPostingBookId(List<ScrubbingFileDetail> affidavitFileDetails)
        {
            var result = affidavitFileDetails.SelectMany(d => d.ClientScrubs.Select(s => new ProposalDetailPostingData
            {
                ProposalVersionDetailId = s.ProposalVersionDetailId,
                PostingBookId = s.PostingBookId.Value,
                PostingPlaybackType = s.PostingPlaybackType.Value
            })).ToList();

            return result;
        }

        private void _SetPostingBookData(List<MatchingDetail> matchedAffidavitDetails, int postingBookId, ProposalEnums.ProposalPlaybackType? playbackType = null)
        {
            foreach (var proposalDetailWeek in matchedAffidavitDetails.SelectMany(d => d.ProposalDetailWeeks))
            {
                if (!proposalDetailWeek.ProposalVersionDetailPostingBookId.HasValue)
                {
                    proposalDetailWeek.ProposalVersionDetailPostingBookId = postingBookId;
                }

                if (!proposalDetailWeek.ProposalVersionDetailPostingPlaybackType.HasValue)
                {
                    if (playbackType == null)
                    {
                        proposalDetailWeek.ProposalVersionDetailPostingPlaybackType = DefaultPlaybackType;
                    }
                    else
                    {
                        proposalDetailWeek.ProposalVersionDetailPostingPlaybackType = playbackType.Value;
                    }
                }
            }
        }

        private void _ScrubMatchedAffidavitRecords(List<ScrubbingFileDetail> affidavitFileDetails)
        {
            var stations = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStations().ToDictionary(k => k.LegacyCallLetters, v => v);

            foreach (var affidavitDetail in affidavitFileDetails)
            {
                if (!affidavitDetail.ClientScrubs.Any())
                    continue;

                var quarterWeekIds = affidavitDetail.ClientScrubs.Select(s => s.ProposalVersionDetailQuarterWeekId).ToList();
                var proposals = _ProposalService.GetProposalsByQuarterWeeks(quarterWeekIds);

                var affidavitStation = _MatchAffidavitStation(stations, affidavitDetail);

                foreach (ClientScrub scrub in affidavitDetail.ClientScrubs)
                {
                    var quarterWeekId = scrub.ProposalVersionDetailQuarterWeekId;
                    var proposal = proposals[quarterWeekId];
                    var proposalDetail = proposal.Details.Single(d =>
                        d.Quarters.Any(q => q.Weeks.Any(w => w.Id == quarterWeekId)));
                    var proposalWeek = proposalDetail.Quarters.SelectMany(d => d.Weeks.Where(w => w.Id == quarterWeekId)).First();
                    var proposalWeekIsci = proposalWeek.Iscis.Any(i => i.HouseIsci.Equals(affidavitDetail.Isci, StringComparison.InvariantCultureIgnoreCase))
                        ? proposalWeek.Iscis.First(i => i.HouseIsci.Equals(affidavitDetail.Isci, StringComparison.InvariantCultureIgnoreCase))
                        : !string.IsNullOrWhiteSpace(affidavitDetail.MappedIsci) ? proposalWeek.Iscis.First(i => i.HouseIsci.Equals(affidavitDetail.MappedIsci, StringComparison.InvariantCultureIgnoreCase))
                                : null;

                    if (proposalWeekIsci == null)
                    {
                        scrub.MatchIsci = false;
                    }
                    else
                    {
                        scrub.MatchIsciDays = _IsIsciDaysMatch(proposalWeekIsci, affidavitDetail.OriginalAirDate.DayOfWeek);
                        scrub.MatchIsci = true;
                        scrub.EffectiveClientIsci = proposalWeekIsci.ClientIsci;
                    }

                    // match market/station
                    scrub.MatchStation = false;
                    scrub.MatchMarket = false;

                    if (affidavitStation != null)
                    {
                        var markets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposal);

                        var marketGeoName = affidavitStation.OriginMarket;
                        if (markets.Any(m => m.Display == marketGeoName))
                        {
                            affidavitDetail.Market = marketGeoName;
                            scrub.MatchStation = true;
                            scrub.MatchMarket = true;
                        }
                    }

                    _AffidavitProgramScrubbingEngine.Scrub(proposalDetail, affidavitDetail, scrub);

                    scrub.Status = (scrub.MatchStation && scrub.MatchMarket && scrub.MatchGenre && scrub.MatchProgram && scrub.MatchTime && scrub.MatchIsciDays && scrub.MatchDate && scrub.MatchShowType && scrub.MatchIsci)
                        ? ScrubbingStatus.InSpec
                        : ScrubbingStatus.OutOfSpec;
                }
            }
        }

        private DisplayBroadcastStation _MatchAffidavitStation(Dictionary<string, DisplayBroadcastStation> stations, ScrubbingFileDetail affidavitDetail)
        {
            string stationName = _StationProcessingEngine.StripStationSuffix(affidavitDetail.Station);

            if (stations.ContainsKey(stationName))
            {
                return stations[stationName];
            }

            return null;
        }

        public List<FileProblem> _MapValidationErrorToAffidavitFileProblem(List<WWTVInboundFileValidationResult> affidavitValidationResults)
        {
            List<FileProblem> problems = new List<FileProblem>();

            affidavitValidationResults.ForEach(v =>
            {
                FileProblem problem = new FileProblem();
                var description = v.ErrorMessage;
                if (!string.IsNullOrEmpty(v.InvalidField))
                {
                    description = string.Format("Record: {0}: Field: '{1}' is invalid\r\n{2}", v.InvalidLine, v.InvalidField, v.ErrorMessage);
                }
                problem.ProblemDescription = description;
                problems.Add(problem);
            });
            return problems;
        }

        private bool _IsIsciDaysMatch(ProposalWeekIsciDto proposalWeekIsci, DayOfWeek dayOfWeek)
        {
            var isMatch = false;

            if (proposalWeekIsci.Sunday)
                isMatch = dayOfWeek == DayOfWeek.Sunday;

            if (proposalWeekIsci.Monday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Monday;

            if (proposalWeekIsci.Tuesday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Tuesday;

            if (proposalWeekIsci.Wednesday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Wednesday;

            if (proposalWeekIsci.Thursday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Thursday;

            if (proposalWeekIsci.Friday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Friday;

            if (proposalWeekIsci.Saturday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Saturday;

            return isMatch;
        }

        private List<MatchingDetail> _LinkAndValidateContractIscis(List<ScrubbingFileDetail> affidavitDetails, int swapProposalDetailId = 0)
        {
            bool isIsciMapped = false;
            var matchedAffidavitDetails = new List<MatchingDetail>();
            List<MatchingProposalWeek> proposalWeeks = new List<MatchingProposalWeek>();
            bool swappingProposalDetail = swapProposalDetailId > 0;
            if (swappingProposalDetail)
            {
                proposalWeeks = _ProposalRepository.GetMatchingProposalWeeksByDetailId(swapProposalDetailId);
                swappingProposalDetail = true;
            }
            foreach (var affidavitDetail in affidavitDetails)
            {
                if (!swappingProposalDetail)
                {
                    proposalWeeks = _ProposalRepository.GetMatchingProposalWeeksByHouseIsci(affidavitDetail.Isci);
                }

                var matchedProposalWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);
                if (!matchedProposalWeeks.Any() && !string.IsNullOrWhiteSpace(affidavitDetail.MappedIsci))
                {
                    proposalWeeks = _ProposalRepository.GetMatchingProposalWeeksByHouseIsci(affidavitDetail.MappedIsci);
                    matchedProposalWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);
                    isIsciMapped = true;
                }

                var matchingProblems = _AffidavitMatchingEngine.GetMatchingProblems();
                matchedAffidavitDetails.Add(new MatchingDetail()
                {
                    FileDetail = affidavitDetail,
                    ProposalDetailWeeks = matchedProposalWeeks,
                    FileDetailProblems = matchingProblems,
                    EffectiveIsci = isIsciMapped ? affidavitDetail.MappedIsci : affidavitDetail.Isci
                });
            }

            return matchedAffidavitDetails;
        }

        private int _GetSpotlength(int spotLength)
        {

            if (!_SpotLengthsDict.ContainsKey(spotLength))
                throw new Exception(string.Format("Invalid spot length '{0}' found.", spotLength));

            return _SpotLengthsDict[spotLength];
        }

        /// <summary>
        /// Returns a list of the posts and unlinked iscis count in the system
        /// </summary>
        /// <returns>List of PostDto objects</returns>
        public PostedContractedProposalsDto GetPosts()
        {
            var postedProposals = _PostRepository.GetAllPostedProposals();

            foreach (var post in postedProposals)
            {
                _SetPostAdvertiser(post);

                var houseHoldAudienceId = _AudiencesCache.GetDefaultAudience().Id;
                post.PrimaryAudienceDeliveredImpressions = _GetImpressionsForAudience(post.ContractId, post.PostType, post.GuaranteedAudienceId, post.Equivalized);
                post.HouseholdDeliveredImpressions = _GetImpressionsForAudience(post.ContractId, post.PostType, houseHoldAudienceId, post.Equivalized);
                post.PrimaryAudienceDelivery = post.PrimaryAudienceDeliveredImpressions / post.PrimaryAudienceBookedImpressions * 100;
            }

            return new PostedContractedProposalsDto()
            {
                Posts = postedProposals,
                UnlinkedIscis = _PostRepository.CountUnlinkedIscis()
            };
        }

        private double _GetImpressionsForAudience(int contractId, SchedulePostType type, int audienceId, bool equivalized)
        {
            var impressionsDataGuaranteed = _GetPostImpressionsData(contractId, audienceId);
            double deliveredImpressions = 0;
            foreach (var impressionData in impressionsDataGuaranteed)
            {
                var impressions = _ImpressionAdjustmentEngine.AdjustImpression(impressionData.Impressions, equivalized, _SpotLengthsDict.Single(x => x.Value == impressionData.SpotLengthId).Key);
                
                if (type == SchedulePostType.NTI)
                {
                    impressions = _ImpressionAdjustmentEngine.AdjustImpression(impressions, impressionData.NtiConversionFactor);
                }
                deliveredImpressions += impressions;
            }
            return deliveredImpressions;
        }

        private void _SetPostAdvertiser(PostedContracts post)
        {
            var advertiserLookupDto = _SmsClient.FindAdvertiserById(post.AdvertiserId);
            post.Advertiser = advertiserLookupDto.Display;
        }

        private List<PostImpressionsData> _GetPostImpressionsData(int contractId, int maestroAudienceId)
        {
            var ratingsAudiencesIds = _BroadcastAudienceRepository
                .GetRatingsAudiencesByMaestroAudience(new List<int> { maestroAudienceId })
                .Select(x => x.rating_audience_id)
                .ToList();

            return _PostRepository.GetPostImpressionsData(contractId, ratingsAudiencesIds);
        }

        /// <summary>
        /// Gets a client post scrubbing proposal with details
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns>ClientPostScrubbingProposalDto object containing the post scrubbing information</returns>
        public ClientPostScrubbingProposalDto GetClientScrubbingForProposal(int proposalId, ProposalScrubbingRequest proposalScrubbingRequest, List<ProposalDetailPostScrubbingDto> clientScrubs = null)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(proposalId);
                var advertiser = _SmsClient.FindAdvertiserById(proposal.AdvertiserId);

                ClientPostScrubbingProposalDto result = new ClientPostScrubbingProposalDto
                {
                    Id = proposal.Id.Value,
                    Name = proposal.ProposalName,
                    Notes = proposal.Notes,
                    Markets = proposal.Markets,
                    MarketGroupId = proposal.MarketGroupId,
                    BlackoutMarketGroup = proposal.BlackoutMarketGroup,
                    BlackoutMarketGroupId = proposal.BlackoutMarketGroupId,
                    Details = _MapToProposalDetailDto(proposal.Details, proposal.SpotLengths),
                    GuaranteedDemo = _AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString,
                    Advertiser = advertiser != null ? advertiser.Display : string.Empty,
                    SecondaryDemos = proposal.SecondaryDemos.Select(x => _AudiencesCache.GetDisplayAudienceById(x).AudienceString).ToList()
                };

                ///Load all Client Scrubs
                if (clientScrubs == null)
                {
                    clientScrubs = _MapClientScrubDataToDto(_AffidavitRepository.GetProposalDetailPostScrubbing(proposalId, proposalScrubbingRequest.ScrubbingStatusFilter), _SpotLengthsDict);
                    _SetClientScrubsMarketAndAffiliate(clientScrubs);
                }

                //Set Sequence and ProposalDetailId
                result.Details.ForEach(x =>
                {
                    var detailClientScrubs = clientScrubs.Where(cs => cs.ProposalDetailId == x.Id.Value).ToList();
                    detailClientScrubs.ForEach(y =>
                    {
                        y.Sequence = x.Sequence;
                        y.ProposalDetailId = x.Id;
                    });
                    result.ClientScrubs.AddRange(detailClientScrubs);
                });

                ///Load Filters
                result.Filters = _LoadFilters(result.ClientScrubs);

                return result;
            }
        }

        private List<ClientPostScrubbingProposalDetailDto> _MapToProposalDetailDto(List<ProposalDetailDto> details, List<LookupDto> spotLengths)
        {
            return details.Select(x => new ClientPostScrubbingProposalDetailDto
            {
                Id = x.Id,
                FlightStartDate = x.FlightStartDate,
                FlightEndDate = x.FlightEndDate,
                FlightWeeks = x.FlightWeeks,
                SpotLength = spotLengths.First(y => y.Id == x.SpotLengthId).Display,
                DayPart = x.Daypart.Text,
                Programs = x.ProgramCriteria,
                Genres = x.GenreCriteria,
                Sequence = x.Sequence,
            }).OrderBy(x => x.Sequence).ToList();
        }

        private FilterOptions _LoadFilters(List<ProposalDetailPostScrubbingDto> clientScrubs)
        {
            return new FilterOptions
            {
                DistinctDayOfWeek = clientScrubs.Select(x => x.DayOfWeek).Distinct().OrderBy(x => x).ToList(),
                DistinctGenres = clientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.GenreName)).Select(x => x.GenreName).Distinct().OrderBy(x => x).ToList(),
                DistinctPrograms = clientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ProgramName)).Select(x => x.ProgramName).Distinct().OrderBy(x => x).ToList(),
                WeekStart = clientScrubs.Any() ? clientScrubs.Select(x => x.WeekStart).OrderBy(x => x).First() : (DateTime?)null,
                WeekEnd = clientScrubs.Any() ? clientScrubs.Select(x => x.WeekStart).OrderBy(x => x).Last().AddDays(7) : (DateTime?)null,
                DateAiredStart = clientScrubs.Any() ? clientScrubs.Select(x => x.DateAired).OrderBy(x => x).First() : (DateTime?)null,
                DateAiredEnd = clientScrubs.Any() ? clientScrubs.Select(x => x.DateAired).OrderBy(x => x).Last() : (DateTime?)null,
                DistinctMarkets = clientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.Market)).Select(x => x.Market).Distinct().OrderBy(x => x).ToList(),
                DistinctClientIscis = clientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ClientISCI)).Select(x => x.ClientISCI).Distinct().OrderBy(x => x).ToList(),
                DistinctHouseIscis = clientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ISCI)).Select(x => x.ISCI).Distinct().OrderBy(x => x).ToList(),
                DistinctSpotLengths = clientScrubs.Select(x => x.SpotLength).Distinct().OrderBy(x => x).ToList(),
                DistinctAffiliates = clientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.Affiliate)).Select(x => x.Affiliate).Distinct().OrderBy(x => x).ToList(),
                DistinctStations = clientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.Station)).Select(x => x.Station).Distinct().OrderBy(x => x).ToList(),
                DistinctWeekStarts = clientScrubs.Select(x => x.WeekStart).Distinct().OrderBy(x => x).ToList(),
                DistinctShowTypes = clientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ShowTypeName)).Select(x => x.ShowTypeName).Distinct().OrderBy(x => x).ToList(),
                DistinctSequences = clientScrubs.Where(x => x.Sequence.HasValue).Select(x => x.Sequence.Value).Distinct().OrderBy(x => x).ToList(),
                TimeAiredStart = clientScrubs.Any() ? clientScrubs.Select(x => x.TimeAired).OrderBy(x => x).First() : (int?)null,
                TimeAiredEnd = clientScrubs.Any() ? clientScrubs.Select(x => x.TimeAired).OrderBy(x => x).Last() : (int?)null
            };
        }

        private List<ProposalDetailPostScrubbingDto> _MapClientScrubDataToDto(List<ProposalDetailPostScrubbing> clientScrubsData, Dictionary<int, int> spotsLengths)
        {
            return clientScrubsData.Select(x => {
                var spotLength = spotsLengths.Single(y => y.Value == x.SpotLengthId).Key;
                var isIsciMarried = x.WeekIscis.SingleOrDefault(i => i.HouseIsci == x.ISCI)?.MarriedHouseIsci ?? false;

                if (isIsciMarried)
                {
                    // We assume that married ISCIs are always two. So that married spot length is half of what is received
                    spotLength /= 2;
                }

                return new ProposalDetailPostScrubbingDto()
                {
                    SpotLength = spotLength,
                    Affiliate = x.Affiliate,
                    ClientISCI = x.ClientISCI,
                    Comments = x.Comments,
                    DateAired = x.DateAired,
                    DayOfWeek = x.DayOfWeek,
                    GenreName = x.GenreName,
                    ISCI = x.ISCI,
                    Market = x.Market,
                    MatchDate = x.MatchDate,
                    MatchGenre = x.MatchGenre,
                    MatchIsci = x.MatchIsci,
                    MatchIsciDays = x.MatchIsciDays,
                    MatchMarket = x.MatchMarket,
                    MatchProgram = x.MatchProgram,
                    MatchShowType = x.MatchShowType,
                    MatchStation = x.MatchStation,
                    MatchTime = x.MatchTime,
                    ProgramName = x.ProgramName,
                    ProposalDetailId = x.ProposalDetailId,
                    ScrubbingClientId = x.ScrubbingClientId,
                    Sequence = x.Sequence,
                    ShowTypeName = x.ShowTypeName,
                    Station = x.Station,
                    Status = x.Status,
                    StatusOverride = x.StatusOverride,
                    TimeAired = x.TimeAired,
                    WeekStart = x.WeekStart,
                    SupliedProgramNameIsUsed = string.IsNullOrWhiteSpace(x.WWTVProgramName)
                };
            }).ToList();
        }

        private void _SetClientScrubsMarketAndAffiliate(List<ProposalDetailPostScrubbingDto> clientScrubs)
        {
            var stationNameList = clientScrubs.Select(s => _StationProcessingEngine.StripStationSuffix(s.Station)).ToList();

            var stationList = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>().GetBroadcastStationListByLegacyCallLetters(stationNameList);

            foreach (var scrub in clientScrubs)
            {
                var scrubStationName = _StationProcessingEngine.StripStationSuffix(scrub.Station);
                var station = stationList.Where(s => s.LegacyCallLetters.Equals(scrubStationName)).SingleOrDefault();
                if (station != null)
                {
                    scrub.Market = station.OriginMarket;
                    scrub.Affiliate = station.Affiliation;
                }
            }
        }

        /// <summary>
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<UnlinkedIscisDto> GetUnlinkedIscis()
        {
            List<UnlinkedIscis> iscis = _PostRepository.GetUnlinkedIscis();

            return iscis.Select(x => new UnlinkedIscisDto()
            {
                Count = x.Count,
                ISCI = x.ISCI,
                SpotLength = _SpotLengthsDict.Single(y => y.Value == x.SpotLengthId).Key,
                UnlinkedReason = EnumHelper.GetFileDetailProblemDescription(x.ProblemType)
            }).ToList();
        }

        /// <summary>
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<ArchivedIscisDto> GetArchivedIscis()
        {
            List<ArchivedIscisDto> iscis = _PostRepository.GetArchivedIscis();

            iscis.ForEach(x =>
            {
                x.SpotLength = _SpotLengthsDict.Single(y => y.Value == x.SpotLength).Key;
            });

            return iscis;
        }

        public ClientPostScrubbingProposalDto OverrideScrubbingStatus(ScrubStatusOverrideRequest scrubStatusOverrides)
        {
            _AffidavitRepository.OverrideScrubStatus(scrubStatusOverrides.ScrubIds,
                scrubStatusOverrides.OverrideStatus);

            ProposalScrubbingRequest filter =
                new ProposalScrubbingRequest() { ScrubbingStatusFilter = scrubStatusOverrides.ReturnStatusFilter };
            return GetClientScrubbingForProposal(scrubStatusOverrides.ProposalId, filter);
        }

        /// <summary>
        /// Undo the overriding of an affidavit client scrub status
        /// </summary>
        /// <param name="request">ScrubStatusOverrideRequest object containing the ids of the records to undo</param>
        /// <returns>ClientPostScrubbingProposalDto object</returns>
        public ClientPostScrubbingProposalDto UndoOverrideScrubbingStatus(ScrubStatusOverrideRequest request)
        {
            var affidavitClientScrubs = _AffidavitRepository.GetAffidavitClientScrubsByIds(request.ScrubIds);
            affidavitClientScrubs.ForEach(x =>
            {
                x.status = (x.match_station && x.match_market && x.match_genre && x.match_program && x.match_time && x.match_isci_days && x.match_date && x.match_show_type)
                        ? (int)ScrubbingStatus.InSpec
                        : (int)ScrubbingStatus.OutOfSpec;
                x.status_override = false;
            });
            _AffidavitRepository.SaveScrubsStatus(affidavitClientScrubs);

            return GetClientScrubbingForProposal(request.ProposalId, new ProposalScrubbingRequest() { ScrubbingStatusFilter = request.ReturnStatusFilter });
        }

        /// <summary>
        /// Archives an isci from the unlinked isci list
        /// </summary>
        /// <param name="iscis">Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        /// <returns>True or false based on the errors</returns>
        public bool ArchiveUnlinkedIsci(List<string> iscis, string username)
        {
            List<string> iscisToArchive = iscis.Distinct().ToList();

            List<ScrubbingFileDetail> fileDetailList = _PostRepository.LoadFileDetailsByIscis(iscisToArchive);
            List<long> fileDetailIdsToProcess = fileDetailList.Select(x => x.Id).ToList();

            if (!_PostRepository.IsIsciBlacklisted(iscisToArchive))
            {
                using (var transaction = new TransactionScopeWrapper()) //Ensure all db requests succeed or fail
                {
                    _IsciService.BlacklistIscis(iscisToArchive, username);
                    _PostRepository.AddNotACadentIsciProblem(fileDetailIdsToProcess);
                    _PostRepository.SetArchivedFlag(fileDetailIdsToProcess, true);
                    transaction.Complete();
                }
            }
            else
            {
                throw new Exception("There are already blacklisted iscis in your list");
            }

            return true;
        }

        /// <summary>
        /// Undo the archive process of a list of iscis
        /// </summary>
        /// <param name="fileDetailIds">List of affidavit file detail ids</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">User requesting the undo operation</param>
        /// <returns>True or false</returns>
        public bool UndoArchiveUnlinkedIsci(List<long> fileDetailIds, DateTime currentDateTime, string username)
        {
            List<ScrubbingFileDetail> fileDetailList = _PostRepository.LoadFileDetailsByIds(fileDetailIds);
            List<string> iscisToUndo = fileDetailList.Select(x => x.Isci).Distinct().ToList();
            List<long> fileDetailIdsToProcess = fileDetailList.Select(x => x.Id).ToList();

            using (var transaction = new TransactionScopeWrapper()) //Ensure all db requests succeed or fail
            {
                _IsciService.RemoveIscisFromBlacklistTable(iscisToUndo);
                _PostRepository.RemoveNotACadentIsciProblems(fileDetailIdsToProcess);
                _PostRepository.SetArchivedFlag(fileDetailIdsToProcess, false);
                transaction.Complete();
            }
            return true;
        }

        #region JSONify

        private static readonly List<string> FileHeaders = new List<string>()
        {
            "ISCI"
            ,"Spot Length"
            ,"Station"
            ,"Spot Time"
            ,"ProgramName"
            ,"Genre"
            ,"LeadInTitle"
            ,"LeadInGenre"
            ,"LeadOutTitle"
            ,"LeadOutGenre"
            ,"Inventory Source"
            ,"Affiliate"
            ,"ShowType"
            ,"LeadInShowType"
            ,"LeadOutShowType"
        };

        public string JSONifyFile(Stream rawStream, string fileName, out InboundFileSaveRequest request)
        {
            TextFileLineReader reader;
            if (fileName.EndsWith("csv"))
            {
                reader = new CsvFileReader(FileHeaders);
            }
            else
            {
                throw new Exception("Unknown file");
            }

            request = new InboundFileSaveRequest();
            request.FileName = fileName;
            request.FileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(rawStream));
            request.Source = (int)AffidavitFileSourceEnum.Strata;

            using (reader.Initialize(rawStream))
            {
                while (!reader.IsEOF())
                {
                    reader.NextRow();

                    if (reader.IsEmptyRow())
                        break;

                    var detail = new InboundFileSaveRequestDetail();

                    detail.AirTime = DateTime.Parse(reader.GetCellValue("Spot Time"));
                    detail.Genre = reader.GetCellValue("Genre");
                    detail.Isci = reader.GetCellValue("ISCI");
                    detail.ProgramName = reader.GetCellValue("ProgramName");
                    detail.Station = reader.GetCellValue("Station");
                    detail.SpotLength = int.Parse(reader.GetCellValue("Spot Length"));
                    detail.LeadInProgramName = reader.GetCellValue("LeadInTitle");
                    detail.LeadInGenre = reader.GetCellValue("LeadInGenre");
                    detail.LeadOutProgramName = reader.GetCellValue("LeadOutTitle");
                    detail.LeadOutGenre = reader.GetCellValue("LeadOutGenre");
                    detail.InventorySource = (AffidavitFileSourceEnum)int.Parse(reader.GetCellValue("Inventory Source"));
                    detail.Affiliate = reader.GetCellValue("Affiliate");
                    detail.ShowType = reader.GetCellValue("ShowType");
                    detail.LeadInShowType = reader.GetCellValue("LeadInShowType");
                    detail.LeadOutShowType = reader.GetCellValue("LeadOutShowType");

                    request.Details.Add(detail);
                }
            }
            var json = JsonConvert.SerializeObject(request);
            return json;
        }
        #endregion
    }
}