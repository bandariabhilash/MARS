using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FarmerBrothers.Models
{
    public class TechnicianAvailabilitySearchResultModel
    {
        public TechnicianAvailabilitySearchResultModel(DataRow dr)
        {
            if (dr["DealerId"] != DBNull.Value)
            {
                TechID = dr["DealerId"].ToString();
            }
            if (dr["CompanyName"] != DBNull.Value)
            {
                TechName = dr["CompanyName"].ToString();
            }
            if (dr["ModifiedDate"] != DBNull.Value)
            {
                UpdateDate = Convert.ToDateTime(dr["ModifiedDate"]).ToString("MM/dd/yyyy");
            }
            if (dr["UpdatedBy"] != DBNull.Value)
            {
                UpdateBy = dr["UpdatedBy"].ToString();
            }
            
        }
        public string TechID { get; set; }
        public string TechName { get; set; }
        public string UpdateDate { get; set; }
        public string UpdateTo { get; set; }
        public string UpdateBy { get; set; }
    }
}
