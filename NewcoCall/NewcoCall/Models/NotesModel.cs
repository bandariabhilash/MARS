using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewcoCall.Models
{
    public class NotesModel
    {
        public IList<NotesHistoryModel> RecordHistory;
        public IList<NotesHistoryModel> NotesHistory;
        public IList<AllFBStatu> FollowUpRequestList;
        public IList<CustomerNotesModel> CustomerNotesResults;

        [AllowHtml]
        public string Notes { get; set; }
        public int? ProjectNumber { get; set; }
        public string CustomerZipCode { get; set; }
        public string CustomerID { get; set; }
        public string ErfID { get; set; }
        public int WorkOrderID { get; set; }
        public string FollowUpRequestID { get; set; }
        public decimal? ProjectFlatRate { get; set; }

        public string WorkOrderStatus { get; set; }

        public IEnumerable<TechHierarchyView> Technicianlist { get; set; }
        public bool IsSpecificTechnician { get; set; }
        public string TechID { get; set; }
        public string PreferredProvider { get; set; }

        public string viewProp { get; set; }

        public bool isFromAutoGenerateWorkOrder { get; set; }

        public bool IsAutoDispatched { get; set; }
    }
}