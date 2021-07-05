using FarmerBrothers.CacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FarmerBrothers.Data;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Common;
using System.Data;

namespace FarmerBrothers.Models
{
    public class Security
    {
        #region Database Attributes

        public static class FieldNames
        {
            public const string TABLE_NAME = "FBRoleFunction";
            public const string ROLE_FUNC_ID = "RoleFunctionID";
            public const string ROLE_ID = "RoleID";
            public const string FUNC_ID = "FunctionID";
            public const string CAN_CREATE = "CanCreate";
            public const string CAN_UPDATE = "CanUpdate";
            public const string CAN_VIEW = "CanView";
            public const string CAN_EXPORT = "CanExport";
            public const string CAN_EMAIL = "CanEmail";
        }

        #endregion

        #region Properties
        public int? roleFunctionId { get; set; }
        public int roleId { get; set; }
        public int functionId { get; set; }
        public int? parentFunctionId { get; set; }
        public string functionName { get; set; }
        public int? canCreate { get; set; }
        public int? canUpdate { get; set; }
        public int? canView { get; set; }
        public int? canExport { get; set; }
        public int? canEmail { get; set; }
        public List<Security> children { get; set; }

        #endregion

        #region Old Methods

        public static Dictionary<int, List<Security>> GetTabSecurity(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            List<Security> tabSecurity = new List<Security>();
            Dictionary<int, List<Security>> securityData = new Dictionary<int, List<Security>>();

            if (MAICacheManager.hasType(MAICacheManager.TypeNames.ROLE_MAINTENANCE))
                securityData = MAICacheManager.getType(MAICacheManager.TypeNames.ROLE_MAINTENANCE) as Dictionary<int, List<Security>>;
            else
            {
                try
                {
                    tabSecurity = (from role in FarmerBrothersEntitites.VW_FBRoleFunction
                                   select new Security()
                                   {
                                       canCreate = role.CanCreate,
                                       canEmail = role.CanEmail,
                                       canExport = role.CanExport,
                                       canUpdate = role.CanUpdate,
                                       canView = role.CanView,
                                       functionId = role.FunctionID,
                                       functionName = role.FunctionName,
                                       parentFunctionId = role.ParentFunctionID,
                                       roleId = role.RoleID,
                                       roleFunctionId = role.RoleFunctionID
                                   }).ToList();
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to get the role information.", ex);
                }

                var groupedData = tabSecurity.GroupBy(s => s.roleId);
                foreach (var group in groupedData)
                {
                    var subGroup = group.GroupBy(s => s.parentFunctionId);
                    var groupItems = subGroup.Where(g => g.Key == 0).FirstOrDefault();
                    Dictionary<int?, List<Security>> data = subGroup.ToDictionary(k => k.Key, v => v.ToList());
                    List<Security> secure = new List<Security>();
                    foreach (var item in groupItems)
                    {
                        List<Security> sec;
                        if (data.TryGetValue(item.functionId, out sec))
                        {
                            var childAccess = sec.Where(s => s.canView != 0);
                            if (childAccess != null && childAccess.Count() > 0)
                                item.canView = 1;
                            else
                                item.canView = 0;
                            item.children = sec;
                        }
                        secure.Add(item);
                    }
                    securityData.Add(group.Key, secure);
                }

                MAICacheManager.setType(MAICacheManager.TypeNames.ROLE_MAINTENANCE, securityData);
            }


            return securityData;
        }

        public static string GetTabSecurityHtml(int roleId, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            Dictionary<int, List<Security>> securityData = GetTabSecurity(FarmerBrothersEntitites);
            List<Security> tabSecurity = securityData[roleId];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<table id='securityTable' style='width: 90%' class='table table-striped jambo_table bulk_action data_center'><thead><tr class='headings'><th class='column-title' width='25%'> </th><th class='column-title'>Can Create</th><th class='column-title'>Can View</th><th class='column-title'>Can Eidt</th><th class='column-title'>Can Export</th><th class='column-title'>Can Email</th></tr></thead><tbody>");
            foreach (Security tab in tabSecurity)
            {
                sb.AppendLine(GetSecurityRow(tab, false));
                if (tab.children != null)
                    foreach (Security sec in tab.children)
                        sb.AppendLine(GetSecurityRow(sec, true));
            }
            sb.AppendLine("</tbody></table>");

            return sb.ToString();
        }

