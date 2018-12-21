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
using System;
using Tam.Maestro.Common;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;

namespace Services.Broadcast.Repositories
{
    public interface IAffidavitRepository : IDataRepository
    {
        int SaveAffidavitFile(ScrubbingFile affidavitFile);
        ScrubbingFile GetAffidavit(int affidavitId, bool includeScrubbingDetail = false);
        List<ProposalDetailPostScrubbing> GetProposalDetailPostScrubbing(int proposalId, ScrubbingStatus? status);

        /// <summary>
        /// Gets the data for the NSI Post Report
        /// </summary>
        /// <param name="proposalId">Proposal Id to get the data for</param>
        /// <returns>List of NSIPostReportDto objects</returns>
        List<InSpecAffidavitFileDetail> GetInSpecSpotsForProposal(int proposalId);

        /// <summary>
        /// Persists a List of OutboundAffidavitFileValidationResultDto objects
        /// </summary>
        /// <param name="model">List of OutboundAffidavitFileValidationResultDto objects to be saved</param>
        void SaveValidationObject(List<WWTVOutboundFileValidationResult> model);
        List<MyEventsReportData> GetMyEventsReportData(int proposalId);
        void OverrideScrubStatus(List<int> ClientScrubIds, ScrubbingStatus overrideToStatus);
        List<ScrubbingFileDetail> GetAffidavitDetails(int proposalDetailId);
        void RemoveAffidavitAudiences(List<ScrubbingFileDetail> affidavitFileDetails);
        void SaveAffidavitAudiences(List<ScrubbingFileDetail> affidavitFileDetails);
        List<ScrubbingFileDetail> GetUnlinkedAffidavitDetailsByIsci(string isci);
        void SaveScrubbedFileDetails(List<ScrubbingFileDetail> affidavitFileDetails);

        /// <summary>
        /// Undo the override status
        /// </summary>
        /// <param name="scrubIds">List of affidavit client scrubs to undo the override status</param>
        void SaveScrubsStatus(List<affidavit_client_scrubs> scrubs);

        /// <summary>
        /// Gets all the affidavit details based on affidavit client scubs ids
        /// </summary>
        /// <param name="affidavitScrubbingIds">Affidavit Client scrubs ids</param>
        /// <returns>List of AffidavitFileDetail objects</returns>
        List<ScrubbingFileDetail> GetAffidavitDetailsByClientScrubIds(List<int> affidavitScrubbingIds);

        /// <summary>
        /// Gets all override affidavit client scrubs based on ids
        /// </summary>
        /// <param name="scrubIds">List of affidavit client scrub ids</param>
        /// <returns>List of override affidavit_client_scrubs objects</returns>
        List<affidavit_client_scrubs> GetAffidavitClientScrubsByIds(List<int> scrubIds);
    }

    public class AffidavitRepository : BroadcastRepositoryBase, IAffidavitRepository
    {

