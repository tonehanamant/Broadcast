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
    
    public partial class program_name_genres
    {
        public int id { get; set; }
        public Nullable<int> program_name_id { get; set; }
        public Nullable<int> genre_id { get; set; }
    
        public virtual genre genre { get; set; }
        public virtual program_names program_names { get; set; }
    }
}
