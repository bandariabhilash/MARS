using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class ERFManagementExpendableModel
    {
       public ERFManagementExpendableModel()
        { }

       public ERFManagementExpendableModel(FBERFExpendable expendable)
        {

            ERFExpendableId = expendable.ERFExpendableId;
            if (!string.IsNullOrEmpty(expendable.ERFId))
            {
                ERFId = Convert.ToInt32(expendable.ERFId);
            }

            WorkOrderId = expendable.WorkOrderId;
            ModelNo = expendable.ModelNo;
            Quantity = expendable.Quantity;
            ProdNo = expendable.ProdNo;
            UnitPrice = Convert.ToDouble(expendable.UnitPrice);
            TransactionType = expendable.TransactionType;
            Extra = expendable.Extra;
            Description = expendable.Description;
            Substitution = expendable.Substitution;
            EquipmentType = expendable.EquipmentType;
            Quantity = expendable.Quantity;
            Category = expendable.ContingentCategoryId;
            Brand = expendable.ContingentCategoryTypeId;
            Branch = expendable.UsingBranch;
            LaidInCost = Convert.ToDouble(expendable.LaidInCost);
            RentalCost = Convert.ToDouble(expendable.RentalCost);
            TotalCost = Convert.ToDouble(expendable.TotalCost);

            InternalOrderNumber = expendable.InternalOrderType;
            VendorOrderNumber = expendable.VendorOrderType;
        }




       public int? ERFExpendableId { get; set; }
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