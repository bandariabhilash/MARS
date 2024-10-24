using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;
using System.Configuration;
using FarmerBrothers.Data;

namespace FarmerBrothers.Models
{
    public class AuthenticationHelper
    {
        private string email { get; set; }
        private string userName = string.Empty;
        private string userPassword = string.Empty;
        private string searchFilter = string.Empty;
        private bool isAuthenticated = false;
        private readonly bool attributeOnly = true;



        public static FbUserMaster GetUserDetails(AdminUserModel user, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            return FarmerBrothersEntitites.FbUserMasters.Where(x => (x.Email == user.Email) && (x.Password == user.Password) && (x.IsActive == 1)).FirstOrDefault();
        }

        public bool IsAuthenticated(AdminUserModel user, FarmerBrothersEntities FarmerBrothersEntitites, out string displayName, out int roleId, ref int isFirstTimeLogin)
        {
            isAuthenticated = false;
            displayName = string.Empty;
            roleId = 0;
            try
            {
                FbUserMaster usermaster = AuthenticationHelper.GetUserDetails(user, FarmerBrothersEntitites);
                if (usermaster != null)
                {
                    displayName = usermaster.FirstName +" " + usermaster.LastName;
                    roleId = usermaster.RoleId;
                    isFirstTimeLogin = Convert.ToInt32(usermaster.IsFirstTimeLogin);
                    if (isFirstTimeLogin == 0 && PasswordChangeRequired(usermaster.PasswordUpdatedDate))
                    {
                        isFirstTimeLogin = 1;
                    }
                    System.Web.HttpContext.Current.Session["UserId"] = usermaster.UserId;
                    System.Web.HttpContext.Current.Session["CanExportSessionValue"] = usermaster.CanExport;
                    System.Web.HttpContext.Current.Session["TechId"] = usermaster.TechId;

                    if(usermaster.TechId != null)
                    {
                        TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == usermaster.TechId).FirstOrDefault();

                        if(techView != null && techView.FamilyAff != "SPT")
                        {
                            System.Web.HttpContext.Current.Session["TechType"] = "InternalTech";
                        }
                       else
                        {
                            System.Web.HttpContext.Current.Session["TechType"] = "ThirdPartyTech";
                        }

                    }

                    //System.Web.HttpContext.Current.Session["IsERFUser"] = usermaster.IsERFUser;
                    usermaster.UpdatedDate = DateTime.UtcNow.ToString();
                    FarmerBrothersEntitites.SaveChanges();
                    isAuthenticated = true;
                }

                return isAuthenticated;
            }
            catch (Exception ex)
            {
                return isAuthenticated;

            }

        }

        private bool PasswordChangeRequired(string pwdDate)
        {
            return (DateTime.Now - Convert.ToDateTime(pwdDate)).TotalDays >= 90;
        }
       public bool IsAuthenticated(AdminModel user, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            isAuthenticated = false;
            isAuthenticated = false;
            int UserId = (int)System.Web.HttpContext.Current.Session["UserId"];
            return IsAuthenticated(user, FarmerBrothersEntitites, UserId);

        }

        public bool IsAuthenticated(AdminModel user, FarmerBrothersEntities FarmerBrothersEntitites, int userid)
        {
            isAuthenticated = false;
            if(System.Web.HttpContext.Current.Session["UserId"]==null)
            {
                System.Web.HttpContext.Current.Session["UserId"] = userid;
            }
            try
            {
                
                FbUserMaster usermaster = FarmerBrothersEntitites.FbUserMasters.Where(x => (x.UserId == userid) && (x.Password == user.CurrentPassword)).FirstOrDefault();
                if (usermaster != null)
                {
                    isAuthenticated = true;
                }

                return isAuthenticated;
            }
            catch (Exception ex)
            {
                return isAuthenticated;

            }

        }


        public bool UpdatePassword(AdminModel user, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            isAuthenticated = false;
            try
            {
                int userId = (int)System.Web.HttpContext.Current.Session["UserId"];
                FbUserMaster usermaster = FarmerBrothersEntitites.FbUserMasters.Where(x => (x.UserId == userId) && (x.Password == user.CurrentPassword)).FirstOrDefault();
                if (usermaster != null)
                {
                    usermaster.Password = user.NewPassword;
                    usermaster.IsFirstTimeLogin = 0;
                    usermaster.PasswordUpdatedDate = DateTime.Now.ToShortDateString();
                    FarmerBrothersEntitites.SaveChanges();
                    isAuthenticated = true;
                }

                return isAuthenticated;
            }
            catch (Exception ex)
            {
                return isAuthenticated;

            }

        }
    }
}