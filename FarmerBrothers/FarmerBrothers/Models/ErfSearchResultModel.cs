using FarmerBrothers.Data;
using System;
using System.Data;
using System.Linq;

namespace FarmerBrothers.Models
{
    public class ErfSearchResultModel
    {
        public ErfSearchResultModel(Erf erf)
        {
            ErfID = erf.ErfID;
            if (erf.EntryDate.HasValue)
            {
                ERFEntryDate = erf.EntryDate.Value.ToString("MM/dd/yyyy hh:mm tt");
            }
            CustomerID = erf.CustomerID.ToString();
            CustomerName = erf.CustomerName;
            Address = erf.CustomerAddress;
            City = erf.CustomerCity;
            State = erf.CustomerState;
            Status = erf.ERFStatus;
            WorkorderId = erf.WorkorderID == null ? "" : erf.WorkorderID.ToString();
            ApprovalStatus = erf.ApprovalStatus == null ? "" : erf.ApprovalStatus.ToString();

            using (FarmerBrothersEntities fbEntities = new FarmerBrothersEntities())
            {
                //AllFormalBrothersStatu FormalBrothersStatus = smcukerEntities.AllFormalBrothersStatus.Where(r => r.FormalBrothersStatusID == erf.ReasonID).FirstOrDefault();
                //if (FormalBrothersStatus != null)
                //{
                //    Reason = FormalBrothersStatus.FormalBrothersStatus;
                //}

                FbUserMaster user = fbEntities.FbUserMasters.Where(r => r.UserId == erf.EntryUserID).FirstOrDefault();
                if(user != null)
                {
                    OriginatorName = user.FirstName + " " + user.LastName;
                }

            }
        }

        public ErfSearchResultModel(DataRow dr, bool dummy)
        {
            if (dr.Table.Columns.Contains("EntryDate") && dr["EntryDate"] != DBNull.Value)
            {
                ERFEntryDate = Convert.ToDateTime(dr["EntryDate"]).ToString("MM/dd/yyyy hh:mm tt");
            }
            if (dr.Table.Columns.Contains("ErfID") && dr["ErfID"] != DBNull.Value)
            {
                ErfID = dr["ErfID"].ToString();
            }
            if (dr.Table.Columns.Contains("CustomerID") && dr["CustomerID"] != DBNull.Value)
            {
                CustomerID = dr["CustomerID"].ToString();
            }
            if (dr.Table.Columns.Contains("CustomerName") && dr["CustomerName"] != DBNull.Value)
            {
                CustomerName = dr["CustomerName"].ToString();
            }
            if (dr.Table.Columns.Contains("CustomerAddress") && dr["CustomerAddress"] != DBNull.Value)
            {
                Address = dr["CustomerAddress"].ToString();
            }
            if (dr.Table.Columns.Contains("CustomerCity") && dr["CustomerCity"] != DBNull.Value)
            {
                City = dr["CustomerCity"].ToString();
            }
            if (dr.Table.Columns.Contains("CustomerState") && dr["CustomerState"] != DBNull.Value)
            {
                State = dr["CustomerState"].ToString();
            }
            if (dr.Table.Columns.Contains("ERFStatus") && dr["ERFStatus"] != DBNull.Value)
            {
                Status = dr["ERFStatus"].ToString();
            }
            if (dr.Table.Columns.Contains("WorkorderID") && dr["WorkorderID"] != DBNull.Value)
            {
                WorkorderId = dr["WorkorderID"].ToString();
            }
            if (dr.Table.Columns.Contains("ApprovalStatus") && dr["ApprovalStatus"] != DBNull.Value)
            {
                ApprovalStatus = dr["ApprovalStatus"].ToString();
            }

            if(dr.Table.Columns.Contains("FirstName") && dr["FirstName"] != DBNull.Value && dr.Table.Columns.Contains("LastName") && dr["LastName"] != DBNull.Value)
            {
                OriginatorName = dr["FirstName"].ToString() + " " + dr["LastName"].ToString(); 
            }
            else if (dr.Table.Columns.Contains("FirstName") && dr["FirstName"] != DBNull.Value)
            {
                OriginatorName = dr["FirstName"].ToString();
            }
            else if (dr.Table.Columns.Contains("LastName") && dr["LastName"] != DBNull.Value)
            {
                OriginatorName = dr["LastName"].ToString() ;
            }
        }

        public string ErfID { get; set; }
        public string ERFEntryDate { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Reason { get; set; }
        public string OriginatorName { get; set; }
        public string Status { get; set; }
        public string WorkorderId { get; set; }
        public string ApprovalStatus { get; set; }
    }
}