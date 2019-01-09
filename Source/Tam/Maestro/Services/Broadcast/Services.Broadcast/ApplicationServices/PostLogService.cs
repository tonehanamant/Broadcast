using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostLogService : IApplicationService
    {
        /// <summary>
        /// Saves a keeping trac file
        /// </summary>
        /// <param name="saveRequest">InboundFileSaveRequest object</param>
        /// <param name="username">Username requesting the save</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <returns>WWTVSaveResult object</returns>
        WWTVSaveResult SaveKeepingTracFile(InboundFileSaveRequest saveRequest, string username, DateTime currentDateTime);

        /// <summary>
        /// Saves keeping trac validation errors
        /// </summary>
        /// <param name="saveRequest">InboundFileSaveRequest object</param>
        /// <param name="userName">User requesting the save</param>
        /// <param name="validationResults">Validation results to save</param>
        /// <returns>WWTVSaveResult object</returns>
        WWTVSaveResult SaveKeepingTracValidationErrors(InboundFileSaveRequest saveRequest, string userName, List<WWTVInboundFileValidationResult> validationResults);

        /// <summary>
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<UnlinkedIscisDto> GetUnlinkedIscis();

        /// <summary>
        /// Returns a list of the posts and the number of unlinked iscis
        /// </summary>
        /// <returns>List of PostDto objects and the number of unlinked iscis</returns>
        PostedContractedProposalsDto GetPostLogs();

        /// <summary>
        /// Returns a list of archived iscis
        /// </summary>
        /// <returns>List of ArchivedIscisDto objects</returns>
        List<ArchivedIscisDto> GetArchivedIscis();

        /// <summary>
        /// Scrubs a postlog detail by an isci
        /// </summary>
        /// <param name="isci">Isci to scrub</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">User requesting the scrubbing</param>
        /// <returns>True or false</returns>
        bool ScrubUnlinkedPostLogDetailsByIsci(string isci, DateTime currentDateTime, string username);

        /// <summary>
        /// Undo the archive process of a list of iscis
        /// </summary>
        /// <param name="fileDetailIds">List of post log file detail ids</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">User requesting the undo operation</param>
        /// <returns>True or false</returns>
        bool UndoArchiveUnlinkedIsci(List<long> fileDetailIds, DateTime currentDateTime, string username);

        /// <summary>
        /// Archives an isci from the unlinked isci list
        /// </summary>
        /// <param name="iscis">Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        /// <returns>True or false based on the errors</returns>
        bool ArchiveUnlinkedIsci(List<string> iscis, string username);

        /// <summary>
        /// Maps an original isci to an effective isci
        /// </summary>
        /// <param name="mapIsciDto">MapIsciDto object containing the iscis to map</param>
        /// <param name="name">User requesting the mapping</param>
        /// <returns>The result of the mapping in true or false</returns>
        bool MapIsci(MapIsciDto mapIsciDto, DateTime currentDateTime, string name);

        /// <summary>
        /// Gets a client post scrubbing proposal with details
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns>ClientPostScrubbingProposalDto object containing the post scrubbing information</returns>
        ClientPostScrubbingProposalDto GetClientScrubbingForProposal(int proposalId, ProposalScrubbingRequest proposalScrubbingRequest, List<ProposalDetailPostScrubbingDto> scrubs = null);

        /// <summary>
        /// Override post log client scrubs statuses
        /// </summary>
        /// <param name="request">ScrubStatusOverrideRequest object containing the ids of the records to undo</param>
        /// <returns>ClientPostScrubbingProposalDto object</returns>
        ClientPostScrubbingProposalDto OverrideScrubbingStatus(ScrubStatusOverrideRequest scrubStatusOverrides);

        /// <summary>
        /// Undo the overriding of an post log client scrub status
        /// </summary>
        /// <param name="request">ScrubStatusOverrideRequest object containing the ids of the records to undo</param>
        /// <returns>ClientPostScrubbingProposalDto object</returns>
        ClientPostScrubbingProposalDto UndoOverrideScrubbingStatus(ScrubStatusOverrideRequest request);

        /// <summary>
        /// Swaps one or more client scrubs to another proposal detail
        /// </summary>
        /// <param name="requestData">SwapProposalDetailRequest object containing a list of postlog client scrubs to be swapped and the proposal detail id to swap to</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">Username requesting the change</param>
        /// <returns>True or false</returns>
        bool SwapProposalDetails(SwapProposalDetailRequest requestData, DateTime currentDateTime, string username);
    }

    public class PostLogService : IPostLogService
    {
        private readonly IPostLogRepository _PostLogRepository;
        private readonly IPostLogEngine _PostLogEngine;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly ISMSClient _SmsClient;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IImpressionsService _ImpressionsService;
        private readonly IPostRepository _PostRepository;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IStationRepository _StationRepository;
        private readonly IProposalService _ProposalService;
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly INsiPostingBookService _NsiPostingBookService;
        private readonly IProposalRepository _ProposalRepository;
        private readonly IProgramScrubbingEngine _ProgramScrubbingEngine;
        private readonly IMatchingEngine _MatchingEngine;
        private readonly IIsciService _IsciService;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        
        private const string ARCHIVED_ISCI = "Not a Cadent Isci";
        private const ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3;

        public PostLogService(IDataRepositoryFactory dataRepositoryFactory
            , IPostLogEngine postLogEngine
            , IBroadcastAudiencesCache audiencesCache
            , IImpressionAdjustmentEngine impressionAdjustmentEngine
            , ISMSClient smsClient
            , IImpressionsService impressionsService
            , IStationProcessingEngine stationProcessingEngine
            , IProposalService proposalService
            , IProposalMarketsCalculationEngine proposalMarketsCalculationEngine
            , INsiPostingBookService nsiPostingBookService
            , IProgramScrubbingEngine programScrubbingEngine
            , IMatchingEngine matchingEngine
            , IIsciService isciService
            , ISpotLengthEngine spotLengthEngine)
        {
            _PostLogRepository = dataRepositoryFactory.GetDataRepository<IPostLogRepository>();
            _PostLogEngine = postLogEngine;
            _PostRepository = dataRepositoryFactory.GetDataRepository<IPostRepository>();
            _StationRepository = dataRepositoryFactory.GetDataRepository<IStationRepository>();
            _ProposalRepository = dataRepositoryFactory.GetDataRepository<IProposalRepository>();
            _AudiencesCache = audiencesCache;
            _SmsClient = smsClient;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
            _BroadcastAudienceRepository = dataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _ImpressionsService = impressionsService;
            _StationProcessingEngine = stationProcessingEngine;
            _ProposalService = proposalService;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _NsiPostingBookService = nsiPostingBookService;
            _ProgramScrubbingEngine = programScrubbingEngine;
            _MatchingEngine = matchingEngine;
            _IsciService = isciService;
            _SpotLengthEngine = spotLengthEngine;
        }

        /// <summary>
        /// Saves a keeping trac file
        /// </summary>
        /// <param name="saveRequest">InboundFileSaveRequest object</param>
        /// <param name="username">Username requesting the save</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <returns>WWTVSaveResult object</returns>
        public WWTVSaveResult SaveKeepingTracFile(InboundFileSaveRequest saveRequest, string username, DateTime currentDateTime)
        {
            var postLogFile = _MapInboundFileSaveRequestToPostLogFile(saveRequest, currentDateTime);
            var result = _PostLogSaveResult(saveRequest, username, currentDateTime, postLogFile);

            return result;
        }

        /// <summary>
        /// Saves keeping trac validation errors
        /// </summary>
        /// <param name="saveRequest">InboundFileSaveRequest object</param>
        /// <param name="userName">User requesting the save</param>
        /// <param name="validationResults">Validation results to save</param>
        /// <returns>WWTVSaveResult object</returns>
        public WWTVSaveResult SaveKeepingTracValidationErrors(InboundFileSaveRequest saveRequest, string userName, List<WWTVInboundFileValidationResult> validationResults)
        {
            var postLogFile = _MapInboundFileSaveRequestToPostLogFile(saveRequest, DateTime.Now);

            var problems = _MapValidationErrorToWWTVFileProblem((validationResults));
            postLogFile.FileProblems.AddRange(problems);
            var result = new WWTVSaveResult
            {
                ValidationResults = validationResults
            };
            postLogFile.Status = validationResults.Any() ? FileProcessingStatusEnum.Invalid : FileProcessingStatusEnum.Valid;

            result.Id = _PostLogRepository.SavePostLogFile(postLogFile);
            return result;
        }

        /// <summary>
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<UnlinkedIscisDto> GetUnlinkedIscis()
        {
            List<UnlinkedIscis> iscis = _PostLogRepository.GetUnlinkedIscis();

            return iscis.Select(x => new UnlinkedIscisDto()
            {
                Count = x.Count,
                ISCI = x.ISCI,
                SpotLength = _SpotLengthEngine.GetSpotLengthValueById(x.SpotLengthId),
                UnlinkedReason = EnumHelper.GetFileDetailProblemDescription(x.ProblemType)
            }).ToList();
        }

        /// <summary>
        /// Returns a list of the posts and unlinked iscis count in the system
        /// </summary>
        /// <returns>List of PostDto objects</returns>
        public PostedContractedProposalsDto GetPostLogs()
        {
            var postedProposals = _PostLogRepository.GetAllPostedProposals();

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
                UnlinkedIscis = _PostLogRepository.CountUnlinkedIscis()
            };
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
            return ScrubUnlinkedPostLogDetailsByIsci(mapIsciDto.OriginalIsci, currentDateTime, username);
        }

        /// <summary>
        /// Scrubs a postlog detail by an isci
        /// </summary>
        /// <param name="isci">Isci to scrub</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">User requesting the scrubbing</param>
        /// <returns>True or false</returns>
        public bool ScrubUnlinkedPostLogDetailsByIsci(string isci, DateTime currentDateTime, string username)
        {
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);
            var unlinkedPostLogDetails = _PostLogRepository.GetUnlinkedPostLogDetailsByIsci(isci);
            _IsciService.LoadIsciMappings(unlinkedPostLogDetails);
            var matchedPostLogDetails = _LinkAndValidateContractIscis(unlinkedPostLogDetails);
            _SetPostingBookData(matchedPostLogDetails, postingBookId);

            //load ClientScrubs and DetailProblems
            _MapToPostLogFileDetails(matchedPostLogDetails, currentDateTime, username);

            var fileDetails = matchedPostLogDetails.Select(d => d.FileDetail).ToList();
            _ScrubMatchedPostLogRecords(fileDetails);
            _ImpressionsService.CalculateImpressionsForFileDetails(fileDetails);
            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                _PostLogRepository.SaveScrubbedFileDetails(fileDetails);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(fileDetails));
                transaction.Complete();
            }
            return true;
        }

        /// <summary>
        /// Undo the archive process of a list of iscis
        /// </summary>
        /// <param name="fileDetailIds">List of post log file detail ids</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">User requesting the undo operation</param>
        /// <returns>True or false</returns>
        public bool UndoArchiveUnlinkedIsci(List<long> fileDetailIds, DateTime currentDateTime, string username)
        {
            List<ScrubbingFileDetail> fileDetailList = _PostLogRepository.LoadFileDetailsByIds(fileDetailIds);
            List<string> iscisToUndo = fileDetailList.Select(x => x.Isci).Distinct().ToList();
            List<long> fileDetailIdsToProcess = fileDetailList.Select(x => x.Id).ToList();

            using (var transaction = new TransactionScopeWrapper()) //Ensure all db requests succeed or fail
            {
                _IsciService.RemoveIscisFromBlacklistTable(iscisToUndo);
                _PostLogRepository.RemoveNotACadentIsciProblems(fileDetailIdsToProcess);
                _PostLogRepository.SetArchivedFlag(fileDetailIdsToProcess, false);
                transaction.Complete();
            }
            return true;
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

            List<ScrubbingFileDetail> fileDetailList = _PostLogRepository.LoadFileDetailsByIscis(iscisToArchive);
            List<long> fileDetailIdsToProcess = fileDetailList.Select(x => x.Id).ToList();

            if (!_PostRepository.IsIsciBlacklisted(iscisToArchive))
            {
                using (var transaction = new TransactionScopeWrapper()) //Ensure all db requests succeed or fail
                {
                    _IsciService.BlacklistIscis(iscisToArchive, username);
                    _PostLogRepository.AddNotACadentIsciProblem(fileDetailIdsToProcess);
                    _PostLogRepository.SetArchivedFlag(fileDetailIdsToProcess, true);
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
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<ArchivedIscisDto> GetArchivedIscis()
        {
            List<ArchivedIscisDto> iscis = _PostLogRepository.GetArchivedIscis();

            iscis.ForEach(x =>
            {
                x.SpotLength = _SpotLengthEngine.GetSpotLengthValueById(x.SpotLength);
            });

            return iscis;
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
                    clientScrubs = _MapClientScrubDataToDto(_PostLogRepository.GetProposalDetailPostScrubbing(proposalId, proposalScrubbingRequest.ScrubbingStatusFilter));
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

        /// <summary>
        /// Override post log client scrubs statuses
        /// </summary>
        /// <param name="request">ScrubStatusOverrideRequest object containing the ids of the records to undo</param>
        /// <returns>ClientPostScrubbingProposalDto object</returns>
        public ClientPostScrubbingProposalDto OverrideScrubbingStatus(ScrubStatusOverrideRequest scrubStatusOverrides)
        {
            _PostLogRepository.OverrideScrubStatus(scrubStatusOverrides.ScrubIds, scrubStatusOverrides.OverrideStatus);

            return GetClientScrubbingForProposal(scrubStatusOverrides.ProposalId, new ProposalScrubbingRequest() { ScrubbingStatusFilter = scrubStatusOverrides.ReturnStatusFilter });
        }

        /// <summary>
        /// Undo the overriding of an post log client scrub status
        /// </summary>
        /// <param name="request">ScrubStatusOverrideRequest object containing the ids of the records to undo</param>
        /// <returns>ClientPostScrubbingProposalDto object</returns>
        public ClientPostScrubbingProposalDto UndoOverrideScrubbingStatus(ScrubStatusOverrideRequest request)
        {
            var clientScrubs = _PostLogRepository.GetPostLogClientScrubsByIds(request.ScrubIds);
            clientScrubs.ForEach(x =>
            {
                x.status = (x.match_station && x.match_market && x.match_genre && x.match_program && x.match_time && x.match_isci_days && x.match_date.Value && x.match_show_type)
                        ? (int)ScrubbingStatus.InSpec
                        : (int)ScrubbingStatus.OutOfSpec;
                x.status_override = false;
            });
            _PostLogRepository.SaveScrubsStatus(clientScrubs);

            return GetClientScrubbingForProposal(request.ProposalId, new ProposalScrubbingRequest() { ScrubbingStatusFilter = request.ReturnStatusFilter });
        }

        /// <summary>
        /// Swaps one or more client scrubs to another proposal detail
        /// </summary>
        /// <param name="requestData">SwapProposalDetailRequest object containing a list of postlog client scrubs to be swapped and the proposal detail id to swap to</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">Username requesting the change</param>
        /// <returns>True or false</returns>
        public bool SwapProposalDetails(SwapProposalDetailRequest requestData, DateTime currentDateTime, string username)
        {
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);
            List<ScrubbingFileDetail> affidavitDetails = _PostLogRepository.GetPostLogDetailsByClientScrubsByIds(requestData.ScrubbingIds);
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(affidavitDetails, requestData.ProposalDetailId);
            _SetPostingBookData(matchedAffidavitDetails, postingBookId);

            //load ClientScrubs and DetailProblems
            _MapToPostLogFileDetails(matchedAffidavitDetails, currentDateTime, username);

            var affidavitFileDetails = matchedAffidavitDetails.Select(d => d.FileDetail).ToList();
            _ScrubMatchedPostLogRecords(affidavitFileDetails);
            _ImpressionsService.CalculateImpressionsForFileDetails(affidavitFileDetails);
            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                _PostLogRepository.SaveScrubbedFileDetails(affidavitFileDetails);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(affidavitFileDetails));
                transaction.Complete();
            }
            return true;
        }

        #region Private Methods
        private List<FileProblem> _MapValidationErrorToWWTVFileProblem(List<WWTVInboundFileValidationResult> validationResults)
        {
            List<FileProblem> problems = new List<FileProblem>();

            validationResults.ForEach(v =>
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

        private List<ProposalDetailPostScrubbingDto> _MapClientScrubDataToDto(List<ProposalDetailPostScrubbing> clientScrubsData)
        {
            return clientScrubsData.Select(x =>
            {
                var spotLength = _SpotLengthEngine.GetSpotLengthValueById(x.SpotLengthId);
                var isIsciMarried = x.WeekIscis.Where(i => i.HouseIsci == x.ISCI).Any(i => i.MarriedHouseIsci);

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
                    SuppliedProgramNameUsed = string.IsNullOrWhiteSpace(x.WWTVProgramName)
                };
            }).ToList();
        }

        private void _SetClientScrubsMarketAndAffiliate(List<ProposalDetailPostScrubbingDto> clientScrubs)
        {
            var stationNameList = clientScrubs.Select(s => _StationProcessingEngine.StripStationSuffix(s.Station)).ToList();

            var stationList = _StationRepository.GetBroadcastStationListByLegacyCallLetters(stationNameList);

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

        private WWTVSaveResult _PostLogSaveResult(InboundFileSaveRequest saveRequest, string username, DateTime currentDateTime, ScrubbingFile postLogFile)
        {
            var result = new WWTVSaveResult();

            var lineNumber = 0;
            foreach (var saveRequestDetail in saveRequest.Details)
            {
                lineNumber++;
                var validationErrors = _PostLogEngine.ValidatePostLogRecord(saveRequestDetail);

                if (validationErrors.Any())
                {
                    validationErrors.ForEach(r => r.InvalidLine = lineNumber);
                    var problems = _MapValidationErrorToWWTVFileProblem(validationErrors);
                    postLogFile.FileProblems.AddRange(problems);

                    result.ValidationResults.AddRange(validationErrors);
                }
            }

            postLogFile.Status = result.ValidationResults.Any() ? FileProcessingStatusEnum.Invalid : FileProcessingStatusEnum.Valid;

            if (result.ValidationResults.Any())
            {   // save and get out
                result.Id = _PostLogRepository.SavePostLogFile(postLogFile);
                return result;
            }

            var ScrubbingFileDetailsToBeLinked = _MapToScrubbingFileDetails(saveRequest.Details);
            foreach (var detail in ScrubbingFileDetailsToBeLinked)
            {
                if (_PostLogRepository.IsIsciBlacklisted(new List<string> { detail.Isci }))
                {
                    detail.FileDetailProblems.Add(new FileDetailProblem
                    {
                        Description = ARCHIVED_ISCI,
                        Type = FileDetailProblemTypeEnum.ArchivedIsci
                    });
                    detail.Archived = true;
                    postLogFile.FileDetails.Add(detail);
                }
            }
            ScrubbingFileDetailsToBeLinked.RemoveAll(d => postLogFile.FileDetails.Contains(d));

            _IsciService.LoadIsciMappings(ScrubbingFileDetailsToBeLinked);
            var matchedPostLogDetails = _LinkAndValidateContractIscis(ScrubbingFileDetailsToBeLinked);
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);
            _SetPostingBookData(matchedPostLogDetails, postingBookId);

            //load ClientScrubs and DetailProblems
            _MapToPostLogFileDetails(matchedPostLogDetails, currentDateTime, username);

            postLogFile.FileDetails.AddRange(matchedPostLogDetails.Select(x => x.FileDetail).ToList());

            _ScrubMatchedPostLogRecords(postLogFile.FileDetails);
            _ImpressionsService.CalculateImpressionsForFileDetails(postLogFile.FileDetails);

            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                result.Id = _PostLogRepository.SavePostLogFile(postLogFile);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(postLogFile.FileDetails));
                transaction.Complete();
            }

            return result;
        }

        private void _MapToPostLogFileDetails(List<MatchingDetail> matchedPostLogDetails, DateTime currentDateTime, string username)
        {
            foreach (var detail in matchedPostLogDetails)
            {
                detail.FileDetail.ClientScrubs = detail.ProposalDetailWeeks.Select(
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
                                EffectiveIsci = detail.EffectiveIsci,
                                EffectiveClientIsci = w.ClientIsci
                            }).ToList();
                detail.FileDetail.FileDetailProblems = detail.FileDetailProblems;
            }
        }

        private void _ScrubMatchedPostLogRecords(List<ScrubbingFileDetail> fileDetails)
        {
            var stations = _StationRepository.GetBroadcastStations().ToDictionary(k => k.LegacyCallLetters, v => v);

            foreach (var detail in fileDetails)
            {
                if (!detail.ClientScrubs.Any())
                    continue;

                var quarterWeekIds = detail.ClientScrubs.Select(s => s.ProposalVersionDetailQuarterWeekId).ToList();
                var proposals = _ProposalService.GetProposalsByQuarterWeeks(quarterWeekIds);

                var postLogStation = _MatchPostLogStation(stations, detail);

                foreach (ClientScrub scrub in detail.ClientScrubs)
                {
                    var quarterWeekId = scrub.ProposalVersionDetailQuarterWeekId;
                    var proposal = proposals[quarterWeekId];
                    var proposalDetail = proposal.Details.Single(d => d.Quarters.Any(q => q.Weeks.Any(w => w.Id == quarterWeekId)));
                    var proposalWeek = proposalDetail.Quarters.SelectMany(d => d.Weeks.Where(w => w.Id == quarterWeekId)).First();
                    var proposalWeekIsci = proposalWeek.Iscis.Any(i => i.HouseIsci.Equals(detail.Isci, StringComparison.InvariantCultureIgnoreCase))
                        ? proposalWeek.Iscis.First(i => i.HouseIsci.Equals(detail.Isci, StringComparison.InvariantCultureIgnoreCase))
                        : !string.IsNullOrWhiteSpace(detail.MappedIsci) ? proposalWeek.Iscis.First(i => i.HouseIsci.Equals(detail.MappedIsci, StringComparison.InvariantCultureIgnoreCase))
                                : null;

                    if (proposalWeekIsci == null)
                    {
                        scrub.MatchIsci = false;
                    }
                    else
                    {
                        scrub.MatchIsciDays = _IsIsciDaysMatch(proposalWeekIsci, detail.OriginalAirDate.DayOfWeek);
                        scrub.MatchIsci = true;
                        scrub.EffectiveClientIsci = proposalWeekIsci.ClientIsci;
                    }

                    // match market/station
                    scrub.MatchStation = false;
                    scrub.MatchMarket = false;

                    if (postLogStation != null)
                    {
                        var markets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposal);

                        var marketGeoName = postLogStation.OriginMarket;
                        if (markets.Any(m => m.Display == marketGeoName))
                        {
                            detail.Market = marketGeoName;
                            scrub.MatchStation = true;
                            scrub.MatchMarket = true;
                        }
                    }

                    _ProgramScrubbingEngine.Scrub(proposalDetail, detail, scrub);

                    scrub.Status = (scrub.MatchStation && scrub.MatchMarket && scrub.MatchGenre && scrub.MatchProgram && scrub.MatchTime
                        && scrub.MatchIsciDays && scrub.MatchDate && scrub.MatchShowType && scrub.MatchIsci)
                        ? ScrubbingStatus.InSpec
                        : ScrubbingStatus.OutOfSpec;
                }
            }
        }

        private DisplayBroadcastStation _MatchPostLogStation(Dictionary<string, DisplayBroadcastStation> stations, ScrubbingFileDetail detail)
        {
            string stationName = _StationProcessingEngine.StripStationSuffix(detail.Station);

            if (stations.ContainsKey(stationName))
            {
                return stations[stationName];
            }

            return null;
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

        private List<ProposalDetailPostingData> _GetProposalDetailIdsWithPostingBookId(List<ScrubbingFileDetail> fileDetails)
        {
            var result = fileDetails.SelectMany(d => d.ClientScrubs.Select(s => new ProposalDetailPostingData
            {
                ProposalVersionDetailId = s.ProposalVersionDetailId,
                PostingBookId = s.PostingBookId.Value,
                PostingPlaybackType = s.PostingPlaybackType.Value
            })).ToList();

            return result;
        }

        private void _SetPostingBookData(List<MatchingDetail> details, int postingBookId, ProposalEnums.ProposalPlaybackType? playbackType = null)
        {
            foreach (var proposalDetailWeek in details.SelectMany(d => d.ProposalDetailWeeks))
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

        private List<MatchingDetail> _LinkAndValidateContractIscis(List<ScrubbingFileDetail> postLogFileDetails, int swapProposalDetailId = 0)
        {
            bool isIsciMapped = false;
            var matchedPostLogDetails = new List<MatchingDetail>();
            List<MatchingProposalWeek> proposalWeeks = new List<MatchingProposalWeek>();
            bool swappingProposalDetail = swapProposalDetailId > 0;
            if (swappingProposalDetail)
            {
                proposalWeeks = _ProposalRepository.GetMatchingProposalWeeksByDetailId(swapProposalDetailId);
                swappingProposalDetail = true;
            }
            foreach (var detail in postLogFileDetails)
            {
                if (!swappingProposalDetail)
                {
                    proposalWeeks = _ProposalRepository.GetMatchingProposalWeeksByHouseIsci(detail.Isci);
                }

                var spotLengthId = _GetScubbingFileDetailSpotLengthId(detail, proposalWeeks);
                var matchedProposalWeeks = _MatchingEngine.Match(detail, proposalWeeks, spotLengthId);

                if (!matchedProposalWeeks.Any() && !string.IsNullOrWhiteSpace(detail.MappedIsci))
                {
                    proposalWeeks = _ProposalRepository.GetMatchingProposalWeeksByHouseIsci(detail.MappedIsci);
                    spotLengthId = _GetScubbingFileDetailSpotLengthId(detail, proposalWeeks);
                    matchedProposalWeeks = _MatchingEngine.Match(detail, proposalWeeks, spotLengthId);
                    isIsciMapped = true;
                }

                var matchingProblems = _MatchingEngine.GetMatchingProblems();
                matchedPostLogDetails.Add(new MatchingDetail()
                {
                    FileDetail = detail,
                    ProposalDetailWeeks = matchedProposalWeeks,
                    FileDetailProblems = matchingProblems,
                    EffectiveIsci = isIsciMapped ? detail.MappedIsci : detail.Isci
                });
            }

            return matchedPostLogDetails;
        }

        private List<ScrubbingFileDetail> _MapToScrubbingFileDetails(List<InboundFileSaveRequestDetail> details)
        {
            var result = details.Select(d => new ScrubbingFileDetail()
            {
                AirTime = Convert.ToInt32(d.AirTime.TimeOfDay.TotalSeconds),
                OriginalAirDate = d.AirTime,
                Isci = d.Isci,
                ProgramName = d.ProgramName,
                Genre = d.Genre,
                SpotLengthId = _SpotLengthEngine.GetSpotLengthIdByValue(d.SpotLength),
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

        private int _GetScubbingFileDetailSpotLengthId(ScrubbingFileDetail scubbingFileDetail, List<MatchingProposalWeek> proposalWeeks)
        {
            var spotLengthId = scubbingFileDetail.SpotLengthId;

            if (proposalWeeks != null)
            {
                var iscisMarried = proposalWeeks.Where(i => i.MarriedHouseIsci).GroupBy(g => g.ProposalVersionId).Count() > 1;

                // take a half of spot length for married iscis
                if (iscisMarried)
                {
                    var spotLength = _SpotLengthEngine.GetSpotLengthValueById(spotLengthId);
                    spotLength /= 2;
                    spotLengthId = _SpotLengthEngine.GetSpotLengthIdByValue(spotLength);
                }
            }

            return spotLengthId;
        }

        private static ScrubbingFile _MapInboundFileSaveRequestToPostLogFile(InboundFileSaveRequest saveRequest, DateTime currentDateTime)
        {
            if (saveRequest == null)
            {
                throw new Exception("No post log data received.");
            }

            var postLogFile = new ScrubbingFile
            {
                CreatedDate = currentDateTime,
                FileHash = saveRequest.FileHash,
                FileName = saveRequest.FileName,
                SourceId = saveRequest.Source
            };
            return postLogFile;
        }

        private void _SetPostAdvertiser(PostedContracts post)
        {
            var advertiserLookupDto = _SmsClient.FindAdvertiserById(post.AdvertiserId);
            post.Advertiser = advertiserLookupDto.Display;
        }

        private double _GetImpressionsForAudience(int contractId, SchedulePostType type, int audienceId, bool equivalized)
        {
            var impressionsDataGuaranteed = _GetPostLogImpressionsData(contractId, audienceId);
            double deliveredImpressions = 0;
            foreach (var impressionData in impressionsDataGuaranteed)
            {
                double impressions = _ImpressionAdjustmentEngine.AdjustImpression(impressionData.Impressions, equivalized, _SpotLengthEngine.GetSpotLengthValueById(impressionData.SpotLengthId));

                if (type == SchedulePostType.NTI)
                {
                    impressions = _ImpressionAdjustmentEngine.AdjustImpression(impressions, impressionData.NtiConversionFactor.Value);
                }
                deliveredImpressions += impressions;
            }
            return deliveredImpressions;
        }

        private List<PostImpressionsData> _GetPostLogImpressionsData(int contractId, int maestroAudienceId)
        {
            var ratingsAudiencesIds = _BroadcastAudienceRepository.GetRatingsAudiencesByMaestroAudience(new List<int> { maestroAudienceId })
                .Select(x => x.rating_audience_id)
                .ToList();

            return _PostLogRepository.GetPostLogImpressionsData(contractId, ratingsAudiencesIds);
        }
        #endregion
    }
}
