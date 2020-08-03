using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.IntegrationTests.Stubs;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    public class EnvironmentServiceTests
    {
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
            var clientStub = new LaunchDarklyClientStub();
            clientStub.FeatureToggles.Add("TestToggle_Enabled", true);
            clientStub.FeatureToggles.Add("TestToggle_disabled", false);
            // do not add TestToggle_unknown

            // register our stub instance so it is used to instantiate the service
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ILaunchDarklyClient>(clientStub);

            var service = IntegrationTestApplicationServiceFactory.GetApplicationService<IEnvironmentService>();

            // Act
            var result = service.IsFeatureToggleEnabled(toggleKey, loggedInUser);

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
            // Arrange
            var clientStub = new LaunchDarklyClientStub();
            clientStub.FeatureToggles.Add("TestToggle_Enabled", true);
            clientStub.FeatureToggles.Add("TestToggle_disabled", false);

            // register our stub instance so it is used to instantiate the service
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ILaunchDarklyClient>(clientStub);
            // instantiate our test service
            var service = IntegrationTestApplicationServiceFactory.GetApplicationService<IEnvironmentService>();
            
            var result = service.IsFeatureToggleEnabledUserAnonymous(toggleKey);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}