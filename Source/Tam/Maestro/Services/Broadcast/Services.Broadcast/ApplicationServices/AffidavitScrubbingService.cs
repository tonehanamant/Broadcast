using Common.Services;
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

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitScrubbingService : IApplicationService
    {
        List<PostDto> GetPosts();
        /// <summary>
        /// Gets a client post scrubbing proposal header
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns>ProposalDto object containing the post scrubbing header</returns>
        ClientPostScrubbingProposalHeaderDto GetClientPostScrubbingProposalHeader(int proposalId);
        /// <summary>
        /// Gets a client post scrubbing proposal detail
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <param name="detailId">Detail Id of the proposal to filter by</param>
        /// <returns></returns>
        ClientPostScrubbingProposalDetailDto GetClientPostScrubbingProposalDetail(int proposalId, int detailId);
    }

    public class PostDto
    {
        public int ContractId { get; set; }
        public string ContractName { get; set; }
        public DateTime? UploadDate { get; set; }
        public int SpotsInSpec { get; set; }
        public int SpotsOutOfSpec { get; set; }
        public double? PrimaryAudienceImpressions { get; set; }
    }

    public class AffidavitScrubbingService : IAffidavitScrubbingService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitRepository _AffidavitRepositry;
        private readonly IPostRepository _PostRepository;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IDaypartCache _DaypartCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly ISMSClient _SmsClient;
        protected readonly IProposalService _ProposalService;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;

        public AffidavitScrubbingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache,
            ISMSClient smsClient,
            IProposalService proposalService,
            IBroadcastAudiencesCache audiencesCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitRepositry = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _SpotLengthRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _PostRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
            _DaypartCache = daypartCache;
            _AudiencesCache = audiencesCache;
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _SmsClient = smsClient;
            _ProposalService = proposalService;
        }

        public List<PostDto> GetPosts()
        {
            return _PostRepository.GetAllPostFiles();
        }

        /// <summary>
        /// Gets a client post scrubbing proposal header
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns>ProposalDto object containing the post scrubbing header</returns>
        public ClientPostScrubbingProposalHeaderDto GetClientPostScrubbingProposalHeader(int proposalId)
        {

            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(proposalId);
                var advertiser = _SmsClient.FindAdvertiserById(proposal.AdvertiserId);

                ClientPostScrubbingProposalHeaderDto result = new ClientPostScrubbingProposalHeaderDto
                {
                    Id = proposal.Id.Value,
                    Name = proposal.ProposalName,
                    Notes = proposal.Notes,
                    Markets = proposal.Markets,
                    MarketGroupId = proposal.MarketGroupId,
                    BlackoutMarketGroup = proposal.BlackoutMarketGroup,
                    BlackoutMarketGroupId = proposal.BlackoutMarketGroupId,
                    Details = proposal.Details.Select(x => new ProposalScrubbingDetailHeaderDto()
                    {
                        DayPart = x.Daypart.Text,
                        FlightEndDate = x.FlightEndDate,
                        FlightStartDate = x.FlightStartDate,
                        Id = x.Id.Value,
                        SpotLength = proposal.SpotLengths.First(y => y.Id == x.SpotLengthId).Display
                    }).ToList(),
                    GuaranteedDemo = _AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString,
                    Advertiser = advertiser != null ? advertiser.Display : string.Empty
                };

                proposal.SecondaryDemos.ForEach(x => result.SecondaryDemos.Add(_AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString));

                return result;
            }
        }

        /// <summary>
        /// Gets a client post scrubbing proposal detail
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns></returns>
        public ClientPostScrubbingProposalDetailDto GetClientPostScrubbingProposalDetail(int proposalId, int detailId)
        {

            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                ProposalDto proposal = _ProposalService.GetProposalById(proposalId);

                ProposalDetailDto proposalDetail = proposal.Details.First(x => x.Id == detailId);

                var result = new ClientPostScrubbingProposalDetailDto
                {
                    Id = proposalDetail.Id,
                    FlightStartDate = proposalDetail.FlightStartDate,
                    FlightEndDate = proposalDetail.FlightEndDate,
                    SpotLength = proposal.SpotLengths.First(x => x.Id == proposalDetail.SpotLengthId).Display,
                    DayPart = proposalDetail.Daypart.Text,
                    Programs = proposalDetail.ProgramCriteria,
                    Genres = proposalDetail.GenreCriteria,
                    ClientScrubs = _AffidavitRepositry.GetProposalDetailPostScrubbing(detailId)
                };
                result.ClientScrubs.ForEach(x => x.WeekStart = _MediaMonthAndWeekCache.GetMediaWeekContainingDate(x.TimeAired).StartDate);
                return result;
            }
        }
    }
}
