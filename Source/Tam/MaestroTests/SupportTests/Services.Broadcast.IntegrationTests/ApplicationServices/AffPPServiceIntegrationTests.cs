using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.IO;
using System.Linq;
using Common.Services;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Microsoft.Practices.Unity;
using Services.Broadcast.ApplicationServices.Security;
using System.Net.Mail;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using System;

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

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));

                VerifyAffidavit(response.Id.Value);
            }
        }

        [Test]
        [Category("Impressions")]
        [UseReporter(typeof(DiffReporter))]
        public void AffPP_ValidFileContent_WithNullColumns()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitValidFile_NullValues.txt";
                var fileContents = File.ReadAllText(filePath);

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));

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

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));
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
                
                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));
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

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVSaveResult), "Id");

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

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVSaveResult), "Id");

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

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));
                
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

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));
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

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));
                VerifyAffidavit(response.Id.Value);
            }
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

                var response = srv.DownloadAndProcessWWTVFiles("WWTV Service", new DateTime(2019, 3, 31));

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
                srv.DownloadAndProcessWWTVFiles("WWTV Service", new DateTime(2019, 3, 31));

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
                srv.DownloadAndProcessWWTVFiles("WWTV Service", new DateTime(2019, 3, 31));

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
                var response = srv.DownloadAndProcessWWTVFiles("WWTV Service", new DateTime(2019, 3, 31));

                Assert.IsNull(EmailerServiceStubb.LastMailMessageGenerated);
                Assert.IsEmpty(response.FailedDownloads);
                Assert.IsEmpty(response.ValidationErrors);
                Assert.IsTrue(response.FilesFoundToProcess.Count() == 1,"Expecting only one file found for processing");

                VerifyAffidavit(response.SaveResults.First().Id.Value);
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

                var response = srv.DownloadAndProcessWWTVFiles("WWTV Service", new DateTime(2019, 3, 31));

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

                var response = srv.DownloadAndProcessWWTVFiles("WWTV Service", new DateTime(2019, 3, 31));

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

        [UseReporter(typeof(DiffReporter))]
        [Test]
        [Ignore]
        // use for manual testing and not automated running 
        public void Test_ProcessErrorFiles_Empty() //Errors returned from WWTV
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_Empty>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();

            _AffidavitPostProcessingService.ProcessErrorFiles();

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
        
        [UseReporter(typeof(DiffReporter))]
        [Test]
        // use for manual testing and not automated running 
        public void Test_ProcessErrorFiles_SingleFile() //Errors returned from WWTV
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_SingleFile>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
            
            _AffidavitPostProcessingService.ProcessErrorFiles();

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
        public void AffPP_ValidFileContent_BCOP4270()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_AffidavitValidFile_BCOP4270.txt";
                var fileContents = File.ReadAllText(filePath);

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));

                VerifyAffidavit(response.Id.Value);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffPP_ValidFileContent_BCOP4333()
        {
            using (new TransactionScopeWrapper())
            {
                var filePath = @".\Files\WWTV_StrataValidFile_BCOP-4333.txt";
                var fileContents = File.ReadAllText(filePath);

                WWTVSaveResult response = _AffidavitPostProcessingService.ProcessFileContents(_UserName, filePath, fileContents, new DateTime(2019, 3, 31));

                VerifyAffidavit(response.Id.Value);
            }
        }

        [Test]
        public void DLAndProcessWWTVFiles_DataLakeCopy()
        {
            using (var trans = new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFtpService, FtpServiceStubb_SingleFile>();
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();

                var affidavitPostProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitPostProcessingService>();
                var fileService = IntegrationTestApplicationServiceFactory.Instance.Resolve<IFileService>();
                
                var dataLakeFolder = BroadcastServiceSystemParameter.DataLake_SharedFolder;
                string filePath = Path.Combine(dataLakeFolder, "Special_Ftp_Phantom_File.txt");
                if (fileService.Exists(filePath))
                {
                    fileService.Delete(filePath);
                }
                
                affidavitPostProcessingService.DownloadAndProcessWWTVFiles("WWTV Service", new DateTime(2019, 3, 31));
                
                Assert.True(fileService.Exists(filePath));
            }
        }

        private void VerifyAffidavit(int affidavitId)
        {
            var response = _AffidavitRepository.GetAffidavit(affidavitId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(FileProblem), "Id");
            jsonResolver.Ignore(typeof(FileProblem), "FileId");
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
