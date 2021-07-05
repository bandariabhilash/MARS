using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FIMAccountSearchModelResult
    {
        public string Active { get; set; }
        public string InvoicingAccount { get; set; }
        public string TechnicianAccount { get; set; }
        public string LocationID { get; set; }
        public string TechId { get; set; }
        public string LocationName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        //public string ParentVendorID { get; set; }
        //public string ParentVendorName { get; set; }

    }
}