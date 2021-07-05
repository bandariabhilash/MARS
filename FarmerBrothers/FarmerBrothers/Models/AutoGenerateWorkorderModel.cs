using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class AutoGenerateWorkorderModel
    {

         public AutoGenerateWorkorderModel()
        {

        }
         public AutoGenerateWorkorderModel(WorkOrder autoGenerateWorkorder)
        {
            
            this.WorkOrderID = autoGenerateWorkorder.WorkorderID;
            this.CustomerID = autoGenerateWorkorder.CustomerID;
            this.callReason = autoGenerateWorkorder.WorkorderCalltypeDesc;
            this.callReasonId = autoGenerateWorkorder.WorkorderCalltypeid;
            this.UserName = "WEB";
            this.CreatedDate = autoGenerateWorkorder.WorkorderEntryDate;
        }
        public NotesModel Notes { get; set; }
        public CustomerModel Customer { get; set; }
        public List<WorkorderType> WorkorderTypes { get; set; }
        public WorkOrderManagementSubmitType Operation { get; set; }
        public string CreatedBy { get; set; }
        public IList<NewNotesModel> NewNotes;
        public string callReason { get; set; }
        public int? callReasonId { get; set; }
        public string EquipmentLocation { get; set; }

        
        
        public int WorkOrderID { get; set; }
        public Nullable<int> CustomerID { get; set; }        
        public string UserName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }
}