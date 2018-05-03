using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class WhosWatchingTvServiceTests
    {
        private IWhosWatchingTvService _WhosWatchingTvService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IWhosWatchingTvService>();

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void FindSomething()
        {
            var result = _WhosWatchingTvService.FindPrograms("half");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));

        }

        /*
         This requires a good deal of configurations to get running.  
         Some, but not all, system settings include
            WWTV_FtpHost
            WWTV_FtpInboundFolder
            WWTV_FtpOutboundFolder
            WWTV_FtpPassword
            WWTV_FtpUsername
            WWTV_LocalFtpErrorFolder
            WWTV_SharedFolder

        so make sure you check the current values in StubbedSMSClient.cs fild

        you may want to use the following in the test's EnvornmentSPecificSettings.Config

            <add key="SMS_Port" value="9999" />
            <add key="SMS_Host" value="cadapps-qa1.crossmw.com" />
            <add key="SMS_Name" value="SMS" />

            <!--Environment setting DEV, QA, PROP, PROD, LOCAL-->
            <add key="TAMEnvironment" value="QA" />
            <add key="Environment" value="Development" />

         */
        [Test]
        [Ignore]
        public void Test_WWTVDownload()
        {
            var service = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitPostProcessingService>();
            List<string> filesFailedDownload;
            service.DownloadAndProcessWWTVFiles("WWTV Service", out filesFailedDownload);
        }
    }
}
