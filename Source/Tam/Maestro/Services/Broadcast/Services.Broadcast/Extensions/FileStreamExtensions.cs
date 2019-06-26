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
    }
}
