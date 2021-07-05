using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FarmerBrothers.Data;
using FarmerBrothers.Models;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using FarmerBrothers.Utilities;

namespace FarmerBrothers.Controllers
{
    public class CustomerNotesController : BaseController
    {
        public ActionResult CustomerNotes()
        {
            CustomerNotesModel objCustomerNotes = new CustomerNotesModel();
            List<CustomerNotes> CustomerNoteslist = new List<CustomerNotes>();

            objCustomerNotes.CustomerNotesResults = CustomerNoteslist;
            return View(objCustomerNotes);
        }

        [HttpPost]
        public JsonResult InsertCustomerNotesList([System.Web.Http.FromBody]CustomerNotesModel custNotesModel)
        {
            string message = string.Empty;
            List<CustomerNotes> CustomerNoteslist = new List<CustomerNotes>();
            try
            {
                if (!IsCustomerExist(custNotesModel.CustomerId))
                {
                    string zipCode = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == custNotesModel.CustomerId).Select(z => z.PostalCode).FirstOrDefault();
                    DateTime currentTime = Utility.GetCurrentTime(zipCode, FarmerBrothersEntitites);

                    if (ModelState.IsValid)
                    {
                        FBCustomerNote custNotes = new FBCustomerNote()
                        {
                            CustomerId = custNotesModel.CustomerId,
                            Notes = custNotesModel.Notes,
                            UserId = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                            UserName = UserName,
                            EntryDate = currentTime,
                            IsActive = custNotesModel.Status,                            
                        };
                        
                        FarmerBrothersEntitites.FBCustomerNotes.Add(custNotes);
                        FarmerBrothersEntitites.SaveChanges();
                        message = "Successfully created Customer Notes!";

                        CustomerNoteslist = (from t in FarmerBrothersEntitites.FBCustomerNotes
                                             where t.CustomerId.Value == custNotesModel.CustomerId
                                             select new CustomerNotes { CustomerNotesId = t.CustomerNotesId, CustomerId = t.CustomerId, Notes = t.Notes, UserId = t.UserId, UserName = t.UserName, EntryDate = t.EntryDate, Status = t.IsActive }).AsEnumerable().ToList()
                   .Select(c => { c.EntryDatestring = c.EntryDate.Value.ToShortDateString(); return c; }).ToList();
                    }
                }
                else
                {
                    message = "Customer Account Number is not valid, Please Enter Valid Account Number!";
                }


            }
            catch (Exception)
            {
                message = "There is a problem in customer notes creation!";
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { notesList = CustomerNoteslist, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public bool IsCustomerExist(int customerId)
        {
            bool isExist = true;

            var customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == customerId).FirstOrDefault();

            if (customer != null)
            {
                isExist = false;
            }

            return isExist;
        }

        public JsonResult UpdateCustomerNotes([System.Web.Http.FromBody]CustomerNotesModel custNotesModel)
        {
            string message = string.Empty;
            List<CustomerNotes> CustomerNoteslist = new List<CustomerNotes>();
            try
            {
                if (!IsCusomterNotesExistToUpdate(custNotesModel.CustomerId, custNotesModel.CustomerNotesId))
                {
                    FBCustomerNote custNotesitem = FarmerBrothersEntitites.FBCustomerNotes.Single(c => c.CustomerNotesId == (custNotesModel.CustomerNotesId));
                    custNotesitem.CustomerId = custNotesModel.CustomerId;                    
                    custNotesitem.IsActive = custNotesModel.Status;
                    custNotesitem.Notes = custNotesModel.Notes;
                    FarmerBrothersEntitites.FBCustomerNotes.Attach(custNotesitem);
                    FarmerBrothersEntitites.Entry(custNotesitem).State = System.Data.Entity.EntityState.Modified;
                    FarmerBrothersEntitites.SaveChanges();
                    message = "Successfully updated Customer Notes!";
                    CustomerNoteslist = (from t in FarmerBrothersEntitites.FBCustomerNotes
                                         where t.CustomerId.Value == custNotesModel.CustomerId
                                         select new CustomerNotes { CustomerNotesId = t.CustomerNotesId, CustomerId = t.CustomerId, Notes = t.Notes, UserId = t.UserId, UserName = t.UserName, EntryDate = t.EntryDate, Status = t.IsActive }).AsEnumerable().ToList()
                    .Select(c => { c.EntryDatestring = c.EntryDate.Value.ToShortDateString(); return c; }).ToList();

                }
                else
                {
                    message = "Customer Account Number is modified, Please do not change Account Number!";
                }
                
            }
            catch (Exception)
            {
                message = "There is a problem in Customer Notes Update!";
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { notesList = CustomerNoteslist, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;



        }
        public JsonResult GetUserNotes([System.Web.Http.FromBody]CustomerNotesModel custNotesModel)
        {
            List<CustomerNotes> CustomerNoteslist = new List<CustomerNotes>();
            string message = string.Empty;

            try
            {
                if (!IsCustomerExist(custNotesModel.CustomerId))
                {
                    CustomerNoteslist = (from t in FarmerBrothersEntitites.FBCustomerNotes
                                         where t.CustomerId.Value == custNotesModel.CustomerId
                                         select new CustomerNotes { CustomerNotesId = t.CustomerNotesId, CustomerId = t.CustomerId, Notes = t.Notes, UserId = t.UserId, UserName = t.UserName, EntryDate = t.EntryDate, Status = t.IsActive }).AsEnumerable().ToList()
                      .Select(c => { c.EntryDatestring = c.EntryDate.Value.ToShortDateString(); return c; }).ToList();
                }
                else
                {
                    message = "Customer Account Number is not valid, Please Enter Valid Account Number!";
                }
            }
            catch (Exception)
            {

                message = "There is a problem in getting Customer Notes!";
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { notesList = CustomerNoteslist, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public bool IsCusomterNotesExistToUpdate(int CustomerId, int UniqueId)
        {
            bool isExist = true;

            var notes = (from custNt in FarmerBrothersEntitites.FBCustomerNotes
                           where custNt.CustomerId == CustomerId && custNt.CustomerNotesId == UniqueId
                           select custNt).FirstOrDefault();

            if (notes != null)
            {
                isExist = false;
            }

            return isExist;
        }

        #region PSP Update
        [HttpGet]
        public JsonResult GetCustomerDetails(int CustomerId)
        {
            string message = string.Empty;
            CustomerNotesModel customerNotesModel = new CustomerNotesModel();
            if (!IsCustomerExist(CustomerId))
            {
                Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == CustomerId).FirstOrDefault();
                
                customerNotesModel.CustomerName = customer.CompanyName;
                customerNotesModel.Address = customer.Address1;
                customerNotesModel.City = customer.City;
                customerNotesModel.ZipCode = customer.PostalCode;
                customerNotesModel.FBPreferProvider = Convert.ToString(customer.FBProviderID);

            }
            else
            {
                message = "Customer Account Number is not valid, Please Enter Valid Account Number!";
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { data = customerNotesModel, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
                
        [HttpGet]
        public JsonResult GetBranchPSP(string Branch)
        {
            string message = string.Empty;
            CustomerNotesModel customerNotesModel = new CustomerNotesModel();
            if (!string.IsNullOrEmpty(Branch))
            {
                Contact customer = null;
                customer = FarmerBrothersEntitites.Contacts.Where(c => c.Branch == Branch && c.FBProviderID != null).FirstOrDefault();
                if(customer == null)
                {
                    customer = FarmerBrothersEntitites.Contacts.Where(c => c.Branch == Branch).FirstOrDefault();
                }

                if (customer != null)
                {
                    customerNotesModel.Branch = customer.Branch;
                    customerNotesModel.FBPreferProvider = customer.FBProviderID == null ? "" : customer.FBProviderID.ToString();
                }
                else
                {
                    customerNotesModel.Branch = Branch;
                    customerNotesModel.FBPreferProvider = "Preferred providerId is Empty for this Branch";
                }

            }
            else
            {
                message = "Customer Account Number is not valid, Please Enter Valid Account Number!";
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { data = customerNotesModel, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpGet]
        public JsonResult UpdateCustomerFBProvider(int CustomerId, string Branch,  int FBProviderId)
        {
            string message = string.Empty;
            CustomerNotesModel customerNotesModel = new CustomerNotesModel();
            if (!IsCustomerExist(CustomerId) || CustomerId != 0)
            {
                try
                {
                    Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == CustomerId).FirstOrDefault();

                    if (FBProviderId == 0)
                    {
                        customer.FBProviderID = null;
                        FarmerBrothersEntitites.SaveChanges();
                    }
                    else
                    {
                        TECH_HIERARCHY tech = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == FBProviderId && t.SearchType == "SP").FirstOrDefault();

                        if (tech != null)
                        {
                            customer.FBProviderID = FBProviderId;
                            FarmerBrothersEntitites.SaveChanges();
                        }
                        else
                        {
                            message = "Preferred Provider Number is not valid, Please Enter Valid Preferred Provider Number!";
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = "Unable to update Preferred Provider Number, Please contact support team!";
                }
            }

            else if(!string.IsNullOrEmpty(Branch))
            {
                try
                {
                    List<Contact> customerList = FarmerBrothersEntitites.Contacts.Where(c => c.Branch == Branch).ToList();

                    foreach (Contact customer in customerList)
                    {
                        if (FBProviderId == 0)
                        {
                            customer.FBProviderID = null;
                            FarmerBrothersEntitites.SaveChanges();
                        }
                        else
                        {
                            TECH_HIERARCHY tech = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == FBProviderId && t.SearchType == "SP").FirstOrDefault();
                            if (tech != null)
                            {
                                customer.FBProviderID = FBProviderId;
                                FarmerBrothersEntitites.SaveChanges();
                            }                           
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = "Unable to update Preferred Provider Number, Please contact support team!";
                }
            }

            else
            {
                message = "Customer Account Number is not valid, Please Enter Valid Account Number!";
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { data = customerNotesModel, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        
        #endregion
    }
}