using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.IO;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.Entities;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitPostProcessingServiceIntegrationTests
    {
        private readonly IAffidavitPostProcessingService _AffidavitPostProcessingService;

        public AffidavitPostProcessingServiceIntegrationTests()
        {
            _AffidavitPostProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitPostProcessingService>();
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPostProcessing_FileDoesNotExist()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\SomeNonExistingFile.txt";
                var request = File.ReadAllText(filePath);

                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(BaseResponse), "Data");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPostProcessing_InvalidFileType()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\Checkers BVS Report.DAT";
                var request = File.ReadAllText(filePath);

                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(BaseResponse), "Data");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPostProcessing_InvalidFileContent()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitInValidFileContent.txt";
                var request = File.ReadAllText(filePath);

                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath);
                if (_AffidavitPostProcessingService.AffidavitValidationResult.Any() 
                        || response == null)
                    throw new Exception("Invalid file content.");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(BaseResponse), "Data");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPostProcessing_ValidFileContent()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitValidFile.txt";
                var request = File.ReadAllText(filePath);

                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(BaseResponse), "Data");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPostProcessing_ValidFileContent_SpotCost()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitValidFileContent_SpotCost.txt";
                var request = File.ReadAllText(filePath);

                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(BaseResponse), "Data");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver,
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPostProcessing_AffidavitValidFileContent_NullDemo()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitValidFileContent_NullDemo.txt";
                var request = File.ReadAllText(filePath);

                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(BaseResponse), "Data");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver,
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }
    }
}
