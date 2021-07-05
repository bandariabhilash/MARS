using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FarmerBrothers.Data;
using FarmerBrothers.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using FarmerBrothers.Utilities;
using LinqKit;
using System.Data;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.Globalization;

namespace FarmerBrothers.Controllers
{
    public class ReportsController : BaseController
    {
        int defaultFollowUpCall;
        public static List<NonServiceSearchResults> serialNumbersearchResults;
        public ReportsController()
        {
            AllFBStatu FarmarBortherStatus = FarmerBrothersEntitites.AllFBStatus.Where(a => a.FBStatus == "None" && a.StatusFor == "Follow Up Call").FirstOrDefault();
            if (FarmarBortherStatus != null)
            {
                defaultFollowUpCall = FarmarBortherStatus.FBStatusID;
            }
        }

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
                && !pmSchedulesModel.DateTo.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriority>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<PMSchedulesSearchResultModel> pMSchedules = GetPMSchedules(pmSchedulesModel);
                pmSchedulesModel.SearchResults = pMSchedules;
                TempData["SearchCriteria"] = pmSchedulesModel;
                return Json(pmSchedulesModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<PMSchedulesSearchResultModel> GetPMSchedules(PMSchedulesModel pmSchedulesModel)
        {
            //string ssql = "select WO.WorkorderID as FBNo,CustomerID as ContactID,CustomerName,ResponsibleTechid as TechID,ResponsibleTechName as TechName,DistributorName as ContactName,"
            //    + " CustomerPhone as Phone,WorkorderEntryDate as StartDate,'' as IntervalType, '' as NextRunDate,'' as IntervalDuration,NH.Notes"
            //    + " from dbo.WorkOrder WO"
            //    + " inner join dbo.NotesHistory NH on WO.WorkorderID = NH.WorkorderID where";

            //ssql = ssql + " wo.WorkorderEntryDate >='" + pmSchedulesModel.DateFrom + "'  ";
            //ssql = ssql + " and  wo.WorkorderCloseDate <'" + pmSchedulesModel.DateTo + "' ";
            // Workorderschedule.
            int customerId = 0;
            if (!string.IsNullOrEmpty(pmSchedulesModel.CustomerJDE))
            {
                customerId = Convert.ToInt32(pmSchedulesModel.CustomerJDE);
                //ssql = ssql + " and wo.CustomerID =" + id;
            }
            int techId = 0;
            if (!string.IsNullOrEmpty(pmSchedulesModel.TechJDE))
            {
                techId = Convert.ToInt32(pmSchedulesModel.TechJDE);
                // ssql = ssql + " and wo.ResponsibleTechid =" + techId;
            }

            //ssql = ssql + " Order By wo.WorkorderID";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.GetPMScheduleData(pmSchedulesModel.DateFrom.ToString(), pmSchedulesModel.DateTo.ToString(), 1, customerId, techId);
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
                //closurePartsSearchResultModel.CallTypeID = dr[""];
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

            //var TData = TempData["TMP_DispatchOrders"] as Category;

            //var FBDeltaCustomers = FarmerBrothersEntitites.Contacts.Where(c => (c.IsUnknownUser == 1) && (System.Data.Entity.DbFunctions.TruncateTime(c.DateCreated) >= searchcriteria.DateFrom && System.Data.Entity.DbFunctions.TruncateTime(c.DateCreated) <= searchcriteria.DateTo)).ToList();
            ////var FBDeltaCustomers = FarmerBrothersEntitites.Database.ExecuteSqlCommand("USP_FBDispatch_TimeElapsed", searchcriteria.DateFrom, searchcriteria.DateTo);
            //foreach (var v in FBDeltaCustomers)
            //{

            //    FBDeltaCustomersNCCCallSearchResultModel SearchResult = new FBDeltaCustomersNCCCallSearchResultModel()
            //    {
            //        AccountId = v.ContactID.ToString(),
            //        Company = v.CompanyName,
            //        Address = v.Address1,
            //        City = v.City,
            //        State = v.State,
            //        Zip = v.PostalCode,
            //        Phone = v.PhoneWithAreaCode

            //    };
            //    FBDeltaCustrest.Add(SearchResult);

            //}
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
                //IList<ZonePriority> zones = GetZones(techReportsModel);
                //techReportsModel.SearchResults = zones;
                TempData["SearchCriteria"] = techReportsModel;
                return Json(techReportsModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ClearTechAvailResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new TechAvailabilityReportsModel(), JsonRequestBehavior.AllowGet);
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
            return View(closurePartsModel);
        }

        public JsonResult SearchClosureParts(ClosurePartsModel closurePartsModel)
        {
            if ((!closurePartsModel.CloseDateStart.HasValue)
                && !closurePartsModel.CloseDateEnd.HasValue)
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ClosurePartsSearchResultModel>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<ClosurePartsSearchResultModel> closureParts = GetCloserParts(closurePartsModel);
                closurePartsModel.SearchResults = closureParts;
                TempData["SearchCriteria"] = closurePartsModel;
                return Json(closureParts, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<ClosurePartsSearchResultModel> GetCloserParts(ClosurePartsModel closurePartsModel)
        {
            string ssql = "SELECT dbo.WorkOrder.WorkorderID, dbo.WorkOrder.CustomerID, dbo.WorkOrder.WorkorderCallstatus as FulfillmentStatus, CONVERT(varchar(20), dbo.WorkOrder.WorkorderEntryDate, 101) AS EntryDate,"
     + " CONVERT(varchar(20), dbo.WorkOrder.WorkorderCloseDate, 101) AS Closedate, dbo.contact.SearchType, dbo.Contact.Distributor as DealerId, dbo.Contact.DistributorName as CompanyName,"
     + " dbo.Contact.FamilyAff,dbo.contact.Route,dbo.WorkorderParts.Quantity,dbo.WorkorderParts.PartsIssueid as part,dbo.WorkorderParts.Manufacturer as Vendor,dbo.WorkorderParts.Description FROM dbo.WorkOrder "
     + " INNER JOIN dbo.WorkorderSchedule ON dbo.WorkOrder.WorkorderID = dbo.WorkorderSchedule.WorkorderID "
     + " INNER JOIN dbo.WorkorderParts ON dbo.WorkOrder.WorkorderID = dbo.WorkorderParts.WorkorderID "
     + " INNER JOIN dbo.Contact ON dbo.WorkOrder.CustomerID = dbo.Contact.ContactID WHERE "
                //+ " INNER JOIN dbo.Contact ON dbo.WorkorderSchedule.TechId = dbo.Contact.Distributor and dbo.WorkOrder.CustomerID = dbo.Contact.ContactID WHERE "
     + " dbo.WorkOrder.WorkorderCallstatus = 'Closed' and dbo.WorkOrder.WorkorderID IN(Select WorkorderID from WorkorderParts)";

            ssql = ssql + " and dbo.WorkOrder.WorkorderCloseDate >='" + closurePartsModel.CloseDateStart + "'  ";
            ssql = ssql + " and  dbo.WorkOrder.WorkorderCloseDate <'" + closurePartsModel.CloseDateEnd + "' ";

            if (!string.IsNullOrEmpty(closurePartsModel.JDENo))
            {
                int id = Convert.ToInt32(closurePartsModel.JDENo);
                ssql = ssql + " and dbo.WorkOrder.CustomerID =" + id;
            }

            if (!string.IsNullOrEmpty(closurePartsModel.EntryNo))
            {
                int woId = Convert.ToInt32(closurePartsModel.EntryNo);
                ssql = ssql + " and dbo.WorkOrder.WorkorderID =" + woId;
            }

            ssql = ssql + " Order By dbo.WorkOrder.WorkorderID";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<ClosurePartsSearchResultModel> closerParts = new List<ClosurePartsSearchResultModel>();
            ClosurePartsSearchResultModel closurePartsSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                closurePartsSearchResultModel = new ClosurePartsSearchResultModel();
                closurePartsSearchResultModel.FBNo = dr["WorkorderID"].ToString();
                closurePartsSearchResultModel.JDENo = dr["CustomerID"].ToString();
                closurePartsSearchResultModel.FBStatus = dr["FulfillmentStatus"].ToString();
                closurePartsSearchResultModel.EntryDate = dr["EntryDate"].ToString();
                closurePartsSearchResultModel.CloseDate = dr["Closedate"].ToString();
                closurePartsSearchResultModel.CustomerType = dr["SearchType"].ToString();
                closurePartsSearchResultModel.ServiceCenterID = dr["DealerId"].ToString();
                closurePartsSearchResultModel.ServiceCompany = dr["CompanyName"].ToString();
                closurePartsSearchResultModel.FamilyAff = dr["FamilyAff"].ToString();
                closurePartsSearchResultModel.Quantity = dr["Quantity"].ToString();
                closurePartsSearchResultModel.ItemNo = dr["part"].ToString();
                closurePartsSearchResultModel.VendorNo = dr["Vendor"].ToString();
                closurePartsSearchResultModel.Description = dr["Description"].ToString();
                closurePartsSearchResultModel.Supplier = dr["Vendor"].ToString();
                //closurePartsSearchResultModel.CallTypeID = dr[""];
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

        public JsonResult SearchSerialNumber(SerialNumberModel serialNumberModel)
        {
            IList<SerialNumberSearchResultModel> serialNumber = GetserialNumber(serialNumberModel);
            serialNumberModel.SearchResults = serialNumber;
            TempData["SearchCriteria"] = serialNumberModel;
            return Json(serialNumber, JsonRequestBehavior.AllowGet);
        }

        private IList<SerialNumberSearchResultModel> GetserialNumber(SerialNumberModel serialNumberModel)
        {

            var predicate = PredicateBuilder.True<TMP_SerialNOReport>();

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
            //UnknownCustomerWorkOrderReports Unknownwrkordreports = new UnknownCustomerWorkOrderReports();
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

            // var equCount =  FarmerBrothersEntitites.V_EquipmentCount.GroupBy(o => o.DealerId).Select(g => new { DealerId = g.Key, EquipCount = g.Sum(i => i.EquipCount) }).Where(c => (c.DealerId > 0) && (System.Data.Entity.DbFunctions.TruncateTime(c.) >= searchcriteria.DateFrom && System.Data.Entity.DbFunctions.TruncateTime(c.WorkorderCloseDate) <= searchcriteria.DateTo)).ToList();


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
            // var machineCustomers = FarmerBrothersEntitites.V_EquipmentCount.Where(c => (c.DealerId > 0) && (System.Data.Entity.DbFunctions.TruncateTime(c.WorkorderCloseDate) >= searchcriteria.DateFrom && System.Data.Entity.DbFunctions.TruncateTime(c.WorkorderCloseDate) <= searchcriteria.DateTo))
            //    .Select(c=> new { c.DealerId,c.CompanyName,c.BranchName,c.BranchNumber,c.EquipCount })
            //     .GroupBy(c => new { c.DealerId, c.CompanyName, c.BranchName, c.BranchNumber }).ToList();
            foreach (var v in distinctTech)
            {

                MachineCountSearchResults SearchResult = new MachineCountSearchResults()
                {
                    DealerId = v.DealerId,
                    Company = v.CompanyName,
                    Branch = v.BranchNumber + " - " + v.BranchName,
                    //BranchName = v.BranchName,
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

            //List<TECH_HIERARCHY> Techlist = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP").Where(x => x.FamilyAff != "SPT").OrderBy(x => x.CompanyName).ToList();
            //TECH_HIERARCHY techhierarchy = new TECH_HIERARCHY()
            //{
            //    DealerId = -1,
            //    CompanyName = "Please select Technician"
            //};
            //Techlist.Insert(0, techhierarchy);
            //RejectCallModel.Technicianlist = Techlist;

            //DataTable dt = Security.GetFamilyAff();
            //List<Technician> TechnicianAffs = new List<Technician>();
            //foreach (DataRow dr in dt.Rows)
            //{
            //    Technician tech = new Technician();
            //    tech.TechID = dr[0].ToString();
            //    tech.TechName = dr[0].ToString();
            //    TechnicianAffs.Add(tech);
            //}

            //Technician tech1 = new Technician();
            //tech1.TechID = "All";
            //tech1.TechName = "All";
            //TechnicianAffs.Insert(0, tech1);

            //RejectCallModel.FamilyAffs = TechnicianAffs;
            IEnumerable<TechHierarchyView> Techlist = Utility.GetTechDataByBranchType(FarmerBrothersEntitites, null, null);

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
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                }

                TechnicianAffs.Add(tech);
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
            ssql = ssql + "TECH_HIERARCHY.State, TECH_HIERARCHY.FamilyAff, TECH_HIERARCHY.BranchName,TECH_HIERARCHY.BranchNumber FROM WorkorderSchedule";
            ssql = ssql + " INNER JOIN dbo.TECH_HIERARCHY ON WorkorderSchedule.TechId = TECH_HIERARCHY.DealerId";
            ssql = ssql + " where AssignedStatus = 'Declined'";
            //ssql = ssql + " join[dbo].[TECH_HIERARCHY] th on th.dealerid = thv.techid";

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

            //List<TECH_HIERARCHY> Techlist = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP").Where(x => x.FamilyAff != "SPT").OrderBy(x => x.CompanyName).ToList();
            //TECH_HIERARCHY techhierarchy = new TECH_HIERARCHY()
            //{
            //    DealerId = -1,
            //    CompanyName = "Please select Technician"
            //};
            //Techlist.Insert(0, techhierarchy);
            //OpenCallByTechModel.Technicianlist = Techlist;

            //DataTable dt = Security.GetFamilyAff();
            //List<Technician> TechnicianAffs = new List<Technician>();
            //foreach (DataRow dr in dt.Rows)
            //{
            //    Technician tech = new Technician();
            //    tech.TechID = dr[0].ToString();
            //    tech.TechName = dr[0].ToString();
            //    TechnicianAffs.Add(tech);
            //}

            //Technician tech1 = new Technician();
            //tech1.TechID = "All";
            //tech1.TechName = "All";
            //TechnicianAffs.Insert(0, tech1);

            //OpenCallByTechModel.FamilyAffs = TechnicianAffs;
            IEnumerable<TechHierarchyView> Techlist = Utility.GetTechDataByBranchType(FarmerBrothersEntitites, null, null);

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
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                }

                TechnicianAffs.Add(tech);
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
                TempData["SearchCriteria"] = OpenCallByTechModel;
                return Json(OpenCallByTechModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<OpenCallByTechSearchResultModel> GetOpenCallByTech(OpenCallByTechModel OpenCallByTechModel)
        {
            string ssql = "SELECT dbo.ESMDSMRSM.Region, dbo.ESMDSMRSM.ESMName, dbo.WorkorderSchedule.TechId, dbo.TECH_HIERARCHY.CompanyName AS DispatchCompany,";

            ssql = ssql + " COUNT(dbo.WorkorderSchedule.TechId) AS EventCount FROM dbo.WorkorderSchedule INNER JOIN dbo.TECH_HIERARCHY ON dbo.WorkorderSchedule.TechId = dbo.TECH_HIERARCHY.DealerId INNER JOIN";

            ssql = ssql + " dbo.Workorder ON dbo.WorkorderSchedule.WorkorderID = dbo.Workorder.WorkorderID INNER JOIN dbo.ESMDSMRSM on TECH_HIERARCHY.BranchNumber COLLATE DATABASE_DEFAULT = ESMDSMRSM.BranchNO COLLATE DATABASE_DEFAULT";

            ssql = ssql + " Where(dbo.Workorder.WorkorderCallstatus <> 'Closed')";
            //ssql = ssql + " join[dbo].[TECH_HIERARCHY] th on th.dealerid = thv.techid";

            ssql = ssql + " and WorkorderSchedule.ScheduleDate >='" + OpenCallByTechModel.OpenCallByTechFromDate + "'  ";
            ssql = ssql + " and  WorkorderSchedule.ScheduleDate <'" + OpenCallByTechModel.OpenCallByTechToDate + "' ";

            if (OpenCallByTechModel.DealerId > 0)
            {
                ssql = ssql + " and TECH_HIERARCHY.dealerid =" + OpenCallByTechModel.DealerId;
            }

            if (OpenCallByTechModel.TechID != "All")
            {
                ssql = ssql + " and TECH_HIERARCHY.familyAff='" + OpenCallByTechModel.TechID + "'";
            }


            ssql = ssql + " GROUP BY  dbo.ESMDSMRSM.Region, dbo.ESMDSMRSM.ESMName,dbo.TECH_HIERARCHY.CompanyName,  dbo.WorkorderSchedule.TechId ORDER BY EventCount DESC";
            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(ssql);
            List<OpenCallByTechSearchResultModel> OpenCallByTechParts = new List<OpenCallByTechSearchResultModel>();
            OpenCallByTechSearchResultModel OpenCallByTechSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                OpenCallByTechSearchResultModel = new OpenCallByTechSearchResultModel();
                OpenCallByTechSearchResultModel.Region = dr["Region"].ToString();
                OpenCallByTechSearchResultModel.ESMName = dr["ESMName"].ToString();
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
            List<NonServiceSearchResults> nonServiceEventResultsList = new List<NonServiceSearchResults>();
            string sSql = " SELECT [FB_DEV].[dbo].[FBCallReason].Description, COUNT(*)";
            sSql = sSql + " FROM [FB_DEV].[dbo].[NonServiceworkorder]";
            sSql = sSql + " INNER JOIN [FB_DEV].[dbo].[FBCallReason] ON [FB_DEV].[dbo].[NonServiceworkorder].[CallReason]=[FB_DEV].[dbo].[FBCallReason].[SourceCode]";
            sSql = sSql + " WHERE [FB_DEV].[dbo].[NonServiceworkorder].[CreatedDate] >= '" + nonServiceEventModel.DateFrom + "' and [FB_DEV].[dbo].[NonServiceworkorder].[CreatedDate] <= '" + nonServiceEventModel.DateTo + "'";
            sSql = sSql + " GROUP BY [FB_DEV].[dbo].[FBCallReason].Description";
            sSql = sSql + " ORDER BY [FB_DEV].[dbo].[FBCallReason].Description";

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
            string[] columns = { "ServiceType", "Count", "EventID", "CustomerType", "Description", "EventStatus", "EntryDate", "CompanyName", "Address1", "City", "State", "PostalCode"
                               , "EmailAddress", "Notes", "Route"};
            byte[] filecontent = ExcelExportHelper.ExportExcel(serialNumbersearchResults, "Non-Service Event Details", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "Non-Service-Events.xlsx");
        }  

        private List<NonServiceSearchResults> GetNonServiceSpecificDataResults(string DateFrom, string DateTo, string description)
        {
            List<NonServiceSearchResults> nonServiceEventResultsList = new List<NonServiceSearchResults>();
            string sSql = @" SELECT dbo.FBCallReason.Description, dbo.NonServiceworkorder.WorkOrderID,dbo.NonServiceworkorder.CreatedDate,
                             dbo.Contact.CompanyName, dbo.Contact.Address1, dbo.Contact.City, dbo.Contact.State, dbo.Contact.PostalCode,
                             STUFF((SELECT CHAR(13) + CHAR(10), dbo.NotesHistory.Notes as [text()]
                                FROM dbo.NotesHistory
                                WHERE dbo.NotesHistory.NonServiceWorkorderID = dbo.NonServiceworkorder.WorkOrderID
                                FOR XML PATH('')),1,1,'') [Notes]
                                    FROM dbo.NonServiceworkorder
                             INNER JOIN dbo.Contact ON dbo.Contact.ContactID = dbo.NonServiceworkorder.CustomerID
                             LEFT JOIN dbo.NotesHistory ON dbo.NotesHistory.NonServiceWorkorderID = dbo.NonServiceworkorder.WorkOrderID
                             INNER JOIN dbo.FBCallReason
                             ON dbo.NonServiceworkorder.CallReason= dbo.FBCallReason.SourceCode WHERE";

            if (description.ToUpper() != "ALL")
            {
                sSql = sSql + " dbo.FBCallReason.Description = '" + description + "' and ";
            }

            sSql = sSql + " dbo.NonServiceworkorder.CreatedDate >= '" + DateFrom + "'";
            sSql = sSql + " and dbo.NonServiceworkorder.CreatedDate <= '" + DateTo + "'";
            sSql = sSql + " GROUP BY dbo.FBCallReason.Description, dbo.NonServiceworkorder.WorkOrderID,dbo.NonServiceworkorder.CreatedDate,";
            sSql = sSql + " dbo.Contact.CompanyName, dbo.Contact.Address1, dbo.Contact.City, dbo.Contact.State, dbo.Contact.PostalCode";

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(sSql);
            NonServiceSearchResults nonServiceSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                nonServiceSearchResultModel = new NonServiceSearchResults();
                nonServiceSearchResultModel.Description = dr["Description"].ToString();
                nonServiceSearchResultModel.EventID = dr["WorkOrderID"].ToString();
                nonServiceSearchResultModel.EventStatus = "Open";
                nonServiceSearchResultModel.EntryDate = dr["CreatedDate"].ToString();
                nonServiceSearchResultModel.CompanyName = dr["CompanyName"].ToString();
                nonServiceSearchResultModel.Address1 = dr["Address1"].ToString();
                nonServiceSearchResultModel.City = dr["City"].ToString();
                nonServiceSearchResultModel.State = dr["State"].ToString();
                nonServiceSearchResultModel.PostalCode = dr["PostalCode"].ToString();
                nonServiceSearchResultModel.Notes = dr["Notes"].ToString();
                nonServiceEventResultsList.Add(nonServiceSearchResultModel);
            }

            return nonServiceEventResultsList;
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

            IEnumerable<TechHierarchyView> Techlist = Utility.GetTechDataByBranchType(FarmerBrothersEntitites, null, null);

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
                }
                if (dr[0].ToString() == "SPT")
                {
                    tech.TechName = "3rd Party";
                }

                TechnicianAffs.Add(tech);
            }

            Technician tech1 = new Technician();
            tech1.TechID = "All";
            tech1.TechName = "All";
            TechnicianAffs.Insert(0, tech1);

            techReportsModel.TechnicianAffs = TechnicianAffs;
            return View("TechnicianProductivityReport", techReportsModel);
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

            workOrderSearchModel.Status = new List<string>();
            workOrderSearchModel.FollowupCall = defaultFollowUpCall;
            workOrderSearchModel.Status.Add("");
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
                //techReportsModel.SearchResults = zones;
                TempData["SearchCriteria"] = techReportsModel;
                return Json(techReportsModel.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        private IList<TechnicianSearchResultModel> GetTechncians(TechReportsModel techReportsModel)
        {
            List<TechnicianSearchResultModel> techResults = new List<TechnicianSearchResultModel>();
            string sSql = " SELECT TOP 100 PERCENT dbo.V_UniqueInvoiceTimingsByDealerID.FamilyAff,dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId, dbo.V_UniqueInvoiceTimingsByDealerID.Region,dbo.V_UniqueInvoiceTimingsByDealerID.ESM,";
            sSql = sSql + " dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechName, dbo.V_UniqueInvoiceTimingsByDealerID.BranchNumber,dbo.V_UniqueInvoiceTimingsByDealerID.BranchName, SUM(ISNULL(DATEDIFF(n, dbo.V_UniqueInvoiceTimingsByDealerID.StartDateTime,";
            sSql = sSql + " dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0)) AS TotalTimeByTech, COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId) AS ToatlEventsByTech,";
            sSql = sSql + " SUM(ISNULL(DATEDIFF(n,dbo.V_UniqueInvoiceTimingsByDealerID.StartDateTime, dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0)) / COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId) AS AvgMinsPerCall, ";
            sSql = sSql + " SUM(ISNULL(DATEDIFF(n,dbo.V_UniqueInvoiceTimingsByDealerID.ArrivalDateTime, dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0)) / COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId) AS AvgMinsOnsitePerCall, ";
            sSql = sSql + " dbo.getElapsedString(SUM(ISNULL(DATEDIFF(n, dbo.V_UniqueInvoiceTimingsByDealerID.ArrivalDateTime, dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0))/ COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId)) AS ElapsedTimeOnSite ,";
            sSql = sSql + " dbo.getElapsedString(SUM(ISNULL(DATEDIFF(n, dbo.V_UniqueInvoiceTimingsByDealerID.StartDateTime, dbo.V_UniqueInvoiceTimingsByDealerID.CompletionDateTime), 0))/ COUNT(dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId)) AS ElapsedTime FROM dbo.Workorder INNER JOIN";
            sSql = sSql + " dbo.V_UniqueInvoiceTimingsByDealerID ON dbo.Workorder.WorkorderId = dbo.V_UniqueInvoiceTimingsByDealerID.WorkorderId WHERE     (dbo.Workorder.WorkorderCallstatus = N'Closed') AND ";

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


            sSql = sSql + " GROUP BY dbo.V_UniqueInvoiceTimingsByDealerID.FamilyAff,  Region,ESM,dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechName, dbo.V_UniqueInvoiceTimingsByDealerID.ResponsibleTechId,dbo.V_UniqueInvoiceTimingsByDealerID.BranchNumber,dbo.V_UniqueInvoiceTimingsByDealerID.BranchName ORDER BY ElapsedTime DESC";

            MarsViews mars = new MarsViews();
            DataTable dt = mars.fnTpspVendors(sSql);
            TechnicianSearchResultModel closurePartsSearchResultModel;
            foreach (DataRow dr in dt.Rows)
            {
                closurePartsSearchResultModel = new TechnicianSearchResultModel();
                closurePartsSearchResultModel.Region = dr["Region"].ToString();
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
                //closurePartsSearchResultModel.CallTypeID = dr[""];
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
            WorkorderSavedSearch savedSearch;
            switch (workOrderSearchModel.Operation)
            {
                case WorkOrderSearchSubmitType.SAVE:
                    if (ModelState.IsValid)
                    {
                        savedSearch = new WorkorderSavedSearch();
                        savedSearch = workOrderSearchModel.GetSavedSearch(savedSearch);
                        FarmerBrothersEntitites.WorkorderSavedSearches.Add(savedSearch);
                        FarmerBrothersEntitites.SaveChanges();
                        workOrderSearchModel.FromSavedSearch(new WorkorderSavedSearch());   //Clear search hmodel
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
                                workOrderSearchModel.FromSavedSearch(new WorkorderSavedSearch());   //Clear search hmodel
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
                    workOrderSearchModel.FromSavedSearch(new WorkorderSavedSearch());   //Clear search hmodel
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
                    workOrderSearchModel.SearchResults = GetWorkOrderData(workOrderSearchModel);
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
            return View("ReportsSearch", workOrderSearchModel);
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
                searchResults.Add(new WorkorderSearchResultModel(workOrder, FarmerBrothersEntitites));
            }

            if (searchResults.Count > 0 && originalWorkOrder != null)
            {
                searchResults.Insert(0, new WorkorderSearchResultModel(originalWorkOrder, FarmerBrothersEntitites));
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
            //workOrderSearchModel.WorkOrderStatusList.Insert(0, new AllFBStatu()
            //{
            //    Active = 1,
            //    FBStatus = "",
            //    FBStatusID = -1,
            //    StatusFor = "Work Order Status",
            //    StatusSequence = 0

            //});

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
            //workOrderSearchModel.Fsms = GetFsmData();
            //workOrderSearchModel.Tsms = GetTsmData();
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

            if (serviceCenterId > 0)
            {
                //IEnumerable<TechHierarchyView> technicians = Utility.GetTechDataByServiceCenterId(FarmerBrothersEntitites, Convert.ToInt32(serviceCenterId));
                //foreach (TechHierarchyView technician in technicians)
                //{
                //    if (string.Compare(technician.ServiceCenter_Desc, "TPSP Branch", 0) != 0)
                //    {
                //        technicianModel = new TechModel(technician);
                //        techniciansList.Add(technicianModel);
                //    }
                //}
            }

            return techniciansList.OrderBy(t => t.TechId).ToList();
        }

        private IList<TechModel> GetTechnicians(double serviceCenterId)
        {
            IList<TechModel> techniciansList = new List<TechModel>();
            TechModel technicianModel = new TechModel(new TechHierarchyView());
            techniciansList.Add(technicianModel);

            if (serviceCenterId > 0)
            {
                //IEnumerable<TechHierarchyView> technicians = Utility.GetTechDataByServiceCenterId(FarmerBrothersEntitites, Convert.ToInt32(serviceCenterId));
                //foreach (TechHierarchyView technician in technicians)
                //{
                //    if (string.Compare(technician.ServiceCenter_Desc, "TPSP Branch", 0) != 0)
                //    {
                //        technicianModel = new TechModel(technician);
                //        techniciansList.Add(technicianModel);
                //    }
                //}
            }

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
    }

    public static class Extension
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
}