using FetcoCall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FBCall.Models
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

        public int? PartsIssueid { get; set; }
        public int? Quantity { get; set; }
        public string Manufacturer { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public bool? Issue { get; set; }
    }
}