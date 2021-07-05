using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using Newtonsoft.Json;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace FarmerBrothers.Controllers
{
    public class UserController : BaseController
    {
        private IList<FBUserResultModel> GetUsers(FBUserModel FBUserModel, bool isName = false)
        {
            IList<FBUserResultModel> userResults = new List<FBUserResultModel>();
            IList<FbUserMaster> users = new List<FbUserMaster>();
            if (FBUserModel != null)
            {
                try
                {
                    if (!isName)
                    {
                        users = FarmerBrothersEntitites.FbUserMasters.Where(x => x.Email.Contains(FBUserModel.Email)).ToList();
                    }
                    else
                    {
                        var names = FBUserModel.Email.Trim().Split(' ');
                        var firstName = names[0];
                        if (names.Length >= 2)
                        {
                            var lastName = names[1];
                            users = FarmerBrothersEntitites.FbUserMasters.Where(x => x.FirstName.Contains(firstName) || x.LastName.Contains(lastName)).ToList();
                        }
                        else
                        {
                            users = FarmerBrothersEntitites.FbUserMasters.Where(x => x.FirstName.Contains(firstName)).ToList();
                        }
                    }
                }
                catch (Exception e)
                {
                    //Need to log this exception
                }
            }

            foreach (var user in users)
            {
                FBUserResultModel userResultModel = new FBUserResultModel(user);
                userResults.Add(userResultModel);
            }
            return userResults;
        }

        public List<FBReportModel> GetReports()
        {
            List<FBReport> reports = FarmerBrothersEntitites.FBReports.Where(r => r.Active == 1).OrderBy(r => r.report_name).ToList();

            List<FBReportModel> reportModelList = new List<FBReportModel>();
            foreach (FBReport report in reports)
            {
                FBReportModel reportModel = new FBReportModel(report);
                reportModelList.Add(reportModel);
            }
            return reportModelList;
        }

        public JsonResult GetUserReports(int UserId)
        {
            List<FBUserReport> reports = FarmerBrothersEntitites.FBUserReports.Where(r => r.UserId == UserId).OrderBy(r => r.FBReport.report_name).ToList();

            List<FBReportModel> reportModelList = new List<FBReportModel>();
            foreach (FBUserReport report in reports)
            {
                FBReportModel reportModel = new FBReportModel(report);
                reportModelList.Add(reportModel);
            }
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = reportModelList };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpPost]
        public JsonResult Search(FBUserModel FBUserModel, string SearchName)
        {
            if (string.IsNullOrWhiteSpace(FBUserModel.Email))
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<FbUserMaster>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<FBUserResultModel> users = null;
                if (SearchName == "Email")
                {
                    users = GetUsers(FBUserModel, false);
                }
                else
                {
                    users = GetUsers(FBUserModel, true);
                }
                FBUserModel.SearchResults = users;
                TempData["SearchCriteria"] = FBUserModel;
                ViewBag.datasource = FBUserModel.SearchResults;

                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = FBUserModel };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
        }
        public ActionResult UserSearch(int? isBack)
        {
            NewProfile newProfile = new Models.NewProfile();
            newProfile.SearchResults = new List<FBUserResultModel>();

            newProfile.modulePrivilegeModel = new UserModulePrivilegeModel();
            newProfile.modulePrivilegeModel.AppRoleModel = new List<ApplicationRoleModel>();


            List<Application> apps = FarmerBrothersEntitites.Applications.OrderBy(o => o.OrderId).Distinct().ToList();
            List<Privilege> privilege = FarmerBrothersEntitites.Privileges.Distinct().ToList();

            newProfile.modulePrivilegeModel.Modules = new ApplicationModel();
            foreach (Application app in apps)
            {
                newProfile.modulePrivilegeModel.Modules = new ApplicationModel(app);
                ApplicationRoleModel aprole = new ApplicationRoleModel();
                aprole.AppName = app.ApplicationName;
                aprole.AppId = app.ApplicationId;
                aprole.PriType = "No-Permission";
                aprole.OrderId = Convert.ToInt16(app.OrderId);
                newProfile.modulePrivilegeModel.AppRoleModel.Add(aprole);
            }
            newProfile.modulePrivilegeModel.AppRoleModel = newProfile.modulePrivilegeModel.AppRoleModel.OrderBy(o => o.OrderId).ToList();
            newProfile.modulePrivilegeModel.Privileges = new List<PrivilegeModel>();
            foreach (Privilege pri in privilege)
            {
                newProfile.modulePrivilegeModel.Privileges.Add(new PrivilegeModel(pri));
            }


            IList<FbUserMaster> users = new List<FbUserMaster>();
            users = FarmerBrothersEntitites.FbUserMasters.ToList();

            foreach (var user in users)
            {
                FBUserResultModel userResultModel = new FBUserResultModel(user);
                newProfile.SearchResults.Add(userResultModel);
            }
            ViewBag.datasource = newProfile.SearchResults;

            //Reports Code 
            int UserId = System.Web.HttpContext.Current.Session["UserId"] != null ? (int)System.Web.HttpContext.Current.Session["UserId"] : 0;
            ViewBag.listboxdata1 = GetReports();
            ViewBag.listboxdata2 = new List<FBReportModel>();

            return View("Users", newProfile);
        }
        public ActionResult CellEditUpdate(FBUserResultModel value)
        {
            ErrorCode code = ErrorCode.SUCCESS;
            string message = string.Empty;
            JsonResult jsonResult = new JsonResult();
            /*FbUserMaster EmailExistuser = FarmerBrothersEntitites.FbUserMasters.Where(u => u.Email == value.Email).Where(u => u.UserId != value.UserId).FirstOrDefault();
            if (EmailExistuser == null)
            {*/
            try
            {
                FbUserMaster user = FarmerBrothersEntitites.FbUserMasters.First(u => u.UserId == value.UserId);
                user.IsActive = Convert.ToInt16(value.IsActive);
                user.CanExport = Convert.ToInt16(value.CanExport);
                user.Company = value.Company;
                user.FirstName = value.FirstName;
                user.LastName = value.LastName;
                user.City = value.City;
                user.State = value.State;
                user.Zip = value.Zip;
                user.Phone = value.Phone;
                user.Email = value.Email;
                user.EmailId = value.EmailId;
                user.TechId = value.TechId;
                user.IsTechnician = Convert.ToInt16(value.IsTechnician);
                if (value.IsTechnician && (value.TechId == 0 || value.TechId == null))
                {
                    message = "|Please enter the TechId for User: " + value.FirstName + " " + value.LastName + " ("+value.UserId+")" ;

                    jsonResult.Data = new { serverError = code, message = message };
                    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                    return jsonResult;
                }
                if (!value.IsTechnician && (value.TechId != 0 || value.TechId != null))
                {
                    user.TechId = null;
                }

                if (user.Password != value.UserPassword)
                {
                    user.Password = value.UserPassword;
                    user.IsFirstTimeLogin = 1;
                    user.PasswordUpdatedDate = DateTime.UtcNow.ToShortDateString();
                    user.UpdatedDate = DateTime.UtcNow.ToShortDateString();
                }
                FarmerBrothersEntitites.SaveChanges();
                message = "|User Details saved successfully!";
            }
            catch (Exception)
            {

                code = ErrorCode.ERROR;
                message = "|There is a problem in user details update!";
            }
            /*}
            else
            {
                code = ErrorCode.ERROR;
                message = "|User Name already exist, Please choose different User Name.";
            }*/

            
            jsonResult.Data = new { serverError = code, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult NewProfile()
        {
            NewProfile user = new NewProfile();
            return View(user);
        }

        public JsonResult IsValidTechnician(int TechId)
        {
            bool isExist = false;
            TECH_HIERARCHY techDetails = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == TechId).FirstOrDefault();
            FbUserMaster user = FarmerBrothersEntitites.FbUserMasters.Where(t => t.TechId == TechId).FirstOrDefault();
            if (techDetails != null && user == null)
            {
                isExist = true;
            }
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = isExist };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult CreateProfile([ModelBinder(typeof(NewProfileBinder))] NewProfile profile)
        {
            ErrorCode success = ErrorCode.SUCCESS;
            try
            {
                using (FarmerBrothersEntities entities = new FarmerBrothersEntities())
                {
                    int PrimaryTechId = 0;
                    if (profile.IsPrimaryTechnician)
                    {
                        PrimaryTechId = SavePrimaryTech(profile.Company);
                    }

                    FbUserMaster user = new FbUserMaster();
                    if (string.IsNullOrEmpty(profile.RoleId.ToString()) || profile.RoleId == 0)
                    {
                        user.RoleId = 102;
                    }
                    else
                    {
                        user.RoleId = profile.RoleId;
                    }

                    IndexCounter counter = Utility.GetIndexCounter("NewUserID", 1);
                    counter.IndexValue++;
                    //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                    user.UserId = counter.IndexValue.Value;

                    user.FirstName = profile.FirstName;
                    user.LastName = profile.LastName;
                    user.Company = profile.Company;
                    user.Email = profile.Email;
                    user.EmailId = profile.EmailId;
                    user.Password = profile.Password;
                    user.ConfirmPassword = profile.ConfirmPassword;
                    user.Title = profile.Title;
                    user.Manager = profile.Manager;
                    user.MI = profile.MI;
                    user.Region = profile.Region;
                    user.Division = profile.Division;
                    user.Address = profile.Address;
                    user.City = profile.City;
                    user.State = profile.State;
                    user.Zip = profile.Zip;
                    user.Phone = profile.Phone;
                    user.Fax = profile.Fax;
                    user.JDE = profile.JDE;
                    user.UserTypeId = 3; //3 = MARS & B2B
                    user.IsActive = 1;
                    user.CanExport = 1;
                    user.IsFirstTimeLogin = 1;
                    user.CreatedDate = DateTime.UtcNow.ToShortDateString();
                    user.UpdatedDate = DateTime.UtcNow.ToShortDateString();

                    if (profile.IsPrimaryTechnician && PrimaryTechId > 0)
                    {
                        user.TechId = Convert.ToInt32(PrimaryTechId);
                        user.IsTechnician = 1;
                    }
                    else
                    {
                        if (profile.TechId != null && Convert.ToInt32(profile.TechId) != 0)
                        {
                            user.TechId = Convert.ToInt32(profile.TechId);
                            user.IsTechnician = 1;
                        }
                    }

                    //if(profile.IsTechnician)
                    //{
                    //    user.TechId = Convert.ToInt32(profile.TechId);
                    //    user.IsTechnician = 1;
                    //    user.IsERFUser = 0;
                    //    user.CanExport = 1;
                    //}
                    //else if(profile.IsERFUser)
                    //{
                    //    user.TechId = null;
                    //    user.IsERFUser = 1;
                    //    user.IsTechnician = 0;
                    //    user.CanExport = 0;
                    //}
                    //else
                    //{
                    //    user.TechId = null;
                    //    user.IsERFUser = 0;
                    //    user.IsTechnician = 0;
                    //    user.CanExport = 1;
                    //}

                    entities.FbUserMasters.Add(user);
                    entities.SaveChanges();
                    foreach (ApplicationRoleModel item in profile.modulePrivilegeModel.AppRoleModel)
                    {
                        UserApplication userapp = new UserApplication();
                        userapp.UserId = counter.IndexValue.Value;
                        userapp.ApplicationId = item.AppId;
                        userapp.PrivilegeId = WorkOrderLookup.GetPrivileageIdByName(item.PriType, entities);
                        entities.UserApplications.Add(userapp);
                    }
                    entities.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                success = ErrorCode.ERROR;
            }

            var redirectUrl = string.Empty;
            if (Request != null)
            {
                redirectUrl = new UrlHelper(Request.RequestContext).Action("UserSearch", "User", new { isBack = 0 });
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = success, Url = redirectUrl };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        private int SavePrimaryTech(string CompanyName)
        {
            int primaryTechId = 0;

            try
            {
                using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                {
                    IndexCounter counter = Utility.GetIndexCounter("PrimaryTechnician", 1);
                    counter.IndexValue++;

                    FbPrimaryTechnician PT = new FbPrimaryTechnician();

                    PT.PrimaryTechId = counter.IndexValue.Value;
                    PT.PrimaryTechName = CompanyName;

                    entity.FbPrimaryTechnicians.Add(PT);
                    entity.SaveChanges();

                    //primaryTechId = effectedRecords > 0 ? PT.PrimaryTechId : 0;
                    primaryTechId = PT.PrimaryTechId;
                }
            }
            catch(Exception ex)
            {

            }

            return primaryTechId;
        }

        public ActionResult ApplicationPrivilegeUpdate(ApplicationRoleModel value)
        {
            IList<ApplicationRoleModel> appRoles = TempData["ApplicationPrivilege"] as IList<ApplicationRoleModel>;
            if (appRoles == null)
            {
                appRoles = new List<ApplicationRoleModel>();
            }
            ApplicationRoleModel appRole = appRoles.Where(n => n.AppName == value.AppName).Where(p => p.PriType == value.PriType).FirstOrDefault();


            if (appRole != null)
            {

                appRole.AppName = value.AppName;
                appRole.PriType = value.PriType;

            }

            TempData["ApplicationPrivilege"] = appRoles;
            TempData.Keep("ApplicationPrivilege");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApplicationPrivilegeInsert(ApplicationRoleModel value)
        {
            IList<ApplicationRoleModel> appRoles = TempData["ApplicationPrivilege"] as IList<ApplicationRoleModel>;
            if (appRoles == null)
            {
                appRoles = new List<ApplicationRoleModel>();
            }
            ApplicationRoleModel appRole = appRoles.Where(n => n.AppName == value.AppName).Where(p => p.PriType == value.PriType).FirstOrDefault();

            if (appRole == null)
            {
                if (TempData["ApplicationPrivilegeId"] != null)
                {
                    int appId = Convert.ToInt32(TempData["ApplicationPrivilegeId"]);
                    value.AppId = appId + 1;
                    TempData["ApplicationPrivilegeId"] = appId + 1;
                }
                else
                {
                    value.AppId = 1;
                    TempData["ApplicationPrivilegeId"] = 1;
                }

                appRoles.Add(value);
                TempData["ApplicationPrivilege"] = appRoles;
                TempData.Keep("ApplicationPrivilege");
            }

            return Json(value, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ApplicationPrivilegeDelete(int key)
        {
            IList<ApplicationRoleModel> appRoles = TempData["ApplicationPrivilege"] as IList<ApplicationRoleModel>;
            ApplicationRoleModel appRole = appRoles.Where(n => n.AppId == key).FirstOrDefault();
            appRoles.Remove(appRole);
            TempData["ApplicationPrivilege"] = appRoles;
            TempData.Keep("ApplicationPrivilege");
            return Json(appRoles, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserPrivilege(DataManager dm)
        {
            NewProfile newProfile = new Models.NewProfile();
            newProfile.SearchResults = new List<FBUserResultModel>();

            newProfile.modulePrivilegeModel = new UserModulePrivilegeModel();
            newProfile.modulePrivilegeModel.AppRoleModel = new List<ApplicationRoleModel>();
            newProfile.modulePrivilegeModel.Privileges = new List<PrivilegeModel>();
            if (dm.Where != null && dm.Where.Count > 0)
            {
                int userid = Convert.ToInt32(dm.Where[0].value);
                TempData["UserApplicationRoleId"] = userid;
                TempData.Keep("UserApplicationRoleId");
                List<Application> apps = FarmerBrothersEntitites.Applications.OrderBy(o => o.OrderId).Distinct().ToList();
                //List<UserApplication> userapp = FarmerBrothersEntitites.UserApplications.Where(u => u.UserId == userid).ToList();
                List<Privilege> privilege = FarmerBrothersEntitites.Privileges.Distinct().ToList();
                //foreach (UserApplication item in userapp)
                //{
                //    ApplicationRoleModel appRole = new ApplicationRoleModel();
                //    appRole.AppId = Convert.ToInt32(item.ApplicationId);
                //    appRole.PriId = Convert.ToInt32(item.PrivilegeId);
                //    appRole.AppName = item.Application.ApplicationName;
                //    appRole.PriType = item.Privilege.PrivilegeType;
                //    appRole.OrderId = WorkOrderLookup.GetApplicationOrderById(appRole.AppId, FarmerBrothersEntitites);
                //    newProfile.modulePrivilegeModel.AppRoleModel.Add(appRole);

                //    foreach (Privilege pri in privilege)
                //    {
                //        newProfile.modulePrivilegeModel.Privileges.Add(new PrivilegeModel(pri));
                //    }
                //}

                foreach (Application app in apps)
                {
                    UserApplication userapp = FarmerBrothersEntitites.UserApplications.Where(u => u.UserId == userid && u.ApplicationId == app.ApplicationId).FirstOrDefault();

                    ApplicationRoleModel appRole = new ApplicationRoleModel();
                    if (userapp != null)
                    {
                        appRole.AppId = Convert.ToInt32(userapp.ApplicationId);
                        appRole.PriId = Convert.ToInt32(userapp.PrivilegeId);
                        appRole.AppName = userapp.Application.ApplicationName;
                        appRole.PriType = userapp.Privilege.PrivilegeType;
                        appRole.OrderId = WorkOrderLookup.GetApplicationOrderById(appRole.AppId, FarmerBrothersEntitites);
                        newProfile.modulePrivilegeModel.AppRoleModel.Add(appRole);
                    }
                    else
                    {
                        appRole.AppId = Convert.ToInt32(app.ApplicationId);
                        appRole.PriId = 4;
                        appRole.AppName = app.ApplicationName;
                        appRole.PriType = "No-Permission";
                        appRole.OrderId = WorkOrderLookup.GetApplicationOrderById(appRole.AppId, FarmerBrothersEntitites);
                        newProfile.modulePrivilegeModel.AppRoleModel.Add(appRole);
                    }

                    foreach (Privilege pri in privilege)
                    {
                        newProfile.modulePrivilegeModel.Privileges.Add(new PrivilegeModel(pri));
                    }
                }



            }
            newProfile.modulePrivilegeModel.AppRoleModel = newProfile.modulePrivilegeModel.AppRoleModel.OrderBy(o => o.OrderId).ToList();
            return Json(new { result = newProfile.modulePrivilegeModel.AppRoleModel, count = newProfile.modulePrivilegeModel.AppRoleModel.Count });
        }

        public ActionResult UserPrivilegeUpdate(ApplicationRoleModel value)
        {
            int UserApplicationRoleId = TempData["UserApplicationRoleId"] != null ? (int)TempData["UserApplicationRoleId"] : 0;
            UserApplication userapp = FarmerBrothersEntitites.UserApplications.Where(u => u.UserId == UserApplicationRoleId).Where(a => a.ApplicationId == value.AppId).FirstOrDefault();
            if (userapp != null)
            {
                userapp.PrivilegeId = WorkOrderLookup.GetPrivileageIdByName(value.PriType, FarmerBrothersEntitites);
            }
            else
            {
                UserApplication usrapp = new UserApplication();
                usrapp.UserId = UserApplicationRoleId;
                usrapp.ApplicationId = value.AppId;
                usrapp.PrivilegeId = WorkOrderLookup.GetPrivileageIdByName(value.PriType, FarmerBrothersEntitites);
                FarmerBrothersEntitites.UserApplications.Add(usrapp);
            }
            FarmerBrothersEntitites.SaveChanges();

            System.Web.HttpContext.Current.Session["UserPrivilege" + UserApplicationRoleId] = Security.GetUserPrivilegeByUserId(UserApplicationRoleId, FarmerBrothersEntitites);
            TempData.Keep("UserApplicationRoleId");
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult IsEmailExist(string emaiId)
        {
            bool result = Security.IsEmailIdExist(emaiId);
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = result };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        public JsonResult UpdateUserReports(string listdata, int UserId)
        {

            ErrorCode code = ErrorCode.SUCCESS;
            string message = string.Empty;
            using (System.Data.Entity.DbContextTransaction dbTran = FarmerBrothersEntitites.Database.BeginTransaction())
            {
                try
                {
                    var listboxobjvalue = JsonConvert.DeserializeObject(listdata);
                    List<FBReportModel> reportsList = JsonConvert.DeserializeObject<List<FBReportModel>>(listdata);

                    List<FBUserReport> ureports = FarmerBrothersEntitites.FBUserReports.Where(r => r.UserId == UserId).ToList();

                    foreach (FBUserReport rp in ureports)
                    {
                        FarmerBrothersEntitites.FBUserReports.Remove(rp);
                    }
                    FarmerBrothersEntitites.SaveChanges();
                    foreach (FBReportModel report in reportsList)
                    {
                        FBUserReport userReport = new FBUserReport();
                        userReport.UserId = UserId;
                        userReport.report_id = report.report_id;
                        FarmerBrothersEntitites.FBUserReports.Add(userReport);
                    }
                    FarmerBrothersEntitites.SaveChanges();
                    System.Web.HttpContext.Current.Session["UserReports" + UserId] = Security.GetUserReports(UserId, FarmerBrothersEntitites);
                    dbTran.Commit();
                    message = "|User Reports saved successfully!";
                }
                catch (Exception)
                {
                    dbTran.Rollback();
                    code = ErrorCode.ERROR;
                    message = "|There is a problem to save user reports!";
                }
            }
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { serverError = code, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;

        }

        public FileResult UserListExport()
        {
            List<FBUserResultModel> resultsList = new List<FBUserResultModel>();

            FBUserModel userModel = TempData["SearchCriteria"] == null ? new FBUserModel() : TempData["SearchCriteria"] as FBUserModel;
            string GridModel = HttpContext.Request.Params["GridModel"];
            NewProfile newProfile = new Models.NewProfile();
            newProfile.SearchResults = new List<FBUserResultModel>();
            IList<FbUserMaster> users = new List<FbUserMaster>();
            users = FarmerBrothersEntitites.FbUserMasters.ToList();
            foreach (var user in users)
            {
                FBUserResultModel userResultModel = new FBUserResultModel(user);
                newProfile.SearchResults.Add(userResultModel);
                resultsList.Add(userResultModel);
            }

            string[] columns = { "Company", "FirstName", "LastName", "Phone", "IsActive"};
            byte[] filecontent = ExcelExportHelper.ExportExcel(resultsList, "User Details", false, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "UserList.xlsx");
        }

        [HttpPost]
        public void ExcelExportUsersList()
        {
            FBUserModel userModel = TempData["SearchCriteria"] == null ? new FBUserModel() : TempData["SearchCriteria"] as FBUserModel;
            string GridModel = HttpContext.Request.Params["GridModel"];
            NewProfile newProfile = new Models.NewProfile();
            newProfile.SearchResults = new List<FBUserResultModel>();
            IList<FbUserMaster> users = new List<FbUserMaster>();
            users = FarmerBrothersEntitites.FbUserMasters.ToList();
            foreach (var user in users)
            {
                FBUserResultModel userResultModel = new FBUserResultModel(user);
                newProfile.SearchResults.Add(userResultModel);
            }

            ExcelExport exp = new ExcelExport();
            var DataSource = newProfile.SearchResults;
            GridProperties obj = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), GridModel);
            exp.Export(obj, DataSource, "Export.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");


            //Syncfusion.EJ.Export.ExcelExport exp = new Syncfusion.EJ.Export.ExcelExport();
            //var DataSource = newProfile.SearchResults;
            //GridProperties obj = ConvertUserGridObject(GridModel);
            ////obj.ChildGrid.DataSource = GetUserPrivilege
            //Syncfusion.EJ.Export.GridExcelExport expo = new Syncfusion.EJ.Export.GridExcelExport();
            //expo.IncludeChildGrid = true;
            //exp.Export(obj, DataSource, expo);
        }

        private GridProperties ConvertUserGridObject(string gridProperty)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            IEnumerable div = (IEnumerable)serializer.Deserialize(gridProperty, typeof(IEnumerable));
            GridProperties gridProp = new GridProperties();
            foreach (KeyValuePair<string, object> ds in div)
            {
                var property = gridProp.GetType().GetProperty(ds.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    Type type = property.PropertyType;
                    object value = null;
                    string serialize = serializer.Serialize(ds.Value);
                    if (ds.Key == "childGrid")
                        value = ConvertGridObject(serialize);
                    else
                        value = serializer.Deserialize(serialize, type);
                    property.SetValue(gridProp, value, null);
                }
            }
            return gridProp;
        }
    }
}