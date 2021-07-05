using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FarmerBrothers.Data;
using System.ComponentModel.DataAnnotations;

namespace FarmerBrothers.Models
{
    public class NonServiceEventModel
    {
        public NonServiceEventModel()
        {

        }
        public NonServiceEventModel(NonServiceworkorder nonServiceWorkorder)
        {
            this.NonServiceID = nonServiceWorkorder.NonServiceID;
            this.WorkOrderID = nonServiceWorkorder.WorkOrderID;
            this.CustomerID = nonServiceWorkorder.CustomerID;
            this.callReason = nonServiceWorkorder.CallReason;
            this.UserId = nonServiceWorkorder.CreatedBy;
            this.CreatedDate = nonServiceWorkorder.CreatedDate;
            this.CallerName = nonServiceWorkorder.CallerName;
            this.CallBack = nonServiceWorkorder.CallBack;
        }
        public NotesModel Notes { get; set; }
        public CustomerModel Customer { get; set; }
        public List<FBCallReason> FBCallReasons { get; set; }
        //public NonServiceworkorder NonServiceWorkOrder { get; set; }
        public WorkOrderManagementSubmitType Operation { get; set; }
        public string CreatedBy { get; set; }
        public IList<NewNotesModel> NewNotes;
        public string callReason { get; set; }

        public int NonServiceID { get; set; }
        public int WorkOrderID { get; set; }
        public Nullable<int> CustomerID { get; set; }        
        public Nullable<int> UserId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        [MaxLength(100, ErrorMessage = "Caller Name cannot be longer than 100 characters.")]
        public string CallerName { get; set; }
        [MaxLength(20, ErrorMessage = "Call Back cannot be longer than 20 characters.")]
        public string CallBack { get; set; }

        public string Status { get; set; }
        public Nullable<System.DateTime> CloseDate { get; set; }

        public string MainContatName { get; set; }
        public string PhoneNumber { get; set; }
    }
}