using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FarmerBrothers.Models
{
    public class FIMModel
    {
        public string VendorBranchID { get; set; }
        public string VendorBranchName { get; set; }
        public string VendorBranchCity { get; set; }
        public string VendorBranchState { get; set; }
        public string VendorBranchPhone { get; set; }
        public string ParentVendorID { get; set; }
        public string ParentVendorName { get; set; }
        public string VendorNickname { get; set; }
        public string VendorBranchEmail { get; set; }
        public string Userpassword { get; set; }

        [Display(Name = "Email address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string CopyOnDispatchEmail { get; set; }

        public string TechId { get; set; }
        public string TechName { get; set; }
        public string TechPhone{ get; set; }
        public string TechnicianJMSLoginID { get; set; }
        public string EmailAccount { get; set; }
        public bool TechnicianAccount { get; set; }
        public bool InvoicingAccount { get; set; }
        public string IsActive { get; set; }
        public string TechType { get; set; }
        public string TechDesc { get; set; }
    }
}