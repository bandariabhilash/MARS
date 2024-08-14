using FarmerBrothers.Data;
//using FarmerBrothers.FeastLocationService;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
//using Customer = FarmerBrothers.FeastLocationService.Customer;

namespace FarmerBrothers.Models
{
    public class CustomerSearchModel
    {
        public CustomerSearchModel()
        {
            CustomerID = 0;
            CustomerName = string.Empty;
            Phone = string.Empty;
            Address = string.Empty;
            City = string.Empty;
            State = string.Empty;
            ParentId = string.Empty;
            PhoneWithAreaCode = string.Empty;
            LongAddressNumber = string.Empty;
            ZipCode = 0;
            using (FarmerBrothersEntities entities = new FarmerBrothersEntities())
            {
                States = Utility.GetStates(entities);
            }
        }

        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string LongAddressNumber { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ParentId { get; set; }
        public string PhoneWithAreaCode { get; set; }
        public int ZipCode { get; set; }
        public string Branch { get; set; }

        public IList<State> States;
        public IList<Contact> CustomerSearchResults;
    }

    public class CustomerZipcodeUpdateModel
    {
        public string AccountNumber { get; set; }
        public string ZipCode { get; set; }
        public string SalesEmail { get; set; }
        public string ParentId { get; set; }
    }

    public class PMUploadContactUpdateModel
    {
        public PMUploadContactUpdateModel() { }
        public PMUploadContactUpdateModel(Contact_PMUploadsALL contact)
        {
            this.UserId = contact.UniqueID;
            this.AccountNumber = Convert.ToInt32(contact.ContactID);
            this.ContactName = contact.ContactName;
            this.CustomerName = contact.CustomerName;
            this.IsActive = Convert.ToBoolean(contact.IsActive);
        }

        public int UserId { get; set; }
        public int AccountNumber { get; set; }
        public string CustomerName { get; set; }
        public string ContactName { get; set; }
        public bool IsActive { get; set; }
    }
}