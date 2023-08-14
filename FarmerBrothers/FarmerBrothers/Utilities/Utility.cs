using FarmerBrothers.Data;
using FarmerBrothers.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Configuration;
using System;
using System.Xml;
using System.Net.Mime;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Helpers;
using Newtonsoft.Json.Linq;
//using FarmerBrothers.FeastLocationService;
using System.Data.Entity.Infrastructure;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data.Common;
using System.Data;
using System.Data.Entity.Validation;

namespace FarmerBrothers.Utilities
{
    public class Utility
    {
        public static List<string> UserGroups = new List<string> { "CallCenter", "FBAccess", "WOMaintenance", "TPSPContract", "Scheduler", "Administration" };

        public static string FormatPhoneNumber(string phoneNumber)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {                    
                    phoneNumber = Regex.Replace(phoneNumber, @"[^\d]", "");
                    phoneNumber = Regex.Replace(phoneNumber, @"\s+", "");
                    if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Length > 0)
                    {
                        phoneNumber = phoneNumber.Substring(0, 10);
                    }
                    int xposition = phoneNumber.ToUpper().IndexOf('X');
                    if (phoneNumber.Length == 10)
                    {
                        phoneNumber = Regex.Replace(phoneNumber, @"-+", "");
                        phoneNumber = Regex.Replace(phoneNumber, @"\s+", "");
                        phoneNumber = String.Format("{0:(###)###-#### }", double.Parse(phoneNumber));
                    }
                    else if (xposition > 0)
                    {
                        string newPhoneNumber = phoneNumber.Substring(0, xposition);

                        newPhoneNumber = Regex.Replace(newPhoneNumber, @"-+", "");
                        newPhoneNumber = Regex.Replace(newPhoneNumber, @"\s+", "");
                        newPhoneNumber = String.Format("{0:(###)###-#### }", double.Parse(newPhoneNumber));

                        phoneNumber = newPhoneNumber + phoneNumber.Substring(xposition);

                    }
                }
            }
            catch (Exception e)
            {

            }
            return phoneNumber;
        }


        public static IEnumerable<TechHierarchyView> GetTechDataByServiceCenterId(FarmerBrothersEntities FarmerBrothersEntities, int serviceCenterId)
        {
            bool isInternal = true;

            IList<TechHierarchyView> techViews = FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(@"exec USP_GetTechDataByServiceCenterId @ServiceCenterId", new SqlParameter("@ServiceCenterId", serviceCenterId)).ToList();

            if (techViews != null && techViews.Count() > 0)
            {
                TechHierarchyView techView = techViews.ElementAt(0);
                if (techView != null)
                {
                    //LG : TODO :: Need to change this logic
                    //if (techView.TechID != techView.ServiceTire)
                    //{
                    //    isInternal = false;
                    //}
                }
            }

            //if (isInternal == true)
            //{
            //    techViews = FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(@"exec USP_GetInternalTechDataByServiceCenterId @ServiceCenterId", new SqlParameter("@ServiceCenterId", serviceCenterId)).ToList();
            //}

            return techViews;
        }

        public static IEnumerable<TechHierarchyView> GetTechDataByFSMId(FarmerBrothersEntities FarmerBrothersEntities, int fsmId)
        {
            string query = @"SELECT * FROM feast_tech_hierarchy where TECH_TYPE = 'Agent' and tech_desc in ('Team Lead','Service Tech') and FSM_Id  = " + fsmId.ToString();
            return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query);
        }

        public static IEnumerable<TechDispatchWithDistance> GetClosestTechDispatchWithDistance(FarmerBrothersEntities FarmerBrothersEntities, string zipCode)
        {
            return FarmerBrothersEntities.Database.SqlQuery<TechDispatchWithDistance>(@"exec USP_ClosestTechDispatch_Details @CustomerZip", new SqlParameter("@CustomerZip", zipCode.Substring(0, 5)));
        }

        public static IEnumerable<TechDispatchWithDistance> GetAfterHoursClosestTechDispatchWithDistance(FarmerBrothersEntities FarmerBrothersEntities, string zipCode)
        {
            return FarmerBrothersEntities.Database.SqlQuery<TechDispatchWithDistance>(@"exec USP_AfterHoursClosestTechDispatch_Details @CustomerZip", new SqlParameter("@CustomerZip", zipCode.Substring(0, 5)));
        }

        public static IEnumerable<TechDispatchWithDistance> GetTechDispatchWithDistance(FarmerBrothersEntities FarmerBrothersEntities, string zipCode,int workOrderId)
        {           
            return FarmerBrothersEntities.Database.SqlQuery<TechDispatchWithDistance>(@"exec USP_TechDispatch_Details @CustomerZip,@WorkOrderId", new SqlParameter("@CustomerZip", zipCode.Substring(0, 5)), new SqlParameter("@WorkOrderId", workOrderId));
        }

        public static IEnumerable<TechDispatchWithDistance> GetAfterHoursTechDispatchWithDistance(FarmerBrothersEntities FarmerBrothersEntities, string zipCode, int workOrderId)
        {          
            return FarmerBrothersEntities.Database.SqlQuery<TechDispatchWithDistance>(@"exec USP_AfterHoursTechDispatch_Details @CustomerZip,@WorkOrderId", new SqlParameter("@CustomerZip", zipCode.Substring(0, 5)), new SqlParameter("@WorkOrderId", workOrderId));
        }

        public static IEnumerable<TechHierarchyView> GetTechDataByTeamLeadId(FarmerBrothersEntities FarmerBrothersEntities, int teamLeadId)
        {
            string query = @"SELECT * FROM feast_tech_hierarchy where TECH_TYPE = 'Agent' and tech_desc in ('Team Lead','Service Tech') and TEAMLEAD_ID = " + teamLeadId.ToString();
            return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query);
        }

        public static IEnumerable<WorkOrder> GetWorkOrdersByFsmId(FarmerBrothersEntities FarmerBrothersEntities, int fsmId)
        {
            string query = @"SELECT wo.* FROM  WorkOrder wo INNER JOIN  WorkorderSchedule ws ON wo.WorkorderID = ws.WorkorderID 
                                Where dbo.WorkorderSchedule.Techid IN (Select Tech_ID from feast_tech_hierarchy 
                                where FSM_ID = " + fsmId.ToString() + " and TECH_TYPE = 'Agent' and tech_desc in ('Team Lead','Service Tech'))";
            return FarmerBrothersEntities.Database.SqlQuery<WorkOrder>(query);
        }

        //public static IEnumerable<TechHierarchyView> GetTechDataByServiceCenterId(FarmerBrothersEntities FarmerBrothersEntities, int serviceCenterId)
        //{
        //    string query = @"SELECT * FROM feast_tech_hierarchy where DEFAULT_SERVICE_CENTER  = " + serviceCenterId.ToString();
        //    return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query);
        //}



        public static IEnumerable<TechHierarchyView> GetTeamLeadsByServiceCenterId(FarmerBrothersEntities FarmerBrothersEntities, int serviceCenterId)
        {
            string query = @"Select * from TECH_HIERARCHY where dealerid  = " + serviceCenterId.ToString();

            return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query);
        }

        public static IEnumerable<TechHierarchyView> GetTechDataByBranchType(FarmerBrothersEntities FarmerBrothersEntities, string branchDesc, string branchType)
        {
            string query = string.Empty;
            if (string.IsNullOrWhiteSpace(branchType))
            {
                query = @"select distinct d.dealerid AS TechID, d.CompanyName +' - '+ d.city AS PreferredProvider from TECH_HIERARCHY d where searchType='SP' and FamilyAff !='SPT' 
                        and dealerID NOT IN (8888888,8888889,8888890,8888891,8888892,8888893,8888894,8888895,8888907,8888908,8888911,8888917,
                        8888918,8888941,8888942,8888945,8888953,9999999,8888897,9999995,9999998,8888980,8888981,8888984,9990061,9990062,9990065) order by PreferredProvider asc";
            }
            else
            {
                query = @"select distinct d.dealerid AS TechID, d.CompanyName +' - '+ d.city AS PreferredProvider from TECH_HIERARCHY d 
                            where searchType='SP' and dealerID NOT IN (8888888,8888889,8888890,8888891,8888892,8888893,8888894,8888895,8888907,8888908,8888911,8888917,
                        8888918,8888941,8888942,8888945,8888953,9999999,8888897,9999995,9999998,8888980,8888981,8888984,9990061,9990062,9990065) and FamilyAff = '" + branchType + "'"
                        + "order by PreferredProvider asc";
            }
            return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query);
        }

        public static IEnumerable<TechHierarchyView> GetAllTechDataByBranchType(FarmerBrothersEntities FarmerBrothersEntities)
        {
            //LG:: removed searchType = SP as per ram request on 5/3/18, now showing all type of technicians
            string query = string.Empty;
            query = @"
                        select distinct d.dealerid AS TechID, d.CompanyName +' - '+ d.city AS PreferredProvider from TECH_HIERARCHY d where 
                        dealerID NOT IN (8888888,8888889,8888890,8888891,8888892,8888893,8888894,8888895,8888907,8888908,8888911,8888917,
                        8888918,8888941,8888942,8888945,8888953,9999999,8888897,9999995,9999998,8888980,8888981,8888984,9990061,9990062,9990065)
                        and d.CompanyName +' - '+ d.city is not null
                        order by PreferredProvider asc";
            return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query);
        }

        public static IEnumerable<TechHierarchyView> Get3rdParyTechData(FarmerBrothersEntities FarmerBrothersEntities)
        {
            //LG:: removed searchType = SP as per ram request on 5/3/18, now showing all type of technicians
            string query = string.Empty;
            query = @"
                        select distinct d.dealerid AS TechID, d.CompanyName +' - '+ d.city AS PreferredProvider from TECH_HIERARCHY d where FamilyAff ='SPT'
                        and dealerID NOT IN (8888888,8888889,8888890,8888891,8888892,8888893,8888894,8888895,8888907,8888908,8888911,8888917,
                        8888918,8888941,8888942,8888945,8888953,9999999,8888897,9999995,9999998,8888980,8888981,8888984,9990061,9990062,9990065)
                        and d.CompanyName +' - '+ d.city is not null
                        order by PreferredProvider asc";
            return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query);
        }

        public static IEnumerable<TechHierarchyView> GetInternalTechData(FarmerBrothersEntities FarmerBrothersEntities)
        {
            //LG:: removed searchType = SP as per ram request on 5/3/18, now showing all type of technicians
            string query = string.Empty;
            query = @"
                        select distinct d.dealerid AS TechID, d.CompanyName +' - '+ d.city AS PreferredProvider from TECH_HIERARCHY d where  FamilyAff ='SPD'
                        and dealerID NOT IN (8888888,8888889,8888890,8888891,8888892,8888893,8888894,8888895,8888907,8888908,8888911,8888917,
                        8888918,8888941,8888942,8888945,8888953,9999999,8888897,9999995,9999998,8888980,8888981,8888984,9990061,9990062,9990065)
                        and d.CompanyName +' - '+ d.city is not null
                        order by PreferredProvider asc";
            return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query);
        }

        public static TechHierarchyView GetTechDataByResponsibleTechId(FarmerBrothersEntities FarmerBrothersEntities, int responsibleTechId)
        {
            string query = @"SELECT * FROM vw_tech_hierarchy where TechID = " + responsibleTechId.ToString();

            return FarmerBrothersEntities.Database.SqlQuery<TechHierarchyView>(query).FirstOrDefault();
        }


        public static IEnumerable<FsmView> GetFsmData(FarmerBrothersEntities FarmerBrothersEntities)
        {
            return FarmerBrothersEntities.Database.SqlQuery<FsmView>(
                                   "Select FSM_ID, FSM_Name from FEAST_FSM");
        }

        public static IEnumerable<TsmView> GetTsmData(FarmerBrothersEntities FarmerBrothersEntities)
        {
            return FarmerBrothersEntities.Database.SqlQuery<TsmView>(
                                   "SELECT * FROM FEAST_TSM");
        }

        public static decimal GetDistance(string fromAddress, string toAddress)
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

        public static dynamic GetTravelDetailsBetweenZipCodes(string fromAddress, string toAddress)
        {
            dynamic jObject = null;
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=");
                stringBuilder.Append("origins=");
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
                        jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        //if (jObject != null)
                        //{
                        //    var element = jObject.rows[0].elements[0];
                        //    duration = element.duration.value / 3600.00;
                        //    duration = Math.Round(duration, 2);
                        //}
                    }
                }
            }
            catch (Exception e)
            {

            }
            //return System.Convert.ToDecimal(duration); ;
            return jObject;
        }

        public static void PopulateDistances(IDictionary<decimal, string> branchZipCodes, IList<BranchModel> branches, string customerZip)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=");
                stringBuilder.Append("origins=");
                stringBuilder.Append(customerZip);
                stringBuilder.Append("&destinations=");

                int count = 0;
                IList<decimal> keys = new List<decimal>();
                foreach (KeyValuePair<decimal, string> pair in branchZipCodes)
                {
                    keys.Add(pair.Key);

                    stringBuilder.Append(pair.Value);
                    stringBuilder.Append("|");
                    if (count++ > 10)
                    {
                        break;
                    }
                }

                stringBuilder.Append("&key=AIzaSyB0w-ABC5reY1a7cyE_XMcl7_ztGdjcB5U");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" + stringBuilder.ToString());
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = client.GetAsync(stringBuilder.ToString()).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        for (int i = 0; i < keys.Count; i++)
                        {
                            var element = jObject.rows[0].elements[i];
                            BranchModel branchModel = branches.Where(b => b.Id == keys[i]).FirstOrDefault();

                            if (branchModel != null)
                            {
                                branchModel.Distance = element.distance.value * (decimal)0.000621371192;
                                branchModel.Distance = Math.Round(branchModel.Distance, 0);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        public static double GetDistance(FarmerBrothersEntities FarmerBrothersEntities, string fromZipCode, string toZipCode)
        {
            double distance = 0;

            try
            {
                if (!string.IsNullOrEmpty(toZipCode))
                {
                    string query = @"select dbo.get_DistanceFromZip('" + fromZipCode.Substring(0, 5) + "','" + toZipCode.Substring(0, 5) + "')";
                    DbRawSqlQuery<double> result = FarmerBrothersEntities.Database.SqlQuery<double>(query);

                    distance = result.ElementAt(0);
                    distance = Math.Round(distance, 2);

                    //StringBuilder stringBuilder = new StringBuilder();
                    //stringBuilder.Append("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=");
                    //stringBuilder.Append("origins=");
                    //stringBuilder.Append(fromZipCode);
                    //stringBuilder.Append("&destinations=");
                    //stringBuilder.Append(toZipCode);
                    //stringBuilder.Append("&key=AIzaSyB0w-ABC5reY1a7cyE_XMcl7_ztGdjcB5U");

                    //using (var client = new HttpClient())
                    //{
                    //    client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" + stringBuilder.ToString());
                    //    client.DefaultRequestHeaders.Accept.Clear();
                    //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //    HttpResponseMessage response = client.GetAsync(stringBuilder.ToString()).Result;

                    //    if (response.IsSuccessStatusCode)
                    //    {
                    //        dynamic jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    //        distance = jObject.rows[0].elements[0].distance.value;
                    //        distance = distance * (decimal)0.000621371192;
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {

            }

            return distance;
        }

        public static IList<UserType> GetUserType(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            IList<UserType> userType = FarmerBrothersEntitites.UserTypes.ToList();

            UserType blankType = new UserType() { TypeId = 0, TypeName = "Please Select" };
            userType.Insert(0, blankType);

            return userType;
        }
        public static IList<ApplicationModel> GetModules(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            IList<Application> modules = FarmerBrothersEntitites.Applications.ToList();

            List<ApplicationModel> appModule = new List<ApplicationModel>();
            foreach (Application item in modules)
            {
                ApplicationModel module = new ApplicationModel(item);
                appModule.Add(module);
            }
            //Application blankType = new Application() { ApplicationId = 0, ApplicationName = "" };
            //module.Insert(0, blankType);

            return appModule;
        }
        public static IList<PrivilegeModel> GetPrivileges(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            IList<Privilege> privilege = FarmerBrothersEntitites.Privileges.ToList();


            List<PrivilegeModel> privModule = new List<PrivilegeModel>();
            foreach (Privilege item in privilege)
            {
                PrivilegeModel module = new PrivilegeModel(item);
                privModule.Add(module);
            }

            //Privilege blankType = new Privilege() { ApplicationId = 0, ApplicationName = "" };
            //privilege.Insert(0, blankType);

            return privModule;
        }
        public static IList<State> GetStates(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            IList<State> states = FarmerBrothersEntitites.States.ToList();

            State blankState = new State() { StateCode = "n/a", StateName = "Please Select" };
            states.Insert(0, blankState);

            return states;
        }

        public static IList<CashSaleModel> GetCashSaleStatusList(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            IList<CashSaleModel> csList = new List<CashSaleModel>(){
                new CashSaleModel() {StatusCode="1", StatusName="Order Entered by Originator" },
                new CashSaleModel() {StatusCode="2", StatusName="Down Payment Invoice Created" },
                new CashSaleModel() {StatusCode="3", StatusName="Down Payment Applied" },
                new CashSaleModel() {StatusCode="4", StatusName="Order Processed with Vendor" },
                new CashSaleModel() {StatusCode="5", StatusName="Order Received" },
                new CashSaleModel() {StatusCode="6", StatusName="Complete, Final Invoice Created"} };

            CashSaleModel blankState = new CashSaleModel() { StatusCode = "", StatusName = " " };
            csList.Insert(0, blankState);

            return csList;
        }

        public static IList<NonFBCustomer> GetNonFBCustomers(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            IList<NonFBCustomer> nonFBCustomers = FarmerBrothersEntitites.NonFBCustomers.ToList();

            foreach(NonFBCustomer nonFbCust in nonFBCustomers)
            {
                nonFbCust.NonFBCustomerName = nonFbCust.NonFBCustomerName + " - " + nonFbCust.NonFBCustomerId;
            }

            NonFBCustomer blankState = new NonFBCustomer() { Id = -1, NonFBCustomerId = "n/a", NonFBCustomerName = "Please Select" };
            nonFBCustomers.Insert(0, blankState);

            return nonFBCustomers;
        }

        public static IList<Role> GetRoles(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            var roles = FarmerBrothersEntitites.FbRoles.ToList();

            List<Role> Roles = new List<Role>();
            foreach (var role in roles)
            {
                Role rl = new Role();
                rl.RoleId = role.RoleId;
                rl.RoleName = role.RoleName;
                Roles.Add(rl);
            }

            return Roles;
        }

        public static ZonePriority GetCustomerZonePriority(FarmerBrothersEntities FarmerBrothersEntitites, string zipCode)
        {
            ZonePriority zonePriority = null;
            ZoneZip zonezip = FarmerBrothersEntitites.ZoneZips.FirstOrDefault(z => z.ZipCode == zipCode.Substring(0, 5));
            if (zonezip != null)
            {
                zonePriority = FarmerBrothersEntitites.ZonePriorities.FirstOrDefault(zp => zp.ZoneIndex == zonezip.ZoneIndex);
            }
            return zonePriority;
        }

        public static CustomerModel PopulateCustomerWithZonePriorityDetails(FarmerBrothersEntities FarmerBrothersEntitites, CustomerModel customerModel)
        {
            TechHierarchyView techView = null;
            Contact customer = null;
            customerModel.ManagerName = WebConfigurationManager.AppSettings["ManagerName"];
            customerModel.ManagerPhone = WebConfigurationManager.AppSettings["ManagerPhone"];
            if (!string.IsNullOrEmpty(customerModel.CustomerId))
            {
                int customerId = Convert<Int32>(customerModel.CustomerId);
                customer = FarmerBrothersEntitites.Contacts.Where(x => x.ContactID == customerId).FirstOrDefault();
                customerModel = new CustomerModel(customer, FarmerBrothersEntitites);
                int FBProviderID = customer.FBProviderID == null ? 0 : Convert<Int32>(customer.FBProviderID.ToString());
                techView = Utility.GetTechDataByResponsibleTechId(FarmerBrothersEntitites, FBProviderID);
            }

            if (techView != null)
            {
                customerModel.PreferredProvider = techView.PreferredProvider;
                customerModel.ProviderPhone = techView.ProviderPhone;                
                customerModel.DSMName = techView.DSMName;
                customerModel.DSMPhone = Utility.FormatPhoneNumber(techView.DSMPhone);
                customerModel.Branch = techView.Branch;
                customerModel.Region = techView.RegionName;
                customerModel.PricingParent = techView.PricingParent;
                customerModel.DistributorName = techView.DistributorName;
                if (!string.IsNullOrEmpty(customer.ServiceLevelCode))
                {
                    customerModel.ServiceLevel = customer.ServiceLevelCode;
                    customerModel.ServiceTier = CustomerModel.GetServiceTier(customer.ServiceLevelCode);
                }

                customerModel.Route = techView.Route;

                using (FarmerBrothersEntities entities = new FarmerBrothersEntities())
                {
                    int regionNum = Convert<Int32>(customer.RegionNumber);
                    /*var ESMDSMRSMs = entities.ESMDSMRSMs.FirstOrDefault(x => x.BranchNO == customer.Branch);
                    if (ESMDSMRSMs != null)
                    {
                        customerModel.ESMName = ESMDSMRSMs.ESMName;
                        customerModel.ESMphone = Utility.FormatPhoneNumber(ESMDSMRSMs.ESMPhone);
                        customerModel.DSMName = ESMDSMRSMs.CCMName;
                        customerModel.DSMPhone = Utility.FormatPhoneNumber(ESMDSMRSMs.CCMPhone);
                        customerModel.RSMName = ESMDSMRSMs.RSM;
                        customerModel.RSMphone = Utility.FormatPhoneNumber(ESMDSMRSMs.RSMPhone);
                    }*/
                    if (customer != null)
                    {
                        customerModel.ESMName = string.IsNullOrEmpty(customer.ESMName) ? "" : customer.ESMName;
                        customerModel.ESMphone = string.IsNullOrEmpty(customer.ESMPhone) ? "" : Utility.FormatPhoneNumber(customer.ESMPhone);
                        customerModel.DSMName = string.IsNullOrEmpty(customer.CCMName) ? "" : customer.CCMName;
                        customerModel.DSMPhone = string.IsNullOrEmpty(customer.CCMPhone) ? "" : Utility.FormatPhoneNumber(customer.CCMPhone);
                        customerModel.RSMName = string.IsNullOrEmpty(customer.RSMName) ? "" : customer.RSMName;
                        customerModel.RSMphone = string.IsNullOrEmpty(customer.RSMPhone) ? "" : Utility.FormatPhoneNumber(customer.RSMPhone);
                    }
                    var Providers = entities.TECH_HIERARCHY.FirstOrDefault(x => x.DealerId == customer.FBProviderID);
                    if (Providers != null)
                    {
                        customerModel.FBProviderID = Providers.DealerId;
                        customerModel.PreferredProvider = Providers.CompanyName;
                        string providerPhone = string.Empty;
                        if (Providers.Phone.Replace("-", "").Length == 7)
                        {
                            providerPhone = Providers.AreaCode + Providers.Phone.Replace("-", "");
                        }
                        else
                        {
                            providerPhone = Providers.Phone;
                        }
                        customerModel.ProviderPhone = Utility.FormatPhoneNumber(providerPhone); ;
                    }
                }


            }

            return customerModel;
        }


        public static int IsDateHolidayWeekend(string dateTime, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            
            string query = @"Select dbo.fct_IsDateHolidayWeekend('" + dateTime + "')";
            DbRawSqlQuery<Int32> result = FarmerBrothersEntitites.Database.SqlQuery<Int32>(query);

            return result.ElementAt(0);
        }

        public static DateTime GetCurrentTime(string zipCode, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (zipCode != null && zipCode.Length > 5)
            {
                zipCode = zipCode.Substring(0, 5);
            }

            string query = @"Select dbo.getCustDateTime('" + zipCode + "')";
            DbRawSqlQuery<DateTime> result = FarmerBrothersEntitites.Database.SqlQuery<DateTime>(query);

            return result.ElementAt(0);
        }

        public static Func<string, T> GetConverter<T>()
        {
            return (x) => Convert<T>(x);
        }

        public static T Convert<T>(string val)
        {
            Type destiny = typeof(T);

            // See if we can cast           
            try
            {
                return (T)(object)val;
            }
            catch { }

            // See if we can parse
            try
            {
                return (T)destiny.InvokeMember("Parse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, null, new object[] { val });
            }
            catch { }

            // See if we can convert
            try
            {
                Type convertType = typeof(Convert);
                return (T)convertType.InvokeMember("To" + destiny.Name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, null, new object[] { val });
            }
            catch { }

            // Give up
            return default(T);
        }



        public static string GetCustomerTimeZone(string zipCode, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            if (zipCode != null && zipCode.Length > 5)
            {
                zipCode = zipCode.Substring(0, 5);
            }

            string timeZone = "Eastern Standard Time";
            Zip zip = FarmerBrothersEntitites.Zips.Where(z => z.ZIP1 == zipCode).FirstOrDefault();
            if (zip != null)
            {
                timeZone = zip.TimeZoneName;
            }

            return GetTimeZoneShortCut(timeZone);
        }

        public static string GetTimeZoneShortCut(string zone)
        {
            string timeZone = string.Empty;
            switch (zone)
            {
                case "Mountain Standard Time":
                    timeZone = "MST";
                    break;
                case "Alaskan Standard Time":
                    timeZone = "AST";
                    break;
                case "Pacific Standard Time":
                    timeZone = "PST";
                    break;
                case "Hawaiian Standard Time":
                    timeZone = "HST";
                    break;
                case "Eastern Standard Time":
                    timeZone = "EST";
                    break;
                case "Central Standard Time":
                    timeZone = "CST";
                    break;
            }
            return timeZone;
        }
        public static string GetTimeZoneByTime(string time)
        {
            string timeZone = string.Empty;
            switch (time)
            {
                case "-5":
                    timeZone = "Eastern";
                    break;
                case "-6":
                    timeZone = "Central";
                    break;
                case "-7":
                    timeZone = "Mountain";
                    break;
                case "-8":
                    timeZone = "Pacific";
                    break;
                case "-9":
                    timeZone = "Alaskan";
                    break;
                case "-10":
                    timeZone = "Hawaiian";
                    break;
            }
            return timeZone;
        }

        public static List<string> InvoiceStatus = new List<string> { "Please Select", "Awaiting 1st Level Approval", "Awaiting 2nd Level Approval", "Contested", "Not Approved", "Paid", "Paid with Adjustments", "Submitted for Payment", "Awaiting Submission" };

        public static T ConvertNode<T>(XmlNode node) where T : class
        {
            MemoryStream stm = new MemoryStream();

            StreamWriter stw = new StreamWriter(stm);
            stw.Write(node.OuterXml);
            stw.Flush();

            stm.Position = 0;

            XmlSerializer ser = new XmlSerializer(typeof(T));
            T result = (ser.Deserialize(stm) as T);

            return result;
        }

        public static IndexCounter GetIndexCounter(string indexName, int countValue)
        {
            /*IndexCounter counter = FarmerBrothersEntities.IndexCounters.Where(i => i.IndexName == indexName).FirstOrDefault();
            if (counter == null)
            {
                counter = new IndexCounter()
                {
                    IndexName = indexName,
                    IndexValue = 10000000
                };
                FarmerBrothersEntities.IndexCounters.Add(counter);
            }

            return counter;*/

            using (FarmerBrothersEntities newEntity = new FarmerBrothersEntities())
            {
                IndexCounter counter = newEntity.IndexCounters.Where(i => i.IndexName == indexName).FirstOrDefault();
                if (counter == null)
                {
                    counter = new IndexCounter()
                    {
                        IndexName = indexName,
                        IndexValue = 10000000
                    };
                    newEntity.IndexCounters.Add(counter);
                }

                string query = @"UPDATE IndexCounter SET IndexValue = " + (counter.IndexValue + countValue) + " WHERE IndexName = '" + indexName + "'";
                newEntity.Database.ExecuteSqlCommand(query);

                return counter;
            }

        }

        private string FormatException(Exception ex)
        {
            DbException dbEx = ex as DbException;
            string message = String.Empty;
            if (dbEx == null)
            {
                message =
                    Global.LOG_ERROR_IN + Environment.NewLine +
                    Global.LOG_ERROR_MESSAGE + ex.Message + Environment.NewLine +
                    Global.LOG_STACK_TRACE + ex.StackTrace + Environment.NewLine +
                    Global.LOG_SOURCE + ex.Source + Environment.NewLine +
                    Global.LOG_TARGET_SITE + ex.TargetSite + Environment.NewLine +
                    Global.LOG_USER_UID + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                message =
                    Global.LOG_ERROR_IN + Environment.NewLine +
                    Global.LOG_ERROR_MESSAGE + dbEx.Message + Environment.NewLine +
                    Global.LOG_STACK_TRACE + dbEx.StackTrace + Environment.NewLine +
                    Global.LOG_SOURCE + dbEx.Source + Environment.NewLine +
                    Global.LOG_TARGET_SITE + dbEx.TargetSite + Environment.NewLine +
                    Global.LOG_USER_UID + Environment.NewLine + Environment.NewLine;
            }
            return message;
        }
        public static void LogError(Exception ex)
        {
            if (ex is DbEntityValidationException)
            {
                DbEntityValidationException realDBException = ex as DbEntityValidationException;
                string errorDetails = string.Empty;
                foreach (var eve in realDBException.EntityValidationErrors)
                {
                    errorDetails += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    errorDetails += Environment.NewLine;
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDetails += string.Format("- Property: \"{0}\", Error: \"{1}\"",ve.PropertyName, ve.ErrorMessage);
                        errorDetails += Environment.NewLine;
                    }
                }

                using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                {
                    FBActivityLog log = new FBActivityLog();
                    log.LogDate = DateTime.UtcNow;
                    log.UserId = (int)System.Web.HttpContext.Current.Session["UserId"];
                    log.ErrorDetails = errorDetails;
                    entity.FBActivityLogs.Add(log);
                    entity.SaveChanges();
                }
            }
            else
            {
                Exception realerror = ex;
                while (realerror.InnerException != null)
                    realerror = realerror.InnerException;


                Utility u = new Utility();
                string Message = u.FormatException(realerror);
                if (!Message.Contains("arterySignalR/ping"))
                {
                    using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                    {
                        FBActivityLog log = new FBActivityLog();
                        log.LogDate = DateTime.UtcNow;
                        log.UserId = (int)System.Web.HttpContext.Current.Session["UserId"];
                        log.ErrorDetails = Message;
                        entity.FBActivityLogs.Add(log);
                        entity.SaveChanges();
                    }
                }
            }

        }
        public static string GetStringWithNewLine(string note)
        {
            string result = string.Empty;
            string[] notes = note.Replace("\\n", "@").Replace("\\t", " ").Replace("\\r", " ").Replace("\n", "@").Replace("\t", " ").Replace("\r", " ").Split('@');
            foreach (string item in notes)
            {
                result += item + Environment.NewLine;
            }

            return result;
        }

        public static string GetElapsedTime(FarmerBrothersEntities FarmerBrothersEntities, int workorderId)
        {
            string elapsedTime = string.Empty;

            try
            {
                string query = @"select dbo.getWOElapsedTime(" + workorderId.ToString() + ")";
                DbRawSqlQuery<string> result = FarmerBrothersEntities.Database.SqlQuery<string>(query);

                elapsedTime = result.ElementAt(0);
            }
            catch (Exception e)
            {

            }

            return elapsedTime;
        }


        public static string ElapsedTimeValue(FarmerBrothersEntities FarmerBrothersEntities, int workorderId)
        {
            string timeDiff = "";

            WorkOrder wo = FarmerBrothersEntities.WorkOrders.Where(w => w.WorkorderID == workorderId).FirstOrDefault();
            string zipCode = wo.CustomerZipCode;
            if (zipCode.Length > 5)
            {
                zipCode = zipCode.Substring(0, 5);
            }
            Zip timeZoneObj = FarmerBrothersEntities.Zips.Where(z => z.ZIP1 == zipCode).FirstOrDefault();//.Select(zi => zi.TimeZone).ToString();
            string timeZone = timeZoneObj.TimeZone.ToString();

            Nullable<System.DateTime> customerDateTime = new DateTime();
            DateTime timeNow = DateTime.Now;
            int noOfHours = 0;
            switch(timeZone)
            {
                case "-5":
                    noOfHours = 0;
                    break;
                case "-6":
                    noOfHours = -1;
                    break;
                case "-7":
                    noOfHours = -2;
                    break;
                case "-8":
                    noOfHours = -3;
                    break;
                case "-9":
                    noOfHours = -4;
                    break;
                case "-10":
                    noOfHours = -5;
                    break;
                default:
                    noOfHours = 0;
                    break;
            }
            customerDateTime = timeNow.AddHours(noOfHours);


          double diff = (customerDateTime - wo.WorkorderModifiedDate).Value.TotalSeconds;
            //int dateDiff = (wo.WorkorderModifiedDate - customerDateTime).Value.Seconds;
            timeDiff = (( (diff) / 60 / 60 / 24).ToString() + " : " + (diff / 60 / 60 % 24).ToString() + " : " + (diff / 60 % 60).ToString()).ToString();

            return timeDiff;

        }

        public static bool isValidEmail(string inputEmail)
        {
            Regex re = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$",
                          RegexOptions.IgnoreCase);
            return re.IsMatch(inputEmail);
        }

        public static List<WorkorderType> GetCallTypeList(FarmerBrothersEntities FarmerBrothersEntitites)
        {
            Dictionary<string, string> UserPrivilege = (Dictionary<string, string>)System.Web.HttpContext.Current.Session["UserPrivilege" + (int)System.Web.HttpContext.Current.Session["UserId"]] == null
                                ? Security.GetUserPrivilegeByUserId((int)System.Web.HttpContext.Current.Session["UserId"], null) :
                                (Dictionary<string, string>)System.Web.HttpContext.Current.Session["UserPrivilege" + (int)System.Web.HttpContext.Current.Session["UserId"]];


            List<WorkorderType> callTypeList = new List<WorkorderType>();
            List<WorkorderType> FullCallTypesList =  FarmerBrothersEntitites.WorkorderTypes.Where(wt => wt.Active == 1).OrderBy(wt => wt.Sequence).ToList();

            if (UserPrivilege["CallTypeAccess"] != "Full") {
                foreach (WorkorderType callType in FullCallTypesList)
                {
                    if(callType.CallTypeID != 1130 && callType.CallTypeID != 1230)
                    {
                        callTypeList.Add(callType);
                    }
                }
            }
            else
            {
                callTypeList.AddRange(FullCallTypesList);
            }

            return callTypeList;
        }

        #region DecryptUrl
        public static string DecryptUrl(string RequestURL)
        {
            string protocol = string.Empty;
            string actualUrl = string.Empty;
            StringBuilder decryptUrl = new StringBuilder();
            if (RequestURL.Contains("encrypt"))
            {
                if (RequestURL.ToString().Contains("https://"))
                {
                    protocol = RequestURL.ToString().Substring(0, 8);
                    actualUrl = RequestURL.ToString().Substring(8, RequestURL.ToString().Length - 8);
                }
                else
                {
                    protocol = RequestURL.ToString().Substring(0, 7);
                    actualUrl = RequestURL.ToString().Substring(7, RequestURL.Length - 7);
                }
                //decryptUrl.Append(protocol);
                //decryptUrl.Append(actualUrl);
                //decryptUrl.Append(ConfigurationManager.AppSettings["RedirectResponseUrl"]);
                string[] urlsplit = actualUrl.Split('?');

                if (urlsplit.Length > 1)
                {
                    StringBuilder strBldr = new StringBuilder();
                    for (int i = 1; i < urlsplit.Length; i++)
                    {
                        if (i < urlsplit.Length - 1)
                            strBldr.AppendFormat("{0}/", urlsplit[i]);
                        else
                            strBldr.AppendFormat("{0}", urlsplit[i]);
                    }

                    string decryptedUrl = new Encrypt_Decrypt().Decrypt(strBldr.ToString().Replace("&encrypt=yes", string.Empty));

                    //newUrl = string.Format("{0}{1}/Default.aspx?{2}", protocol, urlsplit[0], decryptedUrl);
                    //salesEmailBody.Append("<a href=\"" + Redircturl + "?workOrderId=" + workOrder.WorkorderID + "&techId=" + techId.Value + "&response=5&isResponsible=" + isResponsible + "\">REDIRECT</a>");
                    foreach (string temp1 in decryptedUrl.Split('&'))
                    {
                        string[] splitTemp1 = temp1.Split('=');
                        switch (splitTemp1[0])
                        {
                            case "workOrderId":                                
                                decryptUrl.Append("workOrderId=");
                                decryptUrl.Append(splitTemp1[1]);
                                break;
                            case "techId":
                                decryptUrl.Append("&techId=");
                                decryptUrl.Append(splitTemp1[1]);
                                break;
                            case "response":
                                decryptUrl.Append("&response=");
                                decryptUrl.Append(splitTemp1[1]);
                                break;
                            case "isResponsible":
                                decryptUrl.Append("&isResponsible=");
                                decryptUrl.Append(splitTemp1[1]);
                                break;
                            case "isBillable":
                                decryptUrl.Append("&isBillable=");
                                decryptUrl.Append(splitTemp1[1]);
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
            return decryptUrl.ToString();
        }

        public static string GetClientSystemDetails()
        {
            //string computer_name = System.Net.Dns.GetHostEntry(Request.ServerVariables["remote_addr"]).HostName;
            System.Net.IPAddress[] strClientIPAddress = System.Net.Dns.GetHostAddresses(Environment.MachineName);
            string strClientMachineName = Environment.MachineName.ToString().Trim();
            string strClientUserName = Environment.UserName.ToString().Trim();
            string strClientDomainName = Environment.UserDomainName.ToString().Trim();
            string strClientOSVersion = Environment.OSVersion.ToString().Trim();

            StringBuilder sytemInfo = new StringBuilder();
            //sytemInfo.Append("Computer Name: ");
            //sytemInfo.Append(computer_name);
            //sytemInfo.Append(Environment.NewLine);
            sytemInfo.Append("Client MachineName: ");
            sytemInfo.Append(strClientMachineName);
            sytemInfo.Append(Environment.NewLine);
            sytemInfo.Append("Client UserName: ");
            sytemInfo.Append(strClientUserName);
            sytemInfo.Append(Environment.NewLine);
            sytemInfo.Append("Client DomainName: ");
            sytemInfo.Append(strClientDomainName);
            sytemInfo.Append(Environment.NewLine);
            sytemInfo.Append("Client ClientOSVersion: ");
            sytemInfo.Append(strClientOSVersion);
            sytemInfo.Append(Environment.NewLine);

            return sytemInfo.ToString();
        }
        #endregion

        public static PricingDetail GetPricingDetails(int? CustomerID, int? TechId, string CustomerState, FarmerBrothersEntities FarmerBrothersEntitites)
        {
            List<BillingItem> blngItmsList = FarmerBrothersEntitites.BillingItems.Where(b => b.IsActive == true).ToList();
            Contact contact = FarmerBrothersEntitites.Contacts.Where(c => c.ContactID == CustomerID).FirstOrDefault();

            PricingDetail priceDtls = null;
            if (!string.IsNullOrEmpty(contact.PricingParentID) && contact.PricingParentID != "0")
            {
                priceDtls = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingTypeId == 501 && p.PricingEntityId == contact.PricingParentID).FirstOrDefault();
            }

            if (priceDtls == null || priceDtls.HourlyTravlRate == 0)
            {   
                if (TechId != 0)
                {
                    TECH_HIERARCHY tech = FarmerBrothersEntitites.TECH_HIERARCHY.Where(t => t.DealerId == TechId).FirstOrDefault();
                    if (tech.FamilyAff == "SPT")
                    {
                        string tId = TechId.ToString();
                        priceDtls = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingTypeId == 502 && p.PricingEntityId == tId).FirstOrDefault();
                    }
                }
            }

            if (priceDtls == null || priceDtls.HourlyTravlRate == 0)
            {
                priceDtls = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingTypeId == 503 && p.PricingEntityId == CustomerState).FirstOrDefault();
            }

            /*else if (TechId != 0)
            {
                string tId = TechId.ToString();
                priceDtls = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingTypeId == 502 && p.PricingEntityId == tId).FirstOrDefault();
            }
            else
            {
                priceDtls = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingTypeId == 503 && p.PricingEntityId == CustomerState).FirstOrDefault();
            }*/


            if (priceDtls == null || priceDtls.HourlyTravlRate == 0)
            {
                BillingItem TravelItem = blngItmsList.Where(a => a.BillingName.ToLower() == "travel time").FirstOrDefault();
                BillingItem laborItem = blngItmsList.Where(a => a.BillingName.ToLower() == "labor").FirstOrDefault();

                priceDtls = new PricingDetail();
                priceDtls.HourlyLablrRate = TravelItem.UnitPrice;
                priceDtls.HourlyTravlRate = laborItem.UnitPrice;
            }

            return priceDtls;
        }

        public static dynamic GetCustomreNotes(int ClientId, int ParentId, FarmerBrothersEntities FarmerBrothersEntities)
        {
            var CustomerNotes = FarmerBrothersEntities.FBCustomerNotes.Where(c => c.CustomerId == ClientId && c.IsActive == true).ToList();

            if (ParentId != 0)
            {
                CustomerNotes = FarmerBrothersEntities.FBCustomerNotes.Where(c => c.ParentId == ParentId && c.IsActive == true).ToList();
            }

            return CustomerNotes;
        }

        public static bool IsBillableCriteria(int Clientid, int Workorderid, FarmerBrothersEntities FarmerBrothersEntities)
        {
            bool isBilllable = false;

            List<CustomCriteria> criteriaList = FarmerBrothersEntities.CustomCriterias.Where(c => c.CategoryType.ToLower() == "billing").ToList();
            List<string> categoryKeys = criteriaList.Select(s => s.CategoryName).ToList();

            Contact cntct = FarmerBrothersEntities.Contacts.Where(c => c.ContactID == Clientid).FirstOrDefault();
            WorkOrder wo = FarmerBrothersEntities.WorkOrders.Where(w => w.WorkorderID == Workorderid).FirstOrDefault();

            foreach(string ctgry in categoryKeys)
            {
                CustomCriteria category =  criteriaList.Where(s => s.CategoryName == ctgry).FirstOrDefault();
                if (category != null)
                {
                    if (cntct != null)
                    {
                        string fieldValue = cntct.GetType().GetProperty(ctgry) != null ? (cntct.GetType().GetProperty(ctgry).GetValue(cntct, null) != null ? cntct.GetType().GetProperty(ctgry).GetValue(cntct, null).ToString() : "") : "";
                        if(!string.IsNullOrEmpty(fieldValue) && fieldValue.ToLower() == category.CategoryValue.ToLower())
                        {
                            isBilllable = true;
                        }
                    }
                }

            }

            return isBilllable;
        }
    }
}