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
    
    public partial class plan_version_daypart_customizations
    {
        public int id { get; set; }
        public int plan_version_daypart_id { get; set; }
        public int custom_daypart_organization_id { get; set; }
        public string custom_daypart_name { get; set; }
    
        public virtual custom_daypart_organizations custom_daypart_organizations { get; set; }
        public virtual plan_version_dayparts plan_version_dayparts { get; set; }
    }
}
