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
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostReportService : IApplicationService
    {
        /// <summary>
        /// Generates the excep NSI Post Report for a specific proposal
        /// </summary>
        /// <param name="proposalId">Proposal id to generate the report for</param>
        /// <param name="withOvernightImpressions">Optional parameter used to generate the NSI report with overnight impressions</param>
        /// <returns>ReportOutput object containing the report and the filename</returns>
        ReportOutput GenerateNSIPostReport(int proposalId, bool withOvernightImpressions = false);

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
        /// <param name="withOvernightImpressions">Switch for overnight impressions</param>
        /// <returns>List of NSIPostReportDto objects</returns>
        NsiPostReport GetNsiPostReportData(int proposalId, bool withOvernightImpressions = false);

        /// <summary>
        /// Gets the My Events Report data
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <returns>List of My Events Data</returns>
        List<MyEventsReportData> GetMyEventsReportData(int proposalId);
    }

    public class PostReportService : IPostReportService
    {
        private const string MyEventsZipFileName = "MYEventsReport - {0} - {1}.zip";

        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly INsiMarketRepository _NsiMarketRepository;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly ISMSClient _SmsClient;
        private readonly IProposalService _ProposalService;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly Lazy<Image> _LogoImage;
        private readonly IProjectionBooksService _PostingBooksService;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;

        public PostReportService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISMSClient smsClient,
            IProposalService proposalService,
            IBroadcastAudiencesCache audiencesCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IProjectionBooksService postingBooksService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _NsiMarketRepository = _BroadcastDataRepositoryFactory.GetDataRepository<INsiMarketRepository>();
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
        /// Generates the excel NSI Post Report for a specific proposal
        /// </summary>
        /// <param name="proposalId">Proposal id to generate the report for</param>
        /// <param name="withOvernightImpressions">Optional parameter used to generate the NSI report with overnight impressions</param>
        /// <returns>ReportOutput object containing the report and the filename</returns>
        public ReportOutput GenerateNSIPostReport(int proposalId, bool withOvernightImpressions = false)
        {
            var nsiPostReport = GetNsiPostReportData(proposalId, withOvernightImpressions);
            var reportGenerator = new NSIPostReportGenerator(_LogoImage.Value);
            return reportGenerator.Generate(nsiPostReport);
        }

        /// <summary>
        /// Gets the NSI Post Report data
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <param name="withOvernightImpressions">Switch for overnight impressions</param>
        /// <returns>List of NSIPostReportDto objects</returns>
        public NsiPostReport GetNsiPostReportData(int proposalId, bool withOvernightImpressions)
        {
            var proposal = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>().GetProposalById(proposalId);
            
            var flights = _GetFlightsRange(proposal.Details);
            var inspecSpots = _AffidavitRepository.GetInSpecSpotsForProposal(proposalId);

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
            var spotLengthMultipliers = _SpotLengthRepository.GetSpotLengthMultipliers();
            var mediaWeeks = _MediaMonthAndWeekCache.GetMediaWeeksByContainingDate(inspecSpots.Select(s => s.AirDate).Distinct().ToList());
            var stationMappings = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStationListByLegacyCallLetters(inspecSpots.Select(s => s.Station).Distinct().ToList())
                .ToDictionary(k => k.LegacyCallLetters, v => v);
            var latestPostingBooks = _PostingBooksService.GetDefaultProjectionBooks();
            var nsiMarketRankings = _NsiMarketRepository.GetMarketRankingsByMediaMonth(latestPostingBooks.DefaultShareBook.PostingBookId.Value);
            var guaranteedDemo = _AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString;

            return new NsiPostReport(proposalId, inspecSpots, proposalAdvertiser, proposalAudiences, audiencesMappings, spotLengthMappings, spotLengthMultipliers,
                                                mediaWeeks, stationMappings, nsiMarketRankings, guaranteedDemo, proposal.GuaranteedDemoId, flights,
                                                withOvernightImpressions, proposal.Equivalized);
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

            return _CreateReportFromZipArchive(_CreateZipArchive(reports), proposalId);
        }

        private ReportOutput _CreateReportFromZipArchive(MemoryStream memoryStream, int proposalId)
        {
            return new ReportOutput(string.Format(MyEventsZipFileName, proposalId, DateTime.Now.ToString("yyyyMMddHHmmss"))) { Stream = memoryStream };
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
