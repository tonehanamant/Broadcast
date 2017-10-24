using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests.Reporters;
using NUnit.Framework;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using System.Transactions;
using Services.Broadcast.Entities;
using System.IO;
using IntegrationTests.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Newtonsoft.Json;
using ApprovalTests;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class TTNWFileImporterTests
    {
        private TTNWFileImporter _ttnwFileImporter = new TTNWFileImporter();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanParseLNFile()
        {
            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                //var filename = @".\Files\TTNW_AMNews-06.09.17.xlsx";
                var filename = @".\Files\TTNW_06.09.17.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate ,fileProblems);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
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
            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                var filename = @".\Files\TTNW_AMNews-06.09.17.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var  effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate, fileProblems);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
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
            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                var filename = @".\Files\TTNW_EN_06.09.17.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate, fileProblems);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource"); 
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
            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                var filename = @".\Files\TTNW_InvalidStation.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate, fileProblems);

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
        public void NoKnownStations()
        {
            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                var filename = @".\Files\TTNW_NoKnownStations.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate, fileProblems);

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
            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                var filename = @".\Files\TTNW_InvalidDaypartSpots.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate, fileProblems);

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
            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                var filename = @".\Files\TTNW_ValidDayparts.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();

                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate, fileProblems);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(StationInventoryManifest), "EffectiveDate");
                jsonResolver.Ignore(typeof(StationInventoryManifest), "FileId");
                jsonResolver.Ignore(typeof(StationInventoryGroup), "InventorySource");
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
            _ttnwFileImporter.BroadcastDataDataRepository =
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {

                var filename = @".\Files\TTNW_InvalidDaypart.xlsx";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                var inventoryFile = new InventoryFile();

                var effectiveDate = DateTime.Parse("10/1/2017");
                var fileProblems = new List<InventoryFileProblem>();
                _ttnwFileImporter.ExtractFileData(fileStream, inventoryFile, effectiveDate, fileProblems);

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

    }
}
