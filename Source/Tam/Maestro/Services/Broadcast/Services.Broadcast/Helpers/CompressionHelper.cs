using Amazon.S3.Model;
using System.IO;
using System.IO.Compression;
using System.Text;
using Services.Broadcast.Helpers;
using System.Net.Http;

namespace Services.Broadcast.Helpers
{
    public static class CompressionHelper
    {

        /// <summary>Gets the gzip compress.</summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static byte[] GetGzipCompress(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);

            byte[] output;

            using (var originalFileStream = Helpers.BroadcastStreamHelper.CreateStreamFromString(input))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var archive = new GZipStream(outputStream, CompressionMode.Compress, true))
                    {
                        archive.Write(inputBytes, 0, inputBytes.Length);
                    }
                    output = outputStream.ToArray();
                }
            }
            return output;
        }

        /// <summary>Gets the gzip uncompress.</summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        public static string GetGzipUncompress(GetObjectResponse response)
        {
            var content = string.Empty;
            using (var archive = new GZipStream(response.ResponseStream, CompressionMode.Decompress))
            {
                using( var resultStream = new MemoryStream())
                {
                    archive.CopyTo(resultStream);
                    resultStream.Position = 0;
                    var reader = new StreamReader(resultStream);
                    content = reader.ReadToEnd();
                }
            }
            return content;
        }

        /// <summary>Gets the gzip uncompress.</summary>
        /// <param name="gzip">The response.</param>
        /// <returns></returns>
        public static string GetGzipUncompress(byte[] gzip)
        {
            var content = string.Empty;
            using (var memoryStream = new MemoryStream(gzip))
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            using (var memoryStreamOutput = new MemoryStream())
            {
                gZipStream.CopyTo(memoryStreamOutput);
                memoryStreamOutput.Position = 0;
                var reader = new StreamReader(memoryStreamOutput);
                content = reader.ReadToEnd();

                return content;
            }
        }
    }
}
