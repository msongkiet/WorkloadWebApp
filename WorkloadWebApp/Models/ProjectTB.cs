//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WorkloadWebApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProjectTB
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectType { get; set; }
        public string ProjectCategory { get; set; }
        public string ProjectCharacteristic { get; set; }
        public string BusinessStrategy { get; set; }
        public string ProjectOwner { get; set; }
        public Nullable<bool> ProjectActive { get; set; }
    }
}