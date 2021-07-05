using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Customer = FarmerBrothers.Data.Contact;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;

namespace FarmerBrothers.Controllers
{
    public class UnknownCustomerController : BaseController
    {
        // GET: Admin
        public ActionResult unknownCustomer()
        {
            WorkorderSearchModel unknownCustomers = new WorkorderSearchModel();
            unknownCustomers.SearchResults = GetUnKnownWorkOrders();
            return View("UnknownCustomer", unknownCustomers);
        }

        public List<WorkorderSearchResultModel> GetUnKnownWorkOrders()
        {
            List<WorkorderSearchResultModel> unknownWorkOrders = new List<WorkorderSearchResultModel>();
                        
            SqlHelper helper = new SqlHelper();
            string query = @"select w.WorkorderID,CustomerID,WorkorderCallstatus,c.CompanyName as CustomerName,c.Address1,
                            c.City as CustomerCity,c.State CustomerState,
                            c.PostalCode as CustomerZipCode,w.WorkorderEntryDate,t.DealerId,t.CompanyName as TechnicianName from WorkOrder w
                            inner join Contact c on w.CustomerID = c.ContactID
                            left join WorkorderSchedule ws on w.WorkorderID = ws.WorkorderID and ws.AssignedStatus = 'Accepted'
                            left join TECH_HIERARCHY t on ws.Techid = t.DealerId
                            where c.IsUnknownUser = 1";

            DataTable dt = helper.GetDatatable(query);
            foreach (DataRow dr in dt.Rows)
            {
                WorkorderSearchResultModel wsresults = new WorkorderSearchResultModel(dr, true);
                unknownWorkOrders.Add(wsresults);
            }

            return unknownWorkOrders;
        }

        public Customer customerDetails(string customerID)
        {
            CustomerModel customerModel = new CustomerModel();
            Contact cust = null;
            Customer custDetails = new Customer();
            if (!string.IsNullOrEmpty(customerID))
            {
                int id = Convert.ToInt32(customerID);
                cust = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == id).FirstOrDefault();
            }
            if (cust != null)
            {
                customerModel = new CustomerModel(cust, FarmerBrothersEntitites);
                custDetails.Address1 = customerModel.Address;
                custDetails.City = customerModel.City;
                custDetails.State = customerModel.State;
                custDetails.CompanyName = customerModel.CustomerName;
            }
            return custDetails;
        }


        public ActionResult unknowsCustomerDetails(string CustID)
        {
            int customerID = 0;
            if (CustID != null)
                customerID = Int32.Parse(CustID);

            Customer custDetails = customerDetails(customerID.ToString());

            WorkorderSearchModel unknowCustDetails = new WorkorderSearchModel();
            unknowCustDetails.SearchResult = custDetails;
            unknowCustDetails.WorkOrderResults = FarmerBrothersEntitites.WorkOrders.Where(c => c.WorkorderCallstatus == "Hold for AB").ToList();

            return View("UnknownCustomer", unknowCustDetails);
        }

        public JsonResult GetCustomerDetails(string custID)
        {
            Customer custDetails = null;
            if (!string.IsNullOrWhiteSpace(custID))
            {
                custDetails = customerDetails(custID);
            }
            return Json(custDetails, JsonRequestBehavior.AllowGet);
        }

        public ActionResult orderDetails(int? customerId, int? workOrderId,int? unknownCustomerId)
        {
            var wkrorder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();

            var nonServiceWorkOrder = FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == workOrderId).FirstOrDefault();

