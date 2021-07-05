using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class WorkOrderModel : BaseModel
    {
        public Nullable<int> CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerState { get; set; }
        [DataType(DataType.MultilineText)]
        [Required()]
        public string Notes { get; set; }
    }
}