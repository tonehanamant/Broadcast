using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Tam.Maestro.Common
{
    public static class SerializationHelper
    {
        public static string ConvertToJson(object obj)
        {
            var jsonSerializer = Newtonsoft.Json.JsonSerializer.Create();
            jsonSerializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            var sb = new StringBuilder(256);
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (var jsonWriter = new JsonTextWriterWithRounding(sw))
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonSerializer.Serialize(jsonWriter, obj);
            }

            var json = sw.ToString();
            return json;
        }

        public class JsonTextWriterWithRounding : JsonTextWriter
        {
            public JsonTextWriterWithRounding(TextWriter textWriter)
                : base(textWriter)
            {
            }
            public override void WriteValue(double value)
            {
                value = Math.Round(value, 8);
                base.WriteValue(value);
            }
        }
    }
}
