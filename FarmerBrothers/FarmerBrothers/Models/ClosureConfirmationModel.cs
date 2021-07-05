using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Configuration;

namespace FarmerBrothers.Models
{
    public class ClosureConfirmationModel
    {
        public IList<ClosureConfirmation> closureconfirmation { get; set; }
        public int workorderid { get; set; }
    }

    public class ClosureConfirmation
    {
        public static class FieldNames
        {
            public const string WorkOrder = "WorkorderID";
            public const string EntryDate = "WorkorderEntryDate";
            public const string ClosedDate = "WorkorderCloseDate";
            public const string ClosedUserName = "ClosedUserName";
            public const string WOConfirmationCode = "WorkorderClosureConfirmationNo";
        }

        #region properties

        public string WorkOrder { get; set; }
        public string EntryDate { get; set; }
        public string ClosedDate { get; set; }
        public string ClosedUserName { get; set; }
        public string WOConfirmationCode { get; set; }
        
        #endregion

        public ClosureConfirmation()
        {
        }
        public ClosureConfirmation(DataRow dr)
        {

            if (dr[FieldNames.WorkOrder] != DBNull.Value)
            {
                WorkOrder = dr[FieldNames.WorkOrder].ToString();
            }
            if (dr[FieldNames.EntryDate] != DBNull.Value)
            {
                EntryDate = dr[FieldNames.EntryDate].ToString();
            }
            if (dr[FieldNames.ClosedDate] != DBNull.Value)
            {
                ClosedDate = dr[FieldNames.ClosedDate].ToString();
            }
            if (dr[FieldNames.ClosedUserName] != DBNull.Value)
            {
                ClosedUserName = dr[FieldNames.ClosedUserName].ToString();
            }
            if (dr[FieldNames.WOConfirmationCode] != DBNull.Value)
            {
                WOConfirmationCode = dr[FieldNames.WOConfirmationCode].ToString();
            }
            
        }

        public static List<ClosureConfirmation> GetClosureConfirmationReport(ClosureConfirmationModel closuremodel)
        {
            List<ClosureConfirmation> SearchResults = new List<ClosureConfirmation>();
            DataTable dt = new DataTable();
            List<Parameter> parms = new List<Parameter>();
            string cs = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            try
            {
                SqlConnection con = new SqlConnection(cs);
                
                string sql= @"select [WorkorderID],[WorkorderEntryDate],[WorkorderCloseDate],[ClosedUserName],[WorkorderClosureConfirmationNo] from [dbo].[WorkOrder]
 WHERE WorkorderID=@WorkorderID and [WorkorderCallstatus]='Closed'";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@WorkorderID", closuremodel.workorderid);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                
                foreach (DataRow dr in dt.Rows)
                {
                    ClosureConfirmation confirmation = new ClosureConfirmation(dr);
                    SearchResults.Add(confirmation);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Clients from database", ex);
            }

            return SearchResults;

        }

    }
}