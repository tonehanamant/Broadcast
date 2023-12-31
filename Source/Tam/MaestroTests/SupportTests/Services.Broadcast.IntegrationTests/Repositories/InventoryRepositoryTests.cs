﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class InventoryRepositoryTests
    {
	    private readonly IGenreCache _GenreCache = IntegrationTestApplicationServiceFactory.Instance.Resolve<IGenreCache>();
        private readonly IShowTypeCache _ShowTypeCache = IntegrationTestApplicationServiceFactory.Instance.Resolve<IShowTypeCache>();
	    private readonly IProgramMappingRepository _ProgramMappingRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProgramMappingRepository>();

        /// <summary>
        /// Tests both delete within daterange and update.
        /// Within the delete only a subset are deleted.
        /// The result is a mix of old and new programs.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
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

            var newPrograms = _GetPrograms(stationInventoryManifestDaypartIds, newProgramCountPerManifestDaypart);
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            List<StationInventoryManifestDaypart> beforeManifestDayparts;
            List<StationInventoryManifestDaypart> afterManifestDayparts;

            List<StationInventoryManifestDaypartProgram> beforePrograms;
            List<StationInventoryManifestDaypartProgram> afterPrograms;

            using (new TransactionScopeWrapper())
            {
                beforePrograms = repo.GetDaypartProgramsForInventoryDayparts(stationInventoryManifestDaypartIds);
                // align their primary_program_ids
                var daypartsToUpdate = new List<StationInventoryManifestDaypart>();
                var daypartGroups = beforePrograms.GroupBy(s => s.StationInventoryManifestDaypartId);
                foreach (var daypartGroup in daypartGroups)
                {
                    var manifestDaypartId = daypartGroup.Key;
                    var primaryProgramId = daypartGroup.First().Id;

                    daypartsToUpdate.Add(new StationInventoryManifestDaypart { Id = manifestDaypartId, PrimaryProgramId = primaryProgramId });
                }
                repo.UpdatePrimaryProgramsForManifestDayparts(daypartsToUpdate);

                beforeManifestDayparts = repo.GetManifestDayparts(stationInventoryManifestDaypartIds);
                Assert.AreEqual(expectedBeforeProgramCount, beforePrograms.Count);

                // only deletes some of them.
                repo.DeleteInventoryPrograms(relatedStationInventoryManifestIds, startDate, endDate);
                // and create some more...
                repo.CreateInventoryPrograms(newPrograms, createdAtDate);

                afterPrograms = repo.GetDaypartProgramsForInventoryDayparts(stationInventoryManifestDaypartIds);
                afterManifestDayparts = repo.GetManifestDayparts(stationInventoryManifestDaypartIds);
            }

            Assert.AreEqual(expectedAfterProgramCount, afterPrograms.Count);

            var verifiableResult = new
            {
                beforePrograms,
                beforeManifestDayparts,
                afterPrograms,
                afterManifestDayparts
            };

            // contains a mix of old and new.
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(verifiableResult, _GetJsonSettings()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventoryByFileIdForProgramsProcessing()
        {
            /*** Arrange ***/
            const int testFileId = 251392;
            var createdAtDate = new DateTime(2019, 01, 25);
            const int newProgramCountPerManifestDaypart = 1;
            var stationInventoryManifestDaypartIds = new List<int>
            {
                576682,
                576683,
                576684,
                576685
            };

            var newPrograms = _GetPrograms(stationInventoryManifestDaypartIds, newProgramCountPerManifestDaypart);
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            List<StationInventoryManifest> results;
            using (new TransactionScopeWrapper())
            {
                repo.CreateInventoryPrograms(newPrograms, createdAtDate);

                /*** Act ***/
                results = repo.GetInventoryByFileIdForProgramsProcessing(testFileId);
            }

            /*** Assert ***/
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetJsonSettings()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventoryBySourceForProgramsProcessing()
        {
            /*** Arrange ***/
            const int inventorySourceId = 1;
            var weekIds = new List<int> {743, 744, 745};
            var createdAtDate = new DateTime(2019, 01, 25);
            const int newProgramCountPerManifestDaypart = 1;
            var stationInventoryManifestDaypartIds = new List<int>
            {
                576682,
                576683,
                576684,
                576685
            };

            var newPrograms = _GetPrograms(stationInventoryManifestDaypartIds, newProgramCountPerManifestDaypart);
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            List<StationInventoryManifest> results;
            using (new TransactionScopeWrapper())
            {
                repo.CreateInventoryPrograms(newPrograms, createdAtDate);

                /*** Act ***/
                results = repo.GetInventoryBySourceForProgramsProcessing(inventorySourceId, weekIds);
            }
            /*** Assert ***/
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetJsonSettings()));
        }

        [Test]
        [Category("short_running")]
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
                    ProgramSourceId = 2,
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
                repo.CreateInventoryPrograms(newPrograms, createdAtDate);
                
                var totalInventory = repo.GetInventoryBySourceForProgramsProcessing(inventorySourceId, weekIds);
                var withoutPrograms = repo.GetInventoryBySourceWithUnprocessedPrograms(inventorySourceId, weekIds);

                totalCount = totalInventory.Count;
                withoutProgramsCount = withoutPrograms.Count;
            }

            Assert.AreEqual(expectedRecordCountTotal, totalCount);
            Assert.AreEqual(expectedRecordCountWithoutPrograms, withoutProgramsCount);
        }

        [Test]
        [Category("long_running")]
        public void AddNewInventoryGroups_ConcurrentCallsTest()
        {
            const int fileId = 236561;
            var groupIdsThatShouldNotBeCleanedUp = new List<int> { 18558, 18559, 18560 };

            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            var groups = repo.GetStationInventoryGroupsByFileId(fileId);
            var manifestsCountBefore = groups.SelectMany(x => x.Manifests).Count();
            groups.ForEach(x =>
            {
                x.Id = null;
                x.Manifests.ForEach(m => m.Id = null);
            });

            var file = new InventoryFileBase
            {
                InventoryGroups = groups,
                InventorySource = new InventorySource { Id = 1 },
                Id = fileId
            };

            var task1 = Task.Run(() =>
            {
                repo.AddNewInventoryGroups(file);
            });

            var task2 = Task.Run(() =>
            {
                repo.AddNewInventoryGroups(file);
            });

            Task.WaitAll(task1, task2);

            var manifestsCountAfter = repo.GetStationInventoryGroupsByFileId(fileId).SelectMany(x => x.Manifests).Count();

            // we expect the initial number of manifests to be added twice
            Assert.AreEqual(manifestsCountBefore * 3, manifestsCountAfter);

            // CLEAN UP. Looks like TransactionScopeWrapper does not undo changes made in another thread so we need to clean up manually
            groups = repo.GetStationInventoryGroupsByFileId(fileId);
            var groupIdsToRemove = groups.Where(x => !groupIdsThatShouldNotBeCleanedUp.Contains(x.Id.Value)).Select(x => x.Id.Value).ToList();
            repo.RemoveManifestGroups(groupIdsToRemove);
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDistinctUnmappedProgramsTest()
        {
	        const int expectedCount = 2666;

	        var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
		        .GetDataRepository<IInventoryRepository>();

	        using (new TransactionScopeWrapper())
	        {
		        /*** Act ***/
                var result = repo.GetUnmappedPrograms();

		        var totalCount = result.Count;
		        var verifiableResult = new
		        {
			        result
                };

                /*** Assert ***/
                Assert.AreEqual(expectedCount, totalCount);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(verifiableResult));

            }
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetOrphanedManifestDaypartsTest()
        {
	      //  const int expectedCount = 123;

	        var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
		        .GetDataRepository<IInventoryRepository>();
	        var createdBy = "testUser";
	        var createdAt = new DateTime(2020, 10, 17, 8, 32, 12);
	        var genre = _GenreCache.GetMaestroGenreByName("News");
	        var showType = _ShowTypeCache.GetMaestroShowTypeLookupDtoByName("Mini-Movie");
	        var newProgramMapping = new ProgramMappingsDto
	        {
		        OriginalProgramName = "TestOriginalProgramName",
		        OfficialProgramName = "TestOfficialProgramName",
		        OfficialGenre = genre,
		        OfficialShowType = new ShowTypeDto
		        {
			        Id = showType.Id,
			        Name = showType.Display
		        }
	        };

	        var result = new List<StationInventoryManifestDaypart>();

            using (new TransactionScopeWrapper())
	        {
		        _ProgramMappingRepository.CreateProgramMappings(new List<ProgramMappingsDto> { newProgramMapping }, createdBy, createdAt);
                /*** Act ***/
                 result = repo.GetOrphanedManifestDayparts(message => Debug.WriteLine(message));
                

		        /*** Assert ***/
		      //  Assert.AreEqual(expectedCount, totalCount);
	        }

            var toValidate = result.Where(s => s.ProgramName.Equals("TestOfficialProgramName"));
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate));
        }
    }
}