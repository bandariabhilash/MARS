using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace FarmerBrothers.Data
{

    public class MarsViews
    {
        String con = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        
        public DataTable fnTpspVendors(string strSql)
        {
            SqlConnection sqlConnection = new SqlConnection(con);
            sqlConnection.Open();
            SqlCommand cmnd = new SqlCommand(strSql, sqlConnection);
            SqlDataAdapter da = new SqlDataAdapter(cmnd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        /*public DataTable GetPMScheduleData(string sDate,string eDate,int useDates,int contactId,int dealerID)
        {
            sDate = sDate.Substring(0, sDate.IndexOf(" "));
            eDate = eDate.Substring(0, eDate.IndexOf(" "));
            DataSet ds = new DataSet("TimeRanges");
            using (SqlConnection conn = new SqlConnection(con))
            {
                SqlCommand sqlComm = new SqlCommand("USP_GetAllPMSchedules", conn);
                sqlComm.Parameters.AddWithValue("@ScheduleStartDate", sDate);
                sqlComm.Parameters.AddWithValue("@ScheduleEndDate", eDate);
                sqlComm.Parameters.AddWithValue("@UseDates", useDates);
                sqlComm.Parameters.AddWithValue("@ContactID", contactId);
                sqlComm.Parameters.AddWithValue("@DealerID", dealerID);
                sqlComm.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;

                da.Fill(ds);
            }
            return ds.Tables[0];
        }*/

        public DataTable GetPMScheduleData(int contactId, string startDate, string endDate)
        {
            DataSet ds = new DataSet("TimeRanges");
            using (SqlConnection conn = new SqlConnection(con))
            {
                SqlCommand sqlComm = new SqlCommand("USP_GetAllPMSchedules", conn);
                sqlComm.Parameters.AddWithValue("@StartDate", startDate);
                sqlComm.Parameters.AddWithValue("@EndDate", endDate);
                sqlComm.Parameters.AddWithValue("@ContactID", contactId);                              
                sqlComm.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;

                da.Fill(ds);
            }
            return ds.Tables[0];
        }


        public DataTable fn_FSM_View(string strSql)
        {
            SqlConnection sqlConnection = new SqlConnection(con);
            sqlConnection.Open();
            SqlCommand cmnd = new SqlCommand(strSql, sqlConnection);
            SqlDataAdapter da = new SqlDataAdapter(cmnd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
        public DataTable fbDeltaVendors(string strSql,string StartDate, string EndDate)
        {
            SqlConnection sqlConnection = new SqlConnection(con);
            sqlConnection.Open();
            SqlCommand cmnd = new SqlCommand(strSql, sqlConnection);
            cmnd.CommandType = CommandType.StoredProcedure;
            cmnd.Parameters.AddWithValue("@StartDate", StartDate);
            cmnd.Parameters.AddWithValue("@EndDate", EndDate);
            SqlDataAdapter da = new SqlDataAdapter(cmnd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
        public DataTable fbSuperInvoice(string strSql, string StartDate, string EndDate, string ParentACC, string FamilyAffs, string Technicianlist)
        {
            SqlConnection sqlConnection = new SqlConnection(con);
            sqlConnection.Open();
            SqlCommand cmnd = new SqlCommand(strSql, sqlConnection);
            cmnd.CommandType = CommandType.StoredProcedure;
            DateTime SDate = Convert.ToDateTime(StartDate);
            DateTime EDate = Convert.ToDateTime(EndDate);
            String SD = SDate.ToString("MM/dd/yyyy");
            String ED = EDate.ToString("MM/dd/yyyy");
            cmnd.Parameters.AddWithValue("@StartDate", SD);
            cmnd.Parameters.AddWithValue("@EndDate", ED);
            cmnd.Parameters.AddWithValue("@PricingParentID", ParentACC);
            cmnd.Parameters.AddWithValue("@FamilyAff", FamilyAffs);
            cmnd.Parameters.AddWithValue("@TechID", Technicianlist);
            SqlDataAdapter da = new SqlDataAdapter(cmnd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable fbSuperInvoiceByTech(string strSql, string StartDate, string EndDate, string ParentACC, string FamilyAffs, string Technicianlist,string ESM, string Route, string Branch, string Region)
        {
            SqlConnection sqlConnection = new SqlConnection(con);
            sqlConnection.Open();
            SqlCommand cmnd = new SqlCommand(strSql, sqlConnection);
            cmnd.CommandType = CommandType.StoredProcedure;
            DateTime SDate = Convert.ToDateTime(StartDate);
            DateTime EDate = Convert.ToDateTime(EndDate);
            String SD = SDate.ToString("MM/dd/yyyy");
            String ED = EDate.ToString("MM/dd/yyyy");
            cmnd.Parameters.AddWithValue("@StartDate", SD);
            cmnd.Parameters.AddWithValue("@EndDate", ED);
            cmnd.Parameters.AddWithValue("@PricingParentID", ParentACC);
            cmnd.Parameters.AddWithValue("@FamilyAff", FamilyAffs);
            cmnd.Parameters.AddWithValue("@TechID", Technicianlist);
            cmnd.Parameters.AddWithValue("@ESM", ESM);
            cmnd.Parameters.AddWithValue("@Route", Route);
            cmnd.Parameters.AddWithValue("@Branch", Branch);
            cmnd.Parameters.AddWithValue("@Region", Region);
            SqlDataAdapter da = new SqlDataAdapter(cmnd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable fbBillingUpload(string strSql, string StartDate, string EndDate)
        {
            SqlConnection sqlConnection = new SqlConnection(con);
            sqlConnection.Open();
            SqlCommand cmnd = new SqlCommand(strSql, sqlConnection);
            cmnd.CommandType = CommandType.StoredProcedure;
            DateTime SDate = Convert.ToDateTime(StartDate);
            DateTime EDate = Convert.ToDateTime(EndDate);
            String SD = SDate.ToString("MM/dd/yyyy");
            String ED = EDate.ToString("MM/dd/yyyy");
            cmnd.Parameters.AddWithValue("@StartDate", SD);
            cmnd.Parameters.AddWithValue("@EndDate", ED);
            SqlDataAdapter da = new SqlDataAdapter(cmnd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable fbBilling(string strSql, string StartDate, string EndDate, string AccountNo, string Dealerid, string FamilyAff, string PPID)
        {
            SqlConnection sqlConnection = new SqlConnection(con);
            sqlConnection.Open();
            SqlCommand cmnd = new SqlCommand(strSql, sqlConnection);
            cmnd.CommandType = CommandType.StoredProcedure;
            DateTime SDate = Convert.ToDateTime(StartDate);
            DateTime EDate = Convert.ToDateTime(EndDate);
            String SD = SDate.ToString("MM/dd/yyyy");
            String ED = EDate.ToString("MM/dd/yyyy");
            int TL = string.IsNullOrEmpty(Dealerid) ? 0 : (Convert.ToInt32(Dealerid) < 0 ? 0 : Convert.ToInt32(Dealerid));
            String FA = FamilyAff.ToLower() == "all" ? "0" : FamilyAff.ToString();
            String AcntNo = AccountNo == null ? "0" : AccountNo.ToString();

            cmnd.Parameters.AddWithValue("@StartDate", SD);
            cmnd.Parameters.AddWithValue("@EndDate", ED);
            cmnd.Parameters.AddWithValue("@AccountNo", AcntNo);
            cmnd.Parameters.AddWithValue("@DealerId", TL);
            cmnd.Parameters.AddWithValue("@FamilyAff", FA);
            cmnd.Parameters.AddWithValue("@PPID", PPID);
            SqlDataAdapter da = new SqlDataAdapter(cmnd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

    }

}