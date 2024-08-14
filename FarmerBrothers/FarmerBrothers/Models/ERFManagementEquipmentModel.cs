using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class ERFManagementEquipmentModel
    {
        public ERFManagementEquipmentModel()
        { }

        public ERFManagementEquipmentModel(FBERFEquipment equipment)
        {

            ERFEquipmentId = equipment.ERFEquipmentId;
            if (!string.IsNullOrEmpty(equipment.ERFId))
            {
                ERFId = Convert.ToInt32(equipment.ERFId);
            }

            WorkOrderId = equipment.WorkOrderId;
            ModelNo = equipment.ModelNo;
            
            ProdNo = equipment.ProdNo;
            EquipmentType = equipment.EquipmentType;
            UnitPrice = Convert.ToDouble(equipment.UnitPrice);
            TransactionType = equipment.TransactionType;
            Substitution = equipment.Substitution;
            Extra = equipment.Extra;
            Description = equipment.Description;

            Quantity = equipment.Quantity;
            Category = equipment.ContingentCategoryId;
            Brand = equipment.ContingentCategoryTypeId;
            Branch = equipment.UsingBranch;
            LaidInCost = Convert.ToDouble(equipment.LaidInCost);
            RentalCost = Convert.ToDouble(equipment.RentalCost);
            TotalCost = Convert.ToDouble(equipment.TotalCost);

            InternalOrderNumber = equipment.InternalOrderType;
            VendorOrderNumber = equipment.VendorOrderType;

            SerialNumber = equipment.SerialNumber != null ? equipment.SerialNumber : "";
            OrderType = equipment.OrderType != null ? equipment.OrderType : "";
            DepositInvoiceNumber = equipment.DepositInvoiceNumber != null ? equipment.DepositInvoiceNumber : "";
            DepositAmount = equipment.DepositAmount != null ? equipment.DepositAmount.ToString() : "";
            FinalInvoceNumber = equipment.FinalInvoiceNumber != null ? equipment.FinalInvoiceNumber : "";
            InvoiceTotal = equipment.InvoiceTotal != null ? equipment.InvoiceTotal.ToString() : "";

        }

       
        public int? ERFEquipmentId { get; set; }
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
        public string Tracking { get; set; }

        public string SerialNumber { get; set; }
        public string OrderType { get; set; }
        public string DepositInvoiceNumber { get; set; }
        public string DepositAmount { get; set; }
        public string FinalInvoceNumber { get; set; }
        public string InvoiceTotal { get; set; }
    }
}