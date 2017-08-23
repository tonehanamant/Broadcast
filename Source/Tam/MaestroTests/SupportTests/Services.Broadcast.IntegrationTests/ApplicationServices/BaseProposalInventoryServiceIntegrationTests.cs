using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class BaseProposalInventoryServiceTests
    {
        private readonly BaseProposalInventoryService _Sut = new BaseProposalInventoryService(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory, null, null);

        [Test]
        public void ApplyPostTypeConversion_DoesNothing_WhenPostType_Null()
        {
            var impression = new StationImpressions { impressions = 100 };
            var impressions = new List<StationImpressions> { impression };
            var dto = new ProposalDetailInventoryBase();
            dto.PostType = null;
            _Sut.ApplyPostTypeConversion(impressions, dto);

            Assert.That(impression.impressions, Is.EqualTo(100));
        }

        [Test]
        public void ApplyPostTypeConversion_DoesNothing_WhenPostType_NotNTI()
        {
            var impression = new StationImpressions { impressions = 100 };
            var impressions = new List<StationImpressions> { impression };
            var dto = new ProposalDetailInventoryBase();
            dto.PostType = SchedulePostType.NSI;
            _Sut.ApplyPostTypeConversion(impressions, dto);

            Assert.That(impression.impressions, Is.EqualTo(100));
        }

        [Test]
        public void ApplyPostTypeConversion_DoesNothing_Null_Adjustment()
        {
            var impression = new StationImpressions { impressions = 100 };
            var impressions = new List<StationImpressions> { impression };
            var dto = new ProposalDetailInventoryBase();
            dto.PostType = SchedulePostType.NTI;
            dto.HutPostingBookId = 420;

            var repo = new Mock<IRatingAdjustmentsRepository>();
            repo.Setup(r => r.GetRatingAdjustment(dto.HutPostingBookId.Value)).Returns((RatingAdjustmentsDto)null);
            using (new RepositoryMock<IRatingAdjustmentsRepository>(repo))
            {
                var sut = new BaseProposalInventoryService(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory, null, null);
                sut.ApplyPostTypeConversion(impressions, dto);

                Assert.That(impression.impressions, Is.EqualTo(100));
            }
        }

        [Test]
        public void ApplyPostTypeConversion_HasShareAndHut_UsesHut()
        {
            const int origImpression = 100;
            var impression = new StationImpressions { impressions = origImpression };
            var impressions = new List<StationImpressions> { impression };
            var dto = new ProposalDetailInventoryBase();
            dto.HutPostingBookId = 420;
            dto.SharePostingBookId = 322;

            dto.PostType = SchedulePostType.NTI;

            const decimal adjustment = 10m;
            var repo = new Mock<IRatingAdjustmentsRepository>();
            repo.Setup(r => r.GetRatingAdjustment(dto.HutPostingBookId.Value)).Returns(new RatingAdjustmentsDto { MediaMonthId = dto.SharePostingBookId.Value, NtiAdjustment = adjustment });

            using (new RepositoryMock<IRatingAdjustmentsRepository>(repo))
            {
                var sut = new BaseProposalInventoryService(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory, null, null);
                sut.ApplyPostTypeConversion(impressions, dto);

                Assert.That(impression.impressions, Is.EqualTo(origImpression * (double)(1 - adjustment / 100)));
            }
        }

        [Test]
        public void ApplyPostTypeConversion_HasSweeps_UsesSweeps()
        {
            const int origImpression = 100;
            var impression = new StationImpressions { impressions = origImpression };
            var impressions = new List<StationImpressions> { impression };
            var dto = new ProposalDetailInventoryBase();
            dto.SinglePostingBookId = 420;

            dto.PostType = SchedulePostType.NTI;

            const decimal adjustment = 10m;
            var repo = new Mock<IRatingAdjustmentsRepository>();
            repo.Setup(r => r.GetRatingAdjustment(dto.SinglePostingBookId.Value)).Returns(new RatingAdjustmentsDto { NtiAdjustment = adjustment });

            using (new RepositoryMock<IRatingAdjustmentsRepository>(repo))
            {
                var sut = new BaseProposalInventoryService(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory, null, null);
                sut.ApplyPostTypeConversion(impressions, dto);

                Assert.That(impression.impressions, Is.EqualTo(origImpression * (double)(1 - adjustment / 100)));
            }
        }
    }
}
