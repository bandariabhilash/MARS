using FarmerBrothers.Data;
//using FarmerBrothers.MovementSearchService;
using System.Collections.Generic;

namespace FarmerBrothers.Models
{
    public class ErfAssetsModel
    {
        public Erf Erf;
        public IList<ErfWorkorderLogModel> ErfWorkOrderLogs;
        public IList<ERFManagementExpendableModel> ExpendableList;
        public IList<ERFManagementEquipmentModel> EquipmentList;
        public IList<AllFBStatu> Reasons;
        public string PlacementReason { get; set; }
        public string SelectedWorkOrderId { get; set; }
       
        public IList<VendorModelModel> ErfEquipmentModels;

        public IList<VendorModelModel> ErfExpendableModels;
        public IList<ExpendableProduct> ErfExpendableProducts;
        //public IList<EquipmentProduct> ErfEquipmentProducts;
        public string ErfEquipmentProducts { get; set; }

        public IList<TransactionTypeModel> ErfTransactionTypes;
        public IList<EquipmentTypeModel> ErfEquipmentTypes;
        public IList<SubstituionModel> ErfSubstituion;

        //New Fields
        public IList<ErfEqpViewModel> ErfEqpCategory;
        public IList<ErfEqpViewModel> ErfEqpModels;
        public IList<ERFEqpModel> ErfEqpCategoryDetsils;
        public IList<SubstituionModel> UsingBranch;

        public IList<ErfEqpViewModel> ErfExpCategory;
        public IList<ErfEqpViewModel> ErfExpModels;
        public IList<ERFEqpModel> ErfExpCategoryDetsils;

        public IList<Contact> ShipToCustomerList;
        public int ShipToCustomer { get; set; }
        public string ShipToCustomerName { get; set; }
        public string ShipToBranchName { get; set; }
    }
}