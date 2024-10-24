using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewcoCall.Models
{
    public class CustomerServiceModel
    {
        public CustomerServiceModel()
        {
         
        }
        public CustomerServiceModel(Contact contact)
        {
            this.AccountNumber = contact.ContactID;
            this.CustomerName = string.IsNullOrWhiteSpace(contact.CompanyName) ? "" : contact.CompanyName;
            this.Address1 = string.IsNullOrWhiteSpace(contact.Address1) ? "" : contact.Address1;
            this.Address2 = string.IsNullOrWhiteSpace(contact.Address2) ? "" : contact.Address2;
            this.City = string.IsNullOrWhiteSpace(contact.City) ? "" : contact.City;
            this.State = string.IsNullOrWhiteSpace(contact.State) ? "" : contact.State;
            this.PostalCode = string.IsNullOrWhiteSpace(contact.PostalCode) ? "" : contact.PostalCode;
            this.MainContactName = "";
            this.PhoneNumber = "";// string.IsNullOrWhiteSpace(contact.Phone) ? "" : Utility.FormatPhoneNumber(contact.Phone);
            this.Email = "";
            this.PaymentTerm = string.IsNullOrWhiteSpace(contact.PaymentTerm) ? "" : contact.PaymentTerm;
        }

        public bool IsExistingCustomer { get; set; }
        public int AccountNumber { get; set; }
        public string NewcoCustomerNumber { get; set; }
        public string CallReason { get; set; }
        public string CustomerName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string MainContactName { get; set; }
        public string PhoneNumber { get; set; }
        public string ServiceRequestedPartyName { get; set; }
        public string ServiceRequestedPartyPhone { get; set; }
        public string Email { get; set; }
        public string Comments { get; set; }
        public int Operation { get; set; }
        public string EqpBrand { get; set; }
        public string EqpModel { get; set; }
        public DateTime DateNeeded { get; set; }

        public List<FBCallReason> CallReasonList { get; set; }
        public List<State> StateList { get; set; }

        public int WorkorderId { get; set; }

        public string TravelDistance { get; set; }
        public string Distance { get; set; }
        public string TravelTime { get; set; }
        public decimal TravelAmount { get; set; }
        public decimal Labor { get; set; }
        public string LaborHours { get; set; }
        public string TotalServiceQuote { get; set; }
        public string PaymentTransactionId { get; set; }
        public string FinalTransactionId { get; set; }
        public string PaymentTerm { get; set; }
        public List<WorkorderType> ServiceTypeList { get; set; }
        public string ServiceType { get; set; }
        public List<ServiceRates> ServiceRates { get; set; }
    }

    public class ServiceRates
    {
        public int CallTypeId { get; set; }
        public decimal Amount { get; set; }
    }

    public class WorkorderResultModel
    {
        public int WorkorderId { get; set; }
    }

    public class ServiceQuote
    {
        public string TravelDistance { get; set; }
        public string Distance { get; set; }
        public string TravelTime { get; set; }
        public decimal TravelAmount { get; set; }
        public decimal Labor { get; set; }
        public string LaborHours { get; set; }
        public string TotalServiceQuote { get; set; }
    }
}