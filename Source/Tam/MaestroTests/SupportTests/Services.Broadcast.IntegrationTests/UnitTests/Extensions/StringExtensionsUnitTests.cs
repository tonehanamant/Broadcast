using NUnit.Framework;
using Services.Broadcast.Extensions;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class StringExtensionsUnitTests
    {
        [Test]
        public void PrepareForUsingInFileName()
        {
            var result = StringExtensions.PrepareForUsingInFileName("Test\\:*?/<>|\"");

            Assert.AreEqual("Test", result);
        }

        [Test]
        public void RemoveWhiteSpaces()
        {
            var result = StringExtensions.RemoveWhiteSpaces("Text with white space");

            Assert.AreEqual("Textwithwhitespace", result);
        }

        [Test]
        public void Contains()
        {
            var result = StringExtensions.Contains("Text with white space", "white", System.StringComparison.InvariantCulture);

            Assert.IsTrue(result);
        }

        [Test]
        public void Contains_NotContains()
        {
            var result = StringExtensions.Contains("Text with white space", "yellow", System.StringComparison.InvariantCulture);

            Assert.IsFalse(result);
        }

        [Test]
        public void UnicodeDecodeString_Empty()
        {
            var result = StringExtensions.UnicodeDecodeString(string.Empty);

            Assert.IsNull(result);
        }

        [Test]
        public void UnicodeDecodeString()
        {
            var result = StringExtensions.UnicodeDecodeString("This string contains the unicode character Pi (\u03a0)");

            Assert.AreEqual("This string contains the unicode character Pi ()", result);
        }
    }
}
