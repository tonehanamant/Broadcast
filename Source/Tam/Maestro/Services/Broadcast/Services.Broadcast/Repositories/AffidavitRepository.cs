using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IAffidavitRepository : IDataRepository
    {
        int SaveAffidavitFile(affidavit_files affidatite_file);
        AffidavitFile GetAffidavit(int affidavit_id);
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

        public AffidavitFile GetAffidavit(int affidavitId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var affidavitFile = context.affidavit_files
                        .Include(a => a.affidavit_file_details)
                        .Include(a => a.affidavit_file_details.Select(d => d.affidavit_file_detail_audiences))
                        .Single(a => a.id == affidavitId);

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
                            AffidavitClientScrubs = d.affidavit_client_scrubs.Select(a => new AffidavitClientScrub
                            {
                                Id = a.id,
                                AffidavitFileDetailId = a.affidavit_file_detail_id,
                                ProposalVersionDetailQuarterWeekId = a.proposal_version_detail_quarter_week_id,
                                MatchProgram =  a.match_program,
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
                });
        }       
    }
}
