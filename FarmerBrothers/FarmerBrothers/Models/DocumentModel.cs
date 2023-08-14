using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class DocumentModel
    {
        public int WorkOrderID { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string UserName { get; set; }
        public List<WorkorderDocument> WorkorderDocuments { get; set; }
        public bool isNewEvent { get; set; }
    }

    public class WorkorderDocument
    {   
        public string Name { get; set; }
        public string Path { get; set; }
    }
}