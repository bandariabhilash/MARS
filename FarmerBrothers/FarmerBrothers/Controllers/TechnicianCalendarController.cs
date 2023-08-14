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
                                FROM TECH_HIERARCHY tech where tech.BranchName !='' and searchType='SP'
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
        public JsonResult GetTechNames()
        {
            var data = FarmerBrothersEntitites.TECH_HIERARCHY.Where(w => w.SearchType == "SP").OrderBy(t => t.CompanyName).Select(w => new { w.DealerId, w.CompanyName, w.City }).ToList();

            List<GetTechNames> Appoint = new List<GetTechNames>();
            foreach (var x in data)
            {
                Appoint.Add(new GetTechNames { techid = x.DealerId.ToString(), techname = x.CompanyName + " - " + x.City });
            }
            return Json(Appoint, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetTechniciansByBranchId(string Tid, string strBranchID, bool IsTechSchedule, string TimeZone)
        {

            try
            {
                ModelState.Clear();
                int OnCallTechId = 0;
                string Techtype = strBranchID.Contains('/') ? strBranchID.Split('/')[1] : strBranchID;
                string strQuery = string.Empty, strQuery1 = string.Empty, subsql = string.Empty;

                if (Tid != "")
                {
                    subsql = "tech.Dealerid !='' and tech.Dealerid = '" + Tid + "'";
                }
                else
                {
                    subsql = "tech.BranchName != '' and BranchNumber = '" + strBranchID + "'";
                }

                strQuery = @"SELECT tech.Dealerid as Tech_Id, tech.CompanyName +' - '+ tech.city as Tech_Name from 
                            TECH_HIERARCHY tech where " + subsql + " and searchType='SP' GROUP BY tech.Dealerid, tech.CompanyName+' - '+ tech.city";

                strQuery1 = @"SELECT tech.Dealerid as Tech_Id, tech.CompanyName+' - '+ tech.city as Tech_Name from 
                            TECH_HIERARCHY tech where searchType in ('SP','NA') and FamilyAff != 'SPT' GROUP BY tech.Dealerid, tech.CompanyName+' - '+ tech.city";

                CalendarTechnicianModel objTechnicianModel = new CalendarTechnicianModel();
                objTechnicianModel.TimeZone = TimeZone;
                objTechnicianModel.ResourceList = fnResourceList(strQuery);
                objTechnicianModel.ResourceList1 = fnResourceList1(strQuery1);
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
        public string GetTimeZone(String GTZ)
        {
            String GetTZ = "";
            if (GTZ != "Mountain Standard Time" && GTZ != "Alaskan Standard Time" && GTZ != "Pacific Standard Time" && GTZ != "Hawaiian Standard Time" && GTZ != "Eastern Standard Time" && GTZ != "Central Daylight Time")
            {
                GetTZ = "Central Daylight Time";
            }
            else
            {
                GetTZ = GTZ;
            }
            return GetTZ;
        }
        public JsonResult OnTechCallGetData(string Resourcelist, string TimeZone)
        {
            TimeZone = TimeZone.Replace("Daylight", "Standard").Replace("Alaska", "Alaskan").Replace("Local", "Hawaiian");
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
            TimeZone = TimeZone.Replace("Daylight", "Standard").Replace("Alaska", "Alaskan").Replace("Local", "Hawaiian");
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

            if (param.action == "insert" || (param.action == "batch" && param.added != null))
            {
                var value = param.action == "insert" ? param.value : param.added[0];

                int tchid = Convert.ToInt32(value.TechnicianId);
                string tchZipCode = FarmerBrothersEntitites.TECH_HIERARCHY.Where(tid => tid.DealerId == tchid).Select(tid => tid.PostalCode).FirstOrDefault();
                DateTime currTime = Utility.GetCurrentTime(tchZipCode, FarmerBrothersEntitites);

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
                            ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            ModifiedUserName = UserName,
                            ModifiedDate = currTime,
                            ScheduleCreatedDate = currTime,
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
                                ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                ModifiedUserName = UserName,
                                ModifiedDate = currTime,
                                ScheduleCreatedDate = currTime,

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
                            int techid = Convert.ToInt32(item.TechnicianId);
                            string techZipCode = FarmerBrothersEntitites.TECH_HIERARCHY.Where(tid => tid.DealerId == techid).Select(tid => tid.PostalCode).FirstOrDefault();
                            DateTime currentTime = Utility.GetCurrentTime(techZipCode, FarmerBrothersEntitites);

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
                                        ScheduleEndDate = dt1.Date == dt2.Date ? item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i).Date.AddHours(ah).AddMinutes(am) : item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i + 1).Date.AddHours(ah).AddMinutes(am),
                                        EntryUserName = UserName,
                                        ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                        ModifiedUserName = UserName,
                                        ModifiedDate = currentTime,
                                        ScheduleCreatedDate = currentTime,
                                    };
                                    FarmerBrothersEntitites.TechOnCalls.Add(appoint);
                                    FarmerBrothersEntitites.SaveChanges();
                                }
                            }
                            else
                            {

                                //DateTime dt1 = Convert.ToDateTime(item.EndTime.ToShortDateString());
                                ////second date
                                //DateTime dt2 = Convert.ToDateTime(item.StartTime.ToShortDateString());
                                ////Subtact dates
                                //TimeSpan ts = dt1.Subtract(dt2);
                                //int ds = ts.Days;
                                DateTime dt1 = item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                                //second date
                                DateTime dt2 = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                                //Subtact dates
                                TimeSpan ts = dt1.Subtract(dt2);
                                int ds = ts.Days;
                                for (int i = 0; i <= ds; i++)
                                {
                                    if (IsHoliday(item.StartTime.AddDays(i)) || (Convert.ToDateTime(item.StartTime.AddDays(i)).DayOfWeek == DayOfWeek.Saturday ||
                                        Convert.ToDateTime(item.StartTime.AddDays(i)).DayOfWeek == DayOfWeek.Sunday))
                                    {
                                        TechOnCall appoint = new TechOnCall()
                                        {
                                            TechId = techid,
                                            ScheduleStartTime = Convert.ToDecimal(0.00),
                                            ScheduleEndTime = Convert.ToDecimal(23.59),
                                            ScheduleDate = Convert.ToDateTime(item.StartTime.ToShortDateString()).AddDays(i),
                                            ScheduleEndDate = Convert.ToDateTime(item.StartTime.ToShortDateString()).AddDays(i).AddHours(23).AddMinutes(59),
                                            EntryUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                            EntryUserName = UserName,
                                            ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                            ModifiedUserName = UserName,
                                            ModifiedDate = currentTime,
                                            ScheduleCreatedDate = currentTime,
                                        };
                                        FarmerBrothersEntitites.TechOnCalls.Add(appoint);
                                    }
                                    else
                                    {
                                        TechOnCall appoint = new TechOnCall()
                                        {
                                            TechId = techid,
                                            ScheduleStartTime = Convert.ToDecimal(startTime) > 12 ? Convert.ToDecimal(startTime) - 12 : Convert.ToDecimal(startTime),
                                            ScheduleEndTime = Convert.ToDecimal(endTime) > 12 ? Convert.ToDecimal(endTime) - 12 : Convert.ToDecimal(endTime),
                                            ScheduleDate = item.StartTime.AddDays(i).AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes),
                                            ScheduleEndDate = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i + 1).Date.AddHours(endTime),
                                            EntryUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                            EntryUserName = UserName,
                                            ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                            ModifiedUserName = UserName,
                                            ModifiedDate = currentTime,
                                            ScheduleCreatedDate = currentTime,
                                        };
                                        FarmerBrothersEntitites.TechOnCalls.Add(appoint);
                                    }
                                }
                                FarmerBrothersEntitites.SaveChanges();

                            }
                        }
                    }
                }
            }
            if (param.action == "remove" || param.deleted != null)
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
            if ((param.action == "batch" && param.changed != null) || param.action == "update")
            {
                var value = param.action == "update" ? param.value : param.changed[0];
                var filterData = FarmerBrothersEntitites.TechOnCalls.Where(c => c.TechOnCallID == (value.Id));
                if (filterData.Count() > 0)
                {
                    int techid = Convert.ToInt32(value.TechnicianId);
                    string techZipCode = FarmerBrothersEntitites.TECH_HIERARCHY.Where(tid => tid.DealerId == techid).Select(tid => tid.PostalCode).FirstOrDefault();
                    DateTime currentTime = Utility.GetCurrentTime(techZipCode, FarmerBrothersEntitites);


                    TechOnCall appoint = FarmerBrothersEntitites.TechOnCalls.Single(c => c.TechOnCallID == (value.Id));
                    appoint.ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234; //TBD
                    appoint.ModifiedUserName = UserName;
                    appoint.ModifiedDate = currentTime;
                    appoint.TechId = techid;
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
            TimeZone = TimeZone.Replace("Daylight", "Standard").Replace("Alaska", "Alaskan").Replace("Local", "Hawaiian");
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


            var ScheduledEventsQuery = (from c in FarmerBrothersEntitites.WorkOrders
                                        join v in FarmerBrothersEntitites.WorkorderSchedules on c.WorkorderID equals v.WorkorderID
                                        where(c.WorkorderCallstatus == "Scheduled" && v.AssignedStatus == "Scheduled")
                                        select new
                                        {
                                            CallTypeId = c.WorkorderCalltypeid,
                                            Id = c.WorkorderID,
                                            ContactName = c.CustomerName,
                                            ContactId = c.CustomerID,
                                            ErfId = c.WorkorderErfid,
                                            TechnicianId = v.Techid,
                                            StartTime = (v.EventScheduleDate) ?? DateTime.Now,
                                            EndTime = (v.EventScheduleDate) ?? DateTime.Now
                                        }
              ).ToList().Where(c => Resourcelist.Split(',').ToList().Contains(c.TechnicianId.ToString())).ToList();



            double onTechStartTime = Convert.ToDouble(ConfigurationManager.AppSettings["OnTechStartTime"]);
            double onTechEndTime = Convert.ToDouble(ConfigurationManager.AppSettings["OnTechEndTime"]);

            foreach (var item in Query)
            {
                Appoint.Add(new ScheduleData
                {
                    Subject = new List<int>() { 1100, 1110, 1120, 1130 }.Contains(item.CallTypeId ?? 0) ? "PWO " + item.Id : (new List<int>() { 1300, 1310 }.Contains(item.CallTypeId ?? 0)) ? "IWO " + item.Id : (new List<int>() { 1400, 1410 }.Contains(item.CallTypeId ?? 0)) ? "RWO " + item.Id : "",
                    StartTime = item.StartTime,//new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, 0, 0, 0).AddHours(onTechStartTime),
                    EndTime = item.StartTime,//new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, 0, 0, 0).AddHours(onTechEndTime),
                    //AllDay = true,
                    StartTimeZone = "UTC -05:00",
                    EndTimeZone = "UTC -05:00",
                    TechnicianId = item.TechnicianId.ToString(),
                    Id = item.Id

                });
            }

            foreach (var item in ScheduledEventsQuery)
            {
                Appoint.Add(new ScheduleData
                {
                    Subject =  "Scheduled EventId: " + item.Id + ", ERFId: " + item.ErfId + ", \r\n ContactId: " + item.Id + ", AccountName: " + item.ContactName,
                    StartTime = item.StartTime,//new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, 0, 0, 0).AddHours(onTechStartTime),
                    EndTime = item.EndTime,//new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, 0, 0, 0).AddHours(onTechEndTime),
                    //AllDay = true,
                    StartTimeZone = "UTC -05:00",
                    EndTimeZone = "UTC -05:00",
                    TechnicianId = item.TechnicianId.ToString(),
                    Id = item.Id

                });
            }

            foreach (TechSchedule item in data)
            {
                Appoint.Add(new ScheduleData
                {
                    Id = item.TechScheduleID,
                    Subject = item.WorkOrderID == null ? item.AppointmentSubject ?? "" : (new List<int>() { 1100, 1110, 1120, 1130 }.Contains(Convert.ToInt32((from t in FarmerBrothersEntitites.WorkOrders where t.WorkorderID == (item.WorkOrderID) select t.WorkorderCalltypeid).SingleOrDefault()))) ? "PWO " + item.WorkOrderID : (Convert.ToInt32((from t in FarmerBrothersEntitites.WorkOrders where t.WorkorderID == (item.WorkOrderID) select t.WorkorderCalltypeid).SingleOrDefault()) == 1300) ? "IWO " + item.WorkOrderID : "RWO " + item.WorkOrderID,
                    StartTimeZone = "UTC -05:00",
                    EndTimeZone = "UTC -05:00",
                    //StartTime = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == Convert.ToDecimal(onTechStartTime)) ? item.ScheduleDate.Value.AddHours(onTechStartTime) : Convert.ToDateTime(item.ScheduleDate),
                    //EndTime = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == Convert.ToDecimal(onTechStartTime)) ? item.ScheduleDate.Value.AddHours(onTechEndTime) : Convert.ToDateTime(item.ScheduleDate.Value.AddMinutes(Math.Floor(Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime)) * 60 + (Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime) - Math.Floor(Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime)) > 0 ? 30 : 0))),

                    StartTime = Convert.ToDateTime(item.ScheduleDate),
                    EndTime = Convert.ToDateTime(item.ScheduleDate).Date.AddHours(Math.Floor(Convert.ToDouble(item.ScheduleEndTime))).AddMinutes((int)((item.ScheduleEndTime - (int)item.ScheduleEndTime) * 100)),

                    Description = "",
                    //AllDay = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == Convert.ToDecimal(onTechStartTime)) ? true : false,
                    AllDay = (item.ScheduleEndTime - item.ScheduleStartTime >= 8) ? true : false,
                    Recurrence = false,
                    RecurrenceRule = "",
                    TechnicianId = item.TechId.ToString(),
                    Categorize = item.ReplaceTech == null ? "0" : item.ReplaceTech.ToString()

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
            TimeZone = TimeZone.Replace("Daylight", "Standard").Replace("Alaska", "Alaskan").Replace("Local", "Hawaiian");
            TimeZoneInfo.ClearCachedData();

            var ClientHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Hours;
            var ClientMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZone).BaseUtcOffset.Minutes;
            var ServerHour = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Hours;
            var ServerMinutes = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).BaseUtcOffset.Minutes;

            if (param.action == "insert" || (param.action == "batch" && param.added != null))
            {
                if (param.action == "insert")
                {
                    var value = param.value;
                    var totaldays = (value.EndTime.Day - value.StartTime.Day);
                    IndexCounter counter = Utility.GetIndexCounter("scheduleStartEndID", 1);
                    counter.IndexValue += totaldays + 1;
                    //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                    int tchid = Convert.ToInt32(value.TechnicianId);
                    string tchZipCode = FarmerBrothersEntitites.TECH_HIERARCHY.Where(tid => tid.DealerId == tchid).Select(tid => tid.PostalCode).FirstOrDefault();
                    DateTime currTime = Utility.GetCurrentTime(tchZipCode, FarmerBrothersEntitites);

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
                            ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            ModifiedUserName = UserName,
                            ModifiedDate = currTime,
                            ScheduleCreatedDate = currTime
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
                            int techid = Convert.ToInt32(item.TechnicianId);
                            string techZipCode = FarmerBrothersEntitites.TECH_HIERARCHY.Where(tid => tid.DealerId == techid).Select(tid => tid.PostalCode).FirstOrDefault();
                            DateTime currentTime = Utility.GetCurrentTime(techZipCode, FarmerBrothersEntitites);

                            //var totaldays = (item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Hour - item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Hour) < 0 ? 0 : (item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Day - item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Day);
                            var totaldays = new DateTime((item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Ticks - item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).Ticks)).Day;
                            IndexCounter counter = Utility.GetIndexCounter("scheduleStartEndID", 1);
                            counter.IndexValue += totaldays + 1;
                            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
                            for (int i = 0; i < totaldays; i++)
                            {
                                TechSchedule appoint = new TechSchedule()
                                {
                                    TechId = techid,
                                    ReplaceTech = item.Categorize == "" ? 0 : Convert.ToInt32(item.Categorize),
                                    scheduleStartEndID = counter.IndexValue.Value,
                                    ScheduleStartTime = item.AllDay == true ? Convert.ToDecimal(0.01) : Convert.ToDecimal(item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).ToString("HH:mm").Replace(':', '.')),
                                    ScheduleEndTime = item.AllDay == true ? Convert.ToDecimal(23.59) : Convert.ToDecimal(item.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).ToString("HH:mm").Replace(':', '.')),
                                    ScheduleDate = item.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).AddDays(i),
                                    EntryUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                    EntryUserName = UserName,
                                    AppointmentSubject = item.Subject,
                                    WorkOrderID = null,
                                    Availability = "UnAvailable",
                                    ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                    ModifiedUserName = UserName,
                                    ModifiedDate = currentTime,
                                    ScheduleCreatedDate = currentTime

                                };
                                FarmerBrothersEntitites.TechSchedules.Add(appoint);
                                FarmerBrothersEntitites.SaveChanges();
                            }
                        }
                    }
                }
            }
            if (param.action == "remove" || param.deleted != null)
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
            if ((param.action == "batch" && param.changed != null) || param.action == "update")
            {
                var value = param.action == "update" ? param.value : param.changed[0];
                var filterData = FarmerBrothersEntitites.TechSchedules.Where(c => c.TechScheduleID == (value.Id));
                if (filterData.Count() > 0)
                {
                    int techid = Convert.ToInt32(value.TechnicianId);
                    string techZipCode = FarmerBrothersEntitites.TECH_HIERARCHY.Where(tid => tid.DealerId == techid).Select(tid => tid.PostalCode).FirstOrDefault();
                    DateTime currentTime = Utility.GetCurrentTime(techZipCode, FarmerBrothersEntitites);


                    TechSchedule appoint = FarmerBrothersEntitites.TechSchedules.Single(c => c.TechScheduleID == (value.Id));
                    appoint.ScheduleStartTime = value.AllDay == true ? Convert.ToDecimal(0.01) : Convert.ToDecimal(value.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).ToString("HH:mm").Replace(':', '.'));
                    appoint.ScheduleEndTime = value.AllDay == true ? Convert.ToDecimal(23.59) : Convert.ToDecimal(value.EndTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes).ToString("HH:mm").Replace(':', '.'));
                    appoint.ScheduleDate = value.StartTime.AddHours(ClientHour - (ServerHour)).AddMinutes(ClientMinutes - ServerMinutes);
                    appoint.AppointmentSubject = value.Subject;
                    appoint.ReplaceTech = value.Categorize == "" ? 0 : Convert.ToInt32(value.Categorize);
                    appoint.TechId = techid;
                    appoint.ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234;
                    appoint.ModifiedUserName = UserName;
                    appoint.ModifiedDate = currentTime;
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
                    Id = item.Id,

                });


            }


            foreach (TechSchedule item in data)
            {
                Appoint.Add(new ScheduleData
                {
                    Id = item.TechScheduleID,
                    Subject = item.WorkOrderID == null ? item.AppointmentSubject ?? "" : (new List<int>() { 1100, 1110, 1120, 1130 }.Contains(Convert.ToInt32((from t in FarmerBrothersEntitites.WorkOrders where t.WorkorderID == (item.WorkOrderID) select t.WorkorderCalltypeid).SingleOrDefault()))) ? "PWO " + item.WorkOrderID : (Convert.ToInt32((from t in FarmerBrothersEntitites.WorkOrders where t.WorkorderID == (item.WorkOrderID) select t.WorkorderCalltypeid).SingleOrDefault()) == 1300) ? "IWO " + item.WorkOrderID : "RWO " + item.WorkOrderID,
                    StartTimeZone = "UTC -05:00",
                    EndTimeZone = "UTC -05:00",
                    //StartTime = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == 9) ? item.ScheduleDate.Value.AddHours(0) : Convert.ToDateTime(item.ScheduleDate),
                    //EndTime = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == 9) ? item.ScheduleDate.Value.AddHours(23).AddMinutes(59) : Convert.ToDateTime(item.ScheduleDate.Value.AddMinutes(Math.Floor(Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime)) * 60 + (Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime) - Math.Floor(Convert.ToDouble(item.ScheduleEndTime - item.ScheduleStartTime)) > 0 ? 30 : 0))),
                    StartTime = Convert.ToDateTime(item.ScheduleDate),
                    EndTime = Convert.ToDateTime(item.ScheduleDate).Date.AddHours(Math.Floor(Convert.ToDouble(item.ScheduleEndTime))).AddMinutes((int)((item.ScheduleEndTime - (int)item.ScheduleEndTime) * 100)),

                    Description = "",
                    //AllDay = (item.ScheduleEndTime - item.ScheduleStartTime == 8 && item.ScheduleStartTime == 9) ? true : false,
                    AllDay = (item.ScheduleEndTime - item.ScheduleStartTime >= 8) ? true : false,
                    Recurrence = false,
                    RecurrenceRule = "",
                    Categorize = item.ReplaceTech == null ? "0" : item.ReplaceTech.ToString(),
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
            List<string> Colors = new List<string> { "#f8a398", "#56ca85", "#51a0ed", "Red", "Green", "Blue", "Yellow", "#D2E445", "#FF7FD4", "#D42AFF",
                                                     "#FF00AA", "#FF5500", "#AA00D5", "#FFFF00","#41c7f4", "#f441aa", "#d39f12", "#41aa90", "#12af12",
                                                     "#f6b5f5", "#c6b5f6", "#8acdd5", "#92aeb1", "#5fbf5b", "#bf9ef6", "#f69ee4", "#cfee7a", "#20ddf9",
                                                     "#7753ae", "#6483dd", "#93b6c2", "#6ceeaf","#ee916c", "#6cacee", "#63ce6f", "#bece63", "#ddab86",
                                                     "#dd86b7", "#9384c5", "#7ebcce", "#84c2bb", "#36ee80", "#d3cfe5", "#9bc6a9", "#f1e74e", "#e1dfc0" };
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
                        Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dr[1].ToString().ToLower()),
                        Color = Colors[i]
                    }
               );
                i = i + 1;
            }

            return Resourcelists;
        }

        public List<ResourceFields1> fnResourceList1(string Query)
        {
            DataTable dt1 = objMarsview.fn_FSM_View(Query);
            List<ResourceFields1> Resourcelists1 = new List<ResourceFields1>();

            CalendarTechnicianModel objTechModel = new CalendarTechnicianModel();

            foreach (DataRow dr1 in dt1.Rows)
            {

                Resourcelists1.Add
               (
                    new ResourceFields1()
                    {
                        Id = dr1[0].ToString(),
                        Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dr1[1].ToString().ToLower()),

                    }
               );

            }

            return Resourcelists1;
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

        public static bool IsTechUnAvailable(int techId, DateTime StartTime, out int replaceTech)
        {
            bool isAvilable = false;
            replaceTech = techId;
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                //int UnavailableStartHour = StartTime.Hour;
                //int UnavailableStartMin = StartTime.Minute;

                List<TechSchedule> holidays = (from sc in FarmerBrothersEntitites.TechSchedules
                                               join tech in FarmerBrothersEntitites.TECH_HIERARCHY on sc.TechId equals tech.DealerId
                                               where DbFunctions.TruncateTime(sc.ScheduleDate) == DbFunctions.TruncateTime(StartTime) && sc.TechId == techId
                                               && tech.SearchType == "SP" && tech.PostalCode != null
                                               select sc).ToList();

                if (holidays != null)
                {
                    foreach (TechSchedule holiday in holidays)
                    {
                        DateTime UnavailableStartDate = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + new DateTime().AddHours(Convert.ToDouble(holiday.ScheduleStartTime)).ToString("hh:mm tt"));
                        DateTime UnavailableEndDate = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + new DateTime().AddHours(Convert.ToDouble(holiday.ScheduleEndTime)).ToString("hh:mm tt"));

                        //TimeSpan UnavailableStartTime = new TimeSpan(StartTime.Hour, StartTime.Minute, 0);

                        //string[] stDate = holiday.ScheduleStartTime.ToString().Split('.');
                        //string[] edDate = holiday.ScheduleEndTime.ToString().Split('.');

                        //TimeSpan startHour = new TimeSpan(Convert.ToInt32(stDate[0]), Convert.ToInt32(stDate[1]), 0);
                        //TimeSpan endHour = new TimeSpan(Convert.ToInt32(edDate[0]), Convert.ToInt32(edDate[1]), 0);

                        ////int StartHour = StartTime.Hour;
                        ////int StartMin = StartTime.Minute;

                        //if(Convert.ToInt32(stDate[0]) <= StartHour && Convert.ToInt32(edDate[0]) >= StartHour 
                        //&& Convert.ToInt32(stDate[1]) <= StartMin && Convert.ToInt32(edDate[1]) >= StartMin)                        
                        if ((UnavailableStartDate <= StartTime) && (UnavailableEndDate > StartTime)) //Removed this and placed the above condition, as there is an issue with Datetime conversion and comparision
                        //if ((TimeSpan.Compare(UnavailableStartTime, startHour) == 1 || TimeSpan.Compare(UnavailableStartTime, startHour) == 0)
                            //&& (TimeSpan.Compare(endHour, UnavailableStartTime) == 1 || TimeSpan.Compare(endHour, UnavailableStartTime) == 0))
                        {
                            if (holiday.ReplaceTech != null && holiday.ReplaceTech != 0)
                            {
                                replaceTech = Convert.ToInt32(holiday.ReplaceTech);
                                IsTechUnAvailable(replaceTech, StartTime, out replaceTech);
                            }
                            else
                            { return true; }
                        }
                        else
                        {
                            isAvilable = false;
                        }
                    }
                }
            }
            return isAvilable;
        }

    }
}