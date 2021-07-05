using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class PMUploadMaintenanceController : BaseController
    {        
        public ActionResult PMUploadMaintenance()
        {
            PMSchedulesModel pmSchedulesModel = new PMSchedulesModel();

            pmSchedulesModel.intervalTypeList = new List<CategoryModel>();           
            pmSchedulesModel.intervalTypeList.Add(new CategoryModel(""));
            pmSchedulesModel.intervalTypeList.Add(new CategoryModel("Weekly"));
            pmSchedulesModel.intervalTypeList.Add(new CategoryModel("Monthly"));
            pmSchedulesModel.intervalTypeList.Add(new CategoryModel("Yearly"));

            pmSchedulesModel.States = Utility.GetStates(FarmerBrothersEntitites);

            pmSchedulesModel.TechTypeList = new List<string>();
            pmSchedulesModel.TechTypeList.Add("");
            pmSchedulesModel.TechTypeList.Add("Internal");
            pmSchedulesModel.TechTypeList.Add("TPSP");
            pmSchedulesModel.TechTypeList.Add("Nearest Tech");

            pmSchedulesModel.ServiceCenterList = GetServiceCenters(pmSchedulesModel.TechType);

            pmSchedulesModel.TaggedCategories = new List<CategoryModel>();
            IQueryable<string> categories = FarmerBrothersEntitites.Categories.Where(s => s.Active == 1).OrderBy(s => s.ColUpdated).Select(s => s.CategoryCode + " - " + s.CategoryDesc);
            foreach (string category in categories)
            {
                pmSchedulesModel.TaggedCategories.Add(new CategoryModel(category));
            }
            pmSchedulesModel.TaggedCategories.Insert(0, new CategoryModel(""));

            //pmSchedulesModel.intervalTypeList = intervalList;
            return View(pmSchedulesModel);
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
        
        public JsonResult SearchPMSchedule(PMSchedulesModel pmSchedulesModel)
        {
            //if ((!pmSchedulesModel.DateFrom.HasValue)
            //    && !pmSchedulesModel.DateTo.HasValue && string.IsNullOrEmpty(pmSchedulesModel.CustomerJDE))
            if (string.IsNullOrEmpty(pmSchedulesModel.CustomerJDE))
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<PMSchedulesSearchResultModel>(), JsonRequestBehavior.AllowGet);
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
                pMSchedulesSearchResultModel.ID = dr["UniqueID"] == null ? 0 : Convert.ToInt32(dr["UniqueID"]);
                pMSchedulesSearchResultModel.FBNo = dr["EventID"] == null ? "" : dr["EventID"].ToString();
                pMSchedulesSearchResultModel.ContactID = dr["ContactID"] == null ? "" : dr["ContactID"].ToString();
                pMSchedulesSearchResultModel.CustomerName = dr["CustomerName"] == null ? "" : dr["CustomerName"].ToString();
                pMSchedulesSearchResultModel.TechID = dr["TechID"] == null ? "" : dr["TechID"].ToString();
                pMSchedulesSearchResultModel.TechName = dr["TechName"] == null ? "" : dr["TechName"].ToString();
                pMSchedulesSearchResultModel.ContactName = dr["ContactName"] == null ? "" : dr["ContactName"].ToString();
                pMSchedulesSearchResultModel.Phone = dr["Phone"] == null ? "" : dr["Phone"].ToString();
                pMSchedulesSearchResultModel.StartDate = dr["StartDate"] == null ? "" : dr["StartDate"].ToString();
                pMSchedulesSearchResultModel.IntervalType = dr["IntervalType"] == null ? "" : dr["IntervalType"].ToString();
                pMSchedulesSearchResultModel.NextRunDate = dr["NextRunDate"] == null ? "" : dr["NextRunDate"].ToString();
                pMSchedulesSearchResultModel.IntervalDuration = dr["IntervalDuration"] == null ? "" : dr["IntervalDuration"].ToString();
                pMSchedulesSearchResultModel.Notes = dr["Notes"] == null ? "" : dr["Notes"].ToString();
                pMSchedulesSearchResultModel.IsActive = Convert.ToBoolean(dr["IsActive"]);
                pMSchedulesSearchResultModel.Category = dr["EquipmentModel"] == null ? "" : dr["EquipmentModel"].ToString();
                pmScheduleParts.Add(pMSchedulesSearchResultModel);
            }
            return pmScheduleParts;
        }
        public JsonResult ClearPMScheduleResults()
        {
            TempData["SearchCriteria"] = null;
            TempData["PMUploadMaintenanceData"] = null;
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
            GridProperties properties = ConvertGridObject(gridModel);//(GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, searchResults, "PMScheduleReports.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        [HttpPost]
        public ActionResult PMScheduleResultsDataUpdate(PMSchedulesSearchResultModel value)
        {
            IList<PMSchedulesSearchResultModel> PMUploadContactsData = TempData["PMUploadMaintenanceData"] as IList<PMSchedulesSearchResultModel>;
            if (PMUploadContactsData == null)
            {
                PMUploadContactsData = new List<PMSchedulesSearchResultModel>();
            }

            PMUploadContactsData.Add(value);
            TempData["PMUploadMaintenanceData"] = PMUploadContactsData;
            TempData.Keep("PMUploadMaintenanceData");
            //return Json(value, JsonRequestBehavior.AllowGet);
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { message = "PMSchedule Row "+ value.ContactID +" Save Success" };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult SavePMUploadsData()
        {
            try
            {
                IList<PMSchedulesSearchResultModel> pmUploadsDataList = TempData["PMUploadMaintenanceData"] as IList<PMSchedulesSearchResultModel>;

                if (pmUploadsDataList != null)
                {
                    foreach (PMSchedulesSearchResultModel item in pmUploadsDataList)
                    {
                        List<Contact_PMUploadsALL> pmContactsList = FarmerBrothersEntitites.Contact_PMUploadsALL.Where(c => c.ContactID.ToString() == item.ContactID && c.EquipmentModel.ToLower() == item.Category.ToLower()).ToList();//.ForEach(cc => cc.IsActive = item.IsActive);

                        foreach (Contact_PMUploadsALL cpm in pmContactsList)
                        {
                            if (cpm != null)
                            {
                                cpm.ContactName = item.ContactName;
                                cpm.CustomerName = item.CustomerName;
                                cpm.IntervalType = item.IntervalType;
                                cpm.StartDate = DateTime.ParseExact(item.StartDate, "dd/MM/yyyy", null);
                                cpm.IntervalDuration = Convert.ToInt32(item.IntervalDuration);
                                //cpm.NextRunDate = DateTime.ParseExact(item.NextRunDate, "dd/MM/yyyy", null);
                                cpm.IsActive = item.IsActive;
                                cpm.Notes = item.Notes;
                            }
                            else
                            {
                                Contact_PMUploadsALL cpmData = new Contact_PMUploadsALL();
                                cpmData.ContactName = item.ContactName;
                                cpmData.CustomerName = item.CustomerName;
                                cpmData.IntervalType = item.IntervalType;
                                cpmData.StartDate = DateTime.ParseExact(item.StartDate, "dd/MM/yyyy", null);
                                cpm.IntervalDuration = Convert.ToInt32(item.IntervalDuration);
                                //cpmData.NextRunDate = DateTime.ParseExact(item.NextRunDate, "dd/MM/yyyy", null);
                                cpmData.IsActive = item.IsActive;
                                cpmData.Notes = item.Notes;
                                FarmerBrothersEntitites.Contact_PMUploadsALL.Add(cpmData);
                            }
                        }
                    }
                }
                FarmerBrothersEntitites.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            JsonResult jsonResult = new JsonResult();
            //jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult AddPMSchedule(PMSchedulesModel NewPMScheduleData)
        {
            JsonResult jsonResult = new JsonResult();
            int cntcId = string.IsNullOrEmpty(NewPMScheduleData.AccountNumber) ? 0 : Convert.ToInt32(NewPMScheduleData.AccountNumber);
            Contact con = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == cntcId && (c.SearchType.ToLower() == "c"
                                                                                                                                                        || c.SearchType.ToLower() == "ca"
                                                                                                                                                            || c.SearchType.ToLower() == "xc"
                                                                                                                                                            || c.SearchType.ToLower() == "xca"
                                                                                                                                                            || c.SearchType.ToLower() == "xci"
                                                                                                                                                            || c.SearchType.ToLower() == "ccs"
                                                                                                                                                            || c.SearchType.ToLower() == "cfs"
                                                                                                                                                            || c.SearchType.ToLower() == "cb"
                                                                                                                                                            || c.SearchType.ToLower() == "ce"
                                                                                                                                                            || c.SearchType.ToLower() == "cfd"
                                                                                                                                                            || c.SearchType.ToLower() == "pfs")).FirstOrDefault();
            int tchId = 0;

            if (NewPMScheduleData.TechType.ToLower() == "nearest tech")
            {
                List<TechDispatchWithDistance> nearestTechList = Utility.GetClosestTechDispatchWithDistance(FarmerBrothersEntitites, con.PostalCode).ToList();
                tchId = nearestTechList.OrderBy(t => t.Distance)
                   .Select(g => g.ServiceCenterId).FirstOrDefault();                
            }
            else
            {
                tchId = string.IsNullOrEmpty(NewPMScheduleData.serviceCenter) ? 0 : Convert.ToInt32(NewPMScheduleData.serviceCenter);
            }

            TECH_HIERARCHY th = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == tchId && t.SearchType.ToLower() == "sp").FirstOrDefault();
            Contact_PMUploadsALL cpa = FarmerBrothersEntitites.Contact_PMUploadsALL.Where(c => c.ContactID == cntcId
                                                                                                                                                                && c.EquipmentModel == NewPMScheduleData.Catregory).FirstOrDefault();
            
            if (con == null)
            {
                jsonResult.Data = new { message = "Customer is Not Active", success = false };
            }
            else if (th == null)
            {
                jsonResult.Data = new { message = "Tech is Not Active", success = false };
            }
            else if(cpa != null)
            {
                jsonResult.Data = new { message = "PM Upload Already Exists with the Given Account# and Equipment Type", success = false };
            }
            else if (con != null && th != null)
            {
                Contact_PMUploadsALL cpmua = new Contact_PMUploadsALL();
                cpmua.ContactID = string.IsNullOrEmpty(NewPMScheduleData.AccountNumber) ? 0 : Convert.ToInt32(NewPMScheduleData.AccountNumber);
                cpmua.ContactName = string.IsNullOrEmpty(NewPMScheduleData.EventContact) ? "" : NewPMScheduleData.EventContact;
                cpmua.CustomerName = string.IsNullOrEmpty(con.CompanyName) ? "" : con.CompanyName;
                cpmua.ZipCode = string.IsNullOrEmpty(con.PostalCode) ? "" : con.PostalCode;
                cpmua.DayLightSaving = "Y";
                cpmua.Description = string.IsNullOrEmpty(NewPMScheduleData.Catregory) ? "" : NewPMScheduleData.Catregory;
                cpmua.EquipmentLocation = "N/A";
                cpmua.EquipmentModel = string.IsNullOrEmpty(NewPMScheduleData.Catregory) ? "" : NewPMScheduleData.Catregory;
                cpmua.EventCreated = 1;
                cpmua.IntervalDuration = NewPMScheduleData.IntervalDuration == 0 ? 1 : Convert.ToInt32(NewPMScheduleData.IntervalDuration);
                cpmua.IntervalType = string.IsNullOrEmpty(NewPMScheduleData.IntervalType) ? "" : NewPMScheduleData.IntervalType;
                cpmua.IsActive = true;
                cpmua.Notes = string.IsNullOrEmpty(NewPMScheduleData.Notes) ? "" : NewPMScheduleData.Notes;
                cpmua.Phone = string.IsNullOrEmpty(NewPMScheduleData.ContactPhone) ? "" : NewPMScheduleData.ContactPhone;
                cpmua.StartDate = Convert.ToDateTime(NewPMScheduleData.StartDate);
                cpmua.TechID = th.DealerId;//string.IsNullOrEmpty(NewPMScheduleData.TechJDE) ? 0 : Convert.ToInt32(NewPMScheduleData.TechJDE);
                cpmua.TechName = string.IsNullOrEmpty(th.CompanyName) ? "" : th.CompanyName;//string.IsNullOrEmpty(NewPMScheduleData.TechName) ? "" : NewPMScheduleData.TechName;
                //cpmua.TPSP = ;

                FarmerBrothersEntitites.Contact_PMUploadsALL.Add(cpmua);
                FarmerBrothersEntitites.SaveChanges();

                jsonResult.Data = new { message = "PMSchedule Created for Customer: " + NewPMScheduleData.AccountNumber, success=true };
            }

            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
    }
}