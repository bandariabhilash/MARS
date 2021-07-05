using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class AdminModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmPassowrd { get; set; }

    }
}