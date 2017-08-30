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
    
    public partial class station_program_flights
    {
        public station_program_flights()
        {
            this.inventory_detail_slot_components = new HashSet<inventory_detail_slot_components>();
            this.station_program_flight_audiences = new HashSet<station_program_flight_audiences>();
        }
    
        public int id { get; set; }
        public int station_program_id { get; set; }
        public int media_week_id { get; set; }
        public bool active { get; set; }
        public Nullable<decimal> C15s_rate { get; set; }
        public Nullable<decimal> C30s_rate { get; set; }
        public Nullable<decimal> C60s_rate { get; set; }
        public Nullable<decimal> C90s_rate { get; set; }
        public Nullable<decimal> C120s_rate { get; set; }
        public Nullable<int> spots { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
    
        public virtual ICollection<inventory_detail_slot_components> inventory_detail_slot_components { get; set; }
        public virtual ICollection<station_program_flight_audiences> station_program_flight_audiences { get; set; }
        public virtual station_programs station_programs { get; set; }
    }
}
