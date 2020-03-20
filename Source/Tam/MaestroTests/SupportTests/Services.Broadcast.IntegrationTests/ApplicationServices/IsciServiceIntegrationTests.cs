using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class IsciServiceIntegrationTests
    {
        private readonly IIsciService _IsciService;
        private const string _Username = "isci service test";

        public IsciServiceIntegrationTests()
        {
            _IsciService = IntegrationTestApplicationServiceFactory.GetApplicationService<IIsciService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciService_FindValidIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _IsciService.FindValidIscis(string.Empty);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciService_LoadIsciMappings()
        {
            using (new TransactionScopeWrapper())
            {
                ScrubbingFileDetail detail = new ScrubbingFileDetail()
                {
                    Isci = "BCOP-2631/A"
                };
                _IsciService.LoadIsciMappings(new List<ScrubbingFileDetail> { detail });

                Equals(detail.MappedIsci, "BCOP-2631/B");
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciService_RemoveIscisFromBlacklistTable()
        {
            using (new TransactionScopeWrapper())
            {                
                var result = _IsciService.RemoveIscisFromBlacklistTable(new List<string> { "BCOP-2631/A" });
                Assert.IsTrue(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciService_BlacklistIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _IsciService.BlacklistIscis(new List<string> { "BCOP-2631/A" }, _Username);
                Assert.IsTrue(result);
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciService_AddNewMapping()
        {
            using (new TransactionScopeWrapper())
            {                
                var result = _IsciService.AddIsciMapping(new MapIsciDto { EffectiveIsci = "BCOP-2632", OriginalIsci = "BCOP-26" }, _Username);
                Assert.IsTrue(result);
            }
        }
    }
}
