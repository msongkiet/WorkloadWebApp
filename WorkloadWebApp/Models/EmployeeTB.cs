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
    
    public partial class EmployeeTB
    {
        public string EmployeeID { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Position { get; set; }
        public string Role { get; set; }
        public string PL { get; set; }
        public string FunctionGroup { get; set; }
        public Nullable<int> WorkingDay { get; set; }
        public Nullable<int> UnitID { get; set; }
        public string SupervisorID { get; set; }
        public byte[] Picture { get; set; }
        public Nullable<bool> EmployeeActive { get; set; }
    }
}
