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
    
    public partial class WorkloadRecordTB
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Week { get; set; }
        public int ProjectID { get; set; }
        public Nullable<double> Workload { get; set; }
        public bool CommitData { get; set; }
        public string Remark { get; set; }
    }
}
