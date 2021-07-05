using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmerBrothers.Data
{
    public class FbSKUWorkOrder
    {
        public int WorkOrderId { get; set; }
        public string SKU { get; set; }
        public int Qty { get; set; }
    }
}
