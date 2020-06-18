using System;
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

        public static bool Contains(this string source, string search, StringComparison stringComparison)
        {
            return source.IndexOf(search, stringComparison) != -1;
        }

        public static string UnicodeDecodeString(this string encoded)
        {
            if (string.IsNullOrWhiteSpace(encoded))
            {
                return null;
            }

            var decoded = Encoding.ASCII.GetString(
                Encoding.Convert(
                    Encoding.UTF8,
                    Encoding.GetEncoding(
                        Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                    ),
                    Encoding.UTF8.GetBytes(encoded)
                ));

            return decoded;
        }
    }
}
