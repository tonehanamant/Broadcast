using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class TrafficServiceIntegrationTests
    {
        private readonly ITrafficService _TrafficService = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrafficService>();
        private readonly DateTime _CurrentDateTime = new DateTime(2016, 05, 30);

        [Ignore("Not certain why we are ignoring this...")]
        [Test]
        public void CanLoadTrafficWeeksProposals()
        {
            using (new TransactionScopeWrapper())
            {
                var trafficData = _TrafficService.GetTrafficProposals(_CurrentDateTime, null);

                Assert.IsTrue(trafficData.Weeks.Any());
            }
        }
    }
}
