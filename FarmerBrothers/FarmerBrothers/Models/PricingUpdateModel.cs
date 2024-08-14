using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class PricingUpdateModel
    {
       public List<PriceDataModel> ParentPricingModel { get; set; }
        public List<NonFBCustomer> ParentDetails { get; set; }
        public List<PriceDataModel> ThirdPartyPricingModel { get; set; }
        public List<PriceDataModel> StatePricingModel { get; set; }
    }

    public class PriceDataModel
    {
        public string PricingEntityId { get; set; }
        public string PricingEntityName { get; set; }
        public decimal HourlyTravelRate { get; set; }
        public decimal HourlyLaborRate { get; set; }
        public decimal MileageRate { get; set; }
        public decimal AfterHourTravelRate { get; set; }
        public decimal AfterHourLaborRate { get; set; }
        public decimal PartsDiscount { get; set; }
        public bool AfterHourRatesApply { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool Approved3rdPartyUse { get; set; }
    }
}