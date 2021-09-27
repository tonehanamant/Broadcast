using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface ISpotTrackerService : IApplicationService
    {
        /// <summary>
        /// Saves A to AA columns from a sigma file
        /// </summary>
        /// <param name="fileSaveRequest">FileSaveRequest object</param>
        /// <param name="username">User requesting the file load</param>
        /// <returns>List of duplicated messages</returns>
        List<string> SaveSigmaFile(FileSaveRequest fileSaveRequest, string username);

        /// <summary>
        /// Returns spot tracker report data for requested proposal 
        /// </summary>
        /// <param name="proposalId">Proposal Identifier</param>
        /// <returns>Spot tracker report data model</returns>
        SpotTrackerReport GetSpotTrackerReportDataForProposal(int proposalId);

        /// <summary>
        /// Generates spot tracker report for requested proposal 
        /// </summary>
        /// <param name="proposalId">Proposal Identifier</param>
        /// <returns>ReportOutput contains spot tracker report file name and the report which is represented as a stream of zip archive</returns>
        ReportOutput GenerateSpotTrackerReport(int proposalId);
    }

    public class SpotTrackerService : ISpotTrackerService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly ISigmaConverter _SigmaConverter;        
        private readonly ISpotTrackerRepository _SpotTrackerRepository;
        private readonly IProposalRepository _ProposalRepository;
        private readonly IProposalBuyRepository _ProposalBuyRepository;
        private readonly IFileService _FileService;
        private readonly ISMSClient _SmsClient;
        private readonly IMediaMonthAndWeekAggregateRepository _MediaMonthAndWeekAggregateRepository;

        public SpotTrackerService(IDataRepositoryFactory repositoryFactory
            , ISigmaConverter sigmaConverter
            , IFileService fileService
            , ISMSClient smsClient)
        {
            _BroadcastDataRepositoryFactory = repositoryFactory;
            _SigmaConverter = sigmaConverter;
            _SpotTrackerRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotTrackerRepository>();
            _ProposalRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            _ProposalBuyRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalBuyRepository>();
            _MediaMonthAndWeekAggregateRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IMediaMonthAndWeekAggregateRepository>();
            _FileService = fileService;
            _SmsClient = smsClient;
        }
        
        /// <summary>
        /// Saves A to AA columns from a sigma file
        /// </summary>
        /// <param name="fileSaveRequest">FileSaveRequest object</param>
        /// <param name="username">User requesting the file load</param>
        /// <returns>List of duplicated messages</returns>
        public List<string> SaveSigmaFile(FileSaveRequest fileSaveRequest, string username)
        {
            List<string> duplicateMessages = new List<string>();
            
            StringBuilder errorMessages = new StringBuilder();
            var hasErrors = false;

            if (fileSaveRequest.Files.Count < 1)
            {
                throw new ApplicationException("Empty Request. No files uploaded.");
            }

            foreach (var requestFile in fileSaveRequest.Files)
            {
                try
                {
                    //compute file hash to check against duplicate files being loaded
                    var hash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(requestFile.StreamData));

                    //check if file has already been loaded
                    if (_SpotTrackerRepository.GetSigmaFileIdByHash(hash) > 0)
                    {
                        throw new ApplicationException("Unable to load spot tracker file. The selected file has already been loaded.");
                    }

                    //we made it this far, it must be a new file - persist the file
                    TrackerFile<SpotTrackerFileDetail> sigmaFile = new TrackerFile<SpotTrackerFileDetail>();
                    Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int> lineInfo = new Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int>();

                    sigmaFile = _SigmaConverter.ExtractSigmaDataExtended(requestFile.StreamData, hash, username, requestFile.FileName, out lineInfo);

                    string message = string.Empty;
                    if (!string.IsNullOrEmpty(message))
                    {
                        errorMessages.AppendLine(message);
                        hasErrors = true;
                    }

                    var filterResult = _SpotTrackerRepository.FilterOutExistingDetails(sigmaFile.FileDetails);
                    sigmaFile.FileDetails = filterResult.New;

                    duplicateMessages.AddRange(_CreateDuplicateMessages(lineInfo, filterResult.Ignored, "The following line(s) were previously imported and were ignored"));
                    duplicateMessages.AddRange(_CreateDuplicateMessages(lineInfo, filterResult.Updated, "The following line(s) were previously imported and were updated with new program name"));

                    _SpotTrackerRepository.SaveSpotTrackerFile(sigmaFile);

                    errorMessages.AppendLine($"File '{requestFile.FileName}' uploaded successfully.<br />");
                }
                catch (Exception e)
                {
                    hasErrors = true;
                    errorMessages.AppendLine($"File '{requestFile.FileName}' failed validation for Sigma import<br />");
                    errorMessages.AppendLine($"Message: {e.Message}");
                }
            }

            if (hasErrors)
                throw new Exception(errorMessages.ToString());

            return duplicateMessages;
        }

        public ReportOutput GenerateSpotTrackerReport(int proposalId)
        {
            var spotTrackerReportData = GetSpotTrackerReportDataForProposal(proposalId);
            var reportGenerator = new SpotTrackerReportGenerator();
            return reportGenerator.Generate(spotTrackerReportData);
        }

        public SpotTrackerReport GetSpotTrackerReportDataForProposal(int proposalId)
        {
            var proposal = _ProposalRepository.GetProposalById(proposalId);

            var report = new SpotTrackerReport
            {
                Id = proposal.Id.Value,
                ZipFileName = $"{proposal.ProposalName.PrepareForUsingInFileName()} Spot Tracker Report.zip",
                Details = proposal.Details.Select(d => new SpotTrackerReport.Detail
                {
                    Id = d.Id.Value
                }).ToList()
            };

            _SetReportDetailBuys(report);

            // We generate a report file only for details with proposal buys
            report.Details = report.Details.Where(x => x.ProposalBuyFile != null).ToList();

            _SetSpotsData(report);
            _SetFileNames(report, proposal.AdvertiserId);
            return report;
        }

        private void _SetFileNames(SpotTrackerReport report, int advertiserId)
        {
            var advertiser = _SmsClient.FindAdvertiserById(advertiserId);
            foreach (var detail in report.Details)
            {
                detail.FileName = $"{advertiser.Display} {detail.ProposalBuyFile.EstimateId} Spot Tracker Report.xlsx";
            }
        }

        private void _SetReportDetailBuys(SpotTrackerReport report)
        {
            var detailIds = report.Details.Select(x => x.Id);
            var proposalBuys = _ProposalBuyRepository.GetProposalBuyFilesForProposalDetails(detailIds);

            foreach (var reportDetail in report.Details)
            {
                reportDetail.ProposalBuyFile = proposalBuys.SingleOrDefault(x => x.ProposalVersionDetailId == reportDetail.Id);
            }
        }

        private void _SetSpotsData(SpotTrackerReport report)
        {
            var proposalBuys = report.Details.Select(x => x.ProposalBuyFile);
            var estimateIds = proposalBuys.Select(x => x.EstimateId);
            var spotTrackerFileDetails = _SpotTrackerRepository.GetSpotTrackerFileDetailsByEstimateIds(estimateIds);

            foreach (var reportDetail in report.Details)
            {
                reportDetail.Weeks = _GetWeeklySpotsDataForSpotTrackerReport(reportDetail.ProposalBuyFile, spotTrackerFileDetails);
            }
        }

        private IEnumerable<string> _CreateDuplicateMessages(Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int> lineInfo, List<SpotTrackerFileDetail> list, string title)
        {
            var duplicateMessages = new List<string>();
            if (list.Any())
            {
                duplicateMessages.Add($"<p>{title}:</p><ul>");
                duplicateMessages.AddRange(list
                    .Select(x => string.Format("<li>Line {0}: Station {1}, Date {2}, Time Aired {3}, ISCI {4}, Spot Length {5}, Campaign {6}, Advertiser {7}</li>",
                        lineInfo[new TrackerFileDetailKey<SpotTrackerFileDetail>(x)], x.Station, x.DateAired, x.TimeAired, x.Isci, x.SpotLength, x.EstimateId, x.Advertiser))
                    .ToList());
                duplicateMessages.Add("</ul>");
            }
            return duplicateMessages;
        }
        
        private int _GetDeliveredSpotsForWeekAndStation(
            IEnumerable<DeliveredSpotsValueForStationsForWeek> deliveredSpotsData,
            int mediaWeekId,
            string station)
        {
            return deliveredSpotsData
                .SingleOrDefault(x => x.MediaWeekId == mediaWeekId)
                ?.DeliveredSpotsValueForStations.SingleOrDefault(x => x.Station.Equals(station, StringComparison.InvariantCultureIgnoreCase))?.Spots ?? 0;
        }

        private IEnumerable<DeliveredSpotsValueForStationsForWeek> _GetDeliveredSpotsData(IEnumerable<SpotTrackerFileDetail> spotTrackerFileDetails)
        {
            var mediaMonthAggregate = _MediaMonthAndWeekAggregateRepository.GetMediaMonthAggregate();
            var spotTrackerFileDetailsGroupedByMediaWeek = spotTrackerFileDetails.GroupBy(x => mediaMonthAggregate.GetMediaWeekContainingDate(x.DateAired).Id);

            return spotTrackerFileDetailsGroupedByMediaWeek.Select(spotTrackerFileDetailsForMediaWeek => new DeliveredSpotsValueForStationsForWeek
            {
                MediaWeekId = spotTrackerFileDetailsForMediaWeek.Key,
                DeliveredSpotsValueForStations = spotTrackerFileDetailsForMediaWeek
                        .GroupBy(x => x.Station, StringComparer.InvariantCultureIgnoreCase)
                        .Select(x => new DeliveredSpotsValueForStationsForWeek.DeliveredSpotsValueForStation
                        {
                            Station = x.Key,
                            Spots = x.Count()
                        })
            });
        }
        
        private IEnumerable<SpotTrackerReport.Detail.Week> _GetWeeklySpotsDataForSpotTrackerReport(ProposalBuyFile buy, IEnumerable<SpotTrackerFileDetail> spotTrackerFileDetails)
        {
            var spotTrackerFileDetailsByEstimateId = spotTrackerFileDetails.Where(x => x.EstimateId == buy.EstimateId);
            var deliveredSpotsData = _GetDeliveredSpotsData(spotTrackerFileDetailsByEstimateId);

            return buy.Details
                .SelectMany(x => x.Weeks, (detail, week) => new
                {
                    detail.Station,
                    week.MediaWeek,
                    week.Spots
                })
                .GroupBy(x => new { x.MediaWeek.Id, x.MediaWeek.WeekStartDate })
                .Select(groupingByWeek => new SpotTrackerReport.Detail.Week
                {
                    MediaWeekId = groupingByWeek.Key.Id,
                    StartDate = groupingByWeek.Key.WeekStartDate,
                    StationSpotsValues = groupingByWeek
                        .GroupBy(x => new StationGrouping
                        {
                            OriginMarket = x.Station.OriginMarket,
                            Affiliation = x.Station.Affiliation,
                            LegacyCallLetters = x.Station.LegacyCallLetters
                        })
                        .Select(groupingByStation => new SpotTrackerReport.Detail.Week.StationSpotsValue
                        {
                            Market = groupingByStation.Key.OriginMarket,
                            Affiliate = groupingByStation.Key.Affiliation,
                            Station = groupingByStation.Key.LegacyCallLetters,
                            SpotsOrdered = groupingByStation.Sum(spots => spots.Spots),
                            SpotsDelivered = _GetDeliveredSpotsForWeekAndStation(deliveredSpotsData, groupingByWeek.Key.Id, groupingByStation.Key.LegacyCallLetters)
                        })
                });
        }

        // Only for case-insensitive comparison
        private class StationGrouping
        {
            public string OriginMarket { get; set; }

            public string Affiliation { get; set; }

            public string LegacyCallLetters { get; set; }

            public override bool Equals(object obj)
            {
                var stationGrouping = obj as StationGrouping;

                return OriginMarket.Equals(stationGrouping.OriginMarket, StringComparison.InvariantCultureIgnoreCase) &&
                    Affiliation.Equals(stationGrouping.Affiliation, StringComparison.InvariantCultureIgnoreCase) &&
                    LegacyCallLetters.Equals(stationGrouping.LegacyCallLetters, StringComparison.InvariantCultureIgnoreCase);
            }

            public override int GetHashCode()
            {
                var hashCode = 1677836291;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OriginMarket);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Affiliation);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LegacyCallLetters);
                return hashCode;
            }
        }
    }
}
