using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
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
        ClientPostScrubbingProposalDto GetClientScrubbingForProposal(int proposalId);

        /// <summary>
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<UnlinkedIscisDto> GetUnlinkedIscis();

        /// <summary>
        /// Generates the excep NSI Post Report for a specific proposal
        /// </summary>
        /// <param name="proposalId">Proposal id to generate the report for</param>
        /// <returns>ReportOutput object containing the report and the filename</returns>
        ReportOutput GenerateNSIPostReport(int proposalId);

        /// <summary>
        /// Generates My Events report
        /// </summary>
        /// <param name="proposalId">Proposal id to generate the report for</param>
        /// <returns>ReportOutput object containing the report and the filename</returns>
        ReportOutput GenerateMyEventsReport(int proposalId);

        /// <summary>
        /// Gets the NSI Post Report data
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <returns>List of NSIPostReportDto objects</returns>
        NsiPostReport GetNsiPostReportData(int proposalId);
    }

    public class AffidavitScrubbingService : IAffidavitScrubbingService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly INsiMarketRepository _NsiMarketRepository;
        private readonly IAffidavitRepository _AffidavitRepositry;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IPostRepository _PostRepository;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly ISMSClient _SmsClient;
        private readonly IProposalService _ProposalService;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly Lazy<Image> _LogoImage;
        private readonly IPostingBooksService _PostingBooksService;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;

        public AffidavitScrubbingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISMSClient smsClient,
            IProposalService proposalService,
            IBroadcastAudiencesCache audiencesCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IPostingBooksService postingBooksService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _PostRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
            _NsiMarketRepository = _BroadcastDataRepositoryFactory.GetDataRepository<INsiMarketRepository>();
            _AffidavitRepositry = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _SpotLengthRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _BroadcastAudienceRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _AudiencesCache = audiencesCache;
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _SmsClient = smsClient;
            _ProposalService = proposalService;
            _PostingBooksService = postingBooksService;
            _LogoImage = new Lazy<Image>(() => Image.FromStream(new MemoryStream(_SmsClient.GetLogoImage(CMWImageEnums.CMW_CADENT_LOGO).ImageData)));
        }

        /// <summary>
        /// Returns a list of the posts nad unlinked iscis in the system
        /// </summary>
        /// <returns>List of PostDto objects</returns>
        public PostedContractedProposalsDto GetPosts()
        {
            return new PostedContractedProposalsDto()
            {
                Posts = _PostRepository.GetAllPostFiles(),
                UnlinkedIscis = _PostRepository.CountUnlinkedIscis()
            };
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

                //load ClientScrubs
                result.Details.ForEach(x =>
                {
                    var clientScrubs = _AffidavitRepository.GetProposalDetailPostScrubbing(x.Id.Value);
                    clientScrubs.ForEach(y =>
                    {
                        y.Sequence = x.Sequence;
                        y.ProposalDetailId = x.Id;
                    });
                    result.ClientScrubs.AddRange(clientScrubs);
                });
                //load filters
                result.Filters = new FilterOptions
                {
                    DistinctDayOfWeek = result.ClientScrubs.Select(x => x.DayOfWeek).Distinct().OrderBy(x => x).ToList(),
                    DistinctGenres = result.ClientScrubs.Select(x => x.GenreName).Distinct().OrderBy(x => x).ToList(),
                    WeekStart = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.WeekStart).OrderBy(x => x).First() : (DateTime?)null,
                    WeekEnd = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.WeekStart).OrderBy(x => x).Last().AddDays(7) : (DateTime?)null
                };
                return result;
            }
        }

        /// <summary>
        /// Returns a list of unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<UnlinkedIscisDto> GetUnlinkedIscis()
        {
            return _PostRepository.GetUnlinkedIscis();
        }

        /// <summary>
        /// Generates the excep NSI Post Report for a specific proposal
        /// </summary>
        /// <param name="proposalId">Proposal id to generate the report for</param>
        /// <returns>ReportOutput object containing the report and the filename</returns>
        public ReportOutput GenerateNSIPostReport(int proposalId)
        {
            var nsiPostReport = GetNsiPostReportData(proposalId);
            var reportGenerator = new NSIPostReportGenerator(_LogoImage.Value);
            return reportGenerator.Generate(nsiPostReport);
        }

        /// <summary>
        /// Gets the NSI Post Report data
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <returns>List of NSIPostReportDto objects</returns>
        public NsiPostReport GetNsiPostReportData(int proposalId)
        {
            var proposal = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>().GetProposalById(proposalId);

            var flights = _GetFlightsRange(proposal.Details);
            var inspecSpots = _AffidavitRepositry.GetInSpecSpotsForProposal(proposalId);
            var proposalAdvertiser = _SmsClient.FindAdvertiserById(proposal.AdvertiserId);
            var proposalAudienceIds = new List<int>() { proposal.GuaranteedDemoId };
            proposalAudienceIds.AddRange(proposal.SecondaryDemos);
            var proposalAudiences = _BroadcastAudienceRepository.GetAudienceDtosById(proposalAudienceIds)
                .OrderBy(a => proposalAudienceIds.IndexOf(a.Id)).ToList(); //This ordering by the original audience id order. Primary audience first.
            var audiencesMappings = _BroadcastAudienceRepository.GetRatingAudiencesGroupedByMaestroAudience(proposalAudiences.Select(a => a.Id).ToList());
            var spotLengthMappings = _SpotLengthRepository.GetSpotLengthsById();
            var mediaWeeks = _MediaMonthAndWeekCache.GetMediaWeeksByContainingDate(inspecSpots.Select(s => s.AirDate).Distinct().ToList());
            var stationMappings = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStationListByLegacyCallLetters(inspecSpots.Select(s => s.Station).Distinct().ToList())
                .ToDictionary(k => k.LegacyCallLetters, v => v);
            var latestPostingBooks = _PostingBooksService.GetDefaultPostingBooks();
            var nsiMarketRankings = _NsiMarketRepository.GetMarketRankingsByMediaMonth(latestPostingBooks.DefaultShareBook.PostingBookId.Value);
            var guaranteedDemo = _AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString;
            var nsiPostReport = new NsiPostReport(proposalId, inspecSpots, proposalAdvertiser, proposalAudiences,
                                                audiencesMappings, spotLengthMappings,
                                                mediaWeeks, stationMappings, nsiMarketRankings, guaranteedDemo, proposal.GuaranteedDemoId, flights);

            return nsiPostReport;
        }

        private List<Tuple<DateTime, DateTime>> _GetFlightsRange(List<ProposalDetailDto> details)
        {
            var proposalWeekFlights = details.SelectMany(d => d.Quarters.SelectMany(q => q.Weeks.Select(w => new ProposalFlightWeek()
            {
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                IsHiatus = w.IsHiatus,
                MediaWeekId = w.MediaWeekId
            }))).GroupBy(x => x.MediaWeekId).Select(x => new ProposalFlightWeek()
            {
                StartDate = x.First().StartDate,
                EndDate = x.First().EndDate,
                IsHiatus = x.All(y => y.IsHiatus),
                MediaWeekId = x.First().MediaWeekId
            }).OrderBy(x => x.StartDate).ToList();

            var flightRanges = new List<Tuple<DateTime, DateTime>>();
            var flights = new List<ProposalFlightWeek>();
            ProposalFlightWeek lastFlight = null;

            foreach (var flight in proposalWeekFlights)
            {
                var isWeekGap = lastFlight != null && flight.StartDate.AddDays(-1) != lastFlight.EndDate;
                var isDateGap = flight.IsHiatus || isWeekGap;

                if (isDateGap && flights.Any())
                {
                    var dateRangeTuple = new Tuple<DateTime, DateTime>(flights.Min(d => d.StartDate), flights.Max(d => d.EndDate));
                    flightRanges.Add(dateRangeTuple);
                    flights.Clear();
                }

                if (!flight.IsHiatus)
                {
                    flights.Add(flight);
                }

                lastFlight = flight;

                if (isWeekGap)
                    lastFlight = null;
            }

            if (flights.Any())
            {
                var dateRangeTuple = new Tuple<DateTime, DateTime>(flights.Min(d => d.StartDate), flights.Max(d => d.EndDate));
                flightRanges.Add(dateRangeTuple);
            }

            return flightRanges;
        }

        public ReportOutput GenerateMyEventsReport(int proposalId)
        {
            var myEventsReportDataRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IMyEventsReportDataRepository>();
            var myEventsReportGenerator = new MyEventsReportGenerator();

            return myEventsReportGenerator.Generate(myEventsReportDataRepository.GetMyEventsReportData(proposalId));
        }
    }
}
