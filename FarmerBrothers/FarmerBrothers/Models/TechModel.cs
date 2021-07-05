using FarmerBrothers.Data;
using System;

namespace FarmerBrothers.Models
{
    public class TechModel
    {
        public TechModel(TechHierarchyView view)
        {
            Branch = view.PreferredProvider;
            if (!string.IsNullOrWhiteSpace(view.PreferredProvider))
            {
                TechName = view.PreferredProvider;
            }
            else
            {
                TechName = "";
            }

            if (view.TechID > 0)
            {
                TechId = view.TechID.ToString();
            }
            else
            {
                TechId = "";
            }

            if (!string.IsNullOrWhiteSpace(view.ProviderPhone))
            {
                TechPhone = Utilities.Utility.FormatPhoneNumber(view.AreaCode+view.ProviderPhone.Replace("-",""));
            }
            else
            {
                TechPhone = "";
            }

            if (!string.IsNullOrEmpty(view.SearchType))
            {
                if (view.SearchType.ToLower() == "spi")
                {
                    isActive = false;
                }
                else
                {
                    isActive = true;
                }
            }
        }

        public string Branch { get; set; }
        public string TechId { get; set; }
        public string TechName { get; set; }
        public string TechPhone { get; set; }
        public string AssignedStatus { get; set; }
        public string LastCommunication { get; set; }
        public string EventScheduleDate { get; set; }
        public bool isActive { get; set; }
    }
}