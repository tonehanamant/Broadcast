using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Common.Formatters;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryFileServiceIntegrationTests
    {
        private IInventoryService _InventoryService;
        private IStationInventoryGroupService _StationInventoryGroupService;
        private IInventoryRepository _InventoryRepository;
        private IInventoryFileRepository _InventoryFileRepository;
        private static InventorySource _openMarketInventorySource;
        private InventoryFileTestHelper _InventoryFileTestHelper;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
        private static IFeatureToggleHelper _FeatureToggleHelper;

        [SetUp]
        public void SetUp()
        {
            _LaunchDarklyClientStub = (LaunchDarklyClientStub)IntegrationTestApplicationServiceFactory.Instance.Resolve<ILaunchDarklyClient>();
            _InventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();
            _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _StationInventoryGroupService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationInventoryGroupService>();
            _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _openMarketInventorySource = _InventoryRepository.GetInventorySourceByName("Open Market");
            _InventoryFileTestHelper = new InventoryFileTestHelper();
            _FeatureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void InventoryFileLoad_OpenMarket_NegativeDemoValue()
        {
            const string fileName = "Open Market negative demo value.xml";

            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void InventoryFileLoad_UnmappedStation()
        {
            const string fileName = "Unmapped Station.xml";

            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
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
                    RatingBook = 416,
                    AudiencePricing = new List<AudiencePricingDto>() { new AudiencePricingDto() { AudienceId = 13, Price = 210 }, new AudiencePricingDto() { AudienceId = 14, Price = 131 } }
                };
                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                Assert.IsNotNull(result.FileId);
                Assert.AreNotEqual(232995, result.FileId);

                var weeksAfter = _InventoryRepository.GetStationInventoryManifestsByFileId(result.FileId).SelectMany(x => x.ManifestWeeks).ToList();

                Assert.AreEqual(weeksAfter.Single(x => x.MediaWeek.Id == 775).EndDate, new DateTime(2018, 10, 30));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifestWeek), "Id");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(new { weeksBefore, weeksAfter }, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [Category("short_running")]
        public void InventoryFileLoad_IsProprietaryFIle()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var openMarketFile = @".\Files\ImportingRateData\4Q18\4Q18 Baltimore-WMAR-SYN.xml";
                Assert.IsFalse(_InventoryService.IsProprietaryFile(openMarketFile));

                var barterFile = @".\Files\ProprietaryDataFiles\Barter_BadFormats_PRI5379.xlsx";
                Assert.IsTrue(_InventoryService.IsProprietaryFile(barterFile));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void CanLoadOpenMarketInventoryFileWithAvailLineWithPeriods()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\1Chicago WLS Syn 4Q16.xml");
                int stationCode = 5060; // for station WLS

                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

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
        [Category("short_running")]
        public void CanLoadOpenMarketInventoryFile_WithDayDetailedPeriodAttribute()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\1Q18 EMN Rates\Albany - WNYT - EM-NN - 1q18.xml");
                int stationCode = 5491; // for station WNYT

                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

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
        [Category("short_running")]
        public void CanLoadOpenMarketInventoryFile_WithProgramNameLengthLongerThan63Symbols()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\1Q18 ENLN Rates\OKC.KOKH-KOCB.EN-LN.xml");
                int stationCode = 6820; // for station KOCB

                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var results = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCode);
                var resultsHaveProgramWithNameLengthLongerThan63Symbols = results.SelectMany(x => x.ManifestDayparts).Any(x => x.ProgramName.Length > 63);

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
                };
                try
                {
                    _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch { }
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

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(inventory, jsonSettings);
            Approvals.Verify(json);
        }

        [Test]
        [Category("short_running")]
        [ExpectedException(typeof(BroadcastDuplicateInventoryFileException), ExpectedMessage = "The selected rate file has already been loaded or is already loading", MatchType = MessageMatch.Contains)]
        public void ThrowsExceptionWhenLoadingSameInventoryFileAgain()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\Fresno_4Q18_ValidFile.xml");
                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
            }
        }

        [Test]
        [Category("short_running")]
        public void CanLoadGenres()
        {
            using (new TransactionScopeWrapper())
            {
                var genres = _InventoryService.GetAllMaestroGenres();
                Assert.IsTrue(genres.Any());
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
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

            var result = _InventoryService.FindStationContactsByName(contactQueryString);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ImportNewStationContact()
        {
            using (new TransactionScopeWrapper())
            {
                int stationCodeWVTM = 5044;
                string filename = @".\Files\station_contact_new_rate_file_wvtm.xml";
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
                var stationContacts = _InventoryService.GetStationContacts("Open Market", stationCodeWVTM);
                Assert.AreEqual(1, stationContacts.Count);

                var request = _GetInventoryFileSaveRequest(filename);
                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                //then confirm the contacts are added
                stationContacts = _InventoryService.GetStationContacts("Open Market", stationCodeWVTM);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationContacts, jsonSettings));
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateExistingStationContactDuringImport()
        {
            using (new TransactionScopeWrapper())
            {
                int stationCodeWVTM = 5044;
                string filename = @".\Files\station_contact_new_rate_file_wvtm.xml";
                string anotherFileName = @".\Files\station_contact_update_rate_file_wvtm.xml";

                var request = _GetInventoryFileSaveRequest(filename);
                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var request2 = _GetInventoryFileSaveRequest(anotherFileName);
                _InventoryService.SaveInventoryFile(request2, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var stationContacts = _InventoryService.GetStationContacts("Open Market", stationCodeWVTM);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationContact), "Id");
                jsonResolver.Ignore(typeof(StationContact), "ModifiedDate");
                jsonResolver.Ignore(typeof(StationContact), "StationId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationContacts, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void LoadsInventoryFileWithUnknownSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                string fileName = @".\Files\unknown_spot_length_rate_file_wvtm.xml";
                var request = _GetInventoryFileSaveRequest(fileName);

                var result = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>().SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));


                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Assert.IsTrue(result.FileId > 0);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void OpenMarket_InvalidFile()
        {
            using (new TransactionScopeWrapper())
            {
                string filename = @".\Files\ImportingRateData\San Francisco_InvalidFile.xml";
                var request = _GetInventoryFileSaveRequest(filename);

                var result = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>().SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Assert.IsTrue(result.FileId > 0);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void CanLoadProgramInventoryFileWithSimplePeriods() //XML structure that is not using DetailedPeriod
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\simple_period_rate_file_wvtm.xml");
                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Assert.IsTrue(result.FileId > 0);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void CannotLoadProgramWhenTheSameAlreadyExists()
        {
            //Should report no errors anymore since we are skipping existing records. 
            using (new TransactionScopeWrapper())
            {
                var request1 = _GetInventoryFileSaveRequest(@".\Files\Open Market Duplicate Program File1.xml");

                var request2 = _GetInventoryFileSaveRequest(@".\Files\Open Market Duplicate Program File2.xml");

                _InventoryService.SaveInventoryFile(request1, "IntegrationTestUser", new DateTime(2016, 09, 26));

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    _InventoryService.SaveInventoryFile(request2, "IntegrationTestUser", new DateTime(2016, 09, 26));
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
        [Category("short_running")]
        public void LoadInventoryFileWithOverlapingFlightWeeks()
        {
            var service = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();

            using (new TransactionScopeWrapper())
            {
                string filename = @".\Files\station_program_overlapping_flights_wvtm.xml";
                var request = _GetInventoryFileSaveRequest(filename);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var result = service.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Category("short_running")]
        public void CanLoadOpenMarketInventoryFileWithDifferentSpotLengths()
        {
            using (new TransactionScopeWrapper())
            {
                string fileName = @".\Files\multi-quarter_program_rate_file_wvtm.xml";
                var request = _GetInventoryFileSaveRequest(fileName);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                Assert.IsTrue(result.ValidationProblems.Count == 0);
                Assert.IsTrue(result.FileId > 0);
                Assert.IsTrue(result.Status == Entities.Enums.FileStatusEnum.Loaded);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void CanLoadOpenMarketInventoryFile_WhenStationHasNoAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\1Q18 ENT-SYN Rates\Buffalo WBBZ SYN.xml");
                var stationCode = 7397; // for station WBBZ

                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var stationManifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(_openMarketInventorySource, stationCode);
                var stationManifestsDoNotHaveManifestAudiencesReferences = stationManifests.All(x => x.ManifestAudiencesReferences.Count == 0);

                Assert.IsTrue(stationManifestsDoNotHaveManifestAudiencesReferences);
            }
        }

        // This test can be usefull to check if there is exceptions during saving all the XML files from specific folder
        [Test]
        [Ignore("Not certain why we are ignoring this...")]
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

                            _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

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

        [Test]
        [Ignore("Not certain why we are ignoring this...")]
        public void CanLoadTVBFile()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileLoadTest.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasStationProgramWithInvalidSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TVBFileLoadTestInvalidSpothLength.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }


        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

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
        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
            }
        }

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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
            }
        }

        [Test]
        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    FileName = filename
                };
                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
            }
        }

        [Test]
        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(problems, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void CanLoadInitialRatesData()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var initialRatesData = _InventoryService.GetInitialRatesData();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(initialRatesData));
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TTWN",
                    RatingBook = 413
                };

                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                //var rates = _ratesService.GetAllStationRates("TTWN", 1003);

                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(rates));
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Ignore("Not certain why we are ignoring this...")]
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
                    InventorySource = "TVB",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Test]
        [Ignore("Not certain why we are ignoring this...")]
        public void TTWNCanLoadFileWithSameSpotLengthDaypartAndStation()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithSameStationDaypartSpotLength.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    InventorySource = "TTWN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }
        }

        [Test]
        [Ignore("Not certain why we are ignoring this...")]
        public void CNNCanLoadFileWithSameSpotLengthDaypartAndStation()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithSameStationDaypartSpotLength.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    InventorySource = "CNN",
                    RatingBook = 416
                };

                List<InventoryFileProblem> problems = new List<InventoryFileProblem>();
                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }
                Assert.IsEmpty(problems);
            }
        }

        [Test]
        [Category("short_running")]
        public void CanConvert30sRateTo15sRate()
        {
            var result = _InventoryService.ConvertRateForSpotLength(10, 15);
            Assert.AreEqual(6.5, result);
        }

        [Test]
        [Category("short_running")]
        public void StationConflictsUpdateConflictTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var conflicted = _InventoryService.GetStationProgramConflicted(new StationProgramConflictRequest
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
        [Category("short_running")]
        public void HasSpotsAllocatedTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var hasSpotAllocated = _InventoryService.HasSpotsAllocated(26672);

                Assert.IsTrue(hasSpotAllocated);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Ignore("Not certain why we are ignoring this...")] // This does not apply after PRI-22785
        public void CanLoadOpenMarketInventoryFileWithUnknownStation()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\1Chicago WLS Syn 4Q16 UNKNOWN.xml");

                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

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
        [Ignore("Not certain why we are ignoring this...")] // This does not apply after PRI-22785
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
                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var result = _InventoryService.FindStationContactsByName(contactQueryString);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void CanLoadHudsonOpenMarketInventoryFile()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\CTV-Broadcast.xml");

                var problems = new List<InventoryFileProblem>();

                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                Assert.IsEmpty(problems);
            }
        }

        [Test]
        [Category("short_running")]
        public void CanLoadOpenMarketInventoryFileWithoutDuplicatingStationContacts()
        {
            using (new TransactionScopeWrapper())
            {
                // Loading a file with a contact that already exists in the database.
                var request = _GetInventoryFileSaveRequest(@".\Files\CTV-Broadcast-2.xml");

                var problems = new List<InventoryFileProblem>();

                try
                {
                    var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                }
                catch (FileUploadException<InventoryFileProblem> e)
                {
                    problems = e.Problems;
                }

                var stationContacts = _InventoryService.GetStationContacts("Open Market", 5879);

                Assert.AreEqual(1, stationContacts.Count);

                Assert.IsEmpty(problems);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void CanLoadInventoryFileAndFillAllSpotLegnthsWhenSpotLengthIs30()
        {
            using (new TransactionScopeWrapper())
            {
                int stationCodeWVTM = 5044;
                string filename = @".\Files\single_program_rate_file_wvtm.xml";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    RatingBook = 416,
                    FileName = filename
                };

                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var manifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCodeWVTM);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                // only 15 and 30 are used at the moment.
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(manifests.SelectMany(x => x.ManifestRates), jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void CanLoadInventoryFileAndCheckUserName()
        {
            using (new TransactionScopeWrapper())
            {
                string filename = @".\Files\single_program_rate_file_wvtm.xml";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    RatingBook = 416,
                    FileName = filename
                };
                _SetFeatureToggle(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION, true);

                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var inventoryFile = _InventoryFileRepository.GetInventoryFileById(result.FileId);
                var expectedResult = "IntegrationTestUser";
                Assert.AreEqual(expectedResult, inventoryFile.CreatedBy);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void CanLoadInventoryFileAndFillOnlyOneSpotLegnth()
        {
            using (new TransactionScopeWrapper())
            {
                int stationCodeWVTM = 5044;

                string filename = @".\Files\single_program_rate_file_spot_length_15_wvtm.xml";
                var request = _GetInventoryFileSaveRequest(filename);
                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));

                var manifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCodeWVTM);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(StationInventoryManifestRate), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(manifests.SelectMany(x => x.ManifestRates), jsonSettings));
            }
        }

        [Ignore("The test is not throwing any errors")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ThrowExceptionWhenEndDateIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\end_program_flight_file_wvtm.xml");

                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                var stationCodeWVTM = 1027;
                var endDate = DateFormatter.AdjustEndDate(new DateTime(1988, 01, 20));
                var manifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCodeWVTM);
                var program = manifests.SelectMany(x => x.ManifestDayparts).Single(q => q.ProgramName == "TR_WVTM-TV_TEST_1 11:30AM");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(program, jsonSettings));
            }
        }

        [Ignore("This test needs to be reviewed because the last assert is testing for something not in the approval file")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProgramRateWithAirtimeStartOver24h()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\program_rate_over24h_wvtm.xml");

                _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
                var stationCodeWVTM = 5044;
                var startDate = new DateTime(2016, 9, 26);
                var endDate = new DateTime(2016, 10, 09);
                var manifests = _InventoryRepository.GetStationManifestsBySourceAndStationCode(new InventorySource { Name = "Open Market" }, stationCodeWVTM);
                var daypart = manifests.SelectMany(x => x.ManifestDayparts).Where(p => p.ProgramName == "CADENT NEWS AFTER MIDNIGHT").Single();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypart));
                //Assert.AreEqual("M-F 2AM-4AM", rate.Daypart.a.Airtime);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void CanGetOpenMarketUploadHistory()
        {
            var inventorySourceId = 1; //OpenMarket

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadOpenMarketInventoryFile("Open Market projected imps.xml");

                var result = _InventoryService.GetInventoryUploadHistory(inventorySourceId, null, null);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryUploadHistoryDto), "FileId");

                var serializer = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, serializer));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void SuccessfullyLoadsOpenMarketFile_MultithreadingTesting()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\CTV-Broadcast-2E-100-2019-04-10-17-59-16.xml", FileMode.Open, FileAccess.Read),
                    FileName = "CTV-Broadcast-2E-100-2019-04-10-17-59-16.xml"
                };
                
                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2019, 02, 02));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");

                var serializer = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, serializer));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void CanGetOpenMarketUploadHistoryWithQuarterFilter()
        {
            var inventorySourceId = 1; //OpenMarket

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadOpenMarketInventoryFile("Open Market projected imps.xml");

                var result = _InventoryService.GetInventoryUploadHistory(inventorySourceId, 1, 2018);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryUploadHistoryDto), "FileId");

                var serializer = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, serializer));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void CanGetProprietaryUploadHistory()
        {
            var inventorySourceId = 3; // TVB

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_Q1_2025.xlsx", processInventoryRatings: true);

                var result = _InventoryService.GetInventoryUploadHistory(inventorySourceId, null, null);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryUploadHistoryDto), "FileId");

                var serializer = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, serializer));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void CanGetProprietaryUploadHistoryWithQuarterFIlter()
        {
            var inventorySourceId = 3; // TVB

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_Q1_2025.xlsx", processInventoryRatings: true);

                var result = _InventoryService.GetInventoryUploadHistory(inventorySourceId, 1, 2025);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryUploadHistoryDto), "FileId");

                var serializer = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, serializer));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ProprietaryImportStationValidation()
        {
            // Arrange
            const string fileName = "Barter_ImportStationValidation.xlsx";

            using (new TransactionScopeWrapper())
            {
                // Act
                var result = _InventoryFileTestHelper.UploadProprietaryInventoryFile(fileName, processInventoryRatings: false);

                // Assert
                var inventory = _InventoryRepository.GetStationInventoryManifestsByFileId(result.FileId);
                
                // validate the station list is what we think it should be.
                // order should match
                var inventoryStations = inventory.OrderBy(i => i.Id).Select(i => i.Station).ToList();
                var toValidate = new { UploadResult = result, inventoryStations };

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var serializer = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate, serializer));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ProprietaryImportUnmappedStations()
        {
            // Arrange
            const string fileName = "Barter_UnmappedStations.xlsx";

            using (new TransactionScopeWrapper())
            {
                // Act
                var result = _InventoryFileTestHelper.UploadProprietaryInventoryFile(fileName, processInventoryRatings: false);

                // Assert
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");

                var serializer = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, serializer));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void CanGetDiginetUploadHistory()
        {
            var inventorySourceId = 20; // COZI

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Diginet_ValidFile3.xlsx", processInventoryRatings: true);

                var result = _InventoryService.GetInventoryUploadHistory(inventorySourceId, null, null);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryUploadHistoryDto), "FileId");

                var serializer = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, serializer));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void CanImportDiginetFileThatContainsUnmappedStations()
        {
            var inventorySourceId = 20; // COZI

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Diginet_ValidFile_WithUnmappedStations.xlsx", processInventoryRatings: true);

                var result = _InventoryService.GetInventoryUploadHistory(inventorySourceId, null, null);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryUploadHistoryDto), "FileId");

                var serializer = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, serializer));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void CanGetUploadHistoryQuartersOpenMarket()
        {
            // Open Market.
            var inventorySourceId = 1;

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadOpenMarketInventoryFile("Open Market projected imps.xml");

                var result = _InventoryService.GetInventoryUploadHistoryQuarters(inventorySourceId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void CanGetUploadHistoryQuartersProprietary()
        {
            // TVB.
            var inventorySourceId = 3;

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_Q1_2025.xlsx", processInventoryRatings: true);

                var result = _InventoryService.GetInventoryUploadHistoryQuarters(inventorySourceId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        private void _SetFeatureToggle(string feature, bool activate)
        {
            if (_LaunchDarklyClientStub.FeatureToggles.ContainsKey(feature))
                _LaunchDarklyClientStub.FeatureToggles[feature] = activate;
            else
                _LaunchDarklyClientStub.FeatureToggles.Add(feature, activate);
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventoryUploadHistoryQuartersTest()
        {
            var inventorySourceId = 3;

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_Q1_2025.xlsx", processInventoryRatings: true);

                var result = _InventoryService.GetInventoryUploadHistoryQuarters(inventorySourceId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventoryUploadHistoryQuartersTest_ToggleOn()
        {
            var inventorySourceId = 3;
            using (new TransactionScopeWrapper())
            {
                _SetFeatureToggle(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION, true);
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_Q1_2025.xlsx", processInventoryRatings: true);
                var result = _InventoryService.GetInventoryUploadHistoryQuarters(inventorySourceId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }


        private InventoryFileSaveRequest _GetInventoryFileSaveRequest(string filePath)
        {
            return new InventoryFileSaveRequest
            {
                StreamData = new FileStream(filePath, FileMode.Open, FileAccess.Read),
                FileName = Path.GetFileName(filePath),
                InventorySource = "Open Market"
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
        [Category("long_running")]
        public void OpenMarket_SaveErrorFileToDisk()
        {
            const string fileName = @"WilkesBarre_4Q18_InvalidFile.xml";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", now);

                var broadcastAppFolder = IntegrationTestHelper.GetBroadcastAppFolder();
                string errorsFilePath = $@"{broadcastAppFolder}\{result.FileId}_{fileName}.txt";

                var fileService = IntegrationTestApplicationServiceFactory.Instance.Resolve<IFileService>();
                Assert.IsTrue(fileService.Exists(errorsFilePath));
            }
        }        

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void OpenMarket_ExtractData_RowsProcessed()
        {
            const string fileName = @"Fresno_4Q18_ValidFile.xml";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.OpenMarket, Name = "Open Market", Id = 1 };
            var fileImporter = IntegrationTestApplicationServiceFactory.Instance.Resolve<IOpenMarketFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName,
                    InventorySource = "Open Market"
                };

                fileImporter.LoadFromSaveRequest(request);
                var file = fileImporter.GetPendingInventoryFile(inventorySource, "integration test", new DateTime(2019, 01, 01));
                fileImporter.ExtractFileData(request.StreamData, file);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileBase), "Id");
                jsonResolver.Ignore(typeof(InventoryFileBase), "CreatedDate");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var fileJson = IntegrationTestHelper.ConvertToJson(file, jsonSettings);

                Approvals.Verify(fileJson);
            }
        }

        [Ignore("This runs super long.  Run it manually.")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void OpenMarket_SaveLargeFile()
        {
            const string fileName = @"Cadent Q2 News.xml";
            //const string fileName = @"Cadent Q2 Entertainment.xml";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.OpenMarket, Name = "Open Market", Id = 1 };
            var fileImporter = IntegrationTestApplicationServiceFactory.Instance.Resolve<IOpenMarketFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName,
                    InventorySource = "Open Market"
                };

                var now = new DateTime(2019, 02, 02);
                var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", now);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var fileJson = IntegrationTestHelper.ConvertToJson(result, jsonSettings);

                Assert.IsTrue(result.FileId > 0);
                Approvals.Verify(fileJson);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void OpenMarket_InventoryIsExpired()
        {
            // The same rate file is imported twice (files contain small differences so their hash is not the same).
            // The inventory is the same.
            // No duplicate records should be created (with flight weeks).
            // The expiration logic should expire the week records from the first imported file.
            const string firstFileName = @"InventoryExpirationTest.xml";
            const string secondFileName = @"InventoryExpirationTest(2).xml";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.OpenMarket, Name = "Open Market", Id = 1 };
            var fileImporter = IntegrationTestApplicationServiceFactory.Instance.Resolve<IOpenMarketFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var now = new DateTime(2019, 02, 02);

                var firstRequest = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\{firstFileName}", FileMode.Open, FileAccess.Read),
                    FileName = firstFileName,
                    InventorySource = "Open Market"
                };

                var firstResult = _InventoryService.SaveInventoryFile(firstRequest, "IntegrationTestUser", now);

                var secondRequest = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\{secondFileName}", FileMode.Open, FileAccess.Read),
                    FileName = secondFileName,
                    InventorySource = "Open Market"
                };

                var secondResult = _InventoryService.SaveInventoryFile(secondRequest, "IntegrationTestUser", now);

                var firstFileManifests = _InventoryRepository.GetStationInventoryManifestsByFileId(firstResult.FileId);

                var secondFileManifests = _InventoryRepository.GetStationInventoryManifestsByFileId(secondResult.FileId);

                var numberOfWeeksFirstFile = firstFileManifests.Sum(x => x.ManifestWeeks.Count);

                var numberOfWeeksSecondFile = secondFileManifests.Sum(x => x.ManifestWeeks.Count);

                Assert.AreEqual(0, numberOfWeeksFirstFile);

                Assert.AreEqual(5, numberOfWeeksSecondFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void OpenMarket_InventoryIsNotExpired()
        {
            // Two rate files are imported.
            // Several programs from the files overlapped according to the old logic.
            // The old logic checked overlaps by grouping by market and daypart.
            // The new logic also checks by station.
            // There are a few programs that are repeated in the second file, so the weeks from the first are removed.
            // The thee number of weeks and programs that repeat are:
            // 13 - Matter of Fact with Soledad O'Brien
            // 8 - Chronicle
            // 13 - Chronicle
            // 13 - DAYBREAK
            // 13 - INSIDE TEXAS POLITICS
            // 13 - THIS WEEK - ABC
            // 13 - ABC NIGHTLINE
            // 13 - GMA SUNDAY
            // 13 - GMA3
            // 13 - GOOD MORNING AMERICA
            // 13 - GOOD MORNING AMERICA SAT
            // 13 - GN SPORTS
            const string firstFileName = @"1-E320-ENT-NYC-ATL.xml";
            const string secondFileName = @"1-N320-NEWS-NYC-ATL.xml";
            var inventorySource = new InventorySource { InventoryType = InventorySourceTypeEnum.OpenMarket, Name = "Open Market", Id = 1 };
            var fileImporter = IntegrationTestApplicationServiceFactory.Instance.Resolve<IOpenMarketFileImporter>();

            using (new TransactionScopeWrapper())
            {
                var now = new DateTime(2019, 02, 02);

                var firstRequest = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\{firstFileName}", FileMode.Open, FileAccess.Read),
                    FileName = firstFileName,
                    InventorySource = "Open Market"
                };

                var firstResult = _InventoryService.SaveInventoryFile(firstRequest, "IntegrationTestUser", now);

                var secondRequest = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\ImportingRateData\{secondFileName}", FileMode.Open, FileAccess.Read),
                    FileName = secondFileName,
                    InventorySource = "Open Market"
                };

                var secondResult = _InventoryService.SaveInventoryFile(secondRequest, "IntegrationTestUser", now);

                var firstFileManifests = _InventoryRepository.GetStationInventoryManifestsByFileId(firstResult.FileId);

                var secondFileManifests = _InventoryRepository.GetStationInventoryManifestsByFileId(secondResult.FileId);

                var numberOfWeeksFirstFile = firstFileManifests.Sum(x => x.ManifestWeeks.Count);

                var numberOfWeeksSecondFile = secondFileManifests.Sum(x => x.ManifestWeeks.Count);

                Assert.AreEqual(23193, numberOfWeeksFirstFile);

                Assert.AreEqual(6832, numberOfWeeksSecondFile);
            }
        }
    }
}
