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
    
    public partial class shared_folder_files
    {
        public shared_folder_files()
        {
            this.scx_generation_job_files = new HashSet<scx_generation_job_files>();
            this.inventory_export_jobs = new HashSet<inventory_export_jobs>();
            this.inventory_files = new HashSet<inventory_files>();
            this.inventory_files1 = new HashSet<inventory_files>();
        }
    
        public System.Guid id { get; set; }
        public string folder_path { get; set; }
        public string file_name { get; set; }
        public string file_extension { get; set; }
        public string file_media_type { get; set; }
        public int file_usage { get; set; }
        public System.DateTime created_date { get; set; }
        public string created_by { get; set; }
        public Nullable<System.Guid> attachment_id { get; set; }
    
        public virtual ICollection<scx_generation_job_files> scx_generation_job_files { get; set; }
        public virtual ICollection<inventory_export_jobs> inventory_export_jobs { get; set; }
        public virtual ICollection<inventory_files> inventory_files { get; set; }
        public virtual ICollection<inventory_files> inventory_files1 { get; set; }
    }
}
