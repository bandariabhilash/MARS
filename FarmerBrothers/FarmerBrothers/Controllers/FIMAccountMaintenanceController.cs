﻿using FarmerBrothers.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FarmerBrothers.Data;
//using FarmerBrothers.FeastLocationService;
using FarmerBrothers.Utilities;
using LinqKit;
using System;
//using FormalBrothers.WSCustomerKnowEquipmentService;
using Syncfusion.Pdf;
using Syncfusion.HtmlConverter;
using Syncfusion.JavaScript.Models;
using Syncfusion.EJ.Export;
using Syncfusion.XlsIO;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Web;
using System.Data;
using System.Reflection;
using System.Net.Mime;
//using FormalBrothers.MovementSearchService;
namespace FarmerBrothers.Controllers
{

    public class FIMAccountMaintenanceController : BaseController
    {
        MarsViews objMarsView = new MarsViews();
        // GET: FIMAccountMaintenance
        [HttpGet]
        public ActionResult SearchFIM()
        {
            FIMAccountSearchModel fimaccountSearchModel;
            fimaccountSearchModel = new FIMAccountSearchModel();
            fimaccountSearchModel.VendorID = string.Empty;
            fimaccountSearchModel.VendorName = string.Empty;
            fimaccountSearchModel.JMSLogin = string.Empty;
            fimaccountSearchModel.SearchResults = new List<FIMAccountSearchModelResult>();
            return View(fimaccountSearchModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SearchFIM")]
        public ActionResult SearchFIM(FIMAccountSearchModel fIMAccountSearchModel)
        {
            FIMAccountSearchModel fimaccountSearchModel = new FIMAccountSearchModel();
            string StrSql = string.Empty;
            Int32 intValue;
            StrSql = " Select TECH_ID,Tech_Desc from feast_tech_hierarchy where ";
            StrSql += Int32.TryParse(fIMAccountSearchModel.VendorID, out intValue) ? " TECH_ID=" + intValue : "";
            StrSql += fIMAccountSearchModel.VendorName != null ? fIMAccountSearchModel.VendorID != null ? "And TECH_Name = '" + (fIMAccountSearchModel.VendorName) + " ' " : "TECH_Name = '" + (fIMAccountSearchModel.VendorName) + " ' " : "";
            DataTable dt = objMarsView.fn_FSM_View(StrSql);
            if (dt.Rows.Count > 0)
            {
                if (objMarsView.fn_FSM_View(StrSql).Rows[0]["Tech_Desc"].ToString().ToLower() != "TPSP Branch".ToLower() && objMarsView.fn_FSM_View(StrSql).Rows[0]["Tech_Desc"].ToString().ToLower() != "TPSP Vendor".ToLower())
                {
                    StrSql = " Select u.UserActive,u.InvoicingAccount,u.TechnicianAccount,a.tech_id  as [Tech ID] ,DEFAULT_SERVICE_CENTER as [Location ID],TECH_NAME [LocationName],a.Tech_City,a.Tech_State  from feast_tech_hierarchy   A  left join ";
                    StrSql += " (Select Techid, UserActive, CAST(MAX(CAST(InvoicingAccount as INT)) AS BIT) as InvoicingAccount, CAST(MAX(CAST(TechnicianAccount as INT)) AS BIT) ";
                    StrSql += " as TechnicianAccount from UserProfile group by Techid, UserActive) ";
                    StrSql += " u on u.Techid=a.Tech_Id where ";
                    StrSql += " A.DEFAULT_SERVICE_CENTER=" + Convert.ToInt32(dt.Rows[0]["TECH_ID"]) + " and";
                    StrSql += "  a.tech_id!=0 ";
                }

                //Select servicecenter_id   [BranchId], servicecenter_name [BranchName] from feast_tech_hierarchy where TeamLead_Id =6896653
                //  Group By servicecenter_id,servicecenter_name
                else
                {
                    StrSql = " Select u.UserActive,u.InvoicingAccount,u.TechnicianAccount,a.tech_id  as [Tech ID] ,TeamLead_Id as [Location ID],TECH_NAME [LocationName],a.Tech_City,a.Tech_State  from feast_tech_hierarchy   A  left join ";
                    StrSql += " (Select Techid, UserActive, CAST(MAX(CAST(InvoicingAccount as INT)) AS BIT) as InvoicingAccount, CAST(MAX(CAST(TechnicianAccount as INT)) AS BIT) ";
                    StrSql += " as TechnicianAccount from UserProfile group by Techid, UserActive) ";
                    StrSql += " u on u.Techid=a.Tech_Id where ";
                    StrSql += " A.TeamLead_Id=" + Convert.ToInt32(dt.Rows[0]["TECH_ID"]) + " and";
                    StrSql += "  a.tech_id!=0 ";
                }


                List<FIMAccountSearchModelResult> SearchList = new List<FIMAccountSearchModelResult>();
                foreach (DataRow dr in objMarsView.fn_FSM_View(StrSql).Rows)
                {
                    FIMAccountSearchModelResult objFIMModelResult = new FIMAccountSearchModelResult();
                    objFIMModelResult.TechId = dr["Tech ID"].ToString();
                    objFIMModelResult.Active = dr["UserActive"].ToString() == "" ? "" : dr["UserActive"].ToString() == "1" ? "Active" : "InActive";
                    objFIMModelResult.InvoicingAccount = dr["InvoicingAccount"].ToString() == "" ? "" : dr["InvoicingAccount"].ToString().ToLower() == "true" ? "X" : "";
                    objFIMModelResult.TechnicianAccount = dr["TechnicianAccount"].ToString() == "" ? "" : dr["TechnicianAccount"].ToString().ToLower() == "true" ? "X" : "";
                    objFIMModelResult.LocationID = dr["Location ID"].ToString();
                    objFIMModelResult.LocationName = dr["LocationName"].ToString();
                    objFIMModelResult.City = dr["Tech_City"].ToString();
                    objFIMModelResult.State = dr["Tech_State"].ToString();
                    int techid = Convert.ToInt32(dr["Tech ID"]);
                    //objFIMModelResult.City =    (from p in FormalBrothersEntitites.ZonePriorities  join q in FormalBrothersEntitites.ZonePriorities select p.ZoneIndex).ToList().FirstOrDefault().ToString();
                    //objFIMModelResult.ParentVendorID = dr["ParentLocationName"].ToString(); 
                    // objFIMModelResult.ParentVendorName = dr["tech_name"].ToString();
                    //objFIMModelResult.ParentVendorID = dr["ParentLocationName"].ToString(); ;
                    // objFIMModelResult.ParentVendorName = dr["ParentLocationId"].ToString();
                    SearchList.Add(objFIMModelResult);
                    fimaccountSearchModel.SearchResults = SearchList;
                }
            }


            return View(fimaccountSearchModel);
        }

        [HttpGet]
        public ActionResult EditFIM(string TechId)
        {
            string StrSql = string.Empty;

            //StrSql = " Select u.UserActive,u.InvoicingAccount,u.UsrPassword,u.TechnicianAccount,a.tech_id  as [Location ID] ,a.tech_name as [TechName],a.TECH_EMAIL as  [Email],a.Tech_Phone,a.Tech_City,a.Tech_State,a.Default_Service_Center as [ParentLocationId],servicecenter_name [LocationName], a.Tech_Type,a.Tech_Desc  from feast_tech_hierarchy   ";
            //StrSql += "   A  LEFT  join UserProfile u  on u.Techid=A.Tech_Id where Tech_Id=" + Convert.ToInt32(TechId);
            StrSql = " Select A.Tech_Id as TechID,u.UserActive,u.InvoicingAccount,u.UsrPassword,u.TechnicianAccount,a.Tech_Id as [Location ID] ,a.tech_name as [TechName],a.TECH_EMAIL as  [Email],a.Tech_Phone,a.Tech_City,a.Tech_State,a.TeamLead_ID as [ParentLocationId],TeamLead_Name as [LocationName], a.Tech_Type,a.Tech_Desc,u.IsInvoice  from feast_tech_hierarchy ";
            StrSql += "   A  LEFT  join UserProfile u  on u.Techid=A.Tech_Id where Tech_Id=" + Convert.ToInt32(TechId);
            FIMModel fIMModel = new FIMModel();
            foreach (DataRow dr in objMarsView.fn_FSM_View(StrSql).Rows)
            {
                fIMModel.IsActive = dr["UserActive"].ToString() == "1" ? "true" : dr["UserActive"].ToString() == "0" ? "false" : "";
                fIMModel.VendorBranchID = dr["Location ID"].ToString();
                fIMModel.VendorBranchName = dr["TechName"].ToString();
                fIMModel.ParentVendorID = dr["ParentLocationId"].ToString();
                fIMModel.ParentVendorName = dr["LocationName"].ToString();
                fIMModel.VendorBranchEmail = dr["Email"].ToString();
                // fIMModel.
                fIMModel.TechId = dr["TechID"].ToString();
                fIMModel.TechType = dr["Tech_Type"].ToString();
                fIMModel.TechDesc = dr["Tech_Desc"].ToString();
                if (dr["IsInvoice"] != null && dr["IsInvoice"].ToString() == "False")
                {
                    fIMModel.TechnicianAccount = dr["TechnicianAccount"].ToString() == "True" ? true : false;
                }
                if (dr["IsInvoice"] != null && dr["IsInvoice"].ToString() == "True")
                {
                    fIMModel.InvoicingAccount = dr["InvoicingAccount"].ToString() == "True" ? true : false;
                }
                fIMModel.VendorBranchPhone = Utilities.Utility.FormatPhoneNumber(dr["Tech_Phone"].ToString());
                fIMModel.VendorBranchCity = dr["Tech_City"].ToString();
                fIMModel.VendorBranchState = dr["Tech_State"].ToString();
                fIMModel.Userpassword = dr["UsrPassword"].ToString();
                //var zipcode = (dr["ServiceCenter_zip"]).ToString();
                //fIMModel.VendorBranchCity = (from p in FormalBrothersEntitites.Zips where p.ZIP1 == zipcode select p.City).FirstOrDefault();
                //   fIMModel.VendorBranchEmail = dr["Techid"].ToString();
                //fIMModel.CopyOnDispatchEmail = dr["DispatchEmails"].ToString();
                //   fIMModel. = dr["Techid"].ToString();             
            }
            //  fIMModel.IsActive=
            return View(fIMModel);
        }

        [HttpPost]
        public ActionResult EditFIM(FIMModel fIMModel, string submit)
        {
            var techid = Convert.ToInt32(fIMModel.TechId);
            var UsrProfiles = (from x in FarmerBrothersEntitites.UserProfiles
                               where x.Techid == techid
                               select x).ToList();
            switch (submit)
            {

                case "Deactivate":
                    {

                        foreach (UserProfile UsrProfile in UsrProfiles)
                        {
                            if (UsrProfile != null)
                            {
                                UsrProfile.UserActive = 0;
                                FarmerBrothersEntitites.SaveChanges();
                            }
                        }
                        //return RedirectToAction("SearchFIM");
                        TempData["Sucess"] = "Account Deactivated Successfully.";
                        string StrSql = string.Empty;

                        //StrSql = " Select u.UserActive,u.InvoicingAccount,u.UsrPassword,u.TechnicianAccount,a.tech_id  as [Location ID] ,a.tech_name as [TechName],a.TECH_EMAIL as  [Email],a.Tech_Phone,a.Tech_City,a.Tech_State,a.Default_Service_Center as [ParentLocationId],servicecenter_name [LocationName], a.Tech_Type,a.Tech_Desc  from feast_tech_hierarchy   ";
                        //StrSql += "   A  LEFT  join UserProfile u  on u.Techid=A.Tech_Id where Tech_Id=" + Convert.ToInt32(techid);
                        StrSql = " Select A.Tech_Id as TechID,u.UserActive,u.InvoicingAccount,u.UsrPassword,u.TechnicianAccount,a.Tech_Id as [Location ID] ,a.tech_name as [TechName],a.TECH_EMAIL as  [Email],a.Tech_Phone,a.Tech_City,a.Tech_State,a.TeamLead_ID as [ParentLocationId],TeamLead_Name as [LocationName], a.Tech_Type,a.Tech_Desc,u.IsInvoice  from feast_tech_hierarchy ";
                        StrSql += "   A  LEFT  join UserProfile u  on u.Techid=A.Tech_Id where Tech_Id=" + Convert.ToInt32(techid);
                        FIMModel DfIMModel = new FIMModel();
                        foreach (DataRow dr in objMarsView.fn_FSM_View(StrSql).Rows)
                        {
                            DfIMModel.IsActive = dr["UserActive"].ToString() == "1" ? "true" : dr["UserActive"].ToString() == "0" ? "false" : "";
                            DfIMModel.VendorBranchID = dr["Location ID"].ToString();
                            DfIMModel.VendorBranchName = dr["TechName"].ToString();
                            DfIMModel.ParentVendorID = dr["ParentLocationId"].ToString();
                            DfIMModel.ParentVendorName = dr["LocationName"].ToString();
                            DfIMModel.VendorBranchEmail = dr["Email"].ToString();
                            // fIMModel.
                            DfIMModel.TechId = dr["TechID"].ToString();
                            DfIMModel.TechType = dr["Tech_Type"].ToString();
                            DfIMModel.TechDesc = dr["Tech_Desc"].ToString();
                            if (dr["IsInvoice"] != null && dr["IsInvoice"].ToString() == "False")
                            {
                                DfIMModel.TechnicianAccount = dr["TechnicianAccount"].ToString() == "True" ? true : false;
                            }
                            if (dr["IsInvoice"] != null && dr["IsInvoice"].ToString() == "True")
                            {
                                DfIMModel.InvoicingAccount = dr["InvoicingAccount"].ToString() == "True" ? true : false;
                            }
                            DfIMModel.VendorBranchPhone = dr["Tech_Phone"].ToString();
                            DfIMModel.VendorBranchCity = dr["Tech_City"].ToString();
                            DfIMModel.VendorBranchState = dr["Tech_State"].ToString();
                            DfIMModel.Userpassword = dr["UsrPassword"].ToString();
                            //var zipcode = (dr["ServiceCenter_zip"]).ToString();
                            //fIMModel.VendorBranchCity = (from p in FormalBrothersEntitites.Zips where p.ZIP1 == zipcode select p.City).FirstOrDefault();
                            //   fIMModel.VendorBranchEmail = dr["Techid"].ToString();
                            //fIMModel.CopyOnDispatchEmail = dr["DispatchEmails"].ToString();
                            //   fIMModel. = dr["Techid"].ToString();             
                        }
                        return View(DfIMModel);
                        //return RedirectToAction("EditFIM", "FIMAccountMaintenance", new { TechId = fIMModel.TechId });
                        break;
                    }
                case "Activate":
                    {
                        foreach (UserProfile UsrProfile in UsrProfiles)
                        {
                            if (UsrProfile != null)
                            {
                                UsrProfile.UserActive = 1;
                                FarmerBrothersEntitites.SaveChanges();
                            }
                        }
                        //return RedirectToAction("SearchFIM");
                        TempData["Sucess"] = "Account Activated Successfully.";
                        string StrSql = string.Empty;
                        StrSql = " Select A.Tech_Id as TechID,u.UserActive,u.InvoicingAccount,u.UsrPassword,u.TechnicianAccount,a.Tech_Id as [Location ID] ,a.tech_name as [TechName],a.TECH_EMAIL as  [Email],a.Tech_Phone,a.Tech_City,a.Tech_State,a.TeamLead_ID as [ParentLocationId],TeamLead_Name as [LocationName], a.Tech_Type,a.Tech_Desc,u.IsInvoice  from feast_tech_hierarchy ";
                        StrSql += "   A  LEFT  join UserProfile u  on u.Techid=A.Tech_Id where Tech_Id=" + Convert.ToInt32(techid);
                        FIMModel DfIMModel = new FIMModel();
                        foreach (DataRow dr in objMarsView.fn_FSM_View(StrSql).Rows)
                        {
                            DfIMModel.IsActive = dr["UserActive"].ToString() == "1" ? "true" : dr["UserActive"].ToString() == "0" ? "false" : "";
                            DfIMModel.VendorBranchID = dr["Location ID"].ToString();
                            DfIMModel.VendorBranchName = dr["TechName"].ToString();
                            DfIMModel.ParentVendorID = dr["ParentLocationId"].ToString();
                            DfIMModel.ParentVendorName = dr["LocationName"].ToString();
                            DfIMModel.VendorBranchEmail = dr["Email"].ToString();
                            // fIMModel.
                            DfIMModel.TechId = dr["TechID"].ToString();
                            DfIMModel.TechType = dr["Tech_Type"].ToString();
                            DfIMModel.TechDesc = dr["Tech_Desc"].ToString();
                            if (dr["IsInvoice"] != null && dr["IsInvoice"].ToString() == "False")
                            {
                                DfIMModel.TechnicianAccount = dr["TechnicianAccount"].ToString() == "True" ? true : false;
                            }
                            if (dr["IsInvoice"] != null && dr["IsInvoice"].ToString() == "True")
                            {
                                DfIMModel.InvoicingAccount = dr["InvoicingAccount"].ToString() == "True" ? true : false;
                            }
                            DfIMModel.VendorBranchPhone = dr["Tech_Phone"].ToString();
                            DfIMModel.VendorBranchCity = dr["Tech_City"].ToString();
                            DfIMModel.VendorBranchState = dr["Tech_State"].ToString();
                            DfIMModel.Userpassword = dr["UsrPassword"].ToString();
                            //var zipcode = (dr["ServiceCenter_zip"]).ToString();
                            //fIMModel.VendorBranchCity = (from p in FormalBrothersEntitites.Zips where p.ZIP1 == zipcode select p.City).FirstOrDefault();
                            //   fIMModel.VendorBranchEmail = dr["Techid"].ToString();
                            //fIMModel.CopyOnDispatchEmail = dr["DispatchEmails"].ToString();
                            //   fIMModel. = dr["Techid"].ToString();             
                        }
                        return View(DfIMModel);
                        //return RedirectToAction("EditFIM", "FIMAccountMaintenance", new { TechId = fIMModel.TechId });
                        break;
                    }
                case "Save":
                    {

                        if (UsrProfiles.Count > 0)
                        {
                            var UsrProfileforTech = (from x in FarmerBrothersEntitites.UserProfiles
                                                     where x.Techid == techid //&& x.IsInvoice == false
                                                     select x).ToList().FirstOrDefault();
                            if (UsrProfileforTech == null && fIMModel.TechnicianAccount == true)
                            {
                                UserProfile objusrProfile = new UserProfile();
                                //objusrProfile.InvoicingAccount = fIMModel.InvoicingAccount;
                                objusrProfile.TechnicianAccount = fIMModel.TechnicianAccount;
                                objusrProfile.Techid = Convert.ToInt32(fIMModel.TechId);
                                //objusrProfile.TechType = fIMModel.TechType == "Agent" ? "INT" : "SPT";
                                objusrProfile.TechType = ((fIMModel.TechType.ToLower() == "Agent".ToLower() && fIMModel.TechDesc.ToLower() == "TPSP Vendor".ToLower()) || (fIMModel.TechType.ToLower() == "Stock Location".ToLower() && fIMModel.TechDesc.ToLower() == "TPSP Branch".ToLower())) ? "SPT" : "INT";
                                objusrProfile.Branchid = Convert.ToInt32(fIMModel.VendorBranchID);
                                if (fIMModel.VendorBranchEmail != "" && fIMModel.VendorBranchEmail != null)
                                {
                                    objusrProfile.UserName = fIMModel.VendorBranchEmail;
                                    objusrProfile.UsrPassword = CreatePassword(8);
                                }
                                objusrProfile.CreatedDate = DateTime.Now;
                                objusrProfile.UserActive = 1;
                                //objusrProfile.IsInvoice = false;
                                FarmerBrothersEntitites.UserProfiles.Add(objusrProfile);
                                FarmerBrothersEntitites.SaveChanges();
                            }
                            else
                            {
                                if (UsrProfileforTech != null)
                                {
                                    UsrProfileforTech.TechnicianAccount = fIMModel.TechnicianAccount;
                                    if (UsrProfileforTech.UserName == null || UsrProfileforTech.UserName == "")
                                    {
                                        if (fIMModel.VendorBranchEmail != "" && fIMModel.VendorBranchEmail != null)
                                        {
                                            UsrProfileforTech.UserName = fIMModel.VendorBranchEmail;
                                            if (UsrProfileforTech.UsrPassword == null || UsrProfileforTech.UsrPassword == "")
                                            {
                                                UsrProfileforTech.UsrPassword = CreatePassword(8);
                                            }
                                        }
                                    }
                                    if ((UsrProfileforTech.UsrPassword == null || UsrProfileforTech.UsrPassword == "") && (UsrProfileforTech.UserName != "" && UsrProfileforTech.UserName != null))
                                    {
                                        UsrProfileforTech.UsrPassword = CreatePassword(8);
                                    }
                                    FarmerBrothersEntitites.SaveChanges();
                                }
                            }


                            var UsrProfileforInvoice = (from x in FarmerBrothersEntitites.UserProfiles
                                                        where x.Techid == techid //&& x.IsInvoice == true
                                                        select x).ToList().FirstOrDefault();
                            if (UsrProfileforInvoice == null && fIMModel.InvoicingAccount == true)
                            {
                                UserProfile objusrProfile = new UserProfile();
                                objusrProfile.InvoicingAccount = fIMModel.InvoicingAccount;
                                //objusrProfile.TechnicianAccount = fIMModel.TechnicianAccount;
                                objusrProfile.Techid = Convert.ToInt32(fIMModel.TechId);
                                //objusrProfile.TechType = fIMModel.TechType == "Agent" ? "INT" : "SPT";
                                objusrProfile.TechType = ((fIMModel.TechType.ToLower() == "Agent".ToLower() && fIMModel.TechDesc.ToLower() == "TPSP Vendor".ToLower()) || (fIMModel.TechType.ToLower() == "Stock Location".ToLower() && fIMModel.TechDesc.ToLower() == "TPSP Branch".ToLower())) ? "SPT" : "INT";
                                objusrProfile.Branchid = Convert.ToInt32(fIMModel.VendorBranchID);
                                if (fIMModel.VendorBranchEmail != "" && fIMModel.VendorBranchEmail != null)
                                {
                                    objusrProfile.UserName = fIMModel.VendorBranchEmail;
                                    objusrProfile.UsrPassword = CreatePassword(8);
                                }
                                objusrProfile.CreatedDate = DateTime.Now;
                                objusrProfile.UserActive = 1;
                                //objusrProfile.IsInvoice = true;
                                FarmerBrothersEntitites.UserProfiles.Add(objusrProfile);
                                FarmerBrothersEntitites.SaveChanges();
                            }
                            else
                            {
                                if (UsrProfileforInvoice != null)
                                {
                                    UsrProfileforInvoice.InvoicingAccount = fIMModel.InvoicingAccount;
                                    if (UsrProfileforInvoice.UserName == null || UsrProfileforInvoice.UserName == "")
                                    {
                                        if (fIMModel.VendorBranchEmail != "" && fIMModel.VendorBranchEmail != null)
                                        {
                                            UsrProfileforInvoice.UserName = fIMModel.VendorBranchEmail;
                                            if (UsrProfileforInvoice.UsrPassword == null || UsrProfileforInvoice.UsrPassword == "")
                                            {
                                                UsrProfileforInvoice.UsrPassword = CreatePassword(8);
                                            }
                                        }
                                    }
                                    if ((UsrProfileforInvoice.UsrPassword == null || UsrProfileforInvoice.UsrPassword == "") && (UsrProfileforInvoice.UserName != "" && UsrProfileforInvoice.UserName != null))
                                    {
                                        UsrProfileforInvoice.UsrPassword = CreatePassword(8);
                                    }
                                    FarmerBrothersEntitites.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            if (fIMModel.TechnicianAccount == true)
                            {
                                UserProfile objusrProfile = new UserProfile();
                                //objusrProfile.InvoicingAccount = fIMModel.InvoicingAccount;
                                objusrProfile.TechnicianAccount = fIMModel.TechnicianAccount;
                                objusrProfile.Techid = Convert.ToInt32(fIMModel.TechId);
                                //objusrProfile.TechType = fIMModel.TechType == "Agent" ? "INT" : "SPT";
                                objusrProfile.TechType = ((fIMModel.TechType.ToLower() == "Agent".ToLower() && fIMModel.TechDesc.ToLower() == "TPSP Vendor".ToLower()) || (fIMModel.TechType.ToLower() == "Stock Location".ToLower() && fIMModel.TechDesc.ToLower() == "TPSP Branch".ToLower())) ? "SPT" : "INT";
                                objusrProfile.Branchid = Convert.ToInt32(fIMModel.VendorBranchID);
                                if (fIMModel.VendorBranchEmail != "" && fIMModel.VendorBranchEmail != null)
                                {
                                    objusrProfile.UserName = fIMModel.VendorBranchEmail;
                                    objusrProfile.UsrPassword = CreatePassword(8);
                                }
                                objusrProfile.CreatedDate = DateTime.Now;
                                objusrProfile.UserActive = 1;
                                //objusrProfile.IsInvoice = false;
                                FarmerBrothersEntitites.UserProfiles.Add(objusrProfile);
                                FarmerBrothersEntitites.SaveChanges();
                            }
                            if (fIMModel.InvoicingAccount == true)
                            {
                                UserProfile objusrProfile = new UserProfile();
                                objusrProfile.InvoicingAccount = fIMModel.InvoicingAccount;
                                //objusrProfile.TechnicianAccount = fIMModel.TechnicianAccount;
                                objusrProfile.Techid = Convert.ToInt32(fIMModel.TechId);
                                //objusrProfile.TechType = fIMModel.TechType == "Agent" ? "INT" : "SPT";
                                objusrProfile.TechType = ((fIMModel.TechType.ToLower() == "Agent".ToLower() && fIMModel.TechDesc.ToLower() == "TPSP Vendor".ToLower()) || (fIMModel.TechType.ToLower() == "Stock Location".ToLower() && fIMModel.TechDesc.ToLower() == "TPSP Branch".ToLower())) ? "SPT" : "INT";
                                objusrProfile.Branchid = Convert.ToInt32(fIMModel.VendorBranchID);
                                if (fIMModel.VendorBranchEmail != "" && fIMModel.VendorBranchEmail != null)
                                {
                                    objusrProfile.UserName = fIMModel.VendorBranchEmail;
                                    objusrProfile.UsrPassword = CreatePassword(8);
                                }
                                objusrProfile.CreatedDate = DateTime.Now;
                                objusrProfile.UserActive = 1;
                                //objusrProfile.IsInvoice = true;
                                FarmerBrothersEntitites.UserProfiles.Add(objusrProfile);
                                FarmerBrothersEntitites.SaveChanges();
                            }
                        }
                        // return ac("SearchFIM");

                        TempData["Sucess"] = "Account Saved Sucessfully.";
                        string StrSql = string.Empty;
                        StrSql = " Select A.Tech_Id as TechID,u.UserActive,u.InvoicingAccount,u.UsrPassword,u.TechnicianAccount,a.Tech_Id as [Location ID] ,a.tech_name as [TechName],a.TECH_EMAIL as  [Email],a.Tech_Phone,a.Tech_City,a.Tech_State,a.TeamLead_ID as [ParentLocationId],TeamLead_Name as [LocationName], a.Tech_Type,a.Tech_Desc,u.IsInvoice  from feast_tech_hierarchy ";
                        StrSql += "   A  LEFT  join UserProfile u  on u.Techid=A.Tech_Id where Tech_Id=" + Convert.ToInt32(techid);
                        FIMModel DfIMModel = new FIMModel();
                        foreach (DataRow dr in objMarsView.fn_FSM_View(StrSql).Rows)
                        {
                            DfIMModel.IsActive = dr["UserActive"].ToString() == "1" ? "true" : dr["UserActive"].ToString() == "0" ? "false" : "";
                            DfIMModel.VendorBranchID = dr["Location ID"].ToString();
                            DfIMModel.VendorBranchName = dr["TechName"].ToString();
                            DfIMModel.ParentVendorID = dr["ParentLocationId"].ToString();
                            DfIMModel.ParentVendorName = dr["LocationName"].ToString();
                            DfIMModel.VendorBranchEmail = dr["Email"].ToString();
                            // fIMModel.
                            DfIMModel.TechId = dr["TechID"].ToString();
                            DfIMModel.TechType = dr["Tech_Type"].ToString();
                            DfIMModel.TechDesc = dr["Tech_Desc"].ToString();
                            if (dr["IsInvoice"] != null && dr["IsInvoice"].ToString() == "False")
                            {
                                DfIMModel.TechnicianAccount = dr["TechnicianAccount"].ToString() == "True" ? true : false;
                            }
                            if (dr["IsInvoice"] != null && dr["IsInvoice"].ToString() == "True")
                            {
                                DfIMModel.InvoicingAccount = dr["InvoicingAccount"].ToString() == "True" ? true : false;
                            }
                            DfIMModel.VendorBranchPhone = dr["Tech_Phone"].ToString();
                            DfIMModel.VendorBranchCity = dr["Tech_City"].ToString();
                            DfIMModel.VendorBranchState = dr["Tech_State"].ToString();
                            DfIMModel.Userpassword = dr["UsrPassword"].ToString();                                         
                        }
                        return View(DfIMModel);                       
                        //return RedirectToAction("EditFIM","FIMAccountMaintenance",new { TechId=fIMModel.TechId});
                        break;
                    }
                default:
                    return RedirectToAction("SearchFIM");
                    break;

            }
        }
        public JsonResult SendEmail(string TechnicianMail, bool TechnicianAccount, bool InvoiceAccount, string TechnicianName, int Techid)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.IsBodyHtml = true;
                mail.AlternateViews.Add(getEmbdImage(TechnicianAccount, InvoiceAccount, TechnicianName, Techid));
                mail.From = new MailAddress("jmsnotify@jmsmucker.com");
                mail.To.Add(new MailAddress(TechnicianMail));                
                mail.Subject = "FIM Account Details";
                using (var smtp = new SmtpClient())
                {
                    //smtp.Host = "jmsnotify.jmsmucker.com";
                    //smtp.Host = "192.168.3.240";
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = 25;
                    smtp.Send(mail);
                }

                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json("Failed");
            }

        }
        private AlternateView getEmbdImage(bool TechnicianAccount, bool InvoiceAccount, string TechnicianName, int Techid)
        {
            //   var imageData = Convert.FromBase64String("/9j/4AAQSkZJRgABAgEASABIAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB");
            var imageData = Convert.FromBase64String("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UmVhZHlxyWU8AAADZmlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS41LWMwMTQgNzkuMTUxNDgxLCAyMDEzLzAzLzEzLTEyOjA5OjE1ICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOjdBRTgzM0I1RkMzMEU1MTE5RjhEQzA0MDg4MjYxQTlBIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjgxM0FBNEE0RTJDRDExRTU5N0Y4QTBCNkFFMkNGMjY2IiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjgxM0FBNEEzRTJDRDExRTU5N0Y4QTBCNkFFMkNGMjY2IiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzYgKFdpbmRvd3MpIj4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6NzU5RTc4RkVGOERFRTUxMUJFOUE5OEFBRkUzMTQzNUEiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6N0FFODMzQjVGQzMwRTUxMTlGOERDMDQwODgyNjFBOUEiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7nbaeSAAA0MElEQVR4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");
            var contentId = Guid.NewGuid().ToString();
            LinkedResource inline = new LinkedResource(new MemoryStream(imageData), "image/jpeg");
            //LinkedResource inline = new LinkedResource(filePath);
            inline.ContentId = Guid.NewGuid().ToString();

            //var useProfileTech = (from p in FormalBrothersEntitites.UserProfiles where p.Techid == Techid && p.IsInvoice == false select p).FirstOrDefault();
            var useProfileTech = (from p in FarmerBrothersEntitites.UserProfiles where p.Techid == Techid select p).FirstOrDefault();
            string usrName = "";
            string usrPwd = "";
            if (useProfileTech != null)
            {
                usrName = useProfileTech.UserName;
                usrPwd = useProfileTech.UsrPassword;
            }
            //var useProfileInvoice = (from p in FormalBrothersEntitites.UserProfiles where p.Techid == Techid && p.IsInvoice == true select p).FirstOrDefault();
            var useProfileInvoice = (from p in FarmerBrothersEntitites.UserProfiles where p.Techid == Techid select p).FirstOrDefault();
            string usrNameInvoice = "";
            string usrPwdInvoice = "";
            if (useProfileInvoice != null)
            {
                usrNameInvoice = useProfileInvoice.UserName;
                usrPwdInvoice = useProfileInvoice.UsrPassword;
            }

            string body = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/FIMAccountEmail.html"));
            body = body.Replace("#FIMUSERNAME#", TechnicianName).Replace("#TECHUSERName#", usrName).Replace("#TECHUSERPassword#", usrPwd).Replace("#InvoiceUSERName#", usrNameInvoice).Replace("#InvoiceUSERPassword#", usrPwdInvoice);
            int index3 = body.IndexOf("<li>", body.IndexOf("<li>") + 1);
            int index4 = body.IndexOf("</li>", body.IndexOf("</li>") + 1);
            int index1 = body.Remove(body.LastIndexOf("<td>"), (body.LastIndexOf("</td>") - body.LastIndexOf("<td>"))).LastIndexOf("<td>");
            int index2 = body.Remove(body.LastIndexOf("<td>"), (body.LastIndexOf("</td>") - body.LastIndexOf("<td>"))).LastIndexOf("</td>");
            body = (TechnicianAccount && InvoiceAccount) ? body : TechnicianAccount == true ? body.Remove(body.LastIndexOf("<td>"), (body.LastIndexOf("</td>") + 5 - body.LastIndexOf("<td>"))).Remove(index3, index4 + 5 - index3) : body.Remove(index1, index2 + 5 - index1).ToString().Remove(body.IndexOf("<li>"), body.IndexOf("</li>") + 5 - body.IndexOf("<li>"));
            body = body.Insert(body.IndexOf("dy>") + 3, @"<img src='cid:" + inline.ContentId + @"'/>");
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(inline);
            return alternateView;
        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
    }
}