            int custId = Convert.ToInt32(customerId);
            Contact contact = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == custId).FirstOrDefault();
            CustomerModel customerModel = new CustomerModel(contact, FarmerBrothersEntitites);
            customerModel = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, customerModel);
            FarmerBrothers.Data.UnKnownCustomerLog unknownCustLog = new Data.UnKnownCustomerLog();

            unknownCustLog.CustomerLogID = wkrorder != null ? Convert.ToInt32(wkrorder.CustomerID) : Convert.ToInt32(nonServiceWorkOrder.CustomerID);
            unknownCustLog.NewCustomerID = Convert.ToInt32(customerId);
            unknownCustLog.ModifiedDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
            unknownCustLog.OldCustomerID = wkrorder != null ? Convert.ToInt32(wkrorder.CustomerID) : Convert.ToInt32(nonServiceWorkOrder.CustomerID); ;
            unknownCustLog.ModifiedUserID = customerId;
            unknownCustLog.ModifiedUserName = customerModel.CustomerName;
            FarmerBrothersEntitites.UnKnownCustomerLogs.Add(unknownCustLog);
            FarmerBrothersEntitites.SaveChanges();

            if (wkrorder == null)
            {
                nonServiceWorkOrder.CustomerID = customerId;
                nonServiceWorkOrder.CustomerCity = customerModel.City;
                nonServiceWorkOrder.CustomerState = customerModel.State;
                nonServiceWorkOrder.CustomerZipCode = customerModel.ZipCode;
                nonServiceWorkOrder.IsUnknownWorkOrder = false;
                
                DateTime currentTime = Utility.GetCurrentTime(customerModel.ZipCode, FarmerBrothersEntitites);
                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = @"Unknown Customer ID updated from " + unknownCustLog.OldCustomerID + " to " + unknownCustLog.NewCustomerID + " from MARS by " + UserName,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, 
                    UserName = UserName,
                    isDispatchNotes = 1
                };
                nonServiceWorkOrder.NotesHistories.Add(notesHistory);

                WorkorderController wc = new WorkorderController();
                wc.UpdateWOModifiedElapsedTime(Convert.ToInt32(workOrderId));

                FarmerBrothersEntitites.SaveChanges();
            }
            else
            {
                wkrorder.CustomerID = customerId;
                wkrorder.CustomerAddress = customerModel.Address;
                wkrorder.CustomerCity = customerModel.City;
                wkrorder.CustomerState = customerModel.State;
                wkrorder.CustomerName = customerModel.CustomerName;
                wkrorder.CustomerZipCode = customerModel.ZipCode;
                wkrorder.CustomerMainContactName = customerModel.MainContactName;
                wkrorder.CustomerPhone = customerModel.PhoneNumber;
                wkrorder.CustomerMainEmail = customerModel.MainEmailAddress;
                
                DateTime currentTime = Utility.GetCurrentTime(customerModel.ZipCode, FarmerBrothersEntitites);
                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = @"Unknown Customer ID updated from " + unknownCustLog.OldCustomerID + " to " + unknownCustLog.NewCustomerID + " from MARS by " + UserName,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, 
                    UserName = UserName,
                    isDispatchNotes = 1
                };
                wkrorder.NotesHistories.Add(notesHistory);

                WorkorderController wc = new WorkorderController();
                wc.UpdateWOModifiedElapsedTime(Convert.ToInt32(workOrderId));
                
                FarmerBrothersEntitites.SaveChanges();


            }

            using (FarmerBrothersEntities FarmerBrothersEntitites1 = new FarmerBrothersEntities())
            {
                Contact contact1 = FarmerBrothersEntitites1.Contacts.Where(c => c.ContactID == unknownCustomerId).FirstOrDefault();
                WorkOrder wo = FarmerBrothersEntitites1.WorkOrders.Where(c => c.CustomerID == unknownCustomerId).FirstOrDefault();
                if (contact1!=null && wo ==null)
                {
                    contact1.SearchType = "CI";
                    FarmerBrothersEntitites1.Contacts.Add(contact1);
                    FarmerBrothersEntitites1.Entry(contact1).State = System.Data.Entity.EntityState.Modified;
                    FarmerBrothersEntitites1.SaveChanges();
                }
                
            }
              
            
            WorkorderSearchModel unknownCustomers = new WorkorderSearchModel();
            unknownCustomers.SearchResults = GetUnKnownWorkOrders();
            TempData["notice"] = "Data saved successfully";
            return View("UnknownCustomer", unknownCustomers);
        }

        public ActionResult createProject(int? customerId, int? workOrderId)
        {
            return View("CreateProject");
        }

        public JsonResult IsCustomerExist(string custID)
        {
            int custId = Convert.ToInt32(custID);
            bool isCustomerExist = false;

            Contact cust = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == custId && x.IsUnknownUser != 1).FirstOrDefault();
            if (cust != null)
            {
                isCustomerExist = true;
            }
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = isCustomerExist };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpPost]
        public void ExcelExport()
        {
            CustomerSearchModel customerSearchModel = new CustomerSearchModel();
            IList<Contact> customers = new List<Contact>();
            List <WorkorderSearchResultModel> unknownCustomers = GetUnKnownWorkOrders();
            
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, unknownCustomers, "UnKnownCustomers.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

    }
}

