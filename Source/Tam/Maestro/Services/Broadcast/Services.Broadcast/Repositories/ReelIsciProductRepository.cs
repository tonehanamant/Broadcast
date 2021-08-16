﻿using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Helpers;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IReelIsciProductRepository : IDataRepository
    {
        /// <summary>
        /// Deletes reel isci products that do not exist in reel isci
        /// </summary>
        /// <returns>Total number of deleted reel isci products</returns>
        int DeleteReelIsciProductsNotExistInReelIsci();
    }

    public class ReelIsciProductRepository :  BroadcastRepositoryBase, IReelIsciProductRepository
    {
        public ReelIsciProductRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        /// <inheritdoc />
        public int DeleteReelIsciProductsNotExistInReelIsci()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var sql = $@"DELETE FROM reel_isci_products WHERE isci NOT IN (select isci from reel_iscis)";
                var deletedCount = context.Database.ExecuteSqlCommand(sql);
                return deletedCount;
            });
        }
    }
}