//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntityFrameworkMapping.Broadcast
{
    using System;
    using System.Collections.Generic;
    
    public partial class spot_exceptions_out_of_spec_reason_codes
    {
        public spot_exceptions_out_of_spec_reason_codes()
        {
            this.spot_exceptions_out_of_specs_done = new HashSet<spot_exceptions_out_of_specs_done>();
            this.spot_exceptions_out_of_specs = new HashSet<spot_exceptions_out_of_specs>();
        }
    
        public int id { get; set; }
        public int reason_code { get; set; }
        public string reason { get; set; }
        public string label { get; set; }
    
        public virtual ICollection<spot_exceptions_out_of_specs_done> spot_exceptions_out_of_specs_done { get; set; }
        public virtual ICollection<spot_exceptions_out_of_specs> spot_exceptions_out_of_specs { get; set; }
    }
}
