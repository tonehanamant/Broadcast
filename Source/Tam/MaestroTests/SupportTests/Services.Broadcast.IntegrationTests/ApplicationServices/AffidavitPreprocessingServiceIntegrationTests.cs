﻿using ApprovalTests;
using ApprovalTests.Reporters;
using EntityFrameworkMapping.Broadcast;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitPreprocessingServiceIntegrationTests
    {
        private readonly IAffidavitPreprocessingService _AffidavitPreprocessingService;
        private const string USERNAME = "AffidavitPreprocessing_User";

        public AffidavitPreprocessingServiceIntegrationTests()
        {
            _AffidavitPreprocessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitPreprocessingService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_InvalidStrataFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\Checkers BVS Report.DAT" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(OutboundAffidavitFileValidationResultDto), "CreatedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(validations, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_InvalidSheetName()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\StrataSBMSInvoicePostExportInvalidSheetName.xlsx" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(OutboundAffidavitFileValidationResultDto), "CreatedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(validations, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_InvalidHeadersCount()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\StrataSBMSInvoicePostExportInvalidColumnName.xlsx" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(OutboundAffidavitFileValidationResultDto), "CreatedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(validations, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_InvalidData()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\StrataSBMSInvoicePostExportInvalidData.xlsx" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(OutboundAffidavitFileValidationResultDto), "CreatedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(validations, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_ValidFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\StrataSBMSInvoicePostExportValid.xlsx", @".\Files\StrataSBMSInvoicePostExportValid.xlsx" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(OutboundAffidavitFileValidationResultDto), "CreatedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(validations, jsonSettings));
            }
        }


        [Ignore]
        [Test]
        // use for manual testing and not automated running 
        public void Test_ProcessErrorFiles() //Errors returned from WWTV
        {
            _AffidavitPreprocessingService.ProcessErrorFiles();
        }

        [Ignore]
        [Test]
        // use for manual testing and not automated running 
        public void Test_ProcessInvalidFiles() //Validation errors for files going to WWTV
        {
            var fileList = new List<OutboundAffidavitFileValidationResultDto>()
            {
                new OutboundAffidavitFileValidationResultDto()
                {
                     Status = AffidaviteFileProcessingStatus.Invalid,
                     FilePath = @"E:\Users\broadcast-ftp\eula.1028.txt",
                     FileName = "eula.1028.txt"
                }
            };

            _AffidavitPreprocessingService.ProcessInvalidFiles(fileList);
        }
    }
}
