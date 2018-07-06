using Common.Services.Repositories;
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
        /// Gets the archived iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<UnlinkedIscisDto> GetArchivedIscis();

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
                    var iscis = _GetUnlinkedIscisQuery(context);
                    return _MapUnlinkedOrArchiveIsci(iscis);
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
                    var iscis = _GetArchivedIscisQuery(context);
                    return _MapUnlinkedOrArchiveIsci(iscis);
                });
        }

        private List<UnlinkedIscisDto> _MapUnlinkedOrArchiveIsci(IQueryable<affidavit_file_details> iscis)
        {
            return iscis.ToList().Select(x => new UnlinkedIscisDto
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
                SpotLength = x.spot_length_id
            }).OrderBy(x => x.ISCI).ToList();
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

        private IQueryable<affidavit_file_details> _GetArchivedIscisQuery(QueryHintBroadcastContext context)
        {
            return from fileDetails in context.affidavit_file_details
                   where fileDetails.archived == true
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
