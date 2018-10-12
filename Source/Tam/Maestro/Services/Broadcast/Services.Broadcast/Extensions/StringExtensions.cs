namespace Services.Broadcast.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Preps the name for using in a file name.
        /// </summary>
        public static string PrepareForUsingInFileName(this string name)
        {
            return name.Replace("\\", string.Empty)
                       .Replace(":", string.Empty)
                       .Replace("*", string.Empty)
                       .Replace("?", string.Empty)
                       .Replace("/", string.Empty)
                       .Replace("<", string.Empty)
                       .Replace(">", string.Empty)
                       .Replace("|", string.Empty)
                       .Replace("\"", string.Empty);
        }
    }
}
