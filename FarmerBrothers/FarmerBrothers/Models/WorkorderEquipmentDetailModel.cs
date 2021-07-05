using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class WorkorderEquipmentDetailModel
    {
        public WorkorderEquipmentDetailModel()
        {
            //Parts = new List<WorkOrderPartModel>();
            //IQueryable<WorkorderPart> workOrderParts = FormalBrothersEntities.WorkorderParts.Where(wp => wp.AssetID == AssetId);
            //foreach (WorkorderPart workOrderPart in workOrderParts)
            //{
            //    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);
            //    Parts.Add(workOrderPartModel);
            //}
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
        public string Weight { get; set; }
        public string Ratio { get; set; }
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

        public IList<WorkOrderPartModel> Parts;
    }
}