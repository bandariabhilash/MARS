using DataAccess.Db;
using System.ComponentModel.DataAnnotations;

namespace ServiceApis.Models
{
    public class CustomerModel
    {
        public string CustomerId { get; set; }
        [MaxLength(100, ErrorMessage = "Customer Name cannot be longer than 100 characters.")]
        public string CustomerName { get; set; }
        [MaxLength(150, ErrorMessage = "Address1 cannot be longer than 150 characters.")]
        public string Address { get; set; }
        [MaxLength(100, ErrorMessage = "Address2 cannot be longer than 100 characters.")]
        public string Address2 { get; set; }
        [MaxLength(100, ErrorMessage = "Address3 cannot be longer than 100 characters.")]
        public string Address3 { get; set; }
        [MaxLength(50, ErrorMessage = "City cannot be longer than 50 characters.")]
        public string City { get; set; }
        public string State { get; set; }
        [MaxLength(12, ErrorMessage = "Zip Code cannot be longer than 12 characters.")]
        public string ZipCode { get; set; }
        [MaxLength(50, ErrorMessage = "Main Contact Name cannot be longer than 50 characters.")]
        public string MainContactName { get; set; }

        public string AreaCode { get; set; }

        [MaxLength(30, ErrorMessage = "Phone Number cannot be longer than 30 characters.")]
        //[DataType(DataType.PhoneNumber)]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid Phone number")]
        public string PhoneNumber { get; set; }
        public string PhoneExtn { get; set; }
        [MaxLength(150, ErrorMessage = "Email Address cannot be longer than 150 characters.")]
        public string MainEmailAddress { get; set; }
        public string CustomerPreference { get; set; }
        public string RSM { get; set; }
        public string TSM { get; set; }
        public string TSMPhone { get; set; }
        public string TSMEmailAddress { get; set; }
        public string MarketSegment { get; set; }
        public string ProgramName { get; set; }
        public string DistributorName { get; set; }
        public string ServiceTier { get; set; }
        public string ServiceLevel { get; set; }

        public bool Billable { get; set; }
        public string CustomerType { get; set; }

        public string CoverageZone { get; set; }
        public string TechTeamLead { get; set; }
        public int? TechTeamLeadId { get; set; }
        public string TechType { get; set; }
        public int? ResponsibleTechId { get; set; }
        public string ResponsibleTechName { get; set; }
        public string ResponsibleTechPhone { get; set; }
        public string ResponsibleTechBranch { get; set; }
        public int? ResponsibleTechBranchId { get; set; }
        public string SecondaryTechName { get; set; }
        public string SecondaryTechPhone { get; set; }
        public int? SecondaryTechId { get; set; }
        public int? SecondaryTechBranchId { get; set; }
        public string SecondaryTechBrach { get; set; }
        public string FSMName { get; set; }
        public int? FSMId { get; set; }
        public string CustomerSpecialInstructions { get; set; }
        public string CustomerTimeZone { get; set; }
        public string CurrentTime { get; set; }

        public int FBProviderID { get; set; }
        public string PreferredProvider { get; set; }
        public string ManagerName { get; set; }
        public string DSMName { get; set; }
        public string ESMName { get; set; }
        public string RSMName { get; set; }
        public string CCMName { get; set; }
        public string Branch { get; set; }
        public string PricingParent { get; set; }
        public string SrviceTier { get; set; }
        public string ProviderPhone { get; set; }
        public string ManagerPhone { get; set; }
        public string DSMPhone { get; set; }
        public string ESMphone { get; set; }
        public string RSMphone { get; set; }
        public string CCMphone { get; set; }
        public string Region { get; set; }
        public string Route { get; set; }

        public string WorkOrderId { get; set; }
        public string ErfId { get; set; }

        public string LastSaleDate { get; set; }
        public int DaysSinceLastSale { get; set; }

        public string ParentNumber { get; set; }
        public bool unknownCustomer { get; set; }
        public string NonFBCustomerNumber { get; set; }

        public string PaymentTermDesc { get; set; }
        public decimal NetSalesAmt { get; set; }
        public string ContributionMargin { get; set; }

        public string BusinessUnit { get; set; }
        public string PricingParentId { get; set; }
        public string PricingParentDesc { get; set; }
        public string RouteCode { get; set; }
        public string Division { get; set; }
        public string District { get; set; }
        public string CustomerRegion { get; set; }
        public string CustomerBranch { get; set; }
        public string BillingCode { get; set; }
        public string ZoneNumber { get; set; }

        public string Message { get; set; } //Used in Bulk Customer Upload process

        public bool IsNonFBCustomer { get; set; }


        public static int saveCustomerDetails(CustomerModel CustModel, FBContext _context)
        {
            Contact customer = new Contact();
            customer.ContactId = Convert.ToInt32(CustModel.CustomerId);
            customer.CompanyName = CustModel.CustomerName;
            customer.Address1 = CustModel.Address;
            customer.Address2 = CustModel.Address2;
            customer.City = CustModel.City;
            customer.State = CustModel.State;
            customer.PostalCode = CustModel.ZipCode;
            customer.FirstName = CustModel.MainContactName;
            customer.Phone = CustModel.PhoneNumber;
            customer.Email = CustModel.MainEmailAddress;
            customer.SearchType = "CA";
            customer.PricingParentId = CustModel.ParentNumber;
            DateTime CurrentTime = DateTime.Now; //Utility.GetCurrentTime(CustModel.ZipCode, FBE);

            customer.DateCreated = CurrentTime;

            NonFbcustomer nonFBCustomer = _context.NonFbcustomers.Where(n => n.NonFbcustomerId == CustModel.ParentNumber).FirstOrDefault();
            customer.IsNonFbCustomer = nonFBCustomer == null ? false : true;


            _context.Contacts.Add(customer);
            try
            {
                _context.SaveChanges();
                return 1;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
}
