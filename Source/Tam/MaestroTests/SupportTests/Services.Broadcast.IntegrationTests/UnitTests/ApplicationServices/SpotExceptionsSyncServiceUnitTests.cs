using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionsSyncServiceUnitTests
    {
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;        
        private Mock<IFeatureToggleHelper> _FeatureToggleHelperMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        private Mock<IBackgroundJobClient> _BackgroundJobClientMock;
        private Mock<ISpotExceptionsIngestJobRepository> _SpotExceptionsIngestJobRepository;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<ISpotExceptionsIngestApiClient> _ApiClient;

        protected SpotExceptionsSyncService _GetService()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _FeatureToggleHelperMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _BackgroundJobClientMock = new Mock<IBackgroundJobClient>();
            _SpotExceptionsIngestJobRepository = new Mock<ISpotExceptionsIngestJobRepository>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _ApiClient = new Mock<ISpotExceptionsIngestApiClient>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsIngestJobRepository>())
                .Returns(_SpotExceptionsIngestJobRepository.Object);

            var service = new SpotExceptionsSyncService(
                _DataRepositoryFactoryMock.Object,
                _BackgroundJobClientMock.Object,
                _DateTimeEngineMock.Object,
                _ApiClient.Object,
                _FeatureToggleHelperMock.Object,
                _ConfigurationSettingsHelperMock.Object
                );

            return service;
        }

        [Test]
        public async Task TriggerIngestForWeekAsync()
        {
            // Arrange
            const string username = "test-user";
            const bool runInBackground = false;
            const int jobId = 23;
            var service = _GetService();
            const string dateformat = "yyyy-MM-dd";

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(new DateTime(2022, 6, 7, 15, 27, 32));

            var expectedStartDateString = "2022-05-30";
            var expectedEndDateString = "2022-06-05";

            var apiRequests = new List<IngestApiRequest>();
            _ApiClient.Setup(s => s.IngestAsync(It.IsAny<IngestApiRequest>()))
                .Callback<IngestApiRequest>(r => apiRequests.Add(r))
                .Returns(Task.FromResult(new IngestApiResponse { JobId = jobId }));

            var request = new SpotExceptionsIngestTriggerRequest
            {
                Username = username,
                RunInBackground = runInBackground
            };

            // Act
            var result = await service.TriggerIngestForWeekAsync(request);

            // Assert
            _ApiClient.Verify(s => s.IngestAsync(It.IsAny<IngestApiRequest>()), Times.Once);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Request.RequestId.HasValue);
            Assert.AreEqual(expectedStartDateString, apiRequests.First().StartDate.ToString(dateformat));
            Assert.AreEqual(expectedEndDateString, apiRequests.First().EndDate.ToString(dateformat));
        }

        [Test]
        public async Task TriggerIngestForWeekAsyncInBackground()
        {
            // Arrange
            const string username = "test-user";
            const bool runInBackground = true;
            var service = _GetService();

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(new DateTime(2022, 6, 7, 15, 27, 32));

            var passedParameters = new List<object>();
            _BackgroundJobClientMock
                .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
                .Callback<Job, IState>((job, state) => passedParameters.Add(new { job, state }))
                .Returns("hangfire job 35");

            var request = new SpotExceptionsIngestTriggerRequest
            {
                RequestId = new Guid("1469691c-cf2b-4f66-a210-f51cda7b142d"),
                Username = username,
                RunInBackground = runInBackground
            };

            // Act
            var result = await service.TriggerIngestForWeekAsync(request);

            // Assert
            _ApiClient.Verify(s => s.IngestAsync(It.IsAny<IngestApiRequest>()), Times.Never);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Request.RequestId.HasValue);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(Job), "Type");
            jsonResolver.Ignore(typeof(Job), "Method");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters, settings));
        }

        [Test]
        public async Task TriggerIngestForDateRangeAsync()
        {
            // Arrange
            const string username = "test-user";
            const bool runInBackground = false;
            const int jobId = 23;
            var service = _GetService();

            var apiRequests = new List<IngestApiRequest>();
            _ApiClient.Setup(s => s.IngestAsync(It.IsAny<IngestApiRequest>()))
                .Callback<IngestApiRequest>(r => apiRequests.Add(r))
                .Returns(Task.FromResult(new IngestApiResponse { JobId = jobId }));

            var request = new SpotExceptionsIngestTriggerRequest
            {
                RequestId = new Guid("1469691c-cf2b-4f66-a210-f51cda7b142d"),
                Username = username,
                RunInBackground = runInBackground,
                StartDate = new DateTime(2022, 05, 30),
                EndDate = new DateTime(2022, 06, 16)
            };

            // Act
            var result = await service.TriggerIngestForDateRangeAsync(request);

            // Assert
            _ApiClient.Verify(s => s.IngestAsync(It.IsAny<IngestApiRequest>()), Times.Exactly(3));
            var toVerify = new
            {
                result,
                apiRequests
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void SetJobToError()
        {
            // Arrange
            const string username = "test-user";
            const int jobId = 23;
            var expectedResult = $"Successfully set JobId '{jobId}' to error state.";

            var service = _GetService();

            // Action
            var result = service.SetJobToError(username, jobId);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _SpotExceptionsIngestJobRepository.Verify(s => s.SetJobToError(jobId, username, It.IsAny<DateTime>()), Times.Once);
        }
    }
}
