using Newtonsoft.Json;
using FarmerBrothers.Data;
using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
      //  [AuthorizeAD(Groups = "TPSP Contract,MARS Administration")]
    public class ThirdPartyController : BaseController
    {
        // GET: ThirdParty
        public ActionResult ThirdPartyMaintenance()
        {
            ThirdPartyMaintenanceModel objThirdPartyModel = new ThirdPartyMaintenanceModel();
            try
            {
                ModelState.Clear();
                if (TempData["ThirdPartMaintenanceData"] != null)
                {
                    objThirdPartyModel = (ThirdPartyMaintenanceModel)TempData["ThirdPartMaintenanceData"];
                }
                if (objThirdPartyModel.ThirdPartyList == null)
                    objThirdPartyModel.ThirdPartyList = fnThirdPartyList();
                ViewData["BasedOnDropdown"] = fnBasedonList(objThirdPartyModel);
                return View(objThirdPartyModel);
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "objThirdPartyModel", "ThirdPartyMaintenance"));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
                //throw ex;
            }

        }

        [HttpPost]
        public ActionResult GetThirdPartyDataByID(int IntThirdPartyID)
        {
            try
            {
                ModelState.Clear();
                ThirdPartyMaintenanceModel objThirdPartyModel = new ThirdPartyMaintenanceModel();
                objThirdPartyModel.ThirdPartyZoneRatesList = GetThirdPartyZoneRatesByID(IntThirdPartyID);
                var objThirdPartyEntity = (from ThirdPartyEntity in FarmerBrothersEntitites.ThirdPartyContractMaintenances
                                           where ThirdPartyEntity.Techid == IntThirdPartyID
                                           select ThirdPartyEntity).ToList().FirstOrDefault();

                if (objThirdPartyEntity != null)
                {
                    objThirdPartyModel.ContractMaintenanceid = objThirdPartyEntity.ContractMaintenanceid;
                    objThirdPartyModel.LaborHourlyRate = objThirdPartyEntity.LaborHourlyRate;
                    objThirdPartyModel.LaborOvertimeRate = objThirdPartyEntity.LaborOvertimeRate;
                    objThirdPartyModel.MinOneHourFlag = (objThirdPartyEntity.MinOneHourFlag.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.MinOneHourFlag) : false;
                    objThirdPartyModel.PartsUpCharge = objThirdPartyEntity.PartsUpCharge;
                    objThirdPartyModel.RatePerPallet = objThirdPartyEntity.RatePerPallet;
                    objThirdPartyModel.RatePerPalletFlag = (objThirdPartyEntity.RatePerPalletFlag.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.RatePerPalletFlag) : false;
                    //objThirdPartyModel.MinOneHourFlag = (objThirdPartyEntity.MinOneHourFlag.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.MinOneHourFlag.HasValue) : false;
                    objThirdPartyModel.TravelOvertimeRate = objThirdPartyEntity.TravelOvertimeRate;
                    objThirdPartyModel.TravelOverTimeRateFlag = (objThirdPartyEntity.TravelOverTimeRateFlag.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.TravelOverTimeRateFlag) : false;
                    objThirdPartyModel.TravelRatePerMile = objThirdPartyEntity.TravelRatePerMile;
                    objThirdPartyModel.TravelRatePerMileFlag = (objThirdPartyEntity.TravelRatePerMileFlag.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.TravelRatePerMileFlag) : false;
                    objThirdPartyModel.TravelZoneRateFlag = (objThirdPartyEntity.TravelZoneRateFlag.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.TravelZoneRateFlag) : false;
                    objThirdPartyModel.TravelHourlyRateFlag = (objThirdPartyEntity.TravelHourlyRateFlag.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.TravelHourlyRateFlag) : false;
                    objThirdPartyModel.TravelHourlyRate = objThirdPartyEntity.TravelHourlyRate;
                    //objThirdPartyModel.TravelAllowRoundTripFlag = (objThirdPartyEntity.TravelAllowRoundTripFlag.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.TravelAllowRoundTripFlag.HasValue) : false;
                    objThirdPartyModel.TravelAllowRoundTripFlag = (objThirdPartyEntity.TravelAllowRoundTripFlag.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.TravelAllowRoundTripFlag) : false;
                    objThirdPartyModel.TravelMinOneHour = (objThirdPartyEntity.TravelMinOneHour.HasValue) ? Convert.ToBoolean(objThirdPartyEntity.TravelMinOneHour) : false;
                }

                objThirdPartyModel.ThirdPartyList = fnThirdPartyList();
                objThirdPartyModel.Techid = IntThirdPartyID;

                TempData["ThirdPartMaintenanceData"] = objThirdPartyModel;
            }
            catch (Exception ex)
            {
                // log the exception
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("ThirdPartyMaintenance", "ThirdParty");
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [HandleError(ExceptionType = typeof(DbUpdateException), View = "Error")]
        public ActionResult ThirdPartyMaintenanceInsertion([ModelBinder(typeof(ThirdPartyMaintenanceModelBinder))]  ThirdPartyMaintenanceModel thirdPartyManagement)
        {
            try
            {
                ThirdPartyContractMaintenance thirdPartyContractMaintenanceEntity = this.FarmerBrothersEntitites.ThirdPartyContractMaintenances.Where(i => i.Techid == thirdPartyManagement.Techid).FirstOrDefault();

                if (thirdPartyContractMaintenanceEntity != null)
                {
                    FarmerBrothersEntitites.ThirdPartyContractMaintenances.Attach(fninsertThirdParty(thirdPartyContractMaintenanceEntity, thirdPartyManagement));
                    FarmerBrothersEntitites.Entry(thirdPartyContractMaintenanceEntity).State = System.Data.Entity.EntityState.Modified;
                    fnInsertThirdPartyZoneRates(thirdPartyManagement);
                    FarmerBrothersEntitites.SaveChanges();
                    TempData["StatusMessage"] = "Third Party Sucessfully Updated for " + fnThirdPartyList().Where(c => c.ThirdPartyId == thirdPartyManagement.Techid).Select(c => c.ThirdPartyName).FirstOrDefault().ToString();
                    return RedirectToAction("ThirdPartyMaintenance", "ThirdParty");
                }
                else
                {
                    ThirdPartyContractMaintenance thirdPartyContractMaintanceEntity = new ThirdPartyContractMaintenance();
                    FarmerBrothersEntitites.ThirdPartyContractMaintenances.Add(fninsertThirdParty(thirdPartyContractMaintanceEntity, thirdPartyManagement));
                    FarmerBrothersEntitites.SaveChanges();
                    fnInsertThirdPartyZoneRates(thirdPartyManagement, false);

                    FarmerBrothersEntitites.SaveChanges();
                    TempData["StatusMessage"] = "Third Party Sucessfully Inserted for " + fnThirdPartyList().Where(c => c.ThirdPartyId == thirdPartyManagement.Techid).Select(c => c.ThirdPartyName).FirstOrDefault().ToString();
                    return RedirectToAction("ThirdPartyMaintenance", "ThirdParty");
                }
            }
            catch (Exception ex)
            {
                // throw ex;
                // log the exception
                TempData["StatusMessage"] = "Insertion Failed.";
                return RedirectToAction("ThirdPartyMaintenance", "ThirdParty");
            }
        }

        public void fnInsertThirdPartyZoneRates(ThirdPartyMaintenanceModel thirdPartyManagement, bool IsUpdate = true)
        {
            var thirdPartyZoneRates = thirdPartyManagement.TravelZoneRateFlag == true ? JsonConvert.DeserializeObject<List<ThirdPartyContractMaintenanceZoneRate>>(thirdPartyManagement.thirdpartyzonerates) : null;
            if (thirdPartyZoneRates != null)
            {
                fnDeleteThirdPartyZoneRates(thirdPartyManagement);
                foreach (ThirdPartyContractMaintenanceZoneRate zone in thirdPartyZoneRates)
                {
                   ThirdpartyConMaintenanceZonerate zoneRate = new ThirdpartyConMaintenanceZonerate();
                    zoneRate.ContractMaintenanceid = FarmerBrothersEntitites.ThirdPartyContractMaintenances.Single(c => c.Techid == thirdPartyManagement.Techid).ContractMaintenanceid;
                    zoneRate.Techid = thirdPartyManagement.Techid;
                    zoneRate.Description = zone.Description;
                    zoneRate.BasedOn = zone.BasedOn;
                    zoneRate.Rate = zone.Rate;
                    FarmerBrothersEntitites.ThirdpartyConMaintenanceZonerates.Add(zoneRate);
                }
            }
            if (IsUpdate)
            {
                fnDeleteThirdPartyZoneRates(thirdPartyManagement);
            }
        }

        public void fnDeleteThirdPartyZoneRates(ThirdPartyMaintenanceModel thirdPartyManagement)
        {
           FarmerBrothersEntitites.ThirdpartyConMaintenanceZonerates.RemoveRange(FarmerBrothersEntitites.ThirdpartyConMaintenanceZonerates.Where(x => x.Techid == thirdPartyManagement.Techid));
        }

        public ThirdPartyContractMaintenance fninsertThirdParty(ThirdPartyContractMaintenance thirdPartyContractMaintenanceEntity, ThirdPartyMaintenanceModel thirdPartyManagement)
        {
            thirdPartyContractMaintenanceEntity.LaborHourlyRate = thirdPartyManagement.LaborHourlyRate;
            thirdPartyContractMaintenanceEntity.LaborOvertimeRate = thirdPartyManagement.LaborOvertimeRate;
            thirdPartyContractMaintenanceEntity.MinOneHourFlag = thirdPartyManagement.MinOneHourFlag;
            thirdPartyContractMaintenanceEntity.PartsUpCharge = thirdPartyManagement.PartsUpCharge;
            thirdPartyContractMaintenanceEntity.RatePerPalletFlag = thirdPartyManagement.RatePerPalletFlag;
            thirdPartyContractMaintenanceEntity.RatePerPallet = thirdPartyManagement.RatePerPalletFlag == true ? thirdPartyManagement.RatePerPallet : null;
            thirdPartyContractMaintenanceEntity.TravelRatePerMileFlag = thirdPartyManagement.TravelRatePerMileFlag;
            thirdPartyContractMaintenanceEntity.TravelRatePerMile = thirdPartyManagement.TravelRatePerMileFlag == true ? thirdPartyManagement.TravelRatePerMile : null;
            thirdPartyContractMaintenanceEntity.TravelAllowRoundTripFlag = thirdPartyManagement.TravelAllowRoundTripFlag;
            thirdPartyContractMaintenanceEntity.TravelMinOneHour = thirdPartyManagement.TravelMinOneHour;
            thirdPartyContractMaintenanceEntity.TravelHourlyRateFlag = thirdPartyManagement.TravelHourlyRateFlag;
            thirdPartyContractMaintenanceEntity.TravelHourlyRate = thirdPartyManagement.TravelHourlyRateFlag == true ? thirdPartyManagement.TravelHourlyRate : null;
            thirdPartyContractMaintenanceEntity.TravelOverTimeRateFlag = thirdPartyManagement.TravelOverTimeRateFlag;
            thirdPartyContractMaintenanceEntity.TravelOvertimeRate = thirdPartyManagement.TravelOverTimeRateFlag == true ? thirdPartyManagement.TravelOvertimeRate : null;
            thirdPartyContractMaintenanceEntity.TravelZoneRateFlag = thirdPartyManagement.TravelZoneRateFlag;
            thirdPartyContractMaintenanceEntity.Techid = thirdPartyManagement.Techid;
            return thirdPartyContractMaintenanceEntity;
        }

        public List<ThirdpartyConMaintenanceZonerate> GetThirdPartyZoneRatesByID(int intThirdPartyId)
        {
            return this.FarmerBrothersEntitites.ThirdpartyConMaintenanceZonerates.Where(m => m.Techid == intThirdPartyId).ToList();            
        }

        public List<object> fnBasedonList(ThirdPartyMaintenanceModel objThirdPartyModel)
        {
            var BasedOnList = new List<object>();
            foreach (var item in objThirdPartyModel.ThirdPartyBasedOnList)
            {
                BasedOnList.Add(new { value = item, text = item });
            }
            return BasedOnList;
        }

        public List<ThirdParty> fnThirdPartyList()
        {

            MarsViews objMarsView = new MarsViews();
            string StrSql = string.Empty;
            StrSql = "Select tech_id , tech_name from feast_tech_hierarchy where tech_desc='TPSP Vendor' Order By tech_name";
            List<ThirdParty> thirdPartyList = new List<ThirdParty>();
            foreach (DataRow dr in objMarsView.fnTpspVendors(StrSql).Rows)
            {
                ThirdParty thrdParty = new ThirdParty();
                thrdParty.ThirdPartyId = Convert.ToInt32(dr["tech_id"].ToString());
                thrdParty.ThirdPartyName = dr["tech_name"].ToString();
                thirdPartyList.Add(thrdParty);
            }
            return thirdPartyList.OrderBy(x => x.ThirdPartyName).ToList(); ;
        }

    }
}