using Common.Services.ApplicationServices;
using System.Linq;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Services.Broadcast.Converters;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public enum AffidaviteFileSource
    {
        Strata = 1
    };

    public interface IAffidavitService : IApplicationService
    {
        AffidavitSaveResult SaveAffidavit(AffidavitSaveRequest saveRequest, string username,DateTime currentDateTime);

        ///
        /// Scrubs, but does not save results
        ///
        void ScrubAffidavitFile(AffidavitFile affidavitFile,int marketBookingId);

        string JSONifyFile(Stream rawStream,string fileName,out AffidavitSaveRequest request);
    }

    public class AffidavitService : IAffidavitService
    {
        private const ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3;
        private readonly IAffidavitMatchingEngine _AffidavitMatchingEngine;
        private readonly IAffidavitProgramScrubbingEngine _AffidavitProgramScrubbingEngine;
        protected readonly IProposalService _ProposalService;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IPostingBooksService _PostingBooksService;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly int _BroadcastMatchingBuffer;
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IAffidavitValidationEngine _AffidavitValidationEngine;

        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IAffidavitMatchingEngine affidavitMatchingEngine,
            IAffidavitProgramScrubbingEngine affidavitProgramScrubbingEngine,
            IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalService proposalService,
            IPostingBooksService postingBooksService,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IAffidavitValidationEngine affidavitValidationEngine)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitMatchingEngine = affidavitMatchingEngine;
            _AffidavitProgramScrubbingEngine = affidavitProgramScrubbingEngine;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalService = proposalService;
            _PostingBooksService = postingBooksService;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _AffidavitValidationEngine = affidavitValidationEngine;
        }

        public AffidavitSaveResult SaveAffidavit(AffidavitSaveRequest saveRequest, string username, DateTime currentDateTime)
        {
            Dictionary<int, int> spotLengthDict = null;
            if (saveRequest == null)
            {
                throw new Exception("No affidavit data received.");
            }

            var affidavitFile = new AffidavitFile
            {
                CreatedDate = currentDateTime,
                FileHash = saveRequest.FileHash,
                FileName = saveRequest.FileName,
                SourceId = saveRequest.Source
            };

            var matchedAffidavitDetails = _LinkAndValidateContractIscis(saveRequest);
            var affidavitValidationResults = new List<AffidavitValidationResult>();

            foreach (var matchedAffidavitDetail in matchedAffidavitDetails)
            {
                var validationResults = _AffidavitValidationEngine.ValidateAffidavitRecord(matchedAffidavitDetail.AffidavitDetail);

               if (validationResults.Any())
                {
                    validationResults.ForEach(r => r.InvalidLine = matchedAffidavitDetail.LineNumber);
                    affidavitValidationResults.AddRange(validationResults);
                    continue;
                }

		var det = new AffidavitFileDetail
                {
                    AirTime = Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.AirTime.TimeOfDay.TotalSeconds),
                    OriginalAirDate = matchedAffidavitDetail.AffidavitDetail.AirTime,
                    Isci = matchedAffidavitDetail.AffidavitDetail.Isci,
                    ProgramName = matchedAffidavitDetail.AffidavitDetail.ProgramName,
                    Genre = matchedAffidavitDetail.AffidavitDetail.Genre,
                    SpotLengthId = _GetSpotlength(matchedAffidavitDetail.AffidavitDetail.SpotLength, ref spotLengthDict),
                    Station = matchedAffidavitDetail.AffidavitDetail.Station,
                    Market = matchedAffidavitDetail.AffidavitDetail.Market,
                    Affiliate = matchedAffidavitDetail.AffidavitDetail.Affiliate,
                    EstimateId = matchedAffidavitDetail.AffidavitDetail.EstimateId,
                    InventorySource = matchedAffidavitDetail.AffidavitDetail.InventorySource,
                    SpotCost = matchedAffidavitDetail.AffidavitDetail.SpotCost,
                    LeadinGenre = matchedAffidavitDetail.AffidavitDetail.LeadInGenre,
                    LeadoutGenre = matchedAffidavitDetail.AffidavitDetail.LeadOutGenre,
                    LeadinProgramName = matchedAffidavitDetail.AffidavitDetail.LeadInProgramName,
                    LeadoutProgramName = matchedAffidavitDetail.AffidavitDetail.LeadOutProgramName,
                    LeadInEndTime = Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.LeadInEndTime.TimeOfDay.TotalSeconds),
                    LeadOutStartTime = Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.LeadOutStartTime.TimeOfDay.TotalSeconds),
                    ShowType = matchedAffidavitDetail.AffidavitDetail.ShowType,
                    LeadInShowType = matchedAffidavitDetail.AffidavitDetail.LeadInShowType,
                    LeadOutShowType = matchedAffidavitDetail.AffidavitDetail.LeadOutShowType,

                    AffidavitClientScrubs =
                    matchedAffidavitDetail.ProposalDetailWeeks.Select(
                        w => new AffidavitClientScrub
                        {
                            ProposalVersionDetailQuarterWeekId = w.ProposalVersionDetailQuarterWeekId,
                            MatchTime = w.TimeMatch,
                            MatchDate = w.DateMatch,
                            ModifiedBy = username,
                            ModifiedDate = currentDateTime,
                            EffectiveProgramName = matchedAffidavitDetail.AffidavitDetail.ProgramName,
                            EffectiveGenre = matchedAffidavitDetail.AffidavitDetail.Genre,
                            EffectiveShowType = matchedAffidavitDetail.AffidavitDetail.ShowType,
                            LeadIn = w.IsLeadInMatch
                        }).ToList(),
                    AffidavitFileDetailProblems = matchedAffidavitDetail.AffidavitDetailProblems,
                    Demographics = matchedAffidavitDetail.AffidavitDetail.Demographics
                };

                affidavitFile.AffidavitFileDetails.Add(det);
            }

            var result = new AffidavitSaveResult();
            result.ValidationResults = affidavitValidationResults;

            if (affidavitValidationResults.Any())
            {
                return result;
            }

            var postingBookId = _GetPostingBookId();
            ScrubAffidavitFile(affidavitFile, postingBookId);

            _CalculateAffidavitImpressions(affidavitFile, postingBookId);

            var id = _AffidavitRepository.SaveAffidavitFile(affidavitFile);

            result.ID = id;
            return result;
        }

        public void ScrubAffidavitFile(AffidavitFile affidavitFile,int marketBookingId)
        {
            var callLetters = affidavitFile.AffidavitFileDetails.Select(a => a.Station).ToList();
            var stations = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStationListByLegacyCallLetters(callLetters).ToDictionary(k => k.LegacyCallLetters, v => v);

            foreach (var affidavitDetail in affidavitFile.AffidavitFileDetails)
            {
                if (!affidavitDetail.AffidavitClientScrubs.Any())
                    continue;

                var quarterWeekIds =
                    affidavitDetail.AffidavitClientScrubs.Select(s => s.ProposalVersionDetailQuarterWeekId).ToList();
                var proposals = _ProposalService.GetProposalsByQuarterWeeks(quarterWeekIds);

                DisplayBroadcastStation affidavitStation = null;
                if (stations.ContainsKey(affidavitDetail.Station))
                {
                    affidavitStation = stations[affidavitDetail.Station];
                }

                foreach (var scrub in affidavitDetail.AffidavitClientScrubs)
                {
                    var quarterWeekId = scrub.ProposalVersionDetailQuarterWeekId;
                    var proposal = proposals[quarterWeekId];
                    var proposalDetail = proposal.Details.Single(d =>
                        d.Quarters.Any(q => q.Weeks.Any(w => w.Id == quarterWeekId)));
                    var proposalWeek = proposalDetail.Quarters.SelectMany(d => d.Weeks.Where(w => w.Id == quarterWeekId)).First();
                    var proposalWeekIsci = proposalWeek.Iscis.First(i =>
                        i.HouseIsci.Equals(affidavitDetail.Isci, StringComparison.InvariantCultureIgnoreCase));
                    var dayOfWeek = affidavitDetail.OriginalAirDate.DayOfWeek;

                    scrub.MatchIsciDays = _IsIsciDaysMatch(proposalWeekIsci, dayOfWeek);

                    // match market/station
                    scrub.MatchStation = false;
                    scrub.MatchMarket = false;

                    if (affidavitStation != null)
                    {
                        var markets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposal, marketBookingId);

                        var marketGeoName = affidavitStation.OriginMarket;
                        if (markets.Any(m => m.Display == marketGeoName))
                        {
                        affidavitDetail.Market = marketGeoName;
                        scrub.MatchStation = true;
                        scrub.MatchMarket = true;
                        }
                    }

                    _AffidavitProgramScrubbingEngine.Scrub(proposalDetail, affidavitDetail, scrub);

                    scrub.Status = (scrub.MatchStation && scrub.MatchMarket && scrub.MatchGenre && scrub.MatchProgram && scrub.MatchTime && scrub.MatchIsciDays && scrub.MatchDate && scrub.MatchShowType) 
                        ? ScrubbingStatus.InSpec 
                        : ScrubbingStatus.OutOfSpec;
                }
            }
        }

        private bool _IsIsciDaysMatch(ProposalWeekIsciDto proposalWeekIsci, DayOfWeek dayOfWeek)
        {
            var isMatch = false;

            if (proposalWeekIsci.Sunday)
                isMatch = dayOfWeek == DayOfWeek.Sunday;

            if (proposalWeekIsci.Monday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Monday;

            if (proposalWeekIsci.Tuesday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Tuesday;

            if (proposalWeekIsci.Wednesday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Wednesday;

            if (proposalWeekIsci.Thursday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Thursday;

            if (proposalWeekIsci.Friday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Friday;

            if (proposalWeekIsci.Saturday)
                isMatch = isMatch || dayOfWeek == DayOfWeek.Saturday;

            return isMatch;
        }
        
        private List<AffidavitMatchingDetail> _LinkAndValidateContractIscis(AffidavitSaveRequest saveRequest)
        {
            var matchedAffidavitDetails = new List<AffidavitMatchingDetail>();
            int line = 1;
            foreach (var requestDetail in saveRequest.Details)
            {
                var proposalWeeks =
                    _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                        .GetAffidavitMatchingProposalWeeksByHouseIsci(requestDetail.Isci);

                var matchedProposalWeeks = _AffidavitMatchingEngine.Match(requestDetail, proposalWeeks);
                var matchingProblems = _AffidavitMatchingEngine.MatchingProblems();
                matchedAffidavitDetails.Add(new AffidavitMatchingDetail()
                {
                    LineNumber = line,
                    AffidavitDetail = requestDetail,
                    ProposalDetailWeeks = matchedProposalWeeks,
                    AffidavitDetailProblems = matchingProblems
                });

            }

            return matchedAffidavitDetails;
        }

        private int _GetSpotlength(int spotLength, ref Dictionary<int, int> spotLengthDict)
        {
            if (spotLengthDict == null)
                spotLengthDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();

            if (!spotLengthDict.ContainsKey(spotLength))
                throw new Exception(string.Format("Invalid spot length '{0}' found.", spotLength));

            return spotLengthDict[spotLength];
        }

        private int _GetPostingBookId()
        {
            var defaultPostingBooks = _PostingBooksService.GetDefaultPostingBooks();

            if (!defaultPostingBooks.DefaultShareBook.PostingBookId.HasValue)
                throw new Exception("No default posting book available");

            return defaultPostingBooks.DefaultShareBook.PostingBookId.Value;
        }

        private void _CalculateAffidavitImpressions(AffidavitFile affidavitFile, int postingBookId)
        {
            var details = affidavitFile.AffidavitFileDetails;
           
            var audiencesRepository = _BroadcastDataRepositoryFactory.GetDataRepository<INsiComponentAudienceRepository>();
            var audiencesIds =
                audiencesRepository.GetAllNsiComponentAudiences().
                Select(a => a.Id).
                ToList();
            var stationDetails = new List<StationDetailPointInTime>();

            CalcMultiDetailImpressions(postingBookId, details, stationDetails, audiencesIds);
        }

        private void CalcMultiDetailImpressions(int postingBookId, ICollection<AffidavitFileDetail> details, List<StationDetailPointInTime> stationDetails, List<int> audiencesIds)
        {
            var ctr = 1;
            foreach (var affidavitFileDetail in details)
            {
                stationDetails.Add(
                    new StationDetailPointInTime
                    {
                        Id = ctr++,
                        LegacyCallLetters = affidavitFileDetail.Station,
                        DayOfWeek = affidavitFileDetail.OriginalAirDate.DayOfWeek,
                        TimeAired = affidavitFileDetail.AirTime
                    }
                );
            }

            var ratingForecastRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            var impressionsPointInTime = ratingForecastRepository.GetImpressionsPointInTime(postingBookId, audiencesIds,
                stationDetails,
                DefaultPlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

            ctr = 1;
            foreach (var affidavitFileDetail in details)
            {
                var imps = impressionsPointInTime.Where(i => i.id == ctr).ToList();

                affidavitFileDetail.AffidavitFileDetailAudiences =
                    imps.Select(imp => new AffidavitFileDetailAudience
                    {
                        AffidavitFileDetailId = affidavitFileDetail.Id,
                        AudienceId = imp.audience_id,
                        Impressions = imp.impressions
                    }).ToList();
                ctr++;
            }
        }

        private List<AffidavitFileDetailAudience> _CalculdateImpressionsForNielsenAudiences(AffidavitFileDetail affidavitFileDetail, List<int> audiencesIds, int postingBookId)
        {
            var affidavitAudiences = new List<AffidavitFileDetailAudience>();
            var stationDetails = new List<StationDetailPointInTime>
            {
                new StationDetailPointInTime
                {
                    LegacyCallLetters = affidavitFileDetail.Station,
                    DayOfWeek = affidavitFileDetail.OriginalAirDate.DayOfWeek,
                    TimeAired = affidavitFileDetail.AirTime
                }
            };

            var ratingForecastRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            var impressionsPointInTime = ratingForecastRepository.GetImpressionsPointInTime(postingBookId, audiencesIds, stationDetails,
                DefaultPlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

            foreach (var impressionsWithAudience in impressionsPointInTime)
            {
                affidavitAudiences.Add(new AffidavitFileDetailAudience
                {
                    AffidavitFileDetailId = affidavitFileDetail.Id,
                    AudienceId = impressionsWithAudience.audience_id,
                    Impressions = impressionsWithAudience.impressions
                });
            }

            return affidavitAudiences;
        }

        #region JSONify

        private static readonly List<string> FileHeaders = new List<string>()
        {
            "ISCI"
            ,"Spot Length"
            ,"Station"
            ,"Spot Time"
            ,"ProgramName"
            ,"Genre"
            ,"LeadInTitle"
            ,"LeadInGenre"
            ,"LeadOutTitle"
            ,"LeadOutGenre"
        };
        
        public string JSONifyFile(Stream rawStream,string fileName,out AffidavitSaveRequest request)
        {
            TextFileLineReader reader;
            if (fileName.EndsWith("csv"))
            { 
                reader = new CsvFileReader(FileHeaders);
            }
            else
            {
                throw new Exception("Unknown file");
            }

            request = new AffidavitSaveRequest();
            request.FileName = fileName;
            request.FileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(rawStream));
            request.Source = (int)AffidaviteFileSource.Strata;

            using (reader.Initialize(rawStream))
            {
                while (!reader.IsEOF())
                {
                    reader.NextRow();

                    if (reader.IsEmptyRow())
                        break;

                    var detail = new AffidavitSaveRequestDetail();
                    
                    detail.AirTime = DateTime.Parse(reader.GetCellValue("Spot Time"));
                    detail.Genre = reader.GetCellValue("Genre");
                    detail.Isci = reader.GetCellValue("ISCI");
                    detail.ProgramName = reader.GetCellValue("ProgramName");
                    detail.Station = reader.GetCellValue("Station");
                    detail.SpotLength = int.Parse(reader.GetCellValue("Spot Length"));
                    detail.LeadInProgramName = reader.GetCellValue("LeadInTitle");
                    detail.LeadInGenre = reader.GetCellValue("LeadInGenre");
                    detail.LeadOutProgramName = reader.GetCellValue("LeadOutTitle");
                    detail.LeadOutGenre = reader.GetCellValue("LeadOutGenre");
                    detail.InventorySource = Int32.Parse(reader.GetCellValue("Inventory Source"));
                    detail.Affiliate = reader.GetCellValue("Affiliate");
                    detail.ShowType = reader.GetCellValue("ShowType");
                    detail.LeadInShowType = reader.GetCellValue("LeadInShowType");
                    detail.LeadOutShowType = reader.GetCellValue("LeadOutShowType");

                    request.Details.Add(detail);
                }
            }
            var json = JsonConvert.SerializeObject(request);
            return json;
        }
        #endregion
    }
}