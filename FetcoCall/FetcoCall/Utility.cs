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

namespace FetcoCall
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

        

        public static IList<UserType> GetUserType(FetcoEntities FetcoEntity)
        {
            IList<UserType> userType = FetcoEntity.UserTypes.ToList();

            UserType blankType = new UserType() { TypeId = 0, TypeName = "Please Select" };
            userType.Insert(0, blankType);

            return userType;
        }
        
        public static IList<State> GetStates(FetcoEntities ReviveEntitites)
        {
            IList<State> states = ReviveEntitites.States.ToList();

            State blankState = new State() { StateCode = "n/a", StateName = "Please Select" };
            states.Insert(0, blankState);

            return states;
        }

        

        public static ZonePriority GetCustomerZonePriority(FetcoEntities FetcoEntity, string zipCode)
        {
            ZonePriority zonePriority = null;
            ZoneZip zonezip = FetcoEntity.ZoneZips.FirstOrDefault(z => z.ZipCode == zipCode.Substring(0, 5));
            if (zonezip != null)
            {
                zonePriority = FetcoEntity.ZonePriorities.FirstOrDefault(zp => zp.ZoneIndex == zonezip.ZoneIndex);
            }
            return zonePriority;
        }

        public static string GetCustomerTimeZone(string zipCode, FetcoEntities FetcoEntity)
        {
            if (zipCode.Length > 5)
            {
                zipCode = zipCode.Substring(0, 5);
            }

            string timeZone = "Eastern Standard Time";
            Zip zip = FetcoEntity.Zips.Where(z => z.ZIP1 == zipCode).FirstOrDefault();
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

            using (FetcoEntities newEntity = new FetcoEntities())
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

        

        public static bool isValidEmail(string inputEmail)
        {
            Regex re = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$",
                          RegexOptions.IgnoreCase);
            return re.IsMatch(inputEmail);
        }

        public static DateTime GetCurrentTime(string zipCode, FetcoEntities ReviveEntity)
        {
            if (zipCode != null && zipCode.Length > 5)
            {
                zipCode = zipCode.Substring(0, 5);
            }

            string query = @"Select dbo.getCustDateTime('" + zipCode + "')";
            DbRawSqlQuery<DateTime> result = ReviveEntity.Database.SqlQuery<DateTime>(query);

            return result.ElementAt(0);
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

        public static PricingDetail GetPricingDetails(string PricingParentID, int? TechId, string CustomerState, FetcoEntities FarmerBrothersEntitites)
        {
            List<BillingItem> blngItmsList = FarmerBrothersEntitites.BillingItems.Where(b => b.IsActive == true).ToList();
           
            PricingDetail priceDtls = null;
            if (!string.IsNullOrEmpty(PricingParentID) && PricingParentID != "0")
            {
                priceDtls = FarmerBrothersEntitites.PricingDetails.Where(p => p.PricingTypeId == 501 && p.PricingEntityId == PricingParentID).FirstOrDefault();
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

            if (priceDtls == null || priceDtls.HourlyTravlRate == 0)
            {
                BillingItem TravelItem = blngItmsList.Where(a => a.BillingName.ToLower() == "travel time").FirstOrDefault();
                BillingItem laborItem = blngItmsList.Where(a => a.BillingName.ToLower() == "labor").FirstOrDefault();

                priceDtls = new PricingDetail();
                priceDtls.HourlyLablrRate = laborItem.UnitPrice;
                priceDtls.HourlyTravlRate = TravelItem.UnitPrice;
            }

            return priceDtls;
        }
    }

    public enum ErrorCode
    {
        SUCCESS = 0,
        ERROR = 1,
    }
}