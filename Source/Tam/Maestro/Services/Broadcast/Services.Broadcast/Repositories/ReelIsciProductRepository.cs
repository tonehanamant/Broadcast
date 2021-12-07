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
    public interface IReelIsciProductRepository : IDataRepository
    {
        /// <summary>
        /// Deletes reel isci products that do not exist in reel isci
        /// </summary>
        /// <returns>Total number of deleted reel isci products</returns>
        int DeleteReelIsciProductsNotExistInReelIsci();

        /// <summary>
        /// Deletes the isci product mappings for the given iscis.
        /// </summary>
        /// <param name="iscis">The iscis.</param>
        int DeleteIsciProductMapping(List<string> iscis);

        /// <summary>
        /// Gets the isci product mappings for the given list of icsis.
        /// </summary>
        /// <param name="iscis">The iscis.</param>
        /// <returns></returns>
        List<IsciProductMappingDto> GetIsciProductMappings(List<string> iscis);

        /// <summary>
        /// Save Product Isci mapping
        /// </summary>
        /// <param name="isciProductMappings">The List which contains save parameters</param>
        /// /// <param name="createdBy">Created By</param>
        /// /// <param name="createdAt">Created At</param>
        /// <returns>Total number of inserted Product Mappings</returns>
        int SaveIsciProductMappings(List<IsciProductMappingDto> isciProductMappings, string createdBy, DateTime createdAt);
    }

    public class ReelIsciProductRepository :  BroadcastRepositoryBase, IReelIsciProductRepository
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

        /// <inheritdoc />
        public int DeleteIsciProductMapping(List<string> iscis)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var deleted = context.reel_isci_products.RemoveRange(context.reel_isci_products.Where(s => iscis.Contains(s.isci)));
                context.SaveChanges();
                var deletedCount = deleted.Count();
                return deletedCount;
            });
        }

        /// <inheritdoc />
        public List<IsciProductMappingDto> GetIsciProductMappings(List<string> iscis)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.reel_isci_products
                    .Where(i => iscis.Contains(i.isci))
                    .Select(i => new IsciProductMappingDto
                    {
                        Isci = i.isci,
                        ProductName = i.product_name
                    })
                    .ToList();
                return result;
            });
        }

        /// <inheritdoc />
        public int SaveIsciProductMappings(List<IsciProductMappingDto> isciProductMappings, string createdBy, DateTime createdAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var isciProductMappingsToAdd = isciProductMappings.Select(isciProductMapping => new reel_isci_products()
                {
                    product_name = isciProductMapping.ProductName,
                    isci = isciProductMapping.Isci,
                    created_at = createdAt,
                    created_by = createdBy
                }).ToList();
                var addedCount = context.reel_isci_products.AddRange(isciProductMappingsToAdd).Count();
                context.SaveChanges();
                return addedCount;
            });
        }
    }
}
