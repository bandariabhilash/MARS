using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;

namespace FormalBrothers
{
    public class Utilities
    {
        public static void EntityToExcelSheet(string excelFilePath, string sheetName, IQueryable result, DbContext ctx)
        {
            Excel.Application oXL;
            Excel.Workbook oWB;
            Excel.Worksheet oSheet;
            Excel.Range oRange;
            try
            {
                // Start Excel and get Application object.
                oXL = new Excel.Application();

                // Set some properties
                oXL.DisplayAlerts = false;

                // Get a new workbook. 
                oWB = oXL.Workbooks.Add(Missing.Value);

                // Get the active sheet 
                oSheet = (Excel.Worksheet)oWB.ActiveSheet;
                oSheet.Name = sheetName;

                // Process the DataTable
                // BE SURE TO CHANGE THIS LINE TO USE *YOUR* DATATABLE 
                DataTable dt = EntityToDataTable(result, ctx);

                int rowCount = 1;
                foreach (DataRow dr in dt.Rows)
                {
                    rowCount += 1;
                    for (int i = 1; i < dt.Columns.Count + 1; i++)
                    {
                        // Add the header the first time through 
                        if (rowCount == 2)
                            oSheet.Cells[1, i] = dt.Columns[i - 1].ColumnName;
                        oSheet.Cells[rowCount, i] = dr[i - 1].ToString();
                    }
                }

                // Resize the columns 
                oRange = oSheet.Range[oSheet.Cells[1, 1], oSheet.Cells[rowCount, dt.Columns.Count]];
                oRange.Columns.AutoFit();

                // Save the sheet and close 
                oSheet = null;
                oRange = null;
                oWB.SaveAs(excelFilePath, Excel.XlFileFormat.xlWorkbookNormal, Missing.Value,
                  Missing.Value, Missing.Value, Missing.Value,
                  Excel.XlSaveAsAccessMode.xlExclusive, Missing.Value,
                  Missing.Value, Missing.Value, Missing.Value);
                oWB.Close(Missing.Value, Missing.Value, Missing.Value);
                oWB = null;
                oXL.Quit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable EntityToDataTable(IQueryable result, DbContext ctx)
        {
            try
            {
                //EntityConnection conn = ctx.Database.Connection as EntityConnection;
                using (SqlConnection SQLCon = new SqlConnection(ctx.Database.Connection.ConnectionString))
                {
                    using (SqlCommand Cmd = new SqlCommand(result.ToString(), SQLCon))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(Cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                da.Fill(dt);
                                return dt;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}