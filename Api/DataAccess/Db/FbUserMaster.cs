using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbUserMaster
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Address { get; set; }

    public string? Apt { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? CreatedDate { get; set; }

    public string? Country { get; set; }

    public string? ConfirmPassword { get; set; }

    public string? Mi { get; set; }

    public string? Company { get; set; }

    public string? Title { get; set; }

    public string? Manager { get; set; }

    public string? Region { get; set; }

    public string? Division { get; set; }

    public string? UpdatedDate { get; set; }

    public string? Fax { get; set; }

    public string? Jde { get; set; }

    public int? IsActive { get; set; }

    public int? UserTypeId { get; set; }

    public int? CallClosure { get; set; }

    public int? OnCallAccess { get; set; }

    public int? IsFirstTimeLogin { get; set; }

    public string? PasswordUpdatedDate { get; set; }

    public int? CanExport { get; set; }

    public int? TechId { get; set; }

    public int? IsTechnician { get; set; }

    public bool? IsErfuser { get; set; }

    public string? EmailId { get; set; }

    public int? CreatedUserId { get; set; }

    public string? CreatedUserName { get; set; }

    public string? CustomerParent { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string? RefreshToken { get; set; }

    public bool? ServiceAccess { get; set; }

    public virtual ICollection<FbactivityLog> FbactivityLogs { get; set; } = new List<FbactivityLog>();

    public virtual ICollection<FbcustomerNote> FbcustomerNotes { get; set; } = new List<FbcustomerNote>();

    public virtual ICollection<FbuserReport> FbuserReports { get; set; } = new List<FbuserReport>();

    public virtual FbRole Role { get; set; } = null!;

    public virtual ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();

    public virtual ICollection<UserRefreshtoken> UserRefreshtokens { get; set; } = new List<UserRefreshtoken>();

    public virtual UserType? UserType { get; set; }
}
