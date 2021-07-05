

using System;
namespace FarmerBrothers.Data
{
    public class TechDispatchWithDistance
    {
        public int ServiceCenterId { get;set; }
        public string TechType { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }
        public string Phone { get; set; }
        public string AlternativePhone { get; set; }        
        public decimal Distance { get; set; }
        public string City { get; set; }
        public Int16 isUnavailable { get; set; }
        public string ReplaceTechnician { get; set; }
        public string CustomerSpecificTech { get; set; }

    }
}
