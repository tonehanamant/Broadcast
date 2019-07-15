using NUnit.Framework;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using Tam.Maestro.Common.DataLayer;
using Services.Broadcast.Converters.Scx;
using System;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.Entities.Scx;
using System.Collections.Generic;
using System.IO;
using Services.Broadcast.ApplicationServices;
using static Services.Broadcast.Entities.Scx.ScxMarketDto;
using static Services.Broadcast.Entities.OpenMarketInventory.ProposalOpenMarketInventoryWeekDto;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    class InventoryScxDataPrepIntegrationTests
    {
        private readonly IInventoryScxDataPrepFactory _InventoryScxDataPrepFactory;
        private readonly IInventoryScxDataConverter _InventoryScxDataConverter;

        public InventoryScxDataPrepIntegrationTests()
        {
            _InventoryScxDataPrepFactory = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryScxDataPrepFactory>();
            _InventoryScxDataConverter = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryScxDataConverter>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InventoryScxDataPrep()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryScxDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(Entities.Enums.InventorySourceTypeEnum.Barter);

                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    DaypartCodeId = 1,
                    InventorySourceId = 7,
                    UnitNames = new List<string> { "ExpiresGroupsTest" }
                };
                var result = inventoryScxDataPrep.GetInventoryScxData(request.InventorySourceId, request.DaypartCodeId, request.StartDate, request.EndDate, request.UnitNames);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        //this test should be removed when the old UI is removed
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InventoryScxDataPrep_OldUI()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryScxDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(Entities.Enums.InventorySourceTypeEnum.Barter);

                string fileName = "Barter_A25-54_Q1 CNN.xlsx";
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(request, "sroibu", now);

                var scxRequest = new InventoryScxDownloadRequest()
                {
                    StartDate = new DateTime(2019, 01, 01),
                    EndDate = new DateTime(2019, 03, 31),
                    InventorySourceId = 5,
                    UnitNames = new List<string> { "Unit 1", "Unit 2", "Unit 3", "Unit 4"},
                    DaypartCodeId = 2
                };
                var result = inventoryScxDataPrep.GetInventoryScxData(scxRequest.InventorySourceId, scxRequest.DaypartCodeId, scxRequest.StartDate, scxRequest.EndDate, scxRequest.UnitNames);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ScxProgram), "ProgramId");
                jsonResolver.Ignore(typeof(InventoryWeekProgram), "ProgramId");
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
        public void InventoryScxFile_ValidateSingleUnitScxObject()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryScxDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(Entities.Enums.InventorySourceTypeEnum.Barter);

                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 06, 30),
                    StartDate = new DateTime(2019, 04, 01),
                    DaypartCodeId = 1,
                    InventorySourceId = 4,
                    UnitNames = new List<string> { "AM 1" }
                };
                var dataList = inventoryScxDataPrep.GetInventoryScxData(request.InventorySourceId, request.DaypartCodeId, request.StartDate, request.EndDate, request.UnitNames);

                var adx = _InventoryScxDataConverter.CreateAdxObject(dataList.First());

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(detailLine), "startTime");
                jsonResolver.Ignore(typeof(detailLine), "endTime");
                jsonResolver.Ignore(typeof(document), "date");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(adx, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryScxDataAbcOAndOTest()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryScxDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(Entities.Enums.InventorySourceTypeEnum.ProprietaryOAndO);

                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2020, 03, 01),
                    StartDate = new DateTime(2020, 01, 01),
                    DaypartCodeId = 4,
                    InventorySourceId = 10
                };

                var result = inventoryScxDataPrep.GetInventoryScxData(request.InventorySourceId, request.DaypartCodeId, request.StartDate, request.EndDate, request.UnitNames);
                var jsonResolver = new IgnorableSerializerContractResolver();
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
        public void GetInventoryScxDataNbcOAndOTest()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryScxDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(Entities.Enums.InventorySourceTypeEnum.ProprietaryOAndO);

                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 01),
                    StartDate = new DateTime(2019, 01, 01),
                    DaypartCodeId = 1,
                    InventorySourceId = 11
                };

                var result = inventoryScxDataPrep.GetInventoryScxData(request.InventorySourceId, request.DaypartCodeId, request.StartDate, request.EndDate, request.UnitNames);
                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }
    }
}
