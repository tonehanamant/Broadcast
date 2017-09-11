using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryCrunchServiceIntegrationTests
    {
        private readonly IInventoryCrunchService _inventoryCrunchService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryCrunchService>();
        private IRatesService _ratesService = IntegrationTestApplicationServiceFactory.GetApplicationService<IRatesService>();
        private readonly IDaypartCache _daypartCache = DaypartCache.Instance;

        [Test]
        public void CanCrunchInventoryData()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false, 
                    Id = 649
                });

                var listOfStationPrograms = new List<StationProgram>();
                var stationProgram1 = new StationProgram()
                {
                    Daypart = _daypartCache.GetDisplayDaypart(6722),
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    FlightWeeks = new List<StationProgramFlightWeek>()
                    {
                        new StationProgramFlightWeek()
                        {
                            Active = true,
                            FlightWeek = new DisplayMediaWeek()
                            {
                                Id = 671
                            },
                            Spots = 5
                        }
                    }.ToList(),
                    ProgramName = "TTNW Integration Test", // selected by user
                    StationLegacyCallLetters = "KTNW",
                    SpotLength = 15,
                    DaypartCode = "TTNW AM1",
                    RateSource = RatesFile.RateSourceType.TTNW
                };
                listOfStationPrograms.Add(stationProgram1);
                var stationProgram2 = new StationProgram()
                {
                    Daypart = _daypartCache.GetDisplayDaypart(6722),
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    FlightWeeks = new List<StationProgramFlightWeek>()
                    {
                        new StationProgramFlightWeek()
                        {
                            Active = true,
                            FlightWeek = new DisplayMediaWeek()
                            {
                                Id = 671
                            },
                            Spots = 12
                        }
                    }.ToList(),
                    ProgramName = "TTNW Integration Test", // selected by user
                    StationLegacyCallLetters = "KTNW",
                    SpotLength = 30,
                    DaypartCode = "TTNW AM1",
                    RateSource = RatesFile.RateSourceType.TTNW
                };
                listOfStationPrograms.Add(stationProgram2);

                var res = _inventoryCrunchService.CrunchThirdPartyInventory(listOfStationPrograms, RatesFile.RateSourceType.TTNW,
                    flightWeeks);

                Assert.IsEmpty(res);

            }            
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadInventoryCrunchData()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var inventory = _inventoryCrunchService.GetInventoryDetailByDaypartCodeAndRateSource("TTNW AM",
                    RatesFile.RateSourceType.TTNW);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryDetail), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlot), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlot), "InventoryDetailId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "InventoryDetailSlotId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "StationProgramFlightId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventory, jsonSettings));
            }
        }

        [Test]
        public void CanCrunchFile()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\InventoryCrunch.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TVB";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 05, 30),
                    EndDate = new DateTime(2016, 06, 05),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 05, 30);
                request.FlightStartDate = new DateTime(2016, 06, 05);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                _ratesService.SaveRatesFile(request);

                var inventory = _inventoryCrunchService.GetInventoryDetailByDaypartCodeAndRateSource("TVG",
                    RatesFile.RateSourceType.TVB);

                Assert.IsTrue(inventory.DaypartCode == "TVG");
                Assert.IsTrue(inventory.InventorySource == RatesFile.RateSourceType.TVB);
                Assert.IsNotEmpty(inventory.InventoryDetailSlots);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanReturnAffectedProposalsAfterCrunch()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\UpdateInventoryData.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 05, 30),
                    EndDate = new DateTime(2016, 06, 05),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 05, 30);
                request.FlightStartDate = new DateTime(2016, 06, 05);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                var res = _ratesService.SaveRatesFile(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(RatesFileSaveResult), "FileId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(res, jsonSettings));
            }

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanCrunchDataWithFixedPriceAndCalculateSlotCost()
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

                IntegrationTestApplicationServiceFactory.GetApplicationService<IRatesService>().SaveRatesFile(request);

                var inventory = _inventoryCrunchService.GetInventoryDetailByDaypartCodeAndRateSource("AM",
                    RatesFile.RateSourceType.TVB);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryDetail), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlot), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlot), "InventoryDetailId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "InventoryDetailSlotId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "StationProgramFlightId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventory, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanCrunchDataWithoutFixedPriceAndCalculateSlotClost()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string filename = @".\Files\InventoryCrunchNoFixedPrice.csv";

                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read),
                    FileName = filename,
                    UserName = "IntegrationTestUser",
                    RateSource = "TVB",
                    BlockName = "Integration Test",
                    PlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3,
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

                var inventory = _inventoryCrunchService.GetInventoryDetailByDaypartCodeAndRateSource("EE",
                    RatesFile.RateSourceType.TVB);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryDetail), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlot), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlot), "InventoryDetailId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "InventoryDetailSlotId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "StationProgramFlightId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventory, jsonSettings));
            }
        }

        [Test]
        public void CanUpdateInventoryCrunchedData()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\UpdateCrunchedInventoryData.csv";
                var request = new RatesSaveRequest();
                request.RatesStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                request.FileName = filename;
                request.UserName = "IntegrationTestUser";
                request.RateSource = "TTNW";
                request.BlockName = "Integration Test";

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 05, 30),
                    EndDate = new DateTime(2016, 06, 05),
                    IsHiatus = false
                });

                request.FlightEndDate = new DateTime(2016, 05, 30);
                request.FlightStartDate = new DateTime(2016, 06, 05);
                request.FlightWeeks = flightWeeks;
                request.RatingBook = 416;

                // load saved crunched data
                var inventory = _inventoryCrunchService.GetInventoryDetailByDaypartCodeAndRateSource("TTNW AM",
                    RatesFile.RateSourceType.TTNW);

                _ratesService.SaveRatesFile(request);

                var updatedInventory = _inventoryCrunchService.GetInventoryDetailByDaypartCodeAndRateSource("TTNW AM",
                    RatesFile.RateSourceType.TTNW);

                Assert.AreNotEqual(inventory.InventoryDetailSlots.Count, updatedInventory.InventoryDetailSlots.Count );
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanCrunchInventoryForDifferentDaypartCodes()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const string path = @".\Files\ThirdPartyFileMultipleDaypartsWithFixedPriceColumn.csv";

                var request = new RatesSaveRequest
                {
                    RatesStream = new FileStream(path, FileMode.Open, FileAccess.Read),
                    FileName = path,
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

                IntegrationTestApplicationServiceFactory.GetApplicationService<IRatesService>().SaveRatesFile(request);

                var inventory = _inventoryCrunchService.GetInventoryDetailByDaypartCodeAndRateSource("AM",
                    RatesFile.RateSourceType.TVB);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventoryDetail), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlot), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlot), "InventoryDetailId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "Id");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "InventoryDetailSlotId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "StationProgramFlightId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventory, jsonSettings));
            }
        }
    }
}
