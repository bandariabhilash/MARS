using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LumenWorks.Framework.IO.Csv;
using FarmerBrothers.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace FarmerBrothers.Controllers
{
    public class ZoneZipController : BaseController
    {
        // GET: ZoneZip
        public ActionResult ZoneZip()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FileUpload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {

                    string fileName = Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
                    file.SaveAs(path);                  
                    DataTable dt;
                    dt = ProcessCSV(path);                   
                    MakeTempTable(dt);                  

                    TempData["Sucess"] = "File Uploaded Successfully";
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                TempData["Message"] = "You have not specified a file.";
            }
            // return RedirectToAction("ZoneZip");
            return Content("");
        }
        private static DataTable ProcessCSV(string fileName)
        {
            DataTable csvTable = new DataTable();
            using (CsvReader csvReader =
                new CsvReader(new StreamReader(fileName, System.Text.Encoding.GetEncoding("iso-8859-1")), true))
            {
                csvTable.Load(csvReader);
            }
            return csvTable;           
        }        
        public void MakeTempTable(DataTable dtExcelTable)
        {
            try
            {
                
                DataTable localTempTable = new DataTable();
                localTempTable.Columns.Add("ZipCode", typeof(string));
                localTempTable.Columns.Add("ZoneIndex", typeof(Int32));
                localTempTable.Columns.Add("ZoneName", typeof(string));

                foreach (DataRow dr in dtExcelTable.Rows)
                {
                    DataRow row = localTempTable.NewRow();
                    row[0] = dr["ZIP Code"];
                    row[1] = dr["Zone"];
                    row[2] = dr["Territory"];
                    localTempTable.Rows.Add(row);
                }

                localTempTable.AcceptChanges();

                SqlParameter sqlParam = new SqlParameter();
                sqlParam.ParameterName = "@ZoneZipdata";
                sqlParam.TypeName = "dbo.ZoneZipType";
                sqlParam.SqlDbType = SqlDbType.Structured;
                sqlParam.Value = localTempTable;
                String con = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
                SqlConnection sqlCon = new SqlConnection(con);
                sqlCon.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = sqlCon;
                cmd.Parameters.Add(sqlParam);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "USP_Update_ZonePriority";
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                sqlCon.Close();            
                
                //FormalBrothersEntitites.Database.ExecuteSqlCommand("exec dbo.USP_Update_ZonePriority",sqlParam);
                //return true;
            }
            catch (Exception ex)
            {
                //return false;
                throw ex;
            }

        }
    }
}