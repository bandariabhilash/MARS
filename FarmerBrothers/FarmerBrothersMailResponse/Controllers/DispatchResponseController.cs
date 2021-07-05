using FarmerBrothers.Data;
using System.Web.Mvc;
using System.Linq;
using System;
using System.Configuration;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Xml;
using Customer = FarmerBrothers.Data.Contact;


namespace FarmerBrothersMailResponse.Controllers
{
    public class DispatchResponseController : Controller
    {
        
        protected FarmerBrothersEntities FarmerBrothersEntitites;
        protected Customer client;

        public DispatchResponseController()
        {
            FarmerBrothersEntitites = new FarmerBrothersEntities();

            //var feastRequestInterceptor = new CustomInspectorBehavior();
            //feastLocationsClient = new FeastLocationsClient();
            //feastLocationsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(ConfigurationManager.AppSettings["FeastLocationsClient"]);
            //feastLocationsClient.Endpoint.EndpointBehaviors.Add(feastRequestInterceptor);
        }

        private Customer GetCustomerFromService(string customerId)
        {
            Customer customer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == Convert.ToInt32(customerId)).FirstOrDefault();
            return customer;
        }

        private static decimal GetDistance(string fromAddress, string toAddress)
        {
            double distance = 0;
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=");
                stringBuilder.Append("origins=");
                stringBuilder.Append(fromAddress);
                stringBuilder.Append("&destinations=");
                stringBuilder.Append(toAddress);
                stringBuilder.Append("&key=AIzaSyCjMfuakjLPeYGF2CLY56lqz40IH9UfxLM");
                //stringBuilder.Append("&key=AIzaSyB0w-ABC5reY1a7cyE_XMcl7_ztGdjcB5U");

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
            string query = @"SELECT * FROM feast_tech_hierarchy where Tech_Id = " + responsibleTechId.ToString();
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

                    USZipService.USZipSoapClient usZipClient = new USZipService.USZipSoapClient();
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
                }
            }
            return timeZone;
        }

        private void CopyWorkorderEquipments(WorkOrder workOrder)
        {
            if (workOrder != null)
            {
                workOrder.WorkorderEquipments.Clear();
                foreach (WorkorderEquipment requested in workOrder.WorkorderEquipments)
                {
                    WorkorderEquipment equipment = new WorkorderEquipment()
                    {
                        Assetid = requested.Assetid,
                        CallTypeid = requested.CallTypeid,
                        Category = requested.Category,
                        Location = requested.Location,
                        SerialNumber = requested.SerialNumber,
                        CatalogID = requested.CatalogID,
                        Model = requested.Model
                    };
                    workOrder.WorkorderEquipments.Add(equipment);
                }
            }
        }

        private bool UpdateTechAssignedStatus(int techId, WorkOrder workOrder, string assignedStatus)
        {
            bool result = false;
            WorkorderSchedule techWorkOrderSchedule = workOrder.WorkorderSchedules.Where(ws => ws.Techid == techId).FirstOrDefault();

            TimeZoneInfo newTimeZoneInfo = null;
            GetCustomerTimeZone(workOrder.CustomerZipCode, out newTimeZoneInfo);
            DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, newTimeZoneInfo);

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
                }
                else if (string.Compare(assignedStatus, "Declined", true) == 0)
                {
                    notesMessage = "Work order Rejected by " + techWorkOrderSchedule.TechName;
                }
                techWorkOrderSchedule.AssignedStatus = assignedStatus;
                techWorkOrderSchedule.ModifiedScheduleDate = currentTime;

                NotesHistory notesHistory = new NotesHistory()
                {
                    AutomaticNotes = 1,
                    EntryDate = currentTime,
                    Notes = notesMessage,
                    Userid = 1234, //TBD
                    UserName = "test", //TBD
                };
                workOrder.NotesHistories.Add(notesHistory);
                result = true;
            }

            int numberOfAssistAccepted = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted" && ws.AssistTech >= 0).Count();
            int numberOfAssistRejected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Declined" && ws.AssistTech >= 0).Count();
            int numberOfAssistDispatches = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Sent" && ws.AssistTech >= 0).Count();
            int numberOfAssistRedirected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Redirected" && ws.AssistTech >= 0).Count();

            int numberOfPrimaryAccepted = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted" && ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryRejected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Declined" && ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryDispatches = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Sent" && ws.PrimaryTech >= 0).Count();
            int numberOfPrimaryRedirected = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Redirected" && ws.PrimaryTech >= 0).Count();

            if (numberOfPrimaryAccepted <= 0)
            {
                if (string.Compare(assignedStatus, "Declined", true) == 0)
                {
                    workOrder.WorkorderCallstatus = "Open-Review";
                }
                else
                {
                    workOrder.WorkorderCallstatus = "Dispatching";
                }
            }
            else if (numberOfAssistAccepted > 0 || numberOfAssistDispatches > 0 || numberOfAssistRedirected > 0 || numberOfAssistRejected > 0)
            {
                if (numberOfAssistAccepted > 0)
                {
                    workOrder.WorkorderCallstatus = "Accepted";
                }
                else
                {
                    workOrder.WorkorderCallstatus = "Accepted – Partial";
                }
            }
            else
            {
                workOrder.WorkorderCallstatus = "Accepted";
            }
            return result;
        }

        public ActionResult DispatchResponse(int workOrderId, int techId, DispatchResponse response, bool isResponsible)
        {
            TechHierarchyView techHierarchyView = GetTechDataByResponsibleTechId(FarmerBrothersEntitites, techId);
            WorkOrder workOrder = FarmerBrothersEntitites.WorkOrders.Where(w => w.WorkorderID == workOrderId).FirstOrDefault();
            DispatchResponseModel dispatchModel = new DispatchResponseModel();
            dispatchModel.TechId = techId;
            dispatchModel.WorkOrderId = workOrderId;

            if (workOrder != null)
            {
                bool updatedStatus = false;
                switch (response)
                {
                    case FarmerBrothers.Data.DispatchResponse.ACCEPTED:
                        updatedStatus = UpdateTechAssignedStatus(techId, workOrder, "Accepted");
                        if (updatedStatus == true)
                        {
                            CopyWorkorderEquipments(workOrder);
                            workOrder.IsRejected = false;

                            if (workOrder.WorkorderDetails != null && workOrder.WorkorderDetails.Count > 0)
                            {
                                WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                                if (details != null)
                                {
                                    Customer serviceCustomer = GetCustomerFromService(techId.ToString());
                                    details.Mileage = GetDistance(workOrder.CustomerAddress + " " + workOrder.CustomerCity + " " + workOrder.CustomerState + " " + workOrder.CustomerZipCode,
                                                        serviceCustomer.Address1 + " " + serviceCustomer.City + " " + serviceCustomer.State + " " + serviceCustomer.PostalCode);
                                }
                            }

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
                        updatedStatus = UpdateTechAssignedStatus(techId, workOrder, "Declined");
                        if (updatedStatus == true)
                        {
                            workOrder.IsRejected = false;

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
                    case FarmerBrothers.Data.DispatchResponse.ARRIEVED:
                        if (workOrder != null
                            && (string.Compare(workOrder.WorkorderCallstatus, "Accepted", true) == 0
                                || string.Compare(workOrder.WorkorderCallstatus, "Accepted-Partial", true) == 0))
                        {
                            TimeZoneInfo newTimeZoneInfo = null;
                            GetCustomerTimeZone(workOrder.CustomerZipCode, out newTimeZoneInfo);
                            DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, newTimeZoneInfo);

                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = techHierarchyView.PreferredProvider + " arrived at customer location",
                                Userid = 1234, //TBD
                                UserName = "test", //TBD
                            };
                            workOrder.NotesHistories.Add(notesHistory);
                            if (workOrder.WorkorderDetails != null && workOrder.WorkorderDetails.Count > 0)
                            {
                                WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                                if (details != null)
                                {
                                    details.ArrivalDateTime = currentTime;
                                }

                                workOrder.WorkorderCallstatus = "On Site";

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
                            dispatchModel.Message = "Work Order is not yet Accepted!";
                        }
                        break;
                    case FarmerBrothers.Data.DispatchResponse.COMPLETED:
                        if (workOrder != null
                            && string.Compare(workOrder.WorkorderCallstatus, "On Site", true) == 0)
                        {
                            TimeZoneInfo newTimeZoneInfo = null;
                            GetCustomerTimeZone(workOrder.CustomerZipCode, out newTimeZoneInfo);
                            DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, newTimeZoneInfo);

                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = "Work order completed by " + techHierarchyView.PreferredProvider,
                                Userid = 1234, //TBD
                                UserName = "test", //TBD
                            };
                            workOrder.NotesHistories.Add(notesHistory);
                            if (workOrder.WorkorderDetails != null)
                            {
                                WorkorderDetail details = workOrder.WorkorderDetails.ElementAt(0);
                                if (details != null)
                                {
                                    details.CompletionDateTime = currentTime;
                                }
                            }
                            workOrder.WorkorderCallstatus = "Completed";


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
                            dispatchModel.Message = "Work Order is not yet Started!";
                        }
                        break;
                    case FarmerBrothers.Data.DispatchResponse.ACKNOWLEDGED:
                        {
                            TimeZoneInfo newTimeZoneInfo = null;
                            GetCustomerTimeZone(workOrder.CustomerZipCode, out newTimeZoneInfo);
                            DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, newTimeZoneInfo);

                            NotesHistory notesHistory = new NotesHistory()
                            {
                                AutomaticNotes = 1,
                                EntryDate = currentTime,
                                Notes = "Work order redirect acknowledged by " + techHierarchyView.PreferredProvider,
                                Userid = 1234, //TBD
                                UserName = "test", //TBD
                            };

                            workOrder.NotesHistories.Add(notesHistory);
                            if (FarmerBrothersEntitites.SaveChanges() > 0)
                            {
                                dispatchModel.TechId = techId;
                                dispatchModel.WorkOrderId = workOrderId;
                                dispatchModel.Message = "Work Order redirect acknowledged!";
                            }
                        }
                        break;
                }
            }
            else
            {
                dispatchModel.Message = "There is an error in fetching the Work Order! Please contact support!";
            }

            return View("DispatchResponse", "_Layout_WithOutMenu", dispatchModel);
        }
	}
}