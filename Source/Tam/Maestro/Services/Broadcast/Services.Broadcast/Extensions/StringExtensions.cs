using System.IO;
using System.Text;

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

        public static string RemoveWhiteSpaces(this string input)
        {
            var s = new StringBuilder(input.Length); // (input.Length);
            using (var reader = new StringReader(input))
            {
                int i = 0;
                char c;
                for (; i < input.Length; i++)
                {
                    c = (char)reader.Read();
                    if (!char.IsWhiteSpace(c))
                    {
                        s.Append(c);
                    }
                }
            }

            return s.ToString();
        }
    }
}
