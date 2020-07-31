using NUnit.Framework;
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    public class GeneralMathUnitTests
    {
        [Test]
        public void ConvertPercentageToFraction()
        {
            var result = GeneralMath.ConvertPercentageToFraction(83);

            Assert.AreEqual(0.83, result);
        }

        [Test]
        public void ConvertPercentageToFraction_DecimalPlaces()
        {
            var result = GeneralMath.ConvertPercentageToFraction(83.99);

            Assert.AreEqual(0.8399, result);
        }

        [Test]
        public void ConvertFractionToPercentage_DoubleValue()
        {
            var result = GeneralMath.ConvertFractionToPercentage(0.8399);

            Assert.AreEqual(83.99, result);
        }

        [Test]
        public void ConvertFractionToPercentage_DecimalValue()
        {
            var result = GeneralMath.ConvertFractionToPercentage(0.8399m);

            Assert.AreEqual(83.99m, result);
        }

        [Test]
        public void CalculateCostWithMargin_NullMargin()
        {
            var result = GeneralMath.CalculateCostWithMargin(1000, null);

            Assert.AreEqual(1000, result);
        }

        [Test]
        public void CalculateCostWithMargin_MarginWithValue()
        {
            var result = GeneralMath.CalculateCostWithMargin(1000, 20);

            Assert.AreEqual(1250, result);
        }

    }
}
