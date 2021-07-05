using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class OpenCallByTechModel
    {
        public DateTime? OpenCallByTechFromDate { get; set; }
        public DateTime? OpenCallByTechToDate { get; set; }
    
        public int DealerId { get; set; }
        public string TechID { get; set; }
        public List<TECH_HIERARCHY> Technicianlist { get; set; }
        public List<Technician> FamilyAffs { get; set; }

        public IList<OpenCallByTechSearchResultModel> SearchResults;
    }

    public class OpenCallByTechSearchResultModel
    {
        public OpenCallByTechSearchResultModel()
        {

        }

        public OpenCallByTechSearchResultModel(DataRow dr)
        {
            if (dr["WorkOrderID"] != DBNull.Value)
            {
                EventID = Convert.ToString(dr["WorkOrderID"]);
            }
            if (dr["WorkorderCalltypeid"] != DBNull.Value)
            {
                CallTypeId = Convert.ToString(dr["WorkorderCalltypeid"]);
            }
            if (dr["WorkorderCalltypeDesc"] != DBNull.Value)
            {
                CallTypeDesc = Convert.ToString(dr["WorkorderCalltypeDesc"]);
            }
            if (dr["WorkorderCallstatus"] != DBNull.Value)
            {
                EventStatus = Convert.ToString(dr["WorkorderCallstatus"]);
            }
            if (dr["EventScheduleDate"] != DBNull.Value)
            {
                if (EventStatus.ToLower() == "scheduled")
                {
                    EventScheduleDate = Convert.ToString(dr["EventScheduleDate"]);
                }
                else
                {
                    EventScheduleDate = "";
                }
            }
            if (dr["WorkorderEntryDate"] != DBNull.Value)
            {
                EntryDate = Convert.ToString(dr["WorkorderEntryDate"]);
            }
            if (dr["RegionNo"] != DBNull.Value)
            {
                Region = Convert.ToString(dr["RegionNo"]);
            }
            if (dr["CustomerRegion"] != DBNull.Value)
            {
                RegionName = Convert.ToString(dr["CustomerRegion"]);
            }
            if (dr["CustomerBranchNo"] != DBNull.Value)
            {
                Branch = Convert.ToString(dr["CustomerBranchNo"]);
            }
            if (dr["CustomerBranch"] != DBNull.Value)
            {
                BranchName = Convert.ToString(dr["CustomerBranch"]);
            }
            if (dr["SearchType"] != DBNull.Value)
            {
                CustomerType = Convert.ToString(dr["SearchType"]);
            }
            if (dr["ContactID"] != DBNull.Value)
            {
                ContactID = Convert.ToString(dr["ContactID"]);
            }
            if (dr["CompanyName"] != DBNull.Value)
            {
                CompanyName = Convert.ToString(dr["CompanyName"]);
            }

            if (dr["Address1"] != DBNull.Value)
            {
                Address1 = Convert.ToString(dr["Address1"]);
            }

            if (dr["City"] != DBNull.Value)
            {
                City = Convert.ToString(dr["City"]);
            }
            if (dr["State"] != DBNull.Value)
            {
                State = Convert.ToString(dr["State"]);
            }
            if (dr["PostalCode"] != DBNull.Value)
            {
                PostalCode = Convert.ToString(dr["PostalCode"]);
            }

            if (dr["DispatchCompany"] != DBNull.Value)
            {
                DispatchCompany = Convert.ToString(dr["DispatchCompany"]);
            }
            if (dr["ScheduleDate"] != DBNull.Value)
            {
                DispatchDate = Convert.ToString(dr["ScheduleDate"]);
            }
           
            if (dr["ElapsedTime"] != DBNull.Value)
            {
                ElapsedTime = Convert.ToString(dr["ElapsedTime"]);
            }
            if (dr["FamilyAff"] != DBNull.Value)
            {
                FamilyAff = Convert.ToString(dr["FamilyAff"]);
            }
            if (dr["TechID"] != DBNull.Value)
            {
                TechId = Convert.ToString(dr["TechID"]);
            }
        }

        public string Region { get; set; }
        public string ESMName { get; set; }
        public string DispatchCompany { get; set; }
        public string TechId { get; set; }
        public string EventCount { get; set; }

        public string EventID { get; set; }
        public string EventStatus { get; set; }
        public string EventScheduleDate { get; set; }
        public string EntryDate { get; set; }
        public string RegionName { get; set; }
        public string Branch { get; set; }
        public string BranchName { get; set; }
        public string CustomerType { get; set; }
        public string DispatchDate { get; set; }
        public string ElapsedTime { get; set; }
        public string FamilyAff { get; set; }
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }

        public string ContactID { get; set; }

        public string CallTypeId { get; set; }
        public string CallTypeDesc { get; set; }
    }
}