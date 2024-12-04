using DataAccess.Db;
using ServiceApis.Utilities;

namespace ServiceApis.Models
{
    public class WorkorderManagementModel
    {
        public CustomerModel Customer;
        public WorkOrder WorkOrder;
        public NotesModel Notes;
        public Erf Erf;
        public IList<VendorModelModel> SKUList;
        public decimal BillingTotal { get; set; }
        public string PaymentTransactionId { get; set; }

        public bool IsBillable { get; set; }
        public bool IsBillableFeed { get; set; }

        public bool IsServiceBillable { get; set; }

        //used to disable future dates
        public Nullable<System.DateTime> CurrentDateTime { get; set; }

        public bool ShowAllTech { get; set; }
        public bool isCustomerDashboard { get; set; }
        public string customerZipcode { get; set; }
        public bool IsOpen;
        public string SpawnReason { get; set; }
        public string Solution { get; set; }
        public string SpawnNotes { get; set; }

        public string NSRReason { get; set; }
        public string NSRNotes { get; set; }

        public int? AppointmentUpdateReason { get; set; }
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

        public IList<AllFbstatus> PriorityList;
        public IList<BrandName> BrandNames;

        public IList<BrandName> SelectedBrands;
        public string SelectedBrandIds;

        public string SalesNotificationNotes;
        public string SalesNotificationCode;

        public IList<WorkorderType> CallTypes;
        public IList<WorkorderType> ClosureCallTypes;
        public IList<EquipType> EquipmentTypes;

        public IList<State> States;

        
        public IList<WorkOrderManagementEquipmentModel> WorkOrderEquipments;
        public IList<WorkOrderManagementEquipmentModel> WorkOrderEquipmentsRequested;

        public IList<WorkOrderPartModel> WorkOrderParts;

        public IList<NewNotesModel> NewNotes;

        public IList<VendorModelModel> NonTaggedModels;

        public IList<VendorModelModel> TaggedModels;


        public IList<SystemInfo> SystemInfoes;

        public IList<Symptom> Symptoms;


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
        public Nullable<int> ReasonCode { get; set; }

        public string TravelDistance { get; set; }
        public string Distance { get; set; }
        public string TravelTime { get; set; }
        public decimal Labor { get; set; }
        public decimal PartsTotal { get; set; }
        public string TotalServiceQuote { get; set; }


        public string ShippingPriority { get; set; }

        public string Message { get; set; }

        public string Comments { get; set; }
        public WorkOrder FillCustomerData(WorkOrder entityWorkOrder, bool cleanDependentLists, FBContext context, Contact sCustomer = null)
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
                entityWorkOrder.CustomerId = new Nullable<int>(Convert.ToInt32(Customer.CustomerId));
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
            entityWorkOrder.Tsmphone = Customer.TSMPhone;
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
            entityWorkOrder.TechTeamLeadId = Customer.TechTeamLeadId;
            entityWorkOrder.TechType = Customer.TechType;
            entityWorkOrder.Tsm = Customer.TSM;
            entityWorkOrder.Tsmemail = Customer.TSMEmailAddress;
            entityWorkOrder.Tsmphone = Customer.TSMPhone;
            entityWorkOrder.Fsm = Customer.FSMName;
            entityWorkOrder.Fsmid = Customer.FSMId;
            entityWorkOrder.SecondaryTechBranch = Customer.SecondaryTechBranchId;
            entityWorkOrder.SecondaryTechName = Customer.SecondaryTechName;
            entityWorkOrder.SecondaryTechPhone = Customer.SecondaryTechPhone;
            entityWorkOrder.SecondaryTechid = Customer.SecondaryTechId;

            ZonePriority zonePriority = Utility.GetCustomerZonePriority(context, Customer.ZipCode);
            if (zonePriority != null)
            {
                entityWorkOrder.CoverageZone = zonePriority.ZoneIndex;
            }

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
                Zip zip = context.Zips.Where(z => z.Zip1 == customerZip).FirstOrDefault();

