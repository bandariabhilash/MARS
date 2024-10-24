using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FBCall.Models
{
    public class WorkOrderClosureModel
    {
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<System.DateTime> ArrivalDateTime { get; set; }
        public Nullable<System.DateTime> CompletionDateTime { get; set; }

        public string SpecialClosure { get; set; }
        public string InvoiceNo { get; set; }
        public string ResponsibleTechName { get; set; }
        public Nullable<decimal> Mileage { get; set; }
        [MaxLength(90, ErrorMessage = "Customer Name cannot be longer than 90 characters.")]
        public string CustomerName { get; set; }
        [MaxLength(70, ErrorMessage = "Customer Email cannot be longer than 70 characters.")]
        public string CustomerEmail { get; set; }
        public string CustomerSignatureDetails { get; set; }
        public string TravelHours;
        public string TravelMinutes;
        public int? PhoneSolveid { get; set; }
        public IList<WorkOrderManagementEquipmentModel> WorkOrderEquipments;
    }
}