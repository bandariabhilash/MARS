using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FarmerBrothers.Data;
using FarmerBrothers.Utilities;

namespace FarmerBrothers.Controllers
{
    public class WorkOrderInvoiceUpdateController : BaseController
    {
        [HttpGet]
        public ActionResult WorkOrderInvoiceUpdate()
        {
            WorkorderInvoiceModel workorderModel = new WorkorderInvoiceModel();
            return View(workorderModel);
        }
        [HttpGet]
        public JsonResult GetWorkOrderInvoice(int workOrderId)
        {
            WorkorderInvoiceModel workorderModel = new WorkorderInvoiceModel();
            JsonResult jsonResult = new JsonResult();
            try
            {
                //WorkorderDetail woDetails = FarmerBrothersEntitites.WorkorderDetails.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();

                var woDetails = (from p in FarmerBrothersEntitites.WorkOrders
                                 join e in FarmerBrothersEntitites.WorkorderDetails
                                 on p.WorkorderID equals e.WorkorderID into t
                                 from wd in t.DefaultIfEmpty()
                                 where p.WorkorderID == workOrderId && (p.WorkorderCallstatus.ToUpper() == "ACCEPTED"
                                 || p.WorkorderCallstatus.ToUpper() == "ON SITE"
                                 || p.WorkorderCallstatus.ToUpper() == "COMPLETED"
                               || p.WorkorderCallstatus.ToUpper() == "CLOSED")
                                 select new
                                 {
                                     InvoiceNo = wd.InvoiceNo,
                                     WorkorderID = p.WorkorderID
                                 }).FirstOrDefault();


                if (woDetails != null)
                {
                    jsonResult.Data = new { success = true, data = woDetails.InvoiceNo, message = "" };
                }
                else
                {
                    jsonResult.Data = new { success = true, message = "|Please Enter Valid WorkOrder Id" };
                }

            }
            catch (Exception)
            {

                jsonResult.Data = new { success = false };
            }


            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveWorkOrderInvoice(int workOrderId, string invoiceNumber)
        {
            WorkorderInvoiceModel workorderModel = new WorkorderInvoiceModel();
            JsonResult jsonResult = new JsonResult();
            try
            {
                string zipcode = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).Select(w => w.CustomerZipCode).FirstOrDefault();
                DateTime currentTime = Utility.GetCurrentTime(zipcode, FarmerBrothersEntitites);
                if (!string.IsNullOrEmpty(zipcode))
                {
                    using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                    {
                        WorkorderDetail woDetails = entity.WorkorderDetails.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
                        string tmpinvoiceNo = " ";
                        if (woDetails != null)
                        {
                            tmpinvoiceNo = woDetails.InvoiceNo == null ? " " : woDetails.InvoiceNo;
                        }
                       
                        NotesHistory notesHistory = new NotesHistory()
                        {
                            WorkorderID = workOrderId,
                            AutomaticNotes = 1,
                            EntryDate = currentTime,
                            Notes = "Work order invoice number modified from " + tmpinvoiceNo + " to " + invoiceNumber,
                            Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                            UserName = UserName,
                            isDispatchNotes = 1
                        };


                        if (woDetails != null)
                        {

                            woDetails.InvoiceNo = invoiceNumber;
                            entity.Entry(woDetails).State = System.Data.Entity.EntityState.Modified;
                            entity.Entry(notesHistory).State = System.Data.Entity.EntityState.Added;
                            entity.NotesHistories.Add(notesHistory);
                            entity.SaveChanges();
                            WorkorderController wc = new WorkorderController();
                            wc.UpdateWOModifiedElapsedTime(workOrderId);
                            jsonResult.Data = new { success = true, message = "" };
                        }
                        else
                        {
                            WorkorderDetail details = new WorkorderDetail();
                            details.WorkorderID = workOrderId;
                            details.InvoiceNo = invoiceNumber;
                            entity.Entry(details).State = System.Data.Entity.EntityState.Added;
                            entity.Entry(notesHistory).State = System.Data.Entity.EntityState.Added;
                            entity.NotesHistories.Add(notesHistory);
                            entity.WorkorderDetails.Add(details);
                            entity.SaveChanges();
                            WorkorderController wc = new WorkorderController();
                            wc.UpdateWOModifiedElapsedTime(workOrderId);
                            jsonResult.Data = new { success = true, message = "" };
                        }

                    }
                }
                else
                {
                    jsonResult.Data = new { success = true, message = "|Please Entered Valid WorkOrder Id" };
                }
                

            }
            catch (Exception)
            {

                jsonResult.Data = new { success = false };
            }


            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

    }
}