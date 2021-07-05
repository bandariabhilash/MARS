using FarmerBrothers.Data;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmerBrothers.Models
{
    public enum WorkOrderManagementSubmitType
    {
        NONE = 0,
        SAVE = 1,
        NOTIFYSALES = 2,
        OVERTIMEREQUEST = 3,
        PUTONHOLD = 4,
        UPDATEAPPOINTMENT = 5,
        COMPLETE = 6,
        CREATEWORKORDER = 7,
        CREATEFEASTMOVEMENT = 8
    }

    public class YesNoItem
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

   

    public class WorkorderManagementModel
    {
        public CustomerModel Customer;
        public WorkOrder WorkOrder;
        public NotesModel Notes;
        public Erf Erf;
        public FbWorkorderBillableSKUModel SKUModel;
        public IList<FbWorkorderBillableSKUModel> BillableSKUList;
        public IList<VendorModelModel> SKUList;
        public bool IsBillable { get; set; }
        public bool IsBillableFeed { get; set; }


        //used to disable future dates
        public Nullable<System.DateTime> CurrentDateTime { get; set; }

        public bool ShowAllTech { get; set; }
        public string customerZipcode { get; set; }
        public bool IsOpen;
        public string SpawnReason { get; set; }
        public string Solution { get; set; }
        public string SpawnNotes { get; set; }

        public string NSRReason { get; set; }
        public string NSRNotes { get; set; }

        public int? AppointmentUpdateReason { get; set; }
        public IList<AllFBStatu> AppointmentReasons;
        public IList<AllFBStatu> RemovalReasons;
        public int RemovalCount { get; set; }
        public DateTime? RemovalDate { get; set; }
        public bool RemovaAll { get; set; }
        public int? RemovalReason { get; set; }
        public string BeveragesSupplier { get; set; }
        public bool ClosingBusiness { get; set; }
        public bool FlavorOrTasteOfCoffee { get; set; }
        public bool EquipmentServiceReliabilityorResponseTime { get; set; }
        public bool EquipmentReliability { get; set; }
        public bool CostPerCup { get; set; }
        public bool ChangingGroupPurchasingProgram { get; set; }
        public bool ChangingDistributor { get; set; }
        public int? RowId { get; set; }

        public int? PhoneSolveId { get; set; }
        public int? PhoneSolveTechId { get; set; }

        public WorkOrderManagementSubmitType Operation { get; set; }
        public IList<EstimateApprovedByModel> EstimateApprovedByModels;
        public IList<AllFBStatu> PriorityList;
        public IList<BrandName> BrandNames;

        public IList<BrandName> SelectedBrands;
        public string SelectedBrandIds;

        public IList<AllFBStatu> SalesNotificationReasonCodes;
        public string SalesNotificationNotes;
        public string SalesNotificationCode;

        public IList<WorkorderType> CallTypes;
        public IList<WorkorderType> ClosureCallTypes;
        public IList<EquipType> EquipmentTypes;

        public IList<State> States;

        public IList<WorkOrderManagementNonSerializedModel> NonSerializedList;
        public IList<WorkOrderManagementEquipmentModel> WorkOrderEquipments;
        public IList<WorkOrderManagementEquipmentModel> WorkOrderEquipmentsRequested;

        public IList<WorkOrderPartModel> WorkOrderParts;

        public WorkOrderClosureModel Closure;

        


        public IList<CategoryModel> TaggedCategories;
        public IList<CategoryModel> NonTaggedCategories;

        public IList<KnownEquipmentModel> KnownEquipments;

        public IList<NewNotesModel> NewNotes;

        public IList<VendorDataModel> TaggedManufacturer;
        public IList<VendorDataModel> NonTaggedManufacturer;

        public IList<VendorDataModel> CloserNonTaggedManufacturer;
        public IList<VendorDataModel> CloserPartsOrSKUs;
        //public string[] CloserPartsOrSKUs;

        public IList<VendorModelModel> NonTaggedModels;
        public List<FBCBE> SerialNumberList;

        public IList<VendorModelModel> TaggedModels;

        public IList<BranchModel> Branches;

        public IList<TechnicianModel> Technicians;

        public IList<SystemInfo> SystemInfoes;

        public IList<Symptom> Symptoms;

        public IList<AMPSList> Amps;
        public IList<ElectricalPhaseList> ElectricalPhases;
        public IList<NEMANumberList> NmeaNumbers;
        public IList<VoltageList> Voltages;
        public IList<WaterLineList> WaterLines;
        public IList<YesNoItem> YesNoList;


        public bool IsNewPartsOrder { get; set; }
        public bool IsCustomerPartsOrder { get; set; }

        public IList<int> AssistTechIds;
        public int? ResponsibleTechId;
        public IList<int> BranchIds;

        public IList<Solution> Solutions;

        public int PartsShipTo { get; set; }


        public bool IsCustomerAlternateAddress { get; set; }
        public bool IsBranchAlternateAddress { get; set; }

        public string CustomerOtherPartsName { get; set; }
        public string CustomerOtherPartsContactName { get; set; }
        public string CustomerOtherPartsAddress1 { get; set; }
        public string CustomerOtherPartsAddress2 { get; set; }
        public string CustomerOtherPartsCity { get; set; }
        public string CustomerOtherPartsState { get; set; }
        public string CustomerOtherPartsZip { get; set; }
        public string CustomerOtherPartsPhone { get; set; }

        public string BranchOtherPartsName { get; set; }
        public string BranchOtherPartsContactName { get; set; }
        public string BranchOtherPartsAddress1 { get; set; }
        public string BranchOtherPartsAddress2 { get; set; }
        public string BranchOtherPartsCity { get; set; }
        public string BranchOtherPartsState { get; set; }
        public string BranchOtherPartsZip { get; set; }
        public string BranchOtherPartsPhone { get; set; }

        public string OverTimeRequestDescription { get; set; }

        public bool IsOnCallTechVisible { get; set; }
        public string OnCallTech { get; set; }


        public decimal? Estimate { get; set; }
        public decimal? FinalEstimate { get; set; }
        public bool IsEstimateApproved { get; set; }
        public string ThirdPartyPO { get; set; }

        public IList<AllFBStatu> RescheduleReasonCodesList;
        public Nullable<int> ReasonCode { get; set; }

        public WorkOrder FillCustomerData(WorkOrder entityWorkOrder, bool cleanDependentLists, FarmerBrothersEntities FarmerBrothersEntities, Contact sCustomer = null)
        {
            string workOrderstatus = entityWorkOrder.WorkorderCallstatus;

            string marketSegment = entityWorkOrder.MarketSegment;
            string programName = entityWorkOrder.ProgramName;
            string distributorName = entityWorkOrder.DistributorName;
            string serviceTier = entityWorkOrder.ServiceTier;


            foreach (var property in WorkOrder.GetType().GetProperties())
            {
                property.SetValue(entityWorkOrder, property.GetValue(WorkOrder));
            }

            if (cleanDependentLists)
            {
                entityWorkOrder.NonSerializeds = new HashSet<NonSerialized>();
                entityWorkOrder.NotesHistories = new HashSet<NotesHistory>();
                entityWorkOrder.PhoneSolveLogs = new HashSet<PhoneSolveLog>();
                entityWorkOrder.TechSchedules = new HashSet<TechSchedule>();
                entityWorkOrder.WorkOrderBrands = new HashSet<WorkOrderBrand>();
                entityWorkOrder.WorkorderDetails = new HashSet<WorkorderDetail>();
                entityWorkOrder.WorkorderEquipments = new HashSet<WorkorderEquipment>();
                entityWorkOrder.WorkorderEquipmentRequesteds = new HashSet<WorkorderEquipmentRequested>();
                entityWorkOrder.WorkorderImages = new HashSet<WorkorderImage>();
                entityWorkOrder.WorkorderInstallationSurveys = new HashSet<WorkorderInstallationSurvey>();
                entityWorkOrder.WorkorderNonAudits = new HashSet<WorkorderNonAudit>();
                entityWorkOrder.WorkorderParts = new HashSet<WorkorderPart>();
                entityWorkOrder.WorkorderReasonlogs = new HashSet<WorkorderReasonlog>();
                entityWorkOrder.WorkorderSchedules = new HashSet<WorkorderSchedule>();
            }

            if (string.Compare(WorkOrder.WorkorderCallstatus, "Pending Acceptance", true) == 0)
            {
                entityWorkOrder.WorkorderCallstatus = workOrderstatus;
            }

            entityWorkOrder.CustomerAddress = Customer.Address;
            entityWorkOrder.CustomerCity = Customer.City;
            entityWorkOrder.CustomerCustomerPreferences = Customer.CustomerPreference;
            if (!string.IsNullOrWhiteSpace(Customer.CustomerId))
            {
                entityWorkOrder.CustomerID = new Nullable<int>(Convert.ToInt32(Customer.CustomerId));
            }
            entityWorkOrder.CustomerMainContactName = Customer.MainContactName;
            entityWorkOrder.CustomerMainEmail = Customer.MainEmailAddress;
            entityWorkOrder.CustomerName = Customer.CustomerName;
            entityWorkOrder.CustomerPhone = Customer.PhoneNumber;
            entityWorkOrder.CustomerPhoneExtn = Customer.PhoneExtn;
            entityWorkOrder.CustomerState = Customer.State;
            entityWorkOrder.CustomerZipCode = Customer.ZipCode;
            entityWorkOrder.Tsm = Customer.TSM;
            entityWorkOrder.Fsm = Customer.FSMName;
            entityWorkOrder.TSMPhone = Customer.TSMPhone;
            entityWorkOrder.MarketSegment = Customer.MarketSegment;
            entityWorkOrder.ProgramName = Customer.ProgramName;
            entityWorkOrder.DistributorName = Customer.DistributorName;
            entityWorkOrder.ServiceTier = Customer.ServiceTier;

            entityWorkOrder.ResponsibleTechBranch = Customer.ResponsibleTechBranchId;
            //entityWorkOrder.ResponsibleTechName = Customer.ResponsibleTechName;
            entityWorkOrder.ResponsibleTechName = Customer.PreferredProvider;
            
            entityWorkOrder.ResponsibleTechPhone = Customer.ResponsibleTechPhone;
            //entityWorkOrder.ResponsibleTechid = Customer.ResponsibleTechId;
            entityWorkOrder.ResponsibleTechid = Customer.FBProviderID;
            
            entityWorkOrder.TechTeamLead = Customer.TechTeamLead; 
            entityWorkOrder.TechTeamLeadID = Customer.TechTeamLeadId;
            entityWorkOrder.TechType = Customer.TechType;
            entityWorkOrder.Tsm = Customer.TSM;
            entityWorkOrder.TSMEmail = Customer.TSMEmailAddress;
            entityWorkOrder.TSMPhone = Customer.TSMPhone;
            entityWorkOrder.Fsm = Customer.FSMName;
            entityWorkOrder.FSMID = Customer.FSMId;
            entityWorkOrder.SecondaryTechBranch = Customer.SecondaryTechBranchId;
            entityWorkOrder.SecondaryTechName = Customer.SecondaryTechName;
            entityWorkOrder.SecondaryTechPhone = Customer.SecondaryTechPhone;
            entityWorkOrder.SecondaryTechid = Customer.SecondaryTechId;

            ZonePriority zonePriority = Utility.GetCustomerZonePriority(FarmerBrothersEntities, Customer.ZipCode);
            if (zonePriority != null)
            {
                entityWorkOrder.CoverageZone = zonePriority.ZoneIndex;
            }

            //IEnumerable<FsmView> fsmViews = Utility.GetFsmData(FarmerBrothersEntities);
            //FsmView fsmView = fsmViews.Where(f => f.FSM_Name == Customer.FSMName).FirstOrDefault();
            //if (fsmView != null)
            //{
            //    entityWorkOrder.FSMID = Convert.ToInt32(fsmView.FSM_ID);
            //}

            if (Customer != null)
            {
                entityWorkOrder.MarketSegment = Customer.MarketSegment;
                entityWorkOrder.ProgramName = Customer.ProgramName;
                entityWorkOrder.DistributorName = Customer.DistributorName;
                entityWorkOrder.ServiceTier = Customer.ServiceTier;
            }
            else
            {
                entityWorkOrder.MarketSegment = marketSegment;
                entityWorkOrder.ProgramName = programName;
                entityWorkOrder.DistributorName = distributorName;
                entityWorkOrder.ServiceTier = serviceTier;
            }
            if (!string.IsNullOrEmpty(entityWorkOrder.CustomerZipCode))
            {
                string customerZip = entityWorkOrder.CustomerZipCode.Substring(0, 5);
                Zip zip = FarmerBrothersEntities.Zips.Where(z => z.ZIP1 == customerZip).FirstOrDefault();

                if (zip != null)
                {
                    entityWorkOrder.WorkorderTimeZone = zip.TimeZone;
                }
            }
            return entityWorkOrder;
        }

        public static Boolean isUnknownCustomer(int customerId, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (FarmerBrothersEntitites == null)
            {
                FarmerBrothersEntitites = new FarmerBrothersEntities();
            }

            Contact contactObj = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == customerId).FirstOrDefault();
            if (contactObj != null)
            {
                if (contactObj.IsUnknownUser == null)
                    return true;
                else if (contactObj.IsUnknownUser == 1)
                    return false;

                return false;
            }
            else
                return false;

            return true;
        }

        public static Boolean isTechSecurity(NotesModel model)
        {
            if (model.viewProp == "NormalWorkOrderView")
            {
                if (model.WorkOrderID == 0)
                {
                    if (((WorkorderManagementModel.isUnknownCustomer(Convert.ToInt32(model.CustomerID), null))))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static IEnumerable<TechHierarchyView> GetCompleteTechData(FarmerBrothersEntities FarmerBrothersEntities)
        {
            string query = string.Empty;
            
            query = @"select d.CompanyName AS PreferredProvider from TECH_HIERARCHY d where searchType='SP'  
                    and dealerID NOT IN (8888888,8888889,8888890,8888891,8888892,8888893,8888894,8888895,8888907,8888908,8888911,8888917,
                    8888918,8888941,8888942,8888945,8888953,9999999,8888897,9999995,9999998,8888980,8888981,8888984,9990061,9990062,9990065) group by CompanyName order by PreferredProvider asc";
            
            return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query);
        }

    }

}