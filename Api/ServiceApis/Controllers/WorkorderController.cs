﻿using DataAccess.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceApis.IRepository;
using ServiceApis.Models;
using ServiceApis.Repository;
using ServiceApis.Utilities;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;
using Customer = DataAccess.Db.Contact;

namespace ServiceApis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkorderController : BaseController
    {
        private readonly IWorkorderRepository _workorderRepository;
        private readonly IConfiguration _configuration;
        int defaultFollowUpCall;
        int userId;
        string userName;
        public WorkorderController(IWorkorderRepository workorderRepository, IConfiguration configuration, ILogger<AuthController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _workorderRepository = workorderRepository;
            _configuration = configuration;

            //AllFbstatus FarmarBortherStatus = context.AllFbstatuses.Where(a => a.Fbstatus == "None" && a.StatusFor == "Follow Up Call").FirstOrDefault();
            //if (FarmarBortherStatus != null)
            //{
            //    defaultFollowUpCall = FarmarBortherStatus.FbstatusId;
            //}
        }

        [HttpPost]
        [Authorize]
        [Route("CreateERFWorkorder")]
        public JsonResult CreateERFWororder([FromBody] WorkorderRequestModel RequestData)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.GivenName);

            ResultResponse<ERFResponseClass> result = _workorderRepository.SaveERFWorkorder(RequestData, Convert.ToInt32(userId), userName);

            return Json(result);
        }



        //public int ERFWorkOrderSave(WorkorderManagementModel workorderManagement, FBContext WOFBEntity, out WorkOrder workOrder, out string message, bool isAutoGenWO = false)
        //{
        //    var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //    var userName = User.FindFirstValue(ClaimTypes.GivenName);

        //    int returnValue = 0;
        //    message = string.Empty;
        //    workOrder = null;


        //    try
        //    {
        //        DateTime currentTime = DateTime.Now;//Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, WOFBEntity);

        //        var CustomerId = int.Parse(workorderManagement.Customer.CustomerId);
        //        Customer serviceCustomer = WOFBEntity.Contacts.Where(x => x.ContactId == CustomerId).FirstOrDefault();

        //        //serviceCustomer.FilterReplaced = workorderManagement.Closure.FilterReplaced;
        //        serviceCustomer.FilterReplacedDate = currentTime;
        //        serviceCustomer.NextFilterReplacementDate = currentTime.AddMonths(6);

        //        workOrder = workorderManagement.FillCustomerData(new WorkOrder(), true, WOFBEntity, serviceCustomer);
        //        //workOrder.EntryUserName = UserName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : "";

        //        IndexCounterModel counter = Utility.GetIndexCounter("WorkorderID", 1);
        //        counter.IndexValue++;

        //        workOrder.WorkorderId = counter.IndexValue.Value;
        //        workOrder.WorkorderCalltypeid = workorderManagement.WorkOrder.WorkorderCalltypeid;
        //        workOrder.WorkorderCalltypeDesc = workorderManagement.WorkOrder.WorkorderCalltypeDesc;
        //        workOrder.WorkorderErfid = workorderManagement.WorkOrder.WorkorderErfid;
        //        workOrder.WorkorderEquipCount = Convert.ToInt16(workorderManagement.WorkOrderEquipmentsRequested.Count());
        //        //workOrder.PriorityCode = workorderManagement.PriorityList[0].FBStatusID;

        //        workOrder.FollowupCallId = defaultFollowUpCall;

        //        TimeZoneInfo newTimeZoneInfo = null;
        //        Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, WOFBEntity);
        //        DateTime CurrentTime = DateTime.Now;//Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, WOFBEntity);
        //        workOrder.WorkorderEntryDate = CurrentTime;
        //        workOrder.WorkorderModifiedDate = workOrder.WorkorderEntryDate;
        //        //workOrder.ModifiedUserName = UserName;


        //        if (isAutoGenWO)
        //        {
        //            workOrder.IsAutoGenerated = true;
        //            workOrder.EntryUserName = "SERVICE";
        //        }

        //        workOrder.WorkorderModifiedDate = workOrder.WorkorderEntryDate;
        //        workOrder.WorkorderCallstatus = "Open";

        //        {

        //            NotesHistory notesHistory = new NotesHistory()
        //            {
        //                AutomaticNotes = 1,
        //                EntryDate = currentTime,
        //                Notes = @"Work Order created from ERF SERVICE,  WO#: " + workOrder.WorkorderId + @" in “MARS”!",
        //                Userid = userId,
        //                UserName = userName ,
        //                IsDispatchNotes = 0
        //            };
        //            notesHistory.WorkorderId = workOrder.WorkorderId;
        //            workOrder.NotesHistories.Add(notesHistory);


        //            //foreach (NewNotesModel newNotesModel in workorderManagement.NewNotes)
        //            {
        //                NotesHistory newnotesHistory = new NotesHistory()
        //                {
        //                    AutomaticNotes = 0,
        //                    EntryDate = currentTime,
        //                    Notes = "Workorder Created for ERF#  " + workorderManagement.WorkOrder.WorkorderErfid +" from Erf SERVICE",
        //                    Userid =userId,
        //                    UserName = userName,
        //                    WorkorderId = workOrder.WorkorderId,
        //                    IsDispatchNotes = 0
        //                };
        //                WOFBEntity.NotesHistories.Add(newnotesHistory);
        //            }
        //            if (workorderManagement.Notes.TechID != null && workorderManagement.Notes.TechID != "-1")
        //            {
        //                workOrder.SpecificTechnician = workorderManagement.Notes.TechID;
        //            }

        //            workOrder.IsSpecificTechnician = workorderManagement.Notes.IsSpecificTechnician;
        //            workOrder.IsAutoDispatched = workorderManagement.Notes.IsAutoDispatched;

        //            foreach (WorkOrderBrand brand in workorderManagement.WorkOrder.WorkOrderBrands)
        //            {
        //                WorkOrderBrand newBrand = new WorkOrderBrand();
        //                foreach (var property in brand.GetType().GetProperties())
        //                {
        //                    if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
        //                    {
        //                        property.SetValue(newBrand, property.GetValue(brand));
        //                    }
        //                }
        //                newBrand.WorkorderId = workOrder.WorkorderId;
        //                workOrder.WorkOrderBrands.Add(newBrand);
        //            }

        //            IndexCounterModel assetCounter = Utility.GetIndexCounter("AssetID", 1);
        //            assetCounter.IndexValue++;

        //            WorkorderEquipment equipment = new WorkorderEquipment()
        //            {
        //                Assetid = assetCounter.IndexValue.Value,
        //                CallTypeid = isAutoGenWO ? workorderManagement.WorkOrder.WorkorderCalltypeid : 1300,
        //                Category = isAutoGenWO ? "" : "OTHER",
        //                Symptomid = 2001,
        //                Location = isAutoGenWO ? workorderManagement.WorkOrder.ClosedUserName : "OTH"
        //            };
        //            workOrder.WorkorderEquipments.Add(equipment);

        //            WorkorderEquipmentRequested equipmentReq = new WorkorderEquipmentRequested()
        //            {
        //                Assetid = assetCounter.IndexValue.Value,
        //                CallTypeid = isAutoGenWO ? workorderManagement.WorkOrder.WorkorderCalltypeid : 1300,
        //                Category = isAutoGenWO ? "" : "OTHER",
        //                Symptomid = 2001,
        //                Location = isAutoGenWO ? workorderManagement.WorkOrder.ClosedUserName : "OTH"
        //            };
        //            workOrder.WorkorderEquipmentRequesteds.Add(equipmentReq);
        //            if (isAutoGenWO)
        //            {
        //                workorderManagement.WorkOrder.ClosedUserName = string.Empty;
        //            }
        //            notesHistory = new NotesHistory()
        //            {
        //                AutomaticNotes = 1,
        //                EntryDate = workOrder.WorkorderEntryDate,
        //                Notes = workOrder.WorkorderCalltypeDesc + " Work Order # " + workOrder.WorkorderId + " in MARS!",
        //                Userid = userId,
        //                UserName = userName,
        //                IsDispatchNotes = 0
        //            };
        //            workOrder.NotesHistories.Add(notesHistory);
        //            if (workorderManagement.Erf != null)
        //            {
        //                workOrder.WorkorderErfid = workorderManagement.Erf.ErfId;
        //            }

        //        }


        //        if (workorderManagement.RemovalCount > 5)
        //        {
        //            workOrder.WorkorderCallstatus = "Open";
        //        }

        //        WOFBEntity.WorkOrders.Add(workOrder);
        //        SaveRemovalDetails(workorderManagement, workOrder);               

        //        workOrder.WorkOrderOpenedDateTime = DateTime.Now;
        //        workOrder.CurrentUserName = "SERVICE";

        //        returnValue = 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        returnValue = 0;
        //        message += @"|Error Creating Workorder!";
        //    }

        //    return returnValue;
        //}


        //private void SaveRemovalDetails(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        //{
        //    if (workorderManagement.RemovalCount > 0)
        //    {
        //        AllFbstatus status = context.AllFbstatuses.Where(a => a.FbstatusId == workorderManagement.RemovalReason).FirstOrDefault();
        //        RemovalSurvey survey = context.RemovalSurveys.Where(r => r.WorkorderId == workOrder.WorkorderId).FirstOrDefault();
        //        if (survey != null)
        //        {
        //            survey.JmsownedMachines = workorderManagement.RemovalCount;
        //            survey.RemovalDate = workorderManagement.RemovalDate;
        //            if (status != null)
        //            {
        //                survey.RemovalReason = status.Fbstatus;
        //            }
        //            survey.RemoveAllMachines = workorderManagement.RemovaAll.ToString();
        //            survey.BeveragesSupplier = workorderManagement.BeveragesSupplier;
        //        }
        //        else
        //        {
        //            RemovalSurvey newSurvey = new RemovalSurvey()
        //            {
        //                BeveragesSupplier = workorderManagement.BeveragesSupplier,
        //                JmsownedMachines = workorderManagement.RemovalCount,
        //                RemovalDate = workorderManagement.RemovalDate,
        //                RemoveAllMachines = workorderManagement.RemovaAll.ToString(),
        //                WorkorderId = workOrder.WorkorderId,
        //                RemovalReason = status.Fbstatus
        //            };
        //            context.RemovalSurveys.Add(newSurvey);

        //            TimeZoneInfo newTimeZoneInfo = null;
        //            Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, context);
        //            DateTime currentTime = DateTime.Now;//Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, context);

        //            NotesHistory notesHistory = new NotesHistory()
        //            {
        //                AutomaticNotes = 0,
        //                EntryDate = currentTime,
        //                Notes = "How many FB owned machines will we be removing? - " + workorderManagement.RemovalCount,
        //                Userid = userId,
        //                UserName = userName,
        //                IsDispatchNotes = 1
        //            };
        //            workOrder.NotesHistories.Add(notesHistory);

        //            if (workorderManagement.RemovalDate.HasValue)
        //            {
        //                notesHistory = new NotesHistory()
        //                {
        //                    AutomaticNotes = 0,
        //                    EntryDate = currentTime,
        //                    Notes = "What date will you need these machines removed by? - " + workorderManagement.RemovalDate.Value.ToString("MM/dd/yyyy"),
        //                    Userid = userId,
        //                    UserName = userName,
        //                    IsDispatchNotes = 1
        //                };
        //                workOrder.NotesHistories.Add(notesHistory);
        //            }

        //            notesHistory = new NotesHistory()
        //            {
        //                AutomaticNotes = 0,
        //                EntryDate = currentTime,
        //                Notes = "Are we removing all machines from your facility? - " + workorderManagement.RemovaAll.ToString(),
        //                Userid = userId,
        //                UserName = userName,
        //                IsDispatchNotes = 1
        //            };
        //            workOrder.NotesHistories.Add(notesHistory);

        //            if (workorderManagement.RemovaAll)
        //            {

        //                notesHistory = new NotesHistory()
        //                {
        //                    AutomaticNotes = 0,
        //                    EntryDate = currentTime,
        //                    Notes = "May I ask the reason you have chosen to remove our machines from your location? - " + status.Fbstatus,
        //                    Userid = userId,
        //                    UserName = userName,
        //                    IsDispatchNotes = 1
        //                };
        //                workOrder.NotesHistories.Add(notesHistory);

        //                if (!string.IsNullOrWhiteSpace(workorderManagement.BeveragesSupplier))
        //                {
        //                    notesHistory = new NotesHistory()
        //                    {
        //                        AutomaticNotes = 0,
        //                        EntryDate = currentTime,
        //                        Notes = "Who will be supplying your beverages going forward? - " + workorderManagement.BeveragesSupplier,
        //                        Userid = userId,
        //                        UserName = userName,
        //                        IsDispatchNotes = 1
        //                    };
        //                    workOrder.NotesHistories.Add(notesHistory);
        //                }

        //                StringBuilder notes = new StringBuilder();
        //                if (workorderManagement.ClosingBusiness)
        //                {
        //                    notes.Append("Closing Business;");
        //                }
        //                if (workorderManagement.FlavorOrTasteOfCoffee)
        //                {
        //                    notes.Append("Flavor/Taste of Coffee;");
        //                }
        //                if (workorderManagement.EquipmentServiceReliabilityorResponseTime)
        //                {
        //                    notes.Append("Equipment service reliability / response time;");
        //                }
        //                if (workorderManagement.EquipmentReliability)
        //                {
        //                    notes.Append("Equipment reliability;");
        //                }
        //                if (workorderManagement.CostPerCup)
        //                {
        //                    notes.Append("Cost per Cup;");
        //                }
        //                if (workorderManagement.ChangingGroupPurchasingProgram)
        //                {
        //                    notes.Append("Changing group purchasing program;");
        //                }
        //                if (workorderManagement.ChangingDistributor)
        //                {
        //                    notes.Append("Changing Distributor;");
        //                }

        //                if (!string.IsNullOrWhiteSpace(notes.ToString()))
        //                {
        //                    notesHistory = new NotesHistory()
        //                    {
        //                        AutomaticNotes = 0,
        //                        EntryDate = currentTime,
        //                        Notes = "What were the main reasons to change your beverage solution? - " + notes.ToString(),
        //                        Userid = userId,
        //                        UserName = userName,
        //                        IsDispatchNotes = 1
        //                    };
        //                    workOrder.NotesHistories.Add(notesHistory);
        //                }
        //            }
        //        }

        //        if (workorderManagement.RemovalCount > 1 && workorderManagement.RowId.HasValue && workorderManagement.RowId.Value < workorderManagement.WorkOrderEquipmentsRequested.Count())
        //        {
        //            WorkOrderManagementEquipmentModel equipmentFromModel = workorderManagement.WorkOrderEquipmentsRequested.ElementAt(workorderManagement.RowId.Value);
        //            IEnumerable<WorkorderEquipmentRequested> workOrderEquipments = context.WorkorderEquipmentRequesteds.Where(we => we.WorkorderId == workOrder.WorkorderId);
        //            if (workOrderEquipments != null)
        //            {
        //                IndexCounterModel counter = Utility.GetIndexCounter("AssetID", workOrderEquipments.Count());
        //                for (int count = 0; count < workorderManagement.RemovalCount - 1; count++)
        //                {
        //                    counter.IndexValue++;
        //                    WorkorderEquipmentRequested equipment = new WorkorderEquipmentRequested()
        //                    {
        //                        Assetid = counter.IndexValue.Value,
        //                        CallTypeid = 1400,
        //                        Category = equipmentFromModel.Category,
        //                        Location = equipmentFromModel.Location,
        //                        SerialNumber = equipmentFromModel.SerialNumber,
        //                        Model = equipmentFromModel.Model,
        //                        CatalogId = equipmentFromModel.CatelogID,
        //                        Symptomid = equipmentFromModel.SymptomID
        //                    };
        //                    workOrder.WorkorderEquipmentRequesteds.Add(equipment);
        //                }
        //                //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
        //            }
        //        }
        //    }
        //}

    }
}
