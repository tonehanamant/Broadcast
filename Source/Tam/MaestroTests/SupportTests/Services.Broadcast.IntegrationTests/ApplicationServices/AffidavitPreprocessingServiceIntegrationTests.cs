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
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_Empty>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();

            _AffidavitPreprocessingService = IntegrationTestApplicationServiceFactory
                .GetApplicationService<IAffidavitPreprocessingService>();
            _WWTVSharedNetworkHelper =
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IWWTVSharedNetworkHelper>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_ValidFileKeepingTrac()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\KeepingTrac_Test_Clean.csv" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

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
        public void AffidavitPreprocessing_MissingHeadersKeepingTrac()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\KeepingTrac_Test_MissingHeaders.csv" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

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
        public void AffidavitPreprocessing_MissingDataKeepingTrac()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\KeepingTrac_Test_MissingData.csv" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

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
        public void AffidavitPreprocessing_InvalidStrataFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\Checkers BVS Report.DAT" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

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
        public void AffidavitPreprocessing_InvalidSheetName()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\StrataSBMSInvoicePostExportInvalidSheetName.xlsx" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

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
        public void AffidavitPreprocessing_InvalidHeadersCount()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\StrataSBMSInvoicePostExportInvalidColumnName.xlsx" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

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
        public void AffidavitPreprocessing_InvalidData()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\StrataSBMSInvoicePostExportInvalidData.xlsx" };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

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
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_ValidFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>()
                {
                    @".\Files\StrataSBMSInvoicePostExportValid.xlsx",
                    @".\Files\StrataSBMSInvoicePostExportValid.xlsx"
                };
                var validations = _AffidavitPreprocessingService.ValidateFiles(fileNames, USERNAME);

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
    }
}
