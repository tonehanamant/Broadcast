using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public enum AffidavitFileDetailProblemTypeEnum
    {
        UnlinkedIsci = 1,
        UnmarriedOnMultipleContracts = 2
    }

    public class AffidavitFileDetailProblem
    {
        public AffidavitFileDetailProblemTypeEnum Type { get; set; }
        public string Description { get; set; }
    }
}
