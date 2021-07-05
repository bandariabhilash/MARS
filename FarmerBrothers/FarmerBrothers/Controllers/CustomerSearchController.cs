using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using LinqKit;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Customer = FarmerBrothers.Data.Contact;


namespace FarmerBrothers.Controllers
{
    public class CustomerSearchController : BaseController
    {
        private IList<Customer> GetCustomers(CustomerSearchModel customerSearchModel)
        {
            int[] NonApprovedCustomers = { 9269383, 9268818, 9269065, 9268993, 9269325, 9271062, 9269348, 9268755, 9268750, 9268991, 9271344, 9268932, 9268994, 9268925, 9269278, 9262495, 9269400, 9269070, 9268800, 9258432, 9269295, 9269131, 9258878, 9258879, 9269284, 9269298, 9269140, 9269279, 9268902, 9268854, 9268821, 9268746, 9268792, 9269158, 9269127, 9269031, 9269061, 9268754, 9269355, 9268990, 9269253, 9268758, 9269283, 9269391, 9268751, 9268791, 9268983, 9268989, 9252241, 9269062, 9268810, 9293048, 9258690, 9268790, 9269033, 9269506, 9269157, 9268793, 9269287, 9269337, 9268956, 9269240, 9269246, 9270470, 9268815, 9269405, 9270294, 9268745, 9268744, 9269014, 9269159, 9268816, 9269182, 9269254, 9268756, 9269053, 9268924, 9270521, 9270061, 9269017, 9269055, 9268644, 9268999, 9268611, 9268594, 9268723, 9268716, 9268996, 9268722, 9268987, 9268982, 9268589, 9269191, 9269046, 9253351, 9269309, 9269190, 9269037, 9268626, 9268940, 9269322, 9268936, 9269303, 9268624, 9268859, 9268799, 9268992, 9269244, 9268911, 9268898, 9268896, 9269038, 9269282, 9268980, 9268781, 9269320, 9269323 };
            List<int> NonApprovedCustomersList = NonApprovedCustomers.ToList();

            IList<Customer> customers = new List<Customer>();
            if (customerSearchModel != null)
            {
                try
                {
                    if (customerSearchModel != null)
                    {
                        try
                        {
                            var predicate = PredicateBuilder.True<Contact>();


                            if (!string.IsNullOrWhiteSpace(customerSearchModel.LongAddressNumber))
                            {
                                predicate = predicate.And(w => w.LongAddressNumber.ToString().Contains(customerSearchModel.LongAddressNumber));
                            }

                            //predicate = predicate.And(e => e.SearchType.ToString().Equals("C") || e.SearchType.ToString().Equals("CA") 
                            //      || e.SearchType.ToString().Equals("XC") || e.SearchType.ToString().Equals("XCA") || e.SearchType.ToString().Equals("XCI") || e.SearchType.ToString().Equals("CCS")
                            //      || e.SearchType.ToString().Equals("CFS") || e.SearchType.ToString().Equals("CB") || e.SearchType.ToString().Equals("CE") || e.SearchType.ToString().Equals("CFD") || 
                            //      e.SearchType.ToString().Equals("PFS"));
                            predicate = predicate.And(e => e.SearchType.ToString().Equals("C") || e.SearchType.ToString().Equals("CA") || e.SearchType.ToString().Equals("CFD") || 
                                   e.SearchType.ToString().Equals("CB") || e.SearchType.ToString().Equals("CE")  || e.SearchType.ToString().Equals("PFS"));

                            if (customerSearchModel.ZipCode > 0 && !string.IsNullOrWhiteSpace(customerSearchModel.ZipCode.ToString()))
                            {
                                predicate = predicate.And(e => e.PostalCode.ToString().Contains(customerSearchModel.ZipCode.ToString()));
                            }

                            if (customerSearchModel.CustomerID > 0 && !string.IsNullOrWhiteSpace(customerSearchModel.CustomerID.ToString()))
                            {
                                predicate = predicate.And(e => e.ContactID.ToString().Contains(customerSearchModel.CustomerID.ToString()));
                            }

                            if (!string.IsNullOrWhiteSpace(customerSearchModel.CustomerName))
                            {
                                predicate = predicate.And(w => w.CompanyName.ToString().Contains(customerSearchModel.CustomerName));
                            }

                            if (!string.IsNullOrWhiteSpace(customerSearchModel.PhoneWithAreaCode))
                            {
                                predicate = predicate.And(w => w.PhoneWithAreaCode.ToString().Contains(customerSearchModel.PhoneWithAreaCode));
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

                            if (!string.IsNullOrWhiteSpace(customerSearchModel.ParentId))
                            {
                                predicate = predicate.And(w => w.PricingParentID.ToString().Contains(customerSearchModel.ParentId));
                            }                            
                            //predicate = predicate.And(c => c.IsUnknownUser != 1);

                            IQueryable<Contact> contacts = FarmerBrothersEntitites.Set<Contact>().AsExpandable().Where(predicate).Take(500); 

                            foreach (Customer customer in contacts)
                            {
                                //Block to avoid NonFbCustomers in the Customer Search Results
                                Dictionary<string, string> UserPrivilege = (Dictionary<string, string>)System.Web.HttpContext.Current.Session["UserPrivilege" + (int)System.Web.HttpContext.Current.Session["UserId"]] == null
                               ? Security.GetUserPrivilegeByUserId((int)System.Web.HttpContext.Current.Session["UserId"], null) :
                               (Dictionary<string, string>)System.Web.HttpContext.Current.Session["UserPrivilege" + (int)System.Web.HttpContext.Current.Session["UserId"]];

                                string IsNonFBCustomerParentId = ConfigurationManager.AppSettings["NonFBCustomerParentID"];
                                if (customer.PricingParentID == IsNonFBCustomerParentId && UserPrivilege["NonFBCustomer"] != "Full") continue;

                                string phoneNumber = string.Empty;
                                if (!string.IsNullOrWhiteSpace(customer.Phone))
                                {
                                    customer.Phone = Utilities.Utility.FormatPhoneNumber(customer.PhoneWithAreaCode);                                  
                                }

                                if (string.IsNullOrWhiteSpace(customer.DaysSinceLastSale))
                                {
                                    CustomerModel cm = new CustomerModel();
                                    string CurrentTime = Utility.GetCurrentTime(customer.PostalCode, FarmerBrothersEntitites).ToString("hh:mm tt");
                                    int lastSaleDt = cm.ConvertToDays(CurrentTime, customer.LastSaleDate);
                                    customer.DaysSinceLastSale = lastSaleDt == 0 ? "" : lastSaleDt.ToString();
                                }


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
                catch (Exception e)
                {
                    //Need to log this exception
                }


            }

            return customers;
        }

        [HttpPost]
        public JsonResult Search(CustomerSearchModel customerSearchModel)
        {
            if (string.IsNullOrWhiteSpace(customerSearchModel.Address)
                && string.IsNullOrWhiteSpace(customerSearchModel.City)
                && customerSearchModel.CustomerID <= 0
                && string.IsNullOrWhiteSpace(customerSearchModel.CustomerName)
                && (string.IsNullOrWhiteSpace(customerSearchModel.State) || string.Compare(customerSearchModel.State, "n/a", true) == 0)
                && string.IsNullOrWhiteSpace(customerSearchModel.PhoneWithAreaCode)
                && string.IsNullOrWhiteSpace(customerSearchModel.ParentId)
                && string.IsNullOrWhiteSpace(customerSearchModel.ZipCode.ToString()))
            {
                TempData["SearchCriteria"] = null;
                return Json(new List<Customer>(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                IList<Customer> customers = GetCustomers(customerSearchModel);

                TempData["SearchCriteria"] = customerSearchModel;

               
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CustomerSearch(int? isBack)
        {            
            CustomerSearchModel customerSearchModel = new CustomerSearchModel();
            if (TempData["SearchCriteria"] != null && isBack == 1)
            {
                customerSearchModel = TempData["SearchCriteria"] as CustomerSearchModel;
                TempData["SearchCriteria"] = customerSearchModel;
            }
            else
            {
                customerSearchModel = new CustomerSearchModel();
                TempData["SearchCriteria"] = null;
            }

            customerSearchModel.CustomerSearchResults = new List<Contact>();
            return View(customerSearchModel);
        }

        public JsonResult ClearSearchResults()
        {
            TempData["SearchCriteria"] = null;
            return Json(new List<Customer>(), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public void ExcelExport()
        {
            CustomerSearchModel customerSearchModel = new CustomerSearchModel();
            IList<Contact> customers = new List<Contact>();
            if (TempData["SearchCriteria"] != null)
            {
                customerSearchModel = TempData["SearchCriteria"] as CustomerSearchModel;
                customers = GetCustomers(customerSearchModel);
            }

            TempData["SearchCriteria"] = customerSearchModel;
            string gridModel = HttpContext.Request.Params["GridModel"];

            ExcelExport exp = new ExcelExport();
            GridProperties properties = (GridProperties)Syncfusion.JavaScript.Utils.DeserializeToModel(typeof(GridProperties), gridModel);
            exp.Export(properties, customers, "Customers.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }
    }
}
