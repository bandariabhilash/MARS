using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class ProjectNumberModel
    {
        public string ProjectID { get; set; }
        public DateTime?  DeadLine { get; set; }
        public string Notes { get; set; }
    }
}