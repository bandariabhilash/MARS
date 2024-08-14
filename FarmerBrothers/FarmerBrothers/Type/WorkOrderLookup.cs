using FarmerBrothers.CacheManager;
using FarmerBrothers.Data;
using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace FarmerBrothers
{
    public class WorkOrderLookup
    {
        public static IList<VendorDataModel> PartOrderManufacturer(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.PART_ORDER_MNF))
                return MAICacheManager.getType(MAICacheManager.TypeNames.PART_ORDER_MNF) as List<VendorDataModel>;

            List<VendorDataModel> PartOrderManufacturerList = new List<VendorDataModel>();

            try
            {
                IQueryable<string> vendors = FarmerBrothersEntitites.FBSKUs.Where(s => s.SKUActive == true).Select(s => s.VendorCode).Distinct();
                foreach (string vendor in vendors)
                {
                    PartOrderManufacturerList.Add(new VendorDataModel(vendor));
                }
                PartOrderManufacturerList.OrderBy(v => v.VendorDescription).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Part Order Manufacturer ", ex);
            }


            MAICacheManager.setType(MAICacheManager.TypeNames.PART_ORDER_MNF, PartOrderManufacturerList);


            return PartOrderManufacturerList;
        }

        public static IList<VendorModelModel> PartOrderSKU(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.PART_ORDER_SKU))
                return MAICacheManager.getType(MAICacheManager.TypeNames.PART_ORDER_SKU) as List<VendorModelModel>;

            List<VendorModelModel> PartOrderSKUList = new List<VendorModelModel>();

            try
            {
                IQueryable<string> models = FarmerBrothersEntitites.FBSKUs.Where(s => s.SKUActive == true).Select(s => s.SKU).Distinct();

                foreach (string model in models)
                {
                    PartOrderSKUList.Add(new VendorModelModel(model));
                }
                PartOrderSKUList.OrderBy(v => v.Model).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Part Order SKU ", ex);
            }


            MAICacheManager.setType(MAICacheManager.TypeNames.PART_ORDER_SKU, PartOrderSKUList);


            return PartOrderSKUList;
        }
        public static IList<VendorDataModel> CloserManufacturer(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.CLOSER_MNF))
                return MAICacheManager.getType(MAICacheManager.TypeNames.CLOSER_MNF) as List<VendorDataModel>;

            List<VendorDataModel> CloserManufacturerList = new List<VendorDataModel>();

            try
            {
                IQueryable<string> Closervendors = FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true).Select(s => s.Supplier).Distinct();

                foreach (string vendor in Closervendors)
                {
                    if (!string.IsNullOrEmpty(vendor))
                    {
                        CloserManufacturerList.Add(new VendorDataModel(vendor));
                    }
                }

                CloserManufacturerList.OrderBy(v => v.VendorDescription).ToList();

            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Closer Manufacturer ", ex);
            }


            MAICacheManager.setType(MAICacheManager.TypeNames.CLOSER_MNF, CloserManufacturerList);


            return CloserManufacturerList;
        }

        public static IList<VendorDataModel> CloserSKU(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.CLOSER_SKU))
                return MAICacheManager.getType(MAICacheManager.TypeNames.CLOSER_SKU) as List<VendorDataModel>;

            List<VendorDataModel> CloserSkusList = new List<VendorDataModel>();

            try
            {
                IQueryable<string> CloserPartOrSKU = FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true).Select(s => s.ItemNo).Distinct();//.Take(5000);

                foreach (string vendor in CloserPartOrSKU)
                {
                    CloserSkusList.Add(new VendorDataModel(vendor));
                }
                CloserSkusList.OrderBy(v => v.VendorDescription).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the closer SKU ", ex);
            }
            MAICacheManager.setType(MAICacheManager.TypeNames.CLOSER_SKU, CloserSkusList);


            return CloserSkusList;
        }

        //public static string[] CloserSKU(FarmerBrothersEntities FarmerBrothersEntitites)
        //{

        //    if (MAICacheManager.hasType(MAICacheManager.TypeNames.CLOSER_SKU))
        //        return MAICacheManager.getType(MAICacheManager.TypeNames.CLOSER_SKU) as string[];

        //    string[] CloserSkusArray = null;
        //    List<VendorDataModel> CloserSkusList = new List<VendorDataModel>();
        //    try
        //    {
        //        IQueryable<string> CloserPartOrSKU = FarmerBrothersEntitites.FBClosureParts.Where(s => s.SkuActive == true).Select(s => s.ItemNo).Distinct();//.Take(100);
        //        foreach (string vendor in CloserPartOrSKU)
        //        {
        //            CloserSkusList.Add(new VendorDataModel(vendor));
        //        }
        //        CloserSkusArray = new string[CloserSkusList.Count()];
        //        for (int i = 0; i < CloserSkusArray.Count(); i++)
        //        {
        //            CloserSkusArray[i] = CloserSkusList[i].VendorDescription;
        //        }
        //        MAICacheManager.setType(MAICacheManager.TypeNames.CLOSER_SKU, CloserSkusArray);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Unable to get the closer SKU ", ex);
        //    }
        //    return CloserSkusArray;
        //}

        public static IList<PrivilegeModel> GetPrivileges(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.PRIVILEGE))
                return MAICacheManager.getType(MAICacheManager.TypeNames.PRIVILEGE) as List<PrivilegeModel>;

            List<PrivilegeModel> PrivilegeList = new List<PrivilegeModel>();

            try
            {
                List<Privilege> privileges = FarmerBrothersEntitites.Privileges.ToList();

                foreach (Privilege pri in privileges)
                {
                    PrivilegeList.Add(new PrivilegeModel(pri));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Privilege ", ex);
            }
            MAICacheManager.setType(MAICacheManager.TypeNames.PRIVILEGE, PrivilegeList);


            return PrivilegeList;
        }
        public static IList<ApplicationModel> GetApplication(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.APPUSER))
                return MAICacheManager.getType(MAICacheManager.TypeNames.APPUSER) as List<ApplicationModel>;

            List<ApplicationModel> AppUserList = new List<ApplicationModel>();

            try
            {
                List<Application> appUser = FarmerBrothersEntitites.Applications.ToList();

                foreach (Application app in appUser)
                {
                    AppUserList.Add(new ApplicationModel(app));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Application ", ex);
            }
            MAICacheManager.setType(MAICacheManager.TypeNames.APPUSER, AppUserList);


            return AppUserList;
        }

        public static List<BranchRegion> GetTechBranches(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            //if (MAICacheManager.hasType(MAICacheManager.TypeNames.TECHBRANCH))
            //    return MAICacheManager.getType(MAICacheManager.TypeNames.TECHBRANCH) as List<BranchRegion>;

            List<BranchRegion> TechBranchList = new List<BranchRegion>();

            try
            {
                /*IEnumerable<List<ESMDSMRSM>> branches = (from m in FarmerBrothersEntitites.ESMDSMRSMs group m by m.Branch into branch select branch.ToList()).ToList();

                foreach (List<ESMDSMRSM> branch in branches)
                {
                    if (branch.Count() > 0)
                    {
                        TechBranchList.Add(new BranchRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(branch[0].Branch.ToLower()), branch[0].BranchNO.ToString()));
                    }
                }*/

                IEnumerable<List<BranchESM>> branches = (from m in FarmerBrothersEntitites.BranchESMs group m by m.BranchName into branch select branch.ToList()).ToList();

                foreach (List<BranchESM> branch in branches)
                {
                    if (branch.Count() > 0)
                    {
                        TechBranchList.Add(new BranchRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(branch[0].BranchName.ToLower()) + " - " + branch[0].BranchNo.ToString(), branch[0].BranchNo.ToString()));
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Technician Branches ", ex);
            }
            //MAICacheManager.setType(MAICacheManager.TypeNames.TECHBRANCH, TechBranchList);

            BranchRegion blankBranch = new BranchRegion() { Number = "n/a", Name = "Please Select" };
            TechBranchList.Insert(0, blankBranch);

            return TechBranchList;
        }

        public static List<BranchRegion> GetTechBranchesForTechUpdateGrid(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            List<BranchRegion> TechBranchList = new List<BranchRegion>();

            try
            {
                IEnumerable<List<BranchESM>> branches = (from m in FarmerBrothersEntitites.BranchESMs group m by m.BranchName into branch select branch.ToList()).ToList();

                foreach (List<BranchESM> branch in branches)
                {
                    if (branch.Count() > 0)
                    {
                        TechBranchList.Add(new BranchRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(branch[0].BranchName.ToLower()), branch[0].BranchNo.ToString()));
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Technician Branches ", ex);
            }

            BranchRegion blankBranch = new BranchRegion() { Number = "n/a", Name = "Please Select" };
            TechBranchList.Insert(0, blankBranch);

            return TechBranchList;
        }

        public static List<BranchRegion> GetTechBranchesNumbers(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            List<BranchRegion> TechBranchList = new List<BranchRegion>();

            try
            {
                IEnumerable<List<BranchESM>> branches = (from m in FarmerBrothersEntitites.BranchESMs group m by m.BranchNo into branch select branch.ToList()).ToList();

                foreach (List<BranchESM> branch in branches)
                {
                    if (branch.Count() > 0)
                    {
                        TechBranchList.Add(new BranchRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(branch[0].BranchNo.ToLower()), branch[0].BranchNo.ToString()));
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Technician Branches ", ex);
            }

            BranchRegion blankBranch = new BranchRegion() { Number = "n/a", Name = "Please Select" };
            TechBranchList.Insert(0, blankBranch);

            return TechBranchList;
        }


        public static List<BranchRegion> GetPrimaryTechnicians(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.PRIMARYTECH))
                return MAICacheManager.getType(MAICacheManager.TypeNames.PRIMARYTECH) as List<BranchRegion>;

            List<BranchRegion> PrimaryTechList = new List<BranchRegion>();

            try
            {
                /*List<FbPrimaryTechnician> primaryTechnames =FarmerBrothersEntitites.FbPrimaryTechnicians.ToList();
                foreach (FbPrimaryTechnician name in primaryTechnames)
                {
                    PrimaryTechList.Add(new BranchRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.PrimaryTechName.ToLower()), name.PrimaryTechId.ToString()));
                }*/

                var primaryTechnames1 = (from fbp in FarmerBrothersEntitites.FbPrimaryTechnicians
                                         select new {
                                             PrimaryTechName = fbp.PrimaryTechName,
                                             PrimaryTechId = fbp.PrimaryTechId
                                         }).ToList();
                var techList = (from th in FarmerBrothersEntitites.TECH_HIERARCHY where th.SearchType.ToLower() == "sp"
                                select new
                                {
                                    PrimaryTechName = th.CompanyName,
                                    PrimaryTechId = th.DealerId
                                }).ToList();

                var totalPrimaryTechList = primaryTechnames1.Union(techList);

                foreach(var name in totalPrimaryTechList)
                {
                    PrimaryTechList.Add(new BranchRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.PrimaryTechName.ToLower()), name.PrimaryTechId.ToString()));
                }
                PrimaryTechList = PrimaryTechList.OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Primary Technician ", ex);
            }
            MAICacheManager.setType(MAICacheManager.TypeNames.PRIMARYTECH, PrimaryTechList);

            BranchRegion blank = new BranchRegion() { Number = "n/a", Name = "" };
            PrimaryTechList.Insert(0, blank);

            return PrimaryTechList;
        }

        public static List<BranchRegion> GetFSM(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.TECHFSM))
                return MAICacheManager.getType(MAICacheManager.TypeNames.TECHFSM) as List<BranchRegion>;

            List<BranchRegion> TechEsmList = new List<BranchRegion>();

            try
            {
                //IEnumerable<List<ESMDSMRSM>> esmnames = (from m in FarmerBrothersEntitites.ESMDSMRSMs group m by m.ESMName into esName select esName.ToList()).ToList();
                //foreach (List<ESMDSMRSM> esmname in esmnames)

                /*IEnumerable<List<ESMCCMRSMEscalation>> esmnames = (from m in FarmerBrothersEntitites.ESMCCMRSMEscalations group m by m.ESMName into esName select esName.ToList()).ToList();                
                foreach (List<ESMCCMRSMEscalation> esmname in esmnames)
                {
                    if (esmname.Count() > 0)
                    {
                        TechEsmList.Add(new BranchRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(esmname[0].ESMName.ToLower()), esmname[0].EDSMID.ToString()));
                    }
                }*/

                IEnumerable<List<BranchESM>> esmnames = (from m in FarmerBrothersEntitites.BranchESMs group m by m.ESMName into esName select esName.ToList()).ToList();
                foreach (List<BranchESM> esmname in esmnames)
                {
                    if (esmname.Count() > 0)
                    {
                        TechEsmList.Add(new BranchRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(esmname[0].ESMName.ToLower()), esmname[0].ESMName.ToString()));
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Technician ESMS ", ex);
            }
            MAICacheManager.setType(MAICacheManager.TypeNames.TECHFSM, TechEsmList);

            BranchRegion blankEsm = new BranchRegion() { Number = "n/a", Name = "Please Select" };
            TechEsmList.Insert(0, blankEsm);

            return TechEsmList;
        }
              
       
        public static List<BranchRegion> GetTechRegions(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.TECHREGION))
                return MAICacheManager.getType(MAICacheManager.TypeNames.TECHREGION) as List<BranchRegion>;

            List<BranchRegion> TechRegionList = new List<BranchRegion>();

            try
            {
                IEnumerable<List<TECH_HIERARCHY>> regions = (from m in FarmerBrothersEntitites.TECH_HIERARCHY
                                                             where m.RegionName != "" && m.RegionNumber != "" && m.SearchType == "SP" && m.RegionName != null
                                                             group m by m.RegionName into rName
                                                             select rName.ToList()).ToList();

                foreach (List<TECH_HIERARCHY> region in regions)
                {
                    if (region.Count() > 0)
                    {
                        TechRegionList.Add(new BranchRegion(region[0].RegionName, region[0].RegionNumber));
                    }
                }

                /*IEnumerable<List<ESMDSMRSM>> regions = (from m in FarmerBrothersEntitites.ESMDSMRSMs group m by m.RegionName into region select region.ToList()).ToList();

                foreach (List<ESMDSMRSM> regn in regions)
                {
                    if (regn.Count() > 0)
                    {
                        TechRegionList.Add(new BranchRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(regn[0].RegionName.ToLower()), regn[0].Region.ToString()));
                    }
                }*/

            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Technician Regions ", ex);
            }
            MAICacheManager.setType(MAICacheManager.TypeNames.TECHREGION, TechRegionList);

            BranchRegion blankRegion = new BranchRegion() { Number = "n/a", Name = "Please Select" };
            TechRegionList.Insert(0, blankRegion);

            return TechRegionList;
        }

        public static List<WorkorderType> GetWorkOrderTypes(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (MAICacheManager.hasType(MAICacheManager.TypeNames.WOTYPE))
                return MAICacheManager.getType(MAICacheManager.TypeNames.WOTYPE) as List<WorkorderType>;

            List<WorkorderType> WorkOrderTypeList = new List<WorkorderType>();

            try
            {
                WorkOrderTypeList = FarmerBrothersEntitites.WorkorderTypes.Where(t=>t.Active==1).ToList();                
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get Workorder Types ", ex);
            }
            MAICacheManager.setType(MAICacheManager.TypeNames.WOTYPE, WorkOrderTypeList);

            return WorkOrderTypeList;
        }

        public static int GetPrivileageIdByName(string name, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            return GetPrivileges(FarmerBrothersEntitites).Where(p => p.PrivilegeType == name).FirstOrDefault().PrivilegeId;
        }
        public static string GetPrivileageNameById(int id, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            return GetPrivileges(FarmerBrothersEntitites).Where(p => p.PrivilegeId == id).FirstOrDefault().PrivilegeType;
        }

        public static int GetApplicationIdByName(string name, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            return GetApplication(FarmerBrothersEntitites).Where(p => p.ApplicationName == name).FirstOrDefault().ApplicationId;
        }
        public static string GetApplicationNameById(int id, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            return GetApplication(FarmerBrothersEntitites).Where(p => p.ApplicationId == id).FirstOrDefault().ApplicationName;
        }
        public static int GetApplicationOrderById(int id, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            return Convert.ToInt16(GetApplication(FarmerBrothersEntitites).Where(p => p.ApplicationId == id).FirstOrDefault().OrderId);
        }
        public static string GetWorkOrderTypesById(int id, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            return GetWorkOrderTypes(FarmerBrothersEntitites).Where(p => p.CallTypeID == id).FirstOrDefault().Description;
        }

        
    }
}