using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class ImpressionAdjustmentEngineIntegrationTests
    {
        [Test]
        public void AdjustImpression_DoesNothing_WhenNoRatingAdjustmentForBook()
        {
            var repo = new Mock<IRatingAdjustmentsRepository>();
            repo.Setup(r => r.GetRatingAdjustments()).Returns(new List<RatingAdjustmentsDto>());

            using (new RepositoryMock<IRatingAdjustmentsRepository>(repo))
            {
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionAdjustmentEngine>();
                const long impression = 100;

                var result = sut.AdjustImpression(impression, PostingTypeEnum.NTI, 420);

                Assert.That(result, Is.EqualTo(impression));
            }
        }

        [Test]
        public void AdjustImpression_AppliesAnnualAdjustment()
        {
            var dto = new RatingAdjustmentsDto { MediaMonthId = 420, AnnualAdjustment = 10 };

            var repo = new Mock<IRatingAdjustmentsRepository>();

            repo.Setup(r => r.GetRatingAdjustments()).Returns(new List<RatingAdjustmentsDto> { dto });

            using (new RepositoryMock<IRatingAdjustmentsRepository>(repo))
            {
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionAdjustmentEngine>();
                const long impression = 100;

                var result = sut.AdjustImpression(impression, PostingTypeEnum.NTI, dto.MediaMonthId);

                Assert.That(result, Is.EqualTo(impression * (double)(1 - dto.AnnualAdjustment / 100)));
            }
        }

        [Test]
        public void AdjustImpression_AppliesNTIAdjustment_WhenPostType_IsNTI()
        {
            var dto = new RatingAdjustmentsDto { MediaMonthId = 420, NtiAdjustment = 10 };

            var repo = new Mock<IRatingAdjustmentsRepository>();

            repo.Setup(r => r.GetRatingAdjustments()).Returns(new List<RatingAdjustmentsDto> { dto });

            using (new RepositoryMock<IRatingAdjustmentsRepository>(repo))
            {
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionAdjustmentEngine>();
                const long impression = 100;

                var result = sut.AdjustImpression(impression, PostingTypeEnum.NTI, dto.MediaMonthId, false);

                Assert.That(result, Is.EqualTo(impression * (double)(1 - dto.NtiAdjustment / 100)));
            }
        }

        [Test]
        public void AdjustImpression_Applies_AnnualAdjustment_And_NTIAdjustment_WhenPostType_IsNTI()
        {
            var dto = new RatingAdjustmentsDto { MediaMonthId = 420, NtiAdjustment = 35, AnnualAdjustment = 23 };

            var repo = new Mock<IRatingAdjustmentsRepository>();

            repo.Setup(r => r.GetRatingAdjustments()).Returns(new List<RatingAdjustmentsDto> { dto });

            using (new RepositoryMock<IRatingAdjustmentsRepository>(repo))
            {
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionAdjustmentEngine>();
                const long impression = 100;

                var result = sut.AdjustImpression(impression, PostingTypeEnum.NTI, dto.MediaMonthId);

                Assert.That(result, Is.EqualTo(50.050000000000004d));
            }
        }

        [Test]
        public void AdjustImpression_Applies_Equivilization()
        {
            var repo = new Mock<ISpotLengthRepository>();

            const int multiplier = 30;
            const int spotLength = 15;
            repo.Setup(r => r.GetDeliveryMultipliersBySpotLength()).Returns(new Dictionary<int, double> { { spotLength, multiplier } });

            using (new RepositoryMock<ISpotLengthRepository>(repo))
            {
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionAdjustmentEngine>();
                const long impression = 100;

                var result = sut.AdjustImpression(impression, true, spotLength, null, 420);

                Assert.That(result, Is.EqualTo(impression * multiplier));
            }
        }

        [Test]
        public void AdjustImpression_DoesNot_Applies_Equivilization_WhenFalse()
        {
            var repo = new Mock<ISpotLengthRepository>();

            const int multiplier = 30;
            const int spotLength = 15;
            repo.Setup(r => r.GetDeliveryMultipliersBySpotLength()).Returns(new Dictionary<int, double> { { spotLength, multiplier } });

            using (new RepositoryMock<ISpotLengthRepository>(repo))
            {
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionAdjustmentEngine>();
                const long impression = 100;

                var result = sut.AdjustImpression(impression, false, spotLength, null, 420);

                Assert.That(result, Is.EqualTo(impression));
            }
        }
    }
}
