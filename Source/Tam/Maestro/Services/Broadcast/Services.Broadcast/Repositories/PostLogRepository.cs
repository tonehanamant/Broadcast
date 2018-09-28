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
using Services.Broadcast.Entities.DTO;

namespace Services.Broadcast.Repositories
{
    public interface IPostLogRepository : IDataRepository
    {
        /// <summary>
        /// Saves processing validation results
        /// </summary>
        /// <param name="validationResults">List of FileValidationResult objects</param>
        void SavePreprocessingValidationResults(List<FileValidationResult> validationResults);

        /// <summary>
        /// Saves a post log file
        /// </summary>
        /// <param name="postLogFile">Post log file to be saved</param>
        /// <returns>The newly created id</returns>
        int SavePostLogFile(ScrubbingFile postLogFile);

        /// <summary>
        /// Gets the unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscis objects</returns>
        List<UnlinkedIscis> GetUnlinkedIscis();

        /// <summary>
        /// Counts all the unlinked iscis
        /// </summary>
        /// <returns>Number of unlinked iscis</returns>
        int CountUnlinkedIscis();

        /// <summary>
        /// Gets the archived iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<ArchivedIscisDto> GetArchivedIscis();

        /// <summary>
        /// Gets all the contracted proposals that have played spots
        /// </summary>
        /// <returns>List of PostedContracts objects</returns>
        List<PostedContracts> GetAllPostedProposals();

        /// <summary>
        /// Get the impression for a list of audiences of a proposal
        /// </summary>
        /// <param name="proposalId">Proposal to filter by</param>
        /// <param name="ratingsAudiences">List of audiences</param>
        /// <returns>List of PostImpressionsData objects</returns>
        List<PostImpressionsData> GetPostLogImpressionsData(int proposalId, List<int> ratingsAudiences);

        /// <summary>
        /// Checks if an isci is blacklisted
        /// </summary>
        /// <param name="isci">Iscis to check</param>
        /// <returns>True or false</returns>
        bool IsIsciBlacklisted(List<string> isci);

        /// <summary>
        /// Gets all the unlinked post log records for an isci
        /// </summary>
        /// <param name="isci">ISCI to filter by</param>
        /// <returns>List of ScrubbingFileDetail objects</returns>
        List<ScrubbingFileDetail> GetUnlinkedPostLogDetailsByIsci(string isci);

        /// <summary>
        /// Saves scrubbed records
        /// </summary>
        /// <param name="fileDetails">List of ScrubbingFileDetail objects</param>
        void SaveScrubbedFileDetails(List<ScrubbingFileDetail> fileDetails);

        /// <summary>
        /// Loads all the file details records for the specified id list
        /// </summary>
        /// <param name="fileDetailIds">List of ids</param>
        /// <returns>List of ScrubbingFileDetail objects</returns>
        List<ScrubbingFileDetail> LoadFileDetailsByIds(List<long> fileDetailIds);

        /// <summary>
        /// Loads all the file details records for the specified isci list
        /// </summary>
        /// <param name="iscis">List of iscis</param>
        /// <returns>List of ScrubbingFileDetail objects</returns>
        List<ScrubbingFileDetail> LoadFileDetailsByIscis(List<string> iscis);

        /// <summary>
        /// Removes not a cadent entries for specific post log file details
        /// </summary>
        /// <param name="fileDetailIds">PostLog file detail ids to remove the problems for</param>
        void RemoveNotACadentIsciProblems(List<long> fileDetailList);

        /// <summary>
        /// Sets the archived flag for all the iscis in the list
        /// </summary>
        /// <param name="fileDetailIds">List of file detail ids to set the flag to</param>
        /// <param name="flag">Flag to set</param>
        void SetArchivedFlag(List<long> fileDetailIds, bool flag);

        /// <summary>
        /// Adds a 'Not a Cadent Isci' problem
        /// </summary>
        /// <param name="details">List of file detail ids to process</param>
        void AddNotACadentIsciProblem(List<long> details);

        /// <summary>
        /// Gets a post log file record based on id. Optional: includes scrubbing details
        /// </summary>
        /// <param name="postLogFileId">Post log file id</param>
        /// <param name="includeScrubbingDetail">Optional: include scrubbing details</param>
        /// <returns>WWTVFile Object</returns>
        ScrubbingFile GetPostLogFile(int postLogFileId, bool includeScrubbingDetail = false);

