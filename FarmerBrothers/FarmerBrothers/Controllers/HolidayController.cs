using FarmerBrothers.Data;
using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class HolidayController : BaseController
    {
        // GET: Holiday
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HolidayList()
        {
            HolidayModel objHoliday = new HolidayModel();
            List<Holiday> Holidaylist = new List<Holiday>();
            Holidaylist = (from t in FarmerBrothersEntitites.HolidayLists select new Holiday { HolidayDate = t.HolidayDate, HolidayName = t.HolidayName, HolidayUniqueId = t.UniqueID, Status = t.Status }).AsEnumerable().ToList().Select(c => { c.HolidayDatestring = c.HolidayDate.Value.ToShortDateString(); return c; }).ToList();
            objHoliday.Years = Holidaylist.OrderByDescending(c => c.HolidayDate).Select(c => c.HolidayDate.Value.Year).Distinct().ToList();
            Holidaylist = Holidaylist.Where(c => c.HolidayDate.Value.Year == DateTime.Now.Year).ToList();
            objHoliday.year = DateTime.Now.Year.ToString();
            objHoliday.SearchResults = Holidaylist;
            return View(objHoliday);
        }
        [HttpPost]
        public JsonResult InsertHolidayList(string HolidayName, DateTime HolidayDate, int? year, bool HolidayStatus)
        {
            string message = string.Empty;
            List<Holiday> Holidaylist = new List<Holiday>();
            try
            {
                if (!IsHolidayExist(HolidayDate))
                {
                    if (ModelState.IsValid)
                    {
                        HolidayList holiday = new HolidayList()
                        {
                            HolidayDate = HolidayDate,
                            HolidayName = HolidayName,
                            Status = HolidayStatus
                        };
                        FarmerBrothersEntitites.HolidayLists.Add(holiday);
                        FarmerBrothersEntitites.SaveChanges();
                        message = "Successfully created Holiday!";
                    }
                }
                else
                {
                    message = "Holiday is already exist, Can not create holiday on same date!";
                }
                if (!year.HasValue)
                {
                    year = HolidayDate.Year;
                }
                Holidaylist = (from t in FarmerBrothersEntitites.HolidayLists where t.HolidayDate.Value.Year == year select new Holiday { HolidayDate = t.HolidayDate, HolidayName = t.HolidayName, HolidayUniqueId = t.UniqueID, Status = t.Status }).AsEnumerable().ToList().Select(c => { c.HolidayDatestring = c.HolidayDate.Value.ToShortDateString(); return c; }).ToList();
                IList<int> years = Holidaylist.OrderByDescending(c => c.HolidayDate).Select(c => c.HolidayDate.Value.Year).Distinct().ToList();
                
            }
            catch (Exception)
            {
                message = "There is a problem in holiday creation!";
            }
            
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { holidayList = Holidaylist, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;            
        }
        public JsonResult SortHolidayList(int year)

        {
            List<Holiday> Holidaylist = new List<Holiday>();
            Holidaylist = (from t in FarmerBrothersEntitites.HolidayLists where t.HolidayDate.Value.Year == year select new Holiday { HolidayDate = t.HolidayDate, HolidayName = t.HolidayName, HolidayUniqueId = t.UniqueID, Status = t.Status }).AsEnumerable().ToList().Select(c => { c.HolidayDatestring = c.HolidayDate.Value.ToShortDateString(); return c; }).ToList();
            return Json(Holidaylist, JsonRequestBehavior.AllowGet);

        }

        public JsonResult UpdateHolidayList(string HolidayName, DateTime HolidayDate, int UniqueId, int year, bool HolidayStatus)
        {
            string message = string.Empty;
            List<Holiday> Holidaylist = new List<Holiday>();
            try
            {
                if (!IsHolidayExistToUpdate(HolidayDate, UniqueId))
                {
                    HolidayList holidayitem = FarmerBrothersEntitites.HolidayLists.Single(c => c.UniqueID == (UniqueId));
                    holidayitem.HolidayName = HolidayName;
                    holidayitem.HolidayDate = HolidayDate;
                    holidayitem.Status = HolidayStatus;
                    FarmerBrothersEntitites.HolidayLists.Attach(holidayitem);
                    FarmerBrothersEntitites.Entry(holidayitem).State = System.Data.Entity.EntityState.Modified;
                    FarmerBrothersEntitites.SaveChanges();
                    message = "Successfully updated Holiday!";
                    
                }
                else
                {
                    message = "Holiday is already exist, Can not update holiday on same date!";
                }
                Holidaylist = (from t in FarmerBrothersEntitites.HolidayLists where t.HolidayDate.Value.Year == year select new Holiday { HolidayDate = t.HolidayDate, HolidayName = t.HolidayName, HolidayUniqueId = t.UniqueID, Status = t.Status }).AsEnumerable().ToList().Select(c => { c.HolidayDatestring = c.HolidayDate.Value.ToShortDateString(); return c; }).ToList();
            }
            catch (Exception)
            {
                message = "There is a problem in Holiday Update!";
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { holidayList = Holidaylist, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;        

            

        }

        public bool IsHolidayExist(DateTime HolidayDate)
        {
            bool isExist = false;

            var holiday = (from holday in FarmerBrothersEntitites.HolidayLists
                       where DbFunctions.TruncateTime(holday.HolidayDate) == DbFunctions.TruncateTime(HolidayDate.Date)
                       select holday).FirstOrDefault();

            if (holiday!=null)
            {
                isExist = true;
            }

            return isExist;
        }

        public bool IsHolidayExistToUpdate(DateTime HolidayDate, int UniqueId)
        {
            bool isExist = false;

            var holiday = (from holday in FarmerBrothersEntitites.HolidayLists
                           where DbFunctions.TruncateTime(holday.HolidayDate) == DbFunctions.TruncateTime(HolidayDate.Date) && holday.UniqueID != UniqueId
                           select holday).FirstOrDefault();

            if (holiday != null)
            {
                isExist = true;
            }

            return isExist;
        }
    }

}