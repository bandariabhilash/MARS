//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FarmerBrothers.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class FBRoleFunction
    {
        public int RoleID { get; set; }
        public int FunctionID { get; set; }
        public Nullable<int> CanCreate { get; set; }
        public Nullable<int> CanUpdate { get; set; }
        public Nullable<int> CanView { get; set; }
        public Nullable<int> CanExport { get; set; }
        public Nullable<int> CanEmail { get; set; }
        public Nullable<int> RoleFunctionID { get; set; }
    }
}