        /// <summary>
        /// Gets all the scrubbing records for a proposal
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <param name="status">Scrubbing status to filter by</param>
        /// <returns>List of ProposalDetailPostScrubbing objects</returns>
        List<ProposalDetailPostScrubbing> GetProposalDetailPostScrubbing(int proposalId, ScrubbingStatus? status);

        /// <summary>
        /// Overrides the status of post log client scrubs
        /// </summary>
        /// <param name="ClientScrubIds">Client scrub ids to override the status</param>
        /// <param name="overrideToStatus">New Status</param>
        void OverrideScrubStatus(List<int> ClientScrubIds, ScrubbingStatus overrideToStatus);

        /// <summary>
        /// Gets all override post log client scrubs based on ids
        /// </summary>
        /// <param name="scrubIds">List of post log client scrub ids</param>
        /// <returns>List of override postlog_client_scrubs objects</returns>
        List<postlog_client_scrubs> GetPostLogClientScrubsByIds(List<int> scrubIds);

        /// <summary>
        /// Save scrubbing status
        /// </summary>
        /// <param name="scrubs">List of post log client scrubs to undo the override status</param>
        void SaveScrubsStatus(List<postlog_client_scrubs> scrubs);

        /// <summary>
        /// Gets all the post log details based on post log client scubs ids
        /// </summary>
        /// <param name="scrubbingIds">Post log Client scrubs ids</param>
        /// <returns>List of ScrubbingFileDetail objects</returns>
        List<ScrubbingFileDetail> GetPostLogDetailsByClientScrubsByIds(List<int> scrubbingIds);
    }
    public class PostLogRepository : BroadcastRepositoryBase, IPostLogRepository
    {
        public PostLogRepository(ISMSClient pSmsClient,
                                IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
                                ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        /// <summary>
        /// Saves processing validation results
        /// </summary>
        /// <param name="validationResults">List of FileValidationResult objects</param>
        public void SavePreprocessingValidationResults(List<FileValidationResult> validationResults)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.postlog_outbound_files.AddRange(validationResults.Select(item =>
                    new postlog_outbound_files()
                    {
                        created_date = item.CreatedDate,
                        file_hash = item.FileHash,
                        file_name = item.FileName,
                        source_id = (int) item.Source,
                        status = (int)item.Status,
                        created_by = item.CreatedBy,
                        postlog_outbound_file_problems = item.ErrorMessages.Select(y =>
                            new postlog_outbound_file_problems()
                            {
                                problem_description = y
                            }).ToList()
                    }).ToList());
                context.SaveChanges();
            });
        }

        /// <summary>
        /// Saves a post log file
        /// </summary>
        /// <param name="postLogFile">Post log file to be saved</param>
        /// <returns>The newly created id</returns>
        public int SavePostLogFile(ScrubbingFile postLogFile)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var postlog_file = _MapFromPostLogFile(postLogFile);
                    context.postlog_files.Add(postlog_file);
                    context.SaveChanges();
                    return postlog_file.id;
                });
        }

        /// <summary>
        /// Gets the unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscis objects</returns>
        public List<UnlinkedIscis> GetUnlinkedIscis()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var iscis = from detail in _GetUnlinkedIscisQuery(context)
                                from problem in detail.postlog_file_detail_problems
                                group detail by new { detail.isci, detail.spot_length_id, problem.problem_type } into g
                                select new { g.Key.spot_length_id, g.Key.problem_type, g.Key.isci, count = g.Count() };

                    return iscis.ToList().Select(x => new UnlinkedIscis
                    {
                        ISCI = x.isci,
                        SpotLengthId = x.spot_length_id,
                        Count = x.count,
                        ProblemType = (FileDetailProblemTypeEnum)x.problem_type
                    }).OrderByDescending(x => x.Count).ToList();
                });
        }

        /// <summary>
        /// Counts all the unlinked iscis
        /// </summary>
        /// <returns>Number of unlinked iscis</returns>
        public int CountUnlinkedIscis()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return _GetUnlinkedIscisQuery(context).Count();
                });
        }

        /// <summary>
        /// Gets the archived iscis
        /// </summary>
        /// <returns>List of ArchivedIscisDto objects</returns>
        public List<ArchivedIscisDto> GetArchivedIscis()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var iscis = from fileDetails in context.postlog_file_details
                                join blacklist in context.isci_blacklist on fileDetails.isci equals blacklist.ISCI
                                where fileDetails.archived == true
                                select new { fileDetails, blacklist };
                    return iscis.ToList().Select(x => new ArchivedIscisDto
                    {
                        Affiliate = x.fileDetails.affiliate,
                        Genre = x.fileDetails.genre,
                        FileDetailId = x.fileDetails.id,
                        ISCI = x.fileDetails.isci,
                        Market = x.fileDetails.market,
                        ProgramName = x.fileDetails.program_name,
                        Station = x.fileDetails.station,
                        TimeAired = x.fileDetails.air_time,
                        DateAired = x.fileDetails.original_air_date,
                        SpotLength = x.fileDetails.spot_length_id,
                        DateAdded = x.blacklist.created_date
                    }).OrderBy(x => x.ISCI).ToList();
                });
        }

        /// <summary>
        /// Gets all the contracted proposals that have played spots
        /// </summary>
        /// <returns>List of PostedContracts objects</returns>
        public List<PostedContracts> GetAllPostedProposals()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var proposalVersions = context.proposal_versions
                        .Where(p => (ProposalEnums.ProposalStatusType)p.status == ProposalEnums.ProposalStatusType.Contracted)
                        .ToList();
                    var posts = new List<PostedContracts>();

                    foreach (var proposalVersion in proposalVersions)
                    {
                        var spots = (from proposalVersionDetail in proposalVersion.proposal_version_details
                                     from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                     from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                     from postlogFileScrub in proposalVersionWeeks.postlog_client_scrubs
                                     let ScrubbingFileDetail = postlogFileScrub.postlog_file_details
                                     select postlogFileScrub).ToList();

                        posts.Add(new PostedContracts
                        {
                            ContractId = proposalVersion.proposal_id,
                            Equivalized = proposalVersion.equivalized,
                            ContractName = proposalVersion.proposal.name,
                            UploadDate = (from proposalVersionDetail in proposalVersion.proposal_version_details
                                          from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                          from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                          from postlogFileScrub in proposalVersionWeeks.postlog_client_scrubs
                                          orderby postlogFileScrub.postlog_file_details.postlog_files.created_date descending
                                          select (DateTime?)postlogFileScrub.postlog_file_details.postlog_files.created_date).FirstOrDefault(),
                            SpotsInSpec = spots.Count(s => (ScrubbingStatus)s.status == ScrubbingStatus.InSpec),
                            SpotsOutOfSpec = spots.Count(s => (ScrubbingStatus)s.status == ScrubbingStatus.OutOfSpec),
                            AdvertiserId = proposalVersion.proposal.advertiser_id,
                            GuaranteedAudienceId = proposalVersion.guaranteed_audience_id,
                            PostType = (SchedulePostType)proposalVersion.post_type,
                            PrimaryAudienceBookedImpressions = (from proposalVersionDetail in proposalVersion.proposal_version_details
                                                                from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                                                select proposalVersionQuarters.impressions_goal).Sum(),
                        });
                    }

                    return posts.OrderByDescending(x => x.UploadDate).ToList();
                });
        }

        /// <summary>
        /// Saves scrubbed records
        /// </summary>
        /// <param name="fileDetails">List of ScrubbingFileDetail objects</param>
        public void SaveScrubbedFileDetails(List<ScrubbingFileDetail> fileDetails)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var fileDetail in fileDetails)
                    {
                        var detail = context.postlog_file_details.Find(fileDetail.Id);
                        context.postlog_file_detail_problems.RemoveRange(detail.postlog_file_detail_problems);
                        context.postlog_client_scrubs.RemoveRange(detail.postlog_client_scrubs);

                        detail.postlog_file_detail_problems = _MapFromPostLogFileDetailProblems(fileDetail.FileDetailProblems);
                        detail.postlog_client_scrubs = _MapFromPostLogClientScrubs(fileDetail.ClientScrubs);
                        context.SaveChanges();
                    }
                });
        }
        
        /// <summary>
        /// Removes not a cadent entries for specific post log file details
        /// </summary>
        /// <param name="fileDetailIds">PostLog file detail ids to remove the problems for</param>
        public void RemoveNotACadentIsciProblems(List<long> fileDetailIds)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   context.postlog_file_detail_problems.RemoveRange(
                       context.postlog_file_detail_problems
                        .Where(x => fileDetailIds.Contains(x.postlog_file_detail_id) && x.problem_type == (int)FileDetailProblemTypeEnum.ArchivedIsci)
                        .ToList());
                   context.SaveChanges();
               });
        }

        /// <summary>
        /// Sets the archived flag for all the iscis in the list
        /// </summary>
        /// <param name="fileDetailIds">List of Post Log File Detail ids to set the flag to</param>
        /// <param name="flag">Flag to set</param>
        public void SetArchivedFlag(List<long> fileDetailIds, bool flag)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   context.postlog_file_details
                       .Where(x => fileDetailIds.Contains(x.id))
                       .ToList()
                       .ForEach(x =>
                       {
                           x.archived = flag;
                       });
                   context.SaveChanges();
               });
        }

        /// <summary>
        /// Get the impression for a list of audiences of a proposal
        /// </summary>
        /// <param name="proposalId">Proposal to filter by</param>
        /// <param name="ratingsAudiences">List of audiences</param>
        /// <returns>List of PostImpressionsData objects</returns>
        public List<PostImpressionsData> GetPostLogImpressionsData(int proposalId, List<int> ratingsAudiences)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from proposal in context.proposals
                            from proposalVersion in proposal.proposal_versions
                            from proposalVersionDetail in proposalVersion.proposal_version_details
                            from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                            from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                            from postlogClientScrub in proposalVersionWeeks.postlog_client_scrubs
                            from postlogClientScrubAudience in postlogClientScrub.postlog_client_scrub_audiences
                            where proposal.id == proposalId &&
                                  (ScrubbingStatus)postlogClientScrub.status == ScrubbingStatus.InSpec &&
                                  ratingsAudiences.Contains(postlogClientScrubAudience.audience_id)
                            select new PostImpressionsData
                            {
                                Impressions = postlogClientScrubAudience.impressions,
                                NtiConversionFactor = proposalVersionDetail.nti_conversion_factor,
                                SpotLengthId = proposalVersionDetail.spot_length_id
                            }).ToList();
                });
        }

        /// <summary>
        /// Checks if an isci is blacklisted
        /// </summary>
        /// <param name="iscis">Iscis to check</param>
        /// <returns>True or false</returns>
        public bool IsIsciBlacklisted(List<string> iscis)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   return context.isci_blacklist.Any(x => iscis.Contains(x.ISCI));
               });
        }

        /// <summary>
        /// Gets all the unlinked post log records for an isci
        /// </summary>
        /// <param name="isci">ISCI to filter by</param>
        /// <returns>List of ScrubbingFileDetail objects</returns>
        public List<ScrubbingFileDetail> GetUnlinkedPostLogDetailsByIsci(string isci)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var details = (from fileDetail in context.postlog_file_details
                                            where fileDetail.isci.Equals(isci)
                                            && !fileDetail.postlog_client_scrubs.Any()
                                            select fileDetail).ToList();

                    return _MapToPostLogDetail(details);
                });
        }

        /// <summary>
        /// Loads all the file details records for the specified id list
        /// </summary>
        /// <param name="fileDetailIds">List of ids</param>
        /// <returns>List of file detail ids</returns>
        public List<ScrubbingFileDetail> LoadFileDetailsByIds(List<long> fileDetailIds)
        {
            List<string> iscis = new List<string>();
            _InReadUncommitedTransaction(
               context =>
               {
                   iscis = context.postlog_file_details.Where(x => fileDetailIds.Contains(x.id)).Select(x => x.isci).Distinct().ToList();
               });
            return LoadFileDetailsByIscis(iscis);
        }

        /// <summary>
        /// Loads all the file details records for the specified isci list
        /// </summary>
        /// <param name="iscis">List of iscis</param>
        /// <returns>List of file detail ids</returns>
        public List<ScrubbingFileDetail> LoadFileDetailsByIscis(List<string> iscis)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   return context.postlog_file_details
                                    .Where(x => iscis.Contains(x.isci))
                                    .Select(
                                       x => new ScrubbingFileDetail
                                       {
                                           Id = x.id,
                                           Isci = x.isci
                                       }).ToList();
               });
        }

        /// <summary>
        /// Adds a 'Not a Cadent Isci' problem
        /// </summary>
        /// <param name="details">List of file detail ids to process</param>
        public void AddNotACadentIsciProblem(List<long> details)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   context.postlog_file_detail_problems.AddRange(details.Select(x => new postlog_file_detail_problems
                   {
                       postlog_file_detail_id = x,
                       problem_description = "Not a Cadent ISCI",
                       problem_type = (int)FileDetailProblemTypeEnum.ArchivedIsci
                   }).ToList());
                   context.SaveChanges();
               });
        }

        /// <summary>
        /// Gets a post log file record based on id. Optional: includes scrubbing details
        /// </summary>
        /// <param name="postLogFileId">Post log file id</param>
        /// <param name="includeScrubbingDetail">Optional: include scrubbing details</param>
        /// <returns>WWTVFile Object</returns>
        public ScrubbingFile GetPostLogFile(int postLogFileId, bool includeScrubbingDetail = false)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = context.postlog_files
                        .Include(a=> a.postlog_file_details)
                        .Include(a => a.postlog_file_problems);

                    if (includeScrubbingDetail)
                    {
                        query.Include(a => a.postlog_file_details.Select(d => d.postlog_client_scrubs));
                        query.Include(a => a.postlog_file_details.Select(d => d.postlog_file_detail_demographics));
                        query.Include(a => a.postlog_file_details.Select(d => d.postlog_file_detail_problems));
                        query.Include(a => a.postlog_file_details.Select(d =>
                            d.postlog_client_scrubs.Select(s => s.postlog_client_scrub_audiences)));
                    }

                    var postlogFile = query.Single(a => a.id == postLogFileId, "Post Log not found in database");

                    return _MapToPostLogFile(postlogFile);
                });
        }

        /// <summary>
        /// Gets all the scrubbing records for a proposal
        /// </summary>
        /// <param name="proposalId">Proposal id to filter by</param>
        /// <param name="status">Scrubbing status to filter by</param>
        /// <returns>List of ProposalDetailPostScrubbing objects</returns>
        public List<ProposalDetailPostScrubbing> GetProposalDetailPostScrubbing(int proposalId, ScrubbingStatus? status)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = (from proposalVersions in context.proposal_versions
                                 from proposalVersionDetail in proposalVersions.proposal_version_details
                                 from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                 from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                 from postlogFileScrub in proposalVersionWeeks.postlog_client_scrubs
                                 let postlogDetails = postlogFileScrub.postlog_file_details
                                 where proposalVersions.proposal_id == proposalId
                                 orderby proposalVersionDetail.id
                                 select new
                                 {
                                     proposalDetailId = proposalVersionDetail.id,
                                     postlogDetails,
                                     postlogFileScrub,
                                     proposalVersionWeeks
                                 });
                    if (status.HasValue)
                    {
                        query = query.Where(s => s.postlogFileScrub.status == (int)status);
                    }

                    var queryData = query.ToList();

                    var posts = new List<ProposalDetailPostScrubbing>();
                    posts.AddRange(queryData.Select(x =>
                    {

                        return new ProposalDetailPostScrubbing()
                        {
                            ScrubbingClientId = x.postlogFileScrub.id,
                            ProposalDetailId = x.proposalDetailId,
                            Station = x.postlogDetails.station,
                            ISCI = x.postlogDetails.isci,
                            ProgramName = x.postlogFileScrub.effective_program_name,
                            SpotLengthId = x.postlogDetails.spot_length_id,
                            TimeAired = x.postlogDetails.air_time,
                            DateAired = x.postlogDetails.original_air_date,
                            DayOfWeek = x.postlogDetails.original_air_date.DayOfWeek,
                            GenreName = x.postlogFileScrub.effective_genre,
                            MatchGenre = x.postlogFileScrub.match_genre,
                            MatchMarket = x.postlogFileScrub.match_market,
                            MatchProgram = x.postlogFileScrub.match_program,
                            MatchStation = x.postlogFileScrub.match_station,
                            MatchTime = x.postlogFileScrub.match_time,
                            MatchDate = x.postlogFileScrub.match_date.Value,
                            MatchIsciDays = x.postlogFileScrub.match_isci_days,
                            Comments = x.postlogFileScrub.comment,
                            ClientISCI = x.postlogFileScrub.effective_client_isci,
                            WeekStart = x.proposalVersionWeeks.start_date,
                            ShowTypeName = x.postlogFileScrub.effective_show_type,
                            StatusOverride = x.postlogFileScrub.status_override,
                            Status = (ScrubbingStatus)x.postlogFileScrub.status,
                            MatchShowType = x.postlogFileScrub.match_show_type
                        };
                    }
                    ).ToList());
                    return posts;
                });
        }

        /// <summary>
        /// Overrides the status of post log client scrubs
        /// </summary>
        /// <param name="ClientScrubIds">Client scrub ids to override the status</param>
        /// <param name="overrideToStatus">New Status</param>
        public void OverrideScrubStatus(List<int> ClientScrubIds, ScrubbingStatus overrideToStatus)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var scrubs = context.postlog_client_scrubs.Where(s => ClientScrubIds.Contains(s.id));
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
        /// Gets all override post log client scrubs based on ids
        /// </summary>
        /// <param name="scrubIds">List of post log client scrub ids</param>
        /// <returns>List of override postlog_client_scrubs objects</returns>
        public List<postlog_client_scrubs> GetPostLogClientScrubsByIds(List<int> scrubIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.postlog_client_scrubs
                            .Where(s => scrubIds.Contains(s.id) && s.status_override == true)
                            .ToList();
                }
            );
        }

        /// <summary>
        /// Save scrubbing status
        /// </summary>
        /// <param name="scrubs">List of post log client scrubs to undo the override status</param>
        public void SaveScrubsStatus(List<postlog_client_scrubs> scrubs)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var scrubIds = scrubs.Select(x => x.id).ToList();
                    var overrideScrubs = context.postlog_client_scrubs.Where(s => scrubIds.Contains(s.id));
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

        /// <summary>
        /// Gets all the post log details based on post log client scubs ids
        /// </summary>
        /// <param name="scrubbingIds">Post log Client scrubs ids</param>
        /// <returns>List of ScrubbingFileDetail objects</returns>
        public List<ScrubbingFileDetail> GetPostLogDetailsByClientScrubsByIds(List<int> scrubbingIds)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var details = (from fileDetails in context.postlog_file_details
                                           from clientScrubs in fileDetails.postlog_client_scrubs
                                           where scrubbingIds.Contains(clientScrubs.id)
                                           select fileDetails).ToList();

                   return _MapToPostLogDetail(details);
               });
        }

        private ScrubbingFile _MapToPostLogFile(postlog_files postlogFile)
        {
            return new ScrubbingFile
            {
                Id = postlogFile.id,
                FileName = postlogFile.file_name,
                FileHash = postlogFile.file_hash,
                SourceId = postlogFile.source_id,
                Status = (FileProcessingStatusEnum)postlogFile.status,
                CreatedDate = postlogFile.created_date,
                FileProblems = postlogFile.postlog_file_problems.Select(p => new ScrubbingFileProblem()
                {
                    Id = p.id,
                    FileId = p.postlog_file_id,
                    ProblemDescription = p.problem_description
                }).ToList(),
                FileDetails = postlogFile.postlog_file_details.Select(d => new ScrubbingFileDetail
                {
                    Id = d.id,
                    ScrubbingFileId = d.postlog_file_id,
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
                    Demographics = d.postlog_file_detail_demographics.Select(a => new ScrubbingDemographics()
                    {
                        AudienceId = a.audience_id.Value,
                        OvernightImpressions = a.overnight_impressions.Value,
                        OvernightRating = a.overnight_rating.Value
                    }).ToList(),
                    ClientScrubs = d.postlog_client_scrubs.Select(a => new ClientScrub
                    {
                        Id = a.id,
                        ScrubbingFileDetailId = a.postlog_file_detail_id,
                        ProposalVersionDetailQuarterWeekId = a.proposal_version_detail_quarter_week_id,
                        MatchProgram = a.match_program,
                        MatchGenre = a.match_genre,
                        MatchShowType = a.match_show_type,
                        MatchMarket = a.match_market,
                        MatchStation = a.match_station,
                        MatchTime = a.match_time,
                        MatchDate = a.match_date.Value,
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
                        ProposalVersionDetailId = a.proposal_version_detail_quarter_weeks
                            .proposal_version_detail_quarters.proposal_version_details.id,
                        PostingBookId = a.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters
                            .proposal_version_details.posting_book_id,
                        PostingPlaybackType = (ProposalEnums.ProposalPlaybackType?)a
                            .proposal_version_detail_quarter_weeks.proposal_version_detail_quarters
                            .proposal_version_details.posting_playback_type,
                        ClientScrubAudiences = a.postlog_client_scrub_audiences.Select(sa =>
                            new ScrubbingFileAudiences
                            {
                                ClientScrubId = sa.postlog_client_scrub_id,
                                AudienceId = sa.audience_id,
                                Impressions = sa.impressions
                            }).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        private ICollection<postlog_file_detail_problems> _MapFromPostLogFileDetailProblems(List<FileDetailProblem> fileDetailProblems)
        {
            var result = fileDetailProblems.Select(p =>
                        new postlog_file_detail_problems
                        {
                            problem_description = p.Description,
                            problem_type = (int)p.Type
                        }).ToList();
            return result;
        }

        private ICollection<postlog_client_scrubs> _MapFromPostLogClientScrubs(List<ClientScrub> clientScrubs)
        {
            var result = clientScrubs.Select(s => new postlog_client_scrubs
            {
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
                postlog_client_scrub_audiences = s.ClientScrubAudiences.Select(a =>
                    new postlog_client_scrub_audiences
                    {
                        audience_id = a.AudienceId,
                        impressions = a.Impressions
                    }).ToList()
            }).ToList();
            return result;
        }

        private List<ScrubbingFileDetail> _MapToPostLogDetail(List<postlog_file_details> details)
        {
            return details.Select(d => new ScrubbingFileDetail
            {
                Id = d.id,
                ScrubbingFileId = d.postlog_file_id,
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
            }).ToList();
        }

        private IQueryable<postlog_file_details> _GetUnlinkedIscisQuery(QueryHintBroadcastContext context)
        {
            return from fileDetails in context.postlog_file_details
                   join clientScrubs in context.postlog_client_scrubs on fileDetails.id equals clientScrubs.postlog_file_detail_id
                   into dataGroupped
                   from x in dataGroupped.DefaultIfEmpty()
                   where x == null && fileDetails.archived == false
                   select fileDetails;
        }

        private postlog_files _MapFromPostLogFile(ScrubbingFile postlogFile)
        {
            var result = new postlog_files
            {
                created_date = postlogFile.CreatedDate,
                file_hash = postlogFile.FileHash,
                file_name = postlogFile.FileName,
                source_id = postlogFile.SourceId,
                status = (int)postlogFile.Status,
                postlog_file_problems = postlogFile.FileProblems.Select(p => new postlog_file_problems()
                {
                    id = p.Id,
                    postlog_file_id = p.FileId,
                    problem_description = p.ProblemDescription
                }).ToList(),
                postlog_file_details = postlogFile.FileDetails.Select(d => new postlog_file_details
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
                    leadin_end_time = d.LeadInEndTime,
                    leadout_start_time = d.LeadOutStartTime,
                    program_show_type = d.ShowType,
                    leadin_show_type = d.LeadInShowType,
                    leadout_show_type = d.LeadOutShowType,
                    adjusted_air_date = d.AdjustedAirDate,
                    archived = d.Archived,
                    postlog_file_detail_problems = _MapFromFileDetailProblems(d.FileDetailProblems),
                    postlog_file_detail_demographics = d.Demographics.Select(demo =>
                        new postlog_file_detail_demographics
                        {
                            audience_id = demo.AudienceId,
                            overnight_impressions = demo.OvernightImpressions,
                            overnight_rating = demo.OvernightRating
                        }).ToList()
                }).ToList()
            };

            return result;
        }

        private ICollection<postlog_file_detail_problems> _MapFromFileDetailProblems(List<FileDetailProblem> fileDetailProblems)
        {
            var result = fileDetailProblems.Select(p =>
                        new postlog_file_detail_problems
                        {
                            problem_description = p.Description,
                            problem_type = (int)p.Type
                        }).ToList();
            return result;
        }
    }
}
