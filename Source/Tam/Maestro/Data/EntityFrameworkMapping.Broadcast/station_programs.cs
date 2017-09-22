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
    
    public partial class station_programs
    {
        public station_programs()
        {
            this.station_program_flights = new HashSet<station_program_flights>();
            this.genres = new HashSet<genre>();
        }
    
        public int id { get; set; }
        public string program_name { get; set; }
        public short station_code { get; set; }
        public int daypart_id { get; set; }
        public string daypart_name { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public Nullable<int> rate_file_id { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
        public Nullable<byte> rate_source { get; set; }
        public string daypart_code { get; set; }
        public int spot_length_id { get; set; }
        public Nullable<decimal> fixed_price { get; set; }
    
        public virtual rate_files rate_files { get; set; }
        public virtual ICollection<station_program_flights> station_program_flights { get; set; }
        public virtual station station { get; set; }
        public virtual daypart daypart { get; set; }
        public virtual spot_lengths spot_lengths { get; set; }
        public virtual ICollection<genre> genres { get; set; }
    }
}
