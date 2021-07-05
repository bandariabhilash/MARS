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
                    ErfNotes = dataRow[3].ToString();
                if (dataRow[4] != null)
                    OrderType = dataRow[4].ToString();
                if (dataRow[5] != null)
                    ShipToBranch = dataRow[5].ToString();
                if (dataRow[6] != null)
                    InstallDate = dataRow[6].ToString();
                if (dataRow[7] != null && !string.IsNullOrEmpty(dataRow[7]))
                    AdditionalNSV = Convert.ToDecimal(dataRow[7].ToString());
                if (dataRow[8] != null)
                    EqpCategory = dataRow[8].ToString();
                if (dataRow[9] != null)
                    EqpBrand = dataRow[9].ToString();
                if (dataRow[10] != null && !string.IsNullOrEmpty(dataRow[10]))
                    EqpQuantity = Convert.ToInt32(dataRow[10]);
                if (dataRow[11] != null)
                    EqpUsingBranch = dataRow[11].ToString();
                if (dataRow[12] != null)
                    EqpSubstitutionPossible = dataRow[12].ToString();
                if (dataRow[13] != null)
                    EqpTransType = dataRow[13].ToString();
                if (dataRow[14] != null)
                    EqpType = dataRow[14].ToString();
                if (dataRow[15] != null)
                    ExpCategory = dataRow[15].ToString();
                if (dataRow[16] != null)
                    ExpBrand = dataRow[16].ToString();
                if (dataRow[17] != null && !string.IsNullOrEmpty(dataRow[17]))
                    ExpQuantity = Convert.ToInt32(dataRow[17]);
                if (dataRow[18] != null)
                    ExpUsingBranch = dataRow[18].ToString();
                if (dataRow[19] != null)
                    ExpSubstitutionPossible = dataRow[19].ToString();
                if (dataRow[20] != null)
                    ExpTransType = dataRow[20].ToString();
                if (dataRow[21] != null)
                    ExpType = dataRow[21].ToString();

            }
        }


    }

}