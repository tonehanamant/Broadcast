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
    
    public partial class plan_version_pricing_job
    {
        public int id { get; set; }
        public int plan_version_id { get; set; }
        public int status { get; set; }
        public System.DateTime queued_at { get; set; }
        public Nullable<System.DateTime> completed_at { get; set; }
        public string error_message { get; set; }
        public string diagnostic_result { get; set; }
    
        public virtual plan_versions plan_versions { get; set; }
    }
}
