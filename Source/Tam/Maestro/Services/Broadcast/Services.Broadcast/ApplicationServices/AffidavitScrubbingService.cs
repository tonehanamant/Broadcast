using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        ClientPostScrubbingProposalDto GetClientScrubbingForProposal(int proposalId, ProposalScrubbingRequest proposalScrubbingRequest);

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

        /// <summary>
        /// Gets the My Events Report data
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <returns>List of My Events Data</returns>
        List<MyEventsReportData> GetMyEventsReportData(int proposalId);
    }

    public class AffidavitScrubbingService : IAffidavitScrubbingService
    {
        private const string MyEventsZipFileName = "MYEventsReport.zip";

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
        private readonly IProjectionBooksService _PostingBooksService;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;

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
        public ClientPostScrubbingProposalDto GetClientScrubbingForProposal(int proposalId, ProposalScrubbingRequest proposalScrubbingRequest)
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
                    var clientScrubs = _AffidavitRepository.GetProposalDetailPostScrubbing(x.Id.Value, proposalScrubbingRequest.ScrubbingStatusFilter);
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
                    DistinctGenres = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.GenreName)).Select(x => x.GenreName).Distinct().OrderBy(x => x).ToList(),
                    DistinctPrograms = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ProgramName)).Select(x => x.ProgramName).Distinct().OrderBy(x => x).ToList(),
                    WeekStart = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.WeekStart).OrderBy(x => x).First() : (DateTime?)null,
                    WeekEnd = result.ClientScrubs.Any() ? result.ClientScrubs.Select(x => x.WeekStart).OrderBy(x => x).Last().AddDays(7) : (DateTime?)null,
                    DistinctMarkets = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.Market)).Select(x => x.Market).Distinct().OrderBy(x => x).ToList(),
                    DistinctClientIscis = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ClientISCI)).Select(x => x.ClientISCI).Distinct().OrderBy(x => x).ToList(),
                    DistinctHouseIscis = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ISCI)).Select(x => x.ISCI).Distinct().OrderBy(x => x).ToList(),
                    DistinctSpotLengths = result.ClientScrubs.Select(x => x.SpotLength).Distinct().OrderBy(x => x).ToList(),
                    DistinctAffiliates = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.Affiliate)).Select(x => x.Affiliate).Distinct().OrderBy(x => x).ToList(),
                    DistinctStations = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.Station)).Select(x => x.Station).Distinct().OrderBy(x => x).ToList(),
                    DistinctWeekStarts = result.ClientScrubs.Select(x => x.WeekStart).Distinct().OrderBy(x => x).ToList(),
                    DistinctShowTypes = result.ClientScrubs.Where(x => !string.IsNullOrWhiteSpace(x.ShowTypeName)).Select(x => x.ShowTypeName).Distinct().OrderBy(x => x).ToList()
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

            inspecSpots.ForEach(x =>
            {
                x.ProposalWeekCost = x.ProposalWeekTotalCost / x.Units;
                x.ProposalWeekImpressionsGoal = x.ProposalWeekTotalImpressionsGoal / x.Units;
                x.ProposalWeekCPM = x.ProposalWeekTotalCost / (decimal)x.ProposalWeekTotalImpressionsGoal * 1000;
            });

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
            var latestPostingBooks = _PostingBooksService.GetDefaultProjectionBooks();
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

        /// <summary>
        /// Generates My Events report
        /// </summary>
        /// <param name="proposalId">Proposal id to generate the report for</param>
        /// <returns>ReportOutput object containing the report and the filename</returns>
        public ReportOutput GenerateMyEventsReport(int proposalId)
        {
            var myEventsReportData = GetMyEventsReportData(proposalId);
            var myEventsReportGenerator = new MyEventsReportGenerator();
            var reports = new List<ReportOutput>();

            if (!myEventsReportData.Any())
                throw new Exception("No data found for MyEvents report");

            foreach (var reportData in myEventsReportData)
                reports.Add(myEventsReportGenerator.Generate(reportData));

            if (reports.Count == 1)
                return reports.First();

            return _CreateReportFromZipArchive(_CreateZipArchive(reports));
        }

        private ReportOutput _CreateReportFromZipArchive(MemoryStream memoryStream)
        {
            return new ReportOutput(MyEventsZipFileName) { Stream = memoryStream };
        }

        private MemoryStream _CreateZipArchive(List<ReportOutput> reports)
        {
            var memoryStream = new MemoryStream();

            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var report in reports)
                {
                    var zipEntry = zip.CreateEntry(report.Filename);
                    using (var zipStream = zipEntry.Open())
                    {
                        report.Stream.Position = 0;
                        report.Stream.CopyTo(zipStream);
                    }
                }
            }

            return memoryStream;
        }

        public List<MyEventsReportData> GetMyEventsReportData(int proposalId)
        {
            var affidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            var myEventsReportDataList = affidavitRepository.GetMyEventsReportData(proposalId);
            var spotLengths = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthsById();

            foreach (var report in myEventsReportDataList)
            {
                foreach (var line in report.Lines)
                {
                    var advertiser = _SmsClient.FindAdvertiserById(line.AdvertiserId);

                    line.Advertiser = advertiser.Display;
                    line.SpotLength = spotLengths[line.SpotLengthId];
                }

                _UpdateSpotTimesForThreeMinuteWindow(report.Lines);
            }

            return myEventsReportDataList;
        }

        private void _UpdateSpotTimesForThreeMinuteWindow(List<MyEventsReportDataLine> myEventsReportDataList)
        {
            var sorted = myEventsReportDataList.OrderBy(x => x.AirDate).ToArray();

            for (var i = 0; i < sorted.Length; i++)
            {
                for (var j = i + 1; j < sorted.Length; j++)
                {
                    var timeDifference = _GetDateWithoutSeconds(sorted[j].AirDate) - _GetDateWithoutSeconds(sorted[i].AirDate);

                    if (timeDifference.TotalMinutes >= 0 && timeDifference.TotalMinutes < 3)
                    {
                        var adjustmentTimeDifference = 3 - timeDifference.TotalMinutes;

                        sorted[j].AirDate = sorted[j].AirDate.AddMinutes(adjustmentTimeDifference);
                        sorted[j].LineupStartTime = sorted[j].LineupStartTime.AddMinutes(adjustmentTimeDifference);
                    }
                }
            }
        }

        private DateTime _GetDateWithoutSeconds(DateTime dateTime)
        {
            return dateTime.AddSeconds(-dateTime.Second);
        }
    }
}
