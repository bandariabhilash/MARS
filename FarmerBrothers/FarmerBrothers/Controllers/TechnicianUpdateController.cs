using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Script.Serialization;


namespace FarmerBrothers.Controllers
{
    public class TechnicianUpdateController : BaseController
    {
        //
        // GET: /TechnicianUpdate/
        public ActionResult TechnicianUpdate()
        {
            TechnicianUpdateModel model = new TechnicianUpdateModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddTechnician(TechnicianUpdateModel techModel)
        {
            bool result = false;
            string message = string.Empty;
            result = FarmerBrothers.Business.TechnicianUpdate.AddTechnician(techModel,out message);            
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = result, serverError = 1, message=message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        [HttpPost]
        public ActionResult TechnicianUpdate(TechnicianUpdateModel techModel)
        {
            TempData["TecnicianSearchCriteria"] = techModel;
            List<TechnicianUpdateModel>  SearchResults= FarmerBrothers.Business.TechnicianUpdate.GetTechnicianDetails(techModel);
            ViewBag.datasource = SearchResults;
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = 1, data = SearchResults };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ClearSearchResults()
        {

           return Json(new List<TechnicianUpdateModel>(), JsonRequestBehavior.AllowGet);
                       
        }

        [HttpPost]
        public ActionResult TechnicianDetailsUpdate(TechnicianUpdateModel techModel)
        {
            string message = string.Empty;
            ErrorCode code = FarmerBrothers.Business.TechnicianUpdate.UpdateTechnicianDetail(techModel, out message);
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { serverError = code, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpPost]
        public FileResult ExcelExportTechnicianResults()
        {
            TechnicianUpdateModel techModel = new TechnicianUpdateModel();
            List<TechnicianUpdateModel> searchResults = new List<TechnicianUpdateModel>();

            string gridModel = HttpContext.Request.Params["GridModel"];

            if (TempData["TecnicianSearchCriteria"] != null)
            {
                techModel = TempData["TecnicianSearchCriteria"] as TechnicianUpdateModel;
                List<TechnicianUpdateModel> SearchResults = FarmerBrothers.Business.TechnicianUpdate.GetTechnicianDetails(techModel);
                ViewBag.datasource = SearchResults;
                searchResults = SearchResults;
            }

            TempData["TecnicianSearchCriteria"] = techModel;
                       
            string[] columns = {"TechId","TechName","City","State","PhoneNumber","EmailCC","RimEmail","Zip","FamilyAff","BranchName","ParentTechnicianName"};
            byte[] filecontent = ExcelExportHelper.ExportExcel(searchResults, "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "TechnicianResults.xlsx");

        }
    }
}