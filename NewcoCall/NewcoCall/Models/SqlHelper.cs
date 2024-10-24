using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FBCall.Models
{
    public class SqlHelper
    {
        private string connectionString = string.Empty;
        private SqlConnection sqlConn;
        private SqlDataAdapter sqlAdapter;
        private SqlCommand sqlCommand;

        public SqlHelper()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        }

        public DataTable GetDatatable(string commandText)
        {
            return ExecuteReader(commandText, CommandType.Text);
        }

        public DataTable GetDatatable(string commandText, bool isProcedure)
        {
            return ExecuteReader(commandText, CommandType.StoredProcedure);
        }

        private DataTable ExecuteReader(string commandText, CommandType commandType)
        {
            DataTable dataset = new DataTable();
            using (sqlConn = new SqlConnection(connectionString))
            {
                using (sqlCommand = new SqlCommand(commandText, sqlConn))
                {
                    try
                    {
                        sqlCommand.CommandType = commandType;
                        using (sqlAdapter = new SqlDataAdapter())
                        {
                            sqlAdapter.SelectCommand = sqlCommand;
                            sqlAdapter.Fill(dataset);
                            return dataset;
                        }
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                }
            }
        }

        public DataTable GetDatatable(SqlCommand sqlCommand)
        {
            DataTable dataset = new DataTable();
            using (sqlConn = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCommand.Connection = sqlConn;
                    using (sqlCommand)
                    {
                        using (sqlAdapter = new SqlDataAdapter())
                        {
                            sqlAdapter.SelectCommand = sqlCommand;
                            sqlAdapter.Fill(dataset);
                        }
                    }
                }
                catch { }
                return dataset;
            }
        }

        public int UpdateCommand(string commandText)
        {
            using (sqlConn = new SqlConnection(connectionString))
            {
                using (sqlCommand = new SqlCommand(commandText, sqlConn))
                {
                    try
                    {
                        sqlConn.Open();
                        sqlCommand.CommandType = CommandType.Text;
                        return sqlCommand.ExecuteNonQuery();
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                }
            }
        }

        public DataTable GetTechDispatchDetails(string customerZipCode, double DealerLatLongFactor)
        {
            DataSet ds = new DataSet("TechDispatch");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand sqlComm = new SqlCommand("USP_AutoDispatchTechDispatch_Details", conn);
                sqlComm.Parameters.AddWithValue("@CustomerZip", customerZipCode);
                sqlComm.Parameters.AddWithValue("@DealerLatLongFactor", DealerLatLongFactor);
                sqlComm.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                da.Fill(ds);
            }
            return ds.Tables[0];
        }

        public DataTable GetTechDispatchCountDetails(int techId)
        {
            DataSet ds = new DataSet("TechDispatch");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand sqlComm = new SqlCommand("USP_GettechDispatch_CountDetails", conn);
                sqlComm.Parameters.AddWithValue("@techID", techId);
                sqlComm.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                da.Fill(ds);
            }
            return ds.Tables[0];
        }

        public DataTable GetProgramStatusReportData(params string[] arguemnts)
        {
            DataSet ds = new DataSet("ProgramStatusReprot");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlComm = new SqlCommand("USP_ProgramStatus_Report", conn))
                {
                    sqlComm.CommandTimeout = 0;
                    sqlComm.Parameters.AddWithValue("@StartDate", arguemnts[0]);
                    sqlComm.Parameters.AddWithValue("@EndDate", arguemnts[1]);
                    sqlComm.Parameters.AddWithValue("@varState", arguemnts[2]);
                    sqlComm.Parameters.AddWithValue("@EventStatus", arguemnts[3]);
                    sqlComm.Parameters.AddWithValue("@ApptStartDate", arguemnts[4]);
                    sqlComm.Parameters.AddWithValue("@ApptEndDate", arguemnts[5]);
                    sqlComm.Parameters.AddWithValue("@ArrvStartDate", arguemnts[6]);
                    sqlComm.Parameters.AddWithValue("@ArrvEndDate", arguemnts[7]);
                    sqlComm.Parameters.AddWithValue("@ComplStartDate", arguemnts[8]);
                    sqlComm.Parameters.AddWithValue("@ComplEndDate", arguemnts[9]);
                    sqlComm.Parameters.AddWithValue("@CallTypeID", arguemnts[10]);
                    sqlComm.Parameters.AddWithValue("@JDENO", arguemnts[11]);
                    sqlComm.Parameters.AddWithValue("@PricingParentID", arguemnts[12]);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter da = new SqlDataAdapter())
                    {
                        da.SelectCommand = sqlComm;
                        da.Fill(ds);
                    }

                }


            }
            return ds.Tables[0];
        }

        public DataTable GetCloserPartsReportData(params string[] arguemnts)
        {
            DataSet ds = new DataSet("CloserPartReprot");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand sqlComm = new SqlCommand("USP_FBClosure_Parts", conn);
                sqlComm.Parameters.AddWithValue("@StartDate", arguemnts[0]);
                sqlComm.Parameters.AddWithValue("@EndDate", arguemnts[1]);
                sqlComm.Parameters.AddWithValue("@ContactID", arguemnts[2]);
                sqlComm.Parameters.AddWithValue("@EntryNumber", arguemnts[3]);
                sqlComm.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                da.Fill(ds);
            }
            return ds.Tables[0];
        }

        public DataTable GetClosestOnCallTechDetails(string customerZipCode)
        {
            DataSet ds = new DataSet("OnCallTechDetails");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand sqlComm = new SqlCommand("USP_ClosestOnCallTechDispatch_Details", conn);
                sqlComm.Parameters.AddWithValue("@CustomerZip", customerZipCode);
                sqlComm.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                da.Fill(ds);
            }
            return ds.Tables[0];
        }

        public DataTable GetOnCallTechDetails(string customerZipCode, int? workOrderId)
        {
            DataSet ds = new DataSet("OnCallTechDetails");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand sqlComm = new SqlCommand("USP_OnCallTechDispatch_Details", conn);
                sqlComm.Parameters.AddWithValue("@CustomerZip", customerZipCode);
                sqlComm.Parameters.AddWithValue("@WorkOrderId", workOrderId);
                sqlComm.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                da.Fill(ds);
            }
            return ds.Tables[0];
        }

        public DataTable GetAfterHoursOnCallTechDetails(string customerZipCode, int? workOrderId)
        {
            DataSet ds = new DataSet("OnCallTechDetails");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand sqlComm = new SqlCommand("USP_OncallAfterHoursTechDispatch_Details", conn);
                sqlComm.Parameters.AddWithValue("@CustomerZip", customerZipCode);
                sqlComm.Parameters.AddWithValue("@WorkOrderId", workOrderId);
                sqlComm.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                da.Fill(ds);
            }
            return ds.Tables[0];
        }

        public DataTable GetAfterHoursClosestOnCallTechDetails(string customerZipCode)
        {
            DataSet ds = new DataSet("OnCallTechDetails");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand sqlComm = new SqlCommand("USP_OncallAfterHoursClosestTechDispatch_Details", conn);
                sqlComm.Parameters.AddWithValue("@CustomerZip", customerZipCode);
                sqlComm.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                da.Fill(ds);
            }
            return ds.Tables[0];
        }
    }
}