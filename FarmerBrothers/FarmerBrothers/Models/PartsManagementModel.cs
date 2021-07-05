using FarmerBrothers.Data;
using FarmerBrothers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmerBrothers.Models
{
    public enum PartsManagementSubmitType
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

    public class PartsManagementModel
    {
        public CustomerModel Customer;
        public WorkOrder WorkOrder;
        public NotesModel Notes;
        public Erf Erf;

        public WorkOrderManagementSubmitType Operation { get; set; }

        //public IList<AllFormalBrothersStatu> PriorityList;
        public IList<BrandName> BrandNames;

        public IList<BrandName> SelectedBrands;
        public string SelectedBrandIds;

        //public IList<AllFormalBrothersStatu> SalesNotificationReasonCodes;
        public string SalesNotificationNotes;
        public string SalesNotificationCode;

        public IList<WorkorderType> CallTypes;
        public IList<WorkorderType> ClosureCallTypes;
        public IList<EquipType> EquipmentTypes;

        public IList<State> States;

        public IList<WorkOrderManagementNonSerializedModel> NonSerializedList;
        public IList<WorkOrderManagementEquipmentModel> WorkOrderEquipments;

        public IList<WorkOrderPartModel> WorkOrderParts;

        public WorkOrderClosureModel Closure;

        public IList<CategoryModel> TaggedCategories;
        public IList<CategoryModel> NonTaggedCategories;

        public IList<KnownEquipmentModel> KnownEquipments;

        public IList<NewNotesModel> NewNotes;

        public IList<VendorDataModel> TaggedManufacturer;
        public IList<VendorDataModel> NonTaggedManufacturer;

        public IList<VendorModelModel> NonTaggedModels;

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


        public bool CustomerPartsOrder { get; set; }
        public bool TechPartsOrder { get; set; }

        public IList<int> AssistTechIds;
        public int? ResponsibleTechId;
        public IList<int> BranchIds;

        public IList<Solution> Solutions;

        public int PartsShipTo { get; set; }

        public string Customer_OtherPartsContactName { get; set; }
        public string Other_OtherPartsContactName { get; set; }

        public string OverTimeRequestDescription { get; set; }

        public WorkOrder FillCustomerData(WorkOrder entityWorkOrder, FarmerBrothersEntities FarmerBrothersEntities/*, FeastLocationService.Customer serviceCustomer = null*/)
        {
            foreach (var property in WorkOrder.GetType().GetProperties())
            {
                property.SetValue(entityWorkOrder, property.GetValue(WorkOrder));
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

            IEnumerable<FsmView> fsmViews = Utility.GetFsmData(FarmerBrothersEntities);
            FsmView fsmView = fsmViews.Where(f => f.FSM_Name == Customer.FSMName).FirstOrDefault();
            if (fsmView != null)
            {
                entityWorkOrder.FSMID = Convert.ToInt32(fsmView.FSM_ID);
            }

            //if (serviceCustomer != null)
            //{
            //    entityWorkOrder.MarketSegment = serviceCustomer.MarketSegment;
            //    entityWorkOrder.ProgramName = serviceCustomer.ProgramName;
            //    entityWorkOrder.DistributorName = serviceCustomer.DistributorName;
            //    entityWorkOrder.ServiceTier = serviceCustomer.ServiceTier;
            //}

            return entityWorkOrder;
        }
    }
}