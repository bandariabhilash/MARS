using FarmerBrothers.Data;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
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

            Contact cnct = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrderSchedule.WorkOrder.CustomerID).FirstOrDefault();

            this.Address1 = cnct == null ? "" : (cnct.Address1==null ? "" : cnct.Address1);
            this.Address2 = cnct == null ? "" : (cnct.Address2==null ? "" : cnct.Address2);
            this.CustomerPO = workOrderSchedule.WorkOrder.CustomerPO == null ? null : workOrderSchedule.WorkOrder.CustomerPO.ToString();
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
                this.TechName = entity.TECH_HIERARCHY.Where(t => t.DealerId == workOrderSchedule.Techid).Select(s => s.CompanyName).FirstOrDefault();
            }

            string url = ConfigurationManager.AppSettings["DispatchResponseUrl"];
            if (workOrderSchedule.WorkOrder.WorkorderCallstatus.ToLower() == "pending acceptance")
            {
                this.AcceptUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrderSchedule.WorkorderID + "&techId=" + workOrderSchedule.Techid + "&response=0&isResponsible=true"));
                this.RescheduleUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrderSchedule.WorkorderID + "&techId=" + workOrderSchedule.Techid + "&response=8&isResponsible=true"));
            }           
            if (workOrderSchedule.WorkOrder.WorkorderCallstatus.ToLower() == "accepted")
            {
                this.StartUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrderSchedule.WorkorderID + "&techId=" + workOrderSchedule.Techid + "&response=6&isResponsible=true"));
                this.ArrivelUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrderSchedule.WorkorderID + "&techId=" + workOrderSchedule.Techid + "&response=2&isResponsible=true"));
                this.CompleteUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrderSchedule.WorkorderID + "&techId=" + workOrderSchedule.Techid + "&response=3&isResponsible=true"));
                this.RescheduleUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrderSchedule.WorkorderID + "&techId=" + workOrderSchedule.Techid + "&response=8&isResponsible=true"));
            }
            if (workOrderSchedule.WorkOrder.WorkorderCallstatus.ToLower() == "on site")
            {
                this.CompleteUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrderSchedule.WorkorderID + "&techId=" + workOrderSchedule.Techid + "&response=3&isResponsible=true"));
            }

        }

        public int? WorkOrderId { get; set; }
        public int? TechId { get; set; }
        public string TechName { get; set; }
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
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string CustomerPO { get; set; }

        public string AcceptUrl { get; set; }
        public string StartUrl { get; set; }
        public string ArrivelUrl { get; set; }
        public string CompleteUrl { get; set; }
        public string RescheduleUrl { get; set; }

    }
}