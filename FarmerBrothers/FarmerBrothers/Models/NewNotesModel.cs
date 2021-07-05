
using System.Web.Mvc;
namespace FarmerBrothers.Models
{
    public class NewNotesModel
    {
        [AllowHtml]
        public string Text { get; set; }
        public string Value { get; set; }
    }
}