using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using System.IO;
using Services.Broadcast.Clients;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class DataLakeFileServiceTests
    {
        private string _DataLakeFolder;

        [SetUp]
        public void Setup()
        {
            var launchDarklyClientStub = (LaunchDarklyClientStub) IntegrationTestApplicationServiceFactory.Instance.Resolve<ILaunchDarklyClient>();
            var configurationSettingsHelper = IntegrationTestApplicationServiceFactory.Instance.Resolve<IConfigurationSettingsHelper>();

            _DataLakeFolder = launchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_PIPELINE_VARIABLES] 
                ? configurationSettingsHelper.GetConfigValue<string>(ConfigKeys.DataLake_SharedFolder) 
                : BroadcastServiceSystemParameter.DataLake_SharedFolder;
        }

        [Test]
        public void FileStreamSaveTest()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(new FileServiceDataLakeStubb());
            var fileService = IntegrationTestApplicationServiceFactory.Instance.Resolve<IFileService>();

            
            var fileName = "Assembly Schedule For Mapping.csv";

            var request = new FileRequest
            {
                StreamData = new FileStream(
                        @".\Files\Assembly Schedule For Mapping.csv",
                        FileMode.Open,
                        FileAccess.Read),
                FileName = fileName
            };
            IDataLakeFileService _DataLakeFileService =
                IntegrationTestApplicationServiceFactory.GetApplicationService<IDataLakeFileService>();

            _DataLakeFileService.Save(request);

            fileName = Path.Combine(_DataLakeFolder, Path.GetFileName(fileName));
            Assert.True(fileService.Exists(fileName));
        }


        [Test]
        public void FileSaveTest()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(new FileServiceDataLakeStubb());
            var fileService = IntegrationTestApplicationServiceFactory.Instance.Resolve<IFileService>();

            var filePath = @".\Files\1Chicago WLS Syn 4Q16.xml";

            IDataLakeFileService _DataLakeFileService =
                IntegrationTestApplicationServiceFactory.GetApplicationService<IDataLakeFileService>();
            _DataLakeFileService.Save(filePath);

            filePath = Path.Combine(_DataLakeFolder, Path.GetFileName(filePath));
            Assert.True(fileService.Exists(filePath));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ErrorEmailTest()
        {
            var impersonateUser = IntegrationTestApplicationServiceFactory.Instance.Resolve<IImpersonateUser>();
            var emailService = new EmailerServiceStub();
            var fileService = new FileServiceSingleFileStubb();
            var dataLakeSystemParamteres = new Mock<IDataLakeSystemParameters>();
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            dataLakeSystemParamteres.Setup(r => r.GetSharedFolder()).Returns("C:\\");
            dataLakeSystemParamteres.Setup(r => r.GetNotificationEmail()).Returns("bernardo.botelho@axispoint.com");
            dataLakeSystemParamteres.Setup(r => r.GetUserName()).Returns(string.Empty);
            dataLakeSystemParamteres.Setup(r => r.GetPassword()).Returns(string.Empty);
            var dataLakeFileService = new DataLakeFileService(dataLakeSystemParamteres.Object, emailService, impersonateUser, fileService,featureToggle.Object,configurationSettingsHelper.Object);
            var fileName = "KeepingTrac_MissingData.xlsx";

            var request = new FileRequest
            {
                StreamData = new FileStream(
                        @".\Files\KeepingTrac_MissingData.xlsx",
                        FileMode.Open,
                        FileAccess.Read),
                FileName = fileName
            };
            fileService.ThrowOnCopyStreamCall = true;

            dataLakeFileService.Save(request);

            var response = EmailerServiceStub.LastMailMessageGenerated;
            response.Body = response.Body.Substring(0, 60);

            var jsonResolver = new IgnorableSerializerContractResolver();
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
        }
    }
}
