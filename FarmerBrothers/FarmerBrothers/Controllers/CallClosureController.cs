using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using FarmerBrothersMailResponse.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Customer = FarmerBrothers.Data.Contact;

namespace FarmerBrothers.Controllers
{
    public class CallClosureController : BaseController
    {
        int defaultFollowUpCall;

        public CallClosureController()
        {
            AllFBStatu FarmarBortherStatus = FarmerBrothersEntitites.AllFBStatus.Where(a => a.FBStatus == "None" && a.StatusFor == "Follow Up Call").FirstOrDefault();
            if (FarmarBortherStatus != null)
            {
                defaultFollowUpCall = FarmarBortherStatus.FBStatusID;
            }
        }
        //
        // GET: /CallCloser/
        public ActionResult CallClosure()
        {
            int techId = System.Web.HttpContext.Current.Session["TechId"] != null ? (int)System.Web.HttpContext.Current.Session["TechId"] : 0;
            List<WorkorderSchedule> workOrderSchedule = new List<WorkorderSchedule>();
            List<CallCloserModel> callCloser = new List<CallCloserModel>();
            //List<int> techIds = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.PrimaryTechId == techId).Select(te => te.DealerId).ToList();

            List<int> techIds = (from th in FarmerBrothersEntitites.TECH_HIERARCHY
                                 where th.DealerId == techId || th.PrimaryTechId == techId
                                 select th.DealerId).ToList();
            foreach (int id in techIds)
            {
                workOrderSchedule = FarmerBrothersEntitites.WorkorderSchedules.Where(wr => wr.Techid == id && ((wr.AssignedStatus == "Sent") || (wr.AssignedStatus == "Accepted") || (wr.AssignedStatus == "Scheduled")))
               .Where(wr => wr.WorkOrder.WorkorderCallstatus == "Pending Acceptance" || wr.WorkOrder.WorkorderCallstatus == "Accepted" || wr.WorkOrder.WorkorderCallstatus == "Completed"
               || wr.WorkOrder.WorkorderCallstatus == "On Site" || wr.WorkOrder.WorkorderCallstatus == "Scheduled").OrderByDescending(d => d.ModifiedScheduleDate).ToList();

                foreach (WorkorderSchedule call in workOrderSchedule)
                {
                    CallCloserModel closer = new CallCloserModel(call, FarmerBrothersEntitites);
                    callCloser.Add(closer);
                }
            }
           
            ViewBag.callClosers = callCloser;
            return View();
        }

        [AllowAnonymous]
        public ActionResult CallClosureManagement(int customerId, int workOrderId, bool isFromProcessCardScreen = false)
        {
            WorkorderManagementModel workOrderManagementModel = ConstructWorkorderManagementModel(customerId, workOrderId, isFromProcessCardScreen);
            return View(workOrderManagementModel);
        }

