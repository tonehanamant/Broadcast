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
    
    public partial class inventory_detail_slot_components
    {
        public int id { get; set; }
        public int inventory_detail_slot_id { get; set; }
        public int station_program_flight_id { get; set; }
        public short station_code { get; set; }
        public int daypart_id { get; set; }
    
        public virtual inventory_detail_slots inventory_detail_slots { get; set; }
        public virtual station_program_flights station_program_flights { get; set; }
        public virtual station station { get; set; }
        public virtual daypart daypart { get; set; }
    }
}
