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
    
    public partial class inventory_programs_by_file_jobs
    {
        public inventory_programs_by_file_jobs()
        {
            this.inventory_programs_by_file_job_notes = new HashSet<inventory_programs_by_file_job_notes>();
        }
    
        public int id { get; set; }
        public int status { get; set; }
        public int inventory_file_id { get; set; }
        public System.DateTime queued_at { get; set; }
        public string queued_by { get; set; }
        public Nullable<System.DateTime> completed_at { get; set; }
        public string status_message { get; set; }
    
        public virtual inventory_files inventory_files { get; set; }
        public virtual ICollection<inventory_programs_by_file_job_notes> inventory_programs_by_file_job_notes { get; set; }
    }
}
