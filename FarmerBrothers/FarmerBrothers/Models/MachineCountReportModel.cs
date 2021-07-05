using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class MachineCountReportModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<MachineCountSearchResults> SearchResults { get; set; }
    }

    public class MachineCountSearchResults
    {

        //public string AccountId { get; set; }
        //public string Company { get; set; }
        //public string Address { get; set; }
        //public string City { get; set; }
        //public string State { get; set; }
        //public string Zip { get; set; }
        //public string Phone { get; set; }
        public int DealerId { get; set; }
        public string Company { get; set; }
        public string Branch { get; set; }
        public string BranchName { get; set; }
        public Nullable<int> EquipCount { get; set; }
    }
}