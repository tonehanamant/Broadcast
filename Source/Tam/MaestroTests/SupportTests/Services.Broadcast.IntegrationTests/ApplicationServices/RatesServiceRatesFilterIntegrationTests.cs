using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.IO;
using System.Reflection;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    class RatesServiceRatesFilterIntegrationTests
    {
        private IInventoryService _inventoryService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();

        private IInventoryFileRepository _inventoryFileRepository =
            IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();

        int _stationCodeWVTM = 5044;

        private int _Setup(string testName)
        {
            var request = new InventoryFileSaveRequest();


            request.RatesStream = new FileStream(
                @".\Files\multi-quarter_program_rate_file_wvtm.xml",
                FileMode.Open,
                FileAccess.Read);
            request.UserName = "IntegrationTestUser";
            request.FileName = testName;
            request.RatingBook = 416;

            var result = _inventoryService.SaveInventoryFile(request);
            return result.FileId;
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetRatesForToday()
        {
            using (new TransactionScopeWrapper())
            {
                //_Setup(MethodBase.GetCurrentMethod().Name);
                //var response = _ratesService.GetStationRates("OpenMarket", _stationCodeWVTM, "today", new DateTime(2016, 01, 01));

                ////Ignore the Id on each Rate record
                //var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof (StationProgramAudienceRateDto), "Id");
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                //var jsonSettings = new JsonSerializerSettings()
                //{
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //    ContractResolver = jsonResolver
                //};
                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }


        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetRatesForThisQuarter()
        {
            using (new TransactionScopeWrapper())
            {
                //_Setup(MethodBase.GetCurrentMethod().Name);
                //var response = _ratesService.GetStationRates(
                //    "OpenMarket", 
                //    _stationCodeWVTM,
                //    "thisquarter",
                //    new DateTime(2016, 08, 15));

                ////Ignore the Id on each Rate record
                //var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof (StationProgramAudienceRateDto), "Id");
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                //var jsonSettings = new JsonSerializerSettings()
                //{
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //    ContractResolver = jsonResolver
                //};
                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }

        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetRatesForLastQuarter()
        {
            using (new TransactionScopeWrapper())
            {
                //_Setup(MethodBase.GetCurrentMethod().Name);
                //var response = _ratesService.GetStationRates(
                //    "OpenMarket", 
                //    _stationCodeWVTM,
                //    "lastquarter",
                //    new DateTime(2016, 08, 15));

                ////Ignore the Id on each Rate record
                //var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof (StationProgramAudienceRateDto), "Id");
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                //var jsonSettings = new JsonSerializerSettings()
                //{
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //    ContractResolver = jsonResolver
                //};
                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }

        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetRatesForCustomDateRange()
        {
            using (new TransactionScopeWrapper())
            {
            //    _Setup(MethodBase.GetCurrentMethod().Name);
            //    var response = _ratesService.GetStationRates(
            //        "OpenMarket", 
            //        _stationCodeWVTM,
            //        new DateTime(2016, 05, 15),
            //        new DateTime(2016, 12, 15));

            //    //Ignore the Id on each Rate record
            //    var jsonResolver = new IgnorableSerializerContractResolver();
            //    jsonResolver.Ignore(typeof (StationProgramAudienceRateDto), "Id");
            //    jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
            //    var jsonSettings = new JsonSerializerSettings()
            //    {
            //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //        ContractResolver = jsonResolver
            //    };
            //    Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }

        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStationRates()
        {
            using (new TransactionScopeWrapper())
            {
                //_Setup(MethodBase.GetCurrentMethod().Name);
                //var response = _ratesService.GetAllStationRates("OpenMarket", _stationCodeWVTM);

                ////Ignore the Id on each Rate record
                //var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof (StationProgramAudienceRateDto), "Id");
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                //var jsonSettings = new JsonSerializerSettings()
                //{
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //    ContractResolver = jsonResolver
                //};
                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }

        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetRatesForSinglePeriodWithMultiperiodProgram()
        {
            using (new TransactionScopeWrapper())
            {
                //_Setup(MethodBase.GetCurrentMethod().Name);
                //var response = _ratesService.GetStationRates("OpenMarket", _stationCodeWVTM, "today", new DateTime(2016, 10, 01));

                ////Ignore the Id on each Rate record
                //var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof (StationProgramAudienceRateDto), "Id");
                //jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Audiences");
                //var jsonSettings = new JsonSerializerSettings()
                //{
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //    ContractResolver = jsonResolver
                //};
                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }


        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStationsWithTodaysData()
        {
            using (var scope = new TransactionScopeWrapper())
            {
                _Setup(MethodBase.GetCurrentMethod().Name);
                var today = new DateTime(2016, 01, 01);
                var response = _inventoryService.GetStationsWithFilter("OpenMarket", "withtodaysdata", today);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "FlightWeeks");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));

            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStationsWithoutTodaysData()
        {
            using (new TransactionScopeWrapper())
            {
                _Setup(MethodBase.GetCurrentMethod().Name);
                var today = new DateTime(2016, 01, 01);
                var response = _inventoryService.GetStationsWithFilter("OpenMarket", "withouttodaysdata", today);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "FlightWeeks");
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
        [UseReporter(typeof(DiffReporter))]
        public void ShouldNotReturnDataFromFailedInventoryFile()
        {
            using (var scope = new TransactionScopeWrapper())
            {
                var fileId =_Setup(MethodBase.GetCurrentMethod().Name);
                _inventoryFileRepository.UpdateInventoryFileStatus(fileId, InventoryFile.FileStatusEnum.Failed);
                var today = new DateTime(2016, 01, 01);
                var response = _inventoryService.GetStationsWithFilter("OpenMarket", "withtodaysdata", today);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));

            }
        }
    }
}
