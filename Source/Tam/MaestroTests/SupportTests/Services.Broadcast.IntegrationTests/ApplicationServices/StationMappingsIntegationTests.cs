using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class StationMappingsIntegationTests
    {
        private readonly IStationMappingService _StationMappingService;
        private readonly IStationService _StationService;
        private readonly IStationRepository _StationRepository;

        public StationMappingsIntegationTests()
        {
            _StationMappingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationMappingService>();
            _StationService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationService>();
            _StationRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IStationRepository>();
        }

        [Test]
        public void CanSaveAndRetrieveStationMappings()
        {
            using (new TransactionScopeWrapper())
            {
                var stationMappings = (new List<StationMappingsFileRequestDto>
                {
                    new StationMappingsFileRequestDto
                    {
                        CadentCallLetters = "DMAH",
                        ExtendedCallLetters = "WMAH-DT2",
                        NSICallLetters = "WMAH-TV 19.2",
                        NSILegacyCallLetters = "DMAH",
                        SigmaCallLetters = "DMAH"
                    },
                    new StationMappingsFileRequestDto
                    {
                        CadentCallLetters = "DMAH",
                        ExtendedCallLetters = "WMAH-DT2",
                        NSICallLetters = "WMAH-TV+",
                        NSILegacyCallLetters = "DMAH",
                        SigmaCallLetters = "DMAH"
                    }
                }).GroupBy( x => x.CadentCallLetters).First();

                _StationMappingService.SaveStationMappings(stationMappings, "integration_test", DateTime.Now);
                var result = _StationMappingService.GetStationMappingsByCadentCallLetter("DMAH");

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        public void CanSaveAndRetrieveStationMappingsWithOnlyExtendedCallLetters()
        {
            using (new TransactionScopeWrapper())
            {
                var stationMappings = (new List<StationMappingsFileRequestDto>
                {
                    new StationMappingsFileRequestDto
                    {
                        CadentCallLetters = "DMAH",
                        ExtendedCallLetters = "WMAH-DT2",
                    }
                }).GroupBy(x => x.CadentCallLetters).First();

                _StationMappingService.SaveStationMappings(stationMappings, "integration_test", DateTime.Now);
                var result = _StationMappingService.GetStationMappingsByCadentCallLetter("DMAH");

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        public void CanSaveAndRetrieveStationMappingsForNonExistingStation()
        {
            using (new TransactionScopeWrapper())
            {
                var stationMappings = (new List<StationMappingsFileRequestDto>
                {
                    new StationMappingsFileRequestDto
                    {
                        CadentCallLetters = "CLTV",
                        ExtendedCallLetters = "CLTV-DT2",
                        NSICallLetters = "CLTV-TV+",
                        Affiliate = "POS"
                    }
                }).GroupBy(x => x.CadentCallLetters).First();

                _StationMappingService.SaveStationMappings(stationMappings, "integration_test", DateTime.Now);
                var results = _StationMappingService.GetStationMappingsByCadentCallLetter("CLTV");
                var newStationExists = _StationService.StationExistsInBroadcastDatabase("CLTV");

                Assert.IsNotEmpty(results);
                Assert.AreEqual(true, newStationExists);
            }
        }

        [Test]
        public void CreateNewUnratedStation()
        {
            using (new TransactionScopeWrapper())
            {
                var stationMappings = (new List<StationMappingsFileRequestDto>
                {
                    new StationMappingsFileRequestDto
                    {
                        CadentCallLetters = "NPLC",
                        ExtendedCallLetters = "KPLC-DT3",
                        SigmaCallLetters = "NPLC",
                        NSILegacyCallLetters = null,
                        NSICallLetters = null,
                        Affiliate = "BOU"
                    }
                }).GroupBy(x => x.CadentCallLetters).First();

                _StationMappingService.SaveStationMappings(stationMappings, "integration_test", DateTime.Now);
                var results = _StationMappingService.GetStationMappingsByCadentCallLetter("NPLC");
                var newStation = _StationRepository.GetBroadcastStationByCallLetters("NPLC");
                var newStationMonths = _StationRepository.GetStationMonthDetailsForStation(newStation.Id);
                var latestMediaMonth = _StationRepository.GetLatestMediaMonthIdFromStationMonthDetailsList();

                Assert.IsNotEmpty(results);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetJsonSettings()));
                
                Assert.AreEqual("NPLC", newStation.CallLetters);
                Assert.AreEqual("NPLC", newStation.LegacyCallLetters);
                Assert.AreEqual("BOU", newStation.Affiliation);
                Assert.IsNull(newStation.Code);
                Assert.IsNull(newStation.MarketCode);

                Assert.IsNotNull(newStationMonths);
                Assert.AreEqual(1, newStationMonths.Count);
                Assert.IsNull(newStationMonths[0].DistributorCode);
                Assert.IsNull(newStationMonths[0].MarketCode);
                Assert.AreEqual("BOU", newStationMonths[0].Affiliation);
                Assert.AreEqual(latestMediaMonth, newStationMonths[0].MediaMonthId);
            }
        }

        [Test]
        public void CreateNewUnratedStationWithoutAffiliate()
        {
            using (new TransactionScopeWrapper())
            {
                var stationMappings = (new List<StationMappingsFileRequestDto>
                {
                    new StationMappingsFileRequestDto
                    {
                        CadentCallLetters = "QAUO",
                        ExtendedCallLetters = "KAUO-LD4",
                        SigmaCallLetters = null,
                        NSILegacyCallLetters = null,
                        NSICallLetters = null,
                        Affiliate = null
                    }
                }).GroupBy(x => x.CadentCallLetters).First();

                _StationMappingService.SaveStationMappings(stationMappings, "integration_test", DateTime.Now);
                var results = _StationMappingService.GetStationMappingsByCadentCallLetter("QAUO");
                var newStation = _StationRepository.GetBroadcastStationByCallLetters("QAUO");
                var newStationMonths = _StationRepository.GetStationMonthDetailsForStation(newStation.Id);
                var latestMediaMonth = _StationRepository.GetLatestMediaMonthIdFromStationMonthDetailsList();

                Assert.IsNotEmpty(results);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetJsonSettings()));

                Assert.AreEqual("QAUO", newStation.CallLetters);
                Assert.AreEqual("QAUO", newStation.LegacyCallLetters);
                Assert.IsNull(newStation.Affiliation);
                Assert.IsNull(newStation.Code);
                Assert.IsNull(newStation.MarketCode);

                Assert.IsNotNull(newStationMonths);
                Assert.AreEqual(1, newStationMonths.Count);
                Assert.IsNull(newStationMonths[0].DistributorCode);
                Assert.IsNull(newStationMonths[0].MarketCode);
                Assert.IsNull(newStationMonths[0].Affiliation);
                Assert.AreEqual(latestMediaMonth, newStationMonths[0].MediaMonthId);
            }
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StationMappingsDto), "StationId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            return jsonSettings;
        }

        [Test]
        public void CanGetStationByCallLetters()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _StationMappingService.GetStationByCallLetters("KOB+");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "RateDataThrough");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ManifestMaxEndDate");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "OriginMarket");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver,
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        public void CanGetStationByCadentCallLetters()
        {
            using (new TransactionScopeWrapper())
            {
                // Use an existing station
                var result = _StationMappingService.GetStationByCallLetters("OOCO");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "RateDataThrough");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ManifestMaxEndDate");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "OriginMarket");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver,
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        public void CanGetStationStartingWithCallLetters()
        {
            using (new TransactionScopeWrapper())
            {
                var stationMappings = (new List<StationMappingsFileRequestDto>
                {
                    new StationMappingsFileRequestDto
                    {
                        CadentCallLetters = "TOOCO",
                        ExtendedCallLetters = "TOOCO-DT2",
                        NSICallLetters = "TKOCO-TV 5.2",
                        NSILegacyCallLetters = "TKOCO",
                        SigmaCallLetters = "TOOCO"
                    }
                }).GroupBy(x => x.CadentCallLetters).First();

                _StationMappingService.SaveStationMappings(stationMappings, "integration_test", DateTime.Now);
                var result = _StationMappingService.GetStationByCallLetters("TKOCO-TV");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "RateDataThrough");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ManifestMaxEndDate");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "OriginMarket");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver,
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Ignore("This was only used to introduce the mappings to the integration database")]
        public void UploadStationMappingsExcellFile()
        {
            var fileName = "CadentBroadcastStationList.xlsx";
            _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now);
        }

        [Test]
        public void UploadStationMappingsExcellFile_EmptyOwnership()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "CadentBroadcastStationList_EmptyOwnership.xlsx";
                Assert.Throws<ApplicationException>(() => 
                    _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now)
                ,    "Station CW4 does not have ownership group populated");
            }
        }

        [Test]
        public void UploadStationMappingsExcellFile_EmptyOwnershipInd()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "CadentBroadcastStationList_EmptyOwnershipInd.xlsx";
                Assert.DoesNotThrow(() =>
                    _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now));
            }
        }

        [Test]
        public void UploadStationMappingsExcellFile_EmptySalesGroup()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "CadentBroadcastStationList_EmptySales.xlsx";
                Assert.Throws<ApplicationException>(() =>
                    _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now)
                , "Station CLTV does not have sales group populated");
            }
        }

        [Test]
        public void UploadStationMappingsExcellFile_DifferentOwnership()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "CadentBroadcastStationList_DifferentOwnership.xlsx";
                Assert.Throws<ApplicationException>(() =>
                    _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now)
                , "Station CLTV cannot have multiple ownership groups");
            }
        }

        [Test]
        public void UploadStationMappingsExcellFile_DifferentSales()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "CadentBroadcastStationList_DifferentSales.xlsx";
                Assert.Throws<ApplicationException>(() =>
                    _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now)
                , "Station CLTV cannot have multiple sales groups");
            }
        }

        [Test]
        public void UploadStationMappingsExcelFile_MaxOwnershipLength()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "CadentBroadcastStationList_OwnershipMaxLength.xlsx";
                Assert.Throws<ApplicationException>(() =>
                    _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now)
                , "Station CLTV has ownership name greater than 100 chars.");
            }
        }

        [Test]
        public void UploadStationMappingsExcelFile_MaxSalesLength()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "CadentBroadcastStationList_SalesMaxLength.xlsx";
                Assert.Throws<ApplicationException>(() =>
                    _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now)
                , "Station CLTV has sales group name greater than 100 chars.");
            }
        }

        [Test]
        public void UploadStationMappingsExcelFile_MaxOwnershipLength2()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "CadentBroadcastStationList_OwnershipMaxLength2.xlsx";
                Assert.Throws<ApplicationException>(() =>
                    _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now)
                , "Stations CLTV,CW4 have ownership name greater than 100 chars.");
            }
        }

        [Test]
        public void UploadStationMappingsExcelFile_MaxSalesLength2()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "CadentBroadcastStationList_SalesMaxLength2.xlsx";
                Assert.Throws<ApplicationException>(() =>
                    _StationMappingService.LoadStationMappings(new FileStream($@".\Files\ImportStationMappings\{fileName}"
                                , FileMode.Open, FileAccess.Read), fileName, "integration_test", DateTime.Now)
                , "Stations CLTV,CW4 have sales group name greater than 100 chars.");
            }
        }
    }
}
