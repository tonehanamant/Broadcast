using NUnit.Framework;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    [Category("short_running")]
    public class EnumHelperUnitTests
    {
        #region GetEnum

        [Test]
        [TestCase(1, BackgroundJobProcessingStatus.Queued)]
        [TestCase(3, BackgroundJobProcessingStatus.Succeeded)]
        [TestCase(2, InventorySourceTypeEnum.Barter)]
        [TestCase(4, InventorySourceTypeEnum.Syndication)]
        public void GetEnumWithValidValue<T>(int candidate, T expectedResult)
        {
            var result = EnumHelper.GetEnum<T>(candidate);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetEnumWithInvalidType()
        {
            var candidate = 12;
            Exception caught = null;

            try
            {
                EnumHelper.GetEnum<String>(candidate);
            }
            catch (Exception e)
            {
                caught = e;
            }
            
            Assert.IsNotNull(caught);
            Assert.IsTrue(caught.Message.Contains("The type must be an Enum."));
        }

        [Test]
        public void GetEnumWithInvalidValue()
        {
            var candidate = -1;
            Exception caught = null;

            try
            {
                EnumHelper.GetEnum<BackgroundJobProcessingStatus>(candidate);
            }
            catch (Exception ex)
            {
                caught = ex;
            }
            
            Assert.IsNotNull(caught);
            Assert.IsTrue(caught.Message.Contains("Given status of -1 is an invalid Services.Broadcast.Entities.Enums.BackgroundJobProcessingStatus."));
        }

        #endregion // #region GetEnum
    }
}