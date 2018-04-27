using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public class AffidavitSaveResult
    {
        public int? Id { get; set; }
        public List<AffidavitValidationResult> ValidationResults { get; set; }

        public override string ToString()
        {
            string str = "";

            if (Id.HasValue) str += "ID=" + Id.Value;
            if (ValidationResults.Any())
            {
                if (str.Length > 0) str += "\r\n";
                str += "Validation Results\r\n";
                ValidationResults.ForEach(r => { str += r.ToString() + "\r\n"; });
            }

            if (!string.IsNullOrEmpty(str)) str += "\r\n";
            str += GetType().FullName;
            return str;
        }
    }
}
