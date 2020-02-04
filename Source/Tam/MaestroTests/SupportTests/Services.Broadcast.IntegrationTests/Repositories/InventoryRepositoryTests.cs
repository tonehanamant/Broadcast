using System;
using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class InventoryRepositoryTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void StationInventoryManifestDaypartProgramsUpdate()
        {
            const int expectedBeforeProgramCount = 9;
            const int newProgramCountPerManifestDaypart = 2;
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
                        GenreId = 57,
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

                repo.UpdateInventoryPrograms(newPrograms, createdAtDate, stationInventoryManifestDaypartIds, startDate, endDate);

                var afterPrograms = repo.GetDaypartProgramsForInventoryDayparts(stationInventoryManifestDaypartIds);

                Assert.AreEqual(newPrograms.Count, afterPrograms.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(afterPrograms, _GetJsonSettings()));
            }
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypartProgram), "Genre");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypartProgram), "GenreSourceId");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            return jsonSettings;
        }
    }
}