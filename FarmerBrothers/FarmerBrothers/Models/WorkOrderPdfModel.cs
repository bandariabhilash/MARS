using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FarmerBrothers.Data;

namespace FarmerBrothers.Models
{
    public class WorkOrderPdfModel
    {
        public WorkOrder objWorkOrder { get; set; }
        public WorkorderSchedule objWorkorderSchedule { get; set; }
        public Invoice Invoice { get; set; }
        public string CustomerSign { get; set; }
        public string TechnicianSign { get; set; }
        public string CustomerSignatureBy { get; set; }

        public List<WorkorderEquipmentDetailModel> Equipmentlist;
        public List<List<WorkorderEquipmentDetailModel>> EquipmentSuperlist;

        public List<WorkorderEquipmentDetailModel> EquipmentRequestedlist;
        public List<List<WorkorderEquipmentDetailModel>> EquipmentRequestedSuperlist;

        public WorkorderDetail objWorkorderDetails { get; set; }
        public decimal PartsTotal { get; set; }
        public decimal LaborCost { get; set; }
        public decimal Total { get; set; }
        public decimal Tax { get; set; }
        public decimal TravelRate { get; set; }
        public decimal BalanceDue { get; set; }

        public string MachineNotes { get; set; }
        public string TaxPercentage { get; set; }

        public string StartTime { get; set; }
        public string ArrivalTime { get; set; }
        public string CompletionTime { get; set; }
        public string TravelTime { get; set; }
        public string CallPriority { get; set; }

    }
}