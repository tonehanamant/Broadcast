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
    
    public partial class inventory_file_ratings_jobs
    {
        public inventory_file_ratings_jobs()
        {
            this.inventory_file_ratings_job_notes = new HashSet<inventory_file_ratings_job_notes>();
        }
    
        public int id { get; set; }
        public int inventory_file_id { get; set; }
        public int status { get; set; }
        public System.DateTime queued_at { get; set; }
        public Nullable<System.DateTime> completed_at { get; set; }
    
        public virtual inventory_files inventory_files { get; set; }
        public virtual ICollection<inventory_file_ratings_job_notes> inventory_file_ratings_job_notes { get; set; }
    }
}
