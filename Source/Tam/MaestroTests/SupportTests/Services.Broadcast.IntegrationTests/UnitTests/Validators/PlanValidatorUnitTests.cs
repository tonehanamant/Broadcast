using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.Validators
{
    [TestFixture]
    public class PlanValidatorUnitTests
    {
        #region Reusable values

        private readonly int _validSpotLengthId = 1;
        private readonly int _validProductId = 1;
        private readonly int _validShareBookId = 1;
        private readonly int _validAudienceId = 1;
        private readonly List<PlanDaypartDto> _validPlanDaypartList = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 1,
                        StartTimeSeconds = 1,
                        EndTimeSeconds = 10,
                        WeightingGoalPercent = 0.2
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 2,
                        StartTimeSeconds = 100,
                        EndTimeSeconds = 86399,
                        WeightingGoalPercent = 5.0
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 3,
                        StartTimeSeconds = 53000,
                        EndTimeSeconds = 67000,
                        WeightingGoalPercent = 100.0
                    }
                };
        #endregion

        [Test]
        public void ValidateSucces()
        {
            var item = new PlanDto
            {
                Id = 1,
                Name = "New Plan",
                ProductId = _validProductId,
                SpotLengthId = _validSpotLengthId,
                ShareBookId = _validShareBookId,
                AudienceId = _validAudienceId,
                Dayparts = _validPlanDaypartList
            };
            var sut = new PlanValidator(
                _GetMockSpotLengthEngine().Object,
                _GetMockBroadcastAudiencesCache().Object,
                _GetMockRatingForecastService().Object,
                _GetMockTrafficApiClient().Object);

            Assert.DoesNotThrow(() => sut.ValidatePlan(item));
        }

        [Test]
        [TestCase("", 1, 1, "Invalid plan name")]
        [TestCase("Some Plan", 2, 1, "Invalid spot length")]
        [TestCase("Some Plan", 1, 0, "Invalid product")]
        public void ValidateFailure(string planName, int spotLengthId, int productId, string expectedMessage)
        {
            var item = new PlanDto
            {
                Id = 1,
                Name = planName,
                SpotLengthId = spotLengthId,
                ProductId = productId,
                ShareBookId = _validShareBookId,
                AudienceId = _validAudienceId,
                Dayparts = _validPlanDaypartList
            };
            var sut = new PlanValidator(
                _GetMockSpotLengthEngine().Object,
                _GetMockBroadcastAudiencesCache().Object,
                _GetMockRatingForecastService().Object,
                _GetMockTrafficApiClient().Object);

            var caughtException = Assert.Throws<Exception>(() => sut.ValidatePlan(item));

            Assert.AreEqual(expectedMessage, caughtException.Message);
        }

        [Test]
        public void ValidatePlanDaypartLength()
        {
            var item = new PlanDto
            {
                Id = 1,
                Name = "New Plan",
                ProductId = _validProductId,
                SpotLengthId = _validSpotLengthId,
                ShareBookId = _validShareBookId,
                AudienceId = _validAudienceId
            };
            var sut = new PlanValidator(
                _GetMockSpotLengthEngine().Object,
                _GetMockBroadcastAudiencesCache().Object,
                _GetMockRatingForecastService().Object,
                _GetMockTrafficApiClient().Object);

            var caughtException = Assert.Throws<Exception>(() => sut.ValidatePlan(item));

            Assert.AreEqual("There should be at least one daypart selected.", caughtException.Message);
        }

        [Test]
        [TestCase(5000, 6000, 1.0, false, null)]
        [TestCase(-1, 5000, 1.0, true, "Invalid daypart times.")]
        [TestCase(5000, 86401, 1.0, true, "Invalid daypart times.")]
        [TestCase(86401, 6000, 1.0, true, "Invalid daypart times.")]
        [TestCase(5000, -1, 1.0, true, "Invalid daypart times.")]
        [TestCase(5000, 6000, 0.0, true, "Invalid daypart weighting goal.")]
        [TestCase(5000, 6000, 101.0, true, "Invalid daypart weighting goal.")]
        public void ValidatePlanDaypart(int startTimeSeconds, int endTimeSeconds, double minWeightingGoalPercent, bool shouldThrow, string expectedMessage)
        {
            var item = new PlanDto
            {
                Id = 1,
                Name = "New Plan",
                ProductId = _validProductId,
                SpotLengthId = _validSpotLengthId,
                ShareBookId = _validShareBookId,
                AudienceId = _validAudienceId,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 1,
                        StartTimeSeconds = startTimeSeconds,
                        EndTimeSeconds = endTimeSeconds,
                        WeightingGoalPercent = minWeightingGoalPercent
                    }
                }
            };
            var sut = new PlanValidator(
                _GetMockSpotLengthEngine().Object,
                _GetMockBroadcastAudiencesCache().Object,
                _GetMockRatingForecastService().Object,
                _GetMockTrafficApiClient().Object);

            if (shouldThrow)
            {
                var caughtException = Assert.Throws<Exception>(() => sut.ValidatePlan(item));
                Assert.AreEqual(expectedMessage, caughtException.Message);
            }
            else
            {
                Assert.DoesNotThrow(() => sut.ValidatePlan(item));
            }
        }

        [Test]
        [TestCase(255, false, null)]
        [TestCase(256, true, "Invalid plan name")]
        public void ValidatePlanNameBounds(int planNameLength, bool shouldThrow, string expectedMessage)
        {
            var item = new PlanDto
            {
                Id = 1,
                Name = StringHelper.CreateStringOfLength(planNameLength),
                SpotLengthId = _validSpotLengthId,
                ProductId = _validProductId,
                ShareBookId = _validShareBookId,
                AudienceId = _validAudienceId,
                Dayparts = _validPlanDaypartList
            };
            var sut = new PlanValidator(
                _GetMockSpotLengthEngine().Object,
                _GetMockBroadcastAudiencesCache().Object,
                _GetMockRatingForecastService().Object,
                _GetMockTrafficApiClient().Object);

            if (shouldThrow)
            {
                var caughtException = Assert.Throws<Exception>(() => sut.ValidatePlan(item));
                Assert.AreEqual(expectedMessage, caughtException.Message);
            }
            else
            {
                Assert.DoesNotThrow(() => sut.ValidatePlan(item));
            }
        }

        #region Mock dependencies

        private Mock<ISpotLengthEngine> _GetMockSpotLengthEngine()
        {
            var mockSpotLengthEngine = new Mock<ISpotLengthEngine>();
            mockSpotLengthEngine.Setup(x => x.SpotLengthIdExists(_validSpotLengthId)).Returns(true);
            return mockSpotLengthEngine;
        }

        private Mock<IBroadcastAudiencesCache> _GetMockBroadcastAudiencesCache()
        {
            var mockBroadcastAudiencesCache = new Mock<IBroadcastAudiencesCache>();
            mockBroadcastAudiencesCache.Setup(x => x.IsValidAudience(It.IsAny<int>())).Returns(true);
            return mockBroadcastAudiencesCache;
        }

        private Mock<IRatingForecastService> _GetMockRatingForecastService()
        {
            var mockRatingForecastService = new Mock<IRatingForecastService>();
            var ratingsForecastStatus = new RatingsForecastStatus
            {
                MediaMonth = new Tam.Maestro.Data.Entities.MediaMonth
                {
                    Id = _validShareBookId
                },
                UniverseMarkets = 1,
                UsageMarkets = 1,
                ViewerMarkets = 1,
            };
            mockRatingForecastService
                .Setup(x => x.GetMediaMonthCrunchStatuses())
                .Returns(
                    () => new List<MediaMonthCrunchStatus>
                    {
                        new MediaMonthCrunchStatus(ratingsForecastStatus, 1)
                    });
            return mockRatingForecastService;
        }

        private Mock<ITrafficApiClient> _GetMockTrafficApiClient()
        {
            var mockTrafficApiClient = new Mock<ITrafficApiClient>();
            return mockTrafficApiClient;
        }
        #endregion
    }
}
