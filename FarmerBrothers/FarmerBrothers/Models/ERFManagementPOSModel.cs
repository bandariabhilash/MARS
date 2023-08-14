using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class ERFManagementPOSModel
    {
        public ERFManagementPOSModel()
        { }


        public ERFManagementPOSModel(FBERFPos pos)
        {

            ERFPosId = pos.ERFPosId;
            if (!string.IsNullOrEmpty(pos.ERFId))
            {
                ERFId = Convert.ToInt32(pos.ERFId);
            }

            WorkOrderId = pos.WorkOrderId;
            ModelNo = pos.ModelNo;

            ProdNo = pos.ProdNo;
            EquipmentType = pos.EquipmentType;
            UnitPrice = Convert.ToDouble(pos.UnitPrice);
            TransactionType = pos.TransactionType;
            Substitution = pos.Substitution;
            Extra = pos.Extra;
            Description = pos.Description;

            Quantity = pos.Quantity;
            Category = pos.ContingentCategoryId;
            Brand = pos.ContingentCategoryTypeId;
            Branch = pos.UsingBranch;
            LaidInCost = Convert.ToDouble(pos.LaidInCost);
            RentalCost = Convert.ToDouble(pos.RentalCost);
            TotalCost = Convert.ToDouble(pos.TotalCost);

            InternalOrderNumber = pos.InternalOrderType;
            VendorOrderNumber = pos.VendorOrderType;

        }


        public int? ERFPosId { get; set; }
        public int? ERFId { get; set; }
        public int? WorkOrderId { get; set; }
        public string ModelNo { get; set; }
        public int? Quantity { get; set; }
        public string ProdNo { get; set; }
        public string EquipmentType { get; set; }
        public double? UnitPrice { get; set; }
        public string TransactionType { get; set; }
        public string Substitution { get; set; }
        public string Extra { get; set; }
        public string Description { get; set; }

        //new Fields
        public int? Category { get; set; }
        public int? Brand { get; set; }
        public string Branch { get; set; }
        public double? LaidInCost { get; set; }
        public double? RentalCost { get; set; }
        public double? TotalCost { get; set; }

       public string InternalOrderNumber { get; set; }
       public string VendorOrderNumber { get; set; }
    }
}