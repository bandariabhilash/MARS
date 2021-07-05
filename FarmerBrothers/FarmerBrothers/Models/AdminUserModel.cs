using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class AdminUserModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

}