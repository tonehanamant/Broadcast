namespace Services.Broadcast.Entities
{
    public class PostingBookResultDto
    {
        public int PostingBookId { get; set; }
        public bool HasWarning { get; set; }
        public string WarningMessage { get; set; }
    }
}
