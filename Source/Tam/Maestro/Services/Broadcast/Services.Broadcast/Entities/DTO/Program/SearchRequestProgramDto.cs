using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO.Program
{
    public class SearchRequestProgramDto
    {
        public string ProgramName { get; set; }

        /// <summary>
        /// A start index for paging
        /// </summary>
        public int? Start { get; set; }

        /// <summary>
        /// A number of items to return
        /// </summary>
        public int? Limit { get; set; }

        public List<string> IgnorePrograms { get; set; } = new List<string>();
    }
}
