using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IPostRepository : IDataRepository
    {
        List<PostDto> GetAllPostedProposals();

        /// <summary>
        /// Counts all the unlinked iscis
        /// </summary>
        /// <returns>Number of unlinked iscis</returns>
        int CountUnlinkedIscis();

        /// <summary>
        /// Gets the unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<UnlinkedIscisDto> GetUnlinkedIscis();

        /// <summary>
        /// Gets the archived iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<UnlinkedIscisDto> GetArchivedIscis();

        /// <summary>
        /// Gets the impressions and NTI conversion factor for a contract and rating audiences
        /// </summary>
        /// <param name="proposalId">proposal or contract id</param>
        /// <param name="ratingsAudiences">list of rating audiences</param>
        /// <returns></returns>
        List<PostImpressionsDataDto> GetPostImpressionsData(int proposalId, List<int> ratingsAudiences);
        /// <summary>
        /// Adds a new record in affidavit_file_detail_problems with status: ArchivedIsci
        /// </summary>
        /// <param name="details">List of Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        void ArchiveIsci(List<string> details, string username);

        /// <summary>
        /// Adds a 'Not a Cadent Isci' problem
        /// </summary>
        /// <param name="details">List of AffidavitFileDetail objects to process</param>
        void AddNotACadentIsciProblem(List<long> details);

        /// <summary>
        /// Checks if an isci is blacklisted
        /// </summary>
        /// <param name="isci">Iscis to check</param>
        /// <returns>True or false</returns>
        bool IsIsciBlacklisted(List<string> isci);

        /// <summary>
        /// Loads all the file details records for the specified id list
        /// </summary>
        /// <param name="fileDetailIds">List of file detail id</param>
        /// <returns>List of AffidavitFileDetail objects</returns>
        List<AffidavitFileDetail> LoadFileDetailsByIds(List<long> fileDetailIds);

        /// <summary>
        /// Searches for all the iscis that contain the filter in all the contracted proposals
        /// </summary>
        /// <param name="isci">Filter</param>
        /// <returns>List of iscis found</returns>
        List<ValidIsciDto> FindValidIscis(string isci);

        /// <summary>
        /// Adds a new isci mapping
        /// </summary>
        /// <param name="mapIsciDto">MapIsciDto object</param>
        /// <param name="name">User requesting the mapping</param>
        void AddNewMapping(MapIsciDto mapIsciDto, string name);

        /// <summary>
        /// Loads all the isci mappings from DB
        /// </summary>
        /// <param name="iscis">List of iscis to load the mappings for</param>
        /// <returns>Dictionary containing the isci mappings</returns>
        Dictionary<string, string> LoadIsciMappings(List<string> iscis);

        /// <summary>
        /// Gets all the affidavit file detail problems for specific details
        /// </summary>
        /// <param name="fileDetailIds">List of affidavit file detail ids</param>
        /// <returns>List of AffidavitFileDetailProblem objects</returns>
        List<AffidavitFileDetailProblem> GetIsciProblems(List<long> fileDetailIds);

        /// <summary>
        /// Removes iscis from blacklist table
        /// </summary>
        /// <param name="iscisToRemove">Isci list to remove</param>
        void RemoveIscisFromBlacklistTable(List<string> iscisToRemove);

        /// <summary>
        /// Removes not a cadent entries for specific affidavit file details
        /// </summary>
        /// <param name="fileDetailList">Affidavit file detail ids to remove the problems for</param>
        void RemoveNotACadentIsciProblems(List<long> fileDetailList);

        /// <summary>
        /// Sets the archived flag for all the iscis in the list
        /// </summary>
        /// <param name="fileDetailIds">List of AffidavitFileDetails to set the flag to</param>
        /// <param name="flag">Flag to set</param>
        void SetArchivedFlag(List<long> fileDetailIds, bool flag);
    }

    public class PostRepository : BroadcastRepositoryBase, IPostRepository
    {
        public PostRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<PostDto> GetAllPostedProposals()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var proposalVersions = context.proposal_versions.Where(p =>
                        (ProposalEnums.ProposalStatusType)p.status == ProposalEnums.ProposalStatusType.Contracted).ToList();
                    var posts = new List<PostDto>();

                    foreach (var proposalVersion in proposalVersions)
                    {
                        var spots = (from proposalVersionDetail in proposalVersion.proposal_version_details
                                     from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                     from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                     from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                     select affidavitFileScrub).ToList();

                        posts.Add(new PostDto
                        {
                            ContractId = proposalVersion.proposal_id,
                            ContractName = proposalVersion.proposal.name,
                            UploadDate = (from proposalVersionDetail in proposalVersion.proposal_version_details
                                          from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                          from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                          from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                          orderby affidavitFileScrub.affidavit_file_details.affidavit_files.created_date descending
                                          select (DateTime?)affidavitFileScrub.affidavit_file_details.affidavit_files.created_date).FirstOrDefault(),
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

        public List<PostImpressionsDataDto> GetPostImpressionsData(int proposalId, List<int> ratingsAudiences)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from proposal in context.proposals
                            from proposalVersion in proposal.proposal_versions
                            from proposalVersionDetail in proposalVersion.proposal_version_details
                            from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                            from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                            from affidavitClientScrub in proposalVersionWeeks.affidavit_client_scrubs
                            from affidavitClientScrubAudience in affidavitClientScrub.affidavit_client_scrub_audiences
                            where proposal.id == proposalId &&
                                  (ScrubbingStatus)affidavitClientScrub.status == ScrubbingStatus.InSpec &&
                                  ratingsAudiences.Contains(affidavitClientScrubAudience.audience_id)
                            select new PostImpressionsDataDto { Impressions = affidavitClientScrubAudience.impressions, NtiConversionFactor = proposalVersionDetail.nti_conversion_factor }).ToList();
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
        /// Gets the unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<UnlinkedIscisDto> GetUnlinkedIscis()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var iscis = _GetUnlinkedIscisQuery(context);
                    return iscis.ToList().Select(x => _MapUnlinkedOrArchiveIsci(x)).OrderBy(x => x.ISCI).ToList();
                });
        }

        /// <summary>
        /// Gets all the affidavit file detail problems for specific details
        /// </summary>
        /// <param name="fileDetailIds">List of affidavit file detail ids</param>
        /// <returns>List of AffidavitFileDetailProblem objects</returns>
        public List<AffidavitFileDetailProblem> GetIsciProblems(List<long> fileDetailIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.affidavit_file_detail_problems
                    .Where(x => fileDetailIds.Contains(x.affidavit_file_detail_id) && x.problem_type != (int)AffidavitFileDetailProblemTypeEnum.ArchivedIsci)
                    .ToList()
                    .Select(x => new AffidavitFileDetailProblem()
                    {
                        Type = (AffidavitFileDetailProblemTypeEnum)x.problem_type,
                        Description = x.problem_description,
                        DetailId = x.affidavit_file_detail_id
                    }).ToList();
                });
        }

        /// <summary>
        /// Gets the archived iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<UnlinkedIscisDto> GetArchivedIscis()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var iscis = from fileDetails in context.affidavit_file_details
                                join blacklist in context.affidavit_blacklist on fileDetails.isci equals blacklist.ISCI
                                where fileDetails.archived == true
                                select new { fileDetails, blacklist };
                    return iscis.ToList().Select(x => _MapUnlinkedOrArchiveIsci(x.fileDetails, x.blacklist)).OrderBy(x => x.ISCI).ToList();
                });
        }

        private UnlinkedIscisDto _MapUnlinkedOrArchiveIsci(affidavit_file_details x, affidavit_blacklist y = null)
        {
            return new UnlinkedIscisDto
            {
                Affiliate = x.affiliate,
                Genre = x.genre,
                FileDetailId = x.id,
                ISCI = x.isci,
                Market = x.market,
                ProgramName = x.program_name,
                Station = x.station,
                TimeAired = x.air_time,
                DateAired = x.original_air_date,
                SpotLength = x.spot_length_id,
                DateAdded = y?.created_date
            };
        }

        /// <summary>
        /// Adds a new record in affidavit_file_detail_problems with status: ArchivedIsci
        /// </summary>
        /// <param name="details">List of Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        public void ArchiveIsci(List<string> details, string username)
        {
            _InReadUncommitedTransaction(
                 context =>
                 {
                     context.affidavit_blacklist.AddRange(details.Select(x => new affidavit_blacklist
                     {
                         created_by = username,
                         created_date = DateTime.Now,
                         ISCI = x
                     }).ToList());
                     context.SaveChanges();
                 });
        }

        /// <summary>
        /// Removes iscis from blacklist table
        /// </summary>
        /// <param name="iscisToRemove">Isci list to remove</param>
        public void RemoveIscisFromBlacklistTable(List<string> iscisToRemove)
        {
            _InReadUncommitedTransaction(
                 context =>
                 {
                     context.affidavit_blacklist.RemoveRange(context.affidavit_blacklist.Where(x => iscisToRemove.Contains(x.ISCI)).ToList());
                     context.SaveChanges();
                 });
        }

        /// <summary>
        /// Adds a 'Not a Cadent Isci' problem
        /// </summary>
        /// <param name="details">List of AffidavitFileDetail objects to process</param>
        public void AddNotACadentIsciProblem(List<long> details)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   context.affidavit_file_detail_problems.AddRange(details.Select(x => new affidavit_file_detail_problems
                   {
                       affidavit_file_detail_id = x,
                       problem_description = "Not a Cadent ISCI",
                       problem_type = (int)AffidavitFileDetailProblemTypeEnum.ArchivedIsci
                   }).ToList());
                   context.SaveChanges();
               });
        }

        /// <summary>
        /// Removes not a cadent entries for specific affidavit file details
        /// </summary>
        /// <param name="fileDetailIds">Affidavit file detail ids to remove the problems for</param>
        public void RemoveNotACadentIsciProblems(List<long> fileDetailIds)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   context.affidavit_file_detail_problems.RemoveRange(
                       context.affidavit_file_detail_problems
                        .Where(x => fileDetailIds.Contains(x.affidavit_file_detail_id) && x.problem_type == (int)AffidavitFileDetailProblemTypeEnum.ArchivedIsci)
                        .ToList());
                   context.SaveChanges();
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
                   return context.affidavit_blacklist.Any(x => iscis.Contains(x.ISCI));
               });
        }

        /// <summary>
        /// Loads all the file details records for the specified id list
        /// </summary>
        /// <param name="fileDetailIds">List of file detail id</param>
        /// <returns>List of AffidavitFileDetail objects</returns>
        public List<AffidavitFileDetail> LoadFileDetailsByIds(List<long> fileDetailIds)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var iscisOnFileDetails = context.affidavit_file_details.Where(x => fileDetailIds.Contains(x.id)).Select(x => x.isci).ToList();
                   return context.affidavit_file_details
                                    .Where(x => iscisOnFileDetails.Contains(x.isci))
                                    .Select(
                                       x => new AffidavitFileDetail
                                       {
                                           Id = x.id,
                                           Isci = x.isci
                                       }).ToList();
               });
        }

        /// <summary>
        /// Sets the archived flag for all the iscis in the list
        /// </summary>
        /// <param name="fileDetailIds">List of AffidavitFileDetails to set the flag to</param>
        /// <param name="flag">Flag to set</param>
        public void SetArchivedFlag(List<long> fileDetailIds, bool flag)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   context.affidavit_file_details
                       .Where(x => fileDetailIds.Contains(x.id))
                       .ToList()
                       .ForEach(x =>
                       {
                           x.archived = flag;
                       });
                   context.SaveChanges();
               });
        }

        private IQueryable<affidavit_file_details> _GetUnlinkedIscisQuery(QueryHintBroadcastContext context)
        {
            return from fileDetails in context.affidavit_file_details
                   join clientScrubs in context.affidavit_client_scrubs on fileDetails.id equals clientScrubs.affidavit_file_detail_id
                   into dataGroupped
                   from x in dataGroupped.DefaultIfEmpty()
                   where x == null && fileDetails.archived == false
                   select fileDetails;
        }

        /// <summary>
        /// Searches for all the iscis that contain the filter in all the contracted proposals
        /// </summary>
        /// <param name="isci">Filter</param>
        /// <returns>List of iscis found</returns>
        public List<ValidIsciDto> FindValidIscis(string isci)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   return (from proposalVersion in context.proposal_versions
                           from proposalDetail in proposalVersion.proposal_version_details
                           from proposalDetailQuarter in proposalDetail.proposal_version_detail_quarters
                           from proposalDetailQuarterWeek in proposalDetailQuarter.proposal_version_detail_quarter_weeks
                           from proposalDetailQuarterWeekIsci in proposalDetailQuarterWeek.proposal_version_detail_quarter_week_iscis
                           where proposalVersion.status == (int)ProposalEnums.ProposalStatusType.Contracted
                                && proposalDetailQuarterWeekIsci.client_isci.StartsWith(isci)
                           select new ValidIsciDto
                           {
                               HouseIsci = proposalDetailQuarterWeekIsci.house_isci,
                               Married = proposalDetailQuarterWeekIsci.married_house_iscii,
                               ProposalId = proposalVersion.proposal_id
                           }).Distinct().OrderBy(x => x).ToList();
               });
        }

        /// <summary>
        /// Adds a new isci mapping
        /// </summary>
        /// <param name="mapIsciDto">MapIsciDto object</param>
        /// <param name="name">User requesting the mapping</param>
        public void AddNewMapping(MapIsciDto mapIsciDto, string name)
        {
            _InReadUncommitedTransaction(
              context =>
              {
                  context.isci_mapping.Add(new isci_mapping
                  {
                      created_date = DateTime.Now,
                      created_by = name,
                      original_isci = mapIsciDto.OriginalIsci,
                      effective_isci = mapIsciDto.EffectiveIsci
                  });
                  context.SaveChanges();
              });
        }

        /// <summary>
        /// Loads all the isci mappings from DB
        /// </summary>
        /// <param name="iscis">List of iscis to load the mappings for</param>
        /// <returns>Dictionary containing the isci mappings</returns>
        public Dictionary<string, string> LoadIsciMappings(List<string> iscis)
        {
            return _InReadUncommitedTransaction(
              context =>
              {
                  return context.isci_mapping
                              .Where(x => iscis.Contains(x.original_isci))
                              .ToDictionary(x => x.original_isci, x => x.effective_isci);
              });
        }
    }
}
