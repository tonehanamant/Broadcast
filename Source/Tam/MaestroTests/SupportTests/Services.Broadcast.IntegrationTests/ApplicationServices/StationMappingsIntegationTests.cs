using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
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

        public StationMappingsIntegationTests()
        {
            _StationMappingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationMappingService>();
            _StationService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationService>();
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
        public void IfNewStationIsProvidedWithNoNSICallLettersNoNewStationIsSaved()
        {
            using (new TransactionScopeWrapper())
            {
                var stationMappings = (new List<StationMappingsFileRequestDto>
                {
                    new StationMappingsFileRequestDto
                    {
                        CadentCallLetters = "CLTV",
                        ExtendedCallLetters = "CLTV-DT2",
                        Affiliate = "POS"
                    }
                }).GroupBy(x => x.CadentCallLetters).First();

                _StationMappingService.SaveStationMappings(stationMappings, "integration_test", DateTime.Now);
                var results = _StationMappingService.GetStationMappingsByCadentCallLetter("CLTV");
                var newStationExists = _StationService.StationExistsInBroadcastDatabase("CLTV");

                Assert.IsEmpty(results);
                Assert.AreEqual(false, newStationExists);
            }
        }
    }
}
