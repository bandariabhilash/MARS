using FarmerBrothers.Data;
using FarmerBrothers.Models;
using LinqKit;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class ZoneTechnicianController : BaseController
    {
        // GET: ZoneTechnician
        public ActionResult UpdateZoneTechnician()
        {
            ZonePriorityModel zonePriority = new ZonePriorityModel();
            return View("UpdateZoneTechnician", zonePriority);
        }

        public JsonResult Search(ZonePriorityModel zonePriority)
        {
            if (string.IsNullOrWhiteSpace(zonePriority.OnCallGroup)
           && string.IsNullOrWhiteSpace(zonePriority.ResponsibletechID.ToString())
           && string.IsNullOrWhiteSpace(zonePriority.SecondaryTechID.ToString())
           && string.IsNullOrWhiteSpace(zonePriority.ZoneIndex.ToString())
           && string.IsNullOrWhiteSpace(zonePriority.ZoneName)
           && string.IsNullOrWhiteSpace(zonePriority.Fsm.ToString()))
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<ZonePriorityModel>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IEnumerable<ZonePriorityModel> zones = GetZones(zonePriority);
                zonePriority.SearchResults = zones;
                TempData["SearchCriteria"] = zonePriority;
                return Json(zonePriority.SearchResults, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public void ExcelExport()
        {
            ZonePriorityModel zonePriorityModel = new ZonePriorityModel();
            if (TempData["SearchCriteria"] != null)
            {
                zonePriorityModel = TempData["SearchCriteria"] as ZonePriorityModel;
            }
            else
            {
                zonePriorityModel.SearchResults = new List<ZonePriorityModel>();
            }

            string gridModel = HttpContext.Request.Params["GridModel"];
            GridProperties gridProperty = ConvertGridObject(gridModel);
            ExcelExport exp = new ExcelExport();
            exp.Export(gridProperty, zonePriorityModel.SearchResults, "ZoneResults.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        public ActionResult UpdateZoneTechnicians(int id)
        {
            ZonePriorityModel zonePriorityModel = new ZonePriorityModel();
            var zonePriority = FarmerBrothersEntitites.ZonePriorities.FirstOrDefault(zp => zp.ZoneIndex == id);
            if (zonePriority != null)
            {                
                zonePriorityModel.ZoneIndex = zonePriority.ZoneIndex;
                zonePriorityModel.ZoneName = zonePriority.ZoneName;
                zonePriorityModel.ResponsibletechID = zonePriority.ResponsibletechID;
                zonePriorityModel.SecondaryTechID = zonePriority.SecondaryTechID;
                zonePriorityModel.OnCallGroupID = zonePriority.OnCallGroupID;
                zonePriorityModel.Technicians = GetTechnicians(zonePriority);
                zonePriorityModel.OnCallGroupList = (from P in FarmerBrothersEntitites.OnCallGroups select new FarmerBrothers.Models.OnCallGroup { OnCallGroupID = P.OnCallGroupID, OnCallGroupName = P.OnCallGroup1 }).ToList();
                zonePriorityModel.ResponsibleTechBranch = (zonePriority.ResponsibleTechBranch);
                zonePriorityModel.Longitude = (zonePriority.Longitude);
                zonePriorityModel.Latitude = (zonePriority.Latitude);
                zonePriorityModel.OnCallPrimarytechID = zonePriority.OnCallPrimarytechID;
                zonePriorityModel.OnCallBackupTechID = zonePriority.OnCallBackupTechID;
            }
            return View("UpdateZoneTechnicians", zonePriorityModel);
        }

        [HttpPost]
        public ActionResult UpdateZoneTechnicians(ZonePriorityModel zonePriorityModel, int id)
        {
            var zonePriority = FarmerBrothersEntitites.ZonePriorities.First(n => n.ZoneIndex == id);
            if (!string.IsNullOrEmpty(zonePriorityModel.ResponsibletechID.ToString()))
            {
                zonePriority.ResponsibletechID = Convert.ToInt32(zonePriorityModel.ResponsibletechID);
                zonePriority.ResponsibleTechName = zonePriorityModel.ResponsibleTechName;
            }
            else
            {
                zonePriority.ResponsibletechID = null;
                zonePriority.ResponsibleTechName = null;
            }

            if (!string.IsNullOrEmpty(zonePriorityModel.SecondaryTechID.ToString()))
            {
                zonePriority.SecondaryTechID = Convert.ToInt32(zonePriorityModel.SecondaryTechID);
                zonePriority.SecondaryTechName = zonePriorityModel.SecondaryTechName;
            }
            else
            {
                zonePriority.SecondaryTechID = null;
                zonePriority.SecondaryTechName = null;
            }
                        

            if (!string.IsNullOrEmpty(zonePriorityModel.OnCallGroupID.ToString()))
            {
                zonePriority.OnCallGroupID = Convert.ToInt32(zonePriorityModel.OnCallGroupID);
                zonePriority.OnCallPrimarytechID = zonePriorityModel.OnCallPrimarytechID;
                zonePriority.OnCallBackupTechID = zonePriorityModel.OnCallBackupTechID;
            }
            else
            {
                zonePriority.OnCallGroupID = null;
                zonePriority.OnCallPrimarytechID = null;
                zonePriority.OnCallBackupTechID = null;
            }


            zonePriority.ResponsibleTechBranch = Convert.ToInt32(zonePriorityModel.ResponsibleTechBranch);
            zonePriority.Longitude = Convert.ToDouble(zonePriorityModel.Longitude);
            zonePriority.Latitude = Convert.ToDouble(zonePriorityModel.Latitude);
            zonePriority.Coordinates = zonePriorityModel.Latitude + ", " + zonePriorityModel.Longitude;

            FarmerBrothersEntitites.SaveChanges();
            zonePriorityModel.Technicians = GetTechnicians(zonePriority);
            zonePriorityModel.OnCallGroupList = (from P in FarmerBrothersEntitites.OnCallGroups select new FarmerBrothers.Models.OnCallGroup { OnCallGroupID = P.OnCallGroupID, OnCallGroupName = P.OnCallGroup1 }).ToList();
            return RedirectToAction("UpdateZoneTechnicians", "ZoneTechnician", new { id = zonePriority.ZoneIndex });
        }
        private IList<Technician> GetTechnicians(ZonePriority zonePriority)
        {            
            List<TECH_HIERARCHY> Techlist = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.SearchType == "SP").Where(x => x.FamilyAff != "SPT").OrderBy(x => x.CompanyName).ToList();
            Technician tech;
            List<Technician> technicians = new List<Technician>();
            foreach (var dr in Techlist)
            {
                tech = new Technician();
                tech.TechID = dr.DealerId.ToString();
                tech.TechName = dr.CompanyName;
                technicians.Add(tech);
            }

            return technicians;
        }

        private IEnumerable<ZonePriorityModel> GetZones(ZonePriorityModel zonePriority)
        {

            var query = (from zonepriority in FarmerBrothersEntitites.ZonePriorities
                         join
                         oncall in FarmerBrothersEntitites.OnCallGroups on zonepriority.OnCallGroupID equals oncall.OnCallGroupID into gj
                         from x in gj.DefaultIfEmpty()
                         select new ZonePriorityModel
                         {

                             ZoneIndex = zonepriority.ZoneIndex,
                             ZoneName = zonepriority.ZoneName,
                             ResponsibletechID = zonepriority.ResponsibletechID,
                             SecondaryTechID = zonepriority.SecondaryTechID,
                             OnCallGroupID = x.OnCallGroupID,
                             ResponsibleTechName = zonepriority.ResponsibleTechName,
                             SecondaryTechName = zonepriority.SecondaryTechName,
                             ResponsibleTechBranch = (zonepriority.ResponsibleTechBranch),
                             Longitude = (zonepriority.Longitude),
                             Latitude = (zonepriority.Latitude),
                             OnCallGroup = x.OnCallGroup1,
                             OnCallPrimarytechID = zonepriority.OnCallPrimarytechID,
                             OnCallBackupTechID = zonepriority.OnCallBackupTechID,
                             Fsm = zonepriority.Fsm
                         });

            var predicate = PredicateBuilder.True<ZonePriorityModel>();


            if (!string.IsNullOrWhiteSpace(zonePriority.ResponsibletechID.ToString()))
            {
                predicate = predicate.And(w => w.ResponsibletechID == zonePriority.ResponsibletechID);
            }

            if (!string.IsNullOrWhiteSpace(zonePriority.ResponsibleTechName))
            {
                predicate = predicate.And(w => w.ResponsibleTechName.ToString().Contains(zonePriority.ResponsibleTechName));
            }

            if (!string.IsNullOrWhiteSpace(zonePriority.SecondaryTechID.ToString()))
            {
                predicate = predicate.And(w => w.SecondaryTechID == zonePriority.SecondaryTechID);
            }

            if (!string.IsNullOrWhiteSpace(zonePriority.SecondaryTechName))
            {
                predicate = predicate.And(w => w.SecondaryTechName.ToString().Contains(zonePriority.SecondaryTechName));
            }

            if (!string.IsNullOrWhiteSpace(zonePriority.OnCallGroup))
            {
                predicate = predicate.And(w => w.OnCallGroup.Contains(zonePriority.OnCallGroup));
            }

            if (!string.IsNullOrWhiteSpace(zonePriority.Fsm.ToString()))
            {
                predicate = predicate.And(w => w.Fsm == zonePriority.Fsm);
            }

            if (!string.IsNullOrWhiteSpace(zonePriority.ZoneIndex.ToString()))
            {
                predicate = predicate.And(w => w.ZoneIndex == zonePriority.ZoneIndex);
            }

            if (!string.IsNullOrWhiteSpace(zonePriority.ZoneName))
            {
                predicate = predicate.And(w => w.ZoneName.Contains(zonePriority.ZoneName));
            }

            return query.AsExpandable().Where(predicate).ToList();
        }
    }
}