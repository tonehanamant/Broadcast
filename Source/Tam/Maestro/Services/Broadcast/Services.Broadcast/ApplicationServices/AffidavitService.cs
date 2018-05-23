using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public enum AffidaviteFileProcessingStatus
    {
        Valid = 1,
        Invalid = 2
    };

    public interface IAffidavitService : IApplicationService
    {
        AffidavitSaveResult SaveAffidavit(AffidavitSaveRequest saveRequest, string username,DateTime currentDateTime);

        string JSONifyFile(Stream rawStream,string fileName,out AffidavitSaveRequest request);
    }

    public class AffidavitService : IAffidavitService
    {
        private const ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3;
        private readonly IAffidavitMatchingEngine _AffidavitMatchingEngine;
        private readonly IAffidavitProgramScrubbingEngine _AffidavitProgramScrubbingEngine;
        protected readonly IProposalService _ProposalService;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IProjectionBooksService _ProjectionBooksService;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IProposalRepository _ProposalRepository;
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IAffidavitValidationEngine _AffidavitValidationEngine;
        private readonly INsiPostingBookService _NsiPostingBookService;

        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IAffidavitMatchingEngine affidavitMatchingEngine,
            IAffidavitProgramScrubbingEngine affidavitProgramScrubbingEngine,
            IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalService proposalService,
            IProjectionBooksService projectionBooksService,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IAffidavitValidationEngine affidavitValidationEngine,
            INsiPostingBookService nsiPostingBookService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitMatchingEngine = affidavitMatchingEngine;
            _AffidavitProgramScrubbingEngine = affidavitProgramScrubbingEngine;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalService = proposalService;
            _ProjectionBooksService = projectionBooksService;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _ProposalRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _AffidavitValidationEngine = affidavitValidationEngine;
            _NsiPostingBookService = nsiPostingBookService;
        }

        public AffidavitSaveResult SaveAffidavit(AffidavitSaveRequest saveRequest, string username, DateTime currentDateTime)
        {

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

            AffidavitSaveResult result;
            result = _AffidavitSaveResult(saveRequest, username, currentDateTime, affidavitFile);

            return result;
        }

        private AffidavitSaveResult _AffidavitSaveResult(AffidavitSaveRequest saveRequest, string username,
            DateTime currentDateTime, AffidavitFile affidavitFile)
        {
            Dictionary<int, int> spotLengthDict = null;
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(saveRequest);
            _SetPostingBookId(matchedAffidavitDetails, postingBookId);
            var affidavitValidationResults = new List<AffidavitValidationResult>();

            foreach (var matchedAffidavitDetail in matchedAffidavitDetails)
            {
                var validationResults =
                    _AffidavitValidationEngine.ValidateAffidavitRecord(matchedAffidavitDetail.AffidavitDetail);

                if (validationResults.Any())
                {
                    validationResults.ForEach(r => r.InvalidLine = matchedAffidavitDetail.LineNumber);
                    affidavitValidationResults.AddRange(validationResults);

                    var problems = _MapValidationErrorToAffidavitFileProblem(affidavitValidationResults);
                    affidavitFile.AffidavitFileProblems.AddRange(problems);
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
                    InventorySource = (int)matchedAffidavitDetail.AffidavitDetail.InventorySource,
                    SpotCost = matchedAffidavitDetail.AffidavitDetail.SpotCost,
                    LeadinGenre = matchedAffidavitDetail.AffidavitDetail.LeadInGenre,
                    LeadoutGenre = matchedAffidavitDetail.AffidavitDetail.LeadOutGenre,
                    LeadinProgramName = matchedAffidavitDetail.AffidavitDetail.LeadInProgramName,
                    LeadoutProgramName = matchedAffidavitDetail.AffidavitDetail.LeadOutProgramName,
                    LeadInEndTime =
                        Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.LeadInEndTime.TimeOfDay.TotalSeconds),
                    LeadOutStartTime =
                        Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.LeadOutStartTime.TimeOfDay.TotalSeconds),
                    ShowType = matchedAffidavitDetail.AffidavitDetail.ShowType,
                    LeadInShowType = matchedAffidavitDetail.AffidavitDetail.LeadInShowType,
                    LeadOutShowType = matchedAffidavitDetail.AffidavitDetail.LeadOutShowType,

                    AffidavitClientScrubs =
                        matchedAffidavitDetail.ProposalDetailWeeks.Select(
                            w => new AffidavitClientScrub
                            {
                                ProposalVersionDetailQuarterWeekId = w.ProposalVersionDetailQuarterWeekId,
                                ProposalVersionDetailId = w.ProposalVersionDetailId,
                                MatchTime = w.TimeMatch,
                                MatchDate = w.DateMatch,
                                ModifiedBy = username,
                                ModifiedDate = currentDateTime,
                                EffectiveProgramName = matchedAffidavitDetail.AffidavitDetail.ProgramName,
                                EffectiveGenre = matchedAffidavitDetail.AffidavitDetail.Genre,
                                EffectiveShowType = matchedAffidavitDetail.AffidavitDetail.ShowType,
                                LeadIn = w.IsLeadInMatch,
                                PostingBookId = w.ProposalVersionDetailPostingBookId.Value
                            }).ToList(),
                    AffidavitFileDetailProblems = matchedAffidavitDetail.AffidavitDetailProblems,
                    Demographics = matchedAffidavitDetail.AffidavitDetail.Demographics
                };

                affidavitFile.AffidavitFileDetails.Add(det);
            }

            var result = new AffidavitSaveResult();
            result.ValidationResults = affidavitValidationResults;
            affidavitFile.Status = affidavitValidationResults.Any() ? AffidaviteFileProcessingStatus.Invalid : AffidaviteFileProcessingStatus.Valid;

            _ScrubAffidavitFile(affidavitFile);
            _CalculateAffidavitImpressions(affidavitFile);

            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                result.Id = _AffidavitRepository.SaveAffidavitFile(affidavitFile);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(affidavitFile));
                transaction.Complete();
            }

            return result;
        }

        private List<Tuple<int, int>> _GetProposalDetailIdsWithPostingBookId(AffidavitFile affidavitFile)
        {
            var result = affidavitFile.AffidavitFileDetails.SelectMany(d => d.AffidavitClientScrubs.Select(s => new Tuple<int, int>(s.ProposalVersionDetailId, s.PostingBookId.Value))).ToList();
            return result;
        }

        private void _SetPostingBookId(List<AffidavitMatchingDetail> matchedAffidavitDetails, int postingBookId)
        {            
            foreach(var proposalDetailWeek in matchedAffidavitDetails.SelectMany(d => d.ProposalDetailWeeks))
            {
                if (!proposalDetailWeek.ProposalVersionDetailPostingBookId.HasValue)
                {
                    proposalDetailWeek.ProposalVersionDetailPostingBookId = postingBookId;
                }
            }
        }

        public void _ScrubAffidavitFile(AffidavitFile affidavitFile)
        {
            var stations = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStations().ToDictionary(k => k.LegacyCallLetters, v => v);

            foreach (var affidavitDetail in affidavitFile.AffidavitFileDetails)
            {
                if (!affidavitDetail.AffidavitClientScrubs.Any())
                    continue;

                var quarterWeekIds =
                    affidavitDetail.AffidavitClientScrubs.Select(s => s.ProposalVersionDetailQuarterWeekId).ToList();
                var proposals = _ProposalService.GetProposalsByQuarterWeeks(quarterWeekIds);

                var affidavitStation = _MatchAffidavitStation(stations, affidavitDetail);

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
                        var markets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposal, scrub.PostingBookId.Value);

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

        private static DisplayBroadcastStation _MatchAffidavitStation(Dictionary<string, DisplayBroadcastStation> stations, AffidavitFileDetail affidavitDetail)
        {
            const string tvEnding = "-TV";
            string stationName = affidavitDetail.Station;

            if (stations.ContainsKey(stationName))
                return stations[stationName];

            var index = stationName.LastIndexOf(tvEnding,StringComparison.CurrentCultureIgnoreCase);
            if (index < 0)
                return null;

            stationName = stationName.Remove(index);
            if (!stations.ContainsKey(stationName))
            {
                return null;
            }

            return stations[stationName];
        }

        private List<AffidavitFileProblem> _MapValidationErrorToAffidavitFileProblem(List<AffidavitValidationResult> affidavitValidationResults)
        {
            List<AffidavitFileProblem> problems = new List<AffidavitFileProblem>();

            affidavitValidationResults.ForEach(v =>
            {
                AffidavitFileProblem problem = new AffidavitFileProblem();
                var description = v.ErrorMessage;
                if (!string.IsNullOrEmpty(v.InvalidField))
                {
                    description = string.Format("Record: {0}: Field: '{1}' is invalid\r\n{2}",v.InvalidLine,v.InvalidField,v.ErrorMessage);
                }
                problem.ProblemDescription = description;
                problems.Add(problem);
            });
            return problems;
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

        private void _CalculateAffidavitImpressions(AffidavitFile affidavitFile)
        {
            var details = affidavitFile.AffidavitFileDetails;
           
            var audiencesRepository = _BroadcastDataRepositoryFactory.GetDataRepository<INsiComponentAudienceRepository>();
            var audiencesIds =
                audiencesRepository.GetAllNsiComponentAudiences().
                Select(a => a.Id).
                ToList();
            

            CalcMultiDetailImpressions(details, audiencesIds);
        }

        private void CalcMultiDetailImpressions(ICollection<AffidavitFileDetail> details,  List<int> audiencesIds)
        {
            var postingBookIds = details.SelectMany(d => d.AffidavitClientScrubs).Select(s => s.PostingBookId).Distinct().ToList();

            foreach(var postingBookId in postingBookIds)
            {
                var stationDetails = new List<StationDetailPointInTime>();
                var ctr = 1;
                var affidavitDetailsForPostingBook = details.Where(d => d.AffidavitClientScrubs.Any(s => s.PostingBookId == postingBookId)).ToList();
                foreach (var affidavitFileDetail in affidavitDetailsForPostingBook)
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
                var impressionsPointInTime = ratingForecastRepository.GetImpressionsPointInTime(postingBookId.Value, audiencesIds,
                    stationDetails,
                    DefaultPlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

                ctr = 1;
                foreach (var affidavitFileDetail in affidavitDetailsForPostingBook)
                {
                    var imps = impressionsPointInTime.Where(i => i.id == ctr).ToList();

                    affidavitFileDetail.AffidavitClientScrubs.Where(s => s.PostingBookId == postingBookId)
                        .ForEach(s => s.AffidavitClientScrubAudiences = imps.Select(imp => new AffidavitClientScrubAudience
                            {
                                AffidavitClientScrubId = s.Id,
                                AudienceId = imp.audience_id,
                                Impressions = imp.impressions
                            }).ToList()
                        );
                    ctr++;
                }
            }
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
            request.Source = (int)AffidaviteFileSourceEnum.Strata;

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
                    //detail.InventorySource = Int32.Parse(reader.GetCellValue("Inventory Source"));
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