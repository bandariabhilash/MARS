using FarmerBrothers.Data;
using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using FarmerBrothersMailResponse.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace FarmerBrothers.Controllers
{
    public class ScheduleEventController : Controller
    {
        protected FarmerBrothersEntities ScheduleEventEntitites;

        public ScheduleEventController()
        {
            ScheduleEventEntitites = new FarmerBrothersEntities();
        }


        [AllowAnonymous]
        public ActionResult ScheduleEvent(int customerId, int workOrderId, int techId)
        {
            ScheduleEventModel scheduleEvtModel = new ScheduleEventModel();
            scheduleEvtModel.WorkorderID = workOrderId;
            scheduleEvtModel.TechID = techId;
            scheduleEvtModel.CustomerID = customerId;
            //scheduleEvtModel.CustomerName = GetCustomerById(customerId).CompanyName;
            scheduleEvtModel.RescheduleReasonCodesList = ScheduleEventEntitites.AllFBStatus.Where(p => p.StatusFor == "ReScheduleReasonCode" && p.Active == 1).OrderBy(p => p.StatusSequence).ToList();

            return View(scheduleEvtModel);
        }

        [AllowAnonymous]
        public JsonResult ScheduleWorkorder(int workOrderId, int techId, string CustomerID, string CustomerName, DateTime ScheduleDate, string Notes, int ReasonCode)
        {
            string responseMessage = "";
            JsonResult jsonResult = new JsonResult();
            try
            {
                WorkOrder workOrder = ScheduleEventEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
                DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, ScheduleEventEntitites);
                TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(ScheduleEventEntitites, techId);
                WorkorderSchedule workOrderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.Techid == techId).FirstOrDefault();

                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(ScheduleDate, cstZone);

                string tmpUserName = string.Empty;
                if (workOrder != null)
                {
                    if (workOrder.WorkorderCallstatus.ToLower() == "pending acceptance")
                    {
                        AcceptWorkOrder(workOrderId, techId, true);
                    }

                    System.Threading.Thread.Sleep(2000);

                    string currentStatus = workOrder.WorkorderCallstatus;
                    if (workOrderSchedule != null)
                    {
                        if (techHierarchyView != null)
                        {
                            workOrderSchedule.Techid = Convert.ToInt32(techHierarchyView.TechID);
                            workOrderSchedule.TechName = techHierarchyView.PreferredProvider;
                            workOrderSchedule.AssignedStatus = "Scheduled";
                            workOrderSchedule.EventScheduleDate = ScheduleDate;
                            workOrderSchedule.ScheduleContactName = CustomerName;

                            tmpUserName = workOrderSchedule.TechName;
                        }
                    }
                    else
                    {
                        if (techHierarchyView != null)
                        {
                            IndexCounter counter = Utility.GetIndexCounter("ScheduleID", 1);
                            counter.IndexValue++;
                            //ScheduleEventEntitites.Entry(counter).State = System.Data.Entity.EntityState.Modified;

                            WorkorderSchedule newworkOrderSchedule = new WorkorderSchedule()
                            {
                                Scheduleid = counter.IndexValue.Value,
                                Techid = Convert.ToInt32(techHierarchyView.TechID),
                                TechName = techHierarchyView.PreferredProvider,
                                WorkorderID = workOrder.WorkorderID,
                                TechPhone = techHierarchyView.AreaCode + techHierarchyView.ProviderPhone,
                                ServiceCenterName = techHierarchyView.BranchName,
                                ServiceCenterID = Convert.ToInt32(techHierarchyView.TechID),
                                FSMName = techHierarchyView.DSMName,
                                FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>(),
                                EntryDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, ScheduleEventEntitites),
                                ScheduleDate = currentTime,
                                TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"],
                                PrimaryTech = 1,
                                AssignedStatus = "Scheduled",
                                EventScheduleDate = ScheduleDate,
                                ScheduleContactName = CustomerName
                            };

                            tmpUserName = techHierarchyView.PreferredProvider;
                            workOrder.WorkorderSchedules.Add(newworkOrderSchedule);
                        }
                    }

                    workOrder.WorkorderCallstatus = "Scheduled";
                    workOrder.WorkorderModifiedDate = currentTime;
                    workOrder.RescheduleReasonCode = ReasonCode;

                    AllFBStatu ReasonRec = ScheduleEventEntitites.AllFBStatus.Where(p => p.FBStatusID == ReasonCode).FirstOrDefault();
                    string ReasonDesc = "";
                    if (ReasonRec != null)
                    {
                        ReasonDesc = ReasonRec.FBStatus;
                    }

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = "[Schedule Event]:  " + Notes,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = tmpUserName,
                        isDispatchNotes = 1
                    };

                    workOrder.NotesHistories.Add(notesHistory);

                    NotesHistory autoNotes = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = techHierarchyView.PreferredProvider + " Scheduled Event to " + ScheduleDate.ToString("MM/dd/yyyy hh:mm:ss") + ",  Reason : " + ReasonDesc,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = tmpUserName
                    };

                    workOrder.NotesHistories.Add(autoNotes);

                    WorkorderStatusLog statusLog = new WorkorderStatusLog() { StatusFrom = currentStatus, StatusTo = workOrder.WorkorderCallstatus, StausChangeDate = currentTime, WorkorderID = workOrder.WorkorderID };
                    workOrder.WorkorderStatusLogs.Add(statusLog);

                    ScheduleEventEntitites.SaveChanges();

                }

            }
            catch (Exception ex)
            {
                responseMessage = "Error Scheduling the Work order";
                jsonResult.Data = new { success = false, serverError = ErrorCode.ERROR, data = true, techId = techId, workorderid = workOrderId, message = responseMessage };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }

            responseMessage = "Work order Scheduled to " + ScheduleDate.ToString("MM/dd/yyyy hh:mm:ss");
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = true, techId = techId, workorderid = workOrderId, message = responseMessage };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public bool AcceptWorkOrder(int workOrderId, int techId, bool isResponsible)
        {
            string message = string.Empty;
            bool result = true;
            TechHierarchyView techHierarchyView = DispatchResponseController.GetTechDataByResponsibleTechId(ScheduleEventEntitites, techId);
            WorkOrder workOrder = ScheduleEventEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            FarmerBrothers.Data.DispatchResponseModel dispatchModel = new FarmerBrothers.Data.DispatchResponseModel();
            dispatchModel.TechId = techId;
            dispatchModel.WorkOrderId = workOrderId;
            bool updatedStatus = false;

            DispatchResponseController dc = new DispatchResponseController();

            if (workOrder != null)
            {
                updatedStatus = dc.UpdateTechAssignedStatus(techId, workOrder, "Accepted", false);
                if (updatedStatus == true)
                {
                    dc.CopyWorkorderEquipments(workOrder);
                    workOrder.IsRejected = false;

                    if (workOrder.WorkorderDetails != null && workOrder.WorkorderDetails.Count > 0)
                    {
                        WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                        if (details != null)
                        {
                            details.ResponsibleTechName = techHierarchyView.PreferredProvider;
                            TECH_HIERARCHY technicianAddress = dc.GetCustomerByTechId(techId.ToString());
                            string fromAddress = workOrder.CustomerAddress + " " + workOrder.CustomerCity + " " + workOrder.CustomerState + " " + workOrder.CustomerZipCode;
                            string toAddress = string.Empty;
                            if (!string.IsNullOrEmpty(technicianAddress.Address1))
                            {
                                toAddress += technicianAddress.Address1 + " ";
                            }
                            if (!string.IsNullOrEmpty(technicianAddress.Address2))
                            {
                                toAddress += technicianAddress.Address3 + " ";
                            }
                            if (!string.IsNullOrEmpty(technicianAddress.Address3))
                            {
                                toAddress += technicianAddress.Address3 + " ";
                            }

                            if (!string.IsNullOrEmpty(technicianAddress.City))
                            {
                                toAddress += technicianAddress.City + " ";
                            }
                            if (!string.IsNullOrEmpty(technicianAddress.State))
                            {
                                toAddress += technicianAddress.State + " ";
                            }
                            if (!string.IsNullOrEmpty(technicianAddress.PostalCode))
                            {
                                toAddress += technicianAddress.PostalCode + " ";
                            }

                            details.Mileage = DispatchResponseController.GetDistance(fromAddress, toAddress);
                        }
                    }

                    if (ScheduleEventEntitites.SaveChanges() > 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
                
            }
            else
            {                
                result = false;
            }
          
            
            return result;
        }
    }
}