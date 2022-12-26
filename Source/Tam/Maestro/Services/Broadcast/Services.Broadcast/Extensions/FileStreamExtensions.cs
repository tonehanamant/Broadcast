using System;
using System.IO;

namespace Services.Broadcast.Extensions
{
    public static class FileStreamExtensions
    {
        public static string ToBase64String(this FileStream fileStream)
        {
            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            return Convert.ToBase64String(bytes);
        }
        public static string ConvertToBase64String(Stream stream)
        {
            if (stream is MemoryStream memoryStream)
            {
                return Convert.ToBase64String(memoryStream.ToArray());
            }

            var bytes = new Byte[(int)stream.Length];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int)stream.Length);

            return Convert.ToBase64String(bytes);
        }
    }
}
