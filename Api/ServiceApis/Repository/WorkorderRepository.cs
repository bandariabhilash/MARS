using DataAccess.Db;
using ServiceApis.IRepository;
using ServiceApis.Models;
using ServiceApis.Utilities;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;
using Customer = DataAccess.Db.Contact;

namespace ServiceApis.Repository
{
    public class WorkorderRepository : IWorkorderRepository
    {

        //private readonly FBContext _context;
        private readonly ICustomerRepository _customerRepository;
        public WorkorderRepository(FBContext context, ICustomerRepository customerRepository)
        {
            //_context = context;
            _customerRepository = customerRepository;
        }

        public ResultResponse<ERFResponseClass> SaveERFWorkorder(WorkorderRequestModel RequestData, int userId, string userName)        
        {
            ResultResponse<ERFResponseClass> result = new ResultResponse<ERFResponseClass>();

            WorkorderManagementModel workorderModel = new WorkorderManagementModel();
            using (var _context = new FBContext())
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        bool isvalid = true;
                        string errorMessage = string.Empty;
                        if (RequestData.AccountNumber != 0)
                        {
                            workorderModel.Customer = new CustomerModel();
                            CustomerModel custMdl = _customerRepository.GetCustomerDetails(RequestData.AccountNumber);

                            if (custMdl == null)
                            {
                                if (string.IsNullOrWhiteSpace(RequestData.CustomerName))
                                {
                                    errorMessage += " CustomerName is required";
                                    isvalid = false;
                                }
                                if (string.IsNullOrWhiteSpace(RequestData.Address1))
                                {
                                    errorMessage += " Address1 is required";
                                    isvalid = false;
                                }
                                if (string.IsNullOrWhiteSpace(RequestData.City))
                                {
                                    if(!string.IsNullOrWhiteSpace(errorMessage))
                                    {
                                        errorMessage += "\n";
                                    }

                                    errorMessage += " City is required";
                                    isvalid = false;
                                }
                                if (string.IsNullOrWhiteSpace(RequestData.State))
                                {
                                    if (!string.IsNullOrWhiteSpace(errorMessage))
                                    {
                                        errorMessage += "\n";
                                    }

                                    errorMessage += " State is required";
                                    isvalid = false;
                                }
                                if (string.IsNullOrWhiteSpace(RequestData.PostalCode))
                                {
                                    if (!string.IsNullOrWhiteSpace(errorMessage))
                                    {
                                        errorMessage += "\n";
                                    }

                                    errorMessage += " PostalCode is required";
                                    isvalid = false;
                                }

                                if(!isvalid)
                                {
                                    transaction.Rollback();

                                    result.responseCode = 500;
                                    result.Message = errorMessage;
                                    result.IsSuccess = false;

                                    return result;
                                }
                                else
                                {
                                    workorderModel.Customer = new CustomerModel();
                                    workorderModel.Customer.CustomerId = RequestData.AccountNumber.ToString();
                                    workorderModel.Customer.Address = RequestData.Address1;
                                    workorderModel.Customer.Address2 = RequestData.Address2;
                                    workorderModel.Customer.City = RequestData.City;
                                    workorderModel.Customer.State = RequestData.State;
                                    workorderModel.Customer.ZipCode = RequestData.PostalCode;

                                    int custSaveResult = CustomerModel.saveCustomerDetails(workorderModel.Customer, _context);                                    
                                    if (custSaveResult == 0)
                                    {
                                        transaction.Rollback();

                                        result.responseCode = 500;
                                        result.Message = "Customer Details saving Failed";
                                        result.IsSuccess = false;

                                        return result;
                                    }
                                    else
                                    {
                                        custMdl = new CustomerModel();
                                        custMdl.CustomerId = RequestData.AccountNumber.ToString();
                                        custMdl.CustomerName = RequestData.CustomerName;
                                        custMdl.Address = RequestData.Address1;
                                        custMdl.Address2 = RequestData.Address2;
                                        custMdl.City = RequestData.City;
                                        custMdl.State = RequestData.State;
                                        custMdl.ZipCode = RequestData.PostalCode;
                                    }
                                }
                            }

                            workorderModel.Customer = custMdl;
                            workorderModel.Customer.CustomerId = RequestData.AccountNumber.ToString();
                        }
                        else
                        {
                            errorMessage += "Invalid Customer";
                            isvalid = false;
                        }

                        if(RequestData.ERFId == 0)
                        {
                            errorMessage += " ERFId is required";
                            isvalid = false;
                        }

                        if (!isvalid)
                        {
                            transaction.Rollback();

                            result.responseCode = 500;
                            result.Message = errorMessage;
                            result.IsSuccess = false;

                            return result;
                        }


                        //workorderModel.Notes = RequestData.Comments;
                        workorderModel.WorkOrder = new WorkOrder();
                        workorderModel.WorkOrder.CallerName = "N/A";
                        workorderModel.WorkOrder.WorkorderContactName = "N/A";
                        workorderModel.WorkOrder.HoursOfOperation = "N/A";
                        workorderModel.WorkOrder.WorkorderCalltypeid = 1300;
                        workorderModel.WorkOrder.WorkorderCalltypeDesc = "Installation";
                        workorderModel.WorkOrder.WorkorderErfid = RequestData.ERFId.ToString();
                        workorderModel.WorkOrder.PriorityCode = 54;
                        workorderModel.WorkOrder.WorkOrderBrands = new List<WorkOrderBrand>();
                        WorkOrderBrand brand = new WorkOrderBrand();
                        brand.BrandId = 997;
                        workorderModel.WorkOrder.WorkOrderBrands.Add(brand);
                        workorderModel.PriorityList = new List<AllFbstatus>();
                        AllFbstatus priority = new AllFbstatus();
                        priority.FbstatusId = 54;
                        priority.Fbstatus = "P3  - PLANNED";
                        workorderModel.PriorityList.Add(priority);
                        //workorderModel.NewNotes = new List<NewNotesModel>();
                        //workorderModel.NewNotes = erfModel.NewNotes;

                        workorderModel.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
                        workorderModel.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
                        workorderModel.WorkOrderParts = new List<WorkOrderPartModel>();
                        //workorderModel.Erf = erfModel.ErfAssetsModel.Erf;
                        workorderModel.Erf = new Erf();
                        workorderModel.Erf.ErfId = RequestData.ERFId.ToString();


                        result = SaveWorkorderData(workorderModel, userId, userName, _context);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();

                        result.responseCode = 500;
                        result.Message = "Workorder Save Failed";
                        result.IsSuccess = false;

                        return result;
                    }
                }
            }

            return result;
        }

        public ResultResponse<ERFResponseClass> SaveWorkorderData(WorkorderManagementModel workorderManagement, int userId, string userName, FBContext _context)
        {
            ResultResponse<ERFResponseClass> result = new ResultResponse<ERFResponseClass>();

            int returnValue = 0;
            string message = string.Empty;
             WorkOrder workOrder = null;


            try
            {
                DateTime currentTime = DateTime.Now;//Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, WOFBEntity);
                //DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, _context);

                var CustomerId = int.Parse(workorderManagement.Customer.CustomerId);
                Customer serviceCustomer = _context.Contacts.Where(x => x.ContactId == CustomerId).FirstOrDefault();

                //serviceCustomer.FilterReplaced = workorderManagement.Closure.FilterReplaced;
                serviceCustomer.FilterReplacedDate = currentTime;
                serviceCustomer.NextFilterReplacementDate = currentTime.AddMonths(6);

                workOrder = workorderManagement.FillCustomerData(new WorkOrder(), true, _context, serviceCustomer);
                //workOrder.EntryUserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : "";

                IndexCounterModel counter = Utility.GetIndexCounter("WorkorderID", 1);
                counter.IndexValue++;

                workOrder.WorkorderId = counter.IndexValue.Value;
                workOrder.WorkorderCalltypeid = workorderManagement.WorkOrder.WorkorderCalltypeid;
                workOrder.WorkorderCalltypeDesc = workorderManagement.WorkOrder.WorkorderCalltypeDesc;
                workOrder.WorkorderErfid = workorderManagement.WorkOrder.WorkorderErfid;
                workOrder.WorkorderEquipCount = Convert.ToInt16(workorderManagement.WorkOrderEquipmentsRequested.Count());
                //workOrder.PriorityCode = workorderManagement.PriorityList[0].FBStatusID;

                AllFbstatus FarmarBortherStatus = _context.AllFbstatuses.Where(a => a.Fbstatus == "None" && a.StatusFor == "Follow Up Call").FirstOrDefault();
                if (FarmarBortherStatus != null)
                {
                    workOrder.FollowupCallId = FarmarBortherStatus.FbstatusId;
                }

                TimeZoneInfo newTimeZoneInfo = null;
                Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, _context);
                DateTime CurrentTime = DateTime.Now;//Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, WOFBEntity);
                workOrder.WorkorderEntryDate = CurrentTime;
                workOrder.WorkorderModifiedDate = workOrder.WorkorderEntryDate;
                workOrder.ModifiedUserName = userName;

                workOrder.WorkorderModifiedDate = workOrder.WorkorderEntryDate;
                workOrder.WorkorderCallstatus = "Open";

                {
                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Work Order created from ERF SERVICE,  WO#: " + workOrder.WorkorderId + @" in “MARS”!",
                        Userid = userId,
                        UserName = userName,
                        IsDispatchNotes = 0
                    };
                    notesHistory.WorkorderId = workOrder.WorkorderId;
                    workOrder.NotesHistories.Add(notesHistory);


                    //foreach (NewNotesModel newNotesModel in workorderManagement.NewNotes)
                    {
                        NotesHistory newnotesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "Workorder Created for ERF#  " + workorderManagement.WorkOrder.WorkorderErfid + " from Erf SERVICE",
                            Userid = userId,
                            UserName = userName,
                            WorkorderId = workOrder.WorkorderId,
                            IsDispatchNotes = 0
                        };
                        _context.NotesHistories.Add(newnotesHistory);
                    }

                    if (workorderManagement.Notes != null)
                    {
                        if (workorderManagement.Notes.TechID != null && workorderManagement.Notes.TechID != "-1")
                        {
                            workOrder.SpecificTechnician = workorderManagement.Notes.TechID;
                        }

                        workOrder.IsSpecificTechnician = workorderManagement.Notes.IsSpecificTechnician;
                        workOrder.IsAutoDispatched = workorderManagement.Notes.IsAutoDispatched;
                    }

                    foreach (WorkOrderBrand brand in workorderManagement.WorkOrder.WorkOrderBrands)
                    {
                        WorkOrderBrand newBrand = new WorkOrderBrand();
                        foreach (var property in brand.GetType().GetProperties())
                        {
                            if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                            {
                                property.SetValue(newBrand, property.GetValue(brand));
                            }
                        }
                        newBrand.WorkorderId = workOrder.WorkorderId;
                        workOrder.WorkOrderBrands.Add(newBrand);
                    }

                    IndexCounterModel assetCounter = Utility.GetIndexCounter("AssetID", 1);
                    assetCounter.IndexValue++;

                    WorkorderEquipment equipment = new WorkorderEquipment()
                    {
                        Assetid = assetCounter.IndexValue.Value,
                        CallTypeid = 1300,
                        Category = "OTHER",
                        Symptomid = 2001,
                        Location = "OTH"
                    };
                    workOrder.WorkorderEquipments.Add(equipment);

                    WorkorderEquipmentRequested equipmentReq = new WorkorderEquipmentRequested()
                    {
                        Assetid = assetCounter.IndexValue.Value,
                        CallTypeid = 1300,
                        Category = "OTHER",
                        Symptomid = 2001,
                        Location = "OTH"
                    };
                    workOrder.WorkorderEquipmentRequesteds.Add(equipmentReq);
                    //if (isAutoGenWO)
                    //{
                    //    workorderManagement.WorkOrder.ClosedUserName = string.Empty;
                    //}
                    notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = workOrder.WorkorderEntryDate,
                        Notes = workOrder.WorkorderCalltypeDesc + " Work Order # " + workOrder.WorkorderId + " in MARS!",
                        Userid = userId,
                        UserName = userName,
                        IsDispatchNotes = 0,
                        WorkorderId = workOrder.WorkorderId
                    };
                    workOrder.NotesHistories.Add(notesHistory);
                    if (workorderManagement.Erf != null)
                    {
                        workOrder.WorkorderErfid = workorderManagement.Erf.ErfId;
                    }

                }


                if (workorderManagement.RemovalCount > 5)
                {
                    workOrder.WorkorderCallstatus = "Open";
                }

                _context.WorkOrders.Add(workOrder);
                SaveRemovalDetails(workorderManagement, workOrder, userId, userName, _context);

                workOrder.WorkOrderOpenedDateTime = DateTime.Now;
                workOrder.CurrentUserName = userName;

                WorkorderDetail wd = new WorkorderDetail();
                wd.WorkorderId = workOrder.WorkorderId;
                wd.TravelTime = ":";
                workOrder.WorkorderDetails.Add(wd);


                _context.SaveChanges();

                returnValue = 1;

                result.responseCode = 200;
                result.IsSuccess = true;
                result.Data = new ERFResponseClass();
                result.Data.WorkorderId = Convert.ToInt32(workOrder.WorkorderId);
                result.Data.ERFId = Convert.ToInt32(workOrder.WorkorderErfid);
            }
            catch (Exception ex)
            {
                returnValue = 0;
                message += @"|Error Creating Workorder!";
                result.responseCode = 500;
                result.IsSuccess = false;
                result.Message = message;
            }            

            return result;
        }

        private void SaveRemovalDetails(WorkorderManagementModel workorderManagement, WorkOrder workOrder, int userId, string userName, FBContext _context)
        {
            if (workorderManagement.RemovalCount > 0)
            {
                AllFbstatus status = _context.AllFbstatuses.Where(a => a.FbstatusId == workorderManagement.RemovalReason).FirstOrDefault();
                RemovalSurvey survey = _context.RemovalSurveys.Where(r => r.WorkorderId == workOrder.WorkorderId).FirstOrDefault();
                if (survey != null)
                {
                    survey.JmsownedMachines = workorderManagement.RemovalCount;
                    survey.RemovalDate = workorderManagement.RemovalDate;
                    if (status != null)
                    {
                        survey.RemovalReason = status.Fbstatus;
                    }
                    survey.RemoveAllMachines = workorderManagement.RemovaAll.ToString();
                    survey.BeveragesSupplier = workorderManagement.BeveragesSupplier;
                }
                else
                {
                    RemovalSurvey newSurvey = new RemovalSurvey()
                    {
                        BeveragesSupplier = workorderManagement.BeveragesSupplier,
                        JmsownedMachines = workorderManagement.RemovalCount,
                        RemovalDate = workorderManagement.RemovalDate,
                        RemoveAllMachines = workorderManagement.RemovaAll.ToString(),
                        WorkorderId = workOrder.WorkorderId,
                        RemovalReason = status.Fbstatus
                    };
                    _context.RemovalSurveys.Add(newSurvey);

                    TimeZoneInfo newTimeZoneInfo = null;
                    Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, _context);
                    DateTime currentTime = DateTime.Now;//Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, context);

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = currentTime,
                        Notes = "How many FB owned machines will we be removing? - " + workorderManagement.RemovalCount,
                        Userid = userId,
                        UserName = userName,
                        IsDispatchNotes = 1
                    };
                    workOrder.NotesHistories.Add(notesHistory);

                    if (workorderManagement.RemovalDate.HasValue)
                    {
                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "What date will you need these machines removed by? - " + workorderManagement.RemovalDate.Value.ToString("MM/dd/yyyy"),
                            Userid = userId,
                            UserName = userName,
                            IsDispatchNotes = 1
                        };
                        workOrder.NotesHistories.Add(notesHistory);
                    }

                    notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = currentTime,
                        Notes = "Are we removing all machines from your facility? - " + workorderManagement.RemovaAll.ToString(),
                        Userid = userId,
                        UserName = userName,
                        IsDispatchNotes = 1
                    };
                    workOrder.NotesHistories.Add(notesHistory);

                    if (workorderManagement.RemovaAll)
                    {

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "May I ask the reason you have chosen to remove our machines from your location? - " + status.Fbstatus,
                            Userid = userId,
                            UserName = userName,
                            IsDispatchNotes = 1
                        };
                        workOrder.NotesHistories.Add(notesHistory);

                        if (!string.IsNullOrWhiteSpace(workorderManagement.BeveragesSupplier))
                        {
                            notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 0,
                                EntryDate = currentTime,
                                Notes = "Who will be supplying your beverages going forward? - " + workorderManagement.BeveragesSupplier,
                                Userid = userId,
                                UserName = userName,
                                IsDispatchNotes = 1
                            };
                            workOrder.NotesHistories.Add(notesHistory);
                        }

                        StringBuilder notes = new StringBuilder();
                        if (workorderManagement.ClosingBusiness)
                        {
                            notes.Append("Closing Business;");
                        }
                        if (workorderManagement.FlavorOrTasteOfCoffee)
                        {
                            notes.Append("Flavor/Taste of Coffee;");
                        }
                        if (workorderManagement.EquipmentServiceReliabilityorResponseTime)
                        {
                            notes.Append("Equipment service reliability / response time;");
                        }
                        if (workorderManagement.EquipmentReliability)
                        {
                            notes.Append("Equipment reliability;");
                        }
                        if (workorderManagement.CostPerCup)
                        {
                            notes.Append("Cost per Cup;");
                        }
                        if (workorderManagement.ChangingGroupPurchasingProgram)
                        {
                            notes.Append("Changing group purchasing program;");
                        }
                        if (workorderManagement.ChangingDistributor)
                        {
                            notes.Append("Changing Distributor;");
                        }

                        if (!string.IsNullOrWhiteSpace(notes.ToString()))
                        {
                            notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 0,
                                EntryDate = currentTime,
                                Notes = "What were the main reasons to change your beverage solution? - " + notes.ToString(),
                                Userid = userId,
                                UserName = userName,
                                IsDispatchNotes = 1
                            };
                            workOrder.NotesHistories.Add(notesHistory);
                        }
                    }
                }

                if (workorderManagement.RemovalCount > 1 && workorderManagement.RowId.HasValue && workorderManagement.RowId.Value < workorderManagement.WorkOrderEquipmentsRequested.Count())
                {
                    WorkOrderManagementEquipmentModel equipmentFromModel = workorderManagement.WorkOrderEquipmentsRequested.ElementAt(workorderManagement.RowId.Value);
                    IEnumerable<WorkorderEquipmentRequested> workOrderEquipments = _context.WorkorderEquipmentRequesteds.Where(we => we.WorkorderId == workOrder.WorkorderId);
                    if (workOrderEquipments != null)
                    {
                        IndexCounterModel counter = Utility.GetIndexCounter("AssetID", workOrderEquipments.Count());
                        for (int count = 0; count < workorderManagement.RemovalCount - 1; count++)
                        {
                            counter.IndexValue++;
                            WorkorderEquipmentRequested equipment = new WorkorderEquipmentRequested()
                            {
                                Assetid = counter.IndexValue.Value,
                                CallTypeid = 1400,
                                Category = equipmentFromModel.Category,
                                Location = equipmentFromModel.Location,
                                SerialNumber = equipmentFromModel.SerialNumber,
                                Model = equipmentFromModel.Model,
                                CatalogId = equipmentFromModel.CatelogID,
                                Symptomid = equipmentFromModel.SymptomID
                            };
                            workOrder.WorkorderEquipmentRequesteds.Add(equipment);
                        }
                    }
                }
            }
        }
    }
}