                if (zip != null)
                {
                    entityWorkOrder.WorkorderTimeZone = zip.TimeZone;
                }
            }
            return entityWorkOrder;
        }

        public static Boolean isUnknownCustomer(int customerId, FBContext context)
        {
            Contact contactObj = context.Contacts.Where(x => x.ContactId == customerId).FirstOrDefault();
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


    }

    public class WorkOrderManagementEquipmentModel
    {
        public WorkOrderManagementEquipmentModel()
        {
        }

        public WorkOrderManagementEquipmentModel(WorkorderEquipment workOrderEquipment, FBContext FormalBrothersEntities)
        {
            Manufacturer = workOrderEquipment.Manufacturer;
            Solution = workOrderEquipment.Solutionid;
            Temperature = workOrderEquipment.Temperature;
            Weight = workOrderEquipment.Weight;
            Ratio = workOrderEquipment.Ratio;
            Settings = workOrderEquipment.Settings;
            Counter = workOrderEquipment.WorkPerformedCounter;
            System = workOrderEquipment.Systemid;
            SymptomID = workOrderEquipment.Symptomid;
            QualityIssue = workOrderEquipment.QualityIssue;
            Email = workOrderEquipment.Email;
            WorkPerformed = workOrderEquipment.WorkDescription;
            if (workOrderEquipment.NoPartsNeeded.HasValue)
            {
                NoPartsNeeded = workOrderEquipment.NoPartsNeeded.Value;
            }
            else
            {
                NoPartsNeeded = false;
            }

            AssetId = workOrderEquipment.Assetid;
            Location = workOrderEquipment.Location;
            CatelogID = workOrderEquipment.CatalogId;
            Model = workOrderEquipment.Model;
            SerialNumber = workOrderEquipment.SerialNumber;
            SerialNumberManual = workOrderEquipment.SerialNumber;
            CallTypeID = workOrderEquipment.CallTypeid;
            Category = workOrderEquipment.Category;
            IsSlNumberImageExist = workOrderEquipment.IsSlNumberImageExist;

            Parts = new List<WorkOrderPartModel>();
            IQueryable<WorkorderPart> workOrderParts = FormalBrothersEntities.WorkorderParts.Where(wp => wp.AssetId == AssetId);
            foreach (WorkorderPart workOrderPart in workOrderParts)
            {
                WorkOrderPartModel workOrderPartModel = new WorkOrderPartModel(workOrderPart);
                Parts.Add(workOrderPartModel);
            }

            if (workOrderEquipment.WorkorderInstallationSurveys != null && workOrderEquipment.WorkorderInstallationSurveys.Count > 0)
            {
                WorkorderInstallationSurvey survey = workOrderEquipment.WorkorderInstallationSurveys.ElementAt(0);
                if (survey != null)
                {
                    NemwNumber = survey.NemwNumber;
                    ElectricalPhase = survey.ElectricalPhase;
                    MachineAmperage = survey.MachineAmperage;
                    UnitFitSpace = survey.UnitFitSpace;
                    Voltage = survey.Voltage;
                    CounterUnitSpace = survey.CounterUnitSpace;
                    WaterLine = survey.WaterLine;
                    AssetLocation = survey.AssetLocation;
                    Comments = survey.Comments;
                }
            }

            FeastMovementId = workOrderEquipment.FeastMovementid;
        }

        public WorkOrderManagementEquipmentModel(WorkorderEquipmentRequested workOrderEquipment, FBContext FarmerBrothersEntities)
        {
            Manufacturer = workOrderEquipment.Manufacturer;
            Solution = workOrderEquipment.Solutionid;
            Temperature = workOrderEquipment.Temperature;
            Weight = workOrderEquipment.Weight;
            Ratio = workOrderEquipment.Ratio;
            Settings = workOrderEquipment.Settings;
            Counter = workOrderEquipment.WorkPerformedCounter;
            System = workOrderEquipment.Systemid;
            SymptomID = workOrderEquipment.Symptomid;
            QualityIssue = workOrderEquipment.QualityIssue;
            Email = workOrderEquipment.Email;
            WorkPerformed = workOrderEquipment.WorkDescription;
            if (workOrderEquipment.NoPartsNeeded.HasValue)
            {
                NoPartsNeeded = workOrderEquipment.NoPartsNeeded.Value;
            }
            else
            {
                NoPartsNeeded = false;
            }

            AssetId = workOrderEquipment.Assetid;
            Location = workOrderEquipment.Location;
            CatelogID = workOrderEquipment.CatalogId;
            Model = workOrderEquipment.Model;
            SerialNumber = workOrderEquipment.SerialNumber;
            SerialNumberManual = workOrderEquipment.SerialNumber;
            CallTypeID = workOrderEquipment.CallTypeid;
            Category = workOrderEquipment.Category;
            IsSlNumberImageExist = workOrderEquipment.IsSlNumberImageExist;

            FeastMovementId = workOrderEquipment.FeastMovementid;
        }


        public int? SequenceNumber { get; set; }
        public int? AssetId { get; set; }
        public int? CallTypeID { get; set; }
        public string Category { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Location { get; set; }
        public string SerialNumber { get; set; }
        public string SerialNumberManual { get; set; }
        public string CatelogID { get; set; }
        public int? Solution { get; set; }
        public string Temperature { get; set; }
        public string Settings { get; set; }
        public string Counter { get; set; }
        public string Weight { get; set; }
        public string Ratio { get; set; }

        public int? System { get; set; }
        public int? SymptomID { get; set; }

        public bool? QualityIssue { get; set; }
        public string Email { get; set; }
        public string WorkPerformed { get; set; }
        public bool? NoPartsNeeded { get; set; }

        public int Installsurveyid { get; set; }
        public string NemwNumber { get; set; }
        public string ElectricalPhase { get; set; }
        public string MachineAmperage { get; set; }
        public string UnitFitSpace { get; set; }
        public string Voltage { get; set; }
        public string CounterUnitSpace { get; set; }
        public string WaterLine { get; set; }
        public string AssetLocation { get; set; }
        public string Comments { get; set; }
        public bool? IsSlNumberImageExist { get; set; }

        public int? FeastMovementId { get; set; }

        public IList<WorkOrderPartModel> Parts;
    }
   
}
