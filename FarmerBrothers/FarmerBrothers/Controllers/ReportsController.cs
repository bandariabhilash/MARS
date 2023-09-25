using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using LinqKit;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript;
using Syncfusion.JavaScript.Models;
using Syncfusion.JavaScript.Shared.Serializer;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace FarmerBrothers.Controllers
{
    public class ReportsController : BaseController
    {
        int defaultFollowUpCall;
        public static List<NonServiceSearchResults> serialNumbersearchResults;
        public static List<SuperInvoiceSearchResultModel> SupInvTechsearchResults;
        public static List<OpenCallByTechSearchResultModel> OpenCallByTechnicianResuls;
        public ReportsController()
        {
            AllFBStatu FarmarBortherStatus = FarmerBrothersEntitites.AllFBStatus.Where(a => a.FBStatus == "None" && a.StatusFor == "Follow Up Call").FirstOrDefault();
            if (FarmarBortherStatus != null)
            {
                defaultFollowUpCall = FarmarBortherStatus.FBStatusID;
            }
        }

        #region FBReconcillationBillingReport
        public ActionResult FBReconcillationBillingReport(int? isBack)
        {
            FBReconcillationBillingModel FBReconcillationModel = new FBReconcillationBillingModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                FBReconcillationModel = TempData["SearchCriteria"] as FBReconcillationBillingModel;
                TempData["SearchCriteria"] = FBReconcillationModel;
            }
            else
            {
                FBReconcillationModel = new FBReconcillationBillingModel();
                TempData["SearchCriteria"] = null;
            }
            FBReconcillationModel.SearchResults = new List<FBReconcillationBillingSearchResultModel>();
            return View(FBReconcillationModel);
        }

        public JsonResult SearchFBReconcillation(FBReconcillationBillingModel FBReconcillation)
        {
            if (string.IsNullOrWhiteSpace(FBReconcillation.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(FBReconcillation.DateTo.ToString())
                )
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                FBReconcillation.SearchResults = GetFBReconcillation(FBReconcillation);
                TempData["SearchCriteria"] = FBReconcillation;
                return Json(FBReconcillation.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        public List<FBReconcillationBillingSearchResultModel> GetFBReconcillation(FBReconcillationBillingModel searchcriteria)
        {
            List<FBReconcillationBillingSearchResultModel> FBRecrest = new List<FBReconcillationBillingSearchResultModel>();

            String DF = searchcriteria.DateFrom.ToString();
            String DT = searchcriteria.DateTo.ToString();
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fbDeltaVendors("USP_FBServiceEvent_count", DF, DT);
            FBReconcillationBillingSearchResultModel FBReconcillationBilling;
            foreach (DataRow dr in dt.Rows)
            {
                FBReconcillationBilling = new FBReconcillationBillingSearchResultModel();
                FBReconcillationBilling.AgentName = dr["AgentName"].ToString();
                FBReconcillationBilling.ServiceEventCnt = dr["ServiceEventCnt"].ToString();
                FBReconcillationBilling.NonServiceEventCnt = dr["NonServiceEventCnt"].ToString();
                FBReconcillationBilling.SalesEventCnt = dr["SalesEventCnt"].ToString();
                FBReconcillationBilling.RefurbEventCnt = dr["RefurbEventCnt"].ToString();
                FBReconcillationBilling.RowTot = dr["RowTot"].ToString();
                FBRecrest.Add(FBReconcillationBilling);
            }
            searchcriteria.SearchResults = FBRecrest;

            return FBRecrest;
        }
        public void FBRecExcelExport()
        {
            FBReconcillationBillingModel FBRecrest = new FBReconcillationBillingModel();

            IList<FBReconcillationBillingSearchResultModel> searchResults = new List<FBReconcillationBillingSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                FBRecrest = TempData["SearchCriteria"] as FBReconcillationBillingModel;
                searchResults = GetFBReconcillation(FBRecrest);
            }

            TempData["SearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "FBRecBillJDEReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        public JsonResult ClearReconcillationBillingResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new FBReconcillationBillingModel(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region FBMonthlyReport
        public ActionResult FBMonthlyReport(int? isBack)
        {
            FBMonthlyWorkorderModel FBMonthlyModel = new FBMonthlyWorkorderModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                FBMonthlyModel = TempData["SearchCriteria"] as FBMonthlyWorkorderModel;
                TempData["SearchCriteria"] = FBMonthlyModel;
            }
            else
            {
                FBMonthlyModel = new FBMonthlyWorkorderModel();
                TempData["SearchCriteria"] = null;
            }
            FBMonthlyModel.SearchResults = new List<FBMonthlyWorkorderSearchResultModel>();
            return View(FBMonthlyModel);
        }

        public JsonResult SearchFBMonthlyWorkorder(FBMonthlyWorkorderModel FBMonthly)
        {
            if (string.IsNullOrWhiteSpace(FBMonthly.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(FBMonthly.DateTo.ToString())
                )
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                FBMonthly.SearchResults = GetMonthlyWorkorder(FBMonthly);
                TempData["SearchCriteria"] = FBMonthly;
                return Json(FBMonthly.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        public List<FBMonthlyWorkorderSearchResultModel> GetMonthlyWorkorder(FBMonthlyWorkorderModel searchMonthly)
        {
            List<FBMonthlyWorkorderSearchResultModel> FBMonthly = new List<FBMonthlyWorkorderSearchResultModel>();

            String DF = searchMonthly.DateFrom.ToString();
            String DT = searchMonthly.DateTo.ToString();
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fbDeltaVendors("USP_FBC_Monthly", DF, DT);
            FBMonthlyWorkorderSearchResultModel FBMonthlyWorkorder;
            foreach (DataRow dr in dt.Rows)
            {
                FBMonthlyWorkorder = new FBMonthlyWorkorderSearchResultModel();
                FBMonthlyWorkorder.WorkOrderID = dr["WorkOrderID"].ToString();
                FBMonthlyWorkorder.ContactID = dr["ContactID"].ToString();
                FBMonthlyWorkorder.CloseDate = dr["CloseDate"].ToString();
                FBMonthlyWorkorder.UserName = dr["UserName"].ToString();
                FBMonthlyWorkorder.MarsUserCompany = dr["MarsUserCompany"].ToString();
                FBMonthlyWorkorder.CustCompany = dr["CustCompany"].ToString();
                FBMonthlyWorkorder.FieldServiceManager = dr["FieldServiceManager"].ToString();
                FBMonthlyWorkorder.DealerCompanyName = dr["DealerCompanyName"].ToString();
                FBMonthlyWorkorder.FamilyAff = dr["FamilyAff"].ToString();
                FBMonthlyWorkorder.ClosureConfirmationNo = dr["ClosureConfirmationNo"].ToString();
                FBMonthly.Add(FBMonthlyWorkorder);
            }
            searchMonthly.SearchResults = FBMonthly;

            return FBMonthly;
        }
        public void FBMonthlyExcelExport()
        {
            FBMonthlyWorkorderModel FBMonthly = new FBMonthlyWorkorderModel();

            IList<FBMonthlyWorkorderSearchResultModel> searchResults = new List<FBMonthlyWorkorderSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                FBMonthly = TempData["SearchCriteria"] as FBMonthlyWorkorderModel;
                searchResults = GetMonthlyWorkorder(FBMonthly);
            }

            TempData["SearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "FBMonthlyReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        public JsonResult ClearMonthlyWorkorderResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new FBMonthlyWorkorderModel(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region BillingReport
        public ActionResult BillingReport(int? isBack)
        {
            BillingReportModel BillingRptModel = new BillingReportModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                BillingRptModel = TempData["SearchCriteria"] as BillingReportModel;
                TempData["SearchCriteria"] = BillingRptModel;
            }
            else
            {
                BillingRptModel = new BillingReportModel();
                TempData["SearchCriteria"] = null;
            }

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            BillingRptModel.Technicianlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                if (dr[0].ToString() != "" && dr[0].ToString() != null)
                {
                    tech.TechID = dr[0].ToString();
                    if (dr[0].ToString() == "SPD")
                    {
                        tech.TechName = "Internal";
                        TechnicianAffs.Add(tech);
                    }
                    if (dr[0].ToString() == "SPT")
                    {
                        tech.TechName = "3rd Party";
                        TechnicianAffs.Add(tech);
                    }

                }

            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            BillingRptModel.FamilyAffs = TechnicianAffs;

            BillingRptModel.SearchResults = new List<BillingReportSearchResultModel>();

            return View(BillingRptModel);
        }

        public JsonResult SearchBillingReport(BillingReportModel BillingRptModel)
        {
            if (string.IsNullOrEmpty(BillingRptModel.ParentACC))
            {
                BillingRptModel.ParentACC = "0";
            }

            if ((!BillingRptModel.BillingFromDate.HasValue)
                && !BillingRptModel.BillingToDate.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                BillingRptModel.SearchResults = GetBillingreport(BillingRptModel);
                //int totalCount = BillingRptModel.SearchResults.Sum(item => Convert.ToInt32(item.ToatlEventsByTech));
                //BillingReportSearchResultModel BillingSearchResultModel = new BillingReportSearchResultModel();
                //BillingSearchResultModel.BranchName = "Total Calls";
                //BillingSearchResultModel.ToatlEventsByTech = totalCount.ToString();
                //BillingRptModel.SearchResults.Add(BillingSearchResultModel);


                TempData["SearchCriteria"] = BillingRptModel;
                //return Json(BillingRptModel.SearchResults, JsonRequestBehavior.AllowGet);
                JsonResult result = Json(new
                {
                    rows = BillingRptModel.SearchResults
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;

                return result;
            }
        }

        private IList<BillingReportSearchResultModel> GetBillingreport(BillingReportModel BillingRptModel)
        {
            List<BillingReportSearchResultModel> BillingData = new List<BillingReportSearchResultModel>();
            String DF = BillingRptModel.BillingFromDate.ToString();
            String DT = Convert.ToDateTime(BillingRptModel.BillingToDate).AddDays(1).ToString();
            String TL = BillingRptModel.DealerId.ToString();
            String FA = BillingRptModel.TechID.ToString();
            String PPID = BillingRptModel.ParentACC == null ? "0" : BillingRptModel.ParentACC.ToString();
            String AccountNo = BillingRptModel.AccountNo == null ? "0" : BillingRptModel.AccountNo.ToString();
            MarsViews mars = new MarsViews();
            decimal LaborCost = Convert.ToDecimal(ConfigurationManager.AppSettings["LaborCost"]);
            string ssql = @"select W.WorkorderID,W.WorkorderEntryDate,W.CustomerID,
                                    W.CustomerName As CompanyName,C.Address1,C.Address2,C.City,C.PostalCode,C.Route,C.Branch,WS.Techid,WS.TechName,
                                    W.WorkorderCallstatus, WD.StartDateTime,WD.ArrivalDateTime,WD.CompletionDateTime,'' as PurchaseOrder, 'N' as BillingID,
                                    WS.ScheduleDate,W.WorkorderEquipCount,W.CustomerState,W.ThirdPartyPO,W.Estimate,W.FinalEstimate,W.EstimateApprovedBy,
                                    W.OriginalWorkorderid,W.WorkorderCalltypeid as WorkorderCalltypeid,'' as TechCalled,W.AppointmentDate,WS.Techid as DispatchTechID,WS.TechName as DispatchTechName,
                                    W.NoServiceRequired,WD.NSRReason,C.PricingParentID,EQP.Category as Category,EQP.SerialNumber as SerialNumber,EQP.Model as Model,EQP.Manufacturer Manufacturer,EQP.Solutionid as Solutionid
                                    ,TBS.Notes As WorkPerformedNotes,W.CustomerName,WP.Quantity,WP.Sku,sku.SKUCost,sku.VendorCode, WP.Description,WP.Manufacturer as OrderSource,sku.Manufacturer as Supplier,
                                    " + LaborCost + @" as TravelTotal, "+ LaborCost + @" as LaborTotal, WP.Total as PartsTotal, W.CustomerPO, WD.HardnessRating
                                    from workorder W 
                                    inner join Contact C on C.ContactID = W.CustomerID
                                    inner join WorkorderDetails WD on WD.WorkorderID = W.WorkorderID
                                    inner join WorkorderSchedule WS on WS.WorkorderID = W.WorkorderID
                                    inner join WorkorderEquipment EQP on W.WorkorderID = EQP.WorkorderID
                                    left outer join WorkorderParts WP on W.Workorderid = WP.WorkorderID and WP.AssetID = EQP.Assetid
                                    left outer join sku on sku.Sku = WP.Sku  and sku.SkuActive = 1 
                                    left outer join TMP_BlackBerry_SCFAssetInfo TBS on W.WorkorderID = TBS.WorkorderID and EQP.Assetid = TBS.AssetKey and W.WorkorderClosureConfirmationNo = TBS.ClosureConfirmationNo
                                    where (WS.AssignedStatus='Accepted' OR WS.AssignedStatus='Scheduled') AND 
                                    W.WorkorderCallstatus='Closed'";

            ssql = ssql + " and W.WorkorderCloseDate >='" + DF + "'";

            ssql = ssql + "  and  W.WorkorderCloseDate <'" + DT + "'";

            if(!string.IsNullOrEmpty(AccountNo) && AccountNo != "0")
            {
                ssql = ssql + " and C.ContactID =" + AccountNo;
            }

            if (BillingRptModel.DealerId > 0)
            {
                ssql = ssql + " and WS.Techid =" + TL;
            }

            if (FA == "SPD")
            {
                ssql = ssql + " and FamilyAff != 'SPT'";
            }
            if (FA == "SPT")
            {
                ssql = ssql + " and FamilyAff = 'SPT'";
            }
            
            if (!string.IsNullOrEmpty(PPID)  && PPID != "0")
            {
                ssql = ssql + " and W.CustomerID IN (Select ContactID from Contact where PricingParentID = " + PPID + ")"; //'cast(@" + PPID + "as varchar(10)) ')";
            }

            ssql = ssql + " order by W.WorkorderID";

            DataTable dt = mars.fnTpspVendors(ssql);


            //DataTable dt = mars.fbSuperInvoice("USP_SuperInvoice_Report", DF, DT, PPID, FA, TL);
            BillingReportSearchResultModel FBBillingSearchResult;
            foreach (DataRow dr in dt.Rows)
            {
                FBBillingSearchResult = new BillingReportSearchResultModel();
                string WorkorderID = dr["WorkorderID"] == DBNull.Value ? "" : dr["WorkorderID"].ToString();
                FBBillingSearchResult.WorkorderID = WorkorderID;
                FBBillingSearchResult.WorkorderEntryDate = dr["WorkorderEntryDate"] == DBNull.Value ? "" : dr["WorkorderEntryDate"].ToString();
                string customertId = dr["CustomerID"] == DBNull.Value ? "" : dr["CustomerID"].ToString();
                FBBillingSearchResult.CustomerID = customertId;
                FBBillingSearchResult.CompanyName = dr["CompanyName"] == DBNull.Value ? "" : dr["CompanyName"].ToString();
                FBBillingSearchResult.Address1 = dr["Address1"] == DBNull.Value ? "" : dr["Address1"].ToString();
                FBBillingSearchResult.Address2 = dr["Address2"] == DBNull.Value ? "" : dr["Address2"].ToString();
                FBBillingSearchResult.City = dr["City"] == DBNull.Value ? "" : dr["City"].ToString();
                string customerState = dr["CustomerState"] == DBNull.Value ? "" : dr["CustomerState"].ToString();
                FBBillingSearchResult.CustomerState = customerState;
                string postalCode = dr["PostalCode"] == DBNull.Value ? "" : dr["PostalCode"].ToString();
                FBBillingSearchResult.PostalCode = postalCode;
                FBBillingSearchResult.Route = dr["Route"] == DBNull.Value ? "" : dr["Route"].ToString();
                FBBillingSearchResult.Branch = dr["Branch"] == DBNull.Value ? "" : dr["Branch"].ToString();
                int techId = dr["Techid"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Techid"]);
                FBBillingSearchResult.Techid = techId == 0 ? "" : techId.ToString();
                FBBillingSearchResult.TechName = dr["TechName"] == DBNull.Value ? "" : dr["TechName"].ToString();
                FBBillingSearchResult.WorkorderCallstatus = dr["WorkorderCallstatus"] == DBNull.Value ? "" : dr["WorkorderCallstatus"].ToString();

                string arrivalDateTime, startDateTime, completionDateTime = "";
                startDateTime = dr["StartDateTime"] == DBNull.Value ? "" : dr["StartDateTime"].ToString();
                arrivalDateTime = dr["ArrivalDateTime"] == DBNull.Value ? "" : dr["ArrivalDateTime"].ToString();
                completionDateTime = dr["CompletionDateTime"] == DBNull.Value ? "" : dr["CompletionDateTime"].ToString();

                FBBillingSearchResult.StartDateTime = startDateTime;
                FBBillingSearchResult.ArrivalDateTime = arrivalDateTime;
                FBBillingSearchResult.CompletionDateTime = completionDateTime;

                FBBillingSearchResult.PurchaseOrder = dr["PurchaseOrder"] == DBNull.Value ? "" : dr["PurchaseOrder"].ToString();
                FBBillingSearchResult.BillingID = dr["BillingID"] == DBNull.Value ? "" : dr["BillingID"].ToString();
                FBBillingSearchResult.ScheduleDate = dr["ScheduleDate"] == DBNull.Value ? "" : dr["ScheduleDate"].ToString();
                FBBillingSearchResult.AppointmentDate = dr["AppointmentDate"] == DBNull.Value ? "" : dr["AppointmentDate"].ToString();
                FBBillingSearchResult.WorkorderEquipCount = dr["WorkorderEquipCount"] == DBNull.Value ? "" : dr["WorkorderEquipCount"].ToString();                
                FBBillingSearchResult.ThirdPartyPO = dr["ThirdPartyPO"] == DBNull.Value ? "" : dr["ThirdPartyPO"].ToString();
                FBBillingSearchResult.Estimate = dr["Estimate"] == DBNull.Value ? "" : dr["Estimate"].ToString();
                FBBillingSearchResult.FinalEstimate = dr["FinalEstimate"] == DBNull.Value ? "" : dr["FinalEstimate"].ToString();
                FBBillingSearchResult.EstimateApprovedBy = dr["EstimateApprovedBy"] == DBNull.Value ? "" : dr["EstimateApprovedBy"].ToString();
                FBBillingSearchResult.OriginalWorkorderid = dr["OriginalWorkorderid"] == DBNull.Value ? "" : dr["OriginalWorkorderid"].ToString();
                FBBillingSearchResult.WorkorderCalltypeid = dr["WorkorderCalltypeid"] == DBNull.Value ? "" : dr["WorkorderCalltypeid"].ToString();
                FBBillingSearchResult.TechCalled = dr["TechCalled"] == DBNull.Value ? "" : dr["TechCalled"].ToString();                
                FBBillingSearchResult.DispatchTechID = dr["DispatchTechID"] == DBNull.Value ? "" : dr["DispatchTechID"].ToString();
                FBBillingSearchResult.DispatchTechName = dr["DispatchTechName"] == DBNull.Value ? "" : dr["DispatchTechName"].ToString();
                FBBillingSearchResult.NoServiceRequired = dr["NoServiceRequired"] == DBNull.Value ? "" : dr["NoServiceRequired"].ToString();
                FBBillingSearchResult.NSRReason = dr["NSRReason"] == DBNull.Value ? "" : dr["NSRReason"].ToString();
                string parentId = dr["PricingParentID"] == DBNull.Value ? "" : dr["PricingParentID"].ToString();
                FBBillingSearchResult.PricingParentID = parentId;
                FBBillingSearchResult.Category = dr["Category"] == DBNull.Value ? "" : dr["Category"].ToString();
                FBBillingSearchResult.SerialNumber = dr["SerialNumber"] == DBNull.Value ? "" : dr["SerialNumber"].ToString();
                FBBillingSearchResult.Model = dr["Model"] == DBNull.Value ? "" : dr["Model"].ToString();
                FBBillingSearchResult.Manufacturer = dr["Manufacturer"] == DBNull.Value ? "" : dr["Manufacturer"].ToString();
                FBBillingSearchResult.Solutionid = dr["Solutionid"] == DBNull.Value ? "" : dr["Solutionid"].ToString();
                FBBillingSearchResult.WorkPerformedNotes = dr["WorkPerformedNotes"] == DBNull.Value ? "" : dr["WorkPerformedNotes"].ToString();

                //string WorkPerformedNotes = dr["WorkPerformedNotes"] == DBNull.Value ? "" : dr["WorkPerformedNotes"].ToString();
                //NotesHistory nts = FarmerBrothersEntitites.NotesHistories.Where(w => w.WorkorderID.ToString() == WorkorderID && (w.Notes.Contains("comments") || w.Notes.Contains("Comments"))).FirstOrDefault();
                //FBBillingSearchResult.WorkPerformedNotes = nts == null ? "" : nts.Notes.Contains(':') ? nts.Notes.Split(':')[1] : nts.Notes;

                FBBillingSearchResult.WorkPerformedNotes = dr["WorkPerformedNotes"] == DBNull.Value ? "" : dr["WorkPerformedNotes"].ToString();

                FBBillingSearchResult.CustomerName = dr["CustomerName"] == DBNull.Value ? "" : dr["CustomerName"].ToString();

                int Quantity = dr["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Quantity"]);
                FBBillingSearchResult.Quantity = Quantity.ToString();

                FBBillingSearchResult.Sku = dr["Sku"] == DBNull.Value ? "" : dr["Sku"].ToString();

                decimal SKUCost = dr["SKUCost"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["SKUCost"]);
                FBBillingSearchResult.SKUCost = SKUCost;

                FBBillingSearchResult.VendorCode = dr["VendorCode"] == DBNull.Value ? "" : dr["VendorCode"].ToString();
                FBBillingSearchResult.Description = dr["Description"] == DBNull.Value ? "" : dr["Description"].ToString();
                FBBillingSearchResult.OrderSource = dr["OrderSource"] == DBNull.Value ? "" : dr["OrderSource"].ToString();
                FBBillingSearchResult.Supplier = dr["Supplier"] == DBNull.Value ? "" : dr["Supplier"].ToString();

                PricingDetail priceDtls = Utility.GetPricingDetails(Convert.ToInt32(customertId), techId, customerState, FarmerBrothersEntitites);
                decimal? travelRateDefined = priceDtls.HourlyTravlRate;
                decimal? laborRateDefined = priceDtls.HourlyLablrRate;

                double TravelTotal, LaborTotal, PartsTotal, TotalInvoice = 0;
                if(!string.IsNullOrEmpty(startDateTime) && !string.IsNullOrEmpty(arrivalDateTime))
                {
                    DateTime arrival = Convert.ToDateTime(arrivalDateTime);
                    DateTime strt = Convert.ToDateTime(startDateTime);
                    TimeSpan timeDiff = arrival.Subtract(strt);

                    /*if (!string.IsNullOrEmpty(parentId) && parentId == "9001239") //Updated as per the email "SEB - - Parent Acct #9001239"
                    {
                        TravelTotal = Math.Round(((Convert.ToDateTime(arrivalDateTime).Subtract(Convert.ToDateTime(startDateTime))).TotalMinutes) * 1.58333, 2);                        
                    }
                    else
                    {
                        TravelTotal = Math.Round(((Convert.ToDateTime(arrivalDateTime).Subtract(Convert.ToDateTime(startDateTime))).TotalMinutes) * 1.41666, 2);
                    }*/


                    //******                    
                    int isDateHolidayWeekend = Utility.IsDateHolidayWeekend(strt.ToString("MM/dd/yyyy"), FarmerBrothersEntitites);

                    int OnCallStartTime = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallStartTime"]);
                    int OnCallStartTimeMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallStartTimeMinutes"]);
                    int OnCallEndTime = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallEndTime"]);

                    DateTime AfterHoursStartTime = Convert.ToDateTime(strt.ToString("MM-dd-yyyy")).AddHours(OnCallStartTime).AddMinutes(OnCallStartTimeMinutes);
                    DateTime AfterHourdsEndTime = Convert.ToDateTime(strt.ToString("MM-dd-yyyy")).AddHours(OnCallEndTime);

                    if (isDateHolidayWeekend == 1 || isDateHolidayWeekend == 2 ||
                       (strt > AfterHoursStartTime || strt < AfterHourdsEndTime))
                    {
                        if (Convert.ToBoolean(priceDtls.AfterHoursRatesApply))
                        {
                            travelRateDefined = priceDtls.AfterHoursTravelRate;                            
                        }
                    }

                    //******





                    decimal? travelAmt = priceDtls == null ? 0 : travelRateDefined;
                    TravelTotal = Convert.ToDouble(travelAmt * Convert.ToDecimal(timeDiff.TotalHours));
                }
                //else if (string.IsNullOrEmpty(startDateTime) && !string.IsNullOrEmpty(arrivalDateTime))
                //{
                //    TravelTotal = ((Convert.ToDateTime(arrivalDateTime).Ticks) * 1440) * 1.41666;
                //}
                //else if (!string.IsNullOrEmpty(startDateTime) && string.IsNullOrEmpty(arrivalDateTime))
                //{
                //    TravelTotal = ((Convert.ToDateTime(startDateTime).Ticks) * 1440) * 1.41666;
                //}
                //else if (string.IsNullOrEmpty(startDateTime) && string.IsNullOrEmpty(arrivalDateTime))
                //{
                //    TravelTotal = 0;
                //}
                else
                {
                    TravelTotal = 0;
                }

                if (!string.IsNullOrEmpty(arrivalDateTime) && !string.IsNullOrEmpty(completionDateTime))
                {
                    DateTime completion = Convert.ToDateTime(completionDateTime);
                    DateTime arrival = Convert.ToDateTime(arrivalDateTime);
                    TimeSpan timeDiff = completion.Subtract(arrival);

                    /*if (!string.IsNullOrEmpty(parentId) && parentId == "9001239") //Updated as per the email "SEB - - Parent Acct #9001239"
                    {
                        LaborTotal = Math.Round(((Convert.ToDateTime(completionDateTime).Subtract(Convert.ToDateTime(arrivalDateTime))).TotalMinutes) * 1.58333, 2);
                    }
                    else
                    {
                        LaborTotal = Math.Round(((Convert.ToDateTime(completionDateTime).Subtract(Convert.ToDateTime(arrivalDateTime))).TotalMinutes) * 1.41666, 2);
                    }*/

                    //******                    
                    int isDateHolidayWeekend = Utility.IsDateHolidayWeekend(completion.ToString("MM/dd/yyyy"), FarmerBrothersEntitites);

                    int OnCallStartTime = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallStartTime"]);
                    int OnCallStartTimeMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallStartTimeMinutes"]);
                    int OnCallEndTime = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallEndTime"]);

                    DateTime AfterHoursStartTime = Convert.ToDateTime(completion.ToString("MM-dd-yyyy")).AddHours(OnCallStartTime).AddMinutes(OnCallStartTimeMinutes);
                    DateTime AfterHourdsEndTime = Convert.ToDateTime(completion.ToString("MM-dd-yyyy")).AddHours(OnCallEndTime);

                    if (isDateHolidayWeekend == 1 || isDateHolidayWeekend == 2 ||
                       (completion > AfterHoursStartTime || completion < AfterHourdsEndTime))
                    {
                        if (Convert.ToBoolean(priceDtls.AfterHoursRatesApply))
                        {
                            laborRateDefined = priceDtls.AfterHoursTravelRate;
                        }
                    }

                    decimal? laborAmt = priceDtls == null ? 0 : laborRateDefined;
                    LaborTotal = Convert.ToDouble(laborAmt * Convert.ToDecimal(timeDiff.TotalHours));

                }
                //else if (string.IsNullOrEmpty(arrivalDateTime) && !string.IsNullOrEmpty(completionDateTime))
                //{
                //    LaborTotal = ((Convert.ToDateTime(arrivalDateTime).Ticks) * 1440) * 1.41666;
                //}
                //else if (!string.IsNullOrEmpty(arrivalDateTime) && string.IsNullOrEmpty(completionDateTime))
                //{
                //    LaborTotal = ((Convert.ToDateTime(completionDateTime).Ticks) * 1440) * 1.41666;
                //}
                //if (string.IsNullOrEmpty(arrivalDateTime) && string.IsNullOrEmpty(completionDateTime))
                //{
                //    LaborTotal = 0;
                //}
                else
                {
                    LaborTotal = 0;
                }

                //PartsTotal = Math.Round(Convert.ToDouble(Quantity * SKUCost), 2);
                double partsTotalValue = Math.Round(Convert.ToDouble(Quantity * SKUCost), 2);
                double partsDiscount = priceDtls == null ? 0 : Convert.ToDouble(priceDtls.PartsDiscount/100);
                double partsDiscountValue = partsTotalValue * (partsDiscount);
                PartsTotal = partsTotalValue + partsDiscountValue;

                
                TotalInvoice = Math.Round((TravelTotal + LaborTotal + PartsTotal),2);

                FBBillingSearchResult.TravelTotal =TravelTotal.ToString();
                FBBillingSearchResult.LaborTotal = LaborTotal.ToString();
                FBBillingSearchResult.PartsTotal = PartsTotal.ToString();
                FBBillingSearchResult.TotalInvoice = TotalInvoice.ToString();
                FBBillingSearchResult.CustomerPO = dr["CustomerPO"] == DBNull.Value ? "" : dr["CustomerPO"].ToString();
                FBBillingSearchResult.HardnessRating = dr["HardnessRating"] == DBNull.Value ? "" : dr["HardnessRating"].ToString();

                TECH_HIERARCHY techHView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == techId).FirstOrDefault();
                dynamic travelDetails = Utility.GetTravelDetailsBetweenZipCodes(techHView.PostalCode, WorkorderID);
                decimal distance = 0;
                if (travelDetails != null)
                {
                    var element = travelDetails.rows[0].elements[0];
                    distance = element == null ? 0 : (element.distance == null ? 0 : element.distance.value * (decimal)0.000621371192);
                    distance = Math.Round(distance, 0);
                }
                FBBillingSearchResult.Distance = distance.ToString();


                BillingData.Add(FBBillingSearchResult);
            }
            BillingRptModel.SearchResults = BillingData;

            return BillingData;
        }

        public JsonResult ClearBillingResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new BillingReportSearchResultModel(), JsonRequestBehavior.AllowGet);
        }

        public void BillingReportExcelExport()
        {
            try
            {
                BillingReportModel BillingModel = new BillingReportModel();

                IList<BillingReportSearchResultModel> searchResults = new List<BillingReportSearchResultModel>();

                if (TempData["SearchCriteria"] != null)
                {
                    BillingModel = TempData["SearchCriteria"] as BillingReportModel;
                    searchResults = GetBillingreport(BillingModel);
                }

                TempData["SearchCriteria"] = BillingModel;
                string gridModel = HttpContext.Request.Params["GridModel"];

                var count = searchResults.Count;
                ExcelExport exp = new ExcelExport();
                GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
                IWorkbook book = exp.Export(properties, searchResults, "BillingReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron",true);
                book.ActiveSheet.Range["AT"].NumberFormat = "$             #,##0.00";
                book.ActiveSheet.Range["AU"].NumberFormat = "$             #,##0.00";
                book.ActiveSheet.Range["AV"].NumberFormat = "$             #,##0.00";
                book.ActiveSheet.Range["AW"].NumberFormat = "$             #,##0.00";

                //Fit column width to data
                book.ActiveSheet.UsedRange.AutofitColumns();

                book.SaveAs("BillingReports.xlsx", ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.Open);
            }
            catch(Exception ex)
            {

            }
        }
        #endregion

        #region BillingUploadReport
        public ActionResult BillingUploadReport(int? isBack)
        {
            BillingUploadReportModel BillingRptModel = new BillingUploadReportModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                BillingRptModel = TempData["SearchCriteria"] as BillingUploadReportModel;
                TempData["SearchCriteria"] = BillingRptModel;
            }
            else
            {
                BillingRptModel = new BillingUploadReportModel();
                TempData["SearchCriteria"] = null;
            }

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            BillingRptModel.Technicianlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                if (dr[0].ToString() != "" && dr[0].ToString() != null)
                {
                    tech.TechID = dr[0].ToString();
                    if (dr[0].ToString() == "SPD")
                    {
                        tech.TechName = "Internal";
                        TechnicianAffs.Add(tech);
                    }
                    if (dr[0].ToString() == "SPT")
                    {
                        tech.TechName = "3rd Party";
                        TechnicianAffs.Add(tech);
                    }

                }

            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            BillingRptModel.FamilyAffs = TechnicianAffs;

            BillingRptModel.SearchResults = new List<BillingUploadReportSearchResultModel>();

            return View(BillingRptModel);
        }

        public JsonResult SearchBillingUploadReport(BillingUploadReportModel BillingRptModel)
        {
            if (string.IsNullOrEmpty(BillingRptModel.ParentACC))
            {
                BillingRptModel.ParentACC = "0";
            }

            if ((!BillingRptModel.BillingFromDate.HasValue)
                && !BillingRptModel.BillingToDate.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                BillingRptModel.SearchResults = GetBillingUploadReport(BillingRptModel);
                //int totalCount = BillingRptModel.SearchResults.Sum(item => Convert.ToInt32(item.ToatlEventsByTech));
                //BillingReportSearchResultModel BillingSearchResultModel = new BillingReportSearchResultModel();
                //BillingSearchResultModel.BranchName = "Total Calls";
                //BillingSearchResultModel.ToatlEventsByTech = totalCount.ToString();
                //BillingRptModel.SearchResults.Add(BillingSearchResultModel);


                TempData["SearchCriteria"] = BillingRptModel;
                //return Json(BillingRptModel.SearchResults, JsonRequestBehavior.AllowGet);
                JsonResult result = Json(new
                {
                    rows = BillingRptModel.SearchResults
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;

                return result;
            }
        }

        private IList<BillingUploadReportSearchResultModel> GetBillingUploadReport(BillingUploadReportModel BillingRptModel)
        {
            List<BillingUploadReportSearchResultModel> BillingData = new List<BillingUploadReportSearchResultModel>();
            String DF = BillingRptModel.BillingFromDate.ToString();
            String DT = Convert.ToDateTime(BillingRptModel.BillingToDate).AddDays(1).ToString();
            //String TL = BillingRptModel.DealerId.ToString();
            //String FA = BillingRptModel.TechID.ToString();
            //String PPID = BillingRptModel.ParentACC == null ? "0" : BillingRptModel.ParentACC.ToString();
            //String AccountNo = BillingRptModel.AccountNo == null ? "0" : BillingRptModel.AccountNo.ToString();

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fbBillingUpload("USP_BillingUpload_Report", DF, DT);
            //decimal LaborCost = Convert.ToDecimal(ConfigurationManager.AppSettings["LaborCost"]);
            //string ssql = @"select c.PricingParentID, c.CompanyName, c.Address1, w.WorkorderCallstatus, 
            //                        w.NoServiceRequired, ROW_NUMBER() Over(Partition by w.workorderid Order By w.WorkorderId) As SN, c.ContactID, 'Request Date as Blank',
            //                        wbd.BillingCode, wbd.Quantity, 'Blank Column', 'Quantity * Cost',
            //                        w.WorkorderID, w.WorkorderCloseDate, w.CustomerPO, ws.TechName, 
            //                        wp.ModelNo, 'Serial Number', 'Fixed Message'
            //                        from workorder w
            //                        inner join contact c on w.CustomerID = c.ContactID
            //                        inner join WorkorderSchedule ws on ws.workorderid = w.workorderid
            //                        left join WorkorderParts wp on wp.workorderid = w.workorderid
            //                        left join WorkorderBillingDetails wbd on wbd.workorderid = w.workorderid
            //                        where (c.BillingCode = 'S08') --and ws.AssignedStatus = 'Accepted' ";

            //ssql = ssql + " --and W.WorkorderCloseDate >='" + DF + "'";

            //ssql = ssql + "  --and  W.WorkorderCloseDate <'" + DT + "'";

           

            //ssql = ssql + " order by W.WorkorderID";

            //DataTable dt = mars.fnTpspVendors(ssql);


            BillingUploadReportSearchResultModel FBBillingSearchResult;
            foreach (DataRow dr in dt.Rows)
            {
                FBBillingSearchResult = new BillingUploadReportSearchResultModel();
                string ParentId = dr["PricingParentID"] == DBNull.Value ? "" : dr["PricingParentID"].ToString();
                FBBillingSearchResult.PricingParentID = ParentId;
                FBBillingSearchResult.CompanyName = dr["CompanyName"] == DBNull.Value ? "" : dr["CompanyName"].ToString();
                FBBillingSearchResult.Address1 = dr["Address1"] == DBNull.Value ? "" : dr["Address1"].ToString();
                FBBillingSearchResult.WorkorderCallstatus = dr["WorkorderCallstatus"] == DBNull.Value ? "" : dr["WorkorderCallstatus"].ToString();
                FBBillingSearchResult.NoServiceRequired = dr["NoServiceRequired"] == DBNull.Value ? "" : dr["NoServiceRequired"].ToString();
                FBBillingSearchResult.SequenceNumber = dr["SequenceNumber"] == DBNull.Value ? "" : dr["SequenceNumber"].ToString();
                FBBillingSearchResult.CustomerID = dr["CustomerID"] == DBNull.Value ? "" : dr["CustomerID"].ToString();
                FBBillingSearchResult.BillingCode = dr["ItemRef"] == DBNull.Value ? "" : dr["ItemRef"].ToString();
                FBBillingSearchResult.RequestDate= dr["RequestDate"] == DBNull.Value ? "" : dr["RequestDate"].ToString();
                FBBillingSearchResult.DocTy = dr["DocTy"] == DBNull.Value ? "" : dr["DocTy"].ToString();
                int Quantity = dr["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Quantity"]);
                FBBillingSearchResult.Quantity = Quantity.ToString();
                FBBillingSearchResult.SecondItemNumber = dr["ItemRef"] == DBNull.Value ? "" : dr["ItemRef"].ToString();
                FBBillingSearchResult.TravelLaborTime = dr["TravelLaborTime"] == DBNull.Value ? "" : dr["TravelLaborTime"].ToString();
                FBBillingSearchResult.SKUCost = dr["Cost"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Cost"]);
                FBBillingSearchResult.TotalInvoice = dr["TotalPrice"] == DBNull.Value ? "" : dr["TotalPrice"].ToString();
                FBBillingSearchResult.WorkorderID = dr["WorkorderID"] == DBNull.Value ? "" : dr["WorkorderID"].ToString();
                FBBillingSearchResult.WorkorderCloseDate = dr["WorkorderCloseDate"] == DBNull.Value ? "" : dr["WorkorderCloseDate"].ToString();
                FBBillingSearchResult.CustomerPO = dr["CustomerPO"] == DBNull.Value ? "" : dr["CustomerPO"].ToString();
                FBBillingSearchResult.TechName = dr["TechName"] == DBNull.Value ? "" : dr["TechName"].ToString();
                FBBillingSearchResult.Model = dr["ModelNumber"] == DBNull.Value ? "" : dr["ModelNumber"].ToString();
                FBBillingSearchResult.SerialNumber = dr["SerialNumber"] == DBNull.Value ? "" : dr["SerialNumber"].ToString();
                FBBillingSearchResult.FixedMessage = dr["Message"] == DBNull.Value ? "" : dr["Message"].ToString();

                BillingData.Add(FBBillingSearchResult);
            }
            BillingRptModel.SearchResults = BillingData;
            
            return BillingData;
        }

        public JsonResult ClearBillingUploadResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new BillingUploadReportSearchResultModel(), JsonRequestBehavior.AllowGet);
        }

        public void BillingUploadReportExcelExport()
        {
            try
            {
                BillingUploadReportModel BillingModel = new BillingUploadReportModel();

                IList<BillingUploadReportSearchResultModel> searchResults = new List<BillingUploadReportSearchResultModel>();

                if (TempData["SearchCriteria"] != null)
                {
                    BillingModel = TempData["SearchCriteria"] as BillingUploadReportModel;
                    searchResults = GetBillingUploadReport(BillingModel);
                }

                TempData["SearchCriteria"] = BillingModel;
                string gridModel = HttpContext.Request.Params["GridModel"];

                var count = searchResults.Count;
                ExcelExport exp = new ExcelExport();
                GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
                IWorkbook book = exp.Export(properties, searchResults, "BillingReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron", true);
                book.ActiveSheet.Range["AT"].NumberFormat = "$             #,##0.00";
                book.ActiveSheet.Range["AU"].NumberFormat = "$             #,##0.00";
                book.ActiveSheet.Range["AV"].NumberFormat = "$             #,##0.00";
                book.ActiveSheet.Range["AW"].NumberFormat = "$             #,##0.00";

                //Fit column width to data
                book.ActiveSheet.UsedRange.AutofitColumns();

                book.SaveAs("BillingUploadReports.xlsx", ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.Open);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region SuperInvoiceReport
        [HttpGet]
        public ActionResult SuperInvoiceReport(int? isBack)
        {
            SuperInvoiceModel SuperInvModel = new SuperInvoiceModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                SuperInvModel = TempData["SearchCriteria"] as SuperInvoiceModel;
                TempData["SearchCriteria"] = SuperInvModel;
            }
            else
            {
                SuperInvModel = new SuperInvoiceModel();
                TempData["SearchCriteria"] = null;
            }

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            SuperInvModel.Technicianlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                if (dr[0].ToString() != "" && dr[0].ToString() != null)
                {
                    tech.TechID = dr[0].ToString();
                    if (dr[0].ToString() == "SPD")
                    {
                        tech.TechName = "Internal";
                        TechnicianAffs.Add(tech);
                    }
                    if (dr[0].ToString() == "SPT")
                    {
                        tech.TechName = "3rd Party";
                        TechnicianAffs.Add(tech);
                    }

                }

            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            SuperInvModel.FamilyAffs = TechnicianAffs;

            SuperInvModel.SearchResults = new List<SuperInvoiceSearchResultModel>();

            return View(SuperInvModel);
        }

        public JsonResult SearchSupInv(SuperInvoiceModel SuperInvModel)
        {          
            if (string.IsNullOrEmpty(SuperInvModel.ParentACC))
            {
                SuperInvModel.ParentACC = "0";
            }            

            if ((!SuperInvModel.SuperCallFromDate.HasValue)
                && !SuperInvModel.SuperCallToDate.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                SuperInvModel.SearchResults = GetSupInv(SuperInvModel);
                int totalCount = SuperInvModel.SearchResults.Sum(item => Convert.ToInt32(item.ToatlEventsByTech));
                SuperInvoiceSearchResultModel SuperInvoiceSearchResultModel = new SuperInvoiceSearchResultModel();
                SuperInvoiceSearchResultModel.BranchName = "Total Calls";
                SuperInvoiceSearchResultModel.ToatlEventsByTech = totalCount.ToString();
                SuperInvModel.SearchResults.Add(SuperInvoiceSearchResultModel);


                TempData["SearchCriteria"] = SuperInvModel;
                return Json(SuperInvModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }
        private IList<SuperInvoiceSearchResultModel> GetSupInv(SuperInvoiceModel SuperInvModel)
        {
            List<SuperInvoiceSearchResultModel> supinvParts = new List<SuperInvoiceSearchResultModel>();
            String DF = SuperInvModel.SuperCallFromDate.ToString();
            String DT = Convert.ToDateTime(SuperInvModel.SuperCallToDate).AddDays(1).ToString();
            String TL = SuperInvModel.DealerId.ToString();
            String FA = SuperInvModel.TechID.ToString();
            String PPID = SuperInvModel.ParentACC.ToString();
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fbSuperInvoice("USP_SuperInvoice_Report", DF, DT, PPID, FA, TL);
            SuperInvoiceSearchResultModel FBSuperInvSearchResult;
            foreach (DataRow dr in dt.Rows)
            {
                FBSuperInvSearchResult = new SuperInvoiceSearchResultModel();
                FBSuperInvSearchResult.Region = dr["Region"].ToString();
                FBSuperInvSearchResult.BranchNumber = dr["BranchNumber"].ToString();
                FBSuperInvSearchResult.Route = dr["Route"].ToString();
                FBSuperInvSearchResult.ESM = dr["ESM"].ToString();
                FBSuperInvSearchResult.Technician = dr["Technician"].ToString();
                FBSuperInvSearchResult.BranchName = dr["BranchName"].ToString();
                FBSuperInvSearchResult.ToatlEventsByTech = dr["ToatlEventsByTech"].ToString();
                FBSuperInvSearchResult.ElapsedTime = dr["ElapsedTime"].ToString();
                FBSuperInvSearchResult.ElapsedTimeOnSite = dr["ElapsedTimeOnSite"].ToString();
                if (dr["FamilyAff"].ToString() == "SPD")
                {
                    FBSuperInvSearchResult.FamilyAff = "Internal";
                }
                if (dr["FamilyAff"].ToString() == "SPT")
                {
                    FBSuperInvSearchResult.FamilyAff = "3rd Party";
                }
                FBSuperInvSearchResult.TechId = dr["TechId"].ToString();
                supinvParts.Add(FBSuperInvSearchResult);
            }
            SuperInvModel.SearchResults = supinvParts;

            return supinvParts;
        }
        public JsonResult ClearSuperInvResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new SuperInvoiceSearchResultModel(), JsonRequestBehavior.AllowGet);
        }

        public void SupInvExcelExport()
        {
            SuperInvoiceModel SuperInvModel = new SuperInvoiceModel();

            IList<SuperInvoiceSearchResultModel> searchResults = new List<SuperInvoiceSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                SuperInvModel = TempData["SearchCriteria"] as SuperInvoiceModel;
                searchResults = GetSupInv(SuperInvModel);
            }

            TempData["SearchCriteria"] = SuperInvModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "SuperInvReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        private List<SuperInvoiceSearchResultModel> GetSupTechInv(SuperInvoiceModel SuperInvModel, String SDate, String EDate, String TechID, String PACC, int? Tid,string ESM, string Route, string Branch, string Region)
        {
            string techId = Tid == null ? "-1" : Tid.ToString();
            List<SuperInvoiceSearchResultModel> supinvTechParts = new List<SuperInvoiceSearchResultModel>();
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fbSuperInvoiceByTech("USP_SuperInvoice_ReportDetails", SDate, EDate, PACC, TechID, techId, ESM, Route, Branch, Region);
            SuperInvoiceSearchResultModel FBSuperInvSearchResult;
            foreach (DataRow dr in dt.Rows)
            {
                FBSuperInvSearchResult = new SuperInvoiceSearchResultModel();
                FBSuperInvSearchResult.EventID = dr["EventID"].ToString();
                FBSuperInvSearchResult.ContactID = dr["ContactID"].ToString();
                FBSuperInvSearchResult.FulfillmentStatus = dr["FulfillmentStatus"].ToString();
                FBSuperInvSearchResult.EntryDate = dr["EntryDate"].ToString();
                FBSuperInvSearchResult.CloseDate = dr["CloseDate"].ToString();
                FBSuperInvSearchResult.StartDateTime = dr["StartDateTime"].ToString();
                FBSuperInvSearchResult.ArrivalDateTime = dr["ArrivalDateTime"].ToString();
                FBSuperInvSearchResult.CompletionDateTime = dr["CompletionDateTime"].ToString();
                FBSuperInvSearchResult.PricingParentID = dr["PricingParentID"].ToString();
                FBSuperInvSearchResult.TechID = dr["TechID"].ToString();
                FBSuperInvSearchResult.Technician = dr["Technician"].ToString();
                FBSuperInvSearchResult.BranchNumber = dr["BranchNumber"].ToString();
                FBSuperInvSearchResult.FamilyAff = dr["FamilyAff"].ToString();
                FBSuperInvSearchResult.InvoiceNo = dr["InvoiceNo"].ToString();
                FBSuperInvSearchResult.BranchName = dr["BranchName"].ToString();
                FBSuperInvSearchResult.Region = dr["Region"].ToString();
                FBSuperInvSearchResult.ESM = dr["ESM"].ToString();
                FBSuperInvSearchResult.CompanyName = dr["CompanyName"].ToString();
                FBSuperInvSearchResult.Address1 = dr["Address1"].ToString();
                FBSuperInvSearchResult.City = dr["City"].ToString();
                FBSuperInvSearchResult.State = dr["State"].ToString();
                FBSuperInvSearchResult.Zip = dr["Zip"].ToString();
                FBSuperInvSearchResult.OldSearchType = dr["OldSearchType"].ToString();
                FBSuperInvSearchResult.CallTypeID = dr["CallTypeID"].ToString();
                FBSuperInvSearchResult.CallTypeDesc = dr["CallTypeDesc"].ToString();
                FBSuperInvSearchResult.Scheduledate = dr["Scheduledate"].ToString();
                FBSuperInvSearchResult.CustomerBranchNo = dr["CustomerBranchNo"].ToString();
                FBSuperInvSearchResult.RegionNumber = dr["RegionNumber"].ToString();
                FBSuperInvSearchResult.CustomerBranch = dr["CustomerBranch"].ToString();
                FBSuperInvSearchResult.SearchType = dr["SearchType"].ToString();
                FBSuperInvSearchResult.SearchDesc = dr["SearchDesc"].ToString();
                FBSuperInvSearchResult.Route = dr["Route"].ToString();
                FBSuperInvSearchResult.PricingParentDesc = dr["PricingParentDesc"].ToString();
                FBSuperInvSearchResult.CustomerRegion = dr["CustomerRegion"].ToString();
                FBSuperInvSearchResult.DoNotPay = dr["DoNotPay"].ToString();
                FBSuperInvSearchResult.DoNotPayComments = dr["DoNotPayComments"].ToString();
                FBSuperInvSearchResult.Estimate = dr["Estimate"] == null ? "" : dr["Estimate"].ToString();
                FBSuperInvSearchResult.FinalEstimate = dr["FinalEstimate"] == null ? "" : dr["FinalEstimate"].ToString();
                FBSuperInvSearchResult.EstimateApprovedBy = dr["EstimateApprovedBy"] == null ? "" : dr["EstimateApprovedBy"].ToString();
                FBSuperInvSearchResult.ThirdPartyPO = dr["ThirdPartyPO"] == null ? "" : dr["ThirdPartyPO"].ToString();

                supinvTechParts.Add(FBSuperInvSearchResult);
            }
            SuperInvModel.SearchResults = supinvTechParts;

            return supinvTechParts;
        }

        public FileResult SupInvTechExcelExport(int? id)
        {
            String SDate = Request.QueryString["SDate"];
            String EDate = Request.QueryString["EDate"];
            EDate = Convert.ToDateTime(EDate).AddDays(1).ToString();
            String TechID = Request.QueryString["TechID"];
            String Route = string.Empty;
            String ESM = string.Empty;
            String PACC = string.Empty;
            String Branch = string.Empty;
            String Region = string.Empty;
            if (string.IsNullOrEmpty(Request.QueryString["PACC"]) || Request.QueryString["PACC"] == "null")
            {
                PACC = "0";
            }
            else
            {
                PACC = Request.QueryString["PACC"];
            }
            if (string.IsNullOrEmpty(Request.QueryString["ESM"]) || Request.QueryString["ESM"] == "null")
            {
                ESM = "-1";
            }
            else
            {
                ESM = Request.QueryString["ESM"];
            }
            if (string.IsNullOrEmpty(Request.QueryString["Route"]) || Request.QueryString["Route"] == "null")
            {
                Route = "-1";
            }
            else
            {
                Route = Request.QueryString["Route"];
            }
            if (string.IsNullOrEmpty(Request.QueryString["Branch"]) || Request.QueryString["Branch"] == "null")
            {
                Branch = "-1";
            }
            else
            {
                Branch = Request.QueryString["Branch"];
            }
            if (string.IsNullOrEmpty(Request.QueryString["Region"]) || Request.QueryString["Region"] == "null")
            {
                Region = "-1";
            }
            else
            {
                Region = Request.QueryString["Region"];
            }

            SuperInvoiceModel SuperInvModel = new SuperInvoiceModel();

            if (TempData["SearchCriteria"] != null)
            {
                SuperInvModel = TempData["SearchCriteria"] as SuperInvoiceModel;
                SupInvTechsearchResults = GetSupTechInv(SuperInvModel, SDate, EDate, TechID, PACC, id, ESM, Route, Branch, Region);
            }

            TempData["SearchCriteria"] = SuperInvModel;

            string[] columns = { "EventID", "ContactID", "FulfillmentStatus", "EntryDate", "CloseDate", "StartDateTime", "ArrivalDateTime", "CompletionDateTime", "PricingParentID", "TechID",
                "Technician", "BranchNumber", "FamilyAff", "InvoiceNo", "BranchName", "Region", "ESM", "CompanyName", "Address1", "City", "State", "Zip", "OldSearchType", "CallTypeID", "CallTypeDesc",
                "Scheduledate", "CustomerBranchNo", "RegionNumber", "CustomerBranch", "SearchType", "SearchDesc", "Route", "PricingParentDesc", "CustomerRegion", "DoNotPay", "DoNotPayComments",
                "Estimate", "FinalEstimate", "EstimateApprovedBy", "ThirdPartyPO"};
            byte[] filecontent = ExcelExportHelper.ExportExcel(SupInvTechsearchResults, "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "SupInvTechExcel.xlsx");

        }
        #endregion

        #region RepeatCall

        [HttpGet]
        public ActionResult RepeatCallReportSummary(int? isBack)
        {

            RepeatCallReportSummaryModel RepeatCallModel = new RepeatCallReportSummaryModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                RepeatCallModel = TempData["SearchCriteria"] as RepeatCallReportSummaryModel;
                TempData["SearchCriteria"] = RepeatCallModel;
            }
            else
            {
                RepeatCallModel = new RepeatCallReportSummaryModel();
                TempData["SearchCriteria"] = null;
            }

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            RepeatCallModel.Technicianlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                tech.TechID = dr[0].ToString();
                if (dr[0].ToString() == "SPD")
                {
                    tech.TechName = "Internal";
                    TechnicianAffs.Add(tech);
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                    TechnicianAffs.Add(tech);
                }

            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            RepeatCallModel.FamilyAffs = TechnicianAffs;


            RepeatCallModel.SearchResults = new List<RepeatCallrptsummaryResultModel>();

            return View(RepeatCallModel);
        }

        public JsonResult SearchRepeatCall(RepeatCallReportSummaryModel RepeatCallModel)
        {
            if ((!RepeatCallModel.RepeatCallFromDate.HasValue)
                && !RepeatCallModel.RepeatCallToDate.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<RepeatCallReportSummaryModel>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                RepeatCallModel.SearchResults = GetRepeatCallSummary(RepeatCallModel);
                int totalRepeatCount = RepeatCallModel.SearchResults.Sum(item => Convert.ToInt32(item.RepeatCount));
                RepeatCallrptsummaryResultModel repeatCallSearchResultModel = new RepeatCallrptsummaryResultModel();
                repeatCallSearchResultModel.TechId = "Total";
                repeatCallSearchResultModel.RepeatCount = totalRepeatCount.ToString();
                RepeatCallModel.SearchResults.Add(repeatCallSearchResultModel);

                TempData["SearchCriteria"] = RepeatCallModel;
                return Json(RepeatCallModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<RepeatCallrptsummaryResultModel> GetRepeatCallSummary(RepeatCallReportSummaryModel RepeatCallModel)
        {
            string fromdate = Convert.ToDateTime(RepeatCallModel.RepeatCallFromDate).ToString("MM/dd/yyyy");
            DateTime enddate = Convert.ToDateTime(RepeatCallModel.RepeatCallToDate);
            enddate.AddDays(1);
            string todate = Convert.ToDateTime(enddate).ToString("MM/dd/yyyy");


            string ssql = @"SELECT Count(WorkorderID) as RepeatCount,Branch,CustomerBranch,CustomerRegion,OriginalEventTechID,OriginalEventTechName,FSMName 
                            from V_RepeatedPMandServiceEventDetails  where 1=1 ";

            ssql = ssql + " and RepeatedEntryDate >='" + fromdate + "'";

            ssql = ssql + "  and  RepeatedEntryDate <'" + todate + "'";

            if (RepeatCallModel.DealerId > 0)
            {
                ssql = ssql + "and OriginalEventTechID =" + RepeatCallModel.DealerId;
            }

            if (RepeatCallModel.TechID == "SPD")
            {
                ssql = ssql + " and FamilyAff != 'SPT'";
            }
            if (RepeatCallModel.TechID == "SPT")
            {
                ssql = ssql + " and FamilyAff = 'SPT'";
            }


            ssql = ssql + "Group by  Branch,CustomerBranch,CustomerRegion,OriginalEventTechID,OriginalEventTechName,FSMName ORDER BY CustomerRegion,CustomerBranch";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<RepeatCallrptsummaryResultModel> searchResults = new List<RepeatCallrptsummaryResultModel>();
            RepeatCallrptsummaryResultModel RptCallSrchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                RptCallSrchResultModel = new RepeatCallrptsummaryResultModel();
                RptCallSrchResultModel.BranchId = dr["Branch"].ToString();
                RptCallSrchResultModel.BranchName = dr["CustomerBranch"].ToString();
                RptCallSrchResultModel.TechName = dr["OriginalEventTechName"].ToString();
                RptCallSrchResultModel.TechId = dr["OriginalEventTechID"].ToString();
                RptCallSrchResultModel.ESM = dr["FSMName"].ToString();
                RptCallSrchResultModel.Region = dr["CustomerRegion"].ToString();
                RptCallSrchResultModel.RepeatCount = dr["RepeatCount"].ToString();
                searchResults.Add(RptCallSrchResultModel);
            }

            return searchResults;
        }
        public JsonResult ClearRepeatCallResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new RepeatCallReportSummaryModel(), JsonRequestBehavior.AllowGet);
        }

        public void RepeatCallExcelExport()
        {

            RepeatCallReportSummaryModel RepeatCallModel = new RepeatCallReportSummaryModel();

            IList<RepeatCallrptsummaryResultModel> searchResults = new List<RepeatCallrptsummaryResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                RepeatCallModel = TempData["SearchCriteria"] as RepeatCallReportSummaryModel;
                searchResults = GetRepeatCallSummary(RepeatCallModel);
            }

            TempData["SearchCriteria"] = RepeatCallModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "RepeatCallReportSummary.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        public ActionResult RepeatCallReport(int Id)
        {
            string frmDate = Request.QueryString["fromdate"];
            string toDate = Request.QueryString["todate"];
            string familyAff = Request.QueryString["familyAff"];
            string branch = Request.QueryString["branch"];
            RepeatCallReportResult repeatCallResult = new RepeatCallReportResult();
            repeatCallResult.SearchResults = new List<RepeatcallReportModel>();

            repeatCallResult.SearchResults = GetRepeatCall(Id, frmDate, toDate, branch, familyAff);

            DataManagerConverter.Serializer = new DMSerial();
            ViewBag.datasource = repeatCallResult.SearchResults;

            TempData["repeatCallTechId"] = Id;
            TempData["repeatCallFromDate"] = frmDate;
            TempData["repeatCallToDate"] = toDate;
            TempData["repeatcallBranch"] = branch;
            TempData["repeatcallFamilyAff"] = familyAff;

            return View();
        }

        public class DMSerial : IDataSourceSerializer
        {
            public string Serialize(object obj)
            {
                var str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                return str;
            }
        }

        private List<RepeatcallReportModel> GetRepeatCall(int techid, string frmDate, string toDate, string branch, string familyAff)
        {

            DateTime startdate = Convert.ToDateTime(frmDate);
            DateTime enddate = Convert.ToDateTime(toDate);
            enddate.AddDays(1);

            string fromdate = startdate.ToString("MM/dd/yyyy");
            string todate = enddate.ToString("MM/dd/yyyy");

            string ssql = @"select * from V_RepeatedPMandServiceEventDetails where 1=1";
            ssql = ssql + " and RepeatedEntryDate >='" + fromdate + "'  ";
            ssql = ssql + " and RepeatedEntryDate < '" + todate + "'  ";

            if (techid != 0)
            {
                ssql = ssql + " and  OriginalEventTechId =" + techid;
            }
            if (branch != "All")
            { 
                ssql = ssql + " and Branch='" + branch + "'  ";
            }
            if (familyAff != "All")
            {
                ssql = ssql + " and FamilyAff='" + familyAff + "'  ";
            }

            ssql = ssql + " ORDER BY RepeatedEntryDate ";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<RepeatcallReportModel> searchResults = new List<RepeatcallReportModel>();
            RepeatcallReportModel RptCallResultModel;

            foreach (DataRow dr in dt.Rows)
            {
                RptCallResultModel = new RepeatcallReportModel();
                RptCallResultModel.EventID = dr["WorkorderID"].ToString();
                RptCallResultModel.CustomerID = dr["CustomerID"].ToString();
                RptCallResultModel.CustomerName = dr["CompanyName"].ToString();
                RptCallResultModel.CustomerType = dr["SearchType"].ToString();
                RptCallResultModel.CustomerBranchID = dr["Branch"].ToString();
                RptCallResultModel.BranchName = dr["CustomerBranch"].ToString();
                RptCallResultModel.Region = dr["CustomerRegion"].ToString();
                RptCallResultModel.ESM = dr["FSMName"].ToString();
                RptCallResultModel.SerialNumber = dr["RepeatSerialNumber"].ToString();
                RptCallResultModel.Manufacturer = dr["Manufacturer"].ToString();
                //RptCallResultModel.WorkorderID = dr["RepeatedEventID"].ToString();
                RptCallResultModel.EntryDate = dr["RepeatedEntryDate"].ToString();
                RptCallResultModel.originalWorkorderID = dr["WorkorderID"].ToString();
                RptCallResultModel.OriginalWrkorderEntryDate = dr["WorkorderEntryDate"].ToString();
                RptCallResultModel.OriginalWrkorderClosedDate = dr["WorkorderCloseDate"].ToString();
                RptCallResultModel.OrgTechName = dr["OriginalEventTechName"].ToString();
                RptCallResultModel.OrgTechId = dr["OriginalEventTechID"].ToString();
                RptCallResultModel.FamilyAff = dr["FamilyAff"].ToString();
                searchResults.Add(RptCallResultModel);
            }
            return searchResults;
        }

        private DataTable GetRepeatCallDataTable(int techid, string frmDate, string toDate, string branch, string familyAff)
        {
            DateTime startdate = Convert.ToDateTime(frmDate);
            DateTime enddate = Convert.ToDateTime(toDate);
            enddate.AddDays(1);

            string fromdate = startdate.ToString("MM/dd/yyyy");
            string todate = enddate.ToString("MM/dd/yyyy");
                        
            string ssql = @"select WorkorderID,CustomerID,CompanyName,SearchType,Branch,CustomerBranch,CustomerRegion,FSMName,
                                RepeatSerialNumber,Manufacturer,RepeatedEventID,Convert(varchar(30),RepeatedEntryDate,121) as RepeatedEntryDate,Convert(varchar(30),WorkorderEntryDate,121) as WorkorderEntryDate,Convert(varchar(30),WorkorderCloseDate,121) as WorkorderCloseDate,OriginalEventTechName,
                                OriginalEventTechID,FamilyAff from V_RepeatedPMandServiceEventDetails where 1=1";
            ssql = ssql + " and RepeatedEntryDate >='" + fromdate + "'  ";
            ssql = ssql + " and RepeatedEntryDate < '" + todate + "'  ";

            if (techid != 0)
            {
                ssql = ssql + " and  OriginalEventTechId =" + techid;
            }
            if (branch != "All")
            {
                ssql = ssql + " and Branch='" + branch + "'  ";
            }
            if (familyAff != "All")
            {
                ssql = ssql + " and FamilyAff='" + familyAff + "'  ";
            }


            ssql = ssql + " ORDER BY RepeatedEntryDate ";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);

            return dt;
        }


        public FileResult RepeatCallReportExcelExport()
        {
            RepeatCallReportResult repeatResult = new RepeatCallReportResult();            
            //repeatResult.SearchResults = new List<RepeatcallReportModel>();
            IList<RepeatcallReportModel> searchResults = new List<RepeatcallReportModel>();
            DataTable dt = new DataTable();

            if (TempData["repeatCallTechId"] != null &&
            TempData["repeatCallFromDate"] != null &&
            TempData["repeatCallToDate"] != null &&
            TempData["repeatcallBranch"] != null &&
            TempData["repeatcallFamilyAff"] != null )
            {
                int techid = Convert.ToInt32(TempData["repeatCallTechId"]);
                string frmdate = Convert.ToString(TempData["repeatCallFromDate"]);
                string todate = Convert.ToString(TempData["repeatCallToDate"]);
                string branch = TempData["repeatcallBranch"].ToString();
                string familyAff = TempData["repeatcallFamilyAff"].ToString();
                dt = GetRepeatCallDataTable(techid, frmdate, todate, branch, familyAff);
            }

            string gridModel = HttpContext.Request.Params["GridModel"];

            //ExcelExport exp = new ExcelExport();
            //GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);            
            //exp.Export(properties, repeatResult.SearchResults, "RepeatCallReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
            

            string[] columns = {"WorkorderID","CustomerID","CompanyName","SearchType","Branch","CustomerBranch","CustomerRegion","FSMName",
                                "RepeatSerialNumber","Manufacturer","RepeatedEventID","RepeatedEntryDate","WorkorderEntryDate","WorkorderCloseDate","OriginalEventTechName",
                                "OriginalEventTechID","FamilyAff" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(dt, "", false, columns);
            var fileStream = new MemoryStream(filecontent);

            //return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "RepeatCallReports.csv");
            return File(filecontent, ExcelExportHelper.ExcelContentType, "RepeatCallReports.xlsx");
            //return File(filecontent, "text/csv", string.Format("RepeatCallReports.csv", DateTime.Now.ToString("yyyyMMdd-HHmmss")));
            
        }

        public ActionResult OriginalEventDetails(int Id)
        {
            OriginalEventDetailsResultsModel OriginalEventDetailsResult = new OriginalEventDetailsResultsModel();
            OriginalEventDetailsResult.SearchResults = new List<OriginalEventDetailsModel>();

            OriginalEventDetailsResult.SearchResults = GetOriginalEventDetails(Id);

             TempData["OriginalEventDetailsId"] = Id;

            return View(OriginalEventDetailsResult);
        }

        public void OriginalEventDetailsExcelExport()
        {
            OriginalEventDetailsResultsModel OriginalEventDetailsResult = new OriginalEventDetailsResultsModel();
            OriginalEventDetailsResult.SearchResults = new List<OriginalEventDetailsModel>();

            if (TempData["OriginalEventDetailsId"] != null)
            {
                int eventId = Convert.ToInt32(TempData["OriginalEventDetailsId"]);
                OriginalEventDetailsResult.SearchResults = GetOriginalEventDetails(eventId);
            }

            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, OriginalEventDetailsResult.SearchResults, "OriginalEventDetailsReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        private List<OriginalEventDetailsModel> GetOriginalEventDetails(int EventId)
        {
            string ssql = @"select * from V_OriginalCallDetails where 1=1";
            
            if (EventId != 0)
            {
                ssql = ssql + " and  WorkorderID =" + EventId;
            }

            ssql = ssql + " ORDER BY WorkorderID ";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<OriginalEventDetailsModel> searchResults = new List<OriginalEventDetailsModel>();
            OriginalEventDetailsModel OriginalEventDetailsModel;

            foreach (DataRow dr in dt.Rows)
            {
                OriginalEventDetailsModel = new OriginalEventDetailsModel();
                OriginalEventDetailsModel.EventID = dr["WorkorderID"].ToString();
                OriginalEventDetailsModel.Status = dr["WorkorderCallstatus"].ToString();
                OriginalEventDetailsModel.EntryDate = dr["WorkorderEntryDate"].ToString();
                OriginalEventDetailsModel.CustomerRegion = dr["RegionNumber"].ToString();
                OriginalEventDetailsModel.CustomerRegionName = dr["CustomerRegion"].ToString();
                OriginalEventDetailsModel.CustomerBranch = dr["CustomerBranchNo"].ToString();
                OriginalEventDetailsModel.CustomerBranchName = dr["CustomerBranch"].ToString();
                OriginalEventDetailsModel.JDE = dr["ContactID"].ToString();
                OriginalEventDetailsModel.CompanyName = dr["CompanyName"].ToString();
                OriginalEventDetailsModel.Address1 = dr["Address1"].ToString();
                OriginalEventDetailsModel.City = dr["City"].ToString();
                OriginalEventDetailsModel.State = dr["State"].ToString();
                OriginalEventDetailsModel.PostalCode = dr["PostalCode"].ToString();
                OriginalEventDetailsModel.DispatchCompany = dr["DispatchCompany"].ToString();
                OriginalEventDetailsModel.DispatchDate = dr["Scheduledate"].ToString();
                OriginalEventDetailsModel.ElapsedTime = dr["ElapsedTime"].ToString();
                searchResults.Add(OriginalEventDetailsModel);
            }
            return searchResults;
        }

        #endregion


        #region RepeatRepair

        [HttpGet]
        public ActionResult RepeatRepairReportSummary(int? isBack)
        {

            RepeatCallReportSummaryModel RepeatCallModel = new RepeatCallReportSummaryModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                RepeatCallModel = TempData["SearchCriteria"] as RepeatCallReportSummaryModel;
                TempData["SearchCriteria"] = RepeatCallModel;
            }
            else
            {
                RepeatCallModel = new RepeatCallReportSummaryModel();
                TempData["SearchCriteria"] = null;
            }

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            RepeatCallModel.Technicianlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                tech.TechID = dr[0].ToString();
                if (dr[0].ToString() == "SPD")
                {
                    tech.TechName = "Internal";
                    TechnicianAffs.Add(tech);
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                    TechnicianAffs.Add(tech);
                }

            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            RepeatCallModel.FamilyAffs = TechnicianAffs;


            RepeatCallModel.SearchResults = new List<RepeatCallrptsummaryResultModel>();

            return View(RepeatCallModel);
        }

        public JsonResult SearchRepeatRepair(RepeatCallReportSummaryModel RepeatCallModel)
        {
            if ((!RepeatCallModel.RepeatCallFromDate.HasValue)
                && !RepeatCallModel.RepeatCallToDate.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<RepeatCallReportSummaryModel>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                RepeatCallModel.SearchResults = GetRepeatCallSummary(RepeatCallModel);
                int totalRepeatCount = RepeatCallModel.SearchResults.Sum(item => Convert.ToInt32(item.RepeatCount));
                RepeatCallrptsummaryResultModel repeatCallSearchResultModel = new RepeatCallrptsummaryResultModel();
                repeatCallSearchResultModel.TechId = "Total";
                repeatCallSearchResultModel.RepeatCount = totalRepeatCount.ToString();
                RepeatCallModel.SearchResults.Add(repeatCallSearchResultModel);

                TempData["SearchCriteria"] = RepeatCallModel;
                return Json(RepeatCallModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<RepeatCallrptsummaryResultModel> GetRepeatRepairSummary(RepeatCallReportSummaryModel RepeatCallModel)
        {
            string fromdate = Convert.ToDateTime(RepeatCallModel.RepeatCallFromDate).ToString("MM/dd/yyyy");
            DateTime enddate = Convert.ToDateTime(RepeatCallModel.RepeatCallToDate);
            enddate.AddDays(1);
            string todate = Convert.ToDateTime(enddate).ToString("MM/dd/yyyy");


            string ssql = @"SELECT Count(WorkorderID) as RepeatCount,Branch,CustomerBranch,CustomerRegion,OriginalEventTechID,OriginalEventTechName,FSMName 
                            from V_RepeatedPMandServiceEventDetails  where 1=1 ";

            ssql = ssql + " and RepeatedEntryDate >='" + fromdate + "'";

            ssql = ssql + "  and  RepeatedEntryDate <'" + todate + "'";

            if (RepeatCallModel.DealerId > 0)
            {
                ssql = ssql + "and OriginalEventTechID =" + RepeatCallModel.DealerId;
            }

            if (RepeatCallModel.TechID == "SPD")
            {
                ssql = ssql + " and FamilyAff != 'SPT'";
            }
            if (RepeatCallModel.TechID == "SPT")
            {
                ssql = ssql + " and FamilyAff = 'SPT'";
            }


            ssql = ssql + "Group by  Branch,CustomerBranch,CustomerRegion,OriginalEventTechID,OriginalEventTechName,FSMName ORDER BY CustomerRegion,CustomerBranch";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<RepeatCallrptsummaryResultModel> searchResults = new List<RepeatCallrptsummaryResultModel>();
            RepeatCallrptsummaryResultModel RptCallSrchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                RptCallSrchResultModel = new RepeatCallrptsummaryResultModel();
                RptCallSrchResultModel.BranchId = dr["Branch"].ToString();
                RptCallSrchResultModel.BranchName = dr["CustomerBranch"].ToString();
                RptCallSrchResultModel.TechName = dr["OriginalEventTechName"].ToString();
                RptCallSrchResultModel.TechId = dr["OriginalEventTechID"].ToString();
                RptCallSrchResultModel.ESM = dr["FSMName"].ToString();
                RptCallSrchResultModel.Region = dr["CustomerRegion"].ToString();
                RptCallSrchResultModel.RepeatCount = dr["RepeatCount"].ToString();
                searchResults.Add(RptCallSrchResultModel);
            }

            return searchResults;
        }
        public JsonResult ClearRepeatRepairResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new RepeatCallReportSummaryModel(), JsonRequestBehavior.AllowGet);
        }

        public void RepeatRepairExcelExport()
        {

            RepeatCallReportSummaryModel RepeatCallModel = new RepeatCallReportSummaryModel();

            IList<RepeatCallrptsummaryResultModel> searchResults = new List<RepeatCallrptsummaryResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                RepeatCallModel = TempData["SearchCriteria"] as RepeatCallReportSummaryModel;
                searchResults = GetRepeatRepairSummary(RepeatCallModel);
            }

            TempData["SearchCriteria"] = RepeatCallModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "RepeatRepairReportSummary.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        public ActionResult RepeatRepairReport(int Id)
        {
            string frmDate = Request.QueryString["fromdate"];
            string toDate = Request.QueryString["todate"];
            string familyAff = Request.QueryString["familyAff"];
            string branch = Request.QueryString["branch"];
            RepeatCallReportResult RepeatRepairResult = new RepeatCallReportResult();
            RepeatRepairResult.SearchResults = new List<RepeatcallReportModel>();

            RepeatRepairResult.SearchResults = GetRepeatRepair(Id, frmDate, toDate, branch, familyAff);

            DataManagerConverter.Serializer = new DMSerial();
            ViewBag.datasource = RepeatRepairResult.SearchResults;

            TempData["repeatCallTechId"] = Id;
            TempData["repeatCallFromDate"] = frmDate;
            TempData["repeatCallToDate"] = toDate;
            TempData["repeatcallBranch"] = branch;
            TempData["repeatcallFamilyAff"] = familyAff;

            return View();
        }
                
        private List<RepeatcallReportModel> GetRepeatRepair(int techid, string frmDate, string toDate, string branch, string familyAff)
        {

            DateTime startdate = Convert.ToDateTime(frmDate);
            DateTime enddate = Convert.ToDateTime(toDate);
            enddate.AddDays(1);

            string fromdate = startdate.ToString("MM/dd/yyyy");
            string todate = enddate.ToString("MM/dd/yyyy");

            string ssql = @"select * from V_RepeatedPMandServiceEventDetails where 1=1";
            ssql = ssql + " and RepeatedEntryDate >='" + fromdate + "'  ";
            ssql = ssql + " and RepeatedEntryDate < '" + todate + "'  ";

            if (techid != 0)
            {
                ssql = ssql + " and  OriginalEventTechId =" + techid;
            }
            if (branch != "All")
            {
                ssql = ssql + " and Branch='" + branch + "'  ";
            }
            if (familyAff != "All")
            {
                ssql = ssql + " and FamilyAff='" + familyAff + "'  ";
            }

            ssql = ssql + " ORDER BY RepeatedEntryDate ";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<RepeatcallReportModel> searchResults = new List<RepeatcallReportModel>();
            RepeatcallReportModel RptCallResultModel;

            foreach (DataRow dr in dt.Rows)
            {
                RptCallResultModel = new RepeatcallReportModel();
                RptCallResultModel.EventID = dr["WorkorderID"].ToString();
                RptCallResultModel.CustomerID = dr["CustomerID"].ToString();
                RptCallResultModel.CustomerName = dr["CompanyName"].ToString();
                RptCallResultModel.CustomerType = dr["SearchType"].ToString();
                RptCallResultModel.CustomerBranchID = dr["Branch"].ToString();
                RptCallResultModel.BranchName = dr["CustomerBranch"].ToString();
                RptCallResultModel.Region = dr["CustomerRegion"].ToString();
                RptCallResultModel.ESM = dr["FSMName"].ToString();
                RptCallResultModel.SerialNumber = dr["RepeatSerialNumber"].ToString();
                RptCallResultModel.Manufacturer = dr["Manufacturer"].ToString();
                //RptCallResultModel.WorkorderID = dr["RepeatedEventID"].ToString();
                RptCallResultModel.EntryDate = dr["RepeatedEntryDate"].ToString();
                RptCallResultModel.originalWorkorderID = dr["WorkorderID"].ToString();
                RptCallResultModel.OriginalWrkorderEntryDate = dr["WorkorderEntryDate"].ToString();
                RptCallResultModel.OriginalWrkorderClosedDate = dr["WorkorderCloseDate"].ToString();
                RptCallResultModel.OrgTechName = dr["OriginalEventTechName"].ToString();
                RptCallResultModel.OrgTechId = dr["OriginalEventTechID"].ToString();
                RptCallResultModel.FamilyAff = dr["FamilyAff"].ToString();
                searchResults.Add(RptCallResultModel);
            }
            return searchResults;
        }

        private DataTable GetRepeatRepairDataTable(int techid, string frmDate, string toDate, string branch, string familyAff)
        {
            DateTime startdate = Convert.ToDateTime(frmDate);
            DateTime enddate = Convert.ToDateTime(toDate);
            enddate.AddDays(1);

            string fromdate = startdate.ToString("MM/dd/yyyy");
            string todate = enddate.ToString("MM/dd/yyyy");

            string ssql = @"select WorkorderID,CustomerID,CompanyName,SearchType,Branch,CustomerBranch,CustomerRegion,FSMName,
                                RepeatSerialNumber,Manufacturer,RepeatedEventID,Convert(varchar(30),RepeatedEntryDate,121) as RepeatedEntryDate,Convert(varchar(30),WorkorderEntryDate,121) as WorkorderEntryDate,Convert(varchar(30),WorkorderCloseDate,121) as WorkorderCloseDate,OriginalEventTechName,
                                OriginalEventTechID,FamilyAff from V_RepeatedPMandServiceEventDetails where 1=1";
            ssql = ssql + " and RepeatedEntryDate >='" + fromdate + "'  ";
            ssql = ssql + " and RepeatedEntryDate < '" + todate + "'  ";

            if (techid != 0)
            {
                ssql = ssql + " and  OriginalEventTechId =" + techid;
            }
            if (branch != "All")
            {
                ssql = ssql + " and Branch='" + branch + "'  ";
            }
            if (familyAff != "All")
            {
                ssql = ssql + " and FamilyAff='" + familyAff + "'  ";
            }


            ssql = ssql + " ORDER BY RepeatedEntryDate ";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);

            return dt;
        }


        public FileResult RepeatRepairReportExcelExport()
        {
            RepeatCallReportResult repeatResult = new RepeatCallReportResult();
            //repeatResult.SearchResults = new List<RepeatcallReportModel>();
            IList<RepeatcallReportModel> searchResults = new List<RepeatcallReportModel>();
            DataTable dt = new DataTable();

            if (TempData["repeatCallTechId"] != null &&
            TempData["repeatCallFromDate"] != null &&
            TempData["repeatCallToDate"] != null &&
            TempData["repeatcallBranch"] != null &&
            TempData["repeatcallFamilyAff"] != null)
            {
                int techid = Convert.ToInt32(TempData["repeatCallTechId"]);
                string frmdate = Convert.ToString(TempData["repeatCallFromDate"]);
                string todate = Convert.ToString(TempData["repeatCallToDate"]);
                string branch = TempData["repeatcallBranch"].ToString();
                string familyAff = TempData["repeatcallFamilyAff"].ToString();
                dt = GetRepeatRepairDataTable(techid, frmdate, todate, branch, familyAff);
            }

            string gridModel = HttpContext.Request.Params["GridModel"];

            //ExcelExport exp = new ExcelExport();
            //GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);            
            //exp.Export(properties, repeatResult.SearchResults, "RepeatCallReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");


            string[] columns = {"WorkorderID","CustomerID","CompanyName","SearchType","Branch","CustomerBranch","CustomerRegion","FSMName",
                                "RepeatSerialNumber","Manufacturer","RepeatedEventID","RepeatedEntryDate","WorkorderEntryDate","WorkorderCloseDate","OriginalEventTechName",
                                "OriginalEventTechID","FamilyAff" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(dt, "", false, columns);
            var fileStream = new MemoryStream(filecontent);

            //return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "RepeatCallReports.csv");
            return File(filecontent, ExcelExportHelper.ExcelContentType, "RepeatRepairReports.xlsx");
            //return File(filecontent, "text/csv", string.Format("RepeatCallReports.csv", DateTime.Now.ToString("yyyyMMdd-HHmmss")));

        }

        #endregion


        #region PMSchedules
        [HttpGet]
        public ActionResult PMSchedules()
        {
            PMSchedulesModel pmSchedulesModel = new PMSchedulesModel();
            return View(pmSchedulesModel);
        }

        public JsonResult SearchPMSchedule(PMSchedulesModel pmSchedulesModel)
        {
            if ((!pmSchedulesModel.DateFrom.HasValue)
                && !pmSchedulesModel.DateTo.HasValue && string.IsNullOrEmpty(pmSchedulesModel.CustomerJDE))
            //if (string.IsNullOrEmpty(pmSchedulesModel.CustomerJDE))
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<PMSchedulesSearchResultModel> pMSchedules = GetPMSchedules(pmSchedulesModel);
                pmSchedulesModel.SearchResults = pMSchedules;
                TempData["SearchCriteria"] = pmSchedulesModel;
                //return Json(pmSchedulesModel.SearchResults, JsonRequestBehavior.AllowGet);
                return new JsonResult()
                {
                    Data = pmSchedulesModel.SearchResults,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        private IList<PMSchedulesSearchResultModel> GetPMSchedules(PMSchedulesModel pmSchedulesModel)
        {
            int customerId = 0;
            if (!string.IsNullOrEmpty(pmSchedulesModel.CustomerJDE))
            {
                customerId = Convert.ToInt32(pmSchedulesModel.CustomerJDE);
            }
            int techId = 0;
            if (!string.IsNullOrEmpty(pmSchedulesModel.TechJDE))
            {
                techId = Convert.ToInt32(pmSchedulesModel.TechJDE);
            }

            string startDate = "0";
            string endDate = "0";

            if (pmSchedulesModel.DateFrom.HasValue)
            {
                startDate = Convert.ToDateTime(pmSchedulesModel.DateFrom).ToString("MM-dd-yyyy");
            }
            if (pmSchedulesModel.DateTo.HasValue)
            {
                endDate = Convert.ToDateTime(pmSchedulesModel.DateTo).ToString("MM-dd-yyyy");
            }

            //string startDate = pmSchedulesModel.DateFrom.ToString();
            //string endDate = pmSchedulesModel.DateTo.ToString();

            MarsViews mars = new MarsViews();
            //DataTable dt = mars.GetPMScheduleData(pmSchedulesModel.DateFrom.ToString(), pmSchedulesModel.DateTo.ToString(), 1, customerId, techId);
            DataTable dt = mars.GetPMScheduleData(customerId, startDate, endDate);
            List<PMSchedulesSearchResultModel> pmScheduleParts = new List<PMSchedulesSearchResultModel>();
            PMSchedulesSearchResultModel pMSchedulesSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                pMSchedulesSearchResultModel = new PMSchedulesSearchResultModel();
                pMSchedulesSearchResultModel.FBNo = dr["EventID"].ToString();
                pMSchedulesSearchResultModel.ContactID = dr["ContactID"].ToString();
                pMSchedulesSearchResultModel.CustomerName = dr["CustomerName"].ToString();
                pMSchedulesSearchResultModel.TechID = dr["TechID"].ToString();
                pMSchedulesSearchResultModel.TechName = dr["TechName"].ToString();
                pMSchedulesSearchResultModel.ContactName = dr["ContactName"].ToString();
                pMSchedulesSearchResultModel.Phone = dr["Phone"].ToString();
                pMSchedulesSearchResultModel.StartDate = dr["StartDate"].ToString();
                pMSchedulesSearchResultModel.IntervalType = dr["IntervalType"].ToString();
                pMSchedulesSearchResultModel.NextRunDate = dr["NextRunDate"].ToString();
                pMSchedulesSearchResultModel.IntervalDuration = dr["IntervalDuration"].ToString();
                pMSchedulesSearchResultModel.Notes = dr["Notes"].ToString();
                pmScheduleParts.Add(pMSchedulesSearchResultModel);
            }
            return pmScheduleParts;
        }
        public JsonResult ClearPMScheduleResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new PMSchedulesModel(), JsonRequestBehavior.AllowGet);
        }

        public void PMScheduleExcelExport()
        {
            PMSchedulesModel pMSchedulesModel = new PMSchedulesModel();

            IList<PMSchedulesSearchResultModel> searchResults = new List<PMSchedulesSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                pMSchedulesModel = TempData["SearchCriteria"] as PMSchedulesModel;
                searchResults = GetPMSchedules(pMSchedulesModel);
            }

            TempData["SearchCriteria"] = pMSchedulesModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "PMScheduleReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        #endregion

        #region FBDeltaCustomersNCCCallReport
        public ActionResult NCCDispatchReport(int? isBack)
        {
            FBDeltaCustomersNCCCallReportsModel fbDeltaCustomersNCCCallReportsModel = new FBDeltaCustomersNCCCallReportsModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                fbDeltaCustomersNCCCallReportsModel = TempData["SearchCriteria"] as FBDeltaCustomersNCCCallReportsModel;
                TempData["SearchCriteria"] = fbDeltaCustomersNCCCallReportsModel;
            }
            else
            {
                fbDeltaCustomersNCCCallReportsModel = new FBDeltaCustomersNCCCallReportsModel();
                TempData["SearchCriteria"] = null;
            }
            fbDeltaCustomersNCCCallReportsModel.SearchResults = new List<FBDeltaCustomersNCCCallSearchResultModel>();
            return View(fbDeltaCustomersNCCCallReportsModel);
        }

        public JsonResult SearchFBDeltaCustomers(FBDeltaCustomersNCCCallReportsModel fbDeltaCustomersres)
        {
            if (string.IsNullOrWhiteSpace(fbDeltaCustomersres.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(fbDeltaCustomersres.DateTo.ToString())
                )
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                fbDeltaCustomersres.SearchResults = GetFBDeltaCustomers(fbDeltaCustomersres);
                TempData["SearchCriteria"] = fbDeltaCustomersres;
                return Json(fbDeltaCustomersres.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        public List<FBDeltaCustomersNCCCallSearchResultModel> GetFBDeltaCustomers(FBDeltaCustomersNCCCallReportsModel searchcriteria)
        {
            List<FBDeltaCustomersNCCCallSearchResultModel> FBDeltaCustrest = new List<FBDeltaCustomersNCCCallSearchResultModel>();
            String DF = searchcriteria.DateFrom.ToString();
            String DT = searchcriteria.DateTo.ToString();
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fbDeltaVendors("USP_FBDispatch_TimeElapsed", DF, DT);
            FBDeltaCustomersNCCCallSearchResultModel FBDeltaCustomersSearchResult;
            foreach (DataRow dr in dt.Rows)
            {
                FBDeltaCustomersSearchResult = new FBDeltaCustomersNCCCallSearchResultModel();
                FBDeltaCustomersSearchResult.EventID = dr["EventID"].ToString();
                FBDeltaCustomersSearchResult.ElapsedTime = dr["ElapsedTime"].ToString();
                FBDeltaCustomersSearchResult.ScheduleUserName = dr["ScheduleUserName"].ToString();
                FBDeltaCustomersSearchResult.CallTypeDesc = dr["CallTypeDesc"].ToString();
                FBDeltaCustrest.Add(FBDeltaCustomersSearchResult);
            }
            searchcriteria.SearchResults = FBDeltaCustrest;

            return FBDeltaCustrest;
        }
        public void FBDeltaExcelExport()
        {
            FBDeltaCustomersNCCCallReportsModel FBDeltaCustreports = new FBDeltaCustomersNCCCallReportsModel();

            IList<FBDeltaCustomersNCCCallSearchResultModel> searchResults = new List<FBDeltaCustomersNCCCallSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                FBDeltaCustreports = TempData["SearchCriteria"] as FBDeltaCustomersNCCCallReportsModel;
                searchResults = GetFBDeltaCustomers(FBDeltaCustreports);
            }

            TempData["SearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "FBDeltaJDEReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        public JsonResult CleardeltaCustomerResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new FBDeltaCustomersNCCCallReportsModel(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region TechAvailabilityUpdateLog
        [HttpGet]
        public ActionResult TechAvailabilityUpdateLog()
        {
            TechAvailabilityReportsModel techReportsModel = new TechAvailabilityReportsModel();
            return View(techReportsModel);
        }

        public JsonResult SearchTechAvailability(TechAvailabilityReportsModel techReportsModel)
        {
            if ((!techReportsModel.DateFrom.HasValue)
                && !techReportsModel.DateTo.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                techReportsModel.SearchResults = GetTechAvilabilityLogs(techReportsModel);
                techReportsModel.OnCallSearchResults = GetOnCallTechAvilabilityLogs(techReportsModel);

                TempData["TechAvailabilitySearchCriteria"] = techReportsModel;
                return Json(techReportsModel, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ClearTechAvailResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new TechAvailabilityReportsModel(), JsonRequestBehavior.AllowGet);
        }

        public static IList<TechnicianAvailabilitySearchResultModel> GetOnCallTechAvilabilityLogs(TechAvailabilityReportsModel techReportsModel)
        {
            string StartDate = Convert.ToDateTime(techReportsModel.DateFrom).ToString("yyyy-MM-dd");
            string EndDate = Convert.ToDateTime(techReportsModel.DateTo).ToString("yyyy-MM-dd");

            List<TechnicianAvailabilitySearchResultModel> SearchResults = new List<TechnicianAvailabilitySearchResultModel>();
            string query = @"select t.DealerId,t.CompanyName,ts.ModifiedDate,u.FirstName +' '+u.LastName as UpdatedBy from TechOnCall ts (nolock)
                            inner join TECH_HIERARCHY t (nolock) on ts.TechId = t.DealerId 
                            inner join FbUserMaster u (nolock) on ts.ModifiedUserID = u.UserId 
                            where ts.ModifiedDate >= '" + StartDate + "' and ts.ModifiedDate <= '" + EndDate + "'";

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetDatatable(query);
            foreach (DataRow dr in dt.Rows)
            {
                SearchResults.Add(new TechnicianAvailabilitySearchResultModel(dr));
            }
            return SearchResults;

        }

        public static IList<TechnicianAvailabilitySearchResultModel> GetTechAvilabilityLogs(TechAvailabilityReportsModel techReportsModel)
        {
            string StartDate = Convert.ToDateTime(techReportsModel.DateFrom).ToString("yyyy-MM-dd");
            string EndDate = Convert.ToDateTime(techReportsModel.DateTo).ToString("yyyy-MM-dd");

            List<TechnicianAvailabilitySearchResultModel> SearchResults = new List<TechnicianAvailabilitySearchResultModel>();
            string query = @"select t.DealerId,t.CompanyName,ts.ModifiedDate,u.FirstName +' '+u.LastName as UpdatedBy from TechSchedule ts (nolock) 
                            inner join TECH_HIERARCHY t (nolock) on ts.TechId = t.DealerId 
                            inner join FbUserMaster u (nolock) on ts.ModifiedUserID = u.UserId 
                            where ts.ModifiedDate >= '" + StartDate + "' and ts.ModifiedDate <= '" + EndDate + "'";

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetDatatable(query);
            foreach (DataRow dr in dt.Rows)
            {
                SearchResults.Add(new TechnicianAvailabilitySearchResultModel(dr));
            }
            return SearchResults;

        }

        public void TechOnCallAvailabilityExcelExportReport()
        {
            TechAvailabilityReportsModel TechAvailabilitySchedulesModel = new TechAvailabilityReportsModel();

            IList<TechnicianAvailabilitySearchResultModel> searchResults = new List<TechnicianAvailabilitySearchResultModel>();

            if (TempData["TechAvailabilitySearchCriteria"] != null)
            {
                TechAvailabilitySchedulesModel = TempData["TechAvailabilitySearchCriteria"] as TechAvailabilityReportsModel;
                searchResults = GetOnCallTechAvilabilityLogs(TechAvailabilitySchedulesModel);
            }

            TempData["TechAvailabilitySearchCriteria"] = TechAvailabilitySchedulesModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "TechAvailability.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }
        public void TechAvailabilityExcelExportReport()
        {
            TechAvailabilityReportsModel TechAvailabilitySchedulesModel = new TechAvailabilityReportsModel();

            IList<TechnicianAvailabilitySearchResultModel> searchResults = new List<TechnicianAvailabilitySearchResultModel>();

            if (TempData["TechAvailabilitySearchCriteria"] != null)
            {
                TechAvailabilitySchedulesModel = TempData["TechAvailabilitySearchCriteria"] as TechAvailabilityReportsModel;
                searchResults = GetTechAvilabilityLogs(TechAvailabilitySchedulesModel);
            }

            TempData["TechAvailabilitySearchCriteria"] = TechAvailabilitySchedulesModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "TechAvailability.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        #endregion

        #region FBReOpen
        [HttpGet]
        public ActionResult FBReOpen()
        {
            return RedirectToAction("ReopenWorkOrder", "ReopenWorkOrder", new { IsFromReports = true });
        }

        #endregion

        #region ClosurePartsReport
        [HttpGet]
        public ActionResult ClosurePartsReport()
        {
            ClosurePartsModel closurePartsModel = new ClosurePartsModel();
            TempData["SearchCriteria"] = null; 
            return View(closurePartsModel);
        }

        //public JsonResult SearchClosureParts(ClosurePartsModel closurePartsModel)
        //{
        //    IList<ClosurePartsSearchResultModel> closureParts = GetCloserParts(closurePartsModel);
        //    closurePartsModel.SearchResults = closureParts;
        //    TempData["SearchCriteria"] = closurePartsModel;
        //    return Json(closureParts, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SearchClosureParts")]
        public ActionResult SearchClosureParts(ClosurePartsModel closurePartsModel)
        {
            IList<ClosurePartsSearchResultModel> closureParts = GetCloserParts(closurePartsModel);
            closurePartsModel.SearchResults = closureParts;
            TempData["SearchCriteria"] = closurePartsModel;
            return View("ClosurePartsReport", closurePartsModel);
        }

        private IList<ClosurePartsSearchResultModel> GetCloserParts(ClosurePartsModel closurePartsModel)
        {

            string StartDate = string.IsNullOrEmpty(closurePartsModel.CloseDateStart.ToString()) ? "YH9999YH" : Convert.ToDateTime(closurePartsModel.CloseDateStart).ToString("yyyy-MM-dd");
            string EndDate = string.IsNullOrEmpty(closurePartsModel.CloseDateEnd.ToString()) ? "YH9999YH" : Convert.ToDateTime(closurePartsModel.CloseDateEnd).ToString("yyyy-MM-dd");
            string JDENo = string.IsNullOrEmpty(closurePartsModel.JDENo) ? "YH9999YH" : Convert.ToString(closurePartsModel.JDENo);
            string EntryNo = string.IsNullOrEmpty(closurePartsModel.EntryNo) ? "YH9999YH" : Convert.ToString(closurePartsModel.EntryNo);

            string[] inputparam = { StartDate, EndDate, JDENo, EntryNo };

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetCloserPartsReportData(inputparam);

            List<ClosurePartsSearchResultModel> closerParts = new List<ClosurePartsSearchResultModel>();
            ClosurePartsSearchResultModel closurePartsSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                closurePartsSearchResultModel = new ClosurePartsSearchResultModel();
                closurePartsSearchResultModel.FBNo = dr["WorkorderID"].ToString();
                closurePartsSearchResultModel.JDENo = dr["CustomerID"].ToString();
                closurePartsSearchResultModel.FBStatus = dr["WorkorderCallstatus"].ToString();
                closurePartsSearchResultModel.EntryDate = dr["WorkorderEntryDate"].ToString();
                closurePartsSearchResultModel.CloseDate = dr["WorkorderCloseDate"].ToString();
                closurePartsSearchResultModel.CustomerType = dr["SearchType"].ToString();
                closurePartsSearchResultModel.ServiceCenterID = dr["DealerId"].ToString();
                closurePartsSearchResultModel.ServiceCompany = dr["TechName"].ToString();
                closurePartsSearchResultModel.FamilyAff = dr["FamilyAff"].ToString();
                closurePartsSearchResultModel.CallTypeID = dr["CallTypeid"].ToString();
                closurePartsSearchResultModel.SolutionID = dr["Solutionid"].ToString();
                closurePartsSearchResultModel.EntryNo = dr["E1Number"].ToString();
                closurePartsSearchResultModel.ItemNo = dr["Ben02ItemNumber"].ToString();
                closurePartsSearchResultModel.VendorNo = dr["VendorNo"].ToString();
                closurePartsSearchResultModel.Description = dr["Description"].ToString();
                closurePartsSearchResultModel.OrderSource = dr["SystemSource"].ToString();
                closurePartsSearchResultModel.Quantity = dr["Quantity"].ToString();
                closurePartsSearchResultModel.Route = dr["CustRoute"].ToString();
                closurePartsSearchResultModel.Branch = dr["Branch"].ToString();
                closurePartsSearchResultModel.Supplier = dr["Supplier"].ToString();

                closerParts.Add(closurePartsSearchResultModel);
            }
            return closerParts;
        }

        public JsonResult ClearClosurePartsResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new ClosurePartsSearchResultModel(), JsonRequestBehavior.AllowGet);
        }

        public void ClosurePartsExcelExport()
        {
            ClosurePartsModel closurePartsModel = new ClosurePartsModel();

            IList<ClosurePartsSearchResultModel> searchResults = new List<ClosurePartsSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                closurePartsModel = TempData["SearchCriteria"] as ClosurePartsModel;
                searchResults = GetCloserParts(closurePartsModel);
            }

            TempData["SearchCriteria"] = closurePartsModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "ClosurePartsReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        #endregion

        #region SerialNumberReport

        [HttpGet]
        public ActionResult SerialNumberReport()
        {
            SerialNumberModel serialNumberModel = new SerialNumberModel();
            return View(serialNumberModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SearchSerialNumber")]
        public ActionResult SearchSerialNumber(SerialNumberModel serialNumberModel)
        {
            IList<SerialNumberSearchResultModel> serialNumber = GetserialNumber(serialNumberModel);
            serialNumberModel.SearchResults = serialNumber;
            TempData["SearchCriteria"] = serialNumberModel;
            //return Json(serialNumber, JsonRequestBehavior.AllowGet);

           
            return View("SerialNumberReport", serialNumberModel);
        }

        private IList<SerialNumberSearchResultModel> GetserialNumber(SerialNumberModel serialNumberModel)
        {

            /*var predicate = PredicateBuilder.True<TMP_SerialNOReport>();

            if (serialNumberModel.CloseDateStart.HasValue)
            {
                predicate = predicate.And(w => w.EntryDate >= serialNumberModel.CloseDateStart);
            }
            if (serialNumberModel.CloseDateEnd.HasValue)
            {
                predicate = predicate.And(w => w.CloseDate <= serialNumberModel.CloseDateEnd);
            }
            if (!string.IsNullOrWhiteSpace(serialNumberModel.SerialNumber))
            {
                predicate = predicate.And(w => w.SerialNo.ToString().Contains(serialNumberModel.SerialNumber));
            }
            if (!string.IsNullOrWhiteSpace(serialNumberModel.JDENo))
            {
                predicate = predicate.And(w => w.ContactID.ToString().Contains(serialNumberModel.JDENo));
            }

            IQueryable<TMP_SerialNOReport> SerialNOResults = FarmerBrothersEntitites.Set<TMP_SerialNOReport>().AsExpandable().Where(predicate).OrderByDescending(e => e.CloseDate).Distinct();
            IList<SerialNumberSearchResultModel> searchResults = new List<SerialNumberSearchResultModel>();
            foreach (TMP_SerialNOReport serialNo in SerialNOResults)
            {
                searchResults.Add(new SerialNumberSearchResultModel(serialNo));
            }*/
            string query = "Select * from TMP_SerialNOReport Where 1=1";
            StringBuilder str = new StringBuilder();
            str.Append(query);
            str.Append(" ");
            if (serialNumberModel.CloseDateStart.HasValue)
            {
                str.Append(" And CloseDate >='");
                str.Append(serialNumberModel.CloseDateStart);
                str.Append("' ");
            }
            if (serialNumberModel.CloseDateEnd.HasValue)
            {
                str.Append(" And CloseDate <='");
                str.Append(serialNumberModel.CloseDateEnd);
                str.Append("' ");
            }
            if (!string.IsNullOrWhiteSpace(serialNumberModel.SerialNumber))
            {
                str.Append(" And SerialNo like '%" + serialNumberModel.SerialNumber + "%'");

                //str.Append(" And SerialNo ='");
                //str.Append(serialNumberModel.SerialNumber);
                //str.Append("' ");
            }
            if (!string.IsNullOrWhiteSpace(serialNumberModel.JDENo))
            {
                str.Append(" And ContactID ='");
                str.Append(serialNumberModel.JDENo);
                str.Append("' ");
            }

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetDatatable(str.ToString());
            IList<SerialNumberSearchResultModel> searchResults = new List<SerialNumberSearchResultModel>();
            foreach (DataRow dr in dt.Rows)
            {
                searchResults.Add(new SerialNumberSearchResultModel(dr));
            }

            return searchResults;

        }

        public JsonResult ClearSerialNumberResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new SerialNumberSearchResultModel(), JsonRequestBehavior.AllowGet);
        }

        public void SerialNumberExcelExport()
        {
            SerialNumberModel serialNumberModel = new SerialNumberModel();

            IList<SerialNumberSearchResultModel> searchResults = new List<SerialNumberSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                serialNumberModel = TempData["SearchCriteria"] as SerialNumberModel;
                searchResults = GetserialNumber(serialNumberModel);
            }

            TempData["SearchCriteria"] = serialNumberModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "SerialNumberReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        #endregion

        #region closureconfirmationReport


        public ActionResult ClosureConfirmationReport(int? isBack)
        {
            ClosureConfirmationModel confirmModel = new ClosureConfirmationModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                confirmModel = TempData["SearchCriteria"] as ClosureConfirmationModel;
                TempData["SearchCriteria"] = confirmModel;
            }
            else
            {
                confirmModel = new ClosureConfirmationModel();
                TempData["SearchCriteria"] = null;
            }
            return View(confirmModel);
        }

        public JsonResult ClosureConfirmationSearch(ClosureConfirmationModel confirSearch)
        {
            if (string.IsNullOrWhiteSpace(confirSearch.workorderid.ToString()))
            {
                TempData["SearchCriteria"] = null;
                return Json(new ClosureConfirmationModel(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                confirSearch.closureconfirmation = ClosureConfirmation.GetClosureConfirmationReport(confirSearch);
                TempData["SearchCriteria"] = confirSearch;
                return Json(confirSearch, JsonRequestBehavior.AllowGet);
            }
        }

        public void ClosureConfirmationExcelExport()
        {
            ClosureConfirmationModel confirmodel = new ClosureConfirmationModel();

            List<ClosureConfirmation> searchResults = new List<ClosureConfirmation>();

            if (TempData["SearchCriteria"] != null)
            {
                confirmodel = TempData["SearchCriteria"] as ClosureConfirmationModel;
                searchResults = ClosureConfirmation.GetClosureConfirmationReport(confirmodel);
            }

            TempData["SearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "ClosureConfirmationReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }


        public JsonResult ClosureConfirmationClearSearchResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new ClosureConfirmationModel(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region FakeUsersReport

        public ActionResult FakeUsersReport(int? isBack)
        {
            UnknownCustomerSearchModel unknowncustrpt = new UnknownCustomerSearchModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                unknowncustrpt = TempData["SearchCriteria"] as UnknownCustomerSearchModel;
                TempData["SearchCriteria"] = unknowncustrpt;
            }
            else
            {
                unknowncustrpt = new UnknownCustomerSearchModel();
                TempData["SearchCriteria"] = null;
            }
            unknowncustrpt.SearchResults = new List<UnknownCustomersSearchResults>();
            return View(unknowncustrpt);

        }

        public JsonResult UnknownCustomerSearch(UnknownCustomerSearchModel unknownCustSearch)
        {
            List<UnknownCustomersSearchResults> unknownCustresults = new List<UnknownCustomersSearchResults>();
            if (string.IsNullOrWhiteSpace(unknownCustSearch.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(unknownCustSearch.DateTo.ToString())
                )
            {
                TempData["SearchCriteria"] = null;
                return Json(new UnknownCustomerSearchModel(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                unknownCustSearch.SearchResults = GetUnknownCustomers(unknownCustSearch);
                TempData["SearchCriteria"] = unknownCustSearch;
                return Json(unknownCustSearch, JsonRequestBehavior.AllowGet);
            }
        }

        public List<UnknownCustomersSearchResults> GetUnknownCustomers(UnknownCustomerSearchModel searchcriteria)
        {
            List<UnknownCustomersSearchResults> unknownCustresults = new List<UnknownCustomersSearchResults>();
            var unknowCustomers = FarmerBrothersEntitites.Contacts.Where(c => (c.IsUnknownUser == 1) && (System.Data.Entity.DbFunctions.TruncateTime(c.DateCreated) >= searchcriteria.DateFrom && System.Data.Entity.DbFunctions.TruncateTime(c.DateCreated) <= searchcriteria.DateTo)).ToList();
            foreach (var v in unknowCustomers)
            {

                UnknownCustomersSearchResults SearchResult = new UnknownCustomersSearchResults()
                {
                    AccountId = v.ContactID.ToString(),
                    Company = v.CompanyName,
                    Address = v.Address1,
                    City = v.City,
                    State = v.State,
                    Zip = v.PostalCode,
                    Phone = v.PhoneWithAreaCode

                };
                unknownCustresults.Add(SearchResult);

            }
            return unknownCustresults;
        }
        public void FakeJDEExcelExport()
        {
            UnknownCustomerSearchModel unknownCustreports = new UnknownCustomerSearchModel();

            IList<UnknownCustomersSearchResults> searchResults = new List<UnknownCustomersSearchResults>();

            if (TempData["SearchCriteria"] != null)
            {
                unknownCustreports = TempData["SearchCriteria"] as UnknownCustomerSearchModel;
                searchResults = GetUnknownCustomers(unknownCustreports);
            }

            TempData["SearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "FakeJDEReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }


        public JsonResult ClearSearchResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new UnknownCustomerSearchModel(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult MachineCountReport(int? isBack)
        {
            MachineCountReportModel machinecustrpt = new MachineCountReportModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                machinecustrpt = TempData["SearchCriteria"] as MachineCountReportModel;
                TempData["SearchCriteria"] = machinecustrpt;
            }
            else
            {
                machinecustrpt = new MachineCountReportModel();
                TempData["SearchCriteria"] = null;
            }
            machinecustrpt.SearchResults = new List<MachineCountSearchResults>();
            return View(machinecustrpt);
        }
        public JsonResult MachineCustomerSearch(MachineCountReportModel machineCustSearch)
        {
            List<MachineCountSearchResults> machinCustSearch = new List<MachineCountSearchResults>();
            if (string.IsNullOrWhiteSpace(machineCustSearch.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(machineCustSearch.DateTo.ToString())
                )
            {
                TempData["SearchCriteria"] = null;
                return Json(new MachineCountReportModel(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                machineCustSearch.SearchResults = GetmachineCustomers(machineCustSearch);
                TempData["SearchCriteria"] = machineCustSearch;
                return Json(machineCustSearch, JsonRequestBehavior.AllowGet);
            }
        }

        public List<MachineCountSearchResults> GetmachineCustomers(MachineCountReportModel searchcriteria)
        {
            List<MachineCountSearchResults> machineCustresults = new List<MachineCountSearchResults>();
            List<V_EquipmentCount> machineCustomers = FarmerBrothersEntitites.V_EquipmentCount.Where(c => (c.DealerId > 0) && (System.Data.Entity.DbFunctions.TruncateTime(c.WorkorderCloseDate) >= searchcriteria.DateFrom && System.Data.Entity.DbFunctions.TruncateTime(c.WorkorderCloseDate) <= searchcriteria.DateTo)).ToList();
            var distinctTech = machineCustomers.DistinctBy(i => i.DealerId);

            var equCount = from m in machineCustomers
                           where m.DealerId > 0 && (m.WorkorderCloseDate >= searchcriteria.DateFrom) && (m.WorkorderCloseDate <= searchcriteria.DateTo)
                           group m by m.DealerId into g
                           let TotalPoints = g.Sum(m => m.EquipCount)
                           orderby TotalPoints descending
                           select new { DealerId = g.Key, EquipCount = TotalPoints };


            Dictionary<int, int> equCountDictonay = new Dictionary<int, int>();
            foreach (var item in equCount)
            {
                equCountDictonay.Add(item.DealerId, Convert.ToInt16(item.EquipCount));
            }
            foreach (var v in distinctTech)
            {

                MachineCountSearchResults SearchResult = new MachineCountSearchResults()
                {
                    DealerId = v.DealerId,
                    Company = v.CompanyName,
                    Branch = v.BranchNumber + " - " + v.BranchName,
                    EquipCount = equCountDictonay[v.DealerId],

                };
                machineCustresults.Add(SearchResult);

            }
            return machineCustresults;
        }
        public void MachinJDEExcelExport()
        {
            MachineCountReportModel machinCustreports = new MachineCountReportModel();

            IList<MachineCountSearchResults> searchResults = new List<MachineCountSearchResults>();

            if (TempData["SearchCriteria"] != null)
            {
                machinCustreports = TempData["SearchCriteria"] as MachineCountReportModel;
                searchResults = GetmachineCustomers(machinCustreports);
            }

            TempData["SearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "MachinJDEReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }


        public JsonResult ClearMachinSearchResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new MachineCountReportModel(), JsonRequestBehavior.AllowGet);
        }

        #region RejectCall
        [HttpGet]
        public ActionResult RejectCallReport(int? isBack)
        {
            RejectCallModel RejectCallModel = new RejectCallModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                RejectCallModel = TempData["SearchCriteria"] as RejectCallModel;
                TempData["SearchCriteria"] = RejectCallModel;
            }
            else
            {
                RejectCallModel = new RejectCallModel();
                TempData["SearchCriteria"] = null;
            }

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            RejectCallModel.Technicianlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                tech.TechID = dr[0].ToString();
                if (dr[0].ToString() == "SPD")
                {
                    tech.TechName = "Internal";
                    TechnicianAffs.Add(tech);
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                    TechnicianAffs.Add(tech);
                }
            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            RejectCallModel.FamilyAffs = TechnicianAffs;

            RejectCallModel.SearchResults = new List<RejectCallSearchResultModel>();

            return View(RejectCallModel);
        }

        public JsonResult SearchRejectCall(RejectCallModel RejectCallModel)
        {
            if ((!RejectCallModel.RejectCallFromDate.HasValue)
                && !RejectCallModel.RejectCallToDate.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                RejectCallModel.SearchResults = GetRejectCall(RejectCallModel);
                TempData["SearchCriteria"] = RejectCallModel;
                return Json(RejectCallModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<RejectCallSearchResultModel> GetRejectCall(RejectCallModel RejectCallModel)
        {
            string ssql = "SELECT count(WorkorderSchedule.WorkorderID) AS CountOfEventId, WorkorderSchedule.TechId, TECH_HIERARCHY.CompanyName, TECH_HIERARCHY.City,";
            ssql = ssql + "TECH_HIERARCHY.State, TECH_HIERARCHY.FamilyAff, TECH_HIERARCHY.BranchName,TECH_HIERARCHY.BranchNumber FROM WorkorderSchedule (nolock) ";
            ssql = ssql + " INNER JOIN dbo.TECH_HIERARCHY  (nolock)  ON WorkorderSchedule.TechId = TECH_HIERARCHY.DealerId";
            ssql = ssql + " where AssignedStatus = 'Declined'";

            ssql = ssql + " and WorkorderSchedule.EntryDate >='" + RejectCallModel.RejectCallFromDate + "'  ";
            ssql = ssql + " and  WorkorderSchedule.ModifiedScheduleDate <'" + RejectCallModel.RejectCallToDate + "' ";

            if (RejectCallModel.DealerId > 0)
            {
                ssql = ssql + "and TECH_HIERARCHY.dealerid =" + RejectCallModel.DealerId;
            }

            if (RejectCallModel.TechID != "All")
            {
                ssql = ssql + " and TECH_HIERARCHY.familyAff='" + RejectCallModel.TechID + "'";
            }


            ssql = ssql + " GROUP BY WorkorderSchedule.TechId, TECH_HIERARCHY.CompanyName, TECH_HIERARCHY.City, TECH_HIERARCHY.State, TECH_HIERARCHY.FamilyAff, TECH_HIERARCHY.BranchName, TECH_HIERARCHY.BranchNumber ";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<RejectCallSearchResultModel> RejectCallParts = new List<RejectCallSearchResultModel>();
            RejectCallSearchResultModel RejectCallSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                RejectCallSearchResultModel = new RejectCallSearchResultModel();
                RejectCallSearchResultModel.BranchId = dr["BranchNumber"].ToString();
                RejectCallSearchResultModel.BranchName = dr["BranchName"].ToString();
                RejectCallSearchResultModel.TechName = dr["CompanyName"].ToString();
                RejectCallSearchResultModel.TechId = dr["TechId"].ToString();
                RejectCallSearchResultModel.City = dr["City"].ToString();
                RejectCallSearchResultModel.State = dr["State"].ToString();
                RejectCallSearchResultModel.RejectCount = dr["CountOfEventId"].ToString();
                RejectCallParts.Add(RejectCallSearchResultModel);
            }
            return RejectCallParts;
        }
        public JsonResult ClearRejectCallResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new RejectCallModel(), JsonRequestBehavior.AllowGet);
        }

        public void RejectCallExcelExport()
        {
            RejectCallModel RejectCallModel = new RejectCallModel();

            IList<RejectCallSearchResultModel> searchResults = new List<RejectCallSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                RejectCallModel = TempData["SearchCriteria"] as RejectCallModel;
                searchResults = GetRejectCall(RejectCallModel);
            }

            TempData["SearchCriteria"] = RejectCallModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "RejectCallReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        #endregion

        #region RedirectCall
        [HttpGet]
        public ActionResult RedirectCallReport(int? isBack)
        {
            RedirectCallModel RedirectCallModel = new RedirectCallModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                RedirectCallModel = TempData["SearchCriteria"] as RedirectCallModel;
                TempData["SearchCriteria"] = RedirectCallModel;
            }
            else
            {
                RedirectCallModel = new RedirectCallModel();
                TempData["SearchCriteria"] = null;
            }

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            RedirectCallModel.Technicianlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                tech.TechID = dr[0].ToString();
                if (dr[0].ToString() == "SPD")
                {
                    tech.TechName = "Internal";
                    TechnicianAffs.Add(tech);
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                    TechnicianAffs.Add(tech);
                }
            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            RedirectCallModel.FamilyAffs = TechnicianAffs;

            RedirectCallModel.SearchResults = new List<RedirectCallSearchResultModel>();

            return View(RedirectCallModel);
        }

        public JsonResult SearchRedirectCall(RedirectCallModel RedirectCallModel)
        {
            if ((!RedirectCallModel.RedirectCallFromDate.HasValue)
                && !RedirectCallModel.RedirectCallToDate.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                RedirectCallModel.SearchResults = GetRedirectCall(RedirectCallModel);
                TempData["SearchCriteria"] = RedirectCallModel;
                return Json(RedirectCallModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<RedirectCallSearchResultModel> GetRedirectCall(RedirectCallModel RedirectCallModel)
        {
            string ssql = "SELECT count(WorkorderSchedule.WorkorderID) AS CountOfEventId, WorkorderSchedule.TechId, TECH_HIERARCHY.CompanyName, TECH_HIERARCHY.City,";
            ssql = ssql + "TECH_HIERARCHY.State, TECH_HIERARCHY.FamilyAff, TECH_HIERARCHY.BranchName,TECH_HIERARCHY.BranchNumber FROM WorkorderSchedule (nolock) ";
            ssql = ssql + " INNER JOIN dbo.TECH_HIERARCHY  (nolock)  ON WorkorderSchedule.TechId = TECH_HIERARCHY.DealerId";
            ssql = ssql + " where AssignedStatus = 'Redirected'";

            ssql = ssql + " and WorkorderSchedule.EntryDate >='" + RedirectCallModel.RedirectCallFromDate + "'  ";
            ssql = ssql + " and  WorkorderSchedule.ModifiedScheduleDate <'" + RedirectCallModel.RedirectCallToDate + "' ";

            if (RedirectCallModel.DealerId > 0)
            {
                ssql = ssql + "and TECH_HIERARCHY.dealerid =" + RedirectCallModel.DealerId;
            }

            if (RedirectCallModel.TechID != "All")
            {
                ssql = ssql + " and TECH_HIERARCHY.familyAff='" + RedirectCallModel.TechID + "'";
            }


            ssql = ssql + " GROUP BY WorkorderSchedule.TechId, TECH_HIERARCHY.CompanyName, TECH_HIERARCHY.City, TECH_HIERARCHY.State, TECH_HIERARCHY.FamilyAff, TECH_HIERARCHY.BranchName, TECH_HIERARCHY.BranchNumber ";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<RedirectCallSearchResultModel> RedirectCallParts = new List<RedirectCallSearchResultModel>();
            RedirectCallSearchResultModel RedirectCallSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                RedirectCallSearchResultModel = new RedirectCallSearchResultModel();
                RedirectCallSearchResultModel.BranchId = dr["BranchNumber"].ToString();
                RedirectCallSearchResultModel.BranchName = dr["BranchName"].ToString();
                RedirectCallSearchResultModel.TechName = dr["CompanyName"].ToString();
                RedirectCallSearchResultModel.TechId = dr["TechId"].ToString();
                RedirectCallSearchResultModel.City = dr["City"].ToString();
                RedirectCallSearchResultModel.State = dr["State"].ToString();
                RedirectCallSearchResultModel.RedirectCount = dr["CountOfEventId"].ToString();
                RedirectCallParts.Add(RedirectCallSearchResultModel);
            }
            return RedirectCallParts;
        }
        public JsonResult ClearRedirectCallResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new RedirectCallModel(), JsonRequestBehavior.AllowGet);
        }

        public void RedirectCallExcelExport()
        {
            RedirectCallModel RedirectCallModel = new RedirectCallModel();

            IList<RedirectCallSearchResultModel> searchResults = new List<RedirectCallSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                RedirectCallModel = TempData["SearchCriteria"] as RedirectCallModel;
                searchResults = GetRedirectCall(RedirectCallModel);
            }

            TempData["SearchCriteria"] = RedirectCallModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "RedirectCallReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        #endregion

        #region OpenCallReportByTechnician
        [HttpGet]
        public ActionResult OpenCallReportByTechnician(int? isBack)
        {
            OpenCallByTechModel OpenCallByTechModel = new OpenCallByTechModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                OpenCallByTechModel = TempData["SearchCriteria"] as OpenCallByTechModel;
                TempData["SearchCriteria"] = OpenCallByTechModel;
            }
            else
            {
                OpenCallByTechModel = new OpenCallByTechModel();
                TempData["SearchCriteria"] = null;
            }


            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            OpenCallByTechModel.Technicianlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();

            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                tech.TechID = dr[0].ToString();
                if (dr[0].ToString() == "SPD")
                {
                    tech.TechName = "Internal";
                    TechnicianAffs.Add(tech);
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                    TechnicianAffs.Add(tech);
                }


            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            OpenCallByTechModel.FamilyAffs = TechnicianAffs;

            OpenCallByTechModel.SearchResults = new List<OpenCallByTechSearchResultModel>();

            return View(OpenCallByTechModel);
        }

        public JsonResult OpenCallByTechCall(OpenCallByTechModel OpenCallByTechModel)
        {
            if ((!OpenCallByTechModel.OpenCallByTechFromDate.HasValue)
                && !OpenCallByTechModel.OpenCallByTechToDate.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                OpenCallByTechModel.SearchResults = GetOpenCallByTech(OpenCallByTechModel);

                Double TotalEvents = OpenCallByTechModel.SearchResults.Sum(item => Convert.ToDouble(item.EventCount));
                OpenCallByTechSearchResultModel openCallSearchResultModel = new OpenCallByTechSearchResultModel();
                openCallSearchResultModel.TechId = "Total";
                openCallSearchResultModel.EventCount = TotalEvents.ToString();
                OpenCallByTechModel.SearchResults.Add(openCallSearchResultModel);


                TempData["SearchCriteria"] = OpenCallByTechModel;
                return Json(OpenCallByTechModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<OpenCallByTechSearchResultModel> GetOpenCallByTech(OpenCallByTechModel OpenCallByTechModel)
        {
            //string ssql = @"SELECT dbo.ESMDSMRSM.Region, dbo.ESMDSMRSM.ESMName, dbo.WorkorderSchedule.TechId, dbo.TECH_HIERARCHY.CompanyName AS DispatchCompany,
            //                COUNT(dbo.WorkorderSchedule.TechId) AS EventCount FROM dbo.WorkorderSchedule (nolock) INNER JOIN dbo.TECH_HIERARCHY  (nolock)  
            //                ON dbo.WorkorderSchedule.TechId = dbo.TECH_HIERARCHY.DealerId INNER JOIN dbo.Workorder 
            //                ON dbo.WorkorderSchedule.WorkorderID = dbo.Workorder.WorkorderID left JOIN dbo.ESMDSMRSM 
            //                on TECH_HIERARCHY.BranchNumber COLLATE DATABASE_DEFAULT = ESMDSMRSM.BranchNO COLLATE DATABASE_DEFAULT
            //                Where(dbo.Workorder.WorkorderCallstatus <> 'Closed') AND (dbo.WorkorderSchedule.AssignedStatus = 'Accepted' OR dbo.WorkorderSchedule.AssignedStatus = 'Sent' OR dbo.WorkorderSchedule.AssignedStatus = 'Scheduled')
            //                And dbo.WorkorderSchedule.PrimaryTech = 1";

            string ssql = @"SELECT dbo.TECH_HIERARCHY.RegionNumber as Region, dbo.BranchESM.ESMName, --dbo.ESMDSMRSM.Region, dbo.ESMDSMRSM.ESMName, 
                                    dbo.WorkorderSchedule.TechId, dbo.TECH_HIERARCHY.CompanyName AS DispatchCompany,
                                    COUNT(dbo.WorkorderSchedule.TechId) AS EventCount 
                                    FROM dbo.WorkorderSchedule (nolock) 
                                    INNER JOIN dbo.TECH_HIERARCHY  (nolock) ON dbo.WorkorderSchedule.TechId = dbo.TECH_HIERARCHY.DealerId 
                                    INNER JOIN dbo.Workorder ON dbo.WorkorderSchedule.WorkorderID = dbo.Workorder.WorkorderID 
                                    INNER JOIN dbo.Contact ON dbo.Contact.ContactID = dbo.WorkOrder.CustomerID                               
                                    left join dbo.BranchESM on dbo.TECH_HIERARCHY.BranchNumber = dbo.BranchESM.BranchNo
                                    Where(dbo.Workorder.WorkorderCallstatus <> 'Closed') AND (dbo.WorkorderSchedule.AssignedStatus = 'Accepted' 
                                    OR dbo.WorkorderSchedule.AssignedStatus = 'Sent' OR dbo.WorkorderSchedule.AssignedStatus = 'Scheduled')
                                    And (dbo.WorkorderSchedule.PrimaryTech = 1 OR  dbo.WorkorderSchedule.PrimaryTech = 0)";


            ssql = ssql + " and WorkorderSchedule.ScheduleDate >='" + OpenCallByTechModel.OpenCallByTechFromDate + "'  ";
            ssql = ssql + " and  WorkorderSchedule.ScheduleDate <='" + OpenCallByTechModel.OpenCallByTechToDate + "' ";

            if (OpenCallByTechModel.DealerId > 0)
            {
                ssql = ssql + " and TECH_HIERARCHY.dealerid =" + OpenCallByTechModel.DealerId;
            }

            if (OpenCallByTechModel.TechID != "All")
            {
                ssql = ssql + " and TECH_HIERARCHY.familyAff='" + OpenCallByTechModel.TechID + "'";
            }


            //ssql = ssql + " GROUP BY  dbo.ESMDSMRSM.Region, dbo.ESMDSMRSM.ESMName,dbo.TECH_HIERARCHY.CompanyName,  dbo.WorkorderSchedule.TechId ORDER BY EventCount DESC";
            ssql = ssql + " GROUP BY  dbo.TECH_HIERARCHY.RegionNumber, dbo.BranchESM.ESMName,dbo.TECH_HIERARCHY.CompanyName,  dbo.WorkorderSchedule.TechId ORDER BY EventCount DESC";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<OpenCallByTechSearchResultModel> OpenCallByTechParts = new List<OpenCallByTechSearchResultModel>();
            OpenCallByTechSearchResultModel OpenCallByTechSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                OpenCallByTechSearchResultModel = new OpenCallByTechSearchResultModel();
                if (dr["Region"] != DBNull.Value)
                {
                    OpenCallByTechSearchResultModel.Region = dr["Region"].ToString();
                }
                if (dr["ESMName"] != DBNull.Value)
                {
                    OpenCallByTechSearchResultModel.ESMName = dr["ESMName"].ToString();
                }

                OpenCallByTechSearchResultModel.DispatchCompany = dr["DispatchCompany"].ToString();
                OpenCallByTechSearchResultModel.TechId = dr["TechID"].ToString();
                OpenCallByTechSearchResultModel.EventCount = dr["EventCount"].ToString();
                OpenCallByTechParts.Add(OpenCallByTechSearchResultModel);
            }
            return OpenCallByTechParts;
        }

        public JsonResult ClearOpenCallByTechResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new OpenCallByTechModel(), JsonRequestBehavior.AllowGet);
        }

        public void OpenCallByTechExcelExport()
        {
            OpenCallByTechModel OpenCallByTechModel = new OpenCallByTechModel();

            IList<OpenCallByTechSearchResultModel> searchResults = new List<OpenCallByTechSearchResultModel>();

            if (TempData["SearchCriteria"] != null)
            {
                OpenCallByTechModel = TempData["SearchCriteria"] as OpenCallByTechModel;
                searchResults = GetOpenCallByTech(OpenCallByTechModel);
            }

            TempData["SearchCriteria"] = OpenCallByTechModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "OpenCallByTechReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }


        public ActionResult OpenCallByTechnicianResultDataExport(string DateFrom, string DateTo, string TechId, string familyAff)
        {
            int techid = TechId == "Total" ? 0 : Convert.ToInt32(TechId);
            OpenCallByTechnicianResultData(DateFrom, DateTo, techid, familyAff);
            var fName = string.Format("Non-Service-Events-{0}.xlsx", DateTime.Now.ToString("s"));

            return Json(new { success = true, fName }, JsonRequestBehavior.AllowGet);
        }

        public FileResult DownloadingOpenCallByTechSpecificResultDataExport()
        {
            string[] columns = { "EventID", "CallTypeId","CallTypeDesc", "EventStatus", "EventScheduleDate", "EntryDate", "Region", "RegionName", "Branch", "BranchName", "CustomerType", "ContactID", "CompanyName", "Address1", "City"
                               , "State", "PostalCode", "DispatchCompany", "DispatchDate", "ElapsedTime", "FamilyAff", "TechId"};
            byte[] filecontent = ExcelExportHelper.ExportExcel(OpenCallByTechnicianResuls, "OpenCallDetailsbyTech", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "OpenCallDetailsbyTechWODetails.xlsx");
        }

        public void OpenCallByTechnicianResultData(string DateFrom, string DateTo, int TechId, string familyAff)
        {
            //string sqlQuery = @"SELECT        dbo.WorkOrder.WorkorderID, dbo.WorkOrder.WorkorderCalltypeid, dbo.WorkOrder.WorkorderCalltypeDesc, dbo.WorkOrder.WorkorderEntryDate, dbo.WorkorderSchedule.Techid, 
            //                                dbo.TECH_HIERARCHY.CompanyName AS DispatchCompany, dbo.WorkorderSchedule.ScheduleDate,  dbo.WorkorderSchedule.EventScheduleDate,
            //                                dbo.WorkorderSchedule.ScheduleUserid, 
            //                                dbo.getElapsedString(ISNULL(DATEDIFF(n, dbo.WorkorderSchedule.ScheduleDate, GETDATE()), 0)) AS ElapsedTime, 
            //                                ISNULL(DATEDIFF(n, dbo.WorkorderSchedule.ScheduleDate, 
            //                                GETDATE()), 0) AS ElapsedTimeInMins, 
            //                                dbo.WorkOrder.WorkorderCallstatus, dbo.Contact.CompanyName, dbo.Contact.Address1, dbo.Contact.City, dbo.Contact.State, 
            //                                dbo.Contact.PostalCode, dbo.Contact.ContactID, 
            //                                dbo.Contact.CustomerRegion, dbo.Contact.Branch AS CustomerBranchNo, dbo.Contact.RegionNumber, 
            //                                dbo.Contact.CustomerBranch, dbo.TECH_HIERARCHY.FamilyAff, dbo.Contact.SearchType, dbo.Contact.SearchDesc,
            //                                ESMDSMRSM.Region As RegionNo, ESMDSMRSM.ESMName as RegionNm
            //                                FROM dbo.WorkorderSchedule (nolock) 
            //                                INNER JOIN dbo.TECH_HIERARCHY  (nolock)  ON dbo.WorkorderSchedule.TechId = dbo.TECH_HIERARCHY.DealerId 
            //                                INNER JOIN dbo.Workorder ON dbo.WorkorderSchedule.WorkorderID = dbo.Workorder.WorkorderID 
            //                                INNER JOIN dbo.Contact ON dbo.WorkOrder.CustomerID = dbo.Contact.ContactID 
            //                                left JOIN dbo.ESMDSMRSM on TECH_HIERARCHY.BranchNumber COLLATE DATABASE_DEFAULT = ESMDSMRSM.BranchNO COLLATE DATABASE_DEFAULT
            //                                WHERE        (dbo.WorkOrder.WorkorderCallstatus <> N'Closed')
            //                                And (dbo.WorkorderSchedule.AssignedStatus = 'Accepted' OR dbo.WorkorderSchedule.AssignedStatus = 'Sent' OR dbo.WorkorderSchedule.AssignedStatus = 'Scheduled') And dbo.WorkorderSchedule.PrimaryTech = 1";

            string sqlQuery = @"SELECT  dbo.WorkOrder.WorkorderID, dbo.WorkOrder.WorkorderCalltypeid, dbo.WorkOrder.WorkorderCalltypeDesc, dbo.WorkOrder.WorkorderEntryDate, dbo.WorkorderSchedule.Techid, 
                                            dbo.TECH_HIERARCHY.CompanyName AS DispatchCompany, dbo.WorkorderSchedule.ScheduleDate,  dbo.WorkorderSchedule.EventScheduleDate,
                                            dbo.WorkorderSchedule.ScheduleUserid, 
                                            dbo.getElapsedString(ISNULL(DATEDIFF(n, dbo.WorkorderSchedule.ScheduleDate, GETDATE()), 0)) AS ElapsedTime, 
                                            ISNULL(DATEDIFF(n, dbo.WorkorderSchedule.ScheduleDate, 
                                            GETDATE()), 0) AS ElapsedTimeInMins, 
                                            dbo.WorkOrder.WorkorderCallstatus, dbo.Contact.CompanyName, dbo.Contact.Address1, dbo.Contact.City, dbo.Contact.State, 
                                            dbo.Contact.PostalCode, dbo.Contact.ContactID, 
                                            dbo.TECH_HIERARCHY.RegionName As CustomerRegion, dbo.TECH_HIERARCHY.BranchNumber AS CustomerBranchNo, dbo.Contact.RegionNumber As RegionNumber, 
                                            dbo.TECH_HIERARCHY.BranchName As CustomerBranch, dbo.TECH_HIERARCHY.FamilyAff, dbo.Contact.SearchType, dbo.Contact.SearchDesc,
                                            dbo.TECH_HIERARCHY.RegionNumber As RegionNo, dbo.TECH_HIERARCHY.FieldServiceManager as RegionNm
                                            FROM dbo.WorkorderSchedule (nolock) 
                                            INNER JOIN dbo.TECH_HIERARCHY  (nolock)  ON dbo.WorkorderSchedule.TechId = dbo.TECH_HIERARCHY.DealerId 
                                            INNER JOIN dbo.Workorder ON dbo.WorkorderSchedule.WorkorderID = dbo.Workorder.WorkorderID 
                                            INNER JOIN dbo.Contact ON dbo.WorkOrder.CustomerID = dbo.Contact.ContactID 
                                            left join dbo.BranchESM on dbo.TECH_HIERARCHY.BranchNumber = dbo.BranchESM.BranchNo
                                            WHERE        (dbo.WorkOrder.WorkorderCallstatus <> N'Closed')
                                            And (dbo.WorkorderSchedule.AssignedStatus = 'Accepted' OR dbo.WorkorderSchedule.AssignedStatus = 'Sent' 
                                            OR dbo.WorkorderSchedule.AssignedStatus = 'Scheduled') And (dbo.WorkorderSchedule.PrimaryTech = 1 OR  dbo.WorkorderSchedule.PrimaryTech = 0)";
                  
            StringBuilder str = new StringBuilder();
            str.Append(sqlQuery);
            if (TechId == 0)
            {
                if (familyAff == "SPT")
                {
                    //str.Append("Select * from V_OpenCallTime where FamilyAff ='SPT' And ScheduleDate >='");
                    str.Append(" And dbo.TECH_HIERARCHY.FamilyAff ='SPT' And dbo.WorkorderSchedule.ScheduleDate >='");
                    str.Append(DateFrom);
                    //str.Append("' and ScheduleDate <'");
                    str.Append("' and dbo.WorkorderSchedule.ScheduleDate <'");
                    str.Append(DateTo);
                    str.Append("'");
                }
                else if (familyAff == "SPD")
                {
                    //str.Append("Select * from V_OpenCallTime where FamilyAff <> 'SPT' And ScheduleDate >='");
                    str.Append(" And dbo.TECH_HIERARCHY.FamilyAff  <> 'SPT' And dbo.WorkorderSchedule.ScheduleDate >='");
                    str.Append(DateFrom);
                    //str.Append("' and ScheduleDate <'");
                    str.Append("' and dbo.WorkorderSchedule.ScheduleDate <'");
                    str.Append(DateTo);
                    str.Append("'");

                }
                else if (familyAff == "All")
                {
                    //str.Append("Select * from V_OpenCallTime where ScheduleDate >='");
                    str.Append(" AND dbo.WorkorderSchedule.ScheduleDate >='");
                    str.Append(DateFrom);
                    //str.Append("' and ScheduleDate <'");
                    str.Append("' and ScheduleDate <'");
                    str.Append(DateTo);
                    str.Append("'");
                }

            }
            else
            {
                if (familyAff == "SPT")
                {
                    //str.Append("Select * from V_OpenCallTime where FamilyAff ='SPT' And TechID = ");
                    str.Append(" And dbo.TECH_HIERARCHY.FamilyAff ='SPT' And dbo.TECH_HIERARCHY.DealerID = ");
                    str.Append(TechId);
                    //str.Append("And ScheduleDate >='");
                    str.Append(" And dbo.WorkorderSchedule.ScheduleDate >='");
                    str.Append(DateFrom);
                    //str.Append("' and ScheduleDate <'");
                    str.Append("' and dbo.WorkorderSchedule.ScheduleDate <'");
                    str.Append(DateTo);
                    str.Append("'");
                }
                else if (familyAff == "SPD")
                {
                    //str.Append("Select * from V_OpenCallTime where FamilyAff <> 'SPT' And TechID = ");
                    str.Append(" And dbo.TECH_HIERARCHY.FamilyAff <> 'SPT' And dbo.TECH_HIERARCHY.DealerID = ");
                    str.Append(TechId);
                    //str.Append("And ScheduleDate >='");
                    str.Append(" And dbo.WorkorderSchedule.ScheduleDate >='");
                    str.Append(DateFrom);
                    //str.Append("' and ScheduleDate <'");
                    str.Append("' and dbo.WorkorderSchedule.ScheduleDate <'");
                    str.Append(DateTo);
                    str.Append("'");

                }
                else if (familyAff == "All")
                {

                    //str.Append("Select * from V_OpenCallTime where TechID = ");
                    str.Append(" And dbo.TECH_HIERARCHY.DealerID = ");
                    str.Append(TechId);
                    //str.Append("And ScheduleDate >='");
                    str.Append(" And dbo.WorkorderSchedule.ScheduleDate >='");
                    str.Append(DateFrom);
                    //str.Append("' and ScheduleDate <'");
                    str.Append("' and dbo.WorkorderSchedule.ScheduleDate <'");
                    str.Append(DateTo);
                    str.Append("'");
                }
            }

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetDatatable(str.ToString());
            OpenCallByTechnicianResuls = new List<OpenCallByTechSearchResultModel>();
            foreach (DataRow dr in dt.Rows)
            {
                OpenCallByTechSearchResultModel openCallTech = new OpenCallByTechSearchResultModel(dr);
                OpenCallByTechnicianResuls.Add(openCallTech);
            }
        }
        #endregion

        #region NonServiceReport

        public ActionResult NonServiceReport(int? isBack)
        {
            NonServiceSearchModel nonservicerpt = new NonServiceSearchModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                nonservicerpt = TempData["SearchCriteria"] as NonServiceSearchModel;
                TempData["SearchCriteria"] = nonservicerpt;
            }
            else
            {
                nonservicerpt = new NonServiceSearchModel();
                TempData["SearchCriteria"] = null;
            }
            nonservicerpt.SearchResults = new List<NonServiceSearchResults>();
            return View(nonservicerpt);

        }

        public JsonResult nonServiceEventSearch(NonServiceSearchModel nonServiceEvntSearch)
        {
            List<NonServiceSearchResults> unknownCustresults = new List<NonServiceSearchResults>();
            if (string.IsNullOrWhiteSpace(nonServiceEvntSearch.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(nonServiceEvntSearch.DateTo.ToString())
                )
            {
                TempData["SearchCriteria"] = null;
                return Json(new NonServiceSearchModel(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<NonServiceSearchResults> NonServiceSerchResults = GetNonServiceEventResults(nonServiceEvntSearch);
                nonServiceEvntSearch.SearchResults = NonServiceSerchResults;
                TempData["SearchCriteria"] = nonServiceEvntSearch;
                return Json(nonServiceEvntSearch, JsonRequestBehavior.AllowGet);
            }
        }


        private IList<NonServiceSearchResults> GetNonServiceEventResults(NonServiceSearchModel nonServiceEventModel)
        {
            DateTime startdate = Convert.ToDateTime(nonServiceEventModel.DateFrom);
            DateTime enddate = Convert.ToDateTime(nonServiceEventModel.DateTo);
            enddate.AddDays(1);

            string fromdate = startdate.ToString("MM/dd/yyyy");
            string todate = enddate.ToString("MM/dd/yyyy");


            List<NonServiceSearchResults> nonServiceEventResultsList = new List<NonServiceSearchResults>();
            string sSql = " SELECT FBCallReason.Description, COUNT(*)";
            sSql = sSql + " FROM NonServiceworkorder   (nolock) ";
            sSql = sSql + " INNER JOIN FBCallReason   (nolock) ON NonServiceworkorder.CallReason = FBCallReason.SourceCode";
            sSql = sSql + " WHERE NonServiceworkorder.CreatedDate >= '" + nonServiceEventModel.DateFrom + "' and NonServiceworkorder.CreatedDate < '" + nonServiceEventModel.DateTo + "'";
            sSql = sSql + " GROUP BY FBCallReason.Description";
            sSql = sSql + " ORDER BY FBCallReason.Description";

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(sSql);
            NonServiceSearchResults nonServiceSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                nonServiceSearchResultModel = new NonServiceSearchResults();
                nonServiceSearchResultModel.ServiceType = dr["Description"].ToString();
                nonServiceSearchResultModel.Count = dr["COLUMN1"].ToString();
                nonServiceEventResultsList.Add(nonServiceSearchResultModel);
            }

            NonServiceSearchResults serviceCallResult = nonServiceEventResultsList.Where(ns => ns.ServiceType.ToUpper() == "SERVICE CALL").FirstOrDefault();
            if (serviceCallResult != null)
            {
                NonServiceSearchResults miscilineaousResult = nonServiceEventResultsList.Where(ns => ns.ServiceType.ToUpper() == "MISCELLANEOUS").FirstOrDefault();
                if (miscilineaousResult != null)
                {
                    miscilineaousResult.Count = (Convert.ToInt32(miscilineaousResult.Count) + Convert.ToInt32(serviceCallResult.Count)).ToString();
                }
                else
                {
                    nonServiceSearchResultModel = new NonServiceSearchResults();
                    nonServiceSearchResultModel.ServiceType = "Miscellaneous";
                    nonServiceSearchResultModel.Count = serviceCallResult.Count;
                    nonServiceEventResultsList.Add(nonServiceSearchResultModel);
                }
            }

            nonServiceEventResultsList.RemoveAll((x) => x.ServiceType.ToUpper() == "SERVICE CALL");

            Double TotalEvents = nonServiceEventResultsList.Sum(item => Convert.ToDouble(item.Count));
            nonServiceSearchResultModel = new NonServiceSearchResults();
            nonServiceSearchResultModel.ServiceType = "All";
            nonServiceSearchResultModel.Count = TotalEvents.ToString();
            nonServiceEventResultsList.Add(nonServiceSearchResultModel);

            nonServiceEventModel.SearchResults = nonServiceEventResultsList;
            return nonServiceEventResultsList;
        }

        public JsonResult ClearNonSearchResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new NonServiceSearchModel(), JsonRequestBehavior.AllowGet);
        }

        public void NonServiceEventReportExcelExport()
        {
            NonServiceSearchModel nonServiceEventreports = new NonServiceSearchModel();

            IList<NonServiceSearchResults> searchResults = new List<NonServiceSearchResults>();

            if (TempData["SearchCriteria"] != null)
            {
                nonServiceEventreports = TempData["SearchCriteria"] as NonServiceSearchModel;
                searchResults = GetNonServiceEventResults(nonServiceEventreports);
            }

            TempData["SearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "NonServiceEventsReport.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        public ActionResult NonServiceSpecificResultDataExport(string DateFrom, string DateTo, string description)
        {
            serialNumbersearchResults = GetNonServiceSpecificDataResults(DateFrom, DateTo, description);
            var fName = string.Format("Non-Service-Events-{0}.xlsx", DateTime.Now.ToString("s"));

            return Json(new { success = true, fName }, JsonRequestBehavior.AllowGet);
        }


        public FileResult DownloadingNonServiceSpecificResultDataExport()
        {
            //string[] columns = { "ServiceType", "Count", "EventID", "CustomerType", "Description", "EventStatus", "EntryDate", "CompanyName", "Address1", "City", "State", "PostalCode"
            //                   , "EmailAddress", "Notes", "Route"};
            string[] columns = {  "EventID", "CustomerId", "Region", "Branch", "Route", "Description", "EventStatus", "EntryDate", "ClosureDate", "CompanyName", "Address1", "City", "State", "PostalCode", "EmailSentTo", "CreatedUserName" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(serialNumbersearchResults, "Customer-Service Event Details", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "Customer-Service-Events.xlsx");
        }

        private List<NonServiceSearchResults> GetNonServiceSpecificDataResults(string DateFrom, string DateTo, string description)
        {
            List<NonServiceSearchResults> nonServiceEventResultsList = new List<NonServiceSearchResults>();
            string sSql = @" SELECT dbo.FBCallReason.Description, dbo.NonServiceworkorder.WorkOrderID,dbo.NonServiceworkorder.CreatedDate,
                             dbo.Contact.CompanyName, dbo.Contact.ContactID, dbo.Contact.RegionNumber,dbo.Contact.Branch,dbo.Contact.Route,
                            dbo.Contact.Address1, dbo.Contact.City, dbo.Contact.State, dbo.Contact.PostalCode, dbo.NonServiceworkorder.NonServiceEventStatus, 
                            dbo.NonServiceworkorder.EmailSentTo,dbo.NonServiceworkorder.CloseDate, dbo.FbUserMaster.FirstName, dbo.FbUserMaster.LastName
                             --,STUFF((SELECT CHAR(13) + CHAR(10), dbo.NotesHistory.Notes as [text()]
                                --,STUFF((SELECT '; ', dbo.NotesHistory.Notes as [text()]
                                --FROM dbo.NotesHistory   (nolock) 
                                --WHERE dbo.NotesHistory.NonServiceWorkorderID = dbo.NonServiceworkorder.WorkOrderID
                                --FOR XML PATH('')),1,1,'') [Notes]
                                    FROM dbo.NonServiceworkorder
                             INNER JOIN dbo.Contact   (nolock) ON dbo.Contact.ContactID = dbo.NonServiceworkorder.CustomerID
                             LEFT JOIN dbo.NotesHistory  (nolock)  ON dbo.NotesHistory.NonServiceWorkorderID = dbo.NonServiceworkorder.WorkOrderID
                             INNER JOIN dbo.FBCallReason  (nolock) ON dbo.NonServiceworkorder.CallReason= dbo.FBCallReason.SourceCode 
                            INNER JOIN dbo.FbUserMaster (nolock) ON dbo.NonServiceworkorder.CreatedBy = dbo.FbUserMaster.UserId
                            WHERE";

            if (description.ToUpper() != "ALL")
            {
                if (description.ToUpper() == "MISCELLANEOUS")
                {
                    sSql = sSql + " (dbo.FBCallReason.Description = '" + description + "'  OR dbo.FBCallReason.Description = 'Service Call') and ";
                }
                else
                {
                    sSql = sSql + " dbo.FBCallReason.Description = '" + description + "' and ";
                }
            }

            sSql = sSql + " dbo.NonServiceworkorder.CreatedDate >= '" + DateFrom + "'";
            sSql = sSql + " and dbo.NonServiceworkorder.CreatedDate <= '" + DateTo + "'";
            sSql = sSql + " GROUP BY dbo.FBCallReason.Description, dbo.NonServiceworkorder.WorkOrderID,dbo.NonServiceworkorder.CreatedDate,";
            sSql = sSql + " dbo.Contact.CompanyName, dbo.Contact.ContactID, dbo.Contact.RegionNumber,dbo.Contact.Branch,dbo.Contact.Route, dbo.Contact.Address1," +
                                    " dbo.Contact.City, dbo.Contact.State, dbo.Contact.PostalCode,dbo.NonServiceworkorder.NonServiceEventStatus, dbo.NonServiceworkorder.EmailSentTo,dbo.NonServiceworkorder.CloseDate," +
                                    "dbo.FbUserMaster.FirstName, dbo.FbUserMaster.LastName";

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(sSql);
            NonServiceSearchResults nonServiceSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                nonServiceSearchResultModel = new NonServiceSearchResults();
                nonServiceSearchResultModel.Description = dr["Description"].ToString();
                nonServiceSearchResultModel.EventID = dr["WorkOrderID"].ToString();
                nonServiceSearchResultModel.EventStatus = dr["NonServiceEventStatus"].ToString();// "Open";
                nonServiceSearchResultModel.EntryDate = dr["CreatedDate"].ToString();
                nonServiceSearchResultModel.ClosureDate = dr["CloseDate"].ToString();
                nonServiceSearchResultModel.CompanyName = dr["CompanyName"].ToString();
                nonServiceSearchResultModel.CustomerId = dr["ContactID"].ToString();
                nonServiceSearchResultModel.Region = dr["RegionNumber"].ToString();
                nonServiceSearchResultModel.Branch = dr["Branch"].ToString();
                nonServiceSearchResultModel.Route = dr["Route"].ToString();
                nonServiceSearchResultModel.Address1 = dr["Address1"].ToString();
                nonServiceSearchResultModel.City = dr["City"].ToString();
                nonServiceSearchResultModel.State = dr["State"].ToString();
                nonServiceSearchResultModel.PostalCode = dr["PostalCode"].ToString();
                //nonServiceSearchResultModel.Notes = dr["Notes"].ToString();
                nonServiceSearchResultModel.EmailSentTo = dr["EmailSentTo"] == DBNull.Value ? "" : dr["EmailSentTo"].ToString();
                nonServiceSearchResultModel.CreatedUserName = ((dr["FirstName"] == DBNull.Value ? "" : dr["FirstName"].ToString()) + " " + (dr["LastName"] == DBNull.Value ? "" : dr["LastName"].ToString())).Trim();
                nonServiceEventResultsList.Add(nonServiceSearchResultModel);               
            }

            return nonServiceEventResultsList;
        }

        #endregion

        
        #region FB Dispatch Reprot 
        public ActionResult FBDispatchReport(int? isBack)
        {
            FBDispatchReportModel FBDispatchrpt = new FBDispatchReportModel();

            if (TempData["FBDispatchReportSearchCriteria"] != null && isBack == 1)
            {
                FBDispatchrpt = TempData["FBDispatchReportSearchCriteria"] as FBDispatchReportModel;
                TempData["FBDispatchReportSearchCriteria"] = FBDispatchrpt;
            }
            else
            {
                FBDispatchrpt = new FBDispatchReportModel();
                TempData["FBDispatchReportSearchCriteria"] = null;
            }
            FBDispatchrpt.SearchResults = new List<FBDispatchReportSearchResults>();
            return View(FBDispatchrpt);

        }

        public JsonResult FBDispatchReportSearch(FBDispatchReportModel FBDispatchReportSearch)
        {
            List<FBDispatchReportSearchResults> FBDispatchCustresults = new List<FBDispatchReportSearchResults>();
            if (string.IsNullOrWhiteSpace(FBDispatchReportSearch.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(FBDispatchReportSearch.DateTo.ToString())
                )
            {
                TempData["FBDispatchReportSearchCriteria"] = null;
                return Json(new FBDispatchLogModel(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<FBDispatchReportSearchResults> FBDispatchReportSerchResults = GetFBDispatchReportResults(FBDispatchReportSearch);
                FBDispatchReportSearch.SearchResults = FBDispatchReportSerchResults;
                TempData["FBDispatchReportSearchCriteria"] = FBDispatchReportSearch;
                return Json(FBDispatchReportSearch.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }


        private IList<FBDispatchReportSearchResults> GetFBDispatchReportResults(FBDispatchReportModel FBDispatchModel)
        {
            DateTime startdate = Convert.ToDateTime(FBDispatchModel.DateFrom);
            DateTime enddate = Convert.ToDateTime(FBDispatchModel.DateTo);
            enddate.AddDays(1);
            string fromdate = startdate.ToString("MM/dd/yyyy");
            string todate = enddate.ToString("MM/dd/yyyy");

            List<FBDispatchReportSearchResults> FBDispatchEventResultsList = new List<FBDispatchReportSearchResults>();

            string query = @"
                    select * from (select  w.WorkorderID,w.WorkorderEntryDate,n.EntryDate,u.Company,u.FirstName+' '+u.LastName as UserName,w.WorkorderCalltypeDesc,
                    ROW_NUMBER() OVER(Partition by w.WorkorderID ORDER BY n.EntryDate) AS RowNumber
                    from workorder w with (nolock)
                    inner join NotesHistory n with (nolock) on w.WorkorderID = n.WorkorderID
                    left join fbusermaster u with (nolock) on n.userid = u.userid
                    where (n.Notes like 'Work order Accepted by%' or n.Notes like 'Moved work order to be Accepted by%') and
                    w.WorkorderEntryDate >= '" + FBDispatchModel.DateFrom + "' and w.WorkorderEntryDate < '" + FBDispatchModel.DateTo + "') as innerTable where RowNumber =1";
            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetDatatable(query);
            FBDispatchReportSearchResults FBDispatchSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                FBDispatchSearchResultModel = new FBDispatchReportSearchResults();
                FBDispatchSearchResultModel.WorkorderID = dr["WorkorderID"].ToString();

                TimeSpan acceptTimeDiff = Convert.ToDateTime(dr["EntryDate"].ToString()).Subtract(Convert.ToDateTime(dr["WorkorderEntryDate"].ToString()));
                FBDispatchSearchResultModel.TimeElapsed = WorkorderSearchResultModel.convertTimeSpanToDateTimeStringFormatReport(acceptTimeDiff);

                if (dr["Company"].ToString().Equals("Marketing Alternatives") || dr["Company"].ToString().Equals("MAI"))
                {
                    FBDispatchSearchResultModel.UserName = dr["UserName"].ToString();
                }
                else
                {
                    FBDispatchSearchResultModel.UserName = "Auto Dispatch";
                }

                FBDispatchSearchResultModel.WorkOrderCallType = dr["WorkorderCalltypeDesc"].ToString();
                FBDispatchEventResultsList.Add(FBDispatchSearchResultModel);
            }

            FBDispatchModel.SearchResults = FBDispatchEventResultsList;
            return FBDispatchEventResultsList;
        }


        public void FBDispatchExcelExport()
        {
            FBDispatchReportModel FBDispatchreports = new FBDispatchReportModel();

            IList<FBDispatchReportSearchResults> searchResults = new List<FBDispatchReportSearchResults>();

            if (TempData["FBDispatchReportSearchCriteria"] != null)
            {
                FBDispatchreports = TempData["FBDispatchReportSearchCriteria"] as FBDispatchReportModel;
                searchResults = GetFBDispatchReportResults(FBDispatchreports);
            }

            TempData["FBDispatchReportSearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "FBDispatchReport.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        #endregion
        


        #region FBDispatchLog

        public ActionResult FBDispatchLog(int? isBack)
        {
            FBDispatchLogModel FBDispatchrpt = new FBDispatchLogModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                FBDispatchrpt = TempData["SearchCriteria"] as FBDispatchLogModel;
                TempData["SearchCriteria"] = FBDispatchrpt;
            }
            else
            {
                FBDispatchrpt = new FBDispatchLogModel();
                TempData["SearchCriteria"] = null;
            }
            FBDispatchrpt.SearchResults = new List<FBDispatchSearchResults>();
            return View(FBDispatchrpt);

        }

        public JsonResult FBDispatchSearch(FBDispatchLogModel FBDispatchlogSearch)
        {
            List<FBDispatchSearchResults> FBDispatchCustresults = new List<FBDispatchSearchResults>();
            if (string.IsNullOrWhiteSpace(FBDispatchlogSearch.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(FBDispatchlogSearch.DateTo.ToString())
                )
            {
                TempData["SearchCriteria"] = null;
                return Json(new FBDispatchLogModel(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<FBDispatchSearchResults> FBDispatchLogSerchResults = GetFBDispatchLogResults(FBDispatchlogSearch);
                FBDispatchlogSearch.SearchResults = FBDispatchLogSerchResults;
                TempData["SearchCriteria"] = FBDispatchlogSearch;
                return Json(FBDispatchlogSearch.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }


        private IList<FBDispatchSearchResults> GetFBDispatchLogResults(FBDispatchLogModel FBDispatchModel)
        {
            DateTime startdate = Convert.ToDateTime(FBDispatchModel.DateFrom);
            DateTime enddate = Convert.ToDateTime(FBDispatchModel.DateTo);
            enddate.AddDays(1);

            string fromdate = startdate.ToString("MM/dd/yyyy");
            string todate = enddate.ToString("MM/dd/yyyy");


            List<FBDispatchSearchResults> FBDispatchEventResultsList = new List<FBDispatchSearchResults>();
            string sSql = " SELECT * from AgentDispatchLog (nolock) ";
            sSql = sSql + " WHERE tdate >= '" + FBDispatchModel.DateFrom + "' and tdate < '" + FBDispatchModel.DateTo + "'";
            sSql = sSql + " ORDER BY tdate asc";

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(sSql);
            FBDispatchSearchResults FBDispatchSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                FBDispatchSearchResultModel = new FBDispatchSearchResults();
                FBDispatchSearchResultModel.TDate = dr["TDate"].ToString();
                FBDispatchSearchResultModel.WorkorderID = dr["WorkorderID"].ToString();
                FBDispatchSearchResultModel.UserName = dr["UserName"].ToString();
                FBDispatchEventResultsList.Add(FBDispatchSearchResultModel);
            }

            FBDispatchModel.SearchResults = FBDispatchEventResultsList;
            return FBDispatchEventResultsList;
        }

        public JsonResult ClearFBDispatchResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new FBDispatchLogModel(), JsonRequestBehavior.AllowGet);
        }

        public void FBDispatchReportExcelExport()
        {
            FBDispatchLogModel FBDispatchreports = new FBDispatchLogModel();

            IList<FBDispatchSearchResults> searchResults = new List<FBDispatchSearchResults>();

            if (TempData["SearchCriteria"] != null)
            {
                FBDispatchreports = TempData["SearchCriteria"] as FBDispatchLogModel;
                searchResults = GetFBDispatchLogResults(FBDispatchreports);
            }

            TempData["SearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "FBDispatchLog.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }
        #endregion


        #region FBDispatchAcceptedReport

        public ActionResult FBDispatchAcceptedReport(int? isBack)
        {
            FBDispatchAcceptedModel FBDispatchrpt = new FBDispatchAcceptedModel();

            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                FBDispatchrpt = TempData["SearchCriteria"] as FBDispatchAcceptedModel;
                TempData["SearchCriteria"] = FBDispatchrpt;
            }
            else
            {
                FBDispatchrpt = new FBDispatchAcceptedModel();
                TempData["SearchCriteria"] = null;
            }
            FBDispatchrpt.SearchResults = new List<FBDispatchAcceptedSearchResults>();
            return View(FBDispatchrpt);

        }

        public JsonResult FBDispatchAcceptedReportSearch(FBDispatchAcceptedModel FBDispatchAcceptedSearch)
        {
            List<FBDispatchAcceptedSearchResults> FBDispatchCustresults = new List<FBDispatchAcceptedSearchResults>();
            if (string.IsNullOrWhiteSpace(FBDispatchAcceptedSearch.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(FBDispatchAcceptedSearch.DateTo.ToString())
                )
            {
                TempData["SearchCriteria"] = null;
                return Json(new FBDispatchAcceptedModel(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<FBDispatchAcceptedSearchResults> FBDispatchAcceptedSerchResults = GetFBDispatchAcceptedReportResults(FBDispatchAcceptedSearch);
                FBDispatchAcceptedSearch.SearchResults = FBDispatchAcceptedSerchResults;
                TempData["SearchCriteria"] = FBDispatchAcceptedSearch;
                return Json(FBDispatchAcceptedSearch.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }


        private IList<FBDispatchAcceptedSearchResults> GetFBDispatchAcceptedReportResults(FBDispatchAcceptedModel FBDispatchModel)
        {
            DateTime startdate = Convert.ToDateTime(FBDispatchModel.DateFrom);
            DateTime enddate = Convert.ToDateTime(FBDispatchModel.DateTo);
            enddate.AddDays(1);

            string fromdate = startdate.ToString("MM/dd/yyyy");
            string todate = enddate.ToString("MM/dd/yyyy");


            List<FBDispatchAcceptedSearchResults> FBDispatchEventResultsList = new List<FBDispatchAcceptedSearchResults>();
            string sSql = " SELECT * from AgentDispatchLog ad (nolock) ";
            sSql = sSql + "inner join WorkorderStatusLog wsl on ad.WorkorderID = wsl.WorkorderID";
            sSql = sSql + " WHERE wsl.StatusFrom = 'Pending Acceptance' and wsl.StatusTo = 'Accepted'";
            sSql = sSql + " And tdate >= '" + FBDispatchModel.DateFrom + "' and tdate < '" + FBDispatchModel.DateTo + "'";
            sSql = sSql + " ORDER BY tdate asc";

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(sSql);
            FBDispatchAcceptedSearchResults FBDispatchSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                FBDispatchSearchResultModel = new FBDispatchAcceptedSearchResults();
                FBDispatchSearchResultModel.TDate = dr["TDate"].ToString();
                FBDispatchSearchResultModel.WorkorderID = dr["WorkorderID"].ToString();
                FBDispatchSearchResultModel.UserName = dr["UserName"].ToString();
                FBDispatchSearchResultModel.statusFrom = dr["StatusFrom"].ToString();
                FBDispatchSearchResultModel.statusTo = dr["StatusTo"].ToString();
                FBDispatchSearchResultModel.AcceptedDate = dr["StausChangeDate"].ToString();
                FBDispatchEventResultsList.Add(FBDispatchSearchResultModel);
            }

            FBDispatchModel.SearchResults = FBDispatchEventResultsList;
            return FBDispatchEventResultsList;
        }

        public JsonResult ClearFBDispatchAcceptedReportResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new FBDispatchAcceptedModel(), JsonRequestBehavior.AllowGet);
        }

        public void FBDispatchAcceptedReportExcelExport()
        {
            FBDispatchAcceptedModel FBDispatchreports = new FBDispatchAcceptedModel();

            IList<FBDispatchAcceptedSearchResults> searchResults = new List<FBDispatchAcceptedSearchResults>();

            if (TempData["SearchCriteria"] != null)
            {
                FBDispatchreports = TempData["SearchCriteria"] as FBDispatchAcceptedModel;
                searchResults = GetFBDispatchAcceptedReportResults(FBDispatchreports);
            }

            TempData["SearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "FBDispatchAcceptedReport.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }
        #endregion

        [HttpGet]
        public ActionResult AllReports()
        {
            List<Report> reports = new List<Report>();
            var reportTypes = FarmerBrothersEntitites.FBReports.Select(c => c.report_category).Distinct().ToList();
            Report rep;
            foreach (var report in reportTypes)
            {
                rep = new Report();
                rep.ReportType = report;
                rep.ReportNames = FarmerBrothersEntitites.FBReports.Where(r => r.report_category == report && r.Active == 1).Select(c => c.report_name).ToList();
                reports.Add(rep);
            }
            ReportsModel rm = new ReportsModel();
            rm.Reports = reports;
            return View(rm);
        }
        [HttpGet]
        public ActionResult ProductivityReportBytechnician()
        {
            TechReportsModel techReportsModel = new TechReportsModel();

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            techReportsModel.Techlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                tech.TechID = dr[0].ToString();
                if (dr[0].ToString() == "SPD")
                {
                    tech.TechName = "Internal";
                    TechnicianAffs.Add(tech);
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                    TechnicianAffs.Add(tech);
                }


            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            techReportsModel.TechnicianAffs = TechnicianAffs;
            return View("TechnicianProductivityReport", techReportsModel);
        }

        [HttpGet]
        public JsonResult TechnicianByFamilyAff(string FamilyAff)
        {
            TechReportsModel techReportsModel = new TechReportsModel();

            IEnumerable<TechHierarchyView> Techlist = null;
            switch (FamilyAff)
            {
                case "All":
                    Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);
                    break;
                case "SPD":
                    Techlist = Utility.GetInternalTechData(FarmerBrothersEntitites);
                    break;
                case "SPT":
                    Techlist = Utility.Get3rdParyTechData(FarmerBrothersEntitites);
                    break;

            }


            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            techReportsModel.Techlist = newTechlistCollection;

            return Json(techReportsModel.Techlist, JsonRequestBehavior.AllowGet);
            //return View("TechnicianProductivityReport", techReportsModel);
        }

        public FileResult ExportToExcel()
        {
            WorkorderSearchModel workOrderSearchModel = new WorkorderSearchModel();
            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();

            string gridModel = HttpContext.Request.Params["GridModel"];
            DataTable dt = new DataTable();
            if (TempData["WorkOrderSearchCriteria"] != null)
            {
                workOrderSearchModel = TempData["WorkOrderSearchCriteria"] as WorkorderSearchModel;
                dt = GetWorkOrderDataTable(workOrderSearchModel);
            }

            foreach (DataRow dr in dt.Rows)
            {
                if (dr["ERFNO"] != DBNull.Value)
                {
                    if (dr["ERFScheduleDate"] != DBNull.Value)
                    {
                        dr["AppointmentDate"] = dr["ERFScheduleDate"];
                    }
                }
            }

            TempData["WorkOrderSearchCriteria"] = workOrderSearchModel;
            //string[] columns = {"EventID","ContactID","FulfillmentStatus","EntryDate","CloseDate","MaxOfStartDateTime","MaxOfArrivalDateTime","MaxOfCompletionDateTime","NoService","EventCallTypeID",
            //                    "CallTypeDesc","CompanyName","Address1","City","State","PostalCode","FieldServiceManager","FSMJDE","PricingParentName","DeliveryDesc","ERFNO","TechId","RepeatCallEvent",
            //                    "EquipCount","DealerCompany","DealerCity","DealerState","CallTypeID","SymptomID","SolutionId","SystemId","SerialNo","ProductNo","Manufacturer","ManufacturerDesc",
            //                    "AppointmentDate","CategoryDesc","InvoiceNo","FamilyAff","ADT","ONSite","CustomerRegion","BranchName","RegionNumber","CustomerBranch","Branch","ContactSearchType",
            //                    "ContactSearchDesc","RouteNumber","PPID","PPIDDesc","PricingParentID","IsBillable","TotalUnitPrice","ServicePriority" };

            string[] columns = {"EventID","ContactID","FulfillmentStatus","EntryDate","CloseDate","MaxOfStartDateTime","MaxOfArrivalDateTime","MaxOfCompletionDateTime","NoService","EventCallTypeID",
                                "CallTypeDesc","CompanyName","Address1","City","State","PostalCode","FieldServiceManager","FSMJDE","PricingParentName","DeliveryDesc","ERFNO","TechId","RepeatCallEvent","RepeatRepair",
                                "EquipCount","DealerCompany","DealerCity","DealerState","CallTypeID","SymptomID","SolutionId","SystemId","SerialNo","ProductNo","Manufacturer","ManufacturerDesc",
                                "AppointmentDate","CategoryDesc","InvoiceNo","FamilyAff","ADT","ONSite","CustomerRegion","BranchName","RegionNumber","CustomerBranch","Branch","ContactSearchType",
                                "ContactSearchDesc","RouteNumber","PPID","PPIDDesc","PricingParentID","EventScheduleDate","WaterTested","HardnessRating","FilterReplaced","FilterReplacedDate","NextFilterReplacementDate","ServicePriority","RescheduleReason" };

            byte[] filecontent = ExcelExportHelper.ExportExcel(dt, "", false, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "ProgramStatusResults.xlsx");
        }


        [HttpPost]
        public void ExcelExportProgramStatusResults()
        {
            WorkorderSearchModel workOrderSearchModel = new WorkorderSearchModel();
            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();

            string gridModel = HttpContext.Request.Params["GridModel"];

            if (TempData["WorkOrderSearchCriteria"] != null)
            {
                workOrderSearchModel = TempData["WorkOrderSearchCriteria"] as WorkorderSearchModel;
                searchResults = GetWorkOrderDataFromSP(workOrderSearchModel);
            }

            TempData["WorkOrderSearchCriteria"] = workOrderSearchModel;

            ExcelExport exp = new ExcelExport();
            var DataSource = searchResults;
            GridProperties obj = ConvertGridObject(gridModel);
            exp.Export(obj, searchResults, "ProgramStatusResults.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");

        }

        [HttpGet]
        public ActionResult ProgramStatusReportLive(int? isBack)
        {
            WorkorderSearchModel workOrderSearchModel;

            if (TempData["WorkOrderSearchCriteria"] != null && isBack == 1)
            {
                workOrderSearchModel = TempData["WorkOrderSearchCriteria"] as WorkorderSearchModel;
                TempData["WorkOrderSearchCriteria"] = workOrderSearchModel;
            }
            else
            {
                workOrderSearchModel = new WorkorderSearchModel();
                TempData["WorkOrderSearchCriteria"] = null;
            }

            workOrderSearchModel = PopulateWorkOrderSearchModelLists(workOrderSearchModel);
            workOrderSearchModel.State = new List<string>();
            workOrderSearchModel.Priority = new List<string>();
            workOrderSearchModel.WorkorderType = new List<string>();
            workOrderSearchModel.WOTypes = new List<string>();
            workOrderSearchModel.Status = new List<string>();
            workOrderSearchModel.FollowupCall = defaultFollowUpCall;
            workOrderSearchModel.Status.Add("");
            workOrderSearchModel.WOTypes.Add("");
            workOrderSearchModel.State.Add("");
            workOrderSearchModel.WorkorderType.Add("");
            workOrderSearchModel.Priority.Add("");

            return View("ReportsSearch", workOrderSearchModel);
        }

        public JsonResult Search(TechReportsModel techReportsModel)
        {
            if (string.IsNullOrWhiteSpace(techReportsModel.TechID)
                && techReportsModel.DealerId > 0)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<TechnicianSearchResultModel>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<TechnicianSearchResultModel> techncians = GetTechncians(techReportsModel);
                TempData["SearchCriteria"] = techReportsModel;
                return Json(techReportsModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<TechnicianSearchResultModel> GetTechncians(TechReportsModel techReportsModel)
        {
            List<TechnicianSearchResultModel> techResults = new List<TechnicianSearchResultModel>();
            string sSql = " SELECT TOP 100 PERCENT dbo.V_UniqueInvoiceTimingsByDealerID.FamilyAff,dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId, dbo.V_UniqueInvoiceTimingsByDealerID.Region,dbo.V_UniqueInvoiceTimingsByDealerID.RegionNumber,dbo.V_UniqueInvoiceTimingsByDealerID.ESM, dbo.Contact.Route,";
            sSql = sSql + " dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechName, dbo.V_UniqueInvoiceTimingsByDealerID.BranchNumber,dbo.V_UniqueInvoiceTimingsByDealerID.BranchName, SUM(ISNULL(DATEDIFF(n, dbo.V_UniqueInvoiceTimingsByDealerID.StartDateTime,";
            sSql = sSql + " dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0)) AS TotalTimeByTech, COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId) AS ToatlEventsByTech,";
            sSql = sSql + " SUM(ISNULL(DATEDIFF(n,dbo.V_UniqueInvoiceTimingsByDealerID.StartDateTime, dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0)) / COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId) AS AvgMinsPerCall, ";
            sSql = sSql + " SUM(ISNULL(DATEDIFF(n,dbo.V_UniqueInvoiceTimingsByDealerID.ArrivalDateTime, dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0)) / COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId) AS AvgMinsOnsitePerCall, ";
            sSql = sSql + " dbo.getElapsedString(SUM(ISNULL(DATEDIFF(n, dbo.V_UniqueInvoiceTimingsByDealerID.ArrivalDateTime, dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0))/ COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId)) AS ElapsedTimeOnSite ,";
            sSql = sSql + " dbo.getElapsedString(SUM(ISNULL(DATEDIFF(n, dbo.V_UniqueInvoiceTimingsByDealerID.StartDateTime, dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0))/ COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId)) AS ElapsedTime FROM dbo.Workorder  (nolock)  INNER JOIN";
            sSql = sSql + " dbo.V_UniqueInvoiceTimingsByDealerID  (nolock) ON dbo.Workorder.WorkorderId = dbo.V_UniqueInvoiceTimingsByDealerID.WorkorderId ";
            sSql = sSql + "  INNER JOIN Contact (nolock) ON dbo.Workorder.CustomerId = dbo.Contact.ContactId ";
            sSql = sSql + "   WHERE     (dbo.Workorder.WorkorderCallstatus = N'Closed') AND ";

            sSql = sSql + "  dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime >='" + techReportsModel.DateFrom + "' AND ";
            sSql = sSql + " dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime <'" + techReportsModel.DateTo + "' ";

            if (techReportsModel.TechID != "All")
            {
                sSql = sSql + " and dbo.V_UniqueInvoiceTimingsByDealerID.FamilyAff='" + techReportsModel.TechID + "'";
            }

            if (techReportsModel.DealerId > 0)
            {
                sSql = sSql + " and dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId=" + techReportsModel.DealerId;
            }


            sSql = sSql + " GROUP BY dbo.V_UniqueInvoiceTimingsByDealerID.FamilyAff,  Region,ESM,dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechName, dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId,dbo.V_UniqueInvoiceTimingsByDealerID.BranchNumber,dbo.V_UniqueInvoiceTimingsByDealerID.BranchName,dbo.V_UniqueInvoiceTimingsByDealerID.RegionNumber,dbo.Contact.Route ORDER BY ElapsedTime DESC";

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(sSql);
            TechnicianSearchResultModel closurePartsSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                closurePartsSearchResultModel = new TechnicianSearchResultModel();
                closurePartsSearchResultModel.Region = dr["RegionNumber"].ToString();
                closurePartsSearchResultModel.BranchNumber = dr["BranchNumber"].ToString();
                closurePartsSearchResultModel.Route = dr["Route"].ToString();
                closurePartsSearchResultModel.ESM = dr["ESM"].ToString();
                closurePartsSearchResultModel.ResponsibleTechid = dr["ResponsibleTechId"].ToString();
                closurePartsSearchResultModel.ResponsibleTechName = dr["ResponsibleTechName"].ToString();
                closurePartsSearchResultModel.BranchName = dr["BranchName"].ToString();
                closurePartsSearchResultModel.ToatlEventsByTech = dr["ToatlEventsByTech"].ToString();
                closurePartsSearchResultModel.ElapsedTime = dr["ElapsedTime"].ToString();
                closurePartsSearchResultModel.ElapsedTimeOnSite = dr["ElapsedTimeOnSite"].ToString();
                if (dr["FamilyAff"].ToString() == "SPD")
                {
                    closurePartsSearchResultModel.FamilyAff = "Internal";
                }
                if (dr["FamilyAff"].ToString() == "SPT")
                {
                    closurePartsSearchResultModel.FamilyAff = "3rd Party";
                }
                techResults.Add(closurePartsSearchResultModel);
            }
            techReportsModel.SearchResults = techResults;
            return techResults;
        }

        public JsonResult ClearPrdTechResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new TechnicianSearchResultModel(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void ExcelExportReport()
        {
            TechReportsModel techPriorityModel = new TechReportsModel();
            if (TempData["SearchCriteria"] != null)
            {
                techPriorityModel = TempData["SearchCriteria"] as TechReportsModel;
            }
            else
            {
                techPriorityModel.SearchResults = new List<TechnicianSearchResultModel>();
            }

            string gridModel = HttpContext.Request.Params["GridModel"];
            GridProperties gridProperty = ConvertGridObject(gridModel);
            ExcelExport exp = new ExcelExport();
            exp.Export(gridProperty, techPriorityModel.SearchResults, "TechResults.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Reports")]
        public ActionResult Reports(WorkorderSearchModel workOrderSearchModel)
        {
            switch (workOrderSearchModel.Operation)
            {
                case WorkOrderSearchSubmitType.SEARCH:
                    workOrderSearchModel.SearchResults = GetWorkOrderDataFromSP(workOrderSearchModel);
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
                    if (workOrderSearchModel.WOTypes == null)
                    {
                        workOrderSearchModel.WOTypes = new List<string>();
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
            return View("ReportsSearch", workOrderSearchModel);
        }

        private DataTable GetWorkOrderDataTable(WorkorderSearchModel workorderSearchModel)
        {
            DataTable dt = new DataTable();


            string StartDate = string.IsNullOrEmpty(workorderSearchModel.DateFrom.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.DateFrom).ToString("yyyy-MM-dd");
            string EndDate = string.IsNullOrEmpty(workorderSearchModel.DateTo.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.DateTo).ToString("yyyy-MM-dd");
            string varState = "0";
            if (workorderSearchModel.State != null && workorderSearchModel.State.Count > 0)
            {
                varState = string.IsNullOrEmpty(workorderSearchModel.State[0]) ? "0" : Convert.ToString(workorderSearchModel.State[0]);
            }
            string EventStatus = "0";

            if (workorderSearchModel.Status != null && workorderSearchModel.Status.Count > 0)
            {
                EventStatus = string.IsNullOrEmpty(workorderSearchModel.Status[0]) ? "0" : Convert.ToString(workorderSearchModel.Status[0]);
            }

            string ApptStartDate = string.IsNullOrEmpty(workorderSearchModel.AppointmentDateFrom.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.AppointmentDateFrom).ToString("yyyy-MM-dd");
            string ApptEndDate = string.IsNullOrEmpty(workorderSearchModel.AppointmentDateTo.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.AppointmentDateTo).ToString("yyyy-MM-dd");
            string ArrvStartDate = string.IsNullOrEmpty(workorderSearchModel.ArrivalStartDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.ArrivalStartDate).ToString("yyyy-MM-dd");
            string ArrvEndDate = string.IsNullOrEmpty(workorderSearchModel.ArrivalEndDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.ArrivalEndDate).ToString("yyyy-MM-dd");
            string ComplStartDate = string.IsNullOrEmpty(workorderSearchModel.CompletionStartDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.CompletionStartDate).ToString("yyyy-MM-dd");
            string ComplEndDate = string.IsNullOrEmpty(workorderSearchModel.CompletionEndDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.CompletionEndDate).ToString("yyyy-MM-dd");
            string CallTypeID = "0";
            if (workorderSearchModel.WOTypes != null && workorderSearchModel.WOTypes.Count > 0)
            {
                CallTypeID = string.IsNullOrEmpty(workorderSearchModel.WOTypes[0]) ? "0" : Convert.ToString(workorderSearchModel.WOTypes[0]);
            }

            string JDENO = string.IsNullOrEmpty(workorderSearchModel.CustomerId) ? "0" : workorderSearchModel.CustomerId.ToString();
            string PricingParentID = string.IsNullOrEmpty(workorderSearchModel.ParentAccount) ? "0" : workorderSearchModel.ParentAccount.ToString();

            string[] inputparam = { StartDate, EndDate, varState, EventStatus, ApptStartDate, ApptEndDate, ArrvStartDate, ArrvEndDate, ComplStartDate, ComplEndDate, CallTypeID, JDENO, PricingParentID };

            SqlHelper helper = new SqlHelper();
            dt = helper.GetProgramStatusReportData(inputparam);


            return dt;
        }
        private IList<WorkorderSearchResultModel> GetWorkOrderDataFromSP(WorkorderSearchModel workorderSearchModel)
        {
            DataTable dt = GetWorkOrderDataTable(workorderSearchModel);
            List<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();
            foreach (DataRow dr in dt.Rows)
            {
                WorkorderSearchResultModel woresults = new WorkorderSearchResultModel(dr);
                searchResults.Add(woresults);
            }
            return searchResults;
        }


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


            IQueryable<WorkOrder> workOrders = FarmerBrothersEntitites.Set<WorkOrder>().AsExpandable().Where(predicate).OrderByDescending(w => w.WorkorderID).Take(500);

            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();
            foreach (WorkOrder workOrder in workOrders)
            {
                searchResults.Add(new WorkorderSearchResultModel(workOrder));
            }

            if (searchResults.Count > 0 && originalWorkOrder != null)
            {
                searchResults.Insert(0, new WorkorderSearchResultModel(originalWorkOrder));
            }

            return searchResults;
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
                    branchType = "STP";
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

            workOrderSearchModel.States = FarmerBrothersEntitites.States.OrderBy(s => s.StateName).ToList();
            workOrderSearchModel.WorkOrderTypes = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.Active == 1).OrderBy(w => w.Sequence).ToList();
            workOrderSearchModel.WorkOrderStatusList = FarmerBrothersEntitites.AllFBStatus.Where(s => s.StatusFor == "Work Order Status" && s.Active == 1).OrderBy(s => s.StatusSequence).ToList();

            int userId = (int)System.Web.HttpContext.Current.Session["UserId"];
            workOrderSearchModel.SavedSearches = FarmerBrothersEntitites.WorkorderSavedSearches.Where(x => x.UserID == userId).ToList();
            workOrderSearchModel.FollowUpCallList = FarmerBrothersEntitites.AllFBStatus.Where(f => f.StatusFor == "Follow Up Call" && f.Active == 1).OrderBy(f => f.StatusSequence).ToList();
            workOrderSearchModel.FollowUpCallList.Insert(0, new AllFBStatu()
            {
                Active = 1,
                FBStatus = "",
                FBStatusID = -1,
                StatusFor = "Follow Up Call",
                StatusSequence = 0

            });

            workOrderSearchModel.PriorityList = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Priority" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            workOrderSearchModel.ServiceCenterList = GetServiceCenters(workOrderSearchModel.TechType);
            workOrderSearchModel.TechniciansList = GetTechnicians(workOrderSearchModel.ServiceCompany);
            workOrderSearchModel.TechnicianIds = GetTechnicianIds(workOrderSearchModel.ServiceCompany);
            workOrderSearchModel.TeamLeadList = GetTeamLeadsList(workOrderSearchModel.ServiceCompany);
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


        public ActionResult WorkOrderNoteDetails()
        {
            NotesModel notesModel = new NotesModel();
            return View("WorkOrderNoteDetails", notesModel);
        }
        [HttpPost]
        public ActionResult WorkOrderNoteDetails(string Id)
        {
            int noteId = Convert.ToInt32(Id);
            NotesModel notesModel = new NotesModel();
            IQueryable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == noteId).OrderByDescending(nh => nh.EntryDate);
            notesModel.NotesHistory = new List<NotesHistoryModel>();
            foreach (NotesHistory notesHistory in notesHistories)
            {
                notesModel.NotesHistory.Add(new NotesHistoryModel(notesHistory));
            }

            TempData["NotesHistory"] = notesModel;
            ViewBag.workOrderId = Id;
            return View("WorkOrderNoteDetails", notesModel);
        }

        [HttpPost]
        public void ExcelExport()
        {
            NotesModel notesModel = new NotesModel();
            if (TempData["NotesHistory"] != null)
            {
                notesModel = TempData["NotesHistory"] as NotesModel;
            }
            else
            {
                notesModel.NotesHistory = new List<NotesHistoryModel>();
            }

            string gridModel = HttpContext.Request.Params["GridModel"];
            GridProperties gridProperty = ConvertGridObject(gridModel);
            ExcelExport exp = new ExcelExport();
            exp.Export(gridProperty, notesModel.NotesHistory, "NotesHistory.xlsx", Syncfusion.XlsIO.ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        #region ERF Report
        public ActionResult ERFReport(int? isBack)
        {
            WorkorderSearchModel workOrderSearchModel;

            if (TempData["WorkOrderSearchCriteria"] != null && isBack == 1)
            {
                workOrderSearchModel = TempData["WorkOrderSearchCriteria"] as WorkorderSearchModel;
                TempData["WorkOrderSearchCriteria"] = workOrderSearchModel;
            }
            else
            {
                workOrderSearchModel = new WorkorderSearchModel();
                TempData["WorkOrderSearchCriteria"] = null;
            }

            workOrderSearchModel = PopulateWorkOrderSearchModelLists(workOrderSearchModel);
            workOrderSearchModel.State = new List<string>();
            workOrderSearchModel.Priority = new List<string>();
            workOrderSearchModel.WorkorderType = new List<string>();
            workOrderSearchModel.WOTypes = new List<string>();
            workOrderSearchModel.Status = new List<string>();
            workOrderSearchModel.FollowupCall = defaultFollowUpCall;
            workOrderSearchModel.Status.Add("");
            workOrderSearchModel.WOTypes.Add("");
            workOrderSearchModel.State.Add("");
            workOrderSearchModel.WorkorderType.Add("");
            workOrderSearchModel.Priority.Add("");

            workOrderSearchModel.CashSalesList = Utility.GetCashSaleStatusList(FarmerBrothersEntitites);
            workOrderSearchModel.ERFStatusList = ERFStatusModel.GetERFStatusList();

            return View(workOrderSearchModel);

        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ERFReports")]
        public ActionResult ERFReports(WorkorderSearchModel workOrderSearchModel)
        {
            switch (workOrderSearchModel.Operation)
            {
                case WorkOrderSearchSubmitType.SEARCH:
                    workOrderSearchModel.SearchResults = GetERFData(workOrderSearchModel);
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
                    if (workOrderSearchModel.WOTypes == null)
                    {
                        workOrderSearchModel.WOTypes = new List<string>();
                    }
                    if (workOrderSearchModel.State == null)
                    {
                        workOrderSearchModel.State = new List<string>();
                    }
                    if (workOrderSearchModel.CashSalesList == null)
                    {
                        workOrderSearchModel.CashSalesList = new List<CashSaleModel>();
                    }
                    if (workOrderSearchModel.ERFStatusList == null)
                    {
                        workOrderSearchModel.ERFStatusList = new List<ERFStatusModel>();
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
            workOrderSearchModel.CashSalesList = Utility.GetCashSaleStatusList(FarmerBrothersEntitites);
            workOrderSearchModel.ERFStatusList = ERFStatusModel.GetERFStatusList();

            return View("ERFReport", workOrderSearchModel);
        }

        private IList<WorkorderSearchResultModel> GetERFData(WorkorderSearchModel workorderSearchModel)
        {
            DataTable dt = GetERFDataTable(workorderSearchModel);
            List<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();
            foreach (DataRow dr in dt.Rows)
            {
                WorkorderSearchResultModel woresults = new WorkorderSearchResultModel();

                woresults.WorkorderID = dr["WorkorderID"] != DBNull.Value ? Convert.ToInt32(dr["WorkorderID"]) : 0;
                woresults.CustomerID = dr["CustomerID"] != DBNull.Value ? Convert.ToInt32(dr["CustomerID"]) : 0;

                Contact customer = FarmerBrothersEntitites.Contacts.Where(con => con.ContactID == woresults.CustomerID).FirstOrDefault();

                //woresults.CustomerType = customer.
                //string name = (dr["FirstName"] != DBNull.Value ? dr["FirstName"].ToString() : "") + " " + (dr["LastName"] != DBNull.Value ? dr["LastName"].ToString() : "");
                //woresults.OriginatorName = name.Trim();
                woresults.OriginatorName = dr["ERFEntryUser"] != DBNull.Value ? dr["ERFEntryUser"].ToString() : "";
                woresults.ModifiedUser = dr["ERFLastUpdatedUser"] != DBNull.Value ? dr["ERFLastUpdatedUser"].ToString() : "";
                woresults.ModifiedDate = dr["ERFLastUpdatedDate"] != DBNull.Value ? dr["ERFLastUpdatedDate"].ToString() : "";
                woresults.CustomerName = dr["CompanyName"] != DBNull.Value ? dr["CompanyName"].ToString() : "";
                woresults.CustomerCity = dr["City"] != DBNull.Value ? dr["City"].ToString() : "";
                woresults.Address1 = dr["Address1"] != DBNull.Value ? dr["Address1"].ToString() : "";
                woresults.CustomerState = dr["State"] != DBNull.Value ? dr["State"].ToString() : "";
                woresults.CustomerZipCode = dr["PostalCode"] != DBNull.Value ? dr["PostalCode"].ToString() : "";

                woresults.WorkorderCallstatus = dr["ERFStatus"] != DBNull.Value ? dr["ERFStatus"].ToString() : "";
                string cashSaleStatus = dr["CashSaleStatus"] != DBNull.Value ? dr["CashSaleStatus"].ToString() : "";
                string cashSaleStatusName = "";
                if(!string.IsNullOrEmpty(cashSaleStatus))
                {
                    using (FarmerBrothersEntities fbEntities = new FarmerBrothersEntities())
                    {
                        IList<CashSaleModel> salesList = Utility.GetCashSaleStatusList(fbEntities);
                        string saleStatusName = salesList.Where(s => s.StatusCode == cashSaleStatus).Select(c => c.StatusName).FirstOrDefault();
                        cashSaleStatusName = string.IsNullOrEmpty(saleStatusName) ? "" : saleStatusName;
                    }
                }
                woresults.CaseSaleStatus = cashSaleStatusName;
                woresults.AppointmentDate = dr["OriginalRequestedDate"] != DBNull.Value ? dr["OriginalRequestedDate"].ToString() : "";
                woresults.WorkorderEntryDate = dr["ERFEntryDate"] != DBNull.Value ? dr["ERFEntryDate"].ToString() : "";
                //woresults.WorkorderCloseDate = dr["WorkorderCloseDate"] != DBNull.Value ? dr["WorkorderCloseDate"].ToString() : "";

                //woresults.EventCallTypeID = dr["WorkorderCalltypeid"] != DBNull.Value ? dr["WorkorderCalltypeid"].ToString() : "";
                //woresults.WorkorderCalltypeDesc = dr["WorkorderCalltypeDesc"] != DBNull.Value ? dr["WorkorderCalltypeDesc"].ToString() : "";

                string erfId = dr["ErfID"] != DBNull.Value ? dr["ErfID"].ToString() : "";

                woresults.ERFNO = erfId;
                //woresults.FSMJDE = dr["FSMJDE"] != DBNull.Value ? dr["FSMJDE"].ToString() : "";
                woresults.CustomerRegion = dr["CustomerRegion"] != DBNull.Value ? dr["CustomerRegion"].ToString() : "";
                woresults.RegionNumber = dr["RegionNumber"] != DBNull.Value ? dr["RegionNumber"].ToString() : "";
                woresults.CustomerBranch = dr["CustomerBranch"] != DBNull.Value ? dr["CustomerBranch"].ToString() : "";
                woresults.Branch = dr["Branch"] != DBNull.Value ? dr["Branch"].ToString() : "";

                woresults.OrderType = dr["OrderType"] != DBNull.Value ? dr["OrderType"].ToString() : "";
                woresults.ShipToBranch = dr["ShipToBranch"] != DBNull.Value ? dr["ShipToBranch"].ToString() : "";
                woresults.SiteReady = dr["SiteReady"] != DBNull.Value ? dr["SiteReady"].ToString() : "";

                woresults.TotalNSV = dr["TotalNSV"] != DBNull.Value ? dr["TotalNSV"].ToString() : "";

                woresults.ApprovalStatus = dr["ApprovalStatus"] != DBNull.Value ? dr["ApprovalStatus"].ToString() : "";
                woresults.ContactName = dr["CompanyName"] != DBNull.Value ? dr["CompanyName"].ToString() : "";
                woresults.WOClosedDate = dr["WorkorderCloseDate"] != DBNull.Value ? dr["WorkorderCloseDate"].ToString() : "";
                string status = dr["WorkorderCallstatus"] != DBNull.Value ? dr["WorkorderCallstatus"].ToString() : "";
                woresults.WOStatus = status;
                woresults.EqpType = dr["EqpType"] != DBNull.Value ? dr["EqpType"].ToString() : ""; ;
                woresults.EqpName = dr["EqpName"] != DBNull.Value ? dr["EqpName"].ToString() : ""; ;
                woresults.EqpCategoryName = dr["EqpCategoryName"] != DBNull.Value ? dr["EqpCategoryName"].ToString() : ""; ;
                woresults.ExpType = dr["ExpType"] != DBNull.Value ? dr["ExpType"].ToString() : ""; ;
                woresults.EqpSerialNumber = dr["EqpSerialNumber"] != DBNull.Value ? dr["EqpSerialNumber"].ToString() : "";
                woresults.EqpOrderType = dr["EqpOrderType"] != DBNull.Value ? dr["EqpOrderType"].ToString() : "";
                woresults.EqpDepositInvoiceNumber = dr["EqpDepositInvoiceNumber"] != DBNull.Value ? dr["EqpDepositInvoiceNumber"].ToString() : "";
                woresults.EqpDepositAmount = dr["EqpDepositAmount"] != DBNull.Value ? dr["EqpDepositAmount"].ToString() : "";
                woresults.EqpFinalInvoiceNumber = dr["EqpFinalInvoiceNumber"] != DBNull.Value ? dr["EqpFinalInvoiceNumber"].ToString() : "";
                woresults.EqpInvoiceTotal = dr["EqpInvoiceTotal"] != DBNull.Value ? dr["EqpInvoiceTotal"].ToString() : "";
                woresults.ExpName = dr["ExpName"] != DBNull.Value ? dr["ExpName"].ToString() : ""; ;
                woresults.ExpCategoryName = dr["ExpCategoryName"] != DBNull.Value ? dr["ExpCategoryName"].ToString() : "";
                 woresults.EqpInternalOrderType= dr["EqpInternalType"] != DBNull.Value ? dr["EqpInternalType"].ToString() : "";
                 woresults.EqpVendorOrderType = dr["EqpVendorType"] != DBNull.Value ? dr["EqpVendorType"].ToString() : "";
                 woresults.ExpInternalOrderType= dr["ExpInternalType"] != DBNull.Value ? dr["ExpInternalType"].ToString() : "";
                 woresults.ExpVendorOrderType = dr["ExpVendorType"] != DBNull.Value ? dr["ExpVendorType"].ToString() : "";
                woresults.EqpQty = dr["EqpQty"] != DBNull.Value ? dr["EqpQty"].ToString() : "";
                woresults.ExpQty = dr["ExpQty"] != DBNull.Value ? dr["ExpQty"].ToString() : "";
                woresults.DispatchDate = dr["DispatchDate"] != DBNull.Value ? dr["DispatchDate"].ToString() : "";
                if (!string.IsNullOrEmpty(status) && (status.ToLower() == "accepted" || status.ToLower() == "completed" || status.ToLower() == "closed"))
                {
                    woresults.AcceptedDate = dr["AcceptedDate"] != DBNull.Value ? dr["AcceptedDate"].ToString() : "";
                }
                else
                {
                    woresults.AcceptedDate = "";
                }
                woresults.DispatchTech = dr["DispatchTech"] != DBNull.Value ? dr["DispatchTech"].ToString() : "";

                decimal ? eqpTotal = erfId == "" ? 0 : FarmerBrothersEntitites.FBERFEquipments.Where(eqp => eqp.ERFId == erfId).Sum(x => x.TotalCost);
                decimal? expTotal = erfId == "" ? 0 : FarmerBrothersEntitites.FBERFExpendables.Where(eqp => eqp.ERFId == erfId).Sum(x => x.TotalCost);

                decimal GrandTotal = Convert.ToDecimal(eqpTotal + expTotal);

                woresults.EqpTotal = eqpTotal.ToString();
                woresults.ExpTotal = expTotal.ToString();

                if (eqpTotal != null && expTotal != null)
                {
                    woresults.Total = (eqpTotal + expTotal).ToString();
                }
                else if (eqpTotal != null && expTotal == null)
                {
                    woresults.Total = (eqpTotal).ToString();
                }
                else if (eqpTotal == null && expTotal != null)
                {
                    woresults.Total = (expTotal).ToString();
                }
                else
                {
                    woresults.Total = "";
                }
                
                searchResults.Add(woresults);
            }
            return searchResults;
        }

        private DataTable GetERFDataTable(WorkorderSearchModel workorderSearchModel)
        {
            DataTable dt = new DataTable();


            string StartDate = string.IsNullOrEmpty(workorderSearchModel.DateFrom.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.DateFrom).ToString("yyyy-MM-dd");
            string EndDate = string.IsNullOrEmpty(workorderSearchModel.DateTo.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.DateTo).ToString("yyyy-MM-dd");
            string varState = "0";
            if (workorderSearchModel.State != null && workorderSearchModel.State.Count > 0)
            {
                varState = string.IsNullOrEmpty(workorderSearchModel.State[0]) ? "0" : Convert.ToString(workorderSearchModel.State[0]);
            }
            string EventStatus = "0";

            if (workorderSearchModel.Status != null && workorderSearchModel.Status.Count > 0)
            {
                EventStatus = string.IsNullOrEmpty(workorderSearchModel.Status[0]) ? "0" : Convert.ToString(workorderSearchModel.Status[0]);
            }

            string ApptStartDate = string.IsNullOrEmpty(workorderSearchModel.AppointmentDateFrom.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.AppointmentDateFrom).ToString("yyyy-MM-dd");
            string ApptEndDate = string.IsNullOrEmpty(workorderSearchModel.AppointmentDateTo.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.AppointmentDateTo).ToString("yyyy-MM-dd");
            string ArrvStartDate = string.IsNullOrEmpty(workorderSearchModel.ArrivalStartDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.ArrivalStartDate).ToString("yyyy-MM-dd");
            string ArrvEndDate = string.IsNullOrEmpty(workorderSearchModel.ArrivalEndDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.ArrivalEndDate).ToString("yyyy-MM-dd");
            string ComplStartDate = string.IsNullOrEmpty(workorderSearchModel.CompletionStartDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.CompletionStartDate).ToString("yyyy-MM-dd");
            string ComplEndDate = string.IsNullOrEmpty(workorderSearchModel.CompletionEndDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.CompletionEndDate).ToString("yyyy-MM-dd");
            string CallTypeID = "0";
            if (workorderSearchModel.WOTypes != null && workorderSearchModel.WOTypes.Count > 0)
            {
                CallTypeID = string.IsNullOrEmpty(workorderSearchModel.WOTypes[0]) ? "0" : Convert.ToString(workorderSearchModel.WOTypes[0]);
            }

            string JDENO = string.IsNullOrEmpty(workorderSearchModel.CustomerId) ? "0" : workorderSearchModel.CustomerId.ToString();
            string PricingParentID = string.IsNullOrEmpty(workorderSearchModel.ParentAccount) ? "0" : workorderSearchModel.ParentAccount.ToString();

            string workorderId = string.IsNullOrEmpty(workorderSearchModel.WorkorderId) ? "0" : workorderSearchModel.WorkorderId.ToString();
            string customerName = string.IsNullOrEmpty(workorderSearchModel.CustomerName) ? "0" : workorderSearchModel.CustomerName.ToString();
            string erfid = string.IsNullOrEmpty(workorderSearchModel.ErfId) ? "0" : workorderSearchModel.ErfId.ToString();

            string cashSaleStatus = string.IsNullOrEmpty(workorderSearchModel.CashSaleStatus) ? "0" : workorderSearchModel.CashSaleStatus.ToString();
            string erfStatus = string.IsNullOrEmpty(workorderSearchModel.ErfStatus) ? "0" : workorderSearchModel.ErfStatus.ToString();
            //string[] inputparam = { StartDate, EndDate, varState, EventStatus, ApptStartDate, ApptEndDate, ArrvStartDate, ArrvEndDate, ComplStartDate, ComplEndDate, CallTypeID, JDENO, PricingParentID };

            SqlHelper helper = new SqlHelper();
            //dt = helper.GetProgramStatusReportData(inputparam);

            //string sSql = @"SELECT dbo.WorkOrder.WorkorderID, dbo.WorkOrder.[CustomerID], dbo.WorkOrder.[WorkorderCallstatus], dbo.WorkOrder.[WorkorderEntryDate],
            //                         dbo.WorkOrder.[WorkorderCloseDate],dbo.WorkOrder.[NoServiceRequired],
            //                        dbo.WorkOrder.[WorkorderCalltypeid], dbo.WorkOrder.[WorkorderCalltypeDesc],
            //                        dbo.Contact.CompanyName, dbo.Contact.Address1, dbo.Contact.City, 
            //                        dbo.Contact.State,dbo.Contact.PostalCode, dbo.Contact.FieldServiceManager, dbo.Contact.FSMJDE, dbo.Contact.PricingParentName, 
            //                        dbo.Contact.DeliveryDesc,
            //                        dbo.erf.[ErfID], 
            //                        dbo.WorkOrder.[OriginalWorkorderid], 
            //                        dbo.WorkOrder.[WorkorderEquipCount], dbo.WorkOrder.[AppointmentDate], 
            //                        dbo.Contact.CustomerRegion,dbo.Contact.RegionNumber, dbo.Contact.CustomerBranch,dbo.Contact.Branch,dbo.Contact.SearchType,
            //                        dbo.Contact.SearchDesc,dbo.Contact.PricingParentID,0 as IsBillable,0 as TotalUnitPrice,fs.FBStatus as ServicePriority ,ws.EventScheduleDate, dbo.erf.OriginalRequestedDate,
            //                        dbo.erf.OrderType, dbo.erf.ShipToBranch, dbo.erf.SiteReady
            //                        From dbo.WorkOrder (nolock)
            //                        INNER JOIN dbo.Contact (nolock) ON dbo.WorkOrder.[CustomerID] = dbo.Contact.ContactID
            //                        left join erf (nolock) on workorder.workorderid = erf.workorderid
            //                        left join WorkorderSchedule ws WITH (NOLOCK) on workorder.WorkorderID = ws.WorkorderID and  (ws.AssignedStatus = 'Scheduled')
            //                        left join AllFBStatus fs (nolock) on workorder.PriorityCode = fs.FBStatusID and fs.Active =1 and fs.StatusFor='Priority'
            //                        where dbo.WorkOrder.WorkorderID > 10 ";


            string sSql = @"Select ERF.WorkorderID,ERF.ERFID,EntryUser.FirstName+' '+EntryUser.LastName as ERFEntryUser,'' as  OriginatorName, Contact.CompanyName, Workorder.WorkorderCloseDate, Workorder.WorkorderCallstatus,
                                        ERF.ERFStatus,ERF.CashSaleStatus,ERF.CustomerID,ERF.OriginalRequestedDate,ERF.EntryDate as ERFEntryDate, ModifiedUser.FirstName +' '+ModifiedUser.LastName as ERFLastUpdatedUser, ERF.ModifiedDate as ERFLastUpdatedDate,
                                        ERF.OrderType, ERF.ShipToBranch, ERF.SiteReady,ERF.CurrentNSV, (ERF.TotalNSV + ERF.CurrentNSV) as TotalNSV,ERF.ApprovalStatus,
                                        Contact.CompanyName, Contact.Address1, Contact.City, 
                                        Contact.State,Contact.PostalCode, Contact.FieldServiceManager, Contact.FSMJDE, Contact.PricingParentName, 
                                        Contact.DeliveryDesc,
                                        Contact.CustomerRegion,Contact.RegionNumber, Contact.CustomerBranch,Contact.Branch,Contact.SearchType,
                                        Contact.SearchDesc,Contact.PricingParentID, ERF.OriginalRequestedDate as AppointmentDate,
                                        fbeqp.Quantity as EqpQty, '' as EquipmentTotal, fbexp.Quantity as ExpQty, '' as ExpandableTotal,'' as Total,
                                        C.ContingentType as EqpType, C.ContingentName as EqpName, CD.Name as EqpCategoryName, 
                                        C1.ContingentType as ExpType, C1.ContingentName as ExpName,CD1.Name as ExpCategoryName,
                                        ERF.WorkorderID,ERF.ERFID,Contact.CompanyName, fbeqp.InternalOrderType as EqpInternalType, fbeqp.VendorOrderType as EqpVendorType,
                                        fbexp.InternalOrderType as ExpInternalType, fbexp.VendorOrderType as ExpVendorType,
                                        fbexp.InternalOrderType as ExpInternalType, fbexp.VendorOrderType as ExpVendorType,
                                        Workorderschedule.EntryDate as DispatchDate, Workorderschedule.ModifiedScheduleDate as AcceptedDate, DispatchTech.CompanyName As DispatchTech,
                                        fbeqp.SerialNumber As EqpSerialNumber, fbeqp.OrderType As EqpOrderType, fbeqp.DepositInvoiceNumber As EqpDepositInvoiceNumber , 
                                        fbeqp.DepositAmount As EqpDepositAmount, fbeqp.FinalInvoiceNumber As EqpFinalInvoiceNumber, fbeqp.InvoiceTotal As EqpInvoiceTotal                                        
                                        from ERF (nolock)
                                        INNER JOIN Contact (nolock) ON ERF.CustomerID = Contact.ContactID
                                        LEFT JOIN FBERFEquipment as fbeqp (nolock) ON fbeqp.ERFId = Erf.ErfID
                                        LEFT JOIN Contingent as C (nolock) ON C.ContingentID = fbeqp.ContingentCategoryId
                                        LEFT JOIN ContingentDetails as CD (nolock) ON CD.ID = fbeqp.ContingentCategoryTypeId
                                        LEFT JOIN FBERFExpendable as fbexp (nolock) ON fbexp.ERFId = Erf.ErfID
                                        LEFT JOIN Contingent as C1 (nolock) ON C1.ContingentID = fbexp.ContingentCategoryId
                                        LEFT JOIN ContingentDetails as CD1 (nolock) ON CD1.ID = fbexp.ContingentCategoryTypeId
                                        LEFT JOIN Workorder (nolock) ON ERF.WorkorderID = Workorder.WorkorderID
                                        LEFT JOIN Workorderschedule (nolock) ON Workorderschedule.WorkorderID = Workorder.WorkorderID and (Workorderschedule.AssignedStatus = 'Accepted' or Workorderschedule.AssignedStatus = 'Sent')
                                        LEFT JOIN TECH_HIERARCHY DispatchTech (nolock) ON DispatchTech.DealerId = WorkorderSchedule.Techid
                                        LEFT JOIN FbUserMaster EntryUser (nolock) ON EntryUser.UserId = ERF.EntryUserID
                                        LEFT JOIN FbUserMaster ModifiedUser (nolock) ON ModifiedUser.UserId = ERF.ModifiedUserID
                                        Where ERF.ERFID > 10 ";

            if (StartDate != "0")
            {
                sSql += " And ERF.EntryDate >= '" + StartDate + "'";
            }

            if (EndDate != "0")
            {
                sSql += " And ERF.EntryDate <= CONVERT(smalldatetime,  '" + EndDate + "', 101) + 1";
            }

            if (JDENO != "0")
            {
                sSql += " And ERF.CustomerID = " + JDENO;
            }

            if (varState != "0")
            {
                sSql += " And ERF.CustomerState= '" + varState + "'";
            }
            if(erfid != "0")
            {
                sSql += " And ERF.ErfId= " + erfid;
            }
            if (customerName != "0")
            {
                sSql += " AND ERF.CustomerName like '%" + customerName + "%'";
            }
            if (workorderId != "0")
            {
                sSql += " And ERF.WorkorderId= " + workorderId;
            }
            if (cashSaleStatus != "0")
            {
                sSql += " And ERF.CashSaleStatus= '" + cashSaleStatus + "'";
            }
            if (erfStatus != "0")
            {
                sSql += " And ERF.erfstatus= '" + erfStatus + "'";
            }

            MarsViews mars = new MarsViews();
            dt = mars.fnTpspVendors(sSql);


            return dt;
        }


        public FileResult ERFExportToExcel()
        {
            WorkorderSearchModel workOrderSearchModel = new WorkorderSearchModel();
            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();

            string gridModel = HttpContext.Request.Params["GridModel"];
            DataTable dt = new DataTable();
            if (TempData["WorkOrderSearchCriteria"] != null)
            {
                workOrderSearchModel = TempData["WorkOrderSearchCriteria"] as WorkorderSearchModel;
                dt = GetERFDataTable(workOrderSearchModel);
            }

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ErfID"] != DBNull.Value)
                    {
                        string erfId = dr["ErfID"].ToString();
                        if (dr["OriginalRequestedDate"] != DBNull.Value)
                        {
                            dr["AppointmentDate"] = dr["OriginalRequestedDate"];
                        }

                        //string name = (dr["FirstName"] != DBNull.Value ? dr["FirstName"].ToString() : "") +  " " + (dr["LastName"] != DBNull.Value ? dr["LastName"].ToString() : "");
                        //dr["OriginatorName"] = name.Trim();
                        dr["OriginatorName"] = dr["ERFEntryUser"] != DBNull.Value ? dr["ERFEntryUser"].ToString() : "";

                        decimal? eqpTotal = erfId == "" ? 0 : FarmerBrothersEntitites.FBERFEquipments.Where(eqp => eqp.ERFId == erfId).Sum(x => x.TotalCost);
                        decimal? expTotal = erfId == "" ? 0 : FarmerBrothersEntitites.FBERFExpendables.Where(eqp => eqp.ERFId == erfId).Sum(x => x.TotalCost);

                        decimal GrandTotal = Convert.ToDecimal(eqpTotal + expTotal);

                        dr["EquipmentTotal"] = eqpTotal == null ? "" : eqpTotal.ToString();
                        dr["ExpandableTotal"] = expTotal == null ? "" : expTotal.ToString();

                        if (eqpTotal != null && expTotal != null)
                        {
                            dr["Total"] = (eqpTotal + expTotal).ToString();
                        }
                        else if (eqpTotal != null && expTotal == null)
                        {
                            dr["Total"] = (eqpTotal).ToString();
                        }
                        else if (eqpTotal == null && expTotal != null)
                        {
                            dr["Total"] = (expTotal).ToString();
                        }
                        else
                        {
                            dr["Total"] = "";
                        }

                        string status = dr["WorkorderCallstatus"] == null ? "" : dr["WorkorderCallstatus"].ToString();
                        if (dr["AcceptedDate"] != DBNull.Value && !string.IsNullOrEmpty(status) && (status.ToLower() == "accepted" || status.ToLower() == "completed" || status.ToLower() == "closed"))
                        {
                            dr["AcceptedDate"] = dr["AcceptedDate"] != DBNull.Value ? dr["AcceptedDate"].ToString() : "";
                        }
                        else
                        {
                            dr["AcceptedDate"] = DBNull.Value;
                        }

                        string cashSaleStatus = dr["CashSaleStatus"] != DBNull.Value ? dr["CashSaleStatus"].ToString() : "";
                        string cashSaleStatusName = "";
                        if (!string.IsNullOrEmpty(cashSaleStatus))
                        {
                            using (FarmerBrothersEntities fbEntities = new FarmerBrothersEntities())
                            {
                                IList<CashSaleModel> salesList = Utility.GetCashSaleStatusList(fbEntities);
                                string saleStatusName = salesList.Where(s => s.StatusCode == cashSaleStatus).Select(c => c.StatusName).FirstOrDefault();
                                cashSaleStatusName = string.IsNullOrEmpty(saleStatusName) ? "" : saleStatusName;
                            }
                        }
                        dr["CashSaleStatus"] = cashSaleStatusName;

                    }

                }
            }
            else
            {
                DataColumn dc1 = new DataColumn("WorkorderID", typeof(String));                               
                DataColumn dc2 = new DataColumn("CustomerID", typeof(String));
                DataColumn dc3 = new DataColumn("CompanyName", typeof(String));
                DataColumn dc4 = new DataColumn("Address1", typeof(String));
                DataColumn dc5 = new DataColumn("City", typeof(String));
                DataColumn dc6 = new DataColumn("State", typeof(String));
                DataColumn dc7 = new DataColumn("PostalCode", typeof(String));
                DataColumn dc8 = new DataColumn("WorkorderCallstatus", typeof(String));
                DataColumn dc9 = new DataColumn("AppointmentDate", typeof(String));
                DataColumn dc10 = new DataColumn("ERFEntryDate", typeof(String));

                DataColumn dc11 = new DataColumn("DispatchDate", typeof(String));
                DataColumn dc12 = new DataColumn("AcceptedDate", typeof(String));
                DataColumn dc13 = new DataColumn("DispatchTech", typeof(String));

                DataColumn dc14 = new DataColumn("WorkorderCalltypeid", typeof(String));
                DataColumn dc15 = new DataColumn("WorkorderCalltypeDesc", typeof(String));
                DataColumn dc16 = new DataColumn("ERFID", typeof(String));
                DataColumn dc17 = new DataColumn("ERFStatus", typeof(String));
                DataColumn dc18 = new DataColumn("CashSaleStatus", typeof(String));
                DataColumn dc19 = new DataColumn("CustomerRegion", typeof(String));
                DataColumn dc20 = new DataColumn("RegionNumber", typeof(String));
                DataColumn dc21 = new DataColumn("CustomerBranch", typeof(String));
                DataColumn dc22 = new DataColumn("Branch", typeof(String));
                DataColumn dc23 = new DataColumn("FSMJDE", typeof(String));
                DataColumn dc24 = new DataColumn("OrderType", typeof(String));
                DataColumn dc25 = new DataColumn("ShipToBranch", typeof(String));
                DataColumn dc26 = new DataColumn("SiteReady", typeof(String));
                DataColumn dc27 = new DataColumn("EquipmentTotal", typeof(String));
                DataColumn dc28 = new DataColumn("ExpandableTotal", typeof(String));
                DataColumn dc29 = new DataColumn("Total", typeof(String));
                DataColumn dc30 = new DataColumn("TotalNSV", typeof(String));
                DataColumn dc31 = new DataColumn("ApprovalStatus", typeof(String));
                DataColumn dc32 = new DataColumn("CompanyName", typeof(String));
                DataColumn dc33 = new DataColumn("WorkorderCloseDate", typeof(String));
                DataColumn dc34 = new DataColumn("WorkorderCallstatus", typeof(String));
                DataColumn dc35 = new DataColumn("EqpType", typeof(String));
                DataColumn dc36 = new DataColumn("EqpName", typeof(String));
                DataColumn dc37 = new DataColumn("EqpCategoryName", typeof(String));
                DataColumn dc38 = new DataColumn("ExpType", typeof(String));
                DataColumn dc39 = new DataColumn("ExpName", typeof(String));
                DataColumn dc40 = new DataColumn("ExpCategoryName", typeof(String));
                DataColumn dc41 = new DataColumn("OriginatorName", typeof(String));
                DataColumn dc42 = new DataColumn("ERFLastUpdatedUser", typeof(String));
                DataColumn dc43 = new DataColumn("ERFLastUpdatedDate", typeof(String));
                DataColumn dc44 = new DataColumn("EqpInternalType", typeof(String));
                DataColumn dc45 = new DataColumn("EqpVendorType", typeof(String));
                DataColumn dc46 = new DataColumn("ExpInternalType", typeof(String));
                DataColumn dc47 = new DataColumn("ExpVendorType", typeof(String));
                DataColumn dc48 = new DataColumn("EqpQty", typeof(String));
                DataColumn dc49 = new DataColumn("ExpQty", typeof(String));

                DataColumn dc50= new DataColumn("EqpSerialNumber", typeof(String));
                DataColumn dc51 = new DataColumn("EqpOrderType", typeof(String));
                DataColumn dc52 = new DataColumn("EqpDepositInvoiceNumber", typeof(String));
                DataColumn dc53 = new DataColumn("EqpDepositAmount", typeof(String));
                DataColumn dc54 = new DataColumn("EqpFinalInvoiceNumber", typeof(String));
                DataColumn dc55 = new DataColumn("EqpInvoiceTotal", typeof(String));

                dt.Columns.Add(dc1);
                dt.Columns.Add(dc2);
                dt.Columns.Add(dc3);
                dt.Columns.Add(dc4);
                dt.Columns.Add(dc5);
                dt.Columns.Add(dc6);
                dt.Columns.Add(dc7);
                dt.Columns.Add(dc8);
                dt.Columns.Add(dc9);
                dt.Columns.Add(dc10);
                dt.Columns.Add(dc11);
                dt.Columns.Add(dc12);
                dt.Columns.Add(dc13);
                dt.Columns.Add(dc14);
                dt.Columns.Add(dc15);
                dt.Columns.Add(dc16);
                dt.Columns.Add(dc17);
                dt.Columns.Add(dc18);
                dt.Columns.Add(dc19);
                dt.Columns.Add(dc20);
                dt.Columns.Add(dc21);
                dt.Columns.Add(dc22);
                dt.Columns.Add(dc23);
                dt.Columns.Add(dc24);
                dt.Columns.Add(dc25);
                dt.Columns.Add(dc26);
                dt.Columns.Add(dc27);
                dt.Columns.Add(dc28);
                dt.Columns.Add(dc29);
                dt.Columns.Add(dc30);
                dt.Columns.Add(dc31);
                dt.Columns.Add(dc32);
                dt.Columns.Add(dc33);
                dt.Columns.Add(dc34);
                dt.Columns.Add(dc35);
                dt.Columns.Add(dc36);
                dt.Columns.Add(dc37);
                dt.Columns.Add(dc38);
                dt.Columns.Add(dc39);
                dt.Columns.Add(dc40);
                dt.Columns.Add(dc41);
                dt.Columns.Add(dc42);
                dt.Columns.Add(dc43);
                dt.Columns.Add(dc44);
                dt.Columns.Add(dc45);
                dt.Columns.Add(dc46);
                dt.Columns.Add(dc47);
                dt.Columns.Add(dc48);
                dt.Columns.Add(dc49);
                dt.Columns.Add(dc50);
                dt.Columns.Add(dc51);
                dt.Columns.Add(dc52);
                dt.Columns.Add(dc53);
                dt.Columns.Add(dc54);
                dt.Columns.Add(dc55);
            }

            TempData["WorkOrderSearchCriteria"] = workOrderSearchModel;

            string[] columns = {"WorkorderID","CustomerID","OriginatorName","ERFLastUpdatedUser","ERFLastUpdatedDate","CompanyName","Address1","City","State","PostalCode","WorkorderCallstatus","AppointmentDate","ERFEntryDate","DispatchDate","AcceptedDate","DispatchTech","WorkorderCalltypeid",
                                "WorkorderCalltypeDesc","ERFID","ERFStatus","CashSaleStatus","FSMJDE","CustomerRegion","RegionNumber","CustomerBranch","Branch","FSMJDE","OrderType","ShipToBranch","SiteReady","EqpQty",
                                "EquipmentTotal","ExpQty","ExpandableTotal","Total", "TotalNSV", "ApprovalStatus", "CompanyName", "WorkorderCloseDate", "WorkorderCallstatus", "EqpType","EqpName","EqpCategoryName", "ExpType","ExpName","ExpCategoryName",
                                "OriginatorName","EqpInternalType", "EqpVendorType","ExpInternalType", "ExpVendorType", "EqpSerialNumber", "EqpOrderType", "EqpDepositInvoiceNumber", "EqpDepositAmount", "EqpFinalInvoiceNumber", "EqpInvoiceTotal"};
                      
            byte[] filecontent = ExcelExportHelper.ExportExcel(dt, "", false, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "ERFReportResults.xlsx");
        }


        #endregion

        #region Asset Exception Report
        [HttpGet]
        public ActionResult AssetExceptionReport(int? isBack)
        {
            WorkorderSearchModel workOrderSearchModel;

            if (TempData["AssetExceptionSearchCriteria"] != null && isBack == 1)
            {
                workOrderSearchModel = TempData["AssetExceptionSearchCriteria"] as WorkorderSearchModel;
                TempData["AssetExceptionSearchCriteria"] = workOrderSearchModel;
            }
            else
            {
                workOrderSearchModel = new WorkorderSearchModel();
                TempData["AssetExceptionSearchCriteria"] = null;
            }

            workOrderSearchModel = PopulateAssetExceptionModelLists(workOrderSearchModel);
            workOrderSearchModel.State = new List<string>();
            workOrderSearchModel.Priority = new List<string>();
            workOrderSearchModel.WorkorderType = new List<string>();
            workOrderSearchModel.WOTypes = new List<string>();
            workOrderSearchModel.Status = new List<string>();
            workOrderSearchModel.FollowupCall = defaultFollowUpCall;
            workOrderSearchModel.Status.Add("");
            workOrderSearchModel.WOTypes.Add("");
            workOrderSearchModel.State.Add("");
            workOrderSearchModel.WorkorderType.Add("");
            workOrderSearchModel.Priority.Add("");

            return View("AssetExceptionReport", workOrderSearchModel);
        }

        private WorkorderSearchModel PopulateAssetExceptionModelLists(WorkorderSearchModel workOrderSearchModel)
        {
            workOrderSearchModel.AutoDispatchedList = new List<string>();
            workOrderSearchModel.AutoDispatchedList.Add("No");
            workOrderSearchModel.AutoDispatchedList.Add("Yes");
            workOrderSearchModel.AutoDispatchedList.Add(string.Empty);

            workOrderSearchModel.TechTypeList = new List<string>();
            workOrderSearchModel.TechTypeList.Add("");
            workOrderSearchModel.TechTypeList.Add("Internal");
            workOrderSearchModel.TechTypeList.Add("TPSP");

            workOrderSearchModel.States = FarmerBrothersEntitites.States.OrderBy(s => s.StateName).ToList();
            workOrderSearchModel.WorkOrderTypes = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.Active == 1).OrderBy(w => w.Sequence).ToList();
            workOrderSearchModel.WorkOrderStatusList = FarmerBrothersEntitites.AllFBStatus.Where(s => s.StatusFor == "Work Order Status" && s.Active == 1).OrderBy(s => s.StatusSequence).ToList();

            int userId = (int)System.Web.HttpContext.Current.Session["UserId"];
            workOrderSearchModel.SavedSearches = FarmerBrothersEntitites.WorkorderSavedSearches.Where(x => x.UserID == userId).ToList();
            workOrderSearchModel.FollowUpCallList = FarmerBrothersEntitites.AllFBStatus.Where(f => f.StatusFor == "Follow Up Call" && f.Active == 1).OrderBy(f => f.StatusSequence).ToList();
            workOrderSearchModel.FollowUpCallList.Insert(0, new AllFBStatu()
            {
                Active = 1,
                FBStatus = "",
                FBStatusID = -1,
                StatusFor = "Follow Up Call",
                StatusSequence = 0

            });

            workOrderSearchModel.PriorityList = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Priority" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            workOrderSearchModel.ServiceCenterList = GetServiceCenters(workOrderSearchModel.TechType);
            workOrderSearchModel.TechniciansList = GetTechnicians(workOrderSearchModel.ServiceCompany);
            workOrderSearchModel.TechnicianIds = GetTechnicianIds(workOrderSearchModel.ServiceCompany);
            workOrderSearchModel.TeamLeadList = GetTeamLeadsList(workOrderSearchModel.ServiceCompany);
            workOrderSearchModel.TechId = null;

            return workOrderSearchModel;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "AssetExceptionSearch")]
        public ActionResult AssetExceptionSearch(WorkorderSearchModel workOrderSearchModel)
        {
            switch (workOrderSearchModel.Operation)
            {
                case WorkOrderSearchSubmitType.SEARCH:
                    //workOrderSearchModel.SearchResults = GetWorkOrderDataFromSP(workOrderSearchModel);
                    workOrderSearchModel.SearchResults = GetAssetExceptionDataFromSP(workOrderSearchModel);

                    //IEnumerable<WorkorderSearchResultModel> result = workOrderSearchModel.SearchResults.Where(item => FarmerBrothersEntitites.FBCBEs.Any(category => category.SerialNumber != item.SerialNo));
                    //var result = workOrderSearchModel.SearchResults.Where(x => !FarmerBrothersEntitites.FBCBEs.Any(y => y.SerialNumber == x.SerialNo)).Where(xy => xy.SerialNo != null && xy.SerialNo!="");
                    var result = workOrderSearchModel.SearchResults.Where(x => !FarmerBrothersEntitites.FBCBEs.Any(y => y.SerialNumber == x.SerialNo && y.CurrentCustomerId == x.CustomerID)).Where(xy => xy.SerialNo != null && xy.SerialNo.Trim() != " " && xy.SerialNo.Trim() != "");

                    workOrderSearchModel.SearchResults = result.ToList();

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
                    if (workOrderSearchModel.WOTypes == null)
                    {
                        workOrderSearchModel.WOTypes = new List<string>();
                    }
                    if (workOrderSearchModel.State == null)
                    {
                        workOrderSearchModel.State = new List<string>();
                    }

                    TempData["AssetExceptionSearchCriteria"] = workOrderSearchModel;
                    break;
            }

            workOrderSearchModel = PopulateWorkOrderSearchModelLists(workOrderSearchModel);

            if (workOrderSearchModel.State == null)
            {
                workOrderSearchModel.State = new List<string>();
                workOrderSearchModel.State.Add("");
            }
            return View("AssetExceptionReport", workOrderSearchModel);
        }

        public FileResult AssetExpReportToExcel()
        {
            WorkorderSearchModel workOrderSearchModel = new WorkorderSearchModel();
            IList<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();

            string gridModel = HttpContext.Request.Params["GridModel"];

            if (TempData["AssetExceptionSearchCriteria"] != null)
            {
                workOrderSearchModel = TempData["AssetExceptionSearchCriteria"] as WorkorderSearchModel;

                //searchResults = GetWorkOrderDataFromSP(workOrderSearchModel);
                searchResults = GetAssetExceptionDataFromSP(workOrderSearchModel);
                //var result = searchResults.Where(x => !FarmerBrothersEntitites.FBCBEs.Any(y => y.SerialNumber == x.SerialNo)).Where(xy => xy.SerialNo != null && xy.SerialNo != "");
                var result = workOrderSearchModel.SearchResults.Where(x => !FarmerBrothersEntitites.FBCBEs.Any(y => y.SerialNumber == x.SerialNo && y.CurrentCustomerId == x.CustomerID)).Where(xy => xy.SerialNo != null && xy.SerialNo.Trim() != " " && xy.SerialNo.Trim() != "");
                searchResults = result.ToList();
            }

            foreach (WorkorderSearchResultModel wo in searchResults)
            {
                if (!string.IsNullOrEmpty(wo.ErfOriginalScheduleDate))
                {
                    wo.AppointmentDate = wo.ErfOriginalScheduleDate;
                }
            }

            /*string gridModel = HttpContext.Request.Params["GridModel"];
            DataTable dt = new DataTable();
            if (TempData["AssetExceptionSearchCriteria"] != null)
            {
                workOrderSearchModel = TempData["AssetExceptionSearchCriteria"] as WorkorderSearchModel;
                dt = GetWorkOrderDataTable(workOrderSearchModel);
            }

            foreach (DataRow dr in dt.Rows)
            {
                if (dr["ERFNO"] != DBNull.Value)
                {
                    if (dr["ERFScheduleDate"] != DBNull.Value)
                    {
                        dr["AppointmentDate"] = dr["ERFScheduleDate"];
                    }
                }                
            }*/

            TempData["AssetExceptionSearchCriteria"] = workOrderSearchModel;
           
            string[] columns = {"WorkorderID","CustomerID","WorkorderCloseDate","CompletionDateTime","CustomerName","FieldServiceManager","DealerCompany",
                "CallTypeID","SerialNo","ProductNo","Manufacturer","FamilyAff","RegionNumber","CustomerBranch","Branch","RouteNumber" };

            //byte[] filecontent = ExcelExportHelper.ExportExcel(dt, "", true, columns);
            byte[] filecontent = ExcelExportHelper.ExportExcel<WorkorderSearchResultModel>(searchResults.ToList<WorkorderSearchResultModel>(), "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "AssetExceptionResults.xlsx");
        }

        private IList<WorkorderSearchResultModel> GetAssetExceptionDataFromSP(WorkorderSearchModel workorderSearchModel)
        {
            DataTable dt = GetAssetExceptionDataTable(workorderSearchModel);
            List<WorkorderSearchResultModel> searchResults = new List<WorkorderSearchResultModel>();
            foreach (DataRow dr in dt.Rows)
            {
                WorkorderSearchResultModel woresults = new WorkorderSearchResultModel(dr);
                searchResults.Add(woresults);
            }
            return searchResults;
        }

        private DataTable GetAssetExceptionDataTable(WorkorderSearchModel workorderSearchModel)
        {
            DataTable dt = new DataTable();


            string StartDate = string.IsNullOrEmpty(workorderSearchModel.DateFrom.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.DateFrom).ToString("yyyy-MM-dd");
            string EndDate = string.IsNullOrEmpty(workorderSearchModel.DateTo.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.DateTo).ToString("yyyy-MM-dd");
            string varState = "0";
            if (workorderSearchModel.State != null && workorderSearchModel.State.Count > 0)
            {
                varState = string.IsNullOrEmpty(workorderSearchModel.State[0]) ? "0" : Convert.ToString(workorderSearchModel.State[0]);
            }
            string EventStatus = "0";

            if (workorderSearchModel.Status != null && workorderSearchModel.Status.Count > 0)
            {
                EventStatus = string.IsNullOrEmpty(workorderSearchModel.Status[0]) ? "0" : Convert.ToString(workorderSearchModel.Status[0]);
            }

            string ApptStartDate = string.IsNullOrEmpty(workorderSearchModel.AppointmentDateFrom.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.AppointmentDateFrom).ToString("yyyy-MM-dd");
            string ApptEndDate = string.IsNullOrEmpty(workorderSearchModel.AppointmentDateTo.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.AppointmentDateTo).ToString("yyyy-MM-dd");
            string ArrvStartDate = string.IsNullOrEmpty(workorderSearchModel.ArrivalStartDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.ArrivalStartDate).ToString("yyyy-MM-dd");
            string ArrvEndDate = string.IsNullOrEmpty(workorderSearchModel.ArrivalEndDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.ArrivalEndDate).ToString("yyyy-MM-dd");
            string ComplStartDate = string.IsNullOrEmpty(workorderSearchModel.CompletionStartDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.CompletionStartDate).ToString("yyyy-MM-dd");
            string ComplEndDate = string.IsNullOrEmpty(workorderSearchModel.CompletionEndDate.ToString()) ? "0" : Convert.ToDateTime(workorderSearchModel.CompletionEndDate).ToString("yyyy-MM-dd");
            string CallTypeID = "0";
            if (workorderSearchModel.WOTypes != null && workorderSearchModel.WOTypes.Count > 0)
            {
                CallTypeID = string.IsNullOrEmpty(workorderSearchModel.WOTypes[0]) ? "0" : Convert.ToString(workorderSearchModel.WOTypes[0]);
            }

            string JDENO = string.IsNullOrEmpty(workorderSearchModel.CustomerId) ? "0" : workorderSearchModel.CustomerId.ToString();
            string PricingParentID = string.IsNullOrEmpty(workorderSearchModel.ParentAccount) ? "0" : workorderSearchModel.ParentAccount.ToString();

            string[] inputparam = { StartDate, EndDate, varState, EventStatus, ApptStartDate, ApptEndDate, ArrvStartDate, ArrvEndDate, ComplStartDate, ComplEndDate, CallTypeID, JDENO, PricingParentID };

            SqlHelper helper = new SqlHelper();
            dt = helper.GetAssetExceptionReportData(inputparam);


            return dt;
        }
        #endregion

        #region Prepaid Billing Report

        public ActionResult PrepaidBillingReport(int? isBack)
        {
            BillingReportModel BillingRptModel = new BillingReportModel();

            if (TempData["PrepaidBillingSearchCriteria"] != null && isBack == 1)
            {
                BillingRptModel = TempData["PrepaidBillingSearchCriteria"] as BillingReportModel;
                TempData["PrepaidBillingSearchCriteria"] = BillingRptModel;
            }
            else
            {
                BillingRptModel = new BillingReportModel();
                TempData["PrepaidBillingSearchCriteria"] = null;
            }

            IEnumerable<TechHierarchyView> Techlist = Utility.GetAllTechDataByBranchType(FarmerBrothersEntitites);

            List<TECH_HIERARCHY> newTechlistCollection = new List<TECH_HIERARCHY>();
            TECH_HIERARCHY techhierarchy;
            foreach (var item in Techlist.ToList())
            {
                techhierarchy = new TECH_HIERARCHY();
                techhierarchy.DealerId = Convert.ToInt32(item.TechID);
                techhierarchy.CompanyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.PreferredProvider.ToLower());
                newTechlistCollection.Add(techhierarchy);
            }

            TECH_HIERARCHY techhierarchy1 = new TECH_HIERARCHY()
            {
                DealerId = -1,
                CompanyName = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy1);

            BillingRptModel.Technicianlist = newTechlistCollection;

            DataTable dt = Security.GetFamilyAff();
            List<Technician> TechnicianAffs = new List<Technician>();
            foreach (DataRow dr in dt.Rows)
            {
                Technician tech = new Technician();
                if (dr[0].ToString() != "" && dr[0].ToString() != null)
                {
                    tech.TechID = dr[0].ToString();
                    if (dr[0].ToString() == "SPD")
                    {
                        tech.TechName = "Internal";
                        TechnicianAffs.Add(tech);
                    }
                    if (dr[0].ToString() == "SPT")
                    {
                        tech.TechName = "3rd Party";
                        TechnicianAffs.Add(tech);
                    }

                }

            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            BillingRptModel.FamilyAffs = TechnicianAffs;

            BillingRptModel.SearchResults = new List<BillingReportSearchResultModel>();

            return View(BillingRptModel);
        }

        
        public JsonResult SearchPrepaidBillingReport(BillingReportModel BillingRptModel)
        {
            if (string.IsNullOrEmpty(BillingRptModel.ParentACC))
            {
                BillingRptModel.ParentACC = "0";
            }

            if ((!BillingRptModel.BillingFromDate.HasValue)
                && !BillingRptModel.BillingToDate.HasValue)
            {
                TempData["PrepaidBillingSearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                BillingRptModel.SearchResults = GetPrepaidBillingreport(BillingRptModel);

                TempData["PrepaidBillingSearchCriteria"] = BillingRptModel;
                JsonResult result = Json(new
                {
                    rows = BillingRptModel.SearchResults
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;

                return result;
            }
        }

        private IList<BillingReportSearchResultModel> GetPrepaidBillingreport(BillingReportModel BillingRptModel)
        {
            List<BillingReportSearchResultModel> BillingData = new List<BillingReportSearchResultModel>();
            String DF = BillingRptModel.BillingFromDate.ToString();
            String DT = Convert.ToDateTime(BillingRptModel.BillingToDate).AddDays(1).ToString();
            String TL = BillingRptModel.DealerId.ToString();
            String FA = BillingRptModel.TechID.ToString();
            String PPID = BillingRptModel.ParentACC == null ? "0" : BillingRptModel.ParentACC.ToString();
            String AccountNo = BillingRptModel.AccountNo == null ? "0" : BillingRptModel.AccountNo.ToString();
            MarsViews mars = new MarsViews();
            decimal LaborCost = Convert.ToDecimal(ConfigurationManager.AppSettings["LaborCost"]);
            string ssql = @"select W.AuthTransactionId, W.FinalTransactionId, W.WorkorderID,W.WorkorderEntryDate,W.CustomerID,
                                        W.CustomerName As CompanyName,C.Address1,C.Address2,C.City, C.PostalCode,C.Route,C.Branch,WS.Techid,WS.TechName,
                                        W.WorkorderCallstatus, WD.StartDateTime,WD.ArrivalDateTime,WD.CompletionDateTime,'' as PurchaseOrder, 'N' as BillingID,
                                        WS.ScheduleDate,W.WorkorderEquipCount,W.CustomerState,W.ThirdPartyPO,W.Estimate,W.FinalEstimate,W.EstimateApprovedBy,
                                        W.OriginalWorkorderid,W.WorkorderCalltypeid as WorkorderCalltypeid,'' as TechCalled,W.AppointmentDate,WS.Techid as DispatchTechID,WS.TechName as DispatchTechName,
                                        W.NoServiceRequired,WD.NSRReason,C.PricingParentID,EQP.Category as Category,EQP.SerialNumber as SerialNumber,EQP.Model as Model,EQP.Manufacturer Manufacturer,EQP.Solutionid as Solutionid
                                        ,TBS.Notes As WorkPerformedNotes,W.CustomerName,WP.Quantity,WP.Sku,sku.SKUCost,sku.VendorCode, WP.Description,WP.Manufacturer as OrderSource,sku.Manufacturer as Supplier,
                                        WP.Total as PartsTotal, W.CustomerPO, WD.HardnessRating
                                        from workorder W 
                                        inner join Contact C on C.ContactID = W.CustomerID
                                        inner join WorkorderDetails WD on WD.WorkorderID = W.WorkorderID
                                        inner join WorkorderSchedule WS on WS.WorkorderID = W.WorkorderID
                                        left outer join WorkorderEquipment EQP on W.WorkorderID = EQP.WorkorderID
                                        left outer join WorkorderParts WP on W.Workorderid = WP.WorkorderID and WP.AssetID = EQP.Assetid
                                        left outer join sku on sku.Sku = WP.Sku  
                                    left outer join TMP_BlackBerry_SCFAssetInfo TBS on W.WorkorderID = TBS.WorkorderID and EQP.Assetid = TBS.AssetKey and W.WorkorderClosureConfirmationNo = TBS.ClosureConfirmationNo
                                    where  (len(FinalTransactionId) > 0 or len(AuthTransactionId) > 0 )";

            //string ssql = @"select W.AuthTransactionId, W.FinalTransactionId, W.WorkorderID,W.WorkorderEntryDate,W.CustomerID,
            //                        W.CustomerName As CompanyName,C.Address1,C.Address2,C.City, C.PostalCode,C.Route,C.Branch,WS.Techid,WS.TechName,
            //                        W.WorkorderCallstatus, WD.StartDateTime,WD.ArrivalDateTime,WD.CompletionDateTime,'' as PurchaseOrder, 'N' as BillingID,
            //                        WS.ScheduleDate,W.WorkorderEquipCount,W.CustomerState,W.ThirdPartyPO,W.Estimate,W.FinalEstimate,W.EstimateApprovedBy,
            //                        W.OriginalWorkorderid,W.WorkorderCalltypeid as WorkorderCalltypeid,'' as TechCalled,W.AppointmentDate,WS.Techid as DispatchTechID,WS.TechName as DispatchTechName,
            //                        W.NoServiceRequired,WD.NSRReason,C.PricingParentID,W.CustomerName,W.CustomerPO, WD.HardnessRating
            //                        from workorder W 
            //                        inner join Contact C on C.ContactID = W.CustomerID
            //                        inner join WorkorderDetails WD on WD.WorkorderID = W.WorkorderID
            //                        inner join WorkorderSchedule WS on WS.WorkorderID = W.WorkorderID
            //                        where (WS.AssignedStatus='Accepted' OR WS.AssignedStatus='Scheduled') AND 
            //                        (W.NoServiceRequired is null or W.NoServiceRequired = 0) AND (len(FinalTransactionId) > 0 or len(AuthTransactionId) > 0)";

            ssql = ssql + " and W.WorkorderEntryDate >='" + DF + "'";

            ssql = ssql + "  and  W.WorkorderEntryDate <'" + DT + "'";

            if (!string.IsNullOrEmpty(AccountNo) && AccountNo != "0")
            {
                ssql = ssql + " and C.ContactID =" + AccountNo;
            }

            if (BillingRptModel.DealerId > 0)
            {
                ssql = ssql + " and WS.Techid =" + TL;
            }

            if (FA == "SPD")
            {
                ssql = ssql + " and FamilyAff != 'SPT'";
            }
            if (FA == "SPT")
            {
                ssql = ssql + " and FamilyAff = 'SPT'";
            }

            if (!string.IsNullOrEmpty(PPID) && PPID != "0")
            {
                ssql = ssql + " and W.CustomerID IN (Select ContactID from Contact where PricingParentID = " + PPID + ")"; //'cast(@" + PPID + "as varchar(10)) ')";
            }

            ssql = ssql + " order by W.WorkorderID";

            DataTable dt = mars.fnTpspVendors(ssql);


            BillingReportSearchResultModel FBBillingSearchResult;
            foreach (DataRow dr in dt.Rows)
            {
                FBBillingSearchResult = new BillingReportSearchResultModel();
                string WorkorderID = dr["WorkorderID"] == DBNull.Value ? "" : dr["WorkorderID"].ToString();
                FBBillingSearchResult.WorkorderID = WorkorderID;
                FBBillingSearchResult.WorkorderEntryDate = dr["WorkorderEntryDate"] == DBNull.Value ? "" : dr["WorkorderEntryDate"].ToString();
                string customertId = dr["CustomerID"] == DBNull.Value ? "" : dr["CustomerID"].ToString();
                FBBillingSearchResult.CustomerID = customertId;
                FBBillingSearchResult.CompanyName = dr["CompanyName"] == DBNull.Value ? "" : dr["CompanyName"].ToString();
                FBBillingSearchResult.Address1 = dr["Address1"] == DBNull.Value ? "" : dr["Address1"].ToString();
                FBBillingSearchResult.Address2 = dr["Address2"] == DBNull.Value ? "" : dr["Address2"].ToString();
                FBBillingSearchResult.City = dr["City"] == DBNull.Value ? "" : dr["City"].ToString();
                FBBillingSearchResult.PostalCode = dr["PostalCode"] == DBNull.Value ? "" : dr["PostalCode"].ToString();
                FBBillingSearchResult.CustomerState = dr["CustomerState"] == DBNull.Value ? "" : dr["CustomerState"].ToString();
                FBBillingSearchResult.Route = dr["Route"] == DBNull.Value ? "" : dr["Route"].ToString();
                FBBillingSearchResult.Branch = dr["Branch"] == DBNull.Value ? "" : dr["Branch"].ToString();
                int techId = dr["Techid"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Techid"]);
                FBBillingSearchResult.Techid = techId == 0 ? "" : techId.ToString();
                FBBillingSearchResult.TechName = dr["TechName"] == DBNull.Value ? "" : dr["TechName"].ToString();
                FBBillingSearchResult.WorkorderCallstatus = dr["WorkorderCallstatus"] == DBNull.Value ? "" : dr["WorkorderCallstatus"].ToString();

                string arrivalDateTime, startDateTime, completionDateTime = "";
                startDateTime = dr["StartDateTime"] == DBNull.Value ? "" : dr["StartDateTime"].ToString();
                arrivalDateTime = dr["ArrivalDateTime"] == DBNull.Value ? "" : dr["ArrivalDateTime"].ToString();
                completionDateTime = dr["CompletionDateTime"] == DBNull.Value ? "" : dr["CompletionDateTime"].ToString();

                FBBillingSearchResult.StartDateTime = startDateTime;
                FBBillingSearchResult.ArrivalDateTime = arrivalDateTime;
                FBBillingSearchResult.CompletionDateTime = completionDateTime;

                FBBillingSearchResult.PurchaseOrder = dr["PurchaseOrder"] == DBNull.Value ? "" : dr["PurchaseOrder"].ToString();
                FBBillingSearchResult.BillingID = dr["BillingID"] == DBNull.Value ? "" : dr["BillingID"].ToString();
                FBBillingSearchResult.ScheduleDate = dr["ScheduleDate"] == DBNull.Value ? "" : dr["ScheduleDate"].ToString();
                FBBillingSearchResult.AppointmentDate = dr["AppointmentDate"] == DBNull.Value ? "" : dr["AppointmentDate"].ToString();
                FBBillingSearchResult.WorkorderEquipCount = dr["WorkorderEquipCount"] == DBNull.Value ? "" : dr["WorkorderEquipCount"].ToString();
                FBBillingSearchResult.ThirdPartyPO = dr["ThirdPartyPO"] == DBNull.Value ? "" : dr["ThirdPartyPO"].ToString();
                FBBillingSearchResult.Estimate = dr["Estimate"] == DBNull.Value ? "" : dr["Estimate"].ToString();
                FBBillingSearchResult.FinalEstimate = dr["FinalEstimate"] == DBNull.Value ? "" : dr["FinalEstimate"].ToString();
                FBBillingSearchResult.EstimateApprovedBy = dr["EstimateApprovedBy"] == DBNull.Value ? "" : dr["EstimateApprovedBy"].ToString();
                FBBillingSearchResult.OriginalWorkorderid = dr["OriginalWorkorderid"] == DBNull.Value ? "" : dr["OriginalWorkorderid"].ToString();
                FBBillingSearchResult.WorkorderCalltypeid = dr["WorkorderCalltypeid"] == DBNull.Value ? "" : dr["WorkorderCalltypeid"].ToString();
                FBBillingSearchResult.TechCalled = dr["TechCalled"] == DBNull.Value ? "" : dr["TechCalled"].ToString();
                FBBillingSearchResult.DispatchTechID = dr["DispatchTechID"] == DBNull.Value ? "" : dr["DispatchTechID"].ToString();
                FBBillingSearchResult.DispatchTechName = dr["DispatchTechName"] == DBNull.Value ? "" : dr["DispatchTechName"].ToString();
                FBBillingSearchResult.NoServiceRequired = dr["NoServiceRequired"] == DBNull.Value ? "" : dr["NoServiceRequired"].ToString();
                FBBillingSearchResult.NSRReason = dr["NSRReason"] == DBNull.Value ? "" : dr["NSRReason"].ToString();
                string parentId = dr["PricingParentID"] == DBNull.Value ? "" : dr["PricingParentID"].ToString();
                FBBillingSearchResult.PricingParentID = parentId;
                FBBillingSearchResult.Category = dr["Category"] == DBNull.Value ? "" : dr["Category"].ToString();
                FBBillingSearchResult.SerialNumber = dr["SerialNumber"] == DBNull.Value ? "" : dr["SerialNumber"].ToString();
                FBBillingSearchResult.Model = dr["Model"] == DBNull.Value ? "" : dr["Model"].ToString();
                FBBillingSearchResult.Manufacturer = dr["Manufacturer"] == DBNull.Value ? "" : dr["Manufacturer"].ToString();
                FBBillingSearchResult.Solutionid = dr["Solutionid"] == DBNull.Value ? "" : dr["Solutionid"].ToString();
                FBBillingSearchResult.WorkPerformedNotes = dr["WorkPerformedNotes"] == DBNull.Value ? "" : dr["WorkPerformedNotes"].ToString();

                FBBillingSearchResult.WorkPerformedNotes = dr["WorkPerformedNotes"] == DBNull.Value ? "" : dr["WorkPerformedNotes"].ToString();

                FBBillingSearchResult.CustomerName = dr["CustomerName"] == DBNull.Value ? "" : dr["CustomerName"].ToString();

                int Quantity = dr["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Quantity"]);
                FBBillingSearchResult.Quantity = Quantity.ToString();

                FBBillingSearchResult.Sku = dr["Sku"] == DBNull.Value ? "" : dr["Sku"].ToString();

                decimal SKUCost = dr["SKUCost"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["SKUCost"]);
                FBBillingSearchResult.SKUCost = SKUCost;

                FBBillingSearchResult.VendorCode = dr["VendorCode"] == DBNull.Value ? "" : dr["VendorCode"].ToString();
                FBBillingSearchResult.Description = dr["Description"] == DBNull.Value ? "" : dr["Description"].ToString();
                FBBillingSearchResult.OrderSource = dr["OrderSource"] == DBNull.Value ? "" : dr["OrderSource"].ToString();
                FBBillingSearchResult.Supplier = dr["Supplier"] == DBNull.Value ? "" : dr["Supplier"].ToString();

                double PrePaymentTravel = 0; double TravelTotal = 0; double LaborTotal = 0; double PartsTotal = 0; double SalesTax = 0; double TotalInvoice = 0;
                /*if (!string.IsNullOrEmpty(startDateTime) && !string.IsNullOrEmpty(arrivalDateTime))
                {
                    DateTime arrival = Convert.ToDateTime(arrivalDateTime);
                    DateTime strt = Convert.ToDateTime(startDateTime);
                    TimeSpan timeDiff = arrival.Subtract(strt);

                    if (!string.IsNullOrEmpty(parentId) && parentId == "9001239") //Updated as per the email "SEB - - Parent Acct #9001239"
                    {
                        TravelTotal = Math.Round(((Convert.ToDateTime(arrivalDateTime).Subtract(Convert.ToDateTime(startDateTime))).TotalMinutes) * 1.58333, 2);
                    }
                    else
                    {
                        TravelTotal = Math.Round(((Convert.ToDateTime(arrivalDateTime).Subtract(Convert.ToDateTime(startDateTime))).TotalMinutes) * 1.41666, 2);
                    }
                }
                else
                {
                    TravelTotal = 0;
                }

                if (!string.IsNullOrEmpty(arrivalDateTime) && !string.IsNullOrEmpty(completionDateTime))
                {
                    DateTime completion = Convert.ToDateTime(completionDateTime);
                    DateTime arrival = Convert.ToDateTime(arrivalDateTime);
                    TimeSpan timeDiff = completion.Subtract(arrival);

                    if (!string.IsNullOrEmpty(parentId) && parentId == "9001239") //Updated as per the email "SEB - - Parent Acct #9001239"
                    {
                        LaborTotal = Math.Round(((Convert.ToDateTime(completionDateTime).Subtract(Convert.ToDateTime(arrivalDateTime))).TotalMinutes) * 1.58333, 2);
                    }
                    else
                    {
                        LaborTotal = Math.Round(((Convert.ToDateTime(completionDateTime).Subtract(Convert.ToDateTime(arrivalDateTime))).TotalMinutes) * 1.41666, 2);
                    }

                }
                else
                {
                    LaborTotal = 0;
                }

                PartsTotal = Math.Round(Convert.ToDouble(Quantity * SKUCost), 2);
                TotalInvoice = Math.Round((TravelTotal + LaborTotal + PartsTotal), 2);

                FBBillingSearchResult.TravelTotal = TravelTotal.ToString();
                FBBillingSearchResult.LaborTotal = LaborTotal.ToString();
                FBBillingSearchResult.PartsTotal = PartsTotal.ToString();
                FBBillingSearchResult.TotalInvoice = TotalInvoice.ToString();*/

                List<WorkorderBillingDetail> WBDList = FarmerBrothersEntitites.WorkorderBillingDetails.Where(b => b.WorkorderId.ToString() == WorkorderID).ToList();
                foreach(WorkorderBillingDetail bill in WBDList)
                {
                    BillingItem bitem = FarmerBrothersEntitites.BillingItems.Where(b => b.BillingCode == bill.BillingCode).FirstOrDefault();

                    switch (bill.BillingCode)
                    {
                        case "091.4425":
                            PrePaymentTravel = Convert.ToDouble(bill.Quantity * bitem.UnitPrice);
                            break;
                        case "091.4430":
                            TravelTotal = Convert.ToDouble(bill.Quantity * bitem.UnitPrice);
                            break;
                        case "091.4435":
                            LaborTotal = Convert.ToDouble(bill.Quantity * bitem.UnitPrice);
                            break;
                        case "091.4420":
                            PartsTotal = Convert.ToDouble(bill.Quantity * SKUCost);
                            break;
                        case "800.2427":
                            WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID.ToString() == WorkorderID).FirstOrDefault();
                            StateTax st = FarmerBrothersEntitites.StateTaxes.Where(s => s.ZipCode == wo.CustomerZipCode).FirstOrDefault();
                            if (st != null)
                            {
                                SalesTax = Convert.ToDouble(st.StateRate);
                            }
                            //SalesTax = Convert.ToDouble(bill.Quantity * bitem.UnitPrice);
                            break;
                    }
                }

                TotalInvoice = Math.Round((TravelTotal + LaborTotal + PartsTotal + SalesTax), 2);

                FBBillingSearchResult.TravelTotal = TravelTotal.ToString();
                FBBillingSearchResult.LaborTotal = LaborTotal.ToString();
                FBBillingSearchResult.PartsTotal = PartsTotal.ToString();
                FBBillingSearchResult.SalesTaxTotal = SalesTax.ToString();
                FBBillingSearchResult.TotalInvoice = TotalInvoice.ToString();

                FBBillingSearchResult.AuthTransactionId = dr["AuthTransactionId"] == DBNull.Value ? "" : dr["AuthTransactionId"].ToString();
                FBBillingSearchResult.FinalTransactionId = dr["FinalTransactionId"] == DBNull.Value ? "" : dr["FinalTransactionId"].ToString();

                FBBillingSearchResult.CustomerPO = dr["CustomerPO"] == DBNull.Value ? "" : dr["CustomerPO"].ToString();
                FBBillingSearchResult.HardnessRating = dr["HardnessRating"] == DBNull.Value ? "" : dr["HardnessRating"].ToString();

                TECH_HIERARCHY techHView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == techId).FirstOrDefault();
                dynamic travelDetails = Utility.GetTravelDetailsBetweenZipCodes(techHView.PostalCode, WorkorderID);
                decimal distance = 0;
                if (travelDetails != null)
                {
                    var element = travelDetails.rows[0].elements[0];
                    distance = element == null ? 0 : (element.distance == null ? 0 : element.distance.value * (decimal)0.000621371192);
                    distance = Math.Round(distance, 0);
                }
                FBBillingSearchResult.Distance = distance.ToString();


                BillingData.Add(FBBillingSearchResult);
            }
            BillingRptModel.SearchResults = BillingData;

            return BillingData;
        }

       
        public JsonResult ClearPrepaidBillingResults()
        {
            TempData["PrepaidBillingSearchCriteria"] = null;
            return Json(new BillingReportSearchResultModel(), JsonRequestBehavior.AllowGet);
        }

        public void PrepaidBillingReportExcelExport()
        {
            try
            {
                BillingReportModel BillingModel = new BillingReportModel();

                IList<BillingReportSearchResultModel> searchResults = new List<BillingReportSearchResultModel>();

                if (TempData["PrepaidBillingSearchCriteria"] != null)
                {
                    BillingModel = TempData["PrepaidBillingSearchCriteria"] as BillingReportModel;
                    searchResults = GetPrepaidBillingreport(BillingModel);
                }

                TempData["PrepaidBillingSearchCriteria"] = BillingModel;
                string gridModel = HttpContext.Request.Params["GridModel"];

                var count = searchResults.Count;
                ExcelExport exp = new ExcelExport();
                GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
                IWorkbook book = exp.Export(properties, searchResults, "PrepaidBillingReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron", true);
                book.ActiveSheet.Range["AT"].NumberFormat = "$             #,##0.00";
                book.ActiveSheet.Range["AU"].NumberFormat = "$             #,##0.00";
                book.ActiveSheet.Range["AV"].NumberFormat = "$             #,##0.00";
                book.ActiveSheet.Range["AW"].NumberFormat = "$             #,##0.00";

                //Fit column width to data
                book.ActiveSheet.UsedRange.AutofitColumns();

                book.SaveAs("PrepaidBillingReports.xlsx", ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.Open);
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Escalation Report
        public ActionResult EscalationReport(int? isBack)
        {
            EscalationReportModel FBMonthlyModel = new EscalationReportModel();

            if (TempData["EscalationSearchCriteria"] != null && isBack == 1)
            {
                FBMonthlyModel = TempData["EscalationSearchCriteria"] as EscalationReportModel;
                TempData["EscalationSearchCriteria"] = FBMonthlyModel;
            }
            else
            {
                FBMonthlyModel = new EscalationReportModel();
                TempData["EscalationSearchCriteria"] = null;
            }
            FBMonthlyModel.SearchResults = new List<EscalationReportSearchResultModel>();
            return View(FBMonthlyModel);
        }

        public JsonResult SearchEscalationReport(EscalationReportModel FBMonthly)
        {
            if (string.IsNullOrWhiteSpace(FBMonthly.DateFrom.ToString())
                && string.IsNullOrWhiteSpace(FBMonthly.DateTo.ToString())
                )
            {
                TempData["EscalationSearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                FBMonthly.SearchResults = GetEscalationReport(FBMonthly);
                TempData["EscalationSearchCriteria"] = FBMonthly;
                return Json(FBMonthly.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        public List<EscalationReportSearchResultModel> GetEscalationReport(EscalationReportModel searchMonthly)
        {
            List<EscalationReportSearchResultModel> EscalationRportList = new List<EscalationReportSearchResultModel>();
            string sSql = @" select n.WorkorderId, w. WorkorderEntryDate, ISNULL(n.UserName, 'Sent From Email Link') as EscalatedBy, c.ESMName as EscalatedTo, n.EntryDate as EscalatedOn, 
                                        ws.TechName as EventSentTo, ws.AssignedStatus as TechStatus
                                        from NotesHistory n
                                        inner join workorder w on n.workorderid = w.workorderid
                                        inner join contact c on w.customerid = c.contactid
                                        inner join Workorderschedule ws on w.WorkorderID = ws.WorkorderID
                                        where n.workorderid is not null and  n.notes like '%escalation%' 
                                        and n.entrydate >= '" + searchMonthly.DateFrom+ @"'
                                        and n.entrydate <= '"+ searchMonthly.DateTo+ @"'
                                        order by workorderid desc";

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(sSql);
            foreach (DataRow dr in dt.Rows)
            {
                EscalationReportSearchResultModel rsltReport = new EscalationReportSearchResultModel();

                rsltReport.WorkOrderID = dr["WorkorderId"].ToString();
                rsltReport.EscalatedBy = dr["EscalatedBy"].ToString();
                rsltReport.EscalatedOn = dr["EscalatedOn"].ToString();
                rsltReport.EscalatedTo = dr["EscalatedTo"].ToString();
                rsltReport.WorkorderEntryDate = dr["WorkorderEntryDate"].ToString();
                rsltReport.EventSentTo = dr["EventSentTo"].ToString();
                rsltReport.TechStatus = dr["TechStatus"].ToString();

                EscalationRportList.Add(rsltReport);
            }


            return EscalationRportList;
        }
        public void EscalationReportExcelExport()
        {
            EscalationReportModel FBMonthly = new EscalationReportModel();

            IList<EscalationReportSearchResultModel> searchResults = new List<EscalationReportSearchResultModel>();

            if (TempData["EscalationSearchCriteria"] != null)
            {
                FBMonthly = TempData["EscalationSearchCriteria"] as EscalationReportModel;
                searchResults = GetEscalationReport(FBMonthly);
            }

            TempData["EscalationSearchCriteria"] = searchResults;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "EscalationReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        public JsonResult ClearEscalationReportResults()
        {
            TempData["EscalationSearchCriteria"] = null;
            return Json(new EscalationReportModel(), JsonRequestBehavior.AllowGet);
        }
        #endregion

    }

    public static class Extension
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
}