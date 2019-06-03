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
        private readonly IInventoryScxDataPrep _InventoryScxDataPrep;
        private readonly IInventoryScxDataConverter _InventoryScxDataConverter;

        public InventoryScxDataPrepIntegrationTests()
        {
            _InventoryScxDataPrep = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryScxDataPrep>();
            _InventoryScxDataConverter = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryScxDataConverter>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InventoryScxDataPrep()
        {
            using (new TransactionScopeWrapper())
            {
                InventoryScxDownloadRequest request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    DaypartCodeId = 1,
                    InventorySourceId = 7,
                    UnitNames = new List<string> { "ExpiresGroupsTest" }
                };
                var result = _InventoryScxDataPrep.GetInventoryScxData(request.InventorySourceId, request.DaypartCodeId, request.StartDate, request.EndDate, request.UnitNames);

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
                string fileName = "Barter_A25-54_Q1 CNN.xlsx";
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(request, "sroibu", now);

                InventoryScxDownloadRequest scxRequest = new InventoryScxDownloadRequest()
                {
                    StartDate = new DateTime(2019, 01, 01),
                    EndDate = new DateTime(2019, 03, 31),
                    InventorySourceId = 5,
                    UnitNames = new List<string> { "Unit 1", "Unit 2", "Unit 3", "Unit 4"},
                    DaypartCodeId = 2
                };
                var result = _InventoryScxDataPrep.GetInventoryScxData(scxRequest.InventorySourceId, scxRequest.DaypartCodeId, scxRequest.StartDate, scxRequest.EndDate, scxRequest.UnitNames);

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
                InventoryScxDownloadRequest request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 06, 30),
                    StartDate = new DateTime(2019, 04, 01),
                    DaypartCodeId = 1,
                    InventorySourceId = 4,
                    UnitNames = new List<string> { "AM 1" }
                };
                var dataList = _InventoryScxDataPrep.GetInventoryScxData(request.InventorySourceId, request.DaypartCodeId, request.StartDate, request.EndDate, request.UnitNames);

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
    }
}
