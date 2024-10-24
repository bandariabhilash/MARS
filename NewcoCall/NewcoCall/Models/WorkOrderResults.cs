using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FBCall.Models
{
    public class WorkOrderResults
    {
        public bool success { get; set; }
        public string Url { get; set; }
        public int? WorkOrderId { get; set; }
        public int returnValue { get; set; }
        public string message { get; set; }
    }
}