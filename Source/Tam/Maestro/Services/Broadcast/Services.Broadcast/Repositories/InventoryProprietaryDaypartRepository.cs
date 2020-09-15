using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using System.Data.Entity;
using Common.Services.Extensions;

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
                    var inventoryProprietaryMappings = context.inventory_proprietary_daypart_program_mappings
                                .Include(x => x.inventory_proprietary_daypart_programs)
                                .Where(x => x.inventory_source_id == inventorySourceId &&
                                            x.daypart_default_id == defaultDaypartId)
                                 .Single($"Inventory proprietary mappings not found for inventory source {inventorySourceId} and daypart {defaultDaypartId}");

                    return _MapToDto(inventoryProprietaryMappings);
                });
        }

        private InventoryProprietaryDaypartDto _MapToDto(inventory_proprietary_daypart_program_mappings inventoryProprietaryMappings)
        {
            return new InventoryProprietaryDaypartDto
            {
                InventorySourceId = inventoryProprietaryMappings.inventory_source_id,
                DefaultDaypartId = inventoryProprietaryMappings.daypart_default_id,
                ProgramName = inventoryProprietaryMappings.inventory_proprietary_daypart_programs.program_name,
                GenreId = inventoryProprietaryMappings.inventory_proprietary_daypart_programs.genre_id,
                ShowTypeId = inventoryProprietaryMappings.inventory_proprietary_daypart_programs.show_type_id
            };
        }
    }
}
