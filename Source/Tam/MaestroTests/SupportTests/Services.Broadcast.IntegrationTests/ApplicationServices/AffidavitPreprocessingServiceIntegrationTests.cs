using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class AffidavitPreprocessingServiceIntegrationTests
    {
        private readonly IAffidavitPreprocessingService _AffidavitPreprocessingService;
        private readonly IWWTVSharedNetworkHelper _WWTVSharedNetworkHelper;

        private const string USERNAME = "AffidavitPreprocessing_User";

        public AffidavitPreprocessingServiceIntegrationTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStub>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStub_Empty>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStub>();

            _AffidavitPreprocessingService = IntegrationTestApplicationServiceFactory
                .GetApplicationService<IAffidavitPreprocessingService>();
            _WWTVSharedNetworkHelper =
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IWWTVSharedNetworkHelper>();
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_InvalidFile()
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
        public void AffidavitPreprocessing_StrataInvalidHeadersCount()
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
        [Ignore("Source changed to Sigma")]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_KeepingTracInvalidHeadersCount()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\KeepingTrac_MissingHeaders.xlsx" };
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
        public void AffidavitPreprocessing_StrataInvalidData()
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
        [Ignore("Source changed to Sigma")]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_KeepingTracInvalidData()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\KeepingTrac_MissingData.xlsx" };
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
        public void AffidavitPreprocessing_StrataValidFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\StrataSBMSInvoicePostExportValid.xlsx" };
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
        [Ignore("Source changed to Sigma")]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_KeepingTracValidFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\KeepingTrac_ValidFile.xlsx" };
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
        [Ignore("Used for manual testing")]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_ManualTestForZipArchives()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>()
                {
                    @".\Files\Sigma-ValidFile.csv",
                    @".\Files\StrataSBMSInvoicePostExportValid.xlsx"
                };
                //_AffidavitPreprocessingService._CreateAndUploadZipArchiveToWWTV(fileNames);                
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitPreprocessing_SigmaMissingRequiredColumn()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\Sigma-MissingRequiredColumn.csv" };
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
        public void AffidavitPreprocessing_SigmaMissingRequiredData()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\Sigma-MissingRequiredData.csv" };
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
        public void AffidavitPreprocessing_Sigma_ValidFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileNames = new List<string>() { @".\Files\Sigma-ValidFile.csv" };
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
