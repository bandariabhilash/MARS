using FarmerBrothers.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FarmerBrothers.Data;

using FarmerBrothers.Utilities;
using LinqKit;
using System;

using Syncfusion.Pdf;
using Syncfusion.HtmlConverter;
using Syncfusion.JavaScript.Models;
using Syncfusion.EJ.Export;
using Syncfusion.XlsIO;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Web;
//using FarmerBrothers.MovementSearchService;
//using FarmerBrothers.FeastMovementService;
//using FarmerBrothers.WSCustomerKnowEquipmentService;
//using FarmerBrothers.FeastLocationService;


namespace FarmerBrothers.Controllers
{
    public class PartsOrderController : BaseController
    {
       /* public PartsOrderController()
        {
        }

        [HttpGet]
        public ActionResult PartsManagement(int? customerId, int? workOrderId)
        {
            PartsManagementModel partsManagementModel = new PartsManagementModel();
            //partsManagementModel.PriorityList = FormalBrothersEntitites.AllFormalBrothersStatus.Where(p => p.StatusFor == "Priority" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            partsManagementModel.BrandNames = FarmerBrothersEntitites.BrandNames.Where(b => b.Active == 1).ToList();
            //partsManagementModel.SalesNotificationReasonCodes = FormalBrothersEntitites.AllFormalBrothersStatus.Where(p => p.StatusFor == "Notify Sales" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            partsManagementModel.CallTypes = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.Active == 1 && wt.CallTypeID != 1800 && wt.CallTypeID != 1810 && wt.CallTypeID != 1820).OrderBy(wt => wt.Sequence).ToList();
            partsManagementModel.ClosureCallTypes = FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.Active == 1).OrderBy(wt => wt.Sequence).ToList();
            partsManagementModel.EquipmentTypes = FarmerBrothersEntitites.EquipTypes.OrderBy(e => e.Sequence).ToList();
            partsManagementModel.NonSerializedList = new List<WorkOrderManagementNonSerializedModel>();
            partsManagementModel.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
            partsManagementModel.WorkOrderParts = new List<WorkOrderPartModel>();
            partsManagementModel.Operation = WorkOrderManagementSubmitType.NONE;
            partsManagementModel.States = Utility.GetStates(FarmerBrothersEntitites);
            partsManagementModel.Closure = new WorkOrderClosureModel();
            partsManagementModel.BranchIds = new List<int>();
            partsManagementModel.AssistTechIds = new List<int>();
            partsManagementModel.SystemInfoes = FarmerBrothersEntitites.SystemInfoes.Where(s => s.Active == 1).OrderBy(s => s.Sequence).ToList();
            partsManagementModel.Solutions = FarmerBrothersEntitites.Solutions.Where(s => s.Active == 1).OrderBy(s => s.Sequence).ToList();
            partsManagementModel.SystemInfoes.Insert(0, new SystemInfo()
            {
                Active = 1,
                Description = "",
                SystemId = 0
            });

            partsManagementModel.TaggedCategories = new List<CategoryModel>();
            IQueryable<string> categories = FarmerBrothersEntitites.Skus.Where(s => s.SkuActive == 1 && s.EQUIPMENT_TAG == "TAGGED").Select(s => s.Category).Distinct();
            foreach (string category in categories)
            {
                partsManagementModel.TaggedCategories.Add(new CategoryModel(category));
            }

            partsManagementModel.Symptoms = FarmerBrothersEntitites.Symptoms.Where(s => s.Active == 1).OrderBy(s => s.Sequence).ToList();
            partsManagementModel.Symptoms.Insert(0, new Symptom()
            {
                Description = "",
                SymptomID = 0
            });

            partsManagementModel.Amps = FarmerBrothersEntitites.AMPSLists.Where(a => a.Active == 1).OrderBy(a => a.Sequence).ToList();
            partsManagementModel.Amps.Insert(0, new AMPSList()
            {
                AMPSDescription = "",
                AMPSID = 0
            });

            partsManagementModel.ElectricalPhases = FarmerBrothersEntitites.ElectricalPhaseLists.Where(e => e.Active == 1).OrderBy(e => e.Sequence).ToList();
            partsManagementModel.ElectricalPhases.Insert(0, new ElectricalPhaseList()
            {
                ElectricalPhase = "",
                ElectricalPhaseID = 0
            });

            partsManagementModel.NmeaNumbers = FarmerBrothersEntitites.NEMANumberLists.Where(n => n.Active == 1).OrderBy(n => n.Sequence).ToList();
            partsManagementModel.NmeaNumbers.Insert(0, new NEMANumberList()
            {
                NemaNumberDescription = "",
                NemaNumberID = 0
            });

            partsManagementModel.Voltages = FarmerBrothersEntitites.VoltageLists.Where(v => v.Active == 1).OrderBy(v => v.Sequence).ToList();
            partsManagementModel.Voltages.Insert(0, new VoltageList()
            {
                Voltage = "",
                VoltageID = 0
            });

            partsManagementModel.WaterLines = FarmerBrothersEntitites.WaterLineLists.Where(w => w.Active == 1).OrderBy(w => w.Sequence).ToList();
            partsManagementModel.WaterLines.Insert(0, new WaterLineList()
            {
                WaterLine = "",
                WaterLineID = 0
            });

            partsManagementModel.YesNoList = new List<YesNoItem>();
            partsManagementModel.YesNoList.Add(new YesNoItem()
            {
                Description = "Yes",
                Id = 1
            });
            partsManagementModel.YesNoList.Add(new YesNoItem()
            {
                Description = "No",
                Id = 2
            });

            partsManagementModel.YesNoList.Insert(0, new YesNoItem()
            {
                Description = "",
                Id = 0
            });

            IQueryable<string> vendors = FarmerBrothersEntitites.Skus.Where(s => s.SkuActive == 1 && s.EQUIPMENT_TAG == "TAGGED").Select(s => s.Manufacturer).Distinct();
            partsManagementModel.TaggedManufacturer = new List<VendorDataModel>();
            foreach (string vendor in vendors)
            {
                partsManagementModel.TaggedManufacturer.Add(new VendorDataModel(vendor));
            }
            partsManagementModel.TaggedManufacturer = partsManagementModel.TaggedManufacturer.OrderBy(v => v.VendorDescription).ToList();

            vendors = FarmerBrothersEntitites.Skus.Where(s => s.SkuActive == 1 && (s.EQUIPMENT_TAG == "ANCILLARY" || s.EQUIPMENT_TAG == "NONTAGGED")).Select(s => s.Manufacturer).Distinct();
            partsManagementModel.NonTaggedManufacturer = new List<VendorDataModel>();
            foreach (string vendor in vendors)
            {
                partsManagementModel.NonTaggedManufacturer.Add(new VendorDataModel(vendor));
            }
            partsManagementModel.NonTaggedManufacturer = partsManagementModel.NonTaggedManufacturer.OrderBy(v => v.VendorDescription).ToList();

            IQueryable<string> models = FarmerBrothersEntitites.Skus.Where(s => s.SkuActive == 1 && (s.EQUIPMENT_TAG == "ANCILLARY" || s.EQUIPMENT_TAG == "NONTAGGED")).Select(s => s.Sku1).Distinct();
            partsManagementModel.NonTaggedModels = new List<VendorModelModel>();
            foreach (string model in models)
            {
                partsManagementModel.NonTaggedModels.Add(new VendorModelModel(model));
            }
            partsManagementModel.NonTaggedModels = partsManagementModel.NonTaggedModels.OrderBy(v => v.Model).ToList();

            if (workOrderId.HasValue)
            {
                partsManagementModel.WorkOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId.Value).FirstOrDefault();
                partsManagementModel.Customer = new CustomerModel(partsManagementModel.WorkOrder, FarmerBrothersEntitites);
                partsManagementModel.Customer = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, partsManagementModel.Customer);

                WorkorderDetail workOrderDetail = FarmerBrothersEntitites.WorkorderDetails.Where(wd => wd.WorkorderID == workOrderId.Value).FirstOrDefault();
                if (workOrderDetail != null)
                {
                    partsManagementModel.Closure.ArrivalDateTime = workOrderDetail.ArrivalDateTime;
                    partsManagementModel.Closure.CompletionDateTime = workOrderDetail.CompletionDateTime;
                    partsManagementModel.Closure.ResponsibleTechName = workOrderDetail.ResponsibleTechName;
                    partsManagementModel.Closure.Mileage = workOrderDetail.Mileage;
                    partsManagementModel.Closure.CustomerName = workOrderDetail.CustomerName;
                    partsManagementModel.Closure.CustomerEmail = workOrderDetail.CustomerEmail;
                    partsManagementModel.Closure.SpecialClosure = workOrderDetail.SpecialClosure;
                    string signatureFileName = Server.MapPath(@"~/mars_wo_images/" + workOrderId + "/CustomerSignature/" + workOrderId + ".jpg");
                    if (System.IO.File.Exists(signatureFileName))
                    {
                        partsManagementModel.Closure.CustomerSignatureUrl = @"/mars_wo_images/" + workOrderId + "/CustomerSignature/" + workOrderId + ".jpg";
                    }
                    else
                    {
                        partsManagementModel.Closure.CustomerSignatureUrl = string.Empty;
                    }
                    if (!string.IsNullOrWhiteSpace(workOrderDetail.TravelTime))
                    {
                        string[] times = workOrderDetail.TravelTime.Split(':');

                        if (times.Count() >= 2)
                        {
                            partsManagementModel.Closure.TravelHours = times[0];
                            partsManagementModel.Closure.TravelMinutes = times[1];
                        }
                    }
                }

                if (partsManagementModel.WorkOrder.WorkorderCalltypeid == 1800)
                {
                    partsManagementModel.CustomerPartsOrder = true;
                }
                else if (partsManagementModel.WorkOrder.WorkorderCalltypeid == 1820)
                {
                    partsManagementModel.TechPartsOrder = true;
                }

                if (partsManagementModel.WorkOrder.PartsRushOrder.HasValue == false)
                {
                    partsManagementModel.WorkOrder.PartsRushOrder = false;
                }

                models = FarmerBrothersEntitites.Skus.Where(s => s.SkuActive == 1 && s.EQUIPMENT_TAG == "TAGGED").Select(s => s.Sku1).Distinct();
                partsManagementModel.TaggedModels = new List<VendorModelModel>();
                foreach (string model in models)
                {
                    partsManagementModel.TaggedModels.Add(new VendorModelModel(model));
                }
                partsManagementModel.TaggedModels = partsManagementModel.TaggedModels.OrderBy(v => v.Model).ToList();

                IQueryable<WorkorderSchedule> workOrderSchedules = FarmerBrothersEntitites.WorkorderSchedules.Where(ws => ws.WorkorderID == workOrderId);

                int primaryBranchId = 0;
                int secondaryBranchId = 0;

                foreach (WorkorderSchedule workOrderSchedule in workOrderSchedules)
                {
                    if (workOrderSchedule.PrimaryTech == 1)
                    {
                        partsManagementModel.ResponsibleTechId = workOrderSchedule.Techid;

                        if (workOrderSchedule.ServiceCenterID.HasValue)
                        {
                            primaryBranchId = workOrderSchedule.ServiceCenterID.Value;
                            partsManagementModel.BranchIds.Add(workOrderSchedule.ServiceCenterID.Value);
                        }
                    }
                    else if (workOrderSchedule.Techid.HasValue)
                    {
                        partsManagementModel.AssistTechIds.Add(workOrderSchedule.Techid.Value);
                        if (workOrderSchedule.ServiceCenterID.HasValue)
                        {
                            secondaryBranchId = workOrderSchedule.ServiceCenterID.Value;
                            partsManagementModel.BranchIds.Add(workOrderSchedule.ServiceCenterID.Value);
                        }
                    }
                }

                try
                {
                    partsManagementModel.Customer.Entitlements = GetCustomerEntitlementList(partsManagementModel.Customer.CustomerId);
                    //FormalBrothers.Data.FeastLocationService.Customer serviceCustomer = GetCustomerFromService(partsManagementModel.WorkOrder.CustomerID.Value.ToString());
                    //if (serviceCustomer != null)
                    //{
                    //    partsManagementModel.Customer.CustomerSpecialInstructions = serviceCustomer.CustomerSpecialInstruction;
                    //    partsManagementModel.Customer.CustomerPreference = serviceCustomer.CustomerPreference;
                    //}
                }
                catch (Exception e)
                { }

                IList<BranchModel> branches = new List<BranchModel>();

                IDictionary<double, string> branchZipCodes = new Dictionary<double, string>();
                IEnumerable<TechHierarchyView> techViews = Utility.GetTechDataByBranchType(FarmerBrothersEntitites, "Internal Branch", "Stock Location");
                if (techViews != null)
                {
                    foreach (TechHierarchyView techView in techViews)
                    {
                        if (!branchZipCodes.ContainsKey(techView.ServiceCenter_Id))
                        {
                            BranchModel branchModel = new BranchModel(techView, partsManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                            branches.Add(branchModel);
                            branchZipCodes.Add(techView.ServiceCenter_Id, techView.ServiceCenter_zip);
                        }
                    }
                }

                techViews = Utility.GetTechDataByBranchType(FarmerBrothersEntitites, "TPSP Branch", "Stock Location");
                if (techViews != null)
                {
                    foreach (TechHierarchyView techView in techViews)
                    {
                        if (!branchZipCodes.ContainsKey(techView.ServiceCenter_Id))
                        {
                            BranchModel branchModel = new BranchModel(techView, partsManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                            branches.Add(branchModel);
                            branchZipCodes.Add(techView.ServiceCenter_Id, techView.ServiceCenter_zip);
                        }
                    }
                }

                //Utility.PopulateDistances(branchZipCodes, branches, partsManagementModel.Customer.ZipCode);
                partsManagementModel.Branches = branches.OrderBy(b => b.Distance).ToList();

                techViews = Utility.GetTechDataByServiceCenterId(FarmerBrothersEntitites, secondaryBranchId);
                if (techViews != null)
                {
                    foreach (TechHierarchyView techView in techViews)
                    {
                        decimal serviceCenterId = Convert.ToDecimal(techView.ServiceCenter_Id);
                        BranchModel model = partsManagementModel.Branches.SingleOrDefault(b => b.Id == serviceCenterId);
                        if (model == null)
                        {
                            BranchModel branchModel = new BranchModel(techView, partsManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                            partsManagementModel.Branches.Insert(0, branchModel);
                        }
                        else
                        {
                            partsManagementModel.Branches.Remove(model);
                            partsManagementModel.Branches.Insert(0, model);
                        }
                    }
                }

                techViews = Utility.GetTechDataByServiceCenterId(FarmerBrothersEntitites, primaryBranchId);
                if (techViews != null)
                {
                    foreach (TechHierarchyView techView in techViews)
                    {
                        decimal serviceCenterId = Convert.ToDecimal(techView.ServiceCenter_Id);
                        BranchModel model = partsManagementModel.Branches.SingleOrDefault(b => b.Id == serviceCenterId);
                        if (model == null)
                        {
                            BranchModel branchModel = new BranchModel(techView, partsManagementModel.Customer.ZipCode, FarmerBrothersEntitites);
                            partsManagementModel.Branches.Insert(0, branchModel);
                        }
                        else
                        {
                            partsManagementModel.Branches.Remove(model);
                            partsManagementModel.Branches.Insert(0, model);
                        }
                    }
                }

                IList<int?> brandIds = FarmerBrothersEntitites.WorkOrderBrands.Where(wb => wb.WorkorderID == workOrderId.Value).Select(wb => wb.BrandID).ToList();
                partsManagementModel.SelectedBrandIds = string.Join(",", brandIds);
                partsManagementModel.SelectedBrands = FarmerBrothersEntitites.BrandNames.Where(b => brandIds.Contains(b.BrandID)).ToList();

                if (!string.IsNullOrWhiteSpace(partsManagementModel.WorkOrder.WorkorderErfid))
                {
                    partsManagementModel.Erf = FarmerBrothersEntitites.Erfs.Where(e => e.ErfID == partsManagementModel.WorkOrder.WorkorderErfid).FirstOrDefault();
                }

                if (partsManagementModel.WorkOrder.WorkorderCalltypeid == 1300
                    && string.Compare(partsManagementModel.WorkOrder.WorkorderCallstatus, "Closed") != 0
                    && string.Compare(partsManagementModel.WorkOrder.WorkorderCallstatus, "Completed") != 0)
                {
                    try
                    {
                        WOSearchRequest woSearchRequest = new WOSearchRequest()
                        {
                            MarsWorkOrderID = partsManagementModel.WorkOrder.WorkorderID.ToString()
                        };

                        MovementWOResponse woResponse = erfFeastMovementClient.getWOSearch(woSearchRequest);
                        if (woResponse != null)
                        {
                            if (woResponse.MovementWO != null)
                            {
                                foreach (MovementWO movementWo in woResponse.MovementWO)
                                {
                                    if (movementWo.SeialItemWO != null)
                                    {
                                        int sequenceNumber = 1;
                                        foreach (SerializedItem serializedItem in movementWo.SeialItemWO)
                                        {
                                            WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(serializedItem, partsManagementModel.WorkOrder.WorkorderCalltypeid);
                                            equipmentModel.SequenceNumber = sequenceNumber++;
                                            partsManagementModel.WorkOrderEquipments.Add(equipmentModel);
                                        }

                                        foreach (MovementSearchService.NonSerializedItem nonSerialized in movementWo.NonSeialItemWO)
                                        {
                                            WorkOrderManagementNonSerializedModel nonSerializedModel = new WorkOrderManagementNonSerializedModel()
                                            {
                                                Catalogid = nonSerialized.CatalogID,
                                                ManufNumber = nonSerialized.Manufacturer
                                            };

                                            if (!string.IsNullOrWhiteSpace(nonSerialized.OriginalOrderQuantity))
                                            {
                                                nonSerializedModel.OrigOrderQuantity = Convert.ToInt32(nonSerialized.OriginalOrderQuantity);
                                            }

                                            partsManagementModel.NonSerializedList.Add(nonSerializedModel);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    { }
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
                        partsManagementModel.NonSerializedList.Add(nonSerializedModel);
                    }

                    int sequenceNumber = 1;
                    foreach (WorkorderEquipment workOrderEquipment in partsManagementModel.WorkOrder.WorkorderEquipments)
                    {
                        WorkOrderManagementEquipmentModel equipmentModel = new WorkOrderManagementEquipmentModel(workOrderEquipment, FarmerBrothersEntitites);
                        equipmentModel.SequenceNumber = sequenceNumber++;
                        partsManagementModel.WorkOrderEquipments.Add(equipmentModel);
                    }
                }

                IEnumerable<WorkorderPart> parts = partsManagementModel.WorkOrder.WorkorderParts.Where(wp => wp.AssetID == null);
                foreach (WorkorderPart workOrderPart in parts)
                {
                    WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);
                    partsManagementModel.WorkOrderParts.Add(workOrderPartModel);
                }

                if (string.Compare(partsManagementModel.WorkOrder.PartsShipTo, "Local Branch", 0) == 0)
                {
                    partsManagementModel.PartsShipTo = 1;
                }
                else if (string.Compare(partsManagementModel.WorkOrder.PartsShipTo, "Customer", 0) == 0)
                {
                    partsManagementModel.PartsShipTo = 2;
                    partsManagementModel.Customer_OtherPartsContactName = partsManagementModel.WorkOrder.OtherPartsContactName;
                }
                else if (string.Compare(partsManagementModel.WorkOrder.PartsShipTo, "Other", 0) == 0)
                {
                    partsManagementModel.PartsShipTo = 3;
                    partsManagementModel.Other_OtherPartsContactName = partsManagementModel.WorkOrder.OtherPartsContactName;
                }
            }
            else if (customerId.HasValue)
            {
                partsManagementModel.WorkOrder = new WorkOrder() { CustomerID = customerId.Value };

                    partsManagementModel.WorkOrder.WorkorderCalltypeid = 1800;
                    partsManagementModel.WorkOrder.WorkorderCalltypeDesc = "Parts Request";
                    partsManagementModel.WorkOrder.PartsRushOrder = false;

                if (feastLocationsClient != null && partsManagementModel.WorkOrder != null && partsManagementModel.WorkOrder.CustomerID.HasValue)
                {
                    FarmerBrothers.FeastLocationService.Customer serviceCustomer = GetCustomerFromService(partsManagementModel.WorkOrder.CustomerID.Value.ToString());

                    if (serviceCustomer != null)
                    {
                        partsManagementModel.Customer = new CustomerModel(serviceCustomer, FarmerBrothersEntitites);
                        partsManagementModel.Customer = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, partsManagementModel.Customer);
                        if (workOrderId.HasValue)
                        {
                            partsManagementModel.Customer.WorkOrderId = workOrderId.Value.ToString();
                        }
                        else
                        {
                            partsManagementModel.Customer.WorkOrderId = "-1";
                        }

                        partsManagementModel.Customer.Entitlements = GetCustomerEntitlementList(partsManagementModel.Customer.CustomerId);
                    }
                    else
                    {
                        partsManagementModel.Customer = new CustomerModel();
                        partsManagementModel.Customer.Entitlements = new List<Entitlement>();
                    }
                }
            }

            if (partsManagementModel.WorkOrder == null)
            {
                partsManagementModel.WorkOrder = new WorkOrder();
            }
            if (partsManagementModel.Customer == null)
            {
                partsManagementModel.Customer = new CustomerModel();
                partsManagementModel.WorkOrder.WorkorderCallstatus = "Scratch";
            }

            if (customerKnowEquipClient != null && partsManagementModel.Customer != null)
            {
                partsManagementModel.KnownEquipments = new List<KnownEquipmentModel>();

                try
                {
                    KnownEquipRequest knownEquipRequest = new KnownEquipRequest()
                    {
                        CustomerID = partsManagementModel.Customer.CustomerId
                    };

                    KnownEquipResponse knownEquipResponse = customerKnowEquipClient.getCustomerEquipment(knownEquipRequest);


                    if (knownEquipResponse.SerializedLines != null)
                    {
                        foreach (KnownEquipment knownEquipment in knownEquipResponse.SerializedLines)
                        {
                            KnownEquipmentModel knownEquipmentModel = new KnownEquipmentModel()
                            {
                                Category = knownEquipment.Category,
                                CatalogID = knownEquipment.Cataloged,
                                Location = knownEquipment.Location,
                                Manufacturer = knownEquipment.Manufacturer,
                                Model = knownEquipment.Model,
                                SerialNumber = knownEquipment.SerialNumber,
                                Status = knownEquipment.Status
                            };

                            partsManagementModel.KnownEquipments.Add(knownEquipmentModel);
                        }

                        partsManagementModel.KnownEquipments = partsManagementModel.KnownEquipments.OrderBy(k => k.Status).ThenBy(k => k.Category).ThenBy(k => k.Model).ToList();
                    }
                }
                catch (Exception e)
                {
                    //Need to log the exception.
                }
            }

            partsManagementModel.Notes = new NotesModel()
            {
                CustomerZipCode = partsManagementModel.Customer.ZipCode
            };

            IQueryable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == partsManagementModel.WorkOrder.WorkorderID && nh.AutomaticNotes == 0).OrderByDescending(nh => nh.EntryDate);
            partsManagementModel.Notes.NotesHistory = new List<NotesHistoryModel>();
            foreach (NotesHistory notesHistory in notesHistories)
            {
                partsManagementModel.Notes.NotesHistory.Add(new NotesHistoryModel(notesHistory));
            }

            IQueryable<NotesHistory> recordHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.WorkorderID == partsManagementModel.WorkOrder.WorkorderID && nh.AutomaticNotes == 1).OrderByDescending(nh => nh.EntryDate);
            partsManagementModel.Notes.RecordHistory = new List<NotesHistoryModel>();
            foreach (NotesHistory recordHistory in recordHistories)
            {
                partsManagementModel.Notes.RecordHistory.Add(new NotesHistoryModel(recordHistory));
            }

            //partsManagementModel.Notes.FollowUpRequestList = FormalBrothersEntitites.AllFormalBrothersStatus.Where(a => a.StatusFor == "Follow Up Call" && a.Active == 1).ToList();
            partsManagementModel.Notes.FollowUpRequestID = partsManagementModel.WorkOrder.FollowupCallID.ToString();
            partsManagementModel.Notes.WorkOrderID = partsManagementModel.WorkOrder.WorkorderID;
            if (partsManagementModel.WorkOrder.ProjectFlatRate.HasValue)
            {
                partsManagementModel.Notes.ProjectFlatRate = Math.Round(partsManagementModel.WorkOrder.ProjectFlatRate.Value, 2);
            }
            if (partsManagementModel.WorkOrder.ProjectID.HasValue)
            {
                partsManagementModel.Notes.ProjectNumber = partsManagementModel.WorkOrder.ProjectID.Value;
            }

            return View(partsManagementModel);
        }*/
    }
}