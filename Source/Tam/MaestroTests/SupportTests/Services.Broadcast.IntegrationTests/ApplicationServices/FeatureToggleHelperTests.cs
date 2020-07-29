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
        [TestCase("TestUser", "TestToggle_Enabled", true)]
        [TestCase("TestUser", "TestToggle_disabled", false)]
        [TestCase("TestUser", "TestToggle_unknown", false)]
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