using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class NewProfile : FBUserModel
    {
        public NewProfile()
        {
            using (FarmerBrothersEntities entities = new FarmerBrothersEntities())
            {
                UserType = Utilities.Utility.GetUserType(entities);
            }
        }
        public UserModulePrivilegeModel modulePrivilegeModel;
        public IList<UserType> UserType;
        //public FBUserModel userModel;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Apt { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Country { get; set; }
        public string ConfirmPassword { get; set; }
        public string MI { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string Manager { get; set; }
        public string Region { get; set; }
        public string Division { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Fax { get; set; }
        public string JDE { get; set; }
        public int UserTypeId { get; set; }
        public int CallClosure { get; set; }
        public int OnCallAccess { get; set; }
        public string TechId { get; set; }
        public bool IsTechUser { get; set; }
        //public bool IsERFUser { get; set; }

        public WorkOrderManagementSubmitType Operation { get; set; }        
        public string CustomerParent { get; set; }
    }
    public class UserModulePrivilegeModel
    {
        public IList<ApplicationRoleModel> AppRoleModel;
        //public IList<ApplicationModel> Modules;
        public ApplicationModel Modules;
        public IList<PrivilegeModel> Privileges;
    }

    //public class ModulePrivilegeModel
    //{
    //    public IList<ApplicationModel> Modules;
    //    public IList<PrivilegeModel> Privileges;

    //}
    public class ApplicationModel
    {
        public ApplicationModel()
        { }
        public ApplicationModel(Application application)
        {
            ApplicationId = application.ApplicationId;
            ApplicationName = application.ApplicationName;
            OrderId = application.OrderId;
        }
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int? OrderId { get; set; }
    }
    public class PrivilegeModel
    {
        public PrivilegeModel(Privilege privilege)
        {
            PrivilegeId = privilege.PrivilegeId;
            PrivilegeType = privilege.PrivilegeType;
        }
        public int PrivilegeId { get; set; }
        public string PrivilegeType { get; set; }
    }

    public class ApplicationRoleModel
    {
        public ApplicationRoleModel()
        { }
        public ApplicationRoleModel(Application app)
        {
            AppId = app.ApplicationId;
            AppName = app.ApplicationName;
        }
        public ApplicationRoleModel(Privilege pri)
        {
            PriType = pri.PrivilegeType;
        }
        public int AppId { get; set; }
        public string AppName { get; set; }
        public int PriId { get; set; }
        public string PriType { get; set; }
        public int? OrderId { get; set; }
    }

    public class UserApplicationModel
    {
        public UserApplicationModel()
        {

        }
        public UserApplicationModel(UserApplication userapp)
        {
            ID = userapp.ID;
            UserId = userapp.UserId;
            ApplicationId = userapp.ApplicationId;
            PrivilegeId = userapp.PrivilegeId;
        }
        public int ID { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> ApplicationId { get; set; }
        public Nullable<int> PrivilegeId { get; set; }

    }
}
