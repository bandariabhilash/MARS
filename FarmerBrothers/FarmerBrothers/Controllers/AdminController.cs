using FarmerBrothers.Data;
using FarmerBrothers.Models;
using LinqKit;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class BusinessObject
    {

        public int roleId { get; set; }
        public int functionId { get; set; }
        public int? parentFunctionId { get; set; }
        public string functionName { get; set; }
        public string parentFunctionName { get; set; }
        public int level { get; set; }

        public Access canCreate { get; set; }
        public Access canEdit { get; set; }
        public Access canDelete { get; set; }
        public Access canView { get; set; }
        public Access canEmail { get; set; }
        public Access canExport { get; set; }

        public List<BusinessObject> Children { get; set; }

    }
    public class AdminController : BaseController
    {
        public ActionResult UserRoles(int roleId = 101)
        {
            string roles = Security.GetTabSecurityHtml(roleId, FarmerBrothersEntitites);
            UserRole role = new UserRole(roles, roleId.ToString());
            return View(role);
        }

        public ActionResult UpdateSecurityData(List<Security> security)
        {
            Security.SaveSecurityData(security);

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = false, Url = "" };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
    }
}