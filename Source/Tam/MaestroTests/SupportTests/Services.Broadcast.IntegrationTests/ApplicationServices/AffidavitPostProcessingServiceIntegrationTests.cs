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
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.Entities;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitPostProcessingServiceIntegrationTests
    {
        private readonly IAffidavitPostProcessingService _AffidavitPostProcessingService;
        private readonly IAffidavitRepository _AffidavitRepository;

        public AffidavitPostProcessingServiceIntegrationTests()
        {
            _AffidavitPostProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitPostProcessingService>();
            _AffidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
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

                string errorMessage;
                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath,out errorMessage);

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
        public void AffidavitPostProcessing_InvalidFileType()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\Checkers BVS Report.DAT";

                string errorMessage;
                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath, out errorMessage);
                int affidavitId = _AffidavitPostProcessingService.LogAffidavitError(filePath, errorMessage.Substring(0, 25));

                VerifyAffidavitLog(affidavitId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPostProcessing_InvalidFileContent()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitInValidFileContent.txt";
                var request = File.ReadAllText(filePath);

                string errorMessage;
                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath,out errorMessage);
                // necessary to substring as the error will contain line number source code which may change
                int affidavitId = _AffidavitPostProcessingService.LogAffidavitError(filePath, errorMessage.Substring(0, 25));
                VerifyAffidavitLog(affidavitId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPostProcessing_ValidFileContent()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitValidFile.txt";

                string errorMessage;
                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath, out errorMessage);

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

                string errorMessage;
                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath, out errorMessage);

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

                string errorMessage;
                AffidavitSaveRequest response = _AffidavitPostProcessingService.ParseWWTVFile(filePath, out errorMessage);

                Assert.IsEmpty(errorMessage,"Unexpected error: " + errorMessage);

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
        public void AffidavitPostProcessing_File_Error_Logging()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_bad_file.txt";
                var request = File.ReadAllText(filePath);

                string errorMessage;
                _AffidavitPostProcessingService.ParseWWTVFile(filePath, out errorMessage);
                int affidavitId = _AffidavitPostProcessingService.LogAffidavitError(filePath, errorMessage.Substring(0,25));

                VerifyAffidavitLog(affidavitId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPostProcessing_File_Error_Date_Time()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_bad_file_Times.txt";

                string errorMessage;
                _AffidavitPostProcessingService.ParseWWTVFile(filePath, out errorMessage);

                Assert.IsTrue(errorMessage.Contains("Record: 1: field: 'Time'"),errorMessage);
                Assert.IsTrue(errorMessage.Contains("Record: 1: field: 'LeadOutStartTime'"), errorMessage);
                Assert.IsTrue(errorMessage.Contains("Record: 1: field: 'LeadInEndTime'"),errorMessage);
                Assert.IsTrue(!errorMessage.Contains("Record: 2: field: 'Time'"), errorMessage);
                Assert.IsTrue(errorMessage.Contains("Record: 3: field: 'Time'"), errorMessage);
                Assert.IsTrue(errorMessage.Contains("Record: 4: field: 'Time'"), errorMessage);
                Assert.IsTrue(!errorMessage.Contains("Record: 5: field: 'Time'"), errorMessage);
                Assert.IsTrue(errorMessage.Contains("Record: 6: field: 'Time'"), errorMessage);
            }
        }

        private void VerifyAffidavitLog(int affidavitId)
        {
            var response = _AffidavitRepository.GetAffidavit(affidavitId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(AffidavitFileProblem), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileProblem), "AffidavitFileId");
            jsonResolver.Ignore(typeof(AffidavitFile), "CreatedDate");
            jsonResolver.Ignore(typeof(AffidavitFile), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
        }
    }
}
