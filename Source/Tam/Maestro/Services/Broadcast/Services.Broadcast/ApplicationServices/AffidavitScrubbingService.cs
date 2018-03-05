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
        /// Gets a client post scrubbing proposal with details
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <returns>ClientPostScrubbingProposalDto object containing the post scrubbing information</returns>
        ClientPostScrubbingProposalDto GetClientScrubbingForProposal(int proposalId);
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
         /// Gets a client post scrubbing proposal with details
         /// </summary>
         /// <param name="proposalId">Proposal id to filter by</param>
         /// <returns>ClientPostScrubbingProposalDto object containing the post scrubbing information</returns>
        public ClientPostScrubbingProposalDto GetClientScrubbingForProposal(int proposalId)
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
                    Details = proposal.Details.Select(x=> new ClientPostScrubbingProposalDetailDto
                    {
                        Id = x.Id,
                        FlightStartDate = x.FlightStartDate,
                        FlightEndDate = x.FlightEndDate,
                        SpotLength = proposal.SpotLengths.First(y => y.Id == x.SpotLengthId).Display,
                        DayPart = x.Daypart.Text,
                        Programs = x.ProgramCriteria,
                        Genres = x.GenreCriteria,
                        ClientScrubs = _AffidavitRepositry.GetProposalDetailPostScrubbing(x.Id.Value)
                    }).ToList(),
                    GuaranteedDemo = _AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString,
                    Advertiser = advertiser != null ? advertiser.Display : string.Empty
                };

                proposal.SecondaryDemos.ForEach(x => result.SecondaryDemos.Add(_AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString));

                return result;
            }
        }
                result.ClientScrubs.ForEach(x => x.WeekStart = _MediaMonthAndWeekCache.GetMediaWeekContainingDate(x.TimeAired).StartDate);
                return result;
    }
}
