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
    [Category("short_running")]
    public class SpotLengthServiceIntegrationTests
    {
        private readonly ISpotLengthService _SpotLengthService;

        public SpotLengthServiceIntegrationTests()
        {
            _SpotLengthService = IntegrationTestApplicationServiceFactory.GetApplicationService<ISpotLengthService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAll()
        {
            using (new TransactionScopeWrapper())
            {
                var spotLengths = _SpotLengthService.GetAllSpotLengths();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(spotLengths));
            }
        }
    }
}
