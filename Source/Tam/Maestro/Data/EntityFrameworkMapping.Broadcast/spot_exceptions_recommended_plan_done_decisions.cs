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
    
    public partial class spot_exceptions_recommended_plan_done_decisions
    {
        public int id { get; set; }
        public int spot_exceptions_recommended_plan_details_done_id { get; set; }
        public string decided_by { get; set; }
        public System.DateTime decided_at { get; set; }
        public string synced_by { get; set; }
        public Nullable<System.DateTime> synced_at { get; set; }
    
        public virtual spot_exceptions_recommended_plan_details_done spot_exceptions_recommended_plan_details_done { get; set; }
    }
}
