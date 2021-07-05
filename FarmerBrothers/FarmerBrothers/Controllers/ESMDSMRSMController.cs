using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class ESMDSMRSMController : BaseController
    {
        // GET: ESMDSMRSM
        public ActionResult ESMDSMRSM(int? isBack)
        {
            ESMDSMRSMModel edr = new ESMDSMRSMModel();
            edr.ESMBranchList = ESMBranch.GetESMBranches(FarmerBrothersEntitites);
            edr.ESMRegionList = ESMRegion.GetESMRegions(FarmerBrothersEntitites);
            edr.ESMList = ESM.GetServiceManagerDetails(FarmerBrothersEntitites);
            edr.RSMList = RSM.GetServiceManagerDetails(FarmerBrothersEntitites);
            edr.CCMList = CCM.GetServiceManagerDetails(FarmerBrothersEntitites);

            return View(edr);
        }

        [HttpPost]
        public ActionResult ESMDSMRSMSearch(ESMDSMRSMModel esmModel)
        {
            TempData["ESMDSMRSMSearchCriteria"] = esmModel;
            List<ESMDSMRSMModel> SearchResults = ESMDSMRSMModel.GetESMDSMRSMDetails(esmModel, FarmerBrothersEntitites);

            ViewBag.datasource = SearchResults;
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = 1, data = SearchResults };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult ClearSearchResults()
        {
            return Json(new List<ESMDSMRSMModel>(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ESMDSMRSMDetailsUpdate(ESMDSMRSMModel techModel)
        {
            string message = string.Empty;
            ErrorCode code = UpdateESMDSMRSMDetail(techModel, out message);
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { serverError = code, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult GetEsmDetails(string Name)
        {
            string message = string.Empty;

            ESM ESMDetails = ESM.GetESMDetails(Name, FarmerBrothersEntitites);


            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = true, serverError = 1, data = ESMDetails };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        private ErrorCode UpdateESMDSMRSMDetail(ESMDSMRSMModel techModel, out string message)
        {
            ErrorCode code = ErrorCode.SUCCESS;
            message = string.Empty;
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                try
                {
                    ESMDSMRSM esmRecord = FarmerBrothersEntitites.ESMDSMRSMs.Where(z => z.ID == techModel.Id).FirstOrDefault();
                    if (esmRecord != null)
                    {
                        string BranchNm = "";
                        string BranchNo = "";
                        string RegionNm = "";
                        string RegionNo = "";

                        if (!string.IsNullOrEmpty(techModel.BranchName))
                        {
                            string[] BranchValues = techModel.BranchName.Split('-');

                            if (BranchValues != null && BranchValues.Count() > 0)
                            {
                                BranchNo = BranchValues[0].Trim();
                                BranchNm = BranchValues[1].Trim();
                            }

                        }
                        if (!string.IsNullOrEmpty(techModel.RegionName))
                        {
                            string[] RegionValues = techModel.RegionName.Split('-');

                            if (RegionValues != null && RegionValues.Count() > 0)
                            {
                                RegionNo = RegionValues[0].Trim();
                                RegionNm = RegionValues[1].Trim();
                            }
                        }

                        esmRecord.RegionName = RegionNm;
                        esmRecord.Branch = BranchNm;
                        esmRecord.BranchNO = BranchNo;
                        esmRecord.Region = RegionNo;

                        esmRecord.CCMID = string.IsNullOrEmpty(techModel.CCMId) ? 0 : Convert.ToInt32(techModel.CCMId);
                        esmRecord.CCMEmail = string.IsNullOrEmpty(techModel.CCMEmail) ? "" : techModel.CCMEmail;
                        esmRecord.CCMName = string.IsNullOrEmpty(techModel.CCMName) ? "" : techModel.CCMName;
                        esmRecord.CCMPhone = string.IsNullOrEmpty(techModel.CCMPhone) ? "" : techModel.CCMPhone;

                        esmRecord.EDSMID = string.IsNullOrEmpty(techModel.ESMId) ? 0 : Convert.ToInt32(techModel.ESMId);
                        esmRecord.ESMEmail = string.IsNullOrEmpty(techModel.ESMEmail) ? "" : techModel.ESMEmail;
                        esmRecord.ESMName = string.IsNullOrEmpty(techModel.ESMName) ? "" : techModel.ESMName;
                        esmRecord.ESMPhone = string.IsNullOrEmpty(techModel.ESMPhone) ? "" : techModel.ESMPhone;

                        esmRecord.RSMID = string.IsNullOrEmpty(techModel.RSMId) ? 0 : Convert.ToInt32(techModel.RSMId);
                        esmRecord.RSMEmail = string.IsNullOrEmpty(techModel.RSMEmail) ? "" : techModel.RSMEmail;
                        esmRecord.RSM = string.IsNullOrEmpty(techModel.RSMName) ? "" : techModel.RSMName;
                        esmRecord.RSMPhone = string.IsNullOrEmpty(techModel.RSMPhone) ? "" : techModel.RSMPhone;

                        FarmerBrothersEntitites.SaveChanges();

                        techModel.ESMBranchList = ESMBranch.GetESMBranches(FarmerBrothersEntitites);
                        techModel.ESMRegionList = ESMRegion.GetESMRegions(FarmerBrothersEntitites);
                        techModel.ESMList = ESM.GetServiceManagerDetails(FarmerBrothersEntitites);
                        techModel.RSMList = RSM.GetServiceManagerDetails(FarmerBrothersEntitites);
                        techModel.CCMList = CCM.GetServiceManagerDetails(FarmerBrothersEntitites);


                        message = "|ESMDSMRSM Details saved successfully!";
                    }
                    else
                    {
                        message = "|No Record found with given Data!";
                    }
                }
                catch (Exception)
                {
                    code = ErrorCode.ERROR;
                    message = "|There is a problem in ESMDSMRSM details update!";
                }
            }
            return code;
        }

    }
}