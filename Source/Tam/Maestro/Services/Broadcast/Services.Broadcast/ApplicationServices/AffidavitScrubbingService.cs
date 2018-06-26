using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Newtonsoft.Json;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Clients;

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
        /// <param name="archived">Switch for the archived iscis</param>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<UnlinkedIscisDto> GetUnlinkedIscis(bool archived);

        ClientPostScrubbingProposalDto OverrideScrubbingStatus(ScrubStatusOverrideRequest scrubStatusOverrides);

        /// <summary>
        /// Archives an isci from the unlinked isci list
        /// </summary>
        /// <param name="fileDetailIds">Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        /// <returns>True or false based on the errors</returns>
        bool ArchiveUnlinkedIsci(List<long> fileDetailIds, string username);

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
    }

    public class AffidavitScrubbingService : IAffidavitScrubbingService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IPostRepository _PostRepository;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly ISMSClient _SmsClient;
        private readonly IProposalService _ProposalService;
        private readonly IProjectionBooksService _ProjectionBooksService;

        public AffidavitScrubbingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISMSClient smsClient,
            IProposalService proposalService,
            IBroadcastAudiencesCache audiencesCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IProjectionBooksService postingBooksService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _PostRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
            _AudiencesCache = audiencesCache;
            _SmsClient = smsClient;
            _ProposalService = proposalService;
            _ProjectionBooksService = postingBooksService;
        }

        /// <summary>
        /// Returns a list of the posts and unlinked iscis in the system
        /// </summary>
        /// <returns>List of PostDto objects</returns>
        public PostedContractedProposalsDto GetPosts()
        {
            var postedProposals = _PostRepository.GetAllPostedProposals();

            foreach (var post in postedProposals)
            {
                _SetPrimaryAudienceImpressions(post);
            }

            return new PostedContractedProposalsDto()
            {
                Posts = postedProposals,
                UnlinkedIscis = _PostRepository.CountUnlinkedIscis()
            };
        }

        private void _SetPrimaryAudienceImpressions(PostDto post)
        {
            var broadcastAudienceRepository = _BroadcastDataRepositoryFactory
                .GetDataRepository<IBroadcastAudienceRepository>();

            var ratingsAudiencesIds = broadcastAudienceRepository.
                GetRatingsAudiencesByMaestroAudience(new List<int> { post.GuaranteedAudienceId }).
                Select(x => x.rating_audience_id).
                ToList();

            post.PrimaryAudienceImpressions = _PostRepository.GetPostImpressions(post.ContractId, ratingsAudiencesIds);
        }

        /// <summary>
        /// Gets a client post scrubbing proposal with details
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns>ClientPostScrubbingProposalDto object containing the post scrubbing information</returns>
        public ClientPostScrubbingProposalDto GetClientScrubbingForProposal(int proposalId, ProposalScrubbingRequest proposalScrubbingRequest, List<ProposalDetailPostScrubbingDto> scrubs = null)
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
                    Details = proposal.Details.Select(x => new ClientPostScrubbingProposalDetailDto
                    {
                        Id = x.Id,
                        FlightStartDate = x.FlightStartDate,
                        FlightEndDate = x.FlightEndDate,
                        FlightWeeks = x.FlightWeeks,
                        SpotLength = proposal.SpotLengths.First(y => y.Id == x.SpotLengthId).Display,
                        DayPart = x.Daypart.Text,
                        Programs = x.ProgramCriteria,
                        Genres = x.GenreCriteria,
                        Sequence = x.Sequence,
                    }).OrderBy(x => x.Sequence).ToList(),
                    GuaranteedDemo = _AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString,
                    Advertiser = advertiser != null ? advertiser.Display : string.Empty,
                    SecondaryDemos = proposal.SecondaryDemos.Select(x => _AudiencesCache.GetDisplayAudienceById(x).AudienceString).ToList()
                };

                var clientScrubs = scrubs;
                if (clientScrubs == null)
                    clientScrubs = _AffidavitRepository.GetProposalDetailPostScrubbing(proposalId, proposalScrubbingRequest.ScrubbingStatusFilter);

                //load ClientScrubs
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
                //load filters
                result.Filters = new FilterOptions
                {
                    DistinctDayOfWeek = result.ClientScrubs.Select(x => x.DayOfWeek).Distinct().OrderBy(x => x).ToList(),
                    DistinctGenres = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.GenreName)).Select(x => x.GenreName).Distinct().OrderBy(x => x).ToList(),
                    DistinctPrograms = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ProgramName)).Select(x => x.ProgramName).Distinct().OrderBy(x => x).ToList(),
                    WeekStart = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.WeekStart).OrderBy(x => x).First() : (DateTime?)null,
                    WeekEnd = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.WeekStart).OrderBy(x => x).Last().AddDays(7) : (DateTime?)null,
                    DateAiredStart = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.DateAired).OrderBy(x => x).First() : (DateTime?)null,
                    DateAiredEnd = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.DateAired).OrderBy(x => x).Last() : (DateTime?)null,
                    DistinctMarkets = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.Market)).Select(x => x.Market).Distinct().OrderBy(x => x).ToList(),
                    DistinctClientIscis = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ClientISCI)).Select(x => x.ClientISCI).Distinct().OrderBy(x => x).ToList(),
                    DistinctHouseIscis = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ISCI)).Select(x => x.ISCI).Distinct().OrderBy(x => x).ToList(),
                    DistinctSpotLengths = result.ClientScrubs.Select(x => x.SpotLength).Distinct().OrderBy(x => x).ToList(),
                    DistinctAffiliates = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.Affiliate)).Select(x => x.Affiliate).Distinct().OrderBy(x => x).ToList(),
                    DistinctStations = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.Station)).Select(x => x.Station).Distinct().OrderBy(x => x).ToList(),
                    DistinctWeekStarts = result.ClientScrubs.Select(x => x.WeekStart).Distinct().OrderBy(x => x).ToList(),
                    DistinctShowTypes = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ShowTypeName)).Select(x => x.ShowTypeName).Distinct().OrderBy(x => x).ToList(),
                    DistinctSequences = result.ClientScrubs.Where(x => x.Sequence.HasValue).Select(x => x.Sequence.Value).Distinct().OrderBy(x => x).ToList(),
                    TimeAiredStart = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.TimeAired).OrderBy(x => x).First() : (int?)null,
                    TimeAiredEnd = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.TimeAired).OrderBy(x => x).Last() : (int?)null
                };
                return result;
            }
        }

        /// <summary>
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <param name="archived">Switch for the archived iscis</param>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<UnlinkedIscisDto> GetUnlinkedIscis(bool archived)
        {
            var spotsLength = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            var iscis = archived ? _PostRepository.GetArchivedIscis() : _PostRepository.GetUnlinkedIscis();
            iscis.ForEach(x => x.SpotLength = spotsLength.Single(y => y.Value == x.SpotLength).Key);
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
        /// Archives an isci from the unlinked isci list
        /// </summary>
        /// <param name="fileDetailIds">Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        /// <returns>True or false based on the errors</returns>
        public bool ArchiveUnlinkedIsci(List<long> fileDetailIds, string username)
        {
            List<AffidavitFileDetail> fileDetailList = _PostRepository.LoadFileDetailsByIds(fileDetailIds);
            List<string> iscisToArchive = fileDetailList.Select(x => x.Isci).Distinct().ToList();
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
