using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Helpers;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Ignore("Not certain why this is ignored.")]
    public class AffidavitEmailProcessorTests
    {
        private readonly IWWTVEmailProcessorService _AffidavitEmailProcessorService;
        private readonly IFileTransferEmailHelper _EmailHelper;
        private readonly IAffidavitRepository _AffidavitRepository;
        
        public AffidavitEmailProcessorTests()
        {
            _AffidavitEmailProcessorService = IntegrationTestApplicationServiceFactory.GetApplicationService<IWWTVEmailProcessorService>();
            _EmailHelper = IntegrationTestApplicationServiceFactory.GetApplicationService<IFileTransferEmailHelper>();
            _AffidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
        }
        private void VerifyAffidavit(int affidavitId)
        {
            var response = _AffidavitRepository.GetAffidavit(affidavitId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(FileProblem), "Id");
            jsonResolver.Ignore(typeof(FileProblem), "FileId");
            jsonResolver.Ignore(typeof(ScrubbingFileDetail), "Id");
            jsonResolver.Ignore(typeof(ScrubbingFileDetail), "AffidavitFileId");
            jsonResolver.Ignore(typeof(ScrubbingFile), "CreatedDate");
            jsonResolver.Ignore(typeof(ScrubbingFile), "Id");

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
            List<WWTVInboundFileValidationResult> validationErrors = new List<WWTVInboundFileValidationResult>()
            {
                new WWTVInboundFileValidationResult() { ErrorMessage = "is required",InvalidField = "FieldName", InvalidLine =  123 },
                new WWTVInboundFileValidationResult() { ErrorMessage = "is also required",InvalidField = "SecondField" },
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
            var validationError = new WWTVOutboundFileValidationResult()
            {
                Status = FileProcessingStatusEnum.Invalid,
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
