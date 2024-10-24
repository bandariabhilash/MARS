using DataAccess.Db;
using ServiceApis.IRepository;
using ServiceApis.Models;
using ServiceApis.Utilities;

namespace ServiceApis.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly FBContext _context;

        public CustomerRepository(FBContext context)
        {
            _context = context;
        }

        public CustomerModel GetCustomerDetails(int CustomerId)
        {
            CustomerModel CustMdl = null;

            Contact contact = _context.Contacts.Where(c => c.ContactId == CustomerId).FirstOrDefault();

            if (contact != null)
            {
                CustMdl = new CustomerModel();

                //CustMdl.ManagerName = WebConfigurationManager.AppSettings["ManagerName"];
               //CustMdl.ManagerPhone = Utility.FormatPhoneNumber(WebConfigurationManager.AppSettings["ManagerPhone"]);
                CustMdl.CustomerId = contact.ContactId.ToString();
                CustMdl.CustomerName = contact.CompanyName;
                CustMdl.Address = contact.Address1;
                CustMdl.Address2 = contact.Address2;
                CustMdl.City = contact.City;
                CustMdl.State = contact.State;
                CustMdl.ZipCode = contact.PostalCode;
                CustMdl.MainContactName = contact.FirstName + ' ' + contact.LastName;
                CustMdl.AreaCode = contact.AreaCode;
                CustMdl.PhoneNumber = Utility.FormatPhoneNumber(contact.PhoneWithAreaCode);
                CustMdl.MainEmailAddress = contact.Email;
                CustMdl.TSM = contact.TierDesc;
                CustMdl.TSMPhone = Utility.FormatPhoneNumber(contact.PhoneWithAreaCode);
                CustMdl.DistributorName = contact.DistributorName;
                CustMdl.WorkOrderId = null;
                CustMdl.ErfId = null;
                CustMdl.Region = contact.RegionNumber;
                CustMdl.Branch = contact.Branch;
                CustMdl.Route = contact.Route;
                CustMdl.PricingParent = contact.PricingParentDesc;
                CustMdl.ServiceLevel = contact.ServiceLevelCode;
                CustMdl.LastSaleDate = contact.LastSaleDate;
                CustMdl.ParentNumber = contact.PricingParentId;

                CustMdl.unknownCustomer = contact.IsUnknownUser == null ? false : contact.IsUnknownUser == 1 ? true : false;
                CustMdl.IsNonFBCustomer = contact.IsNonFbCustomer == null ? false : Convert.ToBoolean(contact.IsNonFbCustomer);


                CustMdl.ServiceTier = string.IsNullOrEmpty(contact.ProfitabilityTier) ? " - " : contact.ProfitabilityTier;

                CustMdl.NetSalesAmt = contact.NetSalesAmount == null ? 0 : Convert.ToDecimal(contact.NetSalesAmount);
                CustMdl.ContributionMargin = string.IsNullOrEmpty(contact.ContributionMargin) ? "" : contact.ContributionMargin;

                string paymentTerm = string.IsNullOrEmpty(contact.PaymentTerm) ? "" : contact.PaymentTerm;
                if (!string.IsNullOrEmpty(paymentTerm))
                {
                    JdepaymentTerm paymentDesc = _context.JdepaymentTerms.Where(c => c.PaymentTerm == paymentTerm).FirstOrDefault();
                    CustMdl.PaymentTermDesc = paymentDesc == null ? "" : paymentDesc.Description;
                }
                else
                {
                    CustMdl.PaymentTermDesc = "";
                }


                CustMdl.CustomerTimeZone = Utility.GetCustomerTimeZone(contact.PostalCode, _context);
                CustMdl.CurrentTime = DateTime.Now.ToString("hh:mm tt"); //Utility.GetCurrentTime(contact.PostalCode).ToString("hh:mm tt");


                //CustMdl.DaysSinceLastSale = CustMdl.ConvertToDays(CustMdl.CurrentTime, CustMdl.LastSaleDate);
                CustMdl.CustomerSpecialInstructions = contact.CustomerSpecialInstructions;


                //CustMdl.UtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMilliseconds;
                int? providerNumber = contact.FbproviderId == null ? 0 : Convert.ToInt32(contact.FbproviderId);
               
                    CustMdl.ESMName = string.IsNullOrEmpty(contact.Esmname) ? "" : contact.Esmname;
                    CustMdl.ESMphone = string.IsNullOrEmpty(contact.Esmphone) ? "" : Utility.FormatPhoneNumber(contact.Esmphone);

                    CustMdl.DSMName = string.IsNullOrEmpty(contact.Ccmname) ? "" : contact.Ccmname;
                    CustMdl.DSMPhone = string.IsNullOrEmpty(contact.Ccmphone) ? "" : Utility.FormatPhoneNumber(contact.Ccmphone);

                    CustMdl.RSMName = string.IsNullOrEmpty(contact.Rsmname) ? "" : contact.Rsmname;
                    CustMdl.RSMphone = string.IsNullOrEmpty(contact.Rsmphone) ? "" : Utility.FormatPhoneNumber(contact.Rsmphone);

                    CustMdl.CCMName = string.IsNullOrEmpty(contact.Ccmname) ? "" : contact.Ccmname;
                    CustMdl.CCMphone = string.IsNullOrEmpty(contact.Ccmphone) ? "" : Utility.FormatPhoneNumber(contact.Ccmphone);

                    var Providers = _context.TechHierarchies.FirstOrDefault(x => x.DealerId == providerNumber);
                    if (Providers != null)
                    {
                        CustMdl.FBProviderID = Providers.DealerId;
                        CustMdl.PreferredProvider = Providers.CompanyName;
                        string providerPhone = string.Empty;
                        if (Providers.Phone != null && Providers.Phone.Replace("-", "").Length == 7)
                        {
                            providerPhone = Providers.AreaCode + Providers.Phone.Replace("-", "");
                        }
                        else
                        {
                            providerPhone = Providers.Phone;
                        }
                        CustMdl.ProviderPhone = Utility.FormatPhoneNumber(providerPhone); ;
                    }
                
            }

            return CustMdl;
        }

       
        public void CreateUnknownCustomer(CustomerModel CustModel)
        {
            //FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities();
            Contact customer = new Contact();
            customer.CompanyName = CustModel.CustomerName;
            customer.Address1 = CustModel.Address;
            customer.Address2 = CustModel.Address2;
            customer.City = CustModel.City;
            customer.State = CustModel.State;
            customer.PostalCode = CustModel.ZipCode;
            customer.FirstName = CustModel.MainContactName;
            customer.PhoneWithAreaCode = CustModel.PhoneNumber;
            customer.Email = CustModel.MainEmailAddress;
            customer.SearchType = "CA";
            customer.PricingParentId = CustModel.ParentNumber;
            DateTime CurrentTime = DateTime.Now; //Utility.GetCurrentTime(CustModel.ZipCode, FBE);

            customer.DateCreated = CurrentTime;
            customer.IsUnknownUser = 1;

            NonFbcustomer nonFBCustomer = _context.NonFbcustomers.Where(n => n.NonFbcustomerId == CustModel.ParentNumber).FirstOrDefault();
            customer.IsNonFbCustomer = nonFBCustomer == null ? false : true;

            IndexCounterModel counter = Utility.GetIndexCounter("UnknownCustomerID", 1);
            int id = Convert.ToInt32(counter.IndexValue++);
            customer.ContactId = id;
            CustModel.CustomerId = id.ToString();
            _context.Contacts.Add(customer);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                
            }


        }

    }
}
