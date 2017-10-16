
using ApprovalTests;
using ApprovalTests.Reporters;
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

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryFileServiceIntegrationTests
    {
        private IInventoryFileService _InventoryFileService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryFileService>();
        private IStationInventoryGroupService _StationInventoryGroupService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationInventoryGroupService>();

        [Test]
        public void LoadInventoryFileCNN()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var request = new InventoryFileSaveRequest
                {
                    RatesStream = new FileStream(
                        @".\Files\CNNAMPMBarterObligations_Clean.xlsx",
                        FileMode.Open,
                        FileAccess.Read),
                    FileName = "CNNAMPMBarterObligations_Clean.xlsx",
                    InventorySource = "CNN",
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsNotNull(result.FileId);
            }
        }


        [Test]
        public void LoadInventoryStationGroupsAfterSavingCNNFile()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var request = new InventoryFileSaveRequest
                {
                    RatesStream = new FileStream(
                    @".\Files\CNNAMPMBarterObligations_Clean.xlsx",
                    FileMode.Open,
                        FileAccess.Read),
                    FileName = "CNNAMPMBarterObligations_Clean.xlsx",
                    InventorySource = "CNN",
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
                };

                var savedInventoryFile = _InventoryFileService.SaveInventoryFile(request);
                var stationGroups =
                    _StationInventoryGroupService.GetStationInventoryGroupsByFileId(savedInventoryFile.FileId);

                Assert.IsNotEmpty(stationGroups);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadTTNWFile()
        {
            using (var tran = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_06.09.17.xlsx";
                var request = new InventoryFileSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.InventorySource = "TTNW";

                request.RatingBook = 416;

                var result = _InventoryFileService.SaveInventoryFile(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
                Approvals.Verify(json);

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

                request.RatesStream = new FileStream(
                    @".\Files\CNNAMPMBarterObligations_Clean.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                request.FileName = "CNNAMPMBarterObligations_Clean.xlsx";
                request.InventorySource = "CNN";
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);

                request2.RatesStream = new FileStream(
                    @".\Files\CNNAMPMBarterObligations_Clean.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                request2.FileName = "CNNAMPMBarterObligations_Clean.xlsx";
                request2.InventorySource = "CNN";
                request2.UserName = "IntegrationTestUser";

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

                request.RatesStream = new FileStream(
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

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStations()
        {
            var currentDate = new DateTime(2016, 11, 1);
            var response = _InventoryFileService.GetStations("OpenMarket", currentDate);
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

        [Ignore]
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

        [Ignore]
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

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStationDetailsByCode()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();
                int fileId = 0;
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);
                var response = _InventoryFileService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                //Ignore the Id on each Rate record
                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                jsonResolver.Ignore(typeof(StationContact), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }


        [Ignore]
        [Test]
        [UseReporter(typeof (DiffReporter))]
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

        [Ignore]
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
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                //first make sure the contacts don't exist
                var stationContacts = _InventoryFileService.GetStationContacts("OpenMarket", stationCodeWVTM);
                Assert.AreEqual(0, stationContacts.Count); //make sure there are no contacts initially

                request.RatesStream = new FileStream(
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

        [Ignore]
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
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                request.RatesStream = new FileStream(
                    @".\Files\station_contact_new_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);
                request.RatesStream = new FileStream(
                    @".\Files\station_contact_update_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                _InventoryFileService.SaveInventoryFile(request);

                var stationContacts = _InventoryFileService.GetStationContacts("OpenMarket", stationCodeWVTM);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationContacts, jsonSettings));
            }
        }

        [Ignore]
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
                _InventoryFileService.DeleteStationContact(stationContact.Id, "system");
            }
        }

        [Ignore]
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

        [Ignore]
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
                _InventoryFileService.DeleteStationContact(returnContact.Id, "system");
            }
        }

        [Ignore]
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
                var deleted = _InventoryFileService.DeleteStationContact(stationContact.Id, "system");

                Assert.IsTrue(deleted);
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "nknown station", MatchType = MessageMatch.Contains)]
        public void ThrowsExceptionWhenLoadingAllUnknownStation()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\unknown_station_rate_file_zyxw.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.FileName = "unknown_station_rate_file_zyxw.xml";
                request.RatingBook = 416;

                _InventoryFileService.SaveInventoryFile(request);

            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadsInventoryFileWithKnownAndUnknownStations()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\known_and_unknown_station_rate_file_zyxw.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.FileName = "known_and_unknown_station_rate_file_zyxw.xml";
                request.RatingBook = 416;

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadsInventoryFileWithUnknownSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\unknown_spot_length_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.FileName = "unknown_spot_length_rate_file_wvtm.xml";
                request.RatingBook = 416;

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileSaveResult), "FileId");
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(BroadcastInventoryDataException), ExpectedMessage = "file may be invalid", MatchType = MessageMatch.Contains)]
        public void ThrowsExceptionWhenLoadingBadXmlFile()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();

                request.RatesStream = new FileStream(
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
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot select a date beyond the original flight of the program", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenEndDateIsBiggerThanProgramEndDate()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    RatesStream = new FileStream(
                        @".\Files\end_program_flight_file_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
            };

                _InventoryFileService.SaveInventoryFile(request);
                var stationCodeWVTM = 1027;
                var endDate = DateFormatter.AdjustEndDate(new DateTime(2017, 01, 20));
                var stationDetails = _InventoryFileService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);
                //var program = stationDetails.Rates.Single(q => q.Program == "TR_WVTM-TV_TEST_1 11:30AM");
                //_ratesService.TrimProgramFlight(program.Id, endDate, endDate.AddDays(-7), "IntegrationTestUser");
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
                    RatesStream = new FileStream(
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
                    RatesStream = new FileStream(
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

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProgramInventoryFileWithSimplePeriods() //XML structure that is not using DetailedPeriod
        {
            int stationCodeWVTM = 5044;
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    RatesStream = new FileStream(
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


        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadInventoryFileWithOverlapingFlightWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    RatesStream = new FileStream(
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

        [Ignore]
        [Test]
        public void UpdateLastModifiedDateAfterAddingGenre()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
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

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                
                _InventoryFileService.SaveInventoryFile(request);

                // it seems datetime.now has some resolution issues when the time is updated. using utcnow seems to have a higher resolution
                var stationLastModifiedDate = _InventoryFileService.GetStations("OpenMarket", new DateTime(2016, 09, 26)).Single(q => q.Code == stationCodeWVTM).ModifiedDate.ToUniversalTime();
                
                Assert.IsTrue(stationLastModifiedDate != default(DateTime));
                var dateToCompare = DateTime.UtcNow;
                Assert.IsTrue(stationLastModifiedDate <= dateToCompare);

                Console.WriteLine("Station modified date {0}/ms:{1} - now {2}/ms:{3}", stationLastModifiedDate, stationLastModifiedDate.Millisecond, dateToCompare, dateToCompare.Millisecond);
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

                request.RatesStream = new FileStream(
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

                request.RatesStream = new FileStream(
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 10, 31),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
               
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(firstFileName, FileMode.Open, FileAccess.Read),
                    FileName = firstFileName,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                _InventoryFileService.SaveInventoryFile(request);

                request.RatesStream = new FileStream(updateFilename, FileMode.Open, FileAccess.Read);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [Ignore]
        public void CanLoadTTNWFileCsv()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\TTNWFileLoadTest.csv";

                var request = new InventoryFileSaveRequest
                {
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                jsonResolver.Ignore(typeof (FlightWeekDto), "Id");
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                
                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                Assert.IsEmpty(result.Problems);
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
                        RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2017, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);
                
                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2017, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TVB",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };
                
                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "TTNW",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    InventorySource = "CNN",
                    EffectiveDate = new DateTime(2016, 11, 06),
                    RatingBook = 416
                };

                var result = _InventoryFileService.SaveInventoryFile(request);

                Assert.IsEmpty(result.Problems);
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
                    RatesStream = new FileStream(path, FileMode.Open, FileAccess.Read),
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
    }
}
