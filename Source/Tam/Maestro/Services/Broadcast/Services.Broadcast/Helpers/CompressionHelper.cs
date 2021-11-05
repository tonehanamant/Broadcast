using System.IO;
using System.IO.Compression;
using System.Text;

namespace Services.Broadcast.Helpers
{
    public static class CompressionHelper
    {
        public static string ToGZipCompressed(this string input)
        {
            var output = string.Empty;
            var inputBytes = Encoding.UTF8.GetBytes(input);

            using (var outputStream = new MemoryStream())
            using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gZipStream.Write(inputBytes, 0, inputBytes.Length);
                var outputBytes = outputStream.ToArray();
                output = Encoding.UTF8.GetString(outputBytes);
            }

            return output;
        }
    }
}
