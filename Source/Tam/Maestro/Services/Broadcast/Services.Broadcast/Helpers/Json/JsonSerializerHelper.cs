using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Text;

namespace Services.Broadcast.Helpers.Json
{
    /// <summary>
    /// Assists with Json Serialization operations.
    /// </summary>
    public class JsonSerializerHelper
    {
        private static IJsonIgnorignator Ignorer = null;

        /// <summary>
        /// Serialize to a json string.  Use the settings to ignore properties.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="serializerSettings">The optional settings.</param>
        /// <returns>string of serialized json.</returns>
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
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonSerializer.Serialize(jsonWriter, obj);
            }

            var json = sw.ToString();
            return json;
        }

        /// <summary>
        /// Deserialize from the json string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ConvertFromJson<T>(string json)
        {
            var item = JsonConvert.DeserializeObject<T>(json);
            return item;
        }
        
        private static JsonSerializerSettings _GetJsonSettings()
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
    }
}