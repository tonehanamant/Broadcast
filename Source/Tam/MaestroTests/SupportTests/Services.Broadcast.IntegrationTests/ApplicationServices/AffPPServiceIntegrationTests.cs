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
using System.Web.Http.Validation;
using Common.Services;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.Entities;
using Microsoft.Practices.Unity;
using Services.Broadcast.ApplicationServices.Security;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    /// <summary>
    /// AffidavitPostProcessingServiceIntegrationTests
    /// </summary>
    [TestFixture]
    public class AffPPServiceIntegrationTests
    {
        private readonly IAffidavitPostProcessingService _AffidavitPostProcessingService;
        private readonly IAffidavitRepository _AffidavitRepository;
        private const string _UserName = "Test User";

        private readonly IBroadcastAudiencesCache _AudiencesCache;

        public AffPPServiceIntegrationTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_Empty>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();

            _AudiencesCache = IntegrationTestApplicationServiceFactory.Instance.Resolve<IBroadcastAudiencesCache>();
            _AffidavitPostProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitPostProcessingService>();
            _AffidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffPP_ValidFileContent()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitValidFile.txt";
                var fileContents = File.ReadAllText(filePath);

                AffidavitSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);

                VerifyAffidavit(response.Id.Value);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffPP_ValidFileContent_SpotCost()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitValidFileContent_SpotCost.txt";
                var fileContents = File.ReadAllText(filePath);

                AffidavitSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);
                VerifyAffidavit(response.Id.Value);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffPP_AffidavitValidFileContent_NullDemo()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitValidFileContent_NullDemo.txt";
                var fileContents = File.ReadAllText(filePath);
                
                AffidavitSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);
                VerifyAffidavit(response.Id.Value);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffPP_File_Error_Date_Time()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_bad_file_Times.txt";
                var fileContents = File.ReadAllText(filePath);

                AffidavitSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(AffidavitSaveResult), "Id");

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
        public void AffPP_Basic_Required_Field_Validation_Errors()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_Basic_Required_Validation.txt";
                var fileContents = File.ReadAllText(filePath);

                AffidavitSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(AffidavitSaveResult), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver,
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }


        /// <summary>
        /// similar to AffPP_Basic_Required_Field_Validation_Errors() 
        /// but checks the output of the saved affidavit with validation errors (bascially 
        /// looking at the "Problems" table)
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffPP_Basic_Required_Field_Validation_Problems()
        {
            
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_Basic_Required_Validation.txt";
                var fileContents = File.ReadAllText(filePath);

                AffidavitSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);
                
                VerifyAffidavit(response.Id.Value);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffPP_Escaped_DoubleQuotes()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_Escaped_DoubleQuotes.txt";
                var fileContents = File.ReadAllText(filePath);

                AffidavitSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);
                VerifyAffidavit(response.Id.Value);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffPP_Overnight_Impressions_With_Decimals()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_Affidavit_Decimal_Overnight_Impressions.txt";
                var fileContents = File.ReadAllText(filePath);

                AffidavitSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents);
                VerifyAffidavit(response.Id.Value);
            }
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


        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostPrePost_Report_Perf_Test()
        {
            using (var trans = new TransactionScopeWrapper())
            {
                var demos = _AudiencesCache.GetAllLookups();
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();

                var fileName = "Master File APR18.xlsx";
                var filePath = @".\Files\Master File APR18.xlsx";
                var fileContents = File.OpenRead(filePath);

                int postingBookId = 437; // april 2018 book

                PostRequest request = new PostRequest()
                {
                    PlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3,
                    Audiences = demos.Select(d => d.Id).ToList(),
                    FileName = fileName,
                    PostStream= fileContents,
                    Equivalized = true,
                    PostingBookId = postingBookId
                };

                var response = sut.SavePost(request);
                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(response, jsonSettings);
                //Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DLAndProcessWWTVFiles_Empty()
        {
            using (var trans = new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_Empty>();
                IntegrationTestApplicationServiceFactory.Instance
                    .RegisterType<IImpersonateUser, ImpersonateUserStubb>();

                var srv = IntegrationTestApplicationServiceFactory
                    .GetApplicationService<IAffidavitPostProcessingService>();

                var response = srv.DownloadAndProcessWWTVFiles("WWTV Service");

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(response, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DLAndProcessWWTVFiles_Error_InvalidFileFormat()
        {
            using (var trans = new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
                IntegrationTestApplicationServiceFactory.Instance
                    .RegisterType<IFtpService, FtpServiceStubb_SingleFile>();
                IntegrationTestApplicationServiceFactory.Instance
                    .RegisterType<IImpersonateUser, ImpersonateUserStubb>();

                var srv = IntegrationTestApplicationServiceFactory
                    .GetApplicationService<IAffidavitPostProcessingService>();

                EmailerServiceStubb.LastMailMessageGenerated = null;
                srv.DownloadAndProcessWWTVFiles("WWTV Service");

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                // remove stack trace gobble-de-gook
                var body = EmailerServiceStubb.LastMailMessageGenerated.Body;
                EmailerServiceStubb.LastMailMessageGenerated.Body = body.Substring(0,body.IndexOf("It cannot be read in its current state"));
                var json = IntegrationTestHelper.ConvertToJson(EmailerServiceStubb.LastMailMessageGenerated, jsonSettings);
                Approvals.Verify(json);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DLAndProcessWWTVFiles_Validation_Errors()
        {
            using (var trans = new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
                IntegrationTestApplicationServiceFactory.Instance
                    .RegisterType<IFtpService, DownloadAndProcessWWTVFiles_Validation_Errors_Stubb>();
                IntegrationTestApplicationServiceFactory.Instance
                    .RegisterType<IImpersonateUser, ImpersonateUserStubb>();

                var srv = IntegrationTestApplicationServiceFactory
                    .GetApplicationService<IAffidavitPostProcessingService>();

                EmailerServiceStubb.LastMailMessageGenerated = null;
                srv.DownloadAndProcessWWTVFiles("WWTV Service");

                var jsonResolver = new IgnorableSerializerContractResolver();
                //jsonResolver.Ignore(typeof(StationInventoryManifestDaypart), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(EmailerServiceStubb.LastMailMessageGenerated, jsonSettings);
                Approvals.Verify(json);
            }
        }



        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DLAndProcessWWTVFiles_Clean()
        {
            using (var trans = new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
                IntegrationTestApplicationServiceFactory.Instance
                    .RegisterType<IFtpService, DownloadAndProcessWWTVFiles_ValidFile_Stubb>();
                IntegrationTestApplicationServiceFactory.Instance
                    .RegisterType<IImpersonateUser, ImpersonateUserStubb>();

                var srv = IntegrationTestApplicationServiceFactory
                    .GetApplicationService<IAffidavitPostProcessingService>();

                EmailerServiceStubb.LastMailMessageGenerated = null;
                var response = srv.DownloadAndProcessWWTVFiles("WWTV Service");

                Assert.IsNull(EmailerServiceStubb.LastMailMessageGenerated);
                Assert.IsEmpty(response.FailedDownloads);
                Assert.IsEmpty(response.ValidationErrors);
                Assert.IsTrue(response.FilesFoundToProcess.Count() == 1,"Expecting only one file found for processing");

                VerifyAffidavit(response.AffidavitSaveResults.First().Id.Value);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DLAndProcessWWTVFiles_KeepingTrac_Clean()
        {
            using (var trans = new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_KeepingTrac>();
                IntegrationTestApplicationServiceFactory.Instance
                    .RegisterType<IImpersonateUser, ImpersonateUserStubb>();

                var srv = IntegrationTestApplicationServiceFactory
                    .GetApplicationService<IAffidavitPostProcessingService>();

                var response = srv.DownloadAndProcessWWTVFiles("WWTV Service");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(AffidavitSaveResult), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(response, jsonSettings);
                Approvals.Verify(json);
            }
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DLAndProcessWWTVFiles_KeepingTrac_BadTime()
        {
            using (var trans = new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_KeepingTrac_BadTime>();
                IntegrationTestApplicationServiceFactory.Instance
                    .RegisterType<IImpersonateUser, ImpersonateUserStubb>();

                var srv = IntegrationTestApplicationServiceFactory
                    .GetApplicationService<IAffidavitPostProcessingService>();

                var response = srv.DownloadAndProcessWWTVFiles("WWTV Service");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(AffidavitSaveResult), "Id");
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
}
