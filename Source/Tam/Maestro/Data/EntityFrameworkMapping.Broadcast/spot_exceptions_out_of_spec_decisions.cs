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
    
    public partial class spot_exceptions_out_of_spec_decisions
    {
        public int id { get; set; }
        public int spot_exceptions_out_of_spec_id { get; set; }
        public bool accepted_as_in_spec { get; set; }
        public string decision_notes { get; set; }
        public string username { get; set; }
        public System.DateTime created_at { get; set; }
        public string synced_by { get; set; }
        public Nullable<System.DateTime> synced_at { get; set; }
    
        public virtual spot_exceptions_out_of_specs spot_exceptions_out_of_specs { get; set; }
    }
}
