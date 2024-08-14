using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;
using GoogleMaps.LocationServices;
using System.Net;
using System.Data;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FBVixxoService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new Service1()
            //};
            //ServiceBase.Run(ServicesToRun);


            /*string CustomerAddress = "Scooters Coffee, Mason, IA, 50401";
            string TechAddress = "Maplewood, MN, 55109";

            Program.GetCoordinates(CustomerAddress);
            Program.GetCoordinates(TechAddress);*/

            string BaseUrl = ConfigurationSettings.AppSettings["BaseURL"];
            string ApiKey = ConfigurationSettings.AppSettings["ApiKey"];
            

            string accessToken = Program.GetAccessToken(BaseUrl, ApiKey);
            PostETA(BaseUrl, ApiKey, accessToken);
            PostTimeInOut(BaseUrl, ApiKey, accessToken);
        }


        public static void GetCoordinates(string address)
        {
            dynamic jObject = null;
            string url = "https://maps.google.com/maps/api/geocode/json?";

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(url);
            stringBuilder.Append("address=");
            stringBuilder.Append(address);
            stringBuilder.Append("&sensor=false");
            stringBuilder.Append("&key=AIzaSyCjMfuakjLPeYGF2CLY56lqz40IH9UfxLM");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(stringBuilder.ToString()).Result;

                if (response.IsSuccessStatusCode)
                {
                    jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    if (jObject != null)
                    {                        
                        double latitude = jObject.results[0].geometry.location.lat.Value;
                        double longitude = jObject.results[0].geometry.location.lng.Value;
                    }
                }
            }




            //dynamic jObject = null;
            ////string url = "http://maps.google.com/maps/api/geocode/json?address=" + address + "&sensor=false";
            //string url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + address;


            //using (var client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri(url);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //    HttpResponseMessage response = client.GetAsync(url).Result;

            //    if (response.IsSuccessStatusCode)
            //    {
            //        jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            //        //if (jObject != null)
            //        //{
            //        //    var element = jObject.rows[0].elements[0];
            //        //    duration = element.duration.value / 3600.00;
            //        //    duration = Math.Round(duration, 2);
            //        //}
            //    }
            //}








            //WebRequest request = WebRequest.Create(url);

            //using (WebResponse response = (HttpWebResponse)request.GetResponse())
            //{
            //    DataTable dtCoordinates = new DataTable();

            //    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            //    {
            //        DataSet dsResult = new DataSet();
            //        dsResult.ReadXml(reader);
            //        dtCoordinates.Columns.AddRange(new DataColumn[4] { new DataColumn("Id", typeof(int)),
            //        new DataColumn("Address", typeof(string)),
            //        new DataColumn("Latitude",typeof(string)),
            //        new DataColumn("Longitude",typeof(string)) });
            //        foreach (DataRow row in dsResult.Tables["result"].Rows)
            //        {
            //            string geometry_id = dsResult.Tables["geometry"].Select("result_id = " + row["result_id"].ToString())[0]["geometry_id"].ToString();
            //            DataRow location = dsResult.Tables["location"].Select("geometry_id = " + geometry_id)[0];
            //            dtCoordinates.Rows.Add(row["result_id"], row["formatted_address"], location["lat"], location["lng"]);
            //        }
            //    }
            //   // return dtCoordinates;
            //}
        }
        public static string GetAccessToken(string BaseUrl, string APIKey)
        {
            string AccessToken = "";

            string UserName = ConfigurationSettings.AppSettings["UserName"];
            string Password = ConfigurationSettings.AppSettings["Password"];
            string TokenEndPoint = ConfigurationSettings.AppSettings["TokenEndPoint"];

            string TokenURL = BaseUrl + TokenEndPoint;

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, TokenURL);
            request.Headers.Add("x-api-key", APIKey);
            var content = new
            {
                grant_type = "password",
                username = UserName,
                password = Password
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content), null, "application/json");
            var response = client.SendAsync(request).Result;

            if (response.IsSuccessStatusCode)
            {
                string res = response.Content.ReadAsStringAsync().Result;
                var resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(res);

                AccessToken = resultData.access_token;
            }


            return AccessToken;
        }

        public static void PostTimeInOut(string BaseUrl, string APIKey, string AccessToken)
        {
            using (VixxoEntities vixxoEntity = new VixxoEntities())
            {
                var vixxoEventsList = (from w in vixxoEntity.WorkOrders
                                       join c in vixxoEntity.Contacts on w.CustomerID equals c.ContactID
                                       join ws in vixxoEntity.WorkorderSchedules on w.WorkorderID equals ws.WorkorderID
                                       join t in vixxoEntity.TECH_HIERARCHY on ws.Techid equals t.DealerId
                                       join wd in vixxoEntity.WorkorderDetails on w.WorkorderID equals wd.WorkorderID
                                       join z in vixxoEntity.Zips on c.PostalCode equals z.ZIP1
                                       where c.PricingParentID == "9001228" && ws.AssignedStatus == "accepted" 
                                       && w.ETAUpdated != -1 && (w.ETAUpdated == 1 || w.ETAUpdated == 2)
                                       select new
                                       {
                                           w.WorkorderID,
                                           w.ETAUpdated,
                                           w.CustomerPO,
                                           c.ContactID,
                                           t.CompanyName,
                                           z.Latitude,
                                           z.Longitude,
                                           t.DealerId,
                                           wd.StartDateTime,
                                           wd.ArrivalDateTime,
                                           wd.CompletionDateTime
                                       }).ToList();


                string ETAEndPoint = ConfigurationSettings.AppSettings["ETAEndPoint"];
                string OrderETAURL = BaseUrl + ETAEndPoint;

                foreach (var evt in vixxoEventsList)
                {
                    HttpClient client = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, OrderETAURL);
                    request.Headers.Add("Authorization", "Bearer " + AccessToken);
                    request.Headers.Add("x-api-key", APIKey);

                    Contact contct = vixxoEntity.Contacts.Where(c => c.ContactID == evt.ContactID).FirstOrDefault();
                    string CustomerAddress = "";

                    if (contct != null)
                    {
                        CustomerAddress = "";
                        if (!string.IsNullOrEmpty(contct.Address1))
                        {
                            CustomerAddress += contct.Address1;
                        }
                        if (!string.IsNullOrEmpty(contct.Address2))
                        {
                            CustomerAddress += contct.Address2;
                        }
                        if (!string.IsNullOrEmpty(contct.City))
                        {
                            CustomerAddress += contct.City;
                        }
                        if (!string.IsNullOrEmpty(contct.State))
                        {
                            CustomerAddress += contct.State;
                        }
                        if (!string.IsNullOrEmpty(contct.PostalCode))
                        {
                            CustomerAddress += contct.PostalCode;
                        }
                    }

                    TECH_HIERARCHY tech = vixxoEntity.TECH_HIERARCHY.Where(th => th.DealerId == evt.DealerId).FirstOrDefault();
                    string TechAddress = "";
                    if (tech != null)
                    {
                        TechAddress = "";
                        if (!string.IsNullOrEmpty(tech.Address1))
                        {
                            TechAddress += tech.Address1;
                        }
                        if (!string.IsNullOrEmpty(tech.Address2))
                        {
                            TechAddress += tech.Address2;
                        }
                        if (!string.IsNullOrEmpty(tech.City))
                        {
                            TechAddress += tech.City;
                        }
                        if (!string.IsNullOrEmpty(tech.State))
                        {
                            TechAddress += tech.State;
                        }
                        if (!string.IsNullOrEmpty(tech.PostalCode))
                        {
                            TechAddress += tech.PostalCode;
                        }
                    }

                    int type = 0;
                    string SRN = evt.CustomerPO;
                    double? latitude = evt.Latitude;
                    double? longitude = evt.Longitude;
                    string etaDatetime = "";
                    bool isjobComplete = false;
                    string address = "";

                    if (evt.ETAUpdated == null || evt.ETAUpdated == 0)
                    {
                        type = 0;
                        etaDatetime = evt.StartDateTime.ToString();
                        address = TechAddress;
                    }
                    if (evt.ETAUpdated != null && evt.ETAUpdated == 1)
                    {
                        type = 1;
                        etaDatetime = evt.ArrivalDateTime.ToString();
                        address = TechAddress;
                    }
                    if (evt.ETAUpdated != null && evt.ETAUpdated == 2)
                    {
                        type = 2;
                        etaDatetime = evt.CompletionDateTime.ToString();
                        address = CustomerAddress;
                        isjobComplete = true;
                    }

                    var locationService = new GoogleLocationService();
                    var point = locationService.GetLatLongFromAddress(address);
                    //var latitude = point.Latitude;
                    //var longitude = point.Longitude;


                    //var content = new StringContent("\"timeRequest\":{\r\n    \"description\": \"Time Request\",\r\n    \"type\": 2,\r\n    \"ServiceRequestNumber\": ,\r\n    \"isJobComplete\": false,\r\n    \"numberOfTechniciansOnSite\": 1,\r\n    \"time\":\r\n}", null, "application/json");
                    //var content = new
                    //{
                    var content = new
                    {
                        type = 2,
                        ServiceRequestNumber = SRN,
                        isJobComplete = isjobComplete,
                        Latitude = latitude,
                        Longitude = longitude,
                        numberOfTechniciansOnSite = 1,
                        time = etaDatetime
                    };
                    //};

                    request.Content = new StringContent(JsonConvert.SerializeObject(content), null, "application/json");
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        var resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(res);

                        if (evt.ETAUpdated == null || evt.ETAUpdated == 0)
                        {
                            type = 1;
                        }
                        if (evt.ETAUpdated != null && evt.ETAUpdated == 1)
                        {
                            type = 2;
                        }
                        if (evt.ETAUpdated != null && evt.ETAUpdated == 2)
                        {
                            type = 3;
                        }
                        vixxoEntity.SaveChanges();
                    }
                }
            }
        }
        public static void PostETA(string BaseUrl, string APIKey, string AccessToken)
        {
            using (VixxoEntities vixxoEntity = new VixxoEntities())
            {
                var vixxoEventsList = (from w in vixxoEntity.WorkOrders
                                       join c in vixxoEntity.Contacts on w.CustomerID equals c.ContactID
                                       join ws in vixxoEntity.WorkorderSchedules on w.WorkorderID equals ws.WorkorderID
                                       join t in vixxoEntity.TECH_HIERARCHY on ws.Techid equals t.DealerId
                                       join wd in vixxoEntity.WorkorderDetails on w.WorkorderID equals wd.WorkorderID
                                       join z in vixxoEntity.Zips on c.PostalCode equals z.ZIP1
                                       where c.PricingParentID == "9001228" && ws.AssignedStatus == "accepted" 
                                       && w.ETAUpdated == null ||  w.ETAUpdated == 0
                                       select new
                                       {
                                           w.WorkorderID,
                                           w.ETAUpdated,
                                           w.CustomerPO,
                                           c.ContactID,
                                           t.CompanyName,
                                           z.Latitude,
                                           z.Longitude,
                                           t.DealerId,
                                           wd.StartDateTime,
                                           wd.ArrivalDateTime,
                                           wd.CompletionDateTime
                                       }).ToList();


                string ETAEndPoint = ConfigurationSettings.AppSettings["ETAEndPoint"];
                string OrderETAURL = BaseUrl + ETAEndPoint;

                foreach (var evt in vixxoEventsList)
                {
                    HttpClient client = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, OrderETAURL);
                    request.Headers.Add("Authorization", "Bearer " + AccessToken);
                    request.Headers.Add("x-api-key", APIKey);
                  
                    var content = new
                    {
                        type = "estimatedArrival",
                        ServiceRequestNumber = evt.CustomerPO,
                        time = evt.StartDateTime.ToString()
                    };

                    request.Content = new StringContent(JsonConvert.SerializeObject(content), null, "application/json");
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        var resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(res);

                        WorkOrder wo = vixxoEntity.WorkOrders.Where(w => w.WorkorderID == evt.WorkorderID).FirstOrDefault();
                        wo.ETAUpdated = 1;

                        vixxoEntity.SaveChanges();
                    }
                }
            }
        }

       /* public static void PostETA(string BaseUrl, string APIKey, string AccessToken)
        {
            using (VixxoEntities vixxoEntity = new VixxoEntities())
            {
                var vixxoEventsList = (from w in vixxoEntity.WorkOrders
                                       join c in vixxoEntity.Contacts on w.CustomerID equals c.ContactID
                                       join ws in vixxoEntity.WorkorderSchedules on w.WorkorderID equals ws.WorkorderID
                                       join t in vixxoEntity.TECH_HIERARCHY on ws.Techid equals t.DealerId
                                       join wd in vixxoEntity.WorkorderDetails on w.WorkorderID equals wd.WorkorderID
                                       join z in vixxoEntity.Zips on c.PostalCode equals z.ZIP1
                                       where c.PricingParentID == "9001228" && ws.AssignedStatus == "accepted" && w.ETAUpdated != -1 && w.ETAUpdated >= 0 && w.ETAUpdated < 3
                                       select new
                                       {
                                           w.WorkorderID,
                                           w.ETAUpdated,
                                           w.CustomerPO,
                                           c.ContactID,
                                           t.CompanyName,
                                           z.Latitude,
                                           z.Longitude,
                                           t.DealerId,
                                           wd.StartDateTime,
                                           wd.ArrivalDateTime,
                                           wd.CompletionDateTime
                                       }).ToList();


                string ETAEndPoint = ConfigurationSettings.AppSettings["ETAEndPoint"];
                string OrderETAURL = BaseUrl + ETAEndPoint;

                foreach (var evt in vixxoEventsList)
                {
                    HttpClient client = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, OrderETAURL);
                    request.Headers.Add("Authorization", "Bearer " + AccessToken);
                    request.Headers.Add("x-api-key", APIKey);

                    Contact contct = vixxoEntity.Contacts.Where(c => c.ContactID == evt.ContactID).FirstOrDefault();
                    string CustomerAddress = "";

                    if (contct != null)
                    {
                        CustomerAddress = "";
                        if (!string.IsNullOrEmpty(contct.Address1))
                        {
                            CustomerAddress += contct.Address1;
                        }
                        if (!string.IsNullOrEmpty(contct.Address2))
                        {
                            CustomerAddress += contct.Address2;
                        }
                        if (!string.IsNullOrEmpty(contct.City))
                        {
                            CustomerAddress += contct.City;
                        }
                        if (!string.IsNullOrEmpty(contct.State))
                        {
                            CustomerAddress += contct.State;
                        }
                        if (!string.IsNullOrEmpty(contct.PostalCode))
                        {
                            CustomerAddress += contct.PostalCode;
                        }
                    }

                    TECH_HIERARCHY tech = vixxoEntity.TECH_HIERARCHY.Where(th => th.DealerId == evt.DealerId).FirstOrDefault();
                    string TechAddress = "";
                    if (tech != null)
                    {
                        TechAddress = "";
                        if (!string.IsNullOrEmpty(tech.Address1))
                        {
                            TechAddress += tech.Address1;
                        }
                        if (!string.IsNullOrEmpty(tech.Address2))
                        {
                            TechAddress += tech.Address2;
                        }
                        if (!string.IsNullOrEmpty(tech.City))
                        {
                            TechAddress += tech.City;
                        }
                        if (!string.IsNullOrEmpty(tech.State))
                        {
                            TechAddress += tech.State;
                        }
                        if (!string.IsNullOrEmpty(tech.PostalCode))
                        {
                            TechAddress += tech.PostalCode;
                        }
                    }

                    int type = 0;
                    string SRN = evt.CustomerPO;
                    //double? latitude = evt.Latitude;
                    //double? longitude = evt.Longitude;
                    string etaDatetime = "";
                    bool isjobComplete = false;
                    string address = "";

                    if (evt.ETAUpdated == null || evt.ETAUpdated == 0)
                    {
                        type = 0;
                        etaDatetime = evt.StartDateTime.ToString();
                        address = TechAddress;
                    }
                    if (evt.ETAUpdated != null && evt.ETAUpdated == 1)
                    {
                        type = 1;
                        etaDatetime = evt.ArrivalDateTime.ToString();
                        address = TechAddress;
                    }
                    if (evt.ETAUpdated != null && evt.ETAUpdated == 2)
                    {
                        type = 2;
                        etaDatetime = evt.CompletionDateTime.ToString();
                        address = CustomerAddress;
                        isjobComplete = true;
                    }

                    var locationService = new GoogleLocationService();
                    var point = locationService.GetLatLongFromAddress(address);
                    var latitude = point.Latitude;
                    var longitude = point.Longitude;


                    //var content = new StringContent("\"timeRequest\":{\r\n    \"description\": \"Time Request\",\r\n    \"type\": 2,\r\n    \"ServiceRequestNumber\": ,\r\n    \"isJobComplete\": false,\r\n    \"numberOfTechniciansOnSite\": 1,\r\n    \"time\":\r\n}", null, "application/json");
                    //var content = new
                    //{
                    var content = new
                    {
                        type = 2,
                        ServiceRequestNumber = SRN,
                        isJobComplete = isjobComplete,
                        Latitude = latitude,
                        Longitude = longitude,
                        numberOfTechniciansOnSite = 1,
                        time = etaDatetime
                    };
                    //};

                    request.Content = new StringContent(JsonConvert.SerializeObject(content), null, "application/json");
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        var resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(res);

                        if (evt.ETAUpdated == null || evt.ETAUpdated == 0)
                        {
                            type = 1;
                        }
                        if (evt.ETAUpdated != null && evt.ETAUpdated == 1)
                        {
                            type = 2;
                        }
                        if (evt.ETAUpdated != null && evt.ETAUpdated == 2)
                        {
                            type = 3;
                        }
                        vixxoEntity.SaveChanges();
                    }
                }
            }
        }*/

    }
}
