using FarmerBrothers.Data;
//using FarmerBrothers.FeastLocationService;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace FarmerBrothers.Models
{
    public class BranchModel
    {
        public BranchModel()
        { }
        public BranchModel(TechDispatchWithDistance techData)
        {
            ServiceCenterId = Convert.ToInt32(techData.ServiceCenterId);
            Id = Convert.ToInt32(techData.ServiceCenterId);
            Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(techData.Name.ToLower());
            Phone = Utilities.Utility.FormatPhoneNumber(techData.Phone);
            AlternativePhone = Utilities.Utility.FormatPhoneNumber(techData.AlternativePhone); 
            Distance = Convert.ToDouble(techData.Distance);
            Type = techData.TechType;
            BranchName = techData.BranchName;
            City = techData.City;
            isUnavailable = techData.isUnavailable;
            ReplaceTechnician = techData.ReplaceTechnician;
        }

        public BranchModel(TechHierarchyView serviceCenter, string customerZip, FarmerBrothersEntities FarmerBrothersEntities)
        {
            ServiceCenterId = Convert.ToInt32(serviceCenter.TechID);
            Id = Convert.ToDecimal(serviceCenter.TechID);
            if (!string.IsNullOrWhiteSpace(serviceCenter.PreferredProvider))
            {
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(serviceCenter.PreferredProvider.ToLower()); ;
            }
            else
            {
                Name = "";
            }

            Phone = Utilities.Utility.FormatPhoneNumber(serviceCenter.ProviderPhone);
            Distance = Utilities.Utility.GetDistance(FarmerBrothersEntities, customerZip, serviceCenter.TechZip);

            if (!string.IsNullOrWhiteSpace(serviceCenter.PreferredProvider) && serviceCenter.PreferredProvider.IndexOf("Internal") >= 0)
            {
                Type = "FB";
            }
            else
            {
                Type = "3rd Party";
            }
        }

        public int? ServiceCenterId { get; set; }
        public decimal Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }        
        public string Phone { get; set; }
        public string AlternativePhone { get; set; }        
        public double Distance { get; set; }
        public string City { get; set; }
        public Int16 isUnavailable { get; set; }
        public string ReplaceTechnician { get; set; }
    }
}