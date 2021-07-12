using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.Repositories
{
    public interface IPostRepository : IDataRepository
    {
        List<PostedContract> GetAllPostedProposals();

        /// <summary>
        /// Counts all the unlinked iscis
        /// </summary>
        /// <returns>Number of unlinked iscis</returns>
        int CountUnlinkedIscis();

        /// <summary>
        /// Gets the unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscis objects</returns>
        List<UnlinkedIscis> GetUnlinkedIscis();

        /// <summary>
        /// Gets the archived iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<ArchivedIscisDto> GetArchivedIscis();

        List<PostImpressionsData> GetPostImpressionsData(List<int> proposalId, List<int> ratingsAudiences);

        /// <summary>
        /// Gets the impressions and NTI conversion factor for a contract and rating audiences
        /// </summary>
        /// <param name="proposalId">proposal or contract id</param>
        /// <param name="ratingsAudiences">list of rating audiences</param>
        /// <returns></returns>
        List<PostImpressionsData> GetPostImpressionsData(int proposalId, List<int> ratingsAudiences);

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
        /// Loads all the file details records for the specified isci list
        /// </summary>
        /// <param name="iscis">List of iscis</param>
        /// <returns>List of AffidavitFileDetail objects</returns>
        List<ScrubbingFileDetail> LoadFileDetailsByIscis(List<string> iscis);

        /// <summary>
        /// Loads all the file details records for the specified id list
        /// </summary>
        /// <param name="fileDetailIds">List of ids</param>
        /// <returns>List of AffidavitFileDetail objects</returns>
        List<ScrubbingFileDetail> LoadFileDetailsByIds(List<long> fileDetailIds);

        /// <summary>
        /// Gets all the affidavit file detail problems for specific details
        /// </summary>
        /// <param name="fileDetailIds">List of affidavit file detail ids</param>
        /// <returns>List of AffidavitFileDetailProblem objects</returns>
        List<FileDetailProblem> GetIsciProblems(List<long> fileDetailIds);

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
        public PostRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        public List<PostedContract> GetAllPostedProposals()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var posts = context.Database.SqlQuery<PostedContract>("usp_GetPostedProposals");
                    
                    return posts.OrderByDescending(x => x.UploadDate).ToList();
                });
        }

        public List<PostImpressionsData> GetPostImpressionsData(List<int> proposalIds, List<int> ratingsAudiences)
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
                        where proposalIds.Contains(proposal.id)  &&
                              (ScrubbingStatus)affidavitClientScrub.status == ScrubbingStatus.InSpec &&
                              proposalVersion.snapshot_date == null &&
                              ratingsAudiences.Contains(affidavitClientScrubAudience.audience_id)
                        select new PostImpressionsData
                        {
                            ProposalId = proposal.id,
                            Impressions = affidavitClientScrubAudience.impressions,
                            NtiConversionFactor = proposalVersionDetail.nti_conversion_factor,
                            SpotLengthId = proposalVersionDetail.spot_length_id,
                            AudienceId = affidavitClientScrubAudience.audience_id
                        }).ToList();
                });
        }

        public List<PostImpressionsData> GetPostImpressionsData(int proposalId, List<int> ratingsAudiences)
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
                                  ratingsAudiences.Contains(affidavitClientScrubAudience.audience_id) &&
                                  proposalVersion.snapshot_date == null
                            select new PostImpressionsData
                            {
                                Impressions = affidavitClientScrubAudience.impressions,
                                NtiConversionFactor = proposalVersionDetail.nti_conversion_factor,
                                SpotLengthId = proposalVersionDetail.spot_length_id
                            }).ToList();
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
        /// <returns>List of UnlinkedIscis objects</returns>
        public List<UnlinkedIscis> GetUnlinkedIscis()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var iscis = from detail in _GetUnlinkedIscisQuery(context)
                                from problem in detail.affidavit_file_detail_problems
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
        /// Gets all the affidavit file detail problems for specific details
        /// </summary>
        /// <param name="fileDetailIds">List of affidavit file detail ids</param>
        /// <returns>List of AffidavitFileDetailProblem objects</returns>
        public List<FileDetailProblem> GetIsciProblems(List<long> fileDetailIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.affidavit_file_detail_problems
                    .Where(x => fileDetailIds.Contains(x.affidavit_file_detail_id) && x.problem_type != (int)FileDetailProblemTypeEnum.ArchivedIsci)
                    .ToList()
                    .Select(x => new FileDetailProblem()
                    {
                        Type = (FileDetailProblemTypeEnum)x.problem_type,
                        Description = x.problem_description,
                        DetailId = x.affidavit_file_detail_id
                    }).ToList();
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
                    var iscis = from fileDetails in context.affidavit_file_details
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
                       problem_type = (int)FileDetailProblemTypeEnum.ArchivedIsci
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
                        .Where(x => fileDetailIds.Contains(x.affidavit_file_detail_id) && x.problem_type == (int)FileDetailProblemTypeEnum.ArchivedIsci)
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
                   return context.isci_blacklist.Any(x => iscis.Contains(x.ISCI));
               });
        }

        /// <summary>
        /// Loads all the file details records for the specified isci list
        /// </summary>
        /// <param name="iscis">List of iscis</param>
        /// <returns>List of AffidavitFileDetail objects</returns>
        public List<ScrubbingFileDetail> LoadFileDetailsByIscis(List<string> iscis)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   return context.affidavit_file_details
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
        /// Loads all the file details records for the specified id list
        /// </summary>
        /// <param name="fileDetailIds">List of ids</param>
        /// <returns>List of AffidavitFileDetail objects</returns>
        public List<ScrubbingFileDetail> LoadFileDetailsByIds(List<long> fileDetailIds)
        {
            List<string> iscis = new List<string>();
            _InReadUncommitedTransaction(
               context =>
               {
                   iscis = context.affidavit_file_details.Where(x => fileDetailIds.Contains(x.id)).Select(x => x.isci).Distinct().ToList();
               });
            return LoadFileDetailsByIscis(iscis);
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
    }
}
