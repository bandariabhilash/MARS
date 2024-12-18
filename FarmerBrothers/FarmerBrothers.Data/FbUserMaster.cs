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
    
    public partial class FbUserMaster
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FbUserMaster()
        {
            this.FBActivityLogs = new HashSet<FBActivityLog>();
            this.FBUserReports = new HashSet<FBUserReport>();
            this.UserApplications = new HashSet<UserApplication>();
            this.FBCustomerNotes = new HashSet<FBCustomerNote>();
        }
    
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Apt { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CreatedDate { get; set; }
        public string Country { get; set; }
        public string ConfirmPassword { get; set; }
        public string MI { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string Manager { get; set; }
        public string Region { get; set; }
        public string Division { get; set; }
        public string UpdatedDate { get; set; }
        public string Fax { get; set; }
        public string JDE { get; set; }
        public Nullable<int> IsActive { get; set; }
        public Nullable<int> UserTypeId { get; set; }
        public Nullable<int> CallClosure { get; set; }
        public Nullable<int> OnCallAccess { get; set; }
        public Nullable<int> IsFirstTimeLogin { get; set; }
        public string PasswordUpdatedDate { get; set; }
        public Nullable<int> CanExport { get; set; }
        public Nullable<int> TechId { get; set; }
        public Nullable<int> IsTechnician { get; set; }
        public Nullable<bool> IsERFUser { get; set; }
        public string EmailId { get; set; }
        public Nullable<int> CreatedUserId { get; set; }
        public string CreatedUserName { get; set; }
        public string CustomerParent { get; set; }
        public Nullable<System.DateTime> RefreshTokenExpiryTime { get; set; }
        public string RefreshToken { get; set; }
        public Nullable<bool> ServiceAccess { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FBActivityLog> FBActivityLogs { get; set; }
        public virtual FbRole FbRole { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FBUserReport> FBUserReports { get; set; }
        public virtual UserType UserType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserApplication> UserApplications { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FBCustomerNote> FBCustomerNotes { get; set; }
    }
}
