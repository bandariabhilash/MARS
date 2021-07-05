using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class ESMDSMRSMModel
    {
        public int Id { get; set; }
        public string RegionNumber { get; set; }
        public string BranchNumber { get; set; }
        public string RegionName { get; set; }
        public string BranchName { get; set; }

        public string ESMId { get; set; }
        public string ESMName { get; set; }
        public string ESMEmail { get; set; }
        public string ESMPhone { get; set; }

        public string CCMId { get; set; }
        public string CCMName { get; set; }
        public string CCMEmail { get; set; }
        public string CCMPhone { get; set; }

        public string RSMId { get; set; }
        public string RSMName { get; set; }
        public string RSMEmail { get; set; }
        public string RSMPhone { get; set; }


        public List<ESMBranch> ESMBranchList { get; set; }
        public List<ESMRegion> ESMRegionList { get; set; }

        public List<ESM> ESMList { get; set; }
        public List<RSM> RSMList { get; set; }
        public List<CCM> CCMList { get; set; }

        public int Operation { get; set; }

        public static List<ESMDSMRSMModel> GetESMDSMRSMDetails(ESMDSMRSMModel esmModel, FarmerBrothersEntities FarmerBrothersEntities)
        {
            List<ESMDSMRSMModel> results = new List<ESMDSMRSMModel>();

            List<ESMDSMRSM> resultsList = FarmerBrothersEntities.ESMDSMRSMs.ToList();


            if (!string.IsNullOrEmpty(esmModel.BranchNumber))
            {
                resultsList = resultsList.Where(re => re.BranchNO == esmModel.BranchNumber).ToList();
            }
            if (!string.IsNullOrEmpty(esmModel.RegionNumber))
            {
                resultsList = resultsList.Where(re => re.Region == esmModel.RegionNumber).ToList();
            }

            foreach (ESMDSMRSM edrData in resultsList)
            {
                ESMDSMRSMModel edrModelData = new ESMDSMRSMModel();
                edrModelData.Id = edrData.ID;
                edrModelData.RegionNumber = string.IsNullOrEmpty(edrData.Region) ? "" : edrData.Region;
                edrModelData.BranchNumber = string.IsNullOrEmpty(edrData.BranchNO) ? "" : edrData.BranchNO;
                edrModelData.RegionName = string.IsNullOrEmpty(edrData.RegionName) ? "" : edrData.Region + " - " + edrData.RegionName;
                edrModelData.BranchName = string.IsNullOrEmpty(edrData.Branch) ? "" : edrData.BranchNO + " - " + edrData.Branch;
                edrModelData.ESMBranchList = ESMBranch.GetESMBranches(FarmerBrothersEntities);
                edrModelData.ESMRegionList = ESMRegion.GetESMRegions(FarmerBrothersEntities);
                edrModelData.ESMList = ESM.GetServiceManagerDetails(FarmerBrothersEntities);
                edrModelData.RSMList = RSM.GetServiceManagerDetails(FarmerBrothersEntities);
                edrModelData.CCMList = CCM.GetServiceManagerDetails(FarmerBrothersEntities);

                edrModelData.ESMId = edrData.EDSMID == 0 ? "" : edrData.EDSMID.ToString();
                edrModelData.ESMName = string.IsNullOrEmpty(edrData.ESMName) ? "" : edrData.ESMName;
                edrModelData.ESMEmail = string.IsNullOrEmpty(edrData.ESMEmail) ? "" : edrData.ESMEmail;
                edrModelData.ESMPhone = string.IsNullOrEmpty(edrData.ESMPhone) ? "" : edrData.ESMPhone;
                edrModelData.RSMId = edrData.RSMID == 0 ? "" : edrData.RSMID.ToString();
                edrModelData.RSMName = string.IsNullOrEmpty(edrData.RSM) ? "" : edrData.RSM;
                edrModelData.RSMEmail = string.IsNullOrEmpty(edrData.RSMEmail) ? "" : edrData.RSMEmail;
                edrModelData.RSMPhone = string.IsNullOrEmpty(edrData.RSMPhone) ? "" : edrData.RSMPhone;
                edrModelData.CCMId = edrData.CCMID == 0 ? "" : edrData.CCMID.ToString();
                edrModelData.CCMName = string.IsNullOrEmpty(edrData.CCMName) ? "" : edrData.CCMName;
                edrModelData.CCMEmail = string.IsNullOrEmpty(edrData.CCMEmail) ? "" : edrData.CCMEmail;
                edrModelData.CCMPhone = string.IsNullOrEmpty(edrData.CCMPhone) ? "" : edrData.CCMPhone;

                results.Add(edrModelData);
            }

            return results;
        }
    }

    public class ESMBranch
    {
        public string BranchName { get; set; }
        public string BranchNumber { get; set; }

        public static List<ESMBranch> GetESMBranches(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            List<ESMBranch> ESMBranchList = new List<ESMBranch>();
            try
            {
                /*IEnumerable<List<ESMDSMRSM>> branches = (from m in FarmerBrothersEntitites.ESMDSMRSMs group m by m.Branch into branch select branch.ToList()).ToList();

                foreach (List<ESMDSMRSM> branch in branches)
                {
                    if (branch.Count() > 0)
                    {
                        ESMBranchList.Add(new ESMBranch(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(branch[0].Branch.ToLower()), branch[0].BranchNO.ToString()));
                    }
                }*/

                List<ESMDSMRSM> branches = (from m in FarmerBrothersEntitites.ESMDSMRSMs select m).ToList();

                foreach (ESMDSMRSM branch in branches)
                {

                    ESMBranchList.Add(new ESMBranch() { BranchNumber = branch.BranchNO, BranchName = branch.BranchNO + " - " + branch.Branch });//(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(branch.Branch.ToLower()), branch.BranchNO.ToString());

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Technician Branches ", ex);
            }

            ESMBranch blankBranch = new ESMBranch() { BranchNumber = "", BranchName = "Please Select" };
            ESMBranchList.Insert(0, blankBranch);

            return ESMBranchList;
        }
    }

    public class ESMRegion
    {
        public string RegionName { get; set; }
        public string RegionNumber { get; set; }


        public static List<ESMRegion> GetESMRegions(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            List<ESMRegion> ESMRegionList = new List<ESMRegion>();
            try
            {
                IEnumerable<List<ESMDSMRSM>> regions = (from m in FarmerBrothersEntitites.ESMDSMRSMs group m by m.Region into rgn select rgn.ToList()).ToList();

                foreach (List<ESMDSMRSM> region in regions)
                {
                    if (region.Count() > 0)
                    {
                        ESMRegion er = new ESMRegion();
                        er.RegionName = region[0].Region + " - " + region[0].RegionName;
                        er.RegionNumber = region[0].Region;

                        ESMRegionList.Add(er);
                        //ESMRegionList.Add(new ESMRegion(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(region[0].RegionName.ToLower()), region[0].Region.ToString()));
                    }
                }

                /*List<ESMDSMRSM> regions = (from m in FarmerBrothersEntitites.ESMDSMRSMs select m).ToList();

                foreach (ESMDSMRSM region in regions)
                {

                    ESMRegionList.Add(new ESMRegion() { RegionNumber = region.Region, RegionName = region.Region + " - "+ region.RegionName });//(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(branch.Branch.ToLower()), branch.BranchNO.ToString());

                }*/
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the Technician Branches ", ex);
            }

            ESMRegion blankBranch = new ESMRegion() { RegionNumber = "", RegionName = "Please Select" };
            ESMRegionList.Insert(0, blankBranch);

            return ESMRegionList;
        }
    }

    public class ESM
    {
        public string ESMId { get; set; }
        public string ESMName { get; set; }
        public string ESMEmail { get; set; }
        public string ESMPhone { get; set; }

        public static List<ESM> GetServiceManagerDetails(FarmerBrothersEntities FarmerBrothersEntities)
        {
            List<ESM> Results = new List<ESM>();
            try
            {
                List<ESMDSMRSM> SMList = (from m in FarmerBrothersEntities.ESMDSMRSMs select m).ToList();

                foreach (ESMDSMRSM sm in SMList)
                {
                    ESM tmpData = Results.Where(re => re.ESMId == sm.EDSMID.ToString()).FirstOrDefault();

                    if (tmpData == null)
                    {
                        Results.Add(new ESM() { ESMId = sm.EDSMID.ToString(), ESMName = sm.ESMName, ESMEmail = sm.ESMEmail, ESMPhone = sm.ESMPhone });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the ESMRSMCCM Details ", ex);
            }

            return Results;
        }

        public static ESM GetESMDetails(string ESMName, FarmerBrothersEntities FarmerBrothersEntities)
        {
            ESM Results = new ESM();
            try
            {
                ESMDSMRSM esmItem = FarmerBrothersEntities.ESMDSMRSMs.Where(es => es.ESMName == ESMName).FirstOrDefault();
                if(esmItem != null)
                {
                    Results.ESMId = esmItem.EDSMID.ToString();
                    Results.ESMName = esmItem.ESMName;
                    Results.ESMEmail = esmItem.ESMEmail;
                    Results.ESMPhone = esmItem.ESMPhone;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the ESMRSMCCM Details ", ex);
            }

            return Results;
        }
    }

    public class RSM
    {
        public string RSMId { get; set; }
        public string RSMName { get; set; }
        public string RSMEmail { get; set; }
        public string RSMPhone { get; set; }

        public static List<RSM> GetServiceManagerDetails(FarmerBrothersEntities FarmerBrothersEntities)
        {
            List<RSM> Results = new List<RSM>();
            try
            {
                List<ESMDSMRSM> SMList = (from m in FarmerBrothersEntities.ESMDSMRSMs select m).ToList();

                foreach (ESMDSMRSM sm in SMList)
                {
                    RSM tmpData = Results.Where(re => re.RSMId == sm.RSMID.ToString()).FirstOrDefault();

                    if (tmpData == null)
                    {
                        Results.Add(new RSM() { RSMId = sm.RSMID.ToString(), RSMName = sm.RSM, RSMEmail = sm.RSMEmail, RSMPhone = sm.RSMPhone });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the ESMRSMCCM Details ", ex);
            }

            return Results;
        }
    }

    public class CCM
    {
        public string CCMId { get; set; }
        public string CCMName { get; set; }
        public string CCMEmail { get; set; }
        public string CCMPhone { get; set; }

        public static List<CCM> GetServiceManagerDetails(FarmerBrothersEntities FarmerBrothersEntities)
        {
            List<CCM> Results = new List<CCM>();
            try
            {
                List<ESMDSMRSM> SMList = (from m in FarmerBrothersEntities.ESMDSMRSMs select m).ToList();

                foreach (ESMDSMRSM sm in SMList)
                {
                    CCM tmpData = Results.Where(re => re.CCMId == sm.CCMID.ToString()).FirstOrDefault();

                    if (tmpData == null)
                    {
                        Results.Add(new CCM() { CCMId = sm.CCMID.ToString(), CCMName = sm.CCMName, CCMEmail = sm.CCMEmail, CCMPhone = sm.CCMPhone });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get the ESMRSMCCM Details ", ex);
            }

            return Results;
        }
    }
}