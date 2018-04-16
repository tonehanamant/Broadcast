using Common.Services.ApplicationServices;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
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

    public enum ScrubbingStatus
    {
        OutOfSpec = 0,
        InSpec = 1
    }

    public class AffidavitSaveResult
    {
        public int? ID { get; set; }
        public List<AffidavitValidationResult> ValidationResults { get; set; }

        //public override string ToString()
        //{
        //    string str = "";

        //    if (ID.HasValue) str += "ID=" + ID.Value;
        //    if (ValidationResults.Any())
        //    {
        //        if (str.Length > 0) str += "\r\n";
        //        str += "Validation Results\r\n";
        //        ValidationResults.ForEach(r => { str += r.ToString() + "\r\n"; });
        //    }

        //    if (!string.IsNullOrEmpty(str)) str += "\r\n";
        //    str += GetType().FullName;
        //    return str;
        //}
    }
    public interface IAffidavitService : IApplicationService
    {
        AffidavitSaveResult SaveAffidavit(AffidavitSaveRequest saveRequest, string username,DateTime currentDateTime);

        ///
        /// Scrubs, but does not save results
        ///
        void ScrubAffidavitFile(affidavit_files affidavit_file,int marketBookingId);

        string JSONifyFile(Stream rawStream,string fileName,out AffidavitSaveRequest request);
    }

    public class AffidavitService : IAffidavitService
    {
        private const ProposalEnums.ProposalPlaybackType DefaultPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3;
        private readonly IAffidavitMatchingEngine _AffidavitMatchingEngine;
        protected readonly IProposalService _ProposalService;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IPostingBooksService _PostingBooksService;
        private readonly IAffidavitRepository _AffidavitRepository;
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly IAffidavitValidationEngine _AffidavitValidationEngine;

        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IAffidavitMatchingEngine affidavitMatchingEngine,
            IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalService proposalService,
            IPostingBooksService postingBooksService,
            IAffidavitValidationEngine affidavitValidationEngine)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitMatchingEngine = affidavitMatchingEngine;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalService = proposalService;
            _PostingBooksService = postingBooksService;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            _AffidavitValidationEngine = affidavitValidationEngine;
        }

        public AffidavitSaveResult SaveAffidavit(AffidavitSaveRequest saveRequest, string username, DateTime currentDateTime)
        {
            affidavit_files affidavit_file = new affidavit_files();
            Dictionary<int, int> spotLengthDict = null;
            if (saveRequest == null)
            {
                throw new Exception("No affidavit data received.");
            }

            affidavit_file = new affidavit_files
            {
                created_date = currentDateTime,
                file_hash = saveRequest.FileHash,
                file_name = saveRequest.FileName,
                source_id = saveRequest.Source
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

                var det = new affidavit_file_details
                {
                    air_time = Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.AirTime.TimeOfDay.TotalSeconds),
                    original_air_date = matchedAffidavitDetail.AffidavitDetail.AirTime,
                    isci = matchedAffidavitDetail.AffidavitDetail.Isci,
                    program_name = matchedAffidavitDetail.AffidavitDetail.ProgramName,
                    genre = matchedAffidavitDetail.AffidavitDetail.Genre,
                    spot_length_id = _GetSpotlength(matchedAffidavitDetail.AffidavitDetail.SpotLength, ref spotLengthDict),
                    station = matchedAffidavitDetail.AffidavitDetail.Station,
                    market = matchedAffidavitDetail.AffidavitDetail.Market,
                    affiliate = matchedAffidavitDetail.AffidavitDetail.Affiliate,
                    estimate_id = matchedAffidavitDetail.AffidavitDetail.EstimateId,
                    inventory_source = matchedAffidavitDetail.AffidavitDetail.InventorySource,
                    spot_cost = matchedAffidavitDetail.AffidavitDetail.SpotCost,
                    leadin_genre = matchedAffidavitDetail.AffidavitDetail.LeadInGenre,
                    leadout_genre = matchedAffidavitDetail.AffidavitDetail.LeadOutGenre,
                    leadin_program_name = matchedAffidavitDetail.AffidavitDetail.LeadInProgramName,
                    leadout_program_name = matchedAffidavitDetail.AffidavitDetail.LeadOutProgramName,
                    leadin_end_time = Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.LeadInEndTime.TimeOfDay.TotalSeconds),
                    leadout_start_time = Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.LeadOutStartTime.TimeOfDay.TotalSeconds),
                    program_show_type = matchedAffidavitDetail.AffidavitDetail.ProgramShowType,
                    leadin_show_type = matchedAffidavitDetail.AffidavitDetail.LeadInShowType,
                    leadout_show_type = matchedAffidavitDetail.AffidavitDetail.LeadOutShowType,

                    affidavit_client_scrubs =
                    matchedAffidavitDetail.ProposalDetailWeeks.Select(
                        w => new affidavit_client_scrubs
                        {
                            proposal_version_detail_quarter_week_id = w.ProposalVersionDetailQuarterWeekId,
                            match_time = w.TimeMatch,
                            match_date = w.DateMatch,
                            modified_by = username,
                            modified_date = currentDateTime,
                            effective_program_name = matchedAffidavitDetail.AffidavitDetail.ProgramName,
                            effective_genre = matchedAffidavitDetail.AffidavitDetail.Genre,
                            effective_show_type = matchedAffidavitDetail.AffidavitDetail.ProgramShowType,
                            lead_in = w.IsLeadInMatch
                        }).ToList(),
                    affidavit_file_detail_problems =
                    matchedAffidavitDetail.AffidavitDetailProblems.Select(
                        fp => new affidavit_file_detail_problems
                        {
                            problem_description = fp.Description,
                            problem_type = (int)fp.Type
                        }).ToList(),
                    affidavit_file_detail_demographics = matchedAffidavitDetail.AffidavitDetail.Demographics.Select(
                        x=>new affidavit_file_detail_demographics()
                        {
                            audience_id = x.AudienceId,
                            overnight_impressions = x.OvernightImpressions,
                            overnight_rating = x.OvernightRating
                        }).ToList()
                };

                affidavit_file.affidavit_file_details.Add(det);
            }

            var result = new AffidavitSaveResult();
            result.ValidationResults = affidavitValidationResults;

            if (affidavitValidationResults.Any())
            {
                return result;
            }

            var postingBookId = _GetPostingBookId();
            ScrubAffidavitFile(affidavit_file,postingBookId);

            _CalculateAffidavitImpressions(affidavit_file, postingBookId);

            var id = _AffidavitRepository.SaveAffidavitFile(affidavit_file);

            result.ID = id;
            return result;
        }

        public void ScrubAffidavitFile(affidavit_files affidavit_file,int marketBookingId)
        {
            var callLetters = affidavit_file.affidavit_file_details.Select(a => a.station).ToList();
            var stations = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStationListByLegacyCallLetters(callLetters).ToDictionary(k => k.LegacyCallLetters, v => v);

            foreach (var affidavitFileDetail in affidavit_file.affidavit_file_details)
            {
                if (!affidavitFileDetail.affidavit_client_scrubs.Any())
                    continue;

                var quarterWeekIds =
                    affidavitFileDetail.affidavit_client_scrubs.Select(s => s.proposal_version_detail_quarter_week_id).ToList();
                var proposals = _ProposalService.GetProposalsByQuarterWeeks(quarterWeekIds);

                DisplayBroadcastStation affidavitStation = null;
                if (stations.ContainsKey(affidavitFileDetail.station))
                {
                    affidavitStation = stations[affidavitFileDetail.station];
                }

                foreach (var scrub in affidavitFileDetail.affidavit_client_scrubs)
                {
                    var quarterWeekId = scrub.proposal_version_detail_quarter_week_id;
                    var proposal = proposals[quarterWeekId];
                    var proposalDetail = proposal.Details.Single(d =>
                        d.Quarters.Any(q => q.Weeks.Any(w => w.Id == quarterWeekId)));
                    var proposalWeek = proposalDetail.Quarters.SelectMany(d => d.Weeks.Where(w => w.Id == quarterWeekId)).First();
                    var proposalWeekIsci = proposalWeek.Iscis.First(i =>
                        i.HouseIsci.Equals(affidavitFileDetail.isci, StringComparison.InvariantCultureIgnoreCase));
                    var dayOfWeek = affidavitFileDetail.original_air_date.DayOfWeek;

                    scrub.match_isci_days = _IsIsciDaysMatch(proposalWeekIsci, dayOfWeek);

                    // match market/station
                    scrub.match_station = false;
                    scrub.match_market = false;

                    if (affidavitStation != null)
                    {
                        var markets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposal, marketBookingId);

                        var marketGeoName = affidavitStation.OriginMarket;
                        if (markets.Any(m => m.Display == marketGeoName))
                        {
                            affidavitFileDetail.market = marketGeoName;
                            scrub.match_station = true;
                            scrub.match_market = true;
                        }
                    }

                    scrub.match_program = true;
                    if (proposalDetail.ProgramCriteria.Any())
                    {
                        var progCriteria = proposalDetail.ProgramCriteria.SingleOrDefault(pc =>
                                                pc.Program.Display == affidavitFileDetail.program_name);

                        if (progCriteria != null)
                            scrub.match_program = progCriteria.Contain == ContainTypeEnum.Include;
                    }

                    scrub.match_genre = true;
                    if (proposalDetail.GenreCriteria.Any())
                    {
                        var genreCriteria = proposalDetail.GenreCriteria.SingleOrDefault(pc =>
                            pc.Genre.Display == affidavitFileDetail.genre);

                        if (genreCriteria != null)
                            scrub.match_genre = genreCriteria.Contain == ContainTypeEnum.Include;
                    }

                    scrub.status = (scrub.match_station && scrub.match_market && scrub.match_genre && scrub.match_program && scrub.match_time && scrub.match_isci_days && scrub.match_date) 
                        ? (int)ScrubbingStatus.InSpec 
                        : (int)ScrubbingStatus.OutOfSpec;
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

        private void _CalculateAffidavitImpressions(affidavit_files affidavitFile, int postingBookId)
        {
            var details = affidavitFile.affidavit_file_details;
           
            var audiencesRepository = _BroadcastDataRepositoryFactory.GetDataRepository<INsiComponentAudienceRepository>();
            var audiencesIds =
                audiencesRepository.GetAllNsiComponentAudiences().
                Select(a => a.Id).
                ToList();

            foreach (var affidavitFileDetail in details)
            {
                affidavitFileDetail.affidavit_file_detail_audiences =
                    _CalculdateImpressionsForNielsenAudiences(affidavitFileDetail, audiencesIds, postingBookId);
            }
        }

        private List<affidavit_file_detail_audiences> _CalculdateImpressionsForNielsenAudiences(affidavit_file_details affidavitFileDetail, List<int> audiencesIds, int postingBookId)
        {
            var affidavitAudiences = new List<affidavit_file_detail_audiences>();
            var stationDetails = new List<StationDetailPointInTime>
            {
                new StationDetailPointInTime
                {
                    LegacyCallLetters = affidavitFileDetail.station,
                    DayOfWeek = affidavitFileDetail.original_air_date.DayOfWeek,
                    TimeAired = affidavitFileDetail.air_time
                }
            };

            var ratingForecastRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            var impressionsPointInTime = ratingForecastRepository.GetImpressionsPointInTime(postingBookId, audiencesIds, stationDetails,
                DefaultPlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);

            foreach (var impressionsWithAudience in impressionsPointInTime)
            {
                affidavitAudiences.Add(new affidavit_file_detail_audiences
                {
                    affidavit_file_detail_id = affidavitFileDetail.id,
                    audience_id = impressionsWithAudience.audience_id,
                    impressions = impressionsWithAudience.impressions
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
            //if (fileName.EndsWith("xlsx"))
            //    reader = new ExcelFileReader(FileHeaders);
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

                    request.Details.Add(detail);
                }
            }
            var json = JsonConvert.SerializeObject(request);
            return json;
        }
        #endregion
    }
}