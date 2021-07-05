using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FarmerBrothers.Models;
using Newtonsoft.Json;
//using FarmerBrothers.FeastLocationService;
using FarmerBrothers.Utilities;
using LinqKit;
using Syncfusion.MVC.EJ;
using Syncfusion.JavaScript.Models;
using System.Data;
using Syncfusion.EJ.Export;
using Syncfusion.XlsIO;
using System.IO;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Configuration;
using System.Collections;
//using FarmerBrothers.InvoiceServiceReference;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;
using System.Net;
using System.ServiceModel.Channels;
using FarmerBrothers.Data;


namespace FarmerBrothers.Controllers
{

    public class InvoiceController : BaseController
    {
        public const string username = "jmsmars";
        public const string password = "jmsmars2";
        public const string AdjustedpaymentStatus = "Paid with Adjustments";
        public const string SubmittedpaymentStatus = "Submitted for Payment";

/*
        public void fnDisplayInvoiceModel(out DisplayInvoiceModel objInvoiceDisplayModel, int InvoiceUniqueid)
        {
            // string query = @"Select * from feast_tech_hierarchy where DEFAULT_SERVICE_CENTER  = " + serviceCenterId.ToString();

            objInvoiceDisplayModel =
                         (from invoice in FarmerBrothersEntitites.Invoices
                          join workorder in FarmerBrothersEntitites.WorkOrders on invoice.WorkorderID equals workorder.WorkorderID
                          join wrkdetails in FarmerBrothersEntitites.WorkorderDetails on invoice.WorkorderID equals wrkdetails.WorkorderID
                          join wrkscheduled in FarmerBrothersEntitites.WorkorderSchedules on invoice.WorkorderID equals wrkscheduled.WorkorderID
                          where invoice.InvoiceUniqueid == InvoiceUniqueid
                          select new DisplayInvoiceModel
                          {
                              Invoiceid = invoice.Invoiceid,
                              WorkorderID = invoice.WorkorderID,
                              SubmitAmount = invoice.SubmitAmount,
                              InvoiceSubmitDate = invoice.InvoiceSubmitDate,
                              AuthorizedAmount = invoice.PaymentAmtApproved,
                              //AdjustmentAmount = invoice.AdjustmentAmount,
                              InvoiceUniqueid = invoice.InvoiceUniqueid,
                              InvoiceStatus = invoice.InvoiceStatus,
                              FSM = workorder.Fsm,
                              CustomerID = workorder.CustomerID,
                              CustomerName = invoice.CustomerName,
                              CustomerState = workorder.CustomerState,
                              WorkorderCompletionDate = workorder.WorkorderCloseDate,
                              PaymentDate = invoice.PaymentCreated,
                              CheckNumber = invoice.CheckNumber,
                              AdditionalCharge = invoice.AdditionalCharge,
                              PartsTotal = invoice.PartsTotal,
                              LaborTotal = invoice.LaborTotal,
                              TravelTotal = invoice.TravelTotal,
                              MileageTotal = invoice.Mileage,
                              SubTotal = invoice.SubTotal,
                              TaxAmount = invoice.TaxAmount,
                              PhoneSolveID = wrkdetails.PhoneSolveid,
                              Mileage = wrkdetails.Mileage,
                              Comments = invoice.InvoiceComments,
                              InvoiceTotal = invoice.InvoiceTotal,
                              TechID = wrkscheduled.Techid,

                          }).ToList()[0];

            var InvoiceFeastList = FarmerBrothersEntitites.Database.SqlQuery<Invoice_Feast_Data>("Select  * from feast_tech_hierarchy where Tech_Id=" + objInvoiceDisplayModel.TechID).FirstOrDefault();
            //objInvoiceDisplayModel.BranchState = (from p in FormalBrothersEntitites.Database.SqlQuery<string>(@"SELECT TECH_STATE FROM feast_tech_hierarchy where Tech_Id= " + objInvoiceDisplayModel.TPSPVendorID) select p).FirstOrDefault();
            //   objInvoiceDisplayModel.BranchState = FormalBrothersEntitites.Database.SqlQuery<TechHierarchyView>("SELECT TECH_STATE FROM feast_tech_hierarchy where ServiceCenter_Id= ").FirstOrDefault().ToString();

            if (InvoiceFeastList != null)
            {
                objInvoiceDisplayModel.TPSPVendorID = Convert.ToInt32(InvoiceFeastList.TeamLead_Id);
                objInvoiceDisplayModel.TPSPVendorName = InvoiceFeastList.TeamLead_Name;
                objInvoiceDisplayModel.FSM = InvoiceFeastList.FSM_Name;
            }

        }

        [HttpGet]
        public ActionResult DisplayInvoice(int InvoiceUniqueid, int? WorkorderID)
        {
            try
            {
                InvoiceSearchModel objInvoiceSearchModel = new InvoiceSearchModel();
                fnDisplayInvoiceModel(out objInvoiceSearchModel.displayInvoiceModel, InvoiceUniqueid);
                return View(objInvoiceSearchModel);
            }
            catch (Exception ex)
            {
                // log the exception  
                // throw ex;

                return View("Error", new HandleErrorInfo(ex, "objInvoiceSearchModel", "DisplayInvoice"));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        //Generates HTML content of view page.
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

        public ActionResult DisplayPdfInvoice(int InvoiceUniqueid, int? WorkorderID)
        {
            DisplayPdf objDisplayPdf = new Models.DisplayPdf();
            try
            {

                objDisplayPdf.objWorkOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
                objDisplayPdf.customerModel = customerDetails(objDisplayPdf.objWorkOrder.CustomerID.ToString());
                objDisplayPdf.Invoice = FarmerBrothersEntitites.Invoices.Where(w => w.InvoiceUniqueid == InvoiceUniqueid).FirstOrDefault();
                objDisplayPdf.LaborHours = SecstoHours(objDisplayPdf.Invoice.LaborHours ?? 0);
                objDisplayPdf.StandardLabor = SecstoHours(objDisplayPdf.Invoice.StandardLabor ?? 0);
                objDisplayPdf.OvertimeLabor = SecstoHours(objDisplayPdf.Invoice.OvertimeLabor ?? 0);
                objDisplayPdf.TravelTime = SecstoHours(objDisplayPdf.Invoice.TravelTimeInSecs ?? 0);
                //   var ts = TimeSpan.FromSeconds(objDisplayPdf.Invoice.LaborHours);
                // objDisplayPdf.Invoice.LaborHours = objDisplayPdf.Invoice.LaborHours / 3600;
                //   objDisplayPdf.PhoneNumber = Utilities.Utility.FormatPhoneNumber(objDisplayPdf.customerModel.PhoneNumber);
                int ZoneRateID = Convert.ToInt32(objDisplayPdf.Invoice.ZoneRateid);

                var getDesc = FarmerBrothersEntitites.ThirdpartyConMaintenanceZonerates.Where(w => w.ZoneRateid == ZoneRateID).FirstOrDefault();

                if (getDesc != null)
                {
                    objDisplayPdf.ZoneDescription = getDesc.Description;
                }

                //var getTechId = FormalBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == WorkorderID).FirstOrDefault();
                var getTechId = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == WorkorderID && w.AssignedStatus == "Accepted").FirstOrDefault();

                var teamleadid = FarmerBrothersEntitites.Database.SqlQuery<double>(
                                 "Select teamlead_id from feast_tech_hierarchy where tech_id=" + getTechId.Techid).FirstOrDefault();

                objDisplayPdf.ThirdpartyConMaintenanceZonerate = FarmerBrothersEntitites.ThirdpartyConMaintenanceZonerates.Where(w => w.Techid == teamleadid).ToList();
                objDisplayPdf.ThirdPartyContractMaintenance = FarmerBrothersEntitites.ThirdPartyContractMaintenances.Where(w => w.Techid == teamleadid).FirstOrDefault();

                objDisplayPdf.Equipmentlist = (from WorkEquipment in FarmerBrothersEntitites.WorkorderEquipments
                                               join WorkType in FarmerBrothersEntitites.WorkorderTypes
                                               on WorkEquipment.CallTypeid equals WorkType.CallTypeID
                                               join SolutionTbl in FarmerBrothersEntitites.Solutions
                                               on WorkEquipment.Solutionid equals SolutionTbl.SolutionId
                                               join SystemTbl in FarmerBrothersEntitites.SystemInfoes
                                                on WorkEquipment.Systemid equals SystemTbl.SystemId
                                               join SymptomsTbl in FarmerBrothersEntitites.Symptoms
                                                 on WorkEquipment.Symptomid equals SymptomsTbl.SymptomID
                                               where WorkEquipment.WorkorderID == WorkorderID
                                               select new WorkorderEquipmentModel
                                               {
                                                   Assetid = WorkEquipment.Assetid,
                                                   WorkOrderType = WorkType.Description,
                                                   Temperature = WorkEquipment.Temperature,
                                                   WorkPerformedCounter = WorkEquipment.WorkPerformedCounter,
                                                   WorkDescription = WorkEquipment.WorkDescription,
                                                   Category = WorkEquipment.Category,
                                                   Manufacturer = WorkEquipment.Manufacturer,
                                                   Model = WorkEquipment.Model,
                                                   Location = WorkEquipment.Location,
                                                   SerialNumber = WorkEquipment.SerialNumber,
                                                   SolutionDesc = SolutionTbl.Description,
                                                   Settings = WorkEquipment.Settings,
                                                   SystemDesc = SystemTbl.Description,
                                                   SymptomDesc = SymptomsTbl.Description,
                                                   QualityIssue = WorkEquipment.QualityIssue

                                               }).ToList();

                var InvoicePdfId = objDisplayPdf.Invoice.Invoiceid;
                string value = this.RenderRazorViewToString("~/Views/Invoice/DisplayPdfInvoice.cshtml", objDisplayPdf).ToString();

                ViewBag.Message = value;

                ViewData["Status"] = "Success";

                // Html to PDF rendering using Webkit.
                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
                WebKitConverterSettings webKitSettings = new WebKitConverterSettings();
                webKitSettings.WebKitPath = Server.MapPath("~/Content/QtBinaries");
                webKitSettings.PdfPageSize = PdfPageSize.A4;
                webKitSettings.EnableHyperLink = true;
                webKitSettings.EnableJavaScript = true;
                htmlConverter.ConverterSettings = webKitSettings;
                PdfDocument pdfDoc = htmlConverter.Convert("<html><body><div>" + value + "</div></body></html>", Server.MapPath("~/Content/"));
                pdfDoc.PageSettings.Size = PdfPageSize.A4;
                pdfDoc.Save("InvoiceFor_" + InvoicePdfId + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

            }
            catch (Exception ex)
            {
                ViewData["Status"] = "Failed";
                // log the exception        
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
                return View("Error", new HandleErrorInfo(ex, "objDisplayPdf", "DisplayPdfInvoice"));
            }
            return View(objDisplayPdf);
        }

        //public List<ThirdParty> fnThirdPartyList()
        //{
        //    try
        //    {
        //        List<ThirdParty> ThirdPartyList = new List<ThirdParty>();
        //        if (feastLocationsClient != null)
        //        {
        //            CustomerRequest customerRequest = new CustomerRequest()
        //            {
        //                CustomerType = "Agent",
        //                CustomerDescription = "TPSP Vendor",
        //            };
        //            CustomerResponse response = feastLocationsClient.getCustomer(customerRequest);
        //            if (response != null)
        //            {
        //                if (response.Customer != null)
        //                {
        //                    if (response.Customer.Count() > 0)
        //                    {
        //                        foreach (FormalBrothers.FeastLocationService.Customer objCustomer in response.Customer)
        //                        {
        //                            if (objCustomer.CustomerId != null && objCustomer.CustomerName != null && objCustomer.CustomerId != string.Empty && objCustomer.CustomerName != string.Empty)
        //                                ThirdPartyList.Add((new ThirdParty { ThirdPartyId = Convert.ToInt32(objCustomer.CustomerId), ThirdPartyName = objCustomer.CustomerName }));
        //                        }
        //                    }
        //                }

        //            }

        //        }
        //        return ThirdPartyList.OrderBy(x => x.ThirdPartyName).ToList(); 
        //    }
        //    catch (Exception ex)
        //    {
        //        //return View("Error", new HandleErrorInfo(ex, "objThirdPartyModel", "ThirdPartyMaintenance"));
        //        //log the exception;
        //        return ThirdPartyList.OrderBy(x => x.ThirdPartyName).ToList(); 
        //    }
        //}

        public List<FSM_View> fn_FSM_View()
        {
            List<FSM_View> FSMList = new List<FSM_View>();

            try
            {
                MarsViews objMarsView = new MarsViews();
                string StrSql = string.Empty;
                StrSql = "Select distinct FSM_ID, FSM_Name from FEAST_FSM";
                foreach (DataRow dr in objMarsView.fn_FSM_View(StrSql).Rows)
                {
                    FSM_View FSMData = new FSM_View();
                    FSMData.FsmID = Convert.ToInt32(dr["FSM_ID"].ToString());
                    FSMData.FsmName = dr["FSM_Name"].ToString();
                    FSMList.Add(FSMData);
                }
            }
            catch (Exception ex)
            {
                // return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
            }
            FSMList = FSMList.OrderBy(x => x.FsmName).ToList();
            FSM_View blankFsm = new FSM_View() { FsmID = 0, FsmName = "Please Select" };
            FSMList.Insert(0, blankFsm);
            return FSMList;
        }

        public ActionResult InvoiceSearch()
        {
            InvoiceSearchModel invoiceSearchModel = new InvoiceSearchModel();

            if (invoiceSearchModel.FSMList == null)
            {
                invoiceSearchModel.FSMList = fn_FSM_View();
                //ViewBag.FsmList = new SelectList(fn_FSM_View(), "strEligibilityLocationID", "strEligibilityLocationIdName");
                //invoiceSearchModel.FSMList = ViewBag.FsmList;
                // if (objThirdPartyModel.ThirdPartyList == null)
                //   objThirdPartyModel.ThirdPartyList = fnThirdPartyList();
            }

            TempData["ThirdPartMaintenanceData"] = null;
            TempData["objInvoiceSearchModel"] = null;
            return View(invoiceSearchModel);
        }

        [HttpPost]
        public JsonResult Search(InvoiceSearchModel objInvoiceSearchModel)
        {
            try
            {
                if (objInvoiceSearchModel != null)
                {
                    TempData["objInvoiceSearchModel"] = objInvoiceSearchModel;
                }
                objInvoiceSearchModel.SearchResults = GetInvoiceSearchData(objInvoiceSearchModel);
                if (objInvoiceSearchModel.SearchResults != null)
                    objInvoiceSearchModel.SearchResults.Select(c =>
                    {
                        c.InvoiceSubmitDateString = c.InvoiceSubmitDate == null ? string.Empty : Convert.ToDateTime(c.InvoiceSubmitDate).ToString("MM/dd/yyyy");
                        c.DateSubmissionForPaymentString = c.DateSubmissionForPayment == null ? string.Empty : Convert.ToDateTime(c.DateSubmissionForPayment).ToString("MM/dd/yyyy");
                        c.WorkorderCompletionDateString = c.WorkorderCompletionDate == null ? string.Empty : Convert.ToDateTime(c.WorkorderCompletionDate).ToString("MM/dd/yyyy");
                        c.PaymentDateString = c.PaymentDate == null ? string.Empty : Convert.ToDateTime(c.PaymentDate).ToString("MM/dd/yyyy");
                        return c;
                    }).ToList();
                else
                    objInvoiceSearchModel.SearchResults = new List<InvoiceSearchResults>();
            }
            catch (Exception ex)
            {
                // log the exception
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
                // return View("Error", new HandleErrorInfo(ex, "objThirdPartyModel", "ThirdPartyMaintenance"));
            }
            return Json(objInvoiceSearchModel.SearchResults, JsonRequestBehavior.AllowGet);
        }

        private IList<InvoiceSearchResults> GetInvoiceSearchData(InvoiceSearchModel objInvoiceSearchModel)
        {
            MarsViews objMarsView = new MarsViews();
            IQueryable<Invoice_Feast_Data> InvoiceFeastList = FarmerBrothersEntitites.Database.SqlQuery<Invoice_Feast_Data>("Select  * from feast_tech_hierarchy").ToList().AsQueryable();

            var query =
                  (from invoice in FarmerBrothersEntitites.Invoices
                   join workorder in FarmerBrothersEntitites.WorkOrders on invoice.WorkorderID equals workorder.WorkorderID
                   where workorder.WorkorderCallstatus == "Closed"
                   join wrkdetails in FarmerBrothersEntitites.WorkorderDetails on invoice.WorkorderID equals wrkdetails.WorkorderID
                   join wrkscheduled in FarmerBrothersEntitites.WorkorderSchedules on invoice.WorkorderID equals wrkscheduled.WorkorderID
                   where wrkscheduled.AssignedStatus.ToLower() == "accepted"
                   //&& wrkscheduled.PrimaryTech==1 
                   select new InvoiceSearchResults
                   {
                       InvoiceUniqueid = invoice.InvoiceUniqueid,
                       Invoiceid = invoice.Invoiceid,
                       InvoiceStatus = invoice.InvoiceStatus,
                       WorkorderID = invoice.WorkorderID,
                       SubmitAmount = invoice.SubmitAmount,
                       InvoiceSubmitDate = invoice.InvoiceSubmitDate,
                       AuthorizedAmount = invoice.PaymentAmtApproved,
                       FSM = workorder.Fsm,
                       CustomerID = workorder.CustomerID,
                       CustomerName = invoice.CustomerName,
                       WorkorderCompletionDate = workorder.WorkorderCloseDate,
                       PaymentDate = invoice.PaymentCreated,
                       CheckNumber = invoice.CheckNumber,
                       TPSPVendorID = wrkscheduled.TeamLeadID,
                       TPSPVendorName = wrkscheduled.TeamLeadName,
                       BranchID = wrkscheduled.ServiceCenterID,
                       BranchName = wrkscheduled.ServiceCenterName,
                       FSMID = wrkscheduled.FSMID,
                       TechID = wrkscheduled.Techid,
                       //ApproveBy = invoice.ApprovedBy,
                   });




            var predicate = PredicateBuilder.True<InvoiceSearchResults>();
            bool bSearch = false;

            if (objInvoiceSearchModel.SelectedInvoiceStatus != "Please Select")
            {
                predicate = predicate.And(i => i.InvoiceStatus == (objInvoiceSearchModel.SelectedInvoiceStatus));
                bSearch = true;
            }
            if (objInvoiceSearchModel.FSM != 0)
            {
                predicate = predicate.And(i => i.FSMID == objInvoiceSearchModel.FSM);
                bSearch = true;
            }
            if (!string.IsNullOrWhiteSpace(objInvoiceSearchModel.Invoiceid))
            {
                predicate = predicate.And(i => i.Invoiceid == (objInvoiceSearchModel.Invoiceid));
                bSearch = true;
            }
            if (objInvoiceSearchModel.WorkorderID != null)
            {
                predicate = predicate.And(i => i.WorkorderID == objInvoiceSearchModel.WorkorderID);
                bSearch = true;
            }

            if (objInvoiceSearchModel.BeforeDate != null)
            {
                predicate = predicate.And(i => i.InvoiceSubmitDate.Value <= objInvoiceSearchModel.BeforeDate.Value);
                bSearch = true;
            }
            if (objInvoiceSearchModel.OnOrAfterDate != null)
            {
                predicate = predicate.And(i => i.InvoiceSubmitDate.Value >= objInvoiceSearchModel.OnOrAfterDate.Value);
                bSearch = true;
            }

            if (objInvoiceSearchModel.CustomerID != null)
            {
                predicate = predicate.And(i => i.CustomerID == objInvoiceSearchModel.CustomerID);
                bSearch = true;
            }
            if (objInvoiceSearchModel.CustomerName != null)
            {
                predicate = predicate.And(i => i.CustomerName.Contains(objInvoiceSearchModel.CustomerName));
                bSearch = true;
            }
            if (objInvoiceSearchModel.BranchID != null)
            {
                predicate = predicate.And(i => i.BranchID == objInvoiceSearchModel.BranchID);
                bSearch = true;
            }
            if (objInvoiceSearchModel.BranchName != null)
            {
                predicate = predicate.And(i => i.BranchName.Contains(objInvoiceSearchModel.BranchName));
                bSearch = true;
            }
            if (objInvoiceSearchModel.TPSPVendorID != null)
            {
                bSearch = true;
            }
            if (objInvoiceSearchModel.TPSPVendorName != null)
            {
                bSearch = true;
            }
            string strQuery = string.Empty;
            List<int?> TechIDS = new List<int?>();

            strQuery = objInvoiceSearchModel.TPSPVendorID != null ? "Select Tech_ID from feast_tech_hierarchy where TeamLead_ID  = @TpspVendorId" : "";
            strQuery += objInvoiceSearchModel.TPSPVendorID != null && objInvoiceSearchModel.TPSPVendorName != null ? " And TEAMLEAD_NAME like  @TpspVendorName " : objInvoiceSearchModel.TPSPVendorName != null ? " Select Tech_ID from feast_tech_hierarchy where TEAMLEAD_NAME like  @TpspVendorName " : "";
            if (strQuery != "")
            {
                //DataTable daTble = objMarsView.fn_FSM_View(strQuery);
                SqlParameter[] sqlParam;
                if ((objInvoiceSearchModel.TPSPVendorID != null) && objInvoiceSearchModel.TPSPVendorName != null)
                {
                    sqlParam = new SqlParameter[2];
                    sqlParam[0] = new SqlParameter();
                    sqlParam[0].ParameterName = "@TpspVendorId";
                    sqlParam[0].Value = objInvoiceSearchModel.TPSPVendorID;

                    sqlParam[1] = new SqlParameter();
                    sqlParam[1].ParameterName = "@TpspVendorName";
                    sqlParam[1].Value = "%" + objInvoiceSearchModel.TPSPVendorName + "%";
                }
                else if ((objInvoiceSearchModel.TPSPVendorID == null) && objInvoiceSearchModel.TPSPVendorName != null)
                {
                    sqlParam = new SqlParameter[1];
                    sqlParam[0] = new SqlParameter();
                    sqlParam[0].ParameterName = "@TpspVendorName";
                    sqlParam[0].Value = "%" + objInvoiceSearchModel.TPSPVendorName + "%";
                }
                else
                {
                    sqlParam = new SqlParameter[1];
                    sqlParam[0] = new SqlParameter();
                    sqlParam[0].ParameterName = "@TpspVendorId";
                    sqlParam[0].Value = objInvoiceSearchModel.TPSPVendorID;
                }
                var tpspVendorId = new SqlParameter("@TpspVendorId", objInvoiceSearchModel.TPSPVendorID);
                var tpspVendorName = new SqlParameter("@TpspVendorName", objInvoiceSearchModel.TPSPVendorName);
                //            var List = FormalBrothersEntitites.Database.SqlQuery<double>(strQuery, sqlParam) .ToList();
                foreach (double str in FarmerBrothersEntitites.Database.SqlQuery<double>(strQuery, sqlParam).ToList())
                {
                    TechIDS.Add(Convert.ToInt32(str));
                }
            }
            if (bSearch == true)
            {
                //objInvoiceSearchModel.SearchResults = query.AsExpandable().Where(predicate).ToList();
                objInvoiceSearchModel.SearchResults = (objInvoiceSearchModel.SearchResults.Count > 0 && (TechIDS.Count > 0 || objInvoiceSearchModel.TPSPVendorName != null || objInvoiceSearchModel.TPSPVendorID != null)) ? (from p in objInvoiceSearchModel.SearchResults where TechIDS.Exists(x => x == p.TechID) select p).ToList() : (objInvoiceSearchModel.SearchResults.Count == 0 && TechIDS.Count > 0) ? (from p in query where TechIDS.Exists(x => x == p.TechID) select p).ToList() : objInvoiceSearchModel.SearchResults;

                foreach (var x in objInvoiceSearchModel.SearchResults)
                {
                    var itemToChange = InvoiceFeastList.FirstOrDefault(d => d.Tech_Id == x.TechID);
                    if (itemToChange != null)
                    {
                        x.TPSPVendorID = itemToChange.TeamLead_Id;
                        x.TPSPVendorName = itemToChange.TeamLead_Name;
                        x.BranchState = itemToChange.Tech_State;
                        x.FSMID = itemToChange.FSM_Id;
                        x.BranchID = itemToChange.ServiceCenter_Id;
                        x.BranchName = itemToChange.ServiceCenter_Name;
                        x.FSM = itemToChange.FSM_Name;
                    }
                }

                return objInvoiceSearchModel.SearchResults;
            }
            else
            {
                return objInvoiceSearchModel.SearchResults = null;
            }



        }

        [HttpPost]
        public void ExcelExportInvoiceSearchResults()
        {
            try
            {
                InvoiceSearchModel objInvoiceSearchModel = new InvoiceSearchModel();
                if (TempData["objInvoiceSearchModel"] != null)
                {
                    objInvoiceSearchModel = TempData["objInvoiceSearchModel"] as InvoiceSearchModel;
                    TempData.Keep("objInvoiceSearchModel");
                }

                objInvoiceSearchModel.SearchResults = GetInvoiceSearchData(objInvoiceSearchModel);

                if (objInvoiceSearchModel.SearchResults != null)
                {
                    objInvoiceSearchModel.SearchResults.Select(c =>
                    {
                        c.InvoiceSubmitDateString = c.InvoiceSubmitDate == null ? string.Empty : Convert.ToDateTime(c.InvoiceSubmitDate).ToString("MM/dd/yyyy");
                        c.DateSubmissionForPaymentString = c.DateSubmissionForPayment == null ? string.Empty : Convert.ToDateTime(c.DateSubmissionForPayment).ToString("MM/dd/yyyy");
                        c.WorkorderCompletionDateString = c.WorkorderCompletionDate == null ? string.Empty : Convert.ToDateTime(c.WorkorderCompletionDate).ToString("MM/dd/yyyy");
                        c.PaymentDateString = c.PaymentDate == null ? string.Empty : Convert.ToDateTime(c.PaymentDate).ToString("MM/dd/yyyy");
                        return c;
                    }).ToList();
                }

                string gridModel = HttpContext.Request.Params["GridModel"];
                GridProperties gridProperty = ConvertGridObject(gridModel);
                ExcelExport exp = new ExcelExport();
                exp.Export(gridProperty, objInvoiceSearchModel.SearchResults, "Invoice.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
            }
            catch (Exception ex)
            {
                // log the exception       
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);

            }
        }

        public bool fnInvoiceUpdateService(Invoice objInvoiceTbl)
        {
            try
            {
                var objWorkOrder = this.FarmerBrothersEntitites.WorkOrders.Where(i => i.WorkorderID == objInvoiceTbl.WorkorderID).FirstOrDefault();
                int partsQty = FarmerBrothersEntitites.WorkorderParts.Where(i => i.WorkorderID == objInvoiceTbl.WorkorderID).Select(x => x.Quantity).ToList().Sum(x => Convert.ToInt32(x.Value));

                CreateInvoiceClient UpdateInvoiceClient = new CreateInvoiceClient();
                UpdateInvoiceClient.ClientCredentials.UserName.UserName = username;
                UpdateInvoiceClient.ClientCredentials.UserName.Password = password;
                using (new OperationContextScope(UpdateInvoiceClient.InnerChannel))
                {
                    InvoiceUpdateResponse response = new InvoiceUpdateResponse();
                    InvoiceUpdate InputRequestFields = new InvoiceUpdate();
                    if (UpdateInvoiceClient != null)
                    {
                        InputRequestFields.WorkorderId = objWorkOrder.WorkorderID.ToString();
                        InputRequestFields.MarsWorkOrderId = objWorkOrder.WorkorderID.ToString();
                        InputRequestFields.PayInvoice = string.Empty;
                        InputRequestFields.BUNNCustomerNumber = objWorkOrder.CustomerID.ToString();
                        InputRequestFields.BUNNInvoiceNumber = objInvoiceTbl.Invoiceid;
                        // InputRequestFields.BUNNPoNumber = objInvoiceTbl.Invoiceid;
                        InputRequestFields.TotalInvoiceAmount = objInvoiceTbl.InvoiceTotal.ToString();
                        InputRequestFields.TotalAuthorizedAmount = objInvoiceTbl.PaymentAmtApproved.ToString(); ;
                        InputRequestFields.TotalTaxAmount = objInvoiceTbl.TaxAmount.ToString();
                        InputRequestFields.LaborTotal = objInvoiceTbl.LaborTotal.ToString();
                        InputRequestFields.LaborQty = 1.ToString();
                        InputRequestFields.LaborPriceEach = 1.ToString();
                        InputRequestFields.PartsTotal = objInvoiceTbl.PartsTotal.ToString();
                        InputRequestFields.PartsQty = partsQty.ToString();
                        InputRequestFields.PartsPriceEach = string.Empty;
                        InputRequestFields.TravelTotal = objInvoiceTbl.TravelTotal.ToString();
                        InputRequestFields.TravelQty = 1.ToString();
                        InputRequestFields.TravelPriceEach = 1.ToString();
                        InputRequestFields.BUNNWorkorderId = objWorkOrder.WorkorderID.ToString();
                        InputRequestFields.ServiceType = objWorkOrder.WorkorderCalltypeid.ToString();
                        InputRequestFields.WorkorderCreationDate = objWorkOrder.WorkorderEntryDate.ToString();
                        InputRequestFields.SiteCustomerId = string.Empty;
                        InputRequestFields.ServiceAgentId = objWorkOrder.WorkorderSchedules.FirstOrDefault().ServiceCenterID.ToString();
                        InputRequestFields.ServiceAgentName = objWorkOrder.WorkorderSchedules.FirstOrDefault().ServiceCenterName;
                        InputRequestFields.DateServiceWasCompletedFromAgents = string.Empty;
                        InputRequestFields.ProblemDescription = string.Empty;
                        InputRequestFields.InvoiceId = objInvoiceTbl.InvoiceUniqueid.ToString();
                        InputRequestFields.EmergencyService = string.Empty;
                        InputRequestFields.BUNNServeFeeTotal = string.Empty;
                    }
                    HttpRequestMessageProperty requestMessage = new HttpRequestMessageProperty();
                    requestMessage.Headers["username"] = username;
                    requestMessage.Headers["password"] = password;
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;
                    response = UpdateInvoiceClient.updateInvoice(InputRequestFields);
                    if (response.ErrorMsg == null)
                        return true;
                    return false;
                }
            }
            catch (WebException ex)
            {
                return false;
                //throw ex.Message;
            }
        }

        [HttpPost]
        public ActionResult UpdateInvoiceStatus(int intInvoiceUniqueId, string strInvoiceStatus, string strInvoiceComments = "", string strInvoiceAuthorizedAmnt = "")
        {
            try
            {
                InvoiceSearchModel objInvoiceSearchModel = new InvoiceSearchModel();
                Invoice objInvoiceTbl = this.FarmerBrothersEntitites.Invoices.Where(i => i.InvoiceUniqueid == intInvoiceUniqueId).FirstOrDefault();
                objInvoiceTbl.InvoiceStatus = strInvoiceStatus;
                objInvoiceTbl.InvoiceComments = strInvoiceComments;
                string LDAPUserName = "";
                if (Session != null)
                {
                    if (Session["Username"] != null)
                    {
                        LDAPUserName = Session["Username"].ToString();
                    }
                }
                if (LDAPUserName != "")
                {
                    //objInvoiceTbl.ApprovedBy = LDAPUserName;
                }
                if (strInvoiceStatus == "Contested")
                {
                    if (strInvoiceAuthorizedAmnt != "")
                    {
                        //objInvoiceTbl.AdjustmentAmount = Convert.ToDecimal(strInvoiceAuthorizedAmnt);
                    }
                }
                else
                {
                    if (strInvoiceAuthorizedAmnt != "")
                    {
                        objInvoiceTbl.PaymentAmtApproved = Convert.ToDecimal(strInvoiceAuthorizedAmnt);
                    }
                }

                FarmerBrothersEntitites.Invoices.Attach(objInvoiceTbl);

                if (strInvoiceStatus == AdjustedpaymentStatus || strInvoiceStatus == SubmittedpaymentStatus)
                {
                    if (fnInvoiceUpdateService(objInvoiceTbl))
                    {                        
                        FarmerBrothersEntitites.Entry(objInvoiceTbl).State = System.Data.Entity.EntityState.Modified;
                        FarmerBrothersEntitites.SaveChanges();
                        ViewBag.Message = "Success";
                        return Json(ViewBag.Message);
                    }
                    else
                    {
                        ViewBag.Message = "Failed";
                        return Json(ViewBag.Message);
                    }
                }
                else
                {
                    FarmerBrothersEntitites.Entry(objInvoiceTbl).State = System.Data.Entity.EntityState.Modified;
                    FarmerBrothersEntitites.SaveChanges();
                    ViewBag.Message = "Success";
                    return Json(ViewBag.Message);
                }
            }
            catch (Exception ex)
            {
                //log the exception.
                return View("Error");

            }

            // return RedirectToAction("DisplayInvoice", "Invoice");
        }

        [HttpPost]
        public ActionResult SaveInvoice(int intInvoiceUniqueId, string InvoiceComments, string AuthorizedAmnt)
        {
            InvoiceSearchModel objInvoiceSearchModel = new InvoiceSearchModel();
            try
            {
                Invoice objInvoiceTbl = this.FarmerBrothersEntitites.Invoices.Where(i => i.InvoiceUniqueid == intInvoiceUniqueId).FirstOrDefault();
                objInvoiceTbl.InvoiceComments = InvoiceComments;
                objInvoiceTbl.PaymentAmtApproved = Convert.ToDecimal(AuthorizedAmnt.Replace("$", ""));
                FarmerBrothersEntitites.Invoices.Attach(objInvoiceTbl);
                FarmerBrothersEntitites.Entry(objInvoiceTbl).State = System.Data.Entity.EntityState.Modified;
                FarmerBrothersEntitites.SaveChanges();
                ViewBag.Message = "Success";
            }
            catch (Exception ex)
            {
                // log the exception 
                return View("Error");
                //  return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
            }
            return Json(ViewBag.Message);
        }

        public CustomerModel customerDetails(string customerID)
        {
            CustomerModel customerModel = new CustomerModel();
            FeastLocationService.Customer custDetails = new FeastLocationService.Customer();

            if (feastLocationsClient != null)
            {
                CustomerRequest customerRequest = new CustomerRequest()
                {
                    CustId = customerID.ToString()
                };

                CustomerResponse response = feastLocationsClient.getCustomer(customerRequest);

                if (response.Customer != null)
                {
                    if (response.Customer.Length > 0)
                    {
                        customerModel = new CustomerModel(response.Customer[0], FarmerBrothersEntitites);
                        //custDetails.CustomerId = customerModel.CustomerId;
                        //custDetails.CustomerName = customerModel.CustomerName;
                        //custDetails.Address = customerModel.Address;
                        //custDetails.City = customerModel.City;
                        //custDetails.State = customerModel.State;
                        //custDetails.ZipCode = customerModel.ZipCode;
                        //custDetails.PhoneNumber = customerModel.PhoneNumber;
                        //custDetails.PhoneExtn = customerModel.PhoneExtn;
                        //custDetails.MainContactName = customerModel.MainContactName;
                        //custDetails.CustomerPreference = customerModel.CustomerPreference;
                    }
                }
            }
            return customerModel;
        }


        public string SecstoHours(decimal seconds)
        {
            if (seconds == 0)
            {
                return null;
            }
            TimeSpan time = TimeSpan.FromSeconds(Convert.ToDouble(seconds));
            return time.ToString(@"hh\:mm");
        }
        */
    }
}