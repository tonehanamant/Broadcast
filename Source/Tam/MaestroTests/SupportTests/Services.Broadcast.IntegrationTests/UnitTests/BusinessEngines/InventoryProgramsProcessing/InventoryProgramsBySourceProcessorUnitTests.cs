using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    [TestFixture]
    [Category("short_running")]
    public class InventoryProgramsBySourceProcessorUnitTests
    {
        private Mock<IInventoryRepository> _InventoryRepo = new Mock<IInventoryRepository>();
        private Mock<IInventoryProgramsBySourceJobsRepository> _InventoryProgramsBySourceJobsRepo = new Mock<IInventoryProgramsBySourceJobsRepository>();
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaWeekCache = new Mock<IMediaMonthAndWeekAggregateCache>();
        private Mock<IGenreCache> _GenreCacheMock = new Mock<IGenreCache>();

        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
        private Mock<IFeatureToggleHelper> _FeatureToggleHelper = new Mock<IFeatureToggleHelper>();

        [Test]
        public void BySourceJob_NoManifests()
        {
            const int jobId = 13;
            const int sourceID = 1;
            var startDate = new DateTime(2020, 01, 01);
            var endDate = new DateTime(2020, 01, 21);

            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };
            var manifests = new List<StationInventoryManifest>();

            var GetInventoryBySourceForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryBySourceForProgramsProcessing(It.IsAny<int>(), It.IsAny<List<int>>()))
                .Callback(() => GetInventoryBySourceForProgramsProcessingCalled++)
                .Returns(manifests);
            var getInventorySourceCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventorySource(It.IsAny<int>()))
                .Callback(() => getInventorySourceCalled++)
                .Returns(inventorySource);
            var deleteInventoryProgramsFromManifestDaypartsCalled = 0;
            _InventoryRepo.Setup(r => r.DeleteInventoryPrograms(It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() => deleteInventoryProgramsFromManifestDaypartsCalled++);
            var updateInventoryProgramsCalled = 0;
            _InventoryRepo.Setup(r => r.CreateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
                .Callback(() => updateInventoryProgramsCalled++);

            var inventoryProgramsBySourceJobsRepoCalls = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.GetJob(It.IsAny<int>()))
                .Callback(() => inventoryProgramsBySourceJobsRepoCalls++)
                .Returns<int>((id) => new InventoryProgramsBySourceJob
                {
                    Id = id,
                    InventorySourceId = sourceID,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = InventoryProgramsJobStatus.Queued,
                    QueuedAt = DateTime.Now,
                    QueuedBy = "TestUser"
                });

            var getDisplayMediaWeekByFlightCalled = 0;
            _MediaWeekCache.Setup(c => c.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() => getDisplayMediaWeekByFlightCalled++)
                .Returns(new List<DisplayMediaWeek>
                {
                    new DisplayMediaWeek{Id = 1},
                    new DisplayMediaWeek{Id = 2},
                    new DisplayMediaWeek{Id = 3}
                });

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteWarningCalled++);

            var engine = _GetInventoryProgramsProcessingEngine();

            var results = engine.ProcessInventoryJob(jobId);

            Assert.NotNull(results);
            Assert.AreEqual(1, GetInventoryBySourceForProgramsProcessingCalled);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(1, setJobCompleteWarningCalled);
            Assert.AreEqual(0, deleteInventoryProgramsFromManifestDaypartsCalled);
            Assert.AreEqual(0, updateInventoryProgramsCalled);
        }

        [Test]
        public void BySourceJob_WithException()
        {
            const int jobId = 13;
            const int sourceID = 1;
            var startDate = new DateTime(2020, 01, 01);
            var endDate = new DateTime(2020, 01, 21);

            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };

            var GetInventoryBySourceForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r =>
                    r.GetInventoryBySourceForProgramsProcessing(It.IsAny<int>(), It.IsAny<List<int>>()))
                .Callback(() => GetInventoryBySourceForProgramsProcessingCalled++)
                .Throws(new Exception("Test Exception from method ProcessInventoryProgramsBySourceJob_WithException"));
            var getInventorySourceCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventorySource(It.IsAny<int>()))
                .Callback(() => getInventorySourceCalled++)
                .Returns(inventorySource);
            var deleteInventoryProgramsFromManifestDaypartsCalled = 0;
            _InventoryRepo.Setup(r => r.DeleteInventoryPrograms(It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() => deleteInventoryProgramsFromManifestDaypartsCalled++);
            var createInventoryProgramsCalled = 0;
            _InventoryRepo.Setup(r => r.CreateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
                .Callback(() => createInventoryProgramsCalled++);

            var inventoryProgramsBySourceJobsRepoCalls = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.GetJob(It.IsAny<int>()))
                .Callback(() => inventoryProgramsBySourceJobsRepoCalls++)
                .Returns<int>((id) => new InventoryProgramsBySourceJob
                {
                    Id = id,
                    InventorySourceId = sourceID,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = InventoryProgramsJobStatus.Queued,
                    QueuedAt = DateTime.Now,
                    QueuedBy = "TestUser"
                });

            var getDisplayMediaWeekByFlightCalled = 0;
            _MediaWeekCache.Setup(c => c.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() => getDisplayMediaWeekByFlightCalled++)
                .Returns(new List<DisplayMediaWeek>
                {
                    new DisplayMediaWeek{Id = 1},
                    new DisplayMediaWeek{Id = 2},
                    new DisplayMediaWeek{Id = 3}
                });

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);

            var engine = _GetInventoryProgramsProcessingEngine();

            Assert.Throws<Exception>(() => engine.ProcessInventoryJob(jobId));

            Assert.AreEqual(1, GetInventoryBySourceForProgramsProcessingCalled);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(1, setJobCompleteErrorCalled);
            Assert.AreEqual(0, deleteInventoryProgramsFromManifestDaypartsCalled);
            Assert.AreEqual(0, createInventoryProgramsCalled);
        }

        [Test]
        public void BySourceJob()
        {
            /*** Arrange ***/
            const int jobId = 13;
            const int sourceId = 1;
            var startDate = new DateTime(2020, 01, 01);
            var endDate = new DateTime(2020, 01, 21);
            const int testManifestCount = 8;
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };
            var mediaWeeks = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek {Id = 1},
                new DisplayMediaWeek {Id = 2},
                new DisplayMediaWeek {Id = 3}
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(testManifestCount);

            var GetInventoryBySourceForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryBySourceForProgramsProcessing(It.IsAny<int>(), It.IsAny<List<int>>()))
                .Callback(() => GetInventoryBySourceForProgramsProcessingCalled++)
                .Returns(manifests);
            var getInventorySourceCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventorySource(It.IsAny<int>()))
                .Callback(() => getInventorySourceCalled++)
                .Returns(inventorySource);

            var deleteProgramsCalls = new List<Tuple<List<int>, DateTime, DateTime>>();
            _InventoryRepo.Setup(r => r.DeleteInventoryPrograms(It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<List<int>, DateTime, DateTime>((a, b, c) => deleteProgramsCalls.Add(new Tuple<List<int>, DateTime, DateTime>(a, b, c)));

            var createProgramsCalls = new List<Tuple<List<StationInventoryManifestDaypartProgram>, DateTime>>();
            _InventoryRepo.Setup(r => r.CreateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((a, b) =>
                    createProgramsCalls.Add(new Tuple<List<StationInventoryManifestDaypartProgram>, DateTime>(a, b)));

            var inventoryProgramsBySourceJobsRepoCalls = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.GetJob(It.IsAny<int>()))
                .Callback(() => inventoryProgramsBySourceJobsRepoCalls++)
                .Returns<int>((id) => new InventoryProgramsBySourceJob
                {
                    Id = id,
                    JobGroupId = new Guid("33a4940e-e0e7-4ccd-9dda-de0063b3ab40"),
                    InventorySourceId = sourceId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = InventoryProgramsJobStatus.Queued,
                    QueuedAt = DateTime.Now,
                    QueuedBy = "TestUser"
                });

            var getDisplayMediaWeekByFlightCalled = 0;
            _MediaWeekCache.Setup(c => c.GetDisplayMediaWeekByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() => getDisplayMediaWeekByFlightCalled++)
                .Returns(mediaWeeks);

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteWarningCalled++);

            var engine = _GetInventoryProgramsProcessingEngine();
            engine.UT_CurrentDateTime = new DateTime(2020, 03, 06, 14, 22, 35);

            /*** Act ***/
            var results = engine.ProcessInventoryJob(jobId);

            /*** Assert ***/
            Assert.NotNull(results);
            Assert.IsTrue(getInventorySourceCalled > 0);
            Assert.AreEqual(1, getDisplayMediaWeekByFlightCalled);
            Assert.IsTrue(inventoryProgramsBySourceJobsRepoCalls > 0);
            Assert.AreEqual(1, GetInventoryBySourceForProgramsProcessingCalled);

            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
        }        

        /// <remarks>Do this after you've setup all your data repository returns</remarks>
        private Mock<IDataRepositoryFactory> _GetDataRepositoryFactory()
        {
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.UpdateJobStatus(It.IsAny<int>(), It.IsAny<InventoryProgramsJobStatus>(), It.IsAny<string>()));

            _InventoryRepo
                .Setup(x => x.GetDaypartProgramsForInventoryDayparts(It.IsAny<List<int>>()))
                .Returns<List<int>>(x =>
                {
                    var result = new List<StationInventoryManifestDaypartProgram>();

                    foreach (var item in x)
                    {
                        result.Add(new StationInventoryManifestDaypartProgram
                        {
                            Id = 1,
                            StationInventoryManifestDaypartId = item,
                            ProgramName = "ProgramName",
                            ShowType = "ShowType",
                            SourceGenreId = 1,
                            ProgramSourceId = 2,
                            MaestroGenreId = 2,
                            StartTime = 7200,
                            EndTime = 7300
                        });

                        result.Add(new StationInventoryManifestDaypartProgram
                        {
                            Id = 2,
                            StationInventoryManifestDaypartId = item,
                            ProgramName = "ProgramName1",
                            ShowType = "ShowType",
                            SourceGenreId = 1,
                            ProgramSourceId = 2,
                            MaestroGenreId = 2,
                            StartTime = 10800,
                            EndTime = 14400
                        });
                    }

                    return result;
                });

            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryRepository>()).Returns(_InventoryRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryProgramsBySourceJobsRepository>()).Returns(_InventoryProgramsBySourceJobsRepo.Object);

            return dataRepoFactory;
        }

        private InventoryProgramsBySourceProcessorTestClass _GetInventoryProgramsProcessingEngine()
        {
            var dataRepoFactory = _GetDataRepositoryFactory();
            
            _GenreCacheMock
                .Setup(x => x.GetSourceGenreLookupDtoByName(It.IsAny<string>(), It.IsAny<ProgramSourceEnum>()))
                .Returns<string, ProgramSourceEnum>((p1, p2) => new LookupDto
                {
                    Id = 1,
                    Display = $"{p2.ToString()} Genre"
                });

            _GenreCacheMock
                .Setup(x => x.GetMaestroGenreLookupDtoBySourceGenre(It.IsAny<LookupDto>(), It.IsAny<ProgramSourceEnum>()))
                .Returns(new LookupDto
                {
                    Id = 2,
                    Display = "Maestro Genre"
                });

            var engine = new InventoryProgramsBySourceProcessorTestClass(
                dataRepoFactory.Object, 
                _MediaWeekCache.Object,
                _GenreCacheMock.Object,
                _FeatureToggleHelper.Object,
                _ConfigurationSettingsHelper.Object);

            return engine;
        }
    }
}