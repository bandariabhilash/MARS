using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NewcoCall.Models
{
    public class CustomerModel
    {
        public IList<State> States;
        
        public double UtcOffset;
        public int TotalCallsCount;
        public string IsBillable;
        public string ServiceLevelDesc;
        public string ERFStatus;
        
        public IList<NonFBCustomer> NonFBCustomerList;

        public CustomerModel()
        {
            using (NewcoEntity entities = new NewcoEntity())
            {
               
            }
        }

        

        
        public int ConvertToDays(string Date1, string Date2)
        {
            int NoOfDays = 0;

            if (!string.IsNullOrEmpty(Date1) && !string.IsNullOrEmpty(Date2))
            {
                DateTime d1 = Convert.ToDateTime(Date1);
                DateTime d2 = Convert.ToDateTime(Date2);

                NoOfDays = (d1 - d2).Days;
            }

            return NoOfDays;
        }

        public CustomerModel(WorkOrder workOrder, NewcoEntity FarmerBrothersEntitites)
        {
            if (workOrder.CustomerID.HasValue)
            {
                this.CustomerId = workOrder.CustomerID.Value.ToString();
            }

            this.CustomerName = workOrder.CustomerName;
            this.Address = workOrder.CustomerAddress;
            this.City = workOrder.CustomerCity;
            this.State = workOrder.CustomerState;
            this.ZipCode = workOrder.CustomerZipCode;
            this.MainContactName = workOrder.CustomerMainContactName;
            this.PhoneNumber = Utility.FormatPhoneNumber(workOrder.CustomerPhone);
            this.MainEmailAddress = workOrder.CustomerMainEmail;
            this.CustomerPreference = workOrder.CustomerCustomerPreferences;
            this.RSM = workOrder.Rsm;
            this.TSM = workOrder.Tsm;
            this.TSMPhone = Utility.FormatPhoneNumber(workOrder.TSMPhone);
            this.TSMEmailAddress = workOrder.TSMEmail;
            this.MarketSegment = workOrder.MarketSegment;
            this.ProgramName = workOrder.ProgramName;
            this.DistributorName = workOrder.DistributorName;
            this.ServiceTier = workOrder.ServiceTier;


            this.CustomerType = string.Empty;
            this.WorkOrderId = workOrder.WorkorderID.ToString();
            if (!string.IsNullOrEmpty(workOrder.WorkorderErfid))
            {
                this.ErfId = workOrder.WorkorderErfid.ToString();
            }


            TimeZoneInfo newTimeZoneInfo = null;
            //this.CustomerTimeZone = Utility.GetCustomerTimeZone(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            this.CurrentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites).ToString("hh:mm tt");

            ///PopulatePastContacts(FarmerBrothersEntitites);

            using (NewcoEntity entities = new NewcoEntity())
            {
                States = Utility.GetStates(entities);
            }
        }

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
        public string PaymentTransactionId { get; set; }
        public int ServiceType { get; set; }
        public string Comments { get; set; }
        public string ServiceRequestedPartyName { get; set; }
        public string ServiceRequestedPartyPhone { get; set; }
        public string EqpBrand { get; set; }
        public string EqpModel { get; set; }
        public DateTime DateNeeded { get; set; }

        public void CreateUnknownCustomer(CustomerModel CustModel, NewcoEntity FBE)
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
            customer.PricingParentID = CustModel.ParentNumber;
            DateTime CurrentTime = Utility.GetCurrentTime(CustModel.ZipCode, FBE);

            customer.DateCreated = CurrentTime;
            customer.IsUnknownUser = 1;

            NonFBCustomer nonFBCustomer = FBE.NonFBCustomers.Where(n => n.NonFBCustomerId == CustModel.ParentNumber).FirstOrDefault();
            customer.IsNonFbCustomer = nonFBCustomer == null ? false : true;

            IndexCounter counter = Utility.GetIndexCounter("UnknownCustomerID", 1);
            int id = Convert.ToInt32(counter.IndexValue++);
            customer.ContactID = id;
            CustModel.CustomerId = id.ToString();
            FBE.Contacts.Add(customer);
            try
            {
                FBE.SaveChanges();
            }
            catch (Exception e)
            {
                //Utility.LogError(e);
            }


        }

        

        public static string GetServiceTier(string serviceLevelCode)
        {
            string StrTierDesc = string.Empty;
            switch (serviceLevelCode)
            {
                case "001":
                    StrTierDesc = "Tier:001  ";
                    break;
                case "002":
                    StrTierDesc = "Tier:002  ";
                    break;
                case "003":
                    StrTierDesc = "Tier:003  ";
                    break;
                case "004":
                    StrTierDesc = "Tier:004  ";
                    break;
                case "005":
                case "0S5":
                case "NA1":
                case "NA2":
                case "NA3":
                case "NA4":
                case "NA5":
                case "NA6":
                case "NS5":
                case "NSW":
                    StrTierDesc = "Tier:005  ";

                    break;
                default:
                    StrTierDesc = "Tier:003  ";
                    break;
            }
            return StrTierDesc;
        }

        public static int GetCallsTotalCount(NewcoEntity FBE, string CustomerID)
        {
            DateTime Last12Months = DateTime.Now.AddMonths(-12);
            return FBE.Set<WorkOrder>().Where(w => w.CustomerID.ToString() == CustomerID && w.WorkorderCalltypeid == 1200 && w.WorkorderEntryDate >= Last12Months).Count();
        }

        public static string IsBillableService(string BillingCode, int TotalCallCount)
        {
            string flag = "False";

            //string BillingDesc = FBE.FbBillableFeed.Where(b => b.Code == BillingCode).Select(d => d.Description).ToString();

            switch (BillingCode)
            {
                case "S00":
                    flag = "False";
                    break;
                case "S01":
                    if (TotalCallCount > 2)
                        flag = "True";
                    else
                        flag = "False";
                    break;
                case "S02":
                    if (TotalCallCount > 3)
                        flag = "True";
                    else
                        flag = "False";
                    break;
                case "S03":
                    if (TotalCallCount > 4)
                        flag = "True";
                    else
                        flag = "False";
                    break;
                case "S04":
                    flag = "False";
                    break;
                case "S05":
                    flag = "False";
                    break;
                case "S06":
                    flag = "False";
                    break;
                case "S07":
                    flag = "False";
                    break;
                case "S08":
                    flag = "True";
                    break;
                default:
                    flag = "False";
                    break;
            }

            return flag;
        }

        public static string GetServiceLevelDesc(NewcoEntity FBE, string BillingCode)
        {
            string Description = "";
            FBBillableFeed fbFeed = FBE.FBBillableFeeds.Where(b => b.Code == BillingCode).FirstOrDefault();
            if (fbFeed != null)
                Description = fbFeed.Description;

            return BillingCode + "  -  " + Description;
        }

        public static void InsertCustomerData(List<CustomerModel> customerList, NewcoEntity fileUploadEntity)
        {
            DateTime currentDate = DateTime.Now;

            foreach (CustomerModel customerItem in customerList)
            {
                try
                {
                    int contactId = string.IsNullOrEmpty(customerItem.CustomerId) ? 0 : Convert.ToInt32(customerItem.CustomerId);
                    if (contactId > 0)
                    {
                        Contact contacttem = fileUploadEntity.Contacts.Where(cr => cr.ContactID == contactId).FirstOrDefault();
                        if (contacttem == null)
                        {
                            Contact contact = new Contact();
                            contact.ContactID = contactId;
                            contact.CompanyName = customerItem.CustomerName;
                            contact.Address1 = customerItem.Address;
                            contact.Address2 = customerItem.Address2;
                            contact.Address3 = customerItem.Address3;
                            contact.City = customerItem.City;
                            contact.State = customerItem.State;
                            contact.PostalCode = customerItem.ZipCode;
                            contact.Phone = customerItem.PhoneNumber;
                            /*contact.BusinessUnit = customerItem.BusinessUnit;
                            contact.PricingParentID = customerItem.PricingParentDesc;
                            contact.LastSaleDate = customerItem.LastSaleDate;*/
                            contact.Route = customerItem.Route;
                            contact.Branch = customerItem.Branch;
                            contact.RouteCode = customerItem.RouteCode;
                            /*contact.ZoneNumber = customerItem.ZoneNumber;
                            contact.Division = customerItem.Division;
                            contact.District = customerItem.District;
                            contact.CustomerRegion = customerItem.CustomerRegion;
                            contact.CustomerBranch = customerItem.CustomerBranch;
                            contact.BillingCode = customerItem.BillingCode;*/
                            contact.DateCreated = currentDate;
                            contact.LastModified = currentDate;
                            contact.SearchType = "C";

                            contact.CustomerBranch = customerItem.Branch;

                            fileUploadEntity.Contacts.Add(contact);

                        }
                        else
                        {
                            contacttem.ContactID = contactId;
                            contacttem.CompanyName = customerItem.CustomerName;
                            contacttem.Address1 = customerItem.Address;
                            contacttem.Address2 = customerItem.Address2;
                            contacttem.Address3 = customerItem.Address3;
                            contacttem.City = customerItem.City;
                            contacttem.State = customerItem.State;
                            contacttem.PostalCode = customerItem.ZipCode;
                            contacttem.Phone = customerItem.PhoneNumber;
                            /*contacttem.BusinessUnit = customerItem.BusinessUnit;
                            contacttem.PricingParentID = customerItem.PricingParentDesc;
                            contacttem.LastSaleDate = customerItem.LastSaleDate;*/
                            contacttem.Route = customerItem.Route;
                            contacttem.Branch = customerItem.Branch;
                            contacttem.RouteCode = customerItem.RouteCode;
                            /*contacttem.ZoneNumber = customerItem.ZoneNumber;
                            contacttem.Division = customerItem.Division;
                            contacttem.District = customerItem.District;
                            contacttem.CustomerRegion = customerItem.CustomerRegion;
                            contacttem.CustomerBranch = customerItem.CustomerBranch;
                            contacttem.BillingCode = customerItem.BillingCode;*/
                            contacttem.LastModified = currentDate;
                            contacttem.SearchType = "C";

                            contacttem.CustomerBranch = customerItem.Branch;
                        }

                        fileUploadEntity.SaveChanges();

                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }
    }
}