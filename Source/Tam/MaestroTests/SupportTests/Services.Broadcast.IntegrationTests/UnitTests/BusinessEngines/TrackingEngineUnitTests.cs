using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    public class TrackingEngineUnitTests
    {
        private TrackingEngine _TrackingEngine;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IDaypartCache> _DaypartCacheMock;
        private Mock<IDetectionRepository> _DetectionRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _DetectionRepositoryMock = new Mock<IDetectionRepository>();
            _DataRepositoryFactoryMock.Setup(s => s.GetDataRepository<IDetectionRepository>()).Returns(_DetectionRepositoryMock.Object);
            _DaypartCacheMock = new Mock<IDaypartCache>();

            var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
            SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);

            _TrackingEngine = new TrackingEngine(_DataRepositoryFactoryMock.Object, _DaypartCacheMock.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_EmptyProgramNull()
        {
            _DetectionRepositoryMock.Setup(b => b.GetDetectionTrackingDetailById(It.IsAny<int>())).Returns(new Entities.DetectionTrackingDetail());

            var programMapping = _TrackingEngine.GetProgramMappingDto(15350);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programMapping));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_EmptyProgramEmpty()
        {
            _DetectionRepositoryMock.Setup(b => b.GetDetectionTrackingDetailById(It.IsAny<int>())).Returns(new Entities.DetectionTrackingDetail { Program = string.Empty});

            var programMapping = _TrackingEngine.GetProgramMappingDto(15350);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programMapping));
        }
    }
}
