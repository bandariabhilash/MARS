using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Linq;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class ReopenWorkOrderController : BaseController
    {
        // GET: ReopenWorkOrder
        public ActionResult ReopenWorkOrder()
        {
            return View();
        }

        public PartialViewResult WorkOrder(int id)
        {
            try
            {
                var workOrder = FarmerBrothersEntitites.WorkOrders.First(x => x.WorkorderID == id && x.WorkorderCallstatus != "Open");
                WorkOrderModel workOderModel = new WorkOrderModel() { CustomerID = workOrder.CustomerID, CustomerName = workOrder.CustomerName, CustomerState = workOrder.CustomerState, Notes = "" };
                return PartialView("WorkOrder", workOderModel);
            }
            catch (Exception)
            {
                return PartialView("WorkOrder", null);
            }
        }

        [HttpPost]
        public int ReOpenWorkOrder(int id, string notes)
        {
            try
            {
                var workOrder = FarmerBrothersEntitites.WorkOrders.First(x => x.WorkorderID == id);
                string oldworkOrderCallSatus = workOrder.WorkorderCallstatus;
                workOrder.WorkorderCallstatus = WorkOrderStatus.Open.ToString();
                workOrder.ClosedUserName = null;
                workOrder.WorkorderCloseUserid = null;
                workOrder.WorkorderCloseDate = null;
                workOrder.WorkorderClosureConfirmationNo = null;
                workOrder.NoServiceRequired = null;
                if (workOrder.WorkorderDetails != null)
                {
                    if (workOrder.WorkorderDetails.Count > 0)
                    {
                        WorkorderDetail workorderDetail = workOrder.WorkorderDetails.ElementAt(0);
                        if (workorderDetail != null)
                        {
                            workorderDetail.StartDateTime = null;
                            workorderDetail.InvoiceNo = null;
                            workorderDetail.ArrivalDateTime = null;
                            workorderDetail.CompletionDateTime = null;
                            workorderDetail.SpecialClosure = string.Empty;
                            workorderDetail.PhoneSolveid = null;
                        }
                    }
                }

                if (workOrder.WorkorderSchedules != null)
                {
                    foreach (var schedule in workOrder.WorkorderSchedules.ToList())
                    {
                        FarmerBrothersEntitites.WorkorderSchedules.Remove(schedule);
                    }
                }

                if (workOrder.WorkorderEquipments != null)
                {
                    foreach (var equipments in workOrder.WorkorderEquipments.ToList())
                    {
                        FarmerBrothersEntitites.WorkorderEquipments.Remove(equipments);
                    }
                }

                if (workOrder.WorkorderCalltypeid == 1600)
                {
                    if (workOrder.WorkorderInstallationSurveys != null)
                    {
                        foreach (var equipments in workOrder.WorkorderInstallationSurveys.ToList())
                        {
                            FarmerBrothersEntitites.WorkorderInstallationSurveys.Remove(equipments);
                        }
                    }
                }

                DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                NotesHistory noteHistory = new NotesHistory();
                noteHistory.WorkorderID = id;                
                noteHistory.Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234; 
                noteHistory.UserName = UserName;
                noteHistory.Notes = notes;
                noteHistory.AutomaticNotes = 0;
                noteHistory.EntryDate = currentTime;
                noteHistory.isDispatchNotes = 1;
                FarmerBrothersEntitites.NotesHistories.Add(noteHistory);



                NotesHistory noteHistory1 = new NotesHistory();
                noteHistory1.WorkorderID = id;
                noteHistory.Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234; 
                noteHistory1.UserName = UserName;
                noteHistory1.Notes = "Moved from " + oldworkOrderCallSatus + " to Open (Reopen)";
                noteHistory1.AutomaticNotes = 1;
                noteHistory1.EntryDate = currentTime;
                noteHistory1.isDispatchNotes = 1;
                FarmerBrothersEntitites.NotesHistories.Add(noteHistory1);

                WorkorderController wc = new WorkorderController();
                wc.UpdateWOModifiedElapsedTime(workOrder.WorkorderID);

                FarmerBrothersEntitites.SaveChanges();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public enum WorkOrderStatus
        {
            Accepted,
            Open
        }

    }
}