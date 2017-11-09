using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class CNNImportTest
    {
        private static InventorySource _cnnInventorySource;

        public CNNImportTest()
        {
            var inventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

            _cnnInventorySource = inventoryRepository.GetInventorySourceByName("CNN");
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ImportCNN_Clean()
        {
            var filename = @".\Files\CNNAMPMBarterObligations_Clean.xlsx";

            var factory = IntegrationTestApplicationServiceFactory.Instance.Resolve<IInventoryFileImporterFactory>();
            var importer = factory.GetFileImporterInstance(_cnnInventorySource);

            var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var ratesFile = new InventoryFile();

            List<InventoryFileProblem> fileProblems = new List<InventoryFileProblem>();

            var flightWeeks = new List<FlightWeekDto>();
            flightWeeks.Add(new FlightWeekDto()
            {
                StartDate = new DateTime(2016, 10, 31),
                EndDate = new DateTime(2016, 11, 06),
                IsHiatus = false
            });

            var request = new InventoryFileSaveRequest()
            {
                FileName = filename,
                RatesStream = stream,
                EffectiveDate = new DateTime(2016, 10, 31)
            };
            importer.LoadFromSaveRequest(request);
            importer.ExtractFileData(stream, ratesFile, request.EffectiveDate,fileProblems);

            if (fileProblems.Any())
            {
                fileProblems.ForEach(f => Console.WriteLine(f.ProblemDescription));
                Assert.IsTrue(fileProblems.Any(), "Problems in file found.");
            }

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
            jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "Id");
            jsonResolver.Ignore(typeof (StationInventoryManifest), "InventorySourceId");
            jsonResolver.Ignore(typeof (StationInventoryManifest), "FileId");
            jsonResolver.Ignore(typeof(InventoryFile), "Id");
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(ratesFile, jsonSettings);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ImportCNN_Bad_Dayparts()
        {
            var filename = @".\Files\CNNAMPMBarterObligations_BadDayparts.xlsx";

            var factory = IntegrationTestApplicationServiceFactory.Instance.Resolve<IInventoryFileImporterFactory>();
            var importer = factory.GetFileImporterInstance(_cnnInventorySource);

            var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var ratesFile = new InventoryFile();

            List<InventoryFileProblem> fileProblems = new List<InventoryFileProblem>();

            var flightWeeks = new List<FlightWeekDto>();
            flightWeeks.Add(new FlightWeekDto()
            {
                StartDate = new DateTime(2016, 10, 31),
                EndDate = new DateTime(2016, 11, 06),
                IsHiatus = false
            });

            var request = new InventoryFileSaveRequest()
            {
                FileName = filename,
                RatesStream = stream,                
                EffectiveDate = new DateTime(2016, 10, 31)
            };
            importer.LoadFromSaveRequest(request);
            importer.ExtractFileData(stream, ratesFile, request.EffectiveDate, fileProblems);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(fileProblems, jsonSettings);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ImportCNN_Bad_Station()
        {
            var filename = @".\Files\CNNAMPMBarterObligations_BadStations.xlsx";

            var factory = IntegrationTestApplicationServiceFactory.Instance.Resolve<IInventoryFileImporterFactory>();
            var importer = factory.GetFileImporterInstance(_cnnInventorySource);

            var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var ratesFile = new InventoryFile();

            List<InventoryFileProblem> fileProblems = new List<InventoryFileProblem>();

            var flightWeeks = new List<FlightWeekDto>();
            flightWeeks.Add(new FlightWeekDto()
            {
                StartDate = new DateTime(2016, 10, 31),
                EndDate = new DateTime(2016, 11, 06),
                IsHiatus = false
            });

            var request = new InventoryFileSaveRequest()
            {
                FileName = filename,
                RatesStream = stream,
                EffectiveDate = new DateTime(2016, 10, 31)
            };
            importer.LoadFromSaveRequest(request);
            importer.ExtractFileData(stream, ratesFile, request.EffectiveDate,fileProblems);

            if (!fileProblems.Any())
            {
                throw new Exception("Bad stations expected but not found.");
            }

            var jsonResolver = new IgnorableSerializerContractResolver();
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(fileProblems, jsonSettings);
            Approvals.Verify(json);
        }

        [Test]
        public void ImportCNN_Bad_All_Dayparts()
        {
            var filename = @".\Files\CNNAMPMBarterObligations_AllBadDayparts.xlsx";

            var factory = IntegrationTestApplicationServiceFactory.Instance.Resolve<IInventoryFileImporterFactory>();
            var importer = factory.GetFileImporterInstance(_cnnInventorySource);

            var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var ratesFile = new InventoryFile();

            List<InventoryFileProblem> fileProblems = new List<InventoryFileProblem>();

            var flightWeeks = new List<FlightWeekDto>();
            flightWeeks.Add(new FlightWeekDto()
            {
                StartDate = new DateTime(2016, 10, 31),
                EndDate = new DateTime(2016, 11, 06),
                IsHiatus = false
            });

            var request = new InventoryFileSaveRequest()
            {
                FileName = filename,
                RatesStream = stream,
                EffectiveDate = new DateTime(2016, 10, 31)
            };
            importer.LoadFromSaveRequest(request);

            Assert.Catch(() => importer.ExtractFileData(stream, ratesFile, request.EffectiveDate, fileProblems),
                CNNFileImporter.NoGoodDaypartsFound);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ImportCNN_Bad_Spots()
        {
            const string filename = @".\Files\CNNAMPMBarterObligations_BadSpots.xlsx";

            var factory = IntegrationTestApplicationServiceFactory.Instance.Resolve<IInventoryFileImporterFactory>();
            var importer = factory.GetFileImporterInstance(_cnnInventorySource);
            var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var ratesFile = new InventoryFile();
            var fileProblems = new List<InventoryFileProblem>();
            var request = new InventoryFileSaveRequest()
            {
                FileName = filename,
                RatesStream = stream,
                EffectiveDate = new DateTime(2016, 10, 31)
            };

            importer.LoadFromSaveRequest(request);
            importer.ExtractFileData(stream, ratesFile, request.EffectiveDate, fileProblems);

            var jsonResolver = new IgnorableSerializerContractResolver();
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(fileProblems, jsonSettings);
            
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadInventoryFile_Duplicate_Inventory()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\CNNAMPMBarterObligations_Duplicate_Inventory.xlsx";

                var factory = IntegrationTestApplicationServiceFactory.Instance.Resolve<IInventoryFileImporterFactory>();
                var importer = factory.GetFileImporterInstance(_cnnInventorySource);

                var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var ratesFile = new InventoryFile();

                List<InventoryFileProblem> fileProblems = new List<InventoryFileProblem>();

                var flightWeeks = new List<FlightWeekDto>();
                flightWeeks.Add(new FlightWeekDto()
                {
                    StartDate = new DateTime(2016, 10, 31),
                    EndDate = new DateTime(2016, 11, 06),
                    IsHiatus = false
                });

                var request = new InventoryFileSaveRequest()
                {
                    FileName = filename,
                    RatesStream = stream,
                    EffectiveDate = new DateTime(2016, 10, 31)
                };
                importer.LoadFromSaveRequest(request);
                importer.ExtractFileData(stream, ratesFile, request.EffectiveDate, fileProblems);

                if (!fileProblems.Any())
                {
                    throw new Exception("Duplicates expected, but not found :(");
                }

                var jsonResolver = new IgnorableSerializerContractResolver();
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
