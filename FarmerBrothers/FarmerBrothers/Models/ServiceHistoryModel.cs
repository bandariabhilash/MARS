
using System;
namespace FarmerBrothers.Models
{
    public class ServiceHistoryModel
    {
        public string WorkOrderID { get; set; }
        public string WorkOrderType { get; set; }
        public string WorkOrderStatus { get; set; }
        public string DateCreated { get; set; }
        public string AppointmentDate { get; set; }
        public string Technician { get; set; }
    }

    public class EquipmentSummaryModel
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemDescription { get; set; }
        public string SerialNumber { get; set; }
        public string AssetStatus { get; set; }
        public string InitialDate { get; set; }
        public string TransDate { get; set; }
        public decimal Age { get; set; }
        public decimal YearsInService { get; set; }
    }
}