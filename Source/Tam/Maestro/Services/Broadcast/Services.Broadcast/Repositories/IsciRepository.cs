using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IIsciRepository : IDataRepository
    {
        /// <summary>
        /// Searches for all the iscis that contain the filter in all the contracted proposals
        /// </summary>
        /// <param name="isci">Filter</param>
        /// <returns>List of iscis found</returns>
        List<ValidIsciDto> FindValidIscis(string isci);

        /// <summary>
        /// Loads all the isci mappings from DB
        /// </summary>
        /// <param name="iscis">List of iscis to load the mappings for</param>
        /// <returns>Dictionary containing the isci mappings</returns>
        Dictionary<string, string> LoadIsciMappings(List<string> iscis);

        /// <summary>
        /// Adds a new isci mapping
        /// </summary>
        /// <param name="originalIsci">Original isci</param>
        /// <param name="effectiveIsci">Effective Isci</param>
        /// <param name="name">User requesting the mapping</param>
        /// <returns>True or false</returns>
        bool AddIsciMapping(string originalIsci, string effectiveIsci, string name);

        /// <summary>
        /// Removes iscis from blacklist table
        /// </summary>
        /// <param name="iscisToRemove">Isci list to remove</param>
        /// <returns>True or false</returns>
        bool RemoveIscisFromBlacklistTable(List<string> iscisToRemove);

        /// <summary>
        /// Adds a new record in isci_blacklist
        /// </summary>
        /// <param name="iscis">List of Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        /// <returns>True or false</returns>
        bool ArchiveIscis(List<string> iscis, string username);
    }

    /// <summary>
    /// This repository deals with everything related to isci_blacklist and isci_mapping tables
    /// </summary>
    public class IsciRepository : BroadcastRepositoryBase, IIsciRepository
    {
        public IsciRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }


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
                           where proposalVersion.status == (int)ProposalEnums.ProposalStatusType.Contracted && proposalVersion.snapshot_date == null
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

        /// <summary>
        /// Adds a new isci mapping
        /// </summary>
        /// <param name="originalIsci">Original isci</param>
        /// <param name="effectiveIsci">Effective Isci</param>
        /// <param name="name">User requesting the mapping</param>
        /// <returns>True or false</returns>
        public bool AddIsciMapping(string originalIsci, string effectiveIsci, string name)
        {
            return _InReadUncommitedTransaction(
              context =>
              {
                  context.isci_mapping.Add(new isci_mapping
                  {
                      created_date = DateTime.Now,
                      created_by = name,
                      original_isci = originalIsci,
                      effective_isci = effectiveIsci
                  });
                  context.SaveChanges();
                  return true;
              });
        }

        /// <summary>
        /// Removes iscis from blacklist table
        /// </summary>
        /// <param name="iscisToRemove">Isci list to remove</param>
        /// <returns>True or false</returns>
        public bool RemoveIscisFromBlacklistTable(List<string> iscisToRemove)
        {
            return _InReadUncommitedTransaction(
                 context =>
                 {
                     context.isci_blacklist.RemoveRange(context.isci_blacklist.Where(x => iscisToRemove.Contains(x.ISCI)).ToList());
                     context.SaveChanges();
                     return true;
                 });
        }

        /// <summary>
        /// Adds a new record in isci_blacklist
        /// </summary>
        /// <param name="iscis">List of Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        /// <returns>True or false</returns>
        public bool ArchiveIscis(List<string> iscis, string username)
        {
            return _InReadUncommitedTransaction(
                 context =>
                 {
                     context.isci_blacklist.AddRange(iscis.Select(x => new isci_blacklist
                     {
                         created_by = username,
                         created_date = DateTime.Now,
                         ISCI = x
                     }).ToList());
                     context.SaveChanges();
                     return true;
                 });
        }
    }
}
