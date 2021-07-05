using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FarmerBrothers.Data;

namespace FarmerBrothers.Models
{
    public class DisplayPdf
    {
        public WorkOrder objWorkOrder { get; set; }
        public CustomerModel customerModel { get; set; }
        public Invoice Invoice { get; set; }
        public ThirdPartyContractMaintenance ThirdPartyContractMaintenance { get; set; }
        public List<ThirdpartyConMaintenanceZonerate> ThirdpartyConMaintenanceZonerate { get; set; }
        public List<WorkorderEquipmentModel> Equipmentlist;
        public string ZoneDescription { get; set; }
        public string PhoneNumber { get; set; }
        public string TravelTime { get; set; }
        public string LaborHours { get; set; }
        public string StandardLabor { get; set; }
        public string OvertimeLabor { get; set; }
    }
}