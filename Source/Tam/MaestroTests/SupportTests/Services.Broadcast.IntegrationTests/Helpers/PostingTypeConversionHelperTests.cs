using NUnit.Framework;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class PostingTypeConversionHelperTests
    {
        [Test]
        public void ConvertImpressionsFromNsiToNti()
        {
            var impressions = 2000;
            var conversionRate = 0.7;

            var result = PostingTypeConversionHelper.ConvertImpressionsFromNsiToNti(impressions, conversionRate);

            Assert.AreEqual(1400, result);
        }

        [Test]
        public void ConvertImpressionsFromNtiToNsi()
        {
            var impressions = 1500;
            var conversionRate = 0.5;

            var result = PostingTypeConversionHelper.ConvertImpressionsFromNtiToNsi(impressions, conversionRate);

            Assert.AreEqual(3000, result);
        }

        [Test]
        public void ConvertImpressions_WhenFromNtiToNsi()
        {
            var impressions = 1500;
            var conversionRate = 0.5;
            var sourcePostingType = PostingTypeEnum.NTI;

            var result = PostingTypeConversionHelper.ConvertImpressions(impressions, sourcePostingType, conversionRate);

            Assert.AreEqual(3000, result);
        }

        [Test]
        public void ConvertImpressions_WhenFromNsiToNti()
        {
            var impressions = 2000;
            var conversionRate = 0.7;
            var sourcePostingType = PostingTypeEnum.NSI;

            var result = PostingTypeConversionHelper.ConvertImpressions(impressions, sourcePostingType, conversionRate);

            Assert.AreEqual(1400, result);
        }
    }
}
