using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    [TestFixture]
    public class InventoryProgramsByFileProcessorUnitTests
    {
        private Mock<IInventoryRepository> _InventoryRepo = new Mock<IInventoryRepository>();
        private Mock<IInventoryFileRepository> _InventoryFileRepo = new Mock<IInventoryFileRepository>();
        private Mock<IInventoryProgramsByFileJobsRepository> _InventoryProgramsByFileJobsRepo = new Mock<IInventoryProgramsByFileJobsRepository>();
        private Mock<IInventoryProgramsBySourceJobsRepository> _InventoryProgramsBySourceJobsRepo = new Mock<IInventoryProgramsBySourceJobsRepository>();
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaWeekCache = new Mock<IMediaMonthAndWeekAggregateCache>();
        private Mock<IProgramGuideApiClient> _ProgramGuidClient = new Mock<IProgramGuideApiClient>();
        private Mock<IStationMappingService> _StationMappingService = new Mock<IStationMappingService>();
        private Mock<IGenreCache> _GenreCacheMock = new Mock<IGenreCache>();

        [Test]
        public void ByFileJob()
        {
            /*** Arrange ***/
            const int jobId = 13;
            const int fileId = 12;
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            var guideResponse = _GetGuideResponse();

            var getStationInventoryByFileIdForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryByFileIdForProgramsProcessing(It.IsAny<int>()))
                .Callback(() => getStationInventoryByFileIdForProgramsProcessingCalled++)
                .Returns(manifests);

            var deleteProgramsCalls = new List<Tuple<List<int>, DateTime, DateTime>>();
            _InventoryRepo.Setup(r => r.DeleteInventoryPrograms(It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<List<int>, DateTime, DateTime>((a, b, c) => deleteProgramsCalls.Add(new Tuple<List<int>, DateTime, DateTime>(a, b, c)));

            var updateProgramsCalls = new List<Tuple<List<StationInventoryManifestDaypartProgram>, DateTime>>();
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((a, b) =>
                    updateProgramsCalls.Add(new Tuple<List<StationInventoryManifestDaypartProgram>, DateTime>(a, b)));

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
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteWarningCalled++);

            var getProgramsForGuideCallCount = 0;
            var guideRequests = new List<GuideRequestElementDto>();
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback<List<GuideRequestElementDto>>((r) =>
                {
                    getProgramsForGuideCallCount++;
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
            Assert.NotNull(results);
            Assert.IsTrue(inventoryProgramsByFileJobsRepoCalls > 1);
            Assert.AreEqual(1, getStationInventoryByFileIdForProgramsProcessingCalled);

            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);

            // Verify the requests are configured well
            Assert.AreEqual(1, getProgramsForGuideCallCount);
            Assert.AreEqual(2, guideRequests.Count);

            var firstEntry = guideRequests[0];
            Assert.AreEqual("20200101", firstEntry.StartDate.ToString("yyyyMMdd"));
            Assert.AreEqual("20200101", firstEntry.EndDate.ToString("yyyyMMdd"));
            Assert.AreEqual(false, firstEntry.Daypart.Sunday);
            Assert.AreEqual(false, firstEntry.Daypart.Monday);
            Assert.AreEqual(false, firstEntry.Daypart.Tuesday);
            Assert.AreEqual(true, firstEntry.Daypart.Wednesday);
            Assert.AreEqual(false, firstEntry.Daypart.Thursday);
            Assert.AreEqual(false, firstEntry.Daypart.Friday);
            Assert.AreEqual(false, firstEntry.Daypart.Saturday);
            Assert.AreEqual(3600 * 2, firstEntry.Daypart.StartTime);
            Assert.AreEqual((3600 * 2) + (60 * 5), firstEntry.Daypart.EndTime);
            Assert.AreEqual("ExtendedMappedValue", firstEntry.StationCallLetters);

            var secondEntry = guideRequests[1];
            Assert.AreEqual("20200104", secondEntry.StartDate.ToString("yyyyMMdd"));
            Assert.AreEqual("20200104", secondEntry.EndDate.ToString("yyyyMMdd"));
            Assert.AreEqual(false, secondEntry.Daypart.Sunday);
            Assert.AreEqual(false, secondEntry.Daypart.Monday);
            Assert.AreEqual(false, secondEntry.Daypart.Tuesday);
            Assert.AreEqual(false, secondEntry.Daypart.Wednesday);
            Assert.AreEqual(false, secondEntry.Daypart.Thursday);
            Assert.AreEqual(false, secondEntry.Daypart.Friday);
            Assert.AreEqual(true, secondEntry.Daypart.Saturday);
            Assert.AreEqual(3600 * 2, secondEntry.Daypart.StartTime);
            Assert.AreEqual((3600 * 2) + (60 * 5), secondEntry.Daypart.EndTime);
            Assert.AreEqual("ExtendedMappedValue", secondEntry.StationCallLetters);

            // Verify DELETE was called for the full date range and once for all inventory ids.
            Assert.AreEqual(1, deleteProgramsCalls.Count);
            var firstCall = deleteProgramsCalls[0];
            Assert.AreEqual(2, firstCall.Item1.Count); // only one entry per inventory 
            Assert.IsTrue(firstCall.Item1.Contains(1)); // contains the first inventory
            Assert.IsTrue(firstCall.Item1.Contains(2)); // contains the second inventory
            Assert.AreEqual("20200101", firstCall.Item2.ToString("yyyyMMdd"));
            Assert.AreEqual("20200121", firstCall.Item3.ToString("yyyyMMdd"));

            // Verify that we saved programs for ALL the dayparts, not just the first one.
            Assert.AreEqual(1, updateProgramsCalls.Count);
            Assert.AreEqual(4, updateProgramsCalls[0].Item1.Count); // one per inventory daypart
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(1)); // first
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(2)); // second
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(3)); // third
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(4)); // fourth
        }

        [Test]
        public void ByFileJob_DaypartStartsFiveMinutesToMidnight()
        {
            /*** Arrange ***/
            const int jobId = 13;
            const int fileId = 12;
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            manifests[0].ManifestDayparts[0].Daypart.StartTime = (24 * 3600) - (60 * 4);
            manifests[0].ManifestDayparts[0].Daypart.EndTime = (3600 * 3) - 1;
            manifests[0].ManifestDayparts[1].Daypart.StartTime = (24 * 3600) - (60 * 3);
            manifests[0].ManifestDayparts[1].Daypart.EndTime = (3600 * 5) - 1;
            
            var guideResponse = _GetGuideResponse();

            var getStationInventoryByFileIdForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryByFileIdForProgramsProcessing(It.IsAny<int>()))
                .Callback(() => getStationInventoryByFileIdForProgramsProcessingCalled++)
                .Returns(manifests);

            var deleteProgramsCalls = new List<Tuple<List<int>, DateTime, DateTime>>();
            _InventoryRepo.Setup(r => r.DeleteInventoryPrograms(It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<List<int>, DateTime, DateTime>((a, b, c) => deleteProgramsCalls.Add(new Tuple<List<int>, DateTime, DateTime>(a, b, c)));

            var updateProgramsCalls = new List<Tuple<List<StationInventoryManifestDaypartProgram>, DateTime>>();
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((a, b) =>
                    updateProgramsCalls.Add(new Tuple<List<StationInventoryManifestDaypartProgram>, DateTime>(a, b)));

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
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteWarningCalled++);

            var getProgramsForGuideCallCount = 0;
            var guideRequests = new List<GuideRequestElementDto>();
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback<List<GuideRequestElementDto>>((r) =>
                {
                    getProgramsForGuideCallCount++;
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
            Assert.NotNull(results);
            Assert.IsTrue(inventoryProgramsByFileJobsRepoCalls > 1);
            Assert.AreEqual(1, getStationInventoryByFileIdForProgramsProcessingCalled);

            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);

            // Verify the requests are configured well
            Assert.AreEqual(1, getProgramsForGuideCallCount);
            Assert.AreEqual(2, guideRequests.Count);

            var firstEntry = guideRequests[0];
            Assert.AreEqual("20200101", firstEntry.StartDate.ToString("yyyyMMdd"));
            Assert.AreEqual("20200102", firstEntry.EndDate.ToString("yyyyMMdd"));
            Assert.AreEqual(false, firstEntry.Daypart.Sunday);
            Assert.AreEqual(false, firstEntry.Daypart.Monday);
            Assert.AreEqual(false, firstEntry.Daypart.Tuesday);
            Assert.AreEqual(true, firstEntry.Daypart.Wednesday);
            Assert.AreEqual(true, firstEntry.Daypart.Thursday);
            Assert.AreEqual(false, firstEntry.Daypart.Friday);
            Assert.AreEqual(false, firstEntry.Daypart.Saturday);
            Assert.AreEqual(86160, firstEntry.Daypart.StartTime);
            Assert.AreEqual(60, firstEntry.Daypart.EndTime);
            Assert.AreEqual("ExtendedMappedValue", firstEntry.StationCallLetters);

            var secondEntry = guideRequests[1];
            Assert.AreEqual("20200104", secondEntry.StartDate.ToString("yyyyMMdd"));
            Assert.AreEqual("20200104", secondEntry.EndDate.ToString("yyyyMMdd"));
            Assert.AreEqual(false, secondEntry.Daypart.Sunday);
            Assert.AreEqual(false, secondEntry.Daypart.Monday);
            Assert.AreEqual(false, secondEntry.Daypart.Tuesday);
            Assert.AreEqual(false, secondEntry.Daypart.Wednesday);
            Assert.AreEqual(false, secondEntry.Daypart.Thursday);
            Assert.AreEqual(false, secondEntry.Daypart.Friday);
            Assert.AreEqual(true, secondEntry.Daypart.Saturday);
            Assert.AreEqual(3600 * 2, secondEntry.Daypart.StartTime);
            Assert.AreEqual((3600 * 2) + (60 * 5), secondEntry.Daypart.EndTime);
            Assert.AreEqual("ExtendedMappedValue", secondEntry.StationCallLetters);

            // Verify DELETE was called for the full date range and once for all inventory ids.
            Assert.AreEqual(1, deleteProgramsCalls.Count);
            var firstCall = deleteProgramsCalls[0];
            Assert.AreEqual(2, firstCall.Item1.Count); // only one entry per inventory 
            Assert.IsTrue(firstCall.Item1.Contains(1)); // contains the first inventory
            Assert.IsTrue(firstCall.Item1.Contains(2)); // contains the second inventory
            Assert.AreEqual("20200101", firstCall.Item2.ToString("yyyyMMdd"));
            Assert.AreEqual("20200121", firstCall.Item3.ToString("yyyyMMdd"));

            // Verify that we saved programs for ALL the dayparts, not just the first one.
            Assert.AreEqual(1, updateProgramsCalls.Count);
            Assert.AreEqual(4, updateProgramsCalls[0].Item1.Count); // one per inventory daypart
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(1)); // first
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(2)); // second
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(3)); // third
            Assert.IsTrue(updateProgramsCalls[0].Item1.Select(s => s.StationInventoryManifestDaypartId).Contains(4)); // fourth
        }


        [Test]
        public void ByFileJob_NoManifests()
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
            
            var getStationInventoryByFileIdForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryByFileIdForProgramsProcessing(It.IsAny<int>()))
                .Callback(() => getStationInventoryByFileIdForProgramsProcessingCalled++)
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
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteWarningCalled++);

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(new List<GuideResponseElementDto>());

            var engine = _GetInventoryProgramsProcessingEngine();

            var results = engine.ProcessInventoryJob(jobId);

            Assert.NotNull(results);
            Assert.AreEqual(1, getStationInventoryByFileIdForProgramsProcessingCalled);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(1, setJobCompleteWarningCalled);
            Assert.AreEqual(0, getProgramsForGuideCalled);
        }

        [Test]
        public void ByFileJob_WithException()
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

            var getStationInventoryByFileIdForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryByFileIdForProgramsProcessing(It.IsAny<int>()))
                .Callback(() => getStationInventoryByFileIdForProgramsProcessingCalled++)
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
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);

            var getProgramsForGuideCalled = 0;
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback(() => getProgramsForGuideCalled++)
                .Returns(new List<GuideResponseElementDto>());

            var engine = _GetInventoryProgramsProcessingEngine();

            Assert.Throws<Exception>(() => engine.ProcessInventoryJob(jobId));

            Assert.AreEqual(1, getStationInventoryByFileIdForProgramsProcessingCalled);
            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(1, setJobCompleteErrorCalled);
            Assert.AreEqual(0, getProgramsForGuideCalled);
        }

        [Test]
        public void ByFileJob_WithoutStationMapping()
        {
            /*** Arrange ***/
            const int jobId = 13;
            const int fileId = 12;
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            var guideResponse = _GetGuideResponse();

            var getStationInventoryByFileIdForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryByFileIdForProgramsProcessing(It.IsAny<int>()))
                .Callback(() => getStationInventoryByFileIdForProgramsProcessingCalled++)
                .Returns(manifests);

            var deleteInventoryProgramsFromManifestDaypartsCalled = 0;
            _InventoryRepo.Setup(r => r.DeleteInventoryPrograms(It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() => deleteInventoryProgramsFromManifestDaypartsCalled++);
            var updateInventoryProgramsCalled = 0;
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
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

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);

            var getProgramsForGuidCallCount = 0;
            var guideRequests = new List<GuideRequestElementDto>();
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback<List<GuideRequestElementDto>>((r) =>
                {
                    getProgramsForGuidCallCount++;
                    guideRequests.AddRange(r);
                })
                .Returns(guideResponse);

            var mappedStations = new List<StationMappingsDto>();
            _StationMappingService.Setup(s => s.GetStationMappingsByCadentCallLetter(It.IsAny<string>()))
                .Returns(mappedStations);

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var caught = Assert.Throws<Exception>(() => engine.ProcessInventoryJob(jobId));

            /*** Assert ***/
            Assert.IsTrue(caught.Message.Contains($"Mapping for CadentCallsign 'CadentStationName1' and Map Set 'Extended' not found."));
        }

        [Test]
        public void ByFileJob_WithTooManyStationMappings()
        {
            /*** Arrange ***/
            const int jobId = 13;
            const int fileId = 12;
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            var guideResponse = _GetGuideResponse();

            var getStationInventoryByFileIdForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryByFileIdForProgramsProcessing(It.IsAny<int>()))
                .Callback(() => getStationInventoryByFileIdForProgramsProcessingCalled++)
                .Returns(manifests);

            var deleteInventoryProgramsFromManifestDaypartsCalled = 0;
            _InventoryRepo.Setup(r => r.DeleteInventoryPrograms(It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() => deleteInventoryProgramsFromManifestDaypartsCalled++);
            var updateInventoryProgramsCalled = 0;
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
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

            var setJobCompleteSuccessCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);

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
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValueTwo"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSILegacy, MapValue = "NSILegacyMappedValue"}
            };
            _StationMappingService.Setup(s => s.GetStationMappingsByCadentCallLetter(It.IsAny<string>()))
                .Returns(mappedStations);

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var caught = Assert.Throws<Exception>(() => engine.ProcessInventoryJob(jobId));

            /*** Assert ***/
            Assert.IsTrue(caught.Message.Contains($"Mapping for CadentCallsign 'CadentStationName1' and Map Set 'Extended' has 2 mappings when only one expected."));
        }

        [Test]
        public void ByFileJob_AStationWithoutAnAffiliation()
        {
            /*** Arrange ***/
            const int jobId = 13;
            const int fileId = 12;
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "NumberOneSource",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.OpenMarket
            };
            var manifests = InventoryProgramsProcessingTestHelper.GetManifests(2);
            var guideResponse = new List<GuideResponseElementDto>
            {
                new GuideResponseElementDto
                {
                    RequestDaypartId = "R000001.M002.D3",
                    Programs = new List<GuideResponseProgramDto>
                    {
                        new GuideResponseProgramDto
                        {
                            ProgramName = "ProgramTwo",
                            SourceGenre = "SourceGenreTwo",
                            ShowType = "ShowTypeTwo",
                            SyndicationType = "SyndicationTypeTwo",
                            Occurrences = 1,
                            StartDate = new DateTime(2020, 01, 01),
                            EndDate = new DateTime(2020, 01, 01),
                            StartTime = 3600 * 3,
                            EndTime = (3600 * 3) + (60 * 30) - 1,
                            Wednesday = true
                        }
                    }
                }
            };

            manifests.First().Station.Affiliation = null;

            var getStationInventoryByFileIdForProgramsProcessingCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryByFileIdForProgramsProcessing(It.IsAny<int>()))
                .Callback(() => getStationInventoryByFileIdForProgramsProcessingCalled++)
                .Returns(manifests);

            var deleteInventoryProgramsFromManifestDaypartsCalled = 0;
            _InventoryRepo.Setup(r => r.DeleteInventoryPrograms(It.IsAny<List<int>>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() => deleteInventoryProgramsFromManifestDaypartsCalled++);

            var updateInventoryProgramsCalls = new List<List<StationInventoryManifestDaypartProgram>>();
            _InventoryRepo.Setup(r => r.UpdateInventoryPrograms(
                    It.IsAny<List<StationInventoryManifestDaypartProgram>>(), It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((programs, d) => updateInventoryProgramsCalls.Add(programs));

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
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteSuccess(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteSuccessCalled++);
            var setJobCompleteErrorCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteError(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteErrorCalled++);
            var setJobCompleteWarningCalled = 0;
            _InventoryProgramsByFileJobsRepo.Setup(r => r.SetJobCompleteWarning(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Callback(() => setJobCompleteWarningCalled++);

            var getProgramsForGuideCallCount = 0;
            var guideRequests = new List<GuideRequestElementDto>();
            _ProgramGuidClient.Setup(s => s.GetProgramsForGuide(It.IsAny<List<GuideRequestElementDto>>()))
                .Callback<List<GuideRequestElementDto>>((r) =>
                {
                    getProgramsForGuideCallCount++;
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
            Assert.NotNull(results);
            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);
            Assert.AreEqual(1, getProgramsForGuideCallCount);
            Assert.AreEqual(1, deleteInventoryProgramsFromManifestDaypartsCalled);
            Assert.AreEqual(1, updateInventoryProgramsCalls.Count);
            Assert.AreEqual(2, updateInventoryProgramsCalls[0].Count);
            Assert.AreEqual(3, updateInventoryProgramsCalls[0][0].StationInventoryManifestDaypartId);
            Assert.AreEqual(4, updateInventoryProgramsCalls[0][1].StationInventoryManifestDaypartId);
            Assert.AreEqual(0, guideRequests.Count(s => s.StationCallLetters.Equals("ExtendedMappedValue") == false));
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
                            EndDate = new DateTime(2020, 02,12),
                            StartTime = 3600 * 3,
                            EndTime = (3600 * 3) + (60 * 30) - 1,
                            Wednesday = true
                        }
                    }
                },
                new GuideResponseElementDto
                {
                    RequestDaypartId = "R000002.M002.D3",
                    Programs = new List<GuideResponseProgramDto>
                    {
                        new GuideResponseProgramDto
                        {
                            ProgramName = "ProgramTwo",
                            SourceGenre = "SourceGenreTwo",
                            ShowType = "ShowTypeTwo",
                            SyndicationType = "SyndicationTypeTwo",
                            Occurrences = 1,
                            StartDate = new DateTime(2020, 01, 01),
                            EndDate = new DateTime(2020, 01, 01),
                            StartTime = 3600 * 3,
                            EndTime = (3600 * 3) + (60 * 30) - 1,
                            Wednesday = true
                        }
                    }
                }
            };
        }

        /// <remarks>Do this after you've setup all your data repository returns</remarks>
        private Mock<IDataRepositoryFactory> _GetDataRepositoryFactory()
        {
            _InventoryProgramsByFileJobsRepo.Setup(r => r.UpdateJobStatus(It.IsAny<int>(), It.IsAny<InventoryProgramsJobStatus>(), It.IsAny<string>()));

            _InventoryRepo
                .Setup(x => x.GetDaypartProgramsForInventoryDayparts(It.IsAny<List<int>>()))
                .Returns<List<int>>(x => x.Select(mdId => new StationInventoryManifestDaypartProgram
                {
                    Id = 1,
                    StationInventoryManifestDaypartId = mdId,
                    ProgramName = "ProgramName",
                    ShowType = "ShowType",
                    SourceGenreId = 1,
                    GenreSourceId = 2,
                    MaestroGenreId = 2,
                    StartTime = 100,
                    EndTime = 200
                }).ToList());
                
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryRepository>()).Returns(_InventoryRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryFileRepository>()).Returns(_InventoryFileRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryProgramsByFileJobsRepository>()).Returns(_InventoryProgramsByFileJobsRepo.Object);

            return dataRepoFactory;
        }

        private InventoryProgramsByFileProcessorTestClass _GetInventoryProgramsProcessingEngine()
        {
            var dataRepoFactory = _GetDataRepositoryFactory();

            _GenreCacheMock
                .Setup(x => x.GetSourceGenreByName(It.IsAny<string>(), It.IsAny<GenreSourceEnum>()))
                .Returns<string, GenreSourceEnum>((p1, p2) => new LookupDto
                {
                    Id = 1,
                    Display = $"{p2.ToString()} Genre"
                });

            _GenreCacheMock
                .Setup(x => x.GetMaestroGenreBySourceGenre(It.IsAny<LookupDto>(), It.IsAny<GenreSourceEnum>()))
                .Returns(new LookupDto
                {
                    Id = 2,
                    Display = "Maestro Genre"
                });

            var engine = new InventoryProgramsByFileProcessorTestClass(
                dataRepoFactory.Object, 
                _ProgramGuidClient.Object,
                _StationMappingService.Object,
                _GenreCacheMock.Object);
            return engine;
        }
    }
}