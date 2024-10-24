using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class NonServiceSearchModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Description { get; set; }

        public IList<NonServiceSearchResults> SearchResults { get; set; }
    }

    public class NonServiceSearchResults
    {
        public string ServiceType { get; set; }
        public string Count { get; set; }

        public string EventID { get; set; }
        public string EventStatus { get; set; }
        public string EntryDate { get; set; }
        public string ClosureDate { get; set; }
        public string CompanyName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerType { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string EmailAddress { get; set; }        
        public string Description { get; set; }        
        public string Route { get; set; }        
        public string Branch { get; set; }
        public string Region { get; set; }
        public string EmailSentTo { get; set; }
        public string Notes { get; set; }
        public string CreatedUserName { get; set; }
        public string ClosedBy { get; set; }
        public string ResolutionCallerName { get; set; }
        public string ClosureNotes { get; set; }
    }
}