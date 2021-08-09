using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IReelIsciRepository : IDataRepository
    {
        /// <summary>
        /// Deletes reel iscis which are active between startdate and enddate
        /// </summary>
        /// <param name="startDate">The startdate from when reel iscis to be deleted</param>
        /// <param name="endDate">The enddate till when reel iscis to be deleted</param>
        /// <returns>Total number of deleted reel iscis</returns>
        int DeleteReelIscisBetweenRange(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Adds reel iscis
        /// </summary>
        /// <param name="reelIscis">The list of reel iscis to be inserted</param>
        /// <returns>Total number of inserted reel iscis</returns>
        int AddReelIscis(List<ReelIsciDto> reelIscis);
    }

    public class ReelIsciRepository : BroadcastRepositoryBase, IReelIsciRepository
    {
        public ReelIsciRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }
        
        /// <inheritdoc />
        public int DeleteReelIscisBetweenRange(DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var reelIscisToDelete = context.reel_iscis
                                                .Where(reelIsci => reelIsci.active_start_date <= startDate && reelIsci.active_end_date >= endDate 
                                                                    || reelIsci.active_start_date >= startDate && reelIsci.active_start_date <= endDate 
                                                                    || reelIsci.active_end_date >= startDate && reelIsci.active_end_date <= endDate)
                                                .ToList();

                var deletedCount = context.reel_iscis.RemoveRange(reelIscisToDelete).Count();
                context.SaveChanges();
                return deletedCount;
            });
        }

        /// <inheritdoc />
        public int AddReelIscis(List<ReelIsciDto> reelIscis)
        {
            return _InReadUncommitedTransaction(context => 
            {
                var reelIscisToAdd = reelIscis.Select(reelIsci => new reel_iscis() 
                {
                    isci = reelIsci.Isci,
                    spot_length_id = reelIsci.SpotLengthId,
                    active_start_date = reelIsci.ActiveStartDate,
                    active_end_date = reelIsci.ActiveEndDate,
                    reel_isci_advertiser_name_references = reelIsci.ReelIsciAdvertiserNameReferences.Select(x => new reel_isci_advertiser_name_references()
                    {
                        advertiser_name_reference = x.AdvertiserNameReference
                    }).ToList(),
                    ingested_at = reelIsci.IngestedAt
                }).ToList();
                var addedCount = context.reel_iscis.AddRange(reelIscisToAdd).Count();
                context.SaveChanges();
                return addedCount;
            });
        }
    }
}