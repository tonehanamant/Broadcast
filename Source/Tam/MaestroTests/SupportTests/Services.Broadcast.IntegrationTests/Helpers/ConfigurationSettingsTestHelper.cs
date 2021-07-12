using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.Helpers
{

    public class ConfigurationSettingsTestHelper
    {
        protected ConfigurationSettingsHelper _GetConfigurationSettingsHelper()
        {
            return new ConfigurationSettingsHelper();
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadConfigItems()
        {
            var tc = _GetConfigurationSettingsHelper();
            var toVerify = tc._LoadConfigItems(BroadcastConstants.CONFIG_FILE_NAME);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }
        [Test]
        [TestCase("AABCacheExpirationSeconds", 300, 300)]
        [TestCase("ABC", 1800, 1800)]
        public void GetConfigValuesFromKeys<T>(string key, T defaultValue, T expectedResult)
        {
            var tc = _GetConfigurationSettingsHelper();
            var result = tc.GetConfigValueWithDefault(key, defaultValue);
            Assert.AreEqual(result, expectedResult);
        }
        [Test]
        [TestCase("UnitTestingPurposeKey", "Key 'UnitTestingPurposeKey' found but of incorrect type : test is not a valid value for Int32.")]
        [TestCase("ABC", "The key 'ABC' doesn't exist")]
        public void GetConfigValuesFromKeys_WhenNoValuePresent(string key, string expectedResult)
        {
            var tc = _GetConfigurationSettingsHelper();
            var exception = Assert.Throws<InvalidOperationException>(() => tc.GetConfigValue<int>(key));
            Assert.AreEqual(exception.Message, expectedResult);
        }
    }
}
