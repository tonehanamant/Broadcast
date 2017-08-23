using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.Entities
{
    public class DefaultPostingBooksDto
    {
        public PostingBookResultDto DefaultShareBook { get; set; }
        public PostingBookResultDto DefaultHutBook { get; set; }
        public int UseShareBookOnlyId { get; set; }
        public ProposalEnums.ProposalPlaybackType DefaultPlaybackType { get; set; }
    }
}
