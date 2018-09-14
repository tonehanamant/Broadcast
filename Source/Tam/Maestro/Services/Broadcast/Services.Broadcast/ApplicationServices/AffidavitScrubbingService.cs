using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.DTO;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitScrubbingService : IApplicationService
    {
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
        /// Finds all the valid iscis based on the filter
        /// </summary>
        /// <param name="isci">Isci filter</param>
        /// <returns>List of valid iscis</returns>
        List<string> FindValidIscis(string isci);

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

    public class AffidavitScrubbingService : IAffidavitScrubbingService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IPostRepository _PostRepository;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly ISMSClient _SmsClient;
        private readonly IProposalService _ProposalService;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly ISpotLengthRepository _SpotLegthRepository;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;

        public AffidavitScrubbingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISMSClient smsClient,
            IProposalService proposalService,
            IBroadcastAudiencesCache audiencesCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            IImpressionAdjustmentEngine impressionAdjustmentEngine)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _PostRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
            _SpotLegthRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _AudiencesCache = audiencesCache;
            _SmsClient = smsClient;
            _ProposalService = proposalService;
            _StationProcessingEngine = stationProcessingEngine;
            _BroadcastAudienceRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
        }

        /// <summary>
        /// Returns a list of the posts and unlinked iscis count in the system
        /// </summary>
        /// <returns>List of PostDto objects</returns>
        public PostedContractedProposalsDto GetPosts()
        {
            var postedProposals = _PostRepository.GetAllPostedProposals();
            var spotLengthsMap = _SpotLegthRepository.GetSpotLengthAndIds();

            foreach (var post in postedProposals)
            {
                _SetPostAdvertiser(post);

                var houseHoldAudienceId = _AudiencesCache.GetDefaultAudience().Id;
                post.PrimaryAudienceDeliveredImpressions = _GetImpressionsForAudience(post.ContractId, post.PostType, post.GuaranteedAudienceId, spotLengthsMap, post.Equivalized);
                post.HouseholdDeliveredImpressions = _GetImpressionsForAudience(post.ContractId, post.PostType, houseHoldAudienceId, spotLengthsMap, post.Equivalized);
                post.PrimaryAudienceDelivery = post.PrimaryAudienceDeliveredImpressions / post.PrimaryAudienceBookedImpressions * 100;
            }

            return new PostedContractedProposalsDto()
            {
                Posts = postedProposals,
                UnlinkedIscis = _PostRepository.CountUnlinkedIscis()
            };
        }

        private double _GetImpressionsForAudience(int contractId, SchedulePostType type, int audienceId, Dictionary<int, int> spotLengthsMap, bool equivalized)
        {
            var impressionsDataGuaranteed = _GetPostImpressionsData(contractId, audienceId);
            double deliveredImpressions = 0;
            foreach (var impressionData in impressionsDataGuaranteed)
            {
                double impressions = impressionData.Impressions;
                if (equivalized)
                {
                    impressions = _ImpressionAdjustmentEngine.AdjustImpression(impressions, true, spotLengthsMap.Single(x => x.Value == impressionData.SpotLengthId).Key);
                }
                if (type == SchedulePostType.NTI)
                {
                    impressions = _CalculateNtiImpressions(impressionData.Impressions, impressionData.NtiConversionFactor);
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

        private double _CalculateNtiImpressions(double impressions, double ntiConversionFactor)
        {
            return impressions * (1 - ntiConversionFactor);
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
                    clientScrubs = _MapClientScrubDataToDto(
                        _AffidavitRepository.GetProposalDetailPostScrubbing(proposalId, proposalScrubbingRequest.ScrubbingStatusFilter),
                        _SpotLegthRepository.GetSpotLengthAndIds());
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
            return clientScrubsData.Select(x => new ProposalDetailPostScrubbingDto()
            {
                SpotLength = spotsLengths.Single(y => y.Value == x.SpotLengthId).Key,
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
                WeekStart = x.WeekStart
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
            var spotsLength = _SpotLegthRepository.GetSpotLengthAndIds();
            List<UnlinkedIscis> iscis = _PostRepository.GetUnlinkedIscis();

            return iscis.Select(x => new UnlinkedIscisDto()
            {
                Count = x.Count,
                ISCI = x.ISCI,
                SpotLength = spotsLength.Single(y => y.Value == x.SpotLengthId).Key,
                UnlinkedReason = _GetAffidavitDetailProblemDescription(x.ProblemType)
            }).ToList();
        }

        /// <summary>
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<ArchivedIscisDto> GetArchivedIscis()
        {
            var spotsLength = _SpotLegthRepository.GetSpotLengthAndIds();
            List<ArchivedIscisDto> iscis = _PostRepository.GetArchivedIscis();

            iscis.ForEach(x =>
            {
                x.SpotLength = spotsLength.Single(y => y.Value == x.SpotLength).Key;
            });

            return iscis;
        }

        private string _GetAffidavitDetailProblemDescription(FileDetailProblemTypeEnum problemType)
        {
            switch (problemType)
            {
                case FileDetailProblemTypeEnum.UnlinkedIsci:
                    return "Not in system";
                case FileDetailProblemTypeEnum.UnmarriedOnMultipleContracts:
                    return "Multiple Proposals";
                case FileDetailProblemTypeEnum.MarriedAndUnmarried:
                    return "Married and Unmarried";
                case FileDetailProblemTypeEnum.UnmatchedSpotLength:
                    return "Unmatched Spot length";
                default:
                    return null;
            }
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

            List<AffidavitFileDetail> fileDetailList = _PostRepository.LoadFileDetailsByIscis(iscisToArchive);
            List<long> fileDetailIdsToProcess = fileDetailList.Select(x => x.Id).ToList();

            if (!_PostRepository.IsIsciBlacklisted(iscisToArchive))
            {
                using (var transaction = new TransactionScopeWrapper()) //Ensure all db requests succeed or fail
                {
                    _PostRepository.ArchiveIsci(iscisToArchive, username);
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
            List<AffidavitFileDetail> fileDetailList = _PostRepository.LoadFileDetailsByIds(fileDetailIds);
            List<string> iscisToUndo = fileDetailList.Select(x => x.Isci).Distinct().ToList();
            List<long> fileDetailIdsToProcess = fileDetailList.Select(x => x.Id).ToList();

            using (var transaction = new TransactionScopeWrapper()) //Ensure all db requests succeed or fail
            {
                _PostRepository.RemoveIscisFromBlacklistTable(iscisToUndo);
                _PostRepository.RemoveNotACadentIsciProblems(fileDetailIdsToProcess);
                _PostRepository.SetArchivedFlag(fileDetailIdsToProcess, false);
                transaction.Complete();
            }
            return true;
        }

        /// <summary>
        /// Finds all the valid iscis based on the filter
        /// </summary>
        /// <param name="isciFilter">Isci filter</param>
        /// <returns>List of valid iscis</returns>
        public List<string> FindValidIscis(string isciFilter)
        {
            var iscis = _PostRepository.FindValidIscis(isciFilter);
            var groupedIscis = iscis.GroupBy(x => new { x.HouseIsci, x.ProposalId });
            var distinctIscis = iscis.Select(x => x.HouseIsci).Distinct().ToList();
            foreach (var isci in distinctIscis)
            {
                if (groupedIscis.Where(x => x.Key.HouseIsci.Equals(isci)).Count() > 1
                    && iscis.Any(x => x.HouseIsci.Equals(isci) && x.Married == false))
                {
                    iscis.RemoveAll(x => x.HouseIsci.Equals(isci));
                }
            }

            return iscis.Select(x => x.HouseIsci).ToList();
        }
    }
}
