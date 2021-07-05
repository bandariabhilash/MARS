using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FarmerBrothers.Data;
using FarmerBrothers.Utilities;
using System.Data;

namespace FarmerBrothers.Models
{
    public class TechnicianUpdateModel : BaseModel
    {
        public TechnicianUpdateModel()
        {
            TechId = 0;
            TechName = string.Empty;
            City = string.Empty;
            State = string.Empty;
            City = string.Empty;
            State = string.Empty;
            IsActive = true;
            AreaCode = string.Empty;
            PhoneNumber = string.Empty;
            EmailCC = string.Empty;
            RimEmail = string.Empty;
            Zip = string.Empty;
            SearchType = "SP";
            BranchAssociation =0;
            FamilyAff = string.Empty;
            AlternativePhone = string.Empty;
            AutoDispatch = 2;
            RegionName = string.Empty;
            RegionNumber = string.Empty;
            FieldServiceManager = string.Empty;            
            Branches = new List<BranchRegion>();
            Regions = new List<BranchRegion>();
            FSM = new List<BranchRegion>();
            PrimaryTech = new List<BranchRegion>();
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                Branches = WorkOrderLookup.GetTechBranches(FarmerBrothersEntitites);
                Regions = WorkOrderLookup.GetTechRegions(FarmerBrothersEntitites);
                States = Utility.GetStates(FarmerBrothersEntitites);
                FSM = WorkOrderLookup.GetFSM(FarmerBrothersEntitites);
                PrimaryTech = WorkOrderLookup.GetPrimaryTechnicians(FarmerBrothersEntitites);
            }

            //DataTable dt = Security.GetFamilyAff();
            List<BranchRegion> TechnicianAffs = new List<BranchRegion>();         

            BranchRegion tech = new BranchRegion();
            tech.Number = "n/a";
            tech.Name = "Please Select";
            TechnicianAffs.Add(tech);


            BranchRegion tech1 = new BranchRegion();
            tech1.Number = "SPD";
            tech1.Name = "Internal";
            TechnicianAffs.Add(tech1);
            BranchRegion tech2 = new BranchRegion();
            tech2.Number = "SPT";
            tech2.Name = "3rd Party";
            TechnicianAffs.Add(tech2);

            FamilyAffs = TechnicianAffs;
        }

        public TechnicianUpdateModel(TECH_HIERARCHY tech, FarmerBrothersEntities entity)
        {
            this.TechId = tech.DealerId;
            this.TechName = tech.CompanyName;
            this.City = tech.City;
            this.State = tech.State;
            this.IsActive = tech.SearchType == "SP" ? true : false;
            this.AreaCode = tech.AreaCode;
            this.PhoneNumber = Utility.FormatPhoneNumber(tech.AreaCode + tech.Phone);
            this.AlternativePhone= Utility.FormatPhoneNumber(tech.AlternativePhone);
            this.EmailCC = tech.EmailCC;
            this.RimEmail = tech.RimEmail;
            this.Zip = tech.PostalCode;
            this.BranchNumber = tech.BranchNumber;
            this.BranchName = tech.BranchName;
            this.ParentTechnicianId = Convert.ToInt32(tech.PrimaryTechId);
            this.FamilyAff = tech.FamilyAff == "SPD" ? "Internal" : "3rd Party";

            //BranchESM BEsm = null;
            //using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            //{
            //    BEsm = FarmerBrothersEntitites.BranchESMs.Where(b => b.BranchNo == tech.BranchNumber).FirstOrDefault();
            //}
            //this.FieldServiceManager = BEsm == null ? "" : BEsm.ESMName;
            this.FieldServiceManager = tech.FieldServiceManager;
            //IEnumerable<List<ESMCCMRSMEscalation>> esmnames = (from m in entity.ESMCCMRSMEscalations group m by m.ESMName into esName select esName.ToList()).ToList();
            //ESMCCMRSMEscalation esm = entity.ESMCCMRSMEscalations.Where(e => e.ZIPCode == tech.PostalCode).FirstOrDefault();
            //this.FieldServiceManager = esm == null ? "" : esm.ESMName;

            string tmptechId = Convert.ToString(ParentTechnicianId);
            this.ParentTechnicianName = WorkOrderLookup.GetPrimaryTechnicians(entity).ToList().Where(t => t.Number == tmptechId).Select(n => n.Name).FirstOrDefault() ;
            if (string.IsNullOrEmpty(ParentTechnicianName))
            {
                this.ParentTechnicianName = tech.CompanyName;
            }
            this.Branches = new List<BranchRegion>();
            BranchRegion branch = new BranchRegion();
            branch.Number = tech.BranchNumber;
            branch.Name = tech.BranchName;
            this.Branches.Add(branch);
        }

        public int SearchTechId { get; set; }
        public string SearchTechName { get; set; }        
        public int TechId { get; set; }
        public string TechName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public bool IsActive { get; set; }
        public string AreaCode { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailCC { get; set; }
        public string RimEmail { get; set; }  
        public string Zip { get; set; }
        public string BranchNumber { get; set; }
        public string BranchName { get; set; }
        public bool InactiveTechnicians { get; set; }

        #region New Technician

        public string SearchType { get; set; }
        public int? BranchAssociation { get; set; }
        public string FamilyAff { get; set; }
        public string AlternativePhone { get; set; }      
        public int AutoDispatch { get; set; }
        public string RegionName { get; set; }
        public string FieldServiceManager { get; set; }
        public string RegionNumber { get; set; }
        public IList<State> States;
        public List<BranchRegion> Branches { get; set; }
        public List<BranchRegion> Regions { get; set; }
        public List<BranchRegion> FamilyAffs { get; set; }
        public List<BranchRegion> FSM { get; set; }

        public List<BranchRegion> PrimaryTech { get; set; }
        public string ParentTechnicianName { get; set; }
        public int ParentTechnicianId { get; set; }

        public int Operation { get; set; }
        #endregion

        public List<TechnicianUpdateModel> SearchResults { get; set; }

    }

    public class BranchRegion
    {
        public BranchRegion()
        {

        }
        public BranchRegion(string name,string number)
        {
            this.Name = name;
            this.Number = number;
        }
        public string Name { get; set; }
        public string Number { get; set; }
    }


}
