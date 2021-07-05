using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customer = FarmerBrothers.Data.Contact;
using FarmerBrothers.Data;
using System.Web;
using System.Text.RegularExpressions;
using LinqKit;


namespace FarmerBrothers.BL
{
    public class CustomerLogic
    {
        public IList<Customer> GetCustomers1(CustomerSearchDataModel customerSearchModel)
        {
            var predicate = PredicateBuilder.True<Contact>();

            if (customerSearchModel.CustomerID > 0 && !string.IsNullOrWhiteSpace(customerSearchModel.CustomerID.ToString()))
            {
                predicate = predicate.And(e => e.ContactID.ToString().Contains(customerSearchModel.CustomerID.ToString()));
            }
            return null;
        }
        public IList<Customer> GetCustomers(CustomerSearchDataModel customerSearchModel)
        {
            IList<Customer> customers = new List<Customer>();
            if (customerSearchModel != null)
            {
                try
                {
                    var predicate = PredicateBuilder.True<Contact>();

                    if (customerSearchModel.CustomerID > 0 && !string.IsNullOrWhiteSpace(customerSearchModel.CustomerID.ToString()))
                    {
                        predicate = predicate.And(e => e.ContactID.ToString().Contains(customerSearchModel.CustomerID.ToString()));
                    }

                    if (!string.IsNullOrWhiteSpace(customerSearchModel.CustomerName))
                    {
                        predicate = predicate.And(w => w.CompanyName.ToString().Contains(customerSearchModel.CustomerName));
                    }

                    if (!string.IsNullOrWhiteSpace(customerSearchModel.Phone))
                    {
                        predicate = predicate.And(w => w.Phone.ToString().Contains(customerSearchModel.Phone));
                    }

                    if (!string.IsNullOrWhiteSpace(customerSearchModel.Address))
                    {
                        predicate = predicate.And(w => w.Address1.ToString().Contains(customerSearchModel.Address));
                    }

                    if (!string.IsNullOrWhiteSpace(customerSearchModel.City))
                    {
                        predicate = predicate.And(w => w.City.ToString().Contains(customerSearchModel.City));
                    }

                    if (!string.IsNullOrWhiteSpace(customerSearchModel.State) && customerSearchModel.State != "n/a")
                    {
                        predicate = predicate.And(w => w.State.ToString().Contains(customerSearchModel.State));
                    }

                    var farmerBrothersEntitites = new Data.FarmerBrothersEntities();
                    IQueryable<Contact> contacts = farmerBrothersEntitites.Set<Contact>().AsExpandable().Where(predicate);

                    foreach (Customer customer in contacts)
                    {
                        string phoneNumber = string.Empty;
                        if (!string.IsNullOrWhiteSpace(customer.Phone))
                        {
                            phoneNumber = customer.Phone;
                        }
                        customer.Phone = FormatPhoneNumber(phoneNumber);
                        customers.Add(customer);
                    }
                }
                catch (Exception e)
                {
                    //Need to log this exception
                }

                customers = customers.OrderBy(c => c.FirstName).ToList();
            }

            return customers;
        }

        public static string FormatPhoneNumber(string phoneNumber)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    int xposition = phoneNumber.ToUpper().IndexOf('X');
                    if (phoneNumber.Length == 10)
                    {
                        phoneNumber = Regex.Replace(phoneNumber, @"-+", "");
                        phoneNumber = Regex.Replace(phoneNumber, @"\s+", "");
                        phoneNumber = String.Format("{0:(###) ###-#### }", double.Parse(phoneNumber));
                    }
                    else if (xposition > 0)
                    {
                        string newPhoneNumber = phoneNumber.Substring(0, xposition);

                        newPhoneNumber = Regex.Replace(newPhoneNumber, @"-+", "");
                        newPhoneNumber = Regex.Replace(newPhoneNumber, @"\s+", "");
                        newPhoneNumber = String.Format("{0:(###) ###-#### }", double.Parse(newPhoneNumber));

                        phoneNumber = newPhoneNumber + phoneNumber.Substring(xposition);

                    }
                }
            }
            catch (Exception e)
            {

            }
            return phoneNumber;
        }
    }

    public class CustomerSearchDataModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public IList<State> States;
        public IList<Contact> CustomerSearchResults;
    }
}
