using System.Collections.Generic;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Data.Entity;
using System.Linq;
using Common.Services.Extensions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.Repositories
{
    public interface IAffidavitRepository : IDataRepository
    {
        int SaveAffidavitFile(affidavit_files affidatite_file);
        AffidavitFile GetAffidavit(int affidavitId, bool includeScrubbingDetail = false);
        List<ProposalDetailPostScrubbingDto> GetProposalDetailPostScrubbing(int proposalVersionId);

        /// <summary>
        /// Gets the data for the NSI Post Report
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <returns>List of NSIPostReportDto objects</returns>
        List<InSpecAffidavitFileDetail> GetInSpecSpotsForProposal(int proposalId);
    }

    public class AffidavitRepository : BroadcastRepositoryBase, IAffidavitRepository
    {

        public AffidavitRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int SaveAffidavitFile(affidavit_files affidavit_file)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.affidavit_files.Add(affidavit_file);
                    context.SaveChanges();
                });
            return affidavit_file.id;
        }

        public AffidavitFile GetAffidavit(int affidavitId, bool includeScrubbingDetail = false)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = context.affidavit_files
                        .Include(a => a.affidavit_file_details)
                        .Include(a => a.affidavit_file_details.Select(d => d.affidavit_file_detail_audiences));

                    if (includeScrubbingDetail)
                    {
                        query.Include(a => a.affidavit_file_details.Select(d => d.affidavit_client_scrubs));
                        query.Include(a => a.affidavit_file_details.Select(d => d.affidavit_file_detail_demographics));
                    }

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
                    LeadoutProgramName = d.leadout_program_name,
                    ProgramShowType = d.program_show_type,
                    LeadInShowType = d.leadin_show_type,
                    LeadOutShowType = d.leadout_show_type,
                    LeadOutStartTime = d.leadout_start_time,
                    LeadInEndTime = d.leadin_end_time,
                    Market = d.market,
                    Affiliate = d.affiliate,
                    EstimateId = d.estimate_id.Value,
                    InventorySource = d.inventory_source.Value,
                    SpotCost = d.spot_cost.Value,
                    Demographics = d.affidavit_file_detail_demographics.Select(a => new Demographics()
                    {
                        AudienceId = a.audience_id.Value,
                        OvernightImpressions = a.overnight_impressions.Value,
                        OvernightRating = a.overnight_rating.Value
                    }).ToList(),
                    AffidavitClientScrubs = d.affidavit_client_scrubs.Select(a => new AffidavitClientScrub
                    {
                        Id = a.id,
                        AffidavitFileDetailId = a.affidavit_file_detail_id,
                        ProposalVersionDetailQuarterWeekId = a.proposal_version_detail_quarter_week_id,
                        MatchProgram = a.match_program,
                        MatchGenre = a.match_genre,
                        MatchMarket = a.match_market,
                        MatchTime = a.match_time,
                        MatchDate = a.match_date,
                        MatchIsciDays = a.match_isci_days,
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

        public List<ProposalDetailPostScrubbingDto> GetProposalDetailPostScrubbing(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                    var affidavitFiles = (from proposalVersionDetail in context.proposal_version_details
                                          from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                          from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                          from proposalVersionWeekIscis in proposalVersionWeeks.proposal_version_detail_quarter_week_iscis
                                          from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                          let affidavitFile = affidavitFileScrub.affidavit_file_details
                                          where proposalVersionWeekIscis.house_isci == affidavitFile.isci && proposalVersionDetail.id == proposalDetailId
                                          select new { affidavitFile, affidavitFileScrub, proposalVersionWeekIscis }).ToList();
                    var spotLengths = (from sl in context.spot_lengths select sl).ToList();

                    var posts = new List<ProposalDetailPostScrubbingDto>();
                    posts.AddRange(affidavitFiles.Select(x =>
                    {
                        var marketStation = (from stations in context.stations
                                             where stations.legacy_call_letters.Equals(x.affidavitFile.station)
                                             select new { markets = stations.market, stations }).SingleOrDefault();
                        return new ProposalDetailPostScrubbingDto()
                        {
                            Station = x.affidavitFile.station,
                            ISCI = x.affidavitFile.isci,
                            ProgramName = x.affidavitFile.program_name,
                            Market = marketStation?.markets.geography_name,
                            Affiliate = marketStation?.stations.affiliation,
                            SpotLength = spotLengths.Single(y => y.id == x.affidavitFile.spot_length_id).length,
                            TimeAired = x.affidavitFile.original_air_date.AddSeconds(x.affidavitFile.air_time),
                            DayOfWeek = x.affidavitFile.original_air_date.DayOfWeek,
                            GenreName = x.affidavitFile.genre,
                            MatchGenre = x.affidavitFileScrub.match_genre,
                            MatchMarket = x.affidavitFileScrub.match_market,
                            MatchProgram = x.affidavitFileScrub.match_program,
                            MatchStation = x.affidavitFileScrub.match_station,
                            MatchTime = x.affidavitFileScrub.match_time,
                            MatchDate = x.affidavitFileScrub.match_date,
                            MatchISCI = x.affidavitFileScrub.match_isci_days,
                            Comments = x.affidavitFileScrub.comment,
                            ClientISCI = x.proposalVersionWeekIscis.client_isci,
                            WeekStart = x.proposalVersionWeekIscis.proposal_version_detail_quarter_weeks.start_date
                        };
                    }
                    ).ToList());
                    return posts;
                });
        }

        /// <summary>
        /// Gets the data for the NSI Post Report
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <returns>List of NSIPostReportDto objects</returns>
        public List<InSpecAffidavitFileDetail> GetInSpecSpotsForProposal(int proposalId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var inSpecDetails = (from proposal in context.proposals
                                         from proposalVersion in proposal.proposal_versions
                                         from proposalVersionDetail in proposalVersion.proposal_version_details
                                         from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                         from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                         from affidavitClientScrub in proposalVersionWeeks.affidavit_client_scrubs
                                         let affidavitFileDetails = affidavitClientScrub.affidavit_file_details
                                         where proposal.id == proposalId && affidavitClientScrub.status == (int)ScrubbingStatus.InSpec
                                         select new { affidavitFileDetails, proposalVersionQuarters, proposalVersionDetail, proposalVersion, proposal, proposalVersionWeeks })
                                         .ToList();

                    var inSpecAffidavitFileDetails = inSpecDetails.Select(x => new InSpecAffidavitFileDetail()
                    {
                        Station = x.affidavitFileDetails.station,
                        Isci = x.affidavitFileDetails.isci,
                        ProgramName = x.affidavitFileDetails.program_name,
                        SpotLengthId = x.affidavitFileDetails.spot_length_id,
                        AirTime = x.affidavitFileDetails.air_time,
                        AirDate = x.affidavitFileDetails.original_air_date,
                        DaypartName = x.proposalVersionDetail.daypart_code,
                        AudienceImpressions = x.affidavitFileDetails.affidavit_file_detail_audiences
                                                .ToDictionary(i => i.audience_id, j => j.impressions),
                        Quarter = x.proposalVersionQuarters.quarter,
                        Year = x.proposalVersionQuarters.year,
                        AdvertiserId = x.proposal.advertiser_id,
                        ProposalWeekCost = x.proposalVersionWeeks.cost,
                        ProposalWeekImpressionsGoal = x.proposalVersionWeeks.impressions_goal,
                        Units = x.proposalVersionWeeks.units
                    }).ToList();

                    return inSpecAffidavitFileDetails;
                });
        }
    }
}
