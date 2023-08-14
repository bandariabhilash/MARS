using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace FarmerBrothers.Controllers
{
    public class ERFNewController : BaseController
    {
        MarsViews objMarsview = new MarsViews();
        // GET: ERFNew
        public ActionResult ERFNew(int? isBack)
        {
            CustomerSearchModel customerSearchModel = new CustomerSearchModel();
            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                customerSearchModel = TempData["SearchCriteria"] as CustomerSearchModel;
                TempData["SearchCriteria"] = customerSearchModel;
            }
            else
            {
                customerSearchModel = new CustomerSearchModel();
                TempData["SearchCriteria"] = null;
            }

            customerSearchModel.CustomerSearchResults = new List<Contact>();
            return View(customerSearchModel);
        }

        [HttpGet]
        public ActionResult ERFNewCreate(int? customerId, int? erfId)
        {
            ErfModel erfManagementModel = ConstructERFManagementModel(customerId, erfId);

            return View(erfManagementModel);
        }

        private ErfModel ConstructERFManagementModel(int? customerId, int? erfId)
        {
            ErfModel erfManagementModel = new ErfModel();
            erfManagementModel.WorkOrderParts = new List<WorkOrderPartModel>();

            erfManagementModel.ErfAssetsModel = new ErfAssetsModel();
            erfManagementModel.ErfAssetsModel.Erf = new Erf();

            erfManagementModel.ErfAssetsModel.Erf.DateOnERF = DateTime.UtcNow;
            erfManagementModel.ErfAssetsModel.Erf.DateERFReceived = DateTime.UtcNow;
            erfManagementModel.ErfAssetsModel.Erf.DateERFProcessed = DateTime.UtcNow;
            erfManagementModel.ErfAssetsModel.Erf.OriginalRequestedDate = null;

            erfManagementModel.Customer = new CustomerModel();
            int cid = Convert.ToInt32(customerId);
            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == cid).FirstOrDefault();
            erfManagementModel.Customer = new CustomerModel(customer, FarmerBrothersEntitites);
            erfManagementModel.Customer.ErfId = "0";

            erfManagementModel.SiteReadyList = new List<string>();
            erfManagementModel.SiteReadyList.Add("");
            erfManagementModel.SiteReadyList.Add("YES");
            erfManagementModel.SiteReadyList.Add("NO");

            erfManagementModel.OrderTypeList = GetOrderTypeList();

            //    new List<string>();
            //erfManagementModel.OrderTypeList.Add("");
            //erfManagementModel.OrderTypeList.Add("New Account Setup");
            //erfManagementModel.OrderTypeList.Add("Penetration");
            //erfManagementModel.OrderTypeList.Add("Cash Sale");
            //erfManagementModel.OrderTypeList.Add("Replacement");
            //erfManagementModel.OrderTypeList.Add("Equipment Failure - EMG");
            //erfManagementModel.OrderTypeList.Add("Equipment Accessories Only");

            erfManagementModel.BranchList = GetBranchList();
            erfManagementModel.ShipToCustomerList = FarmerBrothersEntitites.Contacts.Where(c => c.SearchType == "C").ToList();

            List<Contingent> erfCategory = FarmerBrothersEntitites.Contingents.Where(e => e.ContingentType.ToLower() == "eqp" && e.IsActive == true).ToList();//.Select(s => s.ModelNO).Distinct();
            erfManagementModel.ErfAssetsModel.ErfEqpCategory = new List<ErfEqpViewModel>();
            foreach (Contingent model in erfCategory)
            {
                erfManagementModel.ErfAssetsModel.ErfEqpCategory.Add(new ErfEqpViewModel(model.ContingentID, model.ContingentName));
            }
            erfManagementModel.ErfAssetsModel.ErfEqpCategory.Insert(0, new ErfEqpViewModel(0, ""));

            List<Contingent> erfExpCategory = FarmerBrothersEntitites.Contingents.Where(e => e.ContingentType.ToLower() == "exp" && e.IsActive == true).ToList();//.Select(s => s.ModelNO).Distinct();
            erfManagementModel.ErfAssetsModel.ErfExpCategory = new List<ErfEqpViewModel>();
            foreach (Contingent model in erfExpCategory)
            {
                erfManagementModel.ErfAssetsModel.ErfExpCategory.Add(new ErfEqpViewModel(model.ContingentID, model.ContingentName));
            }
            erfManagementModel.ErfAssetsModel.ErfExpCategory.Insert(0, new ErfEqpViewModel(0, ""));

            List<Contingent> posCategory = FarmerBrothersEntitites.Contingents.Where(e => e.ContingentType.ToLower() == "pos" && e.IsActive == true).ToList();
            erfManagementModel.ErfAssetsModel.ErfPosCategory = new List<ErfEqpViewModel>();
            foreach (Contingent model in posCategory)
            {
                erfManagementModel.ErfAssetsModel.ErfPosCategory.Add(new ErfEqpViewModel(model.ContingentID, model.ContingentName));
            }
            erfManagementModel.ErfAssetsModel.ErfPosCategory.Insert(0, new ErfEqpViewModel(0, ""));

            //============

            List<ContingentDetail> erfeqpModels = FarmerBrothersEntitites.ContingentDetails.Where(c => c.IsActive == true).ToList();//.Select(s => s.ModelNO).Distinct();
            erfManagementModel.ErfAssetsModel.ErfEqpModels = new List<ErfEqpViewModel>();
            erfManagementModel.ErfAssetsModel.ErfExpModels = new List<ErfEqpViewModel>();
            erfManagementModel.ErfAssetsModel.ErfPosModels = new List<ErfEqpViewModel>();
            foreach (ContingentDetail model in erfeqpModels)
            {
                erfManagementModel.ErfAssetsModel.ErfEqpModels.Add(new ErfEqpViewModel(model.ID, model.Name));
                erfManagementModel.ErfAssetsModel.ErfExpModels.Add(new ErfEqpViewModel(model.ID, model.Name));
                erfManagementModel.ErfAssetsModel.ErfPosModels.Add(new ErfEqpViewModel(model.ID, model.Name));
            }
            erfManagementModel.ErfAssetsModel.ErfEqpModels.Insert(0, new ErfEqpViewModel(0, ""));
            erfManagementModel.ErfAssetsModel.ErfExpModels.Insert(0, new ErfEqpViewModel(0, ""));
            erfManagementModel.ErfAssetsModel.ErfPosModels.Insert(0, new ErfEqpViewModel(0, ""));
            //============

            //IQueryable<string> models = FarmerBrothersEntitites.FBEquipments.Select(s => s.ModelNO).Distinct();
            erfManagementModel.ErfAssetsModel.ErfEquipmentModels = new List<VendorModelModel>();
            var data = new List<object>();            
            data.Add(new { value = "", text = "" });

            IQueryable<string> expmodels = FarmerBrothersEntitites.FBExpendables.Select(s => s.ModelNO).Distinct();
            erfManagementModel.ErfAssetsModel.ErfExpendableModels = new List<VendorModelModel>();
            foreach (string model in expmodels)
            {
                erfManagementModel.ErfAssetsModel.ErfExpendableModels.Add(new VendorModelModel(model));
            }
            erfManagementModel.ErfAssetsModel.ErfExpendableModels.OrderBy(v => v.Model).ToList();


            string eqproductNum = FarmerBrothersEntitites.FBEquipments.Select(s => s.ProdNo).FirstOrDefault();
            erfManagementModel.ErfAssetsModel.ErfEquipmentProducts = string.Empty;
            if (!string.IsNullOrEmpty(eqproductNum))
            {
                erfManagementModel.ErfAssetsModel.ErfEquipmentProducts = eqproductNum;
            }

            erfManagementModel.Customer.TotalCallsCount = CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, customerId.ToString());
            Contact serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == (int)customerId).FirstOrDefault();
            if (serviceCustomer != null)
            {
                if (!string.IsNullOrEmpty(serviceCustomer.BillingCode))
                {
                    erfManagementModel.Customer.IsBillable = CustomerModel.IsBillableService(serviceCustomer.BillingCode, erfManagementModel.Customer.TotalCallsCount);
                    erfManagementModel.Customer.ServiceLevelDesc = CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, serviceCustomer.BillingCode);
                }
                else
                {
                    erfManagementModel.Customer.IsBillable = " ";
                    erfManagementModel.Customer.ServiceLevelDesc = " - ";
                }
            }
            
            IQueryable<string> productNum = FarmerBrothersEntitites.FBExpendables.Select(s => s.ProdNo).Distinct();
            erfManagementModel.ErfAssetsModel.ErfExpendableProducts = new List<ExpendableProduct>();
            foreach (string type in productNum)
            {
                erfManagementModel.ErfAssetsModel.ErfExpendableProducts.Add(new ExpendableProduct(type));
            }

            ExpendableProduct productType = new ExpendableProduct("");
            erfManagementModel.ErfAssetsModel.ErfExpendableProducts.Insert(0, productType);


            IQueryable<string> transType = FarmerBrothersEntitites.FBERFTransactionTypes.Where(t => t.IsActive == true).OrderBy(t => t.IsActive).Select(s => s.Type).Distinct();
            erfManagementModel.ErfAssetsModel.ErfTransactionTypes = new List<TransactionTypeModel>();
            foreach (string type in transType)
            {
                erfManagementModel.ErfAssetsModel.ErfTransactionTypes.Add(new TransactionTypeModel(type));
            }

            TransactionTypeModel transactionType = new TransactionTypeModel("");
            erfManagementModel.ErfAssetsModel.ErfTransactionTypes.Insert(0, transactionType);

            List<string> SubstituionType = new List<string>() { "", "NO", "YES" };
            erfManagementModel.ErfAssetsModel.ErfSubstituion = new List<SubstituionModel>();
            foreach (string substituion in SubstituionType)
            {
                erfManagementModel.ErfAssetsModel.ErfSubstituion.Add(new SubstituionModel(substituion));
            }

            List<string> UsingBranchType = new List<string>() { "", "NO", "YES" };
            erfManagementModel.ErfAssetsModel.UsingBranch = new List<SubstituionModel>();
            foreach (string useBrnch in UsingBranchType)
            {
                erfManagementModel.ErfAssetsModel.UsingBranch.Add(new SubstituionModel(useBrnch));
            }
            

            List<string> eqpType = new List<string>() { "", "NEW", "REFURB" };
            erfManagementModel.ErfAssetsModel.ErfEquipmentTypes = new List<EquipmentTypeModel>();
            foreach (string eqp in eqpType)
            {
                erfManagementModel.ErfAssetsModel.ErfEquipmentTypes.Add(new EquipmentTypeModel(eqp));
            }


            erfManagementModel.Notes = new NotesModel()
            {
                CustomerZipCode = erfManagementModel.Customer.ZipCode
            };
            erfManagementModel.Notes.NotesHistory = new List<NotesHistoryModel>();
            erfManagementModel.Notes.RecordHistory = new List<NotesHistoryModel>();
            erfManagementModel.Notes.CustomerNotesResults = new List<CustomerNotesModel>();
            erfManagementModel.Notes.viewProp = "ERFView";

            erfManagementModel.Notes.CustomerNotesResults = new List<CustomerNotesModel>();
            cid = Convert.ToInt32(erfManagementModel.Customer.CustomerId);
            //var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == cid && c.IsActive == true).ToList();            
            int parentId = string.IsNullOrEmpty(erfManagementModel.Customer.ParentNumber) ? 0 : Convert.ToInt32(erfManagementModel.Customer.ParentNumber);
            var custNotes = Utility.GetCustomreNotes(cid, parentId, FarmerBrothersEntitites);
            foreach (var dbCustNotes in custNotes)
            {
                erfManagementModel.Notes.CustomerNotesResults.Add(new CustomerNotesModel(dbCustNotes));
            }

            erfManagementModel.Notes.FollowUpRequestList = FarmerBrothersEntitites.AllFBStatus.Where(a => a.StatusFor == "Follow Up Call" && a.Active == 1).ToList();

            erfManagementModel.ErfAssetsModel.Reasons = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "ERF Reasons" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            AllFBStatu smuckersStatus = new AllFBStatu() { FBStatus = "Please Select", FBStatusID = -1 };
            erfManagementModel.ErfAssetsModel.Reasons.Insert(0, smuckersStatus);
            erfManagementModel.CreatedBy = UserName;

            List<FBCBE> fbcbeList = FarmerBrothersEntitites.FBCBEs.Where(cbe => cbe.CurrentCustomerId == cid).ToList();

            if(fbcbeList != null)
            {
                erfManagementModel.CurrentEquipmentTotal = fbcbeList.Sum(eq => eq.InitialValue).Value;
            }
            else
            {
                erfManagementModel.CurrentEquipmentTotal = 0;
            }

            erfManagementModel.CurrentNSV = erfManagementModel.Customer.NetSalesAmt;
            erfManagementModel.ContributionMargin = string.IsNullOrEmpty(erfManagementModel.Customer.ContributionMargin) ? "" : (Convert.ToDouble(erfManagementModel.Customer.ContributionMargin) * 100) + "%"; ;

            return erfManagementModel;
        }

        private List<ERFBranchDetails> GetBranchList()
        {
            //string strQuery = @"SELECT tech.BranchNumber, tech.BranchName
            //                    FROM TECH_HIERARCHY tech where tech.BranchName !='' and searchType='SP'
            //                    GROUP BY tech.BranchNumber, tech.BranchName";

            string strQuery = @"SELECT Id, Branch, BranchName
                                FROM ERFBranchDetails where BranchName !='' and isActive = 1";

            DataTable dt = objMarsview.fn_FSM_View(strQuery);
            List<ERFBranchDetails> Branchlists = new List<ERFBranchDetails>();
            foreach (DataRow dr in dt.Rows)
            {
                Branchlists.Add
                (
                    new ERFBranchDetails()
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        BranchNo = dr["Branch"].ToString(),
                        //BranchName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dr["BranchName"].ToString().ToLower())
                        BranchName = dr["BranchName"].ToString()
                    }
                );
            }
            return Branchlists;
        }

        private List<OrderType> GetOrderTypeList()
        {
            string strQuery = @"SELECT OrderTypeId, OrderType
                                FROM ERFOrderType where OrderType !='' and isActive = 1";

            DataTable dt = objMarsview.fn_FSM_View(strQuery);
            List<OrderType> OrderTypeList = new List<OrderType>();
            foreach (DataRow dr in dt.Rows)
            {
                OrderTypeList.Add
                (
                    new OrderType()
                    {
                        OrderTypeId = Convert.ToInt32(dr["OrderTypeId"]),
                        OrderTypeDesc = dr["OrderType"].ToString()
                    }
                );
            }

            OrderTypeList.Insert
            (0,new OrderType()
                {
                    OrderTypeId = 0,
                    OrderTypeDesc = ""
                }
            );
            return OrderTypeList;
        }

        [HttpGet]
        public JsonResult GetContingentDetails(int contingentId)
        {
            ContingentDetails contingentDtls = new ContingentDetails();

            contingentDtls.ContingentList = FarmerBrothersEntitites.ContingentDetails.Where(con => con.ContingentID == contingentId && con.IsActive == true).ToList();
                      
            var data = new List<object>();
            foreach (ContingentDetail item in contingentDtls.ContingentList)
            {
                data.Add(new { value = item.Name.ToUpper().Trim(), text = item.Name.ToUpper().Trim() });
            }
            data.Insert(0, new { value = "", text = "" });

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpGet]
        public JsonResult GetContingentDetailItem(string contingentDetailName)
        {
            ContingentDetail contingentDtl = new ContingentDetail();

            contingentDtl = FarmerBrothersEntitites.ContingentDetails.Where(con => con.Name == contingentDetailName && con.IsActive == true).FirstOrDefault();
                        
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = contingentDtl };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpGet]
        public JsonResult GetContingentItemId(string contingentName)
        {
            Contingent contingentDtl = new Contingent();

            contingentDtl = FarmerBrothersEntitites.Contingents.Where(con => con.ContingentName == contingentName).FirstOrDefault();

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = contingentDtl.ContingentID };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult EquipmentUpdate(ERFManagementEquipmentModel value)
        {
            IList<ERFManagementEquipmentModel> EquipmentItems = TempData["Equipment"] as IList<ERFManagementEquipmentModel>;
            if (EquipmentItems == null)
            {
                EquipmentItems = new List<ERFManagementEquipmentModel>();
            }
            ERFManagementEquipmentModel EquipmentItem = EquipmentItems.Where(n => n.ERFEquipmentId == value.ERFEquipmentId).FirstOrDefault();

            if (EquipmentItem != null)
            {
                EquipmentItem.ModelNo = value.ModelNo;
                EquipmentItem.ProdNo = value.ProdNo;
                EquipmentItem.Quantity = value.Quantity;
                EquipmentItem.UnitPrice = value.UnitPrice;
                EquipmentItem.Description = value.Description;
                EquipmentItem.InternalOrderNumber = value.InternalOrderNumber;
                EquipmentItem.VendorOrderNumber = value.VendorOrderNumber;
                EquipmentItem.Category = value.Category;
                EquipmentItem.Branch = value.Branch;
                EquipmentItem.Brand = value.Brand;
                EquipmentItem.Substitution = value.Substitution;
                EquipmentItem.TotalCost = value.TotalCost;
                EquipmentItem.TransactionType = value.TransactionType;
                EquipmentItem.EquipmentType = value.EquipmentType;
                EquipmentItem.LaidInCost = value.LaidInCost;
                EquipmentItem.RentalCost = value.RentalCost;
            }

            TempData["Equipment"] = EquipmentItems;
            TempData.Keep("Equipment");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EquipmentInsert(ERFManagementEquipmentModel value)
        {
            IList<ERFManagementEquipmentModel> EquipmentItems = TempData["Equipment"] as IList<ERFManagementEquipmentModel>;
            if (EquipmentItems == null)
            {
                EquipmentItems = new List<ERFManagementEquipmentModel>();
            }

            //string validationProperties = "";
            //if(string.IsNullOrEmpty(value.Branch) || value.Brand == null || value.Category == null || string.IsNullOrEmpty(value.EquipmentType)
            //    || string.IsNullOrEmpty(value.TransactionType) || string.IsNullOrEmpty(value.Substitution) || value.Quantity == null)
            //{
            //    validationProperties = "| Please Enter Required details";

            //}

            //string uPrice;
            //if (value.UnitPrice == null && value.ProdNo != null)
            //{
            //    uPrice = FarmerBrothersEntitites.FBEquipments.Where(e => e.ProdNo == value.ProdNo).Select(e => e.UnitPrice).FirstOrDefault();
            //}
            //else
            //{
            //    uPrice = value.UnitPrice.ToString();
            //}            
            if (TempData["ERFEquipmentId"] != null)
            {
                int eqpId = Convert.ToInt32(TempData["ERFEquipmentId"]);
                value.ERFEquipmentId = eqpId + 1;
                TempData["ERFEquipmentId"] = eqpId + 1;
            }
            else
            {
                value.ERFEquipmentId = 1;
                //value.UnitPrice = Convert.ToDouble(uPrice);
                TempData["ERFEquipmentId"] = 1;
            }

            EquipmentItems.Add(value);
            TempData["Equipment"] = EquipmentItems;
            TempData.Keep("Equipment");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EquipmentDelete(int key)
        {
            IList<ERFManagementEquipmentModel> EquipmentItems = TempData["Equipment"] as IList<ERFManagementEquipmentModel>;

            //if (EquipmentItems == null)
            //{
            //    FBERFEquipment fberfeqpData = FarmerBrothersEntitites.FBERFEquipments.Where(e => e.ERFEquipmentId == key).FirstOrDefault();
            //}
            ERFManagementEquipmentModel EquipmentItem = EquipmentItems.Where(n => n.ERFEquipmentId == key).FirstOrDefault();
            EquipmentItems.Remove(EquipmentItem);
            TempData["Equipment"] = EquipmentItems;
            TempData.Keep("Equipment");
            return Json(EquipmentItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExpendableUpdate(ERFManagementExpendableModel value)
        {
            IList<ERFManagementExpendableModel> ExpendableItems = TempData["Expendable"] as IList<ERFManagementExpendableModel>;
            if (ExpendableItems == null)
            {
                ExpendableItems = new List<ERFManagementExpendableModel>();
            }
            ERFManagementExpendableModel ExpendableItem = ExpendableItems.Where(n => n.ERFExpendableId == value.ERFExpendableId).FirstOrDefault();

            if (ExpendableItem != null)
            {
                ExpendableItem.ModelNo = value.ModelNo;
                ExpendableItem.ProdNo = value.ProdNo;
                ExpendableItem.Quantity = value.Quantity;
                ExpendableItem.UnitPrice = value.UnitPrice;
                ExpendableItem.Description = value.Description;
                ExpendableItem.InternalOrderNumber = value.InternalOrderNumber;
                ExpendableItem.VendorOrderNumber = value.VendorOrderNumber;
                ExpendableItem.Category = value.Category;
                ExpendableItem.Branch = value.Branch;
                ExpendableItem.Brand = value.Brand;
                ExpendableItem.Substitution = value.Substitution;
                ExpendableItem.TotalCost = value.TotalCost;
                ExpendableItem.TransactionType = value.TransactionType;
                ExpendableItem.EquipmentType = value.EquipmentType;
                ExpendableItem.LaidInCost = value.LaidInCost;
                ExpendableItem.RentalCost = value.RentalCost;
            }

            TempData["Expendable"] = ExpendableItems;
            TempData.Keep("Expendable");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExpendableInsert(ERFManagementExpendableModel value)
        {
            IList<ERFManagementExpendableModel> ExpendableItems = TempData["Expendable"] as IList<ERFManagementExpendableModel>;
            if (ExpendableItems == null)
            {
                ExpendableItems = new List<ERFManagementExpendableModel>();
            }
            //string uPrice;
            //if (value.UnitPrice == null && value.ProdNo != null)
            //{
            //    uPrice = FarmerBrothersEntitites.FBExpendables.Where(e => e.ProdNo == value.ProdNo).Select(e => e.UnitPrice).FirstOrDefault();
            //}
            //else
            //{
            //    uPrice = value.UnitPrice.ToString();
            //}

            if (TempData["ERFExpendableId"] != null)
            {
                int assetId = Convert.ToInt32(TempData["ERFExpendableId"]);
                //value.ERFExpendableId = assetId + 1;
                TempData["ERFExpendableId"] = assetId + 1;
            }
            else
            {
                value.ERFExpendableId = 1;
                //value.UnitPrice = Convert.ToDouble(uPrice);
                TempData["ERFExpendableId"] = 1;
            }

            ExpendableItems.Add(value);
            TempData["Expendable"] = ExpendableItems;
            TempData.Keep("Expendable");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExpendableDelete(int key)
        {
            IList<ERFManagementExpendableModel> ExpendableItems = TempData["Expendable"] as IList<ERFManagementExpendableModel>;
            ERFManagementExpendableModel ExpendableItem = ExpendableItems.Where(n => n.ERFExpendableId == key).FirstOrDefault();
            ExpendableItems.Remove(ExpendableItem);
            TempData["Expendable"] = ExpendableItems;
            TempData.Keep("Expendable");
            return Json(ExpendableItems, JsonRequestBehavior.AllowGet);
        }



        public ActionResult PosUpdate(ERFManagementPOSModel value)
        {
            IList<ERFManagementPOSModel> PosItems = TempData["Expendable"] as IList<ERFManagementPOSModel>;
            if (PosItems == null)
            {
                PosItems = new List<ERFManagementPOSModel>();
            }
            ERFManagementPOSModel PosItem = PosItems.Where(n => n.ERFPosId == value.ERFPosId).FirstOrDefault();

            if (PosItem != null)
            {
                PosItem.ModelNo = value.ModelNo;
                PosItem.ProdNo = value.ProdNo;
                PosItem.Quantity = value.Quantity;
                PosItem.UnitPrice = value.UnitPrice;
                PosItem.Description = value.Description;
                PosItem.InternalOrderNumber = value.InternalOrderNumber;
                PosItem.VendorOrderNumber = value.VendorOrderNumber;
                PosItem.Category = value.Category;
                PosItem.Branch = value.Branch;
                PosItem.Brand = value.Brand;
                PosItem.Substitution = value.Substitution;
                PosItem.TotalCost = value.TotalCost;
                PosItem.TransactionType = value.TransactionType;
                PosItem.EquipmentType = value.EquipmentType;
                PosItem.LaidInCost = value.LaidInCost;
                PosItem.RentalCost = value.RentalCost;
            }

            TempData["Pos"] = PosItems;
            TempData.Keep("Pos");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PosInsert(ERFManagementPOSModel value)
        {
            IList<ERFManagementPOSModel> PosItems = TempData["Pos"] as IList<ERFManagementPOSModel>;
            if (PosItems == null)
            {
                PosItems = new List<ERFManagementPOSModel>();
            }

            if (TempData["ERFPosId"] != null)
            {
                int assetId = Convert.ToInt32(TempData["ERFPosId"]);
                TempData["ERFPosId"] = assetId + 1;
            }
            else
            {
                value.ERFPosId = 1;
                TempData["ERFPosId"] = 1;
            }

            PosItems.Add(value);
            TempData["Pos"] = PosItems;
            TempData.Keep("Pos");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PosDelete(int key)
        {
            IList<ERFManagementPOSModel> PosItems = TempData["Pos"] as IList<ERFManagementPOSModel>;
            ERFManagementPOSModel PosItem = PosItems.Where(n => n.ERFPosId == key).FirstOrDefault();
            PosItems.Remove(PosItem);
            TempData["Pos"] = PosItems;
            TempData.Keep("Pos");
            return Json(PosItems, JsonRequestBehavior.AllowGet);
        }

        private int ERFSave(ErfModel erfModel, out Erf erf, out string message)
        {
            int returnValue = 0;
            message = string.Empty;
            erf = null;
            FarmerBrothersEntities WOFBEntity = new FarmerBrothersEntities();
            WorkorderController wc = new WorkorderController();
            WorkOrder workOrder = null;
            IndexCounter counter = Utility.GetIndexCounter("ERFNO", 1);
            counter.IndexValue++;
            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
            erfModel.ErfAssetsModel.Erf.ErfID = counter.IndexValue.Value.ToString();

            if (erfModel.Customer != null)
            {
                erf = new Erf();
                erf.ErfID = erfModel.ErfAssetsModel.Erf.ErfID;
                erf.CustomerAddress = erfModel.Customer.Address;
                erf.CustomerCity = erfModel.Customer.City;
                if (!string.IsNullOrWhiteSpace(erfModel.Customer.CustomerId))
                {
                    erf.CustomerID = new Nullable<int>(Convert.ToInt32(erfModel.Customer.CustomerId));
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

                erf.EntryDate = Utility.GetCurrentTime(erfModel.Customer.ZipCode, FarmerBrothersEntitites);
                erf.ModifiedDate = erf.EntryDate;
                if (System.Web.HttpContext.Current.Session["UserId"] != null)
                {
                    erf.ModifiedUserID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
                    erf.EntryUserID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
                }
                else
                {
                    erf.ModifiedUserID = -1;
                    erf.EntryUserID = -1;
                }

                erf.DateOnERF = erfModel.ErfAssetsModel.Erf.DateOnERF;
                erf.DateERFReceived = erfModel.ErfAssetsModel.Erf.DateERFReceived;
                erf.DateERFProcessed = erfModel.ErfAssetsModel.Erf.DateERFProcessed;
                erf.OriginalRequestedDate = erfModel.ErfAssetsModel.Erf.OriginalRequestedDate;
                erf.HoursofOperation = erfModel.ErfAssetsModel.Erf.HoursofOperation;
                erf.InstallLocation = erfModel.ErfAssetsModel.Erf.InstallLocation;
                erf.UserName = erfModel.ErfAssetsModel.Erf.UserName;
                erf.Phone = Utilities.Utility.FormatPhoneNumber(erfModel.ErfAssetsModel.Erf.Phone);
                erf.TotalNSV =  erfModel.TotalNSV;

                /*decimal currentNSV = Convert.ToDecimal(erfModel.TotalNSV + erfModel.CurrentNSV);
                erf.CurrentNSV = currentNSV;*/

                erf.CurrentNSV = erfModel.CurrentNSV;
                erf.ContributionMargin = string.IsNullOrEmpty(erfModel.ContributionMargin) ?"":erfModel.ContributionMargin;

                erf.CurrentEqp =  erfModel.CurrentEquipmentTotal;
                erf.AdditionalEqp =  erfModel.AdditionalEquipmentTotal;
                erf.ApprovalStatus = erfModel.ApprovalStatus == null ? "" : erfModel.ApprovalStatus;

                erf.SiteReady = erfModel.ErfAssetsModel.Erf.SiteReady;

                erf.OrderType = erfModel.OrderType;
                erf.ShipToBranch = erfModel.BranchName;
                erf.ShipToJDE = erfModel.ShipToCustomer;

                //erf.ERFStatus = "Pending";

            }
            FarmerBrothersEntitites.Erfs.Add(erf);

            DateTime CurrentTime = Utility.GetCurrentTime(erfModel.Customer.ZipCode, FarmerBrothersEntitites);
            int effectedRecords = 0;
            try
            {
                JsonResult jsonResult = new JsonResult();
                //if (!erfModel.CrateWorkOrder && erf.ApprovalStatus.ToLower() == "approved for processing" && erfModel.OrderType.ToLower() != "branch stock")
                if (!erfModel.CrateWorkOrder && erf.ApprovalStatus.ToLower() == "approved for processing")
                {
                    WorkorderManagementModel workorderModel = new WorkorderManagementModel();
                    workorderModel.Closure = new WorkOrderClosureModel();
                    workorderModel.Customer = erfModel.Customer;
                    workorderModel.Customer.CustomerId = erfModel.Customer.CustomerId;
                    workorderModel.Notes = erfModel.Notes;
                    workorderModel.Operation = WorkOrderManagementSubmitType.CREATEWORKORDER;
                    workorderModel.WorkOrder = new WorkOrder();
                    workorderModel.WorkOrder.CallerName = "N/A";
                    workorderModel.WorkOrder.WorkorderContactName = "N/A";
                    workorderModel.WorkOrder.HoursOfOperation = "N/A";
                    workorderModel.WorkOrder.WorkorderCalltypeid = 1300;
                    workorderModel.WorkOrder.WorkorderCalltypeDesc = "Installation";
                    workorderModel.WorkOrder.WorkorderErfid = erfModel.ErfAssetsModel.Erf.ErfID;
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
                    workorderModel.NewNotes = erfModel.NewNotes;

                    workorderModel.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
                    workorderModel.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
                    workorderModel.WorkOrderParts = new List<WorkOrderPartModel>();
                    workorderModel.Erf = erfModel.ErfAssetsModel.Erf;

                    

                    //jsonResult = wc.SaveWorkOrder(workorderModel, null, string.Empty, false, true);                         
                    jsonResult = wc.ERFWorkOrderSave(workorderModel, WOFBEntity, out workOrder, out message);

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    WorkOrderResults result = serializer.Deserialize<WorkOrderResults>(serializer.Serialize(jsonResult.Data));
                    if (result.returnValue > 0)
                    {
                        erf.WorkorderID = Convert.ToInt32(result.WorkOrderId);
                        ErfWorkorderLog erfWorkOrderLog = new ErfWorkorderLog();
                        erfWorkOrderLog.ErfID = erf.ErfID;
                        erfWorkOrderLog.WorkorderID = Convert.ToInt32(erf.WorkorderID);
                        FarmerBrothersEntitites.ErfWorkorderLogs.Add(erfWorkOrderLog);

                        NotesHistory notesHistory = new NotesHistory()
                        {
                            AutomaticNotes = 1,
                            EntryDate = CurrentTime,
                            Notes = @"Work Order created from ERF WO#: " + Convert.ToInt32(result.WorkOrderId) + @" in “MARS”!",
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            ErfID = erf.ErfID,
                            WorkorderID = erf.WorkorderID,
                            isDispatchNotes = 0
                        };

                        FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
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

            SaveNotes(erfModel, Convert.ToInt32(erf.WorkorderID));

            string EqpNotes = "";
            string ExpNotes = "";
            if (erfModel.ErfAssetsModel.EquipmentList != null)
            {
                int eqpCount = 1;
                foreach (ERFManagementEquipmentModel equipment in erfModel.ErfAssetsModel.EquipmentList)
                {
                    FBERFEquipment eq = new FBERFEquipment()
                    {
                        ERFId = erf.ErfID,
                        WorkOrderId = erf.WorkorderID,
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

                    Contingent eqpCon = FarmerBrothersEntitites.Contingents.Where(c => c.ContingentID == equipment.Category).FirstOrDefault();
                    ContingentDetail eqpConDtl = FarmerBrothersEntitites.ContingentDetails.Where(c => c.ID == equipment.Brand).FirstOrDefault();
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

                    FarmerBrothersEntitites.FBERFEquipments.Add(eq);
                }
            }

            if (erfModel.ErfAssetsModel.ExpendableList != null)
            {
                int expCount = 1;
                foreach (ERFManagementExpendableModel expItems in erfModel.ErfAssetsModel.ExpendableList)
                {
                    FBERFExpendable eq = new FBERFExpendable()
                    {
                        ERFId = erf.ErfID,
                        WorkOrderId = erf.WorkorderID,
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


                    Contingent expCon = FarmerBrothersEntitites.Contingents.Where(c => c.ContingentID == expItems.Category).FirstOrDefault(); 
                    ContingentDetail expConDtl = FarmerBrothersEntitites.ContingentDetails.Where(c => c.ID == expItems.Brand).FirstOrDefault();
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

                    FarmerBrothersEntitites.FBERFExpendables.Add(eq);
                }
            }


            NotesHistory eqpNotesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = CurrentTime,
                Notes = "Equipments: " + EqpNotes + "Expendables: " + ExpNotes,
                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                UserName = UserName,
                ErfID = erf.ErfID,
                WorkorderID = erf.WorkorderID,
                isDispatchNotes = 0
            };

            FarmerBrothersEntitites.NotesHistories.Add(eqpNotesHistory);

            //if (erfModel.ErfAssetsModel.PosList != null)
            //{
            //    foreach (ERFManagementPOSModel posItem in erfModel.ErfAssetsModel.PosList)
            //    {
            //        FBERFPos pos = new FBERFPos()
            //        {
            //            ERFId = erf.ErfID,
            //            WorkOrderId = erf.WorkorderID,
            //            ModelNo = posItem.ModelNo,
            //            Quantity = posItem.Quantity,
            //            ProdNo = posItem.ProdNo,
            //            EquipmentType = posItem.EquipmentType,
            //            UnitPrice = Convert.ToDecimal(posItem.UnitPrice),
            //            TransactionType = posItem.TransactionType,
            //            Substitution = posItem.Substitution,
            //            Extra = posItem.Extra,
            //            Description = posItem.Description,
            //            LaidInCost = Convert.ToDecimal(posItem.LaidInCost),
            //            RentalCost = Convert.ToDecimal(posItem.RentalCost),
            //            TotalCost = Convert.ToDecimal(posItem.TotalCost),
            //            ContingentCategoryId = posItem.Category,
            //            ContingentCategoryTypeId = posItem.Brand,
            //            UsingBranch = posItem.Branch

            //        };

            //        FarmerBrothersEntitites.FBERFPos.Add(pos);
            //    }
            //}

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
                erf.ERFStatus = "Pending CapEx-FA";
            }
            else
            {
                erf.ERFStatus = "Pending";
            }
            erf.CashSaleStatus = "1"; // Default value for CashSaleStatus while ERF Creation
            
            int woSaveEffectRecords = WOFBEntity.SaveChanges();
            int woReturnValue = woSaveEffectRecords > 0 ? 1 : 0;
            //if (((!erfModel.CrateWorkOrder && erf.ApprovalStatus.ToLower() == "approved for processing" && erfModel.OrderType.ToLower() != "branch stock") && woReturnValue == 1)
               //  || (erfModel.CrateWorkOrder || erf.ApprovalStatus.ToLower() != "approved for processing" || erfModel.OrderType.ToLower() == "branch stock"))
            if (((!erfModel.CrateWorkOrder && erf.ApprovalStatus.ToLower() == "approved for processing") && woReturnValue == 1)
                || (erfModel.CrateWorkOrder || erf.ApprovalStatus.ToLower() != "approved for processing"))
            {
                effectedRecords = FarmerBrothersEntitites.SaveChanges();
                returnValue = effectedRecords > 0 ? 1 : 0;
            }

            ERFEmail(erf.ErfID, erf.WorkorderID, erfModel.CrateWorkOrder, erfModel.ApprovalStatus);

            if (!erfModel.CrateWorkOrder && erf.ApprovalStatus.ToLower() == "approved for processing" && workOrder != null)
            {
                string usrName = System.Web.HttpContext.Current.Session["UserName"] == null ? "" : System.Web.HttpContext.Current.Session["UserName"].ToString();
                //wc.StartAutoDispatchProcess(workOrder, usrName);
            }            

            return returnValue;
        }

        public void SaveNotes(ErfModel erfManagement, int erfWorkorderId)
        {
            if (erfManagement.NewNotes != null)
            {

                TimeZoneInfo newTimeZoneInfo = null;
                Utility.GetCustomerTimeZone(erfManagement.Customer.ZipCode, FarmerBrothersEntitites);

                DateTime CurrentTime = Utility.GetCurrentTime(erfManagement.Customer.ZipCode, FarmerBrothersEntitites);

                foreach (NewNotesModel newNotesModel in erfManagement.NewNotes)
                {
                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 0,
                        EntryDate = CurrentTime,
                        Notes = newNotesModel.Text,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        ErfID = erfManagement.ErfAssetsModel.Erf.ErfID,
                        WorkorderID = erfWorkorderId == 0 ? null : (int?)erfWorkorderId,// erfManagement.ErfAssetsModel.Erf.WorkorderID,
                        isDispatchNotes = 0
                    };
                    FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                }
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ErfSave")]
        public JsonResult SaveErf([ModelBinder(typeof(ErfModelBinder))] ErfModel erfModel)
        {
            int recordsSaved = 0;
            bool isValid = true;
            string message = string.Empty;
            Erf erf = null;
            switch (erfModel.Operation)
            {
                case ERFManagementSubmitType.NONE:
                    break;
                case ERFManagementSubmitType.SAVE:
                    {
                        recordsSaved = -1;
                        if (erfModel.NewNotes == null || erfModel.NewNotes.Count <= 0)
                        {
                            message = @"|Notes required to save ERF!";
                            isValid = false;
                        }
                        else if (erfModel.NewNotes.Count > 0)
                        {
                            NewNotesModel newNotes = erfModel.NewNotes.ElementAt(0);
                            if (string.IsNullOrWhiteSpace(newNotes.Text))
                            {
                                message = @"|Notes can not be blank!";
                                isValid = false;
                            }
                        }

                        if((erfModel.ErfAssetsModel.EquipmentList == null || erfModel.ErfAssetsModel.EquipmentList.Count <=0)
                            && (erfModel.ErfAssetsModel.ExpendableList == null || erfModel.ErfAssetsModel.ExpendableList.Count <= 0))
                            //&& (erfModel.ErfAssetsModel.PosList == null || erfModel.ErfAssetsModel.PosList.Count <= 0))
                        {
                            //message = @"|Equipments Or Expendables Or PointOfSales are Required";
                            message = @"|Equipments Or Expendables are Required";
                            isValid = false;
                        }

                        if (isValid)
                        {
                            if (erfModel.Customer != null)
                            {
                                if (!string.IsNullOrWhiteSpace(erfModel.ErfAssetsModel.Erf.ErfID))
                                {
                                    erf = FarmerBrothersEntitites.Erfs.Where(e => e.ErfID == erfModel.ErfAssetsModel.Erf.ErfID).FirstOrDefault();

                                    erf.CustomerAddress = erfModel.Customer.Address;
                                    erf.CustomerCity = erfModel.Customer.City;
                                    if (!string.IsNullOrWhiteSpace(erfModel.Customer.CustomerId))
                                    {
                                        erf.CustomerID = new Nullable<int>(Convert.ToInt32(erfModel.Customer.CustomerId));
                                    }
                                    erf.CustomerMainContactName = erfModel.Customer.MainContactName;
                                    erf.CustomerMainEmail = erfModel.Customer.MainEmailAddress;
                                    erf.CustomerName = erfModel.Customer.CustomerName;
                                    erf.CustomerPhone = erfModel.Customer.PhoneNumber;
                                    erf.CustomerPhoneExtn = erfModel.Customer.PhoneExtn;
                                    erf.CustomerState = erfModel.Customer.State;
                                    erf.CustomerZipCode = erfModel.Customer.ZipCode;
                                }
                            }
                            List<FBERFEquipment> eqList = FarmerBrothersEntitites.FBERFEquipments.Where(e => e.ERFId == erfModel.ErfAssetsModel.Erf.ErfID).ToList();
                            foreach (FBERFEquipment item in eqList)
                            {
                                FarmerBrothersEntitites.FBERFEquipments.Remove(item);
                            }
                            List<FBERFExpendable> exList = FarmerBrothersEntitites.FBERFExpendables.Where(e => e.ERFId == erfModel.ErfAssetsModel.Erf.ErfID).ToList();
                            foreach (FBERFExpendable item in exList)
                            {
                                FarmerBrothersEntitites.FBERFExpendables.Remove(item);
                            }

                            if (erfModel.ErfAssetsModel.EquipmentList != null)
                            {
                                foreach (ERFManagementEquipmentModel equipment in erfModel.ErfAssetsModel.EquipmentList)
                                {
                                    FBERFEquipment eq = new FBERFEquipment()
                                    {
                                        ERFId = erf.ErfID,
                                        WorkOrderId = erf.WorkorderID,
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
                                        ContingentCategoryTypeId = equipment.Brand

                                    };

                                    FarmerBrothersEntitites.FBERFEquipments.Add(eq);
                                }
                            }

                            if (erfModel.ErfAssetsModel.ExpendableList != null)
                            {
                                foreach (ERFManagementExpendableModel expItems in erfModel.ErfAssetsModel.ExpendableList)
                                {
                                    FBERFExpendable eq = new FBERFExpendable()
                                    {
                                        ERFId = erf.ErfID,
                                        WorkOrderId = erf.WorkorderID,
                                        ModelNo = expItems.ModelNo,
                                        Quantity = expItems.Quantity,
                                        ProdNo = expItems.ProdNo,
                                        UnitPrice = Convert.ToDecimal(expItems.UnitPrice),
                                        TransactionType = expItems.TransactionType,
                                        Extra = expItems.Extra,
                                        Description = expItems.Description,
                                        LaidInCost = Convert.ToDecimal(expItems.LaidInCost),
                                        RentalCost = Convert.ToDecimal(expItems.RentalCost),
                                        TotalCost = Convert.ToDecimal(expItems.TotalCost),
                                        ContingentCategoryId = expItems.Category,
                                        ContingentCategoryTypeId = expItems.Brand,
                                        Substitution = expItems.Substitution,
                                        EquipmentType = expItems.EquipmentType

                                    };
                                    FarmerBrothersEntitites.FBERFExpendables.Add(eq);
                                }
                            }

                            SaveNotes(erfModel, Convert.ToInt32(erf.WorkorderID));
                            recordsSaved = FarmerBrothersEntitites.SaveChanges();
                        }


                    }
                    break;
                case ERFManagementSubmitType.CREATEERF:
                    {
                        if (erfModel.NewNotes == null || erfModel.NewNotes.Count <= 0)
                        {
                            message = @"|Notes required to save ERF!";
                            isValid = false;
                        }
                        else if (erfModel.NewNotes.Count > 0)
                        {
                            NewNotesModel newNotes = erfModel.NewNotes.ElementAt(0);
                            if (string.IsNullOrWhiteSpace(newNotes.Text))
                            {
                                message = @"|Notes can not be blank!";
                                isValid = false;
                            }
                        }
                        if ((erfModel.ErfAssetsModel.EquipmentList == null || erfModel.ErfAssetsModel.EquipmentList.Count <= 0)
                            && (erfModel.ErfAssetsModel.ExpendableList == null || erfModel.ErfAssetsModel.ExpendableList.Count <= 0))
                            //&& (erfModel.ErfAssetsModel.PosList == null || erfModel.ErfAssetsModel.PosList.Count <= 0))
                        {
                            //message = @"|Equipments Or Expendables Or PointOfSales are Required";
                            message = @"|Equipments Or Expendables are Required";
                            isValid = false;
                        }
                        if (string.IsNullOrEmpty(erfModel.Customer.MainContactName.Trim()))
                        {
                            message = @"|Customer Main ContactName is Required";
                            isValid = false;
                        }
                        if(string.IsNullOrEmpty(erfModel.Customer.PhoneNumber.Trim()))
                        {
                            message = @"|Customer PhoneNumber is Required";
                            isValid = false;
                        }

                        if (isValid)
                        {
                            recordsSaved = ERFSave(erfModel, out erf, out message);

                        }
                    }
                    break;
                case ERFManagementSubmitType.CREATEERFWITHWORKORDER:
                    break;
                default:
                    break;
            }

            if (recordsSaved > 0)
            {
                var redirectUrl = string.Empty;
                if (Request != null)
                {
                    //redirectUrl = new UrlHelper(Request.RequestContext).Action("CustomerSearch", "CUSTOMER", new { isBack = 0 });
                    redirectUrl = new UrlHelper(Request.RequestContext).Action("CustomerSearch", "CustomerSearch", new { isBack = 0 });
                }

                message = @"ERF created successfully! ERF#: " + erfModel.ErfAssetsModel.Erf.ErfID;
                if (erf.WorkorderID != null && erf.WorkorderID != 0)
                {
                    message += @"|Work Order created successfully! Work Order ID#: " + erf.WorkorderID;
                }

                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = redirectUrl, WorkorderId = erf.WorkorderID, id = erfModel.ErfAssetsModel.Erf.ErfID, returnValue = recordsSaved, message = message };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            else
            {
                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, Url = "", WorkorderErfid = 0, returnValue = recordsSaved, message = message };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "UpdateNotes")]
        public JsonResult UpdateNotes(NotesModel notesModel)
        {
            DateTime currentTime = Utility.GetCurrentTime(notesModel.CustomerZipCode, FarmerBrothersEntitites);


            NotesHistory notesHistory = new NotesHistory()
            {
                AutomaticNotes = 0,
                EntryDate = currentTime,
                Notes = notesModel.Notes,
                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                UserName = UserName,
                isDispatchNotes = 1
            };


            if (notesModel.WorkOrderID >= 0)
            {
                notesHistory.WorkorderID = new Nullable<int>(notesModel.WorkOrderID);
            }

            if (!string.IsNullOrWhiteSpace(notesModel.ErfID))
            {
                notesHistory.ErfID = notesModel.ErfID;
            }

            FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
            FarmerBrothersEntitites.SaveChanges();

            IEnumerable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.ErfID == notesHistory.ErfID && nh.AutomaticNotes == 0).OrderByDescending(nh => nh.EntryDate).ToList();
            IList<NotesHistoryModel> notesHistoryModelList = new List<NotesHistoryModel>();

            foreach (NotesHistory newNotesHistory in notesHistories)
            {
                notesHistoryModelList.Add(new NotesHistoryModel(newNotesHistory));
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = notesHistoryModelList };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }


        public bool ERFEmail(string ErfId, int? workorderId, bool isAccessory, string approvalStatus, bool isCancel = false, bool isFromApprove = false)
        {
            Erf erfData = FarmerBrothersEntitites.Erfs.Where(er => er.ErfID == ErfId).FirstOrDefault();
            StringBuilder subject = new StringBuilder();
            WorkOrder workorderData = null;

            string toAddress = "";
            string ccAddress = "";
            if (workorderId != null)
            {
                workorderData = FarmerBrothersEntitites.WorkOrders.Where(wo => wo.WorkorderID == workorderId).FirstOrDefault();
            }
           

            bool result = true;
            if (erfData != null)
            {
                Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == erfData.CustomerID).FirstOrDefault();
                string customerBranch = string.Empty;
                string customerZipCode = string.Empty;

                int ESMId = 0;
                string ESMEmail = string.Empty;

                if (customer != null)
                {
                    customerBranch = customer.Branch == null ? "0" : customer.Branch.ToString();
                    customerZipCode = customer.PostalCode == null ? "0" : customer.PostalCode.ToString();

                    ESMId = customer.FSMJDE == null ? 0 : Convert.ToInt32(customer.FSMJDE);
                    ESMEmail = customer.ESMEmail == null ? "" : customer.ESMEmail;
                }
                

                /*ESMDSMRSM esmdsmrsmView = new ESMDSMRSM();
                if (!string.IsNullOrEmpty(customerBranch))
                {
                    esmdsmrsmView = FarmerBrothersEntitites.ESMDSMRSMs.Where(x => x.BranchNO == customerBranch).FirstOrDefault();
                }
                else
                {
                    esmdsmrsmView.EDSMID = 0;
                }

                if (esmdsmrsmView == null)
                {
                    esmdsmrsmView = new ESMDSMRSM();
                    esmdsmrsmView.EDSMID = 0;
                }*/

                //if (esmdsmrsmView != null)
                //{
                decimal? eqpTotal = FarmerBrothersEntitites.FBERFEquipments.Where(eqp => eqp.ERFId == erfData.ErfID).Sum(x => x.TotalCost);
                    decimal? expTotal = FarmerBrothersEntitites.FBERFExpendables.Where(eqp => eqp.ERFId == erfData.ErfID).Sum(x => x.TotalCost);

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

                    StringBuilder salesEmailBody = new StringBuilder();

                    salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("<BR>");
                    string url = ConfigurationManager.AppSettings["ERFResponseUrl"];
                    string Redircturl = ConfigurationManager.AppSettings["RedirectResponseUrl"];
                    string Closureurl = ConfigurationManager.AppSettings["CallClosureUrl"];

                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + url + "ERFId=" + erfData.ErfID + "&ESM=" + ESMId + "&Status=Processed \">PROCESSED</a>");                
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + url + "ERFId=" + erfData.ErfID + "&ESM=" + ESMId + "&Status=Shipped \">SHIPPED</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + url + "ERFId=" + erfData.ErfID + "&ESM=" + ESMId + "&Status=Complete \">COMPLETE</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + url + "ERFId=" + erfData.ErfID + "&ESM=" + ESMId + "&Status=Cancel \">CANCEL</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");


                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("ERF#: ");
                    salesEmailBody.Append(erfData.ErfID);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("Work Order ID#: ");
                    string WOId = (workorderData == null) ? "" : workorderData.WorkorderID.ToString();
                    salesEmailBody.Append(WOId);
                    salesEmailBody.Append("<BR>");

                salesEmailBody.Append("Additional NSV: ");
                salesEmailBody.Append(erfData.TotalNSV);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("Current NSV: ");
                salesEmailBody.Append(erfData.CurrentNSV);
                salesEmailBody.Append("<BR>");

                string ContributionMargin = string.IsNullOrEmpty(erfData.ContributionMargin) ? "" : erfData.ContributionMargin;
                salesEmailBody.Append("Contribution Margin: ");
                salesEmailBody.Append(ContributionMargin);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("Current Equipment: ");
                salesEmailBody.Append(erfData.CurrentEqp);
                salesEmailBody.Append("<BR>");

                salesEmailBody.Append("Approval Status: ");
                if (!isCancel && approvalStatus != null && approvalStatus.ToLower() != "approved for processing")
                {
                    salesEmailBody.Append("<span style='color:#ff0000'><b>");
                    salesEmailBody.Append(approvalStatus);
                    salesEmailBody.Append("</b></span>");
                }
                else
                {
                    salesEmailBody.Append(approvalStatus);
                }                    
                salesEmailBody.Append("<BR>");

                salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("Order Type: ");
                    salesEmailBody.Append(erfData.OrderType);
                    salesEmailBody.Append("<BR>");

                    string ShipToBranchName = "";
                    if (erfData.ShipToBranch != null)
                    {
                        ERFBranchDetail erfBrnchDtls = FarmerBrothersEntitites.ERFBranchDetails.Where(e => e.Branch == erfData.ShipToBranch).FirstOrDefault();
                        if (erfBrnchDtls != null)
                        {
                           ShipToBranchName = erfBrnchDtls.BranchName;
                        }
                    }
                    salesEmailBody.Append("Ship to Branch if Different: ");
                    salesEmailBody.Append(ShipToBranchName);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("Site Ready: ");
                    salesEmailBody.Append(erfData.SiteReady);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("<span style='color:#ff0000'>");
                    salesEmailBody.Append("Install Date: ");
                    salesEmailBody.Append(erfData.OriginalRequestedDate);
                    salesEmailBody.Append("</span>");
                    salesEmailBody.Append("<BR>");

                salesEmailBody.Append("<span style='color:#ff0000'>");
                salesEmailBody.Append("Order Status: ");
                salesEmailBody.Append(erfData.ERFStatus);
                salesEmailBody.Append("</span>");
                salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("ERF Total: ");
                    salesEmailBody.Append(GrandTotal);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("<b>CUSTOMER INFORMATION: </b>");
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("CUSTOMER#: ");
                    salesEmailBody.Append(erfData.CustomerID);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append(erfData.CustomerName);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append(erfData.CustomerAddress);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append(erfData.CustomerCity);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append(erfData.CustomerState);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append(erfData.CustomerZipCode);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("PHONE: ");
                    salesEmailBody.Append(erfData.CustomerPhone);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("BRANCH: ");
                    salesEmailBody.Append(customerBranch);
                    salesEmailBody.Append("<BR>");

                    string route = ""; string lastSalesDate = "";
                    if (customer != null)
                    {
                        route = customer.Route;
                        lastSalesDate = customer.LastSaleDate;
                    }

                    salesEmailBody.Append("ROUTE#: ");
                    salesEmailBody.Append(route);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("LAST SALES DATE: ");
                    salesEmailBody.Append(lastSalesDate);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("Hours of Operation: ");
                    salesEmailBody.Append(erfData.HoursofOperation);
                    salesEmailBody.Append("<BR>");

                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("<BR>");


                    salesEmailBody.Append("<b>EQUIPMENT: </b>");
                    salesEmailBody.Append("<BR>");



                    salesEmailBody.Append("<table cellpadding='5'>");
                    salesEmailBody.Append("<tr>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Quantity</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Equipment Category</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Brand - Equipment Model Number - Description</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Using Branch Stock</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Substitution Possible</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Trans Type</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Equipment Type</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Laid-In-Cost</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Rental/Sale Cost</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Total</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>ST/ON #</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>OT #</th>");
                    salesEmailBody.Append("</tr>");

                    List<FBERFEquipment> eqpModelList = FarmerBrothersEntitites.FBERFEquipments.Where(eqp => eqp.ERFId == erfData.ErfID).ToList();
                    foreach (FBERFEquipment equipment in eqpModelList)
                    {
                        ContingentDetail Brand = FarmerBrothersEntitites.ContingentDetails.Where(cat => cat.ID == equipment.ContingentCategoryTypeId).FirstOrDefault();
                        Contingent category = FarmerBrothersEntitites.Contingents.Where(c => c.ContingentID == equipment.ContingentCategoryId).FirstOrDefault();

                        salesEmailBody.Append("<tr>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + equipment.Quantity + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (category != null ? category.ContingentName : "") + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (Brand != null ? Brand.Name : "") + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(equipment.UsingBranch) ? "" : equipment.UsingBranch) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(equipment.Substitution) ? "" : equipment.Substitution) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(equipment.TransactionType) ? "" : equipment.TransactionType) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(equipment.EquipmentType) ? "" : equipment.EquipmentType) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + equipment.LaidInCost + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + equipment.RentalCost + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + equipment.TotalCost + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(equipment.InternalOrderType) ? "" : equipment.InternalOrderType) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(equipment.VendorOrderType) ? "" : equipment.VendorOrderType) + "</td>");
                        salesEmailBody.Append("</tr>");
                    }
                    salesEmailBody.Append("</table>");

                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("<BR>");


                    salesEmailBody.Append("<b>ACCESSORIES: </b>");
                    salesEmailBody.Append("<BR>");



                    salesEmailBody.Append("<table cellpadding='5'>");
                    salesEmailBody.Append("<tr>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Quantity</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Equipment Category</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Brand - Equipment Model Number - Description</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Using Branch Stock</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Substitution Possible</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Trans Type</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Equipment Type</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Laid-In-Cost</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Rental Cost</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>Total</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>ST/ON #</th>");
                    salesEmailBody.Append("<th style='border: 1px solid;'>OT #</th>");
                    salesEmailBody.Append("</tr>");

                    List<FBERFExpendable> expModelList = FarmerBrothersEntitites.FBERFExpendables.Where(eqp => eqp.ERFId == erfData.ErfID).ToList();
                    foreach (FBERFExpendable expendible in expModelList)
                    {
                        ContingentDetail Brand = FarmerBrothersEntitites.ContingentDetails.Where(cat => cat.ID == expendible.ContingentCategoryTypeId).FirstOrDefault();
                        Contingent category = FarmerBrothersEntitites.Contingents.Where(c => c.ContingentID == expendible.ContingentCategoryId).FirstOrDefault();

                        salesEmailBody.Append("<tr>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + expendible.Quantity + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (category != null ? category.ContingentName : "") + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (Brand != null ? Brand.Name : "") + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(expendible.UsingBranch) ?"":expendible.UsingBranch) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(expendible.Substitution) ? "" : expendible.Substitution) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(expendible.TransactionType) ?"":expendible.TransactionType) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(expendible.EquipmentType) ? "" : expendible.EquipmentType) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + expendible.LaidInCost + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + expendible.RentalCost + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + expendible.TotalCost + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(expendible.InternalOrderType) ? "" : expendible.InternalOrderType) + "</td>");
                        salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(expendible.VendorOrderType) ? "" : expendible.VendorOrderType) + "</td>");
                        salesEmailBody.Append("</tr>");
                    }
                    salesEmailBody.Append("</table>");

                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("<BR>");

                //****************POS**********************

                //salesEmailBody.Append("<b>Point of Sale: </b>");
                //salesEmailBody.Append("<BR>");

                //salesEmailBody.Append("<table cellpadding='5'>");
                //salesEmailBody.Append("<tr>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Quantity</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Equipment Category</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Brand - Equipment Model Number - Description</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Using Branch Stock</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Substitution Possible</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Trans Type</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Equipment Type</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Laid-In-Cost</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Rental Cost</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>Total</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>ST/ON #</th>");
                //salesEmailBody.Append("<th style='border: 1px solid;'>OT #</th>");
                //salesEmailBody.Append("</tr>");

                //List<FBERFPos> posModelList = FarmerBrothersEntitites.FBERFPos.Where(eqp => eqp.ERFId == erfData.ErfID).ToList();
                //foreach (FBERFPos pos in posModelList)
                //{
                //    ContingentDetail Brand = FarmerBrothersEntitites.ContingentDetails.Where(cat => cat.ID == pos.ContingentCategoryTypeId).FirstOrDefault();
                //    Contingent category = FarmerBrothersEntitites.Contingents.Where(c => c.ContingentID == pos.ContingentCategoryId).FirstOrDefault();

                //    salesEmailBody.Append("<tr>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + pos.Quantity + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + (category != null ? category.ContingentName : "") + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + (Brand != null ? Brand.Name : "") + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(pos.UsingBranch) ? "" : pos.UsingBranch) + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(pos.Substitution) ? "" : pos.Substitution) + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(pos.TransactionType) ? "" : pos.TransactionType) + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(pos.EquipmentType) ? "" : pos.EquipmentType) + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + pos.LaidInCost + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + pos.RentalCost + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + pos.TotalCost + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(pos.InternalOrderType) ? "" : pos.InternalOrderType) + "</td>");
                //    salesEmailBody.Append("<td style='border: 1px solid;'>" + (string.IsNullOrEmpty(pos.VendorOrderType) ? "" : pos.VendorOrderType) + "</td>");
                //    salesEmailBody.Append("</tr>");
                //}
                //salesEmailBody.Append("</table>");

                //salesEmailBody.Append("<BR>");
                //salesEmailBody.Append("<BR>");


                //****************POS End**********************

                salesEmailBody.Append("<b>CALL NOTES: </b>");
                    salesEmailBody.Append("<BR>");


                    List<NotesHistory> histories = FarmerBrothersEntitites.NotesHistories.Where(notes => notes.ErfID == erfData.ErfID).ToList();

                    foreach (NotesHistory history in histories)
                    {
                        salesEmailBody.Append(history.UserName);
                        salesEmailBody.Append(" ");
                        salesEmailBody.Append(history.EntryDate);
                        salesEmailBody.Append(" ");
                        salesEmailBody.Append(history.Notes);
                        salesEmailBody.Append("<BR>");
                    }

                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("<BR>");

                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + url + "ERFId=" + erfData.ErfID + "&ESM=" + ESMId + "&Status=Processed \">PROCESSED</a>");                
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + url + "ERFId=" + erfData.ErfID + "&ESM=" + ESMId + "&Status=Shipped \">SHIPPED</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + url + "ERFId=" + erfData.ErfID + "&ESM=" + ESMId + "&Status=Complete \">COMPLETE</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                salesEmailBody.Append("<a href=\"" + url + "ERFId=" + erfData.ErfID + "&ESM=" + ESMId + "&Status=Cancel \">CANCEL</a>");
                salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                string subjectLine = "";               
                if (isCancel)
                {
                    subjectLine += "Order Canceled :  ";
                }

                if (GrandTotal >= 10000)
                {
                    subjectLine += "Pending CapEx-FA: " + erfData.ErfID;
                    if (!isCancel && approvalStatus != null && approvalStatus.ToLower() != "approved for processing")
                    {
                        subjectLine = "Pending CapEx-FA - Not Approved : " + erfData.ErfID;
                    }
                }
                else
                {
                    subjectLine += "Equipment Order Confirmation: " + erfData.ErfID;
                    if (!isCancel && approvalStatus != null && approvalStatus.ToLower() != "approved for processing")
                    {
                        subjectLine = "Equipment Order - Not Approved : " + erfData.ErfID;
                    }
                }               

                subject.Append(subjectLine);
                    //===============================

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

             
                FbUserMaster FBU = FarmerBrothersEntitites.FbUserMasters.Where(user => user.UserId == erfData.EntryUserID).FirstOrDefault();

                if (isCancel)
                {
                    if (FBU != null)
                    {
                        toAddress += ";" + FBU.EmailId;
                    }
                    ccAddress += ";erfprocessing@farmerbros.com";
                }
                else
                {
                    //New Email Process
                    if (isAccessory)
                    {
                        if (approvalStatus != null && approvalStatus.ToUpper() == "APPROVED FOR PROCESSING")
                        {
                            toAddress += ";erfprocessing@farmerbros.com";
                            if (FBU != null)
                            {
                                ccAddress += ";" + FBU.EmailId;
                            }
                        }
                        else
                        {
                            if (FBU != null)
                            {
                                toAddress += ";" + FBU.EmailId;
                            }
                        }
                    }

                    if (erfData.ERFStatus.ToUpper() == "PENDING CAPEX-FA")
                    {
                        toAddress += ";FARequestSales@farmerbros.com";
                        ccAddress += ";erfprocessing@farmerbros.com";
                        if (FBU != null)
                        {
                            ccAddress += ";" + FBU.EmailId;
                        }
                        if (!string.IsNullOrEmpty(ESMEmail))
                        {
                            ccAddress += ";" + ESMEmail;
                        }
                    }
                    else
                    {
                        if (approvalStatus != null && approvalStatus.ToUpper() == "APPROVED FOR PROCESSING")
                        {
                            toAddress += ";erfprocessing@farmerbros.com";
                            if (FBU != null)
                            {
                                ccAddress += ";" + FBU.EmailId;
                            }
                        }
                        else
                        {
                            if (FBU != null)
                            {
                                toAddress += ";" + FBU.EmailId;
                            }
                        }
                    }

                    //if (approvalStatus != null && approvalStatus.ToUpper() == "APPROVED FOR PROCESSING")
                    //{
                    //    toAddress += ";" + erfData.CustomerMainEmail;
                    //}
                    //else
                    //{
                    //    if (!string.IsNullOrEmpty(ESMEmail))
                    //    {
                    //        toAddress += erfData.CustomerMainEmail;
                    //    }
                    //}

                    if (isFromApprove)
                    {
                        if (FBU != null)
                        {
                            toAddress += ";" + FBU.EmailId;
                        }
                    }
                    //Old Email Process
                    /*if (isAccessory)
                    {
                        toAddress += "ERFProcessing@farmerbros.com";
                        //toAddress += "emlprocessing@farmerbros.com";
                    }
                    else
                    {
                        if (erfData.ERFStatus.ToUpper() == "PENDING CAPEX-FA")
                        {
                            toAddress += "FARequestSales@farmerbros.com";
                            ccAddress += ";erfprocessing@farmerbros.com";
                            if (FBU != null)
                            {
                                ccAddress += ";" + FBU.EmailId;
                            }
                           
                            if (!string.IsNullOrEmpty(ESMEmail))
                            {
                                ccAddress += ";" + ESMEmail;
                            }
                        }
                        else
                        {
                            toAddress += "erfprocessing@farmerbros.com";
                        }
                    }

                    if (erfData.ERFStatus.ToUpper() != "PENDING CAPEX-FA")
                    {
                        if (FBU != null)
                        {
                            toAddress += ";" + FBU.EmailId;
                        }
                        
                        if (!string.IsNullOrEmpty(ESMEmail))
                        {
                            toAddress += ";" + ESMEmail;
                        }
                    }


                    if(approvalStatus != null && approvalStatus.ToUpper() == "APPROVED FOR PROCESSING")
                    {
                        //toAddress += ";" + "FARequestSales@farmerbros.com";
                        toAddress += ";" + erfData.CustomerMainEmail;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ESMEmail))
                        {
                            toAddress = erfData.CustomerMainEmail;
                        }
                    }

                    if(isFromApprove)
                    {
                        if (FBU != null)
                        {
                            toAddress += ";" + FBU.EmailId;
                        }
                    }*/

                }

                if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                {
                    toAddress = ConfigurationManager.AppSettings["ERFTestEmail"];
                    ccAddress = "";
                }

                string mailTo = toAddress;
                string mailCC = ccAddress;
                string EmailList = "";
                if (!string.IsNullOrWhiteSpace(mailTo))
                {
                    if (toAddress.Contains("#"))
                    {
                        string[] mailCCAddress = toAddress.Split('#');

                        if (mailCCAddress.Count() > 0)
                        {
                            string[] CCAddresses = mailCCAddress[1].Split(';');
                            EmailList += " CC: "; 
                            foreach (string address in CCAddresses)
                            {
                                    if (!Utility.isValidEmail(address)) continue;

                                    if (!string.IsNullOrWhiteSpace(address))
                                    {
                                        message.CC.Add(new MailAddress(address));
                                        EmailList += address + ";";
                                    }
                            }
                            string[] addresses = mailCCAddress[0].Split(';');
                            EmailList += " TO: ";
                            foreach (string address in addresses)
                            {
                                    if (!Utility.isValidEmail(address)) continue;
                                    if (!string.IsNullOrWhiteSpace(address))
                                    {
                                        message.To.Add(new MailAddress(address));
                                        EmailList += address + ";";
                                    }
                            }
                        }
                    }
                    else
                    {
                        string[] addresses = mailTo.Split(';');
                        EmailList += " TO: ";
                        foreach (string address in addresses)
                        {
                                if (!Utility.isValidEmail(address)) continue;
                                if (!string.IsNullOrWhiteSpace(address))
                                {
                                    message.To.Add(new MailAddress(address));
                                    EmailList += address + ";";
                                }
                        }

                        string[] ccaddresses = mailCC.Split(';');
                        EmailList += " CC: ";
                        foreach (string address in ccaddresses)
                        {
                                if (!Utility.isValidEmail(address)) continue;
                                if (!string.IsNullOrWhiteSpace(address))
                                {
                                    message.CC.Add(new MailAddress(address));
                                    EmailList += address + ";";
                                }
                        }
                    }

                    string fromAddress = "reviveservice@mktalt.com";// ConfigurationManager.AppSettings["DispatchMailFromAddress"];
                        message.From = new MailAddress(fromAddress);
                        message.Subject = subject.ToString();
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
                //====================================
                //}

                if (result)
                {
                    FarmerBrothersEntities fbe = new FarmerBrothersEntities();
                    DateTime CurrentTime = Utility.GetCurrentTime(erfData.CustomerZipCode, fbe);

                    //ESMDSMRSM esmdsmrsmView = FarmerBrothersEntitites.ESMDSMRSMs.FirstOrDefault(x => x.EDSMID == ESMId);
                    ESMCCMRSMEscalation esmdsmrsmView = FarmerBrothersEntitites.ESMCCMRSMEscalations.FirstOrDefault(x => x.EDSMID == ESMId);
                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = CurrentTime,
                        Notes = "ERF Confirmation Email sent to : " + EmailList,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = esmdsmrsmView == null ? "1234" : esmdsmrsmView.ESMName,//System.Web.HttpContext.Current.Session["UserName"] != null ? System.Web.HttpContext.Current.Session["UserName"].ToString() : UserName,
                        ErfID = ErfId,
                        WorkorderID = workorderId,
                        isDispatchNotes = 1
                    };

                    fbe.NotesHistories.Add(notesHistory);
                    fbe.SaveChanges();
                }
            }

            return result;
        }

        [HttpPost]
        public JsonResult CreateERFEvent(int erfId)
        {
            int effectedRecords = 0;

            Erf erf = FarmerBrothersEntitites.Erfs.Where(er => er.ErfID == erfId.ToString()).FirstOrDefault();
            ErfModel erfModel = ConstructERFManagementModel(erf.CustomerID, erfId);
            DateTime CurrentTime = Utility.GetCurrentTime(erfModel.Customer.ZipCode, FarmerBrothersEntitites);
            JsonResult jsonResult = new JsonResult();

            WorkorderManagementModel workorderModel = new WorkorderManagementModel();
            workorderModel.Closure = new WorkOrderClosureModel();
            workorderModel.Customer = erfModel.Customer;
            workorderModel.Customer.CustomerId = erfModel.Customer.CustomerId;
            workorderModel.Notes = erfModel.Notes;
            workorderModel.Operation = WorkOrderManagementSubmitType.CREATEWORKORDER;
            workorderModel.WorkOrder = new WorkOrder();
            workorderModel.WorkOrder.CallerName = "N/A";
            workorderModel.WorkOrder.WorkorderContactName = "N/A";
            workorderModel.WorkOrder.HoursOfOperation = "N/A";
            workorderModel.WorkOrder.WorkorderCalltypeid = 1300;
            workorderModel.WorkOrder.WorkorderCalltypeDesc = "Installation";
            workorderModel.WorkOrder.WorkorderErfid = erfModel.ErfAssetsModel.Erf.ErfID;
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
            workorderModel.NewNotes = erfModel.NewNotes;

            workorderModel.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
            workorderModel.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
            workorderModel.WorkOrderParts = new List<WorkOrderPartModel>();
            workorderModel.Erf = erf;
            WorkorderController wc = new WorkorderController();


            jsonResult = wc.SaveWorkOrder(workorderModel, null, string.Empty, false, true);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            WorkOrderResults result = serializer.Deserialize<WorkOrderResults>(serializer.Serialize(jsonResult.Data));
            if (result.returnValue > 0)
            {
                erf.WorkorderID = Convert.ToInt32(result.WorkOrderId);
                ErfWorkorderLog erfWorkOrderLog = new ErfWorkorderLog();
                erfWorkOrderLog.ErfID = erf.ErfID;
                erfWorkOrderLog.WorkorderID = Convert.ToInt32(erf.WorkorderID);
                FarmerBrothersEntitites.ErfWorkorderLogs.Add(erfWorkOrderLog);

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = CurrentTime,
                    Notes = @"Work Order created from ERF(Approve Process) WO#: " + Convert.ToInt32(result.WorkOrderId) + @" in “MARS”!",
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = UserName,
                    ErfID = erf.ErfID,
                    WorkorderID = erf.WorkorderID == 0 ? null : (int?)erf.WorkorderID,
                    isDispatchNotes = 0
                    //WorkorderID = erf.WorkorderID
                };

                FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
            }

            effectedRecords = FarmerBrothersEntitites.SaveChanges();

            int returnValue = effectedRecords > 0 ? 1 : 0;

            ERFEmail(erf.ErfID, erf.WorkorderID, erfModel.CrateWorkOrder, erfModel.ApprovalStatus, false, true);

            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = "Work Order created from ERF(Approve Process) WO#: " + Convert.ToInt32(result.WorkOrderId) + @" in “MARS”!" };
            return jsonResult;
        }


        #region ERF Data Maintenance
        
        public ActionResult ERFDataMaintenance(int? isBack)
        {
            ERFDataMaintenanceModel erfDataMain = new ERFDataMaintenanceModel();

            List<Contingent> contingentsList = FarmerBrothersEntitites.Contingents.ToList();
            /*erfDataMain.ContingentsList = new List<ContingentModel>();
            foreach (Contingent contin in contingentsList)
            {
                ContingentModel cm = new ContingentModel();
                cm.ContingentId = contin.ContingentID;
                cm.ContingentName = contin.ContingentName;
                cm.ContingentType = contin.ContingentType;
                cm.IsActive = Convert.ToBoolean(contin.IsActive);
                erfDataMain.ContingentsList.Add(cm);
            }*/
            erfDataMain.ContingentsList = GetContingetData();

            /*List<ContingentDetail> contingentDetailsList = FarmerBrothersEntitites.ContingentDetails.ToList();
            erfDataMain.ContingentItemsList = new List<ContingentDetails>();
            foreach (ContingentDetail contindtl in contingentDetailsList)
            {
                ContingentDetails cdm = new ContingentDetails();
                cdm.id = contindtl.ID;
                cdm.ContingentId = contindtl.ContingentID;
                cdm.Name = string.IsNullOrEmpty(contindtl.Name) ? "" : contindtl.Name;
                cdm.LadinCost = contindtl.LaidInCost == null ? 0 : Math.Round(Convert.ToDecimal(contindtl.LaidInCost),2);
                cdm.CashSale = contindtl.CashSale == null ? 0 : Math.Round(Convert.ToDecimal(contindtl.CashSale), 2);
                cdm.Rental = contindtl.Rental == null ? 0 : Math.Round(Convert.ToDecimal(contindtl.Rental), 2);

                Contingent cn = contingentsList.Where(c => c.ContingentID == contindtl.ContingentID).FirstOrDefault();

                cdm.ContingentName = (cn == null) ? "" : string.IsNullOrEmpty(cn.ContingentName) ? "" : cn.ContingentName;
                cdm.IsActive = Convert.ToBoolean(contindtl.IsActive);
                erfDataMain.ContingentItemsList.Add(cdm);
            }*/
            erfDataMain.ContingentItemsList = GetContingetDetailsData();

            /*List<ERFOrderType> OrdertypeList = FarmerBrothersEntitites.ERFOrderTypes.ToList();
            erfDataMain.OrderTypeList = new List<OrderType>();
            foreach (ERFOrderType ordTyp in OrdertypeList)
            {
                OrderType ord = new OrderType();
                ord.OrderTypeId = ordTyp.OrderTypeId;
                ord.OrderTypeDesc = ordTyp.OrderType;
                ord.IsActive = Convert.ToBoolean(ordTyp.IsActive);
                erfDataMain.OrderTypeList.Add(ord);
            }*/
            erfDataMain.OrderTypeList = GetOrderTypeData();

            /*List<ERFBranchDetail> BranchList = FarmerBrothersEntitites.ERFBranchDetails.ToList();
            erfDataMain.BranchList = new List<ERFBranchDetails>();
            foreach (ERFBranchDetail branch in BranchList)
            {
                ERFBranchDetails brch = new ERFBranchDetails();
                brch.Id = branch.Id;
                brch.Region = branch.Region;
                brch.BranchNo = branch.Branch;
                brch.BranchName = branch.BranchName;
                brch.District = branch.District;
                brch.Address = branch.Address;
                brch.City = branch.City;
                brch.State = branch.State;
                brch.PostalCode = branch.PostalCode;
                brch.IsActive = Convert.ToBoolean(branch.IsActive);
                erfDataMain.BranchList.Add(brch);
            }*/
            erfDataMain.BranchList = GetBranchDetailsData();

            List<string> cntgnType = new List<string>() { "", "Eqp", "Exp", "Pos" };
            erfDataMain.ContingentTypeList = new List<SubstituionModel>();
            foreach (string cntgTyp in cntgnType)
            {
                erfDataMain.ContingentTypeList.Add(new SubstituionModel(cntgTyp));
            }

            erfDataMain.ContingentNamesList = new List<SubstituionModel>();
            foreach (Contingent cntgNm in contingentsList)
            {
                erfDataMain.ContingentNamesList.Add(new SubstituionModel(cntgNm.ContingentName));
            }

            return View(erfDataMain);
        }

        public ActionResult ERFContingentDataInsert(ContingentModel value)
        {
            IList<ContingentModel> ContingentsData = TempData["Contingents"] as IList<ContingentModel>;
            if (ContingentsData == null)
            {
                ContingentsData = new List<ContingentModel>();
            }

            IndexCounter counter = Utility.GetIndexCounter("Contingent", 1);
            counter.IndexValue++;
            value.ContingentId = counter.IndexValue.Value;

            ContingentsData.Add(value);
            TempData["Contingents"] = ContingentsData;
            TempData.Keep("Contingents");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ERFContingentDataUpdate(ContingentModel value)
        {
            IList<ContingentModel> ContingentsData = TempData["Contingents"] as IList<ContingentModel>;
            if (ContingentsData == null)
            {
                ContingentsData = new List<ContingentModel>();
            }
            
            ContingentsData.Add(value);
            TempData["Contingents"] = ContingentsData;
            TempData.Keep("Contingents");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public List<ContingentModel> GetContingetData()
        {
            List<ContingentModel> contingetModelList = new List<ContingentModel>();
            List<Contingent> contingentsList = FarmerBrothersEntitites.Contingents.ToList();
            foreach (Contingent contin in contingentsList)
            {
                ContingentModel cm = new ContingentModel();
                cm.ContingentId = contin.ContingentID;
                cm.ContingentName = contin.ContingentName;
                cm.ContingentType = contin.ContingentType;
                cm.IsActive = Convert.ToBoolean(contin.IsActive);
                contingetModelList.Add(cm);
            }
            return contingetModelList;
        }
        
        [HttpPost]
        public FileResult ContingentsExcelExport()
        {
            List<ContingentModel> contingentModelResults = GetContingetData();
            string gridModel = HttpContext.Request.Params["GridModel"];

            string[] columns = { "ContingentName", "ContingentType", "IsActive" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(contingentModelResults, "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "ERFContingetsData.xlsx");

        }

        public ActionResult SaveContingents()
        {
            try
            {
                IList<ContingentModel> contingentsList = TempData["Contingents"] as IList<ContingentModel>;

                if (contingentsList != null)
                {
                    foreach(ContingentModel item in contingentsList)
                    {
                        Contingent contingentData = FarmerBrothersEntitites.Contingents.Where(c => c.ContingentID == item.ContingentId).FirstOrDefault();

                        if (contingentData != null)
                        {
                            contingentData.ContingentName = item.ContingentName;
                            contingentData.ContingentType = item.ContingentType;
                            contingentData.IsActive = item.IsActive;
                        }
                        else
                        {
                            contingentData = new Contingent();
                            contingentData.ContingentID = item.ContingentId;
                            contingentData.ContingentName = item.ContingentName;
                            contingentData.ContingentType = item.ContingentType;
                            contingentData.IsActive = item.IsActive;
                            FarmerBrothersEntitites.Contingents.Add(contingentData);
                        }
                    }
                }
                    FarmerBrothersEntitites.SaveChanges();
            }
            catch(Exception ex)
            {

            }
            JsonResult jsonResult = new JsonResult();
            //jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;       
        }


        //=================================================================

        public ActionResult ERFContingentDetailsDataInsert(ContingentDetails value)
        {
            IList<ContingentDetails> ContingentDetailsData = TempData["ContingentDetails"] as IList<ContingentDetails>;
            if (ContingentDetailsData == null)
            {
                ContingentDetailsData = new List<ContingentDetails>();
            }

            IndexCounter counter = Utility.GetIndexCounter("ContingentDetails", 1);
            counter.IndexValue++;
            value.id = counter.IndexValue.Value;

            ContingentDetailsData.Add(value);
            TempData["ContingentDetails"] = ContingentDetailsData;
            TempData.Keep("ContingentDetails");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ERFContingentDetailsDataUpdate(ContingentDetails value)
        {
            IList<ContingentDetails> ContingentDetailsData = TempData["ContingentDetails"] as IList<ContingentDetails>;
            if (ContingentDetailsData == null)
            {
                ContingentDetailsData = new List<ContingentDetails>();
            }

            ContingentDetailsData.Add(value);
            TempData["ContingentDetails"] = ContingentDetailsData;
            TempData.Keep("ContingentDetails");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public List<ContingentDetails> GetContingetDetailsData()
        {
            List<Contingent> contingentsList = FarmerBrothersEntitites.Contingents.ToList();
            List<ContingentDetail> contingentDetailsList = FarmerBrothersEntitites.ContingentDetails.ToList();
            List<ContingentDetails> ContingentItemsList = new List<ContingentDetails>();
            foreach (ContingentDetail contindtl in contingentDetailsList)
            {
                ContingentDetails cdm = new ContingentDetails();
                cdm.id = contindtl.ID;
                cdm.ContingentId = contindtl.ContingentID;
                cdm.Name = string.IsNullOrEmpty(contindtl.Name) ? "" : contindtl.Name;
                cdm.LadinCost = contindtl.LaidInCost == null ? 0 : Math.Round(Convert.ToDecimal(contindtl.LaidInCost), 2);
                cdm.CashSale = contindtl.CashSale == null ? 0 : Math.Round(Convert.ToDecimal(contindtl.CashSale), 2);
                cdm.Rental = contindtl.Rental == null ? 0 : Math.Round(Convert.ToDecimal(contindtl.Rental), 2);

                Contingent cn = contingentsList.Where(c => c.ContingentID == contindtl.ContingentID).FirstOrDefault();

                cdm.ContingentName = (cn == null) ? "" : string.IsNullOrEmpty(cn.ContingentName) ? "" : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cn.ContingentName.ToUpper().Trim().ToLower());
                cdm.IsActive = Convert.ToBoolean(contindtl.IsActive);
                ContingentItemsList.Add(cdm);
            }
            return ContingentItemsList;
        }

        [HttpPost]
        public FileResult ContingentDetailsExcelExport()
        {
            List<ContingentDetails> contingentDetailsResults = GetContingetDetailsData();
            string gridModel = HttpContext.Request.Params["GridModel"];

            string[] columns = { "Name", "ContingentName", "LadinCost", "CashSale", "Rental", "IsActive" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(contingentDetailsResults, "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "ERFContingetDetailsData.xlsx");

        }

        public ActionResult SaveContingentDetails()
        {
            try
            {
                IList<ContingentDetails> contingentDetailsList = TempData["ContingentDetails"] as IList<ContingentDetails>;

                if (contingentDetailsList != null)
                {
                    foreach (ContingentDetails item in contingentDetailsList)
                    {
                        ContingentDetail contingentDetailsData = FarmerBrothersEntitites.ContingentDetails.Where(c => c.ID == item.id).FirstOrDefault();

                        Contingent cntg = FarmerBrothersEntitites.Contingents.Where(c => c.ContingentName == item.ContingentName).FirstOrDefault();
                        if (contingentDetailsData != null)
                        {
                            contingentDetailsData.Name = string.IsNullOrEmpty(item.Name) ? "" : item.Name;
                            contingentDetailsData.ContingentID = cntg == null ? 0 : cntg.ContingentID;
                            contingentDetailsData.LaidInCost = item.LadinCost;
                            contingentDetailsData.CashSale = item.CashSale;
                            contingentDetailsData.Rental = item.Rental;
                            contingentDetailsData.IsActive = item.IsActive;
                        }
                        else
                        {
                            contingentDetailsData = new ContingentDetail();
                            contingentDetailsData.ID = item.id;
                            contingentDetailsData.Name = string.IsNullOrEmpty(item.Name) ? "" : item.Name;
                            contingentDetailsData.ContingentID = cntg == null ? 0 : cntg.ContingentID;
                            contingentDetailsData.LaidInCost = item.LadinCost;
                            contingentDetailsData.CashSale = item.CashSale;
                            contingentDetailsData.Rental = item.Rental;
                            contingentDetailsData.IsActive = item.IsActive;
                            FarmerBrothersEntitites.ContingentDetails.Add(contingentDetailsData);
                        }
                    }
                }
                FarmerBrothersEntitites.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            JsonResult jsonResult = new JsonResult();
            //jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }



        //=================================================================        

        public ActionResult ERFOrderTypeDataInsert(OrderType value)
        {
            IList<OrderType> OrdTypData = TempData["OrderTypeData"] as IList<OrderType>;
            if (OrdTypData == null)
            {
                OrdTypData = new List<OrderType>();
            }

            IndexCounter counter = Utility.GetIndexCounter("OrderType", 1);
            counter.IndexValue++;
            value.OrderTypeId = counter.IndexValue.Value;

            OrdTypData.Add(value);
            TempData["OrderTypeData"] = OrdTypData;
            TempData.Keep("OrderTypeData");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ERFOrderTypeDataUpdate(OrderType value)
        {
            IList<OrderType> OrdTypData = TempData["OrderTypeData"] as IList<OrderType>;
            if (OrdTypData == null)
            {
                OrdTypData = new List<OrderType>();
            }

            OrdTypData.Add(value);
            TempData["OrderTypeData"] = OrdTypData;
            TempData.Keep("OrderTypeData");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public List<OrderType> GetOrderTypeData()
        {           
            List<OrderType> OrderTypeList = new List<OrderType>();
            List<ERFOrderType> OrdertypeList = FarmerBrothersEntitites.ERFOrderTypes.ToList();           
            foreach (ERFOrderType ordTyp in OrdertypeList)
            {
                OrderType ord = new OrderType();
                ord.OrderTypeId = ordTyp.OrderTypeId;
                ord.OrderTypeDesc = ordTyp.OrderType;
                ord.IsActive = Convert.ToBoolean(ordTyp.IsActive);
                OrderTypeList.Add(ord);
            }
            return OrderTypeList;
        }

        [HttpPost]
        public FileResult OrderTypeExcelExport()
        {
            List<OrderType> orderTypeResults = GetOrderTypeData();
            string gridModel = HttpContext.Request.Params["GridModel"];

            string[] columns = { "OrderTypeDesc", "IsActive" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(orderTypeResults, "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "ERFOrderTypeData.xlsx");

        }

        public ActionResult SaveOrderTypeData()
        {
            try
            {
                IList<OrderType> OrderTypeDetailsList = TempData["OrderTypeData"] as IList<OrderType>;

                if (OrderTypeDetailsList != null)
                {
                    foreach (OrderType item in OrderTypeDetailsList)
                    {
                        ERFOrderType OrderTypeData = FarmerBrothersEntitites.ERFOrderTypes.Where(c => c.OrderTypeId == item.OrderTypeId).FirstOrDefault();
                        
                        if (OrderTypeData != null)
                        {
                            OrderTypeData.OrderType = string.IsNullOrEmpty(item.OrderTypeDesc) ? "" : item.OrderTypeDesc;
                            OrderTypeData.IsActive = item.IsActive;
                        }
                        else
                        {
                            OrderTypeData = new ERFOrderType();
                            OrderTypeData.OrderTypeId = item.OrderTypeId;
                            OrderTypeData.OrderType = string.IsNullOrEmpty(item.OrderTypeDesc) ? "" : item.OrderTypeDesc;
                            OrderTypeData.IsActive = item.IsActive;
                            FarmerBrothersEntitites.ERFOrderTypes.Add(OrderTypeData);
                        }
                    }
                }
                FarmerBrothersEntitites.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            JsonResult jsonResult = new JsonResult();
            //jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        //=================================================================     


        public ActionResult ERFBranchDataInsert(ERFBranchDetails value)
        {
            IList<ERFBranchDetails> OrdTypData = TempData["BranchData"] as IList<ERFBranchDetails>;
            if (OrdTypData == null)
            {
                OrdTypData = new List<ERFBranchDetails>();
            }

            IndexCounter counter = Utility.GetIndexCounter("ERFBranch", 1);
            counter.IndexValue++;
            value.Id = counter.IndexValue.Value;

            OrdTypData.Add(value);
            TempData["BranchData"] = OrdTypData;
            TempData.Keep("BranchData");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ERFBranchDataUpdate(ERFBranchDetails value)
        {
            IList<ERFBranchDetails> OrdTypData = TempData["BranchData"] as IList<ERFBranchDetails>;
            if (OrdTypData == null)
            {
                OrdTypData = new List<ERFBranchDetails>();
            }

            OrdTypData.Add(value);
            TempData["BranchData"] = OrdTypData;
            TempData.Keep("BranchData");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public List<ERFBranchDetails> GetBranchDetailsData()
        {
            List<ERFBranchDetail> BranchList = FarmerBrothersEntitites.ERFBranchDetails.ToList();
            List<ERFBranchDetails> BranchDtlsList = new List<ERFBranchDetails>();
            foreach (ERFBranchDetail branch in BranchList)
            {
                ERFBranchDetails brch = new ERFBranchDetails();
                brch.Id = branch.Id;
                brch.Region = branch.Region;
                brch.BranchNo = branch.Branch;
                brch.BranchName = branch.BranchName;
                brch.District = branch.District;
                brch.Address = branch.Address;
                brch.City = branch.City;
                brch.State = branch.State;
                brch.PostalCode = branch.PostalCode;
                brch.IsActive = Convert.ToBoolean(branch.IsActive);
                BranchDtlsList.Add(brch);
            }
            return BranchDtlsList;
        }

        [HttpPost]
        public FileResult BranchDetailsExcelExport()
        {
            List<ERFBranchDetails> BranchDetailsResults = GetBranchDetailsData();
            string gridModel = HttpContext.Request.Params["GridModel"];

            string[] columns = { "Region", "BranchNo", "BranchName", "District", "Address", "City", "State", "PostalCode", "IsActive" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(BranchDetailsResults, "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "ERFBranchDetailsData.xlsx");

        }

        public ActionResult SaveBranchData()
        {
            try
            {
                IList<ERFBranchDetails> branchDetailsList = TempData["BranchData"] as IList<ERFBranchDetails>;

                if (branchDetailsList != null)
                {
                    foreach (ERFBranchDetails item in branchDetailsList)
                    {
                        ERFBranchDetail brnchData = FarmerBrothersEntitites.ERFBranchDetails.Where(c => c.Id == item.Id).FirstOrDefault();

                        if (brnchData != null)
                        {
                            brnchData.Region = string.IsNullOrEmpty(item.Region) ? "" : item.Region;
                            brnchData.District = string.IsNullOrEmpty(item.District) ? "" : item.District;
                            brnchData.Branch = string.IsNullOrEmpty(item.BranchNo) ? "" : item.BranchNo;
                            brnchData.BranchName = string.IsNullOrEmpty(item.BranchName) ? "" : item.BranchName;
                            brnchData.Address = string.IsNullOrEmpty(item.Address) ? "" : item.Address;

                            string pCode = string.IsNullOrEmpty(item.PostalCode) ? "" : (item.PostalCode.Length > 5 ? item.PostalCode.Substring(0, 5) : item.PostalCode);

                            if (!string.IsNullOrEmpty(pCode))
                            {
                                Zip ZipDetails = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == pCode).FirstOrDefault();

                                brnchData.PostalCode = pCode;

                                if (ZipDetails != null)
                                {
                                    brnchData.City = ZipDetails.City;
                                    brnchData.State = ZipDetails.State;
                                }

                            }                            

                            brnchData.IsActive = item.IsActive;
                        }
                        else
                        {
                            brnchData = new ERFBranchDetail();
                            brnchData.Id = item.Id;
                            brnchData.Region = string.IsNullOrEmpty(item.Region) ? "" : item.Region;
                            brnchData.District = string.IsNullOrEmpty(item.District) ? "" : item.District;
                            brnchData.Branch = string.IsNullOrEmpty(item.BranchNo) ? "" : item.BranchNo;
                            brnchData.BranchName = string.IsNullOrEmpty(item.BranchName) ? "" : item.BranchName;
                            brnchData.Address = string.IsNullOrEmpty(item.Address) ? "" : item.Address;

                            string pCode = string.IsNullOrEmpty(item.PostalCode) ? "" : (item.PostalCode.Length > 5 ? item.PostalCode.Substring(0, 5) : item.PostalCode);

                            if (!string.IsNullOrEmpty(pCode))
                            {
                                Zip ZipDetails = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == pCode).FirstOrDefault();

                                brnchData.PostalCode = pCode;

                                if (ZipDetails != null)
                                {
                                    brnchData.City = ZipDetails.City;
                                    brnchData.State = ZipDetails.State;
                                }

                            }

                            brnchData.IsActive = item.IsActive;
                            FarmerBrothersEntitites.ERFBranchDetails.Add(brnchData);
                        }
                    }
                }
                FarmerBrothersEntitites.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            JsonResult jsonResult = new JsonResult();
            //jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        #endregion


        #region Bulk ERF Upoad
        public ActionResult ERFBulkUpload(int? isBack)
        {
            return View();
        }

        public ActionResult BulkERFUploadFile(HttpPostedFileBase file)
        {
            FarmerBrothersEntities FarmerBrothersEntities = new FarmerBrothersEntities();
            JsonResult jsonResult = new JsonResult();
            //var attachedFile = System.Web.HttpContext.Current.Request.Files["CsvDoc"];
            var attachedFile = file;
            if (attachedFile == null || attachedFile.ContentLength <= 0)
            {
                ViewBag.Message = "No File Selected ";
                ViewBag.isSuccess = false;
                ViewBag.dataSource = new List<Erf>();
                return View("ERFBulkUpload");

                //jsonResult.Data = new { success = false, serverError = ErrorCode.ERROR, message = "No File Selected", data = new List<Erf>() };
                //return Json(jsonResult, JsonRequestBehavior.AllowGet);
            }

            string _FileName = attachedFile.FileName;
            if (Path.GetExtension(_FileName).ToLower() != ".csv")
            {
                ViewBag.Message = "Selected file is not CSV file";
                ViewBag.isSuccess = false;
                ViewBag.dataSource = new List<Erf>();
                return View("ERFBulkUpload");

                //jsonResult.Data = new { success = false, serverError = ErrorCode.ERROR, message = "Selected file is not CSV file", data = new List<Erf>() };
                //return Json(jsonResult, JsonRequestBehavior.AllowGet);
            }

            string DirPath = Server.MapPath("~/UploadedFiles/ERF");
            DateTime currentDate = DateTime.Now;
            if (!Directory.Exists(DirPath))
            {
                Directory.CreateDirectory(DirPath);
            }
            string _inputPath = Path.Combine(DirPath, _FileName);
            attachedFile.SaveAs(_inputPath);

            var csvReader = new StreamReader(attachedFile.InputStream);
            //var uploadModelList = new List<CsvRecordsViewModel>();
            string inputDataRead;
            List<ERFBulkUploadDataModel> erfDataList = new List<ERFBulkUploadDataModel>();

            FileReading fileDataObj = new FileReading();
            fileDataObj.IsValid = true;
            fileDataObj.FileName = _FileName;

            int i = 0;
            while ((inputDataRead = csvReader.ReadLine()) != null)
            {
                if (i == 0)
                {
                    fileDataObj = IsValidERFCSVFile(inputDataRead);

                    if (!fileDataObj.IsValid)
                    {
                        ViewBag.Message = "File upload failed!! " + "\n" + fileDataObj.ErrorMsg;
                        ViewBag.isSuccess = false;
                        ViewBag.dataSource = new List<Erf>();
                        return View("ERFBulkUpload");

                        //jsonResult.Data = new { success = false, serverError = ErrorCode.ERROR, message = "File upload failed!! " + "\n" + fileDataObj.ErrorMsg, data = new List<Erf>() };
                        //return Json(jsonResult, JsonRequestBehavior.AllowGet); ;
                    }
                }

                if (i != 0)
                {
                    if (string.IsNullOrEmpty(inputDataRead.Replace(',', ' ').Trim())) continue;

                    erfDataList.Add(new ERFBulkUploadDataModel(inputDataRead));

                   //erfDataList.Add(inputDataRead.Trim());
                }

                i++;
            }

            List<Erf> erfModelList = new List<Erf>();
            var uniqueAccountNums = erfDataList.Select(e => e.AccountNumber).Distinct();
            foreach(int actNo in uniqueAccountNums)
            {
                List<ERFBulkUploadDataModel> grpDataList = erfDataList.Where(e => e.AccountNumber == actNo).ToList();
                ERFBulkUploadDataModel grpData = grpDataList[0];
                string message = ""; bool validFlag = true;
                ErfModel erfMdl = new ErfModel();
                CustomerModel custMdl = GetCustomerDetails(grpData.AccountNumber, FarmerBrothersEntities);

                Erf erfResult = new Erf();
                if (custMdl == null)
                {
                    erfResult.CustomerID = grpData.AccountNumber;
                    erfResult.CustomerName = grpData.MainContactName;
                    erfResult.GroupId = grpData.groupNum;
                    erfResult.BulkUploadResult = "Failed";
                    erfResult.UploadError = "Invalid Customer";

                    erfModelList.Add(erfResult);

                    continue;
                }

                
                if(string.IsNullOrEmpty(grpData.OrderType))
                {
                    message += " | Order Type Required ";
                    validFlag = false;
                }
                if (string.IsNullOrEmpty(grpData.ShipToBranch))
                {
                    message += " | ShipTo Branch Required";
                    validFlag = false;
                }
                if (grpData.FormDate == null)
                {
                    message += " | Form Date Required";
                    validFlag = false;
                }
                if (grpData.ERFReceivedDate == null)
                {
                    message += " | ERF Received Date Required";
                    validFlag = false;
                }
                if (grpData.ERFProcessedDate == null)
                {
                    message += " | ERF Processed Date Required";
                    validFlag = false;
                }
                if (grpData.InstallDate == null)
                {
                    message += " | InstallDate Date Required";
                    validFlag = false;
                }
                if (string.IsNullOrEmpty(grpData.HoursofOperation))
                {
                    message += " | Hours Of Operation Required";
                    validFlag = false;
                }
                if (string.IsNullOrEmpty(grpData.InstallLocation))
                {
                    message += " | Install Location Required";
                    validFlag = false;
                }
                if (string.IsNullOrEmpty(grpData.SiteReady))
                {
                    message += " | Site Ready Value Required";
                    validFlag = false;
                }
                if (grpData.AdditionalNSV == 0)
                {
                    message += " | Additional NSV Required";
                    validFlag = false;
                }

                erfMdl.Customer = custMdl;
                erfMdl.CreatedBy = "Bulk Upload";
                erfMdl.CrateWorkOrder = false;
                erfMdl.ApprovalStatus = "Approved for Processing";
                erfMdl.OrderType = grpData.OrderType;
                erfMdl.BranchName = grpData.ShipToBranch;
                                
                NotesModel nm = new NotesModel();
                nm.Notes = grpData.ErfNotes;

                erfMdl.Notes = nm;

                erfMdl.ErfAssetsModel = new ErfAssetsModel();
                erfMdl.ErfAssetsModel.Erf = new Erf();

                erfMdl.ErfAssetsModel.Erf.CustomerMainContactName = grpData.MainContactName;
                erfMdl.Customer.MainContactName = grpData.MainContactName;
                erfMdl.Customer.PhoneNumber = grpData.MainContactNum;
                erfMdl.ErfAssetsModel.Erf.Phone = grpData.MainContactNum;
                erfMdl.ErfAssetsModel.Erf.DateERFReceived = grpData.ERFReceivedDate;
                erfMdl.ErfAssetsModel.Erf.DateERFProcessed = grpData.ERFProcessedDate;
                erfMdl.ErfAssetsModel.Erf.DateOnERF = grpData.FormDate;
                erfMdl.ErfAssetsModel.Erf.OriginalRequestedDate = grpData.InstallDate == null ? currentDate : Convert.ToDateTime(grpData.InstallDate);
                erfMdl.ErfAssetsModel.Erf.HoursofOperation = grpData.HoursofOperation;
                erfMdl.ErfAssetsModel.Erf.InstallLocation = grpData.InstallLocation;
                erfMdl.ErfAssetsModel.Erf.SiteReady = grpData.SiteReady;

                List<FBCBE> fbcbeList = FarmerBrothersEntitites.FBCBEs.Where(cbe => cbe.CurrentCustomerId == grpData.AccountNumber).ToList();
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
                erfMdl.TotalNSV = grpData.AdditionalNSV;

                
                
                List<ERFManagementEquipmentModel> erfEqpList = new List<ERFManagementEquipmentModel>();
                List<ERFManagementExpendableModel> erfExpList = new List<ERFManagementExpendableModel>();
                foreach (ERFBulkUploadDataModel grpDataEqp in grpDataList)
                {
                    ERFManagementEquipmentModel eqp = new ERFManagementEquipmentModel();
                    string eqpCategory = string.IsNullOrEmpty(grpDataEqp.EqpCategory) ? "" : grpDataEqp.EqpCategory.Replace('\"', ' ').Trim();
                    if (!string.IsNullOrEmpty(eqpCategory))
                    {
                        Contingent eqpCon = FarmerBrothersEntitites.Contingents.Where(c => c.ContingentName.ToLower() == eqpCategory.ToLower()).FirstOrDefault();
                        if (eqpCon != null)
                        {
                            eqp.Category = eqpCon.ContingentID;

                            string eqpBrand = string.IsNullOrEmpty(grpDataEqp.EqpBrand) ? "" : grpDataEqp.EqpBrand.Replace('\"', ' ').Trim();
                           
                                ContingentDetail eqpConDtl = FarmerBrothersEntitites.ContingentDetails.Where(c => c.Name.ToLower() == eqpBrand.ToLower()).FirstOrDefault();
                            if (eqpConDtl != null)
                            {
                                eqp.Brand = eqpConDtl.ID;

                                eqp.Quantity = grpDataEqp.EqpQuantity;
                                eqp.Substitution = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(grpDataEqp.EqpSubstitutionPossible.ToLower());
                                eqp.TransactionType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(grpDataEqp.EqpTransType.ToLower());
                                eqp.EquipmentType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(grpDataEqp.EqpType.ToLower());
                                eqp.Branch = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(grpDataEqp.EqpUsingBranch.ToLower());

                                if (grpData.EqpType.ToLower() == "new")
                                {
                                    if (grpData.EqpTransType.ToLower() == "case sale")
                                    {
                                        eqp.LaidInCost = Convert.ToDouble(eqpConDtl.LaidInCost);
                                        eqp.RentalCost = Convert.ToDouble(eqpConDtl.CashSale);
                                    }
                                    else if (grpData.EqpTransType.ToLower() == "rental")
                                    {
                                        double laidCost = Convert.ToDouble(eqpConDtl.LaidInCost);
                                        eqp.LaidInCost = laidCost;
                                        eqp.RentalCost = (laidCost) / 24;
                                    }
                                    if (grpData.EqpTransType.ToLower() == "loan")
                                    {
                                        eqp.LaidInCost = Convert.ToDouble(eqpConDtl.LaidInCost);
                                        eqp.RentalCost = 0;
                                    }
                                }
                                else if (grpData.EqpType.ToLower() == "refurb")
                                {
                                    if (grpData.EqpTransType.ToLower() == "case sale")
                                    {
                                        double laidCost = Convert.ToDouble(eqpConDtl.LaidInCost) * 0.75;
                                        eqp.LaidInCost = laidCost;
                                        eqp.RentalCost = laidCost + (0.3 * laidCost);
                                    }
                                    else if (grpData.EqpTransType.ToLower() == "rental")
                                    {
                                        double laidCost = Convert.ToDouble(eqpConDtl.LaidInCost);
                                        eqp.LaidInCost = laidCost;
                                        eqp.RentalCost = (laidCost * 0.75) / 24;
                                    }
                                    if (grpData.EqpTransType.ToLower() == "loan")
                                    {
                                        double laidCost = Convert.ToDouble(eqpConDtl.LaidInCost) * 0.75;
                                        eqp.LaidInCost = laidCost;
                                        eqp.RentalCost = 0;
                                    }
                                }

                                eqp.TotalCost = grpDataEqp.EqpQuantity * eqp.LaidInCost;
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


                    ERFManagementExpendableModel exp = new ERFManagementExpendableModel();
                    string expCategory = string.IsNullOrEmpty(grpDataEqp.ExpCategory) ? "" : grpDataEqp.ExpCategory.Replace('\"', ' ').Trim();
                    if (!string.IsNullOrEmpty(expCategory))
                    {
                        Contingent expCon = FarmerBrothersEntitites.Contingents.Where(c => c.ContingentName.ToLower() == expCategory.ToLower()).FirstOrDefault();
                        if (expCon != null)
                        {
                            exp.Category = expCon.ContingentID;

                            string expBrand = string.IsNullOrEmpty(grpDataEqp.ExpBrand) ? "" : grpDataEqp.ExpBrand.Replace('\"', ' ').Trim();
                            ContingentDetail expConDtl = FarmerBrothersEntitites.ContingentDetails.Where(c => c.Name.ToLower() == expBrand.ToLower()).FirstOrDefault();
                            if (expConDtl != null)
                            {
                                exp.Brand = expConDtl.ID;

                                exp.Quantity = grpDataEqp.ExpQuantity;
                                exp.Substitution = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(grpDataEqp.ExpSubstitutionPossible.ToLower());
                                exp.TransactionType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(grpDataEqp.ExpTransType.ToLower());
                                exp.EquipmentType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(grpDataEqp.ExpType.ToLower());
                                exp.Branch = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(grpDataEqp.ExpUsingBranch.ToLower());


                                if (grpData.EqpTransType.ToLower() == "case sale")
                                {
                                    double laidCost = Convert.ToDouble(expConDtl.LaidInCost);
                                    eqp.LaidInCost = laidCost;
                                    eqp.RentalCost = Convert.ToDouble(expConDtl.CashSale);
                                }
                                else if (grpData.EqpTransType.ToLower() == "rental")
                                {
                                    double laidCost = Convert.ToDouble(expConDtl.LaidInCost);
                                    eqp.LaidInCost = laidCost;
                                    eqp.RentalCost = Convert.ToDouble(expConDtl.Rental);
                                }
                                if (grpData.EqpTransType.ToLower() == "loan")
                                {
                                    double laidCost = Convert.ToDouble(expConDtl.LaidInCost);
                                    eqp.LaidInCost = laidCost;
                                    eqp.RentalCost = 0;
                                }

                                eqp.TotalCost = grpDataEqp.EqpQuantity * eqp.LaidInCost;

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


               
                /*if (validFlag)
                {
                    erfMdl.ErfAssetsModel.EquipmentList = erfEqpList;
                    erfMdl.ErfAssetsModel.ExpendableList = erfExpList;

                    Erf erf = new Erf();
                    int result = ERFSave(erfMdl, out erf, out message);

                    erf.GroupId = grpData.groupNum;
                    if (result > 0)
                    {
                        erf.BulkUploadResult = "Success";
                    }
                    else
                    {
                        erf.BulkUploadResult = "Failed";
                        erf.UploadError = message;
                    }

                    erfModelList.Add(erf);
                }*/
                if(validFlag)
                {
                    erfMdl.ErfAssetsModel.EquipmentList = erfEqpList;
                    erfMdl.ErfAssetsModel.ExpendableList = erfExpList;

                    Erf erf = new Erf();
                    int result = ERFSave(erfMdl, out erf, out message);

                    erfResult.CustomerID = erf.CustomerID;
                    erfResult.CustomerName = erf.CustomerName;
                    erfResult.GroupId = grpData.groupNum;
                    if (result > 0)
                    {
                        erfResult.ErfID = erf.ErfID;
                        erfResult.WorkorderID = erf.WorkorderID;
                        erfResult.BulkUploadResult = "Success";
                    }
                    else
                    {
                        erfResult.BulkUploadResult = "Failed";
                        erfResult.UploadError = message;
                    }

                    erfModelList.Add(erfResult);
                }
                else
                {
                    erfResult.CustomerID = grpData.AccountNumber;
                    erfResult.CustomerName = grpData.MainContactName;
                    erfResult.GroupId = grpData.groupNum;
                    erfResult.BulkUploadResult = "Failed";
                    erfResult.UploadError = message;

                    erfModelList.Add(erfResult);
                }

                
            }

            ViewBag.Message = "Upload Success";
            ViewBag.isSuccess = true;
            ViewBag.dataSource = erfModelList;
            return View("ERFBulkUpload");

            //jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = "Upload Success", data = erfModelList };
            //return Json(jsonResult,JsonRequestBehavior.AllowGet);           
        }

        public static CustomerModel GetCustomerDetails(int CustomerId, FarmerBrothersEntities farmerBrothersEntities)
        {
            CustomerModel CustMdl = null;

            Contact contact = farmerBrothersEntities.Contacts.Where(c => c.ContactID == CustomerId).FirstOrDefault();

            if(contact != null)
            {
                CustMdl = new CustomerModel();

                CustMdl.ManagerName = WebConfigurationManager.AppSettings["ManagerName"];
                CustMdl.ManagerPhone = Utility.FormatPhoneNumber(WebConfigurationManager.AppSettings["ManagerPhone"]);
                CustMdl.CustomerId = contact.ContactID.ToString();
                CustMdl.CustomerName = contact.CompanyName;
                CustMdl.Address = contact.Address1;
                CustMdl.Address2 = contact.Address2;
                CustMdl.City = contact.City;
                CustMdl.State = contact.State;
                CustMdl.ZipCode = contact.PostalCode;
                CustMdl.MainContactName = contact.FirstName + ' ' + contact.LastName;
                CustMdl.AreaCode = contact.AreaCode;
                CustMdl.PhoneNumber = Utility.FormatPhoneNumber(contact.PhoneWithAreaCode);
                CustMdl.MainEmailAddress = contact.Email;
                CustMdl.TSM = contact.TierDesc;
                CustMdl.TSMPhone = Utility.FormatPhoneNumber(contact.PhoneWithAreaCode);
                CustMdl.DistributorName = contact.DistributorName;        
                CustMdl.WorkOrderId = null;
                CustMdl.ErfId = null;
                CustMdl.Region = contact.RegionNumber;
                CustMdl.Branch = contact.Branch;
                CustMdl.Route = contact.Route;
                CustMdl.PricingParent = contact.PricingParentDesc;
                CustMdl.ServiceLevel = contact.ServiceLevelCode;
                CustMdl.LastSaleDate = contact.LastSaleDate;
                CustMdl.ParentNumber = contact.PricingParentID;

               CustMdl.unknownCustomer = contact.IsUnknownUser == null ? false : contact.IsUnknownUser == 1 ? true : false;
                CustMdl.IsNonFBCustomer = contact.IsNonFbCustomer == null ? false : Convert.ToBoolean(contact.IsNonFbCustomer);


                CustMdl.ServiceTier = string.IsNullOrEmpty(contact.ProfitabilityTier) ? " - " : contact.ProfitabilityTier;
             
                CustMdl.NetSalesAmt = contact.NetSalesAmount == null ? 0 : Convert.ToDecimal(contact.NetSalesAmount);
                CustMdl.ContributionMargin = string.IsNullOrEmpty(contact.ContributionMargin) ? "" : contact.ContributionMargin;

                string paymentTerm = string.IsNullOrEmpty(contact.PaymentTerm) ? "" : contact.PaymentTerm;
                if (!string.IsNullOrEmpty(paymentTerm))
                {
                    JDEPaymentTerm paymentDesc = farmerBrothersEntities.JDEPaymentTerms.Where(c => c.PaymentTerm == paymentTerm).FirstOrDefault();
                    CustMdl.PaymentTermDesc = paymentDesc == null ? "" : paymentDesc.Description;
                }
                else
                {
                    CustMdl.PaymentTermDesc = "";
                }


                CustMdl.CustomerTimeZone = Utility.GetCustomerTimeZone(contact.PostalCode, farmerBrothersEntities);
                CustMdl.CurrentTime = Utility.GetCurrentTime(contact.PostalCode, farmerBrothersEntities).ToString("hh:mm tt");


                CustMdl.DaysSinceLastSale = CustMdl.ConvertToDays(CustMdl.CurrentTime, CustMdl.LastSaleDate);
                CustMdl.CustomerSpecialInstructions = contact.CustomerSpecialInstructions;

                
                CustMdl.UtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMilliseconds;
                int? providerNumber = contact.FBProviderID == null ? 0 : Convert.ToInt32(contact.FBProviderID);
                using (FarmerBrothersEntities entities = new FarmerBrothersEntities())
                {
                    CustMdl.ESMName = string.IsNullOrEmpty(contact.ESMName) ? "" : contact.ESMName;
                    CustMdl.ESMphone = string.IsNullOrEmpty(contact.ESMPhone) ? "" : Utility.FormatPhoneNumber(contact.ESMPhone);

                    CustMdl.DSMName = string.IsNullOrEmpty(contact.CCMName) ? "" : contact.CCMName;
                    CustMdl.DSMPhone = string.IsNullOrEmpty(contact.CCMPhone) ? "" : Utility.FormatPhoneNumber(contact.CCMPhone);

                    CustMdl.RSMName = string.IsNullOrEmpty(contact.RSMName) ? "" : contact.RSMName;
                    CustMdl.RSMphone = string.IsNullOrEmpty(contact.RSMPhone) ? "" : Utility.FormatPhoneNumber(contact.RSMPhone);

                    CustMdl.CCMName = string.IsNullOrEmpty(contact.CCMName) ? "" : contact.CCMName;
                    CustMdl.CCMphone = string.IsNullOrEmpty(contact.CCMPhone) ? "" : Utility.FormatPhoneNumber(contact.CCMPhone);

                    var Providers = entities.TECH_HIERARCHY.FirstOrDefault(x => x.DealerId == providerNumber);
                    if (Providers != null)
                    {
                        CustMdl.FBProviderID = Providers.DealerId;
                        CustMdl.PreferredProvider = Providers.CompanyName;
                        string providerPhone = string.Empty;
                        if (Providers.Phone != null && Providers.Phone.Replace("-", "").Length == 7)
                        {
                            providerPhone = Providers.AreaCode + Providers.Phone.Replace("-", "");
                        }
                        else
                        {
                            providerPhone = Providers.Phone;
                        }
                        CustMdl.ProviderPhone = Utility.FormatPhoneNumber(providerPhone); ;
                    }
                }
            }

            return CustMdl;
        }

        public ActionResult BulkERFUploadFile1(HttpPostedFileBase file)
        {
            try
            {
                List<ErfModel> erfList = new List<ErfModel>();
                if (file == null)
                {
                    ViewBag.Message = "No File Selected ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<ErfModel>();
                    return View("ERFDataMaintenance");
                }

                else if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                {
                    ViewBag.Message = "Selected file is not CSV file ";
                    ViewBag.isSuccess = false;
                    ViewBag.dataSource = new List<ErfModel>();
                    return View("ERFDataMaintenance");
                }

                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string DirPath = Server.MapPath("~/UploadedFiles/ERF");
                    DateTime currentDate = DateTime.Now;
                    if (!Directory.Exists(DirPath))
                    {
                        Directory.CreateDirectory(DirPath);
                    }
                    string _inputPath = Path.Combine(DirPath, _FileName);
                    file.SaveAs(_inputPath);

                    string _path = Path.Combine(DirPath, _FileName);

                    var contents = System.IO.File.ReadAllText(_path).Split('\n');
                    FileReading fileDataObj = new FileReading();
                    fileDataObj.IsValid = true;
                    fileDataObj.FileName = _FileName;
                    int i = 0;

                    foreach (string line in contents)
                    {
                        if (string.IsNullOrEmpty(line)) continue;
                        string lineVal = line.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace('\\', ' ').Replace("\"", "");
                        if (string.IsNullOrEmpty(lineVal) || lineVal == " ") continue;

                        if (i == 0)
                        {
                            fileDataObj = IsValidERFCSVFile(lineVal);

                            if (!fileDataObj.IsValid)
                            {
                                ViewBag.Message = "File upload failed!! " + "\n" + fileDataObj.ErrorMsg;
                                ViewBag.isSuccess = false;
                                ViewBag.dataSource = new List<ErfModel>();
                                return View("ERFDataMaintenance");
                            }
                        }
                        string[] lineValues = lineVal.Split(',');
                        if (i != 0)
                        {
                            string CustomerId = "", CustomerName = "", Address1 = "", Address2 = "", Address3 = "", City = "", State = "", ZipCode = "", PhoneNumber = "", Route = "", Branch = "", RouteCode = "";
                            string ErrorMessage = "";
                            for (int ind = 0; ind <= lineValues.Count() - 1; ind++)
                            {
                                string str = lineValues[ind].Trim();
                                switch (ind)
                                {
                                    case 0:
                                        CustomerId = string.IsNullOrEmpty(str) ? "" : str;
                                        if (string.IsNullOrEmpty(CustomerId)) { ErrorMessage += "Customer Number is Missing"; }
                                        break;
                                    case 1:
                                        CustomerName = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(CustomerName)) { ErrorMessage += "Customer Name is Missing"; }
                                        break;
                                    case 2:
                                        Address1 = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(Address1)) { ErrorMessage += "Address1 is Missing"; }
                                        break;
                                    case 3:
                                        Address2 = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 4:
                                        Address3 = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 5:
                                        City = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(City)) { ErrorMessage += "City is Missing"; }
                                        break;
                                    case 6:
                                        State = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(State)) { ErrorMessage += "State is Missing"; }
                                        break;
                                    case 7:
                                        ZipCode = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(ZipCode)) { ErrorMessage += "ZipCode is Missing"; }
                                        break;
                                    case 8:
                                        PhoneNumber = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                    case 9:
                                        Route = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(Route)) { ErrorMessage += "Route is Missing"; }
                                        break;
                                    case 10:
                                        Branch = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        if (string.IsNullOrEmpty(Branch)) { ErrorMessage += "Branch is Missing"; }
                                        break;
                                    case 11:
                                        RouteCode = string.IsNullOrEmpty(str) ? "" : str.ToString();
                                        break;
                                }
                            }

                            int contactId = 0;
                            try
                            {
                                contactId = string.IsNullOrEmpty(CustomerId) ? 0 : Convert.ToInt32(CustomerId);
                            }
                            catch (Exception ex)
                            {
                                ErrorMessage += "Customer Number is not in Correct Format";
                            }
                            if (!string.IsNullOrEmpty(ErrorMessage))
                            {
                                ErfModel cm = new ErfModel();
                                //cm.CustomerId = CustomerId;
                                //cm.CustomerName = CustomerName;
                                //cm.Address = Address1;
                                //cm.Address2 = Address2;
                                //cm.Address3 = Address3;
                                //cm.City = City;
                                //cm.State = State;
                                //cm.ZipCode = ZipCode;
                                //cm.PhoneNumber = PhoneNumber;
                                //cm.Route = Route;
                                //cm.Branch = Branch;
                                //cm.RouteCode = RouteCode;
                                //cm.CustomerBranch = Branch;
                                //cm.Message = ErrorMessage;

                                erfList.Add(cm);

                                continue;
                            }

                            if (contactId > 0)
                            {
                                Contact contacttem = FarmerBrothersEntitites.Contacts.Where(cr => cr.ContactID == contactId).FirstOrDefault();
                                if (contacttem == null)
                                {
                                    Contact contact = new Contact();
                                    contact.ContactID = contactId;
                                    contact.CompanyName = CustomerName;
                                    contact.Address1 = Address1;
                                    contact.Address2 = Address2;
                                    contact.Address3 = Address3;
                                    contact.City = City;
                                    contact.State = State;
                                    contact.PostalCode = ZipCode;
                                    contact.Phone = PhoneNumber;
                                    contact.Route = Route;
                                    contact.Branch = Branch;
                                    contact.RouteCode = RouteCode;
                                    contact.DateCreated = currentDate;
                                    contact.LastModified = currentDate;
                                    contact.SearchType = "C";
                                    contact.CustomerBranch = Branch;

                                    FarmerBrothersEntitites.Contacts.Add(contact);

                                }
                                else
                                {
                                    contacttem.ContactID = contactId;
                                    contacttem.CompanyName = CustomerName;
                                    contacttem.Address1 = Address1;
                                    contacttem.Address2 = Address2;
                                    contacttem.Address3 = Address3;
                                    contacttem.City = City;
                                    contacttem.State = State;
                                    contacttem.PostalCode = ZipCode;
                                    contacttem.Phone = PhoneNumber;
                                    contacttem.Route = Route;
                                    contacttem.Branch = Branch;
                                    contacttem.RouteCode = RouteCode;
                                    contacttem.LastModified = currentDate;
                                    contacttem.SearchType = "C";
                                    contacttem.CustomerBranch = Branch;
                                }

                                try
                                {
                                    int result = FarmerBrothersEntitites.SaveChanges();

                                    ErfModel cm = new ErfModel();
                                    //cm.CustomerId = CustomerId;
                                    //cm.CustomerName = CustomerName;
                                    //cm.Address = Address1;
                                    //cm.Address2 = Address2;
                                    //cm.Address3 = Address3;
                                    //cm.City = City;
                                    //cm.State = State;
                                    //cm.ZipCode = ZipCode;
                                    //cm.PhoneNumber = PhoneNumber;
                                    //cm.Route = Route;
                                    //cm.Branch = Branch;
                                    //cm.RouteCode = RouteCode;
                                    //cm.CustomerBranch = Branch;
                                    //if (result == 1)
                                    //{
                                    //    cm.Message = "Success";
                                    //}
                                    //else
                                    //{
                                    //    cm.Message = "Error Saving";
                                    //}
                                    erfList.Add(cm);
                                }
                                catch (Exception ex)
                                {
                                    ErrorMessage += "Problem Saving Contact,  Exception: " + ex.ToString();

                                    ErfModel cm = new ErfModel();
                                    //cm.CustomerId = CustomerId;
                                    //cm.CustomerName = CustomerName;
                                    //cm.Address = Address1;
                                    //cm.Address2 = Address2;
                                    //cm.Address3 = Address3;
                                    //cm.City = City;
                                    //cm.State = State;
                                    //cm.ZipCode = ZipCode;
                                    //cm.PhoneNumber = PhoneNumber;
                                    //cm.Route = Route;
                                    //cm.Branch = Branch;
                                    //cm.RouteCode = RouteCode;
                                    //cm.CustomerBranch = Branch;
                                    //cm.Message = ErrorMessage;

                                    erfList.Add(cm);
                                    continue;
                                }
                            }
                        }
                        i++;
                    }

                }

                ViewBag.Message = "File uploaded ! ";
                ViewBag.isSuccess = true;
                ViewBag.dataSource = erfList;
                return View("ERFDataMaintenance");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "File upload failed!! " + ex;
                ViewBag.isSuccess = false;
                ViewBag.dataSource = new List<ErfModel>();
                return View("ERFDataMaintenance");
            }
        }

        private static FileReading IsValidERFCSVFile(string HeaderRow)
        {
            FileReading fr = new FileReading();
            fr.ErrorMsg = "";
            fr.IsValid = true;

         string[] headerValues = HeaderRow.Split(',');

            for (var index = 0; index <= headerValues.Count() - 1; index++)
            {
                string hdrValue = headerValues[index].ToLower().Trim();

                switch (index)
                {
                    //case 0:
                    //    if (hdrValue != "erf group no")
                    //    {
                    //        fr.ErrorMsg += "\n ERF Group No Column Missing";
                    //        fr.IsValid = false;
                    //    }
                    //    break;
                    case 0:
                        if (hdrValue != "account number")
                        {
                            fr.ErrorMsg += "\n Account Number Column Missing";
                            fr.IsValid = false;
                        }
                        break;                   
                    case 1:
                        if (hdrValue != "main contactname")
                        {
                            fr.ErrorMsg += "\n Main ContactName Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 2:
                        if (hdrValue != "main contactnumber")
                        {
                            fr.ErrorMsg += "\n Main ContactNumber Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 3:
                        if (hdrValue != "hours of operation")
                        {
                            fr.ErrorMsg += "\n Hours Of Operation Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 4:
                        if (hdrValue != "install location")
                        {
                            fr.ErrorMsg += "\n Install Location Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 5:
                        if (hdrValue != "site ready(yes/no)")
                        {
                            fr.ErrorMsg += "\n Site Ready Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 6:
                        if (hdrValue != "erf notes")
                        {
                            fr.ErrorMsg += "\n ERF Notes  Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 7:
                        if (hdrValue != "erf ordertype")
                        {
                            fr.ErrorMsg += "\n ERF OrderType Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 8:
                        if (hdrValue != "shipto branch")
                        {
                            fr.ErrorMsg += "\n ShipTo Branch Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 9:
                        if (hdrValue != "install date")
                        {
                            fr.ErrorMsg += "\n  Install Date Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 10:
                        if (hdrValue != "additional nsv")
                        {
                            fr.ErrorMsg += "\n Additional NSV Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 11:
                        if (hdrValue != "form date")
                        {
                            fr.ErrorMsg += "\n Form Date Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 12:
                        if (hdrValue != "erf received date")
                        {
                            fr.ErrorMsg += "\n ERF Received Date Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 13:
                        if (hdrValue != "erf processed date")
                        {
                            fr.ErrorMsg += "\n ERF Processed Date Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 14:
                        if (hdrValue != "equipment category")
                        {
                            fr.ErrorMsg += "\n Equipment Category  Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 15:
                        if (hdrValue != "equipment brand")
                        {
                            fr.ErrorMsg += "\n Equipment Brand  Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 16:
                        if (hdrValue != "equipment quantity")
                        {
                            fr.ErrorMsg += "\n Equipment Quantity  Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 17:
                        if (hdrValue != "equipment using branch (yes/no)")
                        {
                            fr.ErrorMsg += "\n Equipment Using Branch(YES / NO) Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 18:
                        if (hdrValue != "equipment substitution possible (yes/no)")
                        {
                            fr.ErrorMsg += "\n Equipment Substitution Possible(YES / NO) Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 19:
                        if (hdrValue != "equipment trans type (cash sale/loan/rental)")
                        {
                            fr.ErrorMsg += "\n Equipment Trans Type(CASH SALE / LOAN / RENTAL) Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 20:
                        if (hdrValue != "equipment type (new/refurb)")
                        {
                            fr.ErrorMsg += "\n Equipment Type (NEW / REFURB)  Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 21:
                        if (hdrValue != "expendable category")
                        {
                            fr.ErrorMsg += "\n Expendable Category  Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 22:
                        if (hdrValue != "expendable brand")
                        {
                            fr.ErrorMsg += "\n  Expendable Brand Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 23:
                        if (hdrValue != "expendable quantity")
                        {
                            fr.ErrorMsg += "\n Expendable Quantity Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 24:
                        if (hdrValue != "expendable using branch (yes/no)")
                        {
                            fr.ErrorMsg += "\n Expendable Using Branch(YES / NO) Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 25:
                        if (hdrValue != "expendable substitution possible (yes/no)")
                        {
                            fr.ErrorMsg += "\n Expendable Substitution Possible(YES / NO)  Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 26:
                        if (hdrValue != "expendable trans type (cash sale/loan/rental)")
                        {
                            fr.ErrorMsg += "\n Expendable Trans Type(CASH SALE / LOAN / RENTAL) Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                    case 27:
                        if (hdrValue != "expendable type (new/refurb)")
                        {
                            fr.ErrorMsg += "\n Expendable Type(NEW / REFURB) Column Missing";
                            fr.IsValid = false;
                        }
                        break;
                }
            }

            return fr;
        }

        #endregion
    }
}