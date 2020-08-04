using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.Converters.RateImport;

namespace Services.Broadcast.IntegrationTests.UnitTests.Converters.RateImport
{
    [TestFixture]
    public class BarterFileImporterUnitTests
    {
        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("WCBS-tv", "WCBS")]
        [TestCase("WCBS", "WCBS")]
        [TestCase("WCBS-TV", "WCBS")]
        [TestCase("WCBS-TV2", "WCBS")]
        [TestCase("WCBS-TV 123.4", "WCBS")]
        [TestCase("WCBS-DT", "WCBS")]
        [TestCase("WCBS-DT2", "WCBS")]
        [TestCase("WCBS-DT 123.4", "WCBS")]
        [TestCase("WCBS-LD", "WCBS")]
        [TestCase("WCBS-LD2", "WCBS")]
        [TestCase("WCBS-LD 123.4", "WCBS")]
        [TestCase("WCBS-CD", "WCBS")]
        [TestCase("WCBS-CD2", "WCBS")]
        [TestCase("WCBS-CD 123.4", "WCBS")]
        [TestCase("WCBS-LPTV", "WCBS")]
        [TestCase("WCBS-LPTV2", "WCBS")]
        [TestCase("WCBS-LPTV 123.4", "WCBS")]
        public void TransformStationCallsign(string rawCallsign, string expectedResult)
        {
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();

            var fileImporter = new BarterFileImporter(
                dataRepositoryFactory.Object, null, null,
                null, null, null,
                null, null, null,
                null);

            var result = fileImporter._TransformStationCallsign(rawCallsign);

            if (expectedResult == null)
            {
                Assert.IsNull(result);
            }
            else
            {
                Assert.AreEqual(expectedResult, result);
            }
        }
    }
}