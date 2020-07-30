using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Clients;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;

namespace Services.Broadcast.IntegrationTests
{
    [TestFixture]
    public class FeatureToggleHelperTests
    {
        /// <summary>
        /// Verifies that the code through the helper works.
        /// </summary>
        /// <param name="loggedInUser"></param>
        /// <param name="toggleKey"></param>
        /// <param name="expectedResult"></param>
        [Test]
        [TestCase("TestUser@cadent.tv", "TestToggle_Enabled", true)]
        [TestCase("TestUser@cadent.tv", "TestToggle_disabled", false)]
        [TestCase("TestUser@cadent.tv", "TestToggle_unknown", false)]
        // username should be in email format
        [TestCase("TestUser", "TestToggle_Enabled", false)]
        // username should not be blank
        [TestCase("", "TestToggle_Enabled", false)]
        public void FeatureToggleTest(string loggedInUser, string toggleKey, bool expectedResult)
        {
            // Arrange
            var clientStub = new LaunchDarklyClientStub();
            clientStub.FeatureToggles.Add("TestToggle_Enabled", true);
            clientStub.FeatureToggles.Add("TestToggle_disabled", false);
            // do not add TestToggle_unknown
            
            var featureToggleHelper = new FeatureToggleHelper(clientStub);

            // Act
            var result = featureToggleHelper.IsToggleEnabled(toggleKey, loggedInUser);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        /// <summary>
        /// Verifies that the code handles an error.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FeatureToggleTest_WithError()
        {
            // Arrange
            const string toggleKey = "testToggle";
            const string loggedInUser = "TestUser";

            var clientStub = new LaunchDarklyClientStub {ThrowError = true};

            var featureToggleHelper = new FeatureToggleHelperTestClass(clientStub);

            // Act
            var result = featureToggleHelper.IsToggleEnabled(toggleKey, loggedInUser);

            // Assert
            var toValidate = new
            {
                result,
                ErrorMessages = featureToggleHelper.ErrorMessages
                    .Select(s => $"message='{s.Item1}' | exception.Message='{s.Item2.Message}' | memberName='{s.Item3}'")
                    .ToList()
            };
                
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate));
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
            var clientStub = new LaunchDarklyClientStub();
            clientStub.FeatureToggles.Add("TestToggle_Enabled", true);
            clientStub.FeatureToggles.Add("TestToggle_disabled", false);
            // do not add TestToggle_unknown

            var featureToggleHelper = new FeatureToggleHelper(clientStub);

            // Act
            var result = featureToggleHelper.IsToggleEnabledUserAnonymous(toggleKey);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        /// <summary>
        /// Verifies that the code handles an error.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FeatureToggleTestAnonymousUser_WithError()
        {
            // Arrange
            const string toggleKey = "testToggle";

            var clientStub = new LaunchDarklyClientStub { ThrowError = true };

            var featureToggleHelper = new FeatureToggleHelperTestClass(clientStub);

            // Act
            var result = featureToggleHelper.IsToggleEnabledUserAnonymous(toggleKey);

            // Assert
            var toValidate = new
            {
                result,
                ErrorMessages = featureToggleHelper.ErrorMessages
                    .Select(s => $"message='{s.Item1}' | exception.Message='{s.Item2.Message}' | memberName='{s.Item3}'")
                    .ToList()
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate));
        }

        [Test]
        public void FeatureToggleTestAuthenticateUser()
        {
            // Arrange
            const string username = "testUser@cadent.tv";
            const string clientHash = "testClientHash";

            var clientStub = new LaunchDarklyClientStub {AuthenticatedClientHash = clientHash};
            var featureToggleHelper = new FeatureToggleHelperTestClass(clientStub);

            // Act
            var result = featureToggleHelper.Authenticate(username);

            // Assert
            Assert.AreEqual(clientHash, result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FeatureToggleTestAuthenticateUser_WithError()
        {
            // Arrange
            const string username = "testUser@cadent.tv";

            var clientStub = new LaunchDarklyClientStub { ThrowAuthenticationError = true};
            var featureToggleHelper = new FeatureToggleHelperTestClass(clientStub);

            // Act
            var caught = Assert.Throws<InvalidOperationException>(() => featureToggleHelper.Authenticate(username));

            // Assert
            Assert.IsNotNull(caught);
            Assert.IsNotNull(caught.InnerException);

            var toValidate = new
            {
                ExceptionMessage = caught.Message,
                InnerExceptionMessage = caught.InnerException.Message,
                ErrorMessages = featureToggleHelper.ErrorMessages
                    .Select(s => $"message='{s.Item1}' | exception.Message='{s.Item2.Message}' | memberName='{s.Item3}'")
                    .ToList()
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FeatureToggleTestAuthenticateUser_InvalidUsernameFormat()
        {
            // Arrange
            var username = "testUser"; // should be an email address
            var clientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelperTestClass(clientStub);

            // Act
            var caught = Assert.Throws<InvalidOperationException>(() => featureToggleHelper.Authenticate(username));

            // Assert
            Assert.IsNotNull(caught);
            Assert.IsNotNull(caught.InnerException);

            var toValidate = new
            {
                ExceptionMessage = caught.Message,
                InnerExceptionMessage = caught.InnerException.Message,
                ErrorMessages = featureToggleHelper.ErrorMessages
                    .Select(s => $"message='{s.Item1}' | exception.Message='{s.Item2.Message}' | memberName='{s.Item3}'")
                    .ToList()
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toValidate));
        }
    }

    /// <summary>
    /// A test class allowing access to a class's protected members for testing.
    /// </summary>
    public class FeatureToggleHelperTestClass : FeatureToggleHelper
    {
        public List<Tuple<string, Exception, string>> ErrorMessages { get; } = new List<Tuple<string, Exception, string>>();

        public FeatureToggleHelperTestClass(ILaunchDarklyClient launchDarklyClient)
        : base(launchDarklyClient)
        {
        }

        protected override void _LogError(string message, Exception ex = null, string memberName = "")
        {
            ErrorMessages.Add(new Tuple<string, Exception, string>(message, ex, memberName));
        }
    }
}