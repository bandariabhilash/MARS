using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class EventsUploadController : BaseController
    {
        // GET: EventsUpload
        [HttpGet]
        public ActionResult EventsUpload()
        {
            return View();
        }

        public ActionResult UploadBulkEvents(HttpPostedFileBase file)
        {
            JsonResult jsonResult = new JsonResult();
            List<WorkorderManagementModel> workorderModelList = new List<WorkorderManagementModel>();
            try
            {
                if (file == null)
                {
                    ViewBag.Message = "No File Selected ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<WorkorderSearchResultModel>();
                }
                else if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                {
                    ViewBag.Message = "Selected file is not CSV file ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<WorkorderSearchResultModel>();
                }
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string DirPath = Server.MapPath("~/UploadedFiles/Events");
                    DateTime currentDate = DateTime.Now;
                    if (!Directory.Exists(DirPath))
                    {
                        Directory.CreateDirectory(DirPath);
                    }
                    string _inputPath = Path.Combine(DirPath, _FileName);
                    file.SaveAs(_inputPath);
                    string line;
                    var csvReader = new StreamReader(file.InputStream);
                    List<ERFBulkUploadDataModel> erfDataList = new List<ERFBulkUploadDataModel>();

                    FileReading fileDataObj = new FileReading();
                    fileDataObj.IsValid = true;
                    fileDataObj.FileName = _FileName;
                    int i = 0;

                    string headerRows = "AccountNumber,CompanyName,Address1,Address2,City,State,Zip,EquipCallType,NOTES,EventContact,ContactPhone,CustomerPO";

                    while ((line = csvReader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line)) continue;

                        string lineVal = line;
                        if (string.IsNullOrEmpty(lineVal) || lineVal == " ") continue;

                        if (i == 0)
                        {
                            fileDataObj = IsValidEventsCSVFile(headerRows, lineVal);

                            if (!fileDataObj.IsValid)
                            {
                                ViewBag.Message = "File upload failed!! " + "\n" + fileDataObj.ErrorMsg;
                                ViewBag.isSuccess = false;
                                ViewBag.dataSource = new List<CustomerModel>();
                            }
                        }

                        Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                        String[] lineValues = CSVParser.Split(lineVal);

                        if (i != 0)
                        {
                            string CustomerId = "", CustomerName = "", Address1 = "", Address2 = "", Address3 = "", City = "", State = "", ZipCode = "", PhoneNumber = "", CustomerPO = "", Notes = "", EvntContact = "", Route = "", Branch = "", RouteCode = "";
                            string ErrorMessage = "", Email = "", pricingParentId = "";
                            int EventCallType = 0;

                            for (int ind = 0; ind <= lineValues.Count() - 1; ind++)
                            {
                                string str = lineValues[ind].Trim();
                                switch (ind)
                                {
                                    case 0:
                                        CustomerId = string.IsNullOrEmpty(str) ? "" : str.Trim();
                                        if (string.IsNullOrEmpty(CustomerId)) { ErrorMessage += "Customer Number is Missing"; }
                                        break;
                                    case 1:
                                        CustomerName = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        if (string.IsNullOrEmpty(CustomerName)) { ErrorMessage += "Customer Name is Missing"; }
                                        break;
                                    case 2:
                                        Address1 = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        if (string.IsNullOrEmpty(Address1)) { ErrorMessage += "Address1 is Missing"; }
                                        break;
                                    case 3:
                                        Address2 = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        break;
                                    case 4:
                                        City = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        if (string.IsNullOrEmpty(City)) { ErrorMessage += "City is Missing"; }
                                        break;
                                    case 5:
                                        State = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        if (string.IsNullOrEmpty(State)) { ErrorMessage += "State is Missing"; }
                                        break;
                                    case 6:
                                        ZipCode = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        if (string.IsNullOrEmpty(ZipCode)) { ErrorMessage += "ZipCode is Missing"; }
                                        break;
                                    case 7:
                                        EventCallType = string.IsNullOrEmpty(str) ? 0 : Convert.ToInt32(str.Trim().ToString());
                                        break;
                                    case 8:
                                        Notes = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        break;
                                    case 9:
                                        EvntContact = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        break;
                                    case 10:
                                        PhoneNumber = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        break;
                                    case 11:
                                        CustomerPO = string.IsNullOrEmpty(str) ? "" : str.Trim().ToString();
                                        break;
                                }
                            }

                            if (!string.IsNullOrEmpty(ErrorMessage))
                            {
                                WorkorderManagementModel woMdl = new WorkorderManagementModel();
                                woMdl.Customer = new CustomerModel();
                                woMdl.Customer.CustomerId = CustomerId;
                                woMdl.Customer.CustomerName = CustomerName;
                                woMdl.Customer.Address = Address1;
                                woMdl.Customer.Address2 = Address2;
                                woMdl.Customer.City = City;
                                woMdl.Customer.State = State;
                                woMdl.Customer.ZipCode = ZipCode;
                                woMdl.Customer.PhoneNumber = PhoneNumber;

                                woMdl.WorkOrder = new WorkOrder();
                                woMdl.WorkOrder.CustomerID = string.IsNullOrEmpty(CustomerId) ? 0 : Convert.ToInt32(CustomerId);
                                woMdl.WorkOrder.CustomerName = CustomerName;
                                woMdl.WorkOrder.CustomerAddress = Address1;
                                woMdl.WorkOrder.CustomerCity = City;
                                woMdl.WorkOrder.CustomerState = State;
                                woMdl.WorkOrder.CustomerZipCode = ZipCode;
                                woMdl.WorkOrder.CustomerPhone = PhoneNumber;

                                woMdl.WorkOrder.CustomerPO = CustomerPO;
                                woMdl.WorkOrder.WorkorderCalltypeid = EventCallType;
                                woMdl.WorkOrder.WorkorderContactName = EvntContact;

                                woMdl.Notes = new NotesModel();
                                woMdl.Notes.Notes = Notes;

                                woMdl.Message = ErrorMessage;

                                workorderModelList.Add(woMdl);
                            }
                            else
                            {
                                WorkorderManagementModel woMdl = new WorkorderManagementModel();
                                woMdl.Customer = new CustomerModel();
                                woMdl.Customer.CustomerId = CustomerId;
                                woMdl.Customer.CustomerName = CustomerName;
                                woMdl.Customer.Address = Address1;
                                woMdl.Customer.Address2 = Address2;
                                woMdl.Customer.City = City;
                                woMdl.Customer.State = State;
                                woMdl.Customer.ZipCode = ZipCode;
                                woMdl.Customer.PhoneNumber = PhoneNumber;

                                woMdl.WorkOrder = new WorkOrder();
                                woMdl.WorkOrder.CustomerID = string.IsNullOrEmpty(CustomerId) ? 0 : Convert.ToInt32(CustomerId);                                
                                woMdl.WorkOrder.CustomerName = CustomerName;
                                woMdl.WorkOrder.CustomerAddress = Address1;
                                woMdl.WorkOrder.CustomerCity = City;
                                woMdl.WorkOrder.CustomerState = State;
                                woMdl.WorkOrder.CustomerZipCode = ZipCode;
                                woMdl.WorkOrder.CustomerPhone = PhoneNumber;

                                woMdl.WorkOrder.CustomerPO = CustomerPO;
                                woMdl.WorkOrder.WorkorderCalltypeid = EventCallType;
                                woMdl.WorkOrder.WorkorderContactName = EvntContact;

                                woMdl.Notes = new NotesModel();
                                woMdl.Notes.Notes = Notes;

                                WorkOrder workOrder = null;
                                string message = string.Empty;
                                int returnValue = 0;

                                returnValue = WorkOrderSave(woMdl, FarmerBrothersEntitites, out workOrder, out message);

                                if(returnValue > 0)
                                {
                                    woMdl.WorkOrder.WorkorderID = workOrder.WorkorderID;
                                    woMdl.Message = "Success";
                                }
                                else
                                {
                                    woMdl.Message = message;
                                }

                                workorderModelList.Add(woMdl);
                            }
                        }

                        i++;
                    }

                }


                ViewBag.Message = "File uploaded ! ";
                ViewBag.isSuccess = true;
                ViewBag.dataSource = workorderModelList;
                return View("EventsUpload");

            }
            catch (Exception ex)
            {
                ViewBag.Message = "File uploaded ! ";
                ViewBag.isSuccess = true;
                ViewBag.dataSource = workorderModelList;
                return View("EventsUpload");

            }            
        }

        private static FileReading IsValidEventsCSVFile(string ExpectedRow, string HeaderRow)
        {
            FileReading fr = new FileReading();
            fr.ErrorMsg = "";
            fr.IsValid = true;

            string[] headerValues = HeaderRow.Split(',');
            string[] expectedValues = ExpectedRow.Split(',');

            for (var index = 0; index <= expectedValues.Count() - 1; index++)
            {
                string expValue = expectedValues.ElementAtOrDefault(index) != null ? expectedValues[index].ToLower().Trim() : "";
                string hdrValue = headerValues.ElementAtOrDefault(index) != null ? headerValues[index].ToLower().Trim() : "";

                if (expValue != hdrValue)
                {
                    fr.ErrorMsg += "\n " + expValue + " Column Missing";
                    fr.IsValid = false;
                }
            }

            return fr;
        }

        public int WorkOrderSave(WorkorderManagementModel workorderManagementModel, FarmerBrothersEntities FarmerBrothersEntitites, out WorkOrder workOrder, out string message)
        {
            int returnValue = 0;
            message = string.Empty;
            workOrder = null;
            //WorkorderManagementModel workorderManagement = new WorkorderManagementModel();

            WorkorderType WOTyp = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == workorderManagementModel.WorkOrder.WorkorderCalltypeid).FirstOrDefault();

            workorderManagementModel.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
            WorkOrderManagementEquipmentModel eqp = new WorkOrderManagementEquipmentModel();
            eqp.CallTypeID = WOTyp.CallTypeID;
            eqp.Category = "N/A";
            eqp.Location = "N/A";

            workorderManagementModel.WorkOrderEquipmentsRequested.Add(eqp);

            var CustomerId = int.Parse(workorderManagementModel.Customer.CustomerId);
            Contact serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();

            workOrder = workorderManagementModel.FillCustomerData(workorderManagementModel.WorkOrder, false, FarmerBrothersEntitites);


            IndexCounter counter = Utility.GetIndexCounter("WorkorderID", 1);
            counter.IndexValue++;
            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

            workOrder.WorkorderID = counter.IndexValue.Value;
            workOrder.WorkorderContactName = workorderManagementModel.WorkOrder.WorkorderContactName;
            workOrder.WorkorderContactPhone = workorderManagementModel.WorkOrder.CustomerPhone;
            workOrder.CallerName = workorderManagementModel.WorkOrder.WorkorderContactName;
           

            workOrder.WorkorderCalltypeid = WOTyp.CallTypeID;//workorderManagement.WorkOrder.WorkorderCalltypeid;
            workOrder.WorkorderCalltypeDesc = WOTyp.Description;// workorderManagement.WorkOrder.WorkorderCalltypeDesc;
            workOrder.WorkorderErfid = workorderManagementModel.WorkOrder.WorkorderErfid;
            workOrder.WorkorderEquipCount = Convert.ToInt16(workorderManagementModel.WorkOrderEquipmentsRequested.Count());

            AllFBStatu priority = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatus.ToLower() == "next day service").FirstOrDefault();
            int priorityStatusId = 52;

            workOrder.PriorityCode = priorityStatusId;

            AllFBStatu FarmarBortherStatus = FarmerBrothersEntitites.AllFBStatus.Where(a => a.FBStatus == "None" && a.StatusFor == "Follow Up Call").FirstOrDefault();
            if (FarmarBortherStatus != null)
            {
                workOrder.FollowupCallID = FarmarBortherStatus.FBStatusID;
            }

            DateTime CurrentTime = Utility.GetCurrentTime(workorderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
            workOrder.WorkorderEntryDate = CurrentTime;
            workOrder.WorkorderModifiedDate = CurrentTime;
            workOrder.ModifiedUserName = "Fetco WEB";
            workOrder.IsAutoGenerated = true;
            workOrder.EntryUserName = "Fetco WEB";

            workOrder.WorkorderModifiedDate = CurrentTime;
            workOrder.WorkorderCallstatus = "Open";

            NotesHistory customerNotes = new NotesHistory()
            {
                AutomaticNotes = 0,
                EntryDate = CurrentTime,
                Notes = workorderManagementModel.Notes.Notes,
                Userid = 99999,
                UserName = "Events Upload",
                WorkorderID = workOrder.WorkorderID
            };
            FarmerBrothersEntitites.NotesHistories.Add(customerNotes);


            {

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = CurrentTime,
                    Notes = "Work Order created from Events Upload, WO#: " + workOrder.WorkorderID + " !",
                    Userid = 99999,
                    UserName = "Events Upload",
                    WorkorderID = workOrder.WorkorderID
                };
                workOrder.NotesHistories.Add(notesHistory);

                workorderManagementModel.NewNotes = new List<NewNotesModel>();

                foreach (NewNotesModel newNotesModel in workorderManagementModel.NewNotes)
                {
                    NotesHistory newnotesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = CurrentTime,
                        Notes = newNotesModel.Text,
                        Userid = 99999,
                        UserName = "Events Upload",
                        WorkorderID = workOrder.WorkorderID
                    };
                    FarmerBrothersEntitites.NotesHistories.Add(newnotesHistory);
                }
                workorderManagementModel.Notes = new NotesModel();
                if (workorderManagementModel.Notes.TechID != null && workorderManagementModel.Notes.TechID != "-1")
                {
                    workOrder.SpecificTechnician = workorderManagementModel.Notes.TechID;
                }

                workOrder.IsSpecificTechnician = workorderManagementModel.Notes.IsSpecificTechnician;
                workOrder.IsAutoDispatched = workorderManagementModel.Notes.IsAutoDispatched;

                foreach (WorkOrderBrand brand in workorderManagementModel.WorkOrder.WorkOrderBrands)
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
                FarmerBrothersEntitites.Entry(assetCounter).State = System.Data.Entity.EntityState.Modified;

                WorkorderEquipment equipment = new WorkorderEquipment()
                {
                    Assetid = assetCounter.IndexValue.Value,
                    CallTypeid = WOTyp.CallTypeID,//workorderManagement.WorkOrder.WorkorderCalltypeid,
                    Category = ".11 - No Info – Only OTHER",
                    Location = workorderManagementModel.WorkOrder.ClosedUserName
                };
                workOrder.WorkorderEquipments.Add(equipment);

                WorkorderEquipmentRequested equipmentReq = new WorkorderEquipmentRequested()
                {
                    Assetid = assetCounter.IndexValue.Value,
                    CallTypeid = WOTyp.CallTypeID,//workorderManagement.WorkOrder.WorkorderCalltypeid,
                    Category = ".11 - No Info – Only OTHER",
                    Location = workorderManagementModel.WorkOrder.ClosedUserName
                };
                workOrder.WorkorderEquipmentRequesteds.Add(equipmentReq);

                notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = workOrder.WorkorderEntryDate,
                    Notes = workOrder.WorkorderCalltypeDesc + " Work Order # " + workOrder.WorkorderID + " in MARS!",
                    Userid = 99999,
                    UserName = "Events Upload"
                };
                workOrder.NotesHistories.Add(notesHistory);
                if (workorderManagementModel.Erf != null)
                {
                    workOrder.WorkorderErfid = workorderManagementModel.Erf.ErfID;
                }

            }


            if (workorderManagementModel.RemovalCount > 5)
            {
                workOrder.WorkorderCallstatus = "Open";
            }

            FarmerBrothersEntitites.WorkOrders.Add(workOrder);


            WorkorderDetail workOrderDetail = new WorkorderDetail()
            {
                WorkorderID = workOrder.WorkorderID,
                EntryDate = workOrder.WorkorderEntryDate,
                ModifiedDate = workOrder.WorkorderEntryDate,
                SpecialClosure = null,
            };


            FarmerBrothersEntitites.WorkorderDetails.Add(workOrderDetail);
            workOrder.CurrentUserName = "Fetco WEB";

            int effectedRecords = FarmerBrothersEntitites.SaveChanges();
            returnValue = effectedRecords > 0 ? 1 : 0;

            return returnValue;
        }

       
    }
}