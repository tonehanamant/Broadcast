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
    
    public partial class station_mappings
    {
        public int id { get; set; }
        public string mapped_call_letters { get; set; }
        public System.DateTime created_date { get; set; }
        public string created_by { get; set; }
        public int station_id { get; set; }
        public int map_set { get; set; }
    
        public virtual station station { get; set; }
    }
}