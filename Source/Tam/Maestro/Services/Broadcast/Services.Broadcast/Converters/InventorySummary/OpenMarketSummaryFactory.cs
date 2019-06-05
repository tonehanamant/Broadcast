using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public class OpenMarketSummaryFactory : BaseInventorySummaryAbstractFactory
    {
        private readonly IProgramRepository _ProgramRepository;

        public OpenMarketSummaryFactory(IInventoryRepository inventoryRepository,
                                        IInventorySummaryRepository inventorySummaryRepository,
                                        IQuarterCalculationEngine quarterCalculationEngine,
                                        IProgramRepository programRepository)

            : base(inventoryRepository, inventorySummaryRepository, quarterCalculationEngine)
        {
            _ProgramRepository = programRepository;
        }

        private DateTime? GetFileLastCreatedDate(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Max(x => (DateTime?)x.CreatedDate);
        }

        public override InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> manifests)
        {
            var quartersForInventoryAvailable = GetQuartersForInventoryAvailable(inventorySource);          
            var inventorySummaryManifestFiles = GetInventorySummaryManifestFiles(manifests);
            var manifestIds = manifests.Select(x => x.ManifestId).ToList();
            var totalPrograms = _ProgramRepository.GetUniqueProgramNamesByManifests(manifestIds)
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .Count();

            return new OpenMarketInventorySummaryDto
            {
                InventorySourceId = inventorySource.Id,
                InventorySourceName = inventorySource.Name,
                HasRatesAvailableForQuarter = manifests.Any(),
                Quarter = quarterDetail,
                TotalMarkets = GetTotalMarkets(manifests),
                TotalStations = GetTotalStations(manifests),
                TotalPrograms = totalPrograms,
                HouseholdImpressions = GetHouseholdImpressions(manifests, householdAudienceId),
                InventoryPostingBooks = GetInventoryPostingBooks(inventorySummaryManifestFiles),
                LastUpdatedDate = GetFileLastCreatedDate(inventorySummaryManifestFiles),
                RatesAvailableFromQuarter = quartersForInventoryAvailable.Item1,
                RatesAvailableToQuarter = quartersForInventoryAvailable.Item2
            };
        }
    }
}
