using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class ValidationWarningDto
    {
        public bool HasWarning { get; set; }
        public string Message { get; set; }
    }
}
