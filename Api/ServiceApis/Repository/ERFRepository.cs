using DataAccess.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServiceApis.IRepository;
using ServiceApis.Models;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using ServiceApis.Utilities;
using System.Data.Entity.Validation;
using System.Security.Claims;
using System.Data.SqlClient;

namespace ServiceApis.Repository
{
    public class ERFRepository:IERFRepository
    {
        private readonly FBContext _context;
        private readonly ICustomerRepository _customerRepository;
        private readonly IWorkorderRepository _workorderRepository;
        public ERFRepository(FBContext context, ICustomerRepository customerRepository, IWorkorderRepository workorderRepository)
        {
            _context = context;
            _customerRepository = customerRepository;
            _workorderRepository = workorderRepository;
        }
        public ResultResponse<ERFResponseClass> SaveERFData(ERFRequestModel ErfData, int userId, string userName)
        {
            ErfModel erfMdl = new ErfModel();
            CustomerModel custMdl = _customerRepository.GetCustomerDetails(ErfData.AccountNumber);
            ResultResponse<ERFResponseClass> result = new ResultResponse<ERFResponseClass>();

            Erf erfResult = new Erf();
            if (custMdl == null)
            {
                result.responseCode = 500;
                result.Message = ErfData.AccountNumber + " - "+ ErfData.MainContactName + " : Invalid Customer";
                result.IsSuccess = false;

                return result;
            }

            DateTime currentDate = DateTime.Now;
            string message = "";
            bool validFlag = true;
            if (string.IsNullOrEmpty(ErfData.OrderType))
            {
                message += " | Order Type Required ";
                validFlag = false;
            }
            if (string.IsNullOrEmpty(ErfData.ShipToBranch))
            {
                message += " | ShipTo Branch Required";
                validFlag = false;
            }
            if (ErfData.FormDate == null)
            {
                message += " | Form Date Required";
                validFlag = false;
            }
            if (ErfData.ERFReceivedDate == null)
            {
                message += " | ERF Received Date Required";
                validFlag = false;
            }
            if (ErfData.ERFProcessedDate == null)
            {
                message += " | ERF Processed Date Required";
                validFlag = false;
            }
            if (ErfData.InstallDate == null)
            {
                message += " | InstallDate Date Required";
                validFlag = false;
            }
            if (string.IsNullOrEmpty(ErfData.HoursofOperation))
            {
                message += " | Hours Of Operation Required";
                validFlag = false;
            }
            if (string.IsNullOrEmpty(ErfData.InstallLocation))
            {
                message += " | Install Location Required";
                validFlag = false;
            }
            if (string.IsNullOrEmpty(ErfData.SiteReady))
            {
                message += " | Site Ready Value Required";
                validFlag = false;
            }
            if (ErfData.AdditionalNSV == 0)
            {
                message += " | Additional NSV Required";
                validFlag = false;
            }

            erfMdl.Customer = custMdl;
            erfMdl.CreatedBy = "Bulk Upload";
            erfMdl.CrateWorkOrder = ErfData.CreateWorkorder;
            erfMdl.ApprovalStatus = "Approved for Processing";
            erfMdl.OrderType = ErfData.OrderType;
            erfMdl.BranchName = ErfData.ShipToBranch;

            NotesModel nm = new NotesModel();
            nm.Notes = ErfData.ErfNotes;

            erfMdl.Notes = nm;

            erfMdl.ErfAssetsModel = new ErfAssetsModel();
            erfMdl.ErfAssetsModel.Erf = new Erf();

            erfMdl.ErfAssetsModel.Erf.CustomerMainContactName = ErfData.MainContactName;
            erfMdl.Customer.MainContactName = ErfData.MainContactName;
            erfMdl.Customer.PhoneNumber = ErfData.MainContactNum;
            erfMdl.ErfAssetsModel.Erf.Phone = ErfData.MainContactNum;
            erfMdl.ErfAssetsModel.Erf.DateErfreceived = ErfData.ERFReceivedDate;
            erfMdl.ErfAssetsModel.Erf.DateErfprocessed = ErfData.ERFProcessedDate;
            erfMdl.ErfAssetsModel.Erf.DateOnErf = ErfData.FormDate;
            erfMdl.ErfAssetsModel.Erf.OriginalRequestedDate = ErfData.InstallDate == null ? currentDate : Convert.ToDateTime(ErfData.InstallDate);
            erfMdl.ErfAssetsModel.Erf.HoursofOperation = ErfData.HoursofOperation;
            erfMdl.ErfAssetsModel.Erf.InstallLocation = ErfData.InstallLocation;
            erfMdl.ErfAssetsModel.Erf.SiteReady = ErfData.SiteReady;

            List<Fbcbe> fbcbeList = Utility.GetFbcbeList(ErfData.AccountNumber, _context);//_context.Fbcbes.Where(cbe => cbe.CurrentCustomerId == ErfData.AccountNumber).ToList();
            if (fbcbeList != null)
            {
                erfMdl.CurrentEquipmentTotal = fbcbeList.Sum(eq => eq.InitialValue).Value;
            }
            else
            {
                erfMdl.CurrentEquipmentTotal = 0;
            }

            erfMdl.CurrentNSV = erfMdl.Customer.NetSalesAmt;
            erfMdl.ContributionMargin = string.IsNullOrEmpty(erfMdl.Customer.ContributionMargin) ? "" : (Convert.ToDouble(erfMdl.Customer.ContributionMargin) * 100) + "%";
            erfMdl.TotalNSV = ErfData.AdditionalNSV;



            List<ERFManagementEquipmentModel> erfEqpList = new List<ERFManagementEquipmentModel>();
            List<ERFManagementExpendableModel> erfExpList = new List<ERFManagementExpendableModel>();
            foreach (ERFEquipmentModel dataEqp in ErfData.EquipmentData)
            {
                ERFManagementEquipmentModel eqp = new ERFManagementEquipmentModel();
                string eqpCategory = string.IsNullOrEmpty(dataEqp.EqpCategory) ? "" : dataEqp.EqpCategory.Replace('\"', ' ').Trim();
                if (!string.IsNullOrEmpty(eqpCategory))
                {
                    Contingent eqpCon = _context.Contingents.Where(c => c.ContingentName.ToLower() == eqpCategory.ToLower()).FirstOrDefault();
                    if (eqpCon != null)
                    {
                        eqp.Category = eqpCon.ContingentId;

                        string eqpBrand = string.IsNullOrEmpty(dataEqp.EqpBrand) ? "" : dataEqp.EqpBrand.Replace('\"', ' ').Trim();

                        ContingentDetail eqpConDtl = _context.ContingentDetails.Where(c => c.Name.ToLower() == eqpBrand.ToLower()).FirstOrDefault();
                        if (eqpConDtl != null)
                        {
                            eqp.Brand = eqpConDtl.Id;

                            eqp.Quantity = dataEqp.EqpQuantity;
                            eqp.Substitution = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataEqp.EqpSubstitutionPossible.ToLower());
                            eqp.TransactionType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataEqp.EqpTransType.ToLower());
                            eqp.EquipmentType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataEqp.EqpType.ToLower());
                            eqp.Branch = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataEqp.EqpUsingBranch.ToLower());

                            if (dataEqp.EqpType.ToLower() == "new")
                            {
                                if (dataEqp.EqpTransType.ToLower() == "case sale")
                                {
                                    eqp.LaidInCost = Convert.ToDouble(eqpConDtl.LaidInCost);
                                    eqp.RentalCost = Convert.ToDouble(eqpConDtl.CashSale);
                                }
                                else if (dataEqp.EqpTransType.ToLower() == "rental")
                                {
                                    double laidCost = Convert.ToDouble(eqpConDtl.LaidInCost);
                                    eqp.LaidInCost = laidCost;
                                    eqp.RentalCost = (laidCost) / 24;
                                }
                                if (dataEqp.EqpTransType.ToLower() == "loan")
                                {
                                    eqp.LaidInCost = Convert.ToDouble(eqpConDtl.LaidInCost);
                                    eqp.RentalCost = 0;
                                }
                            }
                            else if (dataEqp.EqpType.ToLower() == "refurb")
                            {
                                if (dataEqp.EqpTransType.ToLower() == "case sale")
                                {
                                    double laidCost = Convert.ToDouble(eqpConDtl.LaidInCost) * 0.75;
                                    eqp.LaidInCost = laidCost;
                                    eqp.RentalCost = laidCost + (0.3 * laidCost);
                                }
                                else if (dataEqp.EqpTransType.ToLower() == "rental")
                                {
                                    double laidCost = Convert.ToDouble(eqpConDtl.LaidInCost);
                                    eqp.LaidInCost = laidCost;
                                    eqp.RentalCost = (laidCost * 0.75) / 24;
                                }
                                if (dataEqp.EqpTransType.ToLower() == "loan")
                                {
                                    double laidCost = Convert.ToDouble(eqpConDtl.LaidInCost) * 0.75;
                                    eqp.LaidInCost = laidCost;
                                    eqp.RentalCost = 0;
                                }
                            }

                            eqp.TotalCost = dataEqp.EqpQuantity * eqp.LaidInCost;
                            erfEqpList.Add(eqp);
                        }
                        else
                        {
                            validFlag = false;
                            message += " | Invalid Equipment Brand ";
                        }
                    }
                    else
                    {
                        validFlag = false;
                        message += " | Invalid Equipment Category ";
                    }
                }

            }
            foreach (ERFExpendableModel dataExp in ErfData.ExpendableData)
            {
                ERFManagementExpendableModel exp = new ERFManagementExpendableModel();
                string expCategory = string.IsNullOrEmpty(dataExp.ExpCategory) ? "" : dataExp.ExpCategory.Replace('\"', ' ').Trim();
                if (!string.IsNullOrEmpty(expCategory))
                {
                    Contingent expCon = _context.Contingents.Where(c => c.ContingentName.ToLower() == expCategory.ToLower()).FirstOrDefault();
                    if (expCon != null)
                    {
                        exp.Category = expCon.ContingentId;

                        string expBrand = string.IsNullOrEmpty(dataExp.ExpBrand) ? "" : dataExp.ExpBrand.Replace('\"', ' ').Trim();
                        ContingentDetail expConDtl = _context.ContingentDetails.Where(c => c.Name.ToLower() == expBrand.ToLower()).FirstOrDefault();
                        if (expConDtl != null)
                        {
                            exp.Brand = expConDtl.Id;

                            exp.Quantity = dataExp.ExpQuantity;
                            exp.Substitution = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataExp.ExpSubstitutionPossible.ToLower());
                            exp.TransactionType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataExp.ExpTransType.ToLower());
                            exp.EquipmentType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataExp.ExpType.ToLower());
                            exp.Branch = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataExp.ExpUsingBranch.ToLower());


                            if (dataExp.ExpTransType.ToLower() == "case sale")
                            {
                                double laidCost = Convert.ToDouble(expConDtl.LaidInCost);
                                exp.LaidInCost = laidCost;
                                exp.RentalCost = Convert.ToDouble(expConDtl.CashSale);
                            }
                            else if (dataExp.ExpTransType.ToLower() == "rental")
                            {
                                double laidCost = Convert.ToDouble(expConDtl.LaidInCost);
                                exp.LaidInCost = laidCost;
                                exp.RentalCost = Convert.ToDouble(expConDtl.Rental);
                            }
                            if (dataExp.ExpTransType.ToLower() == "loan")
                            {
                                double laidCost = Convert.ToDouble(expConDtl.LaidInCost);
                                exp.LaidInCost = laidCost;
                                exp.RentalCost = 0;
                            }

                            exp.TotalCost = dataExp.ExpQuantity * exp.LaidInCost;

                            erfExpList.Add(exp);
                        }
                        else
                        {
                            validFlag = false;
                            message += " | Invalid Expendable Brand ";
                        }
                    }
                    else
                    {
                        validFlag = false;
                        message += " | Invalid Expendable Category ";
                    }
                }

            }


            if (validFlag)
            {
                erfMdl.ErfAssetsModel.EquipmentList = erfEqpList;
                erfMdl.ErfAssetsModel.ExpendableList = erfExpList;


                Erf erf = new Erf();
                int resultValue = ERFSave(erfMdl, userId, userName, out erf, out message);

                result.responseCode = 200;
                result.Data = new ERFResponseClass();
                result.Data.WorkorderId = Convert.ToInt32(erf.WorkorderId);
                result.Data.ERFId = Convert.ToInt32(erf.ErfId);
                result.Message = "";
                result.IsSuccess = true;

                return result;


            }
            else
            {
                result.responseCode = 500;
                result.Data = new ERFResponseClass();
                result.Data.WorkorderId = 0;
                result.Data.ERFId = 0;
                result.Message = "Save ERF Failed, Please check the Reques Payload";
                result.IsSuccess = false;

                return result;
            }
        }

        private int ERFSave(ErfModel erfModel, int userId, string userName, out Erf erf, out string message)
        {
            int returnValue = 0;
            message = string.Empty;
            erf = null;

            using (var _context = new FBContext())
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {

                        //WorkorderController wc = new WorkorderController();
                        WorkOrder workOrder = null;
                        IndexCounterModel counter = Utility.GetIndexCounter("ERFNO", 1);
                        counter.IndexValue++;

                        erfModel.ErfAssetsModel.Erf.ErfId = counter.IndexValue.Value.ToString();

                        if (erfModel.Customer != null)
                        {
                            erf = new Erf();
                            erf.ErfId = erfModel.ErfAssetsModel.Erf.ErfId;
                            erf.CustomerAddress = erfModel.Customer.Address;
                            erf.CustomerCity = erfModel.Customer.City;
                            if (!string.IsNullOrWhiteSpace(erfModel.Customer.CustomerId))
                            {
                                erf.CustomerId = new Nullable<int>(Convert.ToInt32(erfModel.Customer.CustomerId));
                            }
                            erf.CustomerMainContactName = erfModel.Customer.MainContactName;
                            erf.CustomerMainEmail = erfModel.Customer.MainEmailAddress;
                            erf.CustomerName = erfModel.Customer.CustomerName;

                            if (erfModel.Customer.PhoneNumber != null)
                            {
                                erf.CustomerPhone = erfModel.Customer.PhoneNumber.Replace("(", "").Replace(")", "").Replace("-", "");
                            }
                            erf.CustomerPhoneExtn = erfModel.Customer.PhoneExtn;
                            erf.CustomerState = erfModel.Customer.State;
                            erf.CustomerZipCode = erfModel.Customer.ZipCode;

                            erf.EntryDate = DateTime.Now; //Utility.GetCurrentTime(erfModel.Customer.ZipCode);
                            erf.ModifiedDate = erf.EntryDate;

                            erf.ModifiedUserId = userId;
                            erf.EntryUserId = userId;


                            erf.DateOnErf = erfModel.ErfAssetsModel.Erf.DateOnErf;
                            erf.DateErfreceived = erfModel.ErfAssetsModel.Erf.DateErfreceived;
                            erf.DateErfprocessed = erfModel.ErfAssetsModel.Erf.DateErfprocessed;
                            erf.OriginalRequestedDate = erfModel.ErfAssetsModel.Erf.OriginalRequestedDate;
                            erf.HoursofOperation = erfModel.ErfAssetsModel.Erf.HoursofOperation;
                            erf.InstallLocation = erfModel.ErfAssetsModel.Erf.InstallLocation;
                            erf.UserName = erfModel.ErfAssetsModel.Erf.UserName;
                            erf.Phone = Utilities.Utility.FormatPhoneNumber(erfModel.ErfAssetsModel.Erf.Phone);
                            erf.TotalNsv = erfModel.TotalNSV;

                            /*decimal currentNSV = Convert.ToDecimal(erfModel.TotalNSV + erfModel.CurrentNSV);
                            erf.CurrentNSV = currentNSV;*/

                            erf.CurrentNsv = erfModel.CurrentNSV;
                            erf.ContributionMargin = string.IsNullOrEmpty(erfModel.ContributionMargin) ? "" : erfModel.ContributionMargin;

                            erf.CurrentEqp = erfModel.CurrentEquipmentTotal;
                            erf.AdditionalEqp = erfModel.AdditionalEquipmentTotal;
                            erf.ApprovalStatus = erfModel.ApprovalStatus == null ? "" : erfModel.ApprovalStatus;

                            erf.SiteReady = erfModel.ErfAssetsModel.Erf.SiteReady;

                            erf.OrderType = erfModel.OrderType;
                            erf.ShipToBranch = erfModel.BranchName;
                            erf.ShipToJde = erfModel.ShipToCustomer;

                            //erf.ERFStatus = "Pending";

                        }
                        _context.Erves.Add(erf);

                        DateTime CurrentTime = DateTime.Now; //Utility.GetCurrentTime(erfModel.Customer.ZipCode);
                        int effectedRecords = 0;
                        try
                        {
                            //JsonResult jsonResult = new JsonResult();

                            if (erfModel.CrateWorkOrder && erf.ApprovalStatus.ToLower() == "approved for processing")
                            {
                                WorkorderManagementModel workorderModel = new WorkorderManagementModel();
                                //workorderModel.Closure = new WorkOrderClosureModel();
                                workorderModel.Customer = erfModel.Customer;
                                workorderModel.Customer.CustomerId = erfModel.Customer.CustomerId;
                                workorderModel.Notes = erfModel.Notes;
                                //workorderModel.Operation = WorkOrderManagementSubmitType.CREATEWORKORDER;
                                workorderModel.WorkOrder = new WorkOrder();
                                workorderModel.WorkOrder.CallerName = "N/A";
                                workorderModel.WorkOrder.WorkorderContactName = "N/A";
                                workorderModel.WorkOrder.HoursOfOperation = "N/A";
                                workorderModel.WorkOrder.WorkorderCalltypeid = 1300;
                                workorderModel.WorkOrder.WorkorderCalltypeDesc = "Installation";
                                workorderModel.WorkOrder.WorkorderErfid = erfModel.ErfAssetsModel.Erf.ErfId;
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
                                workorderModel.NewNotes = new List<NewNotesModel>();
                                workorderModel.NewNotes = erfModel.NewNotes;

                                workorderModel.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
                                workorderModel.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
                                workorderModel.WorkOrderParts = new List<WorkOrderPartModel>();
                                workorderModel.Erf = erfModel.ErfAssetsModel.Erf;



                                //jsonResult = wc.SaveWorkOrder(workorderModel, null, string.Empty, false, true);                         
                                //ResultResponse<ERFRequestModel> woResult = _workorderRepository.SaveWorkorderData(workorderModel, WOFBEntity, out workOrder, out message);
                                ResultResponse<ERFResponseClass> woResult = _workorderRepository.SaveWorkorderData(workorderModel, userId, userName, _context);

                                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                                //WorkOrderResults result = serializer.Deserialize<WorkOrderResults>(serializer.Serialize(jsonResult.Data));
                                if (woResult.responseCode == 200 && woResult.IsSuccess)
                                {
                                    erf.WorkorderId = Convert.ToInt32(woResult.Data.WorkorderId);
                                    ErfWorkorderLog erfWorkOrderLog = new ErfWorkorderLog();
                                    erfWorkOrderLog.ErfId = erf.ErfId;
                                    erfWorkOrderLog.WorkorderId = Convert.ToInt32(erf.WorkorderId);
                                    _context.ErfWorkorderLogs.Add(erfWorkOrderLog);

                                    NotesHistory notesHistory = new NotesHistory()
                                    {
                                        AutomaticNotes = 1,
                                        EntryDate = CurrentTime,
                                        Notes = @"Work Order created from ERF WO#: " + Convert.ToInt32(woResult.Data.WorkorderId) + @" in “MARS”!",
                                        Userid = userId,
                                        UserName = userName,
                                        ErfId = erf.ErfId,
                                        WorkorderId = erf.WorkorderId,
                                        IsDispatchNotes = 0
                                    };

                                    _context.NotesHistories.Add(notesHistory);
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

                        SaveNotes(erfModel, Convert.ToInt32(erf.WorkorderId), userId, userName);

                        string EqpNotes = "";
                        string ExpNotes = "";
                        if (erfModel.ErfAssetsModel.EquipmentList != null)
                        {
                            int eqpCount = 1;
                            foreach (ERFManagementEquipmentModel equipment in erfModel.ErfAssetsModel.EquipmentList)
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

                                Contingent eqpCon = _context.Contingents.Where(c => c.ContingentId == equipment.Category).FirstOrDefault();
                                ContingentDetail eqpConDtl = _context.ContingentDetails.Where(c => c.Id == equipment.Brand).FirstOrDefault();
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

                                _context.Fberfequipments.Add(eq);
                            }
                        }

                        if (erfModel.ErfAssetsModel.ExpendableList != null)
                        {
                            int expCount = 1;
                            foreach (ERFManagementExpendableModel expItems in erfModel.ErfAssetsModel.ExpendableList)
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


                                Contingent expCon = _context.Contingents.Where(c => c.ContingentId == expItems.Category).FirstOrDefault();
                                ContingentDetail expConDtl = _context.ContingentDetails.Where(c => c.Id == expItems.Brand).FirstOrDefault();
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

                                _context.Fberfexpendables.Add(eq);
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

                        _context.NotesHistories.Add(eqpNotesHistory);

                        decimal? eqpTotal = Convert.ToDecimal(erfModel.ErfAssetsModel.EquipmentList.Sum(x => x.TotalCost));
                        decimal? expTotal = Convert.ToDecimal(erfModel.ErfAssetsModel.ExpendableList.Sum(x => x.TotalCost));

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

                        int woSaveEffectRecords = _context.SaveChanges();
                        int woReturnValue = woSaveEffectRecords > 0 ? 1 : 0;

                        if (((!erfModel.CrateWorkOrder && erf.ApprovalStatus.ToLower() == "approved for processing") && woReturnValue == 1)
                            || (erfModel.CrateWorkOrder || erf.ApprovalStatus.ToLower() != "approved for processing"))
                        {
                            effectedRecords = _context.SaveChanges();
                            returnValue = effectedRecords > 0 ? 1 : 0;
                        }
                        transaction.Commit();
                    }
                    catch(Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
            }

            return returnValue;
        }

        public void SaveNotes(ErfModel erfManagement, int erfWorkorderId , int userId, string userName)
        {
            if (erfManagement.NewNotes != null)
            {

                TimeZoneInfo newTimeZoneInfo = null;
                Utility.GetCustomerTimeZone(erfManagement.Customer.ZipCode, _context);

                DateTime CurrentTime = DateTime.Now; //Utility.GetCurrentTime(erfManagement.Customer.ZipCode);

                foreach (NewNotesModel newNotesModel in erfManagement.NewNotes)
                {
                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = CurrentTime,
                        Notes = newNotesModel.Text,
                        Userid = userId,
                        UserName = userName,
                        ErfId = erfManagement.ErfAssetsModel.Erf.ErfId,
                        WorkorderId = erfWorkorderId == 0 ? null : (int?)erfWorkorderId,// erfManagement.ErfAssetsModel.Erf.WorkorderID,
                        IsDispatchNotes = 0
                    };
                    _context.NotesHistories.Add(notesHistory);
                }
            }
        }
    }
}
