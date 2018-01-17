using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common;

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
        int SaveAffidavit(AffidavitSaveRequest saveRequest, bool scrub = false);

        ///
        /// Scrubs, but does not save results
        ///
        void ScrubAffidavitFile(affidavit_files affidavit_file);

    }
        
    public class AffidavitService : IAffidavitService
    {
        protected readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        protected readonly IProposalService _ProposalService;

        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory,IProposalService proposalService)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _ProposalService = proposalService;
        }

        public int SaveAffidavit(AffidavitSaveRequest saveRequest,bool scrub = false)
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

            foreach (var detail in saveRequest.Details)
            {
                var det = new affidavit_file_details();
                det.air_time = Convert.ToInt32(detail.AirTime.TimeOfDay.TotalSeconds);
                det.original_air_date = detail.AirTime;
                det.isci = detail.Isci;
                det.program_name = detail.ProgramName;
                det.spot_length_id = GetSpotlength(detail.SpotLength,ref spotLengthDict);
                det.station = detail.Station;

                affidavit_file.affidavit_file_details.Add(det);
            }
            var repo = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();

            if (scrub)
                ScrubAffidavitFile(affidavit_file);

            int id = repo.SaveAffidavitFile(affidavit_file);

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
                var proposals = _BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>().GetProposalsByQuarterWeeks(quarterWeekIds);
                

                foreach (var scrub in affidavitFileDetail.affidavit_client_scrubs)
                {
                    var manifest = stationManifests[scrub.proposal_version_detail_quarter_week_id];

                    scrub.match_station = true;
                    scrub.match_market = true;

                    if (!stations.ContainsKey(manifest.station.legacy_call_letters))
                    {
                        scrub.match_station = false;
                    }
                    var station = stations[manifest.station.legacy_call_letters];
                    var marketGeoName = station.OriginMarket; 
                    var proposal = proposals[scrub.proposal_version_detail_quarter_week_id];
                    if (proposal.Markets.All(m => m.Display != marketGeoName))
                    {
                        scrub.match_market = false;
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
        private int GetSpotlength(int spotLength, ref Dictionary<int, int> spotLengthDict)
        {
            if (spotLengthDict == null)
                spotLengthDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();

            if (!spotLengthDict.ContainsKey(spotLength))
                throw new Exception(string.Format("Invalid spot length '{0}' found.", spotLength));

            return spotLengthDict[spotLength];
        }

    }
}