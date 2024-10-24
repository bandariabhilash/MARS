using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class NonServiceEventController : BaseController
    {
        // GET: NonServiceEvent
        #region Construct Non Service Event
        public ActionResult NonServiceEventCall(int id)
        {
            DateTime currentTime = DateTime.Now;
            NonServiceEventModel nse = new NonServiceEventModel();
            List<FBCallReason> fbcallreasonlist = FarmerBrothersEntitites.FBCallReasons.Where(c => DbFunctions.TruncateTime(c.DateActive) < DbFunctions.TruncateTime(currentTime)
                                                                                                   && DbFunctions.TruncateTime(c.DateInactive) > DbFunctions.TruncateTime(currentTime)).ToList();
            FBCallReason fbReason = new FBCallReason()
            {
                SourceCode = "-1",
                Description = "Please Select Call Reason"
            };
            fbcallreasonlist.Insert(0, fbReason);
            nse.FBCallReasons = fbcallreasonlist;


            CustomerModel customerModel = new CustomerModel();

            IList<Contact> customers = new List<Contact>();

            customers = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == id).ToList();

            if (customers != null)
            {
                if (customers.Count > 0)
                {
                    customerModel = new CustomerModel(customers[0], FarmerBrothersEntitites);
                    customerModel = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, customerModel);
                    customerModel.PhoneNumber = Utility.FormatPhoneNumber(customers[0].PhoneWithAreaCode);
                }
            }
            customerModel.States = Utility.GetStates(FarmerBrothersEntitites);

            nse.Customer = customerModel;

            NotesModel notes = new NotesModel();
            notes.NotesHistory = new List<NotesHistoryModel>();
            notes.RecordHistory = new List<NotesHistoryModel>();
            notes.CustomerNotesResults = new List<CustomerNotesModel>();

            notes.CustomerZipCode = nse.Customer.ZipCode;
            nse.Notes = notes;
            return View(nse);
        }

        [HttpGet]
        public ActionResult NonServiceEventCall(int? customerId, int? workOrderId)
        {
            NonServiceEventModel nonServiceEventModel = ConstructNonServiceEventModel(customerId, workOrderId);
            return View(nonServiceEventModel);
        }

        public NonServiceEventModel ConstructNonServiceEventModel(int? customerId, int? workOrderId)
        {
            DateTime currentTime = DateTime.Now;
            NonServiceEventModel nse = new NonServiceEventModel();          
            
            nse.callReason = FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == workOrderId).Select(w => w.CallReason).FirstOrDefault();
            //nse.FBCallReasons = FarmerBrothersEntitites.FBCallReasons.Where(c => c.SalesCall !=0
            //                                                                                       && DbFunctions.TruncateTime(c.DateActive) < DbFunctions.TruncateTime(currentTime)
            //                                                                                       && DbFunctions.TruncateTime(c.DateInactive) > DbFunctions.TruncateTime(currentTime)
            //                                                                                       ).OrderBy(o=>o.OrderBy).ToList();

            List<FBCallReason> fbcList = FarmerBrothersEntitites.FBCallReasons.Where(c => c.SalesCall != 0
                                && DbFunctions.TruncateTime(c.DateActive) < DbFunctions.TruncateTime(currentTime)
                                && DbFunctions.TruncateTime(c.DateInactive) > DbFunctions.TruncateTime(currentTime)
                                ).OrderBy(o => o.OrderBy).ToList();

            nse.FBCallReasons = new List<FBCallReason>();
            foreach (FBCallReason fbc in fbcList)
            {
                fbc.Description = fbc.SourceCode + " - " + fbc.Description;
                nse.FBCallReasons.Add(fbc);
            }

            FBCallReason fbReason = new FBCallReason()
            {
                SourceCode = "-1",
                Description = "Please Select Call Reason"
            };
            nse.FBCallReasons.Insert(0, fbReason);
            
            
            CustomerModel customerModel = new CustomerModel();

            IList<Contact> customers = new List<Contact>();

            customers = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == customerId).ToList();

            if (customers != null)
            {
                if (customers.Count > 0)
                {
                    customerModel = new CustomerModel(customers[0], FarmerBrothersEntitites);                    
                    customerModel.PhoneNumber = Utility.FormatPhoneNumber(customers[0].PhoneWithAreaCode);
                    customerModel.WorkOrderId = "0";
                }
            }
            customerModel.States = Utility.GetStates(FarmerBrothersEntitites);

            nse.Customer = customerModel;
            nse.Notes = new NotesModel()
            {
                CustomerZipCode = nse.Customer.ZipCode,
            };

            nse.Notes.NotesHistory = new List<NotesHistoryModel>();
            nse.Notes.RecordHistory = new List<NotesHistoryModel>();
            nse.Notes.CustomerNotesResults = new List<CustomerNotesModel>();
            nse.Notes.viewProp = "NonServiceView";          
            nse.Notes.IsAutoDispatched = Convert.ToBoolean(FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == workOrderId).Select(w => w.IsAutoDispatched).FirstOrDefault());
            if (string.IsNullOrEmpty(nse.Customer.MainContactName) || (nse.Customer.MainContactName!= null && string.IsNullOrEmpty(nse.Customer.MainContactName.Trim())))
            {
                nse.Customer.MainContactName = FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == workOrderId).Select(w => w.MainContactName).FirstOrDefault();
            }
            if (string.IsNullOrEmpty(nse.Customer.PhoneNumber) || (nse.Customer.PhoneNumber != null && string.IsNullOrEmpty(nse.Customer.PhoneNumber.Trim())))              
            {
                nse.Customer.PhoneNumber = FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == workOrderId).Select(w => w.PhoneNumber).FirstOrDefault();
            }

            if (workOrderId.HasValue)
            {
                nse.WorkOrderID = Convert.ToInt32(workOrderId);
                NonServiceworkorder nswo = FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == workOrderId).FirstOrDefault();

                int userid = Convert.ToInt32(nswo.CreatedBy);
                nse.CreatedBy = FarmerBrothersEntitites.FbUserMasters.Where(u => u.UserId == userid).Select(n => n.FirstName).FirstOrDefault();

                nse.CreatedDate = nswo.CreatedDate;
                nse.CallerName = nswo.CallerName;
                nse.CallBack = Utility.FormatPhoneNumber(nswo.CallBack);
                nse.CloseDate = nswo.CloseDate;
                nse.Status = nswo.NonServiceEventStatus;
                nse.CloseCall = nswo.NonServiceEventStatus?.ToLower() == "closed" ? true : false;
                nse.ResolutionCallerName = nswo.ResolutionCallerName;

                int closedUserid = nswo.ClosedBy.HasValue ? Convert.ToInt32(nswo.ClosedBy) : 0;
                nse.ClosedBy = FarmerBrothersEntitites.FbUserMasters.Where(u => u.UserId == closedUserid).Select(n => n.FirstName).FirstOrDefault();

                nse.Customer.TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, customerId.ToString());
                nse.Customer.NonFBCustomerList = Utility.GetNonFBCustomers(FarmerBrothersEntitites, false);

                Contact serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == (int)customerId).FirstOrDefault();
                if (serviceCustomer != null)
                {
                    if (!string.IsNullOrEmpty(serviceCustomer.BillingCode))
                    {
                        nse.Customer.IsBillable = CustomerModel.IsBillableService(serviceCustomer.BillingCode, nse.Customer.TotalCallsCount);
                        nse.Customer.ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, serviceCustomer.BillingCode);
                    }
                    else
                    {
                        nse.Customer.IsBillable = " ";
                        nse.Customer.ServiceLevelDesc = " - ";
                    }
                }


                IQueryable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.NonServiceWorkorderID == workOrderId).OrderByDescending(nh => nh.EntryDate);
                foreach (NotesHistory notesHistory in notesHistories)
                {
                    nse.Notes.NotesHistory.Add(new NotesHistoryModel(notesHistory));
                }

                //IQueryable<NotesHistory> recordHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.NonServiceWorkorderID == workOrderId && nh.AutomaticNotes == 1).OrderByDescending(nh => nh.EntryDate);
                //foreach (NotesHistory recordHistory in recordHistories)
                //{
                //    nse.Notes.RecordHistory.Add(new NotesHistoryModel(recordHistory));
                //}


                //To display the Inactive Codes if any for the old events
                if (!string.IsNullOrEmpty(nswo.CallReason))
                {
                    FBCallReason inavtiveFBReason = FarmerBrothersEntitites.FBCallReasons.Where(c => c.SourceCode == nswo.CallReason).FirstOrDefault();

                    FBCallReason existingCallReason = nse.FBCallReasons.Where(w => w.SourceCode == nswo.CallReason).FirstOrDefault();
                    if (existingCallReason == null)
                    {
                        inavtiveFBReason.Description = inavtiveFBReason.SourceCode + " - " + inavtiveFBReason.Description;
                        nse.FBCallReasons.Add(inavtiveFBReason);
                    }
                }

            }
            else
            {
                nse.Customer.NonFBCustomerList = Utility.GetNonFBCustomers(FarmerBrothersEntitites, true);
            }

            nse.Notes.CustomerNotesResults = new List<CustomerNotesModel>();
            //int? custId = Convert.ToInt32(nse.Customer.CustomerId);
            //var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();
            int custId = Convert.ToInt32(nse.Customer.CustomerId);
            int parentId = string.IsNullOrEmpty(nse.Customer.ParentNumber) ? 0 : Convert.ToInt32(nse.Customer.ParentNumber);
            var custNotes = Utility.GetCustomreNotes(custId, parentId, FarmerBrothersEntitites);
            foreach (var dbCustNotes in custNotes)
            {
                nse.Notes.CustomerNotesResults.Add(new CustomerNotesModel(dbCustNotes));
            }

           
            return nse;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "NonServiceEventSave")]
        public JsonResult SaveNonServiceEvent([ModelBinder(typeof(NonServiceEventModelBinder))]NonServiceEventModel NonService, CustomerModel customerdata)
        {
            int returnValue = 0;
            NonServiceworkorder workOrder = null;
            string message = string.Empty;
            bool isValid = true;
            //if (IsNotesRequired(NonService.callReason))
            {
                if (NonService.NewNotes.Count <= 0)
                {
                    message = @"|Notes required to save Customer Service Work Order!";
                    returnValue = -1;
                    isValid = false;
                }
                else if (NonService.NewNotes.Count > 0)
                {
                    NewNotesModel newNotes = NonService.NewNotes.ElementAt(0);
                    if (string.IsNullOrWhiteSpace(newNotes.Text))
                    {
                        message = @"|Notes can not be blank!";
                        returnValue = -1;
                        isValid = false;
                    }
                }
            }
           
            if (NonService.callReason == "-1")
            {
                message = @"|Please select Call Reason!";
                returnValue = -1;
                isValid = false;

            }
            if (string.IsNullOrEmpty(NonService.Customer.ZipCode))
            {
                message = @"|Please Enter Valid Customer ZipCode and Update Customer details!";
                returnValue = -1;
                isValid = false;
            }

            if (string.IsNullOrEmpty(NonService.Customer.CustomerName))
            {
                message = @"|Please Enter Customer Name!";
                returnValue = -1;
                isValid = false;
            }

            //if (string.IsNullOrEmpty(NonService.Customer.MainContactName) || (NonService.Customer.MainContactName != null && string.IsNullOrEmpty(NonService.Customer.MainContactName.Trim())))
            if (string.IsNullOrEmpty(NonService.CallerName) || (NonService.CallerName != null && string.IsNullOrEmpty(NonService.CallerName.Trim())))
            {
                message = @"|Please Enter Main Caller Name!";
                returnValue = -1;
                isValid = false;
            }

            //if (string.IsNullOrEmpty(NonService.Customer.PhoneNumber) || (NonService.Customer.PhoneNumber != null && string.IsNullOrEmpty(NonService.Customer.PhoneNumber.Trim())))
            if (string.IsNullOrEmpty(NonService.CallBack) || (NonService.CallBack != null && string.IsNullOrEmpty(NonService.CallBack.Trim())))              
            {
                message = @"|Please Enter CallBack Number!";
                returnValue = -1;
                isValid = false;
            }

            if (!string.IsNullOrEmpty(NonService.Customer.ParentNumber))
            {
                if(NonService.Customer.ParentNumber.Length > 8)
                {
                    message = @"|Please Enter Valid Parent Number!";
                    returnValue = -1;
                    isValid = false;
                }
            }

            if (NonService.CloseCall)
            {
                if (string.IsNullOrEmpty(NonService.ResolutionCallerName))
                {
                    message = @"|Please Enter Resolution Caller name!";
                    returnValue = -1;
                    isValid = false;
                }
            }

            bool isNewEvent = true;
            if(NonService.WorkOrderID > 0)
            {
                isNewEvent = false;
            }

            if (isValid == true)
            {
                returnValue = NonServiceWorkOrderSave(NonService, customerdata, out workOrder, out message);
            }

            if (returnValue == -1)
            {
                string callStatus = workOrder == null ? "" : "";
                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = "", WorkOrderId = 0, returnValue = returnValue, WorkorderCallstatus = callStatus, message = message };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            else
            {
                if (returnValue >= 0)
                {
                    if (isNewEvent)
                    {
                        SendMail(NonService.WorkOrderID, FarmerBrothersEntitites);
                    }

                    var redirectUrl = string.Empty;
                    if (Request != null)
                    {
                        redirectUrl = new UrlHelper(Request.RequestContext).Action("CustomerSearch", "CustomerSearch", new { isBack = 0 });
                    }

                    JsonResult jsonResult = new JsonResult();
                    jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = redirectUrl, WorkOrderId = workOrder.WorkOrderID, returnValue = returnValue, WorkorderCallstatus = "", message = message };
                    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                    return jsonResult;
                }
                else
                {
                    string callStatus = workOrder == null ? "" : "";
                    JsonResult jsonResult = new JsonResult();
                    jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = "", WorkOrderId = 0, returnValue = returnValue, WorkorderCallstatus = callStatus, message = message };
                    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                    return jsonResult;
                }
            }
            
        }

        #endregion

        public int NonServiceWorkOrderSave(NonServiceEventModel NonService, CustomerModel customerdata, out NonServiceworkorder workOrder, out string message)
        {
            int returnValue = 0;
            message = string.Empty;
            workOrder = new NonServiceworkorder();

           
            if (NonService.Operation == WorkOrderManagementSubmitType.SAVE)
            {
                try
                {
                    if (!string.IsNullOrEmpty(customerdata.NonFBCustomerNumber) && customerdata.NonFBCustomerNumber.ToUpper() != "N/A")
                    {
                        NonFBCustomer nonfbCust = customerdata.NonFBCustomerList.Where(c => c.NonFBCustomerId == customerdata.NonFBCustomerNumber).FirstOrDefault();
                        if (nonfbCust != null)
                        {
                            customerdata.IsNonFBCustomer = true;
                        }
                        else
                        {
                            customerdata.IsNonFBCustomer = false;
                        }
                    }
                    else
                    {
                        customerdata.IsNonFBCustomer = false;
                    }


                    NonService.Customer = customerdata;
                    NonServiceworkorder nsw = new NonServiceworkorder();
                    
                    DateTime currentTime = Utility.GetCurrentTime(NonService.Customer.ZipCode, FarmerBrothersEntitites);
                    if (NonService.Customer != null)
                    {
                        nsw.CustomerID = Convert.ToInt32(NonService.Customer.CustomerId);
                        nsw.CustomerCity = NonService.Customer.City;
                        nsw.CustomerState = NonService.Customer.State;
                        nsw.CustomerZipCode = NonService.Customer.ZipCode;                        
                    }
                   
                    if (NonService.WorkOrderID==0)
                    {

                        if (!nsw.CustomerID.HasValue || nsw.CustomerID.Value <= 0)
                        {
                           
                            CustomerModel custmodel = new CustomerModel();
                            custmodel.CreateUnknownCustomer(customerdata, FarmerBrothersEntitites);
                            nsw.CustomerID = Convert.ToInt32(customerdata.CustomerId);
                            nsw.IsUnknownWorkOrder = true;
                        }

                        nsw.CreatedDate = currentTime;
                        IndexCounter counter = Utility.GetIndexCounter("NonServiceWorkOrderID", 1);
                        counter.IndexValue++;

                        nsw.WorkOrderID = counter.IndexValue.Value;
                        nsw.CallReason = NonService.callReason;
                        nsw.CreatedBy = (int)System.Web.HttpContext.Current.Session["UserId"];
                        nsw.IsAutoDispatched = NonService.Notes.IsAutoDispatched;
                        nsw.CallerName = NonService.CallerName;
                        nsw.CallBack = NonService.CallBack;
                        nsw.NonServiceEventStatus = "Open";
                        nsw.MainContactName = NonService.Customer.MainContactName;
                        nsw.PhoneNumber = NonService.Customer.PhoneNumber;

                        if (NonService.CloseCall)
                        {
                            nsw.NonServiceEventStatus = "Closed";
                            nsw.CloseDate = currentTime;
                            nsw.ClosedBy = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234;
                            nsw.ResolutionCallerName = NonService.ResolutionCallerName;

                            
                            NotesHistory closurenotesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 0,
                                EntryDate = currentTime,
                                Notes = "Closure Notes : " + NonService.ClosureNotes,
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                                NonServiceWorkorderID = counter.IndexValue.Value
                            };
                            FarmerBrothersEntitites.NotesHistories.Add(closurenotesHistory);

                            NotesHistory newnotesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = "Customer Service Event Closed Successfully",
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                                NonServiceWorkorderID = counter.IndexValue.Value
                            };
                            FarmerBrothersEntitites.NotesHistories.Add(newnotesHistory);
                        }

                        FarmerBrothersEntitites.NonServiceworkorders.Add(nsw);
                        workOrder = nsw;
                        NonService.WorkOrderID = nsw.WorkOrderID;
                        
                        NotesHistory notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 1,
                            EntryDate = currentTime,
                            Notes = @"Created Customer Service WO#: " + workOrder.WorkOrderID + @" in “MARS”!",
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, 
                            UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                            isDispatchNotes = 0
                        };
                        notesHistory.NonServiceWorkorderID = workOrder.WorkOrderID;
                        workOrder.NotesHistories.Add(notesHistory);
                    }
                    else
                    {
                        workOrder.WorkOrderID = NonService.WorkOrderID;
                        workOrder.CustomerID = Convert.ToInt32(NonService.Customer.CustomerId);

                        NonServiceworkorder nswsave = FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == NonService.WorkOrderID).FirstOrDefault();
                        nswsave.CallReason = NonService.callReason;
                        nswsave.WorkOrderID = NonService.WorkOrderID;
                        nswsave.IsAutoDispatched = NonService.Notes.IsAutoDispatched;
                        nswsave.CallerName = NonService.CallerName;
                        nswsave.CallBack = NonService.CallBack;

                        if (NonService.CloseCall)
                        {
                            nswsave.NonServiceEventStatus = "Closed";
                            nswsave.CloseDate = currentTime;
                            nswsave.ClosedBy = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234;
                            nswsave.ResolutionCallerName = NonService.ResolutionCallerName;

                            NotesHistory newnotesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = "Customer Service Event Closed Successfully",
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                                NonServiceWorkorderID = NonService.WorkOrderID
                            };
                            FarmerBrothersEntitites.NotesHistories.Add(newnotesHistory);
                        }

                        FarmerBrothersEntitites.SaveChanges();
                    }
                    
                   


                    foreach (NewNotesModel newNotesModel in NonService.NewNotes)
                    {
                        NotesHistory newnotesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = NonService.CloseCall ? ("Closure Notes : " + newNotesModel.Text) : newNotesModel.Text,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, 
                            UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                            NonServiceWorkorderID = workOrder.WorkOrderID
                        };
                        FarmerBrothersEntitites.NotesHistories.Add(newnotesHistory);

                    }

                    returnValue = FarmerBrothersEntitites.SaveChanges();
                }
                catch (Exception ex)
                {
                    message = "Unable to Create Customer Service Work Order!";
                    returnValue = -1;
                }

            }
            return returnValue;
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "UpdateCustomer")]
        public JsonResult UpdateCustomer([ModelBinder(typeof(CustomerModelBinder))]CustomerModel customer)
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
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("NonServiceEventCall", "NonServiceEvent", new { id = customer.CustomerId });
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

        /*public bool SendMail(NonServiceEventModel NonService, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            NonServiceworkorder nswo = FarmerBrothersEntitites.NonServiceworkorders.Where(nwo => nwo.WorkOrderID == NonService.WorkOrderID).FirstOrDefault();
            DateTime currentTime = Utility.GetCurrentTime(NonService.Customer.ZipCode, FarmerBrothersEntitites);

            string completeUrl = ConfigurationManager.AppSettings["CompleteNonServiceEventUrl"];
            string createServiceEventUrl = ConfigurationManager.AppSettings["CreateServiceEventUrl"];
            StringBuilder salesEmailBody = new StringBuilder();

            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", completeUrl, new Encrypt_Decrypt().Encrypt("workOrderId=" + NonService.WorkOrderID)) + "\">COMPLETE</a>");
            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", createServiceEventUrl, new Encrypt_Decrypt().Encrypt("workOrderId=" + NonService.WorkOrderID)) + "\">CREATE SERVICE EVENT</a>");
            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            #region customer details

            salesEmailBody.Append("<table>");

            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("CustomerDetails:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("</tr>");

            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("</tr>");

            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("AccountNumber:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("<td>");
            salesEmailBody.Append(NonService.Customer.CustomerId);
            salesEmailBody.Append("</td>");
            salesEmailBody.Append("</tr>");

            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("CustomerName:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("<td>");
            salesEmailBody.Append(NonService.Customer.CustomerName);
            salesEmailBody.Append("</td>");
            salesEmailBody.Append("</tr>");
            

            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("Address1:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("<td>");
            salesEmailBody.Append(NonService.Customer.Address);
            salesEmailBody.Append("</td>");
            salesEmailBody.Append("</tr>");

            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("Address2:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("<td>");
            salesEmailBody.Append(NonService.Customer.Address2);
            salesEmailBody.Append("</td>");
            salesEmailBody.Append("</tr>");


            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("City:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("<td>");
            salesEmailBody.Append(NonService.Customer.City);
            salesEmailBody.Append("</td>");
            salesEmailBody.Append("</tr>");


            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("State:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("<td>");
            salesEmailBody.Append(NonService.Customer.State);
            salesEmailBody.Append("</td>");
            salesEmailBody.Append("</tr>");

            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("Postal Code:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("<td>");
            salesEmailBody.Append(NonService.Customer.ZipCode);
            salesEmailBody.Append("</td>");
            salesEmailBody.Append("</tr>");

            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("Phone:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("<td>");
            salesEmailBody.Append(NonService.Customer.PhoneNumber);
            salesEmailBody.Append("</td>");
            salesEmailBody.Append("</tr>");

            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("Main Email Address:");
            salesEmailBody.Append("</b></td>");
            salesEmailBody.Append("<td>");
            salesEmailBody.Append(NonService.Customer.MainEmailAddress);
            salesEmailBody.Append("</td>");
            salesEmailBody.Append("</tr>");
                             
            salesEmailBody.Append("</table>");

            #endregion

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            string CallReasonDesc = FarmerBrothersEntitites.FBCallReasons.Where(c => c.SourceCode == NonService.callReason).Select(r => r.Description).FirstOrDefault();

            salesEmailBody.Append("<b>Call Reason: </b>");
            salesEmailBody.Append(CallReasonDesc);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<b>Caller Name: </b>");
            salesEmailBody.Append(NonService.CallerName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<b>Call Back #: </b>");
            salesEmailBody.Append(Utility.FormatPhoneNumber(NonService.CallBack));

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
          

            #region Notes

            salesEmailBody.Append("<table>");
            salesEmailBody.Append("<tr>");
            salesEmailBody.Append("<td><b>");
            salesEmailBody.Append("Notes");
            salesEmailBody.Append("<b></td>");


            IEnumerable<NotesHistory> histories = FarmerBrothersEntitites.NotesHistories.Where(w => w.NonServiceWorkorderID == NonService.WorkOrderID).OrderByDescending(n => n.EntryDate);

            foreach (NotesHistory history in histories)
            {
                salesEmailBody.Append("<tr>");
                salesEmailBody.Append("<td>");
                salesEmailBody.Append(history.UserName);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.EntryDate);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.Notes);
                salesEmailBody.Append("</td>");
                salesEmailBody.Append("</tr>");   
            }

            salesEmailBody.Append("</tr>");
            salesEmailBody.Append("</table>");


            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", completeUrl, new Encrypt_Decrypt().Encrypt("workOrderId=" + NonService.WorkOrderID)) + "\">COMPLETE</a>");
            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", createServiceEventUrl, new Encrypt_Decrypt().Encrypt("workOrderId=" + NonService.WorkOrderID)) + "\">CREATE SERVICE EVENT</a>");
            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            string contentId = Guid.NewGuid().ToString();
            string logoPath  = Path.Combine(HttpRuntime.AppDomainAppPath, "img/mainlogo.jpg");
           
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

            #endregion

            bool result = true;

            #region Mail

            
            string fromAddress = ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"];
            FBCallReason callReason = FarmerBrothersEntitites.FBCallReasons.Where(c => c.SourceCode == NonService.callReason).FirstOrDefault();

            int cid = Convert.ToInt32(NonService.Customer.CustomerId);
            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == cid).FirstOrDefault();
            string ESMMails = string.Empty;
            string CCMMails = string.Empty;
           
            if (!string.IsNullOrEmpty(customer.ESMEmail))
            {
                ESMMails = customer.ESMEmail;
            }

            if (!string.IsNullOrEmpty(customer.CCMEmail)) {
                CCMMails = customer.CCMEmail;
            }

            string mailTo = string.Empty;
            string ccTo = string.Empty;
            string mailToName = string.Empty;

            IDictionary<string, string> mailToUserIds = new Dictionary<string, string>();            
            
            FBCustomerServiceDistribution csd = FarmerBrothersEntitites.FBCustomerServiceDistributions.Where(cs => cs.Route == customer.Route).FirstOrDefault();
            string NotesMsg = "";
           if ((nswo != null && nswo.IsUnknownWorkOrder == true) || NonService.Customer.CustomerId.StartsWith("1000")
                && callReason!= null && callReason.Description.ToLower() != "dropped call")
            {
                mailTo += ConfigurationManager.AppSettings["CallReasonUnknowunCustomerEmail"].ToString();
                NotesMsg += "Customer Service Escalation Mail sent to " + mailTo;
                
                string[] emailIds = mailTo.Split(';');

                if(!mailToUserIds.ContainsKey(emailIds[0])) mailToUserIds.Add(new KeyValuePair<string, string>(emailIds[0], "CustomerService"));
                if (!mailToUserIds.ContainsKey(emailIds[1])) mailToUserIds.Add(new KeyValuePair<string, string>(emailIds[1], "Mike"));

                mailToName += ConfigurationManager.AppSettings["CallReasonUnknowunCustomerEmail"].ToString();
            }
            else
            {
                if (callReason != null && csd != null)
                {
                    if (callReason.EmailRegional == true && !string.IsNullOrEmpty(csd.RegionalsEmail))
                    {
                        mailTo += csd.RegionalsEmail;
                        mailTo += ";";

                        if (!mailToUserIds.ContainsKey(csd.RegionalsEmail)) mailToUserIds.Add(new KeyValuePair<string, string>(csd.RegionalsEmail, csd.RegionalsName));
                        mailToName += "Regionals => " + csd.RegionalsName + " - " + csd.RegionalsEmail + ";    ";
                    }
                    if (callReason.EmailSalesManager == true && !string.IsNullOrEmpty(csd.SalesMmanagerEmail))
                    {
                        mailTo += csd.SalesMmanagerEmail;
                        mailTo += ";";

                        if (!mailToUserIds.ContainsKey(csd.SalesMmanagerEmail)) mailToUserIds.Add(new KeyValuePair<string, string>(csd.SalesMmanagerEmail, csd.SalesManagerName));
                        mailToName += "SalesManager => " + csd.SalesManagerName + " - " + csd.SalesMmanagerEmail + ";    ";
                    }
                    if (callReason.EmailRSR == true && !string.IsNullOrEmpty(csd.RSREmail))
                    {
                        mailTo += csd.RSREmail;
                        mailTo += ";";

                        if (!mailToUserIds.ContainsKey(csd.RSREmail)) mailToUserIds.Add(new KeyValuePair<string, string>(csd.RSREmail, csd.RSRName));
                        mailToName += "RSR => " + csd.RSRName + " - " + csd.RSREmail + ";    ";
                    }

                    NotesMsg += "Customer Service Escalation Mail sent to " + mailToName;
                }
                else
                {
                    if (callReason != null && callReason.Description.ToLower() != "dropped call")
                    {
                        mailTo += ConfigurationManager.AppSettings["MikeEmailId"].ToString();
                        mailTo += ";";
                    
                        if (!mailToUserIds.ContainsKey(ConfigurationManager.AppSettings["MikeEmailId"].ToString())) mailToUserIds.Add(new KeyValuePair<string, string>(ConfigurationManager.AppSettings["MikeEmailId"].ToString(), "Mike"));
                        mailToName += "Mike - " + ConfigurationManager.AppSettings["MikeEmailId"].ToString() + ";    ";
                    }
                    mailTo += ConfigurationManager.AppSettings["DarrylEmailId"].ToString();

                    if (!mailToUserIds.ContainsKey(ConfigurationManager.AppSettings["DarrylEmailId"].ToString())) mailToUserIds.Add(new KeyValuePair<string, string>(ConfigurationManager.AppSettings["DarrylEmailId"].ToString(), "Darryl"));
                    mailToName += "Darryl - " + ConfigurationManager.AppSettings["DarrylEmailId"].ToString() + ";    ";

                    NotesMsg += "Customer Service Escalation Mail sent to " + mailToName;
                }
            }

            if(string.IsNullOrEmpty(mailTo))
            {
                mailTo += "thelms@mktalt.onmicrosoft.com";
                mailToName += "Tim - " + "thelms@mktalt.onmicrosoft.com";
                mailTo += "ssheedy@mktalt.com";
                mailToName += "Shannon - " + "ssheedy@mktalt.com";

                NotesMsg += "Customer Service Escalation Mail sent to " + mailToName;
            }

            NotesHistory notesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = currentTime,
                Notes = NotesMsg,
                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName
            };
            notesHistory.NonServiceWorkorderID = nswo.WorkOrderID;
            nswo.NotesHistories.Add(notesHistory);
            nswo.EmailSentTo = mailToName;
            FarmerBrothersEntitites.SaveChanges();

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
            {
                mailTo = ConfigurationManager.AppSettings["TestEmail"];
            }
            
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

                message.From = new MailAddress(fromAddress);
                message.Subject = "Call Reason: " + CallReasonDesc;
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
            }

            #endregion

            return result;
        }
        */

         public JsonResult ReSendEmail(int WorkorderId)
        {
            JsonResult jsonResult = new JsonResult();
            try
            {
                bool emailSentSuccess = SendMail(WorkorderId, FarmerBrothersEntitites);

                jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = "", WorkOrderId = WorkorderId, message = "Email Resend Success!" };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            catch
            {
                jsonResult = new JsonResult();
                jsonResult.Data = new { success = false, serverError = ErrorCode.SUCCESS, Url = "", WorkOrderId = WorkorderId, message = "Email Resend Failed!" };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
        }


        public bool SendMail(int WorkOrderID, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            NonServiceworkorder nswo = FarmerBrothersEntitites.NonServiceworkorders.Where(nwo => nwo.WorkOrderID == WorkOrderID).FirstOrDefault();
            DateTime currentTime = Utility.GetCurrentTime(nswo.CustomerZipCode, FarmerBrothersEntitites);

            string fromAddress = "reviveservice@mktalt.com"; //ConfigurationManager.AppSettings["CustomerUpdateMailFromAddress"];
            FBCallReason callReason = FarmerBrothersEntitites.FBCallReasons.Where(c => c.SourceCode == nswo.CallReason).FirstOrDefault();

            int cid = Convert.ToInt32(nswo.CustomerID);
            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == cid).FirstOrDefault();
            string ESMMails = string.Empty;
            string CCMMails = string.Empty;

            if (!string.IsNullOrEmpty(customer.ESMEmail))
            {
                ESMMails = customer.ESMEmail;
            }

            if (!string.IsNullOrEmpty(customer.CCMEmail))
            {
                CCMMails = customer.CCMEmail;
            }

            string mailTo = string.Empty;
            string ccTo = string.Empty;
            string mailToName = string.Empty;

            IDictionary<string, string> mailToUserIds = new Dictionary<string, string>();

            FBCustomerServiceDistribution csd = FarmerBrothersEntitites.FBCustomerServiceDistributions.Where(cs => cs.Route == customer.Route).FirstOrDefault();
            string NotesMsg = "";
            if ((nswo != null && nswo.IsUnknownWorkOrder == true) || nswo.CustomerID.ToString().StartsWith("1000")
                && callReason!= null && callReason.Description.ToLower() != "dropped call")
            {
                mailTo += ConfigurationManager.AppSettings["CallReasonUnknowunCustomerEmail"].ToString();
                NotesMsg += "Customer Service Escalation Mail sent to " + mailTo;
                
                string[] emailIds = mailTo.Split(';');

                if(!mailToUserIds.ContainsKey(emailIds[0])) mailToUserIds.Add(new KeyValuePair<string, string>(emailIds[0], "CustomerService"));
                if (!mailToUserIds.ContainsKey(emailIds[1])) mailToUserIds.Add(new KeyValuePair<string, string>(emailIds[1], "Mike"));

                mailToName += ConfigurationManager.AppSettings["CallReasonUnknowunCustomerEmail"].ToString();
            }
            else
            {
                if (callReason != null && csd != null && callReason.Description.ToLower() != "dropped call")
                {
                    if (callReason.EmailRegional == true && !string.IsNullOrEmpty(csd.RegionalsEmail))
                    {
                        mailTo += csd.RegionalsEmail;
                        mailTo += ";";

                        if (!mailToUserIds.ContainsKey(csd.RegionalsEmail)) mailToUserIds.Add(new KeyValuePair<string, string>(csd.RegionalsEmail, csd.RegionalsName));
                        mailToName += "Regionals => " + csd.RegionalsName + " - " + csd.RegionalsEmail + ";    ";
                    }
                    if (callReason.EmailSalesManager == true && !string.IsNullOrEmpty(csd.SalesMmanagerEmail))
                    {
                        mailTo += csd.SalesMmanagerEmail;
                        mailTo += ";";

                        if (!mailToUserIds.ContainsKey(csd.SalesMmanagerEmail)) mailToUserIds.Add(new KeyValuePair<string, string>(csd.SalesMmanagerEmail, csd.SalesManagerName));
                        mailToName += "SalesManager => " + csd.SalesManagerName + " - " + csd.SalesMmanagerEmail + ";    ";
                    }
                    if (callReason.EmailRSR == true && !string.IsNullOrEmpty(csd.RSREmail))
                    {
                        mailTo += csd.RSREmail;
                        mailTo += ";";

                        if (!mailToUserIds.ContainsKey(csd.RSREmail)) mailToUserIds.Add(new KeyValuePair<string, string>(csd.RSREmail, csd.RSRName));
                        mailToName += "RSR => " + csd.RSRName + " - " + csd.RSREmail + ";    ";
                    }

                    NotesMsg += "Customer Service Escalation Mail sent to " + mailToName;
                }
                else
                {
                    if (callReason != null && callReason.Description.ToLower() != "dropped call")
                    {
                        mailTo += ConfigurationManager.AppSettings["MikeEmailId"].ToString();
                        mailTo += ";";
                    
                        if (!mailToUserIds.ContainsKey(ConfigurationManager.AppSettings["MikeEmailId"].ToString())) mailToUserIds.Add(new KeyValuePair<string, string>(ConfigurationManager.AppSettings["MikeEmailId"].ToString(), "Mike"));
                        mailToName += "Mike - " + ConfigurationManager.AppSettings["MikeEmailId"].ToString() + ";    ";
                    }
                    mailTo += ConfigurationManager.AppSettings["DarrylEmailId"].ToString();

                    if (!mailToUserIds.ContainsKey(ConfigurationManager.AppSettings["DarrylEmailId"].ToString())) mailToUserIds.Add(new KeyValuePair<string, string>(ConfigurationManager.AppSettings["DarrylEmailId"].ToString(), "Darryl"));
                    mailToName += "Darryl - " + ConfigurationManager.AppSettings["DarrylEmailId"].ToString() + ";    ";

                    NotesMsg += "Customer Service Escalation Mail sent to " + mailToName;
                }
            }

            /*Updated this block on March 28, 2023 for email subject "FB acct 9268923 and non-service 5172877 - ngatton@mktalt.com"
             * if (callReason != null && callReason.Description.ToLower() == "dropped call")
            {
                mailTo = "thelms@mktalt.onmicrosoft.com";
                mailToName = "Tim - " + "thelms@mktalt.onmicrosoft.com";

                mailTo += "ssheedy@mktalt.com";
                mailToName += "Shannon - " + "ssheedy@mktalt.com";

                NotesMsg = "Customer Service Escalation Mail sent to " + mailToName;
            }

            if (string.IsNullOrEmpty(mailTo))
            {
                mailTo += "thelms@mktalt.onmicrosoft.com;";
                mailToName += "Tim - " + "thelms@mktalt.onmicrosoft.com";
                mailTo += "ssheedy@mktalt.com;";
                mailToName += "Shannon - " + "ssheedy@mktalt.com";

                NotesMsg += "Customer Service Escalation Mail sent to " + mailToName;
            }*/

            if (callReason != null && callReason.Description.ToLower() == "dropped call")
            {
                mailTo = ConfigurationManager.AppSettings["MikeEmailId"].ToString();
                mailToName = "Mike - " + ConfigurationManager.AppSettings["MikeEmailId"].ToString() + ";    ";

                mailTo += ConfigurationManager.AppSettings["DONKite"].ToString();
                mailToName += "Don Kite - " + ConfigurationManager.AppSettings["DONKite"].ToString() + ";    ";

                mailTo += ConfigurationManager.AppSettings["Support"].ToString();
                mailToName += "Support - " + ConfigurationManager.AppSettings["Support"].ToString() + ";    ";

                NotesMsg = "Customer Service Escalation Mail sent to " + mailToName;
            }

            if (string.IsNullOrEmpty(mailTo))
            {
                mailTo += ConfigurationManager.AppSettings["MikeEmailId"].ToString();
                mailToName += "Mike - " + ConfigurationManager.AppSettings["MikeEmailId"].ToString() + ";    ";
                mailTo += ConfigurationManager.AppSettings["DONKite"].ToString();
                mailToName += "Don Kite - " + ConfigurationManager.AppSettings["DONKite"].ToString() + ";    ";
                mailTo += ConfigurationManager.AppSettings["Support"].ToString();
                mailToName += "Support - " + ConfigurationManager.AppSettings["Support"].ToString() + ";    ";

                NotesMsg += "Customer Service Escalation Mail sent to " + mailToName;
            }

            string bcc = "";
            if(callReason != null && callReason.AdditionalEmail != null)
            {
                bcc = callReason.AdditionalEmail;
            }

            NotesHistory notesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = currentTime,
                Notes = NotesMsg,
                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                UserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : UserName,
                isDispatchNotes = 1
            };
            notesHistory.NonServiceWorkorderID = nswo.WorkOrderID;
            nswo.NotesHistories.Add(notesHistory);
            nswo.EmailSentTo = mailToName;
            FarmerBrothersEntitites.SaveChanges();

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
            {
                mailTo = ConfigurationManager.AppSettings["TestEmail"];

                string[] emailIds = mailTo.Split(';');
                foreach(string em in emailIds)
                {
                    if (!string.IsNullOrEmpty(em))
                    {
                        string[] name = em.Split('@');
                        if (!mailToUserIds.ContainsKey(em)) mailToUserIds.Add(new KeyValuePair<string, string>(em, name[0]));
                    }
                }

            }

            bool result = true;
            if (!string.IsNullOrWhiteSpace(mailTo))
            {
                using (var smtp = new SmtpClient())
                {                    
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = 25;

                    string[] addresses = mailTo.Split(';');
                    foreach (string address in addresses)
                    {
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            var message = new MailMessage();
                            message.From = new MailAddress(fromAddress);
                            result = true;
                            message.To.Add(new MailAddress(address));

                            string[] BCCAddresses = bcc.Split(';');
                            foreach (string bccaddress in BCCAddresses)
                            {
                                if (bccaddress.ToLower().Contains("@jmsmucker.com")) continue;
                                if (!string.IsNullOrWhiteSpace(bccaddress))
                                {
                                    message.Bcc.Add(new MailAddress(bccaddress));
                                }
                            }

                            string replyToAddress = ConfigurationManager.AppSettings["DispatchMailReplyToAddress"].ToString();
                            string[] ReplyToAddresses = replyToAddress.Split(';');
                            foreach (string replytoadd in ReplyToAddresses)
                            {
                                if (replytoadd.ToLower().Contains("@jmsmucker.com")) continue;
                                if (!string.IsNullOrWhiteSpace(replytoadd))
                                {
                                    message.ReplyToList.Add(new MailAddress(replytoadd));
                                }
                            }

                            string completeUrl = ConfigurationManager.AppSettings["CompleteNonServiceEventUrl"];
                            string createServiceEventUrl = ConfigurationManager.AppSettings["CreateServiceEventUrl"];
                            StringBuilder salesEmailBody = new StringBuilder();

                            string mailToUserName = mailToUserIds[address];

                            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");

                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", completeUrl, new Encrypt_Decrypt().Encrypt("workOrderId=" + WorkOrderID)) + "\">COMPLETE</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            //salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", createServiceEventUrl, new Encrypt_Decrypt().Encrypt("workOrderId=" + WorkOrderID + "&techId=0&response=0&isResponsible=false&isBillable=" + mailToUserName)) + "\">CREATE SERVICE EVENT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");

                            #region customer details

                            salesEmailBody.Append("<table>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("CustomerDetails:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("AccountNumber:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(nswo.CustomerID);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("CustomerName:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.CompanyName);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");


                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Address1:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.Address1);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Address2:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.Address2);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");


                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("City:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.City);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");


                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("State:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.State);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Postal Code:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.PostalCode);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Phone:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.Phone);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Main Email Address:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.Email);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Branch:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.Branch);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Route:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(customer.Route);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("</table>");

                            #endregion

                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");

                            string CallReasonDesc = FarmerBrothersEntitites.FBCallReasons.Where(c => c.SourceCode == nswo.CallReason).Select(r => r.Description).FirstOrDefault();

                            salesEmailBody.Append("<b>Call Reason: </b>");
                            salesEmailBody.Append(callReason.SourceCode + " - " + CallReasonDesc);
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<b>Caller Name: </b>");
                            salesEmailBody.Append(nswo.CallerName);
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<b>Call Back #: </b>");
                            salesEmailBody.Append(Utility.FormatPhoneNumber(nswo.CallBack));

                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");


                            #region Notes

                            salesEmailBody.Append("<table>");
                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Notes");
                            salesEmailBody.Append("<b></td>");


                            IEnumerable<NotesHistory> histories = FarmerBrothersEntitites.NotesHistories.Where(w => w.NonServiceWorkorderID == WorkOrderID).OrderByDescending(n => n.EntryDate);

                            foreach (NotesHistory history in histories)
                            {
                                salesEmailBody.Append("<tr>");
                                salesEmailBody.Append("<td>");
                                salesEmailBody.Append(history.UserName);
                                salesEmailBody.Append(" ");
                                salesEmailBody.Append(history.EntryDate);
                                salesEmailBody.Append(" ");
                                salesEmailBody.Append(history.Notes);
                                salesEmailBody.Append("</td>");
                                salesEmailBody.Append("</tr>");
                            }

                            salesEmailBody.Append("</tr>");
                            salesEmailBody.Append("</table>");


                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");

                            salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", completeUrl, new Encrypt_Decrypt().Encrypt("workOrderId=" + WorkOrderID)) + "\">COMPLETE</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            //salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", createServiceEventUrl, new Encrypt_Decrypt().Encrypt("workOrderId=" + WorkOrderID + "&techId=0&response=0&isResponsible=false&isBillable=" + mailToUserName)) + "\">CREATE SERVICE EVENT</a>");
                            salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");

                            string contentId = Guid.NewGuid().ToString();
                            string logoPath = Path.Combine(HttpRuntime.AppDomainAppPath, "img/mainlogo.jpg");

                            salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

                            AlternateView avHtml = AlternateView.CreateAlternateViewFromString
                              (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

                            LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
                            inline.ContentId = contentId;
                            avHtml.LinkedResources.Add(inline);



                            message.AlternateViews.Add(avHtml);

                            message.IsBodyHtml = true;
                            message.Body = salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();

                            #endregion



                            #region Mail
                            //Original Format
                            /*if (!string.IsNullOrWhiteSpace(mailTo))
                            {
                                string[] addresses = mailTo.Split(';');
                                foreach (string address in addresses)
                                {
                                    if (!string.IsNullOrWhiteSpace(address))
                                    {
                                        message.To.Add(new MailAddress(address));
                                    }
                                }

                                message.From = new MailAddress(fromAddress);
                                message.Subject = "Call Reason: " + CallReasonDesc;
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
                            }*/


                            message.Subject = "Call Reason: " + callReason.SourceCode + " - " + CallReasonDesc + ", Customer Service Event#: "+ WorkOrderID;
                            message.IsBodyHtml = true;

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
                }
            }

            #endregion

            return result;
        }

        public bool IsNotesRequired(string callReason)
        {
            bool notesRequired = false;

            switch (callReason)
            {
                case "9002":
                case "9003":
                case "9004":
                case "9005":
                case "9007":
                case "9008":
                case "9009":
                case "9012":
                case "9003A":
                case "9009A":
                case "9015":
                case "9016":
                case "9017":
                case "9024":
                case "9025":
                case "9026":
                case "9027":
                case "9028":
                case "9029":
                case "9030":
                case "9031":
                    notesRequired = true;
                    break;
                default:
                    break;
            }
            return notesRequired;
        }

    }
}