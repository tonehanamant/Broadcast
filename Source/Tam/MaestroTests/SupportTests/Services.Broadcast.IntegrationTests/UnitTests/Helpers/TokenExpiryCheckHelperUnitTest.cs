using NUnit.Framework;
using Services.Broadcast.Helpers;
using System;
namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    public class TokenExpiryCheckHelperUnitTest
    {
        [Test]
        [TestCase(20, true)]
        [TestCase(40, false)]      
        [TestCase(30, true)]
        public void HasTokenExpired_True(int expirationMinutes, bool expectedFlag)
        {
            DateTime?  expirationDate = new DateTime(2012, 12, 25, 10, expirationMinutes, 40);
            DateTime currentDate = new DateTime(2012, 12, 25, 10, 30, 50);          
            var result = TokenExpiryCheckHelper.HasTokenExpired(expirationDate, currentDate);
            Assert.AreEqual(result, expectedFlag);         
        }       
        [Test]
        public void HasTokenExpired_WhenExpiryDateNull()
        {
            DateTime? expirationDate = null;
            DateTime currentDate = new DateTime(2012, 12, 25, 10, 30, 50);
            bool expectedFlag = false;
            var result = TokenExpiryCheckHelper.HasTokenExpired(expirationDate, currentDate);
            Assert.AreEqual(result, expectedFlag);
        }      
    }
}
