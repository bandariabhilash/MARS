
using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace FarmerBrothers.Models
{
    public class ErfModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext,
                                ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();

            ErfModel model = new ErfModel();
            model.Customer = new CustomerModel();
            model.ErfAssetsModel = new ErfAssetsModel();
            model.ErfAssetsModel.Erf = new Data.Erf();

            model.Operation = (ERFManagementSubmitType)Convert.ToInt32(request.Unvalidated.Form.Get("Operation"));

            foreach (var property in model.Customer.GetType().GetProperties())
            {
                property.SetValue(model.Customer, request.Form.Get(property.Name));
            }
            model.ErfAssetsModel.Erf.ErfID = request.Form.Get("ErfAssetsModel.Erf.ErfID");

            model.Notes = new NotesModel();
            model.Notes.Notes = request.Form.Get("Notes");

            if (!string.IsNullOrWhiteSpace(request.Form["CrateWorkOrder"]))
                model.CrateWorkOrder = request.Form["CrateWorkOrder"].Contains("true");

            if (model.Operation == ERFManagementSubmitType.CREATEERF)
            {


                if (!string.IsNullOrWhiteSpace(request.Form.Get("ErfAssetsModel.Erf.DateOnERF")))
                    model.ErfAssetsModel.Erf.DateOnERF = Convert.ToDateTime(request.Form.Get("ErfAssetsModel.Erf.DateOnERF"));

                if (!string.IsNullOrWhiteSpace(request.Form.Get("ErfAssetsModel.Erf.DateERFReceived")))
                    model.ErfAssetsModel.Erf.DateERFReceived = Convert.ToDateTime(request.Form.Get("ErfAssetsModel.Erf.DateERFReceived"));
                if (!string.IsNullOrWhiteSpace(request.Form.Get("ErfAssetsModel.Erf.DateERFProcessed")))
                    model.ErfAssetsModel.Erf.DateERFProcessed = Convert.ToDateTime(request.Form.Get("ErfAssetsModel.Erf.DateERFProcessed"));
                if (!string.IsNullOrWhiteSpace(request.Form.Get("ErfAssetsModel.Erf.OriginalRequestedDate")))
                    model.ErfAssetsModel.Erf.OriginalRequestedDate = Convert.ToDateTime(request.Form.Get("ErfAssetsModel.Erf.OriginalRequestedDate"));

                if (!string.IsNullOrWhiteSpace(request.Form.Get("ErfAssetsModel.Erf.HoursofOperation")))
                    model.ErfAssetsModel.Erf.HoursofOperation = request.Form.Get("ErfAssetsModel.Erf.HoursofOperation");
                if (!string.IsNullOrWhiteSpace(request.Form.Get("ErfAssetsModel.Erf.InstallLocation")))
                    model.ErfAssetsModel.Erf.InstallLocation = request.Form.Get("ErfAssetsModel.Erf.InstallLocation");
                if (!string.IsNullOrWhiteSpace(request.Form.Get("ErfAssetsModel.Erf.UserName")))
                    model.ErfAssetsModel.Erf.UserName = request.Form.Get("ErfAssetsModel.Erf.UserName");
                if (!string.IsNullOrWhiteSpace(request.Form.Get("ErfAssetsModel.Erf.Phone")))
                    model.ErfAssetsModel.Erf.Phone = request.Form.Get("ErfAssetsModel.Erf.Phone");

                if (!string.IsNullOrWhiteSpace(request.Form.Get("SiteReady")))
                    model.ErfAssetsModel.Erf.SiteReady = request.Form.Get("SiteReady");

                if (!string.IsNullOrWhiteSpace(request.Form.Get("OrderType")))
                    model.OrderType = request.Form.Get("OrderType");

                if (!string.IsNullOrWhiteSpace(request.Form.Get("BranchName")))
                    model.BranchName = request.Form.Get("BranchName");

                if (!string.IsNullOrWhiteSpace(request.Form.Get("ShipToCustomer")))
                    model.ShipToCustomer = Convert.ToInt32(request.Form.Get("ShipToCustomer"));

                if (!string.IsNullOrWhiteSpace(request.Form.Get("TotalNSV")))
                    model.TotalNSV = Convert.ToDecimal(request.Form.Get("TotalNSV"));

                if (!string.IsNullOrWhiteSpace(request.Form.Get("CurrentNSV")))
                    model.CurrentNSV = Convert.ToDecimal(request.Form.Get("CurrentNSV"));

                if (!string.IsNullOrWhiteSpace(request.Form.Get("ContributionMargin")))
                    model.ContributionMargin = request.Form.Get("ContributionMargin");

                if (!string.IsNullOrWhiteSpace(request.Form.Get("CurrentEquipmentTotal")))
                    model.CurrentEquipmentTotal = Convert.ToDecimal(request.Form.Get("CurrentEquipmentTotal"));

                if (!string.IsNullOrWhiteSpace(request.Form.Get("AdditionalEquipmentTotal")))
                    model.AdditionalEquipmentTotal = Convert.ToDecimal(request.Form.Get("AdditionalEquipmentTotal"));

                if (!string.IsNullOrWhiteSpace(request.Form.Get("ApprovalStatus")))
                    model.ApprovalStatus = request.Form.Get("ApprovalStatus");


            }       
            else
            {
                //This block to Save ERF
                string workOrderId = request.Form.Get("Erf.WorkorderID");
                if (!string.IsNullOrWhiteSpace(workOrderId))
                {
                    model.ErfAssetsModel.Erf.WorkorderID = new Nullable<int>(Convert.ToInt32(workOrderId));
                }

                string feastMovementId = request.Form.Get("Erf.FeastMovementID");
                if (!string.IsNullOrWhiteSpace(feastMovementId))
                {
                    model.ErfAssetsModel.Erf.FeastMovementID = new Nullable<int>(Convert.ToInt32(feastMovementId));
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("FBERFEquipmentDetailsHidden")))
            {
                model.ErfAssetsModel.EquipmentList = json_serializer.Deserialize<IList<ERFManagementEquipmentModel>>(request.Unvalidated.Form.Get("FBERFEquipmentDetailsHidden"));
            }
            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("FBERFExpendableDetailsHidden")))
            {
                model.ErfAssetsModel.ExpendableList = json_serializer.Deserialize<IList<ERFManagementExpendableModel>>(request.Unvalidated.Form.Get("FBERFExpendableDetailsHidden"));
            }


            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ERFNotesHidden")))
            {
                model.NewNotes = json_serializer.Deserialize<IList<NewNotesModel>>(request.Unvalidated.Form.Get("ERFNotesHidden"));
            }
            else
            {
                model.NewNotes = new List<NewNotesModel>();
            }

          
            return model;
        }
    }
}