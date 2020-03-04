using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    [TestFixture]
    public class InventoryProgramsProcessingEngineUnitTests
    {
        private Mock<IGenreRepository> _GenreRepo = new Mock<IGenreRepository>();
        private Mock<IInventoryRepository> _InventoryRepo = new Mock<IInventoryRepository>();
        private Mock<IInventoryProgramsBySourceJobsRepository> _InventoryProgramsBySourceJobsRepo = new Mock<IInventoryProgramsBySourceJobsRepository>();
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaWeekCache = new Mock<IMediaMonthAndWeekAggregateCache>();
        private Mock<IProgramGuideApiClient> _ProgramGuidClient = new Mock<IProgramGuideApiClient>();
        private Mock<IStationMappingService> _StationMappingService = new Mock<IStationMappingService>();
        
        /// <summary>
        /// Exercises the ability to call in parallel.
        /// We expect 1 call Per entry.
        /// </summary>
        [Test]
        public void BySourceJob_ParallelEnabled()
        {
            /*** Arrange ***/
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
            var mediaWeeks = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek {Id = 1},
                new DisplayMediaWeek {Id = 2},
                new DisplayMediaWeek {Id = 3}
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            var genres = InventoryProgramsProcessingTestHelper.GetGenres();
            var guideResponse = _GetGuideResponse();

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
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
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
                .Returns(mediaWeeks);

            var getGenresBySourceIdCalled = 0;
            _GenreRepo.Setup(r => r.GetGenresBySourceId(It.IsAny<int>()))
                .Callback(() => getGenresBySourceIdCalled++)
                .Returns(genres);

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteWarningCalled++);

            var getProgramsForGuidCallCount = 0;
            var guideRequests = new ConcurrentBag<Tuple<int, List<GuideRequestElementDto>>>();
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback<List<GuideRequestElementDto>>((r) =>
                {
                    getProgramsForGuidCallCount++;
                    guideRequests.Add(new Tuple<int, List<GuideRequestElementDto>>(Thread.CurrentThread.ManagedThreadId, r));
                })
                .Returns(guideResponse);

            var mappedStations = new List<StationMappingsDto>
            {
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Sigma, MapValue = "SigmaMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSI, MapValue = "NSIMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSILegacy, MapValue = "NSILegacyMappedValue"}
            };
            _StationMappingService.Setup(s => s.GetStationMappingsByCadentCallLetter(It.IsAny<string>()))
                .Returns(mappedStations);

            var engine = _GetInventoryProgramsProcessingEngine();
            engine.UT_ParallelApiCallsEnabled = true; // enable parallel for this test
            engine.UT_ParallelApiCallsBatchSize = 1;

            /*** Act ***/
            var results = engine.ProcessInventoryJob(jobId);

            /*** Assert ***/
            Assert.NotNull(results);
            Assert.IsTrue(inventoryProgramsBySourceJobsRepoCalls > 0);
            Assert.AreEqual(1, getInventorySourceCalled);
            Assert.AreEqual(1, getGenresBySourceIdCalled);
            Assert.AreEqual(1, getDisplayMediaWeekByFlightCalled);
            Assert.AreEqual(1, GetInventoryBySourceForProgramsProcessingCalled);
            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(1, deleteInventoryProgramsFromManifestDaypartsCalled);
            Assert.AreEqual(1, updateInventoryProgramsCalled);
            // verify was multi=threaded
            var distinctThreadIdsCount = guideRequests.Select(s => s.Item1).Distinct().Count();
            Assert.IsTrue(distinctThreadIdsCount > 1);

            Assert.AreEqual(4, guideRequests.Count); // batches
            Assert.AreEqual(1, guideRequests.ToList()[0].Item2.Count); // per batch
            Assert.AreEqual(1, guideRequests.ToList()[1].Item2.Count); // per batch
            Assert.AreEqual(1, guideRequests.ToList()[2].Item2.Count); // per batch
            Assert.AreEqual(1, guideRequests.ToList()[3].Item2.Count); // per batch
            // all dayparts are there
            var requestIds = guideRequests.SelectMany(s => s.Item2).Select(s => s.Id).ToList();
            Assert.IsTrue(requestIds.Contains("R000001.M001.D1"));
            Assert.IsTrue(requestIds.Contains("R000002.M001.D2"));
            Assert.IsTrue(requestIds.Contains("R000003.M002.D3"));
            Assert.IsTrue(requestIds.Contains("R000004.M002.D4"));
        }

        /// <summary>
        /// Verifies parallel calls are batched.
        /// </summary>
        [Test]
        public void BySourceJob_ParallelEnabled_BatchSize()
        {
            /*** Arrange ***/
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
            var mediaWeeks = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek {Id = 1},
                new DisplayMediaWeek {Id = 2},
                new DisplayMediaWeek {Id = 3}
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            var genres = InventoryProgramsProcessingTestHelper.GetGenres();
            var guideResponse = _GetGuideResponse();

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
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
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
                .Returns(mediaWeeks);

            var getGenresBySourceIdCalled = 0;
            _GenreRepo.Setup(r => r.GetGenresBySourceId(It.IsAny<int>()))
                .Callback(() => getGenresBySourceIdCalled++)
                .Returns(genres);

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteWarningCalled++);

            var guideRequests = new ConcurrentBag<Tuple<int, List<GuideRequestElementDto>>>();
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback<List<GuideRequestElementDto>>((r) => guideRequests.Add(new Tuple<int, List<GuideRequestElementDto>>(Thread.CurrentThread.ManagedThreadId, r)))
                .Returns(guideResponse);

            var mappedStations = new List<StationMappingsDto>
            {
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Sigma, MapValue = "SigmaMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSI, MapValue = "NSIMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSILegacy, MapValue = "NSILegacyMappedValue"}
            };
            _StationMappingService.Setup(s => s.GetStationMappingsByCadentCallLetter(It.IsAny<string>()))
                .Returns(mappedStations);

            var engine = _GetInventoryProgramsProcessingEngine();
            engine.UT_ParallelApiCallsEnabled = true; // enable parallel for this test
            engine.UT_ParallelApiCallsBatchSize = 2;

            /*** Act ***/
            var results = engine.ProcessInventoryJob(jobId);

            /*** Assert ***/
            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(1, deleteInventoryProgramsFromManifestDaypartsCalled);
            Assert.AreEqual(1, updateInventoryProgramsCalled);
            // verify was multi=threaded
            var distinctThreadIdsCount = guideRequests.Select(s => s.Item1).Distinct().Count();
            Assert.IsTrue(distinctThreadIdsCount > 1);

            Assert.AreEqual(2, guideRequests.Count); // 2 batches
            Assert.AreEqual(2, guideRequests.ToList()[0].Item2.Count); // 2 per batch
            Assert.AreEqual(2, guideRequests.ToList()[1].Item2.Count); // 2 per batch
            // all dayparts are there
            var requestIds = guideRequests.SelectMany(s => s.Item2).Select(s => s.Id).ToList();
            Assert.IsTrue(requestIds.Contains("R000001.M001.D1"));
            Assert.IsTrue(requestIds.Contains("R000002.M001.D2"));
            Assert.IsTrue(requestIds.Contains("R000003.M002.D3"));
            Assert.IsTrue(requestIds.Contains("R000004.M002.D4"));
        }

        /// <summary>
        /// Test when an exception occurs and parallel is enabled.
        /// We expect one exception to be thrown having inner exceptions for the actual.
        /// </summary>
        [Test]
        public void BySourceJob_ParallelEnabled_WithException()
        {
            /*** Arrange ***/
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
            var mediaWeeks = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek {Id = 1},
                new DisplayMediaWeek {Id = 2},
                new DisplayMediaWeek {Id = 3}
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            var genres = InventoryProgramsProcessingTestHelper.GetGenres();
            var guideResponse = _GetGuideResponse();

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
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
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
                .Returns(mediaWeeks);

            var getGenresBySourceIdCalled = 0;
            _GenreRepo.Setup(r => r.GetGenresBySourceId(It.IsAny<int>()))
                .Callback(() => getGenresBySourceIdCalled++)
                .Returns(genres);

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteWarningCalled++);

            var guideRequests = new ConcurrentBag<Tuple<int, List<GuideRequestElementDto>>>();
            var guideRequestCallCount = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback<List<GuideRequestElementDto>>((r) =>
                {
                    guideRequests.Add(new Tuple<int, List<GuideRequestElementDto>>(Thread.CurrentThread.ManagedThreadId, r));
                    // throw on even, so we should get 2 back since 4 calls will be made.
                    if (Interlocked.Increment(ref guideRequestCallCount) % 2 == 0)
                    {
                        throw new Exception("Test Exception");
                    }
                })
                .Returns(guideResponse);

            var mappedStations = new List<StationMappingsDto>
            {
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Sigma, MapValue = "SigmaMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSI, MapValue = "NSIMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSILegacy, MapValue = "NSILegacyMappedValue"}
            };
            _StationMappingService.Setup(s => s.GetStationMappingsByCadentCallLetter(It.IsAny<string>()))
                .Returns(mappedStations);

            var engine = _GetInventoryProgramsProcessingEngine();
            engine.UT_ParallelApiCallsEnabled = true; // enable parallel for this test
            engine.UT_ParallelApiCallsBatchSize = 1;

            /*** Act ***/
            var caught = Assert.Throws<AggregateException>(() => engine.ProcessInventoryJob(jobId));

            /*** Assert ***/
            Assert.NotNull(caught);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(1, setJobCompleteErrorCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(4, guideRequestCallCount); // 1 per manifest.daypart = 4 expected calls.
            Assert.AreEqual(0, deleteInventoryProgramsFromManifestDaypartsCalled);
            Assert.AreEqual(0, updateInventoryProgramsCalled);
            // verify was multi=threaded
            var distinctThreadIdsCount = guideRequests.Select(s => s.Item1).Distinct().Count();
            Assert.IsTrue(distinctThreadIdsCount > 1);
            Assert.IsTrue(caught.Message.Contains("errors caught while calling Program Guide in parallel"));
            Assert.AreEqual(2, caught.InnerExceptions.Count); 
        }

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
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
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

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(new List<GuideResponseElementDto>());

            var engine = _GetInventoryProgramsProcessingEngine();

            var results = engine.ProcessInventoryJob(jobId);

            Assert.NotNull(results);
            Assert.AreEqual(1, GetInventoryBySourceForProgramsProcessingCalled);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(1, setJobCompleteWarningCalled);
            Assert.AreEqual(0, getProgramsForGuideCalled);
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
            var updateInventoryProgramsCalled = 0;
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
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

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(new List<GuideResponseElementDto>());

            var engine = _GetInventoryProgramsProcessingEngine();

            Assert.Throws<Exception>(() => engine.ProcessInventoryJob(jobId));

            Assert.AreEqual(1, GetInventoryBySourceForProgramsProcessingCalled);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(1, setJobCompleteErrorCalled);
            Assert.AreEqual(0, getProgramsForGuideCalled);
            Assert.AreEqual(0, deleteInventoryProgramsFromManifestDaypartsCalled);
            Assert.AreEqual(0, updateInventoryProgramsCalled);
        }

        [Test]
        public void BySourceJob()
        {
            /*** Arrange ***/
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
            var mediaWeeks = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek {Id = 1},
                new DisplayMediaWeek {Id = 2},
                new DisplayMediaWeek {Id = 3}
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            var genres = InventoryProgramsProcessingTestHelper.GetGenres();
            var guideResponse = _GetGuideResponse();

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

            var updateProgramsCalls = new List<Tuple<List<StationInventoryManifestDaypartProgram>, DateTime>>();
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((a, b) =>
                    updateProgramsCalls.Add(new Tuple<List<StationInventoryManifestDaypartProgram>, DateTime>(a, b)));

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
                .Returns(mediaWeeks);

            var getGenresBySourceIdCalled = 0;
            _GenreRepo.Setup(r => r.GetGenresBySourceId(It.IsAny<int>()))
                .Callback(() => getGenresBySourceIdCalled++)
                .Returns(genres);

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => setJobCompleteWarningCalled++);

            var getProgramsForGuidCallCount = 0;
            var guideRequests = new List<GuideRequestElementDto>();
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback<List<GuideRequestElementDto>>((r) =>
                {
                    getProgramsForGuidCallCount++;
                    guideRequests.AddRange(r);
                })
                .Returns(guideResponse);

            var mappedStations = new List<StationMappingsDto>
            {
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Sigma, MapValue = "SigmaMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSI, MapValue = "NSIMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSILegacy, MapValue = "NSILegacyMappedValue"}
            };
            _StationMappingService.Setup(s => s.GetStationMappingsByCadentCallLetter(It.IsAny<string>()))
                .Returns(mappedStations);

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var results = engine.ProcessInventoryJob(jobId);

            /*** Assert ***/
            /*** Assert ***/
            Assert.NotNull(results);
            Assert.AreEqual(1, getInventorySourceCalled);
            Assert.AreEqual(1, getDisplayMediaWeekByFlightCalled);
            Assert.IsTrue(inventoryProgramsBySourceJobsRepoCalls > 0);
            Assert.AreEqual(1, GetInventoryBySourceForProgramsProcessingCalled);
            Assert.AreEqual(1, getGenresBySourceIdCalled);

            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);

            // Verify the requests are configured well
            Assert.AreEqual(1, getProgramsForGuidCallCount);
            Assert.AreEqual(4, guideRequests.Count);

            var firstEntry = guideRequests[0];
            Assert.AreEqual("20200101", firstEntry.StartDate.ToString("yyyyMMdd"));
            Assert.AreEqual("20200121", firstEntry.EndDate.ToString("yyyyMMdd"));
            Assert.AreEqual(false, firstEntry.Daypart.Sunday);
            Assert.AreEqual(true, firstEntry.Daypart.Monday);
            Assert.AreEqual(true, firstEntry.Daypart.Tuesday);
            Assert.AreEqual(true, firstEntry.Daypart.Wednesday);
            Assert.AreEqual(true, firstEntry.Daypart.Thursday);
            Assert.AreEqual(true, firstEntry.Daypart.Friday);
            Assert.AreEqual(false, firstEntry.Daypart.Saturday);
            Assert.AreEqual(3600 * 2, firstEntry.Daypart.StartTime);
            Assert.AreEqual((3600 * 4) -1, firstEntry.Daypart.EndTime);
            Assert.AreEqual("ExtendedMappedValue", firstEntry.StationCallLetters);

            var secondEntry = guideRequests[1];
            Assert.AreEqual("20200101", secondEntry.StartDate.ToString("yyyyMMdd"));
            Assert.AreEqual("20200121", secondEntry.EndDate.ToString("yyyyMMdd"));
            Assert.AreEqual(true, secondEntry.Daypart.Sunday);
            Assert.AreEqual(false, secondEntry.Daypart.Monday);
            Assert.AreEqual(false, secondEntry.Daypart.Tuesday);
            Assert.AreEqual(false, secondEntry.Daypart.Wednesday);
            Assert.AreEqual(false, secondEntry.Daypart.Thursday);
            Assert.AreEqual(true, secondEntry.Daypart.Friday);
            Assert.AreEqual(true, secondEntry.Daypart.Saturday);
            Assert.AreEqual(3600 * 4, secondEntry.Daypart.StartTime);
            Assert.AreEqual((3600 * 6) - 1, secondEntry.Daypart.EndTime);
            Assert.AreEqual("ExtendedMappedValue", secondEntry.StationCallLetters);

            var thirdEntry = guideRequests[2];
            Assert.AreEqual("20200101", thirdEntry.StartDate.ToString("yyyyMMdd"));
            Assert.AreEqual("20200121", thirdEntry.EndDate.ToString("yyyyMMdd"));
            Assert.AreEqual(false, firstEntry.Daypart.Sunday);
            Assert.AreEqual(true, firstEntry.Daypart.Monday);
            Assert.AreEqual(true, firstEntry.Daypart.Tuesday);
            Assert.AreEqual(true, firstEntry.Daypart.Wednesday);
            Assert.AreEqual(true, firstEntry.Daypart.Thursday);
            Assert.AreEqual(true, firstEntry.Daypart.Friday);
            Assert.AreEqual(false, firstEntry.Daypart.Saturday);
            Assert.AreEqual(3600 * 2, firstEntry.Daypart.StartTime);
            Assert.AreEqual((3600 * 4) - 1, firstEntry.Daypart.EndTime);
            Assert.AreEqual("ExtendedMappedValue", thirdEntry.StationCallLetters);

            var fourthEntry = guideRequests[3];
            Assert.AreEqual("20200101", fourthEntry.StartDate.ToString("yyyyMMdd"));
            Assert.AreEqual("20200121", fourthEntry.EndDate.ToString("yyyyMMdd"));
            Assert.AreEqual(true, secondEntry.Daypart.Sunday);
            Assert.AreEqual(false, secondEntry.Daypart.Monday);
            Assert.AreEqual(false, secondEntry.Daypart.Tuesday);
            Assert.AreEqual(false, secondEntry.Daypart.Wednesday);
            Assert.AreEqual(false, secondEntry.Daypart.Thursday);
            Assert.AreEqual(true, secondEntry.Daypart.Friday);
            Assert.AreEqual(true, secondEntry.Daypart.Saturday);
            Assert.AreEqual(3600 * 4, fourthEntry.Daypart.StartTime);
            Assert.AreEqual((3600 * 6) - 1, fourthEntry.Daypart.EndTime);
            Assert.AreEqual("ExtendedMappedValue", fourthEntry.StationCallLetters);

            // Verify DELETE was called for the full date range and once for all inventory ids.
            Assert.AreEqual(1, deleteProgramsCalls.Count);
            var firstCall = deleteProgramsCalls[0];
            Assert.AreEqual(2, firstCall.Item1.Count); // only one entry per inventory 
            Assert.IsTrue(firstCall.Item1.Contains(1)); // first
            Assert.IsTrue(firstCall.Item1.Contains(2)); // second
            Assert.AreEqual("20200101", firstCall.Item2.ToString("yyyyMMdd"));
            Assert.AreEqual("20200121", firstCall.Item3.ToString("yyyyMMdd"));

            // Verify that we saved programs for ALL the dayparts
            Assert.AreEqual(1, updateProgramsCalls.Count);
            Assert.AreEqual(4, updateProgramsCalls[0].Item1.Count); // one per inventory daypart
            // Verify all four are present
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(1)); // first
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(2)); // second
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(3)); // third
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(4));
        }

        [Test]
        public void BySourceJob_EmptyResponse()
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
            var mediaWeeks = new List<DisplayMediaWeek>
            {
                new DisplayMediaWeek {Id = 1},
                new DisplayMediaWeek {Id = 2},
                new DisplayMediaWeek {Id = 3}
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            var genres = InventoryProgramsProcessingTestHelper.GetGenres();
            var guideResponse = new List<GuideResponseElementDto>();

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
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
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
                .Returns(mediaWeeks);

            var getGenresBySourceIdCalled = 0;
            _GenreRepo.Setup(r => r.GetGenresBySourceId(It.IsAny<int>()))
                .Callback(() => getGenresBySourceIdCalled++)
                .Returns(genres);

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteWarningCalled++);

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(guideResponse);

            var mappedStations = new List<StationMappingsDto>
            {
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Sigma, MapValue = "SigmaMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSI, MapValue = "NSIMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSILegacy, MapValue = "NSILegacyMappedValue"}
            };
            _StationMappingService.Setup(s => s.GetStationMappingsByCadentCallLetter(It.IsAny<string>()))
                .Returns(mappedStations);

            var engine = _GetInventoryProgramsProcessingEngine();

            var results = engine.ProcessInventoryJob(jobId);

            Assert.NotNull(results);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(1, setJobCompleteWarningCalled);
            Assert.AreEqual(1, getProgramsForGuideCalled);
            Assert.AreEqual(1, deleteInventoryProgramsFromManifestDaypartsCalled);
            Assert.AreEqual(0, updateInventoryProgramsCalled);
        }

        private List<GuideResponseElementDto> _GetGuideResponse()
        {
            return new List<GuideResponseElementDto>
            {
                new GuideResponseElementDto
                {
                    RequestDaypartId = "R000001.M001.D1",
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
                    RequestDaypartId = "R000002.M001.D2",
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
                    RequestDaypartId = "R000003.M002.D3",
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
                    RequestDaypartId = "R000004.M002.D4",
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
                }
            };
        }

        /// <remarks>Do this after you've setup all your data repository returns</remarks>
        private Mock<IDataRepositoryFactory> _GetDataRepositoryFactory()
        {
            _InventoryProgramsBySourceJobsRepo.Setup(r => r.UpdateJobStatus(It.IsAny<int>(), It.IsAny<InventoryProgramsJobStatus>(), It.IsAny<string>()));

            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IGenreRepository>()).Returns(_GenreRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryRepository>()).Returns(_InventoryRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryProgramsBySourceJobsRepository>()).Returns(_InventoryProgramsBySourceJobsRepo.Object);

            return dataRepoFactory;
        }

        private InventoryProgramsBySourceProcessorTestClass _GetInventoryProgramsProcessingEngine()
        {
            var dataRepoFactory = _GetDataRepositoryFactory();
            var engine = new InventoryProgramsBySourceProcessorTestClass(
                dataRepoFactory.Object, _ProgramGuidClient.Object,
                _StationMappingService.Object, _MediaWeekCache.Object);
            return engine;
        }
    }
}