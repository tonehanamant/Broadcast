using FizzWare.NBuilder;
using NUnit.Framework;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class PlanPostingTypeHelperTest
    {
        [Test]
        public void GetNtiAndNsiPricingParametersTest_WhenPassedNTI_ReturnsBothNtiAndNsi()
        {
            //Arrange
            var ntiParameters = Builder<PlanPricingParametersDto>.CreateNew()
                .With(x => x.PostingType = PostingTypeEnum.NTI)
                .Build();

            var ntiToNsiConversionRate = .5d;

            var expectedImpressions = Math.Floor((double)ntiParameters.DeliveryImpressions / ntiToNsiConversionRate);

            //Act
            var result = PlanPostingTypeHelper.GetNtiAndNsiPricingParameters(ntiParameters, ntiToNsiConversionRate);

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(expectedImpressions, result[1].DeliveryImpressions);
        }

        [Test]
        public void GetNtiAndNsiPricingParametersTest_WhenPassedNSI_ReturnsBothNtiAndNsi()
        {
            //Arrange
            var nsiParameters = Builder<PlanPricingParametersDto>.CreateNew()
                .With(x => x.PostingType = PostingTypeEnum.NSI)
                .Build();

            var ntiToNsiConversionRate = .5d;

            var expectedImpressions = Math.Floor((double)nsiParameters.DeliveryImpressions * ntiToNsiConversionRate);

            //Act
            var result = PlanPostingTypeHelper.GetNtiAndNsiPricingParameters(nsiParameters, ntiToNsiConversionRate);

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(expectedImpressions, result[1].DeliveryImpressions);
        }
    }
}

