using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FBUserModel : BaseModel
    {
        public FBUserModel()
        {
            using (FarmerBrothersEntities entities = new FarmerBrothersEntities())
            {
                Roles = Utilities.Utility.GetRoles(entities);
                States = Utilities.Utility.GetStates(entities);
              
            }
            

        }



        public IList<FBUserResultModel> SearchResults;
        public IList<State> States;
        public IList<Role> Roles;
       

        public int Rid { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public bool IsTechnician { get; set; }
        public bool IsPrimaryTechnician { get; set; }
        public string EmailId { get; set; }


    }

    public class Role
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; }
    }

    
}