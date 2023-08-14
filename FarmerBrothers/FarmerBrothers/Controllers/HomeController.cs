using FarmerBrothers.Data;
using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.Caching;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;

namespace FarmerBrothers.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult ChangePassword()
        {
            return View();
        }
        public ActionResult Login()
        {
            Session["UserRoles"] = null;
            Session["Username"] = null;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AdminUserModel objAdminUserModel)
        {
            AuthenticationHelper fbAuthentication = new AuthenticationHelper();
            if (ModelState.IsValid)
            {
                string displayName;
                int roleId;
                int isFirstTimeLogin = 0;
                FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities();
                if (fbAuthentication.IsAuthenticated(objAdminUserModel, FarmerBrothersEntitites, out displayName, out roleId, ref isFirstTimeLogin))
                {
                    int userId = (int)System.Web.HttpContext.Current.Session["UserId"];
                    System.Web.HttpContext.Current.Session["Username"] = displayName;
                    System.Web.HttpContext.Current.Session["UserRoleId"] = roleId;
                    if (isFirstTimeLogin == 1)
                    {
                        return RedirectToAction("ChangePassword", "Home");
                    }
                    FormsAuthentication.SetAuthCookie(objAdminUserModel.Email, true);
                    System.Web.HttpContext.Current.Session["UserPrivilege" + userId] = Security.GetUserPrivilegeByUserId(userId, FarmerBrothersEntitites);
                    Dictionary<string, string> userPrivilege = (Dictionary<string, string>)System.Web.HttpContext.Current.Session["UserPrivilege" + userId];

                    System.Web.HttpContext.Current.Session["UserReports" + userId] = Security.GetUserReports(userId, FarmerBrothersEntitites);

                    if (Session["TechId"] != null )//&& userPrivilege["Call Closure"] != "No-Permission")
                    {
                        return RedirectToAction("CallClosure", "CallClosure");
                    }
                    //if (Session["IsERFUser"] != null && Convert.ToInt32(Session["IsERFUser"]) == 1)
                    //{
                    //    return RedirectToAction("CustomerSearch", "CustomerSearch");
                    //}
                    else
                    {
                        if (userPrivilege["Customer"] != "No-Permission" || userPrivilege["ERFNew"] == "Full")
                        {
                            return RedirectToAction("CustomerSearch", "CustomerSearch");
                        }
                        else if (userPrivilege["CustomerDashboard"] != "No-Permission")
                        {
                            return RedirectToAction("CustomerDashboard", "Customer");
                        }
                        else if (userPrivilege["Work Order"] != "No-Permission")
                        {
                            return RedirectToAction("WorkorderSearch", "Workorder");
                        }
                        else if (userPrivilege["ERF"] != "No-Permission")
                        {
                            return RedirectToAction("ERFSearch", "ERF");
                        }
                        else if (userPrivilege["Reports"] != "No-Permission")
                        {
                            return RedirectToAction("AllReports", "Reports");
                        }
                        else if (userPrivilege["Unknown Customer"] != "No-Permission")
                        {
                            return RedirectToAction("unknownCustomer", "UnknownCustomer");
                        }
                        else if (userPrivilege["Reopen Work Order"] != "No-Permission")
                        {
                            return RedirectToAction("ReopenWorkOrder", "ReopenWorkOrder");
                        }
                        else if (userPrivilege["Technician Schedule"] != "No-Permission")
                        {
                            return RedirectToAction("Calendar", "TechnicianCalendar");
                        }
                        else if (userPrivilege["User Maintenance"] != "No-Permission")
                        {
                            return RedirectToAction("UserSearch", "User");
                        }
                        else if (userPrivilege["Holiday List Maintenance"] != "No-Permission")
                        {
                            return RedirectToAction("HolidayList", "Holiday");
                        }
                        else if (userPrivilege["Customer Notes"] != "No-Permission")
                        {
                            return RedirectToAction("CustomerNotes", "CustomerNotes");
                        }
                        else if (userPrivilege["Technician Update"] != "No-Permission")
                        {
                            return RedirectToAction("TechnicianUpdate", "TechnicianUpdate");
                        }
                        else if (userPrivilege["WO Customer Update"] != "No-Permission")
                        {
                            return RedirectToAction("WorkOrderCustomerUpdate", "WorkOrderCustomerUpdate");
                        }
                        else
                        {
                            return View("NoPermission");
                        }
                    }


                }
                else
                {
                    ViewBag.Message = "User Name and Password does not match";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "Please Enter UserName and Password";
                return View();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(AdminModel objAdminUserModel)
        {
            AuthenticationHelper fbAuthentication = new AuthenticationHelper();
            if (ModelState.IsValid)
            {
                if (objAdminUserModel.NewPassword == objAdminUserModel.CurrentPassword)
                {
                    ViewBag.Message = "New Password  and Current Password should not be same";
                    return View();
                }

                if (objAdminUserModel.NewPassword != objAdminUserModel.ConfirmPassowrd)
                {
                    ViewBag.Message = "New Password  and Confirm Password does not match";
                    return View();
                }

                FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities();
                if (fbAuthentication.IsAuthenticated(objAdminUserModel, FarmerBrothersEntitites))
                {
                    fbAuthentication.UpdatePassword(objAdminUserModel, FarmerBrothersEntitites);

                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    ViewBag.Message = "Current Password does not match";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "Please Enter the required fields";
                return View();
            }
        }
        public ActionResult Error()
        {
            return View("Error");

        }
        public ActionResult Logout()
        {
            List<string> cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                MemoryCache.Default.Remove(cacheKey);
            }

            Session["Username"] = null;
            Session.Clear();
            FormsAuthentication.SignOut();
            Session.Abandon();

            return RedirectToAction("Login", "Home");
        }
        [HttpGet]
        public ActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgetPassword(AdminUserModel adminuser)
        {
            bool isValid = Security.IsEmailIdExist(adminuser.Email);
            if (isValid)
            {
                string mailId = adminuser.Email;
                string subject = "Reset Password";
                string temppwd = RandomPassword();
                FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities();
                FbUserMaster users = FarmerBrothersEntitites.FbUserMasters.Where(x => x.Email == mailId).FirstOrDefault();
                users.Password = temppwd;
                FarmerBrothersEntitites.SaveChanges();
                bool isSent = SendPasswordResetMail(temppwd, mailId, ConfigurationManager.AppSettings["DispatchMailFromAddress"], subject, users.UserId);
                if (isSent)
                {
                    ViewBag.Message = "Current Password sent successfully, Please check the mail and Reset the Password";
                }
            }
            return View();
        }

        public string RandomPassword()
        {
            string pwd = string.Empty;
            int lengthOfPassword = 8;
            string valid = "abcdefghijklmnozABCDEFGHIJKLMNOZ1234567890";
            StringBuilder strB = new StringBuilder(100);
            Random random = new Random();
            while (0 < lengthOfPassword--)
            {
                strB.Append(valid[random.Next(valid.Length)]);
            }
            pwd = strB.ToString();
            return pwd;
        }

        public bool SendPasswordResetMail(string pwd, string toAddress, string fromAddress, string subject, int userid)
        {
            string url = ConfigurationManager.AppSettings["ResetPwdUrl"];
            StringBuilder pwdResetEmailBody = new StringBuilder();

            pwdResetEmailBody.Append("<BR>");
            pwdResetEmailBody.Append("<BR>");
            pwdResetEmailBody.Append("<BR>");

            pwdResetEmailBody.Append("Password Reset Link is:<a href=" + url + "?userid=" + userid + ">Change Password</a>");
            pwdResetEmailBody.Append("<BR>");
            pwdResetEmailBody.Append("<BR>");
            pwdResetEmailBody.Append("<BR>");
            pwdResetEmailBody.Append("Current Password is: " + pwd);

            string contentId = Guid.NewGuid().ToString();
            string logoPath = Server.MapPath("~/img/mainlogo.jpg");
            pwdResetEmailBody = pwdResetEmailBody.Replace("cid:logo", "cid:" + contentId);

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString
               (pwdResetEmailBody.ToString(), null, MediaTypeNames.Text.Html);

            LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
            inline.ContentId = contentId;
            avHtml.LinkedResources.Add(inline);

            var message = new MailMessage();

            message.AlternateViews.Add(avHtml);

            message.IsBodyHtml = true;
            message.Body = pwdResetEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();

            bool result = true;
            string mailTo = toAddress;
            if (!string.IsNullOrWhiteSpace(mailTo))
            {
                string[] addresses = mailTo.Split(';');
                foreach (string address in addresses)
                {
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        message.To.Add(new MailAddress(address));
                    }
                }

                message.From = new MailAddress(fromAddress);
                message.Subject = subject;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = 25;

                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        result = false;
                    }
                }

            }
            return result;
        }
        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ResetPassword(AdminModel objAdminUserModel)
        {

            int userid = Convert.ToInt32(Session["UserIdFromEmail"]);
            System.Web.HttpContext.Current.Session["UserId"] = userid;
            return ChangePassword(objAdminUserModel);

        }


    }
}