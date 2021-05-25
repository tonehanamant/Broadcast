using NUnit.Framework;
using Services.Broadcast.Extensions;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    [TestFixture]
    public class NumericExtensionsUnitTests
    {
        [Test]
        [TestCase(1, 1)]
        [TestCase(1.12345678, 1.1235)]
        [TestCase(1.12344444, 1.1234)]
        public void Decimal_AsSqlTypeMoney(decimal input, decimal expectedResult)
        {
            decimal? typedInput = input;

            var result = typedInput.AsSqlTypeMoney();

            Assert.AreEqual(expectedResult, result.Value);
        }

        [Test]
        public void Decimal_AsSqlTypeMoneyWhenNull()
        {
            decimal? input = null;
            
            var result = input.AsSqlTypeMoney();

            Assert.IsNull(result);
        }
    }
}
