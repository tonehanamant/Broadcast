using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Common.Formatters;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Entities.StationInventory;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryFileServiceIntegrationTests
    {
        private IInventoryService _InventoryFileService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();
        private IStationInventoryGroupService _StationInventoryGroupService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationInventoryGroupService>();
        private IInventoryRepository _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        private IInventoryFileRepository _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
        private static InventorySource _ttnwInventorySource;
        private static InventorySource _cnnInventorySource;
        private static InventorySource _openMarketInventorySource;

        public InventoryFileServiceIntegrationTests()
        {
            _ttnwInventorySource = _InventoryRepository.GetInventorySourceByName("TTNW");
            _cnnInventorySource = _InventoryRepository.GetInventorySourceByName("CNN");
            _openMarketInventorySource = _InventoryRepository.GetInventorySourceByName("OpenMarket");
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
                request.EffectiveDate = DateTime.Parse("10/1/2017");
                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsNotNull(result.FileId);
                // for this we are only concern with "AM New"
                var daypartCodes = new List<string>() { "AM News", "PM News" };
                VerifyInventory(_cnnInventorySource, daypartCodes);
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
                    InventorySource = "OpenMarket",
                    UserName = "IntegrationTestUser"
                };
                request.EffectiveDate = DateTime.Parse("02/06/2019");
                try
                {
                    _InventoryFileService.SaveInventoryFile(request);
                }
                catch{}                
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

                request.EffectiveDate = DateTime.Parse("10/1/2017");
                var results = _InventoryFileService.SaveInventoryFile(request);

                request = new InventoryFileSaveRequest();
                flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.StreamData = new FileStream(
                    @".\Files\CNNAMPMBarterObligations_ForUpdate.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                request.FileName = "CNNAMPMBarterObligations_ForUpdate.xlsx";
                request.InventorySource = "CNN";
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                request.EffectiveDate = DateTime.Parse("11/01/2017");
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

        private void VerifyInventory(InventorySource source, List<string> daypartCodes)
        {
            var inventory = _InventoryRepository.GetActiveInventoryByTypeAndDapartCodes(source, daypartCodes);

            VerifyInventoryRaw(inventory);
        }

        private static void VerifyInventoryRaw(List<StationInventoryGroup> inventory)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationInventoryManifestBase), "FileId");
            jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
            jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
            jsonResolver.Ignore(typeof(StationInventoryManifestBase), "Id");
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
        public void ExpireInventory()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_06.09.17.xlsx";
                var request = new InventoryFileSaveRequest();
                request.StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.InventorySource = "TTNW";
                request.EffectiveDate = DateTime.Parse("10/1/2017");
                request.AudiencePricing = new List<AudiencePricingDto>()
                {
                    new AudiencePricingDto() { AudienceId = 13, Price = 210 },
                    new AudiencePricingDto() { AudienceId = 14, Price = 131 }
                };

                var expireDate = request.EffectiveDate.AddDays(-1);

                request.RatingBook = 416;

                var result = _InventoryFileService.SaveInventoryFile(request);

                var daypartCodes = new List<string>() { "LN1" };
                var inventoryGroups = _InventoryRepository.GetActiveInventoryBySourceAndName(_ttnwInventorySource, daypartCodes);

                _InventoryRepository.ExpireInventoryGroupsAndManifests(inventoryGroups, expireDate);

                inventoryGroups = _InventoryRepository.GetInventoryBySourceAndName(_ttnwInventorySource, daypartCodes);

                VerifyInventoryRaw(inventoryGroups);
            }

        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTNWCanLoadFile()
        {
            using (var tran = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_06.09.17.xlsx";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = DateTime.Parse("10/1/2017"),
                    AudiencePricing = new List<AudiencePricingDto>()
                {
                    new AudiencePricingDto() { AudienceId = 13, Price = 210 },
                    new AudiencePricingDto() { AudienceId = 14, Price = 131 }
                },

                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                var daypartCodes = new List<string>() { "LN" };
                VerifyInventory(_ttnwInventorySource, daypartCodes);
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTNWCanUpdateInventory()
        {
            using (var tran = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                // first do initial load
                var filename = @".\Files\TTNW_06.09.17.xlsx";
                var request = new InventoryFileSaveRequest();
                request.StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.InventorySource = "TTNW";
                request.EffectiveDate = DateTime.Parse("10/1/2017");

                request.RatingBook = 416;
                request.AudiencePricing = new List<AudiencePricingDto>()
                {
                    new AudiencePricingDto() { AudienceId = 13, Price = 210 },
                    new AudiencePricingDto() { AudienceId = 14, Price = 131 }
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                filename = @".\Files\TTNW_UPDATE_06.09.17.xlsx";
                request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = DateTime.Parse("10/10/2017"),
                    AudiencePricing = new List<AudiencePricingDto>()
                {
                    new AudiencePricingDto() { AudienceId = 13, Price = 210 },
                    new AudiencePricingDto() { AudienceId = 14, Price = 131 }
                },

                    RatingBook = 416
                };

                result = _InventoryFileService.SaveInventoryFile(request);

                var daypartCodes = new List<string>() { "LN" };
                VerifyInventory(_ttnwInventorySource, daypartCodes);
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
                    @".\Files\CNNAMPMBarterObligations_Clean.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                request.FileName = "CNNAMPMBarterObligations_Clean.xlsx";
                request.InventorySource = "CNN";
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;
                request.EffectiveDate = DateTime.Parse("10/1/2017");
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
        public void GetAllStationsWithTodaysData()
        {
            var currentDate = new DateTime(2016, 11, 1);
            var response = _InventoryFileService.GetStationsWithFilter(
                "OpenMarket",
                "WithTodaysData",
                new DateTime(2017, 03, 06));
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "FlightWeeks");
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "MarketCode");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStationsWithoutTodaysData()
        {
            var currentDate = new DateTime(2016, 11, 1);
            var response = _InventoryFileService.GetStationsWithFilter(
                "OpenMarket",
                "WithoutTodaysData",
                new DateTime(2017, 03, 06));
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "FlightWeeks");
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "MarketCode");
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "RateDataThrough");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStationDetailsByCode()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();
                int fileId = 0;
                int stationCodeWVTM = 5044;

                request.StreamData = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);
                var response = _InventoryFileService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                //Ignore the Id on each Rate record
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgram), "Id");
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                jsonResolver.Ignore(typeof(StationContact), "Id");
                jsonResolver.Ignore(typeof(StationContact), "ModifiedDate");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FindContacts()
        {
            var contactQueryString = "rogelio";

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationContact), "Id");
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
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                //first make sure the contacts don't exist
                var stationContacts = _InventoryFileService.GetStationContacts("OpenMarket", stationCodeWVTM);
                Assert.AreEqual(1, stationContacts.Count);

                request.StreamData = new FileStream(
                    @".\Files\station_contact_new_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);

                //then confirm the contacts are added
                stationContacts = _InventoryFileService.GetStationContacts("OpenMarket", stationCodeWVTM);
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

                var stationContacts = _InventoryFileService.GetStationContacts("OpenMarket", stationCodeWVTM);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationContacts, jsonSettings));
            }
        }

        [Test]
        public void SaveStationContact()
        {
            using (new TransactionScopeWrapper())
            {
                var currentDate = new DateTime(2016, 11, 1);
                var stationCode = _InventoryFileService.GetStations("OpenMarket", currentDate).First().Code;
                var stationContactName = "Unit Test " + DateTime.Now.Ticks;

                var contact = new StationContact()
                {
                    Id = 0,
                    Company = "Unit Test",
                    Email = "Unit@teste.com",
                    Fax = "+134567890",
                    Name = stationContactName,
                    Phone = "+134567890",
                    StationCode = stationCode,
                    Type = StationContact.StationContactType.Station
                };

                var saved = _InventoryFileService.SaveStationContact(contact, "");
                Assert.IsTrue(saved);
                var stationContact =
                    _InventoryFileService.GetStationContacts("OpenMarket", stationCode).Find(q => q.Name == stationContactName);
                _InventoryFileService.DeleteStationContact("OpenMarket", stationContact.Id, "system");
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ThrowsExceptionWhenSavingContactWithEmptyName()
        {
            //get station code
            using (new TransactionScopeWrapper())
            {
                var currentDate = new DateTime(2016, 11, 1);
                var stationCode = _InventoryFileService.GetStations("OpenMarket", currentDate).First().Code;
                var contact = new StationContact()
                {
                    Id = 0,
                    Company = "Unit Teste",
                    Email = "Unit@teste.com",
                    Fax = "+134567890",
                    Name = string.Empty,
                    Phone = "+134567890",
                    StationCode = stationCode,
                    Type = StationContact.StationContactType.Station
                };

                _InventoryFileService.SaveStationContact(contact, "");
            }
        }

        [Test]
        public void UpdateStationContactIfAlreadyExists()
        {
            using (new TransactionScopeWrapper())
            {
                //get station code and fill in initial data
                var currentDate = new DateTime(2016, 11, 1);
                var stationCode = _InventoryFileService.GetStations("OpenMarket", currentDate).First().Code;
                var name = "Unit Test " + DateTime.Now.Ticks;
                var company = "Company Test " + DateTime.Now.Ticks;
                var contact = new StationContact()
                {
                    Id = 0,
                    Company = company,
                    Email = "Unit@teste.com",
                    Fax = "+134567890",
                    Name = name,
                    Phone = "+134567890",
                    StationCode = stationCode,
                    Type = StationContact.StationContactType.Station
                };

                StationContact returnContact = null;

                //save and return station contact
                _InventoryFileService.SaveStationContact(contact, "");
                returnContact = _InventoryFileService.GetStationContacts("OpenMarket", stationCode).Find(q => q.Name == name);

                // modify a property, save
                company = "Modified Company " + DateTime.Now.Ticks;
                returnContact.Company = company;
                _InventoryFileService.SaveStationContact(returnContact, "");

                // return the updated contacts to check if the values are equal
                returnContact = _InventoryFileService.GetStationContacts("OpenMarket", stationCode).Find(q => q.Name == name);

                Assert.AreEqual(returnContact.Company, company);
                _InventoryFileService.DeleteStationContact("OpenMarket", returnContact.Id, "system");
            }
        }

        [Test]
        public void CanDeleteStationContact()
        {
            using (new TransactionScopeWrapper())
            {
                var currentDate = new DateTime(2016, 11, 1);
                var stationCode = _InventoryFileService.GetStations("OpenMarket", currentDate).First().Code;
                var stationContactName = "Unit Test " + DateTime.Now.Ticks;
                var contact = new StationContact()
                {
                    Id = 0,
                    Company = "Unit Teste",
                    Email = "Unit@teste.com",
                    Fax = "+134567890",
                    Name = stationContactName,
                    Phone = "+134567890",
                    StationCode = stationCode,
                    Type = StationContact.StationContactType.Station
                };

                _InventoryFileService.SaveStationContact(contact, "");

                // remove 
                var stationContact =
                    _InventoryFileService.GetStationContacts("OpenMarket", stationCode).Find(q => q.Name == stationContactName);
                var deleted = _InventoryFileService.DeleteStationContact("OpenMarket", stationContact.Id, "system");

                Assert.IsTrue(deleted);
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


        [Ignore]
        [Test]
        [ExpectedException(typeof(System.Exception), ExpectedMessage = "Unable to find media week containing date 1/24/1988", MatchType = MessageMatch.Contains)]
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
                var stationDetails = _InventoryFileService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);
                //var program = stationDetails.Rates.Single(q => q.Program == "TR_WVTM-TV_TEST_1 11:30AM");
                //_ratesService.TrimProgramFlight(program.Id, endDate, endDate, "IntegrationTestUser");
            }
        }


        [Ignore]
        [Test]
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
                //var stationRates = _ratesService.GetStationRates("OpenMarket", stationCodeWVTM, startDate, endDate);
                //var rate = stationRates.Where(p => p.Program == "CADENT NEWS AFTER MIDNIGHT").Single();
                //Assert.AreEqual("M-F 2AM-4AM", rate.Airtime);
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
                //var result = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM).Rates.Where(p => p.Program == "Simple Period News").ToList();

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
        public void CanLoadOpenMarketInventoryFileWithAvailLineWithPeriods()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\1Chicago WLS Syn 4Q16.xml");
                var jsonSettings = _GetJsonSerializerSettingsForConvertingAllStationProgramsToJson();
                int stationCode = 5060; // for station WLS

                _InventoryFileService.SaveInventoryFile(request);

                var results = _InventoryFileService.GetAllStationPrograms("OpenMarket", stationCode);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFile_WithDayDetailedPeriodAttribute()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\1Q18 EMN Rates\Albany - WNYT - EM-NN - 1q18.xml");
                var jsonSettings = _GetJsonSerializerSettingsForConvertingAllStationProgramsToJson();
                int stationCode = 5491; // for station WNYT

                _InventoryFileService.SaveInventoryFile(request);

                var results = _InventoryFileService.GetAllStationPrograms("OpenMarket", stationCode);
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

                var results = _InventoryFileService.GetAllStationPrograms("OpenMarket", stationCode);
                var resultsHaveProgramWithNameLengthLongerThan63Symbols = results.Any(x => x.ProgramNames.Any(p => p.Length > 63));

                Assert.True(resultsHaveProgramWithNameLengthLongerThan63Symbols);
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFile_WithStationsDuplicated()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\1Q18 EMN Rates\Cinncinnati WSTR EM.xml");
                var jsonSettings = _GetJsonSerializerSettingsForConvertingAllStationProgramsToJson();
                var stationCode = 6826; // for station WSTR

                _InventoryFileService.SaveInventoryFile(request);

                var results = _InventoryFileService.GetAllStationPrograms("OpenMarket", stationCode);
                var json = IntegrationTestHelper.ConvertToJson(results, jsonSettings);
                Approvals.Verify(json);
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFile_WithDetailedPeriodAttribute()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\ImportingRateData\1Q18 EMN Rates\ALBANY WTEN EM-NN.xml");
                var jsonSettings = _GetJsonSerializerSettingsForConvertingAllStationProgramsToJson();
                int stationCode = 428; // for station WTEN

                _InventoryFileService.SaveInventoryFile(request);

                var results = _InventoryFileService.GetAllStationPrograms("OpenMarket", stationCode);
                var json = IntegrationTestHelper.ConvertToJson(results, jsonSettings);
                Approvals.Verify(json);
            }
        }

        private InventoryFileSaveRequest _GetInventoryFileSaveRequest(string filePath)
        {
            return new InventoryFileSaveRequest
            {
                StreamData = new FileStream(filePath, FileMode.Open, FileAccess.Read),
                UserName = "IntegrationTestUser",
                RatingBook = 416
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

        [Ignore]
        [Test]
        public void UpdateLastModifiedDateAfterAddingGenre()
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

                var stationDetails = _InventoryFileService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                //if (stationDetails.Rates.Count > 0)
                //{
                //    StationProgramAudienceRateDto programDetails = stationDetails.Rates[0];
                //    var programLastDate = _ratesService.GetStations("OpenMarket", programDetails.FlightStartDate).Single(q => q.Code == stationCodeWVTM).ModifiedDate;

                //    programDetails.Impressions = 100;
                //    programDetails.Rate15 = 12;
                //    programDetails.Rate30 = 20;
                //    programDetails.Rating = 0;
                //    programDetails.Genres.Add(
                //        new LookupDto()
                //        {
                //            Id = 0,
                //            Display = "Integration Genre"
                //        });

                //    _ratesService.UpdateProgramRate(programDetails.Id, programDetails, "IntegrationTestUser");
                //    var updatedLastDate = _ratesService.GetStations("OpenMarket", programDetails.FlightStartDate).Single(q => q.Code == stationCodeWVTM).ModifiedDate;

                //    Assert.IsTrue(updatedLastDate > programLastDate);
                //}
            }
        }

        [Test]
        [Ignore]
        public void GetLastModifiedDateOfStation()
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

                _InventoryFileService.SaveInventoryFile(request);

                // it seems datetime.now has some resolution issues when the time is updated. using utcnow seems to have a higher resolution
                var stationLastModifiedDate = _InventoryFileService.GetStations("OpenMarket", new DateTime(2016, 09, 26)).Single(q => q.Code == stationCodeWVTM).ModifiedDate?.ToUniversalTime();

                Assert.IsTrue(stationLastModifiedDate != default(DateTime));
                var dateToCompare = DateTime.UtcNow;
                Assert.IsTrue(stationLastModifiedDate <= dateToCompare);

                Console.WriteLine("Station modified date {0}/ms:{1} - now {2}/ms:{3}", stationLastModifiedDate, stationLastModifiedDate?.Millisecond, dateToCompare, dateToCompare.Millisecond);
            }
        }


        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void GetStations_OpenMarket()
        {
            using (new TransactionScopeWrapper())
            {
                var stations = _InventoryFileService.GetStations("OpenMarket", new DateTime(2016, 09, 26));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stations, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void GetStations_CNN()
        {
            using (new TransactionScopeWrapper())
            {
                var stations = _InventoryFileService.GetStations("CNN", new DateTime(2016, 09, 26));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stations, jsonSettings));
            }
        }

        [Ignore]
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

                var stationDetails = _InventoryFileService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

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

                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }
        }

        [Ignore]
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

                var stationDetails = _InventoryFileService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

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
                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }
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
                    EffectiveDate = new DateTime(2016, 10, 31),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWFileHasInvalidStationProgramCPM()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileLoadTestInvalidCPM.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void CanUpdateTVBFile()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string firstFileName = @".\Files\TVBFileLoadTest.csv";
                const string updateFilename = @".\Files\TVBFileLoadTestUpdate.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(firstFileName, FileMode.Open, FileAccess.Read),
                    FileName = firstFileName,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                _InventoryFileService.SaveInventoryFile(request);

                request.StreamData = new FileStream(updateFilename, FileMode.Open, FileAccess.Read);
                request.FileName = updateFilename;

                _InventoryFileService.SaveInventoryFile(request);

                var stationDetails = _InventoryFileService.GetStationDetailByCode("TVB", 7295);

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }

        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileCanHaveDifferentDemos()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\TVBFileHasDifferentDemos.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                _InventoryFileService.SaveInventoryFile(request);

                var stationDetails = _InventoryFileService.GetStationDetailByCode("TVB", 5139);

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }

        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileCanHaveDifferentDemos()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\CNNFileHasDifferentDemos.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                _InventoryFileService.SaveInventoryFile(request);

                var stationDetails = _InventoryFileService.GetStationDetailByCode("CNN", 5139);

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }

        }

        [Test]
        [Ignore]
        public void CanLoadCNNFile()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\CNNFileLoadTest.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void LoadTTNWExcelFile()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\TTWN_06.09.17.xlsx";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWFileHasDuplicateStationProgram()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileLoadTestDuplicateStation.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWFileHasStationProgramWithInvalidSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWBFileLoadTestInvalidSpothLenght.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWFileHasInvalidStationName()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileLoadTestInvalidStation.csv";
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWFileHasAllStationsUnknown()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileHasInvalidStations.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TTNWFileHasInvalidDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileHasInvalidDayPart.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWFileHasAllDaypartInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileHasAllDaypartsInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }


        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "There are no valid spot length in the file", MatchType = MessageMatch.Contains)]
        public void TTNWFileHasAllSpotLengthInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileHasAllSpotLengthInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "Unable to parse any file records", MatchType = MessageMatch.Contains)]
        public void TTNWFileHasAllEntriesInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileHasAllEntriesInvalid.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TTNWFileHasInvalidAudience()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileHasInvalidAudience.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWFileHasInvalidAudienceAndStation()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\TTNWFileHasInvalidAudienceAndStation.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void CanLoadValidFlightForCNNFile()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\CNNFileHasValidFlight.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 10, 31),
                    RatingBook = 416
                };

                _InventoryFileService.SaveInventoryFile(request);
                var stationRates = _InventoryFileService.GetStationDetailByCode("CNN", 1039);

                var jsonResolver = new IgnorableSerializerContractResolver();
                // jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationRates, jsonSettings));
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWFileHasInvalidDaypartCode()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\TTNWFileHasInvalidDaypartCode.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWCanLoadFileWithDaypartCodeWithSpaces()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithDaypartCodeWithSpace.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                const string filename = @".\Files\TTNWFileLoadTest.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    RatingBook = 413,
                    EffectiveDate = new DateTime(2016, 11, 06)
                };

                _InventoryFileService.SaveInventoryFile(request);

                //var rates = _ratesService.GetAllStationRates("TTNW", 1003);

                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(rates));
            }
        }

        [Test]
        [Ignore]
        public void TTNWCanLoadFileWithFixedPriceColumn()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithFixedPriceColumn.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2017, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWThrowExceptionWhenMultipleFixedPriceForSameDaypart()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithMultipleFixedPriceSameDaypart.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2017, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
        public void TTNWCanLoadFileWithSameSpotLengthDaypartAndStation()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithSameStationDaypartSpotLength.csv";

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2016, 11, 06),
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
                    EffectiveDate = new DateTime(2017, 04, 02),
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
        [UseReporter(typeof(DiffReporter))]
        public void SaveNewProgramTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const int stationCode = 5276;

                _InventoryFileService.SaveProgram(new StationProgram
                {
                    FlightWeeks = new List<FlightWeekDto>
                    {
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2017, 12, 01),
                            EndDate = new DateTime(2018, 01, 01)
                        }
                    },
                    HouseHoldImpressions = 1000,
                    Rate15 = 15,
                    Rating = 50,
                    ProgramNames = new List<string>() { "Testing Program" },
                    StationCode = stationCode,
                    RateSource = "OpenMarket",
                    Airtimes = new List<DaypartDto>() { DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(1)) }
                }, "TestUser");

                var programsForStation = _InventoryFileService.GetAllStationPrograms("OpenMarket", stationCode);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgram), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(programsForStation, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveExistingProgramTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const int stationCode = 7353;

                _InventoryFileService.SaveProgram(new StationProgram
                {
                    Id = 24538,
                    EffectiveDate = new DateTime(2017, 12, 01),
                    EndDate = new DateTime(2018, 01, 01),
                    FlightWeeks = new List<FlightWeekDto>
                    {
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2017, 12, 01),
                            EndDate = new DateTime(2018, 01, 01)
                        }
                    },
                    HouseHoldImpressions = 1000,
                    Rate30 = 20,
                    Rating = 50,
                    ProgramNames = new List<string>() { "Edited Program Name 54" },
                    StationCode = stationCode,
                    RateSource = "OpenMarket",
                    Airtimes = new List<DaypartDto>() { DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(1)) }
                }, "TestUser");

                var programsForStation = _InventoryFileService.GetAllStationPrograms("OpenMarket", stationCode);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgram), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(programsForStation, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveNewProgramAndUpdateConflictsTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const int stationCode = 7353;

                _InventoryFileService.SaveProgram(new StationProgram
                {
                    EffectiveDate = new DateTime(2017, 12, 01),
                    EndDate = new DateTime(2018, 01, 01),
                    FlightWeeks = new List<FlightWeekDto>
                    {
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2017, 12, 01),
                            EndDate = new DateTime(2018, 01, 01)
                        }
                    },
                    HouseHoldImpressions = 1000,
                    Rate15 = 15,
                    Rate30 = 20,
                    Rating = 50,
                    ProgramNames = new List<string>() { "Edited Program Name 54" },
                    StationCode = stationCode,
                    RateSource = "OpenMarket",
                    Conflicts = new List<StationProgram.StationProgramConflictChangeDto>
                    {
                        new StationProgram.StationProgramConflictChangeDto
                        {
                            Id = 24538,
                            Flights = new List<FlightWeekDto>
                            {
                                new FlightWeekDto
                                {
                                    StartDate = new DateTime(2017, 01, 25),
                                    EndDate = new DateTime(2018, 01, 02)
                                }
                            }
                        }
                    },
                    Airtimes = new List<DaypartDto>() { DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(1)) }
                }, "TestUser");

                var programsForStation = _InventoryFileService.GetAllStationPrograms("OpenMarket", stationCode);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgram), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(programsForStation, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveNewProgramWithHiatusWeeksTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const int stationCode = 5702;

                _InventoryFileService.SaveProgram(new StationProgram
                {
                    EffectiveDate = new DateTime(2017, 12, 01),
                    EndDate = new DateTime(2018, 01, 01),
                    FlightWeeks = new List<FlightWeekDto>
                    {
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2017, 12, 18),
                            EndDate = new DateTime(2018, 12, 24)
                        },
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2017, 12, 25),
                            EndDate = new DateTime(2018, 01, 31),
                            IsHiatus = true,
                        },
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2018, 01, 01),
                            EndDate = new DateTime(2018, 01, 08),
                        }
                    },
                    HouseHoldImpressions = 1000,
                    Rate30 = 30,
                    Rating = 50,
                    ProgramNames = new List<string>() { "Multiple Flight Weeks" },
                    StationCode = stationCode,
                    RateSource = "OpenMarket",
                    Conflicts = new List<StationProgram.StationProgramConflictChangeDto>
                    {
                        new StationProgram.StationProgramConflictChangeDto
                        {
                            Id = 24538,
                            Flights = new List<FlightWeekDto>
                            {
                                new FlightWeekDto
                                {
                                    StartDate = new DateTime(2017, 01, 25),
                                    EndDate = new DateTime(2018, 01, 02)
                                }
                            }

                        }
                    },
                    Airtimes = new List<DaypartDto>() { DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(1)) }
                }, "TestUser");

                var programsForStation = _InventoryFileService.GetAllStationPrograms("OpenMarket", stationCode);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgram), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(programsForStation, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void StationConflictsTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var conflicts = _InventoryFileService.GetStationProgramConflicts(new StationProgramConflictRequest
                {
                    Airtime = DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(1)),
                    RateSource = "OpenMarket",
                    StartDate = new DateTime(2017, 12, 01),
                    EndDate = new DateTime(2018, 01, 01),
                    StationCode = 7353,
                });

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(conflicts));
            }
        }

        [Test]
        public void StationConflictsNoConflictSameDatesTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var conflicts = _InventoryFileService.GetStationProgramConflicts(new StationProgramConflictRequest
                {
                    Airtime = DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(28)),
                    RateSource = "OpenMarket",
                    StartDate = new DateTime(2017, 12, 01),
                    EndDate = new DateTime(2018, 01, 01),
                    StationCode = 7353,
                });

                Assert.IsEmpty(conflicts);
            }
        }

        [Test]
        public void StationConflictsNoConflictTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var conflicts = _InventoryFileService.GetStationProgramConflicts(new StationProgramConflictRequest
                {
                    Airtime = DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(28)),
                    RateSource = "OpenMarket",
                    StartDate = new DateTime(2018, 12, 01),
                    EndDate = new DateTime(2019, 01, 01),
                    StationCode = 7353,
                });

                Assert.IsEmpty(conflicts);
            }
        }

        [Test]
        public void StationConflictsUpdateConflictTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var conflicted = _InventoryFileService.GetStationProgramConflicted(new StationProgramConflictRequest
                {
                    Airtime = DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(1)),
                    RateSource = "OpenMarket",
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
        public void DeleteProgramTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const int manifestId = 26684;

                _InventoryFileService.DeleteProgram(manifestId, "OpenMarket", 5319, "test integration");

                var allStationPrograms = _InventoryFileService.GetAllStationPrograms("OpenMarket", 5319);

                var deletedStationProgram = allStationPrograms.FirstOrDefault(p => p.Id == manifestId);

                Assert.IsNull(deletedStationProgram);
            }
        }

        [Test]
        public void ExpireManifestTest()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const int manifestId = 26684;
                var expireDate = new DateTime(2017, 9, 17);

                _InventoryFileService.ExpireManifest(manifestId, expireDate, "OpenMarket", 5319, "test integration");

                var allStationPrograms = _InventoryFileService.GetAllStationPrograms("OpenMarket", 5319);

                var manifest = allStationPrograms.First(p => p.Id == manifestId);

                Assert.AreEqual(expireDate, manifest.EndDate);
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
        public void InventoryService_SavesNewProgram_WithGenres()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const int stationCode = 5276;
                const int genreId = 1;
                const string genreName = "Action";

                var program = new StationProgram
                {
                    FlightWeeks = new List<FlightWeekDto>
                    {
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2017, 12, 01),
                            EndDate = new DateTime(2018, 01, 01)
                        }
                    },
                    HouseHoldImpressions = 1000,
                    Rate15 = 15,
                    Rating = 50,
                    ProgramNames = new List<string>() { "Testing Program" },
                    StationCode = stationCode,
                    RateSource = "OpenMarket",
                    Airtimes = new List<DaypartDto>() { DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(1)) },
                    Genres = new List<LookupDto> { new LookupDto { Id = genreId, Display = genreName } }
                };

                _InventoryFileService.SaveProgram(program, "TestUser");

                var stationDetail = _InventoryFileService.GetStationDetailByCode("OpenMarket", stationCode);
                var stationDetailContainsProgramWithExpectedGenre = stationDetail.Programs.Any(x => x.Genres.Any(g => g.Id == genreId));

                Assert.True(stationDetailContainsProgramWithExpectedGenre);
            }
        }

        [Test]
        public void InventoryService_SavesExistingProgram_WithGenres()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const int stationCode = 7353;
                const int genreId = 1;
                const string genreName = "Action";

                var program = new StationProgram
                {
                    Id = 24538,
                    EffectiveDate = new DateTime(2017, 12, 01),
                    EndDate = new DateTime(2018, 01, 01),
                    FlightWeeks = new List<FlightWeekDto>
                    {
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2017, 12, 01),
                            EndDate = new DateTime(2018, 01, 01)
                        }
                    },
                    HouseHoldImpressions = 1000,
                    Rate30 = 20,
                    Rating = 50,
                    ProgramNames = new List<string>() { "Edited Program Name 54" },
                    StationCode = stationCode,
                    RateSource = "OpenMarket",
                    Airtimes = new List<DaypartDto>() { DaypartDto.ConvertDisplayDaypart(DaypartCache.Instance.GetDisplayDaypart(1)) },
                    Genres = new List<LookupDto> { new LookupDto { Id = genreId, Display = genreName } }
                };

                _InventoryFileService.SaveProgram(program, "TestUser");

                var stationDetail = _InventoryFileService.GetStationDetailByCode("OpenMarket", stationCode);
                var stationDetailContainsProgramWithExpectedGenre = stationDetail.Programs.Any(x => x.Genres.Any(g => g.Id == genreId));

                Assert.True(stationDetailContainsProgramWithExpectedGenre);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketInventoryFileWithUnknownStation()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _GetInventoryFileSaveRequest(@".\Files\1Chicago WLS Syn 4Q16 UNKNOWN.xml");
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifestStaging), "ManifestId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                var manifests = _InventoryRepository.GetManifestsStagingByFileId(result.FileId);

                // sort manifests in order to get the same results for several test runs
                manifests = manifests
                    .OrderBy(x => x.EffectiveDate)
                    .ThenBy(x => x.EndDate)
                    .ThenBy(x => x.ManifestAudiencesReferences.FirstOrDefault()?.Impressions ?? 0)
                    .ThenBy(x => x.ManifestDayparts.FirstOrDefault()?.ProgramName ?? string.Empty)
                    .ToList();

                var manifestsJson = IntegrationTestHelper.ConvertToJson(manifests, jsonSettings);

                Approvals.Verify(manifestsJson);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadBarterInventoryFile()
        {
            const string fileName = "BarterFile.barter_extension";

            using (new TransactionScopeWrapper())
            {
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFile), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName,
                    RatingBook = 437
                };

                var result = _InventoryFileService.SaveBarterInventoryFile(request, "IntegrationTestUser");

                var file = _InventoryFileRepository.GetInventoryFileById(result.FileId);
                var fileJson = IntegrationTestHelper.ConvertToJson(file, jsonSettings);

                Approvals.Verify(fileJson);
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

                var stationContacts = _InventoryFileService.GetStationContacts("OpenMarket", 5362);

                Assert.AreEqual(1, stationContacts.Count);

                Assert.IsEmpty(problems);
            }
        }
    }
}
