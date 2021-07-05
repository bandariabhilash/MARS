using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class ClosestTechLookupController : BaseController
    {
        [HttpGet]
        public ActionResult ClosestTechLookup()
        {
            ClosestTechLookupModel techModel = new ClosestTechLookupModel();
            techModel.Branches = new List<BranchModel>();
            return View("ClosestTechLookup",techModel);
        }
        // GET: ClosestTechLookup
        [HttpGet]
        public ActionResult ClosestTechLookupByZipCode(string ZipCode)
        {
            ClosestTechLookupModel techModel = new ClosestTechLookupModel();            
            IList<TechDispatchWithDistance> dispatchBranches = null;
            dispatchBranches = Utility.GetClosestTechDispatchWithDistance(FarmerBrothersEntitites, ZipCode).ToList();
            IList<BranchModel> branches = new List<BranchModel>();
            foreach (TechDispatchWithDistance dispatchBranch in dispatchBranches)
            {
                branches.Add(new BranchModel(dispatchBranch));
            }

            techModel.Branches = branches;
            techModel.ZipCode = ZipCode;

            #region this code to get on call tech details 
            //DateTime currentDateTime = Utility.GetCurrentTime(ZipCode, FarmerBrothersEntitites);
            //int isDateHolidayWeekend = Utility.IsDateHolidayWeekend(currentDateTime.ToString("MM/dd/yyyy"), FarmerBrothersEntitites);

            //int OnCallStartTime = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallStartTime"]);
            //int OnCallStartTimeMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallStartTimeMinutes"]);
            //int OnCallEndTime = Convert.ToInt32(ConfigurationManager.AppSettings["OnCallEndTime"]);

            //DateTime OnCallAfterStartTime = Convert.ToDateTime(currentDateTime.ToString("MM-dd-yyyy")).AddHours(OnCallStartTime).AddMinutes(OnCallStartTimeMinutes);
            //DateTime OnCallAfterEndTime = Convert.ToDateTime(currentDateTime.ToString("MM-dd-yyyy")).AddHours(OnCallEndTime);
            //int OnCallTechId = 0;
            //if (isDateHolidayWeekend == 1 || isDateHolidayWeekend == 2 ||
            //    (currentDateTime > OnCallAfterStartTime || currentDateTime < OnCallAfterEndTime))
            //{
            //    OnCallTechId = WorkorderController.GetAfterHoursClosestOnCallTechIdByZip(ZipCode);
            //}
            //else
            //{
            //    OnCallTechId = WorkorderController.GetClosestOnCallTechIdByZip(ZipCode);
            //}
            //bool isafterHours = false;
            ////1=Weekend ; 2=Holiday
            ////After WorkHours = 4:30PM to 7AM
            //if (isDateHolidayWeekend == 1 || isDateHolidayWeekend == 2 ||
            //    (currentDateTime > OnCallAfterStartTime || currentDateTime < OnCallAfterEndTime))
            //{

            //    if (OnCallTechId != 0)
            //    {
            //        techModel.Branches = new List<BranchModel>();
            //        TechDispatchWithDistance OnCalldispatchBranches = new TechDispatchWithDistance();
            //        IList<TechDispatchWithDistance> afterHoursdispatchBranches = Utility.GetAfterHoursClosestTechDispatchWithDistance(FarmerBrothersEntitites, ZipCode).ToList();
            //        OnCalldispatchBranches = afterHoursdispatchBranches.Where(t => t.ServiceCenterId == OnCallTechId).FirstOrDefault();
            //        if (OnCalldispatchBranches != null)
            //        {
            //            techModel.Branches.Add(new BranchModel(OnCalldispatchBranches));
            //            isafterHours = true;
            //        }
            //    }

            //}
            #endregion

            return View("ClosestTechLookup", techModel);
        }

        public JsonResult IsZipCodeValid(string Zipcode)
        {
            JsonResult jsonResult = new JsonResult();

            var zip = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == Zipcode).FirstOrDefault();
            if (zip == null)
            {
                jsonResult.Data = new { success = false, serverError = ErrorCode.SUCCESS, message = "|Please Enter Valid Zipcode!" };
            }
            else
            {
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, message = "" };
            }

            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
    }
}