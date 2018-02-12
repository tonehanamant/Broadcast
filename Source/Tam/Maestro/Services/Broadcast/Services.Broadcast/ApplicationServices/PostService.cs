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
    public interface IPostService : IApplicationService
    {
        List<PostDto> GetPosts();
        /// <summary>
        /// Gets a client post scrubbing proposal header
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns>ProposalDto object containing the post scrubbing header</returns>
        PostScrubbingProposalHeaderDto GetClientPostScrubbingProposalHeader(int proposalId);
        /// <summary>
        /// Gets a client post scrubbing proposal detail
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <param name="detailId">Detail Id of the proposal to filter by</param>
        /// <returns></returns>
        PostScrubbingProposalDetailDto GetClientPostScrubbingProposalDetail(int proposalId, int detailId);
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

    public class PostService : IPostService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IPostRepository _PostRepository;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IDaypartCache _DaypartCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly ISMSClient _SmsClient;
        protected readonly IProposalService _ProposalService;

        public PostService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache,
            ISMSClient smsClient,
            IProposalService proposalService,
            IBroadcastAudiencesCache audiencesCache)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _PostRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
            _SpotLengthRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _DaypartCache = daypartCache;
            _AudiencesCache = audiencesCache;
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
        public PostScrubbingProposalHeaderDto GetClientPostScrubbingProposalHeader(int proposalId)
        {
            PostScrubbingProposalHeaderDto result = new PostScrubbingProposalHeaderDto();
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(proposalId);
                
                result.Id = proposal.Id.Value;
                result.Name = proposal.ProposalName;
                result.Notes = proposal.Notes;
                proposal.SecondaryDemos.ForEach(x => result.SecondaryDemos.Add(_AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString));
                result.Details = proposal.Details.Select(x => new ProposalScrubbingDetailHeaderDto()
                {
                    DayPart = x.Daypart.Text,
                    FlightEndDate = x.FlightEndDate,
                    FlightStartDate = x.FlightStartDate,
                    Id = x.Id.Value,
                    SpotLength = proposal.SpotLengths.First(y => y.Id == x.SpotLengthId).Display
                }).ToList();
                result.Markets = proposal.Markets;
                result.GuaranteedDemo = _AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString;

                var advertiser = _SmsClient.FindAdvertiserById(proposal.AdvertiserId);
                result.Advertiser = advertiser != null ? advertiser.Display : string.Empty;

                return result;
            }
        }

        /// <summary>
        /// Gets a client post scrubbing proposal detail
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns></returns>
        public PostScrubbingProposalDetailDto GetClientPostScrubbingProposalDetail(int proposalId, int detailId)
        {

            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                ProposalDto proposal = _ProposalService.GetProposalById(proposalId);

                ProposalDetailDto proposalDetail = proposal.Details.First(x => x.Id == detailId);

                return new PostScrubbingProposalDetailDto
                {
                    Id = proposal.Id,
                    FlightStartDate = proposalDetail.FlightStartDate,
                    FlightEndDate = proposalDetail.FlightEndDate,
                    SpotLength = proposal.SpotLengths.First(x => x.Id == proposalDetail.SpotLengthId).Display,
                    DayPart = proposalDetail.Daypart.Text,
                    Details = _PostRepository.GetProposalDetailPostScrubbing(detailId),
                    Programs = proposalDetail.ProgramCriteria,
                    Genres = proposalDetail.GenreCriteria
                };
            }
        }        
    }
}
