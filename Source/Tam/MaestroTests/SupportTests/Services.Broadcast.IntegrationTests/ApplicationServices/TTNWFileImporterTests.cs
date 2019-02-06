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

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class TTNWFileImporterTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanParseLNFile()
        {
            var _ttnwFileImporter = new TTNWFileImporter();

            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                //var filename = @".\Files\TTNW_AMNews-06.09.17.xlsx";
                var filename = @".\Files\TTNW_06.09.17.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile
                {
                    InventorySource = GetTtnwInventorySource()
                };

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate );
                fileProblems = _ttnwFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
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
            var _ttnwFileImporter = new TTNWFileImporter();

            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_AMNews-06.09.17.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile
                {
                    InventorySource = GetTtnwInventorySource()
                };

                var  effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttnwFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate"); 
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
            var _ttnwFileImporter = new TTNWFileImporter();

            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_EN_06.09.17.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile
                {
                    InventorySource = GetTtnwInventorySource()
                };

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttnwFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
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
            var _ttnwFileImporter = new TTNWFileImporter();

            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_InvalidStation.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttnwFileImporter.FileProblems;

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
            var _ttnwFileImporter = new TTNWFileImporter();

            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_NoKnownStations.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttnwFileImporter.FileProblems;

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
            var _ttnwFileImporter = new TTNWFileImporter();

            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_InvalidDaypartSpots.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttnwFileImporter.FileProblems;

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
            var _ttnwFileImporter = new TTNWFileImporter();

            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_ValidDayparts.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile
                {
                    InventorySource = GetTtnwInventorySource()
                };

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttnwFileImporter.FileProblems;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifestBase), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
                jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
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
            var _ttnwFileImporter = new TTNWFileImporter();

            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                var filename = @".\Files\TTNW_InvalidDaypart.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttnwFileImporter.FileProblems;

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
            var _ttnwFileImporter = new TTNWFileImporter();

            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var filename = @".\Files\TTNW_ZeroInDaypartSpot.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var inventoryFile = new InventoryFile();
                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate);
                fileProblems = _ttnwFileImporter.FileProblems;

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

        private static InventorySource GetTtnwInventorySource()
        {
            return new InventorySource
            {
                Id = 4,
                InventoryType = InventoryType.NationalUnit,
                IsActive = true,
                Name = "TTNW"
            };
        }
    }
}
