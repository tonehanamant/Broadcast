using System.Collections.Generic;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Data.Entity;
using System.Linq;
using Common.Services.Extensions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IAffidavitRepository : IDataRepository
    {
        int SaveAffidavitFile(affidavit_files affidatite_file);
        AffidavitFile GetAffidavit(int affidavitId, bool includeScrubbingDetail = false);
        List<ProposalDetailPostScrubbingDto> GetProposalDetailPostScrubbing(int proposalVersionId);
    }

    public class AffidavitRepository: BroadcastRepositoryBase, IAffidavitRepository
    {

        public AffidavitRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int SaveAffidavitFile(affidavit_files affidatite_file)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.affidavit_files.Add(affidatite_file);
                    context.SaveChanges();
                });
            return affidatite_file.id;
        }

        public AffidavitFile GetAffidavit(int affidavitId,bool includeScrubbingDetail = false)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = context.affidavit_files
                        .Include(a => a.affidavit_file_details)
                        .Include(a => a.affidavit_file_details.Select(d => d.affidavit_file_detail_audiences));

                    if (includeScrubbingDetail)
                        query.Include(a => a.affidavit_file_details.Select(d => d.affidavit_client_scrubs));

                    var affidavitFile = query.Single(a => a.id == affidavitId, "Affidavit/Post not found in database");

                    return _MapToAffidavitFile(affidavitFile);
                });
        }

        private AffidavitFile _MapToAffidavitFile(affidavit_files affidavitFile)
        {
            return new AffidavitFile
            {
                Id = affidavitFile.id,
                FileName = affidavitFile.file_name,
                FileHash = affidavitFile.file_hash,
                SourceId = affidavitFile.source_id,
                CreatedDate = affidavitFile.created_date,
                MediaMonthId = affidavitFile.media_month_id,
                AffidavitFileDetails = affidavitFile.affidavit_file_details.Select(d => new AffidavitFileDetail
                {
                    Id = d.id,
                    AffidavitFileId = d.affidavit_file_id,
                    Station = d.station,
                    OriginalAirDate = d.original_air_date,
                    AdjustedAirDate = d.adjusted_air_date,
                    AirTime = d.air_time,
                    SpotLengthId = d.spot_length_id,
                    Isci = d.isci,
                    ProgramName = d.program_name,
                    Genre = d.genre,
                    LeadinGenre = d.leadin_genre,
                    LeadinProgramName = d.leadin_program_name,
                    LeadoutGenre = d.leadout_genre,
                    LeadoutProgramName = d.leadin_program_name,
                    Market = d.market,
                    AffidavitClientScrubs = d.affidavit_client_scrubs.Select(a => new AffidavitClientScrub
                    {
                        Id = a.id,
                        AffidavitFileDetailId = a.affidavit_file_detail_id,
                        ProposalVersionDetailQuarterWeekId = a.proposal_version_detail_quarter_week_id,
                        MatchProgram = a.match_program,
                        MatchGenre = a.match_genre,
                        MatchMarket = a.match_market,
                        MatchTime = a.match_time,
                        Status = (AffidavitClientScrubStatus)a.status,
                        Comment = a.comment,
                        ModifiedBy = a.modified_by,
                        ModifiedDate = a.modified_date,
                        LeadIn = a.lead_in
                    }).ToList(),
                    AffidavitFileDetailAudiences = d.affidavit_file_detail_audiences.Select(a => new AffidavitFileDetailAudience
                    {
                        AffidavitFileDetailId = a.affidavit_file_detail_id,
                        AudienceId = a.audience_id,
                        Impressions = a.impressions
                    }).ToList()
                }).ToList()
            };
        }

        public List<ProposalDetailPostScrubbingDto> GetProposalDetailPostScrubbing(int proposalVersionId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {                    
                    var affidavitFiles = (from proposalVersionDetail in context.proposal_version_details
                                          from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                          from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                          from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                          let affidavitFile = affidavitFileScrub.affidavit_file_details
                                          where proposalVersionDetail.id == proposalVersionId
                                          select new { affidavitFile, affidavitFileScrub }).ToList();
                    var spotLengths = (from sl in context.spot_lengths select sl).ToList();

                    var posts = new List<ProposalDetailPostScrubbingDto>();
                    posts.AddRange(affidavitFiles.Select(x => new ProposalDetailPostScrubbingDto()
                    {
                        Station = x.affidavitFile.station,
                        ISCI = x.affidavitFile.isci,
                        ProgramName = x.affidavitFile.program_name,
                        Market = x.affidavitFile.market,
                        Affiliate = (from station in context.stations
                                     where station.legacy_call_letters.Equals(x.affidavitFile.station)
                                     select station.affiliation).Single(),
                        SpotLength = spotLengths.Single(y => y.id == x.affidavitFile.spot_length_id).length,
                        TimeAired = x.affidavitFile.original_air_date.AddSeconds(x.affidavitFile.air_time),
                        GenreName = x.affidavitFile.genre,
                        MatchGenre = x.affidavitFileScrub.match_genre,
                        MatchMarket = x.affidavitFileScrub.match_market,
                        MatchProgram = x.affidavitFileScrub.match_program,
                        MatchStation = x.affidavitFileScrub.match_station,
                        MatchTime = x.affidavitFileScrub.match_time
                    }).ToList());
                    return posts;
                });
        }
    }
}
