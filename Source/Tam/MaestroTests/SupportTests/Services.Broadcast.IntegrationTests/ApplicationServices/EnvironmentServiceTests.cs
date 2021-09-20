using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.IntegrationTests.Stubs;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    public class EnvironmentServiceTests
    {
        public IEnvironmentService _EnvironmentService;
        [SetUp]
        public void Setup()
        {
            var launchDarklyClientStub = (LaunchDarklyClientStub)IntegrationTestApplicationServiceFactory.Instance.Resolve<ILaunchDarklyClient>();
            launchDarklyClientStub.FeatureToggles["TestToggle_Enabled"] = true;
            launchDarklyClientStub.FeatureToggles["TestToggle_disabled"] = false;
            // do not add TestToggle_unknown

            _EnvironmentService = IntegrationTestApplicationServiceFactory.GetApplicationService<IEnvironmentService>();
        }

        /// <summary>
        /// Verifies that the code through the helper works.
        /// </summary>
        /// <param name="loggedInUser"></param>
        /// <param name="toggleKey"></param>
        /// <param name="expectedResult"></param>
        [Test]
        [TestCase("TestToggle_Enabled", true)]
        [TestCase("TestToggle_disabled", false)]
        [TestCase("TestToggle_unknown", false)]
        public void FeatureToggleTestAnonymousUser(string toggleKey, bool expectedResult)
        {
            // Arrange
            const string loggedInUser = "testUser@cadent.tv";

            // Act
            var result = _EnvironmentService.IsFeatureToggleEnabled(toggleKey, loggedInUser);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        /// <summary>
        /// Verifies that the code through the helper works.
        /// </summary>
        /// <param name="loggedInUser"></param>
        /// <param name="toggleKey"></param>
        /// <param name="expectedResult"></param>
        [Test]
        [TestCase("TestToggle_Enabled", true)]
        [TestCase("TestToggle_disabled", false)]
        [TestCase("TestToggle_unknown", false)]
        public void IsFeatureToggleEnabledUserAnonymous(string toggleKey, bool expectedResult)
        {
            // Act
            var result = _EnvironmentService.IsFeatureToggleEnabledUserAnonymous(toggleKey);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}