using System;
using System.Collections.Generic;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Converters.RateImport;
using Tam.Maestro.Common.DataLayer;
using System.Transactions;
using Services.Broadcast.Entities;
using System.IO;
using IntegrationTests.Common;
using Newtonsoft.Json;
using ApprovalTests;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Common.Services;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class TTWNFileImporterTests
    {
        private readonly IInventoryDaypartParsingEngine _InventoryDaypartParsingEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryDaypartParsingEngine>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanParseLNFile()
        {
            var _ttwnFileImporter = new TTWNFileImporter();
            _ttwnFileImporter.InventoryDaypartParsingEngine = _InventoryDaypartParsingEngine;
            _ttwnFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                //var filename = @".\Files\TTWN_AMNews-06.09.17.xlsx";
                var filename = @".\Files\TTWN_06.09.17.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile
                {
                    InventorySource = GetTtwnInventorySource()
                };

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttwnFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate );
                fileProblems = _ttwnFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "InventoryFileId");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "ProjectedStationImpressions");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(new { inventoryFile, fileProblems }, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanParseEMFile()
        {
            var _ttwnFileImporter = new TTWNFileImporter();
            _ttwnFileImporter.InventoryDaypartParsingEngine = _InventoryDaypartParsingEngine;
            _ttwnFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTWN_AMNews-06.09.17.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile
                {
                    InventorySource = GetTtwnInventorySource()
                };

                var  effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttwnFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttwnFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "InventoryFileId");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "ProjectedStationImpressions");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
                jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(new {inventoryFile, fileProblems}, jsonSettings);
                Approvals.Verify(json);
            }
        }     

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanParseENFile()
        {
            var _ttwnFileImporter = new TTWNFileImporter();
            _ttwnFileImporter.InventoryDaypartParsingEngine = _InventoryDaypartParsingEngine;
            _ttwnFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTWN_EN_06.09.17.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile
                {
                    InventorySource = GetTtwnInventorySource()
                };

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttwnFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttwnFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "InventoryFileId");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "ProjectedStationImpressions");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
                jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(new { inventoryFile, fileProblems }, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InvalidStation()
        {
            var _ttwnFileImporter = new TTWNFileImporter();
            _ttwnFileImporter.InventoryDaypartParsingEngine = _InventoryDaypartParsingEngine;
            _ttwnFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTWN_InvalidStation.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttwnFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttwnFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                //jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(fileProblems, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void NoKnownStations()
        {
            var _ttwnFileImporter = new TTWNFileImporter();
            _ttwnFileImporter.InventoryDaypartParsingEngine = _InventoryDaypartParsingEngine;
            _ttwnFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTWN_NoKnownStations.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttwnFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttwnFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(fileProblems, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InvalidDaypartSpots()
        {
            var _ttwnFileImporter = new TTWNFileImporter();
            _ttwnFileImporter.InventoryDaypartParsingEngine = _InventoryDaypartParsingEngine;
            _ttwnFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTWN_InvalidDaypartSpots.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttwnFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttwnFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(fileProblems, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ValidDayparts()
        {
            var _ttwnFileImporter = new TTWNFileImporter();
            _ttwnFileImporter.InventoryDaypartParsingEngine = _InventoryDaypartParsingEngine;
            _ttwnFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTWN_ValidDayparts.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile
                {
                    InventorySource = GetTtwnInventorySource()
                };

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttwnFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttwnFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "InventoryFileId");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "ProjectedStationImpressions");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
                jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(new { inventoryFile, fileProblems }, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InvalidDaypart()
        {
            var _ttwnFileImporter = new TTWNFileImporter();
            _ttwnFileImporter.InventoryDaypartParsingEngine = _InventoryDaypartParsingEngine;
            _ttwnFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                var filename = @".\Files\TTWN_InvalidDaypart.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttwnFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttwnFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource"); 
                //jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(fileProblems, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ZeroInDaypartSpot()
        {
            var _ttwnFileImporter = new TTWNFileImporter();
            _ttwnFileImporter.InventoryDaypartParsingEngine = _InventoryDaypartParsingEngine;
            _ttwnFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTWN_ZeroInDaypartSpot.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var inventoryFile = new InventoryFile();
                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttwnFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttwnFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var json = IntegrationTestHelper.ConvertToJson(fileProblems, jsonSettings);

                Approvals.Verify(json);
            }
        }

        private static InventorySource GetTtwnInventorySource()
        {
            return new InventorySource
            {
                Id = 4,
                InventoryType = InventorySourceTypeEnum.Barter,
                IsActive = true,
                Name = "TTWN"
            };
        }
    }
}
