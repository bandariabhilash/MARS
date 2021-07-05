using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FarmerBrothers.Data;
using LinqKit;
using FarmerBrothers.Utilities;

namespace FarmerBrothers.Business
{
    public class WOCustomerUpdate
    {
        public static WorkorderSearchResultModel GetWorkOrderDetails(int workOrderId,out bool result)
        {
            result = false;
            WorkorderSearchResultModel WorkOrders = new WorkorderSearchResultModel();

            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                WorkOrder wo = FarmerBrothersEntitites.WorkOrders.Where(c => c.WorkorderID == workOrderId).FirstOrDefault();
                if (wo !=null)
                {
                    WorkOrders.WorkorderID = wo.WorkorderID;
                    WorkOrders.CustomerID = wo.CustomerID;
                    WorkOrders.CustomerName = wo.CustomerName;
                    WorkOrders.Address1 = wo.CustomerAddress;
                    WorkOrders.CustomerCity = wo.CustomerCity;
                    WorkOrders.CustomerState = wo.CustomerState;
                    WorkOrders.CustomerZipCode = wo.CustomerZipCode;
                    WorkOrders.CustomerMainContactName = wo.CustomerMainContactName;
                    WorkOrders.CustomerPhone = wo.CustomerPhone;
                    WorkOrders.CustomerMainEmail = wo.CustomerMainEmail;
                    result = true;
                }
               

            }
            return WorkOrders;
        }

        public static WorkorderSearchResultModel CustomerDetails(string customerID)
        {
            WorkorderSearchResultModel customerModel = new WorkorderSearchResultModel();
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                Contact custDetails = new Contact();
                if (!string.IsNullOrEmpty(customerID))
                {
                    int id = Convert.ToInt32(customerID);
                    custDetails = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == id).FirstOrDefault();
                }
                if (custDetails != null)
                {
                    customerModel.CustomerID = custDetails.ContactID;
                    customerModel.CustomerName = custDetails.CompanyName;
                    customerModel.Address1 = custDetails.Address1;
                    customerModel.CustomerCity = custDetails.City;
                    customerModel.CustomerState = custDetails.State;
                    customerModel.CustomerZipCode = custDetails.PostalCode;
                    customerModel.CustomerMainContactName = custDetails.FirstName + " " + custDetails.LastName;
                    customerModel.CustomerPhone = custDetails.Phone;
                    customerModel.CustomerMainEmail = custDetails.Email;
                }
            }
            return customerModel;
        }

        public static WorkorderSearchResultModel WoCustomerUpdate(int? customerId, int? workOrderId, string userName, out bool success)
        {
            WorkorderSearchResultModel customerModel = new WorkorderSearchResultModel();
            success = false;
            int? oldCustomerid;
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                var wkrorder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
                if (wkrorder != null)
                {
                  
                    int custId = Convert.ToInt32(customerId);
                    Contact custDetails = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == custId).FirstOrDefault();
                    oldCustomerid = wkrorder.CustomerID;                  

                    DateTime currentTime = Utility.GetCurrentTime(custDetails.PostalCode, FarmerBrothersEntitites);
                    wkrorder.CustomerID = custDetails.ContactID;
                    wkrorder.CustomerName = custDetails.CompanyName;
                    wkrorder.CustomerAddress = custDetails.Address1;
                    wkrorder.CustomerCity = custDetails.City;
                    wkrorder.CustomerState = custDetails.State;
                    wkrorder.CustomerZipCode = custDetails.PostalCode;
                    wkrorder.CustomerMainContactName = custDetails.FirstName + " " + custDetails.LastName;
                    wkrorder.CustomerPhone = custDetails.Phone;
                    wkrorder.CustomerMainEmail = custDetails.Email;
                    wkrorder.WorkorderModifiedDate = currentTime;
                    wkrorder.WorkorderModifiedUserid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234;
                    FarmerBrothersEntitites.SaveChanges();

                    

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = @"Work Order Customer ID updated from " + oldCustomerid + " to " + customerId + " from MARS by " + userName,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234, //TBD
                        UserName = userName
                    };
                    wkrorder.NotesHistories.Add(notesHistory);
                    FarmerBrothersEntitites.SaveChanges();
                    
                    using (FarmerBrothersEntities FarmerBrothersEntitites1 = new FarmerBrothersEntities())
                    {
                        Contact contact1 = FarmerBrothersEntitites1.Contacts.Where(c => c.ContactID == oldCustomerid && c.IsUnknownUser == 1).FirstOrDefault();
                        WorkOrder wo = FarmerBrothersEntitites1.WorkOrders.Where(c => c.CustomerID == oldCustomerid).FirstOrDefault();
                        if (contact1 != null && wo == null)
                        {
                            contact1.SearchType = "CI";
                            FarmerBrothersEntitites1.Contacts.Add(contact1);
                            FarmerBrothersEntitites1.Entry(contact1).State = System.Data.Entity.EntityState.Modified;
                            FarmerBrothersEntitites1.SaveChanges();
                        }
                    }

                    success = true;

                }
                else
                {
                    return customerModel;
                }

            }
            return customerModel;
        }
    }
}
