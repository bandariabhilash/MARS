using FarmerBrothers;
using FarmerBrothers.Controllers;
using FarmerBrothers.Data;
using FarmerBrothers.USZipService;
using FarmerBrothers.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;
using Customer = FarmerBrothers.Data.Contact;

namespace FarmerBrothersMailResponse.Controllers
{
    public class DispatchResponseController : BaseController
    {

        protected FarmerBrothersEntities FarmerBrothersEntitites;
        protected Customer client;

        public DispatchResponseController()
        {
            FarmerBrothersEntitites = new FarmerBrothersEntities();
        }

        private Customer GetCustomerFromService(string customerId)
        {
            int cusId = Convert.ToInt32(customerId);
            Customer customer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == cusId).FirstOrDefault();
            return customer;
        }

        private TECH_HIERARCHY GetCustomerByTechId(string techId)
        {
            int cusId = Convert.ToInt32(techId);
            TECH_HIERARCHY technician = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == cusId).FirstOrDefault();
            return technician;
        }

        private static decimal GetDistance(string fromAddress, string toAddress)
        {
            double distance = 0;
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=");
                stringBuilder.Append(fromAddress);
                stringBuilder.Append("&destinations=");
                stringBuilder.Append(toAddress);
                stringBuilder.Append("&key=AIzaSyCjMfuakjLPeYGF2CLY56lqz40IH9UfxLM");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = client.GetAsync(stringBuilder.ToString()).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        if (jObject != null)
                        {
                            var element = jObject.rows[0].elements[0];
                            distance = element.distance.value * (decimal)0.000621371192;
                            distance = Math.Round(distance, 0);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return System.Convert.ToDecimal(distance);
        }

        private static TechHierarchyView GetTechDataByResponsibleTechId(FarmerBrothersEntities FarmerBrothersEntitites, int responsibleTechId)
        {
            string query = @"SELECT * FROM vw_tech_hierarchy where TechId = " + responsibleTechId.ToString();
            return FarmerBrothersEntitites.Database.SqlQuery<TechHierarchyView>(query).FirstOrDefault();
        }

        private static string GetCustomerTimeZone(string zipCode, out TimeZoneInfo timeZoneInfo)
        {
            string timeZone = "Eastern Standard Time";
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            if (zipCode != null)
            {
                try
                {
                    if (zipCode.Length > 5)
                    {
                        zipCode = zipCode.Substring(0, 5);
                    }

                    USZipSoapClient usZipClient = new USZipSoapClient();
                    XmlNode node = usZipClient.GetInfoByZIP(zipCode);
                    if (node != null)
                    {
                        string timeZoneChar = node.ChildNodes[0].ChildNodes[4].InnerText;

                        timeZone = ConfigurationManager.AppSettings["TimeZone_" + timeZoneChar];
                        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }
            return timeZone;
        }

        private void CopyWorkorderEquipments(WorkOrder workOrder)
        {
            if (workOrder != null)
            {
                //workOrder.WorkorderEquipments.Clear();
                foreach (var requested in workOrder.WorkorderEquipmentRequesteds)
                {
                    WorkorderEquipment equipment = new WorkorderEquipment()
                    {
                        Assetid = requested.Assetid,
                        CallTypeid = requested.CallTypeid,
                        Category = requested.Category,
                        Location = requested.Location,
                        SerialNumber = requested.SerialNumber,
                        CatalogID = requested.CatalogID,
                        Model = requested.Model,
                        Symptomid = requested.Symptomid
                    };

                    if (!isWorkOrderEquipmentExist(requested.Assetid))
                    {
                        workOrder.WorkorderEquipments.Add(equipment);
                    }

                }
            }
        }

        public bool isWorkOrderEquipmentExist(int assetId)
        {
            bool isExist = false;

            List<WorkorderEquipment> workEqu = FarmerBrothersEntitites.WorkorderEquipments.Where(weq => weq.Assetid == assetId).ToList();
            if (workEqu.Count > 0)
            {
                isExist = true;
            }

            return isExist;
        }

        private bool UpdateTechAssignedStatus(int techId, WorkOrder workOrder, string assignedStatus, bool IsFromApplicationAcceptButton)
        {
            bool result = false;
            WorkorderSchedule techWorkOrderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.Techid == techId).FirstOrDefault();


            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            if (string.Compare(techWorkOrderSchedule.AssignedStatus, "Sent", 0) == 0 || string.Compare(techWorkOrderSchedule.AssignedStatus, "Scheduled", 0) == 0)
            {
                string notesMessage = string.Empty;
                string tmpUserName = string.Empty;
                if (string.Compare(assignedStatus, "Accepted", 0) == 0)
                {
                    if (IsFromApplicationAcceptButton)
                    {
                        notesMessage = "Moved work order to be Accepted by " + techWorkOrderSchedule.TechName;
                        tmpUserName = System.Web.HttpContext.Current.Session["Username"] != null ? System.Web.HttpContext.Current.Session["Username"].ToString() : "";
                    }
                    else
                    {
                        notesMessage = "Work order Accepted by " + techWorkOrderSchedule.TechName;
                        tmpUserName = techWorkOrderSchedule.TechName;
                    }

                    if (techWorkOrderSchedule.PrimaryTech >= 0)
                    {
                        techWorkOrderSchedule.PrimaryTech = 1;
                    }
                    else if (techWorkOrderSchedule.AssistTech >= 0)
                    {
                        techWorkOrderSchedule.AssistTech = 1;
                    }

                    /*if(techWorkOrderSchedule.EventScheduleDate != null)
                    {
                        techWorkOrderSchedule.EventScheduleDate = null;
                    }
                    if (!string.IsNullOrEmpty(techWorkOrderSchedule.ScheduleContactName))
                    {
                        techWorkOrderSchedule.ScheduleContactName = "";
                    }*/
                }
                else if (string.Compare(assignedStatus, "Declined", true) == 0)
                {
                    notesMessage = "Work order Rejected by " + techWorkOrderSchedule.TechName;
                    tmpUserName = techWorkOrderSchedule.TechName;
                }
                techWorkOrderSchedule.AssignedStatus = assignedStatus;
                techWorkOrderSchedule.ModifiedScheduleDate = currentTime;

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = notesMessage,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    //UserName = techWorkOrderSchedule.TechName, 
                    UserName = tmpUserName,
                    isDispatchNotes = 1
                };
                workOrder.NotesHistories.Add(notesHistory);
                result = true;
            }

            int numberOfAssistAccepted = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted" && ws.AssistTech >= 0).Count();
            int numberOfAssistRejected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Declined" && ws.AssistTech >= 0).Count();
            int numberOfAssistDispatches = workOrder.WorkorderSchedules.Where(ws => ws.AssistTech >= 0).Count();
            int numberOfAssistRedirected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Redirected" && ws.AssistTech >= 0).Count();

            int numberOfPrimaryAccepted = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted" && ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryRejected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Declined" && ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryDispatches = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryRedirected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Redirected" && ws.PrimaryTech >= 0).Count();

            string currentStatus = workOrder.WorkorderCallstatus;

            if ((numberOfPrimaryDispatches > 0 || numberOfPrimaryAccepted > 0) && (numberOfAssistDispatches > 0 || numberOfAssistAccepted > 0))
            {
                if (numberOfPrimaryAccepted > 0 && numberOfAssistAccepted > 0)
                {
                    workOrder.WorkorderCallstatus = "Accepted";
                }
                else if (numberOfPrimaryAccepted > 0 || numberOfAssistAccepted > 0)
                {
                    workOrder.WorkorderCallstatus = "Accepted-Partial";
                }
                else
                {
                    workOrder.WorkorderCallstatus = "Dispatching";
                }
            }
            else if (numberOfPrimaryDispatches > 0 && numberOfPrimaryAccepted > 0)
            {
                workOrder.WorkorderCallstatus = "Accepted";
            }
            else if (numberOfAssistDispatches > 0 && numberOfAssistAccepted > 0)
            {
                workOrder.WorkorderCallstatus = "Accepted";
            }
            else if (string.Compare(assignedStatus, "Declined", true) == 0)
            {
                workOrder.WorkorderCallstatus = "Open";
            }
            else if (string.Compare(assignedStatus, "Sent", true) == 0)
            {
                workOrder.WorkorderCallstatus = "Dispatching";
            }

            WorkorderStatusLog statusLog = new WorkorderStatusLog() { StatusFrom = currentStatus, StatusTo = workOrder.WorkorderCallstatus, StausChangeDate = currentTime, WorkorderID = workOrder.WorkorderID };
            workOrder.WorkorderStatusLogs.Add(statusLog);
            return result;
        }


        public ActionResult AcceptWorkOrder(int workOrderId, int techId, bool isResponsible)
        {
            string message = string.Empty;
            bool result = true;
            TechHierarchyView techHierarchyView = GetTechDataByResponsibleTechId(FarmerBrothersEntitites, techId);
            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            DispatchResponseModel dispatchModel = new DispatchResponseModel();
            dispatchModel.TechId = techId;
            dispatchModel.WorkOrderId = workOrderId;
            bool updatedStatus = false;
            if (workOrder != null)
            {
                updatedStatus = UpdateTechAssignedStatus(techId, workOrder, "Accepted", true);
                if (updatedStatus == true)
                {
                    CopyWorkorderEquipments(workOrder);
                    workOrder.IsRejected = false;

                    if (workOrder.WorkorderDetails != null && workOrder.WorkorderDetails.Count > 0)
                    {
                        WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                        if (details != null)
                        {
                            details.ResponsibleTechName = techHierarchyView.PreferredProvider;
                            TECH_HIERARCHY technicianAddress = GetCustomerByTechId(techId.ToString());
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

                            details.Mileage = GetDistance(fromAddress, toAddress);
                        }
                    }

                    if (FarmerBrothersEntitites.SaveChanges() > 0)
                    {
                        message = "|Work Order scheduled successfully!";
                    }
                    else
                    {
                        message = "|There is an error in scheduling Work Order! Please contact support!";
                        result = false;
                    }
                }
                else
                {
                    message = "|Work Order has been Accepted/Redirected! You cannot accept the Work Order!";
                }
            }
            else
            {
                message = "|There is an error in fetching the Work Order! Please contact support!";
                result = false;
            }
            string redirectUrl = string.Empty;
            if (result)
            {
                redirectUrl = new UrlHelper(Request.RequestContext).Action("WorkorderManagement", "Workorder", new { customerId = workOrder.CustomerID, workOrderId = workOrder.WorkorderID });
            }
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { success = result, serverError = ErrorCode.SUCCESS, Url = redirectUrl, message = message };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }



        [EncryptedActionParameter]
        [AllowAnonymous]
        public ActionResult DispatchResponse(int workOrderId, int techId, DispatchResponse response, bool isResponsible, string isBillable = "False")
        {
            TechHierarchyView techHierarchyView = GetTechDataByResponsibleTechId(FarmerBrothersEntitites, techId);
            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            DispatchResponseModel dispatchModel = new DispatchResponseModel();
            dispatchModel.TechId = techId;
            dispatchModel.WorkOrderId = workOrderId;

            WorkorderSchedule techWorkOrderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.Techid == techId).FirstOrDefault();

            if (techId != 0 && techHierarchyView != null)
            {
                if (workOrder != null)
                {
                    bool updatedStatus = false;
                    switch (response)
                    {
                        case FarmerBrothers.Data.DispatchResponse.ACCEPTED:
                            updatedStatus = UpdateTechAssignedStatus(techId, workOrder, "Accepted", false);
                            if (updatedStatus == true)
                            {
                                CopyWorkorderEquipments(workOrder);
                                workOrder.IsRejected = false;

                                if (workOrder.WorkorderDetails != null && workOrder.WorkorderDetails.Count > 0)
                                {
                                    WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                                    if (details != null)
                                    {
                                        details.ResponsibleTechName = techHierarchyView.PreferredProvider;
                                        TECH_HIERARCHY technicianAddress = GetCustomerByTechId(techId.ToString());
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

                                        details.Mileage = GetDistance(fromAddress, toAddress);
                                    }
                                }

                                DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

                                workOrder.WorkorderModifiedDate = currentTime;
                                workOrder.ModifiedUserName = UserName;

                                if (FarmerBrothersEntitites.SaveChanges() > 0)
                                {
                                    dispatchModel.Message = "Work Order scheduled successfully!";
                                }
                                else
                                {
                                    dispatchModel.Message = "There is an error in scheduling Work Order! Please contact support!";
                                }
                            }
                            else
                            {
                                dispatchModel.Message = "Work Order has been Accepted/Redirected! You cannot accept the Work Order!";
                            }
                            break;
                        case FarmerBrothers.Data.DispatchResponse.REJECTED:
                            updatedStatus = UpdateTechAssignedStatus(techId, workOrder, "Declined", false);
                            if (updatedStatus == true)
                            {
                                workOrder.IsRejected = false;
                                DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                                workOrder.WorkorderModifiedDate = currentTime;
                                workOrder.ModifiedUserName = UserName;
                                if (FarmerBrothersEntitites.SaveChanges() > 0)
                                {
                                    dispatchModel.Message = "Work Order rejected!";
                                }
                                else
                                {
                                    dispatchModel.Message = "There is an error in declining the Work Order! Please contact support!";
                                }
                            }
                            else
                            {
                                dispatchModel.Message = "Work Order has been redirected! You need not reject the Work Order!";
                            }
                            break;
                        case FarmerBrothers.Data.DispatchResponse.STARTED:
                            if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Declined")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Start, You Rejected the Call";
                            }
                            else if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Redirected")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Operate Event, Call Has been Redirected by You";
                            }
                            else
                            {
                                if (workOrder != null
                                    && (string.Compare(workOrder.WorkorderCallstatus, "Accepted", true) == 0
                                        || string.Compare(workOrder.WorkorderCallstatus, "Accepted-Partial", true) == 0))
                                {
                                    DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                                    NotesHistory notesHistory = new NotesHistory()
                                    {
                                        AutomaticNotes = 1,
                                        EntryDate = currentTime,
                                        Notes = techHierarchyView.PreferredProvider + " Started to customer location",
                                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                        UserName = techHierarchyView.PreferredProvider,
                                        isDispatchNotes = 1
                                    };
                                    workOrder.NotesHistories.Add(notesHistory);
                                    if (workOrder.WorkorderDetails != null && workOrder.WorkorderDetails.Count > 0)
                                    {
                                        WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                                        if (details != null)
                                        {
                                            details.StartDateTime = currentTime;
                                            details.ResponsibleTechName = techHierarchyView.PreferredProvider;
                                        }

                                        if (FarmerBrothersEntitites.SaveChanges() > 0)
                                        {
                                            dispatchModel.TechId = techId;
                                            dispatchModel.WorkOrderId = workOrderId;
                                            dispatchModel.Message = "Started status updated!";
                                        }
                                    }
                                    else
                                    {
                                        dispatchModel.TechId = techId;
                                        dispatchModel.WorkOrderId = workOrderId;
                                        dispatchModel.Message = "There is a problem in updating status! Please contact support!";
                                    }
                                }
                                else
                                {
                                    dispatchModel.TechId = techId;
                                    dispatchModel.WorkOrderId = workOrderId;
                                    if (workOrder == null)
                                    {
                                        dispatchModel.Message = "Error Fetching the Workorder, Try Again";
                                    }
                                    else if (string.Compare(workOrder.WorkorderCallstatus, "Accepted", true) != 0
                                        || string.Compare(workOrder.WorkorderCallstatus, "Accepted-Partial", true) != 0)
                                    {
                                        //dispatchModel.Message = "Work Order is in '" + workOrder.WorkorderCallstatus + "' Status, Update Start Status only if the WorkOrder Status is in 'Accepted' or 'Accepted-Partial' ";
                                        dispatchModel.Message = "Work Order is in <b> '" + workOrder.WorkorderCallstatus + "' </b> Status <br/> 'Start' Time can be changed only if the Work order is in <b> 'Accepted' or 'Accepted-Partial' </b> Status.";
                                    }
                                    //dispatchModel.TechId = techId;
                                    //dispatchModel.WorkOrderId = workOrderId;
                                    //dispatchModel.Message = "Work Order is not yet Accepted!";
                                }
                            }
                            break;
                        case FarmerBrothers.Data.DispatchResponse.ARRIEVED:
                            if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Declined")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot update Arrival status, You Rejected the Call";
                            }
                            else if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Redirected")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Operate Event, Call Has been Redirected by You";
                            }
                            else
                            {
                                DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                                if (workOrder != null && workOrder.WorkorderDetails != null && workOrder.WorkorderDetails.Count > 0)
                                {
                                    WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                                    if (details.StartDateTime != null)
                                    {
                                        DateTime startDate = Convert.ToDateTime(details.StartDateTime);
                                        System.TimeSpan diff = currentTime.Subtract(startDate);

                                        if (diff.TotalHours > 4)
                                        {
                                            dispatchModel.TechId = techId;
                                            dispatchModel.WorkOrderId = workOrderId;
                                            dispatchModel.Message = "ArrivalDate cannot be more than 4 Hours from Start Time. Please update the Arrival Date from Closure applicaiton";
                                        }
                                        else
                                        {
                                            if (workOrder != null
                                            && (string.Compare(workOrder.WorkorderCallstatus, "Accepted", true) == 0
                                                || string.Compare(workOrder.WorkorderCallstatus, "Accepted-Partial", true) == 0))
                                            {
                                                NotesHistory notesHistory = new NotesHistory()
                                                {
                                                    AutomaticNotes = 1,
                                                    EntryDate = currentTime,
                                                    Notes = techHierarchyView.PreferredProvider + " arrived at customer location",
                                                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                                    UserName = techHierarchyView.PreferredProvider,
                                                    isDispatchNotes = 1
                                                };
                                                workOrder.NotesHistories.Add(notesHistory);
                                                if (workOrder.WorkorderDetails != null && workOrder.WorkorderDetails.Count > 0)
                                                {
                                                    //WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                                                    if (details != null)
                                                    {
                                                        details.ArrivalDateTime = currentTime;
                                                        details.ResponsibleTechName = techHierarchyView.PreferredProvider;
                                                    }

                                                    workOrder.WorkorderCallstatus = "On Site";

                                                    workOrder.WorkorderModifiedDate = currentTime;
                                                    workOrder.ModifiedUserName = UserName;

                                                    if (FarmerBrothersEntitites.SaveChanges() > 0)
                                                    {
                                                        dispatchModel.TechId = techId;
                                                        dispatchModel.WorkOrderId = workOrderId;
                                                        dispatchModel.Message = "Arrival status updated!";
                                                    }
                                                }
                                                else
                                                {
                                                    dispatchModel.TechId = techId;
                                                    dispatchModel.WorkOrderId = workOrderId;
                                                    dispatchModel.Message = "There is a problem in updating status! Please contact support!";
                                                }
                                            }
                                            else
                                            {
                                                dispatchModel.TechId = techId;
                                                dispatchModel.WorkOrderId = workOrderId;
                                                if (workOrder == null)
                                                {
                                                    dispatchModel.Message = "Error Fetching the Workorder, Try Again";
                                                }
                                                else if (string.Compare(workOrder.WorkorderCallstatus, "On Site", true) == 0)
                                                {
                                                    dispatchModel.Message = "Work Order is already in <b> 'On Site' </b> Status ";
                                                }
                                                else if (string.Compare(workOrder.WorkorderCallstatus, "Accepted", true) != 0
                                                    || string.Compare(workOrder.WorkorderCallstatus, "Accepted-Partial", true) != 0)
                                                {
                                                    //dispatchModel.Message = "Work Order is in '" + workOrder.WorkorderCallstatus + "' Status, Update Arrival/OnSIte Status only  if the WorkOrder Status is in 'Accepted' or 'Accepted-Partial' ";
                                                    dispatchModel.Message = "Work Order is in <b> '" + workOrder.WorkorderCallstatus + "' </b> Status <br/> 'Arrival/OnSIte' Status can be changed only if the Work order is in <b> 'Accepted' </b> or <b> 'Accepted-Partial' </b> Status.";
                                                }

                                                //dispatchModel.TechId = techId;
                                                //dispatchModel.WorkOrderId = workOrderId;
                                                //dispatchModel.Message = "Work Order is not yet Accepted!";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dispatchModel.TechId = techId;
                                        dispatchModel.WorkOrderId = workOrderId;
                                        dispatchModel.Message = "StartDate is not updated. Please set the StartDate before ArrivalDate";
                                    }
                                }
                            }
                            break;
                        case FarmerBrothers.Data.DispatchResponse.REDIRECTED:
                            if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Declined")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Redirect, You Rejected the Call";
                            }
                            else if (techWorkOrderSchedule != null && (techWorkOrderSchedule.AssignedStatus == "Sent" || techWorkOrderSchedule.AssignedStatus == "Accepted"))
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Call has been Sent/Accepted by the selected Tech";
                            }
                            else
                            {                                
                                //WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(sc => sc.Techid == techId && (sc.AssignedStatus.ToLower() == "sent" || sc.AssignedStatus.ToLower() == "accepted")).FirstOrDefault();
                                var  ws = (from sc in FarmerBrothersEntitites.WorkorderSchedules
                                                        join t in FarmerBrothersEntitites.TECH_HIERARCHY on sc.Techid equals t.DealerId
                                                        where sc.WorkorderID == workOrderId && (sc.AssignedStatus.ToLower() == "sent" || sc.AssignedStatus.ToLower() == "accepted")
                                                        && t.FamilyAff == "SPT"
                                                        select new 
                                                        {
                                                            Techid = sc.Techid,
                                                            AssignedStatus = sc.AssignedStatus,
                                                            WorkorderID = sc.WorkorderID,
                                                            familyAff = t.FamilyAff
                                                        }).FirstOrDefault();

                                //WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(sc => sc.Techid == techId && (sc.AssignedStatus.ToLower() == "sent" || sc.AssignedStatus.ToLower() == "accepted"))
                                //    .Join().FirstOrDefault();

                                if (ws == null)
                                {
                                    if (workOrder.WorkorderCallstatus == "Closed")
                                    {
                                        dispatchModel.TechId = techId;
                                        dispatchModel.WorkOrderId = workOrderId;
                                        dispatchModel.Message = "Work Order can't be redirected!";
                                        break;
                                    }

                                    DispatchMail(workOrderId, techId, isResponsible);
                                    DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

                                    workOrder.WorkorderModifiedDate = currentTime;
                                    workOrder.ModifiedUserName = UserName;
                                    if (FarmerBrothersEntitites.SaveChanges() > 0)
                                    {
                                        dispatchModel.TechId = techId;
                                        dispatchModel.WorkOrderId = workOrderId;
                                        dispatchModel.Message = "Work Order has been redirected successfully!";
                                    }
                                }
                                else
                                {
                                    dispatchModel.TechId = techId;
                                    dispatchModel.WorkOrderId = workOrderId;
                                    dispatchModel.Message = "Work Order can't be redirected! It is Assigned to Third Party Tech";
                                }
                            }
                            break;

                        case FarmerBrothers.Data.DispatchResponse.COMPLETED:
                            if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Declined")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Complete, You Rejected the Call";
                            }
                            else if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Redirected")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Operate Event, Call Has been Redirected by You";
                            }
                            else
                            {
                                DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                                if (workOrder != null && workOrder.WorkorderDetails != null && workOrder.WorkorderDetails.Count > 0)
                                {
                                    WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                                    if (details.ArrivalDateTime != null)
                                    {
                                        DateTime arrivalDate = Convert.ToDateTime(details.ArrivalDateTime);
                                        System.TimeSpan diff = currentTime.Subtract(arrivalDate);

                                        if (diff.TotalHours > 12)
                                        {
                                            dispatchModel.TechId = techId;
                                            dispatchModel.WorkOrderId = workOrderId;
                                            dispatchModel.Message = "CompletionDate cannot be more than 12 Hours from Arrival Time. Please update the Completion Date from Closure applicaiton";
                                        }
                                        else
                                        {
                                            if (workOrder != null
                                                && string.Compare(workOrder.WorkorderCallstatus, "On Site", true) == 0)
                                            {

                                                //DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                                                NotesHistory notesHistory = new NotesHistory()
                                                {
                                                    AutomaticNotes = 1,
                                                    EntryDate = currentTime,
                                                    Notes = "Work order completed by " + techHierarchyView.PreferredProvider,
                                                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                                    UserName = techHierarchyView.PreferredProvider,
                                                    isDispatchNotes = 1
                                                };
                                                workOrder.NotesHistories.Add(notesHistory);
                                                if (workOrder.WorkorderDetails != null)
                                                {
                                                    //WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                                                    if (details != null)
                                                    {
                                                        details.CompletionDateTime = currentTime;
                                                        details.ResponsibleTechName = techHierarchyView.PreferredProvider;
                                                    }
                                                }
                                                workOrder.WorkorderCallstatus = "Completed";

                                                workOrder.WorkorderModifiedDate = currentTime;
                                                workOrder.ModifiedUserName = UserName;

                                                if (FarmerBrothersEntitites.SaveChanges() > 0)
                                                {
                                                    dispatchModel.TechId = techId;
                                                    dispatchModel.WorkOrderId = workOrderId;
                                                    dispatchModel.Message = "Work Order completed successfully!";
                                                }
                                            }
                                            else
                                            {
                                                dispatchModel.TechId = techId;
                                                dispatchModel.WorkOrderId = workOrderId;
                                                if (workOrder == null)
                                                {
                                                    dispatchModel.Message = "Error Fetching the Workorder, Try Again";
                                                }
                                                else if (string.Compare(workOrder.WorkorderCallstatus, "Completed", true) == 0)
                                                {
                                                    dispatchModel.Message = "Work Order is already in <b> 'Completed' </b> Status ";
                                                }
                                                else if (string.Compare(workOrder.WorkorderCallstatus, "On Site", true) != 0)
                                                {
                                                    //dispatchModel.Message = "Work Order is in '" + workOrder.WorkorderCallstatus + "' Status, Update Completed Status only  if the WorkOrder Status is in 'On Site' ";
                                                    dispatchModel.Message = "Work Order is in <b> '" + workOrder.WorkorderCallstatus + "' </b> Status <br/> 'Completed' Status can be changed only if the Work order is in <b> 'On site' </b> Status.";
                                                }

                                                //dispatchModel.TechId = techId;
                                                //dispatchModel.WorkOrderId = workOrderId;
                                                //dispatchModel.Message = "Work Order is not yet Started!";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dispatchModel.TechId = techId;
                                        dispatchModel.WorkOrderId = workOrderId;
                                        dispatchModel.Message = "ArrivalDate is not updated. Please set the ArrivalDate before CompletionDate";
                                    }
                                }
                            }
                            break;
                        case FarmerBrothers.Data.DispatchResponse.ACKNOWLEDGED:
                            {

                                DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                                NotesHistory notesHistory = new NotesHistory()
                                {
                                    AutomaticNotes = 1,
                                    EntryDate = currentTime,
                                    Notes = "Work order redirect Disregard by " + techHierarchyView.PreferredProvider,
                                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                    UserName = techHierarchyView.PreferredProvider,
                                    isDispatchNotes = 1
                                };

                                workOrder.NotesHistories.Add(notesHistory);
                                workOrder.WorkorderModifiedDate = currentTime;
                                workOrder.ModifiedUserName = UserName;
                                if (FarmerBrothersEntitites.SaveChanges() > 0)
                                {
                                    dispatchModel.TechId = techId;
                                    dispatchModel.WorkOrderId = workOrderId;
                                    dispatchModel.Message = "Work Order redirect Disregard!";

                                }
                            }
                            break;
                        case FarmerBrothers.Data.DispatchResponse.CALLCLOSURE:
                            if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Declined")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Close, You Rejected the Call";
                            }
                            else if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Redirected")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Operate Event, Call Has been Redirected by You";
                            }
                            else
                            {
                                if (workOrder.WorkorderCallstatus == "Accepted" || workOrder.WorkorderCallstatus == "Accepted-Partial" || workOrder.WorkorderCallstatus == "Completed" || workOrder.WorkorderCallstatus == "On Site")
                                {
                                    if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseExternalCallClosure"]))
                                    {
                                        return Redirect(ConfigurationManager.AppSettings["CallClosureUrl"] + "?Status=CLOSED&TECHID=" + techId + "&SCF=" + workOrder.WorkorderID + "&IsBillable=" + isBillable);
                                    }
                                    else
                                    {
                                        return RedirectToActionPermanent("CallClosureManagement", "CallClosure", new { customerId = workOrder.CustomerID, workOrderId = workOrder.WorkorderID });
                                    }
                                }
                                else
                                {
                                    if (workOrder == null)
                                    {
                                        dispatchModel.Message = "Error Fetching the Workorder, Try Again";
                                    }
                                    else if (string.Compare(workOrder.WorkorderCallstatus, "Accepted", true) != 0
                                        || string.Compare(workOrder.WorkorderCallstatus, "Accepted-Partial", true) != 0
                                        || string.Compare(workOrder.WorkorderCallstatus, "Completed", true) != 0
                                        || string.Compare(workOrder.WorkorderCallstatus, "On Site", true) != 0)
                                    {
                                        //dispatchModel.Message = "Work Order is in '" + workOrder.WorkorderCallstatus + "' Status, Access CallClosure, if the Workorder Status is Accepted/Accepted-Partial/Completed/On Site ";
                                        dispatchModel.Message = "Work Order is in <b> '" + workOrder.WorkorderCallstatus + "' </b> Status <br/> Access CallClosure, only if the Work order Satus is in <b> 'Accepted'/'Accepted-Partial'/'Completed'/'On Site' </b> Status.";
                                    }

                                    //dispatchModel.Message = "Work Order can't be Closed, may be it's not Accepted yet!";
                                }
                            }
                            break;
                        case FarmerBrothers.Data.DispatchResponse.SCHEDULED:
                            if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Declined")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Schedule, You Rejected the Call";
                            }
                           else if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Redirected")
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Cannot Operate Event, Call Has been Redirected by You";
                            }
                            else
                            {
                                if (workOrder.WorkorderCallstatus.ToLower() != "completed" && workOrder.WorkorderCallstatus.ToLower() != "closed" && workOrder.WorkorderCallstatus.ToLower() != "invoiced")
                                {
                                    return RedirectToActionPermanent("ScheduleEvent", "ScheduleEvent", new { customerId = workOrder.CustomerID, workOrderId = workOrder.WorkorderID, techId = techId });
                                    //return Redirect(ConfigurationManager.AppSettings["ScheduleEventUrl"] + "?customerId=" + workOrder.CustomerID + "&workOrderId=" + workOrder.WorkorderID + "&techId=" + techId);
                                }
                                else
                                {
                                    dispatchModel.Message = "Work Order is Already Closed/Completed";
                                }
                            }
                            break;
                    }
                }
                else
                {
                    dispatchModel.Message = "There is an error in fetching the Work Order! Please contact support!";
                }

            }
            else
            {

                string computer_name = System.Net.Dns.GetHostEntry(Request.ServerVariables["remote_addr"]).HostName;
                string sytemInfo = Utility.GetClientSystemDetails();
                sytemInfo += "Computer Name: ";
                sytemInfo += computer_name;
                sytemInfo += Environment.NewLine;

                using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                {
                    FBActivityLog log = new FBActivityLog();
                    log.LogDate = DateTime.UtcNow;
                    log.UserId = System.Web.HttpContext.Current.Session["UserId"] != null ? (int)System.Web.HttpContext.Current.Session["UserId"] : 1;
                    log.ErrorDetails = "EXTERNAL_REQ: " + sytemInfo.ToString();
                    entity.FBActivityLogs.Add(log);
                    entity.SaveChanges();
                }
            }

            return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
        }

        [EncryptedActionParameter]
        [AllowAnonymous]
        public ActionResult CompleteNonServiceEvent(int workOrderId)
        {
            NonServiceworkorder nsw = FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == workOrderId).FirstOrDefault();
            DateTime CurrentTime = Utility.GetCurrentTime(nsw.CustomerZipCode, FarmerBrothersEntitites);

            DispatchResponseModel dispatchModel = new DispatchResponseModel();
            dispatchModel.WorkOrderId = workOrderId;

            if (nsw.NonServiceEventStatus.ToLower() == "closed")
            {
                dispatchModel.Message = "Customer Service Event is already Closed!";
            }
            else
            {
                /*string message = "";
                completeNonServiceWorkorder(workOrderId, out message);

                dispatchModel.Message = message;*/

                FarmerBrothers.Models.NonServiceEventModel NSEM = new FarmerBrothers.Models.NonServiceEventModel();
                NSEM.WorkOrderID = workOrderId;
                return View(NSEM);
            }
            return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
        }

        [AllowAnonymous]
        public JsonResult completeNonServiceWorkorder(int WorkOrderId, string Notes)
        {
            string responseMessage = "";
            JsonResult jsonResult = new JsonResult();
            //DispatchResponseModel dispatchModel = new DispatchResponseModel();
            //dispatchModel.WorkOrderId = WorkOrderId;
            try
            {
                string message = "";
                completeNSWorkorder(WorkOrderId, Notes, out message);

                //dispatchModel.Message = message;
                //return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
            }
            catch (Exception ex)
            {
                //dispatchModel.Message = "Error Completing the Non Service Workorder"; ;
                responseMessage = "Error Completing the Non Service Workorder";
                jsonResult.Data = new { success = false, serverError = ErrorCode.ERROR, data = true, workorderid = WorkOrderId, message = responseMessage };
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
                //return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
            }

            responseMessage = "Customer Service Event " + WorkOrderId + " Closed successfully!"; ;
            jsonResult.Data = new { success = true, serverError = ErrorCode.SUCCESS, data = true, workorderid = WorkOrderId, message = responseMessage };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        private void completeNSWorkorder(int workOrderId, string Notes, out string message)
        {
            try
            {
                NonServiceworkorder nsw = FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == workOrderId).FirstOrDefault();
                DateTime CurrentTime = Utility.GetCurrentTime(nsw.CustomerZipCode, FarmerBrothersEntitites);

                nsw.NonServiceEventStatus = "Closed";
                nsw.CloseDate = CurrentTime;

                NotesHistory customCloseNotes = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = CurrentTime,
                    Notes = Notes,
                    Userid = 1234,
                    UserName = "WEB",
                    NonServiceWorkorderID = workOrderId
                };
                FarmerBrothersEntitites.NotesHistories.Add(customCloseNotes);

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = CurrentTime,
                    Notes = "Customer Service Event Closed successfully",
                    Userid = 1234,
                    UserName = "WEB",
                    NonServiceWorkorderID = workOrderId,
                    isDispatchNotes = 1
                };
                FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                FarmerBrothersEntitites.SaveChanges();

                message = "Customer Service Event " + workOrderId + " Closed successfully!";
            }
            catch(Exception ex)
            {
                message = "There is a problem in closing the Customer Service Event " + workOrderId ;
            }
        }

        private void ServiceEventStatus(int workOrderId, out string message)
        {
            try
            {
                NonServiceworkorder nsw = FarmerBrothersEntitites.NonServiceworkorders.Where(w => w.WorkOrderID == workOrderId).FirstOrDefault();
                DateTime CurrentTime = Utility.GetCurrentTime(nsw.CustomerZipCode, FarmerBrothersEntitites);

                nsw.NonServiceEventStatus = "Service Event";
                nsw.CloseDate = CurrentTime;

                //NotesHistory notesHistory = new NotesHistory()
                //{
                //    AutomaticNotes = 1,
                //    EntryDate = CurrentTime,
                //    Notes = "Customer Service Event Closed successfully",
                //    Userid = 1234,
                //    UserName = "WEB",
                //    NonServiceWorkorderID = workOrderId
                //};
                //FarmerBrothersEntitites.NotesHistories.Add(notesHistory);
                FarmerBrothersEntitites.SaveChanges();

                message = "Status Changed to Service Event For ServiceEventId: " + workOrderId;
            }
            catch (Exception ex)
            {
                message = "There is a problem in changig the status to Customer Service Event For: " + workOrderId;
            }
        }


        [EncryptedActionParameter]
        [AllowAnonymous]
        public ActionResult CreateServiceEvent(int workOrderId, int techId, DispatchResponse response, bool isResponsible, string isBillable="")
        {
            NonServiceworkorder nsw = FarmerBrothersEntitites.NonServiceworkorders.Where(nswo => nswo.WorkOrderID == workOrderId).FirstOrDefault();
            DateTime currentTime = Utility.GetCurrentTime(nsw.CustomerZipCode, FarmerBrothersEntitites);

            string createdUser = isBillable;

            string message = string.Empty;
            int serviceEventId = 0;
            CreateServiceWorkOrder(workOrderId, createdUser, out message, out serviceEventId);

            DispatchResponseModel dispatchModel = new DispatchResponseModel();
            dispatchModel.WorkOrderId = serviceEventId;
            dispatchModel.Message = message;

            return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
        }

        //=================================================================================

        private void CreateServiceWorkOrder(int workOrderId, string createdUserName, out string message, out int serviceEventId)
        {
            NonServiceworkorder nsw = FarmerBrothersEntitites.NonServiceworkorders.Where(nswo => nswo.WorkOrderID == workOrderId).FirstOrDefault();
            DateTime currentTime = Utility.GetCurrentTime(nsw.CustomerZipCode, FarmerBrothersEntitites);
                        
            if (nsw.NonServiceEventStatus != null && 
                nsw.NonServiceEventStatus.ToLower() != "closed" && nsw.NonServiceEventStatus.ToLower() != "service event")
            {
                //FbUserMaster createdUser = null; string createdUserName = "";
                /*if (nsw != null && nsw.CreatedBy.HasValue)
                {
                    createdUser = FarmerBrothersEntitites.FbUserMasters.Where(usr => usr.UserId == nsw.CreatedBy).FirstOrDefault();
                    if(createdUser != null)
                    {
                        if(!string.IsNullOrEmpty(createdUser.FirstName) && !string.IsNullOrEmpty(createdUser.LastName))
                        {
                            createdUserName = createdUser.FirstName + " " + createdUser.LastName;
                        }
                        else if (!string.IsNullOrEmpty(createdUser.FirstName) && string.IsNullOrEmpty(createdUser.LastName))
                        {
                            createdUserName = createdUser.FirstName;
                        }
                        else if (string.IsNullOrEmpty(createdUser.FirstName) && !string.IsNullOrEmpty(createdUser.LastName))
                        {
                            createdUserName = createdUser.LastName;
                        }
                    }
                }*/

                WorkOrder serviceWorkOrder = new WorkOrder();
                List<Type> collections = new List<Type>() { typeof(IEnumerable<>), typeof(IEnumerable) };

                int? responsibleTechId = null;

                //foreach (var property in nsw.GetType().GetProperties())
                //{
                //    if (property.PropertyType == typeof(string) || !property.PropertyType.GetInterfaces().Any(i => collections.Any(c => i == c))
                //        && serviceWorkOrder.GetType().GetProperty(property.Name) != null)
                //    {
                //        property.SetValue(serviceWorkOrder, property.GetValue(nsw));
                //    }
                //}

                FarmerBrothersEntities newEntity = new FarmerBrothersEntities();
                Customer serviceCustomer = null;
                if (!string.IsNullOrEmpty(nsw.CustomerID.ToString()))
                {
                    var CustomerId = int.Parse(nsw.CustomerID.ToString());
                    serviceCustomer = newEntity.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                }

                serviceWorkOrder.EntryUserName = createdUserName;

                IndexCounter workOrderCounter = Utility.GetIndexCounter("WorkorderID", 1);
                workOrderCounter.IndexValue++;

                serviceEventId = workOrderCounter.IndexValue.Value;

                serviceWorkOrder.CustomerID = nsw.CustomerID;
                serviceWorkOrder.CustomerCity = nsw.CustomerCity;
                serviceWorkOrder.CustomerState = nsw.CustomerState;
                serviceWorkOrder.CustomerZipCode = nsw.CustomerZipCode;
                serviceWorkOrder.CustomerName = serviceCustomer.CompanyName;
                serviceWorkOrder.CustomerPhone = serviceCustomer.Phone;
                serviceWorkOrder.PriorityCode = 5;

                serviceWorkOrder.WorkorderID = workOrderCounter.IndexValue.Value;
                serviceWorkOrder.WorkorderEntryDate = currentTime;
                serviceWorkOrder.WorkorderCallstatus = "Open";
                serviceWorkOrder.WorkorderCloseDate = null;
                serviceWorkOrder.WorkorderModifiedDate = currentTime;
                serviceWorkOrder.CustomerMainContactName = "";
                serviceWorkOrder.WorkorderEquipCount = 1;
                serviceWorkOrder.IsSpecificTechnician = false;

                serviceWorkOrder.CallerName = string.IsNullOrEmpty(nsw.CallerName) ? "N/A" : nsw.CallerName;
                serviceWorkOrder.WorkorderContactName = "N/A";
                serviceWorkOrder.WorkorderContactPhone = string.IsNullOrEmpty(nsw.CallBack) ? "N/A" : nsw.CallBack;
                serviceWorkOrder.HoursOfOperation = "N/A";

                WorkorderDetail serviceWorkOrderDetail = new WorkorderDetail();
                serviceWorkOrderDetail.WorkorderID = serviceWorkOrder.WorkorderID;
                serviceWorkOrderDetail.ArrivalDateTime = null;
                serviceWorkOrderDetail.CompletionDateTime = null;
                serviceWorkOrderDetail.StartDateTime = null;
                serviceWorkOrderDetail.EntryDate = null;
                serviceWorkOrderDetail.ModifiedDate = null;
                serviceWorkOrderDetail.SpecialClosure = "";
                serviceWorkOrderDetail.TravelTime = "";
                serviceWorkOrderDetail.InvoiceNo = "";
                serviceWorkOrderDetail.SolutionId = null;

                serviceWorkOrder.WorkorderDetails.Add(serviceWorkOrderDetail);              


                WorkOrderBrand newBrand = new WorkOrderBrand();
                newBrand.WorkorderID = serviceWorkOrder.WorkorderID;
                newBrand.BrandID = 997;
                serviceWorkOrder.WorkOrderBrands.Add(newBrand);

                foreach (NotesHistory notes in nsw.NotesHistories)
                {
                    NotesHistory newNotes = new NotesHistory();
                    foreach (var property in notes.GetType().GetProperties())
                    {
                        if (property.GetValue(notes) != null && property.GetValue(notes).GetType() != null && (property.GetValue(notes).GetType().IsValueType || property.GetValue(notes).GetType() == typeof(string)))
                        {
                            property.SetValue(newNotes, property.GetValue(notes));
                        }
                    }
                    newNotes.NonServiceWorkorderID = null;
                    newNotes.WorkorderID = serviceWorkOrder.WorkorderID;
                    serviceWorkOrder.NotesHistories.Add(newNotes);
                }

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = @"Service Work Order "+ serviceWorkOrder.WorkorderID +" Created for Non-Service work order " + nsw.WorkOrderID,
                    Userid = nsw.CreatedBy != null ? Convert.ToInt32(nsw.CreatedBy) : 1234,
                    UserName = createdUserName,
                    isDispatchNotes = 0
                };
                serviceWorkOrder.NotesHistories.Add(notesHistory);

                serviceWorkOrder.WorkorderCalltypeid = 1200;
                serviceWorkOrder.WorkorderCalltypeDesc = "Service";

                WorkorderEquipmentRequested spawnEquipmentRequested = new WorkorderEquipmentRequested();
                //WorkorderEquipmentRequested workOrderReq = FarmerBrothersEntitites.WorkorderEquipmentRequesteds.Where(wr => wr.Assetid == equipment.Assetid).FirstOrDefault();

                IndexCounter counter = Utility.GetIndexCounter("AssetID", 1);
                counter.IndexValue++;

                WorkorderEquipmentRequested equipment = new WorkorderEquipmentRequested()
                {
                    Assetid = counter.IndexValue.Value,
                    CallTypeid = 1200,
                    Category = ".11 - No Info – Only OTHER",
                    Location = "N/A",
                    SerialNumber = "",
                    Model = "",
                    Symptomid = 2001
                };
                serviceWorkOrder.WorkorderEquipmentRequesteds.Add(equipment);

                newEntity.WorkOrders.Add(serviceWorkOrder);
                newEntity.SaveChanges();

                if (newEntity != null)
                {
                    newEntity.Dispose();
                }

                /*string emailAddresses = string.Empty;


                StringBuilder subject = new StringBuilder();
                subject.Append("Spawned Workorder - Original WO: ");
                subject.Append(serviceWorkOrder.OriginalWorkorderid);
                subject.Append(" ST: ");
                subject.Append(serviceWorkOrder.CustomerState);
                subject.Append(" Call Type: ");
                subject.Append(serviceWorkOrder.WorkorderCalltypeDesc);

                SendWorkOrderMail(serviceWorkOrder, subject.ToString(), emailAddresses, ConfigurationManager.AppSettings["DispatchMailFromAddress"], null, MailType.INFO, false, null);

                if (responsibleTechId.HasValue)
                {
                    subject = new StringBuilder();
                    subject.Append("WO:");
                    subject.Append(serviceWorkOrder.WorkorderID);
                    subject.Append(" ST:");
                    subject.Append(serviceWorkOrder.CustomerState);
                    subject.Append(" Call Type:");
                    subject.Append(serviceWorkOrder.WorkorderCalltypeDesc);


                    string emailAddress = string.Empty;
                    string salesEmailAddress = string.Empty;
                    var CustomerId = int.Parse(responsibleTechId.Value.ToString());
                    Customer serviceCustomer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == CustomerId).FirstOrDefault();
                    if (serviceCustomer != null)
                    {
                        emailAddress = serviceCustomer.Email;
                        if (!string.IsNullOrEmpty(serviceCustomer.SalesEmail))
                        {
                            salesEmailAddress = serviceCustomer.SalesEmail;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TestEmail"]))
                    {
                        emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                    }

                    if (!string.IsNullOrWhiteSpace(emailAddress))
                    {
                        SendWorkOrderMail(spawnWorkOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], responsibleTechId, MailType.SPAWN, true, null, string.Empty, false, salesEmailAddress);
                    }
                }*/

                WorkorderController wc = new WorkorderController();
                wc.StartAutoDispatchProcess(serviceWorkOrder, createdUserName);
                //completeNonServiceWorkorder(workOrderId, out message);      
                ServiceEventStatus(workOrderId, out message);

                NotesHistory WONotesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = @"Service Work Order " + serviceWorkOrder.WorkorderID + " created ",
                    Userid = nsw.CreatedBy != null ? Convert.ToInt32(nsw.CreatedBy) : 1234,
                    UserName = createdUserName,
                    NonServiceWorkorderID = nsw.WorkOrderID
                };
                FarmerBrothersEntitites.NotesHistories.Add(WONotesHistory);
                FarmerBrothersEntitites.SaveChanges();

                message = @"Service Work Order " + serviceWorkOrder.WorkorderID + " is created!";
            }
            else
            {
                serviceEventId = 0;
                string msg = "";
                if (nsw.NonServiceEventStatus.ToLower() == "closed")
                {
                     msg = @"Customer Service Event " + nsw.WorkOrderID + " is Already Closed!";
                }
                else if(nsw.NonServiceEventStatus.ToLower() == "service event")
                {
                    msg = @"Service Event is already created for the Customer Service Event " + nsw.WorkOrderID;
                }
                message = msg;
            }
        }


        //=================================================================================

        [AllowAnonymous]
        public ActionResult ERFStatusUpdate(string ERFId, string ESM, string Status)
        {
            DispatchResponseModel dispatchModel = new DispatchResponseModel();
            Erf erf = FarmerBrothersEntitites.Erfs.Where(er => er.ErfID == ERFId).FirstOrDefault();

            if (Status.ToLower() == "cancel")
            {
                if (erf.ERFStatus.ToLower() == "cancel")
                {
                    dispatchModel.IsERF = true;
                    dispatchModel.Message = "ERF is already in cancel status";
                    return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
                }                
            }


            string tempStatus = erf.ERFStatus == null ? "" : erf.ERFStatus;
            if (erf != null)
            {
                if (Status == "Shipped")
                {
                    return RedirectToActionPermanent("ERFShipped", "DispatchResponse", new { ERFId = ERFId, ESM = ESM, Status = Status });
                }
                else
                {
                    erf.ERFStatus = Status;
                }
            }

            DateTime CurrentTime = Utility.GetCurrentTime(erf.CustomerZipCode, FarmerBrothersEntitites);
            int esmId = Convert.ToInt32(ESM);
            //ESMDSMRSM esmdsmrsmView = FarmerBrothersEntitites.ESMDSMRSMs.FirstOrDefault(x => x.EDSMID == esmId);
            ESMCCMRSMEscalation esmdsmrsmView = FarmerBrothersEntitites.ESMCCMRSMEscalations.FirstOrDefault(x => x.EDSMID == esmId);
            NotesHistory notesHistory = new NotesHistory()
            {
                AutomaticNotes = 1,
                EntryDate = CurrentTime,
                Notes = "[ERF]:  Status Updated from " + tempStatus + " to " + Status,
                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                UserName = esmdsmrsmView == null ? "1234" :esmdsmrsmView.ESMName,
                ErfID = erf.ErfID,
                WorkorderID = erf.WorkorderID,
                isDispatchNotes = 1
            };
            FarmerBrothersEntitites.NotesHistories.Add(notesHistory);



            int returnValue = FarmerBrothersEntitites.SaveChanges();

            if (Status == "Cancel")
            {
                ERFNewController enc = new ERFNewController();
                enc.ERFEmail(erf.ErfID, erf.WorkorderID, false, erf.ApprovalStatus, true);
            }

            dispatchModel.IsERF = true;
            if (returnValue > 0)
            {
                dispatchModel.Message = "ERF Status Successfully Updated to " + Status;
            }

            return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
        }

        [AllowAnonymous]
        public ActionResult ERFShipped(string ERFId, string ESM, string Status)
        {
            ViewBag.erfId = ERFId;
            ViewBag.status = Status;
            ViewBag.ESM = ESM;

            return View();
        }

        [AllowAnonymous]
        public JsonResult ERFShippedStatus(string ERFID, string ESM, string Status, string Notes)
        {
            string message = string.Empty;
            string updateStatus = string.Empty;

            try
            {
                Erf erf = FarmerBrothersEntitites.Erfs.Where(er => er.ErfID == ERFID).FirstOrDefault();
                string tempStatus = erf.ERFStatus == null ? "" : erf.ERFStatus;
                if (erf != null && erf.CustomerZipCode != null)
                {
                    erf.ERFStatus = Status;

                    DateTime CurrentTime = Utility.GetCurrentTime(erf.CustomerZipCode, FarmerBrothersEntitites);
                    int esmId = Convert.ToInt32(ESM);
                    //ESMDSMRSM esmdsmrsmView = FarmerBrothersEntitites.ESMDSMRSMs.FirstOrDefault(x => x.EDSMID == esmId);
                    ESMCCMRSMEscalation esmdsmrsmView = FarmerBrothersEntitites.ESMCCMRSMEscalations.FirstOrDefault(x=>x.EDSMID == esmId);
                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = CurrentTime,
                        Notes = "[ERF Tracking Notes]:  " + Notes,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = esmdsmrsmView == null ? "1234" : esmdsmrsmView.ESMName,
                        ErfID = erf.ErfID,
                        WorkorderID = erf.WorkorderID,
                        isDispatchNotes = 1
                    };
                    FarmerBrothersEntitites.NotesHistories.Add(notesHistory);

                    NotesHistory notesHistory1 = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = CurrentTime,
                        Notes = "[ERF]:  Status Updated from " + tempStatus + " to " + Status,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = esmdsmrsmView == null ? "1234" : esmdsmrsmView.ESMName,
                        ErfID = erf.ErfID,
                        WorkorderID = erf.WorkorderID
                    };
                    FarmerBrothersEntitites.NotesHistories.Add(notesHistory1);



                    int returnValue = FarmerBrothersEntitites.SaveChanges();

                    if (returnValue > 0)
                    {
                        message = "ERF Status Successfully Updated to " + Status;
                    }
                }
                else
                {
                    message = "Cannot Update the ERF Status. Please check the ERF Data or the CustomerZipCode";
                }
            }
            catch (Exception ex)
            {
                message = "There is a problem in ERF Shipping Update : " + ex;
            }

            updateStatus = "erfSave";

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = new { message = message, updateStatus = updateStatus };
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }



        public void DispatchMail(int workOrderId, int techId, bool isResponsible)
        {
            int returnValue = -1;
            TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, techId);
            StringBuilder salesEmailBody = new StringBuilder();
            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            string workOrderStatus = "";
            string oldTechName=string.Empty;
            if (workOrder != null)
            {
                if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0
                && string.Compare(workOrder.WorkorderCallstatus, "Invoiced", true) != 0
                && string.Compare(workOrder.WorkorderCallstatus, "Completed", true) != 0
                && string.Compare(workOrder.WorkorderCallstatus, "Attempting", true) != 0)
                {
                    if (isResponsible == true)
                    {
                        UpdateTechAssignedStatus(techId, workOrder, "Sent",out oldTechName, 0, -1);
                    }
                    else
                    {
                        UpdateTechAssignedStatus(techId, workOrder, "Sent",out oldTechName, - 1, 0);
                    }

                    StringBuilder subject = new StringBuilder();

                    subject.Append("Work Order ID#: ");
                    subject.Append(workOrder.WorkorderID);
                    subject.Append(" Customer: ");
                    subject.Append(workOrder.CustomerName);
                    subject.Append("ST: ");
                    subject.Append(workOrder.CustomerState);
                    subject.Append("Call Type: ");
                    subject.Append(workOrder.WorkorderCalltypeDesc);

                    string emailAddress = string.Empty;
                    int userId = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 0;
                    TECH_HIERARCHY techView = GetTechById(techId);
                    if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                    {
                        emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                    }
                    else
                    {
                        if (techView != null)
                        {
                            if (!string.IsNullOrEmpty(techView.RimEmail))
                            {
                                emailAddress = techView.RimEmail;
                            }

                            if (!string.IsNullOrEmpty(techView.EmailCC))
                            {
                                emailAddress += "#" + techView.EmailCC;
                            }
                        }
                    }


                    
                    if (!string.IsNullOrWhiteSpace(emailAddress))
                    {
                        bool result = SendWorkOrderMail(workOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], techId, MailType.DISPATCH, isResponsible, null);
                        if (result == true)
                        {
                            string emailDetails = @"Email sent to following recipients" + Environment.NewLine +
                                "EmaliCC:" + techView.EmailCC + Environment.NewLine +
                                "RimEmail:" + techView.RimEmail + Environment.NewLine;
                            workOrder.ResponsibleTechid = techHierarchyView.TechID;
                            workOrder.ResponsibleTechName = techHierarchyView.PreferredProvider;

                            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);
                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = emailDetails,
                                Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                                UserName = string.IsNullOrEmpty(oldTechName) ? "" : oldTechName,
                                isDispatchNotes = 1
                            };
                            workOrder.NotesHistories.Add(notesHistory);
                        }

                        returnValue = FarmerBrothersEntitites.SaveChanges();
                    }

                    workOrderStatus = workOrder.WorkorderCallstatus;
                }
            }
        }

        private bool SendWorkOrderMail(WorkOrder workOrder, string subject, string toAddress, string fromAddress, int? techId, MailType mailType, bool isResponsible, string additionalMessage, string mailFrom = "", bool isFromEmailCloserLink = false, string SalesEmailAddress = "")
        {
            Contact customer = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();
            int TotalCallsCount = FarmerBrothers.Models.CustomerModel.GetCallsTotalCount(FarmerBrothersEntitites, workOrder.CustomerID.ToString());

            List<FarmerBrothers.Models.CustomerNotesModel> CustomerNotesResults = new List<FarmerBrothers.Models.CustomerNotesModel>();
            int? custId = Convert.ToInt32(workOrder.CustomerID);
            var custNotes = FarmerBrothersEntitites.FBCustomerNotes.Where(c => c.CustomerId == custId && c.IsActive == true).ToList();

            string IsBillable = "";
            string ServiceLevelDesc = "";
            if (!string.IsNullOrEmpty(customer.BillingCode))
            {
                IsBillable = FarmerBrothers.Models.CustomerModel.IsBillableService(customer.BillingCode, TotalCallsCount);
                ServiceLevelDesc = FarmerBrothers.Models.CustomerModel.GetServiceLevelDesc(FarmerBrothersEntitites, customer.BillingCode);
            }
            else
            {
                IsBillable = " ";
                ServiceLevelDesc = " - ";
            }

            StringBuilder salesEmailBody = new StringBuilder();

            salesEmailBody.Append(@"<img src='cid:logo' width='15%' height='15%'>");

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            if (!string.IsNullOrWhiteSpace(additionalMessage))
            {
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("<BR>");
            }

            UrlHelper u = new UrlHelper(this.ControllerContext.RequestContext);
            string scheme = u.RequestContext.HttpContext.Request.Url.Scheme;
            string url = ConfigurationManager.AppSettings["DispatchResponseUrl"];
            string Redircturl = ConfigurationManager.AppSettings["RedirectResponseUrl"];
            string Closureurl = ConfigurationManager.AppSettings["CallClosureUrl"];
            //string finalUrl = string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=@response&isResponsible=" + isResponsible.ToString()));

            salesEmailBody.Append("<a href=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;></a>");
            if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
            {
                if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                {
                    TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
                    if (mailType == MailType.DISPATCH)
                    {
                        //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible + "\">ACCEPT</a>");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    }
                    if (workOrder.WorkorderCallstatus == "Pending Acceptance" && techView.FamilyAff != "SPT")
                    {
                        //salesEmailBody.Append("<a href=\"" + Redircturl + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible + "\">REDIRECT</a>");
                        string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));
                        salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    }
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=6&isResponsible=" + isResponsible.ToString())) + "\">START</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible.ToString())) + "\">ARRIVAL</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible + "\">REJECT</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=6&isResponsible=" + isResponsible + "\">START</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible + "\">ARRIVAL</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible + "\">COMPLETED</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible + "\">CLOSE WORK ORDER</a>");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=8&isResponsible=" + isResponsible.ToString())) + "\">SCHEDULE EVENT</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                }
            }
            else if (mailType == MailType.REDIRECTED)
            {
                //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible + "\">DISREGARD</a>");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            TECH_HIERARCHY tchView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
            if (tchView != null && tchView.FamilyAff.ToUpper() == "SPT")
            {
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append("Third Party Dispatch ");
                salesEmailBody.Append("</b></span>");
            }
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "TRANSMIT")
            {
                salesEmailBody.Append("<b>ADDITIONAL NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                //salesEmailBody.Append(Utility.GetStringWithNewLine(additionalMessage));
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("<BR>");
            }

            if (custNotes != null && custNotes.Count > 0)
            {
                salesEmailBody.Append("<b>CUSTOMER NOTES: </b>");
                salesEmailBody.Append(Environment.NewLine);
                foreach (var dbCustNotes in custNotes)
                {
                    salesEmailBody.Append("[" + dbCustNotes.UserName + "] : " + dbCustNotes.Notes + Environment.NewLine);
                }
                salesEmailBody.Append("<BR>");
            }

            if (!string.IsNullOrEmpty(additionalMessage) && mailFrom == "ESCALATION")
            {
                salesEmailBody.Append("<span style='color:#ff0000'><b>");
                salesEmailBody.Append("ESCALATION NOTES: ");
                salesEmailBody.Append(additionalMessage);
                salesEmailBody.Append("</b></span>");
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("CALL TIME: ");
            salesEmailBody.Append(workOrder.WorkorderEntryDate);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("Work Order ID#: ");
            salesEmailBody.Append(workOrder.WorkorderID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ERF#: ");
            salesEmailBody.Append(workOrder.WorkorderErfid);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Appointment Date: ");
            salesEmailBody.Append(workOrder.AppointmentDate);
            salesEmailBody.Append("<BR>");

            WorkorderSchedule ws = FarmerBrothersEntitites.WorkorderSchedules.Where(w => w.WorkorderID == workOrder.WorkorderID && (w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();
            string schedlDate = ws == null ? "" : ws.EventScheduleDate.ToString();

            if (workOrder.WorkorderCalltypeid == 1300)
            {
                Erf workorderERF = FarmerBrothersEntitites.Erfs.Where(ew => ew.ErfID == workOrder.WorkorderErfid).FirstOrDefault();
                schedlDate = workorderERF == null ? schedlDate : workorderERF.OriginalRequestedDate.ToString();
            }

            salesEmailBody.Append("Schedule Date: ");
            salesEmailBody.Append(schedlDate);


            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Service Level: ");
            salesEmailBody.Append(ServiceLevelDesc);
            salesEmailBody.Append("<BR>");


            string ServiceTier = customer == null ? "" : string.IsNullOrEmpty(customer.ProfitabilityTier) ? " - " :  customer.ProfitabilityTier;
            string paymentTerm = customer == null ? "" : (string.IsNullOrEmpty(customer.PaymentTerm) ? "" : customer.PaymentTerm);
            string PaymentTermDesc = "";
            if (!string.IsNullOrEmpty(paymentTerm))
            {
                JDEPaymentTerm paymentDesc = FarmerBrothersEntitites.JDEPaymentTerms.Where(c => c.PaymentTerm == paymentTerm).FirstOrDefault();
                PaymentTermDesc = paymentDesc == null ? "" : paymentDesc.Description;
            }
            else
            {
                PaymentTermDesc = "";
            }

            salesEmailBody.Append("Tier: ");
            salesEmailBody.Append(ServiceTier);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Payment Terms: ");
            salesEmailBody.Append(PaymentTermDesc);
            salesEmailBody.Append("<BR>");

            AllFBStatu priority = FarmerBrothersEntitites.AllFBStatus.Where(p => p.FBStatusID == workOrder.PriorityCode).FirstOrDefault();
            string priorityDesc = priority == null ? "" : priority.FBStatus;

            salesEmailBody.Append("Service Priority: ");
            salesEmailBody.Append(priorityDesc);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Billable: ");
            salesEmailBody.Append(IsBillable);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("Customer PO: ");
            salesEmailBody.Append(workOrder.CustomerPO);

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER INFORMATION: ");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CUSTOMER#: ");
            salesEmailBody.Append(workOrder.CustomerID);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.CustomerName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(customer.Address1);
            salesEmailBody.Append(",");
            salesEmailBody.Append(customer.Address2);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.CustomerCity);
            salesEmailBody.Append(",");
            salesEmailBody.Append(workOrder.CustomerState);
            salesEmailBody.Append(" ");
            salesEmailBody.Append(workOrder.CustomerZipCode);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append(workOrder.WorkorderContactName);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("PHONE: ");
            salesEmailBody.Append(workOrder.WorkorderContactPhone);
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("BRANCH: ");
            salesEmailBody.Append(customer.Branch);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("ROUTE#: ");
            salesEmailBody.Append(customer.Route);
            salesEmailBody.Append("<BR>");
            if (workOrder.FollowupCallID == 601 || workOrder.FollowupCallID == 602)
            {
                int? followupId = workOrder.FollowupCallID;
                AllFBStatu status = FarmerBrothersEntitites.AllFBStatus.Where(s => s.FBStatusID == followupId).FirstOrDefault();
                if (status != null && !string.IsNullOrEmpty(status.FBStatus))
                {
                    //salesEmailBody.Append("Follow Up Reason: ");
                    //salesEmailBody.Append(status.FBStatus);
                    if (workOrder.FollowupCallID == 601)
                        salesEmailBody.Append("Customer requesting an ETA phone call within the hour");
                    else if (workOrder.FollowupCallID == 602)
                        salesEmailBody.Append("Contact Customer Within The Hour");
                    salesEmailBody.Append("<BR>");
                }
            }
            salesEmailBody.Append("<span style='color:#ff0000'><b>");
            salesEmailBody.Append("LAST SALES DATE: ");
            salesEmailBody.Append(GetCustomerById(workOrder.CustomerID).LastSaleDate);
            salesEmailBody.Append("</b></span>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("HOURS OF OPERATION: ");
            salesEmailBody.Append(workOrder.HoursOfOperation);
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL CODES: ");
            salesEmailBody.Append("<BR>");


            foreach (WorkorderEquipmentRequested equipment in workOrder.WorkorderEquipmentRequesteds)
            {
                salesEmailBody.Append("CATEGORY: ");
                salesEmailBody.Append(equipment.Category);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("MODEL# : ");
                salesEmailBody.Append(equipment.Model);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("LOCATION: ");
                salesEmailBody.Append(equipment.Location);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("SYMPTOM: ");
                salesEmailBody.Append(equipment.Symptomid);
                salesEmailBody.Append("<BR>");

                WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                if (callType != null)
                {
                    salesEmailBody.Append("CALLTYPE: ");
                    salesEmailBody.Append(callType.CallTypeID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(callType.Description);
                    salesEmailBody.Append("<BR>");
                }

                Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                if (symptom != null)
                {
                    salesEmailBody.Append("SYMPTOM: ");
                    salesEmailBody.Append(symptom.SymptomID);
                    salesEmailBody.Append(" - ");
                    salesEmailBody.Append(symptom.Description);
                    salesEmailBody.Append("<BR>");
                }
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("CALL NOTES: ");
            salesEmailBody.Append("<BR>");
            IEnumerable<NotesHistory> histories = workOrder.NotesHistories.Where(n => n.AutomaticNotes == 0).OrderByDescending(n => n.EntryDate);

            foreach (NotesHistory history in histories)
            {
                //Remove Redirected/Rejected notes for 3rd Party Tech
                if (tchView != null && tchView.FamilyAff.ToUpper() == "SPT")
                {
                    if (history.Notes.ToLower().Contains("redirected") || history.Notes.ToLower().Contains("rejected") || history.Notes.ToLower().Contains("declined"))
                    {
                        continue;
                    }
                }

                salesEmailBody.Append(history.UserName);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.EntryDate);
                salesEmailBody.Append(" ");
                salesEmailBody.Append(history.Notes.Replace("\\n", " ").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\r", " "));
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("SERVICE HISTORY:");
            salesEmailBody.Append("<BR>");

            IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.
               Where(w => w.CustomerID == workOrder.CustomerID).OrderByDescending(ed => ed.WorkorderEntryDate).Take(3);

            //IEnumerable<WorkOrder> previousWorkOrders = FarmerBrothersEntitites.WorkOrders.Where(w => w.CustomerID == workOrder.CustomerID);
            foreach (WorkOrder previousWorkOrder in previousWorkOrders)
            {
                salesEmailBody.Append("Work Order ID#: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderID);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("ENTRY DATE: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderEntryDate);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("STATUS: ");
                salesEmailBody.Append(previousWorkOrder.WorkorderCallstatus);
                salesEmailBody.Append("<BR>");
                salesEmailBody.Append("CALL CODES: ");
                salesEmailBody.Append("<BR>");

                foreach (WorkorderEquipment equipment in previousWorkOrder.WorkorderEquipments)
                {
                    salesEmailBody.Append("MAKE: ");
                    salesEmailBody.Append(equipment.Manufacturer);
                    salesEmailBody.Append("<BR>");
                    salesEmailBody.Append("MODEL# : ");
                    salesEmailBody.Append(equipment.Model);
                    salesEmailBody.Append("<BR>");

                    WorkorderType callType = FarmerBrothersEntitites.WorkorderTypes.Where(w => w.CallTypeID == equipment.CallTypeid).FirstOrDefault();
                    if (callType != null)
                    {
                        salesEmailBody.Append("CALLTYPE: ");
                        salesEmailBody.Append(callType.CallTypeID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(callType.Description);
                        salesEmailBody.Append("<BR>");
                    }

                    Symptom symptom = FarmerBrothersEntitites.Symptoms.Where(s => s.SymptomID == equipment.Symptomid).FirstOrDefault();
                    if (symptom != null)
                    {
                        salesEmailBody.Append("SYMPTOM: ");
                        salesEmailBody.Append(symptom.SymptomID);
                        salesEmailBody.Append(" - ");
                        salesEmailBody.Append(symptom.Description);
                        salesEmailBody.Append("<BR>");
                    }
                }
                salesEmailBody.Append("<BR>");
            }

            salesEmailBody.Append("<BR>");
            salesEmailBody.Append("<BR>");

            salesEmailBody.Append("<a href=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;></a>");
            if ((mailType == MailType.DISPATCH || mailType == MailType.SPAWN) && techId.HasValue)
            {
                if (string.Compare(workOrder.WorkorderCallstatus, "Closed", true) != 0)
                {
                    TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
                    if (mailType == MailType.DISPATCH)
                    {
                        //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible + "\">ACCEPT</a>");
                        salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=0&isResponsible=" + isResponsible.ToString())) + "\">ACCEPT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    }
                    if (workOrder.WorkorderCallstatus == "Pending Acceptance" && techView.FamilyAff != "SPT")
                    {
                        //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible + "\">REDIRECT</a>");
                        string redirectFinalUrl = string.Format("{0}{1}&encrypt=yes", Redircturl, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible.ToString()));
                        salesEmailBody.Append("<a href=\"" + redirectFinalUrl + "\">REDIRECT</a>");
                        salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    }
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible.ToString())) + "\">REJECT</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=6&isResponsible=" + isResponsible.ToString())) + "\">START</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible.ToString())) + "\">ARRIVAL</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible.ToString())) + "\">COMPLETED</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible.ToString() + "&isBillable=" + (IsBillable == "True" ? "True" : "False"))) + "\">CLOSE WORK ORDER</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=1&isResponsible=" + isResponsible + "\">REJECT</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=6&isResponsible=" + isResponsible + "\">START</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=2&isResponsible=" + isResponsible + "\">ARRIVAL</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=3&isResponsible=" + isResponsible + "\">COMPLETED</a>");
                    //salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=7&isResponsible=" + isResponsible + "\">CLOSE WORK ORDER</a>");
                    salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=8&isResponsible=" + isResponsible.ToString())) + "\">SCHEDULE EVENT</a>");
                    salesEmailBody.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                }
            }
            else if (mailType == MailType.REDIRECTED)
            {
                //salesEmailBody.Append("<a href=\"" + url + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible + "\">DISREGARD</a>");
                salesEmailBody.Append("<a href=\"" + string.Format("{0}{1}&encrypt=yes", url, new Encrypt_Decrypt().Encrypt("workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=4&isResponsible=" + isResponsible.ToString())) + "\">DISREGARD</a>");
            }

            string contentId = Guid.NewGuid().ToString();
            string logoPath = Server.MapPath("~/img/mainlogo.jpg");
            salesEmailBody = salesEmailBody.Replace("cid:logo", "cid:" + contentId);

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString
               (salesEmailBody.ToString(), null, MediaTypeNames.Text.Html);

            LinkedResource inline = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
            inline.ContentId = contentId;
            avHtml.LinkedResources.Add(inline);

            var message = new MailMessage();

            message.AlternateViews.Add(avHtml);

            message.IsBodyHtml = true;
            message.Body = salesEmailBody.Replace("cid:logo", "cid:" + inline.ContentId).ToString();

            bool result = true;
            string mailTo = toAddress;
            string mailCC = string.Empty;
            if (!string.IsNullOrWhiteSpace(mailTo))
            {
                if (toAddress.Contains("#"))
                {
                    string[] mailCCAddress = toAddress.Split('#');

                    if (mailCCAddress.Count() > 0)
                    {
                        string[] CCAddresses = mailCCAddress[1].Split(';');
                        foreach (string address in CCAddresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.CC.Add(new MailAddress(address));
                            }
                        }
                        string[] addresses = mailCCAddress[0].Split(';');
                        foreach (string address in addresses)
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                message.To.Add(new MailAddress(address));
                            }
                        }
                    }
                }
                else
                {
                    string[] addresses = mailTo.Split(';');
                    foreach (string address in addresses)
                    {
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            message.To.Add(new MailAddress(address));
                        }
                    }
                }

                string IsNonFBCustomerParentId = ConfigurationManager.AppSettings["NonFBCustomerParentID"];
                if (customer.PricingParentID == IsNonFBCustomerParentId)
                {
                    message.CC.Clear();
                }

                message.From = new MailAddress(fromAddress);
                message.Subject = subject;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = 25;
                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }
        protected TECH_HIERARCHY GetTechById(int? techId)
        {
            TECH_HIERARCHY techView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(x => x.DealerId == techId).FirstOrDefault();
            return techView;
        }

        public bool UpdateTechAssignedStatus(int techId, WorkOrder workOrder, string assignedStatus,out string oldTechName,  int isResponsible = -1, int isAssist = -1)
        {
            oldTechName = string.Empty;
            bool result = false;
            WorkorderSchedule techWorkOrderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.Techid == techId).FirstOrDefault();

            DateTime currentTime = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites);

            if (isResponsible == -1 && isAssist == -1)
            {
                if (string.Compare(techWorkOrderSchedule.AssignedStatus, "Sent", 0) == 0)
                {
                    string notesMessage = "";
                    if (string.Compare(assignedStatus, "Accepted", 0) == 0)
                    {
                        notesMessage = "Work order Accepted by " + techWorkOrderSchedule.TechName;

                        if (techWorkOrderSchedule.PrimaryTech >= 0)
                        {
                            techWorkOrderSchedule.PrimaryTech = 1;
                        }
                        else if (techWorkOrderSchedule.AssistTech >= 0)
                        {
                            techWorkOrderSchedule.AssistTech = 1;
                        }
                        techWorkOrderSchedule.EntryDate = currentTime;
                        techWorkOrderSchedule.ScheduleDate = currentTime;
                        techWorkOrderSchedule.ModifiedScheduleDate = currentTime;
                    }
                    else if (string.Compare(assignedStatus, "Declined", true) == 0)
                    {
                        notesMessage = "Work order Rejected by " + techWorkOrderSchedule.TechName;
                    }
                    techWorkOrderSchedule.AssignedStatus = assignedStatus;

                    NotesHistory notesHistory = new NotesHistory()
                    {
                        AutomaticNotes = 1,
                        EntryDate = currentTime,
                        Notes = notesMessage,
                        Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                        UserName = UserName,
                        isDispatchNotes = 1
                    };
                    workOrder.NotesHistories.Add(notesHistory);
                    result = true;
                }
            }

            if (isResponsible >= 0)
            {
                TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, techId);
                if (techWorkOrderSchedule != null)
                {
                    techWorkOrderSchedule.PrimaryTech = Convert.ToInt16(isResponsible);
                    techWorkOrderSchedule.AssignedStatus = assignedStatus;
                    techWorkOrderSchedule.ModifiedScheduleDate = currentTime;
                }
                else
                {
                    IndexCounter scheduleCounter = Utility.GetIndexCounter("ScheduleID", 1);
                    scheduleCounter.IndexValue++;
                    //FarmerBrothersEntitites.Entry(scheduleCounter).State = System.Data.Entity.EntityState.Modified;

                    WorkorderSchedule newworkOrderSchedule = new WorkorderSchedule()
                    {
                        Scheduleid = scheduleCounter.IndexValue.Value,
                        Techid = Convert.ToInt32(techHierarchyView.TechID),
                        TechName = techHierarchyView.PreferredProvider,
                        WorkorderID = workOrder.WorkorderID,
                        TechPhone = techHierarchyView.AreaCode + techHierarchyView.ProviderPhone,
                        ServiceCenterName = techHierarchyView.BranchName,
                        ServiceCenterID = Convert.ToInt32(techHierarchyView.TechID),
                        FSMName = techHierarchyView.DSMName,
                        FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>(),
                        EntryDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites),
                        ScheduleDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites),
                        TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"],
                        PrimaryTech = Convert.ToInt16(isResponsible),
                        AssistTech = -1,
                        AssignedStatus = assignedStatus,
                        ModifiedScheduleDate = currentTime
                    };

                    workOrder.WorkorderSchedules.Add(newworkOrderSchedule);

                }

                bool redirected = false;                
                IEnumerable<WorkorderSchedule> primaryTechSchedules = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0);
                foreach (WorkorderSchedule workOrderSchedule in primaryTechSchedules)
                {
                    if ((string.Compare(workOrderSchedule.AssignedStatus, "Sent", true) == 0
                        //|| string.Compare(workOrderSchedule.AssignedStatus, "Declined", true) == 0
                        || string.Compare(workOrderSchedule.AssignedStatus, "Accepted", true) == 0
                        || string.Compare(workOrderSchedule.AssignedStatus, "Scheduled", true) == 0)
                        && workOrderSchedule.Techid != techId)
                    {
                        redirected = true;
                        workOrderSchedule.AssignedStatus = "Redirected";
                        workOrderSchedule.PrimaryTech = -1;
                        workOrderSchedule.ModifiedScheduleDate = currentTime;
                        oldTechName = workOrderSchedule.TechName;
                        if (workOrderSchedule.Techid != techId)
                        {
                            StringBuilder subject = new StringBuilder();

                            subject.Append("Call has been redirected! WO: ");
                            subject.Append(workOrder.WorkorderID);
                            subject.Append(" Customer: ");
                            subject.Append(workOrder.CustomerName);
                            subject.Append(" ST: ");
                            subject.Append(workOrder.CustomerState);
                            subject.Append(" Call Type: ");
                            subject.Append(workOrder.WorkorderCalltypeDesc);

                            string emailAddress = string.Empty;
                            Customer serviceCustomer = GetCustomerById(workOrderSchedule.Techid);
                            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseTestMails"]))
                            {
                                emailAddress = ConfigurationManager.AppSettings["TestEmail"];
                            }
                            else
                            {
                                if (serviceCustomer != null)
                                {
                                    emailAddress = serviceCustomer.Email;
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(emailAddress))
                            {
                                SendWorkOrderMail(workOrder, subject.ToString(), emailAddress, ConfigurationManager.AppSettings["DispatchMailFromAddress"], workOrderSchedule.Techid, MailType.REDIRECTED, false, "This Work Order has been redirected!");
                            }
                        }
                    }
                }

                string notes = string.Empty;
                if (redirected == true)
                {
                    notes = oldTechName + " redirected work order to " + techHierarchyView.PreferredProvider;
                }
                else
                {
                    notes = "Work order sent to " + techHierarchyView.PreferredProvider;
                }

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = notes,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = oldTechName == null ? Convert.ToString(System.Web.HttpContext.Current.Session["UserName"]) : oldTechName,
                    isDispatchNotes = 1
                };
                workOrder.NotesHistories.Add(notesHistory);

                //if (System.Web.HttpContext.Current.Session["UserId"] != null)
                //{

                //}              
                //else
                //{
                //    string computer_name = System.Net.Dns.GetHostEntry(Request.ServerVariables["remote_addr"]).HostName;
                //    string sytemInfo = Utility.GetClientSystemDetails();
                //    sytemInfo += "Computer Name: ";
                //    sytemInfo += computer_name;
                //    sytemInfo += Environment.NewLine;

                //    using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                //    {
                //        FBActivityLog log = new FBActivityLog();
                //        log.LogDate = DateTime.UtcNow;
                //        log.UserId = System.Web.HttpContext.Current.Session["UserId"] != null ? (int)System.Web.HttpContext.Current.Session["UserId"] : 1;
                //        log.ErrorDetails = "EXTERNAL_REQ: " + sytemInfo.ToString();
                //        entity.FBActivityLogs.Add(log);
                //        entity.SaveChanges();
                //    }
                //}               


                result = true;
            }

            if (isAssist >= 0)
            {
                TechHierarchyView techHierarchyView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, techId);

                //assist tech dispatch
                if (techWorkOrderSchedule != null)
                {
                    techWorkOrderSchedule.PrimaryTech = Convert.ToInt16(isAssist);
                    techWorkOrderSchedule.AssignedStatus = assignedStatus;
                    techWorkOrderSchedule.ModifiedScheduleDate = currentTime;
                }
                else
                {
                    IndexCounter scheduleCounter = Utility.GetIndexCounter("ScheduleID", 1);
                    scheduleCounter.IndexValue++;
                    //FarmerBrothersEntitites.Entry(scheduleCounter).State = System.Data.Entity.EntityState.Modified;

                    WorkorderSchedule newworkOrderSchedule = new WorkorderSchedule()
                    {
                        Scheduleid = scheduleCounter.IndexValue.Value,
                        Techid = Convert.ToInt32(techHierarchyView.TechID),
                        TechName = techHierarchyView.PreferredProvider,
                        WorkorderID = workOrder.WorkorderID,
                        TechPhone = techHierarchyView.ProviderPhone,
                        FSMName = techHierarchyView.DSMName,
                        FSMID = techHierarchyView.DSMId != 0 ? Convert.ToInt32(techHierarchyView.DSMId) : new Nullable<int>(),
                        EntryDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites),
                        ScheduleDate = Utility.GetCurrentTime(workOrder.CustomerZipCode, FarmerBrothersEntitites),
                        TeamLeadName = WebConfigurationManager.AppSettings["ManagerName"],

                        AssistTech = Convert.ToInt16(isAssist),
                        PrimaryTech = -1,
                        AssignedStatus = assignedStatus,
                        ModifiedScheduleDate = currentTime
                    };
                    workOrder.WorkorderSchedules.Add(newworkOrderSchedule);
                }

                string notes = string.Empty;
                notes = "Work order sent to " + techHierarchyView.PreferredProvider;

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = notes,
                    Userid = System.Web.HttpContext.Current.Session["UserId"] != null ? Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]) : 1234,
                    UserName = techHierarchyView.PreferredProvider,
                    isDispatchNotes = 1
                };
                workOrder.NotesHistories.Add(notesHistory);


                result = true;
            }

            int numberOfAssistAccepted = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted" && ws.AssistTech >= 0).Count();
            int numberOfAssistRejected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Declined" && ws.AssistTech >= 0).Count();
            int numberOfAssistDispatches = workOrder.WorkorderSchedules.Where(ws => ws.AssistTech >= 0).Count();
            int numberOfAssistRedirected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Redirected" && ws.AssistTech >= 0).Count();

            int numberOfPrimaryAccepted = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted" && ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryRejected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Declined" && ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryDispatches = workOrder.WorkorderSchedules.Where(ws => ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryRedirected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Redirected" && ws.PrimaryTech >= 0).Count();

            string currentStatus = workOrder.WorkorderCallstatus;
            if ((numberOfPrimaryDispatches > 0 || numberOfPrimaryAccepted > 0) && (numberOfAssistDispatches > 0 || numberOfAssistAccepted > 0))
            {
                if (numberOfPrimaryAccepted > 0 && numberOfAssistAccepted > 0)
                {
                    workOrder.WorkorderCallstatus = "Accepted";
                }
                else if (numberOfPrimaryAccepted > 0 || numberOfAssistAccepted > 0)
                {
                    workOrder.WorkorderCallstatus = "Accepted-Partial";
                }
                else
                {
                    workOrder.WorkorderCallstatus = "Pending Acceptance";
                }
            }
            else if (numberOfPrimaryDispatches > 0 && numberOfPrimaryAccepted > 0)
            {
                workOrder.WorkorderCallstatus = "Accepted";
            }
            else if (numberOfAssistDispatches > 0 && numberOfAssistAccepted > 0)
            {
                workOrder.WorkorderCallstatus = "Accepted";
            }
            else if (string.Compare(assignedStatus, "Declined", true) == 0)
            {
                workOrder.WorkorderCallstatus = "Open";
            }
            else if (string.Compare(assignedStatus, "Sent", true) == 0)
            {
                workOrder.WorkorderCallstatus = "Pending Acceptance";
            }

            WorkorderStatusLog statusLog = new WorkorderStatusLog() { StatusFrom = currentStatus, StatusTo = workOrder.WorkorderCallstatus, StausChangeDate = currentTime, WorkorderID = workOrder.WorkorderID };
            workOrder.WorkorderStatusLogs.Add(statusLog);

            return result;
        }

        private Customer GetCustomerById(int? customerId)
        {
            Customer customer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == customerId).FirstOrDefault();
            return customer;
        }



        #region TechniciansList               

        [EncryptedActionParameter]
        [AllowAnonymous]
        public ActionResult TechniciansList(int workOrderId, int techId, DispatchResponse response, bool isResponsible)
        {
            WorkorderSchedule techWorkOrderSchedule = FarmerBrothersEntitites.WorkorderSchedules.Where(ws => ws.WorkorderID == workOrderId && ws.Techid == techId).FirstOrDefault();
            if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Declined")
            {
                DispatchResponseModel dispatchModel = new DispatchResponseModel();
                dispatchModel.TechId = techId;
                dispatchModel.WorkOrderId = workOrderId;
                dispatchModel.Message = "Cannot Redirect, You Rejected the Call";
                return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
            }
            else if (techWorkOrderSchedule != null && techWorkOrderSchedule.AssignedStatus == "Redirected")
            {
                DispatchResponseModel dispatchModel = new DispatchResponseModel();
                dispatchModel.TechId = techId;
                dispatchModel.WorkOrderId = workOrderId;
                dispatchModel.Message = "Cannot Operate Event, Call Has been Redirected by You";
                return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
            }

            WorkOrder workOrd = FarmerBrothersEntitites.WorkOrders.Where(wo => wo.WorkorderID == workOrderId).FirstOrDefault();
            DateTime currentTime = Utility.GetCurrentTime(workOrd.CustomerZipCode, FarmerBrothersEntitites);


            IEnumerable<TechHierarchyView> Techlist = Utility.GetTechDataByBranchType(FarmerBrothersEntitites, null, null);

            List<TechHierarchyView> newTechlistCollection = new List<TechHierarchyView>();
            foreach (TechHierarchyView thv in Techlist)
            {
                //bool isTechAvailable = false;
                int tchId = Convert.ToInt32(thv.TechID);

                if (tchId != techId)
                {
                    if (!IsTechUnAvailable(tchId, currentTime))
                    {
                        newTechlistCollection.Add(thv);
                    }
                }
            }

            //var newTechlistCollection = Techlist.ToList();
            string requstUrl = string.Empty;

            ViewBag.id = workOrderId;
            ViewBag.response = (int)response;
            ViewBag.isResponsible = isResponsible;
            ViewBag.userId = techId;

            TechHierarchyView techhierarchy = new TechHierarchyView()
            {
                TechID = -1,
                PreferredProvider = "Please select Technician"
            };
            newTechlistCollection.Insert(0, techhierarchy);
            ViewBag.techlist = newTechlistCollection.Select(t => new { TechID = t.TechID, PreferredProvider = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.PreferredProvider.ToLower()) });
            ////ViewBag.id = Request.QueryString["workOrderId"];
            ////ViewBag.response = Request.QueryString["response"];
            ////ViewBag.isResponsible = Request.QueryString["isResponsible"];
            ////ViewBag.userId = Request.QueryString["userId"];

            return View();
        }


        public static bool IsTechUnAvailable(int techId, DateTime StartTime)
        {
            bool isAvilable = false;
            using (FarmerBrothersEntities FarmerBrothersEntitites = new FarmerBrothersEntities())
            {
                List<TechSchedule> holidays = (from sc in FarmerBrothersEntitites.TechSchedules
                                               join tech in FarmerBrothersEntitites.TECH_HIERARCHY on sc.TechId equals tech.DealerId
                                               where DbFunctions.TruncateTime(sc.ScheduleDate) == DbFunctions.TruncateTime(StartTime) && sc.TechId == techId
                                               && tech.SearchType == "SP" && tech.PostalCode != null
                                               select sc).ToList();

                if (holidays != null)
                {
                    foreach (TechSchedule holiday in holidays)
                    {
                        DateTime UnavailableStartDate = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + new DateTime().AddHours(Convert.ToDouble(holiday.ScheduleStartTime)).ToString("hh:mm tt"));
                        DateTime UnavailableEndDate = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + new DateTime().AddHours(Convert.ToDouble(holiday.ScheduleEndTime)).ToString("hh:mm tt"));

                        if ((UnavailableStartDate <= StartTime) && (UnavailableEndDate > StartTime))
                        {
                            isAvilable = true;
                        }
                    }
                }
            }
            return isAvilable;
        }

        #endregion

    }
}