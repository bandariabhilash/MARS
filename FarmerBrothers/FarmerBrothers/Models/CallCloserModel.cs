using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class CallCloserModel
    {
        public CallCloserModel()
        { }

        public CallCloserModel(WorkorderSchedule workOrderSchedule, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            this.WorkOrderId = workOrderSchedule.WorkorderID;
            this.TechId = workOrderSchedule.Techid;
            this.WorkorderCalltypeid = workOrderSchedule.WorkOrder.WorkorderCalltypeid;
            this.WorkorderCalltypeDesc = workOrderSchedule.WorkOrder.WorkorderCalltypeDesc;//WorkOrderLookup.GetWorkOrderTypesById(Convert.ToInt32(this.WorkorderCalltypeid), FarmerBrothersEntitites);
            this.WorkOrderCallStatus = workOrderSchedule.WorkOrder.WorkorderCallstatus;
            this.CustomerName = workOrderSchedule.WorkOrder.CustomerName;
            this.CustomerId = workOrderSchedule.WorkOrder.CustomerID;
            this.CustomerCity = workOrderSchedule.WorkOrder.CustomerCity;
            this.CustomerState = workOrderSchedule.WorkOrder.CustomerState;
            this.AppointmentDate = workOrderSchedule.WorkOrder.AppointmentDate == null ? null : workOrderSchedule.WorkOrder.AppointmentDate.ToString();
            this.EntryDate = workOrderSchedule.WorkOrder.WorkorderEntryDate == null ? null : workOrderSchedule.WorkOrder.WorkorderEntryDate.ToString();
            this.DispatchDate = workOrderSchedule.ModifiedScheduleDate == null ? null : workOrderSchedule.ModifiedScheduleDate.ToString();
            this.SLACountDown = (DateTime.UtcNow - Convert.ToDateTime(this.DispatchDate)).Days;
            this.ScheduledDate = workOrderSchedule.EventScheduleDate == null ? null : workOrderSchedule.AssignedStatus == "Scheduled" ? workOrderSchedule.EventScheduleDate.ToString() : null;
            using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
            {
                this.EquipmentCount = entity.WorkorderEquipments.Where(wr => wr.WorkorderID == this.WorkOrderId).Count();
            }
            
        }

        public int? WorkOrderId { get; set; }
        public int? TechId { get; set; }
        public int? WorkorderCalltypeid { get; set; }
        public string WorkorderCalltypeDesc { get; set; }
        public string WorkOrderCallStatus { get; set; }    
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string AppointmentDate { get; set; }
        public string EntryDate { get; set; }
        public string DispatchDate { get; set; }
        public int SLACountDown { get; set; }
        public int EquipmentCount { get; set; }
        public string ScheduledDate { get; set; }

    }
}