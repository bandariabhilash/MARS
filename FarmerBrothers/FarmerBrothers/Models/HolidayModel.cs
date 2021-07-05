using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace FarmerBrothers.Models
{
    public class HolidayModel
    {
        [Required(ErrorMessage = "Search Name Required")]
        public string HolidayName { get; set; }

        [Required(ErrorMessage = "Please select Date")]
        public string year { get; set; }
        public bool Status { get; set; }
        public Nullable<DateTime> HolidayDate { get; set; }
        public IList<Holiday> SearchResults;
        public IList<int> Years;
    }

    public class Holiday
    {
        public string HolidayName { get; set; }

        public int HolidayUniqueId { get; set; }
        public string HolidayDatestring { get; set; }
        public Nullable<DateTime> HolidayDate { get; set; }

        public bool Status { get; set; }
    }
}