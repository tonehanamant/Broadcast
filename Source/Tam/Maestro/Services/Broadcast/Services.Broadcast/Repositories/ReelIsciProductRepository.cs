using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Isci;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public class ReelIsciProductRepository :  BroadcastRepositoryBase
    {
        public ReelIsciProductRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

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
