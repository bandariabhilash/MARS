using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{

    public class TechnicianCalendarController : BaseController
    {
        MarsViews objMarsview = new MarsViews();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Calendar()
        {

            string strQuery = @"SELECT tech.BranchNumber, tech.BranchName
                                FROM TECH_HIERARCHY tech where tech.BranchName !='' and searchType='SP' and FamilyAff !='SPT'
                                GROUP BY tech.BranchNumber, tech.BranchName";

            DataTable dt = objMarsview.fn_FSM_View(strQuery);
            CalendarTechnicianModel objTechModel = new CalendarTechnicianModel();
            List<Branch> Branchlists = new List<Branch>();
            List<CallGroup> OnCallList = new List<CallGroup>();
            foreach (DataRow dr in dt.Rows)
            {
                Branchlists.Add
               (
                    new Branch()
                    {
                        BranchID = dr["BranchNumber"].ToString(),
                        BranchName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dr["BranchName"].ToString().ToLower())

                    }
         );
            }
            // strQuery = "Select  * from  OnCallGroup";
            //DataTable dtOnCall = objMarsview.fn_FSM_View(strQuery);
            foreach (DataRow dr in dt.Rows)
            {
                OnCallList.Add
               (
                    new CallGroup()
                    {
                        OnCallGroupID = dr["BranchNumber"].ToString(),
                        OnCallGroupName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dr["BranchName"].ToString().ToLower())
                    }
         );
            }

            objTechModel.BranchList = Branchlists;
            objTechModel.OnCallList = OnCallList;

            string onCallStartTime = ConfigurationManager.AppSettings["OnCallStartTime"];
            string onCallEndTime = ConfigurationManager.AppSettings["OnCallEndTime"];
            string onCallStartTimeMinutes = ConfigurationManager.AppSettings["OnCallStartTimeMinutes"];
            string onCallEndTimeMinutes = ConfigurationManager.AppSettings["OnCallEndTimeMinutes"];

            if (!string.IsNullOrWhiteSpace(onCallStartTime))
            {
                objTechModel.OnCallStartTime = onCallStartTime + ":" + onCallStartTimeMinutes;
            }

            if (!string.IsNullOrWhiteSpace(onCallEndTime))
            {
                objTechModel.OnCallEndTime = onCallEndTime + ":" + onCallEndTimeMinutes;
            }

            return View(objTechModel);
        }
        [HttpPost]
        public ActionResult GetTechniciansByBranchId(string strBranchID, bool IsTechSchedule, string TimeZone)
        {

            try
            {
                ModelState.Clear();
                int OnCallTechId = 0;
                string Techtype = strBranchID.Contains('/') ? strBranchID.Split('/')[1] : strBranchID;
                string strQuery = string.Empty;
                //string strInternalQuery = " Select Tech_Id,Tech_Name from feast_tech_hierarchy where default_Service_center = " + Convert.ToInt64(strBranchID.Split('/')[0]) + "and tech_desc in ('Team Lead','Service Tech')";
                //string strTpspQuery = "  Select Tech_Id,Tech_Name  from feast_tech_hierarchy where SERVICECENTER_DESC  = 'TPSP Branch' and TEAMLEAD_ID = " + Convert.ToInt64(strBranchID.Split('/')[0]) + "Order By SERVICECENTER_NAME";
                //strQuery = Techtype == "INT" ? strInternalQuery : Techtype == "TPSP" ? strTpspQuery : fnOnCallQuery(Techtype, OnCallTechId);
                strQuery = @"SELECT tech.Dealerid as Tech_Id, tech.CompanyName as Tech_Name from 
                            TECH_HIERARCHY tech where tech.BranchName !='' and BranchNumber = '" + strBranchID + "' and searchType='SP' and FamilyAff !='SPT' GROUP BY tech.Dealerid, tech.CompanyName";

                CalendarTechnicianModel objTechnicianModel = new CalendarTechnicianModel();
                objTechnicianModel.TimeZone = TimeZone;
                objTechnicianModel.ResourceList = fnResourceList(strQuery);
                objTechnicianModel.IsTechSchedule = IsTechSchedule;

                string onCallStartTime = "";
                string onCallEndTime = "";
                string onCallStartTimeMinutes = "";
                string onCallEndTimeMinutes = "";

                if (!objTechnicianModel.IsTechSchedule)
                {
                    onCallStartTime = ConfigurationManager.AppSettings["OnCallStartTime"];
                    onCallEndTime = ConfigurationManager.AppSettings["OnCallEndTime"];
                    onCallStartTimeMinutes = ConfigurationManager.AppSettings["OnCallStartTimeMinutes"];
                    onCallEndTimeMinutes = ConfigurationManager.AppSettings["OnCallEndTimeMinutes"];
                }
                else
                {
                    onCallStartTime = ConfigurationManager.AppSettings["OnTechStartTime"];
                    onCallEndTime = ConfigurationManager.AppSettings["OnTechEndTime"];
                    onCallStartTimeMinutes = ConfigurationManager.AppSettings["OnTechStartTimeMinutes"];
                    onCallEndTimeMinutes = ConfigurationManager.AppSettings["OnTechEndTimeMinutes"];
                }

                if (!string.IsNullOrWhiteSpace(onCallStartTime))
                {
                    objTechnicianModel.OnCallStartTime = Convert.ToInt32(onCallStartTime) > 12 ? (Convert.ToInt32(onCallStartTime) - 12).ToString() + ":" + onCallStartTimeMinutes + " PM" : Convert.ToInt32(onCallStartTime) + ":" + onCallEndTimeMinutes + " AM";
                }

                if (!string.IsNullOrWhiteSpace(onCallEndTime))
                {
                    objTechnicianModel.OnCallEndTime = Convert.ToInt32(onCallEndTime) > 12 ? (Convert.ToInt32(onCallEndTime) - 12).ToString() + ":" + onCallEndTimeMinutes + " PM" : Convert.ToInt32(onCallEndTime) + ":" + onCallEndTimeMinutes + " AM";
                }

                return PartialView("GetTechniciansByBranchId", objTechnicianModel);
            }
            catch (Exception)
            {

                throw;
            }

        }
        public JsonResult OnTechCallGetData(string Resourcelist, string TimeZone)
        {

            TimeZone = TimeZone.Replace("Daylight", "Standard");
            TimeZoneInfo.ClearCachedData();
            var ClientHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Hours;
            var ClientMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Minutes;
            var ServerHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Hours;
            var ServerMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Minutes;
            var data = (from t in FarmerBrothersEntitites.TechOnCalls select t).AsEnumerable().Where(c => Resourcelist.Split(',').ToList().Contains(c.TechId.ToString())).ToList();
            List<ScheduleData> Appoint = new List<ScheduleData>();
            foreach (TechOnCall item in data)
            {

                Appoint.Add(new ScheduleData { Id = item.TechOnCallID, Subject = "", StartTimeZone = "UTC -05:00", EndTimeZone = "UTC -05:00", StartTime = item.ScheduleDate.Value, EndTime = item.ScheduleEndDate.Value, Description = "", AllDay = true, Recurrence = false, RecurrenceRule = "", TechnicianId = item.TechId.ToString() });
            }
            Appoint.ForEach(u =>
            {
                u.StartTime = u.StartTime.AddHours(ServerHour - (ClientHour)).AddMinutes(ServerMinutes - ClientMinutes);
                u.EndTime = u.EndTime.AddHours(ServerHour - (ClientHour)).AddMinutes(ServerMinutes - ClientMinutes);
            });
            return Json(Appoint, JsonRequestBehavior.AllowGet);
        }
        public JsonResult OnTechCallBatch(EditParams param, string Resourcelist, string TimeZone)
        {
            TimeZone = TimeZone.Replace("Daylight", "Standard");
            TimeZoneInfo.ClearCachedData();
            var ClientHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Hours;
            var ClientMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Minutes;
            var ServerHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Hours;
            var ServerMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Minutes;

            string onCallStartTime = ConfigurationManager.AppSettings["OnCallStartTime"] + "." + ConfigurationManager.AppSettings["OnCallStartTimeMinutes"];
            string onCallEndTime = ConfigurationManager.AppSettings["OnCallEndTime"] + "." + ConfigurationManager.AppSettings["OnCallEndTimeMinutes"];

            double startTime = 16.30;
            double endTime = 7.00;

            if (!string.IsNullOrWhiteSpace(onCallStartTime))
            {
                startTime = Convert.ToDouble(onCallStartTime);
            }

            if (!string.IsNullOrWhiteSpace(onCallEndTime))
            {
                endTime = Convert.ToDouble(onCallEndTime);
            }

            if (param.action == "insert" || (param.action == "batch" && param.added != null))         // this block of code will execute while inserting the appointments
            {
                var value = param.action == "insert" ? param.value : param.added[0];
                if (param.action == "insert")
                {
                    if (!value.AllDay)
                    {
                        TechOnCall appoint = new TechOnCall()
                        {
                            TechId = Convert.ToInt32(value.TechnicianId),
                            ScheduleStartTime = value.StartTime.Hour,
                            ScheduleEndTime = value.EndTime.Hour,
                            ScheduleDate = value.StartTime.Date,
                            ScheduleEndDate = value.EndTime.Date,
                            EntryUserName = UserName,
                        };
                        FarmerBrothersEntitites.TechOnCalls.Add(appoint);
                        FarmerBrothersEntitites.SaveChanges();
                    }
                    else
                    {
                        var totaldays = (value.EndTime.Day - value.StartTime.Day);
                        for (int i = 0; i <= totaldays; i++)
                        {
                            TechOnCall appoint = new TechOnCall()
                            {
                                TechId = Convert.ToInt32(value.TechnicianId),
                                ScheduleStartTime = Convert.ToDecimal(startTime > 12 ? startTime - 12 : startTime),
                                ScheduleEndTime = Convert.ToDecimal(endTime > 12 ? endTime - 12 : endTime),
                                ScheduleDate = value.StartTime.AddDays(i).Date.AddHours(startTime),
                                ScheduleEndDate = value.StartTime.AddDays(i + 1).Date.AddHours(endTime),
                                EntryUserName = UserName,

                            };
                            FarmerBrothersEntitites.TechOnCalls.Add(appoint);
                        }
                        FarmerBrothersEntitites.SaveChanges();
                    }
                }
                else
                {
                    if (param.added[0].EndTimeZone != null)
                    {
                        foreach (ScheduleData item in param.added)
                        {
                            if (!item.AllDay)
                            {
                                DateTime dt1 = item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                                DateTime dt2 = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                                TimeSpan ts = dt1.Subtract(dt2);
                                Decimal ehr = Convert.ToDecimal(dt1.ToString("HH:mm").Replace(':', '.'));
                                double eh = Convert.ToDouble(ehr);
                                int ds = ts.Days;

                                Decimal ahr = Convert.ToDecimal(dt1.ToString("HH"));
                                Decimal amn = Convert.ToDecimal(dt1.ToString("mm"));
                                double ah = Convert.ToDouble(ahr);
                                double am = Convert.ToDouble(amn);

                                for (int i = 0; i <= ds; i++)
                                {
                                    TechOnCall appoint = new TechOnCall()
                                    {
                                        TechId = Convert.ToInt32(item.TechnicianId),
                                        ScheduleStartTime = item.StartTime.Hour,
                                        ScheduleEndTime = item.EndTime.Hour,
                                        ScheduleDate = item.StartTime.AddDays(i).AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes),
                                        //ScheduleEndDate = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i + 1).Date.AddHours(ah).AddMinutes(am),
                                        ScheduleEndDate = dt1.Date == dt2.Date ? item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i).Date.AddHours(ah).AddMinutes(am) : item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i + 1).Date.AddHours(ah).AddMinutes(am),
                                        EntryUserName = UserName,
                                    };
                                    FarmerBrothersEntitites.TechOnCalls.Add(appoint);
                                    FarmerBrothersEntitites.SaveChanges();
                                }
                            }
                            else
                            {
                                
                                DateTime dt1 = item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                                //second date
                                DateTime dt2 = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                                //Subtact dates
                                TimeSpan ts = dt1.Subtract(dt2);
                                //result.
                                //Console.WriteLine(ts.Days);
                                int ds = ts.Days;
                                //var sdate = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(1).Date.AddHours(startTime);
                                //var sdate1 = dt1.AddDays(1).Date.AddHours(16.30);
                                //var sdate1 = item.StartTime.AddDays(1).AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                                //TimeSpan difference = dt2 - dt1;
                                //var days = difference.TotalDays;

                                //var totaldays = (item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Hour - item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Hour) < 0 ? 0 : (item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Day - item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Day);
                                //var totaldays = (item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Day - item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Day);
                                //for (int i = 0; i <= ds - 1; i++)
                                for (int i = 0; i <= ds; i++)
                                {
                                    TechOnCall appoint = new TechOnCall()
                                    {
                                        TechId = Convert.ToInt32(item.TechnicianId),
                                        ScheduleStartTime = Convert.ToDecimal(startTime) > 12 ? Convert.ToDecimal(startTime) - 12 : Convert.ToDecimal(startTime),
                                        ScheduleEndTime = Convert.ToDecimal(endTime) > 12 ? Convert.ToDecimal(endTime) - 12 : Convert.ToDecimal(endTime),
                                        //ScheduleDate = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i).Date.AddHours(startTime),
                                        ScheduleDate = item.StartTime.AddDays(i).AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes),
                                        ScheduleEndDate = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i + 1).Date.AddHours(endTime),
                                        EntryUserName = UserName,
                                    };
                                    FarmerBrothersEntitites.TechOnCalls.Add(appoint);
                                    FarmerBrothersEntitites.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            if (param.action == "remove" || param.deleted != null)                                        // this block of code will execute while removing the appointment
            {
                if (param.action == "remove")
                {
                    int key = Convert.ToInt32(param.key);
                    TechOnCall app = FarmerBrothersEntitites.TechOnCalls.Where(c => c.TechOnCallID == key).FirstOrDefault();
                    if (app != null) FarmerBrothersEntitites.TechOnCalls.RemoveRange(FarmerBrothersEntitites.TechOnCalls.Where(x => x.TechOnCallID == key));
                }
                else
                {
                    foreach (var apps in param.deleted)
                    {
                        int id = Convert.ToInt32(apps.Id);
                        TechOnCall app = FarmerBrothersEntitites.TechOnCalls.Where(c => c.TechId == id).FirstOrDefault();
                        if (apps != null) FarmerBrothersEntitites.TechOnCalls.RemoveRange(FarmerBrothersEntitites.TechOnCalls.Where(x => x.TechId == id));
                    }
                }
                FarmerBrothersEntitites.SaveChanges();
            }
            if ((param.action == "batch" && param.changed != null) || param.action == "update")   // this block of code will execute while updating the appointment
            {
                var value = param.action == "update" ? param.value : param.changed[0];
                var filterData = FarmerBrothersEntitites.TechOnCalls.Where(c => c.TechOnCallID == (value.Id));
                if (filterData.Count() > 0)
                {
                    //DateTime startTime = Convert.ToDateTime(value.StartTime);
                    //DateTime endTime = Convert.ToDateTime(value.EndTime);
                    TechOnCall appoint = FarmerBrothersEntitites.TechOnCalls.Single(c => c.TechOnCallID == (value.Id));

                    //appoint.ScheduleStartTime = Convert.ToDecimal(value.StartTime.Value.ToShortTimeString().Remove(value.StartTime.Value.ToShortTimeString().Length - 2).Trim().Replace(':', '.'));
                    //appoint.ScheduleEndTime = Convert.ToDecimal(value.EndTime.Value.ToShortTimeString().Remove(value.StartTime.Value.ToShortTimeString().Length - 2).Trim().Replace(':', '.'));
                    //appoint.ScheduleDate = value.StartTime;
                    //appoint.ScheduleEndDate = value.EndTime;
                    appoint.TechId = Convert.ToInt32(value.TechnicianId);
                }
                FarmerBrothersEntitites.SaveChanges();
            }
            var data = (from t in FarmerBrothersEntitites.TechOnCalls select t).AsEnumerable().Where(c => Resourcelist.Split(',').ToList().Contains(c.TechId.ToString())).ToList();
            List<ScheduleData> Appoint = new List<ScheduleData>();
            foreach (TechOnCall item in data)
            {

                Appoint.Add(new ScheduleData
                {
                    Id = item.TechOnCallID,
                    Subject = "",
                    StartTime = Convert.ToDateTime(item.ScheduleDate),
                    StartTimeZone = "UTC -05:00",
                    EndTimeZone = "UTC -05:00",
                    EndTime = Convert.ToDateTime(item.ScheduleEndDate),
                    Description = "",
                    AllDay = true,
                    Recurrence = false,
                    RecurrenceRule = "",
                    TechnicianId = item.TechId.ToString()
                });
            }
            Appoint.ForEach(u =>
            {
                u.StartTime = u.StartTime.AddHours(ServerHour - (ClientHour)).AddMinutes(ServerMinutes - ClientMinutes);
                u.EndTime = u.EndTime.AddHours(ServerHour - (ClientHour)).AddMinutes(ServerMinutes - ClientMinutes);
            });
            return Json(Appoint, JsonRequestBehavior.AllowGet);
        }
        public JsonResult TechScheduleGetData(string Resourcelist, string TimeZone)
        {
            TimeZone = TimeZone.Replace("Daylight", "Standard");
            TimeZoneInfo.ClearCachedData();
            var data = (from t in FarmerBrothersEntitites.TechSchedules select t).AsEnumerable().Where(c => Resourcelist.Split(',').ToList().Contains(c.TechId.ToString())).ToList();
            var ClientHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Hours;
            var ClientMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Minutes;
            var ServerHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Hours;
            var ServerMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Minutes;
            List<ScheduleData> Appoint = new List<ScheduleData>();
            var Query = (from c in FarmerBrothersEntitites.WorkOrders
                         join v in FarmerBrothersEntitites.WorkorderSchedules on c.WorkorderID equals v.WorkorderID
                         where c.AppointmentDate != null && new List<int>() { 1100, 1110, 1120, 1130, 1300, 1310, 1400, 1410 }.Contains(c.WorkorderCalltypeid ?? 0)
                         select new
                         {
                             CallTypeId = c.WorkorderCalltypeid,
                             Id = c.WorkorderID,
                             TechnicianId = v.Techid,
                             StartTime = (c.AppointmentDate) ?? DateTime.Now,
                             EndTime = (c.AppointmentDate) ?? DateTime.Now
                         }
                 ).ToList().Where(c => Resourcelist.Split(',').ToList().Contains(c.TechnicianId.ToString())).ToList();

            double onTechStartTime = Convert.ToDouble(ConfigurationManager.AppSettings["OnTechStartTime"]);
            double onTechEndTime = Convert.ToDouble(ConfigurationManager.AppSettings["OnTechEndTime"]);

            foreach (var item in Query)
            {
                Appoint.Add(new ScheduleData
                {
                    Subject = new List<int>() { 1100, 1110, 1120, 1130 }.Contains(item.CallTypeId ?? 0) ? "PWO " + item.Id : (new List<int>() { 1300, 1310 }.Contains(item.CallTypeId ?? 0)) ? "IWO " + item.Id : (new List<int>() { 1400, 1410 }.Contains(item.CallTypeId ?? 0)) ? "RWO " + item.Id : "",
                    StartTime = new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, 0, 0, 0).AddHours(onTechStartTime),
                    EndTime = new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, 0, 0, 0).AddHours(onTechEndTime),
                    AllDay = true,
                    StartTimeZone = "UTC -05:00",
                    EndTimeZone = "UTC -05:00",
                    TechnicianId = item.TechnicianId.ToString(),
                    Id = item.Id

                });


            }

            //Appoint.ForEach(i => { i.StartTime = i.StartTime.AddHours(9); i.EndTime = i.StartTime.AddHours(8); });
            foreach (TechSchedule item in data)
            {
                Appoint.Add(new ScheduleData
                {
                    Id = item.TechScheduleID,
                    Subject = item.WorkOrderID == null ? item.AppointmentSubject ?? "" : (new List<int>() { 1100, 1110, 1120, 1130 }.Contains(Convert.ToInt32((from t in FarmerBrothersEntitites.WorkOrders where t.WorkorderID == (item.WorkOrderID) select t.WorkorderCalltypeid).SingleOrDefault()))) ? "PWO " + item.WorkOrderID : (Convert.ToInt32((from t in FarmerBrothersEntitites.WorkOrders where t.WorkorderID == (item.WorkOrderID) select t.WorkorderCalltypeid).SingleOrDefault()) == 1300) ? "IWO " + item.WorkOrderID : "RWO " + item.WorkOrderID,
                    StartTimeZone = "UTC -05:00",
                    EndTimeZone = "UTC -05:00",
                    StartTime = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == Convert.ToDecimal(onTechStartTime)) ? item.ScheduleDate.Value.AddHours(onTechStartTime) : Convert.ToDateTime(item.ScheduleDate),
                    EndTime = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == Convert.ToDecimal(onTechStartTime)) ? item.ScheduleDate.Value.AddHours(onTechEndTime) : Convert.ToDateTime(item.ScheduleDate.Value.AddMinutes(Math.Floor(Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime)) * 60 + (Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime) - Math.Floor(Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime)) > 0 ? 30 : 0))),
                    Description = "",
                    AllDay = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == Convert.ToDecimal(onTechStartTime)) ? true : false,
                    Recurrence = false,
                    RecurrenceRule = "",
                    TechnicianId = item.TechId.ToString()
                });
            }
            Appoint.ForEach(u =>
            {
                u.StartTime = u.StartTime.AddHours(ServerHour - (ClientHour)).AddMinutes(ServerMinutes - (ClientMinutes));
                u.EndTime = u.EndTime.AddHours(ServerHour - (ClientHour)).AddMinutes(ServerMinutes - (ClientMinutes));
            });
            return Json(Appoint, JsonRequestBehavior.AllowGet);
        }
        public JsonResult TechScheduleBatch(EditParams param, string Resourcelist, string TimeZone)
        {
            TimeZone = TimeZone.Replace("Daylight", "Standard");
            TimeZoneInfo.ClearCachedData();
            var ClientHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Hours;
            var ClientMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Minutes;
            var ServerHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Hours;
            var ServerMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Minutes;

            //double onTechStartTime = Convert.ToDouble(ConfigurationManager.AppSettings["OnTechStartTime"]);
            //double onTechEndTime = Convert.ToDouble(ConfigurationManager.AppSettings["OnTechEndTime"]);

            if (param.action == "insert" || (param.action == "batch" && param.added != null))
            {
                if (param.action == "insert")
                {
                    var value = param.value;
                    var totaldays = (value.EndTime.Day - value.StartTime.Day);
                    IndexCounter counter = Utility.GetIndexCounter("scheduleStartEndID", FarmerBrothersEntitites);
                    counter.IndexValue += totaldays + 1;
                    FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                    for (int i = 0; i <= totaldays; i++)
                    {
                        TechSchedule appoint = new TechSchedule()
                          {
                              TechId = Convert.ToInt32(value.TechnicianId),
                              scheduleStartEndID = counter.IndexValue.Value,
                              ScheduleStartTime = Convert.ToDecimal(value.StartTime.ToShortTimeString()),
                              ScheduleEndTime = Convert.ToDecimal(value.EndTime.ToShortTimeString()),
                              ScheduleDate = value.StartTime.AddDays(i),
                              WorkOrderID = null,
                              Availability = "UnAvailable",
                          };
                        FarmerBrothersEntitites.TechSchedules.Add(appoint);
                        FarmerBrothersEntitites.SaveChanges();
                    }
                }
                else
                {
                    if (param.added[0].EndTimeZone != null)
                    {
                        TimeZoneInfo.ClearCachedData();
                        foreach (ScheduleData item in param.added)
                        {
                            // .AddHours(TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Hours - (-6)).AddMinutes(TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Minutes)
                            //     var value = param.value;
                            DateTime dt1 = item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                            DateTime dt2 = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                            TimeSpan ts = dt1.Subtract(dt2);
                            int ds = ts.Days;
                           // var totaldays = (item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Hour - item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Hour) < 0 ? 0 : (item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Day - item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Day);
                            IndexCounter counter = Utility.GetIndexCounter("scheduleStartEndID", FarmerBrothersEntitites);
                            counter.IndexValue += ds + 1;
                            FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
                            for (int i = 0; i <= ds; i++)
                            {
                                TechSchedule appoint = new TechSchedule()
                                {
                                    TechId = Convert.ToInt32(item.TechnicianId),
                                    scheduleStartEndID = counter.IndexValue.Value,
                                    ScheduleStartTime = item.AllDay == true ? 0 : Convert.ToDecimal(item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).ToString("HH:mm").Replace(':', '.')),
                                    ScheduleEndTime = item.AllDay == true ? Convert.ToDecimal(23.59) : Convert.ToDecimal(item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).ToString("HH:mm").Replace(':', '.')),
                                    ScheduleDate = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i),
                                    //StartTimeZone ="UTC +00:00",
                                    //EndTimeZone = "UTC +00:00",
                                    AppointmentSubject = item.Subject,
                                    WorkOrderID = null,
                                    Availability = "UnAvailable",
                                };
                                FarmerBrothersEntitites.TechSchedules.Add(appoint);
                                FarmerBrothersEntitites.SaveChanges();
                            }
                        }
                    }
                }
            }
            if (param.action == "remove" || param.deleted != null)                                        // this block of code will execute while removing the appointment
            {
                if (param.action == "remove")
                {
                    int key = Convert.ToInt32(param.key);
                    TechSchedule app = FarmerBrothersEntitites.TechSchedules.Where(c => c.TechScheduleID == key).FirstOrDefault();
                    if (app != null) FarmerBrothersEntitites.TechSchedules.RemoveRange(FarmerBrothersEntitites.TechSchedules.Where(x => x.TechScheduleID == key));
                }
                else
                {
                    foreach (var apps in param.deleted)
                    {
                        int id = Convert.ToInt32(apps.Id);
                        TechSchedule app = FarmerBrothersEntitites.TechSchedules.Where(c => c.TechId == id).FirstOrDefault();
                        if (apps != null) FarmerBrothersEntitites.TechSchedules.RemoveRange(FarmerBrothersEntitites.TechSchedules.Where(x => x.TechScheduleID == id));
                    }
                }
                FarmerBrothersEntitites.SaveChanges();
            }
            if ((param.action == "batch" && param.changed != null) || param.action == "update")   // this block of code will execute while updating the appointment
            {
                var value = param.action == "update" ? param.value : param.changed[0];
                var filterData = FarmerBrothersEntitites.TechSchedules.Where(c => c.TechScheduleID == (value.Id));
                if (filterData.Count() > 0)
                {
                    TechSchedule appoint = FarmerBrothersEntitites.TechSchedules.Single(c => c.TechScheduleID == (value.Id));
                    appoint.ScheduleStartTime = value.AllDay == true ? 0 : Convert.ToDecimal(value.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).ToString("HH:mm").Replace(':', '.'));
                    appoint.ScheduleEndTime = value.AllDay == true ? Convert.ToDecimal(23.59) : Convert.ToDecimal(value.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).ToString("HH:mm").Replace(':', '.'));
                    appoint.ScheduleDate = value.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                    appoint.AppointmentSubject = value.Subject;
                    appoint.TechId = Convert.ToInt32(value.TechnicianId);
                }
                FarmerBrothersEntitites.SaveChanges();
            }
            var data = (from t in FarmerBrothersEntitites.TechSchedules select t).AsEnumerable().Where(c => Resourcelist.Split(',').ToList().Contains(c.TechId.ToString())).ToList();

            List<ScheduleData> Appoint = new List<ScheduleData>();
            var Query = (from c in FarmerBrothersEntitites.WorkOrders
                         join v in FarmerBrothersEntitites.WorkorderSchedules on c.WorkorderID equals v.WorkorderID
                         where c.AppointmentDate != null && v.AssignedStatus.ToLower() == "accepted" && new List<int>() { 1100, 1110, 1120, 1130, 1300, 1310, 1400, 1410 }.Contains(c.WorkorderCalltypeid ?? 0)
                         select new
                         {
                             CallTypeId = c.WorkorderCalltypeid,
                             Id = c.WorkorderID,
                             TechnicianId = v.Techid,
                             StartTime = (c.AppointmentDate) ?? DateTime.Now,
                             EndTime = (c.AppointmentDate) ?? DateTime.Now
                         }
                 ).ToList().Where(c => Resourcelist.Split(',').ToList().Contains(c.TechnicianId.ToString())).ToList();


            foreach (var item in Query)
            {
                Appoint.Add(new ScheduleData
                {
                    Subject = new List<int>() { 1100, 1110, 1120, 1130 }.Contains(item.CallTypeId ?? 0) ? "PWO " + item.Id : (new List<int>() { 1300, 1310 }.Contains(item.CallTypeId ?? 0)) ? "IWO " + item.Id : (new List<int>() { 1400, 1410 }.Contains(item.CallTypeId ?? 0)) ? "RWO " + item.Id : "",
                    StartTime = new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, 0, 0, 0).AddHours(0),
                    EndTime = new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, 0, 0, 0).AddHours(23).AddMinutes(59),
                    AllDay = true,
                    StartTimeZone = "UTC -05:00",
                    EndTimeZone = "UTC -05:00",
                    TechnicianId = item.TechnicianId.ToString(),
                    Id = item.Id

                });


            }

            //Appoint.ForEach(i => { i.StartTime = i.StartTime.AddHours(9); i.EndTime = i.StartTime.AddHours(8); });
            foreach (TechSchedule item in data)
            {
                Appoint.Add(new ScheduleData
                {
                    Id = item.TechScheduleID,
                    Subject = item.WorkOrderID == null ? item.AppointmentSubject ?? "" : (new List<int>() { 1100, 1110, 1120, 1130 }.Contains(Convert.ToInt32((from t in FarmerBrothersEntitites.WorkOrders where t.WorkorderID == (item.WorkOrderID) select t.WorkorderCalltypeid).SingleOrDefault()))) ? "PWO " + item.WorkOrderID : (Convert.ToInt32((from t in FarmerBrothersEntitites.WorkOrders where t.WorkorderID == (item.WorkOrderID) select t.WorkorderCalltypeid).SingleOrDefault()) == 1300) ? "IWO " + item.WorkOrderID : "RWO " + item.WorkOrderID,
                    StartTimeZone = "UTC -05:00",
                    EndTimeZone = "UTC -05:00",
                    StartTime = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == 9) ? item.ScheduleDate.Value.AddHours(0) : Convert.ToDateTime(item.ScheduleDate),
                    EndTime = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == 9) ? item.ScheduleDate.Value.AddHours(23).AddMinutes(59) : Convert.ToDateTime(item.ScheduleDate.Value.AddMinutes(Math.Floor(Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime)) * 60 + (Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime) - Math.Floor(Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime)) > 0 ? 30 : 0))),
                    Description = "",
                    AllDay = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == 9) ? true : false,
                    Recurrence = false,
                    RecurrenceRule = "",
                    TechnicianId = item.TechId.ToString()
                });
            }
            Appoint.ForEach(u =>
            {
                u.StartTime = u.StartTime.AddHours(ServerHour - (ClientHour)).AddMinutes(ServerMinutes - ClientMinutes);
                u.EndTime = u.EndTime.AddHours(ServerHour - (ClientHour)).AddMinutes(ServerMinutes - ClientMinutes);
            });
            return Json(Appoint, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Selctionchange()
        {
            return Json("true");
        }
        public List<ResourceFields> fnResourceList(string Query)
        {
            DataTable dt = objMarsview.fn_FSM_View(Query);
            List<ResourceFields> Resourcelists = new List<ResourceFields>();
            List<string> Colors = new List<string> { "#f8a398", "#56ca85", "#51a0ed", "Red", "Green", "Blue", "Yellow", "#D2E445", "#FF7FD4", "#D42AFF", "#FF00AA", "#FF5500", "#AA00D5", "#FFFF00" };
            CalendarTechnicianModel objTechModel = new CalendarTechnicianModel();
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {

                Resourcelists.Add
               (
                    new ResourceFields()
                    {
                        IsChecked = true,
                        Id = dr[0].ToString(),
                        Text = dr[1].ToString(),
                        Color = Colors[i]
                    }
               );
                i = i + 1;
            }

            return Resourcelists;
        }
        private string fnOnCallQuery(string Techtype, int OnCallTechId)
        {
            string strOnCallQuery = string.Empty;
            if (Int32.TryParse(Techtype, out OnCallTechId))
            {
                strOnCallQuery = "  SELECT dbo.feast_tech_hierarchy.Tech_ID, dbo.feast_tech_hierarchy.Tech_Name FROM dbo.ZonePriority INNER JOIN";
                strOnCallQuery += "  dbo.feast_tech_hierarchy ON dbo.ZonePriority.OnCallPrimarytechID = dbo.feast_tech_hierarchy.Tech_Id";
                strOnCallQuery += "  where dbo.ZonePriority.OnCallGroupID = " + Convert.ToInt32(OnCallTechId) + " Union";
                strOnCallQuery += "  SELECT dbo.feast_tech_hierarchy.Tech_ID,dbo.feast_tech_hierarchy.Tech_Name FROM dbo.ZonePriority INNER JOIN ";
                strOnCallQuery += "  dbo.feast_tech_hierarchy ON dbo.ZonePriority.OnCallBackupTechID = dbo.feast_tech_hierarchy.Tech_Id";
                strOnCallQuery += "  where dbo.ZonePriority.OnCallGroupID = " + Convert.ToInt32(OnCallTechId);
                strOnCallQuery += "   Group By dbo.feast_tech_hierarchy.Tech_ID,dbo.feast_tech_hierarchy.Tech_Name ";
            }
            return strOnCallQuery;
        }

        public static bool IsHoliday(DateTime StartTime)
        {
            bool isExist = false;
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                var holiday = (from holday in FarmerBrothersEntitites.HolidayLists
                               where DbFunctions.TruncateTime(holday.HolidayDate) == DbFunctions.TruncateTime(StartTime)
                               select holday).FirstOrDefault();

                if (holiday != null)
                {
                    isExist = true;
                }
            }
            return isExist;
        }

        public static bool IsTechUnAvailable(int techId, DateTime StartTime)
        {
            //bool isExist = false;
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                var holiday = (from sc in FarmerBrothersEntitites.TechSchedules
                               where DbFunctions.TruncateTime(sc.ScheduleDate) == DbFunctions.TruncateTime(StartTime) && sc.TechId == techId
                               select sc).FirstOrDefault();

                if (holiday != null)
                {
                    DateTime UnavailableStartDate = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + new DateTime().AddHours(Convert.ToDouble(holiday.ScheduleStartTime)).ToString("hh:mm tt"));
                    DateTime UnavailableEndDate = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + new DateTime().AddHours(Convert.ToDouble(holiday.ScheduleEndTime)).ToString("hh:mm tt"));

                    if ((UnavailableStartDate <= StartTime) && (UnavailableEndDate > StartTime))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }
            }
            return false;
        }

    }
}