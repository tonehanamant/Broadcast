using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.IO;
using Common.Services;
using Tam.Maestro.Common.DataLayer;
using Microsoft.Practices.Unity;
using Services.Broadcast.ApplicationServices.Security;
using System;
using Services.Broadcast.Entities.DTO;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PostLogPostProcessingServiceIntegrationTests
    {
        private readonly IPostLogPostProcessingService _PostLogPostProcessingService;
        private readonly IPostLogService _PostLogService;
        private const string _UserName = "PostLog Post Processing Test User";
        
        public PostLogPostProcessingServiceIntegrationTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_Empty>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();

            _PostLogPostProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostLogPostProcessingService>();
            _PostLogService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostLogService>();
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogPostProcessing_ValidFileContent()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_KeepingTracValidFile.txt";
                var fileContents = File.ReadAllText(filePath);

                WWTVSaveResult response = _PostLogPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);
                VerifyPostLogFile(response.Id.Value);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogPostProcessing_ValidFileContent_WithNullValues()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_KeepingTracValidFile_WithNullValues.txt";
                var fileContents = File.ReadAllText(filePath);

                WWTVSaveResult response = _PostLogPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);
                VerifyPostLogFile(response.Id.Value);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogPostProcessing_BCOP3771()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\Keeping Trac BCOP-3771.txt";
                var fileContents = File.ReadAllText(filePath);

                _PostLogPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);
                var result = _PostLogService.GetClientScrubbingForProposal(33029, new ProposalScrubbingRequest());
                VerifyClientPostScrubbingObject(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogPostProcessing_ValidFileContent_BCOP3666()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\BCOP3666.txt";
                var fileContents = File.ReadAllText(filePath);

                WWTVSaveResult response = _PostLogPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);
                // technically nothing to verify, but the original error caused an exception and this is for a black listed isci anyway, so nothing to process
                VerifyResults(response);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogPostProcessing_FileErrorDateTime()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_bad_file_Times.txt";
                var fileContents = File.ReadAllText(filePath);

                WWTVSaveResult response = _PostLogPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);

                VerifyResults(response);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogPostProcessing_BasicRequiredFieldValidationErrors()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_Basic_Required_Validation.txt";
                var fileContents = File.ReadAllText(filePath);

                WWTVSaveResult response = _PostLogPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);

                VerifyResults(response);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DLAndProcessWWTVFiles_Error_InvalidFileFormat()
        {
            using (var trans = new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_SingleFile>();
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
                
                EmailerServiceStubb.LastMailMessageGenerated = null;
                var response = _PostLogPostProcessingService.DownloadAndProcessWWTVFiles(_UserName);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVSaveResult), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(response, jsonSettings);
                Approvals.Verify(json);
            }
        }

        private static void VerifyResults(WWTVSaveResult response)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(WWTVSaveResult), "Id");

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(response, jsonSettings);
            Approvals.Verify(json);
        }

        private void VerifyClientPostScrubbingObject(ClientPostScrubbingProposalDto result)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(LookupDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
            jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
            jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
            jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDetailPostScrubbingDto), "ScrubbingClientId");
            jsonResolver.Ignore(typeof(ProposalDetailPricingGuideDto), "ProposalId");
            jsonResolver.Ignore(typeof(ProposalDetailPricingGuideDto), "ProposalDetailId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        private void VerifyPostLogFile(int fileId)
        {
            var _PostLogRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostLogRepository>();
            var response = _PostLogRepository.GetPostLogFile(fileId, true);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ScrubbingFileProblem), "Id");
            jsonResolver.Ignore(typeof(ScrubbingFileProblem), "FileId");
            jsonResolver.Ignore(typeof(ScrubbingFileDetail), "Id");
            jsonResolver.Ignore(typeof(ScrubbingFileDetail), "ScrubbingFileId");
            jsonResolver.Ignore(typeof(ScrubbingFileDetail), "ModifiedDate");
            jsonResolver.Ignore(typeof(ClientScrub), "Id");
            jsonResolver.Ignore(typeof(ClientScrub), "ScrubbingFileDetailId");
            jsonResolver.Ignore(typeof(ClientScrub), "ModifiedDate");
            jsonResolver.Ignore(typeof(ScrubbingFile), "CreatedDate");
            jsonResolver.Ignore(typeof(ScrubbingFileAudiences), "ClientScrubId");
            jsonResolver.Ignore(typeof(ScrubbingFile), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
        }
    }
}