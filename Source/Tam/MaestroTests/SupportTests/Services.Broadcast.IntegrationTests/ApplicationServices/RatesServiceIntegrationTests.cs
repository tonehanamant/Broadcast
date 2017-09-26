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
    public class RatesServiceIntegrationTests
    {
        private IRatesService _ratesService = IntegrationTestApplicationServiceFactory.GetApplicationService<IRatesService>();
        private IStationProgramRepository _stationProgramRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();

        [Test]
        public void LoadRatesFile()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                
                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [Ignore] //Locking does not work in an integration test.
        public void CantLoadRatesFileWhileStationLocked()
        {
            using (new TransactionScopeWrapper())
            {
                var stationCode = 5044; //WVTM
                var request = new RatesSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";

                _ratesService.LockStation(stationCode);
                try
                {
                    _ratesService.SaveRatesFile(request);
                    Assert.Fail("Should have thrown a lock exception.");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Some error", e.Message);
                }
                finally
                {
                    _ratesService.UnlockStation(stationCode);
                }
                
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAssemblyRateFile()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\assembly_rate_file.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "Assembly";
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAssemblyRateFileWithMissingData()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\assembly_rate_file_missing_data.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "Assembly";
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

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
        public void ValidateSavedAssemblyRatesFile()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\assembly_rate_file.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "Assembly";
                var response = _ratesService.SaveRatesFile(request);
                var result = _ratesService.GetStations("Assembly", new DateTime(2017, 1, 1));

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAssemblyRateFileWithUnknownAudience()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\assembly_rate_file_unknown_audience.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "Assembly";
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAssemblyRateFileWithInvalidRate()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\AssemblyFileHasInvalidRateValue.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "Assembly";
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAssemblyRateFileWithBlankRateValues()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\AssemblyFileHasBlankRateValue.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "Assembly";
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAssemblyRateFileWithZeroRateValues()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\AssemblyFileHasZeroRateValue.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "Assembly";
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastDuplicateRatesFileException))]
        public void ThrowsExceptionWhenLoadingSameRatesFileAgain()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                var request2 = new RatesSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                request2.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request2.UserName = "IntegrationTestUser";
                _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStations()
        {
            var currentDate = new DateTime(2016, 11, 1);
            var response = _ratesService.GetStations("OpenMarket", currentDate);
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
        public void GetAllStationsWithTodaysData()
        {
            var currentDate = new DateTime(2016, 11, 1);
            var response = _ratesService.GetStationsWithFilter(
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
            var response = _ratesService.GetStationsWithFilter(
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
                var request = new RatesSaveRequest();
                int fileId = 0;
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);
                var response = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                //Ignore the Id on each Rate record
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                jsonResolver.Ignore(typeof(StationContact), "Id");
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
        public void CreateProgramRate()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RateSource = "OpenMarket";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                if (stationDetails.Rates.Count > 0)
                {
                    NewStationProgramDto newProgram = new NewStationProgramDto()
                    {
                        StationCode = stationCodeWVTM,
                        RateSource = RatesFile.RateSourceType.OpenMarket,
                        Airtime = new DaypartDto()
                        {
                            startTime = 0,
                            endTime = 86400,
                            mon = true,
                            tue = true,
                            wed = true,
                            thu = true,
                            fri = true,
                            sat = true,
                            sun = true
                        },
                        Program = "Integration Tests Program",
                        Rate15 = 123,
                        Rate30 = 251.0m,
                        Impressions = 11.5f,
                        Rating = .54f,
                        Genres = new List<LookupDto>()
                        {
                            new LookupDto()
                            {
                                Id = 0,
                                Display = "Integration Test Genre"
                            }
                        },
                        FlightStartDate = new DateTime(2016, 10, 31),
                        FlightEndDate = new DateTime(2016, 12, 25),
                        Flights = new List<FlightWeekDto>()
                        {
                            new FlightWeekDto() { StartDate = new DateTime(2016, 10, 31), EndDate = new DateTime(2016, 11, 6), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 11, 7), EndDate = new DateTime(2016, 11, 13), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 11, 14), EndDate = new DateTime(2016, 11, 20), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 11, 21), EndDate = new DateTime(2016, 11, 27), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 11, 28), EndDate = new DateTime(2016, 12, 4), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 12, 5), EndDate = new DateTime(2016, 12, 11), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 12, 12), EndDate = new DateTime(2016, 12, 18), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 12, 19), EndDate = new DateTime(2016, 12, 25), IsHiatus = false }
                        }
                    };

                    var response = _ratesService.CreateStationProgramRate(newProgram, "IntegrationTestUser");

                    var jsonResolver = new IgnorableSerializerContractResolver();
                    jsonResolver.Ignore(typeof(LookupDto), "Id");
                    jsonResolver.Ignore(typeof(NewStationProgramDto), "Id");
                    jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                    jsonResolver.Ignore(typeof(StationContact), "Id");
                    jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                    jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                    var jsonSettings = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = jsonResolver
                    };

                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
                }
            }
        }

        [Test]
        public void NewProgramUpdateConflicts()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RateSource = "OpenMarket";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                if (stationDetails.Rates.Count > 0)
                {
                    var program = stationDetails.Rates[0];
                    var programEndDate = new DateTime(2016, 10, 2);

                    NewStationProgramDto newProgram = new NewStationProgramDto()
                    {
                        StationCode = stationCodeWVTM,
                        RateSource = RatesFile.RateSourceType.OpenMarket,
                        Airtime = new DaypartDto()
                        {
                            startTime = 0,
                            endTime = 86400,
                            mon = true,
                            tue = true,
                            wed = true,
                            thu = true,
                            fri = true,
                            sat = true,
                            sun = true
                        },
                        Program = "Integration Tests Program",
                        Rate15 = 123,
                        Rate30 = 251.0m,
                        Impressions = 11.5f,
                        Rating = .54f,
                        Genres = new List<LookupDto>()
                        {
                            new LookupDto() { Id = 0, Display = "Integration Test Genre" }
                        },
                        FlightStartDate = new DateTime(2016, 10, 31),
                        FlightEndDate = new DateTime(2016, 12, 25),
                        Flights = new List<FlightWeekDto>()
                        {
                            new FlightWeekDto() { StartDate = new DateTime(2016, 10, 31), EndDate = new DateTime(2016, 11, 6), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 11, 7), EndDate = new DateTime(2016, 11, 13), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 11, 14), EndDate = new DateTime(2016, 11, 20), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 11, 21), EndDate = new DateTime(2016, 11, 27), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 11, 28), EndDate = new DateTime(2016, 12, 4), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 12, 5), EndDate = new DateTime(2016, 12, 11), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 12, 12), EndDate = new DateTime(2016, 12, 18), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 12, 19), EndDate = new DateTime(2016, 12, 25), IsHiatus = false }
                        },
                        Conflicts = new List<StationProgramConflictChangeDto>()
                        {
                            new StationProgramConflictChangeDto()
                            {
                                Id = program.Id, 
                                FlightStartDate = program.FlightStartDate, 
                                FlightEndDate = programEndDate,
                                Flights = new List<FlightWeekDto>()
                                {
                                    new FlightWeekDto() { Id = 666, StartDate = new DateTime(2016, 9, 26), EndDate = programEndDate, IsHiatus = true }
                                }
                            }
                        }
                    };

                    var response = _ratesService.CreateStationProgramRate(newProgram, "IntegrationTestUser");

                    // check if conflict program was updated with new flight data
                    var stationDetailsAfterUpdate = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                    StationProgramAudienceRateDto updatedProgram = stationDetailsAfterUpdate.Rates.Single(p => p.Id == program.Id);

                    Assert.AreEqual(programEndDate, updatedProgram.FlightEndDate);  // updated program end date
                    Assert.AreEqual(newProgram.Conflicts[0].Flights.Count(), updatedProgram.Flights.Count());   // update # of flights
                    Assert.AreNotEqual(newProgram.Conflicts[0].Flights.Count(), program.Flights.Count());   // removed a flight
                    Assert.IsTrue(updatedProgram.Flights[0].IsHiatus);  // set is hiatus flag
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanAddConflictedPrograms()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RateSource = "OpenMarket";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                if (stationDetails.Rates.Count > 0)
                {
                    var program = stationDetails.Rates[0];
                    var programEndDate = new DateTime(2016, 10, 2);

                    NewStationProgramDto newProgram = new NewStationProgramDto()
                    {
                        StationCode = stationCodeWVTM,
                        RateSource = RatesFile.RateSourceType.OpenMarket,
                        Airtime = new DaypartDto()
                        {
                            startTime = 57600,
                            endTime = 61200,
                            mon = true,
                            tue = true,
                            wed = true,
                            thu = true,
                            fri = true,
                            sat = false,
                            sun = false
                        },
                        Program = "Integration Tests Program",
                        Rate15 = 123,
                        Rate30 = 251.0m,
                        Impressions = 11.5f,
                        Rating = .54f,
                        Genres = new List<LookupDto>()
                        {
                            new LookupDto() { Id = 0, Display = "Integration Test Genre" }
                        },
                        FlightStartDate = new DateTime(2016, 9, 26),
                        FlightEndDate = new DateTime(2016, 10, 9),
                        Flights = new List<FlightWeekDto>()
                        {
                            new FlightWeekDto() { StartDate = new DateTime(2016, 9, 26), EndDate = new DateTime(2016, 10, 2), IsHiatus = false },
                            new FlightWeekDto() { StartDate = new DateTime(2016, 10, 3), EndDate = new DateTime(2016, 10, 9), IsHiatus = false }
                        }
                    };

                    var response = _ratesService.CreateStationProgramRate(newProgram, "IntegrationTestUser");

                    var jsonResolver = new IgnorableSerializerContractResolver();
                    jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                    jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                    jsonResolver.Ignore(typeof(LookupDto), "Id");
                    jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                    var jsonSettings = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = jsonResolver
                    };
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));

                }
            }
        }

        [Test]
        public void DeleteProgramRates()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);
                _ratesService.DeleteProgramRates(
                    stationDetails.Rates[0].Id,
                    stationDetails.Rates[0].FlightStartDate,
                    stationDetails.Rates[0].FlightEndDate,
                    "system");
                var stationDetailsAfterDelete = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);
                Assert.IsTrue(stationDetailsAfterDelete.Rates.Count == 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateProgramRates()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;
                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                if (stationDetails.Rates.Count > 0)
                {
                    StationProgramAudienceRateDto programDetails = stationDetails.Rates[0];

                    programDetails.Impressions = 5000;
                    programDetails.Rate15 = 123.14m;
                    programDetails.Rate30 = 200.12m;
                    programDetails.Rating = .18f;
                    programDetails.Genres.Add(
                        new LookupDto()
                        {
                            Id = 0,
                            Display = "Integration Test Genre"
                        });

                    _ratesService.UpdateProgramRate(programDetails.Id, programDetails, "IntegrationTestUser");

                    var stationDetailsAfterUpdate = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                    StationProgramAudienceRateDto response = stationDetailsAfterUpdate.Rates[0];

                    var jsonResolver = new IgnorableSerializerContractResolver();
                    jsonResolver.Ignore(typeof(LookupDto), "Id");
                    jsonResolver.Ignore(typeof(NewStationProgramDto), "Id");
                    jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                    jsonResolver.Ignore(typeof(StationContact), "Id");
                    jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                    jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                    var jsonSettings = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = jsonResolver
                    };

                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanAddProgramGenreWithBlankSpaces()
        {
            using (new TransactionScopeWrapper())
            {
                // save the station programs with rates
                int stationCodeWVTM = 1171;

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                //setup json fields
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                jsonResolver.Ignore(typeof(StationContact), "Id");
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Flights");
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                //get first station rate
                StationProgramAudienceRateDto programDetails = stationDetails.Rates[0];

                programDetails.Genres.Add(
                    new LookupDto()
                    {
                        Id = 0,
                        Display = "Integration Test"
                    });

                programDetails.Genres.Add(
                    new LookupDto()
                    {
                        Id = 0,
                        Display = "    INTEGRATION Test     "
                    });

                // update the genre of the program
                _ratesService.UpdateProgramRate(programDetails.Id, programDetails, "IntegrationTestUser");
                var stationDetailsAfterUpdate = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);
                // check if there is only one genre
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetailsAfterUpdate, jsonSettings));
            }
        }

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

            var result = _ratesService.FindStationContactsByName(contactQueryString);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ImportNewStationContact()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationContact), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                //first make sure the contacts don't exist
                var stationContacts = _ratesService.GetStationContacts("OpenMarket", stationCodeWVTM);
                Assert.AreEqual(0, stationContacts.Count); //make sure there are no contacts initially

                request.RatesStream = new FileStream(
                    @".\Files\station_contact_new_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                //then confirm the contacts are added
                stationContacts = _ratesService.GetStationContacts("OpenMarket", stationCodeWVTM);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationContacts, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateExistingStationContactDuringImport()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
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

                _ratesService.SaveRatesFile(request);
                request.RatesStream = new FileStream(
                    @".\Files\station_contact_update_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                _ratesService.SaveRatesFile(request);

                var stationContacts = _ratesService.GetStationContacts("OpenMarket", stationCodeWVTM);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationContacts, jsonSettings));
            }
        }

        [Test]
        public void SaveStationContact()
        {
            using (new TransactionScopeWrapper())
            {
                var currentDate = new DateTime(2016, 11, 1);
                var stationCode = _ratesService.GetStations("OpenMarket", currentDate).First().Code;
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

                var saved = _ratesService.SaveStationContact(contact, "");
                Assert.IsTrue(saved);
                var stationContact =
                    _ratesService.GetStationContacts("OpenMarket", stationCode).Find(q => q.Name == stationContactName);
                _ratesService.DeleteStationContact(stationContact.Id, "system");
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
                var stationCode = _ratesService.GetStations("OpenMarket", currentDate).First().Code;
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

                _ratesService.SaveStationContact(contact, "");
            }
        }

        [Test]
        public void UpdateStationContactIfAlreadyExists()
        {
            using (new TransactionScopeWrapper())
            {
                //get station code and fill in initial data
                var currentDate = new DateTime(2016, 11, 1);
                var stationCode = _ratesService.GetStations("OpenMarket", currentDate).First().Code;
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
                _ratesService.SaveStationContact(contact, "");
                returnContact = _ratesService.GetStationContacts("OpenMarket", stationCode).Find(q => q.Name == name);

                // modify a property, save
                company = "Modified Company " + DateTime.Now.Ticks;
                returnContact.Company = company;
                _ratesService.SaveStationContact(returnContact, "");

                // return the updated contacts to check if the values are equal
                returnContact = _ratesService.GetStationContacts("OpenMarket", stationCode).Find(q => q.Name == name);

                Assert.AreEqual(returnContact.Company, company);
                _ratesService.DeleteStationContact(returnContact.Id, "system");
            }
        }

        [Test]
        public void CanDeleteStationContact()
        {
            using (new TransactionScopeWrapper())
            {
                var currentDate = new DateTime(2016, 11, 1);
                var stationCode = _ratesService.GetStations("OpenMarket", currentDate).First().Code;
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

                _ratesService.SaveStationContact(contact, "");

                // remove 
                var stationContact =
                    _ratesService.GetStationContacts("OpenMarket", stationCode).Find(q => q.Name == stationContactName);
                var deleted = _ratesService.DeleteStationContact(stationContact.Id, "system");

                Assert.IsTrue(deleted);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "nknown station", MatchType = MessageMatch.Contains)]
        public void ThrowsExceptionWhenLoadingAllUnknownStation()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\unknown_station_rate_file_zyxw.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.FileName = "unknown_station_rate_file_zyxw.xml";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadsRateFileWithKnownAndUnknownStations()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\known_and_unknown_station_rate_file_zyxw.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.FileName = "known_and_unknown_station_rate_file_zyxw.xml";
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileSaveResult), "FileId");
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadsRateFileWithUnknownSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\unknown_spot_length_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.FileName = "unknown_spot_length_rate_file_wvtm.xml";
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileSaveResult), "FileId");
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadsProgramsWithoutRates()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\programs_without_rates_or_flights_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.FileName = "programs_without_rates_or_flights_wvtm.xml";
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileSaveResult), "FileId");
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "file may be invalid", MatchType = MessageMatch.Contains)]
        public void ThrowsExceptionWhenLoadingBadXmlFile()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();

                request.RatesStream = new FileStream(
                    @".\Files\rate-file-invalid-schema.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "ThrowsExceptionWhenLoadingBadXmlFile";
                request.FileName = "rate-file-invalid-schema.xml";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramConflicts()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                if (stationDetails.Rates.Count > 0)
                {
                    StationProgramConflictRequest programConflict = new StationProgramConflictRequest()
                    {
                        StationCode = stationCodeWVTM,
                        RateSource = RatesFile.RateSourceType.OpenMarket,
                        StartDate = DateTime.Parse("2016-09-26"),
                        EndDate = DateTime.Parse("2016-10-09"),
                        Airtime = new DaypartDto()
                        {
                            startTime = 0,
                            endTime = 3600,
                            mon = true,
                            tue = true,
                            wed = true,
                            thu = true,
                            fri = true,
                            sat = false,
                            sun = false
                        }
                    };

                    var conflicts = _ratesService.GetStationProgramConflicts(programConflict);
                    Assert.AreEqual(conflicts.Count(), 0);

                    // change airtime to have conflict
                    programConflict.Airtime.startTime = 57600;
                    programConflict.Airtime.endTime = 59400;

                    conflicts = _ratesService.GetStationProgramConflicts(programConflict);

                    var jsonResolver = new IgnorableSerializerContractResolver();
                    jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                    jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                    var jsonSettings = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = jsonResolver
                    };
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(conflicts, jsonSettings));
                }
            }
        }

        [Test]
        public void GetProgramConflicted()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                if (stationDetails.Rates.Count > 0)
                {
                    StationProgramAudienceRateDto programDetails = stationDetails.Rates[0];

                    StationProgramConflictRequest programConflict = new StationProgramConflictRequest()
                    {
                        StartDate = DateTime.Parse("2016-09-26"),
                        EndDate = DateTime.Parse("2016-10-09"),
                        Airtime = new DaypartDto()
                        {
                            startTime = 57600,
                            endTime = 59400,
                            mon = true,
                            tue = true,
                            wed = true,
                            thu = true,
                            fri = true,
                            sat = false,
                            sun = false
                        },
                        ConflictedProgramNewStartDate = DateTime.Parse("2016-09-10"),
                        ConflictedProgramNewEndDate = DateTime.Parse("2016-10-01")
                    };

                    bool isConflicted = _ratesService
                        .GetStationProgramConflicted(programConflict, programDetails.Id);

                    Assert.IsTrue(isConflicted);

                    programConflict.ConflictedProgramNewStartDate = DateTime.Parse("2016-10-10");
                    programConflict.ConflictedProgramNewEndDate = DateTime.Parse("2016-10-25");
                    isConflicted = _ratesService
                        .GetStationProgramConflicted(programConflict, programDetails.Id);

                    Assert.IsFalse(isConflicted);
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanEndProgramFlight()
        {
            using (new TransactionScopeWrapper())
            {
                // load programs for a station
                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(
                        @".\Files\end_program_flight_file_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
                };

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "AudienceId");
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                _ratesService.SaveRatesFile(request);
                var stationCodeWVTM = 1027;

                var endDate = DateFormatter.AdjustEndDate(new DateTime(2016, 11, 26));
                // update the flight to reflect the current date
                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);
                var program = stationDetails.Rates.Single(q => q.Program == "TR_WVTM-TV_TEST_1 11:30AM");

                //try ending the flights
                _ratesService.TrimProgramFlight(program.Id, endDate, endDate, "IntegrationTestUser");
                stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);
                program = stationDetails.Rates.Single(q => q.Program == "TR_WVTM-TV_TEST_1 11:30AM");
                
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(program, jsonSettings));
            }
        }

        [Test]        
        [ExpectedException(typeof(Exception), ExpectedMessage = "Could not find program", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenInvalidProgramFlight()
        {
            using (new TransactionScopeWrapper())
            {
                _ratesService.TrimProgramFlight(0, new DateTime(2016, 11, 27), new DateTime(2016, 11, 27), "IntegrationTestUser");
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot select a date beyond the original flight of the program", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenEndDateIsBiggerThanProgramEndDate()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(
                        @".\Files\end_program_flight_file_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
            };

                _ratesService.SaveRatesFile(request);
                var stationCodeWVTM = 1027;
                var endDate = DateFormatter.AdjustEndDate(new DateTime(2017, 01, 20));
                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);
                var program = stationDetails.Rates.Single(q => q.Program == "TR_WVTM-TV_TEST_1 11:30AM");
                _ratesService.TrimProgramFlight(program.Id, endDate, endDate.AddDays(-7), "IntegrationTestUser");
            }
        }

        [Test]
        [ExpectedException(typeof(System.Exception), ExpectedMessage = "Unable to find media week containing date 1/24/1988", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenEndDateIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(
                        @".\Files\end_program_flight_file_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
            };

                _ratesService.SaveRatesFile(request);
                var stationCodeWVTM = 1027;
                var endDate = DateFormatter.AdjustEndDate(new DateTime(1988, 01, 20));
                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);
                var program = stationDetails.Rates.Single(q => q.Program == "TR_WVTM-TV_TEST_1 11:30AM");
                _ratesService.TrimProgramFlight(program.Id, endDate, endDate, "IntegrationTestUser");
            }
        }

        [Test]
        public void CanLoadGenres()
        {
            using (new TransactionScopeWrapper())
            {
                var genres = _ratesService.GetAllGenres();
                Assert.IsTrue(genres.Any());
            }
        }

        [Test]
        public void CanLoadProgramRateWithAirtimeStartOver24h()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(
                        @".\Files\program_rate_over24h_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
                };

                _ratesService.SaveRatesFile(request);
                var stationCodeWVTM = 5044;
                var startDate = new DateTime(2016, 9, 26);
                var endDate = new DateTime(2016, 10, 09);
                var stationRates = _ratesService.GetStationRates("OpenMarket", stationCodeWVTM, startDate, endDate);
                var rate = stationRates.Where(p => p.Program == "CADENT NEWS AFTER MIDNIGHT").Single();
                Assert.AreEqual("M-F 2AM-4AM", rate.Airtime);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProgramRateFileWithSimplePeriods() //XML structure that is not using DetailedPeriod
        {
            int stationCodeWVTM = 5044;
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(
                        @".\Files\simple_period_rate_file_wvtm.xml",
                        FileMode.Open,
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    FileName = "simple_period_rate_file_wvtm.xml"
            };

                _ratesService.SaveRatesFile(request);
                var result = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM).Rates.Where(p => p.Program == "Simple Period News").ToList();

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadRateFileWithOverlapingFlightWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(
                        @".\Files\station_program_overlapping_flights_wvtm.xml", 
                        FileMode.Open, 
                        FileAccess.Read),
                    UserName = "IntegrationTestUser",
                    RatingBook = 416
            };

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileSaveResult), "FileId");
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                
                var result = _ratesService.SaveRatesFile(request);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

            }
        }

        [Test]
        public void UpdateLastModifiedDateAfterAddingGenre()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                if (stationDetails.Rates.Count > 0)
                {
                    StationProgramAudienceRateDto programDetails = stationDetails.Rates[0];
                    var programLastDate = _ratesService.GetStations("OpenMarket", programDetails.FlightStartDate).Single(q => q.Code == stationCodeWVTM).ModifiedDate;

                    programDetails.Impressions = 100;
                    programDetails.Rate15 = 12;
                    programDetails.Rate30 = 20;
                    programDetails.Rating = 0;
                    programDetails.Genres.Add(
                        new LookupDto()
                        {
                            Id = 0,
                            Display = "Integration Genre"
                        });

                    _ratesService.UpdateProgramRate(programDetails.Id, programDetails, "IntegrationTestUser");
                    var updatedLastDate = _ratesService.GetStations("OpenMarket", programDetails.FlightStartDate).Single(q => q.Code == stationCodeWVTM).ModifiedDate;

                    Assert.IsTrue(updatedLastDate > programLastDate);
                }
            }
        }

        [Test]
        [Ignore]
        public void GetLastModifiedDateOfStation()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                
                _ratesService.SaveRatesFile(request);

                // it seems datetime.now has some resolution issues when the time is updated. using utcnow seems to have a higher resolution
                var stationLastModifiedDate = _ratesService.GetStations("OpenMarket", new DateTime(2016, 09, 26)).Single(q => q.Code == stationCodeWVTM).ModifiedDate.ToUniversalTime();
                
                Assert.IsTrue(stationLastModifiedDate != default(DateTime));
                var dateToCompare = DateTime.UtcNow;
                Assert.IsTrue(stationLastModifiedDate <= dateToCompare);

                Console.WriteLine("Station modified date {0}/ms:{1} - now {2}/ms:{3}", stationLastModifiedDate, stationLastModifiedDate.Millisecond, dateToCompare, dateToCompare.Millisecond);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadRatesFileAndFillOnlyOneSpotLegnth()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_spot_length_15_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadRatesFileAndFillAllSpotLegnthsWhenSpotLengthIs30()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new RatesSaveRequest();
                int stationCodeWVTM = 5044;

                request.RatesStream = new FileStream(
                    @".\Files\single_program_rate_file_wvtm.xml",
                    FileMode.Open,
                    FileAccess.Read);
                request.UserName = "IntegrationTestUser";
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("OpenMarket", stationCodeWVTM);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                // only 15 and 30 are used at the moment.
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        public void CanLoadTVBFile()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileLoadTest.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 11, 07),
                    EndDate = new DateTime(2016, 11, 13),
                    IsHiatus = false
                });
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 11, 14),
                    EndDate = new DateTime(2016, 11, 20),
                    IsHiatus = false
                });
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 11, 21),
                    EndDate = new DateTime(2016, 11, 27),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 10, 31);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasStationProgramWithInvalidSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileLoadTestInvalidSpothLenght.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void TVBFileHasValidAudienceValues()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileHasValidAudienceCPM.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

               // there is no method that brings all stations with audiences. this test will bring only one valid audience
                var wwupStation = _ratesService.GetAllStationRates("TVB", 5667).SelectMany(a => a.Audiences);
                var kusdStation = _ratesService.GetAllStationRates("TVB", 5668).SelectMany(a => a.Audiences);
                var wxgsStation = _ratesService.GetAllStationRates("TVB", 5669).SelectMany(a => a.Audiences);
                var kqcdStation = _ratesService.GetAllStationRates("TVB", 5671).SelectMany(a => a.Audiences);

                // check if the correct audience is in place
                Assert.IsNull(wwupStation.Select(a => a.Audience.AudienceString).FirstOrDefault());
                Assert.IsNotNull(kusdStation.Select(a => a.Audience.AudienceString).FirstOrDefault());
                Assert.IsNull(wxgsStation.Select(a => a.Audience.AudienceString).FirstOrDefault());
                Assert.IsNull(kqcdStation.Select(a => a.Audience.AudienceString).FirstOrDefault());
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileHasDuplicateStationProgram()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileLoadTestDuplicateStation.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void TVBFileHasInvalidStationProgramCPM()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileLoadTestInvalidCPM.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void TTNWFileHasInvalidStationProgramCPM()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWFileLoadTestInvalidCPM.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
                var filename = @".\Files\CNNFileLoadTestInvalidCPM.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

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
        public void TVBFileHasInvalidStationName()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileLoadTestInvalidStation.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void CanUpdateTVBFile()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TVBFileLoadTest.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                filename = @".\Files\TVBFileLoadTestUpdate.csv";
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("TVB", 7295);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }
            
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TVBFileCanHaveDifferentDemos()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TVBFileHasDifferentDemos.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("TVB", 5139);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }

        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileCanHaveDifferentDemos()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\CNNFileHasDifferentDemos.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var stationDetails = _ratesService.GetStationDetailByCode("CNN", 5139);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(FlightWeekDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stationDetails.Rates, jsonSettings));
            }

        }

        [Test]
        [Ignore]
        public void CanLoadCNNFile()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\CNNFileLoadTest.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
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
                var filename = @".\Files\CNNFileLoadTestDuplicateStation.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

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
                var filename = @".\Files\CNNFileLoadTestInvalidSpothLenght.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

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
                var filename = @".\Files\CNNFileLoadTestInvalidStation.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

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
        public void CanLoadTTNWFile()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNWFileLoadTest.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
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
                var filename = @".\Files\TTNWFileLoadTestDuplicateStation.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void TTNWFileHasStationProgramWithInvalidSpotLength()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWBFileLoadTestInvalidSpothLenght.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void TTNWFileHasInvalidStationName()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWFileLoadTestInvalidStation.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "There are no known stations in the file", MatchType = MessageMatch.Contains)]
        public void TTNWFileHasAllStationsUnknown()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWFileHasInvalidStations.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "There are no known stations in the file", MatchType = MessageMatch.Contains)]
        public void TVBFileHasAllStationsUnknown()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileHasInvalidStations.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "There are no known stations in the file", MatchType = MessageMatch.Contains)]
        public void CNNFileHasAllStationsUnknown()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\CNNFileHasInvalidStations.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TTNWFileHasInvalidDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWFileHasInvalidDayPart.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void TVBFileHasInvalidDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileHasInvalidDayPart.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
                var filename = @".\Files\CNNFileHasInvalidDayPart.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Problems, jsonSettings));
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "There are no valid dayparts in the file ", MatchType = MessageMatch.Contains)]
        public void TTNWFileHasAllDaypartInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWFileHasAllDaypartsInvalid.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "There are no valid dayparts in the file ", MatchType = MessageMatch.Contains)]
        public void CNNFileHasAllDaypartInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\CNNFileHasAllDaypartsInvalid.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "There are no valid dayparts in the file ", MatchType = MessageMatch.Contains)]
        public void TVBFileHasAllDaypartInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileHasAllDaypartsInvalid.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }


        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "There are no valid spot length in the file", MatchType = MessageMatch.Contains)]
        public void TTNWFileHasAllSpotLengthInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWFileHasAllSpotLengthInvalid.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "There are no valid spot length in the file", MatchType = MessageMatch.Contains)]
        public void TVBFileHasAllSpotLengthInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileHasAllSpotLengthInvalid.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "There are no valid spot length in the file", MatchType = MessageMatch.Contains)]
        public void CNNFileHasAllSpotLengthInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\CNNFileHasAllSpotLengthInvalid.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "Unable to parse any file records", MatchType = MessageMatch.Contains)]
        public void CNNFileHasAllEntriesInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\CNNFileHasAllEntriesInvalid.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "Unable to parse any file records", MatchType = MessageMatch.Contains)]
        public void TVBFileHasAllEntriesInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileHasAllEntriesInvalid.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "Unable to parse any file records", MatchType = MessageMatch.Contains)]
        public void TTNWFileHasAllEntriesInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWFileHasAllEntriesInvalid.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void TTNWFileHasInvalidAudience()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWFileHasInvalidAudience.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
                var filename = @".\Files\CNNFileHasInvalidAudience.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

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
                var filename = @".\Files\TVBFileHasInvalidAudience.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void TVBFileHasInvalidAudienceAndStation()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TVBFileHasInvalidAudienceAndStation.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void TTNWFileHasInvalidAudienceAndStation()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\TTNWFileHasInvalidAudienceAndStation.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
                var filename = @".\Files\CNNFileHasInvalidAudienceAndStation.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

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
                var filename = @".\Files\CNNFileHasBlankCpm.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

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
        public void CanLoadValidFlightForCNNFile()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\CNNFileHasValidFlight.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 11, 07),
                    EndDate = new DateTime(2016, 11, 13),
                    IsHiatus = false
                });
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 11, 14),
                    EndDate = new DateTime(2016, 11, 20),
                    IsHiatus = false
                });
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 11, 21),
                    EndDate = new DateTime(2016, 11, 27),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 10, 31);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);
                var stationRates = _ratesService.GetStationDetailByCode("CNN", 1039);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");
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

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CNNFileHasInvalidDaypartCode()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\CNNFileHasInvalidDaypartCode.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");

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
                var filename = @".\Files\TTNWFileHasInvalidDaypartCode.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
        public void TVBFileHasInvalidDaypartCode()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TVBFileHasInvalidDaypartCode.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileProblem), "AffectedProposals");
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
                var filename = @".\Files\ThirdPartyFileWithDaypartCodeWithSpace.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                Assert.IsEmpty(result.Problems);
            }

        }

        [Test]
        [Ignore]
        public void CNNCanLoadFileWithDaypartCodeWithSpaces()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\ThirdPartyFileWithDaypartCodeWithSpace.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                Assert.IsEmpty(result.Problems);
            }

        }

        [Test]
        [Ignore]
        public void TVBCanLoadFileWithDaypartCodeWithSpaces()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\ThirdPartyFileWithDaypartCodeWithSpace.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                Assert.IsEmpty(result.Problems);
            }

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadInitialRatesData()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var initialRatesData = _ratesService.GetInitialRatesData();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(initialRatesData));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CanCalculateSpotCost()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
                {
                    const string filename = @".\Files\TTNWFileLoadTest.csv";
                    
                    var request = new RatesSaveRequest
                    {
                        RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                        FileName = filename,
                        UserName = "IntegrationTestUser",
                        RateSource = "TTNW",
                        BlockName = "Integration Test",
                        RatingBook = 413
                    };

                    var flightWeeks = new List<FlightWeekDto>
                    {
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2016, 10, 31),
                            EndDate = new DateTime(2016, 11, 06),
                            IsHiatus = false
                        }
                    };

                    request.FlightEndDate = new DateTime(2016, 11, 06);
                    request.FlightStartDate = new DateTime(2016, 11, 27);
                    request.FlightWeeks = flightWeeks;

                    _ratesService.SaveRatesFile(request);

                    var rates = _ratesService.GetAllStationRates("TTNW", 1003);

                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(rates));
                }
            }
        }

        [Test]
        [Ignore]
        public void TTNWCanLoadFileWithFixedPriceColumn()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithFixedPriceColumn.csv";
                
                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    RateSource = "TTNW",
                    BlockName = "Integration Test"
                };

                var flightWeeks = new List<FlightWeekDto>
                {
                    new FlightWeekDto
                    {
                        StartDate = new DateTime(2017, 10, 31),
                        EndDate = new DateTime(2017, 11, 06),
                        IsHiatus = false
                    }
                };

                request.FlightEndDate = new DateTime(2017, 11, 06);
                request.FlightStartDate = new DateTime(2017, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);

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
                
                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    RateSource = "CNN",
                    BlockName = "Integration Test"
                };

                var flightWeeks = new List<FlightWeekDto>
                {
                    new FlightWeekDto
                    {
                        StartDate = new DateTime(2016, 10, 31),
                        EndDate = new DateTime(2016, 11, 06),
                        IsHiatus = false
                    }
                };

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);

                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [Ignore]
        public void TVBCanLoadFileWithFixedPriceColumn()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithFixedPriceColumn.csv";
                
                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    RateSource = "TVB",
                    BlockName = "Integration Test"
                };

                var flightWeeks = new List<FlightWeekDto>
                {
                    new FlightWeekDto
                    {
                        StartDate = new DateTime(2016, 10, 31),
                        EndDate = new DateTime(2016, 11, 06),
                        IsHiatus = false
                    }
                };

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);
                
                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "Daypart code", MatchType = MessageMatch.Contains)]
        public void TTNWThrowExceptionWhenMultipleFixedPriceForSameDaypart()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithMultipleFixedPriceSameDaypart.csv";

                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    RateSource = "TTNW",
                    BlockName = "Integration Test"
                };

                var flightWeeks = new List<FlightWeekDto>
                {
                    new FlightWeekDto
                    {
                        StartDate = new DateTime(2017, 10, 31),
                        EndDate = new DateTime(2017, 11, 06),
                        IsHiatus = false
                    }
                };

                request.FlightEndDate = new DateTime(2017, 11, 06);
                request.FlightStartDate = new DateTime(2017, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);

                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "Daypart code", MatchType = MessageMatch.Contains)]
        public void CNNThrowExceptionWhenMultipleFixedPriceForSameDaypart()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithMultipleFixedPriceSameDaypart.csv";

                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    RateSource = "CNN",
                    BlockName = "Integration Test"
                };

                var flightWeeks = new List<FlightWeekDto>
                {
                    new FlightWeekDto
                    {
                        StartDate = new DateTime(2016, 10, 31),
                        EndDate = new DateTime(2016, 11, 06),
                        IsHiatus = false
                    }
                };

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);

                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [ExpectedException(typeof(BroadcastRateDataException), ExpectedMessage = "Daypart code", MatchType = MessageMatch.Contains)]
        public void TVBThrowExceptionWhenMultipleFixedPriceForSameDaypart()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\ThirdPartyFileWithMultipleFixedPriceSameDaypart.csv";

                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    RateSource = "TVB",
                    BlockName = "Integration Test"
                };

                var flightWeeks = new List<FlightWeekDto>
                {
                    new FlightWeekDto
                    {
                        StartDate = new DateTime(2016, 10, 31),
                        EndDate = new DateTime(2016, 11, 06),
                        IsHiatus = false
                    }
                };

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);

                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [Ignore]
        public void TVBCanLoadFileWithSameSpotLengthDaypartAndStation()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\ThirdPartyFileWithSameStationDaypartSpotLength.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;
                
                var result = _ratesService.SaveRatesFile(request);

                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [Ignore]
        public void TTNWCanLoadFileWithSameSpotLengthDaypartAndStation()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\ThirdPartyFileWithSameStationDaypartSpotLength.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);

                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [Ignore]
        public void CNNCanLoadFileWithSameSpotLengthDaypartAndStation()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\ThirdPartyFileWithSameStationDaypartSpotLength.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "CNN";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 11, 06);
                request.FlightStartDate = new DateTime(2016, 11, 27);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var result = _ratesService.SaveRatesFile(request);

                Assert.IsEmpty(result.Problems);
            }
        }

        [Test]
        [Ignore]
        public void CanUpdateFixedPriceForThirdParty()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string path = @".\Files\ThirdPartyFileWithFixedPriceColumnUpdate.csv";

                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(path, FileMode.Open, FileAccess.Read),
                    FileName = path,
                    UserName = "IntegrationTestUser",
                    RateSource = "TVB",
                    BlockName = "Integration Test 2"
                };

                var flightWeeks = new List<FlightWeekDto>
                {
                    new FlightWeekDto
                    {
                        StartDate = new DateTime(2017, 04, 02),
                        EndDate = new DateTime(2017, 04, 08),
                        IsHiatus = false
                    }
                };

                request.FlightEndDate = new DateTime(2017, 04, 02);
                request.FlightStartDate = new DateTime(2017, 04, 08);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var stationBeforeUpdate = _stationProgramRepository.GetStationProgramById(320136);

                _ratesService.SaveRatesFile(request);

                var stationAfterUpdate = _stationProgramRepository.GetStationProgramById(320136);

                Assert.AreEqual(7000m, stationBeforeUpdate.fixed_price);
                Assert.AreEqual(7500m, stationAfterUpdate.fixed_price);
            }
        }

        [Test]

        public void CanConvert30sRateTo15sRate()
        {
            var result = _ratesService.ConvertRateForSpotLength(10, 15);
            Assert.AreEqual(6.5, result);
        }
    }
}
