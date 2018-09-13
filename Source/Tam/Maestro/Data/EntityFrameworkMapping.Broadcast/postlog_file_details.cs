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
    
    public partial class postlog_file_details
    {
        public postlog_file_details()
        {
            this.postlog_file_detail_demographics = new HashSet<postlog_file_detail_demographics>();
            this.postlog_file_detail_problems = new HashSet<postlog_file_detail_problems>();
        }
    
        public long id { get; set; }
        public int postlog_file_id { get; set; }
        public string station { get; set; }
        public System.DateTime original_air_date { get; set; }
        public System.DateTime adjusted_air_date { get; set; }
        public int air_time { get; set; }
        public int spot_length_id { get; set; }
        public string isci { get; set; }
        public string program_name { get; set; }
        public string genre { get; set; }
        public string leadin_genre { get; set; }
        public string leadin_program_name { get; set; }
        public string leadout_genre { get; set; }
        public string leadout_program_name { get; set; }
        public string market { get; set; }
        public Nullable<int> estimate_id { get; set; }
        public Nullable<int> inventory_source { get; set; }
        public Nullable<double> spot_cost { get; set; }
        public string affiliate { get; set; }
        public Nullable<int> leadin_end_time { get; set; }
        public Nullable<int> leadout_start_time { get; set; }
        public string program_show_type { get; set; }
        public string leadin_show_type { get; set; }
        public string leadout_show_type { get; set; }
        public bool archived { get; set; }
    
        public virtual ICollection<postlog_file_detail_demographics> postlog_file_detail_demographics { get; set; }
        public virtual ICollection<postlog_file_detail_problems> postlog_file_detail_problems { get; set; }
        public virtual postlog_files postlog_files { get; set; }
    }
}
