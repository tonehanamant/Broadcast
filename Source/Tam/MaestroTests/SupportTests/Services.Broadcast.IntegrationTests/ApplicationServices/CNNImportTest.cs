using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalUtilities.Utilities;
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


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ImportCNN_Clean()
        {
            var filename = @".\Files\CNNAMPMBarterObligations_Clean.xlsx";

            var factory = IntegrationTestApplicationServiceFactory.Instance.Resolve<IInventoryFileImporterFactory>();
            var importer = factory.GetFileImporterInstance(InventoryFile.InventorySourceType.CNN);

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
            importer.ExtractFileData(stream, ratesFile, fileProblems);

            if (fileProblems.Any())
            {
                fileProblems.ForEach(f => Console.WriteLine(f.ProblemDescription));
                Assert.IsTrue(fileProblems.Any(), "Problems in file found.");
            }

            // we're going to ignore effective date in json compare, but first we'll ensure
            // at least one known "blank" effective date is set to the proper default value of today
            var effectiveDate = ratesFile.InventoryGroups.First().Manifests[2].EffectiveDate;
            Assert.AreEqual(DateTime.Today,effectiveDate,string.Format("The effective date for KAEF is expected to be today, but was \'{0}\'",effectiveDate));

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
            jsonResolver.Ignore(typeof(StationInventoryGroup), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "Id");
            jsonResolver.Ignore(typeof(StationInventoryManifest), "EffectiveDate");
            jsonResolver.Ignore(typeof (StationInventoryManifest), "InventorySourceId");
            jsonResolver.Ignore(typeof (StationInventoryManifest), "InvetoryFileId");
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
            var importer = factory.GetFileImporterInstance(InventoryFile.InventorySourceType.CNN);

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
            importer.ExtractFileData(stream, ratesFile, fileProblems);

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
            var importer = factory.GetFileImporterInstance(InventoryFile.InventorySourceType.CNN);

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
            importer.ExtractFileData(stream, ratesFile, fileProblems);

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
            var importer = factory.GetFileImporterInstance(InventoryFile.InventorySourceType.CNN);

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

            Assert.Catch(() => importer.ExtractFileData(stream, ratesFile, fileProblems),
                CNNFileImporter.NoGoodDaypartsFound);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadInventoryFile_Duplicate_Inventory()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\CNNAMPMBarterObligations_Duplicate_Inventory.xlsx";

                var factory = IntegrationTestApplicationServiceFactory.Instance.Resolve<IInventoryFileImporterFactory>();
                var importer = factory.GetFileImporterInstance(InventoryFile.InventorySourceType.CNN);

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
                importer.ExtractFileData(stream, ratesFile, fileProblems);

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