        public AffidavitRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int SaveAffidavitFile(ScrubbingFile affidavitFile)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var affidavit_file = _MapFromScrubbingFile(affidavitFile);
                    context.affidavit_files.Add(affidavit_file);
                    context.SaveChanges();
                    return affidavit_file.id;
                });
        }

        private affidavit_files _MapFromScrubbingFile(ScrubbingFile affidavitFile)
        {
            var result = new affidavit_files
            {
                created_date = affidavitFile.CreatedDate,
                file_hash = affidavitFile.FileHash,
                file_name = affidavitFile.FileName,
                source_id = affidavitFile.SourceId,
                status = (int)affidavitFile.Status,
                affidavit_file_problems = affidavitFile.FileProblems.Select(p => new affidavit_file_problems()
                {
                    id = p.Id,
                    affidavit_file_id = p.FileId,
                    problem_description = p.ProblemDescription
                }).ToList(),
                affidavit_file_details = affidavitFile.FileDetails.Select(d => new affidavit_file_details
                {
                    air_time = d.AirTime,
                    original_air_date = d.OriginalAirDate,
                    isci = d.Isci,
                    program_name = d.ProgramName,
                    genre = d.Genre,
                    spot_length_id = d.SpotLengthId,
                    station = d.Station,
                    market = d.Market,
                    affiliate = d.Affiliate,
                    estimate_id = d.EstimateId,
                    inventory_source = d.InventorySource,
                    spot_cost = d.SpotCost,
                    leadin_genre = d.LeadinGenre,
                    leadout_genre = d.LeadoutGenre,
                    leadin_program_name = d.LeadinProgramName,
                    leadout_program_name = d.LeadoutProgramName,
                    leadin_end_time = d.LeadInEndTime == null ? (int?)null : d.LeadInEndTime.Value,
                    leadout_start_time = d.LeadOutStartTime == null ? (int?)null : d.LeadOutStartTime.Value,
                    program_show_type = d.ShowType,
                    leadin_show_type = d.LeadInShowType,
                    leadout_show_type = d.LeadOutShowType,
                    adjusted_air_date = d.AdjustedAirDate,
                    archived = d.Archived,
                    supplied_program_name = d.SuppliedProgramName,
                    affidavit_client_scrubs = _MapFromClientScrubs(d.ClientScrubs),
                    affidavit_file_detail_problems = _MapFromFileDetailProblems(d.FileDetailProblems),
                    affidavit_file_detail_demographics = d.Demographics.Select(demo =>
                        new affidavit_file_detail_demographics
                        {
                            audience_id = demo.AudienceId,
                            overnight_impressions = demo.OvernightImpressions,
                            overnight_rating = demo.OvernightRating
                        }).ToList()
                }).ToList()
            };

            return result;
        }

        private ICollection<affidavit_file_detail_problems> _MapFromFileDetailProblems(List<FileDetailProblem> affidavitFileDetailProblems)
        {
            var result = affidavitFileDetailProblems.Select(p =>
                        new affidavit_file_detail_problems
                        {
                            problem_description = p.Description,
                            problem_type = (int)p.Type
                        }).ToList();
            return result;
        }

        private ICollection<affidavit_client_scrubs> _MapFromClientScrubs(List<ClientScrub> affidavitClientScrubs)
        {
            var result = affidavitClientScrubs.Select(s => new affidavit_client_scrubs
            {
                comment = s.Comment,
                proposal_version_detail_quarter_week_id = s.ProposalVersionDetailQuarterWeekId,
                match_program = s.MatchProgram,
                match_genre = s.MatchGenre,
                match_show_type = s.MatchShowType,
                match_market = s.MatchMarket,
                match_station = s.MatchStation,
                match_time = s.MatchTime,
                match_date = s.MatchDate,
                modified_by = s.ModifiedBy,
                modified_date = s.ModifiedDate,
                match_isci_days = s.MatchIsciDays,
                match_isci = s.MatchIsci,
                effective_program_name = s.EffectiveProgramName,
                effective_genre = s.EffectiveGenre,
                effective_show_type = s.EffectiveShowType,
                lead_in = s.LeadIn,
                status = (int)s.Status,
                effective_isci = s.EffectiveIsci,
                effective_client_isci = s.EffectiveClientIsci,
                affidavit_client_scrub_audiences = s.ClientScrubAudiences.Select(a =>
                    new affidavit_client_scrub_audiences
                    {
                        audience_id = a.AudienceId,
                        impressions = a.Impressions
                    }).ToList()
            }).ToList();
            return result;
        }

        public void SaveScrubbedFileDetails(List<ScrubbingFileDetail> affidavitFileDetails)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var affidavitFileDetail in affidavitFileDetails)
                    {
                        var detail = context.affidavit_file_details.Find(affidavitFileDetail.Id);
                        context.affidavit_file_detail_problems.RemoveRange(detail.affidavit_file_detail_problems);
                        context.affidavit_client_scrubs.RemoveRange(detail.affidavit_client_scrubs);
                        detail.affidavit_file_detail_problems = _MapFromFileDetailProblems(affidavitFileDetail.FileDetailProblems);
                        detail.affidavit_client_scrubs = _MapFromClientScrubs(affidavitFileDetail.ClientScrubs);
                        context.SaveChanges();
                    }
                });
        }

        public ScrubbingFile GetAffidavit(int affidavitId, bool includeScrubbingDetail = false)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = context.affidavit_files
                        .Include(a => a.affidavit_file_details)
                        .Include(a => a.affidavit_file_problems);

                    if (includeScrubbingDetail)
                    {
                        query.Include(a => a.affidavit_file_details.Select(d => d.affidavit_client_scrubs));
                        query.Include(a => a.affidavit_file_details.Select(d => d.affidavit_file_detail_demographics));
                        query.Include(a => a.affidavit_file_details.Select(d => d.affidavit_file_detail_problems));
                        query.Include(a => a.affidavit_file_details.Select(d =>
                            d.affidavit_client_scrubs.Select(s => s.affidavit_client_scrub_audiences)));
                    }

                    var affidavitFile = query.Single(a => a.id == affidavitId, "Affidavit/Post not found in database");

                    return _MapToScrubbingFile(affidavitFile);
                });
        }

        private ScrubbingFile _MapToScrubbingFile(affidavit_files affidavitFile)
        {
            return new ScrubbingFile
            {
                Id = affidavitFile.id,
                FileName = affidavitFile.file_name,
                FileHash = affidavitFile.file_hash,
                SourceId = affidavitFile.source_id,
                Status = (FileProcessingStatusEnum)affidavitFile.status,
                CreatedDate = affidavitFile.created_date,
                FileProblems = affidavitFile.affidavit_file_problems.Select(p => new FileProblem
                {
                    Id = p.id,
                    FileId = p.affidavit_file_id,
                    ProblemDescription = p.problem_description
                }).ToList(),
                FileDetails = affidavitFile.affidavit_file_details.Select(d => new ScrubbingFileDetail
                {
                    Id = d.id,
                    ScrubbingFileId = d.affidavit_file_id,
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
                    ShowType = d.program_show_type,
                    LeadInShowType = d.leadin_show_type,
                    LeadOutShowType = d.leadout_show_type,
                    LeadOutStartTime = d.leadout_start_time,
                    LeadInEndTime = d.leadin_end_time,
                    Market = d.market,
                    Affiliate = d.affiliate,
                    EstimateId = d.estimate_id,
                    InventorySource = d.inventory_source.Value,
                    SpotCost = d.spot_cost,
                    SuppliedProgramName = d.supplied_program_name,
                    Demographics = d.affidavit_file_detail_demographics.Select(a => new ScrubbingDemographics
                    {
                        AudienceId = a.audience_id.Value,
                        OvernightImpressions = a.overnight_impressions.Value,
                        OvernightRating = a.overnight_rating.Value
                    }).ToList(),
                    ClientScrubs = d.affidavit_client_scrubs.Select(a => new ClientScrub
                    {
                        Id = a.id,
                        ScrubbingFileDetailId = a.affidavit_file_detail_id,
                        ProposalVersionDetailQuarterWeekId = a.proposal_version_detail_quarter_week_id,
                        MatchProgram = a.match_program,
                        MatchGenre = a.match_genre,
                        MatchShowType = a.match_show_type,
                        MatchMarket = a.match_market,
                        MatchStation = a.match_station,
                        MatchTime = a.match_time,
                        MatchDate = a.match_date,
                        MatchIsciDays = a.match_isci_days,
                        MatchIsci = a.match_isci,
                        EffectiveProgramName = a.effective_program_name,
                        EffectiveGenre = a.effective_genre,
                        EffectiveShowType = a.effective_show_type,
                        Status = (ScrubbingStatus)a.status,
                        Comment = a.comment,
                        ModifiedBy = a.modified_by,
                        ModifiedDate = a.modified_date,
                        LeadIn = a.lead_in,
                        EffectiveIsci = a.effective_isci,
                        ProposalVersionDetailId = a.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.id,
                        PostingBookId = a.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.posting_book_id,
                        PostingPlaybackType = (ProposalEnums.ProposalPlaybackType?)a.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters
                            .proposal_version_details.posting_playback_type,
                        ClientScrubAudiences = a.affidavit_client_scrub_audiences.Select(sa =>
                            new ScrubbingFileAudiences
                            {
                                ClientScrubId = sa.affidavit_client_scrub_id,
                                AudienceId = sa.audience_id,
                                Impressions = sa.impressions
                            }).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        public List<ProposalDetailPostScrubbing> GetProposalDetailPostScrubbing(int proposalId, ScrubbingStatus? status)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = (from proposalVersions in context.proposal_versions
                                 from proposalVersionDetail in proposalVersions.proposal_version_details
                                 from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                 from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                 from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                 let affidavitDetails = affidavitFileScrub.affidavit_file_details
                                 where proposalVersions.proposal_id == proposalId && proposalVersions.snapshot_date == null
                                 orderby proposalVersionDetail.id
                                 select new
                                 {
                                     proposalDetailId = proposalVersionDetail.id,
                                     affidavitDetails,
                                     affidavitFileScrub,
                                     proposalVersionWeeks
                                 });
                    if (status.HasValue)
                    {
                        query = query.Where(s => s.affidavitFileScrub.status == (int)status);
                    }

                    var queryData = query.ToList();

                    return queryData.Select(x =>
                    {
                        return new ProposalDetailPostScrubbing()
                        {
                            ScrubbingClientId = x.affidavitFileScrub.id,
                            ProposalDetailId = x.proposalDetailId,
                            Station = x.affidavitDetails.station,
                            ISCI = x.affidavitDetails.isci,
                            ProgramName = x.affidavitFileScrub.effective_program_name,
                            SpotLengthId = x.affidavitDetails.spot_length_id,
                            TimeAired = x.affidavitDetails.air_time,
                            DateAired = x.affidavitDetails.original_air_date,
                            DayOfWeek = x.affidavitDetails.original_air_date.DayOfWeek,
                            GenreName = x.affidavitFileScrub.effective_genre,
                            MatchGenre = x.affidavitFileScrub.match_genre,
                            MatchMarket = x.affidavitFileScrub.match_market,
                            MatchProgram = x.affidavitFileScrub.match_program,
                            MatchStation = x.affidavitFileScrub.match_station,
                            MatchTime = x.affidavitFileScrub.match_time,
                            MatchDate = x.affidavitFileScrub.match_date,
                            MatchIsciDays = x.affidavitFileScrub.match_isci_days,
                            Comments = x.affidavitFileScrub.comment,
                            ClientISCI = x.affidavitFileScrub.effective_client_isci,
                            WeekStart = x.affidavitDetails.original_air_date.GetWeekMonday(),
                            ShowTypeName = x.affidavitFileScrub.effective_show_type,
                            StatusOverride = x.affidavitFileScrub.status_override,
                            Status = (ScrubbingStatus)x.affidavitFileScrub.status,
                            MatchShowType = x.affidavitFileScrub.match_show_type,
                            WWTVProgramName = x.affidavitDetails.program_name,
                            WeekIscis = x.proposalVersionWeeks.proposal_version_detail_quarter_week_iscis.Select(_MapToProposalWeekIsciDto).ToList()
                        };
                    }).ToList();
                });
        }

        private ProposalWeekIsciDto _MapToProposalWeekIsciDto(proposal_version_detail_quarter_week_iscis isci)
        {
            return new ProposalWeekIsciDto
            {
                HouseIsci = isci.house_isci,
                MarriedHouseIsci = isci.married_house_iscii,
                Brand = isci.brand
            };
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
                                         where proposal.id == proposalId &&
                                               affidavitClientScrub.status == (int)ScrubbingStatus.InSpec &&
                                               proposalVersion.snapshot_date == null
                                         select new
                                         {
                                             affidavitFileDetails,
                                             proposalVersionQuarters,
                                             proposalVersionDetail,
                                             proposalVersion,
                                             proposal,
                                             proposalVersionWeeks,
                                             affidavitClientScrub
                                         }).ToList();

                    var inSpecAffidavitFileDetails = inSpecDetails.Select(x => new InSpecAffidavitFileDetail()
                    {
                        Station = x.affidavitFileDetails.station,
                        Isci = x.affidavitClientScrub.effective_client_isci,
                        ProgramName = x.affidavitClientScrub.effective_program_name,
                        SpotLengthId = x.affidavitFileDetails.spot_length_id,
                        AirTime = x.affidavitFileDetails.air_time,
                        AirDate = x.affidavitFileDetails.original_air_date,
                        DaypartName = x.proposalVersionDetail.daypart_code,
                        NsiImpressions = x.affidavitClientScrub.affidavit_client_scrub_audiences.ToDictionary(i => i.audience_id, j => j.impressions),
                        OvernightImpressions = x.affidavitFileDetails.affidavit_file_detail_demographics.ToDictionary(i => i.audience_id.Value, j => j.overnight_impressions.Value),
                        NtiImpressions = x.proposalVersionWeeks.nti_transmittals_audiences.ToDictionary(i => i.audience_id, j => j.impressions),
                        Quarter = x.proposalVersionQuarters.quarter,
                        Year = x.proposalVersionQuarters.year,
                        AdvertiserId = x.proposal.advertiser_id,
                        ProposalWeekTotalCost = x.proposalVersionWeeks.cost,
                        ProposalWeekTotalImpressionsGoal = x.proposalVersionWeeks.impressions_goal,
                        Units = x.proposalVersionWeeks.units,
                        ProposalWeekId = x.proposalVersionWeeks.id,
                        ProposalDetailPostingBookId = x.proposalVersionDetail.posting_book_id,
                        ProposalDetailPlaybackType = (ProposalEnums.ProposalPlaybackType?)x.proposalVersionDetail.posting_playback_type,
                        ProposalDetailSpotLengthId = x.proposalVersionDetail.spot_length_id,
                        Adu = x.proposalVersionDetail.adu,
                        HouseIsci = x.affidavitFileDetails.isci,
                        WeekIscis = x.proposalVersionWeeks.proposal_version_detail_quarter_week_iscis.Select(_MapToProposalWeekIsciDto).ToList()

                    }).OrderBy(x => x.Station).ThenBy(x => x.AirDate).ThenBy(x => x.AirTime).ThenBy(x => x.Isci).ToList();

                    return inSpecAffidavitFileDetails;
                });
        }

        /// <summary>
        /// Persists a List of OutboundAffidavitFileValidationResultDto objects
        /// </summary>
        /// <param name="model">List of OutboundAffidavitFileValidationResultDto objects to be saved</param>
        public void SaveValidationObject(List<WWTVOutboundFileValidationResult> model)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.affidavit_outbound_files.AddRange(model.Select(item =>
                    new affidavit_outbound_files()
                    {
                        created_date = item.CreatedDate,
                        file_hash = item.FileHash,
                        file_name = item.FileName,
                        source_id = (int)item.Source,
                        status = (int)item.Status,
                        created_by = item.CreatedBy,
                        affidavit_outbound_file_problems = item.ErrorMessages.Select(y =>
                            new affidavit_outbound_file_problems()
                            {
                                problem_description = y
                            }).ToList()
                    }).ToList());
                context.SaveChanges();
            });
        }

        public List<MyEventsReportData> GetMyEventsReportData(int proposalId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var myEventsReportDataList = new List<MyEventsReportData>();

                    var affidavitData = (from proposal in context.proposals
                                         from proposalVersion in proposal.proposal_versions
                                         from proposalVersionDetail in proposalVersion.proposal_version_details
                                         from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                         from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                         from affidavitClientScrub in proposalVersionWeeks.affidavit_client_scrubs
                                         let affidavitFileDetail = affidavitClientScrub.affidavit_file_details
                                         where proposalVersionDetail.proposal_versions.proposal_id == proposalId &&
                                               affidavitClientScrub.status == (int)ScrubbingStatus.InSpec &&
                                               proposalVersionWeeks.myevents_report_name != null &&
                                               proposalVersion.snapshot_date == null
                                         select new { affidavitFileDetail, affidavitClientScrub, proposal, proposalVersionDetail, proposalVersionWeeks })
                        .ToList();

                    var affidavitDataGroupedByReportName = affidavitData.
                        OrderBy(x => x.proposalVersionWeeks.start_date).
                        GroupBy(x => x.proposalVersionWeeks.myevents_report_name);

                    foreach (var affidavitDataGroup in affidavitDataGroupedByReportName)
                    {
                        var myEventsReportDataGroup = new MyEventsReportData();

                        foreach (var affidavit in affidavitDataGroup)
                        {
                            var myEventsReportDataItem = new MyEventsReportDataLine
                            {
                                ReportableName = affidavit.proposalVersionWeeks.myevents_report_name,
                                StationCallLetters = affidavit.affidavitFileDetail.station,
                                LineupStartDate = affidavit.affidavitFileDetail.original_air_date,
                                LineupStartTime = new DateTime().AddSeconds(affidavit.affidavitFileDetail.air_time),
                                AirDate = affidavit.affidavitFileDetail.original_air_date.AddSeconds(affidavit
                                    .affidavitFileDetail.air_time),
                                AdvertiserId = affidavit.proposal.advertiser_id,
                                SpotLengthId = affidavit.affidavitFileDetail.spot_length_id,
                                DaypartCode = affidavit.proposalVersionDetail.daypart_code
                            };

                            myEventsReportDataGroup.Lines.Add(myEventsReportDataItem);
                        }

                        myEventsReportDataList.Add(myEventsReportDataGroup);
                    }

                    return myEventsReportDataList;
                });
        }

        public void OverrideScrubStatus(List<int> ClientScrubIds, ScrubbingStatus overrideToStatus)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var scrubs = context.affidavit_client_scrubs.Where(s => ClientScrubIds.Contains(s.id));
                    if (scrubs.Any())
                    {
                        scrubs.ForEach(s =>
                        {
                            s.status = (int)overrideToStatus;
                            s.status_override = true;
                        });
                        context.SaveChanges();
                    }
                }
            );
        }

        /// <summary>
        /// Gets all override affidavit client scrubs based on ids
        /// </summary>
        /// <param name="scrubIds">List of affidavit client scrub ids</param>
        /// <returns>List of override affidavit_client_scrubs objects</returns>
        public List<affidavit_client_scrubs> GetAffidavitClientScrubsByIds(List<int> scrubIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.affidavit_client_scrubs
                            .Where(s => scrubIds.Contains(s.id) && s.status_override == true)
                            .ToList();
                }
            );
        }

        /// <summary>
        /// Undo the override status
        /// </summary>
        /// <param name="scrubs">List of affidavit client scrubs to undo the override status</param>
        public void SaveScrubsStatus(List<affidavit_client_scrubs> scrubs)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var scrubIds = scrubs.Select(x => x.id).ToList();
                    var overrideScrubs = context.affidavit_client_scrubs.Where(s => scrubIds.Contains(s.id));
                    if (overrideScrubs.Any())
                    {
                        overrideScrubs.ForEach(s =>
                        {
                            var scrub = scrubs.Where(x => x.id == s.id).First();
                            s.status = scrub.status;
                            s.status_override = scrub.status_override;
                        });
                        context.SaveChanges();
                    }
                }
            );
        }

        public List<ScrubbingFileDetail> GetAffidavitDetails(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var affidavitDetails = (from proposalVersionDetail in context.proposal_version_details
                                            from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                            from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                            from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                            where proposalVersionDetail.id == proposalDetailId
                                            select affidavitFileScrub.affidavit_file_details);

                    return affidavitDetails.Select(d => new ScrubbingFileDetail
                    {
                        Id = d.id,
                        ScrubbingFileId = d.affidavit_file_id,
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
                        ShowType = d.program_show_type,
                        LeadInShowType = d.leadin_show_type,
                        LeadOutShowType = d.leadout_show_type,
                        LeadOutStartTime = d.leadout_start_time,
                        LeadInEndTime = d.leadin_end_time,
                        Market = d.market,
                        Affiliate = d.affiliate,
                        EstimateId = d.estimate_id,
                        InventorySource = d.inventory_source.Value,
                        SpotCost = d.spot_cost,
                        ClientScrubs = d.affidavit_client_scrubs.Select(a => new ClientScrub
                        {
                            Id = a.id,
                            ScrubbingFileDetailId = a.affidavit_file_detail_id,
                            ProposalVersionDetailQuarterWeekId = a.proposal_version_detail_quarter_week_id,
                            MatchProgram = a.match_program,
                            MatchGenre = a.match_genre,
                            MatchShowType = a.match_show_type,
                            MatchMarket = a.match_market,
                            MatchStation = a.match_station,
                            MatchTime = a.match_time,
                            MatchDate = a.match_date,
                            MatchIsciDays = a.match_isci_days,
                            MatchIsci = a.match_isci,
                            EffectiveProgramName = a.effective_program_name,
                            EffectiveGenre = a.effective_genre,
                            EffectiveShowType = a.effective_show_type,
                            EffectiveIsci = a.effective_isci,
                            EffectiveClientIsci = a.effective_client_isci,
                            Status = (ScrubbingStatus)a.status,
                            Comment = a.comment,
                            ModifiedBy = a.modified_by,
                            ModifiedDate = a.modified_date,
                            LeadIn = a.lead_in,
                            ProposalVersionDetailId = a.proposal_version_detail_quarter_weeks
                                .proposal_version_detail_quarters.proposal_version_details.id,
                            PostingBookId = a.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters
                                .proposal_version_details.posting_book_id,
                            PostingPlaybackType = (ProposalEnums.ProposalPlaybackType?)a
                                .proposal_version_detail_quarter_weeks.proposal_version_detail_quarters
                                .proposal_version_details.posting_playback_type
                        }).ToList()
                    }).ToList();
                });
        }

        public void RemoveAffidavitAudiences(List<ScrubbingFileDetail> affidavitFileDetails)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var affidavitFileDetail in affidavitFileDetails)
                    {
                        foreach (var affidavitClientScrub in affidavitFileDetail.ClientScrubs)
                        {
                            var audiences = context.affidavit_client_scrub_audiences.Where(x =>
                                x.affidavit_client_scrub_id == affidavitClientScrub.Id);

                            context.affidavit_client_scrub_audiences.RemoveRange(audiences);
                        }
                    }

                    context.SaveChanges();
                });
        }

        public void SaveAffidavitAudiences(List<ScrubbingFileDetail> affidavitFileDetails)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var affidavitFileDetail in affidavitFileDetails)
                    {
                        foreach (var affidavitClientScrub in affidavitFileDetail.ClientScrubs)
                        {
                            foreach (var affidavitClientScrubAudience in affidavitClientScrub
                                .ClientScrubAudiences)
                            {
                                context.affidavit_client_scrub_audiences.Add(new affidavit_client_scrub_audiences
                                {
                                    affidavit_client_scrub_id = affidavitClientScrub.Id,
                                    audience_id = affidavitClientScrubAudience.AudienceId,
                                    impressions = affidavitClientScrubAudience.Impressions
                                });
                            }
                        }
                    }

                    context.SaveChanges();
                });
        }

        public List<ScrubbingFileDetail> GetUnlinkedAffidavitDetailsByIsci(string isci)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var affidavitDetails = (from affidavitFileDetail in context.affidavit_file_details
                                            where affidavitFileDetail.isci.Equals(isci)
                                            && !affidavitFileDetail.affidavit_client_scrubs.Any()
                                            select affidavitFileDetail).ToList();

                    return _MapToScrubbingFileDetail(affidavitDetails);
                });
        }

        private List<ScrubbingFileDetail> _MapToScrubbingFileDetail(List<affidavit_file_details> affidavitDetails)
        {
            return affidavitDetails.Select(d => new ScrubbingFileDetail
            {
                Id = d.id,
                ScrubbingFileId = d.affidavit_file_id,
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
                ShowType = d.program_show_type,
                LeadInShowType = d.leadin_show_type,
                LeadOutShowType = d.leadout_show_type,
                LeadOutStartTime = d.leadout_start_time,
                LeadInEndTime = d.leadin_end_time,
                Market = d.market,
                Affiliate = d.affiliate,
                EstimateId = d.estimate_id,
                InventorySource = d.inventory_source.Value,
                SpotCost = d.spot_cost,
                SuppliedProgramName = d.supplied_program_name
            }).ToList();
        }

        /// <summary>
        /// Gets all the affidavit details based on affidavit client scubs ids
        /// </summary>
        /// <param name="affidavitScrubbingIds">Affidavit Client scrubs ids</param>
        /// <returns>List of AffidavitFileDetail objects</returns>
        public List<ScrubbingFileDetail> GetAffidavitDetailsByClientScrubIds(List<int> affidavitScrubbingIds)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var affidavitDetails = (from affidavitFileDetail in context.affidavit_file_details
                                           from affidavit_client_scrubs in affidavitFileDetail.affidavit_client_scrubs
                                           where affidavitScrubbingIds.Contains(affidavit_client_scrubs.id)
                                           select affidavitFileDetail).ToList();

                   return _MapToScrubbingFileDetail(affidavitDetails);
               });
        }
    }
}
