using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using FarmerBrothersMailResponse.Controllers;
using LinqKit;
using Syncfusion.EJ.Export;
using Syncfusion.HtmlConverter;
using Syncfusion.JavaScript.Models;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Customer = FarmerBrothers.Data.Contact;

namespace FarmerBrothers.Controllers
{
    public enum MailType
    {
        INFO,
        DISPATCH,
        REDIRECTED,
        SPAWN,
        SALESNOTIFICATION
    }

    public class WorkorderController : BaseController
    {

        int defaultFollowUpCall;

        public WorkorderController()
        {
            AllFBStatu FarmarBortherStatus = FarmerBrothersEntitites.AllFBStatus.Where(a => a.FBStatus == "None" && a.StatusFor == "Follow Up Call").FirstOrDefault();
            if (FarmarBortherStatus != null)
            {
                defaultFollowUpCall = FarmarBortherStatus.FBStatusID;
            }
        }

        #region load workorder view

        [HttpGet]
        public ActionResult WorkorderManagement(int? customerId, int? workOrderId, bool isNewPartsOrder = false, bool showAllTechs = false, bool isCustomerDashboard = false, bool isFromProcessCardScreen= false)
        {
            WorkorderManagementModel workOrderManagementModel = null;
            try
            {
                workOrderManagementModel = ConstructWorkorderManagementModel(customerId, workOrderId, isNewPartsOrder, showAllTechs, isCustomerDashboard, isFromProcessCardScreen);

            }
            catch (Exception ex)
            {
                int tmpCustomerId = Convert.ToInt32(customerId);
                string DBCustomerZipCode = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == tmpCustomerId).Select(id => id.PostalCode).FirstOrDefault();

                var zip = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == DBCustomerZipCode).FirstOrDefault();
                if (zip == null)
                {
                    return View("InvalidZipCodeError");
                }
                else
                {
                    return View("Error");
                }

            }
            return View(workOrderManagementModel);
        }


        public ActionResult WorkorderPdf(int? WorkorderID)
        {
            WorkOrderPdfModel objDisplayPdf = new Models.WorkOrderPdfModel();

            objDisplayPdf.objWorkOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
            objDisplayPdf.objWorkorderSchedule = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == WorkorderID && w.AssignedStatus != "Redirected" && w.AssignedStatus != "Declined").FirstOrDefault();
            objDisplayPdf.Invoice = FarmerBrothersEntitites.Invoices.Where(w => w.InvoiceUniqueid == FarmerBrothersEntitites.Invoices.Where(i => i.WorkorderID == WorkorderID).FirstOrDefault().InvoiceUniqueid).FirstOrDefault();

            WorkorderDetail workOrderDetail = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
            if (workOrderDetail != null)
            {
                objDisplayPdf.CustomerSign = workOrderDetail.CustomerSignatureDetails;
                objDisplayPdf.TechnicianSign = workOrderDetail.TechnicianSignatureDetails;
                objDisplayPdf.CustomerSignatureBy = workOrderDetail.CustomerSignatureBy;
            }

            var getTechId = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();

            objDisplayPdf.Equipmentlist = (from WorkEquipment in FarmerBrothersEntitites.WorkorderEquipments
                                           join WorkType in FarmerBrothersEntitites.WorkorderTypes
                                           on WorkEquipment.CallTypeid equals WorkType.CallTypeID into temp1
                                           from we in temp1.DefaultIfEmpty()
                                           where WorkEquipment.WorkorderID == WorkorderID
                                           select new WorkorderEquipmentDetailModel()
                                           {
                                               Assetid = WorkEquipment.Assetid,
                                               WorkOrderType = we.Description,
                                               Temperature = WorkEquipment.Temperature,
                                               Weight = WorkEquipment.Weight,
                                               Ratio = WorkEquipment.Ratio,
                                               WorkPerformedCounter = WorkEquipment.WorkPerformedCounter,
                                               WorkDescription = WorkEquipment.WorkDescription,
                                               Category = WorkEquipment.Category,
                                               Manufacturer = WorkEquipment.Manufacturer,
                                               Model = WorkEquipment.Model,
                                               Location = WorkEquipment.Location,
                                               SerialNumber = WorkEquipment.SerialNumber,
                                               QualityIssue = WorkEquipment.QualityIssue,
                                               Email = WorkEquipment.Email
                                           }).ToList();

            IList<WorkOrderPartModel> Parts;

            foreach (var item in objDisplayPdf.Equipmentlist)
            {
                Parts = new List<WorkOrderPartModel>();
                IQueryable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.AssetID == item.Assetid);
                foreach (WorkorderPart workOrderPart in workOrderParts)
                {
                    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);
                    Parts.Add(workOrderPartModel);
                }

                item.Parts = Parts;
            }

            var workorderPdfId = objDisplayPdf.objWorkOrder.WorkorderID;
            string value = this.RenderRazorViewToString("~/Views/Workorder/WorkorderPdf.cshtml", objDisplayPdf).ToString();

            string baseURL = Server.MapPath("~/Views/Workorder/");

            // Html to PDF rendering using Webkit.
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            WebKitConverterSettings webKitSettings = new WebKitConverterSettings();
            webKitSettings.WebKitPath = Server.MapPath("~/Content/QtBinaries");
            webKitSettings.PdfPageSize = PdfPageSize.A4;
            webKitSettings.EnableJavaScript = true;
            webKitSettings.EnableHyperLink = false;
            htmlConverter.ConverterSettings = webKitSettings;

            PdfDocument pdfDoc = htmlConverter.Convert(value, Server.MapPath("~/Content/"));            
            pdfDoc.PageSettings.Size = PdfPageSize.A4;
            pdfDoc.Save("WorkorderFor_" + workorderPdfId + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

            return View(objDisplayPdf);
        }

        public ActionResult WorkorderPdfData(int? WorkorderID)
        {
            WorkOrderPdfModel objDisplayPdf = new Models.WorkOrderPdfModel();

            objDisplayPdf.objWorkOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
            objDisplayPdf.objWorkorderSchedule = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == WorkorderID && w.AssignedStatus != "Redirected" && w.AssignedStatus != "Declined").FirstOrDefault();
            objDisplayPdf.Invoice = FarmerBrothersEntitites.Invoices.Where(w => w.InvoiceUniqueid == FarmerBrothersEntitites.Invoices.Where(i => i.WorkorderID == WorkorderID).FirstOrDefault().InvoiceUniqueid).FirstOrDefault();

            WorkorderDetail workOrderDetail = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
            if (workOrderDetail != null)
            {
                objDisplayPdf.CustomerSign = workOrderDetail.CustomerSignatureDetails;
                objDisplayPdf.TechnicianSign = workOrderDetail.TechnicianSignatureDetails;
                objDisplayPdf.CustomerSignatureBy = workOrderDetail.CustomerSignatureBy;
            }

            Contact contact = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == objDisplayPdf.objWorkOrder.CustomerID).FirstOrDefault();

            objDisplayPdf.Route = contact == null ? "" : contact.Route;

            objDisplayPdf.objWorkorderDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
            //FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.CustomerSignatureDetails).FirstOrDefault();

            objDisplayPdf.EquipmentRequestedlist = (from WorkEquipment in FarmerBrothersEntitites.WorkorderEquipmentRequesteds
                                                    join WorkType in FarmerBrothersEntitites.WorkorderTypes                                                    
                                                    on WorkEquipment.CallTypeid equals WorkType.CallTypeID into temp1                                                    
                                                    from we in temp1.DefaultIfEmpty()
                                                    join Symptom in FarmerBrothersEntitites.Symptoms
                                                    on WorkEquipment.Symptomid equals Symptom.SymptomID into sympt
                                                    from sy in sympt.DefaultIfEmpty()
                                                    where WorkEquipment.WorkorderID == WorkorderID
                                                    select new WorkorderEquipmentDetailModel()
                                                    {
                                                        Assetid = WorkEquipment.Assetid,
                                                        WorkOrderType = we.Description,
                                                        Temperature = WorkEquipment.Temperature,
                                                        Weight = WorkEquipment.Weight,
                                                        Ratio = WorkEquipment.Ratio,
                                                        WorkPerformedCounter = WorkEquipment.WorkPerformedCounter,
                                                        WorkDescription = WorkEquipment.WorkDescription,
                                                        Category = WorkEquipment.Category,
                                                        Manufacturer = WorkEquipment.Manufacturer,
                                                        Model = WorkEquipment.Model,
                                                        Location = WorkEquipment.Location,
                                                        SerialNumber = WorkEquipment.SerialNumber,
                                                        QualityIssue = WorkEquipment.QualityIssue,
                                                        Email = WorkEquipment.Email,
                                                        SymptomDesc = sy.Description
                                                    }).ToList();

            IList<WorkOrderPartModel> Parts;


            foreach (var item in objDisplayPdf.EquipmentRequestedlist)
            {
                Parts = new List<WorkOrderPartModel>();
                IQueryable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.AssetID == item.Assetid);
                foreach (WorkorderPart workOrderPart in workOrderParts)
                {
                    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);

                    if (!string.IsNullOrEmpty(workOrderPartModel.Sku))
                    {
                        Sku sk = FarmerBrothersEntitites.Skus.Where(w => w.Sku1 == workOrderPartModel.Sku).FirstOrDefault();
                        if (sk != null)
                        {
                            workOrderPartModel.skuCost = sk.SKUCost == null ? 0 : Convert.ToDecimal(sk.SKUCost);
                        }
                    }
                    else
                    {
                        workOrderPartModel.skuCost = 0;
                    }

                    Parts.Add(workOrderPartModel);
                }

                item.Parts = Parts;
            }

            objDisplayPdf.EquipmentRequestedSuperlist = SplitToSubList(objDisplayPdf.EquipmentRequestedlist, 40);

            // for closure equipments
            objDisplayPdf.Equipmentlist = (from WorkEquipment in FarmerBrothersEntitites.WorkorderEquipments
                                           join WorkType in FarmerBrothersEntitites.WorkorderTypes
                                           on WorkEquipment.CallTypeid equals WorkType.CallTypeID into temp1
                                           from we in temp1.DefaultIfEmpty()
                                           join Solution in FarmerBrothersEntitites.Solutions
                                           on WorkEquipment.Solutionid equals Solution.SolutionId into solnum
                                           from soln in solnum.DefaultIfEmpty()
                                           where WorkEquipment.WorkorderID == WorkorderID
                                           select new WorkorderEquipmentDetailModel()
                                           {
                                               Assetid = WorkEquipment.Assetid,
                                               WorkOrderType = we.Description,
                                               Temperature = WorkEquipment.Temperature,
                                               Weight = WorkEquipment.Weight,
                                               Ratio = WorkEquipment.Ratio,
                                               WorkPerformedCounter = WorkEquipment.WorkPerformedCounter,
                                               WorkDescription = WorkEquipment.WorkDescription,
                                               Category = WorkEquipment.Category,
                                               Manufacturer = WorkEquipment.Manufacturer,
                                               Model = WorkEquipment.Model,
                                               Location = WorkEquipment.Location,
                                               SerialNumber = WorkEquipment.SerialNumber,
                                               QualityIssue = WorkEquipment.QualityIssue,
                                               Email = WorkEquipment.Email,
                                               SolutionDesc = soln.Description
                                           }).ToList();


            IList<WorkOrderPartModel> ClosureEquipmentParts = new List<WorkOrderPartModel>(); ;

            /*decimal partsTotal = 0;
            foreach (var item in objDisplayPdf.Equipmentlist)
            {
                ClosureEquipmentParts = new List<WorkOrderPartModel>();
                IQueryable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.AssetID == item.Assetid);
                foreach (WorkorderPart workOrderPart in workOrderParts)
                {
                    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);

                    if (!string.IsNullOrEmpty(workOrderPartModel.Sku))
                    {
                        Sku sk = FarmerBrothersEntitites.Skus.Where(w => w.Sku1 == workOrderPartModel.Sku).FirstOrDefault();
                        if(sk != null)
                        {
                            workOrderPartModel.skuCost = sk.SKUCost == null ? 0 : Convert.ToDecimal(sk.SKUCost);
                        }
                    }
                    else
                    {
                        workOrderPartModel.skuCost = 0;
                    }

                    workOrderPartModel.partsTotal =  Convert.ToDecimal(workOrderPartModel.skuCost * workOrderPartModel.Quantity);
                    partsTotal += workOrderPartModel.partsTotal;

                    ClosureEquipmentParts.Add(workOrderPartModel);
                }

                item.Parts = ClosureEquipmentParts;
            }*/



            //===================================================================================================

            string TravelTime = "", StartTime = "", ArrivalTime = "", CompletionTime = "";
            string trvlTime = "", ServiceTime = "";
            if (workOrderDetail != null)
            {
                /*trvlTime = workOrderDetail.TravelTime == null ? "" : workOrderDetail.TravelTime.ToString();
                if (!string.IsNullOrEmpty(trvlTime))
                {
                    if (trvlTime.Contains(':'))
                    {
                        string[] timeSpllit = trvlTime.Split(':');
                        string hours = string.IsNullOrEmpty(timeSpllit[0]) ? "0" : timeSpllit[0].Trim();
                        string minutes = string.IsNullOrEmpty(timeSpllit[1]) ? "0" : timeSpllit[1].Trim();

                        TravelTime = hours + " Hrs : " + minutes + " Min";
                    }
                    else
                    {
                        TravelTime = trvlTime.Trim();
                    }
                }*/

                StartTime = workOrderDetail.StartDateTime == null ? "" : workOrderDetail.StartDateTime.ToString().Trim();
                ArrivalTime = workOrderDetail.ArrivalDateTime == null ? "" : workOrderDetail.ArrivalDateTime.ToString().Trim();
                CompletionTime = workOrderDetail.CompletionDateTime == null ? "" : workOrderDetail.CompletionDateTime.ToString().Trim();

                if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(ArrivalTime))
                {
                    DateTime arrival = Convert.ToDateTime(ArrivalTime);
                    DateTime strt = Convert.ToDateTime(StartTime);
                    TimeSpan timeDiff = arrival.Subtract(strt);

                    trvlTime = timeDiff.Hours + " : " + timeDiff.Minutes;
                    TravelTime = timeDiff.Hours + " Hrs : " + timeDiff.Minutes + " Min";
                }
                else
                {
                    trvlTime = "0 : 0";
                    TravelTime = "0 Hrs : 0 Min";
                }

                if (!string.IsNullOrEmpty(CompletionTime) && !string.IsNullOrEmpty(ArrivalTime))
                {
                    DateTime arrival = Convert.ToDateTime(ArrivalTime);
                    DateTime cmplt = Convert.ToDateTime(CompletionTime);
                    TimeSpan servicetimeDiff = cmplt.Subtract(arrival);

                    ServiceTime = servicetimeDiff.Hours + " : " + servicetimeDiff.Minutes;
                }
                else
                {
                    ServiceTime = "0 : 0";
                }
            }

            objDisplayPdf.StartTime = StartTime;
            objDisplayPdf.ArrivalTime = ArrivalTime;
            objDisplayPdf.CompletionTime = CompletionTime;
            objDisplayPdf.TravelTime = TravelTime;

            objDisplayPdf.WarrentyFor = string.IsNullOrEmpty(workOrderDetail.WarrentyFor) ? "" : workOrderDetail.WarrentyFor;
            objDisplayPdf.StateOfEquipment = string.IsNullOrEmpty(workOrderDetail.StateofEquipment) ? "" : workOrderDetail.StateofEquipment;
            objDisplayPdf.serviceDelayed = string.IsNullOrEmpty(workOrderDetail.ServiceDelayReason) ? "" : workOrderDetail.ServiceDelayReason;
            objDisplayPdf.troubleshootSteps = string.IsNullOrEmpty(workOrderDetail.TroubleshootSteps) ? "" : workOrderDetail.TroubleshootSteps;
            objDisplayPdf.followupComments = string.IsNullOrEmpty(workOrderDetail.FollowupComments) ? "" : workOrderDetail.FollowupComments;
            objDisplayPdf.ReviewedBy = string.IsNullOrEmpty(workOrderDetail.ReviewedBy) ? "" : workOrderDetail.ReviewedBy;
            objDisplayPdf.IsUnderWarrenty = string.IsNullOrEmpty(workOrderDetail.IsUnderWarrenty) ? "" : workOrderDetail.IsUnderWarrenty;
            objDisplayPdf.AdditionalFollowup = string.IsNullOrEmpty(workOrderDetail.AdditionalFollowupReq) ? "" : workOrderDetail.AdditionalFollowupReq;
            objDisplayPdf.Operational = string.IsNullOrEmpty(workOrderDetail.IsOperational) ? "" : workOrderDetail.IsOperational;


            decimal partsTotal = 0; string machineNotes = "";
            foreach (var item in objDisplayPdf.Equipmentlist)
            {
                machineNotes += item.SerialNumber + " :- " + item.WorkDescription + System.Environment.NewLine;

                ClosureEquipmentParts = new List<WorkOrderPartModel>();
                IQueryable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.AssetID == item.Assetid);
                foreach (WorkorderPart workOrderPart in workOrderParts)
                {
                    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);

                    if (!string.IsNullOrEmpty(workOrderPartModel.Sku))
                    {
                        Sku sk = FarmerBrothersEntitites.Skus.Where(w => w.Sku1 == workOrderPartModel.Sku).FirstOrDefault();
                        if (sk != null)
                        {
                            workOrderPartModel.skuCost = sk.SKUCost == null ? 0 : Convert.ToDecimal(sk.SKUCost);
                        }
                    }
                    else
                    {
                        workOrderPartModel.skuCost = 0;
                    }

                    workOrderPartModel.partsTotal = Convert.ToDecimal(workOrderPartModel.skuCost * workOrderPartModel.Quantity);
                    partsTotal += workOrderPartModel.partsTotal;

                    ClosureEquipmentParts.Add(workOrderPartModel);
                }

                item.Parts = ClosureEquipmentParts;
            }

            decimal PartsTotal = Math.Round(partsTotal, 2);
            //double LaborCost = 112.50;// TotalUnitPrice == "" ? "0" : TotalUnitPrice;

            decimal LaborCost = 0;
            decimal TravelRate = 0;
            decimal rate = 75;
            if (!string.IsNullOrEmpty(trvlTime))
            {
                if (trvlTime.Contains(':'))
                {
                    string[] timeSpllit = trvlTime.Split(':');
                    string hours = string.IsNullOrEmpty(timeSpllit[0]) ? "0" : timeSpllit[0].Trim();
                    string minutes = string.IsNullOrEmpty(timeSpllit[1]) ? "0" : timeSpllit[1].Trim();

                    TravelRate = Math.Round(((Convert.ToDecimal(hours) * rate) + ((Convert.ToDecimal(minutes) / 60) * rate)), 2);
                }
                else
                {
                    TravelRate = Math.Round((Convert.ToDecimal(trvlTime) * rate), 2);
                }
            }
            if (!string.IsNullOrEmpty(ServiceTime))
            {
                if (ServiceTime.Contains(':'))
                {
                    string[] timeSpllit = ServiceTime.Split(':');
                    string hours = string.IsNullOrEmpty(timeSpllit[0]) ? "0" : timeSpllit[0].Trim();
                    string minutes = string.IsNullOrEmpty(timeSpllit[1]) ? "0" : timeSpllit[1].Trim();

                    LaborCost = Math.Round(((Convert.ToDecimal(hours) * rate) + ((Convert.ToDecimal(minutes) / 60) * rate)), 2);
                }
                else
                {
                    LaborCost = Math.Round((Convert.ToDecimal(ServiceTime) * rate), 2);
                }
            }

            decimal tmpTotal = Math.Round((partsTotal + TravelRate + Convert.ToDecimal(LaborCost)), 2);
            //===================================================================================================
            State state = FarmerBrothersEntitites.States.Where(st => st.StateCode == objDisplayPdf.objWorkOrder.CustomerState).FirstOrDefault();
            string taxValue = ""; decimal? taxcalculationValue = 0;
            if (state != null)
            {
                taxcalculationValue = state.TaxPercent == null ? 0 : state.TaxPercent;
                taxValue = taxcalculationValue + "%";
            }
            decimal TaxCostValue = Math.Round(Convert.ToDecimal((taxcalculationValue / 100)) * tmpTotal, 2);

            decimal Total = Math.Round((tmpTotal + TaxCostValue), 2);
            
            List<NotesHistory> NHList = FarmerBrothersEntitites.NotesHistories.Where(nt => nt.WorkorderID == WorkorderID).ToList();
            if (NHList != null)
            {
                foreach (NotesHistory nh in NHList)
                {
                    if (nh.Notes != null && nh.Notes.Contains("Comments"))
                    {
                        string[] commentStr = nh.Notes.Split(':');
                        machineNotes += string.IsNullOrEmpty(commentStr[1]) ? "" : (commentStr[1].Trim() + "\n");
                    }
                }
            }

            objDisplayPdf.MachineNotes = machineNotes;
            objDisplayPdf.PartsTotal = PartsTotal;
            objDisplayPdf.LaborCost = Convert.ToDecimal(LaborCost);
            objDisplayPdf.Tax = TaxCostValue;
            objDisplayPdf.TravelRate = TravelRate;
            objDisplayPdf.Total = Total;
            objDisplayPdf.BalanceDue = 0;
            if(Convert.ToBoolean(objDisplayPdf.objWorkOrder.IsBillable))
            {
                objDisplayPdf.BalanceDue = Total;
            }

            if (objDisplayPdf.objWorkOrder.PriorityCode != null)
            {
                AllFBStatu afb = FarmerBrothersEntitites.AllFBStatus.Where(f => f.FBStatusID == objDisplayPdf.objWorkOrder.PriorityCode).FirstOrDefault();
                objDisplayPdf.CallPriority = afb == null ? "" : afb.FBStatus;
            }
            else
            {
                objDisplayPdf.CallPriority = "";
            }

            //===================================================================================================



            //objDisplayPdf.PartsTotal = partsTotal;//ClosureEquipmentParts.Sum(sm => sm.partsTotal);
            //objDisplayPdf.LaborCost = objDisplayPdf.objWorkOrder.TotalUnitPrice == null ? 0 : Convert.ToDecimal(objDisplayPdf.objWorkOrder.TotalUnitPrice);
            //objDisplayPdf.Total = objDisplayPdf.PartsTotal + objDisplayPdf.LaborCost;

            objDisplayPdf.EquipmentSuperlist = SplitToSubList(objDisplayPdf.Equipmentlist, 5);

            var workorderPdfId = objDisplayPdf.objWorkOrder.WorkorderID;
            string htmlView = this.RenderRazorViewToString("~/Views/Workorder/WorkorderPdf.cshtml", objDisplayPdf).ToString();

            return Json(Content(htmlView), JsonRequestBehavior.AllowGet);
        }

        private List<List<WorkorderEquipmentDetailModel>> SplitToSubList(List<WorkorderEquipmentDetailModel> Source, int Size)
        {
            List<List<WorkorderEquipmentDetailModel>> resultSet = new List<List<WorkorderEquipmentDetailModel>>();

            List<WorkorderEquipmentDetailModel> tempList = new List<WorkorderEquipmentDetailModel>();
            int j = 0, itemCount = 0;
            foreach (WorkorderEquipmentDetailModel item in Source)
            {
                tempList.Add(item);
                j++;
                itemCount++;
                if (j == Size || itemCount == Source.Count())
                {
                    resultSet.Add(tempList);
                    j = 0;
                    tempList = new List<WorkorderEquipmentDetailModel>();
                }

            }

            return resultSet;
        }


        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public JsonResult GetBillableSkuDetails(string sku)
        {
            FbBillableSKU fbsku = FarmerBrothersEntitites.FbBillableSKUs.Where(s => s.SKU == sku && s.IsActive == true).FirstOrDefault();
            decimal? unitprice = fbsku.UnitPrice;
            string description = fbsku.SKUDescription;
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = unitprice, desc = description };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult BillableInsert(FbWorkorderBillableSKUModel value)
        {
            IList<FbWorkorderBillableSKUModel> SkuItems = TempData["Billable"] as IList<FbWorkorderBillableSKUModel>;
            if (SkuItems == null)
            {
                SkuItems = new List<FbWorkorderBillableSKUModel>();
            }            
            
            if (TempData["WorkOrderSKUId"] != null)
            {
                int eqpId = Convert.ToInt32(TempData["WorkOrderSKUId"]);
                value.WorkOrderSKUId = eqpId + 1;
                TempData["WorkOrderSKUId"] = eqpId + 1;
            }
            else
            {
                value.WorkOrderSKUId = 1;
                value.UnitPrice = Convert.ToDecimal(value.UnitPrice);
                TempData["WorkOrderSKUId"] = 1;
            }

            SkuItems.Add(value);
           
            TempData["Billable"] = SkuItems;
            TempData.Keep("Billable");
            return Json(value, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BillableUpdate(FbWorkorderBillableSKUModel value)
        {
            IList<FbWorkorderBillableSKUModel> SkuItems = TempData["Billable"] as IList<FbWorkorderBillableSKUModel>;
            if (SkuItems == null)
            {
                SkuItems = new List<FbWorkorderBillableSKUModel>();
            }
            FbWorkorderBillableSKUModel SkuItem = SkuItems.Where(n => n.WorkOrderSKUId == value.WorkOrderSKUId).FirstOrDefault();

            if (SkuItem != null)
            {
                SkuItem.SKU = value.SKU;
                SkuItem.Qty = value.Qty;
                SkuItem.UnitPrice = value.UnitPrice;
            }

            TempData["Billable"] = SkuItems;
            TempData.Keep("Billable");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BillableDelete(int key)
        {
            IList<FbWorkorderBillableSKUModel> skuItems = TempData["Billable"] as IList<FbWorkorderBillableSKUModel>;
            FbWorkorderBillableSKUModel skuItem = skuItems.Where(n => n.WorkOrderSKUId == key).FirstOrDefault();
            skuItems.Remove(skuItem);
            TempData["Billable"] = skuItems;
            TempData.Keep("Billable");
            return Json(skuItems, JsonRequestBehavior.AllowGet);
        }

        private bool IsCustomerSpecificTech(int TechId, int CustomerId)
        {
            bool isCustSpec = false;

            Contact cntct = FarmerBrothersEntitites.Contacts.Where(spc => spc.ContactID == CustomerId).FirstOrDefault();
            TECH_HIERARCHY thv = FarmerBrothersEntitites.TECH_HIERARCHY.Where(tc => tc.DealerId == TechId).FirstOrDefault();

            if(cntct != null && thv != null)
            {
                if(cntct.FBProviderID == TechId && thv.CustomerSpecificTech == true)
                {
                    isCustSpec = true;
                }
            }

            return isCustSpec;
        }

        [HttpGet]
        public JsonResult CloserSKU(string searchParams, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            List<VendorDataModel> CloserSkusList = new List<VendorDataModel>();

            try
            {
                IQueryable<string> CloserPartOrSKU = FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true).Select(s => s.ItemNo).Distinct();//.Take(5000);

                foreach (string vendor in CloserPartOrSKU)
                {
                    CloserSkusList.Add(new VendorDataModel(vendor));
                }
                CloserSkusList.OrderBy(v => v.VendorDescription).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the closer SKU ", ex);
            }
           
            ;
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = CloserSkusList };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }


        public JsonResult GetCloserSKUs(string SearchString)
        {
            List<VendorDataModel> CloserSkusList = new List<VendorDataModel>();
            try
            {
                IQueryable<string> CloserPartOrSKU = FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true && s.ItemNo.Contains(SearchString)).Select(s => s.ItemNo).Distinct();
                foreach (string vendor in CloserPartOrSKU)
                {
                    CloserSkusList.Add(new VendorDataModel(vendor));
                }
                CloserSkusList.OrderBy(v => v.VendorDescription).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the closer SKU ", ex);
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = CloserSkusList };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public WorkorderManagementModel ConstructWorkorderManagementModel(int? customerId, int? workOrderId, bool isNewPartsOrder = false, bool showAllTechs = false, bool isCustomerDashboard = false, bool isFromProcessCardScreen = false)
        {
            #region construct wo
            WorkorderManagementModel workOrderManagementModel = new WorkorderManagementModel();
            try
            {
                workOrderManagementModel.Documents = new DocumentModel();

                //billable changes 
                workOrderManagementModel.SKUModel = new FbWorkorderBillableSKUModel();
                IQueryable<string> skus = FarmerBrothersEntitites.FbBillableSKUs.Select(s => s.SKU).Distinct();
                workOrderManagementModel.SKUList = new List<VendorModelModel>();
                foreach (string sku in skus)
                {
                    workOrderManagementModel.SKUList.Add(new VendorModelModel(sku));
                }

                workOrderManagementModel.isCustomerDashboard = isCustomerDashboard;
                workOrderManagementModel.ShowAllTech = showAllTechs;
                workOrderManagementModel.PriorityList = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Priority" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();

                /*IEnumerable<List<ESMDSMRSM>> esmnames = (from m in FarmerBrothersEntitites.ESMDSMRSMs group m by m.ESMName into esName select esName.ToList()).ToList();

                IEnumerable<List<ESMDSMRSM>> dsmnames = (from m in FarmerBrothersEntitites.ESMDSMRSMs group m by m.CCMName into dsName select dsName.ToList()).ToList();

                IEnumerable<List<FBCustomerServiceDistribution>> asmnames = (from m in FarmerBrothersEntitites.FBCustomerServiceDistributions group m by m.SalesManagerName into ssName select ssName.ToList()).ToList();

                workOrderManagementModel.EstimateApprovedByModels = new List<EstimateApprovedByModel>();
                foreach (List<ESMDSMRSM> esmname in esmnames)
                {
                    if (esmname.Count() > 0)
                    {
                        workOrderManagementModel.EstimateApprovedByModels.Add(new EstimateApprovedByModel(esmname[0].ESMName, esmname[0].EDSMID));
                    }
                }
                foreach (List<ESMDSMRSM> dsmname in dsmnames)
                {
                    if (dsmname.Count() > 0)
                    {
                        workOrderManagementModel.EstimateApprovedByModels.Add(new EstimateApprovedByModel(dsmname[0].CCMName, dsmname[0].CCMID));
                    }
                }
                foreach (List<FBCustomerServiceDistribution> asmname in asmnames)
                {
                    if (asmname.Count() > 0)
                    {
                        workOrderManagementModel.EstimateApprovedByModels.Add(new EstimateApprovedByModel(asmname[0].SalesManagerName, asmname[0].ServiceId));
                    }
                }
                workOrderManagementModel.EstimateApprovedByModels.OrderBy(n => n.EstimateApprovedBy);
                workOrderManagementModel.EstimateApprovedByModels.Insert(0, new EstimateApprovedByModel("", -1));*/

                //List<EstimateEscalation> estescList = (from m in FarmerBrothersEntitites.EstimateEscalations group m by m.Name into esName select esName.ToList()).ToList();
                List<EstimateEscalation> estescList = FarmerBrothersEntitites.EstimateEscalations.Where(x => x.IsActive == true).ToList();
                workOrderManagementModel.EstimateApprovedByModels = new List<EstimateApprovedByModel>();
                foreach (EstimateEscalation estesc in estescList)
                {                  
                        workOrderManagementModel.EstimateApprovedByModels.Add(new EstimateApprovedByModel(estesc.Name, estesc.Code));
                }
                workOrderManagementModel.EstimateApprovedByModels.OrderBy(n => n.EstimateApprovedBy);
                workOrderManagementModel.EstimateApprovedByModels.Insert(0, new EstimateApprovedByModel("", -1));


                workOrderManagementModel.BrandNames = FarmerBrothersEntitites.BrandNames.Where(b => b.Active == 1).OrderBy(n => n.BrandName1).ToList();
                workOrderManagementModel.SalesNotificationReasonCodes = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Notify Sales" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
                workOrderManagementModel.CallTypes = Utility.GetCallTypeList(FarmerBrothersEntitites);//FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.Active == 1).OrderBy(wt => wt.Sequence).ToList();
                workOrderManagementModel.ClosureCallTypes = Utility.GetCallTypeList(FarmerBrothersEntitites);//FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.Active == 1).OrderBy(wt => wt.Sequence).ToList();
                workOrderManagementModel.EquipmentTypes = FarmerBrothersEntitites.EquipTypes.OrderBy(e => e.Sequence).ToList();
                workOrderManagementModel.NonSerializedList = new List<WorkOrderManagementNonSerializedModel>();
                workOrderManagementModel.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
                workOrderManagementModel.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
                workOrderManagementModel.BillableSKUList = new List<FbWorkorderBillableSKUModel>();
                workOrderManagementModel.WorkOrderParts = new List<WorkOrderPartModel>();
                workOrderManagementModel.Operation = WorkOrderManagementSubmitType.NONE;
                workOrderManagementModel.States = Utility.GetStates(FarmerBrothersEntitites);
                workOrderManagementModel.Closure = new WorkOrderClosureModel();
                workOrderManagementModel.Closure.CustomerSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.CustomerSignatureDetails).FirstOrDefault();
                workOrderManagementModel.Closure.TechnicianSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.TechnicianSignatureDetails).FirstOrDefault();
                workOrderManagementModel.Closure.PhoneSolveList = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Phone Solve" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
                workOrderManagementModel.Closure.PhoneSolveList.Insert(0, new AllFBStatu()
                {
                    Active = 1,
                    FBStatus = "",
                    FBStatusID = -1,
                    StatusFor = "Phone Solve",
                    StatusSequence = 0
                });
                workOrderManagementModel.Closure.FilterReplaced = Convert.ToBoolean(FarmerBrothersEntitites.Contacts.Where(wr => wr.ContactID == customerId).Select(w => w.FilterReplaced).FirstOrDefault());
                workOrderManagementModel.Closure.WaterTested = Convert.ToBoolean(FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.WaterTested).FirstOrDefault());
                workOrderManagementModel.Closure.HardnessRatingList = new List<string>();
                List<string> ratingsList = new List<string>() {"", "1", "2", "3", "4", "5", "6", "7", "8", "9", "over 10" };

                foreach (string rating in ratingsList)
                {
                    workOrderManagementModel.Closure.HardnessRatingList.Add(rating);
                }
                workOrderManagementModel.Closure.HardnessRating = FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.HardnessRating).FirstOrDefault();
                workOrderManagementModel.Closure.TDS = Convert.ToDecimal(FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.TotalDissolvedSolids).FirstOrDefault());

                workOrderManagementModel.Closure.WarrentyList = new List<string>();
                workOrderManagementModel.Closure.WarrentyList.Add("");
                workOrderManagementModel.Closure.WarrentyList.Add("YES");
                workOrderManagementModel.Closure.WarrentyList.Add("NO");

                workOrderManagementModel.Closure.WarrentyForList = new List<string>();
                workOrderManagementModel.Closure.WarrentyForList.Add("");
                workOrderManagementModel.Closure.WarrentyForList.Add("Just Parts");
                workOrderManagementModel.Closure.WarrentyForList.Add("Parts and Labor");

                workOrderManagementModel.Closure.AdditionalFollowupList = new List<string>();
                workOrderManagementModel.Closure.AdditionalFollowupList.Add("");
                workOrderManagementModel.Closure.AdditionalFollowupList.Add("YES");
                workOrderManagementModel.Closure.AdditionalFollowupList.Add("NO");

                workOrderManagementModel.Closure.OperationalList = new List<string>();
                workOrderManagementModel.Closure.OperationalList.Add("");
                workOrderManagementModel.Closure.OperationalList.Add("YES");
                workOrderManagementModel.Closure.OperationalList.Add("NO");

                workOrderManagementModel.BranchIds = new List<int>();
                workOrderManagementModel.AssistTechIds = new List<int>();
                workOrderManagementModel.IsNewPartsOrder = isNewPartsOrder;
                workOrderManagementModel.SystemInfoes = FarmerBrothersEntitites.SystemInfoes.Where(s => s.Active == 1).OrderBy(s => s.Sequence).ToList();
                workOrderManagementModel.Solutions = FarmerBrothersEntitites.Solutions.Where(s => s.Active == 1).OrderBy(s => s.Sequence).ToList();
                workOrderManagementModel.Solutions.Insert(0, new Solution()
                {
                    Description = "",
                    SolutionId = -1,
                    Active = 1,
                    Sequence = 0
                });
                workOrderManagementModel.SystemInfoes.Insert(0, new SystemInfo()
                {
                    Active = 1,
                    Description = "",
                    SystemId = 0
                });
                workOrderManagementModel.RemovalReasons = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Removal Reason" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
                workOrderManagementModel.AppointmentReasons = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Appointment Date" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();


                workOrderManagementModel.TaggedCategories = new List<CategoryModel>();
                //IQueryable<string> categories = FarmerBrothersEntitites.Categories.Where(s => s.Active == 1).OrderBy(s => s.ColUpdated).Select(s => s.CategoryCode + " - " + s.CategoryDesc);
                IQueryable<string> categories = FarmerBrothersEntitites.Categories.OrderBy(s => s.ColUpdated).Select(s => s.CategoryCode + " - " + s.CategoryDesc);
                foreach (string category in categories)
                {
                    workOrderManagementModel.TaggedCategories.Add(new CategoryModel(category));
                }
                                
                workOrderManagementModel.ShippigPriorities = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "ShippingPriority" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
                workOrderManagementModel.ShippigPriorities.Insert(0, new AllFBStatu()
                {
                    FBStatusID = 0,
                    FBStatus = "",
                    Active = 1,
                    StatusSequence = 0
                });

                workOrderManagementModel.Symptoms = FarmerBrothersEntitites.Symptoms.Where(s => s.Active == 1).OrderBy(s => s.Description).ToList();
                workOrderManagementModel.Symptoms.Insert(0, new Symptom()
                {
                    Description = "",
                    SymptomID = 0
                });

                workOrderManagementModel.Amps = FarmerBrothersEntitites.AMPSLists.Where(a => a.Active == 1).OrderBy(a => a.Sequence).ToList();
                workOrderManagementModel.Amps.Insert(0, new AMPSList()
                {
                    AMPSDescription = "",
                    AMPSID = 0
                });

                workOrderManagementModel.ElectricalPhases = FarmerBrothersEntitites.ElectricalPhaseLists.Where(e => e.Active == 1).OrderBy(e => e.Sequence).ToList();
                workOrderManagementModel.ElectricalPhases.Insert(0, new ElectricalPhaseList()
                {
                    ElectricalPhase = "",
                    ElectricalPhaseID = 0
                });

                workOrderManagementModel.NmeaNumbers = FarmerBrothersEntitites.NEMANumberLists.Where(n => n.Active == 1).OrderBy(n => n.Sequence).ToList();
                workOrderManagementModel.NmeaNumbers.Insert(0, new NEMANumberList()
                {
                    NemaNumberDescription = "",
                    NemaNumberID = 0
                });

                workOrderManagementModel.Voltages = FarmerBrothersEntitites.VoltageLists.Where(v => v.Active == 1).OrderBy(v => v.Sequence).ToList();
                workOrderManagementModel.Voltages.Insert(0, new VoltageList()
                {
                    Voltage = "",
                    VoltageID = 0
                });

                workOrderManagementModel.WaterLines = FarmerBrothersEntitites.WaterLineLists.Where(w => w.Active == 1).OrderBy(w => w.Sequence).ToList();
                workOrderManagementModel.WaterLines.Insert(0, new WaterLineList()
                {
                    WaterLine = "",
                    WaterLineID = 0
                });

                workOrderManagementModel.YesNoList = new List<YesNoItem>();
                workOrderManagementModel.YesNoList.Add(new YesNoItem()
                {
                    Description = "Yes",
                    Id = 1
                });
                workOrderManagementModel.YesNoList.Add(new YesNoItem()
                {
                    Description = "No",
                    Id = 2
                });

                workOrderManagementModel.YesNoList.Insert(0, new YesNoItem()
                {
                    Description = "",
                    Id = 0
                });

                IQueryable<string> vendors = FarmerBrothersEntitites.Vendors.Where(s => s.VendorActive == 1).Select(s => s.VendorDescription).Distinct();
                workOrderManagementModel.TaggedManufacturer = new List<VendorDataModel>();
                foreach (string vendor in vendors)
                {
                    workOrderManagementModel.TaggedManufacturer.Add(new VendorDataModel(vendor));
                }
                workOrderManagementModel.TaggedManufacturer = workOrderManagementModel.TaggedManufacturer.OrderBy(v => v.VendorDescription).ToList();

                workOrderManagementModel.CloserNonTaggedManufacturer = WorkOrderLookup.CloserManufacturer(FarmerBrothersEntitites);

                workOrderManagementModel.CloserPartsOrSKUs = WorkOrderLookup.CloserSKU(FarmerBrothersEntitites);


                workOrderManagementModel.NonTaggedManufacturer = WorkOrderLookup.PartOrderManufacturer(FarmerBrothersEntitites);

                IQueryable<string> models = FarmerBrothersEntitites.FBSKUs.Where(s => s.SKUActive == true).Select(s => s.SKU).Distinct();

                workOrderManagementModel.NonTaggedModels = WorkOrderLookup.PartOrderSKU(FarmerBrothersEntitites);                

                workOrderManagementModel.IsOpen = false;
                workOrderManagementModel.BillingTotal = 0;

                List<BillingItem> blngItmsList = FarmerBrothersEntitites.BillingItems.Where(b => b.IsActive == true).ToList();
                List<CategoryModel> billingItms = new List<CategoryModel>();
                foreach(BillingItem item in blngItmsList)
                {                    
                    billingItms.Add(new CategoryModel(item.BillingName));
                }
                workOrderManagementModel.BillingItems = billingItms;

                List<BillingModel> bmList = new List<BillingModel>();
                //BillingModel bmItem = new BillingModel();
                //bmItem.BillingType = "TRAVEL TIME";
                //bmItem.BillingCode = "913654";
                //bmItem.Quantity = 2;
                //bmItem.Cost = Convert.ToDecimal(70.45);

                //decimal tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
                //bmItem.Total = tot;

                //workOrderManagementModel.BillingTotal += tot;
                //bmList.Add(bmItem);

                if (workOrderId.HasValue)
                {
                    List<WorkorderBillingDetail> wbdList = FarmerBrothersEntitites.WorkorderBillingDetails.Where(w => w.WorkorderId == workOrderId).ToList();
                    foreach (WorkorderBillingDetail bitem in wbdList)
                    {
                        BillingItem blngItm = FarmerBrothersEntitites.BillingItems.Where(b => b.BillingCode == bitem.BillingCode).FirstOrDefault();

                        if (blngItm != null)
                        {
                            BillingModel bmItem = new BillingModel();
                            bmItem.BillingType = blngItm.BillingName;
                            bmItem.BillingCode = bitem.BillingCode;
                            bmItem.Quantity = Convert.ToInt32(bitem.Quantity);
                            bmItem.Cost = Convert.ToDecimal(blngItm.UnitPrice);
                            decimal tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
                            bmItem.Total = tot;

                            workOrderManagementModel.BillingTotal += tot;
                            bmList.Add(bmItem);
                        }
                    }
                }
                else
                {
                    foreach (BillingItem item in blngItmsList)
                    {
                        if (item.BillingCode == "091.4425")
                        {
                            BillingModel bmItem = new BillingModel();
                            bmItem.BillingType = item.BillingName;
                            bmItem.BillingCode = item.BillingCode;
                            bmItem.Quantity = 1;
                            bmItem.Duration = "1:00 Hrs";
                            bmItem.Cost = Convert.ToDecimal(item.UnitPrice);
                            decimal tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
                            bmItem.Total = tot;

                            workOrderManagementModel.BillingTotal += tot;
                            bmList.Add(bmItem);
                        }
                    }
                }
                workOrderManagementModel.BillingDetails = bmList;

                Customer serviceCustomer = null;
                bool workEqumentAdded = false;
                if (workOrderId.HasValue)
                {
                    workOrderManagementModel.WorkOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId.Value).FirstOrDefault();
                    List<FbWorkOrderSKU> workorderSkus = FarmerBrothersEntitites.FbWorkOrderSKUs.Where(w => w.WorkorderID == workOrderId.Value).ToList();
                    workOrderManagementModel.Estimate = workOrderManagementModel.WorkOrder.Estimate;
                    workOrderManagementModel.FinalEstimate = workOrderManagementModel.WorkOrder.FinalEstimate;
                    workOrderManagementModel.IsEstimateApproved = Convert.ToBoolean(workOrderManagementModel.WorkOrder.IsEstimateApproved);
                    workOrderManagementModel.ThirdPartyPO = workOrderManagementModel.WorkOrder.ThirdPartyPO;
                    //workOrderManagementModel.IsBillableFeed = Convert.ToBoolean(workOrderManagementModel.WorkOrder.IsBillable);

                    workOrderManagementModel.WorkOrder.WorkorderContactPhone = Utilities.Utility.FormatPhoneNumber(workOrderManagementModel.WorkOrder.WorkorderContactPhone);
                    Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrderManagementModel.WorkOrder.CustomerID).FirstOrDefault();
                    workOrderManagementModel.Customer = new CustomerModel(customer, FarmerBrothersEntitites);
                    workOrderManagementModel.Customer.WorkOrderId = workOrderManagementModel.WorkOrder.WorkorderID.ToString();
                    workOrderManagementModel.Customer.MainContactName = workOrderManagementModel.WorkOrder.CustomerMainContactName == null ? "" : workOrderManagementModel.WorkOrder.CustomerMainContactName.ToString();
                    workOrderManagementModel.Customer.PhoneNumber = workOrderManagementModel.WorkOrder.CustomerPhone == null ? "" : workOrderManagementModel.WorkOrder.CustomerPhone.ToString();
                    
                    workOrderManagementModel.Customer.TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, workOrderManagementModel.WorkOrder.CustomerID.ToString());

                    serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == (int)workOrderManagementModel.WorkOrder.CustomerID).FirstOrDefault();
                    if (serviceCustomer != null)
                    {
                        workOrderManagementModel.Customer.BillingCode = serviceCustomer.BillingCode;
                        if (!string.IsNullOrEmpty(serviceCustomer.BillingCode))
                        {
                            workOrderManagementModel.Customer.IsBillable = CustomerModel.IsBillableService(serviceCustomer.BillingCode, workOrderManagementModel.Customer.TotalCallsCount);
                            workOrderManagementModel.Customer.ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, serviceCustomer.BillingCode);
                        }
                        else
                        {
                            workOrderManagementModel.Customer.IsBillable = " ";
                            workOrderManagementModel.Customer.ServiceLevelDesc = " - ";
                        }
                    }

                    workOrderManagementModel.IsBillableFeed = workOrderManagementModel.Customer.IsBillable.ToUpper() == "TRUE" ? true : false;
                    workOrderManagementModel.BillableSKUList = new List<FbWorkorderBillableSKUModel>();
                    foreach (FbWorkOrderSKU sku in workorderSkus)
                    {
                        FbWorkorderBillableSKUModel skuModel = new FbWorkorderBillableSKUModel(sku);
                        workOrderManagementModel.BillableSKUList.Add(skuModel);
                    }

                    TempData["Billable"] = workOrderManagementModel.BillableSKUList;

                    //workOrderManagementModel.IsBillable = Convert.ToBoolean(workOrderManagementModel.WorkOrder.IsBillable);

                    if (!string.IsNullOrWhiteSpace(workOrderManagementModel.WorkOrder.CurrentUserName) &&
                         string.Compare(workOrderManagementModel.WorkOrder.CurrentUserName.TrimEnd(), UserName.TrimEnd(), true) != 0)
                    {

                        DateTime CurrentTime = Utility.GetCurrentTime(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                        workOrderManagementModel.CurrentDateTime = CurrentTime;
                        DateTime WorkorderOpenedTime = Convert.ToDateTime(workOrderManagementModel.WorkOrder.WorkOrderOpenedDateTime);
                        double diffInMinutes = (CurrentTime - WorkorderOpenedTime).TotalMinutes;
                        int AllowedTimeToOpenWorkOrderInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["AllowedTimeToOpenWorkOrderInMinutes"]);
                        if (diffInMinutes > AllowedTimeToOpenWorkOrderInMinutes)
                        {
                            workOrderManagementModel.IsOpen = false;
                            workOrderManagementModel.WorkOrder.CurrentUserName = UserName;
                            workOrderManagementModel.WorkOrder.WorkOrderOpenedDateTime = CurrentTime;
                            FarmerBrothersEntitites.SaveChanges();
                        }
                        else
                        {
                            workOrderManagementModel.IsOpen = true;
                        }
                    }
                    else
                    {
                        workOrderManagementModel.IsOpen = false;
                        workOrderManagementModel.WorkOrder.CurrentUserName = UserName;
                        DateTime CurrentTime = Utility.GetCurrentTime(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                        workOrderManagementModel.WorkOrder.WorkOrderOpenedDateTime = CurrentTime;
                        workOrderManagementModel.CurrentDateTime = CurrentTime;
                        FarmerBrothersEntitites.SaveChanges();
                    }

                    WorkorderDetail workOrderDetail = FarmerBrothersEntitites.WorkorderDetails.Where(wd => wd.WorkorderID == workOrderId.Value).FirstOrDefault();
                    workOrderManagementModel.Closure.Email = "";
                    if (workOrderDetail != null)
                    {
                        if (workOrderDetail.SpawnReason.HasValue)
                        {
                            AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(al => al.FBStatusID == workOrderDetail.SpawnReason.Value).FirstOrDefault();
                            if (status != null)
                            {
                                workOrderManagementModel.SpawnReason = status.FBStatus;
                            }
                        }
                        if (workOrderDetail.NSRReason.HasValue)
                        {
                            AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(al => al.FBStatusID == workOrderDetail.NSRReason.Value).FirstOrDefault();
                            if (status != null)
                            {
                                workOrderManagementModel.NSRReason = status.FBStatus;
                            }
                        }

                        if (workOrderDetail.SolutionId.HasValue)
                        {
                            Solution solution = FarmerBrothersEntitites.Solutions.Where(s => s.SolutionId == workOrderDetail.SolutionId.Value).FirstOrDefault();
                            if (solution != null)
                            {
                                workOrderManagementModel.Solution = solution.Description;
                            }
                        }

                        workOrderManagementModel.Closure.StartDateTime = workOrderDetail.StartDateTime;
                        workOrderManagementModel.Closure.InvoiceNo = workOrderDetail.InvoiceNo;
                        workOrderManagementModel.Closure.ArrivalDateTime = workOrderDetail.ArrivalDateTime;
                        workOrderManagementModel.Closure.CompletionDateTime = workOrderDetail.CompletionDateTime;
                        workOrderManagementModel.Closure.ResponsibleTechName = workOrderDetail.ResponsibleTechName;
                        workOrderManagementModel.Closure.Mileage = workOrderDetail.Mileage;
                        workOrderManagementModel.Closure.CustomerSignedBy = workOrderDetail.CustomerSignatureBy;

                        workOrderManagementModel.Closure.WarrentyFor = workOrderDetail.WarrentyFor;
                        workOrderManagementModel.Closure.StateOfEquipment = workOrderDetail.StateofEquipment;
                        workOrderManagementModel.Closure.serviceDelayed = workOrderDetail.ServiceDelayReason;
                        workOrderManagementModel.Closure.troubleshootSteps = workOrderDetail.TroubleshootSteps;
                        workOrderManagementModel.Closure.followupComments = workOrderDetail.FollowupComments;
                        //workOrderManagementModel.Closure.operationalComments = workOrderDetail.OperationalComments;
                        workOrderManagementModel.Closure.ReviewedBy = workOrderDetail.ReviewedBy;
                        workOrderManagementModel.Closure.IsUnderWarrenty = workOrderDetail.IsUnderWarrenty;
                        workOrderManagementModel.Closure.AdditionalFollowup = workOrderDetail.AdditionalFollowupReq;
                        workOrderManagementModel.Closure.Operational = workOrderDetail.IsOperational;

                        workOrderManagementModel.RescheduleReasonCodesList = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "ReScheduleReasonCode" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
                        workOrderManagementModel.RescheduleReasonCodesList.Insert(0, new AllFBStatu()
                        {
                            FBStatusID = 0,
                            FBStatus = "",
                            Active = 1,
                            StatusSequence = 0
                        });

                        workOrderManagementModel.ReasonCode = workOrderManagementModel.WorkOrder.RescheduleReasonCode;

                        if (string.IsNullOrWhiteSpace(workOrderDetail.CustomerName))
                        {
                            workOrderManagementModel.Closure.CustomerName = workOrderManagementModel.WorkOrder.WorkorderContactName;
                        }
                        else
                        {
                            workOrderManagementModel.Closure.CustomerName = workOrderDetail.CustomerName;
                        }
                        workOrderManagementModel.Closure.CustomerEmail = workOrderDetail.CustomerEmail;
                        workOrderManagementModel.PhoneSolveId = workOrderDetail.PhoneSolveid;
                        workOrderManagementModel.PhoneSolveTechId = workOrderDetail.ResponsibleTechid;
                        workOrderManagementModel.Closure.SpecialClosure = workOrderDetail.SpecialClosure;
                        workOrderManagementModel.Closure.PhoneSolveid = workOrderDetail.PhoneSolveid;

                        if (workOrderDetail.ArrivalDateTime.HasValue && workOrderDetail.StartDateTime.HasValue)
                        {
                            DateTime arrival = Convert.ToDateTime(workOrderDetail.ArrivalDateTime);
                            DateTime strt = Convert.ToDateTime(workOrderDetail.StartDateTime);
                            TimeSpan timeDiff = arrival.Subtract(strt);

                            workOrderManagementModel.Closure.TravelHours = timeDiff.Hours.ToString();
                            workOrderManagementModel.Closure.TravelMinutes = timeDiff.Minutes.ToString();
                        }

                        else
                        {
                            if (!string.IsNullOrWhiteSpace(workOrderDetail.TravelTime))
                            {
                                string[] times = workOrderDetail.TravelTime.Split(':');

                                if (times.Count() >= 2)
                                {
                                    workOrderManagementModel.Closure.TravelHours = times[0];
                                    workOrderManagementModel.Closure.TravelMinutes = times[1];
                                }
                            }
                        }
                    }

                    if (workOrderManagementModel.WorkOrder.PartsRushOrder.HasValue == false)
                    {
                        workOrderManagementModel.WorkOrder.PartsRushOrder = false;
                    }

                    models = FarmerBrothersEntitites.Skus.Where(s => s.SkuActive == 1 && s.EQUIPMENT_TAG == "TAGGED").Select(s => s.Sku1).Distinct();
                    workOrderManagementModel.TaggedModels = new List<VendorModelModel>();
                    foreach (string model in models)
                    {
                        workOrderManagementModel.TaggedModels.Add(new VendorModelModel(model));
                    }
                    workOrderManagementModel.TaggedModels = workOrderManagementModel.TaggedModels.OrderBy(v => v.Model).ToList();

                    IQueryable<WorkorderSchedule> workOrderSchedules = FarmerBrothersEntitites.WorkorderSchedules.Where(ws => ws.WorkorderID == workOrderId);

                    int primaryBranchId = 0;
                    int secondaryBranchId = 0;

                    foreach (WorkorderSchedule workOrderSchedule in workOrderSchedules)
                    {
                        if (workOrderSchedule.PrimaryTech > 0)
                        {
                            if (string.Compare(workOrderSchedule.AssignedStatus, "Accepted", true) == 0
                                || string.Compare(workOrderSchedule.AssignedStatus, "Sent", true) == 0)
                            {
                                workOrderManagementModel.ResponsibleTechId = workOrderSchedule.Techid;
                                primaryBranchId = workOrderSchedule.ServiceCenterID.Value;
                            }
                        }
                        else if (workOrderSchedule.AssistTech > 0)
                        {
                            if (string.Compare(workOrderSchedule.AssignedStatus, "Accepted", true) == 0
                                || string.Compare(workOrderSchedule.AssignedStatus, "Sent", true) == 0)
                            {
                                workOrderManagementModel.AssistTechIds.Add(workOrderSchedule.Techid.Value);
                                if (workOrderSchedule.ServiceCenterID.HasValue)
                                {
                                    secondaryBranchId = workOrderSchedule.ServiceCenterID.Value;
                                }
                            }
                        }

                        if (workOrderSchedule.ServiceCenterID.HasValue)
                        {
                            workOrderManagementModel.BranchIds.Add(workOrderSchedule.ServiceCenterID.Value);
                        }
                    }

                    try
                    {
                        serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == (int)workOrderManagementModel.WorkOrder.CustomerID.Value).FirstOrDefault();
                        if (serviceCustomer != null)
                        {
                            workOrderManagementModel.Customer.CustomerSpecialInstructions = serviceCustomer.CustomerSpecialInstructions;
                        }
                    }
                    catch (Exception e)
                    { }

                    IList<TechDispatchWithDistance> dispatchBranches = null;
                    if (string.IsNullOrWhiteSpace(workOrderManagementModel.Customer.ZipCode) || workOrderManagementModel.Customer.ZipCode == "NONE")
                    {
                        dispatchBranches = new List<TechDispatchWithDistance>();
                    }
                    else
                    {
                        dispatchBranches = Utility.GetTechDispatchWithDistance(FarmerBrothersEntitites, workOrderManagementModel.Customer.ZipCode, workOrderId.Value).ToList();
                    }
                    IList<BranchModel> branches = new List<BranchModel>();

                    foreach (TechDispatchWithDistance dispatchBranch in dispatchBranches)
                    {
                        if (dispatchBranch.CustomerSpecificTech == "1")
                        {
                            if(IsCustomerSpecificTech(dispatchBranch.ServiceCenterId, Convert.ToInt32(customerId)))
                            {
                                branches.Add(new BranchModel(dispatchBranch));
                            }
                        }
                        else
                        {
                            branches.Add(new BranchModel(dispatchBranch));
                        }
                    }

                    workOrderManagementModel.Branches = branches;



                    DateTime currentDateTime = Utility.GetCurrentTime(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                    int isDateHolidayWeekend = Utility.IsDateHolidayWeekend(currentDateTime.ToString("MM/dd/yyyy"), FarmerBrothersEntitites);

                    int OnCallStartTime = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallStartTime"]);
                    int OnCallStartTimeMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallStartTimeMinutes"]);
                    int OnCallEndTime = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallEndTime"]);

                    DateTime OnCallAfterStartTime = Convert.ToDateTime(currentDateTime.ToString("MM-dd-yyyy")).AddHours(OnCallStartTime).AddMinutes(OnCallStartTimeMinutes);
                    DateTime OnCallAfterEndTime = Convert.ToDateTime(currentDateTime.ToString("MM-dd-yyyy")).AddHours(OnCallEndTime);

                    int OnCallTechId = 0;

                    if (isDateHolidayWeekend == 1 || isDateHolidayWeekend == 2 ||
                        (currentDateTime > OnCallAfterStartTime || currentDateTime < OnCallAfterEndTime))
                    {
                        OnCallTechId = GetAfterHoursOnCallTechIdByZip(customer.PostalCode, workOrderId);
                    }
                    else
                    {
                        OnCallTechId = GetOnCallTechIdByZip(customer.PostalCode, workOrderId);
                    }
                    bool isafterHours = false;
                    //1=Weekend ; 2=Holiday
                    //After WorkHours = 4:30PM to 7AM
                    if (isDateHolidayWeekend == 1 || isDateHolidayWeekend == 2 ||
                        (currentDateTime > OnCallAfterStartTime || currentDateTime < OnCallAfterEndTime))
                    {
                        workOrderManagementModel.Branches = new List<BranchModel>();
                        if (OnCallTechId != 0)
                        {
                            TechDispatchWithDistance OnCalldispatchBranches = new TechDispatchWithDistance();
                            IList<TechDispatchWithDistance> afterHoursdispatchBranches = Utility.GetAfterHoursTechDispatchWithDistance(FarmerBrothersEntitites, workOrderManagementModel.Customer.ZipCode, workOrderId.Value).ToList();
                            OnCalldispatchBranches = afterHoursdispatchBranches.Where(t => t.ServiceCenterId == OnCallTechId).FirstOrDefault();
                            if (OnCalldispatchBranches != null)
                            {
                                workOrderManagementModel.Branches.Add(new BranchModel(OnCalldispatchBranches));
                                isafterHours = true;
                            }

                            if (workOrderManagementModel.Customer.Branch == "261")
                            {
                                BranchModel brnh = branches.Where(id => id.Id == workOrderManagementModel.Customer.FBProviderID).FirstOrDefault();
                                workOrderManagementModel.Branches = new List<BranchModel>();
                                workOrderManagementModel.Branches.Add(brnh);
                            }
                        }
                        else
                        {
                            if (!showAllTechs)
                            {
                                if (workOrderManagementModel.Customer.Branch == "261")
                                {
                                    BranchModel brnh = branches.Where(id => id.Id == workOrderManagementModel.Customer.FBProviderID).FirstOrDefault();
                                    workOrderManagementModel.Branches = new List<BranchModel>();
                                    workOrderManagementModel.Branches.Add(brnh);
                                }
                                else
                                {
                                    workOrderManagementModel.ShowAllTech = false;
                                    workOrderManagementModel.Branches = new List<BranchModel>();
                                }
                                //workOrderManagementModel.ShowAllTech = false;
                                //workOrderManagementModel.Branches = new List<BranchModel>();
                            }

                        }
                    }

                    if (workOrderManagementModel.ShowAllTech == true)
                    {
                        workOrderManagementModel.Branches = new List<BranchModel>();
                        workOrderManagementModel.Branches = branches;
                    }



                    IList<int?> brandIds = FarmerBrothersEntitites.WorkOrderBrands.Where(wb => wb.WorkorderID == workOrderId.Value).Select(wb => wb.BrandID).ToList();
                    workOrderManagementModel.SelectedBrandIds = string.Join(",", brandIds);
                    workOrderManagementModel.SelectedBrands = FarmerBrothersEntitites.BrandNames.Where(b => brandIds.Contains(b.BrandID)).ToList();

                    workOrderManagementModel.Customer.ERFStatus = "-";
                    if (!string.IsNullOrWhiteSpace(workOrderManagementModel.WorkOrder.WorkorderErfid))
                    {
                        workOrderManagementModel.Erf = FarmerBrothersEntitites.Erfs.Where(e => e.ErfID == workOrderManagementModel.WorkOrder.WorkorderErfid).FirstOrDefault();
                        if (workOrderManagementModel.Erf != null)
                        {
                            workOrderManagementModel.Customer.ERFStatus = workOrderManagementModel.Erf.ERFStatus == null ? "-" : workOrderManagementModel.Erf.ERFStatus;
                        }
                    }

                    workOrderManagementModel.SerialNumberList = FarmerBrothersEntitites.FBCBEs.Where(s => s.CurrentCustomerId == workOrderManagementModel.WorkOrder.CustomerID).ToList();

                    //foreach (WorkOrderManagementEquipmentModel epm in workOrderManagementModel.WorkOrderEquipments)
                    //{
                    //    FBCBE vmm = workOrderManagementModel.SerialNumberList.Where(se => se.SerialNumber == epm.SerialNumber).FirstOrDefault();
                    //    if (vmm == null)
                    //    {
                    //        if (!string.IsNullOrEmpty(epm.SerialNumber))
                    //        {
                    //            workOrderManagementModel.SerialNumberList.Insert(0, new FBCBE()
                    //            {
                    //                Id = -1,
                    //                SerialNumber = epm.SerialNumber,
                    //                ItemNumber = string.IsNullOrEmpty(epm.Model) ? "" : epm.Model ,
                    //                ItemDescription = string.IsNullOrEmpty(epm.Model) ? "" : epm.Model,
                    //            });
                    //        }
                    //    }
                    //}

                    workOrderManagementModel.SerialNumberList.Add(new FBCBE()
                    {
                        Id = -1,
                        SerialNumber = "Other",
                        ItemNumber = "-1",
                        ItemDescription = " "
                    });

                    if (workOrderManagementModel.SerialNumberList != null && workOrderManagementModel.SerialNumberList.Count > 0)
                    {
                        workOrderManagementModel.SerialNumberList.Insert(0, new FBCBE()
                        {
                            Id = -1,
                            SerialNumber = "",
                            ItemNumber = "-1",
                            ItemDescription = ""
                        });
                    }

                    if (workOrderManagementModel.WorkOrder.WorkorderCalltypeid == 1300
                        && string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0
                        && string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Completed") != 0
                        && !workOrderManagementModel.WorkOrder.OriginalWorkorderid.HasValue)
                    {


                        int sequenceNumber = 1;
                        foreach (WorkorderEquipmentRequested workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipmentRequesteds)
                        {
                            WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);
                            equipmentModel.SequenceNumber = sequenceNumber++;
                            workOrderManagementModel.WorkOrderEquipmentsRequested.Add(equipmentModel);
                        }

                        sequenceNumber = 1;
                        foreach (WorkorderEquipment workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipments)
                        {
                            WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);

                            if (string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0)
                            {
                                FBCBE fbSN = workOrderManagementModel.SerialNumberList.Where(em => em.SerialNumber == equipmentModel.SerialNumber).FirstOrDefault();                            
                                if (fbSN == null)
                                {
                                    equipmentModel.SerialNumber = "Other";
                                }
                                else
                                {
                                    equipmentModel.SerialNumberManual = " ";
                                }
                            }

                            equipmentModel.SequenceNumber = sequenceNumber++;
                            workOrderManagementModel.WorkOrderEquipments.Add(equipmentModel);
                            workEqumentAdded = true;
                        }
                    }
                    else
                    {
                        IQueryable<NonSerialized> nonSerializedItems = FarmerBrothersEntitites.NonSerializeds.Where(ns => ns.WorkorderID == workOrderId);
                        foreach (NonSerialized nonSerializedItem in nonSerializedItems)
                        {
                            WorkOrderManagementNonSerializedModel nonSerializedModel = new WorkOrderManagementNonSerializedModel()
                            {
                                NSerialid = nonSerializedItem.NSerialid,
                                Catalogid = nonSerializedItem.Catalogid,
                                ManufNumber = nonSerializedItem.ManufNumber,
                                OrigOrderQuantity = nonSerializedItem.OrigOrderQuantity.Value
                            };
                            workOrderManagementModel.NonSerializedList.Add(nonSerializedModel);
                        }

                        int sequenceNumber = 1;
                        foreach (WorkorderEquipmentRequested workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipmentRequesteds)
                        {
                            WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);
                            equipmentModel.SequenceNumber = sequenceNumber++;
                            workOrderManagementModel.WorkOrderEquipmentsRequested.Add(equipmentModel);
                        }

                        sequenceNumber = 1;
                        foreach (WorkorderEquipment workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipments)
                        {
                            WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);

                            if (string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0)
                            {
                                FBCBE fbSN = workOrderManagementModel.SerialNumberList.Where(em => em.SerialNumber == equipmentModel.SerialNumber).FirstOrDefault();
                                if (fbSN == null)
                                {
                                    equipmentModel.SerialNumber = "Other";
                                }
                                else
                                {
                                    equipmentModel.SerialNumberManual = " ";
                                }
                            }

                            equipmentModel.SequenceNumber = sequenceNumber++;
                            workOrderManagementModel.WorkOrderEquipments.Add(equipmentModel);
                            workEqumentAdded = true;
                        }
                    } 

                    DateTime currentTime = Utility.GetCurrentTime(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);

                    workOrderManagementModel.IsOnCallTechVisible = false;

                    //workOrderManagementModel.OnCallTech = GetTechOnCall(workOrderManagementModel.Customer.Branch);

                    if (isafterHours)
                    {
                        workOrderManagementModel.OnCallTech = GetAfterHoursOnCallTechDetailsByZip(workOrderManagementModel.Customer.ZipCode, workOrderId);
                    }
                    else
                    {
                        workOrderManagementModel.OnCallTech = GetOnCallTechDetailsByZip(workOrderManagementModel.Customer.ZipCode, workOrderId);
                    }


                    if (string.IsNullOrWhiteSpace(workOrderManagementModel.OnCallTech) == false)
                    {
                        workOrderManagementModel.IsOnCallTechVisible = true;
                    }


                    TempData["WorkOrderEquipments"] = workOrderManagementModel.WorkOrderEquipmentsRequested;
                    TempData["NonSerialized"] = workOrderManagementModel.NonSerializedList;

                    int count = 1;
                    if (!workEqumentAdded)
                    {
                        foreach (WorkorderEquipment workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipments)
                        {
                            WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);

                            if (string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0)
                            {
                                FBCBE fbSN = workOrderManagementModel.SerialNumberList.Where(em => em.SerialNumber == equipmentModel.SerialNumber).FirstOrDefault();
                                if (fbSN == null)
                                {
                                    equipmentModel.SerialNumber = "Other";
                                }
                                else
                                {
                                    equipmentModel.SerialNumberManual = " ";
                                }
                            }

                            equipmentModel.SequenceNumber = count++;
                            workOrderManagementModel.WorkOrderEquipments.Add(equipmentModel);
                        }
                    }

                    IEnumerable<WorkorderPart> parts = workOrderManagementModel.WorkOrder.WorkorderParts.Where(wp => wp.AssetID == null || wp.AssetID == 0);
                    foreach (WorkorderPart workOrderPart in parts)
                    {
                        WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);
                        workOrderManagementModel.WorkOrderParts.Add(workOrderPartModel);
                    }
                    TempData["WorkOrderParts"] = workOrderManagementModel.WorkOrderParts;

                    workOrderManagementModel.IsBranchAlternateAddress = false;
                    workOrderManagementModel.IsCustomerAlternateAddress = false;

                    if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Local Branch", true) == 0)
                    {
                        workOrderManagementModel.PartsShipTo = 1;
                    }
                    else if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Other Local Branch", true) == 0)
                    {
                        workOrderManagementModel.PartsShipTo = 1;
                        workOrderManagementModel.IsBranchAlternateAddress = true;

                        workOrderManagementModel.BranchOtherPartsName = workOrderManagementModel.WorkOrder.OtherPartsName;
                        workOrderManagementModel.BranchOtherPartsContactName = workOrderManagementModel.WorkOrder.OtherPartsContactName;
                        workOrderManagementModel.BranchOtherPartsAddress1 = workOrderManagementModel.WorkOrder.OtherPartsAddress1;
                        workOrderManagementModel.BranchOtherPartsAddress2 = workOrderManagementModel.WorkOrder.OtherPartsAddress2;
                        workOrderManagementModel.BranchOtherPartsCity = workOrderManagementModel.WorkOrder.OtherPartsCity;
                        workOrderManagementModel.BranchOtherPartsState = workOrderManagementModel.WorkOrder.OtherPartsState;
                        workOrderManagementModel.BranchOtherPartsZip = workOrderManagementModel.WorkOrder.OtherPartsZip;
                        workOrderManagementModel.BranchOtherPartsPhone = workOrderManagementModel.WorkOrder.OtherPartsPhone;
                    }
                    else if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Customer", true) == 0)
                    {
                        workOrderManagementModel.PartsShipTo = 2;
                    }
                    else if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Other Customer", true) == 0)
                    {
                        workOrderManagementModel.PartsShipTo = 2;
                        workOrderManagementModel.IsCustomerAlternateAddress = true;

                        workOrderManagementModel.CustomerOtherPartsName = workOrderManagementModel.WorkOrder.OtherPartsName;
                        workOrderManagementModel.CustomerOtherPartsContactName = workOrderManagementModel.WorkOrder.OtherPartsContactName;
                        workOrderManagementModel.CustomerOtherPartsAddress1 = workOrderManagementModel.WorkOrder.OtherPartsAddress1;
                        workOrderManagementModel.CustomerOtherPartsAddress2 = workOrderManagementModel.WorkOrder.OtherPartsAddress2;
                        workOrderManagementModel.CustomerOtherPartsCity = workOrderManagementModel.WorkOrder.OtherPartsCity;
                        workOrderManagementModel.CustomerOtherPartsState = workOrderManagementModel.WorkOrder.OtherPartsState;
                        workOrderManagementModel.CustomerOtherPartsZip = workOrderManagementModel.WorkOrder.OtherPartsZip;
                        workOrderManagementModel.CustomerOtherPartsPhone = Utilities.Utility.FormatPhoneNumber(workOrderManagementModel.WorkOrder.OtherPartsPhone);

                    }
                    else if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Van", true) == 0)
                    {
                        workOrderManagementModel.PartsShipTo = 4;
                    }

                    if (!string.IsNullOrEmpty(workOrderManagementModel.WorkOrder.ShippingPriority))
                    {
                        workOrderManagementModel.ShippingPriority = workOrderManagementModel.WorkOrder.ShippingPriority;
                    }

                    RemovalSurvey survey = FarmerBrothersEntitites.RemovalSurveys.Where(r => r.WorkorderID == workOrderId.Value).FirstOrDefault();
                    if (survey != null)
                    {
                        if (survey.JMSOwnedMachines.HasValue)
                        {
                            workOrderManagementModel.RemovalCount = survey.JMSOwnedMachines.Value;
                        }
                    }

                }
                else if (customerId.HasValue)
                {
                    workOrderManagementModel.WorkOrder = new WorkOrder() { CustomerID = customerId.Value };
                    workOrderManagementModel.WorkOrder.EntryUserName = UserName;
                    
                    if (isNewPartsOrder == true)
                    {
                        //workOrderManagementModel.WorkOrder.WorkorderCalltypeid = 1820;
                        workOrderManagementModel.WorkOrder.WorkorderCalltypeDesc = "Parts Request";
                        workOrderManagementModel.WorkOrder.PartsRushOrder = false;
                    }

                    if (workOrderManagementModel.WorkOrder != null && workOrderManagementModel.WorkOrder.CustomerID.HasValue)
                    {
                        serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == (int)workOrderManagementModel.WorkOrder.CustomerID).FirstOrDefault();
                        if (serviceCustomer != null)
                        {
                            workOrderManagementModel.Customer = new CustomerModel(serviceCustomer, FarmerBrothersEntitites);
                            workOrderManagementModel.Customer = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, workOrderManagementModel.Customer);
                            workOrderManagementModel.Customer.TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, workOrderManagementModel.Customer.CustomerId);
                            workOrderManagementModel.Customer.BillingCode = serviceCustomer.BillingCode;
                            if (!string.IsNullOrEmpty(serviceCustomer.BillingCode))
                            {
                                workOrderManagementModel.Customer.IsBillable = CustomerModel.IsBillableService(serviceCustomer.BillingCode, workOrderManagementModel.Customer.TotalCallsCount);
                                workOrderManagementModel.Customer.ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, serviceCustomer.BillingCode);
                            }
                            else
                            {
                                workOrderManagementModel.Customer.IsBillable = " ";
                                workOrderManagementModel.Customer.ServiceLevelDesc = " - ";
                            }
                            if (workOrderId.HasValue)
                            {
                                workOrderManagementModel.Customer.WorkOrderId = workOrderId.Value.ToString();
                            }
                            else
                            {
                                workOrderManagementModel.Customer.WorkOrderId = "-1";
                            }
                        }
                        else
                        {
                            workOrderManagementModel.Customer = new CustomerModel();
                        }
                    }

                    TempData["WorkOrderEquipments"] = workOrderManagementModel.WorkOrderEquipmentsRequested;
                    TempData["NonSerialized"] = workOrderManagementModel.NonSerializedList;
                }

                if (workOrderManagementModel.WorkOrder != null)
                {
                    workOrderManagementModel.Closure.PopulateSpecialClosureList(workOrderManagementModel.WorkOrder, FarmerBrothersEntitites);
                    workOrderManagementModel.ServiceQuoteDetails = new ServiceQuoteModel();


                    WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID 
                                                                && (w.AssignedStatus == "Sent" || w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();
                    if (ws != null)
                    {
                        TECH_HIERARCHY techHView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == ws.Techid).FirstOrDefault();

                        if (techHView != null && techHView.FamilyAff == "SPT" &&
                            (string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0
                           && string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Invoiced") != 0))
                        {
                            List<string> NSRUserIds = ConfigurationManager.AppSettings["TPSPCloseNSRUserIds"].Split(';').ToList();
                            List<string> NSRUserNames = ConfigurationManager.AppSettings["TPSPCloseNSRUserNames"].Split(';').ToList();
                            string NSRUserNm = NSRUserIds.Where(x => x == System.Web.HttpContext.Current.Session["UserId"].ToString()).FirstOrDefault();
                            if (string.IsNullOrEmpty(NSRUserNm))
                            {
                                var item = workOrderManagementModel.Solutions.Single(x => x.SolutionId == 9999);
                                workOrderManagementModel.Solutions.Remove(item);
                            }
                        }
                        
                        if (techHView != null)
                        {
                            workOrderManagementModel.ServiceQuoteDetails = CalculateServiceQuote(workOrderManagementModel.WorkOrder.WorkorderID);

                            /*
                            dynamic travelDetails = Utility.GetTravelDetailsBetweenZipCodes(techHView.PostalCode, workOrderManagementModel.WorkOrder.CustomerZipCode);
                            if (travelDetails != null)
                            {
                                var element = travelDetails.rows[0].elements[0];
                                decimal distance = element == null ? 0 : (element.distance == null ? 0 : element.distance.value * (decimal)0.000621371192);
                                distance = Math.Round(distance, 2);
                                decimal travelTime = element == null ? Convert.ToDecimal(1800.00 / 3600.00) : (element.duration == null ? Convert.ToDecimal(1800.00 / 3600.00) : element.duration.value / 3600.00);
                                decimal duration = Math.Round(travelTime, 2);

                                decimal TravelHour = Convert.ToDecimal(Math.Truncate(duration));
                                decimal TravelMinutes = (Convert.ToDecimal(travelTime) - (TravelHour)) * Convert.ToDecimal(60.00);

                                workOrderManagementModel.TravelDistance = distance + " Miles";
                                workOrderManagementModel.Distance = distance.ToString();
                                workOrderManagementModel.TravelTime = "Hr " + Math.Abs(TravelHour) + ":" + Math.Round(TravelMinutes) + " Minutes";

                                decimal LaborCost = Convert.ToDecimal(ConfigurationManager.AppSettings["LaborCost"]);
                                workOrderManagementModel.Labor = LaborCost;

                                List<FbWorkorderBillableSKUModel> partsList = new List<FbWorkorderBillableSKUModel>();
                                
                                FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true).Select(s => s.ItemNo).Distinct();
                                decimal? pTotal = (from closureParts in FarmerBrothersEntitites.WorkorderParts
                                                      join sk in FarmerBrothersEntitites.Skus on closureParts.Sku equals sk.Sku1
                                                      where closureParts.WorkorderID == workOrderId
                                                      select sk.SKUCost).Sum();

                                decimal partsTotal = pTotal == null ? 0 : Convert.ToDecimal(pTotal);

                                workOrderManagementModel.PartsTotal = partsTotal;

                                decimal total = (duration * LaborCost) + LaborCost + partsTotal;
                                //workOrderManagementModel.TotalServiceQuote = duration + " * 85 " + " + " + " $85 Labor Cost = " + string.Format("{0:c2}", total);
                                workOrderManagementModel.TotalServiceQuote = string.Format("{0:c2}", total);
                            }
                            */
                        }
                    }

                    workOrderManagementModel.Documents.WorkorderDocuments = GetWorkorderDocuments(workOrderManagementModel.WorkOrder.WorkorderID);
                }

                if (workOrderManagementModel.WorkOrder != null)
                {
                    workOrderManagementModel.Closure.PopulateEmailList();
                }

                workOrderManagementModel.IsCustomerPartsOrder = true;


                if (workOrderManagementModel.WorkOrder == null)
                {
                    workOrderManagementModel.WorkOrder = new WorkOrder();
                    workOrderManagementModel.WorkOrder.EntryUserName = UserName;
                }
                if (workOrderManagementModel.Customer == null)
                {
                    workOrderManagementModel.Customer = new CustomerModel();
                    workOrderManagementModel.WorkOrder.WorkorderCallstatus = "Hold for AB";
                }

                IEnumerable<TechHierarchyView> Techlist = WorkorderManagementModel.GetCompleteTechData(FarmerBrothersEntitites);

                List<TechHierarchyView> newTechlistCollection = new List<TechHierarchyView>();

                foreach (var item in Techlist.ToList())
                {
                    item.PreferredProvider = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                    newTechlistCollection.Add(item);
                }
                workOrderManagementModel.Notes = new NotesModel()
                {
                    CustomerZipCode = workOrderManagementModel.Customer.ZipCode,
                    WorkOrderStatus = workOrderManagementModel.WorkOrder.WorkorderCallstatus,
                    Technicianlist = newTechlistCollection
                };

                /*workOrderManagementModel.Notes.NotesHistory = new List<NotesHistoryModel>();
                IQueryable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID && nh.AutomaticNotes == 0).OrderByDescending(nh => nh.EntryDate);

                foreach (NotesHistory notesHistory in notesHistories)
                {
                    workOrderManagementModel.Notes.NotesHistory.Add(new NotesHistoryModel(notesHistory));
                }

                IQueryable<NotesHistory> recordHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID && nh.AutomaticNotes == 1).OrderByDescending(nh => nh.EntryDate);
                workOrderManagementModel.Notes.RecordHistory = new List<NotesHistoryModel>();
                foreach (NotesHistory recordHistory in recordHistories)
                {
                    workOrderManagementModel.Notes.RecordHistory.Add(new NotesHistoryModel(recordHistory));
                }*/

                workOrderManagementModel.Notes.NotesHistory = new List<NotesHistoryModel>();
                //IQueryable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID && nh.isDispatchNotes == 0).OrderByDescending(nh => nh.EntryDate);
                IQueryable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID).OrderByDescending(nh => nh.NotesID);

                foreach (NotesHistory notesHistory in notesHistories)
                {
                    workOrderManagementModel.Notes.NotesHistory.Add(new NotesHistoryModel(notesHistory));
                }

                workOrderManagementModel.Notes.DispatchNotesHistory = new List<NotesHistoryModel>();
                IQueryable<NotesHistory> dispatchNotesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID && nh.isDispatchNotes == 1).OrderByDescending(nh => nh.NotesID);

                foreach (NotesHistory notesHistory in dispatchNotesHistories)
                {
                    workOrderManagementModel.Notes.DispatchNotesHistory.Add(new NotesHistoryModel(notesHistory));
                }

                workOrderManagementModel.Notes.CustomerNotesResults = new List<CustomerNotesModel>();
                int custId = Convert.ToInt32(workOrderManagementModel.Customer.CustomerId);
                int parentId = string.IsNullOrEmpty(workOrderManagementModel.Customer.ParentNumber) ? 0 : Convert.ToInt32(workOrderManagementModel.Customer.ParentNumber);
                var custNotes = Utility.GetCustomreNotes(custId, parentId, FarmerBrothersEntitites);//FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();
                foreach (var dbCustNotes in custNotes)
                {
                    workOrderManagementModel.Notes.CustomerNotesResults.Add(new CustomerNotesModel(dbCustNotes));
                }

                workOrderManagementModel.Notes.FollowUpRequestList = new List<AllFBStatu>();
                workOrderManagementModel.Notes.FollowUpRequestList = FarmerBrothersEntitites.AllFBStatus
                    .Where(a => a.StatusFor == "Follow Up Call" && a.Active == 1 && a.FBStatus != "Customer Cancelled Service").ToList();

                var followupRequest = FarmerBrothersEntitites.AllFBStatus.Where(a => a.StatusFor == "Follow Up Call" && a.Active == 1 && a.FBStatus == "Customer Cancelled Service").FirstOrDefault();

                if (workOrderManagementModel.WorkOrder.WorkorderCallstatus == "Pending Acceptance" || workOrderManagementModel.WorkOrder.WorkorderCallstatus == "Accepted"
                    || workOrderManagementModel.WorkOrder.WorkorderCallstatus == "Accepted-Partial" || workOrderManagementModel.WorkOrder.WorkorderCallstatus == "Scheduled")
                {
                    if (followupRequest != null)
                        workOrderManagementModel.Notes.FollowUpRequestList.Add(followupRequest);
                }

                workOrderManagementModel.Notes.FollowUpRequestList = workOrderManagementModel.Notes.FollowUpRequestList.OrderBy(f => f.StatusSequence).ToList();

                if (workOrderManagementModel.WorkOrder.WorkorderCallstatus != "Open" && workOrderManagementModel.WorkOrder.FollowupCallID != 681)
                {
                    workOrderManagementModel.Notes.FollowUpRequestID = workOrderManagementModel.WorkOrder.FollowupCallID.ToString();
                }

                workOrderManagementModel.Notes.WorkOrderID = workOrderManagementModel.WorkOrder.WorkorderID;
                if (workOrderManagementModel.WorkOrder.ProjectFlatRate.HasValue)
                {
                    workOrderManagementModel.Notes.ProjectFlatRate = Math.Round(workOrderManagementModel.WorkOrder.ProjectFlatRate.Value, 2);
                }
                if (workOrderManagementModel.WorkOrder.ProjectID.HasValue)
                {
                    workOrderManagementModel.Notes.ProjectNumber = workOrderManagementModel.WorkOrder.ProjectID.Value;
                }

                if (workOrderManagementModel.IsNewPartsOrder == false)
                    workOrderManagementModel.Notes.viewProp = "NormalWorkOrderView";
                workOrderManagementModel.Notes.IsSpecificTechnician = Convert.ToBoolean(workOrderManagementModel.WorkOrder.IsSpecificTechnician);
                workOrderManagementModel.Notes.IsAutoDispatched = false;
                workOrderManagementModel.Notes.TechID = workOrderManagementModel.WorkOrder.SpecificTechnician;
                workOrderManagementModel.Notes.CustomerID = workOrderManagementModel.Customer.CustomerId;
                if (workOrderManagementModel.Erf != null)
                {
                    workOrderManagementModel.Notes.ErfID = workOrderManagementModel.Erf.ErfID;
                }

                //if (workOrderManagementModel.Customer.ServiceTier == "5" && (!string.IsNullOrEmpty(workOrderManagementModel.Customer.CustomerType) && workOrderManagementModel.Customer.CustomerType.ToLower() != "ce")
                //           && (string.IsNullOrEmpty(workOrderManagementModel.Customer.ParentNumber) || workOrderManagementModel.Customer.ParentNumber == "0"))

                //bool isBilling = Utility.IsBillableCriteria(Convert.ToInt32(workOrderManagementModel.Customer.CustomerId), workOrderManagementModel.WorkOrder.WorkorderID, FarmerBrothersEntitites);

                if (!string.IsNullOrEmpty(workOrderManagementModel.Customer.BillingCode) && workOrderManagementModel.Customer.BillingCode.ToLower() == "s08")
                //if(isBilling)
                {                    
                    workOrderManagementModel.IsCCProcessComplete = workOrderManagementModel.WorkOrder.FinalTransactionId == null ? false : true;
                }
                
                workOrderManagementModel.Documents.WorkOrderID = workOrderManagementModel.WorkOrder.WorkorderID;
                workOrderManagementModel.Documents.UserName = UserName;
                if (workOrderId.HasValue)
                {
                    workOrderManagementModel.Documents.isNewEvent = false;
                    workOrderManagementModel.Customer.NonFBCustomerList = Utility.GetNonFBCustomers(FarmerBrothersEntitites, false);
                }
                else
                {
                    workOrderManagementModel.Documents.isNewEvent = true;
                    workOrderManagementModel.Customer.NonFBCustomerList = Utility.GetNonFBCustomers(FarmerBrothersEntitites, true);
                }

                workOrderManagementModel.RedirectFromCardProcess = isFromProcessCardScreen;
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                throw ex;
            }

            #endregion
            return workOrderManagementModel;
        }

        [HttpPost]
        public JsonResult GetServiceQuoteDetailsWithParts(UpdateServiceQuoteModel serviceQuoteObj)
        {
            JsonResult jsonResult = new JsonResult();
            try
            {
                 ServiceQuoteModel quote = new ServiceQuoteModel();

                decimal? SkuTotal = serviceQuoteObj.SkuList.Where(w => w.PartReplenish == false).Select(s => s.Quantity * s.StandardCost).Sum();
                quote.PartsTotal = Convert.ToDecimal(SkuTotal);

                quote.TotalServiceQuote = Convert.ToDecimal(serviceQuoteObj.PreviousQuote) + Convert.ToDecimal(SkuTotal);

                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = quote };
            }
            catch (Exception ex)
            {
                jsonResult.Data = new { success = false, serverError = ErrorCode.ERROR };
            }

            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpGet]
        public JsonResult GetServiceQuoteDetails(int Workorderid)
        {
            JsonResult jsonResult = new JsonResult();
            try
            {
                ServiceQuoteModel quote = CalculateServiceQuote(Workorderid);
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = quote };
            }
            catch(Exception ex)
            {
                jsonResult.Data = new { success = false, serverError = ErrorCode.ERROR };
            }            
          
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ServiceQuoteModel CalculateServiceQuote(int Workorderid)
        {
            WorkOrder workorder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == Workorderid).FirstOrDefault();
            WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == Workorderid
                                                                && (w.AssignedStatus == "Sent" || w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();

            ServiceQuoteModel quote = new ServiceQuoteModel();
            if (ws != null)
            {
                TECH_HIERARCHY techHView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == ws.Techid).FirstOrDefault();
                
                dynamic travelDetails = Utility.GetTravelDetailsBetweenZipCodes(techHView.PostalCode, workorder.CustomerZipCode);
                if (travelDetails != null)
                {
                    var element = travelDetails.rows[0].elements[0];
                    decimal distance = element == null ? 0 : (element.distance == null ? 0 : element.distance.value * (decimal)0.000621371192);
                    distance = Math.Round(distance, 2);
                    decimal travelTime = element == null ? Convert.ToDecimal(1800.00 / 3600.00) : (element.duration == null ? Convert.ToDecimal(1800.00 / 3600.00) : element.duration.value / 3600.00);
                    decimal duration = Math.Round(travelTime, 2);

                    decimal TravelHour = Convert.ToDecimal(Math.Truncate(duration));
                    decimal TravelMinutes = (Convert.ToDecimal(travelTime) - (TravelHour)) * Convert.ToDecimal(60.00);

                    quote.TravelDistance = distance + " Miles";
                    quote.Distance = distance.ToString();
                    quote.TravelTime = "Hr " + Math.Abs(TravelHour) + ":" + Math.Round(TravelMinutes) + " Minutes";

                    PricingDetail priceDtls = Utility.GetPricingDetails(workorder.CustomerID, ws.Techid, workorder.CustomerState, FarmerBrothersEntitites);


                    //decimal LaborCost = Convert.ToDecimal(ConfigurationManager.AppSettings["LaborCost"]);

                    decimal LaborCost = priceDtls == null ? 0 : (priceDtls.HourlyLablrRate == null ? 0 : Convert.ToDecimal(priceDtls.HourlyLablrRate));
                    quote.Labor = LaborCost;// LaborCost;

                    decimal TravelCost = priceDtls == null ? 0 : (priceDtls.HourlyTravlRate == null ? 0 : Convert.ToDecimal(priceDtls.HourlyTravlRate));

                    List<FbWorkorderBillableSKUModel> partsList = new List<FbWorkorderBillableSKUModel>();

                    FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true).Select(s => s.ItemNo).Distinct();
                    decimal? pTotal = (from closureParts in FarmerBrothersEntitites.WorkorderParts
                                       join sk in FarmerBrothersEntitites.Skus on closureParts.Sku equals sk.Sku1
                                       where closureParts.WorkorderID == Workorderid
                                       select (sk.SKUCost * closureParts.Quantity)).Sum();

                    decimal partsTotal = pTotal == null ? 0 : Convert.ToDecimal(pTotal);

                    quote.PartsTotal = partsTotal;

                    decimal total = (duration * TravelCost) + LaborCost;
                    quote.TotalServiceQuote = total;
                }
            }

            return quote;
        }

        [HttpGet]
        public JsonResult GetCategoryDetails()
        {
            List<CategoryModel> TaggedCategories = new List<CategoryModel>();
            IQueryable<string> categories = FarmerBrothersEntitites.Categories.Where(s => s.Active == 1).OrderBy(s => s.ColUpdated).Select(s => s.CategoryCode + " - " + s.CategoryDesc);
            //foreach (string category in categories)
            //{
            //    TaggedCategories.Add(new CategoryModel(category));
            //}

            var data = new List<object>();
            foreach (string category in categories)
            {
                data.Add(new { value = category.Trim(), text = category.Trim() });
            }
            data.Insert(0, new { value = "", text = "" });

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        #endregion

        #region load searchworkorder screen

        [HttpGet]
        public ActionResult WorkorderSearch(int? isBack)
        {
            WorkorderSearchModel workOrderSearchModel;
            bool isFromExitWorkOrder = false;
            if (TempData["WorkOrderSearchCriteria"] != null && isBack == 1)
            {
                workOrderSearchModel = TempData["WorkOrderSearchCriteria"] as WorkorderSearchModel;
                WorkorderSearch(workOrderSearchModel);
                TempData["WorkOrderSearchCriteria"] = workOrderSearchModel;
                isFromExitWorkOrder = true;
            }
            else
            {
                workOrderSearchModel = new WorkorderSearchModel();
                TempData["WorkOrderSearchCriteria"] = null;
            }
            if (!isFromExitWorkOrder)
            {
                workOrderSearchModel = PopulateWorkOrderSearchModelLists(workOrderSearchModel);
                workOrderSearchModel.State = new List<string>();
                workOrderSearchModel.Priority = new List<string>();
                workOrderSearchModel.WorkorderType = new List<string>();
                workOrderSearchModel.Status = new List<string>();
                workOrderSearchModel.Status.Add("");
                workOrderSearchModel.FollowupCall = defaultFollowUpCall;
                workOrderSearchModel.State.Add("");
                workOrderSearchModel.WorkorderType.Add("");
                workOrderSearchModel.Priority.Add("");
                workOrderSearchModel.Esm = new List<string>();
                workOrderSearchModel.Esm.Add("");
            }

            return View(workOrderSearchModel);
        }

        private WorkorderSearchModel PopulateWorkOrderSearchModelLists(WorkorderSearchModel workOrderSearchModel)
        {
            workOrderSearchModel.AutoDispatchedList = new List<string>();
            workOrderSearchModel.AutoDispatchedList.Add("No");
            workOrderSearchModel.AutoDispatchedList.Add("Yes");
            workOrderSearchModel.AutoDispatchedList.Add(string.Empty);

            workOrderSearchModel.TechTypeList = new List<string>();
            workOrderSearchModel.TechTypeList.Add("");
            workOrderSearchModel.TechTypeList.Add("Internal");
            workOrderSearchModel.TechTypeList.Add("TPSP");

            workOrderSearchModel.TimeZoneList = new List<Models.TimeZone>();
            workOrderSearchModel.TimeZoneList.Add(new Models.TimeZone() { TimeZoneName = "Eastern", TimeZoneValue = "-5" });
            workOrderSearchModel.TimeZoneList.Add(new Models.TimeZone() { TimeZoneName = "Central", TimeZoneValue = "-6" });
            workOrderSearchModel.TimeZoneList.Add(new Models.TimeZone() { TimeZoneName = "Mountain", TimeZoneValue = "-7" });
            workOrderSearchModel.TimeZoneList.Add(new Models.TimeZone() { TimeZoneName = "Pacific", TimeZoneValue = "-8" });
            workOrderSearchModel.TimeZoneList.Add(new Models.TimeZone() { TimeZoneName = "Alaskan", TimeZoneValue = "-9" });
            workOrderSearchModel.TimeZoneList.Add(new Models.TimeZone() { TimeZoneName = "Hawaiian", TimeZoneValue = "-10" });
            workOrderSearchModel.TimeZoneList.OrderBy(sc => sc.TimeZoneName).ToList();
            workOrderSearchModel.TimeZone = new List<string>();
            workOrderSearchModel.TimeZone = workOrderSearchModel.TimeZone;
            workOrderSearchModel.timeZoneModel = new Models.TimeZoneModel();
            workOrderSearchModel.timeZoneModel.TimeZones = workOrderSearchModel.TimeZoneList;
            workOrderSearchModel.States = FarmerBrothersEntitites.States.OrderBy(s => s.StateName).ToList();
            workOrderSearchModel.WorkOrderTypes = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.Active == 1).OrderBy(w => w.Sequence).ToList();
            workOrderSearchModel.WorkOrderStatusList = FarmerBrothersEntitites.AllFBStatus.Where(s => s.StatusFor == "Work Order Status" && s.Active == 1).OrderBy(s => s.StatusSequence).ToList();            

            int userId = (int)System.Web.HttpContext.Current.Session["UserId"];
            workOrderSearchModel.SavedSearches = FarmerBrothersEntitites.WorkorderSavedSearches.Where(x => x.UserID == userId).ToList();
            workOrderSearchModel.FollowUpCallList = FarmerBrothersEntitites.AllFBStatus.Where(f => f.StatusFor == "Follow Up Call" && f.Active == 1).OrderBy(f => f.StatusSequence).ToList();

            workOrderSearchModel.PriorityList = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Priority" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            workOrderSearchModel.ServiceCenterList = GetServiceCenters(workOrderSearchModel.TechType);


            workOrderSearchModel.TechniciansList = GetTechnicians(workOrderSearchModel.ServiceCompany);
            workOrderSearchModel.TechnicianIds = GetTechnicianIds(workOrderSearchModel.ServiceCompany);

            //workOrderSearchModel.EsmList = FarmerBrothersEntitites.ESMCCMRSMEscalations.DistinctBy(x => x.EDSMID).ToList();
            List<ESMCCMRSMEscalation> esmList = FarmerBrothersEntitites.ESMCCMRSMEscalations.DistinctBy(x => x.ESMName).ToList();
            workOrderSearchModel.EsmList = new List<ESMCCMRSMEscalation>();
            foreach (ESMCCMRSMEscalation esm in esmList)
            {
                if (string.IsNullOrEmpty(esm.ESMName)) continue;
                ESMCCMRSMEscalation esmRec = workOrderSearchModel.EsmList.Where(x => x.ESMName == esm.ESMName).FirstOrDefault();
                if (esmRec == null)
                {
                    workOrderSearchModel.EsmList.Add(esm);
                }
            }

            workOrderSearchModel.TechId = null;

            return workOrderSearchModel;
        }

        private IList<TeamLeadModel> GetTeamLeadsList(double serviceCenterId)
        {
            IList<TeamLeadModel> teamLeadModels = new List<TeamLeadModel>();
            TeamLeadModel teamLeadModel = new TeamLeadModel(new TechHierarchyView());
            teamLeadModels.Add(teamLeadModel);

            if (serviceCenterId > 0)
            {
                IEnumerable<TechHierarchyView> teamLeads = Utility.GetTeamLeadsByServiceCenterId(FarmerBrothersEntitites, Convert.ToInt32(serviceCenterId));
                foreach (TechHierarchyView teamLead in teamLeads)
                {
                    decimal teamLeadId = Convert.ToDecimal(teamLead.TechID);
                    if (teamLeadModels.Where(t => t.Id == teamLeadId).FirstOrDefault() == null)
                    {
                        teamLeadModel = new TeamLeadModel(teamLead);
                        teamLeadModels.Add(teamLeadModel);
                    }
                }
            }

            return teamLeadModels.OrderBy(t => t.Name).ToList();
        }

        private IList<TechModel> GetTechnicianIds(double serviceCenterId)
        {
            IList<TechModel> techniciansList = new List<TechModel>();
            TechModel technicianModel = new TechModel(new TechHierarchyView());
            techniciansList.Add(technicianModel);

            return techniciansList.OrderBy(t => t.TechId).ToList();
        }

        private IList<TechModel> GetTechnicians(double serviceCenterId)
        {
            IList<TechModel> techniciansList = new List<TechModel>();
            TechModel technicianModel = new TechModel(new TechHierarchyView());
            techniciansList.Add(technicianModel);
            return techniciansList.OrderBy(t => t.TechName).ToList();
        }

        private IList<BranchModel> GetServiceCenters(string techType)
        {
            IList<BranchModel> serviceCenterList = new List<BranchModel>();

            if (!string.IsNullOrWhiteSpace(techType))
            {
                string branchType = string.Empty;
                string branchDesc = string.Empty;
                if (string.Compare(techType, "Internal", 0) == 0)
                {
                    branchDesc = "Internal Branch";
                    branchType = "";
                }
                else
                {
                    branchDesc = "THIRD PARTY SERVICE PROVIDER";
                    branchType = "SPT";
                }

                BranchModel branchModel = new BranchModel(new TechHierarchyView(), "", FarmerBrothersEntitites);
                serviceCenterList.Add(branchModel);

                IEnumerable<TechHierarchyView> serviceCenters = Utility.GetTechDataByBranchType(FarmerBrothersEntitites, branchDesc, branchType);
                foreach (TechHierarchyView serviceCenter in serviceCenters)
                {
                    branchModel = new BranchModel(serviceCenter, "", FarmerBrothersEntitites);
                    serviceCenterList.Add(branchModel);
                }
            }


            return serviceCenterList.OrderBy(b => b.Name).ToList();
        }
        public JsonResult GetTechnicianId(double serviceCenterId)
        {
            IList<TechModel> techniciansList = GetTechnicianIds(serviceCenterId);
            return Json(techniciansList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetServiceCenter(string techType)
        {
            IList<BranchModel> serviceCenterList = GetServiceCenters(techType);
            return Json(serviceCenterList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTechnician(double serviceCenterId)
        {
            IList<TechModel> techniciansList = GetTechnicians(serviceCenterId);
            return Json(techniciansList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTeamLeads(double serviceCenterId)
        {
            IList<TeamLeadModel> teamLeadModels = GetTeamLeadsList(serviceCenterId);
            return Json(teamLeadModels, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region dosearch workorder
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "WorkorderSearch")]
        public ActionResult WorkorderSearch(WorkorderSearchModel workOrderSearchModel)
        {
            WorkorderSavedSearch savedSearch;
            try
            {
                switch (workOrderSearchModel.Operation)
                {
                    case WorkOrderSearchSubmitType.SAVE:
                        if (ModelState.IsValid)
                        {
                            savedSearch = new WorkorderSavedSearch();
                            savedSearch.UserID = (int)System.Web.HttpContext.Current.Session["UserId"];
                            savedSearch = workOrderSearchModel.GetSavedSearch(savedSearch);
                            FarmerBrothersEntitites.WorkorderSavedSearches.Add(savedSearch);
                            FarmerBrothersEntitites.SaveChanges();
                            workOrderSearchModel.FromSavedSearch(new WorkorderSavedSearch());
                            workOrderSearchModel.Status.Add("");
                            workOrderSearchModel.FollowupCall = defaultFollowUpCall;
                            workOrderSearchModel.State.Add("");
                            workOrderSearchModel.WorkorderType.Add("");
                            workOrderSearchModel.Priority.Add("");
                            ModelState.Clear();
                        }
                        break;
                    case WorkOrderSearchSubmitType.UPDATE:
                        if (ModelState.IsValid)
                        {
                            if (!string.IsNullOrWhiteSpace(workOrderSearchModel.SelectedSavedSearchName))
                            {
                                savedSearch = FarmerBrothersEntitites.WorkorderSavedSearches.FirstOrDefault(ss => ss.SavedSearchName == workOrderSearchModel.SelectedSavedSearchName);
                                if (savedSearch != null)
                                {
                                    savedSearch = workOrderSearchModel.GetSavedSearch(savedSearch);
                                    FarmerBrothersEntitites.SaveChanges();
                                    workOrderSearchModel.FromSavedSearch(new WorkorderSavedSearch());
                                    workOrderSearchModel.Status.Add("");
                                    workOrderSearchModel.AutoDispatched = "No";
                                    workOrderSearchModel.FollowupCall = defaultFollowUpCall;
                                    workOrderSearchModel.State.Add("");
                                    workOrderSearchModel.WorkorderType.Add("");
                                    workOrderSearchModel.Priority.Add("");
                                    workOrderSearchModel.SelectedSavedSearchName = "";
                                    ModelState.Clear();
                                }
                            }
                        }
                        break;
                    case WorkOrderSearchSubmitType.CANCEL:
                        ModelState.Remove("SavedSearchName");
                        workOrderSearchModel.FromSavedSearch(new WorkorderSavedSearch());
                        workOrderSearchModel.SelectedSavedSearchName = "";
                        workOrderSearchModel.Status.Add("");
                        workOrderSearchModel.AutoDispatched = "No";
                        workOrderSearchModel.FollowupCall = defaultFollowUpCall;
                        workOrderSearchModel.State.Add("");
                        workOrderSearchModel.WorkorderType.Add("");
                        workOrderSearchModel.Priority.Add("");
                        ModelState.Clear();
                        break;
                    case WorkOrderSearchSubmitType.RETRIEVESAVEDSEARCH:
                        ModelState.Remove("SavedSearchName");
                        savedSearch = FarmerBrothersEntitites.WorkorderSavedSearches.FirstOrDefault(ss => ss.SavedSearchName == workOrderSearchModel.SelectedSavedSearchName);
                        if (savedSearch != null)
                        {
                            ModelState.Clear();
                            workOrderSearchModel.FromSavedSearch(savedSearch);
                        }
                        break;
                    case WorkOrderSearchSubmitType.SEARCH:
                        ModelState.Remove("SavedSearchName");
                        workOrderSearchModel.SavedSearchName = "";
                        workOrderSearchModel.SelectedSavedSearchName = "";

                        if (workOrderSearchModel.SearchInNonServiceWorkOrder)
                        {
                            workOrderSearchModel.SearchResults = GetNonServiceWorkOrderData(workOrderSearchModel);
                        }
                        else
                        {
                            workOrderSearchModel.SearchResults = GetWorkOrderDataUsingQuery(workOrderSearchModel);
                        }

                        workOrderSearchModel.TimeZoneName = Utility.GetTimeZoneByTime(workOrderSearchModel.TimeZoneName);
                        if (workOrderSearchModel.Priority == null)
                        {
                            workOrderSearchModel.Priority = new List<string>();
                        }
                        if (workOrderSearchModel.Status == null)
                        {
                            workOrderSearchModel.Status = new List<string>();
                        }
                        if (workOrderSearchModel.WorkorderType == null)
                        {
                            workOrderSearchModel.WorkorderType = new List<string>();
                        }
                        if (workOrderSearchModel.State == null)
                        {
                            workOrderSearchModel.State = new List<string>();
                        }

                        TempData["WorkOrderSearchCriteria"] = workOrderSearchModel;
                        break;
                }

                workOrderSearchModel = PopulateWorkOrderSearchModelLists(workOrderSearchModel);

                if (workOrderSearchModel.State == null)
                {
                    workOrderSearchModel.State = new List<string>();
                    workOrderSearchModel.State.Add("");
                }
            }
            catch (Exception e)
            {
                //foreach (var eve in e.EntityValidationErrors)
                //{
                //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                //    foreach (var ve in eve.ValidationErrors)
                //    {
                //        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                //            ve.PropertyName, ve.ErrorMessage);
                //    }
                //}
                throw;
            }



            return View(workOrderSearchModel);
        }

        [HttpPost]
        public void ExcelExportWorkorderSearchResults()
        {
            WorkorderSearchModel workOrderSearchModel = new WorkorderSearchModel();
            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();

            string gridModel = HttpContext.Request.Params["GridModel"];

            if (TempData["WorkOrderSearchCriteria"] != null)
            {
                workOrderSearchModel = TempData["WorkOrderSearchCriteria"] as WorkorderSearchModel;
                if (workOrderSearchModel.SearchInNonServiceWorkOrder)
                {
                    searchResults = GetNonServiceWorkOrderData(workOrderSearchModel);
                }
                else
                {
                    searchResults = GetWorkOrderDataUsingQuery(workOrderSearchModel);
                }


            }

            TempData["WorkOrderSearchCriteria"] = workOrderSearchModel;
            ExcelExport exp = new ExcelExport();
            var DataSource = searchResults;
            GridProperties obj = ConvertGridObject(gridModel);
            exp.Export(obj, searchResults, "WorkOrderSearchResults.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");

        }
        private IList<WorkorderSearchResultModel> GetNonServiceWorkOrderData(WorkorderSearchModel workorderSearchModel)
        {
            WorkOrder originalWorkOrder = null;
            var predicate = PredicateBuilder.True<NonServiceworkorder>();

            //predicate = predicate.And(w => w.IsUnknownWorkOrder != true);

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.WorkorderId))
            {
                predicate = predicate.And(w => w.WorkOrderID.ToString().Contains(workorderSearchModel.WorkorderId));
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.CustomerId))
            {
                predicate = predicate.And(w => w.CustomerID.ToString().Contains(workorderSearchModel.CustomerId));
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.City))
            {
                predicate = predicate.And(w => w.CustomerCity.Contains(workorderSearchModel.City));
            }

            if (workorderSearchModel.State != null && workorderSearchModel.State.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(workorderSearchModel.State[0]))
                {
                    predicate = predicate.And(w => workorderSearchModel.State.Contains(w.CustomerState));
                }
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.Zipcode))
            {
                predicate = predicate.And(w => w.CustomerZipCode.Contains(workorderSearchModel.Zipcode));
            }

            if (workorderSearchModel.Status != null && workorderSearchModel.Status.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(workorderSearchModel.Status[0]))
                {
                    predicate = predicate.And(w => workorderSearchModel.Status.Contains(w.NonServiceEventStatus.ToString()));
                }
            }

            TimeSpan time = new TimeSpan(23, 59, 59);
            if (workorderSearchModel.DateTo.HasValue)
            {
                workorderSearchModel.DateTo = workorderSearchModel.DateTo.Value.Date + time;
            }
            if (workorderSearchModel.DateFrom.HasValue && workorderSearchModel.DateTo.HasValue)
            {
                predicate = predicate.And(w => w.CreatedDate >= workorderSearchModel.DateFrom && w.CreatedDate <= workorderSearchModel.DateTo);
            }
            else if (workorderSearchModel.DateFrom.HasValue)
            {
                predicate = predicate.And(w => w.CreatedDate >= workorderSearchModel.DateFrom);
            }
            else if (workorderSearchModel.DateTo.HasValue)
            {
                predicate = predicate.And(w => w.CreatedDate <= workorderSearchModel.DateTo);
            }

            IQueryable<NonServiceworkorder> workOrders = FarmerBrothersEntitites.Set<NonServiceworkorder>().AsExpandable().Where(predicate).OrderByDescending(w=>w.CreatedDate);

            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();
            foreach (NonServiceworkorder workOrder in workOrders)
            {
                Contact contact = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();
                if(contact != null)
                {
                    //int userId = (int)System.Web.HttpContext.Current.Session["UserId"];
                    //int applicationId = FarmerBrothersEntitites.Applications.Where(a => a.ApplicationName.ToLower() == 'NonFBCustomer').FirstOrDefault();
                    //UserApplication userApplication = FarmerBrothersEntitites.UserApplications.Where(u => u.UserId == userId && u.ApplicationId == applicationId).FirstOrDefault();


                    Dictionary<string, string> UserPrivilege = (Dictionary<string, string>)System.Web.HttpContext.Current.Session["UserPrivilege" + (int)System.Web.HttpContext.Current.Session["UserId"]] == null
                                ? Security.GetUserPrivilegeByUserId((int)System.Web.HttpContext.Current.Session["UserId"], null) :
                                (Dictionary<string, string>)System.Web.HttpContext.Current.Session["UserPrivilege" + (int)System.Web.HttpContext.Current.Session["UserId"]];

                    NonFBCustomer nonFBCustomer = FarmerBrothersEntitites.NonFBCustomers.Where(n => n.NonFBCustomerId == contact.PricingParentID).FirstOrDefault();
                    if (nonFBCustomer != null && UserPrivilege["NonFBCustomer"] != "Full")
                    {
                        continue;
                    }
                }

                searchResults.Add(new WorkorderSearchResultModel(workOrder, FarmerBrothersEntitites));
            }

            return searchResults;
        }

       
        private IList<WorkorderSearchResultModel> GetWorkOrderDataUsingQuery(WorkorderSearchModel workorderSearchModel)
        {

            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();
            StringBuilder woSearchSelectQuery = new StringBuilder();
            StringBuilder woSearchWhereQuery = new StringBuilder();
            StringBuilder woSearchJoinQuery = new StringBuilder();

            Dictionary<string, string> UserPrivilege = (Dictionary<string, string>)System.Web.HttpContext.Current.Session["UserPrivilege" + (int)System.Web.HttpContext.Current.Session["UserId"]] == null
                               ? Security.GetUserPrivilegeByUserId((int)System.Web.HttpContext.Current.Session["UserId"], null) :
                               (Dictionary<string, string>)System.Web.HttpContext.Current.Session["UserPrivilege" + (int)System.Web.HttpContext.Current.Session["UserId"]];            

            #region Generate WO Search Query
            woSearchSelectQuery.Append(@"select w.WorkorderID ,wt.description, w.WorkorderCallstatus, st.fbstatus, c.ServiceLevelCode,w.WorkorderEntryDate,w.AppointmentDate,
                                 w.CustomerName, w.CustomerCity, w.CustomerState, w.CustomerZipCode ,w.CustomerID, ws.TechName,ws.TechPhone,ws.ServiceCenterName,ws.AssignedStatus,
                                 ws.ScheduleDate,ws.ModifiedScheduleDate,ws.ServiceCenterID,w.CustomerPO,w.NTE ");
            
                woSearchJoinQuery.Append(@" from workorder w WITH (NOLOCK)
                                        inner join dbo.Contact c WITH (NOLOCK) ON w.CustomerID = c.ContactID
                                        inner join WorkorderDetails wd WITH (NOLOCK) on w.Workorderid = wd.Workorderid
                                        inner join WorkorderType wt WITH (NOLOCK) on w.WorkorderCalltypeid = wt.calltypeid
                                        inner join AllFBStatus st WITH (NOLOCK) on  w.PriorityCode =st.fbstatusid
                                        left join WorkorderSchedule ws WITH (NOLOCK) on w.WorkorderID = ws.WorkorderID and  (ws.AssignedStatus = 'Accepted' or ws.AssignedStatus = 'Sent' or ws.AssignedStatus = 'Scheduled') ");
         

            woSearchWhereQuery.Append(@" where ");



            if (!string.IsNullOrEmpty(workorderSearchModel.SerialNumber))
            {
                woSearchJoinQuery.Append(@" left join WorkorderEquipment we WITH (NOLOCK) on w.WorkorderID  = we.WorkorderID ");
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" we.SerialNumber = '");
                    woSearchWhereQuery.Append(workorderSearchModel.SerialNumber + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and we.SerialNumber = '");
                    woSearchWhereQuery.Append(workorderSearchModel.SerialNumber + "'");
                }

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.CustomerId))
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.CustomerID like '%" + workorderSearchModel.CustomerId + "%'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.CustomerID like '%" + workorderSearchModel.CustomerId + "%'");
                }

            }
            if (!string.IsNullOrWhiteSpace(workorderSearchModel.ErfId))
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.WorkorderErfid like '%" + workorderSearchModel.ErfId + "%'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.WorkorderErfid like '%" + workorderSearchModel.ErfId + "%'");
                }


            }


            if (workorderSearchModel.Status != null && workorderSearchModel.Status.Count > 0)
            {
                string status = string.Empty;
                foreach (string s in workorderSearchModel.Status)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        status += "'" + s + "',";
                    }

                }
                if (!string.IsNullOrEmpty(status))
                {
                    if (woSearchWhereQuery.ToString() == " where ")
                    {
                        woSearchWhereQuery.Append(@" w.WorkorderCallstatus in ( " + status.TrimEnd(',') + " )");
                    }
                    else
                    {
                        woSearchWhereQuery.Append(@" and w.WorkorderCallstatus in ( " + status.TrimEnd(',') + " )");
                    }
                }
            }

            if (workorderSearchModel.Esm != null && workorderSearchModel.Esm.Count > 0)
            {
                string esm = string.Empty;
                foreach (string s in workorderSearchModel.Esm)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        esm += "'" + s + "',";
                    }

                }
                if (!string.IsNullOrEmpty(esm))
                {
                    if (woSearchWhereQuery.ToString() == " where ")
                    {
                        woSearchWhereQuery.Append(@" c.ESMName in ( " + esm.TrimEnd(',') + " )");
                    }
                    else
                    {
                        woSearchWhereQuery.Append(@" and c.ESMName in ( " + esm.TrimEnd(',') + " )");
                    }
                }


            }

            TimeSpan time = new TimeSpan(23, 59, 59);
            if (workorderSearchModel.DateTo.HasValue)
            {
                workorderSearchModel.DateTo = workorderSearchModel.DateTo.Value.Date + time;
            }

            if (workorderSearchModel.DateFrom.HasValue && workorderSearchModel.DateTo.HasValue)
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.WorkorderEntryDate >= '" + workorderSearchModel.DateFrom + "' and w.WorkorderEntryDate <= '" + workorderSearchModel.DateTo + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.WorkorderEntryDate >= '" + workorderSearchModel.DateFrom + "' and  w.WorkorderEntryDate <= '" + workorderSearchModel.DateTo + "'");
                }

            }
            else if (workorderSearchModel.DateFrom.HasValue)
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.WorkorderEntryDate >= '" + workorderSearchModel.DateFrom + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.WorkorderEntryDate >= '" + workorderSearchModel.DateFrom + "'");
                }
            }
            else if (workorderSearchModel.DateTo.HasValue)
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.WorkorderEntryDate <= '" + workorderSearchModel.DateTo + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.WorkorderEntryDate <= '" + workorderSearchModel.DateTo + "'");
                }
            }

            if (workorderSearchModel.AppointmentDateTo.HasValue)
            {
                workorderSearchModel.AppointmentDateTo = workorderSearchModel.AppointmentDateTo.Value.Date + time;
            }

            if (workorderSearchModel.AppointmentDateFrom.HasValue && workorderSearchModel.AppointmentDateTo.HasValue)
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.AppointmentDate >= '" + workorderSearchModel.AppointmentDateFrom + "' and  w.AppointmentDate <= '" + workorderSearchModel.AppointmentDateTo + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.AppointmentDate >= '" + workorderSearchModel.AppointmentDateFrom + "' and w.AppointmentDate <= '" + workorderSearchModel.AppointmentDateTo + "'");
                }

            }
            else if (workorderSearchModel.AppointmentDateFrom.HasValue)
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.AppointmentDate >= '" + workorderSearchModel.AppointmentDateFrom + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.AppointmentDate >= '" + workorderSearchModel.AppointmentDateFrom + "'");
                }

            }
            else if (workorderSearchModel.AppointmentDateTo.HasValue)
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.AppointmentDate <= '" + workorderSearchModel.AppointmentDateTo + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.AppointmentDate <= '" + workorderSearchModel.AppointmentDateTo + "'");
                }

            }



            if (workorderSearchModel.WorkorderType != null && workorderSearchModel.WorkorderType.Count > 0)
            {
                string wtype = string.Empty;
                foreach (string s in workorderSearchModel.WorkorderType)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        wtype += "'" + s + "',";
                    }
                }
                if (!string.IsNullOrEmpty(wtype))
                {
                    if (woSearchWhereQuery.ToString() == " where ")
                    {
                        woSearchWhereQuery.Append(@" w.WorkorderCalltypeid in( " + wtype.TrimEnd(',') + " )");
                    }
                    else
                    {
                        woSearchWhereQuery.Append(@" and w.WorkorderCalltypeid in ( " + wtype.TrimEnd(',') + " )");
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.OriginalWorkOrderId))
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.OriginalWorkorderid like '%" + workorderSearchModel.OriginalWorkOrderId + "%'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.OriginalWorkorderid like '%" + workorderSearchModel.OriginalWorkOrderId + "%'");
                }

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.City))
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.CustomerCity like '%" + workorderSearchModel.City + "%'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.CustomerCity like '%" + workorderSearchModel.City + "%'");
                }

            }



            if (workorderSearchModel.State != null && workorderSearchModel.State.Count > 0)
            {
                string stat = string.Empty;
                foreach (string s in workorderSearchModel.State)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        stat += "'" + s + "',";
                    }
                }
                if (!string.IsNullOrEmpty(stat))
                {
                    if (woSearchWhereQuery.ToString() == " where ")
                    {
                        woSearchWhereQuery.Append(@" w.CustomerState in ( " + stat.TrimEnd(',') + " )");
                    }
                    else
                    {
                        woSearchWhereQuery.Append(@" and w.CustomerState in ( " + stat.TrimEnd(',') + " )");
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.Zipcode))
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.CustomerZipCode like '%" + workorderSearchModel.Zipcode + "%'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.CustomerZipCode like '%" + workorderSearchModel.Zipcode + "%'");
                }

            }

            if (workorderSearchModel.Priority != null && workorderSearchModel.Priority.Count > 0)
            {
                string pr = string.Empty;
                foreach (string s in workorderSearchModel.Priority)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        pr += "'" + s + "',";
                    }
                }
                if (!string.IsNullOrEmpty(pr))
                {
                    if (woSearchWhereQuery.ToString() == " where ")
                    {
                        woSearchWhereQuery.Append(@" w.PriorityCode in ( " + pr.TrimEnd(',') + " )");
                    }
                    else
                    {
                        woSearchWhereQuery.Append(@" and w.PriorityCode in ( " + pr.TrimEnd(',') + " )");
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.FollowupCall.ToString()))
            {
                if (workorderSearchModel.FollowupCall.ToString() != "603")
                {
                    if (woSearchWhereQuery.ToString() == " where ")
                    {
                        woSearchWhereQuery.Append(@" w.FollowupCallID = '" + workorderSearchModel.FollowupCall + "'");
                    }
                    else
                    {
                        woSearchWhereQuery.Append(@" and w.FollowupCallID = '" + workorderSearchModel.FollowupCall + "'");
                    }
                }

            }

            if (workorderSearchModel.TimeZone != null && workorderSearchModel.TimeZone.Count > 0)
            {
                string tz = string.Empty;
                foreach (string s in workorderSearchModel.TimeZone)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        tz += "'" + s + "',";
                    }
                }
                if (!string.IsNullOrEmpty(tz))
                {
                    if (woSearchWhereQuery.ToString() == " where ")
                    {
                        woSearchWhereQuery.Append(@" w.WorkorderTimeZone in( " + tz.TrimEnd(',') + " )");
                    }
                    else
                    {
                        woSearchWhereQuery.Append(@" and w.WorkorderTimeZone in ( " + tz.TrimEnd(',') + " )");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.ParentAccount))
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" c.PricingParentId = " + workorderSearchModel.ParentAccount);
                }
                else
                {
                    woSearchWhereQuery.Append(@" and c.PricingParentId = " + workorderSearchModel.ParentAccount);
                }
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.TechId.ToString()))
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" ws.Techid like '%" + workorderSearchModel.TechId + "%'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and ws.Techid like '%" + workorderSearchModel.TechId + "%'");
                }

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.TechType) && workorderSearchModel.ServiceCompany <= 0)
            {
                IList<BranchModel> serviceCenterList = GetServiceCenters(workorderSearchModel.TechType);
                IList<int?> serviceCenterIds = serviceCenterList.Select(b => b.ServiceCenterId).ToList();
                string tz = string.Empty;
                foreach (int s in serviceCenterIds)
                {
                    tz += s + ",";
                }
                if (!string.IsNullOrEmpty(tz))
                {
                    if (woSearchWhereQuery.ToString() == " where ")
                    {
                        woSearchWhereQuery.Append(@" ws.ServiceCenterID in( " + tz.TrimEnd(',') + " )");
                    }
                    else
                    {
                        woSearchWhereQuery.Append(@" and ws.ServiceCenterID in ( " + tz.TrimEnd(',') + " )");
                    }
                }
            }

            if (workorderSearchModel.ServiceCompany > 0)
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" ws.ServiceCenterID = " + workorderSearchModel.ServiceCompany);
                }
                else
                {
                    woSearchWhereQuery.Append(@" and ws.ServiceCenterID = " + workorderSearchModel.ServiceCompany);
                }

            }
            bool hasOnlyWorkOrderId = false;
            if (!string.IsNullOrWhiteSpace(workorderSearchModel.WorkorderId))
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    hasOnlyWorkOrderId = true;
                    woSearchWhereQuery.Append(@" w.WorkorderID like '%" + workorderSearchModel.WorkorderId + "%'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.WorkorderID like '%" + workorderSearchModel.WorkorderId + "%'");

                }

            }
            //if (!hasOnlyWorkOrderId)
            //{
            //    if (workorderSearchModel.Status == null || workorderSearchModel.Status.Count <= 0 ||
            //        (workorderSearchModel.Status != null && workorderSearchModel.Status.Count > 0 &&
            //        !(workorderSearchModel.Status.Exists(x => string.Equals(x, "Closed", StringComparison.OrdinalIgnoreCase)))))
            //    {
            //        if (woSearchWhereQuery.ToString() == " where ")
            //        {

            //            woSearchWhereQuery.Append(@" w.WorkorderCallstatus != 'Closed'");
            //        }
            //        else
            //        {
            //            woSearchWhereQuery.Append(@" and w.WorkorderCallstatus != 'Closed'");
            //        }
            //    }

            //}

            if (workorderSearchModel.CustomerPO != null && !string.IsNullOrWhiteSpace(workorderSearchModel.CustomerPO.ToString()))
            {
                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.CustomerPO like '%" + workorderSearchModel.CustomerPO + "%'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.CustomerPO like '%" + workorderSearchModel.CustomerPO + "%'");
                }

            }

            if (workorderSearchModel.ClosedDate.HasValue)
            {
                DateTime ClosFrmDt = Convert.ToDateTime(workorderSearchModel.ClosedDate);
                DateTime ClosToDt = Convert.ToDateTime(workorderSearchModel.ClosedDate).AddDays(1);

                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" w.workorderClosedate >= '" + ClosFrmDt + "' and w.workorderClosedate < '" + ClosToDt + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and w.workorderClosedate >= '" + ClosFrmDt + "' and w.workorderClosedate < '" + ClosToDt + "'");
                }
            }
            if (workorderSearchModel.StartDate.HasValue)
            {
                DateTime StrtFrmDt = Convert.ToDateTime(workorderSearchModel.StartDate);
                DateTime StrtToDt = Convert.ToDateTime(workorderSearchModel.StartDate).AddDays(1);

                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" wd.StartDateTime >= '" + StrtFrmDt + "' and wd.StartDateTime < '" + StrtToDt + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and wd.StartDateTime >= '" + StrtFrmDt + "' and wd.StartDateTime < '" + StrtToDt + "'");
                }
            }
            if (workorderSearchModel.ArrivalDate.HasValue)
            {
                DateTime ArrivalFrmDt = Convert.ToDateTime(workorderSearchModel.ArrivalDate);
                DateTime ArrivalToDt = Convert.ToDateTime(workorderSearchModel.ArrivalDate).AddDays(1);

                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" wd.ArrivalDateTime >= '" + ArrivalFrmDt + "' and wd.ArrivalDateTime < '" + ArrivalToDt + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and wd.ArrivalDateTime >= '" + ArrivalFrmDt + "' and wd.ArrivalDateTime < '" + ArrivalToDt + "'");
                }
            }
            if (workorderSearchModel.CompletedDate.HasValue)
            {
                DateTime CompFrmDt = Convert.ToDateTime(workorderSearchModel.CompletedDate);
                DateTime CompToDt = Convert.ToDateTime(workorderSearchModel.CompletedDate).AddDays(1);

                if (woSearchWhereQuery.ToString() == " where ")
                {
                    woSearchWhereQuery.Append(@" wd.CompletionDateTime >= '" + CompFrmDt + "' and wd.CompletionDateTime < '" + CompToDt + "'");
                }
                else
                {
                    woSearchWhereQuery.Append(@" and wd.CompletionDateTime >= '" + CompFrmDt + "' and wd.CompletionDateTime < '" + CompToDt + "'");
                }
            }
            #endregion

            string finalWOSearchQuery = @" select dbo.getWOElapsedTime(woresults.WorkorderID) ElapsedTime,* from ( " + woSearchSelectQuery.ToString() + " " + woSearchJoinQuery.ToString() + " " + woSearchWhereQuery.ToString() + " ) woresults order by WorkorderEntryDate desc";
            //string finalWOSearchQuery = @" select * from ( " + woSearchSelectQuery.ToString() + " " + woSearchJoinQuery.ToString() + " " + woSearchWhereQuery.ToString() + " ) woresults order by WorkorderEntryDate desc";
            SqlHelper helper = new SqlHelper();


            DataTable dt = helper.GetDatatable(finalWOSearchQuery); ;

            int resultsCount = dt.Rows.Count;

            if (resultsCount > Convert.ToInt32(ConfigurationManager.AppSettings["MaxWorkOrderResultsCount"]))
            {
                ViewBag.WOSearchResultsMessage = "Please refine your search criteria, It has more than " + Convert.ToInt32(ConfigurationManager.AppSettings["MaxWorkOrderResultsCount"]) + " records";
            }
            else
            {

                foreach (DataRow dr in dt.Rows)
                {
                    if (dr.Table.Columns.Contains("CustomerID") && dr["CustomerID"] != DBNull.Value)
                    {
                        int customerID = Convert.ToInt32(dr["CustomerID"]);
                        Contact contact = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == customerID).FirstOrDefault();
                        if (contact != null)
                        {
                            NonFBCustomer nonFBCustomer = FarmerBrothersEntitites.NonFBCustomers.Where(n => n.NonFBCustomerId == contact.PricingParentID).FirstOrDefault();
                            if(nonFBCustomer != null && UserPrivilege["NonFBCustomer"] != "Full")
                            {
                                continue;
                            }

                        }
                    }



                    searchResults.Add(new WorkorderSearchResultModel(dr, true));
                }
            }
            return searchResults;
        }

        /*private IList<WorkorderSearchResultModel> GetWorkOrderDataUsingQuery(WorkorderSearchModel workorderSearchModel)
        {

            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();
            StringBuilder woSearchSelectQuery = new StringBuilder();
            StringBuilder woSearchWhereQuery = new StringBuilder();
            StringBuilder woSearchJoinQuery = new StringBuilder();

            #region Generate WO Search Query

            string SerialNumer = ""; string ContactId = ""; int ERFID = 0; string WorkOrderCallStatus = ""; string WOEntryStartDate = ""; string WOEntryEndDate = "";
            string AppointmentFromDate = ""; string AppointmentToDate = ""; string WorkorderCallTypeid = ""; string OriginalWorkorderId = ""; string CustomerCity = ""; string CustomerState="";
            string CustomerZip = ""; string PriorityCode = ""; int FollowupCallId = 0; string WorkorderTimeZone = ""; string Techid = ""; int ServiceCenterid = 0; int WorkorderId = 0;


            if (!string.IsNullOrEmpty(workorderSearchModel.SerialNumber))
            {
                SerialNumer = workorderSearchModel.SerialNumber;
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.CustomerId))
            {
               
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.ErfId))
            {
                
            }
            
            if (workorderSearchModel.Status != null && workorderSearchModel.Status.Count > 0)
            {
                string status = string.Empty;
                int i = 0;
                foreach (string s in workorderSearchModel.Status)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        //status += "'" + s + "',";

                        status += "'''" + s + "'''";

                        if(i != workorderSearchModel.Status.Count()-1)
                        {
                            status += "  , ','  ,";
                        }
                    }
                    i++;
                }

                WorkOrderCallStatus = "Concat("+status+")";

                //if (!string.IsNullOrEmpty(status))
                //{
                //    if (woSearchWhereQuery.ToString() == " where ")
                //    {
                //        woSearchWhereQuery.Append(@" w.WorkorderCallstatus in ( " + status.TrimEnd(',') + " )");
                //    }
                //    else
                //    {
                //        woSearchWhereQuery.Append(@" and w.WorkorderCallstatus in ( " + status.TrimEnd(',') + " )");
                //    }
                //}
            }

            TimeSpan time = new TimeSpan(23, 59, 59);            
            if (workorderSearchModel.DateFrom.HasValue)
            {
                WOEntryStartDate = workorderSearchModel.DateFrom.Value.Date.ToString();
            }

            if (workorderSearchModel.DateTo.HasValue)
            {
                WOEntryEndDate = (workorderSearchModel.DateTo.Value.Date + time).ToString();
            }

            if (workorderSearchModel.AppointmentDateTo.HasValue)
            {
                AppointmentToDate = (workorderSearchModel.AppointmentDateTo.Value.Date + time).ToString();
            }

            if (workorderSearchModel.AppointmentDateFrom.HasValue)
            {
                AppointmentFromDate = workorderSearchModel.AppointmentDateFrom.Value.Date.ToString();
            }



            if (workorderSearchModel.WorkorderType != null && workorderSearchModel.WorkorderType.Count > 0)
            {
                //string wtype = string.Empty;
                //foreach (string s in workorderSearchModel.WorkorderType)
                //{
                //    if (!string.IsNullOrEmpty(s))
                //    {
                //        wtype += "'" + s + "',";
                //    }
                //}
                //if (!string.IsNullOrEmpty(wtype))
                //{
                //    if (woSearchWhereQuery.ToString() == " where ")
                //    {
                //        woSearchWhereQuery.Append(@" w.WorkorderCalltypeid in( " + wtype.TrimEnd(',') + " )");
                //    }
                //    else
                //    {
                //        woSearchWhereQuery.Append(@" and w.WorkorderCalltypeid in ( " + wtype.TrimEnd(',') + " )");
                //    }
                //}

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.OriginalWorkOrderId))
            {
               
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.City))
            {
                
            }

            if (workorderSearchModel.State != null && workorderSearchModel.State.Count > 0)
            {
                //string stat = string.Empty;
                //foreach (string s in workorderSearchModel.State)
                //{
                //    if (!string.IsNullOrEmpty(s))
                //    {
                //        stat += "'" + s + "',";
                //    }
                //}
                //if (!string.IsNullOrEmpty(stat))
                //{
                //    if (woSearchWhereQuery.ToString() == " where ")
                //    {
                //        woSearchWhereQuery.Append(@" w.CustomerState in ( " + stat.TrimEnd(',') + " )");
                //    }
                //    else
                //    {
                //        woSearchWhereQuery.Append(@" and w.CustomerState in ( " + stat.TrimEnd(',') + " )");
                //    }
                //}

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.Zipcode))
            {
                //if (woSearchWhereQuery.ToString() == " where ")
                //{
                //    woSearchWhereQuery.Append(@" w.CustomerZipCode like '%" + workorderSearchModel.Zipcode + "%'");
                //}
                //else
                //{
                //    woSearchWhereQuery.Append(@" and w.CustomerZipCode like '%" + workorderSearchModel.Zipcode + "%'");
                //}

            }

            if (workorderSearchModel.Priority != null && workorderSearchModel.Priority.Count > 0)
            {
                //string pr = string.Empty;
                //foreach (string s in workorderSearchModel.Priority)
                //{
                //    if (!string.IsNullOrEmpty(s))
                //    {
                //        pr += "'" + s + "',";
                //    }
                //}
                //if (!string.IsNullOrEmpty(pr))
                //{
                //    if (woSearchWhereQuery.ToString() == " where ")
                //    {
                //        woSearchWhereQuery.Append(@" w.PriorityCode in ( " + pr.TrimEnd(',') + " )");
                //    }
                //    else
                //    {
                //        woSearchWhereQuery.Append(@" and w.PriorityCode in ( " + pr.TrimEnd(',') + " )");
                //    }
                //}

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.FollowupCall.ToString()))
            {
                //if (workorderSearchModel.FollowupCall.ToString() != "603")
                //{
                //    if (woSearchWhereQuery.ToString() == " where ")
                //    {
                //        woSearchWhereQuery.Append(@" w.FollowupCallID = '" + workorderSearchModel.FollowupCall + "'");
                //    }
                //    else
                //    {
                //        woSearchWhereQuery.Append(@" and w.FollowupCallID = '" + workorderSearchModel.FollowupCall + "'");
                //    }
                //}

            }

            if (workorderSearchModel.TimeZone != null && workorderSearchModel.TimeZone.Count > 0)
            {
                //string tz = string.Empty;
                //foreach (string s in workorderSearchModel.TimeZone)
                //{
                //    if (!string.IsNullOrEmpty(s))
                //    {
                //        tz += "'" + s + "',";
                //    }
                //}
                //if (!string.IsNullOrEmpty(tz))
                //{
                //    if (woSearchWhereQuery.ToString() == " where ")
                //    {
                //        woSearchWhereQuery.Append(@" w.WorkorderTimeZone in( " + tz.TrimEnd(',') + " )");
                //    }
                //    else
                //    {
                //        woSearchWhereQuery.Append(@" and w.WorkorderTimeZone in ( " + tz.TrimEnd(',') + " )");
                //    }
                //}
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.TechId.ToString()))
            {
                //if (woSearchWhereQuery.ToString() == " where ")
                //{
                //    woSearchWhereQuery.Append(@" ws.Techid like '%" + workorderSearchModel.TechId + "%'");
                //}
                //else
                //{
                //    woSearchWhereQuery.Append(@" and ws.Techid like '%" + workorderSearchModel.TechId + "%'");
                //}

            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.TechType) && workorderSearchModel.ServiceCompany <= 0)
            {
                //IList<BranchModel> serviceCenterList = GetServiceCenters(workorderSearchModel.TechType);
                //IList<int?> serviceCenterIds = serviceCenterList.Select(b => b.ServiceCenterId).ToList();
                //string tz = string.Empty;
                //foreach (int s in serviceCenterIds)
                //{
                //    tz += s + ",";
                //}
                //if (!string.IsNullOrEmpty(tz))
                //{
                //    if (woSearchWhereQuery.ToString() == " where ")
                //    {
                //        woSearchWhereQuery.Append(@" ws.ServiceCenterID in( " + tz.TrimEnd(',') + " )");
                //    }
                //    else
                //    {
                //        woSearchWhereQuery.Append(@" and ws.ServiceCenterID in ( " + tz.TrimEnd(',') + " )");
                //    }
                //}
            }

            if (workorderSearchModel.ServiceCompany > 0)
            {
                //if (woSearchWhereQuery.ToString() == " where ")
                //{
                //    woSearchWhereQuery.Append(@" ws.ServiceCenterID = " + workorderSearchModel.ServiceCompany);
                //}
                //else
                //{
                //    woSearchWhereQuery.Append(@" and ws.ServiceCenterID = " + workorderSearchModel.ServiceCompany);
                //}

            }
            bool hasOnlyWorkOrderId = false;
            if (!string.IsNullOrWhiteSpace(workorderSearchModel.WorkorderId))
            {
                //if (woSearchWhereQuery.ToString() == " where ")
                //{
                //    hasOnlyWorkOrderId = true;
                //    woSearchWhereQuery.Append(@" w.WorkorderID like '%" + workorderSearchModel.WorkorderId + "%'");
                //}
                //else
                //{
                //    woSearchWhereQuery.Append(@" and w.WorkorderID like '%" + workorderSearchModel.WorkorderId + "%'");

                //}

            }
            if (!hasOnlyWorkOrderId)
            {
                //if (workorderSearchModel.Status == null || workorderSearchModel.Status.Count <= 0 ||
                //    (workorderSearchModel.Status != null && workorderSearchModel.Status.Count > 0 &&
                //    !(workorderSearchModel.Status.Exists(x => string.Equals(x, "Closed", StringComparison.OrdinalIgnoreCase)))))
                //{
                //    if (woSearchWhereQuery.ToString() == " where ")
                //    {

                //        woSearchWhereQuery.Append(@" w.WorkorderCallstatus != 'Closed'");
                //    }
                //    else
                //    {
                //        woSearchWhereQuery.Append(@" and w.WorkorderCallstatus != 'Closed'");
                //    }
                //}

            }


            #endregion

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fbWorkorderSearch("USP_WO_Search", SerialNumer, ContactId, ERFID, WorkOrderCallStatus, WOEntryStartDate, WOEntryEndDate,
            AppointmentFromDate, AppointmentToDate, WorkorderCallTypeid, OriginalWorkorderId, CustomerCity, CustomerState,
            CustomerZip, PriorityCode, FollowupCallId, WorkorderTimeZone, Techid, ServiceCenterid, WorkorderId);

            int resultsCount = dt.Rows.Count;

            if (resultsCount > Convert.ToInt32(ConfigurationManager.AppSettings["MaxWorkOrderResultsCount"]))
            {
                ViewBag.WOSearchResultsMessage = "Please refine your search criteria, It has more than " + Convert.ToInt32(ConfigurationManager.AppSettings["MaxWorkOrderResultsCount"]) + " records";
            }
            else
            {

                foreach (DataRow dr in dt.Rows)
                {
                    searchResults.Add(new WorkorderSearchResultModel(dr, true));
                }
            }
            return searchResults;
        }
        */
        private IList<WorkorderSearchResultModel> GetWorkOrderData(WorkorderSearchModel workorderSearchModel)
        {
            WorkOrder originalWorkOrder = null;

            var predicate = PredicateBuilder.True<WorkOrder>();

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.WorkorderId))
            {
                predicate = predicate.And(w => w.WorkorderID.ToString().Contains(workorderSearchModel.WorkorderId));
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.CustomerId))
            {
                predicate = predicate.And(w => w.CustomerID.ToString().Contains(workorderSearchModel.CustomerId));
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.ErfId))
            {
                predicate = predicate.And(w => w.WorkorderErfid.ToString().Contains(workorderSearchModel.ErfId));
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.OriginalWorkOrderId))
            {
                predicate = predicate.And(w => w.OriginalWorkorderid.ToString().Contains(workorderSearchModel.OriginalWorkOrderId));
                originalWorkOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID.ToString().Contains(workorderSearchModel.OriginalWorkOrderId)).FirstOrDefault();
            }

            if (workorderSearchModel.Status != null && workorderSearchModel.Status.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(workorderSearchModel.Status[0]))
                {
                    predicate = predicate.And(w => workorderSearchModel.Status.Contains(w.WorkorderCallstatus.ToString()));
                }
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.SerialNumber))
            {
                predicate = predicate.And(w => w.WorkorderEquipments.Any(we => we.SerialNumber == workorderSearchModel.SerialNumber));
            }

            if (workorderSearchModel.FollowupCall > 0)
            {
                predicate = predicate.And(w => w.FollowupCallID == workorderSearchModel.FollowupCall || w.FollowupCallID == null);
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.AutoDispatched))
            {
                predicate = predicate.And(w => string.Equals(w.AutoDispatch, workorderSearchModel.AutoDispatched));
            }

            if (workorderSearchModel.WorkorderType != null && workorderSearchModel.WorkorderType.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(workorderSearchModel.WorkorderType[0]))
                {
                    predicate = predicate.And(w => workorderSearchModel.WorkorderType.Contains(w.WorkorderCalltypeid.ToString()));
                }
            }

            if (workorderSearchModel.Priority != null && workorderSearchModel.Priority.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(workorderSearchModel.Priority[0]))
                {
                    predicate = predicate.And(w => workorderSearchModel.Priority.Contains(w.PriorityCode.ToString()));
                }
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.ProjectID))
            {
                predicate = predicate.And(w => w.CustomerCity.Contains(workorderSearchModel.ProjectID));
            }


            if (!string.IsNullOrWhiteSpace(workorderSearchModel.City))
            {
                predicate = predicate.And(w => w.CustomerCity.Contains(workorderSearchModel.City));
            }

            if (workorderSearchModel.State != null && workorderSearchModel.State.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(workorderSearchModel.State[0]))
                {
                    predicate = predicate.And(w => workorderSearchModel.State.Contains(w.CustomerState));
                }
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.Zipcode))
            {
                predicate = predicate.And(w => w.CustomerZipCode.Contains(workorderSearchModel.Zipcode));
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.CoverageZone))
            {
                predicate = predicate.And(w => w.CoverageZone.ToString().Contains(workorderSearchModel.CoverageZone));
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.TSM))
            {
                predicate = predicate.And(w => string.Equals(w.Tsm, workorderSearchModel.TSM));
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.TechType) && workorderSearchModel.ServiceCompany <= 0)
            {
                IList<BranchModel> serviceCenterList = GetServiceCenters(workorderSearchModel.TechType);
                IList<int?> serviceCenterIds = serviceCenterList.Select(b => b.ServiceCenterId).ToList();
                predicate = predicate.And(w => w.WorkorderSchedules.Any(ws => serviceCenterIds.Contains(ws.ServiceCenterID)));
            }

            if (workorderSearchModel.ServiceCompany > 0)
            {
                predicate = predicate.And(w => w.WorkorderSchedules.Any(ws => ws.ServiceCenterID == workorderSearchModel.ServiceCompany));
            }

            if (workorderSearchModel.Technician.HasValue && workorderSearchModel.Technician.Value > 0)
            {
                predicate = predicate.And(w => w.WorkorderSchedules.Any(ws => ws.Techid == workorderSearchModel.Technician));
            }

            if (workorderSearchModel.TechId.HasValue && workorderSearchModel.TechId.Value > 0)
            {
                predicate = predicate.And(w => w.WorkorderSchedules.Any(ws => ws.Techid == workorderSearchModel.TechId.Value));
            }

            if (!string.IsNullOrWhiteSpace(workorderSearchModel.FSR))
            {
                string query = @"Select * from Feast_FSR where Feast_FSR like '%" + workorderSearchModel.FSR + "%'";
                IList<string> states = FarmerBrothersEntitites.Database.SqlQuery<FSRView>(query).Select(f => f.STATE).ToList();
                predicate = predicate.And(w => states.Contains(w.CustomerState));
            }

            if (workorderSearchModel.TimeZoneName != null)
            {
                if (!string.IsNullOrWhiteSpace(workorderSearchModel.TimeZoneName))
                {
                    int timezone = Convert.ToInt32(workorderSearchModel.TimeZoneName);
                    predicate = predicate.And(w => w.WorkorderTimeZone == timezone);
                }
            }

            if (workorderSearchModel.FSM > 0)
            {
                predicate = predicate.And(w => w.FSMID == workorderSearchModel.FSM);
            }

            if (workorderSearchModel.TeamLead.HasValue && workorderSearchModel.TeamLead.Value > 0)
            {
                predicate = predicate.And(w => w.WorkorderSchedules.Any(ws => ws.AssignedStatus == "Accepted" && ws.PrimaryTech >= 0 && ws.TeamLeadID == workorderSearchModel.TeamLead.Value));
            }

            TimeSpan time = new TimeSpan(23, 59, 59);
            if (workorderSearchModel.DateTo.HasValue)
            {
                workorderSearchModel.DateTo = workorderSearchModel.DateTo.Value.Date + time;
            }
            if (workorderSearchModel.DateFrom.HasValue && workorderSearchModel.DateTo.HasValue)
            {
                predicate = predicate.And(w => w.WorkorderEntryDate >= workorderSearchModel.DateFrom && w.WorkorderEntryDate <= workorderSearchModel.DateTo);
            }
            else if (workorderSearchModel.DateFrom.HasValue)
            {
                predicate = predicate.And(w => w.WorkorderEntryDate >= workorderSearchModel.DateFrom);
            }
            else if (workorderSearchModel.DateTo.HasValue)
            {
                predicate = predicate.And(w => w.WorkorderEntryDate <= workorderSearchModel.DateTo);
            }


            if (workorderSearchModel.AppointmentDateTo.HasValue)
            {
                workorderSearchModel.AppointmentDateTo = workorderSearchModel.AppointmentDateTo.Value.Date + time;
            }

            if (workorderSearchModel.AppointmentDateFrom.HasValue && workorderSearchModel.AppointmentDateTo.HasValue)
            {
                predicate = predicate.And(w => w.AppointmentDate >= workorderSearchModel.AppointmentDateFrom && w.AppointmentDate <= workorderSearchModel.AppointmentDateTo);
            }
            else if (workorderSearchModel.AppointmentDateFrom.HasValue)
            {
                predicate = predicate.And(w => w.AppointmentDate >= workorderSearchModel.AppointmentDateFrom);
            }
            else if (workorderSearchModel.AppointmentDateTo.HasValue)
            {
                predicate = predicate.And(w => w.AppointmentDate <= workorderSearchModel.AppointmentDateTo);
            }

            int resultsCount = FarmerBrothersEntitites.Set<WorkOrder>().AsExpandable().Where(predicate).Count();
            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();

            if (resultsCount > Convert.ToInt32(ConfigurationManager.AppSettings["MaxWorkOrderResultsCount"]))
            {
                ViewBag.WOSearchResultsMessage = "Please refine your search criteria, It has more than " + Convert.ToInt32(ConfigurationManager.AppSettings["MaxWorkOrderResultsCount"]) + " records";
            }
            else
            {
                if (resultsCount > 500)
                {
                    workorderSearchModel.DateFrom = DateTime.Now.AddDays(-30);
                    predicate = predicate.And(w => w.WorkorderEntryDate >= workorderSearchModel.DateFrom);
                    IQueryable<WorkOrder> workOrders = FarmerBrothersEntitites.Set<WorkOrder>().AsExpandable().Where(predicate).OrderByDescending(w => w.WorkorderID);

                    foreach (WorkOrder workOrder in workOrders)
                    {
                        searchResults.Add(new WorkorderSearchResultModel(workOrder));
                    }
                }
                else
                {
                    IQueryable<WorkOrder> workOrders = FarmerBrothersEntitites.Set<WorkOrder>().AsExpandable().Where(predicate).OrderByDescending(w => w.WorkorderID);
                    foreach (WorkOrder workOrder in workOrders)
                    {
                        searchResults.Add(new WorkorderSearchResultModel(workOrder));
                    }
                }
            }



            if (searchResults.Count > 0 && originalWorkOrder != null)
            {
                searchResults.Insert(0, new WorkorderSearchResultModel(originalWorkOrder));
            }

            return searchResults;
        }

        #endregion

        #region create workorder

        public JsonResult GetCloserNonTaggedManufacturer(string skuValue)
        {
            IQueryable<string> vendors = null;

            if (string.IsNullOrWhiteSpace(skuValue))
            {
                vendors = FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true).OrderBy(s => s.Supplier).Select(s => s.Supplier).Distinct();
            }
            else
            {
                vendors = FarmerBrothersEntitites.FBClosureParts.Where(s => s.ItemNo == skuValue && s.SkuActive == true).OrderBy(s => s.Supplier).Select(s => s.Supplier).Distinct();
            }

            var data = new List<object>();
            foreach (string vendor in vendors)
            {
                data.Add(new { value = vendor.ToUpper().Trim(), text = vendor.ToUpper().Trim() });
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetCloserSkuDescription(string skuValue)
        {
            string skuDescription = string.Empty;
            FBClosurePart sku = FarmerBrothersEntitites.FBClosureParts.Where(s => s.ItemNo == skuValue).FirstOrDefault();
            if (sku != null)
            {
                skuDescription = sku.Description;
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = skuDescription };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetSkuCost(string skuValue)
        {
            decimal skuCost = 0;
            Sku sku = FarmerBrothersEntitites.Skus.Where(s => s.Sku1 == skuValue).FirstOrDefault();
            if (sku != null)
            {
                skuCost = sku.SKUCost != null ? Convert.ToDecimal(sku.SKUCost) : 0 ;
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = skuCost };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }


        public JsonResult GetNonTaggedManufacturer(string skuValue)
        {
            string skuDesc = string.Empty;
            string skuMfg = string.Empty;
            var data = new List<object>();
            FBSKU sku = FarmerBrothersEntitites.FBSKUs.Where(s => s.SKU == skuValue && s.SKUActive == true).FirstOrDefault();
            if (sku != null)
            {
                data.Add(new { Manufacturer = sku.VendorCode.ToUpper().Trim(), Description = sku.Description.ToUpper().Trim() });
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetSkuDescription(string skuValue)
        {
            string skuDescription = string.Empty;
            FBSKU sku = FarmerBrothersEntitites.FBSKUs.Where(s => s.SKU == skuValue).FirstOrDefault();
            if (sku != null)
            {
                skuDescription = sku.Description;
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = skuDescription };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetKnownEquipment(string serialNumber)
        {
            KnownEquipmentModel knownEquipmentModel = null;
            if (TempData["KnownEquipments"] != null)
            {
                List<KnownEquipmentModel> knownEquipment = TempData["KnownEquipments"] as List<KnownEquipmentModel>;
                if (knownEquipment != null)
                {
                    knownEquipmentModel = knownEquipment.Where(ke => ke.SerialNumber == serialNumber).FirstOrDefault();
                }
                TempData["KnownEquipments"] = knownEquipment;
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = knownEquipmentModel };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;

        }

        public JsonResult GetTaggedSkus(string manufacturer)
        {
            IEnumerable<Sku> skus = FarmerBrothersEntitites.Skus.Where(s => s.Manufacturer == manufacturer && s.EQUIPMENT_TAG == "TAGGED");
            var data = new List<object>();
            foreach (Sku sku in skus)
            {
                data.Add(new { value = sku.Sku1, text = sku.Sku1 });
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetFBClosureSku(string manufacturer, string Desc)
        {
            FBClosurePart skus = FarmerBrothersEntitites.FBClosureParts.Where(s => s.Supplier == manufacturer && s.Description == Desc).FirstOrDefault();
            string skuValue = "";

            if (skus != null)
            {
                skuValue = skus.ItemNo;
            }            

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = skuValue };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "WorkorderSave")]
        [ActionName("SaveWorkOrder")]
        public JsonResult SaveWorkOrder([ModelBinder(typeof(WorkorderManagementModelBinder))] WorkorderManagementModel workorderManagement, HttpPostedFileBase fileToUpload, string foo, bool isAutoGenWO = false, bool FromERF = false)
        {
            int returnValue = -1;
            WorkOrder workOrder = null;
            string message = string.Empty;

            if (workorderManagement.Customer != null)
            {
                workorderManagement.Customer = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, workorderManagement.Customer);
            }

            TimeZoneInfo newTimeZoneInfo = null;
            Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
            DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
            switch (workorderManagement.Operation)
            {
                case WorkOrderManagementSubmitType.SAVEANDPAY:
                case WorkOrderManagementSubmitType.SAVE:
                    {
                        returnValue = -1;
                        workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);

                        bool isValid = true;

                        if (workorderManagement.WorkOrder.WorkorderCallstatus == "Hold for AB")
                        {
                            var zip = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == workorderManagement.Customer.ZipCode).FirstOrDefault();
                            if (zip == null)
                            {
                                message = @"|Please enter valid Zip code!";
                                isValid = false;
                            }

                        }
                        else if (string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Closed", true) != 0)
                        {
                            int tmpCustomerId = Convert.ToInt32(workorderManagement.Customer.CustomerId);
                            string DBCustomerZipCode = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == tmpCustomerId).Select(id => id.PostalCode).FirstOrDefault();

                            if (workorderManagement.customerZipcode == DBCustomerZipCode)
                            {
                                var zip = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == workorderManagement.Customer.ZipCode).FirstOrDefault();
                                if (zip == null)
                                {
                                    message = @"|Please enter valid Zip code!";
                                    isValid = false;
                                }
                            }
                            else
                            {
                                message = @"|Please Update Customer Details before saving Work Order!";
                                isValid = false;
                            }
                        }

                        if (string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Closed", true) != 0
                            && workorderManagement.IsNewPartsOrder != true
                            /*&& workorderManagement.WorkOrder.WorkorderCalltypeid != 1800
                            && workorderManagement.WorkOrder.WorkorderCalltypeid != 1810
                            && workorderManagement.WorkOrder.WorkorderCalltypeid != 1820*/)
                            //&& workorderManagement.Closure.SpecialClosure.Contains("No Service Required") == false)
                        {
                            if (workorderManagement.WorkOrder.PriorityCode.HasValue == false)
                            {
                                message = @"|Priority field is required!";
                                isValid = false;
                            }

                            if (string.IsNullOrWhiteSpace(workorderManagement.WorkOrder.WorkorderContactName))
                            {
                                message += @"|Work Order Contact Name field is required!";
                                isValid = false;
                            }
                            if (string.IsNullOrWhiteSpace(workorderManagement.WorkOrder.HoursOfOperation))
                            {
                                message += @"|Work Order Hours Of Operation field is required!";
                                isValid = false;
                            }

                            if (string.IsNullOrWhiteSpace(workorderManagement.WorkOrder.CallerName))
                            {
                                message += @"|Caller Name field is required!";
                                isValid = false;
                            }

                            if (string.IsNullOrWhiteSpace(workorderManagement.Closure.SpecialClosure))
                            {
                                workorderManagement.Closure.SpecialClosure = string.Empty;
                            }

                            if (/*workorderManagement.WorkOrder.WorkorderCalltypeid != 1800
                            && workorderManagement.WorkOrder.WorkorderCalltypeid != 1810
                            && workorderManagement.WorkOrder.WorkorderCalltypeid != 1820
                            &&*/ workorderManagement.IsNewPartsOrder != true
                            && workorderManagement.Closure.SpecialClosure.Contains("No Service Required") == false
                            && workorderManagement.Closure.SpecialClosure.Contains("Cancellation") == false)
                            {
                                if (workorderManagement.WorkOrderEquipmentsRequested.Count <= 0)
                                {
                                    message += @"|At least one equipment required in Work Requested section!";
                                    isValid = false;
                                }

                                if (workorderManagement.WorkOrderEquipmentsRequested.Count > 0)
                                {
                                    int nCount = 0;
                                    foreach (WorkOrderManagementEquipmentModel equipment in workorderManagement.WorkOrderEquipmentsRequested)
                                    {
                                        if (equipment.CallTypeID.HasValue == false || equipment.CallTypeID == 0)
                                        {
                                            message += @"|Service Code is required for equipment at row " + nCount;
                                            isValid = false;
                                        }

                                        if (string.IsNullOrWhiteSpace(equipment.Category))
                                        {
                                            message += @"|Equipment Type is required for equipment at row " + nCount;
                                            isValid = false;
                                        }

                                        if (string.IsNullOrWhiteSpace(equipment.Location))
                                        {
                                            //message += @"|Location is required for equipment at row " + nCount;
                                            //isValid = false;

                                            equipment.Location = "N/A";
                                        }
                                        nCount++;
                                    }
                                }
                            }
                        }

                        if (workorderManagement.WorkOrderParts != null
                                && workorderManagement.WorkOrderParts.Count > 0)
                        {
                            int nCount = 0;
                            foreach (WorkOrderPartModel part in workorderManagement.WorkOrderParts)
                            {
                                if (part.Quantity.HasValue)
                                {
                                    if (part.Quantity.Value <= 0)
                                    {
                                        message += @"|Quantity is not valid at row " + nCount;
                                        isValid = false;
                                    }
                                }
                                else
                                {
                                    message += @"|Quantity is not valid at row " + nCount;
                                    isValid = false;
                                }
                                nCount++;
                            }
                        }

                        //if (workorderManagement.Customer.ServiceTier == "5" && (!string.IsNullOrEmpty(workorderManagement.Customer.CustomerType) && workorderManagement.Customer.CustomerType.ToLower() != "ce")
                        //     && (string.IsNullOrEmpty(workorderManagement.Customer.ParentNumber) || workorderManagement.Customer.ParentNumber == "0"))
                        if (workorderManagement.WorkOrder.WorkorderID == 0 && !string.IsNullOrEmpty(workorderManagement.Customer.BillingCode) && workorderManagement.Customer.BillingCode.ToLower() == "s08")
                        {
                            if (string.IsNullOrEmpty(workorderManagement.PaymentTransactionId))
                            {
                                message = @"|Credit Card Processing Incomplete";
                                isValid = false;
                            }
                        }

                        if (workorderManagement.NewNotes.Count <= 0)
                        {
                            message = @"|Notes required to save Work Order!";
                            isValid = false;
                        }
                        else if (workorderManagement.NewNotes.Count > 0)
                        {
                            NewNotesModel newNotes = workorderManagement.NewNotes.ElementAt(0);
                            if (string.IsNullOrWhiteSpace(newNotes.Text))
                            {
                                message = @"|Notes can not be blank!";
                                isValid = false;
                            }
                            if (workorderManagement.Notes.IsSpecificTechnician)
                            {
                                if (string.IsNullOrWhiteSpace(workorderManagement.Notes.TechID))
                                {
                                    message = @"|Specific Technician required!";
                                    isValid = false;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(workorderManagement.Customer.ParentNumber))
                        {
                            if (workorderManagement.Customer.ParentNumber.Length > 8)
                            {
                                message = @"|Please Enter Valid Parent Number!";
                                returnValue = -1;
                                isValid = false;
                            }
                        }
                        
                        //if(workorderManagement.Customer.IsNonFBCustomer && (string.IsNullOrEmpty(workorderManagement.Customer.NonFBCustomerNumber) || workorderManagement.Customer.NonFBCustomerNumber.ToLower() == "n/a"))
                        //{
                        //    message = @"|Please Select NonFBCustomer from the List!";
                        //    returnValue = -1;
                        //    isValid = false;
                        //}

                        /* if (workorderManagement.WorkOrder.WorkorderCallstatus != "Closed")
                         {
                             if ((workorderManagement.IsBillableFeed == false && workorderManagement.BillableSKUList.Count > 0))
                             {
                                 message += @"|Please select IsBillable checkbox before adding sku's to the work order";
                                 isValid = false;
                             }
                             if ((workorderManagement.IsBillableFeed == true && workorderManagement.BillableSKUList.Count == 0))
                             {
                                 message += @"|Please add sku's when IsBillable checkbox is selected";
                                 isValid = false;
                             }
                         }
                         if (workorderManagement.IsBillableFeed == true && workorderManagement.BillableSKUList.Count > 0)
                         {
                             bool DuplicateSkusExist = ValidateSkuList(workorderManagement.BillableSKUList);
                             if (DuplicateSkusExist)
                             {
                                 message += @"|Duplicate SKUs are added to the Billable SKU Grid";
                                 isValid = false;
                             }
                         }*/

                        if(workorderManagement.IsNewPartsOrder)
                        {
                            if(workorderManagement.WorkOrderParts.Count() == 0)
                            {
                                message += @"|At least one part is required for Parts order save!";
                                isValid = false;
                            }
                            if (!workorderManagement.WorkOrder.DateNeeded.HasValue)
                            {
                                message += @"|Date Needed field is required!";
                                isValid = false;
                            }
                            if (string.IsNullOrEmpty(workorderManagement.ShippingPriority) || workorderManagement.ShippingPriority == "0")
                            {
                                message += @"|Shipping Priority field is required!";
                                isValid = false;
                            }

                            if(workorderManagement.IsCustomerAlternateAddress)
                            {
                                if(string.IsNullOrEmpty(workorderManagement.CustomerOtherPartsName))
                                {
                                    message += @"|Alternate Name field is required!";
                                    isValid = false;
                                }
                                if (string.IsNullOrEmpty(workorderManagement.CustomerOtherPartsContactName))
                                {
                                    message += @"|Alternate Contact Name field is required!";
                                    isValid = false;
                                }
                                if (string.IsNullOrEmpty(workorderManagement.CustomerOtherPartsAddress1))
                                {
                                    message += @"|Alternate Address field is required!";
                                    isValid = false;
                                }
                                if (string.IsNullOrEmpty(workorderManagement.CustomerOtherPartsCity))
                                {
                                    message += @"|Alternate City field is required!";
                                    isValid = false;
                                }
                                if (string.IsNullOrEmpty(workorderManagement.CustomerOtherPartsState) || workorderManagement.CustomerOtherPartsState.ToLower() == "n/a")
                                {
                                    message += @"|Alternate State field is required!";
                                    isValid = false;
                                }
                                if (string.IsNullOrEmpty(workorderManagement.CustomerOtherPartsZip))
                                {
                                    message += @"|Alternate Zip field is required!";
                                    isValid = false;
                                }
                            }

                        }

                        if (isValid == true)
                        {
                            returnValue = WorkOrderSave(workorderManagement, workorderManagement.Operation, out workOrder, out message);
                        }
                    }
                    break;
                case WorkOrderManagementSubmitType.NOTIFYSALES:
                    returnValue = NotifySales(workorderManagement, out workOrder);
                    break;
                case WorkOrderManagementSubmitType.OVERTIMEREQUEST:
                    returnValue = OverTimeRequest(workorderManagement, out workOrder);
                    break;
                case WorkOrderManagementSubmitType.PUTONHOLD:
                    {
                        if (workorderManagement.WorkOrder.WorkorderID > 0)
                        {
                            workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);

                            string WOHoldNotes = string.Empty;

                            if (string.Compare(workOrder.WorkorderCallstatus, "Hold", 0) == 0)
                            {
                                if (workOrder.WorkorderSchedules == null || workOrder.WorkorderSchedules.Count == 0)
                                {
                                    workOrder.WorkorderCallstatus = "Open";
                                }
                                else if (workOrder.WorkorderSchedules != null && workOrder.WorkorderSchedules.Count > 0)
                                {
                                    WorkorderSchedule AcceptedTechWorkOrderSchedule = workOrder.WorkorderSchedules.Where(sch => sch.AssignedStatus == "Accepted").FirstOrDefault();
                                    WorkorderSchedule SentTechWorkOrderSchedule = workOrder.WorkorderSchedules.Where(sch => sch.AssignedStatus == "Sent").FirstOrDefault();
                                    WorkorderSchedule ScheduledTechWorkOrderSchedule = workOrder.WorkorderSchedules.Where(sch => sch.AssignedStatus == "Scheduled").FirstOrDefault();

                                    if (AcceptedTechWorkOrderSchedule != null)
                                    {
                                        if (workorderManagement.Closure.CompletionDateTime != null)
                                        {
                                            workOrder.WorkorderCallstatus = "Complete";
                                        }
                                        else if (workorderManagement.Closure.ArrivalDateTime != null)
                                        {
                                            workOrder.WorkorderCallstatus = "On Site";
                                        }
                                        else
                                        {
                                            workOrder.WorkorderCallstatus = "Accepted";
                                        }
                                    }
                                    else if (SentTechWorkOrderSchedule != null)
                                    {
                                        workOrder.WorkorderCallstatus = "Pending Acceptance";
                                    }
                                    else if(ScheduledTechWorkOrderSchedule != null)
                                    {
                                        workOrder.WorkorderCallstatus = "Scheduled";
                                    }
                                    else
                                    {
                                        workOrder.WorkorderCallstatus = "Open";
                                    }
                                }
                                
                                WOHoldNotes = "Work Order removed from Hold Status by User: " + UserName;
                                //workOrder.WorkorderCallstatus = workOrder.WorkorderSchedules.Where()

                            }
                            else
                            {
                                workOrder.WorkorderCallstatus = "Hold";
                                WOHoldNotes = "Work Order put on hold by User : " + UserName;
                            }

                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = WOHoldNotes,
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = UserName,
                                isDispatchNotes = 1
                            };
                            workOrder.NotesHistories.Add(notesHistory);
                        }

                        workOrder.WorkorderModifiedDate = currentTime;
                        workOrder.ModifiedUserName = UserName;
                        int effectedRecords = FarmerBrothersEntitites.SaveChanges();
                        returnValue = effectedRecords > 0 ? 1 : 0;
                    }
                    break;
                case WorkOrderManagementSubmitType.UPDATEAPPOINTMENT:
                    {
                        if (workorderManagement.WorkOrder.WorkorderID > 0)
                        {
                            workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);

                            if (workOrder.AppointmentDate != workorderManagement.WorkOrder.AppointmentDate)
                            {

                                string notes = string.Empty;
                                if (workOrder.AppointmentDate.HasValue)
                                {
                                    if (workorderManagement.WorkOrder.AppointmentDate.HasValue)
                                    {
                                        notes = "Appointment date is changed from " + workOrder.AppointmentDate.Value.ToString("d") + " to " + workorderManagement.WorkOrder.AppointmentDate.Value.ToString("d");
                                    }
                                    else
                                    {
                                        notes = "Appointment is cancelled from " + workOrder.AppointmentDate.Value.ToString("d");
                                    }
                                }
                                else if (workorderManagement.WorkOrder.AppointmentDate.HasValue)
                                {
                                    notes = "Appointment is scheduled at " + workorderManagement.WorkOrder.AppointmentDate.Value.ToString("d");
                                }

                                WorkorderReasonlog workOrderLog = new WorkorderReasonlog()
                                {
                                    EntryDate = currentTime,
                                    Notes = notes,
                                    ReasonFor = "Appointment Date",
                                    WorkorderID = workOrder.WorkorderID,
                                    NewAppointmentDate = workorderManagement.WorkOrder.AppointmentDate,
                                    OldAppointmentDate = workOrder.AppointmentDate
                                };

                                AllFBStatu appointmentReason = FarmerBrothersEntitites.AllFBStatus.Where(r => r.FBStatusID == workorderManagement.AppointmentUpdateReason).FirstOrDefault();
                                if (appointmentReason != null)
                                {
                                    workOrderLog.Reasonid = appointmentReason.FBStatusID;
                                    workOrderLog.ReasonFor = appointmentReason.FBStatus;
                                }

                                FarmerBrothersEntitites.WorkorderReasonlogs.Add(workOrderLog);

                                workOrder.AppointmentDate = workorderManagement.WorkOrder.AppointmentDate;

                                if (appointmentReason != null)
                                {
                                    notes = notes + " - Reason : " + appointmentReason.FBStatus;
                                }

                                NotesHistory notesHistory = new NotesHistory()
                                {
                                    AutomaticNotes = 1,
                                    EntryDate = currentTime,
                                    Notes = notes,
                                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                    UserName = UserName,
                                    isDispatchNotes = 1
                                };

                                workOrder.NotesHistories.Add(notesHistory);

                                workOrder.WorkorderModifiedDate = currentTime;
                                workOrder.ModifiedUserName = UserName;
                                int effectedRecords = FarmerBrothersEntitites.SaveChanges();
                                returnValue = effectedRecords > 0 ? 1 : 0;
                            }
                        }
                    }
                    break;
                case WorkOrderManagementSubmitType.COMPLETE:
                    {
                        returnValue = WorkOrderSave(workorderManagement, workorderManagement.Operation, out workOrder, out message);

                        if (workorderManagement.WorkOrder.WorkorderID > 0)
                        {
                            returnValue = -1;
                            workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);

                            bool isValid = true;
                            bool isNSRAsset = false;
                            WorkorderDetail workOrderDetail = FarmerBrothersEntitites.WorkorderDetails.FirstOrDefault(wd => wd.WorkorderID == workorderManagement.WorkOrder.WorkorderID);

                            if ((workorderManagement.IsBillableFeed == false && workorderManagement.BillableSKUList.Count > 0))
                            {
                                message += @"|Please select IsBillable checkbox before adding sku's to the work order";
                                isValid = false;
                            }
                            if ((workorderManagement.IsBillableFeed == true && workorderManagement.BillableSKUList.Count == 0))
                            {
                                message += @"|Please add sku's when IsBillable checkbox is selected";
                                isValid = false;
                            }
                            if(workorderManagement.IsBillableFeed == true && workorderManagement.BillableSKUList.Count > 0)
                            {
                                bool DuplicateSkusExist = ValidateSkuList(workorderManagement.BillableSKUList);
                                if(DuplicateSkusExist)
                                {
                                    message += @"|Duplicate SKUs are added to the Billable SKU Grid";
                                    isValid = false;
                                }
                            }

                            if (workOrderDetail != null)
                            {
                                if (!workOrderDetail.Mileage.HasValue)
                                {
                                    message = @"Mileage is not updated!";
                                    isValid = false;
                                }
                                if (!workOrderDetail.StartDateTime.HasValue)
                                {
                                    message += @"|Start Date & Time are not updated!";
                                    isValid = false;
                                }

                                if (!workOrderDetail.ArrivalDateTime.HasValue)
                                {
                                    message += @"|Arrival Date & Time are not updated!";
                                    isValid = false;
                                }

                                if (!workOrderDetail.CompletionDateTime.HasValue)
                                {
                                    message += @"|Completion Date & Time are not updated!";
                                    isValid = false;
                                }

                                if (string.IsNullOrWhiteSpace(workOrderDetail.StateofEquipment))
                                {
                                    message += @"|State of Equipment is not updated!";
                                    isValid = false;
                                }
                                if (string.IsNullOrWhiteSpace(workOrderDetail.ServiceDelayReason))
                                {
                                    message += @"|Service Reason is not updated!";
                                    isValid = false;
                                }
                                if (string.IsNullOrWhiteSpace(workOrderDetail.TroubleshootSteps))
                                {
                                    message += @"|Troubleshoot steps not updated!";
                                    isValid = false;
                                }
                                if (string.IsNullOrWhiteSpace(workOrderDetail.ReviewedBy))
                                {
                                    message += @"|Reviewed by not updated!";
                                    isValid = false;
                                }
                                if (string.IsNullOrWhiteSpace(workOrderDetail.IsUnderWarrenty))
                                {
                                    message += @"|Under Warrenty not updated!";
                                    isValid = false;
                                }
                                if (!string.IsNullOrWhiteSpace(workOrderDetail.IsUnderWarrenty) && workOrderDetail.IsUnderWarrenty.ToLower() == "yes")
                                {
                                    if (string.IsNullOrWhiteSpace(workOrderDetail.WarrentyFor))
                                    {
                                        message += @"|Under WarrentyFor not updated!";
                                        isValid = false;
                                    }
                                }
                                if (string.IsNullOrWhiteSpace(workOrderDetail.AdditionalFollowupReq))
                                {
                                    message += @"|Additional Followup not updated!";
                                    isValid = false;
                                }
                                if (!string.IsNullOrWhiteSpace(workOrderDetail.AdditionalFollowupReq) && workOrderDetail.AdditionalFollowupReq.ToLower() == "yes")
                                {
                                    if (string.IsNullOrWhiteSpace(workOrderDetail.FollowupComments))
                                    {
                                        message += @"|Under Followup Comments not updated!";
                                        isValid = false;
                                    }
                                }
                                if (string.IsNullOrWhiteSpace(workOrderDetail.IsOperational))
                                {
                                    message += @"|Operational Field not updated!";
                                    isValid = false;
                                }
                                //if (string.IsNullOrWhiteSpace(workOrderDetail.OperationalComments))
                                //{
                                //    message += @"|Operational Comments not updated!";
                                //    isValid = false;
                                //}


                                if (string.IsNullOrWhiteSpace(workOrderDetail.ResponsibleTechName))
                                {
                                    message += @"|Responsible Tech Name is not updated!";
                                    isValid = false;
                                }
                                if (workorderManagement.WorkOrder.WorkorderCallstatus != "Closed")
                                {
                                    if (string.IsNullOrWhiteSpace(workOrderDetail.InvoiceNo))
                                    {
                                        message += @"|Invoice Number is not updated!";
                                        isValid = false;
                                    }
                                }
                                if (string.IsNullOrEmpty(workorderManagement.Closure.CustomerSignatureDetails))
                                {
                                    message += @"|Customer Signature Required!";
                                    isValid = false;
                                }
                                if (string.IsNullOrEmpty(workorderManagement.Closure.CustomerSignedBy))
                                {
                                    message += @"|SignatureBy Required!";
                                    isValid = false;
                                }

                                if (workOrder.WorkorderEquipments.Count <= 0)
                                {
                                    message += @"|Asset Details are not updated!";
                                    isValid = false;
                                }

                                int nCount = 1;
                                foreach (WorkorderEquipment equipment in workOrder.WorkorderEquipments)
                                {
                                    if (equipment.Solutionid.HasValue && equipment.Solutionid == 9999)
                                    {
                                        isNSRAsset = true;
                                        nCount++;
                                        continue;
                                    }
                                    if (!equipment.Solutionid.HasValue)
                                    {
                                        message += @"|Completion Code is not entered for equipment at row - " + nCount;
                                        isValid = false;
                                    }
                                    if (!equipment.CallTypeid.HasValue)
                                    {
                                        message += @"|Service Code is not entered for equipment at row - " + nCount;
                                        isValid = false;
                                    }

                                    if (string.IsNullOrWhiteSpace(equipment.Category))
                                    {
                                        message += @"|Equipment Type is not entered for equipment at row - " + nCount;
                                        isValid = false;
                                    }

                                    if (string.IsNullOrWhiteSpace(equipment.Manufacturer))
                                    {
                                        message += @"|Manufacturer is not entered for equipment at row - " + nCount;
                                        isValid = false;
                                    }

                                    if (string.IsNullOrWhiteSpace(equipment.Model))
                                    {
                                        message += @"|Model is not entered for equipment at row - " + nCount;
                                        isValid = false;
                                    }

                                    if (string.IsNullOrWhiteSpace(equipment.SerialNumber))
                                    {
                                        message += @"|SerialNumber is not entered for equipment at row - " + nCount;
                                        isValid = false;
                                    }

                                    if (/*workorderManagement.WorkOrder.WorkorderCalltypeid != 1800
                                        && workorderManagement.WorkOrder.WorkorderCalltypeid != 1810
                                        && workorderManagement.WorkOrder.WorkorderCalltypeid != 1820
                                        &&*/ equipment.CallTypeid != 1600)
                                    {

                                        //if (string.IsNullOrEmpty(equipment.WorkDescription))
                                        //{
                                        //    message += @"|Work performed is not entered for equipment at row - " + nCount;
                                        //    isValid = false;
                                        //}

                                        //if (equipment.NoPartsNeeded == false || equipment.NoPartsNeeded == null)
                                        //{
                                        //    IEnumerable<WorkorderPart> parts = workOrder.WorkorderParts.Where(a => a.AssetID == equipment.Assetid);

                                        //    if (parts == null || parts.Count() <= 0)
                                        //    {
                                        //        message += @"|Parts selection is required for equipment at row - " + nCount;
                                        //        isValid = false;
                                        //    }
                                        //}
                                    }
                                    nCount++;
                                }
                                                               
                                if (workorderManagement.WorkOrder.WorkorderCalltypeid == 1300 && (!string.IsNullOrWhiteSpace(workorderManagement.WorkOrder.WorkorderErfid)))
                                {
                                    Erf woErf = FarmerBrothersEntitites.Erfs.Where(er => er.ErfID == workorderManagement.WorkOrder.WorkorderErfid).FirstOrDefault();
                                    DateTime dt1 = currentTime.Date;
                                    DateTime dt2 = woErf.OriginalRequestedDate == null ? DateTime.Now.Date : Convert.ToDateTime(woErf.OriginalRequestedDate).Date;
                                   // if (woErf != null && woErf.OriginalRequestedDate != null && currentTime.CompareTo(woErf.OriginalRequestedDate) > 0)
                                   if(dt1 > dt2)
                                    {
                                        if (workorderManagement.ReasonCode == null || workorderManagement.ReasonCode <= 0)
                                        {
                                            message += @"| Enter the Reason Code ";
                                            isValid = false;
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(workorderManagement.Customer.BillingCode) && workorderManagement.Customer.BillingCode.ToLower() == "s08")
                                {
                                    WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).FirstOrDefault();

                                    if (!string.IsNullOrEmpty(wo.AuthTransactionId) && string.IsNullOrEmpty(wo.FinalTransactionId))
                                    {
                                        message = @"Please process the Credit Card from Email Link, before Closing the Event!";
                                        isValid = false;
                                    }
                                }

                                if (workorderManagement.IsServiceBillable == true)
                                {
                                    WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).FirstOrDefault();

                                    if (string.IsNullOrEmpty(wo.FinalTransactionId))
                                    {
                                        message += @"Please process the Payment, before Closing the Event!";
                                        isValid = false;
                                    }
                                }

                                if (isNSRAsset)
                                {
                                    isValid = true;
                                    message = string.Empty;
                                }

                                int effectedRecords = 0;
                                if (isValid == true)
                                {
                                    NotesHistory notesHistory1 = new NotesHistory()
                                    {
                                        AutomaticNotes = 1,
                                        EntryDate = currentTime,
                                        Notes = "Work Order Closed from MARS by " + UserName,
                                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                        UserName = UserName,
                                        isDispatchNotes = 1
                                    };
                                    notesHistory1.WorkorderID = workOrder.WorkorderID;
                                    workOrder.NotesHistories.Add(notesHistory1);

                                    if(workorderManagement.WorkOrder.WorkorderCalltypeid == 1300 && workorderManagement.ReasonCode > 0)
                                    {
                                        AllFBStatu afb = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workorderManagement.ReasonCode).FirstOrDefault();

                                        if (afb != null)
                                        {
                                            NotesHistory reasonNotes = new NotesHistory()
                                            {
                                                AutomaticNotes = 1,
                                                EntryDate = currentTime,
                                                Notes = "Install Event Schedule ReasonCode - " + afb.FBStatus,
                                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                                UserName = UserName,
                                                isDispatchNotes = 1
                                            };
                                            reasonNotes.WorkorderID = workOrder.WorkorderID;
                                            workOrder.NotesHistories.Add(reasonNotes);

                                            workOrder.RescheduleReasonCode = workorderManagement.ReasonCode;
                                        }
                                    }

                                    workOrder.WorkorderCallstatus = "Closed";
                                    workOrder.ClosedUserName = UserName;
                                    workOrder.WorkorderCloseDate = currentTime;
                                    workOrder.WorkorderModifiedDate = currentTime;
                                    workOrder.ModifiedUserName = UserName;
                                    
                                    //CreateInvoice(workorderManagement, workOrder);
                                    Invoice invoicedetails = FarmerBrothersEntitites.Invoices.Where(inv => inv.WorkorderID == workOrder.WorkorderID).FirstOrDefault();
                                    string invoiceId = string.Empty;
                                    if (invoicedetails != null)
                                    {
                                        FarmerBrothersEntitites.Invoices.Remove(invoicedetails);
                                    }


                                    effectedRecords = FarmerBrothersEntitites.SaveChanges();

                                    returnValue = CreatePartsOrder(workorderManagement, workOrder);

                                    string spawnMessage = string.Empty;
                                    CreateSpawnWorkOrder(workorderManagement, workOrder.WorkorderEquipments.ToList(), out spawnMessage);
                                    if (!string.IsNullOrWhiteSpace(spawnMessage))
                                    {
                                        message += @"|" + spawnMessage;
                                    }

                                    /*bool isAlreadySpawnWOCreatd = false;
                                    foreach (WorkorderEquipment equipment in workOrder.WorkorderEquipments)
                                    {
                                        //if (equipment.Solutionid == 5115
                                        //    || equipment.Solutionid == 5135
                                        //    || equipment.Solutionid == 5140
                                        //    || equipment.Solutionid == 5150
                                        //    || equipment.Solutionid == 5160
                                        //    || equipment.Solutionid == 5170
                                        //    || equipment.Solutionid == 5171
                                        //    || equipment.Solutionid == 5181
                                        //    || equipment.Solutionid == 5191)
                                        if(equipment.Solutionid == 5115
                                            || equipment.Solutionid == 5120
                                            || equipment.Solutionid == 5130
                                            || equipment.Solutionid == 5135
                                            || equipment.Solutionid == 5140
                                            || equipment.Solutionid == 5170
                                            || equipment.Solutionid == 5171
                                            || equipment.Solutionid == 5181
                                            || equipment.Solutionid == 5191)
                                        {
                                            //if (!isAlreadySpawnWOCreatd)
                                            {
                                                CreateSpawnWorkOrder(workorderManagement, equipment, out spawnMessage);
                                                if (!string.IsNullOrWhiteSpace(spawnMessage))
                                                {
                                                    message += @"|" + spawnMessage;
                                                }
                                                isAlreadySpawnWOCreatd = true;
                                            }

                                        }
                                    }*/

                                    returnValue = effectedRecords > 0 ? 1 : 0;
                                }
                            }
                            else
                            {
                                message = @"Mileage is not updated!";
                                message += @"|Arrival Date & Time are not updated!";
                                message += @"|Completion Date & Time are not updated!";
                                message += @"|Responsible Tech Name is not updated!";
                                returnValue = -1;
                            }
                        }
                    }
                    break;
                case WorkOrderManagementSubmitType.CREATEWORKORDER:
                    {
                        returnValue = WorkOrderSave(workorderManagement, workorderManagement.Operation, out workOrder, out message, isAutoGenWO);
                    }
                    break;
                case WorkOrderManagementSubmitType.CREATEFEASTMOVEMENT:
                    {
                        WorkOrderSave(workorderManagement, workorderManagement.Operation, out workOrder, out message, isAutoGenWO);
                        WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0 && ws.AssignedStatus == "Accepted").FirstOrDefault();
                    }
                    break;
            }

            if (returnValue == 0)
            {
                message = "No Updates to Save!";
            }

            if (returnValue > 0)
            {
                var redirectUrl = string.Empty;
                string WOConfirmationCode = string.Empty;
                if (workorderManagement.Operation == WorkOrderManagementSubmitType.COMPLETE)
                {
                    string WorkorderID = workOrder.WorkorderID.ToString();

                    if (WorkorderID.Length == 5) // For Test server events
                    {
                        WOConfirmationCode = WorkorderID.Substring(0, 1) + sGenPwd(2) + WorkorderID.Substring(WorkorderID.Length - 2);
                    }
                    else
                    {
                        WOConfirmationCode = WorkorderID.Substring(0, 3) + sGenPwd(2) + WorkorderID.Substring(WorkorderID.Length - 6);
                    }


                    WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrder.WorkorderID).FirstOrDefault();
                    wo.WorkorderClosureConfirmationNo = WOConfirmationCode;

                    NotesHistory notes = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = "Closure Conf No: " + WOConfirmationCode,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 1
                    };
                    notes.WorkorderID = workOrder.WorkorderID;
                    workOrder.NotesHistories.Add(notes);


                    FarmerBrothersEntitites.SaveChanges();

                }
                //681 == Customer Cancelled Service
                if (workOrder.FollowupCallID == 681)
                {
                    string emailAddress = string.Empty;
                    WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrder.WorkorderID && (w.AssignedStatus == "Sent" || w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();
                    if (ws != null && ws.AssignedStatus != null)
                    {
                        ws.AssignedStatus = "Cancelled";
                        FarmerBrothersEntitites.SaveChanges();

                        TECH_HIERARCHY techView = GetTechById(ws.Techid);
                        if (ws.Techid == Convert.ToInt32(ConfigurationManager.AppSettings["MAITestDispatch"]))
                        {
                            emailAddress = ConfigurationManager.AppSettings["CrystalEmailId"];
                        }
                        else if (ws.Techid == Convert.ToInt32(ConfigurationManager.AppSettings["MikeTestTechId"]))
                        {
                            emailAddress = ConfigurationManager.AppSettings["MikeEmailId"];
                        }
                        else
                        {
                            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                            {
                                emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                            }
                            else
                            {
                                if (techView != null)
                                {
                                    emailAddress = techView.EmailCC;
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(emailAddress))
                        {
                            StringBuilder subject = new StringBuilder();
                            subject.Append("CANCELLATION Email: - WO: ");
                            subject.Append(workOrder.WorkorderID);
                            subject.Append(" Customer: ");
                            subject.Append(workOrder.CustomerName);
                            subject.Append(" ST: ");
                            subject.Append(workOrder.CustomerState);
                            subject.Append(" Call Type: ");
                            subject.Append(workOrder.WorkorderCalltypeDesc);

                            SendWorkOrderMail(workOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, "the service was cancelled", string.Empty);
                        }
                    }

                }
                if (Request != null || FromERF)
                {
                    if (!FromERF)
                    {
                        if (workorderManagement.WorkOrder.WorkorderID == 0 || workOrder.WorkorderCallstatus == "Closed" || workOrder.WorkorderCallstatus == "Completed")
                        {
                            redirectUrl = new UrlHelper(Request.RequestContext).Action("CustomerSearch", "CustomerSearch", new { isBack = 0 });
                        }
                        else
                        {
                            redirectUrl = new UrlHelper(Request.RequestContext).Action("WorkorderSearch", "Workorder", new { isBack = 0 });
                        }
                    }

                    if(workorderManagement.IsNewPartsOrder == true)
                    {
                        //Commented this, as the Parts email is sent from AutoDispatch process
                        //SendPartsOrderMail(workOrder.WorkorderID);
                    }

                    if (workorderManagement.WorkOrder.WorkorderID == 0 || FromERF)
                    {
                        //Start Auto Dispatch Process
                        if (FromERF)
                        {
                            string usrName = System.Web.HttpContext.Current.Session["UserName"] == null ? "" : System.Web.HttpContext.Current.Session["UserName"].ToString();
                            //StartAutoDispatchProcess(workOrder, usrName);
                        }
                        else
                        {
                            //StartAutoDispatchProcess(workOrder);
                        }
                    }

                }

                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = redirectUrl, WorkOrderId = workOrder.WorkorderID, returnValue = returnValue, WorkorderCallstatus = workOrder.WorkorderCallstatus, message = message, WOConfirmationCode = WOConfirmationCode };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            else
            {
                string callStatus = workOrder == null ? "" : workOrder.WorkorderCallstatus;
                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = "", WorkOrderId = 0, returnValue = returnValue, WorkorderCallstatus = callStatus, message = message };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
        }

        public int WorkOrderSave(WorkorderManagementModel workorderManagement, WorkOrderManagementSubmitType operation, out WorkOrder workOrder, out string message, bool isAutoGenWO = false)
        {
            int returnValue = 0;
            message = string.Empty;
            workOrder = null;
            //FarmerBrothersEntitites = new FarmerBrothersEntities();
            if (operation == WorkOrderManagementSubmitType.CREATEWORKORDER)
            {
                DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);

                var CustomerId = int.Parse(workorderManagement.Customer.CustomerId);
                Customer serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();

                serviceCustomer.FilterReplaced = workorderManagement.Closure.FilterReplaced;
                serviceCustomer.FilterReplacedDate = currentTime;
                serviceCustomer.NextFilterReplacementDate = currentTime.AddMonths(6);

                workOrder = workorderManagement.FillCustomerData(new WorkOrder(), true, FarmerBrothersEntitites, serviceCustomer);
                workOrder.EntryUserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : "";

                IndexCounter counter = Utility.GetIndexCounter("WorkorderID", 1);
                counter.IndexValue++;
                //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                workOrder.WorkorderID = counter.IndexValue.Value;
                workOrder.WorkorderCalltypeid = workorderManagement.WorkOrder.WorkorderCalltypeid;
                workOrder.WorkorderCalltypeDesc = workorderManagement.WorkOrder.WorkorderCalltypeDesc;
                workOrder.WorkorderErfid = workorderManagement.WorkOrder.WorkorderErfid;
                workOrder.WorkorderEquipCount = Convert.ToInt16(workorderManagement.WorkOrderEquipmentsRequested.Count());
                workOrder.PriorityCode = workorderManagement.PriorityList[0].FBStatusID;

                workOrder.FollowupCallID = defaultFollowUpCall;

                TimeZoneInfo newTimeZoneInfo = null;
                Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                DateTime CurrentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                workOrder.WorkorderEntryDate = CurrentTime;
                workOrder.WorkorderModifiedDate = workOrder.WorkorderEntryDate;
                workOrder.ModifiedUserName = UserName;


                if (isAutoGenWO)
                {
                    workOrder.IsAutoGenerated = true;
                    workOrder.EntryUserName = "WEB";
                }

                workOrder.WorkorderModifiedDate = workOrder.WorkorderEntryDate;
                workOrder.WorkorderCallstatus = "Open";

                {
                    
                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = isAutoGenWO ? @"Work Order created from MARS WO#: " + workOrder.WorkorderID + @" in “MARS”!" : @"Work Order created from ERF WO#: " + workOrder.WorkorderID + @" in “MARS”!",
                        Userid = isAutoGenWO ? 99999 : System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = isAutoGenWO ? "WEB" : UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                        isDispatchNotes = 0
                    };
                    notesHistory.WorkorderID = workOrder.WorkorderID;
                    workOrder.NotesHistories.Add(notesHistory);


                    //foreach (NewNotesModel newNotesModel in workorderManagement.NewNotes)
                    {
                        NotesHistory newnotesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "Workorder Created for Non-Approved ERF",
                            Userid = isAutoGenWO ? 99999 : System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = isAutoGenWO ? "WEB" : UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                            WorkorderID = workOrder.WorkorderID,
                            isDispatchNotes = 0
                        };
                        FarmerBrothersEntitites.NotesHistories.Add(newnotesHistory);
                    }
                    if (workorderManagement.Notes.TechID != null && workorderManagement.Notes.TechID != "-1")
                    {
                        workOrder.SpecificTechnician = workorderManagement.Notes.TechID;
                    }

                    workOrder.IsSpecificTechnician = workorderManagement.Notes.IsSpecificTechnician;
                    workOrder.IsAutoDispatched = workorderManagement.Notes.IsAutoDispatched;

                    foreach (WorkOrderBrand brand in workorderManagement.WorkOrder.WorkOrderBrands)
                    {
                        WorkOrderBrand newBrand = new WorkOrderBrand();
                        foreach (var property in brand.GetType().GetProperties())
                        {
                            if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                            {
                                property.SetValue(newBrand, property.GetValue(brand));
                            }
                        }
                        newBrand.WorkorderID = workOrder.WorkorderID;
                        workOrder.WorkOrderBrands.Add(newBrand);
                    }

                    IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
                    assetCounter.IndexValue++;
                    //FarmerBrothersEntitites.Entry(assetCounter).State = System.Data.Entity.EntityState.Modified;

                    WorkorderEquipment equipment = new WorkorderEquipment()
                    {
                        Assetid = assetCounter.IndexValue.Value,
                        CallTypeid = isAutoGenWO ? workorderManagement.WorkOrder.WorkorderCalltypeid : 1300,
                        Category = isAutoGenWO ? "" : "OTHER",
                        Symptomid = 2001,
                        Location = isAutoGenWO ? workorderManagement.WorkOrder.ClosedUserName : "OTH"
                    };
                    workOrder.WorkorderEquipments.Add(equipment);

                    WorkorderEquipmentRequested equipmentReq = new WorkorderEquipmentRequested()
                    {
                        Assetid = assetCounter.IndexValue.Value,
                        CallTypeid = isAutoGenWO ? workorderManagement.WorkOrder.WorkorderCalltypeid : 1300,
                        Category = isAutoGenWO ? "" : "OTHER",
                        Symptomid = 2001,
                        Location = isAutoGenWO ? workorderManagement.WorkOrder.ClosedUserName : "OTH"
                    };
                    workOrder.WorkorderEquipmentRequesteds.Add(equipmentReq);
                    if (isAutoGenWO)
                    {
                        workorderManagement.WorkOrder.ClosedUserName = string.Empty;
                    }
                    notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = workOrder.WorkorderEntryDate,
                        Notes = isAutoGenWO ? workOrder.WorkorderCalltypeDesc + " Work Order # " + workOrder.WorkorderID + " in MARS!" : @"Install Work Order # " + workOrder.WorkorderID + " in MARS!",
                        Userid = isAutoGenWO ? 99999 : System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = isAutoGenWO ? "WEB" : UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                        isDispatchNotes = 0
                    };
                    workOrder.NotesHistories.Add(notesHistory);
                    if (workorderManagement.Erf != null)
                    {
                        workOrder.WorkorderErfid = workorderManagement.Erf.ErfID;
                    }

                }


                if (workorderManagement.RemovalCount > 5)
                {
                    workOrder.WorkorderCallstatus = "Open";
                }

                FarmerBrothersEntitites.WorkOrders.Add(workOrder);
                SaveRemovalDetails(workorderManagement, workOrder);


                WorkorderDetail workOrderDetail = new WorkorderDetail()
                {
                    StartDateTime = workorderManagement.Closure.StartDateTime,
                    InvoiceNo = workorderManagement.Closure.InvoiceNo,
                    ArrivalDateTime = workorderManagement.Closure.ArrivalDateTime,
                    CompletionDateTime = workorderManagement.Closure.CompletionDateTime,
                    ResponsibleTechName = workorderManagement.Closure.ResponsibleTechName,
                    Mileage = workorderManagement.Closure.Mileage,
                    CustomerName = workorderManagement.Closure.CustomerName,
                    CustomerEmail = workorderManagement.Closure.CustomerEmail,
                    CustomerSignatureDetails = workorderManagement.Closure.CustomerSignatureDetails,
                    CustomerSignatureBy = workorderManagement.Closure.CustomerSignedBy,
                    TechnicianSignatureDetails = workorderManagement.Closure.TechnicianSignatureDetails,
                    WorkorderID = workOrder.WorkorderID,
                    EntryDate = workOrder.WorkorderEntryDate,
                    ModifiedDate = workOrder.WorkorderEntryDate,
                    SpecialClosure = null,
                    TravelTime = workorderManagement.Closure.TravelHours + ":" + workorderManagement.Closure.TravelMinutes,
                    WaterTested = workorderManagement.Closure.WaterTested,
                    HardnessRating = workorderManagement.Closure.HardnessRating
                };

                if (workorderManagement.Closure.PhoneSolveid > 0)
                {
                    workOrderDetail.PhoneSolveid = workorderManagement.Closure.PhoneSolveid;
                }
                if (workOrderDetail.CustomerSignatureDetails != null)
                {
                    //890 is for empty signature box
                    if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                    {
                        workOrderDetail.CustomerSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                            Select(s => s.CustomerSignatureDetails).FirstOrDefault();
                        if (workOrderDetail.CustomerSignatureDetails != null)
                        {
                            if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                            {
                                workOrderDetail.CustomerSignatureDetails = string.Empty;
                            }
                        }
                        else
                        {
                            workOrderDetail.CustomerSignatureDetails = string.Empty;
                        }
                    }
                }

                if (workOrderDetail.TechnicianSignatureDetails != null)
                {
                    //890 is for empty signature box
                    if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                    {
                        workOrderDetail.TechnicianSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                            Select(s => s.TechnicianSignatureDetails).FirstOrDefault();
                        if (workOrderDetail.TechnicianSignatureDetails != null)
                        {
                            if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                            {
                                workOrderDetail.TechnicianSignatureDetails = string.Empty;
                            }
                        }
                        else
                        {
                            workOrderDetail.TechnicianSignatureDetails = string.Empty;
                        }
                    }
                }

                FarmerBrothersEntitites.WorkorderDetails.Add(workOrderDetail);

                workOrder.WorkOrderOpenedDateTime = DateTime.Now;
                workOrder.CurrentUserName = UserName;

                int effectedRecords = FarmerBrothersEntitites.SaveChanges();
                returnValue = effectedRecords > 0 ? 1 : 0;

                if (returnValue == 1 && workorderManagement.RemovalCount > 5)
                {
                    string projectManagementEmail = ConfigurationManager.AppSettings["ProjectManagementEmail"];
                    StringBuilder subject = new StringBuilder();
                    subject.Append("Removal Count is more than 5!   WO: ");
                    subject.Append(workOrder.WorkorderID);
                    subject.Append(" ST: ");
                    subject.Append(workOrder.CustomerState);
                    subject.Append(" Call Type: ");
                    subject.Append(workOrder.WorkorderCalltypeDesc);

                    SendWorkOrderMail(workOrder, subject.ToString(), projectManagementEmail, ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"], null, MailType.INFO, false, "");
                }
            }
            else
            {
                Erf erf = null;
                bool isValidErf = false;
                if (!string.IsNullOrWhiteSpace(workorderManagement.WorkOrder.WorkorderErfid))
                {
                    erf = FarmerBrothersEntitites.Erfs.Where(e => e.ErfID == workorderManagement.WorkOrder.WorkorderErfid).FirstOrDefault();
                    if (erf != null)
                    {
                        isValidErf = true;
                    }
                }
                else
                {
                    isValidErf = true;
                }

                if (isValidErf == true)
                {
                    TimeZoneInfo newTimeZoneInfo = null;
                    Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                    DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                    int? currentFollowupId = null;

                    Customer serviceCustomer = null;
                    if (!string.IsNullOrEmpty(workorderManagement.Customer.CustomerId))
                    {
                        var CustomerId = int.Parse(workorderManagement.Customer.CustomerId);
                        serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                    }

                    if (serviceCustomer != null)
                    {
                        serviceCustomer.FilterReplaced = workorderManagement.Closure.FilterReplaced;
                        serviceCustomer.FilterReplacedDate = currentTime;
                        serviceCustomer.NextFilterReplacementDate = currentTime.AddMonths(6);
                    }

                    if (workorderManagement.WorkOrder.WorkorderID > 0)
                    {
                        workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);
                        currentFollowupId = workOrder.FollowupCallID;
                        if (!string.IsNullOrEmpty(workorderManagement.Customer.ServiceTier))
                        {
                            workOrder.ServiceTier = workorderManagement.Customer.ServiceTier;
                        }
                        currentFollowupId = workOrder.FollowupCallID;

                        if (string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Closed", true) != 0)
                        {
                            workOrder = workorderManagement.FillCustomerData(workOrder, false, FarmerBrothersEntitites);
                            if (erf != null)
                            {
                                int workorderId = workOrder.WorkorderID;
                                ErfWorkorderLog erfLog = FarmerBrothersEntitites.ErfWorkorderLogs.Where(e => e.WorkorderID == workorderId).FirstOrDefault();
                                if (erfLog != null)
                                {
                                    erfLog.ErfID = erf.ErfID;
                                    FarmerBrothersEntitites.Entry(erfLog).State = System.Data.Entity.EntityState.Modified;
                                }
                                else
                                {
                                    ErfWorkorderLog newLog = new ErfWorkorderLog() { ErfID = erf.ErfID, WorkorderID = workOrder.WorkorderID };
                                    FarmerBrothersEntitites.ErfWorkorderLogs.Add(newLog);
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(workorderManagement.Notes.FollowUpRequestID))
                            {
                                workOrder.FollowupCallID = new Nullable<int>(Convert.ToInt32(workorderManagement.Notes.FollowUpRequestID));

                                //681 ==Customer Cancelled Service
                                if (workOrder.FollowupCallID == 681)
                                {
                                    workOrder.WorkorderCallstatus = "Open";
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(workorderManagement.Notes.TechID))
                            {
                                if (workorderManagement.Notes.TechID != "-1")
                                {
                                    workOrder.SpecificTechnician = workorderManagement.Notes.TechID;
                                }
                            }

                            workOrder.WorkorderEquipCount = Convert.ToInt16(workorderManagement.WorkOrderEquipmentsRequested.Count());


                            if (workorderManagement.IsNewPartsOrder == false && workorderManagement.WorkOrderEquipmentsRequested != null)
                            {
                                WorkOrderManagementEquipmentModel equipment = workorderManagement.WorkOrderEquipmentsRequested.Where(e => e.CallTypeID == 1200).FirstOrDefault();
                                if (equipment != null)
                                {
                                    workOrder.WorkorderCalltypeid = equipment.CallTypeID;
                                    WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeID).FirstOrDefault();
                                    if (workOrderType != null)
                                    {
                                        workOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                        workOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                    }
                                }                               
                                else
                                {
                                    WorkOrderManagementEquipmentModel equipmentExist = workorderManagement.WorkOrderEquipmentsRequested.Where(e => e.CallTypeID == workorderManagement.WorkOrder.WorkorderCalltypeid).FirstOrDefault();
                                    if (equipmentExist != null)
                                    {
                                        workOrder.WorkorderCalltypeid = equipmentExist.CallTypeID;
                                        WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipmentExist.CallTypeID).FirstOrDefault();
                                        if (workOrderType != null)
                                        {
                                            workOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                            workOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                        }
                                    }
                                    else
                                    {
                                        equipment = workorderManagement.WorkOrderEquipmentsRequested.ElementAt(0);
                                        if (equipment != null)
                                        {
                                            workOrder.WorkorderCalltypeid = equipment.CallTypeID;
                                            WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeID).FirstOrDefault();
                                            if (workOrderType != null)
                                            {
                                                workOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                                workOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                            }
                                        }
                                    }
                                }
                            }


                            if (workorderManagement.Notes.IsAutoDispatched == true)
                            {
                                AgentDispatchLog autodispatchLog = new AgentDispatchLog()
                                {
                                    TDate = currentTime,
                                    UserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                    UserName = UserName,
                                    WorkorderID = workOrder.WorkorderID
                                };
                                FarmerBrothersEntitites.AgentDispatchLogs.Add(autodispatchLog);
                            }
                        }

                        FarmerBrothersEntities fbentity = new FarmerBrothersEntities();
                        WorkOrder estimateWO = fbentity.WorkOrders.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).FirstOrDefault();

                        if (estimateWO.Estimate != workorderManagement.Estimate ||
                            estimateWO.FinalEstimate != workorderManagement.FinalEstimate ||
                            estimateWO.ThirdPartyPO != workorderManagement.ThirdPartyPO)
                        {
                            string estimateNotes = string.Empty;
                            estimateNotes = "Following Estimate Data added to Work Order :"
                               + Environment.NewLine + "Estimate:  " + workorderManagement.Estimate;
                            if (!string.IsNullOrEmpty(workorderManagement.FinalEstimate.ToString()))
                            {
                                estimateNotes += "; Final Estimate: " + workorderManagement.FinalEstimate;
                            }

                            //string estaprby = FarmerBrothersEntitites.ESMDSMRSMs.Where(e => e.EDSMID == workorderManagement.WorkOrder.EstimateApprovedBy).Select(es => es.ESMName).FirstOrDefault();
                            //if (string.IsNullOrEmpty(estaprby))
                            //{
                            //    estaprby = FarmerBrothersEntitites.ESMDSMRSMs.Where(e => e.CCMID == workorderManagement.WorkOrder.EstimateApprovedBy).Select(es => es.CCMName).FirstOrDefault();
                            //}

                            string estaprby = FarmerBrothersEntitites.EstimateEscalations.Where(e => e.Code == workorderManagement.WorkOrder.EstimateApprovedBy).Select(es => es.Name).FirstOrDefault();
                            
                            estimateNotes += "; Third Party PO: " + workorderManagement.ThirdPartyPO + "; Estimate Approved By: " + estaprby;

                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = estimateNotes,
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = UserName,
                                WorkorderID = workOrder.WorkorderID,
                                isDispatchNotes = 1
                            };
                            FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                        }

                        workOrder.IsSpecificTechnician = workorderManagement.Notes.IsSpecificTechnician;
                        workOrder.IsAutoDispatched = workorderManagement.Notes.IsAutoDispatched;
                        workOrder.Estimate = workorderManagement.Estimate;
                        workOrder.FinalEstimate = workorderManagement.FinalEstimate;
                        workOrder.IsEstimateApproved = workorderManagement.IsEstimateApproved;
                        workOrder.ThirdPartyPO = workorderManagement.ThirdPartyPO;
                        workOrder.EstimateApprovedBy = workorderManagement.WorkOrder.EstimateApprovedBy;


                    }
                    else
                    {                        
                        //Customer serviceCustomer = null;
                        //if (!string.IsNullOrEmpty(workorderManagement.Customer.CustomerId))
                        //{
                        //    var CustomerId = int.Parse(workorderManagement.Customer.CustomerId);
                        //    serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                        //}

                        workOrder = workorderManagement.FillCustomerData(new WorkOrder(), true, FarmerBrothersEntitites, serviceCustomer);
                        workOrder.EntryUserName = UserName;

                        if (!string.IsNullOrEmpty(workorderManagement.Customer.ServiceTier))
                        {
                            workOrder.ServiceTier = workorderManagement.Customer.ServiceTier;
                        }

                        IndexCounter counter = Utility.GetIndexCounter("WorkorderID", 1);
                        counter.IndexValue++;
                        //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                        workOrder.WorkorderID = counter.IndexValue.Value;
                        workOrder.WorkorderEntryDate = currentTime;
                        workOrder.WorkorderModifiedDate = workOrder.WorkorderEntryDate;
                        workOrder.ModifiedUserName = UserName;
                        workOrder.WorkorderCallstatus = "Open";
                        workOrder.WorkorderEquipCount = Convert.ToInt16(workorderManagement.WorkOrderEquipmentsRequested.Count());
                        if (string.IsNullOrWhiteSpace(workorderManagement.Notes.FollowUpRequestID))
                        {
                            workOrder.FollowupCallID = 603;
                        }
                        else
                        {
                            workOrder.FollowupCallID = new Nullable<int>(Convert.ToInt32(workorderManagement.Notes.FollowUpRequestID));
                        }

                        workOrder.AuthTransactionId = workorderManagement.PaymentTransactionId;
                        NotesHistory authTransactionHistory = new NotesHistory()
                        {
                            AutomaticNotes = 1,
                            EntryDate = currentTime,
                            Notes = @"Auth Payment TransactionId : " + workorderManagement.PaymentTransactionId,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                            isDispatchNotes = 0
                        };

                        workOrder.NotesHistories.Add(authTransactionHistory);



                        NotesHistory notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 1,
                            EntryDate = currentTime,
                            Notes = @"Work Order Created from “MARS”!",
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                            isDispatchNotes = 0
                        };

                        workOrder.NotesHistories.Add(notesHistory);

                        if (workorderManagement.WorkOrderEquipmentsRequested != null)
                        {
                            WorkOrderManagementEquipmentModel equipment = workorderManagement.WorkOrderEquipmentsRequested.Where(e => e.CallTypeID == 1200).FirstOrDefault();
                            if (equipment != null)
                            {
                                workOrder.WorkorderCalltypeid = equipment.CallTypeID;
                                WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeID).FirstOrDefault();
                                if (workOrderType != null)
                                {
                                    workOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                    workOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                }
                            }
                            else if (workorderManagement.IsNewPartsOrder == true)
                            {
                                //workOrder.WorkorderCalltypeid = 1820;
                                workOrder.WorkorderCalltypeDesc = "Parts Request";
                            }
                            else
                            {
                                equipment = workorderManagement.WorkOrderEquipmentsRequested.ElementAt(0);
                                if (equipment != null)
                                {
                                    workOrder.WorkorderCalltypeid = equipment.CallTypeID;
                                    WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeID).FirstOrDefault();
                                    if (workOrderType != null)
                                    {
                                        workOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                        workOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                    }
                                }
                            }
                        }
                        FarmerBrothersEntitites.WorkOrders.Add(workOrder);
                    }

                    if (!string.IsNullOrWhiteSpace(workorderManagement.SelectedBrandIds)
                        && string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Closed", true) != 0)
                    {
                        string selectedBrandIds = workorderManagement.SelectedBrandIds.TrimEnd(',');

                        string[] brandIds = selectedBrandIds.Split(',');
                        int[] numaricBrandIds = brandIds.Select(int.Parse).ToArray();

                        IList<WorkOrderBrand> workOrderBrands = workOrder.WorkOrderBrands.Where(wb => !numaricBrandIds.Contains(wb.BrandID.Value)).ToList();

                        foreach (WorkOrderBrand workOrderBrand in workOrderBrands)
                        {
                            if (workOrderBrand.BrandID.HasValue)
                            {
                                FarmerBrothersEntitites.WorkOrderBrands.Remove(workOrderBrand);
                            }
                        }

                        foreach (int brandId in numaricBrandIds)
                        {
                            WorkOrderBrand workOrderBrand = workOrder.WorkOrderBrands.Where(wb => wb.BrandID.Value == brandId).FirstOrDefault();
                            if (workOrderBrand == null)
                            {
                                workOrder.WorkOrderBrands.Add(new WorkOrderBrand() { BrandID = brandId, WorkorderID = workOrder.WorkorderID });
                            }
                        }
                    }


                    if (string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Closed", true) != 0)
                    {
                        SaveClosureDetails(workorderManagement, workOrder);
                        SaveWorkOrderParts(workorderManagement, workOrder);

                        workOrder.IsBillable = workorderManagement.IsBillableFeed;

                        //if (workorderManagement.WorkOrder.WorkorderCalltypeid != 1820)//1800)
                        if(workorderManagement.IsNewPartsOrder != true)
                        {
                            SaveNonSerialized(workorderManagement, workOrder);
                        }

                        if (string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Completed", true) != 0
                            || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Closed", true) != 0
                            || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Invoiced", true) != 0)
                        {
                            SaveWorkOrderEquipments(workorderManagement, workOrder);
                        }

                        if ((string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Hold", true) == 0
                            || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Accepted", true) == 0
                            || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Accepted-Partial", true) == 0
                            || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "On Site", true) == 0
                            || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "In Progress", true) == 0
                            || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Completed", true) == 0
                            || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Closed", true) == 0
                            || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Invoiced", true) == 0)
                            && string.Compare(workorderManagement.Closure.SpecialClosure, "No Service Required", true) != 0)
                        {
                            SaveClosureAssets(workorderManagement, workOrder);
                        }
                    }

                    if (!string.IsNullOrEmpty(workorderManagement.ShippingPriority))
                    {
                        workOrder.ShippingPriority = workorderManagement.ShippingPriority;
                    }

                    SaveNotes(workorderManagement, workOrder);
                    if (workorderManagement.Notes.TechID != null && workorderManagement.Notes.TechID != "-1")
                    {
                        workOrder.SpecificTechnician = workorderManagement.Notes.TechID;
                    }

                    workOrder.IsSpecificTechnician = workorderManagement.Notes.IsSpecificTechnician;
                    workOrder.IsAutoDispatched = workorderManagement.Notes.IsAutoDispatched;

                    if (workorderManagement.Notes.IsAutoDispatched == true)
                    {
                        AgentDispatchLog autodispatchLog = new AgentDispatchLog()
                        {
                            TDate = currentTime,
                            UserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            WorkorderID = workOrder.WorkorderID
                        };
                        FarmerBrothersEntitites.AgentDispatchLogs.Add(autodispatchLog);
                    }

                    if (string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Closed", true) != 0)
                    {
                        SaveWorkOrderSchedules(workorderManagement, workOrder);
                        if (!workOrder.CustomerID.HasValue || workOrder.CustomerID.Value <= 0)
                        {
                            CustomerModel custmodel = new CustomerModel();

                            custmodel.CreateUnknownCustomer(workorderManagement.Customer, FarmerBrothersEntitites);
                            workOrder.WorkorderCallstatus = "Hold for AB";
                            workOrder.CustomerID = Convert.ToInt32(workorderManagement.Customer.CustomerId);

                        }

                        if (currentFollowupId != workOrder.FollowupCallID && workOrder.FollowupCallID != defaultFollowUpCall)
                        {
                            int? followupId = workOrder.FollowupCallID;
                            AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(s => s.FBStatusID == followupId).FirstOrDefault();
                            if (status != null && !string.IsNullOrEmpty(status.FBStatus))
                            {
                                NotesHistory notesHistory = new NotesHistory()
                                {
                                    AutomaticNotes = 1,
                                    EntryDate = currentTime,
                                    Notes = "Follow Up Requested: " + status.FBStatus,
                                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                    UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                                    isDispatchNotes = 1
                                };
                                workOrder.NotesHistories.Add(notesHistory);
                            }
                        }
                    }

                    string phoneSolveMessage = string.Empty;
                    if (string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Hold", true) == 0
                         || string.Compare(workorderManagement.WorkOrder.WorkorderCallstatus, "Attempting", true) == 0)
                    {
                        SavePhoneSolveDetails(workorderManagement, workOrder, out phoneSolveMessage);
                        message += "|" + phoneSolveMessage;
                    }
                                        
                    double BillingTotal = SaveBillingDetails(workorderManagement.BillingDetails.ToList(), workOrder.WorkorderID);

                    double totalPrice = SaveBillableList(workorderManagement);
                    workOrder.TotalUnitPrice = totalPrice.ToString();
                    workOrder.IsBillable = Convert.ToBoolean(workorderManagement.IsBillableFeed);
                    SaveRemovalDetails(workorderManagement, workOrder);
                    workOrder.WorkorderModifiedDate = currentTime;
                    workOrder.ModifiedUserName = UserName;
                    workOrder.TechTeamLead = workorderManagement.Customer.PreferredProvider;
                    workOrder.ResponsibleTechPhone = workorderManagement.Customer.ProviderPhone;

                    if (workorderManagement.RemovalCount > 5 && workorderManagement.WorkOrder.WorkorderID <= 0)
                    {
                        WorkorderStatusLog statusLog = new WorkorderStatusLog() { StatusFrom = workOrder.WorkorderCallstatus, StatusTo = "Open", StausChangeDate = currentTime, WorkorderID = workOrder.WorkorderID };
                        workOrder.WorkorderStatusLogs.Add(statusLog);
                        workOrder.WorkorderCallstatus = "Open";
                    }

                    if (string.IsNullOrWhiteSpace(phoneSolveMessage))
                    {
                        int effectedRecords = FarmerBrothersEntitites.SaveChanges();
                        returnValue = effectedRecords > 0 ? 1 : 0;

                        if (returnValue == 1 && workorderManagement.RemovalCount > 5)
                        {
                            string projectManagementEmail = ConfigurationManager.AppSettings["ProjectManagementEmail"];
                            StringBuilder subject = new StringBuilder();
                            subject.Append("Removal Count is more than 5! WO: ");
                            subject.Append(workOrder.WorkorderID);
                            subject.Append(" ST: ");
                            subject.Append(workOrder.CustomerState);
                            subject.Append(" Call Type: ");
                            subject.Append(workOrder.WorkorderCalltypeDesc);


                            SendWorkOrderMail(workOrder, subject.ToString(), projectManagementEmail, ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"], null, MailType.INFO, false, "");
                        }

                        if (workOrder.FollowupCallID !=null && workOrder.FollowupCallID != 681 && currentFollowupId != workOrder.FollowupCallID && workOrder.FollowupCallID != defaultFollowUpCall)
                        {

                            returnValue = SendFollowupMail(workorderManagement, workOrder) == true ? 1 : -1;
                            if (returnValue == -1)
                            {
                                message = "|There is a problem in sending Followup email!";
                            }
                        }
                    }
                    else
                    {
                        returnValue = -1;
                    }
                }
                else
                {
                    returnValue = -2;
                }
            }


            return returnValue;
        }

        public JsonResult ERFWorkOrderSave(WorkorderManagementModel workorderManagement, FarmerBrothersEntities  WOFBEntity, out WorkOrder workOrder, out string message, bool isAutoGenWO = false)
        {
            int returnValue = 0;
            message = string.Empty;
            workOrder = null;
            JsonResult jsonResult = new JsonResult();

            try
            {
                DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, WOFBEntity);

                var CustomerId = int.Parse(workorderManagement.Customer.CustomerId);
                Customer serviceCustomer = WOFBEntity.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();

                serviceCustomer.FilterReplaced = workorderManagement.Closure.FilterReplaced;
                serviceCustomer.FilterReplacedDate = currentTime;
                serviceCustomer.NextFilterReplacementDate = currentTime.AddMonths(6);

                workOrder = workorderManagement.FillCustomerData(new WorkOrder(), true, WOFBEntity, serviceCustomer);
                workOrder.EntryUserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : "";

                IndexCounter counter = Utility.GetIndexCounter("WorkorderID", 1);
                counter.IndexValue++;
                //WOFBEntity.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                workOrder.WorkorderID = counter.IndexValue.Value;
                workOrder.WorkorderCalltypeid = workorderManagement.WorkOrder.WorkorderCalltypeid;
                workOrder.WorkorderCalltypeDesc = workorderManagement.WorkOrder.WorkorderCalltypeDesc;
                workOrder.WorkorderErfid = workorderManagement.WorkOrder.WorkorderErfid;
                workOrder.WorkorderEquipCount = Convert.ToInt16(workorderManagement.WorkOrderEquipmentsRequested.Count());
                workOrder.PriorityCode = workorderManagement.PriorityList[0].FBStatusID;

                workOrder.FollowupCallID = defaultFollowUpCall;

                TimeZoneInfo newTimeZoneInfo = null;
                Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, WOFBEntity);
                DateTime CurrentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, WOFBEntity);
                workOrder.WorkorderEntryDate = CurrentTime;
                workOrder.WorkorderModifiedDate = workOrder.WorkorderEntryDate;
                workOrder.ModifiedUserName = UserName;


                if (isAutoGenWO)
                {
                    workOrder.IsAutoGenerated = true;
                    workOrder.EntryUserName = "WEB";
                }

                workOrder.WorkorderModifiedDate = workOrder.WorkorderEntryDate;
                workOrder.WorkorderCallstatus = "Open";

                {

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = isAutoGenWO ? @"Work Order created from MARS WO#: " + workOrder.WorkorderID + @" in “MARS”!" : @"Work Order created from ERF WO#: " + workOrder.WorkorderID + @" in “MARS”!",
                        Userid = isAutoGenWO ? 99999 : System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = isAutoGenWO ? "WEB" : UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                        isDispatchNotes = 0
                    };
                    notesHistory.WorkorderID = workOrder.WorkorderID;
                    workOrder.NotesHistories.Add(notesHistory);


                    //foreach (NewNotesModel newNotesModel in workorderManagement.NewNotes)
                    {
                        NotesHistory newnotesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "Workorder Created for ERF#  " + workorderManagement.WorkOrder.WorkorderErfid,
                            Userid = isAutoGenWO ? 99999 : System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = isAutoGenWO ? "WEB" : UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                            WorkorderID = workOrder.WorkorderID,
                            isDispatchNotes = 0
                        };
                        WOFBEntity.NotesHistories.Add(newnotesHistory);
                    }
                    if (workorderManagement.Notes.TechID != null && workorderManagement.Notes.TechID != "-1")
                    {
                        workOrder.SpecificTechnician = workorderManagement.Notes.TechID;
                    }

                    workOrder.IsSpecificTechnician = workorderManagement.Notes.IsSpecificTechnician;
                    workOrder.IsAutoDispatched = workorderManagement.Notes.IsAutoDispatched;

                    foreach (WorkOrderBrand brand in workorderManagement.WorkOrder.WorkOrderBrands)
                    {
                        WorkOrderBrand newBrand = new WorkOrderBrand();
                        foreach (var property in brand.GetType().GetProperties())
                        {
                            if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                            {
                                property.SetValue(newBrand, property.GetValue(brand));
                            }
                        }
                        newBrand.WorkorderID = workOrder.WorkorderID;
                        workOrder.WorkOrderBrands.Add(newBrand);
                    }

                    IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
                    assetCounter.IndexValue++;
                    //WOFBEntity.Entry(assetCounter).State = System.Data.Entity.EntityState.Modified;

                    WorkorderEquipment equipment = new WorkorderEquipment()
                    {
                        Assetid = assetCounter.IndexValue.Value,
                        CallTypeid = isAutoGenWO ? workorderManagement.WorkOrder.WorkorderCalltypeid : 1300,
                        Category = isAutoGenWO ? "" : "OTHER",
                        Symptomid = 2001,
                        Location = isAutoGenWO ? workorderManagement.WorkOrder.ClosedUserName : "OTH"
                    };
                    workOrder.WorkorderEquipments.Add(equipment);

                    WorkorderEquipmentRequested equipmentReq = new WorkorderEquipmentRequested()
                    {
                        Assetid = assetCounter.IndexValue.Value,
                        CallTypeid = isAutoGenWO ? workorderManagement.WorkOrder.WorkorderCalltypeid : 1300,
                        Category = isAutoGenWO ? "" : "OTHER",
                        Symptomid = 2001,
                        Location = isAutoGenWO ? workorderManagement.WorkOrder.ClosedUserName : "OTH"
                    };
                    workOrder.WorkorderEquipmentRequesteds.Add(equipmentReq);
                    if (isAutoGenWO)
                    {
                        workorderManagement.WorkOrder.ClosedUserName = string.Empty;
                    }
                    notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = workOrder.WorkorderEntryDate,
                        Notes = isAutoGenWO ? workOrder.WorkorderCalltypeDesc + " Work Order # " + workOrder.WorkorderID + " in MARS!" : @"Install Work Order # " + workOrder.WorkorderID + " in MARS!",
                        Userid = isAutoGenWO ? 99999 : System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = isAutoGenWO ? "WEB" : UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                        isDispatchNotes = 0
                    };
                    workOrder.NotesHistories.Add(notesHistory);
                    if (workorderManagement.Erf != null)
                    {
                        workOrder.WorkorderErfid = workorderManagement.Erf.ErfID;
                    }

                }


                if (workorderManagement.RemovalCount > 5)
                {
                    workOrder.WorkorderCallstatus = "Open";
                }

                WOFBEntity.WorkOrders.Add(workOrder);
                SaveRemovalDetails(workorderManagement, workOrder);


                WorkorderDetail workOrderDetail = new WorkorderDetail()
                {
                    StartDateTime = workorderManagement.Closure.StartDateTime,
                    InvoiceNo = workorderManagement.Closure.InvoiceNo,
                    ArrivalDateTime = workorderManagement.Closure.ArrivalDateTime,
                    CompletionDateTime = workorderManagement.Closure.CompletionDateTime,
                    ResponsibleTechName = workorderManagement.Closure.ResponsibleTechName,
                    Mileage = workorderManagement.Closure.Mileage,
                    CustomerName = workorderManagement.Closure.CustomerName,
                    CustomerEmail = workorderManagement.Closure.CustomerEmail,
                    CustomerSignatureDetails = workorderManagement.Closure.CustomerSignatureDetails,
                    CustomerSignatureBy = workorderManagement.Closure.CustomerSignedBy,
                    TechnicianSignatureDetails = workorderManagement.Closure.TechnicianSignatureDetails,
                    WorkorderID = workOrder.WorkorderID,
                    EntryDate = workOrder.WorkorderEntryDate,
                    ModifiedDate = workOrder.WorkorderEntryDate,
                    SpecialClosure = null,
                    TravelTime = workorderManagement.Closure.TravelHours + ":" + workorderManagement.Closure.TravelMinutes,
                    WaterTested = workorderManagement.Closure.WaterTested,
                    HardnessRating = workorderManagement.Closure.HardnessRating
                };

                if (workorderManagement.Closure.PhoneSolveid > 0)
                {
                    workOrderDetail.PhoneSolveid = workorderManagement.Closure.PhoneSolveid;
                }
                if (workOrderDetail.CustomerSignatureDetails != null)
                {
                    //890 is for empty signature box
                    if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                    {
                        workOrderDetail.CustomerSignatureDetails = WOFBEntity.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                            Select(s => s.CustomerSignatureDetails).FirstOrDefault();
                        if (workOrderDetail.CustomerSignatureDetails != null)
                        {
                            if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                            {
                                workOrderDetail.CustomerSignatureDetails = string.Empty;
                            }
                        }
                        else
                        {
                            workOrderDetail.CustomerSignatureDetails = string.Empty;
                        }
                    }
                }

                if (workOrderDetail.TechnicianSignatureDetails != null)
                {
                    //890 is for empty signature box
                    if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                    {
                        workOrderDetail.TechnicianSignatureDetails = WOFBEntity.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                            Select(s => s.TechnicianSignatureDetails).FirstOrDefault();
                        if (workOrderDetail.TechnicianSignatureDetails != null)
                        {
                            if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                            {
                                workOrderDetail.TechnicianSignatureDetails = string.Empty;
                            }
                        }
                        else
                        {
                            workOrderDetail.TechnicianSignatureDetails = string.Empty;
                        }
                    }
                }

                WOFBEntity.WorkorderDetails.Add(workOrderDetail);

                workOrder.WorkOrderOpenedDateTime = DateTime.Now;
                workOrder.CurrentUserName = UserName;

                // int effectedRecords = WOFBEntity.SaveChanges();
                // returnValue = effectedRecords > 0 ? 1 : 0;
                returnValue = 1;
                //message += @"| Work Order Contact Name field is required!";
            }
            catch(Exception ex)
            {
                returnValue = 0;
                message += @"|Error Creating Workorder!";
            }

            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, WorkOrderId = workOrder.WorkorderID, returnValue = returnValue, message = message };         
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;;
        }


        public int CreatePartsOrder(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            int effectedRecords = 0;
            if (workorderManagement.WorkOrderParts.Count > 0)
            {
                if (workOrder.WorkorderSchedules != null && workOrder.WorkorderSchedules.Count > 0)
                {
                    WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0 && ws.AssignedStatus == "Accepted").FirstOrDefault();
                    if (schedule != null)
                    {
                        int custId = Convert.ToInt32(schedule.Techid);

                        Customer serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == custId).FirstOrDefault();

                        WorkOrder newPartsOrder = workorderManagement.FillCustomerData(new WorkOrder(), true, FarmerBrothersEntitites, serviceCustomer);
                        newPartsOrder.EntryUserName = UserName;

                        using (FarmerBrothersEntities newEntity = new FarmerBrothersEntities())
                        {
                            IndexCounter counter = Utility.GetIndexCounter("WorkorderID", 1);
                            counter.IndexValue++;
                            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                            newPartsOrder.WorkorderID = counter.IndexValue.Value;
                            newPartsOrder.WorkorderSpawnEvent = 1;

                            //newPartsOrder.WorkorderCalltypeid = 1820;
                            newPartsOrder.WorkorderCalltypeDesc = "Parts Request - Manual";
                            newPartsOrder.FollowupCallID = defaultFollowUpCall;

                            newPartsOrder.WorkorderEntryDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                            newPartsOrder.WorkorderModifiedDate = newPartsOrder.WorkorderEntryDate;
                            newPartsOrder.ModifiedUserName = UserName;
                            newPartsOrder.WorkorderCallstatus = "Closed";
                            newPartsOrder.ClosedUserName = UserName;
                            newPartsOrder.WorkorderCloseDate = newPartsOrder.WorkorderEntryDate;


                            DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);

                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = @"Closed Parts Order Created from “MARS” by " + UserName,
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = UserName,
                                isDispatchNotes = 0
                            };
                            newPartsOrder.NotesHistories.Add(notesHistory);

                            SaveWorkOrderParts(workorderManagement, newPartsOrder);

                            string message = "";

                            newEntity.WorkOrders.Add(newPartsOrder);
                            effectedRecords = newEntity.SaveChanges();
                        }
                    }
                }
            }
            return effectedRecords > 0 ? 1 : 0;
        }
        private int OverTimeRequest(WorkorderManagementModel workOrderManagementModel, out WorkOrder workOrder)
        {
            int returnValue = -1;
            StringBuilder salesEmailBody = new StringBuilder();
            workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID).FirstOrDefault();
            if (workOrder != null)
            {
                TimeZoneInfo newTimeZoneInfo = null;
                Utility.GetCustomerTimeZone(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                DateTime CurrentTime = Utility.GetCurrentTime(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 0,
                    EntryDate = CurrentTime,
                    Notes = workOrderManagementModel.OverTimeRequestDescription,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = UserName,
                    isDispatchNotes = 1
                };

                workOrder.NotesHistories.Add(notesHistory);
                workOrder.OvertimeRequest = "Yes";
                returnValue = FarmerBrothersEntitites.SaveChanges();

                if (returnValue > 0)
                {
                    StringBuilder subject = new StringBuilder();
                    subject.Append("OVERTIME REQUEST - WO: ");
                    subject.Append(workOrder.WorkorderID);
                    subject.Append(" ST: ");
                    subject.Append(workOrder.CustomerState);
                    subject.Append(" Call Type: ");
                    subject.Append(workOrder.WorkorderCalltypeDesc);

                    SendWorkOrderMail(workOrder, subject.ToString(), ConfigurationManager.AppSettings["OverTimeRequestAddress"].ToString(), ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"], null, MailType.INFO, false, null);
                }
            }
            return returnValue;
        }

        public void CreateInvoice(WorkorderManagementModel workOrderManagement, WorkOrder workOrder)
        {
            WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0 && ws.AssignedStatus == "Accepted").FirstOrDefault();

            if (schedule != null)
            {
                TechHierarchyView techView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, schedule.Techid.Value);
                if (techView != null)
                {
                    //LG : TODO : Need to modify DistributorName with correct values, 
                    if (string.Compare(techView.DistributorName, "TPSP Branch", true) == 0)
                    {
                        IndexCounter workOrderCounter = Utility.GetIndexCounter("InvoiceID", 1);
                        workOrderCounter.IndexValue++;
                        //FarmerBrothersEntitites.Entry(workOrderCounter).State = System.Data.Entity.EntityState.Modified;

                        WorkorderDetail spawnWorkOrderDetail = new WorkorderDetail();
                        if (workOrder.WorkorderDetails.Count > 0)
                        {
                            WorkorderDetail workOrderDetail = workOrder.WorkorderDetails.ElementAt(0);

                            if (workOrderDetail != null)
                            {
                                double travelTime = 0L;
                                if (!string.IsNullOrWhiteSpace(workOrderDetail.TravelTime))
                                {
                                    string[] travelTimes = workOrderDetail.TravelTime.Split(':');
                                    if (travelTimes.Count() >= 2)
                                    {
                                        int hours = string.IsNullOrWhiteSpace(travelTimes[0]) ? 0 : Convert.ToInt32(travelTimes[0]);
                                        int min = string.IsNullOrWhiteSpace(travelTimes[1]) ? 0 : Convert.ToInt32(travelTimes[1]);
                                        int sec = 0;

                                        TimeSpan travelTimeSpan = new TimeSpan(hours, min, sec);
                                        travelTime = travelTimeSpan.TotalSeconds;
                                    }
                                }

                                Invoice invoice = new Invoice()
                                {
                                    Invoiceid = workOrderCounter.IndexValue.Value.ToString(),
                                    WorkorderID = workOrder.WorkorderID,
                                    CustomerName = workOrder.CustomerName,
                                    ServiceLocation = schedule.ServiceCenterName,
                                    WorkorderCompletionDate = workOrderDetail.CompletionDateTime,
                                    Mileage = Convert.ToInt32(workOrderDetail.Mileage),
                                    TravelTimeInSecs = Convert.ToInt32(travelTime),
                                    InvoiceStatus = @"Awaiting Submission"
                                };
                                FarmerBrothersEntitites.Invoices.Add(invoice);
                            }
                        }
                    }
                }
            }
        }

        public JsonResult PopulateSpecialClosure(WorkorderManagementModel workOrderManagementModel, string callStatus)
        {
            workOrderManagementModel.Closure.SpecialClosureList.Clear();
            workOrderManagementModel.Closure.PopulateSpecialClosureList(workOrderManagementModel.WorkOrder, FarmerBrothersEntitites);
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = workOrderManagementModel.Closure.SpecialClosureList };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public static void Copy(object fromObj, object toObj)
        {
            Type fromObjectType = fromObj.GetType();
            Type toObjectType = toObj.GetType();

            foreach (System.Reflection.PropertyInfo fromProperty in
                fromObjectType.GetProperties())
            {
                if (fromProperty.CanRead)
                {
                    string propertyName = fromProperty.Name;
                    Type propertyType = fromProperty.PropertyType;

                    System.Reflection.PropertyInfo toProperty =
                        toObjectType.GetProperty(propertyName);

                    Type toPropertyType = toProperty.PropertyType;

                    if (toProperty != null && toProperty.CanWrite)
                    {
                        object fromValue = fromProperty.GetValue(fromObj, null);
                        toProperty.SetValue(toObj, fromValue, null);
                    }
                }
            }
        }

      /*private void CreateSpawnWorkOrder(WorkorderManagementModel workorderManagement, WorkorderEquipment equipment, out string message)
        {
            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);
            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            WorkOrder spawnWorkOrder = new WorkOrder();
            List<Type> collections = new List<Type>() { typeof(IEnumerable<>), typeof(IEnumerable) };

            int? responsibleTechId = null;

            foreach (var property in workOrder.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(string) || !property.PropertyType.GetInterfaces().Any(i => collections.Any(c => i == c)))
                {
                    property.SetValue(spawnWorkOrder, property.GetValue(workOrder));
                }
            }

            FarmerBrothersEntities newEntity = new FarmerBrothersEntities();

            IndexCounter workOrderCounter = Utility.GetIndexCounter("WorkorderID", 1);
            workOrderCounter.IndexValue++;
            //newEntity.Entry(workOrderCounter).State = System.Data.Entity.EntityState.Modified;

            spawnWorkOrder.WorkorderID = workOrderCounter.IndexValue.Value;
            spawnWorkOrder.WorkorderEntryDate = currentTime;
            spawnWorkOrder.WorkorderCallstatus = "Open";
            spawnWorkOrder.WorkorderSpawnEvent = 1;
            spawnWorkOrder.WorkorderCloseDate = null;
            if (workOrder.OriginalWorkorderid.HasValue)
            {
                spawnWorkOrder.OriginalWorkorderid = workOrder.OriginalWorkorderid;
            }
            else
            {
                spawnWorkOrder.OriginalWorkorderid = workOrder.WorkorderID;
            }

            spawnWorkOrder.ParentWorkorderid = workOrder.WorkorderID;
            if (workOrder.SpawnCounter.HasValue)
            {
                spawnWorkOrder.SpawnCounter = workOrder.SpawnCounter.Value + 1;
            }
            else
            {
                spawnWorkOrder.SpawnCounter = 1;
            }

            WorkorderDetail spawnWorkOrderDetail = new WorkorderDetail();
            if (workOrder.WorkorderDetails.Count > 0)
            {
                WorkorderDetail workOrderDetail = workOrder.WorkorderDetails.ElementAt(0);
                foreach (var property in workOrderDetail.GetType().GetProperties())
                {
                    if (property.GetValue(workOrderDetail) != null && property.GetValue(workOrderDetail).GetType() != null && (property.GetValue(workOrderDetail).GetType().IsValueType || property.GetValue(workOrderDetail).GetType() == typeof(string)))
                    {
                        property.SetValue(spawnWorkOrderDetail, property.GetValue(workOrderDetail));
                    }
                }
                spawnWorkOrderDetail.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnWorkOrderDetail.ArrivalDateTime = null;
                spawnWorkOrderDetail.CompletionDateTime = null;
                spawnWorkOrderDetail.StartDateTime = null;
                spawnWorkOrderDetail.EntryDate = null;
                spawnWorkOrderDetail.ModifiedDate = null;
                spawnWorkOrderDetail.SpecialClosure = "";
                spawnWorkOrderDetail.TravelTime = "";
                spawnWorkOrderDetail.InvoiceNo = "";
                spawnWorkOrderDetail.SolutionId = equipment.Solutionid;

                spawnWorkOrder.WorkorderDetails.Add(spawnWorkOrderDetail);
            }


            foreach (WorkOrderBrand brand in workOrder.WorkOrderBrands)
            {
                WorkOrderBrand newBrand = new WorkOrderBrand();
                foreach (var property in brand.GetType().GetProperties())
                {
                    if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                    {
                        property.SetValue(newBrand, property.GetValue(brand));
                    }
                }
                newBrand.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnWorkOrder.WorkOrderBrands.Add(newBrand);
            }

            foreach (NotesHistory notes in workOrder.NotesHistories)
            {
                NotesHistory newNotes = new NotesHistory();
                foreach (var property in notes.GetType().GetProperties())
                {
                    if (property.GetValue(notes) != null && property.GetValue(notes).GetType() != null && (property.GetValue(notes).GetType().IsValueType || property.GetValue(notes).GetType() == typeof(string)))
                    {
                        property.SetValue(newNotes, property.GetValue(notes));
                    }
                }
                newNotes.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnWorkOrder.NotesHistories.Add(newNotes);
            }

            NotesHistory notesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = currentTime,
                Notes = @"Work Order spawned in MARS from work order " + workOrder.WorkorderID,
                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                UserName = UserName
            };
            spawnWorkOrder.NotesHistories.Add(notesHistory);

            NotesHistory WONotesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = currentTime,
                Notes = @"Workorder " + spawnWorkOrder.WorkorderID + " spawned due to Solution Code " + equipment.Solutionid,
                Userid = 1234, //TBD
                UserName = UserName
            };
            workOrder.NotesHistories.Add(WONotesHistory);

            foreach (WorkorderReasonlog reasonLog in workOrder.WorkorderReasonlogs)
            {
                WorkorderReasonlog newReasonLog = new WorkorderReasonlog();
                foreach (var property in reasonLog.GetType().GetProperties())
                {
                    if (property.GetValue(reasonLog) != null && property.GetValue(reasonLog).GetType() != null && (property.GetValue(reasonLog).GetType().IsValueType || property.GetValue(reasonLog).GetType() == typeof(string)))
                    {
                        property.SetValue(newReasonLog, property.GetValue(reasonLog));
                    }
                }
                newReasonLog.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnWorkOrder.WorkorderReasonlogs.Add(newReasonLog);
            }

            WorkorderType newWorkOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == 1310).FirstOrDefault();

            if (equipment.Solutionid == 5160 && newWorkOrderType != null)
            {
                spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;
            }
            else
            {
                spawnWorkOrder.WorkorderCalltypeid = equipment.CallTypeid;
                WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                if (workOrderType != null)
                {
                    spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                }
            }

            if (equipment.Solutionid == 5160 || equipment.Solutionid == 5191)
            {
                if (!string.IsNullOrWhiteSpace(workorderManagement.SpawnReason))
                {
                    spawnWorkOrderDetail.SpawnReason = Convert.ToInt32(workorderManagement.SpawnReason);
                }

                notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 0,
                    EntryDate = currentTime,
                    Notes = workorderManagement.SpawnNotes,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = UserName
                };
                spawnWorkOrder.NotesHistories.Add(notesHistory);
            }

            if (equipment.Solutionid == 9999)
            {
                if (!string.IsNullOrWhiteSpace(workorderManagement.NSRReason))
                {
                    spawnWorkOrderDetail.NSRReason = Convert.ToInt32(workorderManagement.NSRReason);
                }

                notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 0,
                    EntryDate = currentTime,
                    Notes = workorderManagement.NSRNotes,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = UserName
                };
                spawnWorkOrder.NotesHistories.Add(notesHistory);
            }

            notesHistory = new NotesHistory()
            {
                AutomaticNotes = 0,
                EntryDate = currentTime,
                Notes = "SpawnedEquipment : SerialNumber - " + equipment.SerialNumber + ", Description - " + equipment.WorkDescription,
                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                UserName = UserName
            };
            spawnWorkOrder.NotesHistories.Add(notesHistory);

            WorkorderEquipmentRequested spawnEquipmentRequested = new WorkorderEquipmentRequested();

            WorkorderEquipmentRequested workOrderReq = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == equipment.Assetid).FirstOrDefault();

            spawnEquipmentRequested.Assetid = equipment.Assetid;
            spawnEquipmentRequested.CallTypeid = equipment.CallTypeid;
            spawnEquipmentRequested.CatalogID = equipment.CatalogID;
            spawnEquipmentRequested.Category = equipment.Category;
            spawnEquipmentRequested.EquipmentId = equipment.EquipmentId;
            spawnEquipmentRequested.FeastMovementid = equipment.FeastMovementid;
            spawnEquipmentRequested.Location = equipment.Location;
            spawnEquipmentRequested.Manufacturer = equipment.Manufacturer;
            spawnEquipmentRequested.Model = equipment.Model;
            spawnEquipmentRequested.Name = equipment.Name;
            spawnEquipmentRequested.QualityIssue = equipment.QualityIssue;
            spawnEquipmentRequested.SerialNumber = equipment.SerialNumber;
            spawnEquipmentRequested.WorkorderID = equipment.WorkorderID;
            spawnEquipmentRequested.Temperature = "";
            spawnEquipmentRequested.Weight = "";
            spawnEquipmentRequested.Ratio = "";
            spawnEquipmentRequested.Settings = "";
            spawnEquipmentRequested.WorkPerformedCounter = "";
            spawnEquipmentRequested.WorkDescription = "";
            spawnEquipmentRequested.Systemid = equipment.Systemid;
            spawnEquipmentRequested.Symptomid = equipment.Symptomid;
            spawnEquipmentRequested.Email = "";
            spawnEquipmentRequested.NoPartsNeeded = null;
            spawnEquipmentRequested.Solutionid = null;

            if (equipment.Solutionid == 5160)
            {
                spawnEquipmentRequested.CallTypeid = 1310;
            }

            IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
            assetCounter.IndexValue++;
            //newEntity.Entry(assetCounter).State = System.Data.Entity.EntityState.Modified;
            spawnEquipmentRequested.Assetid = assetCounter.IndexValue.Value;
            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested);

            if (equipment.Solutionid == 5160)
            {
                WorkorderEquipment spawnEquipment2 = new WorkorderEquipment();
                WorkorderEquipmentRequested spawnEquipmentRequested2 = new WorkorderEquipmentRequested();
                WorkorderEquipmentRequested workOrderReq2 = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == equipment.Assetid).FirstOrDefault();
                spawnEquipment2.Assetid = equipment.Assetid;
                spawnEquipment2.CallTypeid = equipment.CallTypeid;
                spawnEquipment2.CatalogID = equipment.CatalogID;
                spawnEquipment2.Category = equipment.Category;
                spawnEquipment2.EquipmentId = equipment.EquipmentId;
                spawnEquipment2.FeastMovementid = equipment.FeastMovementid;
                spawnEquipment2.IsSlNumberImageExist = equipment.IsSlNumberImageExist;
                spawnEquipment2.Location = equipment.Location;
                spawnEquipment2.Manufacturer = equipment.Manufacturer;
                spawnEquipment2.Model = equipment.Model;
                spawnEquipment2.Name = equipment.Name;
                spawnEquipment2.QualityIssue = equipment.QualityIssue;
                spawnEquipment2.SerialNumber = equipment.SerialNumber;
                spawnEquipment2.CallTypeid = 1410;
                spawnEquipment2.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnEquipment2.Temperature = "";
                spawnEquipment2.Weight = "";
                spawnEquipment2.Ratio = "";
                spawnEquipment2.Settings = "";
                spawnEquipment2.WorkPerformedCounter = "";
                spawnEquipment2.WorkDescription = "";
                spawnEquipment2.Systemid = equipment.Systemid;
                spawnEquipment2.Symptomid = equipment.Symptomid;
                spawnEquipment2.Email = "";
                spawnEquipment2.NoPartsNeeded = null;
                spawnEquipment2.Solutionid = null;

                //spawnEquipment2.Systemid = null;
                //spawnEquipment2.Symptomid = equipment.Symptomid;
                //spawnEquipment2.Email = "";
                //spawnEquipment2.NoPartsNeeded = null;
                //spawnEquipment2.Solutionid = null;

                spawnEquipmentRequested2.Assetid = equipment.Assetid;
                spawnEquipmentRequested2.CallTypeid = equipment.CallTypeid;
                spawnEquipmentRequested2.CatalogID = equipment.CatalogID;
                spawnEquipmentRequested2.Category = equipment.Category;
                spawnEquipmentRequested2.EquipmentId = equipment.EquipmentId;
                spawnEquipmentRequested2.FeastMovementid = equipment.FeastMovementid;
                spawnEquipmentRequested2.Location = equipment.Location;
                spawnEquipmentRequested2.Manufacturer = equipment.Manufacturer;
                spawnEquipmentRequested2.Model = equipment.Model;
                spawnEquipmentRequested2.Name = equipment.Name;
                spawnEquipmentRequested2.QualityIssue = equipment.QualityIssue;
                spawnEquipmentRequested2.SerialNumber = equipment.SerialNumber;
                spawnEquipmentRequested2.WorkorderID = equipment.WorkorderID;
                spawnEquipmentRequested2.CallTypeid = 1410;
                spawnEquipmentRequested2.WorkorderID = equipment.WorkorderID;
                spawnEquipmentRequested2.Temperature = "";
                spawnEquipmentRequested2.Weight = "";
                spawnEquipmentRequested2.Ratio = "";
                spawnEquipmentRequested2.Settings = "";
                spawnEquipmentRequested2.WorkPerformedCounter = "";
                spawnEquipmentRequested2.WorkDescription = "";
                spawnEquipmentRequested2.Systemid = equipment.Systemid;
                spawnEquipmentRequested2.Symptomid = equipment.Symptomid;
                spawnEquipmentRequested2.Email = "";
                spawnEquipmentRequested2.NoPartsNeeded = null;
                spawnEquipmentRequested2.Solutionid = null;

                IndexCounter assetCounter2 = Utility.GetIndexCounter("AssetID", 1);
                assetCounter2.IndexValue++;
                //newEntity.Entry(assetCounter2).State = System.Data.Entity.EntityState.Modified;
                spawnEquipment2.Assetid = assetCounter2.IndexValue.Value;
                spawnEquipmentRequested2.Assetid = assetCounter2.IndexValue.Value;

                spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment2);
                spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested2);
            }

            newEntity.WorkOrders.Add(spawnWorkOrder);
            newEntity.SaveChanges();

            if (newEntity != null)
            {
                newEntity.Dispose();
            }

            string emailAddresses = string.Empty;


            StringBuilder subject = new StringBuilder();
            subject.Append("Spawned Workorder - Original WO: ");
            subject.Append(spawnWorkOrder.OriginalWorkorderid);
            subject.Append(" ST: ");
            subject.Append(spawnWorkOrder.CustomerState);
            subject.Append(" Call Type: ");
            subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);

            SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null);

            if (responsibleTechId.HasValue)
            {
                subject = new StringBuilder();
                subject.Append("WO:");
                subject.Append(spawnWorkOrder.WorkorderID);
                subject.Append(" ST:");
                subject.Append(spawnWorkOrder.CustomerState);
                subject.Append(" Call Type:");
                subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);


                string emailAddress = string.Empty;
                string salesEmailAddress = string.Empty;
                var CustomerId = int.Parse(responsibleTechId.Value.ToString());
                Customer serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                if (serviceCustomer != null)
                {
                    emailAddress = serviceCustomer.Email;
                    if (!string.IsNullOrEmpty(serviceCustomer.SalesEmail))
                    {
                        salesEmailAddress = serviceCustomer.SalesEmail;
                    }
                }

                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TestEmail"]))
                {
                    emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                }

                if (!string.IsNullOrWhiteSpace(emailAddress))
                {
                    SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], responsibleTechId, MailType.SPAWN, true, null, string.Empty, false, salesEmailAddress);
                }
            }

            message = @"Spawned Work Order " + spawnWorkOrder.WorkorderID + " is created!";

        }
        */

        //Removed on Aug 31, 2021
        private void CreateSpawnWorkOrder_Old(WorkorderManagementModel workorderManagement, List<WorkorderEquipment> equipment, out string message)
        {
            List<int?> uniqueSolutionIds = workorderManagement.WorkOrder.WorkorderEquipments.Select(x => x.Solutionid).Distinct().ToList();

            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);
            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            string SpawnedWOsCreated = "";
            foreach (int soluitonId in uniqueSolutionIds)
            {
                if (soluitonId == 5115
                                            || soluitonId == 5120
                                            || soluitonId == 5130
                                            || soluitonId == 5135
                                            || soluitonId == 5140
                                            || soluitonId == 5170
                                            || soluitonId == 5171
                                            || soluitonId == 5181
                                            || soluitonId == 5191)
                {

                    List<WorkorderEquipment> workorderEqps = workorderManagement.WorkOrder.WorkorderEquipments.Where(eq => eq.Solutionid == soluitonId).ToList();


                    WorkOrder spawnWorkOrder = new WorkOrder();
                    List<Type> collections = new List<Type>() { typeof(IEnumerable<>), typeof(IEnumerable) };

                    int? responsibleTechId = null;

                    foreach (var property in workOrder.GetType().GetProperties())
                    {
                        if (property.PropertyType == typeof(string) || !property.PropertyType.GetInterfaces().Any(i => collections.Any(c => i == c)))
                        {
                            property.SetValue(spawnWorkOrder, property.GetValue(workOrder));
                        }
                    }

                    FarmerBrothersEntities newEntity = new FarmerBrothersEntities();

                    IndexCounter workOrderCounter = Utility.GetIndexCounter("WorkorderID", 1);
                    workOrderCounter.IndexValue++;
                    //newEntity.Entry(workOrderCounter).State = System.Data.Entity.EntityState.Modified;

                    spawnWorkOrder.WorkorderID = workOrderCounter.IndexValue.Value;
                    spawnWorkOrder.WorkorderEntryDate = currentTime;
                    spawnWorkOrder.WorkorderCallstatus = "Open";
                    spawnWorkOrder.WorkorderSpawnEvent = 1;
                    spawnWorkOrder.WorkorderCloseDate = null;
                    if (workOrder.OriginalWorkorderid.HasValue)
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.OriginalWorkorderid;
                    }
                    else
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.WorkorderID;
                    }

                    spawnWorkOrder.ParentWorkorderid = workOrder.WorkorderID;
                    if (workOrder.SpawnCounter.HasValue)
                    {
                        spawnWorkOrder.SpawnCounter = workOrder.SpawnCounter.Value + 1;
                    }
                    else
                    {
                        spawnWorkOrder.SpawnCounter = 1;
                    }

                    WorkorderDetail spawnWorkOrderDetail = new WorkorderDetail();
                    if (workOrder.WorkorderDetails.Count > 0)
                    {
                        WorkorderDetail workOrderDetail = workOrder.WorkorderDetails.ElementAt(0);
                        foreach (var property in workOrderDetail.GetType().GetProperties())
                        {
                            if (property.GetValue(workOrderDetail) != null && property.GetValue(workOrderDetail).GetType() != null && (property.GetValue(workOrderDetail).GetType().IsValueType || property.GetValue(workOrderDetail).GetType() == typeof(string)))
                            {
                                property.SetValue(spawnWorkOrderDetail, property.GetValue(workOrderDetail));
                            }
                        }
                        spawnWorkOrderDetail.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrderDetail.ArrivalDateTime = null;
                        spawnWorkOrderDetail.CompletionDateTime = null;
                        spawnWorkOrderDetail.StartDateTime = null;
                        spawnWorkOrderDetail.EntryDate = null;
                        spawnWorkOrderDetail.ModifiedDate = null;
                        spawnWorkOrderDetail.SpecialClosure = "";
                        spawnWorkOrderDetail.TravelTime = "";
                        spawnWorkOrderDetail.InvoiceNo = "";
                        spawnWorkOrderDetail.SolutionId = soluitonId;

                        spawnWorkOrder.WorkorderDetails.Add(spawnWorkOrderDetail);
                    }


                    foreach (WorkOrderBrand brand in workOrder.WorkOrderBrands)
                    {
                        WorkOrderBrand newBrand = new WorkOrderBrand();
                        foreach (var property in brand.GetType().GetProperties())
                        {
                            if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                            {
                                property.SetValue(newBrand, property.GetValue(brand));
                            }
                        }
                        newBrand.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkOrderBrands.Add(newBrand);
                    }

                    foreach (NotesHistory notes in workOrder.NotesHistories)
                    {
                        NotesHistory newNotes = new NotesHistory();
                        foreach (var property in notes.GetType().GetProperties())
                        {
                            if (property.GetValue(notes) != null && property.GetValue(notes).GetType() != null && (property.GetValue(notes).GetType().IsValueType || property.GetValue(notes).GetType() == typeof(string)))
                            {
                                property.SetValue(newNotes, property.GetValue(notes));
                            }
                        }
                        newNotes.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.NotesHistories.Add(newNotes);
                    }

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Work Order spawned in MARS from work order " + workOrder.WorkorderID,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 0
                    };
                    spawnWorkOrder.NotesHistories.Add(notesHistory);

                    NotesHistory WONotesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Workorder " + spawnWorkOrder.WorkorderID + " spawned due to Solution Code " + soluitonId,
                        Userid = 1234, //TBD
                        UserName = UserName,
                        isDispatchNotes = 0
                    };
                    workOrder.NotesHistories.Add(WONotesHistory);

                    foreach (WorkorderReasonlog reasonLog in workOrder.WorkorderReasonlogs)
                    {
                        WorkorderReasonlog newReasonLog = new WorkorderReasonlog();
                        foreach (var property in reasonLog.GetType().GetProperties())
                        {
                            if (property.GetValue(reasonLog) != null && property.GetValue(reasonLog).GetType() != null && (property.GetValue(reasonLog).GetType().IsValueType || property.GetValue(reasonLog).GetType() == typeof(string)))
                            {
                                property.SetValue(newReasonLog, property.GetValue(reasonLog));
                            }
                        }
                        newReasonLog.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkorderReasonlogs.Add(newReasonLog);
                    }

                    WorkorderType newWorkOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == 1310).FirstOrDefault();

                    if (soluitonId == 5160 && newWorkOrderType != null)
                    {
                        spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                        spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;
                    }
                    else
                    {
                        WorkorderEquipment eqp = workorderEqps.Where(e => e.CallTypeid == 1200).FirstOrDefault();
                        if (eqp != null)
                        {
                            WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                            if (workOrderType != null)
                            {
                                spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                            }
                        }
                        else
                        {
                            eqp = workorderEqps.OrderBy(equip => equip.Assetid).ElementAt(0);
                            if (eqp != null)
                            {
                                WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                                if (workOrderType != null)
                                {
                                    spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                    spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                }
                            }
                        }


                        //spawnWorkOrder.WorkorderCalltypeid = equipment.CallTypeid;
                        //WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                        //if (workOrderType != null)
                        //{
                        //    spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                        //}
                    }

                    if (soluitonId == 5160 || soluitonId == 5191)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.SpawnReason))
                        {
                            spawnWorkOrderDetail.SpawnReason = Convert.ToInt32(workorderManagement.SpawnReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.SpawnNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 0
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }

                    if (soluitonId == 9999)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.NSRReason))
                        {
                            spawnWorkOrderDetail.NSRReason = Convert.ToInt32(workorderManagement.NSRReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.NSRNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 1
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }                   

                    foreach (WorkorderEquipment eqpItem in workorderEqps)
                    {

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "SpawnedEquipment : SerialNumber - " + eqpItem.SerialNumber + ", Description - " + eqpItem.WorkDescription,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 0
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);

                        WorkorderEquipmentRequested spawnEquipmentRequested = new WorkorderEquipmentRequested();
                        WorkorderEquipmentRequested workOrderReq = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();

                        spawnEquipmentRequested.Assetid = eqpItem.Assetid;
                        spawnEquipmentRequested.CallTypeid = eqpItem.CallTypeid;
                        spawnEquipmentRequested.CatalogID = eqpItem.CatalogID;
                        spawnEquipmentRequested.Category = eqpItem.Category;
                        spawnEquipmentRequested.EquipmentId = eqpItem.EquipmentId;
                        spawnEquipmentRequested.FeastMovementid = eqpItem.FeastMovementid;
                        spawnEquipmentRequested.Location = eqpItem.Location;
                        spawnEquipmentRequested.Manufacturer = eqpItem.Manufacturer;
                        spawnEquipmentRequested.Model = eqpItem.Model;
                        spawnEquipmentRequested.Name = eqpItem.Name;
                        spawnEquipmentRequested.QualityIssue = eqpItem.QualityIssue;
                        spawnEquipmentRequested.SerialNumber = eqpItem.SerialNumber;
                        spawnEquipmentRequested.WorkorderID = eqpItem.WorkorderID;
                        spawnEquipmentRequested.Temperature = "";
                        spawnEquipmentRequested.Weight = "";
                        spawnEquipmentRequested.Ratio = "";
                        spawnEquipmentRequested.Settings = "";
                        spawnEquipmentRequested.WorkPerformedCounter = "";
                        spawnEquipmentRequested.WorkDescription = "";
                        spawnEquipmentRequested.Systemid = eqpItem.Systemid;
                        spawnEquipmentRequested.Symptomid = eqpItem.Symptomid;
                        spawnEquipmentRequested.Email = "";
                        spawnEquipmentRequested.NoPartsNeeded = null;
                        spawnEquipmentRequested.Solutionid = null;

                        if (soluitonId == 5160)
                        {
                            spawnEquipmentRequested.CallTypeid = 1310;
                        }

                        IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
                        assetCounter.IndexValue++;
                        //newEntity.Entry(assetCounter).State = System.Data.Entity.EntityState.Modified;
                        spawnEquipmentRequested.Assetid = assetCounter.IndexValue.Value;
                        spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested);

                        if (soluitonId == 5160)
                        {
                            WorkorderEquipment spawnEquipment2 = new WorkorderEquipment();
                            WorkorderEquipmentRequested spawnEquipmentRequested2 = new WorkorderEquipmentRequested();
                            WorkorderEquipmentRequested workOrderReq2 = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();
                            spawnEquipment2.Assetid = eqpItem.Assetid;
                            spawnEquipment2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipment2.CatalogID = eqpItem.CatalogID;
                            spawnEquipment2.Category = eqpItem.Category;
                            spawnEquipment2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipment2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipment2.IsSlNumberImageExist = eqpItem.IsSlNumberImageExist;
                            spawnEquipment2.Location = eqpItem.Location;
                            spawnEquipment2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipment2.Model = eqpItem.Model;
                            spawnEquipment2.Name = eqpItem.Name;
                            spawnEquipment2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipment2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipment2.CallTypeid = 1410;
                            spawnEquipment2.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipment2.Temperature = "";
                            spawnEquipment2.Weight = "";
                            spawnEquipment2.Ratio = "";
                            spawnEquipment2.Settings = "";
                            spawnEquipment2.WorkPerformedCounter = "";
                            spawnEquipment2.WorkDescription = "";
                            spawnEquipment2.Systemid = eqpItem.Systemid;
                            spawnEquipment2.Symptomid = eqpItem.Symptomid;
                            spawnEquipment2.Email = "";
                            spawnEquipment2.NoPartsNeeded = null;
                            spawnEquipment2.Solutionid = null;

                            //spawnEquipment2.Systemid = null;
                            //spawnEquipment2.Symptomid = eqpItem.Symptomid;
                            //spawnEquipment2.Email = "";
                            //spawnEquipment2.NoPartsNeeded = null;
                            //spawnEquipment2.Solutionid = null;

                            spawnEquipmentRequested2.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipmentRequested2.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested2.Category = eqpItem.Category;
                            spawnEquipmentRequested2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested2.Location = eqpItem.Location;
                            spawnEquipmentRequested2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested2.Model = eqpItem.Model;
                            spawnEquipmentRequested2.Name = eqpItem.Name;
                            spawnEquipmentRequested2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested2.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested2.CallTypeid = 1410;
                            spawnEquipmentRequested2.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested2.Temperature = "";
                            spawnEquipmentRequested2.Weight = "";
                            spawnEquipmentRequested2.Ratio = "";
                            spawnEquipmentRequested2.Settings = "";
                            spawnEquipmentRequested2.WorkPerformedCounter = "";
                            spawnEquipmentRequested2.WorkDescription = "";
                            spawnEquipmentRequested2.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested2.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested2.Email = "";
                            spawnEquipmentRequested2.NoPartsNeeded = null;
                            spawnEquipmentRequested2.Solutionid = null;

                            IndexCounter assetCounter2 = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter2.IndexValue++;
                            //newEntity.Entry(assetCounter2).State = System.Data.Entity.EntityState.Modified;
                            spawnEquipment2.Assetid = assetCounter2.IndexValue.Value;
                            spawnEquipmentRequested2.Assetid = assetCounter2.IndexValue.Value;

                            spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment2);
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested2);
                        }
                    }

                    newEntity.WorkOrders.Add(spawnWorkOrder);
                    newEntity.SaveChanges();

                    if (newEntity != null)
                    {
                        newEntity.Dispose();
                    }

                    string emailAddresses = string.Empty;


                    StringBuilder subject = new StringBuilder();
                    subject.Append("Spawned Workorder - Original WO: ");
                    subject.Append(spawnWorkOrder.OriginalWorkorderid);
                    subject.Append(" ST: ");
                    subject.Append(spawnWorkOrder.CustomerState);
                    subject.Append(" Call Type: ");
                    subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);

                    SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null);

                    if (responsibleTechId.HasValue)
                    {
                        subject = new StringBuilder();
                        subject.Append("WO:");
                        subject.Append(spawnWorkOrder.WorkorderID);
                        subject.Append(" ST:");
                        subject.Append(spawnWorkOrder.CustomerState);
                        subject.Append(" Call Type:");
                        subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);


                        string emailAddress = string.Empty;
                        string salesEmailAddress = string.Empty;
                        var CustomerId = int.Parse(responsibleTechId.Value.ToString());
                        Customer serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                        if (serviceCustomer != null)
                        {
                            emailAddress = serviceCustomer.Email;
                            if (!string.IsNullOrEmpty(serviceCustomer.SalesEmail))
                            {
                                salesEmailAddress = serviceCustomer.SalesEmail;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TestEmail"]))
                        {
                            emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                        }

                        if (!string.IsNullOrWhiteSpace(emailAddress))
                        {
                            SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], responsibleTechId, MailType.SPAWN, true, null, string.Empty, false, salesEmailAddress);
                        }
                    }

                    if (string.IsNullOrEmpty(SpawnedWOsCreated))
                    {
                        SpawnedWOsCreated += spawnWorkOrder.WorkorderID;
                    }
                    else
                    {
                        SpawnedWOsCreated += ", " + spawnWorkOrder.WorkorderID;
                    }
                    //message = @"Spawned Work Order " + spawnWorkOrder.WorkorderID + " is created!";
                }
            }

            if (!string.IsNullOrEmpty(SpawnedWOsCreated))
            {
                NotesHistory WONotesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = @"Spawned Work Order " + SpawnedWOsCreated + " created in MARS ",
                    Userid = 1234, //TBD
                    UserName = UserName,
                    isDispatchNotes = 0
                };
                //workOrder.NotesHistories.Add(WONotesHistory);
                //FarmerBrothersEntitites.SaveChanges();

                message = @"Spawned Work Order " + SpawnedWOsCreated + " created!";
            }
            else
            {
                message = @"";
            }
        }

        private void CreateSpawnWorkOrder1(WorkorderManagementModel workorderManagement, List<WorkorderEquipment> equipment, out string message)
        {
            List<int?> uniqueSolutionIds = workorderManagement.WorkOrder.WorkorderEquipments.Select(x => x.Solutionid).Distinct().ToList();

            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);
            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            string SpawnedWOsCreated = "";
            foreach (int soluitonId in uniqueSolutionIds)
            {
                if (soluitonId == 5115
                 || soluitonId == 5120
                 || soluitonId == 5130
                 || soluitonId == 5135
                 || soluitonId == 5140
                 || soluitonId == 5170
                 || soluitonId == 5171
                 || soluitonId == 5181
                 || soluitonId == 5191)
                {

                    List<WorkorderEquipment> workorderEqps = workorderManagement.WorkOrder.WorkorderEquipments.Where(eq => eq.Solutionid == soluitonId).ToList();

                    WorkOrder spawnWorkOrder = new WorkOrder();
                    List<Type> collections = new List<Type>() { typeof(IEnumerable<>), typeof(IEnumerable) };

                    int? responsibleTechId = null;

                    foreach (var property in workOrder.GetType().GetProperties())
                    {
                        if (property.PropertyType == typeof(string) || !property.PropertyType.GetInterfaces().Any(i => collections.Any(c => i == c)))
                        {
                            property.SetValue(spawnWorkOrder, property.GetValue(workOrder));
                        }
                    }

                    FarmerBrothersEntities newEntity = new FarmerBrothersEntities();

                    IndexCounter workOrderCounter = Utility.GetIndexCounter("WorkorderID", 1);
                    workOrderCounter.IndexValue++;

                    spawnWorkOrder.WorkorderID = workOrderCounter.IndexValue.Value;
                    spawnWorkOrder.WorkorderEntryDate = currentTime;
                    spawnWorkOrder.WorkorderCallstatus = "Open";
                    spawnWorkOrder.WorkorderSpawnEvent = 1;
                    spawnWorkOrder.WorkorderCloseDate = null;
                    if (workOrder.OriginalWorkorderid.HasValue)
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.OriginalWorkorderid;
                    }
                    else
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.WorkorderID;
                    }

                    spawnWorkOrder.ParentWorkorderid = workOrder.WorkorderID;
                    if (workOrder.SpawnCounter.HasValue)
                    {
                        spawnWorkOrder.SpawnCounter = workOrder.SpawnCounter.Value + 1;
                    }
                    else
                    {
                        spawnWorkOrder.SpawnCounter = 1;
                    }

                    WorkorderDetail spawnWorkOrderDetail = new WorkorderDetail();
                    if (workOrder.WorkorderDetails.Count > 0)
                    {
                        WorkorderDetail workOrderDetail = workOrder.WorkorderDetails.ElementAt(0);
                        foreach (var property in workOrderDetail.GetType().GetProperties())
                        {
                            if (property.GetValue(workOrderDetail) != null && property.GetValue(workOrderDetail).GetType() != null && (property.GetValue(workOrderDetail).GetType().IsValueType || property.GetValue(workOrderDetail).GetType() == typeof(string)))
                            {
                                property.SetValue(spawnWorkOrderDetail, property.GetValue(workOrderDetail));
                            }
                        }
                        spawnWorkOrderDetail.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrderDetail.ArrivalDateTime = null;
                        spawnWorkOrderDetail.CompletionDateTime = null;
                        spawnWorkOrderDetail.StartDateTime = null;
                        spawnWorkOrderDetail.EntryDate = null;
                        spawnWorkOrderDetail.ModifiedDate = null;
                        spawnWorkOrderDetail.SpecialClosure = "";
                        spawnWorkOrderDetail.TravelTime = "";
                        spawnWorkOrderDetail.InvoiceNo = "";
                        spawnWorkOrderDetail.SolutionId = soluitonId;

                        spawnWorkOrder.WorkorderDetails.Add(spawnWorkOrderDetail);
                    }


                    foreach (WorkOrderBrand brand in workOrder.WorkOrderBrands)
                    {
                        WorkOrderBrand newBrand = new WorkOrderBrand();
                        foreach (var property in brand.GetType().GetProperties())
                        {
                            if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                            {
                                property.SetValue(newBrand, property.GetValue(brand));
                            }
                        }
                        newBrand.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkOrderBrands.Add(newBrand);
                    }

                    foreach (NotesHistory notes in workOrder.NotesHistories)
                    {
                        NotesHistory newNotes = new NotesHistory();
                        foreach (var property in notes.GetType().GetProperties())
                        {
                            if (property.GetValue(notes) != null && property.GetValue(notes).GetType() != null && (property.GetValue(notes).GetType().IsValueType || property.GetValue(notes).GetType() == typeof(string)))
                            {
                                property.SetValue(newNotes, property.GetValue(notes));
                            }
                        }
                        newNotes.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.NotesHistories.Add(newNotes);
                    }

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Work Order spawned in MARS from work order " + workOrder.WorkorderID,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 0
                    };
                    spawnWorkOrder.NotesHistories.Add(notesHistory);

                    NotesHistory WONotesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Workorder " + spawnWorkOrder.WorkorderID + " spawned due to Solution Code " + soluitonId,
                        Userid = 1234, 
                        UserName = UserName,
                        isDispatchNotes = 0
                    };
                    workOrder.NotesHistories.Add(WONotesHistory);

                    foreach (WorkorderReasonlog reasonLog in workOrder.WorkorderReasonlogs)
                    {
                        WorkorderReasonlog newReasonLog = new WorkorderReasonlog();
                        foreach (var property in reasonLog.GetType().GetProperties())
                        {
                            if (property.GetValue(reasonLog) != null && property.GetValue(reasonLog).GetType() != null && (property.GetValue(reasonLog).GetType().IsValueType || property.GetValue(reasonLog).GetType() == typeof(string)))
                            {
                                property.SetValue(newReasonLog, property.GetValue(reasonLog));
                            }
                        }
                        newReasonLog.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkorderReasonlogs.Add(newReasonLog);
                    }

                    WorkorderType newWorkOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == 1310).FirstOrDefault();

                    if (soluitonId == 5160 && newWorkOrderType != null)
                    {
                        spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                        spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;
                    }
                    else
                    {
                        WorkorderEquipment eqp = workorderEqps.Where(e => e.CallTypeid == 1200).FirstOrDefault();
                        if (eqp != null)
                        {
                            WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                            if (workOrderType != null)
                            {
                                spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                            }
                        }
                        else
                        {
                            eqp = workorderEqps.OrderBy(equip => equip.Assetid).ElementAt(0);
                            if (eqp != null)
                            {
                                WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                                if (workOrderType != null)
                                {
                                    spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                    spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                }
                            }
                        }
                        
                    }

                    if (soluitonId == 5160 || soluitonId == 5191)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.SpawnReason))
                        {
                            spawnWorkOrderDetail.SpawnReason = Convert.ToInt32(workorderManagement.SpawnReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.SpawnNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 0
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }

                    if (soluitonId == 9999)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.NSRReason))
                        {
                            spawnWorkOrderDetail.NSRReason = Convert.ToInt32(workorderManagement.NSRReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.NSRNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 1
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }

                    foreach (WorkorderEquipment eqpItem in workorderEqps)
                    {

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "SpawnedEquipment : SerialNumber - " + eqpItem.SerialNumber + ", Description - " + eqpItem.WorkDescription,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 0
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);

                        WorkorderEquipmentRequested spawnEquipmentRequested = new WorkorderEquipmentRequested();
                        WorkorderEquipmentRequested workOrderReq = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();

                        spawnEquipmentRequested.Assetid = eqpItem.Assetid;
                        spawnEquipmentRequested.CallTypeid = eqpItem.CallTypeid;
                        spawnEquipmentRequested.CatalogID = eqpItem.CatalogID;
                        spawnEquipmentRequested.Category = eqpItem.Category;
                        spawnEquipmentRequested.EquipmentId = eqpItem.EquipmentId;
                        spawnEquipmentRequested.FeastMovementid = eqpItem.FeastMovementid;
                        spawnEquipmentRequested.Location = eqpItem.Location;
                        spawnEquipmentRequested.Manufacturer = eqpItem.Manufacturer;
                        spawnEquipmentRequested.Model = eqpItem.Model;
                        spawnEquipmentRequested.Name = eqpItem.Name;
                        spawnEquipmentRequested.QualityIssue = eqpItem.QualityIssue;
                        spawnEquipmentRequested.SerialNumber = eqpItem.SerialNumber;
                        spawnEquipmentRequested.WorkorderID = eqpItem.WorkorderID;
                        spawnEquipmentRequested.Temperature = "";
                        spawnEquipmentRequested.Weight = "";
                        spawnEquipmentRequested.Ratio = "";
                        spawnEquipmentRequested.Settings = "";
                        spawnEquipmentRequested.WorkPerformedCounter = "";
                        spawnEquipmentRequested.WorkDescription = "";
                        spawnEquipmentRequested.Systemid = eqpItem.Systemid;
                        spawnEquipmentRequested.Symptomid = eqpItem.Symptomid;
                        spawnEquipmentRequested.Email = "";
                        spawnEquipmentRequested.NoPartsNeeded = null;
                        spawnEquipmentRequested.Solutionid = null;

                        if (soluitonId == 5160)
                        {
                            spawnEquipmentRequested.CallTypeid = 1310;
                        }

                        IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
                        assetCounter.IndexValue++;                        
                        spawnEquipmentRequested.Assetid = assetCounter.IndexValue.Value;
                        spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested);


                        if (soluitonId == 5160)
                        {
                            WorkorderEquipment spawnEquipment2 = new WorkorderEquipment();
                            WorkorderEquipmentRequested spawnEquipmentRequested2 = new WorkorderEquipmentRequested();
                            WorkorderEquipmentRequested workOrderReq2 = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();
                            spawnEquipment2.Assetid = eqpItem.Assetid;
                            spawnEquipment2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipment2.CatalogID = eqpItem.CatalogID;
                            spawnEquipment2.Category = eqpItem.Category;
                            spawnEquipment2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipment2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipment2.IsSlNumberImageExist = eqpItem.IsSlNumberImageExist;
                            spawnEquipment2.Location = eqpItem.Location;
                            spawnEquipment2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipment2.Model = eqpItem.Model;
                            spawnEquipment2.Name = eqpItem.Name;
                            spawnEquipment2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipment2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipment2.CallTypeid = 1410;
                            spawnEquipment2.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipment2.Temperature = "";
                            spawnEquipment2.Weight = "";
                            spawnEquipment2.Ratio = "";
                            spawnEquipment2.Settings = "";
                            spawnEquipment2.WorkPerformedCounter = "";
                            spawnEquipment2.WorkDescription = "";
                            spawnEquipment2.Systemid = eqpItem.Systemid;
                            spawnEquipment2.Symptomid = eqpItem.Symptomid;
                            spawnEquipment2.Email = "";
                            spawnEquipment2.NoPartsNeeded = null;
                            spawnEquipment2.Solutionid = null;
                                                        

                            spawnEquipmentRequested2.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipmentRequested2.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested2.Category = eqpItem.Category;
                            spawnEquipmentRequested2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested2.Location = eqpItem.Location;
                            spawnEquipmentRequested2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested2.Model = eqpItem.Model;
                            spawnEquipmentRequested2.Name = eqpItem.Name;
                            spawnEquipmentRequested2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested2.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested2.CallTypeid = 1410;
                            spawnEquipmentRequested2.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested2.Temperature = "";
                            spawnEquipmentRequested2.Weight = "";
                            spawnEquipmentRequested2.Ratio = "";
                            spawnEquipmentRequested2.Settings = "";
                            spawnEquipmentRequested2.WorkPerformedCounter = "";
                            spawnEquipmentRequested2.WorkDescription = "";
                            spawnEquipmentRequested2.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested2.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested2.Email = "";
                            spawnEquipmentRequested2.NoPartsNeeded = null;
                            spawnEquipmentRequested2.Solutionid = null;

                            IndexCounter assetCounter2 = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter2.IndexValue++;
                            
                            spawnEquipment2.Assetid = assetCounter2.IndexValue.Value;
                            spawnEquipmentRequested2.Assetid = assetCounter2.IndexValue.Value;

                            spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment2);
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested2);
                        }
                        else if (soluitonId == 5140 || soluitonId == 5170 || soluitonId == 5171 || soluitonId == 5181 || soluitonId == 5191 || 
                                    soluitonId == 5115 || soluitonId == 5120 || soluitonId == 5130 || soluitonId == 5135)
                        {
                            int calltypeId = 0;
                            switch(soluitonId)
                            {
                                case 5140:
                                    calltypeId = 1210;
                                    break;
                                case 5170:
                                case 5171:
                                case 5181:
                                case 5191:
                                    calltypeId = 1220;
                                    break;
                                case 5115:
                                case 5120:
                                case 5130:
                                case 5135:
                                    calltypeId = 1310;
                                    break;
                            }


                            WorkorderEquipment spawnEquipment3 = new WorkorderEquipment();
                            WorkorderEquipmentRequested spawnEquipmentRequested3 = new WorkorderEquipmentRequested();
                            WorkorderEquipmentRequested workOrderReq3 = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();
                            spawnEquipment3.Assetid = eqpItem.Assetid;
                            spawnEquipment3.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipment3.CatalogID = eqpItem.CatalogID;
                            spawnEquipment3.Category = eqpItem.Category;
                            spawnEquipment3.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipment3.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipment3.IsSlNumberImageExist = eqpItem.IsSlNumberImageExist;
                            spawnEquipment3.Location = eqpItem.Location;
                            spawnEquipment3.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipment3.Model = eqpItem.Model;
                            spawnEquipment3.Name = eqpItem.Name;
                            spawnEquipment3.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipment3.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipment3.CallTypeid = calltypeId;
                            spawnEquipment3.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipment3.Temperature = "";
                            spawnEquipment3.Weight = "";
                            spawnEquipment3.Ratio = "";
                            spawnEquipment3.Settings = "";
                            spawnEquipment3.WorkPerformedCounter = "";
                            spawnEquipment3.WorkDescription = "";
                            spawnEquipment3.Systemid = eqpItem.Systemid;
                            spawnEquipment3.Symptomid = eqpItem.Symptomid;
                            spawnEquipment3.Email = "";
                            spawnEquipment3.NoPartsNeeded = null;
                            spawnEquipment3.Solutionid = null;


                            spawnEquipmentRequested3.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested3.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipmentRequested3.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested3.Category = eqpItem.Category;
                            spawnEquipmentRequested3.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested3.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested3.Location = eqpItem.Location;
                            spawnEquipmentRequested3.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested3.Model = eqpItem.Model;
                            spawnEquipmentRequested3.Name = eqpItem.Name;
                            spawnEquipmentRequested3.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested3.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested3.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested3.CallTypeid = calltypeId;
                            spawnEquipmentRequested3.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested3.Temperature = "";
                            spawnEquipmentRequested3.Weight = "";
                            spawnEquipmentRequested3.Ratio = "";
                            spawnEquipmentRequested3.Settings = "";
                            spawnEquipmentRequested3.WorkPerformedCounter = "";
                            spawnEquipmentRequested3.WorkDescription = "";
                            spawnEquipmentRequested3.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested3.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested3.Email = "";
                            spawnEquipmentRequested3.NoPartsNeeded = null;
                            spawnEquipmentRequested3.Solutionid = null;

                            IndexCounter assetCounter3 = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter3.IndexValue++;

                            spawnEquipment3.Assetid = assetCounter3.IndexValue.Value;
                            spawnEquipmentRequested3.Assetid = assetCounter3.IndexValue.Value;

                            spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment3);
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested3);
                        }
                    }

                    newEntity.WorkOrders.Add(spawnWorkOrder);
                    newEntity.SaveChanges();

                    if (newEntity != null)
                    {
                        newEntity.Dispose();
                    }

                    string emailAddresses = string.Empty;


                    StringBuilder subject = new StringBuilder();
                    subject.Append("Spawned Workorder - Original WO: ");
                    subject.Append(spawnWorkOrder.OriginalWorkorderid);
                    subject.Append(" ST: ");
                    subject.Append(spawnWorkOrder.CustomerState);
                    subject.Append(" Call Type: ");
                    subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);

                    SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null);

                    if (responsibleTechId.HasValue)
                    {
                        subject = new StringBuilder();
                        subject.Append("WO:");
                        subject.Append(spawnWorkOrder.WorkorderID);
                        subject.Append(" ST:");
                        subject.Append(spawnWorkOrder.CustomerState);
                        subject.Append(" Call Type:");
                        subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);


                        string emailAddress = string.Empty;
                        string salesEmailAddress = string.Empty;
                        var CustomerId = int.Parse(responsibleTechId.Value.ToString());
                        Customer serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                        if (serviceCustomer != null)
                        {
                            emailAddress = serviceCustomer.Email;
                            if (!string.IsNullOrEmpty(serviceCustomer.SalesEmail))
                            {
                                salesEmailAddress = serviceCustomer.SalesEmail;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TestEmail"]))
                        {
                            emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                        }

                        if (!string.IsNullOrWhiteSpace(emailAddress))
                        {
                            SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], responsibleTechId, MailType.SPAWN, true, null, string.Empty, false, salesEmailAddress);
                        }
                    }

                    if (string.IsNullOrEmpty(SpawnedWOsCreated))
                    {
                        SpawnedWOsCreated += spawnWorkOrder.WorkorderID;
                    }
                    else
                    {
                        SpawnedWOsCreated += ", " + spawnWorkOrder.WorkorderID;
                    }
                    //message = @"Spawned Work Order " + spawnWorkOrder.WorkorderID + " is created!";
                }
            }

            if (!string.IsNullOrEmpty(SpawnedWOsCreated))
            {
                NotesHistory WONotesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = @"Spawned Work Order " + SpawnedWOsCreated + " created in MARS ",
                    Userid = 1234, //TBD
                    UserName = UserName,
                    isDispatchNotes = 0
                };
                //workOrder.NotesHistories.Add(WONotesHistory);
                //FarmerBrothersEntitites.SaveChanges();

                message = @"Spawned Work Order " + SpawnedWOsCreated + " created!";
            }
            else
            {
                message = @"";
            }
        }

        public void CreateSpawnWorkOrder(WorkorderManagementModel workorderManagement, List<WorkorderEquipment> equipment, out string message)
        {
            List<int?> uniqueSolutionIds = workorderManagement.WorkOrder.WorkorderEquipments.Select(x => x.Solutionid).Distinct().ToList();

            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);
            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            string SpawnedWOsCreated = "";
            foreach (int soluitonId in uniqueSolutionIds)
            {
                if (soluitonId == 5115
                 || soluitonId == 5120
                 || soluitonId == 5130
                 || soluitonId == 5135
                 || soluitonId == 5140
                 || soluitonId == 5170
                 || soluitonId == 5171
                 || soluitonId == 5181
                 || soluitonId == 5191)
                {

                    List<WorkorderEquipment> workorderEqps = workorderManagement.WorkOrder.WorkorderEquipments.Where(eq => eq.Solutionid == soluitonId).ToList();

                    WorkOrder spawnWorkOrder = new WorkOrder();
                    List<Type> collections = new List<Type>() { typeof(IEnumerable<>), typeof(IEnumerable) };

                    int? responsibleTechId = null;

                    foreach (var property in workOrder.GetType().GetProperties())
                    {
                        if (property.PropertyType == typeof(string) || !property.PropertyType.GetInterfaces().Any(i => collections.Any(c => i == c)))
                        {
                            property.SetValue(spawnWorkOrder, property.GetValue(workOrder));
                        }
                    }

                    FarmerBrothersEntities newEntity = new FarmerBrothersEntities();

                    IndexCounter workOrderCounter = Utility.GetIndexCounter("WorkorderID", 1);
                    workOrderCounter.IndexValue++;

                    spawnWorkOrder.WorkorderID = workOrderCounter.IndexValue.Value;
                    spawnWorkOrder.WorkorderEntryDate = currentTime;
                    spawnWorkOrder.WorkorderCallstatus = "Open";
                    spawnWorkOrder.WorkorderSpawnEvent = 1;
                    spawnWorkOrder.WorkorderCloseDate = null;
                    spawnWorkOrder.CustomerPO = workOrder.CustomerPO;
                    if (workOrder.OriginalWorkorderid.HasValue)
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.OriginalWorkorderid;
                    }
                    else
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.WorkorderID;
                    }

                    spawnWorkOrder.ParentWorkorderid = workOrder.WorkorderID;
                    if (workOrder.SpawnCounter.HasValue)
                    {
                        spawnWorkOrder.SpawnCounter = workOrder.SpawnCounter.Value + 1;
                    }
                    else
                    {
                        spawnWorkOrder.SpawnCounter = 1;
                    }

                    WorkorderDetail spawnWorkOrderDetail = new WorkorderDetail();
                    if (workOrder.WorkorderDetails.Count > 0)
                    {
                        WorkorderDetail workOrderDetail = workOrder.WorkorderDetails.ElementAt(0);
                        foreach (var property in workOrderDetail.GetType().GetProperties())
                        {
                            if (property.GetValue(workOrderDetail) != null && property.GetValue(workOrderDetail).GetType() != null && (property.GetValue(workOrderDetail).GetType().IsValueType || property.GetValue(workOrderDetail).GetType() == typeof(string)))
                            {
                                property.SetValue(spawnWorkOrderDetail, property.GetValue(workOrderDetail));
                            }
                        }
                        spawnWorkOrderDetail.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrderDetail.ArrivalDateTime = null;
                        spawnWorkOrderDetail.CompletionDateTime = null;
                        spawnWorkOrderDetail.StartDateTime = null;
                        spawnWorkOrderDetail.EntryDate = null;
                        spawnWorkOrderDetail.ModifiedDate = null;
                        spawnWorkOrderDetail.SpecialClosure = "";
                        spawnWorkOrderDetail.TravelTime = "";
                        spawnWorkOrderDetail.InvoiceNo = "";
                        spawnWorkOrderDetail.SolutionId = soluitonId;

                        spawnWorkOrder.WorkorderDetails.Add(spawnWorkOrderDetail);
                    }


                    foreach (WorkOrderBrand brand in workOrder.WorkOrderBrands)
                    {
                        WorkOrderBrand newBrand = new WorkOrderBrand();
                        foreach (var property in brand.GetType().GetProperties())
                        {
                            if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                            {
                                property.SetValue(newBrand, property.GetValue(brand));
                            }
                        }
                        newBrand.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkOrderBrands.Add(newBrand);
                    }

                    foreach (NotesHistory notes in workOrder.NotesHistories)
                    {
                        NotesHistory newNotes = new NotesHistory();
                        foreach (var property in notes.GetType().GetProperties())
                        {
                            if (property.GetValue(notes) != null && property.GetValue(notes).GetType() != null && (property.GetValue(notes).GetType().IsValueType || property.GetValue(notes).GetType() == typeof(string)))
                            {
                                property.SetValue(newNotes, property.GetValue(notes));
                            }
                        }
                        newNotes.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.NotesHistories.Add(newNotes);
                    }

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Work Order spawned in MARS from work order " + workOrder.WorkorderID,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 0
                    };
                    spawnWorkOrder.NotesHistories.Add(notesHistory);

                    NotesHistory WONotesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Workorder " + spawnWorkOrder.WorkorderID + " spawned due to Solution Code " + soluitonId,
                        Userid = 1234,
                        UserName = UserName,
                        isDispatchNotes = 0
                    };
                    workOrder.NotesHistories.Add(WONotesHistory);

                    foreach (WorkorderReasonlog reasonLog in workOrder.WorkorderReasonlogs)
                    {
                        WorkorderReasonlog newReasonLog = new WorkorderReasonlog();
                        foreach (var property in reasonLog.GetType().GetProperties())
                        {
                            if (property.GetValue(reasonLog) != null && property.GetValue(reasonLog).GetType() != null && (property.GetValue(reasonLog).GetType().IsValueType || property.GetValue(reasonLog).GetType() == typeof(string)))
                            {
                                property.SetValue(newReasonLog, property.GetValue(reasonLog));
                            }
                        }
                        newReasonLog.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkorderReasonlogs.Add(newReasonLog);
                    }

                    /*WorkorderType newWorkOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == 1310).FirstOrDefault();

                    if (soluitonId == 5160 && newWorkOrderType != null)
                    {
                        spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                        spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;
                    }
                    else
                    {
                        WorkorderEquipment eqp = workorderEqps.Where(e => e.CallTypeid == 1200).FirstOrDefault();
                        if (eqp != null)
                        {
                            WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                            if (workOrderType != null)
                            {
                                spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                            }
                        }
                        else
                        {
                            eqp = workorderEqps.OrderBy(equip => equip.Assetid).ElementAt(0);
                            if (eqp != null)
                            {
                                WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                                if (workOrderType != null)
                                {
                                    spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                    spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                }
                            }
                        }

                    }*/

                    if (soluitonId == 5160 || soluitonId == 5191)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.SpawnReason))
                        {
                            spawnWorkOrderDetail.SpawnReason = Convert.ToInt32(workorderManagement.SpawnReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.SpawnNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 0
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }

                    if (soluitonId == 9999)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.NSRReason))
                        {
                            spawnWorkOrderDetail.NSRReason = Convert.ToInt32(workorderManagement.NSRReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.NSRNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 1
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }

                    foreach (WorkorderEquipment eqpItem in workorderEqps)
                    {

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "SpawnedEquipment : SerialNumber - " + eqpItem.SerialNumber + ", Description - " + eqpItem.WorkDescription,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 0
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);

                        WorkorderEquipmentRequested spawnEquipmentRequested = new WorkorderEquipmentRequested();
                       // WorkorderEquipmentRequested workOrderReq = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();
                        if (soluitonId == 5160)
                        {
                            WorkorderType newWorkOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == 1310).FirstOrDefault();
                            spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                            spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;


                            spawnEquipmentRequested.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested.CallTypeid = 1310;
                            spawnEquipmentRequested.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested.Category = eqpItem.Category;
                            spawnEquipmentRequested.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested.Location = eqpItem.Location;
                            spawnEquipmentRequested.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested.Model = eqpItem.Model;
                            spawnEquipmentRequested.Name = eqpItem.Name;
                            spawnEquipmentRequested.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested.Temperature = "";
                            spawnEquipmentRequested.Weight = "";
                            spawnEquipmentRequested.Ratio = "";
                            spawnEquipmentRequested.Settings = "";
                            spawnEquipmentRequested.WorkPerformedCounter = "";
                            spawnEquipmentRequested.WorkDescription = "";
                            spawnEquipmentRequested.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested.Email = "";
                            spawnEquipmentRequested.NoPartsNeeded = null;
                            spawnEquipmentRequested.Solutionid = null;

                            IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter.IndexValue++;
                            spawnEquipmentRequested.Assetid = assetCounter.IndexValue.Value;
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested);



                            WorkorderEquipment spawnEquipment2 = new WorkorderEquipment();
                            WorkorderEquipmentRequested spawnEquipmentRequested2 = new WorkorderEquipmentRequested();
                            WorkorderEquipmentRequested workOrderReq2 = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();
                            spawnEquipment2.Assetid = eqpItem.Assetid;
                            spawnEquipment2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipment2.CatalogID = eqpItem.CatalogID;
                            spawnEquipment2.Category = eqpItem.Category;
                            spawnEquipment2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipment2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipment2.IsSlNumberImageExist = eqpItem.IsSlNumberImageExist;
                            spawnEquipment2.Location = eqpItem.Location;
                            spawnEquipment2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipment2.Model = eqpItem.Model;
                            spawnEquipment2.Name = eqpItem.Name;
                            spawnEquipment2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipment2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipment2.CallTypeid = 1410;
                            spawnEquipment2.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipment2.Temperature = "";
                            spawnEquipment2.Weight = "";
                            spawnEquipment2.Ratio = "";
                            spawnEquipment2.Settings = "";
                            spawnEquipment2.WorkPerformedCounter = "";
                            spawnEquipment2.WorkDescription = "";
                            spawnEquipment2.Systemid = eqpItem.Systemid;
                            spawnEquipment2.Symptomid = eqpItem.Symptomid;
                            spawnEquipment2.Email = "";
                            spawnEquipment2.NoPartsNeeded = null;
                            spawnEquipment2.Solutionid = null;


                            spawnEquipmentRequested2.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipmentRequested2.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested2.Category = eqpItem.Category;
                            spawnEquipmentRequested2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested2.Location = eqpItem.Location;
                            spawnEquipmentRequested2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested2.Model = eqpItem.Model;
                            spawnEquipmentRequested2.Name = eqpItem.Name;
                            spawnEquipmentRequested2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested2.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested2.CallTypeid = 1410;
                            spawnEquipmentRequested2.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested2.Temperature = "";
                            spawnEquipmentRequested2.Weight = "";
                            spawnEquipmentRequested2.Ratio = "";
                            spawnEquipmentRequested2.Settings = "";
                            spawnEquipmentRequested2.WorkPerformedCounter = "";
                            spawnEquipmentRequested2.WorkDescription = "";
                            spawnEquipmentRequested2.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested2.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested2.Email = "";
                            spawnEquipmentRequested2.NoPartsNeeded = null;
                            spawnEquipmentRequested2.Solutionid = null;

                            IndexCounter assetCounter2 = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter2.IndexValue++;

                            spawnEquipment2.Assetid = assetCounter2.IndexValue.Value;
                            spawnEquipmentRequested2.Assetid = assetCounter2.IndexValue.Value;

                            spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment2);
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested2);
                        }
                        else if (soluitonId == 5140 || soluitonId == 5170 || soluitonId == 5171 || soluitonId == 5181 || soluitonId == 5191 ||
                                    soluitonId == 5115 || soluitonId == 5120 || soluitonId == 5130 || soluitonId == 5135)
                        {
                            int calltypeId = 0;
                            switch (soluitonId)
                            {
                                case 5140:
                                    calltypeId = 1210;
                                    break;
                                case 5170:
                                case 5171:
                                case 5181:
                                case 5191:
                                    calltypeId = 1220;
                                    break;
                                case 5115:
                                case 5120:
                                case 5130:
                                case 5135:
                                    calltypeId = 1310;
                                    break;
                            }

                            WorkorderType newWorkOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == calltypeId).FirstOrDefault();
                            spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                            spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;

                            WorkorderEquipment spawnEquipment3 = new WorkorderEquipment();
                            WorkorderEquipmentRequested spawnEquipmentRequested3 = new WorkorderEquipmentRequested();
                            WorkorderEquipmentRequested workOrderReq3 = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();
                            spawnEquipment3.Assetid = eqpItem.Assetid;
                            spawnEquipment3.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipment3.CatalogID = eqpItem.CatalogID;
                            spawnEquipment3.Category = eqpItem.Category;
                            spawnEquipment3.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipment3.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipment3.IsSlNumberImageExist = eqpItem.IsSlNumberImageExist;
                            spawnEquipment3.Location = eqpItem.Location;
                            spawnEquipment3.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipment3.Model = eqpItem.Model;
                            spawnEquipment3.Name = eqpItem.Name;
                            spawnEquipment3.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipment3.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipment3.CallTypeid = calltypeId;
                            spawnEquipment3.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipment3.Temperature = "";
                            spawnEquipment3.Weight = "";
                            spawnEquipment3.Ratio = "";
                            spawnEquipment3.Settings = "";
                            spawnEquipment3.WorkPerformedCounter = "";
                            spawnEquipment3.WorkDescription = "";
                            spawnEquipment3.Systemid = eqpItem.Systemid;
                            spawnEquipment3.Symptomid = eqpItem.Symptomid;
                            spawnEquipment3.Email = "";
                            spawnEquipment3.NoPartsNeeded = null;
                            spawnEquipment3.Solutionid = null;


                            spawnEquipmentRequested3.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested3.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipmentRequested3.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested3.Category = eqpItem.Category;
                            spawnEquipmentRequested3.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested3.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested3.Location = eqpItem.Location;
                            spawnEquipmentRequested3.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested3.Model = eqpItem.Model;
                            spawnEquipmentRequested3.Name = eqpItem.Name;
                            spawnEquipmentRequested3.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested3.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested3.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested3.CallTypeid = calltypeId;
                            spawnEquipmentRequested3.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested3.Temperature = "";
                            spawnEquipmentRequested3.Weight = "";
                            spawnEquipmentRequested3.Ratio = "";
                            spawnEquipmentRequested3.Settings = "";
                            spawnEquipmentRequested3.WorkPerformedCounter = "";
                            spawnEquipmentRequested3.WorkDescription = "";
                            spawnEquipmentRequested3.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested3.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested3.Email = "";
                            spawnEquipmentRequested3.NoPartsNeeded = null;
                            spawnEquipmentRequested3.Solutionid = null;

                            IndexCounter assetCounter3 = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter3.IndexValue++;

                            spawnEquipment3.Assetid = assetCounter3.IndexValue.Value;
                            spawnEquipmentRequested3.Assetid = assetCounter3.IndexValue.Value;

                            spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment3);
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested3);
                        }
                    }

                    newEntity.WorkOrders.Add(spawnWorkOrder);
                    newEntity.SaveChanges();

                    if (newEntity != null)
                    {
                        newEntity.Dispose();
                    }

                    string emailAddresses = string.Empty;


                    StringBuilder subject = new StringBuilder();
                    subject.Append("Spawned Workorder - Original WO: ");
                    subject.Append(spawnWorkOrder.OriginalWorkorderid);
                    subject.Append(" ST: ");
                    subject.Append(spawnWorkOrder.CustomerState);
                    subject.Append(" Call Type: ");
                    subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);

                    SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null);

                    if (responsibleTechId.HasValue)
                    {
                        subject = new StringBuilder();
                        subject.Append("WO:");
                        subject.Append(spawnWorkOrder.WorkorderID);
                        subject.Append(" ST:");
                        subject.Append(spawnWorkOrder.CustomerState);
                        subject.Append(" Call Type:");
                        subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);


                        string emailAddress = string.Empty;
                        string salesEmailAddress = string.Empty;
                        var CustomerId = int.Parse(responsibleTechId.Value.ToString());
                        Customer serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                        if (serviceCustomer != null)
                        {
                            emailAddress = serviceCustomer.Email;
                            if (!string.IsNullOrEmpty(serviceCustomer.SalesEmail))
                            {
                                salesEmailAddress = serviceCustomer.SalesEmail;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TestEmail"]))
                        {
                            emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                        }

                        if (!string.IsNullOrWhiteSpace(emailAddress))
                        {
                            SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], responsibleTechId, MailType.SPAWN, true, null, string.Empty, false, salesEmailAddress);
                        }
                    }

                    if (string.IsNullOrEmpty(SpawnedWOsCreated))
                    {
                        SpawnedWOsCreated += spawnWorkOrder.WorkorderID;
                    }
                    else
                    {
                        SpawnedWOsCreated += ", " + spawnWorkOrder.WorkorderID;
                    }
                    //message = @"Spawned Work Order " + spawnWorkOrder.WorkorderID + " is created!";
                }
            }

            if (!string.IsNullOrEmpty(SpawnedWOsCreated))
            {
                NotesHistory WONotesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = @"Spawned Work Order " + SpawnedWOsCreated + " created in MARS ",
                    Userid = 1234, //TBD
                    UserName = UserName,
                    isDispatchNotes = 0
                };
                //workOrder.NotesHistories.Add(WONotesHistory);
                //FarmerBrothersEntitites.SaveChanges();

                message = @"Spawned Work Order " + SpawnedWOsCreated + " created!";
            }
            else
            {
                message = @"";
            }
        }

        public void SaveClosureDetails(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            if (workorderManagement.Closure != null)
            {
                string specialClosure = string.Empty;

                if (!string.IsNullOrWhiteSpace(workorderManagement.Closure.SpecialClosure))
                {
                    string[] specialClosureList = workorderManagement.Closure.SpecialClosure.Split(',');
                    if (specialClosureList.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(specialClosureList[0]))
                        {
                            specialClosure = specialClosureList[0];
                        }
                    }
                }

                workorderManagement.Closure.SpecialClosure = specialClosure;

                WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0 && ws.AssignedStatus == "Accepted").FirstOrDefault();

                DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);

                WorkorderDetail workOrderDetail = FarmerBrothersEntitites.WorkorderDetails.Where(wd => wd.WorkorderID == workOrder.WorkorderID).FirstOrDefault();
                if (workOrderDetail != null)
                {
                    workOrderDetail.ArrivalDateTime = workorderManagement.Closure.ArrivalDateTime;
                    workOrderDetail.StartDateTime = workorderManagement.Closure.StartDateTime;
                    if (workOrderDetail.ArrivalDateTime.HasValue && workOrderDetail.ArrivalDateTime.Value != DateTime.MinValue)
                    {
                        WorkorderStatusLog statusLog = new WorkorderStatusLog() { StatusFrom = workOrder.WorkorderCallstatus, StatusTo = "On Site", StausChangeDate = currentTime, WorkorderID = workOrder.WorkorderID };
                        workOrder.WorkorderStatusLogs.Add(statusLog);
                        workOrder.WorkorderCallstatus = "On Site";
                    }

                    workOrderDetail.CompletionDateTime = workorderManagement.Closure.CompletionDateTime;
                    if (workOrderDetail.CompletionDateTime.HasValue && workOrderDetail.CompletionDateTime.Value != DateTime.MinValue)
                    {
                        WorkorderStatusLog statusLog = new WorkorderStatusLog() { StatusFrom = workOrder.WorkorderCallstatus, StatusTo = "Completed", StausChangeDate = currentTime, WorkorderID = workOrder.WorkorderID };
                        workOrder.WorkorderStatusLogs.Add(statusLog);
                        workOrder.WorkorderCallstatus = "Completed";
                        if (workOrder.FollowupCallID == 681)
                        {
                            workOrder.WorkorderCallstatus = "Open";
                        }
                    }

                    workOrderDetail.InvoiceNo = workorderManagement.Closure.InvoiceNo;
                    workOrderDetail.ResponsibleTechName = workorderManagement.Closure.ResponsibleTechName;
                    workOrderDetail.Mileage = workorderManagement.Closure.Mileage;
                    workOrderDetail.CustomerName = workorderManagement.Closure.CustomerName;
                    workOrderDetail.CustomerEmail = workorderManagement.Closure.CustomerEmail;
                    workOrderDetail.CustomerSignatureDetails = workorderManagement.Closure.CustomerSignatureDetails;
                    workOrderDetail.TechnicianSignatureDetails = workorderManagement.Closure.TechnicianSignatureDetails;
                    workOrderDetail.WorkorderID = workOrder.WorkorderID;
                    workOrderDetail.InvoiceDate = DateTime.UtcNow;
                    workOrderDetail.ModifiedDate = currentTime;
                    workOrderDetail.CustomerSignatureBy = workorderManagement.Closure.CustomerSignedBy;

                    workOrderDetail.SpecialClosure = specialClosure;
                    workOrderDetail.TravelTime = workorderManagement.Closure.TravelHours + ":" + workorderManagement.Closure.TravelMinutes;

                    workOrderDetail.WaterTested = workorderManagement.Closure.WaterTested;
                    workOrderDetail.HardnessRating = workorderManagement.Closure.HardnessRating;
                    workOrderDetail.TotalDissolvedSolids = workorderManagement.Closure.TDS;

                    workOrderDetail.StateofEquipment = workorderManagement.Closure.StateOfEquipment;
                    workOrderDetail.ServiceDelayReason = workorderManagement.Closure.serviceDelayed;
                    workOrderDetail.TroubleshootSteps = workorderManagement.Closure.troubleshootSteps;
                    workOrderDetail.FollowupComments = workorderManagement.Closure.followupComments;
                    //workOrderDetail.OperationalComments = workorderManagement.Closure.operationalComments;
                    workOrderDetail.ReviewedBy = workorderManagement.Closure.ReviewedBy;

                    workOrderDetail.IsUnderWarrenty = workorderManagement.Closure.IsUnderWarrenty;
                    workOrderDetail.WarrentyFor = workorderManagement.Closure.WarrentyFor;
                    workOrderDetail.AdditionalFollowupReq = workorderManagement.Closure.AdditionalFollowup;
                    workOrderDetail.IsOperational = workorderManagement.Closure.Operational;


                    if (schedule != null)
                    {
                        workOrderDetail.ResponsibleTechid = schedule.Techid;
                    }

                    if (workOrderDetail.CustomerSignatureDetails != null)
                    {
                        //890 is for empty signature box
                        if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                        {
                            workOrderDetail.CustomerSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                                Select(s => s.CustomerSignatureDetails).FirstOrDefault();
                            if (workOrderDetail.CustomerSignatureDetails != null)
                            {
                                if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                                {
                                    workOrderDetail.CustomerSignatureDetails = string.Empty;
                                }
                            }
                            else
                            {
                                workOrderDetail.CustomerSignatureDetails = string.Empty;
                            }
                        }
                    }

                    if (workOrderDetail.TechnicianSignatureDetails != null)
                    {
                        //890 is for empty signature box
                        if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                        {
                            workOrderDetail.TechnicianSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                                Select(s => s.TechnicianSignatureDetails).FirstOrDefault();
                            if (workOrderDetail.TechnicianSignatureDetails != null)
                            {
                                if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                                {
                                    workOrderDetail.TechnicianSignatureDetails = string.Empty;
                                }
                            }
                            else
                            {
                                workOrderDetail.TechnicianSignatureDetails = string.Empty;
                            }
                        }
                    }

                }
                else
                {
                    workOrderDetail = new WorkorderDetail()
                    {
                        ArrivalDateTime = workorderManagement.Closure.ArrivalDateTime,
                        CompletionDateTime = workorderManagement.Closure.CompletionDateTime,
                        ResponsibleTechName = workorderManagement.Closure.ResponsibleTechName,
                        Mileage = workorderManagement.Closure.Mileage,
                        CustomerName = workorderManagement.Closure.CustomerName,
                        CustomerEmail = workorderManagement.Closure.CustomerEmail,
                        CustomerSignatureDetails = workorderManagement.Closure.CustomerSignatureDetails,
                        CustomerSignatureBy = workorderManagement.Closure.CustomerSignedBy,
                        TechnicianSignatureDetails = workorderManagement.Closure.TechnicianSignatureDetails,
                        WorkorderID = workOrder.WorkorderID,
                        InvoiceNo = workorderManagement.Closure.InvoiceNo,
                        StartDateTime = workorderManagement.Closure.StartDateTime,
                        EntryDate = currentTime,
                        ModifiedDate = currentTime,
                        SpecialClosure = specialClosure,
                        TravelTime = workorderManagement.Closure.TravelHours + ":" + workorderManagement.Closure.TravelMinutes,
                        WaterTested = workorderManagement.Closure.WaterTested,
                        HardnessRating = workorderManagement.Closure.HardnessRating,
                        TotalDissolvedSolids = workorderManagement.Closure.TDS
                    };


                    if (schedule != null)
                    {
                        workOrderDetail.ResponsibleTechid = schedule.Techid;
                    }

                    //890 is for empty signature box
                    if (workOrderDetail.CustomerSignatureDetails != null)
                    {
                        if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                        {
                            workOrderDetail.CustomerSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                                Select(s => s.CustomerSignatureDetails).FirstOrDefault();
                            if (workOrderDetail.CustomerSignatureDetails != null)
                            {
                                if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                                {
                                    workOrderDetail.CustomerSignatureDetails = string.Empty;
                                }
                            }
                            else
                            {
                                workOrderDetail.CustomerSignatureDetails = string.Empty;
                            }
                        }
                    }

                    //890 is for empty signature box
                    if (workOrderDetail.TechnicianSignatureDetails != null)
                    {
                        if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                        {
                            workOrderDetail.TechnicianSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                                Select(s => s.TechnicianSignatureDetails).FirstOrDefault();
                            if (workOrderDetail.TechnicianSignatureDetails != null)
                            {
                                if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                                {
                                    workOrderDetail.TechnicianSignatureDetails = string.Empty;
                                }
                            }
                            else
                            {
                                workOrderDetail.TechnicianSignatureDetails = string.Empty;
                            }
                        }
                    }

                    FarmerBrothersEntitites.WorkorderDetails.Add(workOrderDetail);
                }



                if (!string.IsNullOrWhiteSpace(specialClosure))
                {
                    TimeZoneInfo newTimeZoneInfo = null;
                    Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = "Work Order Closed from MARS by " + UserName,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 1
                    };
                    notesHistory.WorkorderID = workOrder.WorkorderID;
                    workOrder.NotesHistories.Add(notesHistory);

                    if (string.Compare(specialClosure, "No Service Required", true) == 0)
                    {
                        workOrder.WorkorderEquipments.Clear();
                        workOrder.NoServiceRequired = true;
                    }

                    if (string.Compare(specialClosure, "Cancellation", true) == 0)
                    {
                        workOrder.NoServiceRequired = true;
                    }

                    workOrder.WorkorderCallstatus = "Closed";
                    workOrder.ClosedUserName = UserName;
                    workOrder.WorkorderCloseDate = currentTime;
                }
            }
        }

        private void SaveWorkOrderParts(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            switch (workorderManagement.PartsShipTo)
            {
                case 1:
                    if (workorderManagement.IsBranchAlternateAddress)
                    {
                        workOrder.PartsShipTo = "Other Local Branch";

                        workOrder.OtherPartsName = workorderManagement.BranchOtherPartsName;
                        workOrder.OtherPartsContactName = workorderManagement.BranchOtherPartsContactName;
                        workOrder.OtherPartsAddress1 = workorderManagement.BranchOtherPartsAddress1;
                        workOrder.OtherPartsAddress2 = workorderManagement.BranchOtherPartsAddress2;
                        workOrder.OtherPartsCity = workorderManagement.BranchOtherPartsCity;
                        workOrder.OtherPartsState = workorderManagement.BranchOtherPartsState;
                        workOrder.OtherPartsZip = workorderManagement.BranchOtherPartsZip;
                        workOrder.OtherPartsPhone = workorderManagement.BranchOtherPartsPhone;
                    }
                    else
                    {
                        workOrder.PartsShipTo = "Local Branch";
                    }

                    break;
                case 2:
                    if (workorderManagement.IsCustomerAlternateAddress == true)
                    {
                        workOrder.PartsShipTo = "Other Customer";

                        workOrder.OtherPartsName = workorderManagement.CustomerOtherPartsName;
                        workOrder.OtherPartsContactName = workorderManagement.CustomerOtherPartsContactName;
                        workOrder.OtherPartsAddress1 = workorderManagement.CustomerOtherPartsAddress1;
                        workOrder.OtherPartsAddress2 = workorderManagement.CustomerOtherPartsAddress2;
                        workOrder.OtherPartsCity = workorderManagement.CustomerOtherPartsCity;
                        workOrder.OtherPartsState = workorderManagement.CustomerOtherPartsState;
                        workOrder.OtherPartsZip = workorderManagement.CustomerOtherPartsZip;

                        workOrder.OtherPartsPhone = Utilities.Utility.FormatPhoneNumber(workorderManagement.CustomerOtherPartsPhone);
                    }
                    else
                    {
                        workOrder.PartsShipTo = "Customer";
                    }
                    break;

            }

            IEnumerable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.WorkorderID == workOrder.WorkorderID);

            if (workOrderParts != null)
            {
                for (int count = workOrderParts.Count() - 1; count >= 0; count--)
                {
                    WorkorderPart workOrderPart = workOrderParts.ElementAt(count);

                    WorkOrderPartModel workOrderPartModel = workorderManagement.WorkOrderParts.Where(e => e.PartsIssueid == workOrderPart.PartsIssueid).FirstOrDefault();
                    if (workOrderPartModel != null)
                    {

                        FarmerBrothersEntitites.WorkorderParts.Remove(workOrderPart);
                    }
                }
            }

            IList<WorkOrderPartModel> newParts = workorderManagement.WorkOrderParts.Where(e => e.PartsIssueid < 100).ToList();
            foreach (WorkOrderPartModel newPart in newParts)
            {
                WorkorderPart part = new WorkorderPart()
                {
                    Quantity = newPart.Quantity,
                    Manufacturer = newPart.Manufacturer,
                    Sku = newPart.Sku,
                    Description = newPart.Description
                };

                workOrder.WorkorderParts.Add(part);
            }
        }

        private void SaveNonSerialized(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            IEnumerable<NonSerialized> nonSerializeds = FarmerBrothersEntitites.NonSerializeds.Where(ns => ns.WorkorderID == workOrder.WorkorderID);
            if (nonSerializeds != null)
            {
                for (int count = nonSerializeds.Count() - 1; count > 0; count--)
                {
                    NonSerialized nonSerialized = nonSerializeds.ElementAt(count);

                    WorkOrderManagementNonSerializedModel nonSerializedFromModel = workorderManagement.NonSerializedList.Where(n => n.NSerialid == nonSerialized.NSerialid).FirstOrDefault();
                    if (nonSerializedFromModel != null)
                    {
                        nonSerialized.ManufNumber = nonSerializedFromModel.ManufNumber;
                        nonSerialized.Catalogid = nonSerializedFromModel.Catalogid;
                        nonSerialized.OrigOrderQuantity = nonSerializedFromModel.OrigOrderQuantity;
                    }
                    else
                    {
                        FarmerBrothersEntitites.NonSerializeds.Remove(nonSerialized);
                    }
                }
            }

            IList<WorkOrderManagementNonSerializedModel> newNonSerialized = workorderManagement.NonSerializedList.Where(e => e.NSerialid < 1000).ToList();
            foreach (WorkOrderManagementNonSerializedModel newNonSerial in newNonSerialized)
            {
                NonSerialized newNonSerialDb = new NonSerialized()
                {
                    ManufNumber = newNonSerial.ManufNumber,
                    Catalogid = newNonSerial.Catalogid,
                    OrigOrderQuantity = newNonSerial.OrigOrderQuantity
                };

                workOrder.NonSerializeds.Add(newNonSerialDb);
            }
        }

        public void SaveWorkOrderEquipments(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            IEnumerable<WorkorderEquipmentRequested> workOrderEquipments = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(we => we.WorkorderID == workOrder.WorkorderID);

            if (workOrderEquipments != null)
            {
                for (int count = workOrderEquipments.Count() - 1; count >= 0; count--)
                {
                    WorkorderEquipmentRequested equipment = workOrderEquipments.ElementAt(count);

                    WorkOrderManagementEquipmentModel equipmentFromModel = workorderManagement.WorkOrderEquipmentsRequested.Where(e => e.AssetId == equipment.Assetid).FirstOrDefault();
                    if (equipmentFromModel != null)
                    {
                        equipment.CallTypeid = equipmentFromModel.CallTypeID;
                        equipment.Category = equipmentFromModel.Category;
                        equipment.Location = equipmentFromModel.Location;
                        equipment.SerialNumber = equipmentFromModel.SerialNumber;
                        equipment.Model = equipmentFromModel.Model;
                        equipment.CatalogID = equipmentFromModel.CatelogID;
                        equipment.Symptomid = equipmentFromModel.SymptomID;
                    }
                    else
                    {
                        FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Remove(equipment);
                    }
                }
            }

            IList<WorkOrderManagementEquipmentModel> newKnownEquipments = workorderManagement.WorkOrderEquipmentsRequested.Where(e => e.AssetId < 1000).ToList();
            if (newKnownEquipments.Count() > 0)
            {
                IndexCounter counter = Utility.GetIndexCounter("AssetID", newKnownEquipments.Count);
                foreach (WorkOrderManagementEquipmentModel newKnownEquipment in newKnownEquipments)
                {
                    counter.IndexValue++;

                    WorkorderEquipmentRequested equipment = new WorkorderEquipmentRequested()
                    {
                        Assetid = counter.IndexValue.Value,
                        CallTypeid = newKnownEquipment.CallTypeID,
                        Category = newKnownEquipment.Category,
                        Location = newKnownEquipment.Location,
                        SerialNumber = newKnownEquipment.SerialNumber,
                        Model = newKnownEquipment.Model,
                        CatalogID = newKnownEquipment.CatelogID,
                        Symptomid = newKnownEquipment.SymptomID
                    };
                    workOrder.WorkorderEquipmentRequesteds.Add(equipment);
                }
                //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
            }

        }

        public void SaveClosureAssets(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            IEnumerable<WorkorderEquipment> workOrderEquipments = FarmerBrothersEntitites.WorkorderEquipments.Where(we => we.WorkorderID == workOrder.WorkorderID);

            if (workOrderEquipments != null)
            {
                for (int count = workOrderEquipments.Count() - 1; count >= 0; count--)
                {
                    WorkorderEquipment equipment = workOrderEquipments.ElementAt(count);

                    WorkOrderManagementEquipmentModel equipmentFromModel = workorderManagement.Closure.WorkOrderEquipments.Where(e => e.AssetId == equipment.Assetid).FirstOrDefault();
                    if (equipmentFromModel != null)
                    {
                        equipment.WorkorderID = workOrder.WorkorderID;
                        equipment.CallTypeid = equipmentFromModel.CallTypeID;
                        equipment.Category = equipmentFromModel.Category;
                        equipment.Manufacturer = equipmentFromModel.Manufacturer;
                        equipment.Model = equipmentFromModel.Model;
                        equipment.Location = equipmentFromModel.Location;

                        if (equipmentFromModel.SerialNumber.ToLower() != "other")
                        {
                            equipment.SerialNumber = equipmentFromModel.SerialNumber;
                        }
                        else
                        {
                            equipment.SerialNumber = equipmentFromModel.SerialNumberManual;
                        }

                        equipment.Solutionid = equipmentFromModel.Solution;
                        equipment.FeastMovementid = equipmentFromModel.FeastMovementId;

                        if (equipment.CallTypeid == 1600)
                        {
                            if (equipment.WorkorderInstallationSurveys != null && equipment.WorkorderInstallationSurveys.Count > 0)
                            {
                                WorkorderInstallationSurvey survey = equipment.WorkorderInstallationSurveys.ElementAt(0);
                                if (survey != null)
                                {
                                    survey.AssetLocation = equipmentFromModel.AssetLocation;
                                    survey.Comments = equipmentFromModel.Comments;
                                    survey.CounterUnitSpace = equipmentFromModel.CounterUnitSpace;
                                    survey.ElectricalPhase = equipmentFromModel.ElectricalPhase;
                                    survey.MachineAmperage = equipmentFromModel.MachineAmperage;
                                    survey.NemwNumber = equipmentFromModel.NemwNumber;
                                    survey.UnitFitSpace = equipmentFromModel.UnitFitSpace;
                                    survey.Voltage = equipmentFromModel.Voltage;
                                    survey.WaterLine = equipmentFromModel.WaterLine;
                                    survey.WorkorderID = workOrder.WorkorderID;
                                }
                            }
                            else
                            {
                                WorkorderInstallationSurvey survey = new WorkorderInstallationSurvey()
                                {
                                    AssetLocation = equipmentFromModel.AssetLocation,
                                    Comments = equipmentFromModel.Comments,
                                    CounterUnitSpace = equipmentFromModel.CounterUnitSpace,
                                    ElectricalPhase = equipmentFromModel.ElectricalPhase,
                                    MachineAmperage = equipmentFromModel.MachineAmperage,
                                    NemwNumber = equipmentFromModel.NemwNumber,
                                    UnitFitSpace = equipmentFromModel.UnitFitSpace,
                                    Voltage = equipmentFromModel.Voltage,
                                    WaterLine = equipmentFromModel.WaterLine,
                                    WorkorderID = workOrder.WorkorderID,
                                    AssetID = equipmentFromModel.AssetId
                                };
                                equipment.WorkorderInstallationSurveys.Add(survey);
                            }

                            equipment.Temperature = "";
                            equipment.Weight = "";
                            equipment.Ratio = "";
                            equipment.Settings = "";
                            equipment.WorkPerformedCounter = "";
                            equipment.WorkDescription = "";
                            equipment.Systemid = null;
                            equipment.Symptomid = null;
                            equipment.Email = "";
                        }
                        else
                        {
                            if (equipment.WorkorderInstallationSurveys != null && equipment.WorkorderInstallationSurveys.Count > 0)
                            {
                                equipment.WorkorderInstallationSurveys.Clear();
                            }

                            equipment.Temperature = equipmentFromModel.Temperature;
                            equipment.Weight = equipmentFromModel.Weight;
                            equipment.Ratio = equipmentFromModel.Ratio;
                            equipment.Settings = equipmentFromModel.Settings;
                            equipment.WorkPerformedCounter = equipmentFromModel.Counter;
                            equipment.WorkDescription = equipmentFromModel.WorkPerformed;
                            equipment.Systemid = equipmentFromModel.System;
                            equipment.Symptomid = equipmentFromModel.SymptomID;
                            equipment.Solutionid = equipmentFromModel.Solution;
                            equipment.Email = equipmentFromModel.Email;
                            equipment.NoPartsNeeded = equipmentFromModel.NoPartsNeeded;
                        }
                        SaveClosureParts(equipmentFromModel, workOrder, equipment);
                    }
                    else
                    {
                        FarmerBrothersEntitites.WorkorderEquipments.Remove(equipment);
                    }
                }
            }
            
            IList<WorkOrderManagementEquipmentModel> newKnownEquipments = workorderManagement.Closure.WorkOrderEquipments.Where(e => e.AssetId < 1000).ToList();
            IndexCounter counter = Utility.GetIndexCounter("AssetID", newKnownEquipments.Count);
            foreach (WorkOrderManagementEquipmentModel newKnownEquipment in newKnownEquipments)
            {
                counter.IndexValue++;

                string SNO = "";
                if (!string.IsNullOrEmpty(newKnownEquipment.SerialNumber) && newKnownEquipment.SerialNumber.ToLower() != "other")
                {
                    SNO = newKnownEquipment.SerialNumber;
                }
                else
                {
                    SNO = newKnownEquipment.SerialNumberManual;
                }

                WorkorderEquipment equipment = new WorkorderEquipment()
                {
                    WorkorderID = workOrder.WorkorderID,
                    Assetid = counter.IndexValue.Value,
                    CallTypeid = newKnownEquipment.CallTypeID,
                    Category = newKnownEquipment.Category,
                    Manufacturer = newKnownEquipment.Manufacturer,
                    Model = newKnownEquipment.Model,
                    Location = newKnownEquipment.Location,
                    SerialNumber = SNO,
                    Solutionid = newKnownEquipment.Solution,
                    FeastMovementid = newKnownEquipment.FeastMovementId
                };

                if (equipment.CallTypeid == 1600)
                {
                    WorkorderInstallationSurvey survey = new WorkorderInstallationSurvey()
                    {
                        AssetLocation = newKnownEquipment.AssetLocation,
                        Comments = newKnownEquipment.Comments,
                        CounterUnitSpace = newKnownEquipment.CounterUnitSpace,
                        ElectricalPhase = newKnownEquipment.ElectricalPhase,
                        MachineAmperage = newKnownEquipment.MachineAmperage,
                        NemwNumber = newKnownEquipment.NemwNumber,
                        UnitFitSpace = newKnownEquipment.UnitFitSpace,
                        Voltage = newKnownEquipment.Voltage,
                        WaterLine = newKnownEquipment.WaterLine,
                        WorkorderID = workOrder.WorkorderID,
                        AssetID = newKnownEquipment.AssetId
                    };
                    equipment.WorkorderInstallationSurveys.Add(survey);
                }
                else
                {
                    equipment.Temperature = newKnownEquipment.Temperature;
                    equipment.Weight = newKnownEquipment.Weight;
                    equipment.Ratio = newKnownEquipment.Ratio;
                    equipment.Settings = newKnownEquipment.Settings;
                    equipment.WorkPerformedCounter = newKnownEquipment.Counter;
                    equipment.WorkDescription = newKnownEquipment.WorkPerformed;
                    equipment.Email = newKnownEquipment.Email;
                    equipment.Systemid = newKnownEquipment.System;
                    equipment.Symptomid = newKnownEquipment.SymptomID;
                    equipment.NoPartsNeeded = newKnownEquipment.NoPartsNeeded;
                }

                FarmerBrothersEntitites.WorkorderEquipments.Add(equipment);
                SaveClosureParts(newKnownEquipment, workOrder, equipment);
            }

            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
        }

        private void SaveClosureParts(WorkOrderManagementEquipmentModel equipmentFromModel, WorkOrder workOrder, WorkorderEquipment equipment)
        {
            IEnumerable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.AssetID == equipment.Assetid);
            for (int partCount = workOrderParts.Count() - 1; partCount >= 0; partCount--)
            {
                WorkorderPart workOrderPart = workOrderParts.ElementAt(partCount);
                WorkOrderPartModel partModel = equipmentFromModel.Parts.Where(wp => wp.PartsIssueid == workOrderPart.PartsIssueid).FirstOrDefault();
                if (partModel != null)
                {
                    workOrderPart.PartReplenish = partModel.PartReplenish;
                    workOrderPart.Quantity = partModel.Quantity;
                    workOrderPart.Manufacturer = partModel.Manufacturer;
                    workOrderPart.Sku = partModel.Sku;
                    workOrderPart.Description = partModel.Description;
                    workOrderPart.NonSerializedIssue = partModel.Issue;
                    workOrderPart.WorkorderID = workOrder.WorkorderID;
                }
                else
                {
                    FarmerBrothersEntitites.WorkorderParts.Remove(workOrderPart);
                }
            }

            if (equipmentFromModel.Parts != null)
            {
                IList<WorkOrderPartModel> newParts = equipmentFromModel.Parts.Where(e => e.PartsIssueid == null).ToList();
                foreach (WorkOrderPartModel newPart in newParts)
                {
                    WorkorderPart part = new WorkorderPart()
                    {
                        AssetID = equipment.Assetid,
                        PartReplenish = newPart.PartReplenish,
                        Quantity = newPart.Quantity,
                        Manufacturer = newPart.Manufacturer,
                        Sku = newPart.Sku,
                        Description = newPart.Description,
                        NonSerializedIssue = newPart.Issue,
                        WorkorderID = workOrder.WorkorderID
                    };

                    FarmerBrothersEntitites.WorkorderParts.Add(part);
                }
            }
        }

        private void SaveWorkOrderSchedules(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            TimeZoneInfo newTimeZoneInfo = null;
            Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);

            if (workorderManagement.ResponsibleTechId.HasValue)
            {
                TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, workorderManagement.ResponsibleTechId.Value);
                WorkorderSchedule workOrderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech == 1 || ws.Techid == workorderManagement.ResponsibleTechId.Value).FirstOrDefault();
                if (workOrderSchedule != null)
                {
                    if (techHierarchyView != null)
                    {
                        workOrderSchedule.Techid = Convert.ToInt32(techHierarchyView.TechID);
                        workOrderSchedule.TechName = techHierarchyView.PreferredProvider;
                    }
                }
                else
                {
                    if (techHierarchyView != null)
                    {
                        IndexCounter counter = Utility.GetIndexCounter("ScheduleID", 1);
                        counter.IndexValue++;
                        //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                        WorkorderSchedule newworkOrderSchedule = new WorkorderSchedule()
                        {
                            Scheduleid = counter.IndexValue.Value,
                            Techid = Convert.ToInt32(techHierarchyView.TechID),
                            TechName = techHierarchyView.PreferredProvider,
                            WorkorderID = workOrder.WorkorderID,
                            TechPhone = techHierarchyView.AreaCode + techHierarchyView.ProviderPhone,
                            ServiceCenterName = techHierarchyView.BranchName,
                            ServiceCenterID = Convert.ToInt32(techHierarchyView.TechID),
                            FSMName = techHierarchyView.DSMName,
                            FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>(),
                            EntryDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites),
                            ScheduleDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites),
                            TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"],
                            PrimaryTech = 1
                        };
                        workOrder.WorkorderSchedules.Add(newworkOrderSchedule);
                    }
                }
            }

            if (workorderManagement.AssistTechIds.Count > 0)
            {
                for (int count = workOrder.WorkorderSchedules.Count - 1; count >= 0; count--)
                {
                    WorkorderSchedule workOrderSchedule = workOrder.WorkorderSchedules.ElementAt(count);

                    if (workOrderSchedule.Techid.HasValue)
                    {
                        if (workOrderSchedule.PrimaryTech == 0 && !workorderManagement.AssistTechIds.Contains(workOrderSchedule.Techid.Value))
                        {
                            workOrder.WorkorderSchedules.Remove(workOrderSchedule);
                        }
                    }
                }

                foreach (int assistTechId in workorderManagement.AssistTechIds)
                {
                    TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, assistTechId);
                    if (techHierarchyView != null)
                    {
                        WorkorderSchedule workOrderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.Techid == techHierarchyView.TechID).FirstOrDefault();
                        if (workOrderSchedule != null)
                        {
                            workOrderSchedule.PrimaryTech = 0;
                        }
                        else
                        {
                            IndexCounter counter = Utility.GetIndexCounter("ScheduleID", 1);
                            counter.IndexValue++;
                            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                            WorkorderSchedule newworkOrderSchedule = new WorkorderSchedule()
                            {
                                Scheduleid = counter.IndexValue.Value,
                                Techid = Convert.ToInt32(techHierarchyView.TechID),
                                TechName = techHierarchyView.PreferredProvider,
                                WorkorderID = workOrder.WorkorderID,
                                TechPhone = techHierarchyView.ProviderPhone,
                                FSMName = techHierarchyView.DSMName,
                                FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>(),
                                EntryDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, newTimeZoneInfo),
                                ScheduleDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, newTimeZoneInfo),
                                TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"],

                                PrimaryTech = 0
                            };
                            workOrder.WorkorderSchedules.Add(newworkOrderSchedule);
                        }
                    }
                }
            }
        }

        public void SaveNotes(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            if (workorderManagement.NewNotes != null)
            {
                if (workorderManagement.Notes.ProjectFlatRate.HasValue)
                {
                    workOrder.ProjectFlatRate = Math.Round(workorderManagement.Notes.ProjectFlatRate.Value, 2);
                }
                workOrder.ProjectID = workorderManagement.Notes.ProjectNumber;
                TimeZoneInfo newTimeZoneInfo = null;
                Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                foreach (NewNotesModel newNotesModel in workorderManagement.NewNotes)
                {
                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = currentTime,
                        Notes = newNotesModel.Text,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        WorkorderID = workOrder.WorkorderID,
                        isDispatchNotes = 0
                    };
                    FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                }

                if (workorderManagement.Notes.IsSpecificTechnician)
                {
                    if (!string.IsNullOrWhiteSpace(workorderManagement.Notes.TechID))
                    {
                        NotesHistory notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 1,
                            EntryDate = currentTime,
                            Notes = "FB Employee specific Technician: " + workorderManagement.Notes.TechID,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            WorkorderID = workOrder.WorkorderID,
                            isDispatchNotes = 1
                        };
                        FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                    }
                }



            }
        }

        private void SaveUnknownCustomer(WorkOrder workOrder)
        {
            IndexCounter counter = Utility.GetIndexCounter("CustomerID", 1);
            counter.IndexValue++;
            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
            workOrder.CustomerID = counter.IndexValue.Value;
            workOrder.WorkorderCallstatus = "Hold for AB";

        }

        private void SavePhoneSolveDetails(WorkorderManagementModel workorderManagement, WorkOrder workOrder, out string message)
        {
            message = string.Empty;
            if (workorderManagement.PhoneSolveId.HasValue)
            {
                if (workorderManagement.PhoneSolveId > 0)
                {
                    AllFBStatu phoneSolveStatus = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workorderManagement.PhoneSolveId).FirstOrDefault();
                    if (phoneSolveStatus != null)
                    {
                        bool isValid = true;
                        TechHierarchyView techHierarchyView = null;
                        if (string.Compare(phoneSolveStatus.FBStatus, "Attempting", true) == 0)
                        {
                            if (workorderManagement.PhoneSolveTechId.HasValue == false)
                            {
                                message += "Phone Solve Tech ID is required!";
                                isValid = false;
                            }
                            else
                            {
                                techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, workorderManagement.PhoneSolveTechId.Value);
                                if (techHierarchyView == null)
                                {
                                    message += "Phone Solve Tech ID is not valid!";
                                    isValid = false;
                                }
                            }
                        }

                        if (isValid == true)
                        {
                            bool isChanged = true;
                            IEnumerable<PhoneSolveLog> phoneSolveLogs = workOrder.PhoneSolveLogs.OrderByDescending(p => p.AttemptedDate);
                            if (phoneSolveLogs.Count() > 0)
                            {
                                PhoneSolveLog log = phoneSolveLogs.ElementAt(0);
                                if (log.TechId == workorderManagement.PhoneSolveTechId && log.PhoneSolveId == workorderManagement.PhoneSolveId)
                                {
                                    isChanged = false;
                                }
                            }

                            DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                            if (isChanged)
                            {
                                if (techHierarchyView != null)
                                {
                                    WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.WorkorderID == workOrder.WorkorderID && ws.Techid == workorderManagement.PhoneSolveTechId).FirstOrDefault();
                                    if (schedule != null)
                                    {
                                        schedule.Techid = Convert.ToInt32(techHierarchyView.TechID);
                                        schedule.TechName = techHierarchyView.PreferredProvider;
                                        schedule.TechPhone = techHierarchyView.ProviderPhone;
                                        schedule.FSMName = techHierarchyView.DSMName;
                                        schedule.FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>();
                                        schedule.EntryDate = currentTime;
                                        schedule.TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"];
                                        schedule.PrimaryTech = 1;
                                        schedule.AssistTech = -1;
                                        schedule.AssignedStatus = "Accepted";
                                        schedule.ModifiedScheduleDate = currentTime;

                                    }
                                    else
                                    {
                                        IndexCounter scheduleCounter = Utility.GetIndexCounter("ScheduleID", 1);
                                        scheduleCounter.IndexValue++;
                                        //FarmerBrothersEntitites.Entry(scheduleCounter).State = System.Data.Entity.EntityState.Modified;

                                        WorkorderSchedule newworkOrderSchedule = new WorkorderSchedule()
                                        {
                                            Scheduleid = scheduleCounter.IndexValue.Value,
                                            Techid = Convert.ToInt32(techHierarchyView.TechID),
                                            TechName = techHierarchyView.PreferredProvider,
                                            WorkorderID = workOrder.WorkorderID,
                                            TechPhone = techHierarchyView.ProviderPhone,
                                            FSMName = techHierarchyView.DSMName,
                                            FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>(),
                                            EntryDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites),
                                            TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"],
                                            PrimaryTech = 1,
                                            AssistTech = -1,
                                            AssignedStatus = "Accepted",
                                            ModifiedScheduleDate = currentTime
                                        };
                                        workOrder.WorkorderSchedules.Add(newworkOrderSchedule);
                                    }
                                }

                                PhoneSolveLog newLog = new PhoneSolveLog()
                                {
                                    PhoneSolveId = workorderManagement.PhoneSolveId.Value,
                                    TechId = workorderManagement.PhoneSolveTechId,
                                    WorkorderId = workOrder.WorkorderID,
                                    AttemptedDate = currentTime
                                };

                                workOrder.PhoneSolveLogs.Add(newLog);

                                WorkorderStatusLog statusLog = new WorkorderStatusLog() { StatusFrom = workOrder.WorkorderCallstatus, StatusTo = phoneSolveStatus.FBStatus, StausChangeDate = currentTime, WorkorderID = workOrder.WorkorderID };
                                workOrder.WorkorderStatusLogs.Add(statusLog);
                                workOrder.WorkorderCallstatus = phoneSolveStatus.FBStatus;

                                if (techHierarchyView != null)
                                {
                                    NotesHistory notesHistory = new NotesHistory()
                                    {
                                        AutomaticNotes = 0,
                                        EntryDate = currentTime,
                                        Notes = techHierarchyView.TechID + " - " + techHierarchyView.PreferredProvider + " - " + phoneSolveStatus.FBStatus,
                                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                        UserName = UserName,
                                        isDispatchNotes = 1
                                    };
                                    workOrder.NotesHistories.Add(notesHistory);
                                }
                                else
                                {
                                    NotesHistory notesHistory = new NotesHistory()
                                    {
                                        AutomaticNotes = 0,
                                        EntryDate = currentTime,
                                        Notes = phoneSolveStatus.FBStatus,
                                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                        UserName = UserName,
                                        isDispatchNotes = 1
                                    };
                                    workOrder.NotesHistories.Add(notesHistory);
                                }
                            }
                        }
                    }
                    else
                    {
                        message += "Phone Solve is not valid!";
                    }
                }
            }
        }

        private void SavePhoneSolveDetails1(WorkorderManagementModel workorderManagement, WorkOrder workOrder, out string message)
        {
            message = string.Empty;
            if (workorderManagement.PhoneSolveId.HasValue)
            {
                AllFBStatu phoneSolveStatus = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workorderManagement.PhoneSolveId).FirstOrDefault();

                if (phoneSolveStatus != null && phoneSolveStatus.StatusSequence == 1)
                {
                    if (workorderManagement.PhoneSolveTechId.HasValue)
                    {
                        TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, workorderManagement.PhoneSolveTechId.Value);
                        if (techHierarchyView != null)
                        {
                            bool isChanged = true;
                            IEnumerable<PhoneSolveLog> phoneSolveLogs = workOrder.PhoneSolveLogs.OrderByDescending(p => p.AttemptedDate);
                            if (phoneSolveLogs.Count() > 0)
                            {
                                PhoneSolveLog log = phoneSolveLogs.ElementAt(0);
                                if (log.TechId == workorderManagement.PhoneSolveTechId && log.PhoneSolveId == workorderManagement.PhoneSolveId)
                                {
                                    isChanged = false;
                                }
                            }

                            if (isChanged)
                            {
                                TimeZoneInfo newTimeZoneInfo = null;
                                Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);

                                DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                                WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.WorkorderID == workOrder.WorkorderID && ws.Techid == workorderManagement.PhoneSolveTechId).FirstOrDefault();
                                if (schedule != null)
                                {
                                    schedule.Techid = Convert.ToInt32(techHierarchyView.TechID);
                                    schedule.TechName = techHierarchyView.PreferredProvider;
                                    schedule.TechPhone = techHierarchyView.ProviderPhone;
                                    schedule.FSMName = techHierarchyView.DSMName;
                                    schedule.FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>();
                                    schedule.EntryDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                                    schedule.TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"];
                                    schedule.PrimaryTech = 0;
                                    schedule.AssistTech = -1;
                                    schedule.AssignedStatus = "Sent";
                                    schedule.ModifiedScheduleDate = currentTime;
                                }
                                else
                                {
                                    IndexCounter scheduleCounter = Utility.GetIndexCounter("ScheduleID", 1);
                                    scheduleCounter.IndexValue++;
                                    //FarmerBrothersEntitites.Entry(scheduleCounter).State = System.Data.Entity.EntityState.Modified;

                                    WorkorderSchedule newworkOrderSchedule = new WorkorderSchedule()
                                    {
                                        Scheduleid = scheduleCounter.IndexValue.Value,
                                        Techid = Convert.ToInt32(techHierarchyView.TechID),
                                        TechName = techHierarchyView.PreferredProvider,
                                        WorkorderID = workOrder.WorkorderID,
                                        TechPhone = techHierarchyView.ProviderPhone,
                                        FSMName = techHierarchyView.DSMName,
                                        FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>(),
                                        EntryDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, newTimeZoneInfo),
                                        TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"],

                                        PrimaryTech = 0,
                                        AssistTech = -1,
                                        AssignedStatus = "Sent",
                                        ModifiedScheduleDate = currentTime
                                    };
                                    workOrder.WorkorderSchedules.Add(newworkOrderSchedule);
                                }

                                PhoneSolveLog newLog = new PhoneSolveLog()
                                {
                                    PhoneSolveId = workorderManagement.PhoneSolveId.Value,
                                    TechId = workorderManagement.PhoneSolveTechId.Value,
                                    WorkorderId = workOrder.WorkorderID,
                                    AttemptedDate = currentTime
                                };

                                workOrder.PhoneSolveLogs.Add(newLog);
                                workOrder.WorkorderCallstatus = phoneSolveStatus.FBStatus;

                                NotesHistory notesHistory = new NotesHistory()
                                {
                                    AutomaticNotes = 0,
                                    EntryDate = currentTime,
                                    Notes = techHierarchyView.TechID + " - " + techHierarchyView.PreferredProvider + " - " + phoneSolveStatus.FBStatus,
                                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                    UserName = UserName,
                                    isDispatchNotes = 1
                                };
                                workOrder.NotesHistories.Add(notesHistory);
                            }
                        }
                        else
                        {
                            message += "Phone solve Tech ID is not valid!";
                        }
                    }
                    else
                    {
                        message += "Phone solve Tech ID is required!";
                    }
                }
                else
                {
                    workOrder.WorkorderCallstatus = phoneSolveStatus.FBStatus;
                }
            }
        }

        private bool SendFollowupMail(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            bool retunValue = false;

            IEnumerable<WorkorderSchedule> schedules = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted");
            if (schedules.Count() > 0)
            {
                StringBuilder salesEmailBody = new StringBuilder();

                salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("<BR>");

                if (workOrder.FollowupCallID.HasValue)
                {
                    AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(a => a.FBStatusID == workOrder.FollowupCallID.Value).FirstOrDefault();
                    if (status != null)
                    {
                        salesEmailBody.Append("Follow Up Reason: ");
                        salesEmailBody.Append(status.FBStatus);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("<BR>");

                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("<BR>");

                        Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();

                        int TotalCallsCount = FarmerBrothers.Models.CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, workOrder.CustomerID.ToString());
                        string IsBillable = "";
                        string ServiceLevelDesc = "";
                        if (!string.IsNullOrEmpty(customer.BillingCode))
                        {
                            IsBillable = FarmerBrothers.Models.CustomerModel.IsBillableService(customer.BillingCode, TotalCallsCount);
                            ServiceLevelDesc = FarmerBrothers.Models.CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, customer.BillingCode);
                        }
                        else
                        {
                            IsBillable = " ";
                            ServiceLevelDesc = " - ";
                        }

                        salesEmailBody.Append("CALL TIME: ");
                        salesEmailBody.Append(workOrder.WorkorderEntryDate);
                        salesEmailBody.Append("<BR>");

                        salesEmailBody.Append("Work Order ID#: ");
                        salesEmailBody.Append(workOrder.WorkorderID);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("Service Level: ");
                        salesEmailBody.Append(ServiceLevelDesc);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("Billable: ");
                        salesEmailBody.Append(IsBillable);

                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("CUSTOMER INFORMATION: ");
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("CUSTOMER#: ");
                        salesEmailBody.Append(workOrder.CustomerID);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append(workOrder.CustomerName);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append(customer.Address1);
                        salesEmailBody.Append(",");
                        salesEmailBody.Append(customer.Address2);
                        salesEmailBody.Append("<BR>");
                        //salesEmailBody.Append(workOrder.CustomerCity);
                        salesEmailBody.Append(customer.City);
                        salesEmailBody.Append(",");
                        //salesEmailBody.Append(workOrder.CustomerState);
                        salesEmailBody.Append(customer.State);
                        salesEmailBody.Append(" ");
                        //salesEmailBody.Append(workOrder.CustomerZipCode);
                        salesEmailBody.Append(customer.PostalCode);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append(workOrder.WorkorderContactName);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("PHONE: ");
                        salesEmailBody.Append(workOrder.WorkorderContactPhone);
                        salesEmailBody.Append("<BR>");

                        salesEmailBody.Append("BRANCH: ");
                        salesEmailBody.Append(customer.Branch);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("ROUTE#: ");
                        salesEmailBody.Append(customer.Route);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("<span style='color:#ff0000'><b>");
                        salesEmailBody.Append("LAST SALES DATE: ");
                        salesEmailBody.Append(GetCustomerById(workOrder.CustomerID).LastSaleDate);
                        salesEmailBody.Append("</b></span>");
                        salesEmailBody.Append("<BR>");

                        salesEmailBody.Append("HOURS OF OPERATION: ");
                        salesEmailBody.Append(workOrder.HoursOfOperation);
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("CALL CODES: ");
                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("<BR>");

                        foreach (WorkorderEquipmentRequested equipment in workOrder.WorkorderEquipmentRequesteds)
                        {
                            salesEmailBody.Append("CATEGORY : ");
                            salesEmailBody.Append(equipment.Category);
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("MODEL# : ");
                            salesEmailBody.Append(equipment.Model);
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("LOCATION : ");
                            salesEmailBody.Append(equipment.Location);
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("SYMPTOM : ");
                            salesEmailBody.Append(equipment.Symptomid);
                            salesEmailBody.Append("<BR>");

                            WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                            if (callType != null)
                            {
                                salesEmailBody.Append("CALLTYPE : ");
                                salesEmailBody.Append(callType.CallTypeID);
                                salesEmailBody.Append(" - ");
                                salesEmailBody.Append(callType.Description);
                                salesEmailBody.Append("<BR>");
                            }

                            Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                            if (symptom != null)
                            {
                                salesEmailBody.Append("SYMPTOM : ");
                                salesEmailBody.Append(symptom.SymptomID);
                                salesEmailBody.Append(" - ");
                                salesEmailBody.Append(symptom.Description);
                                salesEmailBody.Append("<BR>");
                            }
                            salesEmailBody.Append("<BR>");
                        }

                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("CALL NOTES: ");
                        salesEmailBody.Append("<BR>");
                        IEnumerable<NotesHistory> histories = workOrder.NotesHistories.OrderByDescending(n => n.EntryDate);

                        foreach (NotesHistory history in histories)
                        {
                            salesEmailBody.Append(history.UserName);
                            salesEmailBody.Append(" ");
                            salesEmailBody.Append(history.EntryDate);
                            salesEmailBody.Append(" ");
                            salesEmailBody.Append(history.Notes.Replace("\\n", " ").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\r", " "));
                            salesEmailBody.Append("<BR>");
                        }

                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("SERVICE HISTORY:");
                        salesEmailBody.Append("<BR>");

                        DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);

                        IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
                            Where(w => w.CustomerID == workOrder.CustomerID && (DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) < 90
                                          && DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) > -90));


                        foreach (WorkOrder previousWorkOrder in previousWorkOrders)
                        {
                            salesEmailBody.Append("Work Order ID#: ");
                            salesEmailBody.Append(previousWorkOrder.WorkorderID);
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("ENTRY DATE: ");
                            salesEmailBody.Append(previousWorkOrder.WorkorderEntryDate);
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("STATUS : ");
                            salesEmailBody.Append(previousWorkOrder.WorkorderCallstatus);
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("CALL CODES: ");
                            salesEmailBody.Append("<BR>");

                            foreach (WorkorderEquipment equipment in previousWorkOrder.WorkorderEquipments)
                            {
                                salesEmailBody.Append("MAKE : ");
                                salesEmailBody.Append(equipment.Manufacturer);
                                salesEmailBody.Append("<BR>");
                                salesEmailBody.Append("MODEL# : ");
                                salesEmailBody.Append(equipment.Model);
                                salesEmailBody.Append("<BR>");

                                WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                                if (callType != null)
                                {
                                    salesEmailBody.Append("CALLTYPE : ");
                                    salesEmailBody.Append(callType.CallTypeID);
                                    salesEmailBody.Append(" - ");
                                    salesEmailBody.Append(callType.Description);
                                    salesEmailBody.Append("<BR>");
                                }

                                Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                                if (symptom != null)
                                {
                                    salesEmailBody.Append("SYMPTOM : ");
                                    salesEmailBody.Append(symptom.SymptomID);
                                    salesEmailBody.Append(" - ");
                                    salesEmailBody.Append(symptom.Description);
                                    salesEmailBody.Append("<BR>");
                                }

                                salesEmailBody.Append("Location : ");
                                salesEmailBody.Append(equipment.Location);
                                salesEmailBody.Append("<BR>");
                            }
                            salesEmailBody.Append("<BR>");

                            salesEmailBody.Append("CALL NOTES: ");
                            salesEmailBody.Append("<BR>");
                            IEnumerable<NotesHistory> previousHistories = previousWorkOrder.NotesHistories.OrderByDescending(n => n.EntryDate);

                            foreach (NotesHistory history in previousHistories)
                            {
                                salesEmailBody.Append(history.UserName);
                                salesEmailBody.Append(" ");
                                salesEmailBody.Append(history.EntryDate);
                                salesEmailBody.Append(" ");
                                salesEmailBody.Append(history.Notes.Replace("\\n", " ").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\r", " "));
                                salesEmailBody.Append("<BR>");
                            }

                            salesEmailBody.Append("<BR>");
                        }

                        salesEmailBody.Append("<BR>");
                        salesEmailBody.Append("<BR>");

                        string contentId = Guid.NewGuid().ToString();
                        string logoPath = Server.MapPath("~/img/main-logo.png");
                        salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

                        AlternateView avHtml = AlternateView.CreateAlternateViewFromString
                           (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

                        LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
                        inline.ContentId = contentId;
                        avHtml.LinkedResources.Add(inline);

                        var message = new MailMessage();

                        message.AlternateViews.Add(avHtml);

                        message.IsBodyHtml = true;
                        message.Body = salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();
                        message.Priority = MailPriority.High;

                        foreach (WorkorderSchedule schedule in schedules)
                        {
                            int techid = Convert.ToInt32(schedule.Techid);
                            TECH_HIERARCHY techView = GetTechById(techid);

                            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                            {
                                string[] addresses = ConfigurationManager.AppSettings["TestEmail"].Split(';');
                                foreach (string address in addresses)
                                {
                                    if (address.ToLower().Contains("@jmsmucker.com")) continue;
                                    if (!string.IsNullOrWhiteSpace(address))
                                    {
                                        message.To.Add(new MailAddress(address));
                                    }
                                }
                            }
                            else if (techView != null)
                            {
                                if (!string.IsNullOrEmpty(techView.RimEmail))
                                {
                                    string[] addresses = techView.RimEmail.Split(';');
                                    foreach (string address in addresses)
                                    {
                                        if (address.ToLower().Contains("@jmsmucker.com")) continue;
                                        if (!string.IsNullOrWhiteSpace(address))
                                        {
                                            message.To.Add(new MailAddress(address));
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(techView.EmailCC))
                                {
                                    string[] addresses = techView.EmailCC.Split(';');
                                    foreach (string address in addresses)
                                    {
                                        if (!string.IsNullOrWhiteSpace(address))
                                        {
                                            message.CC.Add(new MailAddress(address));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string[] addresses = ConfigurationManager.AppSettings["TestEmail"].Split(';');
                                foreach (string address in addresses)
                                {
                                    if (address.ToLower().Contains("@jmsmucker.com")) continue;
                                    if (!string.IsNullOrWhiteSpace(address))
                                    {
                                        message.To.Add(new MailAddress(address));
                                    }
                                }
                            }
                        }




                        StringBuilder subject = new StringBuilder();
                        //subject.AppendFormat("<font style='color:#ff0000'><b>");
                        subject.Append("Follow Up Email: - WO: ");
                        subject.Append(workOrder.WorkorderID);
                        subject.Append(" Customer: ");
                        subject.Append(workOrder.CustomerName);
                        subject.Append(" ST: ");
                        subject.Append(workOrder.CustomerState);
                        subject.Append(" Call Type: ");
                        subject.Append(workOrder.WorkorderCalltypeDesc);
                        //subject.AppendFormat("</b></font>");

                        message.From = new MailAddress(ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"]);
                        message.Subject = subject. ToString();
                        message.IsBodyHtml = true;

                        try
                        {
                            using (var smtp = new SmtpClient())
                            {
                                smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                                smtp.Port = 25;
                                smtp.Send(message);
                            }
                            retunValue = true;
                        }
                        catch (Exception e)
                        {
                            retunValue = false;
                        }
                    }
                }
            }
            else
            {
                retunValue = true;
            }

            return retunValue;
        }

        private double SaveBillableList(WorkorderManagementModel workorderManagement)
        {
            double totalPrice = 0;
            double UnitPrice = 0;
            IEnumerable<FbWorkOrderSKU> FBWOSkuList = FarmerBrothersEntitites.FbWorkOrderSKUs.Where(a => a.WorkorderID == workorderManagement.WorkOrder.WorkorderID).ToList();

            if (FBWOSkuList != null)
            {
                for (int count = FBWOSkuList.Count() - 1; count >= 0; count--)
                {
                    FbWorkOrderSKU FBWOSku = FBWOSkuList.ElementAt(count);
                    FarmerBrothersEntitites.FbWorkOrderSKUs.Remove(FBWOSku);
                }
            }
            
            IList<FbWorkorderBillableSKUModel> newFBWOList = workorderManagement.BillableSKUList.ToList();            
            if (newFBWOList.Count() > 0)
            {
                foreach (FbWorkorderBillableSKUModel newFBWOItem in newFBWOList)
                {
                    FbWorkOrderSKU FBWOSku = new FbWorkOrderSKU()
                    {
                        WorkorderID = workorderManagement.WorkOrder.WorkorderID,
                        SKU = newFBWOItem.SKU,
                        Qty = newFBWOItem.Qty
                    };
                    using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                    {
                        UnitPrice = Convert.ToDouble(entity.FbBillableSKUs.Where(s => s.SKU == newFBWOItem.SKU).Select(s => s.UnitPrice).FirstOrDefault());
                    }
                    totalPrice += Convert.ToDouble(newFBWOItem.Qty * UnitPrice);
                    FarmerBrothersEntitites.FbWorkOrderSKUs.Add(FBWOSku);
                }
            }

            return totalPrice;
        }

        [AllowAnonymous]
        public double SaveBillingDetails(IList<BillingModel> newFBWOList, int WorkorderID)
        {
            double totalPrice = 0;
            double UnitPrice = 0;
            IEnumerable<WorkorderBillingDetail> FBBillingList = FarmerBrothersEntitites.WorkorderBillingDetails.Where(a => a.WorkorderId == WorkorderID).ToList();

            if (FBBillingList != null)
            {
                for (int count = FBBillingList.Count() - 1; count >= 0; count--)
                {
                    WorkorderBillingDetail FBWOSku = FBBillingList.ElementAt(count);
                    FarmerBrothersEntitites.WorkorderBillingDetails.Remove(FBWOSku);
                }
            }

            if (newFBWOList.Count() > 0)
            {
                foreach (BillingModel newFBWOItem in newFBWOList)
                {
                    WorkorderBillingDetail FBWOSku = new WorkorderBillingDetail()
                    {
                        WorkorderId = WorkorderID,
                        Quantity = newFBWOItem.Quantity,
                        EntryDate = DateTime.Now,
                        BillingCode = newFBWOItem.BillingCode
                    };
                    using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                    {
                        UnitPrice = Convert.ToDouble(entity.BillingItems.Where(s => s.BillingCode == newFBWOItem.BillingCode).Select(s => s.UnitPrice).FirstOrDefault());
                    }
                    totalPrice += Convert.ToDouble(newFBWOItem.Quantity * UnitPrice);
                    FarmerBrothersEntitites.WorkorderBillingDetails.Add(FBWOSku);
                }
            }

            return totalPrice;
        }

        private bool ValidateSkuList(IList<FbWorkorderBillableSKUModel> SKUList)
        {
            List<String> duplicates = SKUList.GroupBy(x => x.SKU)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key)
                             .ToList();

            if (duplicates.Count > 0)
                return true;
            else
                return false;
        }

        private void SaveRemovalDetails(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            if (workorderManagement.RemovalCount > 0)
            {
                AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(a => a.FBStatusID == workorderManagement.RemovalReason).FirstOrDefault();
                RemovalSurvey survey = FarmerBrothersEntitites.RemovalSurveys.Where(r => r.WorkorderID == workOrder.WorkorderID).FirstOrDefault();
                if (survey != null)
                {
                    survey.JMSOwnedMachines = workorderManagement.RemovalCount;
                    survey.RemovalDate = workorderManagement.RemovalDate;
                    if (status != null)
                    {
                        survey.RemovalReason = status.FBStatus;
                    }
                    survey.RemoveAllMachines = workorderManagement.RemovaAll.ToString();
                    survey.BeveragesSupplier = workorderManagement.BeveragesSupplier;
                }
                else
                {
                    RemovalSurvey newSurvey = new RemovalSurvey()
                    {
                        BeveragesSupplier = workorderManagement.BeveragesSupplier,
                        JMSOwnedMachines = workorderManagement.RemovalCount,
                        RemovalDate = workorderManagement.RemovalDate,
                        RemoveAllMachines = workorderManagement.RemovaAll.ToString(),
                        WorkorderID = workOrder.WorkorderID,
                        RemovalReason = status.FBStatus
                    };
                    FarmerBrothersEntitites.RemovalSurveys.Add(newSurvey);

                    TimeZoneInfo newTimeZoneInfo = null;
                    Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                    DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = currentTime,
                        Notes = "How many Smucker owned machines will we be removing? - " + workorderManagement.RemovalCount,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 1
                    };
                    workOrder.NotesHistories.Add(notesHistory);

                    if (workorderManagement.RemovalDate.HasValue)
                    {
                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "What date will you need these machines removed by? - " + workorderManagement.RemovalDate.Value.ToString("MM/dd/yyyy"),
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 1
                        };
                        workOrder.NotesHistories.Add(notesHistory);
                    }

                    notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = currentTime,
                        Notes = "Are we removing all machines from your facility? - " + workorderManagement.RemovaAll.ToString(),
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 1
                    };
                    workOrder.NotesHistories.Add(notesHistory);

                    if (workorderManagement.RemovaAll)
                    {

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "May I ask the reason you have chosen to remove our machines from your location? - " + status.FBStatus,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 1
                        };
                        workOrder.NotesHistories.Add(notesHistory);

                        if (!string.IsNullOrWhiteSpace(workorderManagement.BeveragesSupplier))
                        {
                            notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 0,
                                EntryDate = currentTime,
                                Notes = "Who will be supplying your beverages going forward? - " + workorderManagement.BeveragesSupplier,
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = UserName,
                                isDispatchNotes = 1
                            };
                            workOrder.NotesHistories.Add(notesHistory);
                        }

                        StringBuilder notes = new StringBuilder();
                        if (workorderManagement.ClosingBusiness)
                        {
                            notes.Append("Closing Business;");
                        }
                        if (workorderManagement.FlavorOrTasteOfCoffee)
                        {
                            notes.Append("Flavor/Taste of Coffee;");
                        }
                        if (workorderManagement.EquipmentServiceReliabilityorResponseTime)
                        {
                            notes.Append("Equipment service reliability / response time;");
                        }
                        if (workorderManagement.EquipmentReliability)
                        {
                            notes.Append("Equipment reliability;");
                        }
                        if (workorderManagement.CostPerCup)
                        {
                            notes.Append("Cost per Cup;");
                        }
                        if (workorderManagement.ChangingGroupPurchasingProgram)
                        {
                            notes.Append("Changing group purchasing program;");
                        }
                        if (workorderManagement.ChangingDistributor)
                        {
                            notes.Append("Changing Distributor;");
                        }

                        if (!string.IsNullOrWhiteSpace(notes.ToString()))
                        {
                            notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 0,
                                EntryDate = currentTime,
                                Notes = "What were the main reasons to change your beverage solution? - " + notes.ToString(),
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = UserName,
                                isDispatchNotes = 1
                            };
                            workOrder.NotesHistories.Add(notesHistory);
                        }
                    }
                }

                if (workorderManagement.RemovalCount > 1 && workorderManagement.RowId.HasValue && workorderManagement.RowId.Value < workorderManagement.WorkOrderEquipmentsRequested.Count())
                {
                    WorkOrderManagementEquipmentModel equipmentFromModel = workorderManagement.WorkOrderEquipmentsRequested.ElementAt(workorderManagement.RowId.Value);
                    IEnumerable<WorkorderEquipmentRequested> workOrderEquipments = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(we => we.WorkorderID == workOrder.WorkorderID);
                    if (workOrderEquipments != null)
                    {
                        IndexCounter counter = Utility.GetIndexCounter("AssetID", workOrderEquipments.Count());
                        for (int count = 0; count < workorderManagement.RemovalCount - 1; count++)
                        {
                            counter.IndexValue++;
                            WorkorderEquipmentRequested equipment = new WorkorderEquipmentRequested()
                            {
                                Assetid = counter.IndexValue.Value,
                                CallTypeid = 1400,
                                Category = equipmentFromModel.Category,
                                Location = equipmentFromModel.Location,
                                SerialNumber = equipmentFromModel.SerialNumber,
                                Model = equipmentFromModel.Model,
                                CatalogID = equipmentFromModel.CatelogID,
                                Symptomid = equipmentFromModel.SymptomID
                            };
                            workOrder.WorkorderEquipmentRequesteds.Add(equipment);
                        }
                        //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
                    }
                }
            }
        }

        public void UpdateWOModifiedElapsedTime(int workOrderid)
        {
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workOrderid);
                DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

                workOrder.WorkorderModifiedDate = currentTime;
                workOrder.ModifiedUserName = UserName;
                FarmerBrothersEntitites.Entry(workOrder).State = System.Data.Entity.EntityState.Modified;
                FarmerBrothersEntitites.SaveChanges();
            }

        }

        public StringBuilder GetEmailBodyWithOutLinks(WorkOrder workOrder, string subject, string toAddress, string fromAddress, int? techId, MailType mailType, bool isResponsible, string additionalMessage, string mailFrom = "", bool isFromEmailCloserLink = false, string SalesEmailAddress = "", string esmEmailAddress = "")
        {
            StringBuilder salesEmailBody = new StringBuilder();

            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();
            int TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, workOrder.CustomerID.ToString());

            List<CustomerNotesModel> CustomerNotesResults = new List<CustomerNotesModel>();
            //int? custId = Convert.ToInt32(workOrder.CustomerID);
            //var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();

            int custId = Convert.ToInt32(workOrder.CustomerID);
            int parentId = string.IsNullOrEmpty(customer.PricingParentID) ? 0 : Convert.ToInt32(customer.PricingParentID);
            var custNotes = Utility.GetCustomreNotes(custId, parentId, FarmerBrothersEntitites);

            string BccEmailAddress = fromAddress;
            ESMCCMRSMEscalation esmEscalation = FarmerBrothersEntitites.ESMCCMRSMEscalations.Where(e => e.ZIPCode == workOrder.CustomerZipCode).FirstOrDefault();
            if (esmEscalation != null)
            {
                fromAddress = esmEscalation.ESMEmail != null ? esmEscalation.ESMEmail : BccEmailAddress;
            }
            else
            {
                fromAddress = BccEmailAddress;
            }


            string IsBillable = "";
            string ServiceLevelDesc = "";
            if (!string.IsNullOrEmpty(customer.BillingCode))
            {
                IsBillable = CustomerModel.IsBillableService(customer.BillingCode, TotalCallsCount);
                ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, customer.BillingCode);
            }
            else
            {
                IsBillable = " ";
                ServiceLevelDesc = " - ";
            }


            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            

            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
            if (tchView != null)
            {
                salesEmailBody.Append("<b>");
                salesEmailBody.Append("Dispatched To : ");
                salesEmailBody.Append("</b>");
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append(tchView.CompanyName);
                salesEmailBody.Append("</b></span>");

                if (tchView.FamilyAff.ToUpper() == "SPT")
                {
                    salesEmailBody.Append("<span style='color:#ff0000'><b>");
                    salesEmailBody.Append("Third Party Dispatch ");
                    salesEmailBody.Append("</b></span>");
                }
            }
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "TRANSMIT")
            {
                salesEmailBody.Append("<b>ADDITIONAL NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                //salesEmailBody.Append(Utility.GetStringWithNewLine(additionalMessage));
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("<BR>");
            }

            if (custNotes != null && custNotes.Count > 0)
            {
                salesEmailBody.Append("<b>CUSTOMER NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                foreach (var dbCustNotes in custNotes)
                {
                    salesEmailBody.Append("[" + dbCustNotes.UserName + "] : " + dbCustNotes.Notes + Environment.NewLine);
                }
                salesEmailBody.Append("<BR>");
            }

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "ESCALATION")
            {
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append("ESCALATION NOTES: ");
                //salesEmailBody.Append(Utility.GetStringWithNewLine(additionalMessage));
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("</b></span>");
                salesEmailBody.Append("<BR>");
            }
            salesEmailBody.Append("CALL TIME: ");
            salesEmailBody.Append(workOrder.WorkorderEntryDate);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("Work Order ID#: ");
            salesEmailBody.Append(workOrder.WorkorderID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ERF#: ");
            salesEmailBody.Append(workOrder.WorkorderErfid);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Appointment Date: ");
            salesEmailBody.Append(workOrder.AppointmentDate);
            salesEmailBody.Append("<BR>");

            WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrder.WorkorderID && (w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();
            string schedlDate = ws == null ? "" : ws.EventScheduleDate.ToString();

            if (workOrder.WorkorderCalltypeid == 1300)
            {
                Erf workorderERF = FarmerBrothersEntitites.Erfs.Where(ew => ew.ErfID == workOrder.WorkorderErfid).FirstOrDefault();
                schedlDate = workorderERF == null ? schedlDate : workorderERF.OriginalRequestedDate.ToString();
            }

            salesEmailBody.Append("Schedule Date: ");
            salesEmailBody.Append(schedlDate);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Service Level: ");
            salesEmailBody.Append(ServiceLevelDesc);
            salesEmailBody.Append("<BR>");

            string ServiceTier = customer == null ? "" : string.IsNullOrEmpty(customer.ProfitabilityTier) ? " - " : customer.ProfitabilityTier;
            string paymentTerm = customer == null ? "" : (string.IsNullOrEmpty(customer.PaymentTerm) ? "" : customer.PaymentTerm);
            string PaymentTermDesc = "";
            if (!string.IsNullOrEmpty(paymentTerm))
            {
                JDEPaymentTerm paymentDesc = FarmerBrothersEntitites.JDEPaymentTerms.Where(c => c.PaymentTerm == paymentTerm).FirstOrDefault();
                PaymentTermDesc = paymentDesc == null ? "" : paymentDesc.Description;
            }
            else
            {
                PaymentTermDesc = "";
            }

            salesEmailBody.Append("Tier: ");
            salesEmailBody.Append(ServiceTier);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Payment Terms: ");
            salesEmailBody.Append(PaymentTermDesc);
            salesEmailBody.Append("<BR>");

            AllFBStatu priority = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workOrder.PriorityCode).FirstOrDefault();
            string priorityDesc = priority == null ? "" : priority.FBStatus;

            salesEmailBody.Append("Service Priority: ");
            salesEmailBody.Append(priorityDesc);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Parent: ");
            if (customer.PricingParentID != null)
            {
                NonFBCustomer nonfbcust = FarmerBrothersEntitites.NonFBCustomers.Where(c => c.NonFBCustomerId == customer.PricingParentID).FirstOrDefault();
                string parentNum = "", ParentName = "";
                if (nonfbcust != null)
                {
                    parentNum = nonfbcust.NonFBCustomerId;
                    ParentName = nonfbcust.NonFBCustomerName;
                }
                else
                {
                    parentNum = customer.PricingParentID;
                    ParentName = customer.PricingParentDesc == null ? "" : customer.PricingParentDesc;
                }
                salesEmailBody.Append(parentNum + " " + ParentName);
            }
            else
            {
                salesEmailBody.Append("");
            }
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Billable: ");
            salesEmailBody.Append(IsBillable);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Customer PO: ");
            salesEmailBody.Append(workOrder.CustomerPO);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER INFORMATION: ");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER#: ");
            salesEmailBody.Append(workOrder.CustomerID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.CustomerName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(customer.Address1);
            salesEmailBody.Append(",");
            salesEmailBody.Append(customer.Address2);
            salesEmailBody.Append("<BR>");
            //salesEmailBody.Append(workOrder.CustomerCity);
            salesEmailBody.Append(customer.City);
            salesEmailBody.Append(",");
            //salesEmailBody.Append(workOrder.CustomerState);
            salesEmailBody.Append(customer.State);
            salesEmailBody.Append(" ");
            //salesEmailBody.Append(workOrder.CustomerZipCode);
            salesEmailBody.Append(customer.PostalCode);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.WorkorderContactName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("PHONE: ");
            salesEmailBody.Append(workOrder.WorkorderContactPhone);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("BRANCH: ");
            salesEmailBody.Append(customer.Branch);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ROUTE#: ");
            salesEmailBody.Append(customer.Route);
            salesEmailBody.Append("<BR>");
            if (workOrder.FollowupCallID == 601 || workOrder.FollowupCallID == 602)
            {
                int? followupId = workOrder.FollowupCallID;
                AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(s => s.FBStatusID == followupId).FirstOrDefault();
                if (status != null && !string.IsNullOrEmpty(status.FBStatus))
                {
                    //salesEmailBody.Append("Follow Up Reason: ");
                    //salesEmailBody.Append(status.FBStatus);
                    if (workOrder.FollowupCallID == 601)
                        salesEmailBody.Append("Customer requesting an ETA phone call within the hour");
                    else if (workOrder.FollowupCallID == 602)
                        salesEmailBody.Append("Contact Customer Within The Hour");
                    salesEmailBody.Append("<BR>");
                }
            }
            salesEmailBody.Append("<span style='color:#ff0000'><b>");
            salesEmailBody.Append("LAST SALES DATE: ");
            salesEmailBody.Append(GetCustomerById(workOrder.CustomerID).LastSaleDate);
            salesEmailBody.Append("</b></span>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("HOURS OF OPERATION: ");
            salesEmailBody.Append(workOrder.HoursOfOperation);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL CODES: ");
            salesEmailBody.Append("<BR>");

            foreach (WorkorderEquipmentRequested equipment in workOrder.WorkorderEquipmentRequesteds)
            {
                salesEmailBody.Append("EQUIPMENT TYPE: ");
                salesEmailBody.Append(equipment.Category);
                salesEmailBody.Append("<BR>");

                WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                if (callType != null)
                {
                    salesEmailBody.Append("SERVICE CODE: ");
                    salesEmailBody.Append(callType.CallTypeID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(callType.Description);
                    salesEmailBody.Append("<BR>");
                }
                Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                if (symptom != null)
                {
                    salesEmailBody.Append("SYMPTOM: ");
                    salesEmailBody.Append(symptom.SymptomID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(symptom.Description);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("LOCATION: ");
                salesEmailBody.Append(equipment.Location);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("SERIAL NUMBER: ");
                salesEmailBody.Append(equipment.SerialNumber);

                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL NOTES: ");
            salesEmailBody.Append("<BR>");
            IEnumerable<NotesHistory> histories = workOrder.NotesHistories.Where(n => n.AutomaticNotes == 0).OrderByDescending(n => n.EntryDate);

            foreach (NotesHistory history in histories)
            {
                //Remove Redirected/Rejected notes for 3rd Party Tech
                if (tchView != null && tchView.FamilyAff.ToUpper() == "SPT")
                {
                    if (history != null && history.Notes != null)
                    {
                        if (history.Notes.ToLower().Contains("redirected") || history.Notes.ToLower().Contains("rejected") || history.Notes.ToLower().Contains("declined"))
                        {
                            continue;
                        }
                    }
                }

                salesEmailBody.Append(history.UserName);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.EntryDate);
                salesEmailBody.Append(" ");
                //salesEmailBody.Append(history.Notes.Replace("\\n", " ").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\r", " "));
                salesEmailBody.Append(history.Notes);
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("SERVICE HISTORY:");
            salesEmailBody.Append("<BR>");

            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            /*IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
                Where(w => w.CustomerID == workOrder.CustomerID && (DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) < 90
                              && DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) > -90));*/

            IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
                Where(w => w.CustomerID == workOrder.CustomerID).OrderByDescending(ed => ed.WorkorderEntryDate).Take(3);

            foreach (WorkOrder previousWorkOrder in previousWorkOrders)
            {
                salesEmailBody.Append("Work Order ID#: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderID);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("ENTRY DATE: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderEntryDate);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("STATUS: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderCallstatus);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("CALL CODES: ");
                salesEmailBody.Append("<BR>");

                foreach (WorkorderEquipment equipment in previousWorkOrder.WorkorderEquipments)
                {
                    salesEmailBody.Append("MAKE: ");
                    salesEmailBody.Append(equipment.Manufacturer);
                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("MODEL#: ");
                    salesEmailBody.Append(equipment.Model);
                    salesEmailBody.Append("<BR>");

                    WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                    if (callType != null)
                    {
                        salesEmailBody.Append("SERVICE CODE: ");
                        salesEmailBody.Append(callType.CallTypeID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(callType.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                    if (symptom != null)
                    {
                        salesEmailBody.Append("SYMPTOM: ");
                        salesEmailBody.Append(symptom.SymptomID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(symptom.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    salesEmailBody.Append("Location: ");
                    salesEmailBody.Append(equipment.Location);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<a href=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;></a>");           

            return salesEmailBody;
        }

        public StringBuilder GetEmailBodyWithLinks(WorkOrder workOrder, string subject, string toAddress, string fromAddress, int? techId, MailType mailType, bool isResponsible, string additionalMessage, string mailFrom = "", bool isFromEmailCloserLink = false, string SalesEmailAddress = "", string esmEmailAddress = "")
        {
            StringBuilder salesEmailBody = new StringBuilder();

            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();
            int TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, workOrder.CustomerID.ToString());

            List<CustomerNotesModel> CustomerNotesResults = new List<CustomerNotesModel>();
            //int? custId = Convert.ToInt32(workOrder.CustomerID);
            //var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();

            int custId = Convert.ToInt32(workOrder.CustomerID);
            int parentId = string.IsNullOrEmpty(customer.PricingParentID) ? 0 : Convert.ToInt32(customer.PricingParentID);
            var custNotes = Utility.GetCustomreNotes(custId, parentId, FarmerBrothersEntitites);


            string BccEmailAddress = fromAddress;
            ESMCCMRSMEscalation esmEscalation = FarmerBrothersEntitites.ESMCCMRSMEscalations.Where(e => e.ZIPCode == workOrder.CustomerZipCode).FirstOrDefault();
            if (esmEscalation != null)
            {
                fromAddress = esmEscalation.ESMEmail != null ? esmEscalation.ESMEmail : BccEmailAddress;
            }
            else
            {
                fromAddress = BccEmailAddress;
            }


            string IsBillable = "";
            string ServiceLevelDesc = "";
            if (!string.IsNullOrEmpty(customer.BillingCode))
            {
                IsBillable = CustomerModel.IsBillableService(customer.BillingCode, TotalCallsCount);
                ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, customer.BillingCode);
            }
            else
            {
                IsBillable = " ";
                ServiceLevelDesc = " - ";
            }


            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            string url = ConfigurationManager.AppSettings["DispatchResponseUrl"];
            string Redircturl = ConfigurationManager.AppSettings["RedirectResponseUrl"];
            string Closureurl = ConfigurationManager.AppSettings["CallClosureUrl"];
            string processCardurl = ConfigurationManager.AppSettings["ProcessCardUrl"];
            //string finalUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=@response&isResponsible=" + isResponsible.ToString()));

            salesEmailBody.Append("<a href=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;></a>");
            if (isFromEmailCloserLink)
            {
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            }
            else
            {
                if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
                {
                    if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                    {
                        TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();


                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=8&isResponsible=" + isResponsible.ToString())) + "\">SCHEDULE EVENT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                        /*salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=9&isResponsible=" + isResponsible.ToString())) + "\">ESM ESCALATION</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");*/


                        if (mailType == MailType.DISPATCH)
                        {
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        }
                        if (workOrder.WorkorderCallstatus == "Pending Acceptance" && techView.FamilyAff != "SPT")
                        {
                            /*string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));                                
                            salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");*/
                        }
                        /*salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");*/
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=6&isResponsible=" + isResponsible.ToString())) + "\">START</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible.ToString())) + "\">ARRIVAL</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", processCardurl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=10&isResponsible=" + isResponsible.ToString())) + "\">PROCESS CARD</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                        //}
                    }
                }
                else if (mailType == MailType.REDIRECTED)
                {
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible + "\">DISREGARD</a>");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
                }

            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
            if (tchView != null && tchView.FamilyAff.ToUpper() == "SPT")
            {
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append("Third Party Dispatch ");
                salesEmailBody.Append("</b></span>");
            }
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "TRANSMIT")
            {
                salesEmailBody.Append("<b>ADDITIONAL NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                //salesEmailBody.Append(Utility.GetStringWithNewLine(additionalMessage));
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("<BR>");
            }

            if (custNotes != null && custNotes.Count > 0)
            {
                salesEmailBody.Append("<b>CUSTOMER NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                foreach (var dbCustNotes in custNotes)
                {
                    salesEmailBody.Append("[" + dbCustNotes.UserName + "] : " + dbCustNotes.Notes + Environment.NewLine);
                }
                salesEmailBody.Append("<BR>");
            }

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "ESCALATION")
            {
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append("ESCALATION NOTES: ");
                //salesEmailBody.Append(Utility.GetStringWithNewLine(additionalMessage));
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("</b></span>");
                salesEmailBody.Append("<BR>");
            }
            salesEmailBody.Append("CALL TIME: ");
            salesEmailBody.Append(workOrder.WorkorderEntryDate);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("Work Order ID#: ");
            salesEmailBody.Append(workOrder.WorkorderID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ERF#: ");
            salesEmailBody.Append(workOrder.WorkorderErfid);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Appointment Date: ");
            salesEmailBody.Append(workOrder.AppointmentDate);
            salesEmailBody.Append("<BR>");

            WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrder.WorkorderID && (w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();
            string schedlDate = ws == null ? "" : ws.EventScheduleDate.ToString();

            if (workOrder.WorkorderCalltypeid == 1300)
            {
                Erf workorderERF = FarmerBrothersEntitites.Erfs.Where(ew => ew.ErfID == workOrder.WorkorderErfid).FirstOrDefault();
                schedlDate = workorderERF == null ? schedlDate : workorderERF.OriginalRequestedDate.ToString();
            }

            salesEmailBody.Append("Schedule Date: ");
            salesEmailBody.Append(schedlDate);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Service Level: ");
            salesEmailBody.Append(ServiceLevelDesc);
            salesEmailBody.Append("<BR>");

            string ServiceTier = customer == null ? "" : string.IsNullOrEmpty(customer.ProfitabilityTier) ? " - " : customer.ProfitabilityTier;
            string paymentTerm = customer == null ? "" : (string.IsNullOrEmpty(customer.PaymentTerm) ? "" : customer.PaymentTerm);
            string PaymentTermDesc = "";
            if (!string.IsNullOrEmpty(paymentTerm))
            {
                JDEPaymentTerm paymentDesc = FarmerBrothersEntitites.JDEPaymentTerms.Where(c => c.PaymentTerm == paymentTerm).FirstOrDefault();
                PaymentTermDesc = paymentDesc == null ? "" : paymentDesc.Description;
            }
            else
            {
                PaymentTermDesc = "";
            }

            salesEmailBody.Append("Tier: ");
            salesEmailBody.Append(ServiceTier);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Payment Terms: ");
            salesEmailBody.Append(PaymentTermDesc);
            salesEmailBody.Append("<BR>");

            AllFBStatu priority = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workOrder.PriorityCode).FirstOrDefault();
            string priorityDesc = priority == null ? "" : priority.FBStatus;

            salesEmailBody.Append("Service Priority: ");
            salesEmailBody.Append(priorityDesc);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Parent: ");
            if (customer.PricingParentID != null)
            {
                NonFBCustomer nonfbcust = FarmerBrothersEntitites.NonFBCustomers.Where(c => c.NonFBCustomerId == customer.PricingParentID).FirstOrDefault();
                string parentNum = "", ParentName = "";
                if (nonfbcust != null)
                {
                    parentNum = nonfbcust.NonFBCustomerId;
                    ParentName = nonfbcust.NonFBCustomerName;
                }
                else
                {
                    parentNum = customer.PricingParentID;
                    ParentName = customer.PricingParentDesc == null ? "" : customer.PricingParentDesc;
                }
                salesEmailBody.Append(parentNum + " " + ParentName);
            }
            else
            {
                salesEmailBody.Append("");
            }
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Billable: ");
            salesEmailBody.Append(IsBillable);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Customer PO: ");
            salesEmailBody.Append(workOrder.CustomerPO);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER INFORMATION: ");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER#: ");
            salesEmailBody.Append(workOrder.CustomerID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.CustomerName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(customer.Address1);
            salesEmailBody.Append(",");
            salesEmailBody.Append(customer.Address2);
            salesEmailBody.Append("<BR>");
            //salesEmailBody.Append(workOrder.CustomerCity);
            salesEmailBody.Append(customer.City);
            salesEmailBody.Append(",");
            //salesEmailBody.Append(workOrder.CustomerState);
            salesEmailBody.Append(customer.State);
            salesEmailBody.Append(" ");
            //salesEmailBody.Append(workOrder.CustomerZipCode);
            salesEmailBody.Append(customer.PostalCode);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.WorkorderContactName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("PHONE: ");
            salesEmailBody.Append(workOrder.WorkorderContactPhone);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("BRANCH: ");
            salesEmailBody.Append(customer.Branch);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ROUTE#: ");
            salesEmailBody.Append(customer.Route);
            salesEmailBody.Append("<BR>");
            if (workOrder.FollowupCallID == 601 || workOrder.FollowupCallID == 602)
            {
                int? followupId = workOrder.FollowupCallID;
                AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(s => s.FBStatusID == followupId).FirstOrDefault();
                if (status != null && !string.IsNullOrEmpty(status.FBStatus))
                {
                    //salesEmailBody.Append("Follow Up Reason: ");
                    //salesEmailBody.Append(status.FBStatus);
                    if (workOrder.FollowupCallID == 601)
                        salesEmailBody.Append("Customer requesting an ETA phone call within the hour");
                    else if (workOrder.FollowupCallID == 602)
                        salesEmailBody.Append("Contact Customer Within The Hour");
                    salesEmailBody.Append("<BR>");
                }
            }
            salesEmailBody.Append("<span style='color:#ff0000'><b>");
            salesEmailBody.Append("LAST SALES DATE: ");
            salesEmailBody.Append(GetCustomerById(workOrder.CustomerID).LastSaleDate);
            salesEmailBody.Append("</b></span>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("HOURS OF OPERATION: ");
            salesEmailBody.Append(workOrder.HoursOfOperation);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL CODES: ");
            salesEmailBody.Append("<BR>");

            foreach (WorkorderEquipmentRequested equipment in workOrder.WorkorderEquipmentRequesteds)
            {
                salesEmailBody.Append("EQUIPMENT TYPE: ");
                salesEmailBody.Append(equipment.Category);
                salesEmailBody.Append("<BR>");

                WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                if (callType != null)
                {
                    salesEmailBody.Append("SERVICE CODE: ");
                    salesEmailBody.Append(callType.CallTypeID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(callType.Description);
                    salesEmailBody.Append("<BR>");
                }
                Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                if (symptom != null)
                {
                    salesEmailBody.Append("SYMPTOM: ");
                    salesEmailBody.Append(symptom.SymptomID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(symptom.Description);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("LOCATION: ");
                salesEmailBody.Append(equipment.Location);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("SERIAL NUMBER: ");
                salesEmailBody.Append(equipment.SerialNumber);

                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL NOTES: ");
            salesEmailBody.Append("<BR>");
            IEnumerable<NotesHistory> histories = workOrder.NotesHistories.Where(n => n.AutomaticNotes == 0).OrderByDescending(n => n.EntryDate);

            foreach (NotesHistory history in histories)
            {
                //Remove Redirected/Rejected notes for 3rd Party Tech
                if (tchView != null && tchView.FamilyAff.ToUpper() == "SPT")
                {
                    if (history != null && history.Notes != null)
                    {
                        if (history.Notes.ToLower().Contains("redirected") || history.Notes.ToLower().Contains("rejected") || history.Notes.ToLower().Contains("declined"))
                        {
                            continue;
                        }
                    }
                }

                salesEmailBody.Append(history.UserName);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.EntryDate);
                salesEmailBody.Append(" ");
                //salesEmailBody.Append(history.Notes.Replace("\\n", " ").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\r", " "));
                salesEmailBody.Append(history.Notes);
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("SERVICE HISTORY:");
            salesEmailBody.Append("<BR>");

            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            /*IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
                Where(w => w.CustomerID == workOrder.CustomerID && (DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) < 90
                              && DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) > -90));*/

            IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
                Where(w => w.CustomerID == workOrder.CustomerID).OrderByDescending(ed => ed.WorkorderEntryDate).Take(3);

            foreach (WorkOrder previousWorkOrder in previousWorkOrders)
            {
                salesEmailBody.Append("Work Order ID#: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderID);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("ENTRY DATE: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderEntryDate);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("STATUS: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderCallstatus);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("CALL CODES: ");
                salesEmailBody.Append("<BR>");

                foreach (WorkorderEquipment equipment in previousWorkOrder.WorkorderEquipments)
                {
                    salesEmailBody.Append("MAKE: ");
                    salesEmailBody.Append(equipment.Manufacturer);
                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("MODEL#: ");
                    salesEmailBody.Append(equipment.Model);
                    salesEmailBody.Append("<BR>");

                    WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                    if (callType != null)
                    {
                        salesEmailBody.Append("SERVICE CODE: ");
                        salesEmailBody.Append(callType.CallTypeID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(callType.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                    if (symptom != null)
                    {
                        salesEmailBody.Append("SYMPTOM: ");
                        salesEmailBody.Append(symptom.SymptomID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(symptom.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    salesEmailBody.Append("Location: ");
                    salesEmailBody.Append(equipment.Location);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<a href=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;></a>");
            if (isFromEmailCloserLink)
            {
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

            }
            else
            {
                if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
                {
                    if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                    {
                        TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();

                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=9&isResponsible=" + isResponsible.ToString())) + "\">ESM ESCALATION</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                        if (mailType == MailType.DISPATCH)
                        {
                            /*salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");*/
                        }
                        if (workOrder.WorkorderCallstatus == "Pending Acceptance" && techView.FamilyAff != "SPT")
                        {
                            // salesEmailBody.Append("<a href=\"" + Redircturl + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible + "\">REDIRECT</a>");
                            string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));
                            salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        }
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        // }
                    }
                }
                else if (mailType == MailType.REDIRECTED)
                {
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
                }
            }

            return salesEmailBody;
        }

        public bool SendWorkOrderMail(WorkOrder workOrder, string subject, string toAddress, string fromAddress, int? techId, MailType mailType, bool isResponsible, string additionalMessage, string mailFrom = "", bool isFromEmailCloserLink = false, string SalesEmailAddress = "", string esmEmailAddress = "")
        {
            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();
            int TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, workOrder.CustomerID.ToString());

            List<CustomerNotesModel> CustomerNotesResults = new List<CustomerNotesModel>();
            //int? custId = Convert.ToInt32(workOrder.CustomerID);
            //var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();

            int custId = Convert.ToInt32(workOrder.CustomerID);
            int parentId = string.IsNullOrEmpty(customer.PricingParentID) ? 0 : Convert.ToInt32(customer.PricingParentID);
            var custNotes = Utility.GetCustomreNotes(custId, parentId, FarmerBrothersEntitites);

            //Removed Temporarily on Mike's say
            string BccEmailAddress = fromAddress;
            /* ESMCCMRSMEscalation esmEscalation = FarmerBrothersEntitites.ESMCCMRSMEscalations.Where(e => e.ZIPCode == workOrder.CustomerZipCode).FirstOrDefault();
           if (esmEscalation != null)
            {
                fromAddress = esmEscalation.ESMEmail != null ? esmEscalation.ESMEmail : BccEmailAddress;
            }
            else
            {
                fromAddress = BccEmailAddress;
            }*/

            StringBuilder salesEmailBodywithLinks = GetEmailBodyWithLinks(workOrder, subject, toAddress, fromAddress, techId, mailType, isResponsible, additionalMessage, mailFrom, isFromEmailCloserLink, SalesEmailAddress, esmEmailAddress);
            StringBuilder salesEmailBodywithOutLinks = GetEmailBodyWithOutLinks(workOrder, subject, toAddress, fromAddress, techId, mailType, isResponsible, additionalMessage, mailFrom, isFromEmailCloserLink, SalesEmailAddress, esmEmailAddress);
            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();

            string IsBillable = "";
            string ServiceLevelDesc = "";
            if (!string.IsNullOrEmpty(customer.BillingCode))
            {
                IsBillable = CustomerModel.IsBillableService(customer.BillingCode, TotalCallsCount);
                ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, customer.BillingCode);
            }
            else
            {
                IsBillable = " ";
                ServiceLevelDesc = " - ";
            }
            bool result = true;

            string toMailAddress = string.Empty;
            string ccMailAddress = string.Empty;
            if (!string.IsNullOrWhiteSpace(toAddress))
            {
                if (toAddress.Contains("#"))
                {
                    string[] mailToAddress = toAddress.Split('#');
                    if (mailToAddress.Count() > 0)
                    {
                        string[] addresses = mailToAddress[0].Split(';');
                        foreach (string address in addresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                if (address.ToLower().Contains("@jmsmucker.com")) continue;

                                toMailAddress += address + ";";
                            }
                        }

                        string[] ccaddresses = mailToAddress[1].Split(';');
                        foreach (string address in ccaddresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                if (address.ToLower().Contains("@jmsmucker.com")) continue;

                                ccMailAddress += address + ";";
                            }
                        }
                    }
                }
                else
                {
                    string[] addresses = toAddress.Split(';');
                    foreach (string address in addresses)
                    {
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            if (address.ToLower().Contains("@jmsmucker.com")) continue;

                            toMailAddress += address + ";";
                        }
                    }
                }
            }
            bool toResult = sendToListEmail(salesEmailBodywithLinks, fromAddress, toMailAddress, BccEmailAddress, subject, techId, customer);

            FBCustomerServiceDistribution fbcs = FarmerBrothersEntitites.FBCustomerServiceDistributions.Where(f => f.Route == customer.Route).FirstOrDefault();             
            if (fbcs != null)
            {
                if (fbcs.SalesMmanagerEmail != null)
                {
                    ccMailAddress += fbcs.SalesMmanagerEmail + ";";
                }
                if (fbcs.RegionalsEmail != null)
                {
                    ccMailAddress += fbcs.RegionalsEmail + ";";
                }
                if (fbcs.RSREmail != null)
                {
                    ccMailAddress += fbcs.RSREmail + ";";
                }                
            }
               
            bool ccResult = sendCCListEmail(salesEmailBodywithOutLinks, fromAddress, ccMailAddress, BccEmailAddress, subject, techId, customer, SalesEmailAddress, esmEmailAddress);


            //FarmerBrothersEntities fbent = new FarmerBrothersEntities();
            //NotesHistory nh = new NotesHistory();
            //nh.WorkorderID = workOrder.WorkorderID;
            //nh.Notes = "Dispatch E-mail sent to : " + toMailAddress + ", and Copied to : " + ccMailAddress;
            //nh.EntryDate = DateTime.Now;
            //nh.AutomaticNotes = 1;
            //fbent.NotesHistories.Add(nh);
            //fbent.SaveChanges();

            result = toResult;

            return result;
        }

        public bool sendToListEmail_old(StringBuilder salesEmailBodywithLinks, string fromAddress, string toAddress, string BccEmailAddress, string subject, int? techId, Contact customer)
        {
            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
            string contentId = Guid.NewGuid().ToString();
            string logoPath = string.Empty;
            if (Server == null)
            {
                logoPath = Path.Combine(HttpRuntime.AppDomainAppPath, "img/mainlogo.jpg");
            }
            else
            {
                logoPath = Server.MapPath("~/img/mainlogo.jpg");
            }

            StringBuilder salesEmailBody = new StringBuilder();

            salesEmailBody = salesEmailBodywithLinks;
            salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString
               (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

            LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
            inline.ContentId = contentId;
            avHtml.LinkedResources.Add(inline);

            var message = new MailMessage();

            message.AlternateViews.Add(avHtml);

            message.IsBodyHtml = true;
            message.Body = salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();

            string ToAddr = string.Empty;
            string CcAddr = string.Empty;
            bool result = true;
            if (!string.IsNullOrEmpty(toAddress))
            {
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                {
                    toAddress = ConfigurationManager.AppSettings["TestEmail"];
                    if (toAddress.Contains("#"))
                    {
                        string[] mailCCAddress = toAddress.Split('#');

                        if (mailCCAddress.Count() > 0)
                        {
                            ToAddr = mailCCAddress[0];
                            //string[] addresses = mailCCAddress[0].Split(';');
                            //foreach (string address in addresses)
                            //{
                            //    if (address.ToLower().Contains("@jmsmucker.com")) continue;
                            //    if (!string.IsNullOrWhiteSpace(address))
                            //    {
                            //        ToAddr += address;
                            //    }
                            //}
                        }
                    }

                }
                //string[] addresses = toAddress.Split(';');
                //foreach (string address in addresses)
                //{
                //    if (!string.IsNullOrWhiteSpace(address))
                //    {
                //        if (address.ToLower().Contains("@jmsmucker.com")) continue;

                //        message.To.Add(new MailAddress(address));
                //        ToAddr += address;
                //    }
                //}



                //message.Bcc.Add(BccEmailAddress);

                message.From = new MailAddress(fromAddress);
                message.Subject = subject;
                message.IsBodyHtml = true;

                if (tchView != null && tchView.FamilyAff != "SP")
                {
                    message.Priority = MailPriority.High;
                }

                EmailUtility eu = new EmailUtility();
                eu.SendEmail(fromAddress, ToAddr, CcAddr, subject, salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString());

                //using (var smtp = new SmtpClient())
                //{
                //    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                //    smtp.Port = 25;

                //    try
                //    {
                //        smtp.Send(message);
                //    }
                //    catch (Exception ex)
                //    {
                //        result = false;
                //    }
                //}
            }

            return result;
        }
        public bool sendCCListEmail_old(StringBuilder salesEmailBodywithoutLinks, string fromAddress,string ccMailAddress, string BccEmailAddress, string subject, int? techId, Contact customer, string SalesEmailAddress, string esmEmailAddress)
        {
            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
            string contentId = Guid.NewGuid().ToString();
            string logoPath = string.Empty;
            if (Server == null)
            {
                logoPath = Path.Combine(HttpRuntime.AppDomainAppPath, "img/mainlogo.jpg");
            }
            else
            {
                logoPath = Server.MapPath("~/img/mainlogo.jpg");
            }
            StringBuilder salesEmailBody = new StringBuilder();

            salesEmailBody = salesEmailBodywithoutLinks;
            salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString
               (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

            LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
            inline.ContentId = contentId;
            avHtml.LinkedResources.Add(inline);

            var message = new MailMessage();

            message.AlternateViews.Add(avHtml);

            message.IsBodyHtml = true;
            message.Body = salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();

            string ToAddr = string.Empty;
            string CcAddr = string.Empty;
            bool result = true;
            if (!string.IsNullOrEmpty(ccMailAddress))
            {
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                {
                    ccMailAddress = ConfigurationManager.AppSettings["TestEmail"];
                    if (ccMailAddress.Contains("#"))
                    {
                        string[] mailCCAddress = ccMailAddress.Split('#');

                        if (mailCCAddress.Count() > 0)
                        {
                            CcAddr = mailCCAddress[1];
                            //string[] addresses = mailCCAddress[1].Split(';');
                            //foreach (string address in addresses)
                            //{
                            //    if (address.ToLower().Contains("@jmsmucker.com")) continue;
                            //    if (!string.IsNullOrWhiteSpace(address))
                            //    {
                            //        CcAddr += address;
                            //    }
                            //}
                        }
                    }

                }

                //if (ccMailAddress.Contains(";"))
                //{
                //    string[] addresses = ccMailAddress.Split(';');
                //    foreach (string address in addresses)
                //    {
                //        if (!string.IsNullOrWhiteSpace(address))
                //        {
                //            if (address.ToLower().Contains("@jmsmucker.com")) continue;

                //            message.CC.Add(new MailAddress(address));
                //        }
                //    }
                //}


                //message.Bcc.Add(BccEmailAddress);

                message.From = new MailAddress(fromAddress);
                message.Subject = subject;
                message.IsBodyHtml = true;

                if (tchView != null && tchView.FamilyAff != "SP")
                {
                    message.Priority = MailPriority.High;
                }


                EmailUtility eu = new EmailUtility();
                eu.SendEmail(fromAddress, ToAddr, CcAddr, subject, salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString());

                //using (var smtp = new SmtpClient())
                //{
                //    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                //    smtp.Port = 25;

                //    try
                //    {
                //        smtp.Send(message);
                //    }
                //    catch (Exception ex)
                //    {
                //        result = false;
                //    }
                //}
            }
            
            return result;
        }

        public bool sendToListEmail(StringBuilder salesEmailBodywithLinks, string fromAddress, string toAddress, string BccEmailAddress, string subject, int? techId, Contact customer)
        {
            StringBuilder salesEmailBody = new StringBuilder();
            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();


            salesEmailBody = salesEmailBodywithLinks;

            string ToAddr = string.Empty;
            string CcAddr = string.Empty;
            bool result = true;
            if (!string.IsNullOrEmpty(toAddress))
            {
               
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                {
                    toAddress = ConfigurationManager.AppSettings["TestEmail"];
                }

                if (toAddress.Contains("#"))
                {
                    string[] mailCCAddress = toAddress.Split('#');

                    if (mailCCAddress.Count() > 0)
                    {
                        ToAddr = mailCCAddress[0];
                    }
                }
                else
                {
                    ToAddr = toAddress;
                }




                EmailUtility eu = new EmailUtility();
                eu.SendEmail(fromAddress, ToAddr, CcAddr, subject, salesEmailBody.ToString());

                //#region Comment out this sectiona nd uncomment the above when Needed

                //string contentId = Guid.NewGuid().ToString();
                //string logoPath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "img\\mainlogo.jpg";



                //salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

                //AlternateView avHtml = AlternateView.CreateAlternateViewFromString
                //   (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

                //LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
                //inline.ContentId = contentId;
                //avHtml.LinkedResources.Add(inline);

                //var message = new MailMessage();

                //message.AlternateViews.Add(avHtml);


                //message.Body = salesEmailBody.ToString();

                //if (tchView != null && tchView.FamilyAff != "SP")
                //{
                //    message.Priority = MailPriority.High;
                //}

                //string mailTo = ToAddr;
                //string mailCC = string.Empty;
                //if (!string.IsNullOrWhiteSpace(mailTo))
                //{
                //    if (mailTo.Contains("#"))
                //    {
                //        string[] mailCCAddress = mailTo.Split('#');
                //        if (mailCCAddress.Count() > 0)
                //        {
                //            string[] addresses = mailCCAddress[0].Split(';');
                //            foreach (string address in addresses)
                //            {
                //                if (!string.IsNullOrWhiteSpace(address))
                //                {
                //                    message.To.Add(new MailAddress(address));
                //                }
                //            }
                //        }
                //    }
                //    else
                //    {
                //        string[] addresses = mailTo.Split(';');
                //        foreach (string address in addresses)
                //        {
                //            if (!string.IsNullOrWhiteSpace(address))
                //            {
                //                message.To.Add(new MailAddress(address));
                //            }
                //        }
                //    }

                //    message.From = new MailAddress(fromAddress);
                //    message.Subject = subject.ToString();
                //    message.IsBodyHtml = true;

                //    using (var smtp = new SmtpClient())
                //    {
                //        smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                //        smtp.Port = 25;

                //        try
                //        {
                //            smtp.Send(message);
                //        }
                //        catch (Exception ex)
                //        {
                //            result = false;
                //        }
                //    }

                //}
                //#endregion


            }
            return result;
        }

        public bool sendCCListEmail(StringBuilder salesEmailBodywithoutLinks, string fromAddress, string ccMailAddress, string BccEmailAddress, string subject, int? techId, Contact customer, string SalesEmailAddress, string esmEmailAddress)
        {
            StringBuilder salesEmailBody = new StringBuilder();
            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();


            salesEmailBody = salesEmailBodywithoutLinks;

            string ToAddr = string.Empty;
            string CcAddr = string.Empty;
            bool result = true;
            if (!string.IsNullOrEmpty(ccMailAddress))
            {
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                {
                    ccMailAddress = ConfigurationManager.AppSettings["TestEmail"];
                }
                if (ccMailAddress.Contains("#"))
                {
                    string[] mailCCAddress = ccMailAddress.Split('#');

                    if (mailCCAddress.Count() > 0)
                    {
                        CcAddr = mailCCAddress[1];
                    }
                }
                else
                {
                    CcAddr = ccMailAddress;
                }

                if (!string.IsNullOrEmpty(SalesEmailAddress))
                {
                    CcAddr += ";" + SalesEmailAddress;                    
                }

                EmailUtility eu = new EmailUtility();
                eu.SendEmail(fromAddress, ToAddr, CcAddr, subject, salesEmailBody.ToString());

                //#region Comment out this sectiona nd uncomment the above when Needed
                //string contentId = Guid.NewGuid().ToString();
                //string logoPath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "img\\mainlogo.jpg";



                //salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

                //AlternateView avHtml = AlternateView.CreateAlternateViewFromString
                //   (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

                //LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
                //inline.ContentId = contentId;
                //avHtml.LinkedResources.Add(inline);

                //var message = new MailMessage();

                //message.AlternateViews.Add(avHtml);
                //message.Body = salesEmailBody.ToString();

                //if (tchView != null && tchView.FamilyAff != "SP")
                //{
                //    message.Priority = MailPriority.High;
                //}


                //string mailCC = CcAddr;
                //if (!string.IsNullOrWhiteSpace(mailCC))
                //{
                //    if (mailCC.Contains("#"))
                //    {
                //        string[] mailCCAddress = mailCC.Split('#');
                //        if (mailCCAddress.Count() > 0)
                //        {
                //            string[] addresses = mailCCAddress[1].Split(';');
                //            foreach (string address in addresses)
                //            {
                //                if (!string.IsNullOrWhiteSpace(address))
                //                {
                //                    message.CC.Add(new MailAddress(address));
                //                }
                //            }
                //        }
                //    }
                //    else
                //    {
                //        string[] addresses = mailCC.Split(';');
                //        foreach (string address in addresses)
                //        {
                //            if (!string.IsNullOrWhiteSpace(address))
                //            {
                //                message.CC.Add(new MailAddress(address));
                //            }
                //        }
                //    }


                //    message.From = new MailAddress(fromAddress);
                //    message.Subject = subject.ToString();
                //    message.IsBodyHtml = true;

                //    using (var smtp = new SmtpClient())
                //    {
                //        smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                //        smtp.Port = 25;

                //        try
                //        {
                //            smtp.Send(message);
                //        }
                //        catch (Exception ex)
                //        {
                //            result = false;
                //        }
                //    }

                //}
                //#endregion
            }
            return result;
        }

        public bool SendWorkOrderMail_New_Backup(WorkOrder workOrder, string subject, string toAddress, string fromAddress, int? techId, MailType mailType, bool isResponsible, string additionalMessage, string mailFrom = "", bool isFromEmailCloserLink = false, string SalesEmailAddress = "", string esmEmailAddress = "")
        {
            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();
            int TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, workOrder.CustomerID.ToString());

            List<CustomerNotesModel> CustomerNotesResults = new List<CustomerNotesModel>();
            //int? custId = Convert.ToInt32(workOrder.CustomerID);
            //var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();

            int custId = Convert.ToInt32(workOrder.CustomerID);
            int parentId = string.IsNullOrEmpty(customer.PricingParentID) ? 0 : Convert.ToInt32(customer.PricingParentID);
            var custNotes = Utility.GetCustomreNotes(custId, parentId, FarmerBrothersEntitites);


            string BccEmailAddress = fromAddress;
            ESMCCMRSMEscalation esmEscalation = FarmerBrothersEntitites.ESMCCMRSMEscalations.Where(e => e.ZIPCode == workOrder.CustomerZipCode).FirstOrDefault();
            if(esmEscalation != null)
            {
                fromAddress = esmEscalation.ESMEmail != null ? esmEscalation.ESMEmail : BccEmailAddress;
            }
            else
            {
                fromAddress = BccEmailAddress;
            }


            string IsBillable = "";
            string ServiceLevelDesc = "";
            if (!string.IsNullOrEmpty(customer.BillingCode))
            {
                IsBillable = CustomerModel.IsBillableService(customer.BillingCode, TotalCallsCount);
                ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, customer.BillingCode);
            }
            else
            {
                IsBillable = " ";
                ServiceLevelDesc = " - ";
            }

            StringBuilder salesEmailBody = new StringBuilder();

            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            string url = ConfigurationManager.AppSettings["DispatchResponseUrl"];
            string Redircturl = ConfigurationManager.AppSettings["RedirectResponseUrl"];
            string Closureurl = ConfigurationManager.AppSettings["CallClosureUrl"];
            //string finalUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=@response&isResponsible=" + isResponsible.ToString()));

            salesEmailBody.Append("<a href=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;></a>");
            if (isFromEmailCloserLink)
            {
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            }
            else
            {
                if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
                {
                    if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                    {
                        TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();


                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=8&isResponsible=" + isResponsible.ToString())) + "\">SCHEDULE EVENT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                        /*salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=9&isResponsible=" + isResponsible.ToString())) + "\">ESM ESCALATION</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");*/


                        if (mailType == MailType.DISPATCH)
                            {   
                                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            }
                            if (workOrder.WorkorderCallstatus == "Pending Acceptance" && techView.FamilyAff != "SPT")
                            {
                                /*string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));                                
                                salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");*/
                            }
                            /*salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");*/
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=6&isResponsible=" + isResponsible.ToString())) + "\">START</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible.ToString())) + "\">ARRIVAL</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            
                        //}
                    }
                }
                else if (mailType == MailType.REDIRECTED)
                {
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible + "\">DISREGARD</a>");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
                }

            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
            if (tchView!= null && tchView.FamilyAff.ToUpper() == "SPT")
            {
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append("Third Party Dispatch ");
                salesEmailBody.Append("</b></span>");
            }
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "TRANSMIT")
            {
                salesEmailBody.Append("<b>ADDITIONAL NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                //salesEmailBody.Append(Utility.GetStringWithNewLine(additionalMessage));
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("<BR>");
            }

            if (custNotes != null && custNotes.Count > 0)
            {
                salesEmailBody.Append("<b>CUSTOMER NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                foreach (var dbCustNotes in custNotes)
                {
                    salesEmailBody.Append("["+dbCustNotes.UserName +"] : "+ dbCustNotes.Notes + Environment.NewLine);
                }
                salesEmailBody.Append("<BR>");
            }

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "ESCALATION")
            {
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append("ESCALATION NOTES: ");
                //salesEmailBody.Append(Utility.GetStringWithNewLine(additionalMessage));
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("</b></span>");
                salesEmailBody.Append("<BR>");
            }
            salesEmailBody.Append("CALL TIME: ");
            salesEmailBody.Append(workOrder.WorkorderEntryDate);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("Work Order ID#: ");
            salesEmailBody.Append(workOrder.WorkorderID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ERF#: ");
            salesEmailBody.Append(workOrder.WorkorderErfid);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Appointment Date: ");
            salesEmailBody.Append(workOrder.AppointmentDate);
            salesEmailBody.Append("<BR>");

            WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrder.WorkorderID &&  (w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();
            string schedlDate = ws == null ? "" : ws.EventScheduleDate.ToString();

            if(workOrder.WorkorderCalltypeid == 1300)
            {
                Erf workorderERF = FarmerBrothersEntitites.Erfs.Where(ew => ew.ErfID == workOrder.WorkorderErfid).FirstOrDefault();
                schedlDate = workorderERF == null ? schedlDate : workorderERF.OriginalRequestedDate.ToString();
            }

            salesEmailBody.Append("Schedule Date: ");
            salesEmailBody.Append(schedlDate);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Service Level: ");
            salesEmailBody.Append(ServiceLevelDesc);
            salesEmailBody.Append("<BR>");

            string ServiceTier = customer == null ? "" : string.IsNullOrEmpty(customer.ProfitabilityTier) ? " - " : customer.ProfitabilityTier;
            string paymentTerm = customer == null ? "" : (string.IsNullOrEmpty(customer.PaymentTerm) ? "" : customer.PaymentTerm);
            string PaymentTermDesc = "";
            if (!string.IsNullOrEmpty(paymentTerm))
            {
                JDEPaymentTerm paymentDesc = FarmerBrothersEntitites.JDEPaymentTerms.Where(c => c.PaymentTerm == paymentTerm).FirstOrDefault();
                PaymentTermDesc = paymentDesc == null ? "" : paymentDesc.Description;
            }
            else
            {
                PaymentTermDesc = "";
            }

            salesEmailBody.Append("Tier: ");
            salesEmailBody.Append(ServiceTier);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Payment Terms: ");
            salesEmailBody.Append(PaymentTermDesc);
            salesEmailBody.Append("<BR>");

            AllFBStatu priority = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workOrder.PriorityCode).FirstOrDefault();
            string priorityDesc = priority == null ? "" : priority.FBStatus;

            salesEmailBody.Append("Service Priority: ");
            salesEmailBody.Append(priorityDesc);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Parent: ");
            if (customer.PricingParentID != null)
            {
                NonFBCustomer nonfbcust = FarmerBrothersEntitites.NonFBCustomers.Where(c => c.NonFBCustomerId == customer.PricingParentID).FirstOrDefault();
                string parentNum = "", ParentName = "";
                if (nonfbcust != null)
                {
                    parentNum = nonfbcust.NonFBCustomerId;
                    ParentName = nonfbcust.NonFBCustomerName;
                }
                else
                {
                    parentNum = customer.PricingParentID;
                    ParentName = customer.PricingParentDesc == null ? "" : customer.PricingParentDesc;
                }
                salesEmailBody.Append(parentNum + " " + ParentName);
            }
            else
            {
                salesEmailBody.Append("");
            }
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Billable: ");
            salesEmailBody.Append(IsBillable);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Customer PO: ");
            salesEmailBody.Append(workOrder.CustomerPO);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER INFORMATION: ");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER#: ");
            salesEmailBody.Append(workOrder.CustomerID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.CustomerName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(customer.Address1);
            salesEmailBody.Append(",");
            salesEmailBody.Append(customer.Address2);
            salesEmailBody.Append("<BR>");
            //salesEmailBody.Append(workOrder.CustomerCity);
            salesEmailBody.Append(customer.City);
            salesEmailBody.Append(",");
            //salesEmailBody.Append(workOrder.CustomerState);
            salesEmailBody.Append(customer.State);
            salesEmailBody.Append(" ");
            //salesEmailBody.Append(workOrder.CustomerZipCode);
            salesEmailBody.Append(customer.PostalCode);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.WorkorderContactName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("PHONE: ");
            salesEmailBody.Append(workOrder.WorkorderContactPhone);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("BRANCH: ");
            salesEmailBody.Append(customer.Branch);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ROUTE#: ");
            salesEmailBody.Append(customer.Route);
            salesEmailBody.Append("<BR>");
            if (workOrder.FollowupCallID == 601 || workOrder.FollowupCallID == 602)
            {
                int? followupId = workOrder.FollowupCallID;
                AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(s => s.FBStatusID == followupId).FirstOrDefault();
                if (status != null && !string.IsNullOrEmpty(status.FBStatus))
                {
                    //salesEmailBody.Append("Follow Up Reason: ");
                    //salesEmailBody.Append(status.FBStatus);
                    if(workOrder.FollowupCallID == 601)
                        salesEmailBody.Append("Customer requesting an ETA phone call within the hour");
                    else if(workOrder.FollowupCallID == 602)
                        salesEmailBody.Append("Contact Customer Within The Hour");
                    salesEmailBody.Append("<BR>");
                }
            }
            salesEmailBody.Append("<span style='color:#ff0000'><b>");
            salesEmailBody.Append("LAST SALES DATE: ");
            salesEmailBody.Append(GetCustomerById(workOrder.CustomerID).LastSaleDate);
            salesEmailBody.Append("</b></span>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("HOURS OF OPERATION: ");
            salesEmailBody.Append(workOrder.HoursOfOperation);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL CODES: ");
            salesEmailBody.Append("<BR>");

            foreach (WorkorderEquipmentRequested equipment in workOrder.WorkorderEquipmentRequesteds)
            {
                salesEmailBody.Append("EQUIPMENT TYPE: ");
                salesEmailBody.Append(equipment.Category);
                salesEmailBody.Append("<BR>");

                WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                if (callType != null)
                {
                    salesEmailBody.Append("SERVICE CODE: ");
                    salesEmailBody.Append(callType.CallTypeID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(callType.Description);
                    salesEmailBody.Append("<BR>");
                }
                Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                if (symptom != null)
                {
                    salesEmailBody.Append("SYMPTOM: ");
                    salesEmailBody.Append(symptom.SymptomID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(symptom.Description);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("LOCATION: ");
                salesEmailBody.Append(equipment.Location);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("SERIAL NUMBER: ");
                salesEmailBody.Append(equipment.SerialNumber);

                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL NOTES: ");
            salesEmailBody.Append("<BR>");
            IEnumerable<NotesHistory> histories = workOrder.NotesHistories.Where(n=>n.AutomaticNotes == 0).OrderByDescending(n => n.EntryDate);

            foreach (NotesHistory history in histories)
            {
                //Remove Redirected/Rejected notes for 3rd Party Tech
                if (tchView != null && tchView.FamilyAff.ToUpper() == "SPT")
                {
                    if (history != null && history.Notes != null)
                    {
                        if (history.Notes.ToLower().Contains("redirected") || history.Notes.ToLower().Contains("rejected") || history.Notes.ToLower().Contains("declined"))
                        {
                            continue;
                        }
                    }
                }

                salesEmailBody.Append(history.UserName);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.EntryDate);
                salesEmailBody.Append(" ");
                //salesEmailBody.Append(history.Notes.Replace("\\n", " ").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\r", " "));
                salesEmailBody.Append(history.Notes);
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("SERVICE HISTORY:");
            salesEmailBody.Append("<BR>");

            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            /*IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
                Where(w => w.CustomerID == workOrder.CustomerID && (DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) < 90
                              && DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) > -90));*/

            IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
                Where(w => w.CustomerID == workOrder.CustomerID).OrderByDescending(ed => ed.WorkorderEntryDate).Take(3);

            foreach (WorkOrder previousWorkOrder in previousWorkOrders)
            {
                salesEmailBody.Append("Work Order ID#: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderID);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("ENTRY DATE: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderEntryDate);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("STATUS: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderCallstatus);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("CALL CODES: ");
                salesEmailBody.Append("<BR>");

                foreach (WorkorderEquipment equipment in previousWorkOrder.WorkorderEquipments)
                {
                    salesEmailBody.Append("MAKE: ");
                    salesEmailBody.Append(equipment.Manufacturer);
                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("MODEL#: ");
                    salesEmailBody.Append(equipment.Model);
                    salesEmailBody.Append("<BR>");

                    WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                    if (callType != null)
                    {
                        salesEmailBody.Append("SERVICE CODE: ");
                        salesEmailBody.Append(callType.CallTypeID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(callType.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                    if (symptom != null)
                    {
                        salesEmailBody.Append("SYMPTOM: ");
                        salesEmailBody.Append(symptom.SymptomID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(symptom.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    salesEmailBody.Append("Location: ");
                    salesEmailBody.Append(equipment.Location);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<a href=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;></a>");
            if (isFromEmailCloserLink)
            {
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

            }
            else
            {
                if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
                {
                    if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                    {
                        TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
                       
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=9&isResponsible=" + isResponsible.ToString())) + "\">ESM ESCALATION</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                        if (mailType == MailType.DISPATCH)
                            {                                
                                /*salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");*/
                            }
                            if (workOrder.WorkorderCallstatus == "Pending Acceptance" && techView.FamilyAff != "SPT")
                            {
                                // salesEmailBody.Append("<a href=\"" + Redircturl + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible + "\">REDIRECT</a>");
                                string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));
                                salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            }
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            /*salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=6&isResponsible=" + isResponsible.ToString())) + "\">START</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible.ToString())) + "\">ARRIVAL</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=8&isResponsible=" + isResponsible.ToString())) + "\">SCHEDULE EVENT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");*/
                       // }
                    }
                }
                else if (mailType == MailType.REDIRECTED)
                {
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible + "\">DISREGARD</a>");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
                }
            }


            string contentId = Guid.NewGuid().ToString();
            string logoPath = string.Empty;
            if (Server == null)
            {
                logoPath = Path.Combine(HttpRuntime.AppDomainAppPath, "img/mainlogo.jpg");
            }
            else
            {
                logoPath = Server.MapPath("~/img/mainlogo.jpg");
            }


            salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString
               (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

            LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
            inline.ContentId = contentId;
            avHtml.LinkedResources.Add(inline);

            var message = new MailMessage();

            message.AlternateViews.Add(avHtml);

            message.IsBodyHtml = true;
            message.Body = salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();

            bool result = true;
            string mailTo = toAddress;
            string mailCC = string.Empty;
            if (!string.IsNullOrWhiteSpace(mailTo))
            {
                if (toAddress.Contains("#"))
                {
                    string[] mailCCAddress = toAddress.Split('#');

                    if (mailCCAddress.Count() > 0)
                    {
                        string[] CCAddresses = mailCCAddress[1].Split(';');
                        foreach (string address in CCAddresses)
                        {
                            if (address.ToLower().Contains("@jmsmucker.com")) continue;
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.CC.Add(new MailAddress(address));
                            }
                        }
                        string[] addresses = mailCCAddress[0].Split(';');
                        foreach (string address in addresses)
                        {
                            if (address.ToLower().Contains("@jmsmucker.com")) continue;
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.To.Add(new MailAddress(address));
                            }
                        }
                    }
                }
                else
                {
                    string[] addresses = mailTo.Split(';');
                    foreach (string address in addresses)
                    {
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            if (address.ToLower().Contains("@jmsmucker.com")) continue;

                            message.To.Add(new MailAddress(address));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(SalesEmailAddress))
                {
                    if (SalesEmailAddress.Contains(";"))
                    {
                        string[] addresses = SalesEmailAddress.Split(';');
                        foreach (string address in addresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                if (address.ToLower().Contains("@jmsmucker.com")) continue;

                                message.CC.Add(new MailAddress(address));
                            }
                        }
                    }
                    else
                    {
                        message.CC.Add(SalesEmailAddress);
                    }
                }
                if(!string.IsNullOrEmpty(esmEmailAddress) && !Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                {
                    if (esmEmailAddress.Contains(";"))
                    {
                        string[] addresses = esmEmailAddress.Split(';');
                        foreach (string address in addresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                if (address.ToLower().Contains("@jmsmucker.com")) continue;

                                message.CC.Add(new MailAddress(address));
                            }
                        }
                    }
                    else
                    {
                        message.CC.Add(esmEmailAddress);
                    }
                }
                                
                NonFBCustomer nonFBCustomer = FarmerBrothersEntitites.NonFBCustomers.Where(n => n.NonFBCustomerId == customer.PricingParentID).FirstOrDefault();                
                if(nonFBCustomer != null)
                {
                    message.CC.Clear();
                }

                message.Bcc.Add(BccEmailAddress);


                message.From = new MailAddress(fromAddress);
                message.Subject = subject;
                message.IsBodyHtml = true;
                
                if (tchView != null && tchView.FamilyAff != "SP")
                {
                    message.Priority = MailPriority.High;
                }

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = 25;

                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        public bool SendWorkOrderMail_Old(WorkOrder workOrder, string subject, string toAddress, string fromAddress, int? techId, MailType mailType, bool isResponsible, string additionalMessage, string mailFrom = "", bool isFromEmailCloserLink = false, string SalesEmailAddress = "", string esmEmailAddress = "")
        {
            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();
            int TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, workOrder.CustomerID.ToString());

            List<CustomerNotesModel> CustomerNotesResults = new List<CustomerNotesModel>();
            //int? custId = Convert.ToInt32(workOrder.CustomerID);
            //var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();

            int custId = Convert.ToInt32(workOrder.CustomerID);
            int parentId = string.IsNullOrEmpty(customer.PricingParentID) ? 0 : Convert.ToInt32(customer.PricingParentID);
            var custNotes = Utility.GetCustomreNotes(custId, parentId, FarmerBrothersEntitites);

            string IsBillable = "";
            string ServiceLevelDesc = "";
            if (!string.IsNullOrEmpty(customer.BillingCode))
            {
                IsBillable = CustomerModel.IsBillableService(customer.BillingCode, TotalCallsCount);
                ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, customer.BillingCode);
            }
            else
            {
                IsBillable = " ";
                ServiceLevelDesc = " - ";
            }

            StringBuilder salesEmailBody = new StringBuilder();

            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            string url = ConfigurationManager.AppSettings["DispatchResponseUrl"];
            string Redircturl = ConfigurationManager.AppSettings["RedirectResponseUrl"];
            string processCardurl = ConfigurationManager.AppSettings["ProcessCardUrl"];
            string Closureurl = ConfigurationManager.AppSettings["CallClosureUrl"];
            //string finalUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=@response&isResponsible=" + isResponsible.ToString()));

            salesEmailBody.Append("<a href=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;></a>");
            if (isFromEmailCloserLink)
            {
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            }
            else
            {
                if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
                {
                    if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                    {
                        TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
                       

                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=9&isResponsible=" + isResponsible.ToString())) + "\">ESM ESCALATION</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");


                        if (mailType == MailType.DISPATCH)
                        {
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        }
                        if (workOrder.WorkorderCallstatus == "Pending Acceptance" && techView.FamilyAff != "SPT")
                        {
                            string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));
                            salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        }
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=6&isResponsible=" + isResponsible.ToString())) + "\">START</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible.ToString())) + "\">ARRIVAL</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=8&isResponsible=" + isResponsible.ToString())) + "\">SCHEDULE EVENT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", processCardurl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=10&isResponsible=" + isResponsible.ToString())) + "\">PROCESS CARD</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        //}
                    }
                }
                else if (mailType == MailType.REDIRECTED)
                {
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible + "\">DISREGARD</a>");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
                }

            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
            if (tchView != null && tchView.FamilyAff.ToUpper() == "SPT")
            {
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append("Third Party Dispatch ");
                salesEmailBody.Append("</b></span>");
            }
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "TRANSMIT")
            {
                salesEmailBody.Append("<b>ADDITIONAL NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                //salesEmailBody.Append(Utility.GetStringWithNewLine(additionalMessage));
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("<BR>");
            }

            if (custNotes != null && custNotes.Count > 0)
            {
                salesEmailBody.Append("<b>CUSTOMER NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                foreach (var dbCustNotes in custNotes)
                {
                    salesEmailBody.Append("[" + dbCustNotes.UserName + "] : " + dbCustNotes.Notes + Environment.NewLine);
                }
                salesEmailBody.Append("<BR>");
            }

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "ESCALATION")
            {
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append("ESCALATION NOTES: ");
                //salesEmailBody.Append(Utility.GetStringWithNewLine(additionalMessage));
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("</b></span>");
                salesEmailBody.Append("<BR>");
            }
            salesEmailBody.Append("CALL TIME: ");
            salesEmailBody.Append(workOrder.WorkorderEntryDate);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("Work Order ID#: ");
            salesEmailBody.Append(workOrder.WorkorderID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ERF#: ");
            salesEmailBody.Append(workOrder.WorkorderErfid);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Appointment Date: ");
            salesEmailBody.Append(workOrder.AppointmentDate);
            salesEmailBody.Append("<BR>");

            WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrder.WorkorderID && (w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();
            string schedlDate = ws == null ? "" : ws.EventScheduleDate.ToString();

            if (workOrder.WorkorderCalltypeid == 1300)
            {
                Erf workorderERF = FarmerBrothersEntitites.Erfs.Where(ew => ew.ErfID == workOrder.WorkorderErfid).FirstOrDefault();
                schedlDate = workorderERF == null ? schedlDate : workorderERF.OriginalRequestedDate.ToString();
            }

            salesEmailBody.Append("Schedule Date: ");
            salesEmailBody.Append(schedlDate);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Service Level: ");
            salesEmailBody.Append(ServiceLevelDesc);
            salesEmailBody.Append("<BR>");

            string ServiceTier = customer == null ? "" : string.IsNullOrEmpty(customer.ProfitabilityTier) ? " - " : customer.ProfitabilityTier;
            string paymentTerm = customer == null ? "" : (string.IsNullOrEmpty(customer.PaymentTerm) ? "" : customer.PaymentTerm);
            string PaymentTermDesc = "";
            if (!string.IsNullOrEmpty(paymentTerm))
            {
                JDEPaymentTerm paymentDesc = FarmerBrothersEntitites.JDEPaymentTerms.Where(c => c.PaymentTerm == paymentTerm).FirstOrDefault();
                PaymentTermDesc = paymentDesc == null ? "" : paymentDesc.Description;
            }
            else
            {
                PaymentTermDesc = "";
            }

            salesEmailBody.Append("Tier: ");
            salesEmailBody.Append(ServiceTier);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Payment Terms: ");
            salesEmailBody.Append(PaymentTermDesc);
            salesEmailBody.Append("<BR>");

            AllFBStatu priority = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workOrder.PriorityCode).FirstOrDefault();
            string priorityDesc = priority == null ? "" : priority.FBStatus;

            salesEmailBody.Append("Service Priority: ");
            salesEmailBody.Append(priorityDesc);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Parent: ");
            if (customer.PricingParentID != null)
            {
                NonFBCustomer nonfbcust = FarmerBrothersEntitites.NonFBCustomers.Where(c => c.NonFBCustomerId == customer.PricingParentID).FirstOrDefault();
                string parentNum = "", ParentName = "";
                if (nonfbcust != null)
                {
                    parentNum = nonfbcust.NonFBCustomerId;
                    ParentName = nonfbcust.NonFBCustomerName;
                }
                else
                {
                    parentNum = customer.PricingParentID;
                    ParentName = customer.PricingParentDesc == null ? "" : customer.PricingParentDesc;
                }
                salesEmailBody.Append(parentNum + " " + ParentName);
            }
            else
            {
                salesEmailBody.Append("");
            }
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Billable: ");
            salesEmailBody.Append(IsBillable);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Customer PO: ");
            salesEmailBody.Append(workOrder.CustomerPO);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER INFORMATION: ");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER#: ");
            salesEmailBody.Append(workOrder.CustomerID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.CustomerName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(customer.Address1);
            salesEmailBody.Append(",");
            salesEmailBody.Append(customer.Address2);
            salesEmailBody.Append("<BR>");
            //salesEmailBody.Append(workOrder.CustomerCity);
            salesEmailBody.Append(customer.City);
            salesEmailBody.Append(",");
            //salesEmailBody.Append(workOrder.CustomerState);
            salesEmailBody.Append(customer.State);
            salesEmailBody.Append(" ");
            //salesEmailBody.Append(workOrder.CustomerZipCode);
            salesEmailBody.Append(customer.PostalCode);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.WorkorderContactName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("PHONE: ");
            salesEmailBody.Append(workOrder.WorkorderContactPhone);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("BRANCH: ");
            salesEmailBody.Append(customer.Branch);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ROUTE#: ");
            salesEmailBody.Append(customer.Route);
            salesEmailBody.Append("<BR>");
            if (workOrder.FollowupCallID == 601 || workOrder.FollowupCallID == 602)
            {
                int? followupId = workOrder.FollowupCallID;
                AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(s => s.FBStatusID == followupId).FirstOrDefault();
                if (status != null && !string.IsNullOrEmpty(status.FBStatus))
                {
                    //salesEmailBody.Append("Follow Up Reason: ");
                    //salesEmailBody.Append(status.FBStatus);
                    if (workOrder.FollowupCallID == 601)
                        salesEmailBody.Append("Customer requesting an ETA phone call within the hour");
                    else if (workOrder.FollowupCallID == 602)
                        salesEmailBody.Append("Contact Customer Within The Hour");
                    salesEmailBody.Append("<BR>");
                }
            }
            salesEmailBody.Append("<span style='color:#ff0000'><b>");
            salesEmailBody.Append("LAST SALES DATE: ");
            salesEmailBody.Append(GetCustomerById(workOrder.CustomerID).LastSaleDate);
            salesEmailBody.Append("</b></span>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("HOURS OF OPERATION: ");
            salesEmailBody.Append(workOrder.HoursOfOperation);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL CODES: ");
            salesEmailBody.Append("<BR>");

            foreach (WorkorderEquipmentRequested equipment in workOrder.WorkorderEquipmentRequesteds)
            {
                salesEmailBody.Append("EQUIPMENT TYPE: ");
                salesEmailBody.Append(equipment.Category);
                salesEmailBody.Append("<BR>");

                WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                if (callType != null)
                {
                    salesEmailBody.Append("SERVICE CODE: ");
                    salesEmailBody.Append(callType.CallTypeID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(callType.Description);
                    salesEmailBody.Append("<BR>");
                }
                Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                if (symptom != null)
                {
                    salesEmailBody.Append("SYMPTOM: ");
                    salesEmailBody.Append(symptom.SymptomID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(symptom.Description);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("LOCATION: ");
                salesEmailBody.Append(equipment.Location);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("SERIAL NUMBER: ");
                salesEmailBody.Append(equipment.SerialNumber);

                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL NOTES: ");
            salesEmailBody.Append("<BR>");
            IEnumerable<NotesHistory> histories = workOrder.NotesHistories.Where(n => n.AutomaticNotes == 0).OrderByDescending(n => n.EntryDate);

            foreach (NotesHistory history in histories)
            {
                //Remove Redirected/Rejected notes for 3rd Party Tech
                if (tchView != null && tchView.FamilyAff.ToUpper() == "SPT")
                {
                    if (history != null && history.Notes != null)
                    {
                        if (history.Notes.ToLower().Contains("redirected") || history.Notes.ToLower().Contains("rejected") || history.Notes.ToLower().Contains("declined"))
                        {
                            continue;
                        }
                    }
                }

                salesEmailBody.Append(history.UserName);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.EntryDate);
                salesEmailBody.Append(" ");
                //salesEmailBody.Append(history.Notes.Replace("\\n", " ").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\r", " "));
                salesEmailBody.Append(history.Notes);
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("SERVICE HISTORY:");
            salesEmailBody.Append("<BR>");

            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            /*IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
                Where(w => w.CustomerID == workOrder.CustomerID && (DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) < 90
                              && DbFunctions.DiffDays(w.WorkorderEntryDate, currentTime) > -90));*/

            IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
                Where(w => w.CustomerID == workOrder.CustomerID).OrderByDescending(ed => ed.WorkorderEntryDate).Take(3);

            foreach (WorkOrder previousWorkOrder in previousWorkOrders)
            {
                salesEmailBody.Append("Work Order ID#: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderID);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("ENTRY DATE: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderEntryDate);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("STATUS: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderCallstatus);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("CALL CODES: ");
                salesEmailBody.Append("<BR>");

                foreach (WorkorderEquipment equipment in previousWorkOrder.WorkorderEquipments)
                {
                    salesEmailBody.Append("MAKE: ");
                    salesEmailBody.Append(equipment.Manufacturer);
                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("MODEL#: ");
                    salesEmailBody.Append(equipment.Model);
                    salesEmailBody.Append("<BR>");

                    WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                    if (callType != null)
                    {
                        salesEmailBody.Append("SERVICE CODE: ");
                        salesEmailBody.Append(callType.CallTypeID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(callType.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                    if (symptom != null)
                    {
                        salesEmailBody.Append("SYMPTOM: ");
                        salesEmailBody.Append(symptom.SymptomID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(symptom.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    salesEmailBody.Append("Location: ");
                    salesEmailBody.Append(equipment.Location);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<a href=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;></a>");
            if (isFromEmailCloserLink)
            {
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

            }
            else
            {
                if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
                {
                    if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                    {
                        TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();

                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=9&isResponsible=" + isResponsible.ToString())) + "\">ESM ESCALATION</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                        if (mailType == MailType.DISPATCH)
                        {
                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        }
                        if (workOrder.WorkorderCallstatus == "Pending Acceptance" && techView.FamilyAff != "SPT")
                        {
                            // salesEmailBody.Append("<a href=\"" + Redircturl + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible + "\">REDIRECT</a>");
                            string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));
                            salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        }
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=6&isResponsible=" + isResponsible.ToString())) + "\">START</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible.ToString())) + "\">ARRIVAL</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=8&isResponsible=" + isResponsible.ToString())) + "\">SCHEDULE EVENT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", processCardurl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=10&isResponsible=" + isResponsible.ToString())) + "\">PROCESS CARD</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                        // }
                    }
                }
                else if (mailType == MailType.REDIRECTED)
                {
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible + "\">DISREGARD</a>");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
                }
            }


            string contentId = Guid.NewGuid().ToString();
            string logoPath = string.Empty;
            if (Server == null)
            {
                logoPath = Path.Combine(HttpRuntime.AppDomainAppPath, "img/mainlogo.jpg");
            }
            else
            {
                logoPath = Server.MapPath("~/img/mainlogo.jpg");
            }


            salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString
               (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

            LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
            inline.ContentId = contentId;
            avHtml.LinkedResources.Add(inline);

            var message = new MailMessage();

            message.AlternateViews.Add(avHtml);

            message.IsBodyHtml = true;
            message.Body = salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();


            bool result = true;
            string mailTo = toAddress;
            string mailCC = string.Empty;
            string ToAddr = string.Empty;
            string CcAddr = string.Empty;
            if (!string.IsNullOrWhiteSpace(mailTo))
            {
                if (toAddress.Contains("#"))
                {
                    string[] mailCCAddress = toAddress.Split('#');
                    ToAddr = mailCCAddress[0];
                    CcAddr = mailCCAddress[1];

                    if (mailCCAddress.Count() > 0)
                    {
                        string[] CCAddresses = mailCCAddress[1].Split(';');
                        foreach (string address in CCAddresses)
                        {
                            if (address.ToLower().Contains("@jmsmucker.com")) continue;
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.CC.Add(new MailAddress(address));
                            }
                        }
                        string[] addresses = mailCCAddress[0].Split(';');
                        foreach (string address in addresses)
                        {
                            if (address.ToLower().Contains("@jmsmucker.com")) continue;
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.To.Add(new MailAddress(address));
                            }
                        }
                    }
                }
                else
                {
                    string[] addresses = mailTo.Split(';');
                    ToAddr = mailTo;
                    foreach (string address in addresses)
                    {
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            if (address.ToLower().Contains("@jmsmucker.com")) continue;

                            message.To.Add(new MailAddress(address));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(SalesEmailAddress))
                {
                    CcAddr = SalesEmailAddress;
                    if (SalesEmailAddress.Contains(";"))
                    {
                        string[] addresses = SalesEmailAddress.Split(';');
                        foreach (string address in addresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                if (address.ToLower().Contains("@jmsmucker.com")) continue;

                                message.CC.Add(new MailAddress(address));
                            }
                        }
                    }
                    else
                    {
                        message.CC.Add(SalesEmailAddress);
                    }
                }
                //if (!string.IsNullOrEmpty(esmEmailAddress) && !Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                //{
                //    if (esmEmailAddress.Contains(";"))
                //    {
                //        string[] addresses = esmEmailAddress.Split(';');
                //        foreach (string address in addresses)
                //        {
                //            if (!string.IsNullOrWhiteSpace(address))
                //            {
                //                if (address.ToLower().Contains("@jmsmucker.com")) continue;

                //                message.CC.Add(new MailAddress(address));
                //            }
                //        }
                //    }
                //    else
                //    {
                //        message.CC.Add(esmEmailAddress);
                //    }
                //}

                NonFBCustomer nonFBCustomer = FarmerBrothersEntitites.NonFBCustomers.Where(n => n.NonFBCustomerId == customer.PricingParentID).FirstOrDefault();
                if (nonFBCustomer != null)
                {
                    message.CC.Clear();
                }

                //message.Bcc.Add(BccEmailAddress);


                message.From = new MailAddress(fromAddress);
                //message.ReplyTo = new MailAddress(ConfigurationManager.AppSettings["DispatchMailReplyToAddress"]);
                message.ReplyToList.Add(new MailAddress(ConfigurationManager.AppSettings["DispatchMailReplyToAddress"], "ReviveService"));
                message.Subject = subject;
                message.IsBodyHtml = true;

                if (tchView != null && tchView.FamilyAff != "SP")
                {
                    message.Priority = MailPriority.High;
                }


                using (var smtp = new SmtpClient())
                {
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = 25;

                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        private int NotifySales(WorkorderManagementModel workOrderManagementModel, out WorkOrder workOrder)
        {
            int returnValue = -1;
            StringBuilder salesEmailBody = new StringBuilder();
            workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID).FirstOrDefault();
            if (workOrder != null)
            {
                TimeZoneInfo newTimeZoneInfo = null;
                Utility.GetCustomerTimeZone(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);

                DateTime currentTime = Utility.GetCurrentTime(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);

                int salesNotificationId = Convert.ToInt32(workOrderManagementModel.SalesNotificationCode);
                AllFBStatu reason = FarmerBrothersEntitites.AllFBStatus.Where(al => al.FBStatusID == salesNotificationId).FirstOrDefault();
                if (reason != null)
                {
                    WorkorderReasonlog workOrderLog = new WorkorderReasonlog()
                    {
                        EntryDate = currentTime,
                        Notes = workOrderManagementModel.SalesNotificationNotes,
                        ReasonFor = reason.FBStatus,
                        Reasonid = reason.FBStatusID,
                        LogDescription = reason.FBStatus,
                        WorkorderID = workOrder.WorkorderID
                    };
                    FarmerBrothersEntitites.WorkorderReasonlogs.Add(workOrderLog);

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = "Sales Notification Sent!",
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 1
                    };

                    workOrder.NotesHistories.Add(notesHistory);

                    returnValue = FarmerBrothersEntitites.SaveChanges();
                    if (returnValue > 0)
                    {
                        StringBuilder subject = new StringBuilder();
                        subject.Append("Notify Sales - WO: ");
                        subject.Append(workOrder.WorkorderID);
                        subject.Append(" ST: ");
                        subject.Append(workOrder.CustomerState);
                        subject.Append(" Call Type: ");
                        subject.Append(workOrder.WorkorderCalltypeDesc);

                        StringBuilder additionalInfo = new StringBuilder();
                        additionalInfo.Append("Notify Sales Reason: ");
                        additionalInfo.Append(reason.FBStatus);
                        additionalInfo.Append("<BR/>");
                        additionalInfo.Append("Notify Sales Notes: ");
                        additionalInfo.Append(workOrderManagementModel.SalesNotificationNotes);
                        additionalInfo.Append("<BR/>");

                        SendWorkOrderMail(workOrder, subject.ToString(), ConfigurationManager.AppSettings["SalesEmailAddress"].ToString(), ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"], null, MailType.SALESNOTIFICATION, false, additionalInfo.ToString());
                    }
                }
            }
            return returnValue;
        }

        #endregion

        #region Grid Edit Functions

        public JsonResult GetSolutions(int calltypeId)
        {
            IEnumerable<CallTypeSymptomSolutionMaster> masters = FarmerBrothersEntitites.CallTypeSymptomSolutionMasters.Where(css => css.CallTypeID == calltypeId && css.Active == 1);
            var data = new List<Solution>();
            foreach (CallTypeSymptomSolutionMaster master in masters)
            {
                if (data.Find(d => d.SolutionId == master.SolutionID) == null)
                {
                    Solution solution = FarmerBrothersEntitites.Solutions.Where(s => s.SolutionId == master.SolutionID && s.Active == 1).FirstOrDefault();
                    data.Add(solution);
                }
            }

            data = data.OrderBy(s => s.Sequence).ToList();

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult EquipmentDetailsInsert(WorkOrderManagementEquipmentModel value)
        {
            IList<WorkOrderManagementEquipmentModel> workorderEquipments = TempData["WorkOrderEquipments"] as IList<WorkOrderManagementEquipmentModel>;
            if (workorderEquipments == null)
            {
                workorderEquipments = new List<WorkOrderManagementEquipmentModel>();
            }

            if (TempData["AssetId"] != null)
            {
                int assetId = Convert.ToInt32(TempData["AssetId"]);
                value.AssetId = assetId + 1;
                TempData["AssetId"] = assetId + 1;
            }
            else
            {
                value.AssetId = 1;
                TempData["AssetId"] = 1;
            }

            workorderEquipments.Add(value);
            TempData["WorkOrderEquipments"] = workorderEquipments;
            TempData.Keep("WorkOrderEquipments");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EquipmentDetailsUpdate(WorkOrderManagementEquipmentModel value)
        {
            IList<WorkOrderManagementEquipmentModel> workorderEquipments = TempData["WorkOrderEquipments"] as IList<WorkOrderManagementEquipmentModel>;
            WorkOrderManagementEquipmentModel workorderEquipment = workorderEquipments.Where(we => we.AssetId == value.AssetId).FirstOrDefault();

            if (workorderEquipment != null)
            {
                workorderEquipment.CallTypeID = value.CallTypeID;
                workorderEquipment.Category = value.Category;
                workorderEquipment.CatelogID = value.CatelogID;
                workorderEquipment.Location = value.Location;
                workorderEquipment.SerialNumber = value.SerialNumber;
                workorderEquipment.SerialNumberManual = value.SerialNumberManual;
                workorderEquipment.Model = value.Model;
                workorderEquipment.SymptomID = value.SymptomID;
            }

            TempData["WorkOrderEquipments"] = workorderEquipments;
            TempData.Keep("WorkOrderEquipments");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EquipmentDetailsDelete(int key)
        {
            IList<WorkOrderManagementEquipmentModel> workorderEquipments = TempData["WorkOrderEquipments"] as IList<WorkOrderManagementEquipmentModel>;
            WorkOrderManagementEquipmentModel workorderEquipment = workorderEquipments.Where(we => we.AssetId == key).FirstOrDefault();
            workorderEquipments.Remove(workorderEquipment);
            TempData["WorkOrderEquipments"] = workorderEquipments;
            TempData.Keep("WorkOrderEquipments");

            return Json(workorderEquipments, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NonSerializedUpdate(WorkOrderManagementNonSerializedModel value)
        {
            IList<WorkOrderManagementNonSerializedModel> nonSerializedItems = TempData["NonSerialized"] as IList<WorkOrderManagementNonSerializedModel>;
            WorkOrderManagementNonSerializedModel nonSerializedItem = nonSerializedItems.Where(n => n.NSerialid == value.NSerialid).FirstOrDefault();

            if (nonSerializedItem != null)
            {
                nonSerializedItem.Catalogid = value.Catalogid;
                nonSerializedItem.ManufNumber = value.ManufNumber;
                nonSerializedItem.OrigOrderQuantity = value.OrigOrderQuantity;
            }

            TempData["NonSerialized"] = nonSerializedItems;
            TempData.Keep("NonSerialized");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NonSerializedInsert(WorkOrderManagementNonSerializedModel value)
        {
            IList<WorkOrderManagementNonSerializedModel> nonSerializedItems = TempData["NonSerialized"] as IList<WorkOrderManagementNonSerializedModel>;
            if (nonSerializedItems == null)
            {
                nonSerializedItems = new List<WorkOrderManagementNonSerializedModel>();
            }


            if (TempData["NSerialid"] != null)
            {
                int assetId = Convert.ToInt32(TempData["NSerialid"]);
                value.NSerialid = assetId + 1;
                TempData["NSerialid"] = assetId + 1;
            }
            else
            {
                value.NSerialid = 1;
                TempData["NSerialid"] = 1;
            }

            nonSerializedItems.Add(value);
            TempData["NonSerialized"] = nonSerializedItems;
            TempData.Keep("NonSerialized");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NonSerializedDelete(int key)
        {
            IList<WorkOrderManagementNonSerializedModel> nonSerializedItems = TempData["NonSerialized"] as IList<WorkOrderManagementNonSerializedModel>;
            WorkOrderManagementNonSerializedModel nonSerializedItem = nonSerializedItems.Where(n => n.NSerialid == key).FirstOrDefault();
            nonSerializedItems.Remove(nonSerializedItem);
            TempData["NonSerialized"] = nonSerializedItems;
            TempData.Keep("NonSerialized");
            return Json(nonSerializedItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClosureEquipmentsUpdate(WorkOrderManagementEquipmentModel value)
        {
            IList<WorkOrderManagementEquipmentModel> workorderEquipments;
            if (TempData["WorkOrderEquipments"] != null)
            {
                workorderEquipments = TempData["WorkOrderEquipments"] as IList<WorkOrderManagementEquipmentModel>;
            }
            else
            {
                workorderEquipments = new List<WorkOrderManagementEquipmentModel>();
            }

            WorkOrderManagementEquipmentModel workorderEquipment = workorderEquipments.Where(we => we.AssetId == value.AssetId).FirstOrDefault();

            if (workorderEquipment != null)
            {
                workorderEquipment.CallTypeID = value.CallTypeID;
                workorderEquipment.Category = value.Category;
                workorderEquipment.Manufacturer = value.Manufacturer;
                workorderEquipment.Model = value.Model;
                workorderEquipment.Location = value.Location;
                workorderEquipment.SerialNumber = value.SerialNumber;
                workorderEquipment.SerialNumberManual = value.SerialNumberManual;
                workorderEquipment.Solution = value.Solution;
            }

            TempData["WorkOrderEquipments"] = workorderEquipments;
            TempData.Keep("WorkOrderEquipments");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClosureEquipmentsInsert(WorkOrderManagementEquipmentModel value)
        {
            IList<WorkOrderManagementEquipmentModel> workorderEquipments = TempData["WorkOrderEquipments"] as IList<WorkOrderManagementEquipmentModel>;
            if (workorderEquipments == null)
            {
                workorderEquipments = new List<WorkOrderManagementEquipmentModel>();
            }

            if (TempData["AssetId"] != null)
            {
                int assetId = Convert.ToInt32(TempData["AssetId"]);
                value.AssetId = assetId + 1;
                TempData["AssetId"] = assetId + 1;
            }
            else
            {
                value.AssetId = 1;
                TempData["AssetId"] = 1;
            }

            workorderEquipments.Add(value);
            TempData["WorkOrderEquipments"] = workorderEquipments;
            TempData.Keep("WorkOrderEquipments");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClosureEquipmentsDelete(int key)
        {
            IList<WorkOrderManagementEquipmentModel> workorderEquipments = TempData["WorkOrderEquipments"] as IList<WorkOrderManagementEquipmentModel>;
            WorkOrderManagementEquipmentModel workorderEquipment = workorderEquipments.Where(we => we.AssetId == key).FirstOrDefault();
            workorderEquipments.Remove(workorderEquipment);
            TempData["WorkOrderEquipments"] = workorderEquipments;
            TempData.Keep("WorkOrderEquipments");
            return Json(workorderEquipments, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WorkorderPartsUpdate(WorkOrderPartModel value)
        {
            IList<WorkOrderPartModel> workOrderParts = TempData["WorkOrderParts"] as IList<WorkOrderPartModel>;
            WorkOrderPartModel workorderPart = workOrderParts.Where(we => we.PartsIssueid == value.PartsIssueid).FirstOrDefault();

            if (workorderPart != null)
            {
                workorderPart.Description = value.Description;
                workorderPart.Issue = value.Issue;
                workorderPart.Manufacturer = value.Manufacturer;
                workorderPart.Quantity = value.Quantity;
                workorderPart.Sku = value.Sku;
            }

            TempData["WorkOrderParts"] = workOrderParts;
            TempData.Keep("WorkOrderParts");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateWorkorderPartsGridData(UpdateWorkorderPartsModel PartsItems)
        {
            JsonResult jsonResult = new JsonResult();
            List<WorkOrderPartModel> ResultSet = new List<WorkOrderPartModel>();

            //foreach (BillingModel item in BillingItems.BillingList)
            {

                WorkOrderPartModel tmpData = PartsItems.PartsList.Where(b => b.PartsIssueid == PartsItems.PartsIssueid).FirstOrDefault();
                if (PartsItems.UpdateType.ToLower() == "save")
                {                   
                    if (tmpData == null)
                    {
                        int rowId = 0;
                        if (TempData["PartsIssueid"] != null)
                        {
                            rowId = Convert.ToInt32(TempData["PartsIssueid"]);
                            rowId = rowId + 1;
                        }
                        else
                        {
                            rowId = 1;
                        }
                        TempData["PartsIssueid"] = rowId;

                        WorkOrderPartModel newItem = new WorkOrderPartModel();
                        newItem.PartsIssueid = rowId;
                        newItem.PartReplenish = PartsItems.PartReplenish;
                        newItem.Sku = PartsItems.Sku;
                        newItem.skuCost = PartsItems.skuCost;
                        newItem.Description = PartsItems.Description;
                        newItem.Manufacturer = PartsItems.Manufacturer;
                        newItem.partsTotal = PartsItems.partsTotal;
                        newItem.Quantity = PartsItems.Quantity;

                        PartsItems.PartsList.Add(newItem);
                    }
                    else
                    {
                        if (tmpData.PartsIssueid == 0 || tmpData.PartsIssueid == null)
                        {
                            int rowId = 0;
                            if (TempData["PartsIssueid"] != null)
                            {
                                rowId = Convert.ToInt32(TempData["PartsIssueid"]);
                                rowId = rowId + 1;
                            }
                            else
                            {
                                rowId = 1;
                            }
                            TempData["PartsIssueid"] = rowId;
                            tmpData.PartsIssueid = rowId;
                        }
                        tmpData.PartReplenish = PartsItems.PartReplenish;
                        tmpData.Sku = PartsItems.Sku;
                        tmpData.skuCost = PartsItems.skuCost;
                        tmpData.Description = PartsItems.Description;
                        tmpData.Manufacturer = PartsItems.Manufacturer;
                        tmpData.partsTotal = PartsItems.partsTotal;
                        tmpData.Quantity = PartsItems.Quantity;
                    }
                }
                else if (PartsItems.UpdateType.ToLower() == "delete")
                {
                    //WorkOrderPartModel tmpDelData = PartsItems.PartsList.Where(b => b.Sku == PartsItems.Sku && b.Quantity == PartsItems.Quantity && b.Description == PartsItems.Description && b.Manufacturer == PartsItems.Manufacturer).FirstOrDefault();
                    PartsItems.PartsList.Remove(tmpData);
                }
            }           

            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = PartsItems.PartsList };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult WorkorderPartsInsert(WorkOrderPartModel value)
        {
            IList<WorkOrderPartModel> workOrderParts = TempData["WorkOrderParts"] as IList<WorkOrderPartModel>;

            if (workOrderParts == null)
            {
                workOrderParts = new List<WorkOrderPartModel>();
            }

            if (TempData["PartsIssueid"] != null)
            {
                int partsIssueId = Convert.ToInt32(TempData["PartsIssueid"]);
                value.PartsIssueid = partsIssueId + 1;
                TempData["PartsIssueid"] = partsIssueId + 1;
            }
            else
            {
                value.PartsIssueid = 1;
                TempData["PartsIssueid"] = 1;
            }

            workOrderParts.Add(value);
            TempData["WorkOrderParts"] = workOrderParts;
            TempData.Keep("WorkOrderParts");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WorkorderPartsDelete(int key)
        {
            IList<WorkOrderPartModel> workOrderParts = TempData["WorkOrderParts"] as IList<WorkOrderPartModel>;
            WorkOrderPartModel workorderPart = workOrderParts.Where(we => we.PartsIssueid == key).FirstOrDefault();
            workOrderParts.Remove(workorderPart);
            TempData["WorkOrderParts"] = workOrderParts;
            TempData.Keep("WorkOrderParts");
            return Json(workOrderParts, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Dispatch

        [HttpPost]
        public JsonResult GetTechnician(string[] branchIds, int workOrderId, string[] BranchNumber)
        {
            IList<TechModel> technicianList = new List<TechModel>();
            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            if (workOrder != null)
            {
                if (branchIds != null)
                {
                    foreach (string branchId in branchIds)
                    {
                        int id = Convert.ToInt32(branchId);
                        IEnumerable<TechHierarchyView> technicians = Utility.GetTechDataByServiceCenterId(FarmerBrothersEntitites, id);
                        foreach (TechHierarchyView techView in technicians)
                        {

                            TechModel techModel = new TechModel(techView);
                            int techId = Convert.ToInt32(techView.TechID);
                            WorkorderSchedule workorderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.Techid == techId).FirstOrDefault();
                            if (workorderSchedule != null)
                            {
                                techModel.AssignedStatus = workorderSchedule.AssignedStatus;
                                techModel.LastCommunication = workorderSchedule.ModifiedScheduleDate.HasValue ? workorderSchedule.ModifiedScheduleDate.Value.ToString("MM/dd/yyyy hh:mm tt") : " ";
                                techModel.EventScheduleDate = workorderSchedule.EventScheduleDate.HasValue ? workorderSchedule.EventScheduleDate.Value.ToString("MM/dd/yyyy hh:mm tt") : " ";
                            }
                            technicianList.Add(techModel);
                        }
                    }
                }
            }


            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = technicianList };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult DispatchMail(int workOrderId, int techId, bool isResponsible, List<String> notes, bool IsAutoDispatched, bool isFromAutoDispatch = true)
        {
            int returnValue = -1;
            TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, techId);
            StringBuilder salesEmailBody = new StringBuilder();
            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            string workOrderStatus = "";
            string addtionalNotes = string.Empty;

            DateTime currentTime = techHierarchyView == null ? DateTime.Now : Utility.GetCurrentTime(techHierarchyView.TechZip, FarmerBrothersEntitites);

            string message = string.Empty;
            string redirectUrl = string.Empty;

            if (workOrder != null)
            {
                if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0
                && string.Compare(workOrder.WorkorderCallstatus, "Invoiced", true) != 0
                && string.Compare(workOrder.WorkorderCallstatus, "Completed", true) != 0
                && string.Compare(workOrder.WorkorderCallstatus, "Attempting", true) != 0)
                {
                    if (isResponsible == true)
                    {
                        UpdateTechAssignedStatus(techId, workOrder, "Sent", 0, -1);
                    }
                    else
                    {
                        UpdateTechAssignedStatus(techId, workOrder, "Sent", -1, 0);
                    }

                    StringBuilder subject = new StringBuilder();
                    AllFBStatu priority = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workOrder.PriorityCode).First();

                    if (priority.FBStatus.Contains("critical"))
                    //if (workOrder.PriorityCode == 1 || workOrder.PriorityCode == 2 || workOrder.PriorityCode == 3 || workOrder.PriorityCode == 4)
                    {
                        subject.Append("CRITICAL WO: ");
                    }
                    else
                    {
                        subject.Append("WO: ");
                    }

                    subject.Append(workOrder.WorkorderID);
                    subject.Append(" Customer: ");
                    subject.Append(workOrder.CustomerName);
                    subject.Append(" ST: ");
                    subject.Append(workOrder.CustomerState);
                    subject.Append(" Call Type: ");
                    subject.Append(workOrder.WorkorderCalltypeDesc);

                    string emailAddress = string.Empty;
                    string salesEmailAddress = string.Empty;
                    string esmEmailAddress = string.Empty;
                    int userId = System.Web.HttpContext.Current.Session["UserId"] != null ? (int)System.Web.HttpContext.Current.Session["UserId"] : 0;
                    TECH_HIERARCHY techView = GetTechById(techId);

                    Contact customer = FarmerBrothersEntitites.Contacts.Where(cont => cont.ContactID == workOrder.CustomerID).FirstOrDefault();

                    if(techId == 0)
                    {
                           emailAddress = ConfigurationManager.AppSettings["MikeEmailId"] + ";" + ConfigurationManager.AppSettings["DarrylEmailId"]; 
                    }
                    //This is only for crytal
                    if (techId == Convert.ToInt32(ConfigurationManager.AppSettings["MAITestDispatch"]))
                    {
                        emailAddress = ConfigurationManager.AppSettings["CrystalEmailId"];
                    }
                    else if (techId == Convert.ToInt32(ConfigurationManager.AppSettings["MikeTestTechId"]))
                    {
                        emailAddress = ConfigurationManager.AppSettings["MikeEmailId"];
                    }
                    else
                    {
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                        {
                            emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                        }
                        else if (techView != null)
                        {
                            if (!string.IsNullOrEmpty(techView.RimEmail))
                            {
                                emailAddress = techView.RimEmail;
                            }

                            if (!string.IsNullOrEmpty(techView.EmailCC))
                            {
                                emailAddress += "#" + techView.EmailCC;
                            }
                        }
                        else
                        {
                            emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                        }
                    }

                    if (customer != null)
                    {
                        if (!string.IsNullOrEmpty(customer.SalesEmail))
                        {
                            salesEmailAddress = customer.SalesEmail;
                        }
                    }

                    ESMCCMRSMEscalation esmEscalation = FarmerBrothersEntitites.ESMCCMRSMEscalations.Where(e => e.ZIPCode == workOrder.CustomerZipCode).FirstOrDefault();
                    if (esmEscalation != null && !string.IsNullOrEmpty(esmEscalation.ESMEmail))
                    {
                        esmEmailAddress = esmEscalation.ESMEmail;
                    }

                    if (!string.IsNullOrWhiteSpace(emailAddress))
                    {

                        foreach (string note in notes)
                        {
                            string trNotes = note.Replace("\"", "").Replace("[", "").Replace("]", "");
                            if (!string.IsNullOrEmpty(trNotes))
                            {
                                NotesHistory notesHistory = new NotesHistory()
                                {
                                    AutomaticNotes = 0,
                                    EntryDate = currentTime,
                                    Notes = trNotes,
                                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                    UserName = UserName,
                                    WorkorderID = workOrder.WorkorderID,
                                    isDispatchNotes = 1
                                };
                                addtionalNotes = trNotes;
                                FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                            }

                        }

                        bool result = SendWorkOrderMail(workOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], techId, MailType.DISPATCH, isResponsible, addtionalNotes,  "TRANSMIT", false, salesEmailAddress, esmEmailAddress);
                        if (result == true)
                        {
                            if (techHierarchyView != null)
                            {
                                workOrder.ResponsibleTechid = techHierarchyView.TechID;
                                workOrder.ResponsibleTechName = techHierarchyView.PreferredProvider;
                            }

                            workOrder.WorkorderModifiedDate = currentTime;
                            workOrder.ModifiedUserName = UserName;
                        }
                        returnValue = FarmerBrothersEntitites.SaveChanges();

                    }

                    workOrderStatus = workOrder.WorkorderCallstatus;

                    if (IsAutoDispatched == true)
                    {
                        AgentDispatchLog autodispatchLog = new AgentDispatchLog()
                        {
                            TDate = currentTime,
                            UserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = string.IsNullOrEmpty(UserName) ? (string.IsNullOrEmpty(CustomerUserName) ? UserName : CustomerUserName) : UserName,
                            WorkorderID = workOrder.WorkorderID
                        };
                        FarmerBrothersEntitites.AgentDispatchLogs.Add(autodispatchLog);
                        FarmerBrothersEntitites.SaveChanges();
                    }
                }


            }

            if (isFromAutoDispatch)
            {
                //redirectUrl = new UrlHelper(Request.RequestContext).Action("WorkorderManagement", "Workorder", new { customerId = workOrder.CustomerID, workOrderId = workOrder.WorkorderID });
                redirectUrl = new UrlHelper(Request.RequestContext).Action("WorkorderSearch", "Workorder", new { @IsBack = 1 });
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, returnValue = returnValue > 0 ? 1 : 0, WorkorderCallstatus = workOrderStatus, Url = redirectUrl, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetPartsForAsset(int? assetId)
        {
            IEnumerable<WorkorderPart> workorderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.AssetID == assetId);
            var data = new List<WorkorderPart>();
            if (assetId != null)
            {
                foreach (WorkorderPart workOrderPart in workorderParts)
                {
                    Sku skuItem = FarmerBrothersEntitites.Skus.Where(s => s.Sku1 == workOrderPart.Sku).FirstOrDefault();
                    decimal skuCost = 0;
                    if(skuItem != null)
                    {
                        skuCost = skuItem.SKUCost == null ? 0 : Convert.ToDecimal(skuItem.SKUCost);
                    }

                    data.Add(new WorkorderPart() { Quantity = workOrderPart.Quantity, Manufacturer = workOrderPart.Manufacturer != null ? workOrderPart.Manufacturer.Trim() : "", Sku = workOrderPart.Sku, Description = workOrderPart.Description, PartsIssueid = workOrderPart.PartsIssueid, StandardCost = skuCost, PartReplenish = workOrderPart.PartReplenish });
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public bool UpdateTechAssignedStatus(int techId, WorkOrder workOrder, string assignedStatus, int isResponsible = -1, int isAssist = -1)
        {
            bool result = false;
            WorkorderSchedule techWorkOrderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.Techid == techId).FirstOrDefault();

            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            if (isResponsible == -1 && isAssist == -1)
            {
                if (string.Compare(techWorkOrderSchedule.AssignedStatus, "Sent", 0) == 0)
                {
                    string notesMessage = "";
                    if (string.Compare(assignedStatus, "Accepted", 0) == 0)
                    {
                        notesMessage = "Work order Accepted by " + techWorkOrderSchedule.TechName;

                        if (techWorkOrderSchedule.PrimaryTech >= 0)
                        {
                            techWorkOrderSchedule.PrimaryTech = 1;
                        }
                        else if (techWorkOrderSchedule.AssistTech > 0)
                        {
                            //techWorkOrderSchedule.AssistTech = 1;
                        }
                        techWorkOrderSchedule.EntryDate = currentTime;
                        techWorkOrderSchedule.ScheduleDate = currentTime;
                        techWorkOrderSchedule.ModifiedScheduleDate = currentTime;
                        techWorkOrderSchedule.ScheduleUserid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234;
                    }
                    else if (string.Compare(assignedStatus, "Declined", true) == 0)
                    {
                        notesMessage = "Work order Rejected by " + techWorkOrderSchedule.TechName;
                    }
                    techWorkOrderSchedule.AssignedStatus = assignedStatus;

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = notesMessage,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 1
                    };
                    workOrder.NotesHistories.Add(notesHistory);
                    result = true;
                }
            }

            if (isResponsible >= 0)
            {
                TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, techId);

                //Responsible tech dispatch
                if (techWorkOrderSchedule != null)
                {
                    techWorkOrderSchedule.PrimaryTech = Convert.ToInt16(isResponsible);
                    techWorkOrderSchedule.AssignedStatus = assignedStatus;
                    techWorkOrderSchedule.ModifiedScheduleDate = currentTime;
                    techWorkOrderSchedule.ScheduleUserid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234;
                }
                else
                {
                    IndexCounter scheduleCounter = Utility.GetIndexCounter("ScheduleID", 1);
                    scheduleCounter.IndexValue++;
                    //FarmerBrothersEntitites.Entry(scheduleCounter).State = System.Data.Entity.EntityState.Modified;

                    WorkorderSchedule newworkOrderSchedule = new WorkorderSchedule()
                    {
                        Scheduleid = scheduleCounter.IndexValue.Value,
                        Techid = Convert.ToInt32(techHierarchyView.TechID),
                        TechName = techHierarchyView.PreferredProvider,
                        WorkorderID = workOrder.WorkorderID,
                        TechPhone = techHierarchyView.AreaCode + techHierarchyView.ProviderPhone,
                        ServiceCenterName = techHierarchyView.BranchName,
                        ServiceCenterID = Convert.ToInt32(techHierarchyView.TechID),
                        FSMName = techHierarchyView.DSMName,
                        FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>(),
                        EntryDate = currentTime,
                        ScheduleDate = currentTime,
                        TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"],

                        PrimaryTech = Convert.ToInt16(isResponsible),
                        AssistTech = -1,
                        AssignedStatus = assignedStatus,
                        ModifiedScheduleDate = currentTime,
                        ScheduleUserid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234
                    };



                    workOrder.WorkorderSchedules.Add(newworkOrderSchedule);

                }

                bool redirected = false;
                string oldTechName = string.Empty;
                IEnumerable<WorkorderSchedule> primaryTechSchedules = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0);
                foreach (WorkorderSchedule workOrderSchedule in primaryTechSchedules)
                {
                    if ((string.Compare(workOrderSchedule.AssignedStatus, "Sent", true) == 0
                        || string.Compare(workOrderSchedule.AssignedStatus, "Accepted", true) == 0
                        || string.Compare(workOrderSchedule.AssignedStatus, "Scheduled", true) == 0)
                        && workOrderSchedule.Techid != techId)
                    {
                        redirected = true;
                        workOrderSchedule.AssignedStatus = "Redirected";
                        workOrderSchedule.PrimaryTech = -1;
                        workOrderSchedule.ModifiedScheduleDate = currentTime;
                        oldTechName = workOrderSchedule.TechName;
                        if (workOrderSchedule.Techid != techId)
                        {
                            StringBuilder subject = new StringBuilder();

                            subject.Append("Call has been redirected! WO: ");
                            subject.Append(workOrder.WorkorderID);
                            subject.Append(" Customer: ");
                            subject.Append(workOrder.CustomerName);
                            subject.Append(" ST: ");
                            subject.Append(workOrder.CustomerState);
                            subject.Append(" Call Type: ");
                            subject.Append(workOrder.WorkorderCalltypeDesc);

                            string emailAddress = string.Empty;
                            string salesEmailAddress = string.Empty;

                            TECH_HIERARCHY techView = GetTechById(workOrderSchedule.Techid);

                            //This is only for crytal
                            if (techId == Convert.ToInt32(ConfigurationManager.AppSettings["MAITestDispatch"]))
                            {
                                emailAddress = ConfigurationManager.AppSettings["CrystalEmailId"];
                            }
                            else if (techId == Convert.ToInt32(ConfigurationManager.AppSettings["MikeTestTechId"]))
                            {
                                emailAddress = ConfigurationManager.AppSettings["MikeEmailId"];
                            }
                            else
                            {
                                if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                                {
                                    emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                                }
                                else if (techView != null)
                                {
                                    if (!string.IsNullOrEmpty(techView.RimEmail))
                                    {
                                        emailAddress = techView.RimEmail;
                                    }

                                    if (!string.IsNullOrEmpty(techView.EmailCC))
                                    {
                                        emailAddress += "#" + techView.EmailCC;
                                    }
                                }
                                else
                                {
                                    emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                                }
                            }

                            Contact customer = FarmerBrothersEntitites.Contacts.Where(cont => cont.ContactID == workOrder.CustomerID).FirstOrDefault();
                            if (!string.IsNullOrEmpty(customer.SalesEmail))
                            {
                                salesEmailAddress = customer.SalesEmail;
                            }

                            if (!string.IsNullOrWhiteSpace(emailAddress))
                            {
                                SendWorkOrderMail(workOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], workOrderSchedule.Techid, MailType.REDIRECTED, false, "This Work Order has been redirected!", string.Empty, false, salesEmailAddress);
                            }

                        }
                    }
                }

                string notes = string.Empty;
                if (redirected == true)
                {
                    notes = oldTechName + " redirected work order to " + techHierarchyView.PreferredProvider;
                }
                else
                {
                    notes = "Work order sent to " + techHierarchyView.PreferredProvider;
                }

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = notes,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = string.IsNullOrEmpty(UserName) ? (string.IsNullOrEmpty(CustomerUserName) ? UserName : CustomerUserName) : UserName,
                    isDispatchNotes = 1
                };
                workOrder.NotesHistories.Add(notesHistory);


                result = true;
            }

            if (isAssist > 0)
            {
                TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, techId);

                //assist tech dispatch
                if (techWorkOrderSchedule != null)
                {
                    techWorkOrderSchedule.PrimaryTech = Convert.ToInt16(isAssist);
                    techWorkOrderSchedule.AssignedStatus = assignedStatus;
                    techWorkOrderSchedule.ModifiedScheduleDate = currentTime;
                }
                else
                {
                    IndexCounter scheduleCounter = Utility.GetIndexCounter("ScheduleID", 1);
                    scheduleCounter.IndexValue++;
                    //FarmerBrothersEntitites.Entry(scheduleCounter).State = System.Data.Entity.EntityState.Modified;

                    WorkorderSchedule newworkOrderSchedule = new WorkorderSchedule()
                    {
                        Scheduleid = scheduleCounter.IndexValue.Value,
                        Techid = Convert.ToInt32(techHierarchyView.TechID),
                        TechName = techHierarchyView.PreferredProvider,
                        WorkorderID = workOrder.WorkorderID,
                        TechPhone = techHierarchyView.ProviderPhone,
                        ServiceCenterName = techHierarchyView.BranchName,
                        ServiceCenterID = Convert.ToInt32(techHierarchyView.TechID),
                        FSMName = techHierarchyView.DSMName,
                        FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>(),
                        EntryDate = currentTime,
                        ScheduleDate = currentTime,
                        TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"],

                        AssistTech = Convert.ToInt16(isAssist),
                        PrimaryTech = -1,
                        AssignedStatus = assignedStatus,
                        ModifiedScheduleDate = currentTime,
                        ScheduleUserid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234
                    };

                    workOrder.WorkorderSchedules.Add(newworkOrderSchedule);

                }

                string notes = string.Empty;
                notes = "Work order sent to " + techHierarchyView.PreferredProvider;

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = notes,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = UserName,
                    isDispatchNotes = 1
                };
                workOrder.NotesHistories.Add(notesHistory);


                result = true;
            }

            int numberOfAssistAccepted = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted" && ws.AssistTech >= 0).Count();
            int numberOfAssistRejected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Declined" && ws.AssistTech >= 0).Count();
            int numberOfAssistDispatches = workOrder.WorkorderSchedules.Where(ws => ws.AssistTech >= 0).Count();
            int numberOfAssistRedirected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Redirected" && ws.AssistTech >= 0).Count();

            int numberOfPrimaryAccepted = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted" && ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryRejected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Declined" && ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryDispatches = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryRedirected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Redirected" && ws.PrimaryTech >= 0).Count();

            string currentStatus = workOrder.WorkorderCallstatus;
            if ((numberOfPrimaryDispatches > 0 || numberOfPrimaryAccepted > 0) && (numberOfAssistDispatches > 0 || numberOfAssistAccepted > 0))
            {
                if (numberOfPrimaryAccepted > 0 && numberOfAssistAccepted > 0)
                {
                    workOrder.WorkorderCallstatus = "Accepted";
                }
                else if (numberOfPrimaryAccepted > 0 || numberOfAssistAccepted > 0)
                {
                    workOrder.WorkorderCallstatus = "Accepted-Partial";
                }
                else
                {
                    workOrder.WorkorderCallstatus = "Pending Acceptance";
                }
            }
            else if (numberOfPrimaryDispatches > 0 && numberOfPrimaryAccepted > 0)
            {
                workOrder.WorkorderCallstatus = "Accepted";
            }
            else if (numberOfAssistDispatches > 0 && numberOfAssistAccepted > 0)
            {
                workOrder.WorkorderCallstatus = "Accepted";
            }
            else if (string.Compare(assignedStatus, "Declined", true) == 0)
            {
                workOrder.WorkorderCallstatus = "Open";
            }
            else if (string.Compare(assignedStatus, "Sent", true) == 0)
            {
                workOrder.WorkorderCallstatus = "Pending Acceptance";
            }

            WorkorderStatusLog statusLog = new WorkorderStatusLog() { StatusFrom = currentStatus, StatusTo = workOrder.WorkorderCallstatus, StausChangeDate = currentTime, WorkorderID = workOrder.WorkorderID };
            workOrder.WorkorderStatusLogs.Add(statusLog);

            return result;
        }

        protected Customer GetCustomerById(int? customerId)
        {
            Customer customer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == customerId).FirstOrDefault();
            return customer;
        }

        protected TECH_HIERARCHY GetTechById(int? techId)
        {
            TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
            return techView;
        }

        #endregion


        #region Delete SavedSearch Results

        public JsonResult DeleteSavedSearch(string savedSearchName)
        {
            WorkorderSavedSearch deletedSavedSearch = FarmerBrothersEntitites.WorkorderSavedSearches.Remove(FarmerBrothersEntitites.WorkorderSavedSearches.FirstOrDefault(ws => ws.SavedSearchName == savedSearchName));
            int effectedRecords = FarmerBrothersEntitites.SaveChanges();

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        #endregion


        #region Email WO

        public JsonResult EmailEvent(int workOrderId, string emailAddress)
        {
            bool emailStatus = false;
            if (workOrderId > 0)
            {
                WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workOrderId);
                WorkorderController wc = new WorkorderController();
                wc.UpdateWOModifiedElapsedTime(workOrder.WorkorderID);

                string toEmail = "";
                string[] addresses = emailAddress.Split(';');
                foreach (string address in addresses)
                {
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        if (address.ToLower().Contains("@jmsmucker.com"))
                            continue;

                        toEmail += address + ";";
                    }
                }

                if (workOrder != null)
                {
                    StringBuilder subject = new StringBuilder();
                    subject.Append("Email WO - ");
                    subject.Append(workOrder.WorkorderID);
                    subject.Append(" ST:");
                    subject.Append(workOrder.CustomerState);
                    subject.Append(" Call Type:");
                    subject.Append(workOrder.WorkorderCalltypeDesc);

                    SendWorkOrderMail(workOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"], null, MailType.INFO, false, "");
                    emailStatus = true;
                }
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = emailStatus };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        #endregion

        #region Customer Update
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "UpdateCustomer")]
        [ActionName("UpdateCustomer")]
        public JsonResult UpdateCustomer([ModelBinder(typeof(CustomerModelBinder))] CustomerModel customer)
        {
            var CustomerId = Convert.ToInt32(customer.CustomerId);
            var workOrderId = Convert.ToInt32(customer.WorkOrderId);
            var contact = FarmerBrothersEntitites.Contacts.Find(CustomerId);
            CustomerModel oldCustomer = new CustomerModel();
            oldCustomer.CustomerSpecialInstructions = contact.CustomerSpecialInstructions;
            oldCustomer.AreaCode = contact.AreaCode;
            oldCustomer.PhoneNumber = contact.Phone;
            oldCustomer.MainContactName = contact.FirstName + " " + contact.LastName;
            oldCustomer.CustomerName = contact.CompanyName;
            oldCustomer.Address = contact.Address1;
            oldCustomer.Address2 = contact.Address2;
            oldCustomer.City = contact.City;
            oldCustomer.State = contact.State;
            oldCustomer.ZipCode = contact.PostalCode;
            oldCustomer.DistributorName = contact.DistributorName;
            oldCustomer.MainEmailAddress = contact.Email;
            oldCustomer.CustomerId = contact.ContactID.ToString();

            var workorder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            if (workorder != null)
            {
                workorder.CustomerPhone = customer.PhoneNumber;
                workorder.CustomerMainContactName = customer.MainContactName;
                workorder.CustomerName = customer.CustomerName;
                workorder.CustomerAddress = customer.Address;
                workorder.CustomerCity = customer.City;
                workorder.CustomerZipCode = customer.ZipCode;
                workorder.CustomerMainEmail = customer.MainEmailAddress;
                workorder.CustomerState = customer.State;
                workorder.CustomerMainContactName = customer.MainContactName;
            }

            contact.AreaCode = customer.AreaCode;
            contact.Phone = customer.PhoneNumber;

            if (customer.PhoneNumber != null)
            {
                contact.PhoneWithAreaCode = customer.PhoneNumber.Replace("(", "").Replace(")", "").Replace("-", "");
            }
            if (!string.IsNullOrWhiteSpace(customer.MainContactName))
            {
                var names = customer.MainContactName.Trim().Split(' ');
                if (names.Length >= 2)
                {
                    contact.FirstName = names[0];
                    contact.LastName = string.Empty;
                    for (int ind = 1; ind < names.Length; ind++)
                    {
                        contact.LastName += " " + names[ind];
                    }
                }
                else
                {
                    contact.FirstName = names[0];
                    contact.LastName = string.Empty;
                }
            }
            contact.CompanyName = customer.CustomerName;
            contact.Address1 = customer.Address;
            contact.Address2 = customer.Address2;
            contact.City = customer.City;
            contact.State = customer.State;
            contact.PostalCode = customer.ZipCode;
            contact.DistributorName = customer.DistributorName;
            contact.Email = customer.MainEmailAddress;
            contact.CustomerSpecialInstructions = customer.CustomerSpecialInstructions;
            CustomerController custcontrl = new CustomerController();
            if (custcontrl.ValidateZipCode(customer.ZipCode))
            {
                FarmerBrothersEntitites.SaveChanges();
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("WorkorderManagement", "Workorder", new { customerId = customer.CustomerId });
                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = 0, Url = redirectUrl, data = custcontrl.SendCustomerDetailsUpdateMail(customer, oldCustomer, Server.MapPath("~/img/mainlogo.jpg")) };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            else
            {
                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = 1, data = 0 };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }

        }


        public JsonResult GenerateRandomPONumber()
        {
            string pwd = string.Empty;
            string message = string.Empty;
            try
            {
                int lengthOfPassword = 6;
                string valid = "ABCDEFGHIJKLMNOZ1234567890";
                StringBuilder strB = new StringBuilder(100);
                Random random = new Random();
                while (0 < lengthOfPassword--)
                {
                    strB.Append(valid[random.Next(valid.Length)]);
                }
                pwd = strB.ToString();

            }
            catch (Exception)
            {
                message = "Unable to Generate PO!";
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = 1, data = pwd, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public static string GenerateRandomWOConfirmationCode()
        {
            string WOConfirmationCode = string.Empty;
            string message = string.Empty;
            try
            {
                int lengthOfPassword = 8;
                string valid = "ABCDEFGHIJKLMNOZ1234567890";
                StringBuilder strB = new StringBuilder(100);
                Random random = new Random();
                while (0 < lengthOfPassword--)
                {
                    strB.Append(valid[random.Next(valid.Length)]);
                }
                WOConfirmationCode = strB.ToString();

            }
            catch (Exception)
            {
                message = "Unable to Generate Work Order Confirmation Code!";
            }
            return WOConfirmationCode;
        }

        public static object sGenPwd(int nLength)
        {
            String sPassword = "";
            String sChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ23456789";
            int nSize = sChars.Length;
            var random = new Random();
            for (int x = 1; x <= nLength; x++)
            {
                sPassword += sChars[random.Next(sChars.Length)];
            }
            return sPassword;
        }
        [ValidateInput(false)]
        public JsonResult SendEscalationMail(string emailTo, int workOrderId, List<String> notes)
        {
            bool result = true;
            string emailId = string.Empty;
            StringBuilder salesEmailBody = new StringBuilder();
            StringBuilder subject = new StringBuilder();
            string emailAddress = string.Empty;
            string[] otherEmail = emailTo.Split('|');
            string message = string.Empty;
            if (otherEmail.Length > 1)
            {
                emailTo = otherEmail[0];
                emailAddress = otherEmail[1];
            }



            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();

            subject.Append("ESCALATION - WO: ");
            subject.Append(workOrder.WorkorderID);
            subject.Append(" Customer: ");
            subject.Append(workOrder.CustomerName);
            subject.Append(" ST: ");
            subject.Append(workOrder.CustomerState);
            subject.Append(" Call Type: ");
            subject.Append(workOrder.WorkorderCalltypeDesc);

            //ESMDSMRSM esmdsmrsmView = null;
            ESMCCMRSMEscalation esmdsmrsmView = null;
            Contact customer = null;
            if (workOrder != null)
            {
                customer = FarmerBrothersEntitites.Contacts.Where(w => w.ContactID == workOrder.CustomerID).FirstOrDefault();
                //int regionNumber = Convert.ToInt32(customer.RegionNumber);
                //esmdsmrsmView = FarmerBrothersEntitites.ESMDSMRSMs.FirstOrDefault(x => x.BranchNO == customer.Branch);
                esmdsmrsmView = FarmerBrothersEntitites.ESMCCMRSMEscalations.FirstOrDefault(x => x.ZIPCode == customer.PostalCode);
            }

            if (esmdsmrsmView != null || emailTo == "Other")
            {
                switch (emailTo)
                {
                    case "DSM":
                        emailId = esmdsmrsmView.CCMEmail;
                        break;
                    case "ESM":
                        emailId = esmdsmrsmView.ESMEmail;
                        break;
                    case "Mike Fraser":
                        emailId = ConfigurationManager.AppSettings["MikeEmailId"];
                        break;
                    case "Darryl McGee":
                        emailId = ConfigurationManager.AppSettings["DarrylEmailId"];
                        break;
                    case "RSM":
                        emailId = esmdsmrsmView.RSMEmail;
                        break;
                    case "CCM":
                        emailId = esmdsmrsmView.CCMEmail;
                        break;
                    case "Other":
                        emailId = emailAddress;
                        break;
                    default:
                        break;
                }
            }

            /*if (customer != null || emailTo == "Other")
            {
                switch (emailTo)
                {
                    case "DSM":
                        emailId = customer.CCMEmail;
                        break;
                    case "ESM":
                        emailId = customer.ESMEmail;
                        break;
                    case "Mike Fraser":
                        emailId = ConfigurationManager.AppSettings["MikeEmailId"];
                        break;
                    case "Darryl McGee":
                        emailId = ConfigurationManager.AppSettings["DarrylEmailId"];
                        break;
                    case "RSM":
                        emailId = customer.RSMEmail;
                        break;
                    case "CCM":
                        emailId = customer.CCMEmail;
                        break;
                    case "Other":
                        emailId = emailAddress;
                        break;
                    default:
                        break;
                }
            }*/

            if (!string.IsNullOrEmpty(emailId))
            {
                try
                {
                    string addtionalNotes = string.Empty;
                    DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                    foreach (string note in notes)
                    {
                        string trNotes = note.Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\n", " ").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ").Replace("\\", " ");
                        if (!string.IsNullOrEmpty(trNotes))
                        {
                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 0,
                                EntryDate = currentTime,
                                Notes = trNotes,
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = UserName,
                                WorkorderID = workOrder.WorkorderID,
                                isDispatchNotes = 1
                            };
                            addtionalNotes = trNotes;
                            FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                        }

                    }

                    //sytem notes

                    NotesHistory systemNotesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = emailTo == "Other" ? "Escalation Mail sent to " + emailTo + " (" + emailId + ")" : "Escalation Mail sent to " + emailTo,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        WorkorderID = workOrder.WorkorderID,
                        isDispatchNotes = 1
                    };

                    FarmerBrothersEntitites.NotesHistories.Add(systemNotesHistory);

                    if (workOrder.WorkorderCallstatus.ToLower() != "closed")
                    {
                        workOrder.WorkorderModifiedDate = currentTime;
                        workOrder.ModifiedUserName = UserName;
                        workOrder.WorkorderCallstatus = "Escalated for follow-up";


                        WorkorderSchedule techWorkOrderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus.ToLower() == "accepted").FirstOrDefault();
                        if (techWorkOrderSchedule != null)
                        {
                            techWorkOrderSchedule.AssignedStatus = "Sent";
                        }
                    }
                    FarmerBrothersEntitites.SaveChanges();

                    string salesEmailAddress = string.Empty;
                    if (customer != null)
                    {
                        if (!string.IsNullOrEmpty(customer.SalesEmail))
                        {
                            salesEmailAddress = customer.SalesEmail;
                        }
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                        {
                            salesEmailAddress = "";
                        }
                    }

                    if (emailTo == "Other")
                    {
                        if (IsValidEmail(emailId))
                        {
                            SendWorkOrderMail(workOrder, subject.ToString(), emailId, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, addtionalNotes, "ESCALATION", false, salesEmailAddress);
                        }
                        else
                        {
                            message = "|Please Enter Valid Email";
                        }

                    }
                    else
                    {
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                        {
                            emailId = ConfigurationManager.AppSettings["SalesEmailAddress"];
                        }

                        SendWorkOrderMail(workOrder, subject.ToString(), emailId, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, addtionalNotes, "ESCALATION", false, salesEmailAddress);
                    }


                }
                catch (Exception)
                {
                    result = false;
                    throw;
                }
            }
            else
            {
                message = "|Email Id is null";
            }
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = result, serverError = 1, data = 1, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        //This will be used of spawna and NSR ( no service required)
        public JsonResult GetSpawnReasons(int solutionId)
        {
            var data = new List<AllFBStatu>();
            IEnumerable<AllFBStatu> statuses = FarmerBrothersEntitites.AllFBStatus.Where(al => al.SolutionID == solutionId);
            foreach (AllFBStatu status in statuses)
            {
                data.Add(status);
            }
            data = data.OrderBy(al => al.StatusSequence).ToList();

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }



        public static bool IsValidEmail(string email)
        {
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            if (match.Success)
                return true;
            else
                return false;
        }

        public JsonResult GetParts()
        {

            List<VendorDataModel> CloserSkusList = new List<VendorDataModel>();
            try
            {
                IQueryable<string> CloserPartOrSKU = FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true).Select(s => s.ItemNo).Distinct();

                foreach (string vendor in CloserPartOrSKU)
                {
                    CloserSkusList.Add(new VendorDataModel(vendor));
                }
                CloserSkusList.OrderBy(v => v.VendorDescription).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the closer SKU ", ex);
            }


            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = CloserSkusList };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        #endregion

        public ActionResult Data(string sku)
        {
            return View();
        }

        #region Auto Dispatch Module

        public int IsValidWorkOrderToStartAutoDispatch(WorkOrder workOrderModel)
        {
            int resultFlag = 0;
            bool success = false;
            SqlHelper helper = new SqlHelper();
            DateTime currentTime = Utility.GetCurrentTime(workOrderModel.CustomerZipCode, FarmerBrothersEntitites);
            //string customerSearchType = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrderModel.CustomerID).Select(c => c.SearchType).FirstOrDefault();
            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrderModel.CustomerID).FirstOrDefault();
            string customerSearchType = customer == null ? "" : customer.SearchType;
            string customerBranch = customer == null ? "" : customer.Branch;
            /*
             * Checking for Branch 311 base don the Email "FB Branch 311 no auto rim" from Connie
             */
            if (workOrderModel.IsSpecificTechnician == false && !string.IsNullOrEmpty(customerSearchType) && customerSearchType.Trim() != "CCP" && customerSearchType.Trim() != "LEGACY" && customerBranch != "311")
            {
                if (workOrderModel.WorkorderCalltypeid == 1200 || workOrderModel.WorkorderCalltypeid == 1100 ||
                workOrderModel.WorkorderCalltypeid == 1110 || workOrderModel.WorkorderCalltypeid == 1120 ||
                workOrderModel.WorkorderCalltypeid == 1130 || workOrderModel.WorkorderCalltypeid == 1400 ||
                workOrderModel.WorkorderCalltypeid == 1410 || workOrderModel.WorkorderCalltypeid == 1900 ||
                workOrderModel.WorkorderCalltypeid == 1800 || workOrderModel.WorkorderCalltypeid == 1810 ||
                workOrderModel.WorkorderCalltypeid == 1300 || workOrderModel.WorkorderCalltypeid == 1600 || workOrderModel.WorkorderCalltypeid == 1700 ||
                workOrderModel.WorkorderCalltypeid == 1710 || workOrderModel.WorkorderCalltypeid == 1820 || workOrderModel.WorkorderCalltypeid == 1830 ||
                workOrderModel.WorkorderCalltypeid == 1900)
                {
                    if (string.IsNullOrEmpty(workOrderModel.OverrideAutoEmail.ToString()))
                    {
                        string NoAutoEmailZipCodesQuery = "Select * from NoAutoEmailZipCodes where PostalCode = '" + workOrderModel.CustomerZipCode + "'";
                        DataTable NoAutoEmailZipCodesdt = helper.GetDatatable(NoAutoEmailZipCodesQuery);

                        if (NoAutoEmailZipCodesdt.Rows.Count == 0)
                        {
                            string AvailableEmailStartTime;
                            string AvailableEmailEndTime;
                            //DateTime dateTime = DateTime.UtcNow.Date;
                            DateTime dateTime;
                            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
                            {
                                dateTime = Utility.GetCurrentTime(workOrderModel.CustomerZipCode, FarmerBrothersEntitites);

                            }
                            AvailableEmailStartTime = (dateTime.ToString("MM/dd/yyyy") + " " + ConfigurationManager.AppSettings["AutoDispatchAvailableEmailStartTime"]);
                            AvailableEmailEndTime = (dateTime.ToString("MM/dd/yyyy") + " " + ConfigurationManager.AppSettings["AutoDispatchAvailableEmailEndTime"]);

                            if (((customerSearchType.Trim() == "CBI") && (customerSearchType.Trim() == "PFS")))
                            {
                                AvailableEmailStartTime = (dateTime.ToString("MM/dd/yyyy") + " " + ConfigurationManager.AppSettings["CBIAndPFSAvailableEmailStartTime"]);
                                AvailableEmailEndTime = (dateTime.ToString("MM/dd/yyyy") + " " + ConfigurationManager.AppSettings["CBIAndPFSAvailableEmailEndTime"]);
                            }


                            //********************************************************************(Feb 18, 2021) New Logic to Dispatch all the time Irrespective of Holidays **********************************
                            if (Convert.ToDateTime(dateTime).DayOfWeek == DayOfWeek.Saturday || Convert.ToDateTime(dateTime).DayOfWeek == DayOfWeek.Sunday
                                || TechnicianCalendarController.IsHoliday(dateTime))
                            {
                                resultFlag = 2;
                            }
                            else if (((DateTime.Parse(AvailableEmailStartTime) <= DateTime.Parse(workOrderModel.WorkorderEntryDate.ToString()))
                                   && (DateTime.Parse(workOrderModel.WorkorderEntryDate.ToString()) <= DateTime.Parse(AvailableEmailEndTime))))
                            {
                                resultFlag = 1;
                            }
                            else
                            {
                                string updateAutoEmailQuery = "Update WorkOrder Set AutoEmailSent=0,HighlightDispatchBoard=2 where WorkOrderID = " + workOrderModel.WorkorderID;
                                helper.UpdateCommand(updateAutoEmailQuery);
                                resultFlag = 2;
                            }

                            /*if (!TechnicianCalendarController.IsHoliday(dateTime))
                            {
                                if (Convert.ToDateTime(dateTime).DayOfWeek == DayOfWeek.Saturday || Convert.ToDateTime(dateTime).DayOfWeek == DayOfWeek.Sunday)
                                {
                                    string updateAutoEmailQuery = "Update WorkOrder Set AutoEmailSent=0,HighlightDispatchBoard=2 where WorkOrderID = " + workOrderModel.WorkorderID;
                                    helper.UpdateCommand(updateAutoEmailQuery);
                                }
                                else if (((DateTime.Parse(AvailableEmailStartTime) <= DateTime.Parse(workOrderModel.WorkorderEntryDate.ToString()))
                                   && (DateTime.Parse(workOrderModel.WorkorderEntryDate.ToString()) <= DateTime.Parse(AvailableEmailEndTime))))
                                {
                                    success = true;
                                }
                                else
                                {
                                    string updateAutoEmailQuery = "Update WorkOrder Set AutoEmailSent=0,HighlightDispatchBoard=2 where WorkOrderID = " + workOrderModel.WorkorderID;
                                    helper.UpdateCommand(updateAutoEmailQuery);
                                }
                            }
                            else
                            {
                                string updateAutoEmailQuery = "Update WorkOrder Set AutoEmailSent=0,HighlightDispatchBoard=2 where WorkOrderID = " + workOrderModel.WorkorderID;
                                helper.UpdateCommand(updateAutoEmailQuery);
                            }*/
                            //*************************************************************************************************
                        }
                        else
                        {
                            string updateAutoEmailQuery = "Update WorkOrder Set AutoEmailSent=0,HighlightDispatchBoard=2 where WorkOrderID = " + workOrderModel.WorkorderID;
                            helper.UpdateCommand(updateAutoEmailQuery);
                        }
                    }
                    else
                    {
                        string updateAutoEmailQuery = "Update WorkOrder Set AutoEmailSent=0,HighlightDispatchBoard=3 where WorkOrderID = " + workOrderModel.WorkorderID;
                        helper.UpdateCommand(updateAutoEmailQuery);


                        NotesHistory notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 1,
                            EntryDate = currentTime,
                            Notes = "FB Employee is calling. Auto email functionality is over rided",
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 1
                        };
                        WorkOrder wr = new WorkOrder();
                        wr.NotesHistories.Add(notesHistory);
                        FarmerBrothersEntitites.SaveChanges();
                    }
                }
            }

            //return success;           
            return resultFlag;
        }

        public string CustomerUserName = "";
        public void StartAutoDispatchProcess1(WorkOrder workOrderModel, string usrName = "")
        {
            #region properties initialization

            CustomerUserName = usrName;

            SqlHelper helper = new SqlHelper();
            DateTime currentTime = Utility.GetCurrentTime(workOrderModel.CustomerZipCode, FarmerBrothersEntitites);
            DataTable WorkOrderdt;
            DataTable rsAssetList;
            DataTable Contactdt;

            int TechID = -1;
            DataTable rsDealerEmail;
            int ContactId;

            #endregion

            //if (IsValidWorkOrderToStartAutoDispatch(workOrderModel))
            {
                string WorkOrderQuery = "Select * from WorkOrder where WorkorderID = " + workOrderModel.WorkorderID;
                WorkOrderdt = helper.GetDatatable(WorkOrderQuery);

                ContactId = WorkOrderdt.Rows.Count > 0 ? Convert.ToInt32(WorkOrderdt.Rows[0]["CustomerID"]) : 0;
                string ContactQuery = "Select * from v_Contact where ContactID = " + ContactId;
                Contactdt = helper.GetDatatable(ContactQuery);

                if (Contactdt.Rows.Count <= 0)
                {
                    using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                    {
                        FBActivityLog log = new FBActivityLog();
                        log.LogDate = DateTime.UtcNow;
                        log.UserId = (int)System.Web.HttpContext.Current.Session["UserId"];
                        log.ErrorDetails = "Auto Dispatch - Unable to get contact information";
                        entity.FBActivityLogs.Add(log);
                        entity.SaveChanges();
                    }
                    return;
                }
                else
                {

                    string WorkOrderHistoryQuery = "Select * from v_ContactServiceHistory where WorkorderID = " + WorkOrderdt.Rows[0]["WorkorderID"];
                    rsAssetList = helper.GetDatatable(WorkOrderHistoryQuery);

                    if (rsAssetList.Rows.Count <= 0)
                    {
                        using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                        {
                            FBActivityLog log = new FBActivityLog();
                            log.LogDate = DateTime.UtcNow;
                            log.UserId = (int)System.Web.HttpContext.Current.Session["UserId"];
                            log.ErrorDetails = "Auto Dispatch - Unable to get Asset information";
                            entity.FBActivityLogs.Add(log);
                            entity.SaveChanges();
                        }
                        return;
                    }
                    else
                    {

                    }

                    int replaceTechId = 0;
                    if (string.IsNullOrEmpty(Convert.ToString(Contactdt.Rows[0]["FBProviderID"])))
                    {
                        string postCode = Convert.ToString(Contactdt.Rows[0]["PostalCode"]);
                        DataTable rsReferralList = null;
                        FindAvailableDealers(postCode, false, out rsReferralList);

                        foreach (DataRow dr in rsReferralList.Rows)
                        {
                            int techId = Convert.ToInt32(dr["DealerID"]);
                            bool IsUnavailable = TechnicianCalendarController.IsTechUnAvailable(techId, currentTime, out replaceTechId);

                            if (!IsUnavailable)
                            {
                                if (replaceTechId != 0)
                                {
                                   TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                                    if (THV != null)
                                    {
                                        TechID = replaceTechId;
                                    }
                                }
                                else
                                {
                                    TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == techId).FirstOrDefault();

                                    if (THV != null)
                                    {
                                        TechID = techId;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        string TechnicianQuery = ("Select * from TECH_HIERARCHY where DealerID = " + Contactdt.Rows[0]["FBProviderID"] + " and SearchType='SP'  ");
                        rsDealerEmail = helper.GetDatatable(TechnicianQuery);

                        if (rsDealerEmail.Rows.Count > 0)
                        {
                            int techId = Convert.ToInt32(rsDealerEmail.Rows[0]["DealerID"]);
                            bool IsUnavailable = TechnicianCalendarController.IsTechUnAvailable(techId, currentTime, out replaceTechId);
                            if (!IsUnavailable)
                            {
                                if (replaceTechId != 0)
                                {
                                    TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                                    if (THV != null)
                                    {
                                        TechID = replaceTechId;
                                    }
                                }
                                else
                                {
                                    TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == techId).FirstOrDefault();

                                    if (THV != null)
                                    {
                                        TechID = techId;
                                    }
                                }
                            }
                            else
                            {
                                string postCode = Convert.ToString(Contactdt.Rows[0]["PostalCode"]);
                                DataTable rsReferralList = null;
                                FindAvailableDealers(postCode, false, out rsReferralList);

                                foreach (DataRow dr in rsReferralList.Rows)
                                {
                                    int nearestTechId = Convert.ToInt32(dr["DealerID"]);
                                    bool IsTechUnavailable = TechnicianCalendarController.IsTechUnAvailable(nearestTechId, currentTime, out replaceTechId);

                                    if (!IsTechUnavailable)
                                    {
                                        if (replaceTechId != 0)
                                        {
                                            TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                                            if (THV != null)
                                            {
                                                TechID = replaceTechId;
                                            }
                                        }
                                        else
                                        {
                                            TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == nearestTechId).FirstOrDefault();

                                            if (THV != null)
                                            {
                                                TechID = nearestTechId;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            string postCode = Convert.ToString(Contactdt.Rows[0]["PostalCode"]);
                            DataTable rsReferralList = null;
                            FindAvailableDealers(postCode, false, out rsReferralList);

                            foreach (DataRow dr in rsReferralList.Rows)
                            {
                                int nearestTechId = Convert.ToInt32(dr["DealerID"]);
                                bool IsTechUnavailable = TechnicianCalendarController.IsTechUnAvailable(nearestTechId, currentTime, out replaceTechId);

                                if (!IsTechUnavailable)
                                {
                                    if (replaceTechId != 0)
                                    {
                                        TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                                        if (THV != null)
                                        {
                                            TechID = replaceTechId;
                                        }
                                    }
                                    else
                                    {
                                        TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == nearestTechId).FirstOrDefault();

                                        if (THV != null)
                                        {
                                            TechID = nearestTechId;
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                    }

                    if (TechID != -1)
                    {
                        DispatchMail(workOrderModel.WorkorderID, TechID, true, new List<string>(), false, false);
                        TECH_HIERARCHY techView = GetTechById(TechID);
                        NotesHistory notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 1,
                            EntryDate = currentTime,
                            Notes = "Auto Dispatch E-mail  Sent to " + techView.RimEmail + " " + techView.EmailCC,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = string.IsNullOrEmpty(UserName) ? (string.IsNullOrEmpty(CustomerUserName) ? UserName : CustomerUserName) : UserName,
                            WorkorderID = workOrderModel.WorkorderID,
                            isDispatchNotes = 1
                        };
                        FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                    }


                    FarmerBrothersEntitites.SaveChanges();
                }
            }
            CustomerUserName = "";
        }

        public void StartAutoDispatchProcess(WorkOrder workOrderModel, string usrName = "")
        {
            #region properties initialization

            CustomerUserName = usrName;

            SqlHelper helper = new SqlHelper();
            DateTime currentTime = Utility.GetCurrentTime(workOrderModel.CustomerZipCode, FarmerBrothersEntitites);
            DataTable WorkOrderdt;
            DataTable rsAssetList;
            DataTable Contactdt;

            int TechID = -1;
            DataTable rsDealerEmail;
            int ContactId;

            #endregion

            int resultFlag = IsValidWorkOrderToStartAutoDispatch(workOrderModel);

            if (resultFlag != 0)
            {
                string WorkOrderQuery = "Select * from WorkOrder where WorkorderID = " + workOrderModel.WorkorderID;
                WorkOrderdt = helper.GetDatatable(WorkOrderQuery);

                ContactId = WorkOrderdt.Rows.Count > 0 ? Convert.ToInt32(WorkOrderdt.Rows[0]["CustomerID"]) : 0;
                string ContactQuery = "Select * from v_Contact where ContactID = " + ContactId;
                Contactdt = helper.GetDatatable(ContactQuery);

                if (Contactdt.Rows.Count <= 0)
                {
                    using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                    {
                        FBActivityLog log = new FBActivityLog();
                        log.LogDate = DateTime.UtcNow;
                        log.UserId = (int)System.Web.HttpContext.Current.Session["UserId"];
                        log.ErrorDetails = "Auto Dispatch - Unable to get contact information";
                        entity.FBActivityLogs.Add(log);
                        entity.SaveChanges();
                    }
                    return;
                }
                else
                {

                    string WorkOrderHistoryQuery = "Select * from v_ContactServiceHistory where WorkorderID = " + WorkOrderdt.Rows[0]["WorkorderID"];
                    rsAssetList = helper.GetDatatable(WorkOrderHistoryQuery);

                    if (rsAssetList.Rows.Count <= 0)
                    {
                        using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                        {
                            FBActivityLog log = new FBActivityLog();
                            log.LogDate = DateTime.UtcNow;
                            log.UserId = (int)System.Web.HttpContext.Current.Session["UserId"];
                            log.ErrorDetails = "Auto Dispatch - Unable to get Asset information";
                            entity.FBActivityLogs.Add(log);
                            entity.SaveChanges();
                        }
                        return;
                    }
                    else
                    {

                    }

                    if (resultFlag == 1)
                    {
                        int replaceTechId = 0;
                        if (string.IsNullOrEmpty(Convert.ToString(Contactdt.Rows[0]["FBProviderID"])))
                        {
                            string postCode = Convert.ToString(Contactdt.Rows[0]["PostalCode"]);
                            TechID = getAvailableTechId(postCode, currentTime);

                        }
                        else
                        {
                            string TechnicianQuery = ("Select * from TECH_HIERARCHY where DealerID = " + Contactdt.Rows[0]["FBProviderID"] + " and SearchType='SP'  ");
                            rsDealerEmail = helper.GetDatatable(TechnicianQuery);

                            if (rsDealerEmail.Rows.Count > 0)
                            {
                                int techId = Convert.ToInt32(rsDealerEmail.Rows[0]["DealerID"]);
                                bool IsUnavailable = TechnicianCalendarController.IsTechUnAvailable(techId, currentTime, out replaceTechId);
                                if (!IsUnavailable)
                                {
                                    if (replaceTechId != 0)
                                    {
                                        TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                                        if (THV != null)
                                        {
                                            TechID = replaceTechId;
                                        }
                                    }
                                    else
                                    {
                                        TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == techId).FirstOrDefault();

                                        if (THV != null)
                                        {
                                            TechID = techId;
                                        }
                                    }
                                }
                                else
                                {
                                    string postCode = Convert.ToString(Contactdt.Rows[0]["PostalCode"]);
                                    TechID = getAvailableTechId(postCode, currentTime);
                                }
                            }
                            else
                            {
                                string postCode = Convert.ToString(Contactdt.Rows[0]["PostalCode"]);
                                TechID = getAvailableTechId(postCode, currentTime);
                            }

                        }
                    }
                    else if (resultFlag == 2)
                    {
                        string postCode = Convert.ToString(Contactdt.Rows[0]["PostalCode"]);
                        TechID = getAvailableOnCallTechId(postCode, currentTime, workOrderModel.WorkorderID);
                    }

                    if (TechID != -1 && TechID != 0)
                    {
                        TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == TechID).FirstOrDefault();
                        if (THV.FamilyAff == "SPT")
                        {
                            if (workOrderModel.WorkorderCalltypeid == 1300)
                            {
                                TECH_HIERARCHY th = FarmerBrothersEntitites.TECH_HIERARCHY.Where(a => a.DealerId == 909360).FirstOrDefault();
                                if (th != null)
                                {
                                    TechID = 909360; // If the 3rd party call type is 1300 installation, those events will be sent to Christina Ware – SP 909360(Email from Mike on July 15, 2020)
                                }
                            }
                        }

                        DispatchMail(workOrderModel.WorkorderID, TechID, true, new List<string>(), false, false);
                        TECH_HIERARCHY techView = GetTechById(TechID);
                        if (TechID != 0)
                        {
                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = "Auto Dispatch E-mail  Sent to " + techView.RimEmail + " " + techView.EmailCC,
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = string.IsNullOrEmpty(UserName) ? (string.IsNullOrEmpty(CustomerUserName) ? UserName : CustomerUserName) : UserName,
                                WorkorderID = workOrderModel.WorkorderID,
                                isDispatchNotes = 1
                            };
                            FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                        }
                        else
                        {
                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = "Auto Dispatch E-mail  Sent to " + ConfigurationManager.AppSettings["MikeEmailId"] + ";" + ConfigurationManager.AppSettings["DarrylEmailId"],
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = string.IsNullOrEmpty(UserName) ? (string.IsNullOrEmpty(CustomerUserName) ? UserName : CustomerUserName) : UserName,
                                WorkorderID = workOrderModel.WorkorderID,
                                isDispatchNotes = 1
                            };
                            FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                        }
                    }


                    FarmerBrothersEntitites.SaveChanges();
                }
            }
            CustomerUserName = "";
        }

        private int getAvailableTechId(string PostalCode, DateTime currentTime)
        {
            int availableTechId = 0;
            int replaceTechId = 0;

            DataTable rsReferralList = null;
            FindAvailableDealers(PostalCode, false, out rsReferralList);

            //Check for Internal Techs
            foreach (DataRow dr in rsReferralList.Rows)
            {
                string techType = dr["TechType"].ToString();
                if (techType.ToUpper() != "FB") continue; 

                int techId = Convert.ToInt32(dr["DealerID"]);
                bool IsUnavailable = TechnicianCalendarController.IsTechUnAvailable(techId, currentTime, out replaceTechId);

                if (!IsUnavailable)
                {
                    if (replaceTechId != 0)
                    {
                        TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                        if (THV != null)
                        {
                            availableTechId = replaceTechId;
                            break;
                        }
                    }
                    else
                    {
                        TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == techId).FirstOrDefault();

                        if (THV != null)
                        {
                            availableTechId = techId;
                            break;
                        }
                    }                    
                }
            }

            if (availableTechId == 0)
            {
                //Check for thirdParty Techs
                foreach (DataRow dr in rsReferralList.Rows)
                {
                    string techType = dr["TechType"].ToString();
                    if (techType.ToUpper() == "FB") continue;

                    int techId = Convert.ToInt32(dr["DealerID"]);
                    bool IsUnavailable = TechnicianCalendarController.IsTechUnAvailable(techId, currentTime, out replaceTechId);

                    if (!IsUnavailable)
                    {
                        if (replaceTechId != 0)
                        {
                            TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                            if (THV != null)
                            {
                                availableTechId = replaceTechId;
                                break;
                            }
                        }
                        else
                        {
                            TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == techId).FirstOrDefault();

                            if (THV != null)
                            {
                                availableTechId = techId;
                                break;
                            }
                        }

                    }
                }
            }

            return availableTechId;
        }

        private int getAvailableOnCallTechId(string PostalCode, DateTime currentTime, int WorkorderId)
        {
            int availableTechId = -1;
            int replaceTechId = 0;

           // DataTable rsReferralList =
           SqlHelper helper = new SqlHelper();
            DataTable rsReferralList = helper.GetAfterHoursOnCallTechDetails(PostalCode, WorkorderId);

            //Check for Internal Techs
            foreach (DataRow dr in rsReferralList.Rows)
            {
                string techType = dr["TechType"].ToString();
                if (techType.ToUpper() != "FB") continue;

                int techId = Convert.ToInt32(dr["ServiceCenterId"]);
                bool IsUnavailable = TechnicianCalendarController.IsTechUnAvailable(techId, currentTime, out replaceTechId);

                if (!IsUnavailable)
                {
                    if (replaceTechId != 0)
                    {
                        TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                        if (THV != null)
                        {
                            availableTechId = replaceTechId;
                            break;
                        }
                    }
                    else
                    {
                        TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == techId).FirstOrDefault();

                        if (THV != null)
                        {
                            availableTechId = techId;
                            break;
                        }
                    }
                }
            }

            if (availableTechId == -1)
            {
                //Check for thirdParty Techs
                foreach (DataRow dr in rsReferralList.Rows)
                {
                    string techType = dr["TechType"].ToString();
                    if (techType.ToUpper() == "FB") continue;

                    int techId = Convert.ToInt32(dr["ServiceCenterId"]);
                    bool IsUnavailable = TechnicianCalendarController.IsTechUnAvailable(techId, currentTime, out replaceTechId);

                    if (!IsUnavailable)
                    {
                        if (replaceTechId != 0)
                        {
                            TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                            if (THV != null)
                            {
                                availableTechId = replaceTechId;
                                break;
                            }
                        }
                        else
                        {
                            TECH_HIERARCHY THV = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == techId).FirstOrDefault();

                            if (THV != null)
                            {
                                availableTechId = techId;
                                break;
                            }
                        }

                    }
                }
            }

            return availableTechId;
        }

        public void SendAutoDispatchMail(string toAddress, string EmailCC, string subject, string body)
        {
            var message = new MailMessage();
            string mailTo = toAddress;
            string mailCC = string.Empty;
            if (!string.IsNullOrWhiteSpace(EmailCC))
            {
                string[] addresses = EmailCC.Split(';');
                foreach (string address in addresses)
                {
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        message.CC.Add(new MailAddress(address));
                    }
                }
            }
            message.IsBodyHtml = true;
            message.Body = body;

            message.From = new MailAddress(ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"]);
            message.Subject = subject.ToString();
            message.IsBodyHtml = true;

            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = 25;
                    smtp.Send(message);
                }
            }
            catch (Exception e)
            {

            }
        }

        public bool FindAvailableDealers(string sPostalCode, bool bDefaultDealer, out DataTable rsReferralList)
        {
            SqlHelper helper = new SqlHelper();
            string sSQL;
            DataTable rsHierarchy;
            string sTableName;
            bool bFinished;
            long lReferralID;
            double dDistance;
            DataTable rsLatitudeLongitude;
            string sDealerLatLongFactor;
            double dDealerLatLongFactor;
            double dLatitude;
            double dLongitude;
            double dDealerLatitude;
            double dDealerLongitude;

            rsReferralList = new DataTable();


            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            bool FindAvailableDealers = true;
            bDefaultDealer = false;
            dLatitude = -1;
            dLongitude = -1;
            dDealerLatLongFactor = 2.5;

            if (GetPreference("ReferralLatLongDegrees", out sDealerLatLongFactor))
            {
                dDealerLatLongFactor = double.Parse(sDealerLatLongFactor);
            }
            else
            {
                dDealerLatLongFactor = 2.5;
            }


            if (ReferByAvailableDealersDistance(sPostalCode, dDealerLatLongFactor, out rsReferralList))
            {
                // no errors - check if any referrals were found
                if ((rsReferralList.Rows.Count > 0))
                {
                    bFinished = true;
                }

            }
            else
            {
                // error occurred - exit now
                FindAvailableDealers = false;
                bFinished = true;
            }


            return FindAvailableDealers;
        }


        // ******************************************************************
        // * Description:         Distance-based referral.
        // ******************************************************************
        private bool ReferByAvailableDealersDistance(string customerzipCode, double dDealerLatLongFactor, out DataTable rsReferralList)
        {
            SqlHelper helper = new SqlHelper();
            bool IsTechDetailsExist = false;

            rsReferralList = new DataTable();


            rsReferralList = helper.GetTechDispatchDetails(customerzipCode, dDealerLatLongFactor);
            if (rsReferralList.Rows.Count > 0)
            {
                IsTechDetailsExist = true;
            }
            return IsTechDetailsExist;
        }

        // *********************************************************************************
        // * Description:         Retrieves a record from the Preference table.
        // *********************************************************************************
        public bool GetPreference(string sPreferenceName, out string sPreferenceValue)
        {
            // Define necessary variables
            bool IsPreferenceExist = false;
            sPreferenceValue = string.Empty;
            string ReferralLatLongDegrees = ConfigurationManager.AppSettings[sPreferenceName];
            if (String.IsNullOrEmpty(ReferralLatLongDegrees))
            {
                using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                {
                    FBActivityLog log = new FBActivityLog();
                    log.LogDate = DateTime.UtcNow;
                    log.UserId = (int)System.Web.HttpContext.Current.Session["UserId"];
                    log.ErrorDetails = "Auto Dispatch - No value found for the requested variable name: ReferralLatLongDegrees ";
                    entity.FBActivityLogs.Add(log);
                    entity.SaveChanges();
                }
            }
            else
            {
                IsPreferenceExist = true;
                sPreferenceValue = ReferralLatLongDegrees;
            }
            return IsPreferenceExist;
        }

        public static int GetOncallTechId(string branchNumber)
        {
            int techId = 0;


            SqlHelper helper = new SqlHelper();
            string query = @"select distinct techid from [dbo].[TechOnCall] where techid   in (
                                    SELECT tech.Dealerid as Tech_Id from 
                                    TECH_HIERARCHY tech where tech.BranchName !='' and BranchNumber = '" + branchNumber + "' and searchType='SP' and FamilyAff !='SPT'" +
                                    " GROUP BY tech.Dealerid, tech.CompanyName)  and (SELECT CONVERT(VARCHAR(10),scheduledate,120)) = (SELECT CONVERT(VARCHAR(10),GETDATE(),120)) ";

            DataTable dt = new DataTable();
            dt = helper.GetDatatable(query);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["techid"] != DBNull.Value)
                {
                    techId = Convert.ToInt32(dr["techid"]);
                }
                break;
            }
            return techId;
        }

        public static int GetClosestOnCallTechIdByZip(string customerZipCode)
        {
            int techId = 0;

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetClosestOnCallTechDetails(customerZipCode);

            foreach (DataRow dr in dt.Rows)
            {
                techId = Convert.ToInt32(dr["ServiceCenterId"].ToString());
                break;
            }
            return techId;
        }

        public static int GetOnCallTechIdByZip(string customerZipCode, int? workOrderId)
        {
            int techId = 0;

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetOnCallTechDetails(customerZipCode, workOrderId);

            foreach (DataRow dr in dt.Rows)
            {
                techId = Convert.ToInt32(dr["ServiceCenterId"].ToString());
                break;
            }
            return techId;
        }

        public static int GetAfterHoursOnCallTechIdByZip(string customerZipCode, int? workOrderId)
        {
            int techId = 0;

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetAfterHoursOnCallTechDetails(customerZipCode, workOrderId);

            foreach (DataRow dr in dt.Rows)
            {
                techId = Convert.ToInt32(dr["ServiceCenterId"].ToString());
                break;
            }
            return techId;
        }

        public static int GetAfterHoursClosestOnCallTechIdByZip(string customerZipCode)
        {
            int techId = 0;

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetAfterHoursClosestOnCallTechDetails(customerZipCode);

            foreach (DataRow dr in dt.Rows)
            {
                techId = Convert.ToInt32(dr["ServiceCenterId"].ToString());
                break;
            }
            return techId;
        }
        public static string GetOnCallTechDetailsByZip(string customerZipCode, int? workOrderId)
        {
            string CompanyName = string.Empty;
            string AreaCode = string.Empty;
            string Phone = string.Empty;

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetOnCallTechDetails(customerZipCode, workOrderId);

            foreach (DataRow dr in dt.Rows)
            {
                CompanyName = dr["Name"].ToString();
                Phone = Utility.FormatPhoneNumber(dr["Phone"].ToString());
                break;
            }
            return CompanyName + " " + Phone;
        }

        public static string GetAfterHoursOnCallTechDetailsByZip(string customerZipCode, int? workOrderId)
        {
            string CompanyName = string.Empty;
            string AreaCode = string.Empty;
            string Phone = string.Empty;

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetAfterHoursOnCallTechDetails(customerZipCode, workOrderId);

            foreach (DataRow dr in dt.Rows)
            {
                CompanyName = dr["Name"].ToString();
                Phone = Utility.FormatPhoneNumber(dr["Phone"].ToString());
                break;
            }
            return CompanyName + " " + Phone;
        }
        public static string GetTechOnCall(string branchNumber)
        {
            string CompanyName = string.Empty;
            string AreaCode = string.Empty;
            string Phone = string.Empty;


            SqlHelper helper = new SqlHelper();

            string query = @"select CompanyName,AreaCode,Phone from   TECH_HIERARCHY where dealerid  in (
                                    select distinct techid from [dbo].[TechOnCall] where techid   in (
                                    SELECT tech.Dealerid as Tech_Id from 
                                    TECH_HIERARCHY tech where tech.BranchName !='' and BranchNumber 
                                    = '" + branchNumber + "' and searchType='SP' and FamilyAff !='SPT' " +
                                    " GROUP BY tech.Dealerid, tech.CompanyName) and (SELECT CONVERT(VARCHAR(10),scheduledate,120)) = (SELECT CONVERT(VARCHAR(10),GETDATE(),120)))";

            DataTable dt = new DataTable();
            dt = helper.GetDatatable(query);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["CompanyName"] != DBNull.Value)
                {
                    CompanyName = Convert.ToString(dr["CompanyName"]);
                }
                if (dr["AreaCode"] != DBNull.Value)
                {
                    AreaCode = Convert.ToString(dr["AreaCode"]);
                }
                if (dr["Phone"] != DBNull.Value)
                {
                    if (string.IsNullOrEmpty(AreaCode))
                    {
                        Phone = Convert.ToString(dr["Phone"]);
                    }
                    else
                    {
                        Phone = "(" + AreaCode + ")" + Convert.ToString(dr["Phone"]);
                    }
                }
                break;
            }
            return CompanyName + " " + Phone;
        }
        #endregion


        #region DocumentUpload
        public void WorkorderDocumentUpload111(HttpPostedFileBase file)
        {
            try
            {
                List<CustomerModel> customerList = new List<CustomerModel>();
                if (file == null)
                {
                    ViewBag.Message = "No File Selected ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<CustomerModel>();
                    //return View("CustomerUpload");
                }

                //else if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                //{
                //    ViewBag.Message = "Selected file is not CSV file ";
                //    ViewBag.isSuccess = false;
                //    ViewBag.dataSource = new List<CustomerModel>();
                //    //return View("CustomerUpload");
                //}

                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string DirPath = Server.MapPath("~/UploadedFiles/Customer");
                    DateTime currentDate = DateTime.Now;
                    if (!Directory.Exists(DirPath))
                    {
                        Directory.CreateDirectory(DirPath);
                    }
                    string _inputPath = Path.Combine(DirPath, _FileName);
                    file.SaveAs(_inputPath);
                }

                
            }
            catch (Exception ex)
            {
               
            }
        }

        [HttpPost]
        public ActionResult WorkorderDocumentUpload1()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        int workorderid = Convert.ToInt32(Request.Form["workorderId"].ToString());

                        string DirPath = Server.MapPath("~/UploadedFiles/Documents/"+ workorderid);
                        DateTime currentDate = DateTime.Now;
                        if (!Directory.Exists(DirPath))
                        {
                            Directory.CreateDirectory(DirPath);
                        }

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(DirPath, fname);
                        file.SaveAs(fname);
                    }
                    // Returns message that successfully uploaded  
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        public ActionResult WorkorderDocumentUpload()
        {
            string uname = Request["uploadername"];
            HttpFileCollectionBase files = Request.Files;
            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFileBase file = files[i];
                string fname;
                // Checking for Internet Explorer      
                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = file.FileName;
                }

                int workorderid = Convert.ToInt32(Request.Form["workorderId"].ToString());

                string DirPath = Server.MapPath("~/UploadedFiles/Documents/" + workorderid);
                DateTime currentDate = DateTime.Now;
                if (!Directory.Exists(DirPath))
                {
                    Directory.CreateDirectory(DirPath);
                }


                // Get the complete folder path and store the file inside it.      
                fname = Path.Combine(DirPath, fname);
                file.SaveAs(fname);
            }
            return Json("Hi, " + uname + ". Your files uploaded successfully", JsonRequestBehavior.AllowGet);
        }

        private List<WorkorderDocument> GetWorkorderDocuments(int workorderid)
        {
            List<WorkorderDocument> DocumentsList = new List<WorkorderDocument>();
            string path = Server.MapPath("~/UploadedFiles/Documents/" + workorderid);
            if (Directory.Exists(path))
            {
                DataTable ShowContent = new DataTable();
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo fi in di.GetFiles())
                {
                    WorkorderDocument doc = new WorkorderDocument();
                    doc.Name = fi.Name;

                    var serverPath = Server.MapPath("~/UploadedFiles/Documents/" + workorderid + "/") + fi.Name;


                    doc.Path = ConfigurationManager.AppSettings["FileUploadBaseUrl"] + "UploadedFiles/Documents/" + workorderid + "/" + fi.Name;

                    DocumentsList.Add(doc);
                }
            }

            return DocumentsList;
        }

        [HttpGet]
        public FileResult DownLoadDocument(string Name, string Path)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Path);
            string fileName = Name;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        #endregion

        [HttpPost]
        public JsonResult RemoveCurrentUser(int workOrderId)
        {
            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.FirstOrDefault(w => w.WorkorderID == workOrderId);
            if (workOrder != null)
            {
                if (!string.IsNullOrWhiteSpace(workOrder.CurrentUserName)
                    && string.Compare(workOrder.CurrentUserName, UserName, true) == 0)
                {
                    workOrder.WorkOrderOpenedDateTime = null;
                    workOrder.CurrentUserName = null;
                    FarmerBrothersEntitites.SaveChanges();
                }
            }
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = true };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpPost]
        public JsonResult DispatchEmailCloser(int workOrderId)
        {
            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            JsonResult jsonResult = new JsonResult();
            string message = string.Empty;
            string emailAddress = string.Empty;
            string salesEmailAddress = string.Empty;
            if (workOrder != null)
            {
                StringBuilder subject = new StringBuilder();
                AllFBStatu priority = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workOrder.PriorityCode).First();

                if(priority.FBStatus.Contains("critical"))
                //if (workOrder.PriorityCode == 1 || workOrder.PriorityCode == 2 || workOrder.PriorityCode == 3 || workOrder.PriorityCode == 4)
                {
                    subject.Append("CRITICAL WO: ");
                }
                else
                {
                    subject.Append("WO: ");
                }

                subject.Append(workOrder.WorkorderID);
                subject.Append(" ST: ");
                subject.Append(workOrder.CustomerState);
                subject.Append(" Call Type: ");
                subject.Append(workOrder.WorkorderCalltypeDesc);

                int? techid = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrderId && w.AssignedStatus == "Accepted").Select(w => w.Techid).FirstOrDefault();

                TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == techid).FirstOrDefault();

                if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                {
                    emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                }
                else if (techView != null)
                {
                    if (!string.IsNullOrEmpty(techView.RimEmail))
                    {
                        emailAddress = techView.RimEmail;
                    }

                    if (!string.IsNullOrEmpty(techView.EmailCC))
                    {
                        emailAddress += "#" + techView.EmailCC;
                    }
                }
                else
                {
                    emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                }
                Contact customer = FarmerBrothersEntitites.Contacts.Where(cont => cont.ContactID == workOrder.CustomerID).FirstOrDefault();

                if (customer != null)
                {
                    if (!string.IsNullOrEmpty(customer.SalesEmail))
                    {
                        salesEmailAddress = customer.SalesEmail;
                    }
                }

                SendWorkOrderMail(workOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], techid, MailType.DISPATCH, true, "", "TRANSMIT", true, salesEmailAddress);
                jsonResult.Data = new { success = true, message = "|Successfully sent work order dispatch email" };
            }
            else
            {
                jsonResult.Data = new { success = false, message = "|Unable to send an email! Please contact support." };
            }



            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public bool SendPartsOrderMail(int WorkorderId)
        {
            bool result = true;
            StringBuilder salesEmailBody = new StringBuilder();
            StringBuilder subject = new StringBuilder();

            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == WorkorderId).FirstOrDefault();
            Contact contact = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();

            subject.Append("PARTS ORDER - WO: ");
            subject.Append(workOrder.WorkorderID);
            subject.Append(" Customer: ");
            subject.Append(workOrder.CustomerName);
            subject.Append(" ST: ");
            subject.Append(workOrder.CustomerState);
            subject.Append(" Call Type: ");
            subject.Append(workOrder.WorkorderCalltypeDesc);


            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%' style='margin-right: 100px;margin-bottom: 10px;'>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("CALL TIME: ");
            salesEmailBody.Append(workOrder.WorkorderEntryDate);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Work Order ID#: ");
            salesEmailBody.Append(workOrder.WorkorderID);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<span style='color:#ff0000'><b>");
            salesEmailBody.Append("Date Needed:");
            salesEmailBody.Append(workOrder.DateNeeded);
            salesEmailBody.Append("</b></span>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Customer PO: ");
            salesEmailBody.Append(workOrder.CustomerPO);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Customer Email:");
            salesEmailBody.Append(workOrder.CustomerMainEmail);
            salesEmailBody.Append("<BR>");

            string shippingPriority = "";
            if (!string.IsNullOrEmpty(workOrder.ShippingPriority))
            {
                int statusid = Convert.ToInt32(workOrder.ShippingPriority);
                AllFBStatu fbstatus = FarmerBrothersEntitites.AllFBStatus.Where(f => f.FBStatusID == statusid).FirstOrDefault();
                if (fbstatus != null)
                {
                    shippingPriority = fbstatus.FBStatus;
                }
            }
            salesEmailBody.Append("Shipping Priority:");
            salesEmailBody.Append(shippingPriority);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<span style='color:#ff0000'><b>");
            salesEmailBody.Append("CUSTOMER INFORMATION: ");
            salesEmailBody.Append("</b></span>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Route: ");
            salesEmailBody.Append(contact.Route);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Customer #: ");
            salesEmailBody.Append(contact.ContactID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(contact.Address1);
            salesEmailBody.Append("<BR>");
            if (!string.IsNullOrEmpty(contact.Address2))
            {
                salesEmailBody.Append(contact.Address2);
                salesEmailBody.Append("<BR>");
            }
            salesEmailBody.Append(contact.City);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(contact.State);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(contact.PostalCode);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("PHONE: ");
            salesEmailBody.Append(contact.Phone);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<span style='color:#ff0000'><b>");
            salesEmailBody.Append("SHIP TO LOCATION: ");
            salesEmailBody.Append("</b></span>");
            salesEmailBody.Append(workOrder.OtherPartsContactName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.OtherPartsAddress1);
            salesEmailBody.Append("<BR>");
            if (!string.IsNullOrEmpty(workOrder.OtherPartsAddress2))
            {
                salesEmailBody.Append(workOrder.OtherPartsAddress2);
                salesEmailBody.Append("<BR>");
            }
            salesEmailBody.Append(workOrder.OtherPartsCity);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.OtherPartsState);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.OtherPartsZip);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("PHONE: ");
            salesEmailBody.Append(workOrder.OtherPartsPhone);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<table>");
            salesEmailBody.Append("<tbody>");
            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<th style='border: solid 1px;padding: 0.5em;background: #d9d5d5;'>Manufacturer</th>");
            salesEmailBody.Append("<th style='border: solid 1px;padding: 0.5em;background: #d9d5d5;'>Quantity</th>");
            salesEmailBody.Append("<th style='border: solid 1px;padding: 0.5em;background: #d9d5d5;'>Vendor#</th>");
            salesEmailBody.Append("<th style='border: solid 1px;padding: 0.5em;background: #d9d5d5;'>Description</th>");
           // salesEmailBody.Append("<th style='border: solid 1px;padding: 0.5em;background: #d9d5d5;'>Unit Cost </th>");
            //salesEmailBody.Append("<th style='border: solid 1px;padding: 0.5em;background: #d9d5d5;'>Total </th>");
            salesEmailBody.Append("</tr>");

            /*var partsList = (from wp in FarmerBrothersEntitites.WorkorderParts
                                             join fbp in FarmerBrothersEntitites.FBClosureParts on wp.Sku equals fbp.ItemNo
                                             join sk in FarmerBrothersEntitites.Skus on wp.Sku equals sk.Sku1 
                                             where wp.WorkorderID == WorkorderId
                                             select new {
                                                 Manufacturer = wp.Manufacturer,
                                                 Quantity = wp.Quantity,
                                                 Vendor = fbp.VendorNo,
                                                 Desc = wp.Description,
                                                 Unit = sk.SKUCost,
                                                 Total = wp.Quantity * sk.SKUCost
                                             }).ToList();
            if(partsList != null)
            {
                salesEmailBody.Append("<tr>");
                foreach (var wp in partsList)
                {
                    salesEmailBody.Append("<td>" + wp.Manufacturer + "</td>");
                    salesEmailBody.Append("<td>" + wp.Quantity + "</td>");
                    salesEmailBody.Append("<td>" + wp.Vendor + "</td>");
                    salesEmailBody.Append("<td>" + wp.Desc + "</td>");
                    salesEmailBody.Append("<td>" + wp.Unit + "</td>");
                    salesEmailBody.Append("<td>" + wp.Total + "</td>");
                }
                salesEmailBody.Append("</tr>");
            }*/


            List<WorkorderPart> partsList = FarmerBrothersEntitites.WorkorderParts.Where(p => p.WorkorderID == WorkorderId).ToList();

            if (partsList != null)
            {

                foreach (var wp in partsList)
                {
                    salesEmailBody.Append("<tr>");
                    salesEmailBody.Append("<td style='border: solid 1px;padding: 0.5em;'>" + wp.Manufacturer + "</td>");
                    salesEmailBody.Append("<td style='border: solid 1px;padding: 0.5em;'>" + wp.Quantity + "</td>");

                    FBClosurePart prt = FarmerBrothersEntitites.FBClosureParts.Where(s => s.ItemNo == wp.Sku).FirstOrDefault();

                    decimal totl = 0;

                    salesEmailBody.Append("<td style='border: solid 1px;padding: 0.5em;'>" + (prt == null ? "" : prt.VendorNo) + "</td>");
                    salesEmailBody.Append("<td style='border: solid 1px;padding: 0.5em;'>" + (prt == null ? "" : prt.Description) + "</td>");

                    Sku sk = FarmerBrothersEntitites.Skus.Where(s => s.Sku1 == wp.Sku).FirstOrDefault();

                    decimal? unitCost = sk == null ? 0 : sk.SKUCost;
                    //salesEmailBody.Append("<td style='border: solid 1px;padding: 0.5em;'>" + unitCost + "</td>");

                    totl = Convert.ToDecimal(wp.Quantity * unitCost);

                    //salesEmailBody.Append("<td style='border: solid 1px;padding: 0.5em;'>" + totl + "</td>");
                    salesEmailBody.Append("</tr>");
                }

            }

            salesEmailBody.Append("<tbody>");
            salesEmailBody.Append("</table>");

            string contentId = Guid.NewGuid().ToString();
            string logoPath = string.Empty;
            if (Server == null)
            {
                logoPath = Path.Combine(HttpRuntime.AppDomainAppPath, "img/mainlogo.jpg");
            }
            else
            {
                logoPath = Server.MapPath("~/img/mainlogo.jpg");
            }

            salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString
               (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

            LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
            inline.ContentId = contentId;
            avHtml.LinkedResources.Add(inline);

            var message = new MailMessage();

            message.AlternateViews.Add(avHtml);

            message.IsBodyHtml = true;
            message.Body = salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();

            string toAddress = string.Empty;
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
            {
                toAddress = ConfigurationManager.AppSettings["TestEmail"];
            }
            else
            {
                if (workOrder != null)
                {
                    toAddress = "Partsorders@farmerbros.com";
                }
            }

            string fromAddress = ConfigurationManager.AppSettings["DispatchMailFromAddress"];

            string mailTo = toAddress;
            string mailCC = string.Empty;
            if (!string.IsNullOrWhiteSpace(mailTo))
            {
                if (toAddress.Contains("#"))
                {
                    string[] mailCCAddress = toAddress.Split('#');

                    if (mailCCAddress.Count() > 0)
                    {
                        string[] CCAddresses = mailCCAddress[1].Split(';');
                        foreach (string address in CCAddresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.CC.Add(new MailAddress(address));
                            }
                        }
                        string[] addresses = mailCCAddress[0].Split(';');
                        foreach (string address in addresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.To.Add(new MailAddress(address));
                            }
                        }
                    }
                }
                else
                {
                    string[] addresses = mailTo.Split(';');
                    foreach (string address in addresses)
                    {
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            message.To.Add(new MailAddress(address));
                        }
                    }
                }

                message.From = new MailAddress(fromAddress);
                message.Subject = subject.ToString();
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = 25;

                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        result = false;
                    }
                }

            }


            //string fromAddress = ConfigurationManager.AppSettings["DispatchMailFromAddress"];
            //string ToAddr = string.Empty;
            //string CcAddr = string.Empty;
            //if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
            //{
            //    ToAddr = ConfigurationManager.AppSettings["TestEmail"];
            //}
            //else
            //{
            //    if (workOrder != null)
            //    {
            //        ToAddr = "Partsorders@farmerbros.com";
            //    }
            //}

            //EmailUtility eu = new EmailUtility();
            //eu.SendEmail(fromAddress, ToAddr, CcAddr, subject.ToString(), salesEmailBody.ToString());

            return result;
        }

        public JsonResult GetItemNumber(string serialNumber)
        {
            string itemName = "";

            FBCBE fbcbe = FarmerBrothersEntitites.FBCBEs.Where(se => se.SerialNumber == serialNumber).FirstOrDefault();

            if(fbcbe != null)
            {
                itemName = fbcbe.ItemDescription;
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = itemName };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpGet]
        public JsonResult CreateInvoice(string workOrderId)
        {
            try
            {
                int workOrder_Id = Convert.ToInt32(workOrderId);

                Invoice invoicedetails = FarmerBrothersEntitites.Invoices.Where(inv => inv.WorkorderID == workOrder_Id).FirstOrDefault();
                string invoiceId = string.Empty;
                if (invoicedetails != null)
                {
                    invoiceId = invoicedetails.Invoiceid;
                }
                else
                {
                    WorkorderSchedule schedule = FarmerBrothersEntitites.WorkorderSchedules.Where(ws => ws.WorkorderID == workOrder_Id && ws.PrimaryTech >= 0 && ws.AssignedStatus == "Accepted").FirstOrDefault();

                    if (schedule != null)
                    {
                        IndexCounter workOrderCounter = Utility.GetIndexCounter("InvoiceID", 1);
                        workOrderCounter.IndexValue++;

                        Invoice invoice = new Invoice()
                        {
                            Invoiceid = "FB"+workOrderCounter.IndexValue.Value.ToString(),
                            WorkorderID = workOrder_Id,
                            ServiceLocation = schedule.ServiceCenterName
                        };
                        FarmerBrothersEntitites.Invoices.Add(invoice);
                        FarmerBrothersEntitites.SaveChanges();

                        invoiceId = invoice.Invoiceid;
                    }
                }

                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = invoiceId };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            catch (Exception ex)
            {
                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = false, serverError = ErrorCode.SUCCESS, data = "" };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
        }

        public JsonResult UpdateSerialNumberList(int workOrderId, string customerId, string serialNumber = "")
        {
            List<FBCBE> SerialNumberList = FarmerBrothersEntitites.FBCBEs.Where(s => s.CurrentCustomerId.ToString() == customerId.ToString()).ToList();
            List<WorkorderEquipment> WorkOrderEquipments = FarmerBrothersEntitites.WorkorderEquipments.Where(we => we.WorkorderID == workOrderId).ToList();


            foreach (WorkorderEquipment epm in WorkOrderEquipments)
            {
                FBCBE vmm = SerialNumberList.Where(se => se.SerialNumber == epm.SerialNumber).FirstOrDefault();
                if (vmm == null)
                {
                    if (!string.IsNullOrEmpty(epm.SerialNumber))
                    {
                        SerialNumberList.Insert(0, new FBCBE()
                        {
                            Id = -1,
                            SerialNumber = epm.SerialNumber,
                            ItemNumber = string.IsNullOrEmpty(epm.Model) ? "" : epm.Model,
                            ItemDescription = string.IsNullOrEmpty(epm.Model) ? "" : epm.Model,
                        });
                    }
                }
            }

            if (!string.IsNullOrEmpty(serialNumber))
            {
                SerialNumberList.Add(new FBCBE()
                {
                    Id = -1,
                    SerialNumber = serialNumber,
                    ItemNumber = "",
                    ItemDescription = " "
                });
            }

            SerialNumberList.Add(new FBCBE()
            {
                Id = -1,
                SerialNumber = "Other",
                ItemNumber = "-1",
                ItemDescription = " "
            });

            if (SerialNumberList != null && SerialNumberList.Count > 0)
            {
                SerialNumberList.Insert(0, new FBCBE()
                {
                    Id = -1,
                    SerialNumber = "",
                    ItemNumber = "-1",
                    ItemDescription = ""
                });
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = SerialNumberList };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult ElectronicInvoice(int? WorkorderID)
        {
            WorkOrderPdfModel objDisplayPdf = new Models.WorkOrderPdfModel();

            objDisplayPdf.objWorkOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
            objDisplayPdf.objWorkorderSchedule = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == WorkorderID && w.AssignedStatus != "Redirected" && w.AssignedStatus != "Declined").FirstOrDefault();
            objDisplayPdf.Invoice = FarmerBrothersEntitites.Invoices.Where(w => w.InvoiceUniqueid == FarmerBrothersEntitites.Invoices.Where(i => i.WorkorderID == WorkorderID).FirstOrDefault().InvoiceUniqueid).FirstOrDefault();

            WorkorderDetail workOrderDetail = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
            if (workOrderDetail != null)
            {
                objDisplayPdf.CustomerSign = workOrderDetail.CustomerSignatureDetails;
                objDisplayPdf.TechnicianSign = workOrderDetail.TechnicianSignatureDetails;
            }

            objDisplayPdf.objWorkorderDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
            //FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.CustomerSignatureDetails).FirstOrDefault();

            objDisplayPdf.EquipmentRequestedlist = (from WorkEquipment in FarmerBrothersEntitites.WorkorderEquipmentRequesteds
                                                    join WorkType in FarmerBrothersEntitites.WorkorderTypes
                                                    on WorkEquipment.CallTypeid equals WorkType.CallTypeID into temp1
                                                    from we in temp1.DefaultIfEmpty()
                                                    join Symptom in FarmerBrothersEntitites.Symptoms
                                                    on WorkEquipment.Symptomid equals Symptom.SymptomID into sympt
                                                    from sy in sympt.DefaultIfEmpty()
                                                    where WorkEquipment.WorkorderID == WorkorderID
                                                    select new WorkorderEquipmentDetailModel()
                                                    {
                                                        Assetid = WorkEquipment.Assetid,
                                                        WorkOrderType = we.Description,
                                                        Temperature = WorkEquipment.Temperature,
                                                        Weight = WorkEquipment.Weight,
                                                        Ratio = WorkEquipment.Ratio,
                                                        WorkPerformedCounter = WorkEquipment.WorkPerformedCounter,
                                                        WorkDescription = WorkEquipment.WorkDescription,
                                                        Category = WorkEquipment.Category,
                                                        Manufacturer = WorkEquipment.Manufacturer,
                                                        Model = WorkEquipment.Model,
                                                        Location = WorkEquipment.Location,
                                                        SerialNumber = WorkEquipment.SerialNumber,
                                                        QualityIssue = WorkEquipment.QualityIssue,
                                                        Email = WorkEquipment.Email,
                                                        SymptomDesc = sy.Description
                                                    }).ToList();

            IList<WorkOrderPartModel> Parts;


            foreach (var item in objDisplayPdf.EquipmentRequestedlist)
            {
                Parts = new List<WorkOrderPartModel>();
                IQueryable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.AssetID == item.Assetid);
                foreach (WorkorderPart workOrderPart in workOrderParts)
                {
                    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);

                    if (!string.IsNullOrEmpty(workOrderPartModel.Sku))
                    {
                        Sku sk = FarmerBrothersEntitites.Skus.Where(w => w.Sku1 == workOrderPartModel.Sku).FirstOrDefault();
                        if (sk != null)
                        {
                            workOrderPartModel.skuCost = sk.SKUCost == null ? 0 : Convert.ToDecimal(sk.SKUCost);
                        }
                    }
                    else
                    {
                        workOrderPartModel.skuCost = 0;
                    }

                    Parts.Add(workOrderPartModel);
                }

                item.Parts = Parts;
            }

            //objDisplayPdf.EquipmentRequestedSuperlist = SplitToSubList(objDisplayPdf.EquipmentRequestedlist, 40);

            // for closure equipments
            objDisplayPdf.Equipmentlist = (from WorkEquipment in FarmerBrothersEntitites.WorkorderEquipments
                                           join WorkType in FarmerBrothersEntitites.WorkorderTypes
                                           on WorkEquipment.CallTypeid equals WorkType.CallTypeID into temp1
                                           from we in temp1.DefaultIfEmpty()
                                           join Solution in FarmerBrothersEntitites.Solutions
                                           on WorkEquipment.Solutionid equals Solution.SolutionId into solnum
                                           from soln in solnum.DefaultIfEmpty()
                                           where WorkEquipment.WorkorderID == WorkorderID
                                           select new WorkorderEquipmentDetailModel()
                                           {
                                               Assetid = WorkEquipment.Assetid,
                                               WorkOrderType = we.Description,
                                               Temperature = WorkEquipment.Temperature,
                                               Weight = WorkEquipment.Weight,
                                               Ratio = WorkEquipment.Ratio,
                                               WorkPerformedCounter = WorkEquipment.WorkPerformedCounter,
                                               WorkDescription = WorkEquipment.WorkDescription,
                                               Category = WorkEquipment.Category,
                                               Manufacturer = WorkEquipment.Manufacturer,
                                               Model = WorkEquipment.Model,
                                               Location = WorkEquipment.Location,
                                               SerialNumber = WorkEquipment.SerialNumber,
                                               QualityIssue = WorkEquipment.QualityIssue,
                                               Email = WorkEquipment.Email,
                                               SolutionDesc = soln.Description
                                           }).ToList();


            IList<WorkOrderPartModel> ClosureEquipmentParts = new List<WorkOrderPartModel>(); ;

            decimal partsTotal = 0;
            foreach (var item in objDisplayPdf.Equipmentlist)
            {
                ClosureEquipmentParts = new List<WorkOrderPartModel>();
                IQueryable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.AssetID == item.Assetid);
                foreach (WorkorderPart workOrderPart in workOrderParts)
                {
                    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);

                    if (!string.IsNullOrEmpty(workOrderPartModel.Sku))
                    {
                        Sku sk = FarmerBrothersEntitites.Skus.Where(w => w.Sku1 == workOrderPartModel.Sku).FirstOrDefault();
                        if (sk != null)
                        {
                            workOrderPartModel.skuCost = sk.SKUCost == null ? 0 : Convert.ToDecimal(sk.SKUCost);
                        }
                    }
                    else
                    {
                        workOrderPartModel.skuCost = 0;
                    }

                    workOrderPartModel.partsTotal = Convert.ToDecimal(workOrderPartModel.skuCost * workOrderPartModel.Quantity);
                    partsTotal += workOrderPartModel.partsTotal;

                    ClosureEquipmentParts.Add(workOrderPartModel);
                }

                item.Parts = ClosureEquipmentParts;
            }

            objDisplayPdf.PartsTotal = partsTotal;//ClosureEquipmentParts.Sum(sm => sm.partsTotal);
            objDisplayPdf.LaborCost = objDisplayPdf.objWorkOrder.TotalUnitPrice == null ? 0 : Convert.ToDecimal(objDisplayPdf.objWorkOrder.TotalUnitPrice);
            objDisplayPdf.Total = objDisplayPdf.PartsTotal + objDisplayPdf.LaborCost;

            objDisplayPdf.MachineNotes = "";
            State state = FarmerBrothersEntitites.States.Where(st => st.StateCode == objDisplayPdf.objWorkOrder.CustomerState).FirstOrDefault();
            objDisplayPdf.TaxPercentage = state == null ? "0%" : (state.TaxPercent + "%");

            //objDisplayPdf.EquipmentSuperlist = SplitToSubList(objDisplayPdf.Equipmentlist, 20);

            var workorderPdfId = objDisplayPdf.objWorkOrder.WorkorderID;
            string htmlView = this.RenderRazorViewToString("~/Views/Invoice/ElectronicInvoice.cshtml", objDisplayPdf).ToString();

            //HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            //WebKitConverterSettings webKitSettings = new WebKitConverterSettings();
            //webKitSettings.WebKitPath = Server.MapPath("~/Content/QtBinaries");
            //htmlConverter.ConverterSettings = webKitSettings;
            //Image[] image = htmlConverter.ConvertToImage(htmlView, Server.MapPath("~/Content/")); 

            StringBuilder salesEmailBody = new StringBuilder();

            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%' style='float: right;margin-right: 100px;margin-bottom: 10px;'>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append(htmlView);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            //salesEmailBody.Append(@"<img src='cid:signature' style='height: 5vw;width: 10vw;''>");

            string contentId = Guid.NewGuid().ToString();
            string contentId1 = Guid.NewGuid().ToString();
            string logoPath = string.Empty;
            if (Server == null)
            {
                logoPath = Path.Combine(HttpRuntime.AppDomainAppPath, "img/mainlogo.jpg");
            }
            else
            {
                logoPath = Server.MapPath("~/img/mainlogo.jpg");
            }

            
            //salesEmailBody.Append("<BR>");
            //salesEmailBody.Append("<img height='100' width='250' alt='Customer Signature' src="+ objDisplayPdf.objWorkorderDetails.CustomerSignatureDetails + " />");

            //salesEmailBody = salesEmailBody.Replace("cid:logo", logoPath);
            //salesEmailBody = salesEmailBody.Replace("Customer:Signature", objDisplayPdf.objWorkorderDetails.CustomerSignatureDetails);

            //AlternateView avHtml = AlternateView.CreateAlternateViewFromString
            //   (htmlView, null, MediaTypeNames.Text.Html);

            //LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
            //inline.ContentId = contentId;
            //avHtml.LinkedResources.Add(inline);

            //var message = new MailMessage();

            //message.AlternateViews.Add(avHtml);

            //message.Body = salesEmailBody.ToString();
            //message.IsBodyHtml = true;


            ///==========================
           
            salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);
            //salesEmailBody = salesEmailBody.Replace("cid:signature", "cid:" + contentId1);

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString
               (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

            LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
            inline.ContentId = contentId;
            avHtml.LinkedResources.Add(inline);
            //LinkedResource inline1 = new LinkedResource(objDisplayPdf.objWorkorderDetails.CustomerSignatureDetails, MediaTypeNames.Text.RichText);
            //inline1.ContentId = contentId1;
            //avHtml.LinkedResources.Add(inline1);

            var message = new MailMessage();

            message.AlternateViews.Add(avHtml);

            message.IsBodyHtml = true;
            message.Body = salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();
            //message.Body = salesEmailBody.Replace("cid:signature", "cid:" + inline1.ContentId).ToString();
            ///==========================

            string toAddress = string.Empty;
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
            {
                toAddress = ConfigurationManager.AppSettings["TestEmail"];
            }
            else
            {
                WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
                if(wo != null)
                {
                    toAddress = wo.CustomerMainEmail;
                }                
            }
            
            string fromAddress = ConfigurationManager.AppSettings["DispatchMailFromAddress"];
            string subject = "Electronic Invoice - " + WorkorderID;

            bool result = true;
            string mailTo = toAddress;
            string mailCC = string.Empty;
            if (!string.IsNullOrWhiteSpace(mailTo))
            {
                if (toAddress.Contains("#"))
                {
                    string[] mailCCAddress = toAddress.Split('#');

                    if (mailCCAddress.Count() > 0)
                    {
                        string[] CCAddresses = mailCCAddress[1].Split(';');
                        foreach (string address in CCAddresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.CC.Add(new MailAddress(address));
                            }
                        }
                        string[] addresses = mailCCAddress[0].Split(';');
                        foreach (string address in addresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.To.Add(new MailAddress(address));
                            }
                        }
                    }
                }
                else
                {
                    string[] addresses = mailTo.Split(';');
                    foreach (string address in addresses)
                    {
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            message.To.Add(new MailAddress(address));
                        }
                    }
                }

                message.From = new MailAddress(fromAddress);
                message.Subject = subject;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = 25;

                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        result = false;
                    }
                }

            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        private static string GetEmailString(int? WorkorderID)
        {
            FarmerBrothersEntities fbEntities = new FarmerBrothersEntities();

            string htmlString = string.Empty;

            WorkOrder workorder = fbEntities.WorkOrders.Where(wo => wo.WorkorderID == WorkorderID).FirstOrDefault();
            string CustomerAcNo = "", WOEntryDate = "", CustomerName = "", CustomerAddress = "", CustomerCity = "";
            string CustomerState = "", CustomerZipCode = "", WOContactNm = "", WOContactPh = "", WorkorderId = "", TotalUnitPrice = "";
            if (workorder != null)
            {
                CustomerAcNo = workorder.CustomerID == null ? "" : workorder.CustomerID.ToString().Trim();
                WOEntryDate = workorder.WorkorderEntryDate == null ? "" : workorder.WorkorderEntryDate.ToString().Trim();
                CustomerName = workorder.CustomerName == null ? "" : workorder.CustomerName.ToString().Trim();
                CustomerAddress = workorder.CustomerAddress == null ? "" : workorder.CustomerAddress.ToString().Trim();
                CustomerCity = workorder.CustomerCity == null ? "" : workorder.CustomerCity.ToString().Trim();
                CustomerState = workorder.CustomerState == null ? "" : workorder.CustomerState.ToString().Trim();
                CustomerZipCode = workorder.CustomerZipCode == null ? "" : workorder.CustomerZipCode.ToString().Trim();
                WOContactNm = workorder.CustomerMainContactName == null ? "" : workorder.CustomerMainContactName.ToString().Trim();
                WOContactPh = workorder.CustomerPhone == null ? "" : workorder.CustomerPhone.ToString().Trim();
                WorkorderId = workorder.WorkorderID.ToString().Trim();
                TotalUnitPrice = workorder.TotalUnitPrice == null ? "" : workorder.TotalUnitPrice.ToString().Trim();
            }


            WorkorderSchedule workorderSch = fbEntities.WorkorderSchedules.Where(w => w.WorkorderID == WorkorderID && w.AssignedStatus != "Redirected" && w.AssignedStatus != "Declined").FirstOrDefault();
            string WOScheduleTechName = "";
            if (workorderSch != null)
            {
                WOScheduleTechName = workorderSch.TechName == null ? "" : workorderSch.TechName.Trim();
            }

            WorkorderDetail workOrderDetail = fbEntities.WorkorderDetails.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
            string TravelTime = "", StartTime = "", ArrivalTime = "", CompletionTime = "";
            string trvlTime = "";
            if (workOrderDetail != null)
            {
                trvlTime = workOrderDetail.TravelTime == null ? "" : workOrderDetail.TravelTime.ToString();
                if (!string.IsNullOrEmpty(trvlTime))
                {
                    if (trvlTime.Contains(':'))
                    {
                        string[] timeSpllit = trvlTime.Split(':');
                        string hours = string.IsNullOrEmpty(timeSpllit[0]) ? "0" : timeSpllit[0].Trim();
                        string minutes = string.IsNullOrEmpty(timeSpllit[1]) ? "0" : timeSpllit[1].Trim();

                        TravelTime = hours + " Hrs : " + minutes + " Min";
                    }
                    else
                    {
                        TravelTime = trvlTime.Trim();
                    }
                }

                StartTime = workOrderDetail.StartDateTime == null ? "" : workOrderDetail.StartDateTime.ToString().Trim();
                ArrivalTime = workOrderDetail.ArrivalDateTime == null ? "" : workOrderDetail.ArrivalDateTime.ToString().Trim();
                CompletionTime = workOrderDetail.CompletionDateTime == null ? "" : workOrderDetail.CompletionDateTime.ToString().Trim();
            }

            List<WorkorderEquipmentDetailModel> eqpReqObjList = new List<WorkorderEquipmentDetailModel>();
            eqpReqObjList = (from WorkEquipment in fbEntities.WorkorderEquipmentRequesteds
                             join WorkType in fbEntities.WorkorderTypes
                             on WorkEquipment.CallTypeid equals WorkType.CallTypeID into temp1
                             from we in temp1.DefaultIfEmpty()
                             join Symptom in fbEntities.Symptoms
                             on WorkEquipment.Symptomid equals Symptom.SymptomID into sympt
                             from sy in sympt.DefaultIfEmpty()
                             where WorkEquipment.WorkorderID == WorkorderID
                             select new WorkorderEquipmentDetailModel()
                             {
                                 Assetid = WorkEquipment.Assetid,
                                 WorkOrderType = we.Description,
                                 Temperature = WorkEquipment.Temperature,
                                 Weight = WorkEquipment.Weight,
                                 Ratio = WorkEquipment.Ratio,
                                 WorkPerformedCounter = WorkEquipment.WorkPerformedCounter,
                                 WorkDescription = WorkEquipment.WorkDescription,
                                 Category = WorkEquipment.Category,
                                 Manufacturer = WorkEquipment.Manufacturer,
                                 Model = WorkEquipment.Model,
                                 Location = WorkEquipment.Location,
                                 SerialNumber = WorkEquipment.SerialNumber,
                                 QualityIssue = WorkEquipment.QualityIssue,
                                 Email = WorkEquipment.Email,
                                 SymptomDesc = sy.Description
                             }).ToList();

            List<WorkorderEquipmentDetailModel> eqpObjList = new List<WorkorderEquipmentDetailModel>();
            eqpObjList = (from WorkEquipment in fbEntities.WorkorderEquipments
                          join WorkType in fbEntities.WorkorderTypes
                          on WorkEquipment.CallTypeid equals WorkType.CallTypeID into temp1
                          from we in temp1.DefaultIfEmpty()
                          join Solution in fbEntities.Solutions
                          on WorkEquipment.Solutionid equals Solution.SolutionId into solnum
                          from soln in solnum.DefaultIfEmpty()
                          where WorkEquipment.WorkorderID == WorkorderID
                          select new WorkorderEquipmentDetailModel()
                          {
                              Assetid = WorkEquipment.Assetid,
                              WorkOrderType = we.Description,
                              Temperature = WorkEquipment.Temperature,
                              Weight = WorkEquipment.Weight,
                              Ratio = WorkEquipment.Ratio,
                              WorkPerformedCounter = WorkEquipment.WorkPerformedCounter,
                              WorkDescription = WorkEquipment.WorkDescription,
                              Category = WorkEquipment.Category,
                              Manufacturer = WorkEquipment.Manufacturer,
                              Model = WorkEquipment.Model,
                              Location = WorkEquipment.Location,
                              SerialNumber = WorkEquipment.SerialNumber,
                              QualityIssue = WorkEquipment.QualityIssue,
                              Email = WorkEquipment.Email,
                              SolutionDesc = soln.Description
                          }).ToList();


            //===================================================================================================
            IList<WorkOrderPartModel> ClosureEquipmentParts = new List<WorkOrderPartModel>(); ;

            decimal partsTotal = 0;
            foreach (var item in eqpObjList)
            {
                ClosureEquipmentParts = new List<WorkOrderPartModel>();
                IQueryable<WorkorderPart> workOrderParts = fbEntities.WorkorderParts.Where(wp => wp.AssetID == item.Assetid);
                foreach (WorkorderPart workOrderPart in workOrderParts)
                {
                    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);

                    if (!string.IsNullOrEmpty(workOrderPartModel.Sku))
                    {
                        Sku sk = fbEntities.Skus.Where(w => w.Sku1 == workOrderPartModel.Sku).FirstOrDefault();
                        if (sk != null)
                        {
                            workOrderPartModel.skuCost = sk.SKUCost == null ? 0 : Convert.ToDecimal(sk.SKUCost);
                        }
                    }
                    else
                    {
                        workOrderPartModel.skuCost = 0;
                    }

                    workOrderPartModel.partsTotal = Convert.ToDecimal(workOrderPartModel.skuCost * workOrderPartModel.Quantity);
                    partsTotal += workOrderPartModel.partsTotal;

                    ClosureEquipmentParts.Add(workOrderPartModel);
                }

                item.Parts = ClosureEquipmentParts;
            }

            decimal PartsTotal = Math.Round(partsTotal, 2);
            double LaborCost = 112.50;// TotalUnitPrice == "" ? "0" : TotalUnitPrice;


            decimal TravelRate = 0;
            decimal rate = 75;
            if (!string.IsNullOrEmpty(trvlTime))
            {
                if (trvlTime.Contains(':'))
                {
                    string[] timeSpllit = trvlTime.Split(':');
                    string hours = string.IsNullOrEmpty(timeSpllit[0]) ? "0" : timeSpllit[0].Trim();
                    string minutes = string.IsNullOrEmpty(timeSpllit[1]) ? "0" : timeSpllit[1].Trim();

                    TravelRate = Math.Round(((Convert.ToDecimal(hours) * rate) + ((Convert.ToDecimal(minutes) / 60) * rate)), 2);
                }
                else
                {
                    TravelRate = Math.Round((Convert.ToDecimal(trvlTime) * rate), 2);
                }
            }

            decimal tmpTotal = Math.Round((partsTotal + TravelRate + Convert.ToDecimal(LaborCost)), 2);
            //===================================================================================================
            State state = fbEntities.States.Where(st => st.StateCode == CustomerState).FirstOrDefault();
            string taxValue = ""; decimal? taxcalculationValue = 0;
            if (state != null)
            {
                taxcalculationValue = state.TaxPercent == null ? 0 : state.TaxPercent;
                taxValue = taxcalculationValue + "%";
            }
            decimal TaxCostValue = Math.Round(Convert.ToDecimal((taxcalculationValue / 100)) * tmpTotal, 2);

            decimal Total = Math.Round((tmpTotal + TaxCostValue), 2);


            string machineNotes = "";

            List<NotesHistory> NHList = fbEntities.NotesHistories.Where(nt => nt.WorkorderID == WorkorderID).ToList();
            if (NHList != null)
            {
                foreach (NotesHistory nh in NHList)
                {
                    if (nh.Notes != null && nh.Notes.Contains("Comments"))
                    {
                        string[] commentStr = nh.Notes.Split(':');
                        machineNotes += string.IsNullOrEmpty(commentStr[1]) ? "" : (commentStr[1].Trim() + "\n");
                    }
                }
            }
            
            //-------------------------------------------------------------------------------------------------------
            htmlString += @"<html><body><table border='0' width='100%' cellpadding='0' cellspacing='0'>
  <tr>
    <td height='30'>&nbsp;</td>
  </tr>
  <tr>
    <td width='100%' align='center' valign='top'><table width='850' border='0' cellpadding='0' cellspacing='0' align='center' style='background:#ECECEC'>
        <tr bgcolor='#49463f'>
          <td height='10' bgcolor='#49463f'></td>
        </tr>
        <tr bgcolor='#49463f'>
          <td bgcolor='#49463f'><table border='0' width='820' align='center' cellpadding='0' cellspacing='0'>
              <tr>
                <td><table border='0' align='left' cellpadding='0' cellspacing='0'>
                    <tr>
                      <td align='center'><a href='#' style='display: block;'><img width='150' style='display:block;' src='cid:logo' alt='logo' /></a></td>
                    </tr>
                  </table>
                  </td>
              </tr>
            </table></td>
        </tr>
        <tr bgcolor='#49463f'>
          <td height='10' bgcolor='#49463f'></td>
        </tr>
        <tr>
          <td><table border='0' width='820' align='center' cellpadding='0' cellspacing='0'>
              <tr bgcolor='ffffff'>
                <td height='20'>&nbsp;</td>
              </tr>              
              <tr bgcolor='ffffff'>
                <td><table width='780' border='0' align='center' cellpadding='0' cellspacing='0' style='border-bottom:2px dotted #BEBEBE;'>
                    <tr>
                      <td><table border='0' width='48%' align='left' cellpadding='0' cellspacing='0'>
                          <tr>
                            <td style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'>
Customer Account Number#:&nbsp;<span style='font-weight:normal;text-align:left;'>" + CustomerAcNo + @"</span></td>
                          </tr>
                          <tr><td height='12'></td></tr>
                          <tr>
                            <td style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 	
Technician Name:&nbsp;<span style='font-weight:normal;text-align:left;'>" + WOScheduleTechName + @"</span></td>
                          </tr>
                          <tr><td height='12'></td></tr>
                          <tr>
                            <td style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
Service Contact:&nbsp;<span style='font-weight:normal;text-align:left;'>" + WOContactNm + @"</span></td>
                          </tr>

                          <tr><td height='12'>&nbsp;</td></tr>
                        </table>
                        <table border='0' width='48%' style='float:right;' align='left' cellpadding='0' cellspacing='0' class='section-item'>
                          <tr>
                            <td style='color:#484848;font-size: 13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> Workorder ID:&nbsp;<span style='font-weight: normal;text-align:left;'>" + WorkorderId + @"</span></td>
                          </tr>
                          <tr><td height='12'></td></tr> 
                          <tr>
                            <td style='color:#484848;font-size: 13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> Phone Number:&nbsp;<span style='font-weight: normal;text-align:left;'>" + WOContactPh + @"</span></td>
                          </tr>
                          <tr><td height='12'></td></tr> 
                          <tr>
                            <td style='color:#484848;font-size: 13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 	
Service Date:&nbsp;<span style='font-weight: normal;text-align:left;'>" + WOEntryDate + @"</span></td>
                          </tr>
                          <tr><td height='12'></td></tr>
                        </table></td>
                    </tr>
                    <tr>
                       <td style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'>Customer Location:<span style='font-weight:normal;text-align:left;'>
                                                        <label>" + CustomerName + @"</label></br>
                                                        <label>" + CustomerAddress + @"</label></br>
                                                        <label>" + CustomerCity + @" </label>
                                                        <label>" + CustomerState + @"</label>
                                                        <label>" + CustomerZipCode + @" </label>
                        </span></td>
                    </tr>
                    <tr><td height='12'>&nbsp;</td></tr>
                  </table></td>
              </tr>
              <tr bgcolor='ffffff'>
                <td height='10'></td>
              </tr>
               <tr bgcolor='ffffff'>
                <td><table width='780' border='0' align='center' cellpadding='0' cellspacing='0'>
                	<tr>
                    	<td><h4 style='color:#484848;font-size:15px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;margin:0;padding:0;'>Asset Details</h4></td>
                    </tr>
                    <tr><td height='10'></td></tr>
                    <tr>
                      <td><table border='1' bordercolor='#999999' width='100%' align='left' cellpadding='0' cellspacing='0'>
                          <tr bgcolor='#CCCCCC'>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Service Code</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Equipment Type</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Manufacturer</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Asset ID</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Model</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Serial Number</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Completion Code</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Temperature</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Describe Work Performed</th>
                          </tr>";
            foreach (WorkorderEquipmentDetailModel ep in eqpObjList)
            {
                htmlString += @"

                          <tr>
                          	<td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + ep.WorkOrderType + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + ep.Category + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + ep.Manufacturer + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + ep.Assetid + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + ep.Model + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + ep.SerialNumber + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + ep.SolutionDesc + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + ep.Temperature + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + ep.WorkDescription + @"</td>
                          </tr>";
                if (ep.Parts.Count() > 0)
                {
                    htmlString += @"<tr>
                          <td></td>
                          <td colspan='8'>
                          		<table border='1' bgcolor='#fff1ca' bordercolor='#999999' width='100%' align='left' cellpadding='0' cellspacing='0'>
                                  <tr bgcolor='#ffc41e'>
                                    <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 23px;'>Quantity</th>
                                    <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 23px;'>Manufacturer</th>
                                    <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 23px;'>SKU</th>
                                    <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 23px;'>Description</th>                                    
                                    <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 23px;'>Parts Total</th>
                                  </tr>";
                    foreach (WorkOrderPartModel wop in ep.Parts)
                    {

                        htmlString += @"<tr>
                          	                <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 23px;'>" + wop.Quantity + @"</td>
                                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 23px;'>" + wop.Manufacturer + @"</td>
                                            <td style='color:#484848;font-size:13px;text-align:left;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 23px;'>" + wop.Sku + @"</td>
                                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 23px;'>" + wop.Description + @"</td>
                                            <td style='color:#484848;font-size:13px;text-align:right;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 23px;'>" + String.Format("{0:C}", wop.partsTotal) + @"</td>
                                          </tr>";
                    }
                    htmlString += @"
</table>
                          </td>
                          </tr>";
                }
                htmlString += @"";
            }
            htmlString += @"</table>
                        </td>
                    </tr>
                    <tr><td height='15' style='border-bottom:2px dotted #BEBEBE;'></td></tr>
                  </table></td>
              </tr>
              <tr bgcolor='ffffff'>
                <td height='10'></td>
              </tr>
              <tr bgcolor='ffffff'>
                      <td><table width='780' border='0' align='center' cellpadding='0' cellspacing='0' style='border-bottom:2px dotted #BEBEBE;'>
                          <tr>
                            <td><table border='0' width='100%' align='left' cellpadding='0' cellspacing='0'>
                                <tr>
                                  <td align='right' style='color:#484848;font-size:13px;font-family:Helvetica, Arial, sans-serif;text-align:right;'><b>Travel Rate:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>" + String.Format("{0:C}", TravelRate) + @"</td>
                                </tr>    
                                <tr><td height='12'></td></tr>                            
                                <tr>
                                  <td align='right' style='color:#484848;font-size:13px;font-family:Helvetica, Arial, sans-serif;text-align:right;'><b>Labor:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>" + String.Format("{0:C}", LaborCost) + @"</td>
                                </tr>
                                <tr><td height='12'></td></tr>
                                <tr>
                                  <td align='right' style='color:#484848;font-size:13px;font-family:Helvetica, Arial, sans-serif;text-align:right;'><b>Parts:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>" + String.Format("{0:C}", partsTotal) + @"</td>
                                </tr>
                                <tr><td height='12'></td></tr>
                                <tr>
                                  <td align='right' style='color:#484848;font-size:13px;font-family:Helvetica, Arial, sans-serif;text-align:right;'><b>Tax:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>" + String.Format("{0:C}", TaxCostValue) + @"</td>
                                </tr>
                                <tr><td height='12'></td></tr>
                                <tr>
                                  <td align='right' style='color:#e50322;font-size:13px;font-family:Helvetica, Arial, sans-serif;text-align:right;'><b>Total:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + String.Format("{0:C}", Total) + @"</b></td>
                                </tr>
                                <tr><td height='12'></td></tr>
                              </table></td>
                          </tr>
                        </table></td>
                    </tr>
                    
              <tr bgcolor='ffffff'>
                <td height='10'></td>
              </tr>
<tr bgcolor='ffffff'>
                <td><table width='780' border='0' align='center' cellpadding='0' cellspacing='0'>
                	<tr>
                    	<td><h4 style='color:#484848;font-size:15px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;margin:0;padding:0;'>Accessories </h4></td>
                    </tr>
                    <tr><td height='10'></td></tr>
                    <tr>
                      <td><table border='1' bordercolor='#999999' width='100%' align='left' cellpadding='0' cellspacing='0'>
                          <tr bgcolor='#CCCCCC'>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Equipment Type</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Service Code</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Symptom</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Equipment Location</th>
                            <th style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;height: 26px;'>Serial Number</th>
                          </tr>";
            htmlString += " <tbody> ";
            foreach (WorkorderEquipmentDetailModel epreq in eqpReqObjList)
            {
                htmlString += @"<tr>
                          	<td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + epreq.Category + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'> " + epreq.WorkOrderType + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + epreq.SymptomDesc + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + epreq.Location + @"</td>
                            <td style='color:#484848;font-size:13px;text-align:center;font-weight:normal;font-family:Helvetica, Arial, sans-serif;height: 26px;'>" + epreq.SerialNumber + @"</td>
                          </tr>";
            }

            htmlString += @"  </tbody></table>
                        </td>
                    </tr>
                  </table></td>
              </tr>             
              <tr bgcolor='ffffff'>
                <td height='15'></td>
              </tr>
              <tr bgcolor='ffffff'>
                <td><table width='780' border='0' align='center' cellpadding='0' cellspacing='0'>
                    <tr>
                      <td><table border='0' width='100%' align='left' cellpadding='0' cellspacing='0'>
                          <tr>
                            <td style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
Start Time:&nbsp;<span style='font-weight:normal;text-align:left;'>" + StartTime + @"</span></td>
 
                            <td style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'>	
Arrival Time:&nbsp;<span style='font-weight:normal;text-align:left;'>" + ArrivalTime + @"</span></td>
 </tr>
                          <tr>
                            <td style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
Completion Time:&nbsp;<span style='font-weight:normal;text-align:left;'>" + CompletionTime + @"</span></td>

                            <td style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
Travel Time:&nbsp;<span style='font-weight:normal;text-align:left;'>" + TravelTime + @"</span></td>
                          </tr>
                          <tr><td colspan='3' height='15'></td></tr>
                          <tr>
                            <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
Machine Notes:&nbsp;<span style='font-weight:normal;width:674px;float: right;'>" + machineNotes + @"</span></td>
                          </tr>
                          <tr><td height='15'></td></tr>
                        </table>
                        </td>
                    </tr>
                  </table></td>
              </tr>
              <tr bgcolor='ffffff'><td height='15'></td></tr>
              <tr bgcolor='ffffff'>
                <td align='center' style='color:#484848;font-size:12px;font-family:Helvetica, Arial, sans-serif;'>
                <a style='text-decoration: none;color:#2996f7' href='https://goo.gl/forms/KgvSAgobIEee3kEz2'>Click here for Customer Satisfaction Survey</a>
                </td>
              </tr>
              <tr bgcolor='ffffff'><td height='15'></td></tr>              
              <tr>
                <td height='20'>&nbsp;</td>
              </tr>
              <tr>
                <td><h4 style='color:#484848;font-size:16px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;margin:0;padding:0;text-align:center;'>TERMS AND CONDITIONS</h4></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
EQUIPMENT DESCRIPTION AND LOCATION:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>The equipment covered by these Equipment Usage Terms and Conditions(this “Agreement”) shall consist of the equipment installed at Operator's location(s) by Farmer Bros. Co. (including any subsidiary or affiliate, hereinafter called “FBC”) as described on the reverse or in any addendum hereto. Operator shall not remove any equipment from the location installed by FBC without FBC's prior written consent.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
EQUIPMENT OWNERSHIP:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>Title to and ownership of the equipment shall at all times remain with FBC.Operator shall not remove or obscure labeling on the equipment indicating that it is the property of FBC. Operator shall not sell, assign, transfer, pledge, hypothecate or otherwise dispose of, encumber or permit a lien to be placed on the equipment.Upon termination of this Agreement, Operator shall provide FBC reasonable access to Operator's location(s) to permit FBC to remove the equipment.Operator shall be responsible for all federal, state or local taxes levied upon the equipment or upon its use, and shall reimburse FBC for any such taxes upon receipt of FBC's invoice for such taxes or pay such taxes directly. Operator shall indemnify FBC for any liability for such taxes.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
USE OF EQUIPMENT:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>Operator shall use the equipment only to dispense, brew, sell or store FBC products purchased from FBC(the ""Products""), and shall not use the equipment to dispense, brew, sell or store any products other than FBC products.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
CARE AND OPERATION:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'> Operator shall maintain and use the equipment in a careful and proper manner in accordance with the written instructions of the equipment manufacturer and FBC, and shall not make any modifications to the equipment without FBC's prior written consent.  Any modifications to the equipment of any kind shall immediately become the property of FBC subject to the terms of this Agreement. Operator shall comply with all laws, ordinances and regulations relating to the possession, use and maintenance of the equipment. FBC shall not be responsible for any damages, claims, injury or liability (collectively, ""Damages"") relating to the operation of the equipment while it is in the possession of Operator (except for Damages caused by the negligence of FBC, its employees, agents or contractors). Operator shall be responsible for all Damages caused by its negligent use of the equipment and for the loss, theft or destruction of the equipment.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
INSTALLATION AND SERVICE:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>FBC will conduct a basic installation of the equipment on Operator's premises at no charge.A basic installation consists of connecting the equipment to an established water line with a shut-off valve and calibrating the equipment for optimum service level.Operator must ensure that the plumbing and electrical are in good working order and compliant with all applicable building codes, landlord requirements or other requirements. At Operator's request and expense, FBC will arrange the services of a licensed contractor to perform electrical or plumbing services.FBC will service the equipment at no additional cost to Operator to the extent FBC sees fit in its discretion. Operator will afford reasonable access to the equipment so that FBC may service the equipment. FBC shall not be responsible for any delays in repairing or replacing equipment.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
INSPECTION AND VERIFICATION:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>FBC or its representatives shall have the right at all reasonable times to enter the premises where the equipment is located for purposes of inspection. Operator agrees to provide a record of serial numbers of beverage equipment at Operator's location(s), upon request by FBC.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
ACCEPTANCE OF EQUIPMENT:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>Operator shall immediately inspect each piece of equipment delivered in accordance with this Agreement and immediately give notice to FBC if any equipment is damaged or different from the type of equipment described on the reverse or in any addendum hereto. If Operator gives no such notice within fourteen(14) days after delivery of any piece of equipment, it shall be conclusively presumed that such equipment was delivered in good condition. <b>THE EQUIPMENT AND ALL SERVICES ARE PROVIDED "" AS IS."" FBC MAKES NO REPRESENTATION OR WARRANTY OF ANY KIND AND EXPRESSLY DISCLAIMS ALL SUCH REPRESENTATIONS AND WARRANTIES, EXPRESS OR IMPLIED, WITH RESPECT TO THE EQUIPMENT AND THE SERVICES, THEIR SUITABILITY OR FITNESS FOR ANY PURPOSE AND THEIR MERCHANTABILITY</b>.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
RISK OF LOSS OR DAMAGE:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>Operator assumes all risk of loss or damage to the equipment from any cause, including but not limited to fire, theft, water damage, accidental overturning, dropping or negligence and agrees to return the equipment to FBC in as good condition as when received, normal wear and tear excepted.In the event of loss or damage to the equipment due to any cause other than ordinary wear and tear including fire, theft or otherwise, Operator shall place the equipment in good repair or pay FBC the value of the equipment.Operator will, to the full extent permitted by law, release, indemnify, defend and hold harmless FBC from any loss, damage, liability, cost, fine or expense, including reasonable attorneys' fees, incurred in connection with the services, or Operator's use, possession or operation of the equipment.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
LIMITATION OF LIABILITY:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>Notwithstanding any provisions in this Agreement or any other agreement between the parties to the contrary, the total overall liability of FBC, whether in contract, tort(including negligence and strict liability) or otherwise is limited to repair or replacement of the equipment, subject to FBC's rights in the paragraph entitled ""REMOVAL OF EQUIPMENT"" below. <b> IN NO EVENT SHALL FBC BE LIABLE IN ANY ACTION, INCLUDING WITHOUT LIMITATION, CONTRACT, TORT, STRICT LIABILITY OR OTHERWISE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, CONSEQUENTIAL, OR PUNITIVE DAMAGES OR PENALTIES, INCLUDING WITHOUT LIMITATION PROCUREMENT OF SUBSTITUTE EQUIPMENT, LOSS OF USE, PROFITS, REVENUE, OR DATA, OR BUSINESS INTERRUPTION ARISING OUT OF OR IN CONNECTION WITH THIS AGREEMENT OR THE EQUIPMENT, EVEN IF FBC WAS ADVISED OF THE POSSIBILITY OF SUCH DAMAGES. ANY ACTION RESULTING FROM ANY BREACH ON THE PART OF FBC AS TO THE EQUIPMENT DELIVERED HEREUNDER MUST BE COMMENCED WITHIN ONE(1) YEAR AFTER THE CAUSE OF ACTION HAS ACCRUED</b>.</span></td>
              </tr>
              <tr>
                <td height='20'>&nbsp;</td>
              </tr>
              <tr>
                <td><h4 style='color:#484848;font-size:16px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;margin:0;padding:0;text-align:left;'>OPERATOR’S REMEDIES: The remedies reserved to FBC in this Agreement shall be cumulative and additional to any other remedies in law or in equity.</h4></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
REMOVAL OF EQUIPMENT:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>If any covenant of this Agreement is breached by Operator, or if any of Operator's property is subjected to levy or seizure by any creditor or government agency, or if bankruptcy proceedings are commenced by or against Operator, or if Operator discontinues business, this shall constitute a breach of this Agreement by Operator and FBC may without notice or demand remove and recover possession of the equipment.In addition, FBC may, without limitation, transfer, remove or reduce the equipment assigned to Operator at any time. Operator understands that FBC assigns equipment based on Operator’s expected volume of purchases from FBC and that Operator's failure to meet expected volumes, or failure to purchase exclusively from FBC, may result in a reduction or removal of equipment assigned by Operator.If Operator prevents FBC, either directly or indirectly, from retaking possession of equipment, Operator shall pay to FBC all costs of retaking said equipment, including reasonable attorneys' fees.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
USE OF FBC MARKS:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>FBC owns certain proprietary and other property rights and interests in and to trademarks, service marks, logo types, insignias, trade dress designs and commercial symbols relating to FBC and its products(the “FBC Marks”), which Operator acknowledges are the sole and exclusive property of FBC, with any goodwill arising from the use thereof to inure solely to the benefit of FBC. FBC may provide Operator with displays, signage and other advertising materials incorporating the FBC Marks or approve Operator’s use of the FBC Marks on Operator's menus. Operator shall use such materials solely in connection with the marketing and sale of FBC products and for no other purpose. If at any time Operator shall cease dispensing FBC products, whether in connection with the termination of this Agreement or otherwise, all rights granted hereunder to Operator to use the FBC Marks shall forthwith terminate, and Operator shall immediately and permanently cease to use, in any manner whatsoever, any FBC Marks and all displays, signage, advertising materials, menus and other materials incorporating the FBC Marks and, upon request, immediately return to FBC all such materials owned by FBC or destroy all such materials owned by Operator incorporating such marks.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
CONFIDENTIALITY:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>Operator agrees to maintain in strict confidence all of the terms and the existence of this Agreement.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
GOVERNING LAW; ARBITRATION:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>This Agreement will be governed by, and construed in accordance with, the laws of the State of Texas, USA without application of the conflict of law principles thereof.Any controversy or claim arising out of or relating to this Agreement, or the breach thereof, or any rights granted hereunder, will be exclusively settled by binding arbitration in Dallas, Texas, USA. The arbitration will be conducted in English and in accordance with the rules of the American Arbitration Association, which will administer the arbitration and act as appointing authority.The decision of the arbitrators will be binding upon the parties hereto, and the expense of the arbitration will be paid as the arbitrator determines.The decision of the arbitrator will be final, and judgment thereon may be entered by any court of competent jurisdiction and application may be made to any court for a judicial acceptance of the award or order of enforcement.</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
WAIVER; VALIDITY; SURVIVAL:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>The failure of either Operator or FBC to insist upon the strict observance and performance of the terms and conditions set forth herein will not be deemed a waiver of other obligations hereunder, nor will it be considered a future or continuing waiver of the same terms. If any term of this Agreement, or any part hereof, not essential to the commercial purpose of this Agreement is held to be illegal, invalid, or unenforceable, it is the intention of the parties that the remaining terms will remain in full force and effect. To the extent legally permissible, any illegal, invalid, or unenforceable provision of this Agreement will be replaced by a valid provision that will implement the commercial purpose of the illegal, invalid, or unenforceable provision.Sections regarding remedies, governing law, disputes and such other sections that by their nature must survive termination in order to affect their intended purpose shall survive termination of this Agreement</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr>
                  <td colspan='3' style='color:#484848;font-size:13px;font-weight:bold;font-family:Helvetica, Arial, sans-serif;'> 
ENTIRE AGREEMENT; ASSIGNMENT:&nbsp;<span style='font-weight:normal;text-align:justify;display: block;'>The terms and conditions set forth herein, or as changed or modified by a written agreement signed by Operator and FBC, shall constitute the entire contract between Operator and FBC with respect to the subject matter herein and shall supersede any additional or inconsistent terms and conditions contained in any proposals, invoices, orders, or any other documents or correspondence of Operator. For the avoidance of doubt, this Agreement does not apply to any goods(other than the equipment) that FBC may sell to Operator.All such goods shall be governed by FBC’s standard Terms and Conditions of Sale, which shall not be superseded by this Agreement.No changes or modifications to the terms and conditions set forth herein shall have any force or effect, unless otherwise agreed to in writing by Operator and FBC. Neither party may assign, delegate or otherwise transfer this Agreement, in whole or in part, without the prior written consent of the other party (not to be unreasonably withheld); provided, that FBC may assign this Agreement to any party controlling, controlled by or under common control with FBC or to any person acquiring all or substantially all of the assets or outstanding capital stock of FBC.Any attempted assignment, delegation, or other transfer of this Agreement in violation of this Section shall be null and void. An electronic image of this document and any signature or acknowledgement thereto will be considered an original (to the same extent as any paper or hard copy), including under evidentiary standards applicable to a proceeding between the parties hereto.[ The current version of this Agreement and any modifications or amendments supersede all prior versions of this Agreement.The most current version of this Agreement may be found at FBC’s website(www.farmerbros.com / ____) and is otherwise available upon request.]</span></td>
              </tr>
              <tr><td height='10'>&nbsp;</td></tr>
              <tr><td height='20'>&nbsp;</td></tr>
            </table></td>
        </tr>
      </table></td>
  </tr>
</table>
</body>
</html>";
            return htmlString;
        }

        //public ActionResult ProcessCard(int workOrderId, int techId)
        //{
        //    WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();

        //    WorkorderSchedule techWorkOrderSchedule = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrderId && w.Techid == techId).FirstOrDefault();           

        //    ProcessCardModel cardModel = new ProcessCardModel();
        //    List<FbWorkorderBillableSKUModel> partsList = new List<FbWorkorderBillableSKUModel>();

        //    var evtPrtList = (from closureSku in FarmerBrothersEntitites.WorkorderParts
        //                      where closureSku.WorkorderID == workOrderId && (closureSku.AssetID == null || closureSku.AssetID == 0)
        //                      select new
        //                      {
        //                          sku = closureSku.Sku,
        //                          des = closureSku.Description,
        //                          qty = closureSku.Quantity,
        //                          evtId = closureSku.WorkorderID,
        //                          unitcost = closureSku.Total / closureSku.Quantity,
        //                          Mnftr = closureSku.Manufacturer
        //                      }).ToList();


        //    cardModel.PartsList = new List<FbWorkorderBillableSKUModel>();
        //    foreach (var wp in evtPrtList)
        //    {
        //        FbWorkorderBillableSKUModel fbsm = new FbWorkorderBillableSKUModel();
        //        fbsm.SKU = wp.sku;
        //        fbsm.WorkorderID = Convert.ToInt32(wp.evtId);
        //        fbsm.UnitPrice = wp.unitcost;
        //        fbsm.Qty = wp.qty;
        //        fbsm.Description = wp.des;

        //        cardModel.PartsList.Add(fbsm);
        //    }

        //    cardModel.SKUList = DispatchResponseController.CloserSKU(FarmerBrothersEntitites);

        //    List<BillingItem> blngItmsList = FarmerBrothersEntitites.BillingItems.Where(b => b.IsActive == true).ToList();
        //    List<CategoryModel> billingItms = new List<CategoryModel>();
        //    foreach (BillingItem item in blngItmsList)
        //    {
        //        billingItms.Add(new CategoryModel(item.BillingName));
        //    }
        //    cardModel.BillingItems = billingItms;

        //    List<BillingModel> bmList = new List<BillingModel>();
        //    WorkorderDetail wd = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
        //    TimeSpan servicetimeDiff = TimeSpan.Zero, trvlTimeDiff = TimeSpan.Zero;
        //    if (wd != null)
        //    {
        //        if (wd.StartDateTime != null && wd.ArrivalDateTime != null)
        //        {
        //            DateTime arrival = Convert.ToDateTime(wd.ArrivalDateTime);
        //            DateTime strt = Convert.ToDateTime(wd.StartDateTime);
        //            trvlTimeDiff = arrival.Subtract(strt);
        //        }

        //        if (wd.ArrivalDateTime != null && wd.CompletionDateTime != null)
        //        {
        //            DateTime arrival = Convert.ToDateTime(wd.ArrivalDateTime);
        //            DateTime cmplt = Convert.ToDateTime(wd.CompletionDateTime);
        //            servicetimeDiff = cmplt.Subtract(arrival);
        //        }
        //    }

        //    Contact contact = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == wo.CustomerID).FirstOrDefault();

        //    var ws = (from sc in FarmerBrothersEntitites.WorkorderSchedules
        //              join t in FarmerBrothersEntitites.TECH_HIERARCHY on sc.Techid equals t.DealerId
        //              where sc.WorkorderID == workOrderId && (sc.AssignedStatus.ToLower() == "sent" || sc.AssignedStatus.ToLower() == "accepted")
        //              //&& t.FamilyAff == "SPT"
        //              select new
        //              {
        //                  Techid = sc.Techid,
        //                  AssignedStatus = sc.AssignedStatus,
        //                  WorkorderID = sc.WorkorderID,
        //                  familyAff = t.FamilyAff
        //              }).FirstOrDefault();


        //    PricingDetail priceDtls = Utility.GetPricingDetails(wo.CustomerID, ws.Techid, wo.CustomerState, FarmerBrothersEntitites);
            
        //    List<WorkorderBillingDetail> wbdList = FarmerBrothersEntitites.WorkorderBillingDetails.Where(w => w.WorkorderId == workOrderId).ToList();
        //    foreach (WorkorderBillingDetail bitem in wbdList)
        //    {
        //        BillingItem blngItm = FarmerBrothersEntitites.BillingItems.Where(b => b.BillingCode == bitem.BillingCode).FirstOrDefault();

        //        if (blngItm != null)
        //        {
        //            decimal tot = 0;

        //            BillingModel bmItem = new BillingModel();
        //            bmItem.BillingType = blngItm.BillingName;
        //            bmItem.BillingCode = bitem.BillingCode;
        //            bmItem.Quantity = Convert.ToInt32(bitem.Quantity);

        //            if (blngItm.BillingName.ToLower() == "travel time")
        //            {
        //                decimal? travelAmt = priceDtls == null ? 0 : priceDtls.HourlyTravlRate;

        //                bmItem.Duration = new DateTime(trvlTimeDiff.Ticks).ToString("HH:mm") + " Hrs";
        //                bmItem.Cost = Convert.ToDecimal(travelAmt);
        //                tot = Convert.ToDecimal(travelAmt * Convert.ToDecimal(trvlTimeDiff.TotalHours));
        //                bmItem.Total = tot;
        //            }
        //            else if (blngItm.BillingName.ToLower() == "labor")
        //            {
        //                decimal? laborAmt = priceDtls == null ? 0 : priceDtls.HourlyLablrRate;

        //                bmItem.Duration = new DateTime(servicetimeDiff.Ticks).ToString("HH:mm") + " Hrs";
        //                bmItem.Cost = Convert.ToDecimal(laborAmt);
        //                tot = Convert.ToDecimal(laborAmt * Convert.ToDecimal(servicetimeDiff.TotalHours));
        //                bmItem.Total = tot;
        //            }
        //            else
        //            {
        //                bmItem.Duration = new DateTime(36000000000).ToString("HH:mm") + " Hrs";
        //                bmItem.Cost = Convert.ToDecimal(blngItm.UnitPrice);
        //                tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
        //                bmItem.Total = tot;
        //            }


        //            cardModel.BillingTotal += tot;
        //            bmList.Add(bmItem);
        //        }
        //    }


        //    if (string.IsNullOrEmpty(wo.FinalTransactionId))
        //    {
        //        string StartTime = null, ArrivalTime = null, CompletionTime = null;
        //        if (wd != null)
        //        {
        //            StartTime = wd.StartDateTime.ToString().Trim();
        //            ArrivalTime = wd.ArrivalDateTime.ToString().Trim();
        //            CompletionTime = wd.CompletionDateTime.ToString().Trim();
        //        }

        //        decimal travelCost = 0, laborCost = 0;
        //        if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(ArrivalTime))
        //        {
        //            DateTime arrival = Convert.ToDateTime(ArrivalTime);
        //            DateTime strt = Convert.ToDateTime(StartTime);
        //            TimeSpan timeDiff = arrival.Subtract(strt);

        //            BillingItem TravelItem = blngItmsList.Where(a => a.BillingName.ToLower() == "travel time").FirstOrDefault();
        //            decimal? travelAmt = priceDtls == null ? 0 : priceDtls.HourlyTravlRate;
        //            travelCost = Convert.ToDecimal(travelAmt * Convert.ToDecimal(timeDiff.TotalHours));

        //            if (travelCost >= 0)
        //            {
        //                BillingModel bmItem = new BillingModel();
        //                bmItem.BillingType = TravelItem.BillingName;
        //                bmItem.BillingCode = TravelItem.BillingCode;
        //                bmItem.Quantity = 1;
        //                bmItem.Duration = new DateTime(timeDiff.Ticks).ToString("HH:mm") + " Hrs";
        //                bmItem.Cost = Convert.ToDecimal(travelAmt);
        //                //decimal tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
        //                //bmItem.Total = tot;
        //                bmItem.Total = travelCost;

        //                //cardModel.BillingTotal += tot;
        //                cardModel.BillingTotal += travelCost;
        //                bmList.Add(bmItem);
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(CompletionTime) && !string.IsNullOrEmpty(ArrivalTime))
        //        {
        //            DateTime arrival = Convert.ToDateTime(ArrivalTime);
        //            DateTime cmplt = Convert.ToDateTime(CompletionTime);
        //            TimeSpan srvcetimeDiff = cmplt.Subtract(arrival);

        //            BillingItem laborItem = blngItmsList.Where(a => a.BillingName.ToLower() == "labor").FirstOrDefault();
        //            decimal? laborAmt = priceDtls == null ? 0 : priceDtls.HourlyLablrRate;
        //            laborCost = Convert.ToDecimal(laborAmt * Convert.ToDecimal(srvcetimeDiff.TotalHours));

        //            if (laborCost >= 0)
        //            {
        //                BillingModel bmItem = new BillingModel();
        //                bmItem.BillingType = laborItem.BillingName;
        //                bmItem.BillingCode = laborItem.BillingCode;
        //                bmItem.Quantity = 1;
        //                bmItem.Duration = new DateTime(srvcetimeDiff.Ticks).ToString("HH:mm") + " Hrs";
        //                bmItem.Cost = Convert.ToDecimal(laborAmt);
        //                //decimal tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
        //                //bmItem.Total = tot;
        //                bmItem.Total = laborCost;

        //                //cardModel.BillingTotal += tot;
        //                cardModel.BillingTotal += laborCost;
        //                bmList.Add(bmItem);
        //            }

        //        }

        //    }


        //    StateTax st = FarmerBrothersEntitites.StateTaxes.Where(s => s.ZipCode == wo.CustomerZipCode).FirstOrDefault();
        //    if (st != null)
        //    {
        //        cardModel.SaleTax = Convert.ToDecimal(st.StateRate);
        //    }

        //    cardModel.PartsDiscount = priceDtls == null ? 0 : Convert.ToDecimal(priceDtls.PartsDiscount);
        //    cardModel.BillingDetails = bmList;
        //    cardModel.WorkorderId = workOrderId;
        //    cardModel.FinalTransactionId = wo.FinalTransactionId;
        //    cardModel.WorkorderEntryDate = wo.WorkorderEntryDate;
        //    cardModel.StartDateTime = wd.StartDateTime;
        //    cardModel.ArrivalDateTime = wd.ArrivalDateTime;
        //    cardModel.CompletionDateTime = wd.CompletionDateTime;

        //    BillingItem prePaymentTravle = blngItmsList.Where(a => a.BillingName.ToLower() == "pre-payment travel").FirstOrDefault();
        //    cardModel.PreTravelCost = prePaymentTravle == null ? 0 : Convert.ToDecimal(prePaymentTravle.UnitPrice);


        //    return View(cardModel);
        //}


    }
}

