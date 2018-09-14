using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class FileDetailProblem
    {
        public FileDetailProblemTypeEnum Type { get; set; }
        public string Description { get; set; }
        public long DetailId { get; set; }
    }
}
