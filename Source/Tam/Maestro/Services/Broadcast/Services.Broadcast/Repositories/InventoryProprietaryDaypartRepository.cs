using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryProprietaryDaypartRepository : IDataRepository
    {
        InventoryProprietaryDaypartDto GetInventoryProprietaryDaypartMappings(int inventorySourceId, int defaultDaypartId);
    }

    public class InventoryProprietaryDaypartRepository : BroadcastRepositoryBase, IInventoryProprietaryDaypartRepository
    {
        public InventoryProprietaryDaypartRepository
            (IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
             ITransactionHelper pTransactionHelper, 
             IConfigurationWebApiClient configurationWebApiClient) : 
                base(pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient)
        {
        }

        public InventoryProprietaryDaypartDto GetInventoryProprietaryDaypartMappings(int inventorySourceId, int defaultDaypartId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var mapping = context.inventory_proprietary_daypart_program_mappings
                        .Include(x => x.inventory_proprietary_daypart_programs)
                        .Where(x => x.inventory_source_id == inventorySourceId &&
                                    x.standard_daypart_id == defaultDaypartId)
                        .SingleOrDefault($"Too many Inventory proprietary mappings found for inventory source {inventorySourceId} and daypart {defaultDaypartId}.");

                    var result = mapping == null ? null : _MapToDto(mapping);
                    return result;
                });
        }

        private InventoryProprietaryDaypartDto _MapToDto(inventory_proprietary_daypart_program_mappings inventoryProprietaryMappings)
        {
            return new InventoryProprietaryDaypartDto
            {
                InventorySourceId = inventoryProprietaryMappings.inventory_source_id,
                DefaultDaypartId = inventoryProprietaryMappings.standard_daypart_id,
                ProgramName = inventoryProprietaryMappings.inventory_proprietary_daypart_programs.program_name,
                GenreId = inventoryProprietaryMappings.inventory_proprietary_daypart_programs.genre_id,
                ShowTypeId = inventoryProprietaryMappings.inventory_proprietary_daypart_programs.show_type_id
            };
        }
    }
}
