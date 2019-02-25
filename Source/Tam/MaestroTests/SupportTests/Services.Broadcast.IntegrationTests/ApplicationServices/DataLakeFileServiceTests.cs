using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using System.IO;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class DataLakeFileServiceTests
    {
        private readonly IDataLakeFileService _DataLakeFileService = 
            IntegrationTestApplicationServiceFactory.GetApplicationService<IDataLakeFileService>();

        [Test]
        public void FileSaveTest()
        {
            var dataLakeFolder = BroadcastServiceSystemParameter.DataLake_SharedFolder;
            var fileName = "CNNAMPMBarterObligations_Clean.xlsx";

            var request = new FileRequest
            {
                StreamData = new FileStream(
                        @".\Files\CNNAMPMBarterObligations_Clean.xlsx",
                        FileMode.Open,
                        FileAccess.Read),
                FileName = fileName
            };

            _DataLakeFileService.Save(request);

            Assert.True(File.Exists(Path.Combine(dataLakeFolder, fileName)));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ErrorEmailTest()
        {
            var impersonateUser = IntegrationTestApplicationServiceFactory.Instance.Resolve<IImpersonateUser>();
            var emailService = new EmailerServiceStubb();
            var dataLakeFileService = new DataLakeFileService(emailService, impersonateUser);
            var fileName = "CNNAMPMBarterObligations_Clean.xlsx";

            var request = new FileRequest
            {
                StreamData = new FileStream(
                        @".\Files\CNNAMPMBarterObligations_Clean.xlsx",
                        FileMode.Open,
                        FileAccess.Read),
                FileName = fileName
            };

            dataLakeFileService.Save(request);

            var response = EmailerServiceStubb.LastMailMessageGenerated;

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
