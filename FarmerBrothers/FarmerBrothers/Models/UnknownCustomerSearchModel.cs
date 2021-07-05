using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class UnknownCustomerSearchModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public List<UnknownCustomersSearchResults> SearchResults { get; set; }
    }


    public class UnknownCustomersSearchResults
    {
        public string AccountId { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
    }
}