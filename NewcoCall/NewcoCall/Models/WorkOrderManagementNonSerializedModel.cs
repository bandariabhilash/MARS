using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FBCall.Models
{
    public class WorkOrderManagementNonSerializedModel
    {
        public int? NSerialid { get; set; }
        public string ManufNumber { get; set; }
        public string Catalogid { get; set; }
        public Nullable<int> OrigOrderQuantity { get; set; }
    }
}