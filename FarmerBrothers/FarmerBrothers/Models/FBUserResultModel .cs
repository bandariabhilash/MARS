using FarmerBrothers.Data;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FBUserResultModel
    {
        public FBUserResultModel()
        {
           
        }
        public FBUserResultModel(FbUserMaster user)
        {
            this.UserId = user.UserId;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.UserName = user.FirstName + " " + user.LastName;
            this.Address = user.Address;
            this.City = user.City;
            this.State = user.State;
            this.Zip = user.Zip;
            this.Phone = user.Phone;
            this.Email = user.Email;
            this.RoleId = user.RoleId;
            this.IsActive = Convert.ToBoolean(user.IsActive);
            this.Company = user.Company;
            this.CanExport = Convert.ToBoolean(user.CanExport);
            this.TechId = user.TechId;
            this.IsTechnician = Convert.ToBoolean(user.IsTechnician);
            this.UserPassword = user.Password;
            this.EmailId = user.EmailId;
            this.CustomerParent = user.CustomerParent;
        }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public bool CanExport { get; set; }
        public Nullable<int> TechId { get; set; }
        public bool IsTechnician { get; set; }
        public string CustomerParent { get; set; }
        public string UserPassword { get; set; }
        public string EmailId { get; set; }
    }
}