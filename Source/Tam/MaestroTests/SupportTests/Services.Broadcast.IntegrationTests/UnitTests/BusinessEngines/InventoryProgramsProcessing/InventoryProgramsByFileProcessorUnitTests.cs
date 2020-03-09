﻿using Common.Services;
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
using System.Net.Mail;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    [TestFixture]
    public class InventoryProgramsByFileProcessorUnitTests
    {
        private Mock<IInventoryRepository> _InventoryRepo = new Mock<IInventoryRepository>();
        private Mock<IInventoryFileRepository> _InventoryFileRepo = new Mock<IInventoryFileRepository>();
        private Mock<IInventoryProgramsByFileJobsRepository> _InventoryProgramsByFileJobsRepo = new Mock<IInventoryProgramsByFileJobsRepository>();
        private Mock<IProgramGuideApiClient> _ProgramGuidClient = new Mock<IProgramGuideApiClient>();
        private Mock<IStationMappingService> _StationMappingService = new Mock<IStationMappingService>();
        private Mock<IGenreCache> _GenreCacheMock = new Mock<IGenreCache>();

        private Mock<IFileService> _FileService = new Mock<IFileService>();
        private Mock<IEmailerService> _EmailerService = new Mock<IEmailerService>();

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
                .Returns(new InventoryFile
                {
                    Id = 21,
                    FileName = "TestInventoryFileName",
                    InventorySource = inventorySource
                });

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

            var createdFiles = new List<Tuple<string, List<string>>>();
            _FileService.Setup(s => s.CreateTextFile(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((name, lines) => createdFiles.Add(new Tuple<string, List<string>>(name, lines)));
            var expectedResultFileLines = new[]
            {
                "inventory_id,inventory_week_id,inventory_daypart_id,station_call_letters,affiliation,start_date,end_date,daypart_text,mon,tue,wed,thu,fri,sat,sun,daypart_start_time,daypart_end_time,program_name,show_type,genre,program_start_time,program_end_time,program_start_date,program_end_date",
                "1,1,1,ExtendedMappedValue,ABC,2020-01-01,2020-01-07,M-F 2AM-4AM,1,1,1,1,1,0,0,7200,14399,,,,,,,",
                "1,1,2,ExtendedMappedValue,ABC,2020-01-01,2020-01-07,F-SU 4AM-6AM,0,0,0,0,1,1,1,14400,21599,,,,,,,",
                "1,2,1,ExtendedMappedValue,ABC,2020-01-08,2020-01-14,M-F 2AM-4AM,1,1,1,1,1,0,0,7200,14399,,,,,,,",
                "1,2,2,ExtendedMappedValue,ABC,2020-01-08,2020-01-14,F-SU 4AM-6AM,0,0,0,0,1,1,1,14400,21599,,,,,,,",
                "1,3,1,ExtendedMappedValue,ABC,2020-01-15,2020-01-21,M-F 2AM-4AM,1,1,1,1,1,0,0,7200,14399,,,,,,,",
                "1,3,2,ExtendedMappedValue,ABC,2020-01-15,2020-01-21,F-SU 4AM-6AM,0,0,0,0,1,1,1,14400,21599,,,,,,,",
                "2,4,3,ExtendedMappedValue,ABC,2020-01-01,2020-01-07,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,4,4,ExtendedMappedValue,ABC,2020-01-01,2020-01-07,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,",
                "2,5,3,ExtendedMappedValue,ABC,2020-01-08,2020-01-14,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,5,4,ExtendedMappedValue,ABC,2020-01-08,2020-01-14,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,",
                "2,6,3,ExtendedMappedValue,ABC,2020-01-15,2020-01-21,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,6,4,ExtendedMappedValue,ABC,2020-01-15,2020-01-21,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,"
            };

            var createdDirectories = new List<string>();
            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()))
                .Callback<string>((s) => createdDirectories.Add(s));

            // body, subject, priority, to_emails
            var emailsSent = new List<Tuple<string, string, MailPriority, string[]>>();
            _EmailerService.Setup(s => s.QuickSend(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MailPriority>(), It.IsAny<string[]>(),
                    It.IsAny<List<string>>()))
                .Callback<bool, string, string, MailPriority, string[], List<string>>((h,b,s,p,t,a)=>
                    emailsSent.Add(new Tuple<string, string, MailPriority, string[]>(b,s,p,t)))
                .Returns(true);

            var engine = _GetInventoryProgramsProcessingEngine();
            engine.UT_CurrentDateTime = new DateTime(2020, 03, 06, 14,22, 35);

            /*** Act ***/
            var results = engine.ProcessInventoryJob(jobId);

            /*** Assert ***/
            Assert.NotNull(results);
            Assert.IsTrue(inventoryProgramsByFileJobsRepoCalls > 1);
            Assert.AreEqual(1, getStationInventoryByFileIdForProgramsProcessingCalled);

            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);

            // verify directories created
            Assert.AreEqual(3, createdDirectories.Count);

            // verify the file was exported well
            Assert.AreEqual(1, createdFiles.Count);
            Assert.AreEqual(@"testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Export\ProgramGuideInventoryExportFile_20200306_142235.csv", createdFiles[0].Item1);
            Assert.AreEqual(13, createdFiles[0].Item2.Count);
            for (var i = 0; i < 13; i++)
            {
                Assert.AreEqual(expectedResultFileLines[i], createdFiles[0].Item2[i]);
            }
            
            // verify that the email was sent
            Assert.AreEqual(1, emailsSent.Count);
            var body = emailsSent[0].Item1;
            Assert.IsTrue(body.Contains("A ProgramGuide Interface file has been exported."));
            Assert.IsTrue(body.Contains("Inventory File Id : 21"));
            Assert.IsTrue(body.Contains("Inventory File Name : TestInventoryFileName"));
            Assert.IsTrue(body.Contains("Inventory Source : NumberOneSource"));
            Assert.IsTrue(body.Contains(@"testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Export\ProgramGuideInventoryExportFile_20200306_142235.csv"));

            Assert.AreEqual("Broadcast Inventory Programs - ProgramGuide Interface Export file available", emailsSent[0].Item2);
            Assert.AreEqual(MailPriority.Normal, emailsSent[0].Item3);
            Assert.IsTrue(emailsSent[0].Item4.Any());
        }

        #region PRI-23390 : Commenting out.  We may want to bring this back later.

        /*
            PRI-23390 : Commenting out.  We may want to bring this back later.
         
        [Test]
        public void ByFileJob_DaypartStartsFiveMinutesToMidnight()
        {
            /*** Arrange ***
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

            /*** Act ***
            var results = engine.ProcessInventoryJob(jobId);

            /*** Assert ***
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

    */

        #endregion // #region PRI-23390 : Commenting out.  We may want to bring this back later.

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
                .Returns(new InventoryFile
                {
                    Id = 21,
                    FileName = "TestInventoryFileName",
                    InventorySource = inventorySource
                });

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

            var emptyMappedStations = new List<StationMappingsDto>();
            var mappedStations = new List<StationMappingsDto>
            {
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Sigma, MapValue = "SigmaMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValueThree"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSI, MapValue = "NsiMappedValueTwo"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSILegacy, MapValue = "NSILegacyMappedValue"}
            };
            _StationMappingService.SetupSequence(s => s.GetStationMappingsByCadentCallLetter(It.IsAny<string>()))
                .Returns(emptyMappedStations)
                .Returns(mappedStations);

            var createdFiles = new List<Tuple<string, List<string>>>();
            _FileService.Setup(s => s.CreateTextFile(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((name, lines) => createdFiles.Add(new Tuple<string, List<string>>(name, lines)));
            var expectedResultFileLines = new[]
            {
                "inventory_id,inventory_week_id,inventory_daypart_id,station_call_letters,affiliation,start_date,end_date,daypart_text,mon,tue,wed,thu,fri,sat,sun,daypart_start_time,daypart_end_time,program_name,show_type,genre,program_start_time,program_end_time,program_start_date,program_end_date",
                "2,4,3,ExtendedMappedValueThree,ABC,2020-01-01,2020-01-07,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,4,4,ExtendedMappedValueThree,ABC,2020-01-01,2020-01-07,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,",
                "2,5,3,ExtendedMappedValueThree,ABC,2020-01-08,2020-01-14,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,5,4,ExtendedMappedValueThree,ABC,2020-01-08,2020-01-14,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,",
                "2,6,3,ExtendedMappedValueThree,ABC,2020-01-15,2020-01-21,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,6,4,ExtendedMappedValueThree,ABC,2020-01-15,2020-01-21,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,"
            };

            var createdDirectories = new List<string>();
            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()))
                .Callback<string>((s) => createdDirectories.Add(s));

            // body, subject, priority, to_emails
            var emailsSent = new List<Tuple<string, string, MailPriority, string[]>>();
            _EmailerService.Setup(s => s.QuickSend(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MailPriority>(), It.IsAny<string[]>(),
                    It.IsAny<List<string>>()))
                .Callback<bool, string, string, MailPriority, string[], List<string>>((h, b, s, p, t, a) =>
                    emailsSent.Add(new Tuple<string, string, MailPriority, string[]>(b, s, p, t)))
                .Returns(true);

            var engine = _GetInventoryProgramsProcessingEngine();
            engine.UT_CurrentDateTime = new DateTime(2020, 03, 06, 14, 22, 35);

            /*** Act ***/
            var results = engine.ProcessInventoryJob(jobId);

            /*** Assert ***/
            Assert.NotNull(results);
            Assert.IsTrue(inventoryProgramsByFileJobsRepoCalls > 1);
            Assert.AreEqual(1, getStationInventoryByFileIdForProgramsProcessingCalled);

            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(1, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);

            // verify directories created
            Assert.AreEqual(3, createdDirectories.Count);

            // verify the file was exported well
            Assert.AreEqual(1, createdFiles.Count);
            Assert.AreEqual(@"testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Export\ProgramGuideInventoryExportFile_20200306_142235.csv", createdFiles[0].Item1);
            Assert.AreEqual(7, createdFiles[0].Item2.Count);
            for (var i = 0; i < 7; i++)
            {
                Assert.AreEqual(expectedResultFileLines[i], createdFiles[0].Item2[i]);
            }

            // verify that the email was sent
            Assert.AreEqual(1, emailsSent.Count);
            var body = emailsSent[0].Item1;
            Assert.IsTrue(body.Contains("A ProgramGuide Interface file has been exported."));
            Assert.IsTrue(body.Contains("Inventory File Id : 21"));
            Assert.IsTrue(body.Contains("Inventory File Name : TestInventoryFileName"));
            Assert.IsTrue(body.Contains("Inventory Source : NumberOneSource"));
            Assert.IsTrue(body.Contains(@"testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Export\ProgramGuideInventoryExportFile_20200306_142235.csv"));

            Assert.AreEqual("Broadcast Inventory Programs - ProgramGuide Interface Export file available", emailsSent[0].Item2);
            Assert.AreEqual(MailPriority.Normal, emailsSent[0].Item3);
            Assert.IsTrue(emailsSent[0].Item4.Any());
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
                .Returns(new InventoryFile
                {
                    Id = 21,
                    FileName = "TestInventoryFileName",
                    InventorySource = inventorySource
                });

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

            var mappedStationsWithDouble = new List<StationMappingsDto>
            {
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Sigma, MapValue = "SigmaMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValueOne"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValueTwo"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSILegacy, MapValue = "NSILegacyMappedValue"}
            };
            var mappedStations = new List<StationMappingsDto>
            {
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Sigma, MapValue = "SigmaMappedValue"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.Extended, MapValue = "ExtendedMappedValueThree"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSI, MapValue = "NsiMappedValueTwo"},
                new StationMappingsDto {StationId =  1, MapSet = StationMapSetNamesEnum.NSILegacy, MapValue = "NSILegacyMappedValue"}
            };
            _StationMappingService.SetupSequence(s => s.GetStationMappingsByCadentCallLetter(It.IsAny<string>()))
                .Returns(mappedStationsWithDouble)
                .Returns(mappedStations);

            var createdFiles = new List<Tuple<string, List<string>>>();
            _FileService.Setup(s => s.CreateTextFile(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((name, lines) => createdFiles.Add(new Tuple<string, List<string>>(name, lines)));
            var expectedResultFileLines = new[]
            {
                "inventory_id,inventory_week_id,inventory_daypart_id,station_call_letters,affiliation,start_date,end_date,daypart_text,mon,tue,wed,thu,fri,sat,sun,daypart_start_time,daypart_end_time,program_name,show_type,genre,program_start_time,program_end_time,program_start_date,program_end_date",
                "2,4,3,ExtendedMappedValueThree,ABC,2020-01-01,2020-01-07,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,4,4,ExtendedMappedValueThree,ABC,2020-01-01,2020-01-07,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,",
                "2,5,3,ExtendedMappedValueThree,ABC,2020-01-08,2020-01-14,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,5,4,ExtendedMappedValueThree,ABC,2020-01-08,2020-01-14,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,",
                "2,6,3,ExtendedMappedValueThree,ABC,2020-01-15,2020-01-21,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,6,4,ExtendedMappedValueThree,ABC,2020-01-15,2020-01-21,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,"
            };

            var createdDirectories = new List<string>();
            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()))
                .Callback<string>((s) => createdDirectories.Add(s));

            // body, subject, priority, to_emails
            var emailsSent = new List<Tuple<string, string, MailPriority, string[]>>();
            _EmailerService.Setup(s => s.QuickSend(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MailPriority>(), It.IsAny<string[]>(),
                    It.IsAny<List<string>>()))
                .Callback<bool, string, string, MailPriority, string[], List<string>>((h, b, s, p, t, a) =>
                    emailsSent.Add(new Tuple<string, string, MailPriority, string[]>(b, s, p, t)))
                .Returns(true);

            var engine = _GetInventoryProgramsProcessingEngine();
            engine.UT_CurrentDateTime = new DateTime(2020, 03, 06, 14, 22, 35);

            /*** Act ***/
            var results = engine.ProcessInventoryJob(jobId);

            /*** Assert ***/
            Assert.NotNull(results);
            Assert.IsTrue(inventoryProgramsByFileJobsRepoCalls > 1);
            Assert.AreEqual(1, getStationInventoryByFileIdForProgramsProcessingCalled);

            Assert.AreEqual(0, setJobCompleteSuccessCalled);
            Assert.AreEqual(1, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);

            // verify directories created
            Assert.AreEqual(3, createdDirectories.Count);

            // verify the file was exported well
            Assert.AreEqual(1, createdFiles.Count);
            Assert.AreEqual(@"testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Export\ProgramGuideInventoryExportFile_20200306_142235.csv", createdFiles[0].Item1);
            Assert.AreEqual(7, createdFiles[0].Item2.Count);
            for (var i = 0; i < 7; i++)
            {
                Assert.AreEqual(expectedResultFileLines[i], createdFiles[0].Item2[i]);
            }

            // verify that the email was sent
            Assert.AreEqual(1, emailsSent.Count);
            var body = emailsSent[0].Item1;
            Assert.IsTrue(body.Contains("A ProgramGuide Interface file has been exported."));
            Assert.IsTrue(body.Contains("Inventory File Id : 21"));
            Assert.IsTrue(body.Contains("Inventory File Name : TestInventoryFileName"));
            Assert.IsTrue(body.Contains("Inventory Source : NumberOneSource"));
            Assert.IsTrue(body.Contains(@"testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Export\ProgramGuideInventoryExportFile_20200306_142235.csv"));

            Assert.AreEqual("Broadcast Inventory Programs - ProgramGuide Interface Export file available", emailsSent[0].Item2);
            Assert.AreEqual(MailPriority.Normal, emailsSent[0].Item3);
            Assert.IsTrue(emailsSent[0].Item4.Any());
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
                .Returns(new InventoryFile
                {
                    Id = 21,
                    FileName = "TestInventoryFileName",
                    InventorySource = inventorySource
                });

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

            var createdFiles = new List<Tuple<string, List<string>>>();
            _FileService.Setup(s => s.CreateTextFile(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Callback<string, List<string>>((name, lines) => createdFiles.Add(new Tuple<string, List<string>>(name, lines)));
            var expectedResultFileLines = new[]
            {
                "inventory_id,inventory_week_id,inventory_daypart_id,station_call_letters,affiliation,start_date,end_date,daypart_text,mon,tue,wed,thu,fri,sat,sun,daypart_start_time,daypart_end_time,program_name,show_type,genre,program_start_time,program_end_time,program_start_date,program_end_date",
                "2,4,3,ExtendedMappedValue,ABC,2020-01-01,2020-01-07,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,4,4,ExtendedMappedValue,ABC,2020-01-01,2020-01-07,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,",
                "2,5,3,ExtendedMappedValue,ABC,2020-01-08,2020-01-14,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,5,4,ExtendedMappedValue,ABC,2020-01-08,2020-01-14,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,",
                "2,6,3,ExtendedMappedValue,ABC,2020-01-15,2020-01-21,SA-SU 2AM-4AM,0,0,0,0,0,1,1,7200,14399,,,,,,,",
                "2,6,4,ExtendedMappedValue,ABC,2020-01-15,2020-01-21,M-TH 4AM-6AM,1,1,1,1,0,0,0,14400,21599,,,,,,,"
            };
            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()));

            // body, subject, priority, to_emails
            var emailsSent = new List<Tuple<string, string, MailPriority, string[]>>();
            _EmailerService.Setup(s => s.QuickSend(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MailPriority>(), It.IsAny<string[]>(),
                    It.IsAny<List<string>>()))
                .Callback<bool, string, string, MailPriority, string[], List<string>>((h, b, s, p, t, a) =>
                    emailsSent.Add(new Tuple<string, string, MailPriority, string[]>(b, s, p, t)))
                .Returns(true);

            var engine = _GetInventoryProgramsProcessingEngine();
            engine.UT_CurrentDateTime = new DateTime(2020, 03, 06, 14, 22, 35);

            /*** Act ***/
            var results = engine.ProcessInventoryJob(jobId);

            /*** Assert ***/
            Assert.NotNull(results);
            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);

            // verify the file was exported well
            Assert.AreEqual(1, createdFiles.Count);
            Assert.AreEqual(@"testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Export\ProgramGuideInventoryExportFile_20200306_142235.csv", createdFiles[0].Item1);
            Assert.AreEqual(7, createdFiles[0].Item2.Count);
            for (var i = 0; i < 7; i++)
            {
                Assert.AreEqual(expectedResultFileLines[i], createdFiles[0].Item2[i]);
            }

            // verify that the email was sent
            Assert.AreEqual(1, emailsSent.Count);
            var body = emailsSent[0].Item1;
            Assert.IsTrue(body.Contains("A ProgramGuide Interface file has been exported."));
            Assert.IsTrue(body.Contains("Inventory File Id : 21"));
            Assert.IsTrue(body.Contains("Inventory File Name : TestInventoryFileName"));
            Assert.IsTrue(body.Contains("Inventory Source : NumberOneSource"));
            Assert.IsTrue(body.Contains(@"testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Export\ProgramGuideInventoryExportFile_20200306_142235.csv"));

            Assert.AreEqual("Broadcast Inventory Programs - ProgramGuide Interface Export file available", emailsSent[0].Item2);
            Assert.AreEqual(MailPriority.Normal, emailsSent[0].Item3);
            Assert.IsTrue(emailsSent[0].Item4.Any());
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
                _GenreCacheMock.Object,
                _FileService.Object,
                _EmailerService.Object);
            return engine;
        }
    }
}