using FarmerBrothers.Data;
using System.Globalization;

namespace FarmerBrothers.Models
{
    public class VendorDataModel
    {
        public VendorDataModel(string manufacturer)
        {
            VendorDescription = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(manufacturer.Trim().ToLower()); ;
        }

        public string VendorDescription { get; set; }
    }

    public class EstimateApprovedByModel
    {

        public EstimateApprovedByModel(string EstimateApprovedBy, int? EstimateApprovedId)
        {
            this.EstimateApprovedBy = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(EstimateApprovedBy.Trim().ToLower());
            this.EstimateApprovedId = EstimateApprovedId;
        }

        public string EstimateApprovedBy { get; set; }
        public int? EstimateApprovedId { get; set; }
    }

    public class TransmitNotesModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
