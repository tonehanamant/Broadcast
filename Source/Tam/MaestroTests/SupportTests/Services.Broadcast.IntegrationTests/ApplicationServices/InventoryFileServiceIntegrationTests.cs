using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.IntegrationTests.Stubbs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Common.Formatters;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryFileServiceIntegrationTests
    {
        private IInventoryService _InventoryFileService;
        private IStationInventoryGroupService _StationInventoryGroupService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationInventoryGroupService>();
        private IInventoryRepository _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        private IInventoryFileRepository _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
        private static InventorySource _ttwnInventorySource;
        private static InventorySource _cnnInventorySource;
        private static InventorySource _openMarketInventorySource;

        public InventoryFileServiceIntegrationTests()
        {
            _ttwnInventorySource = _InventoryRepository.GetInventorySourceByName("TTWN");
            _cnnInventorySource = _InventoryRepository.GetInventorySourceByName("CNN");
            _openMarketInventorySource = _InventoryRepository.GetInventorySourceByName("Open Market");
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IDataLakeFileService, DataLakeFileServiceStub>();
            _InventoryFileService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InventoryFileLoadCNN()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(
                        @".\Files\CNNAMPMBarterObligations_Clean.xlsx",
                        FileMode.Open,
                        FileAccess.Read),
                    FileName = "CNNAMPMBarterObligations_Clean.xlsx",
                    InventorySource = "CNN",
                    UserName = "IntegrationTestUser",
                    RatingBook = 416,
                    AudiencePricing = new List<AudiencePricingDto>() { new AudiencePricingDto() { AudienceId = 13, Price = 210 }, new AudiencePricingDto() { AudienceId = 14, Price = 131 } }
                };
                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsNotNull(result.FileId);
                // for this we are only concern with "AM New"
                var daypartCodes = new List<string>() { "AM News", "PM News" };
                VerifyInventory(_cnnInventorySource, daypartCodes);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InventoryFileLoad_OpenMarket()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var weeksBefore = _InventoryRepository.GetStationInventoryManifestsByFileId(232995).SelectMany(x => x.ManifestWeeks).ToList();

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(
                        @".\Files\ImportingRateData\4Q18\4Q18 Baltimore-WMAR-SYN.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    FileName = "4Q18 Baltimore-WMAR-SYN.xml",
                    InventorySource = "Open Market",
                    UserName = "IntegrationTestUser",
                    RatingBook = 416,
                    AudiencePricing = new List<AudiencePricingDto>() { new AudiencePricingDto() { AudienceId = 13, Price = 210 }, new AudiencePricingDto() { AudienceId = 14, Price = 131 } }
                };
                var result = _InventoryFileService.SaveInventoryFile(request);
                Assert.IsNotNull(result.FileId);
                Assert.AreNotEqual(232995, result.FileId);

                var weeksAfter = _InventoryRepository.GetStationInventoryManifestsByFileId(result.FileId).SelectMany(x => x.ManifestWeeks).ToList();
                
                Assert.AreEqual(weeksAfter.Single(x=>x.MediaWeek.Id == 775).EndDate, new DateTime(2018,10,30));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(new { weeksBefore, weeksAfter}, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFileWithAvailLineWithPeriods()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\1Chicago WLS Syn 4Q16.xml");
                int stationCode = 5060; // for station WLS

               _InventoryFileService.SaveInventoryFile(request);

                var results = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market"}, stationCode);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "InventoryFileId");
                jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(results, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFile_WithDayDetailedPeriodAttribute()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\1Q18 EMN Rates\Albany - WNYT - EM-NN - 1q18.xml");
                int stationCode = 5491; // for station WNYT

                _InventoryFileService.SaveInventoryFile(request);

                var results = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCode);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "InventoryFileId");
                jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(results, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFile_WithProgramNameLengthLongerThan63Symbols()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\1Q18 ENLN Rates\OKC.KOKH-KOCB.EN-LN.xml");
                int stationCode = 6820; // for station KOCB

                _InventoryFileService.SaveInventoryFile(request);

                var results = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCode);
                var resultsHaveProgramWithNameLengthLongerThan63Symbols = results.SelectMany(x=>x.ManifestDayparts).Any(x => x.ProgramName.Length > 63);

                Assert.True(resultsHaveProgramWithNameLengthLongerThan63Symbols);
            }
        }

        [Ignore("InventoryFileLoadOpenMarket - used to load inventory data into integration dbs")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InventoryFileLoadOpenMarket()
        {
            foreach (var filePath in Directory.EnumerateFiles(@".\Rates"))
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filePath, FileMode.Open, FileAccess.Read),
                    FileName = Path.GetFileName(filePath),
                    InventorySource = "Open Market",
                    UserName = "IntegrationTestUser"
                };
                try
                {
                    _InventoryFileService.SaveInventoryFile(request);
                }
                catch { }
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InventoryFileUpdateCNN()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.StreamData = new FileStream(
                    @".\Files\CNNAMPMBarterObligations_Clean.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                request.FileName = "CNNAMPMBarterObligations_Clean.xlsx";
                request.InventorySource = "CNN";
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;
                request.AudiencePricing = new List<AudiencePricingDto>()
                {
                    new AudiencePricingDto() { AudienceId = 13, Price = 210 },
                    new AudiencePricingDto() { AudienceId = 14, Price = 131 }
                };

                var results = _InventoryFileService.SaveInventoryFile(request);

                request = new InventoryFileSaveRequest();
                flightWeeks = new List<FlightWeekDto>
                {
                    new FlightWeekDto()
                    {
                        StartDate = new DateTime(2016, 10, 31),
                        EndDate = new DateTime(2016, 11, 06),
                        IsHiatus = false
                    }
                };

                request.StreamData = new FileStream(
                    @".\Files\CNNAMPMBarterObligations_ForUpdate.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                request.FileName = "CNNAMPMBarterObligations_ForUpdate.xlsx";
                request.InventorySource = "CNN";
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                request.AudiencePricing = new List<AudiencePricingDto>()
                {
                    new AudiencePricingDto() { AudienceId = 13, Price = 210 },
                    new AudiencePricingDto() { AudienceId = 14, Price = 131 }
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                var daypartCodes = new List<string>() { "AM News", "PM News" };
                VerifyInventory(_cnnInventorySource, daypartCodes);
            }
        }

        private void VerifyInventory(InventorySource source, List<string> unitNames)
        {
            var inventory = _InventoryRepository.GetActiveInventoryByTypeAndUnitName(source, unitNames);

            VerifyInventoryRaw(inventory);
        }

        private static void VerifyInventoryRaw(List<StationInventoryGroup> inventory)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
            jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
            //jsonResolver.Ignore(typeof(DisplayAudience), "Id");

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(inventory, jsonSettings);
            Approvals.Verify(json);
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNCanLoadFile()
        {
            using (var tran = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTWN_06.09.17.xlsx";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    AudiencePricing = new List<AudiencePricingDto>()
                {
                    new AudiencePricingDto() { AudienceId = 13, Price = 210 },
                    new AudiencePricingDto() { AudienceId = 14, Price = 131 }
                },

                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                var daypartCodes = new List<string>() { "LN" };
                VerifyInventory(_ttwnInventorySource, daypartCodes);
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNCanUpdateInventory()
        {
            using (var tran = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                // first do initial load
                var filename = @".\Files\TTWN_06.09.17.xlsx";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",

                    RatingBook = 416,
                    AudiencePricing = new List<AudiencePricingDto>()
                {
                    new AudiencePricingDto() { AudienceId = 13, Price = 210 },
                    new AudiencePricingDto() { AudienceId = 14, Price = 131 }
                }
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                filename = @".\Files\TTWN_UPDATE_06.09.17.xlsx";
                request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    AudiencePricing = new List<AudiencePricingDto>()
                {
                    new AudiencePricingDto() { AudienceId = 13, Price = 210 },
                    new AudiencePricingDto() { AudienceId = 14, Price = 131 }
                },

                    RatingBook = 416
                };

                result = _InventoryFileService.SaveInventoryFile(request);

                var daypartCodes = new List<string>() { "LN" };
                VerifyInventory(_ttwnInventorySource, daypartCodes);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastDuplicateInventoryFileException))]
        public void ThrowsExceptionWhenLoadingSameInventoryFileAgain()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();
                var request2 = new InventoryFileSaveRequest();

                request.StreamData = new FileStream(
                    @".\Files\simple_period_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.FileName = "simple_period_rate_file_wvtm.xml";
                request.InventorySource = "Open Market";
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;
                _InventoryFileService.SaveInventoryFile(request);
                _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Test]
        public void CanLoadGenres()
        {
            using (new TransactionScopeWrapper())
            {
                var genres = _InventoryFileService.GetAllGenres();
                Assert.IsTrue(genres.Any());
            }
        }

        [Test]
        [Ignore] //Locking does not work in an integration test.
        //todo: someone figure out if this test is needed since it was alway ignored.
        public void CantLoadInventoryFileWhileStationLocked()
        {
            using (new TransactionScopeWrapper())
            {
                var stationCode = 5044; //WVTM
                var request = new InventoryFileSaveRequest();

                request.StreamData = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";

                _InventoryFileService.LockStation(stationCode);
                try
                {
                    _InventoryFileService.SaveInventoryFile(request);
                    Assert.Fail("Should have thrown a lock exception.");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Some error", e.Message);
                }
                finally
                {
                    _InventoryFileService.UnlockStation(stationCode);
                }

            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FindContacts()
        {
            var contactQueryString = "rogelio";

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationContact), "Id");
            jsonResolver.Ignore(typeof(StationContact), "StationId");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var result = _InventoryFileService.FindStationContactsByName(contactQueryString);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ImportNewStationContact()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();
                int stationCodeWVTM = 5044;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationContact), "Id");
                jsonResolver.Ignore(typeof(StationContact), "ModifiedDate");
                jsonResolver.Ignore(typeof(StationContact), "StationId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                //first make sure the contacts don't exist
                var stationContacts = _InventoryFileService.GetStationContacts("Open Market", stationCodeWVTM);
                Assert.AreEqual(1, stationContacts.Count);

                request.StreamData = new FileStream(
                    @".\Files\station_contact_new_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);

                //then confirm the contacts are added
                stationContacts = _InventoryFileService.GetStationContacts("Open Market", stationCodeWVTM);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationContacts, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateExistingStationContactDuringImport()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();
                int stationCodeWVTM = 5044;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationContact), "Id");
                jsonResolver.Ignore(typeof(StationContact), "ModifiedDate");
                jsonResolver.Ignore(typeof(StationContact), "StationId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                request.StreamData = new FileStream(
                    @".\Files\station_contact_new_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);
                request.StreamData = new FileStream(
                    @".\Files\station_contact_update_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                _InventoryFileService.SaveInventoryFile(request);

                var stationContacts = _InventoryFileService.GetStationContacts("Open Market", stationCodeWVTM);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationContacts, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadsInventoryFileWithUnknownSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();

                request.StreamData = new FileStream(
                    @".\Files\unknown_spot_length_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.FileName = "unknown_spot_length_rate_file_wvtm.xml";
                request.RatingBook = 416;

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "file may be invalid", MatchType = MessageMatch.Contains)]
        public void ThrowsExceptionWhenLoadingBadXmlFile()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();

                request.StreamData = new FileStream(
                    @".\Files\rate-file-invalid-schema.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "ThrowsExceptionWhenLoadingBadXmlFile";
                request.FileName = "rate-file-invalid-schema.xml";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProgramInventoryFileWithSimplePeriods() //XML structure that is not using DetailedPeriod
        {
            int stationCodeWVTM = 5044;
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(
                        @".\Files\simple_period_rate_file_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    FileName = "simple_period_rate_file_wvtm.xml"
                };

                _InventoryFileService.SaveInventoryFile(request);
                //var result = _ratesService.GetStationDetailByCode("Open Market", stationCodeWVTM).Rates.Where(p => p.Program == "Simple Period News").ToList();

                //var jsonResolver = new IgnorableSerializerContractResolver();
                ////jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                ////jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                //var jsonSettings = new JsonSerializerSettings()
                //{
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //    ContractResolver = jsonResolver
                //};
                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CannotLoadProgramWhenTheSameAlreadyExists()
        {
            //Should report no errors anymore since we are skipping existing records. 
            using (new TransactionScopeWrapper())
            {
                var request1 = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(
                        @".\Files\Open Market Duplicate Program File1.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    FileName = "Open Market Duplicate Program File1.xml"
                };

                var request2 = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(
                        @".\Files\Open Market Duplicate Program File2.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    FileName = "Open Market Duplicate Program File2.xml"
                };

                _InventoryFileService.SaveInventoryFile(request1);

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    _InventoryFileService.SaveInventoryFile(request2);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems));

            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadInventoryFileWithOverlapingFlightWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(
                        @".\Files\station_program_overlapping_flights_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
                };

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFileWithDifferentSpotLengths()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(
                        @".\Files\multi-quarter_program_rate_file_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
                };

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFile_WhenStationHasNoAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\1Q18 ENT-SYN Rates\Buffalo WBBZ SYN.xml");
                var stationCode = 7397; // for station WBBZ

                _InventoryFileService.SaveInventoryFile(request);

                var stationManifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(_openMarketInventorySource, stationCode);
                var stationManifestsDoNotHaveManifestAudiencesReferences = stationManifests.All(x => x.ManifestAudiencesReferences.Count == 0);

                Assert.IsTrue(stationManifestsDoNotHaveManifestAudiencesReferences);
            }
        }

        // This test can be usefull to check if there is exceptions during saving all the XML files from specific folder
        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFile_CheckAllFilesInSpecificDirectories_ForExceptions()
        {
            var directories = new string[]
            {
                //"1Q18 EMN Rates",
                //"1Q18 ENLN Rates",
                //"1Q18 ENT-SYN Rates"
            };

            var excludedFiles = new[]
            {
                // Cannot save the invalid Daypart:  4:30AM-4:59AM (28:30AM - 28:59AM) - OK
                @".\Files\ImportingRateData\1Q18 EMN Rates\COLUMBIA-WIS-EM-NN.xml",
                @".\Files\ImportingRateData\1Q18 ENLN Rates\COLUMBIA-WIS-EN-LN.xml",

                // Invalid station: EPRI - OK
                @".\Files\ImportingRateData\1Q18 EMN Rates\PROVIDENCE WPRI EPRI EM NN.xml",
                @".\Files\ImportingRateData\1Q18 ENLN Rates\PROVIDENCE WPRI EPRI EN LN.xml",
                @".\Files\ImportingRateData\1Q18 ENT-SYN Rates\PROVIDENCE WPRI EPRI DA EF LF.xml",

                // There are no known stations in the file - OK
                @".\Files\ImportingRateData\1Q18 EMN Rates\Colorado Springs, QRDO, EM-NN.xml",
                @".\Files\ImportingRateData\1Q18 EMN Rates\Madison ETVW EM-NN 1q18 rates 11-22-17.xml",
                @".\Files\ImportingRateData\1Q18 EMN Rates\Madison WIFS EM-NN.xml",
                @".\Files\ImportingRateData\1Q18 EMN Rates\Sarasota WSNN EM-NN.xml",
                @".\Files\ImportingRateData\1Q18 ENLN Rates\Madison WIFS EN-LN.xml",
                @".\Files\ImportingRateData\1Q18 ENLN Rates\Sarasota WSNN EN-LN.xml",
                @".\Files\ImportingRateData\1Q18 ENT-SYN Rates\Colorado Springs, QRDO, SYN (DA-EF-LF).xml",
                @".\Files\ImportingRateData\1Q18 ENT-SYN Rates\Madison ETVW SYN 1q18 rates 11-22-17.xml",
                @".\Files\ImportingRateData\1Q18 ENT-SYN Rates\Madison WIFS SYN.xml",
                @".\Files\ImportingRateData\1Q18 ENT-SYN Rates\Sarasota WSNN SYN.xml",
                @".\Files\ImportingRateData\1Q18 ENT-SYN Rates\Savannah-FSAV-SYN.xml"
            };

            var withError = new List<string>();

            foreach (var directory in directories)
            {
                var allFiles = Directory.GetFiles($@".\Files\ImportingRateData\{directory}\", "*.xml", SearchOption.TopDirectoryOnly);
                var filesForChecking = allFiles.Except(excludedFiles);

                foreach (var file in filesForChecking)
                {
                    try
                    {
                        using (new TransactionScopeWrapper())
                        {
                            Console.WriteLine($"Checking file: {file}");

                            var request = _GetInventoryFileSaveRequest(file);

                            _InventoryFileService.SaveInventoryFile(request);

                            Console.WriteLine($"File: {file} was checked");
                        }
                    }
                    catch
                    {
                        withError.Add(file);
                    }
                }
            }

            Console.WriteLine("With Errors");

            foreach (var file in withError)
            {
                Console.WriteLine(file);
            }
        }

        private InventoryFileSaveRequest _GetInventoryFileSaveRequest(string filePath)
        {
            return new InventoryFileSaveRequest
            {
                StreamData = new FileStream(filePath, FileMode.Open, FileAccess.Read),
                UserName = "IntegrationTestUser",
                RatingBook = 416,
                FileName = Path.GetFileName(filePath)
            };
        }

        private JsonSerializerSettings _GetJsonSerializerSettingsForConvertingAllStationProgramsToJson()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationProgram), "Id");
            jsonResolver.Ignore(typeof(FlightWeekDto), "Id");

            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        [Test]
        [Ignore]
        public void CanLoadTVBFile()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileLoadTest.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasStationProgramWithInvalidSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileLoadTestInvalidSpothLenght.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }


        [Ignore]
        [Test]
        public void TVBFileHasValidAudienceValues()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileHasValidAudienceCPM.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                _InventoryFileService.SaveInventoryFile(request);

                // there is no method that brings all stations with audiences. this test will bring only one valid audience
                //var wwupStation = _ratesService.GetAllStationRates("TVB", 5667).SelectMany(a => a.Audiences);
                //var kusdStation = _ratesService.GetAllStationRates("TVB", 5668).SelectMany(a => a.Audiences);
                //var wxgsStation = _ratesService.GetAllStationRates("TVB", 5669).SelectMany(a => a.Audiences);
                //var kqcdStation = _ratesService.GetAllStationRates("TVB", 5671).SelectMany(a => a.Audiences);

                // check if the correct audience is in place
                //Assert.IsNull(wwupStation.Select(a => a.Audience.AudienceString).FirstOrDefault());
                //Assert.IsNotNull(kusdStation.Select(a => a.Audience.AudienceString).FirstOrDefault());
                //Assert.IsNull(wxgsStation.Select(a => a.Audience.AudienceString).FirstOrDefault());
                //Assert.IsNull(kqcdStation.Select(a => a.Audience.AudienceString).FirstOrDefault());
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasDuplicateStationProgram()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileLoadTestDuplicateStation.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasInvalidStationProgramCPM()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileLoadTestInvalidCPM.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }


        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNFileHasInvalidStationProgramCPM()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileLoadTestInvalidCPM.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();

                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasInvalidStationProgramCPM()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileLoadTestInvalidCPM.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasInvalidStationName()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileLoadTestInvalidStation.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasDuplicateStationProgram()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileLoadTestDuplicateStation.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasStationProgramWithInvalidSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileLoadTestInvalidSpothLenght.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasInvalidStationName()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileLoadTestInvalidStation.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        public void LoadTTWNExcelFile()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\TTWN_06.09.17.xlsx";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNFileHasDuplicateStationProgram()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileLoadTestDuplicateStation.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNFileHasStationProgramWithInvalidSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNBFileLoadTestInvalidSpothLenght.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNFileHasInvalidStationName()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileLoadTestInvalidStation.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "There are no known stations in the file", MatchType = MessageMatch.Contains)]
        public void TTWNFileHasAllStationsUnknown()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileHasInvalidStations.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "There are no known stations in the file", MatchType = MessageMatch.Contains)]
        public void TVBFileHasAllStationsUnknown()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileHasInvalidStations.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }
        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "There are no known stations in the file", MatchType = MessageMatch.Contains)]
        public void CNNFileHasAllStationsUnknown()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNFileHasInvalidDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileHasInvalidDayPart.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasInvalidDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileHasInvalidDayPart.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasInvalidDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileHasInvalidDayPart.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "There are no valid dayparts in the file ", MatchType = MessageMatch.Contains)]
        public void TTWNFileHasAllDaypartInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileHasAllDaypartsInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }


        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "There are no valid dayparts in the file ", MatchType = MessageMatch.Contains)]
        public void TVBFileHasAllDaypartInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileHasAllDaypartsInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }


        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "There are no valid spot length in the file", MatchType = MessageMatch.Contains)]
        public void TTWNFileHasAllSpotLengthInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileHasAllSpotLengthInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Test]
        [Ignore]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "There are no valid spot length in the file", MatchType = MessageMatch.Contains)]
        public void TVBFileHasAllSpotLengthInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileHasAllSpotLengthInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "There are no valid spot length in the file", MatchType = MessageMatch.Contains)]
        public void CNNFileHasAllSpotLengthInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileHasAllSpotLengthInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "Unable to parse any file records", MatchType = MessageMatch.Contains)]
        public void CNNFileHasAllEntriesInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileHasAllEntriesInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "Unable to parse any file records", MatchType = MessageMatch.Contains)]
        public void TVBFileHasAllEntriesInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileHasAllEntriesInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "Unable to parse any file records", MatchType = MessageMatch.Contains)]
        public void TTWNFileHasAllEntriesInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileHasAllEntriesInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNFileHasInvalidAudience()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileHasInvalidAudience.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasInvalidAudience()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileHasInvalidAudience.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasInvalidAudience()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileHasInvalidAudience.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasInvalidAudienceAndStation()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileHasInvalidAudienceAndStation.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNFileHasInvalidAudienceAndStation()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTWNFileHasInvalidAudienceAndStation.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasInvalidAudienceAndStation()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileHasInvalidAudienceAndStation.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasBlankCpm()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileHasBlankCpm.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasInvalidDaypartCode()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\CNNFileHasInvalidDaypartCode.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }

        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TTWNFileHasInvalidDaypartCode()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\TTWNFileHasInvalidDaypartCode.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }

        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasInvalidDaypartCode()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\TVBFileHasInvalidDaypartCode.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        public void TTWNCanLoadFileWithDaypartCodeWithSpaces()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithDaypartCodeWithSpace.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }

        }

        [Test]
        [Ignore]
        public void CNNCanLoadFileWithDaypartCodeWithSpaces()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithDaypartCodeWithSpace.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }

        }

        [Ignore]
        [Test]
        public void TVBCanLoadFileWithDaypartCodeWithSpaces()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithDaypartCodeWithSpace.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }

        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadInitialRatesData()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var initialRatesData = _InventoryFileService.GetInitialRatesData();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(initialRatesData));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanCalculateSpotCost()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\TTWNFileLoadTest.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 413
                };

                _InventoryFileService.SaveInventoryFile(request);

                //var rates = _ratesService.GetAllStationRates("TTWN", 1003);

                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(rates));
            }
        }

        [Test]
        [Ignore]
        public void TTWNCanLoadFileWithFixedPriceColumn()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithFixedPriceColumn.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }
        }

        [Test]
        [Ignore]
        public void CNNCanLoadFileWithFixedPriceColumn()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithFixedPriceColumn.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }
        }

        [Ignore]
        [Test]
        public void TVBCanLoadFileWithFixedPriceColumn()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithFixedPriceColumn.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "Daypart code", MatchType = MessageMatch.Contains)]
        public void TTWNThrowExceptionWhenMultipleFixedPriceForSameDaypart()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithMultipleFixedPriceSameDaypart.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "Daypart code", MatchType = MessageMatch.Contains)]
        public void CNNThrowExceptionWhenMultipleFixedPriceForSameDaypart()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithMultipleFixedPriceSameDaypart.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "Daypart code", MatchType = MessageMatch.Contains)]
        public void TVBThrowExceptionWhenMultipleFixedPriceForSameDaypart()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithMultipleFixedPriceSameDaypart.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Ignore]
        [Test]
        public void TVBCanLoadFileWithSameSpotLengthDaypartAndStation()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithSameStationDaypartSpotLength.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Test]
        [Ignore]
        public void TTWNCanLoadFileWithSameSpotLengthDaypartAndStation()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithSameStationDaypartSpotLength.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }
        }

        [Test]
        [Ignore]
        public void CNNCanLoadFileWithSameSpotLengthDaypartAndStation()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithSameStationDaypartSpotLength.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Ignore]
        [Test]
        public void CanUpdateFixedPriceForThirdParty()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string path = @".\Files\ThirdPartyFileWithFixedPriceColumnUpdate.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(path, FileMode.Open, FileAccess.Read),
                    FileName = path,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                //TODO: get rate info for comparison
                //var stationBeforeUpdate = _stationProgramRepository.GetStationProgramById(320136);

                _InventoryFileService.SaveInventoryFile(request);

                //var stationAfterUpdate = _stationProgramRepository.GetStationProgramById(320136);

                //Assert.AreEqual(7000m, stationBeforeUpdate.fixed_price);
                //Assert.AreEqual(7500m, stationAfterUpdate.fixed_price);
            }
        }

        [Test]
        public void CanConvert30sRateTo15sRate()
        {
            var result = _InventoryFileService.ConvertRateForSpotLength(10, 15);
            Assert.AreEqual(6.5, result);
        }

        [Test]
        public void StationConflictsUpdateConflictTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var conflicted = _InventoryFileService.GetStationProgramConflicted(new StationProgramConflictRequest
                {
                    Airtime = DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(1)),
                    RateSource = "Open Market",
                    StartDate = new DateTime(2017, 12, 01),
                    EndDate = new DateTime(2018, 01, 01),
                    ConflictedProgramNewEndDate = new DateTime(2018, 01, 25),
                    ConflictedProgramNewStartDate = new DateTime(2018, 01, 02),
                    StationCode = 7353,
                }, 24538);

                Assert.IsFalse(conflicted);
            }
        }

        [Test]
        public void HasSpotsAllocatedTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var hasSpotAllocated = _InventoryFileService.HasSpotsAllocated(26672);

                Assert.IsTrue(hasSpotAllocated);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFileWithUnknownStation()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\1Chicago WLS Syn 4Q16 UNKNOWN.xml");

                var result = _InventoryFileService.SaveInventoryFile(request);

                var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(result.FileId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "InventoryFileId");
                jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
                jsonResolver.Ignore(typeof(DisplayAudience), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "StartDate");
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "EndDate");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(manifests, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFileWithUnknownStation_Contacts()
        {
            var contactQueryString = "Hanington, Jack";

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationContact), "Id");
            jsonResolver.Ignore(typeof(StationContact), "StationId");
            jsonResolver.Ignore(typeof(StationContact), "ModifiedDate");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\1Chicago WLS Syn 4Q16 UNKNOWN.xml");
                _InventoryFileService.SaveInventoryFile(request);

                var result = _InventoryFileService.FindStationContactsByName(contactQueryString);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        public void CanLoadHudsonOpenMarketInventoryFile()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\CTV-Broadcast.xml");

                var problems = new List<InventoryFileProblem>();

                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }
        }

        [Test]
        public void CanLoadOpenMarketInventoryFileWithoutDuplicatingStationContacts()
        {
            using (new TransactionScopeWrapper())
            {
                // Loading a file with a contact that already exists in the database.
                var request = _GetInventoryFileSaveRequest(@".\Files\CTV-Broadcast-2.xml");

                var problems = new List<InventoryFileProblem>();

                try
                {
                    var result = _InventoryFileService.SaveInventoryFile(request);
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                var stationContacts = _InventoryFileService.GetStationContacts("Open Market", 5879);

                Assert.AreEqual(1, stationContacts.Count);

                Assert.IsEmpty(problems);
            }
        }

        [Ignore("This test needs to be reviewed")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadInventoryFileAndFillAllSpotLegnthsWhenSpotLengthIs30()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();
                int stationCodeWVTM = 5044;

                request.StreamData = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);

                var manifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCodeWVTM);

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                // only 15 and 30 are used at the moment.
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(manifests.SelectMany(x=>x.ManifestRates), jsonSettings));
            }
        }

        [Ignore("This test needs to be reviewed")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadInventoryFileAndFillOnlyOneSpotLegnth()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();
                int stationCodeWVTM = 5044;

                request.StreamData = new FileStream(
                    @".\Files\single_program_rate_file_spot_length_15_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);

                var manifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCodeWVTM);

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(manifests.SelectMany(x => x.ManifestRates), jsonSettings));
            }
        }

        [Ignore("This test needs to be reviewed")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        //[ExpectedException(typeof(System.Exception), ExpectedMessage = "Unable to find media week containing date 1/24/1988", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenEndDateIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(
                        @".\Files\end_program_flight_file_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
                };

                _InventoryFileService.SaveInventoryFile(request);
                var stationCodeWVTM = 1027;
                var endDate = DateFormatter.AdjustEndDate(new DateTime(1988, 01, 20));
                var manifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCodeWVTM);
                var program = manifests.SelectMany(x=>x.ManifestDayparts).Single(q => q.ProgramName == "TR_WVTM-TV_TEST_1 11:30AM");

                //this method is no longer in the codebase
                //_ratesService.TrimProgramFlight(program.Id, endDate, endDate, "IntegrationTestUser");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(program));
            }
        }
        
        [Ignore("This test needs to be reviewed")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProgramRateWithAirtimeStartOver24h()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(
                        @".\Files\program_rate_over24h_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
                };

                _InventoryFileService.SaveInventoryFile(request);
                var stationCodeWVTM = 5044;
                var startDate = new DateTime(2016, 9, 26);
                var endDate = new DateTime(2016, 10, 09);
                var manifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCodeWVTM);
                var daypart = manifests.SelectMany(x=>x.ManifestDayparts).Where(p => p.ProgramName == "CADENT NEWS AFTER MIDNIGHT").Single();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypart));
                //Assert.AreEqual("M-F 2AM-4AM", rate.Daypart.a.Airtime);
            }
        }
    }
}
