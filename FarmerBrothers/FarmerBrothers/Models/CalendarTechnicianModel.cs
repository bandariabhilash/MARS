using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{

    public class CalendarTechnicianModel
    {
        public List<CallGroup> OnCallList { get; set; }
        public List<Branch> BranchList { get; set; }
        public List<ResourceFields> ResourceList { get; set; }
        public List<ResourceFields1> ResourceList1 { get; set; }
        public string TimeZone { get; set; }
        public string techname { set; get; }
        public string BranchId { get; set; }
        public string TechName { get; set; }
        public string HiddenTechID { get; set; }
        public int TechID { get; set; }
        public string ResourceString { get; set; }
        public bool IsTechSchedule { get; set; }
        public string OnCallStartTime { get; set; }
        public string OnCallEndTime { get; set; }
    }
    public class ScheduleData
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Categorize { get; set; }
        public string RoomId { get; set; }
        public string TechnicianId { get; set; }
        public string RepTech { get; set; }
        public string Priority { get; set; }
        public bool AllDay { get; set; }
        public bool Recurrence { get; set; }
        public string RecurrenceRule { get; set; }
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public string Comments { get; set; }
        public bool IsAllDay { get; set; }
        public bool IsRecurrence { get; set; }
        //[System.ComponentModel.DefaultValue("UTC +00:00")]
        public string StartTimeZone
        { get; set; }
        //[System.ComponentModel.DefaultValue("UTC +00:00")]
        public string EndTimeZone { get; set; }
    }
    public class EditParams
    {
        public string key { get; set; }
        public string action { get; set; }
        public List<ScheduleData> added { get; set; }
        public List<ScheduleData> changed { get; set; }
        public List<ScheduleData> deleted { get; set; }
        public ScheduleData value { get; set; }
    }
    public class ResourceFields
    {
        public string Text { set; get; }
        public string Id { set; get; }
        public string GroupId { set; get; }
        public string Color { set; get; }
        public int WorkHourStart { set; get; }
        public int WorkHourEnd { set; get; }
        public List<string> CustomDays { set; get; }
        public bool IsChecked { get; set; }
    }

    public class ResourceFields1
    {
        public string Text { set; get; }
        public string Id { set; get; }
        public string GroupId { set; get; }
        public string Color { set; get; }
        public int WorkHourStart { set; get; }
        public int WorkHourEnd { set; get; }
        public List<string> CustomDays { set; get; }
        public bool IsChecked { get; set; }
    }

    // Define the below class, whenever the category related data is to be used in Scheduler.
    public class CategorizeSettings
    {
        public string Text { set; get; }
        public string Id { set; get; }
        public string FontColor { set; get; }
        public string Color { set; get; }
    }

    // Define the below class, whenever the priorities are to be used for Scheduler appointments.
    public class PrioritySettings
    {
        public string Text { set; get; }
        public string Value { set; get; }
    }

    // Define the below two classes (Appointment and Cells), if context menu items are to be used in the Scheduler.
    public class Appointment
    {
        public string Text { set; get; }
        public string Id { set; get; }
    }
    public class Cells
    {
        public string Text { set; get; }
        public string Id { set; get; }
        public string ParentId { set; get; }
    }

    // Define the below class, if timezone collection to be specified for Scheduler.
    public class TimezoneCollection
    {
        public string Text { set; get; }
        public string Id { set; get; }
        public string Value { set; get; }
    }
    public class Branch
    {
        public string BranchID { set; get; }
        public string BranchName { set; get; }
        public string TechType { set; get; }

    }
    public class CallGroup
    {
        public string OnCallGroupID { set; get; }
        public string OnCallGroupName { set; get; }
    }
    public class GetTechNames
    {
        public string techid { set; get; }
        public string techname { set; get; }
        //public string uniqueKey { set; get; }
        //public string text { set; get; }
    }

}