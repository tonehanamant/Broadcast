using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class InventoryExportRepositoryTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryForExportOpenMarket()
        {
            /*** Arrange ***/

            var spotLengthIds = new List<int> { 1 };
            var weekIds = new List<int> { 747, 748, 749 }; 
            var genreIds = new List<int> { 39 }; 
            var createdAtDate = new DateTime(2019, 01, 25);

            const int newProgramCountPerManifestDaypart = 1;
            var stationInventoryManifestDaypartIds = new List<int>
            {
                576682,
                576683
            };

            var newPrograms = _GetPrograms(stationInventoryManifestDaypartIds, newProgramCountPerManifestDaypart);
            var inventoryRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            var inventoryExportRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryExportRepository>();

            List<InventoryExportDto> results;
            using (new TransactionScopeWrapper())
            {
                using (var innerTrans = new TransactionScopeWrapper())
                {
                    // create the programs
                    inventoryRepo.CreateInventoryPrograms(newPrograms, createdAtDate);

                    // make them primary on the dayparts
                    var dayparts = new List<StationInventoryManifestDaypart>();
                    foreach (var id in stationInventoryManifestDaypartIds)
                    {
                        // last to ensure it's the programs we think.
                        var primaryProgram = inventoryRepo.GetDaypartProgramsForInventoryDayparts(new List<int> { id }).Last();
                        var daypart = new StationInventoryManifestDaypart
                        {
                            Id = id,
                            PrimaryProgramId = primaryProgram.Id
                        };
                        dayparts.Add(daypart);
                    }
                    inventoryRepo.UpdatePrimaryProgramsForManifestDayparts(dayparts);
                    innerTrans.Complete();
                }

                /*** Act ***/
                results = inventoryExportRepo.GetInventoryForExportOpenMarket(spotLengthIds, genreIds, weekIds);
            }
            /*** Assert ***/
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetJsonSettings()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryForExportOpenMarketNotEnriched()
        {
            /*** Arrange ***/

            var spotLengthIds = new List<int> { 1 };
            var weekIds = new List<int> { 747, 748, 749 };
            var stationInventoryManifestDaypartIds = new List<int> { 576683 };

            var inventoryRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            var inventoryExportRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryExportRepository>();

            List<InventoryExportDto> results;
            using (new TransactionScopeWrapper())
            {
                inventoryRepo.DeleteInventoryPrograms(stationInventoryManifestDaypartIds);

                /*** Act ***/
                results = inventoryExportRepo.GetInventoryForExportOpenMarketNotEnriched(spotLengthIds, weekIds);
            }
            /*** Assert ***/
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetJsonSettings()));
        }

        private List<StationInventoryManifestDaypartProgram> _GetPrograms(List<int> stationInventoryManifestDaypartIds, int newProgramCountPerManifestDaypart)
        {
            var startDate = new DateTime(2019, 02, 01);
            var endDate = new DateTime(2019, 02, 12);

            var newPrograms = new List<StationInventoryManifestDaypartProgram>();
            foreach (var daypartId in stationInventoryManifestDaypartIds)
            {
                for (var i = 0; i < newProgramCountPerManifestDaypart; i++)
                {
                    var newProgram = new StationInventoryManifestDaypartProgram
                    {
                        StationInventoryManifestDaypartId = daypartId,
                        ProgramName = $"Program {(i + 1)} For {daypartId}",
                        ShowType = "movie",
                        SourceGenreId = 57,
                        ProgramSourceId = 2,
                        MaestroGenreId = 39,
                        StartDate = startDate,
                        EndDate = endDate,
                        StartTime = 3600,
                        EndTime = 5400
                    };
                    newPrograms.Add(newProgram);
                }
            }

            return newPrograms;
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypartProgram), "Genre");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypartProgram), "ProgramSourceId");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypartProgram), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypartProgram), "StationInventoryManifestDaypartId");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
            jsonResolver.Ignore(typeof(DaypartDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            return jsonSettings;
        }
    }
}