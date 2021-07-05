using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Syncfusion.JavaScript;
using System.Web.Script.Serialization;
using System.Data.Entity.Validation;

namespace FarmerBrothers.Controllers
{
    public class AutoCallGenerateController : Controller
    {
        //
        // GET: /AutoCallGenerate/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IsCustomerExist(int customerId)
        {
            string message = string.Empty;
            string redirectUrl = string.Empty;
            if (IsValidCustomer(customerId))
            {

            }
            else
            {
                message = "Customer Account Number is not valid, Please Enter Valid Account Number!";
            }
            redirectUrl = new UrlHelper(Request.RequestContext).Action("WorkOrder", "AutoCallGenerate", new { customerId = customerId });
            
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { Url = redirectUrl, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public bool IsValidCustomer(int customerId)
        {
            bool isExist = false;
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                var customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == customerId).FirstOrDefault();

                if (customer != null)
                {
                    isExist = true;
                }
            }           
            

            return isExist;
        }

        public ActionResult WorkOrder(int customerId)
        {
            AutoGenerateWorkorderModel autowoModel = new AutoGenerateWorkorderModel();
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                List<WorkorderType> WorkorderTypes = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.Active == 1 && wt.CallTypeID != 1800 && wt.CallTypeID != 1810 && wt.CallTypeID != 1820
                && wt.CallTypeID != 1300 && wt.CallTypeID != 1310 && wt.CallTypeID != 1600 && wt.CallTypeID != 1910).OrderBy(wt => wt.Sequence).ToList();

                WorkorderType woType = new WorkorderType()
                {
                    CallTypeID = -1,
                    Description = "Please Select Call Reason"
                };
                WorkorderTypes.Insert(0, woType);
                autowoModel.WorkorderTypes = WorkorderTypes;

                CustomerModel customerModel = new CustomerModel();

                IList<Contact> customers = new List<Contact>();

                customers = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == customerId).ToList();

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

                autowoModel.Customer = customerModel;

                NotesModel notes = new NotesModel();
                notes.NotesHistory = new List<NotesHistoryModel>();
                notes.RecordHistory = new List<NotesHistoryModel>();
                notes.CustomerNotesResults = new List<CustomerNotesModel>();

                notes.CustomerZipCode = autowoModel.Customer.ZipCode;
                autowoModel.Notes = notes;
                autowoModel.UserName = "WEB";
                autowoModel.Notes.isFromAutoGenerateWorkOrder = true;
            }
                       
            return View(autowoModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "WorkorderSave")]
        public JsonResult SaveWorkOrder([ModelBinder(typeof(AutoCallGenerateModelBinder))] AutoGenerateWorkorderModel workorderManagement)
        {
            var redirectUrl = string.Empty;
            var message = string.Empty;
            JsonResult jsonResult = new JsonResult();
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                Contact customer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == workorderManagement.CustomerID).FirstOrDefault();
                TimeZoneInfo newTimeZoneInfo = null;
                Utility.GetCustomerTimeZone(customer.PostalCode, FarmerBrothersEntitites);
                DateTime CurrentTime;
                DateTime.TryParse(Utility.GetCurrentTime(customer.PostalCode, FarmerBrothersEntitites).ToString("hh:mm tt"), out CurrentTime);
                

                #region save notes

                if (workorderManagement.NewNotes != null)
                {

                    foreach (NewNotesModel newNotesModel in workorderManagement.NewNotes)
                    {
                        NotesHistory notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = CurrentTime,
                            Notes = newNotesModel.Text,
                            Userid = 99999,
                            UserName = "WEB"

                        };
                        FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                    }
                }
                #endregion

                #region create work order

                try
                {

                    WorkorderManagementModel workorderModel = new WorkorderManagementModel();

                    CustomerModel customerModel = new CustomerModel();
                    workorderModel.Closure = new WorkOrderClosureModel();
                    if (customer != null)
                    {
                        customerModel = new CustomerModel(customer, FarmerBrothersEntitites);
                        customerModel = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, customerModel);
                        customerModel.PhoneNumber = Utility.FormatPhoneNumber(customer.PhoneWithAreaCode);
                    }

                    workorderModel.Customer = customerModel;
                    workorderModel.Customer.CustomerId = customerModel.CustomerId;
                    workorderModel.Notes = workorderManagement.Notes;
                    workorderModel.Operation = WorkOrderManagementSubmitType.CREATEWORKORDER;
                    workorderModel.WorkOrder = new WorkOrder();
                    workorderModel.WorkOrder.WorkorderCalltypeid = Convert.ToInt32(workorderManagement.callReason);
                    if (workorderModel.WorkOrder.WorkorderCalltypeid != null)
                    {
                        workorderModel.WorkOrder.WorkorderCalltypeDesc = FarmerBrothersEntitites.WorkorderTypes.Where(t => t.CallTypeID == workorderModel.WorkOrder.WorkorderCalltypeid).Select(td => td.Description).FirstOrDefault();
                    }
                    workorderModel.WorkOrder.PriorityCode = 5;
                    workorderModel.WorkOrder.WorkOrderBrands = new List<WorkOrderBrand>();
                    WorkOrderBrand brand = new WorkOrderBrand();
                    brand.BrandID = 997;
                    workorderModel.WorkOrder.WorkOrderBrands.Add(brand);
                    workorderModel.PriorityList = new List<AllFBStatu>();
                    AllFBStatu priority = new AllFBStatu();
                    priority.FBStatusID = 5;
                    priority.FBStatus = "Next Day Service";
                    workorderModel.PriorityList.Add(priority);
                    workorderModel.NewNotes = new List<NewNotesModel>();
                    workorderModel.NewNotes = workorderManagement.NewNotes;
                    //Used it to save in work order Equipment table
                    workorderModel.WorkOrder.ClosedUserName = workorderManagement.EquipmentLocation;

                    workorderModel.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
                    workorderModel.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
                    workorderModel.WorkOrderParts = new List<WorkOrderPartModel>();
                    WorkorderController wc = new WorkorderController();


                    jsonResult = wc.SaveWorkOrder(workorderModel, null, string.Empty, true);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    WorkOrderResults result = serializer.Deserialize<WorkOrderResults>(serializer.Serialize(jsonResult.Data));
                    if (result.returnValue > 0)
                    {
                        workorderManagement.WorkOrderID = Convert.ToInt32(result.WorkOrderId);
                        message = @"|Work Order created successfully! Work Order ID#: " + workorderManagement.WorkOrderID;
                        NotesHistory notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 1,
                            EntryDate = CurrentTime,
                            Notes = @"Work Order created from MARS WO#: " + Convert.ToInt32(result.WorkOrderId) + @" in “MARS”!",
                            Userid = 99999,
                            UserName = "WEB"

                        };
                        FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                        FarmerBrothersEntitites.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    message = "|There is a problem in Work Order Creation! Please contact support.";
                }
                #endregion

            }
            
            
            redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "AutoCallGenerate")                                                                      ;

            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = redirectUrl, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        #region captch

        public ActionResult Refresh(CaptchaParams parameters)
        {

            return parameters.CaptchaActions();

        }
        #endregion

    }
}