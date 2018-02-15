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


    public interface IAffidavitService : IApplicationService
    {
        int SaveAffidavit(AffidavitSaveRequest saveRequest, string username, DateTime currentDateTime);

        ///
        /// Scrubs, but does not save results
        ///
        void ScrubAffidavitFile(affidavit_files affidavit_file);

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
        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IAffidavitMatchingEngine affidavitMatchingEngine,
            IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalService proposalService,
            IPostingBooksService postingBooksService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AffidavitMatchingEngine = affidavitMatchingEngine;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalService = proposalService;
            _PostingBooksService = postingBooksService;
            _AffidavitRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
        }

        public int SaveAffidavit(AffidavitSaveRequest saveRequest, string username, DateTime currentDateTime)
        {
            affidavit_files affidavit_file = new affidavit_files();
            Dictionary<int, int> spotLengthDict = null;
            if (saveRequest == null)
            {
                throw new Exception("No affidavit data received.");
            }

            affidavit_file = new affidavit_files();

            affidavit_file.created_date = DateTime.Now;
            affidavit_file.file_hash = saveRequest.FileHash;
            affidavit_file.file_name = saveRequest.FileName;
            affidavit_file.source_id = saveRequest.Source;

            var matchedAffidavitDetails = _LinkAndValidateContractIscis(saveRequest);

            foreach (var matchedAffidavitDetail in matchedAffidavitDetails)
            {
                var det = new affidavit_file_details();
                det.air_time = Convert.ToInt32(matchedAffidavitDetail.AffidavitDetail.AirTime.TimeOfDay.TotalSeconds);
                det.original_air_date = matchedAffidavitDetail.AffidavitDetail.AirTime;
                det.isci = matchedAffidavitDetail.AffidavitDetail.Isci;
                det.program_name = matchedAffidavitDetail.AffidavitDetail.ProgramName;
                det.genre = matchedAffidavitDetail.AffidavitDetail.Genre;
                det.spot_length_id = _GetSpotlength(matchedAffidavitDetail.AffidavitDetail.SpotLength, ref spotLengthDict);
                det.station = matchedAffidavitDetail.AffidavitDetail.Station;
                det.affidavit_client_scrubs =
                    matchedAffidavitDetail.ProposalDetailWeeks.Select(
                        w => new affidavit_client_scrubs
                        {
                            proposal_version_detail_quarter_week_id = w.ProposalVersionDetailQuarterWeekId,
                            match_time = w.AirtimeMatch,
                            modified_by = username,
                            modified_date = currentDateTime,
                            lead_in = w.IsLeadInMatch,
                            status = _GetScrubStatus(w)
                        }).ToList();

                affidavit_file.affidavit_file_details.Add(det);
            }

            ScrubAffidavitFile(affidavit_file);

            var postingBookId = _GetPostingBookId();
            _CalculateAffidavitImpressions(affidavit_file, postingBookId);

            var id = _AffidavitRepository.SaveAffidavitFile(affidavit_file);

            return id;

        }

        public void ScrubAffidavitFile(affidavit_files affidavit_file)
        {
            var callLetters = affidavit_file.affidavit_file_details.Select(a => a.station).ToList();
            var stations = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>()
                .GetBroadcastStationListByLegacyCallLetters(callLetters).ToDictionary(k => k.LegacyCallLetters, v => v);

            foreach (var affidavitFileDetail in affidavit_file.affidavit_file_details)
            {
                if (!stations.ContainsKey(affidavitFileDetail.station))
                {
                    affidavitFileDetail.affidavit_client_scrubs.ForEach(s =>
                    {
                        s.status = (int) ScrubbingStatus.OutOfSpec;
                        s.match_station = false;
                    });
                    continue;
                }
                var quarterWeekIds =
                    affidavitFileDetail.affidavit_client_scrubs.Select(s => s.proposal_version_detail_quarter_week_id).ToList();
                var stationManifests = _BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalOpenMarketInventoryRepository>()
                    .GetStationManifestFromQuarterWeeks(quarterWeekIds);
                var proposals = _ProposalService.GetProposalsByQuarterWeeks(quarterWeekIds);

                var affidavitStation = stations[affidavitFileDetail.station];
                foreach (var scrub in affidavitFileDetail.affidavit_client_scrubs)
                {
                    scrub.match_station = false;
                    scrub.match_market = false;

                    var quarterWeekId = scrub.proposal_version_detail_quarter_week_id;
                    var proposal = proposals[quarterWeekId];
                    var proposalDetail = proposal.Details.Single(d =>
                        d.Quarters.Any(q => q.Weeks.Any(w => w.Id == quarterWeekId)));

                    // match market/station
                    if (stationManifests.Any())
                    {   
                        var scrubManifests = stationManifests[quarterWeekId];

                        if (scrubManifests.Any(m =>
                            m.station.legacy_call_letters == affidavitStation.LegacyCallLetters))
                        {
                            scrub.match_station = true;
                            var markets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposal, proposalDetail);

                            var marketGeoName = affidavitStation.OriginMarket;
                            if (markets.Any(m => m.Display == marketGeoName))
                            {
                                affidavitFileDetail.market = marketGeoName;

                                scrub.match_market = true;
                            }
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

                    EnsureScrubadubdubed(scrub);
                }

            }

        }

        private void EnsureScrubadubdubed(affidavit_client_scrubs scrub)
        {
            scrub.status = (int)ScrubbingStatus.OutOfSpec;
            if (scrub.match_station
                && scrub.match_market
                && scrub.match_genre
                && scrub.match_program
                && scrub.match_time)
            {
                scrub.status = (int) ScrubbingStatus.InSpec;
            }
        }
        private int _GetScrubStatus(AffidavitMatchingProposalWeek affidavitMatchingProposalWeek)
        {
                if (!affidavitMatchingProposalWeek.AirtimeMatch)
                    return (int) AffidavitClientScrubStatus.OutOfSpec;
            
                return (int) AffidavitClientScrubStatus.InSpec;
        }

        private List<AffidavitMatchingDetail> _LinkAndValidateContractIscis(AffidavitSaveRequest saveRequest)
        {
            var matchedAffidavitDetails = new List<AffidavitMatchingDetail>();

            foreach (var requestDetail in saveRequest.Details)
            {
                var proposalWeeks =
                    _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                        .GetAffidavitMatchingProposalWeeksByHouseIsci(requestDetail.Isci);

                var matchedProposalWeeks = _AffidavitMatchingEngine.Match(requestDetail, proposalWeeks);
                var matchingProblems = _AffidavitMatchingEngine.MatchingProblems();
                matchedAffidavitDetails.Add(new AffidavitMatchingDetail()
                {
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
                reader.NextRow();
                while (!reader.IsEOFOrEmptyRow())
                {
                    var detail = new AffidavitSaveRequestDetail();
                    
                    detail.AirTime = DateTime.Parse(reader.GetCellValue("Spot Time"));
                    detail.Genre = reader.GetCellValue("Genre");
                    detail.Isci = reader.GetCellValue("ISCI");
                    detail.ProgramName = reader.GetCellValue("ProgramName");
                    detail.Station = reader.GetCellValue("Station");
                    detail.SpotLength = int.Parse(reader.GetCellValue("Spot Length"));
                    detail.LeadInTitle = reader.GetCellValue("LeadInTitle");
                    detail.LeadInGenre = reader.GetCellValue("LeadInGenre");
                    detail.LeadOutTitle = reader.GetCellValue("LeadOutTitle");
                    detail.LeadOutGenre = reader.GetCellValue("LeadOutGenre");

                    request.Details.Add(detail);
                    reader.NextRow();
                }
            }
            var json = JsonConvert.SerializeObject(request);
            return json;
        }
        #endregion
    }
}