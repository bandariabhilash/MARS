using FarmerBrothers.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FarmerBrothers.Models
{
    public class WorkOrderPartModel
    {
        public WorkOrderPartModel()
        { }

        public WorkOrderPartModel(WorkorderPart workOrderPart)
        {
            PartsIssueid = workOrderPart.PartsIssueid;
            Quantity = workOrderPart.Quantity;
            if (!string.IsNullOrWhiteSpace(workOrderPart.Manufacturer))
            {
                Manufacturer = workOrderPart.Manufacturer.ToUpper().Trim();
            }
            if (!string.IsNullOrWhiteSpace(workOrderPart.Sku))
            {
                Sku = workOrderPart.Sku.ToUpper().Trim();
            }
            if (!string.IsNullOrWhiteSpace(workOrderPart.Description))
            {
                Description = workOrderPart.Description;
            }
            Issue = workOrderPart.NonSerializedIssue;
        }

        [Key]
        public int? PartsIssueid { get; set; }
        public int? Quantity { get; set; }
        public string Manufacturer { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public bool? Issue { get; set; }
        public decimal skuCost { get; set; }
        public decimal partsTotal { get; set; }
        public bool? PartReplenish { get; set; }
    }

    public class UpdateWorkorderPartsModel
    {
        [Key]
        public int? PartsIssueid { get; set; }
        public bool? PartReplenish { get; set; }
        public int? Quantity { get; set; }
        public string Manufacturer { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public bool? Issue { get; set; }
        public decimal skuCost { get; set; }
        public decimal partsTotal { get; set; }
        public string UpdateType { get; set; }
        public List<WorkOrderPartModel> PartsList { get; set; }
    }

    public class BillingModel
    {
        [Key]
        public int Id { get; set; }
        public string BillingType { get; set; }
        public string BillingCode { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal Total { get; set; }
        public string Duration { get; set; }
    }

    public class UpdateBillingModel
    {
        [Key]
        public int Id { get; set; }
        public string BillingType { get; set; }
        public string BillingCode { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal Total { get; set; }
        public string Duration { get; set; }
        public bool IsManualOverride { get; set; }
        public List<BillingModel> BillingList { get; set; }
    }
}