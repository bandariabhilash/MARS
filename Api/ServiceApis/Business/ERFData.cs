using AutoMapper;
using DataAccess.Db;
using ServiceApis.Controllers;
using ServiceApis.Models;
using System;
using System.Data.Entity.Validation;
using System.Security.Claims;

namespace ServiceApis.Business
{
    public class ERFData
    {
        public int userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        public string userName = User.FindFirstValue(ClaimTypes.GivenName);

        public int ERFSave(ErfModel erfModel, out Erf erf, out string message, FBContext context)
        {
            int returnValue = 0;
            message = string.Empty;
            erf = null;
            //FarmerBrothersEntities WOFBEntity = new FarmerBrothersEntities();
            WorkorderController wc = new WorkorderController();
            WorkOrder workOrder = null;
            IndexCounterModel counter = Utilities.Utility.GetIndexCounter("ERFNO", 1);
            counter.IndexValue++;
            erfModel.ErfData.ErfId = counter.IndexValue.Value.ToString();

            if (erfModel.Customer != null)
            {
                erf = new Erf();
                erf.ErfId = erfModel.ErfData.ErfId;
                erf.CustomerAddress = erfModel.Customer.Address;
                erf.CustomerCity = erfModel.Customer.City;
                if (!string.IsNullOrWhiteSpace(erfModel.Customer.CustomerId))
                {
                    erf.CustomerId = new Nullable<int>(Convert.ToInt32(erfModel.Customer.CustomerId));
                }
                erf.CustomerMainContactName = erfModel.Customer.MainContactName;
                //erf.CustomerMainEmail = erfModel.Customer.MainEmailAddress;
                erf.CustomerName = erfModel.Customer.CustomerName;

                if (erfModel.Customer.PhoneNumber != null)
                {
                    erf.CustomerPhone = erfModel.Customer.PhoneNumber.Replace("(", "").Replace(")", "").Replace("-", "");
                }
                
                erf.CustomerState = erfModel.Customer.State;
                erf.CustomerZipCode = erfModel.Customer.ZipCode;

                erf.EntryDate = Utilities.Utility.GetCurrentTime(erfModel.Customer.ZipCode, context);
                erf.ModifiedDate = erf.EntryDate;
                /*if (System.Web.HttpContext.Current.Session["UserId"] != null)
                {
                    erf.ModifiedUserID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
                    erf.EntryUserID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
                }
                else
                {
                    erf.ModifiedUserID = -1;
                    erf.EntryUserID = -1;
                }*/

                erf.DateOnErf = erfModel.ErfData.DateOnErf;
                erf.DateErfreceived = erfModel.ErfData.DateErfreceived;
                erf.DateErfprocessed = erfModel.ErfData.DateErfprocessed;
                erf.OriginalRequestedDate = erfModel.ErfData.OriginalRequestedDate;
                erf.HoursofOperation = erfModel.ErfData.HoursofOperation;
                erf.InstallLocation = erfModel.ErfData.InstallLocation;
                erf.UserName = erfModel.ErfData.UserName;
                erf.Phone = Utilities.Utility.FormatPhoneNumber(erfModel.ErfData.Phone);
                erf.TotalNsv = erfModel.TotalNSV;

                erf.CurrentNsv = erfModel.CurrentNSV;
                erf.ContributionMargin = string.IsNullOrEmpty(erfModel.ContributionMargin) ? "" : erfModel.ContributionMargin;

                erf.CurrentEqp = erfModel.CurrentEquipmentTotal;
                erf.AdditionalEqp = erfModel.AdditionalEquipmentTotal;
                erf.ApprovalStatus = erfModel.ApprovalStatus == null ? "" : erfModel.ApprovalStatus;

                erf.SiteReady = erfModel.ErfData.SiteReady;

                erf.OrderType = erfModel.OrderType;
                erf.ShipToBranch = erfModel.BranchName;
                erf.ShipToJde = erfModel.ShipToCustomer;

            }
            context.Erves.Add(erf);

            DateTime CurrentTime = Utilities.Utility.GetCurrentTime(erfModel.Customer.ZipCode, context);
            int effectedRecords = 0;
            try
            {

                if (erfModel.CrateWorkOrder == false && erf.ApprovalStatus.ToLower() == "approved for processing")
                {
                    WorkorderManagementModel workorderModel = new WorkorderManagementModel();
                    
                    workorderModel.Customer = erfModel.Customer;
                    workorderModel.Customer.CustomerId = erfModel.Customer.CustomerId;
                    //workorderModel.Notes = erfModel.Notes;
                    
                    workorderModel.WorkOrder = new WorkOrder();
                    workorderModel.WorkOrder.CallerName = "N/A";
                    workorderModel.WorkOrder.WorkorderContactName = "N/A";
                    workorderModel.WorkOrder.HoursOfOperation = "N/A";
                    workorderModel.WorkOrder.WorkorderCalltypeid = 1300;
                    workorderModel.WorkOrder.WorkorderCalltypeDesc = "Installation";
                    workorderModel.WorkOrder.WorkorderErfid = erfModel.ErfData.ErfId;
                    workorderModel.WorkOrder.PriorityCode = 54;
                    workorderModel.WorkOrder.WorkOrderBrands = new List<WorkOrderBrand>();
                    WorkOrderBrand brand = new WorkOrderBrand();
                    brand.BrandId = 997;
                    workorderModel.WorkOrder.WorkOrderBrands.Add(brand);
                    

                    workorderModel.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
                    workorderModel.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();

                    workorderModel.Erf = Mapper.Map<Erf>(erfModel.ErfData);
                    workorderModel.Erf = new Erf();
                    workorderModel.Erf.ErfId = erfModel.ErfData.ErfId;




                    int WoResult = wc.ERFWorkOrderSave(workorderModel, context, out workOrder, out message);


                    if (WoResult > 0)
                    {
                        erf.WorkorderId = Convert.ToInt32(workOrder.WorkorderId);
                        ErfWorkorderLog erfWorkOrderLog = new ErfWorkorderLog();
                        erfWorkOrderLog.ErfId = erf.ErfId;
                        erfWorkOrderLog.WorkorderId = Convert.ToInt32(erf.WorkorderId);
                        context.ErfWorkorderLogs.Add(erfWorkOrderLog);

                        NotesHistory notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 1,
                            EntryDate = CurrentTime,
                            Notes = @"Work Order created from ERF SERVICE WO#: " + Convert.ToInt32(workOrder.WorkorderId) + @" in “MARS”!",
                            Userid = userId,
                            UserName = userName,
                            ErfId = erf.ErfId,
                            WorkorderId = erf.WorkorderId,
                            IsDispatchNotes = 0
                        };

                        context.NotesHistories.Add(notesHistory);
                    }
                }


            }
            catch (DbEntityValidationException e)
            {
                string errormsg = string.Empty;
                foreach (var eve in e.EntityValidationErrors)
                {
                    errormsg += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    errormsg += Environment.NewLine;
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errormsg += string.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                        errormsg += Environment.NewLine;
                    }
                }
                throw;
            }

