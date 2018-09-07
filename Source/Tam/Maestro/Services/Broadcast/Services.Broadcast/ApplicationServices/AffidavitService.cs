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
using System.Transactions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.ApplicationServices
{
    public enum AffidaviteFileProcessingStatus
    {
        Valid = 1,
        Invalid = 2
    };

    public interface IAffidavitService : IApplicationService
    {
        AffidavitSaveResult SaveAffidavit(AffidavitSaveRequest saveRequest, string username, DateTime currentDateTime);

        AffidavitSaveResult SaveAffidavitValidationErrors(AffidavitSaveRequest saveRequest, string userName, List<AffidavitValidationResult> affidavitValidationResults);

        /// <summary>
        /// Scrubs an affidavit detail by an isci
        /// </summary>
        /// <param name="isci">Isci to scrub</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">User requesting the scrubbing</param>
        /// <returns>True or false</returns>
        bool ScrubUnlinkedAffidavitDetailsByIsci(string isci, DateTime currentDateTime, string username);

        bool RescrubProposalDetail(RescrubProposalDetailRequest request, string userName, DateTime changeDate);
        bool CanRescrubProposalDetail(ProposalDto proposal, ProposalDetailDto proposalDetail);

        string JSONifyFile(Stream rawStream, string fileName, out AffidavitSaveRequest request);

        /// <summary>
        /// Swaps one or more client scrubs to another proposal detail
        /// </summary>
        /// <param name="requestData">SwapProposalDetailRequest object containing a list of affidavit client scrubs to be swapped and the proposal detail id to swap to</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">Username requesting the change</param>
        /// <returns>True or false</returns>
        bool SwapProposalDetails(SwapProposalDetailRequest requestData, DateTime currentDateTime, string username);

        /// <summary>
        /// Maps an original isci to an effective isci
        /// </summary>
        /// <param name="mapIsciDto">MapIsciDto object containing the iscis to map</param>
        /// <param name="name">User requesting the mapping</param>
        /// <returns>The result of the mapping in true or false</returns>
        bool MapIsci(MapIsciDto mapIsciDto, DateTime currentDateTime, string name);
    }


    public class AffidavitService : IAffidavitService
    {
        public const string ProposalNotContractedMessage = "Proposal must be contracted";

        const string ARCHIVED_ISCI = "Not a Cadent Isci";
        private const ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3;

        private readonly IAffidavitMatchingEngine _AffidavitMatchingEngine;
        private readonly IAffidavitProgramScrubbingEngine _AffidavitProgramScrubbingEngine;
        private readonly IProposalService _ProposalService;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IProposalRepository _ProposalRepository;
        private readonly IPostRepository _PostRepository;
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IAffidavitValidationEngine _AffidavitValidationEngine;
        private readonly INsiPostingBookService _NsiPostingBookService;
        private readonly IAffidavitImpressionsService _AffidavitImpressionsService;
        private readonly Dictionary<int, int> _SpotLengthsDict;
        private readonly IStationProcessingEngine _StationProcessingEngine;

        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IAffidavitMatchingEngine affidavitMatchingEngine,
            IAffidavitProgramScrubbingEngine affidavitProgramScrubbingEngine,
            IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalService proposalService,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IAffidavitValidationEngine affidavitValidationEngine,
            INsiPostingBookService nsiPostingBookService,
            IAffidavitImpressionsService affidavitImpressionsService,
            IStationProcessingEngine stationProcessingEngine)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitMatchingEngine = affidavitMatchingEngine;
            _AffidavitProgramScrubbingEngine = affidavitProgramScrubbingEngine;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalService = proposalService;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _ProposalRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            _PostRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _AffidavitValidationEngine = affidavitValidationEngine;
            _NsiPostingBookService = nsiPostingBookService;
            _AffidavitImpressionsService = affidavitImpressionsService;
            _SpotLengthsDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            _StationProcessingEngine = stationProcessingEngine;
        }

        public AffidavitSaveResult SaveAffidavitValidationErrors(AffidavitSaveRequest saveRequest, string userName, List<AffidavitValidationResult> affidavitValidationResults)
        {
            var affidavitFile = _EnsureAffidavitFile(saveRequest, DateTime.Now);

            var problems = _MapValidationErrorToAffidavitFileProblem((affidavitValidationResults));
            affidavitFile.AffidavitFileProblems.AddRange(problems);
            var result = new AffidavitSaveResult
            {
                ValidationResults = affidavitValidationResults
            };
            affidavitFile.Status = affidavitValidationResults.Any() ? AffidaviteFileProcessingStatus.Invalid : AffidaviteFileProcessingStatus.Valid;

            if (affidavitValidationResults.Any())
            {   // save and get out
                result.Id = _AffidavitRepository.SaveAffidavitFile(affidavitFile);
            }
            return result;
        }

        private static AffidavitFile _EnsureAffidavitFile(AffidavitSaveRequest saveRequest, DateTime currentDateTime)
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
            return affidavitFile;
        }

        public AffidavitSaveResult SaveAffidavit(AffidavitSaveRequest saveRequest, string username, DateTime currentDateTime)
        {
            var affidavitFile = _EnsureAffidavitFile(saveRequest, currentDateTime);
            var result = _AffidavitSaveResult(saveRequest, username, currentDateTime, affidavitFile);

            return result;
        }


        private AffidavitSaveResult _AffidavitSaveResult(AffidavitSaveRequest saveRequest, string username,
            DateTime currentDateTime, AffidavitFile affidavitFile)
        {
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);

            var result = new AffidavitSaveResult();
            var affidavitValidationResults = new List<AffidavitValidationResult>();

            var lineNumber = 0;
            foreach (var saveRequestDetail in saveRequest.Details)
            {
                lineNumber++;
                var validationErrors = _AffidavitValidationEngine.ValidateAffidavitRecord(saveRequestDetail);

                if (validationErrors.Any())
                {
                    validationErrors.ForEach(r => r.InvalidLine = lineNumber);
                    var problems = _MapValidationErrorToAffidavitFileProblem(validationErrors);
                    affidavitFile.AffidavitFileProblems.AddRange(problems);

                    affidavitValidationResults.AddRange(validationErrors);
                }
            }

            result.ValidationResults = affidavitValidationResults;
            affidavitFile.Status = affidavitValidationResults.Any() ? AffidaviteFileProcessingStatus.Invalid : AffidaviteFileProcessingStatus.Valid;

            if (affidavitValidationResults.Any())
            {   // save and get out
                result.Id = _AffidavitRepository.SaveAffidavitFile(affidavitFile);
                return result;
            }

            var affidavitFileDetailsToBeLinked = _MapToAffidavitFileDetails(saveRequest.Details);
            foreach (var detail in affidavitFileDetailsToBeLinked)
            {
                if (_PostRepository.IsIsciBlacklisted(new List<string> { detail.Isci }))
                {
                    detail.AffidavitFileDetailProblems.Add(new AffidavitFileDetailProblem
                    {
                        Description = ARCHIVED_ISCI,
                        Type = AffidavitFileDetailProblemTypeEnum.ArchivedIsci
                    });
                    detail.Archived = true;
                    affidavitFile.AffidavitFileDetails.Add(detail);
                    affidavitFileDetailsToBeLinked.Remove(detail);
                }
            }

            _LoadIsciMappings(affidavitFileDetailsToBeLinked);
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(affidavitFileDetailsToBeLinked);
            _SetPostingBookData(matchedAffidavitDetails, postingBookId);

            //load AffidavitClientScrubs and AffidavitDetailProblems
            _MapToAffidavitFileDetails(matchedAffidavitDetails, currentDateTime, username);

            affidavitFile.AffidavitFileDetails.AddRange(matchedAffidavitDetails.Select(x => x.AffidavitDetail).ToList());

            _ScrubMatchedAffidavitRecords(affidavitFile.AffidavitFileDetails);
            _AffidavitImpressionsService.CalculateAffidavitImpressionsForAffidavitFileDetails(affidavitFile.AffidavitFileDetails);

            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                result.Id = _AffidavitRepository.SaveAffidavitFile(affidavitFile);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(affidavitFile.AffidavitFileDetails));
                transaction.Complete();
            }

            return result;
        }

        private void _LoadIsciMappings(List<AffidavitFileDetail> affidavitFileDetails)
        {
            Dictionary<string, string> isciMappings = _PostRepository.LoadIsciMappings(affidavitFileDetails.Select(x => x.Isci).ToList());
            if (isciMappings.Count > 0)
            {
                affidavitFileDetails.ForEach(x =>
                {
                    if (isciMappings.TryGetValue(x.Isci, out string value))
                    {
                        x.MappedIsci = value;
                    }
                });
            }
        }

        private List<AffidavitFileDetail> _MapToAffidavitFileDetails(List<AffidavitSaveRequestDetail> details)
        {
            var result = details.Select(d => new AffidavitFileDetail()
            {
                AirTime = Convert.ToInt32(d.AirTime.TimeOfDay.TotalSeconds),
                OriginalAirDate = d.AirTime,
                Isci = d.Isci,
                ProgramName = d.ProgramName,
                Genre = d.Genre,
                SpotLengthId = _GetSpotlength(d.SpotLength),
                Station = d.Station,
                Market = d.Market,
                Affiliate = d.Affiliate,
                EstimateId = d.EstimateId,
                InventorySource = (int)d.InventorySource,
                SpotCost = d.SpotCost,
                LeadinGenre = d.LeadInGenre,
                LeadoutGenre = d.LeadOutGenre,
                LeadinProgramName = d.LeadInProgramName,
                LeadoutProgramName = d.LeadOutProgramName,
                LeadInEndTime =
                        Convert.ToInt32(d.LeadInEndTime.TimeOfDay.TotalSeconds),
                LeadOutStartTime =
                        Convert.ToInt32(d.LeadOutStartTime.TimeOfDay.TotalSeconds),
                ShowType = d.ShowType,
                LeadInShowType = d.LeadInShowType,
                LeadOutShowType = d.LeadOutShowType,
                Demographics = d.Demographics
            }).ToList();
            return result;
        }

        public bool RescrubProposalDetail(RescrubProposalDetailRequest request, string userName, DateTime changeDate)
        {
            var proposal = _ProposalService.GetProposalById(request.ProposalId);
            var proposalDetail = proposal.Details.Single(d => d.Id == request.ProposalDetailId);

            EnsureProposalContracted(proposal,proposalDetail);

            var affidavitDetails = _AffidavitRepository.GetAffidavitDetails(proposalDetail.Id.Value);

            // use swap parameter to keep detail the same.
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(affidavitDetails, proposalDetail.Id.Value);
            _SetPostingBookData(matchedAffidavitDetails, proposalDetail.PostingBookId.Value,proposalDetail.PostingPlaybackType);

            _MapToAffidavitFileDetails(matchedAffidavitDetails, changeDate, userName);
            affidavitDetails = matchedAffidavitDetails.Select(ad => ad.AffidavitDetail).ToList();

            _ScrubMatchedAffidavitRecords(affidavitDetails);
            _AffidavitImpressionsService.CalculateAffidavitImpressionsForAffidavitFileDetails(affidavitDetails);

            using (var transaction = new TransactionScopeWrapper()) 
            {
                _AffidavitRepository.SaveScrubbedFileDetails(affidavitDetails);
                transaction.Complete();
            }

            return true;
        }

        public bool CanRescrubProposalDetail(ProposalDto proposal, ProposalDetailDto proposalDetail)
        {
            if (proposal.Status != ProposalEnums.ProposalStatusType.Contracted)
                return false;

            var affidavitDetails = _AffidavitRepository.GetAffidavitDetails(proposalDetail.Id.Value);
            if (!affidavitDetails.Any())
                return false;

            return true;
        }

        private void EnsureProposalContracted(ProposalDto proposal,ProposalDetailDto proposalDetail)
        {
            if (proposal.Status != ProposalEnums.ProposalStatusType.Contracted)
                throw new InvalidOperationException(ProposalNotContractedMessage);
        }

        /// <summary>
        /// Scrubs an affidavit detail by an isci
        /// </summary>
        /// <param name="isci">Isci to scrub</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">User requesting the scrubbing</param>
        /// <returns>True or false</returns>
        public bool ScrubUnlinkedAffidavitDetailsByIsci(string isci, DateTime currentDateTime, string username)
        {
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);
            var unlinkedAffidavitDetails = _AffidavitRepository.GetUnlinkedAffidavitDetailsByIsci(isci);
            _LoadIsciMappings(unlinkedAffidavitDetails);
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(unlinkedAffidavitDetails);
            _SetPostingBookData(matchedAffidavitDetails, postingBookId);

            //load AffidavitClientScrubs and AffidavitDetailProblems
            _MapToAffidavitFileDetails(matchedAffidavitDetails, currentDateTime, username);

            var affidavitFileDetails = matchedAffidavitDetails.Select(d => d.AffidavitDetail).ToList();
            _ScrubMatchedAffidavitRecords(affidavitFileDetails);
            _AffidavitImpressionsService.CalculateAffidavitImpressionsForAffidavitFileDetails(affidavitFileDetails);
            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                _AffidavitRepository.SaveScrubbedFileDetails(affidavitFileDetails);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(affidavitFileDetails));
                transaction.Complete();
            }
            return true;
        }

        private void _MapToAffidavitFileDetails(List<AffidavitMatchingDetail> matchedAffidavitDetails, DateTime currentDateTime, string username)
        {
            foreach (var matchedAffidavitDetail in matchedAffidavitDetails)
            {
                matchedAffidavitDetail.AffidavitDetail.AffidavitClientScrubs = matchedAffidavitDetail.ProposalDetailWeeks.Select(
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
                                PostingBookId = w.ProposalVersionDetailPostingBookId.Value,
                                PostingPlaybackType = w.ProposalVersionDetailPostingPlaybackType,
                                EffectiveIsci = matchedAffidavitDetail.EffectiveIsci,
                                EffectiveClientIsci = w.ClientIsci
                            }).ToList();
                matchedAffidavitDetail.AffidavitDetail.AffidavitFileDetailProblems = matchedAffidavitDetail.AffidavitDetailProblems;
            }
        }

        /// <summary>
        /// Maps an original isci to an effective isci
        /// </summary>
        /// <param name="mapIsciDto">MapIsciDto object containing the iscis to map</param>
        /// <param name="username">User requesting the mapping</param>
        /// <returns>The result of the mapping in true or false</returns>
        public bool MapIsci(MapIsciDto mapIsciDto, DateTime currentDateTime, string username)
        {
            _PostRepository.AddNewMapping(mapIsciDto, username);
            return ScrubUnlinkedAffidavitDetailsByIsci(mapIsciDto.OriginalIsci, currentDateTime, username);
        }

        /// <summary>
        /// Swaps one or more client scrubs to another proposal detail
        /// </summary>
        /// <param name="requestData">SwapProposalDetailRequest object containing a list of affidavit client scrubs to be swapped and the proposal detail id to swap to</param>
        /// <param name="currentDateTime">Current date and time</param>
        /// <param name="username">Username requesting the change</param>
        /// <returns>True or false</returns>
        public bool SwapProposalDetails(SwapProposalDetailRequest requestData, DateTime currentDateTime, string username)
        {
            var postingBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(currentDateTime);
            List<AffidavitFileDetail> affidavitDetails = _AffidavitRepository.GetAffidavitDetailsByClientScrubIds(requestData.AffidavitScrubbingIds);
            var matchedAffidavitDetails = _LinkAndValidateContractIscis(affidavitDetails, requestData.ProposalDetailId);
            _SetPostingBookData(matchedAffidavitDetails, postingBookId);

            //load AffidavitClientScrubs and AffidavitDetailProblems
            _MapToAffidavitFileDetails(matchedAffidavitDetails, currentDateTime, username);

            var affidavitFileDetails = matchedAffidavitDetails.Select(d => d.AffidavitDetail).ToList();
            _ScrubMatchedAffidavitRecords(affidavitFileDetails);
            _AffidavitImpressionsService.CalculateAffidavitImpressionsForAffidavitFileDetails(affidavitFileDetails);
            using (var transaction = new TransactionScopeWrapper()) //Ensure both database requests succeed or fail together
            {
                _AffidavitRepository.SaveScrubbedFileDetails(affidavitFileDetails);
                _ProposalRepository.UpdateProposalDetailPostingBooks(_GetProposalDetailIdsWithPostingBookId(affidavitFileDetails));
                transaction.Complete();
            }
            return true;
        }

        private List<ProposalDetailPostingData> _GetProposalDetailIdsWithPostingBookId(List<AffidavitFileDetail> affidavitFileDetails)
        {
            var result = affidavitFileDetails.SelectMany(d => d.AffidavitClientScrubs.Select(s => new ProposalDetailPostingData
            {
                ProposalVersionDetailId = s.ProposalVersionDetailId,
                PostingBookId = s.PostingBookId.Value,
                PostingPlaybackType = s.PostingPlaybackType.Value
            })).ToList();

            return result;
        }

        private void _SetPostingBookData(List<AffidavitMatchingDetail> matchedAffidavitDetails, int postingBookId, ProposalEnums.ProposalPlaybackType? playbackType = null)
        {
            foreach (var proposalDetailWeek in matchedAffidavitDetails.SelectMany(d => d.ProposalDetailWeeks))
            {
                if (!proposalDetailWeek.ProposalVersionDetailPostingBookId.HasValue)
                {
                    proposalDetailWeek.ProposalVersionDetailPostingBookId = postingBookId;
                }

                if (!proposalDetailWeek.ProposalVersionDetailPostingPlaybackType.HasValue)
                {
                    if (playbackType == null)
                    {
                        proposalDetailWeek.ProposalVersionDetailPostingPlaybackType = DefaultPlaybackType;
                    }
                    else
                    {
                        proposalDetailWeek.ProposalVersionDetailPostingPlaybackType = playbackType.Value;
                    }
                }
            }
        }

        private void _ScrubMatchedAffidavitRecords(List<AffidavitFileDetail> affidavitFileDetails)
        {
            var stations = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStations().ToDictionary(k => k.LegacyCallLetters, v => v);

            foreach (var affidavitDetail in affidavitFileDetails)
            {
                if (!affidavitDetail.AffidavitClientScrubs.Any())
                    continue;

                var quarterWeekIds =
                    affidavitDetail.AffidavitClientScrubs.Select(s => s.ProposalVersionDetailQuarterWeekId).ToList();
                var proposals = _ProposalService.GetProposalsByQuarterWeeks(quarterWeekIds);

                var affidavitStation = _MatchAffidavitStation(stations, affidavitDetail);

                foreach (AffidavitClientScrub scrub in affidavitDetail.AffidavitClientScrubs)
                {
                    var quarterWeekId = scrub.ProposalVersionDetailQuarterWeekId;
                    var proposal = proposals[quarterWeekId];
                    var proposalDetail = proposal.Details.Single(d =>
                        d.Quarters.Any(q => q.Weeks.Any(w => w.Id == quarterWeekId)));
                    var proposalWeek = proposalDetail.Quarters.SelectMany(d => d.Weeks.Where(w => w.Id == quarterWeekId)).First();
                    var proposalWeekIsci = proposalWeek.Iscis.Any(i => i.HouseIsci.Equals(affidavitDetail.Isci, StringComparison.InvariantCultureIgnoreCase))
                        ? proposalWeek.Iscis.First(i => i.HouseIsci.Equals(affidavitDetail.Isci, StringComparison.InvariantCultureIgnoreCase))
                        : !string.IsNullOrWhiteSpace(affidavitDetail.MappedIsci) ? proposalWeek.Iscis.First(i => i.HouseIsci.Equals(affidavitDetail.MappedIsci, StringComparison.InvariantCultureIgnoreCase))
                                : null;

                    if (proposalWeekIsci == null)
                    {
                        scrub.MatchIsci = false;
                    }
                    else
                    {
                        scrub.MatchIsciDays = _IsIsciDaysMatch(proposalWeekIsci, affidavitDetail.OriginalAirDate.DayOfWeek);
                        scrub.MatchIsci = true;
                        scrub.EffectiveClientIsci = proposalWeekIsci.ClientIsci;
                    }

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

                    scrub.Status = (scrub.MatchStation && scrub.MatchMarket && scrub.MatchGenre && scrub.MatchProgram && scrub.MatchTime && scrub.MatchIsciDays && scrub.MatchDate && scrub.MatchShowType && scrub.MatchIsci)
                        ? ScrubbingStatus.InSpec
                        : ScrubbingStatus.OutOfSpec;
                }
            }
        }

        private DisplayBroadcastStation _MatchAffidavitStation(Dictionary<string, DisplayBroadcastStation> stations, AffidavitFileDetail affidavitDetail)
        {
            string stationName = _StationProcessingEngine.StripStationSuffix(affidavitDetail.Station);

            if (stations.ContainsKey(stationName))
            {
                return stations[stationName];
            }

            return null;
        }

        public List<AffidavitFileProblem> _MapValidationErrorToAffidavitFileProblem(List<AffidavitValidationResult> affidavitValidationResults)
        {
            List<AffidavitFileProblem> problems = new List<AffidavitFileProblem>();

            affidavitValidationResults.ForEach(v =>
            {
                AffidavitFileProblem problem = new AffidavitFileProblem();
                var description = v.ErrorMessage;
                if (!string.IsNullOrEmpty(v.InvalidField))
                {
                    description = string.Format("Record: {0}: Field: '{1}' is invalid\r\n{2}", v.InvalidLine, v.InvalidField, v.ErrorMessage);
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

        private List<AffidavitMatchingDetail> _LinkAndValidateContractIscis(List<AffidavitFileDetail> affidavitDetails, int swapProposalDetailId = 0)
        {
            bool isIsciMapped = false;
            var matchedAffidavitDetails = new List<AffidavitMatchingDetail>();
            List<AffidavitMatchingProposalWeek> proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            bool swappingProposalDetail = swapProposalDetailId > 0;
            if (swappingProposalDetail)
            {
                proposalWeeks = _ProposalRepository.GetAffidavitMatchingProposalWeeksByDetailId(swapProposalDetailId);
                swappingProposalDetail = true;
            }
            foreach (var affidavitDetail in affidavitDetails)
            {
                if (!swappingProposalDetail)
                {
                    proposalWeeks = _ProposalRepository.GetAffidavitMatchingProposalWeeksByHouseIsci(affidavitDetail.Isci);
                }

                var matchedProposalWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);
                if (!matchedProposalWeeks.Any() && !string.IsNullOrWhiteSpace(affidavitDetail.MappedIsci))
                {
                    proposalWeeks = _ProposalRepository.GetAffidavitMatchingProposalWeeksByHouseIsci(affidavitDetail.MappedIsci);
                    matchedProposalWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);
                    isIsciMapped = true;
                }

                var matchingProblems = _AffidavitMatchingEngine.MatchingProblems();
                matchedAffidavitDetails.Add(new AffidavitMatchingDetail()
                {
                    AffidavitDetail = affidavitDetail,
                    ProposalDetailWeeks = matchedProposalWeeks,
                    AffidavitDetailProblems = matchingProblems,
                    EffectiveIsci = isIsciMapped ? affidavitDetail.MappedIsci : affidavitDetail.Isci
                });
            }

            return matchedAffidavitDetails;
        }

        private int _GetSpotlength(int spotLength)
        {

            if (!_SpotLengthsDict.ContainsKey(spotLength))
                throw new Exception(string.Format("Invalid spot length '{0}' found.", spotLength));

            return _SpotLengthsDict[spotLength];
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
            ,"Inventory Source"
            ,"Affiliate"
            ,"ShowType"
            ,"LeadInShowType"
            ,"LeadOutShowType"
        };

        public string JSONifyFile(Stream rawStream, string fileName, out AffidavitSaveRequest request)
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
                    detail.InventorySource = (AffidaviteFileSourceEnum)int.Parse(reader.GetCellValue("Inventory Source"));
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