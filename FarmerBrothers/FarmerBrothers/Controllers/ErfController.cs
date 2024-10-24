using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using LinqKit;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace FarmerBrothers.Controllers
{
    public class ErfController : BaseController
    {

        [HttpPost]
        public JsonResult ERFSearch(ErfSearchModel erfSearchModel)
        {
            if (string.IsNullOrWhiteSpace(erfSearchModel.Address)
                && string.IsNullOrWhiteSpace(erfSearchModel.City)
                && string.IsNullOrWhiteSpace(erfSearchModel.CustomerId)
                && string.IsNullOrWhiteSpace(erfSearchModel.CustomerName)
                && string.IsNullOrWhiteSpace(erfSearchModel.ERFID)
                && string.IsNullOrWhiteSpace(erfSearchModel.FeastMovement)
                && string.IsNullOrWhiteSpace(erfSearchModel.Phone)
                && string.IsNullOrWhiteSpace(erfSearchModel.OrderType)
                && string.IsNullOrWhiteSpace(erfSearchModel.WorkOrderID)
                && string.IsNullOrWhiteSpace(erfSearchModel.ZipCode)
                && string.Compare(erfSearchModel.State, "n/a", true) == 0
                && !erfSearchModel.CreatedFrom.HasValue
                && !erfSearchModel.CreatedTo.HasValue)
            {
                TempData["ErfSearchCriteria"] = null;
                return Json(new List<Erf>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<ErfSearchResultModel> erfData = GetErfData(erfSearchModel);

                TempData["ErfSearchCriteria"] = erfSearchModel;
                return Json(erfData, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ErfSearch(int? isBack)
        {
            ErfSearchModel erfSearchModel;
            bool isFromExitErf = false;
            if (TempData["ErfSearchCriteria"] != null && isBack == 1)
            {
                erfSearchModel = TempData["ErfSearchCriteria"] as ErfSearchModel;
                erfSearchModel.SearchResults = GetErfData(erfSearchModel);
                TempData["ErfSearchCriteria"] = erfSearchModel;
                isFromExitErf = true;
            }
            else
            {
                erfSearchModel = new ErfSearchModel();
                erfSearchModel.SearchResults = new List<ErfSearchResultModel>();
                TempData["ErfSearchCriteria"] = null;
            }

            //erfSearchModel.Reasons = FarmerBrothersEntitites.AllFBStatus.Where(p => p.StatusFor == "ERF Reasons" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();
            //AllFBStatu smuckersStatus = new AllFBStatu() { FBStatus = "Please Select", FBStatusID = -1 };
            //erfSearchModel.Reasons.Insert(0, smuckersStatus);

            erfSearchModel.OrderTypeList = FarmerBrothersEntitites.ERFOrderTypes.Where(o => o.IsActive == true && o.OrderType.Length > 0).ToList();
            ERFOrderType ordTyp = new ERFOrderType() {OrderType= "Please Select", OrderTypeId = -1};
            erfSearchModel.OrderTypeList.Insert(0, ordTyp);

            erfSearchModel.CashSalesList = Utility.GetCashSaleStatusList(FarmerBrothersEntitites);
            erfSearchModel.ERFStatusList = ERFStatusModel.GetERFStatusList();

            List<ESMCCMRSMEscalation> esmList = FarmerBrothersEntitites.ESMCCMRSMEscalations.DistinctBy(x => x.ESMName).ToList();
            erfSearchModel.EsmList = new List<ESMCCMRSMEscalation>();
            foreach (ESMCCMRSMEscalation esm in esmList)
            {
                ESMCCMRSMEscalation esmRec = erfSearchModel.EsmList.Where(x => x.ESMName == esm.ESMName).FirstOrDefault();
                if (esmRec == null)
                {
                    erfSearchModel.EsmList.Add(esm);
                }
            }

            if (!isFromExitErf)
            {
                erfSearchModel.Esm = new List<string>();
                erfSearchModel.Esm.Add("");
            }
            return View(erfSearchModel);
        }
        private IList<ErfSearchResultModel> GetErfData1(ErfSearchModel erfSearchModel)
        {
            var predicate = PredicateBuilder.True<Erf>();

            IList<string> erfIds = new List<string>();
            IList<string> userIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(erfSearchModel.FeastMovement))
            {
                erfIds = FarmerBrothersEntitites.FeastMovements.Where(fm => fm.Feastmovementid.ToString().Contains(erfSearchModel.FeastMovement)).Select(fm => fm.Erfid).ToList();
                predicate = predicate.And(e => erfIds.Contains(e.ErfID));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.CustomerId))
            {
                predicate = predicate.And(e => e.CustomerID.ToString().Contains(erfSearchModel.CustomerId));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.CustomerName))
            {
                predicate = predicate.And(w => w.CustomerName.ToString().Contains(erfSearchModel.CustomerName));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.Phone))
            {
                predicate = predicate.And(w => w.CustomerPhone.ToString().Contains(erfSearchModel.Phone));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.Address))
            {
                predicate = predicate.And(w => w.CustomerAddress.ToString().Contains(erfSearchModel.Address));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.City))
            {
                predicate = predicate.And(w => w.CustomerCity.ToString().Contains(erfSearchModel.City));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.State) && string.Compare(erfSearchModel.State, "n/a", true) != 0)
            {
                predicate = predicate.And(w => w.CustomerState.ToString().Contains(erfSearchModel.State));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.ERFID))
            {
                predicate = predicate.And(w => w.ErfID.ToString().Contains(erfSearchModel.ERFID));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.WorkOrderID))
            {
                predicate = predicate.And(w => w.WorkorderID.ToString().Contains(erfSearchModel.WorkOrderID));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.Reason) && string.Compare(erfSearchModel.Reason, "-1", true) != 0)
            {
                predicate = predicate.And(w => w.ReasonID.ToString().Contains(erfSearchModel.Reason));
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.ZipCode))
            {
                predicate = predicate.And(w => w.CustomerZipCode.ToString().Contains(erfSearchModel.ZipCode));
            }

            if (erfSearchModel.CreatedFrom.HasValue)
            {
                predicate = predicate.And(w => w.EntryDate >= erfSearchModel.CreatedFrom);
            }
            if (erfSearchModel.CreatedTo.HasValue)
            {
                predicate = predicate.And(w => w.EntryDate <= erfSearchModel.CreatedTo);
            }

            //if (erfSearchModel.Esm != null && erfSearchModel.Esm.Count > 0)
            //{
            //    if (!string.IsNullOrWhiteSpace(erfSearchModel.Esm[0]))
            //    {
            //        predicate = predicate.And(w => erfSearchModel.Esm.Contains(w.WorkorderCallstatus.ToString()));
            //    }
            //}

            if (erfSearchModel.Esm != null && erfSearchModel.Esm.Count > 0)
            {
                string esm = string.Empty;
                foreach (string s in erfSearchModel.Esm)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        esm += "'" + s + "',";
                    }

                }

                if (!string.IsNullOrEmpty(esm))
                {
                    string query = @"Select ContactId from Contact where ESMName in ( " + esm.TrimEnd(',') + " )";
                    IList<int?> esmsContacts = FarmerBrothersEntitites.Database.SqlQuery<int?>(query).ToList();//.Select(f => f.ContactID).ToList();
                    predicate = predicate.And(w => esmsContacts.Contains(w.CustomerID));
                }
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.OriginatorName))
            {
                userIds = FarmerBrothersEntitites.FbUserMasters.Where(fm => fm.FirstName.ToString().ToLower().Contains(erfSearchModel.OriginatorName.ToLower()) ||
                fm.LastName.ToString().ToLower().Contains(erfSearchModel.OriginatorName.ToLower())).Select(fm => fm.UserId.ToString()).ToList();
                predicate = predicate.And(e => userIds.Contains(e.EntryUserID.ToString()));
            }


            IQueryable<Erf> erfs = FarmerBrothersEntitites.Set<Erf>().AsExpandable().Where(predicate).OrderByDescending(e => e.ErfID).Take(500); ;
            IList<ErfSearchResultModel> searchResults = new List<ErfSearchResultModel>();
            foreach (Erf erf in erfs)
            {
                searchResults.Add(new ErfSearchResultModel(erf));
            }

            return searchResults;
        }

        private IList<ErfSearchResultModel> GetErfData(ErfSearchModel erfSearchModel)
        {
            var predicate = PredicateBuilder.True<Erf>();

            IList<string> erfIds = new List<string>();
            IList<string> userIds = new List<string>();
            StringBuilder erfSearchSelectQuery = new StringBuilder();

            erfSearchSelectQuery.Append(@"Select Top 500 e.ERFId, e.EntryDate, e.CustomerID, e.CustomerName, e.CustomerAddress, e.CustomerCity, e.CustomerState, e.ERFStatus, e.EntryUserId,
                                                                     e.WorkorderID,  e.ApprovalStatus, e.OrderType, c.FirstName, c.LastName, e.CashSaleStatus from ERF e
                                                                    Inner Join Contact c on e.CustomerID = c.ContactID
                                                                    Inner Join FbUserMaster u on e.EntryUserID = u.UserId
                                                                    where 1 = 1");

                if (!string.IsNullOrWhiteSpace(erfSearchModel.CustomerId))
            {
                erfSearchSelectQuery.Append(@" and e.CustomerID like '%"+ erfSearchModel.CustomerId + "%'");
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.CustomerName))
            {
                erfSearchSelectQuery.Append(@" and e.CustomerName like '%" + erfSearchModel.CustomerName + "%'");
            }            

            if (!string.IsNullOrWhiteSpace(erfSearchModel.Address))
            {
                erfSearchSelectQuery.Append(@" and e.CustomerAddress like '%" + erfSearchModel.Address + "%'");
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.City))
            {
                erfSearchSelectQuery.Append(@" and e.CustomerCity like '%" + erfSearchModel.City + "%'");
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.State) && string.Compare(erfSearchModel.State, "n/a", true) != 0 && string.Compare(erfSearchModel.State, "n/a", true) != 0 && string.Compare(erfSearchModel.State, "n/ a", true) != 0)
            {
                erfSearchSelectQuery.Append(@" and e.CustomerState like '%" + erfSearchModel.State + "%'");
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.ERFID))
            {
                erfSearchSelectQuery.Append(@" and e.ErfID like  '%" + erfSearchModel.ERFID + "%'");
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.WorkOrderID))
            {
                erfSearchSelectQuery.Append(@" and e.WorkorderID like  '%" + erfSearchModel.WorkOrderID + "%'");
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.OrderType) && string.Compare(erfSearchModel.OrderType, "-1", true) != 0)
            {
                erfSearchSelectQuery.Append(@" and e.OrderType in (Select OrderType from ERFOrderType where OrderTypeId in (" + erfSearchModel.OrderType + "))");
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.ZipCode))
            {
                erfSearchSelectQuery.Append(@" and e.CustomerZipCode like '%" + erfSearchModel.ZipCode + "%'");

            }

            if (erfSearchModel.CreatedFrom.HasValue)
            {
                erfSearchSelectQuery.Append(@" and e.EntryDate >= '"+ erfSearchModel.CreatedFrom + "'");
            }
            if (erfSearchModel.CreatedTo.HasValue)
            {
                erfSearchSelectQuery.Append(@" and e.EntryDate <= '" + erfSearchModel.CreatedTo + "'");
            }
            if (!string.IsNullOrWhiteSpace(erfSearchModel.CashSaleStatus))
            {
                erfSearchSelectQuery.Append(@" and e.CashSaleStatus like '%" + erfSearchModel.CashSaleStatus + "%'");

            }
            if (!string.IsNullOrWhiteSpace(erfSearchModel.ErfStatus))
            {
                erfSearchSelectQuery.Append(@" and e.erfstatus like '%" + erfSearchModel.ErfStatus + "%'");

            }

            if (erfSearchModel.Esm != null && erfSearchModel.Esm.Count > 0)
            {
                string tz = string.Empty;
                foreach (string s in erfSearchModel.Esm)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        tz += "'" + s + "',";
                    }
                }
                if (!string.IsNullOrEmpty(tz))
                {
                    erfSearchSelectQuery.Append(@" and c.ESMName in (" + tz.TrimEnd(',') + ")");
                }
            }

            if (!string.IsNullOrWhiteSpace(erfSearchModel.OriginatorName))
            {
                erfSearchSelectQuery.Append(@" and(u.FirstName like '%"+ erfSearchModel.OriginatorName + "%' or u.LastName like '%" + erfSearchModel.OriginatorName + "%')");
            }

          //  string finalERFSearchQuery = @" select dbo.getWOElapsedTime(woresults.WorkorderID) ElapsedTime,* from ( " + woSearchSelectQuery.ToString() + " " + woSearchJoinQuery.ToString() + " " + woSearchWhereQuery.ToString() + " ) woresults order by WorkorderEntryDate desc";
            erfSearchSelectQuery.Append(@" order by ErfID desc");

            SqlHelper helper = new SqlHelper();
            DataTable dt = helper.GetDatatable(erfSearchSelectQuery.ToString()); ;

            int resultsCount = dt.Rows.Count;
            IList<ErfSearchResultModel> searchResults = new List<ErfSearchResultModel>();
            foreach (DataRow dr in dt.Rows)
            {
                searchResults.Add(new ErfSearchResultModel(dr, true));
            }
           
            return searchResults;
        }

        private ErfModel BuildErfModel(int erfId)
        {
            ErfModel erfModel = new ErfModel();
            erfModel.ErfAssetsModel = new ErfAssetsModel();
            erfModel.ErfAssetsModel.Erf = FarmerBrothersEntitites.Erfs.Where(e => e.ErfID == erfId.ToString()).FirstOrDefault();
            if (erfModel.ErfAssetsModel != null)
            {
                AllFBStatu allFBStatus = FarmerBrothersEntitites.AllFBStatus.Where(a => a.FBStatusID == erfModel.ErfAssetsModel.Erf.ReasonID).FirstOrDefault();
                if (allFBStatus != null)
                {
                    erfModel.ErfAssetsModel.PlacementReason = allFBStatus.FBStatus;
                }
            }

            Contact customer = FarmerBrothersEntitites.Contacts.Where(a => a.ContactID == erfModel.ErfAssetsModel.Erf.CustomerID).FirstOrDefault();

            if (customer != null)
            {

                erfModel.Customer = new CustomerModel(customer, FarmerBrothersEntitites);
                erfModel.Customer = Utility.PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntitites, erfModel.Customer);
            }
            else
            {
                erfModel.Customer = new CustomerModel();
            }

            //erfModel.Customer.ERFStatusList = new List<CategoryModel>(){                
            //    new CategoryModel("Processed"),
            //    new CategoryModel("Shipped"),
            //    new CategoryModel("Pending"),
            //    new CategoryModel("Complete"),
            //    new CategoryModel("Cancel")};

            erfModel.Customer.ERFStatusList = ERFStatusModel.GetERFStatusList();
                //new List<ERFStatusModel>(){
                // new ERFStatusModel() {StatusId=0, StatusName=" " },
                //new ERFStatusModel() {StatusId=1, StatusName="Processed" },
                //new ERFStatusModel() {StatusId=2, StatusName="Shipped" },
                //new ERFStatusModel() {StatusId=3, StatusName="Pending" },
                //new ERFStatusModel() {StatusId=4, StatusName="Complete" },
                //new ERFStatusModel() {StatusId=5, StatusName="Cancel"},
                //new ERFStatusModel() {StatusId=6, StatusName="Sourcing 3rd Party"}};


            erfModel.Customer.CashSaleStatus = erfModel.ErfAssetsModel.Erf.CashSaleStatus == null ? " " : erfModel.ErfAssetsModel.Erf.CashSaleStatus;
            erfModel.Customer.ERFStatus = erfModel.ErfAssetsModel.Erf.ERFStatus == null ? " " : erfModel.ErfAssetsModel.Erf.ERFStatus;
            

            erfModel.Customer.MainContactName = erfModel.ErfAssetsModel.Erf.CustomerMainContactName == null ? "" : erfModel.ErfAssetsModel.Erf.CustomerMainContactName.ToString();
            erfModel.Customer.PhoneNumber = erfModel.ErfAssetsModel.Erf.CustomerPhone == null ? "" : erfModel.ErfAssetsModel.Erf.CustomerPhone.ToString();


            List<Contingent> erfCategory = FarmerBrothersEntitites.Contingents.Where(e => e.ContingentType.ToLower() == "eqp" && e.IsActive == true).ToList();//.Select(s => s.ModelNO).Distinct();
            erfModel.ErfAssetsModel.ErfEqpCategory = new List<ErfEqpViewModel>();
            foreach (Contingent model in erfCategory)
            {
                erfModel.ErfAssetsModel.ErfEqpCategory.Add(new ErfEqpViewModel(model.ContingentID, model.ContingentName));
            }
            erfModel.ErfAssetsModel.ErfEqpCategory.Insert(0, new ErfEqpViewModel(0, ""));


            //============

            List<ContingentDetail> erfeqpModels = FarmerBrothersEntitites.ContingentDetails.Where(c => c.IsActive == true).ToList();//.Select(s => s.ModelNO).Distinct();
            erfModel.ErfAssetsModel.ErfEqpModels = new List<ErfEqpViewModel>();
            erfModel.ErfAssetsModel.ErfExpModels = new List<ErfEqpViewModel>();
            erfModel.ErfAssetsModel.ErfPosModels = new List<ErfEqpViewModel>();
            foreach (ContingentDetail model in erfeqpModels)
            {
                erfModel.ErfAssetsModel.ErfEqpModels.Add(new ErfEqpViewModel(model.ID, model.Name));
                erfModel.ErfAssetsModel.ErfExpModels.Add(new ErfEqpViewModel(model.ID, model.Name));
                erfModel.ErfAssetsModel.ErfPosModels.Add(new ErfEqpViewModel(model.ID, model.Name));
            }
            erfModel.ErfAssetsModel.ErfEqpModels.Insert(0, new ErfEqpViewModel(0, ""));
            erfModel.ErfAssetsModel.ErfExpModels.Insert(0, new ErfEqpViewModel(0, ""));
            erfModel.ErfAssetsModel.ErfPosModels.Insert(0, new ErfEqpViewModel(0, ""));
            //============
            List<Contingent> erfExpCategory = FarmerBrothersEntitites.Contingents.Where(e => e.ContingentType.ToLower() == "exp" && e.IsActive == true).ToList();//.Select(s => s.ModelNO).Distinct();
            erfModel.ErfAssetsModel.ErfExpCategory = new List<ErfEqpViewModel>();
            foreach (Contingent model in erfExpCategory)
            {
                erfModel.ErfAssetsModel.ErfExpCategory.Add(new ErfEqpViewModel(model.ContingentID, model.ContingentName));
            }
            erfModel.ErfAssetsModel.ErfExpCategory.Insert(0, new ErfEqpViewModel(0, ""));

            List<Contingent> erfPosCategory = FarmerBrothersEntitites.Contingents.Where(e => e.ContingentType.ToLower() == "pos" && e.IsActive == true).ToList();
            erfModel.ErfAssetsModel.ErfPosCategory = new List<ErfEqpViewModel>();
            foreach (Contingent model in erfPosCategory)
            {
                erfModel.ErfAssetsModel.ErfPosCategory.Add(new ErfEqpViewModel(model.ContingentID, model.ContingentName));
            }
            erfModel.ErfAssetsModel.ErfPosCategory.Insert(0, new ErfEqpViewModel(0, ""));


            erfModel.Customer.WorkOrderId = "1";
            int? localerfid = Convert.ToInt32(erfModel.ErfAssetsModel.Erf.ErfID);
            erfModel.ErfAssetsModel.EquipmentList = new List<ERFManagementEquipmentModel>();
            erfModel.ErfAssetsModel.ExpendableList = new List<ERFManagementExpendableModel>();
            erfModel.ErfAssetsModel.PosList = new List<ERFManagementPOSModel>();
            var equipmentItems = FarmerBrothersEntitites.FBERFEquipments.Where(x => x.ERFId == erfModel.ErfAssetsModel.Erf.ErfID).ToList();
            foreach (FBERFEquipment equpItem in equipmentItems)
            {
                ERFManagementEquipmentModel eqmodle = new ERFManagementEquipmentModel(equpItem);
                eqmodle.Tracking = erfModel.ErfAssetsModel.Erf.Tracking;
                erfModel.ErfAssetsModel.EquipmentList.Add(eqmodle);
            }
            TempData["Equipment"] = erfModel.ErfAssetsModel.EquipmentList;

            var expendableItems = FarmerBrothersEntitites.FBERFExpendables.Where(x => x.ERFId == erfModel.ErfAssetsModel.Erf.ErfID).ToList();
            foreach (FBERFExpendable expItems in expendableItems)
            {
                ERFManagementExpendableModel exmodle = new ERFManagementExpendableModel(expItems);
                exmodle.Tracking = erfModel.ErfAssetsModel.Erf.Tracking;
                erfModel.ErfAssetsModel.ExpendableList.Add(exmodle);
            }
            TempData["Expendable"] = erfModel.ErfAssetsModel.ExpendableList;

            var posItems = FarmerBrothersEntitites.FBERFPos.Where(x => x.ERFId == erfModel.ErfAssetsModel.Erf.ErfID).ToList();
            foreach (FBERFPos posItem in posItems)
            {
                ERFManagementPOSModel posmodle = new ERFManagementPOSModel(posItem);
                erfModel.ErfAssetsModel.PosList.Add(posmodle);
            }
            TempData["Pos"] = erfModel.ErfAssetsModel.PosList;

            IQueryable<ErfWorkorderLog> workOrderLogs = FarmerBrothersEntitites.ErfWorkorderLogs.Where(ew => ew.ErfID == erfModel.ErfAssetsModel.Erf.ErfID);
            erfModel.ErfAssetsModel.ErfWorkOrderLogs = new List<ErfWorkorderLogModel>();
            foreach (ErfWorkorderLog workOrderLog in workOrderLogs)
            {
                ErfWorkorderLogModel workOrderLogModel = new ErfWorkorderLogModel()
                {
                    WorkorderID = workOrderLog.WorkorderID.ToString(),
                    ErfID = workOrderLog.ErfID,
                    CustomerID = erfModel.ErfAssetsModel.Erf.CustomerID.ToString()
                };
                erfModel.ErfAssetsModel.ErfWorkOrderLogs.Add(workOrderLogModel);
            }

            erfModel.Notes = new NotesModel()
            {
                ErfID = erfModel.ErfAssetsModel.Erf.ErfID
            };

            if (erfModel.Customer != null)
            {
                erfModel.Notes.CustomerZipCode = erfModel.Customer.ZipCode;
            }

            // IQueryable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.ErfID == erfModel.ErfAssetsModel.Erf.ErfID && nh.AutomaticNotes == 0).OrderByDescending(nh => nh.EntryDate);
            IQueryable<NotesHistory> notesHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.ErfID == erfModel.ErfAssetsModel.Erf.ErfID).OrderByDescending(nh => nh.EntryDate);
            erfModel.Notes.NotesHistory = new List<NotesHistoryModel>();
            foreach (NotesHistory notesHistory in notesHistories)
            {
                erfModel.Notes.NotesHistory.Add(new NotesHistoryModel(notesHistory));
            }

            IQueryable<NotesHistory> recordHistories = FarmerBrothersEntitites.NotesHistories.Where(nh => nh.ErfID == erfModel.ErfAssetsModel.Erf.ErfID && nh.AutomaticNotes == 1).OrderByDescending(nh => nh.EntryDate);
            erfModel.Notes.RecordHistory = new List<NotesHistoryModel>();
            foreach (NotesHistory recordHistory in recordHistories)
            {
                erfModel.Notes.RecordHistory.Add(new NotesHistoryModel(recordHistory));
            }

            erfModel.Notes.CustomerNotesResults = new List<CustomerNotesModel>();
            //int? custId = Convert.ToInt32(erfModel.Customer.CustomerId);
            //var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();
            int custId = Convert.ToInt32(erfModel.Customer.CustomerId);
            int parentId = string.IsNullOrEmpty(erfModel.Customer.ParentNumber) ? 0 : Convert.ToInt32(erfModel.Customer.ParentNumber);
            var custNotes = Utility.GetCustomreNotes(custId, parentId, FarmerBrothersEntitites);
            foreach (var dbCustNotes in custNotes)
            {
                erfModel.Notes.CustomerNotesResults.Add(new CustomerNotesModel(dbCustNotes));
            }

            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == erfModel.ErfAssetsModel.Erf.WorkorderID).FirstOrDefault();
            if (workOrder != null)
            {
                erfModel.Notes.WorkOrderID = workOrder.WorkorderID;
                if (workOrder.ProjectID.HasValue)
                {
                    erfModel.Notes.ProjectNumber = workOrder.ProjectID.Value;
                }
            }

            if (string.IsNullOrEmpty(erfModel.ErfAssetsModel.Erf.ErfID))
            {
                erfModel.CreatedBy = UserName;
            }
            else
            {
                int userId = Convert.ToInt32(erfModel.ErfAssetsModel.Erf.EntryUserID);
                FbUserMaster fbu = FarmerBrothersEntitites.FbUserMasters.Where(u => u.UserId == userId).FirstOrDefault();
                if(fbu != null)
                {
                    erfModel.CreatedBy = fbu.FirstName + " " + fbu.LastName;
                }
            }

            IQueryable<string> models = FarmerBrothersEntitites.FBEquipments.Select(s => s.ModelNO).Distinct();
            erfModel.ErfAssetsModel.ErfEquipmentModels = new List<VendorModelModel>();
            foreach (string model in models)
            {
                erfModel.ErfAssetsModel.ErfEquipmentModels.Add(new VendorModelModel(model));
            }
            erfModel.ErfAssetsModel.ErfEquipmentModels.OrderBy(v => v.Model).ToList();

            IQueryable<string> expmodels = FarmerBrothersEntitites.FBExpendables.Select(s => s.ModelNO).Distinct();
            erfModel.ErfAssetsModel.ErfExpendableModels = new List<VendorModelModel>();
            foreach (string model in expmodels)
            {
                erfModel.ErfAssetsModel.ErfExpendableModels.Add(new VendorModelModel(model));
            }
            erfModel.ErfAssetsModel.ErfExpendableModels.OrderBy(v => v.Model).ToList();

            erfModel.ErfAssetsModel.ExpOrderTypes = new List<VendorModelModel>();
            erfModel.ErfAssetsModel.ExpOrderTypes.Add(new VendorModelModel("SE"));
            erfModel.ErfAssetsModel.ExpOrderTypes.Add(new VendorModelModel("SR"));
            erfModel.ErfAssetsModel.ExpOrderTypes.Add(new VendorModelModel("CE"));
            erfModel.ErfAssetsModel.ExpOrderTypes.Add(new VendorModelModel("CR"));           
            erfModel.ErfAssetsModel.ExpOrderTypes.OrderBy(v => v.Model).ToList();


            string eqproductNum = FarmerBrothersEntitites.FBEquipments.Select(s => s.ProdNo).FirstOrDefault();
            erfModel.ErfAssetsModel.ErfEquipmentProducts = string.Empty;
            if (!string.IsNullOrEmpty(eqproductNum))
            {
                erfModel.ErfAssetsModel.ErfEquipmentProducts = eqproductNum;
            }

            
            IQueryable<string> productNum = FarmerBrothersEntitites.FBExpendables.Select(s => s.ProdNo).Distinct();
            erfModel.ErfAssetsModel.ErfExpendableProducts = new List<ExpendableProduct>();
            foreach (string type in productNum)
            {
                erfModel.ErfAssetsModel.ErfExpendableProducts.Add(new ExpendableProduct(type));
            }

            ExpendableProduct productType = new ExpendableProduct("");
            erfModel.ErfAssetsModel.ErfExpendableProducts.Insert(0, productType);





            IQueryable<string> transType = FarmerBrothersEntitites.FBERFTransactionTypes.Where(t => t.IsActive == true).OrderBy(t => t.IsActive).Select(s => s.Type).Distinct();
            erfModel.ErfAssetsModel.ErfTransactionTypes = new List<TransactionTypeModel>();
            foreach (string type in transType)
            {
                erfModel.ErfAssetsModel.ErfTransactionTypes.Add(new TransactionTypeModel(type));
            }

            TransactionTypeModel eqtransType = new TransactionTypeModel("");
            erfModel.ErfAssetsModel.ErfTransactionTypes.Insert(0, eqtransType);

            List<string> UsingBranchType = new List<string>() { "", "NO", "YES" };
            erfModel.ErfAssetsModel.UsingBranch = new List<SubstituionModel>();
            foreach (string useBrnch in UsingBranchType)
            {
                erfModel.ErfAssetsModel.UsingBranch.Add(new SubstituionModel(useBrnch));
            }

            List<string> SubstituionType = new List<string>() { "", "NO", "YES" };
            erfModel.ErfAssetsModel.ErfSubstituion = new List<SubstituionModel>();
            foreach (string substituion in SubstituionType)
            {
                erfModel.ErfAssetsModel.ErfSubstituion.Add(new SubstituionModel(substituion));
            }

            List<string> eqpType = new List<string>() { "", "NEW", "REFURB" };
            erfModel.ErfAssetsModel.ErfEquipmentTypes = new List<EquipmentTypeModel>();
            foreach (string eqp in eqpType)
            {
                erfModel.ErfAssetsModel.ErfEquipmentTypes.Add(new EquipmentTypeModel(eqp));
            }

            erfModel.ShipToCustomerList = FarmerBrothersEntitites.Contacts.Where(c => c.SearchType == "C").ToList();
            erfModel.ShipToCustomer = erfModel.ErfAssetsModel.Erf.ShipToJDE == null ? 0 : Convert.ToInt32(erfModel.ErfAssetsModel.Erf.ShipToJDE);
            if (erfModel.ErfAssetsModel.Erf.ShipToBranch != null)
            {
                ERFBranchDetail erfBrnchDtls = FarmerBrothersEntitites.ERFBranchDetails.Where(e => e.Branch == erfModel.ErfAssetsModel.Erf.ShipToBranch).FirstOrDefault();
                if (erfBrnchDtls != null)
                {
                    erfModel.ErfAssetsModel.ShipToBranchName = erfBrnchDtls.BranchName;
                }
            }

            Contact shipTocon = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == erfModel.ErfAssetsModel.Erf.ShipToJDE).FirstOrDefault();
            if (shipTocon != null)
            {
                erfModel.ErfAssetsModel.ShipToCustomerName = string.IsNullOrEmpty(shipTocon.CompanyName) ? "" : shipTocon.CompanyName;
            }

            return erfModel;
        }

        public ActionResult ErfDetails(int id)
        {
            ErfModel erfModel = BuildErfModel(id);
            int userId = Convert.ToInt32(erfModel.ErfAssetsModel.Erf.EntryUserID.ToString());
            FbUserMaster userProfile = FarmerBrothersEntitites.FbUserMasters.Where(u => u.UserId == userId).FirstOrDefault();
            if (userProfile != null)
            {
                erfModel.CreatedBy = userProfile.FirstName;
            }

            return View(BuildErfModel(id));
        }



        [HttpGet]
        public ActionResult ERFManagement(int? customerId, int? erfId)
        {
            ErfModel erfManagementModel = ConstructERFManagementModel(customerId, erfId);

            return View(erfManagementModel);
        }
        ErfModel ConstructERFManagementModel(int? customerId, int? erfId)
        {
            ErfModel erfManagementModel = new ErfModel();
            erfManagementModel.WorkOrderParts = new List<WorkOrderPartModel>();

            erfManagementModel.ErfAssetsModel = new ErfAssetsModel();
            erfManagementModel.ErfAssetsModel.Erf = new Erf();

            erfManagementModel.ErfAssetsModel.Erf.DateOnERF = DateTime.UtcNow;
            erfManagementModel.ErfAssetsModel.Erf.DateERFReceived = DateTime.UtcNow;
            erfManagementModel.ErfAssetsModel.Erf.DateERFProcessed = DateTime.UtcNow;
            erfManagementModel.ErfAssetsModel.Erf.OriginalRequestedDate = DateTime.UtcNow;

            erfManagementModel.Customer = new CustomerModel();
            int cid = Convert.ToInt32(customerId);
            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == cid).FirstOrDefault();
            erfManagementModel.Customer = new CustomerModel(customer, FarmerBrothersEntitites);
            erfManagementModel.Customer.ErfId = "0";

            erfManagementModel.SiteReadyList = new List<string>();
            erfManagementModel.SiteReadyList.Add("");
            erfManagementModel.SiteReadyList.Add("YES");
            erfManagementModel.SiteReadyList.Add("NO");
            
            IQueryable<string> models = FarmerBrothersEntitites.FBEquipments.Select(s => s.ModelNO).Distinct();
            erfManagementModel.ErfAssetsModel.ErfEquipmentModels = new List<VendorModelModel>();
            foreach (string model in models)
            {
                erfManagementModel.ErfAssetsModel.ErfEquipmentModels.Add(new VendorModelModel(model));
            }
            erfManagementModel.ErfAssetsModel.ErfEquipmentModels.OrderBy(v => v.Model).ToList();

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

            erfManagementModel.Customer.NonFBCustomerList = Utility.GetNonFBCustomers(FarmerBrothersEntitites, false);

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


            return erfManagementModel;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "UpdateCustomer")]
        public JsonResult UpdateCustomer([ModelBinder(typeof(CustomerModelBinder))]CustomerModel customer)
        {
            var CustomerId = Convert.ToInt32(customer.CustomerId);
            var contact = FarmerBrothersEntitites.Contacts.Find(CustomerId);
            CustomerModel oldCustomer = new CustomerModel();
            oldCustomer.CustomerSpecialInstructions = contact.CustomerSpecialInstructions;
            oldCustomer.AreaCode = contact.AreaCode;
            oldCustomer.PhoneNumber = contact.Phone;
            oldCustomer.MainContactName = contact.FirstName + " " + contact.LastName;
            oldCustomer.CustomerName = contact.CompanyName;
            oldCustomer.Address = contact.Address1;
            oldCustomer.Address2 = contact.Address2;
            oldCustomer.City = contact.City;
            oldCustomer.State = contact.State;
            oldCustomer.ZipCode = contact.PostalCode;
            oldCustomer.DistributorName = contact.DistributorName;
            oldCustomer.MainEmailAddress = contact.Email;
            oldCustomer.CustomerId = contact.ContactID.ToString();

            contact.AreaCode = customer.AreaCode;
            contact.Phone = customer.PhoneNumber;
            if (customer.PhoneNumber != null)
            {
                contact.PhoneWithAreaCode = customer.PhoneNumber.Replace("(", "").Replace(")", "").Replace("-", "");
            }

            if (!string.IsNullOrWhiteSpace(customer.MainContactName))
            {
                var names = customer.MainContactName.Trim().Split(' ');
                if (names.Length >= 2)
                {
                    contact.FirstName = names[0];
                    contact.LastName = string.Empty;
                    for (int ind = 1; ind < names.Length; ind++)
                    {
                        contact.LastName += " " + names[ind];
                    }

                }
                else
                {
                    contact.FirstName = names[0];
                    contact.LastName = string.Empty;
                }
            }
            contact.CompanyName = customer.CustomerName;
            contact.Address1 = customer.Address;
            contact.Address2 = customer.Address2;
            contact.City = customer.City;
            contact.State = customer.State;
            contact.PostalCode = customer.ZipCode;
            contact.DistributorName = customer.DistributorName;
            contact.Email = customer.MainEmailAddress;
            contact.CustomerSpecialInstructions = customer.CustomerSpecialInstructions;

            CustomerController custcontrl = new CustomerController();
            if (custcontrl.ValidateZipCode(customer.ZipCode))
            {
                FarmerBrothersEntitites.SaveChanges();
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("ERFManagement", "ERF", new { customerId = customer.CustomerId });
                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = 0, Url = redirectUrl, data = custcontrl.SendCustomerDetailsUpdateMail(customer, oldCustomer, Server.MapPath("~/img/mainlogo.jpg")) };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            else
            {
                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = 1, data = 0 };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }

        }

        public JsonResult ERFStatusSave(string Status, int CustomerID, string ERFId)
        {
            string message = "";
            Erf erf = FarmerBrothersEntitites.Erfs.Where(er => er.ErfID == ERFId).FirstOrDefault();

            var redirectUrl = string.Empty;
            if (Request != null)
            {
                redirectUrl = new UrlHelper(Request.RequestContext).Action("ErfDetails", "ERF", new { Id = ERFId });
            }

            if (erf != null && !string.IsNullOrEmpty(Status))
            {
                string tempStatus = erf.ERFStatus;

                erf.ERFStatus = Status;

                Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == CustomerID).FirstOrDefault();
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

                DateTime CurrentTime = Utility.GetCurrentTime(erf.CustomerZipCode, FarmerBrothersEntitites);
                int esmId = Convert.ToInt32(ESMId);
                //ESMDSMRSM esmdsmrsmView = FarmerBrothersEntitites.ESMDSMRSMs.FirstOrDefault(x => x.EDSMID == esmId);
                ESMCCMRSMEscalation esmdsmrsmView = FarmerBrothersEntitites.ESMCCMRSMEscalations.FirstOrDefault(x => x.EDSMID == esmId);
                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = CurrentTime,
                    Notes = "[ERF]:  Status Updated from " + tempStatus + " to " + Status,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = esmdsmrsmView == null ? "1234" : esmdsmrsmView.ESMName,
                    ErfID = erf.ErfID,
                    WorkorderID = erf.WorkorderID,
                    isDispatchNotes = 1
                };
                FarmerBrothersEntitites.NotesHistories.Add(notesHistory);



                int returnValue = FarmerBrothersEntitites.SaveChanges();

                if (Status.ToLower() == "cancel")
                {
                    ERFNewController enc = new ERFNewController();
                    enc.ERFEmail(erf.ErfID, erf.WorkorderID, false, erf.ApprovalStatus, true);
                }

                

                message = "| ERF Status Update Success !";

                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = 1, message = message, Url = redirectUrl };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            else
            {
                message = "| Error Updating Erf Status";

                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = 0, message = message, Url = redirectUrl };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
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
            string uPrice;
            if (value.UnitPrice == null && value.ProdNo != null)
            {
                uPrice = FarmerBrothersEntitites.FBEquipments.Where(e => e.ProdNo == value.ProdNo).Select(e => e.UnitPrice).FirstOrDefault();
            }
            else
            {
                uPrice = value.UnitPrice.ToString();
            }
            if (TempData["ERFEquipmentId"] != null)
            {
                int eqpId = Convert.ToInt32(TempData["ERFEquipmentId"]);
                value.ERFEquipmentId = eqpId + 1;
                TempData["ERFEquipmentId"] = eqpId + 1;
            }
            else
            {
                value.ERFEquipmentId = 1;
                value.UnitPrice = Convert.ToDouble(uPrice);
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
            string uPrice;
            if (value.UnitPrice == null && value.ProdNo != null)
            {
                uPrice = FarmerBrothersEntitites.FBExpendables.Where(e => e.ProdNo == value.ProdNo).Select(e => e.UnitPrice).FirstOrDefault();
            }
            else
            {
                uPrice = value.UnitPrice.ToString();
            }

            if (TempData["ERFExpendableId"] != null)
            {
                int assetId = Convert.ToInt32(TempData["ERFExpendableId"]);
                value.ERFExpendableId = assetId + 1;
                TempData["ERFExpendableId"] = assetId + 1;
            }
            else
            {
                value.ERFExpendableId = 1;
                value.UnitPrice = Convert.ToDouble(uPrice);
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

        #region Create ERF
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

                        //ERFNewController ef = new ERFNewController();
                        //ef.ERFEmail(erfModel.ErfAssetsModel.Erf.ErfID, 0, false, "approved for processing");

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
                                    erf.CashSaleStatus = erfModel.Customer.CashSaleStatus;
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
                            List<FBERFPos> posList = FarmerBrothersEntitites.FBERFPos.Where(e => e.ERFId == erfModel.ErfAssetsModel.Erf.ErfID).ToList();
                            foreach (FBERFPos item in posList)
                            {
                                FarmerBrothersEntitites.FBERFPos.Remove(item);
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
                                        InternalOrderType = equipment.InternalOrderNumber,
                                        VendorOrderType = equipment.VendorOrderNumber,
                                        ContingentCategoryId = equipment.Category,
                                        ContingentCategoryTypeId = equipment.Brand,
                                        LaidInCost = Convert.ToDecimal(equipment.LaidInCost),
                                        RentalCost = Convert.ToDecimal(equipment.RentalCost),
                                        TotalCost = Convert.ToDecimal(equipment.TotalCost),
                                        UsingBranch = equipment.Branch,
                                        SerialNumber = equipment.SerialNumber,
                                        OrderType = equipment.OrderType,
                                        DepositInvoiceNumber = equipment.DepositInvoiceNumber,
                                        DepositAmount = string.IsNullOrEmpty(equipment.DepositAmount) ? 0 : Convert.ToDecimal(equipment.DepositAmount),
                                        FinalInvoiceNumber = equipment.FinalInvoceNumber,
                                        InvoiceTotal = string.IsNullOrEmpty(equipment.InvoiceTotal) ? 0 : Convert.ToDecimal(equipment.InvoiceTotal)
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
                                        InternalOrderType = expItems.InternalOrderNumber,
                                        VendorOrderType = expItems.VendorOrderNumber,
                                        ContingentCategoryId = expItems.Category,
                                        ContingentCategoryTypeId = expItems.Brand,
                                        LaidInCost = Convert.ToDecimal(expItems.LaidInCost),
                                        RentalCost = Convert.ToDecimal(expItems.RentalCost),
                                        TotalCost = Convert.ToDecimal(expItems.TotalCost),
                                        UsingBranch = expItems.Branch,
                                        Substitution = expItems.Substitution,
                                        EquipmentType = expItems.EquipmentType

                                    };
                                    FarmerBrothersEntitites.FBERFExpendables.Add(eq);
                                }
                            }

                            if (erfModel.ErfAssetsModel.PosList != null)
                            {
                                foreach (ERFManagementPOSModel posItems in erfModel.ErfAssetsModel.PosList)
                                {
                                    FBERFPos pos = new FBERFPos()
                                    {
                                        ERFId = erf.ErfID,
                                        WorkOrderId = erf.WorkorderID,
                                        ModelNo = posItems.ModelNo,
                                        Quantity = posItems.Quantity,
                                        ProdNo = posItems.ProdNo,
                                        UnitPrice = Convert.ToDecimal(posItems.UnitPrice),
                                        TransactionType = posItems.TransactionType,
                                        Extra = posItems.Extra,
                                        Description = posItems.Description,
                                        ContingentCategoryId = posItems.Category,
                                        ContingentCategoryTypeId = posItems.Brand,
                                        LaidInCost = Convert.ToDecimal(posItems.LaidInCost),
                                        RentalCost = Convert.ToDecimal(posItems.RentalCost),
                                        TotalCost = Convert.ToDecimal(posItems.TotalCost),
                                        UsingBranch = posItems.Branch,
                                        Substitution = posItems.Substitution,
                                        EquipmentType = posItems.EquipmentType,
                                        InternalOrderType = posItems.InternalOrderNumber,
                                        VendorOrderType = posItems.InternalOrderNumber

                                    };
                                    FarmerBrothersEntitites.FBERFPos.Add(pos);
                                }
                            }

                            int WOId = erf.WorkorderID == null ? 0 : Convert.ToInt32(erf.WorkorderID);

                            SaveNotes(erfModel, WOId);
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
                    redirectUrl = new UrlHelper(Request.RequestContext).Action("ErfSearch", "ERF", new { isBack = 0 });
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

        private int ERFSave(ErfModel erfModel, out Erf erf, out string message)
        {
            int returnValue = 0;
            message = string.Empty;
            erf = null;

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

                erf.SiteReady = erfModel.ErfAssetsModel.Erf.SiteReady;
                erf.CashSaleStatus = erfModel.Customer.CashSaleStatus;

            }
            FarmerBrothersEntitites.Erfs.Add(erf);

            int WOId = erf.WorkorderID == null ? 0 : Convert.ToInt32(erf.WorkorderID);
            SaveNotes(erfModel, WOId);
                        
            DateTime CurrentTime = Utility.GetCurrentTime(erfModel.Customer.ZipCode, FarmerBrothersEntitites);
            int effectedRecords = 0;
            try
            {
                JsonResult jsonResult = new JsonResult();
                if (erfModel.CrateWorkOrder)
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
                    WorkorderController wc = new WorkorderController();


                    jsonResult = wc.SaveWorkOrder(workorderModel, null, string.Empty);
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
                        Description = equipment.Description

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
                        Substitution = expItems.Substitution,
                        EquipmentType = expItems.EquipmentType

                    };
                    FarmerBrothersEntitites.FBERFExpendables.Add(eq);
                }
            }

            if (erfModel.ErfAssetsModel.PosList != null)
            {
                foreach (ERFManagementPOSModel posItems in erfModel.ErfAssetsModel.PosList)
                {
                    FBERFPos pos = new FBERFPos()
                    {
                        ERFId = erf.ErfID,
                        WorkOrderId = erf.WorkorderID,
                        ModelNo = posItems.ModelNo,
                        Quantity = posItems.Quantity,
                        ProdNo = posItems.ProdNo,
                        UnitPrice = Convert.ToDecimal(posItems.UnitPrice),
                        TransactionType = posItems.TransactionType,
                        Extra = posItems.Extra,
                        Description = posItems.Description,
                        Substitution = posItems.Substitution,
                        EquipmentType = posItems.EquipmentType

                    };
                    FarmerBrothersEntitites.FBERFPos.Add(pos);
                }
            }
            effectedRecords = FarmerBrothersEntitites.SaveChanges();

            //if(erfModel.CrateWorkOrder)
            //{
            //    WorkOrder WO = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == erfModel.)

            //    WorkorderController wc = new WorkorderController();
            //    wc.StartAutoDispatchProcess(erfModel.wo);
            //}

            returnValue = effectedRecords > 0 ? 1 : 0;
            return returnValue;
        }
        #endregion

        #region Save Note

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

        #endregion

        #region Save Notes

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
                        WorkorderID = erfWorkorderId == 0 ? null : (int?)erfWorkorderId,
                        isDispatchNotes = 0
                    };
                    FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                }
            }
        }

        #endregion

        #region Load Models ProductNumber Descrption

        public JsonResult GetProductNumberByModel(string model)
        {
            FBEquipment Equipment = null;

            if (string.IsNullOrWhiteSpace(model))
            {
                Equipment = FarmerBrothersEntitites.FBEquipments.OrderBy(s => s.ProdNo).FirstOrDefault();
            }
            else
            {
                Equipment = FarmerBrothersEntitites.FBEquipments.Where(s => s.ModelNO == model).OrderBy(s => s.ProdNo).FirstOrDefault();
            }

            var data = new List<object>();

            data.Add(new { value = Equipment.ProdNo.ToUpper().Trim(), text = Equipment.ProdNo.ToUpper().Trim() });
            data.Add(new { value = Equipment.Description.ToUpper().Trim(), text = Equipment.Description.ToUpper().Trim() });
            data.Add(new { value = Equipment.UnitPrice.ToUpper().Trim(), text = Equipment.UnitPrice.ToUpper().Trim() });



            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetExpendableProductNumberByModel(string model)
        {
            IQueryable<string> productNumber = null;

            if (string.IsNullOrWhiteSpace(model))
            {
                productNumber = FarmerBrothersEntitites.FBExpendables.OrderBy(s => s.ProdNo).Select(s => s.ProdNo).Distinct();
            }
            else
            {
                productNumber = FarmerBrothersEntitites.FBExpendables.Where(s => s.ModelNO == model).OrderBy(s => s.ProdNo).Select(s => s.ProdNo).Distinct();
            }

            var data = new List<object>();
            foreach (string prod in productNumber)
            {
                data.Add(new { value = prod.ToUpper().Trim(), text = prod.ToUpper().Trim() });
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = data };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetProductNoDescription(string modValue)
        {
            string prdDescription = string.Empty;
            FBEquipment prd = FarmerBrothersEntitites.FBEquipments.Where(s => s.ModelNO == modValue).FirstOrDefault();
            if (prd != null)
            {
                prdDescription = prd.Description;
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = prdDescription };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetExpendableProductNoDescription(string prdValue)
        {
            string prdDescription = string.Empty;
            FBExpendable prd = FarmerBrothersEntitites.FBExpendables.Where(s => s.ProdNo == prdValue).FirstOrDefault();
            if (prd != null)
            {
                prdDescription = prd.Description;
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = prdDescription };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetProductNoUnitPrice(string modValue)
        {
            string unitprice = string.Empty;
            FBEquipment prd = FarmerBrothersEntitites.FBEquipments.Where(s => s.ModelNO == modValue).FirstOrDefault();
            if (prd != null)
            {
                unitprice = prd.UnitPrice;
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = unitprice };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetExpendableProductNoUnitPrice(string prdValue)
        {
            string unitprice = string.Empty;
            FBExpendable prd = FarmerBrothersEntitites.FBExpendables.Where(s => s.ProdNo == prdValue).FirstOrDefault();
            if (prd != null)
            {
                unitprice = prd.UnitPrice;
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = unitprice };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        #endregion


        #region Excel Export
        [HttpPost]
        public void ExcelExport()
        {
            ErfSearchModel erfSearchModel = new ErfSearchModel();
            if (TempData["ErfSearchCriteria"] != null)
            {
                erfSearchModel = TempData["ErfSearchCriteria"] as ErfSearchModel;
            }

            IList<ErfSearchResultModel> erfData = GetErfData(erfSearchModel);
            string gridModel = HttpContext.Request.Params["GridModel"];
            GridProperties gridProperty = ConvertGridObject(gridModel);
            ExcelExport exp = new ExcelExport();
            exp.Export(gridProperty, erfData, "ErfResults.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }
        #endregion


    }
}