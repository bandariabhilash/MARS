using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FbWorkorderBillableSKUModel
    {
        public FbWorkorderBillableSKUModel()
        { }

        public FbWorkorderBillableSKUModel(FbWorkOrderSKU workordersku)
        {
            this.WorkOrderSKUId = workordersku.WorkOrderSKUId;
            this.WorkorderID = workordersku.WorkorderID;
            this.SKU = workordersku.SKU;
            this.Qty = workordersku.Qty;
            using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
            {
                FbBillableSKU fbsku = entity.FbBillableSKUs.Where(s => s.SKU == workordersku.SKU && s.IsActive == true).FirstOrDefault();
                if (fbsku != null)
                {
                    this.UnitPrice = fbsku.UnitPrice;
                    this.Description = fbsku.SKUDescription;
                }
            }
        }

        public int WorkOrderSKUId { get; set; }
        public int WorkorderID { get; set; }
        public string SKU { get; set; }
        public Nullable<int> Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Description { get; set; }
        public bool? PartReplenish { get; set; }


    }

}