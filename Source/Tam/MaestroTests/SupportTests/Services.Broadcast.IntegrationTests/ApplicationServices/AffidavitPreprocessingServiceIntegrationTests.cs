using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Microsoft.Practices.Unity;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Mail;
using Common.Services;
using Services.Broadcast.ApplicationServices.Security;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitPreprocessingServiceIntegrationTests
    {
        private readonly IAffidavitPreprocessingService _AffidavitPreprocessingService;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;

        private const string USERNAME = "AffidavitPreprocessing_User";

        public AffidavitPreprocessingServiceIntegrationTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();

            _AffidavitPreprocessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitPreprocessingService>();
            _WWTVSharedNetworkHelper = IntegrationTestApplicationServiceFactory.Instance.Resolve<IWWTVSharedNetworkHelper>();
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
        [Ignore]
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



        [UseReporter(typeof(DiffReporter))]
        [Test]
        // use for manual testing and not automated running 
        public void Test_ProcessErrorFiles() //Errors returned from WWTV
        {
            FtpServiceStubb.ResponseFromGetFileList = new List<string>()
            {
                "Special_Ftp_Phantom_File.txt"
            };

            _AffidavitPreprocessingService.ProcessErrorFiles();

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(MailMessage), "Attachments");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            try
            {
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(EmailerServiceStubb.LastMailMessageGenerated, jsonSettings));
            }
            finally
            {
                EmailerServiceStubb.ClearLastMessage();
                FtpServiceStubb.CleanUpCreatedFiles();
            }
        }

        [Ignore]
        [Test]
        public void Testerester()
        {
            var src = "ddr.txt";
            string  dest = "\\\\Cadfs10\\tbn\\WWTVErrors\\ddr.txt";
            string share = "\\\\Cadfs10\\tbn\\WWTVErrors";
            string result = "svc_wwtvdata@crossmw.com";
            result = "78!ttwG&Dc$4fB2xZ94x";

            using (WWTVSharedNetworkHelper.GetConnection(share))
            {
                File.Copy(src, dest);
                File.Delete(dest);
            }
        }
    }
}
