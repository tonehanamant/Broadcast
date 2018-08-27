using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.Entities;
using Microsoft.Practices.Unity;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitEmailProcessorTests
    {
        private readonly IAffidavitEmailProcessorService _AffidavitEmailProcessorService;
        private readonly IFileTransferEmailHelper _EmailHelper;
        private readonly IAffidavitRepository _AffidavitRepository;
        private const string _UserName = "Test User";
        
        public AffidavitEmailProcessorTests()
        {
            _AffidavitEmailProcessorService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitEmailProcessorService>();
            _EmailHelper = IntegrationTestApplicationServiceFactory.GetApplicationService<IFileTransferEmailHelper>();
            _AffidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
        }
        private void VerifyAffidavit(int affidavitId)
        {
            var response = _AffidavitRepository.GetAffidavit(affidavitId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(AffidavitFileProblem), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileProblem), "AffidavitFileId");
            jsonResolver.Ignore(typeof(AffidavitFileDetail), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileDetail), "AffidavitFileId");
            jsonResolver.Ignore(typeof(AffidavitFile), "CreatedDate");
            jsonResolver.Ignore(typeof(AffidavitFile), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
        }




        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitEmailService_ErrorLoggingLogging()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\Checkers BVS Report.DAT";

                var message = "AffidavitEmailService_ErrorLoggingLogging Message";
                int affidavitId = _AffidavitEmailProcessorService.LogAffidavitError(filePath, message);

                VerifyAffidavit(affidavitId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitEmailService_CreateValidationErrorEmailBody()
        {
            List<AffidavitValidationResult> validationErrors = new List<AffidavitValidationResult>()
            {
                new AffidavitValidationResult() { ErrorMessage = "is required",InvalidField = "FieldName", InvalidLine =  123 },
                new AffidavitValidationResult() { ErrorMessage = "is also required",InvalidField = "SecondField" },
            };
            string fileName = "FileNameCausingError";

            var body = _AffidavitEmailProcessorService.CreateValidationErrorEmailBody(validationErrors, fileName);

            Approvals.Verify(body);
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitEmailService_CreateTechErrorEmailBody()
        {
            string errorMessage = "An error occured, this message should follow known email format standards.";
            string fileName = "FileName";

            var body = _AffidavitEmailProcessorService.CreateTechErrorEmailBody(errorMessage, fileName);

            Approvals.Verify(body);
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitEmailService_CreateFailedFTPFileEmailBody()
        {
            List<string> fileList = new List<string>()
            {
                "File1",
                "File2"
            };
            string ftpPath = "ftp://localhost/DonutsAndUnicorns";

            var body = _AffidavitEmailProcessorService.CreateFailedFTPFileEmailBody(fileList, ftpPath);

            Approvals.Verify(body);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        // use for manual testing and not automated running 
        public void AffidavitEmailService_CreateInvalidDataFileEmailBody() //Validation errors for files going to WWTV
        {
            var validationError = new OutboundAffidavitFileValidationResultDto()
            {
                Status = AffidaviteFileProcessingStatus.Invalid,
                FilePath = @"E:\Users\broadcast-ftp\eula.1028.txt",
                FileName = "eula.1028.txt",
                ErrorMessages = new List<string>() {
                    "Required field ISCI/AD-ID is null or empty in row 1",
                    "Required field ISCI/AD-ID is null or empty in row 2",
                    "Required field ISCI/AD-ID is null or empty in row 3"
                }
            };

            var body = _EmailHelper.CreateInvalidDataFileEmailBody(validationError.ErrorMessages, "\\FilePath", validationError.FileName);
            Approvals.Verify(body);
        }        
    }
}
