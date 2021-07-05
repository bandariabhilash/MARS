using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Smuckers.Models
{
    public class WorkorderEquipment
    {
        public WorkorderEquipment()
        {
        }

        public int Assetid { get; set; }
        public Nullable<int> CallTypeid { get; set; }
        public string Category { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Location { get; set; }
        public string SerialNumber { get; set; }
        public Nullable<int> Solutionid { get; set; }
        public string Temperature { get; set; }
        public string Settings { get; set; }
        public Nullable<int> Systemid { get; set; }
        public Nullable<int> Symptomid { get; set; }
        public Nullable<bool> QualityIssue { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string WorkDescription { get; set; }
        public Nullable<int> WorkorderID { get; set; }
        public string WorkPerformedCounter { get; set; }
        public Nullable<bool> NoPartsNeeded { get; set; }

        public string WorkOrderType { get; set; }
        public string SolutionDesc { get; set; }
        public string SymptomDesc { get; set; }
        public string SystemDesc { get; set; }

    }
}