﻿using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
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
        /// Gets the impressions for a contract and rating audiences
        /// </summary>
        /// <param name="proposalId">proposal or contract id</param>
        /// <param name="ratingsAudiences">list of rating audiences</param>
        /// <returns></returns>
        double GetPostImpressions(int proposalId, List<int> ratingsAudiences);
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
        void AddNotACadentIsciProblem(List<AffidavitFileDetail> details);

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
        /// Sets the archived flag for all the records that contain specific iscis
        /// </summary>
        /// <param name="iscis">Iscis to set the flag to</param>
        void ArchiveFileDetailRecord(List<string> iscis);
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
                    var proposals = context.proposal_versions.Where(p =>
                        (ProposalEnums.ProposalStatusType)p.status == ProposalEnums.ProposalStatusType.Contracted).ToList();
                    var posts = new List<PostDto>();

                    foreach (var proposal in proposals)
                    {
                        var spots = (from proposalVersionDetail in proposal.proposal_version_details
                                     from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                     from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                     from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                     select affidavitFileScrub).ToList();

                        posts.Add(new PostDto
                        {
                            ContractId = proposal.proposal_id,
                            ContractName = proposal.proposal.name,
                            UploadDate = (from proposalVersionDetail in proposal.proposal_version_details
                                          from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                          from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                          from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                          orderby affidavitFileScrub.affidavit_file_details.affidavit_files.created_date descending
                                          select (DateTime?)affidavitFileScrub.affidavit_file_details.affidavit_files.created_date).FirstOrDefault(),
                            SpotsInSpec = spots.Count(s => (ScrubbingStatus)s.status == ScrubbingStatus.InSpec),
                            SpotsOutOfSpec = spots.Count(s => (ScrubbingStatus)s.status == ScrubbingStatus.OutOfSpec),
                            GuaranteedAudienceId = proposal.guaranteed_audience_id
                        });
                    }

                    return posts.OrderByDescending(x => x.UploadDate).ToList();
                });
        }

        public double GetPostImpressions(int proposalId, List<int> ratingsAudiences)
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
                            select affidavitClientScrubAudience.impressions).Sum(x => (double?)x) ?? 0;
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
                    var spotLengths = (from sl in context.spot_lengths select sl).ToList();
                    var unlinkedIscis = _GetUnlinkedIscisQuery(context).ToList();
                    return unlinkedIscis.Select(x => new UnlinkedIscisDto
                    {
                        Affiliate = x.affiliate,
                        Genre = x.genre,
                        FileDetailId = x.id,
                        ISCI = x.isci,
                        Market = x.market,
                        ProgramName = x.program_name,
                        SpotLength = spotLengths.Single(y => y.id == x.spot_length_id).length,
                        Station = x.station,
                        TimeAired = x.air_time,
                        DateAired = x.original_air_date
                    }).OrderBy(x => x.ISCI).ToList();
                });
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
        /// Adds a 'Not a Cadent Isci' problem
        /// </summary>
        /// <param name="details">List of AffidavitFileDetail objects to process</param>
        public void AddNotACadentIsciProblem(List<AffidavitFileDetail> details)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   context.affidavit_file_detail_problems.AddRange(details.Select(x => new affidavit_file_detail_problems
                   {
                       affidavit_file_detail_id = x.Id,
                       problem_description = "Not a Cadent ISCI",
                       problem_type = (int)AffidavitFileDetailProblemTypeEnum.ArchivedIsci
                   }).ToList());
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
                   var initialList = context.affidavit_file_details.Where(x => fileDetailIds.Contains(x.id)).Select(x => x.isci).ToList();
                   return context.affidavit_file_details
                                    .Where(x => initialList.Contains(x.isci))
                                    .Select(
                                       x => new AffidavitFileDetail
                                       {
                                           Id = x.id,
                                           Isci = x.isci
                                       }).ToList();
               });
        }

        /// <summary>
        /// Sets the archived flag for all the records that contain specific iscis
        /// </summary>
        /// <param name="iscis">Iscis to set the flag to</param>
        public void ArchiveFileDetailRecord(List<string> iscis)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   context.affidavit_file_details
                       .Where(x => iscis.Contains(x.isci))
                       .ToList()
                       .ForEach(x =>
                       {
                           x.archived = true;
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
