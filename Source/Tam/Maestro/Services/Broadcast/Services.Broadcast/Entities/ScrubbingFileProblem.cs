namespace Services.Broadcast.Entities
{
    public class ScrubbingFileProblem
    {
        public long Id { get; set; }
        public int FileId { get; set; }
        public string ProblemDescription { get; set; }
    }
}
