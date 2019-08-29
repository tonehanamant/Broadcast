using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using System.Data.Entity;
using System.Collections.Generic;
using ConfigurationService.Client;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryLogoRepository : IDataRepository
    {
        void SaveInventoryLogo(InventoryLogo inventoryLogo);
        InventoryLogo GetLatestInventoryLogo(int inventorySourceId);
        List<int> GetInventorySourcesWithLogo(IEnumerable<int> inventorySourceIds);
    }

    public class InventoryLogoRepository : BroadcastRepositoryBase, IInventoryLogoRepository
    {
        public InventoryLogoRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
                : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
        {
        }

        public List<int> GetInventorySourcesWithLogo(IEnumerable<int> inventorySourceIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.inventory_source_logos
                    .Where(x => inventorySourceIds.Contains(x.inventory_source_id))
                    .Select(x => x.inventory_source_id)
                    .Distinct()
                    .ToList();
            });
        }

        public InventoryLogo GetLatestInventoryLogo(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(context => 
            {
                var queryResult = context.inventory_source_logos
                    .Include(x => x.inventory_sources)
                    .Where(x => x.inventory_source_id == inventorySourceId)
                    .OrderByDescending(x => x.created_date)
                    .FirstOrDefault();

                    return queryResult == null ? null : new InventoryLogo
                        {
                            Id = queryResult.id,
                            InventorySource = new InventorySource
                            {
                                Id = queryResult.inventory_sources.id,
                                Name = queryResult.inventory_sources.name,
                                IsActive = queryResult.inventory_sources.is_active,
                                InventoryType = (InventorySourceTypeEnum)queryResult.inventory_sources.inventory_source_type
                            },
                            CreatedBy = queryResult.created_by,
                            CreatedDate = queryResult.created_date,
                            FileName = queryResult.file_name,
                            FileContent = queryResult.file_content
                        };
            });
        }

        public void SaveInventoryLogo(InventoryLogo inventoryLogo)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.inventory_source_logos.Add(new inventory_source_logos
                {
                    inventory_source_id = inventoryLogo.InventorySource.Id,
                    created_by = inventoryLogo.CreatedBy,
                    created_date = inventoryLogo.CreatedDate,
                    file_name = inventoryLogo.FileName,
                    file_content = inventoryLogo.FileContent
                });

                context.SaveChanges();
            });
        }
    }
}