        WorkorderManagementModel ConstructWorkorderManagementModel(int? customerId, int? workOrderId, bool isFromProcessCardScreen = false)
        {
            WorkorderManagementModel workOrderManagementModel = new WorkorderManagementModel();

            workOrderManagementModel.SKUModel = new FbWorkorderBillableSKUModel();
            IQueryable<string> skus = FarmerBrothersEntitites.FbBillableSKUs.Select(s => s.SKU).Distinct();
            workOrderManagementModel.SKUList = new List<VendorModelModel>();
            foreach (string sku in skus)
            {
                workOrderManagementModel.SKUList.Add(new VendorModelModel(sku));
            }


            workOrderManagementModel.PriorityList = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Priority" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            workOrderManagementModel.BrandNames = FarmerBrothersEntitites.BrandNames.Where(b => b.Active == 1).OrderBy(n => n.BrandName1).ToList();
            workOrderManagementModel.SalesNotificationReasonCodes = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Notify Sales" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            workOrderManagementModel.CallTypes = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.Active == 1).OrderBy(wt => wt.Sequence).ToList();
            workOrderManagementModel.ClosureCallTypes = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.Active == 1).OrderBy(wt => wt.Sequence).ToList();
            workOrderManagementModel.EquipmentTypes = FarmerBrothersEntitites.EquipTypes.OrderBy(e => e.Sequence).ToList();
            workOrderManagementModel.NonSerializedList = new List<WorkOrderManagementNonSerializedModel>();
            workOrderManagementModel.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
            workOrderManagementModel.BillableSKUList = new List<FbWorkorderBillableSKUModel>();
            workOrderManagementModel.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
            workOrderManagementModel.WorkOrderParts = new List<WorkOrderPartModel>();
            workOrderManagementModel.Operation = WorkOrderManagementSubmitType.NONE;
            workOrderManagementModel.States = Utility.GetStates(FarmerBrothersEntitites);
            workOrderManagementModel.Closure = new WorkOrderClosureModel();
            workOrderManagementModel.Closure.CustomerSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.CustomerSignatureDetails).FirstOrDefault();
            workOrderManagementModel.Closure.TechnicianSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.TechnicianSignatureDetails).FirstOrDefault();
            workOrderManagementModel.Closure.PhoneSolveList = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Phone Solve" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            workOrderManagementModel.Closure.PhoneSolveList.Insert(0, new AllFBStatu()
            {
                Active = 1,
                FBStatus = "",
                FBStatusID = -1,
                StatusFor = "Phone Solve",
                StatusSequence = 0
            });

            workOrderManagementModel.Closure.FilterReplaced = Convert.ToBoolean(FarmerBrothersEntitites.Contacts.Where(wr => wr.ContactID == customerId).Select(w => w.FilterReplaced).FirstOrDefault());
            workOrderManagementModel.Closure.WaterTested = Convert.ToBoolean(FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.WaterTested).FirstOrDefault());
            workOrderManagementModel.Closure.HardnessRatingList = new List<string>();
            List<string> ratingsList = new List<string>() { "", "1", "2", "3", "4", "5", "6", "7", "8", "9", "over 10" };

            foreach (string rating in ratingsList)
            {
                workOrderManagementModel.Closure.HardnessRatingList.Add(rating);
            }
            workOrderManagementModel.Closure.HardnessRating = FarmerBrothersEntitites.WorkorderDetails.Where(wr => wr.WorkorderID == workOrderId).Select(w => w.HardnessRating).FirstOrDefault();

            workOrderManagementModel.Closure.WarrentyList = new List<string>();
            workOrderManagementModel.Closure.WarrentyList.Add("");
            workOrderManagementModel.Closure.WarrentyList.Add("YES");
            workOrderManagementModel.Closure.WarrentyList.Add("NO");

            workOrderManagementModel.Closure.WarrentyForList = new List<string>();
            workOrderManagementModel.Closure.WarrentyForList.Add("");
            workOrderManagementModel.Closure.WarrentyForList.Add("Just Parts");
            workOrderManagementModel.Closure.WarrentyForList.Add("Parts and Labor");

            workOrderManagementModel.Closure.AdditionalFollowupList = new List<string>();
            workOrderManagementModel.Closure.AdditionalFollowupList.Add("");
            workOrderManagementModel.Closure.AdditionalFollowupList.Add("YES");
            workOrderManagementModel.Closure.AdditionalFollowupList.Add("NO");

            workOrderManagementModel.Closure.OperationalList = new List<string>();
            workOrderManagementModel.Closure.OperationalList.Add("");
            workOrderManagementModel.Closure.OperationalList.Add("YES");
            workOrderManagementModel.Closure.OperationalList.Add("NO");

            workOrderManagementModel.BranchIds = new List<int>();
            workOrderManagementModel.AssistTechIds = new List<int>();
            workOrderManagementModel.SystemInfoes = FarmerBrothersEntitites.SystemInfoes.Where(s => s.Active == 1).OrderBy(s => s.Sequence).ToList();
            workOrderManagementModel.Solutions = FarmerBrothersEntitites.Solutions.Where(s => s.Active == 1).OrderBy(s => s.SolutionId).ToList();
            workOrderManagementModel.Solutions.Insert(0, new Solution()
            {
                Description = "",
                SolutionId = -1,
                Active = 1,
                Sequence = 0
            });
            workOrderManagementModel.SystemInfoes.Insert(0, new SystemInfo()
            {
                Active = 1,
                Description = "",
                SystemId = 0
            });
            workOrderManagementModel.RemovalReasons = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Removal Reason" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            workOrderManagementModel.AppointmentReasons = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "Appointment Date" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();


            workOrderManagementModel.TaggedCategories = new List<CategoryModel>();
            IQueryable<string> categories = FarmerBrothersEntitites.Categories.Where(s => s.Active == 1).OrderBy(a => a.CategoryId).Select(s => s.CategoryCode + " - " + s.CategoryDesc);
            foreach (string category in categories)
            {
                workOrderManagementModel.TaggedCategories.Add(new CategoryModel(category));
            }

            workOrderManagementModel.Symptoms = FarmerBrothersEntitites.Symptoms.Where(s => s.Active == 1).OrderBy(s => s.Sequence).ToList();
            workOrderManagementModel.Symptoms.Insert(0, new Symptom()
            {
                Description = "",
                SymptomID = 0
            });

            workOrderManagementModel.Amps = FarmerBrothersEntitites.AMPSLists.Where(a => a.Active == 1).OrderBy(a => a.Sequence).ToList();
            workOrderManagementModel.Amps.Insert(0, new AMPSList()
            {
                AMPSDescription = "",
                AMPSID = 0
            });

            workOrderManagementModel.ElectricalPhases = FarmerBrothersEntitites.ElectricalPhaseLists.Where(e => e.Active == 1).OrderBy(e => e.Sequence).ToList();
            workOrderManagementModel.ElectricalPhases.Insert(0, new ElectricalPhaseList()
            {
                ElectricalPhase = "",
                ElectricalPhaseID = 0
            });

            workOrderManagementModel.NmeaNumbers = FarmerBrothersEntitites.NEMANumberLists.Where(n => n.Active == 1).OrderBy(n => n.Sequence).ToList();
            workOrderManagementModel.NmeaNumbers.Insert(0, new NEMANumberList()
            {
                NemaNumberDescription = "",
                NemaNumberID = 0
            });

            workOrderManagementModel.Voltages = FarmerBrothersEntitites.VoltageLists.Where(v => v.Active == 1).OrderBy(v => v.Sequence).ToList();
            workOrderManagementModel.Voltages.Insert(0, new VoltageList()
            {
                Voltage = "",
                VoltageID = 0
            });

            workOrderManagementModel.WaterLines = FarmerBrothersEntitites.WaterLineLists.Where(w => w.Active == 1).OrderBy(w => w.Sequence).ToList();
            workOrderManagementModel.WaterLines.Insert(0, new WaterLineList()
            {
                WaterLine = "",
                WaterLineID = 0
            });

            workOrderManagementModel.YesNoList = new List<YesNoItem>();
            workOrderManagementModel.YesNoList.Add(new YesNoItem()
            {
                Description = "Yes",
                Id = 1
            });
            workOrderManagementModel.YesNoList.Add(new YesNoItem()
            {
                Description = "No",
                Id = 2
            });

            workOrderManagementModel.YesNoList.Insert(0, new YesNoItem()
            {
                Description = "",
                Id = 0
            });

            IQueryable<string> vendors = FarmerBrothersEntitites.Vendors.Where(s => s.VendorActive == 1).Select(s => s.VendorDescription).Distinct();
            workOrderManagementModel.TaggedManufacturer = new List<VendorDataModel>();
            foreach (string vendor in vendors)
            {
                workOrderManagementModel.TaggedManufacturer.Add(new VendorDataModel(vendor));
            }
            workOrderManagementModel.TaggedManufacturer = workOrderManagementModel.TaggedManufacturer.OrderBy(v => v.VendorDescription).ToList();

            workOrderManagementModel.CloserNonTaggedManufacturer = WorkOrderLookup.CloserManufacturer(FarmerBrothersEntitites);

            workOrderManagementModel.CloserPartsOrSKUs = WorkOrderLookup.CloserSKU(FarmerBrothersEntitites);


            workOrderManagementModel.NonTaggedManufacturer = WorkOrderLookup.PartOrderManufacturer(FarmerBrothersEntitites);

            IQueryable<string> models = FarmerBrothersEntitites.FBSKUs.Where(s => s.SKUActive == true).Select(s => s.SKU).Distinct();

            workOrderManagementModel.NonTaggedModels = WorkOrderLookup.PartOrderSKU(FarmerBrothersEntitites);

            workOrderManagementModel.IsOpen = false;
            Customer serviceCustomer = null;
            bool workEqumentAdded = false;
            if (workOrderId.HasValue)
            {
                workOrderManagementModel.WorkOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId.Value).FirstOrDefault();
                workOrderManagementModel.Customer = new CustomerModel(workOrderManagementModel.WorkOrder, FarmerBrothersEntitites);
                workOrderManagementModel.Customer = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, workOrderManagementModel.Customer);

                DateTime CurrentTime = Utility.GetCurrentTime(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                workOrderManagementModel.CurrentDateTime = CurrentTime;

                workOrderManagementModel.Customer.WorkOrderId = workOrderManagementModel.WorkOrder.WorkorderID.ToString();
                WorkorderDetail workOrderDetail = FarmerBrothersEntitites.WorkorderDetails.Where(wd => wd.WorkorderID == workOrderId.Value).FirstOrDefault();
                if (workOrderDetail != null)
                {
                    if (workOrderDetail.SpawnReason.HasValue)
                    {
                        AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(al => al.FBStatusID == workOrderDetail.SpawnReason.Value).FirstOrDefault();
                        if (status != null)
                        {
                            workOrderManagementModel.SpawnReason = status.FBStatus;
                        }
                    }

                    if (workOrderDetail.NSRReason.HasValue)
                    {
                        AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(al => al.FBStatusID == workOrderDetail.NSRReason.Value).FirstOrDefault();
                        if (status != null)
                        {
                            workOrderManagementModel.NSRReason = status.FBStatus;
                        }
                    }


                    if (workOrderDetail.SolutionId.HasValue)
                    {
                        Solution solution = FarmerBrothersEntitites.Solutions.Where(s => s.SolutionId == workOrderDetail.SolutionId.Value).FirstOrDefault();
                        if (solution != null)
                        {
                            workOrderManagementModel.Solution = solution.Description;
                        }
                    }

                    workOrderManagementModel.Closure.StartDateTime = workOrderDetail.StartDateTime;
                    workOrderManagementModel.Closure.ArrivalDateTime = workOrderDetail.ArrivalDateTime;
                    workOrderManagementModel.Closure.CompletionDateTime = workOrderDetail.CompletionDateTime;
                    workOrderManagementModel.Closure.ResponsibleTechName = workOrderDetail.ResponsibleTechName;
                    workOrderManagementModel.Closure.Mileage = workOrderDetail.Mileage;
                    workOrderManagementModel.Closure.InvoiceNo = workOrderDetail.InvoiceNo;
                    workOrderManagementModel.Closure.CustomerSignedBy = workOrderDetail.CustomerSignatureBy;

                    workOrderManagementModel.Closure.WarrentyFor = workOrderDetail.WarrentyFor;
                    workOrderManagementModel.Closure.StateOfEquipment = workOrderDetail.StateofEquipment;
                    workOrderManagementModel.Closure.serviceDelayed = workOrderDetail.ServiceDelayReason;
                    workOrderManagementModel.Closure.troubleshootSteps = workOrderDetail.TroubleshootSteps;
                    workOrderManagementModel.Closure.followupComments = workOrderDetail.FollowupComments;
                    //workOrderManagementModel.Closure.operationalComments = workOrderDetail.OperationalComments;
                    workOrderManagementModel.Closure.ReviewedBy = workOrderDetail.ReviewedBy;
                    workOrderManagementModel.Closure.IsUnderWarrenty = workOrderDetail.IsUnderWarrenty;
                    workOrderManagementModel.Closure.AdditionalFollowup = workOrderDetail.AdditionalFollowupReq;
                    workOrderManagementModel.Closure.Operational = workOrderDetail.IsOperational;

                    workOrderManagementModel.RescheduleReasonCodesList = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "ReScheduleReasonCode" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
                    workOrderManagementModel.RescheduleReasonCodesList.Insert(0, new AllFBStatu()
                    {
                        FBStatusID = 0,
                        FBStatus = "",
                        Active = 1,
                        StatusSequence = 0
                    });
                    workOrderManagementModel.ReasonCode = workOrderManagementModel.WorkOrder.RescheduleReasonCode;

                    if (string.IsNullOrWhiteSpace(workOrderDetail.CustomerName))
                    {
                        workOrderManagementModel.Closure.CustomerName = workOrderManagementModel.WorkOrder.WorkorderContactName;
                    }
                    else
                    {
                        workOrderManagementModel.Closure.CustomerName = workOrderDetail.CustomerName;
                    }
                    workOrderManagementModel.Closure.CustomerEmail = workOrderDetail.CustomerEmail;
                    workOrderManagementModel.Closure.CustomerSignatureDetails = workOrderDetail.CustomerSignatureDetails;
                    workOrderManagementModel.Closure.TechnicianSignatureDetails = workOrderDetail.TechnicianSignatureDetails;
                    workOrderManagementModel.PhoneSolveId = workOrderDetail.PhoneSolveid;
                    workOrderManagementModel.PhoneSolveTechId = workOrderDetail.ResponsibleTechid;
                    workOrderManagementModel.Closure.SpecialClosure = workOrderDetail.SpecialClosure;
                    workOrderManagementModel.Closure.PhoneSolveid = workOrderDetail.PhoneSolveid;
                    
                    if (!string.IsNullOrWhiteSpace(workOrderDetail.TravelTime))
                    {
                        string[] times = workOrderDetail.TravelTime.Split(':');

                        if (times.Count() >= 2)
                        {
                            workOrderManagementModel.Closure.TravelHours = times[0];
                            workOrderManagementModel.Closure.TravelMinutes = times[1];
                        }
                    }
                }

                if (workOrderManagementModel.WorkOrder.PartsRushOrder.HasValue == false)
                {
                    workOrderManagementModel.WorkOrder.PartsRushOrder = false;
                }

                models = FarmerBrothersEntitites.Skus.Where(s => s.SkuActive == 1 && s.EQUIPMENT_TAG == "TAGGED").Select(s => s.Sku1).Distinct();
                workOrderManagementModel.TaggedModels = new List<VendorModelModel>();
                foreach (string model in models)
                {
                    workOrderManagementModel.TaggedModels.Add(new VendorModelModel(model));
                }
                workOrderManagementModel.TaggedModels = workOrderManagementModel.TaggedModels.OrderBy(v => v.Model).ToList();

                IQueryable<WorkorderSchedule> workOrderSchedules = FarmerBrothersEntitites.WorkorderSchedules.Where(ws => ws.WorkorderID == workOrderId);

                int primaryBranchId = 0;
                int secondaryBranchId = 0;

                foreach (WorkorderSchedule workOrderSchedule in workOrderSchedules)
                {
                    if (workOrderSchedule.PrimaryTech > 0)
                    {
                        if (string.Compare(workOrderSchedule.AssignedStatus, "Accepted", true) == 0
                            || string.Compare(workOrderSchedule.AssignedStatus, "Sent", true) == 0)
                        {
                            workOrderManagementModel.ResponsibleTechId = workOrderSchedule.Techid;
                            primaryBranchId = workOrderSchedule.ServiceCenterID.Value;
                        }
                    }
                    else if (workOrderSchedule.AssistTech > 0)
                    {
                        if (string.Compare(workOrderSchedule.AssignedStatus, "Accepted", true) == 0
                            || string.Compare(workOrderSchedule.AssignedStatus, "Sent", true) == 0)
                        {
                            workOrderManagementModel.AssistTechIds.Add(workOrderSchedule.Techid.Value);
                            if (workOrderSchedule.ServiceCenterID.HasValue)
                            {
                                secondaryBranchId = workOrderSchedule.ServiceCenterID.Value;
                            }
                        }
                    }

                    if (workOrderSchedule.ServiceCenterID.HasValue)
                    {
                        workOrderManagementModel.BranchIds.Add(workOrderSchedule.ServiceCenterID.Value);
                    }
                }

                try
                {                    
                    serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == (int)workOrderManagementModel.WorkOrder.CustomerID.Value).FirstOrDefault();
                    if (serviceCustomer != null)
                    {
                        workOrderManagementModel.Customer.CustomerSpecialInstructions = serviceCustomer.CustomerSpecialInstructions;                 
                    }
                }
                catch (Exception e)
                { }
                IList<TechDispatchWithDistance> dispatchBranches = null;
                if (string.IsNullOrWhiteSpace(workOrderManagementModel.Customer.ZipCode))
                {
                    dispatchBranches = new List<TechDispatchWithDistance>();
                }
                else
                {

                    dispatchBranches = Utility.GetTechDispatchWithDistance(FarmerBrothersEntitites, workOrderManagementModel.Customer.ZipCode,workOrderId.Value).ToList();
                }
                IList<BranchModel> branches = new List<BranchModel>();

                foreach (TechDispatchWithDistance dispatchBranch in dispatchBranches)
                {
                    branches.Add(new BranchModel(dispatchBranch));
                }
                workOrderManagementModel.Branches = branches;


                IList<int?> brandIds = FarmerBrothersEntitites.WorkOrderBrands.Where(wb => wb.WorkorderID == workOrderId.Value).Select(wb => wb.BrandID).ToList();
                workOrderManagementModel.SelectedBrandIds = string.Join(",", brandIds);
                workOrderManagementModel.SelectedBrands = FarmerBrothersEntitites.BrandNames.Where(b => brandIds.Contains(b.BrandID)).ToList();

                if (!string.IsNullOrWhiteSpace(workOrderManagementModel.WorkOrder.WorkorderErfid))
                {
                    workOrderManagementModel.Erf = FarmerBrothersEntitites.Erfs.Where(e => e.ErfID == workOrderManagementModel.WorkOrder.WorkorderErfid).FirstOrDefault();
                }

                workOrderManagementModel.SerialNumberList = FarmerBrothersEntitites.FBCBEs.Where(s => s.CurrentCustomerId.ToString() == workOrderManagementModel.WorkOrder.CustomerID.ToString()).ToList();

                //foreach (WorkOrderManagementEquipmentModel epm in workOrderManagementModel.WorkOrderEquipments)
                //{
                //    FBCBE vmm = workOrderManagementModel.SerialNumberList.Where(se => se.SerialNumber == epm.SerialNumber).FirstOrDefault();
                //    if (vmm == null)
                //    {
                //        if (!string.IsNullOrEmpty(epm.SerialNumber))
                //        {
                //            workOrderManagementModel.SerialNumberList.Insert(0, new FBCBE()
                //            {
                //                Id = -1,
                //                SerialNumber = epm.SerialNumber,
                //                ItemNumber = string.IsNullOrEmpty(epm.Model) ? "" : epm.Model,
                //                ItemDescription = string.IsNullOrEmpty(epm.Model) ? "" : epm.Model,
                //            });
                //        }
                //    }
                //}

                workOrderManagementModel.SerialNumberList.Add(new FBCBE()
                {
                    Id = -1,
                    SerialNumber = "Other",
                    ItemNumber = "-1",
                    ItemDescription = " "
                });

                if (workOrderManagementModel.SerialNumberList != null && workOrderManagementModel.SerialNumberList.Count > 0)
                {
                    workOrderManagementModel.SerialNumberList.Insert(0, new FBCBE()
                    {
                        Id = -1,
                        SerialNumber = "",
                        ItemNumber = "-1",
                        ItemDescription = ""
                    });
                }

                if (workOrderManagementModel.WorkOrder.WorkorderCalltypeid == 1300
                    && string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0
                    && string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Completed") != 0
                    && !workOrderManagementModel.WorkOrder.OriginalWorkorderid.HasValue)
                {
                    
                    int sequenceNumber = 1;
                    foreach (WorkorderEquipmentRequested workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipmentRequesteds)
                    {
                        WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);
                        equipmentModel.SequenceNumber = sequenceNumber++;
                        workOrderManagementModel.WorkOrderEquipmentsRequested.Add(equipmentModel);
                    }

                    sequenceNumber = 1;
                    foreach (WorkorderEquipment workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipments)
                    {
                        WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);

                        if (string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0)
                        {
                            FBCBE fbSN = workOrderManagementModel.SerialNumberList.Where(em => em.SerialNumber == equipmentModel.SerialNumber).FirstOrDefault();
                            if (fbSN == null)
                            {
                                equipmentModel.SerialNumber = "Other";
                            }
                            else
                            {
                                equipmentModel.SerialNumberManual = " ";
                            }
                        }

                        equipmentModel.SequenceNumber = sequenceNumber++;
                        workOrderManagementModel.WorkOrderEquipments.Add(equipmentModel);
                        workEqumentAdded = true;
                    }

                    //workOrderManagementModel.TaggedCategories = new List<CategoryModel>();


                    //if (workOrderManagementModel.TaggedCategories != null && workOrderManagementModel.TaggedCategories.Count() > 0)
                    //{
                    //    workOrderManagementModel.TaggedCategories.Add(new CategoryModel("OTHER"));
                    //}
                    //else
                    //{
                    //    CategoryModel category = new CategoryModel("OTHER");
                    //    workOrderManagementModel.TaggedCategories.Add(category);
                    //}
                }
                else
                {
                    IQueryable<NonSerialized> nonSerializedItems = FarmerBrothersEntitites.NonSerializeds.Where(ns => ns.WorkorderID == workOrderId);
                    foreach (NonSerialized nonSerializedItem in nonSerializedItems)
                    {
                        WorkOrderManagementNonSerializedModel nonSerializedModel = new WorkOrderManagementNonSerializedModel()
                        {
                            NSerialid = nonSerializedItem.NSerialid,
                            Catalogid = nonSerializedItem.Catalogid,
                            ManufNumber = nonSerializedItem.ManufNumber,
                            OrigOrderQuantity = nonSerializedItem.OrigOrderQuantity.Value
                        };
                        workOrderManagementModel.NonSerializedList.Add(nonSerializedModel);
                    }

                    int sequenceNumber = 1;
                    foreach (WorkorderEquipmentRequested workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipmentRequesteds)
                    {
                        WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);
                        equipmentModel.SequenceNumber = sequenceNumber++;
                        workOrderManagementModel.WorkOrderEquipmentsRequested.Add(equipmentModel);
                    }

                    sequenceNumber = 1;
                    foreach (WorkorderEquipment workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipments)
                    {
                        WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);
                        if (string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0)
                        {
                            FBCBE fbSN = workOrderManagementModel.SerialNumberList.Where(em => em.SerialNumber == equipmentModel.SerialNumber).FirstOrDefault();
                            if (fbSN == null)
                            {
                                equipmentModel.SerialNumber = "Other";
                            }
                            else
                            {
                                equipmentModel.SerialNumberManual = " ";
                            }
                        }
                        equipmentModel.SequenceNumber = sequenceNumber++;
                        workOrderManagementModel.WorkOrderEquipments.Add(equipmentModel);
                        workEqumentAdded = true;
                    }
                }                

                TempData["WorkOrderEquipments"] = workOrderManagementModel.WorkOrderEquipmentsRequested;
                TempData["NonSerialized"] = workOrderManagementModel.NonSerializedList;

                int count = 1;
                if (!workEqumentAdded)
                {
                    foreach (WorkorderEquipment workOrderEquipment in workOrderManagementModel.WorkOrder.WorkorderEquipments)
                    {
                        WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);
                        if (string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0)
                        {
                            FBCBE fbSN = workOrderManagementModel.SerialNumberList.Where(em => em.SerialNumber == equipmentModel.SerialNumber).FirstOrDefault();
                            if (fbSN == null)
                            {
                                equipmentModel.SerialNumber = "Other";
                            }
                            else
                            {
                                equipmentModel.SerialNumberManual = " ";
                            }
                        }
                        equipmentModel.SequenceNumber = count++;
                        workOrderManagementModel.WorkOrderEquipments.Add(equipmentModel);
                    }
                }
                List<FbWorkOrderSKU> workorderSkus = FarmerBrothersEntitites.FbWorkOrderSKUs.Where(w => w.WorkorderID == workOrderId.Value).ToList();
                workOrderManagementModel.BillableSKUList = new List<FbWorkorderBillableSKUModel>();
                foreach (FbWorkOrderSKU sku in workorderSkus)
                {
                    FbWorkorderBillableSKUModel skuModel = new FbWorkorderBillableSKUModel(sku);
                    workOrderManagementModel.BillableSKUList.Add(skuModel);
                }

                TempData["Billable"] = workOrderManagementModel.BillableSKUList;
                workOrderManagementModel.IsBillableFeed = Convert.ToBoolean(workOrderManagementModel.WorkOrder.IsBillable);

                if (!string.IsNullOrWhiteSpace(workOrderManagementModel.WorkOrder.CurrentUserName) &&
                         string.Compare(workOrderManagementModel.WorkOrder.CurrentUserName.TrimEnd(), UserName.TrimEnd(), true) != 0)
                {

                    DateTime CurrentTime1 = Utility.GetCurrentTime(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                    workOrderManagementModel.CurrentDateTime = CurrentTime1;
                    DateTime WorkorderOpenedTime = Convert.ToDateTime(workOrderManagementModel.WorkOrder.WorkOrderOpenedDateTime);
                    double diffInMinutes = (CurrentTime1 - WorkorderOpenedTime).TotalMinutes;
                    int AllowedTimeToOpenWorkOrderInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["AllowedTimeToOpenWorkOrderInMinutes"]);
                    if (diffInMinutes > AllowedTimeToOpenWorkOrderInMinutes)
                    {
                        workOrderManagementModel.IsOpen = false;
                        workOrderManagementModel.WorkOrder.CurrentUserName = UserName;
                        workOrderManagementModel.WorkOrder.WorkOrderOpenedDateTime = CurrentTime1;
                        FarmerBrothersEntitites.SaveChanges();
                    }
                    else
                    {
                        workOrderManagementModel.IsOpen = true;
                    }
                }
                else
                {
                    workOrderManagementModel.IsOpen = false;
                    workOrderManagementModel.WorkOrder.CurrentUserName = UserName;
                    DateTime CurrentTime1 = Utility.GetCurrentTime(workOrderManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                    workOrderManagementModel.WorkOrder.WorkOrderOpenedDateTime = CurrentTime1;
                    workOrderManagementModel.CurrentDateTime = CurrentTime1;
                    FarmerBrothersEntitites.SaveChanges();
                }


                IEnumerable<WorkorderPart> parts = workOrderManagementModel.WorkOrder.WorkorderParts.Where(wp => wp.AssetID == null || wp.AssetID == 0);
                foreach (WorkorderPart workOrderPart in parts)
                {
                    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);
                    workOrderManagementModel.WorkOrderParts.Add(workOrderPartModel);
                }
                TempData["WorkOrderParts"] = workOrderManagementModel.WorkOrderParts;

                workOrderManagementModel.IsBranchAlternateAddress = false;
                workOrderManagementModel.IsCustomerAlternateAddress = false;

                if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Local Branch", true) == 0)
                {
                    workOrderManagementModel.PartsShipTo = 1;
                }
                else if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Other Local Branch", true) == 0)
                {
                    workOrderManagementModel.PartsShipTo = 1;
                    workOrderManagementModel.IsBranchAlternateAddress = true;

                    workOrderManagementModel.BranchOtherPartsName = workOrderManagementModel.WorkOrder.OtherPartsName;
                    workOrderManagementModel.BranchOtherPartsContactName = workOrderManagementModel.WorkOrder.OtherPartsContactName;
                    workOrderManagementModel.BranchOtherPartsAddress1 = workOrderManagementModel.WorkOrder.OtherPartsAddress1;
                    workOrderManagementModel.BranchOtherPartsAddress2 = workOrderManagementModel.WorkOrder.OtherPartsAddress2;
                    workOrderManagementModel.BranchOtherPartsCity = workOrderManagementModel.WorkOrder.OtherPartsCity;
                    workOrderManagementModel.BranchOtherPartsState = workOrderManagementModel.WorkOrder.OtherPartsState;
                    workOrderManagementModel.BranchOtherPartsZip = workOrderManagementModel.WorkOrder.OtherPartsZip;
                    workOrderManagementModel.BranchOtherPartsPhone = workOrderManagementModel.WorkOrder.OtherPartsPhone;
                }
                else if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Customer", true) == 0)
                {
                    workOrderManagementModel.PartsShipTo = 2;
                }
                else if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Other Customer", true) == 0)
                {
                    workOrderManagementModel.PartsShipTo = 2;
                    workOrderManagementModel.IsCustomerAlternateAddress = true;

                    workOrderManagementModel.CustomerOtherPartsName = workOrderManagementModel.WorkOrder.OtherPartsName;
                    workOrderManagementModel.CustomerOtherPartsContactName = workOrderManagementModel.WorkOrder.OtherPartsContactName;
                    workOrderManagementModel.CustomerOtherPartsAddress1 = workOrderManagementModel.WorkOrder.OtherPartsAddress1;
                    workOrderManagementModel.CustomerOtherPartsAddress2 = workOrderManagementModel.WorkOrder.OtherPartsAddress2;
                    workOrderManagementModel.CustomerOtherPartsCity = workOrderManagementModel.WorkOrder.OtherPartsCity;
                    workOrderManagementModel.CustomerOtherPartsState = workOrderManagementModel.WorkOrder.OtherPartsState;
                    workOrderManagementModel.CustomerOtherPartsZip = workOrderManagementModel.WorkOrder.OtherPartsZip;
                    workOrderManagementModel.CustomerOtherPartsPhone = workOrderManagementModel.WorkOrder.OtherPartsPhone;
                }
                else if (string.Compare(workOrderManagementModel.WorkOrder.PartsShipTo, "Van", true) == 0)
                {
                    workOrderManagementModel.PartsShipTo = 4;
                }


                RemovalSurvey survey = FarmerBrothersEntitites.RemovalSurveys.Where(r => r.WorkorderID == workOrderId.Value).FirstOrDefault();
                if (survey != null)
                {
                    if (survey.JMSOwnedMachines.HasValue)
                    {
                        workOrderManagementModel.RemovalCount = survey.JMSOwnedMachines.Value;
                    }
                }

            }
            else if (customerId.HasValue)
            {
                workOrderManagementModel.WorkOrder = new WorkOrder() { CustomerID = customerId.Value };
                workOrderManagementModel.WorkOrder.EntryUserName = UserName;

                if (workOrderManagementModel.WorkOrder != null && workOrderManagementModel.WorkOrder.CustomerID.HasValue)
                {                    
                    serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == (int)workOrderManagementModel.WorkOrder.CustomerID).FirstOrDefault();
                    if (serviceCustomer != null)
                    {
                        workOrderManagementModel.Customer = new CustomerModel(serviceCustomer, FarmerBrothersEntitites);
                        workOrderManagementModel.Customer = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, workOrderManagementModel.Customer);
                        if (workOrderId.HasValue)
                        {
                            workOrderManagementModel.Customer.WorkOrderId = workOrderId.Value.ToString();
                        }
                        else
                        {
                            workOrderManagementModel.Customer.WorkOrderId = "-1";
                        }
                                                
                    }
                    else
                    {
                        workOrderManagementModel.Customer = new CustomerModel();
                    }
                }

                TempData["WorkOrderEquipments"] = workOrderManagementModel.WorkOrderEquipmentsRequested;
                TempData["NonSerialized"] = workOrderManagementModel.NonSerializedList;
            }

            if (workOrderManagementModel.WorkOrder != null)
            {
                workOrderManagementModel.Closure.PopulateSpecialClosureList(workOrderManagementModel.WorkOrder, FarmerBrothersEntitites);

                WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID
                                                                && (w.AssignedStatus == "Sent" || w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();
                if (ws != null)
                {
                    TECH_HIERARCHY techHView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == ws.Techid).FirstOrDefault();

                    if (techHView != null && techHView.FamilyAff == "SPT" &&
                        (string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0
                       && string.Compare(workOrderManagementModel.WorkOrder.WorkorderCallstatus, "Invoiced") != 0))
                    {
                        List<string> NSRUserIds = ConfigurationManager.AppSettings["TPSPCloseNSRUserIds"].Split(';').ToList();
                        List<string> NSRUserNames = ConfigurationManager.AppSettings["TPSPCloseNSRUserNames"].Split(';').ToList();
                        string NSRUserNm = NSRUserIds.Where(x => x == System.Web.HttpContext.Current.Session["UserId"].ToString()).FirstOrDefault();
                        if (string.IsNullOrEmpty(NSRUserNm))
                        {
                            var item = workOrderManagementModel.Solutions.Single(x => x.SolutionId == 9999);
                            workOrderManagementModel.Solutions.Remove(item);
                        }
                    }
                }
            }

            workOrderManagementModel.IsCustomerPartsOrder = true;
            if (serviceCustomer != null)
            {
                
            }

            if (workOrderManagementModel.WorkOrder == null)
            {
                workOrderManagementModel.WorkOrder = new WorkOrder();
                workOrderManagementModel.WorkOrder.EntryUserName = UserName;
            }
            if (workOrderManagementModel.Customer == null)
            {
                workOrderManagementModel.Customer = new CustomerModel();                
                workOrderManagementModel.WorkOrder.WorkorderCallstatus = "Hold for AB";
            }
                      

            workOrderManagementModel.Notes = new NotesModel()
            {
                CustomerZipCode = workOrderManagementModel.Customer.ZipCode,
                WorkOrderStatus = workOrderManagementModel.WorkOrder.WorkorderCallstatus
            };

            workOrderManagementModel.Notes.NotesHistory = new List<NotesHistoryModel>();
            IQueryable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID && nh.AutomaticNotes == 0).OrderByDescending(nh => nh.EntryDate);

            foreach (NotesHistory notesHistory in notesHistories)
            {
                workOrderManagementModel.Notes.NotesHistory.Add(new NotesHistoryModel(notesHistory));
            }

            IQueryable<NotesHistory> recordHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == workOrderManagementModel.WorkOrder.WorkorderID && nh.AutomaticNotes == 1).OrderByDescending(nh => nh.EntryDate);
            workOrderManagementModel.Notes.RecordHistory = new List<NotesHistoryModel>();
            foreach (NotesHistory recordHistory in recordHistories)
            {
                workOrderManagementModel.Notes.RecordHistory.Add(new NotesHistoryModel(recordHistory));
            }

            workOrderManagementModel.Notes.CustomerNotesResults = new List<CustomerNotesModel>();
            //int? custId = Convert.ToInt32(workOrderManagementModel.Customer.CustomerId);
            //var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();

            int custId = Convert.ToInt32(workOrderManagementModel.Customer.CustomerId);
            int parentId = string.IsNullOrEmpty(workOrderManagementModel.Customer.ParentNumber) ? 0 : Convert.ToInt32(workOrderManagementModel.Customer.ParentNumber);
            var custNotes = Utility.GetCustomreNotes(custId, parentId, FarmerBrothersEntitites);

            foreach (var dbCustNotes in custNotes)
            {
                workOrderManagementModel.Notes.CustomerNotesResults.Add(new CustomerNotesModel(dbCustNotes));
            }

            workOrderManagementModel.Notes.FollowUpRequestList = FarmerBrothersEntitites.AllFBStatus.Where(a => a.StatusFor == "Follow Up Call" && a.Active == 1).ToList();
            workOrderManagementModel.Notes.FollowUpRequestID = workOrderManagementModel.WorkOrder.FollowupCallID.ToString();
            workOrderManagementModel.Notes.WorkOrderID = workOrderManagementModel.WorkOrder.WorkorderID;
            if (workOrderManagementModel.WorkOrder.ProjectFlatRate.HasValue)
            {
                workOrderManagementModel.Notes.ProjectFlatRate = Math.Round(workOrderManagementModel.WorkOrder.ProjectFlatRate.Value, 2);
            }
            if (workOrderManagementModel.WorkOrder.ProjectID.HasValue)
            {
                workOrderManagementModel.Notes.ProjectNumber = workOrderManagementModel.WorkOrder.ProjectID.Value;
            }

            //if (workOrderManagementModel.Customer.ServiceTier == "5" && (!string.IsNullOrEmpty(workOrderManagementModel.Customer.CustomerType) && workOrderManagementModel.Customer.CustomerType.ToLower() != "ce")
            //                && (string.IsNullOrEmpty(workOrderManagementModel.Customer.ParentNumber) || workOrderManagementModel.Customer.ParentNumber == "0"))
            if (!string.IsNullOrEmpty(workOrderManagementModel.Customer.BillingCode) && workOrderManagementModel.Customer.BillingCode.ToLower() == "s08")
            {
                workOrderManagementModel.IsCCProcessComplete = workOrderManagementModel.WorkOrder.FinalTransactionId == null ? false : true;
            }

            workOrderManagementModel.ProcessCardDetails = GetInitialCardData(workOrderManagementModel.WorkOrder.WorkorderID, Convert.ToInt32(workOrderManagementModel.ResponsibleTechId));

            workOrderManagementModel.RedirectFromCardProcess = isFromProcessCardScreen;

            return workOrderManagementModel;
        }


        public ProcessCardModel GetInitialCardData(int workOrderId, int techId)
        {
            WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();

            WorkorderSchedule techWorkOrderSchedule = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrderId && w.Techid == techId).FirstOrDefault();

            ProcessCardModel cardModel = new ProcessCardModel();
            List<FbWorkorderBillableSKUModel> partsList = new List<FbWorkorderBillableSKUModel>();

            var evtPrtList = (from closureSku in FarmerBrothersEntitites.WorkorderParts
                              where closureSku.WorkorderID == workOrderId && (closureSku.AssetID == null || closureSku.AssetID == 0)
                              select new
                              {
                                  sku = closureSku.Sku,
                                  des = closureSku.Description,
                                  qty = closureSku.Quantity,
                                  evtId = closureSku.WorkorderID,
                                  unitcost = closureSku.Total / closureSku.Quantity,
                                  Mnftr = closureSku.Manufacturer
                              }).ToList();


            cardModel.PartsList = new List<FbWorkorderBillableSKUModel>();
            foreach (var wp in evtPrtList)
            {
                FbWorkorderBillableSKUModel fbsm = new FbWorkorderBillableSKUModel();
                fbsm.SKU = wp.sku;
                fbsm.WorkorderID = Convert.ToInt32(wp.evtId);
                fbsm.UnitPrice = wp.unitcost;
                fbsm.Qty = wp.qty;
                fbsm.Description = wp.des;

                cardModel.PartsList.Add(fbsm);
            }

            cardModel.SKUList = DispatchResponseController.CloserSKU(FarmerBrothersEntitites);

            List<BillingItem> blngItmsList = FarmerBrothersEntitites.BillingItems.Where(b => b.IsActive == true).ToList();
            List<CategoryModel> billingItms = new List<CategoryModel>();
            foreach (BillingItem item in blngItmsList)
            {
                billingItms.Add(new CategoryModel(item.BillingName));
            }
            cardModel.BillingItems = billingItms;

            return cardModel;
        }

        public JsonResult GetCardDetails(int workOrderId, int techId)
        {
            WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();

            WorkorderSchedule techWorkOrderSchedule = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrderId && w.Techid == techId).FirstOrDefault();

            ProcessCardModel cardModel = new ProcessCardModel();
            List<FbWorkorderBillableSKUModel> partsList = new List<FbWorkorderBillableSKUModel>();

            var evtPrtList = (from closureSku in FarmerBrothersEntitites.WorkorderParts
                              where closureSku.WorkorderID == workOrderId && (closureSku.AssetID == null || closureSku.AssetID == 0)
                              select new
                              {
                                  sku = closureSku.Sku,
                                  des = closureSku.Description,
                                  qty = closureSku.Quantity,
                                  evtId = closureSku.WorkorderID,
                                  unitcost = closureSku.Total / closureSku.Quantity,
                                  Mnftr = closureSku.Manufacturer
                              }).ToList();


            cardModel.PartsList = new List<FbWorkorderBillableSKUModel>();
            foreach (var wp in evtPrtList)
            {
                FbWorkorderBillableSKUModel fbsm = new FbWorkorderBillableSKUModel();
                fbsm.SKU = wp.sku;
                fbsm.WorkorderID = Convert.ToInt32(wp.evtId);
                fbsm.UnitPrice = wp.unitcost;
                fbsm.Qty = wp.qty;
                fbsm.Description = wp.des;

                cardModel.PartsList.Add(fbsm);
            }

            cardModel.SKUList = DispatchResponseController.CloserSKU(FarmerBrothersEntitites);

            List<BillingItem> blngItmsList = FarmerBrothersEntitites.BillingItems.Where(b => b.IsActive == true).ToList();
            List<CategoryModel> billingItms = new List<CategoryModel>();
            foreach (BillingItem item in blngItmsList)
            {
                billingItms.Add(new CategoryModel(item.BillingName));
            }
            cardModel.BillingItems = billingItms;

            List<BillingModel> bmList = new List<BillingModel>();
            WorkorderDetail wd = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            TimeSpan servicetimeDiff = TimeSpan.Zero, trvlTimeDiff = TimeSpan.Zero;
            if (wd != null)
            {
                if (wd.StartDateTime != null && wd.ArrivalDateTime != null)
                {
                    DateTime arrival = Convert.ToDateTime(wd.ArrivalDateTime);
                    DateTime strt = Convert.ToDateTime(wd.StartDateTime);
                    trvlTimeDiff = arrival.Subtract(strt);
                }

                if (wd.ArrivalDateTime != null && wd.CompletionDateTime != null)
                {
                    DateTime arrival = Convert.ToDateTime(wd.ArrivalDateTime);
                    DateTime cmplt = Convert.ToDateTime(wd.CompletionDateTime);
                    servicetimeDiff = cmplt.Subtract(arrival);
                }
            }

            Contact contact = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == wo.CustomerID).FirstOrDefault();

            var ws = (from sc in FarmerBrothersEntitites.WorkorderSchedules
                      join t in FarmerBrothersEntitites.TECH_HIERARCHY on sc.Techid equals t.DealerId
                      where sc.WorkorderID == workOrderId && (sc.AssignedStatus.ToLower() == "sent" || sc.AssignedStatus.ToLower() == "accepted")
                      && t.FamilyAff == "SPT"
                      select new
                      {
                          Techid = sc.Techid,
                          AssignedStatus = sc.AssignedStatus,
                          WorkorderID = sc.WorkorderID,
                          familyAff = t.FamilyAff
                      }).FirstOrDefault();


            PricingDetail priceDtls = Utility.GetPricingDetails(wo.CustomerID, ws.Techid, wo.CustomerState, FarmerBrothersEntitites);


            List<WorkorderBillingDetail> wbdList = FarmerBrothersEntitites.WorkorderBillingDetails.Where(w => w.WorkorderId == workOrderId).ToList();
            foreach (WorkorderBillingDetail bitem in wbdList)
            {
                BillingItem blngItm = FarmerBrothersEntitites.BillingItems.Where(b => b.BillingCode == bitem.BillingCode).FirstOrDefault();

                if (blngItm != null)
                {
                    decimal tot = 0;

                    BillingModel bmItem = new BillingModel();
                    bmItem.BillingType = blngItm.BillingName;
                    bmItem.BillingCode = bitem.BillingCode;
                    bmItem.Quantity = Convert.ToInt32(bitem.Quantity);

                    if (blngItm.BillingName.ToLower() == "travel time")
                    {
                        decimal? travelAmt = priceDtls == null ? 0 : priceDtls.HourlyTravlRate;

                        bmItem.Duration = new DateTime(trvlTimeDiff.Ticks).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(travelAmt);
                        tot = Convert.ToDecimal(travelAmt * Convert.ToDecimal(trvlTimeDiff.TotalHours));
                        bmItem.Total = tot;
                    }
                    else if (blngItm.BillingName.ToLower() == "labor")
                    {
                        decimal? laborAmt = priceDtls == null ? 0 : priceDtls.HourlyLablrRate;

                        bmItem.Duration = new DateTime(servicetimeDiff.Ticks).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(laborAmt);
                        tot = Convert.ToDecimal(laborAmt * Convert.ToDecimal(servicetimeDiff.TotalHours));
                        bmItem.Total = tot;
                    }
                    else
                    {
                        bmItem.Duration = new DateTime(36000000000).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(blngItm.UnitPrice);
                        tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
                        bmItem.Total = tot;
                    }


                    cardModel.BillingTotal += tot;
                    bmList.Add(bmItem);
                }
            }


            if (string.IsNullOrEmpty(wo.FinalTransactionId))
            {
                string StartTime = null, ArrivalTime = null, CompletionTime = null;
                if (wd != null)
                {
                    StartTime = wd.StartDateTime.ToString().Trim();
                    ArrivalTime = wd.ArrivalDateTime.ToString().Trim();
                    CompletionTime = wd.CompletionDateTime.ToString().Trim();
                }

                decimal travelCost = 0, laborCost = 0;
                if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(ArrivalTime))
                {
                    DateTime arrival = Convert.ToDateTime(ArrivalTime);
                    DateTime strt = Convert.ToDateTime(StartTime);
                    TimeSpan timeDiff = arrival.Subtract(strt);

                    BillingItem TravelItem = blngItmsList.Where(a => a.BillingName.ToLower() == "travel time").FirstOrDefault();
                    decimal? travelAmt = priceDtls == null ? 0 : priceDtls.HourlyTravlRate;
                    travelCost = Convert.ToDecimal(travelAmt * Convert.ToDecimal(timeDiff.TotalHours));

                    if (travelCost >= 0)
                    {
                        BillingModel bmItem = new BillingModel();
                        bmItem.BillingType = TravelItem.BillingName;
                        bmItem.BillingCode = TravelItem.BillingCode;
                        bmItem.Quantity = 1;
                        bmItem.Duration = new DateTime(timeDiff.Ticks).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(travelAmt);
                        bmItem.Total = travelCost;

                        //cardModel.BillingTotal += tot;
                        cardModel.BillingTotal += travelCost;
                        bmList.Add(bmItem);
                    }
                }

                if (!string.IsNullOrEmpty(CompletionTime) && !string.IsNullOrEmpty(ArrivalTime))
                {
                    DateTime arrival = Convert.ToDateTime(ArrivalTime);
                    DateTime cmplt = Convert.ToDateTime(CompletionTime);
                    TimeSpan srvcetimeDiff = cmplt.Subtract(arrival);

                    BillingItem laborItem = blngItmsList.Where(a => a.BillingName.ToLower() == "labor").FirstOrDefault();
                    decimal? laborAmt = priceDtls == null ? 0 : priceDtls.HourlyLablrRate;
                    laborCost = Convert.ToDecimal(laborAmt * Convert.ToDecimal(srvcetimeDiff.TotalHours));

                    if (laborCost >= 0)
                    {
                        BillingModel bmItem = new BillingModel();
                        bmItem.BillingType = laborItem.BillingName;
                        bmItem.BillingCode = laborItem.BillingCode;
                        bmItem.Quantity = 1;
                        bmItem.Duration = new DateTime(srvcetimeDiff.Ticks).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(laborAmt);
                        bmItem.Total = laborCost;

                        cardModel.BillingTotal += laborCost;
                        bmList.Add(bmItem);
                    }

                }

            }

            StateTax st = FarmerBrothersEntitites.StateTaxes.Where(s => s.ZipCode == wo.CustomerZipCode).FirstOrDefault();
            if (st != null)
            {
                cardModel.SaleTax = Convert.ToDecimal(st.StateRate);
            }

            cardModel.PartsDiscount = priceDtls == null ? 0 : Convert.ToDecimal(priceDtls.PartsDiscount);
            cardModel.BillingDetails = bmList;
            cardModel.WorkorderId = workOrderId;
            cardModel.FinalTransactionId = wo.FinalTransactionId;
            cardModel.WorkorderEntryDate = wo.WorkorderEntryDate;
            cardModel.StartDateTime = wd.StartDateTime;
            cardModel.ArrivalDateTime = wd.ArrivalDateTime;
            cardModel.CompletionDateTime = wd.CompletionDateTime;

            BillingItem prePaymentTravle = blngItmsList.Where(a => a.BillingName.ToLower() == "pre-payment travel").FirstOrDefault();
            cardModel.PreTravelCost = prePaymentTravle == null ? 0 : Convert.ToDecimal(prePaymentTravle.UnitPrice);

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = cardModel };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ProcessCardModel GetCardProcessDetails1(int workOrderId, int techId)
        {
            WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();

            WorkorderSchedule techWorkOrderSchedule = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrderId && w.Techid == techId).FirstOrDefault();            

            ProcessCardModel cardModel = new ProcessCardModel();
            List<FbWorkorderBillableSKUModel> partsList = new List<FbWorkorderBillableSKUModel>();

            var evtPrtList = (from closureSku in FarmerBrothersEntitites.WorkorderParts
                              where closureSku.WorkorderID == workOrderId && (closureSku.AssetID == null || closureSku.AssetID == 0)
                              select new
                              {
                                  sku = closureSku.Sku,
                                  des = closureSku.Description,
                                  qty = closureSku.Quantity,
                                  evtId = closureSku.WorkorderID,
                                  unitcost = closureSku.Total / closureSku.Quantity,
                                  Mnftr = closureSku.Manufacturer
                              }).ToList();


            cardModel.PartsList = new List<FbWorkorderBillableSKUModel>();
            foreach (var wp in evtPrtList)
            {
                FbWorkorderBillableSKUModel fbsm = new FbWorkorderBillableSKUModel();
                fbsm.SKU = wp.sku;
                fbsm.WorkorderID = Convert.ToInt32(wp.evtId);
                fbsm.UnitPrice = wp.unitcost;
                fbsm.Qty = wp.qty;
                fbsm.Description = wp.des;

                cardModel.PartsList.Add(fbsm);
            }

            cardModel.SKUList = DispatchResponseController.CloserSKU(FarmerBrothersEntitites);

            List<BillingItem> blngItmsList = FarmerBrothersEntitites.BillingItems.Where(b => b.IsActive == true).ToList();
            List<CategoryModel> billingItms = new List<CategoryModel>();
            foreach (BillingItem item in blngItmsList)
            {
                billingItms.Add(new CategoryModel(item.BillingName));
            }
            cardModel.BillingItems = billingItms;

            List<BillingModel> bmList = new List<BillingModel>();
            WorkorderDetail wd = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            TimeSpan servicetimeDiff = TimeSpan.Zero, trvlTimeDiff = TimeSpan.Zero;
            if (wd != null)
            {
                if (wd.StartDateTime != null && wd.ArrivalDateTime != null)
                {
                    DateTime arrival = Convert.ToDateTime(wd.ArrivalDateTime);
                    DateTime strt = Convert.ToDateTime(wd.StartDateTime);
                    trvlTimeDiff = arrival.Subtract(strt);
                }

                if (wd.ArrivalDateTime != null && wd.CompletionDateTime != null)
                {
                    DateTime arrival = Convert.ToDateTime(wd.ArrivalDateTime);
                    DateTime cmplt = Convert.ToDateTime(wd.CompletionDateTime);
                    servicetimeDiff = cmplt.Subtract(arrival);
                }
            }

            Contact contact = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == wo.CustomerID).FirstOrDefault();

            var ws = (from sc in FarmerBrothersEntitites.WorkorderSchedules
                      join t in FarmerBrothersEntitites.TECH_HIERARCHY on sc.Techid equals t.DealerId
                      where sc.WorkorderID == workOrderId && (sc.AssignedStatus.ToLower() == "sent" || sc.AssignedStatus.ToLower() == "accepted")
                      && t.FamilyAff == "SPT"
                      select new
                      {
                          Techid = sc.Techid,
                          AssignedStatus = sc.AssignedStatus,
                          WorkorderID = sc.WorkorderID,
                          familyAff = t.FamilyAff
                      }).FirstOrDefault();


            PricingDetail priceDtls = Utility.GetPricingDetails(wo.CustomerID, ws.Techid, wo.CustomerState, FarmerBrothersEntitites);
            /*PricingDetail priceDtls = null;
            if (!string.IsNullOrEmpty(contact.PricingParentID) && contact.PricingParentID != "0")
            {
                priceDtls = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingTypeId == 501 && p.PricingEntityId == contact.PricingParentID).FirstOrDefault();
            }
            else if (ws != null)
            {
                string tId = ws.Techid.ToString();
                priceDtls = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingTypeId == 502 && p.PricingEntityId == tId).FirstOrDefault();
            }
            else
            {
                priceDtls = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingTypeId == 503 && p.PricingEntityId == wo.CustomerState).FirstOrDefault();
            }


            if (priceDtls == null)
            {
                BillingItem TravelItem = blngItmsList.Where(a => a.BillingName.ToLower() == "travel time").FirstOrDefault();
                BillingItem laborItem = blngItmsList.Where(a => a.BillingName.ToLower() == "labor").FirstOrDefault();

                priceDtls = new PricingDetail();
                priceDtls.HourlyLablrRate = TravelItem.UnitPrice;
                priceDtls.HourlyTravlRate = laborItem.UnitPrice;
            }*/



            List<WorkorderBillingDetail> wbdList = FarmerBrothersEntitites.WorkorderBillingDetails.Where(w => w.WorkorderId == workOrderId).ToList();
            foreach (WorkorderBillingDetail bitem in wbdList)
            {
                BillingItem blngItm = FarmerBrothersEntitites.BillingItems.Where(b => b.BillingCode == bitem.BillingCode).FirstOrDefault();

                if (blngItm != null)
                {
                    decimal tot = 0;

                    BillingModel bmItem = new BillingModel();
                    bmItem.BillingType = blngItm.BillingName;
                    bmItem.BillingCode = bitem.BillingCode;
                    bmItem.Quantity = Convert.ToInt32(bitem.Quantity);

                    if (blngItm.BillingName.ToLower() == "travel time")
                    {
                        decimal? travelAmt = priceDtls == null ? 0 : priceDtls.HourlyTravlRate;

                        bmItem.Duration = new DateTime(trvlTimeDiff.Ticks).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(travelAmt);
                        tot = Convert.ToDecimal(travelAmt * Convert.ToDecimal(trvlTimeDiff.TotalHours));
                        bmItem.Total = tot;
                    }
                    else if (blngItm.BillingName.ToLower() == "labor")
                    {
                        decimal? laborAmt = priceDtls == null ? 0 : priceDtls.HourlyLablrRate;

                        bmItem.Duration = new DateTime(servicetimeDiff.Ticks).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(laborAmt);
                        tot = Convert.ToDecimal(laborAmt * Convert.ToDecimal(servicetimeDiff.TotalHours));
                        bmItem.Total = tot;
                    }
                    else
                    {
                        bmItem.Duration = new DateTime(36000000000).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(blngItm.UnitPrice);
                        tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
                        bmItem.Total = tot;
                    }


                    cardModel.BillingTotal += tot;
                    bmList.Add(bmItem);
                }
            }


            if (string.IsNullOrEmpty(wo.FinalTransactionId))
            {
                string StartTime = null, ArrivalTime = null, CompletionTime = null;
                if (wd != null)
                {
                    StartTime = wd.StartDateTime.ToString().Trim();
                    ArrivalTime = wd.ArrivalDateTime.ToString().Trim();
                    CompletionTime = wd.CompletionDateTime.ToString().Trim();
                }

                decimal travelCost = 0, laborCost = 0;
                if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(ArrivalTime))
                {
                    DateTime arrival = Convert.ToDateTime(ArrivalTime);
                    DateTime strt = Convert.ToDateTime(StartTime);
                    TimeSpan timeDiff = arrival.Subtract(strt);

                    BillingItem TravelItem = blngItmsList.Where(a => a.BillingName.ToLower() == "travel time").FirstOrDefault();
                    decimal? travelAmt = priceDtls == null ? 0 : priceDtls.HourlyTravlRate;
                    travelCost = Convert.ToDecimal(travelAmt * Convert.ToDecimal(timeDiff.TotalHours));

                    if (travelCost >= 0)
                    {
                        BillingModel bmItem = new BillingModel();
                        bmItem.BillingType = TravelItem.BillingName;
                        bmItem.BillingCode = TravelItem.BillingCode;
                        bmItem.Quantity = 1;
                        bmItem.Duration = new DateTime(timeDiff.Ticks).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(travelAmt);
                        //decimal tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
                        //bmItem.Total = tot;
                        bmItem.Total = travelCost;

                        //cardModel.BillingTotal += tot;
                        cardModel.BillingTotal += travelCost;
                        bmList.Add(bmItem);
                    }
                }

                if (!string.IsNullOrEmpty(CompletionTime) && !string.IsNullOrEmpty(ArrivalTime))
                {
                    DateTime arrival = Convert.ToDateTime(ArrivalTime);
                    DateTime cmplt = Convert.ToDateTime(CompletionTime);
                    TimeSpan srvcetimeDiff = cmplt.Subtract(arrival);

                    BillingItem laborItem = blngItmsList.Where(a => a.BillingName.ToLower() == "labor").FirstOrDefault();
                    decimal? laborAmt = priceDtls == null ? 0 : priceDtls.HourlyLablrRate;
                    laborCost = Convert.ToDecimal(laborAmt * Convert.ToDecimal(srvcetimeDiff.TotalHours));

                    if (laborCost >= 0)
                    {
                        BillingModel bmItem = new BillingModel();
                        bmItem.BillingType = laborItem.BillingName;
                        bmItem.BillingCode = laborItem.BillingCode;
                        bmItem.Quantity = 1;
                        bmItem.Duration = new DateTime(srvcetimeDiff.Ticks).ToString("HH:mm") + " Hrs";
                        bmItem.Cost = Convert.ToDecimal(laborAmt);
                        //decimal tot = Convert.ToDecimal(bmItem.Quantity * bmItem.Cost);
                        //bmItem.Total = tot;
                        bmItem.Total = laborCost;

                        //cardModel.BillingTotal += tot;
                        cardModel.BillingTotal += laborCost;
                        bmList.Add(bmItem);
                    }

                }

            }


            StateTax st = FarmerBrothersEntitites.StateTaxes.Where(s => s.ZipCode == wo.CustomerZipCode).FirstOrDefault();
            if (st != null)
            {
                cardModel.SaleTax = Convert.ToDecimal(st.StateRate);
            }

            cardModel.PartsDiscount = priceDtls == null ? 0 : Convert.ToDecimal(priceDtls.PartsDiscount);
            cardModel.BillingDetails = bmList;
            cardModel.WorkorderId = workOrderId;
            cardModel.FinalTransactionId = wo.FinalTransactionId;
            cardModel.WorkorderEntryDate = wo.WorkorderEntryDate;
            cardModel.StartDateTime = wd.StartDateTime;
            cardModel.ArrivalDateTime = wd.ArrivalDateTime;
            cardModel.CompletionDateTime = wd.CompletionDateTime;

            BillingItem prePaymentTravle = blngItmsList.Where(a => a.BillingName.ToLower() == "pre-payment travel").FirstOrDefault();
            cardModel.PreTravelCost = prePaymentTravle == null ? 0 : Convert.ToDecimal(prePaymentTravle.UnitPrice);

            return cardModel;
        }

        public JsonResult GetBillableSkuDetails(string sku)
        {
            decimal? unitprice = FarmerBrothersEntitites.FbBillableSKUs.Where(s => s.SKU == sku && s.IsActive == true).Select(s => s.UnitPrice).FirstOrDefault();
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = unitprice };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult BillableInsert(FbWorkorderBillableSKUModel value)
        {
            IList<FbWorkorderBillableSKUModel> SkuItems = TempData["Billable"] as IList<FbWorkorderBillableSKUModel>;
            if (SkuItems == null)
            {
                SkuItems = new List<FbWorkorderBillableSKUModel>();
            }

            if (TempData["WorkOrderSKUId"] != null)
            {
                int eqpId = Convert.ToInt32(TempData["WorkOrderSKUId"]);
                value.WorkOrderSKUId = eqpId + 1;
                TempData["WorkOrderSKUId"] = eqpId + 1;
            }
            else
            {
                value.WorkOrderSKUId = 1;
                value.UnitPrice = Convert.ToDecimal(value.UnitPrice);
                TempData["WorkOrderSKUId"] = 1;
            }

            SkuItems.Add(value);
            TempData["Billable"] = SkuItems;
            TempData.Keep("Billable");
            return Json(value, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BillableUpdate(FbWorkorderBillableSKUModel value)
        {
            IList<FbWorkorderBillableSKUModel> SkuItems = TempData["Billable"] as IList<FbWorkorderBillableSKUModel>;
            if (SkuItems == null)
            {
                SkuItems = new List<FbWorkorderBillableSKUModel>();
            }
            FbWorkorderBillableSKUModel SkuItem = SkuItems.Where(n => n.WorkOrderSKUId == value.WorkOrderSKUId).FirstOrDefault();

            if (SkuItem != null)
            {
                SkuItem.SKU = value.SKU;
                SkuItem.Qty = value.Qty;
                SkuItem.UnitPrice = value.UnitPrice;
            }

            TempData["Billable"] = SkuItems;
            TempData.Keep("Billable");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BillableDelete(int key)
        {
            IList<FbWorkorderBillableSKUModel> skuItems = TempData["Billable"] as IList<FbWorkorderBillableSKUModel>;
            FbWorkorderBillableSKUModel skuItem = skuItems.Where(n => n.WorkOrderSKUId == key).FirstOrDefault();
            skuItems.Remove(skuItem);
            TempData["Billable"] = skuItems;
            TempData.Keep("Billable");
            return Json(skuItems, JsonRequestBehavior.AllowGet);
        }
        private double SaveBillableList(WorkorderManagementModel workorderManagement, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            double totalPrice = 0;
            double UnitPrice = 0;
            IEnumerable<FbWorkOrderSKU> FBWOSkuList = FarmerBrothersEntitites.FbWorkOrderSKUs.Where(a => a.WorkorderID == workorderManagement.WorkOrder.WorkorderID).ToList();

            if (FBWOSkuList != null)
            {
                for (int count = FBWOSkuList.Count() - 1; count >= 0; count--)
                {
                    FbWorkOrderSKU FBWOSku = FBWOSkuList.ElementAt(count);
                    FarmerBrothersEntitites.FbWorkOrderSKUs.Remove(FBWOSku);
                }
            }

            IList<FbWorkorderBillableSKUModel> newFBWOList = workorderManagement.BillableSKUList.ToList();
            if (newFBWOList.Count() > 0)
            {
                foreach (FbWorkorderBillableSKUModel newFBWOItem in newFBWOList)
                {
                    FbWorkOrderSKU FBWOSku = new FbWorkOrderSKU()
                    {
                        WorkorderID = workorderManagement.WorkOrder.WorkorderID,
                        SKU = newFBWOItem.SKU,
                        Qty = newFBWOItem.Qty
                    };
                    using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                    {
                        UnitPrice = Convert.ToDouble(entity.FbBillableSKUs.Where(s => s.SKU == newFBWOItem.SKU).Select(s => s.UnitPrice).FirstOrDefault());
                    }
                    totalPrice += Convert.ToDouble(newFBWOItem.Qty * UnitPrice);
                    FarmerBrothersEntitites.FbWorkOrderSKUs.Add(FBWOSku);
                }
            }

            return totalPrice;
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "WorkorderSave")]
        [ActionName("SaveWorkOrder")]
        public JsonResult SaveWorkOrder([ModelBinder(typeof(CallCloserModelBinder))] WorkorderManagementModel workorderManagement)
        {
            string message = string.Empty;
            WorkOrder workOrder = null;
            int returnValue = CloseWorkOrder(workorderManagement, out message, out workOrder);
            
            if (string.IsNullOrEmpty(message) || message.Contains("Spawned Work Order"))
            {
                var redirectUrl = string.Empty;
                redirectUrl = new UrlHelper(Request.RequestContext).Action("CallClosure", "CallClosure");
             
                string WOConfirmationCode = string.Empty;
             
                string WorkorderID = workorderManagement.WorkOrder.WorkorderID.ToString();
                //WOConfirmationCode = WorkorderID.Substring(0, 3) + WorkorderController.sGenPwd(2) + WorkorderID.Substring(WorkorderID.Length - 6);

                if (WorkorderID.Length == 5) // For Test server events
                {
                    WOConfirmationCode = WorkorderID.Substring(0, 1) + WorkorderController.sGenPwd(2) + WorkorderID.Substring(WorkorderID.Length - 2);
                }
                else
                {
                    WOConfirmationCode = WorkorderID.Substring(0, 3) + WorkorderController.sGenPwd(2) + WorkorderID.Substring(WorkorderID.Length - 6);
                }

                WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrder.WorkorderID).FirstOrDefault();
                wo.WorkorderClosureConfirmationNo = WOConfirmationCode;

                if (wo != null)
                {
                    if (!string.IsNullOrWhiteSpace(wo.CurrentUserName)
                        && string.Compare(wo.CurrentUserName, UserName, true) == 0)
                    {
                        wo.WorkOrderOpenedDateTime = null;
                        wo.CurrentUserName = null;                        
                    }
                }

                DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                NotesHistory notes = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = "Closure Conf No: " + WOConfirmationCode,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = UserName,
                };
                notes.WorkorderID = workOrder.WorkorderID;
                workOrder.NotesHistories.Add(notes);

                FarmerBrothersEntitites.SaveChanges();
               

                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = redirectUrl, WorkOrderId = workOrder.WorkorderID, returnValue = 1, WorkorderCallstatus = workOrder.WorkorderCallstatus, message = message, WOConfirmationCode = WOConfirmationCode };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            else
            {
                string callStatus = workOrder == null ? "" : workOrder.WorkorderCallstatus;
                JsonResult jsonResult = new JsonResult();

                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = "", WorkOrderId = 0, returnValue = returnValue, WorkorderCallstatus = callStatus, message = message };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
        }

        private bool ValidateSkuList(IList<FbWorkorderBillableSKUModel> SKUList)
        {
            List<String> duplicates = SKUList.GroupBy(x => x.SKU)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key)
                             .ToList();

            if (duplicates.Count > 0)
                return true;
            else
                return false;
        }

        public int CloseWorkOrder(WorkorderManagementModel workorderManagement, out string message, out WorkOrder workOrder)
        {
            int returnValue = -1;
            workOrder = null;
            message = string.Empty;
            TimeZoneInfo newTimeZoneInfo = null;
            Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                        
            DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
            CallClosureController callCloser = new CallClosureController();
            {
                using (FarmerBrothersEntities newEntity = new FarmerBrothersEntities())
                {
                    using (System.Data.Entity.DbContextTransaction dbTran = newEntity.Database.BeginTransaction())
                    {
                        try
                        {
                            workOrder = newEntity.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);

                            double totalPrice = SaveBillableList(workorderManagement, newEntity);
                            workOrder.TotalUnitPrice = totalPrice.ToString();
                            workOrder.IsBillable = Convert.ToBoolean(workorderManagement.IsBillable);

                            SaveClosureDetails(workorderManagement, workOrder, newEntity);
                            newEntity.SaveChanges();
                            //SaveWorkOrderEquipments(workorderManagement, workOrder, newEntity);
                            //newEntity.SaveChanges();
                            SaveClosureAssets(workorderManagement, workOrder, newEntity);
                            returnValue = newEntity.SaveChanges();
                            if (workorderManagement.WorkOrder.WorkorderID > 0)
                            {

                                bool isValid = true;
                                bool isNSRAsset = false;

                                if ((workorderManagement.IsBillableFeed == false && workorderManagement.BillableSKUList.Count > 0))
                                {
                                    message += @"|Please select Parts Billable checkbox before adding sku's to the work order";
                                    isValid = false;
                                }
                                if ((workorderManagement.IsBillableFeed == true && workorderManagement.BillableSKUList.Count == 0))
                                {
                                    message += @"|Please add sku's when Parts Billable checkbox is selected";
                                    isValid = false;
                                }
                                if (workorderManagement.IsBillableFeed == true && workorderManagement.BillableSKUList.Count > 0)
                                {
                                    bool DuplicateSkusExist = ValidateSkuList(workorderManagement.BillableSKUList);
                                    if (DuplicateSkusExist)
                                    {
                                        message += @"|Duplicate SKUs are added to the Billable SKU Grid";
                                        isValid = false;
                                    }
                                }

                                WorkorderDetail workOrderDetail = newEntity.WorkorderDetails.FirstOrDefault(wd => wd.WorkorderID == workorderManagement.WorkOrder.WorkorderID);
                                if (workOrderDetail != null)
                                {
                                    if (!workOrderDetail.Mileage.HasValue && workOrderDetail.Mileage == 0)
                                    {
                                        message = @"|Mileage is not updated!";
                                        isValid = false;
                                    }

                                    if (!workOrderDetail.StartDateTime.HasValue)
                                    {
                                        message += @"|Start Date & Time are not updated!";
                                        isValid = false;
                                    }

                                    if (!workOrderDetail.ArrivalDateTime.HasValue)
                                    {
                                        message += @"|Arrival Date & Time are not updated!";
                                        isValid = false;
                                    }

                                    if (!workOrderDetail.CompletionDateTime.HasValue)
                                    {
                                        message += @"|Completion Date & Time are not updated!";
                                        isValid = false;
                                    }

                                    if (string.IsNullOrWhiteSpace(workOrderDetail.StateofEquipment))
                                    {
                                        message += @"|State of Equipment is not updated!";
                                        isValid = false;
                                    }
                                    if (string.IsNullOrWhiteSpace(workOrderDetail.ServiceDelayReason))
                                    {
                                        message += @"|Service Reason is not updated!";
                                        isValid = false;
                                    }
                                    if (string.IsNullOrWhiteSpace(workOrderDetail.TroubleshootSteps))
                                    {
                                        message += @"|Troubleshoot steps not updated!";
                                        isValid = false;
                                    }
                                    if (string.IsNullOrWhiteSpace(workOrderDetail.ReviewedBy))
                                    {
                                        message += @"|Reviewed by not updated!";
                                        isValid = false;
                                    }
                                    if (string.IsNullOrWhiteSpace(workOrderDetail.IsUnderWarrenty))
                                    {
                                        message += @"|Under Warrenty not updated!";
                                        isValid = false;
                                    }
                                    if (!string.IsNullOrWhiteSpace(workOrderDetail.IsUnderWarrenty) && workOrderDetail.IsUnderWarrenty.ToLower() == "yes")
                                    {
                                        if (string.IsNullOrWhiteSpace(workOrderDetail.WarrentyFor))
                                        {
                                            message += @"|Under WarrentyFor not updated!";
                                            isValid = false;
                                        }
                                    }
                                    if (string.IsNullOrWhiteSpace(workOrderDetail.AdditionalFollowupReq))
                                    {
                                        message += @"|Additional Followup not updated!";
                                        isValid = false;
                                    }
                                    if (!string.IsNullOrWhiteSpace(workOrderDetail.AdditionalFollowupReq) && workOrderDetail.AdditionalFollowupReq.ToLower() == "yes")
                                    {
                                        if (string.IsNullOrWhiteSpace(workOrderDetail.FollowupComments))
                                        {
                                            message += @"|Under Followup Comments not updated!";
                                            isValid = false;
                                        }
                                    }
                                    if (string.IsNullOrWhiteSpace(workOrderDetail.IsOperational))
                                    {
                                        message += @"|Operational Field not updated!";
                                        isValid = false;
                                    }
                                    //if (string.IsNullOrWhiteSpace(workOrderDetail.OperationalComments))
                                    //{
                                    //    message += @"|Operational Comments not updated!";
                                    //    isValid = false;
                                    //}


                                    if (string.IsNullOrWhiteSpace(workOrderDetail.ResponsibleTechName))
                                    {
                                        message += @"|Responsible Tech Name is not updated!";
                                        isValid = false;
                                    }
                                    if (workorderManagement.WorkOrder.WorkorderCallstatus != "Closed")
                                    {
                                        if (string.IsNullOrWhiteSpace(workOrderDetail.InvoiceNo))
                                        {
                                            message += @"|Invoice Number is not updated!";
                                            isValid = false;
                                        }
                                    }

                                    if (string.IsNullOrEmpty(workorderManagement.Closure.CustomerSignatureDetails))
                                    {
                                        message += @"|Customer Signature Required!";
                                        isValid = false;
                                    }
                                    if (string.IsNullOrEmpty(workorderManagement.Closure.CustomerSignedBy))
                                    {
                                        message += @"|SignatureBy Required!";
                                        isValid = false;
                                    }

                                    if (workOrder.WorkorderEquipments.Count <= 0)
                                    {
                                        message += @"|Asset Details are not updated!";
                                        isValid = false;
                                    }


                                    int nCount = 1;
                                    foreach (WorkorderEquipment equipment in workOrder.WorkorderEquipments)
                                    {
                                        if (equipment.Solutionid.HasValue && equipment.Solutionid == 9999)
                                        {
                                            isNSRAsset = true;
                                            nCount++;
                                            continue;
                                        }
                                        if (!equipment.Solutionid.HasValue)
                                        {
                                            message += @"|Completion Code is not entered for equipment at row - " + nCount;
                                            isValid = false;
                                        }
                                        if (!equipment.CallTypeid.HasValue)
                                        {
                                            message += @"|Service Code is not entered for equipment at row - " + nCount;
                                            isValid = false;
                                        }

                                        if (string.IsNullOrWhiteSpace(equipment.Category))
                                        {
                                            message += @"|Equipment Type is not entered for equipment at row - " + nCount;
                                            isValid = false;
                                        }

                                        if (string.IsNullOrWhiteSpace(equipment.Manufacturer))
                                        {
                                            message += @"|Manufacturer is not entered for equipment at row - " + nCount;
                                            isValid = false;
                                        }

                                        if (string.IsNullOrWhiteSpace(equipment.Model))
                                        {
                                            message += @"|Model is not entered for equipment at row - " + nCount;
                                            isValid = false;
                                        }

                                        
                                        if (string.IsNullOrWhiteSpace(equipment.SerialNumber))
                                        {
                                            message += @"|SerialNumber is not entered for equipment at row - " + nCount;
                                            isValid = false;
                                        }


                                        if (/*workorderManagement.WorkOrder.WorkorderCalltypeid != 1800
                                            && workorderManagement.WorkOrder.WorkorderCalltypeid != 1810
                                            && workorderManagement.WorkOrder.WorkorderCalltypeid != 1830
                                            && workorderManagement.WorkOrder.WorkorderCalltypeid != 1820
                                            &&*/ equipment.CallTypeid != 1600)
                                        {

                                            //if (string.IsNullOrEmpty(equipment.WorkDescription))
                                            //{
                                            //    message += @"|Work performed is not entered for equipment at row - " + nCount;
                                            //    isValid = false;
                                            //}

                                            //if (equipment.NoPartsNeeded == false || equipment.NoPartsNeeded == null)
                                            //{
                                            //    IEnumerable<WorkorderPart> parts = workOrder.WorkorderParts.Where(a => a.AssetID == equipment.Assetid);

                                            //    if (parts == null || parts.Count() <= 0)
                                            //    {
                                            //        message += @"|Parts selection is required for equipment at row - " + nCount;
                                            //        isValid = false;
                                            //    }
                                            //}
                                        }
                                        nCount++;
                                    }

                                    if (workorderManagement.WorkOrder.WorkorderCalltypeid == 1300 && (!string.IsNullOrWhiteSpace(workorderManagement.WorkOrder.WorkorderErfid)))
                                    {
                                        Erf woErf = FarmerBrothersEntitites.Erfs.Where(er => er.ErfID == workorderManagement.WorkOrder.WorkorderErfid).FirstOrDefault();
                                        DateTime dt1 = currentTime.Date;
                                        DateTime dt2 = woErf.OriginalRequestedDate == null ? DateTime.Now.Date : Convert.ToDateTime(woErf.OriginalRequestedDate).Date;
                                        // if (woErf != null && woErf.OriginalRequestedDate != null && currentTime.CompareTo(woErf.OriginalRequestedDate) > 0)
                                        if (dt1 > dt2)                                          
                                        {
                                            if (workorderManagement.ReasonCode == null || workorderManagement.ReasonCode <= 0)
                                            {
                                                message += @"| Enter the Reason Code ";
                                                isValid = false;
                                            }
                                        }
                                    }

                                    if ((!string.IsNullOrEmpty(workorderManagement.Customer.BillingCode) && workorderManagement.Customer.BillingCode.ToLower() == "s08"))
                                    {
                                        WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).FirstOrDefault();

                                        if (!string.IsNullOrEmpty(wo.AuthTransactionId) && string.IsNullOrEmpty(wo.FinalTransactionId))
                                        {
                                            message = @"Please process the Credit Card from Email Link, before Closing the Event!";
                                            isValid = false;
                                        }
                                    }

                                    if(workorderManagement.IsServiceBillable == true && string.IsNullOrEmpty(workOrder.FinalTransactionId))
                                    {
                                        message = @"Please process the Payment, before Closing the Event!";
                                        isValid = false;
                                    }

                                    if (isNSRAsset)
                                    {
                                        isValid = true;
                                        message = string.Empty;
                                    }
                                    if (isValid == true)
                                    {
                                        NotesHistory notesHistory1 = new NotesHistory()
                                        {
                                            AutomaticNotes = 1,
                                            EntryDate = currentTime,
                                            Notes = "Work Order Closed from MARS by " + UserName,
                                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                                            UserName = UserName,
                                        };
                                        notesHistory1.WorkorderID = workOrder.WorkorderID;
                                        workOrder.NotesHistories.Add(notesHistory1);

                                        if (workorderManagement.WorkOrder.WorkorderCalltypeid == 1300 && workorderManagement.ReasonCode > 0)
                                        {
                                            AllFBStatu afb = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workorderManagement.ReasonCode).FirstOrDefault();

                                            if (afb != null)
                                            {
                                                NotesHistory reasonNotes = new NotesHistory()
                                                {
                                                    AutomaticNotes = 1,
                                                    EntryDate = currentTime,
                                                    Notes = "Install Event Schedule ReasonCode - " + afb.FBStatus,
                                                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                                    UserName = UserName,
                                                };
                                                reasonNotes.WorkorderID = workOrder.WorkorderID;
                                                workOrder.NotesHistories.Add(reasonNotes);

                                                workOrder.RescheduleReasonCode = workorderManagement.ReasonCode;
                                            }
                                        }

                                        workOrder.WorkorderCallstatus = "Closed";
                                        workOrder.ClosedUserName = UserName;
                                        workOrder.WorkorderCloseDate = currentTime;
                                        workOrder.WorkorderModifiedDate = currentTime;
                                        workOrder.IsBillable = workorderManagement.IsBillableFeed;

                                        //callCloser.CreateInvoice(workorderManagement, workOrder, newEntity);
                                        Invoice invoicedetails = newEntity.Invoices.Where(inv => inv.WorkorderID == workorderManagement.WorkOrder.WorkorderID).FirstOrDefault();
                                        string invoiceId = string.Empty;
                                        if (invoicedetails != null)
                                        {
                                            newEntity.Invoices.Remove(invoicedetails);
                                        }


                                        callCloser.CreatePartsOrder(workorderManagement, workOrder, newEntity);
                                        newEntity.SaveChanges();
                                        string spawnMessage = string.Empty;
                                        CreateSpawnWorkOrder(workorderManagement, workOrder.WorkorderEquipments.ToList(), newEntity, out spawnMessage);
                                        if (!string.IsNullOrWhiteSpace(spawnMessage))
                                        {
                                            message += @"|" + spawnMessage;
                                        }

                                        /*foreach (WorkorderEquipment equipment in workOrder.WorkorderEquipments)
                                        {
                                            //if (equipment.Solutionid == 5115
                                            //    || equipment.Solutionid == 5135
                                            //    || equipment.Solutionid == 5140
                                            //    || equipment.Solutionid == 5150
                                            //    || equipment.Solutionid == 5160
                                            //    || equipment.Solutionid == 5170
                                            //    || equipment.Solutionid == 5171
                                            //    || equipment.Solutionid == 5181
                                            //    || equipment.Solutionid == 5191)
                                            if (equipment.Solutionid == 5115
                                               || equipment.Solutionid == 5120
                                               || equipment.Solutionid == 5130
                                               || equipment.Solutionid == 5135
                                               || equipment.Solutionid == 5140
                                               || equipment.Solutionid == 5170
                                               || equipment.Solutionid == 5171
                                               || equipment.Solutionid == 5181
                                               || equipment.Solutionid == 5191)
                                            {
                                                callCloser.CreateSpawnWorkOrder(workorderManagement, equipment, newEntity, out spawnMessage);
                                                if (!string.IsNullOrWhiteSpace(spawnMessage))
                                                {
                                                    message += @"|" + spawnMessage;
                                                }
                                            }
                                        }*/

                                    }
                                }                                
                                else
                                {
                                    message = @"|Mileage is not updated!";
                                    message += @"|Arrival Date & Time are not updated!";
                                    message += @"|Completion Date & Time are not updated!";
                                    message += @"|Responsible Tech Name is not updated!";
                                    returnValue = -1;
                                }
                            }
                            returnValue = newEntity.SaveChanges();
                            dbTran.Commit();
                            if (newEntity != null)
                            {
                                newEntity.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            dbTran.Rollback();
                            message = string.Empty;
                            message = "|There is a problem in complete Work Order! Please contact support.";
                        }
                    }


                }
            }
            return returnValue;
        }

        #region Call Closer Closer Workorder Methods

        private void SaveClosureDetails(WorkorderManagementModel workorderManagement, WorkOrder workOrder, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (workorderManagement.Closure != null)
            {
                string specialClosure = string.Empty;

                if (!string.IsNullOrWhiteSpace(workorderManagement.Closure.SpecialClosure))
                {
                    string[] specialClosureList = workorderManagement.Closure.SpecialClosure.Split(',');
                    if (specialClosureList.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(specialClosureList[0]))
                        {
                            specialClosure = specialClosureList[0];
                        }
                    }
                }

                workorderManagement.Closure.SpecialClosure = specialClosure;

                WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0 && ws.AssignedStatus == "Accepted").FirstOrDefault();

                WorkorderDetail workOrderDetail = FarmerBrothersEntitites.WorkorderDetails.Where(wd => wd.WorkorderID == workOrder.WorkorderID).FirstOrDefault();
                if (workOrderDetail != null)
                {
                    workOrderDetail.StartDateTime = workorderManagement.Closure.StartDateTime;
                    workOrderDetail.ArrivalDateTime = workorderManagement.Closure.ArrivalDateTime;
                    if (workOrderDetail.ArrivalDateTime.HasValue && workOrderDetail.ArrivalDateTime.Value != DateTime.MinValue)
                    {
                        workOrder.WorkorderCallstatus = "On Site";
                    }

                    workOrderDetail.CompletionDateTime = workorderManagement.Closure.CompletionDateTime;
                    if (workOrderDetail.CompletionDateTime.HasValue && workOrderDetail.CompletionDateTime.Value != DateTime.MinValue)
                    {
                        workOrder.WorkorderCallstatus = "Completed";
                    }
                    workOrderDetail.InvoiceNo = workorderManagement.Closure.InvoiceNo;
                    workOrderDetail.ResponsibleTechName = workorderManagement.Closure.ResponsibleTechName;
                    workOrderDetail.Mileage = workorderManagement.Closure.Mileage;
                    workOrderDetail.CustomerName = workorderManagement.Closure.CustomerName;
                    workOrderDetail.CustomerEmail = workorderManagement.Closure.CustomerEmail;
                    workOrderDetail.CustomerSignatureDetails = workorderManagement.Closure.CustomerSignatureDetails;
                    workOrderDetail.TechnicianSignatureDetails = workorderManagement.Closure.TechnicianSignatureDetails;
                    workOrderDetail.WorkorderID = workOrder.WorkorderID;
                    workOrderDetail.InvoiceDate = DateTime.UtcNow;
                    workOrderDetail.EntryDate = workOrder.WorkorderEntryDate;
                    workOrderDetail.ModifiedDate = workOrder.WorkorderEntryDate;
                    if (workorderManagement.PhoneSolveId > 0)
                    {
                        workOrderDetail.PhoneSolveid = workorderManagement.PhoneSolveId;
                        workorderManagement.Closure.PhoneSolveid = workorderManagement.PhoneSolveId;
                    }

                    if (workorderManagement.PhoneSolveTechId > 0)
                    {
                        workOrderDetail.ResponsibleTechid = workorderManagement.PhoneSolveTechId;
                        workorderManagement.ResponsibleTechId = workorderManagement.PhoneSolveTechId;
                    }

                    workOrderDetail.SpecialClosure = specialClosure;
                    workOrderDetail.TravelTime = workorderManagement.Closure.TravelHours + ":" + workorderManagement.Closure.TravelMinutes;

                    Customer Customer = null;
                    DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                    string customerId = workOrder.CustomerID == null ? "" : workOrder.CustomerID.ToString();
                    if (!string.IsNullOrEmpty(customerId))
                    {
                        var CustomerId = int.Parse(customerId);
                        Customer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                    }

                    if (Customer != null)
                    {
                        Customer.FilterReplaced = workorderManagement.Closure.FilterReplaced;
                        Customer.FilterReplacedDate = currentTime;
                        Customer.NextFilterReplacementDate = currentTime.AddMonths(6);
                    }

                    workOrderDetail.WaterTested = workorderManagement.Closure.WaterTested;
                    workOrderDetail.HardnessRating = workorderManagement.Closure.HardnessRating;
                    workOrderDetail.CustomerSignatureBy = workorderManagement.Closure.CustomerSignedBy;
                    workOrderDetail.TotalDissolvedSolids = workorderManagement.Closure.TDS;

                    workOrderDetail.StateofEquipment = workorderManagement.Closure.StateOfEquipment;
                    workOrderDetail.ServiceDelayReason = workorderManagement.Closure.serviceDelayed;
                    workOrderDetail.TroubleshootSteps = workorderManagement.Closure.troubleshootSteps;
                    workOrderDetail.FollowupComments = workorderManagement.Closure.followupComments;
                    //workOrderDetail.OperationalComments = workorderManagement.Closure.operationalComments;
                    workOrderDetail.ReviewedBy = workorderManagement.Closure.ReviewedBy;

                    workOrderDetail.IsUnderWarrenty = workorderManagement.Closure.IsUnderWarrenty;
                    workOrderDetail.WarrentyFor = workorderManagement.Closure.WarrentyFor;
                    workOrderDetail.AdditionalFollowupReq = workorderManagement.Closure.AdditionalFollowup;
                    workOrderDetail.IsOperational = workorderManagement.Closure.Operational;

                    if (schedule != null)
                    {
                        workOrderDetail.ResponsibleTechid = schedule.Techid;
                    }
                    if (workOrderDetail.CustomerSignatureDetails != null)
                    {
                        //890 is for empty signature box
                        if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                        {
                            workOrderDetail.CustomerSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                                Select(s => s.CustomerSignatureDetails).FirstOrDefault();
                            if (workOrderDetail.CustomerSignatureDetails != null)
                            {
                                if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                                {
                                    workOrderDetail.CustomerSignatureDetails = string.Empty;
                                }
                            }
                            else
                            {
                                workOrderDetail.CustomerSignatureDetails = string.Empty;
                            }
                        }
                    }
                    if (workOrderDetail.TechnicianSignatureDetails != null)
                    {
                        //890 is for empty signature box
                        if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                        {
                            workOrderDetail.TechnicianSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                                Select(s => s.TechnicianSignatureDetails).FirstOrDefault();
                            if (workOrderDetail.TechnicianSignatureDetails != null)
                            {
                                if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                                {
                                    workOrderDetail.TechnicianSignatureDetails = string.Empty;
                                }
                            }
                            else
                            {
                                workOrderDetail.TechnicianSignatureDetails = string.Empty;
                            }
                        }
                    }
                }
                else
                {
                    workOrderDetail = new WorkorderDetail()
                    {
                        StartDateTime = workorderManagement.Closure.StartDateTime,
                        ArrivalDateTime = workorderManagement.Closure.ArrivalDateTime,
                        CompletionDateTime = workorderManagement.Closure.CompletionDateTime,
                        ResponsibleTechName = workorderManagement.Closure.ResponsibleTechName,
                        Mileage = workorderManagement.Closure.Mileage,
                        CustomerName = workorderManagement.Closure.CustomerName,
                        CustomerEmail = workorderManagement.Closure.CustomerEmail,
                        CustomerSignatureDetails = workorderManagement.Closure.CustomerSignatureDetails,
                        CustomerSignatureBy = workorderManagement.Closure.CustomerSignedBy,
                        TechnicianSignatureDetails = workorderManagement.Closure.TechnicianSignatureDetails,
                        WorkorderID = workOrder.WorkorderID,
                        InvoiceNo = workorderManagement.Closure.InvoiceNo,
                        InvoiceDate = DateTime.UtcNow,
                        EntryDate = workOrder.WorkorderEntryDate,
                        ModifiedDate = workOrder.WorkorderEntryDate,
                        SpecialClosure = specialClosure,
                        TravelTime = workorderManagement.Closure.TravelHours + ":" + workorderManagement.Closure.TravelMinutes,
                        WaterTested = workorderManagement.Closure.WaterTested,
                        HardnessRating = workorderManagement.Closure.HardnessRating,
                        TotalDissolvedSolids = workorderManagement.Closure.TDS
                    };

                    if (workorderManagement.Closure.PhoneSolveid > 0)
                    {
                        workOrderDetail.PhoneSolveid = workorderManagement.Closure.PhoneSolveid;
                    }

                    if (schedule != null)
                    {
                        workOrderDetail.ResponsibleTechid = schedule.Techid;
                    }
                    //890 is for empty signature box
                    if (workOrderDetail.CustomerSignatureDetails != null)
                    {
                        if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                        {
                            workOrderDetail.CustomerSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                                Select(s => s.CustomerSignatureDetails).FirstOrDefault();
                            if (workOrderDetail.CustomerSignatureDetails != null)
                            {
                                if (workOrderDetail.CustomerSignatureDetails.Length == 890)
                                {
                                    workOrderDetail.CustomerSignatureDetails = string.Empty;
                                }
                            }
                            else
                            {
                                workOrderDetail.CustomerSignatureDetails = string.Empty;
                            }
                        }
                    }
                    if (workOrderDetail.TechnicianSignatureDetails != null)
                    {
                        if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                        {
                            workOrderDetail.TechnicianSignatureDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID).
                                Select(s => s.TechnicianSignatureDetails).FirstOrDefault();
                            if (workOrderDetail.TechnicianSignatureDetails != null)
                            {
                                if (workOrderDetail.TechnicianSignatureDetails.Length == 890)
                                {
                                    workOrderDetail.TechnicianSignatureDetails = string.Empty;
                                }
                            }
                            else
                            {
                                workOrderDetail.TechnicianSignatureDetails = string.Empty;
                            }
                        }
                    }

                    FarmerBrothersEntitites.WorkorderDetails.Add(workOrderDetail);
                }

                if (!string.IsNullOrWhiteSpace(specialClosure))
                {
                    TimeZoneInfo newTimeZoneInfo = null;
                    Utility.GetCustomerTimeZone(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);
                    
                    DateTime currentTime = Utility.GetCurrentTime(workorderManagement.Customer.ZipCode, FarmerBrothersEntitites);

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = "Work Order Closed from MARS by " + UserName,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                        UserName = UserName,
                        isDispatchNotes = 1
                    };
                    notesHistory.WorkorderID = workOrder.WorkorderID;
                    workOrder.NotesHistories.Add(notesHistory);

                    if (string.Compare(specialClosure, "No Service Required", true) == 0)
                    {
                        workOrder.WorkorderEquipments.Clear();
                    }

                    workOrder.WorkorderCallstatus = "Closed";
                    workOrder.ClosedUserName = UserName;
                    workOrder.WorkorderCloseDate = currentTime;
                }
            }
        }
        private void SaveWorkOrderEquipments(WorkorderManagementModel workorderManagement, WorkOrder workOrder, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            IEnumerable<WorkorderEquipmentRequested> workOrderEquipments = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(we => we.WorkorderID == workOrder.WorkorderID);

            if (workOrderEquipments != null)
            {
                for (int count = workOrderEquipments.Count() - 1; count >= 0; count--)
                {
                    WorkorderEquipmentRequested equipment = workOrderEquipments.ElementAt(count);

                    WorkOrderManagementEquipmentModel equipmentFromModel = workorderManagement.WorkOrderEquipmentsRequested.Where(e => e.AssetId == equipment.Assetid).FirstOrDefault();
                    if (equipmentFromModel != null)
                    {
                        equipment.CallTypeid = equipmentFromModel.CallTypeID;
                        equipment.Category = equipmentFromModel.Category;
                        equipment.Location = equipmentFromModel.Location;
                        equipment.SerialNumber = equipmentFromModel.SerialNumber;
                        equipment.Model = equipmentFromModel.Model;
                        equipment.CatalogID = equipmentFromModel.CatelogID;
                        equipment.Symptomid = equipmentFromModel.SymptomID;
                    }
                    else
                    {
                        FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Remove(equipment);
                    }
                }
            }
            
            IList<WorkOrderManagementEquipmentModel> newKnownEquipments = workorderManagement.WorkOrderEquipmentsRequested.Where(e => e.AssetId < 1000).ToList();
            IndexCounter counter = Utility.GetIndexCounter("AssetID", newKnownEquipments.Count);
            foreach (WorkOrderManagementEquipmentModel newKnownEquipment in newKnownEquipments)
            {
                counter.IndexValue++;

                WorkorderEquipmentRequested equipment = new WorkorderEquipmentRequested()
                {
                    Assetid = counter.IndexValue.Value,
                    CallTypeid = newKnownEquipment.CallTypeID,
                    Category = newKnownEquipment.Category,
                    Location = newKnownEquipment.Location,
                    SerialNumber = newKnownEquipment.SerialNumber,
                    Model = newKnownEquipment.Model,
                    CatalogID = newKnownEquipment.CatelogID,
                    Symptomid = newKnownEquipment.SymptomID,
                };
                workOrder.WorkorderEquipmentRequesteds.Add(equipment);
            }
            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
        }
        public void SaveClosureAssets(WorkorderManagementModel workorderManagement, WorkOrder workOrder, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            IEnumerable<WorkorderEquipment> workOrderEquipments = FarmerBrothersEntitites.WorkorderEquipments.Where(we => we.WorkorderID == workOrder.WorkorderID);

            if (workOrderEquipments != null)
            {
                for (int count = workOrderEquipments.Count() - 1; count >= 0; count--)
                {
                    WorkorderEquipment equipment = workOrderEquipments.ElementAt(count);

                    WorkOrderManagementEquipmentModel equipmentFromModel = workorderManagement.Closure.WorkOrderEquipments.Where(e => e.AssetId == equipment.Assetid).FirstOrDefault();
                    if (equipmentFromModel != null)
                    {
                        equipment.WorkorderID = workOrder.WorkorderID;
                        equipment.CallTypeid = equipmentFromModel.CallTypeID;
                        equipment.Category = equipmentFromModel.Category;
                        equipment.Manufacturer = equipmentFromModel.Manufacturer;
                        equipment.Model = equipmentFromModel.Model;
                        equipment.Location = equipmentFromModel.Location;

                        if (equipmentFromModel.SerialNumber.ToLower() != "other")
                        {
                            equipment.SerialNumber = equipmentFromModel.SerialNumber;
                        }
                        else
                        {
                            equipment.SerialNumber = equipmentFromModel.SerialNumberManual;
                        }

                        equipment.Solutionid = equipmentFromModel.Solution;
                        equipment.FeastMovementid = equipmentFromModel.FeastMovementId;

                        if (equipment.CallTypeid == 1600)
                        {
                            if (equipment.WorkorderInstallationSurveys != null && equipment.WorkorderInstallationSurveys.Count > 0)
                            {
                                WorkorderInstallationSurvey survey = equipment.WorkorderInstallationSurveys.ElementAt(0);
                                if (survey != null)
                                {
                                    survey.AssetLocation = equipmentFromModel.AssetLocation;
                                    survey.Comments = equipmentFromModel.Comments;
                                    survey.CounterUnitSpace = equipmentFromModel.CounterUnitSpace;
                                    survey.ElectricalPhase = equipmentFromModel.ElectricalPhase;
                                    survey.MachineAmperage = equipmentFromModel.MachineAmperage;
                                    survey.NemwNumber = equipmentFromModel.NemwNumber;
                                    survey.UnitFitSpace = equipmentFromModel.UnitFitSpace;
                                    survey.Voltage = equipmentFromModel.Voltage;
                                    survey.WaterLine = equipmentFromModel.WaterLine;
                                    survey.WorkorderID = workOrder.WorkorderID;
                                }
                            }
                            else
                            {
                                WorkorderInstallationSurvey survey = new WorkorderInstallationSurvey()
                                {
                                    AssetLocation = equipmentFromModel.AssetLocation,
                                    Comments = equipmentFromModel.Comments,
                                    CounterUnitSpace = equipmentFromModel.CounterUnitSpace,
                                    ElectricalPhase = equipmentFromModel.ElectricalPhase,
                                    MachineAmperage = equipmentFromModel.MachineAmperage,
                                    NemwNumber = equipmentFromModel.NemwNumber,
                                    UnitFitSpace = equipmentFromModel.UnitFitSpace,
                                    Voltage = equipmentFromModel.Voltage,
                                    WaterLine = equipmentFromModel.WaterLine,
                                    WorkorderID = workOrder.WorkorderID,
                                    AssetID = equipmentFromModel.AssetId
                                };
                                equipment.WorkorderInstallationSurveys.Add(survey);
                            }

                            equipment.Temperature = "";
                            equipment.Weight = "";
                            equipment.Ratio = "";
                            equipment.Settings = "";
                            equipment.WorkPerformedCounter = "";
                            equipment.WorkDescription = "";
                            equipment.Systemid = null;
                            equipment.Symptomid = null;
                            equipment.Email = "";
                        }
                        else
                        {
                            if (equipment.WorkorderInstallationSurveys != null && equipment.WorkorderInstallationSurveys.Count > 0)
                            {
                                equipment.WorkorderInstallationSurveys.Clear();
                            }

                            equipment.Temperature = equipmentFromModel.Temperature;
                            equipment.Weight = equipmentFromModel.Weight;
                            equipment.Ratio = equipmentFromModel.Ratio;
                            equipment.Settings = equipmentFromModel.Settings;
                            equipment.WorkPerformedCounter = equipmentFromModel.Counter;
                            equipment.WorkDescription = equipmentFromModel.WorkPerformed;
                            equipment.Systemid = equipmentFromModel.System;
                            equipment.Symptomid = equipmentFromModel.SymptomID;
                            equipment.Solutionid = equipmentFromModel.Solution;
                            equipment.Email = equipmentFromModel.Email;
                            equipment.NoPartsNeeded = equipmentFromModel.NoPartsNeeded;
                        }
                        SaveClosureParts(equipmentFromModel, workOrder, equipment, FarmerBrothersEntitites);
                    }
                    else
                    {
                        FarmerBrothersEntitites.WorkorderEquipments.Remove(equipment);
                    }
                }
            }

            IList<WorkOrderManagementEquipmentModel> newKnownEquipments = workorderManagement.Closure.WorkOrderEquipments.Where(e => e.AssetId < 1000).ToList();
            IndexCounter counter = Utility.GetIndexCounter("AssetID", newKnownEquipments.Count);
            foreach (WorkOrderManagementEquipmentModel newKnownEquipment in newKnownEquipments)
            {
                counter.IndexValue++;

                string SNO = "";
                if (newKnownEquipment.SerialNumber.ToLower() != "other")
                {
                    SNO = newKnownEquipment.SerialNumber;
                }
                else
                {
                    SNO = newKnownEquipment.SerialNumberManual;
                }

                WorkorderEquipment equipment = new WorkorderEquipment()
                {
                    WorkorderID = workOrder.WorkorderID,
                    Assetid = counter.IndexValue.Value,
                    CallTypeid = newKnownEquipment.CallTypeID,
                    Category = newKnownEquipment.Category,
                    Manufacturer = newKnownEquipment.Manufacturer,
                    Model = newKnownEquipment.Model,
                    Location = newKnownEquipment.Location,
                    SerialNumber = SNO,
                    Solutionid = newKnownEquipment.Solution,
                    FeastMovementid = newKnownEquipment.FeastMovementId
                };

                if (equipment.CallTypeid == 1600)
                {
                    WorkorderInstallationSurvey survey = new WorkorderInstallationSurvey()
                    {
                        AssetLocation = newKnownEquipment.AssetLocation,
                        Comments = newKnownEquipment.Comments,
                        CounterUnitSpace = newKnownEquipment.CounterUnitSpace,
                        ElectricalPhase = newKnownEquipment.ElectricalPhase,
                        MachineAmperage = newKnownEquipment.MachineAmperage,
                        NemwNumber = newKnownEquipment.NemwNumber,
                        UnitFitSpace = newKnownEquipment.UnitFitSpace,
                        Voltage = newKnownEquipment.Voltage,
                        WaterLine = newKnownEquipment.WaterLine,
                        WorkorderID = workOrder.WorkorderID,
                        AssetID = newKnownEquipment.AssetId
                    };
                    equipment.WorkorderInstallationSurveys.Add(survey);
                }
                else
                {
                    equipment.Temperature = newKnownEquipment.Temperature;
                    equipment.Weight = newKnownEquipment.Weight;
                    equipment.Ratio = newKnownEquipment.Ratio;
                    equipment.Settings = newKnownEquipment.Settings;
                    equipment.WorkPerformedCounter = newKnownEquipment.Counter;
                    equipment.WorkDescription = newKnownEquipment.WorkPerformed;
                    equipment.Email = newKnownEquipment.Email;
                    equipment.Systemid = newKnownEquipment.System;
                    equipment.Symptomid = newKnownEquipment.SymptomID;
                    equipment.NoPartsNeeded = newKnownEquipment.NoPartsNeeded;
                }

                FarmerBrothersEntitites.WorkorderEquipments.Add(equipment);
                SaveClosureParts(newKnownEquipment, workOrder, equipment, FarmerBrothersEntitites);
            }

            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
        }
        private void SaveClosureParts(WorkOrderManagementEquipmentModel equipmentFromModel, WorkOrder workOrder, WorkorderEquipment equipment, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            IEnumerable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.AssetID == equipment.Assetid);
            for (int partCount = workOrderParts.Count() - 1; partCount >= 0; partCount--)
            {
                WorkorderPart workOrderPart = workOrderParts.ElementAt(partCount);
                WorkOrderPartModel partModel = equipmentFromModel.Parts.Where(wp => wp.PartsIssueid == workOrderPart.PartsIssueid).FirstOrDefault();
                if (partModel != null)
                {
                    workOrderPart.PartReplenish = partModel.PartReplenish;
                    workOrderPart.Quantity = partModel.Quantity;
                    workOrderPart.Manufacturer = partModel.Manufacturer;
                    workOrderPart.Sku = partModel.Sku;
                    workOrderPart.Description = partModel.Description;
                    workOrderPart.NonSerializedIssue = partModel.Issue;
                    workOrderPart.WorkorderID = workOrder.WorkorderID;
                }
                else
                {
                    FarmerBrothersEntitites.WorkorderParts.Remove(workOrderPart);
                }
            }

            if (equipmentFromModel.Parts != null)
            {
                IList<WorkOrderPartModel> newParts = equipmentFromModel.Parts.Where(e => e.PartsIssueid == null).ToList();
                foreach (WorkOrderPartModel newPart in newParts)
                {
                    WorkorderPart part = new WorkorderPart()
                    {
                        AssetID = equipment.Assetid,
                        PartReplenish = newPart.PartReplenish,
                        Quantity = newPart.Quantity,
                        Manufacturer = newPart.Manufacturer,
                        Sku = newPart.Sku,
                        Description = newPart.Description,
                        NonSerializedIssue = newPart.Issue,
                        WorkorderID = workOrder.WorkorderID
                    };

                    FarmerBrothersEntitites.WorkorderParts.Add(part);
                }
                FarmerBrothersEntitites.SaveChanges();
            }
        }

        private void CreateInvoice(WorkorderManagementModel workOrderManagement, WorkOrder workOrder, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0 && ws.AssignedStatus == "Accepted").FirstOrDefault();

            if (schedule != null)
            {
                TechHierarchyView techView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, schedule.Techid.Value);
                if (techView != null)
                {
                    //LG : TODO : Need to modify DistributorName with correct values, 
                    if (string.Compare(techView.DistributorName, "TPSP Branch", true) == 0)
                    {
                        IndexCounter workOrderCounter = Utility.GetIndexCounter("InvoiceID", 1);
                        workOrderCounter.IndexValue++;
                        //FarmerBrothersEntitites.Entry(workOrderCounter).State = System.Data.Entity.EntityState.Modified;

                        WorkorderDetail spawnWorkOrderDetail = new WorkorderDetail();
                        if (workOrder.WorkorderDetails.Count > 0)
                        {
                            WorkorderDetail workOrderDetail = workOrder.WorkorderDetails.ElementAt(0);

                            if (workOrderDetail != null)
                            {
                                double travelTime = 0L;
                                if (!string.IsNullOrWhiteSpace(workOrderDetail.TravelTime))
                                {
                                    string[] travelTimes = workOrderDetail.TravelTime.Split(':');
                                    if (travelTimes.Count() >= 2)
                                    {
                                        int hours = string.IsNullOrWhiteSpace(travelTimes[0]) ? 0 : Convert.ToInt32(travelTimes[0]);
                                        int min = string.IsNullOrWhiteSpace(travelTimes[1]) ? 0 : Convert.ToInt32(travelTimes[1]);
                                        int sec = 0;

                                        TimeSpan travelTimeSpan = new TimeSpan(hours, min, sec);
                                        travelTime = travelTimeSpan.TotalSeconds;
                                    }
                                }

                                Invoice invoice = new Invoice()
                                {
                                    Invoiceid = workOrderCounter.IndexValue.Value.ToString(),
                                    WorkorderID = workOrder.WorkorderID,
                                    CustomerName = workOrder.CustomerName,
                                    ServiceLocation = schedule.ServiceCenterName,
                                    WorkorderCompletionDate = workOrderDetail.CompletionDateTime,
                                    Mileage = Convert.ToInt32(workOrderDetail.Mileage),
                                    TravelTimeInSecs = Convert.ToInt32(travelTime),
                                    InvoiceStatus = @"Awaiting Submission"
                                };
                                FarmerBrothersEntitites.Invoices.Add(invoice);
                            }
                        }
                    }
                }
            }
        }
        public int CreatePartsOrder(WorkorderManagementModel workorderManagement, WorkOrder workOrder, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            int effectedRecords = 0;
            if (workorderManagement.WorkOrderParts.Count > 0)
            {
                if (workOrder.WorkorderSchedules != null && workOrder.WorkorderSchedules.Count > 0)
                {
                    WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0 && ws.AssignedStatus == "Accepted").FirstOrDefault();
                    if (schedule != null)
                    {
                        int custId = Convert.ToInt32(schedule.Techid);

                        Customer serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == custId).FirstOrDefault();

                        WorkOrder newPartsOrder = workorderManagement.FillCustomerData(new WorkOrder(), true, FarmerBrothersEntitites, serviceCustomer);
                        newPartsOrder.EntryUserName = UserName;

                        using (FarmerBrothersEntities newEntity = new FarmerBrothersEntities())
                        {
                            IndexCounter counter = Utility.GetIndexCounter("WorkorderID", 1);
                            counter.IndexValue++;
                            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                            newPartsOrder.WorkorderID = counter.IndexValue.Value;
                            newPartsOrder.WorkorderSpawnEvent = 1;

                            newPartsOrder.WorkorderCalltypeid = 1820;
                            newPartsOrder.WorkorderCalltypeDesc = "Parts Request - Manual";
                            newPartsOrder.FollowupCallID = defaultFollowUpCall;

                            newPartsOrder.WorkorderEntryDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                            newPartsOrder.WorkorderModifiedDate = newPartsOrder.WorkorderEntryDate;
                            newPartsOrder.ModifiedUserName = UserName;
                            newPartsOrder.WorkorderCallstatus = "Closed";
                            newPartsOrder.ClosedUserName = UserName;
                            newPartsOrder.WorkorderCloseDate = newPartsOrder.WorkorderEntryDate;

                            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = @"Closed Parts Order Created from “MARS” by " + UserName,
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                                UserName = UserName,
                                isDispatchNotes = 0
                            };
                            newPartsOrder.NotesHistories.Add(notesHistory);

                            SaveWorkOrderParts(workorderManagement, newPartsOrder);

                            string message = "";
                            
                            newEntity.WorkOrders.Add(newPartsOrder);
                            effectedRecords = newEntity.SaveChanges();
                        }
                    }
                }
            }
            return effectedRecords > 0 ? 1 : 0;
        }
        private void SaveWorkOrderParts(WorkorderManagementModel workorderManagement, WorkOrder workOrder)
        {
            switch (workorderManagement.PartsShipTo)
            {
                case 1:
                    if (workorderManagement.IsBranchAlternateAddress)
                    {
                        workOrder.PartsShipTo = "Other Local Branch";

                        workOrder.OtherPartsName = workorderManagement.BranchOtherPartsName;
                        workOrder.OtherPartsContactName = workorderManagement.BranchOtherPartsContactName;
                        workOrder.OtherPartsAddress1 = workorderManagement.BranchOtherPartsAddress1;
                        workOrder.OtherPartsAddress2 = workorderManagement.BranchOtherPartsAddress2;
                        workOrder.OtherPartsCity = workorderManagement.BranchOtherPartsCity;
                        workOrder.OtherPartsState = workorderManagement.BranchOtherPartsState;
                        workOrder.OtherPartsZip = workorderManagement.BranchOtherPartsZip;
                        workOrder.OtherPartsPhone = workorderManagement.BranchOtherPartsPhone;
                    }
                    else
                    {
                        workOrder.PartsShipTo = "Local Branch";
                    }

                    break;
                case 2:
                    if (workorderManagement.IsCustomerAlternateAddress == true)
                    {
                        workOrder.PartsShipTo = "Other Customer";

                        workOrder.OtherPartsName = workorderManagement.CustomerOtherPartsName;
                        workOrder.OtherPartsContactName = workorderManagement.CustomerOtherPartsContactName;
                        workOrder.OtherPartsAddress1 = workorderManagement.CustomerOtherPartsAddress1;
                        workOrder.OtherPartsAddress2 = workorderManagement.CustomerOtherPartsAddress2;
                        workOrder.OtherPartsCity = workorderManagement.CustomerOtherPartsCity;
                        workOrder.OtherPartsState = workorderManagement.CustomerOtherPartsState;
                        workOrder.OtherPartsZip = workorderManagement.CustomerOtherPartsZip;
                        workOrder.OtherPartsPhone = workorderManagement.CustomerOtherPartsPhone;
                    }
                    else
                    {
                        workOrder.PartsShipTo = "Customer";
                    }
                    break;

            }

            IEnumerable<WorkorderPart> workOrderParts = FarmerBrothersEntitites.WorkorderParts.Where(wp => wp.WorkorderID == workOrder.WorkorderID);

            if (workOrderParts != null)
            {
                for (int count = workOrderParts.Count() - 1; count >= 0; count--)
                {
                    WorkorderPart workOrderPart = workOrderParts.ElementAt(count);

                    WorkOrderPartModel workOrderPartModel = workorderManagement.WorkOrderParts.Where(e => e.PartsIssueid == workOrderPart.PartsIssueid).FirstOrDefault();
                    if (workOrderPartModel != null)
                    {
                        FarmerBrothersEntitites.WorkorderParts.Remove(workOrderPart);
                    }

                }
            }

            IList<WorkOrderPartModel> newParts = workorderManagement.WorkOrderParts.Where(e => e.PartsIssueid < 100).ToList();
            foreach (WorkOrderPartModel newPart in newParts)
            {
                WorkorderPart part = new WorkorderPart()
                {
                    Quantity = newPart.Quantity,
                    Manufacturer = newPart.Manufacturer,
                    Sku = newPart.Sku,
                    Description = newPart.Description
                };

                workOrder.WorkorderParts.Add(part);
            }
        }

        /*private void CreateSpawnWorkOrder(WorkorderManagementModel workorderManagement, WorkorderEquipment equipment, FarmerBrothersEntities newEntity, out string message)
        {
            WorkOrder workOrder = newEntity.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);
            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, newEntity);

            WorkOrder spawnWorkOrder = new WorkOrder();
            List<Type> collections = new List<Type>() { typeof(IEnumerable<>), typeof(IEnumerable) };

            int? responsibleTechId = null;

            foreach (var property in workOrder.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(string) || !property.PropertyType.GetInterfaces().Any(i => collections.Any(c => i == c)))
                {
                    property.SetValue(spawnWorkOrder, property.GetValue(workOrder));
                }
            }

            

            IndexCounter workOrderCounter = Utility.GetIndexCounter("WorkorderID", 1);
            workOrderCounter.IndexValue++;
            //newEntity.Entry(workOrderCounter).State = System.Data.Entity.EntityState.Modified;

            spawnWorkOrder.WorkorderID = workOrderCounter.IndexValue.Value;
            spawnWorkOrder.WorkorderEntryDate = currentTime;
            spawnWorkOrder.WorkorderCallstatus = "Open";
            spawnWorkOrder.WorkorderSpawnEvent = 1;
            spawnWorkOrder.WorkorderCloseDate = null;
            if (workOrder.OriginalWorkorderid.HasValue)
            {
                spawnWorkOrder.OriginalWorkorderid = workOrder.OriginalWorkorderid;
            }
            else
            {
                spawnWorkOrder.OriginalWorkorderid = workOrder.WorkorderID;
            }

            spawnWorkOrder.ParentWorkorderid = workOrder.WorkorderID;
            if (workOrder.SpawnCounter.HasValue)
            {
                spawnWorkOrder.SpawnCounter = workOrder.SpawnCounter.Value + 1;
            }
            else
            {
                spawnWorkOrder.SpawnCounter = 1;
            }

            WorkorderDetail spawnWorkOrderDetail = new WorkorderDetail();
            if (workOrder.WorkorderDetails.Count > 0)
            {
                WorkorderDetail workOrderDetail = workOrder.WorkorderDetails.ElementAt(0);
                foreach (var property in workOrderDetail.GetType().GetProperties())
                {
                    if (property.GetValue(workOrderDetail) != null && property.GetValue(workOrderDetail).GetType() != null && (property.GetValue(workOrderDetail).GetType().IsValueType || property.GetValue(workOrderDetail).GetType() == typeof(string)))
                    {
                        property.SetValue(spawnWorkOrderDetail, property.GetValue(workOrderDetail));
                    }
                }
                spawnWorkOrderDetail.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnWorkOrderDetail.ArrivalDateTime = null;
                spawnWorkOrderDetail.CompletionDateTime = null;
                spawnWorkOrderDetail.StartDateTime = null;
                spawnWorkOrderDetail.EntryDate = null;
                spawnWorkOrderDetail.ModifiedDate = null;
                spawnWorkOrderDetail.SpecialClosure = "";
                spawnWorkOrderDetail.TravelTime = "";
                spawnWorkOrderDetail.SolutionId = equipment.Solutionid;

                spawnWorkOrder.WorkorderDetails.Add(spawnWorkOrderDetail);
            }
            
            foreach (WorkOrderBrand brand in workOrder.WorkOrderBrands)
            {
                WorkOrderBrand newBrand = new WorkOrderBrand();
                foreach (var property in brand.GetType().GetProperties())
                {
                    if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                    {
                        property.SetValue(newBrand, property.GetValue(brand));
                    }
                }
                newBrand.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnWorkOrder.WorkOrderBrands.Add(newBrand);
            }

            foreach (NotesHistory notes in workOrder.NotesHistories)
            {
                NotesHistory newNotes = new NotesHistory();
                foreach (var property in notes.GetType().GetProperties())
                {
                    if (property.GetValue(notes) != null && property.GetValue(notes).GetType() != null && (property.GetValue(notes).GetType().IsValueType || property.GetValue(notes).GetType() == typeof(string)))
                    {
                        property.SetValue(newNotes, property.GetValue(notes));
                    }
                }
                newNotes.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnWorkOrder.NotesHistories.Add(newNotes);
            }

            NotesHistory notesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = currentTime,
                Notes = @"Work Order spawned in MARS from work order " + workOrder.WorkorderID,
                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                UserName = UserName != null ? UserName : Convert.ToString(System.Web.HttpContext.Current.Session["UserName"])
            };
            spawnWorkOrder.NotesHistories.Add(notesHistory);

            NotesHistory WONotesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = currentTime,
                Notes = @"Workorder " + spawnWorkOrder.WorkorderID + " spawned due to Solution Code " + equipment.Solutionid,
                Userid = 1234, //TBD
                UserName = UserName != null ? UserName : Convert.ToString(System.Web.HttpContext.Current.Session["UserName"])
            };
            workOrder.NotesHistories.Add(WONotesHistory);

            foreach (WorkorderReasonlog reasonLog in workOrder.WorkorderReasonlogs)
            {
                WorkorderReasonlog newReasonLog = new WorkorderReasonlog();
                foreach (var property in reasonLog.GetType().GetProperties())
                {
                    if (property.GetValue(reasonLog) != null && property.GetValue(reasonLog).GetType() != null && (property.GetValue(reasonLog).GetType().IsValueType || property.GetValue(reasonLog).GetType() == typeof(string)))
                    {
                        property.SetValue(newReasonLog, property.GetValue(reasonLog));
                    }
                }
                newReasonLog.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnWorkOrder.WorkorderReasonlogs.Add(newReasonLog);
            }

            WorkorderType newWorkOrderType = newEntity.WorkorderTypes.Where(wt => wt.CallTypeID == 1310).FirstOrDefault();

            if (equipment.Solutionid == 5160 && newWorkOrderType != null)
            {
                spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;
            }
            else
            {
                spawnWorkOrder.WorkorderCalltypeid = equipment.CallTypeid;
                WorkorderType workOrderType = newEntity.WorkorderTypes.Where(wt => wt.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                if (workOrderType != null)
                {
                    spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                }
            }

            if (equipment.Solutionid == 5160 || equipment.Solutionid == 5191)
            {
                if (!string.IsNullOrWhiteSpace(workorderManagement.SpawnReason))
                {
                    spawnWorkOrderDetail.SpawnReason = Convert.ToInt32(workorderManagement.SpawnReason);
                }

                notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 0,
                    EntryDate = currentTime,
                    Notes = workorderManagement.SpawnNotes,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                    UserName = UserName != null ? UserName : Convert.ToString(System.Web.HttpContext.Current.Session["UserName"])
                };
                spawnWorkOrder.NotesHistories.Add(notesHistory);
            }
            if (equipment.Solutionid == 9999)
            {
                if (!string.IsNullOrWhiteSpace(workorderManagement.NSRReason))
                {
                    spawnWorkOrderDetail.NSRReason = Convert.ToInt32(workorderManagement.NSRReason);
                }

                notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 0,
                    EntryDate = currentTime,
                    Notes = workorderManagement.NSRNotes,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                    UserName = UserName != null ? UserName : Convert.ToString(System.Web.HttpContext.Current.Session["UserName"])
                };
                spawnWorkOrder.NotesHistories.Add(notesHistory);
            }

            WorkorderEquipment spawnEquipment = new WorkorderEquipment();
            WorkorderEquipmentRequested spawnEquipmentRequested = new WorkorderEquipmentRequested();

            spawnEquipment.Assetid = equipment.Assetid;
            spawnEquipment.CallTypeid = equipment.CallTypeid;
            spawnEquipment.CatalogID = equipment.CatalogID;
            spawnEquipment.Category = equipment.Category;
            spawnEquipment.EquipmentId = equipment.EquipmentId;
            spawnEquipment.FeastMovementid = equipment.FeastMovementid;
            spawnEquipment.IsSlNumberImageExist = equipment.IsSlNumberImageExist;
            spawnEquipment.Location = equipment.Location;
            spawnEquipment.Manufacturer = equipment.Manufacturer;
            spawnEquipment.Model = equipment.Model;
            spawnEquipment.Name = equipment.Name;
            spawnEquipment.QualityIssue = equipment.QualityIssue;
            spawnEquipment.SerialNumber = equipment.SerialNumber;
            spawnEquipment.WorkorderID = spawnWorkOrder.WorkorderID;
            spawnEquipment.Temperature = "";
            spawnEquipment.Weight = "";
            spawnEquipment.Ratio = "";
            spawnEquipment.Settings = "";
            spawnEquipment.WorkPerformedCounter = "";
            spawnEquipment.WorkDescription = "";
            spawnEquipment.Systemid = null;
            spawnEquipment.Symptomid = equipment.Symptomid;
            spawnEquipment.Email = "";
            spawnEquipment.NoPartsNeeded = null;
            spawnEquipment.Solutionid = null;

            spawnEquipmentRequested.Assetid = equipment.Assetid;
            spawnEquipmentRequested.CallTypeid = equipment.CallTypeid;
            spawnEquipmentRequested.CatalogID = equipment.CatalogID;
            spawnEquipmentRequested.Category = equipment.Category;
            spawnEquipmentRequested.EquipmentId = equipment.EquipmentId;
            spawnEquipmentRequested.FeastMovementid = equipment.FeastMovementid;
            spawnEquipmentRequested.Location = equipment.Location;
            spawnEquipmentRequested.Manufacturer = equipment.Manufacturer;
            spawnEquipmentRequested.Model = equipment.Model;
            spawnEquipmentRequested.Name = equipment.Name;
            spawnEquipmentRequested.QualityIssue = equipment.QualityIssue;
            spawnEquipmentRequested.SerialNumber = equipment.SerialNumber;
            spawnEquipmentRequested.WorkorderID = spawnWorkOrder.WorkorderID;
            spawnEquipmentRequested.Temperature = "";
            spawnEquipmentRequested.Weight = "";
            spawnEquipmentRequested.Weight = "";
            spawnEquipmentRequested.Settings = "";
            spawnEquipmentRequested.WorkPerformedCounter = "";
            spawnEquipmentRequested.WorkDescription = "";
            spawnEquipmentRequested.Systemid = equipment.Systemid;
            spawnEquipmentRequested.Symptomid = equipment.Symptomid;
            spawnEquipmentRequested.Email = "";
            spawnEquipmentRequested.NoPartsNeeded = equipment.NoPartsNeeded;
            spawnEquipmentRequested.Solutionid = equipment.Solutionid;

            if (equipment.Solutionid == 5160)
            {
                spawnEquipment.CallTypeid = 1310;
                spawnEquipmentRequested.CallTypeid = 1310;
            }

            IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
            assetCounter.IndexValue++;
            //newEntity.Entry(assetCounter).State = System.Data.Entity.EntityState.Modified;
            spawnEquipment.Assetid = assetCounter.IndexValue.Value;
            spawnEquipmentRequested.Assetid = assetCounter.IndexValue.Value;

            spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment);
            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested);

            if (equipment.Solutionid == 5160)
            {
                WorkorderEquipment spawnEquipment2 = new WorkorderEquipment();
                WorkorderEquipmentRequested spawnEquipmentRequested2 = new WorkorderEquipmentRequested();

                spawnEquipment2.Assetid = equipment.Assetid;
                spawnEquipment2.CallTypeid = equipment.CallTypeid;
                spawnEquipment2.CatalogID = equipment.CatalogID;
                spawnEquipment2.Category = equipment.Category;
                spawnEquipment2.EquipmentId = equipment.EquipmentId;
                spawnEquipment2.FeastMovementid = equipment.FeastMovementid;
                spawnEquipment2.IsSlNumberImageExist = equipment.IsSlNumberImageExist;
                spawnEquipment2.Location = equipment.Location;
                spawnEquipment2.Manufacturer = equipment.Manufacturer;
                spawnEquipment2.Model = equipment.Model;
                spawnEquipment2.Name = equipment.Name;
                spawnEquipment2.QualityIssue = equipment.QualityIssue;
                spawnEquipment2.SerialNumber = equipment.SerialNumber;
                spawnEquipment2.CallTypeid = 1410;
                spawnEquipment2.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnEquipment2.Temperature = "";
                spawnEquipment2.Weight = "";
                spawnEquipment2.Ratio = "";
                spawnEquipment2.Settings = "";
                spawnEquipment2.WorkPerformedCounter = "";
                spawnEquipment2.WorkDescription = "";
                spawnEquipment2.Systemid = equipment.Systemid;
                spawnEquipment2.Symptomid = equipment.Symptomid;
                spawnEquipment2.Email = "";
                spawnEquipment2.NoPartsNeeded = equipment.NoPartsNeeded;
                spawnEquipment2.Solutionid = equipment.Solutionid;

                spawnEquipmentRequested2.Assetid = equipment.Assetid;
                spawnEquipmentRequested2.CallTypeid = equipment.CallTypeid;
                spawnEquipmentRequested2.CatalogID = equipment.CatalogID;
                spawnEquipmentRequested2.Category = equipment.Category;
                spawnEquipmentRequested2.EquipmentId = equipment.EquipmentId;
                spawnEquipmentRequested2.FeastMovementid = equipment.FeastMovementid;
                spawnEquipmentRequested2.Location = equipment.Location;
                spawnEquipmentRequested2.Manufacturer = equipment.Manufacturer;
                spawnEquipmentRequested2.Model = equipment.Model;
                spawnEquipmentRequested2.Name = equipment.Name;
                spawnEquipmentRequested2.QualityIssue = equipment.QualityIssue;
                spawnEquipmentRequested2.SerialNumber = equipment.SerialNumber;
                spawnEquipmentRequested2.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnEquipmentRequested2.CallTypeid = 1410;
                spawnEquipmentRequested2.WorkorderID = spawnWorkOrder.WorkorderID;
                spawnEquipmentRequested2.Temperature = "";
                spawnEquipmentRequested2.Weight = "";
                spawnEquipmentRequested2.Ratio = "";
                spawnEquipmentRequested2.Settings = "";
                spawnEquipmentRequested2.WorkPerformedCounter = "";
                spawnEquipmentRequested2.WorkDescription = "";
                spawnEquipmentRequested2.Systemid = equipment.Systemid;
                spawnEquipmentRequested2.Symptomid = equipment.Symptomid;
                spawnEquipmentRequested2.Email = "";
                spawnEquipmentRequested2.NoPartsNeeded = equipment.NoPartsNeeded;
                spawnEquipmentRequested2.Solutionid = equipment.Solutionid;

                IndexCounter assetCounter2 = Utility.GetIndexCounter("AssetID", 1);
                assetCounter2.IndexValue++;
                //newEntity.Entry(assetCounter2).State = System.Data.Entity.EntityState.Modified;
                spawnEquipment2.Assetid = assetCounter2.IndexValue.Value;
                spawnEquipmentRequested2.Assetid = assetCounter2.IndexValue.Value;

                spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment2);
                spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested2);
            }

            newEntity.WorkOrders.Add(spawnWorkOrder);
            newEntity.SaveChanges();          

            string emailAddresses = string.Empty;


            StringBuilder subject = new StringBuilder();
            subject.Append("Spawned Workorder - Original WO: ");
            subject.Append(spawnWorkOrder.OriginalWorkorderid);
            subject.Append(" ST: ");
            subject.Append(spawnWorkOrder.CustomerState);
            subject.Append(" Call Type: ");
            subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);

            SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null, newEntity);

            if (responsibleTechId.HasValue)
            {
                subject = new StringBuilder();
                subject.Append("WO:");
                subject.Append(spawnWorkOrder.WorkorderID);
                subject.Append(" ST:");
                subject.Append(spawnWorkOrder.CustomerState);
                subject.Append(" Call Type:");
                subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);


                string emailAddress = string.Empty;
                var CustomerId = int.Parse(responsibleTechId.Value.ToString());
                Customer serviceCustomer = newEntity.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                if (serviceCustomer != null)
                {
                    emailAddress = serviceCustomer.Email;
                }

                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TestEmail"]))
                {
                    emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                }

                if (!string.IsNullOrWhiteSpace(emailAddress))
                {
                    SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null, newEntity);
                }
            }

            message = @"|Spawned Work Order " + spawnWorkOrder.WorkorderID + " is created!";           

        }
        */

        private void CreateSpawnWorkOrder1(WorkorderManagementModel workorderManagement, List<WorkorderEquipment> equipment, FarmerBrothersEntities newEntity, out string message)
    {
            List<int?> uniqueSolutionIds = equipment.Select(x => x.Solutionid).Distinct().ToList();

            WorkOrder workOrder = newEntity.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);
        DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, newEntity);
            string SpawnedWOsCreated = "";
            foreach (int soluitonId in uniqueSolutionIds)
            {
                if (soluitonId == 5115
                                            || soluitonId == 5120
                                            || soluitonId == 5130
                                            || soluitonId == 5135
                                            || soluitonId == 5140
                                            || soluitonId == 5170
                                            || soluitonId == 5171
                                            || soluitonId == 5181
                                            || soluitonId == 5191)
                {

                    List<WorkorderEquipment> workorderEqps = equipment.Where(eq => eq.Solutionid == soluitonId).ToList();
                    WorkOrder spawnWorkOrder = new WorkOrder();
                    List<Type> collections = new List<Type>() { typeof(IEnumerable<>), typeof(IEnumerable) };

                    int? responsibleTechId = null;

                    foreach (var property in workOrder.GetType().GetProperties())
                    {
                        if (property.PropertyType == typeof(string) || !property.PropertyType.GetInterfaces().Any(i => collections.Any(c => i == c)))
                        {
                            property.SetValue(spawnWorkOrder, property.GetValue(workOrder));
                        }
                    }

                    IndexCounter workOrderCounter = Utility.GetIndexCounter("WorkorderID", 1);
                    workOrderCounter.IndexValue++;
                    //newEntity.Entry(workOrderCounter).State = System.Data.Entity.EntityState.Modified;

                    spawnWorkOrder.WorkorderID = workOrderCounter.IndexValue.Value;
                    spawnWorkOrder.WorkorderEntryDate = currentTime;
                    spawnWorkOrder.WorkorderCallstatus = "Open";
                    spawnWorkOrder.WorkorderSpawnEvent = 1;
                    spawnWorkOrder.WorkorderCloseDate = null;
                    if (workOrder.OriginalWorkorderid.HasValue)
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.OriginalWorkorderid;
                    }
                    else
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.WorkorderID;
                    }

                    spawnWorkOrder.ParentWorkorderid = workOrder.WorkorderID;
                    if (workOrder.SpawnCounter.HasValue)
                    {
                        spawnWorkOrder.SpawnCounter = workOrder.SpawnCounter.Value + 1;
                    }
                    else
                    {
                        spawnWorkOrder.SpawnCounter = 1;
                    }

                    WorkorderDetail spawnWorkOrderDetail = new WorkorderDetail();
                    if (workOrder.WorkorderDetails.Count > 0)
                    {
                        WorkorderDetail workOrderDetail = workOrder.WorkorderDetails.ElementAt(0);
                        foreach (var property in workOrderDetail.GetType().GetProperties())
                        {
                            if (property.GetValue(workOrderDetail) != null && property.GetValue(workOrderDetail).GetType() != null && (property.GetValue(workOrderDetail).GetType().IsValueType || property.GetValue(workOrderDetail).GetType() == typeof(string)))
                            {
                                property.SetValue(spawnWorkOrderDetail, property.GetValue(workOrderDetail));
                            }
                        }
                        spawnWorkOrderDetail.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrderDetail.ArrivalDateTime = null;
                        spawnWorkOrderDetail.CompletionDateTime = null;
                        spawnWorkOrderDetail.StartDateTime = null;
                        spawnWorkOrderDetail.EntryDate = null;
                        spawnWorkOrderDetail.ModifiedDate = null;
                        spawnWorkOrderDetail.SpecialClosure = "";
                        spawnWorkOrderDetail.TravelTime = "";
                        spawnWorkOrderDetail.SolutionId = soluitonId;

                        spawnWorkOrder.WorkorderDetails.Add(spawnWorkOrderDetail);
                    }

                    foreach (WorkOrderBrand brand in workOrder.WorkOrderBrands)
                    {
                        WorkOrderBrand newBrand = new WorkOrderBrand();
                        foreach (var property in brand.GetType().GetProperties())
                        {
                            if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                            {
                                property.SetValue(newBrand, property.GetValue(brand));
                            }
                        }
                        newBrand.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkOrderBrands.Add(newBrand);
                    }

                    foreach (NotesHistory notes in workOrder.NotesHistories)
                    {
                        NotesHistory newNotes = new NotesHistory();
                        foreach (var property in notes.GetType().GetProperties())
                        {
                            if (property.GetValue(notes) != null && property.GetValue(notes).GetType() != null && (property.GetValue(notes).GetType().IsValueType || property.GetValue(notes).GetType() == typeof(string)))
                            {
                                property.SetValue(newNotes, property.GetValue(notes));
                            }
                        }
                        newNotes.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.NotesHistories.Add(newNotes);
                    }

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Work Order spawned in MARS from work order " + workOrder.WorkorderID,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                        UserName = UserName != null ? UserName : Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]),
                        isDispatchNotes = 0
                    };
                    spawnWorkOrder.NotesHistories.Add(notesHistory);

                    NotesHistory WONotesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Workorder " + spawnWorkOrder.WorkorderID + " spawned due to Solution Code " + soluitonId,
                        Userid = 1234, //TBD
                        UserName = UserName != null ? UserName : Convert.ToString(System.Web.HttpContext.Current.Session["UserName"])
                    };
                    workOrder.NotesHistories.Add(WONotesHistory);

                    foreach (WorkorderReasonlog reasonLog in workOrder.WorkorderReasonlogs)
                    {
                        WorkorderReasonlog newReasonLog = new WorkorderReasonlog();
                        foreach (var property in reasonLog.GetType().GetProperties())
                        {
                            if (property.GetValue(reasonLog) != null && property.GetValue(reasonLog).GetType() != null && (property.GetValue(reasonLog).GetType().IsValueType || property.GetValue(reasonLog).GetType() == typeof(string)))
                            {
                                property.SetValue(newReasonLog, property.GetValue(reasonLog));
                            }
                        }
                        newReasonLog.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkorderReasonlogs.Add(newReasonLog);
                    }

                    WorkorderType newWorkOrderType = newEntity.WorkorderTypes.Where(wt => wt.CallTypeID == 1310).FirstOrDefault();

                    if (soluitonId == 5160 && newWorkOrderType != null)
                    {
                        spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                        spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;
                    }
                    else
                    {
                        WorkorderEquipment eqp = workorderEqps.Where(e => e.CallTypeid == 1200).FirstOrDefault();
                        if (eqp != null)
                        {
                            WorkorderType workOrderType = newEntity.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                            if (workOrderType != null)
                            {
                                spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                            }
                        }
                        else
                        {
                            eqp = workorderEqps.OrderBy(equip => equip.Assetid).ElementAt(0);
                            if (eqp != null)
                            {
                                WorkorderType workOrderType = newEntity.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                                if (workOrderType != null)
                                {
                                    spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                    spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                }
                            }
                        }

                        //spawnWorkOrder.WorkorderCalltypeid = equipment.CallTypeid;
                        //WorkorderType workOrderType = newEntity.WorkorderTypes.Where(wt => wt.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                        //if (workOrderType != null)
                        //{
                        //    spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                        //}
                    }

                    if (soluitonId == 5160 || soluitonId == 5191)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.SpawnReason))
                        {
                            spawnWorkOrderDetail.SpawnReason = Convert.ToInt32(workorderManagement.SpawnReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.SpawnNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                            UserName = UserName != null ? UserName : Convert.ToString(System.Web.HttpContext.Current.Session["UserName"])
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }
                    if (soluitonId == 9999)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.NSRReason))
                        {
                            spawnWorkOrderDetail.NSRReason = Convert.ToInt32(workorderManagement.NSRReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.NSRNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                            UserName = UserName != null ? UserName : Convert.ToString(System.Web.HttpContext.Current.Session["UserName"])
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }

                    foreach (WorkorderEquipment eqpItem in workorderEqps)
                    {
                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "SpawnedEquipment : SerialNumber - " + eqpItem.SerialNumber + ", Description - " + eqpItem.WorkDescription,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);

                        WorkorderEquipment spawnEquipment = new WorkorderEquipment();
                        WorkorderEquipmentRequested spawnEquipmentRequested = new WorkorderEquipmentRequested();
                        WorkorderEquipment workOrderReq = newEntity.WorkorderEquipments.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();

                        spawnEquipment.Assetid = eqpItem.Assetid;
                        spawnEquipment.CallTypeid = eqpItem.CallTypeid;
                        spawnEquipment.CatalogID = eqpItem.CatalogID;
                        spawnEquipment.Category = eqpItem.Category;
                        spawnEquipment.EquipmentId = eqpItem.EquipmentId;
                        spawnEquipment.FeastMovementid = eqpItem.FeastMovementid;
                        spawnEquipment.IsSlNumberImageExist = eqpItem.IsSlNumberImageExist;
                        spawnEquipment.Location = eqpItem.Location;
                        spawnEquipment.Manufacturer = eqpItem.Manufacturer;
                        spawnEquipment.Model = eqpItem.Model;
                        spawnEquipment.Name = eqpItem.Name;
                        spawnEquipment.QualityIssue = eqpItem.QualityIssue;
                        spawnEquipment.SerialNumber = eqpItem.SerialNumber;
                        spawnEquipment.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnEquipment.Temperature = "";
                        spawnEquipment.Weight = "";
                        spawnEquipment.Ratio = "";
                        spawnEquipment.Settings = "";
                        spawnEquipment.WorkPerformedCounter = "";
                        spawnEquipment.WorkDescription = "";
                        spawnEquipment.Systemid = null;
                        spawnEquipment.Symptomid = eqpItem.Symptomid;
                        spawnEquipment.Email = "";
                        spawnEquipment.NoPartsNeeded = null;
                        spawnEquipment.Solutionid = null;

                        spawnEquipmentRequested.Assetid = eqpItem.Assetid;
                        spawnEquipmentRequested.CallTypeid = eqpItem.CallTypeid;
                        spawnEquipmentRequested.CatalogID = eqpItem.CatalogID;
                        spawnEquipmentRequested.Category = eqpItem.Category;
                        spawnEquipmentRequested.EquipmentId = eqpItem.EquipmentId;
                        spawnEquipmentRequested.FeastMovementid = eqpItem.FeastMovementid;
                        spawnEquipmentRequested.Location = eqpItem.Location;
                        spawnEquipmentRequested.Manufacturer = eqpItem.Manufacturer;
                        spawnEquipmentRequested.Model = eqpItem.Model;
                        spawnEquipmentRequested.Name = eqpItem.Name;
                        spawnEquipmentRequested.QualityIssue = eqpItem.QualityIssue;
                        spawnEquipmentRequested.SerialNumber = eqpItem.SerialNumber;
                        spawnEquipmentRequested.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnEquipmentRequested.Temperature = "";
                        spawnEquipmentRequested.Weight = "";
                        spawnEquipmentRequested.Weight = "";
                        spawnEquipmentRequested.Settings = "";
                        spawnEquipmentRequested.WorkPerformedCounter = "";
                        spawnEquipmentRequested.WorkDescription = "";
                        spawnEquipmentRequested.Systemid = eqpItem.Systemid;
                        spawnEquipmentRequested.Symptomid = eqpItem.Symptomid;
                        spawnEquipmentRequested.Email = "";
                        spawnEquipmentRequested.NoPartsNeeded = eqpItem.NoPartsNeeded;
                        spawnEquipmentRequested.Solutionid = eqpItem.Solutionid;

                        if (soluitonId == 5160)
                        {
                            spawnEquipment.CallTypeid = 1310;
                            spawnEquipmentRequested.CallTypeid = 1310;
                        }

                        IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
                        assetCounter.IndexValue++;
                        //newEntity.Entry(assetCounter).State = System.Data.Entity.EntityState.Modified;
                        spawnEquipment.Assetid = assetCounter.IndexValue.Value;
                        spawnEquipmentRequested.Assetid = assetCounter.IndexValue.Value;

                        spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment);
                        spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested);

                        if (soluitonId == 5160)
                        {
                            WorkorderEquipment spawnEquipment2 = new WorkorderEquipment();
                            WorkorderEquipmentRequested spawnEquipmentRequested2 = new WorkorderEquipmentRequested();

                            spawnEquipment2.Assetid = eqpItem.Assetid;
                            spawnEquipment2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipment2.CatalogID = eqpItem.CatalogID;
                            spawnEquipment2.Category = eqpItem.Category;
                            spawnEquipment2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipment2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipment2.IsSlNumberImageExist = eqpItem.IsSlNumberImageExist;
                            spawnEquipment2.Location = eqpItem.Location;
                            spawnEquipment2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipment2.Model = eqpItem.Model;
                            spawnEquipment2.Name = eqpItem.Name;
                            spawnEquipment2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipment2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipment2.CallTypeid = 1410;
                            spawnEquipment2.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipment2.Temperature = "";
                            spawnEquipment2.Weight = "";
                            spawnEquipment2.Ratio = "";
                            spawnEquipment2.Settings = "";
                            spawnEquipment2.WorkPerformedCounter = "";
                            spawnEquipment2.WorkDescription = "";
                            spawnEquipment2.Systemid = eqpItem.Systemid;
                            spawnEquipment2.Symptomid = eqpItem.Symptomid;
                            spawnEquipment2.Email = "";
                            spawnEquipment2.NoPartsNeeded = eqpItem.NoPartsNeeded;
                            spawnEquipment2.Solutionid = eqpItem.Solutionid;

                            spawnEquipmentRequested2.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipmentRequested2.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested2.Category = eqpItem.Category;
                            spawnEquipmentRequested2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested2.Location = eqpItem.Location;
                            spawnEquipmentRequested2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested2.Model = eqpItem.Model;
                            spawnEquipmentRequested2.Name = eqpItem.Name;
                            spawnEquipmentRequested2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested2.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipmentRequested2.CallTypeid = 1410;
                            spawnEquipmentRequested2.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipmentRequested2.Temperature = "";
                            spawnEquipmentRequested2.Weight = "";
                            spawnEquipmentRequested2.Ratio = "";
                            spawnEquipmentRequested2.Settings = "";
                            spawnEquipmentRequested2.WorkPerformedCounter = "";
                            spawnEquipmentRequested2.WorkDescription = "";
                            spawnEquipmentRequested2.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested2.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested2.Email = "";
                            spawnEquipmentRequested2.NoPartsNeeded = eqpItem.NoPartsNeeded;
                            spawnEquipmentRequested2.Solutionid = eqpItem.Solutionid;

                            IndexCounter assetCounter2 = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter2.IndexValue++;
                            //newEntity.Entry(assetCounter2).State = System.Data.Entity.EntityState.Modified;
                            spawnEquipment2.Assetid = assetCounter2.IndexValue.Value;
                            spawnEquipmentRequested2.Assetid = assetCounter2.IndexValue.Value;

                            spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment2);
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested2);
                        }
                    }
                    newEntity.WorkOrders.Add(spawnWorkOrder);
                    newEntity.SaveChanges();

                    string emailAddresses = string.Empty;


                    StringBuilder subject = new StringBuilder();
                    subject.Append("Spawned Workorder - Original WO: ");
                    subject.Append(spawnWorkOrder.OriginalWorkorderid);
                    subject.Append(" ST: ");
                    subject.Append(spawnWorkOrder.CustomerState);
                    subject.Append(" Call Type: ");
                    subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);

                    SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null, newEntity);

                    if (responsibleTechId.HasValue)
                    {
                        subject = new StringBuilder();
                        subject.Append("WO:");
                        subject.Append(spawnWorkOrder.WorkorderID);
                        subject.Append(" ST:");
                        subject.Append(spawnWorkOrder.CustomerState);
                        subject.Append(" Call Type:");
                        subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);


                        string emailAddress = string.Empty;
                        var CustomerId = int.Parse(responsibleTechId.Value.ToString());
                        Customer serviceCustomer = newEntity.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                        if (serviceCustomer != null)
                        {
                            emailAddress = serviceCustomer.Email;
                        }

                        if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TestEmail"]))
                        {
                            emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                        }

                        if (!string.IsNullOrWhiteSpace(emailAddress))
                        {
                            SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null, newEntity);
                        }
                    }

                    if (string.IsNullOrEmpty(SpawnedWOsCreated))
                    {
                        SpawnedWOsCreated += spawnWorkOrder.WorkorderID;
                    }
                    else
                    {
                        SpawnedWOsCreated += ", " + spawnWorkOrder.WorkorderID;
                    }
                }
            }

            if (!string.IsNullOrEmpty(SpawnedWOsCreated))
            {
                NotesHistory WONotesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = @"Spawned Work Order " + SpawnedWOsCreated + " created in MARS ",
                    Userid = 1234, //TBD
                    UserName = UserName
                };
                message = @"Spawned Work Order " + SpawnedWOsCreated + " created!";
            }
            else
            {
                message = @"";
            }
        }

        private void CreateSpawnWorkOrder(WorkorderManagementModel workorderManagement, List<WorkorderEquipment> equipment, FarmerBrothersEntities newEntity, out string message)
        {
            List<int?> uniqueSolutionIds = equipment.Select(x => x.Solutionid).Distinct().ToList();

            WorkOrder workOrder = newEntity.WorkOrders.FirstOrDefault(w => w.WorkorderID == workorderManagement.WorkOrder.WorkorderID);
            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, newEntity);

            string SpawnedWOsCreated = "";
            foreach (int soluitonId in uniqueSolutionIds)
            {
                if (soluitonId == 5115
                 || soluitonId == 5120
                 || soluitonId == 5130
                 || soluitonId == 5135
                 || soluitonId == 5140
                 || soluitonId == 5170
                 || soluitonId == 5171
                 || soluitonId == 5181
                 || soluitonId == 5191)
                {

                    List<WorkorderEquipment> workorderEqps = equipment.Where(eq => eq.Solutionid == soluitonId).ToList();

                    WorkOrder spawnWorkOrder = new WorkOrder();
                    List<Type> collections = new List<Type>() { typeof(IEnumerable<>), typeof(IEnumerable) };

                    int? responsibleTechId = null;

                    foreach (var property in workOrder.GetType().GetProperties())
                    {
                        if (property.PropertyType == typeof(string) || !property.PropertyType.GetInterfaces().Any(i => collections.Any(c => i == c)))
                        {
                            property.SetValue(spawnWorkOrder, property.GetValue(workOrder));
                        }
                    }

                    IndexCounter workOrderCounter = Utility.GetIndexCounter("WorkorderID", 1);
                    workOrderCounter.IndexValue++;

                    spawnWorkOrder.WorkorderID = workOrderCounter.IndexValue.Value;
                    spawnWorkOrder.WorkorderEntryDate = currentTime;
                    spawnWorkOrder.WorkorderCallstatus = "Open";
                    spawnWorkOrder.WorkorderSpawnEvent = 1;
                    spawnWorkOrder.WorkorderCloseDate = null;
                    spawnWorkOrder.CustomerPO = workOrder.CustomerPO;
                    if (workOrder.OriginalWorkorderid.HasValue)
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.OriginalWorkorderid;
                    }
                    else
                    {
                        spawnWorkOrder.OriginalWorkorderid = workOrder.WorkorderID;
                    }

                    spawnWorkOrder.ParentWorkorderid = workOrder.WorkorderID;
                    if (workOrder.SpawnCounter.HasValue)
                    {
                        spawnWorkOrder.SpawnCounter = workOrder.SpawnCounter.Value + 1;
                    }
                    else
                    {
                        spawnWorkOrder.SpawnCounter = 1;
                    }

                    WorkorderDetail spawnWorkOrderDetail = new WorkorderDetail();
                    if (workOrder.WorkorderDetails.Count > 0)
                    {
                        WorkorderDetail workOrderDetail = workOrder.WorkorderDetails.ElementAt(0);
                        foreach (var property in workOrderDetail.GetType().GetProperties())
                        {
                            if (property.GetValue(workOrderDetail) != null && property.GetValue(workOrderDetail).GetType() != null && (property.GetValue(workOrderDetail).GetType().IsValueType || property.GetValue(workOrderDetail).GetType() == typeof(string)))
                            {
                                property.SetValue(spawnWorkOrderDetail, property.GetValue(workOrderDetail));
                            }
                        }
                        spawnWorkOrderDetail.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrderDetail.ArrivalDateTime = null;
                        spawnWorkOrderDetail.CompletionDateTime = null;
                        spawnWorkOrderDetail.StartDateTime = null;
                        spawnWorkOrderDetail.EntryDate = null;
                        spawnWorkOrderDetail.ModifiedDate = null;
                        spawnWorkOrderDetail.SpecialClosure = "";
                        spawnWorkOrderDetail.TravelTime = "";
                        spawnWorkOrderDetail.InvoiceNo = "";
                        spawnWorkOrderDetail.SolutionId = soluitonId;

                        spawnWorkOrder.WorkorderDetails.Add(spawnWorkOrderDetail);
                    }


                    foreach (WorkOrderBrand brand in workOrder.WorkOrderBrands)
                    {
                        WorkOrderBrand newBrand = new WorkOrderBrand();
                        foreach (var property in brand.GetType().GetProperties())
                        {
                            if (property.GetValue(brand) != null && property.GetValue(brand).GetType() != null && (property.GetValue(brand).GetType().IsValueType || property.GetValue(brand).GetType() == typeof(string)))
                            {
                                property.SetValue(newBrand, property.GetValue(brand));
                            }
                        }
                        newBrand.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkOrderBrands.Add(newBrand);
                    }

                    foreach (NotesHistory notes in workOrder.NotesHistories)
                    {
                        NotesHistory newNotes = new NotesHistory();
                        foreach (var property in notes.GetType().GetProperties())
                        {
                            if (property.GetValue(notes) != null && property.GetValue(notes).GetType() != null && (property.GetValue(notes).GetType().IsValueType || property.GetValue(notes).GetType() == typeof(string)))
                            {
                                property.SetValue(newNotes, property.GetValue(notes));
                            }
                        }
                        newNotes.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.NotesHistories.Add(newNotes);
                    }

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Work Order spawned in MARS from work order " + workOrder.WorkorderID,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 0
                    };
                    spawnWorkOrder.NotesHistories.Add(notesHistory);

                    NotesHistory WONotesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Workorder " + spawnWorkOrder.WorkorderID + " spawned due to Solution Code " + soluitonId,
                        Userid = 1234,
                        UserName = UserName,
                        isDispatchNotes = 0
                    };
                    workOrder.NotesHistories.Add(WONotesHistory);

                    foreach (WorkorderReasonlog reasonLog in workOrder.WorkorderReasonlogs)
                    {
                        WorkorderReasonlog newReasonLog = new WorkorderReasonlog();
                        foreach (var property in reasonLog.GetType().GetProperties())
                        {
                            if (property.GetValue(reasonLog) != null && property.GetValue(reasonLog).GetType() != null && (property.GetValue(reasonLog).GetType().IsValueType || property.GetValue(reasonLog).GetType() == typeof(string)))
                            {
                                property.SetValue(newReasonLog, property.GetValue(reasonLog));
                            }
                        }
                        newReasonLog.WorkorderID = spawnWorkOrder.WorkorderID;
                        spawnWorkOrder.WorkorderReasonlogs.Add(newReasonLog);
                    }

                    /*WorkorderType newWorkOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.CallTypeID == 1310).FirstOrDefault();

                    if (soluitonId == 5160 && newWorkOrderType != null)
                    {
                        spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                        spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;
                    }
                    else
                    {
                        WorkorderEquipment eqp = workorderEqps.Where(e => e.CallTypeid == 1200).FirstOrDefault();
                        if (eqp != null)
                        {
                            WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                            if (workOrderType != null)
                            {
                                spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                            }
                        }
                        else
                        {
                            eqp = workorderEqps.OrderBy(equip => equip.Assetid).ElementAt(0);
                            if (eqp != null)
                            {
                                WorkorderType workOrderType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == eqp.CallTypeid).FirstOrDefault();
                                if (workOrderType != null)
                                {
                                    spawnWorkOrder.WorkorderCalltypeid = workOrderType.CallTypeID;
                                    spawnWorkOrder.WorkorderCalltypeDesc = workOrderType.Description;
                                }
                            }
                        }

                    }*/

                    if (soluitonId == 5160 || soluitonId == 5191)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.SpawnReason))
                        {
                            spawnWorkOrderDetail.SpawnReason = Convert.ToInt32(workorderManagement.SpawnReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.SpawnNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 0
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }

                    if (soluitonId == 9999)
                    {
                        if (!string.IsNullOrWhiteSpace(workorderManagement.NSRReason))
                        {
                            spawnWorkOrderDetail.NSRReason = Convert.ToInt32(workorderManagement.NSRReason);
                        }

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = workorderManagement.NSRNotes,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 1
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);
                    }

                    foreach (WorkorderEquipment eqpItem in workorderEqps)
                    {

                        notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 0,
                            EntryDate = currentTime,
                            Notes = "SpawnedEquipment : SerialNumber - " + eqpItem.SerialNumber + ", Description - " + eqpItem.WorkDescription,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 0
                        };
                        spawnWorkOrder.NotesHistories.Add(notesHistory);

                        WorkorderEquipmentRequested spawnEquipmentRequested = new WorkorderEquipmentRequested();
                        // WorkorderEquipmentRequested workOrderReq = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();
                        if (soluitonId == 5160)
                        {
                            WorkorderType newWorkOrderType = newEntity.WorkorderTypes.Where(wt => wt.CallTypeID == 1310).FirstOrDefault();
                            spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                            spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;


                            spawnEquipmentRequested.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested.CallTypeid = 1310;
                            spawnEquipmentRequested.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested.Category = eqpItem.Category;
                            spawnEquipmentRequested.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested.Location = eqpItem.Location;
                            spawnEquipmentRequested.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested.Model = eqpItem.Model;
                            spawnEquipmentRequested.Name = eqpItem.Name;
                            spawnEquipmentRequested.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested.Temperature = "";
                            spawnEquipmentRequested.Weight = "";
                            spawnEquipmentRequested.Ratio = "";
                            spawnEquipmentRequested.Settings = "";
                            spawnEquipmentRequested.WorkPerformedCounter = "";
                            spawnEquipmentRequested.WorkDescription = "";
                            spawnEquipmentRequested.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested.Email = "";
                            spawnEquipmentRequested.NoPartsNeeded = null;
                            spawnEquipmentRequested.Solutionid = null;

                            IndexCounter assetCounter = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter.IndexValue++;
                            spawnEquipmentRequested.Assetid = assetCounter.IndexValue.Value;
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested);



                            WorkorderEquipment spawnEquipment2 = new WorkorderEquipment();
                            WorkorderEquipmentRequested spawnEquipmentRequested2 = new WorkorderEquipmentRequested();
                            WorkorderEquipmentRequested workOrderReq2 = newEntity.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();
                            spawnEquipment2.Assetid = eqpItem.Assetid;
                            spawnEquipment2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipment2.CatalogID = eqpItem.CatalogID;
                            spawnEquipment2.Category = eqpItem.Category;
                            spawnEquipment2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipment2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipment2.IsSlNumberImageExist = eqpItem.IsSlNumberImageExist;
                            spawnEquipment2.Location = eqpItem.Location;
                            spawnEquipment2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipment2.Model = eqpItem.Model;
                            spawnEquipment2.Name = eqpItem.Name;
                            spawnEquipment2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipment2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipment2.CallTypeid = 1410;
                            spawnEquipment2.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipment2.Temperature = "";
                            spawnEquipment2.Weight = "";
                            spawnEquipment2.Ratio = "";
                            spawnEquipment2.Settings = "";
                            spawnEquipment2.WorkPerformedCounter = "";
                            spawnEquipment2.WorkDescription = "";
                            spawnEquipment2.Systemid = eqpItem.Systemid;
                            spawnEquipment2.Symptomid = eqpItem.Symptomid;
                            spawnEquipment2.Email = "";
                            spawnEquipment2.NoPartsNeeded = null;
                            spawnEquipment2.Solutionid = null;


                            spawnEquipmentRequested2.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested2.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipmentRequested2.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested2.Category = eqpItem.Category;
                            spawnEquipmentRequested2.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested2.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested2.Location = eqpItem.Location;
                            spawnEquipmentRequested2.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested2.Model = eqpItem.Model;
                            spawnEquipmentRequested2.Name = eqpItem.Name;
                            spawnEquipmentRequested2.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested2.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested2.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested2.CallTypeid = 1410;
                            spawnEquipmentRequested2.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested2.Temperature = "";
                            spawnEquipmentRequested2.Weight = "";
                            spawnEquipmentRequested2.Ratio = "";
                            spawnEquipmentRequested2.Settings = "";
                            spawnEquipmentRequested2.WorkPerformedCounter = "";
                            spawnEquipmentRequested2.WorkDescription = "";
                            spawnEquipmentRequested2.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested2.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested2.Email = "";
                            spawnEquipmentRequested2.NoPartsNeeded = null;
                            spawnEquipmentRequested2.Solutionid = null;

                            IndexCounter assetCounter2 = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter2.IndexValue++;

                            spawnEquipment2.Assetid = assetCounter2.IndexValue.Value;
                            spawnEquipmentRequested2.Assetid = assetCounter2.IndexValue.Value;

                            spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment2);
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested2);
                        }
                        else if (soluitonId == 5140 || soluitonId == 5170 || soluitonId == 5171 || soluitonId == 5181 || soluitonId == 5191 ||
                                    soluitonId == 5115 || soluitonId == 5120 || soluitonId == 5130 || soluitonId == 5135)
                        {
                            int calltypeId = 0;
                            switch (soluitonId)
                            {
                                case 5140:
                                    calltypeId = 1210;
                                    break;
                                case 5170:
                                case 5171:
                                case 5181:
                                case 5191:
                                    calltypeId = 1220;
                                    break;
                                case 5115:
                                case 5120:
                                case 5130:
                                case 5135:
                                    calltypeId = 1310;
                                    break;
                            }

                            WorkorderType newWorkOrderType = newEntity.WorkorderTypes.Where(wt => wt.CallTypeID == calltypeId).FirstOrDefault();
                            spawnWorkOrder.WorkorderCalltypeid = newWorkOrderType.CallTypeID;
                            spawnWorkOrder.WorkorderCalltypeDesc = newWorkOrderType.Description;

                            WorkorderEquipment spawnEquipment3 = new WorkorderEquipment();
                            WorkorderEquipmentRequested spawnEquipmentRequested3 = new WorkorderEquipmentRequested();
                            WorkorderEquipmentRequested workOrderReq3 = newEntity.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == eqpItem.Assetid).FirstOrDefault();
                            spawnEquipment3.Assetid = eqpItem.Assetid;
                            spawnEquipment3.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipment3.CatalogID = eqpItem.CatalogID;
                            spawnEquipment3.Category = eqpItem.Category;
                            spawnEquipment3.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipment3.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipment3.IsSlNumberImageExist = eqpItem.IsSlNumberImageExist;
                            spawnEquipment3.Location = eqpItem.Location;
                            spawnEquipment3.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipment3.Model = eqpItem.Model;
                            spawnEquipment3.Name = eqpItem.Name;
                            spawnEquipment3.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipment3.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipment3.CallTypeid = calltypeId;
                            spawnEquipment3.WorkorderID = spawnWorkOrder.WorkorderID;
                            spawnEquipment3.Temperature = "";
                            spawnEquipment3.Weight = "";
                            spawnEquipment3.Ratio = "";
                            spawnEquipment3.Settings = "";
                            spawnEquipment3.WorkPerformedCounter = "";
                            spawnEquipment3.WorkDescription = "";
                            spawnEquipment3.Systemid = eqpItem.Systemid;
                            spawnEquipment3.Symptomid = eqpItem.Symptomid;
                            spawnEquipment3.Email = "";
                            spawnEquipment3.NoPartsNeeded = null;
                            spawnEquipment3.Solutionid = null;


                            spawnEquipmentRequested3.Assetid = eqpItem.Assetid;
                            spawnEquipmentRequested3.CallTypeid = eqpItem.CallTypeid;
                            spawnEquipmentRequested3.CatalogID = eqpItem.CatalogID;
                            spawnEquipmentRequested3.Category = eqpItem.Category;
                            spawnEquipmentRequested3.EquipmentId = eqpItem.EquipmentId;
                            spawnEquipmentRequested3.FeastMovementid = eqpItem.FeastMovementid;
                            spawnEquipmentRequested3.Location = eqpItem.Location;
                            spawnEquipmentRequested3.Manufacturer = eqpItem.Manufacturer;
                            spawnEquipmentRequested3.Model = eqpItem.Model;
                            spawnEquipmentRequested3.Name = eqpItem.Name;
                            spawnEquipmentRequested3.QualityIssue = eqpItem.QualityIssue;
                            spawnEquipmentRequested3.SerialNumber = eqpItem.SerialNumber;
                            spawnEquipmentRequested3.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested3.CallTypeid = calltypeId;
                            spawnEquipmentRequested3.WorkorderID = eqpItem.WorkorderID;
                            spawnEquipmentRequested3.Temperature = "";
                            spawnEquipmentRequested3.Weight = "";
                            spawnEquipmentRequested3.Ratio = "";
                            spawnEquipmentRequested3.Settings = "";
                            spawnEquipmentRequested3.WorkPerformedCounter = "";
                            spawnEquipmentRequested3.WorkDescription = "";
                            spawnEquipmentRequested3.Systemid = eqpItem.Systemid;
                            spawnEquipmentRequested3.Symptomid = eqpItem.Symptomid;
                            spawnEquipmentRequested3.Email = "";
                            spawnEquipmentRequested3.NoPartsNeeded = null;
                            spawnEquipmentRequested3.Solutionid = null;

                            IndexCounter assetCounter3 = Utility.GetIndexCounter("AssetID", 1);
                            assetCounter3.IndexValue++;

                            spawnEquipment3.Assetid = assetCounter3.IndexValue.Value;
                            spawnEquipmentRequested3.Assetid = assetCounter3.IndexValue.Value;

                            spawnWorkOrder.WorkorderEquipments.Add(spawnEquipment3);
                            spawnWorkOrder.WorkorderEquipmentRequesteds.Add(spawnEquipmentRequested3);
                        }
                    }

                    newEntity.WorkOrders.Add(spawnWorkOrder);
                    newEntity.SaveChanges();

                    string emailAddresses = string.Empty;


                    StringBuilder subject = new StringBuilder();
                    subject.Append("Spawned Workorder - Original WO: ");
                    subject.Append(spawnWorkOrder.OriginalWorkorderid);
                    subject.Append(" ST: ");
                    subject.Append(spawnWorkOrder.CustomerState);
                    subject.Append(" Call Type: ");
                    subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);

                    SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null, newEntity);

                    if (responsibleTechId.HasValue)
                    {
                        subject = new StringBuilder();
                        subject.Append("WO:");
                        subject.Append(spawnWorkOrder.WorkorderID);
                        subject.Append(" ST:");
                        subject.Append(spawnWorkOrder.CustomerState);
                        subject.Append(" Call Type:");
                        subject.Append(spawnWorkOrder.WorkorderCalltypeDesc);


                        string emailAddress = string.Empty;
                        var CustomerId = int.Parse(responsibleTechId.Value.ToString());
                        Customer serviceCustomer = newEntity.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                        if (serviceCustomer != null)
                        {
                            emailAddress = serviceCustomer.Email;
                        }

                        if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TestEmail"]))
                        {
                            emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                        }

                        if (!string.IsNullOrWhiteSpace(emailAddress))
                        {
                            SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null, newEntity);
                        }
                    }

                    if (string.IsNullOrEmpty(SpawnedWOsCreated))
                    {
                        SpawnedWOsCreated += spawnWorkOrder.WorkorderID;
                    }
                    else
                    {
                        SpawnedWOsCreated += ", " + spawnWorkOrder.WorkorderID;
                    }
                    //message = @"Spawned Work Order " + spawnWorkOrder.WorkorderID + " is created!";
                }
            }

            if (!string.IsNullOrEmpty(SpawnedWOsCreated))
            {
                NotesHistory WONotesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = @"Spawned Work Order " + SpawnedWOsCreated + " created in MARS ",
                    Userid = 1234, //TBD
                    UserName = UserName,
                    isDispatchNotes = 0
                };
                //workOrder.NotesHistories.Add(WONotesHistory);
                //FarmerBrothersEntitites.SaveChanges();

                message = @"Spawned Work Order " + SpawnedWOsCreated + " created!";
            }
            else
            {
                message = @"";
            }           
        }
        private bool SendWorkOrderMail(WorkOrder workOrder, string subject, string toAddress, string fromAddress, int? techId, MailType mailType, bool isResponsible, string additionalMessage, FarmerBrothersEntities entity)
        {
            StringBuilder salesEmailBody = new StringBuilder();

            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            if (!string.IsNullOrWhiteSpace(additionalMessage))
            {
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("<BR>");
            }

          
            string url = ConfigurationManager.AppSettings["DispatchResponseUrl"];
            string Redircturl = ConfigurationManager.AppSettings["RedirectResponseUrl"];

            //string finalUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=@response&isResponsible=" + isResponsible.ToString()));
            if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
            {
                if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                {
                    if (mailType == MailType.DISPATCH)
                    {
                        //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible + "\">ACCEPT</a>");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    }
                    if (workOrder.WorkorderCallstatus == "Pending Acceptance")
                    {
                        //salesEmailBody.Append("<a href=\"" + Redircturl + "?workOrderId=" + workOrder.WorkorderID +  "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible + "\">REDIRECT</a>");
                        string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));
                        salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    }
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible.ToString())) + "\">ARRIVAL</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID +  "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible + "\">REJECT</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID +  "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible + "\">ARRIVAL</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID +  "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible + "\">COMPLETED</a>");
                }
            }
            else if (mailType == MailType.REDIRECTED)
            {
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
                //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible + "\">DISREGARD</a>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("CALL TIME: ");
            salesEmailBody.Append(workOrder.WorkorderEntryDate);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("Work Order ID#: ");
            salesEmailBody.Append(workOrder.WorkorderID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER INFORMATION: ");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER#: ");
            salesEmailBody.Append(workOrder.CustomerID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.CustomerName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.CustomerAddress);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.CustomerCity);
            salesEmailBody.Append(",");
            salesEmailBody.Append(workOrder.CustomerState);
            salesEmailBody.Append(" ");
            salesEmailBody.Append(workOrder.CustomerZipCode);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.WorkorderContactName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("PHONE: ");
            salesEmailBody.Append(workOrder.WorkorderContactPhone);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER PREFERENCES: ");
            salesEmailBody.Append(workOrder.CustomerCustomerPreferences);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL CODES: ");
            salesEmailBody.Append("<BR>");

            foreach (WorkorderEquipmentRequested equipment in workOrder.WorkorderEquipmentRequesteds)
            {
                salesEmailBody.Append("CATEGORY: ");
                salesEmailBody.Append(equipment.Category);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("MODEL#: ");
                salesEmailBody.Append(equipment.Model);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("LOCATION: ");
                salesEmailBody.Append(equipment.Location);
                salesEmailBody.Append("<BR>");
                
                salesEmailBody.Append("SYMPTOM: ");
                salesEmailBody.Append(equipment.Symptomid);
                salesEmailBody.Append("<BR>");

                WorkorderType callType = entity.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                if (callType != null)
                {
                    salesEmailBody.Append("CALLTYPE: ");
                    salesEmailBody.Append(callType.CallTypeID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(callType.Description);
                    salesEmailBody.Append("<BR>");
                }

                Symptom symptom = entity.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                if (symptom != null)
                {
                    salesEmailBody.Append("SYMPTOM: ");
                    salesEmailBody.Append(symptom.SymptomID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(symptom.Description);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL NOTES: ");
            salesEmailBody.Append("<BR>");
            IEnumerable<NotesHistory> histories = workOrder.NotesHistories.OrderByDescending(n => n.EntryDate);

            foreach (NotesHistory history in histories)
            {
                salesEmailBody.Append(history.UserName);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.EntryDate);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.Notes.Replace("\\n", " ").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\r", " "));
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("SERVICE HISTORY:");
            salesEmailBody.Append("<BR>");

            

            IEnumerable<WorkOrder> previousWorkOrders = entity.WorkOrders.Where(w => w.CustomerID == workOrder.CustomerID);
            foreach (WorkOrder previousWorkOrder in previousWorkOrders)
            {
                salesEmailBody.Append("Work Order ID#: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderID);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("ENTRY DATE: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderEntryDate);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("STATUS: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderCallstatus);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("CALL CODES: ");
                salesEmailBody.Append("<BR>");

                foreach (WorkorderEquipment equipment in previousWorkOrder.WorkorderEquipments)
                {
                    salesEmailBody.Append("MAKE: ");
                    salesEmailBody.Append(equipment.Manufacturer);
                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("MODEL#: ");
                    salesEmailBody.Append(equipment.Model);
                    salesEmailBody.Append("<BR>");

                    WorkorderType callType = entity.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                    if (callType != null)
                    {
                        salesEmailBody.Append("CALLTYPE: ");
                        salesEmailBody.Append(callType.CallTypeID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(callType.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    Symptom symptom = entity.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                    if (symptom != null)
                    {
                        salesEmailBody.Append("SYMPTOM: ");
                        salesEmailBody.Append(symptom.SymptomID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(symptom.Description);
                        salesEmailBody.Append("<BR>");
                    }
                                        
                }
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            
            if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
            {
                if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                {
                    if (mailType == MailType.DISPATCH)
                    {
                        //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible + "\">ACCEPT</a>");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    }
                    if (workOrder.WorkorderCallstatus == "Pending Acceptance")
                    {
                        //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID +  "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible + "\">REDIRECT</a>");
                        string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));
                        salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    }
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible.ToString())) + "\">ARRIVAL</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID +  "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible + "\">REJECT</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID +  "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible + "\">ARRIVAL</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID +  "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible + "\">COMPLETED</a>");
                }
            }
            else if (mailType == MailType.REDIRECTED)
            {
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
                //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible + "\">DISREGARD</a>");
            }

            string contentId = Guid.NewGuid().ToString();
            string logoPath = string.Empty;
            if (Server == null)
            {
                logoPath = Path.Combine(HttpRuntime.AppDomainAppPath, "img/mainlogo.jpg");
            }
            else
            {
                logoPath = Server.MapPath("~/img/mainlogo.jpg");
            }


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

            bool result = true;
            string mailTo = toAddress;
            if (!string.IsNullOrWhiteSpace(mailTo))
            {
                string[] addresses = mailTo.Split(';');
                foreach (string address in addresses)
                {
                    if (address.ToLower().Contains("@jmsmucker.com")) continue;
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        message.To.Add(new MailAddress(address));
                    }
                }

                message.From = new MailAddress(fromAddress);
                message.Subject = subject;
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
            return result;
        }

        public JsonResult GetCloserNonTaggedManufacturer(string skuValue)
        {
            IQueryable<string> vendors = null;

            if (string.IsNullOrWhiteSpace(skuValue))
            {
                vendors = FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true).OrderBy(s => s.Supplier).Select(s => s.Supplier).Distinct();
            }
            else
            {
                vendors = FarmerBrothersEntitites.FBClosureParts.Where(s => s.ItemNo == skuValue && s.SkuActive == true).OrderBy(s => s.Supplier).Select(s => s.Supplier).Distinct();
            }

            var data = new List<object>();
            foreach (string vendor in vendors)
            {
                string skuDescription = string.Empty;
                FBClosurePart sku = FarmerBrothersEntitites.FBClosureParts.Where(s => s.ItemNo == skuValue).FirstOrDefault();
                if (sku != null)
                {
                    skuDescription = sku.Description;
                }
                data.Add(new { Manufacturer = vendor.ToUpper().Trim(), Description = skuDescription.ToUpper().Trim() });
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        #endregion
    }
}