            SaveNotes(erfModel, Convert.ToInt32(erf.WorkorderId), context);

            NotesHistory notesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = CurrentTime,
                Notes = @"ERF created from ERF SERVICE ERF#: " + Convert.ToInt32(erf.ErfId),
                Userid = userId,
                UserName = userName,
                ErfId = erf.ErfId,
                WorkorderId = erf.WorkorderId,
                IsDispatchNotes = 0
            };

            context.NotesHistories.Add(notesHistory);

            string EqpNotes = "";
            string ExpNotes = "";
            if (erfModel.ErfData.EquipmentList != null)
            {
                int eqpCount = 1;
                foreach (ERFManagementEquipmentModel equipment in erfModel.ErfData.EquipmentList)
                {
                    Fberfequipment eq = new Fberfequipment()
                    {
                        Erfid = erf.ErfId,
                        WorkOrderId = erf.WorkorderId,
                        ModelNo = equipment.ModelNo,
                        Quantity = equipment.Quantity,
                        ProdNo = equipment.ProdNo,
                        EquipmentType = equipment.EquipmentType,
                        UnitPrice = Convert.ToDecimal(equipment.UnitPrice),
                        TransactionType = equipment.TransactionType,
                        Substitution = equipment.Substitution,
                        Extra = equipment.Extra,
                        Description = equipment.Description,
                        LaidInCost = Convert.ToDecimal(equipment.LaidInCost),
                        RentalCost = Convert.ToDecimal(equipment.RentalCost),
                        TotalCost = Convert.ToDecimal(equipment.TotalCost),
                        ContingentCategoryId = equipment.Category,
                        ContingentCategoryTypeId = equipment.Brand,
                        UsingBranch = equipment.Branch

                    };

                    Contingent eqpCon = context.Contingents.Where(c => c.ContingentId == equipment.Category).FirstOrDefault();
                    ContingentDetail eqpConDtl = context.ContingentDetails.Where(c => c.Id == equipment.Brand).FirstOrDefault();
                    string eqpCategoryName = "";
                    string eqpBrandName = "";
                    if (eqpCon != null)
                    {
                        eqpCategoryName = eqpCon.ContingentName;
                    }
                    if (eqpConDtl != null)
                    {
                        eqpBrandName = eqpConDtl.Name;
                    }

                    EqpNotes += eqpCount + ") Category: " + eqpCategoryName + ", Brand: " + eqpBrandName + ", Quantity: " + equipment.Quantity + ", UsingBranch: " + equipment.Branch + "\n\r";
                    eqpCount++;

                    context.Fberfequipments.Add(eq);
                }
            }

            if (erfModel.ErfData.ExpendableList != null)
            {
                int expCount = 1;
                foreach (ERFManagementExpendableModel expItems in erfModel.ErfData.ExpendableList)
                {
                    Fberfexpendable eq = new Fberfexpendable()
                    {
                        Erfid = erf.ErfId,
                        WorkOrderId = erf.WorkorderId,
                        ModelNo = expItems.ModelNo,
                        Quantity = expItems.Quantity,
                        ProdNo = expItems.ProdNo,
                        UnitPrice = Convert.ToDecimal(expItems.UnitPrice),
                        TransactionType = expItems.TransactionType,
                        Substitution = expItems.Substitution,
                        EquipmentType = expItems.EquipmentType,
                        Extra = expItems.Extra,
                        Description = expItems.Description,
                        LaidInCost = Convert.ToDecimal(expItems.LaidInCost),
                        RentalCost = Convert.ToDecimal(expItems.RentalCost),
                        TotalCost = Convert.ToDecimal(expItems.TotalCost),
                        ContingentCategoryId = expItems.Category,
                        ContingentCategoryTypeId = expItems.Brand,
                        UsingBranch = expItems.Branch

                    };


                    Contingent expCon = context.Contingents.Where(c => c.ContingentId == expItems.Category).FirstOrDefault();
                    ContingentDetail expConDtl = context.ContingentDetails.Where(c => c.Id == expItems.Brand).FirstOrDefault();
                    string expCategoryName = "";
                    string expBrandName = "";
                    if (expCon != null)
                    {
                        expCategoryName = expCon.ContingentName;
                    }
                    if (expConDtl != null)
                    {
                        expBrandName = expConDtl.Name;
                    }

                    ExpNotes += expCount + ") Category: " + expCategoryName + ", Brand: " + expBrandName + ", Quantity: " + expItems.Quantity + ", UsingBranch: " + expItems.Branch + "\n\r";
                    expCount++;

                    context.Fberfexpendables.Add(eq);
                }
            }


            NotesHistory eqpNotesHistory = new NotesHistory()
            {
                AutomaticNotes = 0,
                EntryDate = CurrentTime,
                Notes = "Equipments: " + EqpNotes + "Expendables: " + ExpNotes,
                Userid = userId,
                UserName = userName,
                ErfId = erf.ErfId,
                WorkorderId = erf.WorkorderId,
                IsDispatchNotes = 0
            };

            context.NotesHistories.Add(eqpNotesHistory);



            decimal? eqpTotal = Convert.ToDecimal(erfModel.ErfData.EquipmentList.Sum(x => x.TotalCost));
            decimal? expTotal = Convert.ToDecimal(erfModel.ErfData.ExpendableList.Sum(x => x.TotalCost));

            decimal GrandTotal = 0;

            if (eqpTotal != null && expTotal != null)
            {
                GrandTotal = Convert.ToDecimal(eqpTotal + expTotal);
            }
            else if (eqpTotal != null && expTotal == null)
            {
                GrandTotal = Convert.ToDecimal(eqpTotal);
            }
            else if (eqpTotal == null && expTotal != null)
            {
                GrandTotal = Convert.ToDecimal(expTotal);
            }

            if (GrandTotal >= 10000)
            {
                erf.Erfstatus = "Pending CapEx-FA";
            }
            else
            {
                erf.Erfstatus = "Pending";
            }
            erf.CashSaleStatus = "1"; // Default value for CashSaleStatus while ERF Creation

            int woSaveEffectRecords = context.SaveChanges();
            int woReturnValue = woSaveEffectRecords > 0 ? 1 : 0;

            if (((erfModel.CrateWorkOrder == false && erf.ApprovalStatus.ToLower() == "approved for processing") && woReturnValue == 1)
                || (erfModel.CrateWorkOrder == true || erf.ApprovalStatus.ToLower() != "approved for processing"))
            {
                effectedRecords = context.SaveChanges();
                returnValue = effectedRecords > 0 ? 1 : 0;
            }

            //ERFEmail(erf.ErfID, erf.WorkorderID, erfModel.CrateWorkOrder, erfModel.ApprovalStatus);

            return returnValue;
        }

        public void SaveNotes(ErfModel erfManagement, int erfWorkorderId, FBContext context)
        {
            if (erfManagement.Notes != null)
            {

                TimeZoneInfo newTimeZoneInfo = null;
                Utilities.Utility.GetCustomerTimeZone(erfManagement.Customer.ZipCode, context);

                DateTime CurrentTime = Utilities.Utility.GetCurrentTime(erfManagement.Customer.ZipCode, context);

                foreach (NewNotesModel newNotesModel in erfManagement.Notes)
                {
                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = CurrentTime,
                        Notes = newNotesModel.Text,
                        Userid = userId,
                        UserName = userName,
                        ErfId = erfManagement.ErfData.ErfId,
                        WorkorderId = erfWorkorderId == 0 ? null : (int?)erfWorkorderId,// erfManagement.ErfData.Erf.WorkorderID,
                        IsDispatchNotes = 0
                    };
                    context.NotesHistories.Add(notesHistory);
                }
            }
        }
    }
}
