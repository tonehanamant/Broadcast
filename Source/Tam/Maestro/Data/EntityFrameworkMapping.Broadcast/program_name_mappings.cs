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
    
    public partial class program_name_mappings
    {
        public int id { get; set; }
        public string inventory_program_name { get; set; }
        public string official_program_name { get; set; }
        public int genre_id { get; set; }
        public int show_type_id { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_at { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_at { get; set; }
    
        public virtual show_types show_types { get; set; }
        public virtual genre genre { get; set; }
    }
}
