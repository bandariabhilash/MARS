using System.Linq;
using FarmerBrothers.Data;
using System;
using System.Text.RegularExpressions;
using FarmerBrothers.Utilities;
using System.Data;

namespace FarmerBrothers.Models
{
    public class WorkorderSearchResultModel
    {
        public WorkorderSearchResultModel()
        {

        }
        public WorkorderSearchResultModel(DataRow dr, bool dummy)
        {
            string scheduleAssignedStatus = "";

            if (dr.Table.Columns.Contains("WorkorderID") && dr["WorkorderID"] != DBNull.Value)
            {
                WorkorderID = Convert.ToInt32(dr["WorkorderID"]);
            }
            if (dr.Table.Columns.Contains("description") && dr["description"] != DBNull.Value)
            {
                WorkorderCalltypeDesc = Convert.ToString(dr["description"]);
            }
            if (dr.Table.Columns.Contains("WorkorderCallstatus") && dr["WorkorderCallstatus"] != DBNull.Value)
            {
                WorkorderCallstatus = Convert.ToString(dr["WorkorderCallstatus"]);
            }
            if (dr.Table.Columns.Contains("fbstatus") && dr["fbstatus"] != DBNull.Value)
            {
                PriorityCode = Convert.ToString(dr["fbstatus"]);
            }
            if (dr.Table.Columns.Contains("ServiceLevelCode") && dr["ServiceLevelCode"] != DBNull.Value)
            {
                ServiceTier = Convert.ToString(dr["ServiceLevelCode"]);
                if (!string.IsNullOrEmpty(ServiceTier))
                {
                    ServiceTier = GetServiceTier(ServiceTier);
                }
                else
                {
                    ServiceTier = "Tier003  ";
                }
            }
            else
            {
                ServiceTier = "Tier003  ";
            }
            if (dr.Table.Columns.Contains("WorkorderEntryDate") && dr["WorkorderEntryDate"] != DBNull.Value)
            {
                WorkorderEntryDate = Convert.ToString(dr["WorkorderEntryDate"]);
            }
            if (dr.Table.Columns.Contains("ElapsedTime") && dr["ElapsedTime"] != DBNull.Value)
            {
                ElapsedTime = Convert.ToString(dr["ElapsedTime"]);
            }
            if (dr.Table.Columns.Contains("AppointmentDate") && dr["AppointmentDate"] != DBNull.Value)
            {
                AppointmentDate = Convert.ToDateTime(Convert.ToString(dr["AppointmentDate"])).Date.ToShortDateString(); ;

            }
            
            if (dr.Table.Columns.Contains("EventScheduleDate") && dr["EventScheduleDate"] != DBNull.Value)
            {
                EventScheduleDate = Convert.ToDateTime(Convert.ToString(dr["EventScheduleDate"])).Date.ToShortDateString(); ;

            }



            if (dr.Table.Columns.Contains("CustomerName") && dr["CustomerName"] != DBNull.Value)
            {
                CustomerName = Convert.ToString(dr["CustomerName"]);
            }
            if (dr.Table.Columns.Contains("CustomerCity") && dr["CustomerCity"] != DBNull.Value)
            {
                CustomerCity = Convert.ToString(dr["CustomerCity"]);
            }

            if (dr.Table.Columns.Contains("CustomerState") && dr["CustomerState"] != DBNull.Value)
            {
                CustomerState = Convert.ToString(dr["CustomerState"]);
            }
            if (dr.Table.Columns.Contains("CustomerZipCode") && dr["CustomerZipCode"] != DBNull.Value)
            {
                CustomerZipCode = Convert.ToString(dr["CustomerZipCode"]);
            }
            if (dr.Table.Columns.Contains("CustomerID") && dr["CustomerID"] != DBNull.Value)
            {
                CustomerID = Convert.ToInt32(dr["CustomerID"]);
            }
            if (dr.Table.Columns.Contains("CustomerPO") && dr["CustomerPO"] != DBNull.Value)
            {
                CustomerPO = dr["CustomerPO"].ToString();
            }

            if (dr.Table.Columns.Contains("DealerId") && dr["DealerId"] != DBNull.Value)
            {
                TechId = Convert.ToString(dr["DealerId"]);
            }
            if (dr.Table.Columns.Contains("TechnicianName") && dr["TechnicianName"] != DBNull.Value)
            {
                AssignedTech = Convert.ToString(dr["TechnicianName"]);
            }

            if (dr.Table.Columns.Contains("AssignedStatus") && dr["AssignedStatus"] != DBNull.Value)
            {
                scheduleAssignedStatus = Convert.ToString(dr["AssignedStatus"]);


                WorkOrderAcceptDate = String.IsNullOrEmpty(dr["ModifiedScheduleDate"].ToString()) ? null : Convert.ToDateTime(dr["ModifiedScheduleDate"]).ToString();
                ScheduledDate = String.IsNullOrEmpty(dr["ScheduleDate"].ToString()) ? null : Convert.ToDateTime(dr["ScheduleDate"]).ToString();

                if (scheduleAssignedStatus == "Accepted")
                {
                    TimeSpan acceptTimeDiff = Convert.ToDateTime(WorkOrderAcceptDate).Subtract(Convert.ToDateTime(WorkorderEntryDate));
                    AcceptElapsedTime = convertTimeSpanToDateTimeStringFormat(acceptTimeDiff);// string.Format("{0}:{1}:{2}", acceptTimeDiff.TotalHours, acceptTimeDiff.TotalMinutes, acceptTimeDiff.TotalSeconds);
                }

                TimeSpan dispatchTimeDiff = Convert.ToDateTime(ScheduledDate).Subtract(Convert.ToDateTime(WorkorderEntryDate));
                DispatchElapsedTime = convertTimeSpanToDateTimeStringFormat(dispatchTimeDiff);//string.Format("{0}:{1}:{2}", dispatchTimeDiff.TotalHours, dispatchTimeDiff.TotalMinutes, dispatchTimeDiff.TotalSeconds);

            }
            if (dr.Table.Columns.Contains("TechName"))
            {
                if (dr["TechName"] != DBNull.Value && (scheduleAssignedStatus == "Accepted" || scheduleAssignedStatus == "Scheduled"))
                {
                    AssignedTech = Convert.ToString(dr["TechName"]);
                }
            }
            if (dr.Table.Columns.Contains("TechPhone"))
            {
                if (dr["TechPhone"] != DBNull.Value && (scheduleAssignedStatus == "Accepted" || scheduleAssignedStatus == "Scheduled"))
                {
                    TechPhone = Convert.ToString(dr["TechPhone"]);
                }
            }
            if (dr.Table.Columns.Contains("ServiceCenterName"))
            {
                if (dr["ServiceCenterName"] != DBNull.Value && (scheduleAssignedStatus == "Accepted" || scheduleAssignedStatus == "Scheduled"))
                {
                    TechBranch = Convert.ToString(dr["ServiceCenterName"]);
                }
            }
            if (dr.Table.Columns.Contains("Address1") && dr["Address1"] != DBNull.Value)
            {
                Address1 = Convert.ToString(dr["Address1"]);
            }

        }
        public WorkorderSearchResultModel(DataRow dr)
        {
            if (dr["EventID"] != DBNull.Value)
            {
                WorkorderID = Convert.ToInt32(dr["EventID"]);
            }
            if (dr["ContactID"] != DBNull.Value)
            {
                CustomerID = Convert.ToInt32(dr["ContactID"]);
            }
            if (dr["ContactSearchType"] != DBNull.Value)
            {
                CustomerType = Convert.ToString(dr["ContactSearchType"]);
            }

            if (dr["FulfillmentStatus"] != DBNull.Value)
            {
                WorkorderCallstatus = Convert.ToString(dr["FulfillmentStatus"]);
            }
            if (dr["Address1"] != DBNull.Value)
            {
                Address1 = Convert.ToString(dr["Address1"]);
            }

            if (dr["EntryDate"] != DBNull.Value)
            {
                WorkorderEntryDate = Convert.ToDateTime(dr["EntryDate"]).ToString();
            }
            if (dr["CloseDate"] != DBNull.Value)
            {
                WorkorderCloseDate = Convert.ToDateTime(dr["CloseDate"]).ToString();
            }
            if (dr["MaxOfStartDateTime"] != DBNull.Value)
            {
                StartDateTime = Convert.ToDateTime(dr["MaxOfStartDateTime"]).ToString();
            }
            if (dr["MaxOfArrivalDateTime"] != DBNull.Value)
            {
                ArrivalDateTime = Convert.ToDateTime(dr["MaxOfArrivalDateTime"]).ToString();
            }
            if (dr["MaxOfCompletionDateTime"] != DBNull.Value)
            {
                CompletionDateTime = Convert.ToDateTime(dr["MaxOfCompletionDateTime"]).ToString();
            }
            if (dr["NoService"] != DBNull.Value)
            {
                NoService = Convert.ToString(dr["NoService"]);
            }

            if (dr["EventCallTypeID"] != DBNull.Value)
            {
                EventCallTypeID = Convert.ToString(dr["EventCallTypeID"]);
            }
            if (dr["CallTypeDesc"] != DBNull.Value)
            {
                WorkorderCalltypeDesc = Convert.ToString(dr["CallTypeDesc"]);
            }
            if (dr["CompanyName"] != DBNull.Value)
            {
                CustomerName = Convert.ToString(dr["CompanyName"]);
            }

            if (dr["City"] != DBNull.Value)
            {
                CustomerCity = Convert.ToString(dr["City"]);
            }
            if (dr["State"] != DBNull.Value)
            {
                CustomerState = Convert.ToString(dr["State"]);
            }
            if (dr["PostalCode"] != DBNull.Value)
            {
                CustomerZipCode = Convert.ToString(dr["PostalCode"]);
            }

            if (dr["FieldServiceManager"] != DBNull.Value)
            {
                FieldServiceManager = Convert.ToString(dr["FieldServiceManager"]);
            }
            if (dr["FSMJDE"] != DBNull.Value)
            {
                FSMJDE = Convert.ToString(dr["FSMJDE"]);
            }
            if (dr["PricingParentName"] != DBNull.Value)
            {
                PricingParentName = Convert.ToString(dr["PricingParentName"]);
            }

            //if (dr.Table.Columns.Contains("TotalUnitPrice") && dr["TotalUnitPrice"] != DBNull.Value)
            //{
            //    TotalUnitPrice = Convert.ToString(dr["TotalUnitPrice"]);
            //}
            //if (dr.Table.Columns.Contains("IsBillable") && dr["IsBillable"] != DBNull.Value)
            //{
            //    IsBillable = Convert.ToString(dr["IsBillable"]);
            //}

            if (dr["DeliveryDesc"] != DBNull.Value)
            {
                DeliveryDesc = Convert.ToString(dr["DeliveryDesc"]);
            }
            if (dr["ERFNO"] != DBNull.Value)
            {
                ERFNO = Convert.ToString(dr["ERFNO"]);
            }
            if (dr["TechId"] != DBNull.Value)
            {
                TechId = Convert.ToString(dr["TechId"]);
            }
            //if (dr["TechBranch"] != DBNull.Value)
            //{
            //    TechBranch = Convert.ToString(dr["TechBranch"]);
            //}


            if (dr["RepeatCallEvent"] != DBNull.Value)
            {
                RepeatcallEvent = Convert.ToString(dr["RepeatCallEvent"]);
            }
            if (dr["RepeatRepair"] != DBNull.Value)
            {
                RepeatRepairEvent = Convert.ToString(dr["RepeatRepair"]);
            }
            
            if (dr["EquipCount"] != DBNull.Value)
            {
                EquipCount = Convert.ToString(dr["EquipCount"]);
            }
            if (dr["DealerCompany"] != DBNull.Value)
            {
                DealerCompany = Convert.ToString(dr["DealerCompany"]);
            }

            if (dr["DealerCity"] != DBNull.Value)
            {
                DealerCity = Convert.ToString(dr["DealerCity"]);
            }
            if (dr["DealerState"] != DBNull.Value)
            {
                DealerState = Convert.ToString(dr["DealerState"]);
            }
            if (dr["CallTypeID"] != DBNull.Value)
            {
                CallTypeID = Convert.ToString(dr["CallTypeID"]);
            }
            if (dr["SymptomID"] != DBNull.Value)
            {
                SymptomID = Convert.ToString(dr["SymptomID"]);
            }
            if (dr["SolutionId"] != DBNull.Value)
            {
                SolutionId = Convert.ToString(dr["SolutionId"]);
            }
            if (dr["SystemId"] != DBNull.Value)
            {
                SystemId = Convert.ToString(dr["SystemId"]);
            }
            if (dr["SerialNo"] != DBNull.Value)
            {
                SerialNo = Convert.ToString(dr["SerialNo"]).Trim();
            }
            if (dr["ProductNo"] != DBNull.Value)
            {
                ProductNo = Convert.ToString(dr["ProductNo"]);
            }
            if (dr["Manufacturer"] != DBNull.Value)
            {
                Manufacturer = Convert.ToString(dr["Manufacturer"]);
            }
            if (dr["CategoryDesc"] != DBNull.Value)
            {
                EquipmentType = Convert.ToString(dr["CategoryDesc"]);
            }

            if(dr["ERFNO"] != DBNull.Value)
            {
                if (dr["ERFScheduleDate"] != DBNull.Value)
                {
                    AppointmentDate = Convert.ToDateTime(dr["ERFScheduleDate"]).ToString();
                }
            }
            else if(dr["AppointmentDate"] != DBNull.Value)
            {
                AppointmentDate = Convert.ToDateTime(dr["AppointmentDate"]).ToString();
            }            

            if (dr["EventScheduleDate"] != DBNull.Value)
            {
                EventScheduleDate = Convert.ToDateTime(dr["EventScheduleDate"]).ToString();
            }

            //if (dr["CategoryDesc"] != DBNull.Value)
            //{
            //    CategoryDesc = Convert.ToString(dr["CategoryDesc"]);
            //}
            if (dr["InvoiceNo"] != DBNull.Value)
            {
                InvoiceNo = Convert.ToString(dr["InvoiceNo"]);
            }
            if (dr["FamilyAff"] != DBNull.Value)
            {
                FamilyAff = Convert.ToString(dr["FamilyAff"]).ToString();
            }




            if (dr["ADT"] != DBNull.Value)
            {
                DriveTimeMin = Convert.ToString(dr["ADT"]);
            }
            if (dr["ONSite"] != DBNull.Value)
            {
                OnSiteTimeMin = Convert.ToString(dr["ONSite"]);
            }

            if (dr["CustomerRegion"] != DBNull.Value)
            {
                CustomerRegion = Convert.ToString(dr["CustomerRegion"]);
            }
            if (dr["BranchName"] != DBNull.Value)
            {
                BranchName = Convert.ToString(dr["BranchName"]);
            }
            if (dr["RegionNumber"] != DBNull.Value)
            {
                RegionNumber = Convert.ToString(dr["RegionNumber"]).ToString();
            }

            if (dr["CustomerBranch"] != DBNull.Value)
            {
                CustomerBranch = Convert.ToString(dr["CustomerBranch"]);
            }
            if (dr["Branch"] != DBNull.Value)
            {
                Branch = Convert.ToString(dr["Branch"]);
            }
            if (dr["ContactSearchType"] != DBNull.Value)
            {
                ContactSearchType = Convert.ToString(dr["ContactSearchType"]).ToString();
            }

            if (dr["ContactSearchDesc"] != DBNull.Value)
            {
                ContactSearchDesc = Convert.ToString(dr["ContactSearchDesc"]);
            }
            if (dr["RouteNumber"] != DBNull.Value)
            {
                RouteNumber = Convert.ToString(dr["RouteNumber"]).ToString();
            }

            if (dr["PPID"] != DBNull.Value)
            {
                PPID = Convert.ToString(dr["PPID"]);
            }
            if (dr["PPIDDESC"] != DBNull.Value)
            {
                PPIDDESC = Convert.ToString(dr["PPIDDESC"]).ToString();
            }
            if (dr["PricingParentID"] != DBNull.Value)
            {
                PricingParentID = Convert.ToString(dr["PricingParentID"]);
            }

            if (dr["FilterReplaced"] != DBNull.Value)
            {
                FilterReplaced = Convert.ToString(dr["FilterReplaced"]);
            }
            if (dr["FilterReplacedDate"] != DBNull.Value)
            {
                FilterReplacedDate = Convert.ToString(dr["FilterReplacedDate"]);
            }
            if (dr["NextFilterReplacementDate"] != DBNull.Value)
            {
                NextFilterReplacementDate = Convert.ToString(dr["NextFilterReplacementDate"]);
            }
            if (dr["WaterTested"] != DBNull.Value)
            {
                WaterTested = Convert.ToString(dr["WaterTested"]);
            }
            if (dr["HardnessRating"] != DBNull.Value)
            {
               HardnessRating = Convert.ToString(dr["HardnessRating"]);
            }

            if (dr.Table.Columns.Contains("ServicePriority") && dr["ServicePriority"] != DBNull.Value)
            {
                ServicePriority = Convert.ToString(dr["ServicePriority"]);
            }
            if (dr.Table.Columns.Contains("RescheduleReason") && dr["RescheduleReason"] != DBNull.Value)
            {
                RescheduleReason = Convert.ToString(dr["RescheduleReason"]);
            }

        }

