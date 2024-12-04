using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Db;

public partial class FBContext : DbContext
{
    public FBContext()
    {
    }

    public FBContext(DbContextOptions<FBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AbhiFetcoDatum> AbhiFetcoData { get; set; }

    public virtual DbSet<AgentDispatchLog> AgentDispatchLogs { get; set; }

    public virtual DbSet<AllFbstatus> AllFbstatuses { get; set; }

    public virtual DbSet<Ampslist> Ampslists { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<BillingItem> BillingItems { get; set; }

    public virtual DbSet<BranchEsm> BranchEsms { get; set; }

    public virtual DbSet<BrandName> BrandNames { get; set; }

    public virtual DbSet<CallTypeSymptomSolutionMaster> CallTypeSymptomSolutionMasters { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CcinvoiceDetail> CcinvoiceDetails { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<ContactPmuploadsAll> ContactPmuploadsAlls { get; set; }

    public virtual DbSet<Contingent> Contingents { get; set; }

    public virtual DbSet<ContingentDetail> ContingentDetails { get; set; }

    public virtual DbSet<CustomCriterion> CustomCriteria { get; set; }

    public virtual DbSet<ElectricalPhaseList> ElectricalPhaseLists { get; set; }

    public virtual DbSet<EquipType> EquipTypes { get; set; }

    public virtual DbSet<Erf> Erves { get; set; }

    public virtual DbSet<ErfWorkorderLog> ErfWorkorderLogs { get; set; }

    public virtual DbSet<ErfbranchDetail> ErfbranchDetails { get; set; }

    public virtual DbSet<ErforderType> ErforderTypes { get; set; }

    public virtual DbSet<Esmccmrsmescalation> Esmccmrsmescalations { get; set; }

    public virtual DbSet<Esmdsmrsm> Esmdsmrsms { get; set; }

    public virtual DbSet<EstimateEscalation> EstimateEscalations { get; set; }

    public virtual DbSet<FbBillableSku> FbBillableSkus { get; set; }

    public virtual DbSet<FbPrimaryTechnician> FbPrimaryTechnicians { get; set; }

    public virtual DbSet<FbPrimaryTechnicians1> FbPrimaryTechnicians1s { get; set; }

    public virtual DbSet<FbRole> FbRoles { get; set; }

    public virtual DbSet<FbUserMaster> FbUserMasters { get; set; }

    public virtual DbSet<FbWorkOrderSku> FbWorkOrderSkus { get; set; }

    public virtual DbSet<FbactivityLog> FbactivityLogs { get; set; }

    public virtual DbSet<FbbillableFeed> FbbillableFeeds { get; set; }

    public virtual DbSet<FbcallReason> FbcallReasons { get; set; }

    public virtual DbSet<Fbcbe> Fbcbes { get; set; }

    public virtual DbSet<FbclosurePart> FbclosureParts { get; set; }

    public virtual DbSet<FbcustomerNote> FbcustomerNotes { get; set; }

    public virtual DbSet<FbcustomerServiceDistribution> FbcustomerServiceDistributions { get; set; }

    public virtual DbSet<FbdailyContact> FbdailyContacts { get; set; }

    public virtual DbSet<Fbequipment> Fbequipments { get; set; }

    public virtual DbSet<Fberfequipment> Fberfequipments { get; set; }

    public virtual DbSet<Fberfexpendable> Fberfexpendables { get; set; }

    public virtual DbSet<Fberfpo> Fberfpos { get; set; }

    public virtual DbSet<FberftransactionType> FberftransactionTypes { get; set; }

    public virtual DbSet<Fbexpendable> Fbexpendables { get; set; }

    public virtual DbSet<Fbfunctionality> Fbfunctionalities { get; set; }

    public virtual DbSet<Fbreport> Fbreports { get; set; }

    public virtual DbSet<FbroleFunction> FbroleFunctions { get; set; }

    public virtual DbSet<Fbsku> Fbskus { get; set; }

    public virtual DbSet<FbuserReport> FbuserReports { get; set; }

    public virtual DbSet<FeastMovement> FeastMovements { get; set; }

    public virtual DbSet<FetcoCustomer> FetcoCustomers { get; set; }

    public virtual DbSet<HolidayList> HolidayLists { get; set; }

    public virtual DbSet<IndexCounter> IndexCounters { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<JdepaymentTerm> JdepaymentTerms { get; set; }

    public virtual DbSet<NarfNewAccount> NarfNewAccounts { get; set; }

    public virtual DbSet<NarfTradeReference> NarfTradeReferences { get; set; }

    public virtual DbSet<NemanumberList> NemanumberLists { get; set; }

    public virtual DbSet<NoAutoEmailZipCode> NoAutoEmailZipCodes { get; set; }

    public virtual DbSet<NoAutomaticEmailContract> NoAutomaticEmailContracts { get; set; }

    public virtual DbSet<NonFbcustomer> NonFbcustomers { get; set; }

    public virtual DbSet<NonSerialized> NonSerializeds { get; set; }

    public virtual DbSet<NonServiceworkorder> NonServiceworkorders { get; set; }

    public virtual DbSet<NotesHistory> NotesHistories { get; set; }

    public virtual DbSet<OnCallGroup> OnCallGroups { get; set; }

    public virtual DbSet<PhoneSolveLog> PhoneSolveLogs { get; set; }

    public virtual DbSet<PricingDetail> PricingDetails { get; set; }

    public virtual DbSet<PricingType> PricingTypes { get; set; }

    public virtual DbSet<Privilege> Privileges { get; set; }

    public virtual DbSet<RemovalSurvey> RemovalSurveys { get; set; }

    public virtual DbSet<Sku> Skus { get; set; }

    public virtual DbSet<Solution> Solutions { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<StateTax> StateTaxes { get; set; }

    public virtual DbSet<Symptom> Symptoms { get; set; }

    public virtual DbSet<SystemInfo> SystemInfos { get; set; }

    public virtual DbSet<TechHierarchy> TechHierarchies { get; set; }

    public virtual DbSet<TechHierarchyBackup> TechHierarchyBackups { get; set; }

    public virtual DbSet<TechOnCall> TechOnCalls { get; set; }

    public virtual DbSet<TechSchedule> TechSchedules { get; set; }

    public virtual DbSet<ThirdPartyContractMaintenance> ThirdPartyContractMaintenances { get; set; }

    public virtual DbSet<ThirdpartyConMaintenanceZonerate> ThirdpartyConMaintenanceZonerates { get; set; }

    public virtual DbSet<TmpBlackBerryNsr> TmpBlackBerryNsrs { get; set; }

    public virtual DbSet<TmpBlackBerryScfassetInfo> TmpBlackBerryScfassetInfos { get; set; }

    public virtual DbSet<TmpBlackBerryScfinvoiceInfo> TmpBlackBerryScfinvoiceInfos { get; set; }

    public virtual DbSet<TmpBlackBerryScfpartsInfo> TmpBlackBerryScfpartsInfos { get; set; }

    public virtual DbSet<TmpInvTransfer> TmpInvTransfers { get; set; }

    public virtual DbSet<TmpSerialNoreport> TmpSerialNoreports { get; set; }

    public virtual DbSet<TmpfeastPart> TmpfeastParts { get; set; }

    public virtual DbSet<TmpinvTransferNonserialized> TmpinvTransferNonserializeds { get; set; }

    public virtual DbSet<TmpinvTransferSerialized> TmpinvTransferSerializeds { get; set; }

    public virtual DbSet<UnKnownCustomerLog> UnKnownCustomerLogs { get; set; }

    public virtual DbSet<UserApplication> UserApplications { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserRefreshtoken> UserRefreshtokens { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<VAllPmandServiceEventsWithUniqueSerial> VAllPmandServiceEventsWithUniqueSerials { get; set; }

    public virtual DbSet<VContact> VContacts { get; set; }

    public virtual DbSet<VContactServiceHistory> VContactServiceHistories { get; set; }

    public virtual DbSet<VEquipmentCount> VEquipmentCounts { get; set; }

    public virtual DbSet<VLastOneyearDispatch> VLastOneyearDispatches { get; set; }

    public virtual DbSet<VLastOneyearInvoice> VLastOneyearInvoices { get; set; }

    public virtual DbSet<VOpenCallTime> VOpenCallTimes { get; set; }

    public virtual DbSet<VOriginalCallDetail> VOriginalCallDetails { get; set; }

    public virtual DbSet<VRepeatServiceRepair> VRepeatServiceRepairs { get; set; }

    public virtual DbSet<VRepeatServiceRepairDetail> VRepeatServiceRepairDetails { get; set; }

    public virtual DbSet<VRepeatedPmandServiceEvent> VRepeatedPmandServiceEvents { get; set; }

    public virtual DbSet<VRepeatedPmandServiceEventDetail> VRepeatedPmandServiceEventDetails { get; set; }

    public virtual DbSet<VTechDispatchCountToday> VTechDispatchCountTodays { get; set; }

    public virtual DbSet<VUniqueInvoiceTimingsByDealerId> VUniqueInvoiceTimingsByDealerIds { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    public virtual DbSet<VoltageList> VoltageLists { get; set; }

    public virtual DbSet<VwFbroleFunction> VwFbroleFunctions { get; set; }

    public virtual DbSet<VwTechHierarchy> VwTechHierarchies { get; set; }

    public virtual DbSet<WaterLineList> WaterLineLists { get; set; }

    public virtual DbSet<WorkOrder> WorkOrders { get; set; }

    public virtual DbSet<WorkOrderBrand> WorkOrderBrands { get; set; }

    public virtual DbSet<WorkorderBillingDetail> WorkorderBillingDetails { get; set; }

    public virtual DbSet<WorkorderDetail> WorkorderDetails { get; set; }

    public virtual DbSet<WorkorderEquipment> WorkorderEquipments { get; set; }

    public virtual DbSet<WorkorderEquipmentRequested> WorkorderEquipmentRequesteds { get; set; }

    public virtual DbSet<WorkorderImage> WorkorderImages { get; set; }

    public virtual DbSet<WorkorderInstallationSurvey> WorkorderInstallationSurveys { get; set; }

    public virtual DbSet<WorkorderNonAudit> WorkorderNonAudits { get; set; }

    public virtual DbSet<WorkorderPart> WorkorderParts { get; set; }

    public virtual DbSet<WorkorderReasonlog> WorkorderReasonlogs { get; set; }

    public virtual DbSet<WorkorderSavedSearch> WorkorderSavedSearches { get; set; }

    public virtual DbSet<WorkorderSchedule> WorkorderSchedules { get; set; }

    public virtual DbSet<WorkorderServiceClaim> WorkorderServiceClaims { get; set; }

    public virtual DbSet<WorkorderStatusLog> WorkorderStatusLogs { get; set; }

    public virtual DbSet<WorkorderType> WorkorderTypes { get; set; }

    public virtual DbSet<YabhiPosdetail> YabhiPosdetails { get; set; }

    public virtual DbSet<YhcompassSurvey> YhcompassSurveys { get; set; }

    public virtual DbSet<Yhcontact> Yhcontacts { get; set; }

    public virtual DbSet<YhcontactPmuploadsAll> YhcontactPmuploadsAlls { get; set; }

    public virtual DbSet<Yherfequipment> Yherfequipments { get; set; }

    public virtual DbSet<Yherfexpendable> Yherfexpendables { get; set; }

    public virtual DbSet<Yherfmaster> Yherfmasters { get; set; }

    public virtual DbSet<YheventBrand> YheventBrands { get; set; }

    public virtual DbSet<YheventClosurePart> YheventClosureParts { get; set; }

    public virtual DbSet<YheventNote> YheventNotes { get; set; }

    public virtual DbSet<YhfeastTechHierarchy> YhfeastTechHierarchies { get; set; }

    public virtual DbSet<Yhsmevent> Yhsmevents { get; set; }

    public virtual DbSet<YhsmeventInvoice> YhsmeventInvoices { get; set; }

    public virtual DbSet<YhsmeventSymcall> YhsmeventSymcalls { get; set; }

    public virtual DbSet<Yhsmschedule> Yhsmschedules { get; set; }

    public virtual DbSet<Zip> Zips { get; set; }

    public virtual DbSet<ZonePriority> ZonePriorities { get; set; }

    public virtual DbSet<ZoneZip> ZoneZips { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-Q171TQA\\SQLEXPRESS;Database=FB_Dev;Trusted_Connection=True;TrustServerCertificate=true;");
        //=> optionsBuilder.UseSqlServer("Server=863781-APP1\\SQLEXPRESS;User Id=sa;Password=M@ir0cks;Database=FB_TEST;Trusted_Connection=True;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AbhiFetcoDatum>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Abhi_Fetco_Data");

            entity.Property(e => e.CustomerName)
                .HasMaxLength(255)
                .HasColumnName("Customer Name");
            entity.Property(e => e.CustomerNumber)
                .HasMaxLength(255)
                .HasColumnName("Customer Number");
            entity.Property(e => e.PaymentTermsId)
                .HasMaxLength(255)
                .HasColumnName("Payment Terms ID");
        });

        modelBuilder.Entity<AgentDispatchLog>(entity =>
        {
            entity.ToTable("AgentDispatchLog");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Tdate)
                .HasColumnType("datetime")
                .HasColumnName("TDate");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<AllFbstatus>(entity =>
        {
            entity.HasKey(e => e.FbstatusId);

            entity.ToTable("AllFBStatus");

            entity.Property(e => e.FbstatusId)
                .ValueGeneratedNever()
                .HasColumnName("FBStatusID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Fbstatus)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("FBStatus");
            entity.Property(e => e.SolutionId).HasColumnName("SolutionID");
            entity.Property(e => e.StatusFor)
                .HasMaxLength(45)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ampslist>(entity =>
        {
            entity.HasKey(e => e.Ampsid);

            entity.ToTable("AMPSList");

            entity.Property(e => e.Ampsid)
                .ValueGeneratedNever()
                .HasColumnName("AMPSID");
            entity.Property(e => e.Ampsdescription)
                .HasMaxLength(255)
                .HasColumnName("AMPSDescription");
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("Application");

            entity.Property(e => e.ApplicationId).ValueGeneratedNever();
            entity.Property(e => e.ApplicationName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BillingItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BillingI__3214EC07ABD7BFB9");

            entity.Property(e => e.BillingCode)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.BillingName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("smalldatetime");
            entity.Property(e => e.UnitPrice).HasColumnType("money");
        });

        modelBuilder.Entity<BranchEsm>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("PK__BranchES__A2A2A54A2C56285B");

            entity.ToTable("BranchESM");

            entity.Property(e => e.BranchName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.BranchNo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Esmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ESMName");
        });

        modelBuilder.Entity<BrandName>(entity =>
        {
            entity.HasKey(e => e.BrandId);

            entity.Property(e => e.BrandId)
                .ValueGeneratedNever()
                .HasColumnName("BrandID");
            entity.Property(e => e.BrandName1)
                .HasMaxLength(255)
                .HasColumnName("BrandName");
        });

        modelBuilder.Entity<CallTypeSymptomSolutionMaster>(entity =>
        {
            entity.ToTable("CallTypeSymptomSolutionMaster");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CallTypeId).HasColumnName("CallTypeID");
            entity.Property(e => e.ColUpdated).HasDefaultValue(0);
            entity.Property(e => e.SolutionId).HasColumnName("SolutionID");
            entity.Property(e => e.SymptomId).HasColumnName("SymptomID");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).ValueGeneratedNever();
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CategoryDesc).HasMaxLength(255);
        });

        modelBuilder.Entity<CcinvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CCInvoic__3214EC07076C1F53");

            entity.ToTable("CCInvoiceDetails");

            entity.Property(e => e.LaborCost).HasColumnType("money");
            entity.Property(e => e.PartsCost).HasColumnType("money");
            entity.Property(e => e.PartsDiscount).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TravelCost).HasColumnType("money");

            entity.HasOne(d => d.Workorder).WithMany(p => p.CcinvoiceDetails)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__CCInvoice__Worko__52793849");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__Contact__5C6625BBEFC4C174");

            entity.ToTable("Contact");

            entity.Property(e => e.ContactId)
                .ValueGeneratedNever()
                .HasColumnName("ContactID");
            entity.Property(e => e.AcctRep).HasMaxLength(3);
            entity.Property(e => e.AcctRepDesc).HasMaxLength(30);
            entity.Property(e => e.Address1).HasMaxLength(150);
            entity.Property(e => e.Address2).HasMaxLength(100);
            entity.Property(e => e.Address3).HasMaxLength(40);
            entity.Property(e => e.AnniversaryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.AreaCode).HasMaxLength(6);
            entity.Property(e => e.BillingCode)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Branch).HasMaxLength(3);
            entity.Property(e => e.BrewmaticAgentCode).HasMaxLength(3);
            entity.Property(e => e.BusinessUnit).HasMaxLength(12);
            entity.Property(e => e.CategoryCode).HasMaxLength(3);
            entity.Property(e => e.Ccmemail)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("CCMEmail");
            entity.Property(e => e.Ccmjde).HasColumnName("CCMJDE");
            entity.Property(e => e.Ccmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CCMName");
            entity.Property(e => e.Ccmphone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CCMPhone");
            entity.Property(e => e.Chain).HasMaxLength(3);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.ContributionMargin).HasMaxLength(50);
            entity.Property(e => e.CustomerBranch).HasMaxLength(50);
            entity.Property(e => e.CustomerRegion).HasMaxLength(50);
            entity.Property(e => e.CustomerSpecialInstructions).IsUnicode(false);
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.DaysSinceLastSale)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DelDayOrFob)
                .HasMaxLength(3)
                .HasColumnName("DelDayOrFOB");
            entity.Property(e => e.DeliveryDesc).HasMaxLength(30);
            entity.Property(e => e.DeliveryMethod).HasMaxLength(3);
            entity.Property(e => e.DistributorName).HasMaxLength(40);
            entity.Property(e => e.District).HasMaxLength(3);
            entity.Property(e => e.DistrictDesc).HasMaxLength(30);
            entity.Property(e => e.Division).HasMaxLength(3);
            entity.Property(e => e.DivisionDesc).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Esmemail)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ESMEmail");
            entity.Property(e => e.Esmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ESMName");
            entity.Property(e => e.Esmphone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ESMPhone");
            entity.Property(e => e.FamilyAff).HasMaxLength(3);
            entity.Property(e => e.FamilyAffDesc).HasMaxLength(30);
            entity.Property(e => e.FbproviderId).HasColumnName("FBProviderID");
            entity.Property(e => e.FieldServiceManager).HasMaxLength(50);
            entity.Property(e => e.FilterReplacedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Fsmjde).HasColumnName("FSMJDE");
            entity.Property(e => e.LastAutoEmail).HasColumnType("smalldatetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedByFtp)
                .HasColumnType("datetime")
                .HasColumnName("LastModifiedByFTP");
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.LastSaleDate).HasMaxLength(15);
            entity.Property(e => e.LongAddressNumber).HasMaxLength(30);
            entity.Property(e => e.MailingName).HasMaxLength(40);
            entity.Property(e => e.MiddleName).HasMaxLength(25);
            entity.Property(e => e.NetSalesAmount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.NextFilterReplacementDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Nofbpspemails).HasColumnName("NOFBPSPEmails");
            entity.Property(e => e.Ntr)
                .HasMaxLength(3)
                .HasColumnName("NTR");
            entity.Property(e => e.OperatingUnit).HasMaxLength(3);
            entity.Property(e => e.PaymentTerm).HasMaxLength(30);
            entity.Property(e => e.Phone).HasMaxLength(25);
            entity.Property(e => e.PhoneWithAreaCode).HasMaxLength(30);
            entity.Property(e => e.PostalCode).HasMaxLength(12);
            entity.Property(e => e.PricingParent).HasMaxLength(8);
            entity.Property(e => e.PricingParentDesc).HasMaxLength(255);
            entity.Property(e => e.PricingParentId)
                .HasMaxLength(20)
                .HasColumnName("PricingParentID");
            entity.Property(e => e.PricingParentName).HasMaxLength(40);
            entity.Property(e => e.ProfitabilityTier).HasMaxLength(30);
            entity.Property(e => e.Rccmjde).HasColumnName("RCCMJDE");
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.Route).HasMaxLength(10);
            entity.Property(e => e.RouteCode).HasMaxLength(10);
            entity.Property(e => e.Rsmemail)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("RSMEmail");
            entity.Property(e => e.Rsmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("RSMName");
            entity.Property(e => e.Rsmphone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RSMPhone");
            entity.Property(e => e.SalesEmail).HasMaxLength(150);
            entity.Property(e => e.SearchDesc).HasMaxLength(30);
            entity.Property(e => e.SearchType).HasMaxLength(3);
            entity.Property(e => e.ServiceLevelCode).HasMaxLength(5);
            entity.Property(e => e.State).HasMaxLength(3);
            entity.Property(e => e.TierDesc).HasMaxLength(30);
            entity.Property(e => e.ZoneNumber).HasMaxLength(3);
            entity.Property(e => e._1099reporting)
                .HasMaxLength(3)
                .HasColumnName("1099Reporting");
        });

        modelBuilder.Entity<ContactPmuploadsAll>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("PK__Contact___A2A2BAAA34447C5A");

            entity.ToTable("Contact_PMUploadsALL");

            entity.Property(e => e.UniqueId).HasColumnName("UniqueID");
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.ContactName).HasMaxLength(70);
            entity.Property(e => e.CustomerName).HasMaxLength(255);
            entity.Property(e => e.DayLightSaving)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EquipmentLocation).HasMaxLength(255);
            entity.Property(e => e.EquipmentModel).HasMaxLength(255);
            entity.Property(e => e.IntervalType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NextRunDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Notes).HasColumnType("ntext");
            entity.Property(e => e.Phone).HasMaxLength(80);
            entity.Property(e => e.StartDate).HasColumnType("smalldatetime");
            entity.Property(e => e.TechId).HasColumnName("TechID");
            entity.Property(e => e.TechName)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.Tpsp)
                .HasMaxLength(255)
                .HasColumnName("TPSP");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Contingent>(entity =>
        {
            entity.HasKey(e => e.ContingentId).HasName("PK__Continge__59111B3D5AE8A468");

            entity.ToTable("Contingent");

            entity.Property(e => e.ContingentId).HasColumnName("ContingentID");
            entity.Property(e => e.ContingentName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ContingentType)
                .HasMaxLength(5)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ContingentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Continge__3214EC277FF289F5");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CashSale).HasColumnType("money");
            entity.Property(e => e.ContingentId).HasColumnName("ContingentID");
            entity.Property(e => e.LaidInCost).HasColumnType("money");
            entity.Property(e => e.Rental).HasColumnType("money");
        });

        modelBuilder.Entity<CustomCriterion>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("PK__CustomCr__A2A2A54AA7B5986C");

            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.CategoryType).HasMaxLength(25);
            entity.Property(e => e.CategoryValue).HasMaxLength(255);
        });

        modelBuilder.Entity<ElectricalPhaseList>(entity =>
        {
            entity.HasKey(e => e.ElectricalPhaseId);

            entity.ToTable("ElectricalPhaseList");

            entity.Property(e => e.ElectricalPhaseId)
                .ValueGeneratedNever()
                .HasColumnName("ElectricalPhaseID");
            entity.Property(e => e.ElectricalPhase).HasMaxLength(255);
        });

        modelBuilder.Entity<EquipType>(entity =>
        {
            entity.ToTable("EquipType");

            entity.Property(e => e.EquipTypeId).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EquipTypeCode).HasMaxLength(255);
        });

        modelBuilder.Entity<Erf>(entity =>
        {
            entity.HasKey(e => e.ErfId).HasName("PK_erf");

            entity.ToTable("Erf");

            entity.Property(e => e.ErfId)
                .HasMaxLength(50)
                .HasColumnName("ErfID");
            entity.Property(e => e.AdditionalEqp).HasColumnType("money");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.BulkUploadResult)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CashSaleStatus)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ChannelId).HasColumnName("ChannelID");
            entity.Property(e => e.CmReceviedDate).HasColumnType("datetime");
            entity.Property(e => e.CmUser)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.ContributionMargin)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CurrentEqp).HasColumnType("money");
            entity.Property(e => e.CurrentNsv)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("CurrentNSV");
            entity.Property(e => e.CustomerAddress)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CustomerCity)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CustomerMainContactName)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.CustomerMainEmail)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.CustomerName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(35)
                .IsUnicode(false);
            entity.Property(e => e.CustomerPhoneExtn)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.CustomerPo)
                .HasMaxLength(90)
                .HasColumnName("CustomerPO");
            entity.Property(e => e.CustomerState)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.CustomerZipCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DateErfprocessed)
                .HasColumnType("datetime")
                .HasColumnName("DateERFProcessed");
            entity.Property(e => e.DateErfreceived)
                .HasColumnType("datetime")
                .HasColumnName("DateERFReceived");
            entity.Property(e => e.DateOnErf)
                .HasColumnType("datetime")
                .HasColumnName("DateOnERF");
            entity.Property(e => e.DateReceived).HasColumnType("datetime");
            entity.Property(e => e.DayLightSaving)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.EntryUserId).HasColumnName("EntryUserID");
            entity.Property(e => e.EquipEtadate)
                .HasColumnType("datetime")
                .HasColumnName("EquipETADate");
            entity.Property(e => e.Erfstatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ERFStatus");
            entity.Property(e => e.FeastMovementId).HasColumnName("FeastMovementID");
            entity.Property(e => e.HoursofOperation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InstallLocation)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.JdeProcessDate).HasColumnType("datetime");
            entity.Property(e => e.LoanSale).HasMaxLength(50);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedUserId).HasColumnName("ModifiedUserID");
            entity.Property(e => e.OrderType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.OriginalRequestedDate).HasColumnType("datetime");
            entity.Property(e => e.Phone)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.ReasonId).HasColumnName("ReasonID");
            entity.Property(e => e.SalesPerson)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.ShipToBranch)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ShipToJde).HasColumnName("ShipToJDE");
            entity.Property(e => e.ShipfromJde).HasColumnName("ShipfromJDE");
            entity.Property(e => e.SiteReady)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.TimeErfprocessed)
                .HasColumnType("datetime")
                .HasColumnName("TimeERFProcessed");
            entity.Property(e => e.TotalNsv)
                .HasColumnType("money")
                .HasColumnName("TotalNSV");
            entity.Property(e => e.Tracking)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TransType)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.UploadError).HasColumnType("ntext");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<ErfWorkorderLog>(entity =>
        {
            entity.HasKey(e => new { e.ErfId, e.WorkorderId }).HasName("pk_erfworkorder_erfworkorderlog");

            entity.ToTable("ErfWorkorderLog");

            entity.Property(e => e.ErfId)
                .HasMaxLength(50)
                .HasColumnName("ErfID");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<ErfbranchDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ERFBranc__3214EC0773E24196");

            entity.ToTable("ERFBranchDetails");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Branch)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BranchName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.District)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.Region)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(3)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ErforderType>(entity =>
        {
            entity.HasKey(e => e.OrderTypeId).HasName("PK__ERFOrder__23AC266CEE5D8CF2");

            entity.ToTable("ERFOrderType");

            entity.Property(e => e.OrderTypeId).ValueGeneratedNever();
            entity.Property(e => e.OrderType).HasMaxLength(100);
        });

        modelBuilder.Entity<Esmccmrsmescalation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ESMCCMRS__3214EC27EF905C81");

            entity.ToTable("ESMCCMRSMEscalation");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Ccmemail)
                .HasMaxLength(255)
                .HasColumnName("CCMEmail");
            entity.Property(e => e.Ccmid).HasColumnName("CCMID");
            entity.Property(e => e.Ccmname)
                .HasMaxLength(255)
                .HasColumnName("CCMName");
            entity.Property(e => e.Ccmphone)
                .HasMaxLength(255)
                .HasColumnName("CCMPhone");
            entity.Property(e => e.Country).HasMaxLength(255);
            entity.Property(e => e.Edsmid).HasColumnName("EDSMID");
            entity.Property(e => e.Esmemail)
                .HasMaxLength(255)
                .HasColumnName("ESMEmail");
            entity.Property(e => e.Esmname)
                .HasMaxLength(255)
                .HasColumnName("ESMName");
            entity.Property(e => e.Esmphone)
                .HasMaxLength(255)
                .HasColumnName("ESMPhone");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Rsm)
                .HasMaxLength(255)
                .HasColumnName("RSM");
            entity.Property(e => e.Rsmemail)
                .HasMaxLength(255)
                .HasColumnName("RSMEmail");
            entity.Property(e => e.Rsmid).HasColumnName("RSMID");
            entity.Property(e => e.Rsmphone)
                .HasMaxLength(255)
                .HasColumnName("RSMPhone");
            entity.Property(e => e.Zipcode)
                .HasMaxLength(255)
                .HasColumnName("ZIPCode");
        });

        modelBuilder.Entity<Esmdsmrsm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ESMDSMRS__3214EC270E564089");

            entity.ToTable("ESMDSMRSM");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Branch)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.BranchNo)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("BranchNO");
            entity.Property(e => e.Ccmemail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CCMEmail");
            entity.Property(e => e.Ccmid).HasColumnName("CCMID");
            entity.Property(e => e.Ccmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CCMName");
            entity.Property(e => e.Ccmphone)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("CCMPhone");
            entity.Property(e => e.Edsmid).HasColumnName("EDSMID");
            entity.Property(e => e.Esmemail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ESMEmail");
            entity.Property(e => e.Esmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ESMName");
            entity.Property(e => e.Esmphone)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ESMPhone");
            entity.Property(e => e.Region)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RegionName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Rsm)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("RSM");
            entity.Property(e => e.Rsmemail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("RSMEmail");
            entity.Property(e => e.Rsmid).HasColumnName("RSMID");
            entity.Property(e => e.Rsmphone)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("RSMPhone");
        });

        modelBuilder.Entity<EstimateEscalation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Estimate__3214EC27EA1B8219");

            entity.ToTable("EstimateEscalation");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FbBillableSku>(entity =>
        {
            entity.HasKey(e => e.Sku).HasName("PK__FbBillab__CA1ECF0C3C5C3833");

            entity.ToTable("FbBillableSKU");

            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.Skudescription)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("SKUDescription");
            entity.Property(e => e.UnitPrice).HasColumnType("money");
        });

        modelBuilder.Entity<FbPrimaryTechnician>(entity =>
        {
            entity.HasKey(e => e.PrimaryTechId).HasName("PK__FbPrimar__6A0C996FD09AD969");

            entity.Property(e => e.PrimaryTechId).ValueGeneratedNever();
            entity.Property(e => e.PrimaryTechName).HasMaxLength(50);
        });

        modelBuilder.Entity<FbPrimaryTechnicians1>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("FbPrimaryTechnicians1");

            entity.Property(e => e.PrimaryTechId).ValueGeneratedOnAdd();
            entity.Property(e => e.PrimaryTechName).HasMaxLength(50);
        });

        modelBuilder.Entity<FbRole>(entity =>
        {
            entity.HasKey(e => e.RoleId);

            entity.Property(e => e.RoleId).ValueGeneratedNever();
            entity.Property(e => e.RoleName)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FbUserMaster>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__FbUserMa__CAF055CA47BFB940");

            entity.ToTable("FbUserMaster");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Address)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Apt)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Company)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ConfirmPassword)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CustomerParent).HasMaxLength(150);
            entity.Property(e => e.Division)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EmailId).IsUnicode(false);
            entity.Property(e => e.Fax)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.IsErfuser).HasColumnName("IsERFUser");
            entity.Property(e => e.Jde)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("JDE");
            entity.Property(e => e.LastName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Mi)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MI");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordUpdatedDate)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RefreshToken).HasColumnType("ntext");
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.Region)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Zip)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.FbUserMasters)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FbUserMas__RoleI__4589517F");

            entity.HasOne(d => d.UserType).WithMany(p => p.FbUserMasters)
                .HasForeignKey(d => d.UserTypeId)
                .HasConstraintName("FK_FbUserMaster_UserType");
        });

        modelBuilder.Entity<FbWorkOrderSku>(entity =>
        {
            entity.HasKey(e => e.WorkOrderSkuid).HasName("PK__FbWorkOr__A777EE1D05AAF0CB");

            entity.ToTable("FbWorkOrderSKU");

            entity.Property(e => e.WorkOrderSkuid).HasColumnName("WorkOrderSKUId");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<FbactivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK_FBActivityLog_1");

            entity.ToTable("FBActivityLog");

            entity.Property(e => e.ErrorDetails).IsUnicode(false);
            entity.Property(e => e.LogDate).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.FbactivityLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_FBActivityLog_FbUserMaster");
        });

        modelBuilder.Entity<FbbillableFeed>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("FBBillableFeeds");

            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(100);
        });

        modelBuilder.Entity<FbcallReason>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PK__FBCallRe__16E019F96296DCD5");

            entity.ToTable("FBCallReason");

            entity.Property(e => e.SourceId)
                .ValueGeneratedNever()
                .HasColumnName("SourceID");
            entity.Property(e => e.AdditionalEmail).IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.EmailCcm).HasColumnName("EmailCCM");
            entity.Property(e => e.EmailFsm).HasColumnName("EmailFSM");
            entity.Property(e => e.EmailRsr).HasColumnName("EmailRSR");
            entity.Property(e => e.SourceCode)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Fbcbe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FBCBE__3214EC07163505F2");

            entity.ToTable("FBCBE");

            entity.Property(e => e.AssetStatus).HasMaxLength(1);
            entity.Property(e => e.CurrentCustomerName).HasMaxLength(50);
            entity.Property(e => e.CurrentGlcode)
                .HasMaxLength(4)
                .HasColumnName("CurrentGLCode");
            entity.Property(e => e.CurrentGlobject)
                .HasMaxLength(4)
                .HasColumnName("CurrentGLObject");
            entity.Property(e => e.CurrentLocation).HasMaxLength(12);
            entity.Property(e => e.InitialDate).HasColumnType("smalldatetime");
            entity.Property(e => e.InitialValue).HasColumnType("money");
            entity.Property(e => e.ItemDescription).HasMaxLength(30);
            entity.Property(e => e.ItemNumber).HasMaxLength(25);
            entity.Property(e => e.SerialNumber).HasMaxLength(30);
            entity.Property(e => e.TransDate).HasColumnType("smalldatetime");
        });

        modelBuilder.Entity<FbclosurePart>(entity =>
        {
            entity.HasKey(e => e.SkuId);

            entity.ToTable("FBClosureParts");

            entity.Property(e => e.SkuId).ValueGeneratedNever();
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.EntryNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ItemNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrderSource)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VendorNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Version)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FbcustomerNote>(entity =>
        {
            entity.HasKey(e => e.CustomerNotesId);

            entity.ToTable("FBCustomerNotes");

            entity.Property(e => e.EntryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Notes).HasColumnType("ntext");
            entity.Property(e => e.UserName)
                .HasMaxLength(75)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.FbcustomerNotes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_FBCustomerNotes_FbUserMaster");
        });

        modelBuilder.Entity<FbcustomerServiceDistribution>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__FBCustom__C51BB00A5017D827");

            entity.ToTable("FBCustomerServiceDistribution");

            entity.Property(e => e.ServiceId).ValueGeneratedNever();
            entity.Property(e => e.Branch)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.RegionalsEmail).HasMaxLength(255);
            entity.Property(e => e.RegionalsName).HasMaxLength(255);
            entity.Property(e => e.RegonalsPhone).HasMaxLength(255);
            entity.Property(e => e.Route)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Rsremail)
                .HasMaxLength(255)
                .HasColumnName("RSREmail");
            entity.Property(e => e.Rsrname)
                .HasMaxLength(255)
                .HasColumnName("RSRName");
            entity.Property(e => e.Rsrphone)
                .HasMaxLength(255)
                .HasColumnName("RSRPhone");
            entity.Property(e => e.SalesManagerName).HasMaxLength(255);
            entity.Property(e => e.SalesManagerPhone).HasMaxLength(255);
            entity.Property(e => e.SalesMmanagerEmail).HasMaxLength(255);
        });

        modelBuilder.Entity<FbdailyContact>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("FBDailyContacts");

            entity.Property(e => e.AddressLine1)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("Address Line 1");
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Address Line 2");
            entity.Property(e => e.AddressLine3)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Address Line 3");
            entity.Property(e => e.AddressNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Address Number");
            entity.Property(e => e.AlphaName)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("Alpha Name");
            entity.Property(e => e.BillingCode)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Branch)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.BrewmaticAgentCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Brewmatic Agent Code");
            entity.Property(e => e.BusinessUnit)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Business Unit");
            entity.Property(e => e.CategoryCode01)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Category Code 01");
            entity.Property(e => e.Ccmjde)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCMJDE");
            entity.Property(e => e.Ccmjde1)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCMJDE1");
            entity.Property(e => e.Chain)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.CustomerBranch)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CustomerRegion)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.DelDayFob)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Del Day / FOB");
            entity.Property(e => e.District)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Division)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ErrorCode).HasDefaultValue((short)0);
            entity.Property(e => e.ErrorDescription)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.FieldServiceManager)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Fsmjde)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FSMJDE");
            entity.Property(e => e.Fsmjde1)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FSMJDE1");
            entity.Property(e => e.GivenName)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("Given Name");
            entity.Property(e => e.LastSaleDate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LongAddressNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Long Address Number");
            entity.Property(e => e.MailingName)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("Mailing Name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("Middle Name");
            entity.Property(e => e.NewSaleDate).HasColumnType("smalldatetime");
            entity.Property(e => e.NewSearchType)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Ntr)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("NTR");
            entity.Property(e => e.OldSearchType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.OperatingUnit)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Operating Unit");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Phone Number");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("Postal Code");
            entity.Property(e => e.Prefix)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PricingParentDesc)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.PricingParentId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PricingParentID");
            entity.Property(e => e.Rccmjde)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RCCMJDE");
            entity.Property(e => e.Rccmjde1)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RCCMJDE1");
            entity.Property(e => e.RecExists).HasDefaultValue((short)0);
            entity.Property(e => e.RegionNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Route)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RouteCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Route Code");
            entity.Property(e => e.SaleDate)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.SearchType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Search Type");
            entity.Property(e => e.State)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Surname)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.ZoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Zone Number");
            entity.Property(e => e._1099Reporting)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("1099 Reporting");
        });

        modelBuilder.Entity<Fbequipment>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__FBEquipm__B40CC6CDFDC7DD7F");

            entity.ToTable("FBEquipment");

            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ModelNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ModelNO");
            entity.Property(e => e.ProdNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Fberfequipment>(entity =>
        {
            entity.HasKey(e => e.ErfequipmentId);

            entity.ToTable("FBERFEquipment");

            entity.Property(e => e.ErfequipmentId).HasColumnName("ERFEquipmentId");
            entity.Property(e => e.DepositAmount).HasColumnType("money");
            entity.Property(e => e.DepositInvoiceNumber)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.EquipmentType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Erfid)
                .HasMaxLength(50)
                .HasColumnName("ERFId");
            entity.Property(e => e.Extra)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FinalInvoiceNumber)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.InternalOrderType)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceTotal).HasColumnType("money");
            entity.Property(e => e.LaidInCost).HasColumnType("money");
            entity.Property(e => e.ModelNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrderType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ProdNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RentalCost).HasColumnType("money");
            entity.Property(e => e.SerialNumber).HasMaxLength(255);
            entity.Property(e => e.Substitution)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TotalCost).HasColumnType("money");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice).HasColumnType("money");
            entity.Property(e => e.UsingBranch)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.VendorOrderType)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.Erf).WithMany(p => p.Fberfequipments)
                .HasForeignKey(d => d.Erfid)
                .HasConstraintName("FK_FBERFEquipment_Erf");
        });

        modelBuilder.Entity<Fberfexpendable>(entity =>
        {
            entity.HasKey(e => e.ErfexpendableId);

            entity.ToTable("FBERFExpendable");

            entity.Property(e => e.ErfexpendableId).HasColumnName("ERFExpendableId");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.EquipmentType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Erfid)
                .HasMaxLength(50)
                .HasColumnName("ERFId");
            entity.Property(e => e.Extra)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InternalOrderType)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.LaidInCost).HasColumnType("money");
            entity.Property(e => e.ModelNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProdNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RentalCost).HasColumnType("money");
            entity.Property(e => e.Substitution)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TotalCost).HasColumnType("money");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice).HasColumnType("money");
            entity.Property(e => e.UsingBranch)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.VendorOrderType)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.Erf).WithMany(p => p.Fberfexpendables)
                .HasForeignKey(d => d.Erfid)
                .HasConstraintName("FK_FBERFExpendable_FBERFExpendable");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.Fberfexpendables)
                .HasForeignKey(d => d.WorkOrderId)
                .HasConstraintName("FK_FBERFExpendable_WorkOrder");
        });

        modelBuilder.Entity<Fberfpo>(entity =>
        {
            entity.HasKey(e => e.ErfposId).HasName("PK__FBERFPos__BC8DAC6CDAEA4F5A");

            entity.ToTable("FBERFPos");

            entity.Property(e => e.ErfposId).HasColumnName("ERFPosId");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.EquipmentType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Erfid)
                .HasMaxLength(50)
                .HasColumnName("ERFId");
            entity.Property(e => e.Extra)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InternalOrderType)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.LaidInCost).HasColumnType("money");
            entity.Property(e => e.ModelNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProdNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RentalCost).HasColumnType("money");
            entity.Property(e => e.Substitution)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TotalCost).HasColumnType("money");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice).HasColumnType("money");
            entity.Property(e => e.UsingBranch)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.VendorOrderType)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FberftransactionType>(entity =>
        {
            entity.ToTable("FBERFTransactionType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Fbexpendable>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__FBExpend__B40CC6CDC53BE3FC");

            entity.ToTable("FBExpendable");

            entity.Property(e => e.ProductId).ValueGeneratedNever();
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ModelNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ModelNO");
            entity.Property(e => e.ProdNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Fbfunctionality>(entity =>
        {
            entity.HasKey(e => e.FunctionId);

            entity.ToTable("FBFunctionality");

            entity.Property(e => e.FunctionId)
                .ValueGeneratedNever()
                .HasColumnName("FunctionID");
            entity.Property(e => e.FunctionName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ParentFunctionId).HasColumnName("ParentFunctionID");
        });

        modelBuilder.Entity<Fbreport>(entity =>
        {
            entity.HasKey(e => e.ReportId);

            entity.ToTable("FBReports");

            entity.Property(e => e.ReportId)
                .ValueGeneratedNever()
                .HasColumnName("report_id");
            entity.Property(e => e.DescriptionFile).HasMaxLength(255);
            entity.Property(e => e.DisplayFile).HasMaxLength(255);
            entity.Property(e => e.DownloadFile).HasMaxLength(255);
            entity.Property(e => e.DownloadName).HasMaxLength(255);
            entity.Property(e => e.ReportCategory)
                .HasMaxLength(255)
                .HasColumnName("report_category");
            entity.Property(e => e.ReportFilename)
                .HasMaxLength(255)
                .HasColumnName("report_filename");
            entity.Property(e => e.ReportName)
                .HasMaxLength(255)
                .HasColumnName("report_name");
            entity.Property(e => e.ReportType)
                .HasMaxLength(25)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FbroleFunction>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.FunctionId });

            entity.ToTable("FBRoleFunction");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.FunctionId).HasColumnName("FunctionID");
            entity.Property(e => e.RoleFunctionId).HasColumnName("RoleFunctionID");
        });

        modelBuilder.Entity<Fbsku>(entity =>
        {
            entity.HasKey(e => e.Skuid);

            entity.ToTable("FBSKU");

            entity.Property(e => e.Skuid)
                .ValueGeneratedNever()
                .HasColumnName("SKUId");
            entity.Property(e => e.Ben02ItemNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.E1number)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("E1Number");
            entity.Property(e => e.ShortProductNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.Skuactive).HasColumnName("SKUActive");
            entity.Property(e => e.Skucost)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SKUCost");
            entity.Property(e => e.SystemSource)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VendorCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VendorNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Weight)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FbuserReport>(entity =>
        {
            entity.HasKey(e => e.UserReportId).HasName("PK_marketing_shopper_report");

            entity.ToTable("FBUserReports");

            entity.Property(e => e.ReportId).HasColumnName("report_id");

            entity.HasOne(d => d.Report).WithMany(p => p.FbuserReports)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FBUserReports_FBReports");

            entity.HasOne(d => d.User).WithMany(p => p.FbuserReports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FBReports_FbUserMaster");
        });

        modelBuilder.Entity<FeastMovement>(entity =>
        {
            entity.HasKey(e => e.UniqueId);

            entity.ToTable("FeastMovement");

            entity.Property(e => e.UniqueId).HasColumnName("UniqueID");
            entity.Property(e => e.Comments).HasColumnType("ntext");
            entity.Property(e => e.Erfid).HasMaxLength(50);
            entity.Property(e => e.FeastmovementState)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ShipCarrier).HasMaxLength(50);
            entity.Property(e => e.ShipMethod).HasMaxLength(50);
            entity.Property(e => e.TrackingNumber).HasMaxLength(50);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<FetcoCustomer>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("PK__FetcoCus__A2A2A54A7A2E4D44");

            entity.Property(e => e.CustomerName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CustomerNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PaymentType)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HolidayList>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("PK__HolidayL__A2A2BAAA738813DA");

            entity.ToTable("HolidayList");

            entity.Property(e => e.UniqueId).HasColumnName("UniqueID");
            entity.Property(e => e.HolidayDate).HasColumnType("smalldatetime");
            entity.Property(e => e.HolidayName).HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<IndexCounter>(entity =>
        {
            entity.HasKey(e => e.Indexid);

            entity.ToTable("IndexCounter");

            entity.Property(e => e.Indexid).ValueGeneratedNever();
            entity.Property(e => e.IndexName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceUniqueid);

            entity.HasIndex(e => e.WorkorderId, "indx_Invoices_workorderID");

            entity.Property(e => e.AdditionalCharge).HasColumnType("money");
            entity.Property(e => e.AdjustmentAmount).HasColumnType("money");
            entity.Property(e => e.AmountPaid).HasColumnType("money");
            entity.Property(e => e.ApprovedBy)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.CheckNumber).HasMaxLength(25);
            entity.Property(e => e.CustomerName).HasMaxLength(150);
            entity.Property(e => e.DateCheckCreated).HasColumnType("datetime");
            entity.Property(e => e.InvoiceStatus).HasMaxLength(50);
            entity.Property(e => e.InvoiceSubmitDate).HasColumnType("smalldatetime");
            entity.Property(e => e.InvoiceTotal).HasColumnType("money");
            entity.Property(e => e.Invoiceid).HasMaxLength(50);
            entity.Property(e => e.LaborHours).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LaborTotal).HasColumnType("money");
            entity.Property(e => e.OverTimeRequestApproved)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.OvertimeLabor).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PartsTotal).HasColumnType("money");
            entity.Property(e => e.PaymentAmtApproved).HasColumnType("money");
            entity.Property(e => e.PaymentCreated).HasColumnType("smalldatetime");
            entity.Property(e => e.PhoneSolveTotal).HasColumnType("money");
            entity.Property(e => e.ProjectTotal).HasColumnType("money");
            entity.Property(e => e.ServiceLocation).HasMaxLength(90);
            entity.Property(e => e.StandardLabor).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.SubmitAmount).HasColumnType("money");
            entity.Property(e => e.TaxAmount).HasColumnType("money");
            entity.Property(e => e.TravelTotal).HasColumnType("money");
            entity.Property(e => e.WorkorderCompletionDate).HasColumnType("smalldatetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<JdepaymentTerm>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("PK__JDEPayme__A2A2A54A692030CE");

            entity.ToTable("JDEPaymentTerms");

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PaymentTerm)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NarfNewAccount>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__NARF_New__C93A4C99336B71F4");

            entity.ToTable("NARF_NewAccount");

            entity.Property(e => e.AccountPayableContact).HasMaxLength(50);
            entity.Property(e => e.AccountPayableEmail).HasMaxLength(150);
            entity.Property(e => e.AccountPayablePhone).HasMaxLength(25);
            entity.Property(e => e.AccountPayableTitle).HasMaxLength(100);
            entity.Property(e => e.AlreadyHasFbaccount).HasColumnName("AlreadyHasFBAccount");
            entity.Property(e => e.BilllingAddress).HasMaxLength(200);
            entity.Property(e => e.BilllingCity).HasMaxLength(50);
            entity.Property(e => e.BilllingState).HasMaxLength(10);
            entity.Property(e => e.BilllingZip).HasMaxLength(12);
            entity.Property(e => e.CompanyType).HasMaxLength(20);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUserId).HasColumnName("CreatedUserID");
            entity.Property(e => e.CreatedUserName).HasMaxLength(150);
            entity.Property(e => e.CreditRequested).HasColumnType("money");
            entity.Property(e => e.Dbaname)
                .HasMaxLength(200)
                .HasColumnName("DBAName");
            entity.Property(e => e.DeliveryAddress).HasMaxLength(200);
            entity.Property(e => e.DeliveryCity).HasMaxLength(50);
            entity.Property(e => e.DeliveryState).HasMaxLength(10);
            entity.Property(e => e.DeliveryZip).HasMaxLength(12);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EstablishedYear).HasMaxLength(5);
            entity.Property(e => e.Fbaccount)
                .HasMaxLength(150)
                .HasColumnName("FBAccount");
            entity.Property(e => e.FederalTaxId)
                .HasMaxLength(50)
                .HasColumnName("FederalTaxID");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedUserId).HasColumnName("ModifiedUserID");
            entity.Property(e => e.ModifiedUserName).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.NatureOfBusiness).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Porequired).HasColumnName("PORequired");
            entity.Property(e => e.PrincipalOfficer).HasMaxLength(50);
            entity.Property(e => e.PrincipalOfficerTitle).HasMaxLength(150);
            entity.Property(e => e.ResaleCertificateNumber).HasMaxLength(50);
            entity.Property(e => e.StateIncorporated).HasMaxLength(100);
        });

        modelBuilder.Entity<NarfTradeReference>(entity =>
        {
            entity.HasKey(e => e.TradeRefId).HasName("PK__NARF_Tra__AE15A8A809422F7F");

            entity.ToTable("NARF_TradeReference");

            entity.Property(e => e.TradeRefId).HasColumnName("TradeRefID");
            entity.Property(e => e.AccountId).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUserId).HasColumnName("CreatedUserID");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedUserId).HasColumnName("ModifiedUserID");
            entity.Property(e => e.Phone).HasMaxLength(25);

            entity.HasOne(d => d.Application).WithMany(p => p.NarfTradeReferences)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK__NARF_Trad__Appli__1F63A897");
        });

        modelBuilder.Entity<NemanumberList>(entity =>
        {
            entity.HasKey(e => e.NemaNumberId);

            entity.ToTable("NEMANumberList");

            entity.Property(e => e.NemaNumberId)
                .ValueGeneratedNever()
                .HasColumnName("NemaNumberID");
            entity.Property(e => e.NemaNumberDescription).HasMaxLength(255);
        });

        modelBuilder.Entity<NoAutoEmailZipCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NoAutoEm__3214EC27C225FB70");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PostalCode).HasMaxLength(10);
        });

        modelBuilder.Entity<NoAutomaticEmailContract>(entity =>
        {
            entity.HasKey(e => e.ContractId);

            entity.Property(e => e.ContractId)
                .ValueGeneratedNever()
                .HasColumnName("ContractID");
        });

        modelBuilder.Entity<NonFbcustomer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NonFBCus__3214EC07644936CB");

            entity.ToTable("NonFBCustomers");

            entity.Property(e => e.NonFbcustomerId)
                .HasMaxLength(20)
                .HasColumnName("NonFBCustomerId");
            entity.Property(e => e.NonFbcustomerName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("NonFBCustomerName");
        });

        modelBuilder.Entity<NonSerialized>(entity =>
        {
            entity.HasKey(e => e.Nserialid).HasName("PK_erf_nonserialized");

            entity.ToTable("NonSerialized");

            entity.Property(e => e.Nserialid).HasColumnName("NSerialid");
            entity.Property(e => e.Bin).HasMaxLength(10);
            entity.Property(e => e.Catalogid).HasMaxLength(25);
            entity.Property(e => e.Description).HasMaxLength(90);
            entity.Property(e => e.Location)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.ManufNumber).HasMaxLength(50);
            entity.Property(e => e.Model)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TagType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TotalLineAmount).HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Workorder).WithMany(p => p.NonSerializeds)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__NonSerial__Worko__2057CCD0");
        });

        modelBuilder.Entity<NonServiceworkorder>(entity =>
        {
            entity.HasKey(e => e.WorkOrderId);

            entity.ToTable("NonServiceworkorder");

            entity.Property(e => e.WorkOrderId)
                .ValueGeneratedNever()
                .HasColumnName("WorkOrderID");
            entity.Property(e => e.CallBack)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CallReason).IsUnicode(false);
            entity.Property(e => e.CallerName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CloseDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerCity)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CustomerState)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.CustomerZipCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.EmailSentTo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.MainContactName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NonServiceEventStatus)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.NonServiceId)
                .ValueGeneratedOnAdd()
                .HasColumnName("NonServiceID");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ResolutionCallerName)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NotesHistory>(entity =>
        {
            entity.HasKey(e => e.NotesId);

            entity.ToTable("NotesHistory");

            entity.Property(e => e.NotesId).HasColumnName("NotesID");
            entity.Property(e => e.AutomaticNotes).HasDefaultValue((short)0);
            entity.Property(e => e.EntryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ErfId)
                .HasMaxLength(50)
                .HasColumnName("ErfID");
            entity.Property(e => e.FeastMovementId).HasColumnName("FeastMovementID");
            entity.Property(e => e.IsDispatchNotes).HasColumnName("isDispatchNotes");
            entity.Property(e => e.NonServiceWorkorderId).HasColumnName("NonServiceWorkorderID");
            entity.Property(e => e.Notes).HasColumnType("ntext");
            entity.Property(e => e.UserName)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.NonServiceWorkorder).WithMany(p => p.NotesHistories)
                .HasForeignKey(d => d.NonServiceWorkorderId)
                .HasConstraintName("FK_NotesHistory_NonServiceworkorder");

            entity.HasOne(d => d.Workorder).WithMany(p => p.NotesHistories)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__NotesHist__Worko__3493CFA7");
        });

        modelBuilder.Entity<OnCallGroup>(entity =>
        {
            entity.HasKey(e => e.OnCallGroupId).HasName("PK__OnCallGr__A2598A25238063C7");

            entity.ToTable("OnCallGroup");

            entity.Property(e => e.OnCallGroupId).HasColumnName("OnCallGroupID");
            entity.Property(e => e.OnCallGroup1)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("OnCallGroup");
        });

        modelBuilder.Entity<PhoneSolveLog>(entity =>
        {
            entity.HasKey(e => e.PhoneSolveLogId).HasName("PK__PhoneSol__5ADA34141DD1FB25");

            entity.ToTable("PhoneSolveLog");

            entity.Property(e => e.AttemptedDate).HasColumnType("smalldatetime");

            entity.HasOne(d => d.Workorder).WithMany(p => p.PhoneSolveLogs)
                .HasForeignKey(d => d.WorkorderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhoneSolv__Worko__3587F3E0");
        });

        modelBuilder.Entity<PricingDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PricingD__3214EC0798092C04");

            entity.Property(e => e.AdditionalFee).HasColumnType("money");
            entity.Property(e => e.AfterHoursLaborRate).HasColumnType("money");
            entity.Property(e => e.AfterHoursTravelRate).HasColumnType("money");
            entity.Property(e => e.HourlyLablrRate).HasColumnType("money");
            entity.Property(e => e.HourlyTravlRate).HasColumnType("money");
            entity.Property(e => e.MilageRate).HasColumnType("money");
            entity.Property(e => e.PartsDiscount).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.PricingEntityId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PricingEntityName)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.PricingType).WithMany(p => p.PricingDetails)
                .HasForeignKey(d => d.PricingTypeId)
                .HasConstraintName("FK__PricingDe__Prici__5F141958");
        });

        modelBuilder.Entity<PricingType>(entity =>
        {
            entity.HasKey(e => e.PricingTypeId).HasName("PK__PricingT__E401A584A7D6883B");

            entity.ToTable("PricingType");

            entity.Property(e => e.PricingTypeName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Privilege>(entity =>
        {
            entity.ToTable("Privilege");

            entity.Property(e => e.PrivilegeId).ValueGeneratedNever();
            entity.Property(e => e.PrivilegeType)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RemovalSurvey>(entity =>
        {
            entity.HasKey(e => e.RemovalSurveyId).HasName("PK__RemovalS__B05F1F8BD0C33665");

            entity.ToTable("RemovalSurvey");

            entity.Property(e => e.RemovalSurveyId).HasColumnName("RemovalSurveyID");
            entity.Property(e => e.BeveragesSupplier).HasColumnType("ntext");
            entity.Property(e => e.JmsownedMachines).HasColumnName("JMSOwnedMachines");
            entity.Property(e => e.RemovalDate).HasColumnType("smalldatetime");
            entity.Property(e => e.RemovalReason).HasColumnType("ntext");
            entity.Property(e => e.RemoveAllMachines)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<Sku>(entity =>
        {
            entity.HasKey(e => e.Skuid).HasName("PK_sku");

            entity.ToTable("Sku");

            entity.Property(e => e.Skuid).ValueGeneratedNever();
            entity.Property(e => e.Category)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(120);
            entity.Property(e => e.EquipmentTag)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("EQUIPMENT_TAG");
            entity.Property(e => e.JmsItemNumber)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("JMS_ITEM_NUMBER");
            entity.Property(e => e.LastModified).HasColumnType("smalldatetime");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Sku1)
                .HasMaxLength(50)
                .HasColumnName("Sku");
            entity.Property(e => e.Skucost)
                .HasDefaultValue(0m)
                .HasColumnType("money")
                .HasColumnName("SKUCost");
            entity.Property(e => e.VendItemNumber)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("VEND_ITEM_NUMBER");
            entity.Property(e => e.VendorCode).HasMaxLength(15);
        });

        modelBuilder.Entity<Solution>(entity =>
        {
            entity.ToTable("Solution");

            entity.Property(e => e.SolutionId).ValueGeneratedNever();
            entity.Property(e => e.ColUpdated).HasDefaultValue(0);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.StateCode).HasName("PK_state");

            entity.ToTable("State");

            entity.Property(e => e.StateCode).HasMaxLength(255);
            entity.Property(e => e.StateName).HasMaxLength(255);
            entity.Property(e => e.TaxPercent).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<StateTax>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StateTax__3214EC07472E2E3A");

            entity.ToTable("StateTax");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.State).HasMaxLength(255);
            entity.Property(e => e.TaxRegionName).HasMaxLength(255);
            entity.Property(e => e.ZipCode).HasMaxLength(50);
        });

        modelBuilder.Entity<Symptom>(entity =>
        {
            entity.ToTable("Symptom");

            entity.Property(e => e.SymptomId)
                .ValueGeneratedNever()
                .HasColumnName("SymptomID");
            entity.Property(e => e.ColUpdated).HasDefaultValue(0);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<SystemInfo>(entity =>
        {
            entity.HasKey(e => e.SystemId);

            entity.ToTable("SystemInfo");

            entity.Property(e => e.SystemId).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(100);
        });

        modelBuilder.Entity<TechHierarchy>(entity =>
        {
            entity.HasKey(e => e.DealerId).HasName("PK__TECH_HIE__CA2F8EB2A1CE3263");

            entity.ToTable("TECH_HIERARCHY");

            entity.Property(e => e.DealerId).ValueGeneratedNever();
            entity.Property(e => e.Address1).HasMaxLength(40);
            entity.Property(e => e.Address2).HasMaxLength(40);
            entity.Property(e => e.Address3).HasMaxLength(40);
            entity.Property(e => e.Address4).HasMaxLength(40);
            entity.Property(e => e.AlternativePhone).HasMaxLength(15);
            entity.Property(e => e.AreaCode).HasMaxLength(6);
            entity.Property(e => e.BranchName).HasMaxLength(50);
            entity.Property(e => e.BranchNumber).HasMaxLength(10);
            entity.Property(e => e.BranchType).HasMaxLength(20);
            entity.Property(e => e.BusinessUnit).HasMaxLength(15);
            entity.Property(e => e.City).HasMaxLength(25);
            entity.Property(e => e.CompanyName).HasMaxLength(40);
            entity.Property(e => e.Contact).HasMaxLength(40);
            entity.Property(e => e.CustomerOwnNumber).HasMaxLength(10);
            entity.Property(e => e.DealerType).HasMaxLength(2);
            entity.Property(e => e.EmailCc)
                .HasMaxLength(150)
                .HasColumnName("EmailCC");
            entity.Property(e => e.FamilyAff).HasMaxLength(10);
            entity.Property(e => e.FamilyAffDesc).HasMaxLength(30);
            entity.Property(e => e.Fax).HasMaxLength(50);
            entity.Property(e => e.FieldServiceManager)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedByFtp)
                .HasColumnType("datetime")
                .HasColumnName("LastModifiedByFTP");
            entity.Property(e => e.LongAddressNumber).HasMaxLength(25);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedUserId).HasColumnName("ModifiedUserID");
            entity.Property(e => e.NextelPhone).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(25);
            entity.Property(e => e.PostalCode).HasMaxLength(12);
            entity.Property(e => e.PricingParentName).HasMaxLength(50);
            entity.Property(e => e.RegionName)
                .HasMaxLength(85)
                .IsUnicode(false);
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.RimEmail).HasMaxLength(150);
            entity.Property(e => e.SearchDesc).HasMaxLength(30);
            entity.Property(e => e.SearchType).HasMaxLength(3);
            entity.Property(e => e.SendNsremail).HasColumnName("SendNSREmail");
            entity.Property(e => e.ServiceRegion).HasMaxLength(5);
            entity.Property(e => e.State).HasMaxLength(3);
            entity.Property(e => e.UnavailableEnddate).HasColumnType("smalldatetime");
            entity.Property(e => e.UnavailableStartDate).HasColumnType("smalldatetime");
        });

        modelBuilder.Entity<TechHierarchyBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TECH_HIERARCHY_BACKUP");

            entity.Property(e => e.Address1).HasMaxLength(40);
            entity.Property(e => e.Address2).HasMaxLength(40);
            entity.Property(e => e.Address3).HasMaxLength(40);
            entity.Property(e => e.Address4).HasMaxLength(40);
            entity.Property(e => e.AlternativePhone).HasMaxLength(15);
            entity.Property(e => e.AreaCode).HasMaxLength(6);
            entity.Property(e => e.BranchName).HasMaxLength(50);
            entity.Property(e => e.BranchNumber).HasMaxLength(10);
            entity.Property(e => e.BranchType).HasMaxLength(20);
            entity.Property(e => e.BusinessUnit).HasMaxLength(15);
            entity.Property(e => e.City).HasMaxLength(25);
            entity.Property(e => e.CompanyName).HasMaxLength(40);
            entity.Property(e => e.Contact).HasMaxLength(40);
            entity.Property(e => e.CustomerOwnNumber).HasMaxLength(10);
            entity.Property(e => e.DealerType).HasMaxLength(2);
            entity.Property(e => e.EmailCc)
                .HasMaxLength(150)
                .HasColumnName("EmailCC");
            entity.Property(e => e.FamilyAff).HasMaxLength(10);
            entity.Property(e => e.FamilyAffDesc).HasMaxLength(30);
            entity.Property(e => e.Fax).HasMaxLength(50);
            entity.Property(e => e.FieldServiceManager)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedByFtp)
                .HasColumnType("datetime")
                .HasColumnName("LastModifiedByFTP");
            entity.Property(e => e.LongAddressNumber).HasMaxLength(25);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedUserId).HasColumnName("ModifiedUserID");
            entity.Property(e => e.NextelPhone).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(25);
            entity.Property(e => e.PostalCode).HasMaxLength(12);
            entity.Property(e => e.PricingParentName).HasMaxLength(50);
            entity.Property(e => e.RegionName)
                .HasMaxLength(85)
                .IsUnicode(false);
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.RimEmail).HasMaxLength(150);
            entity.Property(e => e.SearchDesc).HasMaxLength(30);
            entity.Property(e => e.SearchType).HasMaxLength(3);
            entity.Property(e => e.SendNsremail).HasColumnName("SendNSREmail");
            entity.Property(e => e.ServiceRegion).HasMaxLength(5);
            entity.Property(e => e.State).HasMaxLength(3);
            entity.Property(e => e.UnavailableEnddate).HasColumnType("smalldatetime");
            entity.Property(e => e.UnavailableStartDate).HasColumnType("smalldatetime");
        });

        modelBuilder.Entity<TechOnCall>(entity =>
        {
            entity.ToTable("TechOnCall");

            entity.Property(e => e.TechOnCallId).HasColumnName("TechOnCallID");
            entity.Property(e => e.EntryUserId).HasColumnName("EntryUserID");
            entity.Property(e => e.EntryUserName)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ModifiedUserId).HasColumnName("ModifiedUserID");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.ScheduleCreatedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ScheduleDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ScheduleEndDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ScheduleEndTime).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ScheduleStartTime).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<TechSchedule>(entity =>
        {
            entity.ToTable("TechSchedule");

            entity.Property(e => e.TechScheduleId).HasColumnName("TechScheduleID");
            entity.Property(e => e.AppointmentSubject)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.Availability)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.EntryUserId).HasColumnName("EntryUserID");
            entity.Property(e => e.EntryUserName)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ModifiedUserId).HasColumnName("ModifiedUserID");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.ReplaceTech).HasDefaultValue(0);
            entity.Property(e => e.ScheduleCreatedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ScheduleDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ScheduleEndTime).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ScheduleStartEndId).HasColumnName("scheduleStartEndID");
            entity.Property(e => e.ScheduleStartTime).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.WorkOrderId).HasColumnName("WorkOrderID");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.TechSchedules)
                .HasForeignKey(d => d.WorkOrderId)
                .HasConstraintName("FK__TechSched__WorkO__57DD0BE4");
        });

        modelBuilder.Entity<ThirdPartyContractMaintenance>(entity =>
        {
            entity.HasKey(e => e.ContractMaintenanceid).HasName("PK_thirdpartycontractmaintenance");

            entity.ToTable("ThirdPartyContractMaintenance");

            entity.Property(e => e.LaborHourlyRate).HasColumnType("money");
            entity.Property(e => e.LaborOvertimeRate).HasColumnType("money");
            entity.Property(e => e.PartsUpCharge).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RatePerPallet).HasColumnType("money");
            entity.Property(e => e.TravelHourlyRate).HasColumnType("money");
            entity.Property(e => e.TravelOvertimeRate).HasColumnType("money");
            entity.Property(e => e.TravelRatePerMile).HasColumnType("money");
        });

        modelBuilder.Entity<ThirdpartyConMaintenanceZonerate>(entity =>
        {
            entity.HasKey(e => e.ZoneRateid).HasName("PK_thirdpartconmaintenancezonerates");

            entity.ToTable("ThirdpartyConMaintenanceZonerate");

            entity.Property(e => e.BasedOn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Rate).HasColumnType("money");
        });

        modelBuilder.Entity<TmpBlackBerryNsr>(entity =>
        {
            entity.HasKey(e => e.UniqueInvoiceId);

            entity.ToTable("TMP_BlackBerry_NSR");

            entity.Property(e => e.UniqueInvoiceId).HasColumnName("UniqueInvoiceID");
            entity.Property(e => e.Comments).IsUnicode(false);
            entity.Property(e => e.Reasion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.TechId).HasColumnName("TechID");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<TmpBlackBerryScfassetInfo>(entity =>
        {
            entity.HasKey(e => e.UniqueId);

            entity.ToTable("TMP_BlackBerry_SCFAssetInfo");

            entity.Property(e => e.UniqueId).HasColumnName("UniqueID");
            entity.Property(e => e.CategoryDesc)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CloseCall).HasDefaultValue((short)0);
            entity.Property(e => e.ClosureConfirmationNo).HasMaxLength(50);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("smalldatetime");
            entity.Property(e => e.EquipTypeCode).HasMaxLength(5);
            entity.Property(e => e.EventUpdated).HasDefaultValue((short)0);
            entity.Property(e => e.ManufacturerDesc)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Notes).HasColumnType("ntext");
            entity.Property(e => e.Ratio)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ReasonCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Sent5191Email).HasDefaultValue((short)0);
            entity.Property(e => e.SerialNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SpawnEventId).HasColumnName("SpawnEventID");
            entity.Property(e => e.SpawnOrderCreated).HasDefaultValue((short)0);
            entity.Property(e => e.TechId).HasColumnName("TechID");
            entity.Property(e => e.TechName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.Temperature)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.VendorCode).HasMaxLength(20);
            entity.Property(e => e.Weight)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<TmpBlackBerryScfinvoiceInfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TMP_BlackBerry_SCFInvoiceInfo");

            entity.Property(e => e.AdditionalFollowupReq).HasMaxLength(10);
            entity.Property(e => e.ArrivalDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.CompletionDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.CustomerSign).HasColumnType("text");
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("smalldatetime");
            entity.Property(e => e.FollowupComments).HasColumnType("ntext");
            entity.Property(e => e.HardnessRating)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.Property(e => e.IsOperational).HasMaxLength(10);
            entity.Property(e => e.IsUnderWarrenty).HasMaxLength(10);
            entity.Property(e => e.ReviewedBy).HasMaxLength(150);
            entity.Property(e => e.ServiceDelayReason).HasColumnType("ntext");
            entity.Property(e => e.SignedBy)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.StartDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.StateofEquipment).HasColumnType("ntext");
            entity.Property(e => e.TechId).HasColumnName("TechID");
            entity.Property(e => e.TechName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.TechnicianSign).HasColumnType("text");
            entity.Property(e => e.TroubleshootSteps).HasColumnType("ntext");
            entity.Property(e => e.UniqueInvoiceId)
                .ValueGeneratedOnAdd()
                .HasColumnName("UniqueInvoiceID");
            entity.Property(e => e.WarrentyFor).HasMaxLength(150);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<TmpBlackBerryScfpartsInfo>(entity =>
        {
            entity.HasKey(e => e.UniquePartsId);

            entity.ToTable("TMP_BlackBerry_SCFPartsInfo");

            entity.Property(e => e.UniquePartsId).HasColumnName("UniquePartsID");
            entity.Property(e => e.AssetId).HasColumnName("AssetID");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("smalldatetime");
            entity.Property(e => e.EntryNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ItemNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RecExists)
                .HasDefaultValue((short)0)
                .HasColumnName("recExists");
            entity.Property(e => e.Sku)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.Skuid).HasColumnName("SKUID");
            entity.Property(e => e.TechId).HasColumnName("TechID");
            entity.Property(e => e.VendorNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<TmpInvTransfer>(entity =>
        {
            entity.HasKey(e => e.Transferid).HasName("PK_tmp_invtransfer");

            entity.ToTable("TmpInvTransfer");

            entity.Property(e => e.Transferid).ValueGeneratedNever();
            entity.Property(e => e.Comments).HasColumnType("ntext");
            entity.Property(e => e.TransferDate).HasColumnType("smalldatetime");
        });

        modelBuilder.Entity<TmpSerialNoreport>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TMP_SerialNOReport");

            entity.Property(e => e.CallTypeDesc).HasMaxLength(100);
            entity.Property(e => e.CloseDate).HasColumnType("smalldatetime");
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.EntryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.FulfillmentStatus).HasMaxLength(30);
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.ManufacturerDesc).HasMaxLength(80);
            entity.Property(e => e.ProductDesc1).HasMaxLength(50);
            entity.Property(e => e.ProductNo).HasMaxLength(120);
            entity.Property(e => e.SearchDesc)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.SearchType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SerialNo).HasMaxLength(125);
        });

        modelBuilder.Entity<TmpfeastPart>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TMPFeastParts");

            entity.Property(e => e.Active)
                .HasMaxLength(255)
                .HasColumnName("ACTIVE");
            entity.Property(e => e.CatalogId).HasColumnName("catalog_ID");
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasColumnName("CATEGORY");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.EquipmentTag)
                .HasMaxLength(255)
                .HasColumnName("EQUIPMENT_TAG");
            entity.Property(e => e.JmsItemNumber)
                .HasMaxLength(255)
                .HasColumnName("JMS_ITEM_NUMBER");
            entity.Property(e => e.LastEditedDate)
                .HasColumnType("datetime")
                .HasColumnName("LAST_EDITED_DATE");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(255)
                .HasColumnName("MANUFACTURER");
            entity.Property(e => e.Model)
                .HasMaxLength(255)
                .HasColumnName("MODEL");
            entity.Property(e => e.Price).HasColumnName("PRICE");
            entity.Property(e => e.SkuActive).HasColumnName("SKuActive");
            entity.Property(e => e.VendItemNumber)
                .HasMaxLength(255)
                .HasColumnName("VEND_ITEM_NUMBER");
            entity.Property(e => e.VersionNumber)
                .HasMaxLength(255)
                .HasColumnName("VERSION_NUMBER");
        });

        modelBuilder.Entity<TmpinvTransferNonserialized>(entity =>
        {
            entity.HasKey(e => e.Transferid).HasName("PK_tmp_invtransfer_nonserialized");

            entity.ToTable("TmpinvTransferNonserialized");

            entity.Property(e => e.Transferid).ValueGeneratedNever();
            entity.Property(e => e.Bin).HasMaxLength(15);
            entity.Property(e => e.Description).HasMaxLength(90);
            entity.Property(e => e.ManufNumber).HasMaxLength(50);
            entity.Property(e => e.TechId).HasColumnName("TechID");
        });

        modelBuilder.Entity<TmpinvTransferSerialized>(entity =>
        {
            entity.HasKey(e => e.Transferid).HasName("PK_tmp_invtransfer_serialized");

            entity.ToTable("TmpinvTransferSerialized");

            entity.Property(e => e.Transferid).ValueGeneratedNever();
            entity.Property(e => e.Bin).HasMaxLength(20);
            entity.Property(e => e.EquipmentStatus).HasMaxLength(20);
            entity.Property(e => e.FileName).HasMaxLength(50);
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.TechId).HasColumnName("TechID");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<UnKnownCustomerLog>(entity =>
        {
            entity.HasKey(e => e.CustomerLogId);

            entity.ToTable("UnKnownCustomerLog");

            entity.Property(e => e.CustomerLogId).HasColumnName("CustomerLogID");
            entity.Property(e => e.ModifiedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ModifiedUserId).HasColumnName("ModifiedUserID");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.NewCustomerId).HasColumnName("NewCustomerID");
            entity.Property(e => e.OldCustomerId).HasColumnName("OldCustomerID");
        });

        modelBuilder.Entity<UserApplication>(entity =>
        {
            entity.ToTable("UserApplication");

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.HasOne(d => d.Application).WithMany(p => p.UserApplications)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK_UserApplication_Application");

            entity.HasOne(d => d.Privilege).WithMany(p => p.UserApplications)
                .HasForeignKey(d => d.PrivilegeId)
                .HasConstraintName("FK_UserApplication_Privilege1");

            entity.HasOne(d => d.User).WithMany(p => p.UserApplications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserApplication_FbUserMaster");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => new { e.Techid, e.IsInvoice }).HasName("pk_techid_userprofile");

            entity.ToTable("UserProfile");

            entity.Property(e => e.AcceptDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ApiKey).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.DispatchEmails)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.LastAcceptDate).HasColumnType("smalldatetime");
            entity.Property(e => e.TechType).HasMaxLength(25);
            entity.Property(e => e.TokenKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UserActive).HasDefaultValue(1);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");
            entity.Property(e => e.UserName).HasMaxLength(70);
            entity.Property(e => e.UsrPassword).HasMaxLength(50);
        });

        modelBuilder.Entity<UserRefreshtoken>(entity =>
        {
            entity.HasKey(e => e.Uniqueid).HasName("PK__UserRefr__A2A5A15230837E40");

            entity.ToTable("UserRefreshtoken");

            entity.Property(e => e.Refreshtoken).HasColumnType("ntext");
            entity.Property(e => e.Refreshtokenexpirytime).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.UserRefreshtokens)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("FK__UserRefre__Useri__789EE131");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.TypeId);

            entity.ToTable("UserType");

            entity.Property(e => e.TypeId).ValueGeneratedNever();
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VAllPmandServiceEventsWithUniqueSerial>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_AllPMandServiceEventsWithUniqueSerial");

            entity.Property(e => e.Category).HasMaxLength(90);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.Model).HasMaxLength(90);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.WorkorderCallstatus)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderCloseDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VContact>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_Contact");

            entity.Property(e => e.AcctRep).HasMaxLength(3);
            entity.Property(e => e.AcctRepDesc).HasMaxLength(30);
            entity.Property(e => e.Address1).HasMaxLength(150);
            entity.Property(e => e.Address2).HasMaxLength(100);
            entity.Property(e => e.Address3).HasMaxLength(40);
            entity.Property(e => e.AnniversaryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.AreaCode).HasMaxLength(6);
            entity.Property(e => e.Branch).HasMaxLength(3);
            entity.Property(e => e.BrewmaticAgentCode).HasMaxLength(3);
            entity.Property(e => e.BusinessUnit).HasMaxLength(12);
            entity.Property(e => e.CategoryCode).HasMaxLength(3);
            entity.Property(e => e.Ccmjde).HasColumnName("CCMJDE");
            entity.Property(e => e.Chain).HasMaxLength(3);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.CustomerBranch).HasMaxLength(50);
            entity.Property(e => e.CustomerRegion).HasMaxLength(50);
            entity.Property(e => e.CustomerSpecialInstructions).IsUnicode(false);
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.DelDayOrFob)
                .HasMaxLength(3)
                .HasColumnName("DelDayOrFOB");
            entity.Property(e => e.DeliveryDesc).HasMaxLength(30);
            entity.Property(e => e.DeliveryMethod).HasMaxLength(3);
            entity.Property(e => e.DistributorName).HasMaxLength(40);
            entity.Property(e => e.District).HasMaxLength(3);
            entity.Property(e => e.DistrictDesc).HasMaxLength(30);
            entity.Property(e => e.Division).HasMaxLength(3);
            entity.Property(e => e.DivisionDesc).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EmailCc)
                .HasMaxLength(150)
                .HasColumnName("EmailCC");
            entity.Property(e => e.Expr1).HasMaxLength(40);
            entity.Property(e => e.Expr2).HasMaxLength(15);
            entity.Property(e => e.Expr3).HasMaxLength(10);
            entity.Property(e => e.FamilyAff).HasMaxLength(3);
            entity.Property(e => e.FamilyAffDesc).HasMaxLength(30);
            entity.Property(e => e.FbproviderId).HasColumnName("FBProviderID");
            entity.Property(e => e.FieldServiceManager).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Fsmjde).HasColumnName("FSMJDE");
            entity.Property(e => e.LastAutoEmail).HasColumnType("smalldatetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedByFtp)
                .HasColumnType("datetime")
                .HasColumnName("LastModifiedByFTP");
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.LastSaleDate).HasMaxLength(15);
            entity.Property(e => e.LongAddressNumber).HasMaxLength(30);
            entity.Property(e => e.MailingName).HasMaxLength(40);
            entity.Property(e => e.MiddleName).HasMaxLength(25);
            entity.Property(e => e.Nofbpspemails).HasColumnName("NOFBPSPEmails");
            entity.Property(e => e.Ntr)
                .HasMaxLength(3)
                .HasColumnName("NTR");
            entity.Property(e => e.OperatingUnit).HasMaxLength(3);
            entity.Property(e => e.Phone).HasMaxLength(25);
            entity.Property(e => e.PhoneWithAreaCode).HasMaxLength(30);
            entity.Property(e => e.PostalCode).HasMaxLength(12);
            entity.Property(e => e.PricingParent).HasMaxLength(8);
            entity.Property(e => e.PricingParentDesc).HasMaxLength(255);
            entity.Property(e => e.PricingParentId)
                .HasMaxLength(20)
                .HasColumnName("PricingParentID");
            entity.Property(e => e.PricingParentName).HasMaxLength(40);
            entity.Property(e => e.Rccmjde).HasColumnName("RCCMJDE");
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.RimEmail).HasMaxLength(150);
            entity.Property(e => e.Route).HasMaxLength(10);
            entity.Property(e => e.RouteCode).HasMaxLength(10);
            entity.Property(e => e.SearchDesc).HasMaxLength(30);
            entity.Property(e => e.SearchType).HasMaxLength(3);
            entity.Property(e => e.ServiceLevelCode).HasMaxLength(5);
            entity.Property(e => e.State).HasMaxLength(3);
            entity.Property(e => e.TierDesc).HasMaxLength(30);
            entity.Property(e => e.ZoneNumber).HasMaxLength(3);
            entity.Property(e => e._1099reporting)
                .HasMaxLength(3)
                .HasColumnName("1099Reporting");
        });

        modelBuilder.Entity<VContactServiceHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_ContactServiceHistory");

            entity.Property(e => e.CallTypeDesc).HasMaxLength(255);
            entity.Property(e => e.CallTypeId).HasColumnName("CallTypeID");
            entity.Property(e => e.Category).HasMaxLength(90);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(150);
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.Model).HasMaxLength(90);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.SymptomDesc).HasMaxLength(255);
            entity.Property(e => e.SymptomId).HasColumnName("SymptomID");
            entity.Property(e => e.WorkorderCallstatus)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VEquipmentCount>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_EquipmentCount");

            entity.Property(e => e.BranchName).HasMaxLength(50);
            entity.Property(e => e.BranchNumber).HasMaxLength(10);
            entity.Property(e => e.CompanyName).HasMaxLength(40);
            entity.Property(e => e.WorkorderCalltypeDesc)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderCloseDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<VLastOneyearDispatch>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_LastOneyearDispatches");

            entity.Property(e => e.Address1).HasMaxLength(40);
            entity.Property(e => e.Address2).HasMaxLength(40);
            entity.Property(e => e.City).HasMaxLength(25);
            entity.Property(e => e.CompanyName).HasMaxLength(40);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.PostalCode).HasMaxLength(12);
            entity.Property(e => e.ScheduleDate).HasColumnType("datetime");
            entity.Property(e => e.State).HasMaxLength(3);
            entity.Property(e => e.WorkorderCallstatus)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VLastOneyearInvoice>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_LastOneyearInvoices");

            entity.Property(e => e.ArrivalDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.CompletionDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.InvoiceDate).HasColumnType("smalldatetime");
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.Property(e => e.ResponsibleTechName).HasMaxLength(90);
            entity.Property(e => e.StartDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.WorkorderCallstatus)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VOpenCallTime>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_OpenCallTime");

            entity.Property(e => e.Address1).HasMaxLength(150);
            entity.Property(e => e.AssignedStatus)
                .HasMaxLength(55)
                .IsUnicode(false);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.CustomerBranch).HasMaxLength(50);
            entity.Property(e => e.CustomerBranchNo).HasMaxLength(3);
            entity.Property(e => e.CustomerRegion).HasMaxLength(50);
            entity.Property(e => e.DispatchCompany).HasMaxLength(40);
            entity.Property(e => e.ElapsedTime)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FamilyAff).HasMaxLength(10);
            entity.Property(e => e.PostalCode).HasMaxLength(12);
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.ScheduleDate).HasColumnType("datetime");
            entity.Property(e => e.SearchDesc).HasMaxLength(30);
            entity.Property(e => e.SearchType).HasMaxLength(3);
            entity.Property(e => e.State).HasMaxLength(3);
            entity.Property(e => e.WorkorderCallstatus)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VOriginalCallDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_OriginalCallDetails");

            entity.Property(e => e.Address1).HasMaxLength(150);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.CustomerBranch).HasMaxLength(50);
            entity.Property(e => e.CustomerBranchNo).HasMaxLength(3);
            entity.Property(e => e.CustomerRegion).HasMaxLength(50);
            entity.Property(e => e.DispatchCompany).HasMaxLength(40);
            entity.Property(e => e.ElapsedTime)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode).HasMaxLength(12);
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.Scheduledate).HasColumnType("datetime");
            entity.Property(e => e.State).HasMaxLength(3);
            entity.Property(e => e.WorkorderCallstatus)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VRepeatServiceRepair>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_RepeatServiceRepair");

            entity.Property(e => e.Category).HasMaxLength(90);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.Model).HasMaxLength(90);
            entity.Property(e => e.RepeatSerialNumber).HasMaxLength(50);
            entity.Property(e => e.RepeatedCallTypeId).HasColumnName("RepeatedCallTypeID");
            entity.Property(e => e.RepeatedCategory).HasMaxLength(90);
            entity.Property(e => e.RepeatedCloseDate).HasColumnType("datetime");
            entity.Property(e => e.RepeatedEntryDate).HasColumnType("datetime");
            entity.Property(e => e.RepeatedManufacturer).HasMaxLength(90);
            entity.Property(e => e.RepeatedModel).HasMaxLength(90);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.WorkorderCloseDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VRepeatServiceRepairDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_RepeatServiceRepairDetails");

            entity.Property(e => e.Branch).HasMaxLength(3);
            entity.Property(e => e.Category).HasMaxLength(90);
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.CustomerBranch).HasMaxLength(50);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CustomerRegion).HasMaxLength(50);
            entity.Property(e => e.Expr1).HasMaxLength(50);
            entity.Property(e => e.FamilyAff).HasMaxLength(10);
            entity.Property(e => e.Fsmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("FSMName");
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.OriginalEventTechId).HasColumnName("OriginalEventTechID");
            entity.Property(e => e.OriginalEventTechName).HasMaxLength(40);
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.RepeatSerialNumber).HasMaxLength(50);
            entity.Property(e => e.RepeatedCallTypeId).HasColumnName("RepeatedCallTypeID");
            entity.Property(e => e.RepeatedCategory).HasMaxLength(90);
            entity.Property(e => e.RepeatedCloseDate).HasColumnType("datetime");
            entity.Property(e => e.RepeatedEntryDate).HasColumnType("datetime");
            entity.Property(e => e.SearchDesc).HasMaxLength(30);
            entity.Property(e => e.SearchType).HasMaxLength(3);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.WorkorderCloseDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VRepeatedPmandServiceEvent>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_RepeatedPMandServiceEvents");

            entity.Property(e => e.Category).HasMaxLength(90);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.Model).HasMaxLength(90);
            entity.Property(e => e.RepeatSerialNumber).HasMaxLength(50);
            entity.Property(e => e.RepeatedCallTypeId).HasColumnName("RepeatedCallTypeID");
            entity.Property(e => e.RepeatedCategory).HasMaxLength(90);
            entity.Property(e => e.RepeatedCloseDate).HasColumnType("datetime");
            entity.Property(e => e.RepeatedEntryDate).HasColumnType("datetime");
            entity.Property(e => e.RepeatedManufacturer).HasMaxLength(90);
            entity.Property(e => e.RepeatedModel).HasMaxLength(90);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.WorkorderCloseDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VRepeatedPmandServiceEventDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_RepeatedPMandServiceEventDetails");

            entity.Property(e => e.Branch).HasMaxLength(3);
            entity.Property(e => e.Category).HasMaxLength(90);
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.CustomerBranch).HasMaxLength(50);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CustomerRegion).HasMaxLength(50);
            entity.Property(e => e.Expr1).HasMaxLength(50);
            entity.Property(e => e.FamilyAff).HasMaxLength(10);
            entity.Property(e => e.Fsmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("FSMName");
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.OriginalEventTechId).HasColumnName("OriginalEventTechID");
            entity.Property(e => e.OriginalEventTechName).HasMaxLength(40);
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.RepeatSerialNumber).HasMaxLength(50);
            entity.Property(e => e.RepeatedCallTypeId).HasColumnName("RepeatedCallTypeID");
            entity.Property(e => e.RepeatedCategory).HasMaxLength(90);
            entity.Property(e => e.RepeatedCloseDate).HasColumnType("datetime");
            entity.Property(e => e.RepeatedEntryDate).HasColumnType("datetime");
            entity.Property(e => e.SearchDesc).HasMaxLength(30);
            entity.Property(e => e.SearchType).HasMaxLength(3);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.WorkorderCloseDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<VTechDispatchCountToday>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_techDispatchCountToday");
        });

        modelBuilder.Entity<VUniqueInvoiceTimingsByDealerId>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_UniqueInvoiceTimingsByDealerID");

            entity.Property(e => e.ArrivalDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.BranchName).HasMaxLength(50);
            entity.Property(e => e.BranchNumber).HasMaxLength(10);
            entity.Property(e => e.CompletionDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.Esm)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("ESM");
            entity.Property(e => e.FamilyAff).HasMaxLength(10);
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.Property(e => e.Region)
                .HasMaxLength(85)
                .IsUnicode(false);
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.ResponsibleTechName).HasMaxLength(90);
            entity.Property(e => e.StartDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendor");

            entity.Property(e => e.VendorId)
                .ValueGeneratedNever()
                .HasColumnName("VendorID");
            entity.Property(e => e.ApvendorNo)
                .HasMaxLength(255)
                .HasColumnName("APVendorNo");
            entity.Property(e => e.Contact).HasMaxLength(255);
            entity.Property(e => e.Fax).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
            entity.Property(e => e.VendorCode).HasMaxLength(255);
            entity.Property(e => e.VendorDescription).HasMaxLength(255);
        });

        modelBuilder.Entity<VoltageList>(entity =>
        {
            entity.HasKey(e => e.VoltageId);

            entity.ToTable("VoltageList");

            entity.Property(e => e.VoltageId)
                .ValueGeneratedNever()
                .HasColumnName("VoltageID");
            entity.Property(e => e.Voltage).HasMaxLength(255);
        });

        modelBuilder.Entity<VwFbroleFunction>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_FBRoleFunction");

            entity.Property(e => e.FunctionId).HasColumnName("FunctionID");
            entity.Property(e => e.FunctionName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ParentFunctionId).HasColumnName("ParentFunctionID");
            entity.Property(e => e.RoleFunctionId).HasColumnName("RoleFunctionID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
        });

        modelBuilder.Entity<VwTechHierarchy>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_tech_Hierarchy");

            entity.Property(e => e.AreaCode).HasMaxLength(6);
            entity.Property(e => e.Branch)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.BranchName).HasMaxLength(50);
            entity.Property(e => e.BranchNumber).HasMaxLength(10);
            entity.Property(e => e.Dsmid).HasColumnName("DSMID");
            entity.Property(e => e.Dsmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("DSMName");
            entity.Property(e => e.Dsmphone)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("DSMPhone");
            entity.Property(e => e.Esmid).HasColumnName("ESMID");
            entity.Property(e => e.Esmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ESMName");
            entity.Property(e => e.Esmphone)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ESMPhone");
            entity.Property(e => e.PreferredProvider).HasMaxLength(40);
            entity.Property(e => e.PricingParent).HasMaxLength(50);
            entity.Property(e => e.ProviderPhone).HasMaxLength(25);
            entity.Property(e => e.RegionName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.Rsmid).HasColumnName("RSMID");
            entity.Property(e => e.Rsmname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("RSMName");
            entity.Property(e => e.Rsmphone)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("RSMPhone");
            entity.Property(e => e.SearchType).HasMaxLength(3);
            entity.Property(e => e.TechEmail).HasMaxLength(150);
            entity.Property(e => e.TechId).HasColumnName("TechID");
            entity.Property(e => e.TechType).HasMaxLength(10);
            entity.Property(e => e.TechTypeDesc).HasMaxLength(30);
            entity.Property(e => e.TechZip)
                .HasMaxLength(12)
                .HasColumnName("TechZIP");
        });

        modelBuilder.Entity<WaterLineList>(entity =>
        {
            entity.HasKey(e => e.WaterLineId);

            entity.ToTable("WaterLineList");

            entity.Property(e => e.WaterLineId)
                .ValueGeneratedNever()
                .HasColumnName("WaterLineID");
            entity.Property(e => e.WaterLine).HasMaxLength(255);
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(e => e.WorkorderId).HasName("PK_workorder");

            entity.ToTable("WorkOrder");

            entity.HasIndex(e => e.CustomerState, "indx_WorkOrder_CustomerState");

            entity.HasIndex(e => e.WorkorderCallstatus, "indx_WorkOrder_WorkOrderCallStatus");

            entity.HasIndex(e => e.WorkorderCloseDate, "indx_WorkOrder_WorkorderCloseDate");

            entity.HasIndex(e => e.WorkorderEntryDate, "indx_WorkOrder_WorkorderEntryDate");

            entity.Property(e => e.WorkorderId)
                .ValueGeneratedNever()
                .HasColumnName("WorkorderID");
            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.AuthTransactionId)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.AutoDispatch)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("Yes");
            entity.Property(e => e.CallerName)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.CallerPhone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ClosedUserName)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.CurrentUserName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CustomerAddress)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CustomerCity)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.CustomerCustomerPreferences).HasColumnType("ntext");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CustomerMainContactName)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.CustomerMainEmail)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.CustomerName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.CustomerPhoneExtn)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.CustomerPo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("CustomerPO");
            entity.Property(e => e.CustomerState)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.CustomerZipCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DateNeeded).HasColumnType("smalldatetime");
            entity.Property(e => e.DistributorName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.EmailSent).HasDefaultValue((short)0);
            entity.Property(e => e.EntryUserName)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.EquipmentBrand)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EquipmentModel)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Estimate).HasColumnType("numeric(18, 2)");
            entity.Property(e => e.Etaupdated).HasColumnName("ETAUpdated");
            entity.Property(e => e.FinalEstimate).HasColumnType("numeric(18, 2)");
            entity.Property(e => e.FinalTransactionId)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FollowupCallId)
                .HasDefaultValue(603)
                .HasColumnName("FollowupCallID");
            entity.Property(e => e.Fsm)
                .HasMaxLength(55)
                .IsUnicode(false);
            entity.Property(e => e.Fsmid).HasColumnName("FSMID");
            entity.Property(e => e.Fsr).HasColumnName("FSR");
            entity.Property(e => e.Fsrname)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("FSRName");
            entity.Property(e => e.HoldDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.HoursOfOperation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MarketSegment)
                .HasMaxLength(55)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.Nte)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("NTE");
            entity.Property(e => e.OtherParts).HasColumnType("ntext");
            entity.Property(e => e.OtherPartsAddress1)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.OtherPartsAddress2)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.OtherPartsCity)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.OtherPartsContactName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.OtherPartsName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.OtherPartsPhone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.OtherPartsState)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.OtherPartsZip)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.OvertimeRequest)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.PartsShipTo).HasMaxLength(30);
            entity.Property(e => e.PaymentTerm)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProgramName)
                .HasMaxLength(55)
                .IsUnicode(false);
            entity.Property(e => e.ProjectFlatRate).HasColumnType("money");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.PurchaseOrder).HasMaxLength(40);
            entity.Property(e => e.ResponsibleTechName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ResponsibleTechPhone)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Rsm)
                .HasMaxLength(55)
                .IsUnicode(false);
            entity.Property(e => e.SecondaryTechName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.SecondaryTechPhone)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.ServiceTier)
                .HasMaxLength(55)
                .IsUnicode(false);
            entity.Property(e => e.ShippingPriority)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.SpecificTechnician)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.TechTeamLead)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.TechTeamLeadId).HasColumnName("TechTeamLeadID");
            entity.Property(e => e.TechType)
                .HasMaxLength(55)
                .IsUnicode(false);
            entity.Property(e => e.ThirdPartyPo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("ThirdPartyPO");
            entity.Property(e => e.TotalUnitPrice)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Tsm)
                .HasMaxLength(55)
                .IsUnicode(false);
            entity.Property(e => e.Tsmemail)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("TSMEmail");
            entity.Property(e => e.Tsmphone)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("TSMPhone");
            entity.Property(e => e.WorkOrderOpenedDateTime).HasColumnType("datetime");
            entity.Property(e => e.WorkorderCallstatus)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderCalltypeDesc)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderCloseDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderClosureConfirmationNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderContactName)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderContactPhone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderDaylightSaving)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderEntryDate).HasColumnType("datetime");
            entity.Property(e => e.WorkorderErfid).HasMaxLength(50);
            entity.Property(e => e.WorkorderModifiedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<WorkOrderBrand>(entity =>
        {
            entity.HasKey(e => e.UniqueId);

            entity.HasIndex(e => e.WorkorderId, "indx_WorkOrderBrands_workorderID");

            entity.Property(e => e.UniqueId).HasColumnName("UniqueID");
            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkOrderBrands)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__WorkOrder__Worko__27F8EE98");
        });

        modelBuilder.Entity<WorkorderBillingDetail>(entity =>
        {
            entity.HasKey(e => e.BillingId).HasName("PK__Workorde__F1656DF3204538AE");

            entity.Property(e => e.BillingCode)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("smalldatetime");
        });

        modelBuilder.Entity<WorkorderDetail>(entity =>
        {
            entity.HasKey(e => e.WorkorderDetailid).HasName("PK_workorderdetails");

            entity.Property(e => e.AdditionalFollowupReq).HasMaxLength(10);
            entity.Property(e => e.ArrivalDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.CompletionDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.CustomerEmail).HasMaxLength(70);
            entity.Property(e => e.CustomerName).HasMaxLength(90);
            entity.Property(e => e.CustomerSignature).HasColumnType("image");
            entity.Property(e => e.CustomerSignatureBy).HasMaxLength(250);
            entity.Property(e => e.EntryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.FollowupComments).HasColumnType("ntext");
            entity.Property(e => e.HardnessRating)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceDate).HasColumnType("smalldatetime");
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.Property(e => e.IsOperational).HasMaxLength(10);
            entity.Property(e => e.IsUnderWarrenty).HasMaxLength(10);
            entity.Property(e => e.Mileage).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ModifiedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Nsrreason).HasColumnName("NSRReason");
            entity.Property(e => e.OperationalComments).HasColumnType("ntext");
            entity.Property(e => e.ResponsibleTechName).HasMaxLength(90);
            entity.Property(e => e.ReviewedBy).HasMaxLength(150);
            entity.Property(e => e.ServiceDelayReason).HasColumnType("ntext");
            entity.Property(e => e.SpecialClosure)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StartDateTime).HasColumnType("smalldatetime");
            entity.Property(e => e.StateofEquipment).HasColumnType("ntext");
            entity.Property(e => e.TotalDissolvedSolids).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TravelTime).HasMaxLength(50);
            entity.Property(e => e.TroubleshootSteps).HasColumnType("ntext");
            entity.Property(e => e.WarrentyFor).HasMaxLength(150);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderDetails)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__Workorder__Worko__4C6B5938");
        });

        modelBuilder.Entity<WorkorderEquipment>(entity =>
        {
            entity.HasKey(e => e.Assetid).HasName("PK_workorder_equipment");

            entity.ToTable("WorkorderEquipment");

            entity.Property(e => e.Assetid).ValueGeneratedNever();
            entity.Property(e => e.CatalogId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CatalogID");
            entity.Property(e => e.Category).HasMaxLength(90);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.IsSlNumberImageExist).HasDefaultValue(false);
            entity.Property(e => e.Location).HasMaxLength(150);
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.Model).HasMaxLength(90);
            entity.Property(e => e.Name).HasMaxLength(70);
            entity.Property(e => e.Ratio)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.Settings).HasMaxLength(50);
            entity.Property(e => e.Temperature)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Weight)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.WorkDescription).HasColumnType("ntext");
            entity.Property(e => e.WorkPerformedCounter)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderEquipments)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__Workorder__Worko__4D5F7D71");
        });

        modelBuilder.Entity<WorkorderEquipmentRequested>(entity =>
        {
            entity.HasKey(e => e.Assetid).HasName("PK_workorder_equipment_requested");

            entity.ToTable("WorkorderEquipmentRequested");

            entity.Property(e => e.Assetid).ValueGeneratedNever();
            entity.Property(e => e.CatalogId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CatalogID");
            entity.Property(e => e.Category).HasMaxLength(90);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.IsSlNumberImageExist).HasDefaultValue(false);
            entity.Property(e => e.Location).HasMaxLength(150);
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.Model).HasMaxLength(90);
            entity.Property(e => e.Name).HasMaxLength(70);
            entity.Property(e => e.Ratio)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.Settings).HasMaxLength(50);
            entity.Property(e => e.Temperature)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Weight)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.WorkDescription).HasColumnType("ntext");
            entity.Property(e => e.WorkPerformedCounter)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderEquipmentRequesteds)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__Workorder__Worko__4D5F7D72");
        });

        modelBuilder.Entity<WorkorderImage>(entity =>
        {
            entity.HasKey(e => e.WorkorderImageid).HasName("PK_workorderimages");

            entity.Property(e => e.WorkorderImageid).ValueGeneratedNever();
            entity.Property(e => e.AssetId).HasColumnName("AssetID");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
            entity.Property(e => e.WorkorderImagePath).HasMaxLength(150);
            entity.Property(e => e.WorkorderPicture).HasColumnType("image");

            entity.HasOne(d => d.Asset).WithMany(p => p.WorkorderImages)
                .HasForeignKey(d => d.AssetId)
                .HasConstraintName("FK__Workorder__Asset__5F7E2DAC");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderImages)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__Workorder__Worko__4E53A1AA");
        });

        modelBuilder.Entity<WorkorderInstallationSurvey>(entity =>
        {
            entity.HasKey(e => e.Installsurveyid).HasName("pk_installsurveyid_woinstallsurvey");

            entity.ToTable("WorkorderInstallationSurvey");

            entity.Property(e => e.AssetId).HasColumnName("AssetID");
            entity.Property(e => e.AssetLocation).HasMaxLength(50);
            entity.Property(e => e.Comments).HasColumnType("ntext");
            entity.Property(e => e.CounterUnitSpace).HasMaxLength(10);
            entity.Property(e => e.ElectricalPhase).HasMaxLength(30);
            entity.Property(e => e.MachineAmperage).HasMaxLength(10);
            entity.Property(e => e.NemwNumber).HasMaxLength(50);
            entity.Property(e => e.UnitFitSpace)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Voltage).HasMaxLength(10);
            entity.Property(e => e.WaterLine).HasMaxLength(50);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Asset).WithMany(p => p.WorkorderInstallationSurveys)
                .HasForeignKey(d => d.AssetId)
                .HasConstraintName("FK__Workorder__Asset__0E6E26BF");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderInstallationSurveys)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__Workorder__Worko__4F47C5E3");
        });

        modelBuilder.Entity<WorkorderNonAudit>(entity =>
        {
            entity.HasKey(e => e.NonAuditid).HasName("PK_workorder_nonaudit");

            entity.ToTable("WorkorderNonAudit");

            entity.Property(e => e.NonAuditid).ValueGeneratedNever();
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Equipment).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderNonAudits)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__Workorder__Worko__503BEA1C");
        });

        modelBuilder.Entity<WorkorderPart>(entity =>
        {
            entity.HasKey(e => e.PartsIssueid).HasName("pk_partsissueid_workorder_parts");

            entity.Property(e => e.AssetId).HasColumnName("AssetID");
            entity.Property(e => e.Description).HasMaxLength(120);
            entity.Property(e => e.Manufacturer).HasMaxLength(90);
            entity.Property(e => e.ModelNo).HasMaxLength(50);
            entity.Property(e => e.ProdNo).HasMaxLength(50);
            entity.Property(e => e.Sku).HasMaxLength(50);
            entity.Property(e => e.StandardCost).HasColumnType("money");
            entity.Property(e => e.Total).HasColumnType("money");
            entity.Property(e => e.Tpspcost)
                .HasColumnType("money")
                .HasColumnName("TPSPCost");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderParts)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__Workorder__Worko__51300E55");
        });

        modelBuilder.Entity<WorkorderReasonlog>(entity =>
        {
            entity.HasKey(e => e.WorkorderLogid).HasName("pk_workorderlogid_workorder_reasonlog");

            entity.ToTable("WorkorderReasonlog");

            entity.Property(e => e.EntryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.LogDescription).HasMaxLength(90);
            entity.Property(e => e.NewAppointmentDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Notes).HasColumnType("ntext");
            entity.Property(e => e.OldAppointmentDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ReasonFor).HasMaxLength(90);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderReasonlogs)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__Workorder__Worko__5224328E");
        });

        modelBuilder.Entity<WorkorderSavedSearch>(entity =>
        {
            entity.HasKey(e => e.SavedSearchid).HasName("pk_savedsearchid_workorder_savedsearch");

            entity.ToTable("WorkorderSavedSearch");

            entity.Property(e => e.ApptDateTo).HasColumnType("smalldatetime");
            entity.Property(e => e.ApptdateFrom).HasColumnType("smalldatetime");
            entity.Property(e => e.AutoDispatch).HasMaxLength(5);
            entity.Property(e => e.City).HasMaxLength(70);
            entity.Property(e => e.DateFrom).HasColumnType("smalldatetime");
            entity.Property(e => e.DateTo).HasColumnType("smalldatetime");
            entity.Property(e => e.OriginalWorkOrderId).HasColumnName("OriginalWorkOrderID");
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.SavedSearchName).HasMaxLength(90);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.ServiceCompany).HasMaxLength(90);
            entity.Property(e => e.TechType).HasMaxLength(50);
            entity.Property(e => e.Technician).HasMaxLength(90);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.WorkorderCallStatus)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
            entity.Property(e => e.WorkorderType).HasMaxLength(150);
            entity.Property(e => e.ZipCode).HasMaxLength(10);
        });

        modelBuilder.Entity<WorkorderSchedule>(entity =>
        {
            entity.HasKey(e => e.Scheduleid).HasName("PK_workorder_schedule");

            entity.ToTable("WorkorderSchedule");

            entity.HasIndex(e => e.ScheduleDate, "indx_WorkorderSchedule_ScheduleDate");

            entity.HasIndex(e => e.ServiceCenterId, "indx_WorkorderSchedule_ServiceCenterID");

            entity.HasIndex(e => e.Techid, "indx_WorkorderSchedule_techID");

            entity.Property(e => e.Scheduleid).ValueGeneratedNever();
            entity.Property(e => e.AssignedStatus)
                .HasMaxLength(55)
                .IsUnicode(false);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.EventScheduleDate).HasColumnType("datetime");
            entity.Property(e => e.Fsmemail)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("FSMEmail");
            entity.Property(e => e.Fsmid).HasColumnName("FSMID");
            entity.Property(e => e.Fsmname)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("FSMName");
            entity.Property(e => e.Fsmphone)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("FSMPhone");
            entity.Property(e => e.ModifiedScheduleDate).HasColumnType("datetime");
            entity.Property(e => e.ScheduleContactName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.ScheduleDate).HasColumnType("datetime");
            entity.Property(e => e.ServiceCenterId).HasColumnName("ServiceCenterID");
            entity.Property(e => e.ServiceCenterName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.TeamLeadId).HasColumnName("TeamLeadID");
            entity.Property(e => e.TeamLeadName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.TechName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.TechPhone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Tsmemail)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("TSMEmail");
            entity.Property(e => e.Tsmid).HasColumnName("TSMID");
            entity.Property(e => e.Tsmname)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("TSMName");
            entity.Property(e => e.Tsmphone)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("TSMPhone");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");
            entity.Property(e => e.WsAppointmentDate)
                .HasColumnType("datetime")
                .HasColumnName("WS_AppointmentDate");
            entity.Property(e => e.WsArrivalDateTime)
                .HasColumnType("smalldatetime")
                .HasColumnName("WS_ArrivalDateTime");
            entity.Property(e => e.WsCompletionDateTime)
                .HasColumnType("smalldatetime")
                .HasColumnName("WS_CompletionDateTime");
            entity.Property(e => e.WsMileage)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("WS_Mileage");
            entity.Property(e => e.WsTravelTime)
                .HasMaxLength(50)
                .HasColumnName("WS_TravelTime");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderSchedules)
                .HasForeignKey(d => d.WorkorderId)
                .HasConstraintName("FK__Workorder__Worko__58D1301D");
        });

        modelBuilder.Entity<WorkorderServiceClaim>(entity =>
        {
            entity.HasKey(e => e.ServiceClaimid).HasName("pk_serviceclaimid_workorder_serviceclaim");

            entity.ToTable("WorkorderServiceClaim");

            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Asset).WithMany(p => p.WorkorderServiceClaims)
                .HasForeignKey(d => d.Assetid)
                .HasConstraintName("FK__Workorder__Asset__671F4F74");
        });

        modelBuilder.Entity<WorkorderStatusLog>(entity =>
        {
            entity.HasKey(e => e.WorkorderStatusLogId).HasName("pk_workorderstatuslogid_workorder_statuslog");

            entity.ToTable("WorkorderStatusLog");

            entity.Property(e => e.WorkorderStatusLogId).HasColumnName("WorkorderStatusLogID");
            entity.Property(e => e.StatusFrom)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.StatusTo)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.StausChangeDate).HasColumnType("smalldatetime");
            entity.Property(e => e.WorkorderId).HasColumnName("WorkorderID");

            entity.HasOne(d => d.Workorder).WithMany(p => p.WorkorderStatusLogs)
                .HasForeignKey(d => d.WorkorderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Workorder__workorderstatuslog");
        });

        modelBuilder.Entity<WorkorderType>(entity =>
        {
            entity.HasKey(e => e.CallTypeId);

            entity.ToTable("WorkorderType");

            entity.Property(e => e.CallTypeId)
                .ValueGeneratedNever()
                .HasColumnName("CallTypeID");
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<YabhiPosdetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YAbhiPOSDetails");

            entity.Property(e => e.ContingentName).HasMaxLength(255);
            entity.Property(e => e.Contingentid).HasColumnName("contingentid");
        });

        modelBuilder.Entity<YhcompassSurvey>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHCompassSurvey");

            entity.Property(e => e.Ampsid).HasColumnName("AMPSID");
            entity.Property(e => e.AssetLocation).HasMaxLength(150);
            entity.Property(e => e.CompassSurveyId).HasColumnName("CompassSurveyID");
            entity.Property(e => e.CounterUnitSpace).HasMaxLength(10);
            entity.Property(e => e.ElectricalPhase).HasMaxLength(30);
            entity.Property(e => e.ElectricalPhaseId).HasColumnName("ElectricalPhaseID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.MachineAmperage).HasMaxLength(50);
            entity.Property(e => e.NemaNumberId).HasColumnName("NemaNumberID");
            entity.Property(e => e.NemwNumber).HasMaxLength(50);
            entity.Property(e => e.SurveyLocation).HasMaxLength(255);
            entity.Property(e => e.UnitFitSpace)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.UnitSpaceId).HasColumnName("UnitSpaceID");
            entity.Property(e => e.UnitWeightId).HasColumnName("UnitWeightID");
            entity.Property(e => e.Voltage).HasMaxLength(90);
            entity.Property(e => e.VoltageId).HasColumnName("VoltageID");
            entity.Property(e => e.WaterLine).HasMaxLength(50);
            entity.Property(e => e.WaterLineId).HasColumnName("WaterLineID");
        });

        modelBuilder.Entity<Yhcontact>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHContact");

            entity.Property(e => e.AcctRep).HasMaxLength(3);
            entity.Property(e => e.AcctRepDesc).HasMaxLength(30);
            entity.Property(e => e.AcquisitionCode).HasMaxLength(3);
            entity.Property(e => e.ActionCenterJde).HasColumnName("ActionCenterJDE");
            entity.Property(e => e.Address1).HasMaxLength(40);
            entity.Property(e => e.Address2).HasMaxLength(40);
            entity.Property(e => e.Address3).HasMaxLength(40);
            entity.Property(e => e.Address4).HasMaxLength(40);
            entity.Property(e => e.AnniversaryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.AreaCode).HasMaxLength(6);
            entity.Property(e => e.Branch).HasMaxLength(3);
            entity.Property(e => e.BrewmaticAgentCode).HasMaxLength(3);
            entity.Property(e => e.BusinessLine).HasMaxLength(100);
            entity.Property(e => e.BusinessType).HasMaxLength(3);
            entity.Property(e => e.BusinessUnit).HasMaxLength(12);
            entity.Property(e => e.CategoryCode).HasMaxLength(3);
            entity.Property(e => e.Ccmjde).HasColumnName("CCMJDE");
            entity.Property(e => e.Ccmjde1).HasColumnName("CCMJDE1");
            entity.Property(e => e.Chain).HasMaxLength(3);
            entity.Property(e => e.City).HasMaxLength(25);
            entity.Property(e => e.Class).HasMaxLength(3);
            entity.Property(e => e.ClassDesc).HasMaxLength(30);
            entity.Property(e => e.CompanyName).HasMaxLength(40);
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.ContractId).HasColumnName("ContractID");
            entity.Property(e => e.County).HasMaxLength(25);
            entity.Property(e => e.CreditMessage).HasMaxLength(3);
            entity.Property(e => e.CreditMessageDesc).HasMaxLength(30);
            entity.Property(e => e.CustInternal).HasMaxLength(10);
            entity.Property(e => e.CustomerBranch).HasMaxLength(50);
            entity.Property(e => e.CustomerOwnNumber).HasMaxLength(10);
            entity.Property(e => e.CustomerRegion).HasMaxLength(50);
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.DelDayOrFob)
                .HasMaxLength(3)
                .HasColumnName("DelDayOrFOB");
            entity.Property(e => e.DeliveryDesc).HasMaxLength(30);
            entity.Property(e => e.DeliveryMethod).HasMaxLength(3);
            entity.Property(e => e.DistributorName).HasMaxLength(40);
            entity.Property(e => e.District).HasMaxLength(3);
            entity.Property(e => e.DistrictDesc).HasMaxLength(30);
            entity.Property(e => e.Division).HasMaxLength(3);
            entity.Property(e => e.DivisionDesc).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.Employee).HasMaxLength(1);
            entity.Property(e => e.FamilyAff).HasMaxLength(3);
            entity.Property(e => e.FamilyAffDesc).HasMaxLength(30);
            entity.Property(e => e.FbproviderId).HasColumnName("FBProviderID");
            entity.Property(e => e.FieldServiceManager).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(25);
            entity.Property(e => e.Fsmjde).HasColumnName("FSMJDE");
            entity.Property(e => e.Fsmjde1).HasColumnName("FSMJDE1");
            entity.Property(e => e.LastAutoEmail).HasColumnType("smalldatetime");
            entity.Property(e => e.LastInvDate).HasColumnType("datetime");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedByFtp)
                .HasColumnType("datetime")
                .HasColumnName("LastModifiedByFTP");
            entity.Property(e => e.LastName).HasMaxLength(25);
            entity.Property(e => e.LastSaleDate).HasMaxLength(15);
            entity.Property(e => e.LastSurveyed).HasColumnType("smalldatetime");
            entity.Property(e => e.LongAddressNumber).HasMaxLength(30);
            entity.Property(e => e.MailingName).HasMaxLength(40);
            entity.Property(e => e.MiddleName).HasMaxLength(25);
            entity.Property(e => e.Nofbpspemails).HasColumnName("NOFBPSPEmails");
            entity.Property(e => e.Ntr)
                .HasMaxLength(3)
                .HasColumnName("NTR");
            entity.Property(e => e.OldContactId).HasColumnName("OldContactID");
            entity.Property(e => e.OldSearchType).HasMaxLength(5);
            entity.Property(e => e.OperatingUnit).HasMaxLength(3);
            entity.Property(e => e.OrgStructure).HasMaxLength(3);
            entity.Property(e => e.Phone).HasMaxLength(25);
            entity.Property(e => e.PostalCode).HasMaxLength(12);
            entity.Property(e => e.PricingParent).HasMaxLength(8);
            entity.Property(e => e.PricingParentDesc).HasMaxLength(255);
            entity.Property(e => e.PricingParentId)
                .HasMaxLength(20)
                .HasColumnName("PricingParentID");
            entity.Property(e => e.PricingParentName).HasMaxLength(40);
            entity.Property(e => e.Rccmjde).HasColumnName("RCCMJDE");
            entity.Property(e => e.Rccmjde1).HasColumnName("RCCMJDE1");
            entity.Property(e => e.RegionDesc).HasMaxLength(30);
            entity.Property(e => e.RegionNumber).HasMaxLength(5);
            entity.Property(e => e.ReportingFamily).HasMaxLength(3);
            entity.Property(e => e.ReportingFamilyDesc).HasMaxLength(30);
            entity.Property(e => e.Route).HasMaxLength(3);
            entity.Property(e => e.RouteCode).HasMaxLength(3);
            entity.Property(e => e.SalesRegion).HasMaxLength(3);
            entity.Property(e => e.SearchDesc).HasMaxLength(30);
            entity.Property(e => e.SearchType).HasMaxLength(3);
            entity.Property(e => e.SeasonalCustomer).HasMaxLength(3);
            entity.Property(e => e.ServiceLevelCode).HasMaxLength(5);
            entity.Property(e => e.State).HasMaxLength(3);
            entity.Property(e => e.TierDesc).HasMaxLength(30);
            entity.Property(e => e.ZoneNumber).HasMaxLength(3);
            entity.Property(e => e._1099reporting)
                .HasMaxLength(3)
                .HasColumnName("1099Reporting");
        });

        modelBuilder.Entity<YhcontactPmuploadsAll>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHContact_PMUploadsALL");

            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.ContactName).HasMaxLength(70);
            entity.Property(e => e.CustomerName).HasMaxLength(255);
            entity.Property(e => e.DayLightSaving)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EquipmentLocation).HasMaxLength(255);
            entity.Property(e => e.EquipmentModel).HasMaxLength(255);
            entity.Property(e => e.IntervalType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NextRunDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Notes).HasColumnType("ntext");
            entity.Property(e => e.Phone).HasMaxLength(80);
            entity.Property(e => e.StartDate).HasColumnType("smalldatetime");
            entity.Property(e => e.TechId).HasColumnName("TechID");
            entity.Property(e => e.TechName)
                .HasMaxLength(70)
                .IsUnicode(false);
            entity.Property(e => e.Tpsp)
                .HasMaxLength(255)
                .HasColumnName("TPSP");
            entity.Property(e => e.UniqueId).HasColumnName("UniqueID");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Yherfequipment>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHERFEquipment");

            entity.Property(e => e.EquipDescription).HasColumnType("text");
            entity.Property(e => e.EquipModelNo).HasMaxLength(25);
            entity.Property(e => e.EquipProdNo).HasMaxLength(25);
            entity.Property(e => e.EquipType).HasMaxLength(10);
            entity.Property(e => e.Erfno)
                .HasMaxLength(9)
                .HasColumnName("ERFNO");
            entity.Property(e => e.Extra).HasColumnType("money");
            entity.Property(e => e.Substitution).HasMaxLength(5);
            entity.Property(e => e.TotalPrice).HasColumnType("money");
            entity.Property(e => e.TransactionTypeId).HasColumnName("TransactionTypeID");
            entity.Property(e => e.UnitPrice).HasColumnType("money");
        });

        modelBuilder.Entity<Yherfexpendable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHERFExpendable");

            entity.Property(e => e.Erfno)
                .HasMaxLength(50)
                .HasColumnName("ERFNO");
            entity.Property(e => e.ExpDescription)
                .HasColumnType("ntext")
                .HasColumnName("EXpDescription");
            entity.Property(e => e.ExpModelNo).HasMaxLength(50);
            entity.Property(e => e.ExpProdNo).HasMaxLength(9);
            entity.Property(e => e.Extra).HasColumnType("money");
            entity.Property(e => e.TotalPrice).HasColumnType("money");
            entity.Property(e => e.TransactionTypeId).HasColumnName("TransactionTypeID");
            entity.Property(e => e.UnitPrice).HasColumnType("money");
        });

        modelBuilder.Entity<Yherfmaster>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHERFMaster");

            entity.Property(e => e.Approved).HasMaxLength(5);
            entity.Property(e => e.BilltoJde).HasColumnName("BilltoJDE");
            entity.Property(e => e.ChannelId).HasColumnName("ChannelID");
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.ContactName).HasMaxLength(50);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.DateErfprocessed)
                .HasColumnType("smalldatetime")
                .HasColumnName("DateERFProcessed");
            entity.Property(e => e.DateErfreceived)
                .HasColumnType("smalldatetime")
                .HasColumnName("DateERFReceived");
            entity.Property(e => e.DateOnErf)
                .HasColumnType("smalldatetime")
                .HasColumnName("DateOnERF");
            entity.Property(e => e.DayLightSaving).HasMaxLength(3);
            entity.Property(e => e.EntryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.EntryUserId).HasColumnName("EntryUserID");
            entity.Property(e => e.EquipEtadate)
                .HasColumnType("smalldatetime")
                .HasColumnName("EquipETADate");
            entity.Property(e => e.EquipOrderdate).HasColumnType("smalldatetime");
            entity.Property(e => e.Erfno)
                .HasMaxLength(9)
                .HasColumnName("ERFNO");
            entity.Property(e => e.Erfnotes)
                .HasColumnType("ntext")
                .HasColumnName("ERFNotes");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.FiscalId).HasColumnName("FiscalID");
            entity.Property(e => e.HoursOfOperation).HasMaxLength(50);
            entity.Property(e => e.InstallLocation).HasMaxLength(50);
            entity.Property(e => e.JdeprocessDate)
                .HasColumnType("smalldatetime")
                .HasColumnName("JDEProcessDate");
            entity.Property(e => e.ModifiedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ModifiedUserId).HasColumnName("ModifiedUserID");
            entity.Property(e => e.OriginalRequestedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.ReasonId).HasColumnName("ReasonID");
            entity.Property(e => e.SalesPerson).HasMaxLength(100);
            entity.Property(e => e.ShipToJde).HasColumnName("ShipToJDE");
            entity.Property(e => e.ShipToName).HasMaxLength(50);
            entity.Property(e => e.SiteReady).HasMaxLength(5);
            entity.Property(e => e.TimeErfprocessed)
                .HasColumnType("smalldatetime")
                .HasColumnName("TimeERFProcessed");
        });

        modelBuilder.Entity<YheventBrand>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHEventBrands");

            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.BrandName).HasMaxLength(50);
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<YheventClosurePart>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHEventClosureParts");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Newskuid).HasColumnName("NEWSKUID");
            entity.Property(e => e.Skuid).HasColumnName("SKUID");
        });

        modelBuilder.Entity<YheventNote>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHEventNotes");

            entity.Property(e => e.EnterDate).HasColumnType("datetime");
            entity.Property(e => e.EvenTid).HasColumnName("EvenTID");
            entity.Property(e => e.EventNotesId).HasColumnName("EventNotesID");
            entity.Property(e => e.Notes).HasColumnType("ntext");
            entity.Property(e => e.UniqueId).HasColumnName("UniqueID");
            entity.Property(e => e.UserId).HasColumnName("USerID");
            entity.Property(e => e.UserName)
                .HasMaxLength(40)
                .HasColumnName("USerName");
        });

        modelBuilder.Entity<YhfeastTechHierarchy>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHFeast_tech_Hierarchy");

            entity.Property(e => e.DefaultScLat).HasColumnName("DEFAULT_SC_LAT");
            entity.Property(e => e.DefaultScLng).HasColumnName("DEFAULT_SC_LNG");
            entity.Property(e => e.DefaultServiceCenter)
                .HasMaxLength(40)
                .HasColumnName("DEFAULT_SERVICE_CENTER");
            entity.Property(e => e.FsmDesc)
                .HasMaxLength(255)
                .HasColumnName("FSM_DESC");
            entity.Property(e => e.FsmId).HasColumnName("FSM_ID");
            entity.Property(e => e.FsmName)
                .HasMaxLength(75)
                .HasColumnName("FSM_NAME");
            entity.Property(e => e.FsmType)
                .HasMaxLength(50)
                .HasColumnName("FSM_TYPE");
            entity.Property(e => e.ServicecenterDesc)
                .HasMaxLength(255)
                .HasColumnName("SERVICECENTER_DESC");
            entity.Property(e => e.ServicecenterId).HasColumnName("SERVICECENTER_ID");
            entity.Property(e => e.ServicecenterName)
                .HasMaxLength(75)
                .HasColumnName("SERVICECENTER_NAME");
            entity.Property(e => e.ServicecenterPhone)
                .HasMaxLength(25)
                .HasColumnName("SERVICECENTER_PHONE");
            entity.Property(e => e.ServicecenterType)
                .HasMaxLength(50)
                .HasColumnName("SERVICECENTER_TYPE");
            entity.Property(e => e.ServicecenterZip)
                .HasMaxLength(25)
                .HasColumnName("SERVICECENTER_ZIP");
            entity.Property(e => e.TeamleadDesc)
                .HasMaxLength(255)
                .HasColumnName("TEAMLEAD_DESC");
            entity.Property(e => e.TeamleadId).HasColumnName("TEAMLEAD_ID");
            entity.Property(e => e.TeamleadName)
                .HasMaxLength(75)
                .HasColumnName("TEAMLEAD_NAME");
            entity.Property(e => e.TeamleadType)
                .HasMaxLength(50)
                .HasColumnName("TEAMLEAD_TYPE");
            entity.Property(e => e.TechCity)
                .HasMaxLength(50)
                .HasColumnName("TECH_CITY");
            entity.Property(e => e.TechDesc)
                .HasMaxLength(255)
                .HasColumnName("TECH_DESC");
            entity.Property(e => e.TechEmail)
                .HasMaxLength(148)
                .HasColumnName("TECH_EMAIL");
            entity.Property(e => e.TechId).HasColumnName("TECH_ID");
            entity.Property(e => e.TechName)
                .HasMaxLength(75)
                .HasColumnName("TECH_NAME");
            entity.Property(e => e.TechPhone)
                .HasMaxLength(25)
                .HasColumnName("TECH_PHONE");
            entity.Property(e => e.TechState)
                .HasMaxLength(25)
                .HasColumnName("TECH_STATE");
            entity.Property(e => e.TechType)
                .HasMaxLength(50)
                .HasColumnName("TECH_TYPE");
            entity.Property(e => e.TechVan).HasColumnName("TECH_VAN");
            entity.Property(e => e.Type)
                .HasMaxLength(8)
                .HasColumnName("TYPE");
        });

        modelBuilder.Entity<Yhsmevent>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHSMEvent");

            entity.Property(e => e.AppointmentDate).HasColumnType("smalldatetime");
            entity.Property(e => e.CallTypeDesc).HasMaxLength(100);
            entity.Property(e => e.CallerName).HasMaxLength(50);
            entity.Property(e => e.CloseDate).HasColumnType("smalldatetime");
            entity.Property(e => e.CloseUserId).HasColumnName("CloseUserID");
            entity.Property(e => e.ClosureConfirmationNo).HasMaxLength(50);
            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.DayLightSaving).HasMaxLength(2);
            entity.Property(e => e.EntryDate).HasColumnType("smalldatetime");
            entity.Property(e => e.EntryUserId).HasColumnName("EntryUserID");
            entity.Property(e => e.Erfno)
                .HasMaxLength(20)
                .HasColumnName("ERFNO");
            entity.Property(e => e.EventContact).HasMaxLength(40);
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Fsm).HasColumnName("FSM");
            entity.Property(e => e.FulfillmentStatus).HasMaxLength(30);
            entity.Property(e => e.ModifiedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Recallevent)
                .HasMaxLength(30)
                .HasColumnName("recallevent");
            entity.Property(e => e.SeriviceLevelCode).HasMaxLength(5);
            entity.Property(e => e.UniqueId).HasColumnName("UniqueID");
        });

        modelBuilder.Entity<YhsmeventInvoice>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHSMEventInvoice");

            entity.Property(e => e.ArrivalDateTime).HasColumnType("datetime");
            entity.Property(e => e.CompletionDateTime).HasColumnType("datetime");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.InvoiceDate).HasColumnType("smalldatetime");
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.Property(e => e.InvoiceUserId).HasColumnName("InvoiceUserID");
            entity.Property(e => e.ModifiedDate).HasColumnType("smalldatetime");
            entity.Property(e => e.StartDateTime).HasColumnType("datetime");
            entity.Property(e => e.TechId).HasColumnName("techID");
            entity.Property(e => e.Technician)
                .HasMaxLength(50)
                .HasColumnName("technician");
        });

        modelBuilder.Entity<YhsmeventSymcall>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHSMEventSymcall");

            entity.Property(e => e.CallTypeId).HasColumnName("CallTypeID");
            entity.Property(e => e.Category)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.CategoryDesc).HasMaxLength(80);
            entity.Property(e => e.Defective).HasMaxLength(5);
            entity.Property(e => e.Electricity).HasMaxLength(250);
            entity.Property(e => e.EquipTypeCode).HasMaxLength(5);
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Manufacturer).HasMaxLength(10);
            entity.Property(e => e.ManufacturerDesc).HasMaxLength(80);
            entity.Property(e => e.NeworReman).HasMaxLength(5);
            entity.Property(e => e.Plumbing).HasMaxLength(250);
            entity.Property(e => e.ProductNo).HasMaxLength(90);
            entity.Property(e => e.SerialNo).HasMaxLength(25);
            entity.Property(e => e.Settings).HasMaxLength(50);
            entity.Property(e => e.SymptomId).HasColumnName("SymptomID");
            entity.Property(e => e.Temperature).HasMaxLength(10);
        });

        modelBuilder.Entity<Yhsmschedule>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("YHSMSchedule");

            entity.Property(e => e.BranchId).HasColumnName("BranchID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.ModifiedScheduleDate).HasColumnType("datetime");
            entity.Property(e => e.ScheduleDate).HasColumnType("smalldatetime");
        });

        modelBuilder.Entity<Zip>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Zip");

            entity.Property(e => e.AreaCode).HasMaxLength(3);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(20);
            entity.Property(e => e.County).HasMaxLength(25);
            entity.Property(e => e.DayLightSaving).HasMaxLength(2);
            entity.Property(e => e.Fipscity)
                .HasMaxLength(5)
                .HasColumnName("FIPSCity");
            entity.Property(e => e.Fipscounty)
                .HasMaxLength(3)
                .HasColumnName("FIPSCounty");
            entity.Property(e => e.Fipsstate)
                .HasMaxLength(2)
                .HasColumnName("FIPSState");
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.TimeZoneName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UniqueId).HasColumnName("UniqueID");
            entity.Property(e => e.Zip1)
                .HasMaxLength(10)
                .HasColumnName("ZIP");
        });

        modelBuilder.Entity<ZonePriority>(entity =>
        {
            entity.HasKey(e => e.ZoneIndex).HasName("PK_zonepriority");

            entity.ToTable("ZonePriority");

            entity.Property(e => e.ZoneIndex).ValueGeneratedNever();
            entity.Property(e => e.Coordinates)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Fsmname)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("FSMName");
            entity.Property(e => e.OnCallBackupTechId).HasColumnName("OnCallBackupTechID");
            entity.Property(e => e.OnCallGroupId).HasColumnName("OnCallGroupID");
            entity.Property(e => e.OnCallPrimarytechId).HasColumnName("OnCallPrimarytechID");
            entity.Property(e => e.PhoneSolveTechId).HasColumnName("PhoneSolveTechID");
            entity.Property(e => e.PhoneSolveTechName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.ResponsibleTechBranchName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.ResponsibleTechName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.ResponsibleTechPhone)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.ResponsibletechId).HasColumnName("ResponsibletechID");
            entity.Property(e => e.Rsmname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RSMName");
            entity.Property(e => e.SecondaryTechBranchName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.SecondaryTechId).HasColumnName("SecondaryTechID");
            entity.Property(e => e.SecondaryTechName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.SecondaryTechPhone)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TechTeamLeadName)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.ZoneName)
                .HasMaxLength(90)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ZoneZip>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ZoneZip");

            entity.Property(e => e.UniqueId)
                .ValueGeneratedOnAdd()
                .HasColumnName("UniqueID");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.ZoneName)
                .HasMaxLength(90)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
