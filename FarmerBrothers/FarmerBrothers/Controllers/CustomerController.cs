using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class CustomerController : BaseController
    {
        public ActionResult CustomerDetails(int id)
        {
            CustomerModel customerModel = new CustomerModel();                        
            IList<Contact> customers = new List<Contact>();
            customers = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == id).ToList();
            DateTime CurrentTime = DateTime.Now;
            string billingCode = "";
            if (customers != null)
            {
                if (customers.Count > 0)
                {
                    customerModel = new CustomerModel(customers[0], FarmerBrothersEntitites);
                    customerModel = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, customerModel);
                    customerModel.PhoneNumber = Utility.FormatPhoneNumber(customers[0].PhoneWithAreaCode);
                    billingCode = customers[0].BillingCode;

                    CurrentTime = Utility.GetCurrentTime(customers[0].PostalCode, FarmerBrothersEntitites);
                }
            }
            customerModel.WorkOrderId = "1";
            IQueryable<WorkOrder> workOrders = FarmerBrothersEntitites.Set<WorkOrder>().Where(w => w.CustomerID == id).OrderByDescending(w => w.WorkorderID);
            customerModel.ServiceHistory = new List<ServiceHistoryModel>();
            foreach (WorkOrder workOrder in workOrders)
            {
                ServiceHistoryModel serviceHistoryModel = new ServiceHistoryModel()
                {
                    WorkOrderID = workOrder.WorkorderID.ToString(),
                    WorkOrderStatus = workOrder.WorkorderCallstatus
                };

                serviceHistoryModel.AppointmentDate = workOrder.AppointmentDate == null ? null : Convert.ToDateTime(workOrder.AppointmentDate).Date.ToShortDateString();
                serviceHistoryModel.DateCreated = workOrder.WorkorderEntryDate == null ? null : workOrder.WorkorderEntryDate.ToString();

                WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == workOrder.WorkorderCalltypeid).FirstOrDefault();
                if (workOrderType != null)
                {
                    serviceHistoryModel.WorkOrderType = workOrderType.Description;
                }

                if (workOrder.WorkorderSchedules.Count > 0)
                {
                    WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => (ws.AssignedStatus == "Accepted" || ws.AssignedStatus == "Scheduled") && ws.PrimaryTech >= 0).FirstOrDefault();
                    if (schedule != null)
                    {
                        serviceHistoryModel.Technician = schedule.TechName;

                        if(schedule.AssignedStatus == "Scheduled")
                        {
                            serviceHistoryModel.AppointmentDate = schedule.EventScheduleDate == null ? null : Convert.ToDateTime(schedule.EventScheduleDate).Date.ToShortDateString();
                        }
                    }
                }

                customerModel.ServiceHistory.Add(serviceHistoryModel);
            }

            customerModel.EquipmentSummary = new List<EquipmentSummaryModel>();
            List<FBCBE> fbcbeList = FarmerBrothersEntitites.FBCBEs.Where(cbe => cbe.CurrentCustomerId == id).ToList();
            foreach(FBCBE cbeData in fbcbeList)
            {
                EquipmentSummaryModel eqpSmryMdl = new EquipmentSummaryModel();
                eqpSmryMdl.Id = cbeData.Id;
                eqpSmryMdl.CustomerId = cbeData.CurrentCustomerId.ToString();
                eqpSmryMdl.ItemDescription = string.IsNullOrEmpty(cbeData.ItemDescription) ? "" : cbeData.ItemDescription;
                eqpSmryMdl.ItemNumber = string.IsNullOrEmpty(cbeData.ItemNumber) ? "" : cbeData.ItemNumber;
                eqpSmryMdl.SerialNumber = string.IsNullOrEmpty(cbeData.SerialNumber) ? "" : cbeData.SerialNumber;

                DateTime? transDt = cbeData.TransDate;
                DateTime? initialDt = cbeData.InitialDate;

                eqpSmryMdl.TransDate = transDt == null ? "" : transDt.ToString();
                eqpSmryMdl.InitialDate = initialDt == null ? "" : initialDt.ToString();

                string assetStatus = string.IsNullOrEmpty(cbeData.AssetStatus) ? "" : cbeData.AssetStatus.ToUpper();
                switch (assetStatus)
                {
                    case "A":
                        eqpSmryMdl.AssetStatus = "Loaned";
                        break;
                    case "X":
                        eqpSmryMdl.AssetStatus = "Sold";
                        break;
                    case "S":
                        eqpSmryMdl.AssetStatus = "Retired";
                        break;
                    default:
                        eqpSmryMdl.AssetStatus = assetStatus;
                        break;
                }
                //eqpSmryMdl.AssetStatus = cbeData.AssetStatus;

                decimal age = 0;
                decimal yearsInService = 0;

                //DateTime currDt = CurrentTime.ToShortDateString

                if (initialDt != null)
                {
                    age = Convert.ToDecimal(CurrentTime.Subtract(Convert.ToDateTime(initialDt)).TotalDays / 365);
                  
                    //age = Convert.ToDecimal(CurrentTime.ToShortDateString() - Convert.ToDateTime(initialDt).ToShortDateString()) / 365;
                }
                if (transDt != null)
                {
                    yearsInService = Convert.ToDecimal(CurrentTime.Subtract(Convert.ToDateTime(transDt)).TotalDays / 365);
                }


                eqpSmryMdl.Age = Decimal.Round(age, 1);
                eqpSmryMdl.YearsInService = Decimal.Round(yearsInService, 1);

                customerModel.EquipmentSummary.Add(eqpSmryMdl);
            }

            customerModel.States = Utility.GetStates(FarmerBrothersEntitites);
            customerModel.TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, id.ToString());
            if (!string.IsNullOrEmpty(billingCode))
            {
                customerModel.IsBillable = CustomerModel.IsBillableService(billingCode, customerModel.TotalCallsCount);
                customerModel.ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, billingCode);
            }
            else
            {
                customerModel.IsBillable = " ";
                customerModel.ServiceLevelDesc = " - ";
            }
            

            return View(customerModel);
        }

        [HttpPost]
        public void ServiceHistoryExcelExport(int id)
        {
            IQueryable<WorkOrder> workOrders = FarmerBrothersEntitites.Set<WorkOrder>().Where(w => w.CustomerID == id);
            IList<ServiceHistoryModel> serviceHistory = new List<ServiceHistoryModel>();
            foreach (WorkOrder workOrder in workOrders)
            {
                ServiceHistoryModel serviceHistoryModel = new ServiceHistoryModel()
                {
                    WorkOrderID = workOrder.WorkorderID.ToString(),
                    WorkOrderStatus = workOrder.WorkorderCallstatus
                };

                serviceHistoryModel.AppointmentDate = workOrder.AppointmentDate == null ? null : Convert.ToDateTime(workOrder.AppointmentDate).Date.ToShortDateString();
                serviceHistoryModel.DateCreated = workOrder.WorkorderEntryDate == null ? null : workOrder.WorkorderEntryDate.ToString();

                WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == workOrder.WorkorderCalltypeid).FirstOrDefault();
                if (workOrderType != null)
                {
                    serviceHistoryModel.WorkOrderType = workOrderType.Description;
                }

                WorkorderSchedule schedule = FarmerBrothersEntitites.WorkorderSchedules.Where(ws => ws.WorkorderID == workOrder.WorkorderID && ws.AssignedStatus == "Accepted" && ws.PrimaryTech >= 0).FirstOrDefault();
                if (schedule != null)
                {
                    serviceHistoryModel.Technician = schedule.TechName;
                }
                serviceHistory.Add(serviceHistoryModel);
            }

            string gridModel = HttpContext.Request.Params["GridModel"];
            GridProperties gridProperty = ConvertGridObject(gridModel);
            ExcelExport exp = new ExcelExport();
            exp.Export(gridProperty, serviceHistory, "WorkOrders.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "UpdateCustomer")]
        public JsonResult UpdateCustomer(CustomerModel customer)
        {
            var CustomerId = Convert.ToInt32(customer.CustomerId);
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
                    contact.LastName = string.Empty ;
                    for(int ind = 1; ind<names.Length;ind++)
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
            if (ValidateZipCode(customer.ZipCode))
            {
                FarmerBrothersEntitites.SaveChanges();
                
                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = 0, data = SendCustomerDetailsUpdateMail(customer, oldCustomer, Server.MapPath("~/img/mainlogo.jpg")) };
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

        public bool ValidateZipCode(string zipCode)
        {
            var zip = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == zipCode).FirstOrDefault();
            if (zip != null)
            {
                return true;
            }

            return false;
        }

        public bool SendCustomerDetailsUpdateMail(CustomerModel customer, CustomerModel serviceCustomer, string logoPath)
        {
            bool mailSent = false;
            try
            {
                var message = new MailMessage();

                int custID = Convert.ToInt32(customer.CustomerId);
                Contact contact = FarmerBrothersEntitites.Contacts.FirstOrDefault(c => c.ContactID == custID);                
                //var ESMDSMRSMs = FarmerBrothersEntitites.ESMDSMRSMs.FirstOrDefault(x => x.BranchNO == contact.Branch);                
                string mailTo = ConfigurationManager.AppSettings["CustomerUpdateMailToAddress"] ;

                //if (!string.IsNullOrEmpty(ESMDSMRSMs.CCMEmail))
                //{
                //    mailTo += ";" + ESMDSMRSMs.CCMEmail;
                //}
                //if (!string.IsNullOrEmpty(contact.CCMEmail))
                //{
                //    mailTo += ";" + contact.CCMEmail;
                //}
                if (!string.IsNullOrWhiteSpace(mailTo))
                {
                    string[] addresses = mailTo.Split(';');
                    foreach (string address in addresses)
                    {
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            message.To.Add(new MailAddress(address));
                        }
                    }

                    message.From = new MailAddress(ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"]);
                    message.Subject = ConfigurationManager.AppSettings["CustomerUpdateMailSubject"];

                    string contentId = Guid.NewGuid().ToString();
                    string htmlBody = customer.GetCustomerUpdateEmailText(serviceCustomer);
                    htmlBody = htmlBody.Replace("cid:logo", "cid:" + contentId);

                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString
                       (htmlBody, null, MediaTypeNames.Text.Html);

                    LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = contentId;
                    avHtml.LinkedResources.Add(inline);
                    message.AlternateViews.Add(avHtml);

                    message.IsBodyHtml = true;
                    message.Body = htmlBody.Replace("cid:logo", "cid:" + inline.ContentId);

                    using (var smtp = new SmtpClient())
                    {
                        smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                        smtp.Port = 25;
                        smtp.Send(message);
                    }
                    mailSent = true;
                }
            }
            catch (Exception e)
            {
                mailSent = false;
            }

            return mailSent;
        }


        #region Customer Zipcode Update
        [HttpGet]
        public ActionResult CustomerZipCodeUpdate()
        {
             CustomerZipcodeUpdateModel customrZipcodeModel = new CustomerZipcodeUpdateModel();
            return View(customrZipcodeModel);
        }

        [HttpGet]
        public JsonResult GetCustomerZipCode(int CustomerId)
        {
            CustomerZipcodeUpdateModel CusZipModel = new CustomerZipcodeUpdateModel();
            JsonResult jsonResult = new JsonResult();
            try
            {
                var CustomerDetails = (from p in FarmerBrothersEntitites.Contacts                                 
                                 where p.ContactID == CustomerId && (p.SearchType == "CA" || p.SearchType == "C" || p.SearchType == "XCA"
                                 || p.SearchType == "XC" || p.SearchType == "XCI" || p.SearchType == "CCS" || p.SearchType == "CFS" || p.SearchType == "CB"
                                 || p.SearchType == "CE" || p.SearchType == "CFD" || p.SearchType == "PFS")
                                 select new
                                 {
                                     CustomerId = p.ContactID,
                                     ZipCode = p.PostalCode,
                                     SalesEmail = p.SalesEmail
                                 }).FirstOrDefault();


                if (CustomerDetails != null)
                {
                    jsonResult.Data = new { success = true, data = CustomerDetails, message = "" };
                }
                else
                {
                    jsonResult.Data = new { success = true, message = "|Please Enter Valid Account Number" };
                }

            }
            catch (Exception)
            {

                jsonResult.Data = new { success = false };
            }


            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpPost]
        public JsonResult UpdateCustomerZipCode(int CustomerId, string ZipCode)
        {
            CustomerZipcodeUpdateModel workorderModel = new CustomerZipcodeUpdateModel();
            JsonResult jsonResult = new JsonResult();
            try
            {
                bool IsValidZip = ValidateZipCode(ZipCode);

                if (IsValidZip)
                {
                    Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == CustomerId).FirstOrDefault();

                    if (customer != null)
                    {
                        customer.PostalCode = ZipCode;
                        FarmerBrothersEntitites.SaveChanges();
                        jsonResult.Data = new { success = true, message = "" };
                    }
                    else
                    {
                        jsonResult.Data = new { success = true, message = "|Please Enter Valid Customer Id" };
                    }
                }
                else
                {
                    jsonResult.Data = new { success = true, message = "|Please Enter Valid Zip Code" };
                }
                
            }
            catch (Exception)
            {

                jsonResult.Data = new { success = false };
            }

            
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpPost]
        public JsonResult UpdateCustomerSalesEmail(int CustomerId, string SalesEmail)
        {
            CustomerZipcodeUpdateModel workorderModel = new CustomerZipcodeUpdateModel();
            JsonResult jsonResult = new JsonResult();
            try
            {
                Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == CustomerId).FirstOrDefault();
                if (customer != null)
                {
                    if (string.IsNullOrEmpty(SalesEmail))
                    {
                        customer.SalesEmail = "";
                        FarmerBrothersEntitites.SaveChanges();
                        jsonResult.Data = new { success = true, message = "" };
                    }
                    else
                    {
                        bool IsValidEmail = Utility.isValidEmail(SalesEmail);
                        if (IsValidEmail)
                        {
                            customer.SalesEmail = SalesEmail;
                            FarmerBrothersEntitites.SaveChanges();
                            jsonResult.Data = new { success = true, message = "" };
                        }
                        else
                        {
                            jsonResult.Data = new { success = true, message = "|Please Enter Valid Email" };
                        }
                    }
                }
                else
                {
                    jsonResult.Data = new { success = true, message = "|Please Enter Valid Customer Id" };
                }
            }
            catch (Exception)
            {

                jsonResult.Data = new { success = false };
            }


            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        #endregion


        #region PMUpload Contact Update        
        [HttpGet]
        public ActionResult PMUploadContactUpdate()
        {
            PMUploadContactUpdateModel pmUploadContactUpdateModel = new PMUploadContactUpdateModel();
            List<PMUploadContactUpdateModel> pmuploadlist = new List<PMUploadContactUpdateModel>();

            List<Contact_PMUploadsALL> pmUploadsList = FarmerBrothersEntitites.Contact_PMUploadsALL.DistinctBy(x => x.ContactID).ToList();//      Distinct().ToList();

            foreach (var user in pmUploadsList)
            {
                PMUploadContactUpdateModel userResultModel = new PMUploadContactUpdateModel(user);
                pmuploadlist.Add(userResultModel);
            }
            ViewBag.datasource = pmuploadlist;

            return View(pmUploadContactUpdateModel);
        }


        public ActionResult PMUploadContactDataUpdate(PMUploadContactUpdateModel value)
        {
            IList<PMUploadContactUpdateModel> PMUploadContactsData = TempData["PMUploadContacts"] as IList<PMUploadContactUpdateModel>;
            if (PMUploadContactsData == null)
            {
                PMUploadContactsData = new List<PMUploadContactUpdateModel>();
            }

            PMUploadContactsData.Add(value);
            TempData["PMUploadContacts"] = PMUploadContactsData;
            TempData.Keep("PMUploadContacts");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SavePMUploadsData()
        {
            try
            {
                IList<PMUploadContactUpdateModel> pmUploadsDataList = TempData["PMUploadContacts"] as IList<PMUploadContactUpdateModel>;

                if (pmUploadsDataList != null)
                {
                    foreach (PMUploadContactUpdateModel item in pmUploadsDataList)
                    {
                        FarmerBrothersEntitites.Contact_PMUploadsALL.Where(c => c.ContactID == item.AccountNumber).ToList().ForEach(cc => cc.IsActive = item.IsActive);

                        //if (contactData != null)
                        //{
                        //    contactData.ContactName = item.ContactName;
                        //    contactData.CustomerName = item.CustomerName;
                        //    contactData.IsActive = item.IsActive;
                        //}
                        //else
                        //{
                        //    contactData = new Contact_PMUploadsALL();
                        //    contactData.ContactName = item.ContactName;
                        //    contactData.CustomerName = item.CustomerName;
                        //    contactData.IsActive = item.IsActive;
                        //    FarmerBrothersEntitites.Contact_PMUploadsALL.Add(contactData);
                        //}
                    }
                }
                //FarmerBrothersEntitites.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            JsonResult jsonResult = new JsonResult();
            //jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        #endregion

        #region New Customer Upload

        [HttpPost]
        public ActionResult NewCustomerUploadFile_Old(HttpPostedFileBase file)
        {
            try
            {
                if (file == null)
                {
                    ViewBag.Message = "No File Selected ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<CustomerModel>();
                    return View("CustomerZipCodeUpdate");
                }

                else if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                {
                    ViewBag.Message = "Selected file is not CSV file ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<CustomerModel>();
                    return View("CustomerZipCodeUpdate");
                }

                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string DirPath = Server.MapPath("~/UploadedFiles/Customer");

                    if (!Directory.Exists(DirPath))
                    {
                        Directory.CreateDirectory(DirPath);
                    }
                    string _inputPath = Path.Combine(DirPath, _FileName);
                    file.SaveAs(_inputPath);

                        
                    FileReading fileData = FileReading.ReadCustomerCSVFile(DirPath, _FileName);

                    if (fileData != null && fileData.IsValid)
                    {
                        CustomerModel.InsertCustomerData(fileData.CustomerDataList, FarmerBrothersEntitites);
                        string CompletedFilePath = Path.Combine(DirPath, "Completed");
                        if (!Directory.Exists(CompletedFilePath))
                        {
                            Directory.CreateDirectory(CompletedFilePath);
                        }

                        string _completedPath = Path.Combine(CompletedFilePath, _FileName);
                        if (System.IO.File.Exists(_completedPath))
                        {
                            System.IO.File.Delete(_completedPath);
                        }
                        System.IO.File.Move(_inputPath, _completedPath);

                        ViewBag.Message = "File Uploaded Successfully!!";
                        ViewBag.isSuccess = true;
                        ViewBag.dataSource = fileData.EsmDataList;
                    }
                    else
                    {
                        //sendEmail(esmData);
                        ViewBag.Message = "File upload failed!! " + "\n" + fileData.ErrorMsg;
                        ViewBag.isSuccess = false;
                        ViewBag.dataSource = new List<CustomerModel>();
                    }
                }

                return View("CustomerZipCodeUpdate");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "File upload failed!! " + ex;
                ViewBag.isSuccess = false;
                ViewBag.dataSource = new List<CustomerModel>();
                return View("CustomerZipCodeUpdate");
            }
        }

        public ActionResult NewCustomerUploadFile(HttpPostedFileBase file)
        {
            try
            {
                List<CustomerModel> customerList = new List<CustomerModel>();
                if (file == null)
                {
                    ViewBag.Message = "No File Selected ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<CustomerModel>();
                    return View("CustomerZipCodeUpdate");
                }

                else if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                {
                    ViewBag.Message = "Selected file is not CSV file ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<CustomerModel>();
                    return View("CustomerZipCodeUpdate");
                }

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
                                       
                    string _path = Path.Combine(DirPath, _FileName);

                    var contents = System.IO.File.ReadAllText(_path).Split('\n');
                    FileReading fileDataObj = new FileReading();
                    fileDataObj.IsValid = true;
                    fileDataObj.FileName = _FileName;
                    int i = 0;

                    foreach (string line in contents)
                    {
                        if (string.IsNullOrEmpty(line)) continue;
                        string lineVal = line.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace('\\', ' ').Replace("\"", "");
                        if (string.IsNullOrEmpty(lineVal) || lineVal == " ") continue;

                        if (i == 0)
                        {
                            fileDataObj = IsValidCustomerCSVFile(lineVal);

                            if (!fileDataObj.IsValid)
                            {
                                ViewBag.Message = "File upload failed!! " + "\n" + fileDataObj.ErrorMsg;
                                ViewBag.isSuccess = false;
                                ViewBag.dataSource = new List<CustomerModel>();
                                return View("CustomerZipCodeUpdate");
                            }
                        }
                        string[] lineValues = lineVal.Split(',');
                        if (i != 0)
                        {                            
                            string CustomerId = "", CustomerName = "", Address1 = "", Address2 = "", Address3 = "", City = "", State = "", ZipCode = "", PhoneNumber = "", Route = "", Branch = "", RouteCode = "";
                            string ErrorMessage = "";
                            for (int ind = 0; ind <= lineValues.Count() - 1; ind++)
                            {
                                string str = lineValues[ind].Trim();
                                switch (ind)
                                {
                                    case 0:
                                        CustomerId = string.IsNullOrEmpty(str) ? "" : str;
                                        if(string.IsNullOrEmpty(CustomerId)) { ErrorMessage += "Customer Number is Missing"; }
                                        break;
                                    case 1:
                                        CustomerName = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(CustomerName)) { ErrorMessage += "Customer Name is Missing"; }
                                        break;
                                    case 2:
                                        Address1 = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(Address1)) { ErrorMessage += "Address1 is Missing"; }
                                        break;
                                    case 3:
                                        Address2 = string.IsNullOrEmpty(str) ? "" : str.ToString();                                        
                                        break;
                                    case 4:
                                        Address3 = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 5:
                                        City = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(City)) { ErrorMessage += "City is Missing"; }
                                        break;
                                    case 6:
                                        State = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(State)) { ErrorMessage += "State is Missing"; }
                                        break;
                                    case 7:
                                        ZipCode = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(ZipCode)) { ErrorMessage += "ZipCode is Missing"; }
                                        break;
                                    case 8:
                                        PhoneNumber = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 9:
                                        Route = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(Route)) { ErrorMessage += "Route is Missing"; }
                                        break;
                                    case 10:
                                        Branch = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(Branch)) { ErrorMessage += "Branch is Missing"; }
                                        break;
                                    case 11:
                                        RouteCode = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                }
                            }

                            int contactId = 0;
                            try
                            {
                                contactId = string.IsNullOrEmpty(CustomerId) ? 0 : Convert.ToInt32(CustomerId);
                            }
                            catch(Exception ex)
                            {
                                ErrorMessage += "Customer Number is not in Correct Format";
                            }
                            if (!string.IsNullOrEmpty(ErrorMessage))
                            {
                                CustomerModel cm = new CustomerModel();
                                cm.CustomerId = CustomerId;
                                cm.CustomerName = CustomerName;
                                cm.Address = Address1;
                                cm.Address2 = Address2;
                                cm.Address3 = Address3;
                                cm.City = City;
                                cm.State = State;
                                cm.ZipCode = ZipCode;
                                cm.PhoneNumber = PhoneNumber;
                                cm.Route = Route;
                                cm.Branch = Branch;
                                cm.RouteCode = RouteCode;
                                cm.CustomerBranch = Branch;
                                cm.Message = ErrorMessage;

                                customerList.Add(cm);

                                continue;
                            }
                            
                            if (contactId > 0)
                            {
                                Contact contacttem = FarmerBrothersEntitites.Contacts.Where(cr => cr.ContactID == contactId).FirstOrDefault();
                                if (contacttem == null)
                                {
                                    Contact contact = new Contact();
                                    contact.ContactID = contactId;
                                    contact.CompanyName = CustomerName;
                                    contact.Address1 = Address1;
                                    contact.Address2 = Address2;
                                    contact.Address3 = Address3;
                                    contact.City = City;
                                    contact.State = State;
                                    contact.PostalCode = ZipCode;
                                    contact.Phone = PhoneNumber;
                                    contact.Route = Route;
                                    contact.Branch = Branch;
                                    contact.RouteCode = RouteCode;
                                    contact.DateCreated = currentDate;
                                    contact.LastModified = currentDate;
                                    contact.SearchType = "C";
                                    contact.CustomerBranch = Branch;

                                    FarmerBrothersEntitites.Contacts.Add(contact);

                                }
                                else
                                {
                                    contacttem.ContactID = contactId;
                                    contacttem.CompanyName = CustomerName;
                                    contacttem.Address1 = Address1;
                                    contacttem.Address2 = Address2;
                                    contacttem.Address3 = Address3;
                                    contacttem.City = City;
                                    contacttem.State = State;
                                    contacttem.PostalCode = ZipCode;
                                    contacttem.Phone = PhoneNumber;
                                    contacttem.Route = Route;
                                    contacttem.Branch = Branch;
                                    contacttem.RouteCode = RouteCode;
                                    contacttem.LastModified = currentDate;
                                    contacttem.SearchType = "C";
                                    contacttem.CustomerBranch = Branch;
                                }

                                try
                                {
                                    int result = FarmerBrothersEntitites.SaveChanges();

                                    CustomerModel cm = new CustomerModel();
                                    cm.CustomerId = CustomerId;
                                    cm.CustomerName = CustomerName;
                                    cm.Address = Address1;
                                    cm.Address2 = Address2;
                                    cm.Address3 = Address3;
                                    cm.City = City;
                                    cm.State = State;
                                    cm.ZipCode = ZipCode;
                                    cm.PhoneNumber = PhoneNumber;
                                    cm.Route = Route;
                                    cm.Branch = Branch;
                                    cm.RouteCode = RouteCode;
                                    cm.CustomerBranch = Branch;
                                    if (result == 1)
                                    {   
                                        cm.Message = "Success";
                                    }
                                    else
                                    {
                                        cm.Message = "Error Saving";
                                    }
                                    customerList.Add(cm);
                                }
                                catch (Exception ex)
                                {
                                    ErrorMessage += "Problem Saving Contact,  Exception: " + ex.ToString();

                                    CustomerModel cm = new CustomerModel();
                                    cm.CustomerId = CustomerId;
                                    cm.CustomerName = CustomerName;
                                    cm.Address = Address1;
                                    cm.Address2 = Address2;
                                    cm.Address3 = Address3;
                                    cm.City = City;
                                    cm.State = State;
                                    cm.ZipCode = ZipCode;
                                    cm.PhoneNumber = PhoneNumber;
                                    cm.Route = Route;
                                    cm.Branch = Branch;
                                    cm.RouteCode = RouteCode;
                                    cm.CustomerBranch = Branch;
                                    cm.Message = ErrorMessage;

                                    customerList.Add(cm);
                                    continue;
                                }
                            }
                        }
                        i++;
                    }
                    
                }

                ViewBag.Message = "File uploaded ! ";
                ViewBag.isSuccess = true;
                ViewBag.dataSource = customerList;
                return View("CustomerZipCodeUpdate");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "File upload failed!! " + ex;
                ViewBag.isSuccess = false;
                ViewBag.dataSource = new List<CustomerModel>();
                return View("CustomerZipCodeUpdate");
            }
        }

        private static FileReading IsValidCustomerCSVFile(string HeaderRow)
        {
            FileReading fr = new FileReading();
            fr.ErrorMsg = "";
            fr.IsValid = true;

            string[] headerValues = HeaderRow.Split(',');

            for (var index = 0; index <= headerValues.Count() - 1; index++)
            {
                string hdrValue = headerValues[index].ToLower().Trim();

                switch (index)
                {
                    case 0:
                        if (hdrValue != "customer number")
                        {
                            fr.ErrorMsg += "\n Customer  Number Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 1:
                        if (hdrValue != "company")
                        {
                            fr.ErrorMsg += "\n Company Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 2:
                        if (hdrValue != "address1")
                        {
                            fr.ErrorMsg += "\n Address1 Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 3:
                        if (hdrValue != "address2")
                        {
                            fr.ErrorMsg += "\n Address2 Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 4:
                        if (hdrValue != "address3")
                        {
                            fr.ErrorMsg += "\n Address3 Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 5:
                        if (hdrValue != "city")
                        {
                            fr.ErrorMsg += "\n City Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 6:
                        if (hdrValue != "state")
                        {
                            fr.ErrorMsg += "\n State Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 7:
                        if (hdrValue != "postalcode")
                        {
                            fr.ErrorMsg += "\n PostalCode Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 8:
                        if (hdrValue != "phone")
                        {
                            fr.ErrorMsg += "\n Phone Number Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 9:
                        if (hdrValue.Trim() != "route")
                        {
                            fr.ErrorMsg += "\n Route Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 10:
                        if (hdrValue.Trim() != "branch")
                        {
                            fr.ErrorMsg += "\n Branch Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 11:
                        if (hdrValue.Trim() != "route code")
                        {
                            fr.ErrorMsg += "\n Route Code Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                }
            }

            return fr;
        }


        #endregion
    }

}