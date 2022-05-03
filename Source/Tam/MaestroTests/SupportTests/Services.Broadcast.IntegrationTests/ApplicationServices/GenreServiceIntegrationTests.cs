using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.IntegrationTests.Stubs;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class GenreServiceIntegrationTests
    {
        private const int MAESTRO_ID = (int)ProgramSourceEnum.Maestro;
        private const int REDBEE_ID = (int)ProgramSourceEnum.Master;
        private IGenreService _GenreService;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;

        [SetUp]
        public void Setup()
        {
            _LaunchDarklyClientStub = (LaunchDarklyClientStub)IntegrationTestApplicationServiceFactory.Instance.Resolve<ILaunchDarklyClient>();
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_VARIOUS_GENRE_RESTRICTION] = true;

            _GenreService  = IntegrationTestApplicationServiceFactory.GetApplicationService<IGenreService>();
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
        public void GetAllMaestroGenres()
        {
            using (new TransactionScopeWrapper())
            {
                var genres = _GenreService.GetGenres(MAESTRO_ID);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(genres));
            }
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void GetAllMaestroGenres_ToggleTest_EnableVariousGenreRestriction(bool toggleEnabled, bool shouldContainVarious)
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_VARIOUS_GENRE_RESTRICTION, toggleEnabled);

            using (new TransactionScopeWrapper())
            {
                var genres = _GenreService.GetGenres(MAESTRO_ID);

                var containsVarious = genres.Any(g => g.Display.Equals("Various"));
                Assert.AreEqual(shouldContainVarious, containsVarious);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDativaGenres()
        {
            using (new TransactionScopeWrapper())
            {
                var genres = _GenreService.GetGenres(REDBEE_ID);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(genres));
            }
        }

    }
}
