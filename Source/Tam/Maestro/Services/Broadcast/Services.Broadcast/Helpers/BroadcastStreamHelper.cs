using System.IO;

namespace Services.Broadcast.Helpers
{
    public static class BroadcastStreamHelper
    {
        public static Stream CreateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
