using System;
using System.IO;
using System.IO.Compression;
using Services.Broadcast.Helpers.Json;

namespace PricingModelEndpointTester
{
    public static class Utilities
    {
        public static T GetFromFile<T>(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File Not found.", path);
            }

            var raw = File.ReadAllText(path);
            var transformed = JsonSerializerHelper.ConvertFromJson<T>(raw);
            return transformed;
        }

        public static void WriteToFile(string path, object content)
        {
            if (File.Exists(path))
            {
                Console.WriteLine($"Deleting found file to overwrite : '{path}'");
            }

            var raw = JsonSerializerHelper.ConvertToJson(content);
            File.WriteAllText(path, raw);
        }

        public static void WriteStringToFile(string path, string content)
        {
            if (File.Exists(path))
            {
                Console.WriteLine($"Deleting found file to overwrite : '{path}'");
            }

            File.WriteAllText(path, content);
        }

        public static ZipArchive CreateZipArchive(string name, object content)
        {
            var raw = JsonSerializerHelper.ConvertToJson(content);
            var contentStream = new MemoryStream();
            var writer = new StreamWriter(contentStream);
            writer.WriteLine(raw);
            writer.Flush();
            contentStream.Seek(0, SeekOrigin.Begin);
            

            MemoryStream archiveFile = new MemoryStream();
            var archive = new ZipArchive(archiveFile, ZipArchiveMode.Update, true);
            var archiveEntry = archive.CreateEntry(name);
            using (var zippedStreamEntry = archiveEntry.Open())
            {
                contentStream.CopyTo(zippedStreamEntry);
            }
            archiveFile.Seek(0, SeekOrigin.Begin);
            return archive;
        }

        public static byte[] ReadAllBytes(Stream sourceStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                sourceStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}