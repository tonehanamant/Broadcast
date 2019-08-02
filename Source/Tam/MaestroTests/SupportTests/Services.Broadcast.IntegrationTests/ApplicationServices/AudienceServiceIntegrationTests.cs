using NUnit.Framework;
using ApprovalTests.Reporters;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;
using ApprovalTests;
using IntegrationTests.Common;
using Newtonsoft.Json;
using System;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AudienceServiceIntegrationTests
    {
        private readonly IAudienceService _AudienceService;

        public AudienceServiceIntegrationTests()
        {
            _AudienceService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAudienceService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAudienceTypes()
        {
            using (new TransactionScopeWrapper())
            {
                var audienceTypes = _AudienceService.GetAudienceTypes();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(audienceTypes));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                var audiences = _AudienceService.GetAudiences();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(audiences));
            }
        }
    }
}
