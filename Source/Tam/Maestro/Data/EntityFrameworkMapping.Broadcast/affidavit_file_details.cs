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
    
    public partial class affidavit_file_details
    {
        public affidavit_file_details()
        {
            this.affidavit_client_scrubs = new HashSet<affidavit_client_scrubs>();
            this.affidavit_file_detail_audiences = new HashSet<affidavit_file_detail_audiences>();
        }
    
        public long id { get; set; }
        public int affidavit_file_id { get; set; }
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
    
        public virtual ICollection<affidavit_client_scrubs> affidavit_client_scrubs { get; set; }
        public virtual affidavit_files affidavit_files { get; set; }
        public virtual ICollection<affidavit_file_detail_audiences> affidavit_file_detail_audiences { get; set; }
    }
}
