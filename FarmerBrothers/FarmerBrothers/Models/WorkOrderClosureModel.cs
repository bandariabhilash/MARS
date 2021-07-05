using System.Collections.Generic;
using System.Linq;
using System;
using FarmerBrothers.Data;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace FarmerBrothers.Models
{
    public class PhoneSolveModel
    {
        public string Description { get; set; }
    }

    public class SpecialClosureModel
    {
        public string Description { get; set; }
    }

    public class EmailModel
    {
        public string Description { get; set; }
    }

    public class MinutesModel
    {
        public string Description { get; set; }
    }

    public class WorkOrderClosureModel
    {
        public WorkOrderClosureModel()
        {
            //PhoneSolveList = new List<PhoneSolveModel>();
            //PhoneSolveList.Add(new PhoneSolveModel() { Description = "" });
            //PhoneSolveList.Add(new PhoneSolveModel() { Description = "Attempting" });
            //PhoneSolveList.Add(new PhoneSolveModel() { Description = "Attempted" });
            //PhoneSolveList.Add(new PhoneSolveModel() { Description = "Reviewed" });
            //PhoneSolveList.Add(new PhoneSolveModel() { Description = "Solved" });

            SpecialClosureList = new List<SpecialClosureModel>();
            EmailList = new List<EmailModel>();

            MinutesList = new List<MinutesModel>();
            MinutesList.Add(new MinutesModel() { Description = "00" });
            MinutesList.Add(new MinutesModel() { Description = "05" });
            MinutesList.Add(new MinutesModel() { Description = "10" });
            MinutesList.Add(new MinutesModel() { Description = "15" });
            MinutesList.Add(new MinutesModel() { Description = "20" });
            MinutesList.Add(new MinutesModel() { Description = "25" });
            MinutesList.Add(new MinutesModel() { Description = "30" });
            MinutesList.Add(new MinutesModel() { Description = "35" });
            MinutesList.Add(new MinutesModel() { Description = "40" });
            MinutesList.Add(new MinutesModel() { Description = "45" });
            MinutesList.Add(new MinutesModel() { Description = "50" });
            MinutesList.Add(new MinutesModel() { Description = "55" });
        }

        public void PopulateEmailList()
        {
            //EmailList.Add(new EmailModel() { Description = "Email to DSM" });
            EmailList.Add(new EmailModel() { Description = "First Contact ESM" });
            EmailList.Add(new EmailModel() { Description = "Second Contact CCM" });
            EmailList.Add(new EmailModel() { Description = "Third Contact RSM" });
            //EmailList.Add(new EmailModel() { Description = "Email to Mike Fraser" });
            //EmailList.Add(new EmailModel() { Description = "Email to Darryl McGee" });
            EmailList.Add(new EmailModel() { Description = "OTHER" });
            //EmailList.Add(new EmailModel() { Description = "Email to RSM" });
            
            EmailList.Insert(0, new EmailModel() { Description = "" });

        }
        //LG :: As per RAM modifided this method, it will contain only NSR,Cancellation,PhoneSolved in all wo status except closed.
        public void PopulateSpecialClosureList(WorkOrder workorder, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            string callStatus = workorder.WorkorderCallstatus;

            bool isBlankRequired = true;

            if (string.Compare(callStatus, "Closed", true) == 0)
            {
                SpecialClosureList.Add(new SpecialClosureModel() { Description = "No Service Required" });
                SpecialClosureList.Add(new SpecialClosureModel() { Description = "Cancellation" });
                //SpecialClosureList.Add(new SpecialClosureModel() { Description = "Parts to Sales" });
                SpecialClosureList.Add(new SpecialClosureModel() { Description = "Phone Solved" });
                SpecialClosureList = SpecialClosureList.OrderBy(sc => sc.Description).ToList();
                SpecialClosureList.Insert(0, new SpecialClosureModel() { Description = "" });
            }
            else
            {
                //SpecialClosureList.Add(new SpecialClosureModel() { Description = "No Service Required" });
                //if (string.Compare(callStatus, "Open", true) == 0)
                //{
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Cancellation" });
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Parts to Sales" });
                //    //LG :: As per Ram added new status to the SpecialClosureList on 08-05-2017
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Closed" });
                //    isBlankRequired = false;
                //}
                ////else if (string.Compare(callStatus, "Open-Review", true) == 0)
                ////{
                ////    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Cancellation" });
                ////    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Parts to Sales" });
                ////}
                //else if (string.Compare(callStatus, "Hold", true) == 0)
                //{
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Phone Solved" });
                //}
                //else if (string.Compare(callStatus, "Hold for AB", true) == 0)
                //{
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Cancellation" });
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Phone Solved" });
                //}
                //else if (string.Compare(callStatus, "Accepted", true) == 0)
                //{
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Phone Solved" });
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Parts to Sales" });
                //}
                //else if (string.Compare(callStatus, "Accepted-Partial", true) == 0)
                //{
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Phone Solved" });
                //}
                //else if (string.Compare(callStatus, "Attempting", true) == 0)
                //{
                //    SpecialClosureList.Add(new SpecialClosureModel() { Description = "Phone Solved" });
                //}

                SpecialClosureList.Add(new SpecialClosureModel() { Description = "No Service Required" });
                SpecialClosureList.Add(new SpecialClosureModel() { Description = "Cancellation" });                
                SpecialClosureList.Add(new SpecialClosureModel() { Description = "Phone Solved" });
                SpecialClosureList = SpecialClosureList.OrderBy(sc => sc.Description).ToList();
                SpecialClosureList.Insert(0, new SpecialClosureModel() { Description = "" });

            }

            WorkorderSchedule ws = workorder.WorkorderSchedules.Where(w => w.WorkorderID == workorder.WorkorderID
                                                               && (w.AssignedStatus == "Sent" || w.AssignedStatus == "Accepted" || w.AssignedStatus == "Scheduled")).FirstOrDefault();
            if (ws != null)
            {
                TECH_HIERARCHY techHView = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == ws.Techid).FirstOrDefault();

                if (techHView != null && techHView.FamilyAff == "SPT" &&
                     string.Compare(callStatus, "Closed") != 0
                         && string.Compare(callStatus, "Invoiced") != 0)
                {
                    List<string> NSRUserIds = ConfigurationManager.AppSettings["TPSPCloseNSRUserIds"].Split(';').ToList();
                    List<string> NSRUserNames = ConfigurationManager.AppSettings["TPSPCloseNSRUserNames"].Split(';').ToList();

                    string NSRUserNm = NSRUserIds.Where(x => x == System.Web.HttpContext.Current.Session["UserId"].ToString()).FirstOrDefault();

                    if (string.IsNullOrEmpty(NSRUserNm))
                    {
                        var item = SpecialClosureList.Single(x => x.Description == "No Service Required");
                        SpecialClosureList.Remove(item);
                    }
                }
            }
        }

        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<System.DateTime> ArrivalDateTime { get; set; }
        public Nullable<System.DateTime> CompletionDateTime { get; set; }
        [MaxLength(90, ErrorMessage = "Technician Name cannot be longer than 90 characters.")]
        public string ResponsibleTechName { get; set; }
        public Nullable<decimal> Mileage { get; set; }
        public int? PhoneSolveid { get; set; }
        public string SpecialClosure { get; set; }
        [MaxLength(90, ErrorMessage = "Customer Name cannot be longer than 90 characters.")]
        public string CustomerName { get; set; }
        [MaxLength(70, ErrorMessage = "Customer Email cannot be longer than 70 characters.")]
        public string CustomerEmail { get; set; }
        public byte[] CustomerSignature { get; set; }
        public string CustomerSignatureDetails { get; set; }
        public string CustomerSignatureUrl { get; set; }
        public string CustomerSignedBy { get; set; }
        public byte[] TechnicianSignature { get; set; }
        public string TechnicianSignatureDetails { get; set; }
        public string TechnicianSignatureUrl { get; set; }
        public string TravelHours;
        public string TravelMinutes;
        public string Email { get; set; }
        [MaxLength(50, ErrorMessage = "Invoice Number cannot be longer than 50 characters.")]
        public string InvoiceNo { get; set; }
        
        public bool WaterTested { get; set; }
        public bool FilterReplaced { get; set; }
        public List<string> HardnessRatingList { get; set; }
        public string HardnessRating { get; set; }

        public IList<WorkOrderManagementEquipmentModel> WorkOrderEquipments;
        public IList<AllFBStatu> PhoneSolveList;
        public IList<SpecialClosureModel> SpecialClosureList;
        public IList<MinutesModel> MinutesList;
        public IList<EmailModel> EmailList;
    }
}