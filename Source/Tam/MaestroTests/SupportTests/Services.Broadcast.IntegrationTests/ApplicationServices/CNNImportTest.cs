﻿using System;
using System.Collections.Generic;
using System.IO;
using ApprovalUtilities.Utilities;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class CNNImportTest
    {
       

        [Test]
        public void ImportCNN()
        {
            var filename = @".\Files\CNNAMPMBarterObligations.xlsx";

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
                BlockName = "integration Test",
                FlightWeeks = flightWeeks,
                FlightEndDate = new DateTime(2016, 10, 31),
                FlightStartDate = new DateTime(2016, 11, 27)
            };
            importer.LoadFromSaveRequest(request);
            importer.ExtractFileData(stream,ratesFile,fileProblems);
            fileProblems.ForEach(f => Console.WriteLine(f.ProblemDescription));
        }
    }
}
