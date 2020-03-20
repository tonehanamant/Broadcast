using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class GenreServiceIntegrationTests
    {
        private const int MAESTRO_ID = (int)GenreSourceEnum.Maestro;
        private const int DATIVA_ID = (int)GenreSourceEnum.Dativa;
        private IGenreService _GenreService;

        [SetUp]
        public void Setup()
        {
            _GenreService  = IntegrationTestApplicationServiceFactory.GetApplicationService<IGenreService>();
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
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDativaGenres()
        {
            using (new TransactionScopeWrapper())
            {
                var genres = _GenreService.GetGenres(DATIVA_ID);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(genres));
            }

        }

    }
}
