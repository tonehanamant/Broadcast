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
    
    public partial class spot_exceptions_results_jobs
    {
        public int id { get; set; }
        public long databricks_job_id { get; set; }
        public int databricks_run_id { get; set; }
        public System.DateTime queued_at { get; set; }
        public string queued_by { get; set; }
        public Nullable<System.DateTime> completed_at { get; set; }
        public string result { get; set; }
    }
}
