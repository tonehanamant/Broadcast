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
    
    public partial class program_name_mapping_keywords
    {
        public int id { get; set; }
        public string keyword { get; set; }
        public string program_name { get; set; }
        public int genre_id { get; set; }
        public int show_type_id { get; set; }
    
        public virtual genre genre { get; set; }
        public virtual show_types show_types { get; set; }
    }
}
