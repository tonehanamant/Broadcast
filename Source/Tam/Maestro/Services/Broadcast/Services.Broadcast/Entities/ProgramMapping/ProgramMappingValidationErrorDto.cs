using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.ProgramMapping
{
    public class ProgramMappingValidationErrorDto
    {
        public string OfficialProgramName { get; set; }
        public string ErrorMessage { get; internal set; }
    }
}
