using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class QuarterDetailExtensionUnitTests
    {
        [Test]
        public void ShortFormat()
        {
            var result = QuarterDetailExtension.ShortFormat(_GetQuarterDetailDto());

            Assert.AreEqual("Q1'20", result);
        }

        [Test]
        public void ShortFormatQuarterNumberFirst()
        {
            var result = QuarterDetailExtension.ShortFormatQuarterNumberFirst(_GetQuarterDetailDto());

            Assert.AreEqual("1Q '20", result);
        }

        [Test]
        public void LongFormat()
        {
            var result = QuarterDetailExtension.LongFormat(_GetQuarterDetailDto());

            Assert.AreEqual("Q1 2020", result);
        }

        private QuarterDetailDto _GetQuarterDetailDto() =>
            new QuarterDetailDto
            {
                Year = 2020,
                EndDate = new DateTime(2020, 3, 31),
                StartDate = new DateTime(2020, 1, 1),
                Quarter = 1
            };
    }
}