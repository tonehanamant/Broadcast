using Newtonsoft.Json;
using System;
using System.IO;

namespace Services.Broadcast.IntegrationTests
{
    public class JsonTextWriterOptimized : JsonTextWriter
    {
        public JsonTextWriterOptimized(TextWriter textWriter)
            : base(textWriter)
        {
        }
        public override void WriteValue(double value)
        {
            value = Math.Round(value, 8);
            base.WriteValue(value);
        }
    }

    public class JsonTextWriterOptimizedMoreRounding : JsonTextWriter
    {
        private int _placesToRound;

        public JsonTextWriterOptimizedMoreRounding(TextWriter textWriter, int placesToRound)
            : base(textWriter)
        {
            _placesToRound = placesToRound;
        }
        public override void WriteValue(double value)
        {
            value = Math.Round(value, _placesToRound);
            base.WriteValue(value);
        }

        public override void WriteValue(double? value)
        {
            if (value.HasValue)
                value = Math.Round(value.Value, _placesToRound);

            base.WriteValue(value);
        }

        public override void WriteValue(decimal value)
        {
            value = Math.Round(value, _placesToRound);
            base.WriteValue(value);
        }

        public override void WriteValue(decimal? value)
        {
            if (value.HasValue)
                value = Math.Round(value.Value, _placesToRound);

            base.WriteValue(value);
        }

        public override void WriteValue(float value)
        {
            value = (float)Math.Round(value, _placesToRound);
            base.WriteValue(value);
        }

        public override void WriteValue(float? value)
        {
            if (value.HasValue)
                value = (float)Math.Round(value.Value, _placesToRound);

            base.WriteValue(value);
        }
    }
}