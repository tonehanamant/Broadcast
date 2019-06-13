using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
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
            var request = new InventoryFileSaveRequest
            {
                StreamData = new FileStream(
                @".\Files\multi-quarter_program_rate_file_wvtm.xml",
                FileMode.Open,
                FileAccess.Read),
                FileName = testName,
                RatingBook = 416
            };

            var result = _inventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2016, 09, 26));
            return result.FileId;
        }        
    }
}
