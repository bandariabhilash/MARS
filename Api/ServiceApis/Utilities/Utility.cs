using DataAccess.Db;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServiceApis.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace ServiceApis.Utilities
{
    public class Utility
    {
        public static IndexCounterModel GetIndexCounter(string indexName, int countValue)
        {
            using (FBContext newEntity = new FBContext())
            {
                IndexCounter counter = newEntity.IndexCounters.Where(i => i.IndexName == indexName).FirstOrDefault();
                IndexCounterModel counterData = new IndexCounterModel();
                if (counter == null)
                {
                    counterData.IndexName = indexName;
                    counterData.IndexValue = 10000000;

                    counter.IndexName = indexName;
                    counter.IndexValue = 10000000;
                    newEntity.IndexCounters.Add(counter);
                }
                else
                {
                    counterData.IndexName = indexName;
                    counterData.IndexValue = counter.IndexValue;

                    counter.IndexName = indexName;
                    counter.IndexValue = counter.IndexValue + countValue;
                    newEntity.IndexCounters.Update(counter);
                }

                newEntity.SaveChanges();

                return counterData;
            }

        }

        public static ZonePriority GetCustomerZonePriority(FBContext context, string zipCode)
        {
            ZonePriority zonePriority = null;
            ZoneZip zonezip = context.ZoneZips.FirstOrDefault(z => z.ZipCode == zipCode.Substring(0, 5));
            if (zonezip != null)
            {
                zonePriority = context.ZonePriorities.FirstOrDefault(zp => zp.ZoneIndex == zonezip.ZoneIndex);
            }
            return zonePriority;
        }
        public static List<Fbcbe> GetFbcbeList(int? accountNumber, FBContext context)
        {
            context.Database.SetCommandTimeout(60); 

            return context.Fbcbes
                .Where(cbe => cbe.CurrentCustomerId == accountNumber)
                .ToList();
        }
        public static DateTime GetCurrentTime(string zipCode, FBContext _context)
        {
            //using (FBContext newEntity = new FBContext())
            {
                if (zipCode != null && zipCode.Length > 5)
                {
                    zipCode = zipCode.Substring(0, 5);
                }

                FormattableString query = $"SELECT dbo.getCustDateTime('{zipCode}')";
                //string query = @"Select dbo.getCustDateTime('" + zipCode + "')";
                var result = _context.Database.SqlQuery<DateTime>(query);

                return result.ElementAt(0);
            }
        }


        //public static DateTime GetCurrentTime(string zipCode)
        //{
        //    using (FBContext newEntity = new FBContext())
        //    {
        //        // Ensure zipCode is valid
        //        if (zipCode != null && zipCode.Length > 5)
        //        {
        //            zipCode = zipCode.Substring(0, 5);
        //        }

        //        // Use parameterized queries to avoid the error and improve security
        //        string query = "SELECT dbo.getCustDateTime('" + zipCode + "')";

        //        // Create a SQL parameter
        //        var zipCodeParam = new Microsoft.Data.SqlClient.SqlParameter("@zipCode", zipCode);

        //        // Execute the query
        //        var result = newEntity.Database.SqlQueryRaw<DateTime>(query, zipCodeParam);

        //        // Return the first result
        //        return result.FirstOrDefault(); // Use FirstOrDefault to avoid exceptions if the result is empty
        //    }
        //}

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
                        phoneNumber = string.Format("{0:(###)###-#### }", double.Parse(phoneNumber));
                    }
                    else if (xposition > 0)
                    {
                        string newPhoneNumber = phoneNumber.Substring(0, xposition);

                        newPhoneNumber = Regex.Replace(newPhoneNumber, @"-+", "");
                        newPhoneNumber = Regex.Replace(newPhoneNumber, @"\s+", "");
                        newPhoneNumber = string.Format("{0:(###)###-#### }", double.Parse(newPhoneNumber));

                        phoneNumber = newPhoneNumber + phoneNumber.Substring(xposition);

                    }
                }
            }
            catch (Exception e)
            {

            }
            return phoneNumber;
        }

        public static string GetCustomerTimeZone(string zipCode, FBContext FarmerBrothersEntitites)
        {
            if (zipCode != null && zipCode.Length > 5)
            {
                zipCode = zipCode.Substring(0, 5);
            }

            string timeZone = "Eastern Standard Time";
            Zip zip = FarmerBrothersEntitites.Zips.Where(z => z.Zip1 == zipCode).FirstOrDefault();
            if (zip != null)
            {
                timeZone = zip.TimeZoneName;
            }

            return GetTimeZoneShortCut(timeZone);
        }
        public static IList<State> GetStates(FBContext FarmerBrothersEntitites)
        {
            IList<State> states = FarmerBrothersEntitites.States.ToList();

            State blankState = new State() { StateCode = "n/a", StateName = "Please Select" };
            states.Insert(0, blankState);

            return states;
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
    }
}
