using FarmerBrothers.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace FarmerBrothers.Models
{
    public class WorkOrderResults
    {
        public bool success { get; set; }
        public string Url { get; set; }
        public int? WorkOrderId { get; set; }
        public int returnValue { get; set; }
        public string message { get; set; }
    }
    public class ErfModel
    {
        public CustomerModel Customer;
        public ErfAssetsModel ErfAssetsModel;
        public NotesModel Notes;
        public IList<NewNotesModel> NewNotes;

        public ERFManagementSubmitType Operation { get; set; }

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

    public class ContingentDetails
    {
        public int id { get; set; }
        public int ContingentId { get; set; }
        public string Name { get; set; }
        public decimal LadinCost { get; set; }
        public decimal CashSale { get; set; }
        public decimal Rental { get; set; }
        public bool IsActive { get; set; }

        public string ContingentName { get; set; }

        public List<ContingentDetail> ContingentList;
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

    public class ERFStatusModel
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }

        public static List<ERFStatusModel> GetERFStatusList()
        {
            return new List<ERFStatusModel>(){
                 new ERFStatusModel() {StatusId=0, StatusName="" },
                new ERFStatusModel() {StatusId=1, StatusName="Processed" },
                new ERFStatusModel() {StatusId=2, StatusName="Shipped" },
                new ERFStatusModel() {StatusId=3, StatusName="Pending" },
                new ERFStatusModel() {StatusId=4, StatusName="Complete" },
                new ERFStatusModel() {StatusId=5, StatusName="Cancel"},
                new ERFStatusModel() {StatusId=6, StatusName="Sourcing 3rd Party"}};
        }

    }

    public class ERFBulkUploadDataModel
    {
        public int groupNum { get; set; }
        public int AccountNumber { get; set; }
        public string MainContactNum { get; set; }
        public string MainContactName { get; set; }
        public string ErfNotes { get; set; }
        public string OrderType { get; set; }
        public string ShipToBranch { get; set; }
        public string InstallDate { get; set; }
        public decimal AdditionalNSV { get; set; }
        public string EqpCategory { get; set; }
        public string EqpBrand { get; set; }
        public int EqpQuantity { get; set; }
        public string EqpUsingBranch { get; set; }
        public string EqpSubstitutionPossible { get; set; }
        public string EqpTransType { get; set; }
        public string EqpType { get; set; }
        public string ExpCategory { get; set; }
        public string ExpBrand { get; set; }
        public int ExpQuantity { get; set; }
        public string ExpUsingBranch { get; set; }
        public string ExpSubstitutionPossible { get; set; }
        public string ExpTransType { get; set; }
        public string ExpType { get; set; }

        public string HoursofOperation { get; set; }
        public string InstallLocation { get; set; }
        public string SiteReady { get; set; }

        public DateTime FormDate { get; set; }
        public DateTime ERFReceivedDate { get; set; }
        public DateTime ERFProcessedDate { get; set; }

        public ERFBulkUploadDataModel(string row)
        {
            string[] dataRow = Regex.Split(row, "[,]{1}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            if (dataRow.Length > 0)
            {
                //if (dataRow[0] != null)
                //    groupNum = Convert.ToInt32(dataRow[0]);
                if (dataRow[0] != null && !string.IsNullOrEmpty(dataRow[0]))
                    AccountNumber = Convert.ToInt32(dataRow[0]);
                if (dataRow[1] != null)
                    MainContactName = dataRow[1].ToString();
                if (dataRow[2] != null)
                    MainContactNum= dataRow[2].ToString();
                if (dataRow[3] != null)
                    HoursofOperation = dataRow[3].ToString();
                if (dataRow[4] != null)
                    InstallLocation = dataRow[4].ToString();
                if (dataRow[5] != null)
                    SiteReady = dataRow[5].ToString();
                if (dataRow[6] != null)
                    ErfNotes = dataRow[6].ToString();
                if (dataRow[7] != null)
                    OrderType = dataRow[7].ToString();
                if (dataRow[8] != null)
                    ShipToBranch = dataRow[8].ToString();
                if (dataRow[9] != null)
                    InstallDate = dataRow[9].ToString();
                if (dataRow[10] != null && !string.IsNullOrEmpty(dataRow[10]))
                    AdditionalNSV = Convert.ToDecimal(dataRow[10].ToString());
                if (dataRow[11] != null && !string.IsNullOrEmpty(dataRow[11]))
                    FormDate = Convert.ToDateTime(dataRow[11].ToString());
                if (dataRow[12] != null && !string.IsNullOrEmpty(dataRow[12]))
                    ERFReceivedDate = Convert.ToDateTime(dataRow[12].ToString());
                if (dataRow[13] != null && !string.IsNullOrEmpty(dataRow[13]))
                    ERFProcessedDate = Convert.ToDateTime(dataRow[13].ToString());
                if (dataRow[14] != null)
                    EqpCategory = dataRow[14].ToString();
                if (dataRow[15] != null)
                    EqpBrand = dataRow[15].ToString();
                if (dataRow[16] != null && !string.IsNullOrEmpty(dataRow[16]))
                    EqpQuantity = Convert.ToInt32(dataRow[16]);
                if (dataRow[17] != null)
                    EqpUsingBranch = dataRow[17].ToString();
                if (dataRow[18] != null)
                    EqpSubstitutionPossible = dataRow[18].ToString();
                if (dataRow[19] != null)
                    EqpTransType = dataRow[19].ToString();
                if (dataRow[20] != null)
                    EqpType = dataRow[20].ToString();
                if (dataRow[21] != null)
                    ExpCategory = dataRow[21].ToString();
                if (dataRow[22] != null)
                    ExpBrand = dataRow[22].ToString();
                if (dataRow[23] != null && !string.IsNullOrEmpty(dataRow[23]))
                    ExpQuantity = Convert.ToInt32(dataRow[23]);
                if (dataRow[24] != null)
                    ExpUsingBranch = dataRow[24].ToString();
                if (dataRow[25] != null)
                    ExpSubstitutionPossible = dataRow[25].ToString();
                if (dataRow[26] != null)
                    ExpTransType = dataRow[26].ToString();
                if (dataRow[27] != null)
                    ExpType = dataRow[27].ToString();

            }
        }


    }

    public class CashSaleModel
    {
        public string StatusCode { get; set; }
        public string StatusName { get; set; }
    }

}