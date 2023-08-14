using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FarmerBrothers.Models
{
    public enum WorkOrderSearchSubmitType
    {
        SAVE = 0,
        SEARCH = 1,
        UPDATE = 2,
        CANCEL = 3,
        RETRIEVESAVEDSEARCH = 4
    }

    public class TimeZone
    {
        public string TimeZoneValue { get; set; }
        public string TimeZoneName { get; set; }
    }

    public class TimeZoneModel
    {
        public string TimeZoneName { get; set; }
        [DisplayName("TimeZoneName")]
        public IList<TimeZone> TimeZones { get; set; }

    }

    public class ReportsModel
    {
        public List<Report> Reports { get; set; }
    }

    public class TechReportsModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<TECH_HIERARCHY> Techlist { get; set; }
        public List<TechHierarchyView> Technicianlist { get; set; }
        public int DealerId { get; set; }
        public string TechID { get; set; }
        public List<Technician> TechnicianAffs { get; set; }

        public IList<TechnicianSearchResultModel> SearchResults;
    }

    public class PMSchedulesModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string CustomerJDE { get; set; }
        public string TechJDE { get; set; }

        public string AccountNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public string CustomerName { get; set; }
        public string ContactPhone { get; set; }
        public string TechName { get; set; }
        public string IntervalType { get; set; }
        public int IntervalDuration { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }        
        public string Notes { get; set; }
        public string ProjectNumber { get; set; }
        public string EventContact { get; set; }
        public string TechType { get; set; }
        public string serviceCenter { get; set; }
        public string Catregory { get; set; }

        public IList<State> States;
        public IList<string> TechTypeList;
        public IList<BranchModel> ServiceCenterList;
        public List<CategoryModel> intervalTypeList { get; set; }

        public List<CategoryModel> TaggedCategories;

        public int Operation { get; set; }

        public IList<PMSchedulesSearchResultModel> SearchResults;
    }

    public class ClosurePartsModel
    {
        public DateTime? CloseDateStart { get; set; }
        public DateTime? CloseDateEnd { get; set; }
        public string JDENo { get; set; }
        public string EntryNo { get; set; }

        public IList<ClosurePartsSearchResultModel> SearchResults;
    }

    public class SerialNumberModel
    {
        public DateTime? CloseDateStart { get; set; }
        public DateTime? CloseDateEnd { get; set; }
        public string JDENo { get; set; }
        public string SerialNumber { get; set; }

        public IList<SerialNumberSearchResultModel> SearchResults;
    }

    public class TechAvailabilityReportsModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public IList<TechnicianAvailabilitySearchResultModel> SearchResults;
        public IList<TechnicianAvailabilitySearchResultModel> OnCallSearchResults;
        
    }

    public class FBDeltaCustomersNCCCallReportsModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public IList<FBDeltaCustomersNCCCallSearchResultModel> SearchResults;
    }

    public class Report
    {
        public string ReportType { get; set; }
        public List<string> ReportNames { get; set; }

    }
    public class WorkorderInvoiceModel : BaseModel
    {
        public string InvoiceNumber { get; set; }
        public int WorkOrderId { get; set; }
    }

    public class ClosestTechLookupModel
    {
        public string ZipCode { get; set; }
        public IList<BranchModel> Branches;
    }
    public class WorkorderSearchModel
    {
        public WorkorderSearchModel()
        {
            this.UtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMilliseconds;
        }

        private const char MULTISELECTDELIMETERCHAR = ',';
        private const string MULTISELECTDELIMETER = ",";

        private IList<string> technicianList = new List<string>();
        private IList<string> fsmList = new List<string>();
        private IList<string> tsmList = new List<string>();
        private IList<string> fsrLIst = new List<string>();

        private IList<string> TechnicianList { get { return technicianList; } }
        private IList<string> FsmList { get { return fsmList; } }
        private IList<string> TsmList { get { return tsmList; } }
        private IList<string> FsrLIst { get { return fsrLIst; } }

        public WorkOrderSearchSubmitType Operation { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ErfId { get; set; }
        public string WorkorderId { get; set; }
        public List<string> Status { get; set; }
        public List<string> WOTypes { get; set; }        
        public List<string> StatusList { get; set; }
        public string SerialNumber { get; set; }
        public int? FollowupCall { get; set; }
        public string AutoDispatched { get; set; }
        public List<string> WorkorderType { get; set; }
        public List<string> Priority { get; set; }
        public string FSR { get; set; }
        public string City { get; set; }
        public List<string> State { get; set; }
        public string Zipcode { get; set; }
        public string CoverageZone { get; set; }
        public string TSM { get; set; }
        public string TechType { get; set; }
        public int ServiceCompany { get; set; }
        public int? Technician { get; set; }
        public int? TechId { get; set; }
        public int? TeamLead { get; set; }
        public int FSM { get; set; }
        public string ProjectID { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? AppointmentDateFrom { get; set; }
        public DateTime? AppointmentDateTo { get; set; }

        public DateTime? ArrivalStartDate { get; set; }
        public DateTime? ArrivalEndDate { get; set; }
        public DateTime? CompletionStartDate { get; set; }
        public DateTime? CompletionEndDate { get; set; }
        public string ParentAccount { get; set; }
        public string OriginalWorkOrderId { get; set; }

        public bool SearchInNonServiceWorkOrder { get; set; }

        public List<ESMCCMRSMEscalation> EsmList { get; set; }
        public List<string> Esm { get; set; }

        [Required(ErrorMessage = "Search Name Required")]
        public string SavedSearchName { get; set; }
        public string SelectedSavedSearchName { get; set; }

        public List<string> TimeZone { get; set; }
        public string TimeZoneName { get; set; }
        public TimeZoneModel timeZoneModel { get; set; }
        public IList<WorkorderSavedSearch> SavedSearches;
        public IList<WorkorderSearchResultModel> SearchResults;
        public IList<WorkOrder> WorkOrderResults;
        public Contact SearchResult;
        public IList<AllFBStatu> PriorityList;
        public IList<AllFBStatu> WorkOrderStatusList;
        public IList<WorkorderType> WorkOrderTypes;
        public IList<State> States;
        public IList<string> AutoDispatchedList;
        public IList<string> TechTypeList;
        public IList<AllFBStatu> FollowUpCallList;
        public IList<BranchModel> ServiceCenterList;
        public IList<TechModel> TechniciansList;
        public IList<TeamLeadModel> TeamLeadList;
        public IList<TechModel> TechnicianIds;
        public IList<FsmModel> Fsms;
        public IList<TsmModel> Tsms;
        public IList<TimeZone> TimeZoneList;

        public double UtcOffset;

        public string CustomerPO { get; set; }

        public string CashSaleStatus { get; set; }
        public string ErfStatus { get; set; }
        public IList<CashSaleModel> CashSalesList { get; set; }
        public IList<ERFStatusModel> ERFStatusList { get; set; }

        public DateTime? ClosedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public void FromSavedSearch(WorkorderSavedSearch savedSearch)
        {
            CustomerId = savedSearch.Customerid.ToString();
            ErfId = savedSearch.Erfid.ToString();
            WorkorderId = savedSearch.WorkorderID.ToString();
            //OriginalWorkOrderId = savedSearch.OriginalWorkOrderID.ToString();

            if (savedSearch.WorkorderCallStatus != null)
            {
                Status = savedSearch.WorkorderCallStatus.Split(MULTISELECTDELIMETERCHAR).ToList();
            }
            else
            {
                Status = new List<string>();
            }
            SerialNumber = savedSearch.SerialNumber;
            DateFrom = savedSearch.DateFrom;
            FollowupCall = savedSearch.FollowupCall;
            if (savedSearch.AutoDispatch == null)
            {
                AutoDispatched = string.Empty;
            }
            else
            {
                AutoDispatched = savedSearch.AutoDispatch;
            }

            if (savedSearch.WorkorderType != null)
            {
                WorkorderType = savedSearch.WorkorderType.Split(MULTISELECTDELIMETERCHAR).ToList();
            }
            else
            {
                WorkorderType = new List<string>();
            }

            if (savedSearch.Priority != null)
            {
                Priority = savedSearch.Priority.Split(MULTISELECTDELIMETERCHAR).ToList();
            }
            else
            {
                Priority = new List<string>();
            }

            FSR = savedSearch.Fsr.ToString();
            DateTo = savedSearch.DateTo;
            City = savedSearch.City;
            if (savedSearch.State != null)
            {
                State = savedSearch.State.Split(MULTISELECTDELIMETERCHAR).ToList();
            }
            else
            {
                State = new List<string>();
            }

            Zipcode = savedSearch.ZipCode;
            CoverageZone = savedSearch.CoverageZoneIndex.ToString();
            TSM = savedSearch.Tsm.ToString();
            AppointmentDateFrom = savedSearch.ApptdateFrom;
            TechType = savedSearch.TechType;
            ServiceCompany = Convert.ToInt32(savedSearch.ServiceCompany);
            if (!string.IsNullOrWhiteSpace(savedSearch.Technician))
            {
                Technician = Convert.ToInt32(savedSearch.Technician);
            }
            TechId = savedSearch.Techid;
            if (savedSearch.Fsm.HasValue)
            {
                FSM = savedSearch.Fsm.Value;
            }
            AppointmentDateTo = savedSearch.ApptDateTo;
            SavedSearchName = savedSearch.SavedSearchName;
        }

        public WorkorderSavedSearch GetSavedSearch(WorkorderSavedSearch savedSearch)
        {
            savedSearch.Customerid = string.IsNullOrWhiteSpace(CustomerId) ? new Nullable<int>() : Int32.Parse(CustomerId);
            savedSearch.Erfid = string.IsNullOrWhiteSpace(ErfId) ? new Nullable<int>() : Int32.Parse(ErfId);
            savedSearch.WorkorderID = string.IsNullOrWhiteSpace(WorkorderId) ? new Nullable<int>() : Int32.Parse(WorkorderId);
            if (Status != null)
            {
                savedSearch.WorkorderCallStatus = string.Join(MULTISELECTDELIMETER,Status);
            }

            savedSearch.SerialNumber = SerialNumber;
            savedSearch.DateFrom = DateFrom;
            savedSearch.FollowupCall = FollowupCall;
            savedSearch.AutoDispatch = AutoDispatched;
            if (WorkorderType != null)
            {
                savedSearch.WorkorderType = string.Join(MULTISELECTDELIMETER, WorkorderType);
            }


            //savedSearch.OriginalWorkOrderID = string.IsNullOrWhiteSpace(OriginalWorkOrderId) ? new Nullable<int>() : Int32.Parse(OriginalWorkOrderId);

            if (Priority != null)
            {
                savedSearch.Priority = string.Join(MULTISELECTDELIMETER, Priority);
            }
            savedSearch.Fsr = string.IsNullOrWhiteSpace(FSR) ? new Nullable<int>() : Int32.Parse(FSR);
            savedSearch.DateTo = DateTo;
            savedSearch.City = City;
            if (State != null)
            {
                savedSearch.State = string.Join(MULTISELECTDELIMETER, State);
            }
            savedSearch.ZipCode = Zipcode;
            savedSearch.CoverageZoneIndex = string.IsNullOrWhiteSpace(CoverageZone) ? new Nullable<int>() : Int32.Parse(CoverageZone);
            savedSearch.Tsm = string.IsNullOrWhiteSpace(TSM) ? new Nullable<int>() : Int32.Parse(TSM);
            savedSearch.ApptdateFrom = AppointmentDateFrom;
            savedSearch.TechType = TechType;
            savedSearch.ServiceCompany = ServiceCompany.ToString();
            if (Technician.HasValue)
            {
                savedSearch.Technician = Technician.Value.ToString();
            }
            savedSearch.Techid = TechId;
            savedSearch.Fsm = FSM;
            savedSearch.ApptDateTo = AppointmentDateTo;
            savedSearch.SavedSearchName = SavedSearchName;
            return savedSearch;
        }
    }
}
