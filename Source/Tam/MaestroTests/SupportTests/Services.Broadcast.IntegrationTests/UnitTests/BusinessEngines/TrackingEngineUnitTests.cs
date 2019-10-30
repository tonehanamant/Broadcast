using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubbs;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class TrackingEngineUnitTests
    {
        private TrackingEngine _TrackingEngine;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IDaypartCache> _DaypartCacheMock;
        private Mock<IBvsRepository> _BvsRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _BvsRepositoryMock = new Mock<IBvsRepository>();
            _DataRepositoryFactoryMock.Setup(s => s.GetDataRepository<IBvsRepository>()).Returns(_BvsRepositoryMock.Object);
            _DaypartCacheMock = new Mock<IDaypartCache>();

            var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
            SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);

            _TrackingEngine = new TrackingEngine(_DataRepositoryFactoryMock.Object, _DaypartCacheMock.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_EmptyProgramNull()
        {
            _BvsRepositoryMock.Setup(b => b.GetBvsTrackingDetailById(It.IsAny<int>())).Returns(new Entities.BvsTrackingDetail());

            var programMapping = _TrackingEngine.GetProgramMappingDto(15350);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programMapping));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_EmptyProgramEmpty()
        {
            _BvsRepositoryMock.Setup(b => b.GetBvsTrackingDetailById(It.IsAny<int>())).Returns(new Entities.BvsTrackingDetail { Program = string.Empty});

            var programMapping = _TrackingEngine.GetProgramMappingDto(15350);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programMapping));
        }
    }
}
