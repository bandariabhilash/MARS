using FarmerBrothers.Data;
using System;
using System.Globalization;

namespace FarmerBrothers.Models
{
    public class TeamLeadModel
    {
        public TeamLeadModel(TechHierarchyView view)
        {
            Id = Convert.ToDecimal(view.TechID);
            if (!string.IsNullOrWhiteSpace(view.PreferredProvider))
            {
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(view.PreferredProvider.ToLower());
            }
            else
            {
                Name = "";
            }
        }

        public decimal? Id { get; set; }
        public string Name { get; set; }
    }
}