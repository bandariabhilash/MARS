using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class SerialNumberSearchResultModel
    {
        public SerialNumberSearchResultModel()
        {

        }

        public SerialNumberSearchResultModel(DataRow dr)
        {
            if (dr["EventID"] != DBNull.Value)
            {
                this.EventID = Convert.ToInt32(dr["EventID"]);
            }
            if (dr["FulfillmentStatus"] != DBNull.Value)
            {
                this.FulfillmentStatus = dr["FulfillmentStatus"].ToString();
            }
            if (dr["EntryDate"] != DBNull.Value)
            {
                this.EntryDate = dr["EntryDate"] == null ? null : dr["EntryDate"].ToString();
            }
            if (dr["CloseDate"] != DBNull.Value)
            {
                this.CloseDate = dr["CloseDate"] == null ? null : dr["CloseDate"].ToString();
            }
            if (dr["ContactID"] != DBNull.Value)
            {
                this.ContactID = Convert.ToInt32(dr["ContactID"]);
            }
            if (dr["CompanyName"] != DBNull.Value)
            {
                this.CompanyName = dr["CompanyName"].ToString();
            }
            if (dr["CallTypeId"] != DBNull.Value)
            {
                this.CallTypeId = Convert.ToInt32(dr["CallTypeId"]);
            }
            if (dr["CallTypeDesc"] != DBNull.Value)
            {
                this.CallTypeDesc = dr["CallTypeDesc"].ToString();
            }
            if (dr["SerialNo"] != DBNull.Value)
            {
                this.SerialNo = dr["SerialNo"].ToString();
            }
            if (dr["ProductNo"] != DBNull.Value)
            {
                this.ProductNo = dr["ProductNo"].ToString();
            }
            if (dr["ProductDesc1"] != DBNull.Value)
            {
                this.ProductDesc1 = dr["ProductDesc1"].ToString();
            }
            if (dr["Manufacturer"] != DBNull.Value)
            {
                this.Manufacturer = dr["Manufacturer"].ToString();
            }
            if (dr["ManufacturerDesc"] != DBNull.Value)
            {
                this.ManufacturerDesc = dr["ManufacturerDesc"].ToString();
            }
            if (dr["NoService"] != DBNull.Value)
            {
                this.NoService = Convert.ToInt32(dr["NoService"]);
            }
            if (dr["SearchType"] != DBNull.Value)
            {
                this.SearchType = dr["SearchType"].ToString();
            }
            if (dr["SearchDesc"] != DBNull.Value)
            {
                this.SearchDesc = dr["SearchDesc"].ToString();
            }

        }

    public SerialNumberSearchResultModel(TMP_SerialNOReport results)
        {
            this.EventID = results.EventID;
            this.FulfillmentStatus = results.FulfillmentStatus;
            this.EntryDate = results.EntryDate == null ? null : results.EntryDate.ToString();
            this.CloseDate = results.CloseDate == null ? null : results.CloseDate.ToString();
            this.ContactID = results.ContactID;
            this.CompanyName = results.CompanyName;
            this.CallTypeId = results.CallTypeId;
            this.CallTypeDesc = results.CallTypeDesc;
            this.SerialNo = results.SerialNo;
            this.ProductNo = results.ProductNo;
            this.ProductDesc1 = results.ProductDesc1;
            this.Manufacturer = results.Manufacturer;
            this.ManufacturerDesc = results.ManufacturerDesc;
            this.NoService = results.NoService;
            this.SearchType = results.SearchType;
            this.SearchDesc = results.SearchDesc;
        }

        public int EventID { get; set; }
        public string FulfillmentStatus { get; set; }
        public string EntryDate { get; set; }
        public string CloseDate { get; set; }
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public int? CallTypeId { get; set; }
        public string CallTypeDesc { get; set; }
        public string SerialNo { get; set; }
        public string ProductNo { get; set; }
        public string ProductDesc1 { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerDesc { get; set; }
        public int? NoService { get; set; }
        public string SearchType { get; set; }
        public string SearchDesc { get; set; }

    }


}