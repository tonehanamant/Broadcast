using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System.IO;
using Common.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class DataLakeFileServiceTests
    {

        [Test]
        public void FileStreamSaveTest()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(new FileServiceDataLakeStubb());
            var fileService = IntegrationTestApplicationServiceFactory.Instance.Resolve<IFileService>();

            var dataLakeFolder = BroadcastServiceSystemParameter.DataLake_SharedFolder;
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

            fileName = Path.Combine(dataLakeFolder, Path.GetFileName(fileName));
            Assert.True(fileService.Exists(fileName));
        }


        [Test]
        public void FileSaveTest()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(new FileServiceDataLakeStubb());
            var fileService = IntegrationTestApplicationServiceFactory.Instance.Resolve<IFileService>();

            var filePath = @".\Files\1Chicago WLS Syn 4Q16.xml";
            var dataLakeFolder = BroadcastServiceSystemParameter.DataLake_SharedFolder;

            IDataLakeFileService _DataLakeFileService =
                IntegrationTestApplicationServiceFactory.GetApplicationService<IDataLakeFileService>();
            _DataLakeFileService.Save(filePath);

            filePath = Path.Combine(dataLakeFolder, Path.GetFileName(filePath));
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
            dataLakeSystemParamteres.Setup(r => r.GetSharedFolder()).Returns("C:\\");
            dataLakeSystemParamteres.Setup(r => r.GetNotificationEmail()).Returns("bernardo.botelho@axispoint.com");
            dataLakeSystemParamteres.Setup(r => r.GetUserName()).Returns(string.Empty);
            dataLakeSystemParamteres.Setup(r => r.GetPassword()).Returns(string.Empty);
            var dataLakeFileService = new DataLakeFileService(dataLakeSystemParamteres.Object, emailService, impersonateUser, fileService);
            var fileName = "KeepingTrac_MissingData.xlsx";

            var request = new FileRequest
            {
                StreamData = new FileStream(
                        @".\Files\KeepingTrac_MissingData.xlsx",
                        FileMode.Open,
                        FileAccess.Read),
                FileName = fileName
            };

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
