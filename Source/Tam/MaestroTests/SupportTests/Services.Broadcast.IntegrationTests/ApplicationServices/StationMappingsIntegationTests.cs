using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
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
        [UseReporter(typeof(DiffReporter))]
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
        [UseReporter(typeof(DiffReporter))]
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
        [UseReporter(typeof(DiffReporter))]
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
        [UseReporter(typeof(DiffReporter))]
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
    }
}
