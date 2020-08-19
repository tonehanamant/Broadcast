using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Services.Broadcast.IntegrationTests
{
    public interface IJsonIgnorignator
    {
        void SetupIgnoreFields(IgnorableSerializerContractResolver resolver);
    }

    public class IntegrationTestHelper
    {
        public static IJsonIgnorignator Ignorer;

        public static string ConvertToJson(object obj, JsonSerializerSettings serializerSettings = null)
        {
            var jsonSerializer = Newtonsoft.Json.JsonSerializer.Create();
            jsonSerializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            if (serializerSettings == null)
            {
                jsonSerializer.ContractResolver = _GetJsonSettings().ContractResolver;
            }
            else
            {
                jsonSerializer.ContractResolver = serializerSettings.ContractResolver;
            }

            var sb = new StringBuilder(256);
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (var jsonWriter = new JsonTextWriterOptimized(sw))
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonSerializer.Serialize(jsonWriter, obj);
            }

            var json = sw.ToString();
            return json;
        }

        public static string ConvertToJsonMoreRounding(object obj, JsonSerializerSettings serializerSettings = null, int placesToRound = 5)
        {
            var jsonSerializer = Newtonsoft.Json.JsonSerializer.Create();
            jsonSerializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            if (serializerSettings == null)
            {
                jsonSerializer.ContractResolver = _GetJsonSettings().ContractResolver;
            }
            else
            {
                jsonSerializer.ContractResolver = serializerSettings.ContractResolver;
            }

            var sb = new StringBuilder(256);
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (var jsonWriter = new JsonTextWriterOptimizedMoreRounding(sw, placesToRound))
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonSerializer.Serialize(jsonWriter, obj);
            }

            var json = sw.ToString();
            return json;
        }

        public static JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            if (Ignorer != null)
            {
                Ignorer.SetupIgnoreFields(jsonResolver);
            }

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            return jsonSettings;
        }

        public static string GetBroadcastAppFolder()
        {
#if DEBUG
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
#else
            return BroadcastServiceSystemParameter.BroadcastAppFolder;
#endif
        }
    }
}