using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Nti
{
    public class NtiFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public FileProcessingStatusEnum Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<NtiRatingDocument> Details { get; set; } = new List<NtiRatingDocument>();
        public List<FileProblem> FileProblems { get; set; } = new List<FileProblem>();
    }
}
