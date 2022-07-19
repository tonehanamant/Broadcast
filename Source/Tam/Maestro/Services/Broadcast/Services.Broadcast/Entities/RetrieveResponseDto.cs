using System.Text;

namespace Services.Broadcast.Entities
{
    public class RetrieveResponseDto
    {
        public bool success { get; set; }
        public string message { get; set; }
        public byte[] result { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("******************");

            sb.AppendLine($"success = {success}");
            sb.AppendLine($"message = {message}");

            sb.AppendLine("******************");

            return sb.ToString();
        }
    }
}
