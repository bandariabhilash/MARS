using FBCall.Models;
using NewcoCall.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace NewcoCall.Controllers
{
    public class WorkorderController : Controller
    {
        int defaultFollowUpCall;
        string SubmittedBy = "";
        public WorkorderController()
        {
            using (NewcoEntity s = new NewcoEntity())
            {
                AllFBStatu FarmarBortherStatus = s.AllFBStatus.Where(a => a.FBStatus == "None" && a.StatusFor == "Follow Up Call").FirstOrDefault();
                if (FarmarBortherStatus != null)
                {
                    defaultFollowUpCall = FarmarBortherStatus.FBStatusID;
                }
            }
        }

        [HttpPost]
        public JsonResult SaveWorkorder(CustomerServiceModel CustomerDetails)
        {
            int returnValue = 0;
            WorkOrder workOrder = null;
            string message = string.Empty;
            bool isValid = true;
            WorkorderResultModel wrc = new WorkorderResultModel();

            if (string.IsNullOrEmpty(CustomerDetails.PostalCode))
            {
                message = @"|Please Enter Valid Customer ZipCode and Update Customer details!";
                returnValue = -1;
                isValid = false;
            }

            if (string.IsNullOrEmpty(CustomerDetails.MainContactName) || (CustomerDetails.MainContactName != null && string.IsNullOrEmpty(CustomerDetails.MainContactName.Trim())))
            {
                message = @"|Please Enter Onsite Caller Name!";
                returnValue = -1;
                isValid = false;
            }


            if (string.IsNullOrEmpty(CustomerDetails.PhoneNumber) || (CustomerDetails.PhoneNumber != null && string.IsNullOrEmpty(CustomerDetails.PhoneNumber.Trim())))
            {
                message = @"|Please Enter CallBack Number!";
                returnValue = -1;
                isValid = false;
            }            

            if(string.IsNullOrEmpty(CustomerDetails.NewcoCustomerNumber) && string.IsNullOrEmpty(CustomerDetails.PaymentTransactionId))
            {
                message = @"|Payment Process is Incomplete!";
                returnValue = -1;
                isValid = false;
            }

            if(!string.IsNullOrEmpty(CustomerDetails.NewcoCustomerNumber) && CustomerDetails.PaymentTerm.ToLower() == "credit card" && string.IsNullOrEmpty(CustomerDetails.PaymentTransactionId))
            {
                message = @"|Payment Process is Incomplete!";
                returnValue = -1;
                isValid = false;
            }

            if (isValid == true)
            {
                using (NewcoEntity newcoEntity = new NewcoEntity())
                {
                    CustomerModel customerdata = new CustomerModel();
                    customerdata.CustomerId = CustomerDetails.AccountNumber.ToString();
                    customerdata.CustomerName = CustomerDetails.CustomerName;
                    customerdata.Address = CustomerDetails.Address1;
                    customerdata.Address2 = CustomerDetails.Address2;
                    customerdata.City = CustomerDetails.City;
                    customerdata.State = CustomerDetails.State;
                    customerdata.ZipCode = CustomerDetails.PostalCode;
                    customerdata.MainContactName = CustomerDetails.MainContactName;
                    customerdata.PhoneNumber = CustomerDetails.PhoneNumber;
                    customerdata.MainEmailAddress = CustomerDetails.Email;
                    customerdata.PaymentTransactionId = CustomerDetails.PaymentTransactionId;
                    customerdata.ServiceType = CustomerDetails.ServiceType == "0" ? 1200 : Convert.ToInt32(CustomerDetails.ServiceType);
                    customerdata.Comments = CustomerDetails.Comments;
                    customerdata.ServiceRequestedPartyName = CustomerDetails.ServiceRequestedPartyName;
                    customerdata.ServiceRequestedPartyPhone = CustomerDetails.ServiceRequestedPartyPhone;
                    customerdata.EqpBrand = CustomerDetails.EqpBrand;
                    customerdata.EqpModel = CustomerDetails.EqpModel;
                    customerdata.PaymentTermDesc = CustomerDetails.PaymentTerm;
                    customerdata.DateNeeded = CustomerDetails.DateNeeded;

                    NonFBCustomer nonFBCust = newcoEntity.NonFBCustomers.Where(f => f.NonFBCustomerName.ToLower() == "newprotect").FirstOrDefault();
                    if (nonFBCust != null)
                    {
                        customerdata.ParentNumber = nonFBCust.NonFBCustomerId;
                    }

                    returnValue = WorkOrderSave(customerdata, newcoEntity, out workOrder, out message);

                    if (CustomerDetails.AccountNumber == 0)
                    {
                        CustomerModel cust = new CustomerModel();

                        cust.CreateUnknownCustomer(customerdata, newcoEntity);
                        workOrder.CustomerID = Convert.ToInt32(customerdata.CustomerId);
                        newcoEntity.SaveChanges();
                    }

                    if (returnValue == 1)
                    {   
                        message = "Workorder Created Successfully " + workOrder.WorkorderID;                        
                    }

                }
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = message, data = wrc };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public int WorkOrderSave(CustomerModel customerdata, NewcoEntity newcoEntity, out WorkOrder workOrder, out string message)
        {
            int returnValue = 0;
            message = string.Empty;
            workOrder = null;
            WorkorderManagementModel workorderManagement = new WorkorderManagementModel();

            //WorkorderType WOTyp = newcoEntity.WorkorderTypes.Where(w => w.CallTypeID == customerdata.ServiceType).FirstOrDefault();

            workorderManagement.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
            WorkOrderManagementEquipmentModel eqp = new WorkOrderManagementEquipmentModel();
            eqp.CallTypeID = 1200;
            eqp.Category = "N/A";
            eqp.Location = "N/A";

            workorderManagement.WorkOrderEquipmentsRequested.Add(eqp);

            var CustomerId = int.Parse(customerdata.CustomerId);
            Contact serviceCustomer = newcoEntity.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();

            workOrder = workorderManagement.FillCustomerData(new WorkOrder(), true, newcoEntity, customerdata, serviceCustomer);


            IndexCounter counter = Utility.GetIndexCounter("WorkorderID", 1);
            counter.IndexValue++;
            newcoEntity.Entry(counter).State = System.Data.Entity.EntityState.Modified;            

            workOrder.WorkorderID = counter.IndexValue.Value;
            workOrder.WorkorderContactName = customerdata.MainContactName;
            workOrder.WorkorderContactPhone = customerdata.PhoneNumber;
            workOrder.CallerName = customerdata.ServiceRequestedPartyName;
            workOrder.CallerPhone = customerdata.ServiceRequestedPartyPhone;
            workOrder.EquipmentBrand = customerdata.EqpBrand;
            workOrder.EquipmentModel = customerdata.EqpModel;
            workOrder.PaymentTerm = customerdata.PaymentTermDesc;
            workOrder.AppointmentDate = customerdata.DateNeeded;

            workOrder.WorkorderCalltypeid = 1200;
            workOrder.WorkorderCalltypeDesc = "Service";
            workOrder.WorkorderErfid = workorderManagement.WorkOrder.WorkorderErfid;
            workOrder.WorkorderEquipCount = Convert.ToInt16(workorderManagement.WorkOrderEquipmentsRequested.Count());

            AllFBStatu priority = newcoEntity.AllFBStatus.Where(p => p.FBStatus.ToLower() == "next day service").FirstOrDefault();
            int priorityStatusId = 52;

            workOrder.PriorityCode = priorityStatusId;

            workOrder.FollowupCallID = defaultFollowUpCall;

            DateTime CurrentTime = Utility.GetCurrentTime(customerdata.ZipCode, newcoEntity);
            workOrder.WorkorderEntryDate = CurrentTime;
            workOrder.WorkorderModifiedDate = CurrentTime;
            workOrder.ModifiedUserName = "Newco WEB";
            workOrder.IsAutoGenerated = true;
            workOrder.EntryUserName = "Newco WEB";

            workOrder.WorkorderModifiedDate = CurrentTime;
            workOrder.WorkorderCallstatus = "Open";
            workOrder.FinalTransactionId = customerdata.PaymentTransactionId;


            NotesHistory customerNotes = new NotesHistory()
            {
                AutomaticNotes = 0,
                EntryDate = CurrentTime,
                Notes = customerdata.Comments,
                Userid = 99999,
                UserName = "Newco WEB",
                WorkorderID = workOrder.WorkorderID
            };
            newcoEntity.NotesHistories.Add(customerNotes);


            {
                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = CurrentTime,
                    Notes = "Work Order created from Newco Web, WO#: " + workOrder.WorkorderID + " !",
                    Userid = 99999,
                    UserName = "Newco WEB",
                    WorkorderID = workOrder.WorkorderID
            };
                workOrder.NotesHistories.Add(notesHistory);

                workorderManagement.NewNotes = new List<NewNotesModel>();
                //NewNotesModel nts = new NewNotesModel();
                //nts.Text = "Event created from Newco Web";
                //workorderManagement.NewNotes.Add(nts);

                foreach (NewNotesModel newNotesModel in workorderManagement.NewNotes)
                {
                    NotesHistory newnotesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = CurrentTime,
                        Notes = newNotesModel.Text,
                        Userid = 99999,
                        UserName = "Newco WEB",
                        WorkorderID = workOrder.WorkorderID
                    };
                    newcoEntity.NotesHistories.Add(newnotesHistory);
                }
                workorderManagement.Notes = new NotesModel();
                if (workorderManagement.Notes.TechID != null && workorderManagement.Notes.TechID != "-1")
                {
                    workOrder.SpecificTechnician = workorderManagement.Notes.TechID;
                }

                workOrder.IsSpecificTechnician = workorderManagement.Notes.IsSpecificTechnician;
                workOrder.IsAutoDispatched = workorderManagement.Notes.IsAutoDispatched;

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
                    newBrand.WorkorderID = workOrder.WorkorderID;
                    workOrder.WorkOrderBrands.Add(newBrand);
                }

                IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
                assetCounter.IndexValue++;
                newcoEntity.Entry(assetCounter).State = System.Data.Entity.EntityState.Modified;

                WorkorderEquipment equipment = new WorkorderEquipment()
                {
                    Assetid = assetCounter.IndexValue.Value,
                    CallTypeid = 1200,
                    Category = ".11 - No Info – Only OTHER",
                    Location = workorderManagement.WorkOrder.ClosedUserName
                };
                workOrder.WorkorderEquipments.Add(equipment);

                WorkorderEquipmentRequested equipmentReq = new WorkorderEquipmentRequested()
                {
                    Assetid = assetCounter.IndexValue.Value,
                    CallTypeid = 1200,
                    Category = ".11 - No Info – Only OTHER",
                    Location = workorderManagement.WorkOrder.ClosedUserName
                };
                workOrder.WorkorderEquipmentRequesteds.Add(equipmentReq);

                notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = workOrder.WorkorderEntryDate,
                    Notes = workOrder.WorkorderCalltypeDesc + " Work Order # " + workOrder.WorkorderID + " in Newco!",
                    Userid = 99999,
                    UserName = SubmittedBy
                };
                workOrder.NotesHistories.Add(notesHistory);
                if (workorderManagement.Erf != null)
                {
                    workOrder.WorkorderErfid = workorderManagement.Erf.ErfID;
                }

            }


            if (workorderManagement.RemovalCount > 5)
            {
                workOrder.WorkorderCallstatus = "Open";
            }

            newcoEntity.WorkOrders.Add(workOrder);
            SaveRemovalDetails(workorderManagement, workOrder, newcoEntity);


            WorkorderDetail workOrderDetail = new WorkorderDetail()
            {
                WorkorderID = workOrder.WorkorderID,
                EntryDate = workOrder.WorkorderEntryDate,
                ModifiedDate = workOrder.WorkorderEntryDate,
                SpecialClosure = null,
            };

            newcoEntity.WorkorderDetails.Add(workOrderDetail);
            workOrder.CurrentUserName = "Newco WEB";

            int effectedRecords = newcoEntity.SaveChanges();
            returnValue = effectedRecords > 0 ? 1 : 0;

            return returnValue;
        }

        private void SaveRemovalDetails(WorkorderManagementModel workorderManagement, WorkOrder workOrder, NewcoEntity newcoEntity)
        {
            if (workorderManagement.RemovalCount > 0)
            {
                AllFBStatu status = newcoEntity.AllFBStatus.Where(a => a.FBStatusID == workorderManagement.RemovalReason).FirstOrDefault();
                RemovalSurvey survey = newcoEntity.RemovalSurveys.Where(r => r.WorkorderID == workOrder.WorkorderID).FirstOrDefault();
                if (survey != null)
                {
                    survey.JMSOwnedMachines = workorderManagement.RemovalCount;
                    survey.RemovalDate = workorderManagement.RemovalDate;
                    if (status != null)
                    {
                        survey.RemovalReason = status.FBStatus;
                    }
                    survey.RemoveAllMachines = workorderManagement.RemovaAll.ToString();
                    survey.BeveragesSupplier = workorderManagement.BeveragesSupplier;
                }
                else
                {
                    RemovalSurvey newSurvey = new RemovalSurvey()
                    {
                        BeveragesSupplier = workorderManagement.BeveragesSupplier,
                        JMSOwnedMachines = workorderManagement.RemovalCount,
                        RemovalDate = workorderManagement.RemovalDate,
                        RemoveAllMachines = workorderManagement.RemovaAll.ToString(),
                        WorkorderID = workOrder.WorkorderID,
                        RemovalReason = status.FBStatus
                    };
                    newcoEntity.RemovalSurveys.Add(newSurvey);

                    TimeZoneInfo newTimeZoneInfo = null;
                    Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, newcoEntity);
                    DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, newcoEntity);

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = currentTime,
                        Notes = "How many Smucker owned machines will we be removing? - " + workorderManagement.RemovalCount,
                        Userid = 99999,
                        UserName = SubmittedBy//"WEB"
                    };
                    workOrder.NotesHistories.Add(notesHistory);

                    if (workorderManagement.RemovalDate.HasValue)
                    {
                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "What date will you need these machines removed by? - " + workorderManagement.RemovalDate.Value.ToString("MM/dd/yyyy"),
                            Userid = 99999,
                            UserName = SubmittedBy//"WEB"
                        };
                        workOrder.NotesHistories.Add(notesHistory);
                    }

                    notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = currentTime,
                        Notes = "Are we removing all machines from your facility? - " + workorderManagement.RemovaAll.ToString(),
                        Userid = 99999,
                        UserName = SubmittedBy//"WEB"
                    };
                    workOrder.NotesHistories.Add(notesHistory);

                    if (workorderManagement.RemovaAll)
                    {

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "May I ask the reason you have chosen to remove our machines from your location? - " + status.FBStatus,
                            Userid = 99999,
                            UserName = SubmittedBy//"WEB"
                        };
                        workOrder.NotesHistories.Add(notesHistory);

                        if (!string.IsNullOrWhiteSpace(workorderManagement.BeveragesSupplier))
                        {
                            notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 0,
                                EntryDate = currentTime,
                                Notes = "Who will be supplying your beverages going forward? - " + workorderManagement.BeveragesSupplier,
                                Userid = 99999,
                                UserName = SubmittedBy//"WEB"
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
                                Userid = 99999,
                                UserName = SubmittedBy//"WEB"
                            };
                            workOrder.NotesHistories.Add(notesHistory);
                        }
                    }
                }

                if (workorderManagement.RemovalCount > 1 && workorderManagement.RowId.HasValue && workorderManagement.RowId.Value < workorderManagement.WorkOrderEquipmentsRequested.Count())
                {
                    WorkOrderManagementEquipmentModel equipmentFromModel = workorderManagement.WorkOrderEquipmentsRequested.ElementAt(workorderManagement.RowId.Value);
                    IEnumerable<WorkorderEquipmentRequested> workOrderEquipments = newcoEntity.WorkorderEquipmentRequesteds.Where(we => we.WorkorderID == workOrder.WorkorderID);
                    if (workOrderEquipments != null)
                    {
                        IndexCounter counter = Utility.GetIndexCounter("AssetID", 1);
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
                                CatalogID = equipmentFromModel.CatelogID,
                                Symptomid = equipmentFromModel.SymptomID
                            };
                            workOrder.WorkorderEquipmentRequesteds.Add(equipment);
                        }
                        newcoEntity.Entry(counter).State = System.Data.Entity.EntityState.Modified;
                    }
                }
            }
        }

        public JsonResult GetServiceQuote(string ZipCode, string State, string ServiceType)
        {
            JsonResult jsonResult = new JsonResult();
            ServiceQuote quote = new ServiceQuote();
            int flag = 0;
            string message = "";

            List<ServiceRates>  ServiceRates = new List<ServiceRates>() {
            new ServiceRates()
            {
                CallTypeId = 1100,
                Amount = 300  //Flat Rate
            },
            new ServiceRates()
            {
                CallTypeId = 1200,
                Amount = 125 //Per Hour
            },
            new ServiceRates()
            {
                CallTypeId = 1300,
                Amount = 125 //Per Hour
            },
            new ServiceRates()
            {
                CallTypeId = 1600,
                Amount = 240  //Flat Rate
            }
            };

            using (NewcoEntity newcoEntity = new NewcoEntity())
            {
                int ClosestTechId = GetClosestTech(ZipCode, newcoEntity);

                TECH_HIERARCHY techHView = newcoEntity.TECH_HIERARCHY.Where(t => t.DealerId == ClosestTechId).FirstOrDefault();
                if (techHView != null)
                {
                    dynamic travelDetails = Utility.GetTravelDetailsBetweenZipCodes(techHView.PostalCode, ZipCode);

                    NonFBCustomer nonFBCust = newcoEntity.NonFBCustomers.Where(f => f.NonFBCustomerName.ToLower() == "newco").FirstOrDefault();
                    string ParentId = "0";
                    if (nonFBCust != null)
                    {
                        ParentId = nonFBCust.NonFBCustomerId;
                    }

                    PricingDetail priceDtls = Utility.GetPricingDetails(ParentId, ClosestTechId, State, newcoEntity); //For UnknownCustomer CustomerId is 0

                    decimal travelRateDefined = Convert.ToDecimal(priceDtls.HourlyTravlRate);

                    //decimal laborRateDefined = Convert.ToDecimal(priceDtls.HourlyLablrRate);
                    ServiceRates currentServiceRate = ServiceRates.Where(s => s.CallTypeId.ToString() == ServiceType).FirstOrDefault();
                    decimal laborRateDefined = 0;
                    if (currentServiceRate != null)
                    {
                        laborRateDefined = Convert.ToDecimal(currentServiceRate.Amount);
                    }

                    if (travelDetails != null)
                    {
                        var element = travelDetails.rows[0].elements[0];
                        decimal distance = element == null ? 0 : (element.distance == null ? 0 : element.distance.value * (decimal)0.000621371192);
                        distance = Math.Round(distance, 2);
                        decimal travelTime = element == null ? Convert.ToDecimal(1800.00 / 3600.00) : (element.duration == null ? Convert.ToDecimal(1800.00 / 3600.00) : element.duration.value / 3600.00);
                        decimal duration = Math.Round(travelTime, 2);

                        decimal TravelHour = Convert.ToDecimal(Math.Truncate(duration));
                        decimal TravelMinutes = (Convert.ToDecimal(travelTime) - (TravelHour)) * Convert.ToDecimal(60.00);
                        
                        quote.TravelDistance = distance + " Miles";
                        quote.Distance = distance.ToString();
                        quote.TravelTime = "Hr " + Math.Abs(TravelHour) + ":" + Math.Round(TravelMinutes) + " Minutes";

                        int lablrHours = 1;
                        quote.LaborHours = "Hr " + lablrHours + ":00" + " Minutes";
                        if (ServiceType == "1100" || ServiceType == "1600") //Flat Rate
                        {
                            quote.Labor = laborRateDefined;
                        }
                        if (ServiceType == "1200" || ServiceType == "1300") // Per Hour Calculation
                        {
                            quote.Labor = lablrHours * laborRateDefined;
                        }
                        decimal TravelAmount = duration * travelRateDefined;
                        quote.TravelAmount = TravelAmount;

                        decimal total = TravelAmount + laborRateDefined;
                        quote.TotalServiceQuote = Math.Round(total, 2).ToString(); ;//string.Format("{0:c2}", total);

                        flag = 1;
                    }
                    else
                    {
                        message = "Problem calulating the Distance!";
                    }
                }
                
                message = "No Tech Available!";
            }

            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = quote, message = message, flag=flag };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }


        private int GetClosestTech(string postCode, NewcoEntity newcoEntity)
        {
            int TechID = -1;
            DateTime currentTime = Utility.GetCurrentTime(postCode, newcoEntity);
            TechID = getAvailableTechId(postCode, currentTime, newcoEntity);


            return TechID;
        }


        private int getAvailableTechId(string PostalCode, DateTime currentTime, NewcoEntity newcoEntity)
        {
            int availableTechId = 0;
            int replaceTechId = 0;

            DataTable rsReferralList = null;
            FindAvailableDealers(PostalCode, false, out rsReferralList);

            //Check for Internal Techs
            foreach (DataRow dr in rsReferralList.Rows)
            {
                string techType = dr["TechType"].ToString();
                if (techType.ToUpper() != "FB") continue;

                int techId = Convert.ToInt32(dr["DealerID"]);
                bool IsUnavailable = IsTechUnAvailable(techId, currentTime, newcoEntity, out replaceTechId);

                if (!IsUnavailable)
                {
                    if (replaceTechId != 0)
                    {
                        TECH_HIERARCHY THV = newcoEntity.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                        if (THV != null)
                        {
                            availableTechId = replaceTechId;
                            break;
                        }
                    }
                    else
                    {
                        TECH_HIERARCHY THV = newcoEntity.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == techId).FirstOrDefault();

                        if (THV != null)
                        {
                            availableTechId = techId;
                            break;
                        }
                    }
                }
            }

            if (availableTechId == 0)
            {
                //Check for thirdParty Techs
                foreach (DataRow dr in rsReferralList.Rows)
                {
                    string techType = dr["TechType"].ToString();
                    if (techType.ToUpper() == "FB") continue;

                    int techId = Convert.ToInt32(dr["DealerID"]);
                    bool IsUnavailable = IsTechUnAvailable(techId, currentTime, newcoEntity, out replaceTechId);

                    if (!IsUnavailable)
                    {
                        if (replaceTechId != 0)
                        {
                            TECH_HIERARCHY THV = newcoEntity.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == replaceTechId).FirstOrDefault();

                            if (THV != null)
                            {
                                availableTechId = replaceTechId;
                                break;
                            }
                        }
                        else
                        {
                            TECH_HIERARCHY THV = newcoEntity.TECH_HIERARCHY.Where(x => x.SearchType == "SP" && x.DealerId == techId).FirstOrDefault();

                            if (THV != null)
                            {
                                availableTechId = techId;
                                break;
                            }
                        }

                    }
                }
            }

            return availableTechId;
        }

        public bool IsTechUnAvailable(int techId, DateTime StartTime, NewcoEntity newcoEntity, out int replaceTech)
        {
            bool isAvilable = false;
            replaceTech = techId;


            List<TechSchedule> holidays = (from sc in newcoEntity.TechSchedules
                                           join tech in newcoEntity.TECH_HIERARCHY on sc.TechId equals tech.DealerId
                                           where DbFunctions.TruncateTime(sc.ScheduleDate) == DbFunctions.TruncateTime(StartTime) && sc.TechId == techId
                                           && tech.SearchType == "SP" && tech.PostalCode != null
                                           select sc).ToList();

            if (holidays != null)
            {
                foreach (TechSchedule holiday in holidays)
                {
                    DateTime UnavailableStartDate = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + new DateTime().AddHours(Convert.ToDouble(holiday.ScheduleStartTime)).ToString("hh:mm tt"));
                    DateTime UnavailableEndDate = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + new DateTime().AddHours(Convert.ToDouble(holiday.ScheduleEndTime)).ToString("hh:mm tt"));

                    if ((UnavailableStartDate <= StartTime) && (UnavailableEndDate > StartTime))
                    {
                        if (holiday.ReplaceTech != null && holiday.ReplaceTech != 0)
                        {
                            replaceTech = Convert.ToInt32(holiday.ReplaceTech);
                            IsTechUnAvailable(replaceTech, StartTime, newcoEntity, out replaceTech);
                        }
                        else
                        { return true; }
                    }
                    else
                    {
                        isAvilable = false;
                    }
                }
            }

            return isAvilable;
        }

        public bool FindAvailableDealers(string sPostalCode, bool bDefaultDealer, out DataTable rsReferralList)
        {
            SqlHelper helper = new SqlHelper();
            string sSQL;
            DataTable rsHierarchy;
            string sTableName;
            bool bFinished;
            long lReferralID;
            double dDistance;
            DataTable rsLatitudeLongitude;
            string sDealerLatLongFactor;
            double dDealerLatLongFactor;
            double dLatitude;
            double dLongitude;
            double dDealerLatitude;
            double dDealerLongitude;

            rsReferralList = new DataTable();


            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            bool FindAvailableDealers = true;
            bDefaultDealer = false;
            dLatitude = -1;
            dLongitude = -1;
            dDealerLatLongFactor = 2.5;

            if (GetPreference("ReferralLatLongDegrees", out sDealerLatLongFactor))
            {
                dDealerLatLongFactor = double.Parse(sDealerLatLongFactor);
            }
            else
            {
                dDealerLatLongFactor = 2.5;
            }


            if (ReferByAvailableDealersDistance(sPostalCode, dDealerLatLongFactor, out rsReferralList))
            {
                // no errors - check if any referrals were found
                if ((rsReferralList.Rows.Count > 0))
                {
                    bFinished = true;
                }

            }
            else
            {
                // error occurred - exit now
                FindAvailableDealers = false;
                bFinished = true;
            }


            return FindAvailableDealers;
        }

        // ******************************************************************
        // * Description:         Distance-based referral.
        // ******************************************************************
        private bool ReferByAvailableDealersDistance(string customerzipCode, double dDealerLatLongFactor, out DataTable rsReferralList)
        {
            SqlHelper helper = new SqlHelper();
            bool IsTechDetailsExist = false;

            rsReferralList = new DataTable();


            rsReferralList = helper.GetTechDispatchDetails(customerzipCode, dDealerLatLongFactor);
            if (rsReferralList.Rows.Count > 0)
            {
                IsTechDetailsExist = true;
            }
            return IsTechDetailsExist;
        }

        // *********************************************************************************
        // * Description:         Retrieves a record from the Preference table.
        // *********************************************************************************
        public bool GetPreference(string sPreferenceName, out string sPreferenceValue)
        {
            // Define necessary variables
            bool IsPreferenceExist = false;
            sPreferenceValue = string.Empty;
            string ReferralLatLongDegrees = ConfigurationManager.AppSettings[sPreferenceName];
            if (String.IsNullOrEmpty(ReferralLatLongDegrees))
            {
            }
            else
            {
                IsPreferenceExist = true;
                sPreferenceValue = ReferralLatLongDegrees;
            }
            return IsPreferenceExist;
        }

        public JsonResult ProcessBillPayment(string sourceToken, decimal Amount, bool Capture)
        {
            JsonResult jsonResult = new JsonResult();
            string paymentTransactionId = "";

            string autohToken = ConfigurationManager.AppSettings["CloverAuthToken"];
            string accessKey = ConfigurationManager.AppSettings["CloverAccessKey"];

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                                               | SecurityProtocolType.Tls11
                                                                               | SecurityProtocolType.Tls12
                                                                               | SecurityProtocolType.Ssl3
                                                                               | (SecurityProtocolType)3072;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["CloverBaseUrl"]);
                //client.BaseAddress = new Uri("https://scl.clover.com");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autohToken);
                var guid = Guid.NewGuid().ToString();
                client.DefaultRequestHeaders.Add("Idempotency-key", guid);

                Amount = Amount * 100; //API accepts the value in cents, to capture in dollars, need to multiply with 100
                var dataObj = new
                {
                    amount = Convert.ToDecimal(Amount.ToString("F")),
                    currency = "usd",
                    capture = Capture,
                    source = sourceToken
                };

                var JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(dataObj);

                var content = new StringContent(JSONString.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync("v1/charges", content).Result;


                if (response.IsSuccessStatusCode)
                {
                    dynamic jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    paymentTransactionId = jObject["id"].ToString();
                }
            }

            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = paymentTransactionId };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }


        //Not In Use: Sending Emails from Autodispatch Process
        public bool SendMail(int WorkOrderID, NewcoEntity newcoEntity)
        {
            WorkOrder wo = newcoEntity.WorkOrders.Where(w => w.WorkorderID == WorkOrderID).FirstOrDefault();
            DateTime currentTime = Utility.GetCurrentTime(wo.CustomerZipCode, newcoEntity);

            string fromAddress = "reviveservice@mktalt.com"; 

            int cid = Convert.ToInt32(wo.CustomerID);
            Contact customer = newcoEntity.Contacts.Where(c => c.ContactID == cid).FirstOrDefault();
            
            string mailTo = string.Empty;
            string ccTo = string.Empty;
            string mailToName = string.Empty;
            string NotesMsg = "";

            IDictionary<string, string> mailToUserIds = new Dictionary<string, string>();

            mailTo += ConfigurationManager.AppSettings["supportEmailId"].ToString();
                mailToName += "Newco Protect - " + ConfigurationManager.AppSettings["supportEmailId"].ToString() + ";    ";

                NotesMsg += "Newco Event Create Mail sent to " + mailToName;
           

            NotesHistory notesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = currentTime,
                Notes = NotesMsg,
                Userid = 99999,
                UserName = "Newco WEB",
                isDispatchNotes = 1
            };
            notesHistory.WorkorderID = wo.WorkorderID;
            wo.NotesHistories.Add(notesHistory);            
            newcoEntity.SaveChanges();

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
            {
                mailTo = ConfigurationManager.AppSettings["TestEmail"];

                string[] emailIds = mailTo.Split(';');
                foreach (string em in emailIds)
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
                           

                           
                            StringBuilder salesEmailBody = new StringBuilder();

                            string mailToUserName = mailToUserIds[address];

                            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");

                           

                            #region customer details

                            salesEmailBody.Append("<table>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Customer Details:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("AccountNumber:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(wo.CustomerID);
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

                            salesEmailBody.Append("<table>");
                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Workorder Details:");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Workorder#");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(wo.WorkorderID);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Payment Type");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(wo.PaymentTerm);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Equipment Brand");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(wo.EquipmentBrand);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Equipment Model");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(wo.EquipmentModel);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Name of Person Submitting Service Request");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(wo.CallerName);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Submitting Party’s Phone Number");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(wo.CallerPhone);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Onsite Contact Name");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(wo.WorkorderContactName);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Onsite Contact’s Phone Number");
                            salesEmailBody.Append("</b></td>");
                            salesEmailBody.Append("<td>");
                            salesEmailBody.Append(wo.WorkorderContactPhone);
                            salesEmailBody.Append("</td>");
                            salesEmailBody.Append("</tr>");

                            salesEmailBody.Append("</table>");

                            salesEmailBody.Append("<BR>");
                            salesEmailBody.Append("<BR>");


                            #region Notes

                            salesEmailBody.Append("<table>");
                            salesEmailBody.Append("<tr>");
                            salesEmailBody.Append("<td><b>");
                            salesEmailBody.Append("Notes");
                            salesEmailBody.Append("<b></td>");


                            IEnumerable<NotesHistory> histories = newcoEntity.NotesHistories.Where(w => w.WorkorderID == WorkOrderID).OrderByDescending(n => n.EntryDate);

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

                            string contentId = Guid.NewGuid().ToString();
                            string logoPath = Path.Combine(HttpRuntime.AppDomainAppPath, "images/Newco.png");

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

                            message.Subject = "Newco Service Event#: " + WorkOrderID;
                            message.IsBodyHtml = true;

                            try
                            {
                                smtp.Send(message);
                            }
                            catch (Exception ex)
                            {
                                result = false;
                            }

                            #endregion
                        }
                    }
                }
            }

           

            return result;
        }

    }
}