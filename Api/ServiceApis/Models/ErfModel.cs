using DataAccess.Db;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ServiceApis.Models
{
    public class ErfModel
    {
        public CustomerModel Customer;
        public ErfAssetsModel ErfAssetsModel;
        public NotesModel Notes;
        public IList<NewNotesModel> NewNotes;

        public string CreatedBy { get; set; }
        public bool ReadOnly { get; set; }

        public bool CrateWorkOrder { get; set; }

        public IList<string> SiteReadyList;
        public string SiteReady { get; set; }

        public IList<OrderType> OrderTypeList;
        public string OrderType { get; set; }

        public IList<ERFBranchDetails> BranchList;
        public string BranchName { get; set; }

        public IList<Contact> ShipToCustomerList;
        public int ShipToCustomer { get; set; }

        public IList<Contingent> EqpValidationList;
        public string EqpValidationId { get; set; }

        public IList<Contingent> ExpValidationList;
        public string ExpValidationId { get; set; }

        public IList<WorkOrderPartModel> WorkOrderParts;

        public decimal TotalNSV { get; set; }
        public decimal CurrentEquipmentTotal { get; set; }
        public decimal AdditionalEquipmentTotal { get; set; }
        public string ApprovalStatus { get; set; }

        public decimal CurrentNSV { get; set; }
        public string ContributionMargin { get; set; }
    }

    public class ErfAssetsModel
    {
        public Erf Erf;
        
        public IList<ERFManagementExpendableModel> ExpendableList;
        public IList<ERFManagementEquipmentModel> EquipmentList;
        //public IList<ERFManagementPOSModel> PosList;
        //public IList<AllFBStatu> Reasons;
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
        public IList<VendorModelModel> ExpOrderTypes;

        //New Fields
        public IList<ErfEqpViewModel> ErfEqpCategory;
        public IList<ErfEqpViewModel> ErfEqpModels;
        public IList<ERFEqpModel> ErfEqpCategoryDetsils;
        public IList<SubstituionModel> UsingBranch;

        public IList<ErfEqpViewModel> ErfExpCategory;
        public IList<ErfEqpViewModel> ErfExpModels;
        public IList<ERFEqpModel> ErfExpCategoryDetsils;

        public IList<ErfEqpViewModel> ErfPosCategory;
        public IList<ErfEqpViewModel> ErfPosModels;
        public IList<ERFEqpModel> ErfPosCategoryDetsils;

        public IList<Contact> ShipToCustomerList;
        public int ShipToCustomer { get; set; }
        public string ShipToCustomerName { get; set; }
        public string ShipToBranchName { get; set; }
    }

    public class NotesModel
    {        public string Notes { get; set; }
        public int? ProjectNumber { get; set; }
        public string CustomerZipCode { get; set; }
        public string CustomerID { get; set; }
        public string ErfID { get; set; }
        //public string FeastMovementID { get; set; }
        public int WorkOrderID { get; set; }
        public string FollowUpRequestID { get; set; }
        public decimal? ProjectFlatRate { get; set; }

        public string WorkOrderStatus { get; set; }
        public bool IsSpecificTechnician { get; set; }
        public string TechID { get; set; }
        public string PreferredProvider { get; set; }

        public string viewProp { get; set; }

        public bool isFromAutoGenerateWorkOrder { get; set; }

        public bool IsAutoDispatched { get; set; }
    }

    public class NewNotesModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class OrderType
    {
        public int OrderTypeId { get; set; }
        public string OrderTypeDesc { get; set; }
        public bool IsActive { get; set; }
    }

    public class WorkOrderPartModel
    {
        public WorkOrderPartModel()
        { }

        public WorkOrderPartModel(WorkorderPart workOrderPart)
        {
            PartsIssueid = workOrderPart.PartsIssueid;
            Quantity = workOrderPart.Quantity;
            if (!string.IsNullOrWhiteSpace(workOrderPart.Manufacturer))
            {
                Manufacturer = workOrderPart.Manufacturer.ToUpper().Trim();
            }
            if (!string.IsNullOrWhiteSpace(workOrderPart.Sku))
            {
                Sku = workOrderPart.Sku.ToUpper().Trim();
            }
            if (!string.IsNullOrWhiteSpace(workOrderPart.Description))
            {
                Description = workOrderPart.Description;
            }
            Issue = workOrderPart.NonSerializedIssue;
        }

        [Key]
        public int? PartsIssueid { get; set; }
        public int? Quantity { get; set; }
        public string Manufacturer { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public bool? Issue { get; set; }
        public decimal skuCost { get; set; }
        public decimal partsTotal { get; set; }
        public bool? PartReplenish { get; set; }
    }

    public class ERFBranchDetails
    {
        public int Id { get; set; }
        public string BranchNo { get; set; }
        public string BranchName { get; set; }

        public string Region { get; set; }
        public string District { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public bool IsActive { get; set; }
    }

    public class VendorModelModel
    {
        public VendorModelModel(string model)
        {
            Model = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.ToUpper().Trim().ToLower());
        }
        public string Model { get; set; }
    }

    public class ExpendableProduct
    {
        public ExpendableProduct(string product)
        {
            Product = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(product.ToUpper().Trim().ToLower());
        }
        public string Product { get; set; }

    }
    public class EquipmentProduct
    {
        public EquipmentProduct(string product)
        {
            Product = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(product.ToUpper().Trim().ToLower());
        }
        public string Product { get; set; }
    }

    public class TransactionTypeModel
    {
        public TransactionTypeModel(string model)
        {
            Model = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.ToUpper().Trim().ToLower());
        }
        public string Model { get; set; }

    }

    public class EquipmentTypeModel
    {
        public EquipmentTypeModel(string model)
        {
            Model = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.ToUpper().Trim().ToLower());
        }
        public string Model { get; set; }
    }

    public class SubstituionModel
    {
        public SubstituionModel(string model)
        {
            Model = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.ToUpper().Trim().ToLower());
        }
        public string Model { get; set; }
    }

    public class ErfEqpViewModel
    {
        public ErfEqpViewModel(int modelId, string modelName)
        {
            ModelName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(modelName.ToUpper().Trim().ToLower());
            ModelId = modelId;
        }
        public string ModelName { get; set; }
        public int ModelId { get; set; }
    }

    public class ERFEqpModel
    {
        public IList<ContingentDetail> ContingentDetails { get; set; }
        public int id { get; set; }
        public int ContingentId { get; set; }
        public decimal LaidInCost { get; set; }
        public decimal CashSale { get; set; }
        public decimal Rental { get; set; }
    }
    public class ERFManagementEquipmentModel
    {
        public ERFManagementEquipmentModel()
        { }

        public ERFManagementEquipmentModel(Fberfequipment equipment)
        {

            ERFEquipmentId = equipment.ErfequipmentId;
            if (!string.IsNullOrEmpty(equipment.Erfid))
            {
                ERFId = Convert.ToInt32(equipment.Erfid);
            }

            WorkOrderId = equipment.WorkOrderId;
            ModelNo = equipment.ModelNo;

            ProdNo = equipment.ProdNo;
            EquipmentType = equipment.EquipmentType;
            UnitPrice = Convert.ToDouble(equipment.UnitPrice);
            TransactionType = equipment.TransactionType;
            Substitution = equipment.Substitution;
            Extra = equipment.Extra;
            Description = equipment.Description;

            Quantity = equipment.Quantity;
            Category = equipment.ContingentCategoryId;
            Brand = equipment.ContingentCategoryTypeId;
            Branch = equipment.UsingBranch;
            LaidInCost = Convert.ToDouble(equipment.LaidInCost);
            RentalCost = Convert.ToDouble(equipment.RentalCost);
            TotalCost = Convert.ToDouble(equipment.TotalCost);

            InternalOrderNumber = equipment.InternalOrderType;
            VendorOrderNumber = equipment.VendorOrderType;

            SerialNumber = equipment.SerialNumber != null ? equipment.SerialNumber : "";
            OrderType = equipment.OrderType != null ? equipment.OrderType : "";
            DepositInvoiceNumber = equipment.DepositInvoiceNumber != null ? equipment.DepositInvoiceNumber : "";
            DepositAmount = equipment.DepositAmount != null ? equipment.DepositAmount.ToString() : "";
            FinalInvoceNumber = equipment.FinalInvoiceNumber != null ? equipment.FinalInvoiceNumber : "";
            InvoiceTotal = equipment.InvoiceTotal != null ? equipment.InvoiceTotal.ToString() : "";

        }


        public int? ERFEquipmentId { get; set; }
        public int? ERFId { get; set; }
        public int? WorkOrderId { get; set; }
        public string ModelNo { get; set; }
        public int? Quantity { get; set; }
        public string ProdNo { get; set; }
        public string EquipmentType { get; set; }
        public double? UnitPrice { get; set; }
        public string TransactionType { get; set; }
        public string Substitution { get; set; }
        public string Extra { get; set; }
        public string Description { get; set; }

        //new Fields
        public int? Category { get; set; }
        public int? Brand { get; set; }
        public string Branch { get; set; }
        public double? LaidInCost { get; set; }
        public double? RentalCost { get; set; }
        public double? TotalCost { get; set; }

        public string InternalOrderNumber { get; set; }
        public string VendorOrderNumber { get; set; }
        public string Tracking { get; set; }

        public string SerialNumber { get; set; }
        public string OrderType { get; set; }
        public string DepositInvoiceNumber { get; set; }
        public string DepositAmount { get; set; }
        public string FinalInvoceNumber { get; set; }
        public string InvoiceTotal { get; set; }
    }

    public class ERFManagementExpendableModel
    {
        public ERFManagementExpendableModel()
        { }

        public ERFManagementExpendableModel(Fberfexpendable expendable)
        {

            ERFExpendableId = expendable.ErfexpendableId;
            if (!string.IsNullOrEmpty(expendable.Erfid))
            {
                ERFId = Convert.ToInt32(expendable.Erfid);
            }

            WorkOrderId = expendable.WorkOrderId;
            ModelNo = expendable.ModelNo;
            Quantity = expendable.Quantity;
            ProdNo = expendable.ProdNo;
            UnitPrice = Convert.ToDouble(expendable.UnitPrice);
            TransactionType = expendable.TransactionType;
            Extra = expendable.Extra;
            Description = expendable.Description;
            Substitution = expendable.Substitution;
            EquipmentType = expendable.EquipmentType;
            Quantity = expendable.Quantity;
            Category = expendable.ContingentCategoryId;
            Brand = expendable.ContingentCategoryTypeId;
            Branch = expendable.UsingBranch;
            LaidInCost = Convert.ToDouble(expendable.LaidInCost);
            RentalCost = Convert.ToDouble(expendable.RentalCost);
            TotalCost = Convert.ToDouble(expendable.TotalCost);

            InternalOrderNumber = expendable.InternalOrderType;
            VendorOrderNumber = expendable.VendorOrderType;
        }




        public int? ERFExpendableId { get; set; }
        public int? ERFId { get; set; }
        public int? WorkOrderId { get; set; }
        public string ModelNo { get; set; }
        public int? Quantity { get; set; }
        public string ProdNo { get; set; }
        public string EquipmentType { get; set; }
        public double? UnitPrice { get; set; }
        public string TransactionType { get; set; }
        public string Substitution { get; set; }
        public string Extra { get; set; }
        public string Description { get; set; }

        //new Fields
        public int? Category { get; set; }
        public int? Brand { get; set; }
        public string Branch { get; set; }
        public double? LaidInCost { get; set; }
        public double? RentalCost { get; set; }
        public double? TotalCost { get; set; }

        public string InternalOrderNumber { get; set; }
        public string VendorOrderNumber { get; set; }
        public string Tracking { get; set; }
    }
}
