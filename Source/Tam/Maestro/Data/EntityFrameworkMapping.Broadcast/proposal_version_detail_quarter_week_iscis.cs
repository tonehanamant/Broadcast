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
    
    public partial class proposal_version_detail_quarter_week_iscis
    {
        public int id { get; set; }
        public int proposal_version_detail_quarter_week_id { get; set; }
        public string client_isci { get; set; }
        public string house_isci { get; set; }
        public string brand { get; set; }
        public bool married_house_iscii { get; set; }
    
        public virtual proposal_version_detail_quarter_weeks proposal_version_detail_quarter_weeks { get; set; }
    }
}
