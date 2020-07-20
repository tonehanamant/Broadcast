using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using System.Net.Mail;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class PostLogPreprocessingServiceTests
    {
        private readonly IPostLogPreprocessingService _PostLogPreprocessingService;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;

        private const string USERNAME = "PostLogPreprocessing_User";

        public PostLogPreprocessingServiceTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStub>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStub_Empty>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStub>();

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
                var validations = _PostLogPreprocessingService.ValidateFiles(fileNames, USERNAME, DeliveryFileSourceEnum.Sigma);

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
                var validations = _PostLogPreprocessingService.ValidateFiles(fileNames, USERNAME, DeliveryFileSourceEnum.Sigma);

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
                var validations = _PostLogPreprocessingService.ValidateFiles(fileNames, USERNAME, DeliveryFileSourceEnum.Sigma);

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
                var validations = _PostLogPreprocessingService.ValidateFiles(fileNames, USERNAME, DeliveryFileSourceEnum.Unknown);

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
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStub>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStub_Empty>();
            //for testing real upload: IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpService>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStub>();
            
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
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(EmailerServiceStub.LastMailMessageGenerated,
                    jsonSettings));
            }
            finally
            {
                EmailerServiceStub.ClearLastMessage();
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
                        @".\Files\KeepingTrac_MissingHeaders.xlsx",
                        @".\Files\C786.Sigma.MissingColumns.csv"
                    },
                    userName, DeliveryFileSourceEnum.KeepingTrac);
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
