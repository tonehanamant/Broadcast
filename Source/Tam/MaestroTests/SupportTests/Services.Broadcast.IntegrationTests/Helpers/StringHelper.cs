namespace Services.Broadcast.IntegrationTests.Helpers
{
    /// <summary>
    /// Helpful operations for strings.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Creates a string of the given length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>Returns a string of the given length.</returns>
        public static string CreateStringOfLength(int length)
        {
            var result = new string('*', length);
            return result;
        }
    }
}