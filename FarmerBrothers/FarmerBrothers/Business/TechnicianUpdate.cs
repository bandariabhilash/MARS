using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FarmerBrothers.Data;
using LinqKit;
using FarmerBrothers.Utilities;

namespace FarmerBrothers.Business
{
    public class TechnicianUpdate
    {
        public static bool AddTechnician(TechnicianUpdateModel techModel, out string message)
        {
            bool result = true;
            message = string.Empty;

            try
            {
                using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
                {
                    Zip zip = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == techModel.Zip).FirstOrDefault();

                    if (zip != null)
                    {
                        TECH_HIERARCHY tech = new TECH_HIERARCHY();

                        if (techModel.TechId != 0)
                        {
                            TECH_HIERARCHY validtechDetails = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == techModel.TechId).FirstOrDefault();
                            if (validtechDetails != null)
                            {
                                message = "| Please Enter valid Technician ID, Entered Technician ID already used by some other technician";
                                return false;
                            }
                            else
                            {
                                tech.DealerId = techModel.TechId;
                                tech.PrimaryTechId = techModel.TechId;
                            }
                        }

                        List<BranchRegion> branches = WorkOrderLookup.GetTechBranches(FarmerBrothersEntitites);
                        List<BranchRegion> regions = WorkOrderLookup.GetTechRegions(FarmerBrothersEntitites);                        
                        DateTime currentTime = Utility.GetCurrentTime(techModel.Zip, FarmerBrothersEntitites);
                        if (tech.DealerId == 0)
                        {
                            //int tmpTechId = FarmerBrothersEntitites.TECH_HIERARCHY.Max(t => t.DealerId);
                            //tech.DealerId = ++tmpTechId;

                            IndexCounter counter = Utility.GetIndexCounter("TechId", 1);
                            counter.IndexValue++;
                            //FarmerBrothersEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;
                            tech.DealerId = counter.IndexValue.Value;
                            tech.PrimaryTechId = counter.IndexValue.Value;
                        }

                        tech.CompanyName = techModel.TechName;
                        tech.City = zip.City;
                        tech.State = zip.State;
                        tech.PostalCode = techModel.Zip;
                        tech.Phone = techModel.PhoneNumber;
                        tech.SearchType = techModel.SearchType;
                        tech.AreaCode = techModel.AreaCode;
                        tech.Longitude = zip.Longitude;
                        tech.Latitude = zip.Latitude;
                        tech.EmailCC = techModel.EmailCC;
                        tech.RimEmail = techModel.RimEmail;
                        tech.BranchAssociations = techModel.BranchAssociation;
                        tech.FamilyAff = techModel.FamilyAff;
                        tech.AlternativePhone = techModel.AlternativePhone;
                        tech.ModifiedDate = currentTime;
                        tech.ModifiedUserID = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234;
                        tech.BranchNumber = techModel.BranchName;
                        tech.BranchName = (from b in branches where b.Number == techModel.BranchName select b.Name).FirstOrDefault();
                        tech.RegionNumber = techModel.RegionName;
                        tech.RegionName = (from r in regions where r.Number == techModel.RegionName select r.Name).FirstOrDefault();
                        tech.AutoDispatch = techModel.AutoDispatch;
                        tech.FieldServiceManager = techModel.FieldServiceManager;

                        FarmerBrothersEntitites.TECH_HIERARCHY.Add(tech);
                        FarmerBrothersEntitites.SaveChanges();


                    }
                    else
                    {
                        result = false;
                        message = "| Please Enter Valid Zip Code!";
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public static List<TechnicianUpdateModel> GetTechnicianDetails(TechnicianUpdateModel techModel)
        {
            List<TechnicianUpdateModel> SearchResults = new List<TechnicianUpdateModel>();


            var predicate = PredicateBuilder.True<TECH_HIERARCHY>();

            if (techModel.SearchTechId > 0 && !string.IsNullOrWhiteSpace(techModel.SearchTechId.ToString()))
            {
                predicate = predicate.And(e => e.DealerId.ToString().Contains(techModel.SearchTechId.ToString()));
            }

            if (!string.IsNullOrEmpty(techModel.SearchTechName) && !string.IsNullOrWhiteSpace(techModel.SearchTechName))
            {
                predicate = predicate.And(e => e.CompanyName.ToString().Contains(techModel.SearchTechName.ToString()));
            }

            if (!string.IsNullOrEmpty(techModel.FamilyAff) && techModel.FamilyAff != "n/a")
            {
                predicate = predicate.And(e => e.FamilyAff.ToString().Contains(techModel.FamilyAff.ToString()));
            }

            if (techModel.InactiveTechnicians == false)
            {
                predicate = predicate.And(e => e.SearchType == "SP");
            }
            if (techModel.InactiveTechnicians == true)
            {
                predicate = predicate.And(e => e.SearchType != "SP");
            }

            using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
            {
                IQueryable<TECH_HIERARCHY> techies = entity.Set<TECH_HIERARCHY>().AsExpandable().Where(predicate);

                foreach (TECH_HIERARCHY tech in techies)
                {
                    TechnicianUpdateModel tModel = new TechnicianUpdateModel(tech, entity);
                    techModel.BranchNumber = tModel.BranchNumber;
                    techModel.BranchName = tModel.BranchName +" - "+ tModel.BranchNumber;

                    SearchResults.Add(tModel);
                }
            }

            return SearchResults;
        }

        public static ErrorCode UpdateTechnicianDetail(TechnicianUpdateModel techModel, out string message)
        {
            ErrorCode code = ErrorCode.SUCCESS;
            message = string.Empty;
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                try
                {
                    Zip zipcode = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == techModel.Zip).FirstOrDefault();
                    if (zipcode != null)
                    {
                        TECH_HIERARCHY tech = FarmerBrothersEntitites.TECH_HIERARCHY.Where(u => u.DealerId == techModel.TechId).FirstOrDefault();
                        if (tech != null)
                        {
                            List<BranchRegion> branches = WorkOrderLookup.GetTechBranches(FarmerBrothersEntitites);

                            tech.SearchType = techModel.IsActive ? "SP" : "SPI";
                            tech.AreaCode = techModel.AreaCode;
                            tech.Phone = techModel.PhoneNumber;
                            tech.AlternativePhone = techModel.AlternativePhone;
                            tech.EmailCC = techModel.EmailCC;
                            tech.RimEmail = techModel.RimEmail;
                            tech.PostalCode = techModel.Zip;
                            tech.City = zipcode.City;
                            tech.BranchNumber = (from b in branches where b.Name == techModel.BranchName select b.Number).FirstOrDefault();
                            tech.BranchName = techModel.BranchName;
                            tech.Longitude = zipcode.Longitude;
                            tech.Latitude = zipcode.Latitude;
                            tech.State = zipcode.State;
                            string tmpParentTechnicianId = WorkOrderLookup.GetPrimaryTechnicians(FarmerBrothersEntitites).Where(t => t.Name == techModel.ParentTechnicianName).Select(tt => tt.Number).FirstOrDefault();
                            tech.PrimaryTechId = Convert.ToInt32(tmpParentTechnicianId);//techModel.ParentTechnicianId == 0 ? techModel.TechId : techModel.ParentTechnicianId;
                            tech.CompanyName = techModel.TechName;
                            tech.FieldServiceManager = techModel.FieldServiceManager;
                            tech.ModifiedDate = DateTime.Now;

                            int userId = (int)System.Web.HttpContext.Current.Session["UserId"];
                            tech.ModifiedUserID = userId;

                           FarmerBrothersEntitites.SaveChanges();
                            message = "|Technician Details saved successfully!";
                        }
                    }
                    else
                    {
                        message = "|Please enter valid zip code!";
                    }
                }
                catch (Exception e)
                {
                    code = ErrorCode.ERROR;
                    message = "|There is a problem in technician details update!";
                }
            }
            return code;
        }
    }
}