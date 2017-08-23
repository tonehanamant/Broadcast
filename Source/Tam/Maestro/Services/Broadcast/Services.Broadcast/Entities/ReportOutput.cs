using System.IO;

namespace Services.Broadcast.Entities
{
    public class ReportOutput
    {
        public MemoryStream Stream { get; set; }
        public string Filename { get; set; }

        public ReportOutput(string filename)
        {
            Stream = new MemoryStream();
            Filename = filename;
        }
    }
}
