using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Microsoft.Practices.Unity;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Net.Mail;
using Common.Services;
using Services.Broadcast.ApplicationServices.Security;
using Tam.Maestro.Common.DataLayer;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PostLogPreprocessingServiceTests
    {
        private readonly IPostLogPreprocessingService _PostLogPreprocessingService;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;

        private const string USERNAME = "PostLogPreprocessing_User";

        public PostLogPreprocessingServiceTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_Empty>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();

            _PostLogPreprocessingService = IntegrationTestApplicationServiceFactory
                .GetApplicationService<IPostLogPreprocessingService>();
            _WWTVSharedNetworkHelper =
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IWWTVSharedNetworkHelper>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogPreprocessing_ValidSigmaFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\C786.Sigma.ValidFile.csv" };
                var validations = _PostLogPreprocessingService.ValidateFiles(fileNames, USERNAME, FileSourceEnum.Sigma);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVOutboundFileValidationResult), "CreatedDate");

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
        public void PostLogPreprocessing_MissingDataSigmaFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\C786.Sigma.MissingData.csv" };
                var validations = _PostLogPreprocessingService.ValidateFiles(fileNames, USERNAME, FileSourceEnum.Sigma);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVOutboundFileValidationResult), "CreatedDate");

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
        public void PostLogPreprocessing_MissingColumnsSigmaFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\C786.Sigma.MissingColumns.csv" };
                var validations = _PostLogPreprocessingService.ValidateFiles(fileNames, USERNAME, FileSourceEnum.Sigma);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVOutboundFileValidationResult), "CreatedDate");

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
        public void PostLogPreprocessing_UnknownFileType()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\BCOP3666.txt" };
                var validations = _PostLogPreprocessingService.ValidateFiles(fileNames, USERNAME, FileSourceEnum.Unknown);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVOutboundFileValidationResult), "CreatedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(validations, jsonSettings));
            }
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        [Ignore]
        // use for manual testing and not automated running 
        public void PostLogPreprocessing_ProcessFiles()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_Empty>();
            //for testing real upload: IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpService>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
            
            _PostLogPreprocessingService.ProcessFiles("PostLogPreprocessingTest");

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(MailMessage), "Attachments");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            try
            {
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(EmailerServiceStubb.LastMailMessageGenerated,
                    jsonSettings));
            }
            finally
            {
                EmailerServiceStubb.ClearLastMessage();
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessKeepingTracFiles()
        {
            using (new TransactionScopeWrapper())
            {
                var userName = "Test_ProcessKeepingTracFiles";

                var result = _PostLogPreprocessingService.ValidateFiles(
                    new List<string>
                    {
                        @".\Files\KeepingTrac_ValidFile.xlsx",
                        @".\Files\KeepingTrac_MissingData.xlsx",
                        @".\Files\KeepingTrac_MissingHeaders.xlsx"
                    },
                    userName, FileSourceEnum.KeepingTrac);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVOutboundFileValidationResult), "CreatedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver,
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }
    }
    
}
