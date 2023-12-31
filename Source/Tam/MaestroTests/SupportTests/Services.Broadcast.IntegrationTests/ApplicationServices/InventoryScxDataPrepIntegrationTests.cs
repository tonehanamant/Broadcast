﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using static Services.Broadcast.Entities.OpenMarketInventory.ProposalOpenMarketInventoryWeekDto;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    class InventoryScxDataPrepIntegrationTests
    {
        private readonly IInventoryScxDataPrepFactory _InventoryScxDataPrepFactory;
        private readonly IInventoryScxDataConverter _InventoryScxDataConverter;
        private readonly IInventoryRatingsProcessingService _InventoryRatingsProcessingService;
        private readonly IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;

        public InventoryScxDataPrepIntegrationTests()
        {
            _InventoryScxDataPrepFactory = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryScxDataPrepFactory>();
            _InventoryScxDataConverter = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryScxDataConverter>();
            _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
            _InventoryFileRatingsJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRatingsJobsRepository>();
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void InventoryScxDataPrep()
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

                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.Id);

                var scxRequest = new InventoryScxDownloadRequest()
                {
                    StartDate = new DateTime(2019, 01, 01),
                    EndDate = new DateTime(2019, 03, 31),
                    InventorySourceId = 5,
                    UnitNames = new List<string> { "Unit 1", "Unit 2", "Unit 3", "Unit 4"},
                    StandardDaypartId = 2
                };
                var result = inventoryScxDataPrep.GetInventoryScxData(scxRequest.InventorySourceId, scxRequest.StandardDaypartId, scxRequest.StartDate, scxRequest.EndDate, scxRequest.UnitNames);

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
        [Category("long_running")]
        public void DoesNotReturnData_ForInventoryWithoutRatings()
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
                    UnitNames = new List<string> { "Unit 1", "Unit 2", "Unit 3", "Unit 4" },
                    StandardDaypartId = 2
                };
                var result = inventoryScxDataPrep.GetInventoryScxData(scxRequest.InventorySourceId, scxRequest.StandardDaypartId, scxRequest.StartDate, scxRequest.EndDate, scxRequest.UnitNames);

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
        [Category("long_running")]
        public void InventoryScxFile_ValidateSingleUnitScxObject()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryScxDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(Entities.Enums.InventorySourceTypeEnum.Barter);

                var fileName = "Barter_A25-54_Q1 CNN.xlsx";
                var request = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(request, "sroibu", now);

                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.Id);

                var scxRequest = new InventoryScxDownloadRequest()
                {
                    StartDate = new DateTime(2019, 01, 01),
                    EndDate = new DateTime(2019, 03, 31),
                    InventorySourceId = 5,
                    UnitNames = new List<string> { "Unit 1", "Unit 2", "Unit 3", "Unit 4" },
                    StandardDaypartId = 2
                };
                var dataList = inventoryScxDataPrep.GetInventoryScxData(scxRequest.InventorySourceId, scxRequest.StandardDaypartId, scxRequest.StartDate, scxRequest.EndDate, scxRequest.UnitNames);

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

        [Ignore("Not certain why we are ignoring this...")]
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
                    StandardDaypartId = 4,
                    InventorySourceId = 10
                };

                var result = inventoryScxDataPrep.GetInventoryScxData(request.InventorySourceId, request.StandardDaypartId, request.StartDate, request.EndDate, request.UnitNames);
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
        [Category("long_running")]
        public void GetInventoryScxDataNbcOAndOTest()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "OAndO_ValidFile4.xlsx";
                var fileSaveRequest = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(fileSaveRequest, "sroibu", now);

                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.Id);

                var inventoryScxDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(Entities.Enums.InventorySourceTypeEnum.ProprietaryOAndO);

                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 01),
                    StartDate = new DateTime(2019, 01, 01),
                    StandardDaypartId = 2,
                    InventorySourceId = 11
                };

                var result = inventoryScxDataPrep.GetInventoryScxData(request.InventorySourceId, request.StandardDaypartId, request.StartDate, request.EndDate, request.UnitNames);
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
        [Category("long_running")]
        public void GetInventoryScxDataNbcOAndOTest_Scx()
        {
            using (new TransactionScopeWrapper())
            {
                var fileName = "OAndO_ValidFile4.xlsx";
                var fileSaveRequest = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(fileSaveRequest, "sroibu", now);

                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.Id);

                var inventoryScxDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(Entities.Enums.InventorySourceTypeEnum.ProprietaryOAndO);

                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 01),
                    StartDate = new DateTime(2019, 01, 01),
                    StandardDaypartId = 2,
                    InventorySourceId = 11
                };

                var result = inventoryScxDataPrep.GetInventoryScxData(request.InventorySourceId, request.StandardDaypartId, request.StartDate, request.EndDate, request.UnitNames);

                var adx = _InventoryScxDataConverter.CreateAdxObject(result.First());

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
