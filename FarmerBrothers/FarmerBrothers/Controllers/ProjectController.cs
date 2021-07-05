using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using FarmerBrothers.Data;

using FarmerBrothers.Models;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Reflection;

using System.ServiceModel.Channels;
using System.ServiceModel;
//using FarmerBrothers.FeastCatalogValidation;
//using FarmerBrothers.FeastERFUpdateService;
//using FarmerBrothers.FeastLocationService;
using Syncfusion.JavaScript.Models;
using Syncfusion.EJ.Export;
using Syncfusion.XlsIO;
using LumenWorks.Framework.IO.Csv;

namespace FarmerBrothers.Controllers
{
    public class ProjectController : BaseController
    {
       /* MarsViews objMarsView = new MarsViews();
        // GET: Project
        public ActionResult Project()
        {
            ViewBag.datasource = TempData["datasource"];
            return View("Project", new ProjectNumberModel());
        }

        public PartialViewResult EditProject(int id)
        {
            try
            {
                var projectNumber = FarmerBrothersEntitites.ProjectNumbers.First(x => x.ProjectID == id);
                ProjectNumberModel workOderModel = new ProjectNumberModel() { ProjectID = projectNumber.ProjectID.ToString(), DeadLine = projectNumber.DeadLine, Notes = projectNumber.Notes };
                return PartialView("UpdateProject", workOderModel);
            }
            catch (Exception)
            {
                return PartialView("UpdateProject", new ProjectNumberModel());
            }
        }

        [HttpPost]
        public int UpdateProject(int id, string testUpdateDate, ProjectNumberModel pm)
        {
            try
            {
                var project = FarmerBrothersEntitites.ProjectNumbers.First(x => x.ProjectID == id);
                project.Notes = pm.Notes;
                if (pm.DeadLine != null)
                    project.DeadLine = pm.DeadLine;
                else if (!string.IsNullOrEmpty(testUpdateDate))
                {
                    DateTime date = DateTime.ParseExact(testUpdateDate, "M/d/yyyy",
                                             new CultureInfo("en-US"),
                                             DateTimeStyles.None);
                    project.DeadLine = date;
                }
                FarmerBrothersEntitites.SaveChanges();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        [ChildActionOnly]
        public ActionResult CreateProject()
        {
            return PartialView("CreateProject");
        }
        [HttpPost]
        public int CreateProject(string testDate, ProjectNumberModel pm)
        {
            try
            {
                ProjectNumber project = new ProjectNumber();
                project.Notes = pm.Notes;
                if (pm.DeadLine != null)
                    project.DeadLine = pm.DeadLine;
                else if (!string.IsNullOrEmpty(testDate))
                {
                    DateTime date = DateTime.ParseExact(testDate, "M/d/yyyy",
                                             new CultureInfo("en-US"),
                                             DateTimeStyles.None);
                    project.DeadLine = date;
                }
                FarmerBrothersEntitites.ProjectNumbers.Add(project);
                FarmerBrothersEntitites.SaveChanges();
                return project.ProjectID;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public string CheckColumnNameExists(DataTable dtCheck, string columnNames, int ISERF)
        {
            string strMessage = "";
            string[] strcols = columnNames.Split(',');
            if (ISERF == 0)
            {
                foreach (DataColumn col in dtCheck.Columns)
                {
                    col.ColumnName = col.ColumnName.Replace(" ", "").Trim();
                }
            }
            else
            {
                foreach (DataColumn col in dtCheck.Columns)
                {
                    col.ColumnName = col.ColumnName.Trim();
                }

            }
            DataColumnCollection columns = dtCheck.Columns;
            foreach (string colName in strcols)
            {
                if (!columns.Contains(colName))
                {
                    strMessage = "Upload Failed: '" + colName + "' column not found in uploaded file. Please check the template.";
                    break;
                }
            }
            return strMessage;
        }
        public ActionResult SaveDefault(HttpPostedFileBase UploadDefault)
        {
            DataTable dt = new DataTable();
            DataTable Uploaddata = new DataTable();
            HttpPostedFileBase WorkOrderFile = UploadDefault;
            string fileExtension = Path.GetExtension(WorkOrderFile.FileName);
            if (fileExtension.ToLower() != ".csv")
            {
                TempData["notice"] = "Upload Failed: Uploaded file is not a .CSV file.";
                return Content("");
            }
            if (WorkOrderFile != null && WorkOrderFile.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileName(WorkOrderFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/App_Data/"), fileName.Replace('-', '_'));
                    if (fileName.ToLower().StartsWith("Bulk".ToLower()))
                    {
                        WorkOrderFile.SaveAs(path);
                        dt = ProcessCSV(path);
                        string colNames = "CustomerNumber,TechID,CustomerPO,Notes,ProjectNumber,AppointmentDate,EventContact,ContactName,ContactPhone,CallTypeID,Status,EquipmentType,SerialNo,Symptom,Location,BusinessEmail,HoursofOperation,CallerName,Brands,Priority";
                        string str = CheckColumnNameExists(dt, colNames, 0);
                        if (str != "")
                        {
                            TempData["notice"] = str;
                            return Content("");
                        }

                    }
                    else if (fileName.ToLower().StartsWith("WorkOrder".ToLower()))
                    {
                        WorkOrderFile.SaveAs(path);
                        dt = ProcessCSV(path);
                        string colNames = "CustomerNumber,ServiceProviderJDE,CustomerPO,Notes,ProjectNumber,AppointmentDate,ContactName,ContactPhone,CallTypeID,Status,CallerName,Brands,Priority";
                        string str = CheckColumnNameExists(dt, colNames, 0);
                        if (str != "")
                        {
                            TempData["notice"] = str;
                            return Content("");
                        }
                    }
                    else if (fileName.ToLower().StartsWith("Closure".ToLower()))
                    {
                        WorkOrderFile.SaveAs(path);
                        dt = ProcessCSVClosure(path);
                        string colNames = "WorkorderID,InvoiceNo,M1CallType,M1Symptom,M1System,M1Solution,M1MachineType,M1Model,M1Serial,M1Asset,Temparature1,Ratio1,CustSignature,StartDateTime,ArrivalDateTime,CompletionDateTime,TechID,MileagetoCustomer,Location";
                        string str = CheckColumnNameExists(dt, colNames, 0);
                        if (str != "")
                        {
                            TempData["notice"] = str;
                            return Content("");
                        }
                    }
                    else
                    {
                        TempData["notice"] = "Upload Failed: Upload FileName starts with Bulk / Workorder / Closure for this type of upload.";
                        return Content("");
                    }
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        if (fileName.ToLower().StartsWith("Bulk".ToLower()))
                        {
                            var dtFinal = BulkWorkOrderDataTable(dt);
                            Uploaddata = fnBulkWorkOrderInsert(dtFinal, dt);

                        }
                        else if (fileName.ToLower().StartsWith("WorkOrder".ToLower()))
                        {
                            var dtFinal = MultipleWorkOrderDataTable(dt);
                            Uploaddata = fnMultipleWorkOrderInsert(dtFinal);
                        }
                        else if (fileName.ToLower().StartsWith("Closure".ToLower()))
                        {
                            var dtFinal = WorkOrderClosureDataTable(dt);
                            if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["WorkorderID"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[0]["WorkorderID"]), @"^\d+$"))
                            {
                                TempData["notice"] = "Upload Failed: WorkorderID should be numeric.";
                                return Content("");
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["CompletionDateTime"])) && !IsDate(dt.Rows[0]["CompletionDateTime"].ToString()))
                            {
                                TempData["notice"] = "Upload Failed: Invalid CompletionDateTime.";
                                return Content("");
                            }
                            bool closeValidation = true;
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1Symptom"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["M1Symptom"]), @"^\d+$"))
                                {
                                    TempData["notice"] = "Upload Failed: M1Symptom should be numeric.";
                                    closeValidation = false;
                                    break;
                                }
                                if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1CallType"])) || (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1CallType"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["M1CallType"]), @"^\d+$")))
                                {
                                    TempData["notice"] = "Upload Failed: M1CallType should be numeric.";
                                    closeValidation = false;
                                    break;
                                }
                                if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1Solution"])) || (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1Solution"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["M1Solution"]), @"^\d+$")))
                                {
                                    TempData["notice"] = "Upload Failed: M1Solution should be numeric/not empty.";
                                    closeValidation = false;
                                    break;
                                }
                                if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1Symptom"])) || (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1Symptom"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["M1Symptom"]), @"^\d+$")))
                                {
                                    TempData["notice"] = "Upload Failed: M1Symptom should be numeric/not empty.";
                                    closeValidation = false;
                                    break;
                                }
                                if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1System"])) || (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1System"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["M1System"]), @"^\d+$")))
                                {
                                    TempData["notice"] = "Upload Failed: M1System should be numeric/not empty.";
                                    closeValidation = false;
                                    break;
                                }
                                if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["MileagetoCustomer"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["MileagetoCustomer"]), @"^\d*\.?\d*"))
                                {
                                    TempData["notice"] = "Upload Failed: MileagetoCustomer should be numeric.";
                                    closeValidation = false;
                                    break;
                                }
                                if (!IsDate(dt.Rows[0]["CompletionDateTime"].ToString()))
                                {
                                    TempData["notice"] = "Upload Failed: Invalid CompletionDateTime.";
                                    closeValidation = false;
                                    return Content("");
                                }
                                if (!IsDate(dt.Rows[0]["StartDateTime"].ToString()))
                                {
                                    TempData["notice"] = "Upload Failed: Invalid StartDateTime.";
                                    closeValidation = false;
                                    return Content("");
                                }
                                if (!IsDate(dt.Rows[0]["ArrivalDateTime"].ToString()))
                                {
                                    TempData["notice"] = "Upload Failed: Invalid ArrivalDateTime.";
                                    closeValidation = false;
                                    return Content("");
                                }
                            }
                            if (closeValidation == false)
                            {
                                return Content("");
                            }
                            int workorderid = Convert.ToInt32(dtFinal.Rows[0]["WorkorderID"]);
                            var result = FarmerBrothersEntitites.WorkOrders.SingleOrDefault(b => b.WorkorderID == workorderid);
                            if (result == null)
                            {
                                TempData["notice"] = "Upload Failed: WorkorderId doesn't exists.";
                                return Content("");
                            }

                            fnWorkOrderClosureInsert(dtFinal);
                        }
                        else
                        {
                            TempData["notice"] = "Uploaded files are not in correct format";
                        }

                        dt.Dispose();
                        TempData["notice"] = "Successfully Uploaded.";
                    }
                    else
                    {
                        TempData["notice"] = "Upload Failed";
                    }
                }
                catch (Exception ex)
                {
                    dt.Dispose();
                    TempData["notice"] = ex.Message;
                }
            }
            // return View();
            TempData["datasource"] = Uploaddata.Rows.Count > 0 ? Uploaddata : null;
            TempData["ExportToExcel"] = Uploaddata.Rows.Count > 0 ? Uploaddata : null;
            return Content("");
            // return RedirectToAction("Project");

        }
        [HttpPost]
        public ActionResult UploadWorkOrder(HttpPostedFileBase FileUpload, HttpPostedFileBase FileUploadERF, string Command)
        {
            DataTable dt = new DataTable();
            DataTable Uploaddata = new DataTable();
            HttpPostedFileBase WorkOrderFile = FileUploadERF;
            if (Command == "UploadWorkOrder")
            {
                WorkOrderFile = FileUpload;
            }

            if (WorkOrderFile != null && WorkOrderFile.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileName(WorkOrderFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
                    WorkOrderFile.SaveAs(path);
                    dt = ProcessCSV(path);
                    if (dt.Rows.Count > 0)
                    {
                        if (fileName.StartsWith("Bulk"))
                        {
                            var dtFinal = BulkWorkOrderDataTable(dt);
                            Uploaddata = fnBulkWorkOrderInsert(dtFinal, dt);

                        }
                        else
                        {
                            var dtFinal = MultipleWorkOrderDataTable(dt);
                            Uploaddata = fnMultipleWorkOrderInsert(dtFinal);

                        }
                        dt.Dispose();
                        TempData["notice"] = "Successfully uploaded.";
                    }
                    else
                    {
                        TempData["notice"] = path;
                    }
                }
                catch (Exception ex)
                {
                    dt.Dispose();
                    TempData["notice"] = "Upload Failed";
                }
            }
            TempData["datasource"] = Uploaddata;
            return RedirectToAction("Project");
        }
        public string ReplaceWordChars(string text)
        {
            var s = text;
            // smart single quotes and apostrophe
            s = Regex.Replace(s, "[\u2018\u2019\u201A]", "'");
            // smart double quotes
            s = Regex.Replace(s, "[\u201C\u201D\u201E]", "\"");
            // ellipsis
            s = Regex.Replace(s, "\u2026", "...");
            // dashes
            s = Regex.Replace(s, "[\u2013\u2014]", "-");
            // circumflex
            s = Regex.Replace(s, "\u02C6", "^");
            // open angle bracket
            s = Regex.Replace(s, "\u2039", "<");
            // close angle bracket
            s = Regex.Replace(s, "\u203A", ">");
            // spaces
            s = Regex.Replace(s, "[\u02DC\u00A0]", " ");

            s = Regex.Replace(s, "\u0096", "–");

            return s;
        }
        private DataTable fnBulkWorkOrderInsert(DataTable dtUpload, DataTable dtOriginal)
        {
            DataView view = new DataView(dtUpload);
            DataTable distinctValues = view.ToTable(true, "CustomerNumber");
            distinctValues.Columns.Add("Count");
            DataTable dt = dtUpload.Clone();
            for (int i = 0; i < distinctValues.Rows.Count; i++)
            {
                if (dtUpload.Select("CustomerNumber=" + distinctValues.Rows[i][0].ToString()).Length > 0)
                {
                    distinctValues.Rows[i]["Count"] = dtUpload.Select("CustomerNumber=" + distinctValues.Rows[i][0].ToString()).Length;
                    DataRow[] result = dtUpload.Select("CustomerNumber=" + distinctValues.Rows[i][0].ToString());
                    dt.Rows.Add(result[0].ItemArray);
                    int iCustCount = 0;
                    foreach (DataRow drCustomerCount in result)
                    {
                        iCustCount = iCustCount + Convert.ToInt32(drCustomerCount["CustomerCount"].ToString());
                    }
                    if (iCustCount > Convert.ToInt16(dt.Rows[i]["CustomerCount"].ToString()))
                    {
                        dt.Rows[i]["CustomerCount"] = result.Length;
                    }
                }
                else
                {
                    distinctValues.Rows[i]["Count"] = 0;
                }
            }
            DataTable Uploadetable = dt.Copy();
            Uploadetable.Columns.Add("UploadStatus", typeof(string)).SetOrdinal(0);
            for (int i = 0; i < Uploadetable.Columns.Count; i++)
            {
                Uploadetable.Columns[i].ReadOnly = false;
            }


            string StrSql = string.Empty;
            string StrPrioirity = string.Empty;
            string strCategory = string.Empty;
            int iPriorityCode;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["CustomerNumber"].ToString() == "")
                {
                    DataRow dr = Uploadetable.Rows[i];
                    dr.Delete();

                }
                else
                {
                    var customer = customerDetails(dt.Rows[i]["CustomerNumber"].ToString());
                    Uploadetable.Rows[i]["UploadStatus"] = Convert.ToInt32(customer.CustomerId) != 0 ? "Uploaded Sucessfully" : "Upload failed: Invalid Customer Number";
                    strCategory = dt.Rows[i]["EquipmentType"].ToString();

                    if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Priority"])) || (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Priority"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["Priority"]), @"^\d+$")))
                    {
                        Uploadetable.Rows[i]["UploadStatus"] = "Upload Failed: Priority should be numeric/not empty.";
                        continue;
                    }
                    if (dt.Rows[i]["Status"].ToString().ToLower() == "accepted")
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["TechID"])) || (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["TechID"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["TechID"]), @"^\d+$")))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload Failed: TechID should be numeric/not empty for Accepted Status.";
                            continue;
                        }
                    }
                    //strPriority = ReplaceWordChars(dt.Rows[i]["Priority"].ToString());
                    iPriorityCode = Convert.ToInt32(dt.Rows[i]["Priority"].ToString());
                    //var PriorityCode = (from p in FormalBrothersEntitites.AllFormalBrothersStatus where p.FormalBrothersStatusID == iPriorityCode select p.FormalBrothersStatusID).FirstOrDefault();
                    int PriorityCode = 0;
                    var BrandStatus = true;
                    var IsCategoryValid = true;
                    string BrandMsg = string.Empty;

                    int iBrandID = 0;
                    foreach (string str in dt.Rows[i]["Brands"].ToString().Split(',').ToArray())
                    {
                        iBrandID = 0;
                        if (Regex.IsMatch(str, @"^\d+$"))
                        {
                            iBrandID = Convert.ToInt32(str);
                        }
                        var BrandID = (from p in FarmerBrothersEntitites.BrandNames where p.BrandID == iBrandID && p.Active == 1 select p.BrandID).FirstOrDefault();
                        if (BrandID == 0)
                        {
                            BrandStatus = false;
                            BrandMsg = str;
                            break;
                        }

                    }

                    DataTable dtWrkEquip = dtUpload.Select("CustomerNumber=" + dt.Rows[i]["CustomerNumber"]).CopyToDataTable();
                    foreach (DataRow dr in dtWrkEquip.Rows)
                    {
                        var checkCategory = (from p in FarmerBrothersEntitites.Skus where p.EQUIPMENT_TAG == "TAGGED" && p.Category == strCategory select p.Category).FirstOrDefault();
                        if (checkCategory == null)
                        {
                            IsCategoryValid = false;
                            break;
                        }
                    }
                    if (PriorityCode == 0)
                    {
                        Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid Priority Code";
                        continue;
                    }
                    else if (BrandStatus == false)
                    {
                        Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid BrandID - " + BrandMsg + "";
                        continue;
                    }
                    else if (IsCategoryValid == false)
                    {
                        Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid Equipment Type";
                        continue;
                    }
                    //else if (dt.Rows[i]["Status"].ToString().ToLower() == "accepted" && dt.Rows[i]["TechID"].ToString().Trim() == "")
                    //{
                    //    Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: TechID not available for Accepted Status";
                    //    continue;
                    //}

                    if (Convert.ToInt32(customer.CustomerId) != 0)
                    {
                        FarmerBrothers.Data.WorkOrder wr = new FarmerBrothers.Data.WorkOrder();
                        wr.CustomerID = Convert.ToInt32(customer.CustomerId);
                        wr.CustomerName = customer.CustomerName;
                        wr.CustomerAddress = customer.Address;
                        wr.CustomerCity = customer.City;
                        wr.CustomerState = customer.State;
                        wr.CustomerZipCode = customer.ZipCode;
                        wr.CustomerMainContactName = customer.MainContactName;
                        wr.CustomerPhone = customer.PhoneNumber;
                        wr.CustomerPhoneExtn = customer.PhoneExtn;
                        wr.CustomerCustomerPreferences = customer.CustomerPreference;
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["AppointmentDate"])) && IsDate(dt.Rows[i]["AppointmentDate"].ToString()))
                            wr.AppointmentDate = Convert.ToDateTime(dt.Rows[i]["AppointmentDate"].ToString());
                        wr.WorkorderContactName = dt.Rows[i]["ContactName"].ToString();
                        wr.WorkorderContactPhone = dt.Rows[i]["ContactPhone"].ToString();
                        wr.CallerName = dt.Rows[i]["CallerName"].ToString();
                        // wr.WorkorderCalltypeid = Convert.ToInt32(dt.Rows[i]["CallTypeID"]);
                        wr.WorkorderCallstatus = dt.Rows[i]["Status"].ToString();
                        wr.CustomerMainEmail = dt.Rows[i]["BusinessEmail"].ToString();

                        DataTable dtCustomerDatetime = objMarsView.fn_FSM_View("Select dbo.getCustDateTime('" + customer.ZipCode + "')");
                        if (dtCustomerDatetime.Rows.Count > 0)
                        {
                            wr.WorkorderEntryDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                            wr.WorkorderModifiedDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                        }


                        wr.EntryUserName = "Mass Upload";
                        // int calltypeid = Convert.ToInt32(dt.Rows[i]["CallTypeID"]);
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["ProjectNumber"])) && Regex.IsMatch(Convert.ToString(dt.Rows[i]["ProjectNumber"]), @"^\d+$"))
                        {
                            wr.ProjectID = Convert.ToInt32(dt.Rows[i]["ProjectNumber"].ToString());
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["CallTypeID"])) && Regex.IsMatch(Convert.ToString(dt.Rows[i]["CallTypeID"]), @"^\d+$"))
                        {
                            int calltypeid = Convert.ToInt32(dt.Rows[i]["CallTypeID"]);
                            wr.WorkorderCalltypeid = calltypeid;
                            wr.WorkorderCalltypeDesc = (from p in FarmerBrothersEntitites.WorkorderTypes where p.CallTypeID == calltypeid select p.Description).FirstOrDefault();
                        }
                        //    wr.WorkorderCalltypeDesc = (from p in FormalBrothersEntitites.WorkorderTypes where p.CallTypeID == calltypeid select p.Description).FirstOrDefault();
                        wr.WorkorderEquipCount = Convert.ToInt16(dt.Rows[i]["CustomerCount"]);
                        wr.CoverageZone = (from p in FarmerBrothersEntitites.ZoneZips where p.ZipCode == customer.ZipCode.Substring(0, 5) select p.ZoneIndex).FirstOrDefault();
                        wr.WorkorderTimeZone = (from p in FarmerBrothersEntitites.Zips where p.ZIP1 == customer.ZipCode select p.TimeZone).FirstOrDefault();
                        var techids = (from p in FarmerBrothersEntitites.ZonePriorities
                                       where
                                     ((from q in FarmerBrothersEntitites.ZoneZips where q.ZipCode == customer.ZipCode.Substring(0, 5) select q.ZoneIndex).ToList().Contains(p.ZoneIndex))
                                       select new { p.ResponsibletechID, p.SecondaryTechID }).FirstOrDefault();
                        wr = fnInsertWorkOrderTech(wr, techids.ResponsibletechID, techids.SecondaryTechID);
                        //StrPrioirity = ReplaceWordChars(dt.Rows[i]["Priority"].ToString());
                        //wr.PriorityCode = (from p in FormalBrothersEntitites.AllFormalBrothersStatus where p.FormalBrothersStatusID == iPriorityCode select p.FormalBrothersStatusID).FirstOrDefault();
                        wr.FollowupCallID = 603;
                        IndexCounter counter = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "WorkorderID");
                        counter.IndexValue++;
                        wr.WorkorderID = counter.IndexValue.Value;   //wr.Notes = dt.Rows[i]["Notes"].ToString();
                        Uploadetable.Rows[i]["WorkorderID"] = counter.IndexValue.Value;
                        wr.CallerName = dt.Rows[i]["CallerName"].ToString();
                        FarmerBrothersEntitites.WorkOrders.Add(wr);
                        //FormalBrothersEntitites.SaveChanges();

                        foreach (string str in dt.Rows[i]["Brands"].ToString().Split(',').ToArray())
                        {
                            WorkOrderBrand wrkOrderBrands = new WorkOrderBrand();
                            wrkOrderBrands.WorkorderID = wr.WorkorderID;
                            // wrkOrderBrands.BrandID = (from p in FormalBrothersEntitites.BrandNames where p.BrandName1 == str select p.BrandID).FirstOrDefault();
                            wrkOrderBrands.BrandID = Convert.ToInt32(str);
                            FarmerBrothersEntitites.WorkOrderBrands.Add(wrkOrderBrands);
                        }
                        // FormalBrothersEntitites.SaveChanges();
                        NotesHistory notesHistory = new NotesHistory();
                        notesHistory.AutomaticNotes = 1;
                        notesHistory.WorkorderID = wr.WorkorderID;
                        if (dtCustomerDatetime.Rows.Count > 0)
                        {
                            notesHistory.EntryDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                            notesHistory.Notes = dt.Rows[i]["Notes"].ToString();
                        }

                        FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                        FarmerBrothersEntitites.SaveChanges();
                        notesHistory.Notes = "Work Order Created By Upload";
                        FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                        int workOrderCount = Convert.ToInt32(dt.Rows[i]["CustomerCount"]);
                        dtWrkEquip = dtOriginal.Select("CustomerNumber=" + dt.Rows[i]["CustomerNumber"]).CopyToDataTable();
                        if (dt.Rows[i]["Status"].ToString().ToLower() == "accepted")
                        {
                            for (int j = 0; j < dtWrkEquip.Rows.Count; j++)
                            {
                                WorkorderEquipmentRequested wrEquipmentReq = new WorkorderEquipmentRequested();
                                wrEquipmentReq.WorkorderID = wr.WorkorderID;
                                IndexCounter Assetcounter = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "AssetID");
                                Assetcounter.IndexValue++;
                                wrEquipmentReq.Assetid = Assetcounter.IndexValue.Value;
                                Regex.IsMatch(Convert.ToString(dtWrkEquip.Rows[j]["Symptom"]), @"^\d+$");
                                //   wrEquipment.CallTypeid = dt.Rows[i]["SerialNo"].ToString();
                                if (!string.IsNullOrEmpty(Convert.ToString(dtWrkEquip.Rows[j]["Symptom"])))
                                    wrEquipmentReq.Symptomid = Regex.IsMatch(Convert.ToString(dtWrkEquip.Rows[j]["Symptom"]), @"^\d+$") ? Convert.ToInt32(dtWrkEquip.Rows[j]["Symptom"]) : default(int);
                                wrEquipmentReq.Location = dtWrkEquip.Rows[j]["Location"].ToString();
                                wrEquipmentReq.Category = dtWrkEquip.Rows[j]["EquipmentType"].ToString();
                                if (!string.IsNullOrEmpty(Convert.ToString(dtWrkEquip.Rows[j]["CallTypeID"])) && Regex.IsMatch(Convert.ToString(dtWrkEquip.Rows[j]["CallTypeID"]), @"^\d+$"))
                                    wrEquipmentReq.CallTypeid = Convert.ToInt32(dtWrkEquip.Rows[j]["CallTypeID"]);
                                wrEquipmentReq.SerialNumber = dtWrkEquip.Rows[j]["SerialNo"].ToString();
                                FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Add(wrEquipmentReq);
                                FarmerBrothersEntitites.SaveChanges();

                                WorkorderEquipment wrEquipment = new WorkorderEquipment();
                                wrEquipment.WorkorderID = wr.WorkorderID;
                                IndexCounter Assetcounter1 = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "AssetID");
                                Assetcounter1.IndexValue++;
                                wrEquipment.Assetid = Assetcounter1.IndexValue.Value;
                                Regex.IsMatch(Convert.ToString(dtWrkEquip.Rows[j]["Symptom"]), @"^\d+$");
                                //   wrEquipment.CallTypeid = dt.Rows[i]["SerialNo"].ToString();
                                if (!string.IsNullOrEmpty(Convert.ToString(dtWrkEquip.Rows[j]["Symptom"])))
                                    wrEquipment.Symptomid = Regex.IsMatch(Convert.ToString(dtWrkEquip.Rows[j]["Symptom"]), @"^\d+$") ? Convert.ToInt32(dtWrkEquip.Rows[j]["Symptom"]) : default(int);
                                wrEquipment.Location = dtWrkEquip.Rows[j]["Location"].ToString();
                                wrEquipment.Category = dtWrkEquip.Rows[j]["EquipmentType"].ToString();
                                if (!string.IsNullOrEmpty(Convert.ToString(dtWrkEquip.Rows[j]["CallTypeID"])) && Regex.IsMatch(Convert.ToString(dtWrkEquip.Rows[j]["CallTypeID"]), @"^\d+$"))
                                    wrEquipment.CallTypeid = Convert.ToInt32(dtWrkEquip.Rows[j]["CallTypeID"]);
                                wrEquipment.SerialNumber = dtWrkEquip.Rows[j]["SerialNo"].ToString();
                                FarmerBrothersEntitites.WorkorderEquipments.Add(wrEquipment);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < workOrderCount; j++)
                            {
                                WorkorderEquipmentRequested wrEquipmentReq = new WorkorderEquipmentRequested();
                                wrEquipmentReq.WorkorderID = wr.WorkorderID;
                                IndexCounter Assetcounter = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "AssetID");
                                Assetcounter.IndexValue++;
                                wrEquipmentReq.Assetid = Assetcounter.IndexValue.Value;
                                Regex.IsMatch(Convert.ToString(dtWrkEquip.Rows[j]["Symptom"]), @"^\d+$");
                                //   wrEquipment.CallTypeid = dt.Rows[i]["SerialNo"].ToString();
                                if (!string.IsNullOrEmpty(Convert.ToString(dtWrkEquip.Rows[j]["Symptom"])))
                                    wrEquipmentReq.Symptomid = Regex.IsMatch(Convert.ToString(dtWrkEquip.Rows[j]["Symptom"]), @"^\d+$") ? Convert.ToInt32(dtWrkEquip.Rows[j]["Symptom"]) : default(int);
                                wrEquipmentReq.Location = dtWrkEquip.Rows[j]["Location"].ToString();
                                wrEquipmentReq.Category = dtWrkEquip.Rows[j]["EquipmentType"].ToString();
                                if (!string.IsNullOrEmpty(Convert.ToString(dtWrkEquip.Rows[j]["CallTypeID"])) && Regex.IsMatch(Convert.ToString(dtWrkEquip.Rows[j]["CallTypeID"]), @"^\d+$"))
                                    wrEquipmentReq.CallTypeid = Convert.ToInt32(dtWrkEquip.Rows[j]["CallTypeID"]);
                                wrEquipmentReq.SerialNumber = dtWrkEquip.Rows[j]["SerialNo"].ToString();
                                FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Add(wrEquipmentReq);
                                //FormalBrothersEntitites.SaveChanges();
                            }
                        }
                        if (dt.Rows[i]["Status"].ToString().ToLower() == "accepted" && dt.Rows[i]["TechID"] != DBNull.Value)
                        {
                            WorkorderSchedule wrkSchedule = new WorkorderSchedule();
                            IndexCounter counterSchedule = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "WorkorderID");
                            counterSchedule.IndexValue++;
                            wrkSchedule.WorkorderID = wr.WorkorderID;
                            wrkSchedule.Scheduleid = counterSchedule.IndexValue.Value;

                            if (dtCustomerDatetime.Rows.Count > 0)
                            {
                                wrkSchedule.EntryDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                                wrkSchedule.ScheduleDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                            }
                            wrkSchedule.Techid = Convert.ToInt32(dt.Rows[i]["TechID"]);
                            wrkSchedule.AssignedStatus = "Accepted";
                            wrkSchedule.AssistTech = -1;
                            wrkSchedule.PrimaryTech = 1;
                            DataTable dtTechData = objMarsView.fn_FSM_View("Select Tech_Name, ServiceCenter_Name, FSM_ID, FSM_Name from feast_tech_hierarchy where tech_id = " + dt.Rows[i]["TechID"]);
                            if (dtTechData.Rows.Count > 0)
                            {
                                wrkSchedule.TechName = dtTechData.Rows[0]["Tech_Name"].ToString();
                                wrkSchedule.ServiceCenterName = dtTechData.Rows[0]["ServiceCenter_Name"].ToString();
                                if (dtTechData.Rows[0]["FSM_ID"].ToString() != "")
                                    wrkSchedule.FSMID = Convert.ToInt32(dtTechData.Rows[0]["FSM_ID"].ToString());
                                wrkSchedule.FSMName = dtTechData.Rows[0]["FSM_Name"].ToString();
                            }

                            var ServiceCenterID = (from p in FarmerBrothersEntitites.Database.SqlQuery<double>(@"Select  Case left(Tech_Desc,4) 	 WHEN  'TPSP' THEN  ServiceCenter_Id    ELSE Default_service_center END  as ServiceCenter  from feast_tech_hierarchy  where Tech_Id=" + Convert.ToInt32(dt.Rows[i]["TechID"])) select p).FirstOrDefault();
                            if (ServiceCenterID != 0)
                            {
                                wrkSchedule.ServiceCenterID = Convert.ToInt32(ServiceCenterID);
                            }
                            //  wrkSchedule.ServiceCenterID = Convert.ToInt32(objMarsView.fn_FSM_View(Sqlquery).Rows[0]["ServiceCenter"]);
                            FarmerBrothersEntitites.WorkorderSchedules.Add(wrkSchedule);
                            //  FormalBrothersEntitites.SaveChanges();

                        }
                        FarmerBrothersEntitites.SaveChanges();
                    }
                }
            }
            return Uploadetable;
        }


        private static DataTable ProcessCSV(string fileName)
        {
            DataTable csvTable = new DataTable();
            using (CsvReader csvReader =
                new CsvReader(new StreamReader(fileName, System.Text.Encoding.GetEncoding("iso-8859-1")), true))
            {
                csvTable.Load(csvReader);
            }
            return csvTable;
            //     DataTable dt = new DataTable();
            //     var connString = string.Format(
            //    @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;IMEX=1""",
            //    Path.GetDirectoryName(fileName)
            //);

            //     using (var conn = new OleDbConnection(connString))
            //     {
            //         if (conn.State.ToString() == "Open")
            //         { conn.Close(); }
            //         conn.Open();
            //         var StrSql = "SELECT * FROM [" + Path.GetFileName(fileName) + "]";
            //         using (var adapter = new OleDbDataAdapter(StrSql, conn))
            //         {

            //             adapter.Fill(dt);
            //         }
            //     }

            //     return dt;
        }

        public static DataTable ProcessCSVClosure(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }

            }


            return dt;
        }

        
        public FeastLocationService.Customer customerDetails(string customerID)
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
                        customerModel = new CustomerModel(response.Customer[0], FormalBrothersEntitites);
                        custDetails.CustomerId = customerModel.CustomerId;
                        custDetails.CustomerName = customerModel.CustomerName;
                        custDetails.Address = customerModel.Address;
                        custDetails.City = customerModel.City;
                        custDetails.State = customerModel.State;
                        custDetails.ZipCode = customerModel.ZipCode;
                        custDetails.PhoneNumber = customerModel.PhoneNumber;
                        custDetails.PhoneExtn = customerModel.PhoneExtn;
                        custDetails.MainContactName = customerModel.MainContactName;
                        custDetails.CustomerPreference = customerModel.CustomerPreference;
                    }
                }
            }
            return custDetails;
        }
        
        private DataTable BulkWorkOrderDataTable(DataTable sourceDt)
        {
            foreach (DataColumn col in sourceDt.Columns)
            {
                col.ColumnName = col.ColumnName.Replace(" ", "").Trim();
            }
            var filterGroup = from d in sourceDt.AsEnumerable()
                              group d by new
                              {
                                  //EventID = d["EventID"],
                                  CustomerNumber = d["CustomerNumber"],
                                  TechID = d["TechID"],
                                  CustomerPO = d["CustomerPO"],
                                  Notes = d["Notes"],
                                  ProjectNumber = d["ProjectNumber"],
                                  AppointmentDate = d["AppointmentDate"],
                                  EventContact = d["EventContact"],
                                  ContactName = d["ContactName"],
                                  ContactPhone = d["ContactPhone"],
                                  CallTypeID = d["CallTypeID"],
                                  Status = d["Status"],
                                  EquipmentType = d["EquipmentType"],
                                  SerialNo = d["SerialNo"],
                                  Symptom = d["Symptom"],
                                  Location = d["Location"],
                                  BusinessEmail = d["BusinessEmail"],
                                  HoursofOperation = d["HoursofOperation"],
                                  CallerName = d["CallerName"],
                                  Brands = d["Brands"],
                                  Priority = d["Priority"]
                              } into tempGrp
                              select new
                              {
                                  CustomerNumber = tempGrp.Key.CustomerNumber,
                                  TechID = tempGrp.Key.TechID,
                                  AppointmentDate = tempGrp.Key.AppointmentDate,
                                  EventContact = tempGrp.Key.EventContact,
                                  Notes = tempGrp.Key.Notes,
                                  ProjectNumber = tempGrp.Key.ProjectNumber,
                                  ContactName = tempGrp.Key.ContactName,
                                  ContactPhone = tempGrp.Key.ContactPhone,
                                  CallTypeID = tempGrp.Key.CallTypeID,
                                  Status = tempGrp.Key.Status,
                                  BusinessEmail = tempGrp.Key.BusinessEmail,
                                  CallerName = tempGrp.Key.CallerName,
                                  EquipmentType = tempGrp.Key.EquipmentType,
                                  SerialNo = tempGrp.Key.SerialNo,
                                  Symptom = tempGrp.Key.Symptom,
                                  Location = tempGrp.Key.Location,
                                  Brands = tempGrp.Key.Brands,
                                  Priority = tempGrp.Key.Priority,
                                  CustomerCount = tempGrp.Count(),
                              };
            var destDt = sourceDt.Clone();

            //destDt.Columns["CustomerJDE"].ColumnName = "CustomerNumber";
            destDt.Columns.Add("CustomerCount");
            foreach (var item in filterGroup)
            {
                DataRow dr = destDt.NewRow();

                if (item.CustomerNumber == DBNull.Value) { }
                else
                {
                    dr["CustomerNumber"] = item.CustomerNumber;
                    dr["TechID"] = item.TechID;
                    dr["AppointmentDate"] = item.AppointmentDate;
                    dr["EventContact"] = item.EventContact;
                    dr["Notes"] = item.Notes;
                    dr["ProjectNumber"] = item.ProjectNumber;
                    dr["ContactName"] = item.ContactName;
                    dr["ContactPhone"] = item.ContactPhone;
                    dr["CallTypeID"] = item.CallTypeID;
                    dr["Status"] = item.Status;
                    dr["BusinessEmail"] = item.BusinessEmail;
                    dr["CallerName"] = item.CallerName;
                    dr["SerialNo"] = item.SerialNo;
                    dr["Symptom"] = item.Symptom;
                    dr["EquipmentType"] = item.EquipmentType;
                    dr["Location"] = item.Location;
                    dr["CallTypeID"] = item.CallTypeID;
                    dr["CallerName"] = item.CallerName;
                    dr["Brands"] = item.Brands;
                    dr["Priority"] = item.Priority;
                    dr["CustomerCount"] = item.CustomerCount;
                    destDt.Rows.Add(dr);
                }
            }
            return destDt;
        }

        private DataTable fnMultipleWorkOrderInsert(DataTable dt)
        {


            DataTable Uploadetable = dt.Copy();
            for (int i = 0; i < Uploadetable.Columns.Count; i++)
            {
                Uploadetable.Columns[i].ReadOnly = false;
            }
            try
            {
                Uploadetable.Columns.Add("UploadStatus", typeof(string)).SetOrdinal(0);
                string StrSql = string.Empty;
                string strPriority = string.Empty;
                int iPriorityCode;

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    if (dt.Rows[i]["CustomerNumber"].ToString() == "")
                    {
                        DataRow dr = Uploadetable.Rows[i];
                        dr.Delete();

                    }
                    else
                    {


                        var customer = customerDetails(dt.Rows[i]["CustomerNumber"].ToString());
                        Uploadetable.Rows[i]["UploadStatus"] = Convert.ToInt32(customer.CustomerId) != 0 ? "Uploaded Sucessfully" : "Upload failed: Invalid Customer Number";
                        if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Priority"])) || (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Priority"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["Priority"]), @"^\d+$")))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload Failed: Priority should be numeric/not empty.";
                            continue;
                        }
                        if (dt.Rows[i]["Status"].ToString().ToLower() == "accepted")
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["TechID"])) || (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["TechID"])) && !Regex.IsMatch(Convert.ToString(dt.Rows[i]["TechID"]), @"^\d+$")))
                            {
                                Uploadetable.Rows[i]["UploadStatus"] = "Upload Failed: TechID should be numeric/not empty for Accepted Status.";
                                continue;
                            }
                        }
                        //strPriority = ReplaceWordChars(dt.Rows[i]["Priority"].ToString());
                        iPriorityCode = Convert.ToInt32(dt.Rows[i]["Priority"].ToString());
                        //var PriorityCode = (from p in FormalBrothersEntitites.AllFormalBrothersStatus where p.FormalBrothersStatusID == iPriorityCode select p.FormalBrothersStatusID).FirstOrDefault();
                        int PriorityCode = 0;
                        var BrandStatus = true;
                        string BrandMsg = string.Empty;

                        int iBrandID = 0;
                        foreach (string str in dt.Rows[i]["Brands"].ToString().Split(',').ToArray())
                        {
                            iBrandID = 0;
                            if (Regex.IsMatch(str, @"^\d+$"))
                            {
                                iBrandID = Convert.ToInt32(str);
                            }
                            var BrandID = (from p in FarmerBrothersEntitites.BrandNames where p.BrandID == iBrandID && p.Active == 1 select p.BrandID).FirstOrDefault();
                            if (BrandID == 0)
                            {
                                BrandStatus = false;
                                BrandMsg = str;
                                break;
                            }

                        }

                        if (PriorityCode == 0)
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid Priority Code";
                            continue;
                        }
                        else if (BrandStatus == false)
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid BrandID - " + BrandMsg + "";
                            continue;
                        }

                        if (Convert.ToInt32(customer.CustomerId) != 0)
                        {
                            FarmerBrothers.Data.WorkOrder wr = new FarmerBrothers.Data.WorkOrder();
                            wr.CustomerID = Convert.ToInt32(customer.CustomerId);
                            wr.CustomerName = customer.CustomerName;
                            wr.CustomerAddress = customer.Address;
                            wr.CustomerCity = customer.City;
                            wr.CustomerState = customer.State;
                            wr.CustomerZipCode = customer.ZipCode;
                            wr.CustomerMainContactName = customer.MainContactName;
                            wr.CustomerPhone = customer.PhoneNumber;
                            wr.CustomerPhoneExtn = customer.PhoneExtn;
                            wr.CustomerCustomerPreferences = customer.CustomerPreference;
                            if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["AppointmentDate"])) && IsDate(dt.Rows[i]["AppointmentDate"].ToString()))
                                wr.AppointmentDate = Convert.ToDateTime(dt.Rows[i]["AppointmentDate"].ToString());
                            wr.WorkorderContactName = dt.Rows[i]["ContactName"].ToString();
                            wr.CallerName = dt.Rows[i]["CallerName"].ToString();
                            wr.WorkorderContactPhone = dt.Rows[i]["ContactPhone"].ToString();
                            wr.WorkorderCallstatus = dt.Rows[i]["Status"].ToString();
                            //strPriority = ReplaceWordChars(dt.Rows[i]["Priority"].ToString());
                            //wr.PriorityCode = (from p in FormalBrothersEntitites.AllFormalBrothersStatus where p.FormalBrothersStatus == strPriority select p.FormalBrothersStatusID).FirstOrDefault();
                            wr.PriorityCode = Convert.ToInt32(dt.Rows[i]["Priority"].ToString());
                            wr.FollowupCallID = 603;
                            if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["ProjectNumber"])) && Regex.IsMatch(Convert.ToString(dt.Rows[i]["ProjectNumber"]), @"^\d+$"))
                            {
                                wr.ProjectID = Convert.ToInt32(dt.Rows[i]["ProjectNumber"].ToString());
                            }
                            //wr.CustomerMainEmail = dt.Rows[i]["BusinessEmail"].ToString();
                            DataTable dtCustomerDatetime = objMarsView.fn_FSM_View("Select dbo.getCustDateTime('" + customer.ZipCode + "')");
                            if (dtCustomerDatetime.Rows.Count > 0)
                            {
                                wr.WorkorderEntryDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                                wr.WorkorderModifiedDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                            }
                            wr.EntryUserName = "Mass Upload";

                            if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["CallTypeID"])) && Regex.IsMatch(Convert.ToString(dt.Rows[i]["CallTypeID"]), @"^\d+$"))
                            {
                                int calltypeid = Convert.ToInt32(dt.Rows[i]["CallTypeID"]);
                                wr.WorkorderCalltypeid = calltypeid;
                                wr.WorkorderCalltypeDesc = (from p in FarmerBrothersEntitites.WorkorderTypes where p.CallTypeID == calltypeid select p.Description).FirstOrDefault();
                            }
                            wr.WorkorderEquipCount = Convert.ToInt16(dt.Rows[i]["CustomerCount"]);
                            wr.WorkorderTimeZone = (from p in FarmerBrothersEntitites.Zips where p.ZIP1 == customer.ZipCode.Substring(0, 5) select p.TimeZone).FirstOrDefault();
                            wr.CoverageZone = (from p in FarmerBrothersEntitites.ZoneZips where p.ZipCode == customer.ZipCode.Substring(0, 5) select p.ZoneIndex).FirstOrDefault();
                            var techids = (from p in FarmerBrothersEntitites.ZonePriorities
                                           where
                                         ((from q in FarmerBrothersEntitites.ZoneZips where q.ZipCode == customer.ZipCode.Substring(0, 5) select q.ZoneIndex).ToList().Contains(p.ZoneIndex))
                                           select new { p.ResponsibletechID, p.SecondaryTechID }).FirstOrDefault();

                            wr = fnInsertWorkOrderTech(wr, techids.ResponsibletechID, techids.SecondaryTechID);

                            IndexCounter counter = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "WorkorderID");
                            counter.IndexValue++;
                            wr.WorkorderID = counter.IndexValue.Value;   //wr.Notes = dt.Rows[i]["Notes"].ToString();
                            //   wr.CallerName = dt.Rows[i]["CallerName"].ToString();
                            if (Uploadetable.Rows[i]["WorkOrderID"].ToString() == "")
                                Uploadetable.Rows[i]["WorkOrderID"] = counter.IndexValue.Value;
                            FarmerBrothersEntitites.WorkOrders.Add(wr);
                            FarmerBrothersEntitites.SaveChanges();



                            foreach (string str in dt.Rows[i]["Brands"].ToString().Split(',').ToArray())
                            {
                                //wrkOrderBrands.BrandID = (from p in FormalBrothersEntitites.BrandNames where p.BrandName1 == str select p.BrandID).FirstOrDefault();
                                WorkOrderBrand wrkOrderBrands = new WorkOrderBrand();
                                wrkOrderBrands.WorkorderID = wr.WorkorderID;
                                wrkOrderBrands.BrandID = Convert.ToInt32(str);
                                FarmerBrothersEntitites.WorkOrderBrands.Add(wrkOrderBrands);
                                FarmerBrothersEntitites.SaveChanges();
                            }

                            fnNotesHistory(dt.Rows[i]["Notes"].ToString(), wr.WorkorderID, dtCustomerDatetime);
                            fnNotesHistory("Work Order Created By Upload", wr.WorkorderID, dtCustomerDatetime);
                            int workOrderCount = Convert.ToInt32(dt.Rows[i]["CustomerCount"]);
                            if (dt.Rows[i]["Status"].ToString().ToLower() == "accepted")
                            {
                                for (int j = 0; j < workOrderCount; j++)
                                {
                                    WorkorderEquipmentRequested wrEquipmentReq = new WorkorderEquipmentRequested();
                                    wrEquipmentReq.WorkorderID = wr.WorkorderID;
                                    IndexCounter Assetcounter = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "AssetID");
                                    Assetcounter.IndexValue++;
                                    wrEquipmentReq.Assetid = Assetcounter.IndexValue.Value;
                                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["CallTypeID"])))
                                        wrEquipmentReq.CallTypeid = Convert.ToInt32(dt.Rows[i]["CallTypeID"]);
                                    FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Add(wrEquipmentReq);
                                    FarmerBrothersEntitites.SaveChanges();

                                    WorkorderEquipment wrEquipment = new WorkorderEquipment();
                                    wrEquipment.WorkorderID = wr.WorkorderID;
                                    IndexCounter Assetcounter1 = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "AssetID");
                                    Assetcounter1.IndexValue++;
                                    wrEquipment.Assetid = Assetcounter1.IndexValue.Value;
                                    //wrEquipment.SerialNumber = dt.Rows[i]["SerialNo"].ToString();
                                    //if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Symptom"])))
                                    //   wrEquipment.Symptomid = Convert.ToInt32(dt.Rows[i]["Symptom"]);
                                    //wrEquipment.Location = dt.Rows[i]["Location"].ToString();
                                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["CallTypeID"])))
                                        wrEquipment.CallTypeid = Convert.ToInt32(dt.Rows[i]["CallTypeID"]);
                                    //wrEquipment.SerialNumber = dt.Rows[i]["CallerName"].ToString();
                                    FarmerBrothersEntitites.WorkorderEquipments.Add(wrEquipment);
                                    FarmerBrothersEntitites.SaveChanges();
                                }
                            }
                            else
                            {
                                for (int j = 0; j < workOrderCount; j++)
                                {
                                    WorkorderEquipmentRequested wrEquipmentReq = new WorkorderEquipmentRequested();
                                    wrEquipmentReq.WorkorderID = wr.WorkorderID;
                                    IndexCounter Assetcounter = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "AssetID");
                                    Assetcounter.IndexValue++;
                                    wrEquipmentReq.Assetid = Assetcounter.IndexValue.Value;
                                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["CallTypeID"])))
                                        wrEquipmentReq.CallTypeid = Convert.ToInt32(dt.Rows[i]["CallTypeID"]);
                                    FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Add(wrEquipmentReq);
                                    FarmerBrothersEntitites.SaveChanges();

                                }
                            }
                            if (dt.Rows[i]["Status"].ToString().ToLower() == "accepted" && dt.Rows[i]["TechID"] != DBNull.Value)
                            {
                                WorkorderSchedule wrkSchedule = new WorkorderSchedule();
                                IndexCounter counterSchedule = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "WorkorderID");
                                counterSchedule.IndexValue++;
                                wrkSchedule.WorkorderID = wr.WorkorderID;
                                wrkSchedule.Scheduleid = counterSchedule.IndexValue.Value;

                                if (dtCustomerDatetime.Rows.Count > 0)
                                {
                                    wrkSchedule.EntryDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                                    wrkSchedule.ScheduleDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                                }
                                wrkSchedule.Techid = Convert.ToInt32(dt.Rows[i]["TechID"]);
                                wrkSchedule.AssignedStatus = "Accepted";
                                wrkSchedule.AssistTech = -1;
                                wrkSchedule.PrimaryTech = 1;
                                DataTable dtTechData = objMarsView.fn_FSM_View("Select Tech_Name, ServiceCenter_Name, FSM_ID, FSM_Name from feast_tech_hierarchy where tech_id = " + dt.Rows[i]["TechID"]);
                                if (dtTechData.Rows.Count > 0)
                                {
                                    wrkSchedule.TechName = dtTechData.Rows[0]["Tech_Name"].ToString();
                                    wrkSchedule.ServiceCenterName = dtTechData.Rows[0]["ServiceCenter_Name"].ToString();
                                    if (dtTechData.Rows[0]["FSM_ID"].ToString() != "")
                                        wrkSchedule.FSMID = Convert.ToInt32(dtTechData.Rows[0]["FSM_ID"].ToString());
                                    wrkSchedule.FSMName = dtTechData.Rows[0]["FSM_Name"].ToString();
                                }

                                var ServiceCenterID = (from p in FarmerBrothersEntitites.Database.SqlQuery<double>(@"Select  Case left(Tech_Desc,4) 	 WHEN  'TPSP' THEN  ServiceCenter_Id    ELSE Default_service_center END  as ServiceCenter  from feast_tech_hierarchy  where Tech_Id=" + Convert.ToInt32(dt.Rows[i]["TechID"])) select p).FirstOrDefault();
                                if (ServiceCenterID != 0)
                                {
                                    wrkSchedule.ServiceCenterID = Convert.ToInt32(ServiceCenterID);
                                }
                                //  wrkSchedule.ServiceCenterID = Convert.ToInt32(objMarsView.fn_FSM_View(Sqlquery).Rows[0]["ServiceCenter"]);
                                FarmerBrothersEntitites.WorkorderSchedules.Add(wrkSchedule);
                                //  FormalBrothersEntitites.SaveChanges();

                            }
                        }

                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Uploadetable;
        }

        private DataTable MultipleWorkOrderDataTable(DataTable sourceDt)
        {
            foreach (DataColumn col in sourceDt.Columns)
            {
                col.ColumnName = col.ColumnName.Replace(" ", "").Trim();
            }

            var filterGroup = from d in sourceDt.AsEnumerable()
                              group d by new
                              {
                                  //  EventID = d["EventID"],
                                  CustomerNumber = d["CustomerNumber"],
                                  TechID = d["ServiceProviderJDE"],
                                  CustomerPO = d["CustomerPO"],
                                  Notes = d["Notes"],
                                  ProjectNumber = d["ProjectNumber"],
                                  AppointmentDate = d["AppointmentDate"],
                                  ContactName = d["ContactName"],
                                  ContactPhone = d["ContactPhone"],
                                  CallTypeID = d["CallTypeID"],
                                  Status = d["Status"],
                                  CallerName = d["CallerName"],
                                  Brands = d["Brands"],
                                  Priority = d["Priority"],

                              } into tempGrp
                              select new
                              {
                                  CustomerNumber = tempGrp.Key.CustomerNumber,
                                  TechID = tempGrp.Key.TechID,
                                  AppointmentDate = tempGrp.Key.AppointmentDate,
                                  ContactName = tempGrp.Key.ContactName,
                                  Notes = tempGrp.Key.Notes,
                                  ProjectNumber = tempGrp.Key.ProjectNumber,
                                  ContactPhone = tempGrp.Key.ContactPhone,
                                  CallTypeID = tempGrp.Key.CallTypeID,
                                  Status = tempGrp.Key.Status,
                                  CallerName = tempGrp.Key.CallerName,
                                  Brands = tempGrp.Key.Brands,
                                  Priority = tempGrp.Key.Priority,
                                  CustomerCount = tempGrp.Count(),
                              };
            var destDt = sourceDt.Clone();
            // destDt.Columns["Customer JDE"].ColumnName = "CustomerNumber";
            destDt.Columns["ServiceProviderJDE"].ColumnName = "TechID";

            destDt.Columns.Add("CustomerCount");
            foreach (var item in filterGroup)
            {

                DataRow dr = destDt.NewRow();

                if (item.CustomerNumber == DBNull.Value) { }
                else
                {
                    dr["CustomerNumber"] = item.CustomerNumber;
                    dr["TechID"] = item.TechID;
                    dr["AppointmentDate"] = item.AppointmentDate;
                    dr["ContactName"] = item.ContactName;
                    dr["Notes"] = item.Notes;
                    dr["ProjectNumber"] = item.ProjectNumber;
                    dr["ContactPhone"] = item.ContactPhone;
                    dr["CallTypeID"] = item.CallTypeID;
                    dr["Status"] = item.Status;
                    dr["CallerName"] = item.CallerName;
                    dr["Brands"] = item.Brands;
                    dr["Priority"] = item.Priority;
                    dr["CustomerCount"] = item.CustomerCount;
                    destDt.Rows.Add(dr);
                }
            }
            return destDt;
        }

        private void fnWorkOrderClosureInsert(DataTable dt)
        {
            try
            {
                int workorderid = Convert.ToInt32(dt.Rows[0]["WorkorderID"]);
                FarmerBrothersEntitites.WorkorderEquipments.RemoveRange(FarmerBrothersEntitites.WorkorderEquipments.Where(x => x.WorkorderID == workorderid));
                FarmerBrothersEntitites.SaveChanges();
                var distinctIds = dt.AsEnumerable().Select(s => new
                {
                    TechId = s.Field<string>("InvoiceNo"),

                })
                        .Distinct().ToList();
                foreach (var item in distinctIds)
                {
                    string invoiceid = (item.TechId).ToString();
                    FarmerBrothersEntitites.WorkorderDetails.RemoveRange(FarmerBrothersEntitites.WorkorderDetails.Where(x => x.InvoiceNo == invoiceid));
                    FarmerBrothersEntitites.SaveChanges();
                }
                var result = FarmerBrothersEntitites.WorkOrders.SingleOrDefault(b => b.WorkorderID == workorderid);

                if (result != null)
                {
                    result.WorkorderCallstatus = "Closed";
                    if (dt.Rows[0]["CompletionDateTime"] != null)
                        result.WorkorderCloseDate = Convert.ToDateTime(dt.Rows[0]["CompletionDateTime"]);
                    DataTable dtCustomerDatetime = objMarsView.fn_FSM_View("Select dbo.getCustDateTime('" + result.CustomerZipCode + "')");
                    if (dtCustomerDatetime.Rows.Count > 0)
                    {
                        result.WorkorderModifiedDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                    }
                    else
                    {
                        result.WorkorderModifiedDate = DateTime.Now;
                    }
                    result.EntryUserName = "Mass Upload";
                    FarmerBrothersEntitites.SaveChanges();
                }
                else
                {
                    FarmerBrothers.Data.WorkOrder wrkOrder = new FarmerBrothers.Data.WorkOrder();
                    FarmerBrothersEntitites.WorkOrders.Add(wrkOrder);
                    wrkOrder.WorkorderID = Convert.ToInt32(dt.Rows[0]["WorkorderID"]);
                    if (dt.Rows[0]["CompletionDateTime"] != null)
                        wrkOrder.WorkorderCloseDate = Convert.ToDateTime(dt.Rows[0]["CompletionDateTime"]);
                    wrkOrder.WorkorderCallstatus = "Closed";
                    result.WorkorderEntryDate = DateTime.Now;
                    result.WorkorderModifiedDate = DateTime.Now;
                    result.EntryUserName = "Mass Upload";
                    FarmerBrothersEntitites.SaveChanges();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    WorkorderEquipment wrEquipment = new WorkorderEquipment();
                    wrEquipment.WorkorderID = Convert.ToInt32(dt.Rows[i]["WorkorderID"]);
                    IndexCounter Assetcounter = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "AssetID");
                    Assetcounter.IndexValue++;
                    wrEquipment.Assetid = Assetcounter.IndexValue.Value;
                    wrEquipment.SerialNumber = dt.Rows[i]["M1Serial"].ToString();
                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1Symptom"])))
                        wrEquipment.Symptomid = Convert.ToInt32(dt.Rows[i]["M1Symptom"]);
                    if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1CallType"])))
                        wrEquipment.CallTypeid = Convert.ToInt32(dt.Rows[i]["M1CallType"]);
                    //   wrEquipment.SerialNumber = dt.Rows[i]["CallerName"].ToString();
                    wrEquipment.Model = (dt.Rows[i]["M1Model"]).ToString();
                    wrEquipment.Temperature = (dt.Rows[i]["Temparature1"]).ToString();
                    wrEquipment.Solutionid = Convert.ToInt32(dt.Rows[i]["M1Solution"]);
                    wrEquipment.Location = (dt.Rows[i]["Location"]).ToString();
                    wrEquipment.IsSlNumberImageExist = false;
                    FarmerBrothersEntitites.WorkorderEquipments.Add(wrEquipment);
                    FarmerBrothersEntitites.SaveChanges();
                }

                var Workdetailresults = (from p in dt.AsEnumerable()
                                         group p by new { Invoiceid = p["InvoiceNo"] } into dtworkdetails
                                         select new
                                         {
                                             EventID = dtworkdetails.FirstOrDefault()["WorkorderID"],
                                             invoiceId = dtworkdetails.Key.Invoiceid,
                                             signature = dtworkdetails.FirstOrDefault()["CustSignature"],
                                             StartDateTime = dtworkdetails.FirstOrDefault()["StartDateTime"],
                                             ArrivalDateTime = dtworkdetails.FirstOrDefault()["ArrivalDateTime"],
                                             CompletionDateTime = dtworkdetails.FirstOrDefault()["CompletionDateTime"],
                                             TechID = dtworkdetails.FirstOrDefault()["TechID"],
                                             MileagetoCustomer = dtworkdetails.FirstOrDefault()["MileagetoCustomer"],
                                         }
                              ).ToList();

                foreach (var item in Workdetailresults)
                {
                    WorkorderDetail wrkDetail = new WorkorderDetail();
                    wrkDetail.WorkorderID = Convert.ToInt32(item.EventID);
                    wrkDetail.InvoiceNo = (item.invoiceId).ToString();
                    //      wrkDetail.CustomerSignature = (item.signature).ToString() ; 
                    wrkDetail.StartDateTime = Convert.ToDateTime(item.StartDateTime);
                    wrkDetail.ArrivalDateTime = Convert.ToDateTime(item.ArrivalDateTime);
                    wrkDetail.CompletionDateTime = Convert.ToDateTime(item.CompletionDateTime);
                    if (!string.IsNullOrEmpty(Convert.ToString(item.MileagetoCustomer)))
                        wrkDetail.Mileage = Convert.ToDecimal(item.MileagetoCustomer);
                    FarmerBrothersEntitites.WorkorderDetails.Add(wrkDetail);
                    FarmerBrothersEntitites.SaveChanges();
                }


                SqlParameter sqlParam = new SqlParameter();
                sqlParam.ParameterName = "@closuredata";
                sqlParam.TypeName = "dbo.WorkOrderClosure";
                sqlParam.Value = dt;
                FarmerBrothersEntitites.Database.ExecuteSqlCommand("exec dbo.USP_Closure @closuredata", sqlParam);
                //param[0]=
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        private DataTable WorkOrderClosureDataTable(DataTable sourceDt)
        {
            foreach (DataColumn col in sourceDt.Columns)
            {
                col.ColumnName = col.ColumnName.Replace(" ", "").Trim();
            }
            var filterGroup =
                     (from d in sourceDt.AsEnumerable()
                      select new
                      {
                          WorkorderID = d["WorkorderID"],
                          InvoiceNo = d["InvoiceNo"],
                          M1CallType = d["M1CallType"],
                          M1Symptom = d["M1Symptom"],
                          M1System = d["M1System"],
                          M1Solution = d["M1Solution"],
                          M1MachineType = d["M1MachineType"],
                          //M1Make = d["M1 Make"],
                          M1Model = d["M1Model"],
                          M1Serial = d["M1Serial"],
                          M1Asset = d["M1Asset"],
                          Temparature1 = d["Temparature1"],
                          Ratio1 = d["Ratio1"],
                          //Refurb1 = d["Refurb1"],
                          //Action1 = d["Action1"],
                          //FailureDesc1 = d["FailureDesc1"],
                          CustSignature = d["CustSignature"],
                          StartDateTime = d["StartDateTime"],
                          ArrivalDateTime = d["ArrivalDateTime"],
                          CompletionDateTime = d["CompletionDateTime"],
                          TechID = d["TechID"],
                          MileagetoCustomer = d["MileagetoCustomer"],
                          Location = d["Location"]
                      }).ToList();

            var destDt = new DataTable();
            foreach (var item in (from DataColumn x in sourceDt.Columns
                                  select new { x.ColumnName, x.DataType }).ToList())
            {
                string columnname = item.ColumnName.Replace(" ", String.Empty);
                destDt.Columns.Add(columnname.Trim(), item.DataType);
            }
            //destDt.Columns.Add("CustomerCount");
            foreach (var item in filterGroup)
            {
                DataRow dr = destDt.NewRow();

                if (item.WorkorderID == DBNull.Value) { }
                else
                {

                    dr["WorkorderID"] = item.WorkorderID;
                    dr["InvoiceNo"] = item.InvoiceNo;
                    dr["M1CallType"] = item.M1CallType;
                    dr["M1Symptom"] = item.M1Symptom;
                    dr["M1System"] = item.M1System;
                    dr["M1Solution"] = item.M1Solution;
                    dr["M1MachineType"] = item.M1MachineType;
                    //dr["M1Make"] = item.M1Make;
                    dr["M1Model"] = item.M1Model;
                    dr["M1Serial"] = item.M1Serial;
                    dr["M1Asset"] = item.M1Asset;
                    dr["Temparature1"] = item.Temparature1;
                    dr["Ratio1"] = item.Ratio1;
                    //dr["Refurb1"] = item.Refurb1;
                    //dr["Action1"] = item.Action1;
                    //dr["FailureDesc1"] = item.FailureDesc1;
                    dr["CustSignature"] = item.CustSignature;
                    dr["StartDateTime"] = item.StartDateTime;
                    dr["ArrivalDateTime"] = item.ArrivalDateTime;
                    dr["CompletionDateTime"] = item.CompletionDateTime;
                    dr["TechID"] = item.TechID;
                    dr["MileagetoCustomer"] = item.MileagetoCustomer;
                    dr["Location"] = item.Location;
                    destDt.Rows.Add(dr);
                }
            }
            return destDt;
        }

        public ActionResult ErfUpload(HttpPostedFileBase UploadFile)
        {
            DataTable dt = new DataTable();
            DataTable Uploaddata = new DataTable();
            HttpPostedFileBase ErfFile = UploadFile;
            string fileExtension = Path.GetExtension(ErfFile.FileName);
            if (fileExtension.ToLower() != ".csv")
            {
                TempData["notice"] = "Upload Failed: Uploaded file is not a .CSV file.";
                return Content("");
            }
            if (ErfFile != null && ErfFile.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileName(ErfFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/App_Data/"), fileName.Replace('-', '_'));
                    ErfFile.SaveAs(path);
                    dt = ProcessCSV(path);
                    string colNames = "Category Id,New/Existing Customer,Customer Number,Mailing/Corporation Name,Address Line 1,City,State,Zip,Phone#,Contact Name/Title,Sales TSM#,Sales TSM Name,Sales Person Ph#,User Name,Date Received,Install Required (choose),Customer PO Number,Placement Reason,Ship to Loc Number,Ship From Loc Number,Contract Type,Install Location/Floor,Request Date,Project Number,NEW/Refurb,Equipment Prod No,EquipmentQty,NonSerializedQty,Ancillaries Prod No,User,Date Rcvd,Site Ready,Notes";
                    string str = CheckColumnNameExists(dt, colNames, 1);
                    if (str != "")
                    {
                        TempData["notice"] = str;
                        return Content("");
                    }

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    var dtFinal = ErfDataTable(dt, fileName);
                    Uploaddata = ErfUploadTest(dtFinal);
                    TempData["notice"] = "Successfully Uploaded.";
                }
                catch (Exception ex)
                {
                    TempData["notice"] = ex.Message;

                }

            }
            TempData["datasource"] = Uploaddata.Rows.Count > 0 ? Uploaddata : null;
            TempData["ExportToExcel"] = Uploaddata.Rows.Count > 0 ? Uploaddata : null;
            return Content("");

        }

        private static DataTable ProcessXlx(string fileName)
        {
            DataTable dt = new DataTable();
            string connectionString = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=YES\";", fileName);
            OleDbConnection excelConn = new OleDbConnection(connectionString);
            excelConn.Open();
            try
            {
                DataTable dtPatterns = new DataTable();
                DataTable dtsheet = new DataTable();
                dtsheet = excelConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string ExcelSheetName = dtsheet.Rows[0]["Table_Name"].ToString().TrimEnd('$');
                string query = String.Format("SELECT * from [{0}$]", ExcelSheetName);
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, connectionString);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                dt = dataSet.Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                return dt;
                throw ex;
            }
            finally
            {
                excelConn.Close();
            }
        }

        private DataTable ErfDataTable(DataTable sourceDt, string StrUploadFileName)
        {

            foreach (DataColumn col in sourceDt.Columns)
                col.ColumnName = col.ColumnName.Trim();
            var strQtyColumnName = "";
            // strQtyColumnName = StrUploadFileName.Replace(".csv", "") + "#csv.Qty1";
            try
            {
                var filterGroup =
                  (from d in sourceDt.AsEnumerable()
                   select new
                   {
                       DateInitiated = d[sourceDt.Columns[0].ColumnName],
                       CategoryId = d["Category Id"],
                       NewCustomer = d["New/Existing Customer"],
                       CustomerNumber = d["Customer Number"],
                       MailingName = d["Mailing/Corporation Name"],
                       AddressLine1 = d["Address Line 1"],
                       City = d["City"],
                       State = d["State"],
                       Zip = d["Zip"],
                       Phone = d["Phone#"],
                       ContactName = d["Contact Name/Title"],
                       SalesTSMId = d["Sales TSM#"],
                       SalesTSMName = d["Sales TSM Name"],
                       SalesPersonPhone = d["Sales Person Ph#"],
                       UserName = d["User Name"],
                       DateReceived = d["Date Received"],
                       InstallRequired = d["Install Required (choose)"],
                       CustomerPONumber = d["Customer PO Number"],
                       PlacementReason = d["Placement Reason"],
                       ShiptoLocNumber = d["Ship to Loc Number"],
                       ShipFromLocNumber = d["Ship From Loc Number"],
                       ContractType = d["Contract Type"],
                       InstallLocation = d["Install Location/Floor"],
                       RequestDate = d["Request Date"],
                       ProjectNumber = d["Project Number"],
                       NeworRefurb = d["NEW/Refurb"],
                       EquipmentProdNo = d["Equipment Prod No"],
                       //Quantity = d["Qty1"],
                       EquipmentQuantity = d["EquipmentQty"],
                       NonSerializedQty = d["NonSerializedQty"],
                       CatalogID = d["Ancillaries Prod No"],
                       User = d["User"],
                       DateRcvd = d["Date Rcvd"],
                       SiteReady = d["Site Ready"],
                       Notes = d["Notes"]
                   }).ToList();
                var destDt = ToDataTable(filterGroup);
                return destDt;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        private bool IsDate(String date)
        {
            try
            {
                DateTime dt = DateTime.Parse(date);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private DataTable ErfUploadTest(DataTable sourceDt)
        {
            var Erfdetailresults = (from d in sourceDt.AsEnumerable()
                                    group d by new
                                    {
                                        CustomerNumber = d["CustomerNumber"],
                                        MailingName = d["MailingName"],
                                        AddressLine1 = d["AddressLine1"],
                                        State = d["State"],
                                        Zip = d["Zip"],
                                        Phone = d["Phone"],
                                        ContactName = d["ContactName"],
                                        SalesTSMId = d["SalesTSMId"],
                                        SalesPersonPhone = d["SalesPersonPhone"],
                                        UserName = d["UserName"],
                                        SalesTSMName = d["SalesTSMName"],
                                        DateReceived = d["DateReceived"],
                                        RequestDate = d["RequestDate"],
                                        DateInitiated = d["DateInitiated"],
                                        InstallRequired = d["InstallRequired"],
                                        InstallLocation = d["InstallLocation"],
                                        CategoryId = d["CategoryId"],
                                        CustomerPONumber = d["CustomerPONumber"],
                                        ShiptoLocNumber = d["ShiptoLocNumber"],
                                        ShipFromLocNumber = d["ShipFromLocNumber"],
                                        User = d["User"],
                                        DateRcvd = d["DateRcvd"],
                                        ProjectNumber = d["ProjectNumber"]
                                    } into tempGrp
                                    select new
                                    {
                                        CustomerNumber = tempGrp.Key.CustomerNumber,
                                        Location = tempGrp.Key.InstallLocation,
                                        InstallRequired = tempGrp.Key.InstallRequired,
                                        UserName = tempGrp.Key.UserName,
                                        SalesTSMName = tempGrp.Key.SalesTSMName,
                                        ShiptoLocNumber = tempGrp.Key.ShiptoLocNumber,
                                        ShipFromLocNumber = tempGrp.Key.ShipFromLocNumber,
                                        CustomerPONumber = tempGrp.Key.CustomerPONumber,
                                        CustomerCount = tempGrp.Count(),
                                        DateReceived = tempGrp.Key.DateReceived,
                                        RequestDate = tempGrp.Key.RequestDate,
                                        DateInitiated = tempGrp.Key.DateInitiated,
                                        User = tempGrp.Key.User,
                                        CategoryId = tempGrp.Key.CategoryId,
                                        DateRcvd = tempGrp.Key.DateRcvd,
                                        ProjectNumber = tempGrp.Key.ProjectNumber,
                                        Phone = tempGrp.Key.Phone,
                                        ContactName = tempGrp.Key.ContactName,
                                    }).ToList();

            DataTable dtResult = ToDataTable(Erfdetailresults);
            DataTable Uploadetable = dtResult.Copy();
            //Uploadetable.Columns.Remove("DateRcvd");
            Uploadetable.Columns.Add("UploadStatus", typeof(string)).SetOrdinal(0);

            Uploadetable.Columns.Add("WorkOrderID", typeof(string)).SetOrdinal(2);
            Uploadetable.Columns.Add("ERFID", typeof(string)).SetOrdinal(3);
            try
            {

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (dtResult.Rows[i]["CustomerNumber"].ToString() == "")
                    {
                        DataRow dr = Uploadetable.Rows[i];
                        dr.Delete();

                    }
                    else
                    {
                        var customer = customerDetails(dtResult.Rows[i]["CustomerNumber"].ToString());
                        Uploadetable.Rows[i]["UploadStatus"] = Convert.ToInt32(customer.CustomerId) != 0 ? "Uploaded Sucessfully" : "Upload failed: Invalid Customer Number";
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["CategoryId"])) && !Regex.IsMatch(Convert.ToString(dtResult.Rows[i]["CategoryId"]), @"^\d+$"))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid CategoryId - CategoryId should be numeric.";
                            continue;
                        }
                        if (!IsCatalogValid((!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["CategoryId"])) ? Regex.IsMatch(Convert.ToString(dtResult.Rows[i]["CategoryId"]), @"^\d+$") ? Convert.ToString(dtResult.Rows[i]["CategoryId"]) : "0" : "0")))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid CategoryId.";
                            continue;
                        }
                        if (!dtResult.Rows[i]["InstallRequired"].ToString().ToUpper().Contains("YES"))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: InstallRequired column must contain 'Yes'";
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["ProjectNumber"])) && !Regex.IsMatch(Convert.ToString(dtResult.Rows[i]["ProjectNumber"]), @"^\d+$"))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid ProjectNumber - ProjectNumber should be numeric.";
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["RequestDate"])) && !IsDate(dtResult.Rows[i]["RequestDate"].ToString()))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid RequestDate.";
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["DateRcvd"])) && !IsDate(dtResult.Rows[i]["DateRcvd"].ToString()))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid Date in DateRcvd column.";
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["DateReceived"])) && !IsDate(dtResult.Rows[i]["DateReceived"].ToString()))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid Date in DateReceived column.";
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["DateInitiated"])) && !IsDate(dtResult.Rows[i]["DateInitiated"].ToString()))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid Date in DateInitiated column.";
                            continue;
                        }
                        if (string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["ShiptoLocNumber"])))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: ShiptoLocNumber shouldn't be empty.";
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["ShiptoLocNumber"])) && !Regex.IsMatch(Convert.ToString(dtResult.Rows[i]["ShiptoLocNumber"]), @"^\d+$"))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid ShiptoLocNumber - ShiptoLocNumber should be numeric.";
                            continue;
                        }
                        if (string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["ShipFromLocNumber"])))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: ShipFromLocNumber shouldn't be empty.";
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["ShipFromLocNumber"])) && !Regex.IsMatch(Convert.ToString(dtResult.Rows[i]["ShipFromLocNumber"]), @"^\d+$"))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid ShipFromLocNumber - ShipFromLocNumber should be numeric.";
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["ShipFromLocNumber"])) && !Regex.IsMatch(Convert.ToString(dtResult.Rows[i]["ShipFromLocNumber"]), @"^\d+$"))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid ShipFromLocNumber - ShipFromLocNumber should be numeric.";
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["ShipFromLocNumber"])) && !Regex.IsMatch(Convert.ToString(dtResult.Rows[i]["ShipFromLocNumber"]), @"^\d+$"))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid ShipFromLocNumber - ShipFromLocNumber should be numeric.";
                            continue;
                        }

                        DataTable dtWrkEquip = sourceDt.Select("CustomerNumber=" + dtResult.Rows[i]["CustomerNumber"]).CopyToDataTable();
                        var checkIsNumforEquipProd = true;
                        var checkIsNumforCatalogID = true;
                        foreach (DataRow dr in dtWrkEquip.Rows)
                        {
                            if (dr["EquipmentProdNo"].ToString() != "" && !Regex.IsMatch(dr["EquipmentProdNo"].ToString(), @"^\d+$"))
                            {
                                checkIsNumforEquipProd = false;
                                break;
                            }
                        }
                        if (checkIsNumforEquipProd == false)
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid Equipment Prod No - Equipment Prod No should be numeric.";
                            continue;
                        }
                        foreach (DataRow dr in dtWrkEquip.Rows)
                        {
                            if (dr["CatalogID"].ToString() != "" && !Regex.IsMatch(dr["CatalogID"].ToString(), @"^\d+$"))
                            {
                                checkIsNumforCatalogID = false;
                                break;
                            }
                        }
                        if (checkIsNumforCatalogID == false)
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed: Invalid Ancillaries Prod No - Ancillaries Prod No should be numeric.";
                            continue;
                        }


                        if (IsCatalogValid((!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["CategoryId"])) ? Regex.IsMatch(Convert.ToString(dtResult.Rows[i]["CategoryId"]), @"^\d+$") ? Convert.ToString(dtResult.Rows[i]["CategoryId"]) : "0" : "0")) && Convert.ToInt32(customer.CustomerId) != 0 && dtResult.Rows[i]["InstallRequired"].ToString().ToUpper().Contains("YES"))
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Uploaded Sucessfully";
                            string StrSql = string.Empty;
                            FarmerBrothers.Data.WorkOrder wr = new FarmerBrothers.Data.WorkOrder();
                            wr.CustomerID = Convert.ToInt32(customer.CustomerId);
                            wr.CustomerName = customer.CustomerName;
                            wr.CustomerAddress = customer.Address;
                            wr.CustomerCity = customer.City;
                            wr.CustomerState = customer.State;
                            wr.CustomerZipCode = customer.ZipCode;
                            wr.CustomerMainContactName = customer.MainContactName;
                            wr.CustomerPhone = customer.PhoneNumber;
                            wr.WorkorderContactName = dtResult.Rows[i]["ContactName"].ToString();
                            wr.WorkorderContactPhone = dtResult.Rows[i]["Phone"].ToString();
                            wr.CallerName= dtResult.Rows[i]["ContactName"].ToString();
                            wr.CustomerPhoneExtn = customer.PhoneExtn;
                            wr.WorkorderCallstatus = "Open";
                            wr.CustomerCustomerPreferences = customer.CustomerPreference;
                            wr.WorkorderEntryUserid = ERFConstants.WORKORDER_ENTRY_USER_ID;
                            wr.PriorityCode = ERFConstants.PRIRITY_CODE_OTHERS;
                            wr.FollowupCallID = 603;
                            //wr.Tsm = customer.TSM;
                            wr.Tsm = dtResult.Rows[i]["SalesTSMName"].ToString();
                            wr.MarketSegment = customer.MarketSegment;
                            wr.ProgramName = customer.ProgramName;
                            wr.DistributorName = customer.DistributorName;
                            wr.ServiceTier = customer.ServiceTier;
                            if (!string.IsNullOrEmpty(dtResult.Rows[i]["ProjectNumber"].ToString()) && Regex.IsMatch(Convert.ToString(dtResult.Rows[i]["ProjectNumber"]), @"^\d+$"))
                            {
                                wr.ProjectID = Convert.ToInt32(dtResult.Rows[i]["ProjectNumber"].ToString());
                            }
                            //wr.CustomerMainEmail = dt.Rows[i]["BusinessEmail"].ToString();
                            DataTable dtCustomerDatetime = objMarsView.fn_FSM_View("Select dbo.getCustDateTime('" + customer.ZipCode + "')");
                            if (dtCustomerDatetime.Rows.Count > 0)
                            {
                                wr.WorkorderEntryDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                                wr.WorkorderModifiedDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                            }
                            else
                            {
                                wr.WorkorderEntryDate = DateTime.Now.Date;
                                wr.WorkorderModifiedDate = DateTime.Now.Date;
                            }
                            wr.EntryUserName = "Mass Upload";
                            // int calltypeid = Convert.ToInt32(dtResult.Rows[i]["InstallRequired"].ToString().Substring(0, 4));
                            if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["InstallRequired"])))
                            {
                                int calltypeid = Convert.ToInt32(dtResult.Rows[i]["InstallRequired"].ToString().Substring(0, 4));
                                wr.WorkorderCalltypeid = calltypeid;
                                wr.WorkorderCalltypeDesc = (from p in FarmerBrothersEntitites.WorkorderTypes where p.CallTypeID == calltypeid select p.Description).FirstOrDefault();
                            }
                            wr.WorkorderEquipCount = Convert.ToInt16(dtResult.Rows[i]["CustomerCount"]);
                            wr.WorkorderTimeZone = (from p in FarmerBrothersEntitites.Zips where p.ZIP1 == customer.ZipCode.Substring(0, 5) select p.TimeZone).FirstOrDefault();
                            wr.CoverageZone = (from p in FarmerBrothersEntitites.ZoneZips where p.ZipCode == customer.ZipCode.Substring(0, 5) select p.ZoneIndex).FirstOrDefault();
                            var techids = (from p in FarmerBrothersEntitites.ZonePriorities
                                           where
                                            ((from q in FarmerBrothersEntitites.ZoneZips where q.ZipCode == customer.ZipCode.Substring(0, 5) select q.ZoneIndex).ToList().Contains(p.ZoneIndex))
                                           select new { p.ResponsibletechID, p.SecondaryTechID }).FirstOrDefault();
                            wr = fnInsertWorkOrderTech(wr, techids.ResponsibletechID, techids.SecondaryTechID);
                            IndexCounter counter = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "WorkorderID");
                            counter.IndexValue++;
                            wr.WorkorderID = counter.IndexValue.Value;   //wr.Notes = dt.Rows[i]["Notes"].ToString();
                            //   wr.CallerName = dt.Rows[i]["CallerName"].ToString();
                            FarmerBrothersEntitites.WorkOrders.Add(wr);
                            FarmerBrothersEntitites.SaveChanges();

                            Erf ErfInsert = new Erf();
                            ErfInsert.CustomerID = Convert.ToInt32(customer.CustomerId);
                            ErfInsert.WorkorderID = wr.WorkorderID;
                            ErfInsert.CustomerName = customer.CustomerName;
                            ErfInsert.CustomerMainContactName = customer.MainContactName;
                            ErfInsert.CustomerAddress = customer.Address;
                            ErfInsert.CustomerCity = customer.City;
                            ErfInsert.CustomerState = customer.State;
                            ErfInsert.CustomerZipCode = customer.ZipCode;
                            ErfInsert.CustomerPhone = customer.PhoneNumber;
                            //ErfInsert.CustomerPhone = dtResult.Rows[i]["Phone"].ToString();
                            ErfInsert.CustomerPhoneExtn = customer.PhoneExtn;
                            ErfInsert.CustomerMainEmail = customer.MainEmailAddress;
                            ErfInsert.SalesPerson = dtResult.Rows[i]["SalesTSMName"].ToString();
                            ErfInsert.DateOnERF = Convert.ToDateTime(dtResult.Rows[i]["RequestDate"]);
                            ErfInsert.DateERFReceived = Convert.ToDateTime(dtResult.Rows[i]["DateReceived"]);
                            ErfInsert.OriginalRequestedDate = Convert.ToDateTime(dtResult.Rows[i]["DateInitiated"]);
                            ErfInsert.InstallLocation = dtResult.Rows[i]["Location"].ToString();
                            ErfInsert.ShipToJDE = Convert.ToInt32(dtResult.Rows[i]["ShiptoLocNumber"]);
                            ErfInsert.ShipfromJDE = Convert.ToInt32(dtResult.Rows[i]["ShipFromLocNumber"]);
                            ErfInsert.UserName = dtResult.Rows[i]["UserName"].ToString();
                            ErfInsert.CustomerPO = dtResult.Rows[i]["CustomerPONumber"].ToString();
                            ErfInsert.CmUser = dtResult.Rows[i]["User"].ToString();
                            ErfInsert.DateReceived = Convert.ToDateTime(dtResult.Rows[i]["DateRcvd"]);
                            if (dtCustomerDatetime.Rows.Count > 0)
                            {
                                ErfInsert.EntryDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                                ErfInsert.ModifiedDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                            }
                            else
                            {
                                ErfInsert.EntryDate = DateTime.Now.Date;
                                ErfInsert.ModifiedDate = DateTime.Now.Date;
                            }

                            IndexCounter countererf = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "ERFNO");
                            countererf.IndexValue++;
                            ErfInsert.ErfID = (countererf.IndexValue.Value).ToString();

                            FarmerBrothersEntitites.Erfs.Add(ErfInsert);
                            FarmerBrothersEntitites.SaveChanges();
                            var tblErfAutomation = (from row in sourceDt.AsEnumerable()
                                                    where row.Field<string>("CustomerNumber") == (dtResult.Rows[i]["CustomerNumber"].ToString()) &&
                                                   (row.Field<string>("InstallLocation") == dtResult.Rows[i]["location"].ToString())
                                                    select row).ToList();
                            updateERFResponse ERFResponse = FeastUpdate(Convert.ToInt32(ErfInsert.ErfID), wr.WorkorderID, tblErfAutomation.CopyToDataTable());
                            int FeastID = 0;
                            if (ERFResponse != null && ERFResponse.MovementResponse.Movement != null && ERFResponse.MovementResponse.Movement.result.Contains("Success"))
                            {
                                FeastID = int.Parse(ERFResponse.MovementResponse.Movement.FeastID);
                                var Erfresult = FarmerBrothersEntitites.Erfs.SingleOrDefault(b => b.ErfID == ErfInsert.ErfID);
                                if (Erfresult != null)
                                {
                                    Erfresult.FeastMovementID = FeastID;
                                    FarmerBrothersEntitites.SaveChanges();
                                }
                            }
                            else
                            {
                                Uploadetable.Rows[i]["UploadStatus"] = "Not Update to feast. Error from feast: '" + ERFResponse.MovementResponse.ErrorMsg + "'";
                            }

                            if (FeastID != 0)
                            {
                                foreach (var equipitem in tblErfAutomation)
                                {
                                    if (equipitem["EquipmentQuantity"].ToString() != "")
                                    {
                                        for (int j = 0; j < Convert.ToInt32(equipitem["EquipmentQuantity"]); j++)
                                        {
                                            if (equipitem["EquipmentProdNo"].ToString() != "")
                                            {
                                                WorkorderEquipmentRequested wrEquipment = new WorkorderEquipmentRequested();
                                                if (!string.IsNullOrEmpty(Convert.ToString(dtResult.Rows[i]["InstallRequired"])))
                                                {
                                                    int calltypeid = Convert.ToInt32(dtResult.Rows[i]["InstallRequired"].ToString().Substring(0, 4));
                                                    wrEquipment.CallTypeid = calltypeid;
                                                }
                                                wrEquipment.FeastMovementid = FeastID;
                                                wrEquipment.WorkorderID = wr.WorkorderID;
                                                wrEquipment.Location = equipitem["InstallLocation"].ToString();
                                                wrEquipment.CatalogID = equipitem["EquipmentProdNo"].ToString();
                                                IndexCounter Assetcounter = FarmerBrothersEntitites.IndexCounters.FirstOrDefault(p => p.IndexName == "AssetID");
                                                Assetcounter.IndexValue++;
                                                wrEquipment.Assetid = Assetcounter.IndexValue.Value;
                                                //  wrEquipment
                                                FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Add(wrEquipment);
                                                FarmerBrothersEntitites.SaveChanges();
                                            }

                                        }
                                    }
                                    if (equipitem["NonSerializedQty"].ToString() != "")
                                    {

                                        for (int j = 0; j < Convert.ToInt32(equipitem["NonSerializedQty"]); j++)
                                        {
                                            if (equipitem["CatalogID"].ToString() != "")
                                            {

                                                NonSerialized nonSerialized = new NonSerialized();
                                                nonSerialized.Erfid = Convert.ToInt32(ErfInsert.ErfID);
                                                nonSerialized.WorkorderID = wr.WorkorderID;
                                                nonSerialized.FeastMovementid = FeastID;
                                                nonSerialized.Catalogid = equipitem["CatalogID"].ToString();
                                                FarmerBrothersEntitites.NonSerializeds.Add(nonSerialized);
                                                FarmerBrothersEntitites.SaveChanges();
                                            }

                                        }
                                    }
                                    //                        }
                                }
                                FeastMovement feastMovement = new FeastMovement();
                                feastMovement.WorkorderID = wr.WorkorderID;
                                feastMovement.Feastmovementid = FeastID;
                                feastMovement.FeastmovementState = "Draft";
                                feastMovement.Erfid = ErfInsert.ErfID;
                                FarmerBrothersEntitites.FeastMovements.Add(feastMovement);
                                FarmerBrothersEntitites.SaveChanges();
                            }
                            NotesHistory notesHistory = new NotesHistory();
                            notesHistory.AutomaticNotes = 1;
                            notesHistory.WorkorderID = wr.WorkorderID;
                            notesHistory.ErfID = ErfInsert.ErfID;
                            if (FeastID != 0)
                            {
                                notesHistory.FeastMovementID = FeastID;
                            }
                            if (dtCustomerDatetime.Rows.Count > 0)
                            {
                                notesHistory.EntryDate = Convert.ToDateTime(dtCustomerDatetime.Rows[0][0].ToString());
                            }
                            else
                            {
                                notesHistory.EntryDate = DateTime.Now;
                            }

                            notesHistory.Notes = (from row in sourceDt.AsEnumerable()
                                                  where row.Field<string>("CustomerNumber") == (dtResult.Rows[i]["CustomerNumber"].ToString()) &&
                                                 (row.Field<string>("InstallLocation") == dtResult.Rows[i]["location"].ToString())
                                                  select row).FirstOrDefault()["Notes"].ToString();
                            FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                            FarmerBrothersEntitites.SaveChanges();

                            ErfWorkorderLog erfLog = new ErfWorkorderLog();
                            erfLog.WorkorderID = wr.WorkorderID;
                            erfLog.ErfID = ErfInsert.ErfID;
                            FarmerBrothersEntitites.ErfWorkorderLogs.Add(erfLog);
                            FarmerBrothersEntitites.SaveChanges();
                            Uploadetable.Rows[i]["WorkOrderID"] = wr.WorkorderID.ToString();
                            Uploadetable.Rows[i]["ErfID"] = ErfInsert.ErfID.ToString(); ;
                        }

                        else
                        {
                            Uploadetable.Rows[i]["UploadStatus"] = "Upload failed";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            DataTable dtReturn = sourceDt.Copy();
            dtReturn.Columns.Remove("NewCustomer");
            dtReturn.Columns.Remove("MailingName");
            dtReturn.Columns.Remove("AddressLine1");
            dtReturn.Columns.Remove("SiteReady");
            dtReturn.Columns.Remove("NeworRefurb");
            dtReturn.Columns.Remove("ContractType");
            dtReturn.Columns.Add("UploadStatus", typeof(string)).SetOrdinal(0);
            dtReturn.Columns.Add("WorkOrderID", typeof(string)).SetOrdinal(1);
            dtReturn.Columns.Add("ERFID", typeof(string)).SetOrdinal(2);
            dtReturn.Columns["CustomerNumber"].SetOrdinal(3);
            dtReturn.Columns["InstallLocation"].SetOrdinal(4);
            dtReturn.Columns["InstallRequired"].SetOrdinal(5);
            dtReturn.Columns["SalesTSMName"].SetOrdinal(6);
            dtReturn.Columns["SalesPersonPhone"].SetOrdinal(7);
            dtReturn.Columns["ShiptoLocNumber"].SetOrdinal(8);
            dtReturn.Columns["ShipFromLocNumber"].SetOrdinal(9);
            dtReturn.Columns["CustomerPONumber"].SetOrdinal(10);
            dtReturn.Columns["DateReceived"].SetOrdinal(11);
            dtReturn.Columns["RequestDate"].SetOrdinal(12);
            dtReturn.Columns["DateInitiated"].SetOrdinal(13);
            dtReturn.Columns["User"].SetOrdinal(14);
            dtReturn.Columns["CategoryId"].SetOrdinal(15);
            dtReturn.Columns["DateRcvd"].SetOrdinal(16);
            dtReturn.Columns["ProjectNumber"].SetOrdinal(17);

            foreach (DataRow dr in Uploadetable.Rows)
            {
                DataRow[] drResult = dtReturn.Select("CustomerNumber=" + dr["CustomerNumber"] + " and InstallLocation ='" + dr["Location"].ToString() + "' and InstallRequired='" + dr["InstallRequired"].ToString() + "' and SalesTSMName='" + dr["SalesTSMName"].ToString() + "'");
                foreach (DataRow drUpdate in drResult)
                {
                    drUpdate["UploadStatus"] = dr["UploadStatus"];
                    drUpdate["WorkOrderID"] = dr["WorkOrderID"];
                    drUpdate["ERFID"] = dr["ERFID"];
                }
            }
            return dtReturn;
            //return Uploadetable;

        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;

        }
/*
        public updateERFResponse FeastUpdate(int eRFID, int workOrderID, DataTable tblErfAutomation)
        {

            FeastERFUpdateClient feastErfClient = new FeastERFUpdateClient();
            using (new OperationContextScope(feastErfClient.InnerChannel))
            {
                //   FeastERFUpdateService.FeastERFUpdateClient erfUpdate = new FeastERFUpdateService.FeastERFUpdateClient();
                FarmerBrothers.FeastERFUpdateService.WorkOrder WorkOrder = new FarmerBrothers.FeastERFUpdateService.WorkOrder();
                MovementRequest oMovementRequest = new MovementRequest();
                List<FarmerBrothers.FeastERFUpdateService.Serialized> SerializedList = new List<FarmerBrothers.FeastERFUpdateService.Serialized>();
                List<Part> PartList = new List<Part>();
                QuantityModel Quantity = new QuantityModel();
                FarmerBrothers.FeastERFUpdateService.Serialized SerializedItem;
                Part PartItem;

                IEnumerable<DataRow> SerializedEquipments = tblErfAutomation.AsEnumerable().ToList();

                foreach (DataRow SerializedEquipment in SerializedEquipments)
                {
                    if (SerializedEquipment["EquipmentProdNo"].ToString() != "")
                    {
                        SerializedItem = new FarmerBrothers.FeastERFUpdateService.Serialized();
                        SerializedItem.Equipment_Number = SerializedEquipment["EquipmentProdNo"].ToString();
                        SerializedItem.Qty = SerializedEquipment["EquipmentQuantity"].ToString();
                        SerializedList.Add(SerializedItem);
                    }
                    if (SerializedEquipment["CatalogID"].ToString() != "")
                    {
                        PartItem = new Part();
                        PartItem.Part_Number = SerializedEquipment["CatalogID"].ToString();
                        PartItem.Qty = SerializedEquipment["NonSerializedQty"].ToString();
                        PartList.Add(PartItem);
                    }
                }
                Quantity.SerializedLines = SerializedList.ToArray();
                Quantity.Parts = PartList.ToArray();

                WorkOrder.ERF = eRFID.ToString();

                if (workOrderID != 0)
                {
                    WorkOrder.WorkOrder1 = workOrderID.ToString();
                }
                WorkOrder.User_Id = tblErfAutomation.Rows[0]["User"].ToString();
                WorkOrder.Install_At_Location_ID = tblErfAutomation.Rows[0]["CustomerNumber"].ToString();
                //WorkOrder.Install_At_Location_ID = tblErfAutomation.Rows[0]["InstallLocation"].ToString();
                WorkOrder.Ship_To_Location_ID = tblErfAutomation.Rows[0]["ShiptoLocNumber"].ToString();
                WorkOrder.Ship_From_Location_ID = tblErfAutomation.Rows[0]["ShipFromLocNumber"].ToString();

                DateTime dt = Convert.ToDateTime(tblErfAutomation.Rows[0]["DateReceived"]);
                WorkOrder.Date_Requested = dt.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                string contractType = tblErfAutomation.Rows[0]["ContractType"].ToString();
                if (contractType.ToLower() == "loan")
                {
                    WorkOrder.Type = ERFConstants.WORKORDER_LOANTYPE;
                }
                else //if (contractType.ToLower() == "cash")
                {
                    WorkOrder.Type = ERFConstants.WORKORDER_CASHTYPE;
                }

                //   WorkOrder.User_Id = tblErfAutomation.Rows[19][1].ToString();
                WorkOrder.SerializedLines = Quantity.SerializedLines;
                WorkOrder.NonSerializedLines = Quantity.Parts;

                oMovementRequest.WorkOrder = WorkOrder;

                updateERFResponse ERFResponse = new updateERFResponse();

                HttpRequestMessageProperty requestMessage = new HttpRequestMessageProperty();
                requestMessage.Headers["username"] = "jmsmars";
                requestMessage.Headers["password"] = "jmsmars2";
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;

                //service.ClientCredentials.UserName.UserName = "username";
                //service.ClientCredentials.UserName.Password = "password";
                ERFResponse.MovementResponse = feastErfClient.updateERF(oMovementRequest);
                return ERFResponse;
            }
        }

        public class QuantityModel
        {
            public FarmerBrothers.FeastERFUpdateService.Serialized[] SerializedLines { get; set; }
            public Part[] Parts { get; set; }
        }
        
        public bool IsCatalogValid(string categoryID)
        {
            FarmerBrothers.FeastCatalogValidation.FeastCatalogValidatorClient feastCatalogValidatorClient = new FarmerBrothers.FeastCatalogValidation.FeastCatalogValidatorClient();
            using (new OperationContextScope(feastCatalogValidatorClient.InnerChannel))
            {

                FarmerBrothers.FeastCatalogValidation.CatalogValidationRequest catalogRequest = new FarmerBrothers.FeastCatalogValidation.CatalogValidationRequest();
                catalogRequest.CatalogID = categoryID;
                HttpRequestMessageProperty requestMessage = new HttpRequestMessageProperty();
                requestMessage.Headers["username"] = "jmsmars";
                requestMessage.Headers["password"] = "jmsmars2";
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;

                CatalogValidationResponse catalogResponse = feastCatalogValidatorClient.getCatalogValidation(catalogRequest);
                if (catalogResponse.ErrorMsg == null)
                {
                    if (catalogResponse.CatalogValidation != null)
                    {
                        if (catalogResponse.CatalogValidation[0].result.ToString().ToUpper() == "PASS")
                            return true;
                        return false;
                    }
                    return false;
                }
                return false;
            }
        }

        private string fnsqlQuery(int techId)
        {
            string StrSql = string.Empty;
            StrSql = " Select Tech_Name,ServiceCenter_Id,Tech_Id,CASE left(Tech_Desc,4)";
            StrSql += " WHEN  'TPSP' THEN  ServiceCenter_Id ";
            StrSql += " ELSE Default_service_center";
            StrSql += " END as  BranchID,Tech_Desc,FSM_Id,FSM_Name,TECH_PHONE,ServiceCenter_Name,TeamLead_ID,TeamLead_Name   from feast_tech_hierarchy";
            StrSql += " where tech_ID=" + techId;
            return StrSql;

        }

        private void fnNotesHistory(string Notes, int WorkorderId, DataTable dtCustomerDate)
        {

            NotesHistory notesHistory = new NotesHistory();
            notesHistory.AutomaticNotes = 1;
            //  notesHistory.NotesID = wr.WorkorderID + 1000;
            notesHistory.WorkorderID = WorkorderId;
            if (dtCustomerDate.Rows.Count > 0)
            {
                notesHistory.EntryDate = Convert.ToDateTime(dtCustomerDate.Rows[0][0].ToString());
            }
            else
            {
                notesHistory.EntryDate = DateTime.Now;
            }
            notesHistory.Notes = Notes;
            FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
            FarmerBrothersEntitites.SaveChanges();
        }

        private FarmerBrothers.Data.WorkOrder fnInsertWorkOrderTech(FarmerBrothers.Data.WorkOrder wr, int? ResponsibletechID, int? SecondaryTechID)
        {
            if (ResponsibletechID != null)
            {

                DataTable dtFeastHierarchy = objMarsView.fn_FSM_View(fnsqlQuery(ResponsibletechID ?? 0));
                if (dtFeastHierarchy.Rows.Count > 0)
                {
                    wr.ResponsibleTechBranch = Convert.ToInt32(dtFeastHierarchy.Rows[0]["BranchID"]);
                    wr.ResponsibleTechid = Convert.ToInt32(dtFeastHierarchy.Rows[0]["Tech_Id"]);
                    wr.ResponsibleTechPhone = (dtFeastHierarchy.Rows[0]["TECH_PHONE"]).ToString();
                    wr.ResponsibleTechName = (dtFeastHierarchy.Rows[0]["Tech_Name"]).ToString();
                    wr.Fsm = (dtFeastHierarchy.Rows[0]["FSM_Name"]).ToString();
                    wr.FSMID = Convert.ToInt32(dtFeastHierarchy.Rows[0]["FSM_Id"]);
                    wr.WorkorderFsm = Convert.ToInt32(dtFeastHierarchy.Rows[0]["FSM_Id"]);
                    if (dtFeastHierarchy.Rows[0]["TeamLead_ID"].ToString() != "")
                        wr.TechTeamLeadID = Convert.ToInt32(dtFeastHierarchy.Rows[0]["TeamLead_ID"]);
                    wr.TechTeamLead = dtFeastHierarchy.Rows[0]["TeamLead_Name"].ToString();
                }
            }
            if (SecondaryTechID != null)
            {

                DataTable dtSecondaryFeastHierarchy = objMarsView.fn_FSM_View(fnsqlQuery(SecondaryTechID ?? 0));
                if (dtSecondaryFeastHierarchy.Rows.Count > 0)
                {
                    wr.SecondaryTechBranch = Convert.ToInt32(dtSecondaryFeastHierarchy.Rows[0]["BranchID"]);
                    wr.SecondaryTechid = Convert.ToInt32(dtSecondaryFeastHierarchy.Rows[0]["Tech_Id"]);
                    wr.SecondaryTechPhone = (dtSecondaryFeastHierarchy.Rows[0]["TECH_PHONE"]).ToString();
                    wr.SecondaryTechName = (dtSecondaryFeastHierarchy.Rows[0]["Tech_Name"]).ToString();
                }
            }
            return wr;

        }
        [HttpPost]
        public void ExcelAction()
        {
            try
            {
                if (TempData["ExportToExcel"] != null)
                {
                    TempData.Keep("ExportToExcel");
                }
                string gridModel = HttpContext.Request.Params["GridModel"];
                GridProperties gridProperty = ConvertGridObject(gridModel);
                ExcelExport exp = new ExcelExport();
                exp.Export(gridProperty, TempData["ExportToExcel"], "UploadedData.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
            }
            catch (Exception ex)
            {
                // log the exception       
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);

            }
        }*/
    }
}