        private static string GetSecurityRow(Security tab, bool isChild)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<tr id='{0}' class='{1} securityRow'>", tab.roleFunctionId, tab.parentFunctionId);
            if (tab.children == null)
            {
                sb.AppendFormat("<td style='padding-left: {0}px;'>{1}</td>", isChild ? "40" : "20", tab.functionName);
                sb.AppendFormat("<td align='left'><input type='checkbox' name='chkCanCreate' id='chkCanCreate' {0} {1} /></td>",
                                 tab.canCreate == 1 ? "checked='checked'" : "", tab.canCreate == -1 ? "disabled='true'" : "");

                sb.AppendFormat("<td align='left'><input type='checkbox' name='chkCanView' id='chkCanView' {0} {1} /></td>",
                                 tab.canView == 1 ? "checked='checked'" : "", tab.canView == -1 ? "disabled='true'" : "");

                sb.AppendFormat("<td align='left'><input type='checkbox' name='chkCanUpdate' id='chkCanUpdate' {0} {1} /></td>",
                                 tab.canUpdate == 1 ? "checked='checked'" : "", tab.canUpdate == -1 ? "disabled='true'" : "");

                sb.AppendFormat("<td align='left'><input type='checkbox' name='chkCanExport' id='chkCanExport' {0} {1} /></td>",
                                 tab.canExport == 1 ? "checked='checked'" : "", tab.canExport == -1 ? "disabled='true'" : "");

                sb.AppendFormat("<td align='left'><input type='checkbox' name='chkCanEmail' id='chkCanEmail' {0} {1} /></td>",
                                 tab.canEmail == 1 ? "checked='checked'" : "", tab.canEmail == -1 ? "disabled='true'" : "");
            }
            else
            {
                sb.AppendLine("<td colspan='6'>");
                sb.AppendFormat("<img src='../images/expand.png' alt='Expand' class='imgExpColl' id='{0}' />", tab.functionId);
                sb.AppendFormat("<img src='../images/collapse.png' alt='Collapse' class='imgExpColl' style='display:none' id='{0}' />  {1}</td>",
                                 tab.functionId, tab.functionName);
            }
            sb.AppendLine("</tr>");
            return sb.ToString();
        }

        public static void SaveSecurityData(List<Security> tabSecurity)
        {
            string update = "UPDATE {0} SET {1}=@CAN_CREATE,{2}=@CAN_UPDATE,{3}=@CAN_VIEW  WHERE  {4}=@ROLE_ID  and {5}=@ROLE_FUNC_ID ";
            update = string.Format(update, FieldNames.TABLE_NAME, FieldNames.CAN_CREATE, FieldNames.CAN_UPDATE, FieldNames.CAN_VIEW, FieldNames.ROLE_ID, FieldNames.ROLE_FUNC_ID);

            String con = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection db = new SqlConnection(con))
            {
                SqlTransaction transaction;
                db.Open();
                transaction = db.BeginTransaction();
                try
                {
                    foreach (Security secure in tabSecurity)
                    {
                        update = update.Replace("@CAN_CREATE", secure.canCreate.ToString()).Replace("@CAN_UPDATE", secure.canUpdate.ToString())
                            .Replace("@CAN_VIEW", secure.canView.ToString()).Replace("@ROLE_ID", secure.roleId.ToString()).Replace("@ROLE_FUNC_ID", secure.roleFunctionId.ToString());
                        new SqlCommand(update, db, transaction).ExecuteNonQuery();
                    }
                    transaction.Commit();
                    MAICacheManager.removeType(MAICacheManager.TypeNames.ROLE_MAINTENANCE);
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    throw new Exception("Unable to update user roles", ex);
                }
            }

        }

        public static DataTable GetFamilyAff()
        {
            String con = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection db = new SqlConnection(con))
            {
                string sqlQuery = "SELECT distinct FamilyAff from dbo.V_UniqueInvoiceTimingsByDealerID";
                SqlDataAdapter da = new SqlDataAdapter(sqlQuery, db);
                DataSet ds = new DataSet();
                try
                {
                    da.Fill(ds);
                    return ds.Tables[0];
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

        }

        #endregion

        #region New Methods


        public static List<string> GetUserReports(int UserId, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (FarmerBrothersEntitites == null)
            {
                FarmerBrothersEntitites = new FarmerBrothersEntities();
            }
            List<string> userReports = new List<string>();
            //List<FBUserReport> reports = FarmerBrothersEntitites.FBUserReports.Where(r => r.UserId == UserId).OrderBy(r => r.FBReport.report_name).ToList();

             var reports = (from t1 in FarmerBrothersEntitites.FBUserReports
                                        join t2 in FarmerBrothersEntitites.FBReports on t1.report_id equals t2.report_id
                                        where t1.UserId == UserId
                                        select new { t2.report_name }).ToList();
             //userReports = reports.ToList();
             foreach(var report in reports)
             {
                 userReports.Add(Convert.ToString(report.report_name));
             }

             return userReports;
        }
        public static Dictionary<string, string> GetUserPrivilegeByUserId(int userId, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (FarmerBrothersEntitites == null)
            {
                FarmerBrothersEntitites = new FarmerBrothersEntities();
            }
            Dictionary<string, string> userModel = new Dictionary<string, string>();
            //List<UserApplication> userapp = FarmerBrothersEntitites.UserApplications.Where(u => u.UserId == userId).ToList();
            List<Application> apps = FarmerBrothersEntitites.Applications.OrderBy(o => o.OrderId).Distinct().ToList();

            //foreach (UserApplication item in userapp)
            //{
            //    userModel.Add(item.Application.ApplicationName, item.Privilege.PrivilegeType);
            //}

            foreach (Application app in apps)
            {
                UserApplication userapp = FarmerBrothersEntitites.UserApplications.Where(u => u.UserId == userId && u.ApplicationId == app.ApplicationId).FirstOrDefault();

                if (userapp != null)
                {
                    userModel.Add(userapp.Application.ApplicationName, userapp.Privilege.PrivilegeType);
                }
                else
                {
                    userModel.Add(app.ApplicationName, "No-Permission");
                }
            }

            return userModel;
        }

        public static bool IsEmailIdExist(string emailId)
        {
            bool success = false;
            FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities();
            FbUserMaster user = FarmerBrothersEntitites.FbUserMasters.Where(u => u.Email == emailId).FirstOrDefault();
            if (user != null)
            {
                success = true;
            }
            return success;
        }
        #endregion
    }
}