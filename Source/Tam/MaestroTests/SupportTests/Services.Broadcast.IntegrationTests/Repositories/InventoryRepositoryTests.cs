using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Plan;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class InventoryRepositoryTests
    {
        /// <summary>
        /// Tests both delete within daterange and update.
        /// Within the delete only a subset are deleted.
        /// The result is a mix of old and new programs.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void StationInventoryManifestDaypartProgramsUpdate()
        {
            const int expectedBeforeProgramCount = 9;
            const int newProgramCountPerManifestDaypart = 6;
            const int expectedAfterProgramCount = 58;
            var startDate = new DateTime(2019, 02, 01);
            var endDate = new DateTime(2019, 02, 12);
            var createdAtDate = new DateTime(2019,01, 25);
            var stationInventoryManifestDaypartIds = new List<int>
            {
                358772,
                358773,
                358776,
                358777,
                358780,
                360318,
                360319,
                360320,
                360321
            };
            var relatedStationInventoryManifestIds = new List<int>
            {
                287146,
                287147,
                287150,
                287151,
                287154,
                288692,
                288693,
                288694,
                288695
            };

            var newPrograms = new List<StationInventoryManifestDaypartProgram>();
            foreach (var daypartId in stationInventoryManifestDaypartIds)
            {
                for (var i = 0; i < newProgramCountPerManifestDaypart; i++)
                {
                    var newProgram = new StationInventoryManifestDaypartProgram
                    {
                        StationInventoryManifestDaypartId = daypartId,
                        ProgramName = $"Program {(i+1)} For {daypartId}",
                        ShowType = "movie",
                        SourceGenreId = 57,
                        GenreSourceId = 2,
                        MaestroGenreId = 39,
                        StartDate = startDate,
                        EndDate = endDate,
                        StartTime = 3600,
                        EndTime = 5400
                    };
                    newPrograms.Add(newProgram);
                }
            }

            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            using (new TransactionScopeWrapper())
            {
                var beforePrograms = repo.GetDaypartProgramsForInventoryDayparts(stationInventoryManifestDaypartIds);
                Assert.AreEqual(expectedBeforeProgramCount, beforePrograms.Count);

                // only deletes some of them.
                repo.DeleteInventoryPrograms(relatedStationInventoryManifestIds, startDate, endDate);
                repo.UpdateInventoryPrograms(newPrograms, createdAtDate);

                var afterPrograms = repo.GetDaypartProgramsForInventoryDayparts(stationInventoryManifestDaypartIds);

                Assert.AreEqual(expectedAfterProgramCount, afterPrograms.Count);
                // contains a mix of old and new.
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(afterPrograms, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryByFileIdForProgramsProcessing()
        {
            const int testFileId = 251392;
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            using (new TransactionScopeWrapper())
            {
                var results = repo.GetInventoryByFileIdForProgramsProcessing(testFileId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryBySourceForProgramsProcessing()
        {
            const int inventorySourceId = 1;
            var weekIds = new List<int> {743, 744, 745};

            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            using (new TransactionScopeWrapper())
            {
                var results = repo.GetInventoryBySourceForProgramsProcessing(inventorySourceId, weekIds);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetJsonSettings()));
            }
        }

        [Test]
        public void GetInventoryBySourceWithUnprocessedPrograms()
        {
            const int inventorySourceId = 1;
            var weekIds = new List<int> { 768, 769, 770 };
            
            const int expectedRecordCountTotal = 8;
            const int expectedRecordCountWithoutPrograms = 3;

            var startDate = new DateTime(2019, 02, 01);
            var endDate = new DateTime(2019, 02, 12);
            var createdAtDate = new DateTime(2019, 01, 25);
            var stationInventoryManifestDaypartIdsToAddProgramsTo = new List<int>
            {
                353961,
                353961,
                353961,
                353962,
                353962,
                353962,
                353963,
                353963,
                353963,
                358289,
                358289,
                358289,
                358290,
                358290
            };

            var newPrograms = new List<StationInventoryManifestDaypartProgram>();
            foreach (var daypartId in stationInventoryManifestDaypartIdsToAddProgramsTo)
            {
                var newProgram = new StationInventoryManifestDaypartProgram
                {
                    StationInventoryManifestDaypartId = daypartId,
                    ProgramName = $"Program For {daypartId}",
                    ShowType = "movie",
                    SourceGenreId = 57,
                    GenreSourceId = 2,
                    MaestroGenreId = 39,
                    StartDate = startDate,
                    EndDate = endDate,
                    StartTime = 3600,
                    EndTime = 5400
                };
                newPrograms.Add(newProgram);
            }
            
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            int totalCount;
            int withoutProgramsCount;

            using (new TransactionScopeWrapper())
            {
                // create the programs for our test.
                repo.UpdateInventoryPrograms(newPrograms, createdAtDate);
                
                var totalInventory = repo.GetInventoryBySourceForProgramsProcessing(inventorySourceId, weekIds);
                var withoutPrograms = repo.GetInventoryBySourceWithUnprocessedPrograms(inventorySourceId, weekIds);

                totalCount = totalInventory.Count;
                withoutProgramsCount = withoutPrograms.Count;
            }

            Assert.AreEqual(expectedRecordCountTotal, totalCount);
            Assert.AreEqual(expectedRecordCountWithoutPrograms, withoutProgramsCount);
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypartProgram), "Genre");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypartProgram), "GenreSourceId");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypartProgram), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            return jsonSettings;
        }
    }
}