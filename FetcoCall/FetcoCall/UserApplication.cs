//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FetcoCall
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserApplication
    {
        public int ID { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> ApplicationId { get; set; }
        public Nullable<int> PrivilegeId { get; set; }
    
        public virtual Application Application { get; set; }
        public virtual FbUserMaster FbUserMaster { get; set; }
        public virtual Privilege Privilege { get; set; }
    }
}