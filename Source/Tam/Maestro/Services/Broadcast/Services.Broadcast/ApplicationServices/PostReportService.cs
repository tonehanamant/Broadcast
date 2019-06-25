using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
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
        /// Generates the excel NSI or NTI Post Report for a specific proposal
        /// </summary>
        /// <param name="proposalId">Proposal id to generate the report for</param>
        /// <param name="withOvernightImpressions">Optional parameter used to generate the NSI report with overnight impressions</param>
        /// <param name="isNtiReport">Optional parameter used to generate the NTI report</param>
        /// <returns>ReportOutput object containing the report and the filename</returns>
        ReportOutput GeneratePostReport(int proposalId, bool withOvernightImpressions = false, bool isNtiReport = false);

        /// <summary>
        /// Generates My Events report
        /// </summary>
        /// <param name="proposalId">Proposal id to generate the report for</param>
        /// <returns>ReportOutput object containing the report and the filename</returns>
        ReportOutput GenerateMyEventsReport(int proposalId);

        /// <summary>
        /// Gets the NSI/NTI Post Report data
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <param name="withOvernightImpressions">Switch for overnight impressions</param>
        /// <param name="isNtiReport">Switch for NTI report</param>
        /// <returns>List of PostReportDto objects</returns>
        PostReport GetPostReportData(int proposalId, bool withOvernightImpressions = false, bool isNtiReport = false);

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
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IMyEventsReportNamingEngine _MyEventsReportNamingEngine;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;

        public PostReportService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISMSClient smsClient,
            IProposalService proposalService,
            IBroadcastAudiencesCache audiencesCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IMyEventsReportNamingEngine myEventsReportNamingEngine,
            IStationProcessingEngine stationProcessingEngine,
            IImpressionAdjustmentEngine impressionAdjustmentEngine)
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
            _MyEventsReportNamingEngine = myEventsReportNamingEngine;
            _StationProcessingEngine = stationProcessingEngine;
            _LogoImage = new Lazy<Image>(() => Image.FromStream(new MemoryStream(_SmsClient.GetLogoImage(CMWImageEnums.CMW_CADENT_LOGO).ImageData)));
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
        }

        /// <summary>
        /// Generates the excel NSI or NTI Post Report for a specific proposal
        /// </summary>
        /// <param name="proposalId">Proposal id to generate the report for</param>
        /// <param name="withOvernightImpressions">Optional parameter used to generate the NSI report with overnight impressions</param>
        /// <param name="isNtiReport">Optional parameter used to generate the NTI report</param>
        /// <returns>ReportOutput object containing the report and the filename</returns>
        public ReportOutput GeneratePostReport(int proposalId, bool withOvernightImpressions = false, bool isNtiReport = false)
        {
            var nsiPostReport = GetPostReportData(proposalId, withOvernightImpressions, isNtiReport);
            var reportGenerator = new PostReportGenerator(_LogoImage.Value);
            return reportGenerator.Generate(nsiPostReport);
        }
        
        /// <summary>
        /// Gets the NSI/NTI Post Report data
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <param name="withOvernightImpressions">Switch for overnight impressions</param>
        /// <param name="isNtiReport">Switch for NTI report</param>
        /// <returns>List of PostReportDto objects</returns>
        public PostReport GetPostReportData(int proposalId, bool withOvernightImpressions, bool isNtiReport)
        {
            var proposal = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>().GetProposalById(proposalId);
            
            var flights = _GetFlightsRange(proposal.Details);
            var inspecSpots = _AffidavitRepository.GetInSpecSpotsForProposal(proposalId);

            inspecSpots.ForEach(x =>
            {
                x.ProposalWeekCost = x.ProposalWeekTotalCost / x.Units;
                x.ProposalWeekImpressionsGoal = x.ProposalWeekTotalImpressionsGoal / x.Units;
                x.ProposalWeekCPM = x.ProposalWeekTotalCost / (decimal)x.ProposalWeekTotalImpressionsGoal * 1000;
                var iscis = x.WeekIscis.Where(i => i.HouseIsci == x.HouseIsci && !string.IsNullOrWhiteSpace(i.Brand));
                x.Brand = string.Join(", ", iscis.Select(i => i.Brand));
            });

            var proposalAdvertiser = _SmsClient.FindAdvertiserById(proposal.AdvertiserId);
            var proposalAudienceIds = new List<int>() { proposal.GuaranteedDemoId };
            proposalAudienceIds.AddRange(proposal.SecondaryDemos);
            var proposalAudiences = _BroadcastAudienceRepository.GetAudienceDtosById(proposalAudienceIds)
                .OrderBy(a => proposalAudienceIds.IndexOf(a.Id)).ToList(); //This ordering by the original audience id order. Primary audience first.
            var audiencesMappings = _BroadcastAudienceRepository.GetRatingAudiencesGroupedByMaestroAudience(proposalAudiences.Select(a => a.Id).ToList());
            var spotLengthMappings = _SpotLengthRepository.GetSpotLengthAndIds();
            var mediaWeeks = _MediaMonthAndWeekCache.GetMediaWeeksByContainingDate(inspecSpots.Select(s => s.AirDate).Distinct().ToList());
            var stationMappings = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStationListByLegacyCallLetters(inspecSpots.Select(s => _StationProcessingEngine.StripStationSuffix(s.Station)).Distinct().ToList())
                .ToDictionary(k => k.LegacyCallLetters, v => v);
            var nsiMarketRankings = _GetMarketRankingsByPostingBook(inspecSpots);
            var guaranteedDemo = _AudiencesCache.GetDisplayAudienceById(proposal.GuaranteedDemoId).AudienceString;
            var postingBooks = _MediaMonthAndWeekCache.GetMediaMonthsByIds(inspecSpots.Select(x => x.ProposalDetailPostingBookId.Value).Distinct().ToList());
            var playbackTypes = inspecSpots.Select(x => x.ProposalDetailPlaybackType.Value).Distinct().ToList();

            return new PostReport(proposalId, inspecSpots, proposalAdvertiser.Display, proposalAudiences, audiencesMappings, spotLengthMappings,
                                                mediaWeeks, stationMappings, nsiMarketRankings, guaranteedDemo, flights,
                                                withOvernightImpressions, _ImpressionAdjustmentEngine, postingBooks, playbackTypes, proposal, isNtiReport);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inspecSpots"></param>
        /// <returns>Dictionary by posting book id of dictionaries of maret rankings by market id</returns>
        private Dictionary<int, Dictionary<int, int>> _GetMarketRankingsByPostingBook(List<InSpecAffidavitFileDetail> inspecSpots)
        {
            var result = new Dictionary<int, Dictionary<int, int>>();
            var postingBooksFromProposalDetails = inspecSpots.Select(s => s.ProposalDetailPostingBookId.Value).Distinct().ToList();
            foreach (var postingBookId in postingBooksFromProposalDetails)
            {
                var marketRankings = _NsiMarketRepository.GetMarketRankingsByMediaMonth(postingBookId);
                result.Add(postingBookId, marketRankings);
            }
            return result;
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
            var myEventsReportDataList = _AffidavitRepository.GetMyEventsReportData(proposalId);
            var spotLengths = _SpotLengthRepository.GetSpotLengthAndIds();

            foreach (var report in myEventsReportDataList)
            {
                foreach (var line in report.Lines)
                {
                    var advertiser = _SmsClient.FindAdvertiserById(line.AdvertiserId);
                    var mediaWeeks = _MediaMonthAndWeekCache.GetMediaWeeksByContainingDate(new List<DateTime>() { line.LineupStartDate });

                    line.Advertiser = advertiser.Display;
                    line.SpotLength = spotLengths.Single(x => x.Value == line.SpotLengthId).Key;
                    line.ScheduleStartDate = mediaWeeks[line.LineupStartDate].StartDate;
                    line.ScheduleEndDate = mediaWeeks[line.LineupStartDate].EndDate;

                    //remove -xx like endings from the station name
                    line.StationCallLetters = _StationProcessingEngine.StripStationSuffix(line.StationCallLetters);
                }

                _UpdateToNationalClock(report.Lines);
                _UpdateSpotTimesForThreeMinuteWindow(report.Lines);
            }

            return myEventsReportDataList;
        }

        private void _UpdateToNationalClock(List<MyEventsReportDataLine> lines)
        {
            TimeSpan startTime = new TimeSpan(3, 0, 0);
            TimeSpan endTime = new TimeSpan(5, 59, 0);
            foreach (var line in lines)
            {
                if (line.LineupStartDate.DayOfWeek != DayOfWeek.Monday && line.LineupStartTime.TimeOfDay >= startTime && line.LineupStartTime.TimeOfDay <= endTime)
                {
                    line.LineupStartDate = line.LineupStartDate.AddDays(-1);
                }
            }
        }

        private void _UpdateSpotTimesForThreeMinuteWindow(List<MyEventsReportDataLine> myEventsReportDataList)
        {
            var grouped = myEventsReportDataList.GroupBy(x => x.StationCallLetters);

            foreach (var group in grouped)
            {
                var sorted = group.OrderBy(x => x.AirDate).ToArray();

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
        }

        private DateTime _GetDateWithoutSeconds(DateTime dateTime)
        {
            return dateTime.AddSeconds(-dateTime.Second);
        }
    }
}
