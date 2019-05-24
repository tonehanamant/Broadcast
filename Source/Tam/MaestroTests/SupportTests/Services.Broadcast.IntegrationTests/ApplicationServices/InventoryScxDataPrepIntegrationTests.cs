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

        [Ignore("To be fixed in PRI-8713")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InventoryScxDataPrep()
        {
            using (new TransactionScopeWrapper())
            {
                QuarterDetailDto quarter = new QuarterDetailDto
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                };
                var result = _InventoryScxDataPrep.GetInventoryScxData(quarter);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Ignore("To be fixed in PRI-8713")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InventoryScxFile_ValidateSingleUnitScxObject()
        {
            using (new TransactionScopeWrapper())
            {
                QuarterDetailDto quarter = new QuarterDetailDto
                {
                    EndDate = new DateTime(2019, 06, 30),
                    StartDate = new DateTime(2019, 04, 01),
                };
                var dataList = _InventoryScxDataPrep.GetInventoryScxData(quarter);
                var adx = _InventoryScxDataConverter.CreateAdxObject(dataList.Where(x => x.UnitName.Equals("AM 1")).Single());

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
