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
    
    public partial class inventory_files
    {
        public inventory_files()
        {
            this.station_contacts = new HashSet<station_contacts>();
            this.station_contacts1 = new HashSet<station_contacts>();
            this.station_inventory_manifest = new HashSet<station_inventory_manifest>();
            this.station_inventory_manifest_staging = new HashSet<station_inventory_manifest_staging>();
            this.inventory_file_problems = new HashSet<inventory_file_problems>();
            this.inventory_file_ratings_jobs = new HashSet<inventory_file_ratings_jobs>();
            this.inventory_file_proprietary_header = new HashSet<inventory_file_proprietary_header>();
            this.inventory_programs_by_file_jobs = new HashSet<inventory_programs_by_file_jobs>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string identifier { get; set; }
        public string file_hash { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public byte status { get; set; }
        public int inventory_source_id { get; set; }
        public Nullable<int> rows_processed { get; set; }
        public Nullable<System.DateTime> effective_date { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public Nullable<System.Guid> shared_folder_files_id { get; set; }
        public Nullable<System.Guid> error_file_shared_folder_files_id { get; set; }
    
        public virtual inventory_sources inventory_sources { get; set; }
        public virtual ICollection<station_contacts> station_contacts { get; set; }
        public virtual ICollection<station_contacts> station_contacts1 { get; set; }
        public virtual ICollection<station_inventory_manifest> station_inventory_manifest { get; set; }
        public virtual ICollection<station_inventory_manifest_staging> station_inventory_manifest_staging { get; set; }
        public virtual ICollection<inventory_file_problems> inventory_file_problems { get; set; }
        public virtual ICollection<inventory_file_ratings_jobs> inventory_file_ratings_jobs { get; set; }
        public virtual ICollection<inventory_file_proprietary_header> inventory_file_proprietary_header { get; set; }
        public virtual ICollection<inventory_programs_by_file_jobs> inventory_programs_by_file_jobs { get; set; }
        public virtual shared_folder_files shared_folder_files { get; set; }
        public virtual shared_folder_files shared_folder_files1 { get; set; }
    }
}