        private string elapsedTime(int workorderId)
        {
            using (FarmerBrothersEntities FarmerBrothersEntities = new FarmerBrothersEntities())
            {
                return Utility.ElapsedTimeValue(FarmerBrothersEntities, workorderId);
            }
        }

        public WorkorderSearchResultModel(NonServiceworkorder workOrder, FarmerBrothersEntities FarmerBrothersEntities)
        {
            WorkorderID = workOrder.WorkOrderID;

            Contact customer = FarmerBrothersEntities.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();
            if (customer != null)
            {
                CustomerName = customer.CompanyName;
                CustomerCity = customer.City;
                CustomerState = customer.State;
                CustomerZipCode = customer.PostalCode;
                CustomerID = workOrder.CustomerID;
                SearchInNonServiceWorkOrder = true;

                WorkorderEntryDate = workOrder.CreatedDate == null ? null : workOrder.CreatedDate.ToString();

                if (workOrder.CreatedDate.HasValue)
                {
                    //WorkorderEntryDate = workOrder.WorkorderEntryDate.Value.ToString("MM/dd/yyyy hh:mm tt");
                    TimeSpan elapsedTime = DateTime.Now.Subtract(workOrder.CreatedDate.Value);
                    ElapsedTime = string.Format("{0}:{1}:{2}", elapsedTime.Days, elapsedTime.Hours, elapsedTime.Minutes);
                }

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
        public WorkorderSearchResultModel(WorkOrder workOrder/*, FarmerBrothersEntities FarmerBrothersEntities*/)
        {
            using (FarmerBrothersEntities FarmerBrothersEntities = new FarmerBrothersEntities())
            {
                Contact customer = FarmerBrothersEntities.Contacts.Where(c => c.ContactID == workOrder.CustomerID).FirstOrDefault();
                if (customer != null)
                {
                    if (!string.IsNullOrEmpty(customer.ServiceLevelCode))
                    {
                        ServiceTier = GetServiceTier(customer.ServiceLevelCode);
                    }
                    else
                    {
                        ServiceTier = "Tier003  ";
                    }
                }
                WorkorderID = workOrder.WorkorderID;
                WorkorderErfid = workOrder.WorkorderErfid;
                WorkorderCalltypeDesc = workOrder.WorkorderCalltypeDesc;
                WorkorderCallstatus = workOrder.WorkorderCallstatus;

                if (workOrder.PriorityCode.HasValue)
                {
                    AllFBStatu priority = FarmerBrothersEntities.AllFBStatus.Where(p => p.StatusFor == "Priority" && p.Active == 1 && p.FBStatusID == workOrder.PriorityCode.Value).FirstOrDefault();
                    if (priority != null)
                    {
                        PriorityCode = priority.FBStatus;
                    }
                }
                WorkorderEntryDate = workOrder.WorkorderEntryDate == null ? null : workOrder.WorkorderEntryDate.ToString();
                ElapsedTime = Utility.GetElapsedTime(FarmerBrothersEntities, WorkorderID);


                {
                    AppointmentDate = workOrder.AppointmentDate == null ? null : Convert.ToDateTime(workOrder.AppointmentDate).Date.ToShortDateString();
                }

                CustomerName = workOrder.CustomerName;
                CustomerCity = workOrder.CustomerCity;
                CustomerState = workOrder.CustomerState;
                CustomerZipCode = workOrder.CustomerZipCode;
                CustomerID = workOrder.CustomerID;

                string scheduleAssignedStatus = "";
                if (workOrder.WorkorderSchedules.Count > 0)
                {
                    WorkorderSchedule schedule = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Accepted" && ws.PrimaryTech >= 0).FirstOrDefault();
                    if (schedule != null)
                    {
                        AssignedTech = schedule.TechName;
                        if (schedule.TechPhone != null)
                        {
                            if (schedule.TechPhone.IndexOf("-") > 0)
                            {
                                schedule.TechPhone = schedule.TechPhone.Replace("-", "");
                            }
                            TechPhone = Utilities.Utility.FormatPhoneNumber(schedule.TechPhone);
                            TechBranch = schedule.ServiceCenterName;
                            FSMName = schedule.FSMName;
                            FSMPhone = Utilities.Utility.FormatPhoneNumber(schedule.FSMPhone);

                        }

                        scheduleAssignedStatus = schedule.AssignedStatus;
                        WorkOrderAcceptDate = schedule.ModifiedScheduleDate == null ? null : Convert.ToDateTime(schedule.ModifiedScheduleDate).ToString();
                        ScheduledDate = schedule.ScheduleDate == null ? null : Convert.ToDateTime(schedule.ScheduleDate).ToString();
                    }

                    else
                    {
                        WorkorderSchedule DispatchSchedule = workOrder.WorkorderSchedules.Where(ws => ws.AssignedStatus == "Sent" && ws.PrimaryTech >= 0).FirstOrDefault();
                        if (DispatchSchedule != null)
                        {
                            scheduleAssignedStatus = DispatchSchedule.AssignedStatus;
                            ScheduledDate = DispatchSchedule.ScheduleDate == null ? null : Convert.ToDateTime(DispatchSchedule.ScheduleDate).ToString();
                        }
                    }
                }

                if (scheduleAssignedStatus != "")
                {
                    if (scheduleAssignedStatus == "Accepted")
                    {
                        TimeSpan acceptTimeDiff = Convert.ToDateTime(WorkOrderAcceptDate).Subtract(Convert.ToDateTime(WorkorderEntryDate));
                        AcceptElapsedTime = convertTimeSpanToDateTimeStringFormat(acceptTimeDiff);// string.Format("{0}:{1}:{2}", acceptTimeDiff.TotalHours, acceptTimeDiff.TotalMinutes, acceptTimeDiff.TotalSeconds);
                    }

                    TimeSpan dispatchTimeDiff = Convert.ToDateTime(ScheduledDate).Subtract(Convert.ToDateTime(WorkorderEntryDate));
                    DispatchElapsedTime = convertTimeSpanToDateTimeStringFormat(dispatchTimeDiff);//string.Format("{0}:{1}:{2}", dispatchTimeDiff.TotalHours, dispatchTimeDiff.TotalMinutes, dispatchTimeDiff.TotalSeconds);


                }

                OriginalMarsWorkOrderId = workOrder.OriginalWorkorderid;

                if (workOrder.ParentWorkorderid.HasValue)
                {
                    ParentWorkOrderId = workOrder.ParentWorkorderid;
                }

                SpawnReason = "";
                if (workOrder.WorkorderDetails != null)
                {
                    if (workOrder.WorkorderDetails.Count > 0)
                    {
                        WorkorderDetail workorderDetail = workOrder.WorkorderDetails.ElementAt(0);
                        if (workorderDetail != null)
                        {
                            if (workorderDetail.SolutionId.HasValue)
                            {
                                Solution solution = FarmerBrothersEntities.Solutions.Where(s => s.SolutionId == workorderDetail.SolutionId.Value).FirstOrDefault();
                                if (solution != null)
                                {
                                    SpawnReason = solution.Description;
                                }
                            }

                            if (workorderDetail.SpawnReason.HasValue)
                            {

                                AllFBStatu spawnReason = FarmerBrothersEntities.AllFBStatus.Where(p => p.StatusFor == "Spawn Reason" && p.Active == 1 && p.FBStatusID == workorderDetail.SpawnReason.Value).FirstOrDefault();
                                if (spawnReason != null)
                                {
                                    SpawnReason = SpawnReason + "-" + spawnReason.FBStatus;
                                }
                            }
                        }
                    }
                }
            }

        }

        public static string convertTimeSpanToDateTimeStringFormatReport(TimeSpan timeSpanObj)
        {
            string dateTimeStr = "";
            int days = timeSpanObj.Days;
            int hours = timeSpanObj.Hours;
            int DaysToHours = (timeSpanObj.Days * 24) + hours;

            string Minutes = timeSpanObj.Minutes.ToString().Length == 1 ? "0" + timeSpanObj.Minutes : timeSpanObj.Minutes.ToString();
            string seconds = timeSpanObj.Seconds.ToString().Length == 1 ? "0" + timeSpanObj.Seconds : timeSpanObj.Seconds.ToString();

            dateTimeStr = string.Format("{0}:{1}:{2}", DaysToHours.ToString().Length == 1 ? "0" + DaysToHours : DaysToHours.ToString(), Minutes, seconds);

            return dateTimeStr;
        }

        public static string convertTimeSpanToDateTimeStringFormat(TimeSpan timeSpanObj)
        {
            string dateTimeStr = "";
            int days = timeSpanObj.Days;
            int hours = timeSpanObj.Hours;
            int DaysToHours = (timeSpanObj.Days * 24) + hours;

            int Minutes = timeSpanObj.Minutes;
            int seconds = timeSpanObj.Seconds;

            dateTimeStr = string.Format("{0}:{1}:{2}", DaysToHours, Minutes, seconds);

            return dateTimeStr;
        }

        public int WorkorderID { get; set; }
        public string WorkorderErfid { get; set; }
        public string WorkorderCalltypeDesc { get; set; }
        public string WorkorderCallstatus { get; set; }
        public string PriorityCode { get; set; }
        public string WorkorderEntryDate { get; set; }
        public string ElapsedTime { get; set; }

        public string OriginatorName { get; set; }
        public string CustomerName { get; set; }
        public string ServiceTier { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerZipCode { get; set; }
        public Nullable<int> CustomerID { get; set; }
        public string AssignedTech { get; set; }
        public string TechPhone { get; set; }
        public string TechBranch { get; set; }
        public string FSMName { get; set; }
        public string FSMPhone { get; set; }
        public int? OriginalMarsWorkOrderId { get; set; }
        public int? ParentWorkOrderId { get; set; }
        public string SpawnReason { get; set; }

        public bool SearchInNonServiceWorkOrder { get; set; }

        public string WorkOrderAcceptDate { get; set; }
        public string AcceptElapsedTime { get; set; }
        public string ScheduledDate { get; set; }
        public string DispatchElapsedTime { get; set; }


        //Newly added columsn in program status reprot 
        public string CustomerType { get; set; }
        public string CustomerRegion { get; set; }
        public string AppointmentDate { get; set; }
        public string EventScheduleDate { get; set; }
        public string WorkorderCloseDate { get; set; }
        public string StartDateTime { get; set; }
        public string ArrivalDateTime { get; set; }
        public string CompletionDateTime { get; set; }
        public string NoService { get; set; }
        public string ErfOriginalScheduleDate { get; set; }

        public string EventCallTypeID { get; set; }
        public string Address1 { get; set; }
        public string FieldServiceManager { get; set; }
        public string FSMJDE { get; set; }
        public string PricingParentName { get; set; }
        public string DeliveryDesc { get; set; }
        public string ERFNO { get; set; }
        public string TechId { get; set; }
        public string RepeatcallEvent { get; set; }
        public string RepeatRepairEvent { get; set; }
        public string EquipCount { get; set; }
        public string DealerCompany { get; set; }
        public string DealerCity { get; set; }
        public string DealerState { get; set; }
        public string CallTypeID { get; set; }
        public string SymptomID { get; set; }
        public string SolutionId { get; set; }
        public string SystemId { get; set; }
        public string SerialNo { get; set; }
        public string ProductNo { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerDesc { get; set; }
        public string EquipmentType { get; set; }
        public string CategoryDesc { get; set; }
        public string InvoiceNo { get; set; }
        public string FamilyAff { get; set; }
        public string DriveTimeMin { get; set; }
        public string OnSiteTimeMin { get; set; }
        public string BranchName { get; set; }


        public string RegionNumber { get; set; }
        public string CustomerBranch { get; set; }
        public string Branch { get; set; }
        public string ContactSearchType { get; set; }
        public string ContactSearchDesc { get; set; }
        public string RouteNumber { get; set; }

        public string Route { get; set; }
        public string PricingParentID { get; set; }
        public string PricingParentDescription { get; set; }

        public string DoNotPay { get; set; }
        public string ParentAccount { get; set; }

        //public string IsBillable { get; set; }
        //public string TotalUnitPrice { get; set; }
        public string ServicePriority { get; set; }
        public string PPID { get; set; }
        public string PPIDDESC { get; set; }

        public string CustomerMainContactName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerMainEmail { get; set; }


        public string OrderType { get; set; }
        public string ShipToBranch { get; set; }
        public string SiteReady { get; set; }
        public string EqpQty { get; set; }       
        public string EqpTotal { get; set; }
        public string ExpTotal { get; set; }
        public string ExpQty { get; set; }
        public string Total { get; set; }
        public string TotalNSV { get; set; }
        public string ApprovalStatus { get; set; }
        public string WOStatus { get; set; }
        public string WOClosedDate { get; set; }
        public string ContactName { get; set; }
        public string EqpType { get; set; }
        public string EqpName { get; set; }
        public string EqpCategoryName { get; set; }
        public string ExpType { get; set; }
        public string ExpName { get; set; }
        public string ExpCategoryName { get; set; }

        public string EqpInternalOrderType { get; set; }
        public string EqpVendorOrderType{ get; set; }
        public string ExpInternalOrderType{ get; set; }
        public string ExpVendorOrderType { get; set; }

        //Closure Filter Data
        public string FilterReplaced { get; set; }
        public string FilterReplacedDate { get; set; }
        public string NextFilterReplacementDate { get; set; }
        public string WaterTested { get; set; }
        public string HardnessRating { get; set; }

        public string RescheduleReason { get; set; }

        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }
         
        public string DispatchDate { get; set; }
        public string AcceptedDate { get; set; }
        public string DispatchTech { get; set; }

        public string EqpSerialNumber { get; set; }
        public string EqpOrderType { get; set; }
        public string EqpDepositInvoiceNumber { get; set; }
        public string EqpDepositAmount { get; set; }
        public string EqpFinalInvoiceNumber { get; set; }
        public string EqpInvoiceTotal { get; set; }
        public string CaseSaleStatus { get; set; }
        public string CustomerPO { get; set; }
    }
}