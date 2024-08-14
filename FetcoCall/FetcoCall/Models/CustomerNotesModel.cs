using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FetcoCall.Models
{
    public class CustomerNotesModel
    {
        public CustomerNotesModel()
        {

        }
        public CustomerNotesModel(FBCustomerNote custNotes)
        {
            CustomerNotesId = custNotes.CustomerNotesId;
            CustomerId = Convert.ToInt32(custNotes.CustomerId);
            UserId = Convert.ToInt32(custNotes.UserId);
            UserName = custNotes.UserName;
            Status = Convert.ToBoolean(custNotes.IsActive);
            EntryDate = custNotes.EntryDate;
            Notes = "[" + UserName + "] - " + (EntryDate.HasValue ? EntryDate.Value.ToString("MM/dd/yyyy hh:mm tt") : " ") + " - " + custNotes.Notes;
        }

        public int CustomerNotesId { get; set; }
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        [Required(ErrorMessage = "Notes Required")]
        public string Notes { get; set; }
        public bool Status { get; set; }
        public Nullable<System.DateTime> EntryDate { get; set; }
        public IList<CustomerNotes> CustomerNotesResults;

        //PSP Update
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string FBPreferProvider { get; set; }

    }
    public class CustomerNotes
    {
        public int CustomerNotesId { get; set; }
        public int? CustomerId { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string Notes { get; set; }
        public bool? Status { get; set; }
        public string EntryDatestring { get; set; }
        public Nullable<System.DateTime> EntryDate { get; set; }

        //PSP UPDate

        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string FBPreferProvider { get; set; }
    }
}