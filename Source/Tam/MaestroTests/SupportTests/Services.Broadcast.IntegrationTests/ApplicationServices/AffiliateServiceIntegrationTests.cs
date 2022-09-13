using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class AffiliateServiceIntegrationTests
    {
        private IAffiliateService _AffiliateService;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
        private static IFeatureToggleHelper _FeatureToggleHelper;

        [SetUp]
        public void Setup()
        {
            _AffiliateService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffiliateService>();
            _LaunchDarklyClientStub = (LaunchDarklyClientStub)IntegrationTestApplicationServiceFactory.Instance.Resolve<ILaunchDarklyClient>();
            _SetFeatureToggle(FeatureToggles.ENABLE_STATION_SECONDARY_AFFILIATIONS, false);
            _FeatureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);
        }

        private void _SetFeatureToggle(string feature, bool activate)
        {
            if (_LaunchDarklyClientStub.FeatureToggles.ContainsKey(feature))
                _LaunchDarklyClientStub.FeatureToggles[feature] = activate;
            else
                _LaunchDarklyClientStub.FeatureToggles.Add(feature, activate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllAffiliates()
        {
            using (new TransactionScopeWrapper())
            {
                var affiliates = _AffiliateService.GetAllAffiliates();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(affiliates));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllAffiliates_WithtoggleOn()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_STATION_SECONDARY_AFFILIATIONS, true);
            using (new TransactionScopeWrapper())
            {
                var affiliates = _AffiliateService.GetAllAffiliates();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(affiliates));
            }
        }
    }
}
