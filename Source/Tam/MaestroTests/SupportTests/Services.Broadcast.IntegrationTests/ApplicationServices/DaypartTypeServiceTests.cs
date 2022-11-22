using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Helpers;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class DaypartTypeServiceTests
    {
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private IDaypartTypeService _DaypartTypeService;
        [SetUp]
        public void SetUp()
        {
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _DaypartTypeService = new DaypartTypeService(
               _FeatureToggleMock.Object
              );
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDaypartTypes()
        {
            using (new TransactionScopeWrapper())
            {
                var daypartTypes = _DaypartTypeService.GetDaypartTypes();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartTypes));
            }
        }
        [Test]
        [TestCase(true)]
        public void GetDaypartTypes_DaypartTypesToggle(bool toggleEnabled)
        {
            // Arrange
           
            int expectedCount = toggleEnabled ? 4 : 3;           
            var daypartTypes = _DaypartTypeService.GetDaypartTypes();

            Assert.AreEqual(daypartTypes.Count, expectedCount);
        }
    }
}