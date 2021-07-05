using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class ERFDataMaintenanceModel
    {
        public string ContingentName { get; set; }
        public string ContingentType { get; set; }
        public string ContingentItemName { get; set; }
        public decimal LaidInCost { get; set; }
        public decimal CostSale { get; set; }
        public decimal Rental { get; set; }

        public List<ContingentModel> ContingentsList { get; set; }
        public List<ContingentDetails> ContingentItemsList { get; set; }
        public List<OrderType> OrderTypeList { get; set; }
        public List<ERFBranchDetails> BranchList { get; set; }

        public List<SubstituionModel> ContingentTypeList { get; set; }
        public List<SubstituionModel> ContingentNamesList { get; set; }
        public List<SubstituionModel> OrderTypeDetailsList { get; set; }
        public List<SubstituionModel> BranchDetailsList { get; set; }

        //BulkERF Upload Fields
        public bool ResultType { get; set; }
        public string Message { get; set; } 
        public List<Erf> ResultData { get; set; }
    }

    public class ContingentModel
    {
        public int ContingentId { get; set; }
        public string ContingentName { get; set; }
        public string ContingentType { get; set; }
        public bool IsActive { get; set; }
    }

    public class OrderType
    {
        public  int OrderTypeId { get; set; }
        public string OrderTypeDesc { get; set; }
        public bool IsActive { get; set; }
    }
}