using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class PricingUpdatesController : BaseController
    {
        // GET: PricingUpdates
        public ActionResult PricingUpdates()
        {
            PricingUpdateModel model = new PricingUpdateModel();

            //var PricingDataList = (from parent in FarmerBrothersEntitites.NonFBCustomers
            //                            join pricing in FarmerBrothersEntitites.ParentPricings on parent.Id equals pricing.ParentId into pricingData
            //                            from pric in pricingData.DefaultIfEmpty()
            //                            select new
            //                            {
            //                                parent.NonFBCustomerId,
            //                                parent.NonFBCustomerName,
            //                                pric.HourlyLablrRate,
            //                                pric.HourlyTravlRate,
            //                                pric.AfterHoursLaborRate,
            //                                pric.AfterHoursTravelRate,
            //                                pric.AfterHoursRatesApply,
            //                                pric.AdditionalFee,
            //                                pric.MilageRate,
            //                                pric.ParentId,
            //                                pric.PartsDiscount
            //                            }).ToList();


            //model.ParentPricingModel = new List<PriceDataModel>();
            //foreach(var item in PricingDataList)
            //{
            //    PriceDataModel dataMdl = new PriceDataModel();
            //    dataMdl.ParentId = item.NonFBCustomerId;
            //    dataMdl.ParentName = item.NonFBCustomerName;
            //    dataMdl.HourlyLaborRate = Convert.ToDecimal(item.HourlyLablrRate);
            //    dataMdl.HourlyTravelRate = Convert.ToDecimal(item.HourlyTravlRate);
            //    dataMdl.AfterHourLaborRate = Convert.ToDecimal(item.AfterHoursLaborRate);
            //    dataMdl.AfterHourTravelRate = Convert.ToDecimal(item.AfterHoursTravelRate);
            //    dataMdl.PartsDiscount = Convert.ToDecimal(item.PartsDiscount);
            //    dataMdl.AdditionalFee = Convert.ToDecimal(item.AdditionalFee);
            //    dataMdl.AfterHourRatesApply = Convert.ToBoolean(item.AfterHoursRatesApply);

            //    model.ParentPricingModel.Add(dataMdl);
            //}

            model.ParentPricingModel = GetParentPricingDetails();
            model.ParentDetails = FarmerBrothersEntitites.NonFBCustomers.ToList();

            model.ThirdPartyPricingModel = Get3rdPartyPricingDetails();
            model.StatePricingModel = GetStatePricingDetails();

            return View(model);
        }

        #region ParentPricing       
        public List<PriceDataModel> GetParentPricingDetails()
        {
            List<PriceDataModel> priceList = new List<PriceDataModel>();
            int pTypeId = FarmerBrothersEntitites.PricingTypes.Where(p => p.PricingTypeName.ToLower() == "parent").Select(s => s.PricingTypeId).FirstOrDefault();

            var PricingDataList = (from parent in FarmerBrothersEntitites.NonFBCustomers
                                   join pricing in FarmerBrothersEntitites.PricingDetails on parent.NonFBCustomerId equals pricing.PricingEntityId into pricingData
                                   from pric in pricingData.DefaultIfEmpty()
                                   select new
                                   {
                                       parentId = parent.NonFBCustomerId,
                                       parentName = parent.NonFBCustomerName,
                                       hourlyLaborPrice = pric.HourlyLablrRate == null ? 0 : pric.HourlyLablrRate,
                                       hourlyTravelPrice = pric.HourlyTravlRate == null ? 0 : pric.HourlyTravlRate,
                                       afterHourLaborPrice = pric.AfterHoursLaborRate == null ? 0 : pric.AfterHoursLaborRate,
                                       afterHourTravlePrice = pric.AfterHoursTravelRate == null ? 0 : pric.AfterHoursTravelRate,
                                       afterHourRateApply = pric.AfterHoursRatesApply,
                                       additionalFee = pric.AdditionalFee == null ? 0 : pric.AdditionalFee,
                                       milageRt = pric.MilageRate == null ? 0 : pric.MilageRate,
                                       id = pric.HourlyLablrRate == null ? 0 : pric.Id,
                                       partsDiscount = pric.PartsDiscount == null ? 0 : pric.PartsDiscount,
                                       approved3rdPartyUse = pric.Approved3rdPartyUse
                                   }).ToList();
            foreach(var priceItem in PricingDataList)
            {
                PriceDataModel data = new PriceDataModel();
                data.PricingEntityId = priceItem.parentId;
                data.PricingEntityName = priceItem.parentName;
                data.HourlyLaborRate = Convert.ToDecimal(priceItem.hourlyLaborPrice);
                data.HourlyTravelRate = Convert.ToDecimal(priceItem.hourlyTravelPrice);
                data.AfterHourLaborRate = Convert.ToDecimal(priceItem.afterHourLaborPrice);
                data.AfterHourTravelRate = Convert.ToDecimal(priceItem.afterHourTravlePrice);
                data.PartsDiscount = Convert.ToDecimal(priceItem.partsDiscount);
                data.AdditionalFee = Convert.ToDecimal(priceItem.additionalFee);
                data.AfterHourRatesApply = Convert.ToBoolean(priceItem.afterHourRateApply);
                data.Approved3rdPartyUse = Convert.ToBoolean(priceItem.approved3rdPartyUse);

                priceList.Add(data);
            }

            return priceList;
        }

        public ActionResult ParentPricingUpdate(PriceDataModel value)
        {
            int ParentId = Convert.ToInt32(value.PricingEntityId);
            NonFBCustomer fbCust = FarmerBrothersEntitites.NonFBCustomers.Where(c => c.NonFBCustomerId == value.PricingEntityId).FirstOrDefault();

            if (fbCust != null)
            {
                PricingDetail ParentItem = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingEntityId.ToString() == fbCust.NonFBCustomerId).FirstOrDefault();

                if (ParentItem != null)
                {
                    ParentItem.AdditionalFee = value.AdditionalFee;
                    ParentItem.AfterHoursLaborRate = value.AfterHourLaborRate;
                    ParentItem.AfterHoursRatesApply = value.AfterHourRatesApply;
                    ParentItem.AfterHoursTravelRate = value.AfterHourTravelRate;
                    ParentItem.HourlyLablrRate = value.HourlyLaborRate;
                    ParentItem.HourlyTravlRate = value.HourlyTravelRate;
                    ParentItem.MilageRate = value.MileageRate;
                    ParentItem.PartsDiscount = value.PartsDiscount;
                    ParentItem.Approved3rdPartyUse = value.Approved3rdPartyUse;
                }
                else
                {
                    PricingDetail ParentItm = new PricingDetail();

                    NonFBCustomer parent = FarmerBrothersEntitites.NonFBCustomers.Where(c => c.NonFBCustomerId == value.PricingEntityId).FirstOrDefault();

                    ParentItm.PricingEntityId = parent != null ? parent.NonFBCustomerId : "";
                    ParentItm.PricingEntityName = parent != null ? parent.NonFBCustomerName : "";
                    ParentItm.AdditionalFee = value.AdditionalFee;
                    ParentItm.AfterHoursLaborRate = value.AfterHourLaborRate;
                    ParentItm.AfterHoursRatesApply = value.AfterHourRatesApply;
                    ParentItm.AfterHoursTravelRate = value.AfterHourTravelRate;
                    ParentItm.HourlyLablrRate = value.HourlyLaborRate;
                    ParentItm.HourlyTravlRate = value.HourlyTravelRate;
                    ParentItm.MilageRate = value.MileageRate;
                    ParentItm.PartsDiscount = value.PartsDiscount;
                    ParentItm.PricingTypeId = FarmerBrothersEntitites.PricingTypes.Where(p => p.PricingTypeName.ToLower() == "parent").Select(s => s.PricingTypeId).FirstOrDefault();
                    ParentItm.Approved3rdPartyUse = value.Approved3rdPartyUse;

                    FarmerBrothersEntitites.PricingDetails.Add(ParentItm);

                }
                FarmerBrothersEntitites.SaveChanges();
            }
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public FileResult ParentPricingExcelExport()
        {
            List<PriceDataModel> ParentPricingModelResults = GetParentPricingDetails();
            string gridModel = HttpContext.Request.Params["GridModel"];

            string[] columns = { "PricingEntityId", "PricingEntityName", "HourlyLaborRate" , "HourlyTravelRate", "AfterHourLaborRate", "AfterHourTravelRate", "PartsDiscount", "AdditionalFee", "AfterHourRatesApply", "Approved3rdPartyUse" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(ParentPricingModelResults, "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "ParentPricingData.xlsx");

        }
        #endregion

        #region 3rdParty Pricing
        public List<PriceDataModel> Get3rdPartyPricingDetails()
        {
            List<PriceDataModel> priceList = new List<PriceDataModel>();
            int pTypeId = FarmerBrothersEntitites.PricingTypes.Where(p => p.PricingTypeName.ToLower() == "parent").Select(s => s.PricingTypeId).FirstOrDefault();

            var PricingDataList = (from parent in FarmerBrothersEntitites.TECH_HIERARCHY
                                   join pricing in FarmerBrothersEntitites.PricingDetails on parent.DealerId.ToString() equals pricing.PricingEntityId into pricingData
                                   from pric in pricingData.DefaultIfEmpty()
                                   where parent.FamilyAff.ToLower() == "spt"
                                   select new
                                   {
                                       parentId = parent.DealerId,
                                       parentName = parent.CompanyName,
                                       hourlyLaborPrice = pric.HourlyLablrRate == null ? 0 : pric.HourlyLablrRate,
                                       hourlyTravelPrice = pric.HourlyTravlRate == null ? 0 : pric.HourlyTravlRate,
                                       afterHourLaborPrice = pric.AfterHoursLaborRate == null ? 0 : pric.AfterHoursLaborRate,
                                       afterHourTravlePrice = pric.AfterHoursTravelRate == null ? 0 : pric.AfterHoursTravelRate,
                                       afterHourRateApply = pric.AfterHoursRatesApply,
                                       additionalFee = pric.AdditionalFee == null ? 0 : pric.AdditionalFee,
                                       milageRt = pric.MilageRate == null ? 0 : pric.MilageRate,
                                       id = pric.HourlyLablrRate == null ? 0 : pric.Id,
                                       partsDiscount = pric.PartsDiscount == null ? 0 : pric.PartsDiscount,
                                       approved3rdPartyUse = pric.Approved3rdPartyUse
                                   }).ToList();
            foreach (var priceItem in PricingDataList)
            {
                PriceDataModel data = new PriceDataModel();
                data.PricingEntityId = priceItem.parentId.ToString();
                data.PricingEntityName = priceItem.parentName;
                data.HourlyLaborRate = Convert.ToDecimal(priceItem.hourlyLaborPrice);
                data.HourlyTravelRate = Convert.ToDecimal(priceItem.hourlyTravelPrice);
                data.AfterHourLaborRate = Convert.ToDecimal(priceItem.afterHourLaborPrice);
                data.AfterHourTravelRate = Convert.ToDecimal(priceItem.afterHourTravlePrice);
                data.PartsDiscount = Convert.ToDecimal(priceItem.partsDiscount);
                data.AdditionalFee = Convert.ToDecimal(priceItem.additionalFee);
                data.AfterHourRatesApply = Convert.ToBoolean(priceItem.afterHourRateApply);
                data.Approved3rdPartyUse = Convert.ToBoolean(priceItem.approved3rdPartyUse);

                priceList.Add(data);
            }

            return priceList;
        }

        public ActionResult ThirdPartyPricingUpdate(PriceDataModel value)
        {
            int ParentId = Convert.ToInt32(value.PricingEntityId);
            TECH_HIERARCHY techDtls = FarmerBrothersEntitites.TECH_HIERARCHY.Where(c => c.DealerId.ToString() == value.PricingEntityId).FirstOrDefault();

            if (techDtls != null)
            {
                PricingDetail ParentItem = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingEntityId.ToString() == techDtls.DealerId.ToString()).FirstOrDefault();

                if (ParentItem != null)
                {
                    ParentItem.AdditionalFee = value.AdditionalFee;
                    ParentItem.AfterHoursLaborRate = value.AfterHourLaborRate;
                    ParentItem.AfterHoursRatesApply = value.AfterHourRatesApply;
                    ParentItem.AfterHoursTravelRate = value.AfterHourTravelRate;
                    ParentItem.HourlyLablrRate = value.HourlyLaborRate;
                    ParentItem.HourlyTravlRate = value.HourlyTravelRate;
                    ParentItem.MilageRate = value.MileageRate;
                    ParentItem.PartsDiscount = value.PartsDiscount;
                    ParentItem.Approved3rdPartyUse = value.Approved3rdPartyUse;
                }
                else
                {
                    PricingDetail ParentItm = new PricingDetail();

                    ParentItm.PricingEntityId = value.PricingEntityId;
                    ParentItm.PricingEntityName = value.PricingEntityName;
                    ParentItm.AdditionalFee = value.AdditionalFee;
                    ParentItm.AfterHoursLaborRate = value.AfterHourLaborRate;
                    ParentItm.AfterHoursRatesApply = value.AfterHourRatesApply;
                    ParentItm.AfterHoursTravelRate = value.AfterHourTravelRate;
                    ParentItm.HourlyLablrRate = value.HourlyLaborRate;
                    ParentItm.HourlyTravlRate = value.HourlyTravelRate;
                    ParentItm.MilageRate = value.MileageRate;
                    ParentItm.PartsDiscount = value.PartsDiscount;
                    ParentItm.PricingTypeId = FarmerBrothersEntitites.PricingTypes.Where(p => p.PricingTypeName.ToLower() == "3rdparty").Select(s => s.PricingTypeId).FirstOrDefault();
                    ParentItm.Approved3rdPartyUse = value.Approved3rdPartyUse;

                    FarmerBrothersEntitites.PricingDetails.Add(ParentItm);

                }
                FarmerBrothersEntitites.SaveChanges();
            }
            return Json(value, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public FileResult ThirdPartyPricingExcelExport()
        {
            List<PriceDataModel> ParentPricingModelResults = Get3rdPartyPricingDetails();
            string gridModel = HttpContext.Request.Params["GridModel"];

            string[] columns = { "PricingEntityId", "PricingEntityName", "HourlyLaborRate", "HourlyTravelRate", "AfterHourLaborRate", "AfterHourTravelRate", "PartsDiscount", "AdditionalFee", "AfterHourRatesApply", "Approved3rdPartyUse" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(ParentPricingModelResults, "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "ThirdPartyPricingData.xlsx");

        }
        #endregion

        #region State Pricing
        public List<PriceDataModel> GetStatePricingDetails()
        {
            List<PriceDataModel> priceList = new List<PriceDataModel>();
            int pTypeId = FarmerBrothersEntitites.PricingTypes.Where(p => p.PricingTypeName.ToLower() == "state").Select(s => s.PricingTypeId).FirstOrDefault();

            var PricingDataList = (from parent in FarmerBrothersEntitites.States
                                   join pricing in FarmerBrothersEntitites.PricingDetails on parent.StateCode equals pricing.PricingEntityId into pricingData
                                   from pric in pricingData.DefaultIfEmpty()
                                   select new
                                   {
                                       parentId = parent.StateCode,
                                       parentName = parent.StateName,
                                       hourlyLaborPrice = pric.HourlyLablrRate == null ? 0 : pric.HourlyLablrRate,
                                       hourlyTravelPrice = pric.HourlyTravlRate == null ? 0 : pric.HourlyTravlRate,
                                       afterHourLaborPrice = pric.AfterHoursLaborRate == null ? 0 : pric.AfterHoursLaborRate,
                                       afterHourTravlePrice = pric.AfterHoursTravelRate == null ? 0 : pric.AfterHoursTravelRate,
                                       afterHourRateApply = pric.AfterHoursRatesApply,
                                       additionalFee = pric.AdditionalFee == null ? 0 : pric.AdditionalFee,
                                       milageRt = pric.MilageRate == null ? 0 : pric.MilageRate,
                                       id = pric.HourlyLablrRate == null ? 0 : pric.Id,
                                       partsDiscount = pric.PartsDiscount == null ? 0 : pric.PartsDiscount,
                                       approved3rdPartyUse = pric.Approved3rdPartyUse
                                   }).ToList();
            foreach (var priceItem in PricingDataList)
            {
                PriceDataModel data = new PriceDataModel();
                data.PricingEntityId = priceItem.parentId.ToString();
                data.PricingEntityName = priceItem.parentName;
                data.HourlyLaborRate = Convert.ToDecimal(priceItem.hourlyLaborPrice);
                data.HourlyTravelRate = Convert.ToDecimal(priceItem.hourlyTravelPrice);
                data.AfterHourLaborRate = Convert.ToDecimal(priceItem.afterHourLaborPrice);
                data.AfterHourTravelRate = Convert.ToDecimal(priceItem.afterHourTravlePrice);
                data.PartsDiscount = Convert.ToDecimal(priceItem.partsDiscount);
                data.AdditionalFee = Convert.ToDecimal(priceItem.additionalFee);
                data.AfterHourRatesApply = Convert.ToBoolean(priceItem.afterHourRateApply);
                data.Approved3rdPartyUse = Convert.ToBoolean(priceItem.approved3rdPartyUse);

                priceList.Add(data);
            }

            return priceList;
        }

        public ActionResult StatePricingUpdate(PriceDataModel value)
        {
            string ParentId = value.PricingEntityId;
            State techDtls = FarmerBrothersEntitites.States.Where(c => c.StateCode == value.PricingEntityId).FirstOrDefault();

            if (techDtls != null)
            {
                PricingDetail ParentItem = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingEntityId.ToString() == techDtls.StateCode).FirstOrDefault();

                if (ParentItem != null)
                {
                    ParentItem.AdditionalFee = value.AdditionalFee;
                    ParentItem.AfterHoursLaborRate = value.AfterHourLaborRate;
                    ParentItem.AfterHoursRatesApply = value.AfterHourRatesApply;
                    ParentItem.AfterHoursTravelRate = value.AfterHourTravelRate;
                    ParentItem.HourlyLablrRate = value.HourlyLaborRate;
                    ParentItem.HourlyTravlRate = value.HourlyTravelRate;
                    ParentItem.MilageRate = value.MileageRate;
                    ParentItem.PartsDiscount = value.PartsDiscount;
                    ParentItem.Approved3rdPartyUse = value.Approved3rdPartyUse;
                }
                else
                {
                    PricingDetail ParentItm = new PricingDetail();

                    ParentItm.PricingEntityId = value.PricingEntityId;
                    ParentItm.PricingEntityName = value.PricingEntityName;
                    ParentItm.AdditionalFee = value.AdditionalFee;
                    ParentItm.AfterHoursLaborRate = value.AfterHourLaborRate;
                    ParentItm.AfterHoursRatesApply = value.AfterHourRatesApply;
                    ParentItm.AfterHoursTravelRate = value.AfterHourTravelRate;
                    ParentItm.HourlyLablrRate = value.HourlyLaborRate;
                    ParentItm.HourlyTravlRate = value.HourlyTravelRate;
                    ParentItm.MilageRate = value.MileageRate;
                    ParentItm.PartsDiscount = value.PartsDiscount;
                    ParentItm.PricingTypeId = FarmerBrothersEntitites.PricingTypes.Where(p => p.PricingTypeName.ToLower() == "state").Select(s => s.PricingTypeId).FirstOrDefault();
                    ParentItm.Approved3rdPartyUse = value.Approved3rdPartyUse;

                    FarmerBrothersEntitites.PricingDetails.Add(ParentItm);

                }
                FarmerBrothersEntitites.SaveChanges();
            }
            return Json(value, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public FileResult StatePricingExcelExport()
        {
            List<PriceDataModel> ParentPricingModelResults = GetStatePricingDetails();
            string gridModel = HttpContext.Request.Params["GridModel"];

            string[] columns = { "PricingEntityId", "PricingEntityName", "HourlyLaborRate", "HourlyTravelRate", "AfterHourLaborRate", "AfterHourTravelRate", "PartsDiscount", "AdditionalFee", "AfterHourRatesApply", "Approved3rdPartyUse" };
            byte[] filecontent = ExcelExportHelper.ExportExcel(ParentPricingModelResults, "", true, columns);
            var fileStream = new MemoryStream(filecontent);
            return File(filecontent, System.Net.Mime.MediaTypeNames.Application.Octet, "StatePricingData.xlsx");

        }
        #endregion
    }
}