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

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PostLogPostProcessingServiceIntegrationTests
    {
        private readonly IPostLogPostProcessingService _PostLogPostProcessingService;
        private const string _UserName = "PostLog Post Processing Test User";
        
        public PostLogPostProcessingServiceIntegrationTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_Empty>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();

            _PostLogPostProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostLogPostProcessingService>();
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
    }
}