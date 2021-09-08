using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers.Json;

namespace AttachmentMicroServiceApiTester
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

        public static string TransformResultToSql(PlanPricingApiSpotsResponseDto_v3 toTransform, string seed = "")
        {
            var sqlSb = new StringBuilder();
            var tableName = $"#AllocatedSpots_{seed}";

            var createTableSql = $"CREATE TABLE {tableName} ( id int, week_id int, spot_length_id int, frequency int);";
            sqlSb.AppendLine(createTableSql);

            foreach (var result in toTransform.Results)
            {
                foreach (var frequency in result.Frequencies)
                {
                    var insertSql = $"INSERT INTO {tableName} (id, week_id, spot_length_id, frequency) " +
                                    $"VALUES ({result.ManifestId}, {result.MediaWeekId}, {frequency.SpotLengthId}, {frequency.Frequency});";
                    sqlSb.AppendLine(insertSql);
                }
            }

            sqlSb.AppendLine("GO");
            return sqlSb.ToString();
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static byte[] StreamToByteArray(Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.Seek(0, SeekOrigin.Begin);
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}