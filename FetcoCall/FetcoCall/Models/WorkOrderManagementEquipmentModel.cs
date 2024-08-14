using FetcoCall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FBCall.Models
{
    public class WorkOrderManagementEquipmentModel
    {
        public WorkOrderManagementEquipmentModel()
        {
        }

        public WorkOrderManagementEquipmentModel(WorkorderEquipment workOrderEquipment, FetcoEntities FetcoEntities)
        {
            Manufacturer = workOrderEquipment.Manufacturer;
            Solution = workOrderEquipment.Solutionid;
            Temperature = workOrderEquipment.Temperature;
            Settings = workOrderEquipment.Settings;
            Counter = workOrderEquipment.WorkPerformedCounter;
            System = workOrderEquipment.Systemid;
            SymptomID = workOrderEquipment.Symptomid;
            QualityIssue = workOrderEquipment.QualityIssue;
            Email = workOrderEquipment.Email;
            WorkPerformed = workOrderEquipment.WorkDescription;
            if (workOrderEquipment.NoPartsNeeded.HasValue)
            {
                NoPartsNeeded = workOrderEquipment.NoPartsNeeded.Value;
            }
            else
            {
                NoPartsNeeded = false;
            }

            AssetId = workOrderEquipment.Assetid;
            Location = workOrderEquipment.Location;
            CatelogID = workOrderEquipment.CatalogID;
            Model = workOrderEquipment.Model;
            SerialNumber = workOrderEquipment.SerialNumber;
            CallTypeID = workOrderEquipment.CallTypeid;
            Category = workOrderEquipment.Category;
            IsSlNumberImageExist = workOrderEquipment.IsSlNumberImageExist;

            Parts = new List<WorkOrderPartModel>();
            IQueryable<WorkorderPart> workOrderParts = FetcoEntities.WorkorderParts.Where(wp => wp.AssetID == AssetId);
            foreach (WorkorderPart workOrderPart in workOrderParts)
            {
                WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);
                Parts.Add(workOrderPartModel);
            }

            if (workOrderEquipment.WorkorderInstallationSurveys != null && workOrderEquipment.WorkorderInstallationSurveys.Count > 0)
            {
                WorkorderInstallationSurvey survey = workOrderEquipment.WorkorderInstallationSurveys.ElementAt(0);
                if (survey != null)
                {
                    NemwNumber = survey.NemwNumber;
                    ElectricalPhase = survey.ElectricalPhase;
                    MachineAmperage = survey.MachineAmperage;
                    UnitFitSpace = survey.UnitFitSpace;
                    Voltage = survey.Voltage;
                    CounterUnitSpace = survey.CounterUnitSpace;
                    WaterLine = survey.WaterLine;
                    AssetLocation = survey.AssetLocation;
                    Comments = survey.Comments;
                }
            }

            FeastMovementId = workOrderEquipment.FeastMovementid;
        }

        public WorkOrderManagementEquipmentModel(WorkorderEquipmentRequested workOrderEquipment, FetcoEntities FetcoEntities)
        {
            Manufacturer = workOrderEquipment.Manufacturer;
            Solution = workOrderEquipment.Solutionid;
            Temperature = workOrderEquipment.Temperature;
            Settings = workOrderEquipment.Settings;
            Counter = workOrderEquipment.WorkPerformedCounter;
            System = workOrderEquipment.Systemid;
            SymptomID = workOrderEquipment.Symptomid;
            QualityIssue = workOrderEquipment.QualityIssue;
            Email = workOrderEquipment.Email;
            WorkPerformed = workOrderEquipment.WorkDescription;
            if (workOrderEquipment.NoPartsNeeded.HasValue)
            {
                NoPartsNeeded = workOrderEquipment.NoPartsNeeded.Value;
            }
            else
            {
                NoPartsNeeded = false;
            }

            AssetId = workOrderEquipment.Assetid;
            Location = workOrderEquipment.Location;
            CatelogID = workOrderEquipment.CatalogID;
            Model = workOrderEquipment.Model;
            SerialNumber = workOrderEquipment.SerialNumber;
            CallTypeID = workOrderEquipment.CallTypeid;
            Category = workOrderEquipment.Category;
            IsSlNumberImageExist = workOrderEquipment.IsSlNumberImageExist;

            FeastMovementId = workOrderEquipment.FeastMovementid;
        }

        //public WorkOrderManagementEquipmentModel(SerializedItem serializedItem, int? callTypeId)
        //{
        //    Manufacturer = serializedItem.Manufacturer.Trim().ToUpper();
        //    Location = serializedItem.InstallLocation.Trim().ToUpper();
        //    CatelogID = serializedItem.CatalogID;
        //    Model = serializedItem.Model.Trim().ToUpper();
        //    SerialNumber = serializedItem.SerialNumber.Trim().ToUpper();
        //    Category = serializedItem.Category.Trim().ToUpper();
        //    CallTypeID = callTypeId;
        //    if (!string.IsNullOrWhiteSpace(serializedItem.FeastMovementID))
        //    {
        //        FeastMovementId = Convert.ToInt32(serializedItem.FeastMovementID);
        //    }
        //}

        public int? SequenceNumber { get; set; }
        public int? AssetId { get; set; }
        public int? CallTypeID { get; set; }
        public string Category { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Location { get; set; }
        public string SerialNumber { get; set; }
        public string CatelogID { get; set; }
        public int? Solution { get; set; }
        public string Temperature { get; set; }
        public string Settings { get; set; }
        public string Counter { get; set; }

        public int? System { get; set; }
        public int? SymptomID { get; set; }

        public bool? QualityIssue { get; set; }
        public string Email { get; set; }
        public string WorkPerformed { get; set; }
        public bool? NoPartsNeeded { get; set; }

        public int Installsurveyid { get; set; }
        public string NemwNumber { get; set; }
        public string ElectricalPhase { get; set; }
        public string MachineAmperage { get; set; }
        public string UnitFitSpace { get; set; }
        public string Voltage { get; set; }
        public string CounterUnitSpace { get; set; }
        public string WaterLine { get; set; }
        public string AssetLocation { get; set; }
        public string Comments { get; set; }
        public bool? IsSlNumberImageExist { get; set; }

        public int? FeastMovementId { get; set; }

        public IList<WorkOrderPartModel> Parts;
    }
}