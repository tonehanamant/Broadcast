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
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    [TestFixture]
    public class InventoryProgramsBySourceUnprocessedProcessorUnitTests
    {
        private Mock<IInventoryRepository> _InventoryRepo = new Mock<IInventoryRepository>();
        private Mock<IInventoryProgramsBySourceJobsRepository> _InventoryProgramsBySourceJobsRepo = new Mock<IInventoryProgramsBySourceJobsRepository>();
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaWeekCache = new Mock<IMediaMonthAndWeekAggregateCache>();
        private Mock<IProgramGuideApiClient> _ProgramGuidClient = new Mock<IProgramGuideApiClient>();
        private Mock<IStationMappingService> _StationMappingService = new Mock<IStationMappingService>();
        private Mock<IGenreCache> _GenreCacheMock = new Mock<IGenreCache>();

        private Mock<IFileService> _FileService = new Mock<IFileService>();
        private Mock<IEmailerService> _EmailerService = new Mock<IEmailerService>();

        [Test]
        public void BySourceUnprocessedJob()
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
            var guideResponse = _GetGuideResponse();

            var getInventoryBySourceUnprocessedCalled = 0;
            _InventoryRepo.Setup(r => r.GetInventoryBySourceWithUnprocessedPrograms(It.IsAny<int>(), It.IsAny<List<int>>()))
                .Callback(() => getInventoryBySourceUnprocessedCalled++)
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
                    JobGroupId = new Guid("33a4940e-e0e7-4ccd-9dda-de0063b3ab40"),
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
            Assert.IsTrue(getInventorySourceCalled > 0);
            Assert.AreEqual(1, getDisplayMediaWeekByFlightCalled);
            Assert.IsTrue(inventoryProgramsBySourceJobsRepoCalls > 0);
            Assert.AreEqual(1, getInventoryBySourceUnprocessedCalled);

            Assert.AreEqual(1, setJobCompleteSuccessCalled);
            Assert.AreEqual(0, setJobCompleteWarningCalled);
            Assert.AreEqual(0, setJobCompleteErrorCalled);

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
            Assert.IsTrue(body.Contains("JobGroupID : 33a4940e-e0e7-4ccd-9dda-de0063b3ab40"));
            Assert.IsTrue(body.Contains("Inventory Source : NumberOneSource"));
            Assert.IsTrue(body.Contains("Range Start Date : 2020-01-01"));
            Assert.IsTrue(body.Contains("Range End Date : 2020-01-21"));
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
                            GenreSourceId = 2,
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
                            GenreSourceId = 2,
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

        private InventoryProgramsBySourceUnprocessedProcessorTestClass _GetInventoryProgramsProcessingEngine()
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

            var engine = new InventoryProgramsBySourceUnprocessedProcessorTestClass(
                dataRepoFactory.Object, 
                _ProgramGuidClient.Object,
                _StationMappingService.Object, 
                _MediaWeekCache.Object,
                _GenreCacheMock.Object,
                _FileService.Object,
                _EmailerService.Object);

            return engine;
        }
    }
}