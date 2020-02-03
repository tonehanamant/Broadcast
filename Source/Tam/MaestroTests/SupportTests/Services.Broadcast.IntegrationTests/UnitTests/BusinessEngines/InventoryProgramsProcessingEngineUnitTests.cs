using System;
using System.Collections.Generic;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    public class InventoryProgramsProcessingEngineUnitTests
    {
        private Mock<IGenreRepository> _GenreRepo = new Mock<IGenreRepository>();
        private Mock<IInventoryRepository> _InventoryRepo = new Mock<IInventoryRepository>();
        private Mock<IInventoryFileRepository> _InventoryFileRepo = new Mock<IInventoryFileRepository>();
        private Mock<IInventoryProgramsByFileJobsRepository> _InventoryProgramsByFileJobsRepo = new Mock<IInventoryProgramsByFileJobsRepository>();
        private Mock<IInventoryProgramsBySourceJobsRepository> _InventoryProgramsBySourceJobsRepo = new Mock<IInventoryProgramsBySourceJobsRepository>();
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaWeekCache = new Mock<IMediaMonthAndWeekAggregateCache>();
        private Mock<IProgramGuideApiClient> _ProgramGuidClient = new Mock<IProgramGuideApiClient>();

        [Test]
        public void ProcessInventoryProgramsByFileJob_NoManifests()
        {
            const int jobId = 13;
            const int fileId = 12;
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };
            var manifests = new List<StationInventoryManifest>();
            
            var getStationInventoryManifestsByFileIdCalled = 0;
            _InventoryRepo.Setup(r => r.GetStationInventoryManifestsByFileId(It.IsAny<int>()))
                .Callback(() => getStationInventoryManifestsByFileIdCalled++)
                .Returns(manifests);

            _InventoryFileRepo.Setup(r => r.GetInventoryFileById(It.IsAny<int>()))
                .Returns(new InventoryFile {InventorySource = inventorySource});

            var inventoryProgramsByFileJobsRepoCalls = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.GetJob(It.IsAny<int>()))
                .Callback(() => inventoryProgramsByFileJobsRepoCalls++)
                .Returns<int>((id) => new InventoryProgramsByFileJob
                {
                    Id = id,
                    InventoryFileId = fileId,
                    Status = InventoryProgramsJobStatus.Queued,
                    QueuedAt = DateTime.Now,
                    QueuedBy = "TestUser"
                });

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);
            
            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(new List<GuideResponseElementDto>());

            var engine = _GetInventoryProgramsProcessingEngine();

            var results = engine.ProcessInventoryProgramsByFileJob(jobId);

            Assert.NotNull(results);
            Assert.AreEqual(1, inventoryProgramsByFileJobsRepoCalls);
            Assert.AreEqual(1, getStationInventoryManifestsByFileIdCalled);
            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(0, getProgramsForGuideCalled);
        }

        [Test]
        public void ProcessInventoryProgramsByFileJob_WithException()
        {
            const int jobId = 13;
            const int fileId = 12;
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };

            var getStationInventoryManifestsByFileIdCalled = 0;
            _InventoryRepo.Setup(r => r.GetStationInventoryManifestsByFileId(It.IsAny<int>()))
                .Callback(() => getStationInventoryManifestsByFileIdCalled++)
                .Throws(new Exception("TestException from test ProcessInventoryProgramsByFileJob_WithException"));

            _InventoryFileRepo.Setup(r => r.GetInventoryFileById(It.IsAny<int>()))
                .Returns(new InventoryFile { InventorySource = inventorySource });

            var inventoryProgramsByFileJobsRepoCalls = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.GetJob(It.IsAny<int>()))
                .Callback(() => inventoryProgramsByFileJobsRepoCalls++)
                .Returns<int>((id) => new InventoryProgramsByFileJob
                {
                    Id = id,
                    InventoryFileId = fileId,
                    Status = InventoryProgramsJobStatus.Queued,
                    QueuedAt = DateTime.Now,
                    QueuedBy = "TestUser"
                });

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(new List<GuideResponseElementDto>());

            var engine = _GetInventoryProgramsProcessingEngine();

            Assert.Throws<Exception>(() => engine.ProcessInventoryProgramsByFileJob(jobId));

            Assert.AreEqual(1, inventoryProgramsByFileJobsRepoCalls);
            Assert.AreEqual(1, getStationInventoryManifestsByFileIdCalled);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(1, setJobCompleteErrorCalled);
            Assert.AreEqual(0, getProgramsForGuideCalled);
        }

        [Test]
        public void ProcessInventoryProgramsByFileJob()
        {
            const int jobId = 13;
            const int fileId = 12;
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };
            var manifests = _GetManifests();
            var genres = _GetGenres();
            var guideResponse = _GetGuideResponse();

            var getStationInventoryManifestsByFileIdCalled = 0;
            _InventoryRepo.Setup(r => r.GetStationInventoryManifestsByFileId(It.IsAny<int>()))
                .Callback(() => getStationInventoryManifestsByFileIdCalled++)
                .Returns(manifests);
            var updateInventoryProgramsCalled = 0;
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() => updateInventoryProgramsCalled++);

            _InventoryFileRepo.Setup(r => r.GetInventoryFileById(It.IsAny<int>()))
                .Returns(new InventoryFile { InventorySource = inventorySource });

            var inventoryProgramsByFileJobsRepoCalls = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.GetJob(It.IsAny<int>()))
                .Callback(() => inventoryProgramsByFileJobsRepoCalls++)
                .Returns<int>((id) => new InventoryProgramsByFileJob
                {
                    Id = id,
                    InventoryFileId = fileId,
                    Status = InventoryProgramsJobStatus.Queued,
                    QueuedAt = DateTime.Now,
                    QueuedBy = "TestUser"
                });

            var getGenresBySourceIdCalled = 0;
            _GenreRepo.Setup(r => r.GetGenresBySourceId(It.IsAny<int>()))
                .Callback(() => getGenresBySourceIdCalled++)
                .Returns(genres);

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(guideResponse);

            var engine = _GetInventoryProgramsProcessingEngine();

            var results = engine.ProcessInventoryProgramsByFileJob(jobId);

            Assert.NotNull(results);
            Assert.AreEqual(1, inventoryProgramsByFileJobsRepoCalls);
            Assert.AreEqual(1, getStationInventoryManifestsByFileIdCalled);
            Assert.AreEqual(1, getGenresBySourceIdCalled);

            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(3, getProgramsForGuideCalled);
            Assert.AreEqual(3, updateInventoryProgramsCalled);
        }

        [Test]
        public void ProcessInventoryProgramsBySourceJob_NoManifests()
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

            var getInventoryManifestsBySourceAndMediaWeekCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryManifestsBySourceAndMediaWeek(It.IsAny<int>(), It.IsAny<List<int>>()))
                .Callback(() => getInventoryManifestsBySourceAndMediaWeekCalled++)
                .Returns(manifests);
            var getInventorySourceCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventorySource(It.IsAny<int>()))
                .Callback(() => getInventorySourceCalled++)
                .Returns(inventorySource);

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
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(new List<GuideResponseElementDto>());

            var engine = _GetInventoryProgramsProcessingEngine();

            var results = engine.ProcessInventoryProgramsBySourceJob(jobId);

            Assert.NotNull(results);
            Assert.AreEqual(1, inventoryProgramsBySourceJobsRepoCalls);
            Assert.AreEqual(1, getInventorySourceCalled);
            Assert.AreEqual(1, getDisplayMediaWeekByFlightCalled);
            Assert.AreEqual(1, getInventoryManifestsBySourceAndMediaWeekCalled);
            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(0, getProgramsForGuideCalled);
        }

        [Test]
        public void ProcessInventoryProgramsBySourceJob_WithException()
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

            var getInventoryManifestsBySourceAndMediaWeekCalled = 0;
            _InventoryRepo.Setup(r =>
                    r.GetInventoryManifestsBySourceAndMediaWeek(It.IsAny<int>(), It.IsAny<List<int>>()))
                .Callback(() => getInventoryManifestsBySourceAndMediaWeekCalled++)
                .Throws(new Exception("Test Exception from method ProcessInventoryProgramsBySourceJob_WithException"));
            var getInventorySourceCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventorySource(It.IsAny<int>()))
                .Callback(() => getInventorySourceCalled++)
                .Returns(inventorySource);

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
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(new List<GuideResponseElementDto>());

            var engine = _GetInventoryProgramsProcessingEngine();

            Assert.Throws<Exception>(() => engine.ProcessInventoryProgramsBySourceJob(jobId));

            Assert.AreEqual(1, inventoryProgramsBySourceJobsRepoCalls);
            Assert.AreEqual(1, getInventorySourceCalled);
            Assert.AreEqual(1, getDisplayMediaWeekByFlightCalled);
            Assert.AreEqual(1, getInventoryManifestsBySourceAndMediaWeekCalled);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(1, setJobCompleteErrorCalled);
            Assert.AreEqual(0, getProgramsForGuideCalled);
        }

        [Test]
        public void ProcessInventoryProgramsBySourceJob()
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
            var manifests = _GetManifests();
            var genres = _GetGenres();
            var guideResponse = _GetGuideResponse();

            var getInventoryManifestsBySourceAndMediaWeekCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryManifestsBySourceAndMediaWeek(It.IsAny<int>(), It.IsAny<List<int>>()))
                .Callback(() => getInventoryManifestsBySourceAndMediaWeekCalled++)
                .Returns(manifests);
            var getInventorySourceCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventorySource(It.IsAny<int>()))
                .Callback(() => getInventorySourceCalled++)
                .Returns(inventorySource);
            var updateInventoryProgramsCalled = 0;
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
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

            var getGenresBySourceIdCalled = 0;
            _GenreRepo.Setup(r => r.GetGenresBySourceId(It.IsAny<int>()))
                .Callback(() => getGenresBySourceIdCalled++)
                .Returns(genres);

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(guideResponse);

            var engine = _GetInventoryProgramsProcessingEngine();

            var results = engine.ProcessInventoryProgramsBySourceJob(jobId);

            Assert.NotNull(results);
            Assert.AreEqual(1, inventoryProgramsBySourceJobsRepoCalls);
            Assert.AreEqual(1, getInventorySourceCalled);
            Assert.AreEqual(1, getGenresBySourceIdCalled);
            Assert.AreEqual(1, getDisplayMediaWeekByFlightCalled);
            Assert.AreEqual(1, getInventoryManifestsBySourceAndMediaWeekCalled);
            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(3, getProgramsForGuideCalled);
            Assert.AreEqual(3, updateInventoryProgramsCalled);
        }

        private List<GuideResponseElementDto> _GetGuideResponse()
        {
            return new List<GuideResponseElementDto>
            {
                new GuideResponseElementDto
                {
                    RequestDaypartId = "R000001.M001.W1.D1",
                    Programs = new List<GuideResponseProgramDto>
                    {
                        new GuideResponseProgramDto
                        {
                            ProgramName = "ProgramOne",
                            SourceGenre = "SourceGenreOne",
                            ShowType = "ShowTypeOne",
                            SyndicationType = "SyndicationTypeOne",
                            Occurrences = 1,
                            StartDate = new DateTime(2020, 01, 01),
                            EndDate = new DateTime(2020, 01, 06),
                            StartTime = 3600 * 3,
                            EndTime = (3600 * 4) - 1
                        }
                    }
                },
                new GuideResponseElementDto
                {
                    RequestDaypartId = "R000002.M001.W1.D2",
                    Programs = new List<GuideResponseProgramDto>
                    {
                        new GuideResponseProgramDto
                        {
                            ProgramName = "ProgramTwo",
                            SourceGenre = "SourceGenreTwo",
                            ShowType = "ShowTypeTwo",
                            SyndicationType = "SyndicationTypeTwo",
                            Occurrences = 1,
                            StartDate = new DateTime(2020, 01, 08),
                            EndDate = new DateTime(2020, 01, 13),
                            StartTime = 3600 * 4,
                            EndTime = (3600 * 5) - 1
                        }
                    }
                },
                new GuideResponseElementDto
                {
                    RequestDaypartId = "R000003.M001.W2.D1",
                    Programs = new List<GuideResponseProgramDto>
                    {
                        new GuideResponseProgramDto
                        {
                            ProgramName = "ProgramThree",
                            SourceGenre = "SourceGenreThree",
                            ShowType = "ShowTypeThree",
                            SyndicationType = "SyndicationTypeThree",
                            Occurrences = 1,
                            StartDate = new DateTime(2020, 01, 01),
                            EndDate = new DateTime(2020, 01, 06),
                            StartTime = 3600 * 3,
                            EndTime = (3600 * 4) - 1
                        }
                    }
                },
                new GuideResponseElementDto
                {
                    RequestDaypartId = "R000004.M001.W2.D2",
                    Programs = new List<GuideResponseProgramDto>
                    {
                        new GuideResponseProgramDto
                        {
                            ProgramName = "ProgramFour",
                            SourceGenre = "SourceGenreFour",
                            ShowType = "ShowTypeFour",
                            SyndicationType = "SyndicationTypeFour",
                            Occurrences = 1,
                            StartDate = new DateTime(2020, 01, 08),
                            EndDate = new DateTime(2020, 01, 13),
                            StartTime = 3600 * 4,
                            EndTime = (3600 * 5) - 1
                        }
                    }
                },
                new GuideResponseElementDto
                {
                    RequestDaypartId = "R000005.M001.W3.D1",
                    Programs = new List<GuideResponseProgramDto>
                    {
                        new GuideResponseProgramDto
                        {
                            ProgramName = "ProgramFive",
                            SourceGenre = "SourceGenreFive",
                            ShowType = "ShowTypeFive",
                            SyndicationType = "SyndicationTypeFive",
                            Occurrences = 1,
                            StartDate = new DateTime(2020, 01, 01),
                            EndDate = new DateTime(2020, 01, 06),
                            StartTime = 3600 * 3,
                            EndTime = (3600 * 4) - 1
                        }
                    }
                },
                new GuideResponseElementDto
                {
                    RequestDaypartId = "R000006.M001.W3.D2",
                    Programs = new List<GuideResponseProgramDto>
                    {
                        new GuideResponseProgramDto
                        {
                            ProgramName = "ProgramSix",
                            SourceGenre = "SourceGenreSix",
                            ShowType = "ShowTypeSix",
                            SyndicationType = "SyndicationTypeSix",
                            Occurrences = 1,
                            StartDate = new DateTime(2020, 01, 08),
                            EndDate = new DateTime(2020, 01, 13),
                            StartTime = 3600 * 4,
                            EndTime = (3600 * 5) - 1
                        }
                    }
                }
            };
        }

        /// <remarks>Do this after you've setup all your data repository returns</remarks>
        private Mock<IDataRepositoryFactory> _GetDataRepositoryFactory()
        {
            _InventoryProgramsByFileJobsRepo.Setup(r => r.UpdateJobStatus(It.IsAny<int>(), It.IsAny<InventoryProgramsJobStatus>(), It.IsAny<string>()));
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.UpdateJobStatus(It.IsAny<int>(), It.IsAny<InventoryProgramsJobStatus>(), It.IsAny<string>()));

            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IGenreRepository>()).Returns(_GenreRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryRepository>()).Returns(_InventoryRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryFileRepository>()).Returns(_InventoryFileRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryProgramsByFileJobsRepository>()).Returns(_InventoryProgramsByFileJobsRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryProgramsBySourceJobsRepository>()).Returns(_InventoryProgramsBySourceJobsRepo.Object);

            return dataRepoFactory;
        }

        private InventoryProgramsProcessingEngine _GetInventoryProgramsProcessingEngine()
        {
            var dataRepoFactory = _GetDataRepositoryFactory();
            var engine = new InventoryProgramsProcessingEngineTestClass(
                dataRepoFactory.Object, _MediaWeekCache.Object, _ProgramGuidClient.Object);
            return engine;
        }

        private List<LookupDto> _GetGenres()
        {
            return new List<LookupDto>
            {
                new LookupDto(1, "SourceGenreOne"),
                new LookupDto(2, "SourceGenreTwo"),
                new LookupDto(3, "SourceGenreThree"),
                new LookupDto(4, "SourceGenreFour"),
                new LookupDto(4, "SourceGenreFive"),
                new LookupDto(4, "SourceGenreSix"),
            };
        }

        private List<StationInventoryManifest> _GetManifests()
        {
            var result = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    Id = 1,
                    Station = new DisplayBroadcastStation(),
                    ManifestDayparts = new List<StationInventoryManifestDaypart>
                    {
                        new StationInventoryManifestDaypart
                        {
                            Id = 1,
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 0,
                                EndTime = (3600 * 3) - 1
                            }
                        },
                        new StationInventoryManifestDaypart
                        {
                            Id = 2,
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 3600 * 3,
                                EndTime = (3600 * 5) - 1
                            }
                        }
                    },
                    ManifestWeeks = new List<StationInventoryManifestWeek>
                    {
                        new StationInventoryManifestWeek
                        {
                            Id = 1,
                            MediaWeek = new MediaWeek
                            {
                                Id = 1,
                                StartDate = new DateTime(2020, 01, 01),
                                EndDate = new DateTime(2020, 01, 07)
                            },
                            StartDate = new DateTime(2020, 01, 01),
                            EndDate = new DateTime(2020, 01, 07)
                        },
                        new StationInventoryManifestWeek
                        {
                            Id = 2,
                            MediaWeek = new MediaWeek
                            {
                                Id = 2,
                                StartDate = new DateTime(2020, 01, 08),
                                EndDate = new DateTime(2020, 01, 14)
                            },
                            StartDate = new DateTime(2020, 01, 08),
                            EndDate = new DateTime(2020, 01, 14)
                        },
                        new StationInventoryManifestWeek
                        {
                            Id = 3,
                            MediaWeek = new MediaWeek
                            {
                                Id = 3,
                                StartDate = new DateTime(2020, 01, 15),
                                EndDate = new DateTime(2020, 01, 21)
                            },
                            StartDate = new DateTime(2020, 01, 15),
                            EndDate = new DateTime(2020, 01, 21)
                        }
                    }
                },
            };
            return result;
        }
    }
}