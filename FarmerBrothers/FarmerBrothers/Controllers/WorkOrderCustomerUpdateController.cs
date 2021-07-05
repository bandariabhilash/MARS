using FarmerBrothers.Data;
using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class WorkOrderCustomerUpdateController : BaseController
    {
        [HttpGet]
        public ActionResult WorkOrderCustomerUpdate()
        {
            WorkorderSearchModel workorderModel = new WorkorderSearchModel();
            workorderModel.WorkOrderResults = new List<WorkOrder>();
            return View(workorderModel);
        }
        [HttpPost]
        public ActionResult WorkOrderCustomerUpdate(string workOrderId)
        {
            bool result = false;
            WorkorderSearchModel workorderModel = new WorkorderSearchModel();
            workorderModel.SearchResults = new List<WorkorderSearchResultModel>();
            if (!string.IsNullOrEmpty(workOrderId))
            {
                workorderModel.SearchResults.Add(FarmerBrothers.Business.WOCustomerUpdate.GetWorkOrderDetails(Convert.ToInt32(workOrderId), out result));
            }           
            
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = result, data = workorderModel.SearchResults };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetCustomerDetails(string custID)
        {
            WorkorderSearchResultModel customerModel = new WorkorderSearchResultModel();
            if (!string.IsNullOrWhiteSpace(custID))
            {
                customerModel = FarmerBrothers.Business.WOCustomerUpdate.CustomerDetails(custID);
            }
            return Json(customerModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult WorkOrderUpdate(string custID, string workOrderId)
        {
            bool result = false;
            WorkorderSearchResultModel customerModel = new WorkorderSearchResultModel();
            if (!string.IsNullOrWhiteSpace(custID) && !string.IsNullOrWhiteSpace(workOrderId))
            {
                customerModel = FarmerBrothers.Business.WOCustomerUpdate.WoCustomerUpdate(Convert.ToInt32(custID), Convert.ToInt32(workOrderId), UserName, out result);
            }                      

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = result };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }




    }
}